// StiltzkinsBag.Core/Parsing/FieldScriptOperation.cs
//
// Port of HW's ScriptOperation and the MACRO_SCRIPT_IOFUNCTION_OPREAD/OPWRITE
// macros (Scripts.cpp).
//
// A field script operation consists of:
//   • An opcode (1–N bytes in the stream; extended opcodes are preceded by 0xFF prefixes)
//   • Optional header bytes specific to certain opcode families
//   • Zero or more FieldScriptArgument values
//
// ── Five decode paths (matching HW macro precedence) ─────────────────────────────
//
//  Path 1 — Extended opcode prefix (0xFF accumulation)
//    Detected when the first opcode byte is 0xFF. Each 0xFF increments the opcode
//    by 0x100 and advances one byte. The first non-0xFF byte is the low byte.
//    E.g., stream [FF][02] → opcode 0x102 (BSACTIVE). [FF][FF][02] → opcode 0x202.
//    All extended opcodes (0x100–0x111) are in the standard opcode table and decoded
//    via Path 5 after the prefix is resolved.
//
//  Path 2 — JMP_SWITCHEX (opcode 0x06)
//    Header: size_byte (uint8, case count N).
//    Args:   [default_jump: int16][N × (case_value: uint16, jump: int16)]
//    Total arg bytes: 2 + N*4. All args stored as constants.
//    ByteSize: 1(opcode) + 1(size_byte) + 2 + N*4.
//
//  Path 3 — JMP_SWITCH (opcode 0x0B)
//    Header: size_byte (uint8, case count N).
//    Args:   [start_value: uint16][default_jump: int16][N × jump: int16]
//    Total arg bytes: 2 + 2 + N*2. All args stored as constants.
//    ByteSize: 1(opcode) + 1(size_byte) + 4 + N*2.
//
//  Path 4 — SetRegion (opcode 0x29)
//    Header: vararg_flag (uint8) + size_byte (uint8, vertex count N).
//    Args:   N × 4-byte vertex (signed X, signed Y packed as one 4-byte arg).
//            Each arg's isVar = (vararg_flag & (1 << i)) != 0.
//    ByteSize: 1(opcode) + 1(vararg_flag) + 1(size_byte) + sum(arg.ByteSize).
//
//  Path 5 — Standard table lookup (all other opcodes)
//    Looks up the opcode in FieldScriptOpcodeTable.
//    Unknown opcode → ArgumentException.
//    UseVarArg = true:  read vararg_flag byte (1 byte), then for each arg:
//                        bit i of vararg_flag clear → constant (ArgLengths[i] bytes);
//                        bit i of vararg_flag set   → VarOp expression.
//    UseVarArg = false: for each arg:
//                        ArgLengths[i] > 0 → constant;
//                        ArgLengths[i] == 0 → VarOp expression (only valid for opcode 0x05 set).
//
// ── Opcode 0x0D (unknown switch variant) ────────────────────────────────────────
//
// HW comment: "Steam seems to handle it like a JMP_SWITCH with a short instead of
// a char (number of cases)." Format assumed:
//    [size_lo][size_hi][start_value: uint16][default_jump: int16][N × jump: int16]
// ByteSize: 1(opcode) + 2(size) + 2(start) + 2(default) + N*2.
// UNCONFIRMED — flagged in code. If round-trip tests pass on fields containing 0x0D,
// the format is validated.
//
// ── Round-trip guarantee ──────────────────────────────────────────────────────────
//
// Read(data, ref pos) followed by Write(output) must reproduce the exact bytes that
// were consumed. Verified by FieldScriptRoundTripTests.

using System;
using System.Collections.Generic;

namespace StiltzkinsBag.Core.Parsing;

/// <summary>
/// A decoded field script operation (opcode + arguments).
/// Port of HW's <c>ScriptOperation</c> (Scripts.cpp).
/// </summary>
public sealed class FieldScriptOperation
{
    // ── Properties ────────────────────────────────────────────────────────────────

    /// <summary>The fully-resolved opcode ID (e.g., 0x102 for extended opcode FF 02).</summary>
    public uint Opcode { get; private init; }

    /// <summary>
    /// For Path 4 (SetRegion 0x29) and Path 5 UseVarArg=true: the vararg_flag byte.
    /// Bit i: 0 = argument i is a constant; 1 = argument i is a VarOp expression.
    /// Zero for all other paths.
    /// </summary>
    public byte VarargFlag { get; private init; }

    /// <summary>
    /// For Paths 2, 3, 4 (JMP_SWITCHEX, JMP_SWITCH, SetRegion): the inline count byte.
    /// For 0x0D (unconfirmed switch variant): low byte of the uint16 case count.
    /// Zero for all other paths.
    /// </summary>
    public byte SizeByte { get; private init; }

    /// <summary>
    /// For opcode 0x0D only: high byte of the uint16 case count.
    /// Zero for all other opcodes.
    /// </summary>
    public byte SizeByteHigh { get; private init; }

    /// <summary>Decoded arguments for this operation.</summary>
    public FieldScriptArgument[] Args { get; private init; } = Array.Empty<FieldScriptArgument>();

    /// <summary>
    /// Total bytes consumed from the input stream by this operation
    /// (opcode prefix bytes + header bytes + all argument bytes).
    /// </summary>
    public int ByteSize { get; private init; }

    // ── Factory: Read ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Reads one complete operation from <paramref name="data"/> starting at
    /// <paramref name="pos"/>. Advances <paramref name="pos"/> past all consumed bytes.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown on unknown opcode or data underrun — both indicate a corrupt field or
    /// an opcode table gap that must be fixed before this field can be decoded.
    /// </exception>
    public static FieldScriptOperation Read(byte[] data, ref int pos)
    {
        int startPos = pos;

        // ── Step 1: Resolve opcode, counting 0xFF prefix bytes ───────────────────
        if (pos >= data.Length)
            throw new ArgumentException(
                $"FieldScriptOperation: read at pos {pos} with no data remaining.");

        uint opcode = 0;
        byte opByte = data[pos++];
        while (opByte == 0xFF)
        {
            opcode += 0x100;
            if (pos >= data.Length)
                throw new ArgumentException(
                    $"FieldScriptOperation: 0xFF prefix at pos {pos - 1} not followed by opcode byte.");
            opByte = data[pos++];
        }
        opcode += opByte;

        int opByteSize = pos - startPos; // bytes consumed so far (opcode prefix + opcode byte)

        // ── Step 2: Decode per path ───────────────────────────────────────────────

        if (opcode == 0x06)
            return DecodeSwitchEx(opcode, opByteSize, data, ref pos, startPos);

        if (opcode == 0x0B)
            return DecodeSwitch(opcode, opByteSize, data, ref pos, startPos);

        if (opcode == 0x0D)
            return DecodeSwitchUnknown(opcode, opByteSize, data, ref pos, startPos);

        if (opcode == 0x29)
            return DecodeSetRegion(opcode, opByteSize, data, ref pos, startPos);

        return DecodeStandard(opcode, opByteSize, data, ref pos, startPos);
    }

    // ── Serialization: Write ──────────────────────────────────────────────────────

    /// <summary>
    /// Appends this operation's bytes to <paramref name="output"/>.
    /// Round-trips byte-identically with <see cref="Read"/>.
    /// </summary>
    public void Write(List<byte> output)
    {
        // ── Emit opcode bytes (with 0xFF prefixes for extended opcodes) ──────────
        uint remaining = Opcode;
        while (remaining >= 0x100)
        {
            output.Add(0xFF);
            remaining -= 0x100;
        }
        output.Add((byte)remaining);

        // ── Emit path-specific header bytes and args ─────────────────────────────
        if (Opcode == 0x06)
        {
            output.Add(SizeByte);                    // case count N
            foreach (var arg in Args) arg.Write(output);
        }
        else if (Opcode == 0x0B)
        {
            output.Add(SizeByte);                    // case count N
            foreach (var arg in Args) arg.Write(output);
        }
        else if (Opcode == 0x0D)
        {
            // UNCONFIRMED: uint16 case count as two bytes (lo, hi)
            output.Add(SizeByte);
            output.Add(SizeByteHigh);
            foreach (var arg in Args) arg.Write(output);
        }
        else if (Opcode == 0x29)
        {
            output.Add(VarargFlag);                  // vertex vararg flags
            output.Add(SizeByte);                    // vertex count N
            foreach (var arg in Args) arg.Write(output);
        }
        else
        {
            // Standard path
            if (!FieldScriptOpcodeTable.TryGet((int)Opcode, out var info))
                throw new InvalidOperationException(
                    $"FieldScriptOperation.Write: opcode 0x{Opcode:X} has no table entry. " +
                    "Cannot re-encode an operation that was not decoded from the table.");

            if (info.UseVarArg)
                output.Add(VarargFlag);

            foreach (var arg in Args) arg.Write(output);
        }
    }

    // ── Path 2: JMP_SWITCHEX (0x06) ──────────────────────────────────────────────
    //
    // Stream layout after opcode byte:
    //   [size_byte: 1]  [default_jump: 2]  [size_byte × (case: 2, jump: 2)]
    //
    // HW macro:
    //   arg_amount = 1 + size_byte * 2
    //   arg[0] = default_jump (int16)
    //   for i in 0..size_byte-1: arg[2i+1] = case_value (uint16), arg[2i+2] = jump (int16)
    //   size += 3 + size_byte * 4

    private static FieldScriptOperation DecodeSwitchEx(
        uint opcode, int opByteSize, byte[] data, ref int pos, int startPos)
    {
        if (pos >= data.Length)
            throw new ArgumentException(
                $"FieldScriptOperation: 0x06 JMP_SWITCHEX at {startPos}: size_byte missing.");

        byte sizeByte = data[pos++];
        int n = sizeByte;
        int argCount = 1 + n * 2;
        var args = new FieldScriptArgument[argCount];

        // arg[0]: default jump (int16, signed)
        args[0] = FieldScriptArgument.Read(data, ref pos, 2, isVar: false, isSigned: true);

        // arg[2i+1]: case value (uint16, unsigned); arg[2i+2]: jump (int16, signed)
        for (int i = 0; i < n; i++)
        {
            args[2 * i + 1] = FieldScriptArgument.Read(data, ref pos, 2, isVar: false, isSigned: false);
            args[2 * i + 2] = FieldScriptArgument.Read(data, ref pos, 2, isVar: false, isSigned: true);
        }

        return new FieldScriptOperation
        {
            Opcode   = opcode,
            SizeByte = sizeByte,
            Args     = args,
            ByteSize = pos - startPos
        };
    }

    // ── Path 3: JMP_SWITCH (0x0B) ────────────────────────────────────────────────
    //
    // Stream layout after opcode byte:
    //   [size_byte: 1]  [start_value: 2]  [default_jump: 2]  [size_byte × jump: 2]
    //
    // HW macro:
    //   arg_amount = 2 + size_byte
    //   arg[0] = start_value (uint16)
    //   arg[1..arg_amount-1] = default_jump then N case jumps (all int16, signed)
    //   size += 5 + size_byte * 2

    private static FieldScriptOperation DecodeSwitch(
        uint opcode, int opByteSize, byte[] data, ref int pos, int startPos)
    {
        if (pos >= data.Length)
            throw new ArgumentException(
                $"FieldScriptOperation: 0x0B JMP_SWITCH at {startPos}: size_byte missing.");

        byte sizeByte = data[pos++];
        int n = sizeByte;
        int argCount = 2 + n;
        var args = new FieldScriptArgument[argCount];

        // arg[0]: starting value (uint16, unsigned)
        args[0] = FieldScriptArgument.Read(data, ref pos, 2, isVar: false, isSigned: false);

        // arg[1..argCount-1]: default jump then N case jumps (int16, signed)
        for (int i = 1; i < argCount; i++)
            args[i] = FieldScriptArgument.Read(data, ref pos, 2, isVar: false, isSigned: true);

        return new FieldScriptOperation
        {
            Opcode   = opcode,
            SizeByte = sizeByte,
            Args     = args,
            ByteSize = pos - startPos
        };
    }

    // ── Path: 0x0D unknown switch variant ────────────────────────────────────────
    //
    // UNCONFIRMED FORMAT. HW comment: "Steam seems to handle it like a JMP_SWITCH
    // with a short instead of a char (number of cases)."
    //
    // Assumed layout (two-byte N, otherwise identical to JMP_SWITCH 0x0B):
    //   [size_lo: 1]  [size_hi: 1]  [start_value: 2]  [default_jump: 2]  [N × jump: 2]
    //
    // If round-trip tests pass on all fields containing 0x0D, this format is confirmed.
    // If they fail, the format assumption is wrong — update this method and re-run tests.

    private static FieldScriptOperation DecodeSwitchUnknown(
        uint opcode, int opByteSize, byte[] data, ref int pos, int startPos)
    {
        if (pos + 1 >= data.Length)
            throw new ArgumentException(
                $"FieldScriptOperation: 0x0D at {startPos}: uint16 size bytes missing.");

        byte sizeLo = data[pos++];
        byte sizeHi = data[pos++];
        int n = sizeLo | (sizeHi << 8);

        // UNCONFIRMED: same structure as JMP_SWITCH but 2-byte N
        int argCount = 2 + n;
        var args = new FieldScriptArgument[argCount];

        args[0] = FieldScriptArgument.Read(data, ref pos, 2, isVar: false, isSigned: false); // start
        for (int i = 1; i < argCount; i++)
            args[i] = FieldScriptArgument.Read(data, ref pos, 2, isVar: false, isSigned: true); // jumps

        return new FieldScriptOperation
        {
            Opcode       = opcode,
            SizeByte     = sizeLo,
            SizeByteHigh = sizeHi,
            Args         = args,
            ByteSize     = pos - startPos
        };
    }

    // ── Path 4: SetRegion (0x29) ─────────────────────────────────────────────────
    //
    // Stream layout after opcode byte:
    //   [vararg_flag: 1]  [size_byte: 1 = vertex count N]
    //   [N × vertex_arg: each vertex is decoded as a 4-byte arg with isVar per flag bit]
    //
    // HW macro:
    //   uint8_t flag = 1;
    //   arg[i].Read(f, 4, flag & vararg_flag, true);   // 4 bytes, isSigned=true (int16 X+Y)
    //   flag *= 2;
    //
    // Note: the 4 bytes of each vertex arg are decoded as a single FieldScriptArgument
    // with typeSize=4 (or as a VarOp expression if the vararg_flag bit is set). For
    // round-trip, we don't decompose into separate X/Y — we decode it exactly as HW does.

    private static FieldScriptOperation DecodeSetRegion(
        uint opcode, int opByteSize, byte[] data, ref int pos, int startPos)
    {
        if (pos + 1 >= data.Length)
            throw new ArgumentException(
                $"FieldScriptOperation: 0x29 SetRegion at {startPos}: vararg_flag or size_byte missing.");

        byte varargFlag = data[pos++];
        byte sizeByte   = data[pos++];
        int n = sizeByte;

        var args = new FieldScriptArgument[n];
        byte flag = 1;
        for (int i = 0; i < n; i++)
        {
            bool isVar = (flag & varargFlag) != 0;
            args[i] = FieldScriptArgument.Read(data, ref pos, 4, isVar, isSigned: true);
            flag = (byte)(flag * 2);
        }

        return new FieldScriptOperation
        {
            Opcode     = opcode,
            VarargFlag = varargFlag,
            SizeByte   = sizeByte,
            Args       = args,
            ByteSize   = pos - startPos
        };
    }

    // ── Path 5: Standard table lookup ────────────────────────────────────────────
    //
    // Covers all opcodes except 0x06, 0x0B, 0x0D, 0x29.
    // Extended opcodes (0x100+) arrive here after their 0xFF prefix is resolved.
    //
    // HW macro two sub-paths:
    //
    //   UseVarArg = true:
    //     Read vararg_flag (1 byte).
    //     For each arg i (0-indexed), with rolling bit flag starting at 1:
    //       (vararg_flag & flag) == 0 → constant, ArgLengths[i] bytes
    //       (vararg_flag & flag) != 0 → VarOp expression
    //
    //   UseVarArg = false:
    //     No vararg_flag byte.
    //     For each arg i:
    //       ArgLengths[i] > 0 → constant, ArgLengths[i] bytes
    //       ArgLengths[i] == 0 → VarOp expression  (only 0x05 "set" uses this)
    //
    // isSigned: for this decoder, we pass false for all standard args. The signed
    // flag only affects SignedValue interpretation, not encoding — round-trip is identical.
    // Callers that need signed values (e.g., future field entrance scanner) can re-interpret
    // after decoding.

    private static FieldScriptOperation DecodeStandard(
        uint opcode, int opByteSize, byte[] data, ref int pos, int startPos)
    {
        if (!FieldScriptOpcodeTable.TryGet((int)opcode, out var info))
            throw new ArgumentException(
                $"FieldScriptOperation: unknown opcode 0x{opcode:X} at stream position {startPos}. " +
                "Add it to FieldScriptOpcodeTable or handle it as a special case.");

        byte varargFlag = 0;
        var args = new FieldScriptArgument[info.ArgLengths?.Length ?? 0];

        if (info.UseVarArg)
        {
            // Read the vararg_flag byte
            if (pos >= data.Length)
                throw new ArgumentException(
                    $"FieldScriptOperation: opcode 0x{opcode:X} at {startPos}: " +
                    $"vararg_flag byte missing at pos {pos}.");

            varargFlag = data[pos++];

            byte flag = 1;
            for (int i = 0; i < args.Length; i++)
            {
                bool isVar = (varargFlag & flag) != 0;
                args[i] = FieldScriptArgument.Read(
                    data, ref pos,
                    typeSize: info.ArgLengths![i],
                    isVar:    isVar,
                    isSigned: false);
                flag = (byte)(flag * 2);
            }
        }
        else
        {
            // Fixed-format: ArgLengths[i] > 0 = constant, == 0 = VarOp
            for (int i = 0; i < args.Length; i++)
            {
                int len = info.ArgLengths![i];
                bool isVar = (len == 0);
                args[i] = FieldScriptArgument.Read(
                    data, ref pos,
                    typeSize: isVar ? 0 : len,
                    isVar:    isVar,
                    isSigned: false);
            }
        }

        return new FieldScriptOperation
        {
            Opcode     = opcode,
            VarargFlag = varargFlag,
            Args       = args,
            ByteSize   = pos - startPos
        };
    }
}
