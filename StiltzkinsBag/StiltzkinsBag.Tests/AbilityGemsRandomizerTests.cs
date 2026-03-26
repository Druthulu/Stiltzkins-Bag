using StiltzkinsBag.Models;
using StiltzkinsBag.Models.Csv;
using StiltzkinsBag.Randomizers;
using Xunit;

namespace StiltzkinsBag.Tests;

public class AbilityGemsRandomizerTests
{
    // -------------------------------------------------------------------------
    // Test data helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Representative rows drawn from vanilla AbilityGems.csv.
    /// Costs: 15, 6, 9, 10, 12, 4, 8, 1 — varied enough to make shuffle observable.
    /// </summary>
    private static List<AbilityGemsRow> VanillaRows() =>
    [
        new() { Comment = "Auto-Reflect",  Id = 0,  Gems = 15, BoostedVersions = [] },
        new() { Comment = "Auto-Float",    Id = 1,  Gems = 6,  BoostedVersions = [] },
        new() { Comment = "Auto-Haste",    Id = 2,  Gems = 9,  BoostedVersions = [] },
        new() { Comment = "Auto-Regen",    Id = 3,  Gems = 10, BoostedVersions = [] },
        new() { Comment = "Auto-Life",     Id = 4,  Gems = 12, BoostedVersions = [] },
        new() { Comment = "HP+10%",        Id = 5,  Gems = 4,  BoostedVersions = [] },
        new() { Comment = "HP+20%",        Id = 6,  Gems = 8,  BoostedVersions = [] },
        new() { Comment = "Gamble Defence",Id = 26, Gems = 1,  BoostedVersions = [] },
    ];

    private static Settings MakeSettings(
        AbilityGemMode mode = AbilityGemMode.Shuffle,
        int minCost = 1,
        int maxCost = 20,
        bool debugMode = false) => new()
        {
            RandomizeAbilityGems = true,
            AbilityGemMode = mode,
            AbilityGemMinCost = minCost,
            AbilityGemMaxCost = maxCost,
            IsDebugMode = debugMode
        };

    private static Random Rng(int seed = 42) => new(seed);

    // -------------------------------------------------------------------------
    // Disabled — passthrough
    // -------------------------------------------------------------------------

    [Fact]
    public void Disabled_ReturnsInputUnchanged()
    {
        var settings = new Settings { RandomizeAbilityGems = false };
        var sut = new AbilityGemsRandomizer(Rng(), settings);
        var rows = VanillaRows();

        var result = sut.Randomize(rows);

        Assert.Same(rows, result);
    }

    // -------------------------------------------------------------------------
    // All modes — shared invariants
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(AbilityGemMode.Shuffle)]
    [InlineData(AbilityGemMode.BoundedRandom)]
    public void AnyMode_RowCountPreserved(AbilityGemMode mode)
    {
        var sut = new AbilityGemsRandomizer(Rng(), MakeSettings(mode));
        var result = sut.Randomize(VanillaRows());

        Assert.Equal(VanillaRows().Count, result.Count);
    }

    [Theory]
    [InlineData(AbilityGemMode.Shuffle)]
    [InlineData(AbilityGemMode.BoundedRandom)]
    public void AnyMode_IdNeverModified(AbilityGemMode mode)
    {
        var vanilla = VanillaRows();
        var sut = new AbilityGemsRandomizer(Rng(), MakeSettings(mode));
        var result = sut.Randomize(vanilla);

        for (int i = 0; i < vanilla.Count; i++)
            Assert.Equal(vanilla[i].Id, result[i].Id);
    }

    [Theory]
    [InlineData(AbilityGemMode.Shuffle)]
    [InlineData(AbilityGemMode.BoundedRandom)]
    public void AnyMode_CommentNeverModified(AbilityGemMode mode)
    {
        var vanilla = VanillaRows();
        var sut = new AbilityGemsRandomizer(Rng(), MakeSettings(mode));
        var result = sut.Randomize(vanilla);

        for (int i = 0; i < vanilla.Count; i++)
            Assert.Equal(vanilla[i].Comment, result[i].Comment);
    }

    [Theory]
    [InlineData(AbilityGemMode.Shuffle)]
    [InlineData(AbilityGemMode.BoundedRandom)]
    public void AnyMode_BoostedVersionsNeverModified(AbilityGemMode mode)
    {
        var vanilla = VanillaRows();
        var sut = new AbilityGemsRandomizer(Rng(), MakeSettings(mode));
        var result = sut.Randomize(vanilla);

        for (int i = 0; i < vanilla.Count; i++)
            Assert.Equal(vanilla[i].BoostedVersions, result[i].BoostedVersions);
    }

    // -------------------------------------------------------------------------
    // Shuffle
    // -------------------------------------------------------------------------

    [Fact]
    public void Shuffle_CostsArePermutationOfVanilla()
    {
        var vanilla = VanillaRows();
        var sut = new AbilityGemsRandomizer(Rng(), MakeSettings(AbilityGemMode.Shuffle));
        var result = sut.Randomize(vanilla);

        var originalCosts = vanilla.Select(r => r.Gems).OrderBy(x => x).ToList();
        var resultCosts = result.Select(r => r.Gems).OrderBy(x => x).ToList();

        Assert.Equal(originalCosts, resultCosts);
    }

    [Fact]
    public void Shuffle_TotalCostBudgetPreserved()
    {
        var vanilla = VanillaRows();
        var sut = new AbilityGemsRandomizer(Rng(), MakeSettings(AbilityGemMode.Shuffle));
        var result = sut.Randomize(vanilla);

        Assert.Equal(vanilla.Sum(r => r.Gems), result.Sum(r => r.Gems));
    }

    [Fact]
    public void Shuffle_IsDeterministic()
    {
        var settings = MakeSettings(AbilityGemMode.Shuffle);
        var result1 = new AbilityGemsRandomizer(Rng(42), settings).Randomize(VanillaRows());
        var result2 = new AbilityGemsRandomizer(Rng(42), settings).Randomize(VanillaRows());

        Assert.Equal(result1.Select(r => r.Gems), result2.Select(r => r.Gems));
    }

    [Fact]
    public void Shuffle_DifferentSeedsDifferentOutput()
    {
        var settings = MakeSettings(AbilityGemMode.Shuffle);
        var result1 = new AbilityGemsRandomizer(Rng(1), settings).Randomize(VanillaRows());
        var result2 = new AbilityGemsRandomizer(Rng(99), settings).Randomize(VanillaRows());

        Assert.NotEqual(result1.Select(r => r.Gems), result2.Select(r => r.Gems));
    }

    // -------------------------------------------------------------------------
    // BoundedRandom
    // -------------------------------------------------------------------------

    [Fact]
    public void BoundedRandom_AllCostsWithinRange()
    {
        var sut = new AbilityGemsRandomizer(Rng(), MakeSettings(AbilityGemMode.BoundedRandom, minCost: 3, maxCost: 12));
        var result = sut.Randomize(VanillaRows());

        Assert.All(result, row => Assert.InRange(row.Gems, 3, 12));
    }

    [Fact]
    public void BoundedRandom_IsDeterministic()
    {
        var settings = MakeSettings(AbilityGemMode.BoundedRandom, minCost: 1, maxCost: 20);
        var result1 = new AbilityGemsRandomizer(Rng(7), settings).Randomize(VanillaRows());
        var result2 = new AbilityGemsRandomizer(Rng(7), settings).Randomize(VanillaRows());

        Assert.Equal(result1.Select(r => r.Gems), result2.Select(r => r.Gems));
    }

    [Fact]
    public void BoundedRandom_MinCostClampedToOne()
    {
        // MinCost = 0 (below legal minimum) — all results must still be >= 1.
        var sut = new AbilityGemsRandomizer(Rng(), MakeSettings(AbilityGemMode.BoundedRandom, minCost: 0, maxCost: 5));
        var result = sut.Randomize(VanillaRows());

        Assert.All(result, row => Assert.True(row.Gems >= AbilityGemsRandomizer.HardMinCost,
            $"Gems = {row.Gems} is below HardMinCost = {AbilityGemsRandomizer.HardMinCost}"));
    }

    [Fact]
    public void BoundedRandom_SwappedRangeHandledGracefully()
    {
        // MaxCost < MinCost — values are swapped; all results still within [3, 10].
        var sut = new AbilityGemsRandomizer(Rng(), MakeSettings(AbilityGemMode.BoundedRandom, minCost: 10, maxCost: 3));
        var result = sut.Randomize(VanillaRows());

        Assert.All(result, row => Assert.InRange(row.Gems, 3, 10));
    }

    // -------------------------------------------------------------------------
    // AllCheap (debug mode)
    // -------------------------------------------------------------------------

    [Fact]
    public void AllCheap_WithDebugMode_AllCostsAreOne()
    {
        var sut = new AbilityGemsRandomizer(Rng(), MakeSettings(AbilityGemMode.AllCheap, debugMode: true));
        var result = sut.Randomize(VanillaRows());

        Assert.All(result, row => Assert.Equal(1, row.Gems));
    }

    [Fact]
    public void AllCheap_WithDebugMode_RowCountPreserved()
    {
        var sut = new AbilityGemsRandomizer(Rng(), MakeSettings(AbilityGemMode.AllCheap, debugMode: true));
        var result = sut.Randomize(VanillaRows());

        Assert.Equal(VanillaRows().Count, result.Count);
    }

    [Fact]
    public void AllCheap_WithoutDebugMode_DowngradesToShuffle()
    {
        // Without debug mode, AllCheap → Shuffle. Costs must be a permutation of vanilla.
        var sut = new AbilityGemsRandomizer(Rng(), MakeSettings(AbilityGemMode.AllCheap, debugMode: false));
        var result = sut.Randomize(VanillaRows());

        var originalCosts = VanillaRows().Select(r => r.Gems).OrderBy(x => x).ToList();
        var resultCosts = result.Select(r => r.Gems).OrderBy(x => x).ToList();

        Assert.Equal(originalCosts, resultCosts);
    }

    // -------------------------------------------------------------------------
    // Guard clauses
    // -------------------------------------------------------------------------

    [Fact]
    public void Randomize_NullRows_Throws()
    {
        var sut = new AbilityGemsRandomizer(Rng(), MakeSettings());
        Assert.Throws<ArgumentNullException>(() => sut.Randomize(null!));
    }

    [Fact]
    public void Randomize_EmptyRows_Throws()
    {
        var sut = new AbilityGemsRandomizer(Rng(), MakeSettings());
        Assert.Throws<ArgumentException>(() => sut.Randomize([]));
    }
}