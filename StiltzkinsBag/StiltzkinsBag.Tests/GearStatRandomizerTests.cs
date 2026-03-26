using StiltzkinsBag.Models;
using StiltzkinsBag.Models.Csv;
using StiltzkinsBag.Randomizers;
using Xunit;

namespace StiltzkinsBag.Tests;

public class GearStatRandomizerTests
{
    // -------------------------------------------------------------------------
    // Test data helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Eight rows covering all filter cases.
    ///
    /// Qualifying stat-bearing:
    ///   ID  1: Str=1                     (sum=1)
    ///   ID  2: Magic=2                   (sum=2)
    ///   ID  3: Will=3                    (sum=3)
    ///   ID  4: Dex=1, Str=1, Mag=1, Wil=1 (sum=4)
    /// Total qualifying stat sum: 10
    ///
    /// Zero-stat (element-only):
    ///   ID 10: GuardElement=64, no stats
    ///
    /// Always-skipped:
    ///   ID  0: Empty
    ///   ID140: Gem (Diamond)
    ///   ID160: Padding
    /// </summary>
    private static List<StatsRow> MakeRows() =>
    [
        new() { Id = 0,   Comment = "Empty",   Dexterity = 0, Strength = 0, Magic = 0, Will = 0, GuardElement = 0  },
        new() { Id = 1,   Comment = "Item1",   Dexterity = 0, Strength = 1, Magic = 0, Will = 0, GuardElement = 0  },
        new() { Id = 2,   Comment = "Item2",   Dexterity = 0, Strength = 0, Magic = 2, Will = 0, GuardElement = 0  },
        new() { Id = 3,   Comment = "Item3",   Dexterity = 0, Strength = 0, Magic = 0, Will = 3, GuardElement = 0  },
        new() { Id = 4,   Comment = "Item4",   Dexterity = 1, Strength = 1, Magic = 1, Will = 1, GuardElement = 0  },
        new() { Id = 10,  Comment = "Elem",    Dexterity = 0, Strength = 0, Magic = 0, Will = 0, GuardElement = 64 },
        new() { Id = 140, Comment = "Diamond", Dexterity = 0, Strength = 0, Magic = 0, Will = 0, GuardElement = 0  },
        new() { Id = 160, Comment = "Padding", Dexterity = 0, Strength = 0, Magic = 0, Will = 0, GuardElement = 0  },
    ];

    private static Settings MakeSettings(
        GearStatMode mode = GearStatMode.Proportional,
        GearStatWeighting weighting = GearStatWeighting.Geometric,
        int statMax = 3,
        int epicMax = 4,
        int epicChancePct = 10,
        int hardCap = 5,
        bool zeroStatCanGain = false,
        bool allStatsMaxed = false,
        bool debugMode = false) => new()
        {
            RandomizeGearStatBonuses = true,
            GearStatMode = mode,
            GearStatWeighting = weighting,
            GearStatMax = statMax,
            GearStatEpicMax = epicMax,
            GearStatEpicChancePercent = epicChancePct,
            GearStatHardCap = hardCap,
            ZeroStatItemsCanGainStats = zeroStatCanGain,
            AllStatsMaxed = allStatsMaxed,
            IsDebugMode = debugMode
        };

    private static Random Rng(int seed = 42) => new(seed);

    // -------------------------------------------------------------------------
    // Disabled — passthrough
    // -------------------------------------------------------------------------

    [Fact]
    public void Disabled_ReturnsInputUnchanged()
    {
        var settings = new Settings { RandomizeGearStatBonuses = false };
        var sut = new GearStatRandomizer(Rng(), settings);
        var rows = MakeRows();

        Assert.Same(rows, sut.Randomize(rows));
    }

    // -------------------------------------------------------------------------
    // Row filtering
    // -------------------------------------------------------------------------

    [Fact]
    public void IdZero_AlwaysSkipped()
    {
        var result = new GearStatRandomizer(Rng(), MakeSettings()).Randomize(MakeRows());
        var row0 = result.Single(r => r.Id == 0);

        Assert.Equal(0, row0.Dexterity + row0.Strength + row0.Magic + row0.Will);
    }

    [Fact]
    public void GemRow_AlwaysSkipped()
    {
        var result = new GearStatRandomizer(Rng(), MakeSettings()).Randomize(MakeRows());
        var gem = result.Single(r => r.Id == 140);

        Assert.Equal(0, gem.Dexterity + gem.Strength + gem.Magic + gem.Will);
    }

    [Fact]
    public void PaddingRow_AlwaysSkipped()
    {
        var result = new GearStatRandomizer(Rng(), MakeSettings()).Randomize(MakeRows());
        var padding = result.Single(r => r.Id == 160);

        Assert.Equal(0, padding.Dexterity + padding.Strength + padding.Magic + padding.Will);
    }

    [Fact]
    public void ZeroStatRow_SkippedByDefault()
    {
        var result = new GearStatRandomizer(Rng(), MakeSettings(zeroStatCanGain: false)).Randomize(MakeRows());
        var elem = result.Single(r => r.Id == 10);

        Assert.Equal(0, elem.Dexterity + elem.Strength + elem.Magic + elem.Will);
    }

    [Fact]
    public void ZeroStatRow_IncludedWhenFlagEnabled()
    {
        // Run many seeds until the element-only row gains a stat.
        bool gained = false;
        for (int seed = 0; seed < 200 && !gained; seed++)
        {
            var result = new GearStatRandomizer(Rng(seed),
                MakeSettings(zeroStatCanGain: true, weighting: GearStatWeighting.Uniform)).Randomize(MakeRows());
            var elem = result.Single(r => r.Id == 10);
            if (elem.Dexterity + elem.Strength + elem.Magic + elem.Will > 0)
                gained = true;
        }
        Assert.True(gained, "Zero-stat row should gain stats when ZeroStatItemsCanGainStats=true.");
    }

    // -------------------------------------------------------------------------
    // Invariants — all modes
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(GearStatMode.Proportional)]
    [InlineData(GearStatMode.Shuffle)]
    [InlineData(GearStatMode.Chaos)]
    public void AnyMode_ElementColumnsNeverModified(GearStatMode mode)
    {
        var vanilla = MakeRows();
        var result = new GearStatRandomizer(Rng(), MakeSettings(mode, zeroStatCanGain: true)).Randomize(MakeRows());

        foreach (var (orig, res) in vanilla.Zip(result.OrderBy(r => r.Id)))
        {
            Assert.Equal(orig.GuardElement, res.GuardElement);
            Assert.Equal(orig.AttackElement, res.AttackElement);
            Assert.Equal(orig.AbsorbElement, res.AbsorbElement);
            Assert.Equal(orig.HalfElement, res.HalfElement);
            Assert.Equal(orig.WeakElement, res.WeakElement);
        }
    }

    [Theory]
    [InlineData(GearStatMode.Proportional)]
    [InlineData(GearStatMode.Shuffle)]
    [InlineData(GearStatMode.Chaos)]
    public void AnyMode_CommentAndIdNeverModified(GearStatMode mode)
    {
        var vanilla = MakeRows();
        var result = new GearStatRandomizer(Rng(), MakeSettings(mode)).Randomize(MakeRows());

        foreach (var (orig, res) in vanilla.OrderBy(r => r.Id).Zip(result.OrderBy(r => r.Id)))
        {
            Assert.Equal(orig.Id, res.Id);
            Assert.Equal(orig.Comment, res.Comment);
        }
    }

    [Theory]
    [InlineData(GearStatMode.Proportional)]
    [InlineData(GearStatMode.Shuffle)]
    [InlineData(GearStatMode.Chaos)]
    public void AnyMode_RowCountPreserved(GearStatMode mode)
    {
        var rows = MakeRows();
        var result = new GearStatRandomizer(Rng(), MakeSettings(mode)).Randomize(rows);

        Assert.Equal(rows.Count, result.Count);
    }

    // -------------------------------------------------------------------------
    // Proportional
    // -------------------------------------------------------------------------

    [Fact]
    public void Proportional_StatsInRange()
    {
        var sut = new GearStatRandomizer(Rng(), MakeSettings(statMax: 3, hardCap: 5));
        var result = sut.Randomize(MakeRows());

        foreach (var row in result.Where(r => r.Id is >= 1 and <= 4))
        {
            Assert.InRange((int)row.Dexterity, 0, 5);
            Assert.InRange((int)row.Strength, 0, 5);
            Assert.InRange((int)row.Magic, 0, 5);
            Assert.InRange((int)row.Will, 0, 5);
        }
    }

    [Fact]
    public void Proportional_NoValueExceedsHardCap()
    {
        var sut = new GearStatRandomizer(Rng(), MakeSettings(statMax: 10, epicMax: 10, hardCap: 3));
        var result = sut.Randomize(MakeRows());

        Assert.All(result, row =>
        {
            Assert.True(row.Dexterity <= 3);
            Assert.True(row.Strength <= 3);
            Assert.True(row.Magic <= 3);
            Assert.True(row.Will <= 3);
        });
    }

    [Fact]
    public void Proportional_IsDeterministic()
    {
        var settings = MakeSettings();
        var result1 = new GearStatRandomizer(Rng(42), settings).Randomize(MakeRows());
        var result2 = new GearStatRandomizer(Rng(42), settings).Randomize(MakeRows());

        foreach (var (r1, r2) in result1.OrderBy(r => r.Id).Zip(result2.OrderBy(r => r.Id)))
        {
            Assert.Equal(r1.Dexterity, r2.Dexterity);
            Assert.Equal(r1.Strength, r2.Strength);
            Assert.Equal(r1.Magic, r2.Magic);
            Assert.Equal(r1.Will, r2.Will);
        }
    }

    [Fact]
    public void Proportional_EpicRoll_CanExceedStandardMax()
    {
        // EpicChance=100%, GearStatMax=0 (standard stats always 0),
        // EpicMax=5, Uniform weighting — epic stat should be > 0 across 200 seeds.
        bool epicSeen = false;
        var settings = MakeSettings(
            statMax: 0, epicMax: 5, epicChancePct: 100,
            hardCap: 5, weighting: GearStatWeighting.Uniform);

        for (int seed = 0; seed < 200 && !epicSeen; seed++)
        {
            var result = new GearStatRandomizer(Rng(seed), settings).Randomize(MakeRows());
            epicSeen = result.Any(r =>
                r.Dexterity > 0 || r.Strength > 0 || r.Magic > 0 || r.Will > 0);
        }
        Assert.True(epicSeen, "Epic roll with 100% chance should produce a non-zero stat.");
    }

    // -------------------------------------------------------------------------
    // Shuffle
    // -------------------------------------------------------------------------

    [Fact]
    public void Shuffle_TotalStatBudgetPreserved()
    {
        var rows = MakeRows();
        var vanilla = rows.Where(r => r.Id is >= 1 and <= 4)
                         .Sum(r => r.Dexterity + r.Strength + r.Magic + r.Will);

        var result = new GearStatRandomizer(Rng(), MakeSettings(GearStatMode.Shuffle)).Randomize(rows);
        var after = result.Where(r => r.Id is >= 1 and <= 4)
                            .Sum(r => r.Dexterity + r.Strength + r.Magic + r.Will);

        Assert.Equal(vanilla, after);
    }

    [Fact]
    public void Shuffle_IsDeterministic()
    {
        var settings = MakeSettings(GearStatMode.Shuffle);
        var result1 = new GearStatRandomizer(Rng(7), settings).Randomize(MakeRows());
        var result2 = new GearStatRandomizer(Rng(7), settings).Randomize(MakeRows());

        foreach (var (r1, r2) in result1.OrderBy(r => r.Id).Zip(result2.OrderBy(r => r.Id)))
            Assert.Equal(r1.Strength + r1.Magic + r1.Will + r1.Dexterity,
                         r2.Strength + r2.Magic + r2.Will + r2.Dexterity);
    }

    [Fact]
    public void Shuffle_NonQualifyingRowsPassThrough()
    {
        var result = new GearStatRandomizer(Rng(), MakeSettings(GearStatMode.Shuffle)).Randomize(MakeRows());

        Assert.Equal(0, result.Single(r => r.Id == 0).Dexterity);
        Assert.Equal(0, result.Single(r => r.Id == 140).Will);
        Assert.Equal(0, result.Single(r => r.Id == 160).Magic);
    }

    // -------------------------------------------------------------------------
    // Chaos
    // -------------------------------------------------------------------------

    [Fact]
    public void Chaos_AllStatsWithinHardCap()
    {
        var sut = new GearStatRandomizer(Rng(), MakeSettings(GearStatMode.Chaos, hardCap: 5));
        var result = sut.Randomize(MakeRows());

        Assert.All(result, row =>
        {
            Assert.True(row.Dexterity <= 5);
            Assert.True(row.Strength <= 5);
            Assert.True(row.Magic <= 5);
            Assert.True(row.Will <= 5);
        });
    }

    [Fact]
    public void Chaos_IsDeterministic()
    {
        var settings = MakeSettings(GearStatMode.Chaos);
        var result1 = new GearStatRandomizer(Rng(3), settings).Randomize(MakeRows());
        var result2 = new GearStatRandomizer(Rng(3), settings).Randomize(MakeRows());

        foreach (var (r1, r2) in result1.OrderBy(r => r.Id).Zip(result2.OrderBy(r => r.Id)))
        {
            Assert.Equal(r1.Strength, r2.Strength);
            Assert.Equal(r1.Magic, r2.Magic);
        }
    }

    // -------------------------------------------------------------------------
    // AllStatsMaxed (debug)
    // -------------------------------------------------------------------------

    [Fact]
    public void AllStatsMaxed_WithDebugMode_QualifyingStatsAtHardCap()
    {
        var sut = new GearStatRandomizer(Rng(), MakeSettings(hardCap: 4, allStatsMaxed: true, debugMode: true));
        var result = sut.Randomize(MakeRows());

        foreach (var row in result.Where(r => r.Id is >= 1 and <= 4))
        {
            Assert.Equal(4, row.Dexterity);
            Assert.Equal(4, row.Strength);
            Assert.Equal(4, row.Magic);
            Assert.Equal(4, row.Will);
        }
    }

    [Fact]
    public void AllStatsMaxed_WithoutDebugMode_HasNoEffect()
    {
        // AllStatsMaxed requires IsDebugMode=true — without it the flag is ignored
        // and Proportional runs normally. Not all stats should be at hardCap.
        var sut = new GearStatRandomizer(Rng(42), MakeSettings(hardCap: 5, allStatsMaxed: true, debugMode: false));
        var result = sut.Randomize(MakeRows());

        bool allMaxed = result
            .Where(r => r.Id is >= 1 and <= 4)
            .All(r => r.Dexterity == 5 && r.Strength == 5 && r.Magic == 5 && r.Will == 5);

        Assert.False(allMaxed, "AllStatsMaxed without debug mode should not produce all-maxed stats.");
    }

    // -------------------------------------------------------------------------
    // Weighting — statistical
    // -------------------------------------------------------------------------

    [Fact]
    public void Weighting_Uniform_HigherAverageThanVanillaWeighted()
    {
        double MeanStat(GearStatWeighting w)
        {
            var settings = MakeSettings(GearStatMode.Chaos, weighting: w, hardCap: 5);
            var total = 0.0;
            var count = 0;
            for (int seed = 0; seed < 500; seed++)
            {
                var result = new GearStatRandomizer(Rng(seed), settings).Randomize(MakeRows());
                foreach (var row in result.Where(r => r.Id is >= 1 and <= 4))
                {
                    total += row.Dexterity + row.Strength + row.Magic + row.Will;
                    count += 4;
                }
            }
            return total / count;
        }

        double uniformMean = MeanStat(GearStatWeighting.Uniform);
        double vanillaMean = MeanStat(GearStatWeighting.VanillaWeighted);

        Assert.True(uniformMean > vanillaMean,
            $"Uniform mean ({uniformMean:F2}) should exceed VanillaWeighted mean ({vanillaMean:F2}).");
    }

    [Fact]
    public void Weighting_Geometric_HigherAverageThanVanillaWeighted()
    {
        double MeanStat(GearStatWeighting w)
        {
            var settings = MakeSettings(GearStatMode.Chaos, weighting: w, hardCap: 5);
            var total = 0.0;
            var count = 0;
            for (int seed = 0; seed < 500; seed++)
            {
                var result = new GearStatRandomizer(Rng(seed), settings).Randomize(MakeRows());
                foreach (var row in result.Where(r => r.Id is >= 1 and <= 4))
                {
                    total += row.Dexterity + row.Strength + row.Magic + row.Will;
                    count += 4;
                }
            }
            return total / count;
        }

        double geometricMean = MeanStat(GearStatWeighting.Geometric);
        double vanillaMean = MeanStat(GearStatWeighting.VanillaWeighted);

        Assert.True(geometricMean > vanillaMean,
            $"Geometric mean ({geometricMean:F2}) should exceed VanillaWeighted mean ({vanillaMean:F2}).");
    }

    // -------------------------------------------------------------------------
    // Guard clauses
    // -------------------------------------------------------------------------

    [Fact]
    public void Randomize_NullRows_Throws()
    {
        var sut = new GearStatRandomizer(Rng(), MakeSettings());
        Assert.Throws<ArgumentNullException>(() => sut.Randomize(null!));
    }

    [Fact]
    public void Randomize_EmptyRows_Throws()
    {
        var sut = new GearStatRandomizer(Rng(), MakeSettings());
        Assert.Throws<ArgumentException>(() => sut.Randomize([]));
    }
}