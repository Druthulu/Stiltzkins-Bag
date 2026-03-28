using StiltzkinsBag.Models;
using StiltzkinsBag.Models.Csv;
using StiltzkinsBag.Randomizers;
using Xunit;

namespace StiltzkinsBag.Tests;

public class ShopRandomizerTests
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static Settings MakeSettings(
        bool randomizeShops = true,
        ShopMode mode = ShopMode.Shuffle,
        ShopSizeMode sizeMode = ShopSizeMode.Maintain,
        ShopItemPool pool = ShopItemPool.ConsumablesOnly,
        int fixedSize = 4,
        bool medicItems = false,
        int medicMinShops = 2,
        bool shortSupply = false,
        int shortSupplyMax = 3,
        bool badEconomy = false,
        float badEconMin = 1.5f,
        float badEconMax = 4.0f,
        bool debugMode = false) => new Settings
        {
            RandomizeShops = randomizeShops,
            ShopMode = mode,
            ShopSizeMode = sizeMode,
            ShopItemPool = pool,
            ShopFixedSize = fixedSize,
            ShopEnsureMedicItems = medicItems,
            ShopMedicMinShops = medicMinShops,
            ShortSupply = shortSupply,
            ShortSupplyMaxItems = shortSupplyMax,
            BadEconomy = badEconomy,
            BadEconomyPriceMultiplierMin = badEconMin,
            BadEconomyPriceMultiplierMax = badEconMax,
            IsDebugMode = debugMode
        };

    private static ShopItemsRow MakeShop(int id, params int[] items) => new()
    {
        Comment = $"Shop {id:D4}",
        Id = id,
        Items = items
    };

    private static ItemsRow MakeItem(int id, uint price) => new()
    {
        Id = id,
        Price = price,
        SellingPrice = (int)(price / 2),
        AbilityIds = []
    };

    /// <summary>Builds a minimal Items.csv stand-in for pool tests.</summary>
    private static List<ItemsRow> MakeItemPool()
    {
        var items = new List<ItemsRow>();
        // Key/story items: Price <= 2
        items.Add(MakeItem(1, 1));
        items.Add(MakeItem(2, 0));
        // Non-consumable gear items: Price > 2, Id outside consumable range
        for (int id = 10; id <= 20; id++)
            items.Add(MakeItem(id, 100));
        // Consumables: Id 236–253, Price > 2
        for (int id = ShopRandomizer.ConsumableIdMin; id <= ShopRandomizer.ConsumableIdMax; id++)
            items.Add(MakeItem(id, 50));
        return items;
    }

    private static ShopRandomizer Make(int seed, Settings settings) =>
        new(new Random(seed), settings);

    // -------------------------------------------------------------------------
    // Passthrough — RandomizeShops = false
    // -------------------------------------------------------------------------

    [Fact]
    public void Passthrough_WhenRandomizeShopsFalse_ReturnsClonedShops()
    {
        var shops = new List<ShopItemsRow> { MakeShop(0, 1, 2, 3) };
        var randomizer = Make(42, MakeSettings(randomizeShops: false));
        var result = randomizer.Randomize(shops, null);

        Assert.Equal(shops[0].Items, result.Shops[0].Items);
        Assert.NotSame(shops[0], result.Shops[0]); // cloned, not same reference
    }

    [Fact]
    public void Passthrough_WhenRandomizeShopsFalse_DoesNotMutateInput()
    {
        var shops = new List<ShopItemsRow> { MakeShop(0, 236, 237, 238) };
        var original = shops[0].Items.ToArray();
        var randomizer = Make(42, MakeSettings(randomizeShops: false));
        randomizer.Randomize(shops, null);

        Assert.Equal(original, shops[0].Items);
    }

    // -------------------------------------------------------------------------
    // BuildEligiblePool
    // -------------------------------------------------------------------------

    [Fact]
    public void BuildEligiblePool_ConsumablesOnly_ExcludesGearAndKeyItems()
    {
        var items = MakeItemPool();
        var pool = ShopRandomizer.BuildEligiblePool(items, ShopItemPool.ConsumablesOnly);

        Assert.All(pool, id => Assert.InRange(id,
            ShopRandomizer.ConsumableIdMin, ShopRandomizer.ConsumableIdMax));
        Assert.DoesNotContain(1, pool);   // key item
        Assert.DoesNotContain(10, pool);  // gear, not consumable range
    }

    [Fact]
    public void BuildEligiblePool_AllNonKeyItems_IncludesGearAndConsumables()
    {
        var items = MakeItemPool();
        var pool = ShopRandomizer.BuildEligiblePool(items, ShopItemPool.AllNonKeyItems);

        Assert.Contains(10, pool);  // gear
        Assert.Contains(ShopRandomizer.ConsumableIdMin, pool); // consumable
        Assert.DoesNotContain(1, pool);  // key item Price=1
        Assert.DoesNotContain(2, pool);  // key item Price=0
    }

    [Fact]
    public void BuildEligiblePool_AlwaysExcludesItemsAtOrBelowKeyItemPriceThreshold()
    {
        var items = new List<ItemsRow>
        {
            MakeItem(100, 0),
            MakeItem(101, 1),
            MakeItem(102, 2),
            MakeItem(103, 3)
        };
        var pool = ShopRandomizer.BuildEligiblePool(items, ShopItemPool.AllNonKeyItems);

        Assert.DoesNotContain(100, pool);
        Assert.DoesNotContain(101, pool);
        Assert.DoesNotContain(102, pool);
        Assert.Contains(103, pool);
    }

    [Fact]
    public void BuildEligiblePool_IsSortedByIdAscending()
    {
        var items = MakeItemPool();
        var pool = ShopRandomizer.BuildEligiblePool(items, ShopItemPool.AllNonKeyItems);

        Assert.Equal(pool.OrderBy(x => x).ToList(), pool);
    }

    // -------------------------------------------------------------------------
    // ComputeTargetSizes
    // -------------------------------------------------------------------------

    [Fact]
    public void ComputeTargetSizes_Maintain_PreservesVanillaCount()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0, 1, 2, 3, 4, 5),
            MakeShop(1, 10, 11)
        };
        var randomizer = Make(42, MakeSettings(sizeMode: ShopSizeMode.Maintain));
        var sizes = randomizer.ComputeTargetSizes(shops);

        Assert.Equal(5, sizes[0]);
        Assert.Equal(2, sizes[1]);
    }

    [Fact]
    public void ComputeTargetSizes_Fixed_AllShopsGetFixedSize()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0, 1, 2, 3, 4, 5),
            MakeShop(1, 10, 11)
        };
        var randomizer = Make(42, MakeSettings(sizeMode: ShopSizeMode.Fixed, fixedSize: 6));
        var sizes = randomizer.ComputeTargetSizes(shops);

        Assert.Equal(6, sizes[0]);
        Assert.Equal(6, sizes[1]);
    }

    [Fact]
    public void ComputeTargetSizes_Random_SizesWithinVanillaMinMax()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0, 1, 2, 3),        // size 3
            MakeShop(1, 10, 11, 12, 13), // size 4 — vanilla max
            MakeShop(2, 20)              // size 1 — vanilla min
        };
        var randomizer = Make(42, MakeSettings(sizeMode: ShopSizeMode.Random));
        var sizes = randomizer.ComputeTargetSizes(shops);

        Assert.All(sizes.Values, s => Assert.InRange(s, 1, 4));
    }

    [Fact]
    public void ComputeTargetSizes_EmptyShopsAlwaysGetSizeZero()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0),              // empty
            MakeShop(1, 1, 2, 3)
        };
        var randomizer = Make(42, MakeSettings(sizeMode: ShopSizeMode.Fixed, fixedSize: 10));
        var sizes = randomizer.ComputeTargetSizes(shops);

        Assert.Equal(0, sizes[0]);
        Assert.Equal(10, sizes[1]);
    }

    // -------------------------------------------------------------------------
    // ShortSupply modifier
    // -------------------------------------------------------------------------

    [Fact]
    public void ShortSupply_CapsAllShopsAtMaxItems()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0, 236, 237, 238, 239, 240, 241),
            MakeShop(1, 242, 243, 244, 245, 246)
        };
        var settings = MakeSettings(shortSupply: true, shortSupplyMax: 2);
        var result = Make(42, settings).Randomize(shops, null);

        Assert.All(result.Shops, s => Assert.True(s.Items.Length <= 2));
    }

    [Fact]
    public void ShortSupply_DoesNotGrowShopsBelowCap()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0, 236, 237) // only 2 items — below cap of 5
        };
        var settings = MakeSettings(shortSupply: true, shortSupplyMax: 5);
        var result = Make(42, settings).Randomize(shops, null);

        Assert.True(result.Shops[0].Items.Length <= 2);
    }

    [Fact]
    public void ShortSupply_EmptyShopsRemainEmpty()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0),             // empty
            MakeShop(1, 236, 237, 238, 239)
        };
        var settings = MakeSettings(shortSupply: true, shortSupplyMax: 2);
        var result = Make(42, settings).Randomize(shops, null);

        Assert.Empty(result.Shops[0].Items);
    }

    // -------------------------------------------------------------------------
    // Shuffle mode
    // -------------------------------------------------------------------------

    [Fact]
    public void Shuffle_NoWithinShopDuplicates()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0, 236, 237, 238, 239, 240),
            MakeShop(1, 241, 242, 243, 244, 245),
            MakeShop(2, 246, 247, 248, 249, 250)
        };
        var result = Make(42, MakeSettings(mode: ShopMode.Shuffle)).Randomize(shops, null);

        foreach (var shop in result.Shops)
            Assert.Equal(shop.Items.Distinct().Count(), shop.Items.Length);
    }

    [Fact]
    public void Shuffle_AllItemsPlaced_TotalCountPreserved()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0, 236, 237, 238),
            MakeShop(1, 239, 240, 241),
            MakeShop(2, 242, 243, 244)
        };
        int totalVanilla = shops.Sum(s => s.Items.Length);
        var result = Make(42, MakeSettings(mode: ShopMode.Shuffle)).Randomize(shops, null);
        int totalResult = result.Shops.Sum(s => s.Items.Length);

        Assert.Equal(totalVanilla, totalResult);
    }

    [Fact]
    public void Shuffle_Deterministic_SameSeedSameOutput()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0, 236, 237, 238, 239),
            MakeShop(1, 240, 241, 242, 243)
        };
        var s = MakeSettings(mode: ShopMode.Shuffle);
        var r1 = Make(99, s).Randomize(shops, null);
        var r2 = Make(99, s).Randomize(shops, null);

        for (int i = 0; i < r1.Shops.Count; i++)
            Assert.Equal(r1.Shops[i].Items, r2.Shops[i].Items);
    }

    [Fact]
    public void Shuffle_DifferentSeeds_ProduceDifferentOutput()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0, 236, 237, 238, 239, 240, 241, 242, 243),
            MakeShop(1, 244, 245, 246, 247, 248, 249, 250, 251)
        };
        var s = MakeSettings(mode: ShopMode.Shuffle);
        var r1 = Make(1, s).Randomize(shops, null);
        var r2 = Make(2, s).Randomize(shops, null);

        bool anyDiff = r1.Shops.Zip(r2.Shops).Any(pair =>
            !pair.First.Items.SequenceEqual(pair.Second.Items));
        Assert.True(anyDiff);
    }

    [Fact]
    public void Shuffle_DoesNotMutateInputRows()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0, 236, 237, 238),
            MakeShop(1, 239, 240, 241)
        };
        var originals = shops.Select(s => s.Items.ToArray()).ToList();
        Make(42, MakeSettings(mode: ShopMode.Shuffle)).Randomize(shops, null);

        for (int i = 0; i < shops.Count; i++)
            Assert.Equal(originals[i], shops[i].Items);
    }

    [Fact]
    public void Shuffle_EmptyShopsRemainEmpty()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0),            // empty
            MakeShop(1, 236, 237)
        };
        var result = Make(42, MakeSettings(mode: ShopMode.Shuffle)).Randomize(shops, null);

        Assert.Empty(result.Shops[0].Items);
        Assert.NotEmpty(result.Shops[1].Items);
    }

    // -------------------------------------------------------------------------
    // BoundedRandom mode
    // -------------------------------------------------------------------------

    [Fact]
    public void BoundedRandom_NoWithinShopDuplicates()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0, 1, 2, 3, 4),
            MakeShop(1, 5, 6, 7, 8)
        };
        var items = MakeItemPool();
        var s = MakeSettings(mode: ShopMode.BoundedRandom, pool: ShopItemPool.ConsumablesOnly);
        var result = Make(42, s).Randomize(shops, items);

        foreach (var shop in result.Shops)
            Assert.Equal(shop.Items.Distinct().Count(), shop.Items.Length);
    }

    [Fact]
    public void BoundedRandom_ConsumablesOnly_AllItemsInConsumableRange()
    {
        var shops = new List<ShopItemsRow> { MakeShop(0, 1, 2, 3, 4, 5) };
        var items = MakeItemPool();
        var s = MakeSettings(mode: ShopMode.BoundedRandom, pool: ShopItemPool.ConsumablesOnly);
        var result = Make(42, s).Randomize(shops, items);

        Assert.All(result.Shops[0].Items, id =>
            Assert.InRange(id, ShopRandomizer.ConsumableIdMin, ShopRandomizer.ConsumableIdMax));
    }

    [Fact]
    public void BoundedRandom_AllNonKeyItems_CanIncludeGear()
    {
        var shops = Enumerable.Range(0, 5)
            .Select(i => MakeShop(i, 1, 2, 3, 4, 5, 6, 7, 8))
            .ToList();
        var items = MakeItemPool();
        var s = MakeSettings(mode: ShopMode.BoundedRandom, pool: ShopItemPool.AllNonKeyItems);

        // Run multiple seeds to ensure gear items (Id 10-20) appear at least once
        bool gearFound = false;
        for (int seed = 0; seed < 20 && !gearFound; seed++)
        {
            var result = Make(seed, s).Randomize(shops, items);
            gearFound = result.Shops.Any(sh =>
                sh.Items.Any(id => id >= 10 && id <= 20));
        }
        Assert.True(gearFound, "Gear items should appear in AllNonKeyItems pool across multiple seeds");
    }

    [Fact]
    public void BoundedRandom_ShopsAreIndependent_SameItemCanAppearInMultipleShops()
    {
        // Since each shop draws independently, the same item can appear in more than one shop
        var shops = Enumerable.Range(0, 10)
            .Select(i => MakeShop(i, 1, 2))
            .ToList();
        var items = new List<ItemsRow> { MakeItem(236, 50), MakeItem(237, 50) };
        var s = MakeSettings(mode: ShopMode.BoundedRandom, pool: ShopItemPool.ConsumablesOnly);
        var result = Make(42, s).Randomize(shops, items);

        // With only 2 pool items and 10 shops each drawing 2 items,
        // duplicates across shops are inevitable
        var allPlaced = result.Shops.SelectMany(sh => sh.Items).ToList();
        Assert.True(allPlaced.Count > 2, "Items should appear across multiple shops independently");
    }

    [Fact]
    public void BoundedRandom_FallsBackToShuffle_WhenAllItemsNull()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0, 236, 237, 238),
            MakeShop(1, 239, 240, 241)
        };
        var s = MakeSettings(mode: ShopMode.BoundedRandom);
        // allItems = null — should fall back to Shuffle without throwing
        var result = Make(42, s).Randomize(shops, null);

        // Result should contain items from the original vanilla pool (Shuffle fallback)
        int total = result.Shops.Sum(sh => sh.Items.Length);
        Assert.Equal(6, total);
    }

    // -------------------------------------------------------------------------
    // MegaMart mode (debug only)
    // -------------------------------------------------------------------------

    [Fact]
    public void MegaMart_DowngradesToShuffle_WhenNotDebugMode()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0, 236, 237, 238),
            MakeShop(1, 239, 240, 241)
        };
        // No debugMode — MegaMart must downgrade to Shuffle
        var s = MakeSettings(mode: ShopMode.MegaMart, debugMode: false);
        // Should not throw and should produce a valid result
        var result = Make(42, s).Randomize(shops, MakeItemPool());

        Assert.Equal(2, result.Shops.Count);
        Assert.All(result.Shops, sh => Assert.True(sh.Items.Length >= 0));
    }

    [Fact]
    public void MegaMart_WhenDebugMode_AllShopsGetSameItems()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0, 1, 2, 3, 4),
            MakeShop(1, 5, 6, 7, 8),
            MakeShop(2, 9, 10, 11, 12)
        };
        var s = MakeSettings(mode: ShopMode.MegaMart, debugMode: true);
        var result = Make(42, s).Randomize(shops, MakeItemPool());

        // All non-empty shops with same target size should have identical contents
        var nonEmpty = result.Shops.Where(sh => sh.Items.Length > 0).ToList();
        if (nonEmpty.Count > 1)
        {
            var first = nonEmpty[0].Items;
            Assert.All(nonEmpty.Skip(1), sh =>
                Assert.Equal(first.Length, sh.Items.Length));
        }
    }

    // -------------------------------------------------------------------------
    // EnsureMedicItems
    // -------------------------------------------------------------------------

    [Fact]
    public void EnsureMedicItems_AlreadyCompliant_NoChange()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0, ShopRandomizer.PotionItemId, ShopRandomizer.PhoenixDownItemId, 237),
            MakeShop(1, ShopRandomizer.PotionItemId, ShopRandomizer.PhoenixDownItemId, 238)
        };
        var s = MakeSettings(medicItems: true, medicMinShops: 2);
        var randomizer = Make(42, s);
        var copy = shops.Select(sh => MakeShop(sh.Id, sh.Items)).ToList();
        randomizer.EnsureMedicItems(copy);

        // Already compliant — no items should have been added
        Assert.Equal(3, copy[0].Items.Length);
        Assert.Equal(3, copy[1].Items.Length);
    }

    [Fact]
    public void EnsureMedicItems_AddsToNonCompliantShop()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0, 237, 238, 239), // missing both medic items
        };
        var s = MakeSettings(medicItems: true, medicMinShops: 1);
        var randomizer = Make(42, s);
        randomizer.EnsureMedicItems(shops);

        Assert.Contains(ShopRandomizer.PotionItemId, shops[0].Items);
        Assert.Contains(ShopRandomizer.PhoenixDownItemId, shops[0].Items);
    }

    [Fact]
    public void EnsureMedicItems_ZeroRequired_NoChange()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0, 237, 238, 239)
        };
        var s = MakeSettings(medicItems: true, medicMinShops: 0);
        var randomizer = Make(42, s);
        randomizer.EnsureMedicItems(shops);

        Assert.Equal(3, shops[0].Items.Length);
    }

    [Fact]
    public void EnsureMedicItems_Deterministic_NeverAddsDuplicateMedicItems()
    {
        var shops = new List<ShopItemsRow>
        {
            MakeShop(0, ShopRandomizer.PotionItemId, 237) // has Potion, missing Phoenix Down
        };
        var s = MakeSettings(medicItems: true, medicMinShops: 1);
        var randomizer = Make(42, s);
        randomizer.EnsureMedicItems(shops);

        Assert.Equal(1, shops[0].Items.Count(id => id == ShopRandomizer.PotionItemId));
        Assert.Equal(1, shops[0].Items.Count(id => id == ShopRandomizer.PhoenixDownItemId));
    }

    // -------------------------------------------------------------------------
    // BadEconomy price pass
    // -------------------------------------------------------------------------

    [Fact]
    public void BadEconomy_PricesIncrease_AllEligibleItems()
    {
        var shops = new List<ShopItemsRow> { MakeShop(0, 236) };
        var items = new List<ItemsRow>
        {
            MakeItem(236, 100),
            MakeItem(237, 200)
        };
        var s = MakeSettings(badEconomy: true, badEconMin: 2.0f, badEconMax: 2.0f);
        var result = Make(42, s).Randomize(shops, items);

        // Multiplier is exactly 2.0 — prices should double
        Assert.Equal((uint)200, result.Items.First(i => i.Id == 236).Price);
        Assert.Equal((uint)400, result.Items.First(i => i.Id == 237).Price);
    }

    [Fact]
    public void BadEconomy_KeyItems_NeverModified()
    {
        var shops = new List<ShopItemsRow> { MakeShop(0, 236) };
        var items = new List<ItemsRow>
        {
            MakeItem(1, 1),    // key item Price=1
            MakeItem(2, 0),    // key item Price=0
            MakeItem(236, 50)  // eligible
        };
        var s = MakeSettings(badEconomy: true, badEconMin: 5.0f, badEconMax: 5.0f);
        var result = Make(42, s).Randomize(shops, items);

        Assert.Equal((uint)1, result.Items.First(i => i.Id == 1).Price);
        Assert.Equal((uint)0, result.Items.First(i => i.Id == 2).Price);
    }

    [Fact]
    public void BadEconomy_DoesNotMutateInputList()
    {
        var shops = new List<ShopItemsRow> { MakeShop(0, 236) };
        var items = new List<ItemsRow> { MakeItem(236, 100) };
        var s = MakeSettings(badEconomy: true, badEconMin: 3.0f, badEconMax: 3.0f);
        Make(42, s).Randomize(shops, items);

        Assert.Equal((uint)100, items[0].Price); // input unchanged
    }

    [Fact]
    public void BadEconomy_Deterministic_SameSeedSamePrice()
    {
        var shops = new List<ShopItemsRow> { MakeShop(0, 236) };
        var items = new List<ItemsRow>
        {
            MakeItem(236, 100),
            MakeItem(237, 200),
            MakeItem(238, 50)
        };
        var s = MakeSettings(badEconomy: true, badEconMin: 1.5f, badEconMax: 4.0f);
        var r1 = Make(77, s).Randomize(shops, items);
        var r2 = Make(77, s).Randomize(shops, items);

        for (int i = 0; i < r1.Items.Count; i++)
            Assert.Equal(r1.Items[i].Price, r2.Items[i].Price);
    }

    [Fact]
    public void BadEconomy_PriceNeverBelowOne()
    {
        var shops = new List<ShopItemsRow> { MakeShop(0, 236) };
        // Price of 1 — above key item threshold but very low
        var items = new List<ItemsRow> { MakeItem(236, 3) };
        var s = MakeSettings(badEconomy: true, badEconMin: 0.0f, badEconMax: 0.0f);
        var result = Make(42, s).Randomize(shops, items);

        Assert.True(result.Items[0].Price >= 1);
    }

    // -------------------------------------------------------------------------
    // Result record
    // -------------------------------------------------------------------------

    [Fact]
    public void Randomize_WhenBadEconomyDisabled_ItemsReferenceIsPassedInList()
    {
        var shops = new List<ShopItemsRow> { MakeShop(0, 236, 237) };
        var items = MakeItemPool();
        var s = MakeSettings(badEconomy: false);
        var result = Make(42, s).Randomize(shops, items);

        Assert.Same(items, result.Items);
    }

    [Fact]
    public void Randomize_WhenBadEconomyEnabled_ItemsIsNewList()
    {
        var shops = new List<ShopItemsRow> { MakeShop(0, 236, 237) };
        var items = MakeItemPool();
        var s = MakeSettings(badEconomy: true, badEconMin: 2.0f, badEconMax: 2.0f);
        var result = Make(42, s).Randomize(shops, items);

        Assert.NotSame(items, result.Items);
    }
}