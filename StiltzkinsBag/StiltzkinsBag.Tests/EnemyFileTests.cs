// StiltzkinsBag.Tests/EnemyFileTests.cs
//
// Drop StockEnemyBytesJsonNoZeros.json into StiltzkinsBag.Tests/TestData/ before running.
// Set its Build Action to "Content" and Copy to Output Directory to "Copy if newer".

using System;
using System.IO;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;
using StiltzkinsBag.Core.Models.Battle;

namespace StiltzkinsBag.Tests;

public class EnemyFileTests
{
    // ── JSON catalog ───────────────────────────────────────────────────────

    private record EnemyCatalogEntry(
        string EnemyFolder,
        string EnemyBytes);

    private static readonly string JsonPath =
        Path.Combine(AppContext.BaseDirectory, "TestData", "StockEnemyBytesJsonNoZeros.json");

    private static readonly EnemyCatalogEntry[] Catalog = LoadCatalog();

    private static EnemyCatalogEntry[] LoadCatalog()
    {
        if (!File.Exists(JsonPath))
            throw new FileNotFoundException(
                $"Test data not found: {JsonPath}\n" +
                "Copy StockEnemyBytesJsonNoZeros.json into StiltzkinsBag.Tests/TestData/ " +
                "and set its Build Action to 'Content, Copy if newer'.",
                JsonPath);

        string json = File.ReadAllText(JsonPath);
        return JsonSerializer.Deserialize<EnemyCatalogEntry[]>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Catalog deserialized to null.");
    }

    private static EnemyFile FileFromEntry(EnemyCatalogEntry entry) =>
        new(Convert.FromBase64String(entry.EnemyBytes), entry.EnemyFolder);

    private readonly ITestOutputHelper _out;
    public EnemyFileTests(ITestOutputHelper output) => _out = output;

    // ── 1. Header parsing — all catalog entries ────────────────────────────

    [Fact]
    public void Header_GroupCount_IsPositiveForAllEntries()
    {
        foreach (var entry in Catalog)
        {
            var file = FileFromEntry(entry);
            Assert.True(file.GroupCount > 0,
                $"GroupCount must be > 0 for {entry.EnemyFolder}, got {file.GroupCount}");
        }
    }

    [Fact]
    public void Header_StatCount_IsPositiveForAllEntries()
    {
        foreach (var entry in Catalog)
        {
            var file = FileFromEntry(entry);
            Assert.True(file.StatCount > 0,
                $"StatCount must be > 0 for {entry.EnemyFolder}, got {file.StatCount}");
        }
    }

    [Fact]
    public void Header_Version_IsSevenForAllVanillaEntries()
    {
        foreach (var entry in Catalog)
        {
            byte[] raw = Convert.FromBase64String(entry.EnemyBytes);
            Assert.Equal(7, raw[0]);
        }
    }

    // ── 2. Offset math cross-validation ───────────────────────────────────
    //
    // v2.2 research: for 1 group, 1 stat →
    //   drop[0]  = 8 + 1*56 + 20 = 84
    //   steal[0] = 8 + 1*56 + 24 = 88
    //   blue     = 8 + 1*56 + 71 = 135
    //   card     = 8 + 1*56 + 105 = 169

    [Fact]
    public void OffsetMath_OneGroupOneStat_MatchesV22ResearchOffsets()
    {
        EnemyCatalogEntry? entry = null;
        foreach (var e in Catalog)
        {
            byte[] eb = Convert.FromBase64String(e.EnemyBytes);
            if (eb.Length >= 4 && eb[1] == 1 && eb[2] == 1) { entry = e; break; }
        }

        Assert.NotNull(entry);

        byte[] raw = Convert.FromBase64String(entry!.EnemyBytes);
        var file = new EnemyFile(raw, entry.EnemyFolder);

        Assert.Equal(raw[84], file.GetDrop(0, 0));
        Assert.Equal(raw[88], file.GetSteal(0, 0));
        Assert.Equal(raw[135], file.GetBlueMagic(0));
        Assert.Equal(raw[169], file.GetCardDrop(0));
    }

    [Fact]
    public void OffsetMath_SixGroupTwoStat_MatchesExpectedAbsoluteOffsets()
    {
        // Catalog[0]: evt_battle_ac_e028f — 6 groups, 2 stats
        // stat 0 base = 8 + 6*56 = 344
        // drop[0..3]  = 364–367, steal[0..3] = 368–371, blue = 415, card = 449
        var entry = Catalog[0];
        byte[] raw0 = Convert.FromBase64String(entry.EnemyBytes);
        Assert.Equal(6, raw0[1]);   // GroupCount from bytes
        Assert.Equal(2, raw0[2]);   // StatCount from bytes

        byte[] raw = Convert.FromBase64String(entry.EnemyBytes);
        var file = new EnemyFile(raw, entry.EnemyFolder);

        Assert.Equal(raw[364], file.GetDrop(0, 0));
        Assert.Equal(raw[365], file.GetDrop(0, 1));
        Assert.Equal(raw[366], file.GetDrop(0, 2));
        Assert.Equal(raw[367], file.GetDrop(0, 3));
        Assert.Equal(raw[368], file.GetSteal(0, 0));
        Assert.Equal(raw[369], file.GetSteal(0, 1));
        Assert.Equal(raw[370], file.GetSteal(0, 2));
        Assert.Equal(raw[371], file.GetSteal(0, 3));
        Assert.Equal(raw[415], file.GetBlueMagic(0));
        Assert.Equal(raw[449], file.GetCardDrop(0));
    }

    // ── 3. Round-trip write ────────────────────────────────────────────────

    [Fact]
    public void SetDrop_RoundTrip_AllSlots()
    {
        var file = FileFromEntry(Catalog[0]);
        for (int slot = 0; slot < 4; slot++)
        {
            byte original = file.GetDrop(0, slot);
            byte testVal = (byte)(original == 200 ? 42 : 200);
            file.SetDrop(0, slot, testVal);
            Assert.Equal(testVal, file.GetDrop(0, slot));
            file.SetDrop(0, slot, original);
        }
    }

    [Fact]
    public void SetSteal_RoundTrip_AllSlots()
    {
        var file = FileFromEntry(Catalog[0]);
        for (int slot = 0; slot < 4; slot++)
        {
            byte original = file.GetSteal(0, slot);
            byte testVal = (byte)(original == 200 ? 42 : 200);
            file.SetSteal(0, slot, testVal);
            Assert.Equal(testVal, file.GetSteal(0, slot));
            file.SetSteal(0, slot, original);
        }
    }

    [Fact]
    public void SetBlueMagic_RoundTrip()
    {
        var file = FileFromEntry(Catalog[0]);
        byte original = file.GetBlueMagic(0);
        byte testVal = (byte)(original == 200 ? 42 : 200);
        file.SetBlueMagic(0, testVal);
        Assert.Equal(testVal, file.GetBlueMagic(0));
        file.SetBlueMagic(0, original);
        Assert.Equal(original, file.GetBlueMagic(0));
    }

    [Fact]
    public void SetCardDrop_RoundTrip()
    {
        var file = FileFromEntry(Catalog[0]);
        byte original = file.GetCardDrop(0);
        byte testVal = (byte)(original == 200 ? 42 : 200);
        file.SetCardDrop(0, testVal);
        Assert.Equal(testVal, file.GetCardDrop(0));
        file.SetCardDrop(0, original);
        Assert.Equal(original, file.GetCardDrop(0));
    }

    // ── 4. Multi-stat indexing ─────────────────────────────────────────────

    [Fact]
    public void StatIndex_MultiStat_ReadsFromCorrectBlock()
    {
        // Stat 0 drop[0] = raw[364], stat 1 drop[0] = raw[344 + 116 + 20] = raw[480]
        var entry = Catalog[0];
        byte[] raw = Convert.FromBase64String(entry.EnemyBytes);
        var file = new EnemyFile(raw, entry.EnemyFolder);

        Assert.Equal(raw[364], file.GetDrop(0, 0));
        Assert.Equal(raw[480], file.GetDrop(1, 0));
    }

    [Fact]
    public void SetDrop_StatOne_DoesNotCorruptStatZero()
    {
        var file = FileFromEntry(Catalog[0]);
        byte stat0Drop = file.GetDrop(0, 0);
        byte newVal = (byte)(file.GetDrop(1, 0) == 42 ? 99 : 42);

        file.SetDrop(1, 0, newVal);

        Assert.Equal(stat0Drop, file.GetDrop(0, 0));
        Assert.Equal(newVal, file.GetDrop(1, 0));
    }

    [Fact]
    public void GetAllFields_AllCatalogEntries_DoesNotThrow()
    {
        // Smoke test: every field in every stat block of every catalog entry is readable.
        foreach (var entry in Catalog)
        {
            var file = FileFromEntry(entry);
            for (int s = 0; s < file.StatCount; s++)
            {
                _ = file.GetDrop(s, 0);
                _ = file.GetDrop(s, 1);
                _ = file.GetDrop(s, 2);
                _ = file.GetDrop(s, 3);
                _ = file.GetSteal(s, 0);
                _ = file.GetSteal(s, 1);
                _ = file.GetSteal(s, 2);
                _ = file.GetSteal(s, 3);
                _ = file.GetBlueMagic(s);
                _ = file.GetCardDrop(s);
            }
        }
    }

    // ── 5. ToBytes() isolation ─────────────────────────────────────────────

    [Fact]
    public void ToBytes_ReflectsEditsAndIsIsolatedFromFurtherChanges()
    {
        var file = FileFromEntry(Catalog[0]);
        file.SetDrop(0, 0, 42);
        byte[] snapshot = file.ToBytes();

        Assert.Equal(42, snapshot[364]); // snapshot captured the edit

        file.SetDrop(0, 0, 99);          // further edit after snapshot
        Assert.Equal(42, snapshot[364]); // snapshot must be unaffected
    }

    [Fact]
    public void Constructor_TakesDefensiveCopy_CallerMutationDoesNotAffectFile()
    {
        byte[] raw = Convert.FromBase64String(Catalog[0].EnemyBytes);
        var file = new EnemyFile(raw, Catalog[0].EnemyFolder);
        byte before = file.GetDrop(0, 0);

        raw[364] = (byte)(before == 0 ? 99 : 0); // mutate caller's array

        Assert.Equal(before, file.GetDrop(0, 0));
    }

    // ── 6. Error handling ──────────────────────────────────────────────────

    [Fact]
    public void Constructor_Version8_ThrowsNotSupportedException()
    {
        byte[] raw = Convert.FromBase64String(Catalog[0].EnemyBytes);
        raw[0] = 8;
        Assert.Throws<NotSupportedException>(() => new EnemyFile(raw));
    }

    [Fact]
    public void Constructor_TruncatedData_ThrowsArgumentException()
    {
        byte[] raw = Convert.FromBase64String(Catalog[0].EnemyBytes);
        Assert.Throws<ArgumentException>(() => new EnemyFile(raw[..50]));
    }

    [Fact]
    public void Constructor_TooShortForHeader_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new EnemyFile(new byte[4]));
    }

    [Fact]
    public void GetDrop_OutOfRangeStatIndex_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FileFromEntry(Catalog[0]).GetDrop(99, 0));
    }

    [Fact]
    public void GetDrop_OutOfRangeSlot_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FileFromEntry(Catalog[0]).GetDrop(0, 4));
    }

    [Fact]
    public void GetSteal_NegativeSlot_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FileFromEntry(Catalog[0]).GetSteal(0, -1));
    }

    [Fact]
    public void SetDrop_OutOfRangeStatIndex_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FileFromEntry(Catalog[0]).SetDrop(99, 0, 1));
    }

    [Fact]
    public void GetBlueMagic_NegativeStatIndex_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FileFromEntry(Catalog[0]).GetBlueMagic(-1));
    }
}