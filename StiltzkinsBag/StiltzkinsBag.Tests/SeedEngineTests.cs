using StiltzkinsBag.Randomizers;

namespace StiltzkinsBag.Tests;

public class SeedEngineTests
{
    // -------------------------------------------------------------------------
    // Resolve — string → int
    // -------------------------------------------------------------------------

    [Fact]
    public void Resolve_SameSeedString_AlwaysReturnsSameInt()
    {
        int first = SeedEngine.Resolve("42069");
        int second = SeedEngine.Resolve("42069");
        Assert.Equal(first, second);
    }

    [Fact]
    public void Resolve_DifferentSeedStrings_ReturnDifferentInts()
    {
        int a = SeedEngine.Resolve("42069");
        int b = SeedEngine.Resolve("99999");
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Resolve_AlwaysReturnsNonNegative()
    {
        string[] seeds = { "0", "abc", "!@#$", "hello world", "-1", "2147483647" };
        foreach (string seed in seeds)
            Assert.True(SeedEngine.Resolve(seed) >= 0, $"Seed '{seed}' produced a negative int.");
    }

    [Fact]
    public void Resolve_TrimsWhitespace_ProducesSameIntAsUnpadded()
    {
        int trimmed = SeedEngine.Resolve("42069");
        int padded = SeedEngine.Resolve("  42069  ");
        Assert.Equal(trimmed, padded);
    }

    [Fact]
    public void Resolve_EmptyOrWhitespace_ReturnsZero()
    {
        Assert.Equal(0, SeedEngine.Resolve(""));
        Assert.Equal(0, SeedEngine.Resolve("   "));
    }

    // -------------------------------------------------------------------------
    // CreateRandom — same seed int → same RNG sequence
    // -------------------------------------------------------------------------

    [Fact]
    public void CreateRandom_SameSeedInt_ProducesSameSequence()
    {
        int seedInt = SeedEngine.Resolve("42069");

        Random r1 = SeedEngine.CreateRandom(seedInt);
        Random r2 = SeedEngine.CreateRandom(seedInt);

        // Draw 20 values from each — they must be identical
        for (int i = 0; i < 20; i++)
            Assert.Equal(r1.Next(), r2.Next());
    }

    [Fact]
    public void CreateRandom_FromString_MatchesCreateRandom_FromResolvedInt()
    {
        string seed = "stiltzkin";
        int seedInt = SeedEngine.Resolve(seed);

        Random fromString = SeedEngine.CreateRandom(seed);
        Random fromInt = SeedEngine.CreateRandom(seedInt);

        for (int i = 0; i < 20; i++)
            Assert.Equal(fromString.Next(), fromInt.Next());
    }

    [Fact]
    public void CreateRandom_DifferentSeeds_ProduceDifferentSequences()
    {
        Random r1 = SeedEngine.CreateRandom("42069");
        Random r2 = SeedEngine.CreateRandom("99999");

        // At least one value in 20 draws must differ
        bool anyDifference = false;
        for (int i = 0; i < 20; i++)
        {
            if (r1.Next() != r2.Next())
            {
                anyDifference = true;
                break;
            }
        }
        Assert.True(anyDifference);
    }
}