// StiltzkinsBag.Tests/TestHelpers.cs
//
// Shared test utilities used across all test classes.

namespace StiltzkinsBag.Tests;

/// <summary>
/// Throw to skip a test with a reason message.
/// Shows as failed in Test Explorer with the skip reason.
/// xUnit has no built-in runtime skip — this is the standard workaround.
/// </summary>
public class SkipException : Exception
{
    public SkipException(string reason) : base(reason) { }
}