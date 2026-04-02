// StiltzkinsBag.Core/Models/SynthesisShopData.cs
//
// Static mapping of synthesis shop IDs to the field IDs of the fields
// that host those shops in the FF9 Steam release.
//
// ── Purpose ───────────────────────────────────────────────────────────────────
//
// Used by RecommendedLogicEngine.EnforceSynthesisReachability() as a ~90%
// accurate story-progression proxy: ingredients for a synthesis recipe must
// have at least one obtainability source whose field ID is lower than the
// shop's field ID.
//
// ── Methodology ───────────────────────────────────────────────────────────────
//
// Shop IDs were identified by scanning vanilla field scripts for the pattern:
//   Wait( 3 )
//   Menu( 2, N )      ← N = synthesis shop ID
//   Wait( 3 )
// using the regex: .*Wait\( 3 \)\n.*Menu\( 2, 3[2-9] \)\n.*Wait\( 3 \)
//
// Field IDs were identified from the vanilla field script filename numeric prefixes.
// Where shop ID 36 appears in multiple fields (Alexandria, Treno, Lindblum, Alley),
// the highest field ID (2453 — Alexandria Alley) is used as the representative.
// This is the most conservative/permissive choice — an ingredient that is available
// before field 2453 is guaranteed to be available before all earlier shop 36 locations.
//
// ── Story order vs. field ID ordering ─────────────────────────────────────────
//
// Shop 38 (Mage Village end-game, field 3054) has a higher numeric field ID than
// shop 39 (Hades/Memoria Birth, field 2914), but story-wise it is accessed first.
// This is a known game development artifact (confirmed by developer). Shop 38 is
// therefore assigned a virtual story-order field ID of 2857 — the midpoint between
// shops 37 (2801) and 39 (2914) — to place it correctly in the progression proxy.
//
// All other field IDs match the actual vanilla field script numeric prefixes.
//
// ── Limitations ───────────────────────────────────────────────────────────────
//
// This is a field-ID proxy, not a scenario_counter storyboard. It will produce
// incorrect results for ~10% of edge cases. Accurate reachability validation
// requires Gen2 scenario_counter storyboard integration (Phase 8 parking lot).
//
// ── Source ────────────────────────────────────────────────────────────────────
//
// Confirmed by developer scan of vanilla field scripts (2026-04-02).
// Field script filenames: _560.txt, _902.txt, _1308.txt, _1455.txt,
//   _2453.txt, _2801.txt, _2914.txt, _3054.txt (+ earlier _138, _242, etc.)

using System.Collections.Generic;

namespace StiltzkinsBag.Core.Models;

/// <summary>
/// Static mapping of synthesis shop IDs to story-order field IDs.
/// Used by <see cref="StiltzkinsBag.Randomizers.RecommendedLogicEngine.EnforceSynthesisReachability"/>
/// as a story-progression proxy for ingredient reachability validation.
/// </summary>
public static class SynthesisShopData
{
    /// <summary>
    /// Maps synthesis shop ID → representative story-order field ID.
    ///
    /// Story order (ascending field ID = earlier in story):
    ///   32 → 560   Lindblum (Disc 1, after escaping Alexandria)
    ///   33 → 902   Treno (Disc 1, after leaving Gargan Roo)
    ///   34 → 1308  Lindblum (Disc 2, after Cleyra)
    ///   35 → 1455  Black Mage Village (Disc 2, after Iifa Tree)
    ///   36 → 2453  Alexandria/Treno/Lindblum (Disc 3, highest representative field ID)
    ///   37 → 2801  Daguerreo (Disc 3/4, after Oeilvert)
    ///   38 → 2857  Black Mage Village end-game (Disc 4, story-adjusted — see file header)
    ///   39 → 2914  Hades/Memoria Birth (Disc 4, after Memoria entrance)
    /// </summary>
    public static readonly IReadOnlyDictionary<int, int> ShopFieldIds =
        new Dictionary<int, int>
        {
            { 32, 560  },  // Lindblum — first synthesis shop
            { 33, 902  },  // Treno
            { 34, 1308 },  // Lindblum (Disc 2)
            { 35, 1455 },  // Black Mage Village (Disc 2)
            { 36, 2453 },  // Alexandria/Treno/Lindblum/Alley — highest representative
            { 37, 2801 },  // Daguerreo
            { 38, 2857 },  // Black Mage Village end-game — virtual story-order field ID
                           // (actual field 3054 > field 2914, but story-order is before Hades)
            { 39, 2914 },  // Hades/Memoria Birth — final synthesis shop
        };

    /// <summary>
    /// Returns true if <paramref name="shopId"/> is a known synthesis shop.
    /// </summary>
    public static bool IsKnownShop(int shopId) => ShopFieldIds.ContainsKey(shopId);
}