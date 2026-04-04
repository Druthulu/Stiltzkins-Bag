using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using StiltzkinsBag.Core.Models;
using StiltzkinsBag.Core.Output;
using StiltzkinsBag.Models;
using StiltzkinsBag.Models.Csv;
using StiltzkinsBag.Parsing;
using StiltzkinsBag.Randomizers;
using StiltzkinsBag.Core.Randomizers;
using StiltzkinsBag.Core.Models.Battle;

namespace StiltzkinsBag.Core
{
    /// <summary>
    /// Orchestrates the full randomization pipeline for a single generation run.
    ///
    /// ── Pipeline order (RNG call order is sacred) ────────────────────────────
    ///  1. Seed resolution
    ///  2. Enemy binary patches  (RemapDropsAndSteals, ShuffleBlueMagic, ShuffleCardDrops)
    ///  3. Field binary patches  (FieldItemRandomizer — builds its own remap tables)
    ///  4. Stiltzkin patches     (StiltzkinRandomizer)
    ///  5. Character pipeline    (CharacterRandomizer: stats→speciality→abilities→equipment)
    ///  6. Ability AP            (AbilityApRandomizer — post-process on ability tables)
    ///  7. Starting items        (InitialItemsRandomizer)
    ///  8. Gear stats            (GearStatRandomizer)
    ///  9. Shops                 (ShopRandomizer incl. BadEconomy)
    /// 10. Synthesis             (SynthesisRandomizer)
    /// 11. Ability gems          (AbilityGemsRandomizer)
    /// 12. Tetramaster           (TetraMasterRandomizer)
    /// 13. Legendary rarity      (RecommendedLogicEngine.EnforceLegendaryRarity — consumes RNG)
    ///     Note: CorrectEquipmentCoherence and EnforceSynthesisReachability consume no RNG.
    ///
    /// ── IMPLEMENTATION REQUIRED (two stubs below) ────────────────────────────
    ///  • <see cref="LoadEnemyFiles"/> — needs EnemyFile / BattleItemScanner API
    ///  • <see cref="ExtractTetraMasterData"/> — needs TetraMasterFile read methods
    ///  • <see cref="WireEnforceSynthesisReachability"/> — needs VanillaItemCatalog.Build()
    ///
    /// ── Game data directory structure ────────────────────────────────────────
    ///   StreamingAssets/Data/Characters/          BaseStats, CharacterParameters,
    ///                                             CommandSets, DefaultEquipment
    ///   StreamingAssets/Data/Characters/Abilities/ AbilityGems + per-character CSVs
    ///   StreamingAssets/Data/Items/               Items, ShopItems, Stats, Synthesis,
    ///                                             Weapons, Armors, InitialItems
    /// </summary>
    public static class RandomizerEngine
    {
        // ── Game path helpers ─────────────────────────────────────────────────

        private static string StreamingAssetsPath(string gamePath) =>
            Path.Combine(gamePath, "StreamingAssets");

        private static string DataPath(string gamePath) =>
            Path.Combine(StreamingAssetsPath(gamePath), "Data");

        /// <summary>
        /// Path to a CSV directly in Data/Characters/
        /// (BaseStats, CharacterParameters, CommandSets, DefaultEquipment, AbilityFeatures.txt).
        /// </summary>
        private static string CharsCsvPath(string gamePath, string fileName) =>
            Path.Combine(DataPath(gamePath), "Characters", fileName);

        /// <summary>
        /// Path to a CSV in Data/Characters/Abilities/
        /// (AbilityGems.csv and all per-character ability CSVs: Zidane.csv, Vivi.csv, etc.).
        /// </summary>
        private static string AbilitiesCsvPath(string gamePath, string fileName) =>
            Path.Combine(DataPath(gamePath), "Characters", "Abilities", fileName);

        /// <summary>
        /// Path to a CSV in Data/Items/
        /// (Items, ShopItems, Stats, Synthesis, Weapons, Armors, InitialItems).
        /// </summary>
        private static string ItemsCsvPath(string gamePath, string fileName) =>
            Path.Combine(DataPath(gamePath), "Items", fileName);

        private static string P0Data7Path(string gamePath) =>
            Path.Combine(StreamingAssetsPath(gamePath), "p0data7.bin");

        // Maps character ID (0–15) to its CSV filename in Data/Characters/Abilities/.
        // VERIFY: Guest IDs 8–15 mapped to Marcus/Beatrix/Blank/Cinna variants —
        // confirm against CharacterParameters.csv DefaultRow values if abilities
        // are writing to wrong guest characters.
        private static readonly (int CharId, string CsvFileName)[] CharAbilityCsvMap =
        {
            (0,  "Zidane.csv"),   (1,  "Vivi.csv"),      (2,  "Garnet.csv"),   (3,  "Steiner.csv"),
            (4,  "Freya.csv"),    (5,  "Quina.csv"),      (6,  "Eiko.csv"),     (7,  "Amarant.csv"),
            (8,  "Marcus1.csv"),  (9,  "Beatrix1.csv"),   (10, "Blank1.csv"),   (11, "Cinna1.csv"),
            (12, "Marcus2.csv"),  (13, "Beatrix2.csv"),   (14, "Blank2.csv"),   (15, "Cinna2.csv"),
        };

        // ── Public entry point ────────────────────────────────────────────────

        /// <summary>
        /// Executes the full randomization pipeline asynchronously.
        /// Reports fractional progress and human-readable status messages via <paramref name="progress"/>.
        /// </summary>
        /// <param name="settings">
        /// Fully populated Settings with SeedString and GamePath set.
        /// SeedInt will be computed and written back by this method.
        /// </param>
        /// <param name="progress">
        /// Optional progress sink. Reports (fraction 0.0–1.0, status string).
        /// </param>
        /// <param name="ct">Optional cancellation token.</param>
        public static async Task<GenerationResult> RunAsync(
            Settings settings,
            IProgress<(double fraction, string message)>? progress = null,
            CancellationToken ct = default)
        {
            try
            {
                return await Task.Run(() => RunPipeline(settings, progress, ct), ct)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return GenerationResult.Failed("Generation cancelled by user.");
            }
            catch (Exception ex)
            {
                return GenerationResult.Failed($"Unexpected error: {ex.Message}");
            }
        }

        // ── Pipeline ──────────────────────────────────────────────────────────

        private static GenerationResult RunPipeline(
            Settings settings,
            IProgress<(double, string)>? progress,
            CancellationToken ct)
        {
            string gamePath = settings.GamePath;

            // ── Step 1: Seed resolution ───────────────────────────────────────────
            Report(progress, 0.02, "Resolving seed...");
            int seedInt = SeedEngine.Resolve(settings.SeedString);
            settings.SeedInt = seedInt;
            Random rng = SeedEngine.CreateRandom(seedInt);

            // ── Step 2: Set up mod output folder ─────────────────────────────────
            Report(progress, 0.04, "Preparing output folder...");
            var modWriter = new ModOutputWriter(gamePath, seedInt);
            modWriter.EnsureCleanSeedFolder();
            string seedFolder = modWriter.SeedFolderPath;
            ct.ThrowIfCancellationRequested();

            // ── Step 3: Read all game CSVs ────────────────────────────────────────
            Report(progress, 0.07, "Reading game data...");

            // Characters/ (top-level)
            var baseStatsParsed = Read<BaseStatsRow, BaseStatsRowMap>(CharsCsvPath(gamePath, "BaseStats.csv"));
            var charParamsParsed = Read<CharacterParametersRow, CharacterParametersRowMap>(CharsCsvPath(gamePath, "CharacterParameters.csv"));
            var commandSetsParsed = Read<CommandSetsRow, CommandSetsRowMap>(CharsCsvPath(gamePath, "CommandSets.csv"));
            var defaultEquipParsed = Read<DefaultEquipmentRow, DefaultEquipmentRowMap>(CharsCsvPath(gamePath, "DefaultEquipment.csv"));

            // Characters/Abilities/
            var gemsParsed = Read<AbilityGemsRow, AbilityGemsRowMap>(AbilitiesCsvPath(gamePath, "AbilityGems.csv"));
            var abilityParsedByCharId = ReadAbilityTables(gamePath);

            // AbilityFeatures.txt lives directly in Characters/
            string abilityFeaturesText = File.ReadAllText(
                AbilitiesCsvPath(gamePath, "AbilityFeatures.txt"), System.Text.Encoding.UTF8);

            // Items/
            var itemsParsed = Read<ItemsRow, ItemsRowMap>(ItemsCsvPath(gamePath, "Items.csv"));
            var shopsParsed = Read<ShopItemsRow, ShopItemsRowMap>(ItemsCsvPath(gamePath, "ShopItems.csv"));
            var synthesisParsed = Read<SynthesisRow, SynthesisRowMap>(ItemsCsvPath(gamePath, "Synthesis.csv"));
            var statsParsed = Read<StatsRow, StatsRowMap>(ItemsCsvPath(gamePath, "Stats.csv"));
            var initialItemsParsed = Read<InitialItemsRow, InitialItemsRowMap>(ItemsCsvPath(gamePath, "InitialItems.csv"));

            ct.ThrowIfCancellationRequested();

            // ── Step 4: Build item name map for spoiler log ───────────────────────
            var itemNames = BuildItemNameMap(itemsParsed);

            // ── Step 5: Item remap table ──────────────────────────────────────────
            // RULE: ItemRemapTable must be built BEFORE any binary patcher runs.
            // FieldItemRandomizer builds its own internal remap tables from field scan.
            // EnemyRandomizer uses the direct-item remap table built here.
            //
            // Current implementation: Passthrough (no global item ID shuffle).
            ItemRemapTable itemRemapTable = ItemRemapTable.Passthrough();

            // ── Step 6: Enemy binary patches ─────────────────────────────────────
            Report(progress, 0.13, "Randomizing enemy data...");
            bool enemyDropsRemapped = false;
            bool enemyBlueMagicShuffled = false;
            bool enemyCardDropsShuffled = false;

            if (settings.RandomizeEnemies)
            {
                var enemyFiles = LoadEnemyFiles(gamePath);

                if (settings.RandomizeItemDrops || settings.RandomizeItemSteals)
                {
                    EnemyRandomizer.RemapDropsAndSteals(enemyFiles, itemRemapTable);
                    enemyDropsRemapped = true;
                }
                if (settings.RandomizeBlueMagic)
                {
                    EnemyRandomizer.ShuffleBlueMagic(enemyFiles, rng);
                    enemyBlueMagicShuffled = true;
                }
                if (settings.RandomizeCardDrops)
                {
                    EnemyRandomizer.ShuffleCardDrops(enemyFiles, rng);
                    enemyCardDropsShuffled = true;
                }

                foreach (var file in enemyFiles)
                {
                    string relPath = ("StreamingAssets/" + file.SourcePath).Replace('\\', '/');
                    modWriter.WritePatchedBinaryFile(relPath, file.ToBytes());
                }
            }
            ct.ThrowIfCancellationRequested();

            // ── Step 7: Field binary patches ─────────────────────────────────────
            Report(progress, 0.20, "Randomizing treasure chests...");
            FieldRandomizationResult? fieldResult = null;

            if (settings.RandomizeTreasureChests)
            {
                var fieldRand = FieldItemRandomizer.FromGameRoot(gamePath, seedFolder);
                fieldResult = fieldRand.Randomize(rng, settings);
            }
            ct.ThrowIfCancellationRequested();

            // ── Step 8: Stiltzkin patches ─────────────────────────────────────────
            Report(progress, 0.27, "Randomizing Stiltzkin packages...");
            StiltzkinRandomizerResult stiltzkinResult = StiltzkinRandomizer.Randomize(
                P0Data7Path(gamePath),
                settings,
                rng,
                itemRemapTable,
                seedFolder);
            ct.ThrowIfCancellationRequested();

            // ── Step 9: Character pipeline ────────────────────────────────────────
            Report(progress, 0.33, "Randomizing characters...");
            CharacterRandomizerResult? charResult = null;
            Dictionary<int, IReadOnlyList<CharacterAbilityRow>> finalAbilityTables =
                abilityParsedByCharId.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (IReadOnlyList<CharacterAbilityRow>)kvp.Value.Rows.AsReadOnly());

            if (settings.RandomizeCharacters)
            {
                var mutableAbilityTables = abilityParsedByCharId.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Rows.ToList());

                var charRand = new CharacterRandomizer(
                    rng, settings,
                    baseStatsParsed.Rows,
                    charParamsParsed.Rows,
                    commandSetsParsed.Rows,
                    mutableAbilityTables,
                    abilityFeaturesText);

                charResult = charRand.Randomize();

                var apRand = new AbilityApRandomizer(rng, settings);
                var mutableFinalTables = charResult.AbilityTables.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToList());
                var apResult = apRand.Randomize(mutableFinalTables);

                finalAbilityTables = apResult.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (IReadOnlyList<CharacterAbilityRow>)kvp.Value.AsReadOnly());
            }
            ct.ThrowIfCancellationRequested();

            // ── Step 10: Starting items ───────────────────────────────────────────
            Report(progress, 0.38, "Randomizing starting items...");
            IReadOnlyList<InitialItemsRow>? initialItemsResult = null;

            if (settings.RandomizeInitialItems)
            {
                var initRand = new InitialItemsRandomizer(rng, settings, itemsParsed.Rows);
                initialItemsResult = initRand.Randomize(initialItemsParsed.Rows.ToList());
            }
            ct.ThrowIfCancellationRequested();

            // ── Step 11: Gear stat bonuses ────────────────────────────────────────
            Report(progress, 0.43, "Randomizing gear stats...");
            IReadOnlyList<StatsRow>? gearStatsResult = null;

            if (settings.RandomizeGearStatBonuses)
            {
                var gearRand = new GearStatRandomizer(rng, settings);
                gearStatsResult = gearRand.Randomize(statsParsed.Rows.ToList());
            }
            ct.ThrowIfCancellationRequested();

            // ── Step 12: Shops ────────────────────────────────────────────────────
            Report(progress, 0.49, "Randomizing shops...");
            ShopRandomizerResult? shopResult = null;

            if (settings.RandomizeShops)
            {
                var shopRand = new ShopRandomizer(rng, settings);
                shopResult = shopRand.Randomize(shopsParsed.Rows.ToList(), itemsParsed.Rows.ToList());
            }
            ct.ThrowIfCancellationRequested();

            // ── Step 13: Synthesis ────────────────────────────────────────────────
            Report(progress, 0.55, "Randomizing synthesis...");
            SynthesisRandomizerResult? synthesisResult = null;

            if (settings.RandomizeSynthesis)
            {
                var synthRand = new SynthesisRandomizer(rng, settings);
                synthesisResult = synthRand.Randomize(
                    synthesisParsed.Rows.ToList(),
                    itemsParsed.Rows.ToList(),
                    statsParsed.Rows.ToList());
            }
            ct.ThrowIfCancellationRequested();

            // ── Step 14: Ability gems ─────────────────────────────────────────────
            Report(progress, 0.60, "Randomizing ability gems...");
            IReadOnlyList<AbilityGemsRow>? gemsResult = null;

            if (settings.RandomizeAbilityGems)
            {
                var gemsRand = new AbilityGemsRandomizer(rng, settings);
                gemsResult = gemsRand.Randomize(gemsParsed.Rows.ToList());
            }
            ct.ThrowIfCancellationRequested();

            // ── Step 15: Tetramaster cards ────────────────────────────────────────
            Report(progress, 0.65, "Randomizing Tetramaster...");
            bool tetraMasterRan = false;

            if (settings.RandomizeTetraMaster)
            {
                var (cardStatsBytes, cardSetsBytes, npcDecksBytes, cardNamesByLang) =
                    ExtractTetraMasterData(gamePath);

                var tmRand = new TetraMasterRandomizer(rng, settings);
                var tmResult = tmRand.Randomize(
                    cardStatsBytes, cardSetsBytes, npcDecksBytes, cardNamesByLang);

                WriteTetraMasterOutput(modWriter, tmResult, settings);
                tetraMasterRan = true;
            }
            ct.ThrowIfCancellationRequested();

            // ── Step 16: Recommended Logic Engine ────────────────────────────────
            // RULE: EnforceLegendaryRarity DOES consume RNG — must run in pipeline order.
            // RULE: Entire block skipped in Chaos mode — zero RNG calls consumed.
            if (settings.Mode == RandomizerMode.Recommended)
            {
                Report(progress, 0.70, "Applying coherence corrections...");

                // 16a — Equipment coherence (no RNG)
                if (charResult != null && settings.RandomizeEquipment)
                {
                    var correctedParams = RecommendedLogicEngine.CorrectEquipmentCoherence(
                        charResult.CharacterParameters,
                        charResult.SlotAssignment,
                        defaultEquipParsed.Rows);

                    charResult = new CharacterRandomizerResult(
                        charResult.BaseStats, correctedParams,
                        charResult.CommandSetRows, charResult.SlotAssignment,
                        charResult.AbilityTables, charResult.AbilityFeaturesText);
                }

                // 16b — Legendary rarity enforcement (consumes RNG)
                if (shopResult != null && settings.RandomizeShops)
                {
                    var replacementPool = itemsParsed.Rows
                        .Where(i => i.Id >= 236 && i.Id <= 253)
                        .Select(i => i.Id)
                        .Except(LegendaryItemList.AllProtectedItemIds)
                        .ToList();

                    if (replacementPool.Count == 0)
                        replacementPool = new List<int> { 236 };

                    var correctedShops = RecommendedLogicEngine.EnforceLegendaryRarity(
                        shopResult.Shops,
                        LegendaryItemList.AllProtectedItemIds,
                        replacementPool,
                        rng);

                    shopResult = new ShopRandomizerResult(correctedShops.ToList(), shopResult.Items);
                }

                // 16c — Synthesis reachability (no RNG — requires VanillaItemCatalog)
                // TODO: Wire once VanillaItemCatalog.Build() is threaded through the engine.
                // DISABLED: WireEnforceSynthesisReachability(synthesisResult, rng);
            }
            ct.ThrowIfCancellationRequested();

            // ── Step 17: Write all modified CSVs ─────────────────────────────────
            Report(progress, 0.78, "Writing mod files...");
            WriteAllCsvOutputs(
                modWriter, charResult, finalAbilityTables,
                gearStatsResult, shopResult, synthesisResult,
                gemsResult, initialItemsResult,
                baseStatsParsed, charParamsParsed, commandSetsParsed,
                abilityParsedByCharId, statsParsed, shopsParsed,
                itemsParsed, synthesisParsed, gemsParsed, initialItemsParsed,
                settings);

            // AbilityFeatures.txt — write if speciality was randomized
            if (charResult != null && settings.RandomizeSpeciality)
            {
                string abFeatDir = modWriter.GetAbsolutePath(ModOutputWriter.AbilitiesRelPath);
                File.WriteAllText(
                    Path.Combine(abFeatDir, "AbilityFeatures.txt"),
                    charResult.AbilityFeaturesText,
                    System.Text.Encoding.UTF8);
            }
            ct.ThrowIfCancellationRequested();

            // ── Step 18: Spoiler log ──────────────────────────────────────────────
            Report(progress, 0.87, "Writing spoiler log...");
            var spoilerData = new SpoilerLogData
            {
                SeedInt = seedInt,
                Settings = settings,
                ItemNames = itemNames,
                CharacterResult = charResult,
                FieldResult = fieldResult,
                StiltzkinResult = stiltzkinResult,
                ShopResult = shopResult,
                SynthesisResult = synthesisResult,
                GemsResult = gemsResult,
                InitialItemsResult = initialItemsResult,
                EnemyDropsRemapped = enemyDropsRemapped,
                EnemyBlueMagicShuffled = enemyBlueMagicShuffled,
                EnemyCardDropsShuffled = enemyCardDropsShuffled,
                TetraMasterRandomizationApplied = tetraMasterRan,
            };
            SpoilerLogWriter.Write(seedFolder, spoilerData);

            // ── Step 19: Metadata files ───────────────────────────────────────────
            Report(progress, 0.93, "Writing metadata...");
            SettingsFileWriter.Write(seedFolder, seedInt, settings);
            ModDescriptionWriter.Write(seedFolder, seedInt, settings);
            ModMemoriaIniWriter.Write(seedFolder, settings);

            // ── Step 20: Update base Memoria.ini load order ───────────────────────
            Report(progress, 0.97, "Updating Memoria load order...");
            string memoriaIniPath = Path.Combine(gamePath, "Memoria.ini");
            MemoriaLoadOrder.InjectSeedFolder(memoriaIniPath, seedInt);

            Report(progress, 1.0, "Generation complete.");
            return GenerationResult.Succeeded(seedFolder);
        }

        // ── CSV output ────────────────────────────────────────────────────────

        private static void WriteAllCsvOutputs(
            ModOutputWriter modWriter,
            CharacterRandomizerResult? charResult,
            Dictionary<int, IReadOnlyList<CharacterAbilityRow>> finalAbilityTables,
            IReadOnlyList<StatsRow>? gearStatsResult,
            ShopRandomizerResult? shopResult,
            SynthesisRandomizerResult? synthesisResult,
            IReadOnlyList<AbilityGemsRow>? gemsResult,
            IReadOnlyList<InitialItemsRow>? initialItemsResult,
            ParsedCsv<BaseStatsRow> baseStatsParsed,
            ParsedCsv<CharacterParametersRow> charParamsParsed,
            ParsedCsv<CommandSetsRow> commandSetsParsed,
            Dictionary<int, ParsedCsv<CharacterAbilityRow>> abilityParsedByCharId,
            ParsedCsv<StatsRow> statsParsed,
            ParsedCsv<ShopItemsRow> shopsParsed,
            ParsedCsv<ItemsRow> itemsParsed,
            ParsedCsv<SynthesisRow> synthesisParsed,
            ParsedCsv<AbilityGemsRow> gemsParsed,
            ParsedCsv<InitialItemsRow> initialItemsParsed,
            Settings settings)
        {
            const string chars = ModOutputWriter.CharactersRelPath;
            const string abilities = ModOutputWriter.AbilitiesRelPath;
            const string items = ModOutputWriter.ItemsRelPath;

            // Characters (top-level CSVs)
            if (charResult != null)
            {
                UpdateRows(baseStatsParsed, charResult.BaseStats);
                modWriter.WriteModifiedCsv<BaseStatsRow, BaseStatsRowMap>(
                    $"{chars}/BaseStats.csv", baseStatsParsed);

                UpdateRows(charParamsParsed, charResult.CharacterParameters);
                modWriter.WriteModifiedCsv<CharacterParametersRow, CharacterParametersRowMap>(
                    $"{chars}/CharacterParameters.csv", charParamsParsed);

                // CommandSets — rows 0–7 rewritten; rows 8–19 pass through unchanged
                var modifiedSets = charResult.CommandSetRows;
                for (int i = 0; i < modifiedSets.Count && i < commandSetsParsed.Rows.Count; i++)
                    commandSetsParsed.Rows[i] = modifiedSets[i];
                modWriter.WriteModifiedCsv<CommandSetsRow, CommandSetsRowMap>(
                    $"{chars}/CommandSets.csv", commandSetsParsed);

                // Characters/Abilities/ — per-character ability CSVs
                foreach (var (charId, csvFileName) in CharAbilityCsvMap)
                {
                    if (!finalAbilityTables.TryGetValue(charId, out var rows)) continue;
                    if (!abilityParsedByCharId.TryGetValue(charId, out var parsed)) continue;

                    UpdateRows(parsed, rows);
                    modWriter.WriteModifiedCsv<CharacterAbilityRow, CharacterAbilityRowMap>(
                        $"{abilities}/{csvFileName}", parsed);
                }
            }

            // Ability gems — Characters/Abilities/AbilityGems.csv
            if (gemsResult != null)
            {
                UpdateRows(gemsParsed, gemsResult);
                modWriter.WriteModifiedCsv<AbilityGemsRow, AbilityGemsRowMap>(
                    $"{abilities}/AbilityGems.csv", gemsParsed);
            }

            // Gear stats
            if (gearStatsResult != null)
            {
                UpdateRows(statsParsed, gearStatsResult);
                modWriter.WriteModifiedCsv<StatsRow, StatsRowMap>(
                    $"{items}/Stats.csv", statsParsed);
            }

            // Shops
            if (shopResult != null)
            {
                UpdateRows(shopsParsed, shopResult.Shops);
                modWriter.WriteModifiedCsv<ShopItemsRow, ShopItemsRowMap>(
                    $"{items}/ShopItems.csv", shopsParsed);

                if (settings.BadEconomy)
                {
                    UpdateRows(itemsParsed, shopResult.Items);
                    modWriter.WriteModifiedCsv<ItemsRow, ItemsRowMap>(
                        $"{items}/Items.csv", itemsParsed);
                }
            }

            // Synthesis
            if (synthesisResult != null)
            {
                UpdateRows(synthesisParsed, synthesisResult.Recipes);
                modWriter.WriteModifiedCsv<SynthesisRow, SynthesisRowMap>(
                    $"{items}/Synthesis.csv", synthesisParsed);
            }

            // Initial items
            if (initialItemsResult != null)
            {
                UpdateRows(initialItemsParsed, initialItemsResult);
                modWriter.WriteModifiedCsv<InitialItemsRow, InitialItemsRowMap>(
                    $"{items}/InitialItems.csv", initialItemsParsed);
            }
        }

        // ── Stub methods — implement before shipping ──────────────────────────

        /// <summary>
        /// Loads all enemy stat files from the game's p0data2.bin archive,
        /// sorted by SourcePath for deterministic RNG consumption.
        ///
        /// IMPLEMENTATION REQUIRED: Use BattleItemScanner or EnemyFile static
        /// factory methods to extract EnemyFile instances from p0data2.bin.
        ///
        /// Example:
        ///   string archivePath = Path.Combine(StreamingAssetsPath(gamePath), "p0data2.bin");
        ///   return EnemyFile.LoadAll(archivePath)
        ///       .OrderBy(f => f.SourcePath, StringComparer.Ordinal)
        ///       .ToList();
        /// </summary>
        private static IReadOnlyList<EnemyFile> LoadEnemyFiles(string gamePath) =>
            throw new NotImplementedException(
                "LoadEnemyFiles: implement using BattleItemScanner or EnemyFile.LoadAll(). " +
                "See BattleItemScanner.cs for the p0data2.bin extraction pattern. " +
                "Sort by EnemyFile.SourcePath (Ordinal) for deterministic RNG order.");

        /// <summary>
        /// Extracts the three Tetramaster binary data byte arrays and per-language
        /// card name files from the game's resources.assets (or equivalent archive).
        ///
        /// IMPLEMENTATION REQUIRED: Use TetraMasterFile static read methods.
        /// See TetraMasterRandomizer.cs header for data source paths and byte sizes.
        ///
        /// Returns: (cardStats, cardSets, npcDecks, cardNamesByLang)
        /// </summary>
        private static (byte[] cardStats, byte[] cardSets, byte[] npcDecks,
                        Dictionary<string, byte[]> cardNamesByLang)
            ExtractTetraMasterData(string gamePath) =>
            throw new NotImplementedException(
                "ExtractTetraMasterData: implement using TetraMasterFile read methods. " +
                "See TetraMasterRandomizer.cs for data source paths and byte sizes.");

        /// <summary>
        /// Writes Tetramaster patched binary outputs to the mod output folder.
        /// TODO: Verify paths against Memoria's mod overlay structure for Tetramaster assets.
        /// </summary>
        private static void WriteTetraMasterOutput(
            ModOutputWriter modWriter,
            TetraMasterRandomizerResult tmResult,
            Settings settings)
        {
            const string CardDataPath = "StreamingAssets/assets/resources/cardgame/minigame_card_data_address";
            const string CardSetsPath = "StreamingAssets/assets/resources/cardgame/minigame_card_level_address";
            const string NpcDecksPath = "StreamingAssets/assets/resources/cardgame/minigame_stage_address";
            const string CardNamesBase = "StreamingAssets/assets/resources/text/{lang}/minista.mes";

            modWriter.WritePatchedBinaryFile(CardDataPath, tmResult.CardStats);
            modWriter.WritePatchedBinaryFile(CardSetsPath, tmResult.CardSets);
            modWriter.WritePatchedBinaryFile(NpcDecksPath, tmResult.NpcDecks);

            if (settings.ShuffleCardOrder && tmResult.CardNames != null)
            {
                foreach (var (lang, nameBytes) in tmResult.CardNames)
                {
                    modWriter.WritePatchedBinaryFile(
                        CardNamesBase.Replace("{lang}", lang), nameBytes);
                }
            }
        }

        // ── CSV read helpers ──────────────────────────────────────────────────

        private static ParsedCsv<T> Read<T, TMap>(string path)
            where TMap : ClassMap<T> =>
            MemoriaCsvParser.Read<T, TMap>(path);

        /// <summary>
        /// Reads all per-character ability CSVs from Characters/Abilities/.
        /// Returns a dictionary keyed by character ID (0–15).
        /// </summary>
        private static Dictionary<int, ParsedCsv<CharacterAbilityRow>> ReadAbilityTables(
            string gamePath)
        {
            var result = new Dictionary<int, ParsedCsv<CharacterAbilityRow>>();
            foreach (var (charId, csvFileName) in CharAbilityCsvMap)
            {
                string path = AbilitiesCsvPath(gamePath, csvFileName);
                result[charId] = MemoriaCsvParser.Read<CharacterAbilityRow, CharacterAbilityRowMap>(path);
            }
            return result;
        }

        /// <summary>
        /// Replaces all rows in <paramref name="parsed"/> with <paramref name="newRows"/>.
        /// Preserves all ParsedCsv metadata (comment block, inline comments, encoding, line ending).
        /// </summary>
        private static void UpdateRows<T>(ParsedCsv<T> parsed, IReadOnlyList<T> newRows)
        {
            parsed.Rows.Clear();
            parsed.Rows.AddRange(newRows);
        }

        /// <summary>
        /// Builds an item ID → display name dictionary from Items.csv inline comments.
        /// Comments are in the format ";# NNN - Item Name".
        /// Used by SpoilerLogWriter for human-readable item references.
        /// </summary>
        private static IReadOnlyDictionary<int, string> BuildItemNameMap(
            ParsedCsv<ItemsRow> itemsParsed)
        {
            var map = new Dictionary<int, string>(itemsParsed.Rows.Count);

            for (int i = 0; i < itemsParsed.Rows.Count && i < itemsParsed.InlineComments.Count; i++)
            {
                string? comment = itemsParsed.InlineComments[i];
                if (string.IsNullOrWhiteSpace(comment)) continue;

                string cleaned = comment.TrimStart(';', '#', ' ');
                int dashIdx = cleaned.IndexOf(" - ", StringComparison.Ordinal);
                if (dashIdx >= 0 && dashIdx <= 5)
                {
                    string name = cleaned[(dashIdx + 3)..].Trim();
                    if (!string.IsNullOrEmpty(name))
                        map[itemsParsed.Rows[i].Id] = name;
                }
            }

            return map;
        }

        private static void Report(
            IProgress<(double, string)>? progress,
            double fraction,
            string message) =>
            progress?.Report((fraction, message));
    }
}