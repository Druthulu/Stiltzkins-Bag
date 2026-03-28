using StiltzkinsBag.Models;
using StiltzkinsBag.Models.Csv;
using StiltzkinsBag.Randomizers;
using Xunit;

namespace StiltzkinsBag.Tests;

public class SynthesisRandomizerTests
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static Settings MakeSettings(
        bool randomizeSynthesis = true,
        bool randomizeResults = true,
        bool randomizeIngredients = false,
        bool allowNew = false,
        SynthesisPriceMode priceMode = SynthesisPriceMode.Vanilla,
        int synthPriceMin = 100,
        int synthPriceMax = 5000,
        int synthScaleMin = 50,
        int synthScaleMax = 200,
        bool badEconomy = false,
        float badEconMin = 2.0f,
        float badEconMax = 2.0f,
        RandomizerMode mode = RandomizerMode.Recommended) => new Settings
        {
            RandomizeSynthesis = randomizeSynthesis,
            RandomizeSynthesisResults = randomizeResults,
            RandomizeSynthesisIngredients = randomizeIngredients,
            AllowNewSynthesisResults = allowNew,
            SynthesisPriceMode = priceMode,
            SynthPriceMin = synthPriceMin,
            SynthPriceMax = synthPriceMax,
            SynthPriceScaleMinPercent = synthScaleMin,
            SynthPriceScaleMaxPercent = synthScaleMax,
            BadEconomy = badEconomy,
            BadEconomyPriceMultiplierMin = badEconMin,
            BadEconomyPriceMultiplierMax = badEconMax,
            Mode = mode
        };

    private static SynthesisRow MakeRecipe(
        int id,
        int result,
        uint price = 1000,
        int[]? ingredients = null,
        int[]? shops = null) => new()
        {
            Comment = $"Recipe {id:D4}",
            Id = id,
            Shops = shops ?? [0],
            Price = price,
            Result = result,
            Ingredients = ingredients ?? [id * 10, id * 10 + 1]
        };

    private static ItemsRow MakeItem(int id, uint price, int bonusId = 0) => new()
    {
        Id = id,
        Price = price,
        BonusId = bonusId,
        AbilityIds = []
    };

    private static StatsRow MakeStats(int id, int dex = 0, int str = 0, int mag = 0, int wil = 0) =>
        new()
        {
            Id = id,
            Dexterity = (byte)dex,
            Strength = (byte)str,
            Magic = (byte)mag,
            Will = (byte)wil
        };

    private static SynthesisRandomizer Make(int seed, Settings settings) =>
        new(new Random(seed), settings);

    /// <summary>Standard set of 5 recipes for general tests.</summary>
    private static List<SynthesisRow> MakeRecipes() =>
    [
        MakeRecipe(0, result: 10, price: 500,  ingredients: [1, 2]),
        MakeRecipe(1, result: 20, price: 800,  ingredients: [3, 4]),
        MakeRecipe(2, result: 30, price: 1200, ingredients: [5, 6]),
        MakeRecipe(3, result: 40, price: 2000, ingredients: [7, 8]),
        MakeRecipe(4, result: 50, price: 3000, ingredients: [9, 10])
    ];

    private static List<ItemsRow> MakeItemPool() =>
    [
        MakeItem(1,  1),    // key item — Price <= 2
        MakeItem(2,  0),    // key item — Price = 0
        MakeItem(10, 100),
        MakeItem(20, 200),
        MakeItem(30, 300),
        MakeItem(40, 400),
        MakeItem(50, 500),
        MakeItem(60, 600),
        MakeItem(70, 700)
    ];

    // -------------------------------------------------------------------------
    // Passthrough — RandomizeSynthesis = false
    // -------------------------------------------------------------------------

    [Fact]
    public void Passthrough_WhenRandomizeSynthesisFalse_ReturnsClonedRecipes()
    {
        var recipes = MakeRecipes();
        var result = Make(42, MakeSettings(randomizeSynthesis: false))
            .Randomize(recipes, null, null);

        Assert.Equal(recipes.Count, result.Recipes.Count);
        for (int i = 0; i < recipes.Count; i++)
        {
            Assert.Equal(recipes[i].Result, result.Recipes[i].Result);
            Assert.Equal(recipes[i].Price, result.Recipes[i].Price);
            Assert.Equal(recipes[i].Ingredients, result.Recipes[i].Ingredients);
            Assert.NotSame(recipes[i], result.Recipes[i]);
        }
    }

    [Fact]
    public void Passthrough_DoesNotMutateInput()
    {
        var recipes = MakeRecipes();
        var origResults = recipes.Select(r => r.Result).ToArray();
        Make(42, MakeSettings(randomizeSynthesis: false))
            .Randomize(recipes, null, null);

        Assert.Equal(origResults, recipes.Select(r => r.Result).ToArray());
    }

    // -------------------------------------------------------------------------
    // Result shuffle — vanilla mode
    // -------------------------------------------------------------------------

    [Fact]
    public void ShuffleResults_VanillaMode_OnlyUsesExistingResultIds()
    {
        var recipes = MakeRecipes();
        var vanillaResults = recipes.Select(r => r.Result).ToHashSet();
        var s = MakeSettings(randomizeResults: true, allowNew: false);
        var result = Make(42, s).Randomize(recipes, null, null);

        Assert.All(result.Recipes, r => Assert.Contains(r.Result, vanillaResults));
    }

    [Fact]
    public void ShuffleResults_VanillaMode_AllResultIdsPresent()
    {
        var recipes = MakeRecipes();
        var vanillaResults = recipes.Select(r => r.Result).OrderBy(x => x).ToList();
        var s = MakeSettings(randomizeResults: true, allowNew: false);
        var result = Make(42, s).Randomize(recipes, null, null);
        var shuffledResults = result.Recipes.Select(r => r.Result).OrderBy(x => x).ToList();

        Assert.Equal(vanillaResults, shuffledResults);
    }

    [Fact]
    public void ShuffleResults_Deterministic_SameSeedSameOutput()
    {
        var recipes = MakeRecipes();
        var s = MakeSettings(randomizeResults: true, allowNew: false);
        var r1 = Make(77, s).Randomize(recipes, null, null);
        var r2 = Make(77, s).Randomize(recipes, null, null);

        Assert.Equal(
            r1.Recipes.Select(r => r.Result),
            r2.Recipes.Select(r => r.Result));
    }

    [Fact]
    public void ShuffleResults_DifferentSeeds_ProduceDifferentOutput()
    {
        var recipes = MakeRecipes();
        var s = MakeSettings(randomizeResults: true, allowNew: false);
        var r1 = Make(1, s).Randomize(recipes, null, null);
        var r2 = Make(2, s).Randomize(recipes, null, null);

        bool anyDiff = r1.Recipes.Zip(r2.Recipes)
            .Any(pair => pair.First.Result != pair.Second.Result);
        Assert.True(anyDiff);
    }

    [Fact]
    public void ShuffleResults_DoesNotMutateInputRows()
    {
        var recipes = MakeRecipes();
        var origResults = recipes.Select(r => r.Result).ToArray();
        Make(42, MakeSettings(randomizeResults: true))
            .Randomize(recipes, null, null);

        Assert.Equal(origResults, recipes.Select(r => r.Result).ToArray());
    }

    // -------------------------------------------------------------------------
    // Result shuffle — AllowNew mode
    // -------------------------------------------------------------------------

    [Fact]
    public void AllowNew_DrawsFromItemPool_NotLimitedToVanillaResults()
    {
        var recipes = MakeRecipes();
        var vanillaResults = recipes.Select(r => r.Result).ToHashSet();
        var items = MakeItemPool();
        var s = MakeSettings(randomizeResults: true, allowNew: true);

        // Run multiple seeds — at least one should produce a result outside vanilla set
        bool newResultFound = false;
        for (int seed = 0; seed < 20 && !newResultFound; seed++)
        {
            var result = Make(seed, s).Randomize(recipes, items, null);
            newResultFound = result.Recipes.Any(r => !vanillaResults.Contains(r.Result));
        }
        Assert.True(newResultFound, "AllowNew should produce results outside vanilla set");
    }

    [Fact]
    public void AllowNew_RecommendedMode_ExcludesKeyItems()
    {
        var recipes = MakeRecipes();
        var items = MakeItemPool();
        var s = MakeSettings(randomizeResults: true, allowNew: true,
            mode: RandomizerMode.Recommended);

        for (int seed = 0; seed < 10; seed++)
        {
            var result = Make(seed, s).Randomize(recipes, items, null);
            // Key items have Id=1 (Price=1) and Id=2 (Price=0)
            Assert.All(result.Recipes, r => Assert.NotEqual(1, r.Result));
            Assert.All(result.Recipes, r => Assert.NotEqual(2, r.Result));
        }
    }

    [Fact]
    public void AllowNew_WhenAllItemsNull_FallsBackToVanillaShuffle()
    {
        var recipes = MakeRecipes();
        var vanillaResults = recipes.Select(r => r.Result).ToHashSet();
        var s = MakeSettings(randomizeResults: true, allowNew: true);
        var result = Make(42, s).Randomize(recipes, null, null);

        // With null allItems, must fall back — only vanilla result IDs appear
        Assert.All(result.Recipes, r => Assert.Contains(r.Result, vanillaResults));
    }

    // -------------------------------------------------------------------------
    // Ingredient shuffle
    // -------------------------------------------------------------------------

    [Fact]
    public void ShuffleIngredients_IngredientSetsRedistributed()
    {
        var recipes = MakeRecipes();
        var vanillaSets = recipes.Select(r => r.Ingredients.ToArray()).ToList();
        var s = MakeSettings(randomizeResults: false, randomizeIngredients: true);
        var result = Make(42, s).Randomize(recipes, null, null);

        // Every result ingredient set must be one of the original sets
        foreach (var recipe in result.Recipes)
        {
            bool found = vanillaSets.Any(vs => vs.SequenceEqual(recipe.Ingredients));
            Assert.True(found, $"Ingredient set on recipe {recipe.Id} not from vanilla set");
        }
    }

    [Fact]
    public void ShuffleIngredients_AllSetsPresent()
    {
        var recipes = MakeRecipes();
        var vanillaSets = recipes.Select(r => string.Join(",", r.Ingredients)).ToHashSet();
        var s = MakeSettings(randomizeResults: false, randomizeIngredients: true);
        var result = Make(42, s).Randomize(recipes, null, null);
        var resultSets = result.Recipes.Select(r => string.Join(",", r.Ingredients)).ToHashSet();

        Assert.Equal(vanillaSets, resultSets);
    }

    [Fact]
    public void ShuffleIngredients_Deterministic_SameSeedSameOutput()
    {
        var recipes = MakeRecipes();
        var s = MakeSettings(randomizeResults: false, randomizeIngredients: true);
        var r1 = Make(55, s).Randomize(recipes, null, null);
        var r2 = Make(55, s).Randomize(recipes, null, null);

        for (int i = 0; i < r1.Recipes.Count; i++)
            Assert.Equal(r1.Recipes[i].Ingredients, r2.Recipes[i].Ingredients);
    }

    [Fact]
    public void ShuffleIngredients_DoesNotAffectResults()
    {
        var recipes = MakeRecipes();
        var origResults = recipes.Select(r => r.Result).ToArray();
        var s = MakeSettings(randomizeResults: false, randomizeIngredients: true);
        var result = Make(42, s).Randomize(recipes, null, null);

        Assert.Equal(origResults, result.Recipes.Select(r => r.Result).ToArray());
    }

    // -------------------------------------------------------------------------
    // Both flags together
    // -------------------------------------------------------------------------

    [Fact]
    public void BothFlags_ResultsAndIngredientsChanged()
    {
        var recipes = MakeRecipes();
        var origResults = recipes.Select(r => r.Result).ToArray();
        var origIngredients = recipes.Select(r => string.Join(",", r.Ingredients)).ToArray();
        var s = MakeSettings(randomizeResults: true, randomizeIngredients: true);

        // Run a few seeds — at least one should change both
        bool resultChanged = false, ingredientsChanged = false;
        for (int seed = 0; seed < 10; seed++)
        {
            var result = Make(seed, s).Randomize(recipes, null, null);
            var newResults = result.Recipes.Select(r => r.Result).ToArray();
            var newIngredients = result.Recipes
                .Select(r => string.Join(",", r.Ingredients)).ToArray();
            if (!newResults.SequenceEqual(origResults)) resultChanged = true;
            if (!newIngredients.SequenceEqual(origIngredients)) ingredientsChanged = true;
        }
        Assert.True(resultChanged, "Results should change when RandomizeSynthesisResults=true");
        Assert.True(ingredientsChanged, "Ingredients should change when RandomizeSynthesisIngredients=true");
    }

    // -------------------------------------------------------------------------
    // Chaos mode
    // -------------------------------------------------------------------------

    [Fact]
    public void ChaosMode_ForcesAllFlagsOn_EvenWhenFlagsAreFalse()
    {
        var recipes = MakeRecipes();
        var items = MakeItemPool();
        // All individual flags off — Chaos should override
        var s = MakeSettings(
            randomizeResults: false,
            randomizeIngredients: false,
            allowNew: false,
            mode: RandomizerMode.Chaos);

        bool resultChanged = false, ingredientsChanged = false;
        var origResults = recipes.Select(r => r.Result).ToArray();
        var origIngredients = recipes.Select(r => string.Join(",", r.Ingredients)).ToArray();

        for (int seed = 0; seed < 20; seed++)
        {
            var result = Make(seed, s).Randomize(recipes, items, null);
            if (!result.Recipes.Select(r => r.Result).SequenceEqual(origResults))
                resultChanged = true;
            if (!result.Recipes.Select(r => string.Join(",", r.Ingredients))
                    .SequenceEqual(origIngredients))
                ingredientsChanged = true;
        }
        Assert.True(resultChanged, "Chaos mode must shuffle results regardless of flag");
        Assert.True(ingredientsChanged, "Chaos mode must shuffle ingredients regardless of flag");
    }

    [Fact]
    public void ChaosMode_AllowNew_CanIncludeKeyItems()
    {
        // In Chaos mode, key items (Price <= 2) ARE in the pool
        var recipes = MakeRecipes();
        var items = MakeItemPool(); // contains Id=1 (Price=1) and Id=2 (Price=0)
        var pool = SynthesisRandomizer.BuildResultPool(items, allowNew: true, isChaos: true);

        Assert.Contains(1, pool);
        Assert.Contains(2, pool);
    }

    // -------------------------------------------------------------------------
    // BuildResultPool
    // -------------------------------------------------------------------------

    [Fact]
    public void BuildResultPool_AllowNewFalse_ReturnsEmptyList()
    {
        var items = MakeItemPool();
        var pool = SynthesisRandomizer.BuildResultPool(items, allowNew: false, isChaos: false);
        Assert.Empty(pool);
    }

    [Fact]
    public void BuildResultPool_Recommended_ExcludesKeyItems()
    {
        var items = MakeItemPool();
        var pool = SynthesisRandomizer.BuildResultPool(items, allowNew: true, isChaos: false);

        Assert.DoesNotContain(1, pool); // Price=1
        Assert.DoesNotContain(2, pool); // Price=0
        Assert.Contains(10, pool);
    }

    [Fact]
    public void BuildResultPool_Chaos_IncludesAllItems()
    {
        var items = MakeItemPool();
        var pool = SynthesisRandomizer.BuildResultPool(items, allowNew: true, isChaos: true);

        Assert.Contains(1, pool);
        Assert.Contains(2, pool);
        Assert.Equal(items.Count, pool.Count);
    }

    [Fact]
    public void BuildResultPool_IsSortedByIdAscending()
    {
        var items = MakeItemPool();
        var pool = SynthesisRandomizer.BuildResultPool(items, allowNew: true, isChaos: false);
        Assert.Equal(pool.OrderBy(x => x).ToList(), pool);
    }

    // -------------------------------------------------------------------------
    // Price modes
    // -------------------------------------------------------------------------

    [Fact]
    public void PriceMode_Vanilla_PricesUnchanged()
    {
        var recipes = MakeRecipes();
        var origPrices = recipes.Select(r => r.Price).ToArray();
        var s = MakeSettings(randomizeResults: false, priceMode: SynthesisPriceMode.Vanilla);
        var result = Make(42, s).Randomize(recipes, null, null);

        Assert.Equal(origPrices, result.Recipes.Select(r => r.Price).ToArray());
    }

    [Fact]
    public void PriceMode_BoundedRandom_PricesWithinRange()
    {
        var recipes = MakeRecipes();
        var s = MakeSettings(randomizeResults: false,
            priceMode: SynthesisPriceMode.BoundedRandom,
            synthPriceMin: 200,
            synthPriceMax: 1000);
        var result = Make(42, s).Randomize(recipes, null, null);

        Assert.All(result.Recipes, r =>
            Assert.InRange(r.Price, 200u, 1000u));
    }

    [Fact]
    public void PriceMode_BoundedRandom_Deterministic()
    {
        var recipes = MakeRecipes();
        var s = MakeSettings(randomizeResults: false,
            priceMode: SynthesisPriceMode.BoundedRandom,
            synthPriceMin: 100,
            synthPriceMax: 5000);
        var r1 = Make(33, s).Randomize(recipes, null, null);
        var r2 = Make(33, s).Randomize(recipes, null, null);

        Assert.Equal(
            r1.Recipes.Select(r => r.Price),
            r2.Recipes.Select(r => r.Price));
    }

    [Fact]
    public void PriceMode_ProportionalScale_PricesScaled()
    {
        var recipes = MakeRecipes();
        // Scale exactly 200% — all prices should double
        var s = MakeSettings(randomizeResults: false,
            priceMode: SynthesisPriceMode.ProportionalScale,
            synthScaleMin: 200,
            synthScaleMax: 200);
        var result = Make(42, s).Randomize(recipes, null, null);

        for (int i = 0; i < recipes.Count; i++)
            Assert.Equal((uint)Math.Round(recipes[i].Price * 2.0), result.Recipes[i].Price);
    }

    [Fact]
    public void PriceMode_ProportionalScale_PriceNeverBelowOne()
    {
        var recipes = new List<SynthesisRow> { MakeRecipe(0, result: 10, price: 1) };
        var s = MakeSettings(randomizeResults: false,
            priceMode: SynthesisPriceMode.ProportionalScale,
            synthScaleMin: 0,
            synthScaleMax: 0);
        var result = Make(42, s).Randomize(recipes, null, null);

        Assert.True(result.Recipes[0].Price >= 1);
    }

    [Fact]
    public void PriceMode_GearScored_UsesStatSum()
    {
        // Result item has BonusId=5; Stats row Id=5 has str=2, mag=2 → statSum=4
        // Expected price = 500 + 4 * 500 = 2500
        var recipes = new List<SynthesisRow> { MakeRecipe(0, result: 10) };
        var items = new List<ItemsRow> { MakeItem(10, 300, bonusId: 5) };
        var stats = new List<StatsRow> { MakeStats(5, str: 2, mag: 2) };

        var s = MakeSettings(randomizeResults: false,
            priceMode: SynthesisPriceMode.GearScored);
        var result = Make(42, s).Randomize(recipes, items, stats);

        Assert.Equal(2500u, result.Recipes[0].Price);
    }

    [Fact]
    public void PriceMode_GearScored_FallsBackToVanilla_WhenStatsRowNull()
    {
        var recipes = new List<SynthesisRow> { MakeRecipe(0, result: 10, price: 999) };
        var items = new List<ItemsRow> { MakeItem(10, 300, bonusId: 5) };

        var s = MakeSettings(randomizeResults: false,
            priceMode: SynthesisPriceMode.GearScored);
        // statsRows = null → fallback
        var result = Make(42, s).Randomize(recipes, items, null);

        Assert.Equal(999u, result.Recipes[0].Price);
    }

    [Fact]
    public void PriceMode_GearScored_FallsBackToVanilla_WhenBonusIdZero()
    {
        var recipes = new List<SynthesisRow> { MakeRecipe(0, result: 10, price: 888) };
        var items = new List<ItemsRow> { MakeItem(10, 300, bonusId: 0) }; // BonusId=0
        var stats = new List<StatsRow> { MakeStats(0) };

        var s = MakeSettings(randomizeResults: false,
            priceMode: SynthesisPriceMode.GearScored);
        var result = Make(42, s).Randomize(recipes, items, stats);

        Assert.Equal(888u, result.Recipes[0].Price);
    }

    [Fact]
    public void PriceMode_GearScored_ZeroStats_UsesBasePrice()
    {
        // BonusId=3, statsRow has all zeros → price = GearScoredBasePrice + 0 = 500
        var recipes = new List<SynthesisRow> { MakeRecipe(0, result: 10) };
        var items = new List<ItemsRow> { MakeItem(10, 300, bonusId: 3) };
        var stats = new List<StatsRow> { MakeStats(3) }; // all zero

        var s = MakeSettings(randomizeResults: false,
            priceMode: SynthesisPriceMode.GearScored);
        var result = Make(42, s).Randomize(recipes, items, stats);

        Assert.Equal(SynthesisRandomizer.GearScoredBasePrice, result.Recipes[0].Price);
    }

    // -------------------------------------------------------------------------
    // BadEconomy
    // -------------------------------------------------------------------------

    [Fact]
    public void BadEconomy_PricesMultiplied()
    {
        var recipes = new List<SynthesisRow> { MakeRecipe(0, result: 10, price: 1000) };
        // Exact 2× multiplier
        var s = MakeSettings(randomizeResults: false,
            badEconomy: true, badEconMin: 2.0f, badEconMax: 2.0f);
        var result = Make(42, s).Randomize(recipes, null, null);

        Assert.Equal(2000u, result.Recipes[0].Price);
    }

    [Fact]
    public void BadEconomy_Deterministic_SameSeedSamePrice()
    {
        var recipes = MakeRecipes();
        var s = MakeSettings(randomizeResults: false,
            badEconomy: true, badEconMin: 1.5f, badEconMax: 4.0f);
        var r1 = Make(99, s).Randomize(recipes, null, null);
        var r2 = Make(99, s).Randomize(recipes, null, null);

        Assert.Equal(
            r1.Recipes.Select(r => r.Price),
            r2.Recipes.Select(r => r.Price));
    }

    [Fact]
    public void BadEconomy_PriceNeverBelowOne()
    {
        var recipes = new List<SynthesisRow> { MakeRecipe(0, result: 10, price: 1) };
        var s = MakeSettings(randomizeResults: false,
            badEconomy: true, badEconMin: 0.0f, badEconMax: 0.0f);
        var result = Make(42, s).Randomize(recipes, null, null);

        Assert.True(result.Recipes[0].Price >= 1u);
    }

    [Fact]
    public void BadEconomy_DoesNotMutateInputRows()
    {
        var recipes = MakeRecipes();
        var origPrices = recipes.Select(r => r.Price).ToArray();
        var s = MakeSettings(randomizeResults: false,
            badEconomy: true, badEconMin: 3.0f, badEconMax: 3.0f);
        Make(42, s).Randomize(recipes, null, null);

        Assert.Equal(origPrices, recipes.Select(r => r.Price).ToArray());
    }

    // -------------------------------------------------------------------------
    // ShopIds and structure preservation
    // -------------------------------------------------------------------------

    [Fact]
    public void Randomize_ShopAssignments_NeverModified()
    {
        var recipes = MakeRecipes();
        var origShops = recipes.Select(r => r.Shops.ToArray()).ToList();
        var s = MakeSettings(randomizeResults: true, randomizeIngredients: true);
        var result = Make(42, s).Randomize(recipes, null, null);

        for (int i = 0; i < recipes.Count; i++)
            Assert.Equal(origShops[i], result.Recipes[i].Shops);
    }

    [Fact]
    public void Randomize_RecipeIds_NeverModified()
    {
        var recipes = MakeRecipes();
        var origIds = recipes.Select(r => r.Id).ToArray();
        var s = MakeSettings(randomizeResults: true, randomizeIngredients: true);
        var result = Make(42, s).Randomize(recipes, null, null);

        Assert.Equal(origIds, result.Recipes.Select(r => r.Id).ToArray());
    }

    // -------------------------------------------------------------------------
    // ComputeGearScoredPrice static method
    // -------------------------------------------------------------------------

    [Fact]
    public void ComputeGearScoredPrice_CorrectFormula()
    {
        // statSum = 1+2+3+4 = 10 → 500 + 10*500 = 5500
        var recipe = MakeRecipe(0, result: 10, price: 999);
        var items = new List<ItemsRow> { MakeItem(10, 100, bonusId: 1) };
        var stats = new List<StatsRow> { MakeStats(1, dex: 1, str: 2, mag: 3, wil: 4) };

        var price = SynthesisRandomizer.ComputeGearScoredPrice(recipe, items, stats);

        Assert.Equal(5500u, price);
    }

    [Fact]
    public void ComputeGearScoredPrice_FallsBack_WhenResultItemNotFound()
    {
        var recipe = MakeRecipe(0, result: 999, price: 777); // result Id 999 not in items
        var items = new List<ItemsRow> { MakeItem(10, 100, bonusId: 1) };
        var stats = new List<StatsRow> { MakeStats(1, str: 5) };

        var price = SynthesisRandomizer.ComputeGearScoredPrice(recipe, items, stats);

        Assert.Equal(777u, price);
    }
}