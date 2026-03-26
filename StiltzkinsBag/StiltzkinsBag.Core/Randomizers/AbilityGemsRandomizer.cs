using StiltzkinsBag.Models;
using StiltzkinsBag.Models.Csv;

namespace StiltzkinsBag.Randomizers;

/// <summary>
/// Randomizes gem equip costs in AbilityGems.csv.
///
/// Only the <c>Gems</c> column is modified. <c>Id</c>, <c>Comment</c>, and
/// <c>BoostedVersions</c> are never touched.
///
/// Modes (controlled by <see cref="Settings.AbilityGemMode"/>):
/// <list type="bullet">
///   <item><see cref="AbilityGemMode.Shuffle"/> —
///     Fisher-Yates shuffle of all vanilla costs across abilities.
///     Total gem budget preserved. 63 RNG calls.</item>
///   <item><see cref="AbilityGemMode.BoundedRandom"/> —
///     Each ability gets an independent random cost in
///     [<see cref="Settings.AbilityGemMinCost"/>, <see cref="Settings.AbilityGemMaxCost"/>].
///     64 RNG calls.</item>
///   <item><see cref="AbilityGemMode.AllCheap"/> —
///     All costs set to 1. Debug only (<see cref="Settings.IsDebugMode"/> required).
///     Downgrades to Shuffle otherwise. 0 RNG calls.</item>
/// </list>
///
/// Range validation for BoundedRandom:
/// <c>MinCost</c> is clamped to minimum 1. If <c>MaxCost</c> &lt; <c>MinCost</c>
/// the two values are swapped silently.
/// </summary>
public sealed class AbilityGemsRandomizer
{
    /// <summary>Minimum legal gem equip cost. Values below this break the game.</summary>
    public const int HardMinCost = 1;

    private readonly Random _rng;
    private readonly Settings _settings;

    public AbilityGemsRandomizer(Random rng, Settings settings)
    {
        _rng = rng ?? throw new ArgumentNullException(nameof(rng));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Returns a new list of <see cref="AbilityGemsRow"/> with randomized <c>Gems</c> values.
    /// If <see cref="Settings.RandomizeAbilityGems"/> is false, returns the input unchanged.
    /// </summary>
    /// <param name="rows">Rows from AbilityGems.csv. Must not be empty.</param>
    public List<AbilityGemsRow> Randomize(List<AbilityGemsRow> rows)
    {
        if (rows is null) throw new ArgumentNullException(nameof(rows));
        if (rows.Count == 0) throw new ArgumentException("AbilityGems rows must not be empty.", nameof(rows));

        if (!_settings.RandomizeAbilityGems)
            return rows;

        return ResolveMode() switch
        {
            AbilityGemMode.Shuffle => ApplyShuffle(rows),
            AbilityGemMode.BoundedRandom => ApplyBoundedRandom(rows),
            AbilityGemMode.AllCheap => ApplyAllCheap(rows),
            var m => throw new ArgumentOutOfRangeException(nameof(_settings.AbilityGemMode), m, "Unexpected AbilityGemMode.")
        };
    }

    // -------------------------------------------------------------------------
    // Mode implementations
    // -------------------------------------------------------------------------

    /// <summary>
    /// Shuffle: Fisher-Yates shuffle of all vanilla Gems values.
    /// Preserves total cost budget. 63 RNG calls for 64 rows.
    /// </summary>
    private List<AbilityGemsRow> ApplyShuffle(List<AbilityGemsRow> rows)
    {
        // Copy vanilla costs into a mutable list and shuffle in-place.
        var costs = rows.Select(r => r.Gems).ToList();
        for (int i = costs.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (costs[i], costs[j]) = (costs[j], costs[i]);
        }

        return BuildResult(rows, costs);
    }

    /// <summary>
    /// BoundedRandom: each ability gets an independent random cost in [min, max].
    /// 64 RNG calls (one per row).
    /// </summary>
    private List<AbilityGemsRow> ApplyBoundedRandom(List<AbilityGemsRow> rows)
    {
        var (min, max) = ResolvedCostRange();

        var costs = new List<int>(rows.Count);
        for (int i = 0; i < rows.Count; i++)
            costs.Add(_rng.Next(min, max + 1));

        return BuildResult(rows, costs);
    }

    /// <summary>
    /// AllCheap: all costs set to 1. Debug mode only. 0 RNG calls.
    /// </summary>
    private static List<AbilityGemsRow> ApplyAllCheap(List<AbilityGemsRow> rows)
    {
        var costs = Enumerable.Repeat(HardMinCost, rows.Count).ToList();
        return BuildResult(rows, costs);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Resolves effective mode. Downgrades <see cref="AbilityGemMode.AllCheap"/>
    /// to <see cref="AbilityGemMode.Shuffle"/> unless debug mode is active.
    /// </summary>
    private AbilityGemMode ResolveMode()
    {
        if (_settings.AbilityGemMode == AbilityGemMode.AllCheap && !_settings.IsDebugMode)
            return AbilityGemMode.Shuffle;

        return _settings.AbilityGemMode;
    }

    /// <summary>
    /// Returns a validated (min, max) cost range for BoundedRandom mode.
    /// Min is clamped to <see cref="HardMinCost"/>. If max &lt; min, the two are swapped.
    /// </summary>
    private (int min, int max) ResolvedCostRange()
    {
        int min = Math.Max(_settings.AbilityGemMinCost, HardMinCost);
        int max = Math.Max(_settings.AbilityGemMaxCost, HardMinCost);

        if (max < min)
            (min, max) = (max, min);

        return (min, max);
    }

    /// <summary>
    /// Builds result rows by copying all fields from <paramref name="source"/> and
    /// substituting <c>Gems</c> values from <paramref name="newCosts"/>.
    /// </summary>
    private static List<AbilityGemsRow> BuildResult(List<AbilityGemsRow> source, List<int> newCosts)
    {
        var result = new List<AbilityGemsRow>(source.Count);
        for (int i = 0; i < source.Count; i++)
        {
            result.Add(new AbilityGemsRow
            {
                Comment = source[i].Comment,
                Id = source[i].Id,
                Gems = newCosts[i],
                BoostedVersions = source[i].BoostedVersions
            });
        }
        return result;
    }
}