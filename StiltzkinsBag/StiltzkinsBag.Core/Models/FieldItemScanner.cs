// StiltzkinsBag.Core/Models/FieldItemScanner.cs
//
// Scans all FF9 Steam field script files (.eb.bytes) and returns a per-item count
// of finite field instances (chests, hidden items, scripted item rewards).
//
// Two entry points:
//
//   ScanArchive(archivePath)
//     Opens p0data7.bin via UnityArchiver, iterates all US-locale field files,
//     runs FieldParser.FindItemLocations on each, and accumulates counts.
//
//   ScanFiles(IEnumerable<(string name, byte[] bytes)>)
//     Lower-level: scans a caller-supplied collection of (filename, bytes) pairs.
//     Used by tests to scan pre-committed .bytes files without a live archive.
//
// Counting rules:
//   • Only DirectItem and TreasureItem locations are counted (not TextSync / DirectGil).
//   • Only item IDs 1–255 are counted. Item ID 0 is excluded — see False positives below.
//   • Each unique (file, byte-offset) pair counts as exactly ONE item instance.
//     This correctly de-duplicates AddItem calls that appear in multiple branch arms
//     of the same if-else block, because they each occupy different byte offsets.
//   • TextSync locations (SetTextVariable display pairs) are skipped — they are
//     not additional item instances.
//   • The returned dictionary maps item ID → count of distinct field instances.
//     Items not found in any field file are absent from the dictionary.
//
// Archive path within p0data7.bin:
//   assets/resources/commonasset/eventengine/eventbinary/field/us/
//   All files in this folder have the .eb.bytes extension and are US-locale field scripts.
//   Entries for other locales (es, fr, gr, it, jp, uk) are skipped to avoid double-counting.
//
//   evt_battle_wm_XXXX.eb files are EXCLUDED — these are world-map battle event scripts
//   (IDs 9900–9903, one per disc). Their AddItem calls are battle-trigger rewards, not
//   field item pickups. They will be handled by BattleItemScanner (Phase 5.9 Task 3).
//
//   EVT_ALEX3_AC_SEAT_N.eb is EXCLUDED — this is an "unknown field" with no field ID
//   in the game's script system. It must be excluded from all scans and randomization.
//   See FieldScriptExclusions.UnknownFieldNoId.
//
// False positives:
//   FieldParser.FindItemLocations can return DirectItem locations with IDs > 255
//   (e.g., 10008, 18241) because the 0x48 opcode byte can appear inside variable
//   expressions of unrelated opcodes. These are filtered out by the range check.
//
//   Item ID 0 (Hammer) is also filtered. AddItem(0, X) is used as a null/default
//   sentinel in many field scripts, producing a false count of 85 for what is actually
//   a single scripted give (Field 1911 — Treno / Queen Stella's house, Stellazzio quest).
//   VanillaObtainabilityData.SentinelExcludedFieldItems provides the correct verified
//   count of 1, which VanillaItemCatalog merges into effective field counts at build time.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StiltzkinsBag.Core.Parsing;
using StiltzkinsBag.Parsing;

namespace StiltzkinsBag.Core.Models;

/// <summary>
/// Hardcoded exclusions for field scripts that must be skipped in all scans.
/// </summary>
public static class FieldScriptExclusions
{
    /// <summary>
    /// EVT_ALEX3_AC_SEAT_N — "unknown field" with no field ID in the game's script system.
    /// This file exists in p0data7.bin but is not reachable through normal gameplay.
    /// Excluded from all field item scans, randomization, and diagnostics.
    /// </summary>
    public const string UnknownFieldNoId = "EVT_ALEX3_AC_SEAT_N";

    /// <summary>
    /// Returns true if the given filename (short name, with or without extension)
    /// matches a hardcoded exclusion.
    /// </summary>
    public static bool IsExcluded(string name)
    {
        // Strip any extension(s) to get the base name for comparison
        string baseName = Path.GetFileNameWithoutExtension(name);
        // Handle double extension (.eb.bytes → strip .bytes → strip .eb)
        if (baseName.EndsWith(".eb", StringComparison.OrdinalIgnoreCase))
            baseName = Path.GetFileNameWithoutExtension(baseName);

        return baseName.Equals(UnknownFieldNoId, StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Scans FF9 Steam field script files to count per-item finite field instances.
/// </summary>
public static class FieldItemScanner
{
    // ── Public entry points ───────────────────────────────────────────────────

    /// <summary>
    /// Opens p0data7.bin (or any equivalent Unity archive containing field scripts)
    /// and scans all US-locale field files.
    /// </summary>
    /// <param name="archivePath">Full path to p0data7.bin.</param>
    /// <returns>
    /// Dictionary mapping item ID (0–255) to the number of distinct field instances
    /// (unique file × offset pairs). Items with zero instances are absent.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="archivePath"/> is null.</exception>
    /// <exception cref="System.IO.FileNotFoundException">Archive not found.</exception>
    public static IReadOnlyDictionary<int, int> ScanArchive(string archivePath)
    {
        ArgumentNullException.ThrowIfNull(archivePath);

        var counts = new Dictionary<int, int>();

        using var archive = UnityArchiver.Open(archivePath);

        // Archive entry names are leaf names as stored in the asset bundle (TextAsset).
        // The Unity convention for p0data7.bin is that field script TextAsset entries
        // are named "evt_XXX.eb" (WITHOUT the ".bytes" extension) or sometimes include
        // ".eb.bytes". We filter the same way FieldItemRandomizer does: by "evt_" prefix.
        //
        // All locales (es, fr, gr, it, jp, uk, us) share byte-identical bytecode — only
        // AT_TEXT string IDs differ. Since UnityArchiver returns distinct leaf names and
        // field files from different locales share the same leaf name, each unique name
        // is extracted once (the first occurrence in the archive, which is 'us').
        // This matches the FieldItemRandomizer's deduplication approach.
        // evt_battle_wm_XXXX.eb files are world-map battle event scripts (disc variants
        // 9900–9903). Their AddItem calls are battle-trigger rewards, not field pickups.
        // They share the evt_ prefix but belong to the battle scanner (Phase 5.9 Task 3).
        var distinctFieldNames = archive.GetFileNames()
            .Where(n => n.StartsWith("evt_", StringComparison.OrdinalIgnoreCase)
                     && !n.StartsWith("evt_battle_", StringComparison.OrdinalIgnoreCase)
                     && !FieldScriptExclusions.IsExcluded(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (string name in distinctFieldNames)
        {
            byte[] bytes;
            try { bytes = archive.Extract(name); }
            catch { continue; } // skip unreadable entries

            ScanSingleFile(name, bytes, counts);
        }

        return counts;
    }

    /// <summary>
    /// Scans a caller-supplied collection of (filename, bytes) pairs.
    /// Useful for tests using pre-committed .bytes files without a live archive.
    /// EVT_ALEX3_AC_SEAT_N is excluded regardless of how it appears in the input.
    /// </summary>
    /// <param name="files">
    /// Each tuple: (filename for diagnostics, .eb.bytes content).
    /// </param>
    public static IReadOnlyDictionary<int, int> ScanFiles(
        IEnumerable<(string Name, byte[] Bytes)> files)
    {
        ArgumentNullException.ThrowIfNull(files);

        var counts = new Dictionary<int, int>();

        foreach (var (name, bytes) in files)
        {
            if (bytes is null || bytes.Length == 0) continue;
            if (FieldScriptExclusions.IsExcluded(name)) continue;
            ScanSingleFile(name, bytes, counts);
        }

        return counts;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Parses one .eb.bytes file and accumulates item counts into <paramref name="counts"/>.
    /// Each unique byte offset that yields a valid item ID (0–255) adds 1 to that item's count.
    /// </summary>
    private static void ScanSingleFile(
        string filename,
        byte[] bytes,
        Dictionary<int, int> counts)
    {
        IReadOnlyList<FieldItemLocation> locations;
        try
        {
            locations = FieldParser.FindItemLocations(bytes);
        }
        catch
        {
            // Malformed file — skip cleanly. FieldParser validates structure.
            return;
        }

        foreach (FieldItemLocation loc in locations)
        {
            // Only count actual item instances — skip TextSync and DirectGil.
            if (loc.LocationKind != FieldLocationKind.DirectItem &&
                loc.LocationKind != FieldLocationKind.TreasureItem)
                continue;

            // For TreasureItem: only count plain item IDs (< 512).
            // Values 512–999 = cards, 1000–29998 = gil, 29999 = disabled.
            int itemId = loc.CurrentValue;
            if (loc.LocationKind == FieldLocationKind.TreasureItem)
            {
                if (!loc.TreasureIsItem) continue; // card, gil, or disabled
                // TreasureIsItem guarantees currentValue < 512
            }

            // Filter to valid item ID range (1–255).
            // DirectItem false-positives (opcode byte collisions) produce IDs > 255.
            // Item ID 0 is excluded — it is a null sentinel in FFIX field scripts;
            // see file header for details. VanillaItemCatalog injects the correct count.
            if (itemId < 1 || itemId > 255) continue;

            counts[itemId] = counts.TryGetValue(itemId, out int existing)
                ? existing + loc.ItemCount
                : loc.ItemCount;
        }
    }
}