// StiltzkinsBag.Tests/StiltzkinFieldDiagnosticTests.cs
//
// Task 7 — Stiltzkin field identification.
//
// Scans p0data7.bin for evt_* field scripts containing AddGil(X) where X is
// one of Stiltzkin's 8 known package prices: 333, 444, 555, 666, 777, 888, 2222, 5555.
//
// Uses FieldParser.FindItemLocations which parses DirectGil locations.
// Each Stiltzkin visit script contains exactly one AddGil call for the package price.
//
// Known field IDs from prior research (PhaseEnd_Phase5_9_2.md):
//   333G  → Field 764   (Burmecia)
//   444G  → Field 1105  (Cleyra)
//   555G  → Field 1418  (Fossil Roo)
//   666G  → Field 1553  (Conde Petie Mountain Path)
//   777G  → Field 1865  (Alexandria)
//   888G  → Field 2259  (Oeilvert)
//  2222G  → Field 2655  (Bran Bal)
//  2456G  → Field 2456  (Alexandria 2nd visit / 5555G package)
//
// Output:
//   StiltzkinFieldDiagnostic.txt — all evt_ filenames with matching AddGil amounts
//
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

public class StiltzkinFieldDiagnosticTests
{
    // ── Paths ──────────────────────────────────────────────────────────────

    private static readonly string TestDataDir =
        Path.Combine(AppContext.BaseDirectory, "TestData");

    private static readonly string ArchivePath =
        Path.Combine(TestDataDir, "p0data7.bin");

    private static readonly string OutputPath =
        Path.Combine(TestDataDir, "StiltzkinFieldDiagnostic.txt");

    // ── Stiltzkin package prices (all 8 visits) ────────────────────────────

    private static readonly HashSet<int> StiltzkinPrices = new()
    {
        333,   // Burmecia
        444,   // Cleyra
        555,   // Fossil Roo
        666,   // Conde Petie Mountain Path
        777,   // Alexandria (1st visit)
        888,   // Oeilvert
        2222,  // Bran Bal
        5555,  // Alexandria (2nd visit / final)
    };

    private readonly ITestOutputHelper _out;
    public StiltzkinFieldDiagnosticTests(ITestOutputHelper output) => _out = output;

    // ── Test 1 — Diagnostic scan ───────────────────────────────────────────

    [Fact]
    public void Diagnostic_FindStiltzkinFieldScripts()
    {
        if (!File.Exists(ArchivePath))
        {
            _out.WriteLine($"SKIP: p0data7.bin not found at {ArchivePath}");
            return;
        }

        // price → list of (evtName, fileOffset) pairs
        var hits = new Dictionary<int, List<(string Name, int Offset)>>();
        foreach (int price in StiltzkinPrices)
            hits[price] = new List<(string, int)>();

        // Also capture ALL AddGil locations per file for context
        var allGilHits = new List<(string Name, int GilAmount, int Offset)>();

        using var archive = UnityArchiver.Open(ArchivePath);

        var evtNames = archive.GetFileNames()
            .Where(n => n.StartsWith("evt_", StringComparison.OrdinalIgnoreCase)
                     && !n.StartsWith("evt_battle_", StringComparison.OrdinalIgnoreCase)
                     && !n.StartsWith("evt_world_", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _out.WriteLine($"Scanning {evtNames.Count} field scripts for Stiltzkin package prices (TextSync slot 0, value > 255)...");

        foreach (string name in evtNames)
        {
            byte[] bytes;
            try { bytes = archive.Extract(name); }
            catch { continue; }

            IReadOnlyList<FieldItemLocation> locations;
            try { locations = FieldParser.FindItemLocations(bytes); }
            catch { continue; }

            foreach (var loc in locations)
            {
                // Stiltzkin uses SetTextVariable(0, price) to display the package cost.
                // This is parsed as TextSync with CurrentValue = the price.
                // Package prices (333-5555) are not valid item IDs — unambiguous identifiers.
                if (loc.LocationKind != FieldLocationKind.TextSync) continue;
                // Only slot 0 (CurrentValue range check — prices are > 255, items are 0-255)
                int gilAmount = loc.CurrentValue;
                if (gilAmount <= 255) continue; // skip item display TextSync calls

                allGilHits.Add((name, gilAmount, loc.FileOffset));

                if (StiltzkinPrices.Contains(gilAmount))
                    hits[gilAmount].Add((name, loc.FileOffset));
            }
        }

        // ── Build output ───────────────────────────────────────────────────
        var sb = new StringBuilder();
        sb.AppendLine("Stiltzkin Field Script Diagnostic");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Scanned: {evtNames.Count} field scripts (world map excluded)");
        sb.AppendLine();

        sb.AppendLine("=== STILTZKIN PRICE HITS ===");
        sb.AppendLine();

        int foundCount = 0;
        foreach (int price in StiltzkinPrices.OrderBy(x => x))
        {
            var matches = hits[price];
            string status = matches.Count == 0 ? "NOT FOUND" :
                            matches.Count == 1 ? "✓ FOUND" : $"MULTIPLE ({matches.Count})";
            sb.AppendLine($"  {price,5} Gil — {status}");

            foreach (var (name, offset) in matches)
            {
                sb.AppendLine($"    {name}  (offset=0x{offset:X4})");
                foundCount++;
            }
        }

        sb.AppendLine();
        sb.AppendLine($"Found: {foundCount} / {StiltzkinPrices.Count} prices matched");
        sb.AppendLine();

        sb.AppendLine("=== CONFIRMED MAPPING (evt_ filename → gil price) ===");
        sb.AppendLine();
        foreach (int price in StiltzkinPrices.OrderBy(x => x))
        {
            var matches = hits[price];
            if (matches.Count == 1)
                sb.AppendLine($"  {matches[0].Name,-45} → {price} Gil");
            else if (matches.Count > 1)
            {
                sb.AppendLine($"  [AMBIGUOUS {price} Gil]:");
                foreach (var (name, _) in matches)
                    sb.AppendLine($"    {name}");
            }
            else
            {
                sb.AppendLine($"  [NOT FOUND: {price} Gil]");
            }
        }

        sb.AppendLine();
        sb.AppendLine("=== ALL TEXTSYNC >255 LOCATIONS (reference) ===");
        sb.AppendLine();
        foreach (var group in allGilHits.GroupBy(h => h.Name).OrderBy(g => g.Key))
        {
            sb.AppendLine($"  [{group.Key}]");
            foreach (var (_, amount, offset) in group.OrderBy(h => h.Offset))
                sb.AppendLine($"    offset=0x{offset:X4}  gil={amount}");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
        File.WriteAllText(OutputPath, sb.ToString(), Encoding.UTF8);

        // ── Assertions & output ────────────────────────────────────────────
        _out.WriteLine($"Output: {OutputPath}");
        _out.WriteLine(string.Empty);
        _out.WriteLine("Price → filename mapping:");
        foreach (int price in StiltzkinPrices.OrderBy(x => x))
        {
            var matches = hits[price];
            string name = matches.Count == 1 ? matches[0].Name : $"({matches.Count} matches)";
            _out.WriteLine($"  {price,5}G → {name}");
        }
    }

    // ── Test 2 — Verify known field IDs match expected scripts ────────────

    [Fact]
    public void Verification_KnownStiltzkinScripts_ContainCorrectGilAmount()
    {
        // Spot-checks specific scripts we expect to contain specific gil amounts.
        // If any of these fail, investigate — the script may have changed or the
        // field ID mapping may be wrong.
        if (!File.Exists(ArchivePath))
        {
            _out.WriteLine($"SKIP: p0data7.bin not found at {ArchivePath}");
            return;
        }

        // Confirmed evt_ names from Diagnostic_FindStiltzkinFieldScripts (2026-03-31).
        // 444G appears in TWO paired Cleyra scripts — both must be patched identically.
        // (same architecture as disc-variant field pairs)
        var confirmed = new (string Name, int Price)[]
        {
            ("EVT_BURMECIA_SQUARE_1.eb",  333),
            ("EVT_CLEYRA3_ANTRION.eb",    444),  // paired field — patch with EVT_CLEYRA3_INN.eb
            ("EVT_CLEYRA3_INN.eb",        444),  // paired field — patch with EVT_CLEYRA3_ANTRION.eb
            ("EVT_FOSSIL_FR_DN1_0.eb",    555),
            ("EVT_PATA_M_CM_MP3_0.eb",    666),
            ("EVT_ALEX3_AT_SENTOU.eb",    777),
            ("EVT_OEIL_UV_DEP_0.eb",      888),
            ("EVT_BAL_BB_WPS_0.eb",       2222),
            ("EVT_ALEX5_AT_SENTOU.eb",    5555),
        };

        using var archive3 = UnityArchiver.Open(ArchivePath);
        foreach (var (evtName, expectedPrice) in confirmed)
        {
            byte[] bytes3;
            try { bytes3 = archive3.Extract(evtName); }
            catch
            {
                _out.WriteLine($"SKIP: {evtName} not found in archive.");
                continue;
            }

            var locs3 = FieldParser.FindItemLocations(bytes3);
            bool found = locs3.Any(l =>
                l.LocationKind == FieldLocationKind.TextSync &&
                l.CurrentValue == expectedPrice);

            _out.WriteLine($"  {evtName,-45} {expectedPrice}G → {(found ? "✓" : "FAIL")}");
            Assert.True(found,
                $"{evtName} should contain SetTextVariable(0, {expectedPrice}) for Stiltzkin price.");
        }
    }
}