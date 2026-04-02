using System;
using System.Collections.Generic;
using System.Linq;
using StiltzkinsBag.Core.Models;
using StiltzkinsBag.Models.Csv;
using StiltzkinsBag.Randomizers;
using Xunit;

namespace StiltzkinsBag.Tests;

/// <summary>
/// Tests for <see cref="RecommendedLogicEngine.EnforceSynthesisReachability"/>.
///
/// Rule 8A — Ingredient reachability (field ID proxy):
///   If an ingredient's earliest known field source is at or after a shop's field ID,
///   that shop is removed from the recipe's Shops array.
///   Recipes with no surviving shops are dropped entirely.
///
/// Rule 8B — Finite result cap:
///   Recipes whose Result is a Unique item must not exceed the item's vanilla finite count.
///   Excess recipes (sorted by Id, keep lowest) are dropped.
///
/// Shop field IDs used in fixtures (from SynthesisShopData):
///   Shop 32 → field 560   (earliest)
///   Shop 33 → field 902
///   Shop 36 → field 2453
///   Shop 37 → field 2801
///
/// Protected item IDs: 221 (Ribbon, vanilla count = 1).
/// </summary>
public class RecommendedLogicEngineSynthesisTests
{
    // ── Fixture helpers ────────────────────────────────────────────────────────

    private static readonly IReadOnlyDictionary<int, int> ShopFieldIds =
        SynthesisShopData.ShopFieldIds;

    private static readonly IReadOnlySet<int> FiniteProtectedIds =
        new HashSet<int> { 221 };  // Ribbon

    /// <summary>
    /// Builds an ItemObtainabilityEntry for a shop-sold (infinite) ingredient.
    /// </summary>
    private static ItemObtainabilityEntry InfiniteIngredient(int id) =>
        new() { ItemId = id, IsInShop = true };

    /// <summary>
    /// Builds an ItemObtainabilityEntry for a field-chest-only ingredient
    /// with a known minimum field ID.
    /// </summary>
    private static ItemObtainabilityEntry FieldIngredient(int id, int minFieldId,
        IReadOnlyDictionary<int, int> fieldIdMap)
        => new() { ItemId = id, IsFieldItem = true, FieldInstanceCount = 1 };
    // Note: the actual min field ID lives in the itemIdToMinFieldId dict, not the entry.

    /// <summary>
    /// Builds an ItemObtainabilityEntry for a boss-drop-only ingredient
    /// (finite, no field source).
    /// </summary>
    private static ItemObtainabilityEntry BossIngredient(int id) =>
        new() { ItemId = id, BossInstanceCount = 1, IsFieldItem = false };

    /// <summary>
    /// Builds a minimal SynthesisRow.
    /// </summary>
    private static SynthesisRow Recipe(int id, int result, int[] shops, int[] ingredients) =>
        new()
        {
            Comment = $"Recipe {id}",
            Id = id,
            Result = result,
            Shops = shops,
            Price = 1000,
            Ingredients = ingredients,
        };

    /// <summary>
    /// Builds a minimal catalog with the given entries.
    /// </summary>
    private static Dictionary<int, ItemObtainabilityEntry> Catalog(
        params ItemObtainabilityEntry[] entries) =>
        entries.ToDictionary(e => e.ItemId);

    // ── Rule 8A: reachable infinite ingredient → shop preserved ───────────────

    [Fact]
    public void RuleA_InfiniteIngredient_ShopPreserved()
    {
        var catalog = Catalog(InfiniteIngredient(1));
        var recipe = Recipe(0, 10, new[] { 32 }, new[] { 1 });

        var result = RecommendedLogicEngine.EnforceSynthesisReachability(
            new[] { recipe }, ShopFieldIds, new Dictionary<int, int>(),
            catalog, FiniteProtectedIds, new Random(1));

        Assert.Single(result);
        Assert.Same(recipe, result[0]);  // unchanged — original reference
    }

    // ── Rule 8A: field ingredient reachable (min field < shop field) ──────────

    [Fact]
    public void RuleA_FieldIngredient_ReachableBefore_ShopPreserved()
    {
        // Ingredient min field 500, shop 32 field 560 → 500 < 560 → reachable.
        var catalog = Catalog(new ItemObtainabilityEntry
        { ItemId = 1, IsFieldItem = true, FieldInstanceCount = 1 });
        var fieldIds = new Dictionary<int, int> { { 1, 500 } };
        var recipe = Recipe(0, 10, new[] { 32 }, new[] { 1 });

        var result = RecommendedLogicEngine.EnforceSynthesisReachability(
            new[] { recipe }, ShopFieldIds, fieldIds,
            catalog, FiniteProtectedIds, new Random(1));

        Assert.Single(result);
        Assert.Contains(32, result[0].Shops);
    }

    // ── Rule 8A: field ingredient NOT reachable (min field >= shop field) ─────

    [Fact]
    public void RuleA_FieldIngredient_NotReachableBefore_ShopRemoved()
    {
        // Ingredient min field 800, shop 32 field 560 → 800 >= 560 → NOT reachable.
        var catalog = Catalog(new ItemObtainabilityEntry
        { ItemId = 1, IsFieldItem = true, FieldInstanceCount = 1 });
        var fieldIds = new Dictionary<int, int> { { 1, 800 } };
        var recipe = Recipe(0, 10, new[] { 32 }, new[] { 1 });

        var result = RecommendedLogicEngine.EnforceSynthesisReachability(
            new[] { recipe }, ShopFieldIds, fieldIds,
            catalog, FiniteProtectedIds, new Random(1));

        // Shop 32 removed; recipe has no surviving shops → dropped.
        Assert.Empty(result);
    }

    // ── Rule 8A: multi-shop — unreachable for early shop, reachable for later ─

    [Fact]
    public void RuleA_MultiShop_EarlyShopRemoved_LateShopPreserved()
    {
        // Ingredient min field 1000.
        // Shop 32 field 560: 1000 >= 560 → removed.
        // Shop 36 field 2453: 1000 < 2453 → preserved.
        var catalog = Catalog(new ItemObtainabilityEntry
        { ItemId = 1, IsFieldItem = true, FieldInstanceCount = 1 });
        var fieldIds = new Dictionary<int, int> { { 1, 1000 } };
        var recipe = Recipe(0, 10, new[] { 32, 36 }, new[] { 1 });

        var result = RecommendedLogicEngine.EnforceSynthesisReachability(
            new[] { recipe }, ShopFieldIds, fieldIds,
            catalog, FiniteProtectedIds, new Random(1));

        Assert.Single(result);
        Assert.DoesNotContain(32, result[0].Shops);
        Assert.Contains(36, result[0].Shops);
        Assert.NotSame(recipe, result[0]);  // new row instance
    }

    // ── Rule 8A: all shops removed → recipe dropped ───────────────────────────

    [Fact]
    public void RuleA_AllShopsRemoved_RecipeDropped()
    {
        var catalog = Catalog(new ItemObtainabilityEntry
        { ItemId = 1, IsFieldItem = true, FieldInstanceCount = 1 });
        var fieldIds = new Dictionary<int, int> { { 1, 9999 } };
        // Both shops have field IDs well below 9999.
        var recipe = Recipe(0, 10, new[] { 32, 33 }, new[] { 1 });

        var result = RecommendedLogicEngine.EnforceSynthesisReachability(
            new[] { recipe }, ShopFieldIds, fieldIds,
            catalog, FiniteProtectedIds, new Random(1));

        Assert.Empty(result);
    }

    // ── Rule 8A: unknown shop ID → shop preserved (conservative) ─────────────

    [Fact]
    public void RuleA_UnknownShopId_Preserved()
    {
        var catalog = Catalog(new ItemObtainabilityEntry
        { ItemId = 1, IsFieldItem = true, FieldInstanceCount = 1 });
        var fieldIds = new Dictionary<int, int> { { 1, 9999 } };
        var recipe = Recipe(0, 10, new[] { 99 }, new[] { 1 });  // shop 99 = unknown

        var result = RecommendedLogicEngine.EnforceSynthesisReachability(
            new[] { recipe }, ShopFieldIds, fieldIds,
            catalog, FiniteProtectedIds, new Random(1));

        Assert.Single(result);
        Assert.Contains(99, result[0].Shops);
    }

    // ── Rule 8A: no shops → passes through as original reference ─────────────

    [Fact]
    public void RuleA_NoShops_PassesThrough()
    {
        var recipe = Recipe(0, 10, Array.Empty<int>(), new[] { 1 });

        var result = RecommendedLogicEngine.EnforceSynthesisReachability(
            new[] { recipe }, ShopFieldIds, new Dictionary<int, int>(),
            new Dictionary<int, ItemObtainabilityEntry>(),
            FiniteProtectedIds, new Random(1));

        Assert.Single(result);
        Assert.Same(recipe, result[0]);
    }

    // ── Rule 8A: boss-only ingredient (no field source) → treated as reachable─

    [Fact]
    public void RuleA_BossIngredient_NoFieldSource_TreatedAsReachable()
    {
        // Boss drop — finite but we don't know its field ID. Treated as reachable.
        var catalog = Catalog(BossIngredient(1));
        var recipe = Recipe(0, 10, new[] { 32 }, new[] { 1 });

        var result = RecommendedLogicEngine.EnforceSynthesisReachability(
            new[] { recipe }, ShopFieldIds, new Dictionary<int, int>(),
            catalog, FiniteProtectedIds, new Random(1));

        Assert.Single(result);
        Assert.Same(recipe, result[0]);
    }

    // ── Rule 8A: unknown ingredient (not in catalog) → treated as reachable ───

    [Fact]
    public void RuleA_UnknownIngredient_TreatedAsReachable()
    {
        // Ingredient 99 not in catalog.
        var recipe = Recipe(0, 10, new[] { 32 }, new[] { 99 });

        var result = RecommendedLogicEngine.EnforceSynthesisReachability(
            new[] { recipe }, ShopFieldIds, new Dictionary<int, int>(),
            new Dictionary<int, ItemObtainabilityEntry>(),
            FiniteProtectedIds, new Random(1));

        Assert.Single(result);
        Assert.Same(recipe, result[0]);
    }

    // ── Rule 8A: corrected row preserves all non-Shops fields ─────────────────

    [Fact]
    public void RuleA_CorrectedRow_PreservesNonShopFields()
    {
        var catalog = Catalog(new ItemObtainabilityEntry
        { ItemId = 1, IsFieldItem = true, FieldInstanceCount = 1 });
        var fieldIds = new Dictionary<int, int> { { 1, 1000 } };
        // Shop 32 (field 560) removed; shop 36 (field 2453) preserved.
        var recipe = new SynthesisRow
        {
            Comment = "Special Recipe",
            Id = 42,
            Shops = new[] { 32, 36 },
            Price = 9999,
            Result = 10,
            Ingredients = new[] { 1, 2 },
        };

        var result = RecommendedLogicEngine.EnforceSynthesisReachability(
            new[] { recipe }, ShopFieldIds, fieldIds,
            catalog, FiniteProtectedIds, new Random(1));

        Assert.Single(result);
        Assert.Equal("Special Recipe", result[0].Comment);
        Assert.Equal(42, result[0].Id);
        Assert.Equal(9999u, result[0].Price);
        Assert.Equal(10, result[0].Result);
        Assert.Equal(new[] { 1, 2 }, result[0].Ingredients);
    }

    // ── Rule 8B: single recipe for Unique item → no change ────────────────────

    [Fact]
    public void RuleB_SingleRecipe_UniqueResult_NoChange()
    {
        var catalog = Catalog(new ItemObtainabilityEntry
        { ItemId = 221, BossInstanceCount = 1 });
        var recipe = Recipe(0, 221, new[] { 36 }, new[] { 1 });

        var result = RecommendedLogicEngine.EnforceSynthesisReachability(
            new[] { recipe }, ShopFieldIds, new Dictionary<int, int>(),
            catalog, FiniteProtectedIds, new Random(1));

        Assert.Single(result);
        Assert.Same(recipe, result[0]);
    }

    // ── Rule 8B: two recipes, vanilla count 1 → second (higher Id) dropped ────

    [Fact]
    public void RuleB_TwoRecipes_VanillaCountOne_SecondDropped()
    {
        var catalog = Catalog(new ItemObtainabilityEntry
        { ItemId = 221, BossInstanceCount = 1 });  // vanilla count = 1
        var recipe0 = Recipe(0, 221, new[] { 36 }, new[] { 1 });
        var recipe1 = Recipe(1, 221, new[] { 37 }, new[] { 2 });

        var result = RecommendedLogicEngine.EnforceSynthesisReachability(
            new[] { recipe0, recipe1 }, ShopFieldIds, new Dictionary<int, int>(),
            catalog, FiniteProtectedIds, new Random(1));

        Assert.Single(result);
        Assert.Same(recipe0, result[0]);  // Id 0 kept; Id 1 dropped
    }

    // ── Rule 8B: Unique result with infinite source → no cap applied ──────────

    [Fact]
    public void RuleB_UniqueResult_HasInfiniteSource_NoCap()
    {
        var catalog = Catalog(new ItemObtainabilityEntry
        { ItemId = 221, IsInShop = true });  // infinite source
        var recipe0 = Recipe(0, 221, new[] { 36 }, new[] { 1 });
        var recipe1 = Recipe(1, 221, new[] { 37 }, new[] { 2 });

        var result = RecommendedLogicEngine.EnforceSynthesisReachability(
            new[] { recipe0, recipe1 }, ShopFieldIds, new Dictionary<int, int>(),
            catalog, FiniteProtectedIds, new Random(1));

        Assert.Equal(2, result.Count);  // both preserved
    }

    // ── Rule 8B: non-protected result → no cap ────────────────────────────────

    [Fact]
    public void RuleB_NonProtectedResult_NoCap()
    {
        // Result ID 10 is not in finiteProtectedIds.
        var recipe0 = Recipe(0, 10, new[] { 36 }, new[] { 1 });
        var recipe1 = Recipe(1, 10, new[] { 37 }, new[] { 2 });

        var result = RecommendedLogicEngine.EnforceSynthesisReachability(
            new[] { recipe0, recipe1 }, ShopFieldIds, new Dictionary<int, int>(),
            new Dictionary<int, ItemObtainabilityEntry>(),
            FiniteProtectedIds, new Random(1));

        Assert.Equal(2, result.Count);  // both preserved
    }

    // ── Rule 8A + 8B combined ─────────────────────────────────────────────────

    [Fact]
    public void RuleAandB_Combined_BothRulesApplied()
    {
        // Recipe 0: shops [32, 36], ingredient reachable only for shop 36.
        //           Result = 10 (not protected). → shop 32 removed, shop 36 kept.
        // Recipe 1: shops [36], result = 221 (Ribbon, vanilla count 1).
        // Recipe 2: shops [37], result = 221 → second Ribbon recipe → dropped.
        var catalog = Catalog(
            new ItemObtainabilityEntry
            { ItemId = 1, IsFieldItem = true, FieldInstanceCount = 1 },
            new ItemObtainabilityEntry
            { ItemId = 221, BossInstanceCount = 1 });
        var fieldIds = new Dictionary<int, int> { { 1, 1000 } };

        var recipe0 = Recipe(0, 10, new[] { 32, 36 }, new[] { 1 });
        var recipe1 = Recipe(1, 221, new[] { 36 }, new[] { 2 });
        var recipe2 = Recipe(2, 221, new[] { 37 }, new[] { 2 });

        var result = RecommendedLogicEngine.EnforceSynthesisReachability(
            new[] { recipe0, recipe1, recipe2 }, ShopFieldIds, fieldIds,
            catalog, FiniteProtectedIds, new Random(1));

        Assert.Equal(2, result.Count);

        var r0 = result.First(r => r.Id == 0);
        Assert.DoesNotContain(32, r0.Shops);
        Assert.Contains(36, r0.Shops);

        Assert.Contains(result, r => r.Id == 1);   // recipe1 kept
        Assert.DoesNotContain(result, r => r.Id == 2); // recipe2 dropped
    }

    // ── Guard conditions ──────────────────────────────────────────────────────

    [Fact]
    public void NullSynthesisRows_Throws()
        => Assert.Throws<ArgumentNullException>(() =>
               RecommendedLogicEngine.EnforceSynthesisReachability(
                   null!, ShopFieldIds, new Dictionary<int, int>(),
                   new Dictionary<int, ItemObtainabilityEntry>(),
                   FiniteProtectedIds, new Random(1)));

    [Fact]
    public void NullShopFieldIds_Throws()
        => Assert.Throws<ArgumentNullException>(() =>
               RecommendedLogicEngine.EnforceSynthesisReachability(
                   Array.Empty<SynthesisRow>(), null!, new Dictionary<int, int>(),
                   new Dictionary<int, ItemObtainabilityEntry>(),
                   FiniteProtectedIds, new Random(1)));

    [Fact]
    public void NullCatalog_Throws()
        => Assert.Throws<ArgumentNullException>(() =>
               RecommendedLogicEngine.EnforceSynthesisReachability(
                   Array.Empty<SynthesisRow>(), ShopFieldIds, new Dictionary<int, int>(),
                   null!, FiniteProtectedIds, new Random(1)));
}

/// <summary>
/// Integration tests running all three RecommendedLogicEngine correction methods
/// in pipeline sequence with a single synthetic scenario that has a deliberate
/// mismatch in each layer.
///
/// Scenario:
///   Equipment: Vivi (Magical) given Steiner's sword set → coherence correction swaps
///   Shops:     Ribbon (221) planted in shop 0 → legendary rarity removes it
///   Synthesis: Ribbon recipe in shop 32 with field-only ingredient unreachable → shop dropped
/// </summary>
public class RecommendedLogicEngineIntegrationTests
{
    // ── Fixtures ───────────────────────────────────────────────────────────────

    private static List<DefaultEquipmentRow> EquipmentSets() => new()
    {
        new() { Id = 0, Weapon = 1  },   // Balanced (Dagger)
        new() { Id = 1, Weapon = 70 },   // Magical  (Mage Staff)
        new() { Id = 2, Weapon = 57 },   // Magical  (Rod)
        new() { Id = 3, Weapon = 16 },   // Physical (Broadsword)
        new() { Id = 4, Weapon = 31 },   // Physical (Javelin)
        new() { Id = 5, Weapon = 79 },   // Balanced (Fork)
        new() { Id = 6, Weapon = 64 },   // Magical  (Golem Flute)
        new() { Id = 7, Weapon = 41 },   // Physical (Cat's Claws)
    };

    private static List<CharacterParametersRow> CharParams()
    {
        var rows = Enumerable.Range(0, 12).Select(i => new CharacterParametersRow
        {
            Id = i,
            DefaultRow = true,
            DefaultWinPose = true,
            DefaultCategory = 5,
            DefaultCommandSet = i,
            DefaultEquipmentSet = i,
            BattleParameterFormula = i.ToString(),
            NameKeyword = $"CH{i:D2}",
        }).ToList();
        // Classic mismatch: Vivi (id=1, Magical) gets set 3 (Physical sword)
        rows.First(r => r.Id == 1).DefaultEquipmentSet = 3;
        rows.First(r => r.Id == 3).DefaultEquipmentSet = 1;
        return rows;
    }

    private static Dictionary<int, IReadOnlyList<string>> SlotAssignment() => new()
    {
        { 0, new[] { "Steal",     "Skill"     } },
        { 1, new[] { "Blk Mag",   "Focus"     } },
        { 2, new[] { "Summon-A",  "Wht Mag-A" } },
        { 3, new[] { "Swd Art",   "Swd Mag"   } },
        { 4, new[] { "Jump",      "Dragon"    } },
        { 5, new[] { "Eat",       "Blu Mag"   } },
        { 6, new[] { "Wht Mag-B", "Summon-B"  } },
        { 7, new[] { "Flair",     "Throw"     } },
    };

    // ── Integration test ──────────────────────────────────────────────────────

    [Fact]
    public void FullPipeline_AllThreeCorrectionsApplied()
    {
        var rng = new Random(42);

        // ── Step 1: Equipment coherence ───────────────────────────────────────
        var charParams = CharParams();
        var correctedParams = RecommendedLogicEngine.CorrectEquipmentCoherence(
            charParams, SlotAssignment(), EquipmentSets());

        // Vivi (Magical) must not have a Physical weapon set after correction.
        var setById = EquipmentSets().ToDictionary(s => s.Id);
        var viviSetId = correctedParams.First(r => r.Id == 1).DefaultEquipmentSet;
        var viviWeaponAffinity = RecommendedLogicEngine.ClassifyWeapon(setById[viviSetId].Weapon);
        Assert.NotEqual((int)RecommendedLogicEngine.WeaponAffinity.Physical,
                        (int)viviWeaponAffinity);

        // ── Step 2: Legendary rarity ──────────────────────────────────────────
        var shopRows = new List<ShopItemsRow>
        {
            new() { Comment = "Shop 0000", Id = 0, Items = new[] { 1, 221, 3 } },
            new() { Comment = "Shop 0001", Id = 1, Items = new[] { 4, 5, 6 } },
        };
        var replacementPool = new[] { 236, 237, 240 };

        var correctedShops = RecommendedLogicEngine.EnforceLegendaryRarity(
            shopRows,
            LegendaryItemList.AllProtectedItemIds,
            replacementPool,
            rng);

        // Ribbon must be gone from all shop rows.
        foreach (var row in correctedShops)
            Assert.DoesNotContain(221, row.Items);

        // Shop 1 had no protected items — must be original reference.
        Assert.Same(shopRows[1], correctedShops[1]);

        // ── Step 3: Synthesis reachability ────────────────────────────────────
        // Ribbon recipe in shop 32 (field 560), ingredient only available at field 800.
        var catalog = new Dictionary<int, ItemObtainabilityEntry>
        {
            { 5, new ItemObtainabilityEntry
                { ItemId = 5, IsFieldItem = true, FieldInstanceCount = 1 } },
            { 221, new ItemObtainabilityEntry
                { ItemId = 221, BossInstanceCount = 1 } },
        };
        var fieldIds = new Dictionary<int, int> { { 5, 800 } };

        var recipes = new List<SynthesisRow>
        {
            new() { Id = 0, Comment = "Ribbon",   Result = 221,
                    Shops = new[] { 32, 36 }, Price = 50000,
                    Ingredients = new[] { 5 } },  // ingredient reachable for 36 (2453) but not 32 (560)
            new() { Id = 1, Comment = "Butterfly", Result = 7,
                    Shops = new[] { 33 }, Price = 700,
                    Ingredients = new[] { 1 } },  // ingredient 1 not in catalog → reachable
        };

        var correctedRecipes = RecommendedLogicEngine.EnforceSynthesisReachability(
            recipes,
            SynthesisShopData.ShopFieldIds,
            fieldIds,
            catalog,
            new HashSet<int> { 221 },
            rng);

        // Ribbon recipe: shop 32 removed (ingredient field 800 >= 560), shop 36 kept.
        var ribbonRecipe = correctedRecipes.First(r => r.Result == 221);
        Assert.DoesNotContain(32, ribbonRecipe.Shops);
        Assert.Contains(36, ribbonRecipe.Shops);

        // Butterfly Sword recipe unchanged.
        var butterfly = correctedRecipes.First(r => r.Result == 7);
        Assert.Contains(33, butterfly.Shops);

        // ── All three corrections applied — total result is coherent ──────────
        Assert.Equal(2, correctedRecipes.Count);
        Assert.Equal(2, correctedShops.Count);
        Assert.Equal(12, correctedParams.Count);
    }

    [Fact]
    public void FullPipeline_Deterministic_SameSeedProducesSameOutput()
    {
        // Runs the legendary rarity step (the only RNG-consuming step) twice
        // with the same seed and verifies identical shop output.
        var shopRows = new List<ShopItemsRow>
        {
            new() { Comment = "Shop 0000", Id = 0, Items = new[] { 221, 250, 1 } },
        };
        var replacementPool = new[] { 236, 237, 240 };
        var protectedIds = LegendaryItemList.AllProtectedItemIds;

        var result1 = RecommendedLogicEngine.EnforceLegendaryRarity(
            shopRows, protectedIds, replacementPool, new Random(99));
        var result2 = RecommendedLogicEngine.EnforceLegendaryRarity(
            shopRows, protectedIds, replacementPool, new Random(99));

        Assert.Equal(result1[0].Items, result2[0].Items);
    }
}