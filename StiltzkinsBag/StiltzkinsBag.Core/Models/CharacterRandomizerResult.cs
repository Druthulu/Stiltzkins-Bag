using StiltzkinsBag.Models.Csv;

namespace StiltzkinsBag.Models;

/// <summary>
/// Immutable result produced by <see cref="StiltzkinsBag.Randomizers.CharacterRandomizer"/>.
/// Contains all CSV data that was modified by the character randomization pipeline.
/// </summary>
/// <param name="BaseStats">
/// Modified rows from <c>BaseStats.csv</c>.
/// Stat values may be shuffled (Recommended) or regenerated in range (Chaos).
/// </param>
/// <param name="CharacterParameters">
/// Modified rows from <c>CharacterParameters.csv</c>.
/// <c>DefaultEquipmentSet</c> values are shuffled by the Equipment sub-step (Task 5).
/// <c>DefaultCommandSet</c> and all other columns are never modified —
/// command slot assignment is expressed entirely through <see cref="CommandSetRows"/>.
/// </param>
/// <param name="CommandSetRows">
/// Rewritten rows 0–7 from <c>CommandSets.csv</c>.
/// Each row's <c>Regular1</c>, <c>Regular2</c>, <c>Trance1</c>, <c>Trance2</c> are
/// fully rewritten by the Speciality sub-step (Option B slot assignment).
/// Fixed columns (Attack, Defend, Item, Change and their trance equivalents) are
/// always written as vanilla values and never modified.
/// Rows 8–19 (guest/stage sets) are never included here — they are passed through
/// from vanilla by the mod output writer unchanged.
/// </param>
/// <param name="SlotAssignment">
/// Maps character ID (0–7) to the ordered list of slot type names assigned to them.
/// Example: <c>{0: ["Steal","Skill"], 3: ["Summon-A","Dragon"]}</c>.
/// Consumed by the Abilities sub-step (Task 4) to determine which AA package and
/// SA pre-seeds belong to each character.
/// Slot type name constants are defined in <c>SlotTypes</c>.
/// </param>
/// <param name="AbilityTables">
/// Modified ability rows keyed by character ID (0–15).
/// Each value is the full ordered list of ability entries for that character's CSV
/// (e.g. key 0 → Zidane.csv rows, key 1 → Vivi.csv rows).
/// The Abilities sub-step (Task 4) assigns AA packages and SA pool entries.
/// Guest character tables (IDs 8–15) are passed through unchanged.
/// </param>
/// <param name="AbilityFeaturesText">
/// Full text content for <c>AbilityFeatures.txt</c> in the mod output folder.
/// Identical to the input if <c>Settings.RandomizeSpeciality</c> is false.
/// When Speciality is enabled, the <c>&gt;CMD 31 Magic Sword</c>
/// <c>HardDisable</c> condition is rewritten to reference whichever character
/// received the Blk Mag slot.
/// </param>
public record CharacterRandomizerResult(
    IReadOnlyList<BaseStatsRow> BaseStats,
    IReadOnlyList<CharacterParametersRow> CharacterParameters,
    IReadOnlyList<CommandSetsRow> CommandSetRows,
    IReadOnlyDictionary<int, IReadOnlyList<string>> SlotAssignment,
    IReadOnlyDictionary<int, IReadOnlyList<CharacterAbilityRow>> AbilityTables,
    string AbilityFeaturesText);