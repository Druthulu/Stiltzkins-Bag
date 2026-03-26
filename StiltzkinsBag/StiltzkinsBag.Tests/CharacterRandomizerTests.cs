using System.Collections.Generic;
using System.Linq;
using StiltzkinsBag.Models;
using StiltzkinsBag.Models.Csv;
using StiltzkinsBag.Randomizers;
using Xunit;

namespace StiltzkinsBag.Tests;

/// <summary>
/// Tests for <see cref="CharacterRandomizer"/> covering all four sub-steps:
///   1. Base stats  — column shuffle (Recommended) and ranged random (Chaos)
///   2. Speciality  — slot assignment (stat-biased and random paths), CommandSets output,
///                    AbilityFeatures.txt patch, Focus constraint, Zidane Dex bias
///   3. Abilities   — AA package assignment, summon/white magic pool splits,
///                    T1/T2 pre-seeding, SA pool shuffle and round-robin assignment
///   4. Equipment   — DefaultEquipmentSet shuffle
///
/// All determinism tests run the same seed+settings twice and assert identical output.
/// </summary>
public class CharacterRandomizerTests
{
    // ── Fixture Helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Builds a minimal vanilla BaseStatsRow list matching the real game values.
    /// </summary>
    private static List<BaseStatsRow> VanillaBaseStats() =>
        new()
        {
            new() { Comment = "Zidane",  Id = 0,  Dexterity = 23, Strength = 21, Magic = 18, Will = 23, Gems = 18 },
            new() { Comment = "Vivi",    Id = 1,  Dexterity = 16, Strength = 12, Magic = 24, Will = 19, Gems = 14 },
            new() { Comment = "Garnet",  Id = 2,  Dexterity = 21, Strength = 14, Magic = 23, Will = 17, Gems = 14 },
            new() { Comment = "Steiner", Id = 3,  Dexterity = 18, Strength = 24, Magic = 12, Will = 21, Gems = 17 },
            new() { Comment = "Freya",   Id = 4,  Dexterity = 20, Strength = 20, Magic = 16, Will = 22, Gems = 18 },
            new() { Comment = "Quina",   Id = 5,  Dexterity = 14, Strength = 18, Magic = 20, Will = 11, Gems = 15 },
            new() { Comment = "Eiko",    Id = 6,  Dexterity = 19, Strength = 13, Magic = 21, Will = 18, Gems = 12 },
            new() { Comment = "Amarant", Id = 7,  Dexterity = 22, Strength = 22, Magic = 13, Will = 15, Gems = 18 },
            new() { Comment = "Cinna",   Id = 8,  Dexterity = 19, Strength = 15, Magic = 16, Will = 19, Gems = 8  },
            new() { Comment = "Marcus",  Id = 9,  Dexterity = 20, Strength = 18, Magic = 11, Will = 21, Gems = 8  },
            new() { Comment = "Blank",   Id = 10, Dexterity = 23, Strength = 21, Magic = 12, Will = 21, Gems = 12 },
            new() { Comment = "Beatrix", Id = 11, Dexterity = 24, Strength = 21, Magic = 19, Will = 23, Gems = 10 },
        };

    /// <summary>
    /// Builds vanilla CharacterParameters rows from the real CSV data.
    /// DefaultCategory = 6 for Garnet, Freya, Eiko (female); 5 or 9 or 21/22 for others.
    /// </summary>
    private static List<CharacterParametersRow> VanillaCharParams() =>
        new()
        {
            new() { Id = 0,  DefaultRow = true,  DefaultWinPose = true,  DefaultCategory = 9,  DefaultCommandSet = 0,  DefaultEquipmentSet = 0,  BattleParameterFormula = "WeaponShape == 1 ? 0 : 1", NameKeyword = "ZDNE" },
            new() { Id = 1,  DefaultRow = false, DefaultWinPose = true,  DefaultCategory = 5,  DefaultCommandSet = 1,  DefaultEquipmentSet = 1,  BattleParameterFormula = "2",                         NameKeyword = "VIVI" },
            new() { Id = 2,  DefaultRow = false, DefaultWinPose = true,  DefaultCategory = 6,  DefaultCommandSet = 2,  DefaultEquipmentSet = 2,  BattleParameterFormula = "ScenarioCounter < 10300 ? (WeaponShape == 7 ? 4 : 3) : (WeaponShape == 7 ? 6 : 5)", NameKeyword = "DGGR" },
            new() { Id = 3,  DefaultRow = true,  DefaultWinPose = true,  DefaultCategory = 5,  DefaultCommandSet = 3,  DefaultEquipmentSet = 3,  BattleParameterFormula = "7",                         NameKeyword = "STNR" },
            new() { Id = 4,  DefaultRow = false, DefaultWinPose = true,  DefaultCategory = 6,  DefaultCommandSet = 4,  DefaultEquipmentSet = 4,  BattleParameterFormula = "12",                        NameKeyword = "FRYA" },
            new() { Id = 5,  DefaultRow = true,  DefaultWinPose = true,  DefaultCategory = 5,  DefaultCommandSet = 5,  DefaultEquipmentSet = 5,  BattleParameterFormula = "9",                         NameKeyword = "QUIN" },
            new() { Id = 6,  DefaultRow = true,  DefaultWinPose = true,  DefaultCategory = 6,  DefaultCommandSet = 6,  DefaultEquipmentSet = 6,  BattleParameterFormula = "WeaponShape == 7 ? 11 : 10", NameKeyword = "EIKO" },
            new() { Id = 7,  DefaultRow = true,  DefaultWinPose = true,  DefaultCategory = 5,  DefaultCommandSet = 7,  DefaultEquipmentSet = 7,  BattleParameterFormula = "13",                        NameKeyword = "AMRT" },
            new() { Id = 8,  DefaultRow = true,  DefaultWinPose = true,  DefaultCategory = 21, DefaultCommandSet = 8,  DefaultEquipmentSet = 8,  BattleParameterFormula = "14",                        NameKeyword = "CINA" },
            new() { Id = 9,  DefaultRow = true,  DefaultWinPose = true,  DefaultCategory = 21, DefaultCommandSet = 10, DefaultEquipmentSet = 9,  BattleParameterFormula = "15",                        NameKeyword = "MRCS" },
            new() { Id = 10, DefaultRow = true,  DefaultWinPose = true,  DefaultCategory = 21, DefaultCommandSet = 12, DefaultEquipmentSet = 10, BattleParameterFormula = "(1500 <= ScenarioCounter && ScenarioCounter < 1600) ? 17 : 16", NameKeyword = "BLNK" },
            new() { Id = 11, DefaultRow = true,  DefaultWinPose = true,  DefaultCategory = 22, DefaultCommandSet = 14, DefaultEquipmentSet = 11, BattleParameterFormula = "18",                        NameKeyword = "BTRX" },
        };

    /// <summary>
    /// Builds vanilla CommandSets rows 0–7 matching the real CSV data.
    /// </summary>
    private static List<CommandSetsRow> VanillaCommandSets() =>
        new()
        {
            new() { Id = 0, Attack = 1, Defend = 4, Regular1 = 2,  Regular2 = 25, Item = 14, Change = 7, AttackTrance = 1, DefendTrance = 4, Trance1 = 2,  Trance2 = 26, ItemTrance = 14, ChangeTrance = 7 },
            new() { Id = 1, Attack = 1, Defend = 4, Regular1 = 22, Regular2 = 13, Item = 14, Change = 7, AttackTrance = 1, DefendTrance = 4, Trance1 = 23, Trance2 = 13, ItemTrance = 14, ChangeTrance = 7 },
            new() { Id = 2, Attack = 1, Defend = 4, Regular1 = 16, Regular2 = 17, Item = 14, Change = 7, AttackTrance = 1, DefendTrance = 4, Trance1 = 18, Trance2 = 17, ItemTrance = 14, ChangeTrance = 7 },
            new() { Id = 3, Attack = 1, Defend = 4, Regular1 = 30, Regular2 = 31, Item = 14, Change = 7, AttackTrance = 1, DefendTrance = 4, Trance1 = 30, Trance2 = 31, ItemTrance = 14, ChangeTrance = 7 },
            new() { Id = 4, Attack = 1, Defend = 4, Regular1 = 3,  Regular2 = 27, Item = 14, Change = 7, AttackTrance = 1, DefendTrance = 4, Trance1 = 12, Trance2 = 27, ItemTrance = 14, ChangeTrance = 7 },
            new() { Id = 5, Attack = 1, Defend = 4, Regular1 = 8,  Regular2 = 24, Item = 14, Change = 7, AttackTrance = 1, DefendTrance = 4, Trance1 = 9,  Trance2 = 24, ItemTrance = 14, ChangeTrance = 7 },
            new() { Id = 6, Attack = 1, Defend = 4, Regular1 = 19, Regular2 = 20, Item = 14, Change = 7, AttackTrance = 1, DefendTrance = 4, Trance1 = 21, Trance2 = 20, ItemTrance = 14, ChangeTrance = 7 },
            new() { Id = 7, Attack = 1, Defend = 4, Regular1 = 28, Regular2 = 15, Item = 14, Change = 7, AttackTrance = 1, DefendTrance = 4, Trance1 = 29, Trance2 = 15, ItemTrance = 14, ChangeTrance = 7 },
        };

    /// <summary>
    /// Builds a minimal ability table for the 8 main characters.
    /// Contains only a representative subset of vanilla abilities, enough for:
    ///   - SA pool construction
    ///   - AP lookup
    ///   - T1/T2 pre-seed retrieval
    ///   - Odin tracking (AA:58 present in Garnet's table)
    /// </summary>
    private static Dictionary<int, List<CharacterAbilityRow>> VanillaAbilityTables()
    {
        return new()
        {
            // Zidane (0) — Thief abilities + universals + T1 candidates
            [0] = new()
            {
                new() { AbilityRef = "AA:101", AP = 40 }, new() { AbilityRef = "AA:102", AP = 40 },
                new() { AbilityRef = "AA:103", AP = 40 }, new() { AbilityRef = "AA:104", AP = 35 },
                new() { AbilityRef = "AA:105", AP = 50 }, new() { AbilityRef = "AA:106", AP = 55 },
                new() { AbilityRef = "AA:107", AP = 85 }, new() { AbilityRef = "AA:108", AP = 100 },
                new() { AbilityRef = "SA:62",  AP = 40 }, new() { AbilityRef = "SA:61",  AP = 65 },
                new() { AbilityRef = "SA:22",  AP = 50 }, new() { AbilityRef = "SA:23",  AP = 40 },
                new() { AbilityRef = "SA:46",  AP = 45 }, new() { AbilityRef = "SA:38",  AP = 35 },
                new() { AbilityRef = "SA:0",   AP = 95 }, new() { AbilityRef = "SA:1",   AP = 20 },
                new() { AbilityRef = "SA:2",   AP = 55 }, new() { AbilityRef = "SA:3",   AP = 25 },
                new() { AbilityRef = "SA:4",   AP = 130 }, new() { AbilityRef = "SA:35",  AP = 35 },
                new() { AbilityRef = "SA:40",  AP = 25 }, new() { AbilityRef = "SA:43",  AP = 75 },
                new() { AbilityRef = "SA:44",  AP = 95 }, new() { AbilityRef = "SA:48",  AP = 30 },
                new() { AbilityRef = "SA:49",  AP = 20 }, new() { AbilityRef = "SA:53",  AP = 35 },
                new() { AbilityRef = "SA:56",  AP = 30 }, new() { AbilityRef = "SA:57",  AP = 30 },
                new() { AbilityRef = "SA:58",  AP = 25 }, new() { AbilityRef = "SA:25",  AP = 35 },
            },
            // Vivi (1) — Black Magic + T2 magic candidates
            [1] = new()
            {
                new() { AbilityRef = "SA:31",  AP = 110 }, new() { AbilityRef = "SA:32",  AP = 115 },
                new() { AbilityRef = "SA:30",  AP = 55  }, new() { AbilityRef = "SA:8",   AP = 30  },
                new() { AbilityRef = "SA:34",  AP = 140 }, new() { AbilityRef = "SA:54",  AP = 90  },
                new() { AbilityRef = "SA:24",  AP = 20  }, new() { AbilityRef = "SA:25",  AP = 25  },
                new() { AbilityRef = "SA:0",   AP = 70  }, new() { AbilityRef = "SA:1",   AP = 20  },
                new() { AbilityRef = "SA:2",   AP = 55  }, new() { AbilityRef = "SA:3",   AP = 30  },
                new() { AbilityRef = "SA:4",   AP = 70  }, new() { AbilityRef = "SA:35",  AP = 25  },
                new() { AbilityRef = "SA:40",  AP = 15  }, new() { AbilityRef = "SA:43",  AP = 30  },
                new() { AbilityRef = "SA:44",  AP = 55  }, new() { AbilityRef = "SA:48",  AP = 25  },
                new() { AbilityRef = "SA:49",  AP = 30  }, new() { AbilityRef = "SA:53",  AP = 25  },
                new() { AbilityRef = "SA:56",  AP = 10  }, new() { AbilityRef = "SA:57",  AP = 35  },
                new() { AbilityRef = "SA:58",  AP = 15  },
            },
            // Garnet (2) — Summoner/White magic + Odin (AA:58) + summoner SAs
            [2] = new()
            {
                new() { AbilityRef = "AA:49",  AP = 20  }, new() { AbilityRef = "AA:51",  AP = 35  },
                new() { AbilityRef = "AA:53",  AP = 30  }, new() { AbilityRef = "AA:55",  AP = 30  },
                new() { AbilityRef = "AA:58",  AP = 30  }, new() { AbilityRef = "AA:60",  AP = 40  },
                new() { AbilityRef = "AA:62",  AP = 80  }, new() { AbilityRef = "AA:64",  AP = 100 },
                new() { AbilityRef = "SA:59",  AP = 190 }, new() { AbilityRef = "SA:60",  AP = 50  },
                new() { AbilityRef = "SA:33",  AP = 80  }, new() { AbilityRef = "SA:8",   AP = 45  },
                new() { AbilityRef = "SA:34",  AP = 125 }, new() { AbilityRef = "SA:24",  AP = 30  },
                new() { AbilityRef = "SA:0",   AP = 75  }, new() { AbilityRef = "SA:1",   AP = 20  },
                new() { AbilityRef = "SA:2",   AP = 55  }, new() { AbilityRef = "SA:3",   AP = 35  },
                new() { AbilityRef = "SA:4",   AP = 105 }, new() { AbilityRef = "SA:35",  AP = 30  },
                new() { AbilityRef = "SA:40",  AP = 25  }, new() { AbilityRef = "SA:43",  AP = 50  },
                new() { AbilityRef = "SA:44",  AP = 60  }, new() { AbilityRef = "SA:48",  AP = 25  },
                new() { AbilityRef = "SA:49",  AP = 15  }, new() { AbilityRef = "SA:53",  AP = 40  },
                new() { AbilityRef = "SA:56",  AP = 20  }, new() { AbilityRef = "SA:57",  AP = 30  },
                new() { AbilityRef = "SA:58",  AP = 25  },
            },
            // Steiner (3) — Knight abilities
            [3] = new()
            {
                new() { AbilityRef = "SA:37",  AP = 20  }, new() { AbilityRef = "SA:6",   AP = 60  },
                new() { AbilityRef = "SA:9",   AP = 40  }, new() { AbilityRef = "SA:12",  AP = 50  },
                new() { AbilityRef = "SA:36",  AP = 100 }, new() { AbilityRef = "SA:25",  AP = 50  },
                new() { AbilityRef = "SA:0",   AP = 95  }, new() { AbilityRef = "SA:1",   AP = 20  },
                new() { AbilityRef = "SA:2",   AP = 65  }, new() { AbilityRef = "SA:3",   AP = 75  },
                new() { AbilityRef = "SA:4",   AP = 155 }, new() { AbilityRef = "SA:35",  AP = 35  },
                new() { AbilityRef = "SA:40",  AP = 35  }, new() { AbilityRef = "SA:43",  AP = 50  },
                new() { AbilityRef = "SA:44",  AP = 70  }, new() { AbilityRef = "SA:48",  AP = 25  },
                new() { AbilityRef = "SA:49",  AP = 35  }, new() { AbilityRef = "SA:53",  AP = 30  },
                new() { AbilityRef = "SA:56",  AP = 20  }, new() { AbilityRef = "SA:57",  AP = 35  },
                new() { AbilityRef = "SA:58",  AP = 30  },
            },
            // Freya (4) — Dragoon
            [4] = new()
            {
                new() { AbilityRef = "SA:21",  AP = 75  }, new() { AbilityRef = "SA:42",  AP = 95  },
                new() { AbilityRef = "SA:17",  AP = 70  }, new() { AbilityRef = "SA:6",   AP = 75  },
                new() { AbilityRef = "SA:9",   AP = 30  }, new() { AbilityRef = "SA:36",  AP = 85  },
                new() { AbilityRef = "SA:0",   AP = 95  }, new() { AbilityRef = "SA:1",   AP = 20  },
                new() { AbilityRef = "SA:2",   AP = 75  }, new() { AbilityRef = "SA:3",   AP = 25  },
                new() { AbilityRef = "SA:4",   AP = 125 }, new() { AbilityRef = "SA:35",  AP = 20  },
                new() { AbilityRef = "SA:40",  AP = 20  }, new() { AbilityRef = "SA:43",  AP = 40  },
                new() { AbilityRef = "SA:44",  AP = 65  }, new() { AbilityRef = "SA:48",  AP = 30  },
                new() { AbilityRef = "SA:49",  AP = 15  }, new() { AbilityRef = "SA:53",  AP = 30  },
                new() { AbilityRef = "SA:56",  AP = 30  }, new() { AbilityRef = "SA:57",  AP = 25  },
                new() { AbilityRef = "SA:58",  AP = 35  }, new() { AbilityRef = "SA:25",  AP = 25  },
            },
            // Quina (5) — Blue Mage
            [5] = new()
            {
                new() { AbilityRef = "SA:55",  AP = 80  }, new() { AbilityRef = "SA:34",  AP = 90  },
                new() { AbilityRef = "SA:24",  AP = 60  }, new() { AbilityRef = "SA:45",  AP = 100 },
                new() { AbilityRef = "SA:0",   AP = 75  }, new() { AbilityRef = "SA:1",   AP = 40  },
                new() { AbilityRef = "SA:2",   AP = 70  }, new() { AbilityRef = "SA:3",   AP = 30  },
                new() { AbilityRef = "SA:4",   AP = 165 }, new() { AbilityRef = "SA:35",  AP = 250 },
                new() { AbilityRef = "SA:40",  AP = 20  }, new() { AbilityRef = "SA:43",  AP = 60  },
                new() { AbilityRef = "SA:44",  AP = 40  }, new() { AbilityRef = "SA:48",  AP = 40  },
                new() { AbilityRef = "SA:49",  AP = 20  }, new() { AbilityRef = "SA:53",  AP = 35  },
                new() { AbilityRef = "SA:56",  AP = 30  }, new() { AbilityRef = "SA:57",  AP = 20  },
                new() { AbilityRef = "SA:58",  AP = 25  }, new() { AbilityRef = "SA:25",  AP = 35  },
            },
            // Eiko (6) — Summoner 2 / White Mage 2
            [6] = new()
            {
                new() { AbilityRef = "AA:66",  AP = 55  }, new() { AbilityRef = "AA:68",  AP = 35  },
                new() { AbilityRef = "AA:72",  AP = 40  }, new() { AbilityRef = "AA:74",  AP = 120 },
                new() { AbilityRef = "SA:59",  AP = 150 }, new() { AbilityRef = "SA:47",  AP = 30  },
                new() { AbilityRef = "SA:33",  AP = 90  }, new() { AbilityRef = "SA:34",  AP = 120 },
                new() { AbilityRef = "SA:30",  AP = 55  }, new() { AbilityRef = "SA:8",   AP = 50  },
                new() { AbilityRef = "SA:24",  AP = 20  }, new() { AbilityRef = "SA:0",   AP = 70  },
                new() { AbilityRef = "SA:1",   AP = 25  }, new() { AbilityRef = "SA:2",   AP = 65  },
                new() { AbilityRef = "SA:3",   AP = 35  }, new() { AbilityRef = "SA:4",   AP = 100 },
                new() { AbilityRef = "SA:35",  AP = 30  }, new() { AbilityRef = "SA:40",  AP = 20  },
                new() { AbilityRef = "SA:43",  AP = 65  }, new() { AbilityRef = "SA:44",  AP = 55  },
                new() { AbilityRef = "SA:48",  AP = 25  }, new() { AbilityRef = "SA:49",  AP = 20  },
                new() { AbilityRef = "SA:53",  AP = 35  }, new() { AbilityRef = "SA:56",  AP = 30  },
                new() { AbilityRef = "SA:57",  AP = 15  }, new() { AbilityRef = "SA:58",  AP = 15  },
            },
            // Amarant (7) — Flair/Monk
            [7] = new()
            {
                new() { AbilityRef = "SA:28",  AP = 125 }, new() { AbilityRef = "SA:29",  AP = 30  },
                new() { AbilityRef = "SA:6",   AP = 40  }, new() { AbilityRef = "SA:36",  AP = 240 },
                new() { AbilityRef = "SA:52",  AP = 75  }, new() { AbilityRef = "SA:54",  AP = 170 },
                new() { AbilityRef = "SA:24",  AP = 40  }, new() { AbilityRef = "SA:25",  AP = 20  },
                new() { AbilityRef = "SA:0",   AP = 85  }, new() { AbilityRef = "SA:1",   AP = 35  },
                new() { AbilityRef = "SA:2",   AP = 70  }, new() { AbilityRef = "SA:3",   AP = 35  },
                new() { AbilityRef = "SA:4",   AP = 140 }, new() { AbilityRef = "SA:35",  AP = 60  },
                new() { AbilityRef = "SA:40",  AP = 30  }, new() { AbilityRef = "SA:43",  AP = 50  },
                new() { AbilityRef = "SA:44",  AP = 80  }, new() { AbilityRef = "SA:48",  AP = 20  },
                new() { AbilityRef = "SA:49",  AP = 25  }, new() { AbilityRef = "SA:53",  AP = 15  },
                new() { AbilityRef = "SA:56",  AP = 30  }, new() { AbilityRef = "SA:57",  AP = 20  },
                new() { AbilityRef = "SA:58",  AP = 30  },
            },
        };
    }

    private const string VanillaAbilityFeaturesText = @">SA 0 Auto-Reflect
>CMD 31 Magic Sword
[code=HardDisable] !IsCharacterInParty(CharacterId_Vivi) [/code]
>SA 1 Auto-Float";

    private static CharacterRandomizer MakeRandomizer(
        string seedString,
        bool randomizeStats = false,
        bool randomizeSpeciality = false,
        bool randomizeAbilities = false,
        bool randomizeEquipment = false,
        RandomizerMode mode = RandomizerMode.Recommended)
    {
        var settings = new Settings
        {
            SeedString = seedString,
            SeedInt = SeedEngine.Resolve(seedString),
            Mode = mode,
            RandomizeBaseStats = randomizeStats,
            RandomizeSpeciality = randomizeSpeciality,
            RandomizeAbilities = randomizeAbilities,
            RandomizeEquipment = randomizeEquipment,
        };
        var rng = SeedEngine.CreateRandom(settings.SeedInt);

        return new CharacterRandomizer(
            rng,
            settings,
            VanillaBaseStats(),
            VanillaCharParams(),
            VanillaCommandSets(),
            VanillaAbilityTables(),
            VanillaAbilityFeaturesText);
    }

    private static CharacterRandomizerResult RunTwice(
        string seedString,
        bool randomizeStats = false,
        bool randomizeSpeciality = false,
        bool randomizeAbilities = false,
        bool randomizeEquipment = false,
        RandomizerMode mode = RandomizerMode.Recommended)
    {
        var r1 = MakeRandomizer(seedString, randomizeStats, randomizeSpeciality,
                                randomizeAbilities, randomizeEquipment, mode).Randomize();
        var r2 = MakeRandomizer(seedString, randomizeStats, randomizeSpeciality,
                                randomizeAbilities, randomizeEquipment, mode).Randomize();
        return r1; // Caller also runs r2 assertions
    }

    // ── Sub-Step 1: Base Stats ─────────────────────────────────────────────────

    [Fact]
    public void BaseStats_Disabled_LeavesStatValuesUnchanged()
    {
        var result = MakeRandomizer("test").Randomize();
        var vanilla = VanillaBaseStats();

        foreach (var row in result.BaseStats)
        {
            var v = vanilla.First(r => r.Id == row.Id);
            Assert.Equal(v.Dexterity, row.Dexterity);
            Assert.Equal(v.Strength, row.Strength);
            Assert.Equal(v.Magic, row.Magic);
            Assert.Equal(v.Will, row.Will);
        }
    }

    [Fact]
    public void BaseStats_Recommended_PreservesAllStatValues()
    {
        var result = MakeRandomizer("seed1", randomizeStats: true).Randomize();
        var vanilla = VanillaBaseStats();

        // Every vanilla stat value must still appear exactly once per column
        Assert.Equal(
            vanilla.Select(r => (int)r.Dexterity).OrderBy(x => x),
            result.BaseStats.Select(r => (int)r.Dexterity).OrderBy(x => x));
        Assert.Equal(
            vanilla.Select(r => (int)r.Strength).OrderBy(x => x),
            result.BaseStats.Select(r => (int)r.Strength).OrderBy(x => x));
        Assert.Equal(
            vanilla.Select(r => (int)r.Magic).OrderBy(x => x),
            result.BaseStats.Select(r => (int)r.Magic).OrderBy(x => x));
        Assert.Equal(
            vanilla.Select(r => (int)r.Will).OrderBy(x => x),
            result.BaseStats.Select(r => (int)r.Will).OrderBy(x => x));
    }

    [Fact]
    public void BaseStats_Recommended_ZidaneHasMaxDex()
    {
        var result = MakeRandomizer("dexbias", randomizeStats: true).Randomize();
        var mainRows = result.BaseStats.Where(r => r.Id < 8).ToList();
        var maxDex = mainRows.Max(r => r.Dexterity);
        var zidaneDex = result.BaseStats.First(r => r.Id == 0).Dexterity;

        Assert.Equal(maxDex, zidaneDex);
    }

    [Fact]
    public void BaseStats_Chaos_AllValuesInVanillaRange()
    {
        var vanilla = VanillaBaseStats();
        var result = MakeRandomizer("chaos1", randomizeStats: true,
                                     mode: RandomizerMode.Chaos).Randomize();

        int dexMin = vanilla.Min(r => r.Dexterity), dexMax = vanilla.Max(r => r.Dexterity);
        int magMin = vanilla.Min(r => r.Magic), magMax = vanilla.Max(r => r.Magic);

        foreach (var row in result.BaseStats)
        {
            Assert.InRange(row.Dexterity, (byte)dexMin, (byte)dexMax);
            Assert.InRange(row.Magic, (byte)magMin, (byte)magMax);
        }
    }

    [Fact]
    public void BaseStats_Deterministic_SameSeedSameOutput()
    {
        var r1 = MakeRandomizer("det1", randomizeStats: true).Randomize();
        var r2 = MakeRandomizer("det1", randomizeStats: true).Randomize();

        for (int i = 0; i < r1.BaseStats.Count; i++)
        {
            Assert.Equal(r1.BaseStats[i].Dexterity, r2.BaseStats[i].Dexterity);
            Assert.Equal(r1.BaseStats[i].Magic, r2.BaseStats[i].Magic);
            Assert.Equal(r1.BaseStats[i].Will, r2.BaseStats[i].Will);
        }
    }

    [Fact]
    public void BaseStats_GuestCharactersAlwaysPresent()
    {
        var result = MakeRandomizer("guests", randomizeStats: true).Randomize();
        // IDs 8–11 (Cinna, Marcus, Blank, Beatrix) must be in output
        for (int id = 8; id <= 11; id++)
            Assert.Contains(result.BaseStats, r => r.Id == id);
    }

    // ── Sub-Step 2: Speciality ─────────────────────────────────────────────────

    [Fact]
    public void Speciality_Disabled_ZidaneKeepsVanillaCommandIds()
    {
        var result = MakeRandomizer("test").Randomize();
        var zidane = result.CommandSetRows.First(r => r.Id == 0);

        Assert.Equal(2, zidane.Regular1); // Steal
        Assert.Equal(25, zidane.Regular2); // Skill
    }

    [Fact]
    public void Speciality_ZidaneAlwaysHasSteelAndSkill()
    {
        var result = MakeRandomizer("zlock", randomizeSpeciality: true).Randomize();
        var zidane = result.CommandSetRows.First(r => r.Id == 0);

        Assert.Equal(2, zidane.Regular1);  // Steal
        Assert.Equal(25, zidane.Regular2);  // Skill
        Assert.Equal(2, zidane.Trance1);   // Steal (trance)
        Assert.Equal(26, zidane.Trance2);   // Dyne
    }

    [Fact]
    public void Speciality_ExactlyOneBlueMagePairAssigned()
    {
        var result = MakeRandomizer("bm1", randomizeSpeciality: true).Randomize();
        int blueMageCount = result.SlotAssignment.Values
            .Count(slots => slots.Contains("Blue Mage"));
        Assert.Equal(1, blueMageCount);
    }

    [Fact]
    public void Speciality_ExactlyOneKnightPairAssigned()
    {
        var result = MakeRandomizer("kn1", randomizeSpeciality: true).Randomize();
        int knightCount = result.SlotAssignment.Values
            .Count(slots => slots.Contains("Knight"));
        Assert.Equal(1, knightCount);
    }

    [Fact]
    public void Speciality_ExactlyTwoSummonersAndTwoWhiteMages()
    {
        var result = MakeRandomizer("summon1", randomizeSpeciality: true).Randomize();
        int summons = result.SlotAssignment.Values.Count(s => s.Contains("Summon-A") || s.Contains("Summon-B"));
        int whtMages = result.SlotAssignment.Values.Count(s => s.Contains("Wht Mag-A") || s.Contains("Wht Mag-B"));
        Assert.Equal(2, summons);
        Assert.Equal(2, whtMages);
    }

    [Fact]
    public void Speciality_AllMainCharsHaveAtLeastOneSlot()
    {
        var result = MakeRandomizer("allslots", randomizeSpeciality: true).Randomize();
        for (int charId = 0; charId < 8; charId++)
        {
            Assert.True(result.SlotAssignment.ContainsKey(charId),
                $"Char {charId} missing from SlotAssignment");
            Assert.NotEmpty(result.SlotAssignment[charId]);
        }
    }

    [Fact]
    public void Speciality_CommandSetRowsHaveCorrectFixedColumns()
    {
        var result = MakeRandomizer("fixed", randomizeSpeciality: true).Randomize();
        foreach (var row in result.CommandSetRows)
        {
            Assert.Equal(1, row.Attack);
            Assert.Equal(4, row.Defend);
            Assert.Equal(14, row.Item);
            Assert.Equal(7, row.Change);
            Assert.Equal(1, row.AttackTrance);
            Assert.Equal(4, row.DefendTrance);
            Assert.Equal(14, row.ItemTrance);
            Assert.Equal(7, row.ChangeTrance);
        }
    }

    [Fact]
    public void Speciality_BlueMagePairWritesCorrectCommandIds()
    {
        // Run many seeds until we find one that puts Blue Mage on a non-Zidane char
        for (int s = 0; s < 50; s++)
        {
            var result = MakeRandomizer($"bmcmd{s}", randomizeSpeciality: true).Randomize();
            var blueMageEntry = result.SlotAssignment
                .FirstOrDefault(kvp => kvp.Value.Contains("Blue Mage"));
            if (blueMageEntry.Value == null) continue;

            var row = result.CommandSetRows.First(r => r.Id == blueMageEntry.Key);
            Assert.Equal(8, row.Regular1); // Eat
            Assert.Equal(9, row.Trance1);  // Cook
            Assert.Equal(24, row.Regular2); // Blu Mag
            Assert.Equal(24, row.Trance2);  // Blu Mag
            return;
        }
    }

    [Fact]
    public void Speciality_KnightPairWritesCorrectCommandIds()
    {
        for (int s = 0; s < 50; s++)
        {
            var result = MakeRandomizer($"kncmd{s}", randomizeSpeciality: true).Randomize();
            var knightEntry = result.SlotAssignment
                .FirstOrDefault(kvp => kvp.Value.Contains("Knight"));
            if (knightEntry.Value == null) continue;

            var row = result.CommandSetRows.First(r => r.Id == knightEntry.Key);
            Assert.Equal(30, row.Regular1);
            Assert.Equal(30, row.Trance1);
            Assert.Equal(31, row.Regular2);
            Assert.Equal(31, row.Trance2);
            return;
        }
    }

    [Fact]
    public void Speciality_AbilityFeaturesPatched_BlkMagHolderReferenced()
    {
        var result = MakeRandomizer("patch1", randomizeSpeciality: true).Randomize();

        // Find which char has Blk Mag
        var blkMagEntry = result.SlotAssignment
            .FirstOrDefault(kvp => kvp.Value.Contains("Blk Mag"));

        if (blkMagEntry.Value == null) return; // Blk Mag not assigned this seed — skip

        int charId = blkMagEntry.Key;
        string expectedName = GetCharacterIdName(charId);
        Assert.Contains(expectedName, result.AbilityFeaturesText);
    }

    [Fact]
    public void Speciality_AbilityFeatures_OnlyOneCmdBlockChanged()
    {
        var result = MakeRandomizer("oneblock", randomizeSpeciality: true).Randomize();
        // The SA blocks before and after CMD 31 must be unchanged
        Assert.Contains(">SA 0 Auto-Reflect", result.AbilityFeaturesText);
        Assert.Contains(">SA 1 Auto-Float", result.AbilityFeaturesText);
    }

    [Fact]
    public void Speciality_Recommended_FocusAlwaysOnMagicChar()
    {
        // Run many seeds and verify Focus always lands on a magic-type holder
        var magicSlots = new HashSet<string>
            { "Blk Mag", "Wht Mag-A", "Wht Mag-B", "Summon-A", "Summon-B", "Blue Mage" };

        for (int s = 0; s < 30; s++)
        {
            var result = MakeRandomizer($"focus{s}", randomizeSpeciality: true).Randomize();
            var focusHolder = result.SlotAssignment.FirstOrDefault(
                kvp => kvp.Value.Contains("Focus"));
            if (focusHolder.Value == null) continue;

            bool hasMagicSlot = focusHolder.Value.Any(slot => magicSlots.Contains(slot));
            Assert.True(hasMagicSlot,
                $"Seed focus{s}: Focus on char {focusHolder.Key} who has slots: " +
                string.Join(", ", focusHolder.Value));
        }
    }

    [Fact]
    public void Speciality_Deterministic_SameSeedSameSlotAssignment()
    {
        var r1 = MakeRandomizer("det_spec", randomizeSpeciality: true).Randomize();
        var r2 = MakeRandomizer("det_spec", randomizeSpeciality: true).Randomize();

        for (int charId = 0; charId < 8; charId++)
        {
            Assert.Equal(
                r1.SlotAssignment[charId],
                r2.SlotAssignment[charId]);
        }
    }

    [Fact]
    public void Speciality_Deterministic_SameSeedSameCommandSetRows()
    {
        var r1 = MakeRandomizer("det_cmd", randomizeSpeciality: true).Randomize();
        var r2 = MakeRandomizer("det_cmd", randomizeSpeciality: true).Randomize();

        for (int i = 0; i < r1.CommandSetRows.Count; i++)
        {
            Assert.Equal(r1.CommandSetRows[i].Regular1, r2.CommandSetRows[i].Regular1);
            Assert.Equal(r1.CommandSetRows[i].Regular2, r2.CommandSetRows[i].Regular2);
            Assert.Equal(r1.CommandSetRows[i].Trance1, r2.CommandSetRows[i].Trance1);
            Assert.Equal(r1.CommandSetRows[i].Trance2, r2.CommandSetRows[i].Trance2);
        }
    }

    // ── Sub-Step 3: Abilities ─────────────────────────────────────────────────

    [Fact]
    public void Abilities_Disabled_LeavesTablesUnchanged()
    {
        var result = MakeRandomizer("test").Randomize();
        var vanilla = VanillaAbilityTables();

        // Zidane's vanilla entries should be intact
        var zidaneVanilla = vanilla[0].Select(r => r.AbilityRef).ToHashSet();
        var zidaneResult = result.AbilityTables[0].Select(r => r.AbilityRef).ToHashSet();
        Assert.True(zidaneVanilla.SetEquals(zidaneResult));
    }

    [Fact]
    public void Abilities_ZidaneAlwaysHasSteelAndSkillAbilities()
    {
        var result = MakeRandomizer("zabil", randomizeSpeciality: true,
                                    randomizeAbilities: true).Randomize();
        var zidaneRefs = result.AbilityTables[0].Select(r => r.AbilityRef).ToHashSet();

        // Zidane always has the Steal/Skill AA package
        Assert.Contains("AA:101", zidaneRefs); // Flee
        Assert.Contains("AA:108", zidaneRefs); // Thievery
    }

    [Fact]
    public void Abilities_T1_BanditAlwaysOnStealHolder()
    {
        for (int s = 0; s < 20; s++)
        {
            var result = MakeRandomizer($"bandit{s}", randomizeSpeciality: true,
                                        randomizeAbilities: true).Randomize();
            var stealHolder = result.SlotAssignment
                .First(kvp => kvp.Value.Contains("Steal")).Key;

            var holderRefs = result.AbilityTables[stealHolder]
                .Select(r => r.AbilityRef).ToHashSet();
            Assert.Contains("SA:62", holderRefs); // Bandit
            Assert.Contains("SA:22", holderRefs); // Master Thief
        }
    }

    [Fact]
    public void Abilities_T1_ReflectX2AlwaysOnBlkMagHolder()
    {
        for (int s = 0; s < 20; s++)
        {
            var result = MakeRandomizer($"reflx2{s}", randomizeSpeciality: true,
                                        randomizeAbilities: true).Randomize();
            var blkMagEntry = result.SlotAssignment
                .FirstOrDefault(kvp => kvp.Value.Contains("Blk Mag"));
            if (blkMagEntry.Value == null) continue;

            var holderRefs = result.AbilityTables[blkMagEntry.Key]
                .Select(r => r.AbilityRef).ToHashSet();
            Assert.Contains("SA:31", holderRefs); // Reflectx2
            Assert.Contains("SA:32", holderRefs); // Mag Elem Null
        }
    }

    [Fact]
    public void Abilities_T1_BoostOnBothSummoners()
    {
        for (int s = 0; s < 20; s++)
        {
            var result = MakeRandomizer($"boost{s}", randomizeSpeciality: true,
                                        randomizeAbilities: true).Randomize();
            var summonAEntry = result.SlotAssignment
                .FirstOrDefault(kvp => kvp.Value.Contains("Summon-A"));
            var summonBEntry = result.SlotAssignment
                .FirstOrDefault(kvp => kvp.Value.Contains("Summon-B"));
            if (summonAEntry.Value == null || summonBEntry.Value == null) continue;

            Assert.Contains("SA:59",
                result.AbilityTables[summonAEntry.Key].Select(r => r.AbilityRef));
            Assert.Contains("SA:59",
                result.AbilityTables[summonBEntry.Key].Select(r => r.AbilityRef));
            return;
        }
    }

    [Fact]
    public void Abilities_SummonPool_ExactlySixSummonsPerSummoner()
    {
        var result = MakeRandomizer("splitsum", randomizeSpeciality: true,
                                    randomizeAbilities: true).Randomize();

        var allSummonRefs = new HashSet<string>
        {
            "AA:49","AA:51","AA:53","AA:55","AA:58","AA:60","AA:62","AA:64",
            "AA:66","AA:68","AA:72","AA:74"
        };

        var summonAEntry = result.SlotAssignment
            .FirstOrDefault(kvp => kvp.Value.Contains("Summon-A"));
        var summonBEntry = result.SlotAssignment
            .FirstOrDefault(kvp => kvp.Value.Contains("Summon-B"));
        if (summonAEntry.Value == null || summonBEntry.Value == null) return;

        int aCount = result.AbilityTables[summonAEntry.Key]
            .Count(r => allSummonRefs.Contains(r.AbilityRef));
        int bCount = result.AbilityTables[summonBEntry.Key]
            .Count(r => allSummonRefs.Contains(r.AbilityRef));

        Assert.Equal(6, aCount);
        Assert.Equal(6, bCount);
    }

    [Fact]
    public void Abilities_SummonPool_NoSummonAppearsOnBothSummoners()
    {
        var result = MakeRandomizer("nodupesum", randomizeSpeciality: true,
                                    randomizeAbilities: true).Randomize();

        var allSummonRefs = new HashSet<string>
        {
            "AA:49","AA:51","AA:53","AA:55","AA:58","AA:60","AA:62","AA:64",
            "AA:66","AA:68","AA:72","AA:74"
        };

        var summonAEntry = result.SlotAssignment
            .FirstOrDefault(kvp => kvp.Value.Contains("Summon-A"));
        var summonBEntry = result.SlotAssignment
            .FirstOrDefault(kvp => kvp.Value.Contains("Summon-B"));
        if (summonAEntry.Value == null || summonBEntry.Value == null) return;

        var summonASet = result.AbilityTables[summonAEntry.Key]
            .Where(r => allSummonRefs.Contains(r.AbilityRef))
            .Select(r => r.AbilityRef).ToHashSet();
        var summonBSet = result.AbilityTables[summonBEntry.Key]
            .Where(r => allSummonRefs.Contains(r.AbilityRef))
            .Select(r => r.AbilityRef).ToHashSet();

        Assert.Empty(summonASet.Intersect(summonBSet));
    }

    [Fact]
    public void Abilities_OdinsSword_AlwaysOnOdinHolder()
    {
        for (int s = 0; s < 30; s++)
        {
            var result = MakeRandomizer($"odin{s}", randomizeSpeciality: true,
                                        randomizeAbilities: true).Randomize();

            // Find who has Odin (AA:58)
            int odinHolderCharId = -1;
            for (int c = 0; c < 8; c++)
            {
                if (!result.AbilityTables.TryGetValue(c, out var table)) continue;
                if (table.Any(r => r.AbilityRef == "AA:58"))
                {
                    odinHolderCharId = c;
                    break;
                }
            }
            if (odinHolderCharId < 0) continue;

            var holderRefs = result.AbilityTables[odinHolderCharId]
                .Select(r => r.AbilityRef).ToHashSet();
            Assert.Contains("SA:60", holderRefs);
            return;
        }
    }

    [Fact]
    public void Abilities_WhiteMagicPool_ExactlyTwelveSpellsPerWhiteMage()
    {
        var result = MakeRandomizer("splitwm", randomizeSpeciality: true,
                                    randomizeAbilities: true).Randomize();

        var allWhtRefs = Enumerable.Range(1, 24).Select(i => $"AA:{i}").ToHashSet();

        var whtAEntry = result.SlotAssignment
            .FirstOrDefault(kvp => kvp.Value.Contains("Wht Mag-A"));
        var whtBEntry = result.SlotAssignment
            .FirstOrDefault(kvp => kvp.Value.Contains("Wht Mag-B"));
        if (whtAEntry.Value == null || whtBEntry.Value == null) return;

        int aCount = result.AbilityTables[whtAEntry.Key]
            .Count(r => allWhtRefs.Contains(r.AbilityRef));
        int bCount = result.AbilityTables[whtBEntry.Key]
            .Count(r => allWhtRefs.Contains(r.AbilityRef));

        Assert.Equal(12, aCount);
        Assert.Equal(12, bCount);
    }

    [Fact]
    public void Abilities_WhiteMagicPool_NoSpellAppearsOnBothWhiteMages()
    {
        var result = MakeRandomizer("nodupewm", randomizeSpeciality: true,
                                    randomizeAbilities: true).Randomize();

        var allWhtRefs = Enumerable.Range(1, 24).Select(i => $"AA:{i}").ToHashSet();

        var whtAEntry = result.SlotAssignment
            .FirstOrDefault(kvp => kvp.Value.Contains("Wht Mag-A"));
        var whtBEntry = result.SlotAssignment
            .FirstOrDefault(kvp => kvp.Value.Contains("Wht Mag-B"));
        if (whtAEntry.Value == null || whtBEntry.Value == null) return;

        var whtASet = result.AbilityTables[whtAEntry.Key]
            .Where(r => allWhtRefs.Contains(r.AbilityRef)).Select(r => r.AbilityRef).ToHashSet();
        var whtBSet = result.AbilityTables[whtBEntry.Key]
            .Where(r => allWhtRefs.Contains(r.AbilityRef)).Select(r => r.AbilityRef).ToHashSet();

        Assert.Empty(whtASet.Intersect(whtBSet));
    }

    [Fact]
    public void Abilities_NoCharacterHasDuplicateSA()
    {
        var result = MakeRandomizer("nodupe", randomizeSpeciality: true,
                                    randomizeAbilities: true).Randomize();

        for (int charId = 0; charId < 8; charId++)
        {
            if (!result.AbilityTables.TryGetValue(charId, out var table)) continue;
            var saRefs = table.Where(r => r.AbilityRef.StartsWith("SA:"))
                              .Select(r => r.AbilityRef).ToList();
            Assert.Equal(saRefs.Count, saRefs.Distinct().Count());
        }
    }

    [Fact]
    public void Abilities_ProtectGirls_NeverOnFemaleCharacter()
    {
        // Female chars: Garnet (2), Freya (4), Eiko (6) — DefaultCategory = 6
        var femaleIds = new HashSet<int> { 2, 4, 6 };

        for (int s = 0; s < 50; s++)
        {
            var result = MakeRandomizer($"gender{s}", randomizeSpeciality: true,
                                        randomizeAbilities: true).Randomize();
            foreach (int femId in femaleIds)
            {
                if (!result.AbilityTables.TryGetValue(femId, out var table)) continue;
                Assert.DoesNotContain(table, r => r.AbilityRef == "SA:38");
            }
        }
    }

    [Fact]
    public void Abilities_Deterministic_SameSeedSameAbilityTables()
    {
        var r1 = MakeRandomizer("det_abil", randomizeSpeciality: true,
                                randomizeAbilities: true).Randomize();
        var r2 = MakeRandomizer("det_abil", randomizeSpeciality: true,
                                randomizeAbilities: true).Randomize();

        for (int charId = 0; charId < 8; charId++)
        {
            var refs1 = r1.AbilityTables[charId].Select(r => r.AbilityRef).ToList();
            var refs2 = r2.AbilityTables[charId].Select(r => r.AbilityRef).ToList();
            Assert.Equal(refs1, refs2);
        }
    }

    // ── Sub-Step 4: Equipment ──────────────────────────────────────────────────

    [Fact]
    public void Equipment_Disabled_LeavesEquipmentSetValuesUnchanged()
    {
        var result = MakeRandomizer("test").Randomize();
        var vanilla = VanillaCharParams();

        foreach (var row in result.CharacterParameters.Where(r => r.Id < 8))
        {
            var v = vanilla.First(r => r.Id == row.Id);
            Assert.Equal(v.DefaultEquipmentSet, row.DefaultEquipmentSet);
        }
    }

    [Fact]
    public void Equipment_Shuffled_AllVanillaSetValuesPresent()
    {
        var result = MakeRandomizer("equip1", randomizeEquipment: true).Randomize();
        var vanilla = VanillaCharParams().Where(r => r.Id < 8)
                                         .Select(r => r.DefaultEquipmentSet)
                                         .OrderBy(x => x).ToList();
        var actual = result.CharacterParameters.Where(r => r.Id < 8)
                                                .Select(r => r.DefaultEquipmentSet)
                                                .OrderBy(x => x).ToList();
        Assert.Equal(vanilla, actual);
    }

    [Fact]
    public void Equipment_GuestCharacters_NeverModified()
    {
        var result = MakeRandomizer("equip2", randomizeEquipment: true).Randomize();
        var vanilla = VanillaCharParams();

        foreach (var row in result.CharacterParameters.Where(r => r.Id >= 8))
        {
            var v = vanilla.First(r => r.Id == row.Id);
            Assert.Equal(v.DefaultEquipmentSet, row.DefaultEquipmentSet);
        }
    }

    [Fact]
    public void Equipment_Deterministic_SameSeedSameEquipmentSets()
    {
        var r1 = MakeRandomizer("det_equip", randomizeEquipment: true).Randomize();
        var r2 = MakeRandomizer("det_equip", randomizeEquipment: true).Randomize();

        foreach (var row1 in r1.CharacterParameters.Where(r => r.Id < 8))
        {
            var row2 = r2.CharacterParameters.First(r => r.Id == row1.Id);
            Assert.Equal(row1.DefaultEquipmentSet, row2.DefaultEquipmentSet);
        }
    }

    // ── Full Pipeline ─────────────────────────────────────────────────────────

    [Fact]
    public void FullPipeline_Deterministic_AllSubStepsEnabled()
    {
        var r1 = MakeRandomizer("full1", randomizeStats: true, randomizeSpeciality: true,
                                randomizeAbilities: true, randomizeEquipment: true).Randomize();
        var r2 = MakeRandomizer("full1", randomizeStats: true, randomizeSpeciality: true,
                                randomizeAbilities: true, randomizeEquipment: true).Randomize();

        // Stats
        for (int i = 0; i < r1.BaseStats.Count; i++)
            Assert.Equal(r1.BaseStats[i].Dexterity, r2.BaseStats[i].Dexterity);

        // Slot assignment
        for (int charId = 0; charId < 8; charId++)
            Assert.Equal(r1.SlotAssignment[charId], r2.SlotAssignment[charId]);

        // Ability tables
        for (int charId = 0; charId < 8; charId++)
        {
            var refs1 = r1.AbilityTables[charId].Select(r => r.AbilityRef).ToList();
            var refs2 = r2.AbilityTables[charId].Select(r => r.AbilityRef).ToList();
            Assert.Equal(refs1, refs2);
        }

        // Equipment
        foreach (var row1 in r1.CharacterParameters.Where(r => r.Id < 8))
        {
            var row2 = r2.CharacterParameters.First(r => r.Id == row1.Id);
            Assert.Equal(row1.DefaultEquipmentSet, row2.DefaultEquipmentSet);
        }
    }

    [Fact]
    public void FullPipeline_Chaos_Deterministic()
    {
        var r1 = MakeRandomizer("chaos_full", randomizeStats: true, randomizeSpeciality: true,
                                randomizeAbilities: true, randomizeEquipment: true,
                                mode: RandomizerMode.Chaos).Randomize();
        var r2 = MakeRandomizer("chaos_full", randomizeStats: true, randomizeSpeciality: true,
                                randomizeAbilities: true, randomizeEquipment: true,
                                mode: RandomizerMode.Chaos).Randomize();

        for (int charId = 0; charId < 8; charId++)
        {
            Assert.Equal(r1.SlotAssignment[charId], r2.SlotAssignment[charId]);
            var refs1 = r1.AbilityTables[charId].Select(r => r.AbilityRef).ToList();
            var refs2 = r2.AbilityTables[charId].Select(r => r.AbilityRef).ToList();
            Assert.Equal(refs1, refs2);
        }
    }

    [Fact]
    public void FullPipeline_DifferentSeedsDifferentOutput()
    {
        var r1 = MakeRandomizer("seedA", randomizeSpeciality: true,
                                randomizeAbilities: true).Randomize();
        var r2 = MakeRandomizer("seedB", randomizeSpeciality: true,
                                randomizeAbilities: true).Randomize();

        // With different seeds, at least one character must have a different slot assignment
        bool anyDifference = false;
        for (int charId = 0; charId < 8 && !anyDifference; charId++)
        {
            var slots1 = string.Join(",", r1.SlotAssignment[charId]);
            var slots2 = string.Join(",", r2.SlotAssignment[charId]);
            if (slots1 != slots2) anyDifference = true;
        }
        Assert.True(anyDifference, "Different seeds produced identical slot assignments");
    }

    [Fact]
    public void FullPipeline_CharacterParameterNonEquipColumns_NeverModified()
    {
        var result = MakeRandomizer("nomod", randomizeStats: true, randomizeSpeciality: true,
                                     randomizeAbilities: true, randomizeEquipment: true).Randomize();
        var vanilla = VanillaCharParams();

        foreach (var row in result.CharacterParameters)
        {
            var v = vanilla.First(r => r.Id == row.Id);
            // BattleParameterFormula and NameKeyword must never be touched
            Assert.Equal(v.BattleParameterFormula, row.BattleParameterFormula);
            Assert.Equal(v.NameKeyword, row.NameKeyword);
            Assert.Equal(v.DefaultCategory, row.DefaultCategory);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string GetCharacterIdName(int charId) => charId switch
    {
        0 => "CharacterId_Zidane",
        1 => "CharacterId_Vivi",
        2 => "CharacterId_Garnet",
        3 => "CharacterId_Steiner",
        4 => "CharacterId_Freya",
        5 => "CharacterId_Quina",
        6 => "CharacterId_Eiko",
        7 => "CharacterId_Amarant",
        _ => throw new ArgumentOutOfRangeException(nameof(charId))
    };
}