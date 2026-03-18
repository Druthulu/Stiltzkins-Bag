namespace StiltzkinsBag.Randomizers;

/// <summary>
/// Converts a user-supplied seed string into a deterministic integer,
/// and creates the single seeded Random instance used by the entire pipeline.
///
/// ARCHITECTURAL RULE: CreateRandom() must be called exactly once per generation run.
/// The returned Random instance is passed to every randomizer in pipeline order.
/// No randomizer ever calls new Random() — they all consume this shared instance.
/// Violating this rule breaks seed determinism.
/// </summary>
public static class SeedEngine
{
    /// <summary>
    /// Converts a seed string to a stable, deterministic, platform-independent integer.
    ///
    /// Uses a djb2-style polynomial hash over UTF-16 char values.
    /// Math.Abs + modulo clamp ensures the result is always a non-negative int.
    ///
    /// NOTE: Do NOT use string.GetHashCode() here — it is randomized per-process
    /// since .NET Core and will produce different values across runs.
    /// </summary>
    /// <param name="seedString">The raw string entered by the user. Trimmed before hashing.</param>
    /// <returns>A non-negative deterministic integer in [0, int.MaxValue].</returns>
    public static int Resolve(string seedString)
    {
        if (string.IsNullOrWhiteSpace(seedString))
            return 0;

        string normalized = seedString.Trim();

        unchecked
        {
            int hash = 17;
            foreach (char c in normalized)
            {
                hash = hash * 31 + c;
            }
            return Math.Abs(hash);
        }
    }

    /// <summary>
    /// Creates a new seeded Random instance for the given seed integer.
    ///
    /// Call this exactly once per generation run.
    /// Pass the returned instance to every randomizer in pipeline order.
    /// </summary>
    /// <param name="seedInt">The resolved seed integer from Resolve().</param>
    /// <returns>A seeded System.Random instance ready for the pipeline.</returns>
    public static Random CreateRandom(int seedInt) => new Random(seedInt);

    /// <summary>
    /// Convenience overload — resolves the string and creates the Random in one call.
    /// </summary>
    /// <param name="seedString">The raw seed string from the user.</param>
    /// <returns>A seeded System.Random instance ready for the pipeline.</returns>
    public static Random CreateRandom(string seedString) => CreateRandom(Resolve(seedString));
}