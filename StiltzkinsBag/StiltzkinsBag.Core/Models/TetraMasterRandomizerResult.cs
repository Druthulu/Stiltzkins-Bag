namespace StiltzkinsBag.Core.Models;

/// <summary>
/// Immutable result record produced by <see cref="Randomizers.TetraMasterRandomizer"/>.
///
/// Each byte array is a ready-to-write replacement for the corresponding file extracted
/// from resources.assets. Callers should write non-null outputs to the mod folder.
///
/// Output paths:
/// <list type="bullet">
///   <item><see cref="CardStats"/>  → {modRoot}\embeddedasset\quadmist\minigame_card_data_address</item>
///   <item><see cref="CardSets"/>   → {modRoot}\embeddedasset\quadmist\minigame_card_level_address</item>
///   <item><see cref="NpcDecks"/>   → {modRoot}\embeddedasset\quadmist\minigame_stage_address</item>
///   <item><see cref="CardNames"/>  → {modRoot}\embeddedasset\text\{lang}\etc\minista.mes  (one per language key)</item>
/// </list>
/// </summary>
/// <param name="CardStats">
/// Patched minigame_card_data_address bytes (500 bytes, 100 cards × 5 bytes each).
/// Always present.
/// </param>
/// <param name="CardSets">
/// Patched minigame_card_level_address bytes (1024 bytes, 64 sets × 16 card IDs each).
/// Always present.
/// </param>
/// <param name="NpcDecks">
/// Patched minigame_stage_address bytes (512 bytes, 256 decks × 2 bytes each).
/// Always present.
/// </param>
/// <param name="CardNames">
/// Per-language patched minista.mes bytes, keyed by language code (e.g. "us", "jp").
/// <see langword="null"/> when <see cref="Settings.ShuffleCardOrder"/> is <see langword="false"/>.
/// When non-null, contains one entry per language whose name file was supplied to the randomizer.
/// </param>
public sealed record TetraMasterRandomizerResult(
    byte[] CardStats,
    byte[] CardSets,
    byte[] NpcDecks,
    IReadOnlyDictionary<string, byte[]>? CardNames);