namespace StiltzkinsBag.Models;

/// <summary>
/// Maps original item IDs to their randomized replacements.
/// Produced by ItemRemapper and consumed by all binary editors
/// (EnemyRandomizer, ChestRandomizer) that write item ID references.
///
/// PIPELINE RULE: If item shuffle is enabled, ItemRemapper must complete
/// and produce this table before any binary editor runs. Never pass a
/// stale or empty table to a binary editor when shuffle is active.
/// </summary>
public class ItemRemapTable
{
    private readonly Dictionary<int, int> _map;

    /// <summary>
    /// Creates a populated remap table from a completed mapping.
    /// </summary>
    /// <param name="map">Dictionary of originalItemId → newItemId.</param>
    public ItemRemapTable(Dictionary<int, int> map)
    {
        _map = new Dictionary<int, int>(map);
    }

    /// <summary>
    /// Creates an empty passthrough table — all IDs remap to themselves.
    /// Use this when item shuffle is disabled so binary editors can call
    /// Remap() unconditionally without checking a flag.
    /// </summary>
    public static ItemRemapTable Passthrough() => new(new Dictionary<int, int>());

    /// <summary>
    /// Returns the remapped item ID for the given original ID.
    /// If the ID is not in the table, returns the original ID unchanged.
    /// This means binary editors can always call Remap() safely.
    /// </summary>
    /// <param name="originalId">The item ID as it appears in the base game data.</param>
    /// <returns>The remapped ID, or originalId if no mapping exists.</returns>
    public int Remap(int originalId) =>
        _map.TryGetValue(originalId, out int newId) ? newId : originalId;

    /// <summary>True if this table contains any actual remappings.</summary>
    public bool HasRemappings => _map.Count > 0;

    /// <summary>Total number of item ID remappings in this table.</summary>
    public int Count => _map.Count;
}