using Microsoft.Win32;
using System.IO;

namespace StiltzkinsBag.App.Services;

/// <summary>
/// Result of validating a candidate FFIX installation directory.
/// </summary>
public enum PathValidationResult
{
    /// <summary>FF9_Launcher.exe was not found — not a valid FFIX directory.</summary>
    NotFound,

    /// <summary>
    /// FF9_Launcher.exe found but Memoria.ini is absent.
    /// Valid FFIX install, but Memoria Engine is not present.
    /// Warn the user to install Memoria before generating.
    /// </summary>
    ValidNoMemoria,

    /// <summary>
    /// FF9_Launcher.exe and Memoria.ini both found.
    /// Valid FFIX install with Memoria Engine confirmed.
    /// </summary>
    ValidWithMemoria
}

/// <summary>
/// Attempts to auto-detect the Final Fantasy IX Steam installation directory
/// by reading well-known registry keys. No elevated permissions required —
/// all reads are on HKLM/HKCU with default user access.
///
/// Validation landmark: FF9_Launcher.exe (present in every Steam FFIX install).
/// Secondary check:     Memoria.ini     (present after Memoria Engine is installed).
///
/// Attempt order:
///   1. HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 377840  → InstallLocation
///   2. HKLM\SOFTWARE\WOW6432Node\...\Steam App 377840                             → InstallLocation (32-bit node)
///   3. HKCU\SOFTWARE\Valve\Steam                                                  → SteamPath + \steamapps\common\FINAL FANTASY IX
/// </summary>
public static class GamePathLocator
{
    private const string SteamAppKeyName = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 377840";
    private const string SteamAppKeyWow = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 377840";
    private const string SteamClientKey = @"SOFTWARE\Valve\Steam";
    private const string SteamAppSubPath = @"steamapps\common\FINAL FANTASY IX";

    private const string LauncherFile = "FF9_Launcher.exe";
    private const string MemoriaIniFile = "Memoria.ini";

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Attempts all three registry sources in order.
    /// Returns the first path containing FF9_Launcher.exe, or null if none succeed.
    /// </summary>
    public static string? TryAutoDetect()
    {
        return TryFromUninstallKey(Registry.LocalMachine, SteamAppKeyName)
            ?? TryFromUninstallKey(Registry.LocalMachine, SteamAppKeyWow)
            ?? TryFromSteamClient();
    }

    /// <summary>
    /// Validates a candidate path and returns a detailed result.
    /// Call this whenever GamePath changes to determine the correct UI state.
    /// </summary>
    public static PathValidationResult Validate(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return PathValidationResult.NotFound;

        if (!File.Exists(Path.Combine(path!, LauncherFile)))
            return PathValidationResult.NotFound;

        return File.Exists(Path.Combine(path!, MemoriaIniFile))
            ? PathValidationResult.ValidWithMemoria
            : PathValidationResult.ValidNoMemoria;
    }

    /// <summary>
    /// Returns true if the path contains FF9_Launcher.exe (regardless of Memoria).
    /// Convenience wrapper used for boolean CanGenerate checks.
    /// </summary>
    public static bool IsValidGamePath(string? path) =>
        Validate(path) != PathValidationResult.NotFound;

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private static string? TryFromUninstallKey(RegistryKey hive, string subKeyPath)
    {
        try
        {
            using var key = hive.OpenSubKey(subKeyPath, writable: false);
            var value = key?.GetValue("InstallLocation") as string;
            return IsValidGamePath(value) ? value : null;
        }
        catch
        {
            return null;
        }
    }

    private static string? TryFromSteamClient()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(SteamClientKey, writable: false);
            var steamPath = key?.GetValue("SteamPath") as string;
            if (string.IsNullOrWhiteSpace(steamPath))
                return null;

            var candidate = Path.Combine(steamPath!, SteamAppSubPath);
            return IsValidGamePath(candidate) ? candidate : null;
        }
        catch
        {
            return null;
        }
    }
}