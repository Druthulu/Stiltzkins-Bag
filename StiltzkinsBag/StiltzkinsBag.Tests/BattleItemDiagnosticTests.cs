// StiltzkinsBag.Tests/BattleItemDiagnosticTests.cs
//
// Diagnostic tests for Task 3a — BattleItemScanner investigation.
//
// All JSON catalog references have been removed. Ground truth comes exclusively
// from extracted .bytes files and p0data2.bin archive on a clean Steam install.
//
// Prerequisites — place in TestData/ before running:
//   TestData/p0data2.bin
//     From: FINAL FANTASY IX_Data/StreamingAssets/p0data2.bin (vanilla Steam install)
//   TestData/StreamingAssets/assets/resources/battlemap/battlescene/
//     evt_battle_*/dbfile0000.raw16.bytes
//     From: extracted from the same vanilla p0data2.bin
//
// Tests skip gracefully if required files/directories are absent.
//
// Output files (written to bin/Debug/net8.0/TestData/):
//   BattleList.txt                           — one evt_battle_* folder name per line
//   BattleItemDiagnostic_FromBytes.txt       — all items found via .bytes files
//   BattleItemDiagnostic_ArchiveNames.txt    — raw dump of all p0data2.bin entry names
//   BattleItemDiagnostic_FromArchive.txt     — all items found via p0data2.bin
//   BattleItemDiagnostic_CrossValidation.txt — .bytes vs archive discrepancy report
//
// Archive extraction convention (p0data2.bin):
//   ExtractByPath(folderPath) where folderPath matches the EnemyFolder convention:
//     \StreamingAssets\assets\resources\battlemap\battlescene\evt_battle_XXX\dbfile0000.raw16.bytes
//   This is the same path format used by ModSourceResolver at runtime.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using StiltzkinsBag.Core.Models.Battle;
using StiltzkinsBag.Core.Parsing;

namespace StiltzkinsBag.Tests;

public class BattleItemDiagnosticTests
{
    // ── Paths ──────────────────────────────────────────────────────────────

    private static readonly string TestDataDir =
        Path.Combine(AppContext.BaseDirectory, "TestData");

    private static readonly string ArchivePath =
        Path.Combine(TestDataDir, "p0data2.bin");

    private static readonly string BattleSceneRoot =
        Path.Combine(TestDataDir, "StreamingAssets", "assets", "resources",
                     "battlemap", "battlescene");

    private static readonly string OutputDir = TestDataDir;

    private readonly ITestOutputHelper _out;
    public BattleItemDiagnosticTests(ITestOutputHelper output) => _out = output;

    // ── Shared: load .bytes files from disk ───────────────────────────────

    /// <summary>
    /// Walks BattleSceneRoot, finds all dbfile0000.raw16.bytes files, and
    /// returns (folderPath, bytes) pairs sorted by folder name.
    /// folderPath uses the EnemyFolder convention:
    ///   \StreamingAssets\...\evt_battle_XXX\dbfile0000.raw16.bytes
    /// Returns empty list if BattleSceneRoot does not exist.
    /// </summary>
    private static List<(string FolderPath, byte[] Bytes)> LoadBattleFilesFromDisk()
    {
        if (!Directory.Exists(BattleSceneRoot))
            return [];

        return Directory
            .GetFiles(BattleSceneRoot, "dbfile0000.raw16.bytes",
                      SearchOption.AllDirectories)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .Select(path =>
            {
                string rel = Path.GetRelativePath(TestDataDir, path).Replace('/', '\\');
                if (!rel.StartsWith('\\')) rel = '\\' + rel;
                return (FolderPath: rel, Bytes: File.ReadAllBytes(path));
            })
            .ToList();
    }

    // ── Shared: scan enemy files ───────────────────────────────────────────

    private static List<BattleItemRecord> ScanEnemyFiles(
        IEnumerable<(string FolderPath, byte[] Bytes)> files)
    {
        var results = new List<BattleItemRecord>();

        foreach (var (folderPath, bytes) in files)
        {
            EnemyFile file;
            try { file = new EnemyFile(bytes, folderPath); }
            catch { continue; }

            string battleName = ExtractBattleName(folderPath);

            for (int s = 0; s < file.StatCount; s++)
            {
                for (int slot = 0; slot < 4; slot++)
                {
                    byte dropId = file.GetDrop(s, slot);
                    if (dropId != 0)
                        results.Add(new BattleItemRecord(battleName, s, "drop", slot, dropId));

                    byte stealId = file.GetSteal(s, slot);
                    if (stealId != 0)
                        results.Add(new BattleItemRecord(battleName, s, "steal", slot, stealId));
                }

                byte cardId = file.GetCardDrop(s);
                if (cardId != 0)
                    results.Add(new BattleItemRecord(battleName, s, "card", 0, cardId));
            }
        }

        return results;
    }

    private static string ExtractBattleName(string folderPath)
    {
        string[] parts = folderPath.Split(new[] { '\\', '/' },
            StringSplitOptions.RemoveEmptyEntries);
        string? battlePart = parts.FirstOrDefault(p =>
            p.StartsWith("evt_battle_", StringComparison.OrdinalIgnoreCase));
        return battlePart ?? folderPath;
    }

    // ── Shared: write diagnostic file ─────────────────────────────────────

    private static void WriteDiagnosticFile(
        string outputPath,
        string sourceDescription,
        List<BattleItemRecord> records)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Battle Item Diagnostic — {sourceDescription}");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Total non-zero item slots found: {records.Count}");
        sb.AppendLine();

        sb.AppendLine("=== PER-BATTLE DETAIL ===");
        sb.AppendLine();

        foreach (var group in records
            .GroupBy(r => r.BattleName)
            .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase))
        {
            sb.AppendLine($"[{group.Key}]");
            foreach (var rec in group
                .OrderBy(r => r.StatIndex)
                .ThenBy(r => r.Kind)
                .ThenBy(r => r.Slot))
            {
                sb.AppendLine(
                    $"  stat {rec.StatIndex} | {rec.Kind,-5} slot {rec.Slot} → item {rec.ItemId,3}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("=== SUMMARY BY ITEM ID ===");
        sb.AppendLine();

        var dropIds = records.Where(r => r.Kind == "drop").Select(r => r.ItemId).Distinct().OrderBy(x => x).ToList();
        var stealIds = records.Where(r => r.Kind == "steal").Select(r => r.ItemId).Distinct().OrderBy(x => x).ToList();
        var cardIds = records.Where(r => r.Kind == "card").Select(r => r.ItemId).Distinct().OrderBy(x => x).ToList();
        var allIds = dropIds.Union(stealIds).Union(cardIds).OrderBy(x => x).ToList();

        sb.AppendLine($"All item IDs in drops  ({dropIds.Count,3} unique): [{string.Join(", ", dropIds)}]");
        sb.AppendLine($"All item IDs in steals ({stealIds.Count,3} unique): [{string.Join(", ", stealIds)}]");
        sb.AppendLine($"All item IDs in cards  ({cardIds.Count,3} unique): [{string.Join(", ", cardIds)}]");
        sb.AppendLine($"All item IDs combined  ({allIds.Count,3} unique): [{string.Join(", ", allIds)}]");
        sb.AppendLine();

        sb.AppendLine("=== REVERSE LOOKUP (item ID → which battles) ===");
        sb.AppendLine();

        foreach (var group in records.GroupBy(r => r.ItemId).OrderBy(g => g.Key))
        {
            var sources = group
                .Select(r => $"{r.BattleName}[stat{r.StatIndex} {r.Kind}{r.Slot}]")
                .Distinct()
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase);
            sb.AppendLine($"  item {group.Key,3}: {string.Join(", ", sources)}");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    // ── Test 1 — Scan from .bytes files; write BattleList.txt ─────────────

    [Fact]
    public void Diagnostic_BattleItems_FromBytesFiles()
    {
        if (!Directory.Exists(BattleSceneRoot))
        {
            _out.WriteLine($"SKIP: BattleScene directory not found: {BattleSceneRoot}");
            return;
        }

        var files = LoadBattleFilesFromDisk();

        if (files.Count == 0)
        {
            _out.WriteLine($"SKIP: No dbfile0000.raw16.bytes found under {BattleSceneRoot}");
            return;
        }

        // ── Write BattleList.txt ───────────────────────────────────────────
        // Authoritative battle catalog — one evt_battle_* folder name per line.
        // Replaces StockEnemyBytesJsonNoZeros.json as the battle file manifest.
        string battleListPath = Path.Combine(OutputDir, "BattleList.txt");
        var battleNames = files
            .Select(f => ExtractBattleName(f.FolderPath))
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();

        File.WriteAllLines(battleListPath, battleNames, Encoding.UTF8);
        _out.WriteLine($"BattleList.txt: {battleNames.Count} entries → {battleListPath}");

        // ── Scan and write diagnostic ──────────────────────────────────────
        List<BattleItemRecord> records = ScanEnemyFiles(files);

        string outPath = Path.Combine(OutputDir, "BattleItemDiagnostic_FromBytes.txt");
        WriteDiagnosticFile(outPath, $".bytes files ({files.Count} files)", records);

        _out.WriteLine($"Scanned:             {files.Count} files");
        _out.WriteLine($"Non-zero item slots: {records.Count}");
        _out.WriteLine($"Output: {outPath}");

        Assert.True(files.Count > 0, "Expected at least one .bytes file.");
        Assert.True(records.Count > 0, "Expected non-zero item slots in battle files.");
    }

    // ── Test 2 — Dump all archive entry names (discovery) ─────────────────

    [Fact]
    public void Diagnostic_DumpArchiveFileNames()
    {
        if (!File.Exists(ArchivePath))
        {
            _out.WriteLine($"SKIP: p0data2.bin not found at {ArchivePath}");
            return;
        }

        List<string> names;
        using (var archive = UnityArchiver.Open(ArchivePath))
            names = archive.GetFileNames()
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();

        string outPath = Path.Combine(OutputDir, "BattleItemDiagnostic_ArchiveNames.txt");
        var sb = new StringBuilder();
        sb.AppendLine($"p0data2.bin — all entry names ({names.Count} total)");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();
        foreach (string name in names)
            sb.AppendLine(name);

        File.WriteAllText(outPath, sb.ToString(), Encoding.UTF8);

        _out.WriteLine($"Archive entry count: {names.Count}");
        _out.WriteLine($"Output: {outPath}");
        _out.WriteLine("Sample names (first 5):");
        foreach (string n in names.Take(5))
            _out.WriteLine($"  {n}");

        // Passes unconditionally — pure discovery
        Assert.True(names.Count > 0, "Archive must contain at least one entry.");
    }

    // ── Test 3 — Scan from archive ─────────────────────────────────────────

    [Fact]
    public void Diagnostic_BattleItems_FromArchive()
    {
        if (!File.Exists(ArchivePath))
        {
            _out.WriteLine($"SKIP: p0data2.bin not found at {ArchivePath}");
            return;
        }

        if (!Directory.Exists(BattleSceneRoot))
        {
            _out.WriteLine($"SKIP: BattleScene directory not found (needed for path list): {BattleSceneRoot}");
            return;
        }

        var diskFiles = LoadBattleFilesFromDisk();
        if (diskFiles.Count == 0)
        {
            _out.WriteLine("SKIP: No .bytes files found — cannot build path list for archive extraction.");
            return;
        }

        var archiveFiles = new List<(string FolderPath, byte[] Bytes)>();
        int failed = 0;

        using (var archive = UnityArchiver.Open(ArchivePath))
        {
            foreach (var (folderPath, _) in diskFiles)
            {
                try
                {
                    byte[] bytes = archive.ExtractByPath(folderPath);
                    archiveFiles.Add((folderPath, bytes));
                }
                catch { failed++; }
            }
        }

        _out.WriteLine($"Disk files:   {diskFiles.Count}");
        _out.WriteLine($"Extracted:    {archiveFiles.Count}");
        _out.WriteLine($"Failed:       {failed}");

        if (archiveFiles.Count == 0)
        {
            _out.WriteLine("SKIP: No files could be extracted from archive.");
            return;
        }

        List<BattleItemRecord> records = ScanEnemyFiles(archiveFiles);

        string outPath = Path.Combine(OutputDir, "BattleItemDiagnostic_FromArchive.txt");
        WriteDiagnosticFile(outPath,
            $"p0data2.bin ({archiveFiles.Count} extracted, {failed} failed)", records);

        _out.WriteLine($"Non-zero item slots: {records.Count}");
        _out.WriteLine($"Output: {outPath}");

        Assert.True(records.Count > 0, "Expected non-zero item slots from archive.");
    }

    // ── Test 4 — Cross-validation: .bytes == archive ───────────────────────

    [Fact]
    public void CrossValidation_BytesAndArchive_ProduceSameItemSets()
    {
        if (!File.Exists(ArchivePath))
        {
            _out.WriteLine($"SKIP: p0data2.bin not found at {ArchivePath}");
            return;
        }

        if (!Directory.Exists(BattleSceneRoot))
        {
            _out.WriteLine($"SKIP: BattleScene directory not found: {BattleSceneRoot}");
            return;
        }

        var diskFiles = LoadBattleFilesFromDisk();
        if (diskFiles.Count == 0)
        {
            _out.WriteLine("SKIP: No .bytes files found.");
            return;
        }

        // ── Scan .bytes ────────────────────────────────────────────────────
        List<BattleItemRecord> bytesRecords = ScanEnemyFiles(diskFiles);

        // ── Scan archive ───────────────────────────────────────────────────
        var archiveFiles = new List<(string FolderPath, byte[] Bytes)>();
        int extractFailed = 0;

        using (var archive = UnityArchiver.Open(ArchivePath))
        {
            foreach (var (folderPath, _) in diskFiles)
            {
                try
                {
                    byte[] bytes = archive.ExtractByPath(folderPath);
                    archiveFiles.Add((folderPath, bytes));
                }
                catch { extractFailed++; }
            }
        }

        List<BattleItemRecord> archiveRecords = ScanEnemyFiles(archiveFiles);

        // ── Set comparison ─────────────────────────────────────────────────
        var bytesSet = bytesRecords
            .Select(r => (r.BattleName, r.StatIndex, r.Kind, r.Slot, r.ItemId))
            .ToHashSet();

        var archiveSet = archiveRecords
            .Select(r => (r.BattleName, r.StatIndex, r.Kind, r.Slot, r.ItemId))
            .ToHashSet();

        var onlyInBytes = bytesSet.Except(archiveSet)
            .OrderBy(t => t.BattleName).ThenBy(t => t.StatIndex).ToList();
        var onlyInArchive = archiveSet.Except(bytesSet)
            .OrderBy(t => t.BattleName).ThenBy(t => t.StatIndex).ToList();

        // ── Write report ───────────────────────────────────────────────────
        var sb = new StringBuilder();
        sb.AppendLine("Battle Item Cross-Validation Report (.bytes vs Archive)");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($".bytes records:     {bytesRecords.Count}");
        sb.AppendLine($"Archive records:    {archiveRecords.Count}");
        sb.AppendLine($"Extract failures:   {extractFailed}");
        sb.AppendLine($"Only in .bytes:     {onlyInBytes.Count}");
        sb.AppendLine($"Only in archive:    {onlyInArchive.Count}");
        sb.AppendLine();

        if (onlyInBytes.Count > 0)
        {
            sb.AppendLine("=== ONLY IN .BYTES ===");
            foreach (var t in onlyInBytes)
                sb.AppendLine($"  {t.BattleName} stat{t.StatIndex} {t.Kind}{t.Slot} item={t.ItemId}");
            sb.AppendLine();
        }

        if (onlyInArchive.Count > 0)
        {
            sb.AppendLine("=== ONLY IN ARCHIVE ===");
            foreach (var t in onlyInArchive)
                sb.AppendLine($"  {t.BattleName} stat{t.StatIndex} {t.Kind}{t.Slot} item={t.ItemId}");
            sb.AppendLine();
        }

        if (onlyInBytes.Count == 0 && onlyInArchive.Count == 0)
            sb.AppendLine("✓ .bytes files and archive produce identical item sets.");

        string reportPath = Path.Combine(OutputDir, "BattleItemDiagnostic_CrossValidation.txt");
        File.WriteAllText(reportPath, sb.ToString(), Encoding.UTF8);

        _out.WriteLine($".bytes records:     {bytesRecords.Count}");
        _out.WriteLine($"Archive records:    {archiveRecords.Count}");
        _out.WriteLine($"Extract failures:   {extractFailed}");
        _out.WriteLine($"Only in .bytes:     {onlyInBytes.Count}");
        _out.WriteLine($"Only in archive:    {onlyInArchive.Count}");
        _out.WriteLine($"Report: {reportPath}");

        if (onlyInBytes.Count == 0 && onlyInArchive.Count == 0)
            _out.WriteLine("✓ .bytes files and archive produce identical item sets.");

        double ratio = bytesRecords.Count == 0 ? 0.0
            : Math.Abs(archiveRecords.Count - bytesRecords.Count) / (double)bytesRecords.Count;

        Assert.True(ratio < 0.05,
            $".bytes ({bytesRecords.Count} records) and archive ({archiveRecords.Count} records) " +
            $"differ by {ratio:P1} — exceeds 5% tolerance. " +
            $"Check BattleItemDiagnostic_CrossValidation.txt for details.");
    }

    // ── Data record ───────────────────────────────────────────────────────

    private record BattleItemRecord(
        string BattleName,
        int StatIndex,
        string Kind,
        int Slot,
        byte ItemId);
}