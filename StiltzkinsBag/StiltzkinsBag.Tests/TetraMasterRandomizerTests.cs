using StiltzkinsBag.Core.Models;
using StiltzkinsBag.Core.Models.TetraMaster;
using StiltzkinsBag.Core.Randomizers;
using StiltzkinsBag.Models;
using Xunit;

namespace StiltzkinsBag.Tests;

/// <summary>
/// Tests for <see cref="TetraMasterFile"/> (read/write round-trips)
/// and <see cref="TetraMasterRandomizer"/> (all randomization sub-steps).
///
/// Test data files live in TestData/TetraMaster/ and are copied to the output directory.
/// Sizes confirmed from analysis:
///   minigame_card_data_address  — 500 bytes  (100 cards × 5 bytes)
///   minigame_card_level_address — 1024 bytes (64 sets × 16 card IDs)
///   minigame_stage_address      — 512 bytes  (256 decks × 2 bytes)
///   minista.mes                 — 1369 bytes (US language card names)
///
/// Note: test data is from an AlternateFantasy-modded install. Card stat values
/// reach up to 255 (AF raises vanilla 0–15 range). Tests do not hardcode vanilla
/// stat values — they operate on whatever the test data contains.
/// </summary>
public class TetraMasterRandomizerTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static string TestDataPath(string filename) =>
        Path.Combine(AppContext.BaseDirectory, "TestData", "TetraMaster", filename);

    private static byte[] LoadCardStats() => File.ReadAllBytes(TestDataPath("minigame_card_data_address"));
    private static byte[] LoadCardSets() => File.ReadAllBytes(TestDataPath("minigame_card_level_address"));
    private static byte[] LoadNpcDecks() => File.ReadAllBytes(TestDataPath("minigame_stage_address"));
    private static byte[] LoadCardNames() => File.ReadAllBytes(TestDataPath("minista.mes"));

    /// <summary>
    /// Returns a card names dictionary with the real US file plus a second entry
    /// (keyed "fr") that uses the same bytes — sufficient for multi-language tests.
    /// </summary>
    private static Dictionary<string, byte[]> MakeCardNames()
    {
        var usBytes = LoadCardNames();
        return new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["us"] = (byte[])usBytes.Clone(),
            ["fr"] = (byte[])usBytes.Clone()
        };
    }

    /// <summary>Builds a Settings with all TetraMaster flags disabled by default.</summary>
    private static Settings MakeSettings(Action<Settings>? configure = null)
    {
        var s = new Settings
        {
            CardStatMode = CardStatMode.Shuffle,
            CardTypeMode = CardTypeMode.Preserve,
            ArrowMode = ArrowMode.Preserve,
            CardSetMode = CardSetMode.Shuffle,
            NpcDifficultyMode = NpcDifficultyMode.Preserve
        };
        configure?.Invoke(s);
        return s;
    }

    private static TetraMasterRandomizer MakeRandomizer(int seed = 42, Action<Settings>? configure = null)
        => new(new Random(seed), MakeSettings(configure));

    private static TetraMasterRandomizerResult RunRandomize(
        TetraMasterRandomizer r,
        byte[]? cardStats = null,
        byte[]? cardSets = null,
        byte[]? npcDecks = null,
        IReadOnlyDictionary<string, byte[]>? names = null)
        => r.Randomize(
            cardStats ?? LoadCardStats(),
            cardSets ?? LoadCardSets(),
            npcDecks ?? LoadNpcDecks(),
            names ?? MakeCardNames());

    // ─────────────────────────────────────────────────────────────────────────
    // Round-trip tests (TetraMasterFile)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void ReadWriteCardStats_RoundTrip_ByteIdentical()
    {
        var original = LoadCardStats();
        var entries = TetraMasterFile.ReadCardStats(original);
        var output = TetraMasterFile.WriteCardStats(entries);
        Assert.Equal(original, output);
    }

    [Fact]
    public void ReadWriteCardSets_RoundTrip_ByteIdentical()
    {
        var original = LoadCardSets();
        var entries = TetraMasterFile.ReadCardSets(original);
        var output = TetraMasterFile.WriteCardSets(entries);
        Assert.Equal(original, output);
    }

    [Fact]
    public void ReadWriteNpcDecks_RoundTrip_ByteIdentical()
    {
        var original = LoadNpcDecks();
        var entries = TetraMasterFile.ReadNpcDecks(original);
        var output = TetraMasterFile.WriteNpcDecks(entries);
        Assert.Equal(original, output);
    }

    [Fact]
    public void ReadWriteCardNames_RoundTrip_ByteIdentical()
    {
        var original = LoadCardNames();
        var names = TetraMasterFile.ReadCardNames(original);
        var output = TetraMasterFile.WriteCardNames(names);
        Assert.Equal(original, output);
    }

    [Fact]
    public void ReadCardStats_Returns100Entries()
    {
        var entries = TetraMasterFile.ReadCardStats(LoadCardStats());
        Assert.Equal(TetraMasterFile.CardCount, entries.Count);
    }

    [Fact]
    public void ReadCardSets_Returns64SetsEachWith16Ids()
    {
        var sets = TetraMasterFile.ReadCardSets(LoadCardSets());
        Assert.Equal(TetraMasterFile.SetCount, sets.Count);
        Assert.All(sets, s => Assert.Equal(TetraMasterFile.SetCapacity, s.CardIds.Length));
    }

    [Fact]
    public void ReadNpcDecks_Returns256Entries()
    {
        var decks = TetraMasterFile.ReadNpcDecks(LoadNpcDecks());
        Assert.Equal(TetraMasterFile.DeckCount, decks.Count);
    }

    [Fact]
    public void ReadCardNames_Returns100Names()
    {
        var names = TetraMasterFile.ReadCardNames(LoadCardNames());
        Assert.Equal(TetraMasterFile.CardCount, names.Count);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CardStats — Shuffle
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void CardStats_Shuffle_IsDeterministic()
    {
        var r1 = MakeRandomizer(42, s => s.RandomizeCardStats = true);
        var r2 = MakeRandomizer(42, s => s.RandomizeCardStats = true);
        Assert.Equal(RunRandomize(r1).CardStats, RunRandomize(r2).CardStats);
    }

    [Fact]
    public void CardStats_Shuffle_TypeAndArrowsUnchanged()
    {
        var original = TetraMasterFile.ReadCardStats(LoadCardStats());
        var r = MakeRandomizer(42, s => s.RandomizeCardStats = true);
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);

        for (int i = 0; i < TetraMasterFile.CardCount; i++)
        {
            Assert.Equal(original[i].Type, result[i].Type);
            Assert.Equal(original[i].Arrows, result[i].Arrows);
        }
    }

    [Fact]
    public void CardStats_Shuffle_AttackPoolIsPermutationOfOriginal()
    {
        var original = TetraMasterFile.ReadCardStats(LoadCardStats());
        var r = MakeRandomizer(42, s => s.RandomizeCardStats = true);
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);

        var originalAttacks = original.Select(c => c.Attack).OrderBy(x => x).ToList();
        var resultAttacks = result.Select(c => c.Attack).OrderBy(x => x).ToList();
        Assert.Equal(originalAttacks, resultAttacks);
    }

    [Fact]
    public void CardStats_Shuffle_AllValuesInByteRange()
    {
        var r = MakeRandomizer(42, s => s.RandomizeCardStats = true);
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);
        // byte range is [0,255] — trivially true for byte type; this guards against sign extension bugs
        Assert.All(result, c =>
        {
            Assert.InRange(c.Attack, 0, 255);
            Assert.InRange(c.Defence, 0, 255);
            Assert.InRange(c.MagicDefence, 0, 255);
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CardStats — BoundedRandom
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void CardStats_BoundedRandom_IsDeterministic()
    {
        var r1 = MakeRandomizer(99, s => { s.RandomizeCardStats = true; s.CardStatMode = CardStatMode.BoundedRandom; s.CardStatMin = 1; s.CardStatMax = 15; });
        var r2 = MakeRandomizer(99, s => { s.RandomizeCardStats = true; s.CardStatMode = CardStatMode.BoundedRandom; s.CardStatMin = 1; s.CardStatMax = 15; });
        Assert.Equal(RunRandomize(r1).CardStats, RunRandomize(r2).CardStats);
    }

    [Fact]
    public void CardStats_BoundedRandom_AllStatsInConfiguredRange()
    {
        const int min = 3, max = 12;
        var r = MakeRandomizer(42, s => { s.RandomizeCardStats = true; s.CardStatMode = CardStatMode.BoundedRandom; s.CardStatMin = min; s.CardStatMax = max; });
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);

        Assert.All(result, c =>
        {
            Assert.InRange(c.Attack, min, max);
            Assert.InRange(c.Defence, min, max);
            Assert.InRange(c.MagicDefence, min, max);
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CardStats — TierLock
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void CardStats_TierLock_StatSumsMonotonicallyNonDecreasing()
    {
        var r = MakeRandomizer(42, s => { s.RandomizeCardStats = true; s.CardStatMode = CardStatMode.TierLock; });
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);

        for (int i = 1; i < TetraMasterFile.CardCount; i++)
        {
            int prev = result[i - 1].Attack + result[i - 1].Defence + result[i - 1].MagicDefence;
            int curr = result[i].Attack + result[i].Defence + result[i].MagicDefence;
            Assert.True(curr >= prev, $"Card {i} total ({curr}) is less than card {i - 1} total ({prev}).");
        }
    }

    [Fact]
    public void CardStats_TierLock_TypeAndArrowsAnchoredToOriginalCardSlot()
    {
        var original = TetraMasterFile.ReadCardStats(LoadCardStats());
        var r = MakeRandomizer(42, s => { s.RandomizeCardStats = true; s.CardStatMode = CardStatMode.TierLock; });
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);

        // TierLock reassigns stats but keeps Type and Arrows from the original card at each slot
        for (int i = 0; i < TetraMasterFile.CardCount; i++)
        {
            Assert.Equal(original[i].Type, result[i].Type);
            Assert.Equal(original[i].Arrows, result[i].Arrows);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CardStats — AllCardsMaxed (debug gate)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void CardStats_AllCardsMaxed_DebugTrue_AllStatsAre255()
    {
        var r = MakeRandomizer(42, s => { s.RandomizeCardStats = true; s.CardStatMode = CardStatMode.AllCardsMaxed; s.IsDebugMode = true; });
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);

        Assert.All(result, c =>
        {
            Assert.Equal(255, c.Attack);
            Assert.Equal(255, c.Defence);
            Assert.Equal(255, c.MagicDefence);
        });
    }

    [Fact]
    public void CardStats_AllCardsMaxed_DebugFalse_DowngradesToShuffle_TypeAndArrowsUnchanged()
    {
        var original = TetraMasterFile.ReadCardStats(LoadCardStats());
        // Debug=false → AllCardsMaxed downgrades to Shuffle
        var r = MakeRandomizer(42, s => { s.RandomizeCardStats = true; s.CardStatMode = CardStatMode.AllCardsMaxed; s.IsDebugMode = false; });
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);

        // Stats must have changed (Shuffle with seed 42 produces different output than input)
        Assert.False(original.Select(c => c.Attack).SequenceEqual(result.Select(c => c.Attack)),
            "Expected Shuffle to reorder attacks but output matched input exactly.");

        // Type and Arrows must be unchanged (Shuffle preserves them)
        for (int i = 0; i < TetraMasterFile.CardCount; i++)
        {
            Assert.Equal(original[i].Type, result[i].Type);
            Assert.Equal(original[i].Arrows, result[i].Arrows);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CardTypes
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void CardType_Preserve_TypeBytesUnchanged()
    {
        var original = TetraMasterFile.ReadCardStats(LoadCardStats());
        var r = MakeRandomizer(42, s => { s.RandomizeCardStats = true; s.CardTypeMode = CardTypeMode.Preserve; });
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);

        Assert.Equal(original.Select(c => c.Type), result.Select(c => c.Type));
    }

    [Fact]
    public void CardType_Shuffle_TypeDistributionPreserved()
    {
        var original = TetraMasterFile.ReadCardStats(LoadCardStats());
        var r = MakeRandomizer(42, s => s.CardTypeMode = CardTypeMode.Shuffle);
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);

        Assert.Equal(
            original.GroupBy(c => c.Type).ToDictionary(g => g.Key, g => g.Count()),
            result.GroupBy(c => c.Type).ToDictionary(g => g.Key, g => g.Count()));
    }

    [Fact]
    public void CardType_Shuffle_StatsUnchanged()
    {
        var original = TetraMasterFile.ReadCardStats(LoadCardStats());
        var r = MakeRandomizer(42, s => s.CardTypeMode = CardTypeMode.Shuffle);
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);

        for (int i = 0; i < TetraMasterFile.CardCount; i++)
        {
            Assert.Equal(original[i].Attack, result[i].Attack);
            Assert.Equal(original[i].Defence, result[i].Defence);
            Assert.Equal(original[i].MagicDefence, result[i].MagicDefence);
        }
    }

    [Fact]
    public void CardType_AllP_AllTypesAreZero()
    {
        var r = MakeRandomizer(42, s => s.CardTypeMode = CardTypeMode.AllP);
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);
        Assert.All(result, c => Assert.Equal(0, c.Type));
    }

    [Fact]
    public void CardType_AllM_AllTypesAreOne()
    {
        var r = MakeRandomizer(42, s => s.CardTypeMode = CardTypeMode.AllM);
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);
        Assert.All(result, c => Assert.Equal(1, c.Type));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Arrows
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Arrow_Preserve_ArrowBytesUnchanged()
    {
        var original = TetraMasterFile.ReadCardStats(LoadCardStats());
        var r = MakeRandomizer(42, s => s.ArrowMode = ArrowMode.Preserve);
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);
        Assert.Equal(original.Select(c => c.Arrows), result.Select(c => c.Arrows));
    }

    [Fact]
    public void Arrow_Random_AllValuesAtLeastOneBitSet()
    {
        var r = MakeRandomizer(42, s => s.ArrowMode = ArrowMode.Random);
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);
        Assert.All(result, c => Assert.NotEqual(0, c.Arrows));
    }

    [Fact]
    public void Arrow_Chaos_AllValuesInByteRange()
    {
        var r = MakeRandomizer(42, s => s.ArrowMode = ArrowMode.Chaos);
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);
        Assert.All(result, c => Assert.InRange(c.Arrows, 0, 255));
    }

    [Fact]
    public void Arrow_AllDirections_AllAreByte0xFF()
    {
        var r = MakeRandomizer(42, s => s.ArrowMode = ArrowMode.AllDirections);
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);
        Assert.All(result, c => Assert.Equal(0xFF, c.Arrows));
    }

    [Fact]
    public void Arrow_NoArrows_AllAreByte0x00()
    {
        var r = MakeRandomizer(42, s => s.ArrowMode = ArrowMode.NoArrows);
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);
        Assert.All(result, c => Assert.Equal(0x00, c.Arrows));
    }

    [Fact]
    public void Arrow_Random_IsDeterministic()
    {
        var r1 = MakeRandomizer(7, s => s.ArrowMode = ArrowMode.Random);
        var r2 = MakeRandomizer(7, s => s.ArrowMode = ArrowMode.Random);
        Assert.Equal(RunRandomize(r1).CardStats, RunRandomize(r2).CardStats);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ShuffleCardOrder
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void CardOrder_False_CardNamesIsNull()
    {
        var r = MakeRandomizer(42, s => s.ShuffleCardOrder = false);
        var result = RunRandomize(r);
        Assert.Null(result.CardNames);
    }

    [Fact]
    public void CardOrder_True_StatMultisetPreserved()
    {
        var original = TetraMasterFile.ReadCardStats(LoadCardStats());
        var r = MakeRandomizer(42, s => s.ShuffleCardOrder = true);
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);

        // Every (attack, type, defence, magicdefence, arrows) tuple in input
        // must appear exactly once in output
        var inputKeys = original.Select(StatKey).OrderBy(x => x).ToList();
        var resultKeys = result.Select(StatKey).OrderBy(x => x).ToList();
        Assert.Equal(inputKeys, resultKeys);

        static string StatKey(TetraMasterCardEntry c) =>
            $"{c.Attack},{c.Type},{c.Defence},{c.MagicDefence},{c.Arrows}";
    }

    [Fact]
    public void CardOrder_True_NameMultisetPreservedPerLanguage()
    {
        var inputNames = TetraMasterFile.ReadCardNames(LoadCardNames());
        var r = MakeRandomizer(42, s => s.ShuffleCardOrder = true);
        var result = RunRandomize(r);

        Assert.NotNull(result.CardNames);
        foreach (var (lang, mesBytes) in result.CardNames!)
        {
            var outputNames = TetraMasterFile.ReadCardNames(mesBytes);
            var inputHex = inputNames.Select(n => BitConverter.ToString(n)).OrderBy(x => x).ToList();
            var outputHex = outputNames.Select(n => BitConverter.ToString(n)).OrderBy(x => x).ToList();
            Assert.Equal(inputHex, outputHex);
        }
    }

    [Fact]
    public void CardOrder_True_CoPermutationConsistent()
    {
        // Card 70 (Excalibur II) has a unique stat signature in the test data:
        // attack=255 appears only on this card. After ShuffleCardOrder, whichever
        // slot Excalibur II's stats land in, that slot's name must be the same
        // bytes as the original minista.mes name at index 70.
        var originalCards = TetraMasterFile.ReadCardStats(LoadCardStats());
        var originalNames = TetraMasterFile.ReadCardNames(LoadCardNames());
        const int anchorIndex = 70;
        var anchorStats = originalCards[anchorIndex];

        var r = MakeRandomizer(42, s => s.ShuffleCardOrder = true);
        var result = RunRandomize(r);

        var resultCards = TetraMasterFile.ReadCardStats(result.CardStats);
        var resultNames = TetraMasterFile.ReadCardNames(result.CardNames!["us"]);

        // Find which slot holds Excalibur II's stats after the shuffle
        int newPos = resultCards.FindIndex(
            c => c.Attack == anchorStats.Attack &&
                 c.Type == anchorStats.Type &&
                 c.Defence == anchorStats.Defence &&
                 c.MagicDefence == anchorStats.MagicDefence);

        Assert.True(newPos >= 0, "Excalibur II stat signature not found in shuffled output.");
        Assert.Equal(originalNames[anchorIndex], resultNames[newPos]);
    }

    [Fact]
    public void CardOrder_True_MultipleLanguagesPermutedIdentically()
    {
        // "us" and "fr" both start with the same bytes (identical US file duplicated in MakeCardNames).
        // After shuffle both must be permuted with the same permutation, so outputs are equal.
        var r = MakeRandomizer(42, s => s.ShuffleCardOrder = true);
        var result = RunRandomize(r);

        Assert.NotNull(result.CardNames);
        Assert.True(result.CardNames!.ContainsKey("us") && result.CardNames.ContainsKey("fr"));
        Assert.Equal(result.CardNames["us"], result.CardNames["fr"]);
    }

    [Fact]
    public void CardOrder_True_IsDeterministic()
    {
        var r1 = MakeRandomizer(13, s => s.ShuffleCardOrder = true);
        var r2 = MakeRandomizer(13, s => s.ShuffleCardOrder = true);
        var res1 = RunRandomize(r1);
        var res2 = RunRandomize(r2);

        Assert.Equal(res1.CardStats, res2.CardStats);
        Assert.Equal(res1.CardNames!["us"], res2.CardNames!["us"]);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CardSets
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void CardSets_Shuffle_AllCardIdsInValidRange()
    {
        var r = MakeRandomizer(42, s => s.RandomizeCardSets = true);
        var result = TetraMasterFile.ReadCardSets(RunRandomize(r).CardSets);
        Assert.All(result, set =>
            Assert.All(set.CardIds, id => Assert.InRange(id, 0, TetraMasterFile.CardCount - 1)));
    }

    [Fact]
    public void CardSets_Shuffle_FlatPoolPreserved()
    {
        var original = TetraMasterFile.ReadCardSets(LoadCardSets());
        var r = MakeRandomizer(42, s => s.RandomizeCardSets = true);
        var result = TetraMasterFile.ReadCardSets(RunRandomize(r).CardSets);

        // Fisher-Yates of flat pool — total multiset of card IDs is preserved
        var originalFlat = original.SelectMany(s => s.CardIds).OrderBy(x => x).ToList();
        var resultFlat = result.SelectMany(s => s.CardIds).OrderBy(x => x).ToList();
        Assert.Equal(originalFlat, resultFlat);
    }

    [Fact]
    public void CardSets_Shuffle_IsDeterministic()
    {
        var r1 = MakeRandomizer(42, s => s.RandomizeCardSets = true);
        var r2 = MakeRandomizer(42, s => s.RandomizeCardSets = true);
        Assert.Equal(RunRandomize(r1).CardSets, RunRandomize(r2).CardSets);
    }

    [Fact]
    public void CardSets_BuildFromScratch_AllCardIdsInValidRange()
    {
        var r = MakeRandomizer(42, s => { s.RandomizeCardSets = true; s.CardSetMode = CardSetMode.BuildFromScratch; });
        var result = TetraMasterFile.ReadCardSets(RunRandomize(r).CardSets);
        Assert.All(result, set =>
            Assert.All(set.CardIds, id => Assert.InRange(id, 0, TetraMasterFile.CardCount - 1)));
    }

    [Fact]
    public void CardSets_BuildFromScratch_Produces64SetsOf16Slots()
    {
        var r = MakeRandomizer(42, s => { s.RandomizeCardSets = true; s.CardSetMode = CardSetMode.BuildFromScratch; });
        var result = TetraMasterFile.ReadCardSets(RunRandomize(r).CardSets);
        Assert.Equal(TetraMasterFile.SetCount, result.Count);
        Assert.All(result, set => Assert.Equal(TetraMasterFile.SetCapacity, set.CardIds.Length));
    }

    [Fact]
    public void CardSets_BuildFromScratch_IsDeterministic()
    {
        var r1 = MakeRandomizer(42, s => { s.RandomizeCardSets = true; s.CardSetMode = CardSetMode.BuildFromScratch; });
        var r2 = MakeRandomizer(42, s => { s.RandomizeCardSets = true; s.CardSetMode = CardSetMode.BuildFromScratch; });
        Assert.Equal(RunRandomize(r1).CardSets, RunRandomize(r2).CardSets);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // NpcDecks
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void NpcDecks_ShuffleSetIndices_SetMultisetPreserved()
    {
        var original = TetraMasterFile.ReadNpcDecks(LoadNpcDecks());
        var r = MakeRandomizer(42, s => s.ShuffleNpcDecks = true);
        var result = TetraMasterFile.ReadNpcDecks(RunRandomize(r).NpcDecks);

        var originalSets = original.Select(d => d.SetIndex).OrderBy(x => x).ToList();
        var resultSets = result.Select(d => d.SetIndex).OrderBy(x => x).ToList();
        Assert.Equal(originalSets, resultSets);
    }

    [Fact]
    public void NpcDecks_ShuffleSetIndices_DifficultyUnchangedWhenPreserve()
    {
        var original = TetraMasterFile.ReadNpcDecks(LoadNpcDecks());
        var r = MakeRandomizer(42, s => { s.ShuffleNpcDecks = true; s.NpcDifficultyMode = NpcDifficultyMode.Preserve; });
        var result = TetraMasterFile.ReadNpcDecks(RunRandomize(r).NpcDecks);

        Assert.Equal(
            original.Select(d => d.Difficulty),
            result.Select(d => d.Difficulty));
    }

    [Fact]
    public void NpcDecks_DifficultyPreserve_DifficultyBytesUnchanged()
    {
        var original = TetraMasterFile.ReadNpcDecks(LoadNpcDecks());
        var r = MakeRandomizer(42, s => s.NpcDifficultyMode = NpcDifficultyMode.Preserve);
        var result = TetraMasterFile.ReadNpcDecks(RunRandomize(r).NpcDecks);

        Assert.Equal(
            original.Select(d => d.Difficulty),
            result.Select(d => d.Difficulty));
    }

    [Fact]
    public void NpcDecks_DifficultyRaiseAll_AllAre3()
    {
        var r = MakeRandomizer(42, s => s.NpcDifficultyMode = NpcDifficultyMode.RaiseAll);
        var result = TetraMasterFile.ReadNpcDecks(RunRandomize(r).NpcDecks);
        Assert.All(result, d => Assert.Equal(3, d.Difficulty));
    }

    [Fact]
    public void NpcDecks_DifficultyLowerAll_AllAre0()
    {
        var r = MakeRandomizer(42, s => s.NpcDifficultyMode = NpcDifficultyMode.LowerAll);
        var result = TetraMasterFile.ReadNpcDecks(RunRandomize(r).NpcDecks);
        Assert.All(result, d => Assert.Equal(0, d.Difficulty));
    }

    [Fact]
    public void NpcDecks_DifficultyShuffle_DifficultyMultisetPreserved()
    {
        var original = TetraMasterFile.ReadNpcDecks(LoadNpcDecks());
        var r = MakeRandomizer(42, s => s.NpcDifficultyMode = NpcDifficultyMode.Shuffle);
        var result = TetraMasterFile.ReadNpcDecks(RunRandomize(r).NpcDecks);

        var originalDiffs = original.Select(d => d.Difficulty).OrderBy(x => x).ToList();
        var resultDiffs = result.Select(d => d.Difficulty).OrderBy(x => x).ToList();
        Assert.Equal(originalDiffs, resultDiffs);
    }

    [Fact]
    public void NpcDecks_ShuffleSetAndDifficulty_BothApplied()
    {
        var original = TetraMasterFile.ReadNpcDecks(LoadNpcDecks());
        var r = MakeRandomizer(42, s => { s.ShuffleNpcDecks = true; s.NpcDifficultyMode = NpcDifficultyMode.Shuffle; });
        var result = TetraMasterFile.ReadNpcDecks(RunRandomize(r).NpcDecks);

        // Both multisets preserved
        Assert.Equal(
            original.Select(d => d.SetIndex).OrderBy(x => x).ToList(),
            result.Select(d => d.SetIndex).OrderBy(x => x).ToList());
        Assert.Equal(
            original.Select(d => d.Difficulty).OrderBy(x => x).ToList(),
            result.Select(d => d.Difficulty).OrderBy(x => x).ToList());
    }

    [Fact]
    public void NpcDecks_ShuffleSetIndices_IsDeterministic()
    {
        var r1 = MakeRandomizer(42, s => { s.ShuffleNpcDecks = true; s.NpcDifficultyMode = NpcDifficultyMode.Shuffle; });
        var r2 = MakeRandomizer(42, s => { s.ShuffleNpcDecks = true; s.NpcDifficultyMode = NpcDifficultyMode.Shuffle; });
        Assert.Equal(RunRandomize(r1).NpcDecks, RunRandomize(r2).NpcDecks);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Full passthrough
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void AllFlagsDisabled_AllOutputsByteIdenticalToInput()
    {
        var inputStats = LoadCardStats();
        var inputSets = LoadCardSets();
        var inputDecks = LoadNpcDecks();

        // All flags false — MakeSettings already produces this default state
        var r = MakeRandomizer(42);
        var result = RunRandomize(r, inputStats, inputSets, inputDecks);

        Assert.Equal(inputStats, result.CardStats);
        Assert.Equal(inputSets, result.CardSets);
        Assert.Equal(inputDecks, result.NpcDecks);
        Assert.Null(result.CardNames);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Combinability
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void CardStatShuffle_AndCardTypeShuffle_ApplySimultaneously()
    {
        var original = TetraMasterFile.ReadCardStats(LoadCardStats());
        var r = MakeRandomizer(42, s => { s.RandomizeCardStats = true; s.CardTypeMode = CardTypeMode.Shuffle; });
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);

        // Stats are permuted (attack pool preserved as multiset)
        Assert.Equal(
            original.Select(c => c.Attack).OrderBy(x => x).ToList(),
            result.Select(c => c.Attack).OrderBy(x => x).ToList());

        // Types are permuted (type pool preserved as multiset)
        Assert.Equal(
            original.Select(c => c.Type).OrderBy(x => x).ToList(),
            result.Select(c => c.Type).OrderBy(x => x).ToList());
    }

    [Fact]
    public void CardStatBoundedRandom_AndArrowAllDirections_ApplySimultaneously()
    {
        const int min = 1, max = 15;
        var r = MakeRandomizer(42, s =>
        {
            s.RandomizeCardStats = true;
            s.CardStatMode = CardStatMode.BoundedRandom;
            s.CardStatMin = min;
            s.CardStatMax = max;
            s.ArrowMode = ArrowMode.AllDirections;
        });
        var result = TetraMasterFile.ReadCardStats(RunRandomize(r).CardStats);

        // Stats in bounded range
        Assert.All(result, c =>
        {
            Assert.InRange(c.Attack, min, max);
            Assert.InRange(c.Defence, min, max);
            Assert.InRange(c.MagicDefence, min, max);
        });

        // Arrows all 0xFF
        Assert.All(result, c => Assert.Equal(0xFF, c.Arrows));
    }
}