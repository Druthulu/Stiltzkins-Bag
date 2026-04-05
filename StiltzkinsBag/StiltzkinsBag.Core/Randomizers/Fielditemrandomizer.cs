using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StiltzkinsBag.Core.Models;
using StiltzkinsBag.Core.Parsing;
using StiltzkinsBag.Models;
using StiltzkinsBag.Parsing;
using StiltzkinsBag.Randomizers;

namespace StiltzkinsBag.Randomizers
{
    /// <summary>
    /// Summary of a completed field item randomization run.
    /// </summary>
    public sealed class FieldRandomizationResult
    {
        /// <summary>Total field files scanned (distinct field names, language-deduplicated).</summary>
        public int FieldsScanned { get; init; }
        /// <summary>Fields that contained at least one patchable location.</summary>
        public int FieldsPatched { get; init; }
        /// <summary>Total TreasureItem locations patched across all fields.</summary>
        public int TreasureLocationsPatched { get; init; }
        /// <summary>Total DirectItem locations patched across all fields.</summary>
        public int DirectLocationsPatched { get; init; }
        /// <summary>Total TextSync locations patched across all fields.</summary>
        public int TextSyncLocationsPatched { get; init; }
        /// <summary>
        /// Number of Stiltzkin scripts excluded from the field item pool.
        /// Zero when <see cref="StiltzkinMode"/> is Off or IncludeInFieldPool.
        /// </summary>
        public int StiltzkinScriptsExcluded { get; init; }
        /// <summary>
        /// Field files skipped during Pass 1 because FieldParser threw ArgumentException
        /// (file too small / unexpected header). These files are excluded from both
        /// scanning and patching. Populated in debug builds via Debug.WriteLine.
        /// </summary>
        public IReadOnlyList<string> SkippedFields { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Scans all FF9 field script files for item/card/gil locations, shuffles them
    /// using <see cref="ItemRemapper"/>, and writes patched .bytes files to the
    /// mod output folder for all 7 language variants.
    ///
    /// ── Two-pass design ─────────────────────────────────────────────────────
    ///
    /// Pass 1 — Scan:
    ///   For each of the ~817 distinct field names found in the archive, extract
    ///   the 'us' language bytes (byte-identical across all 7 languages), run
    ///   <see cref="FieldParser.FindItemLocations"/>, and accumulate a global
    ///   treasure pool (TreasureItem values) and direct item pool (DirectItem values).
    ///
    /// Pass 2 — Shuffle and patch:
    ///   Build <see cref="ItemRemapTable"/> instances via <see cref="ItemRemapper"/>:
    ///     - treasureTable: full cross-shuffle of items, cards, and gil encodings
    ///     - directTable:   item-ID-only shuffle for AddItem constant calls (chests)
    ///   For each field, apply patches and write patched bytes to all 7 language
    ///   output paths under the mod output folder.
    ///
    /// ── Stiltzkin exclusion ──────────────────────────────────────────────────
    ///
    /// When <see cref="StiltzkinMode"/> is <c>Shuffle</c> or <c>Recommended</c>,
    /// Stiltzkin scripts are excluded from the field item pool before Pass 1.
    /// StiltzkinRandomizer handles those scripts independently.
    ///
    /// When <see cref="StiltzkinMode"/> is <c>IncludeInFieldPool</c>, Stiltzkin
    /// scripts remain in the normal field pool and are treated like any other field.
    ///
    /// When <see cref="StiltzkinMode"/> is <c>Off</c>, Stiltzkin scripts remain in
    /// the pool but are not patched by StiltzkinRandomizer — they pass through
    /// FieldItemRandomizer unchanged (vanilla items stay, positions unchanged).
    ///
    /// ── Language handling ────────────────────────────────────────────────────
    ///
    /// Field scripts are byte-identical across all 7 languages (es, fr, gr, it,
    /// jp, uk, us) — opcodes and byte offsets are the same; only AT_TEXT string IDs
    /// differ (which live in a separate asset). We therefore:
    ///   1. Extract and scan only the 'us' variant.
    ///   2. Apply patches to the 'us' bytes.
    ///   3. Write the same patched bytes to all 7 language output paths.
    ///
    /// ── TextSync resolution rule ─────────────────────────────────────────────
    ///
    /// A TextSync location (SetTextVariable(0, X)) may be paired with either a
    /// TreasureItem or a DirectItem location in the same field. To apply the
    /// correct table:
    ///   - X >= 512:  always treasureTable (cards/gil only exist in treasure system)
    ///   - X &lt;512, and X appears as a TreasureItem value in this field:  treasureTable
    ///   - X &lt;512, and X appears only as a DirectItem value in this field: directTable
    ///   - X in neither table: passthrough (ItemRemapTable.Remap returns X unchanged)
    ///
    /// ── Mod stack resolution ─────────────────────────────────────────────────
    ///
    /// File resolution order for each field (mirrors ModSourceResolver for enemies):
    ///   1. For each active mod (highest priority first):
    ///      a. Raw .bytes file at the correct path inside the mod folder.
    ///      b. p0data7.bin archive inside the mod's StreamingAssets folder.
    ///   2. Vanilla p0data7.bin in FINAL FANTASY IX_Data\StreamingAssets\.
    ///
    /// ── Output paths ─────────────────────────────────────────────────────────
    ///
    /// {modOutputRoot}\StreamingAssets\assets\resources\commonasset\eventengine\
    ///   eventbinary\field\{lang}\{fieldFileName}
    ///
    /// where {fieldFileName} is the archive name with ".bytes" appended
    /// (e.g. evt_alex1_at_house_2.eb.bytes).
    /// </summary>
    public sealed class FieldItemRandomizer
    {
        // ── Constants ──────────────────────────────────────────────────────────

        private const string FieldArchiveName = "p0data7.bin";

        private const string VanillaArchiveRelPath =
            @"StreamingAssets\" + FieldArchiveName;

        private const string FieldAssetPathBase =
            @"StreamingAssets\assets\resources\commonasset\eventengine\eventbinary\field";

        /// <summary>All 7 language codes used by FF9 field scripts.</summary>
        public static readonly IReadOnlyList<string> Languages =
            new[] { "es", "fr", "gr", "it", "jp", "uk", "us" };

        /// <summary>Language used for scanning (byte-identical to all others).</summary>
        private const string ScanLanguage = "us";

        // ── State ──────────────────────────────────────────────────────────────

        private readonly string _gameRoot;
        private readonly IReadOnlyList<string> _activeMods;
        private readonly string _modOutputRoot;

        // ── Factory ────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a randomizer by reading the active mod list from Memoria.ini.
        /// </summary>
        /// <param name="gameRoot">Full path to the FF9 game root directory.</param>
        /// <param name="modOutputRoot">Root of the mod output folder to write patched files to.</param>
        public static FieldItemRandomizer FromGameRoot(string gameRoot, string modOutputRoot)
        {
            ArgumentException.ThrowIfNullOrEmpty(gameRoot);
            ArgumentException.ThrowIfNullOrEmpty(modOutputRoot);

            var resolver = ModSourceResolver.FromGameRoot(gameRoot);
            return new FieldItemRandomizer(gameRoot, resolver.ActiveMods, modOutputRoot);
        }

        /// <summary>
        /// Creates a randomizer with an explicit mod list. Useful for testing.
        /// </summary>
        public static FieldItemRandomizer WithExplicitMods(
            string gameRoot,
            IEnumerable<string> modsHighestFirst,
            string modOutputRoot)
        {
            ArgumentException.ThrowIfNullOrEmpty(gameRoot);
            ArgumentNullException.ThrowIfNull(modsHighestFirst);
            ArgumentException.ThrowIfNullOrEmpty(modOutputRoot);

            return new FieldItemRandomizer(
                gameRoot, modsHighestFirst.ToList(), modOutputRoot);
        }

        private FieldItemRandomizer(
            string gameRoot,
            IReadOnlyList<string> activeMods,
            string modOutputRoot)
        {
            _gameRoot = gameRoot;
            _activeMods = activeMods;
            _modOutputRoot = modOutputRoot;
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Runs the full two-pass scan → shuffle → patch → write pipeline.
        /// Creates output directories as needed.
        /// </summary>
        /// <param name="rng">Seeded Random instance from SeedEngine.</param>
        /// <param name="settings">
        /// Active run settings. Controls Stiltzkin exclusion:
        /// <see cref="StiltzkinMode.Shuffle"/> and <see cref="StiltzkinMode.Recommended"/>
        /// exclude Stiltzkin scripts from the field item pool — StiltzkinRandomizer
        /// handles those scripts independently.
        /// <see cref="StiltzkinMode.IncludeInFieldPool"/> and <see cref="StiltzkinMode.Off"/>
        /// leave Stiltzkin scripts in the normal pool.
        /// </param>
        public FieldRandomizationResult Randomize(Random rng, Settings settings)
        {
            ArgumentNullException.ThrowIfNull(rng);
            ArgumentNullException.ThrowIfNull(settings);

            string vanillaArchive = Path.Combine(_gameRoot, VanillaArchiveRelPath);

            // ── Build Stiltzkin exclusion set ─────────────────────────────────
            //
            // When StiltzkinMode is Shuffle or Recommended, Stiltzkin scripts are
            // handled by StiltzkinRandomizer and must not be touched here.
            // The exclusion set uses the archive name form (without .bytes suffix),
            // matching the names returned by UnityArchiver.GetFileNames().
            var stiltzkinExclusions = BuildStiltzkinExclusionSet(settings.StiltzkinMode);

            // ── Enumerate distinct field file names from the vanilla archive ──
            List<string> fieldNames;
            using (var archive = UnityArchiver.Open(vanillaArchive))
            {
                fieldNames = archive.GetFileNames()
                    .Where(n => n.StartsWith("evt_", StringComparison.OrdinalIgnoreCase))
                    .Where(n => !stiltzkinExclusions.Contains(
                        n.EndsWith(".bytes", StringComparison.OrdinalIgnoreCase)
                            ? n[..^".bytes".Length]
                            : n))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            int stiltzkinExcluded = stiltzkinExclusions.Count;

            // ── Pass 1: Extract and scan all fields ────────────────────────────
            var bytesByName = new Dictionary<string, byte[]>(fieldNames.Count,
                                     StringComparer.OrdinalIgnoreCase);
            var locationsByName = new Dictionary<string, IReadOnlyList<FieldItemLocation>>(
                                     fieldNames.Count, StringComparer.OrdinalIgnoreCase);
            var skippedFields = new List<string>();

            foreach (string name in fieldNames)
            {
                byte[] bytes = ResolveFieldBytes(name, ScanLanguage, vanillaArchive);

                // DEFENSIVE SKIP: FieldParser.FindItemLocations throws ArgumentException
                // on field files that are smaller than the expected header size.
                // These are valid game files that simply contain no patchable content
                // (stub scripts, title/menu scripts, etc.). Skip and continue rather
                // than crashing the entire generation run.
                // Re-enable concern if: a field with known item content is being skipped.
                IReadOnlyList<FieldItemLocation> locations;
                try
                {
                    locations = FieldParser.FindItemLocations(bytes);
                }
                catch (ArgumentException ex)
                {
                    skippedFields.Add(name);
                    System.Diagnostics.Debug.WriteLine(
                        $"[FieldItemRandomizer] Skipped '{name}': {ex.Message}");
                    continue;
                }

                bytesByName[name] = bytes;
                locationsByName[name] = locations;
            }

            // ── Build shuffle pools ─────────────────────────────────────────────
            var (treasurePool, directPool) = BuildPools(locationsByName);

            // ── Build remap tables ─────────────────────────────────────────────
            ItemRemapTable treasureTable = ItemRemapper.ShuffleTreasurePool(treasurePool, rng);
            ItemRemapTable directTable = ItemRemapper.ShuffleItemPool(directPool, rng);

            // ── Pass 2: Patch and write ─────────────────────────────────────────
            int fieldsPatched = 0;
            int treasureLocationsPatched = 0;
            int directLocationsPatched = 0;
            int textSyncLocationsPatched = 0;

            foreach (string name in fieldNames)
            {
                IReadOnlyList<FieldItemLocation> locations = locationsByName[name];
                if (locations.Count == 0) continue;

                var patches = BuildPatches(locations, treasureTable, directTable).ToList();
                if (patches.Count == 0) continue;

                byte[] patched = FieldParser.ApplyPatches(bytesByName[name], patches);
                fieldsPatched++;

                // Count by kind
                foreach (var (loc, _) in patches)
                {
                    switch (loc.LocationKind)
                    {
                        case FieldLocationKind.TreasureItem: treasureLocationsPatched++; break;
                        case FieldLocationKind.DirectItem: directLocationsPatched++; break;
                        case FieldLocationKind.TextSync: textSyncLocationsPatched++; break;
                    }
                }

                // Write patched bytes to all 7 language output paths.
                // Archive name (e.g. "evt_alex1_at_house_2.eb") → output file adds ".bytes"
                string outputFileName = name.EndsWith(".bytes", StringComparison.OrdinalIgnoreCase)
                    ? name
                    : name + ".bytes";

                foreach (string lang in Languages)
                {
                    string outputPath = GetOutputPath(_modOutputRoot, lang, outputFileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                    File.WriteAllBytes(outputPath, patched);
                }
            }

            return new FieldRandomizationResult
            {
                FieldsScanned = fieldNames.Count,
                FieldsPatched = fieldsPatched,
                TreasureLocationsPatched = treasureLocationsPatched,
                DirectLocationsPatched = directLocationsPatched,
                TextSyncLocationsPatched = textSyncLocationsPatched,
                StiltzkinScriptsExcluded = stiltzkinExcluded,
                SkippedFields = skippedFields.AsReadOnly(),
            };
        }

        // ── Stiltzkin exclusion helper ─────────────────────────────────────────

        /// <summary>
        /// Builds the set of archive field names (without .bytes suffix) that
        /// FieldItemRandomizer must exclude when <paramref name="mode"/> is
        /// <see cref="StiltzkinMode.Shuffle"/> or <see cref="StiltzkinMode.Recommended"/>.
        ///
        /// Returns an empty set for <see cref="StiltzkinMode.Off"/> and
        /// <see cref="StiltzkinMode.IncludeInFieldPool"/> — those modes leave
        /// Stiltzkin scripts in the normal field pool.
        /// </summary>
        public static HashSet<string> BuildStiltzkinExclusionSet(StiltzkinMode mode)
        {
            if (mode != StiltzkinMode.Shuffle && mode != StiltzkinMode.Recommended)
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var exclusions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string[] scripts in VanillaObtainabilityData.StiltzkinVisitLocations.Values)
                foreach (string script in scripts)
                    exclusions.Add(script); // already in .eb form, no .bytes suffix

            return exclusions;
        }

        // ── Internal static methods (testable without file system) ─────────────

        /// <summary>
        /// Scans a collection of (fieldFileName, bytes) pairs and returns item
        /// locations grouped by field file name.
        /// </summary>
        public static Dictionary<string, IReadOnlyList<FieldItemLocation>> ScanAll(
            IEnumerable<(string FileName, byte[] Bytes)> fields)
        {
            ArgumentNullException.ThrowIfNull(fields);

            var result = new Dictionary<string, IReadOnlyList<FieldItemLocation>>(
                StringComparer.OrdinalIgnoreCase);

            foreach (var (name, bytes) in fields)
                result[name] = FieldParser.FindItemLocations(bytes);

            return result;
        }

        /// <summary>
        /// Extracts the global treasure pool and direct item pool from a set of
        /// scanned field locations.
        ///
        /// Treasure pool: all distinct TreasureItem values (items, cards, gil encodings).
        /// Direct pool:   all distinct DirectItem values (item IDs only, &lt; 512).
        /// </summary>
        public static (List<int> TreasurePool, List<int> DirectPool) BuildPools(
            IReadOnlyDictionary<string, IReadOnlyList<FieldItemLocation>> locationsByFile)
        {
            ArgumentNullException.ThrowIfNull(locationsByFile);

            var treasurePool = new HashSet<int>();
            var directPool = new HashSet<int>();

            foreach (var locations in locationsByFile.Values)
            {
                foreach (var loc in locations)
                {
                    switch (loc.LocationKind)
                    {
                        case FieldLocationKind.TreasureItem:
                            treasurePool.Add(loc.CurrentValue);
                            break;
                        case FieldLocationKind.DirectItem:
                            if (loc.CurrentValue < 512)
                                directPool.Add(loc.CurrentValue);
                            break;
                    }
                }
            }

            return (treasurePool.OrderBy(v => v).ToList(),
                    directPool.OrderBy(v => v).ToList());
        }

        /// <summary>
        /// Produces the patch list for a single field's locations using both
        /// remap tables.
        ///
        /// TextSync resolution rule:
        ///   - value >= 512            → treasureTable (cards/gil only in treasure system)
        ///   - value &lt; 512 and is a TreasureItem value in these locations → treasureTable
        ///   - value &lt; 512 otherwise → directTable
        ///
        /// A patch is only emitted when the remapped value differs from the current
        /// value, so this method is safe to call with passthrough tables.
        /// </summary>
        public static IEnumerable<(FieldItemLocation Location, int NewValue)> BuildPatches(
            IReadOnlyList<FieldItemLocation> locations,
            ItemRemapTable treasureTable,
            ItemRemapTable directTable)
        {
            ArgumentNullException.ThrowIfNull(locations);
            ArgumentNullException.ThrowIfNull(treasureTable);
            ArgumentNullException.ThrowIfNull(directTable);

            // Build a lookup set of TreasureItem values in this field for TextSync resolution
            var treasureValues = locations
                .Where(l => l.LocationKind == FieldLocationKind.TreasureItem)
                .Select(l => l.CurrentValue)
                .ToHashSet();

            foreach (var loc in locations)
            {
                int newValue = loc.LocationKind switch
                {
                    FieldLocationKind.TreasureItem =>
                        treasureTable.Remap(loc.CurrentValue),

                    FieldLocationKind.DirectItem =>
                        directTable.Remap(loc.CurrentValue),

                    FieldLocationKind.TextSync =>
                        ResolveTextSync(loc.CurrentValue, treasureValues,
                                        treasureTable, directTable),

                    _ => loc.CurrentValue, // DirectGil: not shuffled in Phase 4
                };

                // Only emit a patch if the value actually changes
                if (newValue != loc.CurrentValue)
                    yield return (loc, newValue);
            }
        }

        /// <summary>
        /// Constructs the output path for a field file under the mod output root.
        /// </summary>
        /// <param name="modOutputRoot">Root of the mod output folder.</param>
        /// <param name="language">One of the 7 language codes (es, fr, gr, it, jp, uk, us).</param>
        /// <param name="fieldFileName">
        /// The field file name including extension, e.g. "evt_alex1_at_house_2.eb.bytes".
        /// </param>
        public static string GetOutputPath(
            string modOutputRoot, string language, string fieldFileName)
        {
            return Path.Combine(modOutputRoot, FieldAssetPathBase, language, fieldFileName);
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private static int ResolveTextSync(
            int value,
            HashSet<int> treasureValues,
            ItemRemapTable treasureTable,
            ItemRemapTable directTable)
        {
            if (value >= 512)
                return treasureTable.Remap(value);

            return treasureValues.Contains(value)
                ? treasureTable.Remap(value)
                : directTable.Remap(value);
        }

        /// <summary>
        /// Resolves the best available bytes for a field file in the given language,
        /// walking the mod stack before falling back to the vanilla archive.
        ///
        /// Resolution order (mirrors ModSourceResolver for enemies):
        ///   1. Raw .bytes file in mod folder
        ///   2. p0data7.bin archive in mod's StreamingAssets
        ///   3. Vanilla p0data7.bin archive
        /// </summary>
        private byte[] ResolveFieldBytes(string archiveName, string language, string vanillaArchive)
        {
            // Construct the relative path for this field + language.
            // archiveName from archive: "evt_alex1_at_house_2.eb" (no .bytes)
            // Raw file on disk: "evt_alex1_at_house_2.eb.bytes"
            string diskFileName = archiveName.EndsWith(".bytes",
                                        StringComparison.OrdinalIgnoreCase)
                                    ? archiveName
                                    : archiveName + ".bytes";

            string relPath = Path.Combine(FieldAssetPathBase, language, diskFileName);

            // 1. Walk mod stack
            foreach (string modFolder in _activeMods)
            {
                string modRoot = Path.Combine(_gameRoot, modFolder);
                if (!Directory.Exists(modRoot)) continue;

                // 1a. Raw .bytes file
                string rawPath = Path.Combine(modRoot, relPath);
                if (File.Exists(rawPath))
                    return File.ReadAllBytes(rawPath);

                // 1b. p0data7.bin archive in the mod
                string archivePath = Path.Combine(modRoot, "StreamingAssets", FieldArchiveName);
                if (File.Exists(archivePath))
                {
                    try
                    {
                        using var arc = UnityArchiver.Open(archivePath);
                        return arc.Extract(archiveName);
                    }
                    catch (FileNotFoundException) { }
                    catch { }
                }
            }

            // 2. Vanilla archive
            using var vanilla = UnityArchiver.Open(vanillaArchive);
            return vanilla.Extract(archiveName);
        }
    }
}