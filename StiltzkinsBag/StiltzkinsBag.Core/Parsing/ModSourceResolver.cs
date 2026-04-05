// StiltzkinsBag.Core/Parsing/ModSourceResolver.cs
//
// Resolves the best available bytes for a given enemy file path by walking the
// active Memoria mod stack, then falling back to the vanilla catalog bytes.
//
// Resolution order for a given relative path (e.g. \StreamingAssets\...\dbfile0000.raw16.bytes):
//
//   1. Walk FolderNames from Memoria.ini in order (index 0 = highest priority).
//      For each mod folder:
//        a. Check if a raw .bytes file exists at the correct subfolder path inside the mod.
//           e.g. <GameRoot>\<ModFolder>\StreamingAssets\...\dbfile0000.raw16.bytes
//           → read and return those bytes directly.
//        b. Check if the mod folder contains a p0data2.bin archive.
//           e.g. <GameRoot>\<ModFolder>\StreamingAssets\p0data2.bin
//           → open the archive and try to extract the file by path.
//           If found, return those bytes.
//
//   2. If no mod provides the file, fall back to the vanilla game archive:
//        <GameRoot>\FINAL FANTASY IX_Data\StreamingAssets\p0data2.bin
//      Extract from there.
//
//   3. If the vanilla archive extract also fails, return the vanilla bytes
//      from the EnemyCatalogEntry (the pre-extracted JSON fallback).
//      This covers the case where the game install is not accessible.
//
// Note: Memoria.ini lives in the game root. FolderNames defines the active mod
// list in priority order (first entry = highest priority). Mods not in FolderNames
// are ignored even if they appear in Priorities — Priorities is launcher metadata only.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using StiltzkinsBag.Core.Models.Battle;

namespace StiltzkinsBag.Core.Parsing;

/// <summary>
/// Result of a source resolution, including the bytes and where they came from.
/// </summary>
public sealed class ResolvedEnemyFile
{
    public byte[] Bytes { get; }

    /// <summary>Human-readable description of which source was used.</summary>
    public string Source { get; }

    public ResolvedEnemyFile(byte[] bytes, string source)
    {
        Bytes = bytes;
        Source = source;
    }
}

/// <summary>
/// Reads Memoria.ini and resolves the best available bytes for enemy battle files
/// by walking the active mod stack before falling back to vanilla game data.
/// </summary>
public sealed class ModSourceResolver
{
    // Archive filename containing battle stat files (confirmed by HW source)
    private const string BattleArchiveName = "p0data2.bin";

    // Path inside the game's own data folder, relative to game root
    private const string VanillaArchiveRelPath =
        @"FINAL FANTASY IX_Data\StreamingAssets\" + BattleArchiveName;

    private readonly string _gameRoot;
    private readonly List<string> _activeMods;   // ordered: index 0 = highest priority

    // ── Factory ────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a resolver by reading Memoria.ini from the given game root.
    /// </summary>
    /// <param name="gameRoot">
    /// Full path to the FF9 game root directory (the folder containing Memoria.ini).
    /// e.g. Z:\Games\steamapps\common\FINAL FANTASY IX
    /// </param>
    public static ModSourceResolver FromGameRoot(string gameRoot)
    {
        ArgumentException.ThrowIfNullOrEmpty(gameRoot);

        string iniPath = Path.Combine(gameRoot, "Memoria.ini");
        if (!File.Exists(iniPath))
            throw new FileNotFoundException(
                $"Memoria.ini not found in game root '{gameRoot}'. " +
                "Verify the game root path is the FINAL FANTASY IX folder.", iniPath);

        List<string> mods = ParseFolderNames(iniPath);
        return new ModSourceResolver(gameRoot, mods);
    }

    /// <summary>
    /// Creates a resolver with an explicit ordered mod list (highest priority first).
    /// Useful for testing without a real game install.
    /// </summary>
    public static ModSourceResolver WithExplicitMods(string gameRoot, IEnumerable<string> modsHighestFirst)
    {
        return new ModSourceResolver(gameRoot, new List<string>(modsHighestFirst));
    }

    // ── Constructor ────────────────────────────────────────────────────────

    private ModSourceResolver(string gameRoot, List<string> activeMods)
    {
        _gameRoot = gameRoot;
        _activeMods = activeMods;
    }

    /// <summary>The active mod folder names in priority order (index 0 = highest).</summary>
    public IReadOnlyList<string> ActiveMods => _activeMods.AsReadOnly();

    // ── Resolve API ────────────────────────────────────────────────────────

    /// <summary>
    /// Resolves the best available bytes for the given catalog entry.
    /// Never returns null — always falls back to the vanilla catalog bytes as a last resort.
    /// </summary>
    /// <param name="entry">The catalog entry to resolve.</param>
    public ResolvedEnemyFile Resolve(EnemyCatalogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        // The relative path from catalog uses backslashes starting with \StreamingAssets\
        // Strip the leading backslash for path joining
        string relPath = entry.EnemyFolder.TrimStart('\\', '/');

        // ── 1. Walk mod stack ──────────────────────────────────────────────
        foreach (string modFolder in _activeMods)
        {
            string modRoot = Path.Combine(_gameRoot, modFolder);
            if (!Directory.Exists(modRoot))
                continue;

            // 1a. Raw .bytes file in mod folder
            string rawPath = Path.Combine(modRoot, relPath);
            if (File.Exists(rawPath))
                return new ResolvedEnemyFile(
                    File.ReadAllBytes(rawPath),
                    $"mod:{modFolder} (raw file)");

            // 1b. p0data2.bin archive inside the mod's StreamingAssets folder
            string archivePath = Path.Combine(modRoot, "StreamingAssets", BattleArchiveName);
            if (File.Exists(archivePath))
            {
                try
                {
                    using var archive = UnityArchiver.Open(archivePath);
                    byte[] bytes = archive.ExtractByPath(entry.EnemyFolder);
                    return new ResolvedEnemyFile(bytes, $"mod:{modFolder} (archive)");
                }
                catch (FileNotFoundException)
                {
                    // This mod's archive doesn't contain this particular file — keep looking
                }
                catch
                {
                    // Archive is corrupt or unreadable — skip this mod, keep looking
                }
            }
        }

        // ── 2. Vanilla game archive ────────────────────────────────────────
        string vanillaArchive = Path.Combine(_gameRoot, VanillaArchiveRelPath);
        if (File.Exists(vanillaArchive))
        {
            try
            {
                using var archive = UnityArchiver.Open(vanillaArchive);
                byte[] bytes = archive.ExtractByPath(entry.EnemyFolder);
                return new ResolvedEnemyFile(bytes, "vanilla archive");
            }
            catch
            {
                // Fall through to catalog bytes
            }
        }

        // ── 3. Catalog vanilla bytes (last resort) ─────────────────────────
        return new ResolvedEnemyFile(
            entry.GetVanillaBytes(),
            "catalog (vanilla fallback)");
    }

    /// <summary>
    /// Resolves bytes for a battle file identified by its archive-relative path
    /// (e.g. "assets/resources/battlemap/battlescene/foo/dbfile0000.raw16.bytes").
    ///
    /// Used by <c>RandomizerEngine.LoadEnemyFiles</c> to support active Memoria mods
    /// when enumerating battle files directly from the vanilla archive path list.
    /// Unlike <see cref="Resolve(EnemyCatalogEntry)"/>, this overload works from
    /// a raw path string and does not require an EnemyCatalogEntry or catalog bytes.
    ///
    /// Resolution order:
    ///   1. Walk mod stack (highest priority first):
    ///      a. Raw .bytes file at [modRoot]/StreamingAssets/{path}
    ///      b. mod's p0data2.bin archive → ExtractByPath
    ///   2. Vanilla game archive at [gameRoot]/StreamingAssets/p0data2.bin
    ///
    /// Throws <see cref="FileNotFoundException"/> only if the vanilla archive itself
    /// is missing (broken game install). Files absent from a mod's archive are
    /// silently skipped — that is expected behaviour.
    /// </summary>
    /// <param name="archiveRelPath">
    /// Path as returned by <c>UnityArchiver.GetFullPaths()</c>, e.g.
    /// "assets/resources/battlemap/battlescene/evt_battle_ac_e001/dbfile0000.raw16.bytes".
    /// </param>
    public ResolvedEnemyFile ResolveByPath(string archiveRelPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(archiveRelPath);

        // Normalise separators for consistent path joining on Windows
        string normalisedForJoin = archiveRelPath.TrimStart('/', '\\')
            .Replace('/', Path.DirectorySeparatorChar);

        // ── 1. Walk mod stack ──────────────────────────────────────────────
        foreach (string modFolder in _activeMods)
        {
            string modRoot = Path.Combine(_gameRoot, modFolder);
            if (!Directory.Exists(modRoot)) continue;

            // 1a. Raw .bytes override in mod folder
            string rawPath = Path.Combine(modRoot, "StreamingAssets", normalisedForJoin);
            if (File.Exists(rawPath))
                return new ResolvedEnemyFile(
                    File.ReadAllBytes(rawPath),
                    $"mod:{modFolder} (raw file)");

            // 1b. p0data2.bin archive inside the mod
            string modArchivePath = Path.Combine(modRoot, "StreamingAssets", BattleArchiveName);
            if (File.Exists(modArchivePath))
            {
                try
                {
                    using var archive = UnityArchiver.Open(modArchivePath);
                    byte[] bytes = archive.ExtractByPath(archiveRelPath);
                    return new ResolvedEnemyFile(bytes, $"mod:{modFolder} (archive)");
                }
                catch (FileNotFoundException)
                {
                    // File not in this mod's archive — keep walking the stack
                }
                catch
                {
                    // Archive corrupt or unreadable — skip and keep walking
                }
            }
        }

        // ── 2. Vanilla game archive ────────────────────────────────────────
        // NOTE: Correct path is StreamingAssets/p0data2.bin — NOT under FINAL FANTASY IX_Data.
        // The FINAL FANTASY IX_Data subfolder does not exist in all Steam installs.
        // This was confirmed during Phase 8 testing (deviation logged in PhaseEnd_Phase8.md).
        string vanillaArchivePath = Path.Combine(_gameRoot, "StreamingAssets", BattleArchiveName);
        if (!File.Exists(vanillaArchivePath))
            throw new FileNotFoundException(
                $"Vanilla battle archive not found at '{vanillaArchivePath}'. " +
                "Verify the game path is the FINAL FANTASY IX root directory " +
                "and that the game files are intact.",
                vanillaArchivePath);

        using var vanillaArchive = UnityArchiver.Open(vanillaArchivePath);
        byte[] vanillaBytes = vanillaArchive.ExtractByPath(archiveRelPath);
        return new ResolvedEnemyFile(vanillaBytes, "vanilla archive");
    }

    /// <summary>
    /// Resolves bytes for a file embedded inside a Unity archive (e.g. resources.assets).
    ///
    /// Used by <c>RandomizerEngine.ExtractTetraMasterData</c> and any future caller
    /// that needs to load content from a Unity archive other than p0data2.bin.
    ///
    /// Resolution order:
    ///   1. Walk mod stack (highest priority first):
    ///      a. Raw file override at [modRoot]/{assetFullPath}
    ///      b. Archive at [modRoot]/{archiveRelPath} → extract by shortName or full path
    ///   2. Vanilla archive at [gameRoot]/{archiveRelPath} → extract
    ///
    /// Throws <see cref="FileNotFoundException"/> if the vanilla archive itself is missing.
    /// </summary>
    /// <param name="archiveRelPath">
    /// Path to the Unity archive relative to the game/mod root, forward-slash separated.
    /// e.g. "x64/FF9_Data/resources.assets"
    /// </param>
    /// <param name="assetFullPath">
    /// Full path of the asset inside the archive as used by
    /// <c>UnityArchiver.ExtractByPath</c>. Also used as the raw override path
    /// within the mod folder (without leading slash).
    /// e.g. "embeddedasset/quadmist/minigame_card_data_address"
    /// </param>
    /// <param name="shortName">
    /// If provided, use <c>UnityArchiver.Extract(shortName)</c> instead of
    /// <c>ExtractByPath</c> when reading archives. Only safe when the short name
    /// is unique within the archive. Do NOT use for minista.mes (7 language copies
    /// with identical short names — use <c>ExtractByPath</c> instead).
    /// </param>
    public ResolvedEnemyFile ResolveEmbeddedAsset(
        string archiveRelPath,
        string assetFullPath,
        string? shortName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(archiveRelPath);
        ArgumentException.ThrowIfNullOrEmpty(assetFullPath);

        string normalisedAssetPath = assetFullPath.TrimStart('/', '\\')
            .Replace('/', Path.DirectorySeparatorChar);
        string normalisedArchivePath = archiveRelPath.TrimStart('/', '\\')
            .Replace('/', Path.DirectorySeparatorChar);

        // ── 1. Walk mod stack ──────────────────────────────────────────────
        foreach (string modFolder in _activeMods)
        {
            string modRoot = Path.Combine(_gameRoot, modFolder);
            if (!Directory.Exists(modRoot)) continue;

            // 1a. Raw file override at [modRoot]/{assetFullPath}
            string rawPath = Path.Combine(modRoot, normalisedAssetPath);
            if (File.Exists(rawPath))
                return new ResolvedEnemyFile(
                    File.ReadAllBytes(rawPath),
                    $"mod:{modFolder} (raw file)");

            // 1b. Archive at [modRoot]/{archiveRelPath}
            string modArchivePath = Path.Combine(modRoot, normalisedArchivePath);
            if (File.Exists(modArchivePath))
            {
                try
                {
                    using var archive = UnityArchiver.Open(modArchivePath);
                    byte[] bytes = shortName is not null
                        ? archive.Extract(shortName)
                        : archive.ExtractByPath(assetFullPath);
                    return new ResolvedEnemyFile(bytes, $"mod:{modFolder} (archive)");
                }
                catch (FileNotFoundException)
                {
                    // Asset not in this mod's archive — keep walking the stack
                }
                catch
                {
                    // Archive corrupt or unreadable — skip and keep walking
                }
            }
        }

        // ── 2. Vanilla archive ─────────────────────────────────────────────
        string vanillaArchivePath = Path.Combine(_gameRoot, normalisedArchivePath);
        if (!File.Exists(vanillaArchivePath))
            throw new FileNotFoundException(
                $"Archive not found at '{vanillaArchivePath}'. " +
                "Verify the game path is the FINAL FANTASY IX root directory " +
                "and that the game files are intact.",
                vanillaArchivePath);

        using var vanillaArchive = UnityArchiver.Open(vanillaArchivePath);
        byte[] vanillaBytes = shortName is not null
            ? vanillaArchive.Extract(shortName)
            : vanillaArchive.ExtractByPath(assetFullPath);
        return new ResolvedEnemyFile(vanillaBytes, "vanilla archive");
    }

    // ── Memoria.ini parser ─────────────────────────────────────────────────

    private static List<string> ParseFolderNames(string iniPath)
    {
        string[] lines = File.ReadAllLines(iniPath);
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (!trimmed.StartsWith("FolderNames", StringComparison.OrdinalIgnoreCase))
                continue;

            int eq = trimmed.IndexOf('=');
            if (eq < 0) continue;

            string value = trimmed[(eq + 1)..].Trim();

            // Extract quoted strings: "ModA", "ModB/Sub", ...
            var matches = Regex.Matches(value, "\"([^\"]+)\"");
            var result = new List<string>(matches.Count);
            foreach (Match m in matches)
                result.Add(m.Groups[1].Value);

            return result;
        }

        return [];
    }
}