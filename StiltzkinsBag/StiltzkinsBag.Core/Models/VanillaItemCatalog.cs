// StiltzkinsBag.Core/Models/VanillaItemCatalog.cs
//
// Assembles the complete vanilla item obtainability catalog from all data sources:
//   • Items.csv                — item category flags (consumable, usable)
//   • ShopItems.csv            — items in shops (infinite source)
//   • Synthesis.csv            — recipes: result items and ingredient items
//   • VanillaObtainabilityData — enemy, chocograph, auction, boss, friendly, ragtime (hardcoded)
//   • FieldItemScanner         — field chests, hidden items, scripted rewards (runtime binary scan)
//   • WorldMapVariableScanner  — world map variable-ref AddItem rewards (dead pepper, chocograph
//                                World_Chest deliveries)
//
// The catalog produces:
//
//   1. ObtainabilityCounts — Dictionary<int, int>
//      Master count for every item ID (0–255):
//        int.MaxValue  = at least one infinite source (shop, normal enemy, repeatable auction)
//        N             = sum of all finite instance counts across all sources
//        absent        = item is not obtainable through any vanilla means
//
//   2. Entries — IReadOnlyDictionary<int, ItemObtainabilityEntry>
//      Per-item source breakdown for pool construction and constraint enforcement.
//
// Synthesis infinite-source resolution:
//   A synthesis recipe's result is considered infinite if ALL its ingredients are
//   available from at least one infinite source (shop or normal enemy or repeatable auction,
//   transitively through other synthesis recipes). The catalog resolves this iteratively
//   until no more items can be promoted to infinite.
//
// Usage:
//   var catalog = VanillaItemCatalog.Build(
//       itemsCsvPath, shopItemsCsvPath, synthesisCsvPath,
//       fieldItemCounts   // from FieldItemScanner.ScanArchive() or .ScanFiles()
//   );
//   int count = catalog.ObtainabilityCounts[87];   // Wing Edge: int.MaxValue (in shop)
//   bool isKeyItem = catalog.Entries[211].IsKeyItem; // Always false (Phase 5 placeholder)

using System;
using System.Collections.Generic;
using System.Linq;
using StiltzkinsBag.Core.Models.Battle;
using StiltzkinsBag.Models.Csv;
using StiltzkinsBag.Parsing;

namespace StiltzkinsBag.Core.Models;

/// <summary>
/// Complete vanilla item obtainability catalog for FFIX PC (Steam).
/// Build once per generation via <see cref="Build"/>.
/// </summary>
public sealed class VanillaItemCatalog
{
    // ── Public surface ─────────────────────────────────────────────────────────

    /// <summary>
    /// Master obtainability count for each item ID.
    ///
    /// Key   = item ID (0–255)
    /// Value = <see cref="int.MaxValue"/> if the item has at least one infinite source;
    ///         otherwise, the total number of finite copies available in vanilla.
    ///         Items absent from this dictionary are not obtainable through any means.
    /// </summary>
    public IReadOnlyDictionary<int, int> ObtainabilityCounts { get; }

    /// <summary>
    /// Per-item flag breakdown for all 256 item IDs.
    /// Items with no known source still have an entry (all flags false, count 0).
    /// </summary>
    public IReadOnlyDictionary<int, ItemObtainabilityEntry> Entries { get; }

    // ── Constructor (private — use Build factory) ─────────────────────────────

    private VanillaItemCatalog(
        IReadOnlyDictionary<int, int> counts,
        IReadOnlyDictionary<int, ItemObtainabilityEntry> entries)
    {
        ObtainabilityCounts = counts;
        Entries = entries;
    }

    // ── Convenience queries ────────────────────────────────────────────────────

    /// <summary>Returns true if this item has any infinite source.</summary>
    public bool IsInfinite(int itemId) =>
        ObtainabilityCounts.TryGetValue(itemId, out int c) && c == int.MaxValue;

    /// <summary>Returns true if this item has at least one obtainable copy (finite or infinite).</summary>
    public bool IsObtainable(int itemId) =>
        ObtainabilityCounts.ContainsKey(itemId);

    /// <summary>
    /// Returns the total finite copy count, or 0 if the item is infinite or unobtainable.
    /// </summary>
    public int FiniteCount(int itemId) =>
        ObtainabilityCounts.TryGetValue(itemId, out int c) && c != int.MaxValue ? c : 0;

    // ── Name parsing ──────────────────────────────────────────────────────────

    /// <summary>
    /// Extracts item names from Items.csv inline comments aligned with parsed rows.
    ///
    /// Items.csv stores inline comments in the format: <c># NNN - Name</c>
    /// e.g. <c># 000 - Hammer</c>, <c># 001 - Dagger</c>.
    ///
    /// <see cref="MemoriaCsvParser.Read{T,TMap}"/> strips these into
    /// <see cref="ParsedCsv{T}.InlineComments"/> (index-aligned with Rows).
    /// This helper parses each comment and returns a map of item ID → display name.
    /// </summary>
    private static Dictionary<int, string> ParseItemNames(
        IReadOnlyList<ItemsRow> rows,
        List<string?> inlineComments)
    {
        var names = new Dictionary<int, string>(rows.Count);

        for (int i = 0; i < rows.Count && i < inlineComments.Count; i++)
        {
            string? comment = inlineComments[i];
            if (string.IsNullOrWhiteSpace(comment)) continue;

            // Strip leading # and whitespace, then split on first " - "
            // Format: "# 000 - Hammer" or ";# 000 - Hammer"
            string trimmed = comment.TrimStart(';', '#', ' ', '\t');
            int dashIdx = trimmed.IndexOf(" - ", StringComparison.Ordinal);
            if (dashIdx < 0) continue;

            string name = trimmed[(dashIdx + 3)..].Trim();
            if (name.Length > 0)
                names[rows[i].Id] = name;
        }

        return names;
    }

    // ── Factory ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds the catalog from CSV file paths and pre-computed item count maps.
    /// </summary>
    /// <param name="itemsCsvPath">Full path to Items.csv.</param>
    /// <param name="shopItemsCsvPath">Full path to ShopItems.csv.</param>
    /// <param name="synthesisCsvPath">Full path to Synthesis.csv.</param>
    /// <param name="fieldItemCounts">
    /// Per-item field instance counts from <see cref="FieldItemScanner"/>.
    /// Pass an empty dictionary if field scanning was skipped.
    /// </param>
    /// <param name="worldMapCounts">
    /// Per-item world map variable-reference AddItem counts from <see cref="WorldMapVariableScanner"/>.
    /// Covers Dead Pepper ocean/crack rewards (Convention A/B) and Chocograph World_Chest
    /// deliveries (Convention C). When null, falls back to
    /// <see cref="VanillaObtainabilityData.WorldMapVariableItemCounts"/>.
    /// These items use variable-reference AddItem calls that FieldItemScanner cannot resolve.
    /// </param>
    /// <param name="battleScanResult">
    /// Drop, steal, and card item sets from <see cref="BattleItemScanner"/>.
    /// When provided, replaces the hardcoded <see cref="VanillaObtainabilityData.NormalEnemyItemIds"/>
    /// for HasNormalEnemySource computation. Items in AllEnemyItemIds that are not in
    /// BossOnlyItemIds are treated as having an infinite repeatable enemy source.
    /// Pass null to fall back to the hardcoded set (backward compatible).
    /// </param>
    /// <exception cref="ArgumentNullException">Any required path is null.</exception>
    /// <exception cref="System.IO.FileNotFoundException">A CSV file was not found.</exception>
    public static VanillaItemCatalog Build(
        string itemsCsvPath,
        string shopItemsCsvPath,
        string synthesisCsvPath,
        IReadOnlyDictionary<int, int> fieldItemCounts,
        IReadOnlyDictionary<int, int>? worldMapCounts = null,
        BattleScanResult? battleScanResult = null)
    {
        ArgumentNullException.ThrowIfNull(itemsCsvPath);
        ArgumentNullException.ThrowIfNull(shopItemsCsvPath);
        ArgumentNullException.ThrowIfNull(synthesisCsvPath);
        ArgumentNullException.ThrowIfNull(fieldItemCounts);

        // Resolve world map source: runtime scanner output takes priority;
        // hardcoded VanillaObtainabilityData.WorldMapVariableItemCounts is the fallback
        // when scanner output is not available (e.g. world map .eb.bytes not configured).
        var effectiveWorldMap = worldMapCounts
            ?? (IReadOnlyDictionary<int, int>)VanillaObtainabilityData.WorldMapVariableItemCounts;

        // ── Merge sentinel-excluded field items ───────────────────────────────
        //
        // FieldItemScanner filters item ID 0 (null/default sentinel in FFIX scripts)
        // to prevent false positives (e.g. Hammer appearing 85 times).
        // VanillaObtainabilityData.SentinelExcludedFieldItems provides the verified
        // correct count for each sentinel-excluded item. Merging here ensures that
        // IsFieldItem and FieldInstanceCount are accurate in the assembled entries.
        var effectiveFieldCounts = new Dictionary<int, int>(fieldItemCounts);
        foreach (var (sentinelId, sentinelCount) in VanillaObtainabilityData.SentinelExcludedFieldItems)
            effectiveFieldCounts[sentinelId] = effectiveFieldCounts.TryGetValue(sentinelId, out int sentinelEx)
                ? sentinelEx + sentinelCount
                : sentinelCount;

        // ── 1. Load CSVs ───────────────────────────────────────────────────────

        var itemsParsed = MemoriaCsvParser.Read<ItemsRow, ItemsRowMap>(itemsCsvPath);
        var shopParsed = MemoriaCsvParser.Read<ShopItemsRow, ShopItemsRowMap>(shopItemsCsvPath);
        var synthParsed = MemoriaCsvParser.Read<SynthesisRow, SynthesisRowMap>(synthesisCsvPath);

        IReadOnlyList<ItemsRow> items = itemsParsed.Rows;
        IReadOnlyList<ShopItemsRow> shops = shopParsed.Rows;
        IReadOnlyList<SynthesisRow> syntheses = synthParsed.Rows;

        // ── 1b. Parse item names from Items.csv inline comments ───────────────
        //
        // Items.csv stores a trailing inline comment on each data row:
        //   e.g.  0;...;# 000 - Hammer
        //         1;...;# 001 - Dagger
        // MemoriaCsvParser strips these into ParsedCsv.InlineComments (index-aligned
        // with Rows). ParseItemNames extracts the display name for each item ID.

        var itemNames = ParseItemNames(itemsParsed.Rows, itemsParsed.InlineComments);

        // ── 2. Build lookup sets ───────────────────────────────────────────────

        // All items sold in any shop
        var shopItemSet = new HashSet<int>();
        foreach (ShopItemsRow shop in shops)
            foreach (int id in shop.Items)
                shopItemSet.Add(id);

        // Synthesis results and ingredients
        var synthesisResults = new HashSet<int>();
        var synthesisIngredients = new HashSet<int>();
        // Map: result item ID → list of ingredient ID sets (one per recipe producing it)
        var recipeIngredients = new Dictionary<int, List<int[]>>();
        foreach (SynthesisRow recipe in syntheses)
        {
            if (recipe.Result <= 0) continue;
            synthesisResults.Add(recipe.Result);
            foreach (int ing in recipe.Ingredients)
                synthesisIngredients.Add(ing);
            if (!recipeIngredients.TryGetValue(recipe.Result, out var list))
                recipeIngredients[recipe.Result] = list = new List<int[]>();
            list.Add(recipe.Ingredients);
        }

        // ── 3. Build per-item entries ─────────────────────────────────────────

        var entries = new Dictionary<int, ItemObtainabilityEntry>(256);

        foreach (ItemsRow item in items)
        {
            if (item.Id < 0 || item.Id > 255) continue;

            // IsKeyItem is always false for Phase 5.
            // All 256 Items.csv entries are regular items (weapons, armor, consumables, gems).
            // FFIX key items (Mist, Oglop, Cid's Hammer, etc.) live in a separate data table
            // not yet parsed. Items with Price = 2 are rare equipment (Save the Queen, Genji
            // set, Ribbon) — NOT key items.
            const bool isKey = false;

            bool isGem = item.Gem;
            bool isEquip = item.Weapon || item.Armlet || item.Helmet ||
                           item.Armor || item.Accessory;

            // "Consumable" = item-slot item consumed on use. Usable = battle-usable flag.
            // Tents/Cottages have Usable=0 but carry the Item flag — include both.
            // Gems and equipment are excluded (they occupy item slots but are not consumed).
            bool isConsumable = item.Usable ||
                                (item.Item && !isGem && !isEquip);

            bool inShop = shopItemSet.Contains(item.Id);

            // HasNormalEnemySource: uses scanner output minus BossOnlyItemIds when available;
            // falls back to hardcoded NormalEnemyItemIds set when no scanner result provided.
            bool normalE = battleScanResult != null
                ? (battleScanResult.AllEnemyItemIds.Contains(item.Id)
                   && !VanillaObtainabilityData.BossOnlyItemIds.Contains(item.Id))
                : VanillaObtainabilityData.NormalEnemyItemIds.Contains(item.Id);

            // BossInstanceCount = one-time boss steal/drop + friendly monster + Ragtime Mouse.
            // Each contributes 1 (one encounter, one item opportunity).
            // Ragtime rewards an item only on first encounter; later encounters give XP/AP only.
            int bossCount = 0;
            if (VanillaObtainabilityData.BossOnlyItemIds.Contains(item.Id)) bossCount++;
            if (VanillaObtainabilityData.FriendlyMonsterItemIds.Contains(item.Id)) bossCount++;
            if (VanillaObtainabilityData.RagtimeMouseItemIds.Contains(item.Id)) bossCount++;

            // WorldMapInstanceCount from WorldMapVariableScanner (or hardcoded fallback).
            // Covers dead pepper rewards (Conv A/B) and chocograph World_Chest (Conv C).
            effectiveWorldMap.TryGetValue(item.Id, out int worldMapCount);

            // AuctionCount: int.MaxValue = repeatable (infinite source),
            //               1            = one-time (finite source),
            //               0            = not at auction.
            int auctionCount = 0;
            if (VanillaObtainabilityData.AuctionRepeatableItemIds.Contains(item.Id))
                auctionCount = int.MaxValue;
            else if (VanillaObtainabilityData.AuctionOneTimeItemIds.Contains(item.Id))
                auctionCount = 1;

            bool missable = VanillaObtainabilityData.MissableItemIds.Contains(item.Id);
            bool synthR = synthesisResults.Contains(item.Id);
            bool synthI = synthesisIngredients.Contains(item.Id);

            effectiveFieldCounts.TryGetValue(item.Id, out int fieldCount);
            bool fieldItem = fieldCount > 0;

            entries[item.Id] = new ItemObtainabilityEntry
            {
                ItemId = item.Id,
                Name = itemNames.TryGetValue(item.Id, out var name) ? name : string.Empty,
                IsKeyItem = isKey,
                IsConsumable = isConsumable,
                IsGem = isGem,
                IsEquipment = isEquip,
                IsInShop = inShop,
                HasNormalEnemySource = normalE,
                BossInstanceCount = bossCount,
                WorldMapInstanceCount = worldMapCount,
                AuctionCount = auctionCount,
                IsMissable = missable,
                IsFieldItem = fieldItem,
                FieldInstanceCount = fieldCount,
                IsSynthesisResult = synthR,
                IsSynthesisIngredient = synthI,
                // SynthesisInstanceCount set in step 5b below
            };
        }

        // ── 4. Resolve synthesis infinite sources iteratively ─────────────────
        //
        // A synthesis result is infinite if ALL its ingredients are infinite.
        // "Infinite" at this stage means: in shop, OR has normal enemy source, OR
        // auction repeatable, OR is itself an infinite synthesis result.
        //
        // Example: Butterfly Sword (result) from Dagger + Mage Masher (both in shops)
        //          → Butterfly Sword synthesis is infinite.
        //          Pumice (result) from Pumice Piece x2 (finite boss drop + chocograph)
        //          → Pumice synthesis is NOT infinite.

        // Seed the infinite set with all currently-infinite items
        var infiniteSet = new HashSet<int>();
        foreach (var (id, entry) in entries)
            if (entry.HasInfiniteSource) // shop / normal enemy / auction repeatable
                infiniteSet.Add(id);

        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (var (resultId, ingredientsList) in recipeIngredients)
            {
                if (infiniteSet.Contains(resultId)) continue;
                // Check if ANY recipe for this result has ALL infinite ingredients
                foreach (int[] ingredients in ingredientsList)
                {
                    if (ingredients.Length == 0) continue;
                    if (ingredients.All(ing => infiniteSet.Contains(ing)))
                    {
                        infiniteSet.Add(resultId);
                        changed = true;
                        // Mark IsInShop as proxy for "infinite synthesis source"
                        // so HasInfiniteSource computed property returns true.
                        if (entries.TryGetValue(resultId, out var existing))
                        {
                            entries[resultId] = existing with { IsInShop = true };
                        }
                        break;
                    }
                }
            }
        }

        // ── 5. Build master ObtainabilityCounts dictionary ────────────────────

        var counts = new Dictionary<int, int>();

        foreach (var (id, entry) in entries)
        {
            // Items with infinite sources get int.MaxValue
            if (entry.HasInfiniteSource || infiniteSet.Contains(id))
            {
                counts[id] = int.MaxValue;
                continue;
            }

            // Sum all finite sources
            int total = 0;

            // Boss-type encounters: boss fights, friendly monsters, Ragtime Mouse.
            // Each is a one-time encounter yielding at most 1 item (steal or drop).
            // BossInstanceCount encodes the combined count of such opportunities.
            total += entry.BossInstanceCount;

            // World map variable-reference AddItem rewards (dead pepper, chocograph
            // World_Chest). FieldItemScanner cannot resolve these — WorldMapVariableScanner
            // or the hardcoded WorldMapVariableItemCounts fallback provides the counts.
            total += entry.WorldMapInstanceCount;

            // Chocograph fallback — for chocograph items NOT covered by WorldMapInstanceCount
            // and NOT found by FieldItemScanner.
            //
            // Most chocograph rewards are delivered via literal AddItem in field scripts
            // (found by FieldItemScanner, counted in FieldInstanceCount). A subset use
            // variable-reference AddItem in the World_Chest function (found by the world map
            // scanner, counted in WorldMapInstanceCount — e.g. Ragnarok ID 29, Dragon's
            // Claws ID 45). ChocographItemCounts is a guide-verified fallback for any item
            // not yet resolved by either scanner.
            //
            // Guard: skip if already counted by WorldMapVariableScanner (WorldMapInstanceCount > 0)
            // or FieldItemScanner (IsFieldItem). Both would double-count otherwise.
            if (entry.WorldMapInstanceCount == 0 && !entry.IsFieldItem)
            {
                if (VanillaObtainabilityData.ChocographItemCounts.TryGetValue(id, out int chocoCount)
                    && chocoCount > 0)
                    total += chocoCount;
            }

            // Auction one-time purchase (repeatable auction already handled by infiniteSet above).
            if (entry.AuctionCount == 1)
                total += 1;

            if (entry.IsFieldItem)
                total += entry.FieldInstanceCount;

            // Only include in the dictionary if at least one obtainable copy exists
            if (total > 0)
                counts[id] = total;

            // IsSynthesisResult without an infinite ingredient path → synthesis is finite
            // (e.g., Pumice from Pumice Piece). The synthesis count for finite-only results
            // is resolved in step 5b below.
        }

        // ── 5b. Add finite synthesis result counts ────────────────────────────
        //
        // For synthesis results that are NOT infinite (not promoted in step 4) and have
        // no count from other sources yet (not in counts from step 5), compute the number
        // of copies obtainable via synthesis from finite ingredients.
        //
        // For each recipe producing such a result:
        //   qty_needed[ingId] = number of times ingId appears in Ingredients (encoded as repetition)
        //   available[ingId]  = counts[ingId] if finite; int.MaxValue if infinite; 0 if absent
        //   possible          = min over all distinct ingIds of floor(available / qty_needed)
        //
        // We take the MAX possible across all recipes for the same result (best-case crafting path)
        // and add that count only if the result has no other obtainable source yet.
        //
        // Example: Tin Armor (176) = Hammer (ID 0, count=1) + Ore (ID 254, infinite)
        //   → possible = min(1/1, ∞) = 1 → counts[176] = 1

        foreach (var (resultId, ingredientsList) in recipeIngredients)
        {
            // Skip results already counted as infinite or with counts from other sources.
            if (infiniteSet.Contains(resultId)) continue;
            if (counts.ContainsKey(resultId)) continue;

            int maxSynthCount = 0;

            foreach (int[] ingredients in ingredientsList)
            {
                if (ingredients.Length == 0) continue;

                // Tally quantity needed per ingredient ID (repetition = quantity).
                var ingredientQtys = new Dictionary<int, int>();
                foreach (int ingId in ingredients)
                    ingredientQtys[ingId] = ingredientQtys.TryGetValue(ingId, out int q) ? q + 1 : 1;

                // Compute copies possible from this recipe.
                int possible = int.MaxValue;
                foreach (var (ingId, qtyNeeded) in ingredientQtys)
                {
                    if (!counts.TryGetValue(ingId, out int available))
                    {
                        possible = 0;
                        break;
                    }
                    if (available == int.MaxValue) continue; // infinite ingredient; no constraint
                    possible = Math.Min(possible, available / qtyNeeded);
                }
                // Guard: if every ingredient was infinite but result is not in infiniteSet,
                // that indicates a data inconsistency — treat as 0 to be conservative.
                if (possible == int.MaxValue) possible = 0;

                maxSynthCount = Math.Max(maxSynthCount, possible);
            }

            if (maxSynthCount > 0)
            {
                counts[resultId] = maxSynthCount;
                // Stamp SynthesisInstanceCount onto the entry so Entries reflects the source.
                if (entries.TryGetValue(resultId, out var synthEntry))
                    entries[resultId] = synthEntry with { SynthesisInstanceCount = maxSynthCount };
            }
        }

        return new VanillaItemCatalog(counts, entries);
    }
}