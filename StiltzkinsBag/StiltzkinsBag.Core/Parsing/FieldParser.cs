using System;
using System.Collections.Generic;

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

        public FieldItemLocation(int fileOffset, int currentValue,
                                 int argByteWidth, FieldLocationKind locationKind)
        {
            FileOffset = fileOffset;
            CurrentValue = currentValue;
            ArgByteWidth = argByteWidth;
            LocationKind = locationKind;
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
    /// How FF9 item pickups work (confirmed from HW script exports + binary analysis):
    ///
    /// CHESTS — direct constant opcodes:
    ///   SetTextVariable( 0, 240 )     // display item name (constant)
    ///   AddItem( 240, 1 )             // AddItem with CONSTANT arg — Pattern 2 catches this
    ///
    /// HIDDEN ITEMS (exclamation-point, Treasure variable system):
    ///   set Treasure_Item = 236       // 0x05 var expr — PRIMARY patchable value (Pattern 1)
    ///   SetTextVariable( 0, 236 )     // 0x66 display sync — SECONDARY patchable (Pattern 3)
    ///   RunScriptSync( 2, 250, 12 )   // calls generic pickup on player entry
    ///   // inside generic pickup (read-only from our perspective):
    ///   AddItem( Treasure_Item, 1 )   // is_var=true — scanner skips correctly
    ///
    /// Treasure_Item value encoding confirmed from generic pickup function:
    ///   X less than 512          item ID
    ///   512 to 999               card slot (= X - 512)
    ///   1000 to 29998            gil amount (= X - 1000)
    ///   29999                    disabled / key item — skip
    ///
    /// Binary pattern for set Treasure_Item = X (confirmed against evt_alex1_at_house_2_eb.bytes):
    ///   [05][D8 E0 7D][lo hi][2C 7F][66 00 00][lo hi]
    ///    SET var-prefix value suffix  SetTextVar(0,X)
    ///
    ///   VarOpList byte meanings (Database_Script.cpp VarOpList):
    ///     D8 = VAR_GenInt16_ (type 11, reads 1 byte ID)
    ///     E0 = 224 = Treasure_Item variable ID
    ///     7D = constant short (type 6, reads 2 bytes)
    ///     2C = '=' assignment operator (type 2)
    ///     7F = terminate (type -1)
    ///
    ///   Verified: 236 (Potion), 513 (Fang card), 1009 (9 gil) in house_2 bytes file.
    ///
    /// Unknown opcodes 0x102-0x10A appear in field scripts but have no arg definition in HW.
    /// This parser uses function_point[]/entry_size bounds + byte-pattern matching rather than
    /// sequential opcode parsing, so unknown opcodes do not affect correctness.
    /// </summary>
    public static class FieldParser
    {
        // ── Constants ─────────────────────────────────────────────────────────

        private const byte OpcodeSet = 0x05;
        private const byte OpcodeAddItem = 0x48;
        private const byte OpcodeSetTextVar = 0x66;
        private const byte OpcodeAddGil = 0xCE;

        // Treasure_Item var expression prefix bytes (D8 E0 7D):
        //   D8 = VAR_GenInt16_, E0 = id 224 (Treasure_Item), 7D = constant short
        private const byte VarPrefixCat = 0xD8;
        private const byte VarPrefixId = 0xE0; // 224 = Treasure_Item ID
        private const byte VarPrefixConst = 0x7D; // constant short

        // Treasure_Item var expression suffix bytes (2C 7F):
        //   2C = '=' assignment, 7F = terminate
        private const byte VarSuffixAssign = 0x2C;
        private const byte VarSuffixTerminate = 0x7F;

        // Sentinel: any Treasure_Item value >= 29999 must not be randomized.
        //   29999         = explicit disabled / key-item marker
        //   64776, 64785  = dead-code "empty chest" placeholders (engine room scripts)
        // The filter is >= rather than == to catch all sentinel/garbage values.
        private const int TreasureDisabled = 29999;

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
                // bytes t+6, t+7 are zero padding

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

                // function table starts immediately after entry_type + func_amount
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

                // HW bytecode length formula (from Scripts.cpp):
                //   func[j].length = funcPoints[j+1] - funcPoints[j]       (j < funcCount-1)
                //   func[j].length = entrySize - funcPoints[j] - 2         (last function)
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
        /// Scans all function bodies and returns every patchable item/gil location.
        ///
        /// Returns:
        ///   TreasureItem — set Treasure_Item = X (hidden/exclamation-point pickups)
        ///   DirectItem   — AddItem(X, n) with constant X (chests, direct story gives)
        ///   TextSync     — SetTextVariable(0, X) constant (display sync for both above)
        ///   DirectGil    — AddGil(X) constant (scripted gil on the floor, etc.)
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

        // ── Private scanner ───────────────────────────────────────────────────

        private static void ScanFunction(byte[] file, int start, int length,
                                         List<FieldItemLocation> results)
        {
            if (length <= 0) return;
            int end = Math.Min(start + length, file.Length);

            for (int pos = start; pos < end; pos++)
            {
                byte b = file[pos];

                // ── Pattern 1: set Treasure_Item = X ─────────────────────────
                //
                // The hidden-item system's primary patchable location.
                // Binary: [05][D8][E0][7D][lo][hi][2C][7F]
                //                             ^^^^    value (uint16 LE) at pos+4
                //
                // Immediately followed by the paired display sync:
                //         [66][00][00][lo][hi]
                //                      ^^^^   same value at pos+11
                //
                // Minimum span to match full pair: pos+12 must be within bounds.
                // We also accept the SET alone (without the paired SetTextVariable)
                // to handle any script variants where they are not directly adjacent.
                if (b == OpcodeSet)
                {
                    // Need at least 8 bytes for the SET expression alone
                    if (pos + 7 < end
                        && file[pos + 1] == VarPrefixCat
                        && file[pos + 2] == VarPrefixId
                        && file[pos + 3] == VarPrefixConst
                        && file[pos + 6] == VarSuffixAssign
                        && file[pos + 7] == VarSuffixTerminate)
                    {
                        int value = ReadUInt16LE(file, pos + 4);

                        // value >= TreasureDisabled (29999) catches:
                        //   29999         explicit disabled / key-item marker
                        //   64776, 64785  dead-code "empty chest" placeholders
                        if (value < TreasureDisabled)
                        {
                            // Record the TreasureItem location (bytes pos+4 and pos+5)
                            results.Add(new FieldItemLocation(
                                pos + 4, value, 2, FieldLocationKind.TreasureItem));

                            // Check for immediately-following SetTextVariable(0, X)
                            // Pattern: [66][00][00][lo][hi] — all 5 bytes starting at pos+8
                            if (pos + 12 < end
                                && file[pos + 8] == OpcodeSetTextVar
                                && file[pos + 9] == 0x00  // vararg_flag: both args constant
                                && file[pos + 10] == 0x00  // arg[0] slot = 0
                                && ReadUInt16LE(file, pos + 11) == value) // sanity: same value
                            {
                                results.Add(new FieldItemLocation(
                                    pos + 11, value, 2, FieldLocationKind.TextSync));

                                // Skip past both the SET (8 bytes) and the SetTextVar (5 bytes)
                                pos += 12;
                                continue;
                            }
                        }

                        // For sentinel values (>= 29999): skip SET + any immediately
                        // paired SetTextVariable(0, X) so Pattern 3 does not catch them.
                        if (pos + 12 < end
                            && file[pos + 8] == OpcodeSetTextVar
                            && file[pos + 9] == 0x00
                            && file[pos + 10] == 0x00
                            && ReadUInt16LE(file, pos + 11) == ReadUInt16LE(file, pos + 4))
                        {
                            pos += 12; // skip both instructions (loop adds 1 more)
                        }
                        else
                        {
                            pos += 7;  // skip just the SET expression
                        }
                        continue;
                    }
                }

                // ── Pattern 2: AddItem(X, amount) with constant X ─────────────
                //
                // Chests and direct story-award AddItem calls.
                // Binary: [48][vararg_flag][lo][hi][amount...]
                // vararg_flag bit 0 = 0 → arg[0] (item ID) is a constant.
                // Minimum span: 4 bytes.
                if (b == OpcodeAddItem && pos + 3 < end)
                {
                    byte varargFlag = file[pos + 1];
                    if ((varargFlag & 0x01) == 0)
                    {
                        int itemId = ReadUInt16LE(file, pos + 2);
                        results.Add(new FieldItemLocation(
                            pos + 2, itemId, 2, FieldLocationKind.DirectItem));
                    }
                    // Advance past opcode(1) + flag(1) + item(2); loop adds 1 more
                    pos += 3;
                    continue;
                }

                // ── Pattern 3: SetTextVariable(0, X) with both args constant ──
                //
                // Display sync for both Pattern 1 (treasure) and Pattern 2 (chest).
                // Pattern 1 pairs are already captured above; this additionally captures
                // any SetTextVariable that was NOT immediately adjacent to a SET expr
                // (e.g., the ones paired with direct AddItem chest calls).
                //
                // Binary: [66][vararg_flag][00][lo][hi]
                // vararg_flag bits 0 and 1 both clear = both args are constants.
                // arg[0] byte must be 0x00 (slot 0 = item display slot).
                // Minimum span: 5 bytes.
                //
                // If arg[0] is variable, its byte length is unknown so we cannot
                // safely find arg[1] — skip entirely.
                if (b == OpcodeSetTextVar && pos + 4 < end)
                {
                    byte varargFlag = file[pos + 1];
                    bool arg0IsVar = (varargFlag & 0x01) != 0;
                    bool arg1IsVar = (varargFlag & 0x02) != 0;

                    if (!arg0IsVar && !arg1IsVar && file[pos + 2] == 0x00)
                    {
                        int value = ReadUInt16LE(file, pos + 3);
                        results.Add(new FieldItemLocation(
                            pos + 3, value, 2, FieldLocationKind.TextSync));
                    }
                    // Advance past opcode(1) + flag(1) + slot(1) + value(2)
                    pos += 4;
                    continue;
                }

                // ── Pattern 4: AddGil(X) with constant X ──────────────────────
                //
                // Direct scripted gil rewards ("found 138 gil on the floor", etc.).
                // Binary: [CE][vararg_flag][3 bytes signed LE]
                // vararg_flag bit 0 = 0 → arg[0] is a constant.
                // Minimum span: 5 bytes.
                if (b == OpcodeAddGil && pos + 4 < end)
                {
                    byte varargFlag = file[pos + 1];
                    if ((varargFlag & 0x01) == 0)
                    {
                        int gilAmount = ReadInt24LE(file, pos + 2);
                        results.Add(new FieldItemLocation(
                            pos + 2, gilAmount, 3, FieldLocationKind.DirectGil));
                    }
                    // Advance past opcode(1) + flag(1) + amount(3)
                    pos += 4;
                    continue;
                }
            }
        }

        // ── Binary helpers ─────────────────────────────────────────────────────

        private static ushort ReadUInt16LE(byte[] data, int offset)
            => (ushort)(data[offset] | (data[offset + 1] << 8));

        /// <summary>
        /// Reads a 3-byte little-endian signed integer.
        /// AT_SPIN in HW source is a signed 3-byte value (arg_length=3, AT_SPIN type).
        /// </summary>
        private static int ReadInt24LE(byte[] data, int offset)
        {
            int raw = data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16);
            // Sign-extend from bit 23
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