using System.Text.RegularExpressions;
using StiltzkinsBag.Models;
using StiltzkinsBag.Models.Csv;

namespace StiltzkinsBag.Randomizers;

/// <summary>
/// Randomizes character data across four ordered sub-steps:
///     1. Base stats   (BaseStats.csv)
///     2. Speciality   (CommandSets.csv rewrite + AbilityFeatures.txt patch)
///     3. Abilities    (individual character ability CSV files)
///     4. Equipment    (CharacterParameters.csv — DefaultEquipmentSet only)
///
/// CRITICAL — RNG call order is sacred:
///   Sub-steps that are disabled via Settings flags must contribute ZERO calls
///   to the shared <see cref="Random"/> instance. Never consume-and-discard.
///   The pipeline order above is fixed and must never be reordered.
/// </summary>
public sealed class CharacterRandomizer
{
    // ── Constants ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Character IDs 0–7 are the eight main playable characters:
    /// Zidane (0), Vivi (1), Garnet (2), Steiner (3), Freya (4),
    /// Quina (5), Eiko (6), Amarant (7).
    /// Speciality and equipment sub-steps target IDs 0–7 only.
    /// Stats sub-step targets all 12 IDs (0–11) including guests.
    /// </summary>
    private const int MainCharacterCount = 8;

    // Fixed CommandSetsRow column values — identical for all 8 main chars
    private const int CmdAttack = 1;
    private const int CmdDefend = 4;
    private const int CmdItem = 14;
    private const int CmdChange = 7;

    // DefaultCategory value for female characters — used for SA:38 gender filter
    private const int FemaleCategoryValue = 6;

    // Tracked ability refs used in multiple places
    private const string OdinAbilityRef = "AA:58";   // Odin summon
    private const string OdinsSwordAbilityRef = "SA:60";   // Odin's Sword SA
    private const string GuardianMogAbilityRef = "SA:47";   // Guardian Mog SA
    private const string ProtectGirlsAbilityRef = "SA:38";  // Protect Girls SA

    // ── Slot Type Registry ────────────────────────────────────────────────────

    /// <summary>
    /// Canonical slot type name constants.
    /// These strings are the keys in <see cref="CharacterRandomizerResult.SlotAssignment"/>.
    /// Task 4 reads them to determine which AA package each character receives.
    /// </summary>
    private static class SlotTypes
    {
        // Zidane-locked slots
        public const string Steal = "Steal";
        public const string Skill = "Skill";

        // Locked pairs — one list entry covers both R1 and R2
        public const string BlueMage = "Blue Mage";   // Eat + Blu Mag
        public const string Knight = "Knight";       // Swd Art + Swd Mag

        // Primary slots (assigned as R1; R2 comes from free pool)
        public const string SummonA = "Summon-A";
        public const string SummonB = "Summon-B";
        public const string WhtMagA = "Wht Mag-A";
        public const string WhtMagB = "Wht Mag-B";

        // Free pool slots (R2 of primaries, or both slots of the unassigned char)
        public const string BlkMag = "Blk Mag";
        public const string Focus = "Focus";
        public const string Jump = "Jump";
        public const string Dragon = "Dragon";
        public const string Flair = "Flair";
        public const string Throw = "Throw";

        /// <summary>
        /// Slot types that qualify as "magic" for the Focus constraint check and
        /// T2 magic pre-seed distribution.
        /// </summary>
        public static readonly IReadOnlySet<string> MagicTypes =
            new HashSet<string> { BlkMag, WhtMagA, WhtMagB, SummonA, SummonB, BlueMage };

        /// <summary>Slot types that are summoner-type for SA:33 Concentrate priority.</summary>
        public static readonly IReadOnlySet<string> SummonerTypes =
            new HashSet<string> { SummonA, SummonB };

        /// <summary>
        /// The canonical free pool — always exactly 6 items, always the same set.
        /// Order is the starting order before any shuffle.
        /// </summary>
        public static IReadOnlyList<string> FreePool =>
            new List<string> { BlkMag, Focus, Jump, Dragon, Flair, Throw };

        /// <summary>
        /// Primary role assignment order for the random path.
        /// Positional: shuffled char index 0 → BlueMage, 1 → Knight, etc.
        /// </summary>
        public static IReadOnlyList<string> PrimaryOrder =>
            new List<string> { BlueMage, Knight, SummonA, SummonB, WhtMagA, WhtMagB };
    }

    // ── Stat-Bias Score Functions ──────────────────────────────────────────────

    /// <summary>
    /// Maps slot type name to its stat-bias scoring formula.
    /// Higher score = better candidate for that slot type.
    /// Used only in the Recommended + RandomizeBaseStats = true path.
    ///
    /// Formulas reflect the vanilla character who defined each archetype.
    /// Negative terms are intentional — they make formulas exclusive and prevent
    /// one high-stat character from dominating every slot type.
    ///
    /// byte arithmetic promotes to int automatically; no casts needed in lambdas.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, Func<BaseStatsRow, int>> SlotScores =
        new Dictionary<string, Func<BaseStatsRow, int>>
        {
            // Magic archetypes — all Magic-primary, distinct Will/penalty terms
            [SlotTypes.BlkMag] = r => r.Magic * 2 - r.Dexterity,              // Vivi: devastating but slow
            [SlotTypes.WhtMagA] = r => r.Magic * 2 - r.Strength,               // Garnet: gentle, not a fighter
            [SlotTypes.WhtMagB] = r => r.Magic * 2 + r.Will,                   // Eiko: spirited conviction
            [SlotTypes.SummonA] = r => r.Magic + r.Will - r.Strength,  // Garnet-style: measured, elegant
            [SlotTypes.SummonB] = r => r.Magic + r.Will * 2 - r.Dexterity, // Eiko-style: Will-dominant
            [SlotTypes.BlueMage] = r => r.Magic + r.Will - r.Dexterity, // Quina: versatile but slow
            [SlotTypes.Focus] = r => r.Magic + r.Will - r.Strength,  // Concentration: aptitude + spirit

            // Physical archetypes — distinct Strength/Dexterity/Will emphasis
            [SlotTypes.Knight] = r => r.Strength * 2 + r.Will - r.Dexterity,    // Steiner: immovable, not nimble
            [SlotTypes.Jump] = r => r.Dexterity + r.Strength + r.Will,      // Freya: perfectly balanced
            [SlotTypes.Dragon] = r => r.Will * 2 - r.Dexterity,             // Ancient wisdom — ignores Strength
                                                                            // Dragon's Crest scales off kills
            [SlotTypes.Flair] = r => r.Strength + r.Will * 2 - r.Magic,    // Amarant: raw power, dislikes magic
            [SlotTypes.Throw] = r => r.Dexterity * 2 + r.Strength - r.Magic,   // Pure physical precision
        };

    // ── Ability Pool Data Tables ───────────────────────────────────────────────

    /// <summary>
    /// The 12 summon AAs in their canonical starting order (before shuffle).
    /// Garnet's 8 first, Eiko's 4 second.
    /// Fisher-Yates shuffle on this pool; Summon-A gets indices 0–5, Summon-B gets 6–11.
    /// </summary>
    private static readonly IReadOnlyList<string> SummonPool = new[]
    {
        "AA:49", "AA:51", "AA:53", "AA:55",  // Garnet: Shiva, Ifrit, Ramuh, Atomos
        "AA:58", "AA:60", "AA:62", "AA:64",  // Garnet: Odin, Leviathan, Bahamut, Ark
        "AA:66", "AA:68", "AA:72", "AA:74"   // Eiko: Fenrir, Carbuncle, Phoenix, Madeen
    };

    /// <summary>
    /// The 24 white magic AAs in their canonical starting order (before shuffle).
    /// All from the merged Garnet + Eiko pool — no duplicates.
    /// Fisher-Yates shuffle; Wht Mag-A gets indices 0–11, Wht Mag-B gets 12–23.
    /// </summary>
    private static readonly IReadOnlyList<string> WhiteMagicPool = new[]
    {
        "AA:1",  "AA:2",  "AA:3",  "AA:4",  "AA:5",  "AA:6",
        "AA:7",  "AA:8",  "AA:9",  "AA:10", "AA:11", "AA:12",
        "AA:13", "AA:14", "AA:15", "AA:16", "AA:17", "AA:18",
        "AA:19", "AA:20", "AA:21", "AA:22", "AA:23", "AA:24"
    };

    /// <summary>
    /// Maps slot type name to the complete list of fixed AA ability refs for that class.
    /// Only slot types with a static (non-pool) AA package are listed here.
    /// Summon-A/B, Wht Mag-A/B are handled by pool splits.
    /// Jump, Throw, Focus, Skill have no AAs.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> AaPackages =
        new Dictionary<string, IReadOnlyList<string>>
        {
            // Zidane: Flee, Detect, What's That!?, Soul Blade, Annoy, Sacrifice, Lucky Seven, Thievery
            [SlotTypes.Steal] = new[] { "AA:101", "AA:102", "AA:103", "AA:104", "AA:105", "AA:106", "AA:107", "AA:108" },

            // Black Mage: Fire through Doomsday (24 spells)
            [SlotTypes.BlkMag] = Enumerable.Range(25, 24).Select(i => $"AA:{i}").ToArray(),

            // Knight: Darkside through Shock (12 Sword Arts)
            [SlotTypes.Knight] = Enumerable.Range(141, 12).Select(i => $"AA:{i}").ToArray(),

            // Dragon: Lancer, Reis's Wind, Dragon Breath, White Draw, Luna,
            //         Six Dragons, Cherry Blossom, Dragon's Crest
            [SlotTypes.Dragon] = new[] { "AA:117", "AA:118", "AA:119", "AA:120", "AA:121", "AA:122", "AA:123", "AA:124" },

            // Blue Mage: Goblin Punch through Auto-Life (24 blue magic spells)
            [SlotTypes.BlueMage] = Enumerable.Range(77, 24).Select(i => $"AA:{i}").ToArray(),

            // Flair: Chakra, Spare Change, No Mercy, Aura, Curse, Revive, Demi Shock, Countdown
            [SlotTypes.Flair] = new[] { "AA:125", "AA:126", "AA:127", "AA:128", "AA:129", "AA:130", "AA:131", "AA:132" },

            // Skill, Jump, Throw, Focus, Summon-A/B, Wht Mag-A/B:
            // Either no AAs, or handled via pool splits. Not listed here.
        };

    // ── SA Pre-Seed Data Tables ────────────────────────────────────────────────

    /// <summary>
    /// T1 — Guaranteed pre-seeds. Applied in all modes before pool shuffle.
    /// Each entry: (AbilityRef, SlotType whose holder receives this SA).
    ///
    /// Special T1 seeds not in this table (handled with explicit RNG or tracking):
    ///   SA:47 Guardian Mog   — random pick between Summon-A and Summon-B holder (1 RNG call)
    ///   SA:60 Odin's Sword   — always goes to whichever summoner received AA:58 Odin
    /// </summary>
    private static readonly IReadOnlyList<(string AbilityRef, string SlotType)> T1Seeds =
        new (string, string)[]
        {
            // Thief (always Zidane)
            ("SA:62", SlotTypes.Steal),   // Bandit — core Steal success rate
            ("SA:61", SlotTypes.Steal),   // Mug — physical damage on steal
            ("SA:22", SlotTypes.Steal),   // Master Thief — guarantees rare steal

            // Black Mage
            ("SA:32", SlotTypes.BlkMag),  // Mag Elem Null — negates elemental weaknesses
            ("SA:31", SlotTypes.BlkMag),  // Reflectx2 — reflected spells deal double damage

            // Knight
            ("SA:37", SlotTypes.Knight),  // Cover — intercepts attacks on critical-HP allies

            // Jump / Dragoon
            ("SA:21", SlotTypes.Jump),    // High Jump — extends Jump air time and damage
            ("SA:42", SlotTypes.Jump),    // Initiative — guarantees preemptive strikes

            // Throw
            ("SA:28", SlotTypes.Throw),   // Power Throw — doubles Throw damage by item count

            // Flair / Monk
            ("SA:29", SlotTypes.Flair),   // Power Up — doubles physical damage at critical HP

            // Summoners (both get Boost)
            ("SA:59", SlotTypes.SummonA), // Boost — enhances summon power (1st copy)
            ("SA:59", SlotTypes.SummonB), // Boost — enhances summon power (2nd copy)
        };

    /// <summary>
    /// T2 — Recommended mode only magic pre-seeds.
    /// Distributed first-come-first-served to magic-type chars sorted by char ID
    /// until pool copies are exhausted.
    /// SummonerPriority = true means summoner chars are sorted first within the list.
    ///
    /// The number of copies available in the vanilla pool naturally limits distribution
    /// (TryPreSeedFromPool returns false when pool is exhausted for that AbilityRef).
    /// </summary>
    private static readonly IReadOnlyList<(string AbilityRef, bool SummonerPriority)> T2MagicSeeds =
        new (string, bool)[]
        {
            ("SA:33", true),   // Concentrate — summoner priority (2 copies: Garnet + Eiko)
            ("SA:8",  false),  // MP+20%       (3 copies: Vivi + Garnet + Eiko)
            ("SA:34", false),  // Half MP       (4 copies: Vivi + Garnet + Quina + Eiko)
            ("SA:24", false),  // Healer        (5 copies: Vivi + Garnet + Quina + Amarant + Eiko)
            ("SA:25", false),  // Add Status    (6 copies: Zidane + Vivi + Steiner + Freya + Quina + Amarant)
            ("SA:54", false),  // Return Magic  (2 copies: Vivi + Amarant)
            ("SA:55", false),  // Absorb MP     (1 copy: Quina)
            ("SA:30", false),  // Reflect-Null  (2 copies: Vivi + Eiko)
        };

    /// <summary>
    /// T2 — Recommended mode only physical pre-seeds.
    /// Each entry: one copy of AbilityRef pre-seeded to the holder of SlotType.
    /// Entries with the same AbilityRef represent separate copies for different slot holders.
    /// SA:38 Protect Girls has a gender filter applied at assignment time.
    /// </summary>
    private static readonly IReadOnlyList<(string AbilityRef, string SlotType)> T2PhysicalSeeds =
        new (string, string)[]
        {
            ("SA:6",  SlotTypes.Knight),  // HP+20% — tank needs HP
            ("SA:6",  SlotTypes.Flair),   // HP+20% — brawler needs HP (2nd copy)
            ("SA:9",  SlotTypes.Knight),  // Accuracy+ — Sword Arts need to connect
            ("SA:9",  SlotTypes.Throw),   // Accuracy+ — Throw needs precision (2nd copy)
            ("SA:12", SlotTypes.Knight),  // MP Attack — bonus damage via MP, synergizes with Sword Arts
            ("SA:36", SlotTypes.Knight),  // Counter — retaliates physically (Knight identity)
            ("SA:36", SlotTypes.Flair),   // Counter — retaliates physically (Monk identity, 2nd copy)
            ("SA:17", SlotTypes.Dragon),  // Dragon Killer — bonus damage to dragons
            ("SA:17", SlotTypes.Jump),    // Dragon Killer — thematic for dragon hunter (2nd copy)
            ("SA:35", SlotTypes.Jump),    // High Tide — fills Trance faster for Jump 2
            ("SA:52", SlotTypes.Flair),   // Restore HP — synergizes with Power Up risk/reward
            ("SA:23", SlotTypes.Steal),   // Steal Gil — auto-steals gil (Thief thematic)
            ("SA:46", SlotTypes.Steal),   // Flee-Gil — gil on flee (Thief thematic)
            (ProtectGirlsAbilityRef, SlotTypes.Steal), // Protect Girls — male chars only (gender filter)
        };

    // ── Character ID → Memoria Name Map ───────────────────────────────────────

    /// <summary>
    /// Maps character ID (0–7) to the <c>CharacterId_*</c> name used in
    /// Memoria's NCalc expressions, including <c>AbilityFeatures.txt</c>.
    /// </summary>
    private static readonly IReadOnlyDictionary<int, string> CharacterIdNames =
        new Dictionary<int, string>
        {
            [0] = "CharacterId_Zidane",
            [1] = "CharacterId_Vivi",
            [2] = "CharacterId_Garnet",
            [3] = "CharacterId_Steiner",
            [4] = "CharacterId_Freya",
            [5] = "CharacterId_Quina",
            [6] = "CharacterId_Eiko",
            [7] = "CharacterId_Amarant"
        };

    // ── Inputs ────────────────────────────────────────────────────────────────

    private readonly Random _rng;
    private readonly Settings _settings;
    private readonly IReadOnlyList<BaseStatsRow> _vanillaBaseStats;
    private readonly IReadOnlyList<CharacterParametersRow> _characterParameters;
    private readonly IReadOnlyList<CommandSetsRow> _vanillaCommandSets;
    private readonly Dictionary<int, List<CharacterAbilityRow>> _abilityTables;
    private readonly string _abilityFeaturesText;

    // ── Constructor ───────────────────────────────────────────────────────────

    /// <param name="rng">
    /// The single seeded <see cref="Random"/> from <c>SeedEngine</c>.
    /// Never construct a new <see cref="Random"/> here.
    /// </param>
    /// <param name="baseStats">Rows from <c>BaseStats.csv</c>. 12 entries, IDs 0–11.</param>
    /// <param name="characterParameters">Rows from <c>CharacterParameters.csv</c>. 12 entries, IDs 0–11.</param>
    /// <param name="commandSets">
    /// Rows from <c>CommandSets.csv</c>. Must include at least rows 0–7.
    /// Rows 0–7 are rewritten by the Speciality sub-step.
    /// Rows 8–19 (guests and stage sets) are never modified and not included in the result.
    /// </param>
    /// <param name="abilityTables">
    /// Ability rows keyed by character ID.
    /// Task 4 completely rebuilds entries for IDs 0–7. Guests (8–15) pass through unchanged.
    /// </param>
    /// <param name="abilityFeaturesText">
    /// Full text of <c>AbilityFeatures.txt</c>. Passed through unless
    /// <c>Settings.RandomizeSpeciality</c> is true, in which case the
    /// <c>&gt;CMD 31 Magic Sword</c> HardDisable is rewritten.
    /// </param>
    public CharacterRandomizer(
        Random rng,
        Settings settings,
        IReadOnlyList<BaseStatsRow> baseStats,
        IReadOnlyList<CharacterParametersRow> characterParameters,
        IReadOnlyList<CommandSetsRow> commandSets,
        Dictionary<int, List<CharacterAbilityRow>> abilityTables,
        string abilityFeaturesText)
    {
        _rng = rng;
        _settings = settings;
        _vanillaBaseStats = baseStats;
        _characterParameters = characterParameters;
        _vanillaCommandSets = commandSets;
        _abilityTables = abilityTables;
        _abilityFeaturesText = abilityFeaturesText;
    }

    // ── Public Entry Point ────────────────────────────────────────────────────

    /// <summary>
    /// Executes all enabled sub-steps in pipeline order.
    /// Defensive copies of all inputs are made before any modification.
    /// Constructor inputs are never mutated.
    /// </summary>
    public CharacterRandomizerResult Randomize()
    {
        // ── Defensive copies ──────────────────────────────────────────────────
        var baseStats = _vanillaBaseStats.Select(CloneRow).ToList();
        var charParams = _characterParameters.Select(CloneRow).ToList();
        var abilities = _abilityTables.ToDictionary(
                             kvp => kvp.Key,
                             kvp => kvp.Value.Select(CloneRow).ToList());
        var abilityFeatures = _abilityFeaturesText;

        // ── Speciality outputs — populated below or via vanilla fallback ──────
        List<CommandSetsRow> commandSetRows;
        Dictionary<int, List<string>> slotAssignment;

        // ── Pipeline — order is enforced, disabled steps consume zero RNG ─────

        if (_settings.RandomizeBaseStats)
            RandomizeBaseStatsStep(baseStats);

        if (_settings.RandomizeSpeciality)
        {
            (commandSetRows, slotAssignment) =
                RandomizeSpecialityStep(baseStats, ref abilityFeatures);
        }
        else
        {
            commandSetRows = BuildVanillaCommandSetRows();
            slotAssignment = BuildVanillaSlotAssignment();
        }

        if (_settings.RandomizeAbilities)
            RandomizeAbilitiesStep(abilities, slotAssignment);

        if (_settings.RandomizeEquipment)
            RandomizeEquipmentStep(charParams);

        // ── Build result ──────────────────────────────────────────────────────
        return new CharacterRandomizerResult(
            baseStats,
            charParams,
            commandSetRows,
            slotAssignment.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyList<string>)kvp.Value),
            abilities.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyList<CharacterAbilityRow>)kvp.Value),
            abilityFeatures);
    }

    // ── Sub-Step 1: Base Stats ────────────────────────────────────────────────

    /// <summary>
    /// Dispatches to the appropriate stats strategy based on mode.
    /// In Recommended mode, applies the Zidane Dex bias after shuffling.
    /// All 12 characters (IDs 0–11) are included — guests participate in battles.
    /// </summary>
    private void RandomizeBaseStatsStep(List<BaseStatsRow> rows)
    {
        if (_settings.Mode == RandomizerMode.Chaos)
            RandomizeBaseStatsChaos(rows);
        else
        {
            RandomizeBaseStatsRecommended(rows);
            ApplyZidaneDexBias(rows);
        }
    }

    /// <summary>
    /// Shuffles each stat column independently via Fisher-Yates.
    /// Every vanilla value is preserved — only the character distribution changes.
    /// RNG: 5 × (N−1) calls where N = row count.
    /// </summary>
    private void RandomizeBaseStatsRecommended(List<BaseStatsRow> rows)
    {
        ShuffleColumn(rows, r => r.Dexterity, (r, v) => r.Dexterity = v);
        ShuffleColumn(rows, r => r.Strength, (r, v) => r.Strength = v);
        ShuffleColumn(rows, r => r.Magic, (r, v) => r.Magic = v);
        ShuffleColumn(rows, r => r.Will, (r, v) => r.Will = v);
        ShuffleColumn(rows, r => r.Gems, (r, v) => r.Gems = v);
    }

    /// <summary>
    /// Generates a uniformly random value in the vanilla [min, max] range per stat per character.
    /// Bounds are always derived from <c>_vanillaBaseStats</c>, never the working copy.
    /// RNG: 5 × N calls.
    /// </summary>
    private void RandomizeBaseStatsChaos(List<BaseStatsRow> rows)
    {
        int dexMin = _vanillaBaseStats.Min(r => r.Dexterity), dexMax = _vanillaBaseStats.Max(r => r.Dexterity);
        int strMin = _vanillaBaseStats.Min(r => r.Strength), strMax = _vanillaBaseStats.Max(r => r.Strength);
        int magMin = _vanillaBaseStats.Min(r => r.Magic), magMax = _vanillaBaseStats.Max(r => r.Magic);
        int wilMin = _vanillaBaseStats.Min(r => r.Will), wilMax = _vanillaBaseStats.Max(r => r.Will);
        int gemMin = (int)_vanillaBaseStats.Min(r => r.Gems), gemMax = (int)_vanillaBaseStats.Max(r => r.Gems);

        foreach (var row in rows)
        {
            row.Dexterity = (byte)_rng.Next(dexMin, dexMax + 1);
            row.Strength = (byte)_rng.Next(strMin, strMax + 1);
            row.Magic = (byte)_rng.Next(magMin, magMax + 1);
            row.Will = (byte)_rng.Next(wilMin, wilMax + 1);
            row.Gems = (uint)_rng.Next(gemMin, gemMax + 1);
        }
    }

    /// <summary>
    /// Ensures Zidane (char 0) always has the highest Dexterity among the 8 main characters.
    /// If he does not, swaps his Dex with whoever has the maximum.
    /// Zero RNG calls — deterministic correction.
    ///
    /// Rationale: Zidane's Steal slot is always locked to him. Steal success rate
    /// scales with Dexterity. This bias ensures the locked Steal holder benefits
    /// from maximum precision after any stat shuffle.
    /// </summary>
    private static void ApplyZidaneDexBias(List<BaseStatsRow> rows)
    {
        var mainRows = rows.Where(r => r.Id < MainCharacterCount).ToList();
        var zidane = mainRows.First(r => r.Id == 0);
        var maxRow = mainRows.OrderByDescending(r => r.Dexterity).ThenBy(r => r.Id).First();

        if (maxRow.Id == 0) return;
        (zidane.Dexterity, maxRow.Dexterity) = (maxRow.Dexterity, zidane.Dexterity);
    }

    // ── Sub-Step 2: Speciality ────────────────────────────────────────────────

    /// <summary>
    /// Option B slot assignment: rewrites all 8 <c>CommandSets.csv</c> rows for main
    /// characters. Produces a <c>SlotAssignment</c> dictionary consumed by Task 4.
    /// Patches <c>AbilityFeatures.txt</c> for the Magic Sword (CMD 31) HardDisable.
    ///
    /// Three paths:
    ///   Recommended + RandomizeBaseStats=true  → stat-biased  (0 RNG for assignment)
    ///   Recommended + RandomizeBaseStats=false → random       (11 RNG calls)
    ///   Chaos (either stats setting)           → random       (11 RNG calls)
    ///
    /// An additional Focus constraint check in Recommended mode may consume 1 more call.
    /// </summary>
    private (List<CommandSetsRow>, Dictionary<int, List<string>>) RandomizeSpecialityStep(
        IReadOnlyList<BaseStatsRow> currentStats,
        ref string abilityFeatures)
    {
        var slotAssignment = DetermineSlotAssignment(currentStats);

        if (_settings.Mode == RandomizerMode.Recommended)
            ApplyFocusConstraint(slotAssignment);

        var commandSetRows = BuildCommandSetRows(slotAssignment);

        var blkMagEntry = slotAssignment.FirstOrDefault(
            kvp => kvp.Value.Contains(SlotTypes.BlkMag));

        if (blkMagEntry.Value != null)
            abilityFeatures = PatchMagicSwordHardDisable(abilityFeatures, blkMagEntry.Key);

        return (commandSetRows, slotAssignment);
    }

    private Dictionary<int, List<string>> DetermineSlotAssignment(
        IReadOnlyList<BaseStatsRow> currentStats)
    {
        bool useStatBias = _settings.Mode == RandomizerMode.Recommended
                        && _settings.RandomizeBaseStats;
        return useStatBias
            ? AssignSlotsStatBiased(currentStats)
            : AssignSlotsRandom();
    }

    // ── Stat-Biased Path ──────────────────────────────────────────────────────

    private Dictionary<int, List<string>> AssignSlotsStatBiased(
        IReadOnlyList<BaseStatsRow> currentStats)
    {
        var result = new Dictionary<int, List<string>>();
        var available = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
        var statsLookup = currentStats.ToDictionary(r => r.Id);

        result[0] = new List<string> { SlotTypes.Steal, SlotTypes.Skill };

        foreach (string slotType in SlotTypes.PrimaryOrder)
        {
            int best = PickBestChar(available, slotType, statsLookup);
            available.Remove(best);
            result[best] = new List<string> { slotType };
        }

        int unassigned = available[0];
        result[unassigned] = new List<string>();

        var freePool = new List<string>(SlotTypes.FreePool);
        var openSlotOwners = new List<int>();
        foreach (string primary in new[] { SlotTypes.SummonA, SlotTypes.SummonB,
                                           SlotTypes.WhtMagA, SlotTypes.WhtMagB })
        {
            int owner = result.First(kvp => kvp.Value.Count > 0 && kvp.Value[0] == primary).Key;
            openSlotOwners.Add(owner);
        }
        openSlotOwners.Add(unassigned);
        openSlotOwners.Add(unassigned);

        foreach (int charId in openSlotOwners)
        {
            string best = PickBestFreeSlot(freePool, charId, statsLookup);
            freePool.Remove(best);
            result[charId].Add(best);
        }

        return result;
    }

    private static int PickBestChar(
        List<int> available, string slotType, Dictionary<int, BaseStatsRow> statsLookup)
    {
        var score = SlotScores[slotType];
        return available.OrderByDescending(id => score(statsLookup[id])).ThenBy(id => id).First();
    }

    private static string PickBestFreeSlot(
        List<string> freePool, int charId, Dictionary<int, BaseStatsRow> statsLookup)
    {
        var charStats = statsLookup[charId];
        return freePool
            .OrderByDescending(slot => SlotScores.TryGetValue(slot, out var s) ? s(charStats) : 0)
            .ThenBy(slot => slot)
            .First();
    }

    // ── Random Path ───────────────────────────────────────────────────────────

    /// <summary>RNG: 6 calls (chars) + 5 calls (free pool) = 11 total.</summary>
    private Dictionary<int, List<string>> AssignSlotsRandom()
    {
        var result = new Dictionary<int, List<string>>();
        result[0] = new List<string> { SlotTypes.Steal, SlotTypes.Skill };

        var chars = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
        for (int i = chars.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        for (int i = 0; i < SlotTypes.PrimaryOrder.Count; i++)
            result[chars[i]] = new List<string> { SlotTypes.PrimaryOrder[i] };

        result[chars[6]] = new List<string>();

        var freePool = new List<string>(SlotTypes.FreePool);
        for (int i = freePool.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (freePool[i], freePool[j]) = (freePool[j], freePool[i]);
        }

        result[chars[2]].Add(freePool[0]);
        result[chars[3]].Add(freePool[1]);
        result[chars[4]].Add(freePool[2]);
        result[chars[5]].Add(freePool[3]);
        result[chars[6]].Add(freePool[4]);
        result[chars[6]].Add(freePool[5]);

        return result;
    }

    // ── Focus Constraint ──────────────────────────────────────────────────────

    private void ApplyFocusConstraint(Dictionary<int, List<string>> slotAssignment)
    {
        var focusEntry = slotAssignment.FirstOrDefault(kvp => kvp.Value.Contains(SlotTypes.Focus));
        if (focusEntry.Value == null) return;

        int focusCharId = focusEntry.Key;
        List<string> focusSlots = focusEntry.Value;

        if (focusSlots.Any(s => SlotTypes.MagicTypes.Contains(s))) return;

        var swapCandidates = slotAssignment
            .Where(kvp => kvp.Key != focusCharId
                       && kvp.Value.Count == 2
                       && kvp.Value.Any(s => SlotTypes.MagicTypes.Contains(s))
                       && !SlotTypes.MagicTypes.Contains(kvp.Value[1]))
            .Select(kvp => kvp.Key)
            .ToList();

        int focusIdx = focusSlots.IndexOf(SlotTypes.Focus);

        if (swapCandidates.Count > 0)
        {
            int swapCharId = swapCandidates[_rng.Next(swapCandidates.Count)];
            var swapSlots = slotAssignment[swapCharId];
            string displaced = swapSlots[1];
            swapSlots[1] = SlotTypes.Focus;
            focusSlots[focusIdx] = displaced;
        }
        else
        {
            var fallback = slotAssignment
                .Where(kvp => kvp.Key != focusCharId
                           && kvp.Value.Any(s => SlotTypes.MagicTypes.Contains(s)))
                .OrderBy(kvp => kvp.Key)
                .FirstOrDefault();

            if (fallback.Value == null) return;

            var fallbackSlots = fallback.Value;
            if (fallbackSlots.Count < 2)
            {
                fallbackSlots.Add(SlotTypes.Focus);
                focusSlots.RemoveAt(focusIdx);
            }
            else
            {
                string displaced = fallbackSlots[1];
                fallbackSlots[1] = SlotTypes.Focus;
                focusSlots[focusIdx] = displaced;
            }
        }
    }

    // ── CommandSetsRow Construction ───────────────────────────────────────────

    private static List<CommandSetsRow> BuildCommandSetRows(
        Dictionary<int, List<string>> slotAssignment)
    {
        var rows = new List<CommandSetsRow>(MainCharacterCount);
        for (int charId = 0; charId < MainCharacterCount; charId++)
        {
            var row = new CommandSetsRow
            {
                Id = charId,
                Attack = CmdAttack,
                Defend = CmdDefend,
                Item = CmdItem,
                Change = CmdChange,
                AttackTrance = CmdAttack,
                DefendTrance = CmdDefend,
                ItemTrance = CmdItem,
                ChangeTrance = CmdChange
            };
            WriteCommandSlots(row, slotAssignment[charId]);
            rows.Add(row);
        }
        return rows;
    }

    private static void WriteCommandSlots(CommandSetsRow row, List<string> slots)
    {
        if (slots.Count == 0) { row.Regular1 = row.Trance1 = row.Regular2 = row.Trance2 = 0; return; }

        string slot1 = slots[0];
        if (slot1 == SlotTypes.BlueMage) { row.Regular1 = 8; row.Trance1 = 9; row.Regular2 = 24; row.Trance2 = 24; return; }
        if (slot1 == SlotTypes.Knight) { row.Regular1 = 30; row.Trance1 = 30; row.Regular2 = 31; row.Trance2 = 31; return; }

        (row.Regular1, row.Trance1) = GetSlotCommandIds(slot1);
        string slot2 = slots.Count > 1 ? slots[1] : "None";
        (row.Regular2, row.Trance2) = GetSlotCommandIds(slot2);
    }

    private static (int Regular, int Trance) GetSlotCommandIds(string slotName) => slotName switch
    {
        SlotTypes.Steal => (2, 2),
        SlotTypes.Skill => (25, 26),
        SlotTypes.SummonA => (16, 18),
        SlotTypes.SummonB => (20, 20),
        SlotTypes.WhtMagA => (17, 17),
        SlotTypes.WhtMagB => (19, 21),
        SlotTypes.BlkMag => (22, 23),
        SlotTypes.Focus => (13, 13),
        SlotTypes.Jump => (3, 12),
        SlotTypes.Dragon => (27, 27),
        SlotTypes.Flair => (28, 29),
        SlotTypes.Throw => (15, 15),
        "None" => (0, 0),
        _ => throw new ArgumentException($"Unknown slot type: '{slotName}'")
    };

    // ── Vanilla Fallbacks (RandomizeSpeciality = false) ───────────────────────

    private List<CommandSetsRow> BuildVanillaCommandSetRows() =>
        _vanillaCommandSets.Where(r => r.Id < MainCharacterCount).OrderBy(r => r.Id).Select(CloneRow).ToList();

    private static Dictionary<int, List<string>> BuildVanillaSlotAssignment() =>
        new()
        {
            [0] = new List<string> { SlotTypes.Steal, SlotTypes.Skill },
            [1] = new List<string> { SlotTypes.BlkMag, SlotTypes.Focus },
            [2] = new List<string> { SlotTypes.SummonA, SlotTypes.WhtMagA },
            [3] = new List<string> { SlotTypes.Knight },
            [4] = new List<string> { SlotTypes.Jump, SlotTypes.Dragon },
            [5] = new List<string> { SlotTypes.BlueMage },
            [6] = new List<string> { SlotTypes.WhtMagB, SlotTypes.SummonB },
            [7] = new List<string> { SlotTypes.Flair, SlotTypes.Throw },
        };

    // ── AbilityFeatures.txt Patching ──────────────────────────────────────────

    private static string PatchMagicSwordHardDisable(string text, int blkMagCharId)
    {
        const string blockMarker = ">CMD 31 Magic Sword";
        int blockStart = text.IndexOf(blockMarker, StringComparison.Ordinal);
        if (blockStart < 0) return text;

        int blockEnd = FindNextBlockStart(text, blockStart + blockMarker.Length);
        string before = text[..blockStart];
        string block = text[blockStart..blockEnd];
        string after = text[blockEnd..];

        string patched = Regex.Replace(
            block,
            @"!IsCharacterInParty\(CharacterId_\w+\)",
            $"!IsCharacterInParty({CharacterIdNames[blkMagCharId]})");

        return before + patched + after;
    }

    private static int FindNextBlockStart(string text, int searchFrom)
    {
        int lineStart = text.IndexOf('\n', searchFrom);
        while (lineStart >= 0 && lineStart < text.Length - 1)
        {
            if (text[lineStart + 1] == '>') return lineStart + 1;
            lineStart = text.IndexOf('\n', lineStart + 1);
        }
        return text.Length;
    }

    // ── Sub-Step 3: Abilities ─────────────────────────────────────────────────

    /// <summary>
    /// Completely rebuilds ability tables for main characters 0–7:
    ///
    ///   1. Extract vanilla AP values and SA pool from current (vanilla) tables.
    ///   2. Clear all main char ability tables.
    ///   3. Assign fixed AA packages per slot type.
    ///   4. Run summon pool split (11 RNG calls) — tracks Odin holder.
    ///   5. Run white magic pool split (23 RNG calls).
    ///   6. Apply T1 guaranteed SA pre-seeds (all modes).
    ///      Includes 1 RNG call for Guardian Mog summoner pick.
    ///   7. Apply T2 Recommended SA pre-seeds (Recommended mode only, 0 RNG).
    ///   8. Fisher-Yates shuffle remaining SA pool (N−1 RNG calls).
    ///   9. Round-robin SA pool assignment with duplicate-skip and gender filter.
    ///
    /// Guest character tables (IDs 8–15) are left untouched.
    /// </summary>
    private void RandomizeAbilitiesStep(
        Dictionary<int, List<CharacterAbilityRow>> abilities,
        Dictionary<int, List<string>> slotAssignment)
    {
        // 1. Extract vanilla data before any modification
        var apLookup = BuildVanillaApLookup(abilities);
        var saPool = BuildSaPool(abilities);

        // 2. Clear main char tables — rebuild from scratch
        for (int i = 0; i < MainCharacterCount; i++)
            abilities[i] = new List<CharacterAbilityRow>();

        // 3 + 4 + 5. Assign AA packages (includes pool splits)
        int odinHolderCharId = AssignAaPackages(abilities, slotAssignment, apLookup);

        // 6. T1 pre-seeds (all modes) — 1 RNG call for Guardian Mog pick
        ApplyT1PreSeeds(abilities, slotAssignment, saPool, odinHolderCharId);

        // 7. T2 pre-seeds (Recommended mode only — 0 RNG calls)
        if (_settings.Mode == RandomizerMode.Recommended)
            ApplyT2PreSeeds(abilities, slotAssignment, saPool);

        // 8. Fisher-Yates shuffle remaining SA pool
        for (int i = saPool.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (saPool[i], saPool[j]) = (saPool[j], saPool[i]);
        }

        // 9. Round-robin SA pool assignment
        AssignSaPool(abilities, saPool);
    }

    /// <summary>
    /// Builds a lookup from AbilityRef → AP using the first occurrence found
    /// when iterating character IDs 0–7 in ascending order.
    /// Called before the ability tables are cleared — reads from vanilla data.
    /// </summary>
    private static Dictionary<string, int> BuildVanillaApLookup(
        Dictionary<int, List<CharacterAbilityRow>> vanillaTables)
    {
        var lookup = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int charId = 0; charId < MainCharacterCount; charId++)
        {
            if (!vanillaTables.TryGetValue(charId, out var rows)) continue;
            foreach (var row in rows)
                lookup.TryAdd(row.AbilityRef, row.AP);
        }
        return lookup;
    }

    /// <summary>
    /// Collects all SA entries from characters 0–7, preserving duplicates,
    /// maintaining the order char 0 → char 7, file order within each char.
    /// Called before tables are cleared — reads from vanilla data.
    ///
    /// SA:49 Antibody appears in all 8 vanilla files → 8 copies in the pool.
    /// Class-unique SAs appear once.
    /// </summary>
    private static List<CharacterAbilityRow> BuildSaPool(
        Dictionary<int, List<CharacterAbilityRow>> vanillaTables)
    {
        var pool = new List<CharacterAbilityRow>();
        for (int charId = 0; charId < MainCharacterCount; charId++)
        {
            if (!vanillaTables.TryGetValue(charId, out var rows)) continue;
            foreach (var row in rows)
                if (row.AbilityRef.StartsWith("SA:", StringComparison.Ordinal))
                    pool.Add(new CharacterAbilityRow { AbilityRef = row.AbilityRef, AP = row.AP });
        }
        return pool;
    }

    /// <summary>
    /// Assigns fixed AA packages and runs both pool splits.
    /// Returns the character ID of whichever summoner received AA:58 Odin.
    ///
    /// RNG: 0 calls for fixed packages; 11 calls for summon split; 23 calls for white magic split.
    /// </summary>
    private int AssignAaPackages(
        Dictionary<int, List<CharacterAbilityRow>> abilities,
        Dictionary<int, List<string>> slotAssignment,
        Dictionary<string, int> apLookup)
    {
        // Fixed packages — iterate in char ID order for determinism
        foreach (var (charId, slots) in slotAssignment.OrderBy(kvp => kvp.Key))
        {
            foreach (string slotType in slots)
            {
                if (!AaPackages.TryGetValue(slotType, out var package)) continue;
                foreach (string abilityRef in package)
                {
                    int ap = apLookup.GetValueOrDefault(abilityRef, 30);
                    abilities[charId].Add(new CharacterAbilityRow { AbilityRef = abilityRef, AP = ap });
                }
            }
        }

        // Summon pool split — 11 RNG calls
        int odinHolderCharId = SplitSummonPool(abilities, slotAssignment, apLookup);

        // White magic pool split — 23 RNG calls
        SplitWhiteMagicPool(abilities, slotAssignment, apLookup);

        return odinHolderCharId;
    }

    /// <summary>
    /// Fisher-Yates shuffles the 12-summon pool (11 RNG calls).
    /// Summon-A holder receives indices 0–5; Summon-B holder receives 6–11.
    /// Returns the character ID of whichever holder received AA:58 Odin.
    /// </summary>
    private int SplitSummonPool(
        Dictionary<int, List<CharacterAbilityRow>> abilities,
        Dictionary<int, List<string>> slotAssignment,
        Dictionary<string, int> apLookup)
    {
        int summonACharId = slotAssignment.First(kvp => kvp.Value.Contains(SlotTypes.SummonA)).Key;
        int summonBCharId = slotAssignment.First(kvp => kvp.Value.Contains(SlotTypes.SummonB)).Key;

        var pool = new List<string>(SummonPool);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        int odinHolderCharId = summonACharId; // Default — overwritten when Odin is found
        for (int i = 0; i < pool.Count; i++)
        {
            string abilityRef = pool[i];
            int targetChar = i < 6 ? summonACharId : summonBCharId;
            int ap = apLookup.GetValueOrDefault(abilityRef, 30);
            abilities[targetChar].Add(new CharacterAbilityRow { AbilityRef = abilityRef, AP = ap });

            if (abilityRef == OdinAbilityRef)
                odinHolderCharId = targetChar;
        }

        return odinHolderCharId;
    }

    /// <summary>
    /// Fisher-Yates shuffles the 24-spell white magic pool (23 RNG calls).
    /// Wht Mag-A holder receives indices 0–11; Wht Mag-B holder receives 12–23.
    /// Each white magic spell appears on exactly one white mage per run.
    /// </summary>
    private void SplitWhiteMagicPool(
        Dictionary<int, List<CharacterAbilityRow>> abilities,
        Dictionary<int, List<string>> slotAssignment,
        Dictionary<string, int> apLookup)
    {
        int whtMagACharId = slotAssignment.First(kvp => kvp.Value.Contains(SlotTypes.WhtMagA)).Key;
        int whtMagBCharId = slotAssignment.First(kvp => kvp.Value.Contains(SlotTypes.WhtMagB)).Key;

        var pool = new List<string>(WhiteMagicPool);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }

        for (int i = 0; i < pool.Count; i++)
        {
            string abilityRef = pool[i];
            int targetChar = i < 12 ? whtMagACharId : whtMagBCharId;
            int ap = apLookup.GetValueOrDefault(abilityRef, 30);
            abilities[targetChar].Add(new CharacterAbilityRow { AbilityRef = abilityRef, AP = ap });
        }
    }

    /// <summary>
    /// Applies T1 guaranteed SA pre-seeds (all modes).
    /// Iterates <see cref="T1Seeds"/> in table order, then handles the two special cases:
    ///   SA:47 Guardian Mog — random pick between the two summoner holders (1 RNG call).
    ///   SA:60 Odin's Sword — always goes to <paramref name="odinHolderCharId"/>.
    ///
    /// <see cref="TryPreSeedFromPool"/> silently no-ops if a pool copy is unavailable,
    /// or if the target character already has that ability.
    /// </summary>
    private void ApplyT1PreSeeds(
        Dictionary<int, List<CharacterAbilityRow>> abilities,
        Dictionary<int, List<string>> slotAssignment,
        List<CharacterAbilityRow> saPool,
        int odinHolderCharId)
    {
        // Standard T1 seeds
        foreach (var (abilityRef, slotType) in T1Seeds)
        {
            var entry = slotAssignment.FirstOrDefault(kvp => kvp.Value.Contains(slotType));
            if (entry.Value == null) continue;
            TryPreSeedFromPool(saPool, abilityRef, abilities[entry.Key]);
        }

        // SA:47 Guardian Mog — random summoner pick (1 RNG call)
        int summonAHolder = slotAssignment.First(kvp => kvp.Value.Contains(SlotTypes.SummonA)).Key;
        int summonBHolder = slotAssignment.First(kvp => kvp.Value.Contains(SlotTypes.SummonB)).Key;
        int mogHolder = _rng.Next(2) == 0 ? summonAHolder : summonBHolder;
        TryPreSeedFromPool(saPool, GuardianMogAbilityRef, abilities[mogHolder]);

        // SA:60 Odin's Sword — always to whoever received Odin from the summon split
        TryPreSeedFromPool(saPool, OdinsSwordAbilityRef, abilities[odinHolderCharId]);
    }

    /// <summary>
    /// Applies T2 Recommended SA pre-seeds. Called only in Recommended mode.
    /// Zero RNG calls — all distribution is first-come-first-served by char ID.
    ///
    /// Magic T2 seeds are distributed to magic-type chars sorted by char ID.
    /// SA:33 Concentrate uses summoner priority (summoners sorted first).
    ///
    /// Physical T2 seeds are assigned directly to their target slot holders.
    /// SA:38 Protect Girls is skipped for female characters (DefaultCategory == 6).
    /// </summary>
    private void ApplyT2PreSeeds(
        Dictionary<int, List<CharacterAbilityRow>> abilities,
        Dictionary<int, List<string>> slotAssignment,
        List<CharacterAbilityRow> saPool)
    {
        // Build ordered magic char lists
        var magicCharIds = slotAssignment
            .Where(kvp => kvp.Value.Any(s => SlotTypes.MagicTypes.Contains(s)))
            .Select(kvp => kvp.Key)
            .OrderBy(id => id)
            .ToList();

        var summonerCharIds = slotAssignment
            .Where(kvp => kvp.Value.Any(s => SlotTypes.SummonerTypes.Contains(s)))
            .Select(kvp => kvp.Key)
            .OrderBy(id => id)
            .ToList();

        // Magic T2: first-come-first-served, summoner priority where applicable
        foreach (var (abilityRef, summonerPriority) in T2MagicSeeds)
        {
            IEnumerable<int> ordered = summonerPriority
                ? summonerCharIds.Concat(magicCharIds.Except(summonerCharIds))
                : magicCharIds;

            foreach (int charId in ordered)
                TryPreSeedFromPool(saPool, abilityRef, abilities[charId]);
            // TryPreSeedFromPool returns false when pool copies are exhausted — natural stop
        }

        // Physical T2: direct slot-to-holder assignment
        var categories = _characterParameters
            .Where(r => r.Id < MainCharacterCount)
            .ToDictionary(r => r.Id, r => r.DefaultCategory);

        foreach (var (abilityRef, targetSlot) in T2PhysicalSeeds)
        {
            var entry = slotAssignment.FirstOrDefault(kvp => kvp.Value.Contains(targetSlot));
            if (entry.Value == null) continue;

            int holderCharId = entry.Key;

            // Gender filter — SA:38 is useless on female characters
            if (abilityRef == ProtectGirlsAbilityRef
                && categories.TryGetValue(holderCharId, out byte cat)
                && cat == FemaleCategoryValue)
                continue;

            TryPreSeedFromPool(saPool, abilityRef, abilities[holderCharId]);
        }
    }

    /// <summary>
    /// Round-robin assigns remaining SA pool entries across characters 0–7.
    /// Each pass: character 0 → 1 → ... → 7. Each character receives one SA per pass
    /// (the first pool entry they don't already have and that passes the gender filter).
    /// Continues until pool is exhausted or no character can accept any remaining entry.
    ///
    /// This correctly handles universals: SA:49 Antibody with 8 copies will eventually
    /// reach all 8 characters since each scan finds the first unassigned copy for each char.
    /// Excess copies (more than 8 of any universal) are silently discarded.
    /// </summary>
    private void AssignSaPool(
        Dictionary<int, List<CharacterAbilityRow>> abilities,
        List<CharacterAbilityRow> saPool)
    {
        // Pre-populate assigned sets from T1/T2 pre-seeds
        var assigned = new Dictionary<int, HashSet<string>>();
        for (int i = 0; i < MainCharacterCount; i++)
            assigned[i] = new HashSet<string>(
                abilities[i].Select(r => r.AbilityRef), StringComparer.Ordinal);

        var categories = _characterParameters
            .Where(r => r.Id < MainCharacterCount)
            .ToDictionary(r => r.Id, r => r.DefaultCategory);

        var pool = new List<CharacterAbilityRow>(saPool); // work on a copy

        bool progressMade = true;
        while (pool.Count > 0 && progressMade)
        {
            progressMade = false;
            for (int charId = 0; charId < MainCharacterCount && pool.Count > 0; charId++)
            {
                // Find first valid pool entry for this character
                for (int i = 0; i < pool.Count; i++)
                {
                    var entry = pool[i];

                    if (assigned[charId].Contains(entry.AbilityRef)) continue;

                    if (entry.AbilityRef == ProtectGirlsAbilityRef
                        && categories.TryGetValue(charId, out byte cat)
                        && cat == FemaleCategoryValue)
                        continue;

                    abilities[charId].Add(
                        new CharacterAbilityRow { AbilityRef = entry.AbilityRef, AP = entry.AP });
                    assigned[charId].Add(entry.AbilityRef);
                    pool.RemoveAt(i);
                    progressMade = true;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Removes the first pool entry matching <paramref name="abilityRef"/> and appends it
    /// to <paramref name="target"/>. Returns false (no-op) if:
    ///   - no matching entry exists in the pool (all copies already consumed), or
    ///   - <paramref name="target"/> already contains this AbilityRef (duplicate guard).
    /// </summary>
    private static bool TryPreSeedFromPool(
        List<CharacterAbilityRow> pool,
        string abilityRef,
        List<CharacterAbilityRow> target)
    {
        if (target.Any(r => r.AbilityRef == abilityRef)) return false;

        int idx = pool.FindIndex(r => r.AbilityRef == abilityRef);
        if (idx < 0) return false;

        target.Add(pool[idx]);
        pool.RemoveAt(idx);
        return true;
    }

    // ── Sub-Step 4: Equipment ─────────────────────────────────────────────────

    /// <summary>
    /// Shuffles <c>DefaultEquipmentSet</c> values for main characters 0–7 as a
    /// Fisher-Yates permutation. Every vanilla equipment set still appears exactly
    /// once — only which character starts with it changes.
    ///
    /// Guest characters (IDs 8–15) are never touched.
    ///
    /// RNG: 7 calls (Fisher-Yates on 8 elements: i from 7 down to 1).
    /// </summary>
    private void RandomizeEquipmentStep(List<CharacterParametersRow> charParams)
    {
        var mainRows = charParams
            .Where(r => r.Id < MainCharacterCount)
            .OrderBy(r => r.Id)
            .ToList();

        var equipValues = mainRows.Select(r => r.DefaultEquipmentSet).ToList();
        for (int i = equipValues.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (equipValues[i], equipValues[j]) = (equipValues[j], equipValues[i]);
        }

        for (int i = 0; i < mainRows.Count; i++)
            mainRows[i].DefaultEquipmentSet = equipValues[i];
    }

    // ── Shared Helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Fisher-Yates column shuffle using the shared RNG.
    /// Generic over row and value type — handles byte, int, uint without duplication.
    /// </summary>
    private void ShuffleColumn<TRow, TVal>(
        List<TRow> rows,
        Func<TRow, TVal> getter,
        Action<TRow, TVal> setter)
    {
        var values = rows.Select(getter).ToList();
        for (int i = values.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }
        for (int i = 0; i < rows.Count; i++)
            setter(rows[i], values[i]);
    }

    // ── Clone Helpers ─────────────────────────────────────────────────────────

    private static BaseStatsRow CloneRow(BaseStatsRow r) => new()
    {
        Comment = r.Comment,
        Id = r.Id,
        Dexterity = r.Dexterity,
        Strength = r.Strength,
        Magic = r.Magic,
        Will = r.Will,
        Gems = r.Gems
    };

    private static CharacterParametersRow CloneRow(CharacterParametersRow r) => new()
    {
        Id = r.Id,
        DefaultRow = r.DefaultRow,
        DefaultWinPose = r.DefaultWinPose,
        DefaultCategory = r.DefaultCategory,
        DefaultCommandSet = r.DefaultCommandSet,
        DefaultEquipmentSet = r.DefaultEquipmentSet,
        BattleParameterFormula = r.BattleParameterFormula,
        NameKeyword = r.NameKeyword
    };

    private static CommandSetsRow CloneRow(CommandSetsRow r) => new()
    {
        Id = r.Id,
        Attack = r.Attack,
        Defend = r.Defend,
        Regular1 = r.Regular1,
        Regular2 = r.Regular2,
        Item = r.Item,
        Change = r.Change,
        AttackTrance = r.AttackTrance,
        DefendTrance = r.DefendTrance,
        Trance1 = r.Trance1,
        Trance2 = r.Trance2,
        ItemTrance = r.ItemTrance,
        ChangeTrance = r.ChangeTrance
    };

    private static CharacterAbilityRow CloneRow(CharacterAbilityRow r) => new()
    {
        AbilityRef = r.AbilityRef,
        AP = r.AP
    };
}