using StiltzkinsBag.Models;
using StiltzkinsBag.Models.Csv;
using StiltzkinsBag.Randomizers;
using Xunit;

namespace StiltzkinsBag.Tests;

public class InitialItemsRandomizerTests
{
    // -------------------------------------------------------------------------
    // Test data helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Vanilla starting rows as parsed from InitialItems.csv.
    /// 236×7, 237×2, 238×2, 240×2, 247×2, 249×1, 253×1
    /// </summary>
    private static List<InitialItemsRow> VanillaRows() =>
    [
        new() { ItemID = 236, Count = 7 },
        new() { ItemID = 237, Count = 2 },
        new() { ItemID = 238, Count = 2 },
        new() { ItemID = 240, Count = 2 },
        new() { ItemID = 247, Count = 2 },
        new() { ItemID = 249, Count = 1 },
        new() { ItemID = 253, Count = 1 }
    ];

    /// <summary>
    /// Minimal fake ItemsRow list for GearRandom tests.
    /// Contains: 1 obtainable weapon, 1 legendary weapon, 1 consumable, 1 non-gear item.
    /// </summary>
    private static List<ItemsRow> FakeGearItems() =>
    [
        // Obtainable weapon — should appear in gear pool
        new() { Id = 1,  Weapon = true,  Price = 320,  Armlet = false, Helmet = false, Armor = false, Accessory = false },
        // Legendary weapon (Price <= 2) — must be excluded
        new() { Id = 30, Weapon = true,  Price = 2,    Armlet = false, Helmet = false, Armor = false, Accessory = false },
        // Obtainable armor — should appear in gear pool
        new() { Id = 100, Armor = true,  Price = 500,  Weapon = false, Armlet = false, Helmet = false, Accessory = false },
        // Consumable — not gear, should NOT appear via gear filter
        new() { Id = 236, Weapon = false, Armlet = false, Helmet = false, Armor = false, Accessory = false, Price = 50 },
    ];

    private static Settings MakeSettings(
        StartingItemMode mode = StartingItemMode.ConsumablesRandom,
        bool randomizeCounts = false,
        bool debugMode = false) => new()
        {
            RandomizeInitialItems = true,
            StartingItemMode = mode,
            RandomizeStartingCounts = randomizeCounts,
            IsDebugMode = debugMode
        };

    private static Random Rng(int seed = 42) => new(seed);

    // -------------------------------------------------------------------------
    // Disabled — passthrough
    // -------------------------------------------------------------------------

    [Fact]
    public void Disabled_ReturnsInputUnchanged()
    {
        var settings = new Settings { RandomizeInitialItems = false };
        var sut = new InitialItemsRandomizer(Rng(), settings);
        var rows = VanillaRows();

        var result = sut.Randomize(rows);

        Assert.Same(rows, result);
    }

    // -------------------------------------------------------------------------
    // ConsumablesRandom
    // -------------------------------------------------------------------------

    [Fact]
    public void ConsumablesRandom_RowCountMatchesVanilla()
    {
        var sut = new InitialItemsRandomizer(Rng(), MakeSettings());
        var result = sut.Randomize(VanillaRows());

        Assert.Equal(VanillaRows().Count, result.Count);
    }

    [Fact]
    public void ConsumablesRandom_AllIDsInConsumablePool()
    {
        var sut = new InitialItemsRandomizer(Rng(), MakeSettings());
        var result = sut.Randomize(VanillaRows());

        Assert.All(result, row =>
            Assert.InRange(row.ItemID,
                InitialItemsRandomizer.ConsumablePoolMin,
                InitialItemsRandomizer.ConsumablePoolMax));
    }

    [Fact]
    public void ConsumablesRandom_NoRepeatedItemIDs()
    {
        var sut = new InitialItemsRandomizer(Rng(), MakeSettings());
        var result = sut.Randomize(VanillaRows());

        var ids = result.Select(r => r.ItemID).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void ConsumablesRandom_VanillaCountsPreservedInOrder()
    {
        var vanilla = VanillaRows();
        var sut = new InitialItemsRandomizer(Rng(), MakeSettings());
        var result = sut.Randomize(vanilla);

        for (int i = 0; i < vanilla.Count; i++)
            Assert.Equal(vanilla[i].Count, result[i].Count);
    }

    [Fact]
    public void ConsumablesRandom_IsDeterministic()
    {
        var settings = MakeSettings();
        var result1 = new InitialItemsRandomizer(Rng(42), settings).Randomize(VanillaRows());
        var result2 = new InitialItemsRandomizer(Rng(42), settings).Randomize(VanillaRows());

        Assert.Equal(result1.Select(r => r.ItemID), result2.Select(r => r.ItemID));
        Assert.Equal(result1.Select(r => r.Count), result2.Select(r => r.Count));
    }

    [Fact]
    public void ConsumablesRandom_DifferentSeedsDifferentOutput()
    {
        var settings = MakeSettings();
        var result1 = new InitialItemsRandomizer(Rng(1), settings).Randomize(VanillaRows());
        var result2 = new InitialItemsRandomizer(Rng(99), settings).Randomize(VanillaRows());

        // At least one ID must differ (two identical shuffles with different seeds is
        // astronomically unlikely for a pool of 18 items taken 7)
        Assert.NotEqual(result1.Select(r => r.ItemID), result2.Select(r => r.ItemID));
    }

    // -------------------------------------------------------------------------
    // AbilityStarter
    // -------------------------------------------------------------------------

    [Fact]
    public void AbilityStarter_RowCountMatchesVanilla()
    {
        var sut = new InitialItemsRandomizer(Rng(), MakeSettings(StartingItemMode.AbilityStarter));
        var result = sut.Randomize(VanillaRows());

        Assert.Equal(VanillaRows().Count, result.Count);
    }

    [Fact]
    public void AbilityStarter_AllIDsInGemOrConsumableRange()
    {
        var sut = new InitialItemsRandomizer(Rng(), MakeSettings(StartingItemMode.AbilityStarter));
        var result = sut.Randomize(VanillaRows());

        Assert.All(result, row =>
        {
            bool inGems = row.ItemID >= InitialItemsRandomizer.GemPoolMin &&
                                 row.ItemID <= InitialItemsRandomizer.GemPoolMax;
            bool inConsumables = row.ItemID >= InitialItemsRandomizer.ConsumablePoolMin &&
                                 row.ItemID <= InitialItemsRandomizer.ConsumablePoolMax;
            Assert.True(inGems || inConsumables,
                $"ItemID {row.ItemID} is outside gem (224–235) and consumable (236–253) ranges.");
        });
    }

    [Fact]
    public void AbilityStarter_IsDeterministic()
    {
        var settings = MakeSettings(StartingItemMode.AbilityStarter);
        var result1 = new InitialItemsRandomizer(Rng(7), settings).Randomize(VanillaRows());
        var result2 = new InitialItemsRandomizer(Rng(7), settings).Randomize(VanillaRows());

        Assert.Equal(result1.Select(r => r.ItemID), result2.Select(r => r.ItemID));
    }

    // -------------------------------------------------------------------------
    // GearRandom
    // -------------------------------------------------------------------------

    [Fact]
    public void GearRandom_RowCountMatchesVanilla()
    {
        var sut = new InitialItemsRandomizer(Rng(), MakeSettings(StartingItemMode.GearRandom), FakeGearItems());
        var result = sut.Randomize(VanillaRows());

        Assert.Equal(VanillaRows().Count, result.Count);
    }

    [Fact]
    public void GearRandom_ExcludesLegendaries()
    {
        // FakeGearItems has item 30 (legendary, Price=2) — it must never appear.
        var sut = new InitialItemsRandomizer(Rng(), MakeSettings(StartingItemMode.GearRandom), FakeGearItems());
        var result = sut.Randomize(VanillaRows());

        Assert.DoesNotContain(result, r => r.ItemID == 30);
    }

    [Fact]
    public void GearRandom_NullItemsFallsBackToConsumables()
    {
        // allItems = null → graceful fallback, all IDs still in consumable range.
        var sut = new InitialItemsRandomizer(Rng(), MakeSettings(StartingItemMode.GearRandom), allItems: null);
        var result = sut.Randomize(VanillaRows());

        Assert.All(result, row =>
            Assert.InRange(row.ItemID,
                InitialItemsRandomizer.ConsumablePoolMin,
                InitialItemsRandomizer.ConsumablePoolMax));
    }

    [Fact]
    public void GearRandom_IsDeterministic()
    {
        var settings = MakeSettings(StartingItemMode.GearRandom);
        var items = FakeGearItems();
        var result1 = new InitialItemsRandomizer(Rng(3), settings, items).Randomize(VanillaRows());
        var result2 = new InitialItemsRandomizer(Rng(3), settings, items).Randomize(VanillaRows());

        Assert.Equal(result1.Select(r => r.ItemID), result2.Select(r => r.ItemID));
    }

    // -------------------------------------------------------------------------
    // SpeedRunner
    // -------------------------------------------------------------------------

    [Fact]
    public void SpeedRunner_AlwaysReturnsTwoRows()
    {
        var sut = new InitialItemsRandomizer(Rng(), MakeSettings(StartingItemMode.SpeedRunner));
        var result = sut.Randomize(VanillaRows());

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void SpeedRunner_FirstRowIsPotion()
    {
        var sut = new InitialItemsRandomizer(Rng(), MakeSettings(StartingItemMode.SpeedRunner));
        var result = sut.Randomize(VanillaRows());

        Assert.Equal(InitialItemsRandomizer.ItemIdPotion, result[0].ItemID);
        Assert.Equal(1, result[0].Count);
    }

    [Fact]
    public void SpeedRunner_SecondRowIsPhoenixDown()
    {
        var sut = new InitialItemsRandomizer(Rng(), MakeSettings(StartingItemMode.SpeedRunner));
        var result = sut.Randomize(VanillaRows());

        Assert.Equal(InitialItemsRandomizer.ItemIdPhoenixDown, result[1].ItemID);
        Assert.Equal(1, result[1].Count);
    }

    [Fact]
    public void SpeedRunner_OutputIsIdenticalAcrossSeeds()
    {
        var settings = MakeSettings(StartingItemMode.SpeedRunner);
        var result1 = new InitialItemsRandomizer(Rng(1), settings).Randomize(VanillaRows());
        var result2 = new InitialItemsRandomizer(Rng(9999), settings).Randomize(VanillaRows());

        Assert.Equal(result1.Select(r => r.ItemID), result2.Select(r => r.ItemID));
        Assert.Equal(result1.Select(r => r.Count), result2.Select(r => r.Count));
    }

    [Fact]
    public void SpeedRunner_RandomizeStartingCounts_HasNoEffect()
    {
        // SpeedRunner always returns count=1 per slot regardless of this flag.
        var settingsOff = MakeSettings(StartingItemMode.SpeedRunner, randomizeCounts: false);
        var settingsOn = MakeSettings(StartingItemMode.SpeedRunner, randomizeCounts: true);

        var resultOff = new InitialItemsRandomizer(Rng(42), settingsOff).Randomize(VanillaRows());
        var resultOn = new InitialItemsRandomizer(Rng(42), settingsOn).Randomize(VanillaRows());

        Assert.Equal(resultOff.Select(r => r.Count), resultOn.Select(r => r.Count));
    }

    // -------------------------------------------------------------------------
    // AllItems (debug mode)
    // -------------------------------------------------------------------------

    [Fact]
    public void AllItems_WithDebugMode_Returns255Rows()
    {
        var sut = new InitialItemsRandomizer(Rng(), MakeSettings(StartingItemMode.AllItems, debugMode: true));
        var result = sut.Randomize(VanillaRows());

        Assert.Equal(255, result.Count); // IDs 0–254
    }

    [Fact]
    public void AllItems_WithDebugMode_ContainsAllIDsFrom0To254()
    {
        var sut = new InitialItemsRandomizer(Rng(), MakeSettings(StartingItemMode.AllItems, debugMode: true));
        var result = sut.Randomize(VanillaRows());

        var ids = result.Select(r => r.ItemID).OrderBy(x => x).ToList();
        var expected = Enumerable.Range(0, 255).ToList();
        Assert.Equal(expected, ids);
    }

    [Fact]
    public void AllItems_WithDebugMode_AllCountsAreOne()
    {
        var sut = new InitialItemsRandomizer(Rng(), MakeSettings(StartingItemMode.AllItems, debugMode: true));
        var result = sut.Randomize(VanillaRows());

        Assert.All(result, row => Assert.Equal(1, row.Count));
    }

    [Fact]
    public void AllItems_WithoutDebugMode_DowngradesToConsumablesRandom()
    {
        // IsDebugMode = false — AllItems must silently degrade to ConsumablesRandom.
        var sut = new InitialItemsRandomizer(Rng(), MakeSettings(StartingItemMode.AllItems, debugMode: false));
        var result = sut.Randomize(VanillaRows());

        Assert.Equal(VanillaRows().Count, result.Count);
        Assert.All(result, row =>
            Assert.InRange(row.ItemID,
                InitialItemsRandomizer.ConsumablePoolMin,
                InitialItemsRandomizer.ConsumablePoolMax));
    }

    [Fact]
    public void AllItems_RandomizeStartingCounts_HasNoEffect()
    {
        var settingsOff = MakeSettings(StartingItemMode.AllItems, randomizeCounts: false, debugMode: true);
        var settingsOn = MakeSettings(StartingItemMode.AllItems, randomizeCounts: true, debugMode: true);

        var resultOff = new InitialItemsRandomizer(Rng(42), settingsOff).Randomize(VanillaRows());
        var resultOn = new InitialItemsRandomizer(Rng(42), settingsOn).Randomize(VanillaRows());

        Assert.All(resultOff, row => Assert.Equal(1, row.Count));
        Assert.All(resultOn, row => Assert.Equal(1, row.Count));
    }

    // -------------------------------------------------------------------------
    // RandomizeStartingCounts
    // -------------------------------------------------------------------------

    [Fact]
    public void RandomizeStartingCounts_AllCountsInRange()
    {
        var sut = new InitialItemsRandomizer(Rng(), MakeSettings(randomizeCounts: true));
        var result = sut.Randomize(VanillaRows());

        Assert.All(result, row =>
            Assert.InRange((int)row.Count, 1, InitialItemsRandomizer.MaxRandomCount));
    }

    [Fact]
    public void RandomizeStartingCounts_IsDeterministic()
    {
        var settings = MakeSettings(randomizeCounts: true);
        var result1 = new InitialItemsRandomizer(Rng(55), settings).Randomize(VanillaRows());
        var result2 = new InitialItemsRandomizer(Rng(55), settings).Randomize(VanillaRows());

        Assert.Equal(result1.Select(r => r.Count), result2.Select(r => r.Count));
    }

    [Fact]
    public void RandomizeStartingCounts_False_PreservesVanillaCountsInOrder()
    {
        var vanilla = VanillaRows();
        var settings = MakeSettings(randomizeCounts: false);
        var result = new InitialItemsRandomizer(Rng(), settings).Randomize(vanilla);

        for (int i = 0; i < vanilla.Count; i++)
            Assert.Equal(vanilla[i].Count, result[i].Count);
    }

    // -------------------------------------------------------------------------
    // Guard clauses
    // -------------------------------------------------------------------------

    [Fact]
    public void Randomize_NullRows_Throws()
    {
        var sut = new InitialItemsRandomizer(Rng(), MakeSettings());
        Assert.Throws<ArgumentNullException>(() => sut.Randomize(null!));
    }

    [Fact]
    public void Randomize_EmptyRows_Throws()
    {
        var sut = new InitialItemsRandomizer(Rng(), MakeSettings());
        Assert.Throws<ArgumentException>(() => sut.Randomize([]));
    }

    // -------------------------------------------------------------------------
    // FilthyRich
    // -------------------------------------------------------------------------

    /// <summary>Item pool for FilthyRich / JunkDrawer price-filter tests.</summary>
    private static List<ItemsRow> FakePricedItems() =>
    [
        new() { Id = 10,  Price = 50,   AbilityIds = [] }, // cheap — below FilthyRich default
        new() { Id = 11,  Price = 100,  AbilityIds = [] }, // mid
        new() { Id = 12,  Price = 200,  AbilityIds = [] }, // at default FilthyRich threshold
        new() { Id = 13,  Price = 500,  AbilityIds = [] }, // expensive
        new() { Id = 14,  Price = 1000, AbilityIds = [] }, // very expensive
        new() { Id = 1,   Price = 1,    AbilityIds = [] }, // key item — always excluded
        // Consumables with varying prices
        new() { Id = 236, Price = 50,  AbilityIds = [] },
        new() { Id = 237, Price = 100, AbilityIds = [] },
        new() { Id = 238, Price = 200, AbilityIds = [] },
        new() { Id = 239, Price = 30,  AbilityIds = [] },
        new() { Id = 240, Price = 75,  AbilityIds = [] },
    ];

    private static Settings MakeFilthyRichSettings(int threshold = 200) => new()
    {
        RandomizeInitialItems = true,
        StartingItemMode = StartingItemMode.FilthyRich,
        FilthyRichThreshold = threshold
    };

    [Fact]
    public void FilthyRich_AllItemsAtOrAboveThreshold()
    {
        var settings = MakeFilthyRichSettings(threshold: 200);
        var sut = new InitialItemsRandomizer(Rng(), settings, FakePricedItems());
        var result = sut.Randomize(VanillaRows());

        var priceById = FakePricedItems().ToDictionary(i => i.Id, i => i.Price);
        Assert.All(result, row =>
        {
            Assert.True(priceById.ContainsKey(row.ItemID),
                $"ItemID {row.ItemID} not found in item pool");
            Assert.True(priceById[row.ItemID] >= 200,
                $"ItemID {row.ItemID} has price {priceById[row.ItemID]} below threshold 200");
        });
    }

    [Fact]
    public void FilthyRich_ExcludesKeyItems()
    {
        // Threshold=1 would include key item if not filtered — key items must always be excluded.
        var settings = MakeFilthyRichSettings(threshold: 1);
        var sut = new InitialItemsRandomizer(Rng(), settings, FakePricedItems());
        var result = sut.Randomize(VanillaRows());

        Assert.DoesNotContain(result, r => r.ItemID == 1); // key item Price=1
    }

    [Fact]
    public void FilthyRich_NullItemsFallsBackToConsumables()
    {
        var settings = MakeFilthyRichSettings();
        var sut = new InitialItemsRandomizer(Rng(), settings, allItems: null);
        var result = sut.Randomize(VanillaRows());

        Assert.All(result, row =>
            Assert.InRange(row.ItemID,
                InitialItemsRandomizer.ConsumablePoolMin,
                InitialItemsRandomizer.ConsumablePoolMax));
    }

    [Fact]
    public void FilthyRich_IsDeterministic()
    {
        var settings = MakeFilthyRichSettings();
        var items = FakePricedItems();
        var r1 = new InitialItemsRandomizer(Rng(42), settings, items).Randomize(VanillaRows());
        var r2 = new InitialItemsRandomizer(Rng(42), settings, items).Randomize(VanillaRows());

        Assert.Equal(r1.Select(r => r.ItemID), r2.Select(r => r.ItemID));
    }

    [Fact]
    public void FilthyRich_EmptyPoolFallsBackToConsumables()
    {
        // Threshold set absurdly high — nothing qualifies, must fall back.
        var settings = MakeFilthyRichSettings(threshold: 999999);
        var sut = new InitialItemsRandomizer(Rng(), settings, FakePricedItems());
        var result = sut.Randomize(VanillaRows());

        Assert.All(result, row =>
            Assert.InRange(row.ItemID,
                InitialItemsRandomizer.ConsumablePoolMin,
                InitialItemsRandomizer.ConsumablePoolMax));
    }

    // -------------------------------------------------------------------------
    // JunkDrawer
    // -------------------------------------------------------------------------

    private static Settings MakeJunkDrawerSettings(int threshold = 50) => new()
    {
        RandomizeInitialItems = true,
        StartingItemMode = StartingItemMode.JunkDrawer,
        JunkDrawerThreshold = threshold
    };

    [Fact]
    public void JunkDrawer_AllItemsAreConsumables()
    {
        var settings = MakeJunkDrawerSettings(threshold: 100);
        var sut = new InitialItemsRandomizer(Rng(), settings, FakePricedItems());
        var result = sut.Randomize(VanillaRows());

        Assert.All(result, row =>
            Assert.InRange(row.ItemID,
                InitialItemsRandomizer.ConsumablePoolMin,
                InitialItemsRandomizer.ConsumablePoolMax));
    }

    [Fact]
    public void JunkDrawer_AllItemsAtOrBelowThreshold()
    {
        var settings = MakeJunkDrawerSettings(threshold: 75);
        var items = FakePricedItems();
        var sut = new InitialItemsRandomizer(Rng(), settings, items);
        var result = sut.Randomize(VanillaRows());

        var priceById = items.ToDictionary(i => i.Id, i => i.Price);
        Assert.All(result, row =>
        {
            if (!priceById.ContainsKey(row.ItemID)) return;
            Assert.True(priceById[row.ItemID] <= 75,
                $"ItemID {row.ItemID} has price {priceById[row.ItemID]} above threshold 75");
        });
    }

    [Fact]
    public void JunkDrawer_NullItemsFallsBackToConsumables()
    {
        var settings = MakeJunkDrawerSettings();
        var sut = new InitialItemsRandomizer(Rng(), settings, allItems: null);
        var result = sut.Randomize(VanillaRows());

        Assert.All(result, row =>
            Assert.InRange(row.ItemID,
                InitialItemsRandomizer.ConsumablePoolMin,
                InitialItemsRandomizer.ConsumablePoolMax));
    }

    [Fact]
    public void JunkDrawer_EmptyPoolFallsBackToConsumables()
    {
        // Threshold=0 — nothing qualifies (Price > 0 filter), must fall back.
        var settings = MakeJunkDrawerSettings(threshold: 0);
        var sut = new InitialItemsRandomizer(Rng(), settings, FakePricedItems());
        var result = sut.Randomize(VanillaRows());

        Assert.All(result, row =>
            Assert.InRange(row.ItemID,
                InitialItemsRandomizer.ConsumablePoolMin,
                InitialItemsRandomizer.ConsumablePoolMax));
    }

    [Fact]
    public void JunkDrawer_IsDeterministic()
    {
        var settings = MakeJunkDrawerSettings();
        var items = FakePricedItems();
        var r1 = new InitialItemsRandomizer(Rng(42), settings, items).Randomize(VanillaRows());
        var r2 = new InitialItemsRandomizer(Rng(42), settings, items).Randomize(VanillaRows());

        Assert.Equal(r1.Select(r => r.ItemID), r2.Select(r => r.ItemID));
    }
}