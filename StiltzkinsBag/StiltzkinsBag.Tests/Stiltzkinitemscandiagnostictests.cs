// StiltzkinsBag.Tests/StiltzkinItemScanDiagnosticTests.cs
//
// Phase 5.99 — Task 1 — Stiltzkin item scan diagnostic + algorithm verification.
//
// Diagnostic confirms item location structure in all 9 Stiltzkin scripts.
// Verification confirms the position-based extraction algorithm returns the
// correct vanilla item IDs for all 8 visits.
//
// === EXTRACTION ALGORITHM (used by StiltzkinRandomizer) ===
//
//   1. Find the price TextSync offset:
//      the FieldItemLocation with LocationKind == TextSync and CurrentValue == packagePrice.
//      Use the last occurrence (some scripts have earlier TextSync hits at the same value).
//   2. Collect all DirectItem locations where:
//      FileOffset > priceOffset  AND  1 <= CurrentValue <= 255
//   3. Sort by FileOffset — these arrive as sequential pairs (same ID, ~22 bytes apart).
//   4. Take the first 6 locations as 3 pairs; verify each pair has matching IDs.
//   5. Return [pair0_id, pair1_id, pair2_id]
//
// This algorithm is purely positional — it does NOT require hardcoded item IDs.
// Known vanilla IDs (KnownPackageItems below) are TEST-LOCAL ONLY for verification.
// The randomizer never references them.
//
// === EDGE CASES HANDLED BY THE ALGORITHM ===
//
//   Alex5 (5555G) — Ribbon (item 221) appears as a DirectItem BEFORE the price
//     TextSync. It is excluded because the filter requires offset > priceOffset.
//     The 3 pairs immediately after are the package items (229, 230, 239). ✓
//
//   Oeilvert (888G) — item 161 appears as a 4th DirectItem pair after the package.
//     It is excluded because the algorithm takes only the first 3 pairs. ✓
//
//   Bran Bal (2222G) — item 87 appears as a 4th DirectItem pair after the package.
//     Same exclusion by taking only 3 pairs. ✓
//
//   Cleyra pair — EVT_CLEYRA3_ANTRION and EVT_CLEYRA3_INN both host the 444G
//     encounter. The algorithm runs independently on each script and returns the
//     same IDs ([237, 238, 249]) at different absolute offsets. Both must be patched.
//
// Output: TestData/StiltzkinItemDump.txt
// Requires p0data7.bin — skips gracefully if absent.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using StiltzkinsBag.Core.Parsing;
using StiltzkinsBag.Parsing;

namespace StiltzkinsBag.Tests;

public class StiltzkinItemScanDiagnosticTests
{
    // ── Paths ──────────────────────────────────────────────────────────────

    private static readonly string TestDataDir =
        Path.Combine(AppContext.BaseDirectory, "TestData");

    private static readonly string ArchivePath =
        Path.Combine(TestDataDir, "p0data7.bin");

    private static readonly string OutputPath =
        Path.Combine(TestDataDir, "StiltzkinItemDump.txt");

    // ── Confirmed Stiltzkin scripts (from Phase 5.9.3) ─────────────────────
    //
    // 9 scripts total for 8 visits.
    // 444G appears in TWO paired Cleyra scripts — both must be patched identically.

    private static readonly (string Name, int Price)[] StiltzkinScripts =
    {
        ("EVT_BURMECIA_SQUARE_1.eb",  333),
        ("EVT_CLEYRA3_ANTRION.eb",    444),  // Cleyra pair — same items as INN
        ("EVT_CLEYRA3_INN.eb",        444),  // Cleyra pair — same items as ANTRION
        ("EVT_FOSSIL_FR_DN1_0.eb",    555),
        ("EVT_PATA_M_CM_MP3_0.eb",    666),
        ("EVT_ALEX3_AT_SENTOU.eb",    777),
        ("EVT_OEIL_UV_DEP_0.eb",      888),
        ("EVT_BAL_BB_WPS_0.eb",       2222),
        ("EVT_ALEX5_AT_SENTOU.eb",    5555),
    };

    // ── Known vanilla package contents (TEST-LOCAL — not used by randomizer) ──
    //
    // Derived from: Final_Fantasy_IX_Reference_Guide_-_Slitzkin_Locations.csv
    // Verified against: binary dump (Task 1 diagnostic, 2026-04-01)
    // Item IDs are in script-execution order (sorted by AddItem offset in bytecode).
    //
    // Item ID → name reference (derived from binary cross-matching):
    //   87  = Tent         227 = Diamond       237 = Hi-Potion    247 = Remedy
    //   161 = (extra NPC)  228 = Emerald        238 = Ether        249 = Phoenix Pinion
    //   221 = Ribbon       229 = Moonstone      239 = Elixir       253 = Tent
    //   230 = Ruby         242 = Soft           245 = Magic Tag
    //
    // Note: 87, 161 appear as 4th pairs in Bran Bal / Oeilvert — excluded by algorithm.
    //       221 (Ribbon) appears in Alex5 BEFORE the price TextSync — excluded by algorithm.

    private static readonly Dictionary<int, int[]> KnownPackageItems = new()
    {
        { 333,  new[] { 242, 237, 238 } },  // Soft,           Hi-Potion, Ether
        { 444,  new[] { 237, 238, 249 } },  // Hi-Potion,      Ether,     Phoenix Pinion
        { 555,  new[] { 249, 247, 238 } },  // Phoenix Pinion, Remedy,    Ether
        { 666,  new[] { 245, 253, 238 } },  // Magic Tag,      Tent,      Ether
        { 777,  new[] { 249, 237, 239 } },  // Phoenix Pinion, Hi-Potion, Elixir
        { 888,  new[] { 237, 228, 239 } },  // Hi-Potion,      Emerald,   Elixir
        { 2222, new[] { 227, 238, 239 } },  // Diamond,        Ether,     Elixir
        { 5555, new[] { 229, 230, 239 } },  // Moonstone,      Ruby,      Elixir
    };

    // Ribbon — final reward in Alex5 for completing all purchases.
    // Appears as a DirectItem BEFORE the 5555G price TextSync; excluded by algorithm.
    private const int RibbonItemId = 221;

    private readonly ITestOutputHelper _out;
    public StiltzkinItemScanDiagnosticTests(ITestOutputHelper output) => _out = output;

    // ── Algorithm ─────────────────────────────────────────────────────────
    //
    // Extracts the 3 Stiltzkin package item IDs from a parsed field script.
    // Returns an empty array if the price TextSync is not found or the pair
    // structure breaks.
    //
    // This is the same logic that StiltzkinRandomizer will use to locate patch offsets.

    private static int[] FindPackageItemIds(IReadOnlyList<FieldItemLocation> locations, int price)
    {
        // Step 1 — find the price TextSync offset (last occurrence for safety).
        var priceLoc = locations
            .Where(l => l.LocationKind == FieldLocationKind.TextSync
                     && l.CurrentValue == price)
            .OrderBy(l => l.FileOffset)
            .LastOrDefault();

        if (priceLoc == null) return Array.Empty<int>();
        int priceOffset = priceLoc.FileOffset;

        // Step 2 — collect valid-ID DirectItem locations strictly after the price TextSync.
        var candidates = locations
            .Where(l => l.LocationKind == FieldLocationKind.DirectItem
                     && l.FileOffset > priceOffset
                     && l.CurrentValue >= 1
                     && l.CurrentValue <= 255)
            .OrderBy(l => l.FileOffset)
            .ToList();

        if (candidates.Count < 6) return Array.Empty<int>();

        // Step 3 — take first 6 as 3 pairs; verify each pair has matching IDs.
        var result = new int[3];
        for (int i = 0; i < 3; i++)
        {
            int id1 = candidates[i * 2].CurrentValue;
            int id2 = candidates[i * 2 + 1].CurrentValue;
            if (id1 != id2) return Array.Empty<int>();
            result[i] = id1;
        }

        return result;
    }

    // ── Test 1 — Full item location dump ───────────────────────────────────

    [Fact]
    public void Diagnostic_StiltzkinScripts_ItemLocationDump()
    {
        if (!File.Exists(ArchivePath))
        {
            _out.WriteLine($"SKIP: p0data7.bin not found at {ArchivePath}");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("=== Stiltzkin Item Scan Diagnostic (Phase 5.99 Task 1) ===");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Archive:   {ArchivePath}");
        sb.AppendLine();

        using var archive = UnityArchiver.Open(ArchivePath);
        var resultsByScript = new Dictionary<string, IReadOnlyList<FieldItemLocation>>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var (name, price) in StiltzkinScripts)
        {
            sb.AppendLine("─────────────────────────────────────────────────────────────");
            sb.AppendLine($"Script:  {name}");
            sb.AppendLine($"Package: {price} Gil");
            sb.AppendLine();

            byte[] bytes;
            try { bytes = archive.Extract(name); sb.AppendLine($"  Extracted: {bytes.Length} bytes"); }
            catch (Exception ex) { sb.AppendLine($"  ERROR extracting: {ex.Message}"); sb.AppendLine(); continue; }

            IReadOnlyList<FieldItemLocation> locations;
            try { locations = FieldParser.FindItemLocations(bytes); resultsByScript[name] = locations; }
            catch (Exception ex) { sb.AppendLine($"  ERROR parsing: {ex.Message}"); sb.AppendLine(); continue; }

            var byKind = locations.GroupBy(l => l.LocationKind).ToDictionary(g => g.Key, g => g.ToList());
            sb.AppendLine($"  Total locations found: {locations.Count}");
            sb.AppendLine();

            if (byKind.TryGetValue(FieldLocationKind.DirectItem, out var directItems))
            {
                sb.AppendLine($"  DirectItem × {directItems.Count}:");
                foreach (var loc in directItems.OrderBy(l => l.FileOffset))
                    sb.AppendLine($"    offset=0x{loc.FileOffset:X4}  itemId={loc.CurrentValue,6}  (0x{loc.CurrentValue:X4})");
            }
            else sb.AppendLine("  DirectItem: NONE");
            sb.AppendLine();

            if (byKind.TryGetValue(FieldLocationKind.TreasureItem, out var treasureItems))
            {
                sb.AppendLine($"  TreasureItem × {treasureItems.Count}:");
                foreach (var loc in treasureItems.OrderBy(l => l.FileOffset))
                    sb.AppendLine($"    offset=0x{loc.FileOffset:X4}  value={loc.CurrentValue,5}");
            }
            else sb.AppendLine("  TreasureItem: NONE");
            sb.AppendLine();

            if (byKind.TryGetValue(FieldLocationKind.TextSync, out var textSyncs))
            {
                var itemSync = textSyncs.Where(l => l.CurrentValue <= 255).ToList();
                var priceSync = textSyncs.Where(l => l.CurrentValue > 255).ToList();
                if (itemSync.Count > 0)
                {
                    sb.AppendLine($"  TextSync (item display) × {itemSync.Count}:");
                    foreach (var loc in itemSync.OrderBy(l => l.FileOffset))
                        sb.AppendLine($"    offset=0x{loc.FileOffset:X4}  itemId={loc.CurrentValue,3}");
                }
                if (priceSync.Count > 0)
                {
                    sb.AppendLine($"  TextSync (price/other) × {priceSync.Count}:");
                    foreach (var loc in priceSync.OrderBy(l => l.FileOffset))
                        sb.AppendLine($"    offset=0x{loc.FileOffset:X4}  value={loc.CurrentValue,5}");
                }
            }
            else sb.AppendLine("  TextSync: NONE");
            sb.AppendLine();

            var algIds = FindPackageItemIds(locations, price);
            sb.AppendLine($"  ALGORITHM RESULT: [{string.Join(", ", algIds)}]");
            sb.AppendLine();

            int dCount = byKind.TryGetValue(FieldLocationKind.DirectItem, out var diS) ? diS.Count : 0;
            _out.WriteLine($"  {name,-45}  {price,5}G  DirectItem×{dCount}  alg=[{string.Join(", ", algIds)}]");
        }

        // Cleyra cross-validation using algorithm
        sb.AppendLine("═════════════════════════════════════════════════════════════");
        sb.AppendLine("CLEYRA PAIR CROSS-VALIDATION (444G)");
        sb.AppendLine();

        if (resultsByScript.TryGetValue("EVT_CLEYRA3_ANTRION.eb", out var antrionLocs) &&
            resultsByScript.TryGetValue("EVT_CLEYRA3_INN.eb", out var innLocs))
        {
            var antrionIds = FindPackageItemIds(antrionLocs, 444);
            var innIds = FindPackageItemIds(innLocs, 444);
            bool match = antrionIds.SequenceEqual(innIds);

            sb.AppendLine($"  ANTRION: [{string.Join(", ", antrionIds)}]");
            sb.AppendLine($"  INN:     [{string.Join(", ", innIds)}]");
            sb.AppendLine($"  Match:   {match}");
            sb.AppendLine(match
                ? "\n  ✓  Consistent. Patch both scripts independently at their own offsets."
                : "\n  ⚠  MISMATCH — investigate before building randomizer.");

            _out.WriteLine($"\nCleyra: ANTRION=[{string.Join(", ", antrionIds)}]  INN=[{string.Join(", ", innIds)}]  Match={match}");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
        File.WriteAllText(OutputPath, sb.ToString(), Encoding.UTF8);
        _out.WriteLine($"\nOutput: {OutputPath}");
    }

    // ── Test 2 — Verify algorithm returns correct vanilla IDs ──────────────
    //
    // Asserts FindPackageItemIds returns the expected item IDs for every Stiltzkin
    // script. Validates that the position-based algorithm is correct and robust
    // against the edge cases documented above before the randomizer is built.

    [Fact]
    public void Verification_PackageItemIds_MatchVanillaData()
    {
        if (!File.Exists(ArchivePath))
        {
            _out.WriteLine($"SKIP: p0data7.bin not found at {ArchivePath}");
            return;
        }

        using var archive = UnityArchiver.Open(ArchivePath);
        var cleyraResults = new List<int[]>();

        foreach (var (name, price) in StiltzkinScripts)
        {
            byte[] bytes;
            try { bytes = archive.Extract(name); }
            catch (Exception ex) { _out.WriteLine($"SKIP: {name} — {ex.Message}"); continue; }

            var locations = FieldParser.FindItemLocations(bytes);
            var foundIds = FindPackageItemIds(locations, price);

            Assert.True(foundIds.Length == 3,
                $"{name}: algorithm returned {foundIds.Length} IDs, expected 3. " +
                $"Verify price TextSync (value={price}) exists and 3 valid-ID pairs follow it.");

            var expectedIds = KnownPackageItems[price];

            Assert.True(expectedIds.SequenceEqual(foundIds),
                $"{name}: expected [{string.Join(", ", expectedIds)}] " +
                $"but got [{string.Join(", ", foundIds)}] for {price}G package.");

            // Alex5 only: Ribbon must not appear in package IDs
            if (price == 5555)
                Assert.False(foundIds.Contains(RibbonItemId),
                    $"Alex5: Ribbon (id={RibbonItemId}) must not appear in 5555G package IDs. " +
                    "Confirm it sits before the price TextSync in the bytecode.");

            _out.WriteLine($"  ✓  {name,-45}  {price,5}G  [{string.Join(", ", foundIds)}]");

            if (price == 444) cleyraResults.Add(foundIds);
        }

        // Cleyra: both scripts must return identical IDs
        if (cleyraResults.Count == 2)
        {
            Assert.True(cleyraResults[0].SequenceEqual(cleyraResults[1]),
                "Cleyra paired scripts must return identical package item IDs. " +
                $"ANTRION=[{string.Join(", ", cleyraResults[0])}]  " +
                $"INN=[{string.Join(", ", cleyraResults[1])}]");

            _out.WriteLine($"\n  ✓  Cleyra pair: both scripts return [{string.Join(", ", cleyraResults[0])}]");
        }
    }
}