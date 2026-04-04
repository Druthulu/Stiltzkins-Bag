using System.Collections.Generic;
using StiltzkinsBag.Models;
using StiltzkinsBag.Models.Csv;
using StiltzkinsBag.Randomizers;

namespace StiltzkinsBag.Core.Output
{
    /// <summary>
    /// Bundles all randomizer result data produced during a generation run
    /// so it can be passed as a single unit to <see cref="SpoilerLogWriter"/>.
    ///
    /// Null result properties indicate the corresponding feature was disabled
    /// or did not produce a result for this run.
    ///
    /// Built by <c>RandomizerEngine</c> after all randomizers complete;
    /// consumed by <c>SpoilerLogWriter.Write()</c>.
    /// </summary>
    public class SpoilerLogData
    {
        // ── Required ─────────────────────────────────────────────────────────

        /// <summary>Deterministic seed integer produced by SeedEngine.</summary>
        public required int SeedInt { get; init; }

        /// <summary>Settings used for this generation run.</summary>
        public required Settings Settings { get; init; }

        // ── Optional — item name lookup ───────────────────────────────────────

        /// <summary>
        /// Item ID → display name map, parsed from Items.csv inline comments
        /// by VanillaItemCatalog.ParseItemNames() during generation.
        /// When null, the spoiler log renders item IDs as "Item #N".
        /// </summary>
        public IReadOnlyDictionary<int, string>? ItemNames { get; init; }

        // ── Optional — CSV randomizer results ─────────────────────────────────

        /// <summary>Null if Settings.RandomizeCharacters was false.</summary>
        public CharacterRandomizerResult? CharacterResult { get; init; }

        /// <summary>Null if Settings.RandomizeShops was false.</summary>
        public ShopRandomizerResult? ShopResult { get; init; }

        /// <summary>Null if Settings.RandomizeSynthesis was false.</summary>
        public SynthesisRandomizerResult? SynthesisResult { get; init; }

        /// <summary>
        /// Randomized gem equip costs. Null if Settings.RandomizeAbilityGems was false.
        /// </summary>
        public IReadOnlyList<AbilityGemsRow>? GemsResult { get; init; }

        /// <summary>
        /// Randomized starting inventory. Null if Settings.RandomizeInitialItems was false.
        /// </summary>
        public IReadOnlyList<InitialItemsRow>? InitialItemsResult { get; init; }

        // ── Optional — binary randomizer results ──────────────────────────────

        /// <summary>Null if Settings.RandomizeTreasureChests was false.</summary>
        public FieldRandomizationResult? FieldResult { get; init; }

        /// <summary>
        /// Stiltzkin result. Always populated (may be StiltzkinRandomizerResult.NotRun).
        /// </summary>
        public StiltzkinRandomizerResult? StiltzkinResult { get; init; }

        // ── Optional — summary-only flags (no human-readable result objects) ──

        /// <summary>
        /// True if any EnemyRandomizer method ran
        /// (RemapDropsAndSteals, ShuffleBlueMagic, or ShuffleCardDrops).
        /// EnemyRandomizer has no result object — it modifies EnemyFile in-place.
        /// </summary>
        public bool EnemyDropsRemapped { get; init; }
        public bool EnemyBlueMagicShuffled { get; init; }
        public bool EnemyCardDropsShuffled { get; init; }

        /// <summary>
        /// True if TetraMasterRandomizer ran. The result object (patched byte arrays)
        /// has no human-readable content, so only the flag is captured here.
        /// </summary>
        public bool TetraMasterRandomizationApplied { get; init; }
    }
}