using System.Collections.Generic;
using System.Linq;
using StiltzkinsBag.Models.Csv;
using StiltzkinsBag.Randomizers;
using Xunit;

namespace StiltzkinsBag.Tests;

/// <summary>
/// Tests for <see cref="RecommendedLogicEngine"/> equipment coherence methods:
///   • <see cref="RecommendedLogicEngine.ClassifyWeapon"/> — weapon affinity by item ID range
///   • <see cref="RecommendedLogicEngine.ClassifySlots"/>  — slot affinity by majority vote
///   • <see cref="RecommendedLogicEngine.CorrectEquipmentCoherence"/> — greedy pairwise swap
///
/// Vanilla data (from DefaultEquipment.csv and CharacterParameters.csv):
///   Set 0 Zidane  Weapon=1  (Dagger)       → Balanced
///   Set 1 Vivi    Weapon=70 (Mage Staff)   → Magical
///   Set 2 Garnet  Weapon=57 (Rod)          → Magical
///   Set 3 Steiner Weapon=16 (Broadsword)   → Physical
///   Set 4 Freya   Weapon=31 (Javelin)      → Physical
///   Set 5 Quina   Weapon=79 (Fork)         → Balanced
///   Set 6 Eiko    Weapon=64 (Golem Flute)  → Magical
///   Set 7 Amarant Weapon=41 (Cat's Claws)  → Physical
/// </summary>
public class RecommendedLogicEngineEquipmentTests
{
    // ── Fixture helpers ────────────────────────────────────────────────────────

    /// <summary>Vanilla equipment sets 0–7 from DefaultEquipment.csv.</summary>
    private static List<DefaultEquipmentRow> VanillaEquipmentSets() => new()
    {
        new() { Comment = "Zidane",  Id = 0, Weapon = 1,  Head = 112, Wrist = 88,  Armor = 149, Accessory = -1  },
        new() { Comment = "Vivi",    Id = 1, Weapon = 70, Head = 112, Wrist = -1,  Armor = 149, Accessory = -1  },
        new() { Comment = "Garnet",  Id = 2, Weapon = 57, Head = -1,  Wrist = -1,  Armor = 150, Accessory = -1  },
        new() { Comment = "Steiner", Id = 3, Weapon = 16, Head = 137, Wrist = -1,  Armor = 177, Accessory = -1  },
        new() { Comment = "Freya",   Id = 4, Weapon = 31, Head = 136, Wrist = 102, Armor = 178, Accessory = -1  },
        new() { Comment = "Quina",   Id = 5, Weapon = 79, Head = -1,  Wrist = -1,  Armor = -1,  Accessory = -1  },
        new() { Comment = "Eiko",    Id = 6, Weapon = 64, Head = 114, Wrist = 90,  Armor = 150, Accessory = 232 },
        new() { Comment = "Amarant", Id = 7, Weapon = 41, Head = -1,  Wrist = 89,  Armor = 155, Accessory = 194 },
    };

    /// <summary>
    /// Vanilla CharacterParameters rows 0–11 with vanilla DefaultEquipmentSet values.
    /// </summary>
    private static List<CharacterParametersRow> VanillaCharParams() => new()
    {
        new() { Id = 0,  DefaultRow = true,  DefaultWinPose = true,  DefaultCategory = 9,  DefaultCommandSet = 0,  DefaultEquipmentSet = 0,  BattleParameterFormula = "WeaponShape == 1 ? 0 : 1",                                                              NameKeyword = "ZDNE" },
        new() { Id = 1,  DefaultRow = false, DefaultWinPose = true,  DefaultCategory = 5,  DefaultCommandSet = 1,  DefaultEquipmentSet = 1,  BattleParameterFormula = "2",                                                                                        NameKeyword = "VIVI" },
        new() { Id = 2,  DefaultRow = false, DefaultWinPose = true,  DefaultCategory = 6,  DefaultCommandSet = 2,  DefaultEquipmentSet = 2,  BattleParameterFormula = "ScenarioCounter < 10300 ? (WeaponShape == 7 ? 4 : 3) : (WeaponShape == 7 ? 6 : 5)",        NameKeyword = "DGGR" },
        new() { Id = 3,  DefaultRow = true,  DefaultWinPose = true,  DefaultCategory = 5,  DefaultCommandSet = 3,  DefaultEquipmentSet = 3,  BattleParameterFormula = "7",                                                                                        NameKeyword = "STNR" },
        new() { Id = 4,  DefaultRow = false, DefaultWinPose = true,  DefaultCategory = 6,  DefaultCommandSet = 4,  DefaultEquipmentSet = 4,  BattleParameterFormula = "12",                                                                                       NameKeyword = "FRYA" },
        new() { Id = 5,  DefaultRow = true,  DefaultWinPose = true,  DefaultCategory = 5,  DefaultCommandSet = 5,  DefaultEquipmentSet = 5,  BattleParameterFormula = "9",                                                                                        NameKeyword = "QUIN" },
        new() { Id = 6,  DefaultRow = true,  DefaultWinPose = true,  DefaultCategory = 6,  DefaultCommandSet = 6,  DefaultEquipmentSet = 6,  BattleParameterFormula = "WeaponShape == 7 ? 11 : 10",                                                              NameKeyword = "EIKO" },
        new() { Id = 7,  DefaultRow = true,  DefaultWinPose = true,  DefaultCategory = 5,  DefaultCommandSet = 7,  DefaultEquipmentSet = 7,  BattleParameterFormula = "13",                                                                                       NameKeyword = "AMRT" },
        new() { Id = 8,  DefaultRow = true,  DefaultWinPose = true,  DefaultCategory = 21, DefaultCommandSet = 8,  DefaultEquipmentSet = 8,  BattleParameterFormula = "14",                                                                                       NameKeyword = "CINA" },
        new() { Id = 9,  DefaultRow = true,  DefaultWinPose = true,  DefaultCategory = 21, DefaultCommandSet = 10, DefaultEquipmentSet = 9,  BattleParameterFormula = "15",                                                                                       NameKeyword = "MRCS" },
        new() { Id = 10, DefaultRow = true,  DefaultWinPose = true,  DefaultCategory = 21, DefaultCommandSet = 12, DefaultEquipmentSet = 10, BattleParameterFormula = "(1500 <= ScenarioCounter && ScenarioCounter < 1600) ? 17 : 16",                            NameKeyword = "BLNK" },
        new() { Id = 11, DefaultRow = true,  DefaultWinPose = true,  DefaultCategory = 22, DefaultCommandSet = 14, DefaultEquipmentSet = 11, BattleParameterFormula = "18",                                                                                       NameKeyword = "BTRX" },
    };

    /// <summary>
    /// Vanilla slot assignment: maps each char ID to its actual vanilla slot types.
    /// SlotAffinity results:
    ///   0 Zidane  [Steal, Skill]        → Physical  (0 + -1 = -1)
    ///   1 Vivi    [Blk Mag, Focus]      → Magical   (+1 + +1 = +2)
    ///   2 Garnet  [Summon-A, Wht Mag-A] → Magical   (+1 + +1 = +2)
    ///   3 Steiner [Swd Art, Swd Mag]    → Physical  (-1 + -1 = -2)
    ///   4 Freya   [Jump, Dragon]        → Physical  (-1 + -1 = -2)
    ///   5 Quina   [Eat, Blu Mag]        → Balanced  (0  + 0  = 0)
    ///   6 Eiko    [Wht Mag-B, Summon-B] → Magical   (+1 + +1 = +2)
    ///   7 Amarant [Flair, Throw]        → Physical  (-1 + -1 = -2)
    /// </summary>
    private static Dictionary<int, IReadOnlyList<string>> VanillaSlotAssignment() => new()
    {
        { 0, new[] { "Steal",     "Skill"     } },
        { 1, new[] { "Blk Mag",   "Focus"     } },
        { 2, new[] { "Summon-A",  "Wht Mag-A" } },
        { 3, new[] { "Swd Art",   "Swd Mag"   } },
        { 4, new[] { "Jump",      "Dragon"    } },
        { 5, new[] { "Eat",       "Blu Mag"   } },
        { 6, new[] { "Wht Mag-B", "Summon-B"  } },
        { 7, new[] { "Flair",     "Throw"     } },
    };

    /// <summary>
    /// Returns vanilla CharParams with Vivi (char 1) and Steiner (char 3) equipment
    /// sets swapped — the classic mismatch scenario.
    /// Vivi   (Magical slots) gets set 3 (Broadsword → Physical) → score 0
    /// Steiner (Physical slots) gets set 1 (Mage Staff → Magical)  → score 0
    /// Correct assignment: swap them back.
    /// </summary>
    private static List<CharacterParametersRow> MismatchedCharParams()
    {
        var rows = VanillaCharParams();
        rows.First(r => r.Id == 1).DefaultEquipmentSet = 3;
        rows.First(r => r.Id == 3).DefaultEquipmentSet = 1;
        return rows;
    }

    // ── ClassifyWeapon ─────────────────────────────────────────────────────────

    // WeaponAffinity is internal — InlineData must use int; cast inside method body.
    // None=0, Magical=1 (... varies by enum order) — use the named constants via cast.
    // Simpler: express expected as the actual ClassifyWeapon call on a known ID
    // and compare results. Or use int values matching the enum definition order:
    //   Magical=0, Physical=1, Balanced=2, None=3
    [Theory]
    [InlineData(-1, 3)]   // None
    [InlineData(0, 3)]   // None
    [InlineData(1, 2)]   // Balanced — dagger low boundary
    [InlineData(15, 2)]   // Balanced — dagger high boundary
    [InlineData(16, 1)]   // Physical — sword low boundary
    [InlineData(50, 1)]   // Physical — claw high boundary
    [InlineData(51, 0)]   // Magical  — racket low boundary
    [InlineData(78, 0)]   // Magical  — flute high boundary
    [InlineData(79, 2)]   // Balanced — fork low boundary
    [InlineData(87, 2)]   // Balanced — fork high boundary
    [InlineData(88, 2)]   // Balanced — accessory fallback
    public void ClassifyWeapon_ReturnsExpected(int weaponId, int expectedInt)
        => Assert.Equal(
               (RecommendedLogicEngine.WeaponAffinity)expectedInt,
               RecommendedLogicEngine.ClassifyWeapon(weaponId));

    // ── ClassifySlots ──────────────────────────────────────────────────────────

    [Fact]
    public void ClassifySlots_AllMagical_ReturnsMagical()
        => Assert.Equal(RecommendedLogicEngine.SlotAffinity.Magical,
               RecommendedLogicEngine.ClassifySlots(new[] { "Blk Mag", "Focus" }));

    [Fact]
    public void ClassifySlots_MagicalSummonAndWhiteMage_ReturnsMagical()
        => Assert.Equal(RecommendedLogicEngine.SlotAffinity.Magical,
               RecommendedLogicEngine.ClassifySlots(new[] { "Summon-A", "Wht Mag-A" }));

    [Fact]
    public void ClassifySlots_AllPhysical_ReturnsPhysical()
        => Assert.Equal(RecommendedLogicEngine.SlotAffinity.Physical,
               RecommendedLogicEngine.ClassifySlots(new[] { "Swd Art", "Swd Mag" }));

    [Fact]
    public void ClassifySlots_JumpAndDragon_ReturnsPhysical()
        => Assert.Equal(RecommendedLogicEngine.SlotAffinity.Physical,
               RecommendedLogicEngine.ClassifySlots(new[] { "Jump", "Dragon" }));

    [Fact]
    public void ClassifySlots_ZidaneSlots_ReturnsPhysical()
        // Steal = 0 (Balanced), Skill = -1 (Physical) → sum -1 → Physical
        => Assert.Equal(RecommendedLogicEngine.SlotAffinity.Physical,
               RecommendedLogicEngine.ClassifySlots(new[] { "Steal", "Skill" }));

    [Fact]
    public void ClassifySlots_BaChingChingSlots_ReturnsBalanced()
        // Eat = 0, Blu Mag = 0 → sum 0 → Balanced
        => Assert.Equal(RecommendedLogicEngine.SlotAffinity.Balanced,
               RecommendedLogicEngine.ClassifySlots(new[] { "Eat", "Blu Mag" }));

    [Fact]
    public void ClassifySlots_MagicalPhysicalTie_ReturnsBalanced()
        // Summon-A = +1, Dragon = -1 → sum 0 → Balanced
        => Assert.Equal(RecommendedLogicEngine.SlotAffinity.Balanced,
               RecommendedLogicEngine.ClassifySlots(new[] { "Summon-A", "Dragon" }));

    [Fact]
    public void ClassifySlots_SingleMagicalSlot_ReturnsMagical()
        => Assert.Equal(RecommendedLogicEngine.SlotAffinity.Magical,
               RecommendedLogicEngine.ClassifySlots(new[] { "Wht Mag-B" }));

    [Fact]
    public void ClassifySlots_NullOrEmpty_ReturnsBalanced()
    {
        Assert.Equal(RecommendedLogicEngine.SlotAffinity.Balanced,
            RecommendedLogicEngine.ClassifySlots(null));
        Assert.Equal(RecommendedLogicEngine.SlotAffinity.Balanced,
            RecommendedLogicEngine.ClassifySlots(System.Array.Empty<string>()));
    }

    // ── CorrectEquipmentCoherence ──────────────────────────────────────────────

    [Fact]
    public void CorrectEquipment_VanillaInput_NoChanges()
    {
        // Vanilla pairing is already locally optimal — no beneficial swaps exist.
        var charParams = VanillaCharParams();
        var slots = VanillaSlotAssignment();
        var sets = VanillaEquipmentSets();

        var result = RecommendedLogicEngine.CorrectEquipmentCoherence(charParams, slots, sets);

        // All 8 main character set IDs unchanged
        for (int id = 0; id <= 7; id++)
        {
            Assert.Equal(id, result.First(r => r.Id == id).DefaultEquipmentSet);
        }
    }

    [Fact]
    public void CorrectEquipment_ClassicMismatch_ViviGetsNonPhysicalWeapon_SteinerGetsNonMagicalWeapon()
    {
        // Vivi (Magical slots) starts with a Physical weapon set — score 0.
        // Steiner (Physical slots) starts with a Magical weapon set — score 0.
        // The algorithm must improve both affinities. It may achieve this by a
        // different combination of swaps than a direct Vivi↔Steiner exchange —
        // multiple local optima exist. We test the affinity outcome, not specific set IDs.
        var charParams = MismatchedCharParams();
        var slots = VanillaSlotAssignment();
        var sets = VanillaEquipmentSets();
        var setById = sets.ToDictionary(s => s.Id);

        var result = RecommendedLogicEngine.CorrectEquipmentCoherence(charParams, slots, sets);

        // Vivi (Magical) must not end up with a Physical weapon
        var viviSetId = result.First(r => r.Id == 1).DefaultEquipmentSet;
        var viviAffinity = RecommendedLogicEngine.ClassifyWeapon(setById[viviSetId].Weapon);
        Assert.NotEqual((int)RecommendedLogicEngine.WeaponAffinity.Physical, (int)viviAffinity);

        // Steiner (Physical) must not end up with a Magical weapon
        var steinerSetId = result.First(r => r.Id == 3).DefaultEquipmentSet;
        var steinerAffinity = RecommendedLogicEngine.ClassifyWeapon(setById[steinerSetId].Weapon);
        Assert.NotEqual((int)RecommendedLogicEngine.WeaponAffinity.Magical, (int)steinerAffinity);

        // The set IDs across chars 0–7 must be a permutation of the input set IDs
        var before = charParams.Where(r => r.Id <= 7)
                               .Select(r => r.DefaultEquipmentSet).OrderBy(x => x).ToList();
        var after = result.Where(r => r.Id <= 7)
                           .Select(r => r.DefaultEquipmentSet).OrderBy(x => x).ToList();
        Assert.Equal(before, after);
    }

    [Fact]
    public void CorrectEquipment_AllBalancedSlots_NoChanges()
    {
        // Balanced slot affinity scores 1 with any weapon — no swap can improve.
        var charParams = VanillaCharParams();
        var slots = Enumerable.Range(0, 8)
            .ToDictionary(i => i, _ => (IReadOnlyList<string>)new[] { "Steal", "Blu Mag" });
        var sets = VanillaEquipmentSets();

        var result = RecommendedLogicEngine.CorrectEquipmentCoherence(charParams, slots, sets);

        for (int id = 0; id <= 7; id++)
            Assert.Equal(id, result.First(r => r.Id == id).DefaultEquipmentSet);
    }

    [Fact]
    public void CorrectEquipment_AllBalancedWeapons_NoChanges()
    {
        // All sets have Balanced weapons (daggers) — any slot pairing scores 1.
        var charParams = VanillaCharParams();
        var slots = VanillaSlotAssignment();
        var sets = Enumerable.Range(0, 8)
            .Select(i => new DefaultEquipmentRow { Id = i, Weapon = 1 }) // Dagger = Balanced
            .ToList();

        var result = RecommendedLogicEngine.CorrectEquipmentCoherence(charParams, slots, sets);

        for (int id = 0; id <= 7; id++)
            Assert.Equal(id, result.First(r => r.Id == id).DefaultEquipmentSet);
    }

    [Fact]
    public void CorrectEquipment_NoInputMutation_OriginalObjectsUnchanged()
    {
        // After correction, the original CharacterParametersRow objects must not be mutated.
        var charParams = MismatchedCharParams();
        var viviRow = charParams.First(r => r.Id == 1);
        var steinerRow = charParams.First(r => r.Id == 3);
        int originalViviSet = viviRow.DefaultEquipmentSet;    // 3
        int originalSteinerSet = steinerRow.DefaultEquipmentSet; // 1

        RecommendedLogicEngine.CorrectEquipmentCoherence(
            charParams, VanillaSlotAssignment(), VanillaEquipmentSets());

        Assert.Equal(originalViviSet, viviRow.DefaultEquipmentSet);
        Assert.Equal(originalSteinerSet, steinerRow.DefaultEquipmentSet);
    }

    [Fact]
    public void CorrectEquipment_ChangedRows_AreNewInstances()
    {
        // Rows whose DefaultEquipmentSet changed must be new objects, not the originals.
        var charParams = MismatchedCharParams();
        var originalVivi = charParams.First(r => r.Id == 1);
        var originalSteiner = charParams.First(r => r.Id == 3);

        var result = RecommendedLogicEngine.CorrectEquipmentCoherence(
            charParams, VanillaSlotAssignment(), VanillaEquipmentSets());

        Assert.NotSame(originalVivi, result.First(r => r.Id == 1));
        Assert.NotSame(originalSteiner, result.First(r => r.Id == 3));
    }

    [Fact]
    public void CorrectEquipment_VanillaInput_AllRowsAreOriginalReferences()
    {
        // With vanilla input (already locally optimal), no swaps occur.
        // Every row in the output must be the exact same object reference as in the input.
        var charParams = VanillaCharParams();
        var originals = charParams.ToDictionary(r => r.Id);

        var result = RecommendedLogicEngine.CorrectEquipmentCoherence(
            charParams, VanillaSlotAssignment(), VanillaEquipmentSets());

        foreach (var (id, original) in originals)
            Assert.Same(original, result.First(r => r.Id == id));
    }

    [Fact]
    public void CorrectEquipment_GuestRowsPassThrough_Unchanged()
    {
        // Rows with Id 8–11 must appear in output unchanged and as original references.
        var charParams = VanillaCharParams(); // includes IDs 8–11
        var originals = charParams.Where(r => r.Id >= 8).ToDictionary(r => r.Id);

        var result = RecommendedLogicEngine.CorrectEquipmentCoherence(
            charParams, VanillaSlotAssignment(), VanillaEquipmentSets());

        foreach (var (id, original) in originals)
        {
            var inResult = result.First(r => r.Id == id);
            Assert.Same(original, inResult);
            Assert.Equal(original.DefaultEquipmentSet, inResult.DefaultEquipmentSet);
        }
    }

    [Fact]
    public void CorrectEquipment_MissingCharacterRow_ThrowsArgumentException()
    {
        // If any char ID 0–7 is absent, method must throw ArgumentException.
        var charParams = VanillaCharParams()
            .Where(r => r.Id != 3)  // remove Steiner
            .ToList();

        Assert.Throws<ArgumentException>(() =>
            RecommendedLogicEngine.CorrectEquipmentCoherence(
                charParams, VanillaSlotAssignment(), VanillaEquipmentSets()));
    }

    [Fact]
    public void CorrectEquipment_Deterministic_SameInputProducesSameOutput()
    {
        var slots = VanillaSlotAssignment();
        var sets = VanillaEquipmentSets();

        var result1 = RecommendedLogicEngine.CorrectEquipmentCoherence(
            MismatchedCharParams(), slots, sets);
        var result2 = RecommendedLogicEngine.CorrectEquipmentCoherence(
            MismatchedCharParams(), slots, sets);

        for (int id = 0; id <= 7; id++)
        {
            Assert.Equal(
                result1.First(r => r.Id == id).DefaultEquipmentSet,
                result2.First(r => r.Id == id).DefaultEquipmentSet);
        }
    }
}

/// <summary>
/// Tests for <see cref="RecommendedLogicEngine.EnforceLegendaryRarity"/>.
///
/// Protected IDs: 221 (Ribbon), 250 (Dark Matter).
/// Replacement pool: [236, 237, 240] (Potion, Hi-Potion, Phoenix Down).
/// </summary>
public class RecommendedLogicEngineLegendaryRarityTests
{
    private static readonly IReadOnlySet<int> ProtectedIds =
        new HashSet<int> { 221, 250 };

    private static readonly IReadOnlyList<int> ReplacementPool =
        new[] { 236, 237, 240 };

    private static List<ShopItemsRow> MakeShops(params (int id, int[] items)[] shops) =>
        shops.Select(s => new ShopItemsRow
        {
            Comment = $"Shop {s.id:D4}",
            Id = s.id,
            Items = s.items,
        }).ToList();

    [Fact]
    public void NoProtectedItems_ReturnsOriginalReferences()
    {
        var shops = MakeShops(
            (0, new[] { 236, 237, 240 }),
            (1, new[] { 1, 2, 3 }));

        var result = RecommendedLogicEngine.EnforceLegendaryRarity(
            shops, ProtectedIds, ReplacementPool, new Random(42));

        Assert.Same(shops[0], result[0]);
        Assert.Same(shops[1], result[1]);
    }

    [Fact]
    public void ProtectedItemInShop_IsReplaced()
    {
        var shops = MakeShops((0, new[] { 1, 221, 3 }));

        var result = RecommendedLogicEngine.EnforceLegendaryRarity(
            shops, ProtectedIds, ReplacementPool, new Random(42));

        Assert.DoesNotContain(221, result[0].Items);
        Assert.Contains(result[0].Items[1], ReplacementPool);
    }

    [Fact]
    public void AllProtectedShop_AllReplaced()
    {
        var shops = MakeShops((0, new[] { 221, 250, 221 }));

        var result = RecommendedLogicEngine.EnforceLegendaryRarity(
            shops, ProtectedIds, ReplacementPool, new Random(99));

        foreach (int item in result[0].Items)
            Assert.Contains(item, ReplacementPool);
    }

    [Fact]
    public void MultipleShops_OnlyAffectedShopChanges()
    {
        var shops = MakeShops(
            (0, new[] { 1, 2, 3 }),
            (1, new[] { 4, 250, 6 }));

        var result = RecommendedLogicEngine.EnforceLegendaryRarity(
            shops, ProtectedIds, ReplacementPool, new Random(42));

        Assert.Same(shops[0], result[0]);
        Assert.NotSame(shops[1], result[1]);
        Assert.DoesNotContain(250, result[1].Items);
    }

    [Fact]
    public void NonProtectedSlotsPreserved()
    {
        var shops = MakeShops((0, new[] { 10, 221, 20 }));

        var result = RecommendedLogicEngine.EnforceLegendaryRarity(
            shops, ProtectedIds, ReplacementPool, new Random(42));

        Assert.Equal(10, result[0].Items[0]);
        Assert.Equal(20, result[0].Items[2]);
    }

    [Fact]
    public void EmptyShop_PassesThrough()
    {
        var shops = MakeShops((0, Array.Empty<int>()));

        var result = RecommendedLogicEngine.EnforceLegendaryRarity(
            shops, ProtectedIds, ReplacementPool, new Random(42));

        Assert.Same(shops[0], result[0]);
        Assert.Empty(result[0].Items);
    }

    [Fact]
    public void NoInputMutation_OriginalArrayUnchanged()
    {
        var originalItems = new[] { 1, 221, 3 };
        var shops = new List<ShopItemsRow>
        {
            new() { Comment = "Shop 0000", Id = 0, Items = originalItems }
        };

        RecommendedLogicEngine.EnforceLegendaryRarity(
            shops, ProtectedIds, ReplacementPool, new Random(42));

        Assert.Equal(221, originalItems[1]);
    }

    [Fact]
    public void ChangedRow_IsNewInstance()
    {
        var shops = MakeShops((0, new[] { 221 }));
        var original = shops[0];

        var result = RecommendedLogicEngine.EnforceLegendaryRarity(
            shops, ProtectedIds, ReplacementPool, new Random(42));

        Assert.NotSame(original, result[0]);
    }

    [Fact]
    public void ChangedRow_PreservesCommentAndId()
    {
        var shops = MakeShops((7, new[] { 250 }));
        shops[0].Comment = "Shop 0007";

        var result = RecommendedLogicEngine.EnforceLegendaryRarity(
            shops, ProtectedIds, ReplacementPool, new Random(42));

        Assert.Equal("Shop 0007", result[0].Comment);
        Assert.Equal(7, result[0].Id);
    }

    [Fact]
    public void Deterministic_SameSeedProducesSameOutput()
    {
        var shops = MakeShops(
            (0, new[] { 221, 1, 250 }),
            (1, new[] { 2, 221, 3 }));

        var result1 = RecommendedLogicEngine.EnforceLegendaryRarity(
            shops, ProtectedIds, ReplacementPool, new Random(12345));
        var result2 = RecommendedLogicEngine.EnforceLegendaryRarity(
            shops, ProtectedIds, ReplacementPool, new Random(12345));

        for (int i = 0; i < result1.Count; i++)
            Assert.Equal(result1[i].Items, result2[i].Items);
    }

    [Fact]
    public void DifferentSeeds_ProduceDifferentReplacements()
    {
        // 10 protected slots, pool size 3 — P(identical) < 0.003%
        var shops = MakeShops((0, Enumerable.Repeat(221, 10).ToArray()));

        var result1 = RecommendedLogicEngine.EnforceLegendaryRarity(
            shops, ProtectedIds, ReplacementPool, new Random(1));
        var result2 = RecommendedLogicEngine.EnforceLegendaryRarity(
            shops, ProtectedIds, ReplacementPool, new Random(2));

        Assert.False(result1[0].Items.SequenceEqual(result2[0].Items));
    }

    [Fact]
    public void EmptyReplacementPool_Throws()
        => Assert.Throws<ArgumentException>(() =>
               RecommendedLogicEngine.EnforceLegendaryRarity(
                   MakeShops((0, new[] { 221 })),
                   ProtectedIds, Array.Empty<int>(), new Random(1)));

    [Fact]
    public void NullShopRows_Throws()
        => Assert.Throws<ArgumentNullException>(() =>
               RecommendedLogicEngine.EnforceLegendaryRarity(
                   null!, ProtectedIds, ReplacementPool, new Random(1)));

    [Fact]
    public void NullProtectedIds_Throws()
        => Assert.Throws<ArgumentNullException>(() =>
               RecommendedLogicEngine.EnforceLegendaryRarity(
                   MakeShops((0, new[] { 1 })), null!, ReplacementPool, new Random(1)));

    [Fact]
    public void NullReplacementPool_Throws()
        => Assert.Throws<ArgumentNullException>(() =>
               RecommendedLogicEngine.EnforceLegendaryRarity(
                   MakeShops((0, new[] { 1 })), ProtectedIds, null!, new Random(1)));
}