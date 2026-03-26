using StiltzkinsBag.Models;
using StiltzkinsBag.Models.Csv;

namespace StiltzkinsBag.Randomizers;

/// <summary>
/// Randomizes the player's starting item set (InitialItems.csv).
///
/// Behaviour is controlled by <see cref="Settings.StartingItemMode"/>:
///
/// <list type="bullet">
///   <item><see cref="StartingItemMode.ConsumablesRandom"/> —
///     Fisher-Yates shuffle of IDs 236–253 (Potions, Ethers, Phoenix Downs, etc.).
///     17 RNG calls.</item>
///   <item><see cref="StartingItemMode.GearRandom"/> —
///     Shuffle across consumables (236–253) plus obtainable gear (Price &gt; 2).
///     pool.Count−1 RNG calls.</item>
///   <item><see cref="StartingItemMode.AbilityStarter"/> —
///     Shuffle across consumables (236–253) plus Magic Gems (224–235).
///     pool.Count−1 RNG calls.</item>
///   <item><see cref="StartingItemMode.SpeedRunner"/> —
///     Fixed output: 1× Potion + 1× Phoenix Down. 0 RNG calls.</item>
///   <item><see cref="StartingItemMode.AllItems"/> —
///     One of every item (IDs 0–254). Debug mode only. 0 RNG calls.</item>
/// </list>
///
/// When <see cref="Settings.RandomizeStartingCounts"/> is true and the mode
/// supports it, each stack count is independently randomized in [1, MaxVanillaCount].
/// SpeedRunner and AllItems always use fixed counts regardless of this flag.
///
/// <see cref="StartingItemMode.AllItems"/> is silently downgraded to
/// <see cref="StartingItemMode.ConsumablesRandom"/> unless
/// <see cref="Settings.IsDebugMode"/> is true.
/// </summary>
public sealed class InitialItemsRandomizer
{
    // -------------------------------------------------------------------------
    // Pool constants (verified against Items.csv)
    // -------------------------------------------------------------------------

    /// <summary>First Magic Gem item ID (inclusive). Garnet = 224.</summary>
    public const int GemPoolMin = 224;

    /// <summary>Last Magic Gem item ID (inclusive). Lapis Lazuli = 235.</summary>
    public const int GemPoolMax = 235;

    /// <summary>First consumable item ID (inclusive). Potion = 236.</summary>
    public const int ConsumablePoolMin = 236;

    /// <summary>Last consumable item ID (inclusive). Tent = 253.</summary>
    public const int ConsumablePoolMax = 253;

    /// <summary>Potion item ID. Used by SpeedRunner mode.</summary>
    public const int ItemIdPotion = 236;

    /// <summary>Phoenix Down item ID. Used by SpeedRunner mode.</summary>
    public const int ItemIdPhoenixDown = 240;

    /// <summary>Last valid item ID for AllItems mode (item 255 is empty/unused).</summary>
    public const int AllItemsMax = 254;

    /// <summary>
    /// Maximum stack count used when <see cref="Settings.RandomizeStartingCounts"/> is true.
    /// Matches the vanilla max (7× Potions in slot 0).
    /// </summary>
    public const int MaxRandomCount = 7;

    // -------------------------------------------------------------------------
    // Fields
    // -------------------------------------------------------------------------

    private readonly Random _rng;
    private readonly Settings _settings;

    /// <summary>
    /// All items from Items.csv. Required for <see cref="StartingItemMode.GearRandom"/>.
    /// If null and GearRandom is selected, falls back to ConsumablesRandom.
    /// </summary>
    private readonly IReadOnlyList<ItemsRow>? _allItems;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <param name="rng">The single shared seeded Random instance from SeedEngine.</param>
    /// <param name="settings">Current run settings.</param>
    /// <param name="allItems">
    /// All rows from Items.csv. Required for <see cref="StartingItemMode.GearRandom"/>.
    /// Pass null if gear mode is not needed.
    /// </param>
    public InitialItemsRandomizer(Random rng, Settings settings, IReadOnlyList<ItemsRow>? allItems = null)
    {
        _rng = rng ?? throw new ArgumentNullException(nameof(rng));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _allItems = allItems;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a new list of <see cref="InitialItemsRow"/> according to the active
    /// <see cref="StartingItemMode"/>. If <see cref="Settings.RandomizeInitialItems"/>
    /// is false, returns the input list unchanged.
    /// </summary>
    /// <param name="rows">Vanilla rows from InitialItems.csv. Must not be empty.</param>
    public List<InitialItemsRow> Randomize(List<InitialItemsRow> rows)
    {
        if (rows is null) throw new ArgumentNullException(nameof(rows));
        if (rows.Count == 0) throw new ArgumentException("InitialItems rows must not be empty.", nameof(rows));

        if (!_settings.RandomizeInitialItems)
            return rows;

        var effectiveMode = ResolveMode();

        return effectiveMode switch
        {
            StartingItemMode.SpeedRunner => BuildSpeedRunnerSet(),
            StartingItemMode.AllItems => BuildAllItemsSet(),
            _ => BuildShuffledSet(rows, effectiveMode)
        };
    }

    // -------------------------------------------------------------------------
    // Mode implementations
    // -------------------------------------------------------------------------

    /// <summary>
    /// SpeedRunner: fixed output, 0 RNG calls.
    /// Always returns exactly 2 rows regardless of vanilla count.
    /// </summary>
    private static List<InitialItemsRow> BuildSpeedRunnerSet() =>
    [
        new InitialItemsRow { ItemID = ItemIdPotion,      Count = 1 },
        new InitialItemsRow { ItemID = ItemIdPhoenixDown, Count = 1 }
    ];

    /// <summary>
    /// AllItems (debug): one of every item ID 0–254, count=1. 0 RNG calls.
    /// </summary>
    private static List<InitialItemsRow> BuildAllItemsSet()
    {
        var result = new List<InitialItemsRow>(AllItemsMax + 1);
        for (int id = 0; id <= AllItemsMax; id++)
            result.Add(new InitialItemsRow { ItemID = id, Count = 1 });
        return result;
    }

    /// <summary>
    /// ConsumablesRandom / GearRandom / AbilityStarter:
    /// Fisher-Yates shuffle the pool, take first rows.Count items,
    /// apply vanilla or randomized counts.
    /// </summary>
    private List<InitialItemsRow> BuildShuffledSet(List<InitialItemsRow> vanillaRows, StartingItemMode mode)
    {
        var pool = BuildPool(mode);

        // Full Fisher-Yates shuffle — pool.Count − 1 RNG calls.
        FisherYatesShuffle(pool);

        // Take first vanillaRows.Count items from the shuffled pool.
        int take = Math.Min(vanillaRows.Count, pool.Count);
        var result = new List<InitialItemsRow>(take);

        for (int i = 0; i < take; i++)
        {
            byte count = _settings.RandomizeStartingCounts
                ? (byte)_rng.Next(1, MaxRandomCount + 1)   // [1, 7] — 1 RNG call per slot
                : vanillaRows[i].Count;

            result.Add(new InitialItemsRow { ItemID = pool[i], Count = count });
        }

        return result;
    }

    // -------------------------------------------------------------------------
    // Pool builders
    // -------------------------------------------------------------------------

    private List<int> BuildPool(StartingItemMode mode) => mode switch
    {
        StartingItemMode.ConsumablesRandom => BuildConsumablePool(),
        StartingItemMode.AbilityStarter => BuildAbilityStarterPool(),
        StartingItemMode.GearRandom => BuildGearPool(),
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unexpected mode in BuildPool.")
    };

    /// <summary>Consumables only: IDs 236–253 (18 items).</summary>
    private static List<int> BuildConsumablePool()
    {
        var pool = new List<int>(ConsumablePoolMax - ConsumablePoolMin + 1);
        for (int id = ConsumablePoolMin; id <= ConsumablePoolMax; id++)
            pool.Add(id);
        return pool;
    }

    /// <summary>
    /// AbilityStarter: consumables (236–253) + Magic Gems (224–235) = 30 items.
    /// </summary>
    private static List<int> BuildAbilityStarterPool()
    {
        var pool = new List<int>();
        for (int id = GemPoolMin; id <= GemPoolMax; id++) pool.Add(id);
        for (int id = ConsumablePoolMin; id <= ConsumablePoolMax; id++) pool.Add(id);
        return pool;
    }

    /// <summary>
    /// GearRandom: consumables + obtainable gear (Weapon/Armlet/Helmet/Armor/Accessory = 1
    /// AND Price &gt; 2, to exclude story-only legendaries).
    /// Falls back to consumables if <see cref="_allItems"/> was not provided.
    /// </summary>
    private List<int> BuildGearPool()
    {
        var pool = BuildConsumablePool();

        if (_allItems is null)
            return pool; // graceful fallback — no gear data available

        foreach (var item in _allItems)
        {
            bool isGear = item.Weapon || item.Armlet || item.Helmet || item.Armor || item.Accessory;

            bool isObtainable = item.Price > 2; // Price <= 2 flags legendaries/story-only items

            if (isGear && isObtainable)
                pool.Add(item.Id);
        }

        return pool;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Resolves the effective <see cref="StartingItemMode"/>.
    /// Downgrades <see cref="StartingItemMode.AllItems"/> to
    /// <see cref="StartingItemMode.ConsumablesRandom"/> unless debug mode is active.
    /// </summary>
    private StartingItemMode ResolveMode()
    {
        if (_settings.StartingItemMode == StartingItemMode.AllItems && !_settings.IsDebugMode)
            return StartingItemMode.ConsumablesRandom;

        return _settings.StartingItemMode;
    }

    /// <summary>
    /// In-place Fisher-Yates shuffle using the shared <see cref="_rng"/> instance.
    /// Consumes exactly <c>pool.Count − 1</c> RNG calls.
    /// </summary>
    private void FisherYatesShuffle(List<int> pool)
    {
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
    }
}