// StiltzkinsBag.Tests/UnityArchiverTests.cs
//
// Prerequisites — place in StiltzkinsBag.Tests/TestData/ on disk.
// Do NOT set as Content/Copy if newer — files are too large.
// Add to .gitignore. Tests skip cleanly if files are absent.
//
//   p0data2.bin            — from FF9 Steam install: StreamingAssets\p0data2.bin
//   StreamingAssets/...    — exported battlescene .bytes files, full folder structure

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using StiltzkinsBag.Core.Parsing;
using StiltzkinsBag.Core.Models.Battle;

namespace StiltzkinsBag.Tests;

public class UnityArchiverTests
{
    // ── Test data paths ────────────────────────────────────────────────────

    private static readonly string TestDataDir = FindTestDataDir();

    private static string FindTestDataDir()
    {
        string dir = AppContext.BaseDirectory;
        for (int i = 0; i < 10; i++)
        {
            string candidate = Path.Combine(dir, "TestData");
            if (Directory.Exists(candidate) &&
                (File.Exists(Path.Combine(candidate, "p0data2.bin")) ||
                 Directory.Exists(Path.Combine(candidate, "StreamingAssets"))))
                return candidate;
            string? parent = Directory.GetParent(dir)?.FullName;
            if (parent is null) break;
            dir = parent;
        }
        return Path.Combine(AppContext.BaseDirectory, "TestData");
    }

    private static readonly string ArchivePath =
        Path.Combine(TestDataDir, "p0data2.bin");

    private static readonly string BattleSceneRoot =
        Path.Combine(TestDataDir, "StreamingAssets", "assets", "resources",
                     "battlemap", "battlescene");

    private static void SkipIfNoArchive()
    {
        if (!File.Exists(ArchivePath))
            throw new SkipException(
                $"p0data2.bin not found at: {ArchivePath}\n" +
                "Copy it from your FF9 Steam install (StreamingAssets\\p0data2.bin).");
    }

    private readonly ITestOutputHelper _out;
    public UnityArchiverTests(ITestOutputHelper output) => _out = output;

    // ── 1. Open and parse ──────────────────────────────────────────────────

    [Fact]
    public void Open_ValidArchive_ParsesWithoutException()
    {
        SkipIfNoArchive();
        using var archive = UnityArchiver.Open(ArchivePath);
        Assert.True(archive.FileCount > 0,
            $"Archive parsed but reports 0 entries. Path: {ArchivePath}");
        _out.WriteLine($"Archive contains {archive.FileCount} entries.");
    }

    [Fact]
    public void Open_NonExistentFile_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(
            () => UnityArchiver.Open(@"C:\does\not\exist\p0data2.bin"));
    }

    [Fact]
    public void Open_InvalidData_ThrowsInvalidDataException()
    {
        string tempPath = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempPath, new byte[256]);
            Assert.Throws<InvalidDataException>(() => UnityArchiver.Open(tempPath));
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    // ── 2. File name enumeration ───────────────────────────────────────────

    [Fact]
    public void GetFileNames_ReturnsShortNames()
    {
        // GetFileNames() always returns short embedded names (e.g. "dbfile0000.raw16"),
        // not full paths. This preserves backward compatibility with FieldItemScanner
        // which filters with StartsWith("evt_") on p0data7.bin short names.
        SkipIfNoArchive();
        using var archive = UnityArchiver.Open(ArchivePath);
        IReadOnlyList<string> names = archive.GetFileNames();
        Assert.True(names.Count > 0, "GetFileNames() returned empty list.");

        // No name should look like a full path
        foreach (string n in names.Take(20))
            Assert.False(n.StartsWith("assets/", StringComparison.OrdinalIgnoreCase),
                $"GetFileNames() returned a full path '{n}' — expected short names only.");

        _out.WriteLine($"Short names count: {names.Count}");
        _out.WriteLine($"First 5: {string.Join(", ", names.Take(5))}");
    }

    [Fact]
    public void GetFullPaths_ReturnsBattlePaths()
    {
        // GetFullPaths() returns full asset paths from the AssetBundle table.
        // p0data2.bin has an AssetBundle — should return many full paths.
        SkipIfNoArchive();
        using var archive = UnityArchiver.Open(ArchivePath);
        IReadOnlyList<string> paths = archive.GetFullPaths();
        Assert.True(paths.Count > 0,
            "GetFullPaths() returned empty list. AssetBundle table was not parsed.");

        // All should look like full paths starting with "assets/"
        int battlePaths = paths.Count(p =>
            p.Contains("battlescene", StringComparison.OrdinalIgnoreCase) &&
            p.EndsWith("dbfile0000.raw16.bytes", StringComparison.OrdinalIgnoreCase));

        Assert.True(battlePaths > 500,
            $"Expected 500+ battle dbfile paths, got {battlePaths}.");

        _out.WriteLine($"Total full paths: {paths.Count}");
        _out.WriteLine($"Battle dbfile paths: {battlePaths}");
        _out.WriteLine($"First 3: {string.Join(", ", paths.Take(3))}");
    }

    // ── 3. Extract by short name ───────────────────────────────────────────

    [Fact]
    public void Extract_KnownShortName_ReturnsNonEmptyBytes()
    {
        // Extract() uses short name lookup — finds the first entry with that name.
        // For p0data2.bin, "dbfile0000.raw16" appears 562+ times; Extract() returns
        // the first match. Use ExtractByPath() when you need a specific battle file.
        SkipIfNoArchive();
        using var archive = UnityArchiver.Open(ArchivePath);

        IReadOnlyList<string> names = archive.GetFileNames();
        string? dbFile = names.FirstOrDefault(n =>
            n.StartsWith("dbfile", StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(dbFile);
        _out.WriteLine($"Extracting short name: {dbFile}");

        byte[] data = archive.Extract(dbFile!);
        Assert.True(data.Length > 0, $"Extract('{dbFile}') returned empty array.");
        _out.WriteLine($"Extracted {data.Length} bytes.");
    }

    // ── 4. ExtractByPath — full path disambiguation ────────────────────────

    [Fact]
    public void ExtractByPath_WithFullAssetPath_ReturnsValidEnemyBytes()
    {
        // ExtractByPath() uses the AssetBundle full-path table to find the correct
        // entry even when many entries share the same short name.
        SkipIfNoArchive();
        using var archive = UnityArchiver.Open(ArchivePath);

        IReadOnlyList<string> paths = archive.GetFullPaths();
        string? battlePath = paths.FirstOrDefault(p =>
            p.Contains("battlescene", StringComparison.OrdinalIgnoreCase) &&
            p.EndsWith("dbfile0000.raw16.bytes", StringComparison.OrdinalIgnoreCase));

        Assert.NotNull(battlePath);
        _out.WriteLine($"Extracting full path: {battlePath}");

        // ExtractByPath accepts both "assets/..." and "\StreamingAssets\assets\..." formats
        byte[] byAssetPath = archive.ExtractByPath(battlePath!);
        Assert.True(byAssetPath.Length > 0);

        string catalogFormat = @"\StreamingAssets\" + battlePath!.Replace('/', '\\');
        byte[] byCatalogPath = archive.ExtractByPath(catalogFormat);
        Assert.True(byCatalogPath.Length > 0);

        Assert.Equal(byAssetPath, byCatalogPath);
        _out.WriteLine($"Extracted {byAssetPath.Length} bytes. Both path formats agree.");
    }

    [Fact]
    public void ExtractByPath_DifferentBattles_ReturnDifferentBytes()
    {
        // Confirms that full-path lookup correctly distinguishes between two different
        // battles that both contain a file named "dbfile0000.raw16".
        // This would fail with leaf-name lookup (both would return the first match).
        SkipIfNoArchive();
        using var archive = UnityArchiver.Open(ArchivePath);

        IReadOnlyList<string> paths = archive.GetFullPaths();
        var battlePaths = paths
            .Where(p => p.Contains("battlescene", StringComparison.OrdinalIgnoreCase) &&
                        p.EndsWith("dbfile0000.raw16.bytes", StringComparison.OrdinalIgnoreCase))
            .Take(2)
            .ToList();

        if (battlePaths.Count < 2)
            throw new SkipException("Need at least 2 battle paths to test disambiguation.");

        byte[] bytes1 = archive.ExtractByPath(battlePaths[0]);
        byte[] bytes2 = archive.ExtractByPath(battlePaths[1]);

        Assert.True(bytes1.Length > 0, "First battle extraction returned empty.");
        Assert.True(bytes2.Length > 0, "Second battle extraction returned empty.");
        Assert.False(bytes1.SequenceEqual(bytes2),
            $"Two different battle paths returned identical bytes — " +
            $"path lookup is not distinguishing entries correctly.\n" +
            $"  Path 1: {battlePaths[0]}\n  Path 2: {battlePaths[1]}");

        _out.WriteLine($"Path 1: {battlePaths[0]} → {bytes1.Length} bytes");
        _out.WriteLine($"Path 2: {battlePaths[1]} → {bytes2.Length} bytes");
        _out.WriteLine("✓ Two different battles produced different bytes.");
    }

    [Fact]
    public void ExtractByPath_UnknownPath_ThrowsFileNotFoundException()
    {
        SkipIfNoArchive();
        using var archive = UnityArchiver.Open(ArchivePath);
        Assert.Throws<FileNotFoundException>(
            () => archive.ExtractByPath(
                @"\StreamingAssets\assets\does\not\exist\file.bytes"));
    }

    // ── 5. Full byte match — walks exported .bytes folder tree ────────────

    [Fact]
    public void Extract_AllExportedBytesFiles_FullByteMatchAgainstArchive()
    {
        SkipIfNoArchive();

        if (!Directory.Exists(BattleSceneRoot))
            throw new SkipException(
                $"Exported .bytes folder not found: {BattleSceneRoot}\n" +
                "Copy your battlescene .bytes files into TestData/StreamingAssets/ " +
                "preserving the full folder structure.");

        string[] bytesFiles = Directory.GetFiles(
            BattleSceneRoot, "dbfile0000.raw16.bytes", SearchOption.AllDirectories);

        if (bytesFiles.Length == 0)
            throw new SkipException($"No dbfile0000.raw16.bytes files found under {BattleSceneRoot}.");

        _out.WriteLine($"Found {bytesFiles.Length} .bytes files to validate.");

        using var archive = UnityArchiver.Open(ArchivePath);

        int matched = 0;
        int mismatches = 0;
        int skipped = 0;

        foreach (string filePath in bytesFiles)
        {
            string relativePath = filePath
                .Substring(TestDataDir.Length)
                .Replace('/', '\\');

            byte[] expected = File.ReadAllBytes(filePath);

            byte[] actual;
            try { actual = archive.ExtractByPath(relativePath); }
            catch (FileNotFoundException) { skipped++; continue; }

            // Compare up to shorter length; verify any tail bytes are zero-padding
            int compareLen = Math.Min(actual.Length, expected.Length);
            bool ok = true;

            for (int i = 0; i < compareLen; i++)
            {
                if (actual[i] != expected[i])
                {
                    _out.WriteLine(
                        $"MISMATCH {relativePath} offset={i} " +
                        $"archive=0x{actual[i]:X2} disk=0x{expected[i]:X2}");
                    mismatches++;
                    ok = false;
                    break;
                }
            }

            if (ok && actual.Length != expected.Length)
            {
                byte[] longer = actual.Length > expected.Length ? actual : expected;
                string longerLabel = actual.Length > expected.Length ? "archive" : "disk";
                for (int i = compareLen; i < longer.Length; i++)
                {
                    if (longer[i] != 0)
                    {
                        _out.WriteLine(
                            $"NON-ZERO TAIL in {longerLabel} at offset {i}: {relativePath} " +
                            $"value=0x{longer[i]:X2}");
                        mismatches++;
                        ok = false;
                        break;
                    }
                }
            }

            if (ok) matched++;
        }

        _out.WriteLine(
            $"Results: {matched} matched, {skipped} skipped, {mismatches} mismatches.");
        Assert.True(matched + mismatches > 0,
            "No files were successfully extracted from archive.");
        Assert.Equal(0, mismatches);
    }

    // ── 6. Cross-validate: archive vs disk .bytes produce same EnemyFile data ──

    [Fact]
    public void ExtractByPath_AllBattleFiles_ProduceSameEnemyFileAsDisks()
    {
        // Confirms that archive extraction and disk .bytes files produce
        // byte-identical content when parsed as EnemyFile instances.
        SkipIfNoArchive();

        if (!Directory.Exists(BattleSceneRoot))
            throw new SkipException($"BattleScene directory not found: {BattleSceneRoot}");

        string[] bytesFiles = Directory.GetFiles(
            BattleSceneRoot, "dbfile0000.raw16.bytes", SearchOption.AllDirectories);

        if (bytesFiles.Length == 0)
            throw new SkipException("No .bytes files found.");

        using var archive = UnityArchiver.Open(ArchivePath);

        int compared = 0;
        int failures = 0;

        foreach (string filePath in bytesFiles)
        {
            string relativePath = filePath
                .Substring(TestDataDir.Length)
                .Replace('/', '\\');

            byte[] diskBytes = File.ReadAllBytes(filePath);

            byte[] archiveBytes;
            try { archiveBytes = archive.ExtractByPath(relativePath); }
            catch (FileNotFoundException) { continue; }

            // Parse both as EnemyFile and compare drop/steal/card for each stat
            EnemyFile diskFile, archiveFile;
            try
            {
                diskFile = new EnemyFile(diskBytes, relativePath);
                archiveFile = new EnemyFile(archiveBytes, relativePath);
            }
            catch { continue; }

            if (diskFile.StatCount != archiveFile.StatCount)
            {
                _out.WriteLine($"STAT COUNT MISMATCH: {relativePath} " +
                               $"disk={diskFile.StatCount} archive={archiveFile.StatCount}");
                failures++;
                continue;
            }

            bool ok = true;
            for (int s = 0; s < diskFile.StatCount && ok; s++)
            {
                for (int slot = 0; slot < 4; slot++)
                {
                    if (diskFile.GetDrop(s, slot) != archiveFile.GetDrop(s, slot) ||
                        diskFile.GetSteal(s, slot) != archiveFile.GetSteal(s, slot))
                    {
                        _out.WriteLine($"ITEM MISMATCH stat{s}: {relativePath}");
                        failures++;
                        ok = false;
                        break;
                    }
                }
                if (ok && diskFile.GetCardDrop(s) != archiveFile.GetCardDrop(s))
                {
                    _out.WriteLine($"CARD MISMATCH stat{s}: {relativePath}");
                    failures++;
                    ok = false;
                }
            }

            if (ok) compared++;
        }

        _out.WriteLine($"Compared: {compared} files, failures: {failures}");
        Assert.Equal(0, failures);
        Assert.True(compared > 0, "No files were compared.");
    }

    // ── 7. Dispose safety ─────────────────────────────────────────────────

    [Fact]
    public void Extract_AfterDispose_ThrowsObjectDisposedException()
    {
        SkipIfNoArchive();
        var archive = UnityArchiver.Open(ArchivePath);
        archive.Dispose();
        Assert.Throws<ObjectDisposedException>(() => archive.Extract("anything"));
    }

    [Fact]
    public void GetFileNames_AfterDispose_ThrowsObjectDisposedException()
    {
        SkipIfNoArchive();
        var archive = UnityArchiver.Open(ArchivePath);
        archive.Dispose();
        Assert.Throws<ObjectDisposedException>(() => archive.GetFileNames());
    }

    [Fact]
    public void GetFullPaths_AfterDispose_ThrowsObjectDisposedException()
    {
        SkipIfNoArchive();
        var archive = UnityArchiver.Open(ArchivePath);
        archive.Dispose();
        Assert.Throws<ObjectDisposedException>(() => archive.GetFullPaths());
    }
}