using System;
using System.Collections.Generic;
using StiltzkinsBag.Core.Models.Battle;
using StiltzkinsBag.Models;

namespace StiltzkinsBag.Randomizers
{
    /// <summary>
    /// Randomizes enemy drop tables, steal tables, blue magic assignments,
    /// and card drop assignments across a collection of <see cref="EnemyFile"/> instances.
    ///
    /// ── Three independent operations ────────────────────────────────────────
    ///
    /// <see cref="RemapDropsAndSteals"/> — applies an <see cref="ItemRemapTable"/>
    ///   to every non-zero drop and steal slot across all stat blocks in all files.
    ///   Item ID 0 ("no item") is always preserved unchanged.
    ///   Callers should use <see cref="ItemRemapTable.Passthrough()"/> when the
    ///   item shuffle feature is disabled, so this method can be called unconditionally.
    ///
    /// <see cref="ShuffleBlueMagic"/> — collects all non-zero blue magic ability IDs
    ///   across all stat blocks in all files, shuffles them with Fisher-Yates, then
    ///   distributes the shuffled values back to the same slots. Blue magic IDs are
    ///   ability IDs, not item IDs — they do not go through ItemRemapTable.
    ///   Enemies that had no blue magic (value 0) remain empty after the shuffle.
    ///
    /// <see cref="ShuffleCardDrops"/> — same two-pass pattern as ShuffleBlueMagic,
    ///   applied to the Tetramaster card drop slot. Card IDs are raw card numbers
    ///   (0–~100), not item IDs, and are shuffled within their own pool.
    ///
    /// ── Determinism ─────────────────────────────────────────────────────────
    ///
    /// The two-pass shuffle methods (ShuffleBlueMagic, ShuffleCardDrops) visit
    /// files in the order provided. To guarantee seed-identical results across
    /// runs, the caller must pass files in a consistent order — typically sorted
    /// by <see cref="EnemyFile.SourcePath"/>.
    ///
    /// Within each file, stat blocks are visited 0..StatCount-1 in order.
    ///
    /// ── Caller responsibility ────────────────────────────────────────────────
    ///
    /// Each method is independent. The orchestrator (e.g. a generation pipeline
    /// class) is responsible for invoking only the methods that Settings enables.
    /// No settings checks are performed inside this class.
    ///
    /// All modifications are made in-place on the <see cref="EnemyFile"/> objects.
    /// Call <see cref="EnemyFile.ToBytes()"/> on each file after randomization
    /// to obtain the patched bytes for writing to the mod output folder.
    /// </summary>
    public static class EnemyRandomizer
    {
        /// <summary>
        /// Applies <paramref name="table"/> to every non-zero drop and steal item ID
        /// across all stat blocks in all provided <paramref name="files"/>.
        ///
        /// Item ID 0 ("no item") is always left unchanged, both because 0 is never
        /// present in the shuffle pool and as an explicit safety guard.
        ///
        /// Pass <see cref="ItemRemapTable.Passthrough()"/> when item shuffle is
        /// disabled — all values will remap to themselves and no bytes are changed.
        /// </summary>
        /// <param name="files">Enemy files to modify in-place. Must not be null.</param>
        /// <param name="table">The item remap table. Must not be null.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="files"/> or <paramref name="table"/> is null.
        /// </exception>
        public static void RemapDropsAndSteals(
            IEnumerable<EnemyFile> files, ItemRemapTable table)
        {
            if (files == null) throw new ArgumentNullException(nameof(files));
            if (table == null) throw new ArgumentNullException(nameof(table));

            foreach (EnemyFile file in files)
            {
                for (int stat = 0; stat < file.StatCount; stat++)
                {
                    // Remap 4 drop slots
                    for (int slot = 0; slot < 4; slot++)
                    {
                        byte current = file.GetDrop(stat, slot);
                        if (current != 0)
                            file.SetDrop(stat, slot, (byte)table.Remap(current));
                    }

                    // Remap 4 steal slots
                    for (int slot = 0; slot < 4; slot++)
                    {
                        byte current = file.GetSteal(stat, slot);
                        if (current != 0)
                            file.SetSteal(stat, slot, (byte)table.Remap(current));
                    }
                }
            }
        }

        /// <summary>
        /// Shuffles all non-zero blue magic ability IDs across all stat blocks
        /// in all provided <paramref name="files"/>.
        ///
        /// Blue magic IDs are ability IDs (not item IDs) and are shuffled within
        /// their own pool. Enemies with no blue magic (value 0) are not affected.
        ///
        /// The shuffle is performed across all files collectively — one enemy's
        /// blue magic spell can move to a completely different enemy file.
        ///
        /// Files must be provided in a consistent order across runs for
        /// seed-identical output.
        /// </summary>
        /// <param name="files">Enemy files to modify in-place. Must not be null.</param>
        /// <param name="rng">Seeded random instance from SeedEngine. Must not be null.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="files"/> or <paramref name="rng"/> is null.
        /// </exception>
        public static void ShuffleBlueMagic(IEnumerable<EnemyFile> files, Random rng)
        {
            if (files == null) throw new ArgumentNullException(nameof(files));
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            // Pass 1: collect all (file, statIndex) tuples with non-zero blue magic
            var slots = new List<(EnemyFile File, int Stat)>();
            var values = new List<byte>();

            foreach (EnemyFile file in files)
            {
                for (int stat = 0; stat < file.StatCount; stat++)
                {
                    byte bm = file.GetBlueMagic(stat);
                    if (bm != 0)
                    {
                        slots.Add((file, stat));
                        values.Add(bm);
                    }
                }
            }

            if (values.Count == 0) return;

            // Fisher-Yates shuffle on the values list
            FisherYates(values, rng);

            // Pass 2: write shuffled values back to the same slots
            for (int i = 0; i < slots.Count; i++)
                slots[i].File.SetBlueMagic(slots[i].Stat, values[i]);
        }

        /// <summary>
        /// Shuffles all non-zero Tetramaster card drop IDs across all stat blocks
        /// in all provided <paramref name="files"/>.
        ///
        /// Card IDs are raw Tetramaster card numbers (0–~100), not item IDs.
        /// They are shuffled within their own pool. Enemies with no card drop
        /// (value 0) are not affected.
        ///
        /// Files must be provided in a consistent order across runs for
        /// seed-identical output.
        /// </summary>
        /// <param name="files">Enemy files to modify in-place. Must not be null.</param>
        /// <param name="rng">Seeded random instance from SeedEngine. Must not be null.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="files"/> or <paramref name="rng"/> is null.
        /// </exception>
        public static void ShuffleCardDrops(IEnumerable<EnemyFile> files, Random rng)
        {
            if (files == null) throw new ArgumentNullException(nameof(files));
            if (rng == null) throw new ArgumentNullException(nameof(rng));

            // Pass 1: collect all (file, statIndex) tuples with non-zero card drops
            var slots = new List<(EnemyFile File, int Stat)>();
            var values = new List<byte>();

            foreach (EnemyFile file in files)
            {
                for (int stat = 0; stat < file.StatCount; stat++)
                {
                    byte card = file.GetCardDrop(stat);
                    if (card != 0)
                    {
                        slots.Add((file, stat));
                        values.Add(card);
                    }
                }
            }

            if (values.Count == 0) return;

            FisherYates(values, rng);

            for (int i = 0; i < slots.Count; i++)
                slots[i].File.SetCardDrop(slots[i].Stat, values[i]);
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private static void FisherYates(List<byte> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}