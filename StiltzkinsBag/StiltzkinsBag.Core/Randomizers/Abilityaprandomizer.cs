using StiltzkinsBag.Models;
using StiltzkinsBag.Models.Csv;

namespace StiltzkinsBag.Randomizers;

/// <summary>
/// Randomizes AP costs to learn abilities via one of four modes.
///
/// Operates as a post-processor on <c>CharacterRandomizerResult.AbilityTables</c>
/// (a <c>Dictionary&lt;int, List&lt;CharacterAbilityRow&gt;&gt;</c> mapping char ID → rows).
/// Must run after <c>CharacterRandomizer</c> so it operates on final ability assignments.
///
/// <b>Scale factors are per-ability:</b> the same AbilityRef always receives the same
/// scaled AP cost on every character that has it.
///
/// <b>RNG order:</b> unique AbilityRefs are sorted alphabetically before any RNG calls.
/// This guarantees deterministic consumption regardless of dictionary iteration order.
///
/// <b>Modes:</b>
/// <list type="bullet">
///   <item><see cref="AbilityApMode.ProportionalScale"/> —
///     Each ability scaled by a random % in [MinPercent, MaxPercent].
///     One RNG call per unique AbilityRef.</item>
///   <item><see cref="AbilityApMode.Inverse"/> —
///     Costs flipped: invertedCost = vanillaMin + vanillaMax − vanillaCost.
///     0 RNG calls.</item>
///   <item><see cref="AbilityApMode.FlatCost"/> —
///     All abilities cost <see cref="Settings.ApFlatCost"/> AP.
///     0 RNG calls.</item>
///   <item><see cref="AbilityApMode.Amnesia"/> —
///     All costs = 1. Debug only. Downgrades to ProportionalScale without IsDebugMode.
///     0 RNG calls.</item>
/// </list>
///
/// All results are clamped to minimum <see cref="MinAp"/> (= 1).
/// </summary>
public sealed class AbilityApRandomizer
{
    /// <summary>Minimum legal AP cost. 0 would mean the ability is learned before the first battle.</summary>
    public const int MinAp = 1;

    private readonly Random _rng;
    private readonly Settings _settings;

    public AbilityApRandomizer(Random rng, Settings settings)
    {
        _rng = rng ?? throw new ArgumentNullException(nameof(rng));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Returns a new ability table dictionary with AP costs modified per the active
    /// <see cref="AbilityApMode"/>. If <see cref="Settings.RandomizeAbilityAp"/> is false,
    /// returns the input unchanged.
    /// </summary>
    /// <param name="abilityTables">
    /// Ability tables from <c>CharacterRandomizerResult</c>.
    /// Keys are character IDs; values are lists of <see cref="CharacterAbilityRow"/>.
    /// </param>
    public Dictionary<int, List<CharacterAbilityRow>> Randomize(
        Dictionary<int, List<CharacterAbilityRow>> abilityTables)
    {
        if (abilityTables is null)
            throw new ArgumentNullException(nameof(abilityTables));
        if (abilityTables.Count == 0)
            throw new ArgumentException("AbilityTables must not be empty.", nameof(abilityTables));

        if (!_settings.RandomizeAbilityAp)
            return abilityTables;

        // Build a per-ability AP map from vanilla costs then apply the active mode.
        var vanillaApMap = BuildVanillaApMap(abilityTables);
        var scaledApMap = BuildScaledApMap(vanillaApMap);
        return ApplyApMap(abilityTables, scaledApMap);
    }

    // -------------------------------------------------------------------------
    // Step 1 — collect vanilla AP per unique AbilityRef
    // -------------------------------------------------------------------------

    /// <summary>
    /// Collects one vanilla AP value per unique AbilityRef.
    /// When the same ref appears on multiple characters with different AP values
    /// (possible after CharacterRandomizer), the first encountered value (by char ID
    /// ascending, then row order) is used as the canonical vanilla cost.
    /// </summary>
    private static Dictionary<string, int> BuildVanillaApMap(
        Dictionary<int, List<CharacterAbilityRow>> abilityTables)
    {
        var map = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var charId in abilityTables.Keys.OrderBy(id => id))
        {
            foreach (var row in abilityTables[charId])
            {
                if (!map.ContainsKey(row.AbilityRef))
                    map[row.AbilityRef] = row.AP;
            }
        }

        return map;
    }

    // -------------------------------------------------------------------------
    // Step 2 — compute scaled AP per unique AbilityRef
    // -------------------------------------------------------------------------

    /// <summary>
    /// Produces a new per-AbilityRef AP map with costs adjusted per the active mode.
    /// AbilityRefs are processed in alphabetical order for deterministic RNG consumption.
    /// </summary>
    private Dictionary<string, int> BuildScaledApMap(Dictionary<string, int> vanillaApMap)
    {
        var mode = ResolveMode();
        var sortedRefs = vanillaApMap.Keys.OrderBy(r => r, StringComparer.Ordinal).ToList();
        var scaledMap = new Dictionary<string, int>(sortedRefs.Count, StringComparer.Ordinal);

        switch (mode)
        {
            case AbilityApMode.ProportionalScale:
                {
                    var (minPct, maxPct) = ResolvedScaleRange();
                    foreach (var abilityRef in sortedRefs)
                    {
                        int pct = _rng.Next(minPct, maxPct + 1);
                        int scaled = (int)Math.Round(vanillaApMap[abilityRef] * pct / 100.0);
                        scaledMap[abilityRef] = Math.Max(scaled, MinAp);
                    }
                    break;
                }

            case AbilityApMode.Inverse:
                {
                    int min = vanillaApMap.Values.Min();
                    int max = vanillaApMap.Values.Max();
                    foreach (var abilityRef in sortedRefs)
                        scaledMap[abilityRef] = Math.Max(min + max - vanillaApMap[abilityRef], MinAp);
                    break;
                }

            case AbilityApMode.FlatCost:
                {
                    int flat = Math.Max(_settings.ApFlatCost, MinAp);
                    foreach (var abilityRef in sortedRefs)
                        scaledMap[abilityRef] = flat;
                    break;
                }

            case AbilityApMode.Amnesia:
                {
                    foreach (var abilityRef in sortedRefs)
                        scaledMap[abilityRef] = MinAp;
                    break;
                }

            default:
                throw new ArgumentOutOfRangeException(nameof(_settings.AbilityApMode), mode, "Unexpected AbilityApMode.");
        }

        return scaledMap;
    }

    // -------------------------------------------------------------------------
    // Step 3 — apply scaled AP map to all ability tables
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds a new ability table dictionary substituting AP values from
    /// <paramref name="scaledApMap"/>. Input is never mutated.
    /// </summary>
    private static Dictionary<int, List<CharacterAbilityRow>> ApplyApMap(
        Dictionary<int, List<CharacterAbilityRow>> abilityTables,
        Dictionary<string, int> scaledApMap)
    {
        var result = new Dictionary<int, List<CharacterAbilityRow>>(abilityTables.Count);

        foreach (var (charId, rows) in abilityTables)
        {
            var newRows = new List<CharacterAbilityRow>(rows.Count);
            foreach (var row in rows)
            {
                newRows.Add(new CharacterAbilityRow
                {
                    AbilityRef = row.AbilityRef,
                    AP = scaledApMap[row.AbilityRef]
                });
            }
            result[charId] = newRows;
        }

        return result;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Resolves the effective mode. Downgrades <see cref="AbilityApMode.Amnesia"/>
    /// to <see cref="AbilityApMode.ProportionalScale"/> unless debug mode is active.
    /// </summary>
    private AbilityApMode ResolveMode()
    {
        if (_settings.AbilityApMode == AbilityApMode.Amnesia && !_settings.IsDebugMode)
            return AbilityApMode.ProportionalScale;

        return _settings.AbilityApMode;
    }

    /// <summary>
    /// Returns a validated (min%, max%) scale range for ProportionalScale mode.
    /// Min is clamped to 1. If max &lt; min, the two are swapped.
    /// </summary>
    private (int min, int max) ResolvedScaleRange()
    {
        int min = Math.Max(_settings.ApScaleMinPercent, 1);
        int max = Math.Max(_settings.ApScaleMaxPercent, 1);

        if (max < min)
            (min, max) = (max, min);

        return (min, max);
    }
}