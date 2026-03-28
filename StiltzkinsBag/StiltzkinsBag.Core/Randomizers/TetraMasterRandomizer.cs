using StiltzkinsBag.Core.Models;
using StiltzkinsBag.Core.Models.TetraMaster;
using StiltzkinsBag.Models;

namespace StiltzkinsBag.Core.Randomizers;

/// <summary>
/// Randomizes TetraMaster card stats, card types, arrow bitmasks, card identity order,
/// NPC card set contents, and NPC deck assignments.
///
/// All three binary files are read via <see cref="TetraMasterFile"/> static methods.
/// The same applies to per-language card name files (minista.mes).
///
/// RNG call order — strictly enforced for seed determinism:
/// <list type="number">
///   <item>Card stats   (if <see cref="Settings.RandomizeCardStats"/>)</item>
///   <item>Card types   (if <see cref="Settings.CardTypeMode"/> != Preserve)</item>
///   <item>Arrows       (if <see cref="Settings.ArrowMode"/> != Preserve)</item>
///   <item>Card order   (if <see cref="Settings.ShuffleCardOrder"/>)  — 99 calls</item>
///   <item>Card sets    (if <see cref="Settings.RandomizeCardSets"/>)</item>
///   <item>NPC decks    (if <see cref="Settings.ShuffleNpcDecks"/> or difficulty mode != Preserve)</item>
/// </list>
///
/// <see cref="Settings.RandomizeTetraMaster"/> is the caller's master gate —
/// this class does not check it. Callers should skip construction entirely when the flag is false.
/// </summary>
public sealed class TetraMasterRandomizer
{
    private readonly Random _rng;
    private readonly Settings _settings;

    public TetraMasterRandomizer(Random rng, Settings settings)
    {
        _rng = rng ?? throw new ArgumentNullException(nameof(rng));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Runs all enabled TetraMaster randomization sub-steps and returns the patched outputs.
    /// </summary>
    /// <param name="cardStats">
    /// Raw bytes of minigame_card_data_address (must be >= 500 bytes).
    /// </param>
    /// <param name="cardSets">
    /// Raw bytes of minigame_card_level_address (must be >= 1024 bytes).
    /// </param>
    /// <param name="npcDecks">
    /// Raw bytes of minigame_stage_address (must be >= 512 bytes).
    /// </param>
    /// <param name="cardNames">
    /// Per-language raw bytes of minista.mes, keyed by language code (e.g. "us", "jp").
    /// Must contain at least one entry. Ignored when <see cref="Settings.ShuffleCardOrder"/>
    /// is false, but must still be provided (pass an empty dictionary if unavailable).
    /// </param>
    /// <returns>
    /// <see cref="TetraMasterRandomizerResult"/> with patched byte arrays.
    /// <see cref="TetraMasterRandomizerResult.CardNames"/> is null when ShuffleCardOrder is false.
    /// </returns>
    public TetraMasterRandomizerResult Randomize(
        byte[] cardStats,
        byte[] cardSets,
        byte[] npcDecks,
        IReadOnlyDictionary<string, byte[]> cardNames)
    {
        ArgumentNullException.ThrowIfNull(cardStats);
        ArgumentNullException.ThrowIfNull(cardSets);
        ArgumentNullException.ThrowIfNull(npcDecks);
        ArgumentNullException.ThrowIfNull(cardNames);

        // Parse all inputs up front
        var cards = TetraMasterFile.ReadCardStats(cardStats);
        var sets = TetraMasterFile.ReadCardSets(cardSets);
        var decks = TetraMasterFile.ReadNpcDecks(npcDecks);

        // Step 1 — Card stats
        if (_settings.RandomizeCardStats)
            cards = ApplyCardStats(cards);

        // Step 2 — Card types
        if (_settings.CardTypeMode != CardTypeMode.Preserve)
            cards = ApplyCardTypes(cards);

        // Step 3 — Arrows
        if (_settings.ArrowMode != ArrowMode.Preserve)
            cards = ApplyArrows(cards);

        // Step 4 — Card order (same permutation applied to stat entries AND name files)
        Dictionary<string, byte[]>? outputNames = null;
        if (_settings.ShuffleCardOrder)
            (cards, outputNames) = ApplyCardOrder(cards, cardNames);

        // Step 5 — Card sets
        if (_settings.RandomizeCardSets)
            sets = ApplyCardSets(sets);

        // Step 6 + 7 — NPC deck set indices and difficulty
        if (_settings.ShuffleNpcDecks || _settings.NpcDifficultyMode != NpcDifficultyMode.Preserve)
            decks = ApplyNpcDecks(decks);

        return new TetraMasterRandomizerResult(
            CardStats: TetraMasterFile.WriteCardStats(cards),
            CardSets: TetraMasterFile.WriteCardSets(sets),
            NpcDecks: TetraMasterFile.WriteNpcDecks(decks),
            CardNames: outputNames);
    }

    // ── Step 1: Card Stats ─────────────────────────────────────────────────────

    private List<TetraMasterCardEntry> ApplyCardStats(List<TetraMasterCardEntry> cards)
    {
        var mode = _settings.CardStatMode;

        // Debug gate: AllCardsMaxed downgrades to Shuffle when IsDebugMode = false
        if (mode == CardStatMode.AllCardsMaxed && !_settings.IsDebugMode)
            mode = CardStatMode.Shuffle;

        return mode switch
        {
            CardStatMode.Shuffle => ShuffleStats(cards),
            CardStatMode.BoundedRandom => BoundedRandomStats(cards),
            CardStatMode.TierLock => TierLockStats(cards),
            CardStatMode.AllCardsMaxed => AllCardsMaxedStats(cards),
            _ => cards
        };
    }

    /// <summary>
    /// Fisher-Yates shuffle of each stat column independently.
    /// RNG calls: 99 (attack) + 99 (defence) + 99 (magicdefence) = 297.
    /// </summary>
    private List<TetraMasterCardEntry> ShuffleStats(List<TetraMasterCardEntry> cards)
    {
        var attacks = cards.Select(c => c.Attack).ToList();
        var defences = cards.Select(c => c.Defence).ToList();
        var magicDefs = cards.Select(c => c.MagicDefence).ToList();

        FisherYates(attacks);   // 99 RNG calls
        FisherYates(defences);  // 99 RNG calls
        FisherYates(magicDefs); // 99 RNG calls

        return Enumerable.Range(0, TetraMasterFile.CardCount)
            .Select(i => cards[i] with
            {
                Attack = attacks[i],
                Defence = defences[i],
                MagicDefence = magicDefs[i]
            })
            .ToList();
    }

    /// <summary>
    /// Independent random draw per stat per card.
    /// RNG calls: 3 per card × 100 cards = 300. Order per card: Attack, Defence, MagicDefence.
    /// </summary>
    private List<TetraMasterCardEntry> BoundedRandomStats(List<TetraMasterCardEntry> cards)
    {
        int min = Math.Max(1, _settings.CardStatMin);
        int max = Math.Clamp(_settings.CardStatMax, min, 255);

        // Properties in the with-expression are evaluated left-to-right.
        // .ToList() forces eager sequential evaluation (card 0 → card 99).
        return cards.Select(c => c with
        {
            Attack = (byte)_rng.Next(min, max + 1),
            Defence = (byte)_rng.Next(min, max + 1),
            MagicDefence = (byte)_rng.Next(min, max + 1)
        }).ToList();
    }

    /// <summary>
    /// Sort all 100 cards by total stat sum ascending; assign weakest stats to card ID 0
    /// (Goblin), strongest to card ID 99 (Airship). Fully deterministic — 0 RNG calls.
    /// Type and arrow bytes of each original card are preserved.
    /// </summary>
    private static List<TetraMasterCardEntry> TierLockStats(List<TetraMasterCardEntry> cards)
    {
        // Stable sort preserves relative order of cards with equal total stats
        var sorted = cards.OrderBy(c => c.Attack + c.Defence + c.MagicDefence).ToList();

        return Enumerable.Range(0, TetraMasterFile.CardCount)
            .Select(i => cards[i] with
            {
                Attack = sorted[i].Attack,
                Defence = sorted[i].Defence,
                MagicDefence = sorted[i].MagicDefence
            })
            .ToList();
    }

    /// <summary>Debug only. All attack/defence/magicdefence = 255. 0 RNG calls.</summary>
    private static List<TetraMasterCardEntry> AllCardsMaxedStats(List<TetraMasterCardEntry> cards) =>
        cards.Select(c => c with { Attack = 255, Defence = 255, MagicDefence = 255 }).ToList();

    // ── Step 2: Card Types ─────────────────────────────────────────────────────

    private List<TetraMasterCardEntry> ApplyCardTypes(List<TetraMasterCardEntry> cards) =>
        _settings.CardTypeMode switch
        {
            CardTypeMode.Shuffle => ShuffleTypes(cards),
            CardTypeMode.AllP => cards.Select(c => c with { Type = 0 }).ToList(),
            CardTypeMode.AllM => cards.Select(c => c with { Type = 1 }).ToList(),
            _ => cards
        };

    /// <summary>Fisher-Yates shuffle of type bytes. 99 RNG calls.</summary>
    private List<TetraMasterCardEntry> ShuffleTypes(List<TetraMasterCardEntry> cards)
    {
        var types = cards.Select(c => c.Type).ToList();
        FisherYates(types); // 99 RNG calls

        return Enumerable.Range(0, TetraMasterFile.CardCount)
            .Select(i => cards[i] with { Type = types[i] })
            .ToList();
    }

    // ── Step 3: Arrows ─────────────────────────────────────────────────────────

    private List<TetraMasterCardEntry> ApplyArrows(List<TetraMasterCardEntry> cards) =>
        _settings.ArrowMode switch
        {
            // Random: rng.Next(1, 256) guarantees at least 1 bit set. 100 RNG calls.
            ArrowMode.Random => cards.Select(c => c with { Arrows = (byte)_rng.Next(1, 256) }).ToList(),
            // Chaos: rng.Next(0, 256) — 0x00 (no arrows) is possible. 100 RNG calls.
            ArrowMode.Chaos => cards.Select(c => c with { Arrows = (byte)_rng.Next(0, 256) }).ToList(),
            ArrowMode.AllDirections => cards.Select(c => c with { Arrows = 0xFF }).ToList(),
            ArrowMode.NoArrows => cards.Select(c => c with { Arrows = 0x00 }).ToList(),
            _ => cards
        };

    // ── Step 4: Card Order ─────────────────────────────────────────────────────

    /// <summary>
    /// Generates a Fisher-Yates permutation (99 RNG calls) and applies it to both
    /// the card stat entries and every language's name byte arrays.
    /// perm[newIndex] = oldIndex — card in slot newIndex came from oldIndex.
    /// </summary>
    private (List<TetraMasterCardEntry> cards, Dictionary<string, byte[]> names) ApplyCardOrder(
        List<TetraMasterCardEntry> cards,
        IReadOnlyDictionary<string, byte[]> cardNames)
    {
        // Build permutation: perm[newIndex] = oldIndex
        var perm = Enumerable.Range(0, TetraMasterFile.CardCount).ToList();
        FisherYates(perm); // 99 RNG calls

        // Apply to card stat entries
        var reorderedCards = perm.Select(oldIndex => cards[oldIndex]).ToList();

        // Apply identical permutation to each language's name byte arrays
        var reorderedNames = new Dictionary<string, byte[]>(cardNames.Count, StringComparer.OrdinalIgnoreCase);
        foreach (var (lang, mesBytes) in cardNames)
        {
            var names = TetraMasterFile.ReadCardNames(mesBytes);
            var permutedNames = perm.Select(oldIndex => names[oldIndex]).ToList();
            reorderedNames[lang] = TetraMasterFile.WriteCardNames(permutedNames);
        }

        return (reorderedCards, reorderedNames);
    }

    // ── Step 5: Card Sets ──────────────────────────────────────────────────────

    private List<TetraMasterSetEntry> ApplyCardSets(List<TetraMasterSetEntry> sets) =>
        _settings.CardSetMode switch
        {
            CardSetMode.Shuffle => ShuffleSets(sets),
            CardSetMode.BuildFromScratch => BuildSetsFromScratch(),
            _ => sets
        };

    /// <summary>
    /// Fisher-Yates of all 1024 card ID slots as a flat pool; repacked into 64 sets.
    /// 1023 RNG calls.
    /// </summary>
    private List<TetraMasterSetEntry> ShuffleSets(List<TetraMasterSetEntry> sets)
    {
        // Flatten all 64 × 16 = 1024 card IDs
        var flat = new List<byte>(TetraMasterFile.SetCount * TetraMasterFile.SetCapacity);
        foreach (var set in sets)
            flat.AddRange(set.CardIds);

        FisherYates(flat); // 1023 RNG calls

        // Repack into 64 sets of 16
        var result = new List<TetraMasterSetEntry>(TetraMasterFile.SetCount);
        for (int i = 0; i < TetraMasterFile.SetCount; i++)
        {
            var ids = flat
                .GetRange(i * TetraMasterFile.SetCapacity, TetraMasterFile.SetCapacity)
                .ToArray();
            result.Add(new TetraMasterSetEntry(ids));
        }
        return result;
    }

    /// <summary>
    /// Each of the 1024 slots independently draws rng.Next(0, 100).
    /// 1024 RNG calls. Outer loop: sets 0→63. Inner loop: slots 0→15.
    /// </summary>
    private List<TetraMasterSetEntry> BuildSetsFromScratch()
    {
        var result = new List<TetraMasterSetEntry>(TetraMasterFile.SetCount);
        for (int i = 0; i < TetraMasterFile.SetCount; i++)
        {
            var ids = new byte[TetraMasterFile.SetCapacity];
            for (int j = 0; j < TetraMasterFile.SetCapacity; j++)
                ids[j] = (byte)_rng.Next(0, TetraMasterFile.CardCount);
            result.Add(new TetraMasterSetEntry(ids));
        }
        return result;
    }

    // ── Step 6 + 7: NPC Decks ─────────────────────────────────────────────────

    /// <summary>
    /// Optionally shuffles set index bytes (255 RNG calls if ShuffleNpcDecks = true),
    /// then applies the active NpcDifficultyMode.
    /// The two operations are independent — either can run without the other.
    /// </summary>
    private List<TetraMasterDeckEntry> ApplyNpcDecks(List<TetraMasterDeckEntry> decks)
    {
        var setIndices = decks.Select(d => d.SetIndex).ToList();
        var difficulties = decks.Select(d => d.Difficulty).ToList();

        // Step 6 — Shuffle set indices
        if (_settings.ShuffleNpcDecks)
            FisherYates(setIndices); // 255 RNG calls

        // Step 7 — Difficulty mode
        switch (_settings.NpcDifficultyMode)
        {
            case NpcDifficultyMode.Shuffle:
                FisherYates(difficulties); // 255 RNG calls
                break;
            case NpcDifficultyMode.RaiseAll:
                for (int i = 0; i < difficulties.Count; i++) difficulties[i] = 3;
                break;
            case NpcDifficultyMode.LowerAll:
                for (int i = 0; i < difficulties.Count; i++) difficulties[i] = 0;
                break;
                // Preserve: no change
        }

        return Enumerable.Range(0, TetraMasterFile.DeckCount)
            .Select(i => new TetraMasterDeckEntry(setIndices[i], difficulties[i]))
            .ToList();
    }

    // ── Shared utility ─────────────────────────────────────────────────────────

    /// <summary>
    /// Standard Fisher-Yates in-place shuffle. Produces n−1 RNG calls for a list of size n.
    /// All randomizers in this project use this identical implementation.
    /// </summary>
    private void FisherYates<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}