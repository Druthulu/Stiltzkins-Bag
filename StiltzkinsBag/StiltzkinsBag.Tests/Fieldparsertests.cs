using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StiltzkinsBag.Core.Models;
using StiltzkinsBag.Parsing;
using Xunit;

namespace StiltzkinsBag.Tests.Parsing
{
    /// <summary>
    /// Tests for FieldParser — FF9 Steam field script file parser and patcher.
    ///
    /// TEST DATA
    /// ---------
    /// Place the following .bytes files in Tests/TestData/FieldParser/:
    ///   evt_alex1_at_house_2.eb.bytes    (field 114 — grandma's house)
    ///   evt_alex1_ts_cargo_0.eb.bytes    (field  50 — cargo room)
    ///   evt_alex1_ts_engin.eb.bytes      (field  57 — engine room)
    ///   evt_alex1_at_center.eb.bytes     (field 103 — Alexandria Square)
    ///   evt_alex1_at_gate.eb.bytes       (field 107 — Alexandria Main Street)
    ///   evt_alex1_at_item.eb.bytes       (field 108 — Item Shop)
    ///   evt_alex1_at_house_1.eb.bytes    (field 113 — little girl's house)
    ///
    /// These are extracted from p0data7.bin via UnityArchiver and committed to the
    /// test data folder (set CopyToOutputDirectory = Always in the .csproj).
    ///
    /// GROUND-TRUTH METHODOLOGY
    /// ------------------------
    /// All expected offsets and values were derived by running the same scanning
    /// logic as a Python simulation directly against the raw .bytes files, then
    /// cross-referenced against Hades Workshop C script exports (both with and
    /// without readability comments) for each field.
    ///
    /// KNOWN FALSE POSITIVES
    /// ---------------------
    /// Some fields contain DirectItem and DirectGil results with impossible item IDs
    /// (e.g., 10008, 18241, 32536) or extreme gil values (-3276454, 2487446). These
    /// arise because 0x48, 0xCE etc. can appear inside variable expressions of other
    /// opcodes. They are harmless — the ItemRemapTable passthrough returns unknown
    /// IDs unchanged, so patching is a no-op. Tests assert that the KNOWN-GOOD
    /// locations are present; they do not assert exact total counts for fields that
    /// contain false positives.
    /// </summary>
    public sealed class FieldParserTests
    {
        // ── Test data loading ─────────────────────────────────────────────────

        private static string TestDataDir =>
            Path.Combine(AppContext.BaseDirectory, "TestData", "FieldParser");

        private static byte[] Load(string filename) =>
            File.ReadAllBytes(Path.Combine(TestDataDir, filename));

        // Convenience aliases matching the field nicknames used throughout
        private static byte[] House2 => Load("evt_alex1_at_house_2.eb.bytes");  // field 114
        private static byte[] Cargo => Load("evt_alex1_ts_cargo_0.eb.bytes");  // field  50
        private static byte[] Engine => Load("evt_alex1_ts_engin.eb.bytes");    // field  57
        private static byte[] Center => Load("evt_alex1_at_center.eb.bytes");   // field 103
        private static byte[] Gate => Load("evt_alex1_at_gate.eb.bytes");     // field 107
        private static byte[] ItemShop => Load("evt_alex1_at_item.eb.bytes");    // field 108
        private static byte[] House1 => Load("evt_alex1_at_house_1.eb.bytes"); // field 113

        // ── ParseHeader ───────────────────────────────────────────────────────

        public sealed class ParseHeaderTests
        {
            [Fact]
            public void NullFile_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() => FieldParser.ParseHeader(null!));
            }

            [Fact]
            public void TooShortFile_ThrowsArgumentException()
            {
                Assert.Throws<ArgumentException>(
                    () => FieldParser.ParseHeader(new byte[64]));
            }

            // ── Field 108 (Item Shop) — simplest clean header ─────────────────
            // Ground truth: magic=0x5645, 14 entries, 3208 bytes

            [Fact]
            public void Field108_MagicNumber_Is0x5645()
            {
                var header = FieldParser.ParseHeader(Load("evt_alex1_at_item.eb.bytes"));
                Assert.Equal(0x5645, header.MagicNumber);
            }

            [Fact]
            public void Field108_EntryCount_Is14()
            {
                var header = FieldParser.ParseHeader(Load("evt_alex1_at_item.eb.bytes"));
                Assert.Equal(14, header.EntryAmount);
                Assert.Equal(14, header.Entries.Length);
            }

            [Fact]
            public void Field108_Entry0_IsCorrect()
            {
                var e = FieldParser.ParseHeader(Load("evt_alex1_at_item.eb.bytes")).Entries[0];
                Assert.Equal(112, e.EntryOffset);
                Assert.Equal(756, e.EntrySize);
                Assert.Equal(0x00, e.EntryType);
                Assert.Equal(2, e.Functions.Length);
            }

            [Fact]
            public void Field108_Entry2_HasFourFunctions()
            {
                var e = FieldParser.ParseHeader(Load("evt_alex1_at_item.eb.bytes")).Entries[2];
                Assert.Equal(0x02, e.EntryType);
                Assert.Equal(4, e.Functions.Length);
            }

            // ── Field 113 (little girl's house) ──────────────────────────────
            // Ground truth: magic=0x5645, 15 entries

            [Fact]
            public void Field113_EntryCount_Is15()
            {
                var header = FieldParser.ParseHeader(Load("evt_alex1_at_house_1.eb.bytes"));
                Assert.Equal(15, header.EntryAmount);
            }

            [Fact]
            public void Field113_Entry2_HasFourFunctions()
            {
                var e = FieldParser.ParseHeader(Load("evt_alex1_at_house_1.eb.bytes")).Entries[2];
                Assert.Equal(0x02, e.EntryType);
                Assert.Equal(4, e.Functions.Length);
            }

            // ── Field 114 (grandma's house) ───────────────────────────────────
            // Ground truth: 19 entries

            [Fact]
            public void Field114_EntryCount_Is19()
            {
                var header = FieldParser.ParseHeader(Load("evt_alex1_at_house_2.eb.bytes"));
                Assert.Equal(19, header.EntryAmount);
            }

            // ── Shared across all fields ──────────────────────────────────────

            [Theory]
            [InlineData("evt_alex1_at_item.eb.bytes")]
            [InlineData("evt_alex1_at_house_1.eb.bytes")]
            [InlineData("evt_alex1_at_house_2.eb.bytes")]
            [InlineData("evt_alex1_at_gate.eb.bytes")]
            [InlineData("evt_alex1_ts_cargo_0.eb.bytes")]
            [InlineData("evt_alex1_ts_engin.eb.bytes")]
            [InlineData("evt_alex1_at_center.eb.bytes")]
            public void AllTestFiles_HaveMagic0x5645(string filename)
            {
                var header = FieldParser.ParseHeader(Load(filename));
                Assert.Equal(0x5645, header.MagicNumber);
            }

            [Theory]
            [InlineData("evt_alex1_at_item.eb.bytes")]
            [InlineData("evt_alex1_at_house_1.eb.bytes")]
            [InlineData("evt_alex1_at_house_2.eb.bytes")]
            [InlineData("evt_alex1_at_gate.eb.bytes")]
            [InlineData("evt_alex1_ts_cargo_0.eb.bytes")]
            [InlineData("evt_alex1_ts_engin.eb.bytes")]
            [InlineData("evt_alex1_at_center.eb.bytes")]
            public void AllTestFiles_HavePositiveEntryCount(string filename)
            {
                var header = FieldParser.ParseHeader(Load(filename));
                Assert.True(header.EntryAmount > 0, "Expected at least one entry");
            }
        }

        // ── FindItemLocations ─────────────────────────────────────────────────

        public sealed class FindItemLocationsTests
        {
            // ─── Field 108: Item Shop — gil-only, two clean locations ──────────
            //
            // Script: set Treasure_Item = 1038  (= 38 gil, encoded as 1038)
            //         SetTextVariable( 0, 1038 )
            //
            // Ground truth: exactly 2 locations (TreasureItem @2087, TextSync @2094).
            // This is the cleanest test field — no false positives, no DirectItems.

            [Fact]
            public void Field108_ExactlyTwoLocations()
            {
                var locs = FieldParser.FindItemLocations(
                    Load("evt_alex1_at_item.eb.bytes"));
                Assert.Equal(2, locs.Count);
            }

            [Fact]
            public void Field108_TreasureItem_38Gil_AtOffset2087()
            {
                var loc = FieldParser.FindItemLocations(
                    Load("evt_alex1_at_item.eb.bytes"))
                    .Single(l => l.LocationKind == FieldLocationKind.TreasureItem);

                Assert.Equal(2087, loc.FileOffset);
                Assert.Equal(1038, loc.CurrentValue);
                Assert.Equal(2, loc.ArgByteWidth);
                Assert.True(loc.TreasureIsGil, "Should be TreasureIsGil");
                Assert.False(loc.TreasureIsItem, "Should NOT be TreasureIsItem");
                Assert.False(loc.TreasureIsCard, "Should NOT be TreasureIsCard");
                Assert.Equal(38, loc.TreasureGilAmount);
            }

            [Fact]
            public void Field108_TextSync_38Gil_AtOffset2094()
            {
                var loc = FieldParser.FindItemLocations(
                    Load("evt_alex1_at_item.eb.bytes"))
                    .Single(l => l.LocationKind == FieldLocationKind.TextSync);

                Assert.Equal(2094, loc.FileOffset);
                Assert.Equal(1038, loc.CurrentValue);
                Assert.Equal(2, loc.ArgByteWidth);
            }

            // ─── Field 113: Little Girl's House — two items, four clean locs ──
            //
            // Script: set Treasure_Item = 244  (Eye Drops)
            //         SetTextVariable( 0, 244 )
            //         set Treasure_Item = 1003 (3 gil, encoded)
            //         SetTextVariable( 0, 1003 )
            //
            // Ground truth: exactly 4 locations, no false positives.

            [Fact]
            public void Field113_ExactlyFourLocations()
            {
                var locs = FieldParser.FindItemLocations(
                    Load("evt_alex1_at_house_1.eb.bytes"));
                Assert.Equal(4, locs.Count);
            }

            [Fact]
            public void Field113_TreasureItem_EyeDrops_AtOffset1883()
            {
                var loc = GetTreasureItem(
                    Load("evt_alex1_at_house_1.eb.bytes"), 244);

                Assert.Equal(1883, loc.FileOffset);
                Assert.True(loc.TreasureIsItem, "Eye Drops should be TreasureIsItem");
                Assert.False(loc.TreasureIsGil);
                Assert.False(loc.TreasureIsCard);
            }

            [Fact]
            public void Field113_TextSync_EyeDrops_AtOffset1890()
            {
                var loc = GetTextSync(
                    Load("evt_alex1_at_house_1.eb.bytes"), 244);
                Assert.Equal(1890, loc.FileOffset);
            }

            [Fact]
            public void Field113_TreasureItem_3Gil_AtOffset2171()
            {
                var loc = GetTreasureItem(
                    Load("evt_alex1_at_house_1.eb.bytes"), 1003);

                Assert.Equal(2171, loc.FileOffset);
                Assert.True(loc.TreasureIsGil, "1003 should be TreasureIsGil");
                Assert.Equal(3, loc.TreasureGilAmount);
            }

            [Fact]
            public void Field113_TextSync_3Gil_AtOffset2178()
            {
                var loc = GetTextSync(
                    Load("evt_alex1_at_house_1.eb.bytes"), 1003);
                Assert.Equal(2178, loc.FileOffset);
            }

            // ─── Field 114: Grandma's House — three items ─────────────────────
            //
            // Script (confirmed in HW export + binary):
            //   set Treasure_Item = 513  (Fang card = card slot 1)
            //   set Treasure_Item = 1009 (9 gil, encoded)
            //   set Treasure_Item = 236  (Potion)
            //   set Treasure_Item = 29999 (disabled — scanner SKIPS)
            //
            // Ground truth: 7 total locations (6 known-good + 1 false-positive DirectItem).
            // Tests assert that every known-good location is present at its exact offset.

            [Fact]
            public void Field114_ContainsThreeTreasureItems()
            {
                var locs = FieldParser.FindItemLocations(
                    Load("evt_alex1_at_house_2.eb.bytes"));
                Assert.Equal(3, locs.Count(l => l.LocationKind == FieldLocationKind.TreasureItem));
            }

            [Fact]
            public void Field114_TreasureItem_FangCard_AtOffset3187()
            {
                var loc = GetTreasureItem(
                    Load("evt_alex1_at_house_2.eb.bytes"), 513);

                Assert.Equal(3187, loc.FileOffset);
                Assert.True(loc.TreasureIsCard, "513 should be TreasureIsCard");
                Assert.False(loc.TreasureIsItem);
                Assert.False(loc.TreasureIsGil);
                // Card slot = 513 - 512 = 1
                Assert.Equal(1, loc.CurrentValue - 512);
            }

            [Fact]
            public void Field114_TextSync_FangCard_AtOffset3194()
            {
                var loc = GetTextSync(
                    Load("evt_alex1_at_house_2.eb.bytes"), 513);
                Assert.Equal(3194, loc.FileOffset);
            }

            [Fact]
            public void Field114_TreasureItem_9Gil_AtOffset3475()
            {
                var loc = GetTreasureItem(
                    Load("evt_alex1_at_house_2.eb.bytes"), 1009);

                Assert.Equal(3475, loc.FileOffset);
                Assert.True(loc.TreasureIsGil);
                Assert.Equal(9, loc.TreasureGilAmount);
            }

            [Fact]
            public void Field114_TextSync_9Gil_AtOffset3482()
            {
                var loc = GetTextSync(
                    Load("evt_alex1_at_house_2.eb.bytes"), 1009);
                Assert.Equal(3482, loc.FileOffset);
            }

            [Fact]
            public void Field114_TreasureItem_Potion_AtOffset3763()
            {
                var loc = GetTreasureItem(
                    Load("evt_alex1_at_house_2.eb.bytes"), 236);

                Assert.Equal(3763, loc.FileOffset);
                Assert.True(loc.TreasureIsItem);
            }

            [Fact]
            public void Field114_TextSync_Potion_AtOffset3770()
            {
                var loc = GetTextSync(
                    Load("evt_alex1_at_house_2.eb.bytes"), 236);
                Assert.Equal(3770, loc.FileOffset);
            }

            [Fact]
            public void Field114_NoTreasureItemWithSentinelValue()
            {
                // 29999 is explicitly disabled; the >= filter also blocks 64776/64785
                var locs = FieldParser.FindItemLocations(
                    Load("evt_alex1_at_house_2.eb.bytes"));
                Assert.DoesNotContain(locs, l =>
                    l.LocationKind == FieldLocationKind.TreasureItem
                    && l.CurrentValue >= 29999);
            }

            // ─── Field 50: Cargo Room — two items ─────────────────────────────
            //
            // Script: Potion (236) + 47 gil (encoded 1047)
            // Ground truth: 5 total (4 known-good + 1 false-positive DirectItem).

            [Fact]
            public void Field50_ContainsTwoTreasureItems()
            {
                var locs = FieldParser.FindItemLocations(
                    Load("evt_alex1_ts_cargo_0.eb.bytes"));
                Assert.Equal(2, locs.Count(l => l.LocationKind == FieldLocationKind.TreasureItem));
            }

            [Fact]
            public void Field50_TreasureItem_Potion_AtOffset2731()
            {
                var loc = GetTreasureItem(
                    Load("evt_alex1_ts_cargo_0.eb.bytes"), 236);

                Assert.Equal(2731, loc.FileOffset);
                Assert.True(loc.TreasureIsItem);
            }

            [Fact]
            public void Field50_TreasureItem_47Gil_AtOffset3019()
            {
                var loc = GetTreasureItem(
                    Load("evt_alex1_ts_cargo_0.eb.bytes"), 1047);

                Assert.Equal(3019, loc.FileOffset);
                Assert.True(loc.TreasureIsGil);
                Assert.Equal(47, loc.TreasureGilAmount);
            }

            [Fact]
            public void Field50_TextSyncs_At2738_And_3026()
            {
                var locs = FieldParser.FindItemLocations(
                    Load("evt_alex1_ts_cargo_0.eb.bytes"))
                    .Where(l => l.LocationKind == FieldLocationKind.TextSync)
                    .ToList();

                Assert.Contains(locs, l => l.FileOffset == 2738 && l.CurrentValue == 236);
                Assert.Contains(locs, l => l.FileOffset == 3026 && l.CurrentValue == 1047);
            }

            // ─── Field 57: Engine Room — two chests (Phoenix Downs) ───────────
            //
            // Script: AddItem(240, 1) + AddItem(249, 1) for two chest objects.
            // Each item appears twice per chest (primary branch + alternate branch).
            // Dead-code branches: set Treasure_Item=64776/64785 — must be SKIPPED.
            //
            // Ground truth: 11 total locations.

            [Fact]
            public void Field57_ContainsDirectItem_PhoenixDown240()
            {
                var locs = FieldParser.FindItemLocations(
                    Load("evt_alex1_ts_engin.eb.bytes"));
                Assert.Contains(locs, l =>
                    l.LocationKind == FieldLocationKind.DirectItem
                    && l.CurrentValue == 240);
            }

            [Fact]
            public void Field57_ContainsDirectItem_PhoenixDownPlus_249()
            {
                var locs = FieldParser.FindItemLocations(
                    Load("evt_alex1_ts_engin.eb.bytes"));
                Assert.Contains(locs, l =>
                    l.LocationKind == FieldLocationKind.DirectItem
                    && l.CurrentValue == 249);
            }

            [Fact]
            public void Field57_DirectItem240_PrimaryBranch_AtOffset4423()
            {
                var locs = FieldParser.FindItemLocations(
                    Load("evt_alex1_ts_engin.eb.bytes"))
                    .Where(l => l.LocationKind == FieldLocationKind.DirectItem
                                && l.CurrentValue == 240)
                    .ToList();

                Assert.Contains(locs, l => l.FileOffset == 4423);
            }

            [Fact]
            public void Field57_DirectItem249_PrimaryBranch_AtOffset5079()
            {
                var locs = FieldParser.FindItemLocations(
                    Load("evt_alex1_ts_engin.eb.bytes"))
                    .Where(l => l.LocationKind == FieldLocationKind.DirectItem
                                && l.CurrentValue == 249)
                    .ToList();

                Assert.Contains(locs, l => l.FileOffset == 5079);
            }

            [Fact]
            public void Field57_NoTreasureItemsSurviveSentinelFilter()
            {
                // Dead-code set Treasure_Item=64776/64785 must be filtered by >= 29999 check
                var locs = FieldParser.FindItemLocations(
                    Load("evt_alex1_ts_engin.eb.bytes"));
                Assert.DoesNotContain(locs, l =>
                    l.LocationKind == FieldLocationKind.TreasureItem
                    && l.CurrentValue >= 29999);
                Assert.Empty(locs.Where(l => l.LocationKind == FieldLocationKind.TreasureItem));
            }

            // ─── Field 107: Alexandria Main Street — four hidden items ─────────
            //
            // Script: Potion (236) + three cards (517=card5, 521=card9, 518=card6)
            // Ground truth: 10 total (8 known-good + 2 false-positive DirectItem(1)).

            [Fact]
            public void Field107_ContainsFourTreasureItems()
            {
                var locs = FieldParser.FindItemLocations(
                    Load("evt_alex1_at_gate.eb.bytes"));
                Assert.Equal(4, locs.Count(l => l.LocationKind == FieldLocationKind.TreasureItem));
            }

            [Fact]
            public void Field107_TreasureItem_Potion_AtOffset7675()
            {
                var loc = GetTreasureItem(
                    Load("evt_alex1_at_gate.eb.bytes"), 236);

                Assert.Equal(7675, loc.FileOffset);
                Assert.True(loc.TreasureIsItem);
            }

            [Fact]
            public void Field107_TreasureItems_ThreeCards_AtCorrectOffsets()
            {
                var locs = FieldParser.FindItemLocations(
                    Load("evt_alex1_at_gate.eb.bytes"))
                    .Where(l => l.LocationKind == FieldLocationKind.TreasureItem
                                && l.TreasureIsCard)
                    .OrderBy(l => l.FileOffset)
                    .ToList();

                Assert.Equal(3, locs.Count);
                Assert.Equal(517, locs[0].CurrentValue); Assert.Equal(7963, locs[0].FileOffset);
                Assert.Equal(521, locs[1].CurrentValue); Assert.Equal(8251, locs[1].FileOffset);
                Assert.Equal(518, locs[2].CurrentValue); Assert.Equal(8539, locs[2].FileOffset);
            }

            [Fact]
            public void Field107_TextSyncs_PairedWithTreasureItems()
            {
                var locs = FieldParser.FindItemLocations(
                    Load("evt_alex1_at_gate.eb.bytes")).ToList();

                // Each TreasureItem value must have a matching TextSync value
                var treasureValues = locs
                    .Where(l => l.LocationKind == FieldLocationKind.TreasureItem)
                    .Select(l => l.CurrentValue)
                    .ToHashSet();

                var textSyncValues = locs
                    .Where(l => l.LocationKind == FieldLocationKind.TextSync)
                    .Select(l => l.CurrentValue)
                    .ToHashSet();

                foreach (int v in treasureValues)
                    Assert.Contains(v, textSyncValues);
            }

            [Fact]
            public void Field107_NoSentinelTreasureItems()
            {
                var locs = FieldParser.FindItemLocations(
                    Load("evt_alex1_at_gate.eb.bytes"));
                Assert.DoesNotContain(locs, l =>
                    l.LocationKind == FieldLocationKind.TreasureItem
                    && l.CurrentValue >= 29999);
            }

            // ─── Shared: no sentinel value survives in any field ─────────────

            [Theory]
            [InlineData("evt_alex1_at_item.eb.bytes")]
            [InlineData("evt_alex1_at_house_1.eb.bytes")]
            [InlineData("evt_alex1_at_house_2.eb.bytes")]
            [InlineData("evt_alex1_at_gate.eb.bytes")]
            [InlineData("evt_alex1_ts_cargo_0.eb.bytes")]
            [InlineData("evt_alex1_ts_engin.eb.bytes")]
            [InlineData("evt_alex1_at_center.eb.bytes")]
            public void AllFields_NoTreasureItemAtOrAboveSentinel(string filename)
            {
                var locs = FieldParser.FindItemLocations(Load(filename));
                Assert.DoesNotContain(locs, l =>
                    l.LocationKind == FieldLocationKind.TreasureItem
                    && l.CurrentValue >= 29999);
            }
        }

        // ── TreasureItem helper properties ────────────────────────────────────

        public sealed class TreasureItemHelperTests
        {
            [Theory]
            [InlineData(0, true, false, false)]   // item 0 (edge: valid item range)
            [InlineData(236, true, false, false)]  // Potion
            [InlineData(244, true, false, false)]  // Eye Drops
            [InlineData(511, true, false, false)]  // last valid item ID
            public void TreasureIsItem_CorrectForItemRange(
                int value, bool expectItem, bool expectCard, bool expectGil)
            {
                var loc = MakeTreasure(value);
                Assert.Equal(expectItem, loc.TreasureIsItem);
                Assert.Equal(expectCard, loc.TreasureIsCard);
                Assert.Equal(expectGil, loc.TreasureIsGil);
            }

            [Theory]
            [InlineData(512, false, true, false)]  // first card slot (card 0)
            [InlineData(513, false, true, false)]  // Fang card (card slot 1)
            [InlineData(517, false, true, false)]  // card slot 5
            [InlineData(521, false, true, false)]  // card slot 9
            [InlineData(518, false, true, false)]  // card slot 6
            [InlineData(999, false, true, false)]  // last valid card slot
            public void TreasureIsCard_CorrectForCardRange(
                int value, bool expectItem, bool expectCard, bool expectGil)
            {
                var loc = MakeTreasure(value);
                Assert.Equal(expectItem, loc.TreasureIsItem);
                Assert.Equal(expectCard, loc.TreasureIsCard);
                Assert.Equal(expectGil, loc.TreasureIsGil);
            }

            [Theory]
            [InlineData(1000, false, false, true, 0)]    // 0 gil (edge)
            [InlineData(1003, false, false, true, 3)]    // 3 gil
            [InlineData(1009, false, false, true, 9)]    // 9 gil
            [InlineData(1038, false, false, true, 38)]   // 38 gil
            [InlineData(1047, false, false, true, 47)]   // 47 gil
            [InlineData(2000, false, false, true, 1000)] // 1000 gil
            [InlineData(29998, false, false, true, 28998)] // last valid gil value
            public void TreasureIsGil_CorrectForGilRange(
                int value, bool expectItem, bool expectCard, bool expectGil, int gilAmount)
            {
                var loc = MakeTreasure(value);
                Assert.Equal(expectItem, loc.TreasureIsItem);
                Assert.Equal(expectCard, loc.TreasureIsCard);
                Assert.Equal(expectGil, loc.TreasureIsGil);
                if (expectGil)
                    Assert.Equal(gilAmount, loc.TreasureGilAmount);
            }

            [Theory]
            [InlineData(29999)]   // disabled sentinel
            [InlineData(64776)]   // dead-code "empty chest" (engine room)
            [InlineData(64785)]   // dead-code "empty chest" (engine room)
            [InlineData(65535)]   // max uint16
            public void SentinelValues_AreNotAnyCategory(int value)
            {
                // These values should never reach the helper properties because
                // FindItemLocations filters them out — but the model is still correct:
                // none of the helper predicates should fire for sentinel values.
                var loc = MakeTreasure(value);
                Assert.False(loc.TreasureIsItem,
                    $"Sentinel {value} should not be TreasureIsItem");
                Assert.False(loc.TreasureIsCard,
                    $"Sentinel {value} should not be TreasureIsCard");
                Assert.False(loc.TreasureIsGil,
                    $"Sentinel {value} should not be TreasureIsGil");
            }

            private static FieldItemLocation MakeTreasure(int value) =>
                new FieldItemLocation(0, value, 2, FieldLocationKind.TreasureItem);
        }

        // ── ApplyPatches ──────────────────────────────────────────────────────

        public sealed class ApplyPatchesTests
        {
            [Fact]
            public void NullFile_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    FieldParser.ApplyPatches(null!,
                        Array.Empty<(FieldItemLocation, int)>()));
            }

            [Fact]
            public void NullPatches_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    FieldParser.ApplyPatches(new byte[256], null!));
            }

            // ── Round-trip: Field 108, remap 38-gil treasure → Potion (item 236) ──
            //
            // Field 108 has exactly 2 locations:
            //   TreasureItem @2087  value=1038
            //   TextSync     @2094  value=1038
            //
            // We remap both to 236 (Potion, uint16 LE = EC 00).
            // After patch:
            //   byte[2087..2088] = { 0xEC, 0x00 }
            //   byte[2094..2095] = { 0xEC, 0x00 }
            // All other bytes must be unchanged.

            [Fact]
            public void Field108_PatchBothLocations_BytesCorrectAtTargetOffsets()
            {
                byte[] original = Load("evt_alex1_at_item.eb.bytes");
                var locs = FieldParser.FindItemLocations(original);

                var patches = locs.Select(l => (l, 236)).ToList(); // remap everything to Potion
                byte[] patched = FieldParser.ApplyPatches(original, patches);

                // TreasureItem @2087 → 236 (0xEC 0x00 LE)
                Assert.Equal(0xEC, patched[2087]);
                Assert.Equal(0x00, patched[2088]);

                // TextSync @2094 → 236 (0xEC 0x00 LE)
                Assert.Equal(0xEC, patched[2094]);
                Assert.Equal(0x00, patched[2095]);
            }

            [Fact]
            public void Field108_PatchBothLocations_AllOtherBytesUnchanged()
            {
                byte[] original = Load("evt_alex1_at_item.eb.bytes");
                var locs = FieldParser.FindItemLocations(original);
                var patches = locs.Select(l => (l, 236)).ToList();
                byte[] patched = FieldParser.ApplyPatches(original, patches);

                var changed = new HashSet<int>();
                foreach (var (loc, _) in patches)
                    for (int k = 0; k < loc.ArgByteWidth; k++)
                        changed.Add(loc.FileOffset + k);

                for (int i = 0; i < original.Length; i++)
                {
                    if (!changed.Contains(i))
                        Assert.Equal(original[i], patched[i]);
                }
            }

            [Fact]
            public void ApplyPatches_DoesNotMutateOriginalFile()
            {
                byte[] original = Load("evt_alex1_at_item.eb.bytes");
                byte[] copy = (byte[])original.Clone();
                var locs = FieldParser.FindItemLocations(original);
                _ = FieldParser.ApplyPatches(original, locs.Select(l => (l, 999)).ToList());

                Assert.Equal(copy, original); // original must be unchanged
            }

            // ── Round-trip: Field 113, remap Eye Drops (244) → Potion (236) ──

            [Fact]
            public void Field113_PatchEyeDrops_BytesAtOffset1883And1890()
            {
                byte[] original = Load("evt_alex1_at_house_1.eb.bytes");
                var locs = FieldParser.FindItemLocations(original)
                    .Where(l => l.CurrentValue == 244)
                    .ToList();

                byte[] patched = FieldParser.ApplyPatches(original,
                    locs.Select(l => (l, 236)).ToList());

                // TreasureItem @1883 → 236 (0xEC 0x00)
                Assert.Equal(0xEC, patched[1883]);
                Assert.Equal(0x00, patched[1884]);

                // TextSync @1890 → 236 (0xEC 0x00)
                Assert.Equal(0xEC, patched[1890]);
                Assert.Equal(0x00, patched[1891]);
            }

            // ── Round-trip: re-scan after patch returns new values ─────────────

            [Fact]
            public void Field108_RescanAfterPatch_ReturnsNewValue()
            {
                byte[] original = Load("evt_alex1_at_item.eb.bytes");
                var locs = FieldParser.FindItemLocations(original);

                // Remap 1038 → 500
                byte[] patched = FieldParser.ApplyPatches(original,
                    locs.Select(l => (l, 500)).ToList());

                var newLocs = FieldParser.FindItemLocations(patched);
                Assert.Equal(2, newLocs.Count);
                Assert.All(newLocs, l => Assert.Equal(500, l.CurrentValue));
            }

            [Fact]
            public void Field113_RescanAfterPatch_ReturnsNewValues()
            {
                byte[] original = Load("evt_alex1_at_house_1.eb.bytes");
                var locs = FieldParser.FindItemLocations(original);

                // Remap Eye Drops(244) → Potion(236), 3-gil(1003) → Ether(248)
                var patches = locs.Select(l =>
                    (l, l.CurrentValue == 244 ? 236 : 248)).ToList();
                byte[] patched = FieldParser.ApplyPatches(original, patches);

                var newLocs = FieldParser.FindItemLocations(patched)
                    .Where(l => l.LocationKind == FieldLocationKind.TreasureItem)
                    .ToList();

                Assert.Contains(newLocs, l => l.CurrentValue == 236);
                Assert.Contains(newLocs, l => l.CurrentValue == 248);
                Assert.DoesNotContain(newLocs, l => l.CurrentValue == 244);
                Assert.DoesNotContain(newLocs, l => l.CurrentValue == 1003);
            }

            [Fact]
            public void Field114_RescanAfterPatch_ReturnsNewTreasureValues()
            {
                byte[] original = Load("evt_alex1_at_house_2.eb.bytes");
                var locs = FieldParser.FindItemLocations(original)
                    .Where(l => l.LocationKind == FieldLocationKind.TreasureItem
                                || l.LocationKind == FieldLocationKind.TextSync)
                    .ToList();

                // Remap everything to Ether (248)
                byte[] patched = FieldParser.ApplyPatches(original,
                    locs.Select(l => (l, 248)).ToList());

                var newTreasures = FieldParser.FindItemLocations(patched)
                    .Where(l => l.LocationKind == FieldLocationKind.TreasureItem)
                    .ToList();

                Assert.Equal(3, newTreasures.Count);
                Assert.All(newTreasures, l => Assert.Equal(248, l.CurrentValue));
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static FieldItemLocation GetTreasureItem(byte[] file, int value) =>
            FieldParser.FindItemLocations(file)
                .Single(l => l.LocationKind == FieldLocationKind.TreasureItem
                             && l.CurrentValue == value);

        private static FieldItemLocation GetTextSync(byte[] file, int value) =>
            FieldParser.FindItemLocations(file)
                .Single(l => l.LocationKind == FieldLocationKind.TextSync
                             && l.CurrentValue == value);
    }

    // ── FieldItemLocation.ItemCount / FieldItemScanner quantity tests ─────────

    /// <summary>
    /// Verifies that AddItem opcodes with a constant count argument are read
    /// correctly by FieldParser and accumulated correctly by FieldItemScanner.
    ///
    /// Uses synthetic .eb.bytes files built in-memory — no real game files needed.
    /// </summary>
    public sealed class FieldItemLocationItemCountTests
    {
        /// <summary>
        /// AddItem(113, 8) — both args constant (varargFlag = 0x00).
        /// Parser should expose ItemCount = 8 on the returned DirectItem location.
        /// </summary>
        [Fact]
        public void DirectItem_ConstantCount_IsReadFromBytecode()
        {
            // [0x48][varargFlag=0x00][item=113 lo][item hi=0][count=8 lo][count hi=0]
            byte[] file = BuildMinimalFieldFile(new byte[] { 0x48, 0x00, 113, 0x00, 8, 0x00 });

            var locs = FieldParser.FindItemLocations(file)
                .Where(l => l.LocationKind == FieldLocationKind.DirectItem
                            && l.CurrentValue == 113)
                .ToList();

            Assert.Single(locs);
            Assert.Equal(8, locs[0].ItemCount);
        }

        /// <summary>
        /// FieldItemScanner must sum loc.ItemCount, not count occurrences.
        /// One AddItem(113, 8) → scanner reports 8 copies, not 1.
        /// </summary>
        [Fact]
        public void FieldItemScanner_SumsItemCount_NotOccurrenceCount()
        {
            byte[] file = BuildMinimalFieldFile(new byte[] { 0x48, 0x00, 113, 0x00, 8, 0x00 });

            var counts = FieldItemScanner.ScanFiles(
                new[] { ("synthetic_evt_test.eb", file) });

            Assert.True(counts.TryGetValue(113, out int count),
                "Item 113 (Straw Hat) should be found in the synthetic file.");
            Assert.Equal(8, count);
        }

        /// <summary>
        /// AddItem(113, variable) — count arg is a variable expression (varargFlag bit 1 = 1).
        /// Parser cannot know the count, so ItemCount defaults to 1.
        /// </summary>
        [Fact]
        public void DirectItem_VariableCount_DefaultsToOne()
        {
            // varargFlag = 0x02 → item is constant, count is variable.
            // No count bytes follow (variable-length expression).
            byte[] file = BuildMinimalFieldFile(new byte[] { 0x48, 0x02, 113, 0x00 });

            var locs = FieldParser.FindItemLocations(file)
                .Where(l => l.LocationKind == FieldLocationKind.DirectItem
                            && l.CurrentValue == 113)
                .ToList();

            Assert.Single(locs);
            Assert.Equal(1, locs[0].ItemCount);
        }

        /// <summary>
        /// False-positive artefact: 0x48 appears in other opcode argument streams.
        /// When the bytes at pos+4 happen to encode a large value (e.g. 0x3E02 = 15874),
        /// the raw count must be clamped to MaxPlausibleItemCount (9) to prevent
        /// catastrophic inflation of FieldCounts in the catalog.
        /// </summary>
        [Fact]
        public void DirectItem_LargeCount_IsCappedAtMaxPlausible()
        {
            // count bytes = 0xAB, 0xCD → uint16 LE = 0xCDAB = 52651, well above cap
            byte[] file = BuildMinimalFieldFile(new byte[] { 0x48, 0x00, 113, 0x00, 0xAB, 0xCD });

            var locs = FieldParser.FindItemLocations(file)
                .Where(l => l.LocationKind == FieldLocationKind.DirectItem
                            && l.CurrentValue == 113)
                .ToList();

            Assert.Single(locs);
            // Must be capped at 9 (MaxPlausibleItemCount), not the raw 52651
            Assert.Equal(9, locs[0].ItemCount);
        }

        /// <summary>
        /// Non-DirectItem locations (TreasureItem, TextSync, DirectGil) always
        /// have ItemCount = 1 because they do not carry a quantity argument.
        /// </summary>
        [Fact]
        public void NonDirectItem_AlwaysHasItemCountOne()
        {
            // TreasureItem location from a real fixture
            byte[] house2 = File.ReadAllBytes(Path.Combine(
                AppContext.BaseDirectory, "TestData", "FieldParser",
                "evt_alex1_at_house_2.eb.bytes"));

            var nonDirect = FieldParser.FindItemLocations(house2)
                .Where(l => l.LocationKind != FieldLocationKind.DirectItem)
                .ToList();

            Assert.NotEmpty(nonDirect);
            Assert.All(nonDirect, l => Assert.Equal(1, l.ItemCount));
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a minimal valid .eb.bytes file with one entry, one function,
        /// and the supplied bytecode payload.
        ///
        /// Layout:
        ///   [0..127]          header (entryAmount=1, rest zero)
        ///   [128..135]        entry table (1 entry: offset=8, size=entryBodySize)
        ///   [136..136+sz-1]   entry body: [entryType=1][funcCount=1]
        ///                       [funcType(2)=0][funcPoint(2)=4][opcodeBytes...]
        /// </summary>
        private static byte[] BuildMinimalFieldFile(byte[] opcodeBytes)
        {
            // entryBodySize = 2 (type+funcCount) + 4 (1-function table) + payload
            int entryBodySize = 6 + opcodeBytes.Length;
            int entryOffset = 8; // offset from byte 128 to body start

            byte[] file = new byte[128 + 8 + entryBodySize];

            // Header: entryAmount at byte 3
            file[3] = 1;

            // Entry table at 128
            file[128] = (byte)(entryOffset & 0xFF);
            file[129] = (byte)((entryOffset >> 8) & 0xFF);
            file[130] = (byte)(entryBodySize & 0xFF);
            file[131] = (byte)((entryBodySize >> 8) & 0xFF);

            // Entry body at 136
            int bodyBase = 136;
            file[bodyBase + 0] = 1; // entryType
            file[bodyBase + 1] = 1; // funcCount = 1
            // funcType[0] = 0 (bytes +2,+3 already zero)
            file[bodyBase + 4] = 4; // funcPoint[0] = 4 → bytecode starts after 4-byte func table
            // funcPoint[0] hi (byte +5) already zero
            Array.Copy(opcodeBytes, 0, file, bodyBase + 6, opcodeBytes.Length);

            return file;
        }
    }
}