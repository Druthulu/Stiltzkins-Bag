namespace StiltzkinsBag.Core.Models.TetraMaster;

/// <summary>
/// Stats for a single TetraMaster card as stored in minigame_card_data_address.
/// Binary layout: attack(1) | type(1) | defence(1) | magicdefence(1) | arrows(1) — 5 bytes.
///
/// In the Steam version the "player" stat block is used for both player and NPC matches.
/// The fifth byte is the directional arrow bitmask (8 bits = 8 directions), not a point value,
/// despite Hades Workshop naming it "points".
/// </summary>
public sealed record TetraMasterCardEntry(
    byte Attack,
    byte Type,
    byte Defence,
    byte MagicDefence,
    byte Arrows);

/// <summary>
/// One of the 64 NPC draw pools stored in minigame_card_level_address.
/// Always contains exactly <see cref="TetraMasterFile.SetCapacity"/> (16) card ID bytes.
/// Card IDs are always in [0, 99]. Duplicates within a set are permitted — vanilla uses them.
/// </summary>
public sealed class TetraMasterSetEntry
{
    /// <summary>
    /// The 16 card IDs in this set. Each value is in [0, 99].
    /// Returns a copy — mutating the returned array does not affect this entry.
    /// </summary>
    public byte[] CardIds => (byte[])_cardIds.Clone();

    private readonly byte[] _cardIds;

    /// <param name="cardIds">Must have exactly <see cref="TetraMasterFile.SetCapacity"/> elements.</param>
    public TetraMasterSetEntry(byte[] cardIds)
    {
        if (cardIds.Length != TetraMasterFile.SetCapacity)
            throw new ArgumentException(
                $"CardIds must have exactly {TetraMasterFile.SetCapacity} elements.",
                nameof(cardIds));
        _cardIds = (byte[])cardIds.Clone();
    }

    /// <summary>
    /// Returns the card ID at the given slot index without allocating a copy.
    /// </summary>
    public byte GetCardId(int slot) => _cardIds[slot];
}

/// <summary>
/// One of the 256 NPC deck entries stored in minigame_stage_address.
/// Binary layout: setIndex(1) | difficulty(1) — 2 bytes.
/// <see cref="SetIndex"/> references one of the 64 card sets in minigame_card_level_address.
/// <see cref="Difficulty"/> is always in [0, 3].
/// </summary>
public sealed record TetraMasterDeckEntry(byte SetIndex, byte Difficulty);

/// <summary>
/// Static read/write methods for the three TetraMaster binary files and the
/// per-language card name files (minista.mes).
///
/// All methods are pure: inputs are never mutated, and outputs are new byte arrays.
/// Sizes and counts are validated on both Read and Write paths.
///
/// Files inside resources.assets (extracted via UnityArchiver):
/// <list type="bullet">
///   <item>minigame_card_data_address  — embeddedasset/quadmist/minigame_card_data_address</item>
///   <item>minigame_card_level_address — embeddedasset/quadmist/minigame_card_level_address</item>
///   <item>minigame_stage_address      — embeddedasset/quadmist/minigame_stage_address</item>
///   <item>minista.mes                 — embeddedasset/text/{lang}/etc/minista.mes  (7 languages)</item>
/// </list>
///
/// The first three files have unique short names in resources.assets — use
/// <c>UnityArchiver.Extract(shortName)</c>. The name files share the short name
/// "minista.mes" across languages — use <c>UnityArchiver.ExtractByPath(fullPath)</c>.
///
/// Mod output paths (direct raw file write, no archive repacking):
/// <list type="bullet">
///   <item>{modRoot}\embeddedasset\quadmist\{filename}</item>
///   <item>{modRoot}\embeddedasset\text\{lang}\etc\minista.mes</item>
/// </list>
/// </summary>
public static class TetraMasterFile
{
    // ── Constants ─────────────────────────────────────────────────────────────

    /// <summary>Total number of cards in the game. Always 100.</summary>
    public const int CardCount = 100;

    /// <summary>Total number of NPC card sets. Always 64.</summary>
    public const int SetCount = 64;

    /// <summary>Number of card ID slots per NPC card set. Always 16.</summary>
    public const int SetCapacity = 16;

    /// <summary>Total number of NPC deck entries. Always 256.</summary>
    public const int DeckCount = 256;

    private const int CardEntrySize = 5;   // attack|type|defence|magicdefence|arrows
    private const int DeckEntrySize = 2;   // setIndex|difficulty

    /// <summary>
    /// The literal 6-byte ASCII sequence used to delimit card names in minista.mes.
    /// Value: 0x5B 0x45 0x4E 0x44 0x4E 0x5D  ("[ENDN]").
    /// </summary>
    private static readonly byte[] EndnSeparator = "[ENDN]"u8.ToArray();

    // ── Card Stats (minigame_card_data_address) ────────────────────────────────

    /// <summary>
    /// Parses <paramref name="data"/> into exactly <see cref="CardCount"/> card entries.
    /// Expected minimum size: <see cref="CardCount"/> × 5 = 500 bytes.
    /// </summary>
    /// <exception cref="ArgumentException">Data is shorter than expected.</exception>
    public static List<TetraMasterCardEntry> ReadCardStats(byte[] data)
    {
        int expected = CardCount * CardEntrySize;
        if (data.Length < expected)
            throw new ArgumentException(
                $"Card stats data too short: expected >= {expected} bytes, got {data.Length}.",
                nameof(data));

        var result = new List<TetraMasterCardEntry>(CardCount);
        for (int i = 0; i < CardCount; i++)
        {
            int o = i * CardEntrySize;
            result.Add(new TetraMasterCardEntry(
                Attack: data[o],
                Type: data[o + 1],
                Defence: data[o + 2],
                MagicDefence: data[o + 3],
                Arrows: data[o + 4]));
        }
        return result;
    }

    /// <summary>
    /// Serializes <paramref name="cards"/> to a 500-byte array in
    /// minigame_card_data_address format. List must contain exactly
    /// <see cref="CardCount"/> entries.
    /// </summary>
    /// <exception cref="ArgumentException">List does not contain exactly <see cref="CardCount"/> entries.</exception>
    public static byte[] WriteCardStats(IReadOnlyList<TetraMasterCardEntry> cards)
    {
        if (cards.Count != CardCount)
            throw new ArgumentException(
                $"Expected exactly {CardCount} card entries, got {cards.Count}.",
                nameof(cards));

        var output = new byte[CardCount * CardEntrySize];
        for (int i = 0; i < CardCount; i++)
        {
            int o = i * CardEntrySize;
            var c = cards[i];
            output[o] = c.Attack;
            output[o + 1] = c.Type;
            output[o + 2] = c.Defence;
            output[o + 3] = c.MagicDefence;
            output[o + 4] = c.Arrows;
        }
        return output;
    }

    // ── Card Sets (minigame_card_level_address) ────────────────────────────────

    /// <summary>
    /// Parses <paramref name="data"/> into exactly <see cref="SetCount"/> card sets.
    /// Expected minimum size: <see cref="SetCount"/> × <see cref="SetCapacity"/> = 1024 bytes.
    /// </summary>
    /// <exception cref="ArgumentException">Data is shorter than expected.</exception>
    public static List<TetraMasterSetEntry> ReadCardSets(byte[] data)
    {
        int expected = SetCount * SetCapacity;
        if (data.Length < expected)
            throw new ArgumentException(
                $"Card sets data too short: expected >= {expected} bytes, got {data.Length}.",
                nameof(data));

        var result = new List<TetraMasterSetEntry>(SetCount);
        for (int i = 0; i < SetCount; i++)
        {
            var ids = new byte[SetCapacity];
            Array.Copy(data, i * SetCapacity, ids, 0, SetCapacity);
            result.Add(new TetraMasterSetEntry(ids));
        }
        return result;
    }

    /// <summary>
    /// Serializes <paramref name="sets"/> to a 1024-byte array in
    /// minigame_card_level_address format. List must contain exactly
    /// <see cref="SetCount"/> entries, each with exactly <see cref="SetCapacity"/> card IDs.
    /// </summary>
    /// <exception cref="ArgumentException">List count is wrong.</exception>
    public static byte[] WriteCardSets(IReadOnlyList<TetraMasterSetEntry> sets)
    {
        if (sets.Count != SetCount)
            throw new ArgumentException(
                $"Expected exactly {SetCount} set entries, got {sets.Count}.",
                nameof(sets));

        var output = new byte[SetCount * SetCapacity];
        for (int i = 0; i < SetCount; i++)
        {
            var ids = sets[i].CardIds;
            Array.Copy(ids, 0, output, i * SetCapacity, SetCapacity);
        }
        return output;
    }

    // ── NPC Decks (minigame_stage_address) ────────────────────────────────────

    /// <summary>
    /// Parses <paramref name="data"/> into exactly <see cref="DeckCount"/> deck entries.
    /// Expected minimum size: <see cref="DeckCount"/> × 2 = 512 bytes.
    /// </summary>
    /// <exception cref="ArgumentException">Data is shorter than expected.</exception>
    public static List<TetraMasterDeckEntry> ReadNpcDecks(byte[] data)
    {
        int expected = DeckCount * DeckEntrySize;
        if (data.Length < expected)
            throw new ArgumentException(
                $"NPC decks data too short: expected >= {expected} bytes, got {data.Length}.",
                nameof(data));

        var result = new List<TetraMasterDeckEntry>(DeckCount);
        for (int i = 0; i < DeckCount; i++)
        {
            int o = i * DeckEntrySize;
            result.Add(new TetraMasterDeckEntry(
                SetIndex: data[o],
                Difficulty: data[o + 1]));
        }
        return result;
    }

    /// <summary>
    /// Serializes <paramref name="decks"/> to a 512-byte array in
    /// minigame_stage_address format. List must contain exactly
    /// <see cref="DeckCount"/> entries.
    /// </summary>
    /// <exception cref="ArgumentException">List does not contain exactly <see cref="DeckCount"/> entries.</exception>
    public static byte[] WriteNpcDecks(IReadOnlyList<TetraMasterDeckEntry> decks)
    {
        if (decks.Count != DeckCount)
            throw new ArgumentException(
                $"Expected exactly {DeckCount} deck entries, got {decks.Count}.",
                nameof(decks));

        var output = new byte[DeckCount * DeckEntrySize];
        for (int i = 0; i < DeckCount; i++)
        {
            int o = i * DeckEntrySize;
            output[o] = decks[i].SetIndex;
            output[o + 1] = decks[i].Difficulty;
        }
        return output;
    }

    // ── Card Names (minista.mes, per language) ─────────────────────────────────

    /// <summary>
    /// Parses a minista.mes file into a list of exactly <see cref="CardCount"/>
    /// raw name byte arrays.
    ///
    /// Format: name₀[ENDN]name₁[ENDN]…name₉₉[ENDN]
    /// The [ENDN] sequence is the 6 literal ASCII bytes 0x5B 0x45 0x4E 0x44 0x4E 0x5D.
    ///
    /// Name byte arrays are never decoded to strings. They are treated as opaque
    /// byte sequences so that encoding is preserved for every language file
    /// (Japanese, etc.) without any conversion.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// File does not split into exactly <see cref="CardCount"/> names.
    /// </exception>
    public static List<byte[]> ReadCardNames(byte[] data)
    {
        var segments = SplitOnSeparator(data, EndnSeparator);

        // SplitOnSeparator produces a trailing empty segment because the file
        // ends with [ENDN]. Drop it.
        if (segments.Count > 0 && segments[^1].Length == 0)
            segments.RemoveAt(segments.Count - 1);

        if (segments.Count != CardCount)
            throw new InvalidDataException(
                $"Card names file contains {segments.Count} name(s); expected {CardCount}.");

        return segments;
    }

    /// <summary>
    /// Serializes <paramref name="names"/> to the minista.mes byte format.
    /// List must contain exactly <see cref="CardCount"/> name byte arrays.
    /// Produces: name₀[ENDN]name₁[ENDN]…name₉₉[ENDN]
    /// </summary>
    /// <exception cref="ArgumentException">List does not contain exactly <see cref="CardCount"/> entries.</exception>
    public static byte[] WriteCardNames(IReadOnlyList<byte[]> names)
    {
        if (names.Count != CardCount)
            throw new ArgumentException(
                $"Expected exactly {CardCount} name entries, got {names.Count}.",
                nameof(names));

        int separatorLen = EndnSeparator.Length;
        int totalSize = names.Sum(n => n.Length) + CardCount * separatorLen;
        var output = new byte[totalSize];
        int pos = 0;

        for (int i = 0; i < CardCount; i++)
        {
            var name = names[i];
            name.CopyTo(output, pos);
            pos += name.Length;
            EndnSeparator.CopyTo(output, pos);
            pos += separatorLen;
        }

        return output;
    }

    // ── Internal helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Splits <paramref name="source"/> on every occurrence of <paramref name="separator"/>,
    /// returning a list of segment byte arrays. The segment after the final separator
    /// (which may be empty) is always included.
    /// </summary>
    private static List<byte[]> SplitOnSeparator(byte[] source, byte[] separator)
    {
        var result = new List<byte[]>();
        int sepLen = separator.Length;
        int start = 0;
        int limit = source.Length - sepLen;

        for (int i = 0; i <= limit; i++)
        {
            if (MatchesAt(source, i, separator))
            {
                int segLen = i - start;
                var segment = new byte[segLen];
                Array.Copy(source, start, segment, 0, segLen);
                result.Add(segment);
                i += sepLen - 1; // skip separator body; loop i++ advances past it
                start = i + 1;
            }
        }

        // Append whatever remains after the last separator
        int tailLen = source.Length - start;
        var tail = new byte[tailLen];
        Array.Copy(source, start, tail, 0, tailLen);
        result.Add(tail);

        return result;
    }

    private static bool MatchesAt(byte[] source, int index, byte[] pattern)
    {
        for (int j = 0; j < pattern.Length; j++)
        {
            if (source[index + j] != pattern[j])
                return false;
        }
        return true;
    }
}