// StiltzkinsBag.Core/Randomizers/RecommendedLogicEngine.cs
//
// Constraint validation and correction pass for Recommended mode.
//
// ── Architecture ──────────────────────────────────────────────────────────────
//
// All methods are pure static functions:
//   • Accept data (CSV rows, catalogs, lookup tables)
//   • Return corrected data
//   • No file I/O, no random state, no side effects
//
// Phase 8 (RandomizerEngine) is responsible for:
//   • Calling each method only when Recommended mode is active
//   • Passing the output of each Phase 5 randomizer as input here
//   • Writing the corrected data to the mod output folder
//
// ── Methods ────────────────────────────────────────────────────────────────────
//
//   CorrectEquipmentCoherence  — re-assigns DefaultEquipmentSet values to better
//                                match each character's randomized slot types
//   EnforceLegendaryRarity     — removes protected items (Unique/Legendary) from
//                                all shop rows and replaces them from a safe pool
//   EnforceSynthesisReachability — validates synthesis recipes against two rules:
//                                   Rule 8A: ingredients reachable before shop unlocks
//                                   Rule 8B: finite result items not over-produced
//
// ── Caller contract (Phase 8) ─────────────────────────────────────────────────
//
//   // 1. After CharacterRandomizer:
//   var correctedParams = RecommendedLogicEngine.CorrectEquipmentCoherence(
//       characterResult.CharacterParameters,
//       characterResult.SlotAssignment,
//       vanillaEquipmentRows);
//   var correctedCharResult = characterResult with { CharacterParameters = correctedParams };
//
//   // 2. After ShopRandomizer:
//   var correctedShop = RecommendedLogicEngine.EnforceLegendaryRarity(
//       shopResult,
//       LegendaryItemList.AllProtectedItemIds,
//       ItemPool.ShopFriendly,
//       rng);
//
//   // 3. After SynthesisRandomizer:
//   var correctedSynth = RecommendedLogicEngine.EnforceSynthesisReachability(
//       synthesisResult,
//       SynthesisShopFieldIds.All,           // developer-provided field ID table (Task 8)
//       itemIdToMinFieldId,                  // pre-computed from FieldItemScanner output
//       catalog,
//       LegendaryItemList.UniqueItemIds,
//       rng);

using System;
using System.Collections.Generic;
using System.Linq;
using StiltzkinsBag.Core.Models;
using StiltzkinsBag.Models.Csv;

namespace StiltzkinsBag.Randomizers;

/// <summary>
/// Constraint validation and correction pass for Recommended mode.
/// All methods are pure static functions — no file I/O, no side effects.
/// Phase 8 calls these methods after each randomizer and writes the corrected data.
/// </summary>
public static class RecommendedLogicEngine
{
    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Re-assigns <c>DefaultEquipmentSet</c> values among characters 0–7 to
    /// improve alignment between each character's slot types (speciality) and
    /// the weapon affinity of their equipment set.
    ///
    /// ── What this corrects ────────────────────────────────────────────────────
    ///
    /// Phase 5 Sub-step 4 Fisher-Yates shuffles equipment set IDs across characters.
    /// This can produce a Black Mage holding a sword-heavy physical set while a
    /// Knight holds a rod/staff magical set. This method swaps set assignments to
    /// reduce such mismatches.
    ///
    /// ── Affinity classification ───────────────────────────────────────────────
    ///
    /// Equipment sets are scored by the weapon item ID in their Weapon slot:
    ///   • Rods, staves, rackets, flutes (IDs 51–78) → Magical
    ///   • Swords, spears, claws, knuckles (IDs 16–50) → Physical
    ///   • Daggers (IDs 1–15) and forks (IDs 79–87) → Balanced
    ///   • No weapon (slot value ≤ 0) → None
    ///
    /// Slot types are classified by their command role:
    ///   • Magical: "Blk Mag", "Focus", "Summon-A", "Summon-B", "Wht Mag-A", "Wht Mag-B"
    ///   • Physical: "Skill", "Swd Art", "Swd Mag", "Jump", "Dragon", "Flair", "Throw"
    ///   • Balanced: "Steal", "Blu Mag", "Eat" (and any unrecognised slot name)
    ///
    /// A character's overall SlotAffinity is determined by majority vote across their
    /// assigned slot types. Ties resolve to Balanced.
    ///
    /// ── Correction strategy ───────────────────────────────────────────────────
    ///
    /// Greedy pairwise improvement over characters 0–7:
    ///   For each pair (i, j), if swapping their DefaultEquipmentSet values produces
    ///   a higher combined MatchScore, perform the swap. Repeat until no beneficial
    ///   swap exists in a full pass.
    ///
    /// MatchScore table (SlotAffinity × WeaponAffinity):
    ///   Magical + Magical   = 2   ← perfect match
    ///   Physical + Physical = 2   ← perfect match
    ///   Balanced + any      = 1   ← no preference
    ///   any + Balanced/None = 1   ← universal weapon
    ///   Magical + Physical  = 0   ← mismatch
    ///   Physical + Magical  = 0   ← mismatch
    ///
    /// Characters 8–15 (guests) are never modified — their rows pass through unchanged.
    /// If no swap improves the total score, the original assignment is returned as-is.
    ///
    /// ── Phase 8 integration ───────────────────────────────────────────────────
    ///
    /// Returns a corrected list of <see cref="CharacterParametersRow"/> objects.
    /// Only <c>DefaultEquipmentSet</c> values may change — all other columns are
    /// preserved exactly. The caller substitutes this list into the result record:
    /// <code>
    /// var corrected = RecommendedLogicEngine.CorrectEquipmentCoherence(...);
    /// var finalResult = characterResult with { CharacterParameters = corrected };
    /// </code>
    ///
    /// ── Deviation from Task 3 skeleton ───────────────────────────────────────
    ///
    /// The skeleton declared a <c>baseStats</c> parameter for tie-breaking by stat
    /// profile. This parameter was removed: the greedy pairwise swap handles ties
    /// naturally (no swap = no improvement), making stat-based tie-breaking
    /// unnecessary complexity.
    /// </summary>
    /// <param name="characterParams">
    /// All character rows from CharacterRandomizerResult.CharacterParameters.
    /// Must include rows with Id 0–7. DefaultEquipmentSet values have already
    /// been Fisher-Yates shuffled by Phase 5.
    /// </param>
    /// <param name="slotAssignment">
    /// From CharacterRandomizerResult.SlotAssignment.
    /// Maps character ID (0–7) to ordered list of slot type name strings.
    /// </param>
    /// <param name="equipmentSets">
    /// Vanilla DefaultEquipment.csv rows. The available equipment sets.
    /// Read from the game's StreamingAssets before any randomization.
    /// </param>
    public static IReadOnlyList<CharacterParametersRow> CorrectEquipmentCoherence(
        IReadOnlyList<CharacterParametersRow> characterParams,
        IReadOnlyDictionary<int, IReadOnlyList<string>> slotAssignment,
        IReadOnlyList<DefaultEquipmentRow> equipmentSets)
    {
        ArgumentNullException.ThrowIfNull(characterParams);
        ArgumentNullException.ThrowIfNull(slotAssignment);
        ArgumentNullException.ThrowIfNull(equipmentSets);

        // Build O(1) set lookup: setId → DefaultEquipmentRow
        var setById = equipmentSets.ToDictionary(r => r.Id);

        // Collect the 8 main character rows (IDs 0–7) into an indexed array.
        // CharacterParameters may contain all 12+ rows (IDs 0–11).
        var charRows = new CharacterParametersRow[8];
        foreach (var row in characterParams)
        {
            if (row.Id is >= 0 and <= 7)
                charRows[row.Id] = row;
        }

        // Guard: all 8 main character rows must be present.
        for (int i = 0; i < 8; i++)
        {
            if (charRows[i] is null)
                throw new ArgumentException(
                    $"characterParams is missing a row for character ID {i}.",
                    nameof(characterParams));
        }

        // Working array: charSetIds[charId] = current DefaultEquipmentSet value.
        // The algorithm mutates only this array — input objects are never touched.
        var charSetIds = new int[8];
        for (int i = 0; i < 8; i++)
            charSetIds[i] = charRows[i].DefaultEquipmentSet;

        // Pre-compute slot affinity for each character.
        var slotAffinities = new SlotAffinity[8];
        for (int i = 0; i < 8; i++)
        {
            slotAffinities[i] = slotAssignment.TryGetValue(i, out var slots)
                ? ClassifySlots(slots)
                : SlotAffinity.Balanced;
        }

        // Helper: weapon affinity for a given set ID.
        WeaponAffinity WeaponAffinityForSet(int setId) =>
            setById.TryGetValue(setId, out var set)
                ? ClassifyWeapon(set.Weapon)
                : WeaponAffinity.None;

        // Greedy pairwise improvement.
        // Worst case: 7 passes × 28 pairs = 196 comparisons for 8 characters.
        bool improved;
        do
        {
            improved = false;
            for (int i = 0; i < 7; i++)
            {
                for (int j = i + 1; j < 8; j++)
                {
                    int setI = charSetIds[i];
                    int setJ = charSetIds[j];

                    int scoreBefore = MatchScore(slotAffinities[i], WeaponAffinityForSet(setI))
                                    + MatchScore(slotAffinities[j], WeaponAffinityForSet(setJ));

                    int scoreAfter = MatchScore(slotAffinities[i], WeaponAffinityForSet(setJ))
                                    + MatchScore(slotAffinities[j], WeaponAffinityForSet(setI));

                    if (scoreAfter > scoreBefore)
                    {
                        charSetIds[i] = setJ;
                        charSetIds[j] = setI;
                        improved = true;
                    }
                }
            }
        }
        while (improved);

        // Build output: corrected rows for changed characters; original references otherwise.
        var result = new List<CharacterParametersRow>(characterParams.Count);
        foreach (var row in characterParams)
        {
            if (row.Id is >= 0 and <= 7 && charSetIds[row.Id] != row.DefaultEquipmentSet)
            {
                // Create a new instance — never mutate the input object.
                result.Add(new CharacterParametersRow
                {
                    Id = row.Id,
                    DefaultRow = row.DefaultRow,
                    DefaultWinPose = row.DefaultWinPose,
                    DefaultCategory = row.DefaultCategory,
                    DefaultCommandSet = row.DefaultCommandSet,
                    DefaultEquipmentSet = charSetIds[row.Id],   // ← corrected
                    BattleParameterFormula = row.BattleParameterFormula,
                    NameKeyword = row.NameKeyword,
                });
            }
            else
            {
                result.Add(row);  // unchanged — return original reference
            }
        }

        return result;
    }

    /// <summary>
    /// Ensures no item from <paramref name="protectedIds"/> appears in any shop row.
    ///
    /// ── What this corrects ────────────────────────────────────────────────────
    ///
    /// ShopRandomizer may place Unique or Legendary items into shops. Since shops
    /// are infinite sources, a Unique item in a shop destroys its rarity. This
    /// method scans all shop rows and replaces any protected item with a random
    /// draw from <paramref name="replacementPool"/>.
    ///
    /// ── Replacement strategy ──────────────────────────────────────────────────
    ///
    /// For each shop row, iterate all item slots. If a slot contains a protected
    /// item ID, replace it with <c>rng.Next(replacementPool.Count)</c> draw.
    /// The replacement pool must be pre-filtered by the caller to contain only
    /// shop-safe items (e.g. <c>ItemPool.ShopFriendly</c>).
    ///
    /// The same item may appear as a replacement in multiple shops — there is no
    /// uniqueness constraint on shop inventory. Only the "no protected item in any
    /// shop" constraint is enforced here.
    ///
    /// ── Phase 8 integration ───────────────────────────────────────────────────
    ///
    /// <code>
    /// var corrected = RecommendedLogicEngine.EnforceLegendaryRarity(
    ///     shopRows,
    ///     LegendaryItemList.AllProtectedItemIds,
    ///     ItemPool.ShopFriendly,
    ///     rng);
    /// </code>
    ///
    /// ── RNG calls ─────────────────────────────────────────────────────────────
    ///
    /// One <c>rng.Next(replacementPool.Count)</c> call per replaced item slot.
    /// Shops are visited in row order (ID ascending). Within each row, item slots
    /// are visited in list order. This order is deterministic given sorted input.
    /// </summary>
    /// <param name="shopRows">All shop rows from ShopItems.csv after ShopRandomizer.</param>
    /// <param name="protectedIds">
    /// Item IDs that must not appear in any shop.
    /// Use <see cref="LegendaryItemList.AllProtectedItemIds"/> for Recommended mode.
    /// </param>
    /// <param name="replacementPool">
    /// Pool of shop-safe item IDs for replacement draws.
    /// Caller is responsible for ensuring this pool contains no protected items.
    /// Use <c>ItemPool.ShopFriendly</c> or equivalent.
    /// </param>
    /// <param name="rng">Seeded Random instance from SeedEngine. Must not be null.</param>
    public static IReadOnlyList<ShopItemsRow> EnforceLegendaryRarity(
        IReadOnlyList<ShopItemsRow> shopRows,
        IReadOnlySet<int> protectedIds,
        IReadOnlyList<int> replacementPool,
        Random rng)
    {
        ArgumentNullException.ThrowIfNull(shopRows);
        ArgumentNullException.ThrowIfNull(protectedIds);
        ArgumentNullException.ThrowIfNull(replacementPool);
        ArgumentNullException.ThrowIfNull(rng);

        if (replacementPool.Count == 0)
            throw new ArgumentException(
                "Replacement pool must not be empty.", nameof(replacementPool));

        var result = new List<ShopItemsRow>(shopRows.Count);

        foreach (ShopItemsRow row in shopRows)
        {
            // Fast path: empty shop or no protected items present — return original reference.
            if (row.Items.Length == 0 || !row.Items.Any(id => protectedIds.Contains(id)))
            {
                result.Add(row);
                continue;
            }

            // At least one protected item found — build a corrected Items array.
            // Never mutate the original array.
            var correctedItems = new int[row.Items.Length];
            for (int i = 0; i < row.Items.Length; i++)
            {
                correctedItems[i] = protectedIds.Contains(row.Items[i])
                    ? replacementPool[rng.Next(replacementPool.Count)]
                    : row.Items[i];
            }

            result.Add(new ShopItemsRow
            {
                Comment = row.Comment,
                Id = row.Id,
                Items = correctedItems,
            });
        }

        return result;
    }

    /// <summary>
    /// Validates synthesis recipes against two reachability rules and corrects
    /// or removes recipes that would violate them.
    ///
    /// ── Rule 8A: Ingredient reachability (field ID proxy) ────────────────────
    ///
    /// For each synthesis shop (identified by field ID), all ingredients of all
    /// recipes sold by that shop must have at least one obtainability source whose
    /// minimum field ID is strictly less than the shop's field ID.
    ///
    /// This uses field ID as a ~90% accurate story progression proxy. A definitive
    /// reachability check requires scenario_counter storyboard (Gen2).
    ///
    /// ── Rule 8B: Finite result cap ────────────────────────────────────────────
    ///
    /// A synthesis recipe whose result is in <paramref name="finiteProtectedIds"/>
    /// must not allow crafting more copies than the item's vanilla finite count.
    ///
    /// ── Parking lot note ──────────────────────────────────────────────────────
    ///
    /// <paramref name="synthesisShopFieldIds"/> data (synthesis shop ID → field ID)
    /// is developer-provided and has not been collected yet (Phase 6 Task 8).
    /// </summary>
    public static IReadOnlyList<SynthesisRow> EnforceSynthesisReachability(
        IReadOnlyList<SynthesisRow> synthesisRows,
        IReadOnlyDictionary<int, int> synthesisShopFieldIds,
        IReadOnlyDictionary<int, int> itemIdToMinFieldId,
        IReadOnlyDictionary<int, ItemObtainabilityEntry> catalog,
        IReadOnlySet<int> finiteProtectedIds,
        Random rng)
    {
        ArgumentNullException.ThrowIfNull(synthesisRows);
        ArgumentNullException.ThrowIfNull(synthesisShopFieldIds);
        ArgumentNullException.ThrowIfNull(itemIdToMinFieldId);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(finiteProtectedIds);
        ArgumentNullException.ThrowIfNull(rng);

        // rng is accepted for API consistency and future use (e.g. generating fallback
        // recipes with reachable replacement ingredients). Current implementation does
        // not consume RNG — Rule 8A removes offending shops rather than rebuilding recipes.

        // ── Rule 8A — Ingredient reachability (field ID proxy) ────────────────
        //
        // For each recipe, check every shop it appears in. If a shop's field ID is
        // known and an ingredient is not reachable before that field ID, remove that
        // shop from the recipe's Shops array. If all shops are removed, drop the recipe.
        //
        // Ingredient reachability rules (in priority order):
        //   1. HasInfiniteSource (IsInShop / HasNormalEnemySource / repeatable auction)
        //      → always reachable; treat as field ID 0
        //   2. IsFieldItem + present in itemIdToMinFieldId
        //      → reachable if minFieldId < shopFieldId
        //   3. Finite-only with no field source (boss drops, world map digs, etc.)
        //      → treated as reachable; we lack field IDs for these sources
        //   4. Not in catalog (unknown item)
        //      → treated as reachable; don't block on missing data

        var afterRuleA = new List<SynthesisRow>(synthesisRows.Count);

        foreach (SynthesisRow row in synthesisRows)
        {
            // Fast path: no shops listed — pass through unchanged.
            if (row.Shops.Length == 0)
            {
                afterRuleA.Add(row);
                continue;
            }

            var survivingShops = new List<int>(row.Shops.Length);

            foreach (int shopId in row.Shops)
            {
                // If we don't know this shop's field ID, leave it in — conservative.
                if (!synthesisShopFieldIds.TryGetValue(shopId, out int shopFieldId))
                {
                    survivingShops.Add(shopId);
                    continue;
                }

                // Check every ingredient against this shop's field ID.
                bool allIngredientsReachable = true;
                foreach (int ingredientId in row.Ingredients)
                {
                    if (!catalog.TryGetValue(ingredientId, out var entry))
                    {
                        // Unknown ingredient — assume reachable.
                        continue;
                    }

                    if (entry.HasInfiniteSource)
                    {
                        // Infinite source (shop, normal enemy, repeatable auction) —
                        // always available, field ID effectively 0.
                        continue;
                    }

                    if (entry.IsFieldItem &&
                        itemIdToMinFieldId.TryGetValue(ingredientId, out int minFieldId))
                    {
                        if (minFieldId >= shopFieldId)
                        {
                            // Earliest known field source is at or after this shop —
                            // ingredient is not reachable in time.
                            allIngredientsReachable = false;
                            break;
                        }
                        continue;
                    }

                    // Finite-only, no field source data — treat as reachable.
                    // (Boss drops, world map items, etc. — we lack field IDs for these.)
                }

                if (allIngredientsReachable)
                    survivingShops.Add(shopId);
            }

            if (survivingShops.Count == row.Shops.Length)
            {
                // No shops were removed — return original reference.
                afterRuleA.Add(row);
            }
            else if (survivingShops.Count > 0)
            {
                // Some shops removed — build corrected row.
                afterRuleA.Add(new SynthesisRow
                {
                    Comment = row.Comment,
                    Id = row.Id,
                    Shops = survivingShops.ToArray(),
                    Price = row.Price,
                    Result = row.Result,
                    Ingredients = row.Ingredients,
                });
            }
            // survivingShops.Count == 0 → recipe dropped entirely (not added to afterRuleA).
        }

        // ── Rule 8B — Finite result cap ───────────────────────────────────────
        //
        // For each Unique item (in finiteProtectedIds) that appears as a synthesis
        // result, count how many recipes produce it. If the count exceeds the item's
        // vanilla finite obtain count, remove excess recipes (keep lowest Id first).
        //
        // Only applies when the item is finite-only in vanilla — if it has an infinite
        // source, no cap is needed.

        // Group remaining recipes by Result item ID for Unique items.
        var recipesByResult = new Dictionary<int, List<SynthesisRow>>();
        foreach (SynthesisRow row in afterRuleA)
        {
            if (!finiteProtectedIds.Contains(row.Result))
                continue;

            if (!recipesByResult.TryGetValue(row.Result, out var list))
            {
                list = new List<SynthesisRow>();
                recipesByResult[row.Result] = list;
            }
            list.Add(row);
        }

        // Build the set of row IDs to drop.
        var droppedIds = new HashSet<int>();
        foreach (var (resultId, recipes) in recipesByResult)
        {
            if (recipes.Count <= 1)
                continue;  // Only one recipe — no cap violation possible.

            if (!catalog.TryGetValue(resultId, out var resultEntry))
                continue;  // Unknown item — don't enforce cap.

            if (resultEntry.HasInfiniteSource)
                continue;  // Infinite source — no cap needed.

            // Vanilla finite count: sum of all finite source counts.
            int vanillaCount =
                resultEntry.BossInstanceCount +
                resultEntry.WorldMapInstanceCount +
                (resultEntry.AuctionCount == int.MaxValue ? 0 : resultEntry.AuctionCount) +
                resultEntry.FieldInstanceCount +
                resultEntry.SynthesisInstanceCount;

            if (vanillaCount <= 0)
                vanillaCount = 1;  // At minimum, allow 1 synthesis recipe.

            // Sort by Id ascending — keep the first vanillaCount, drop the rest.
            var sorted = recipes.OrderBy(r => r.Id).ToList();
            for (int i = vanillaCount; i < sorted.Count; i++)
                droppedIds.Add(sorted[i].Id);
        }

        if (droppedIds.Count == 0)
            return afterRuleA;  // No Rule 8B removals — return Rule 8A result as-is.

        // Build final output excluding dropped recipe IDs.
        var result = new List<SynthesisRow>(afterRuleA.Count);
        foreach (SynthesisRow row in afterRuleA)
        {
            if (!droppedIds.Contains(row.Id))
                result.Add(row);
        }

        return result;
    }

    // ── Internal classification helpers ────────────────────────────────────────
    //
    // Used by CorrectEquipmentCoherence.
    // Weapon item ID ranges map directly to physical/magical affinity without
    // requiring Stats.csv or Weapons.csv joins.
    //
    // Slot type name strings match the Command Slot Type Registry in
    // AbilityTierClassification_Rev4.md (the implementation contract for CharacterRandomizer).

    /// <summary>
    /// Physical/magical affinity of an equipment set, based on its weapon slot.
    /// </summary>
    internal enum WeaponAffinity
    {
        /// <summary>Rods, staves, rackets, flutes — item IDs 51–78.</summary>
        Magical,

        /// <summary>Swords, spears, claws, knuckles — item IDs 16–50.</summary>
        Physical,

        /// <summary>Daggers (1–15) and forks (79–87) — usable by any class.</summary>
        Balanced,

        /// <summary>Weapon slot is empty (value ≤ 0).</summary>
        None,
    }

    /// <summary>
    /// Physical/magical affinity of a character's assigned slot types.
    /// </summary>
    internal enum SlotAffinity
    {
        /// <summary>
        /// Primarily magical: "Blk Mag", "Focus",
        /// "Summon-A", "Summon-B", "Wht Mag-A", "Wht Mag-B".
        /// </summary>
        Magical,

        /// <summary>
        /// Primarily physical: "Skill", "Swd Art", "Swd Mag",
        /// "Jump", "Dragon", "Flair", "Throw".
        /// </summary>
        Physical,

        /// <summary>
        /// Mixed or role-neutral: "Steal", "Blu Mag", "Eat",
        /// or any unrecognised slot name.
        /// </summary>
        Balanced,
    }

    /// <summary>
    /// Returns the weapon affinity of an item by ID, using the canonical
    /// FFIX weapon type ID ranges.
    /// </summary>
    /// <param name="weaponItemId">
    /// Weapon item ID from <see cref="DefaultEquipmentRow.Weapon"/>.
    /// Pass ≤ 0 for empty slot (DefaultEquipmentRow uses -1 for "no item").
    /// </param>
    internal static WeaponAffinity ClassifyWeapon(int weaponItemId)
    {
        if (weaponItemId <= 0) return WeaponAffinity.None;      // empty slot
        if (weaponItemId <= 15) return WeaponAffinity.Balanced;  // daggers
        if (weaponItemId <= 50) return WeaponAffinity.Physical;  // swords/spears/claws/knuckles
        if (weaponItemId <= 78) return WeaponAffinity.Magical;   // rackets/rods/flutes
        if (weaponItemId <= 87) return WeaponAffinity.Balanced;  // forks

        // IDs 88+ are armlets/accessories — should never appear in the Weapon slot.
        return WeaponAffinity.Balanced;
    }

    /// <summary>
    /// Returns the slot affinity of a character based on their assigned slot types.
    ///
    /// Each slot type contributes a score: Magical = +1, Physical = −1, Balanced = 0.
    /// Final score > 0 → Magical; &lt; 0 → Physical; = 0 → Balanced.
    ///
    /// Examples with 2 slot types:
    ///   ["Blk Mag", "Focus"]      = +2 → Magical
    ///   ["Swd Art", "Swd Mag"]    = -2 → Physical
    ///   ["Summon-A", "Dragon"]    =  0 → Balanced  (magic + physical tie)
    ///   ["Steal", "Skill"]        = -1 → Physical  (Zidane)
    ///   ["Eat", "Blu Mag"]        =  0 → Balanced  (BaChingChing)
    /// </summary>
    internal static SlotAffinity ClassifySlots(IReadOnlyList<string> slotTypes)
    {
        if (slotTypes is null || slotTypes.Count == 0)
            return SlotAffinity.Balanced;

        int score = 0;
        foreach (string slot in slotTypes)
            score += GetSlotScore(slot);

        if (score > 0) return SlotAffinity.Magical;
        if (score < 0) return SlotAffinity.Physical;
        return SlotAffinity.Balanced;
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Match quality score for a (SlotAffinity, WeaponAffinity) pairing.
    /// Used by the greedy pairwise swap to evaluate improvement candidates.
    /// </summary>
    private static int MatchScore(SlotAffinity slot, WeaponAffinity weapon) =>
        (slot, weapon) switch
        {
            (SlotAffinity.Magical, WeaponAffinity.Magical) => 2,  // perfect match
            (SlotAffinity.Physical, WeaponAffinity.Physical) => 2,  // perfect match
            (SlotAffinity.Balanced, _) => 1,  // no preference
            (_, WeaponAffinity.Balanced) => 1,  // universal weapon
            (_, WeaponAffinity.None) => 1,  // no weapon — neutral
            _ => 0,  // cross-affinity mismatch
        };

    /// <summary>
    /// Returns the affinity score contribution of a single slot type name.
    /// Magical = +1, Physical = −1, Balanced/unknown = 0.
    ///
    /// Case-sensitive — slot type name strings are written by CharacterRandomizer
    /// from the Command Slot Type Registry (AbilityTierClassification_Rev4.md).
    /// </summary>
    private static int GetSlotScore(string slotType) => slotType switch
    {
        // ── Magical ───────────────────────────────────────────────────────────
        "Blk Mag" => +1,
        "Focus" => +1,
        "Summon-A" => +1,
        "Summon-B" => +1,
        "Wht Mag-A" => +1,
        "Wht Mag-B" => +1,

        // ── Physical ──────────────────────────────────────────────────────────
        "Skill" => -1,  // Zidane's Skill (Flee + physical Barbarian skills)
        "Swd Art" => -1,  // Knight sword arts
        "Swd Mag" => -1,  // Magic Sword — Knight class, uses swords
        "Jump" => -1,  // Dragoon jump
        "Dragon" => -1,  // Druid dragon skills
        "Flair" => -1,  // Monk/Amarant physical skills
        "Throw" => -1,  // Throw physical items

        // ── Balanced (0): "Steal", "Blu Mag", "Eat", and any unknown slot ─────
        _ => 0,
    };
}