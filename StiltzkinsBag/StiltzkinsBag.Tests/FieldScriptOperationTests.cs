using System.Collections.Generic;
using StiltzkinsBag.Core.Parsing;
using Xunit;

namespace StiltzkinsBag.Tests.Parsing
{
    /// <summary>
    /// Unit tests for <see cref="FieldScriptOperation"/> decode and encode paths.
    ///
    /// Each test uses a synthetic byte sequence, decodes it via
    /// <see cref="FieldScriptOperation.Read"/>, asserts structural properties,
    /// and then round-trips back through <see cref="FieldScriptOperation.Write"/>
    /// to confirm byte-identical re-encoding.
    ///
    /// Paths covered:
    ///   • Standard no-arg opcode (UseVarArg=false, 0 args)
    ///   • Standard fixed-arg opcode (UseVarArg=false, fixed byte-widths)
    ///   • Standard VarOp arg (UseVarArg=false, argLen=0)
    ///   • UseVarArg=true — constant arg
    ///   • UseVarArg=true — VarOp arg (flag bit set)
    ///   • JMP_SWITCH (0x0B) — variable jump table
    ///   • JMP_SWITCHEX (0x06) — variable case/jump table
    ///   • SetRegion (0x29) — polygon vertex table
    ///   • Extended opcode (0xFF prefix, opcode ≥ 0x100)
    /// </summary>
    public sealed class FieldScriptOperationTests
    {
        // ── Helpers ────────────────────────────────────────────────────────────

        private static FieldScriptOperation Decode(byte[] data)
        {
            int pos = 0;
            return FieldScriptOperation.Read(data, ref pos);
        }

        /// <summary>
        /// Encodes <paramref name="op"/> and asserts the output bytes match
        /// <paramref name="expected"/> exactly. Failure message names the test.
        /// </summary>
        private static void AssertRoundTrip(FieldScriptOperation op, byte[] expected)
        {
            var actual = new List<byte>();
            op.Write(actual);

            Assert.Equal(expected.Length, actual.Count);
            for (int i = 0; i < expected.Length; i++)
                Assert.True(expected[i] == actual[i],
                    $"Round-trip diff @ byte {i}: expected 0x{expected[i]:X2}, got 0x{actual[i]:X2}");
        }

        // ── Standard no-arg opcodes ────────────────────────────────────────────

        /// <summary>0x00 (NOTHING) — F(), 0 args, 1 byte.</summary>
        [Fact]
        public void Decode_0x00_Nothing_ZeroArgsOneByteRoundTrip()
        {
            byte[] data = { 0x00 };
            var op = Decode(data);

            Assert.Equal(0x00u, op.Opcode);
            Assert.Empty(op.Args);
            Assert.Equal(1, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        /// <summary>0x04 (return) — F(), 0 args, 1 byte.</summary>
        [Fact]
        public void Decode_0x04_Return_ZeroArgsOneByteRoundTrip()
        {
            byte[] data = { 0x04 };
            var op = Decode(data);

            Assert.Equal(0x04u, op.Opcode);
            Assert.Empty(op.Args);
            Assert.Equal(1, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        // ── Standard fixed-arg opcodes (UseVarArg=false) ──────────────────────

        /// <summary>
        /// 0x01 (JMP) — F(2): 1 unsigned 2-byte jump offset.
        /// Bytes: [01][0A][00]  →  offset = 10
        /// </summary>
        [Fact]
        public void Decode_0x01_Jmp_OneFixedTwoByteArg()
        {
            byte[] data = { 0x01, 0x0A, 0x00 };
            var op = Decode(data);

            Assert.Equal(0x01u, op.Opcode);
            Assert.Single(op.Args);
            Assert.False(op.Args[0].IsVar);
            Assert.Equal(10u, op.Args[0].Value);
            Assert.Equal(3, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        /// <summary>
        /// 0x07 (InitCode) — F(1,1): 2 unsigned 1-byte args.
        /// Bytes: [07][03][05]
        /// </summary>
        [Fact]
        public void Decode_0x07_InitCode_TwoFixedOneByteArgs()
        {
            byte[] data = { 0x07, 0x03, 0x05 };
            var op = Decode(data);

            Assert.Equal(0x07u, op.Opcode);
            Assert.Equal(2, op.Args.Length);
            Assert.Equal(3u, op.Args[0].Value);
            Assert.Equal(5u, op.Args[1].Value);
            Assert.Equal(3, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        // ── VarOp argument (UseVarArg=false, argLen=0) ────────────────────────

        /// <summary>
        /// 0x05 (set) — F(0): single VarOp expression.
        /// Bytes: [05][2C][7F]
        ///   Token 0x2C (type=2): 0 extra bytes, then next token 0x7F = terminate.
        ///   Collected var bytes = [0x2C, 0x7F].
        /// </summary>
        [Fact]
        public void Decode_0x05_Set_SimpleVarOpArg()
        {
            byte[] data = { 0x05, 0x2C, 0x7F };
            var op = Decode(data);

            Assert.Equal(0x05u, op.Opcode);
            Assert.Single(op.Args);
            Assert.True(op.Args[0].IsVar);
            Assert.Equal(new byte[] { 0x2C, 0x7F }, op.Args[0].Var);
            Assert.Equal(3, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        /// <summary>
        /// 0x05 (set) — VarOp expression with a const-short token (0x7D, type=5, 2 extra bytes).
        /// Bytes: [05][7D][34][12][7F]
        ///   Token 0x7D: 2 extra bytes [34][12], then 0x7F = terminate.
        ///   Collected var bytes = [0x7D, 0x34, 0x12, 0x7F].
        /// </summary>
        [Fact]
        public void Decode_0x05_Set_VarOpWithConstShortToken()
        {
            byte[] data = { 0x05, 0x7D, 0x34, 0x12, 0x7F };
            var op = Decode(data);

            Assert.Equal(0x05u, op.Opcode);
            Assert.Single(op.Args);
            Assert.True(op.Args[0].IsVar);
            Assert.Equal(new byte[] { 0x7D, 0x34, 0x12, 0x7F }, op.Args[0].Var);
            Assert.Equal(5, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        // ── UseVarArg=true opcodes ─────────────────────────────────────────────

        /// <summary>
        /// 0x22 (Wait) — V(1): vararg_flag=0x00, arg is constant 1-byte value.
        /// Bytes: [22][00][0A]  →  arg = 10
        /// </summary>
        [Fact]
        public void Decode_0x22_Wait_ConstArg()
        {
            byte[] data = { 0x22, 0x00, 0x0A };
            var op = Decode(data);

            Assert.Equal(0x22u, op.Opcode);
            Assert.Equal(0x00, op.VarargFlag);
            Assert.Single(op.Args);
            Assert.False(op.Args[0].IsVar);
            Assert.Equal(10u, op.Args[0].Value);
            Assert.Equal(3, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        /// <summary>
        /// 0x22 (Wait) — V(1): vararg_flag=0x01, arg is a VarOp expression.
        /// Bytes: [22][01][2C][7F]
        ///   Flag bit 0 = 1 → arg[0] is VarOp = [0x2C, 0x7F].
        /// </summary>
        [Fact]
        public void Decode_0x22_Wait_VarArg()
        {
            byte[] data = { 0x22, 0x01, 0x2C, 0x7F };
            var op = Decode(data);

            Assert.Equal(0x22u, op.Opcode);
            Assert.Equal(0x01, op.VarargFlag);
            Assert.Single(op.Args);
            Assert.True(op.Args[0].IsVar);
            Assert.Equal(new byte[] { 0x2C, 0x7F }, op.Args[0].Var);
            Assert.Equal(4, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        /// <summary>
        /// 0x48 (AddItem) — V(2,1): both args constant.
        /// Bytes: [48][00][64][00][03]  →  item=100, count=3
        /// </summary>
        [Fact]
        public void Decode_0x48_AddItem_BothArgsConst()
        {
            byte[] data = { 0x48, 0x00, 0x64, 0x00, 0x03 };
            var op = Decode(data);

            Assert.Equal(0x48u, op.Opcode);
            Assert.Equal(0x00, op.VarargFlag);
            Assert.Equal(2, op.Args.Length);
            Assert.False(op.Args[0].IsVar);
            Assert.Equal(100u, op.Args[0].Value);   // item ID 100
            Assert.False(op.Args[1].IsVar);
            Assert.Equal(3u, op.Args[1].Value);     // count 3
            Assert.Equal(5, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        /// <summary>
        /// 0x48 (AddItem) — V(2,1): item arg is VarOp, count is constant.
        /// Bytes: [48][01][7D][34][12][7F][03]
        ///   Flag=0x01 → arg[0] var (VarOp=[7D,34,12,7F]), arg[1] const (count=3).
        /// </summary>
        [Fact]
        public void Decode_0x48_AddItem_ItemVarCountConst()
        {
            byte[] data = { 0x48, 0x01, 0x7D, 0x34, 0x12, 0x7F, 0x03 };
            var op = Decode(data);

            Assert.Equal(0x48u, op.Opcode);
            Assert.Equal(0x01, op.VarargFlag);
            Assert.Equal(2, op.Args.Length);
            Assert.True(op.Args[0].IsVar);
            Assert.Equal(new byte[] { 0x7D, 0x34, 0x12, 0x7F }, op.Args[0].Var);
            Assert.False(op.Args[1].IsVar);
            Assert.Equal(3u, op.Args[1].Value);
            Assert.Equal(7, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        // ── JMP_SWITCH (0x0B) ──────────────────────────────────────────────────

        /// <summary>
        /// 0x0B (JMP_SWITCH) — N=2 cases.
        /// Format: [0B][N=2][start:2][default:2][jump0:2][jump1:2]
        /// Bytes: [0B][02][00][00][65][00][A0][00][B0][00]
        ///   start=0, default=101, jump[0]=160, jump[1]=176
        ///   ByteSize = 1+1+2+2+2×2 = 10
        /// </summary>
        [Fact]
        public void Decode_0x0B_JmpSwitch_N2()
        {
            byte[] data = { 0x0B, 0x02, 0x00, 0x00, 0x65, 0x00, 0xA0, 0x00, 0xB0, 0x00 };
            var op = Decode(data);

            Assert.Equal(0x0Bu, op.Opcode);
            Assert.Equal(2, op.SizeByte);               // N stored in SizeByte
            Assert.Equal(4, op.Args.Length);            // start + default + 2 jumps
            Assert.False(op.Args[0].IsVar);
            Assert.Equal(0u,   op.Args[0].Value);       // start = 0
            Assert.Equal(0x65u, op.Args[1].Value);      // default = 101
            Assert.Equal(0xA0u, op.Args[2].Value);      // jump[0] = 160
            Assert.Equal(0xB0u, op.Args[3].Value);      // jump[1] = 176
            Assert.Equal(10, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        /// <summary>
        /// 0x0B (JMP_SWITCH) — N=0 (degenerate: only start + default, no case jumps).
        /// Bytes: [0B][00][00][00][65][00]
        ///   ByteSize = 1+1+2+2 = 6
        /// </summary>
        [Fact]
        public void Decode_0x0B_JmpSwitch_N0()
        {
            byte[] data = { 0x0B, 0x00, 0x00, 0x00, 0x65, 0x00 };
            var op = Decode(data);

            Assert.Equal(0x0Bu, op.Opcode);
            Assert.Equal(0, op.SizeByte);
            Assert.Equal(2, op.Args.Length);            // start + default only
            Assert.Equal(0u, op.Args[0].Value);
            Assert.Equal(0x65u, op.Args[1].Value);
            Assert.Equal(6, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        // ── JMP_SWITCHEX (0x06) ────────────────────────────────────────────────

        /// <summary>
        /// 0x06 (JMP_SWITCHEX) — N=2 cases.
        /// Format: [06][N=2][default:2][case0:2][jump0:2][case1:2][jump1:2]
        /// Bytes: [06][02][65][00][01][00][A0][00][02][00][B0][00]
        ///   default=101, case[0]=1, jump[0]=160, case[1]=2, jump[1]=176
        ///   ByteSize = 1+1+2+2×4 = 12
        /// </summary>
        [Fact]
        public void Decode_0x06_JmpSwitchEx_N2()
        {
            byte[] data =
            {
                0x06, 0x02,             // opcode + N
                0x65, 0x00,             // default = 101
                0x01, 0x00,             // case[0] = 1
                0xA0, 0x00,             // jump[0] = 160
                0x02, 0x00,             // case[1] = 2
                0xB0, 0x00              // jump[1] = 176
            };
            var op = Decode(data);

            Assert.Equal(0x06u, op.Opcode);
            Assert.Equal(2, op.SizeByte);
            Assert.Equal(5, op.Args.Length);            // default + 2×(case+jump)
            Assert.Equal(0x65u, op.Args[0].Value);      // default = 101
            Assert.Equal(0x01u, op.Args[1].Value);      // case[0] = 1
            Assert.Equal(0xA0u, op.Args[2].Value);      // jump[0] = 160
            Assert.Equal(0x02u, op.Args[3].Value);      // case[1] = 2
            Assert.Equal(0xB0u, op.Args[4].Value);      // jump[1] = 176
            Assert.Equal(12, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        /// <summary>
        /// 0x06 (JMP_SWITCHEX) — N=0 (only default jump, no cases).
        /// Bytes: [06][00][65][00]
        ///   ByteSize = 1+1+2 = 4
        /// </summary>
        [Fact]
        public void Decode_0x06_JmpSwitchEx_N0()
        {
            byte[] data = { 0x06, 0x00, 0x65, 0x00 };
            var op = Decode(data);

            Assert.Equal(0x06u, op.Opcode);
            Assert.Equal(0, op.SizeByte);
            Assert.Single(op.Args);                     // default only
            Assert.Equal(0x65u, op.Args[0].Value);
            Assert.Equal(4, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        // ── SetRegion (0x29) ───────────────────────────────────────────────────

        /// <summary>
        /// 0x29 (SetRegion) — N=2 vertices, vararg_flag=0x00 (all constant 4-byte coords).
        /// Format: [29][vararg_flag][N][v0:4][v1:4]
        /// Bytes: [29][00][02][01][02][03][04][05][06][07][08]
        ///   v[0] = 0x04030201, v[1] = 0x08070605
        ///   ByteSize = 1+1+1+4+4 = 11
        /// </summary>
        [Fact]
        public void Decode_0x29_SetRegion_N2_AllConstVertices()
        {
            byte[] data =
            {
                0x29,                       // opcode
                0x00,                       // vararg_flag = 0 (all const)
                0x02,                       // N = 2
                0x01, 0x02, 0x03, 0x04,    // vertex[0] = 0x04030201
                0x05, 0x06, 0x07, 0x08     // vertex[1] = 0x08070605
            };
            var op = Decode(data);

            Assert.Equal(0x29u, op.Opcode);
            Assert.Equal(0x00, op.VarargFlag);
            Assert.Equal(2, op.SizeByte);
            Assert.Equal(2, op.Args.Length);
            Assert.False(op.Args[0].IsVar);
            Assert.Equal(0x04030201u, op.Args[0].Value);
            Assert.False(op.Args[1].IsVar);
            Assert.Equal(0x08070605u, op.Args[1].Value);
            Assert.Equal(11, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        /// <summary>
        /// 0x29 (SetRegion) — N=1 vertex, vararg_flag=0x01 (vertex[0] is VarOp).
        /// Format: [29][vararg_flag=1][N=1][varop...7F]
        /// The single vertex arg is a VarOp expression.
        /// Bytes: [29][01][01][2C][7F]
        ///   ByteSize = 1+1+1+2 = 5
        /// </summary>
        [Fact]
        public void Decode_0x29_SetRegion_N1_VarVertex()
        {
            byte[] data = { 0x29, 0x01, 0x01, 0x2C, 0x7F };
            var op = Decode(data);

            Assert.Equal(0x29u, op.Opcode);
            Assert.Equal(0x01, op.VarargFlag);
            Assert.Equal(1, op.SizeByte);
            Assert.Single(op.Args);
            Assert.True(op.Args[0].IsVar);
            Assert.Equal(new byte[] { 0x2C, 0x7F }, op.Args[0].Var);
            Assert.Equal(5, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        // ── Extended opcode (0xFF prefix) ──────────────────────────────────────

        /// <summary>
        /// Extended opcode 0x100 (BSSTART) — encoded as [FF][00] then V(1,1) args.
        /// Bytes: [FF][00][00][0A][0B]
        ///   0xFF → opcode += 0x100, read next → 0x00 → opcode = 0x100
        ///   flag=0x00, arg[0]=10, arg[1]=11
        ///   ByteSize = 2 (FF+00) + 1 (flag) + 1 + 1 = 5
        /// </summary>
        [Fact]
        public void Decode_ExtendedOpcode_0x100_ViaSingleFfPrefix()
        {
            byte[] data = { 0xFF, 0x00, 0x00, 0x0A, 0x0B };
            var op = Decode(data);

            Assert.Equal(0x100u, op.Opcode);
            Assert.Equal(0x00, op.VarargFlag);
            Assert.Equal(2, op.Args.Length);
            Assert.False(op.Args[0].IsVar);
            Assert.Equal(10u, op.Args[0].Value);
            Assert.False(op.Args[1].IsVar);
            Assert.Equal(11u, op.Args[1].Value);
            Assert.Equal(5, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        /// <summary>
        /// Extended opcode 0x111 (BAVISIBLE) — uses [FF][11] prefix.
        /// V(1,1) args.
        /// Bytes: [FF][11][00][05][07]
        ///   ByteSize = 2 + 1 + 1 + 1 = 5
        /// </summary>
        [Fact]
        public void Decode_ExtendedOpcode_0x111_Via_FF11_Prefix()
        {
            byte[] data = { 0xFF, 0x11, 0x00, 0x05, 0x07 };
            var op = Decode(data);

            Assert.Equal(0x111u, op.Opcode);
            Assert.Equal(0x00, op.VarargFlag);
            Assert.Equal(2, op.Args.Length);
            Assert.Equal(5u, op.Args[0].Value);
            Assert.Equal(7u, op.Args[1].Value);
            Assert.Equal(5, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        // ── Multi-argument VarOp correctness ──────────────────────────────────

        /// <summary>
        /// Verifies VarOp token 0x78 (GetEntryProperty, type=55) consumes 2 extra bytes.
        /// Bytes: [05][78][01][02][7F]
        ///   Token 0x78 (type=55): 2 extra bytes → [01][02], then 0x7F terminates.
        ///   collected = [0x78, 0x01, 0x02, 0x7F], ByteSize = 1+4 = 5
        /// </summary>
        [Fact]
        public void Decode_VarOp_Token0x78_GetEntryProperty_TwoExtraBytes()
        {
            byte[] data = { 0x05, 0x78, 0x01, 0x02, 0x7F };
            var op = Decode(data);

            Assert.Equal(0x05u, op.Opcode);
            Assert.True(op.Args[0].IsVar);
            Assert.Equal(new byte[] { 0x78, 0x01, 0x02, 0x7F }, op.Args[0].Var);
            Assert.Equal(5, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        /// <summary>
        /// Verifies VarOp token 0xD3 (MemoriaVarCode_, type=60) consumes 3 extra bytes.
        /// Bytes: [05][D3][01][02][03][7F]
        ///   Token 0xD3 (type=60): 3 extra bytes → [01][02][03], then 0x7F terminates.
        ///   collected = [0xD3, 0x01, 0x02, 0x03, 0x7F], ByteSize = 1+5 = 6
        /// </summary>
        [Fact]
        public void Decode_VarOp_Token0xD3_MemoriaVarCode_ThreeExtraBytes()
        {
            byte[] data = { 0x05, 0xD3, 0x01, 0x02, 0x03, 0x7F };
            var op = Decode(data);

            Assert.Equal(0x05u, op.Opcode);
            Assert.True(op.Args[0].IsVar);
            Assert.Equal(new byte[] { 0xD3, 0x01, 0x02, 0x03, 0x7F }, op.Args[0].Var);
            Assert.Equal(6, op.ByteSize);
            AssertRoundTrip(op, data);
        }

        /// <summary>
        /// Verifies VarOp tokens in range 0x4F/0x52/0x53/0x58-0x5C/0x65/0x6A-0x6B/0x6D-0x71
        /// (type=50 stack functions) consume ZERO extra bytes (not 1 as the old Phase 9.1
        /// table incorrectly used).
        /// Token 0x52 (GetHP, type=50): 0 extra bytes.
        /// Bytes: [05][52][7F]  →  collected = [0x52, 0x7F], ByteSize = 1+2 = 3
        /// </summary>
        [Fact]
        public void Decode_VarOp_Token0x52_GetHP_ZeroExtraBytes()
        {
            byte[] data = { 0x05, 0x52, 0x7F };
            var op = Decode(data);

            Assert.Equal(0x05u, op.Opcode);
            Assert.True(op.Args[0].IsVar);
            Assert.Equal(new byte[] { 0x52, 0x7F }, op.Args[0].Var);
            Assert.Equal(3, op.ByteSize);
            AssertRoundTrip(op, data);
        }
    }
}
