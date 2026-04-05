// StiltzkinsBag.Tests/Diagnostics/IndirectItemGiveDiagnosticTests.cs
//
// Diagnostic: finds item gives that the production FieldParser currently misses.
//
// Three patterns investigated:
//
//   A) Variable-arg AddItem  — opcode 0x48 where vararg_flag bit 0 = 1.
//      The item ID is stored in a local variable rather than as a constant.
//      Production FieldParser skips these entirely. Gastro Fork (84) and
//      Whale Whisker (63) are suspected here.
//
//   B) Local variable assignments with item-range values
//      Pattern: [D8][id][7D][lo][hi][2C][7F]  where id != 0xE0 (not Treasure_Item)
//      and value is 1–255. The Dead Pepper pattern — item ID written to a local
//      variable, then passed to AddItem as a variable arg.
//
//   C) Early scan stops — functions that hit a switch or unknown opcode before
//      reaching an AddItem. These are the "invisible" item gives.
//
// Cross-referencing A+B per file reveals indirect item gives:
//   file has LocalVarAssign(id=X, value=84) + VariableAddItem → likely Gastro Fork.
//
// Output: TestData/Diagnostics/IndirectItemGiveDiagnostic.txt
// Requires: FF9_GAME_PATH environment variable pointing to game root.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using StiltzkinsBag.Core.Models;
using StiltzkinsBag.Core.Parsing;
using StiltzkinsBag.Parsing;
using Xunit;
using Xunit.Abstractions;

namespace StiltzkinsBag.Tests.Diagnostics;

public sealed class IndirectItemGiveDiagnosticTests
{
    private readonly ITestOutputHelper _out;

    public IndirectItemGiveDiagnosticTests(ITestOutputHelper output)
    {
        _out = output;
    }

    // VarOp constants (same as FieldParser internals)
    private const byte VarCatInt16 = 0xD8; // VAR_GenInt16_ category
    private const byte VarIdTreasure = 0xE0; // Treasure_Item variable ID = 224
    private const byte VarConstShort = 0x7D; // const short token
    private const byte VarAssign = 0x2C; // "=" operator
    private const byte VarTerminate = 0x7F; // terminate

    // ── Result types ──────────────────────────────────────────────────────────

    private sealed record VariableAddItemHit(string FileName, int FunctionOffset, int OpcodeOffset);
    private sealed record LocalVarAssignHit(string FileName, int FunctionOffset, int OpcodeOffset, byte VarId, int Value);
    private sealed record ScanStopHit(string FileName, int FunctionOffset, int StopOffset, int StopOpcode, string Reason);

    [Fact]
    public void Diagnostic_IndirectItemGives_FullArchiveScan()
    {
        string? gamePath = Environment.GetEnvironmentVariable("FF9_GAME_PATH");
        if (string.IsNullOrWhiteSpace(gamePath)) return;

        string p0data7Path = Path.Combine(gamePath, "StreamingAssets", "p0data7.bin");
        if (!File.Exists(p0data7Path)) return;

        // Load item names for readable output
        string itemsCsvPath = Path.Combine(
            gamePath, "StreamingAssets", "Data", "Items", "Items.csv");
        var itemNames = LoadItemNames(itemsCsvPath);

        // ── Scan all fields ───────────────────────────────────────────────────
        var varAddItemHits = new List<VariableAddItemHit>();
        var localVarHits = new List<LocalVarAssignHit>();
        var scanStopHits = new List<ScanStopHit>();
        int filesScanned = 0;

        using var archive = UnityArchiver.Open(p0data7Path);

        var fieldNames = archive.GetFileNames()
            .Where(n => n.StartsWith("evt_", StringComparison.OrdinalIgnoreCase)
                     && !n.StartsWith("evt_battle_", StringComparison.OrdinalIgnoreCase)
                     && !FieldScriptExclusions.IsExcluded(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (string name in fieldNames)
        {
            byte[] bytes;
            try { bytes = archive.Extract(name); }
            catch { continue; }

            FieldScriptHeader header;
            try { header = FieldParser.ParseHeader(bytes); }
            catch { continue; }

            filesScanned++;

            foreach (var entry in header.Entries)
            {
                foreach (var func in entry.Functions)
                {
                    ScanFunction(bytes, name, func.BytecodeOffset, func.BytecodeLength,
                        varAddItemHits, localVarHits, scanStopHits);
                }
            }
        }

        // ── Build output ──────────────────────────────────────────────────────
        string outputDir = Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..",
            "TestData", "Diagnostics");
        Directory.CreateDirectory(outputDir);
        string outputPath = Path.Combine(outputDir, "IndirectItemGiveDiagnostic.txt");

        using var writer = new StreamWriter(outputPath, append: false, Encoding.UTF8);

        writer.WriteLine("=== INDIRECT ITEM GIVE DIAGNOSTIC ===");
        writer.WriteLine($"Generated : {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        writer.WriteLine($"Archive   : {p0data7Path}");
        writer.WriteLine($"Files scanned: {filesScanned}");
        writer.WriteLine();

        // ── Section 1: Pattern A — Variable-arg AddItem ───────────────────────
        writer.WriteLine($"=== PATTERN A: Variable-arg AddItem (item ID is a variable, not constant) ===");
        writer.WriteLine($"Total hits: {varAddItemHits.Count}");
        writer.WriteLine($"These are item gives the production scanner MISSES.");
        writer.WriteLine();

        var varAddByFile = varAddItemHits.GroupBy(h => h.FileName)
            .OrderBy(g => g.Key);
        foreach (var group in varAddByFile)
        {
            writer.WriteLine($"  [{group.Key}]  {group.Count()} variable AddItem call(s)");
            foreach (var hit in group)
                writer.WriteLine($"    opcode at 0x{hit.OpcodeOffset:X4} (in function at 0x{hit.FunctionOffset:X4})");
        }
        writer.WriteLine();

        // ── Section 2: Pattern B — Local var assignments with item values ─────
        writer.WriteLine($"=== PATTERN B: Local variable assignments with item-range values (1-255, not Treasure_Item) ===");
        writer.WriteLine($"Total hits: {localVarHits.Count}");
        writer.WriteLine($"These are item IDs stored in local variables (e.g., Dead Pepper pattern).");
        writer.WriteLine();

        // Group by value (item ID) sorted descending by frequency
        var byValue = localVarHits.GroupBy(h => h.Value)
            .OrderByDescending(g => g.Count())
            .ToList();

        writer.WriteLine($"  Per-item-value summary:");
        writer.WriteLine($"  {"Value",6} {"Name",-28} {"Occurrences",12}");
        writer.WriteLine($"  {new string('-', 50)}");
        foreach (var group in byValue)
        {
            string name = GetItemName(itemNames, group.Key);
            writer.WriteLine($"  {group.Key,6} {name,-28} {group.Count(),12}");
        }
        writer.WriteLine();

        writer.WriteLine($"  Per-file detail:");
        var localVarByFile = localVarHits.GroupBy(h => h.FileName)
            .OrderBy(g => g.Key);
        foreach (var group in localVarByFile)
        {
            writer.WriteLine($"  [{group.Key}]");
            foreach (var hit in group)
            {
                string name = GetItemName(itemNames, hit.Value);
                writer.WriteLine(
                    $"    VarId=0x{hit.VarId:X2} Value={hit.Value} ({name}) " +
                    $"at 0x{hit.OpcodeOffset:X4} (func 0x{hit.FunctionOffset:X4})");
            }
        }
        writer.WriteLine();

        // ── Section 3: Pattern A+B cross-reference ────────────────────────────
        writer.WriteLine("=== CROSS-REFERENCE: Files with BOTH variable AddItem AND local var item assignment ===");
        writer.WriteLine("These files almost certainly contain indirect item gives.");
        writer.WriteLine();

        var filesWithVarAdd = varAddItemHits.Select(h => h.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var filesWithLocalVar = localVarHits.Select(h => h.FileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var intersection = filesWithVarAdd.Intersect(filesWithLocalVar, StringComparer.OrdinalIgnoreCase)
            .OrderBy(f => f)
            .ToList();

        if (intersection.Count == 0)
        {
            writer.WriteLine("  None found.");
        }
        else
        {
            foreach (string file in intersection)
            {
                var varAdds = varAddItemHits.Where(h =>
                    h.FileName.Equals(file, StringComparison.OrdinalIgnoreCase)).ToList();
                var localVars = localVarHits.Where(h =>
                    h.FileName.Equals(file, StringComparison.OrdinalIgnoreCase)).ToList();

                writer.WriteLine($"  [{file}]");
                writer.WriteLine($"    Variable AddItem calls: {varAdds.Count}");
                writer.WriteLine($"    Local var item assignments:");
                foreach (var lv in localVars)
                {
                    string name = GetItemName(itemNames, lv.Value);
                    writer.WriteLine($"      VarId=0x{lv.VarId:X2} → {lv.Value} ({name})");
                }
            }
        }
        writer.WriteLine();

        // ── Section 4: Scan stops ─────────────────────────────────────────────
        writer.WriteLine($"=== PATTERN C: Functions with early scan stops ===");
        writer.WriteLine($"Total stops: {scanStopHits.Count} across {scanStopHits.Select(h => h.FileName).Distinct().Count()} files");
        writer.WriteLine($"These functions may contain item gives that the scanner cannot reach.");
        writer.WriteLine();

        var stopsByReason = scanStopHits.GroupBy(h => h.Reason)
            .OrderByDescending(g => g.Count());
        writer.WriteLine("  Stop reason summary:");
        foreach (var group in stopsByReason)
            writer.WriteLine($"    {group.Key,-40} {group.Count(),6} occurrences");
        writer.WriteLine();

        // List files with scan stops that ALSO have variable AddItem (high priority)
        var stoppedFilesWithVarAdd = scanStopHits
            .Select(h => h.FileName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(f => filesWithVarAdd.Contains(f))
            .OrderBy(f => f)
            .ToList();

        if (stoppedFilesWithVarAdd.Count > 0)
        {
            writer.WriteLine("  Files with BOTH scan stops AND variable AddItem (highest priority to investigate):");
            foreach (string f in stoppedFilesWithVarAdd)
                writer.WriteLine($"    {f}");
            writer.WriteLine();
        }

        // List files with stops that also have local var item assignments
        // (may indicate an indirect give that gets stopped before AddItem)
        var stoppedFilesWithLocalVar = scanStopHits
            .Select(h => h.FileName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(f => filesWithLocalVar.Contains(f))
            .OrderBy(f => f)
            .ToList();

        if (stoppedFilesWithLocalVar.Count > 0)
        {
            writer.WriteLine("  Files with BOTH scan stops AND local var item assignment:");
            foreach (string f in stoppedFilesWithLocalVar)
            {
                var lv = localVarHits.Where(h =>
                    h.FileName.Equals(f, StringComparison.OrdinalIgnoreCase)).ToList();
                var itemsAssigned = string.Join(", ", lv.Select(h =>
                    $"{h.Value}({GetItemName(itemNames, h.Value)})").Distinct());
                writer.WriteLine($"    {f}  →  item values: {itemsAssigned}");
            }
        }

        writer.WriteLine();
        writer.WriteLine("=== END ===");

        _out.WriteLine($"Output: {outputPath}");
        _out.WriteLine($"Variable AddItem hits: {varAddItemHits.Count} across {filesWithVarAdd.Count} files");
        _out.WriteLine($"Local var item assignments: {localVarHits.Count} across {filesWithLocalVar.Count} files");
        _out.WriteLine($"Cross-reference (both): {intersection.Count} files");
        _out.WriteLine($"Scan stops: {scanStopHits.Count}");

        Assert.True(File.Exists(outputPath));
    }

    // ── Sequential scanner ────────────────────────────────────────────────────

    private static void ScanFunction(
        byte[] file,
        string fileName,
        int start,
        int length,
        List<VariableAddItemHit> varAddItems,
        List<LocalVarAssignHit> localVars,
        List<ScanStopHit> scanStops)
    {
        if (length <= 0) return;
        int end = Math.Min(start + length, file.Length);
        int pos = start;

        while (pos < end)
        {
            if (pos >= file.Length) return;
            byte rawOpcode = file[pos++];

            int opcode;
            if (rawOpcode == 0xFF)
            {
                if (pos >= end) return;
                opcode = 0x100 + file[pos++];
            }
            else
            {
                opcode = rawOpcode;
            }

            if (!FieldScriptOpcodeTable.TryGet(opcode, out var info))
            {
                scanStops.Add(new ScanStopHit(
                    fileName, start, pos - 1, opcode,
                    $"Unknown opcode 0x{opcode:X2}"));
                return;
            }

            if (info.IsSwitch)
            {
                scanStops.Add(new ScanStopHit(
                    fileName, start, pos - 1, opcode,
                    $"Switch opcode 0x{opcode:X2}"));
                return;
            }

            int[] argLengths = info.ArgLengths;
            int argCount = argLengths?.Length ?? 0;

            // ── Pattern A: Variable-arg AddItem ───────────────────────────────
            if (opcode == 0x48)
            {
                if (pos < end)
                {
                    byte varargFlag = file[pos];
                    bool itemIsVariable = (varargFlag & 0x01) != 0;
                    if (itemIsVariable)
                    {
                        varAddItems.Add(new VariableAddItemHit(fileName, start, pos - 1));
                    }
                }
                pos = AdvanceInstruction(file, pos, end, info);
                continue;
            }

            // ── Pattern B: Local var assignment with item-range value ─────────
            if (opcode == 0x05 && argCount == 1 && (argLengths![0] == 0))
            {
                pos = ScanSetExpressionForLocalVarAssign(
                    file, pos, end, fileName, start, localVars);
                continue;
            }

            // ── Generic advance ────────────────────────────────────────────────
            pos = AdvanceInstruction(file, pos, end, info);
        }
    }

    /// <summary>
    /// Scans a set (0x05) VarOp expression.
    /// Records local variable assignments where value is 1–255 AND the variable
    /// is NOT Treasure_Item (id != 0xE0).
    /// Also records the normal Treasure_Item assignment so total scan is consistent.
    /// Returns position after the terminating 0x7F.
    /// </summary>
    private static int ScanSetExpressionForLocalVarAssign(
        byte[] file, int pos, int end,
        string fileName, int funcStart,
        List<LocalVarAssignHit> localVars)
    {
        int exprStart = pos;

        while (pos < end)
        {
            if (pos >= file.Length) break;
            byte token = file[pos];

            if (token == VarTerminate)
            {
                pos++;
                break;
            }

            // Check for VAR_GenInt16_ assignment pattern:
            //   [D8][id][7D][lo][hi][2C][7F]
            // This covers BOTH Treasure_Item (id=0xE0) and other local vars.
            if (token == VarCatInt16
                && pos + 6 < end
                && file[pos + 1] != 0x00      // skip null/padding
                && file[pos + 2] == VarConstShort
                && file[pos + 5] == VarAssign
                && file[pos + 6] == VarTerminate)
            {
                byte varId = file[pos + 1];
                int value = ReadUInt16LE(file, pos + 3);

                // Record local var assignments with item-range values
                // EXCLUDING Treasure_Item (0xE0) — that's handled by production scanner
                if (varId != VarIdTreasure && value >= 1 && value <= 255)
                {
                    localVars.Add(new LocalVarAssignHit(
                        fileName, funcStart, pos, varId, value));
                }

                pos += 7; // consume full pattern including 0x7F
                break;
            }

            // Consume this token via the VarOp skip table
            pos++;
            pos += FieldScriptOpcodeTable.GetVarOpSkipBytes(token);
        }

        return pos;
    }

    // ── Generic instruction advance (mirrors FieldParser) ─────────────────────

    private static int AdvanceInstruction(byte[] file, int pos, int end, OpcodeInfo info)
    {
        int[] argLengths = info.ArgLengths;
        if (argLengths == null || argLengths.Length == 0) return pos;

        if (info.UseVarArg)
        {
            if (pos >= end) return end;
            byte varargFlag = file[pos++];

            for (int i = 0; i < argLengths.Length; i++)
            {
                if (pos >= end) return end;
                bool isVarOp = (varargFlag & (1 << i)) != 0;
                pos = isVarOp
                    ? SkipVarOpExpression(file, pos, end)
                    : Math.Min(pos + argLengths[i], end);
            }
        }
        else
        {
            for (int i = 0; i < argLengths.Length; i++)
            {
                if (pos >= end) return end;
                pos = argLengths[i] == 0
                    ? SkipVarOpExpression(file, pos, end)
                    : Math.Min(pos + argLengths[i], end);
            }
        }

        return pos;
    }

    private static int SkipVarOpExpression(byte[] file, int pos, int end)
    {
        while (pos < end)
        {
            if (pos >= file.Length) break;
            byte token = file[pos++];
            if (token == VarTerminate) break;
            pos = Math.Min(pos + FieldScriptOpcodeTable.GetVarOpSkipBytes(token), end);
        }
        return pos;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ushort ReadUInt16LE(byte[] data, int offset)
        => (ushort)(data[offset] | (data[offset + 1] << 8));

    private static Dictionary<int, string> LoadItemNames(string itemsCsvPath)
    {
        var names = new Dictionary<int, string>();
        if (!File.Exists(itemsCsvPath)) return names;
        try
        {
            foreach (string line in File.ReadAllLines(itemsCsvPath, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
                    continue;
                int commentStart = line.IndexOf(";#", StringComparison.Ordinal);
                if (commentStart < 0) continue;
                string comment = line[(commentStart + 1)..].TrimStart(';', '#', ' ');
                int dashIdx = comment.IndexOf(" - ", StringComparison.Ordinal);
                if (dashIdx >= 0 && dashIdx <= 5 &&
                    int.TryParse(comment[..dashIdx].Trim(), out int id))
                {
                    string itemName = comment[(dashIdx + 3)..].Trim();
                    if (!string.IsNullOrEmpty(itemName))
                        names[id] = itemName;
                }
            }
        }
        catch { }
        return names;
    }

    private static string GetItemName(Dictionary<int, string> names, int itemId)
        => names.TryGetValue(itemId, out string? name) ? name : $"Item #{itemId}";
}