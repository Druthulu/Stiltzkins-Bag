// StiltzkinsBag.Core/Models/LegendaryItemList.cs
//
// Static definitions of item rarity tiers used by RecommendedLogicEngine.
//
// ── Two tiers ────────────────────────────────────────────────────────────────
//
//   Unique    — exactly 1 copy exists in a vanilla playthrough.
//               These items must NEVER appear in shops (shops are infinite sources).
//               They MAY appear in chests, enemy drops, or synthesis results —
//               each of those is a one-time find, preserving rarity.
//               Enforcement: if a Unique ID lands in shop output, replace it.
//
//   Legendary — rare items with 2–3 vanilla copies (e.g. one boss drop + one
//               chocograph). Must not appear in randomized shops in Recommended
//               mode, but may appear in multiple finite sources.
//               Enforcement: same shop rule as Unique.
//
// ── Shop rule (both tiers) ───────────────────────────────────────────────────
//
//   No item from AllLegendaryItemIds may appear in any shop row in Recommended
//   mode. RecommendedLogicEngine.EnforceLegendaryRarity() removes any such item
//   and replaces it with a shop-safe item at a comparable price tier.
//
// ── Verification notes ───────────────────────────────────────────────────────
//
//   All IDs verified against Items.csv (0–255) and ShopItems.csv (Phase 6).
//   None of the items listed here appear in any vanilla shop — confirmed by
//   scanning all 32 shop rows in ShopItems.csv. Items that DO appear in vanilla
//   shops (e.g. Excalibur II would have been Unique but is absent from shops by
//   vanilla design) are included based on their rarity in the world, not shop data.
//
//   Save the Queen (26) — Beatrix's personal weapon. Unobtainable by the player
//   in normal play. Classified Unique to prevent item shuffle from placing it in
//   a shop. May appear in a chest (one-time find) if item shuffle assigns it there.

using System.Collections.Generic;
using System.Linq;

namespace StiltzkinsBag.Core.Models;

/// <summary>
/// Static rarity definitions for items that require special handling in
/// Recommended mode. Used by <see cref="RecommendedLogicEngine"/>.
/// </summary>
public static class LegendaryItemList
{
    // UNVERIFIED: field chest counts not trustworthy until FieldParser false-positive fix (Phase 9)

    // ── Unique items ──────────────────────────────────────────────────────────

    /// <summary>
    /// Items that exist as exactly 1 copy in a vanilla playthrough.
    ///
    /// Rules in Recommended mode:
    ///   • Must NEVER appear in any shop (shops are infinite — rarity destroyed).
    ///   • May appear in chests, enemy drops, or synthesis results (one-time finds).
    ///   • Combined across all output sources, each Unique ID must appear at most once.
    ///
    /// Note: "At most once across all sources" is enforced by ChestRandomizer's
    /// existing uniqueness constraint (Phase 4). RecommendedLogicEngine enforces
    /// the shop-exclusion rule.
    /// </summary>
    public static readonly IReadOnlySet<int> UniqueItemIds = new HashSet<int>
    {
        // ── Best-in-slot weapons ─────────────────────────────────────────────
         15,  // Ultima Weapon   — Dead Pepper dig (Forgotten Continent crack)
         26,  // Save the Queen  — Beatrix's weapon; player-unobtainable normally
         29,  // Ragnarok        — Chocograph (Outer Island) / Dead Pepper
         30,  // Excalibur II    — Memoria, sub-12-hour clock
         50,  // Rune Claws      — Late-game weapon, finite source only

        // ── Genji set (missable steal from Memoria bosses) ───────────────────
         49,  // Duel Claws      — steal from Deathguise
        109,  // Genji Gloves    — steal from Tiamat
        146,  // Genji Helmet    — steal from Maliris
        189,  // Genji Armor     — steal from Kraken

        // ── Story-significant accessories ────────────────────────────────────
        210,  // Pumice Piece    — Chocograph #22 / Dead Pepper (Shimmering Island)
        211,  // Pumice          — Ozma drop (combines two Pumice Pieces)
        221,  // Ribbon          — Chocograph (Fairy Continent) / Stiltzkin 5555G

        // ── Rare one-time items ───────────────────────────────────────────────
         98,  // Thief Gloves    — Treno Auction (one-time lot)
        175,  // Robe of Lords   — Late-game robe, finite source only
        222,  // Maiden Prayer   — Dead Pepper dig (Iifa Tree underwater)
    };

    // ── Legendary items ───────────────────────────────────────────────────────

    /// <summary>
    /// Items that are rare but have 2–3 vanilla copies across the playthrough.
    ///
    /// Rules in Recommended mode:
    ///   • Must NOT appear in any shop.
    ///   • May appear in multiple finite sources (chests, drops, synthesis).
    /// </summary>
    public static readonly IReadOnlySet<int> LegendaryItemIds = new HashSet<int>
    {
        // ── Rare weapons (finite, late-game) ─────────────────────────────────
         13,  // Masamune        — boss drop (Trance Kuja)
         14,  // The Tower       — late-game weapon, limited sources
         27,  // Ultima Sword    — limited synthesis + finite sources
         28,  // Excalibur       — late-game weapon, limited sources
         40,  // Dragon's Hair   — Dead Pepper dig
         45,  // Dragon's Claws  — Chocograph (Forgotten Lagoon)
         47,  // Avenger         — late-game, finite only
         48,  // Kaiser Knuckles — Chocograph + normal enemy (finite counts)
         56,  // Tiger Racket    — Dead Pepper dig
         84,  // Gastro Fork     — late-game fork, finite only
         78,  // Mace of Zeus    — late-game staff, finite only

        // ── Rare armor ───────────────────────────────────────────────────────
        147,  // Grand Helm      — boss drop only
        166,  // Rubber Suit     — boss drop only
        171,  // Glutton's Robe  — boss drop only
        172,  // White Robe      — Chocograph + boss drop
        173,  // Black Robe      — Dead Pepper + boss drop
        174,  // Light Robe      — Chocograph + boss drop
        184,  // Demon's Mail    — Chocograph + late-game shop (Shop 0016/0017/0018 — confirmed absent)
        190,  // Maximillian     — Dead Pepper dig
        191,  // Grand Armor     — boss drop only

        // ── Rare accessories ─────────────────────────────────────────────────
        197,  // Battle Boots    — boss drop only
        198,  // Running Shoes   — boss drop + missable steal (Tantarian/Hades)
        201,  // Black Belt      — boss drop only
        204,  // Rosetta Ring    — Dead Pepper dig + Friendly Monster (Yan)
        205,  // Reflect Ring    — Treno Auction (one-time) + boss drop
        208,  // Rebirth Ring    — Chocograph #24 + boss drop
        209,  // Protect Ring    — Dead Pepper + Ragtime Mouse + enemy (all finite)

        // ── Consumables that are rare enough to protect ───────────────────────
        250,  // Dark Matter     — Treno Auction (one-time) + Necron steal/drop
    };

    // ── Convenience union ─────────────────────────────────────────────────────

    /// <summary>
    /// All item IDs subject to the "no legendary items in shops" rule.
    /// Union of <see cref="UniqueItemIds"/> and <see cref="LegendaryItemIds"/>.
    /// Used by <see cref="RecommendedLogicEngine.EnforceLegendaryRarity"/>.
    /// </summary>
    public static readonly IReadOnlySet<int> AllProtectedItemIds =
        new HashSet<int>(UniqueItemIds.Concat(LegendaryItemIds));
}