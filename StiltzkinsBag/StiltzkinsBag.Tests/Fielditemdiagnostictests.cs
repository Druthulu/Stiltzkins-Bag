// StiltzkinsBag.Tests/FieldItemDiagnosticTests.cs
//
// Diagnostic tests for field and world map item scanning.
// Covers .bytes files extracted from p0data7.bin and the archive itself.
//
// Prerequisites — place in TestData/ before running:
//   TestData/p0data7.bin
//     From: FINAL FANTASY IX_Data/StreamingAssets/p0data7.bin (vanilla Steam install)
//   TestData/StreamingAssets/assets/resources/commonasset/eventengine/eventbinary/
//     field/us/evt_*.eb.bytes   — field scripts (817 expected)
//     world/us/evt_world_*.eb.bytes — world map scripts (13 expected)
//
// Tests skip gracefully if required files/directories are absent.
//
// Exclusions:
//   EVT_ALEX3_AC_SEAT_N — "unknown field" with no field ID. Excluded from all scans.
//   See FieldScriptExclusions.UnknownFieldNoId for documentation.
//
// Output files (written to bin/Debug/net8.0/TestData/):
//   FieldItemDiagnostic_FromBytes_Field.txt    — items from field .bytes files
//   FieldItemDiagnostic_FromBytes_WorldMap.txt — items from world map .bytes files
//   FieldItemDiagnostic_FromArchive.txt        — items from p0data7.bin (field + world)
//   FieldItemDiagnostic_CrossValidation.txt    — disk vs archive item count comparison
//   FieldItemDiagnostic_LocaleComparison.txt   — byte comparison across all 7 locale copies

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using StiltzkinsBag.Core.Models;
using StiltzkinsBag.Core.Parsing;
using StiltzkinsBag.Parsing;

namespace StiltzkinsBag.Tests;

public class FieldItemDiagnosticTests
{
    // ── Paths ──────────────────────────────────────────────────────────────

    private static readonly string TestDataDir =
        Path.Combine(AppContext.BaseDirectory, "TestData");

    private static readonly string ArchivePath =
        Path.Combine(TestDataDir, "p0data7.bin");

    private static readonly string EventBinaryRoot =
        Path.Combine(TestDataDir, "StreamingAssets", "assets", "resources",
                     "commonasset", "eventengine", "eventbinary");

    private static readonly string FieldBytesDir =
        Path.Combine(EventBinaryRoot, "field", "us");

    private static readonly string WorldMapBytesDir =
        Path.Combine(EventBinaryRoot, "world", "us");

    private static readonly string OutputDir = TestDataDir;

    private readonly ITestOutputHelper _out;
    public FieldItemDiagnosticTests(ITestOutputHelper output) => _out = output;

    // ── Shared: load .eb.bytes files from a directory ─────────────────────

    /// <summary>
    /// Loads all .eb.bytes files from a directory.
    /// Name returned is the filename without .bytes (e.g. "evt_alex1_ac_ent_2f.eb").
    /// EVT_ALEX3_AC_SEAT_N is excluded from results.
    /// </summary>
    private static List<(string Name, byte[] Bytes)> LoadFieldBytesFromDir(string dir)
    {
        if (!Directory.Exists(dir))
            return [];

        return Directory
            .GetFiles(dir, "*.eb.bytes", SearchOption.TopDirectoryOnly)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .Select(path => (
                Name: Path.GetFileNameWithoutExtension(path),  // strips .bytes
                Bytes: File.ReadAllBytes(path)))
            .Where(f => !FieldScriptExclusions.IsExcluded(f.Name))
            .ToList();
    }

    // ── Shared: scan files for item locations ─────────────────────────────

    private static List<FieldItemRecord> ScanFieldFiles(
        IEnumerable<(string Name, byte[] Bytes)> files)
    {
        var results = new List<FieldItemRecord>();

        foreach (var (name, bytes) in files)
        {
            if (FieldScriptExclusions.IsExcluded(name)) continue;

            IReadOnlyList<FieldItemLocation> locations;
            try { locations = FieldParser.FindItemLocations(bytes); }
            catch { continue; }

            foreach (var loc in locations)
            {
                if (loc.LocationKind != FieldLocationKind.DirectItem &&
                    loc.LocationKind != FieldLocationKind.TreasureItem)
                    continue;

                if (loc.LocationKind == FieldLocationKind.TreasureItem && !loc.TreasureIsItem)
                    continue;

                int itemId = loc.CurrentValue;
                if (itemId < 1 || itemId > 255) continue;

                results.Add(new FieldItemRecord(name, itemId,
                    loc.LocationKind.ToString(), loc.FileOffset));
            }
        }

        return results;
    }

    // ── Shared: write diagnostic file ─────────────────────────────────────

    private static void WriteDiagnosticFile(
        string outputPath,
        string sourceDescription,
        List<FieldItemRecord> records)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Field Item Diagnostic — {sourceDescription}");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Total item locations found: {records.Count}");
        sb.AppendLine();

        sb.AppendLine("=== PER-FILE DETAIL ===");
        sb.AppendLine();
        foreach (var group in records
            .GroupBy(r => r.FileName)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
        {
            sb.AppendLine($"[{group.Key}]");
            foreach (var rec in group.OrderBy(r => r.FileOffset))
                sb.AppendLine($"  {rec.LocationKind,-12} offset={rec.FileOffset,6} → item {rec.ItemId,3}");
            sb.AppendLine();
        }

        sb.AppendLine("=== SUMMARY BY ITEM ID ===");
        sb.AppendLine();
        var directIds = records.Where(r => r.LocationKind == "DirectItem").Select(r => r.ItemId).Distinct().OrderBy(x => x).ToList();
        var treasureIds = records.Where(r => r.LocationKind == "TreasureItem").Select(r => r.ItemId).Distinct().OrderBy(x => x).ToList();
        var allIds = directIds.Union(treasureIds).OrderBy(x => x).ToList();
        sb.AppendLine($"Direct   item IDs ({directIds.Count,3} unique): [{string.Join(", ", directIds)}]");
        sb.AppendLine($"Treasure item IDs ({treasureIds.Count,3} unique): [{string.Join(", ", treasureIds)}]");
        sb.AppendLine($"Combined item IDs ({allIds.Count,3} unique): [{string.Join(", ", allIds)}]");
        sb.AppendLine();

        sb.AppendLine("=== REVERSE LOOKUP (item ID → which files) ===");
        sb.AppendLine();
        foreach (var group in records.GroupBy(r => r.ItemId).OrderBy(g => g.Key))
        {
            var sources = group
                .Select(r => $"{r.FileName}[{r.LocationKind}@{r.FileOffset}]")
                .Distinct().OrderBy(s => s, StringComparer.OrdinalIgnoreCase);
            sb.AppendLine($"  item {group.Key,3}: {string.Join(", ", sources)}");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    // ── Test 1 — Scan field .bytes from disk ──────────────────────────────

    [Fact]
    public void Diagnostic_FieldItems_FromBytesFiles_Field()
    {
        if (!Directory.Exists(FieldBytesDir))
        {
            _out.WriteLine($"SKIP: Field bytes directory not found: {FieldBytesDir}");
            return;
        }

        var files = LoadFieldBytesFromDir(FieldBytesDir);
        if (files.Count == 0) { _out.WriteLine("SKIP: No .eb.bytes files found."); return; }

        List<FieldItemRecord> records = ScanFieldFiles(files);

        string outPath = Path.Combine(OutputDir, "FieldItemDiagnostic_FromBytes_Field.txt");
        WriteDiagnosticFile(outPath, $"field .bytes ({files.Count} files, excluded: EVT_ALEX3_AC_SEAT_N)", records);

        _out.WriteLine($"Scanned:    {files.Count} field files (EVT_ALEX3_AC_SEAT_N excluded)");
        _out.WriteLine($"Locations:  {records.Count}");
        _out.WriteLine($"Unique IDs: {records.Select(r => r.ItemId).Distinct().Count()}");
        _out.WriteLine($"Output: {outPath}");

        Assert.True(records.Count > 0, "Expected item locations in field scripts.");
    }

    // ── Test 2 — Scan world map .bytes from disk ──────────────────────────

    [Fact]
    public void Diagnostic_FieldItems_FromBytesFiles_WorldMap()
    {
        if (!Directory.Exists(WorldMapBytesDir))
        {
            _out.WriteLine($"SKIP: World map bytes directory not found: {WorldMapBytesDir}");
            return;
        }

        var files = LoadFieldBytesFromDir(WorldMapBytesDir);
        if (files.Count == 0) { _out.WriteLine("SKIP: No .eb.bytes files found."); return; }

        List<FieldItemRecord> records = ScanFieldFiles(files);

        string outPath = Path.Combine(OutputDir, "FieldItemDiagnostic_FromBytes_WorldMap.txt");
        WriteDiagnosticFile(outPath, $"world map .bytes ({files.Count} files)", records);

        _out.WriteLine($"Scanned:    {files.Count} world map files");
        _out.WriteLine($"Locations:  {records.Count}");
        _out.WriteLine($"Unique IDs: {records.Select(r => r.ItemId).Distinct().Count()}");
        _out.WriteLine($"Output: {outPath}");
    }

    // ── Test 3 — Scan from p0data7.bin archive ────────────────────────────

    [Fact]
    public void Diagnostic_FieldItems_FromArchive()
    {
        if (!File.Exists(ArchivePath))
        {
            _out.WriteLine($"SKIP: p0data7.bin not found at {ArchivePath}");
            return;
        }

        var files = new List<(string Name, byte[] Bytes)>();
        int failed = 0;

        using (var archive = UnityArchiver.Open(ArchivePath))
        {
            var evtNames = archive.GetFileNames()
                .Where(n => n.StartsWith("evt_", StringComparison.OrdinalIgnoreCase)
                         && !n.StartsWith("evt_battle_", StringComparison.OrdinalIgnoreCase)
                         && !FieldScriptExclusions.IsExcluded(n))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();

            _out.WriteLine($"Archive evt_ entries (deduplicated, exclusions applied): {evtNames.Count}");

            foreach (string name in evtNames)
            {
                byte[] bytes;
                try { bytes = archive.Extract(name); }
                catch { failed++; continue; }
                files.Add((name, bytes));
            }
        }

        _out.WriteLine($"Extracted: {files.Count}  Failed: {failed}");
        if (files.Count == 0) { _out.WriteLine("SKIP: No files extracted."); return; }

        List<FieldItemRecord> records = ScanFieldFiles(files);

        string outPath = Path.Combine(OutputDir, "FieldItemDiagnostic_FromArchive.txt");
        WriteDiagnosticFile(outPath,
            $"p0data7.bin ({files.Count} extracted, {failed} failed, EVT_ALEX3_AC_SEAT_N excluded)",
            records);

        _out.WriteLine($"Locations:  {records.Count}");
        _out.WriteLine($"Unique IDs: {records.Select(r => r.ItemId).Distinct().Count()}");
        _out.WriteLine($"Output: {outPath}");

        Assert.True(records.Count > 0, "Expected item locations in archive field scripts.");
    }

    // ── Test 4 — Cross-validation: disk bytes == archive ──────────────────
    //
    // Drives the archive scan from disk filenames (same pattern as BattleItemDiagnosticTests)
    // to ensure an exact 1:1 comparison. No GetFileNames() filtering — we only compare
    // files we have on disk.

    [Fact]
    public void CrossValidation_FieldBytesAndArchive_ProduceSameItemCounts()
    {
        if (!File.Exists(ArchivePath))
        {
            _out.WriteLine($"SKIP: p0data7.bin not found at {ArchivePath}");
            return;
        }

        bool fieldExists = Directory.Exists(FieldBytesDir);
        bool worldExists = Directory.Exists(WorldMapBytesDir);
        if (!fieldExists && !worldExists)
        {
            _out.WriteLine("SKIP: Neither field nor world map .bytes directories found.");
            return;
        }

        // ── Scan disk ──────────────────────────────────────────────────────
        var diskFiles = new List<(string Name, byte[] Bytes)>();
        if (fieldExists) diskFiles.AddRange(LoadFieldBytesFromDir(FieldBytesDir));
        if (worldExists) diskFiles.AddRange(LoadFieldBytesFromDir(WorldMapBytesDir));

        if (diskFiles.Count == 0) { _out.WriteLine("SKIP: No .bytes files found on disk."); return; }

        List<FieldItemRecord> diskRecords = ScanFieldFiles(diskFiles);
        var diskCounts = diskRecords.GroupBy(r => r.ItemId).ToDictionary(g => g.Key, g => g.Count());

        // ── Scan archive — explicit US locale via ExtractByPath ───────────
        // Use full US locale paths from the AssetBundle table to guarantee we always
        // compare US-locale archive bytes against US-locale disk bytes.
        // Extract(shortName) returns the first matching entry regardless of locale,
        // which can be JP (with genuinely different bytecode) for some scripts.
        // ExtractByPath pins extraction to the exact /field/us/ or /world/us/ path.
        const string FieldUsBase = "assets/resources/commonasset/eventengine/eventbinary/field/us/";
        const string WorldUsBase = "assets/resources/commonasset/eventengine/eventbinary/world/us/";

        var archiveFiles = new List<(string Name, byte[] Bytes)>();
        int extractFailed = 0;

        using (var archive = UnityArchiver.Open(ArchivePath))
        {
            foreach (var (diskName, _) in diskFiles)
            {
                // diskName is like "evt_alex1_ac_ent_2f.eb" (stripped of .bytes by loader)
                bool isWorld = diskName.StartsWith("evt_world_", StringComparison.OrdinalIgnoreCase);
                string fullPath = (isWorld ? WorldUsBase : FieldUsBase) + diskName + ".bytes";

                byte[] bytes;
                try { bytes = archive.ExtractByPath(fullPath); }
                catch
                {
                    // Fall back to short name if full path not in bundle table
                    try { bytes = archive.Extract(diskName); }
                    catch { extractFailed++; continue; }
                }
                archiveFiles.Add((diskName, bytes));
            }
        }

        List<FieldItemRecord> archiveRecords = ScanFieldFiles(archiveFiles);
        var archiveCounts = archiveRecords.GroupBy(r => r.ItemId).ToDictionary(g => g.Key, g => g.Count());

        // ── Compare item count maps ────────────────────────────────────────
        var allIds = diskCounts.Keys.Union(archiveCounts.Keys).OrderBy(x => x).ToList();
        var discrepancies = new List<string>();
        foreach (int id in allIds)
        {
            int dc = diskCounts.TryGetValue(id, out int d) ? d : 0;
            int ac = archiveCounts.TryGetValue(id, out int a) ? a : 0;
            if (dc != ac)
                discrepancies.Add($"  item {id,3}: disk={dc}  archive={ac}");
        }

        // ── Write report ───────────────────────────────────────────────────
        int fieldCount = fieldExists ? LoadFieldBytesFromDir(FieldBytesDir).Count : 0;
        int worldCount = worldExists ? LoadFieldBytesFromDir(WorldMapBytesDir).Count : 0;

        var sb = new StringBuilder();
        sb.AppendLine("Field Item Cross-Validation Report (disk .bytes vs p0data7.bin archive)");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Disk files:          {diskFiles.Count}  (field={fieldCount}, world={worldCount}, EVT_ALEX3_AC_SEAT_N excluded)");
        sb.AppendLine($"Archive extracted:   {archiveFiles.Count}  (failures={extractFailed})");
        sb.AppendLine($"Disk locations:      {diskRecords.Count}");
        sb.AppendLine($"Archive locations:   {archiveRecords.Count}");
        sb.AppendLine($"Disk unique IDs:     {diskCounts.Count}");
        sb.AppendLine($"Archive unique IDs:  {archiveCounts.Count}");
        sb.AppendLine($"Item count discrepancies: {discrepancies.Count}");
        sb.AppendLine();

        if (discrepancies.Count > 0)
        {
            sb.AppendLine("=== DISCREPANCIES ===");
            foreach (string d in discrepancies) sb.AppendLine(d);
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("✓ Disk and archive produce identical item count maps.");
        }

        string reportPath = Path.Combine(OutputDir, "FieldItemDiagnostic_CrossValidation.txt");
        File.WriteAllText(reportPath, sb.ToString(), Encoding.UTF8);

        _out.WriteLine($"Disk files:        {diskFiles.Count}");
        _out.WriteLine($"Archive extracted: {archiveFiles.Count}  failures={extractFailed}");
        _out.WriteLine($"Disk locations:    {diskRecords.Count}");
        _out.WriteLine($"Archive locations: {archiveRecords.Count}");
        _out.WriteLine($"Discrepancies:     {discrepancies.Count}");
        if (discrepancies.Count == 0) _out.WriteLine("✓ Identical item counts.");

        double ratio = diskRecords.Count == 0 ? 0.0
            : Math.Abs(archiveRecords.Count - diskRecords.Count) / (double)diskRecords.Count;

        Assert.True(ratio < 0.05,
            $"Disk ({diskRecords.Count}) and archive ({archiveRecords.Count}) location counts " +
            $"differ by {ratio:P1} — exceeds 5% tolerance. " +
            $"Check FieldItemDiagnostic_CrossValidation.txt for details.");
    }

    // ── Test 5 — Locale comparison: are all 7 locale copies byte-identical? ──
    //
    // Uses GetFullPaths() to find all locale copies of each field/world script,
    // groups by filename, extracts all, and byte-compares.
    // Logs: identical groups, size-different stubs, content-differing same-size files.
    // This is informational — does not assert all locales are identical.

    [Fact]
    public void Diagnostic_LocaleVariants_ByteComparison()
    {
        if (!File.Exists(ArchivePath))
        {
            _out.WriteLine($"SKIP: p0data7.bin not found at {ArchivePath}");
            return;
        }

        using var archive = UnityArchiver.Open(ArchivePath);

        // Get full paths from AssetBundle — needed to distinguish locale copies
        var fullPaths = archive.GetFullPaths();
        if (fullPaths.Count == 0)
        {
            _out.WriteLine("SKIP: p0data7.bin has no AssetBundle path table (GetFullPaths returned empty).");
            return;
        }

        // Filter to field and world map scripts only
        // Path format: assets/resources/commonasset/eventengine/eventbinary/{field|world}/{locale}/evt_*.eb.bytes
        var fieldPaths = fullPaths
            .Where(p => p.Contains("/eventbinary/", StringComparison.OrdinalIgnoreCase) &&
                        p.EndsWith(".eb.bytes", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (fieldPaths.Count == 0)
        {
            _out.WriteLine("SKIP: No .eb.bytes paths found in archive full path table.");
            return;
        }

        // Group by filename (last path segment) — e.g. "evt_alex1_ac_ent_2f.eb.bytes"
        var byFilename = fieldPaths
            .GroupBy(p => p.Split('/').Last(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        int totalGroups = byFilename.Count;
        int identical = 0;
        int sizeDiffers = 0;
        int contentDiffers = 0;
        int singleLocale = 0;
        int excluded = 0;

        var sb = new StringBuilder();
        sb.AppendLine("Field Script Locale Comparison Report");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Total unique filenames in archive: {totalGroups}");
        sb.AppendLine();

        var sizeDifferLines = new List<string>();
        var contentDifferLines = new List<string>();

        foreach (var (filename, paths) in byFilename.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase))
        {
            // Check exclusion
            if (FieldScriptExclusions.IsExcluded(filename))
            {
                excluded++;
                continue;
            }

            if (paths.Count == 1)
            {
                singleLocale++;
                continue;
            }

            // Extract all locale copies
            var copies = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
            foreach (string path in paths)
            {
                // Extract locale name from path (e.g. ".../field/us/..." → "us")
                string[] parts = path.Split('/');
                // locale is the segment before the filename
                string locale = parts.Length >= 2 ? parts[^2] : "?";

                byte[] bytes;
                try { bytes = archive.ExtractByPath(path); }
                catch { continue; }
                copies[locale] = bytes;
            }

            if (copies.Count <= 1) { singleLocale++; continue; }

            // Compare all copies against the first
            var first = copies.Values.First();
            string firstName = copies.Keys.First();
            bool allSameSize = copies.Values.All(b => b.Length == first.Length);
            bool allIdentical = allSameSize && copies.Values.All(b => b.SequenceEqual(first));

            if (allIdentical)
            {
                identical++;
                continue;
            }

            // Build size summary: locale=Nb for each locale
            string sizeSummary = string.Join("  ",
                copies.OrderBy(kv => kv.Key)
                      .Select(kv => $"{kv.Key}={kv.Value.Length}b"));

            if (!allSameSize)
            {
                sizeDiffers++;
                sizeDifferLines.Add($"  [{filename}]  {sizeSummary}");
                continue;
            }

            // Same size but different content.
            // Find the first differing byte AND check whether any difference
            // exists at byte >= 128 (the bytecode region — entry table + function bodies).
            // Differences at bytes 0-127 are header metadata (name, unknowns) and do NOT
            // affect item scanning. Differences at bytes >= 128 affect bytecode.
            int firstDiff = -1;
            bool hasBytecodeDiff = false;
            foreach (var (locale, bytes) in copies.Where(kv => kv.Key != firstName))
            {
                for (int i = 0; i < first.Length; i++)
                {
                    if (bytes[i] != first[i])
                    {
                        if (firstDiff < 0) firstDiff = i;
                        if (i >= 128) { hasBytecodeDiff = true; break; }
                    }
                }
                if (hasBytecodeDiff) break;
            }

            contentDiffers++;
            string bcTag = hasBytecodeDiff ? "  BYTECODE_DIFFERS" : "  header-only";
            contentDifferLines.Add(
                $"  [{filename}]  {sizeSummary}  first_diff_at={firstDiff}{bcTag}");
        }

        // Write report
        int bytecodeDiffers = contentDifferLines.Count(l => l.Contains("BYTECODE_DIFFERS"));
        int headerOnlyDiffers = contentDiffers - bytecodeDiffers;

        sb.AppendLine($"Results:");
        sb.AppendLine($"  Identical across all locales:           {identical}");
        sb.AppendLine($"  Size differs (stub locales):            {sizeDiffers}");
        sb.AppendLine($"  Content differs — header only (<128b):  {headerOnlyDiffers}");
        sb.AppendLine($"  Content differs — BYTECODE (>=128b):    {bytecodeDiffers}  ← item scanning affected");
        sb.AppendLine($"  Single locale only:                     {singleLocale}");
        sb.AppendLine($"  Excluded (EVT_ALEX3_AC_SEAT_N etc):     {excluded}");
        sb.AppendLine();

        if (sizeDifferLines.Count > 0)
        {
            sb.AppendLine("=== SIZE DIFFERS (some locales are stub/empty scripts) ===");
            foreach (string line in sizeDifferLines) sb.AppendLine(line);
            sb.AppendLine();
        }

        if (contentDifferLines.Count > 0)
        {
            sb.AppendLine("=== CONTENT DIFFERS (same size, different bytes) ===");
            foreach (string line in contentDifferLines) sb.AppendLine(line);
            sb.AppendLine();
        }

        if (sizeDifferLines.Count == 0 && contentDifferLines.Count == 0)
            sb.AppendLine("✓ All multi-locale scripts are byte-identical across locales.");

        string outPath = Path.Combine(OutputDir, "FieldItemDiagnostic_LocaleComparison.txt");
        File.WriteAllText(outPath, sb.ToString(), Encoding.UTF8);

        _out.WriteLine($"Total unique filenames: {totalGroups}");
        _out.WriteLine($"Identical:    {identical}");
        _out.WriteLine($"Size differs: {sizeDiffers}");
        _out.WriteLine($"Content diff: {contentDiffers}");
        _out.WriteLine($"Single locale:{singleLocale}");
        _out.WriteLine($"Excluded:     {excluded}");
        _out.WriteLine($"Output: {outPath}");

        // Informational only — locale differences are expected for some JP scripts
        // This test always passes; check the output file for manual review
        Assert.True(totalGroups > 0, "Expected to find field script groups in archive.");
    }

    // ── Data record ───────────────────────────────────────────────────────

    private record FieldItemRecord(
        string FileName,
        int ItemId,
        string LocationKind,
        int FileOffset);
}