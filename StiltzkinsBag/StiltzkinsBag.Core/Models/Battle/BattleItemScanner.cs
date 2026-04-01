// StiltzkinsBag.Core/Models/Battle/BattleItemScanner.cs
//
// Scans all FF9 Steam battle stat files (dbfile0000.raw16.bytes) and returns
// the set of item IDs found in drop slots, steal slots, and card drop slots.
//
// Two entry points:
//
//   ScanArchive(archivePath)
//     Opens p0data2.bin via UnityArchiver, uses GetFullPaths() to enumerate all
//     battle stat files, extracts each via ExtractByPath, parses as EnemyFile,
//     and collects all non-zero item IDs.
//
//   ScanFiles(IEnumerable<(string FolderPath, byte[] Bytes)>)
//     Lower-level: scans a caller-supplied collection of (folderPath, bytes) pairs.
//     Used by tests to scan pre-extracted .bytes files without a live archive.
//
// Results:
//   BattleScanResult.DropItemIds   — item IDs (> 0) found in any drop slot
//   BattleScanResult.StealItemIds  — item IDs (> 0) found in any steal slot
//   BattleScanResult.CardDropIds   — card IDs (> 0) found in any card drop slot
//   BattleScanResult.AllEnemyItemIds — union of DropItemIds and StealItemIds;
//                                      used to populate HasNormalEnemySource in
//                                      VanillaItemCatalog.
//
// Archive path convention (p0data2.bin):
//   Battle stat files are stored under:
//     assets/resources/battlemap/battlescene/{evt_battle_*}/dbfile0000.raw16.bytes
//   UnityArchiver.GetFullPaths() returns these full paths from the AssetBundle table.
//   ExtractByPath is used for all extractions — Extract(shortName) is NOT used here
//   because all battle stat files share the short name "dbfile0000.raw16" and would
//   all resolve to the first match.
//
// Item ID filtering:
//   EnemyFile.GetDrop/GetSteal return 0 for empty slots. Only non-zero values are
//   collected. No upper-bound filter is applied — the caller (VanillaItemCatalog)
//   determines which IDs are valid for its purposes.
//   Card IDs are collected separately since they are Tetramaster card numbers,
//   not consumable item IDs.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StiltzkinsBag.Core.Parsing;

namespace StiltzkinsBag.Core.Models.Battle;

/// <summary>
/// The result of a battle file scan — sets of item/card IDs found across all battles.
/// </summary>
public sealed class BattleScanResult
{
    /// <summary>
    /// Item IDs (> 0) found in any drop slot across all scanned battle files.
    /// Drop slots: common (256/256), uncommon (96/256), rare (32/256), super-rare (1/256).
    /// </summary>
    public IReadOnlySet<int> DropItemIds { get; }

    /// <summary>
    /// Item IDs (> 0) found in any steal slot across all scanned battle files.
    /// Steal slots: common (256/256), uncommon (64/256), rare (16/256), super-rare (1/256).
    /// </summary>
    public IReadOnlySet<int> StealItemIds { get; }

    /// <summary>
    /// Card IDs (> 0) found in the card drop slot across all scanned battle files.
    /// These are Tetramaster card numbers, not consumable item IDs.
    /// </summary>
    public IReadOnlySet<int> CardDropIds { get; }

    /// <summary>
    /// Union of DropItemIds and StealItemIds.
    /// An item present here has at least one enemy source in the game.
    /// Used to populate VanillaItemCatalog.HasNormalEnemySource.
    /// </summary>
    public IReadOnlySet<int> AllEnemyItemIds { get; }

    public BattleScanResult(
        HashSet<int> dropItemIds,
        HashSet<int> stealItemIds,
        HashSet<int> cardDropIds)
    {
        DropItemIds = dropItemIds;
        StealItemIds = stealItemIds;
        CardDropIds = cardDropIds;

        var allEnemy = new HashSet<int>(dropItemIds);
        allEnemy.UnionWith(stealItemIds);
        AllEnemyItemIds = allEnemy;
    }
}

/// <summary>
/// Scans FF9 Steam battle stat files to collect drop, steal, and card drop item IDs.
/// </summary>
public static class BattleItemScanner
{
    // Full path suffix pattern for battle stat files inside p0data2.bin.
    // All battle stat files share the short name "dbfile0000.raw16" — GetFullPaths()
    // is required to distinguish them by their full battlescene folder path.
    private const string BattleScenePath = "battlemap/battlescene/";
    private const string BattleStatFile = "dbfile0000.raw16.bytes";

    // ── Public entry points ───────────────────────────────────────────────────

    /// <summary>
    /// Opens p0data2.bin (or any equivalent Unity archive containing battle stat files)
    /// and scans all battle stat files found in the AssetBundle path table.
    /// </summary>
    /// <param name="archivePath">Full path to p0data2.bin.</param>
    /// <returns>
    /// BattleScanResult containing the sets of item IDs found across all battle files.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="archivePath"/> is null.</exception>
    /// <exception cref="FileNotFoundException">Archive not found.</exception>
    public static BattleScanResult ScanArchive(string archivePath)
    {
        ArgumentNullException.ThrowIfNull(archivePath);

        var drops = new HashSet<int>();
        var steals = new HashSet<int>();
        var cards = new HashSet<int>();

        using var archive = UnityArchiver.Open(archivePath);

        // GetFullPaths() returns paths from the AssetBundle table, e.g.:
        //   assets/resources/battlemap/battlescene/evt_battle_ac_e028f/dbfile0000.raw16.bytes
        // We filter to battle stat files only.
        var battlePaths = archive.GetFullPaths()
            .Where(p => p.Contains(BattleScenePath, StringComparison.OrdinalIgnoreCase)
                     && p.EndsWith(BattleStatFile, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (string path in battlePaths)
        {
            byte[] bytes;
            try { bytes = archive.ExtractByPath(path); }
            catch { continue; }

            ScanSingleFile(path, bytes, drops, steals, cards);
        }

        return new BattleScanResult(drops, steals, cards);
    }

    /// <summary>
    /// Scans a caller-supplied collection of (folderPath, bytes) pairs.
    /// Useful for tests using pre-extracted .bytes files without a live archive.
    /// </summary>
    /// <param name="files">
    /// Each tuple: (folderPath for diagnostics, dbfile0000.raw16.bytes content).
    /// folderPath matches the EnemyFolder catalog convention:
    ///   \StreamingAssets\assets\resources\battlemap\battlescene\evt_battle_*\dbfile0000.raw16.bytes
    /// </param>
    public static BattleScanResult ScanFiles(
        IEnumerable<(string FolderPath, byte[] Bytes)> files)
    {
        ArgumentNullException.ThrowIfNull(files);

        var drops = new HashSet<int>();
        var steals = new HashSet<int>();
        var cards = new HashSet<int>();

        foreach (var (folderPath, bytes) in files)
        {
            if (bytes is null || bytes.Length == 0) continue;
            ScanSingleFile(folderPath, bytes, drops, steals, cards);
        }

        return new BattleScanResult(drops, steals, cards);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static void ScanSingleFile(
        string sourcePath,
        byte[] bytes,
        HashSet<int> drops,
        HashSet<int> steals,
        HashSet<int> cards)
    {
        EnemyFile file;
        try { file = new EnemyFile(bytes, sourcePath); }
        catch { return; } // skip version-8 or malformed entries

        for (int s = 0; s < file.StatCount; s++)
        {
            for (int slot = 0; slot < 4; slot++)
            {
                int dropId = file.GetDrop(s, slot);
                if (dropId > 0) drops.Add(dropId);

                int stealId = file.GetSteal(s, slot);
                if (stealId > 0) steals.Add(stealId);
            }

            int cardId = file.GetCardDrop(s);
            if (cardId > 0) cards.Add(cardId);
        }
    }
}