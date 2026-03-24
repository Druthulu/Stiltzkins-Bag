using System;
using System.Collections.Generic;
using System.Linq;
using StiltzkinsBag.Models;

namespace StiltzkinsBag.Randomizers
{
    /// <summary>
    /// Produces <see cref="ItemRemapTable"/> instances by shuffling item value pools.
    ///
    /// This class is a pure shuffle utility — it has no knowledge of field files,
    /// binary offsets, or CSV data. The caller (<c>FieldItemRandomizer</c>) is
    /// responsible for collecting the pools from scanned field locations and
    /// applying the resulting tables to the correct location kinds.
    ///
    /// ── Two-table design ────────────────────────────────────────────────────
    ///
    /// FF9 uses two distinct item-dispensing mechanisms in field scripts:
    ///
    ///   TreasureItem system  — the generic pickup handler (function 12 on the
    ///     player character entry). The script writes a value to the Treasure_Item
    ///     variable before triggering the handler. The handler then interprets the
    ///     value based on range:
    ///       X &lt; 512           → give item ID X
    ///       512 &lt;= X &lt; 1000  → give card slot (card ID = X - 512)
    ///       1000 &lt;= X &lt; 29999 → give gil (amount = X - 1000)
    ///     Because the handler performs the dispatch, any value type can be written
    ///     to any TreasureItem location. Full cross-shuffle (items, cards, gil) is
    ///     supported natively.
    ///
    ///   DirectItem system  — AddItem(X, n) opcode with a constant item ID.
    ///     Used by traditional treasure chests (handler function 14). The opcode
    ///     passes the value directly to the inventory system as a raw item ID.
    ///     There is no range-dispatch — card and gil encodings have no meaning here.
    ///     DirectItem locations are limited to item IDs (values &lt; 512).
    ///
    /// ── Usage pattern ───────────────────────────────────────────────────────
    ///
    ///   // In FieldItemRandomizer:
    ///   var treasureValues = locations
    ///       .Where(l => l.LocationKind == FieldLocationKind.TreasureItem)
    ///       .Select(l => l.CurrentValue)
    ///       .Distinct().ToList();
    ///
    ///   var directValues = locations
    ///       .Where(l => l.LocationKind == FieldLocationKind.DirectItem)
    ///       .Select(l => l.CurrentValue)
    ///       .Distinct().ToList();
    ///
    ///   ItemRemapTable treasureTable = ItemRemapper.ShuffleTreasurePool(treasureValues, rng);
    ///   ItemRemapTable directTable   = ItemRemapper.ShuffleItemPool(directValues, rng);
    ///
    ///   // Apply:
    ///   //   TreasureItem + TextSync(paired with treasure) → treasureTable
    ///   //   DirectItem   + TextSync(paired with chest)   → directTable
    ///   //   Unknown values in either table               → passthrough (unchanged)
    /// </summary>
    public static class ItemRemapper
    {
        // Maximum valid item ID in FF9. Values at or above this threshold are
        // card slots or gil encodings and are never valid raw item IDs.
        private const int MaxItemId = 512;

        /// <summary>
        /// Shuffles all provided values (item IDs, card slots, and gil encodings)
        /// among themselves using Fisher-Yates, producing a bijective remapping.
        ///
        /// Designed for TreasureItem and TextSync locations. Any value type
        /// (item, card, gil) can map to any other value type in the output.
        ///
        /// Duplicate values in <paramref name="pool"/> are deduplicated before
        /// shuffling — each distinct value appears exactly once in the mapping.
        /// </summary>
        /// <param name="pool">
        /// Distinct or non-distinct values from TreasureItem locations.
        /// Duplicates are removed automatically.
        /// </param>
        /// <param name="rng">
        /// The seeded <see cref="Random"/> instance from <c>SeedEngine</c>.
        /// The caller must not reuse this instance for other purposes between
        /// calls if deterministic ordering across multiple tables is required.
        /// </param>
        /// <returns>
        /// A populated <see cref="ItemRemapTable"/> mapping each original value
        /// to a new value drawn from the same pool.
        /// Returns <see cref="ItemRemapTable.Passthrough()"/> if the pool is empty.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="pool"/> or <paramref name="rng"/> is null.
        /// </exception>
        public static ItemRemapTable ShuffleTreasurePool(IEnumerable<int> pool, Random rng)
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            List<int> keys = pool.Distinct().OrderBy(v => v).ToList();
            return BuildTable(keys, rng);
        }

        /// <summary>
        /// Shuffles item IDs (values strictly less than 512) among themselves.
        /// Non-item values (card slots &gt;= 512, gil encodings &gt;= 1000) in the
        /// input are silently filtered out — they will passthrough unchanged via
        /// <see cref="ItemRemapTable.Remap"/> since they are not in the table.
        ///
        /// Designed for DirectItem locations (AddItem constant calls in chests)
        /// and for <c>EnemyRandomizer</c> drop/steal tables which use raw item IDs.
        /// </summary>
        /// <param name="pool">
        /// Values from DirectItem or enemy drop locations.
        /// Non-item values are filtered; duplicates are removed.
        /// </param>
        /// <param name="rng">
        /// The seeded <see cref="Random"/> instance from <c>SeedEngine</c>.
        /// </param>
        /// <returns>
        /// A populated <see cref="ItemRemapTable"/> mapping each item ID to another
        /// item ID drawn from the same filtered pool.
        /// Returns <see cref="ItemRemapTable.Passthrough()"/> if no item IDs remain
        /// after filtering.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="pool"/> or <paramref name="rng"/> is null.
        /// </exception>
        public static ItemRemapTable ShuffleItemPool(IEnumerable<int> pool, Random rng)
        {
            if (pool == null) throw new ArgumentNullException(nameof(pool));
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            List<int> keys = pool
                .Where(v => v < MaxItemId)
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            return BuildTable(keys, rng);
        }

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Core shuffle: Fisher-Yates on the values list, then zip keys → shuffled values.
        /// Sorting keys before shuffling ensures the Random draw order is deterministic
        /// regardless of the order in which the caller collected the values.
        /// </summary>
        private static ItemRemapTable BuildTable(List<int> keys, Random rng)
        {
            if (keys.Count == 0)
                return ItemRemapTable.Passthrough();

            // Shuffle a copy of the keys to produce the value permutation.
            // Fisher-Yates: iterate from last to first, swap with a random earlier element.
            List<int> values = new List<int>(keys);
            for (int i = values.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (values[i], values[j]) = (values[j], values[i]);
            }

            var map = new Dictionary<int, int>(keys.Count);
            for (int i = 0; i < keys.Count; i++)
                map[keys[i]] = values[i];

            return new ItemRemapTable(map);
        }
    }
}