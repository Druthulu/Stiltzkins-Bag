using System;
using System.Collections.Generic;

namespace StiltzkinsBag.Core.Parsing
{
    /// <summary>
    /// Top-level decode/encode for one FF9 Steam field script binary (.eb.bytes).
    ///
    /// Faithfully ports HW's ScriptDataStruct + MACRO_SCRIPT_IOFUNCTION (Steam path).
    ///
    /// ── Binary layout ──────────────────────────────────────────────────────────
    ///
    ///   Offset  Size  Field
    ///   ------  ----  -----
    ///   0       2     magic_number (uint16 LE)
    ///   2       1     header_unknown1
    ///   3       1     entry_amount
    ///   4       40    header_unknown2/3 interleaved (20 pairs of uint8)
    ///   44      84    header_name (SCRIPT_NAME_MAX_LENGTH = 84)
    ///   128     N×8   entry table  { uint16 offset, uint16 size,
    ///                                uint8 local_var, uint8 flag, uint16 zero }
    ///
    ///   At (128 + entry_offset[i]):
    ///     0       1     entry_type
    ///     1       1     entry_function_amount
    ///     2       N×4   function table  { uint16 func_type, uint16 func_point }
    ///     (2 + func_point[j]) ..  bytecodes for function j
    ///
    ///   function_point[j] is an offset relative to the function table start
    ///   (i.e., relative to localEntryPos + 2).
    ///
    ///   Function lengths (from HW MACRO_SCRIPT_IOFUNCTION, READ path):
    ///     j &lt; last:  length = function_point[j+1] - function_point[j]
    ///     j == last: length = entry_size[i] - function_point[j] - 2
    ///
    /// ── Round-trip guarantee ───────────────────────────────────────────────────
    ///
    ///   Decode() followed immediately by Encode() produces a byte-identical copy of
    ///   the original data (assuming no modifications between the two calls).
    ///   All stored header/table values are written back verbatim; per-entry body
    ///   bytes not covered by function bytecodes remain zero (matching original
    ///   padding, which HW also writes as zero).
    /// </summary>
    public sealed class FieldScript
    {
        // ── Constants ─────────────────────────────────────────────────────────
        private const int EntryPos         = 128;   // fixed header size
        private const int EntryRowBytes    = 8;     // bytes per entry in entry table
        private const int NameLength       = 84;    // SCRIPT_NAME_MAX_LENGTH
        private const int UnknownPairCount = 20;    // header_unknown2/3 pair count

        // ── Header fields ──────────────────────────────────────────────────────
        public ushort MagicNumber   { get; set; }
        public byte   HeaderUnknown1 { get; set; }
        /// <summary>header_unknown2[0..19] — interleaved with Unknown3 in binary.</summary>
        public byte[] HeaderUnknown2 { get; set; } = new byte[UnknownPairCount];
        /// <summary>header_unknown3[0..19] — interleaved with Unknown2 in binary.</summary>
        public byte[] HeaderUnknown3 { get; set; } = new byte[UnknownPairCount];
        /// <summary>header_name[0..83] — raw bytes (not decoded as a string here).</summary>
        public byte[] HeaderName { get; set; } = new byte[NameLength];

        // ── Per-entry arrays (index = entry index) ─────────────────────────────
        public int      EntryCount        { get; private set; }
        public ushort[] EntryOffset       { get; private set; } = Array.Empty<ushort>();
        public ushort[] EntrySize         { get; private set; } = Array.Empty<ushort>();
        public byte[]   EntryLocalVar     { get; private set; } = Array.Empty<byte>();
        public byte[]   EntryFlag         { get; private set; } = Array.Empty<byte>();
        public byte[]   EntryType         { get; private set; } = Array.Empty<byte>();
        public byte[]   EntryFunctionCount { get; private set; } = Array.Empty<byte>();

        // ── Per-entry per-function arrays (first index = entry, second = function) ──
        public ushort[][] FunctionType  { get; private set; } = Array.Empty<ushort[]>();
        public ushort[][] FunctionPoint { get; private set; } = Array.Empty<ushort[]>();

        // ── Decoded function ASTs ──────────────────────────────────────────────
        public FieldScriptFunction[][] Functions { get; private set; } = Array.Empty<FieldScriptFunction[]>();

        // ── Internal: original file length (used to preserve trailing bytes) ───
        private int _originalFileLength;

        // ── Decode ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Decodes a complete field script binary blob into in-memory structs.
        /// </summary>
        /// <param name="data">Raw bytes of the .eb.bytes file.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException">Data too short or structurally invalid.</exception>
        public static FieldScript Decode(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.Length < EntryPos)
                throw new ArgumentException(
                    $"Data is {data.Length} bytes — too short for header (need {EntryPos}).",
                    nameof(data));

            var fs = new FieldScript { _originalFileLength = data.Length };

            // ── Header ────────────────────────────────────────────────────────
            fs.MagicNumber    = ReadU16(data, 0);
            fs.HeaderUnknown1 = data[2];
            int entryCount    = data[3];

            for (int i = 0; i < UnknownPairCount; i++)
            {
                fs.HeaderUnknown2[i] = data[4 + i * 2];
                fs.HeaderUnknown3[i] = data[4 + i * 2 + 1];
            }
            Array.Copy(data, 44, fs.HeaderName, 0, NameLength);

            int entryTableEnd = EntryPos + entryCount * EntryRowBytes;
            if (data.Length < entryTableEnd)
                throw new ArgumentException(
                    $"Data ({data.Length} B) too short for {entryCount}-entry table " +
                    $"(need {entryTableEnd} B).", nameof(data));

            // ── Entry table ───────────────────────────────────────────────────
            fs.EntryCount         = entryCount;
            fs.EntryOffset        = new ushort[entryCount];
            fs.EntrySize          = new ushort[entryCount];
            fs.EntryLocalVar      = new byte[entryCount];
            fs.EntryFlag          = new byte[entryCount];
            fs.EntryType          = new byte[entryCount];
            fs.EntryFunctionCount = new byte[entryCount];
            fs.FunctionType       = new ushort[entryCount][];
            fs.FunctionPoint      = new ushort[entryCount][];
            fs.Functions          = new FieldScriptFunction[entryCount][];

            for (int i = 0; i < entryCount; i++)
            {
                int t = EntryPos + i * EntryRowBytes;
                fs.EntryOffset[i]   = ReadU16(data, t);
                fs.EntrySize[i]     = ReadU16(data, t + 2);
                fs.EntryLocalVar[i] = data[t + 4];
                fs.EntryFlag[i]     = data[t + 5];
                // t+6, t+7 = zero16, intentionally ignored
            }

            // ── Entry bodies ──────────────────────────────────────────────────
            for (int i = 0; i < entryCount; i++)
            {
                if (fs.EntrySize[i] == 0)
                {
                    // HW stores entry_type=0xFF, empty function arrays for empty entries
                    fs.EntryType[i]          = 0xFF;
                    fs.EntryFunctionCount[i] = 0;
                    fs.FunctionType[i]       = Array.Empty<ushort>();
                    fs.FunctionPoint[i]      = Array.Empty<ushort>();
                    fs.Functions[i]          = Array.Empty<FieldScriptFunction>();
                    continue;
                }

                int localEntryPos = EntryPos + fs.EntryOffset[i];
                fs.EntryType[i]          = data[localEntryPos];
                int funcCount            = data[localEntryPos + 1];
                fs.EntryFunctionCount[i] = (byte)funcCount;

                // function table starts at localEntryPos + 2
                int functionPos = localEntryPos + 2;
                fs.FunctionType[i]  = new ushort[funcCount];
                fs.FunctionPoint[i] = new ushort[funcCount];
                fs.Functions[i]     = new FieldScriptFunction[funcCount];

                for (int j = 0; j < funcCount; j++)
                {
                    fs.FunctionType[i][j]  = ReadU16(data, functionPos + j * 4);
                    fs.FunctionPoint[i][j] = ReadU16(data, functionPos + j * 4 + 2);
                }

                // Compute function lengths (mirrors HW READ path in MACRO_SCRIPT_IOFUNCTION):
                //   j < last:  length = function_point[j+1] - function_point[j]
                //   j == last: length = entry_size[i] - function_point[last] - 2
                for (int j = 0; j < funcCount; j++)
                {
                    int length = (j + 1 < funcCount)
                        ? fs.FunctionPoint[i][j + 1] - fs.FunctionPoint[i][j]
                        : fs.EntrySize[i] - fs.FunctionPoint[i][j] - 2;

                    if (length < 0) length = 0;

                    // Absolute start of this function's bytecodes
                    // = functionPos + FunctionPoint[i][j]
                    //   (function_point is offset from function table start)
                    int funcStart = functionPos + fs.FunctionPoint[i][j];

                    var fn = new FieldScriptFunction { Length = length };
                    fn.Read(data, ref funcStart);
                    fs.Functions[i][j] = fn;
                }
            }

            return fs;
        }

        // ── Encode ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Encodes all in-memory structs back into a field script binary.
        ///
        /// Uses stored EntryOffset / EntrySize / FunctionPoint values verbatim —
        /// call <see cref="RecomputeOffsets"/> first if function bytecodes have been
        /// modified to bring those tables up to date.
        /// </summary>
        public byte[] Encode()
        {
            // Compute output size: at minimum covers header + entry table.
            // Extend to fit each non-empty entry body.
            int outputSize = EntryPos + EntryCount * EntryRowBytes;
            for (int i = 0; i < EntryCount; i++)
                if (EntrySize[i] > 0)
                    outputSize = Math.Max(outputSize, EntryPos + EntryOffset[i] + EntrySize[i]);

            // Preserve original file length in case it is larger (e.g., trailing data).
            outputSize = Math.Max(outputSize, _originalFileLength);

            byte[] output = new byte[outputSize]; // zero-initialized — handles all padding

            // ── Header ────────────────────────────────────────────────────────
            WriteU16(output, 0, MagicNumber);
            output[2] = HeaderUnknown1;
            output[3] = (byte)EntryCount;
            for (int i = 0; i < UnknownPairCount; i++)
            {
                output[4 + i * 2]     = HeaderUnknown2[i];
                output[4 + i * 2 + 1] = HeaderUnknown3[i];
            }
            Array.Copy(HeaderName, 0, output, 44, NameLength);

            // ── Entry table ───────────────────────────────────────────────────
            for (int i = 0; i < EntryCount; i++)
            {
                int t = EntryPos + i * EntryRowBytes;
                WriteU16(output, t,     EntryOffset[i]);
                WriteU16(output, t + 2, EntrySize[i]);
                output[t + 4] = EntryLocalVar[i];
                output[t + 5] = EntryFlag[i];
                // t+6, t+7 = zero16 — already zero
            }

            // ── Entry bodies ──────────────────────────────────────────────────
            for (int i = 0; i < EntryCount; i++)
            {
                if (EntrySize[i] == 0) continue;

                int localEntryPos = EntryPos + EntryOffset[i];
                output[localEntryPos]     = EntryType[i];
                output[localEntryPos + 1] = EntryFunctionCount[i];

                int functionPos = localEntryPos + 2;
                int funcCount   = EntryFunctionCount[i];

                // Function table
                for (int j = 0; j < funcCount; j++)
                {
                    WriteU16(output, functionPos + j * 4,     FunctionType[i][j]);
                    WriteU16(output, functionPos + j * 4 + 2, FunctionPoint[i][j]);
                }

                // Function bytecodes
                // Each function is written at functionPos + FunctionPoint[i][j]
                // (mirrors HW's SEEK(f, function_pos, function_point[i][j]) before Write).
                for (int j = 0; j < funcCount; j++)
                {
                    var funcBytes = new List<byte>();
                    Functions[i][j].Write(funcBytes);

                    int dest = functionPos + FunctionPoint[i][j];
                    for (int k = 0; k < funcBytes.Count; k++)
                        output[dest + k] = funcBytes[k];
                }
                // Bytes from end of last function to localEntryPos + EntrySize[i]
                // remain zero — matching HW's padding write.
            }

            return output;
        }

        // ── Offset recomputation ───────────────────────────────────────────────

        /// <summary>
        /// Recomputes <see cref="EntryFunctionCount"/>, <see cref="FunctionPoint"/>,
        /// <see cref="EntrySize"/>, <see cref="EntryOffset"/>, and the internal file
        /// length from the current state of <see cref="Functions"/>.
        ///
        /// Call this after modifying function bytecodes before calling
        /// <see cref="Encode"/> to produce a structurally valid output.
        /// </summary>
        public void RecomputeOffsets()
        {
            // Pass 1: recompute function_point and entry_size per entry.
            for (int i = 0; i < EntryCount; i++)
            {
                if (EntrySize[i] == 0) continue;

                int funcCount = Functions[i].Length;
                EntryFunctionCount[i] = (byte)funcCount;

                FunctionPoint[i] = new ushort[funcCount];

                // function_point[0] = 4*funcCount (function table itself takes 4 bytes × N)
                // function_point[j] = function_point[0] + Σ(length[0..j-1])
                int offset = 4 * funcCount;
                for (int j = 0; j < funcCount; j++)
                {
                    FunctionPoint[i][j] = (ushort)offset;
                    offset += Functions[i][j].ComputeEncodedSize();
                }

                // From HW:  entry_size = function_point[last] + func[last].length + 2
                //         = cumulative_offset + 2
                // (The -2 in the last-length formula accounts for entry_type + entry_function_amount
                //  which are counted in entry_size but not in function_point offsets.)
                EntrySize[i] = (ushort)(offset + 2);
            }

            // Pass 2: recompute entry_offset — pack entries sequentially after the table.
            int entryTableBytes = EntryCount * EntryRowBytes; // relative to EntryPos
            int cursor = entryTableBytes;
            for (int i = 0; i < EntryCount; i++)
            {
                if (EntrySize[i] == 0) continue;
                EntryOffset[i] = (ushort)cursor;
                cursor += EntrySize[i];
            }

            _originalFileLength = EntryPos + cursor;
        }

        // ── Binary helpers ─────────────────────────────────────────────────────

        private static ushort ReadU16(byte[] data, int offset)
            => (ushort)(data[offset] | (data[offset + 1] << 8));

        private static void WriteU16(byte[] output, int offset, ushort value)
        {
            output[offset]     = (byte)(value & 0xFF);
            output[offset + 1] = (byte)((value >> 8) & 0xFF);
        }
    }
}
