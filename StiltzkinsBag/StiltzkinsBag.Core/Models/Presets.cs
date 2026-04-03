using StiltzkinsBag.Models;

namespace StiltzkinsBag.App.Models;

/// <summary>
/// Factory methods for named randomizer presets.
/// Each preset returns a fully-populated Settings object.
/// GamePath and SeedString are always blank — the caller preserves those from the UI.
///
/// Preset philosophy:
///   Recommended — full experience, all backend features on, constraint-enforced
///   Chaos        — maximum randomness, no guardrails, everything cranked
///   Casual       — items + loot only, no character changes, gentle difficulty
///   Competitive  — race-tuned Recommended; Tetramaster off, no medic guarantee
/// </summary>
public static class Presets
{
    public static Settings? Get(string presetName) => presetName switch
    {
        "Recommended" => Recommended(),
        "Chaos" => Chaos(),
        "Casual" => Casual(),
        "Competitive" => Competitive(),
        _ => null
    };

    // -------------------------------------------------------------------------

    /// <summary>
    /// Full constraint-enforced experience. All features on.
    /// Sensible sub-option defaults — exciting but fair.
    /// </summary>
    public static Settings Recommended() => new()
    {
        Mode = RandomizerMode.Recommended,

        // ── Characters ───────────────────────────────────────────────────────
        RandomizeBaseStats = true,
        RandomizeSpeciality = true,
        RandomizeAbilities = true,
        RandomizeEquipment = true,
        EquipmentMode = EquipmentMode.Random,
        RandomizeInitialItems = true,
        StartingItemMode = StartingItemMode.ConsumablesRandom,
        RandomizeStartingCounts = false,

        // Gem costs shuffled — total budget preserved, keeps game equippable
        RandomizeAbilityGems = true,
        AbilityGemMode = AbilityGemMode.Shuffle,
        AbilityGemMinCost = 1,
        AbilityGemMaxCost = 20,

        // AP costs unchanged — vanilla pacing is the safest default
        RandomizeAbilityAp = false,
        AbilityApMode = AbilityApMode.ProportionalScale,
        ApScaleMinPercent = 50,
        ApScaleMaxPercent = 200,
        ApFlatCost = 20,

        // Gear stats — proportional with geometric weighting (generous but not broken)
        RandomizeGearStatBonuses = true,
        GearStatMode = GearStatMode.Proportional,
        GearStatWeighting = GearStatWeighting.Geometric,
        ZeroStatItemsCanGainStats = false,

        // ── Items ─────────────────────────────────────────────────────────────
        RandomizeTreasureChests = true,

        // Shops — shuffle vanilla pool, keep vanilla size, consumables only, medic guaranteed
        RandomizeShops = true,
        ShopMode = ShopMode.Shuffle,
        ShopSizeMode = ShopSizeMode.Maintain,
        ShopFixedSize = 4,
        ShopItemPool = ShopItemPool.ConsumablesOnly,
        ShopEnsureMedicItems = true,
        ShopMedicMinShops = 2,

        // Synthesis — shuffle results only (ingredients stay reachable)
        RandomizeSynthesis = true,
        RandomizeSynthesisResults = true,
        RandomizeSynthesisIngredients = false,
        AllowNewSynthesisResults = false,
        SynthesisPriceMode = SynthesisPriceMode.Vanilla,
        SynthPriceMin = 100,
        SynthPriceMax = 5000,
        SynthPriceScaleMinPercent = 50,
        SynthPriceScaleMaxPercent = 200,

        // No challenge modifiers in Recommended — fair baseline
        BadEconomy = false,
        BadEconomyPriceMultiplierMin = 1.5f,
        BadEconomyPriceMultiplierMax = 4.0f,
        ShortSupply = false,
        ShortSupplyMaxItems = 3,

        // ── Stiltzkin ─────────────────────────────────────────────────────────
        StiltzkinMode = StiltzkinMode.Recommended,
        StiltzkinRecommendedSubMode = StiltzkinRecommendedSubMode.Fun,
        StiltzkinPriceMode = StiltzkinPriceMode.Off,

        // ── Enemies ───────────────────────────────────────────────────────────
        RandomizeItemDrops = true,
        RandomizeItemSteals = true,
        RandomizeBlueMagic = true,
        RandomizeCardDrops = true,

        // ── Tetramaster ───────────────────────────────────────────────────────
        RandomizeTetraMaster = true,
        RandomizeCardStats = true,
        CardStatMode = CardStatMode.Shuffle,
        CardStatMin = 1,
        CardStatMax = 15,
        CardTypeMode = CardTypeMode.Preserve,   // keep P/M types vanilla
        ArrowMode = ArrowMode.Preserve,       // keep arrows vanilla
        ShuffleCardOrder = true,
        RandomizeCardSets = true,
        CardSetMode = CardSetMode.Shuffle,
        ShuffleNpcDecks = true,
        NpcDifficultyMode = NpcDifficultyMode.Preserve,
    };

    // -------------------------------------------------------------------------

    /// <summary>
    /// Maximum randomness. No guardrails. Everything cranked.
    /// </summary>
    public static Settings Chaos() => new()
    {
        Mode = RandomizerMode.Chaos,

        // ── Characters ───────────────────────────────────────────────────────
        RandomizeBaseStats = true,
        RandomizeSpeciality = true,
        RandomizeAbilities = true,
        RandomizeEquipment = true,
        EquipmentMode = EquipmentMode.Random,
        RandomizeInitialItems = true,
        StartingItemMode = StartingItemMode.GearRandom,  // gear in your starting bag
        RandomizeStartingCounts = true,

        // Gem costs fully randomized in a wide range
        RandomizeAbilityGems = true,
        AbilityGemMode = AbilityGemMode.BoundedRandom,
        AbilityGemMinCost = 1,
        AbilityGemMaxCost = 20,

        // AP costs scaled randomly — can make abilities very cheap or expensive
        RandomizeAbilityAp = true,
        AbilityApMode = AbilityApMode.ProportionalScale,
        ApScaleMinPercent = 25,
        ApScaleMaxPercent = 400,
        ApFlatCost = 20,

        // Gear stats — full Chaos mode, uniform distribution
        RandomizeGearStatBonuses = true,
        GearStatMode = GearStatMode.Chaos,
        GearStatWeighting = GearStatWeighting.Uniform,
        ZeroStatItemsCanGainStats = true,

        // ── Items ─────────────────────────────────────────────────────────────
        RandomizeTreasureChests = true,

        // Shops — draw from all non-key items, random size, no medic safety net
        RandomizeShops = true,
        ShopMode = ShopMode.BoundedRandom,
        ShopSizeMode = ShopSizeMode.Random,
        ShopFixedSize = 4,
        ShopItemPool = ShopItemPool.AllNonKeyItems,
        ShopEnsureMedicItems = false,
        ShopMedicMinShops = 2,

        // Synthesis — results AND ingredients randomized, new items allowed
        RandomizeSynthesis = true,
        RandomizeSynthesisResults = true,
        RandomizeSynthesisIngredients = true,
        AllowNewSynthesisResults = true,
        SynthesisPriceMode = SynthesisPriceMode.BoundedRandom,
        SynthPriceMin = 1,
        SynthPriceMax = 9999,
        SynthPriceScaleMinPercent = 50,
        SynthPriceScaleMaxPercent = 200,

        // No economy modifiers — Chaos is already extreme enough without price inflation
        BadEconomy = false,
        BadEconomyPriceMultiplierMin = 1.5f,
        BadEconomyPriceMultiplierMax = 4.0f,
        ShortSupply = false,
        ShortSupplyMaxItems = 3,

        // ── Stiltzkin ─────────────────────────────────────────────────────────
        StiltzkinMode = StiltzkinMode.Shuffle,
        StiltzkinRecommendedSubMode = StiltzkinRecommendedSubMode.Fun,
        StiltzkinPriceMode = StiltzkinPriceMode.StiltzkinsMood,

        // ── Enemies ───────────────────────────────────────────────────────────
        RandomizeItemDrops = true,
        RandomizeItemSteals = true,
        RandomizeBlueMagic = true,
        RandomizeCardDrops = true,

        // ── Tetramaster ───────────────────────────────────────────────────────
        RandomizeTetraMaster = true,
        RandomizeCardStats = true,
        CardStatMode = CardStatMode.BoundedRandom,
        CardStatMin = 1,
        CardStatMax = 15,
        CardTypeMode = CardTypeMode.Shuffle,
        ArrowMode = ArrowMode.Random,
        ShuffleCardOrder = true,
        RandomizeCardSets = true,
        CardSetMode = CardSetMode.BuildFromScratch,
        ShuffleNpcDecks = true,
        NpcDifficultyMode = NpcDifficultyMode.Shuffle,
    };

    // -------------------------------------------------------------------------

    /// <summary>
    /// Gentle run for newcomers.
    /// Characters untouched. Items and loot randomized. Medic items guaranteed.
    /// </summary>
    public static Settings Casual() => new()
    {
        Mode = RandomizerMode.Recommended,

        // ── Characters — all off ─────────────────────────────────────────────
        RandomizeBaseStats = false,
        RandomizeSpeciality = false,
        RandomizeAbilities = false,
        RandomizeEquipment = false,
        RandomizeInitialItems = false,
        StartingItemMode = StartingItemMode.ConsumablesRandom,
        RandomizeStartingCounts = false,

        RandomizeAbilityGems = false,
        AbilityGemMode = AbilityGemMode.Shuffle,
        AbilityGemMinCost = 1,
        AbilityGemMaxCost = 20,

        RandomizeAbilityAp = false,
        AbilityApMode = AbilityApMode.ProportionalScale,
        ApScaleMinPercent = 50,
        ApScaleMaxPercent = 200,
        ApFlatCost = 20,

        RandomizeGearStatBonuses = false,
        GearStatMode = GearStatMode.Proportional,
        GearStatWeighting = GearStatWeighting.VanillaWeighted,
        ZeroStatItemsCanGainStats = false,

        // ── Items ─────────────────────────────────────────────────────────────
        RandomizeTreasureChests = true,

        // Shops — shuffle vanilla pool, keep sizes, consumables only, medic guaranteed
        RandomizeShops = true,
        ShopMode = ShopMode.Shuffle,
        ShopSizeMode = ShopSizeMode.Maintain,
        ShopFixedSize = 4,
        ShopItemPool = ShopItemPool.ConsumablesOnly,
        ShopEnsureMedicItems = true,
        ShopMedicMinShops = 2,

        // Synthesis — vanilla recipes unchanged, just shuffle results
        RandomizeSynthesis = true,
        RandomizeSynthesisResults = true,
        RandomizeSynthesisIngredients = false,
        AllowNewSynthesisResults = false,
        SynthesisPriceMode = SynthesisPriceMode.Vanilla,
        SynthPriceMin = 100,
        SynthPriceMax = 5000,
        SynthPriceScaleMinPercent = 50,
        SynthPriceScaleMaxPercent = 200,

        BadEconomy = false,
        BadEconomyPriceMultiplierMin = 1.5f,
        BadEconomyPriceMultiplierMax = 4.0f,
        ShortSupply = false,
        ShortSupplyMaxItems = 3,

        // ── Stiltzkin — off for simplicity ────────────────────────────────────
        StiltzkinMode = StiltzkinMode.Off,
        StiltzkinRecommendedSubMode = StiltzkinRecommendedSubMode.Fun,
        StiltzkinPriceMode = StiltzkinPriceMode.Off,

        // ── Enemies — drops and steals only ──────────────────────────────────
        RandomizeItemDrops = true,
        RandomizeItemSteals = true,
        RandomizeBlueMagic = false,
        RandomizeCardDrops = false,

        // ── Tetramaster — off ─────────────────────────────────────────────────
        RandomizeTetraMaster = false,
        RandomizeCardStats = false,
        CardStatMode = CardStatMode.Shuffle,
        CardStatMin = 1,
        CardStatMax = 15,
        CardTypeMode = CardTypeMode.Preserve,
        ArrowMode = ArrowMode.Preserve,
        ShuffleCardOrder = false,
        RandomizeCardSets = false,
        CardSetMode = CardSetMode.Shuffle,
        ShuffleNpcDecks = false,
        NpcDifficultyMode = NpcDifficultyMode.Preserve,
    };

    // -------------------------------------------------------------------------

    /// <summary>
    /// Race-tuned Recommended.
    /// All main features on. Tetramaster off. No medic guarantee — racers manage resources.
    /// </summary>
    public static Settings Competitive() => new()
    {
        Mode = RandomizerMode.Recommended,

        // ── Characters ───────────────────────────────────────────────────────
        RandomizeBaseStats = true,
        RandomizeSpeciality = true,
        RandomizeAbilities = true,
        RandomizeEquipment = true,
        EquipmentMode = EquipmentMode.Random,
        RandomizeInitialItems = true,
        StartingItemMode = StartingItemMode.ConsumablesRandom,
        RandomizeStartingCounts = false,

        RandomizeAbilityGems = true,
        AbilityGemMode = AbilityGemMode.Shuffle,
        AbilityGemMinCost = 1,
        AbilityGemMaxCost = 20,

        // AP unchanged — consistent knowledge across racers
        RandomizeAbilityAp = false,
        AbilityApMode = AbilityApMode.ProportionalScale,
        ApScaleMinPercent = 50,
        ApScaleMaxPercent = 200,
        ApFlatCost = 20,

        RandomizeGearStatBonuses = true,
        GearStatMode = GearStatMode.Proportional,
        GearStatWeighting = GearStatWeighting.Geometric,
        ZeroStatItemsCanGainStats = false,

        // ── Items ─────────────────────────────────────────────────────────────
        RandomizeTreasureChests = true,

        // Shops — shuffle vanilla pool, consumables only, no medic guarantee
        RandomizeShops = true,
        ShopMode = ShopMode.Shuffle,
        ShopSizeMode = ShopSizeMode.Maintain,
        ShopFixedSize = 4,
        ShopItemPool = ShopItemPool.ConsumablesOnly,
        ShopEnsureMedicItems = false,
        ShopMedicMinShops = 2,

        // Synthesis — shuffle results, keep ingredients reachable
        RandomizeSynthesis = true,
        RandomizeSynthesisResults = true,
        RandomizeSynthesisIngredients = false,
        AllowNewSynthesisResults = false,
        SynthesisPriceMode = SynthesisPriceMode.Vanilla,
        SynthPriceMin = 100,
        SynthPriceMax = 5000,
        SynthPriceScaleMinPercent = 50,
        SynthPriceScaleMaxPercent = 200,

        BadEconomy = false,
        BadEconomyPriceMultiplierMin = 1.5f,
        BadEconomyPriceMultiplierMax = 4.0f,
        ShortSupply = false,
        ShortSupplyMaxItems = 3,

        // ── Stiltzkin ─────────────────────────────────────────────────────────
        StiltzkinMode = StiltzkinMode.Recommended,
        StiltzkinRecommendedSubMode = StiltzkinRecommendedSubMode.Fun,
        StiltzkinPriceMode = StiltzkinPriceMode.Off,

        // ── Enemies ───────────────────────────────────────────────────────────
        RandomizeItemDrops = true,
        RandomizeItemSteals = true,
        RandomizeBlueMagic = true,
        RandomizeCardDrops = true,

        // ── Tetramaster — off for races ───────────────────────────────────────
        RandomizeTetraMaster = false,
        RandomizeCardStats = false,
        CardStatMode = CardStatMode.Shuffle,
        CardStatMin = 1,
        CardStatMax = 15,
        CardTypeMode = CardTypeMode.Preserve,
        ArrowMode = ArrowMode.Preserve,
        ShuffleCardOrder = false,
        RandomizeCardSets = false,
        CardSetMode = CardSetMode.Shuffle,
        ShuffleNpcDecks = false,
        NpcDifficultyMode = NpcDifficultyMode.Preserve,
    };
}