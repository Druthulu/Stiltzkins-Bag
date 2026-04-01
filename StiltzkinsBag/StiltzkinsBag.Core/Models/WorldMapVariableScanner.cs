// StiltzkinsBag.Core/Models/WorldMapVariableScanner.cs
//
// Scans FF9 Steam world map script files (.eb.bytes, located in world/us/) for
// Dead Pepper reward delivery blocks.
//
// ── Background ───────────────────────────────────────────────────────────────
//
// FFIX's world map scripts use a generic 4-slot item-delivery function. Before
// calling the delivery function, a switch-case block populates four local
// variables with the item IDs and quantities for the current event (chocograph
// dig or dead pepper dig). The delivery function then calls AddItem four times.
//
// ALL world-map delivery blocks found by this scanner are Dead Pepper rewards.
// The 8 dead pepper events split across three variable conventions:
//
//   Convention A (world00–world11 — Dead Pepper ocean/crack digs):
//     Item  slots: VAR_LocUInt8_37, VAR_LocUInt8_38, VAR_LocUInt8_39,
//                  VAR_LocInt16_40
//     Count slots: VAR_LocUInt8_42, VAR_LocUInt8_43, VAR_LocUInt8_44,
//                  VAR_LocUInt8_45
//
//   Convention B (world12 only — evt_world_world12.eb.bytes, Chocobo Garden):
//     Item  slots: VAR_LocUInt8_2, VAR_LocUInt8_3, VAR_LocUInt8_4,
//                  VAR_LocInt16_5
//     Count slots: VAR_LocUInt8_7, VAR_LocUInt8_8, VAR_LocUInt8_9,
//                  VAR_LocUInt8_10
//
//   Convention C (world00–world11 — chocograph World_Chest function rewards):
//     These are the delivery blocks inside World_Chest_N case handlers, where
//     items are first placed into a different set of local variables. Notable
//     items in this convention include Ragnarok (ID 29) and Dragon's Claws (ID 45).
//     Item  slots: VAR_LocUInt8_9,  VAR_LocUInt8_10, VAR_LocUInt8_11,
//                  VAR_LocInt16_12
//     Count slots: VAR_LocUInt8_14, VAR_LocUInt8_15, VAR_LocUInt8_16,
//                  VAR_LocUInt8_17
//
// ── Binary encoding ──────────────────────────────────────────────────────────
//
// "set VAR_LocUInt8_N = X" uses postfix (RPN) notation in the variable stream:
//
//   Byte layout: 05  D6  [N]  7D  [lo]  [hi]  2C  7F
//                |   |   |    |   |     |     |   |
//                |   |   |    |   +-----+     |   Terminate (0x7F)
//                |   |   |    |   value LE16   Assign (0x2C = '=')
//                |   |   |    ConstShort (0x7D)
//                |   |   Variable index
//                |   VAR_LocUInt8_ prefix (0xD6)
//                set opcode (0x05)
//
// "set VAR_LocInt16_N = X" is identical but uses prefix 0xDA instead of 0xD6.
//
// Each delivery event = exactly 8 consecutive 8-byte instructions (64 bytes total),
// in the order: item0, count0, item1, count1, item2, count2, item3, count3.
//
// The value bytes (lo at instr_offset+4, hi at instr_offset+5) are the bytes
// the randomizer will replace to change the item ID.
//
// ── Deduplication ────────────────────────────────────────────────────────────
//
// The same 7 Convention-A delivery blocks appear verbatim in all 6 active-
// transport world maps (world00, world03, world05, world07, world08, world09).
// Scanning all files without deduplication would count each event 6×.
// This class deduplicates by the 4-item ID signature of each block, keeping
// each unique event exactly once.
//
// ── Filtering ────────────────────────────────────────────────────────────────
//
// Item IDs outside 1–255 are FFIX card IDs (512+) used as chocograph-menu
// rewards in some events. They are excluded from the returned counts (not
// randomizable as regular items).
//
// ── File locations ───────────────────────────────────────────────────────────
//
// World map .eb.bytes files live at:
//   <fieldBytesRoot>/assets/resources/commonasset/eventengine/eventbinary/world/us/
//   evt_world_world00.eb.bytes … evt_world_world12.eb.bytes
//
// Pass the full path to that directory to ScanDirectory(), or use ScanFiles()
// with caller-supplied (name, bytes) tuples for tests.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StiltzkinsBag.Core.Models;

/// <summary>
/// Scans FF9 Steam world map script files to count Dead Pepper reward items.
/// </summary>
public static class WorldMapVariableScanner
{
    // ── Set-opcode binary constants ───────────────────────────────────────────

    /// <summary>Field script opcode for "set" (variable assignment expression).</summary>
    private const byte OpcodeSet = 0x05;

    /// <summary>VarOpList prefix byte for VAR_LocUInt8_ variables (type 10).</summary>
    private const byte PrefixLocUInt8 = 0xD6;

    /// <summary>VarOpList prefix byte for VAR_LocInt16_ variables (type 11).</summary>
    private const byte PrefixLocInt16 = 0xDA;

    /// <summary>VarOpList token for a 16-bit constant literal (type 6; reads 2 more bytes).</summary>
    private const byte TokenConstShort = 0x7D;

    /// <summary>VarOpList token for the numerical assignment operator '=' (type 2).</summary>
    private const byte TokenAssign = 0x2C;

    /// <summary>VarOpList terminate token (type -1; ends the variable expression).</summary>
    private const byte TokenTerminate = 0x7F;

    // ── Delivery convention A: world00–world11 (vars 37–40 / 42–45) ──────────

    /// <summary>
    /// 8-instruction sequence for Convention-A delivery blocks.
    /// Each element is (variablePrefix, variableIndex).
    /// Odd indices (1, 3, 5, 7) are count slots; even (0, 2, 4, 6) are item slots.
    /// </summary>
    private static readonly (byte Prefix, byte Idx)[] ConventionASequence =
    {
        (PrefixLocUInt8, 37),  // item slot 0  (VAR_LocUInt8_37)
        (PrefixLocUInt8, 42),  // count slot 0 (VAR_LocUInt8_42)
        (PrefixLocUInt8, 38),  // item slot 1
        (PrefixLocUInt8, 43),  // count slot 1
        (PrefixLocUInt8, 39),  // item slot 2
        (PrefixLocUInt8, 44),  // count slot 2
        (PrefixLocInt16, 40),  // item slot 3  (VAR_LocInt16_40)
        (PrefixLocUInt8, 45),  // count slot 3
    };

    // ── Delivery convention B: world12 only (vars 2–5 / 7–10) ───────────────

    /// <summary>
    /// 8-instruction sequence for Convention-B delivery blocks (world12 only).
    /// </summary>
    private static readonly (byte Prefix, byte Idx)[] ConventionBSequence =
    {
        (PrefixLocUInt8, 2),   // item slot 0  (VAR_LocUInt8_2)
        (PrefixLocUInt8, 7),   // count slot 0
        (PrefixLocUInt8, 3),   // item slot 1
        (PrefixLocUInt8, 8),   // count slot 1
        (PrefixLocUInt8, 4),   // item slot 2
        (PrefixLocUInt8, 9),   // count slot 2
        (PrefixLocInt16, 5),   // item slot 3  (VAR_LocInt16_5)
        (PrefixLocUInt8, 10),  // count slot 3
    };

    // ── Delivery convention C: world00–world11 World_Chest rewards ───────────

    /// <summary>
    /// 8-instruction sequence for Convention-C delivery blocks.
    /// Used inside the chocograph World_Chest function in non-world12 files.
    /// Items Ragnarok (ID 29) and Dragon's Claws (ID 45) appear in this convention.
    /// </summary>
    private static readonly (byte Prefix, byte Idx)[] ConventionCSequence =
    {
        (PrefixLocUInt8,  9), // item slot 0  (VAR_LocUInt8_9)
        (PrefixLocUInt8, 14), // count slot 0 (VAR_LocUInt8_14)
        (PrefixLocUInt8, 10), // item slot 1  (VAR_LocUInt8_10)
        (PrefixLocUInt8, 15), // count slot 1
        (PrefixLocUInt8, 11), // item slot 2  (VAR_LocUInt8_11)
        (PrefixLocUInt8, 16), // count slot 2
        (PrefixLocInt16, 12), // item slot 3  (VAR_LocInt16_12)
        (PrefixLocUInt8, 17), // count slot 3
    };

    // ── Public entry points ───────────────────────────────────────────────────

    /// <summary>
    /// Scans world map .eb.bytes files in the given directory for Dead Pepper
    /// reward delivery blocks and returns the per-item count.
    /// </summary>
    /// <param name="worldUsDirectory">
    /// Full path to the directory containing the 13 world map files
    /// (evt_world_world00.eb.bytes … evt_world_world12.eb.bytes).
    /// Typically: &lt;fieldBytesRoot&gt;/assets/resources/commonasset/eventengine/
    ///             eventbinary/world/us/
    /// </param>
    /// <returns>
    /// Dictionary mapping item ID (1–255) to the total quantity obtainable from
    /// Dead Pepper rewards. Card IDs and items with ID 0 are excluded.
    /// Each unique delivery event is counted exactly once (deduplicated across
    /// world map files that contain identical event blocks).
    /// </returns>
    public static IReadOnlyDictionary<int, int> ScanDirectory(string worldUsDirectory)
    {
        ArgumentNullException.ThrowIfNull(worldUsDirectory);

        var fileInputs = new List<(string Name, byte[] Bytes)>();

        for (int i = 0; i <= 12; i++)
        {
            string fileName = $"evt_world_world{i:D2}.eb.bytes";
            string fullPath = Path.Combine(worldUsDirectory, fileName);
            if (!File.Exists(fullPath)) continue;

            byte[] data;
            try { data = File.ReadAllBytes(fullPath); }
            catch { continue; }

            fileInputs.Add((fileName, data));
        }

        return ScanFiles(fileInputs);
    }

    /// <summary>
    /// Scans a caller-supplied collection of (filename, bytes) pairs.
    /// Used by tests or callers that load file bytes independently.
    /// </summary>
    /// <param name="files">
    /// Each tuple: (filename for routing/diagnostics, .eb.bytes content).
    /// Files whose name ends in "world12.eb.bytes" are scanned with Convention B only.
    /// All other files are scanned with both Convention A and Convention C — each
    /// convention targets a different variable set and may yield different blocks.
    /// </param>
    public static IReadOnlyDictionary<int, int> ScanFiles(
        IEnumerable<(string Name, byte[] Bytes)> files)
    {
        ArgumentNullException.ThrowIfNull(files);

        var counts = new Dictionary<int, int>();
        var seenBlockSignatures = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (name, bytes) in files)
        {
            if (bytes is null || bytes.Length == 0) continue;

            // Convention B applies to world12 (Chocobo Treasure) only.
            // All other files are scanned with Convention A (Dead Pepper ocean/crack
            // digs) AND Convention C (chocograph World_Chest rewards).
            bool isConventionB = name.EndsWith("world12.eb.bytes",
                StringComparison.OrdinalIgnoreCase);

            IEnumerable<(int[] Items, int[] Counts)> blocks = isConventionB
                ? FindDeliveryBlocks(bytes, ConventionBSequence)
                : FindDeliveryBlocks(bytes, ConventionASequence)
                    .Concat(FindDeliveryBlocks(bytes, ConventionCSequence));

            foreach (var (items, itemCounts) in blocks)
            {
                // Deduplicate by the 4-item ID signature.
                string sig = string.Join(",", items);
                if (!seenBlockSignatures.Add(sig)) continue;

                for (int slot = 0; slot < 4; slot++)
                {
                    int itemId = items[slot];
                    // Item IDs 1–255 only; skip card IDs (>255) and sentinel 0.
                    if (itemId < 1 || itemId > 255) continue;

                    int qty = itemCounts[slot];
                    if (qty <= 0) qty = 1;

                    counts[itemId] = counts.TryGetValue(itemId, out int existing)
                        ? existing + qty
                        : qty;
                }
            }
        }

        return counts;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Scans <paramref name="data"/> for all 64-byte delivery blocks that match
    /// the given variable sequence. Yields (items[4], counts[4]) for each block.
    /// </summary>
    private static IEnumerable<(int[] Items, int[] Counts)> FindDeliveryBlocks(
        byte[] data,
        (byte Prefix, byte Idx)[] sequence)
    {
        if (data.Length < 64) yield break;

        var (firstPrefix, firstIdx) = sequence[0];
        int limit = data.Length - 64;

        for (int pos = 0; pos <= limit; pos++)
        {
            // Fast pre-check on the first instruction.
            if (data[pos]     != OpcodeSet      ||
                data[pos + 1] != firstPrefix     ||
                data[pos + 2] != firstIdx        ||
                data[pos + 3] != TokenConstShort ||
                data[pos + 6] != TokenAssign     ||
                data[pos + 7] != TokenTerminate)
                continue;

            // Full verification: all 8 instructions must match.
            bool ok = true;
            int[] values = new int[8];

            for (int i = 0; i < 8; i++)
            {
                int p = pos + i * 8;
                var (epfx, eidx) = sequence[i];

                if (data[p]     != OpcodeSet      ||
                    data[p + 1] != epfx            ||
                    data[p + 2] != eidx            ||
                    data[p + 3] != TokenConstShort ||
                    data[p + 6] != TokenAssign     ||
                    data[p + 7] != TokenTerminate)
                {
                    ok = false;
                    break;
                }

                values[i] = data[p + 4] | (data[p + 5] << 8);
            }

            if (!ok) continue;

            // Even indices = items; odd indices = counts.
            int[] items  = { values[0], values[2], values[4], values[6] };
            int[] counts = { values[1], values[3], values[5], values[7] };

            yield return (items, counts);

            // Skip past this 64-byte block (loop increments pos once more).
            pos += 63;
        }
    }
}
