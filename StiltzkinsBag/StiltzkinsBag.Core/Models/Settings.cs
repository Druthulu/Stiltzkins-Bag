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
    /// and <see cref="Settings.AllStatsMaxed"/>.
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

    public bool RandomizeTreasureChests { get; set; }
    public bool RandomizeShops { get; set; }
    public bool RandomizeSynthesis { get; set; }

    /// <summary>Enables unique synthesis Bonus Set recipes for legendary items.</summary>
    public bool RandomizeBonusSets { get; set; }

    /// <summary>Allows medic/recovery items to appear in randomized shops.</summary>
    public bool ShopIncludeMedicItems { get; set; }

    /// <summary>Overrides dedicated medic shops with the general randomized pool.</summary>
    public bool ShopOverrideMedicShops { get; set; }

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
    // Tetramaster (bytecode only — TripleTriad.csv is NOT supported)
    // -------------------------------------------------------------------------

    public bool RandomizeTetraMaster { get; set; }
    public bool RandomizeCardStats { get; set; }
    public bool RandomizeCardOrder { get; set; }
    public bool RandomizeDecks { get; set; }

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