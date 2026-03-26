using StiltzkinsBag.Models;
using StiltzkinsBag.Models.Csv;

namespace StiltzkinsBag.Randomizers;

/// <summary>
/// Randomizes the four stat columns (Dexterity, Strength, Magic, Will) in Stats.csv.
/// Element columns (AttackElement–WeakElement) are never modified.
///
/// <b>Row eligibility:</b>
/// <list type="bullet">
///   <item>ID 0 (Empty) — always skipped.</item>
///   <item>Gem rows (IDs 137–148) — always skipped.</item>
///   <item>Padding rows (IDs 157+) — always skipped.</item>
///   <item>Zero-stat rows — skipped unless <see cref="Settings.ZeroStatItemsCanGainStats"/> is true.</item>
/// </list>
///
/// <b>Modes (<see cref="GearStatMode"/>):</b>
/// <list type="bullet">
///   <item><see cref="GearStatMode.Proportional"/> —
///     Each stat drawn via active weighting in [0, GearStatMax].
///     Epic roll (GearStatEpicChancePercent) can boost one stat to GearStatEpicMax.
///     All values clamped to GearStatHardCap.</item>
///   <item><see cref="GearStatMode.Shuffle"/> —
///     Fisher-Yates shuffle of all collected stat values across qualifying rows.
///     Total stat budget preserved. Ignores weighting.</item>
///   <item><see cref="GearStatMode.Chaos"/> —
///     Each stat drawn via active weighting in [0, GearStatHardCap].
///     No total sum constraint.</item>
/// </list>
///
/// <b>Weightings (<see cref="GearStatWeighting"/>):</b>
/// Geometric (p=0.5), VanillaWeighted (p=0.35), Linear, Uniform.
/// Ignored by Shuffle mode.
///
/// <b>Debug:</b>
/// <see cref="Settings.AllStatsMaxed"/> sets all stats to GearStatHardCap.
/// Requires <see cref="Settings.IsDebugMode"/> = true.
///
/// <b>RNG order:</b> Rows processed in ascending ID order. Within each row:
/// Dexterity → Strength → Magic → Will → epic roll → epic target stat.
/// </summary>
public sealed class GearStatRandomizer
{
    // -------------------------------------------------------------------------
    // Row filter constants (verified against Stats.csv)
    // -------------------------------------------------------------------------

    private const int GemPoolMin = 137;
    private const int GemPoolMax = 148;
    private const int PaddingStart = 157;

    // -------------------------------------------------------------------------
    // Fields
    // -------------------------------------------------------------------------

    private readonly Random _rng;
    private readonly Settings _settings;

    public GearStatRandomizer(Random rng, Settings settings)
    {
        _rng = rng ?? throw new ArgumentNullException(nameof(rng));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a new list of <see cref="StatsRow"/> with stat columns randomized.
    /// If <see cref="Settings.RandomizeGearStatBonuses"/> is false, returns input unchanged.
    /// Non-qualifying rows are passed through unchanged in all modes.
    /// </summary>
    public List<StatsRow> Randomize(List<StatsRow> rows)
    {
        if (rows is null) throw new ArgumentNullException(nameof(rows));
        if (rows.Count == 0) throw new ArgumentException("Stats rows must not be empty.", nameof(rows));

        if (!_settings.RandomizeGearStatBonuses)
            return rows;

        // AllStatsMaxed debug shortcut.
        if (_settings.AllStatsMaxed && _settings.IsDebugMode)
            return ApplyAllStatsMaxed(rows);

        return _settings.GearStatMode switch
        {
            GearStatMode.Proportional => ApplyProportional(rows),
            GearStatMode.Shuffle => ApplyShuffle(rows),
            GearStatMode.Chaos => ApplyChaos(rows),
            var m => throw new ArgumentOutOfRangeException(nameof(_settings.GearStatMode), m, "Unexpected GearStatMode.")
        };
    }

    // -------------------------------------------------------------------------
    // Mode implementations
    // -------------------------------------------------------------------------

    private List<StatsRow> ApplyProportional(List<StatsRow> rows)
    {
        int max = Math.Max(_settings.GearStatMax, 0);
        int epicMax = Math.Max(_settings.GearStatEpicMax, max);
        int hardCap = Math.Max(_settings.GearStatHardCap, epicMax);
        int epicPct = Math.Clamp(_settings.GearStatEpicChancePercent, 0, 100);

        var result = new List<StatsRow>(rows.Count);

        foreach (var row in rows.OrderBy(r => r.Id))
        {
            if (!IsQualifying(row))
            {
                result.Add(Clone(row));
                continue;
            }

            byte dex = DrawStat(max, hardCap);
            byte str = DrawStat(max, hardCap);
            byte mag = DrawStat(max, hardCap);
            byte wil = DrawStat(max, hardCap);

            // Epic roll — one stat gets a chance at epicMax.
            if (_rng.Next(100) < epicPct)
            {
                int target = _rng.Next(4);
                byte epicVal = DrawStat(epicMax, hardCap);
                switch (target)
                {
                    case 0: dex = epicVal; break;
                    case 1: str = epicVal; break;
                    case 2: mag = epicVal; break;
                    case 3: wil = epicVal; break;
                }
            }

            result.Add(CloneWithStats(row, dex, str, mag, wil));
        }

        return result;
    }

    private List<StatsRow> ApplyShuffle(List<StatsRow> rows)
    {
        // Collect all stat values from qualifying rows into a flat pool.
        var qualifyingIds = rows
            .OrderBy(r => r.Id)
            .Where(IsQualifying)
            .Select(r => r.Id)
            .ToHashSet();

        // Pool: [dex0, str0, mag0, wil0, dex1, str1, ...] for qualifying rows in ID order.
        var pool = rows
            .OrderBy(r => r.Id)
            .Where(r => qualifyingIds.Contains(r.Id))
            .SelectMany(r => new[] { r.Dexterity, r.Strength, r.Magic, r.Will })
            .ToList();

        // Fisher-Yates shuffle — pool.Count − 1 RNG calls.
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        // Re-assign shuffled values back in ID order.
        var result = new List<StatsRow>(rows.Count);
        int poolIdx = 0;

        foreach (var row in rows.OrderBy(r => r.Id))
        {
            if (!qualifyingIds.Contains(row.Id))
            {
                result.Add(Clone(row));
                continue;
            }

            result.Add(CloneWithStats(row,
                pool[poolIdx],
                pool[poolIdx + 1],
                pool[poolIdx + 2],
                pool[poolIdx + 3]));
            poolIdx += 4;
        }

        return result;
    }

    private List<StatsRow> ApplyChaos(List<StatsRow> rows)
    {
        int hardCap = Math.Max(_settings.GearStatHardCap, 0);
        var result = new List<StatsRow>(rows.Count);

        foreach (var row in rows.OrderBy(r => r.Id))
        {
            if (!IsQualifying(row))
            {
                result.Add(Clone(row));
                continue;
            }

            result.Add(CloneWithStats(row,
                DrawStat(hardCap, hardCap),
                DrawStat(hardCap, hardCap),
                DrawStat(hardCap, hardCap),
                DrawStat(hardCap, hardCap)));
        }

        return result;
    }

    private List<StatsRow> ApplyAllStatsMaxed(List<StatsRow> rows)
    {
        int cap = Math.Max(_settings.GearStatHardCap, 0);
        var maxVal = (byte)Math.Min(cap, byte.MaxValue);
        var result = new List<StatsRow>(rows.Count);

        foreach (var row in rows)
        {
            if (!IsQualifying(row))
            {
                result.Add(Clone(row));
                continue;
            }

            result.Add(CloneWithStats(row, maxVal, maxVal, maxVal, maxVal));
        }

        return result;
    }

    // -------------------------------------------------------------------------
    // Weighted stat drawing
    // -------------------------------------------------------------------------

    /// <summary>
    /// Draws a random stat value in [0, max] using the active
    /// <see cref="GearStatWeighting"/>, then clamps to hardCap.
    /// Consumes exactly 1 RNG call.
    /// </summary>
    private byte DrawStat(int max, int hardCap)
    {
        if (max <= 0) return 0;

        int val = _settings.GearStatWeighting switch
        {
            GearStatWeighting.Geometric => DrawGeometric(max, 0.5),
            GearStatWeighting.VanillaWeighted => DrawGeometric(max, 0.35),
            GearStatWeighting.Linear => DrawLinear(max),
            GearStatWeighting.Uniform => _rng.Next(max + 1),
            var w => throw new ArgumentOutOfRangeException(nameof(_settings.GearStatWeighting), w, "Unexpected GearStatWeighting.")
        };

        return (byte)Math.Min(Math.Min(val, hardCap), byte.MaxValue);
    }

    /// <summary>
    /// Geometric weighted draw using integer cumulative thresholds.
    /// Consumes 1 RNG call. Base p controls steepness (0.5 = halving, 0.35 = vanilla-like).
    /// </summary>
    private int DrawGeometric(int max, double p)
    {
        // Build integer weight array: weight[k] = round(scale * p^k * (1-p)) for k in [0, max-1],
        // weight[max] = remaining. Scale to at least 1000 for precision.
        const int scale = 100_000;
        var weights = new int[max + 1];
        int remaining = scale;

        for (int k = 0; k < max; k++)
        {
            weights[k] = (int)Math.Round(scale * Math.Pow(p, k) * (1 - p));
            remaining -= weights[k];
        }
        weights[max] = Math.Max(remaining, 1);

        int total = weights.Sum();
        int roll = _rng.Next(total);
        int cum = 0;

        for (int k = 0; k <= max; k++)
        {
            cum += weights[k];
            if (roll < cum) return k;
        }

        return max;
    }

    /// <summary>
    /// Linear weighted draw. Probability decreases linearly: weight[k] = (max + 1 - k).
    /// Consumes 1 RNG call.
    /// </summary>
    private int DrawLinear(int max)
    {
        // total = sum(1..max+1) = (max+1)(max+2)/2
        int total = (max + 1) * (max + 2) / 2;
        int roll = _rng.Next(total);
        int cum = 0;

        for (int k = 0; k <= max; k++)
        {
            cum += (max + 1 - k);
            if (roll < cum) return k;
        }

        return max;
    }

    // -------------------------------------------------------------------------
    // Row qualification
    // -------------------------------------------------------------------------

    private bool IsQualifying(StatsRow row)
    {
        if (row.Id == 0) return false;
        if (row.Id >= GemPoolMin && row.Id <= GemPoolMax) return false;
        if (row.Id >= PaddingStart) return false;
        if (!row.HasAnyStat && !_settings.ZeroStatItemsCanGainStats) return false;
        return true;
    }

    // -------------------------------------------------------------------------
    // Row helpers
    // -------------------------------------------------------------------------

    private static StatsRow Clone(StatsRow src) =>
        CloneWithStats(src, src.Dexterity, src.Strength, src.Magic, src.Will);

    private static StatsRow CloneWithStats(StatsRow src, byte dex, byte str, byte mag, byte wil) => new()
    {
        Comment = src.Comment,
        Id = src.Id,
        Dexterity = dex,
        Strength = str,
        Magic = mag,
        Will = wil,
        AttackElement = src.AttackElement,
        GuardElement = src.GuardElement,
        AbsorbElement = src.AbsorbElement,
        HalfElement = src.HalfElement,
        WeakElement = src.WeakElement
    };
}