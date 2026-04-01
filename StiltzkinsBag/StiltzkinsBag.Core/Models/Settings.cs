using System.Text.Json;
using System.Text.Json.Serialization;

namespace StiltzkinsBag.Models;

/// <summary>
/// Randomizer mode — controls whether constraint logic is enforced.
/// </summary>
public enum RandomizerMode
{
    /// <summary>
    /// Constraint-enforced mode. Classes, abilities, and equipment are
    /// coherent. Key items are always obtainable. Legendaries stay rare.
    /// </summary>
    Recommended,

    /// <summary>
    /// Unconstrained mode. Maximum randomness. No guardrails.
    /// For experienced players who want full chaos.
    /// </summary>
    Chaos
}

/// <summary>
/// Controls how equipment is distributed across characters.
/// </summary>
public enum EquipmentMode
{
    /// <summary>Each character gets equipment drawn from the full pool.</summary>
    Random,

    /// <summary>Equipment pool is shared and distributed across all characters.</summary>
    ShareAll,

    /// <summary>Characters receive equipment from their class archetype's typical stock.</summary>
    Stock
}

/// <summary>
/// Controls what item pool is used when randomizing starting items.
/// </summary>
public enum StartingItemMode
{
    /// <summary>
    /// Shuffle items within the consumable pool (IDs 236–253).
    /// Potions, Ethers, Phoenix Downs, and similar survival items.
    /// Default mode.
    /// </summary>
    ConsumablesRandom,

    /// <summary>
    /// Mix of consumables and obtainable gear (weapons, armor, accessories).
    /// Legendaries and story-only items (Price &lt;= 2) are excluded.
    /// </summary>
    GearRandom,

    /// <summary>
    /// Survival consumables plus a selection of Magic Gems (IDs 224–235).
    /// Start the run already learning abilities.
    /// </summary>
    AbilityStarter,

    /// <summary>
    /// Fixed minimal kit: 1× Potion, 1× Phoenix Down. No randomization.
    /// Challenge mode for experienced players.
    /// </summary>
    SpeedRunner,

    /// <summary>
    /// High-value sellable items only (Price >= <see cref="Settings.FilthyRichThreshold"/>).
    /// Key items are excluded. Start the run wealthy — sell for gil advantage.
    /// </summary>
    FilthyRich,

    /// <summary>
    /// Low-tier cheap consumables only (Price &lt;= <see cref="Settings.JunkDrawerThreshold"/>).
    /// Hard mode flavour — you start with whatever junk fell out of a drawer.
    /// </summary>
    JunkDrawer,

    /// <summary>
    /// One of every item in the game (IDs 0–254).
    /// Development and debug use only. Requires <see cref="Settings.IsDebugMode"/> = true.
    /// </summary>
    AllItems
}

/// <summary>
/// Controls how gem equip costs are randomized in AbilityGems.csv.
/// </summary>
public enum AbilityGemMode
{
    /// <summary>
    /// Fisher-Yates shuffle of the 64 vanilla gem costs across all abilities.
    /// Total gem budget is preserved — the game stays equally equippable overall,
    /// but which abilities are cheap vs. expensive is fully scrambled.
    /// Best choice for Recommended mode. 63 RNG calls.
    /// </summary>
    Shuffle,

    /// <summary>
    /// Each ability receives an independent random cost in
    /// [<see cref="Settings.AbilityGemMinCost"/>, <see cref="Settings.AbilityGemMaxCost"/>].
    /// Maximum variety. 64 RNG calls.
    /// </summary>
    BoundedRandom,

    /// <summary>
    /// All gem equip costs set to 1. Equip any combination of abilities instantly.
    /// Development and debug use only. Requires <see cref="Settings.IsDebugMode"/> = true.
    /// Downgrades to <see cref="Shuffle"/> when debug mode is inactive.
    /// </summary>
    AllCheap
}

/// <summary>
/// Controls how AP costs to learn abilities are randomized.
/// Scale factors are always per-ability — the same AbilityRef costs the same AP
/// on every character that has it.
/// </summary>
public enum AbilityApMode
{
    /// <summary>
    /// Each ability's vanilla AP cost is multiplied by a random scale factor drawn
    /// from [<see cref="Settings.ApScaleMinPercent"/>%, <see cref="Settings.ApScaleMaxPercent"/>%].
    /// One RNG call per unique AbilityRef (sorted for determinism). Default mode.
    /// </summary>
    ProportionalScale,

    /// <summary>
    /// AP costs are flipped around the midpoint of the vanilla range.
    /// Formula: invertedCost = vanillaMin + vanillaMax − vanillaCost.
    /// Expensive abilities become cheap; cheap abilities become expensive.
    /// Fully deterministic — 0 RNG calls.
    /// </summary>
    Inverse,

    /// <summary>
    /// Every ability costs exactly <see cref="Settings.ApFlatCost"/> AP.
    /// Removes the "which ability do I learn first" decision entirely.
    /// Fully deterministic — 0 RNG calls.
    /// </summary>
    FlatCost,

    /// <summary>
    /// Every ability costs 1 AP — learned in a single battle.
    /// Development and debug use only. Requires <see cref="Settings.IsDebugMode"/> = true.
    /// Downgrades to <see cref="ProportionalScale"/> when debug mode is inactive.
    /// </summary>
    Amnesia
}

/// <summary>
/// Controls which stat-bearing rows GearStatRandomizer modifies and how
/// new stat values are generated.
/// </summary>
public enum GearStatMode
{
    /// <summary>
    /// Each stat on each qualifying row is independently drawn using the active
    /// <see cref="GearStatWeighting"/>. Standard stats cap at
    /// <see cref="Settings.GearStatMax"/>; an epic roll can push one stat to
    /// <see cref="Settings.GearStatEpicMax"/>. Default mode.
    /// </summary>
    Proportional,

    /// <summary>
    /// Fisher-Yates shuffle of all stat values across all qualifying rows.
    /// Total stat budget in the game is preserved — just redistributed.
    /// A +3 Str that was on a Power Belt might end up on a Hat.
    /// Ignores <see cref="GearStatWeighting"/> — uses existing values only.
    /// </summary>
    Shuffle,

    /// <summary>
    /// Each stat draws independently from the full [0, <see cref="Settings.GearStatHardCap"/>]
    /// range using the active <see cref="GearStatWeighting"/>.
    /// No total sum constraint — can produce very powerful items.
    /// </summary>
    Chaos
}

/// <summary>
/// Controls the probability distribution used when generating individual stat values.
/// Applies to <see cref="GearStatMode.Proportional"/> and <see cref="GearStatMode.Chaos"/>.
/// Ignored by <see cref="GearStatMode.Shuffle"/>.
/// </summary>
public enum GearStatWeighting
{
    /// <summary>
    /// Geometric distribution with base 0.5. Each step is half as likely as the previous.
    /// Approximate probabilities: +0=50%, +1=25%, +2=12.5%, +3=6.25%, +4=3.1%, +5=1.6%.
    /// More generous than vanilla — good for an exciting randomizer feel.
    /// </summary>
    Geometric,

    /// <summary>
    /// Geometric distribution with base 0.35. Matches the natural rarity curve of
    /// vanilla Stats.csv almost exactly (~65% zeros, ~23% ones, ~8% twos).
    /// Subtler changes — gear feels close to vanilla power level.
    /// </summary>
    VanillaWeighted,

    /// <summary>
    /// Probability decreases linearly from 0 to max.
    /// Middle ground between Geometric and Uniform.
    /// </summary>
    Linear,

    /// <summary>
    /// Flat equal probability across all values in range.
    /// Produces very powerful gear on average — use with caution.
    /// </summary>
    Uniform
}

/// <summary>
/// Controls which items are eligible to appear in a randomized shop.
/// Applies to <see cref="ShopMode.BoundedRandom"/>.
/// </summary>
public enum ShopItemPool
{
    /// <summary>
    /// Only consumable items (roughly IDs 236–253) are eligible.
    /// Shops will never sell equipment or gems.
    /// Safest option for Recommended mode.
    /// </summary>
    ConsumablesOnly,

    /// <summary>
    /// All non-key items are eligible — consumables, weapons, armor, accessories, gems.
    /// Key items and story-only items (Price &lt;= 2) are always excluded.
    /// Maximum variety.
    /// </summary>
    AllNonKeyItems
}

/// <summary>
/// Controls how items are selected and distributed across randomized shops.
/// </summary>
public enum ShopMode
{
    /// <summary>
    /// Fisher-Yates shuffle of all item slots across all shops globally.
    /// Items that exist in vanilla shops stay in the shop system — just redistributed.
    /// No duplicates within a single shop. Per-shop item count governed by
    /// <see cref="ShopSizeMode"/>. Default mode.
    /// </summary>
    Shuffle,

    /// <summary>
    /// Each shop independently draws items from the pool defined by
    /// <see cref="ShopItemPool"/>. No duplicates within a single shop.
    /// Per-shop item count governed by <see cref="ShopSizeMode"/>.
    /// </summary>
    BoundedRandom,

    /// <summary>
    /// Every shop sells the same broad item list drawn from <see cref="ShopItemPool"/>.
    /// Useful for testing. Development and debug use only.
    /// Requires <see cref="Settings.IsDebugMode"/> = true.
    /// Downgrades to <see cref="Shuffle"/> when debug mode is inactive.
    /// </summary>
    MegaMart
}

/// <summary>
/// Controls how many items each shop offers after randomization.
/// When <see cref="Settings.ShortSupply"/> is true, the ShortSupply cap is applied
/// after this mode computes the target size — it may further reduce the count.
/// </summary>
public enum ShopSizeMode
{
    /// <summary>
    /// Each shop keeps its vanilla item count. Default mode.
    /// </summary>
    Maintain,

    /// <summary>
    /// Each shop receives a random item count drawn from
    /// [global vanilla minimum shop size, global vanilla maximum shop size].
    /// One RNG call per shop.
    /// </summary>
    Random,

    /// <summary>
    /// Every shop contains exactly <see cref="Settings.ShopFixedSize"/> items.
    /// User-configurable.
    /// </summary>
    Fixed
}

/// <summary>
/// Controls how synthesis recipe prices are randomized.
/// <see cref="Settings.BadEconomy"/> multiplier is applied on top of this mode
/// when enabled.
/// </summary>
public enum SynthesisPriceMode
{
    /// <summary>
    /// All synthesis prices are unchanged from vanilla. Default mode.
    /// </summary>
    Vanilla,

    /// <summary>
    /// Each recipe price is drawn independently from
    /// [<see cref="Settings.SynthPriceMin"/>, <see cref="Settings.SynthPriceMax"/>].
    /// One RNG call per recipe.
    /// </summary>
    BoundedRandom,

    /// <summary>
    /// Each recipe price is scaled by a random factor drawn from
    /// [<see cref="Settings.SynthPriceScaleMinPercent"/>%, <see cref="Settings.SynthPriceScaleMaxPercent"/>%].
    /// Preserves relative cost relationships while introducing variance.
    /// One RNG call per recipe.
    /// </summary>
    ProportionalScale,

    /// <summary>
    /// Price is derived from the gear stat score of the result item.
    /// Higher-stat gear costs more to synthesize. Items with no stats
    /// fall back to vanilla price. 0 RNG calls.
    /// </summary>
    GearScored
}

/// <summary>
/// Controls how card stat values (attack, defence, magic defence) are randomized.
/// Type and arrow bytes are controlled separately by <see cref="CardTypeMode"/>
/// and <see cref="ArrowMode"/>.
/// </summary>
public enum CardStatMode
{
    /// <summary>
    /// Independently Fisher-Yates shuffle attack values across all 100 cards,
    /// then defence values, then magic defence values.
    /// Existing stat pool is preserved — just redistributed. Default mode.
    /// 297 RNG calls.
    /// </summary>
    Shuffle,

    /// <summary>
    /// Each stat per card draws independently from
    /// [<see cref="Settings.CardStatMin"/>, <see cref="Settings.CardStatMax"/>].
    /// 300 RNG calls.
    /// </summary>
    BoundedRandom,

    /// <summary>
    /// Cards are sorted by total stat sum (attack + defence + magic defence) ascending
    /// and assigned to card IDs 0→99 in that order. Goblin (0) gets the weakest stats,
    /// Airship (99) gets the strongest. Fully deterministic — 0 RNG calls.
    /// Predictable power curve — good for racing or challenge runs.
    /// </summary>
    TierLock,

    /// <summary>
    /// All stats set to 255. Every card is equally devastating.
    /// Development and debug use only. Requires <see cref="Settings.IsDebugMode"/> = true.
    /// Downgrades to <see cref="Shuffle"/> when debug mode is inactive.
    /// </summary>
    AllCardsMaxed
}

/// <summary>
/// Controls how card type bytes (Physical / Magical) are randomized.
/// Independent of <see cref="CardStatMode"/> — both can apply simultaneously.
/// </summary>
public enum CardTypeMode
{
    /// <summary>Type bytes are never modified. Default mode.</summary>
    Preserve,

    /// <summary>
    /// Fisher-Yates shuffle of all 100 type bytes.
    /// P/M distribution is preserved — same count of each, just reassigned.
    /// 99 RNG calls.
    /// </summary>
    Shuffle,

    /// <summary>All cards become Physical (type byte = 0). 0 RNG calls.</summary>
    AllP,

    /// <summary>All cards become Magical (type byte = 1). 0 RNG calls.</summary>
    AllM
}

/// <summary>
/// Controls how card arrow bytes (directional attack pattern bitmask) are randomized.
/// Each bit in the byte represents one of 8 cardinal/diagonal directions.
/// </summary>
public enum ArrowMode
{
    /// <summary>Arrow bytes are never modified. Default mode.</summary>
    Preserve,

    /// <summary>
    /// rng.Next(1, 256) per card. At least 1 arrow bit is always set.
    /// 100 RNG calls.
    /// </summary>
    Random,

    /// <summary>All cards get 0xFF — all 8 arrows active on every card. 0 RNG calls.</summary>
    AllDirections,

    /// <summary>All cards get 0x00 — no arrows on any card. 0 RNG calls.</summary>
    NoArrows,

    /// <summary>
    /// rng.Next(0, 256) per card. 0 arrows (0x00) is possible.
    /// 100 RNG calls.
    /// </summary>
    Chaos
}

/// <summary>
/// Controls how the 64 card sets (NPC draw pools) are randomized.
/// Each set contains 16 card ID slots; duplicates within a set are permitted.
/// </summary>
public enum CardSetMode
{
    /// <summary>
    /// Fisher-Yates of all 1024 card ID slots as a single flat pool.
    /// Every card ID that existed in vanilla sets stays in the system — just redistributed.
    /// 1023 RNG calls.
    /// </summary>
    Shuffle,

    /// <summary>
    /// Each of the 1024 slots independently draws rng.Next(0, 100).
    /// Produces fully novel set compositions — any card can appear in any set.
    /// 1024 RNG calls.
    /// </summary>
    BuildFromScratch
}

/// <summary>
/// Controls how NPC deck difficulty bytes are randomized.
/// </summary>
public enum NpcDifficultyMode
{
    /// <summary>Difficulty bytes are never modified. Default mode.</summary>
    Preserve,

    /// <summary>
    /// Fisher-Yates shuffle of all 256 difficulty bytes.
    /// Early NPCs might play at max difficulty; final bosses might be easy.
    /// 255 RNG calls.
    /// </summary>
    Shuffle,

    /// <summary>All 256 NPC decks set to difficulty 3 (hardest). 0 RNG calls.</summary>
    RaiseAll,

    /// <summary>All 256 NPC decks set to difficulty 0 (easiest). 0 RNG calls.</summary>
    LowerAll
}

/// <summary>
/// Controls how Stiltzkin's package items are randomized.
/// Independent of <see cref="StiltzkinPriceMode"/> — both can be set freely.
/// </summary>
public enum StiltzkinMode
{
    /// <summary>
    /// All 9 Stiltzkin scripts are left exactly as vanilla. Default mode.
    /// </summary>
    Off,

    /// <summary>
    /// Fisher-Yates shuffle of all 24 vanilla Stiltzkin items (8 visits × 3 items).
    /// Items are redistributed 3-per-visit. The overall Stiltzkin item economy is
    /// preserved — the same 24 items exist, just in different packages.
    /// Stiltzkin scripts are excluded from FieldItemRandomizer when this mode is active.
    /// </summary>
    Shuffle,

    /// <summary>
    /// Stiltzkin scripts are included in FieldItemRandomizer's normal field item pool.
    /// Stiltzkin's items are treated like any other field AddItem location.
    /// No special Stiltzkin logic runs. Overrides <see cref="StiltzkinRecommendedSubMode"/>.
    /// </summary>
    IncludeInFieldPool,

    /// <summary>
    /// Curated package generation using the active <see cref="StiltzkinRecommendedSubMode"/>.
    /// Stiltzkin scripts are excluded from FieldItemRandomizer when this mode is active.
    /// </summary>
    Recommended
}

/// <summary>
/// Controls the item pool used when <see cref="StiltzkinMode.Recommended"/> is active.
/// Ignored when <see cref="StiltzkinMode"/> is not <see cref="StiltzkinMode.Recommended"/>.
/// </summary>
public enum StiltzkinRecommendedSubMode
{
    /// <summary>
    /// "Stiltzkin's Junk" — packages drawn from low-value consumables.
    /// Potions, Antidotes, Ethers. Stiltzkin sells garbage. Comedy run.
    /// </summary>
    StiltzkinsJunk,

    /// <summary>
    /// "Fun" — the true vision. Each package contains one useful item, one mid-tier item,
    /// and one wildcard. Surprising but never completely useless.
    /// </summary>
    Fun,

    /// <summary>
    /// "Challenging" — packages drawn from high-value and rare items.
    /// Makes Gil management critical — save up or miss out.
    /// </summary>
    Challenging
}

/// <summary>
/// Controls how Stiltzkin's package prices are randomized.
/// Independent of <see cref="StiltzkinMode"/> — prices can be randomized even
/// when item contents are unchanged, and vice versa.
/// </summary>
public enum StiltzkinPriceMode
{
    /// <summary>
    /// All package prices are unchanged from vanilla (333, 444, 555, 666, 777, 888,
    /// 2222, 5555 Gil). Default mode.
    /// </summary>
    Off,

    /// <summary>
    /// "Clearance Sale" — prices slashed. Gil cost is essentially irrelevant.
    /// Stiltzkin practically gives things away. Good for casual or item-focused runs.
    /// </summary>
    ClearanceSale,

    /// <summary>
    /// "Stiltzkin's Mood" — prices are fully random per visit.
    /// Could be 1 Gil, could be 9999 Gil. You never know what you're walking into.
    /// </summary>
    StiltzkinsMood,

    /// <summary>
    /// "Highway Robbery" — punishing prices. Save up for every visit or miss out.
    /// Makes the Stiltzkin quest a meaningful economic challenge.
    /// </summary>
    HighwayRobbery
}

/// <summary>
/// All user-configurable settings for a randomizer run.
/// This is the single source of truth passed into the randomization pipeline.
/// Serialized to Settings-Seed-[int].json in the mod output folder.
/// </summary>
public class Settings
{
    // -------------------------------------------------------------------------
    // Core
    // -------------------------------------------------------------------------

    /// <summary>The raw seed string entered by the user.</summary>
    public string SeedString { get; set; } = string.Empty;

    /// <summary>
    /// Deterministic integer resolved from SeedString by SeedEngine.
    /// This is the value used to construct the single Random instance.
    /// </summary>
    public int SeedInt { get; set; }

    /// <summary>Absolute path to the FF9 Steam installation root.</summary>
    public string GamePath { get; set; } = string.Empty;

    /// <summary>Recommended or Chaos — controls constraint enforcement throughout the pipeline.</summary>
    public RandomizerMode Mode { get; set; } = RandomizerMode.Recommended;

    /// <summary>
    /// Enables developer/debug features such as <see cref="StartingItemMode.AllItems"/>,
    /// <see cref="AbilityGemMode.AllCheap"/>, <see cref="AbilityApMode.Amnesia"/>,
    /// <see cref="ShopMode.MegaMart"/>, <see cref="Settings.AllStatsMaxed"/>,
    /// and <see cref="CardStatMode.AllCardsMaxed"/>.
    /// Never expose this toggle in the public UI.
    /// </summary>
    public bool IsDebugMode { get; set; }

    // -------------------------------------------------------------------------
    // Character
    // -------------------------------------------------------------------------

    public bool RandomizeCharacters { get; set; }
    public bool RandomizeBaseStats { get; set; }
    public bool RandomizeSpeciality { get; set; }
    public bool RandomizeAbilities { get; set; }
    public bool RandomizeEquipment { get; set; }
    public EquipmentMode EquipmentMode { get; set; } = EquipmentMode.Random;
    public bool RandomizeStartingItems { get; set; }

    // -------------------------------------------------------------------------
    // Items
    // -------------------------------------------------------------------------

    public bool RandomizeInitialItems { get; set; }

    /// <summary>
    /// Determines which item pool is used for starting item randomization.
    /// See <see cref="StartingItemMode"/> for all options.
    /// <see cref="StartingItemMode.AllItems"/> requires <see cref="IsDebugMode"/> = true.
    /// </summary>
    public StartingItemMode StartingItemMode { get; set; } = StartingItemMode.ConsumablesRandom;

    /// <summary>
    /// When true, item stack counts are also randomized in range [1, 7].
    /// When false, vanilla counts are preserved in position order.
    /// Has no effect in <see cref="StartingItemMode.SpeedRunner"/> or
    /// <see cref="StartingItemMode.AllItems"/> modes, which have fixed counts.
    /// </summary>
    public bool RandomizeStartingCounts { get; set; }

    /// <summary>
    /// Minimum item price threshold for <see cref="StartingItemMode.FilthyRich"/>.
    /// Only items with Price >= this value are eligible for the starting pool.
    /// Key items are always excluded regardless of price.
    /// Default: 200.
    /// </summary>
    public int FilthyRichThreshold { get; set; } = 200;

    /// <summary>
    /// Maximum item price threshold for <see cref="StartingItemMode.JunkDrawer"/>.
    /// Only consumable items with Price &lt;= this value are eligible.
    /// Default: 50.
    /// </summary>
    public int JunkDrawerThreshold { get; set; } = 50;

    public bool RandomizeTreasureChests { get; set; }

    // -------------------------------------------------------------------------
    // Stiltzkin
    // -------------------------------------------------------------------------

    /// <summary>
    /// Controls how Stiltzkin's package items are randomized.
    /// <see cref="StiltzkinMode.Shuffle"/> and <see cref="StiltzkinMode.Recommended"/>
    /// exclude Stiltzkin scripts from FieldItemRandomizer.
    /// <see cref="StiltzkinMode.IncludeInFieldPool"/> leaves them in the normal field pool.
    /// Default: Off.
    /// </summary>
    public StiltzkinMode StiltzkinMode { get; set; } = StiltzkinMode.Off;

    /// <summary>
    /// Controls the item pool used when <see cref="StiltzkinMode.Recommended"/> is active.
    /// Ignored for all other <see cref="StiltzkinMode"/> values.
    /// Default: Fun.
    /// </summary>
    public StiltzkinRecommendedSubMode StiltzkinRecommendedSubMode { get; set; } =
        StiltzkinRecommendedSubMode.Fun;

    /// <summary>
    /// Controls how Stiltzkin's package prices are randomized.
    /// Fully independent of <see cref="StiltzkinMode"/> — prices can be randomized
    /// even when item contents are unchanged, and vice versa.
    /// Default: Off.
    /// </summary>
    public StiltzkinPriceMode StiltzkinPriceMode { get; set; } = StiltzkinPriceMode.Off;

    // -------------------------------------------------------------------------
    // Shops
    // -------------------------------------------------------------------------

    /// <summary>Master on/off gate for shop randomization.</summary>
    public bool RandomizeShops { get; set; }

    /// <summary>
    /// Controls how items are selected and distributed across shops.
    /// See <see cref="ShopMode"/> for all options.
    /// <see cref="ShopMode.MegaMart"/> requires <see cref="IsDebugMode"/> = true.
    /// </summary>
    public ShopMode ShopMode { get; set; } = ShopMode.Shuffle;

    /// <summary>
    /// Controls how many items each shop offers after randomization.
    /// See <see cref="ShopSizeMode"/> for all options.
    /// <see cref="Settings.ShortSupply"/> cap is applied after this if enabled.
    /// </summary>
    public ShopSizeMode ShopSizeMode { get; set; } = ShopSizeMode.Maintain;

    /// <summary>
    /// Fixed item count per shop when <see cref="ShopSizeMode.Fixed"/> is active.
    /// Must be at least 1. Default: 4.
    /// </summary>
    public int ShopFixedSize { get; set; } = 4;

    /// <summary>
    /// Controls which items are eligible to appear in shops when
    /// <see cref="ShopMode.BoundedRandom"/> is active.
    /// See <see cref="ShopItemPool"/> for all options.
    /// </summary>
    public ShopItemPool ShopItemPool { get; set; } = ShopItemPool.ConsumablesOnly;

    /// <summary>
    /// When true, guarantees at least <see cref="ShopMedicMinShops"/> shops
    /// carry both a Potion and a Phoenix Down, regardless of randomization mode.
    /// Applied as a post-processing pass after all shop randomization.
    /// </summary>
    public bool ShopEnsureMedicItems { get; set; } = true;

    /// <summary>
    /// Minimum number of shops that must carry medic items (Potion + Phoenix Down)
    /// when <see cref="ShopEnsureMedicItems"/> is true.
    /// Default: 2.
    /// </summary>
    public int ShopMedicMinShops { get; set; } = 2;

    // -------------------------------------------------------------------------
    // Synthesis
    // -------------------------------------------------------------------------

    /// <summary>Master on/off gate for synthesis randomization.</summary>
    public bool RandomizeSynthesis { get; set; }

    /// <summary>
    /// When true, synthesis Result IDs are shuffled or randomized across recipes.
    /// In <see cref="RandomizerMode.Chaos"/> or when <see cref="AllowNewSynthesisResults"/>
    /// is true, results may be drawn from outside the vanilla synthesis result pool.
    /// Independent of <see cref="RandomizeSynthesisIngredients"/>.
    /// </summary>
    public bool RandomizeSynthesisResults { get; set; }

    /// <summary>
    /// When true, synthesis ingredient sets are shuffled across recipes.
    /// Independent of <see cref="RandomizeSynthesisResults"/>.
    /// Both flags can be enabled simultaneously for full recipe chaos.
    /// </summary>
    public bool RandomizeSynthesisIngredients { get; set; }

    /// <summary>
    /// When true, synthesis results are drawn from the full item pool rather than
    /// only vanilla synthesis result IDs. Only has effect when
    /// <see cref="RandomizeSynthesisResults"/> is also true.
    /// In <see cref="RandomizerMode.Recommended"/>, obtainability constraints still apply.
    /// In <see cref="RandomizerMode.Chaos"/>, no constraints — any item can appear.
    /// </summary>
    public bool AllowNewSynthesisResults { get; set; }

    /// <summary>
    /// Controls how synthesis recipe prices are randomized.
    /// See <see cref="SynthesisPriceMode"/> for all options.
    /// <see cref="Settings.BadEconomy"/> multiplier is applied on top when enabled.
    /// </summary>
    public SynthesisPriceMode SynthesisPriceMode { get; set; } = SynthesisPriceMode.Vanilla;

    /// <summary>
    /// Minimum synthesis price for <see cref="SynthesisPriceMode.BoundedRandom"/> mode.
    /// Must be at least 1. Default: 100.
    /// </summary>
    public int SynthPriceMin { get; set; } = 100;

    /// <summary>
    /// Maximum synthesis price for <see cref="SynthesisPriceMode.BoundedRandom"/> mode.
    /// Must be >= <see cref="SynthPriceMin"/>. Swapped with min if set lower.
    /// Default: 5000.
    /// </summary>
    public int SynthPriceMax { get; set; } = 5000;

    /// <summary>
    /// Minimum price scale factor as a percentage for
    /// <see cref="SynthesisPriceMode.ProportionalScale"/> mode.
    /// 50 = multiply vanilla price by 0.5×. Must be at least 1. Default: 50.
    /// </summary>
    public int SynthPriceScaleMinPercent { get; set; } = 50;

    /// <summary>
    /// Maximum price scale factor as a percentage for
    /// <see cref="SynthesisPriceMode.ProportionalScale"/> mode.
    /// 200 = multiply vanilla price by 2.0×.
    /// Must be >= <see cref="SynthPriceScaleMinPercent"/>. Swapped with min if set lower.
    /// Default: 200.
    /// </summary>
    public int SynthPriceScaleMaxPercent { get; set; } = 200;

    // -------------------------------------------------------------------------
    // Challenge Modifiers
    // These flags stack on top of any ShopMode or SynthesisPriceMode.
    // Both can be enabled simultaneously.
    // -------------------------------------------------------------------------

    /// <summary>
    /// When true, all shop and synthesis prices are multiplied by a random factor
    /// drawn per-item from
    /// [<see cref="BadEconomyPriceMultiplierMin"/>, <see cref="BadEconomyPriceMultiplierMax"/>].
    /// Applied as a final post-processing pass after all price randomization.
    /// Applies to both ShopRandomizer and SynthesisRandomizer.
    /// </summary>
    public bool BadEconomy { get; set; }

    /// <summary>
    /// Minimum price multiplier for <see cref="BadEconomy"/> mode.
    /// 1.0 = no change. Default: 1.5 (50% markup minimum).
    /// </summary>
    public float BadEconomyPriceMultiplierMin { get; set; } = 1.5f;

    /// <summary>
    /// Maximum price multiplier for <see cref="BadEconomy"/> mode.
    /// Must be >= <see cref="BadEconomyPriceMultiplierMin"/>. Default: 4.0 (400% markup maximum).
    /// </summary>
    public float BadEconomyPriceMultiplierMax { get; set; } = 4.0f;

    /// <summary>
    /// When true, every shop's item count is capped at <see cref="ShortSupplyMaxItems"/>
    /// after all other shop size calculations. Items are trimmed from the end of the list.
    /// Applies only to shops — synthesis is unaffected.
    /// Compatible with all <see cref="ShopMode"/> and <see cref="ShopSizeMode"/> values.
    /// </summary>
    public bool ShortSupply { get; set; }

    /// <summary>
    /// Maximum number of items any single shop may offer when <see cref="ShortSupply"/> is true.
    /// Must be at least 1. Default: 3.
    /// </summary>
    public int ShortSupplyMaxItems { get; set; } = 3;

    // -------------------------------------------------------------------------
    // Gear Stat Bonuses
    // -------------------------------------------------------------------------

    public bool RandomizeGearStatBonuses { get; set; }

    /// <summary>
    /// Controls which rows are modified and how stat values are generated.
    /// See <see cref="GearStatMode"/> for all options.
    /// </summary>
    public GearStatMode GearStatMode { get; set; } = GearStatMode.Proportional;

    /// <summary>
    /// Controls the probability distribution used when drawing stat values.
    /// Ignored by <see cref="GearStatMode.Shuffle"/>.
    /// See <see cref="GearStatWeighting"/> for all options.
    /// </summary>
    public GearStatWeighting GearStatWeighting { get; set; } = GearStatWeighting.Geometric;

    /// <summary>
    /// Standard per-stat ceiling for <see cref="GearStatMode.Proportional"/> mode.
    /// Most stats will not exceed this value. Must be >= 0.
    /// Default: 3.
    /// </summary>
    public int GearStatMax { get; set; } = 3;

    /// <summary>
    /// Ceiling for the epic bonus roll in <see cref="GearStatMode.Proportional"/> mode.
    /// One randomly chosen stat may reach this value when the epic roll fires.
    /// Must be >= <see cref="GearStatMax"/>. Default: 4.
    /// </summary>
    public int GearStatEpicMax { get; set; } = 4;

    /// <summary>
    /// Percentage chance (0–100) for the epic bonus roll per qualifying row.
    /// Default: 10.
    /// </summary>
    public int GearStatEpicChancePercent { get; set; } = 10;

    /// <summary>
    /// Absolute ceiling for any individual stat value. No stat ever exceeds this
    /// regardless of mode or epic roll. Matches vanilla's maximum (Save The Queen Will=5).
    /// Default: 5.
    /// </summary>
    public int GearStatHardCap { get; set; } = 5;

    /// <summary>
    /// When true, rows that currently have zero stats (pure element-bonus items)
    /// are eligible to gain stats. When false (default), those rows are skipped.
    /// </summary>
    public bool ZeroStatItemsCanGainStats { get; set; }

    /// <summary>
    /// Debug only (<see cref="IsDebugMode"/> required). Sets every stat on every
    /// qualifying row to <see cref="GearStatHardCap"/>. Instant god gear.
    /// </summary>
    public bool AllStatsMaxed { get; set; }

    // -------------------------------------------------------------------------
    // Abilities
    // -------------------------------------------------------------------------

    /// <summary>Randomizes gem equip costs in AbilityGems.csv.</summary>
    public bool RandomizeAbilityGems { get; set; }

    /// <summary>
    /// Controls how gem equip costs are randomized.
    /// See <see cref="AbilityGemMode"/> for all options.
    /// <see cref="AbilityGemMode.AllCheap"/> requires <see cref="IsDebugMode"/> = true.
    /// </summary>
    public AbilityGemMode AbilityGemMode { get; set; } = AbilityGemMode.Shuffle;

    /// <summary>
    /// Minimum gem equip cost used in <see cref="AbilityGemMode.BoundedRandom"/> mode.
    /// Must be at least 1. Clamped to 1 if set lower.
    /// </summary>
    public int AbilityGemMinCost { get; set; } = 1;

    /// <summary>
    /// Maximum gem equip cost used in <see cref="AbilityGemMode.BoundedRandom"/> mode.
    /// Must be >= <see cref="AbilityGemMinCost"/>. Swapped with min if set lower.
    /// </summary>
    public int AbilityGemMaxCost { get; set; } = 20;

    /// <summary>
    /// Randomizes AP costs to learn abilities.
    /// Applied as a post-process on CharacterRandomizerResult.AbilityTables.
    /// </summary>
    public bool RandomizeAbilityAp { get; set; }

    /// <summary>
    /// Controls how AP costs are randomized.
    /// See <see cref="AbilityApMode"/> for all options.
    /// <see cref="AbilityApMode.Amnesia"/> requires <see cref="IsDebugMode"/> = true.
    /// </summary>
    public AbilityApMode AbilityApMode { get; set; } = AbilityApMode.ProportionalScale;

    /// <summary>
    /// Minimum AP scale factor as a percentage for <see cref="AbilityApMode.ProportionalScale"/>.
    /// 50 = multiply vanilla AP by 0.5× (halve costs). Must be at least 1.
    /// Default: 50.
    /// </summary>
    public int ApScaleMinPercent { get; set; } = 50;

    /// <summary>
    /// Maximum AP scale factor as a percentage for <see cref="AbilityApMode.ProportionalScale"/>.
    /// 200 = multiply vanilla AP by 2.0× (double costs).
    /// Must be >= <see cref="ApScaleMinPercent"/>. Swapped with min if set lower.
    /// Default: 200.
    /// </summary>
    public int ApScaleMaxPercent { get; set; } = 200;

    /// <summary>
    /// Fixed AP cost applied to every ability in <see cref="AbilityApMode.FlatCost"/> mode.
    /// Must be at least 1. Clamped to 1 if set lower.
    /// Default: 20.
    /// </summary>
    public int ApFlatCost { get; set; } = 20;

    // -------------------------------------------------------------------------
    // Enemies
    // -------------------------------------------------------------------------

    public bool RandomizeEnemies { get; set; }
    public bool RandomizeItemDrops { get; set; }
    public bool RandomizeItemSteals { get; set; }
    public bool RandomizeBlueMagic { get; set; }
    public bool RandomizeCardDrops { get; set; }

    // -------------------------------------------------------------------------
    // TetraMaster (bytecode only — TripleTriad.csv is NOT supported)
    // minigame_card_data_address  : card stats (attack, type, defence, magicdefence, arrows)
    // minigame_card_level_address : card sets (64 NPC draw pools × 16 card IDs each)
    // minigame_stage_address      : NPC decks (256 entries × set index + difficulty)
    // minista.mes                 : card names (7 language files, written only if ShuffleCardOrder)
    // All files live in resources.assets; mod output goes to embeddedasset/quadmist/
    // (names files go to embeddedasset/text/{lang}/etc/)
    // -------------------------------------------------------------------------

    /// <summary>Master on/off gate for all TetraMaster randomization.</summary>
    public bool RandomizeTetraMaster { get; set; }

    // ── Card Stats ──────────────────────────────────────────────────────────

    /// <summary>
    /// When true, card attack, defence, and magic defence values are randomized.
    /// Arrow and type bytes are controlled separately.
    /// </summary>
    public bool RandomizeCardStats { get; set; }

    /// <summary>
    /// Controls how card stat values are generated.
    /// See <see cref="CardStatMode"/> for all options.
    /// <see cref="CardStatMode.AllCardsMaxed"/> requires <see cref="IsDebugMode"/> = true.
    /// </summary>
    public CardStatMode CardStatMode { get; set; } = CardStatMode.Shuffle;

    /// <summary>
    /// Minimum stat value for <see cref="CardStatMode.BoundedRandom"/> mode.
    /// Applies to attack, defence, and magic defence. Must be >= 1.
    /// Default: 1 (vanilla-aligned — vanilla cards display 0–F hex = 0–15).
    /// </summary>
    public int CardStatMin { get; set; } = 1;

    /// <summary>
    /// Maximum stat value for <see cref="CardStatMode.BoundedRandom"/> mode.
    /// Must be >= <see cref="CardStatMin"/> and &lt;= 255.
    /// Default: 15 (vanilla-aligned — vanilla cards display 0–F hex = 0–15).
    /// </summary>
    public int CardStatMax { get; set; } = 15;

    // ── Card Types ──────────────────────────────────────────────────────────

    /// <summary>
    /// Controls whether and how card type bytes (Physical/Magical) are randomized.
    /// Independent of <see cref="CardStatMode"/> — both can apply in the same run.
    /// See <see cref="CardTypeMode"/> for all options.
    /// </summary>
    public CardTypeMode CardTypeMode { get; set; } = CardTypeMode.Preserve;

    // ── Arrows ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Controls whether and how card arrow bytes (directional bitmask) are randomized.
    /// Each bit represents one of 8 directions. Independent of stat and type randomization.
    /// See <see cref="ArrowMode"/> for all options.
    /// </summary>
    public ArrowMode ArrowMode { get; set; } = ArrowMode.Preserve;

    // ── Card Order ──────────────────────────────────────────────────────────

    /// <summary>
    /// When true, card identities are shuffled: card slot 0 may become Bahamut,
    /// card slot 65 may become Goblin, etc.
    /// The same Fisher-Yates permutation (99 RNG calls) is applied to both the
    /// stat entries in minigame_card_data_address AND the name byte arrays in
    /// all 7 language minista.mes files.
    /// Card set and NPC deck references remain valid after reorder — they index
    /// into the reordered stat table, so NPC decks using card ID 0 will play
    /// whichever card ended up in slot 0.
    /// </summary>
    public bool ShuffleCardOrder { get; set; }

    // ── Card Sets (NPC draw pools) ──────────────────────────────────────────

    /// <summary>
    /// When true, the 64 card sets (NPC opponent draw pools) are randomized.
    /// Each set contains 16 card ID slots; duplicates within a set are permitted.
    /// </summary>
    public bool RandomizeCardSets { get; set; }

    /// <summary>
    /// Controls how card set contents are generated.
    /// See <see cref="CardSetMode"/> for all options.
    /// </summary>
    public CardSetMode CardSetMode { get; set; } = CardSetMode.Shuffle;

    // ── NPC Decks ───────────────────────────────────────────────────────────

    /// <summary>
    /// When true, the set index bytes across all 256 NPC deck entries are
    /// Fisher-Yates shuffled. Each NPC opponent will draw from a different card pool.
    /// 255 RNG calls.
    /// </summary>
    public bool ShuffleNpcDecks { get; set; }

    /// <summary>
    /// Controls whether and how NPC deck difficulty bytes are randomized.
    /// Applied after set index shuffle when both are enabled.
    /// See <see cref="NpcDifficultyMode"/> for all options.
    /// </summary>
    public NpcDifficultyMode NpcDifficultyMode { get; set; } = NpcDifficultyMode.Preserve;

    // -------------------------------------------------------------------------
    // Serialization
    // -------------------------------------------------------------------------

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Serializes all settings to a formatted JSON string.
    /// Written to Settings-Seed-[int].json in the mod output folder.
    /// </summary>
    public string ToJson() => JsonSerializer.Serialize(this, _jsonOptions);

    /// <summary>
    /// Deserializes a Settings instance from a JSON string.
    /// </summary>
    public static Settings? FromJson(string json) =>
        JsonSerializer.Deserialize<Settings>(json, _jsonOptions);

    /// <summary>
    /// Encodes seed + settings into a short shareable alphanumeric string.
    /// </summary>
    /// <remarks>
    /// TODO (Phase 7): Implement compact settings string encoding.
    /// Format TBD — likely base62 or base64 over a packed bitfield.
    /// </remarks>
    public string ToSettingsString()
    {
        // TODO (Phase 7): implement shareable settings string encoding
        throw new NotImplementedException("ToSettingsString is not yet implemented — Phase 7.");
    }

    /// <summary>
    /// Reconstructs a Settings instance from a shareable settings string.
    /// </summary>
    /// <remarks>
    /// TODO (Phase 7): Implement compact settings string decoding.
    /// </remarks>
    public static Settings FromSettingsString(string s)
    {
        // TODO (Phase 7): implement shareable settings string decoding
        throw new NotImplementedException("FromSettingsString is not yet implemented — Phase 7.");
    }
}