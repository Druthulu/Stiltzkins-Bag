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