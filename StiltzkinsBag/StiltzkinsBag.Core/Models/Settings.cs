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