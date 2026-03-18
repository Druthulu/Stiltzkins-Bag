namespace StiltzkinsBag.Models;

/// <summary>
/// The result of a full randomizer pipeline run.
/// Returned by RandomizerEngine and consumed by MainViewModel.
/// </summary>
public class GenerationResult
{
    /// <summary>True if the run completed without fatal errors.</summary>
    public bool Success { get; init; }

    /// <summary>
    /// Absolute path to the generated seed folder.
    /// Null if the run failed before output was written.
    /// </summary>
    public string? OutputPath { get; init; }

    /// <summary>
    /// All errors and warnings collected during the run.
    /// Empty on a clean run. May contain non-fatal warnings even when Success is true.
    /// </summary>
    public IReadOnlyList<string> Messages { get; init; } = [];

    /// <summary>Creates a successful result.</summary>
    public static GenerationResult Succeeded(string outputPath, IEnumerable<string>? messages = null) =>
        new()
        {
            Success = true,
            OutputPath = outputPath,
            Messages = messages?.ToList() ?? []
        };

    /// <summary>Creates a failed result with one or more error messages.</summary>
    public static GenerationResult Failed(IEnumerable<string> errors) =>
        new()
        {
            Success = false,
            OutputPath = null,
            Messages = errors.ToList()
        };

    /// <summary>Convenience overload for a single error message.</summary>
    public static GenerationResult Failed(string error) => Failed([error]);
}