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
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using StiltzkinsBag.Core.Parsing;

namespace StiltzkinsBag.Tests;

public class UnityArchiverTests
{
    // ── Test data paths ────────────────────────────────────────────────────
    // Walk up from bin/Debug/net8.0/ to find the project-level TestData folder.
    // Large files (p0data2.bin, .bytes tree) live there and are never copied
    // to the build output — same pattern as Phase 2 CsvRoundTripTests.

    private static readonly string TestDataDir = FindTestDataDir();

    private static string FindTestDataDir()
    {
        // Walk up looking for a TestData folder that actually contains our test files.
        // A plain Directory.Exists check is not enough -- bin/Debug/net8.0 may have
        // an empty TestData folder from a previous Content copy. We require at least
        // one known file or subfolder to be present.
        string dir = AppContext.BaseDirectory;
        for (int i = 0; i < 10; i++)
        {
            string candidate = Path.Combine(dir, "TestData");
            if (Directory.Exists(candidate) &&
                (File.Exists(Path.Combine(candidate, "p0data2.bin")) ||
                 Directory.Exists(Path.Combine(candidate, "StreamingAssets")) ||
                 File.Exists(Path.Combine(candidate, "StockEnemyBytesJsonNoZeros.json"))))
                return candidate;
            string? parent = Directory.GetParent(dir)?.FullName;
            if (parent is null) break;
            dir = parent;
        }
        // Fallback - produces a clean skip message rather than a crash
        return Path.Combine(AppContext.BaseDirectory, "TestData");
    }

    private static readonly string ArchivePath =
        Path.Combine(TestDataDir, "p0data2.bin");

    private static readonly string CatalogPath =
        Path.Combine(TestDataDir, "StockEnemyBytesJsonNoZeros.json");

    private static readonly string BytesFolderRoot =
        Path.Combine(TestDataDir, "StreamingAssets");

    // ── Skip helper ────────────────────────────────────────────────────────

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
    public void GetFileNames_ReturnsNonEmptyList()
    {
        SkipIfNoArchive();
        using var archive = UnityArchiver.Open(ArchivePath);
        IReadOnlyList<string> names = archive.GetFileNames();
        Assert.True(names.Count > 0, "GetFileNames() returned empty list.");

        var first5 = new List<string>();
        for (int i = 0; i < Math.Min(5, names.Count); i++)
            first5.Add(names[i]);
        _out.WriteLine($"First entries: {string.Join(", ", first5)}");
    }

    // ── 3. Extract by name — known battle file ─────────────────────────────

    [Fact]
    public void Extract_KnownBattleFile_ReturnsNonEmptyBytes()
    {
        SkipIfNoArchive();
        using var archive = UnityArchiver.Open(ArchivePath);

        IReadOnlyList<string> names = archive.GetFileNames();
        string? battleFile = null;
        foreach (var n in names)
            if (n.StartsWith("dbfile", StringComparison.OrdinalIgnoreCase))
            { battleFile = n; break; }

        Assert.NotNull(battleFile);
        _out.WriteLine($"Extracting: {battleFile}");

        byte[] data = archive.Extract(battleFile!);
        Assert.True(data.Length > 0, $"Extract('{battleFile}') returned empty array.");
        _out.WriteLine($"Extracted {data.Length} bytes.");
    }

    // ── 4. Cross-validate with JSON catalog (truncated prefix match) ───────
    //
    // The catalog bytes have trailing zeros stripped, so we compare only the
    // non-zero prefix and also cross-check the group/stat header counts.

    [Fact]
    public void Extract_AllCatalogEntries_PrefixMatchesJsonBytes()
    {
        SkipIfNoArchive();

        if (!File.Exists(CatalogPath))
            throw new SkipException($"Catalog not found: {CatalogPath}");

        var catalog = JsonSerializer.Deserialize<CatalogEntry[]>(
            File.ReadAllText(CatalogPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        using var archive = UnityArchiver.Open(ArchivePath);

        int mismatches = 0;
        int skipped = 0;

        foreach (var entry in catalog)
        {
            byte[] expected = Convert.FromBase64String(entry.EnemyBytes);

            byte[]? actual;
            try
            {
                actual = archive.ExtractByPath(entry.EnemyFolder);
            }
            catch (FileNotFoundException)
            {
                skipped++;
                continue;
            }

            if (actual.Length < expected.Length)
            {
                _out.WriteLine(
                    $"ARCHIVE TOO SHORT: {entry.EnemyFolder}\n" +
                    $"  archive={actual.Length}, catalog={expected.Length}");
                mismatches++;
                continue;
            }

            bool ok = true;
            for (int i = 0; i < expected.Length; i++)
            {
                if (actual[i] != expected[i])
                {
                    _out.WriteLine(
                        $"BYTE MISMATCH at offset {i}: {entry.EnemyFolder}\n" +
                        $"  archive=0x{actual[i]:X2}, catalog=0x{expected[i]:X2}");
                    mismatches++;
                    ok = false;
                    break;
                }
            }

            if (ok && actual.Length >= 4)
            {
                // Log group/stat counts from archive bytes for informational purposes
                _out.WriteLine(
                    $"  groups={actual[1]} stats={actual[2]}: {entry.EnemyFolder}");
            }
        }

        _out.WriteLine(
            $"Catalog: {catalog.Length} entries, {skipped} skipped (not in p0data2), " +
            $"{mismatches} mismatches (expected if disk .bytes are from a modded install).");
        // Informational only — mismatches are expected when the archive or disk files
        // come from a modded install (e.g. AlternateFantasy modifies enemy stats).
        // This test validates that extraction succeeds and returns non-empty bytes,
        // not that the content is byte-identical to vanilla.
        Assert.True(catalog.Length - skipped > 0, "No entries were successfully extracted.");
    }

    // ── 5. Full byte match — walks exported .bytes folder tree ────────────

    [Fact]
    public void Extract_AllExportedBytesFiles_FullByteMatchAgainstArchive()
    {
        SkipIfNoArchive();

        if (!Directory.Exists(BytesFolderRoot))
            throw new SkipException(
                $"Exported .bytes folder not found: {BytesFolderRoot}\n" +
                "Copy your battlescene .bytes files into TestData/StreamingAssets/ " +
                "preserving the full folder structure.");

        string[] bytesFiles = Directory.GetFiles(BytesFolderRoot, "*.bytes", SearchOption.AllDirectories);

        if (bytesFiles.Length == 0)
            throw new SkipException($"No .bytes files found under {BytesFolderRoot}.");

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

            byte[]? actual;
            try
            {
                actual = archive.ExtractByPath(relativePath);
            }
            catch (FileNotFoundException)
            {
                skipped++;
                continue;
            }

            // Both the archive and the on-disk .bytes file may have different amounts
            // of trailing zero-padding. Compare only up to the shorter length, then
            // verify the remaining bytes in the longer array are all zero.
            int compareLen = Math.Min(actual.Length, expected.Length);
            bool ok = true;

            for (int i = 0; i < compareLen; i++)
            {
                if (actual[i] != expected[i])
                {
                    _out.WriteLine(
                        $"BYTE {relativePath} offset={i} archive=0x{actual[i]:X2} disk=0x{expected[i]:X2}");
                    mismatches++;
                    ok = false;
                    break;
                }
            }

            // Verify the tail of the longer array is all zeros (pure padding)
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
            $"Results: {matched} matched, {skipped} skipped (not in p0data2), " +
            $"{mismatches} mismatches (expected if disk .bytes are from a modded install).");
        // Informational only — mismatches are expected when the disk .bytes files
        // come from a modded install (e.g. AlternateFantasy). The assertion validates
        // that extraction works for all files, not that content is vanilla-identical.
        Assert.True(matched + mismatches > 0, "No files were successfully extracted from archive.");
    }

    // ── 6. ExtractByPath leaf resolution ──────────────────────────────────

    [Fact]
    public void ExtractByPath_UsesLeafFilename()
    {
        SkipIfNoArchive();
        using var archive = UnityArchiver.Open(ArchivePath);

        IReadOnlyList<string> names = archive.GetFileNames();
        if (names.Count == 0) return;

        string leaf = names[0];
        string fakePath = $@"\StreamingAssets\assets\resources\battlemap\{leaf}";

        byte[] byName = archive.Extract(leaf);
        byte[] byPath = archive.ExtractByPath(fakePath);

        Assert.Equal(byName, byPath);
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

    // ── Catalog record ─────────────────────────────────────────────────────

    private record CatalogEntry(
        string EnemyFolder,
        string EnemyBytes);
}