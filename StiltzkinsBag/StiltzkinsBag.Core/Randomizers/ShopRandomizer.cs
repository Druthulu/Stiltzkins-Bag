using StiltzkinsBag.Models;
using StiltzkinsBag.Models.Csv;

namespace StiltzkinsBag.Randomizers;

/// <summary>
/// Result returned by <see cref="ShopRandomizer.Randomize"/>.
/// Always contains the full modified shop list. When <see cref="Settings.BadEconomy"/>
/// is enabled, <see cref="Items"/> is a new list with modified Price values; otherwise
/// it is the same list reference that was passed in.
/// </summary>
public record ShopRandomizerResult(
    List<ShopItemsRow> Shops,
    List<ItemsRow> Items);

/// <summary>
/// Randomizes shop contents in ShopItems.csv and optionally modifies global item prices
/// in Items.csv via the <see cref="Settings.BadEconomy"/> challenge modifier.
///
/// <para>
/// Three population modes are supported:
/// <list type="bullet">
///   <item><see cref="ShopMode.Shuffle"/> — Fisher-Yates shuffle of all vanilla item slots
///   globally, redistributed without within-shop duplicates.</item>
///   <item><see cref="ShopMode.BoundedRandom"/> — each shop independently draws from
///   an eligible item pool filtered by <see cref="ShopItemPool"/>.</item>
///   <item><see cref="ShopMode.MegaMart"/> — all shops sell the same list; debug-gated,
///   downgrades to Shuffle when <see cref="Settings.IsDebugMode"/> is false.</item>
/// </list>
/// </para>
///
/// <para>
/// Shop sizes are governed by <see cref="ShopSizeMode"/> (Maintain / Random / Fixed)
/// and may be further capped by the <see cref="Settings.ShortSupply"/> challenge modifier.
/// </para>
///
/// <para>
/// <b>BadEconomy price pass:</b> When <see cref="Settings.BadEconomy"/> is enabled,
/// a global price multiplier is applied to Items.csv after all shop population.
/// Each item with Price &gt; <see cref="KeyItemPriceThreshold"/> receives an independent
/// multiplier drawn from
/// [<see cref="Settings.BadEconomyPriceMultiplierMin"/>, <see cref="Settings.BadEconomyPriceMultiplierMax"/>],
/// rounded to the nearest 1 gil, clamped to a minimum of 1.
/// This affects buy prices and sell prices everywhere — shops, synthesis display prices,
/// and player sell values — because price is a global property of each item in Items.csv.
/// Key items (Price &lt;= <see cref="KeyItemPriceThreshold"/>) are never touched.
/// Items processed in Id order ascending for determinism. 1 RNG call per eligible item.
/// </para>
///
/// <para>
/// <b>Conservative item pool (Phase 5.6):</b> BoundedRandom and MegaMart draw from
/// Items.csv filtered by Price and ID range. Full obtainability constraints
/// (unique item caps, boss-drop awareness) will be enforced by the VanillaItemCatalog
/// introduced in Phase 5.8. Until then, the pool excludes only key/story items
/// (Price &lt;= 2) — a conservative but safe baseline.
/// </para>
/// </summary>
public sealed class ShopRandomizer
{
    private readonly Random _rng;
    private readonly Settings _settings;

    /// <summary>
    /// Item ID for Potion — used by the EnsureMedicItems post-pass.
    /// Must match the hardcoded ID used in InitialItemsRandomizer (SpeedRunner kit).
    /// Verify against Items.csv if EnsureMedicItems tests fail.
    /// </summary>
    internal const int PotionItemId = 236;

    /// <summary>
    /// Item ID for Phoenix Down — used by the EnsureMedicItems post-pass.
    /// Must match the hardcoded ID used in InitialItemsRandomizer (SpeedRunner kit).
    /// Verify against Items.csv if EnsureMedicItems tests fail.
    /// </summary>
    internal const int PhoenixDownItemId = 240;

    /// <summary>
    /// Items with Price &lt;= this value are treated as story-only or key items
    /// and are always excluded from randomized shop pools and the BadEconomy price pass.
    /// Matches the threshold used by InitialItemsRandomizer.GearRandom.
    /// </summary>
    internal const uint KeyItemPriceThreshold = 2;

    /// <summary>
    /// Minimum item ID for the ConsumablesOnly pool.
    /// Consumable items (Potions, Ethers, Phoenix Downs, etc.) occupy IDs 236–253.
    /// Matches the range used by InitialItemsRandomizer.ConsumablesRandom.
    /// </summary>
    internal const int ConsumableIdMin = 236;

    /// <summary>Maximum item ID for the ConsumablesOnly pool.</summary>
    internal const int ConsumableIdMax = 253;

    public ShopRandomizer(Random rng, Settings settings)
    {
        _rng = rng;
        _settings = settings;
    }

    /// <summary>
    /// Randomizes shop contents and optionally modifies global item prices.
    /// Returns a <see cref="ShopRandomizerResult"/> containing the modified shop list
    /// and the (possibly modified) items list.
    ///
    /// <para>
    /// Pipeline order (RNG calls in this exact sequence):
    /// <list type="number">
    ///   <item>ComputeTargetSizes — 0 or 1 RNG call per non-empty shop (Random mode only)</item>
    ///   <item>ShortSupply cap — 0 RNG calls (deterministic)</item>
    ///   <item>Shop population (Shuffle / BoundedRandom / MegaMart) — mode-dependent RNG calls</item>
    ///   <item>EnsureMedicItems post-pass — 0 RNG calls (deterministic)</item>
    ///   <item>ApplyBadEconomy — 1 RNG call per eligible item, Id order ascending</item>
    /// </list>
    /// </para>
    ///
    /// Input rows are never mutated. Empty shops (Items.Length == 0) are always
    /// preserved as empty regardless of mode or settings.
    /// </summary>
    /// <param name="shops">
    /// All rows from ShopItems.csv, in ID order.
    /// Order must be consistent across calls for seed determinism.
    /// </param>
    /// <param name="allItems">
    /// All rows from Items.csv. Required for <see cref="ShopMode.BoundedRandom"/>,
    /// <see cref="ShopMode.MegaMart"/>, and <see cref="Settings.BadEconomy"/> pool
    /// filtering. If null and a pool-dependent mode is active, falls back to
    /// <see cref="ShopMode.Shuffle"/> automatically. BadEconomy is skipped if null.
    /// </param>
    public ShopRandomizerResult Randomize(List<ShopItemsRow> shops, List<ItemsRow>? allItems)
    {
        if (!_settings.RandomizeShops)
        {
            var passthrough = shops.Select(CloneRow).ToList();
            var passthroughItems = allItems ?? new List<ItemsRow>();
            return new ShopRandomizerResult(passthrough, passthroughItems);
        }

        // Resolve effective mode.
        // MegaMart downgrades to Shuffle outside debug mode.
        // BoundedRandom and MegaMart fall back to Shuffle if allItems unavailable.
        var effectiveMode = _settings.ShopMode;
        if (effectiveMode == ShopMode.MegaMart && !_settings.IsDebugMode)
            effectiveMode = ShopMode.Shuffle;
        if (allItems == null && effectiveMode != ShopMode.Shuffle)
            effectiveMode = ShopMode.Shuffle;

        // Step 1: Compute target sizes per shop.
        // RNG calls: 1 per non-empty shop when ShopSizeMode.Random; 0 otherwise.
        var targetSizes = ComputeTargetSizes(shops);

        // Step 2: Apply ShortSupply cap to target sizes (no RNG).
        if (_settings.ShortSupply)
        {
            int cap = Math.Max(1, _settings.ShortSupplyMaxItems);
            foreach (var key in targetSizes.Keys.ToList())
                targetSizes[key] = Math.Min(targetSizes[key], cap);
        }

        // Step 3: Populate shops (mode-dependent RNG calls).
        List<ShopItemsRow> resultShops = effectiveMode switch
        {
            ShopMode.Shuffle => PopulateShuffle(shops, targetSizes),
            ShopMode.BoundedRandom => PopulateBoundedRandom(shops, allItems!, targetSizes),
            ShopMode.MegaMart => PopulateMegaMart(shops, allItems!, targetSizes),
            _ => PopulateShuffle(shops, targetSizes)
        };

        // Step 4: EnsureMedicItems post-pass (0 RNG calls — deterministic).
        if (_settings.ShopEnsureMedicItems)
            EnsureMedicItems(resultShops);

        // Step 5: BadEconomy global price pass (1 RNG call per eligible item, Id order).
        List<ItemsRow> resultItems = allItems ?? new List<ItemsRow>();
        if (_settings.BadEconomy && allItems != null)
            resultItems = ApplyBadEconomy(allItems);

        return new ShopRandomizerResult(resultShops, resultItems);
    }

    // -------------------------------------------------------------------------
    // Target size computation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Computes the target item count for each shop based on <see cref="ShopSizeMode"/>.
    /// Empty shops (Items.Length == 0) always receive target size 0 and are never touched.
    /// RNG calls: 0 for Maintain and Fixed; 1 per non-empty shop for Random.
    /// </summary>
    internal Dictionary<int, int> ComputeTargetSizes(List<ShopItemsRow> shops)
    {
        var nonEmpty = shops.Where(s => s.Items.Length > 0).ToList();
        int vanillaMin = nonEmpty.Count > 0 ? nonEmpty.Min(s => s.Items.Length) : 1;
        int vanillaMax = nonEmpty.Count > 0 ? nonEmpty.Max(s => s.Items.Length) : 1;

        var sizes = new Dictionary<int, int>();
        foreach (var shop in shops)
        {
            if (shop.Items.Length == 0)
            {
                sizes[shop.Id] = 0;
                continue;
            }

            sizes[shop.Id] = _settings.ShopSizeMode switch
            {
                ShopSizeMode.Maintain => shop.Items.Length,
                ShopSizeMode.Random => _rng.Next(vanillaMin, vanillaMax + 1),
                ShopSizeMode.Fixed => Math.Max(1, _settings.ShopFixedSize),
                _ => shop.Items.Length
            };
        }
        return sizes;
    }

    // -------------------------------------------------------------------------
    // Shop population modes
    // -------------------------------------------------------------------------

    /// <summary>
    /// Shuffle mode: Fisher-Yates shuffle of all vanilla item slots globally,
    /// then distribute into shops avoiding within-shop duplicates.
    /// Each pool slot is consumed at most once. Shops processed in input order.
    /// If the pool is exhausted before a shop's target is reached, the shop
    /// receives fewer items — no crash.
    /// RNG calls: globalPool.Count − 1 (Fisher-Yates) + 0 (distribution is deterministic).
    /// </summary>
    private List<ShopItemsRow> PopulateShuffle(
        List<ShopItemsRow> shops,
        Dictionary<int, int> targetSizes)
    {
        var globalPool = shops
            .Where(s => s.Items.Length > 0)
            .SelectMany(s => s.Items)
            .ToList();

        for (int i = globalPool.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (globalPool[i], globalPool[j]) = (globalPool[j], globalPool[i]);
        }

        var usedPositions = new HashSet<int>();
        var result = new List<ShopItemsRow>();

        foreach (var shop in shops)
        {
            int target = targetSizes[shop.Id];
            if (target == 0)
            {
                result.Add(CloneRow(shop, Array.Empty<int>()));
                continue;
            }

            var assigned = new List<int>(target);
            var usedInShop = new HashSet<int>();

            for (int i = 0; i < globalPool.Count && assigned.Count < target; i++)
            {
                if (usedPositions.Contains(i)) continue;
                if (!usedInShop.Add(globalPool[i])) continue;
                assigned.Add(globalPool[i]);
                usedPositions.Add(i);
            }

            result.Add(CloneRow(shop, assigned.ToArray()));
        }

        return result;
    }

    /// <summary>
    /// BoundedRandom mode: each shop independently draws items from the eligible pool
    /// without replacement. No within-shop duplicates. One shop's draws do not affect
    /// any other shop's pool.
    /// RNG calls: 1 per item slot drawn, across all shops in input order.
    /// </summary>
    private List<ShopItemsRow> PopulateBoundedRandom(
        List<ShopItemsRow> shops,
        List<ItemsRow> allItems,
        Dictionary<int, int> targetSizes)
    {
        var eligiblePool = BuildEligiblePool(allItems, _settings.ShopItemPool);

        var result = new List<ShopItemsRow>();
        foreach (var shop in shops)
        {
            int target = targetSizes[shop.Id];
            if (target == 0 || eligiblePool.Count == 0)
            {
                result.Add(CloneRow(shop, Array.Empty<int>()));
                continue;
            }

            var remaining = eligiblePool.ToList();
            var assigned = new List<int>(target);
            int drawCount = Math.Min(target, remaining.Count);

            for (int i = 0; i < drawCount; i++)
            {
                int idx = _rng.Next(remaining.Count);
                assigned.Add(remaining[idx]);
                remaining.RemoveAt(idx);
            }

            result.Add(CloneRow(shop, assigned.ToArray()));
        }

        return result;
    }

    /// <summary>
    /// MegaMart mode (debug only): all shops sell the same broad item list.
    /// A single Fisher-Yates shuffle determines master item order; each shop
    /// receives the first N items from that list.
    /// Always uses <see cref="ShopItemPool.AllNonKeyItems"/> regardless of settings.
    /// Downgrades to Shuffle when IsDebugMode = false — handled in Randomize().
    /// RNG calls: eligiblePool.Count − 1 (one Fisher-Yates shuffle).
    /// </summary>
    private List<ShopItemsRow> PopulateMegaMart(
        List<ShopItemsRow> shops,
        List<ItemsRow> allItems,
        Dictionary<int, int> targetSizes)
    {
        var eligiblePool = BuildEligiblePool(allItems, ShopItemPool.AllNonKeyItems);

        for (int i = eligiblePool.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (eligiblePool[i], eligiblePool[j]) = (eligiblePool[j], eligiblePool[i]);
        }

        var result = new List<ShopItemsRow>();
        foreach (var shop in shops)
        {
            int target = targetSizes[shop.Id];
            if (target == 0)
            {
                result.Add(CloneRow(shop, Array.Empty<int>()));
                continue;
            }

            int take = Math.Min(target, eligiblePool.Count);
            result.Add(CloneRow(shop, eligiblePool.Take(take).ToArray()));
        }

        return result;
    }

    // -------------------------------------------------------------------------
    // Post-passes
    // -------------------------------------------------------------------------

    /// <summary>
    /// Ensures at least <see cref="Settings.ShopMedicMinShops"/> shops carry both
    /// a Potion and a Phoenix Down. Applied after all shop population.
    /// Deterministic — 0 RNG calls.
    /// Medic items are appended to the shop's item list; no items are removed or replaced.
    /// Candidate shops chosen by descending item count, then ascending Id (tie-break).
    /// </summary>
    internal void EnsureMedicItems(List<ShopItemsRow> shops)
    {
        int required = Math.Max(0, _settings.ShopMedicMinShops);
        if (required == 0) return;

        int compliant = shops.Count(s =>
            s.Items.Contains(PotionItemId) && s.Items.Contains(PhoenixDownItemId));

        if (compliant >= required) return;

        var candidates = shops
            .Where(s => s.Items.Length > 0 &&
                        (!s.Items.Contains(PotionItemId) || !s.Items.Contains(PhoenixDownItemId)))
            .OrderByDescending(s => s.Items.Length)
            .ThenBy(s => s.Id)
            .ToList();

        int toFix = required - compliant;
        foreach (var shop in candidates)
        {
            if (toFix <= 0) break;
            var items = shop.Items.ToList();
            AppendIfMissing(items, PotionItemId);
            AppendIfMissing(items, PhoenixDownItemId);
            shop.Items = items.ToArray();
            toFix--;
        }
    }

    /// <summary>
    /// Applies the BadEconomy global price multiplier to all eligible items.
    /// Returns a new list of cloned ItemsRow objects with modified Price values.
    /// Input list is never mutated.
    /// Items processed in Id order ascending for determinism.
    /// RNG calls: 1 per item with Price &gt; <see cref="KeyItemPriceThreshold"/>.
    /// </summary>
    internal List<ItemsRow> ApplyBadEconomy(List<ItemsRow> allItems)
    {
        float min = Math.Min(
            _settings.BadEconomyPriceMultiplierMin,
            _settings.BadEconomyPriceMultiplierMax);
        float max = Math.Max(
            _settings.BadEconomyPriceMultiplierMin,
            _settings.BadEconomyPriceMultiplierMax);

        // Build price overrides in Id order ascending for determinism.
        var ordered = allItems.OrderBy(i => i.Id).ToList();
        var modified = new Dictionary<int, uint>(); // itemId → new price

        foreach (var item in ordered)
        {
            if (item.Price <= KeyItemPriceThreshold) continue;
            float multiplier = min + (float)(_rng.NextDouble() * (max - min));
            uint newPrice = (uint)Math.Max(1, (int)Math.Round(item.Price * multiplier));
            modified[item.Id] = newPrice;
        }

        // Return new list preserving original order, with modified prices applied.
        return allItems.Select(item =>
        {
            var clone = CloneItemRow(item);
            if (modified.TryGetValue(item.Id, out uint newPrice))
                clone.Price = newPrice;
            return clone;
        }).ToList();
    }

    // -------------------------------------------------------------------------
    // Pool building
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds the eligible item ID pool for BoundedRandom and MegaMart modes.
    /// Results sorted by ID ascending for determinism.
    /// Items with Price &lt;= <see cref="KeyItemPriceThreshold"/> always excluded.
    /// Phase 5.8 will add full obtainability constraints via VanillaItemCatalog.
    /// </summary>
    internal static List<int> BuildEligiblePool(List<ItemsRow> allItems, ShopItemPool poolType)
    {
        return poolType switch
        {
            ShopItemPool.ConsumablesOnly =>
                allItems
                    .Where(i => i.Price > KeyItemPriceThreshold &&
                                i.Id >= ConsumableIdMin && i.Id <= ConsumableIdMax)
                    .Select(i => i.Id)
                    .OrderBy(id => id)
                    .ToList(),

            ShopItemPool.AllNonKeyItems =>
                allItems
                    .Where(i => i.Price > KeyItemPriceThreshold)
                    .Select(i => i.Id)
                    .OrderBy(id => id)
                    .ToList(),

            _ =>
                allItems
                    .Where(i => i.Price > KeyItemPriceThreshold)
                    .Select(i => i.Id)
                    .OrderBy(id => id)
                    .ToList()
        };
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static void AppendIfMissing(List<int> items, int itemId)
    {
        if (!items.Contains(itemId))
            items.Add(itemId);
    }

    private static ShopItemsRow CloneRow(ShopItemsRow source) =>
        CloneRow(source, source.Items.ToArray());

    private static ShopItemsRow CloneRow(ShopItemsRow source, int[] newItems) =>
        new()
        {
            Comment = source.Comment,
            Id = source.Id,
            Items = newItems
        };

    private static ItemsRow CloneItemRow(ItemsRow source) =>
        new()
        {
            Id = source.Id,
            WeaponId = source.WeaponId,
            ArmorId = source.ArmorId,
            EffectId = source.EffectId,
            Price = source.Price,
            SellingPrice = source.SellingPrice,
            GraphicsId = source.GraphicsId,
            ColorId = source.ColorId,
            Quality = source.Quality,
            BonusId = source.BonusId,
            AbilityIds = source.AbilityIds.ToArray(),
            Weapon = source.Weapon,
            Armlet = source.Armlet,
            Helmet = source.Helmet,
            Armor = source.Armor,
            Accessory = source.Accessory,
            Item = source.Item,
            Gem = source.Gem,
            Usable = source.Usable,
            Order = source.Order,
            Zidane = source.Zidane,
            Vivi = source.Vivi,
            Garnet = source.Garnet,
            Steiner = source.Steiner,
            Freya = source.Freya,
            Quina = source.Quina,
            Eiko = source.Eiko,
            Amarant = source.Amarant,
            Cinna = source.Cinna,
            Marcus = source.Marcus,
            Blank = source.Blank,
            Beatrix = source.Beatrix
        };
}