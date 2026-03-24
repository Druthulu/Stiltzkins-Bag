using System;
using System.Collections.Generic;
using System.Linq;
using StiltzkinsBag.Core.Models.Battle;
using StiltzkinsBag.Models;
using StiltzkinsBag.Randomizers;
using Xunit;

namespace StiltzkinsBag.Tests.Randomizers
{
    /// <summary>
    /// Tests for EnemyRandomizer.
    ///
    /// All tests use in-memory EnemyFile instances built from minimal valid byte arrays —
    /// no binary test data files required.
    ///
    /// EnemyFile binary layout (vanilla, header + groups + stats):
    ///   [0] version=1, [1] group_amount, [2] stat_amount, [3] spell_amount,
    ///   [4-5] flag=0, [6-7] zero padding
    ///   Then group_amount * 56 bytes (group section)
    ///   Then stat_amount * 116 bytes (stat section)
    ///
    /// Intra-stat offsets for fields we test:
    ///   drop[0..3]  → +20..+23
    ///   steal[0..3] → +24..+27
    ///   blue_magic  → +71
    ///   card_drop   → +105
    /// </summary>
    public sealed class EnemyRandomizerTests
    {
        // ── EnemyFile builder ─────────────────────────────────────────────────

        private const int HeaderSize = 8;
        private const int GroupSize = 56;
        private const int StatSize = 116;

        private const int StatOffDrop0 = 20;
        private const int StatOffSteal0 = 24;
        private const int StatOffBlueMagic = 71;
        private const int StatOffCardDrop = 105;

        /// <summary>
        /// Builds a minimal valid EnemyFile byte array with the given parameters.
        /// All unspecified bytes are zero.
        /// </summary>
        private static EnemyFile MakeFile(
            int groupCount, int statCount,
            Action<byte[], int>? initStat = null)
        {
            int size = HeaderSize + groupCount * GroupSize + statCount * StatSize;
            var data = new byte[size];
            data[0] = 1;                        // version = 1 (vanilla)
            data[1] = (byte)groupCount;
            data[2] = (byte)statCount;
            data[3] = 0;                        // spell_amount

            // Let caller populate stat blocks
            if (initStat != null)
                for (int s = 0; s < statCount; s++)
                {
                    int statBase = HeaderSize + groupCount * GroupSize + s * StatSize;
                    initStat(data, statBase);
                }

            return new EnemyFile(data);
        }

        /// <summary>Returns the stat base offset within the file bytes.</summary>
        private static int StatBase(int groupCount, int statIndex) =>
            HeaderSize + groupCount * GroupSize + statIndex * StatSize;

        // ── RemapDropsAndSteals ───────────────────────────────────────────────

        public sealed class RemapDropsAndStealsTests
        {
            [Fact]
            public void NullFiles_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    EnemyRandomizer.RemapDropsAndSteals(null!, ItemRemapTable.Passthrough()));
            }

            [Fact]
            public void NullTable_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    EnemyRandomizer.RemapDropsAndSteals(new List<EnemyFile>(), null!));
            }

            [Fact]
            public void PassthroughTable_DoesNotChangeAnyValues()
            {
                var file = MakeFile(1, 1, (data, b) =>
                {
                    data[b + StatOffDrop0] = 236; // Potion
                    data[b + StatOffDrop0 + 1] = 240; // Phoenix Down
                    data[b + StatOffSteal0] = 244; // Eye Drops
                    data[b + StatOffSteal0 + 1] = 249; // Phoenix Pinion
                });

                EnemyRandomizer.RemapDropsAndSteals(
                    new[] { file }, ItemRemapTable.Passthrough());

                Assert.Equal(236, file.GetDrop(0, 0));
                Assert.Equal(240, file.GetDrop(0, 1));
                Assert.Equal(244, file.GetSteal(0, 0));
                Assert.Equal(249, file.GetSteal(0, 1));
            }

            [Fact]
            public void PopulatedTable_RemapsDropSlots()
            {
                var file = MakeFile(1, 1, (data, b) =>
                {
                    data[b + StatOffDrop0] = 236; // Potion → should become 240
                    data[b + StatOffDrop0 + 1] = 240; // Phoenix Down → should become 236
                });

                var table = new ItemRemapTable(new Dictionary<int, int>
                {
                    { 236, 240 },
                    { 240, 236 },
                });
                EnemyRandomizer.RemapDropsAndSteals(new[] { file }, table);

                Assert.Equal(240, file.GetDrop(0, 0));
                Assert.Equal(236, file.GetDrop(0, 1));
            }

            [Fact]
            public void PopulatedTable_RemapsStealSlots()
            {
                var file = MakeFile(1, 1, (data, b) =>
                {
                    data[b + StatOffSteal0] = 244; // Eye Drops → 249
                    data[b + StatOffSteal0 + 1] = 249; // Phoenix Pinion → 244
                });

                var table = new ItemRemapTable(new Dictionary<int, int>
                {
                    { 244, 249 },
                    { 249, 244 },
                });
                EnemyRandomizer.RemapDropsAndSteals(new[] { file }, table);

                Assert.Equal(249, file.GetSteal(0, 0));
                Assert.Equal(244, file.GetSteal(0, 1));
            }

            [Fact]
            public void ZeroSlots_AreNeverRemapped()
            {
                // Slots 2 and 3 are 0 (no item) — must remain 0 even with a populated table
                var file = MakeFile(1, 1, (data, b) =>
                {
                    data[b + StatOffDrop0] = 236;
                    data[b + StatOffDrop0 + 1] = 0;   // no item
                    data[b + StatOffDrop0 + 2] = 0;   // no item
                    data[b + StatOffDrop0 + 3] = 0;   // no item
                });

                // A table that would remap 0 to something (should never be called)
                var table = new ItemRemapTable(new Dictionary<int, int>
                {
                    { 236, 240 },
                    // 0 intentionally not in table — Remap(0) returns 0 via passthrough
                });
                EnemyRandomizer.RemapDropsAndSteals(new[] { file }, table);

                Assert.Equal(0, file.GetDrop(0, 1));
                Assert.Equal(0, file.GetDrop(0, 2));
                Assert.Equal(0, file.GetDrop(0, 3));
                Assert.Equal(240, file.GetDrop(0, 0)); // the non-zero slot was remapped
            }

            [Fact]
            public void UnknownItemId_PassesThroughUnchanged()
            {
                // Item ID not in the table → Remap returns it unchanged
                var file = MakeFile(1, 1, (data, b) =>
                {
                    data[b + StatOffDrop0] = 200; // not in table
                });

                var table = new ItemRemapTable(new Dictionary<int, int>
                {
                    { 236, 240 }, // only 236 is mapped
                });
                EnemyRandomizer.RemapDropsAndSteals(new[] { file }, table);

                Assert.Equal(200, file.GetDrop(0, 0)); // unchanged
            }

            [Fact]
            public void MultipleFiles_AllAreRemapped()
            {
                var file1 = MakeFile(1, 1, (data, b) => data[b + StatOffDrop0] = 236);
                var file2 = MakeFile(1, 1, (data, b) => data[b + StatOffDrop0] = 236);

                var table = new ItemRemapTable(new Dictionary<int, int> { { 236, 240 } });
                EnemyRandomizer.RemapDropsAndSteals(new[] { file1, file2 }, table);

                Assert.Equal(240, file1.GetDrop(0, 0));
                Assert.Equal(240, file2.GetDrop(0, 0));
            }

            [Fact]
            public void MultipleStatBlocks_AllAreRemapped()
            {
                var file = MakeFile(1, 3, (data, b) =>
                {
                    data[b + StatOffDrop0] = 236;
                });

                var table = new ItemRemapTable(new Dictionary<int, int> { { 236, 240 } });
                EnemyRandomizer.RemapDropsAndSteals(new[] { file }, table);

                for (int s = 0; s < 3; s++)
                    Assert.Equal(240, file.GetDrop(s, 0));
            }

            [Fact]
            public void EmptyFileList_DoesNotThrow()
            {
                // Should complete without exceptions
                EnemyRandomizer.RemapDropsAndSteals(
                    new List<EnemyFile>(), ItemRemapTable.Passthrough());
            }
        }

        // ── ShuffleBlueMagic ──────────────────────────────────────────────────

        public sealed class ShuffleBlueMagicTests
        {
            [Fact]
            public void NullFiles_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    EnemyRandomizer.ShuffleBlueMagic(null!, new Random(1)));
            }

            [Fact]
            public void NullRng_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    EnemyRandomizer.ShuffleBlueMagic(new List<EnemyFile>(), null!));
            }

            [Fact]
            public void EmptyFileList_DoesNotThrow()
            {
                EnemyRandomizer.ShuffleBlueMagic(new List<EnemyFile>(), new Random(1));
            }

            [Fact]
            public void AllZeroBlueMagic_NoChanges()
            {
                var file = MakeFile(1, 3); // all blue_magic = 0
                EnemyRandomizer.ShuffleBlueMagic(new[] { file }, new Random(1));
                for (int s = 0; s < 3; s++)
                    Assert.Equal(0, file.GetBlueMagic(s));
            }

            [Fact]
            public void SingleNonZero_MapsToItself()
            {
                var file = MakeFile(1, 2, (data, b) =>
                {
                    // Only stat 0 has blue magic; set first stat to 10
                    // We call initStat for every stat, but check b against header
                });

                // Simpler: build directly
                int size = HeaderSize + GroupSize + 2 * StatSize;
                var data = new byte[size];
                data[0] = 1; data[1] = 1; data[2] = 2;
                data[HeaderSize + GroupSize + StatOffBlueMagic] = 10; // stat 0: Fire
                // stat 1: 0 (no blue magic)
                var f = new EnemyFile(data);

                EnemyRandomizer.ShuffleBlueMagic(new[] { f }, new Random(1));

                Assert.Equal(10, f.GetBlueMagic(0)); // only one value, maps to itself
                Assert.Equal(0, f.GetBlueMagic(1)); // still empty
            }

            [Fact]
            public void Shuffle_IsPermutationOfOriginalValues()
            {
                // Three enemies with distinct blue magic spells
                byte[] original = { 10, 20, 30 };
                var files = original.Select(bm =>
                {
                    int sz = HeaderSize + GroupSize + StatSize;
                    var d = new byte[sz];
                    d[0] = 1; d[1] = 1; d[2] = 1;
                    d[HeaderSize + GroupSize + StatOffBlueMagic] = bm;
                    return new EnemyFile(d);
                }).ToList();

                EnemyRandomizer.ShuffleBlueMagic(files, new Random(42));

                var result = files.Select(f => f.GetBlueMagic(0)).OrderBy(x => x).ToList();
                Assert.Equal(new byte[] { 10, 20, 30 }, result);
            }

            [Fact]
            public void ZeroSlots_RemainZeroAfterShuffle()
            {
                // Mix of zero and non-zero blue magic
                var data1 = BuildStatFile(1, 1, bm: 10);
                var data2 = BuildStatFile(1, 1, bm: 0);  // no blue magic
                var data3 = BuildStatFile(1, 1, bm: 20);

                var files = new[] { new EnemyFile(data1), new EnemyFile(data2), new EnemyFile(data3) };
                EnemyRandomizer.ShuffleBlueMagic(files, new Random(1));

                Assert.Equal(0, files[1].GetBlueMagic(0)); // unchanged
                // The other two must still hold 10 and 20 (permuted)
                var nonZero = new[] { files[0].GetBlueMagic(0), files[2].GetBlueMagic(0) }
                    .OrderBy(x => x).ToList();
                Assert.Equal(new byte[] { 10, 20 }, nonZero);
            }

            [Fact]
            public void SameSeed_ProducesSameResult()
            {
                byte[] bms = { 10, 20, 30, 40, 50 };

                var files1 = bms.Select(bm => new EnemyFile(BuildStatFile(1, 1, bm: bm))).ToList();
                var files2 = bms.Select(bm => new EnemyFile(BuildStatFile(1, 1, bm: bm))).ToList();

                EnemyRandomizer.ShuffleBlueMagic(files1, new Random(77));
                EnemyRandomizer.ShuffleBlueMagic(files2, new Random(77));

                for (int i = 0; i < files1.Count; i++)
                    Assert.Equal(files1[i].GetBlueMagic(0), files2[i].GetBlueMagic(0));
            }

            [Fact]
            public void CrossFile_Shuffle_IsSupported()
            {
                // Blue magic from file1 can end up in file2 and vice versa
                bool foundCrossFile = false;

                for (int seed = 0; seed < 100 && !foundCrossFile; seed++)
                {
                    var f1 = new EnemyFile(BuildStatFile(1, 1, bm: 10));
                    var f2 = new EnemyFile(BuildStatFile(1, 1, bm: 20));
                    EnemyRandomizer.ShuffleBlueMagic(new[] { f1, f2 }, new Random(seed));

                    if (f1.GetBlueMagic(0) == 20 && f2.GetBlueMagic(0) == 10)
                        foundCrossFile = true;
                }
                Assert.True(foundCrossFile,
                    "Expected at least one seed to swap blue magic between files");
            }
        }

        // ── ShuffleCardDrops ──────────────────────────────────────────────────

        public sealed class ShuffleCardDropsTests
        {
            [Fact]
            public void NullFiles_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    EnemyRandomizer.ShuffleCardDrops(null!, new Random(1)));
            }

            [Fact]
            public void NullRng_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    EnemyRandomizer.ShuffleCardDrops(new List<EnemyFile>(), null!));
            }

            [Fact]
            public void EmptyFileList_DoesNotThrow()
            {
                EnemyRandomizer.ShuffleCardDrops(new List<EnemyFile>(), new Random(1));
            }

            [Fact]
            public void AllZeroCardDrops_NoChanges()
            {
                var file = MakeFile(1, 3); // all card_drop = 0
                EnemyRandomizer.ShuffleCardDrops(new[] { file }, new Random(1));
                for (int s = 0; s < 3; s++)
                    Assert.Equal(0, file.GetCardDrop(s));
            }

            [Fact]
            public void Shuffle_IsPermutationOfOriginalValues()
            {
                byte[] original = { 1, 5, 12 };
                var files = original.Select(card =>
                    new EnemyFile(BuildStatFile(1, 1, cardDrop: card))).ToList();

                EnemyRandomizer.ShuffleCardDrops(files, new Random(42));

                var result = files.Select(f => f.GetCardDrop(0)).OrderBy(x => x).ToList();
                Assert.Equal(new byte[] { 1, 5, 12 }, result);
            }

            [Fact]
            public void ZeroSlots_RemainZeroAfterShuffle()
            {
                var f1 = new EnemyFile(BuildStatFile(1, 1, cardDrop: 5));
                var f2 = new EnemyFile(BuildStatFile(1, 1, cardDrop: 0));
                var f3 = new EnemyFile(BuildStatFile(1, 1, cardDrop: 12));

                EnemyRandomizer.ShuffleCardDrops(new[] { f1, f2, f3 }, new Random(1));

                Assert.Equal(0, f2.GetCardDrop(0)); // unchanged
                var nonZero = new[] { f1.GetCardDrop(0), f3.GetCardDrop(0) }
                    .OrderBy(x => x).ToList();
                Assert.Equal(new byte[] { 5, 12 }, nonZero);
            }

            [Fact]
            public void SameSeed_ProducesSameResult()
            {
                byte[] cards = { 1, 5, 12, 33, 67 };

                var files1 = cards.Select(c => new EnemyFile(BuildStatFile(1, 1, cardDrop: c))).ToList();
                var files2 = cards.Select(c => new EnemyFile(BuildStatFile(1, 1, cardDrop: c))).ToList();

                EnemyRandomizer.ShuffleCardDrops(files1, new Random(55));
                EnemyRandomizer.ShuffleCardDrops(files2, new Random(55));

                for (int i = 0; i < files1.Count; i++)
                    Assert.Equal(files1[i].GetCardDrop(0), files2[i].GetCardDrop(0));
            }

            [Fact]
            public void BlueMagic_IsNotAffectedByCardDropShuffle()
            {
                var file = new EnemyFile(BuildStatFile(1, 1, bm: 10, cardDrop: 5));
                EnemyRandomizer.ShuffleCardDrops(new[] { file }, new Random(1));
                Assert.Equal(10, file.GetBlueMagic(0)); // unchanged
            }

            [Fact]
            public void CardDrops_AreNotAffectedByBlueMagicShuffle()
            {
                var file = new EnemyFile(BuildStatFile(1, 1, bm: 10, cardDrop: 5));
                EnemyRandomizer.ShuffleBlueMagic(new[] { file }, new Random(1));
                Assert.Equal(5, file.GetCardDrop(0)); // unchanged
            }
        }

        // ── Test helpers ──────────────────────────────────────────────────────

        /// <summary>
        /// Builds a minimal valid enemy file byte array with one group and the
        /// given number of stat blocks, setting optional fields in stat 0.
        /// </summary>
        private static byte[] BuildStatFile(
            int groupCount, int statCount,
            byte bm = 0,
            byte cardDrop = 0,
            byte drop0 = 0,
            byte steal0 = 0)
        {
            int size = HeaderSize + groupCount * GroupSize + statCount * StatSize;
            var data = new byte[size];
            data[0] = 1;                    // version
            data[1] = (byte)groupCount;
            data[2] = (byte)statCount;

            int statBase = HeaderSize + groupCount * GroupSize; // stat 0
            data[statBase + StatOffBlueMagic] = bm;
            data[statBase + StatOffCardDrop] = cardDrop;
            data[statBase + StatOffDrop0] = drop0;
            data[statBase + StatOffSteal0] = steal0;

            return data;
        }
    }
}