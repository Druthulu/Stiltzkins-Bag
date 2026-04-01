// StiltzkinsBag.Tests/WorldMapVariableScannerTests.cs
//
// Unit tests for WorldMapVariableScanner.
//
// All tests use SYNTHETIC binary data — no real .eb.bytes game files required.
// Synthetic blocks are crafted from the confirmed 8-byte instruction encoding:
//
//   set VAR_LocUInt8_N = X   →   05 D6 [N] 7D [lo] [hi] 2C 7F
//   set VAR_LocInt16_N = X   →   05 DA [N] 7D [lo] [hi] 2C 7F
//
// Each delivery block = 8 consecutive instructions × 8 bytes = 64 bytes total.
// Order: item0, count0, item1, count1, item2, count2, item3, count3.
//
// Convention A (world00–world11): variables 37,42,38,43,39,44,40(Int16),45
// Convention B (world12):         variables  2, 7, 3, 8, 4, 9, 5(Int16),10
// Convention C (world00–world11): variables  9,14,10,15,11,16,12(Int16),17
//   Used for chocograph World_Chest reward delivery (variable-reference AddItem).
//   Notable items: Ragnarok (ID 29, slot 3 of some blocks), Dragon's Claws (ID 45).

using System;
using System.Collections.Generic;
using System.Linq;
using StiltzkinsBag.Core.Models;
using Xunit;

namespace StiltzkinsBag.Tests;

public sealed class WorldMapVariableScannerTests
{
    // ── Binary construction helpers ──────────────────────────────────────────

    private const byte OpcodeSet       = 0x05;
    private const byte PrefixLocUInt8  = 0xD6;
    private const byte PrefixLocInt16  = 0xDA;
    private const byte TokenConstShort = 0x7D;
    private const byte TokenAssign     = 0x2C;
    private const byte TokenTerminate  = 0x7F;

    /// <summary>
    /// Builds one 8-byte set-variable instruction:
    ///   05 [prefix] [idx] 7D [lo] [hi] 2C 7F
    /// </summary>
    private static byte[] Instruction(byte prefix, byte idx, ushort value) =>
        new byte[]
        {
            OpcodeSet, prefix, idx, TokenConstShort,
            (byte)(value & 0xFF), (byte)(value >> 8),
            TokenAssign, TokenTerminate
        };

    /// <summary>
    /// Builds a 64-byte Convention-A delivery block.
    /// Slot order: item0, count0, item1, count1, item2, count2, item3(Int16), count3.
    /// </summary>
    private static byte[] ConventionABlock(
        ushort item0, ushort count0,
        ushort item1, ushort count1,
        ushort item2, ushort count2,
        ushort item3, ushort count3)
    {
        var block = new List<byte>();
        block.AddRange(Instruction(PrefixLocUInt8, 37, item0));
        block.AddRange(Instruction(PrefixLocUInt8, 42, count0));
        block.AddRange(Instruction(PrefixLocUInt8, 38, item1));
        block.AddRange(Instruction(PrefixLocUInt8, 43, count1));
        block.AddRange(Instruction(PrefixLocUInt8, 39, item2));
        block.AddRange(Instruction(PrefixLocUInt8, 44, count2));
        block.AddRange(Instruction(PrefixLocInt16, 40, item3));
        block.AddRange(Instruction(PrefixLocUInt8, 45, count3));
        return block.ToArray();
    }

    /// <summary>
    /// Builds a 64-byte Convention-B delivery block.
    /// Slot order: item0, count0, item1, count1, item2, count2, item3(Int16), count3.
    /// </summary>
    private static byte[] ConventionBBlock(
        ushort item0, ushort count0,
        ushort item1, ushort count1,
        ushort item2, ushort count2,
        ushort item3, ushort count3)
    {
        var block = new List<byte>();
        block.AddRange(Instruction(PrefixLocUInt8,  2, item0));
        block.AddRange(Instruction(PrefixLocUInt8,  7, count0));
        block.AddRange(Instruction(PrefixLocUInt8,  3, item1));
        block.AddRange(Instruction(PrefixLocUInt8,  8, count1));
        block.AddRange(Instruction(PrefixLocUInt8,  4, item2));
        block.AddRange(Instruction(PrefixLocUInt8,  9, count2));
        block.AddRange(Instruction(PrefixLocInt16,  5, item3));
        block.AddRange(Instruction(PrefixLocUInt8, 10, count3));
        return block.ToArray();
    }

    /// <summary>
    /// Wraps a block payload in padding bytes (0xFF) so the block starts at
    /// <paramref name="offset"/>. Ensures the file is large enough for full scanning.
    /// </summary>
    private static byte[] FileWithBlockAt(byte[] block, int offset = 0)
    {
        int totalLength = offset + block.Length + 64; // trailing padding
        var bytes = new byte[totalLength];
        Array.Fill(bytes, (byte)0xFF);
        Buffer.BlockCopy(block, 0, bytes, offset, block.Length);
        return bytes;
    }

    // ── Convention A basic detection ─────────────────────────────────────────

    [Fact]
    public void ScanFiles_ConventionA_DetectsBlockAtStartOfFile()
    {
        // Beneath Quan's Dwelling: Ore(254)x9, Topaz(234)x15, Tiger Racket(56)x1, card
        byte[] block = ConventionABlock(254, 9, 234, 15, 56, 1, 596, 1);
        byte[] file  = FileWithBlockAt(block, offset: 0);

        var result = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world00.eb.bytes", file) });

        // Card slot (596) must be excluded
        Assert.DoesNotContain(596, result.Keys);

        // Item IDs 1-255 must be present with correct counts
        Assert.Equal(9,  result[254]);  // Ore
        Assert.Equal(15, result[234]);  // Topaz
        Assert.Equal(1,  result[56]);   // Tiger Racket
    }

    [Fact]
    public void ScanFiles_ConventionA_DetectsBlockAtNonZeroOffset()
    {
        // Block buried inside a larger file
        byte[] block = ConventionABlock(236, 50, 237, 25, 238, 9, 239, 7);
        byte[] file  = FileWithBlockAt(block, offset: 200);

        var result = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world05.eb.bytes", file) });

        Assert.Equal(50, result[236]);  // Potion
        Assert.Equal(25, result[237]);  // Hi-Potion
        Assert.Equal(9,  result[238]);  // Ether
        Assert.Equal(7,  result[239]);  // Elixir
    }

    [Fact]
    public void ScanFiles_ConventionA_ExtractsAllFourItemSlotsAndCounts()
    {
        // Verify each item and count slot is extracted at the correct byte position
        // Near Oeilvert: Maiden Prayer(222)x1, Dragon's Hair(40)x1, Gauntlets(111)x1, card(575=Odin)
        byte[] block = ConventionABlock(222, 1, 40, 1, 111, 1, 575, 1);
        byte[] file  = FileWithBlockAt(block);

        var result = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world07.eb.bytes", file) });

        Assert.Equal(1, result[222]);  // Maiden Prayer
        Assert.Equal(1, result[40]);   // Dragon's Hair
        Assert.Equal(1, result[111]);  // Gauntlets
        Assert.DoesNotContain(575, result.Keys);  // Odin Card excluded
    }

    // ── Convention B detection ───────────────────────────────────────────────

    [Fact]
    public void ScanFiles_ConventionB_RequiresWorld12FileName()
    {
        // The same bytes should be found ONLY when the file is named world12.eb.bytes.
        // A Convention-B block will NOT match Convention-A sequence (different var indices).
        byte[] block = ConventionBBlock(226, 10, 15, 1, 190, 1, 611, 1);
        byte[] file  = FileWithBlockAt(block);

        // Named as world12 — should find the block
        var resultB = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world12.eb.bytes", file) });
        Assert.True(resultB.ContainsKey(226), "Convention B block should be found in world12 file");

        // Named as world00 — should NOT find it (Convention A sequence won't match)
        var resultA = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world00.eb.bytes", file) });
        Assert.Empty(resultA);
    }

    [Fact]
    public void ScanFiles_ConventionB_ExtractsCorrectValues()
    {
        // Unmarked ocean: Aquamarine(226)x10, Ultima Weapon(15)x1, Maximillian(190)x1, Invincible Card(611)
        byte[] block = ConventionBBlock(226, 10, 15, 1, 190, 1, 611, 1);
        byte[] file  = FileWithBlockAt(block);

        var result = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world12.eb.bytes", file) });

        Assert.Equal(10, result[226]);  // Aquamarine
        Assert.Equal(1,  result[15]);   // Ultima Weapon
        Assert.Equal(1,  result[190]);  // Maximillian
        Assert.DoesNotContain(611, result.Keys);  // Invincible Card excluded
    }

    // ── Deduplication ────────────────────────────────────────────────────────

    [Fact]
    public void ScanFiles_Deduplication_SameBlockInTwoFilesCountedOnce()
    {
        // Convention A blocks are identical across world00/03/05/07/08/09.
        // Scanning the same block in multiple files must produce the same count
        // as scanning it in one file — not multiply.
        // Between continents: Straw Hat(113)x8, Pearl Armlet(217)x8, Aloha T-shirt(148)x7, Sandals(195)x8
        byte[] block = ConventionABlock(113, 8, 217, 8, 148, 7, 195, 8);

        // Two files with the identical block
        var file1 = FileWithBlockAt(block);
        var file2 = FileWithBlockAt(block);

        var result = WorldMapVariableScanner.ScanFiles(new[]
        {
            ("evt_world_world00.eb.bytes", file1),
            ("evt_world_world03.eb.bytes", file2),
        });

        Assert.Equal(8, result[113]);  // Straw Hat — not 16
        Assert.Equal(8, result[217]);  // Pearl Armlet — not 16
        Assert.Equal(7, result[148]);  // Aloha T-Shirt — not 14
        Assert.Equal(8, result[195]);  // Sandals — not 16
    }

    [Fact]
    public void ScanFiles_Deduplication_DifferentBlocksNotDeduplicated()
    {
        // Two distinct blocks (different item ID signatures) must each be counted.
        byte[] blockA = ConventionABlock(113, 8, 217, 8, 148, 7, 570, 1);
        byte[] blockB = ConventionABlock(247, 10, 173, 1, 109, 1, 563, 1);

        var fileA = FileWithBlockAt(blockA);
        var fileB = FileWithBlockAt(blockB);

        var result = WorldMapVariableScanner.ScanFiles(new[]
        {
            ("evt_world_world00.eb.bytes", fileA),
            ("evt_world_world03.eb.bytes", fileB),
        });

        // Both blocks counted
        Assert.Equal(8,  result[113]);  // Straw Hat (from block A)
        Assert.Equal(10, result[247]);  // Remedy (from block B)
        Assert.Equal(1,  result[109]);  // Genji Gloves (from block B)
    }

    [Fact]
    public void ScanFiles_Deduplication_TwoBlocksInSameFileBothCounted()
    {
        // Multiple different delivery blocks in one file must all be found.
        byte[] blockA = ConventionABlock(254, 9, 234, 15, 56, 1, 596, 1);
        byte[] blockB = ConventionABlock(236, 50, 237, 25, 238, 9, 239, 7);

        // Place the two blocks contiguously
        var fileBytes = new List<byte>();
        fileBytes.AddRange(new byte[100]);        // leading padding
        fileBytes.AddRange(blockA);
        fileBytes.AddRange(new byte[50]);         // gap between blocks
        fileBytes.AddRange(blockB);
        fileBytes.AddRange(new byte[64]);         // trailing padding

        var result = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world00.eb.bytes", fileBytes.ToArray()) });

        Assert.Equal(9,  result[254]);  // Ore (from block A)
        Assert.Equal(50, result[236]);  // Potion (from block B)
        Assert.Equal(25, result[237]);  // Hi-Potion (from block B)
    }

    // ── Accumulation ─────────────────────────────────────────────────────────

    [Fact]
    public void ScanFiles_Accumulation_SameItemInTwoUniqueBlocksSumsQuantities()
    {
        // If two distinct delivery blocks both give the same item, the quantities sum.
        // Example: Ether(238) in two different events.
        byte[] blockA = ConventionABlock(238, 9, 234, 15, 56, 1, 596, 1);
        byte[] blockB = ConventionABlock(238, 5, 237, 25, 111, 1, 575, 1);

        var fileA = FileWithBlockAt(blockA);
        var fileB = FileWithBlockAt(blockB);

        var result = WorldMapVariableScanner.ScanFiles(new[]
        {
            ("evt_world_world00.eb.bytes", fileA),
            ("evt_world_world03.eb.bytes", fileB),
        });

        // Ether from two distinct locations: 9 + 5 = 14
        Assert.Equal(14, result[238]);
        // Other items from their respective blocks
        Assert.Equal(15, result[234]);  // Topaz (block A)
        Assert.Equal(25, result[237]);  // Hi-Potion (block B)
    }

    // ── Card ID / boundary filtering ─────────────────────────────────────────

    [Fact]
    public void ScanFiles_CardIds_ExcludedFromResult()
    {
        // Card IDs (512+) are used as rewards in some slots but are not regular items.
        // IDs outside 1–255 must be excluded.
        byte[] block = ConventionABlock(226, 10, 15, 1, 190, 1, 611, 1);
        byte[] file  = FileWithBlockAt(block);

        var result = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world05.eb.bytes", file) });

        // Only item IDs 1–255 should appear
        Assert.All(result.Keys, id => Assert.InRange(id, 1, 255));
        Assert.DoesNotContain(611, result.Keys);
    }

    [Fact]
    public void ScanFiles_ItemIdZero_Excluded()
    {
        // Item ID 0 is the null sentinel — must be excluded.
        byte[] block = ConventionABlock(0, 1, 15, 1, 190, 1, 570, 1);
        byte[] file  = FileWithBlockAt(block);

        var result = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world00.eb.bytes", file) });

        Assert.DoesNotContain(0, result.Keys);
        Assert.Equal(1, result[15]);   // Ultima Weapon still found
        Assert.Equal(1, result[190]);  // Maximillian still found
    }

    // ── Edge cases / safety ──────────────────────────────────────────────────

    [Fact]
    public void ScanFiles_EmptyFile_ReturnsEmptyResult()
    {
        var result = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world00.eb.bytes", Array.Empty<byte>()) });

        Assert.Empty(result);
    }

    [Fact]
    public void ScanFiles_FileTooSmall_ReturnsEmptyResult()
    {
        // File smaller than one 64-byte delivery block — nothing to find
        var result = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world00.eb.bytes", new byte[63]) });

        Assert.Empty(result);
    }

    [Fact]
    public void ScanFiles_NoMatchingBlock_ReturnsEmptyResult()
    {
        // File full of 0xFF — no valid delivery blocks
        var result = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world00.eb.bytes", new byte[512]) });

        Assert.Empty(result);
    }

    [Fact]
    public void ScanFiles_NullFileCollection_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            WorldMapVariableScanner.ScanFiles(null!));
    }

    [Fact]
    public void ScanFiles_NullByteArray_SkipsFile()
    {
        // A null byte array for a single file must be skipped gracefully.
        byte[] block = ConventionABlock(236, 50, 237, 25, 238, 9, 239, 7);
        byte[] file  = FileWithBlockAt(block);

        var result = WorldMapVariableScanner.ScanFiles(new (string, byte[])[]
        {
            ("evt_world_world00.eb.bytes", null!),     // should be skipped
            ("evt_world_world03.eb.bytes", file),
        });

        // Good file is still scanned
        Assert.Equal(50, result[236]);
    }

    [Fact]
    public void ScanDirectory_NullPath_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            WorldMapVariableScanner.ScanDirectory(null!));
    }

    // ── Partial match / false positive rejection ─────────────────────────────

    [Fact]
    public void ScanFiles_PartialBlock_NotDetected()
    {
        // A 63-byte sequence that matches the first 7 instructions but is truncated
        // at the file boundary must NOT produce a result.
        byte[] block = ConventionABlock(236, 50, 237, 25, 238, 9, 239, 7);
        byte[] truncated = block[..63];  // 1 byte short of 64

        var result = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world00.eb.bytes", truncated) });

        Assert.Empty(result);
    }

    [Fact]
    public void ScanFiles_CorrectInstructionWithWrongVariableIndex_NotDetected()
    {
        // A 64-byte region that has the right opcode+prefix but wrong variable index
        // for one slot must not match — all 8 instruction slots must be verified.
        byte[] block = ConventionABlock(236, 50, 237, 25, 238, 9, 239, 7);

        // Corrupt slot 2's item variable index (byte at offset 16+2 = 18):
        // Convention A slot 2 item = VAR_LocUInt8_38 (0x26). Change to 0x27 (VAR_39).
        block[18] = 0x27;

        byte[] file = FileWithBlockAt(block);

        var result = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world00.eb.bytes", file) });

        Assert.Empty(result);
    }

    // ── Real-world example blocks (values from binary scan) ──────────────────

    [Fact]
    public void ScanFiles_RealWorldExample_AllSevenConventionABlocks()
    {
        // Replicate the 7 unique Convention-A blocks found in the real world map files.
        // All 7 appear in world00 (and identically in world03/05/07/08/09).
        // Items verified by WorldMapVariableScanner binary analysis of actual .eb.bytes.

        byte[] block1 = ConventionABlock(254, 9,  234, 15, 56,  1,  596, 1); // Quan's Dwelling
        byte[] block2 = ConventionABlock(236, 50, 237, 25, 238, 9,  239, 7); // North of Iifa
        byte[] block3 = ConventionABlock(113, 8,  217, 8,  148, 7,  195, 8); // Between continents: ...Sandals(195)x8
        byte[] block4 = ConventionABlock(247, 10, 173, 1,  109, 1,  563, 1); // South Forgotten Cont.
        byte[] block5 = ConventionABlock(235, 41, 204, 1,  209, 1,  611, 1); // Eastern Lost Cont.
        byte[] block6 = ConventionABlock(244, 19, 203, 1,  146, 1,  596, 1); // NE Forgotten Cont.
        byte[] block7 = ConventionABlock(222, 1,  40,  1,  111, 1,  575, 1); // Near Oeilvert

        // Build a file with all 7 blocks contiguously with small gaps
        var allBytes = new List<byte>();
        allBytes.AddRange(new byte[100]);
        foreach (byte[] blk in new[] { block1, block2, block3, block4, block5, block6, block7 })
        {
            allBytes.AddRange(blk);
            allBytes.AddRange(new byte[16]);  // gap between blocks
        }
        allBytes.AddRange(new byte[64]);  // trailing

        var result = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world00.eb.bytes", allBytes.ToArray()) });

        // Spot-check a selection of items and counts
        Assert.Equal(9,   result[254]);  // Ore
        Assert.Equal(15,  result[234]);  // Topaz
        Assert.Equal(1,   result[56]);   // Tiger Racket
        Assert.Equal(50,  result[236]);  // Potion
        Assert.Equal(7,   result[239]);  // Elixir
        Assert.Equal(8,   result[113]);  // Straw Hat
        Assert.Equal(10,  result[247]);  // Remedy
        Assert.Equal(1,   result[109]);  // Genji Gloves
        Assert.Equal(41,  result[235]);  // Lapis Lazuli
        Assert.Equal(1,   result[204]);  // Rosetta Ring
        Assert.Equal(19,  result[244]);  // Eye Drops
        Assert.Equal(1,   result[146]);  // Genji Helmet
        Assert.Equal(1,   result[222]);  // Maiden Prayer
        Assert.Equal(1,   result[40]);   // Dragon's Hair
        Assert.Equal(1,   result[111]);  // Gauntlets

        Assert.Equal(8,   result[195]);  // Sandals (block 3, all 4 slots are items)

        // Cards excluded
        Assert.DoesNotContain(596, result.Keys);
        Assert.DoesNotContain(563, result.Keys);
        Assert.DoesNotContain(611, result.Keys);
        Assert.DoesNotContain(575, result.Keys);
    }

    [Fact]
    public void ScanFiles_RealWorldExample_ConventionBUnmarkedOcean()
    {
        // The single Convention-B block in world12.eb.bytes (Chocobo Treasure):
        // Aquamarine(226)x10, Ultima Weapon(15)x1, Maximillian(190)x1, Invincible Card(611)
        byte[] block = ConventionBBlock(226, 10, 15, 1, 190, 1, 611, 1);
        byte[] file  = FileWithBlockAt(block, offset: 300);

        var result = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world12.eb.bytes", file) });

        Assert.Equal(10, result[226]);  // Aquamarine
        Assert.Equal(1,  result[15]);   // Ultima Weapon
        Assert.Equal(1,  result[190]);  // Maximillian
        Assert.DoesNotContain(611, result.Keys);
    }

    /// <summary>
    /// Builds a 64-byte Convention-C delivery block.
    /// Variable sequence: item0(D6,9), count0(D6,14), item1(D6,10), count1(D6,15),
    ///                    item2(D6,11), count2(D6,16), item3(DA,12), count3(D6,17).
    /// </summary>
    private static byte[] ConventionCBlock(
        ushort item0, ushort count0,
        ushort item1, ushort count1,
        ushort item2, ushort count2,
        ushort item3, ushort count3)
    {
        var block = new List<byte>();
        block.AddRange(Instruction(PrefixLocUInt8,  9, item0));
        block.AddRange(Instruction(PrefixLocUInt8, 14, count0));
        block.AddRange(Instruction(PrefixLocUInt8, 10, item1));
        block.AddRange(Instruction(PrefixLocUInt8, 15, count1));
        block.AddRange(Instruction(PrefixLocUInt8, 11, item2));
        block.AddRange(Instruction(PrefixLocUInt8, 16, count2));
        block.AddRange(Instruction(PrefixLocInt16, 12, item3));
        block.AddRange(Instruction(PrefixLocUInt8, 17, count3));
        return block.ToArray();
    }

    // ── Convention C detection ───────────────────────────────────────────────

    [Fact]
    public void ScanFiles_ConventionC_DetectsBlockInNonWorld12File()
    {
        // Convention C (chocograph World_Chest delivery) must be found in non-world12 files.
        // Outer Island chocograph: first 3 slots are arbitrary gems/items, slot 3 = Ragnarok(29).
        byte[] block = ConventionCBlock(224, 4, 225, 8, 231, 6, 29, 1);
        byte[] file  = FileWithBlockAt(block, offset: 0);

        var result = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world00.eb.bytes", file) });

        Assert.Equal(4,  result[224]);  // Garnet
        Assert.Equal(8,  result[225]);  // Amethyst
        Assert.Equal(6,  result[231]);  // Peridot
        Assert.Equal(1,  result[29]);   // Ragnarok
    }

    [Fact]
    public void ScanFiles_ConventionC_DragonClaws_Found()
    {
        // Forgotten Lagoon chocograph: slot 3 = Dragon's Claws (45).
        byte[] block = ConventionCBlock(237, 5, 238, 3, 240, 10, 45, 1);
        byte[] file  = FileWithBlockAt(block, offset: 150);

        var result = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world05.eb.bytes", file) });

        Assert.Equal(5,  result[237]);  // Hi-Potion
        Assert.Equal(3,  result[238]);  // Ether
        Assert.Equal(10, result[240]);  // Phoenix Down
        Assert.Equal(1,  result[45]);   // Dragon's Claws
    }

    [Fact]
    public void ScanFiles_ConventionC_NotDetectedInWorld12File()
    {
        // Convention C must NOT be scanned in world12 (only Convention B runs there).
        byte[] block = ConventionCBlock(224, 4, 225, 8, 231, 6, 29, 1);
        byte[] file  = FileWithBlockAt(block);

        var result = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world12.eb.bytes", file) });

        // Convention C vars (D6,9) etc. do not match Convention B vars (D6,2 etc.)
        Assert.Empty(result);
    }

    [Fact]
    public void ScanFiles_ConventionC_And_ConventionA_InSameFile_BothFound()
    {
        // A non-world12 file can contain both Convention-A (Dead Pepper) and
        // Convention-C (chocograph World_Chest) blocks. Both must be detected.
        byte[] blockA = ConventionABlock(254, 9, 234, 15, 56, 1, 596, 1);  // Dead Pepper: Ore/Topaz/Tiger Racket
        byte[] blockC = ConventionCBlock(224, 4, 225, 8, 231, 6, 29, 1);   // World_Chest: Ragnarok slot 3

        var fileBytes = new List<byte>();
        fileBytes.AddRange(new byte[100]);  // leading padding
        fileBytes.AddRange(blockA);
        fileBytes.AddRange(new byte[32]);   // gap
        fileBytes.AddRange(blockC);
        fileBytes.AddRange(new byte[64]);   // trailing padding

        var result = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world00.eb.bytes", fileBytes.ToArray()) });

        // Convention-A items
        Assert.Equal(9,  result[254]);  // Ore
        Assert.Equal(15, result[234]);  // Topaz
        Assert.Equal(1,  result[56]);   // Tiger Racket

        // Convention-C items
        Assert.Equal(4,  result[224]);  // Garnet
        Assert.Equal(8,  result[225]);  // Amethyst
        Assert.Equal(6,  result[231]);  // Peridot
        Assert.Equal(1,  result[29]);   // Ragnarok
    }

    [Fact]
    public void ScanFiles_ConventionC_Deduplication_SameBlockAcrossFiles()
    {
        // Convention-C blocks that appear verbatim in multiple world files
        // must be counted only once (same deduplication guarantee as Convention A).
        byte[] block = ConventionCBlock(224, 4, 225, 8, 231, 6, 29, 1);

        var file1 = FileWithBlockAt(block);
        var file2 = FileWithBlockAt(block);

        var result = WorldMapVariableScanner.ScanFiles(new[]
        {
            ("evt_world_world00.eb.bytes", file1),
            ("evt_world_world03.eb.bytes", file2),
        });

        Assert.Equal(4,  result[224]);  // Garnet — not 8
        Assert.Equal(1,  result[29]);   // Ragnarok — not 2
    }

    [Fact]
    public void ScanFiles_ConventionC_CardIdsExcluded()
    {
        // Card IDs in Convention-C blocks must be excluded, same as other conventions.
        byte[] block = ConventionCBlock(29, 1, 45, 1, 600, 1, 601, 1);
        byte[] file  = FileWithBlockAt(block);

        var result = WorldMapVariableScanner.ScanFiles(
            new[] { ("evt_world_world00.eb.bytes", file) });

        Assert.Equal(1, result[29]);   // Ragnarok
        Assert.Equal(1, result[45]);   // Dragon's Claws
        Assert.DoesNotContain(600, result.Keys);
        Assert.DoesNotContain(601, result.Keys);
    }

    [Fact]
    public void ScanFiles_AllBlocks_SameAsVanillaObtainabilityData_DeadPepperItemCounts()
    {
        // End-to-end: scan all 7 Convention-A blocks (as in world00) + 1 Convention-B block
        // and verify the totals match VanillaObtainabilityData.DeadPepperItemCounts exactly.
        // This validates that the hardcoded reference data was derived correctly from binary.

        byte[] block1 = ConventionABlock(254, 9,  234, 15, 56,  1,  596, 1); // Quan's Dwelling: ...Red Rose Card
        byte[] block2 = ConventionABlock(236, 50, 237, 25, 238, 9,  239, 7); // Iifa Tree: all items
        byte[] block3 = ConventionABlock(113, 8,  217, 8,  148, 7,  195, 8); // Between conts: all 4 items
        byte[] block4 = ConventionABlock(247, 10, 173, 1,  109, 1,  563, 1); // South Forgotten: ...Blue Narciss Card
        byte[] block5 = ConventionABlock(235, 41, 204, 1,  209, 1,  611, 1); // Eastern Lost: ...Airship Card
        byte[] block6 = ConventionABlock(244, 19, 203, 1,  146, 1,  596, 1); // NE Forgotten: ...Hilda Garde I Card
        byte[] block7 = ConventionABlock(222, 1,  40,  1,  111, 1,  575, 1); // Near Oeilvert: ...Odin Card
        byte[] blockB = ConventionBBlock(226, 10,  15, 1,  190, 1,  611, 1); // Unmarked ocean: ...Invincible Card

        var worldABytes = new List<byte>(new byte[100]);
        foreach (byte[] blk in new[] { block1, block2, block3, block4, block5, block6, block7 })
        {
            worldABytes.AddRange(blk);
            worldABytes.AddRange(new byte[16]);
        }
        worldABytes.AddRange(new byte[64]);

        var worldBBytes = new List<byte>(FileWithBlockAt(blockB));

        var result = WorldMapVariableScanner.ScanFiles(new[]
        {
            ("evt_world_world00.eb.bytes", worldABytes.ToArray()),
            ("evt_world_world12.eb.bytes", worldBBytes.ToArray()),
        });

        var expected = VanillaObtainabilityData.DeadPepperItemCounts;

        // Count must match
        Assert.Equal(expected.Count, result.Count);

        // Every entry must match exactly
        foreach (var (id, expectedCount) in expected)
        {
            Assert.True(result.ContainsKey(id),
                $"Scanner result missing item {id}");
            Assert.True(expectedCount == result[id],
                $"Item {id}: expected count {expectedCount}, got {result[id]}");
        }

        // No extra items in scanner result
        foreach (int id in result.Keys)
            Assert.True(expected.ContainsKey(id),
                $"Scanner result contains unexpected item {id}");
    }
}
