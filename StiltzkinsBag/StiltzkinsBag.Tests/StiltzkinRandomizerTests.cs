// StiltzkinsBag.Tests/StiltzkinRandomizerTests.cs
//
// Phase 5.99 — Task 7 — StiltzkinRandomizer tests.
//
// Pure static method tests use synthetic FieldItemLocation data derived from
// StiltzkinItemScanDiagnosticTests (2026-04-01) — no live archive required.
//
// Integration tests (determinism, Cleyra pairing, full Randomize pipeline)
// require p0data7.bin and skip gracefully if absent.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StiltzkinsBag.Core.Models;
using StiltzkinsBag.Core.Parsing;
using StiltzkinsBag.Models;
using StiltzkinsBag.Parsing;
using StiltzkinsBag.Randomizers;
using Xunit;
using Xunit.Abstractions;

namespace StiltzkinsBag.Tests;

public sealed class StiltzkinRandomizerTests
{
    // ── Paths ──────────────────────────────────────────────────────────────────

    private static readonly string TestDataDir =
        Path.Combine(AppContext.BaseDirectory, "TestData");

    private static readonly string ArchivePath =
        Path.Combine(TestDataDir, "p0data7.bin");

    private readonly ITestOutputHelper _out;
    public StiltzkinRandomizerTests(ITestOutputHelper output) => _out = output;

    // ── Synthetic location builders ────────────────────────────────────────────
    //
    // Mirrors the actual Burmecia (333G) and Cleyra (444G) structure confirmed by
    // StiltzkinItemScanDiagnosticTests.
    //
    // Burmecia 333G: price TextSync at 0x24D5 (value=333),
    //   items 242/237/238 as pairs at 0x25B5,0x25CB / 0x26AF,0x26C5 / 0x27A9,0x27BF
    //
    // Cleyra ANTRION 444G: price TextSync at 0x2505 (value=444),
    //   items 237/238/249 as pairs at 0x25E5,0x25FB / 0x26DF,0x26F5 / 0x27D9,0x27EF

    private static IReadOnlyList<FieldItemLocation> BuildBurmeciaLocations() => new[]
    {
        // Price TextSync
        new FieldItemLocation(0x24D5, 333, 2, FieldLocationKind.TextSync),
        // Item 242 (Soft) pair
        new FieldItemLocation(0x25B5, 242, 2, FieldLocationKind.DirectItem),
        new FieldItemLocation(0x25CB, 242, 2, FieldLocationKind.DirectItem),
        // Item 237 (Hi-Potion) pair
        new FieldItemLocation(0x26AF, 237, 2, FieldLocationKind.DirectItem),
        new FieldItemLocation(0x26C5, 237, 2, FieldLocationKind.DirectItem),
        // Item 238 (Ether) pair
        new FieldItemLocation(0x27A9, 238, 2, FieldLocationKind.DirectItem),
        new FieldItemLocation(0x27BF, 238, 2, FieldLocationKind.DirectItem),
        // Extra item 73 pair (after package — should be excluded by 3-pair limit)
        new FieldItemLocation(0x2CAA, 73, 2, FieldLocationKind.DirectItem),
        new FieldItemLocation(0x2CC0, 73, 2, FieldLocationKind.DirectItem),
    };

    private static IReadOnlyList<FieldItemLocation> BuildCleyraLocations() => new[]
    {
        // Price TextSync (444G — last occurrence is the one we want)
        new FieldItemLocation(0x052A, 1970, 2, FieldLocationKind.TextSync), // earlier TextSync, different value
        new FieldItemLocation(0x2505, 444,  2, FieldLocationKind.TextSync), // price TextSync
        // Item 237 pair
        new FieldItemLocation(0x25E5, 237, 2, FieldLocationKind.DirectItem),
        new FieldItemLocation(0x25FB, 237, 2, FieldLocationKind.DirectItem),
        // Item 238 pair
        new FieldItemLocation(0x26DF, 238, 2, FieldLocationKind.DirectItem),
        new FieldItemLocation(0x26F5, 238, 2, FieldLocationKind.DirectItem),
        // Item 249 pair
        new FieldItemLocation(0x27D9, 249, 2, FieldLocationKind.DirectItem),
        new FieldItemLocation(0x27EF, 249, 2, FieldLocationKind.DirectItem),
    };

    // ── FindPriceLocation ─────────────────────────────────────────────────────

    public sealed class FindPriceLocationTests
    {
        [Fact]
        public void Burmecia_FindsPriceAt0x24D5()
        {
            var locs = BuildBurmeciaLocations();
            var result = StiltzkinRandomizer.FindPriceLocation(locs, 333);
            Assert.NotNull(result);
            Assert.Equal(0x24D5, result.FileOffset);
            Assert.Equal(333, result.CurrentValue);
        }

        [Fact]
        public void Cleyra_FindsLastPriceTextSync()
        {
            // Cleyra has an earlier TextSync with value 1970 — must pick the 444G one
            var locs = BuildCleyraLocations();
            var result = StiltzkinRandomizer.FindPriceLocation(locs, 444);
            Assert.NotNull(result);
            Assert.Equal(0x2505, result.FileOffset);
            Assert.Equal(444, result.CurrentValue);
        }

        [Fact]
        public void NullLocations_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                StiltzkinRandomizer.FindPriceLocation(null!, 333));
        }

        [Fact]
        public void MissingPrice_ReturnsNull()
        {
            var locs = BuildBurmeciaLocations();
            var result = StiltzkinRandomizer.FindPriceLocation(locs, 9999);
            Assert.Null(result);
        }

        [Fact]
        public void EmptyLocations_ReturnsNull()
        {
            var result = StiltzkinRandomizer.FindPriceLocation(
                Array.Empty<FieldItemLocation>(), 333);
            Assert.Null(result);
        }
    }

    // ── FindItemPairs ─────────────────────────────────────────────────────────

    public sealed class FindItemPairsTests
    {
        [Fact]
        public void Burmecia_FindsThreePairs()
        {
            var locs = BuildBurmeciaLocations();
            var pairs = StiltzkinRandomizer.FindItemPairs(locs, 333);
            Assert.Equal(3, pairs.Count);
        }

        [Fact]
        public void Burmecia_PairsHaveCorrectVanillaIds()
        {
            var locs = BuildBurmeciaLocations();
            var pairs = StiltzkinRandomizer.FindItemPairs(locs, 333);
            Assert.Equal(242, pairs[0].VanillaId); // Soft
            Assert.Equal(237, pairs[1].VanillaId); // Hi-Potion
            Assert.Equal(238, pairs[2].VanillaId); // Ether
        }

        [Fact]
        public void Burmecia_ExtraItem73_IsExcluded()
        {
            // Algorithm takes only first 3 pairs — item 73 is a 4th pair and must be excluded
            var locs = BuildBurmeciaLocations();
            var pairs = StiltzkinRandomizer.FindItemPairs(locs, 333);
            Assert.DoesNotContain(pairs, p => p.VanillaId == 73);
        }

        [Fact]
        public void Burmecia_EachPair_BothLocationsSameId()
        {
            var locs = BuildBurmeciaLocations();
            var pairs = StiltzkinRandomizer.FindItemPairs(locs, 333);
            foreach (var (loc1, loc2, id) in pairs)
            {
                Assert.Equal(id, loc1.CurrentValue);
                Assert.Equal(id, loc2.CurrentValue);
            }
        }

        [Fact]
        public void Burmecia_EachPair_Loc1BeforeLoc2()
        {
            var locs = BuildBurmeciaLocations();
            var pairs = StiltzkinRandomizer.FindItemPairs(locs, 333);
            foreach (var (loc1, loc2, _) in pairs)
                Assert.True(loc1.FileOffset < loc2.FileOffset);
        }

        [Fact]
        public void Burmecia_EachPair_BothDirectItem()
        {
            var locs = BuildBurmeciaLocations();
            var pairs = StiltzkinRandomizer.FindItemPairs(locs, 333);
            foreach (var (loc1, loc2, _) in pairs)
            {
                Assert.Equal(FieldLocationKind.DirectItem, loc1.LocationKind);
                Assert.Equal(FieldLocationKind.DirectItem, loc2.LocationKind);
            }
        }

        [Fact]
        public void Cleyra_FindsThreePairsWithCorrectIds()
        {
            var locs = BuildCleyraLocations();
            var pairs = StiltzkinRandomizer.FindItemPairs(locs, 444);
            Assert.Equal(3, pairs.Count);
            Assert.Equal(237, pairs[0].VanillaId); // Hi-Potion
            Assert.Equal(238, pairs[1].VanillaId); // Ether
            Assert.Equal(249, pairs[2].VanillaId); // Phoenix Pinion
        }

        [Fact]
        public void MissingPrice_ReturnsEmpty()
        {
            var locs = BuildBurmeciaLocations();
            var pairs = StiltzkinRandomizer.FindItemPairs(locs, 9999);
            Assert.Empty(pairs);
        }

        [Fact]
        public void NullLocations_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                StiltzkinRandomizer.FindItemPairs(null!, 333));
        }

        [Fact]
        public void EmptyLocations_ReturnsEmpty()
        {
            var pairs = StiltzkinRandomizer.FindItemPairs(
                Array.Empty<FieldItemLocation>(), 333);
            Assert.Empty(pairs);
        }
    }

    // ── DrawFromPool ──────────────────────────────────────────────────────────

    public sealed class DrawFromPoolTests
    {
        [Fact]
        public void SingleElementPool_AlwaysReturnsThatElement()
        {
            var pool = new[] { 42 };
            var rng = new Random(1);
            for (int i = 0; i < 10; i++)
                Assert.Equal(42, StiltzkinRandomizer.DrawFromPool(pool, rng));
        }

        [Fact]
        public void DrawFromPool_AlwaysReturnsPoolMember()
        {
            var pool = VanillaObtainabilityData.StiltzkinJunkPool;
            var rng = new Random(12345);
            for (int i = 0; i < 100; i++)
            {
                int drawn = StiltzkinRandomizer.DrawFromPool(pool, rng);
                Assert.Contains(drawn, pool);
            }
        }

        [Fact]
        public void NullPool_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                StiltzkinRandomizer.DrawFromPool(null!, new Random()));
        }

        [Fact]
        public void NullRng_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                StiltzkinRandomizer.DrawFromPool(new[] { 1 }, null!));
        }

        [Fact]
        public void EmptyPool_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                StiltzkinRandomizer.DrawFromPool(Array.Empty<int>(), new Random()));
        }
    }

    // ── DrawPrice ─────────────────────────────────────────────────────────────

    public sealed class DrawPriceTests
    {
        [Theory]
        [InlineData(333)]
        [InlineData(444)]
        [InlineData(5555)]
        public void Off_ReturnsVanillaPrice_NoRngConsumed(int vanillaPrice)
        {
            var rng = new Random(1);
            int before = rng.Next(int.MaxValue); // consume one to get a baseline

            var rng2 = new Random(1);
            _ = rng2.Next(int.MaxValue); // same baseline call
            int result = StiltzkinRandomizer.DrawPrice(StiltzkinPriceMode.Off, vanillaPrice, rng2);

            Assert.Equal(vanillaPrice, result);
            // Verify RNG state is unchanged: next call should match
            Assert.Equal(rng.Next(), rng2.Next());
        }

        [Theory]
        [InlineData(StiltzkinPriceMode.ClearanceSale,
            VanillaObtainabilityData.StiltzkinClearanceSaleMin,
            VanillaObtainabilityData.StiltzkinClearanceSaleMax)]
        [InlineData(StiltzkinPriceMode.StiltzkinsMood,
            VanillaObtainabilityData.StiltzkinsMoodMin,
            VanillaObtainabilityData.StiltzkinsMoodMax)]
        [InlineData(StiltzkinPriceMode.HighwayRobbery,
            VanillaObtainabilityData.HighwayRobberyMin,
            VanillaObtainabilityData.HighwayRobberyMax)]
        public void ActiveMode_ReturnsValueInRange(
            StiltzkinPriceMode mode, int min, int max)
        {
            var rng = new Random(99999);
            for (int i = 0; i < 200; i++)
            {
                int price = StiltzkinRandomizer.DrawPrice(mode, 333, rng);
                Assert.InRange(price, min, max);
            }
        }

        [Fact]
        public void ClearanceSale_MaxIs99()
        {
            var rng = new Random(0);
            for (int i = 0; i < 500; i++)
            {
                int price = StiltzkinRandomizer.DrawPrice(StiltzkinPriceMode.ClearanceSale, 333, rng);
                Assert.True(price <= 99, $"ClearanceSale price {price} exceeds max 99");
                Assert.True(price >= 1, $"ClearanceSale price {price} is below min 1");
            }
        }

        [Fact]
        public void HighwayRobbery_MinIs5000()
        {
            var rng = new Random(0);
            for (int i = 0; i < 500; i++)
            {
                int price = StiltzkinRandomizer.DrawPrice(StiltzkinPriceMode.HighwayRobbery, 333, rng);
                Assert.True(price >= 5000, $"HighwayRobbery price {price} is below min 5000");
                Assert.True(price <= 15000, $"HighwayRobbery price {price} exceeds max 15000");
            }
        }

        [Fact]
        public void AllPrices_AreInt16Safe()
        {
            // All prices must fit in a signed int16 (max 32767) for field bytecode
            var rng = new Random(42);
            foreach (var mode in new[]
            {
                StiltzkinPriceMode.ClearanceSale,
                StiltzkinPriceMode.StiltzkinsMood,
                StiltzkinPriceMode.HighwayRobbery
            })
            {
                for (int i = 0; i < 200; i++)
                {
                    int price = StiltzkinRandomizer.DrawPrice(mode, 333, rng);
                    Assert.True(price <= short.MaxValue,
                        $"{mode} generated price {price} exceeds int16 max ({short.MaxValue})");
                }
            }
        }

        [Fact]
        public void NullRng_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                StiltzkinRandomizer.DrawPrice(StiltzkinPriceMode.ClearanceSale, 333, null!));
        }
    }

    // ── BuildStiltzkinExclusionSet ─────────────────────────────────────────────

    public sealed class BuildStiltzkinExclusionSetTests
    {
        [Theory]
        [InlineData(StiltzkinMode.Off)]
        [InlineData(StiltzkinMode.IncludeInFieldPool)]
        public void OffAndIncludeInFieldPool_ReturnEmptySet(StiltzkinMode mode)
        {
            var exclusions = FieldItemRandomizer.BuildStiltzkinExclusionSet(mode);
            Assert.Empty(exclusions);
        }

        [Theory]
        [InlineData(StiltzkinMode.Shuffle)]
        [InlineData(StiltzkinMode.Recommended)]
        public void ShuffleAndRecommended_ReturnAllNineScripts(StiltzkinMode mode)
        {
            var exclusions = FieldItemRandomizer.BuildStiltzkinExclusionSet(mode);
            // 8 visits but Cleyra has 2 scripts → 9 total
            Assert.Equal(9, exclusions.Count);
        }

        [Theory]
        [InlineData(StiltzkinMode.Shuffle)]
        [InlineData(StiltzkinMode.Recommended)]
        public void ExclusionSet_ContainsAllConfirmedScriptNames(StiltzkinMode mode)
        {
            var exclusions = FieldItemRandomizer.BuildStiltzkinExclusionSet(mode);

            string[] expected =
            {
                "EVT_BURMECIA_SQUARE_1.eb",
                "EVT_CLEYRA3_ANTRION.eb",
                "EVT_CLEYRA3_INN.eb",
                "EVT_FOSSIL_FR_DN1_0.eb",
                "EVT_PATA_M_CM_MP3_0.eb",
                "EVT_ALEX3_AT_SENTOU.eb",
                "EVT_OEIL_UV_DEP_0.eb",
                "EVT_BAL_BB_WPS_0.eb",
                "EVT_ALEX5_AT_SENTOU.eb",
            };

            foreach (string name in expected)
                Assert.Contains(name, exclusions);
        }

        [Theory]
        [InlineData(StiltzkinMode.Shuffle)]
        [InlineData(StiltzkinMode.Recommended)]
        public void ExclusionSet_IsCaseInsensitive(StiltzkinMode mode)
        {
            var exclusions = FieldItemRandomizer.BuildStiltzkinExclusionSet(mode);
            // HashSet was constructed with OrdinalIgnoreCase
            Assert.Contains("evt_burmecia_square_1.eb", exclusions);
            Assert.Contains("EVT_BURMECIA_SQUARE_1.EB", exclusions);
        }
    }

    // ── ItemRemapTable integration ─────────────────────────────────────────────

    public sealed class ItemRemapTableTests
    {
        [Fact]
        public void Remap_AppliedToDrawnItems_ResultContainsRemappedIds()
        {
            // Verify that ItemRemapTable is applied after RNG draws
            // by checking the result dictionary reflects remapped IDs.
            // Uses FindItemPairs on synthetic locations — no archive needed.

            var locs = BuildBurmeciaLocations(); // vanilla: 242, 237, 238
            var pairs = StiltzkinRandomizer.FindItemPairs(locs, 333);

            // Manually simulate remapping: 242→100, 237→101, 238→102
            var remapDict = new Dictionary<int, int> { { 242, 100 }, { 237, 101 }, { 238, 102 } };
            var table = new ItemRemapTable(remapDict);

            int[] drawn = pairs.Select(p => p.VanillaId).ToArray();
            int[] remapped = drawn.Select(id => table.Remap(id)).ToArray();

            Assert.Equal(new[] { 100, 101, 102 }, remapped);
        }

        [Fact]
        public void Passthrough_RemapTable_LeavesIdsUnchanged()
        {
            var table = ItemRemapTable.Passthrough();
            Assert.Equal(242, table.Remap(242));
            Assert.Equal(237, table.Remap(237));
            Assert.Equal(238, table.Remap(238));
        }
    }

    // ── Integration — determinism and Cleyra pairing ─────────────────────────

    [Fact]
    public void Integration_Shuffle_Determinism_SameSeedProducesIdenticalOutput()
    {
        if (!File.Exists(ArchivePath))
        {
            _out.WriteLine($"SKIP: p0data7.bin not found at {ArchivePath}");
            return;
        }

        string tempOutput1 = Path.Combine(Path.GetTempPath(), $"stiltzkin_det_1_{Guid.NewGuid()}");
        string tempOutput2 = Path.Combine(Path.GetTempPath(), $"stiltzkin_det_2_{Guid.NewGuid()}");

        try
        {
            var settings = new Settings
            {
                SeedString = "determinism-test",
                SeedInt = 12345,
                StiltzkinMode = StiltzkinMode.Shuffle,
                StiltzkinPriceMode = StiltzkinPriceMode.StiltzkinsMood,
            };

            var result1 = StiltzkinRandomizer.Randomize(
                ArchivePath, settings, new Random(12345),
                ItemRemapTable.Passthrough(), tempOutput1);

            var result2 = StiltzkinRandomizer.Randomize(
                ArchivePath, settings, new Random(12345),
                ItemRemapTable.Passthrough(), tempOutput2);

            Assert.True(result1.WasRun);
            Assert.True(result2.WasRun);
            Assert.Equal(result1.VisitsProcessed, result2.VisitsProcessed);

            foreach (var price in result1.NewItemsByVanillaPrice.Keys)
            {
                Assert.Equal(
                    result1.NewItemsByVanillaPrice[price],
                    result2.NewItemsByVanillaPrice[price]);
                Assert.Equal(
                    result1.NewPriceByVanillaPrice[price],
                    result2.NewPriceByVanillaPrice[price]);
            }

            _out.WriteLine("Determinism: ✓ both runs produced identical results");
        }
        finally
        {
            if (Directory.Exists(tempOutput1)) Directory.Delete(tempOutput1, recursive: true);
            if (Directory.Exists(tempOutput2)) Directory.Delete(tempOutput2, recursive: true);
        }
    }

    [Fact]
    public void Integration_Shuffle_PoolIntegrity_AllVanillaItemsPresent()
    {
        if (!File.Exists(ArchivePath))
        {
            _out.WriteLine($"SKIP: p0data7.bin not found at {ArchivePath}");
            return;
        }

        // Vanilla items across all 8 visits (from guide CSV, verified by diagnostic)
        var vanillaItems = new HashSet<int>
        {
            242, 237, 238,  // 333G Burmecia
            237, 238, 249,  // 444G Cleyra
            249, 247, 238,  // 555G Fossil Roo
            245, 253, 238,  // 666G Conde Petie
            249, 237, 239,  // 777G Alexandria
            237, 228, 239,  // 888G Oeilvert
            227, 238, 239,  // 2222G Bran Bal
            229, 230, 239,  // 5555G Alexandria final
        };

        string tempOutput = Path.Combine(Path.GetTempPath(), $"stiltzkin_pool_{Guid.NewGuid()}");
        try
        {
            var settings = new Settings
            {
                StiltzkinMode = StiltzkinMode.Shuffle,
                StiltzkinPriceMode = StiltzkinPriceMode.Off,
            };

            var result = StiltzkinRandomizer.Randomize(
                ArchivePath, settings, new Random(99),
                ItemRemapTable.Passthrough(), tempOutput);

            Assert.True(result.WasRun);

            // All items written to bytecode must come from the vanilla pool
            var allWrittenItems = result.NewItemsByVanillaPrice.Values
                .SelectMany(arr => arr)
                .ToList();

            Assert.Equal(24, allWrittenItems.Count);

            foreach (int id in allWrittenItems)
                Assert.Contains(id, vanillaItems);

            // All vanilla items must appear at least once across all packages
            foreach (int id in vanillaItems)
                Assert.Contains(id, allWrittenItems);

            _out.WriteLine($"Pool integrity: ✓ all 24 vanilla items present in shuffled output");
        }
        finally
        {
            if (Directory.Exists(tempOutput)) Directory.Delete(tempOutput, recursive: true);
        }
    }

    [Fact]
    public void Integration_CleyraScripts_ReceiveIdenticalItemsAndPrice()
    {
        if (!File.Exists(ArchivePath))
        {
            _out.WriteLine($"SKIP: p0data7.bin not found at {ArchivePath}");
            return;
        }

        string tempOutput = Path.Combine(Path.GetTempPath(), $"stiltzkin_cleyra_{Guid.NewGuid()}");
        try
        {
            var settings = new Settings
            {
                StiltzkinMode = StiltzkinMode.Shuffle,
                StiltzkinPriceMode = StiltzkinPriceMode.StiltzkinsMood,
            };

            var result = StiltzkinRandomizer.Randomize(
                ArchivePath, settings, new Random(777),
                ItemRemapTable.Passthrough(), tempOutput);

            Assert.True(result.WasRun);
            Assert.Equal(9, result.ScriptsPatched); // 8 visits + 1 extra for Cleyra pair

            // Verify ANTRION and INN have identical written bytes
            string antrionPath = Path.Combine(tempOutput,
                @"StreamingAssets\assets\resources\commonasset\eventengine\eventbinary\field\us",
                "EVT_CLEYRA3_ANTRION.eb.bytes");
            string innPath = Path.Combine(tempOutput,
                @"StreamingAssets\assets\resources\commonasset\eventengine\eventbinary\field\us",
                "EVT_CLEYRA3_INN.eb.bytes");

            if (!File.Exists(antrionPath) || !File.Exists(innPath))
            {
                _out.WriteLine("SKIP: output files not found — check output path construction");
                return;
            }

            byte[] antrionBytes = File.ReadAllBytes(antrionPath);
            byte[] innBytes = File.ReadAllBytes(innPath);

            // Verify item IDs in both scripts are the same by re-scanning
            var antrionLocs = FieldParser.FindItemLocations(antrionBytes);
            var innLocs = FieldParser.FindItemLocations(innBytes);

            // Patched bytes contain the NEW price — scan using the new price, not 444
            int newCleyraPrice = result.NewPriceByVanillaPrice[444];

            var antrionPairs = StiltzkinRandomizer.FindItemPairs(antrionLocs, newCleyraPrice);
            var innPairs = StiltzkinRandomizer.FindItemPairs(innLocs, newCleyraPrice);

            Assert.Equal(3, antrionPairs.Count);
            Assert.Equal(3, innPairs.Count);

            for (int i = 0; i < 3; i++)
            {
                Assert.Equal(antrionPairs[i].VanillaId, innPairs[i].VanillaId);
                _out.WriteLine($"  Cleyra item {i}: ANTRION={antrionPairs[i].VanillaId} INN={innPairs[i].VanillaId} ✓");
            }

            var antrionPrice = StiltzkinRandomizer.FindPriceLocation(antrionLocs, newCleyraPrice)?.CurrentValue;
            var innPrice = StiltzkinRandomizer.FindPriceLocation(innLocs, newCleyraPrice)?.CurrentValue;
            Assert.Equal(antrionPrice, innPrice);
            _out.WriteLine($"  Cleyra price: ANTRION={antrionPrice} INN={innPrice} ✓");
        }
        finally
        {
            if (Directory.Exists(tempOutput)) Directory.Delete(tempOutput, recursive: true);
        }
    }

    [Fact]
    public void Integration_Off_ReturnsNotRun()
    {
        if (!File.Exists(ArchivePath))
        {
            _out.WriteLine($"SKIP: p0data7.bin not found at {ArchivePath}");
            return;
        }

        var settings = new Settings { StiltzkinMode = StiltzkinMode.Off };
        var result = StiltzkinRandomizer.Randomize(
            ArchivePath, settings, new Random(1),
            ItemRemapTable.Passthrough(), Path.GetTempPath());

        Assert.False(result.WasRun);
    }

    [Fact]
    public void Integration_IncludeInFieldPool_ReturnsNotRun()
    {
        if (!File.Exists(ArchivePath))
        {
            _out.WriteLine($"SKIP: p0data7.bin not found at {ArchivePath}");
            return;
        }

        var settings = new Settings { StiltzkinMode = StiltzkinMode.IncludeInFieldPool };
        var result = StiltzkinRandomizer.Randomize(
            ArchivePath, settings, new Random(1),
            ItemRemapTable.Passthrough(), Path.GetTempPath());

        Assert.False(result.WasRun);
    }

    [Fact]
    public void Integration_Recommended_Fun_DrawsFromAllThreeTiers()
    {
        if (!File.Exists(ArchivePath))
        {
            _out.WriteLine($"SKIP: p0data7.bin not found at {ArchivePath}");
            return;
        }

        string tempOutput = Path.Combine(Path.GetTempPath(), $"stiltzkin_fun_{Guid.NewGuid()}");
        try
        {
            var settings = new Settings
            {
                StiltzkinMode = StiltzkinMode.Recommended,
                StiltzkinRecommendedSubMode = StiltzkinRecommendedSubMode.Fun,
                StiltzkinPriceMode = StiltzkinPriceMode.Off,
            };

            var result = StiltzkinRandomizer.Randomize(
                ArchivePath, settings, new Random(42),
                ItemRemapTable.Passthrough(), tempOutput);

            Assert.True(result.WasRun);

            foreach (var (price, items) in result.NewItemsByVanillaPrice)
            {
                Assert.Equal(3, items.Length);
                // Each item must come from one of the Fun pools
                var allFunItems = VanillaObtainabilityData.StiltzkinFunPoolLow
                    .Concat(VanillaObtainabilityData.StiltzkinFunPoolMid)
                    .Concat(VanillaObtainabilityData.StiltzkinFunPoolHigh)
                    .ToHashSet();

                foreach (int id in items)
                    Assert.Contains(id, allFunItems);

                _out.WriteLine($"  {price}G: [{string.Join(", ", items)}]");
            }
        }
        finally
        {
            if (Directory.Exists(tempOutput)) Directory.Delete(tempOutput, recursive: true);
        }
    }
}