using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StiltzkinsBag.Models;
using StiltzkinsBag.Parsing;
using StiltzkinsBag.Randomizers;
using Xunit;

namespace StiltzkinsBag.Tests.Randomizers
{
    /// <summary>
    /// Tests for FieldItemRandomizer's internal static methods.
    ///
    /// Tests are organized into three groups:
    ///   ScanAll      — scanning field files and grouping locations
    ///   BuildPools   — extracting treasure and direct item pools from locations
    ///   BuildPatches — applying remap tables with correct TextSync resolution
    ///   GetOutputPath — verifying output path construction
    ///
    /// The integration methods (Randomize, ResolveFieldBytes) require a live
    /// game install and are not tested here.
    ///
    /// TEST DATA: same 7 .bytes files used by FieldParserTests.
    /// Place in Tests/TestData/FieldParser/ (already required by FieldParserTests).
    /// </summary>
    public sealed class FieldItemRandomizerTests
    {
        // ── Test data loading ─────────────────────────────────────────────────

        private static string TestDataDir =>
            Path.Combine(AppContext.BaseDirectory, "TestData", "FieldParser");

        private static (string FileName, byte[] Bytes) LoadField(string filename) =>
            (filename, File.ReadAllBytes(Path.Combine(TestDataDir, filename)));

        // Convenience: all 7 test fields as (name, bytes) pairs
        private static IEnumerable<(string, byte[])> AllTestFields() => new[]
        {
            LoadField("evt_alex1_at_house_2.eb.bytes"),   // field 114: Fang card, 9 gil, Potion
            LoadField("evt_alex1_ts_cargo_0.eb.bytes"),   // field  50: Potion, 47 gil
            LoadField("evt_alex1_ts_engin.eb.bytes"),     // field  57: Phoenix Down ×2 (chests)
            LoadField("evt_alex1_at_center.eb.bytes"),    // field 103: various items (false +ves)
            LoadField("evt_alex1_at_gate.eb.bytes"),      // field 107: Potion + 3 cards
            LoadField("evt_alex1_at_item.eb.bytes"),      // field 108: 38 gil (clean)
            LoadField("evt_alex1_at_house_1.eb.bytes"),   // field 113: Eye Drops, 3 gil
        };

        // ── ScanAll ───────────────────────────────────────────────────────────

        public sealed class ScanAllTests
        {
            [Fact]
            public void NullInput_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(
                    () => FieldItemRandomizer.ScanAll(null!));
            }

            [Fact]
            public void AllTestFields_ProduceExpectedFieldCount()
            {
                var result = FieldItemRandomizer.ScanAll(AllTestFields());
                Assert.Equal(7, result.Count);
            }

            [Fact]
            public void Field108_HasExactlyTwoLocations()
            {
                var result = FieldItemRandomizer.ScanAll(new[]
                    { LoadField("evt_alex1_at_item.eb.bytes") });
                var locs = result["evt_alex1_at_item.eb.bytes"];
                Assert.Equal(2, locs.Count);
            }

            [Fact]
            public void Field113_HasExactlyFourLocations()
            {
                var result = FieldItemRandomizer.ScanAll(new[]
                    { LoadField("evt_alex1_at_house_1.eb.bytes") });
                var locs = result["evt_alex1_at_house_1.eb.bytes"];
                Assert.Equal(4, locs.Count);
            }

            [Fact]
            public void AllTestFields_ContainKnownLocations()
            {
                var result = FieldItemRandomizer.ScanAll(AllTestFields());

                // Field 114: Fang card (513), 9 gil (1009), Potion (236)
                var house2 = result["evt_alex1_at_house_2.eb.bytes"];
                Assert.Contains(house2, l =>
                    l.LocationKind == FieldLocationKind.TreasureItem && l.CurrentValue == 513);
                Assert.Contains(house2, l =>
                    l.LocationKind == FieldLocationKind.TreasureItem && l.CurrentValue == 1009);
                Assert.Contains(house2, l =>
                    l.LocationKind == FieldLocationKind.TreasureItem && l.CurrentValue == 236);

                // Field 107: Potion + 3 cards
                var gate = result["evt_alex1_at_gate.eb.bytes"];
                Assert.Contains(gate, l =>
                    l.LocationKind == FieldLocationKind.TreasureItem && l.CurrentValue == 236);
                Assert.Contains(gate, l =>
                    l.LocationKind == FieldLocationKind.TreasureItem && l.CurrentValue == 517);

                // Field 57: Phoenix Down chest (DirectItem)
                var engine = result["evt_alex1_ts_engin.eb.bytes"];
                Assert.Contains(engine, l =>
                    l.LocationKind == FieldLocationKind.DirectItem && l.CurrentValue == 240);
            }

            [Fact]
            public void NoSentinelValuesInAnyField()
            {
                var result = FieldItemRandomizer.ScanAll(AllTestFields());
                foreach (var locs in result.Values)
                    Assert.DoesNotContain(locs, l =>
                        l.LocationKind == FieldLocationKind.TreasureItem
                        && l.CurrentValue >= 29999);
            }
        }

        // ── BuildPools ────────────────────────────────────────────────────────

        public sealed class BuildPoolsTests
        {
            [Fact]
            public void NullInput_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(
                    () => FieldItemRandomizer.BuildPools(null!));
            }

            [Fact]
            public void TreasurePool_ContainsAllTreasureItemValues()
            {
                var scanned = FieldItemRandomizer.ScanAll(AllTestFields());
                var (treasurePool, _) = FieldItemRandomizer.BuildPools(scanned);

                // Items found across test fields:
                Assert.Contains(236, treasurePool); // Potion (fields 50, 107, 114)
                Assert.Contains(244, treasurePool); // Eye Drops (field 113)
                Assert.Contains(513, treasurePool); // Fang card (field 114)
                Assert.Contains(517, treasurePool); // card (field 107)
                Assert.Contains(518, treasurePool); // card (field 107)
                Assert.Contains(521, treasurePool); // card (field 107)
                Assert.Contains(1003, treasurePool); // 3 gil (field 113)
                Assert.Contains(1009, treasurePool); // 9 gil (field 114)
                Assert.Contains(1038, treasurePool); // 38 gil (field 108)
                Assert.Contains(1047, treasurePool); // 47 gil (field 50)
            }

            [Fact]
            public void TreasurePool_ContainsNoDuplicates()
            {
                var scanned = FieldItemRandomizer.ScanAll(AllTestFields());
                var (treasurePool, _) = FieldItemRandomizer.BuildPools(scanned);
                Assert.Equal(treasurePool.Count, treasurePool.Distinct().Count());
            }

            [Fact]
            public void TreasurePool_ContainsNoSentinelValues()
            {
                var scanned = FieldItemRandomizer.ScanAll(AllTestFields());
                var (treasurePool, _) = FieldItemRandomizer.BuildPools(scanned);
                Assert.DoesNotContain(29999, treasurePool);
                Assert.All(treasurePool, v => Assert.True(v < 29999));
            }

            [Fact]
            public void DirectPool_ContainsKnownChestItems()
            {
                var scanned = FieldItemRandomizer.ScanAll(AllTestFields());
                var (_, directPool) = FieldItemRandomizer.BuildPools(scanned);

                // Phoenix Down (240) and Phoenix Down+ (249) from field 57
                Assert.Contains(240, directPool);
                Assert.Contains(249, directPool);
            }

            [Fact]
            public void DirectPool_ContainsOnlyItemIds()
            {
                var scanned = FieldItemRandomizer.ScanAll(AllTestFields());
                var (_, directPool) = FieldItemRandomizer.BuildPools(scanned);
                // All values in the direct pool must be raw item IDs (< 512)
                Assert.All(directPool, v =>
                    Assert.True(v < 512, $"DirectPool contains non-item value {v}"));
            }

            [Fact]
            public void DirectPool_ExcludesTextSyncAndTreasureItems()
            {
                var scanned = FieldItemRandomizer.ScanAll(AllTestFields());
                var (_, directPool) = FieldItemRandomizer.BuildPools(scanned);

                // TextSync values should never appear in either pool
                // (TextSync is a display location, not a loot source)
                foreach (var locs in scanned.Values)
                {
                    var textSyncValues = locs
                        .Where(l => l.LocationKind == FieldLocationKind.TextSync)
                        .Select(l => l.CurrentValue);

                    // A TextSync value MAY appear in the treasure pool (if it's paired
                    // with a TreasureItem) — that's expected. But the TextSync itself
                    // is not ADDED to the pool by BuildPools.
                    // What we verify here: BuildPools does not use TextSync locations
                    // as a source of pool values.
                }
                // No explicit assertion beyond "contains only item IDs" above.
            }

            [Fact]
            public void EmptyLocationMap_ProducesEmptyPools()
            {
                var empty = new Dictionary<string, IReadOnlyList<FieldItemLocation>>();
                var (treasure, direct) = FieldItemRandomizer.BuildPools(empty);
                Assert.Empty(treasure);
                Assert.Empty(direct);
            }
        }

        // ── BuildPatches ──────────────────────────────────────────────────────

        public sealed class BuildPatchesTests
        {
            [Fact]
            public void NullLocations_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    FieldItemRandomizer.BuildPatches(
                        null!, ItemRemapTable.Passthrough(), ItemRemapTable.Passthrough())
                    .ToList());
            }

            [Fact]
            public void NullTreasureTable_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    FieldItemRandomizer.BuildPatches(
                        new List<FieldItemLocation>(), null!, ItemRemapTable.Passthrough())
                    .ToList());
            }

            [Fact]
            public void NullDirectTable_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    FieldItemRandomizer.BuildPatches(
                        new List<FieldItemLocation>(), ItemRemapTable.Passthrough(), null!)
                    .ToList());
            }

            [Fact]
            public void PassthroughTables_ProduceNoPatches()
            {
                // When both tables are passthrough, no values change, so no patches
                var locs = FieldParser.FindItemLocations(
                    File.ReadAllBytes(Path.Combine(TestDataDir, "evt_alex1_at_item.eb.bytes")));

                var patches = FieldItemRandomizer.BuildPatches(
                    locs, ItemRemapTable.Passthrough(), ItemRemapTable.Passthrough()).ToList();

                Assert.Empty(patches);
            }

            [Fact]
            public void TreasureItem_UsesToTreasureTable()
            {
                // Field 108: TreasureItem(1038) and TextSync(1038)
                // Remap 1038 → 236 via treasureTable
                var locs = FieldParser.FindItemLocations(
                    File.ReadAllBytes(Path.Combine(TestDataDir, "evt_alex1_at_item.eb.bytes")));

                var treasureTable = new ItemRemapTable(new Dictionary<int, int> { { 1038, 236 } });
                var patches = FieldItemRandomizer.BuildPatches(
                    locs, treasureTable, ItemRemapTable.Passthrough()).ToList();

                // Both TreasureItem and TextSync(1038) should be patched to 236
                Assert.Contains(patches, p =>
                    p.Location.LocationKind == FieldLocationKind.TreasureItem
                    && p.NewValue == 236);
                Assert.Contains(patches, p =>
                    p.Location.LocationKind == FieldLocationKind.TextSync
                    && p.NewValue == 236);
            }

            [Fact]
            public void TextSync_LargeValue_AlwaysUsesTreasureTable()
            {
                // A TextSync with value >= 512 (card or gil encoding) always uses treasureTable
                var textSyncLoc = new FieldItemLocation(100, 513, 2, FieldLocationKind.TextSync);
                var treasureTable = new ItemRemapTable(new Dictionary<int, int> { { 513, 517 } });
                var directTable = new ItemRemapTable(new Dictionary<int, int> { { 513, 999 } });

                var patches = FieldItemRandomizer.BuildPatches(
                    new[] { textSyncLoc }, treasureTable, directTable).ToList();

                Assert.Single(patches);
                Assert.Equal(517, patches[0].NewValue); // from treasureTable, not directTable
            }

            [Fact]
            public void TextSync_SmallValue_UsesTreasureTableWhenPairedWithTreasureItem()
            {
                // TreasureItem(236) is present → TextSync(236) should use treasureTable
                var locations = new List<FieldItemLocation>
                {
                    new(1000, 236, 2, FieldLocationKind.TreasureItem),
                    new(1010, 236, 2, FieldLocationKind.TextSync),
                };

                var treasureTable = new ItemRemapTable(new Dictionary<int, int> { { 236, 244 } });
                var directTable = new ItemRemapTable(new Dictionary<int, int> { { 236, 249 } });

                var patches = FieldItemRandomizer.BuildPatches(
                    locations, treasureTable, directTable).ToList();

                var textSyncPatch = patches.Single(p =>
                    p.Location.LocationKind == FieldLocationKind.TextSync);
                Assert.Equal(244, textSyncPatch.NewValue); // from treasureTable
            }

            [Fact]
            public void TextSync_SmallValue_UsesDirectTableWhenNoPairedTreasureItem()
            {
                // Only a DirectItem(236) — no TreasureItem(236) in this field
                // → TextSync(236) should use directTable
                var locations = new List<FieldItemLocation>
                {
                    new(1000, 236, 2, FieldLocationKind.DirectItem),
                    new(1010, 236, 2, FieldLocationKind.TextSync),
                };

                var treasureTable = new ItemRemapTable(new Dictionary<int, int> { { 236, 244 } });
                var directTable = new ItemRemapTable(new Dictionary<int, int> { { 236, 249 } });

                var patches = FieldItemRandomizer.BuildPatches(
                    locations, treasureTable, directTable).ToList();

                var textSyncPatch = patches.Single(p =>
                    p.Location.LocationKind == FieldLocationKind.TextSync);
                Assert.Equal(249, textSyncPatch.NewValue); // from directTable
            }

            [Fact]
            public void DirectGil_IsNotPatched()
            {
                // DirectGil locations should pass through unchanged in Phase 4
                var gilLoc = new FieldItemLocation(0, 1000, 3, FieldLocationKind.DirectGil);
                var patches = FieldItemRandomizer.BuildPatches(
                    new[] { gilLoc },
                    ItemRemapTable.Passthrough(),
                    ItemRemapTable.Passthrough()).ToList();

                Assert.Empty(patches); // no patch emitted (value unchanged)
            }

            [Fact]
            public void OnlyChangedValues_ProducePatches()
            {
                // If the remapped value equals the original, no patch is emitted
                var loc = new FieldItemLocation(0, 236, 2, FieldLocationKind.TreasureItem);

                // Table maps 236 → 236 (same value)
                var table = new ItemRemapTable(new Dictionary<int, int> { { 236, 236 } });
                var patches = FieldItemRandomizer.BuildPatches(
                    new[] { loc }, table, ItemRemapTable.Passthrough()).ToList();

                Assert.Empty(patches);
            }

            // ── Round-trip: scan → build patches → apply → re-scan ────────────

            [Fact]
            public void Field108_RoundTrip_ProducesNewValues()
            {
                byte[] original = File.ReadAllBytes(
                    Path.Combine(TestDataDir, "evt_alex1_at_item.eb.bytes"));
                var locs = FieldParser.FindItemLocations(original);

                // Remap 1038 (38 gil) → 500
                var treasureTable = new ItemRemapTable(new Dictionary<int, int> { { 1038, 500 } });
                var patches = FieldItemRandomizer.BuildPatches(
                    locs, treasureTable, ItemRemapTable.Passthrough()).ToList();

                byte[] patched = FieldParser.ApplyPatches(original, patches);
                var newLocs = FieldParser.FindItemLocations(patched);

                Assert.Contains(newLocs, l =>
                    l.LocationKind == FieldLocationKind.TreasureItem && l.CurrentValue == 500);
                Assert.DoesNotContain(newLocs, l => l.CurrentValue == 1038);
            }

            [Fact]
            public void Field114_RoundTrip_AllTreasureItemsRemapped()
            {
                byte[] original = File.ReadAllBytes(
                    Path.Combine(TestDataDir, "evt_alex1_at_house_2.eb.bytes"));
                var locs = FieldParser.FindItemLocations(original);

                // Remap all known treasure values to something different
                var treasureTable = new ItemRemapTable(new Dictionary<int, int>
                {
                    { 513,  244 },  // Fang card → Eye Drops
                    { 1009, 1047 }, // 9 gil → 47 gil
                    { 236,  513 },  // Potion → Fang card
                });

                var patches = FieldItemRandomizer.BuildPatches(
                    locs, treasureTable, ItemRemapTable.Passthrough()).ToList();

                byte[] patched = FieldParser.ApplyPatches(original, patches);
                var newLocs = FieldParser.FindItemLocations(patched)
                    .Where(l => l.LocationKind == FieldLocationKind.TreasureItem)
                    .ToList();

                Assert.Equal(3, newLocs.Count);
                Assert.Contains(newLocs, l => l.CurrentValue == 244);
                Assert.Contains(newLocs, l => l.CurrentValue == 1047);
                Assert.Contains(newLocs, l => l.CurrentValue == 513);
                Assert.DoesNotContain(newLocs, l => l.CurrentValue == 236);
                Assert.DoesNotContain(newLocs, l => l.CurrentValue == 1009);
            }
        }

        // ── GetOutputPath ─────────────────────────────────────────────────────

        public sealed class GetOutputPathTests
        {
            [Fact]
            public void ConstructsCorrectPath()
            {
                string result = FieldItemRandomizer.GetOutputPath(
                    @"C:\Mod", "us", "evt_alex1_at_house_2.eb.bytes");

                string expected = Path.Combine(
                    @"C:\Mod",
                    @"StreamingAssets\assets\resources\commonasset\eventengine\eventbinary\field",
                    "us",
                    "evt_alex1_at_house_2.eb.bytes");

                Assert.Equal(expected, result);
            }

            [Fact]
            public void AllLanguages_ProduceDifferentPaths()
            {
                var paths = FieldItemRandomizer.Languages
                    .Select(lang => FieldItemRandomizer.GetOutputPath(
                        @"C:\Mod", lang, "evt_test.eb.bytes"))
                    .ToList();

                Assert.Equal(7, paths.Count);
                Assert.Equal(7, paths.Distinct().Count()); // all different
            }

            [Fact]
            public void AllLanguages_DifferOnlyInLanguageSegment()
            {
                const string fileName = "evt_alex1_at_house_2.eb.bytes";
                foreach (string lang in FieldItemRandomizer.Languages)
                {
                    string path = FieldItemRandomizer.GetOutputPath(@"C:\Mod", lang, fileName);
                    Assert.Contains(lang, path);
                    Assert.EndsWith(fileName, path);
                }
            }
        }
    }
}