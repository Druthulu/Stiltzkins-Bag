using StiltzkinsBag.Models;
using StiltzkinsBag.Models.Csv;
using StiltzkinsBag.Randomizers;
using Xunit;

namespace StiltzkinsBag.Tests;

public class AbilityApRandomizerTests
{
    // -------------------------------------------------------------------------
    // Test data
    // -------------------------------------------------------------------------

    /// <summary>
    /// Three characters sharing some AbilityRefs to verify per-ability consistency.
    ///
    /// Unique refs and vanilla AP:
    ///   SA:1 = 20  (chars 0 and 1)
    ///   SA:2 = 40  (char 0 only)
    ///   SA:3 = 60  (char 1 only)
    ///   AA:1 = 15  (chars 0 and 2)
    ///   AA:2 = 10  (chars 1 and 2)
    ///
    /// Vanilla AP range: min=10, max=60.
    /// Inverse values: SA:1→50, SA:2→30, SA:3→10, AA:1→55, AA:2→60.
    /// </summary>
    private static Dictionary<int, List<CharacterAbilityRow>> MakeTable() => new()
    {
        [0] = [ new() { AbilityRef = "SA:1", AP = 20 },
                new() { AbilityRef = "SA:2", AP = 40 },
                new() { AbilityRef = "AA:1", AP = 15 } ],
        [1] = [ new() { AbilityRef = "SA:1", AP = 20 },
                new() { AbilityRef = "SA:3", AP = 60 },
                new() { AbilityRef = "AA:2", AP = 10 } ],
        [2] = [ new() { AbilityRef = "AA:1", AP = 15 },
                new() { AbilityRef = "AA:2", AP = 10 } ],
    };

    private static Settings MakeSettings(
        AbilityApMode mode = AbilityApMode.ProportionalScale,
        int minPct = 50,
        int maxPct = 200,
        int flatCost = 20,
        bool debugMode = false) => new()
        {
            RandomizeAbilityAp = true,
            AbilityApMode = mode,
            ApScaleMinPercent = minPct,
            ApScaleMaxPercent = maxPct,
            ApFlatCost = flatCost,
            IsDebugMode = debugMode
        };

    private static Random Rng(int seed = 42) => new(seed);

    // -------------------------------------------------------------------------
    // Disabled — passthrough
    // -------------------------------------------------------------------------

    [Fact]
    public void Disabled_ReturnsInputUnchanged()
    {
        var settings = new Settings { RandomizeAbilityAp = false };
        var sut = new AbilityApRandomizer(Rng(), settings);
        var table = MakeTable();

        var result = sut.Randomize(table);

        Assert.Same(table, result);
    }

    // -------------------------------------------------------------------------
    // All modes — shared invariants
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(AbilityApMode.ProportionalScale)]
    [InlineData(AbilityApMode.Inverse)]
    [InlineData(AbilityApMode.FlatCost)]
    public void AnyMode_AbilityRefNeverModified(AbilityApMode mode)
    {
        var vanilla = MakeTable();
        var sut = new AbilityApRandomizer(Rng(), MakeSettings(mode));
        var result = sut.Randomize(MakeTable());

        foreach (var charId in vanilla.Keys)
        {
            var originalRefs = vanilla[charId].Select(r => r.AbilityRef).ToList();
            var resultRefs = result[charId].Select(r => r.AbilityRef).ToList();
            Assert.Equal(originalRefs, resultRefs);
        }
    }

    [Theory]
    [InlineData(AbilityApMode.ProportionalScale)]
    [InlineData(AbilityApMode.Inverse)]
    [InlineData(AbilityApMode.FlatCost)]
    public void AnyMode_InputNotMutated(AbilityApMode mode)
    {
        var table = MakeTable();
        var sut = new AbilityApRandomizer(Rng(), MakeSettings(mode));
        sut.Randomize(table);

        // Original table rows must be unchanged.
        Assert.Equal(20, table[0].First(r => r.AbilityRef == "SA:1").AP);
        Assert.Equal(40, table[0].First(r => r.AbilityRef == "SA:2").AP);
        Assert.Equal(60, table[1].First(r => r.AbilityRef == "SA:3").AP);
    }

    [Theory]
    [InlineData(AbilityApMode.ProportionalScale)]
    [InlineData(AbilityApMode.Inverse)]
    [InlineData(AbilityApMode.FlatCost)]
    public void AnyMode_AllCostsAtLeastOne(AbilityApMode mode)
    {
        var sut = new AbilityApRandomizer(Rng(), MakeSettings(mode));
        var result = sut.Randomize(MakeTable());

        Assert.All(result.Values.SelectMany(rows => rows),
            row => Assert.True(row.AP >= AbilityApRandomizer.MinAp,
                $"{row.AbilityRef} AP={row.AP} is below MinAp"));
    }

    [Theory]
    [InlineData(AbilityApMode.ProportionalScale)]
    [InlineData(AbilityApMode.Inverse)]
    [InlineData(AbilityApMode.FlatCost)]
    public void AnyMode_PerAbility_SameRefSameCostAcrossChars(AbilityApMode mode)
    {
        var sut = new AbilityApRandomizer(Rng(), MakeSettings(mode));
        var result = sut.Randomize(MakeTable());

        // SA:1 appears on chars 0 and 1.
        int sa1Char0 = result[0].First(r => r.AbilityRef == "SA:1").AP;
        int sa1Char1 = result[1].First(r => r.AbilityRef == "SA:1").AP;
        Assert.Equal(sa1Char0, sa1Char1);

        // AA:1 appears on chars 0 and 2.
        int aa1Char0 = result[0].First(r => r.AbilityRef == "AA:1").AP;
        int aa1Char2 = result[2].First(r => r.AbilityRef == "AA:1").AP;
        Assert.Equal(aa1Char0, aa1Char2);

        // AA:2 appears on chars 1 and 2.
        int aa2Char1 = result[1].First(r => r.AbilityRef == "AA:2").AP;
        int aa2Char2 = result[2].First(r => r.AbilityRef == "AA:2").AP;
        Assert.Equal(aa2Char1, aa2Char2);
    }

    // -------------------------------------------------------------------------
    // ProportionalScale
    // -------------------------------------------------------------------------

    [Fact]
    public void ProportionalScale_IsDeterministic()
    {
        var settings = MakeSettings(AbilityApMode.ProportionalScale);
        var result1 = new AbilityApRandomizer(Rng(42), settings).Randomize(MakeTable());
        var result2 = new AbilityApRandomizer(Rng(42), settings).Randomize(MakeTable());

        foreach (var charId in result1.Keys)
            Assert.Equal(result1[charId].Select(r => r.AP), result2[charId].Select(r => r.AP));
    }

    [Fact]
    public void ProportionalScale_DifferentSeedsDifferentOutput()
    {
        var settings = MakeSettings(AbilityApMode.ProportionalScale);
        var result1 = new AbilityApRandomizer(Rng(1), settings).Randomize(MakeTable());
        var result2 = new AbilityApRandomizer(Rng(99), settings).Randomize(MakeTable());

        var costs1 = result1.Values.SelectMany(r => r).Select(r => r.AP);
        var costs2 = result2.Values.SelectMany(r => r).Select(r => r.AP);
        Assert.NotEqual(costs1, costs2);
    }

    [Fact]
    public void ProportionalScale_MinClampedToOne()
    {
        // Scale range [0, 0] — all costs must still be >= 1.
        var sut = new AbilityApRandomizer(Rng(), MakeSettings(AbilityApMode.ProportionalScale, minPct: 0, maxPct: 0));
        var result = sut.Randomize(MakeTable());

        Assert.All(result.Values.SelectMany(r => r),
            row => Assert.True(row.AP >= AbilityApRandomizer.MinAp));
    }

    [Fact]
    public void ProportionalScale_SwappedRangeHandledGracefully()
    {
        // maxPct < minPct — should swap silently and still produce valid costs.
        var sut = new AbilityApRandomizer(Rng(), MakeSettings(AbilityApMode.ProportionalScale, minPct: 200, maxPct: 50));
        var result = sut.Randomize(MakeTable());

        Assert.All(result.Values.SelectMany(r => r),
            row => Assert.True(row.AP >= AbilityApRandomizer.MinAp));
    }

    // -------------------------------------------------------------------------
    // Inverse
    // -------------------------------------------------------------------------

    [Fact]
    public void Inverse_CostsAreCorrectlyFlipped()
    {
        // Vanilla: SA:1=20, SA:2=40, SA:3=60, AA:1=15, AA:2=10. Min=10, Max=60.
        // Inverse: SA:1=50, SA:2=30, SA:3=10, AA:1=55, AA:2=60.
        var sut = new AbilityApRandomizer(Rng(), MakeSettings(AbilityApMode.Inverse));
        var result = sut.Randomize(MakeTable());

        Assert.Equal(50, result[0].First(r => r.AbilityRef == "SA:1").AP);
        Assert.Equal(30, result[0].First(r => r.AbilityRef == "SA:2").AP);
        Assert.Equal(10, result[1].First(r => r.AbilityRef == "SA:3").AP);
        Assert.Equal(55, result[0].First(r => r.AbilityRef == "AA:1").AP);
        Assert.Equal(60, result[1].First(r => r.AbilityRef == "AA:2").AP);
    }

    [Fact]
    public void Inverse_OutputIdenticalAcrossSeeds()
    {
        // Inverse is deterministic — seed should have no effect.
        var settings = MakeSettings(AbilityApMode.Inverse);
        var result1 = new AbilityApRandomizer(Rng(1), settings).Randomize(MakeTable());
        var result2 = new AbilityApRandomizer(Rng(9999), settings).Randomize(MakeTable());

        foreach (var charId in result1.Keys)
            Assert.Equal(result1[charId].Select(r => r.AP), result2[charId].Select(r => r.AP));
    }

    // -------------------------------------------------------------------------
    // FlatCost
    // -------------------------------------------------------------------------

    [Fact]
    public void FlatCost_AllCostsEqualFlatValue()
    {
        var sut = new AbilityApRandomizer(Rng(), MakeSettings(AbilityApMode.FlatCost, flatCost: 30));
        var result = sut.Randomize(MakeTable());

        Assert.All(result.Values.SelectMany(r => r), row => Assert.Equal(30, row.AP));
    }

    [Fact]
    public void FlatCost_ValueClampedToOne()
    {
        // ApFlatCost = 0 — must clamp to MinAp = 1.
        var sut = new AbilityApRandomizer(Rng(), MakeSettings(AbilityApMode.FlatCost, flatCost: 0));
        var result = sut.Randomize(MakeTable());

        Assert.All(result.Values.SelectMany(r => r),
            row => Assert.Equal(AbilityApRandomizer.MinAp, row.AP));
    }

    [Fact]
    public void FlatCost_OutputIdenticalAcrossSeeds()
    {
        var settings = MakeSettings(AbilityApMode.FlatCost, flatCost: 20);
        var result1 = new AbilityApRandomizer(Rng(1), settings).Randomize(MakeTable());
        var result2 = new AbilityApRandomizer(Rng(9999), settings).Randomize(MakeTable());

        foreach (var charId in result1.Keys)
            Assert.Equal(result1[charId].Select(r => r.AP), result2[charId].Select(r => r.AP));
    }

    // -------------------------------------------------------------------------
    // Amnesia (debug mode)
    // -------------------------------------------------------------------------

    [Fact]
    public void Amnesia_WithDebugMode_AllCostsAreOne()
    {
        var sut = new AbilityApRandomizer(Rng(), MakeSettings(AbilityApMode.Amnesia, debugMode: true));
        var result = sut.Randomize(MakeTable());

        Assert.All(result.Values.SelectMany(r => r),
            row => Assert.Equal(AbilityApRandomizer.MinAp, row.AP));
    }

    [Fact]
    public void Amnesia_WithoutDebugMode_DowngradesToProportionalScale()
    {
        // Without debug mode Amnesia → ProportionalScale.
        // Costs must NOT all be 1 (ProportionalScale with default 50–200% range on AP ≥ 10
        // will never produce all-1 results).
        var sut = new AbilityApRandomizer(Rng(), MakeSettings(AbilityApMode.Amnesia, debugMode: false));
        var result = sut.Randomize(MakeTable());

        var allOne = result.Values.SelectMany(r => r).All(r => r.AP == 1);
        Assert.False(allOne, "Amnesia without debug mode should not produce all-1 AP costs.");
    }

    // -------------------------------------------------------------------------
    // Guard clauses
    // -------------------------------------------------------------------------

    [Fact]
    public void Randomize_NullInput_Throws()
    {
        var sut = new AbilityApRandomizer(Rng(), MakeSettings());
        Assert.Throws<ArgumentNullException>(() => sut.Randomize(null!));
    }

    [Fact]
    public void Randomize_EmptyInput_Throws()
    {
        var sut = new AbilityApRandomizer(Rng(), MakeSettings());
        Assert.Throws<ArgumentException>(() => sut.Randomize([]));
    }
}