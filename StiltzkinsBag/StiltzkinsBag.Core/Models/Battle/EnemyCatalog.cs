// StiltzkinsBag.Core/Models/Battle/EnemyCatalog.cs
//
// Loads the enemy file catalog from StockEnemyBytesJsonNoZeros.json.
// This JSON is the master list of every battle file the randomizer needs to touch.
// It ships with the application as a resource.
//
// Each entry provides:
//   - EnemyFolder: relative path to the .bytes file (used to locate the file on disk
//     or inside an archive via ModSourceResolver)
//   - GroupCount / StatCount: header metadata, used to validate extracted bytes
//   - VanillaBytes: the vanilla byte content (trailing zeros stripped) — used as the
//     vanilla fallback when no mod provides a copy of this file

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StiltzkinsBag.Core.Models.Battle;

/// <summary>
/// A single entry in the enemy catalog.
/// </summary>
public sealed class EnemyCatalogEntry
{
    /// <summary>
    /// Relative path from the game root, using backslashes, starting with \StreamingAssets.
    /// e.g. \StreamingAssets\assets\resources\battlemap\battlescene\evt_battle_ac_e028f\dbfile0000.raw16.bytes
    /// </summary>
    [JsonPropertyName("EnemyFolder")]
    public string EnemyFolder { get; init; } = string.Empty;

    /// <summary>
    /// Base64-encoded vanilla bytes with trailing zeros stripped.
    /// Use VanillaBytes property to get the decoded array.
    /// </summary>
    [JsonPropertyName("EnemyBytes")]
    public string EnemyBytesBase64 { get; init; } = string.Empty;

    // GroupCount and StatCount are read directly from bytes[1] and bytes[2] at runtime.
    // BinAddressStart, IncorrectBytesCount, IncorrectBytesPercent were test artifacts
    // and have been removed from the JSON.

    /// <summary>Decoded vanilla bytes. Decoded on first access and cached.</summary>
    public byte[] GetVanillaBytes()
    {
        if (string.IsNullOrEmpty(EnemyBytesBase64))
            return [];
        return Convert.FromBase64String(EnemyBytesBase64);
    }
}

/// <summary>
/// Loads and exposes the complete enemy file catalog.
/// </summary>
public sealed class EnemyCatalog
{
    private readonly IReadOnlyList<EnemyCatalogEntry> _entries;

    /// <summary>All catalog entries in their original order.</summary>
    public IReadOnlyList<EnemyCatalogEntry> Entries => _entries;

    /// <summary>Number of enemy files in the catalog.</summary>
    public int Count => _entries.Count;

    // ── Factory methods ────────────────────────────────────────────────────

    /// <summary>
    /// Loads the catalog from a JSON file on disk.
    /// </summary>
    /// <param name="jsonPath">Full path to StockEnemyBytesJsonNoZeros.json.</param>
    public static EnemyCatalog FromFile(string jsonPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(jsonPath);
        if (!File.Exists(jsonPath))
            throw new FileNotFoundException(
                $"Enemy catalog not found: '{jsonPath}'", jsonPath);

        string json = File.ReadAllText(jsonPath);
        return FromJson(json);
    }

    /// <summary>
    /// Loads the catalog from a JSON string.
    /// </summary>
    public static EnemyCatalog FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        var entries = JsonSerializer.Deserialize<List<EnemyCatalogEntry>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (entries is null || entries.Count == 0)
            throw new InvalidDataException("Enemy catalog JSON produced no entries.");

        return new EnemyCatalog(entries);
    }

    // ── Private constructor ────────────────────────────────────────────────

    private EnemyCatalog(List<EnemyCatalogEntry> entries)
    {
        _entries = entries.AsReadOnly();
    }
}