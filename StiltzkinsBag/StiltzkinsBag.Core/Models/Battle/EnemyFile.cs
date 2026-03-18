// StiltzkinsBag.Core/Models/Battle/EnemyFile.cs

namespace StiltzkinsBag.Core.Models.Battle;

/// <summary>
/// Parses and edits a raw FF9 Steam battle enemy file (dbfile0000.raw16.bytes).
///
/// Supports vanilla format (version ≤ 7). Extended HW format (version 8) is detected
/// and rejected with a clear exception — callers must handle this if ever needed.
///
/// Binary layout (vanilla, confirmed against Hades Workshop Enemies.cpp):
///
///   [0]       version         (byte)
///   [1]       group_amount    (byte)   — number of enemy formations in this file
///   [2]       stat_amount     (byte)   — number of distinct enemy stat blocks
///   [3]       spell_amount    (byte)
///   [4–5]     flag            (uint16 LE)
///   [6–7]     zero padding    (uint16)
///
///   [8 .. 8 + group_amount*56)          — group section (56 bytes × group_amount)
///   [8 + group_amount*56 ..)            — stat section  (116 bytes × stat_amount)
///
/// Within each 116-byte stat block, the fields we edit are at these intra-stat offsets:
///
///   item_drop[0..3]   → bytes 20–23   (common, uncommon, rare, super-rare drop item ID)
///   item_steal[0..3]  → bytes 24–27   (common, uncommon, rare, super-rare steal item ID)
///   blue_magic        → byte  71       (learnable blue magic ability ID; 0 = none)
///   card_drop         → byte  105      (Tetramaster card drop item ID; 0 = none)
///
/// All edits operate in-place on the internal byte array.
/// Call ToBytes() to obtain the modified file ready for disk write.
/// </summary>
public sealed class EnemyFile
{
    // ── Header byte offsets ────────────────────────────────────────────────
    private const int OffVersion = 0;
    private const int OffGroupAmount = 1;
    private const int OffStatAmount = 2;
    private const int OffSpellAmount = 3;

    // ── Section sizes (bytes) ──────────────────────────────────────────────
    private const int HeaderSize = 8;
    private const int GroupSize = 56;   // 0x38 per Enemies.cpp UpdateOffset / AddGroup
    private const int StatSize = 116;  // 0x74 per Enemies.cpp UpdateOffset / RemoveStat

    // ── Intra-stat offsets for randomizer-relevant fields (vanilla only) ───
    // Derived by stepping through MACRO_ENEMY_IOFUNCTION field-by-field,
    // accumulating byte widths. Cross-validated against known game data offsets.
    //
    //   status_immune   4 bytes  →  0
    //   status_auto     4 bytes  →  4
    //   status_initial  4 bytes  →  8
    //   hp     uint16   2 bytes  → 12
    //   mp     uint16   2 bytes  → 14
    //   gils   uint16   2 bytes  → 16
    //   exp    uint16   2 bytes  → 18
    //   item_drop[0]    1 byte   → 20  ◄
    //   item_drop[1]    1 byte   → 21  ◄
    //   item_drop[2]    1 byte   → 22  ◄
    //   item_drop[3]    1 byte   → 23  ◄
    //   item_steal[0]   1 byte   → 24  ◄
    //   item_steal[1]   1 byte   → 25  ◄
    //   item_steal[2]   1 byte   → 26  ◄
    //   item_steal[3]   1 byte   → 27  ◄
    //   radius uint16   2 bytes  → 28
    //   model  uint16   2 bytes  → 30
    //   anim_idle       2 bytes  → 32
    //   anim_idle_alt   2 bytes  → 34
    //   anim_hit        2 bytes  → 36
    //   anim_hit_alt    2 bytes  → 38
    //   anim_death      2 bytes  → 40
    //   anim_death_alt  2 bytes  → 42
    //   mesh            2 bytes  → 44
    //   mesh_vanish     2 bytes  → 46
    //   death_flag      2 bytes  → 48
    //   attack  uint16  2 bytes  → 50
    //   speed           1 byte   → 52
    //   strength        1 byte   → 53
    //   magic           1 byte   → 54
    //   spirit          1 byte   → 55
    //   zerostat        1 byte   → 56
    //   trans           1 byte   → 57
    //   cur_capa        1 byte   → 58
    //   max_capa        1 byte   → 59
    //   element_immune  1 byte   → 60
    //   element_absorb  1 byte   → 61
    //   element_half    1 byte   → 62
    //   element_weak    1 byte   → 63
    //   lvl             1 byte   → 64
    //   classification  1 byte   → 65
    //   accuracy        1 byte   → 66
    //   defence         1 byte   → 67
    //   evade           1 byte   → 68
    //   magic_defence   1 byte   → 69
    //   magic_evade     1 byte   → 70
    //   blue_magic      1 byte   → 71  ◄
    //   bone_camera1    1 byte   → 72
    //   bone_camera2    1 byte   → 73
    //   bone_camera3    1 byte   → 74
    //   bone_target     1 byte   → 75
    //   sound_death     2 bytes  → 76
    //   default_attack  1 byte   → 78
    //   text_amount     1 byte   → 79
    //   selection_bone[6]  6 bytes → 80
    //   selection_offsetz[6] 6 bytes → 86
    //   selection_offsety[6] 6 bytes → 92
    //   sound_engage    2 bytes  → 98
    //   shadow_size_x   2 bytes  → 100
    //   shadow_size_y   2 bytes  → 102
    //   shadow_bone1    1 byte   → 104
    //   card_drop       1 byte   → 105  ◄
    //   shadow_offset_x 2 bytes  → 106
    //   shadow_offset_y 2 bytes  → 108
    //   shadow_bone2    1 byte   → 110
    //   zero1           1 byte   → 111
    //   zero2           2 bytes  → 112
    //   zero3           2 bytes  → 114
    //                            total = 116  ✓
    private const int StatOffDrop0 = 20;
    private const int StatOffSteal0 = 24;
    private const int StatOffBlueMagic = 71;
    private const int StatOffCardDrop = 105;

    private const int SlotsPerTable = 4;   // drop[0..3], steal[0..3]
    private const int MaxVersion = 7;   // version 8 = extended HW format, not supported

    // ── Backing data ───────────────────────────────────────────────────────
    private readonly byte[] _data;

    // ── Header properties (read-only from file bytes) ─────────────────────
    public byte Version => _data[OffVersion];
    public int GroupCount => _data[OffGroupAmount];
    public int StatCount => _data[OffStatAmount];
    public int SpellCount => _data[OffSpellAmount];

    /// <summary>
    /// Path relative to game root, as stored in the JSON catalog
    /// (e.g. \StreamingAssets\assets\resources\battlemap\battlescene\...\dbfile0000.raw16.bytes).
    /// Used for output path resolution. Empty if not loaded from catalog.
    /// </summary>
    public string SourcePath { get; }

    // ── Constructor ────────────────────────────────────────────────────────

    /// <param name="data">Raw bytes of the enemy file. A defensive copy is taken.</param>
    /// <param name="sourcePath">
    /// Optional relative path used for output resolution.
    /// Pass the EnemyFolder value from the JSON catalog, or the path inside the archive.
    /// </param>
    public EnemyFile(byte[] data, string sourcePath = "")
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Length < HeaderSize)
            throw new ArgumentException(
                $"Data too short to be a valid enemy file: {data.Length} byte(s), need at least {HeaderSize}.",
                nameof(data));

        if (data[OffVersion] > MaxVersion)
            throw new NotSupportedException(
                $"Extended Hades Workshop enemy format (version {data[OffVersion]}) is not supported. " +
                $"Only vanilla format (version ≤ {MaxVersion}) is handled. " +
                $"Source: '{sourcePath}'");

        _data = (byte[])data.Clone();   // always take a defensive copy
        SourcePath = sourcePath;

        ValidateExpectedSize();
    }

    // ── Read API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Gets a drop item ID for the given stat block and slot.
    /// Slot 0 = common (256/256), 1 = uncommon (96/256), 2 = rare (32/256), 3 = super-rare (1/256).
    /// Item ID 0 means "no item".
    /// </summary>
    public byte GetDrop(int statIndex, int slot)
    {
        ValidateStatIndex(statIndex);
        ValidateSlot(slot);
        return _data[StatOffset(statIndex) + StatOffDrop0 + slot];
    }

    /// <summary>
    /// Gets a steal item ID for the given stat block and slot.
    /// Slot 0 = common (256/256), 1 = uncommon (64/256), 2 = rare (16/256), 3 = super-rare (1/256).
    /// Item ID 0 means "no item".
    /// </summary>
    public byte GetSteal(int statIndex, int slot)
    {
        ValidateStatIndex(statIndex);
        ValidateSlot(slot);
        return _data[StatOffset(statIndex) + StatOffSteal0 + slot];
    }

    /// <summary>
    /// Gets the learnable blue magic ability ID for the given stat block.
    /// 0 means this enemy does not teach blue magic.
    /// </summary>
    public byte GetBlueMagic(int statIndex)
    {
        ValidateStatIndex(statIndex);
        return _data[StatOffset(statIndex) + StatOffBlueMagic];
    }

    /// <summary>
    /// Gets the Tetramaster card drop item ID for the given stat block.
    /// 0 means no card drop.
    /// </summary>
    public byte GetCardDrop(int statIndex)
    {
        ValidateStatIndex(statIndex);
        return _data[StatOffset(statIndex) + StatOffCardDrop];
    }

    // ── Write API ──────────────────────────────────────────────────────────

    /// <summary>Sets a drop item ID in-place.</summary>
    public void SetDrop(int statIndex, int slot, byte value)
    {
        ValidateStatIndex(statIndex);
        ValidateSlot(slot);
        _data[StatOffset(statIndex) + StatOffDrop0 + slot] = value;
    }

    /// <summary>Sets a steal item ID in-place.</summary>
    public void SetSteal(int statIndex, int slot, byte value)
    {
        ValidateStatIndex(statIndex);
        ValidateSlot(slot);
        _data[StatOffset(statIndex) + StatOffSteal0 + slot] = value;
    }

    /// <summary>Sets the blue magic ability ID in-place.</summary>
    public void SetBlueMagic(int statIndex, byte value)
    {
        ValidateStatIndex(statIndex);
        _data[StatOffset(statIndex) + StatOffBlueMagic] = value;
    }

    /// <summary>Sets the card drop item ID in-place.</summary>
    public void SetCardDrop(int statIndex, byte value)
    {
        ValidateStatIndex(statIndex);
        _data[StatOffset(statIndex) + StatOffCardDrop] = value;
    }

    /// <summary>
    /// Returns a copy of the (possibly modified) file bytes, ready to write to disk.
    /// The returned array is a fresh copy — further edits to this EnemyFile will not
    /// affect previously returned arrays.
    /// </summary>
    public byte[] ToBytes() => (byte[])_data.Clone();

    // ── Private helpers ────────────────────────────────────────────────────

    /// <summary>Absolute byte offset of stat block <paramref name="statIndex"/> within the file.</summary>
    private int StatOffset(int statIndex) =>
        HeaderSize + GroupCount * GroupSize + statIndex * StatSize;

    private void ValidateStatIndex(int statIndex)
    {
        if ((uint)statIndex >= (uint)StatCount)
            throw new ArgumentOutOfRangeException(nameof(statIndex),
                $"Stat index {statIndex} is out of range. This file has {StatCount} stat block(s) (indices 0–{StatCount - 1}). " +
                $"Source: '{SourcePath}'");
    }

    private static void ValidateSlot(int slot)
    {
        if ((uint)slot >= SlotsPerTable)
            throw new ArgumentOutOfRangeException(nameof(slot),
                $"Slot {slot} is out of range. Valid slots are 0–{SlotsPerTable - 1}.");
    }

    private void ValidateExpectedSize()
    {
        int minimumSize = HeaderSize + GroupCount * GroupSize + StatCount * StatSize;
        if (_data.Length < minimumSize)
            throw new ArgumentException(
                $"Enemy file data is truncated. " +
                $"Header declares {GroupCount} group(s) and {StatCount} stat(s), " +
                $"requiring at least {minimumSize} bytes, but file is only {_data.Length} bytes. " +
                $"Source: '{SourcePath}'");
    }
}