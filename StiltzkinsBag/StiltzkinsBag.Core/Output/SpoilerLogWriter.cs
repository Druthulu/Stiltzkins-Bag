using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using StiltzkinsBag.Models;

namespace StiltzkinsBag.Core.Output
{
    /// <summary>
    /// Writes the human-readable spoiler log for a completed generation run.
    ///
    /// Output: [SeedFolderPath]/Spoiler-Seed-[seedInt].txt
    ///
    /// Sections emitted depend on which features were enabled in Settings.
    /// Sections for disabled features are omitted entirely — the log only
    /// documents what actually changed.
    ///
    /// Item IDs are resolved to names when <see cref="SpoilerLogData.ItemNames"/>
    /// is provided. Without it, items appear as "Item #N".
    /// </summary>
    public static class SpoilerLogWriter
    {
        private const int SectionWidth = 55;

        // Vanilla Stiltzkin prices in visit order (ascending price = visit order).
        // Visit 1 = 333 Gil (Alexandria), Visit 8 = 5555 Gil (Memoria).
        private static readonly int[] VanillaStiltzkinPrices =
            { 333, 444, 555, 666, 777, 888, 2222, 5555 };

        // Character names by ID (0–7 = main party, 8–15 = guests).
        private static readonly string[] CharacterNames =
            { "Zidane", "Vivi", "Garnet", "Steiner", "Freya", "Quina", "Eiko", "Amarant" };

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>
        /// Writes the spoiler log file to the seed folder root.
        /// </summary>
        /// <param name="seedFolderPath">
        /// Absolute path to the root of the seed folder
        /// (i.e. <see cref="ModOutputWriter.SeedFolderPath"/>).
        /// </param>
        /// <param name="data">All result data collected during the generation run.</param>
        public static void Write(string seedFolderPath, SpoilerLogData data)
        {
            string fileName = $"Spoiler-Seed-{data.SeedInt}.txt";
            string outputPath = Path.Combine(seedFolderPath, fileName);

            var sb = new StringBuilder();

            AppendHeader(sb, data);
            AppendCharacters(sb, data);
            AppendFieldItems(sb, data);
            AppendStiltzkin(sb, data);
            AppendShops(sb, data);
            AppendSynthesis(sb, data);
            AppendAbilityGems(sb, data);
            AppendInitialItems(sb, data);
            AppendEnemies(sb, data);
            AppendTetraMaster(sb, data);
            AppendSettingsSummary(sb, data);

            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
        }

        // ── Section builders ─────────────────────────────────────────────────

        private static void AppendHeader(StringBuilder sb, SpoilerLogData data)
        {
            string bar = new string('═', SectionWidth);
            string mode = data.Settings.Mode == RandomizerMode.Recommended
                ? "Recommended"
                : "Chaos";
            string timestamp = DateTime.UtcNow.ToString(
                "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            sb.AppendLine(bar);
            sb.AppendLine(Center("STILTZKIN'S BAG — SPOILER LOG", SectionWidth));
            sb.AppendLine(bar);
            sb.AppendLine();
            sb.AppendLine($"  Seed:      {data.SeedInt}");
            sb.AppendLine($"  Mode:      {mode}");
            sb.AppendLine($"  Generated: {timestamp} UTC");
            sb.AppendLine($"  Version:   1.0");
            sb.AppendLine();
        }

        private static void AppendCharacters(StringBuilder sb, SpoilerLogData data)
        {
            if (data.CharacterResult == null || !data.Settings.RandomizeCharacters)
                return;

            AppendSectionHeader(sb, "CHARACTERS");

            for (int id = 0; id < 8; id++)
            {
                string name = id < CharacterNames.Length ? CharacterNames[id] : $"Char {id}";

                if (data.CharacterResult.SlotAssignment.TryGetValue(id, out var slots))
                {
                    sb.AppendLine($"  {name,-10} — {string.Join(", ", slots)}");
                }
                else
                {
                    sb.AppendLine($"  {name,-10} — (unchanged)");
                }
            }

            sb.AppendLine();
        }

        private static void AppendFieldItems(StringBuilder sb, SpoilerLogData data)
        {
            if (data.FieldResult == null || !data.Settings.RandomizeTreasureChests)
                return;

            AppendSectionHeader(sb, "FIELD ITEMS / TREASURE CHESTS");

            var r = data.FieldResult;
            sb.AppendLine($"  Fields scanned:              {r.FieldsScanned,5}");
            sb.AppendLine($"  Fields patched:              {r.FieldsPatched,5}");
            sb.AppendLine($"  TreasureItem locations:      {r.TreasureLocationsPatched,5}");
            sb.AppendLine($"  DirectItem locations:        {r.DirectLocationsPatched,5}");
            sb.AppendLine($"  TextSync locations:          {r.TextSyncLocationsPatched,5}");

            if (r.StiltzkinScriptsExcluded > 0)
                sb.AppendLine($"  Stiltzkin scripts excluded:  {r.StiltzkinScriptsExcluded,5}");

            sb.AppendLine();
            sb.AppendLine("  Note: Per-location breakdown requires a full field scan log.");
            sb.AppendLine("        See the mod folder's Battle/ and field binary files.");
            sb.AppendLine();
        }

        private static void AppendStiltzkin(StringBuilder sb, SpoilerLogData data)
        {
            if (data.StiltzkinResult == null || !data.StiltzkinResult.WasRun)
                return;

            AppendSectionHeader(sb, "STILTZKIN PACKAGES");

            var r = data.StiltzkinResult;
            int visitNum = 1;

            foreach (int vanillaPrice in VanillaStiltzkinPrices)
            {
                if (!r.NewItemsByVanillaPrice.TryGetValue(vanillaPrice, out int[]? items))
                {
                    visitNum++;
                    continue;
                }

                r.NewPriceByVanillaPrice.TryGetValue(vanillaPrice, out int newPrice);
                string priceDisplay = newPrice == vanillaPrice
                    ? $"{vanillaPrice} Gil"
                    : $"{vanillaPrice} → {newPrice} Gil";

                string itemList = string.Join(", ",
                    items.Select(id => GetItemName(id, data.ItemNames)));

                sb.AppendLine($"  Visit {visitNum} ({priceDisplay}):");
                sb.AppendLine($"    {itemList}");
                visitNum++;
            }

            sb.AppendLine();
        }

        private static void AppendShops(StringBuilder sb, SpoilerLogData data)
        {
            if (data.ShopResult == null || !data.Settings.RandomizeShops)
                return;

            AppendSectionHeader(sb, "SHOPS");

            var nonEmpty = data.ShopResult.Shops
                .Where(s => s.Items.Length > 0)
                .OrderBy(s => s.Id);

            foreach (var shop in nonEmpty)
            {
                string items = string.Join(", ",
                    shop.Items.Select(id => GetItemName(id, data.ItemNames)));
                sb.AppendLine($"  Shop {shop.Id,3}: {items}");
            }

            sb.AppendLine();
        }

        private static void AppendSynthesis(StringBuilder sb, SpoilerLogData data)
        {
            if (data.SynthesisResult == null || !data.Settings.RandomizeSynthesis)
                return;

            AppendSectionHeader(sb, "SYNTHESIS RECIPES");

            foreach (var recipe in data.SynthesisResult.Recipes.OrderBy(r => r.Id))
            {
                string result = GetItemName(recipe.Result, data.ItemNames);
                string ingredients = string.Join(" + ",
                    recipe.Ingredients.Select(id => GetItemName(id, data.ItemNames)));
                string shopsStr = recipe.Shops.Length > 0
                    ? $"  [Shops: {string.Join(", ", recipe.Shops)}]"
                    : "  [No shops]";

                sb.AppendLine($"  #{recipe.Id,2}: {ingredients} → {result}  [{recipe.Price} Gil]{shopsStr}");
            }

            sb.AppendLine();
        }

        private static void AppendAbilityGems(StringBuilder sb, SpoilerLogData data)
        {
            if (data.GemsResult == null || !data.Settings.RandomizeAbilityGems)
                return;

            AppendSectionHeader(sb, "ABILITY GEM EQUIP COSTS");

            foreach (var row in data.GemsResult)
            {
                string name = CleanAbilityComment(row.Comment) ?? $"Ability {row.Id}";
                sb.AppendLine($"  {name,-32} {row.Gems,2} gems");
            }

            sb.AppendLine();
        }

        private static void AppendInitialItems(StringBuilder sb, SpoilerLogData data)
        {
            if (data.InitialItemsResult == null || !data.Settings.RandomizeInitialItems)
                return;

            AppendSectionHeader(sb, "STARTING ITEMS");

            foreach (var item in data.InitialItemsResult)
            {
                sb.AppendLine($"  {item.Count,2}× {GetItemName(item.ItemID, data.ItemNames)}");
            }

            sb.AppendLine();
        }

        private static void AppendEnemies(StringBuilder sb, SpoilerLogData data)
        {
            bool anyEnemy = data.EnemyDropsRemapped
                || data.EnemyBlueMagicShuffled
                || data.EnemyCardDropsShuffled;

            if (!anyEnemy)
                return;

            AppendSectionHeader(sb, "ENEMIES");

            if (data.EnemyDropsRemapped)
                sb.AppendLine("  Drop / steal item IDs remapped via ItemRemapTable.");
            if (data.EnemyBlueMagicShuffled)
                sb.AppendLine("  Blue magic spells shuffled across all enemy stat blocks.");
            if (data.EnemyCardDropsShuffled)
                sb.AppendLine("  Card drop IDs shuffled across all enemy stat blocks.");

            sb.AppendLine();
            sb.AppendLine("  Note: Per-enemy detail requires an enemy binary scan.");
            sb.AppendLine("        Extract and inspect the Battle/ binary files for specifics.");
            sb.AppendLine();
        }

        private static void AppendTetraMaster(StringBuilder sb, SpoilerLogData data)
        {
            if (!data.TetraMasterRandomizationApplied || !data.Settings.RandomizeTetraMaster)
                return;

            AppendSectionHeader(sb, "TETRAMASTER");

            if (data.Settings.RandomizeCardStats)
                sb.AppendLine($"  Card stats:   {data.Settings.CardStatMode}");
            if (data.Settings.CardTypeMode != CardTypeMode.Preserve)
                sb.AppendLine($"  Card types:   {data.Settings.CardTypeMode}");
            if (data.Settings.ArrowMode != ArrowMode.Preserve)
                sb.AppendLine($"  Arrows:       {data.Settings.ArrowMode}");
            if (data.Settings.ShuffleCardOrder)
                sb.AppendLine("  Card order:   Shuffled");
            if (data.Settings.RandomizeCardSets)
                sb.AppendLine($"  Card sets:    {data.Settings.CardSetMode}");
            if (data.Settings.ShuffleNpcDecks)
                sb.AppendLine("  NPC decks:    Shuffled");
            if (data.Settings.NpcDifficultyMode != NpcDifficultyMode.Preserve)
                sb.AppendLine($"  NPC difficulty: {data.Settings.NpcDifficultyMode}");

            sb.AppendLine();
            sb.AppendLine("  Note: Card stat values are in binary — inspect card data files");
            sb.AppendLine("        for specific per-card stats.");
            sb.AppendLine();
        }

        private static void AppendSettingsSummary(StringBuilder sb, SpoilerLogData data)
        {
            AppendSectionHeader(sb, "SETTINGS USED");

            var s = data.Settings;
            sb.AppendLine($"  Mode:                  {s.Mode}");
            sb.AppendLine($"  Seed string:           \"{s.SeedString}\"");
            sb.AppendLine($"  Seed integer:          {data.SeedInt}");
            sb.AppendLine();
            sb.AppendLine($"  Randomize characters:  {s.RandomizeCharacters}");
            sb.AppendLine($"  Randomize chests:      {s.RandomizeTreasureChests}");
            sb.AppendLine($"  Stiltzkin mode:        {s.StiltzkinMode}");
            sb.AppendLine($"  Randomize shops:       {s.RandomizeShops}");
            sb.AppendLine($"  Randomize synthesis:   {s.RandomizeSynthesis}");
            sb.AppendLine($"  Randomize ability gems:{s.RandomizeAbilityGems}");
            sb.AppendLine($"  Randomize ability AP:  {s.RandomizeAbilityAp}");
            sb.AppendLine($"  Randomize gear stats:  {s.RandomizeGearStatBonuses}");
            sb.AppendLine($"  Randomize enemies:     {s.RandomizeEnemies}");
            sb.AppendLine($"  Randomize Tetramaster: {s.RandomizeTetraMaster}");

            if (s.BadEconomy)
                sb.AppendLine($"  Bad Economy:           ON  [{s.BadEconomyPriceMultiplierMin:F1}×–{s.BadEconomyPriceMultiplierMax:F1}×]");
            if (s.ShortSupply)
                sb.AppendLine($"  Short Supply:          ON  [max {s.ShortSupplyMaxItems} items/shop]");

            sb.AppendLine();
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the display name for an item ID. Falls back to "Item #N" when
        /// itemNames is null or the ID is not found.
        /// </summary>
        private static string GetItemName(int id, IReadOnlyDictionary<int, string>? itemNames)
        {
            if (itemNames != null && itemNames.TryGetValue(id, out string? name))
                return name;
            return $"Item #{id}";
        }

        /// <summary>
        /// Strips the leading ;# prefix and optional ID prefix from an AbilityGemsRow
        /// Comment field, returning just the ability name.
        ///
        /// Input examples:
        ///   ";# 101 - Auto-Regen"  → "Auto-Regen"
        ///   "Auto-Regen"           → "Auto-Regen"
        ///   null / whitespace      → null
        /// </summary>
        private static string? CleanAbilityComment(string? comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
                return null;

            // Strip leading ;# characters and whitespace
            string cleaned = comment.TrimStart(';', '#', ' ');

            // Strip leading numeric ID prefix like "101 - " (at most 5 chars before " - ")
            int dashIdx = cleaned.IndexOf(" - ", StringComparison.Ordinal);
            if (dashIdx >= 0 && dashIdx <= 5)
                cleaned = cleaned[(dashIdx + 3)..];

            return cleaned.Trim().Length > 0 ? cleaned.Trim() : null;
        }

        private static void AppendSectionHeader(StringBuilder sb, string title)
        {
            int fillLen = Math.Max(0, SectionWidth - title.Length - 4); // 4 = "══ " + " "
            string fill = new string('═', fillLen);
            sb.AppendLine($"══ {title} {fill}");
        }

        private static string Center(string text, int width)
        {
            int pad = Math.Max(0, (width - text.Length) / 2);
            return new string(' ', pad) + text;
        }
    }
}