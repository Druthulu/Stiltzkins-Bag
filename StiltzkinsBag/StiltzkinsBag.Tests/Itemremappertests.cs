using System;
using System.Collections.Generic;
using System.Linq;
using StiltzkinsBag.Models;
using StiltzkinsBag.Randomizers;
using Xunit;

namespace StiltzkinsBag.Tests.Randomizers
{
    /// <summary>
    /// Tests for ItemRemapper — the shuffle utility that produces ItemRemapTable instances.
    ///
    /// All tests use in-memory data only — no binary files required.
    ///
    /// Test pools are drawn from values actually found in the 7 test field files:
    ///   Items:  236 (Potion), 240 (Phoenix Down), 244 (Eye Drops), 249 (Phoenix Pinion)
    ///   Cards:  513 (Fang), 517 (card5), 518 (card6), 521 (card9)
    ///   Gil:    1003 (3g), 1009 (9g), 1038 (38g), 1047 (47g)
    /// </summary>
    public sealed class ItemRemapperTests
    {
        // ── Representative pools from actual field scan data ──────────────────

        private static readonly List<int> ItemValues = new() { 236, 240, 244, 249 };
        private static readonly List<int> CardValues = new() { 513, 517, 518, 521 };
        private static readonly List<int> GilValues = new() { 1003, 1009, 1038, 1047 };
        private static readonly List<int> MixedValues =
            ItemValues.Concat(CardValues).Concat(GilValues).ToList();

        // ── ShuffleTreasurePool ───────────────────────────────────────────────

        public sealed class ShuffleTreasurePoolTests
        {
            [Fact]
            public void NullPool_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(
                    () => ItemRemapper.ShuffleTreasurePool(null!, new Random(1)));
            }

            [Fact]
            public void NullRng_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(
                    () => ItemRemapper.ShuffleTreasurePool(MixedValues, null!));
            }

            [Fact]
            public void EmptyPool_ReturnsPassthroughTable()
            {
                var table = ItemRemapper.ShuffleTreasurePool(
                    Array.Empty<int>(), new Random(1));
                Assert.False(table.HasRemappings);
                Assert.Equal(42, table.Remap(42)); // unknown ID passes through
            }

            [Fact]
            public void SingleValue_MapsToItself()
            {
                // A pool of one element can only map to itself
                var table = ItemRemapper.ShuffleTreasurePool(new[] { 236 }, new Random(1));
                Assert.Equal(1, table.Count);
                Assert.Equal(236, table.Remap(236));
            }

            // ── Bijectivity ───────────────────────────────────────────────────
            // The output must be a permutation of the input — every input value
            // appears exactly once in the mapping, both as key and as a value.

            [Fact]
            public void MixedPool_OutputIsPermutationOfInput()
            {
                var table = ItemRemapper.ShuffleTreasurePool(MixedValues, new Random(42));

                // Every original value must be a key in the table
                foreach (int v in MixedValues)
                    Assert.True(table.Count > 0, $"Table should not be empty");

                // Collect all mapped values
                var outputValues = MixedValues.Select(v => table.Remap(v)).ToList();

                // Output set must equal input set (same elements, possibly reordered)
                Assert.Equal(
                    MixedValues.OrderBy(x => x).ToList(),
                    outputValues.OrderBy(x => x).ToList());
            }

            [Fact]
            public void ItemOnlyPool_OutputIsPermutationOfInput()
            {
                var table = ItemRemapper.ShuffleTreasurePool(ItemValues, new Random(42));
                var outputValues = ItemValues.Select(v => table.Remap(v)).OrderBy(x => x).ToList();
                Assert.Equal(ItemValues.OrderBy(x => x).ToList(), outputValues);
            }

            [Fact]
            public void CardOnlyPool_OutputIsPermutationOfInput()
            {
                var table = ItemRemapper.ShuffleTreasurePool(CardValues, new Random(42));
                var outputValues = CardValues.Select(v => table.Remap(v)).OrderBy(x => x).ToList();
                Assert.Equal(CardValues.OrderBy(x => x).ToList(), outputValues);
            }

            [Fact]
            public void GilOnlyPool_OutputIsPermutationOfInput()
            {
                var table = ItemRemapper.ShuffleTreasurePool(GilValues, new Random(42));
                var outputValues = GilValues.Select(v => table.Remap(v)).OrderBy(x => x).ToList();
                Assert.Equal(GilValues.OrderBy(x => x).ToList(), outputValues);
            }

            [Fact]
            public void MixedPool_TableCountMatchesDistinctInputCount()
            {
                var table = ItemRemapper.ShuffleTreasurePool(MixedValues, new Random(1));
                Assert.Equal(MixedValues.Distinct().Count(), table.Count);
            }

            // ── Cross-shuffle: items, cards, and gil can swap with each other ─

            [Fact]
            public void MixedPool_ItemCanMapToCard()
            {
                // Run enough seeds until we observe an item mapping to a card value.
                // With 12 values in the pool the probability per seed is high.
                bool foundItemToCard = false;
                for (int seed = 0; seed < 200 && !foundItemToCard; seed++)
                {
                    var table = ItemRemapper.ShuffleTreasurePool(MixedValues, new Random(seed));
                    foreach (int item in ItemValues)
                    {
                        int mapped = table.Remap(item);
                        if (CardValues.Contains(mapped)) { foundItemToCard = true; break; }
                    }
                }
                Assert.True(foundItemToCard,
                    "Expected at least one seed where an item maps to a card slot");
            }

            [Fact]
            public void MixedPool_ItemCanMapToGil()
            {
                bool foundItemToGil = false;
                for (int seed = 0; seed < 200 && !foundItemToGil; seed++)
                {
                    var table = ItemRemapper.ShuffleTreasurePool(MixedValues, new Random(seed));
                    foreach (int item in ItemValues)
                    {
                        int mapped = table.Remap(item);
                        if (GilValues.Contains(mapped)) { foundItemToGil = true; break; }
                    }
                }
                Assert.True(foundItemToGil,
                    "Expected at least one seed where an item maps to a gil encoding");
            }

            [Fact]
            public void MixedPool_CardCanMapToItem()
            {
                bool foundCardToItem = false;
                for (int seed = 0; seed < 200 && !foundCardToItem; seed++)
                {
                    var table = ItemRemapper.ShuffleTreasurePool(MixedValues, new Random(seed));
                    foreach (int card in CardValues)
                    {
                        int mapped = table.Remap(card);
                        if (ItemValues.Contains(mapped)) { foundCardToItem = true; break; }
                    }
                }
                Assert.True(foundCardToItem,
                    "Expected at least one seed where a card maps to an item");
            }

            // ── Determinism ───────────────────────────────────────────────────

            [Fact]
            public void SameSeed_ProducesSameTable()
            {
                var t1 = ItemRemapper.ShuffleTreasurePool(MixedValues, new Random(99));
                var t2 = ItemRemapper.ShuffleTreasurePool(MixedValues, new Random(99));

                foreach (int v in MixedValues)
                    Assert.Equal(t1.Remap(v), t2.Remap(v));
            }

            [Fact]
            public void DifferentSeeds_ProduceDifferentTables()
            {
                // Not guaranteed for every pair, but highly probable with 12 values
                var t1 = ItemRemapper.ShuffleTreasurePool(MixedValues, new Random(1));
                var t2 = ItemRemapper.ShuffleTreasurePool(MixedValues, new Random(2));

                bool anyDifference = MixedValues.Any(v => t1.Remap(v) != t2.Remap(v));
                Assert.True(anyDifference,
                    "Different seeds should produce different tables for a 12-element pool");
            }

            [Fact]
            public void InputOrderDoesNotAffectOutput()
            {
                // Sorted vs reversed input must produce identical tables for the same seed
                var sorted = MixedValues.OrderBy(x => x).ToList();
                var reversed = MixedValues.OrderByDescending(x => x).ToList();

                var t1 = ItemRemapper.ShuffleTreasurePool(sorted, new Random(7));
                var t2 = ItemRemapper.ShuffleTreasurePool(reversed, new Random(7));

                foreach (int v in MixedValues)
                    Assert.Equal(t1.Remap(v), t2.Remap(v));
            }

            // ── Deduplication ─────────────────────────────────────────────────

            [Fact]
            public void DuplicateInputValues_AreDeduplicatedBeforeShuffling()
            {
                var withDupes = new[] { 236, 236, 240, 240, 513, 513 };
                var table = ItemRemapper.ShuffleTreasurePool(withDupes, new Random(1));
                Assert.Equal(3, table.Count); // only 3 distinct values
            }

            // ── Passthrough for unknown values ─────────────────────────────────

            [Fact]
            public void UnknownValue_PassesThroughUnchanged()
            {
                var table = ItemRemapper.ShuffleTreasurePool(MixedValues, new Random(1));
                Assert.Equal(999, table.Remap(999));   // not in pool
                Assert.Equal(2000, table.Remap(2000)); // not in pool
            }
        }

        // ── ShuffleItemPool ───────────────────────────────────────────────────

        public sealed class ShuffleItemPoolTests
        {
            [Fact]
            public void NullPool_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(
                    () => ItemRemapper.ShuffleItemPool(null!, new Random(1)));
            }

            [Fact]
            public void NullRng_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(
                    () => ItemRemapper.ShuffleItemPool(ItemValues, null!));
            }

            [Fact]
            public void EmptyPool_ReturnsPassthroughTable()
            {
                var table = ItemRemapper.ShuffleItemPool(Array.Empty<int>(), new Random(1));
                Assert.False(table.HasRemappings);
            }

            // ── Card and gil values are filtered out ──────────────────────────

            [Fact]
            public void CardValues_AreFilteredFromPool()
            {
                // A pool of only card slots produces an empty (passthrough) table
                var table = ItemRemapper.ShuffleItemPool(CardValues, new Random(1));
                Assert.Equal(0, table.Count);
                Assert.False(table.HasRemappings);
            }

            [Fact]
            public void GilValues_AreFilteredFromPool()
            {
                var table = ItemRemapper.ShuffleItemPool(GilValues, new Random(1));
                Assert.Equal(0, table.Count);
            }

            [Fact]
            public void MixedPool_OnlyItemsInTable()
            {
                var table = ItemRemapper.ShuffleItemPool(MixedValues, new Random(1));
                // Only the 4 item IDs should be in the table
                Assert.Equal(ItemValues.Count, table.Count);
            }

            [Fact]
            public void MixedPool_CardValuesPassThroughUnchanged()
            {
                var table = ItemRemapper.ShuffleItemPool(MixedValues, new Random(1));
                foreach (int card in CardValues)
                    Assert.Equal(card, table.Remap(card));
            }

            [Fact]
            public void MixedPool_GilValuesPassThroughUnchanged()
            {
                var table = ItemRemapper.ShuffleItemPool(MixedValues, new Random(1));
                foreach (int gil in GilValues)
                    Assert.Equal(gil, table.Remap(gil));
            }

            // ── Bijectivity among item IDs ────────────────────────────────────

            [Fact]
            public void ItemPool_OutputIsPermutationOfItemIds()
            {
                var table = ItemRemapper.ShuffleItemPool(MixedValues, new Random(42));
                var outputValues = ItemValues.Select(v => table.Remap(v)).OrderBy(x => x).ToList();
                Assert.Equal(ItemValues.OrderBy(x => x).ToList(), outputValues);
            }

            [Fact]
            public void ItemPool_OutputValuesAreAllItemIds()
            {
                var table = ItemRemapper.ShuffleItemPool(MixedValues, new Random(42));
                foreach (int item in ItemValues)
                {
                    int mapped = table.Remap(item);
                    Assert.True(mapped < 512,
                        $"Item ID {item} mapped to {mapped} which is not a valid item ID");
                }
            }

            // ── Determinism ───────────────────────────────────────────────────

            [Fact]
            public void SameSeed_ProducesSameTable()
            {
                var t1 = ItemRemapper.ShuffleItemPool(MixedValues, new Random(99));
                var t2 = ItemRemapper.ShuffleItemPool(MixedValues, new Random(99));
                foreach (int v in ItemValues)
                    Assert.Equal(t1.Remap(v), t2.Remap(v));
            }

            [Fact]
            public void InputOrderDoesNotAffectOutput()
            {
                var sorted = MixedValues.OrderBy(x => x).ToList();
                var reversed = MixedValues.OrderByDescending(x => x).ToList();
                var t1 = ItemRemapper.ShuffleItemPool(sorted, new Random(7));
                var t2 = ItemRemapper.ShuffleItemPool(reversed, new Random(7));
                foreach (int v in ItemValues)
                    Assert.Equal(t1.Remap(v), t2.Remap(v));
            }

            // ── Boundary: value 511 is an item, 512 is a card slot ────────────

            [Fact]
            public void Value511_IsIncludedAsItemId()
            {
                var pool = new[] { 511, 240 };
                var table = ItemRemapper.ShuffleItemPool(pool, new Random(1));
                Assert.Equal(2, table.Count);
                Assert.True(table.HasRemappings);
            }

            [Fact]
            public void Value512_IsFilteredAsCardSlot()
            {
                var pool = new[] { 512, 240 };
                var table = ItemRemapper.ShuffleItemPool(pool, new Random(1));
                Assert.Equal(1, table.Count); // only 240
                Assert.Equal(512, table.Remap(512)); // passthrough
            }
        }
    }
}