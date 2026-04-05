// StiltzkinsBag.Core/Models/FieldItemScanner.cs
//
// Scans all FF9 Steam field script files (.eb.bytes) and returns a per-item count
// of finite field instances (chests, hidden items, scripted item rewards).
//
// Two aggregate entry points:
//
//   ScanArchive(archivePath)
//     Opens p0data7.bin via UnityArchiver, iterates all US-locale field files,
//     runs FieldParser.FindItemLocations on each, and accumulates counts.
//
//   ScanFiles(IEnumerable<(string name, byte[] bytes)>)
//     Lower-level: scans a caller-supplied collection of (filename, bytes) pairs.
//     Used by tests to scan pre-committed .bytes files without a live archive.
//
// One per-file entry point:
//
//   ScanArchiveDetailed(archivePath)
//     Same enumeration as ScanArchive but returns one FieldFileScanResult per file
//     rather than aggregated counts. Used for diagnostics, for building
//     itemIdToMinFieldId (Task 4 — EnforceSynthesisReachability wiring), and for
//     any future caller that needs to know which specific files contain each item.
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
//   field item pickups. They share the evt_ prefix but belong to the battle scanner.
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
/// Per-file scan result from <see cref="FieldItemScanner.ScanArchiveDetailed"/>.
/// Contains all raw locations found in one field script file.
/// Only files with at least one DirectItem or TreasureItem location are included.
/// </summary>
/// <param name="FileName">
/// Short archive name of the field script (e.g. "EVT_ALEX1_AT_HOUSE_1.eb").
/// </param>
/// <param name="Locations">
/// All patchable locations found in this file by
/// <see cref="FieldParser.FindItemLocations"/>. Includes DirectItem, TreasureItem,
/// TextSync, and DirectGil locations — the full raw output, unfiltered.
/// </param>
public sealed record FieldFileScanResult(
    string FileName,
    IReadOnlyList<FieldItemLocation> Locations);

/// <summary>
/// Scans FF9 Steam field script files to count per-item finite field instances.
/// </summary>
public static class FieldItemScanner
{
    // ── Public entry points ───────────────────────────────────────────────────

    /// <summary>
    /// Opens p0data7.bin and scans all US-locale field files.
    /// Returns an aggregate item ID → count map.
    /// </summary>
    /// <param name="archivePath">Full path to p0data7.bin.</param>
    /// <returns>
    /// Dictionary mapping item ID (1–255) to the number of distinct field instances.
    /// Items with zero instances are absent.
    /// </returns>
    public static IReadOnlyDictionary<int, int> ScanArchive(string archivePath)
    {
        ArgumentNullException.ThrowIfNull(archivePath);

        var counts = new Dictionary<int, int>();

        using var archive = UnityArchiver.Open(archivePath);

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
            catch { continue; }

            ScanSingleFile(name, bytes, counts);
        }

        return counts;
    }

    /// <summary>
    /// Opens p0data7.bin and scans all US-locale field files. Returns one
    /// <see cref="FieldFileScanResult"/> per file that contains at least one
    /// DirectItem or TreasureItem location.
    ///
    /// This is the per-file variant of <see cref="ScanArchive"/>. Use it when you
    /// need to know which specific files contribute each item count — for example:
    ///   • Diagnostic output to investigate false-positive inflation (e.g. Dagger-101)
    ///   • Building itemIdToMinFieldId for EnforceSynthesisReachability (Task 4)
    ///   • Any future caller that needs file-level provenance of item locations
    ///
    /// Results are sorted by FileName (OrdinalIgnoreCase) for determinism.
    /// Files that fail to parse are silently skipped — same as ScanArchive.
    /// </summary>
    /// <param name="archivePath">Full path to p0data7.bin.</param>
    public static IReadOnlyList<FieldFileScanResult> ScanArchiveDetailed(string archivePath)
    {
        ArgumentNullException.ThrowIfNull(archivePath);

        var results = new List<FieldFileScanResult>();

        using var archive = UnityArchiver.Open(archivePath);

        var distinctFieldNames = archive.GetFileNames()
            .Where(n => n.StartsWith("evt_", StringComparison.OrdinalIgnoreCase)
                     && !n.StartsWith("evt_battle_", StringComparison.OrdinalIgnoreCase)
                     && !FieldScriptExclusions.IsExcluded(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (string name in distinctFieldNames)
        {
            byte[] bytes;
            try { bytes = archive.Extract(name); }
            catch { continue; }

            IReadOnlyList<FieldItemLocation> locations;
            try { locations = FieldParser.FindItemLocations(bytes); }
            catch { continue; } // ArgumentException on malformed files — skip cleanly

            // Only include files with at least one item or treasure location.
            // Files with only TextSync / DirectGil are not item-bearing.
            bool hasItemLocations = locations.Any(l =>
                l.LocationKind == FieldLocationKind.DirectItem ||
                l.LocationKind == FieldLocationKind.TreasureItem);

            if (hasItemLocations)
                results.Add(new FieldFileScanResult(name, locations));
        }

        return results;
    }

    /// <summary>
    /// Scans a caller-supplied collection of (filename, bytes) pairs.
    /// Useful for tests using pre-committed .bytes files without a live archive.
    /// EVT_ALEX3_AC_SEAT_N is excluded regardless of how it appears in the input.
    /// </summary>
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

    private static void ScanSingleFile(
        string filename,
        byte[] bytes,
        Dictionary<int, int> counts)
    {
        IReadOnlyList<FieldItemLocation> locations;
        try { locations = FieldParser.FindItemLocations(bytes); }
        catch { return; }

        foreach (FieldItemLocation loc in locations)
        {
            if (loc.LocationKind != FieldLocationKind.DirectItem &&
                loc.LocationKind != FieldLocationKind.TreasureItem)
                continue;

            int itemId = loc.CurrentValue;
            if (loc.LocationKind == FieldLocationKind.TreasureItem)
            {
                if (!loc.TreasureIsItem) continue;
            }

            if (itemId < 1 || itemId > 255) continue;

            counts[itemId] = counts.TryGetValue(itemId, out int existing)
                ? existing + loc.ItemCount
                : loc.ItemCount;
        }
    }
}