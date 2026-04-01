// StiltzkinsBag.Tests/BattleItemScannerTests.cs
//
// Tests for BattleItemScanner.
// The scanner collects item IDs from drop/steal/card slots across all battle files.
// It makes no classification judgements — that belongs in VanillaItemCatalog.
//
// Prerequisites — place in TestData/ before running:
//   TestData/p0data2.bin
//   TestData/StreamingAssets/assets/resources/battlemap/battlescene/
//     evt_battle_*/dbfile0000.raw16.bytes

using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using StiltzkinsBag.Core.Models.Battle;

namespace StiltzkinsBag.Tests;

public class BattleItemScannerTests
{
    // ── Paths ──────────────────────────────────────────────────────────────

    private static readonly string TestDataDir =
        Path.Combine(AppContext.BaseDirectory, "TestData");

    private static readonly string ArchivePath =
        Path.Combine(TestDataDir, "p0data2.bin");

    private static readonly string BattleSceneRoot =
        Path.Combine(TestDataDir, "StreamingAssets", "assets", "resources",
                     "battlemap", "battlescene");

    private readonly ITestOutputHelper _out;
    public BattleItemScannerTests(ITestOutputHelper output) => _out = output;

    // ── Helpers ────────────────────────────────────────────────────────────

    private static System.Collections.Generic.List<(string FolderPath, byte[] Bytes)> LoadDiskFiles()
    {
        if (!Directory.Exists(BattleSceneRoot)) return [];

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

    private static void SkipIfNoBattleFiles(
        System.Collections.Generic.List<(string, byte[])> files)
    {
        if (files.Count == 0)
            throw new SkipException(
                $"No dbfile0000.raw16.bytes found under {BattleSceneRoot}.");
    }

    private static void SkipIfNoArchive()
    {
        if (!File.Exists(ArchivePath))
            throw new SkipException($"p0data2.bin not found at {ArchivePath}.");
    }

    // ── 1. ScanFiles ──────────────────────────────────────────────────────

    [Fact]
    public void ScanFiles_WithDiskBytes_ReturnsNonEmptyResults()
    {
        var files = LoadDiskFiles();
        SkipIfNoBattleFiles(files);

        BattleScanResult result = BattleItemScanner.ScanFiles(files);

        Assert.True(result.DropItemIds.Count > 0, "Expected drop items from .bytes files.");
        Assert.True(result.StealItemIds.Count > 0, "Expected steal items from .bytes files.");
        Assert.True(result.CardDropIds.Count > 0, "Expected card drops from .bytes files.");

        _out.WriteLine($"Files scanned:  {files.Count}");
        _out.WriteLine($"Drop items:     {result.DropItemIds.Count} unique");
        _out.WriteLine($"Steal items:    {result.StealItemIds.Count} unique");
        _out.WriteLine($"Card drops:     {result.CardDropIds.Count} unique");
        _out.WriteLine($"All enemy:      {result.AllEnemyItemIds.Count} unique");
        _out.WriteLine($"Drop IDs:   [{string.Join(", ", result.DropItemIds.OrderBy(x => x))}]");
        _out.WriteLine($"Steal IDs:  [{string.Join(", ", result.StealItemIds.OrderBy(x => x))}]");
        _out.WriteLine($"Card IDs:   [{string.Join(", ", result.CardDropIds.OrderBy(x => x))}]");
    }

    [Fact]
    public void ScanFiles_NoZeroIds_InAnyResultSet()
    {
        // 0 is the EnemyFile empty-slot sentinel — must be filtered by the scanner.
        var files = LoadDiskFiles();
        SkipIfNoBattleFiles(files);

        BattleScanResult result = BattleItemScanner.ScanFiles(files);

        Assert.DoesNotContain(0, result.DropItemIds);
        Assert.DoesNotContain(0, result.StealItemIds);
        Assert.DoesNotContain(0, result.CardDropIds);
    }

    [Fact]
    public void ScanFiles_AllEnemyItemIds_IsUnionOfDropsAndSteals()
    {
        var files = LoadDiskFiles();
        SkipIfNoBattleFiles(files);

        BattleScanResult result = BattleItemScanner.ScanFiles(files);

        // Every drop and steal ID must be in AllEnemyItemIds
        foreach (int id in result.DropItemIds)
            Assert.Contains(id, result.AllEnemyItemIds);
        foreach (int id in result.StealItemIds)
            Assert.Contains(id, result.AllEnemyItemIds);

        // AllEnemyItemIds must contain nothing outside drops or steals
        foreach (int id in result.AllEnemyItemIds)
            Assert.True(result.DropItemIds.Contains(id) || result.StealItemIds.Contains(id),
                $"ID {id} in AllEnemyItemIds but absent from both drops and steals.");
    }

    [Fact]
    public void ScanFiles_EmptyInput_ReturnsEmptyResult()
    {
        BattleScanResult result = BattleItemScanner.ScanFiles(
            Array.Empty<(string, byte[])>());

        Assert.Empty(result.DropItemIds);
        Assert.Empty(result.StealItemIds);
        Assert.Empty(result.CardDropIds);
        Assert.Empty(result.AllEnemyItemIds);
    }

    // ── 2. ScanArchive ────────────────────────────────────────────────────

    [Fact]
    public void ScanArchive_ReturnsNonEmptyResults()
    {
        SkipIfNoArchive();

        BattleScanResult result = BattleItemScanner.ScanArchive(ArchivePath);

        Assert.True(result.DropItemIds.Count > 0, "Expected drop items from archive.");
        Assert.True(result.StealItemIds.Count > 0, "Expected steal items from archive.");
        Assert.True(result.CardDropIds.Count > 0, "Expected card drops from archive.");

        _out.WriteLine($"Drop items:  {result.DropItemIds.Count} unique");
        _out.WriteLine($"Steal items: {result.StealItemIds.Count} unique");
        _out.WriteLine($"Card drops:  {result.CardDropIds.Count} unique");
        _out.WriteLine($"All enemy:   {result.AllEnemyItemIds.Count} unique");
    }

    [Fact]
    public void ScanArchive_NoZeroIds_InAnyResultSet()
    {
        SkipIfNoArchive();

        BattleScanResult result = BattleItemScanner.ScanArchive(ArchivePath);

        Assert.DoesNotContain(0, result.DropItemIds);
        Assert.DoesNotContain(0, result.StealItemIds);
        Assert.DoesNotContain(0, result.CardDropIds);
    }

    // ── 3. Cross-validation: disk files == archive ────────────────────────

    [Fact]
    public void CrossValidation_ScanFiles_And_ScanArchive_ProduceSameItemSets()
    {
        var files = LoadDiskFiles();
        SkipIfNoBattleFiles(files);
        SkipIfNoArchive();

        BattleScanResult fromFiles = BattleItemScanner.ScanFiles(files);
        BattleScanResult fromArchive = BattleItemScanner.ScanArchive(ArchivePath);

        var onlyInFiles = fromFiles.AllEnemyItemIds.Except(fromArchive.AllEnemyItemIds).OrderBy(x => x).ToList();
        var onlyInArchive = fromArchive.AllEnemyItemIds.Except(fromFiles.AllEnemyItemIds).OrderBy(x => x).ToList();
        var cardOnlyInFiles = fromFiles.CardDropIds.Except(fromArchive.CardDropIds).OrderBy(x => x).ToList();
        var cardOnlyInArchive = fromArchive.CardDropIds.Except(fromFiles.CardDropIds).OrderBy(x => x).ToList();

        _out.WriteLine($"Files   — drops:{fromFiles.DropItemIds.Count} steals:{fromFiles.StealItemIds.Count} cards:{fromFiles.CardDropIds.Count}");
        _out.WriteLine($"Archive — drops:{fromArchive.DropItemIds.Count} steals:{fromArchive.StealItemIds.Count} cards:{fromArchive.CardDropIds.Count}");

        if (onlyInFiles.Count > 0)
            _out.WriteLine($"Only in files (enemy):   [{string.Join(", ", onlyInFiles)}]");
        if (onlyInArchive.Count > 0)
            _out.WriteLine($"Only in archive (enemy): [{string.Join(", ", onlyInArchive)}]");
        if (cardOnlyInFiles.Count > 0)
            _out.WriteLine($"Only in files (cards):   [{string.Join(", ", cardOnlyInFiles)}]");
        if (cardOnlyInArchive.Count > 0)
            _out.WriteLine($"Only in archive (cards): [{string.Join(", ", cardOnlyInArchive)}]");

        Assert.Empty(onlyInFiles);
        Assert.Empty(onlyInArchive);
        Assert.Empty(cardOnlyInFiles);
        Assert.Empty(cardOnlyInArchive);
    }
}