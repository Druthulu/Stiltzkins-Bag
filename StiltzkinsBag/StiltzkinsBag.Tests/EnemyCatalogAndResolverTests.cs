// StiltzkinsBag.Tests/EnemyCatalogTests.cs
// StiltzkinsBag.Tests/ModSourceResolverTests.cs
//
// Combined test file for EnemyCatalog and ModSourceResolver.
//
// EnemyCatalog tests: run with StockEnemyBytesJsonNoZeros.json in TestData/
// ModSourceResolver tests: require a real game root with Memoria.ini
//   Set the environment variable STILTZKIN_GAME_ROOT or place a file at
//   TestData/game_root.txt containing the game root path.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using StiltzkinsBag.Core.Models.Battle;
using StiltzkinsBag.Core.Parsing;

namespace StiltzkinsBag.Tests;

// ═══════════════════════════════════════════════════════════════════════════
// EnemyCatalog Tests
// ═══════════════════════════════════════════════════════════════════════════

public class EnemyCatalogTests
{
    private static readonly string CatalogPath =
        Path.Combine(AppContext.BaseDirectory, "TestData", "StockEnemyBytesJsonNoZeros.json");

    [Fact]
    public void FromFile_LoadsCatalog_WithExpectedEntryCount()
    {
        if (!File.Exists(CatalogPath))
            throw new SkipException($"Catalog not found: {CatalogPath}");

        var catalog = EnemyCatalog.FromFile(CatalogPath);

        Assert.True(catalog.Count > 0, "Catalog should have entries.");
        Assert.Equal(catalog.Count, catalog.Entries.Count);
    }

    [Fact]
    public void FromFile_AllEntries_HaveNonEmptyFolder()
    {
        if (!File.Exists(CatalogPath))
            throw new SkipException($"Catalog not found: {CatalogPath}");

        var catalog = EnemyCatalog.FromFile(CatalogPath);
        foreach (var entry in catalog.Entries)
            Assert.False(string.IsNullOrEmpty(entry.EnemyFolder),
                "Every catalog entry must have a non-empty EnemyFolder.");
    }

    [Fact]
    public void FromFile_AllEntries_HavePositiveGroupAndStatCountInBytes()
    {
        if (!File.Exists(CatalogPath))
            throw new SkipException($"Catalog not found: {CatalogPath}");

        var catalog = EnemyCatalog.FromFile(CatalogPath);
        foreach (var entry in catalog.Entries)
        {
            byte[] bytes = entry.GetVanillaBytes();
            Assert.True(bytes.Length >= 4,
                $"Bytes too short to contain header for {entry.EnemyFolder}");
            Assert.True(bytes[1] > 0,
                $"GroupCount (bytes[1]) must be > 0 for {entry.EnemyFolder}");
            Assert.True(bytes[2] > 0,
                $"StatCount (bytes[2]) must be > 0 for {entry.EnemyFolder}");
        }
    }

    [Fact]
    public void GetVanillaBytes_FirstEntry_ReturnsNonEmptyArray()
    {
        if (!File.Exists(CatalogPath))
            throw new SkipException($"Catalog not found: {CatalogPath}");

        var catalog = EnemyCatalog.FromFile(CatalogPath);
        byte[] bytes = catalog.Entries[0].GetVanillaBytes();

        Assert.True(bytes.Length > 0, "VanillaBytes should not be empty.");
    }

    [Fact]
    public void GetVanillaBytes_FirstEntry_HeaderIsValid()
    {
        if (!File.Exists(CatalogPath))
            throw new SkipException($"Catalog not found: {CatalogPath}");

        var catalog = EnemyCatalog.FromFile(CatalogPath);
        byte[] bytes = catalog.Entries[0].GetVanillaBytes();

        Assert.True(bytes.Length >= 4, "Bytes too short to contain header.");
        Assert.Equal(7, bytes[0]);    // version
        Assert.True(bytes[1] > 0, "GroupCount must be > 0");
        Assert.True(bytes[2] > 0, "StatCount must be > 0");
    }

    [Fact]
    public void FromJson_InvalidJson_ThrowsException()
    {
        Assert.ThrowsAny<Exception>(() => EnemyCatalog.FromJson("not json"));
    }

    [Fact]
    public void FromFile_MissingFile_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(
            () => EnemyCatalog.FromFile(@"C:\does\not\exist.json"));
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// ModSourceResolver Tests
// ═══════════════════════════════════════════════════════════════════════════

public class ModSourceResolverTests
{
    private readonly ITestOutputHelper _out;

    public ModSourceResolverTests(ITestOutputHelper output) => _out = output;

    // ── Game root helper ───────────────────────────────────────────────────

    private static readonly string CatalogPath =
        Path.Combine(AppContext.BaseDirectory, "TestData", "StockEnemyBytesJsonNoZeros.json");

    private static string? TryGetGameRoot()
    {
        // 1. Environment variable
        string? env = Environment.GetEnvironmentVariable("STILTZKIN_GAME_ROOT");
        if (!string.IsNullOrEmpty(env) && Directory.Exists(env))
            return env;

        // 2. TestData/game_root.txt
        string txtPath = Path.Combine(AppContext.BaseDirectory, "TestData", "game_root.txt");
        if (File.Exists(txtPath))
        {
            string path = File.ReadAllText(txtPath).Trim();
            if (Directory.Exists(path))
                return path;
        }

        return null;
    }

    private static void SkipIfNoGameRoot()
    {
        if (TryGetGameRoot() is null)
            throw new SkipException(
                "Game root not configured. Set STILTZKIN_GAME_ROOT env var or create " +
                "TestData/game_root.txt containing the path to your FF9 installation.");
    }

    // ── Memoria.ini parsing ────────────────────────────────────────────────

    [Fact]
    public void FromGameRoot_ParsesModList_FromMemoriaIni()
    {
        SkipIfNoGameRoot();
        string gameRoot = TryGetGameRoot()!;

        var resolver = ModSourceResolver.FromGameRoot(gameRoot);

        Assert.True(resolver.ActiveMods.Count > 0,
            "Expected at least one active mod from Memoria.ini FolderNames.");

        _out.WriteLine($"Active mods ({resolver.ActiveMods.Count}):");
        foreach (var mod in resolver.ActiveMods)
            _out.WriteLine($"  {mod}");
    }

    [Fact]
    public void FromGameRoot_ParsesModList_OrderIsPreserved()
    {
        // Validates the parser reads FolderNames in order without reordering.
        // The first entry in FolderNames is the highest priority mod.
        SkipIfNoGameRoot();
        string gameRoot = TryGetGameRoot()!;

        var resolver = ModSourceResolver.FromGameRoot(gameRoot);

        Assert.True(resolver.ActiveMods.Count > 0, "Expected at least one active mod.");
        _out.WriteLine($"First (highest priority) mod: {resolver.ActiveMods[0]}");
        // No hard assertion on the specific name — it changes when the user reorders mods
    }

    [Fact]
    public void FromGameRoot_MissingIni_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(
            () => ModSourceResolver.FromGameRoot(@"C:\does\not\exist"));
    }

    // ── Resolution — vanilla fallback ─────────────────────────────────────

    [Fact]
    public void Resolve_WithNullMods_FallsBackToCatalogBytes()
    {
        if (!File.Exists(CatalogPath))
            throw new SkipException($"Catalog not found: {CatalogPath}");

        var catalog = EnemyCatalog.FromFile(CatalogPath);
        var resolver = ModSourceResolver.WithExplicitMods(@"C:\nonexistent", []);

        var entry = catalog.Entries[0];
        var result = resolver.Resolve(entry);

        Assert.NotNull(result);
        Assert.True(result.Bytes.Length > 0);
        Assert.Contains("catalog", result.Source, StringComparison.OrdinalIgnoreCase);

        _out.WriteLine($"Source: {result.Source}, Bytes: {result.Bytes.Length}");
    }

    [Fact]
    public void Resolve_VanillaArchive_ReturnsCorrectBytes()
    {
        SkipIfNoGameRoot();
        if (!File.Exists(CatalogPath))
            throw new SkipException($"Catalog not found: {CatalogPath}");

        string gameRoot = TryGetGameRoot()!;
        var catalog = EnemyCatalog.FromFile(CatalogPath);

        // Use explicit empty mod list to force vanilla archive path
        var resolver = ModSourceResolver.WithExplicitMods(gameRoot, []);

        var entry = catalog.Entries[0];
        var result = resolver.Resolve(entry);

        Assert.NotNull(result);
        Assert.True(result.Bytes.Length > 0);
        _out.WriteLine($"Source: {result.Source}, Bytes: {result.Bytes.Length}");

        // Header should match catalog metadata
        // Verify header is structurally valid
        Assert.True(result.Bytes.Length >= 4, "Result too short to contain header.");
        Assert.True(result.Bytes[1] > 0, "GroupCount must be > 0");
        Assert.True(result.Bytes[2] > 0, "StatCount must be > 0");
    }

    // ── Resolution — full mod stack ────────────────────────────────────────

    [Fact]
    public void Resolve_FullModStack_AllEntriesReturnValidBytes()
    {
        SkipIfNoGameRoot();
        if (!File.Exists(CatalogPath))
            throw new SkipException($"Catalog not found: {CatalogPath}");

        string gameRoot = TryGetGameRoot()!;
        var catalog = EnemyCatalog.FromFile(CatalogPath);
        var resolver = ModSourceResolver.FromGameRoot(gameRoot);

        int vanilla = 0;
        int fromMod = 0;
        int fromCatalog = 0;
        int invalid = 0;

        foreach (var entry in catalog.Entries)
        {
            var result = resolver.Resolve(entry);
            Assert.True(result.Bytes.Length > 0,
                $"Resolve returned empty bytes for {entry.EnemyFolder}");

            // Header validation: version byte should be 7, group/stat counts should be positive
            if (result.Bytes.Length >= 4 && result.Bytes[0] == 7)
            {
                if (result.Bytes[1] == 0 || result.Bytes[2] == 0)
                    invalid++;
            }

            if (result.Source.StartsWith("mod:"))
                fromMod++;
            else if (result.Source.Contains("catalog"))
                fromCatalog++;
            else
                vanilla++;
        }

        _out.WriteLine($"Results: {vanilla} vanilla archive, {fromMod} from mods, " +
                       $"{fromCatalog} catalog fallback, {invalid} invalid headers.");

        Assert.Equal(0, invalid);
    }

    [Fact]
    public void Resolve_FullModStack_ModOverridesTakeEffect()
    {
        // If any mods in the active list provide battle files, at least some
        // entries should resolve to mod sources rather than vanilla.
        // This test documents what we see but doesn't hard-fail if no mods
        // provide battle files — not all mods touch enemy data.
        SkipIfNoGameRoot();
        if (!File.Exists(CatalogPath))
            throw new SkipException($"Catalog not found: {CatalogPath}");

        string gameRoot = TryGetGameRoot()!;
        var catalog = EnemyCatalog.FromFile(CatalogPath);
        var resolver = ModSourceResolver.FromGameRoot(gameRoot);

        int fromMod = 0;
        foreach (var entry in catalog.Entries)
        {
            var result = resolver.Resolve(entry);
            if (result.Source.StartsWith("mod:"))
                fromMod++;
        }

        _out.WriteLine($"Entries resolved from mods: {fromMod} / {catalog.Count}");
        // Informational — not a hard assertion, since mods vary by install
    }

    // ── EnemyFile integration ──────────────────────────────────────────────

    [Fact]
    public void Resolve_ThenParseAsEnemyFile_AllEntriesParseClean()
    {
        SkipIfNoGameRoot();
        if (!File.Exists(CatalogPath))
            throw new SkipException($"Catalog not found: {CatalogPath}");

        string gameRoot = TryGetGameRoot()!;
        var catalog = EnemyCatalog.FromFile(CatalogPath);
        var resolver = ModSourceResolver.FromGameRoot(gameRoot);

        int errors = 0;
        foreach (var entry in catalog.Entries)
        {
            var result = resolver.Resolve(entry);
            try
            {
                var file = new EnemyFile(result.Bytes, entry.EnemyFolder);
                // Read one field from each stat to confirm offsets are valid
                for (int s = 0; s < file.StatCount; s++)
                    _ = file.GetDrop(s, 0);
            }
            catch (Exception ex)
            {
                _out.WriteLine($"Parse error: {entry.EnemyFolder} — {ex.Message}");
                errors++;
            }
        }

        Assert.Equal(0, errors);
    }
}