// StiltzkinsBag.Core/Parsing/FieldScriptArgument.cs
//
// Port of HW's ScriptArgument (Scripts.cpp / Scripts.h).
//
// A field script argument has two forms:
//
//   Constant — a fixed number of bytes read as a little-endian unsigned integer.
//              TypeSize is 1, 2, or 4 bytes. IsSigned applies to how the caller
//              interprets the value; the raw bits are preserved unchanged in Value.
//
//   VarOp    — a variable-length expression built from VarOp tokens. All bytes
//              (tokens + inline data + the 0x7F terminator) are collected verbatim
//              into Var. No semantic interpretation is done here; round-trip fidelity
//              is the only goal.
//
// ── VarOp collection protocol (ported from MACRO_SCRIPT_IOFUNCTION_ARGREAD) ────
//
// 1. Read the first token byte, append to collection.
// 2. Look up the token in s_varOpExtraBytes — this gives the number of inline data
//    bytes that immediately follow the token in the stream.
// 3. Read and collect those inline data bytes.
// 4. Read the next token byte, append to collection.
// 5. If this byte is 0x7F (terminate, VarOpList type -1), stop.
//    Otherwise, go to step 2.
//
// The final Var[] includes the 0x7F terminator, matching HW's var.push_back layout.
//
// ── VarOp extra-bytes table (authoritative source: VarOpList in Database_Script.cpp) ──
//
// Each token byte maps to the number of inline data bytes that follow it, derived from
// VarOpList[token].type using the same if/else chain as the HW argread macro:
//
//   type  3: 1 byte
//   type  5: 1 byte  (no type-5 entries present in VarOpList; included for completeness)
//   type  6: 2 bytes  (token 0x7D = const short)
//   type  7: 4 bytes  (token 0x7E = const long)
//   type 10–19: 1 byte  (VAR_*Int8_, VAR_*Bool_, VAR_*Int16_, etc.; ObjectUID_, SV_, GetData_)
//   type 20–29: 2 bytes  (VARL_* list variable refs)
//   type 55: 2 bytes  (token 0x78 = GetEntryProperty; NOT 1 — Phase 9.1 skip table was wrong)
//   type 60: 3 bytes  (token 0xD3 = MemoriaVarCode_; NOT 1 — Phase 9.1 skip table was wrong)
//   all others (type 0, 1, 2, 50, 51, 100): 0 bytes
//
// IMPORTANT — VarOpTypeList vs VarOpList distinction:
//   VarOpTypeList (Database_Script.cpp) is a GUI/editor metadata list describing the
//   semantic argument types of type-50 stack-based functions (IsButton, GetHP, etc.).
//   It is NOT used by the binary decoder. Type-50 tokens are stack-based — their
//   "argument" is the preceding stack value, not an inline byte. The argread macro
//   has no case for vartype == 50 and falls through with zero extra bytes.
//
//   Phase 9.1's FieldScriptOpcodeTable.VarOpSkipTable incorrectly assigned 1 extra byte
//   to tokens 0x4F, 0x52, 0x53, 0x58, 0x59, 0x5A, 0x5B, 0x5C, 0x65, 0x6A, 0x6B, 0x6D,
//   0x6E, 0x6F, 0x70, 0x71 (all VarOpTypeList entries, all type 50 in VarOpList).
//   FieldScriptArgument corrects all of these.
//
// ── Round-trip guarantee ──────────────────────────────────────────────────────────
//
// Read(data, ref pos, ...) followed by Write(output) must reproduce the exact bytes
// that were read. For constants: little-endian Value re-encoded at TypeSize width.
// For VarOp: Var written verbatim.

using System;
using System.Collections.Generic;

namespace StiltzkinsBag.Core.Parsing;

/// <summary>
/// A decoded field script argument.
/// Port of HW's <c>ScriptArgument</c> (Scripts.cpp).
/// </summary>
public sealed class FieldScriptArgument
{
    // ── Properties ────────────────────────────────────────────────────────────────

    /// <summary>True if this argument is a VarOp expression; false if it is a constant.</summary>
    public bool IsVar { get; private init; }

    /// <summary>
    /// True if this constant argument should be interpreted as signed (AT_JUMP, AT_SPIN,
    /// AT_POSITION_X/Y/Z). Does not affect round-trip encoding — Value always stores the
    /// raw unsigned bits. Only affects <see cref="SignedValue"/>.
    /// </summary>
    public bool IsSigned { get; private init; }

    /// <summary>
    /// For constant arguments: the declared byte width (1, 2, or 4).
    /// For VarOp arguments: not meaningful — use <see cref="ByteSize"/> instead.
    /// </summary>
    public int TypeSize { get; private init; }

    /// <summary>
    /// Total bytes consumed from the input stream by this argument.
    /// Constant: equals <see cref="TypeSize"/>.
    /// VarOp: equals <c>Var.Length</c>.
    /// </summary>
    public int ByteSize { get; private init; }

    /// <summary>
    /// For constant arguments: the decoded unsigned value (little-endian).
    /// For VarOp arguments: always 0 — use <see cref="Var"/> instead.
    /// </summary>
    public uint Value { get; private init; }

    /// <summary>
    /// For constant arguments with <see cref="IsSigned"/> = true: value sign-extended
    /// to int32 according to <see cref="TypeSize"/>.
    /// For unsigned constants or VarOp arguments: same as casting Value to int.
    /// </summary>
    public int SignedValue => IsSigned
        ? TypeSize switch
        {
            1 => (int)(sbyte)(byte)Value,
            2 => (int)(short)(ushort)Value,
            _ => (int)Value
        }
        : (int)Value;

    /// <summary>
    /// For VarOp arguments: the raw bytes of the expression, including the 0x7F terminator.
    /// For constant arguments: empty.
    /// </summary>
    public byte[] Var { get; private init; } = Array.Empty<byte>();

    // ── Factory: Read ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Reads one argument from <paramref name="data"/> starting at <paramref name="pos"/>.
    /// Advances <paramref name="pos"/> past the bytes consumed.
    /// </summary>
    /// <param name="data">Raw field script bytes.</param>
    /// <param name="pos">Current read position; updated on return.</param>
    /// <param name="typeSize">Byte width for constant form (1, 2, or 4).</param>
    /// <param name="isVar">
    /// True if the corresponding vararg_flag bit was set (or the opcode table
    /// marks this argument as always-varop). Drives VarOp collection vs. constant decode.
    /// </param>
    /// <param name="isSigned">True for signed argument types (AT_JUMP, AT_SPIN, etc.).</param>
    public static FieldScriptArgument Read(
        byte[] data,
        ref int pos,
        int typeSize,
        bool isVar,
        bool isSigned)
    {
        if (isVar)
        {
            return ReadVarOp(data, ref pos, typeSize, isSigned);
        }
        else
        {
            return ReadConstant(data, ref pos, typeSize, isSigned);
        }
    }

    // ── Serialization: Write ──────────────────────────────────────────────────────

    /// <summary>
    /// Appends this argument's bytes to <paramref name="output"/>.
    /// Round-trips byte-identically with <see cref="Read"/>.
    /// </summary>
    public void Write(List<byte> output)
    {
        if (IsVar)
        {
            // VarOp: write all collected bytes verbatim (includes 0x7F terminator)
            foreach (byte b in Var)
                output.Add(b);
        }
        else
        {
            // Constant: re-encode as little-endian at TypeSize width
            // HW write macro: for size != 2 || is_signed → byte-by-byte LE; for size == 2 && !is_signed →
            // special case in HW but the output is the same LE encoding for standard values.
            // We always use LE here — identical result for all field script values in practice.
            uint v = Value;
            for (int i = 0; i < TypeSize; i++)
            {
                output.Add((byte)(v & 0xFF));
                v >>= 8;
            }
        }
    }

    // ── Private: constant decode ──────────────────────────────────────────────────

    private static FieldScriptArgument ReadConstant(
        byte[] data,
        ref int pos,
        int typeSize,
        bool isSigned)
    {
        if (pos + typeSize > data.Length)
            throw new ArgumentException(
                $"FieldScriptArgument: constant read of {typeSize} byte(s) at pos {pos} " +
                $"exceeds data length {data.Length}.");

        uint value = 0;
        for (int i = 0; i < typeSize; i++)
            value |= (uint)data[pos++] << (i * 8);

        return new FieldScriptArgument
        {
            IsVar    = false,
            IsSigned = isSigned,
            TypeSize = typeSize,
            ByteSize = typeSize,
            Value    = value,
            Var      = Array.Empty<byte>()
        };
    }

    // ── Private: VarOp collection ─────────────────────────────────────────────────

    /// <summary>
    /// Collects a complete VarOp expression from the stream.
    ///
    /// Protocol (mirrors MACRO_SCRIPT_IOFUNCTION_ARGREAD):
    ///   1. Read the first token byte.
    ///   2. Look up s_varOpExtraBytes[token] → N.
    ///   3. Read and collect N inline data bytes.
    ///   4. Read the next token.
    ///   5. If token == 0x7F (type -1, terminate): collect it and stop.
    ///      Otherwise goto 2.
    ///
    /// The 0x7F terminator is included in Var, matching HW's layout.
    /// </summary>
    private static FieldScriptArgument ReadVarOp(
        byte[] data,
        ref int pos,
        int typeSize,
        bool isSigned)
    {
        var collected = new List<byte>(16);

        if (pos >= data.Length)
            throw new ArgumentException(
                $"FieldScriptArgument: VarOp read at pos {pos} with no data remaining.");

        // Read first token
        byte token = data[pos++];
        collected.Add(token);

        while (token != 0x7F)
        {
            // Read inline data bytes for this token
            int extra = s_varOpExtraBytes[token];
            if (pos + extra > data.Length)
                throw new ArgumentException(
                    $"FieldScriptArgument: VarOp inline data read of {extra} byte(s) for " +
                    $"token 0x{token:X2} at pos {pos} exceeds data length {data.Length}.");

            for (int i = 0; i < extra; i++)
                collected.Add(data[pos++]);

            // Read next token
            if (pos >= data.Length)
                throw new ArgumentException(
                    $"FieldScriptArgument: VarOp reached end of data without 0x7F terminator " +
                    $"(last token = 0x{token:X2}, pos = {pos}).");

            token = data[pos++];
            collected.Add(token);
        }
        // token == 0x7F is already appended; loop exited.

        byte[] var = collected.ToArray();
        return new FieldScriptArgument
        {
            IsVar    = true,
            IsSigned = isSigned,
            TypeSize = typeSize,   // preserved for metadata (not used in ByteSize)
            ByteSize = var.Length,
            Value    = 0,
            Var      = var
        };
    }

    // ── VarOp extra-bytes table ───────────────────────────────────────────────────
    //
    // Indexed by token byte (0x00-0xFF). Value = number of inline data bytes that
    // immediately follow this token in the stream.
    //
    // Derived from VarOpList[token].type in HW's Database_Script.cpp using the same
    // if/else chain as MACRO_SCRIPT_IOFUNCTION_ARGREAD. See class-level comments for
    // complete derivation and corrections relative to Phase 9.1 FieldScriptOpcodeTable.

    private static readonly int[] s_varOpExtraBytes = BuildVarOpExtraBytesTable();

    private static int[] BuildVarOpExtraBytesTable()
    {
        // All entries default to 0 (types 0, 1, 2, 50, 51, 100 — operators, casts,
        // stack-based functions; no inline bytes in the VarOp stream).
        var t = new int[256];

        // ── Token 0x29: SV_BattleChar_ (type 3) — 1 extra byte ──────────────────
        t[0x29] = 1;

        // ── Tokens 0x7D, 0x7E: constant value literals ────────────────────────────
        t[0x7D] = 2;  // const short (type 6)  — 2-byte LE value follows
        t[0x7E] = 4;  // const long  (type 7)  — 4-byte LE value follows
        // 0x7F = terminate (type -1) — handled by caller, not this table

        // ── Type-19 tokens (10 ≤ type < 20): 1 extra byte (variable ID) ──────────
        t[0x5F] = 1;  // ObjectUID_
        t[0x79] = 1;  // SV_           (VARCODE_SHARED)
        t[0x7A] = 1;  // GetData_      (VARCODE_ENGINE)
        // 0xC3 = VAR_Null1_ (type 19); 0xCB = VAR_Null2_; etc. — covered by loop below

        // ── Type-55 token: GetEntryProperty (0x78) — 2 extra bytes ───────────────
        // NOTE: Phase 9.1 FieldScriptOpcodeTable incorrectly assigned 1 here.
        // HW argread macro: vartype == 55 → read 2 bytes (property_type + 1 unknown).
        t[0x78] = 2;

        // ── 0xC0-0xDF: single-byte variable references (types 10-19) ─────────────
        //
        // Layout from VarOpList (lines 476-480 in Database_Script.cpp):
        //   0xC0-0xC7: types 13,13,13,19,13,13,13,19  (VAR_GenSBool_ etc.)
        //   0xC8-0xCF: types 12,12,12,19,12,12,12,19  (VAR_GenInt24_ etc.)
        //   0xD0-0xD2: types 10,10,10                  (VAR_GenInt8_, VAR_GlobInt8_, VAR_LocInt8_)
        //   0xD3:      type  60 ← EXCEPTION: MemoriaVarCode_, 3 extra bytes (see below)
        //   0xD4-0xD7: types 10,10,10,19               (VAR_GenUInt8_ etc.)
        //   0xD8-0xDF: types 11,11,11,19,11,11,11,19   (VAR_GenInt16_ etc.)
        //
        // All types 10-19 → 1 extra byte, except 0xD3 which overrides to 3 (type 60).
        for (int i = 0xC0; i <= 0xDF; i++)
            t[i] = 1;

        // Override: 0xD3 = MemoriaVarCode_ (type 60) → 3 extra bytes
        // HW argread macro: vartype == 60 → read 3 bytes.
        // NOTE: Phase 9.1 FieldScriptOpcodeTable loop left this at 1 (a bug).
        // Only present in Memoria-extended scripts; unmodded FFIX fields never use it.
        t[0xD3] = 3;

        // ── 0xE0-0xFF: two-byte list variable references (types 20-29) ───────────
        //
        // Layout from VarOpList (lines 482-486 in Database_Script.cpp):
        //   0xE0-0xFF: types 23,23,23,29,23,23,23,29, 22,...,29, 20,...,29, 21,...,29
        // All types 20-29 → 2 extra bytes.
        for (int i = 0xE0; i <= 0xFF; i++)
            t[i] = 2;

        // ── All other 0x00-0x7C tokens: 0 extra bytes ────────────────────────────
        // Types 0, 1, 2, 50, 51, 100: operators and stack-based functions.
        // Specifically: 0x4F (IsButton), 0x52 (GetHP), 0x53 (GetMaxHP), 0x58 (IsUnbutton),
        // 0x59 (IsButtonDown), 0x5A, 0x5B, 0x5C, 0x65 (GetTileAnimFrame),
        // 0x6A (GetAnimDuration), 0x6B (IsInParty), 0x6D (AddParty),
        // 0x6E (GetMP), 0x6F (GetMaxMP), 0x70 (GetWalkpathTriangle), 0x71 (GetWalkpath)
        // — all type 50 in VarOpList, all 0 extra bytes.
        //   Phase 9.1 assigned 1 for these based on VarOpTypeList (GUI metadata, not the
        //   binary format). Default 0 initialization here is correct.

        // 0x80-0xBF: all type 100 (unknown/unused operators) → 0 extra bytes
        // (already 0 from default initialization)

        return t;
    }
}
