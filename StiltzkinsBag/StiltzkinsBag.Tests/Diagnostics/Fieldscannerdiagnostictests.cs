// StiltzkinsBag.Tests/Diagnostics/FieldScannerDiagnosticTests.cs
//
// Diagnostic tests that write a comprehensive trace of every item location found
// by FieldItemScanner across all field scripts.
//
// Purpose:
//   1. Investigate the Dagger-101 false positive — see which files contribute
//      to the inflated count and at what byte offsets.
//   2. Provide human-readable evidence for the Phase 9 FieldParser fix (Task 10).
//   3. Produce the per-file data needed to build itemIdToMinFieldId for
//      EnforceSynthesisReachability wiring (Task 4).
//
// Requires: FF9_GAME_PATH environment variable pointing to the game root.
// Skips silently when absent — same pattern as DeterminismIntegrationTests.
//
// Output: TestData/Diagnostics/FieldScanDiagnostic.txt (written to test project root)
//
// Sections in output file:
//   1. Header / totals
//   2. Per-item summary sorted by TotalCount descending (false positives surface first)
//   3. High-count items (TotalCount > 20) with contributing file list
//   4. Per-file detail — every item/treasure location per field file

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using StiltzkinsBag.Core.Models;
using StiltzkinsBag.Parsing;
using Xunit;

namespace StiltzkinsBag.Tests.Diagnostics;

public sealed class FieldScannerDiagnosticTests
{
    // Threshold above which an item count is flagged as a potential false positive.
    // Real vanilla FFIX has at most ~15 copies of any single item in field chests.
    // Items above this threshold are listed with their contributing files.
    private const int HighCountThreshold = 20;

    [Fact]
    public void Diagnostic_FieldScanner_PerFileDump()
    {
        string? gamePath = Environment.GetEnvironmentVariable("FF9_GAME_PATH");
        if (string.IsNullOrWhiteSpace(gamePath))
        {
            // Skip gracefully — same pattern as DeterminismIntegrationTests
            return;
        }

        string p0data7Path = Path.Combine(gamePath, "StreamingAssets", "p0data7.bin");
        if (!File.Exists(p0data7Path))
        {
            return;
        }

        // ── Load item names ────────────────────────────────────────────────────
        // Load from Items.csv when available; fall back to "Item #N" placeholders.
        string itemsCsvPath = Path.Combine(
            gamePath, "StreamingAssets", "Data", "Items", "Items.csv");

        var itemNames = LoadItemNames(itemsCsvPath);

        // ── Run detailed scan ──────────────────────────────────────────────────
        IReadOnlyList<FieldFileScanResult> fileResults =
            FieldItemScanner.ScanArchiveDetailed(p0data7Path);

        // ── Build aggregate structures ─────────────────────────────────────────
        // Per-item: total count + list of (fileName, locations) contributing to it
        var itemContributions = new Dictionary<int, List<(string File, IReadOnlyList<FieldItemLocation> Locs)>>();

        foreach (FieldFileScanResult fileResult in fileResults)
        {
            // Collect item-bearing locations for this file
            var itemLocs = fileResult.Locations
                .Where(l => l.LocationKind == FieldLocationKind.DirectItem ||
                            l.LocationKind == FieldLocationKind.TreasureItem)
                .ToList();

            // Group by item ID
            var byItemId = itemLocs
                .Where(l => l.LocationKind == FieldLocationKind.TreasureItem
                    ? l.TreasureIsItem && l.CurrentValue >= 1 && l.CurrentValue <= 255
                    : l.CurrentValue >= 1 && l.CurrentValue <= 255)
                .GroupBy(l => l.CurrentValue);

            foreach (var group in byItemId)
            {
                if (!itemContributions.TryGetValue(group.Key, out var list))
                {
                    list = new List<(string, IReadOnlyList<FieldItemLocation>)>();
                    itemContributions[group.Key] = list;
                }
                list.Add((fileResult.FileName, group.ToList()));
            }
        }

        // Per-item total count
        var itemTotals = itemContributions.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Sum(t => t.Locs.Sum(l => l.ItemCount)));

        // ── Write output file ──────────────────────────────────────────────────
        string outputDir = Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", // up from bin/Debug/net8.0
            "TestData", "Diagnostics");
        Directory.CreateDirectory(outputDir);

        string outputPath = Path.Combine(outputDir, "FieldScanDiagnostic.txt");

        using var writer = new StreamWriter(outputPath, append: false, Encoding.UTF8);

        // ── Section 1: Header ──────────────────────────────────────────────────
        writer.WriteLine("=== FIELD SCANNER DIAGNOSTIC ===");
        writer.WriteLine($"Generated : {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        writer.WriteLine($"Archive   : {p0data7Path}");
        writer.WriteLine($"Files with items : {fileResults.Count}");
        writer.WriteLine($"Distinct item IDs: {itemContributions.Count}");
        writer.WriteLine($"Total item instances (sum): {itemTotals.Values.Sum()}");
        writer.WriteLine();

        // ── Section 2: Per-item summary (sorted by count DESC) ─────────────────
        writer.WriteLine("=== PER-ITEM SUMMARY (sorted by TotalCount DESC) ===");
        writer.WriteLine($"{"ID",-5} {"Name",-30} {"TotalCount",12} {"FilesCount",12}");
        writer.WriteLine(new string('-', 65));

        foreach (var (itemId, total) in itemTotals.OrderByDescending(kvp => kvp.Value))
        {
            string name = GetItemName(itemNames, itemId);
            int filesCount = itemContributions[itemId].Count;
            writer.WriteLine($"{itemId,-5} {name,-30} {total,12} {filesCount,12}");
        }
        writer.WriteLine();

        // ── Section 3: High-count items (potential false positives) ────────────
        var highCount = itemTotals
            .Where(kvp => kvp.Value > HighCountThreshold)
            .OrderByDescending(kvp => kvp.Value)
            .ToList();

        writer.WriteLine($"=== HIGH-COUNT ITEMS (TotalCount > {HighCountThreshold}) ===");
        if (highCount.Count == 0)
        {
            writer.WriteLine("  None.");
        }
        else
        {
            foreach (var (itemId, total) in highCount)
            {
                string name = GetItemName(itemNames, itemId);
                writer.WriteLine();
                writer.WriteLine($"  [{itemId}] {name}  — TotalCount={total}");
                writer.WriteLine($"  {"File",-50} {"Count",8} {"Offsets"}");
                writer.WriteLine($"  {new string('-', 90)}");

                foreach (var (file, locs) in itemContributions[itemId]
                    .OrderByDescending(t => t.Locs.Sum(l => l.ItemCount)))
                {
                    int fileCount = locs.Sum(l => l.ItemCount);
                    string offsets = string.Join(", ",
                        locs.Select(l => $"0x{l.FileOffset:X4}({l.LocationKind.ToString()[0]}×{l.ItemCount})"));
                    writer.WriteLine($"  {file,-50} {fileCount,8}  {offsets}");
                }
            }
        }
        writer.WriteLine();

        // ── Section 4: Per-file detail ─────────────────────────────────────────
        writer.WriteLine("=== PER-FILE DETAIL (files with items, sorted by filename) ===");
        writer.WriteLine();

        foreach (FieldFileScanResult fileResult in fileResults)
        {
            var itemLocs = fileResult.Locations
                .Where(l => l.LocationKind == FieldLocationKind.DirectItem ||
                            l.LocationKind == FieldLocationKind.TreasureItem)
                .OrderBy(l => l.FileOffset)
                .ToList();

            if (itemLocs.Count == 0) continue;

            writer.WriteLine($"[{fileResult.FileName}]");

            foreach (FieldItemLocation loc in itemLocs)
            {
                string kindStr = loc.LocationKind == FieldLocationKind.DirectItem
                    ? "DirectItem  "
                    : "TreasureItem";

                string valueStr;
                if (loc.LocationKind == FieldLocationKind.TreasureItem)
                {
                    if (loc.TreasureIsItem)
                        valueStr = $"ID={loc.CurrentValue} ({GetItemName(itemNames, loc.CurrentValue)})";
                    else if (loc.TreasureIsCard)
                        valueStr = $"Card={loc.CurrentValue - 512}";
                    else if (loc.TreasureIsGil)
                        valueStr = $"{loc.TreasureGilAmount}G";
                    else
                        valueStr = $"raw={loc.CurrentValue}";
                }
                else
                {
                    if (loc.CurrentValue >= 1 && loc.CurrentValue <= 255)
                        valueStr = $"ID={loc.CurrentValue} ({GetItemName(itemNames, loc.CurrentValue)})";
                    else
                        valueStr = $"ID={loc.CurrentValue} [OUT OF RANGE — false positive]";
                }

                writer.WriteLine(
                    $"  {kindStr} | 0x{loc.FileOffset:X4} | ×{loc.ItemCount} | {valueStr}");
            }

            writer.WriteLine();
        }

        writer.WriteLine("=== END ===");

        // ── Assertions ────────────────────────────────────────────────────────
        // Sanity checks — ensure the scan ran and produced meaningful output.
        // These are not correctness assertions on item counts (that's the point
        // of the diagnostic — to discover what the counts actually are).
        Assert.True(fileResults.Count > 100,
            $"Expected at least 100 field files with items, got {fileResults.Count}. " +
            "Scanner may have failed to open the archive.");

        Assert.True(itemContributions.Count > 50,
            $"Expected at least 50 distinct item IDs, got {itemContributions.Count}.");

        // Report the output path so it's visible in test output
        Assert.True(File.Exists(outputPath),
            $"Diagnostic file was not written to {outputPath}.");
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static Dictionary<int, string> LoadItemNames(string itemsCsvPath)
    {
        var names = new Dictionary<int, string>();
        if (!File.Exists(itemsCsvPath)) return names;

        try
        {
            // Items.csv inline comments: ";# NNN - Item Name"
            // Parse them manually — we don't want a full CsvHelper dependency in a diagnostic.
            string[] lines = File.ReadAllLines(itemsCsvPath, Encoding.UTF8);
            int rowIndex = 0;

            foreach (string line in lines)
            {
                // Skip comment-block lines and empty lines
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.TrimStart().StartsWith('#')) continue;

                // Look for inline comment on data rows: something like "0;0;...;# 000 - Hammer"
                int commentStart = line.IndexOf(";#", StringComparison.Ordinal);
                if (commentStart < 0)
                {
                    rowIndex++;
                    continue;
                }

                string comment = line[(commentStart + 1)..].TrimStart(';', '#', ' ');
                int dashIdx = comment.IndexOf(" - ", StringComparison.Ordinal);
                if (dashIdx >= 0 && dashIdx <= 5)
                {
                    string itemName = comment[(dashIdx + 3)..].Trim();
                    if (!string.IsNullOrEmpty(itemName))
                    {
                        // The ID is in the comment number (e.g., "000 - Hammer")
                        if (int.TryParse(comment[..dashIdx].Trim(), out int parsedId))
                            names[parsedId] = itemName;
                    }
                }

                rowIndex++;
            }
        }
        catch
        {
            // If parsing fails for any reason, return whatever we got
        }

        return names;
    }

    private static string GetItemName(Dictionary<int, string> names, int itemId)
    {
        return names.TryGetValue(itemId, out string? name) ? name : $"Item #{itemId}";
    }
}