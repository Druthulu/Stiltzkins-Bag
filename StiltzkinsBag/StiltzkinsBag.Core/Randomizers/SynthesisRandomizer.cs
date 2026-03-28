using StiltzkinsBag.Models;
using StiltzkinsBag.Models.Csv;

namespace StiltzkinsBag.Randomizers;

/// <summary>
/// Result returned by <see cref="SynthesisRandomizer.Randomize"/>.
/// Contains the full list of modified synthesis recipes.
/// Recipe IDs and shop assignments are never modified — only Result and/or
/// Ingredients change depending on active flags.
/// </summary>
public record SynthesisRandomizerResult(List<SynthesisRow> Recipes);

/// <summary>
/// Randomizes synthesis recipes in Synthesis.csv.
///
/// <para>
/// Three independent flags control which aspects of recipes are randomized.
/// Any combination may be enabled simultaneously:
/// <list type="bullet">
///   <item><see cref="Settings.RandomizeSynthesisResults"/> — shuffles or replaces
///   the Result item ID on each recipe.</item>
///   <item><see cref="Settings.RandomizeSynthesisIngredients"/> — Fisher-Yates shuffles
///   ingredient sets as whole arrays across all recipes.</item>
///   <item><see cref="Settings.AllowNewSynthesisResults"/> — when true alongside
///   RandomizeSynthesisResults, results are drawn from a filtered item pool rather than
///   only the vanilla synthesis result IDs.</item>
/// </list>
/// </para>
///
/// <para>
/// <b>Chaos mode overrides:</b> When <see cref="RandomizerMode.Chaos"/> is active,
/// RandomizeSynthesisResults, RandomizeSynthesisIngredients, and AllowNewSynthesisResults
/// are all treated as enabled regardless of their individual flag values. The result pool
/// in Chaos mode contains all items with no price filter.
/// </para>
///
/// <para>
/// <b>Price randomization:</b> Controlled by <see cref="SynthesisPriceMode"/>.
/// Modifies the Price column on each SynthesisRow directly — not the global Items.csv price.
/// The <see cref="Settings.BadEconomy"/> multiplier is applied on top of the chosen price
/// mode as a final pass.
/// </para>
///
/// <para>
/// <b>GearScored price mode:</b> Price is derived from the gear stat total of the result item
/// (sum of Dexterity + Strength + Magic + Will from Stats.csv via the result item's BonusId).
/// Requires both <paramref name="allItems"/> and <paramref name="statsRows"/> to be non-null;
/// recipes whose result item has BonusId = 0, or whose result item is not found, fall back to
/// vanilla price. 0 RNG calls — fully deterministic.
/// </para>
///
/// <para>
/// <b>Conservative result pool (Phase 5.6):</b> AllowNewSynthesisResults draws from Items.csv
/// filtered by Price &gt; <see cref="KeyItemPriceThreshold"/>. Full obtainability constraints
/// will be enforced by the VanillaItemCatalog introduced in Phase 5.8.
/// </para>
///
/// <para>
/// <b>Pipeline RNG call order:</b>
/// <list type="number">
///   <item>Shuffle results — vanilla shuffle: recipes.Count − 1 calls;
///   AllowNew draw: 1 call per recipe in input order.</item>
///   <item>Shuffle ingredients — Fisher-Yates of ingredient sets: recipes.Count − 1 calls.</item>
///   <item>Price mode — 0 calls (Vanilla/GearScored); 1 call per recipe (BoundedRandom/ProportionalScale).</item>
///   <item>BadEconomy multiplier — 1 call per recipe, Id order ascending.</item>
/// </list>
/// </para>
/// </summary>
public sealed class SynthesisRandomizer
{
    private readonly Random _rng;
    private readonly Settings _settings;

    /// <summary>
    /// Items with Price &lt;= this value are excluded from the AllowNewSynthesisResults pool.
    /// Matches the threshold used by ShopRandomizer and InitialItemsRandomizer.
    /// </summary>
    internal const uint KeyItemPriceThreshold = 2;

    /// <summary>
    /// Base price in gil for <see cref="SynthesisPriceMode.GearScored"/> mode.
    /// Applied to all result items regardless of stat total.
    /// </summary>
    internal const uint GearScoredBasePrice = 500;

    /// <summary>
    /// Additional gil per stat point for <see cref="SynthesisPriceMode.GearScored"/> mode.
    /// A result item with total stats of 4 (e.g. +2 Str, +2 Mag) costs
    /// GearScoredBasePrice + 4 × GearScoredPricePerStatPoint = 2500 gil.
    /// </summary>
    internal const uint GearScoredPricePerStatPoint = 500;

    public SynthesisRandomizer(Random rng, Settings settings)
    {
        _rng = rng;
        _settings = settings;
    }

    /// <summary>
    /// Randomizes synthesis recipes and returns a <see cref="SynthesisRandomizerResult"/>.
    /// Input rows are never mutated.
    ///
    /// <para>
    /// Recipes must be passed in a consistent order across calls for seed determinism.
    /// CSV Id order (ascending) is the expected input order.
    /// </para>
    /// </summary>
    /// <param name="recipes">All rows from Synthesis.csv in Id order.</param>
    /// <param name="allItems">
    /// All rows from Items.csv. Required for <see cref="Settings.AllowNewSynthesisResults"/>
    /// and <see cref="SynthesisPriceMode.GearScored"/>. If null when AllowNewSynthesisResults
    /// is active, falls back to vanilla result shuffle automatically.
    /// </param>
    /// <param name="statsRows">
    /// All rows from Stats.csv. Required only for <see cref="SynthesisPriceMode.GearScored"/>.
    /// If null, GearScored falls back to vanilla price per recipe.
    /// </param>
    public SynthesisRandomizerResult Randomize(
        List<SynthesisRow> recipes,
        List<ItemsRow>? allItems,
        List<StatsRow>? statsRows)
    {
        if (!_settings.RandomizeSynthesis)
            return new SynthesisRandomizerResult(recipes.Select(CloneRow).ToList());

        bool isChaos = _settings.Mode == RandomizerMode.Chaos;

        // Chaos mode forces all three flags on.
        bool effectiveRandomizeResults = _settings.RandomizeSynthesisResults || isChaos;
        bool effectiveRandomizeIngredients = _settings.RandomizeSynthesisIngredients || isChaos;
        bool effectiveAllowNew = _settings.AllowNewSynthesisResults || isChaos;

        // Start from clones — never mutate input.
        var working = recipes.Select(CloneRow).ToList();

        // Step 1: Shuffle results.
        // RNG calls: (recipes.Count − 1) for vanilla shuffle;
        //            1 per recipe for AllowNew draw.
        if (effectiveRandomizeResults)
            ShuffleResults(working, allItems, effectiveAllowNew, isChaos);

        // Step 2: Shuffle ingredient sets.
        // RNG calls: recipes.Count − 1 (Fisher-Yates of ingredient array list).
        if (effectiveRandomizeIngredients)
            ShuffleIngredients(working);

        // Step 3: Randomize prices by mode.
        // RNG calls: 0 (Vanilla/GearScored); 1 per recipe (BoundedRandom/ProportionalScale).
        ApplyPriceMode(working, allItems, statsRows);

        // Step 4: BadEconomy multiplier.
        // RNG calls: 1 per recipe, Id order ascending.
        if (_settings.BadEconomy)
            ApplyBadEconomy(working);

        return new SynthesisRandomizerResult(working);
    }

    // -------------------------------------------------------------------------
    // Step 1 — Result shuffle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Shuffles result IDs across all recipes.
    /// AllowNew mode draws from the full eligible item pool (1 RNG call per recipe).
    /// Vanilla mode Fisher-Yates shuffles the existing result IDs (recipes.Count − 1 calls).
    /// If AllowNew is active but allItems is null, falls back to vanilla shuffle.
    /// </summary>
    private void ShuffleResults(
        List<SynthesisRow> working,
        List<ItemsRow>? allItems,
        bool allowNew,
        bool isChaos)
    {
        if (allowNew && allItems != null)
        {
            ShuffleResultsFromPool(working, allItems, isChaos);
        }
        else
        {
            // DISABLED: AllowNew pool draw — allItems was null or allowNew=false
            // Falls back to vanilla Fisher-Yates shuffle of existing result IDs.
            ShuffleVanillaResults(working);
        }
    }

    /// <summary>
    /// Draws result IDs from the eligible item pool without replacement.
    /// If the pool is exhausted before all recipes are assigned, cycles back through
    /// a fresh copy of the pool — prevents crashes on small pools.
    /// RNG calls: 1 per recipe in input order.
    /// </summary>
    private void ShuffleResultsFromPool(
        List<SynthesisRow> working,
        List<ItemsRow> allItems,
        bool isChaos)
    {
        var pool = BuildResultPool(allItems, allowNew: true, isChaos);

        if (pool.Count == 0)
        {
            // Pool is empty — fall back to vanilla shuffle.
            // This should not happen in practice (allItems is always non-empty),
            // but is a safe defensive fallback.
            ShuffleVanillaResults(working);
            return;
        }

        var remaining = pool.ToList();
        foreach (var recipe in working)
        {
            if (remaining.Count == 0)
                remaining = pool.ToList(); // cycle if pool exhausted

            int idx = _rng.Next(remaining.Count);
            recipe.Result = remaining[idx];
            remaining.RemoveAt(idx);
        }
    }

    /// <summary>
    /// Fisher-Yates shuffle of vanilla result IDs across all recipes.
    /// No item is drawn from outside the existing result set.
    /// RNG calls: recipes.Count − 1.
    /// </summary>
    private void ShuffleVanillaResults(List<SynthesisRow> working)
    {
        var results = working.Select(r => r.Result).ToList();

        for (int i = results.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (results[i], results[j]) = (results[j], results[i]);
        }

        for (int i = 0; i < working.Count; i++)
            working[i].Result = results[i];
    }

    // -------------------------------------------------------------------------
    // Step 2 — Ingredient shuffle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Fisher-Yates shuffle of ingredient sets as whole arrays across all recipes.
    /// Recipe at position i may receive any other recipe's ingredient set.
    /// Recipe Ids, Shops, and Results are not affected by this step.
    /// RNG calls: recipes.Count − 1.
    /// </summary>
    private void ShuffleIngredients(List<SynthesisRow> working)
    {
        var ingredientSets = working.Select(r => r.Ingredients).ToList();

        for (int i = ingredientSets.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (ingredientSets[i], ingredientSets[j]) = (ingredientSets[j], ingredientSets[i]);
        }

        for (int i = 0; i < working.Count; i++)
            working[i].Ingredients = ingredientSets[i];
    }

    // -------------------------------------------------------------------------
    // Step 3 — Price mode
    // -------------------------------------------------------------------------

    /// <summary>
    /// Applies the active <see cref="SynthesisPriceMode"/> to all recipe prices.
    /// Recipes processed in working list order (Id order).
    /// </summary>
    private void ApplyPriceMode(
        List<SynthesisRow> working,
        List<ItemsRow>? allItems,
        List<StatsRow>? statsRows)
    {
        if (_settings.SynthesisPriceMode == SynthesisPriceMode.Vanilla)
            return; // 0 RNG calls

        float min, max;

        switch (_settings.SynthesisPriceMode)
        {
            case SynthesisPriceMode.BoundedRandom:
                min = Math.Min(_settings.SynthPriceMin, _settings.SynthPriceMax);
                max = Math.Max(_settings.SynthPriceMin, _settings.SynthPriceMax);
                foreach (var recipe in working)
                    recipe.Price = (uint)Math.Max(1, _rng.Next((int)min, (int)max + 1));
                break;

            case SynthesisPriceMode.ProportionalScale:
                min = Math.Min(_settings.SynthPriceScaleMinPercent, _settings.SynthPriceScaleMaxPercent);
                max = Math.Max(_settings.SynthPriceScaleMinPercent, _settings.SynthPriceScaleMaxPercent);
                foreach (var recipe in working)
                {
                    float scale = (min + (float)(_rng.NextDouble() * (max - min))) / 100f;
                    recipe.Price = (uint)Math.Max(1, (int)Math.Round(recipe.Price * scale));
                }
                break;

            case SynthesisPriceMode.GearScored:
                // 0 RNG calls — fully deterministic.
                foreach (var recipe in working)
                    recipe.Price = ComputeGearScoredPrice(recipe, allItems, statsRows);
                break;
        }
    }

    // -------------------------------------------------------------------------
    // Step 4 — BadEconomy multiplier
    // -------------------------------------------------------------------------

    /// <summary>
    /// Applies the BadEconomy price multiplier to all recipe prices.
    /// Multiplier drawn per recipe from
    /// [BadEconomyPriceMultiplierMin, BadEconomyPriceMultiplierMax].
    /// Recipes processed in Id order ascending for determinism.
    /// RNG calls: 1 per recipe.
    /// </summary>
    private void ApplyBadEconomy(List<SynthesisRow> working)
    {
        float min = Math.Min(
            _settings.BadEconomyPriceMultiplierMin,
            _settings.BadEconomyPriceMultiplierMax);
        float max = Math.Max(
            _settings.BadEconomyPriceMultiplierMin,
            _settings.BadEconomyPriceMultiplierMax);

        // Build overrides in Id order for determinism.
        var ordered = working.OrderBy(r => r.Id).ToList();
        var modified = new Dictionary<int, uint>();

        foreach (var recipe in ordered)
        {
            float multiplier = min + (float)(_rng.NextDouble() * (max - min));
            modified[recipe.Id] = (uint)Math.Max(1, (int)Math.Round(recipe.Price * multiplier));
        }

        foreach (var recipe in working)
            recipe.Price = modified[recipe.Id];
    }

    // -------------------------------------------------------------------------
    // Pool building
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds the eligible result item ID pool for AllowNewSynthesisResults mode.
    /// Results sorted by ID ascending for determinism.
    ///
    /// <para>
    /// Chaos mode: all items included, no price filter.
    /// Recommended + AllowNew: items with Price &gt; <see cref="KeyItemPriceThreshold"/> only.
    /// Phase 5.8 will add full obtainability constraints via VanillaItemCatalog.
    /// </para>
    ///
    /// <para>
    /// Only called when allowNew = true. Returns empty list if allItems is empty.
    /// </para>
    /// </summary>
    internal static List<int> BuildResultPool(
        List<ItemsRow> allItems,
        bool allowNew,
        bool isChaos)
    {
        if (!allowNew) return new List<int>();

        return isChaos
            ? allItems
                .Select(i => i.Id)
                .OrderBy(id => id)
                .ToList()
            : allItems
                .Where(i => i.Price > KeyItemPriceThreshold)
                .Select(i => i.Id)
                .OrderBy(id => id)
                .ToList();
    }

    // -------------------------------------------------------------------------
    // GearScored price calculation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Computes the GearScored synthesis price for a recipe.
    /// Price = <see cref="GearScoredBasePrice"/> + statSum × <see cref="GearScoredPricePerStatPoint"/>
    /// where statSum = Dexterity + Strength + Magic + Will from the result item's Stats.csv row.
    ///
    /// <para>
    /// Falls back to the recipe's current Price (vanilla or already-randomized) when:
    /// <list type="bullet">
    ///   <item>allItems or statsRows is null</item>
    ///   <item>The result item ID is not found in allItems</item>
    ///   <item>The result item has BonusId = 0 (no gear stat bonus row)</item>
    ///   <item>The BonusId does not match any row in statsRows</item>
    /// </list>
    /// </para>
    ///
    /// 0 RNG calls — fully deterministic.
    /// </summary>
    internal static uint ComputeGearScoredPrice(
        SynthesisRow recipe,
        List<ItemsRow>? allItems,
        List<StatsRow>? statsRows)
    {
        if (allItems == null || statsRows == null)
            return recipe.Price;

        var resultItem = allItems.FirstOrDefault(i => i.Id == recipe.Result);
        if (resultItem == null) return recipe.Price;

        if (resultItem.BonusId == 0) return recipe.Price;

        var statsRow = statsRows.FirstOrDefault(s => s.Id == resultItem.BonusId);
        if (statsRow == null) return recipe.Price;

        // StatsRow stat columns are int (values 0–5 in vanilla).
        uint statSum = (uint)(statsRow.Dexterity + statsRow.Strength + statsRow.Magic + statsRow.Will);
        uint price = GearScoredBasePrice + statSum * GearScoredPricePerStatPoint;
        return Math.Max(1u, price);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static SynthesisRow CloneRow(SynthesisRow source) =>
        new()
        {
            Comment = source.Comment,
            Id = source.Id,
            Shops = source.Shops.ToArray(),
            Price = source.Price,
            Result = source.Result,
            Ingredients = source.Ingredients.ToArray()
        };
}