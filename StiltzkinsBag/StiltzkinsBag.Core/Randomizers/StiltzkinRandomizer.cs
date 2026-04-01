// StiltzkinsBag.Core/Randomizers/StiltzkinRandomizer.cs
//
// Randomizes Stiltzkin's package items and/or prices across all 8 visits.
//
// ── Modes ────────────────────────────────────────────────────────────────────
//
//   Off              — no-op; returns StiltzkinRandomizerResult.NotRun
//   IncludeInFieldPool — no-op; FieldItemRandomizer already handled these scripts
//   Shuffle          — Fisher-Yates shuffle of all 24 vanilla items, 3 per visit
//   Recommended      — curated per-visit draws (Stiltzkin's Junk / Fun / Challenging)
//
//   StiltzkinPriceMode is independent of item mode — Off = vanilla prices.
//
// ── RNG call order (sacred) ──────────────────────────────────────────────────
//
//   1. Item selection (all 8 visits, in ascending price order):
//      - Shuffle:      23 Fisher-Yates calls (flat 24-item array), then 0 per visit
//      - Junk/Challenging: 3 × rng.Next(poolSize) per visit × 8 visits = 24 calls
//      - Fun:          3 × rng.Next(poolSize) per visit × 8 visits = 24 calls
//                      (1 Low + 1 Mid + 1 High draw per visit)
//   2. Price selection (all 8 visits, in ascending price order):
//      - Off:          0 calls
//      - Any active:   1 × rng.Next(min, max+1) per visit × 8 visits = 8 calls
//
//   Item draws always precede price draws, per pipeline convention.
//
// ── Cleyra pairing ───────────────────────────────────────────────────────────
//
//   444G maps to [EVT_CLEYRA3_ANTRION.eb, EVT_CLEYRA3_INN.eb]. One set of RNG
//   draws covers both scripts — ANTRION is the authoritative scan source for
//   Shuffle mode's vanilla item collection. Both scripts are patched independently
//   at their own byte offsets but with the same item IDs and price.
//
// ── Patching ─────────────────────────────────────────────────────────────────
//
//   Each package item appears as exactly 2 DirectItem locations (a pair ~22 bytes
//   apart). Both offsets must be patched with the new item ID.
//
//   Price is stored as SetTextVariable(0, price) — a TextSync location. Patching
//   this single location is sufficient: the game's NPC purchase system reads the
//   text variable for both display and Gil deduction.
//
//   ItemRemapTable is applied to all selected item IDs after RNG draws, before
//   writing to bytecode. Ensures item shuffle (if active) is reflected in packages.
//
//   Patched US bytes are written to all 7 language output paths (consistent with
//   FieldItemRandomizer's current approach — per-locale patching is a future upgrade).
//
// ── Scanning algorithm ───────────────────────────────────────────────────────
//
//   Confirmed by StiltzkinItemScanDiagnosticTests (2026-04-01):
//   1. Find price TextSync — last FieldItemLocation with Kind=TextSync, Value==price
//   2. Collect DirectItem locations with FileOffset > priceOffset AND 1 ≤ Value ≤ 255
//   3. Sort by offset — items arrive as pairs (same ID, ~22 bytes apart)
//   4. Take first 6 locations as 3 pairs; verify matching IDs per pair
//   5. These 3 pairs (6 locations total) are the package items

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StiltzkinsBag.Core.Models;
using StiltzkinsBag.Core.Parsing;
using StiltzkinsBag.Models;
using StiltzkinsBag.Parsing;

namespace StiltzkinsBag.Randomizers;

/// <summary>
/// Result of a <see cref="StiltzkinRandomizer"/> run.
/// </summary>
public sealed class StiltzkinRandomizerResult
{
    /// <summary>True if any randomization was performed.</summary>
    public bool WasRun { get; init; }

    /// <summary>Number of distinct Stiltzkin visits processed (max 8).</summary>
    public int VisitsProcessed { get; init; }

    /// <summary>Number of field scripts patched (one visit may patch 2 scripts).</summary>
    public int ScriptsPatched { get; init; }

    /// <summary>
    /// New package item IDs per vanilla price, after ItemRemapTable is applied.
    /// Key = vanilla package price; Value = array of 3 item IDs written to bytecode.
    /// Empty when <see cref="WasRun"/> is false.
    /// </summary>
    public IReadOnlyDictionary<int, int[]> NewItemsByVanillaPrice { get; init; } =
        new Dictionary<int, int[]>();

    /// <summary>
    /// New price per vanilla price (equals vanilla price when StiltzkinPriceMode.Off).
    /// Key = vanilla price; Value = new price written to bytecode.
    /// Empty when <see cref="WasRun"/> is false.
    /// </summary>
    public IReadOnlyDictionary<int, int> NewPriceByVanillaPrice { get; init; } =
        new Dictionary<int, int>();

    /// <summary>Returns a result indicating the randomizer did not run.</summary>
    public static StiltzkinRandomizerResult NotRun { get; } = new();
}

/// <summary>
/// Randomizes Stiltzkin's package items and/or prices.
/// </summary>
public static class StiltzkinRandomizer
{
    // ── Constants ─────────────────────────────────────────────────────────────

    private const string FieldAssetPathBase =
        @"StreamingAssets\assets\resources\commonasset\eventengine\eventbinary\field";

    private static readonly IReadOnlyList<string> Languages =
        new[] { "es", "fr", "gr", "it", "jp", "uk", "us" };

    private const int ItemsPerPackage = 3;

    // ── Public entry point ────────────────────────────────────────────────────

    /// <summary>
    /// Randomizes Stiltzkin's packages according to <paramref name="settings"/>.
    /// Returns <see cref="StiltzkinRandomizerResult.NotRun"/> when mode is
    /// <see cref="StiltzkinMode.Off"/> or <see cref="StiltzkinMode.IncludeInFieldPool"/>.
    /// </summary>
    /// <param name="p0data7Path">Full path to the vanilla p0data7.bin archive.</param>
    /// <param name="settings">Active run settings.</param>
    /// <param name="rng">Seeded Random instance from SeedEngine (shared, never new'd here).</param>
    /// <param name="itemRemapTable">
    /// ItemRemapTable from ItemRemapper. Applied to selected item IDs after RNG draws.
    /// Pass <see cref="ItemRemapTable.Passthrough()"/> when item shuffle is inactive.
    /// </param>
    /// <param name="modOutputRoot">Root of the mod output folder.</param>
    public static StiltzkinRandomizerResult Randomize(
        string p0data7Path,
        Settings settings,
        Random rng,
        ItemRemapTable itemRemapTable,
        string modOutputRoot)
    {
        ArgumentNullException.ThrowIfNull(p0data7Path);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(rng);
        ArgumentNullException.ThrowIfNull(itemRemapTable);
        ArgumentNullException.ThrowIfNull(modOutputRoot);

        // No-op modes — FieldItemRandomizer (IncludeInFieldPool) or vanilla (Off)
        if (settings.StiltzkinMode == StiltzkinMode.Off ||
            settings.StiltzkinMode == StiltzkinMode.IncludeInFieldPool)
            return StiltzkinRandomizerResult.NotRun;

        // ── Ordered visit list (ascending price = story order) ────────────────
        var visits = VanillaObtainabilityData.StiltzkinVisitLocations
            .OrderBy(kv => kv.Key)
            .ToList();

        // ── Scan pass — extract US bytes + parse locations for all scripts ─────
        // Keyed by script name (e.g. "EVT_BURMECIA_SQUARE_1.eb")
        var scanData = ScanAllScripts(p0data7Path, visits);

        // ── RNG step 1: Item selection ─────────────────────────────────────────
        // All item draws happen before any price draws (RNG call order rule).
        int[][] itemAssignments; // [visitIndex][0..2] = item IDs (pre-remap)

        if (settings.StiltzkinMode == StiltzkinMode.Shuffle)
            itemAssignments = BuildShuffleAssignments(visits, scanData, rng);
        else // Recommended
            itemAssignments = BuildRecommendedAssignments(settings, visits, rng);

        // Apply ItemRemapTable to all selected item IDs (post-draw, pre-write)
        for (int v = 0; v < itemAssignments.Length; v++)
            for (int i = 0; i < ItemsPerPackage; i++)
                itemAssignments[v][i] = itemRemapTable.Remap(itemAssignments[v][i]);

        // ── RNG step 2: Price selection ────────────────────────────────────────
        int[] priceAssignments = BuildPriceAssignments(
            settings.StiltzkinPriceMode, visits, rng);

        // ── Write patches ──────────────────────────────────────────────────────
        int scriptsPatched = WritePatches(
            p0data7Path, modOutputRoot, visits, scanData, itemAssignments, priceAssignments);

        // ── Build result ───────────────────────────────────────────────────────
        var newItemsByPrice = new Dictionary<int, int[]>();
        var newPriceByPrice = new Dictionary<int, int>();
        for (int v = 0; v < visits.Count; v++)
        {
            int vanillaPrice = visits[v].Key;
            newItemsByPrice[vanillaPrice] = itemAssignments[v];
            newPriceByPrice[vanillaPrice] = priceAssignments[v];
        }

        return new StiltzkinRandomizerResult
        {
            WasRun = true,
            VisitsProcessed = visits.Count,
            ScriptsPatched = scriptsPatched,
            NewItemsByVanillaPrice = newItemsByPrice,
            NewPriceByVanillaPrice = newPriceByPrice,
        };
    }

    // ── Public pure helpers (testable without file system) ────────────────────

    /// <summary>
    /// Finds the 3 Stiltzkin package item location pairs in a parsed field script.
    /// Returns an empty list if the price TextSync is not found or the pair structure
    /// is invalid.
    ///
    /// Each returned tuple contains the two <see cref="FieldItemLocation"/> objects
    /// for a single item (both must be patched) and the vanilla item ID.
    /// </summary>
    public static IReadOnlyList<(FieldItemLocation Loc1, FieldItemLocation Loc2, int VanillaId)>
        FindItemPairs(IReadOnlyList<FieldItemLocation> locations, int packagePrice)
    {
        ArgumentNullException.ThrowIfNull(locations);

        var priceLoc = FindPriceLocation(locations, packagePrice);
        if (priceLoc is null) return Array.Empty<(FieldItemLocation, FieldItemLocation, int)>();

        int priceOffset = priceLoc.FileOffset;

        var candidates = locations
            .Where(l => l.LocationKind == FieldLocationKind.DirectItem
                     && l.FileOffset > priceOffset
                     && l.CurrentValue >= 1
                     && l.CurrentValue <= 255)
            .OrderBy(l => l.FileOffset)
            .ToList();

        if (candidates.Count < ItemsPerPackage * 2)
            return Array.Empty<(FieldItemLocation, FieldItemLocation, int)>();

        var result = new List<(FieldItemLocation, FieldItemLocation, int)>(ItemsPerPackage);
        for (int i = 0; i < ItemsPerPackage; i++)
        {
            var loc1 = candidates[i * 2];
            var loc2 = candidates[i * 2 + 1];
            if (loc1.CurrentValue != loc2.CurrentValue)
                return Array.Empty<(FieldItemLocation, FieldItemLocation, int)>();
            result.Add((loc1, loc2, loc1.CurrentValue));
        }

        return result;
    }

    /// <summary>
    /// Finds the price TextSync location — the last
    /// <see cref="FieldLocationKind.TextSync"/> with <c>CurrentValue == packagePrice</c>.
    /// Returns null if not found.
    /// </summary>
    public static FieldItemLocation? FindPriceLocation(
        IReadOnlyList<FieldItemLocation> locations, int packagePrice)
    {
        ArgumentNullException.ThrowIfNull(locations);

        return locations
            .Where(l => l.LocationKind == FieldLocationKind.TextSync
                     && l.CurrentValue == packagePrice)
            .OrderBy(l => l.FileOffset)
            .LastOrDefault();
    }

    /// <summary>
    /// Draws one item at random from a pool using the shared RNG instance.
    /// One RNG call consumed.
    /// </summary>
    public static int DrawFromPool(IReadOnlyList<int> pool, Random rng)
    {
        ArgumentNullException.ThrowIfNull(pool);
        ArgumentNullException.ThrowIfNull(rng);
        if (pool.Count == 0) throw new ArgumentException("Pool must not be empty.", nameof(pool));

        return pool[rng.Next(pool.Count)];
    }

    /// <summary>
    /// Draws a new package price for the given <see cref="StiltzkinPriceMode"/>.
    /// Returns the vanilla price unchanged when mode is <see cref="StiltzkinPriceMode.Off"/>.
    /// One RNG call consumed for any active mode.
    /// </summary>
    public static int DrawPrice(StiltzkinPriceMode mode, int vanillaPrice, Random rng)
    {
        ArgumentNullException.ThrowIfNull(rng);

        return mode switch
        {
            StiltzkinPriceMode.ClearanceSale => rng.Next(
                VanillaObtainabilityData.StiltzkinClearanceSaleMin,
                VanillaObtainabilityData.StiltzkinClearanceSaleMax + 1),

            StiltzkinPriceMode.StiltzkinsMood => rng.Next(
                VanillaObtainabilityData.StiltzkinsMoodMin,
                VanillaObtainabilityData.StiltzkinsMoodMax + 1),

            StiltzkinPriceMode.HighwayRobbery => rng.Next(
                VanillaObtainabilityData.HighwayRobberyMin,
                VanillaObtainabilityData.HighwayRobberyMax + 1),

            _ => vanillaPrice, // Off — no RNG call
        };
    }

    /// <summary>
    /// Builds the output path for a Stiltzkin script file under the mod output root.
    /// </summary>
    public static string GetOutputPath(string modOutputRoot, string language, string scriptFileName)
    {
        return Path.Combine(modOutputRoot, FieldAssetPathBase, language, scriptFileName);
    }

    // ── Private: scan ─────────────────────────────────────────────────────────

    private sealed record ScriptScanData(
        IReadOnlyList<FieldItemLocation> Locations,
        IReadOnlyList<(FieldItemLocation Loc1, FieldItemLocation Loc2, int VanillaId)> ItemPairs,
        FieldItemLocation? PriceLocation);

    /// <summary>
    /// Extracts and scans the US-locale bytes for every Stiltzkin script.
    /// Returns a map of script name → scan data.
    /// </summary>
    private static Dictionary<string, ScriptScanData> ScanAllScripts(
        string p0data7Path,
        List<KeyValuePair<int, string[]>> visits)
    {
        var result = new Dictionary<string, ScriptScanData>(
            StringComparer.OrdinalIgnoreCase);

        using var archive = UnityArchiver.Open(p0data7Path);

        foreach (var (price, scripts) in visits)
        {
            foreach (string scriptName in scripts)
            {
                if (result.ContainsKey(scriptName)) continue;

                byte[] bytes = archive.Extract(scriptName);
                var locations = FieldParser.FindItemLocations(bytes);
                var itemPairs = FindItemPairs(locations, price);
                var priceLoc = FindPriceLocation(locations, price);

                result[scriptName] = new ScriptScanData(locations, itemPairs, priceLoc);
            }
        }

        return result;
    }

    // ── Private: item assignment builders ─────────────────────────────────────

    /// <summary>
    /// Shuffle mode: Fisher-Yates shuffles all 24 vanilla items (flat array),
    /// then slices into 8 groups of 3 in visit order.
    /// Consumes 23 RNG calls.
    /// </summary>
    private static int[][] BuildShuffleAssignments(
        List<KeyValuePair<int, string[]>> visits,
        Dictionary<string, ScriptScanData> scanData,
        Random rng)
    {
        // Collect vanilla items in visit order.
        // For Cleyra (paired field), use the first script (ANTRION) as authoritative.
        var allItems = new List<int>(visits.Count * ItemsPerPackage);
        foreach (var (_, scripts) in visits)
        {
            string authoritative = scripts[0];
            if (scanData.TryGetValue(authoritative, out var data))
                foreach (var (_, _, id) in data.ItemPairs)
                    allItems.Add(id);
        }

        // Fisher-Yates shuffle — 23 RNG calls for 24 elements
        for (int i = allItems.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (allItems[i], allItems[j]) = (allItems[j], allItems[i]);
        }

        // Slice into 3-item groups
        var assignments = new int[visits.Count][];
        for (int v = 0; v < visits.Count; v++)
        {
            int offset = v * ItemsPerPackage;
            assignments[v] = new[] { allItems[offset], allItems[offset + 1], allItems[offset + 2] };
        }

        return assignments;
    }

    /// <summary>
    /// Recommended mode: draws 3 items per visit from the appropriate pool.
    /// Consumes 3 RNG calls per visit (24 total across 8 visits).
    /// </summary>
    private static int[][] BuildRecommendedAssignments(
        Settings settings,
        List<KeyValuePair<int, string[]>> visits,
        Random rng)
    {
        var assignments = new int[visits.Count][];
        var subMode = settings.StiltzkinRecommendedSubMode;

        for (int v = 0; v < visits.Count; v++)
        {
            assignments[v] = subMode switch
            {
                StiltzkinRecommendedSubMode.StiltzkinsJunk => new[]
                {
                    DrawFromPool(VanillaObtainabilityData.StiltzkinJunkPool, rng),
                    DrawFromPool(VanillaObtainabilityData.StiltzkinJunkPool, rng),
                    DrawFromPool(VanillaObtainabilityData.StiltzkinJunkPool, rng),
                },

                StiltzkinRecommendedSubMode.Fun => new[]
                {
                    DrawFromPool(VanillaObtainabilityData.StiltzkinFunPoolLow,  rng),
                    DrawFromPool(VanillaObtainabilityData.StiltzkinFunPoolMid,  rng),
                    DrawFromPool(VanillaObtainabilityData.StiltzkinFunPoolHigh, rng),
                },

                StiltzkinRecommendedSubMode.Challenging => new[]
                {
                    DrawFromPool(VanillaObtainabilityData.StiltzkinChallengingPool, rng),
                    DrawFromPool(VanillaObtainabilityData.StiltzkinChallengingPool, rng),
                    DrawFromPool(VanillaObtainabilityData.StiltzkinChallengingPool, rng),
                },

                _ => throw new ArgumentOutOfRangeException(
                    nameof(settings), subMode, "Unknown StiltzkinRecommendedSubMode.")
            };
        }

        return assignments;
    }

    /// <summary>
    /// Draws prices for all 8 visits.
    /// Consumes 1 RNG call per visit when price mode is active; 0 when Off.
    /// </summary>
    private static int[] BuildPriceAssignments(
        StiltzkinPriceMode priceMode,
        List<KeyValuePair<int, string[]>> visits,
        Random rng)
    {
        var prices = new int[visits.Count];
        for (int v = 0; v < visits.Count; v++)
            prices[v] = DrawPrice(priceMode, visits[v].Key, rng);
        return prices;
    }

    // ── Private: patch writing ─────────────────────────────────────────────────

    /// <summary>
    /// Applies item and price patches to all Stiltzkin scripts and writes output.
    /// Returns total number of scripts patched.
    /// </summary>
    private static int WritePatches(
        string p0data7Path,
        string modOutputRoot,
        List<KeyValuePair<int, string[]>> visits,
        Dictionary<string, ScriptScanData> scanData,
        int[][] itemAssignments,
        int[] priceAssignments)
    {
        int scriptsPatched = 0;

        using var archive = UnityArchiver.Open(p0data7Path);

        for (int v = 0; v < visits.Count; v++)
        {
            int vanillaPrice = visits[v].Key;
            string[] scripts = visits[v].Value;
            int[] newItems = itemAssignments[v];
            int newPrice = priceAssignments[v];

            foreach (string scriptName in scripts)
            {
                if (!scanData.TryGetValue(scriptName, out var data)) continue;
                if (data.ItemPairs.Count < ItemsPerPackage) continue;

                // Build patch list
                var patches = new List<(FieldItemLocation, int)>(ItemsPerPackage * 2 + 1);

                // Item patches — both locations per pair
                for (int i = 0; i < ItemsPerPackage; i++)
                {
                    var (loc1, loc2, _) = data.ItemPairs[i];
                    patches.Add((loc1, newItems[i]));
                    patches.Add((loc2, newItems[i]));
                }

                // Price patch — TextSync only (engine handles Gil deduction from variable)
                if (data.PriceLocation is not null && newPrice != vanillaPrice)
                    patches.Add((data.PriceLocation, newPrice));

                // Extract US bytes and apply patches
                byte[] usBytes = archive.Extract(scriptName);
                byte[] patched = FieldParser.ApplyPatches(usBytes, patches);

                // Write patched bytes to all 7 language output paths
                // (same bytes to all locales — consistent with FieldItemRandomizer;
                // per-locale patching is a future upgrade per Phase 5.9.3 parking lot)
                string outputFileName = scriptName.EndsWith(".bytes",
                    StringComparison.OrdinalIgnoreCase)
                    ? scriptName
                    : scriptName + ".bytes";

                foreach (string lang in Languages)
                {
                    string outputPath = GetOutputPath(modOutputRoot, lang, outputFileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                    File.WriteAllBytes(outputPath, patched);
                }

                scriptsPatched++;
            }
        }

        return scriptsPatched;
    }
}