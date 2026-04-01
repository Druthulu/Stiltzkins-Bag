// StiltzkinsBag.Core/Models/ItemPool.cs
//
// Pre-built, sorted item ID pools derived from VanillaItemCatalog.
// Consumed by all randomizers that need an eligible item list.
//
// Two top-level modes:
//
//   Recommended — Pools constrained to items that are logically safe to randomize:
//     - No key items (Price ≤ 2 in Items.csv), no gems, only obtainable items.
//     - Further refined by context (shop-appropriate, field-appropriate, etc.)
//     - Phase 6 RecommendedLogicEngine adds cap enforcement on top of these pools.
//
//   Chaos — Broader pools; includes gems and items that are technically obtainable
//     even through single finite sources. Key items are still excluded (they break
//     game progress). Gems are included in Chaos because they can be synthesis inputs.
//
// Pool collections (both modes):
//
//   AllEligible   — Every non-key, obtainable item in the catalog. The "full" pool.
//   Consumables   — Usable items only (IDs 236–255 range, per Items.csv Usable flag).
//   Equipment     — Equipment-type items (Weapon/Armlet/Helmet/Armor/Accessory flags).
//   ShopFriendly  — Items appropriate for shop placement: must have at least one
//                   infinite source OR be from a repeatable context. In Recommended
//                   mode this is IsInShop already — shop-friendly = infinite-source items.
//                   In Chaos mode this is all non-key, non-gem obtainable items.
//   Gems          — Gem-type items (always the same 12 IDs regardless of mode;
//                   only populated in Chaos mode — Recommended excludes gems).
//   FiniteOnly    — Items whose ONLY sources are finite (boss drops, chocographs,
//                   auction one-time, field chests). Useful for Phase 6 cap tracking.
//
// All pool lists are sorted ascending by item ID for deterministic RNG behavior.
// Pools are immutable after construction.

using System;
using System.Collections.Generic;
using System.Linq;

namespace StiltzkinsBag.Core.Models;

/// <summary>
/// Randomizer mode selector — determines how aggressively the item pools are populated.
/// </summary>
public enum ItemPoolMode
{
    /// <summary>
    /// Conservative, logic-safe pools. Excludes gems, key items, and unobtainable items.
    /// Intended for use with Phase 6 RecommendedLogicEngine.
    /// </summary>
    Recommended,

    /// <summary>
    /// Broad pools. Key items are still excluded (they break progression), but gems
    /// and finite-only rare items are included. Intended for full-chaos randomization.
    /// </summary>
    Chaos,
}

/// <summary>
/// Pre-built item ID pools for each randomizer context, derived from
/// <see cref="VanillaItemCatalog"/>. Immutable after construction.
/// </summary>
public sealed class ItemPool
{
    // ── Pool collections ──────────────────────────────────────────────────────

    /// <summary>
    /// Every non-key, obtainable item in the catalog. The broadest eligible pool.
    /// In Recommended mode: excludes gems. In Chaos mode: includes gems.
    /// Key items (IsKeyItem = true) are always excluded from all pools.
    /// </summary>
    public IReadOnlyList<int> AllEligible { get; }

    /// <summary>
    /// Usable/consumable items only (Usable flag set in Items.csv).
    /// The same in both modes — consumables are never key items.
    /// </summary>
    public IReadOnlyList<int> Consumables { get; }

    /// <summary>
    /// Equipment-type items (Weapon/Armlet/Helmet/Armor/Accessory flags).
    /// In Recommended mode: only equipment with at least one obtainable source.
    /// In Chaos mode: all non-key equipment that is obtainable.
    /// </summary>
    public IReadOnlyList<int> Equipment { get; }

    /// <summary>
    /// Items suitable for placement in shops — items with at least one infinite source.
    /// In Recommended mode: items where IsInfinite = true (shop, normal enemy, repeatable auction,
    /// or infinite-ingredient synthesis).
    /// In Chaos mode: same as AllEligible (all obtainable non-key items).
    /// </summary>
    public IReadOnlyList<int> ShopFriendly { get; }

    /// <summary>
    /// Gem-type items (Gem flag set in Items.csv). IDs 224–235 in vanilla.
    /// Empty in Recommended mode (gems are not directly equippable rewards).
    /// Populated in Chaos mode.
    /// </summary>
    public IReadOnlyList<int> Gems { get; }

    /// <summary>
    /// Items whose ONLY obtainable sources are finite (boss drops, chocographs,
    /// one-time auction, field chests, friendly monsters, Ragtime Mouse).
    /// Used by Phase 6 RecommendedLogicEngine to enforce copy-count caps.
    /// </summary>
    public IReadOnlyList<int> FiniteOnly { get; }

    // ── Metadata ──────────────────────────────────────────────────────────────

    /// <summary>The mode this pool was built for.</summary>
    public ItemPoolMode Mode { get; }

    /// <summary>The catalog this pool was derived from.</summary>
    public VanillaItemCatalog Catalog { get; }

    // ── Constructor (private — use Build) ─────────────────────────────────────

    private ItemPool(
        ItemPoolMode mode,
        VanillaItemCatalog catalog,
        IReadOnlyList<int> allEligible,
        IReadOnlyList<int> consumables,
        IReadOnlyList<int> equipment,
        IReadOnlyList<int> shopFriendly,
        IReadOnlyList<int> gems,
        IReadOnlyList<int> finiteOnly)
    {
        Mode = mode;
        Catalog = catalog;
        AllEligible = allEligible;
        Consumables = consumables;
        Equipment = equipment;
        ShopFriendly = shopFriendly;
        Gems = gems;
        FiniteOnly = finiteOnly;
    }

    // ── Factory ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds an <see cref="ItemPool"/> from a <see cref="VanillaItemCatalog"/> in the
    /// specified mode. All pools are sorted ascending by item ID.
    /// </summary>
    /// <param name="catalog">Fully-built catalog (call <see cref="VanillaItemCatalog.Build"/> first).</param>
    /// <param name="mode">Pool mode — Recommended (conservative) or Chaos (broad).</param>
    /// <exception cref="ArgumentNullException"><paramref name="catalog"/> is null.</exception>
    public static ItemPool Build(VanillaItemCatalog catalog, ItemPoolMode mode)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var allEligible   = new List<int>();
        var consumables   = new List<int>();
        var equipment     = new List<int>();
        var shopFriendly  = new List<int>();
        var gems          = new List<int>();
        var finiteOnly    = new List<int>();

        bool isRecommended = mode == ItemPoolMode.Recommended;

        foreach (var (id, entry) in catalog.Entries.OrderBy(kv => kv.Key))
        {
            // Key items are always excluded
            if (entry.IsKeyItem) continue;

            // Must be obtainable through at least one source
            if (!entry.IsObtainable) continue;

            // Recommended mode excludes gems (they're synthesis inputs, not equippable rewards)
            bool includeGem = !isRecommended;
            if (entry.IsGem && !includeGem) continue;

            // ── AllEligible ──
            allEligible.Add(id);

            // ── Consumables ──
            if (entry.IsConsumable)
                consumables.Add(id);

            // ── Equipment ──
            if (entry.IsEquipment)
                equipment.Add(id);

            // ── Gems ──
            if (entry.IsGem)
                gems.Add(id);

            // ── ShopFriendly ──
            bool shopFriendlyItem = isRecommended
                ? catalog.IsInfinite(id)          // Recommended: must be infinite
                : true;                            // Chaos: all eligible items

            if (shopFriendlyItem)
                shopFriendly.Add(id);

            // ── FiniteOnly ──
            if (entry.IsFiniteOnly)
                finiteOnly.Add(id);
        }

        return new ItemPool(
            mode, catalog,
            allEligible.AsReadOnly(),
            consumables.AsReadOnly(),
            equipment.AsReadOnly(),
            shopFriendly.AsReadOnly(),
            gems.AsReadOnly(),
            finiteOnly.AsReadOnly());
    }

    // ── Convenience ───────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a copy of <paramref name="basePool"/> filtered to the given item IDs.
    /// Useful when a randomizer needs a subset of a standard pool.
    /// Preserves sort order.
    /// </summary>
    public static IReadOnlyList<int> FilterPool(IReadOnlyList<int> basePool, ISet<int> allowed)
    {
        ArgumentNullException.ThrowIfNull(basePool);
        ArgumentNullException.ThrowIfNull(allowed);
        return basePool.Where(allowed.Contains).ToList().AsReadOnly();
    }
}
