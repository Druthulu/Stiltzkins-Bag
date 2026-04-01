// StiltzkinsBag.Core/Models/ItemObtainabilityEntry.cs
//
// Per-item classification record populated by VanillaItemCatalog.Build().
// Counts and flags encode every vanilla obtainability source for a single item ID.
// Computed properties derive pool membership and infinite/finite status.
//
// Source counts are set independently — an item can appear in multiple categories
// (e.g., Feather Boots: BossInstanceCount + WorldMapInstanceCount + AuctionCount).
// VanillaItemCatalog.ObtainabilityCounts uses these to compute the final master count:
//   int.MaxValue if ANY infinite source, else the sum of all finite source counts.

namespace StiltzkinsBag.Core.Models;

/// <summary>
/// Immutable per-item obtainability classification for a single FFIX item ID.
/// Created by <see cref="VanillaItemCatalog"/> and consumed by <see cref="ItemPool"/>
/// and (Phase 6) RecommendedLogicEngine.
/// </summary>
public sealed record ItemObtainabilityEntry
{
    // ── Identity ──────────────────────────────────────────────────────────────

    /// <summary>Item ID (0–255, matches Items.csv Id column).</summary>
    public int ItemId { get; init; }

    /// <summary>
    /// Display name parsed from the Items.csv inline comment (e.g. "# 000 - Hammer").
    /// Empty string if no inline comment was present on the row.
    /// Used in catalog CSV output, diagnostics, and UI display.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    // ── Category flags (from Items.csv) ───────────────────────────────────────

    /// <summary>
    /// Placeholder — always false for Phase 5.
    ///
    /// In FFIX, "key items" (Mist, Oglop, Cid's Hammer, etc.) live in a separate
    /// data table, not in Items.csv. All 256 Items.csv entries are regular items
    /// (weapons, armor, consumables, gems). Key item support will be wired in once
    /// that data source is parsed. Until then, IsKeyItem is always false.
    ///
    /// Items with Price = 2 in Items.csv are NOT key items — they are rare equipment
    /// or accessories that cannot be purchased (Save the Queen, Genji set, Ribbon, etc.).
    /// </summary>
    public bool IsKeyItem { get; init; }

    /// <summary>True if the Usable flag (column index 18) is set in Items.csv.</summary>
    public bool IsConsumable { get; init; }

    /// <summary>
    /// True if the Gem flag is set in Items.csv (IDs 224–235 in vanilla FFIX).
    /// Used by <see cref="ItemPool"/> to populate the Gems pool and to exclude gems
    /// from Recommended mode. Not shown in the catalog CSV export.
    /// </summary>
    public bool IsGem { get; init; }

    /// <summary>
    /// True if any equipment flag (Weapon/Armlet/Helmet/Armor/Accessory) is set in Items.csv.
    /// Used by <see cref="ItemPool"/> to populate the Equipment pool.
    /// Not shown in the catalog CSV export.
    /// </summary>
    public bool IsEquipment { get; init; }

    // ── Infinite source flags ──────────────────────────────────────────────────

    /// <summary>
    /// True if this item appears in any vanilla shop (ShopItems.csv).
    /// Shop items are infinitely obtainable. Also used as a proxy by VanillaItemCatalog
    /// to mark items whose synthesis is infinite (all ingredients infinite).
    /// </summary>
    public bool IsInShop { get; init; }

    /// <summary>
    /// True if this item is dropped or stolen from Normal-type enemies.
    /// Normal enemies are repeatable — infinite source.
    /// </summary>
    public bool HasNormalEnemySource { get; init; }

    // ── Finite source counts ───────────────────────────────────────────────────

    /// <summary>
    /// Number of copies obtainable from boss, friendly monster, and Ragtime Mouse
    /// encounters combined. Each such encounter contributes 1 to this count.
    ///
    ///   • Boss/Stage enemies   — one-time steal or drop
    ///   • Friendly monsters    — 9 unique Mu→Yan chain encounters, each one-time
    ///   • Ragtime Mouse        — item reward is one-time (subsequent encounters give XP/AP)
    ///
    /// Zero if this item has no boss-type source.
    /// </summary>
    public int BossInstanceCount { get; init; }

    /// <summary>
    /// Number of copies found by WorldMapVariableScanner (or the hardcoded
    /// VanillaObtainabilityData.WorldMapVariableItemCounts fallback).
    ///
    /// Covers all delivery events that use variable-reference AddItem in world map
    /// scripts — FieldItemScanner cannot resolve these:
    ///   • Convention A / B — Dead Pepper ocean/crack/Chocobo Garden digs
    ///   • Convention C     — Chocograph World_Chest function rewards (e.g., Ragnarok,
    ///                        Dragon's Claws)
    ///
    /// Zero if this item is not delivered via world map variable AddItem.
    /// </summary>
    public int WorldMapInstanceCount { get; init; }

    /// <summary>
    /// Combined count of copies obtainable from the Treno Auction House.
    ///   • int.MaxValue — item can be won repeatedly (infinite source)
    ///   • 1            — item appears exactly once (finite source)
    ///   • 0            — item is not available at auction
    ///
    /// Items with AuctionCount == int.MaxValue are promoted to the infinite set
    /// by HasInfiniteSource.
    /// </summary>
    public int AuctionCount { get; init; }

    /// <summary>
    /// True if this item appears in any field chest, hidden item, or scripted
    /// item reward found by <see cref="FieldItemScanner"/>.
    /// The instance count is stored in <see cref="FieldInstanceCount"/>.
    /// </summary>
    public bool IsFieldItem { get; init; }

    /// <summary>
    /// Number of distinct field instances (unique file × offset pairs) found
    /// by <see cref="FieldItemScanner"/>. Zero if <see cref="IsFieldItem"/> is false.
    /// </summary>
    public int FieldInstanceCount { get; init; }

    /// <summary>
    /// Number of copies obtainable via synthesis from finite ingredients.
    /// Set by VanillaItemCatalog.Build() step 5b.
    /// Zero if:
    ///   • the item is not a synthesis result, OR
    ///   • synthesis is infinite (item is already in the infinite set), OR
    ///   • insufficient finite ingredients exist to produce even one copy.
    /// </summary>
    public int SynthesisInstanceCount { get; init; }

    // ── Type flags (from Synthesis.csv) ───────────────────────────────────────

    /// <summary>
    /// True if this item can be produced by any Synthesis recipe (Synthesis.csv Result column).
    /// Whether synthesis is an infinite source depends on ingredient availability.
    /// </summary>
    public bool IsSynthesisResult { get; init; }

    /// <summary>
    /// True if this item is used as an ingredient in any Synthesis recipe
    /// (Synthesis.csv Ingredients column).
    /// </summary>
    public bool IsSynthesisIngredient { get; init; }

    // ── Metadata flags ─────────────────────────────────────────────────────────

    /// <summary>
    /// True if at least one obtainable copy of this item requires specific story
    /// timing, a one-time steal/drop opportunity, or a shop that permanently closes.
    ///
    /// Source: <see cref="VanillaObtainabilityData.MissableItemIds"/> (guide-derived).
    ///
    /// This is metadata — it does NOT affect <see cref="IsFiniteOnly"/>,
    /// ObtainabilityCounts, or pool candidacy. The RecommendedLogicEngine (Phase 6)
    /// uses this flag to enforce placement constraints and warn when missable copies
    /// are required to meet item caps.
    ///
    /// Notable missables: Genji set (Memoria steals), Excalibur II (sub-12h),
    /// Running Shoes (Tantarian/Hades steals), Moonstone/Emerald/Diamond
    /// (story-point pickups + Stiltzkin packages), Dark Matter (final boss).
    /// </summary>
    public bool IsMissable { get; init; }

    // ── Computed properties ───────────────────────────────────────────────────

    /// <summary>
    /// True if ANY source for this item is infinite (repeatable without limit).
    ///
    /// Infinite sources:
    ///   • Shop (IsInShop) — also used as proxy for synthesis from all-infinite ingredients
    ///   • Normal enemy drop/steal (HasNormalEnemySource)
    ///   • Auction repeatable (AuctionCount == int.MaxValue)
    /// </summary>
    public bool HasInfiniteSource =>
        IsInShop ||
        HasNormalEnemySource ||
        AuctionCount == int.MaxValue;

    /// <summary>
    /// True if this item has at least one finite obtainable source but no infinite source.
    /// </summary>
    public bool IsFiniteOnly =>
        !HasInfiniteSource &&
        (BossInstanceCount > 0 ||
         WorldMapInstanceCount > 0 ||
         AuctionCount > 0 ||
         IsFieldItem ||
         SynthesisInstanceCount > 0);

    /// <summary>
    /// True if this item is obtainable through any means (infinite or finite).
    /// </summary>
    public bool IsObtainable => HasInfiniteSource || IsFiniteOnly;

    /// <summary>
    /// True if this item is a valid candidate for Recommended-mode randomization.
    /// Excludes key items and items with no obtainable source.
    /// (Phase 6 RecommendedLogicEngine enforces caps separately for finite items.)
    /// </summary>
    public bool IsRecommendedPoolCandidate =>
        !IsKeyItem &&
        IsObtainable;
}