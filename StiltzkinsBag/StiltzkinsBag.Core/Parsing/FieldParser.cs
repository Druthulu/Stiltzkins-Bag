using System;
using System.Collections.Generic;
using StiltzkinsBag.Core.Parsing;

namespace StiltzkinsBag.Parsing
{
    // ── Header / navigation types ────────────────────────────────────────────

    /// <summary>
    /// Parsed header of a FF9 Steam field script file (.eb.bytes).
    ///
    /// Binary layout confirmed from Hades Workshop Scripts.cpp MACRO_SCRIPT_IOFUNCTION
    /// and GetDataSize().
    ///
    ///   [0-1]    uint16 LE  magic_number
    ///   [2]      uint8      header_unknown1
    ///   [3]      uint8      entry_amount
    ///   [4-43]   uint8[40]  header_unknown2/3 interleaved (20 pairs)
    ///   [44-127] uint8[84]  header_name
    ///   [128]    entry table — entry_amount x 8 bytes:
    ///            { uint16 entry_offset, uint16 entry_size,
    ///              uint8 local_var, uint8 flag, uint16 zero }
    ///   At (128 + entry_offset[i]): entry body
    ///            { uint8 entry_type, uint8 func_amount,
    ///              func_amount x { uint16 func_type, uint16 func_point },
    ///              bytecode blobs, 4-byte-aligned }
    /// </summary>
    public sealed class FieldScriptHeader
    {
        public const int EntryPos = 128;

        public ushort MagicNumber { get; }
        public byte EntryAmount { get; }
        public FieldScriptEntry[] Entries { get; }

        public FieldScriptHeader(ushort magic, byte entryAmount, FieldScriptEntry[] entries)
        {
            MagicNumber = magic;
            EntryAmount = entryAmount;
            Entries = entries;
        }
    }

    /// <summary>One script entry (entity, region, object) within a field script.</summary>
    public sealed class FieldScriptEntry
    {
        public ushort EntryOffset { get; }
        public ushort EntrySize { get; }
        public byte LocalVar { get; }
        public byte Flag { get; }
        public byte EntryType { get; }
        public FieldScriptFunction[] Functions { get; }

        public FieldScriptEntry(ushort entryOffset, ushort entrySize, byte localVar, byte flag,
                                byte entryType, FieldScriptFunction[] functions)
        {
            EntryOffset = entryOffset;
            EntrySize = entrySize;
            LocalVar = localVar;
            Flag = flag;
            EntryType = entryType;
            Functions = functions;
        }

        public int AbsoluteBodyOffset => FieldScriptHeader.EntryPos + EntryOffset;
    }

    /// <summary>One function within a script entry.</summary>
    public sealed class FieldScriptFunction
    {
        public ushort FunctionType { get; }
        public int BytecodeOffset { get; }
        public int BytecodeLength { get; }

        public FieldScriptFunction(ushort functionType, int bytecodeOffset, int bytecodeLength)
        {
            FunctionType = functionType;
            BytecodeOffset = bytecodeOffset;
            BytecodeLength = bytecodeLength;
        }
    }

    // ── Item location types ──────────────────────────────────────────────────

    /// <summary>
    /// Classifies what a FieldItemLocation represents.
    /// The randomizer uses this to decide whether to treat CurrentValue as an
    /// item ID, a treasure-encoded value, a gil amount, or a display sync.
    /// </summary>
    public enum FieldLocationKind
    {
        /// <summary>
        /// AddItem(X, amount) opcode with a constant item ID X.
        /// Used by chests and direct story-award opcodes.
        /// CurrentValue is a plain item ID.
        /// </summary>
        DirectItem,

        /// <summary>
        /// set Treasure_Item = X in the 0x05 variable expression.
        /// Used by the generic hidden-item / exclamation-point pickup system.
        ///
        /// CurrentValue encoding (confirmed from generic pickup handler):
        ///   X less than 512              item ID = X
        ///   512 less than or equal to X less than 1000   card slot (card ID = X - 512)
        ///   1000 less than or equal to X less than 29999 gil amount = X - 1000
        ///   X == 29999                   disabled — scanner skips this
        /// </summary>
        TreasureItem,

        /// <summary>
        /// SetTextVariable(0, X) with constant X.
        /// Display-sync call always paired with a DirectItem or TreasureItem.
        /// CurrentValue matches the associated DirectItem/TreasureItem value.
        /// Must be patched to the same new value as its pair to keep display correct.
        /// </summary>
        TextSync,

        /// <summary>
        /// AddGil(X) opcode with a constant amount X (signed 3-byte).
        /// Used for direct scripted gil rewards ("found 138 gil on the floor").
        /// CurrentValue is the raw gil amount.
        /// </summary>
        DirectGil,
    }

    /// <summary>A single patchable value location found in a field script's bytecode.</summary>
    public sealed class FieldItemLocation
    {
        /// <summary>Absolute byte offset in the file where the value begins.</summary>
        public int FileOffset { get; }
        /// <summary>Current value — interpretation depends on LocationKind.</summary>
        public int CurrentValue { get; }
        /// <summary>Byte width of the value: 2 for item/treasure/card, 3 for direct gil.</summary>
        public int ArgByteWidth { get; }
        /// <summary>What this location represents — drives randomizer behavior.</summary>
        public FieldLocationKind LocationKind { get; }
        /// <summary>
        /// Number of copies given by this AddItem call.
        /// Always 1 for TreasureItem / TextSync / DirectGil locations.
        /// For DirectItem: the constant count argument read from the bytecode,
        /// or 1 if the count argument is a variable expression.
        /// </summary>
        public int ItemCount { get; }

        public FieldItemLocation(int fileOffset, int currentValue,
                                 int argByteWidth, FieldLocationKind locationKind,
                                 int itemCount = 1)
        {
            FileOffset = fileOffset;
            CurrentValue = currentValue;
            ArgByteWidth = argByteWidth;
            LocationKind = locationKind;
            ItemCount = itemCount > 0 ? itemCount : 1;
        }

        /// <summary>True if this TreasureItem encodes a plain item ID (X less than 512).</summary>
        public bool TreasureIsItem =>
            LocationKind == FieldLocationKind.TreasureItem && CurrentValue < 512;

        /// <summary>True if this TreasureItem encodes a card slot (512 to 999 inclusive).</summary>
        public bool TreasureIsCard =>
            LocationKind == FieldLocationKind.TreasureItem
            && CurrentValue >= 512 && CurrentValue < 1000;

        /// <summary>True if this TreasureItem encodes a gil amount (1000 to 29998 inclusive).</summary>
        public bool TreasureIsGil =>
            LocationKind == FieldLocationKind.TreasureItem
            && CurrentValue >= 1000 && CurrentValue < 29999;

        /// <summary>For TreasureIsGil: the actual gil amount encoded in CurrentValue.</summary>
        public int TreasureGilAmount => CurrentValue - 1000;
    }

    // ── Parser ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Parses and patches FF9 Steam field script files (.eb.bytes) extracted from p0data7.bin.
    ///
    /// ── Sequential opcode decoding ───────────────────────────────────────────
    ///
    /// This parser uses the complete opcode table from Hades Workshop's
    /// Database_Script.cpp (<see cref="FieldScriptOpcodeTable"/>) to walk bytecode
    /// sequentially. Each opcode's byte layout is fully specified — argument counts
    /// and widths are known. This eliminates the false positives that occurred when
    /// the previous pattern-matching approach misidentified argument bytes of other
    /// opcodes as AddItem (0x48) opcodes.
    ///
    /// Key insight from opcode table:
    ///   AddItem (0x48): item arg = uint16 (2 bytes), count arg = uint8 (1 byte)
    ///   The previous implementation read count as uint16, consuming 1 byte too many
    ///   and landing on wrong instruction boundaries for subsequent opcodes.
    ///
    /// When an unknown opcode is encountered, or a variable-length switch opcode
    /// (0x06, 0x0B, 0x0D, 0x29) is encountered, scanning of that function stops.
    /// Items reported up to that point are still returned.
    ///
    /// ── Item pickup mechanisms ────────────────────────────────────────────────
    ///
    /// CHESTS (TreasureItem system):
    ///   Opcode 0x05 (set): sets Treasure_Item variable via VarOp expression.
    ///   VarOp pattern: [D8][E0][7D][lo][hi][2C][7F]
    ///     D8 = VAR_GenInt16_ (variable category)
    ///     E0 = variable ID 224 (Treasure_Item)
    ///     7D = const short (2-byte value follows)
    ///     2C = "=" assignment operator
    ///     7F = terminate
    ///   The patchable value is at the [lo][hi] position.
    ///
    /// DIRECT ITEMS (AddItem opcode):
    ///   Opcode 0x48 (AddItem): vararg_flag, then item uint16 + count uint8.
    ///   Recorded when item arg is a constant (vararg_flag bit 0 = 0).
    ///
    /// DISPLAY SYNC (SetTextVariable):
    ///   Opcode 0x66 (SetTextVariable): slot uint8 + value uint16.
    ///   Recorded as TextSync when slot arg is constant == 0 and value arg is constant.
    ///
    /// GIL REWARDS (AddGil):
    ///   Opcode 0xCE (AddGil): 3-byte signed int.
    ///   Recorded when arg is constant.
    /// </summary>
    public static class FieldParser
    {
        // ── Treasure_Item VarOp pattern ───────────────────────────────────────
        // These bytes identify the "set Treasure_Item = X" expression:
        //   [D8] VAR_GenInt16_ category byte
        //   [E0] variable ID = 224 = Treasure_Item
        //   [7D] const short (2 bytes follow = the value)
        //   [2C] "=" assignment operator
        //   [7F] terminate
        private const byte VarCatInt16 = 0xD8;        // VAR_GenInt16_
        private const byte VarIdTreasureItem = 0xE0;  // Treasure_Item variable ID = 224
        private const byte VarConstShort = 0x7D;      // const short token
        private const byte VarAssign = 0x2C;          // "=" operator
        private const byte VarTerminate = 0x7F;       // terminate

        // Treasure sentinel: values >= this must not be randomized
        // 29999 = disabled/key-item marker; 64776, 64785 = dead-code "empty chest"
        private const int TreasureDisabled = 29999;

        // Maximum plausible item count per AddItem call.
        // Vanilla FFIX AddItem gives at most 8 copies (Straw Hat, Sandals, Pearl Armlet).
        // With correct sequential decoding, false giant counts should not occur,
        // but we retain this cap as a safety net.
        private const int MaxPlausibleItemCount = 9;

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Parses the header and entry/function navigation table.
        /// Does not scan bytecode — use FindItemLocations for that.
        /// </summary>
        /// <exception cref="ArgumentNullException">file is null.</exception>
        /// <exception cref="ArgumentException">File is too short or structurally invalid.</exception>
        public static FieldScriptHeader ParseHeader(byte[] file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (file.Length < FieldScriptHeader.EntryPos)
                throw new ArgumentException(
                    $"File is {file.Length} bytes — too short for field script header " +
                    $"(minimum {FieldScriptHeader.EntryPos}).", nameof(file));

            ushort magic = ReadUInt16LE(file, 0);
            byte entryAmount = file[3];

            int entryTableEnd = FieldScriptHeader.EntryPos + entryAmount * 8;
            if (file.Length < entryTableEnd)
                throw new ArgumentException(
                    $"File ({file.Length} B) too short for {entryAmount}-entry table " +
                    $"(need {entryTableEnd} B).", nameof(file));

            var entries = new FieldScriptEntry[entryAmount];

            for (int i = 0; i < entryAmount; i++)
            {
                int t = FieldScriptHeader.EntryPos + i * 8;
                ushort entOff = ReadUInt16LE(file, t);
                ushort entSize = ReadUInt16LE(file, t + 2);
                byte localVar = file[t + 4];
                byte flag = file[t + 5];

                if (entSize == 0)
                {
                    entries[i] = new FieldScriptEntry(entOff, entSize, localVar, flag,
                                                      0xFF, Array.Empty<FieldScriptFunction>());
                    continue;
                }

                int bodyStart = FieldScriptHeader.EntryPos + entOff;
                if (bodyStart + 2 > file.Length)
                    throw new ArgumentException(
                        $"Entry {i}: body at {bodyStart} overflows file.", nameof(file));

                byte entryType = file[bodyStart];
                byte funcCount = file[bodyStart + 1];

                int functionPos = bodyStart + 2;
                if (functionPos + funcCount * 4 > file.Length)
                    throw new ArgumentException(
                        $"Entry {i}: function table ({funcCount} functions) overflows file.",
                        nameof(file));

                ushort[] funcTypes = new ushort[funcCount];
                ushort[] funcPoints = new ushort[funcCount];
                for (int j = 0; j < funcCount; j++)
                {
                    funcTypes[j] = ReadUInt16LE(file, functionPos + j * 4);
                    funcPoints[j] = ReadUInt16LE(file, functionPos + j * 4 + 2);
                }

                var functions = new FieldScriptFunction[funcCount];
                for (int j = 0; j < funcCount; j++)
                {
                    int bcOffset = functionPos + funcPoints[j];
                    int bcLength = (j + 1 < funcCount)
                        ? funcPoints[j + 1] - funcPoints[j]
                        : entSize - funcPoints[j] - 2;
                    if (bcLength < 0) bcLength = 0;
                    functions[j] = new FieldScriptFunction(funcTypes[j], bcOffset, bcLength);
                }

                entries[i] = new FieldScriptEntry(entOff, entSize, localVar, flag,
                                                  entryType, functions);
            }

            return new FieldScriptHeader(magic, entryAmount, entries);
        }

        /// <summary>
        /// Scans all function bodies using sequential opcode decoding and returns
        /// every patchable item/gil location.
        ///
        /// Returns:
        ///   TreasureItem — set Treasure_Item = X (hidden/exclamation-point pickups)
        ///   DirectItem   — AddItem(X, n) with constant X (chests, direct story gives)
        ///   TextSync     — SetTextVariable(0, X) constant (display sync for both above)
        ///   DirectGil    — AddGil(X) constant (scripted gil on the floor, etc.)
        ///
        /// Functions containing switch opcodes (0x06, 0x0B, 0x0D, 0x29) are scanned
        /// up to the switch and then stopped — items before the switch are reported.
        /// </summary>
        public static IReadOnlyList<FieldItemLocation> FindItemLocations(byte[] file)
        {
            FieldScriptHeader header = ParseHeader(file);
            var results = new List<FieldItemLocation>();

            foreach (FieldScriptEntry entry in header.Entries)
                foreach (FieldScriptFunction func in entry.Functions)
                    ScanFunction(file, func.BytecodeOffset, func.BytecodeLength, results);

            return results.AsReadOnly();
        }

        /// <summary>
        /// Applies patches to a defensive copy of the file and returns the new bytes.
        /// Never modifies the input array.
        /// </summary>
        public static byte[] ApplyPatches(byte[] file,
            IEnumerable<(FieldItemLocation Location, int NewValue)> patches)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (patches == null) throw new ArgumentNullException(nameof(patches));

            byte[] result = (byte[])file.Clone();

            foreach (var (loc, newValue) in patches)
            {
                switch (loc.ArgByteWidth)
                {
                    case 2:
                        WriteUInt16LE(result, loc.FileOffset, (ushort)(newValue & 0xFFFF));
                        break;
                    case 3:
                        WriteInt24LE(result, loc.FileOffset, newValue);
                        break;
                    default:
                        throw new ArgumentException(
                            $"Unexpected ArgByteWidth {loc.ArgByteWidth} at offset {loc.FileOffset}.",
                            nameof(patches));
                }
            }

            return result;
        }

        // ── Sequential function scanner ───────────────────────────────────────

        /// <summary>
        /// Sequentially decodes one function body using the HW opcode table.
        ///
        /// For each opcode encountered:
        ///   1. Look up in FieldScriptOpcodeTable.
        ///   2. If IsSwitch → stop scanning this function.
        ///   3. If unknown → stop scanning this function (conservative, no false positives).
        ///   4. Otherwise, record any item/gil locations, then advance past all arguments.
        ///
        /// Advancing rules:
        ///   UseVarArg=true  → read vararg_flag byte; for each arg: if flag bit=0 read
        ///                     ArgLengths[i] bytes, if flag bit=1 skip VarOp expression.
        ///   UseVarArg=false → for each arg: if ArgLengths[i]=0 skip VarOp expression,
        ///                     else read ArgLengths[i] bytes directly.
        /// </summary>
        private static void ScanFunction(byte[] file, int start, int length,
                                         List<FieldItemLocation> results)
        {
            if (length <= 0) return;
            int end = Math.Min(start + length, file.Length);
            int pos = start;

            while (pos < end)
            {
                if (pos >= file.Length) return;
                byte rawOpcode = file[pos++];

                // ── Extended opcode (0xFF prefix) ─────────────────────────────
                int opcode;
                if (rawOpcode == 0xFF)
                {
                    if (pos >= end) return;
                    int ext = file[pos++];
                    opcode = 0x100 + ext;
                }
                else
                {
                    opcode = rawOpcode;
                }

                // ── Lookup opcode in table ─────────────────────────────────────
                if (!FieldScriptOpcodeTable.TryGet(opcode, out var info))
                    return; // Unknown opcode — stop scanning this function

                if (info.IsSwitch)
                    return; // Variable-length opcode with no computable extent — stop

                int[] argLengths = info.ArgLengths;
                int argCount = argLengths?.Length ?? 0;

                // ── Computed-extent switch opcodes ─────────────────────────────
                // JMP_SWITCH (0x0B) and JMP_SWITCHEX (0x06) have variable-length
                // jump tables, but N (the case count) is encoded in the bytecode
                // immediately after the opcode. We read N, compute the exact byte
                // extent, advance past the entire table, and CONTINUE scanning.
                // This allows the scanner to reach AddItem calls that live in
                // functions using switch dispatch (e.g., Gastro Fork reward).
                //
                // JMP_SWITCH (0x0B):
                //   [opcode][N:1][start-value:2][default-jump:2][N × jump:2 each]
                //   Advance: 1 + 2 + 2 + N*2  = 5 + N*2 bytes
                //   Confirmed from field 656: 0B 08 00 00 65 00 [16 bytes] ✓
                //
                // JMP_SWITCHEX (0x06):
                //   [opcode][N:1][default-jump:2][N × (case-value:2 + jump:2)]
                //   Advance: 1 + 2 + N*4 = 3 + N*4 bytes
                //   Confirmed from field 656: 06 04 60 02 [16 bytes] ✓

                if (opcode == 0x0B) // JMP_SWITCH
                {
                    if (pos >= end) return;
                    int n = file[pos]; // case count
                    pos += 1 + 2 + 2 + n * 2; // N + start + default + N*jump
                    pos = Math.Min(pos, end);
                    continue;
                }

                if (opcode == 0x06) // JMP_SWITCHEX
                {
                    if (pos >= end) return;
                    int n = file[pos]; // case count
                    pos += 1 + 2 + n * 4; // N + default + N*(case+jump)
                    pos = Math.Min(pos, end);
                    continue;
                }

                // ── Handle target opcodes before advancing ─────────────────────

                if (opcode == 0x05 && argCount == 1 && argLengths![0] == 0)
                {
                    // set opcode — scan VarOp expression for Treasure_Item assignment
                    pos = ScanVarOpForTreasureItem(file, pos, end, results);
                    continue; // ScanVarOpForTreasureItem already consumed the expression
                }

                if (opcode == 0x48 && info.UseVarArg && argCount == 2)
                {
                    // AddItem — item=uint16 (arg 0), count=uint8 (arg 1)
                    pos = RecordAndAdvanceAddItem(file, pos, end, results);
                    continue;
                }

                if (opcode == 0x66 && info.UseVarArg && argCount == 2)
                {
                    // SetTextVariable — slot=uint8 (arg 0), value=uint16 (arg 1)
                    pos = RecordAndAdvanceSetTextVariable(file, pos, end, results);
                    continue;
                }

                if (opcode == 0xCE && info.UseVarArg && argCount == 1)
                {
                    // AddGil — amount=int24 (arg 0)
                    pos = RecordAndAdvanceAddGil(file, pos, end, results);
                    continue;
                }

                // ── Generic advance ────────────────────────────────────────────
                pos = AdvanceInstruction(file, pos, end, info);
            }
        }

        // ── Target opcode handlers ────────────────────────────────────────────

        /// <summary>
        /// Scans the VarOp expression for opcode 0x05 (set).
        /// Records a TreasureItem location if the Treasure_Item ASSIGNMENT pattern
        /// is found within the expression.
        /// Returns the position after the terminating 0x7F.
        ///
        /// The full pattern that must match (confirmed from HW binary analysis and
        /// the old FieldParser's verified byte pattern):
        ///   [D8] VAR_GenInt16_ category byte
        ///   [E0] variable ID = 224 = Treasure_Item
        ///   [7D] const short token (2 bytes follow)
        ///   [lo][hi] the value (patchable)
        ///   [2C] "=" assignment operator   ← MUST be present to distinguish write from read
        ///   [7F] terminate                 ← MUST be present to confirm full expression end
        ///
        /// Without [2C] and [7F] checks, READ expressions involving Treasure_Item
        /// (e.g., "if Treasure_Item == X") would also match, causing false recordings
        /// and position corruption that cascades into further false positives.
        /// </summary>
        private static int ScanVarOpForTreasureItem(byte[] file, int pos, int end,
                                                     List<FieldItemLocation> results)
        {
            while (pos < end)
            {
                if (pos >= file.Length) break;
                byte token = file[pos];

                if (token == VarTerminate) // 0x7F
                {
                    pos++; // consume terminate
                    break;
                }

                // Check for the complete Treasure_Item ASSIGNMENT pattern:
                //   [D8][E0][7D][lo][hi][2C][7F]
                // All 7 bytes must match to distinguish a write (set Treasure_Item = X)
                // from a read (e.g., if Treasure_Item == X) or other uses.
                if (token == VarCatInt16               // 0xD8 = VAR_GenInt16_
                    && pos + 6 < end
                    && file[pos + 1] == VarIdTreasureItem  // 0xE0 = variable ID 224
                    && file[pos + 2] == VarConstShort      // 0x7D = const short
                    && file[pos + 5] == VarAssign          // 0x2C = "=" assignment operator
                    && file[pos + 6] == VarTerminate)      // 0x7F = terminate
                {
                    int value = ReadUInt16LE(file, pos + 3);

                    if (value < TreasureDisabled)
                    {
                        // Record TreasureItem at the position of the 2-byte value
                        results.Add(new FieldItemLocation(
                            pos + 3, value, 2, FieldLocationKind.TreasureItem));
                    }

                    // Advance past the full 7-byte pattern (including the 0x7F terminator)
                    pos += 7;
                    break;
                }

                // Not the target pattern — consume this token via the VarOp skip table
                pos++; // consume token byte itself
                int skipBytes = FieldScriptOpcodeTable.GetVarOpSkipBytes(token);
                pos += skipBytes;
            }

            return pos;
        }

        /// <summary>
        /// Records DirectItem from AddItem (0x48) and advances past its arguments.
        /// Called with pos pointing to the vararg_flag byte (after the opcode byte).
        /// Returns the new position after all arguments.
        ///
        /// AddItem: { UseVarArg=true, ArgLengths=[2, 1] }
        ///   Arg 0: item ID (2 bytes when constant)
        ///   Arg 1: count  (1 byte  when constant)
        /// </summary>
        private static int RecordAndAdvanceAddItem(byte[] file, int pos, int end,
                                                   List<FieldItemLocation> results)
        {
            if (pos >= end) return pos;
            byte varargFlag = file[pos++];

            bool itemIsConst = (varargFlag & 0x01) == 0;
            bool countIsConst = (varargFlag & 0x02) == 0;

            // Read arg 0 (item ID, 2 bytes if constant)
            int itemId = -1;
            int itemOffset = pos;
            if (itemIsConst)
            {
                if (pos + 1 >= end) return end; // truncated
                itemId = ReadUInt16LE(file, pos);
                pos += 2;
            }
            else
            {
                pos = SkipVarOpExpression(file, pos, end);
            }

            // Read arg 1 (count, 1 byte if constant)
            int itemCount = 1;
            if (countIsConst)
            {
                if (pos >= end) return end; // truncated
                int rawCount = file[pos];
                itemCount = Math.Clamp(rawCount, 1, MaxPlausibleItemCount);
                pos += 1;
            }
            else
            {
                pos = SkipVarOpExpression(file, pos, end);
            }

            // Record only if item was a valid constant
            if (itemIsConst && itemId >= 0)
            {
                results.Add(new FieldItemLocation(
                    itemOffset, itemId, 2, FieldLocationKind.DirectItem, itemCount));
            }

            return pos;
        }

        /// <summary>
        /// Records TextSync from SetTextVariable (0x66) and advances past its arguments.
        /// Called with pos pointing to the vararg_flag byte.
        /// Returns the new position after all arguments.
        ///
        /// SetTextVariable: { UseVarArg=true, ArgLengths=[1, 2] }
        ///   Arg 0: slot ID  (1 byte  when constant — must be 0 to record)
        ///   Arg 1: value    (2 bytes when constant)
        /// </summary>
        private static int RecordAndAdvanceSetTextVariable(byte[] file, int pos, int end,
                                                            List<FieldItemLocation> results)
        {
            if (pos >= end) return pos;
            byte varargFlag = file[pos++];

            bool slotIsConst = (varargFlag & 0x01) == 0;
            bool valueIsConst = (varargFlag & 0x02) == 0;

            // Read arg 0 (slot, 1 byte if constant)
            int slot = -1;
            if (slotIsConst)
            {
                if (pos >= end) return end;
                slot = file[pos];
                pos += 1;
            }
            else
            {
                pos = SkipVarOpExpression(file, pos, end);
            }

            // Read arg 1 (value, 2 bytes if constant)
            int value = -1;
            int valueOffset = pos;
            if (valueIsConst)
            {
                if (pos + 1 >= end) return end;
                value = ReadUInt16LE(file, pos);
                pos += 2;
            }
            else
            {
                pos = SkipVarOpExpression(file, pos, end);
            }

            // Record TextSync only when slot 0 and both args were constants
            if (slotIsConst && slot == 0 && valueIsConst && value >= 0)
            {
                results.Add(new FieldItemLocation(
                    valueOffset, value, 2, FieldLocationKind.TextSync));
            }

            return pos;
        }

        /// <summary>
        /// Records DirectGil from AddGil (0xCE) and advances past its argument.
        /// Called with pos pointing to the vararg_flag byte.
        /// Returns the new position after the argument.
        ///
        /// AddGil: { UseVarArg=true, ArgLengths=[3] }
        ///   Arg 0: amount (3-byte signed int when constant)
        /// </summary>
        private static int RecordAndAdvanceAddGil(byte[] file, int pos, int end,
                                                  List<FieldItemLocation> results)
        {
            if (pos >= end) return pos;
            byte varargFlag = file[pos++];

            bool amountIsConst = (varargFlag & 0x01) == 0;

            if (amountIsConst)
            {
                if (pos + 2 >= end) return end;
                int gilAmount = ReadInt24LE(file, pos);
                results.Add(new FieldItemLocation(
                    pos, gilAmount, 3, FieldLocationKind.DirectGil));
                pos += 3;
            }
            else
            {
                pos = SkipVarOpExpression(file, pos, end);
            }

            return pos;
        }

        // ── Generic instruction advance ───────────────────────────────────────

        /// <summary>
        /// Advances pos past all arguments of the instruction described by <paramref name="info"/>.
        /// Called with pos pointing to the first byte after the opcode byte (and vararg_flag,
        /// if the caller already consumed it). For target opcodes (0x05, 0x48, 0x66, 0xCE),
        /// the dedicated Record* methods handle advancing instead.
        /// </summary>
        private static int AdvanceInstruction(byte[] file, int pos, int end, OpcodeInfo info)
        {
            int[] argLengths = info.ArgLengths;
            if (argLengths == null || argLengths.Length == 0)
                return pos; // 0-arg opcode, nothing to advance past

            if (info.UseVarArg)
            {
                // Read vararg_flag, then advance per-arg based on flag bits
                if (pos >= end) return end;
                byte varargFlag = file[pos++];

                for (int i = 0; i < argLengths.Length; i++)
                {
                    if (pos >= end) return end;
                    bool isVarOp = (varargFlag & (1 << i)) != 0;
                    if (isVarOp)
                    {
                        pos = SkipVarOpExpression(file, pos, end);
                    }
                    else
                    {
                        pos = AdvanceFixedArg(file, pos, end, argLengths[i]);
                    }
                }
            }
            else
            {
                // No vararg_flag — each arg is either fixed-length or a VarOp expression
                for (int i = 0; i < argLengths.Length; i++)
                {
                    if (pos >= end) return end;
                    if (argLengths[i] == 0)
                        pos = SkipVarOpExpression(file, pos, end);
                    else
                        pos = AdvanceFixedArg(file, pos, end, argLengths[i]);
                }
            }

            return pos;
        }

        /// <summary>
        /// Advances pos past a fixed-length argument of <paramref name="byteCount"/> bytes.
        /// </summary>
        private static int AdvanceFixedArg(byte[] file, int pos, int end, int byteCount)
        {
            return Math.Min(pos + byteCount, end);
        }

        // ── VarOp expression skipper ──────────────────────────────────────────

        /// <summary>
        /// Skips a complete VarOp expression in the bytecode stream.
        /// Reads tokens until 0x7F (terminate). For each non-terminator token,
        /// consumes its associated data bytes per <see cref="FieldScriptOpcodeTable.GetVarOpSkipBytes"/>.
        /// Returns the new position (after the 0x7F).
        /// </summary>
        private static int SkipVarOpExpression(byte[] file, int pos, int end)
        {
            while (pos < end)
            {
                if (pos >= file.Length) break;
                byte token = file[pos++];

                if (token == VarTerminate) // 0x7F
                    break;

                int extra = FieldScriptOpcodeTable.GetVarOpSkipBytes(token);
                pos = Math.Min(pos + extra, end);
            }
            return pos;
        }

        // ── Binary helpers ─────────────────────────────────────────────────────

        private static ushort ReadUInt16LE(byte[] data, int offset)
            => (ushort)(data[offset] | (data[offset + 1] << 8));

        private static int ReadInt24LE(byte[] data, int offset)
        {
            int raw = data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16);
            if ((raw & 0x80_0000) != 0)
                raw |= unchecked((int)0xFF00_0000);
            return raw;
        }

        private static void WriteUInt16LE(byte[] data, int offset, ushort value)
        {
            data[offset] = (byte)(value & 0xFF);
            data[offset + 1] = (byte)((value >> 8) & 0xFF);
        }

        private static void WriteInt24LE(byte[] data, int offset, int value)
        {
            data[offset] = (byte)(value & 0xFF);
            data[offset + 1] = (byte)((value >> 8) & 0xFF);
            data[offset + 2] = (byte)((value >> 16) & 0xFF);
        }
    }
}