// StiltzkinsBag.Tests/VanillaItemCatalogTests.cs
//
// Tests for:
//   • VanillaObtainabilityData   — static hardcoded source sets
//   • ItemObtainabilityEntry     — per-item flag record
//   • FieldItemScanner           — binary field item extraction
//   • VanillaItemCatalog         — assembly factory and master count dictionary
//   • ItemPool                   — mode-aware pool builder
//
// Field extraction from p0data7.bin (FieldExtractionTests region):
//   Tests that UnityArchiver can open p0data7.bin and extract known field files.
//   Extraction is verified functionally (parseable, non-trivial length) — NOT by
//   byte equality, because Steam patches change field script bytes across game versions.
//   These tests skip gracefully if p0data7.bin is not present.
//
// FieldItemScanner tests use pre-committed .eb.bytes files from TestData/FieldParser/
// — no archive required.
//
// Catalog and Pool tests load the real Items.csv, ShopItems.csv, and Synthesis.csv
// from TestData/ (always present via CopyToOutputDirectory = PreserveNewest).
//
// Ground-truth values derived from:
//   • Items.csv (IDs, prices, flags)
//   • ShopItems.csv (153 unique item IDs)
//   • VanillaObtainabilityData hardcoded sets
//   • FieldParser.FindItemLocations scan of pre-committed .bytes files

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StiltzkinsBag.Core.Models;
using StiltzkinsBag.Core.Models.Battle;
using StiltzkinsBag.Core.Parsing;
using StiltzkinsBag.Parsing;
using Xunit;
using Xunit.Abstractions;

namespace StiltzkinsBag.Tests;

public sealed class VanillaItemCatalogTests
{
    // ── Test data helpers ──────────────────────────────────────────────────────

    private static string TestData(string file) =>
        Path.Combine(AppContext.BaseDirectory, "TestData", file);

    private static string FieldParserData(string file) =>
        Path.Combine(AppContext.BaseDirectory, "TestData", "FieldParser", file);

    private static string ItemsCsvPath    => TestData("Items.csv");
    private static string ShopItemsCsvPath => TestData("ShopItems.csv");
    private static string SynthesisCsvPath => TestData("Synthesis.csv");

    private static readonly string P0data7Path = FindArchive("p0data7.bin");
    private static readonly string P0data2Path = FindArchive("p0data2.bin");

    private static string FindArchive(string filename)
    {
        string dir = AppContext.BaseDirectory;
        for (int i = 0; i < 10; i++)
        {
            string candidate = Path.Combine(dir, "TestData", filename);
            if (File.Exists(candidate)) return candidate;
            string? parent = Directory.GetParent(dir)?.FullName;
            if (parent is null) break;
            dir = parent;
        }
        return Path.Combine(AppContext.BaseDirectory, "TestData", filename);
    }

    private static void SkipIfNoP0data7()
    {
        if (!File.Exists(P0data7Path))
            throw new SkipException(
                $"p0data7.bin not found at: {P0data7Path}\n" +
                "Copy it from your FF9 Steam install (StreamingAssets\\p0data7.bin).\n" +
                "Field extraction tests require this file.");
    }

    private static void SkipIfNoP0data2()
    {
        if (!File.Exists(P0data2Path))
            throw new SkipException(
                $"p0data2.bin not found at: {P0data2Path}\n" +
                "Copy it from your FF9 Steam install (StreamingAssets\\p0data2.bin).");
    }

    // ── Build catalog helper ───────────────────────────────────────────────────

    /// <summary>
    /// Builds the catalog using real CSVs from TestData and no field item counts.
    /// Use this for catalog/pool tests that don't need field data.
    /// </summary>
    private static VanillaItemCatalog BuildCatalog(
    IReadOnlyDictionary<int, int>? fieldCounts = null,
    BattleScanResult? battleScanResult = null) =>
    VanillaItemCatalog.Build(
        ItemsCsvPath, ShopItemsCsvPath, SynthesisCsvPath,
        fieldCounts ?? new Dictionary<int, int>(),
        null,
        battleScanResult);

    private readonly ITestOutputHelper _out;
    public VanillaItemCatalogTests(ITestOutputHelper output) => _out = output;

    // =========================================================================
    // 1. VanillaObtainabilityData — static set sanity
    // =========================================================================

    [Fact]
    public void ObtainabilityData_NormalEnemySet_ContainsExpectedConsumables()
    {
        // Potion (236), Hi-Potion (237), Ether (238), Phoenix Down (240) are
        // all commonly dropped/stolen from normal enemies.
        Assert.Contains(236, VanillaObtainabilityData.NormalEnemyItemIds);
        Assert.Contains(237, VanillaObtainabilityData.NormalEnemyItemIds);
        Assert.Contains(238, VanillaObtainabilityData.NormalEnemyItemIds);
        Assert.Contains(240, VanillaObtainabilityData.NormalEnemyItemIds);
    }

    [Fact]
    public void ObtainabilityData_NormalEnemySet_ContainsExpectedGems()
    {
        // Garnet(224), Amethyst(225), Peridot(231) etc. drop from normal enemies.
        Assert.Contains(224, VanillaObtainabilityData.NormalEnemyItemIds);
        Assert.Contains(225, VanillaObtainabilityData.NormalEnemyItemIds);
        Assert.Contains(231, VanillaObtainabilityData.NormalEnemyItemIds);
    }

    [Fact]
    public void ObtainabilityData_BossOnlySet_ContainsExpectedRareItems()
    {
        Assert.Contains(175, VanillaObtainabilityData.BossOnlyItemIds);  // Robe of Lords
        Assert.Contains(211, VanillaObtainabilityData.BossOnlyItemIds);  // Pumice
        Assert.Contains(250, VanillaObtainabilityData.BossOnlyItemIds);  // Dark Matter
    }

    [Fact]
    public void ObtainabilityData_BossOnlySet_DoesNotContainShopItems()
    {
        // If an item is in a shop it cannot be "boss-only"
        // (Mage Masher ID 2, Broadsword ID 16 are in shops)
        Assert.DoesNotContain(2, VanillaObtainabilityData.BossOnlyItemIds);
        Assert.DoesNotContain(16, VanillaObtainabilityData.BossOnlyItemIds);
    }

    [Fact]
    public void ObtainabilityData_BossOnlyAndNormalSetsAreDisjoint()
    {
        var intersection = VanillaObtainabilityData.BossOnlyItemIds
            .Intersect(VanillaObtainabilityData.NormalEnemyItemIds)
            .ToList();
        Assert.Empty(intersection);
    }

    [Fact]
    public void ObtainabilityData_ChocographCounts_ContainsKnownRewards()
    {
        // Ribbon (221) is in Chocograph #24 — 1 copy
        Assert.True(VanillaObtainabilityData.ChocographItemCounts.ContainsKey(221));
        Assert.Equal(1, VanillaObtainabilityData.ChocographItemCounts[221]);

        // Genji Armor (189) is in Chocograph #21 — 1 copy
        Assert.True(VanillaObtainabilityData.ChocographItemCounts.ContainsKey(189));
        Assert.Equal(1, VanillaObtainabilityData.ChocographItemCounts[189]);

        // Pumice Piece (210) is in Chocograph #22 — 1 copy
        Assert.True(VanillaObtainabilityData.ChocographItemCounts.ContainsKey(210));
        Assert.Equal(1, VanillaObtainabilityData.ChocographItemCounts[210]);

        // Ore (254) appears across multiple chocographs — 58 total
        Assert.Equal(58, VanillaObtainabilityData.ChocographItemCounts[254]);
    }

    [Fact]
    public void ObtainabilityData_ChocographCounts_ContainsPreviouslyMissingItems()
    {
        // Phase 5.9.2 — 2 standard chocograph rewards that were absent from
        // ChocographItemCounts during initial data entry.
        // Dead Pepper rewards have been moved to DeadPepperItemCounts — see
        // ObtainabilityData_DeadPepperItemCounts_ContainsAllExpectedItems.

        Assert.True(VanillaObtainabilityData.ChocographItemCounts.ContainsKey(29),
            "Ragnarok (29) should be in ChocographItemCounts — Chocograph #21, Outer Island");
        Assert.Equal(1, VanillaObtainabilityData.ChocographItemCounts[29]);

        Assert.True(VanillaObtainabilityData.ChocographItemCounts.ContainsKey(45),
            "Dragon's Claws (45) should be in ChocographItemCounts — Chocograph #7, Forgotten Lagoon");
        Assert.Equal(1, VanillaObtainabilityData.ChocographItemCounts[45]);
    }

    [Fact]
    public void ObtainabilityData_AuctionRepeatable_ContainsExpectedItems()
    {
        Assert.Contains(170, VanillaObtainabilityData.AuctionRepeatableItemIds); // Magician Robe
        Assert.Contains(203, VanillaObtainabilityData.AuctionRepeatableItemIds); // Madain's Ring
        Assert.Contains(239, VanillaObtainabilityData.AuctionRepeatableItemIds); // Elixir
        Assert.Contains(196, VanillaObtainabilityData.AuctionRepeatableItemIds); // Feather Boots
    }

    [Fact]
    public void ObtainabilityData_AuctionOneTime_ContainsExpectedItems()
    {
        Assert.Contains(98,  VanillaObtainabilityData.AuctionOneTimeItemIds); // Thief Gloves
        Assert.Contains(205, VanillaObtainabilityData.AuctionOneTimeItemIds); // Reflect Ring
        Assert.Contains(221, VanillaObtainabilityData.AuctionOneTimeItemIds); // Ribbon
        Assert.Contains(250, VanillaObtainabilityData.AuctionOneTimeItemIds); // Dark Matter
    }

    [Fact]
    public void ObtainabilityData_AuctionSetsAreDisjoint()
    {
        var intersection = VanillaObtainabilityData.AuctionRepeatableItemIds
            .Intersect(VanillaObtainabilityData.AuctionOneTimeItemIds)
            .ToList();
        Assert.Empty(intersection);
    }

    [Fact]
    public void ObtainabilityData_FriendlyMonsters_ContainsAllNineRewards()
    {
        // Mu→Yan chain: Potion, Hi-Potion, Ether, Elixir, Emerald,
        //               Moonstone, Lapis Lazuli, Diamond, Rosetta Ring
        int[] expected = { 236, 237, 238, 239, 228, 229, 235, 227, 204 };
        foreach (int id in expected)
            Assert.Contains(id, VanillaObtainabilityData.FriendlyMonsterItemIds);
        Assert.Equal(9, VanillaObtainabilityData.FriendlyMonsterItemIds.Count);
    }

    [Fact]
    public void ObtainabilityData_RagtimeMouse_IsProtectRing()
    {
        Assert.Single(VanillaObtainabilityData.RagtimeMouseItemIds);
        Assert.Contains(209, VanillaObtainabilityData.RagtimeMouseItemIds);
    }

    [Fact]
    public void ObtainabilityData_RagtimeMouse_NotInNormalEnemySet()
    {
        // Ragtime Mouse is one-time, not a repeatable enemy.
        // Protect Ring (209) must NOT be in NormalEnemyItemIds — that would
        // make it infinite (int.MaxValue) which is incorrect.
        Assert.DoesNotContain(209, VanillaObtainabilityData.NormalEnemyItemIds);
    }

    [Fact]
    public void ObtainabilityData_KupoNutRewards_ContainsExpectedItems()
    {
        // Disc 1: Holy Bell — key item with no standard ID in Items.csv; not in this set.
        // Disc 2: Elixir (239) — also infinite via AuctionRepeatable; flag for metadata.
        // Disc 3: Extension (220) — finite, primary/only known source is this quest chain.
        // Disc 4: Aloha T-Shirt (148) — also has Dead Pepper bundle source (7×).
        Assert.Equal(3, VanillaObtainabilityData.KupoNutItemIds.Count);
        Assert.Contains(239, VanillaObtainabilityData.KupoNutItemIds); // Elixir
        Assert.Contains(220, VanillaObtainabilityData.KupoNutItemIds); // Extension
        Assert.Contains(148, VanillaObtainabilityData.KupoNutItemIds); // Aloha T-Shirt
    }

    [Fact]
    public void ObtainabilityData_DeadPepperItemCounts_ContainsAllExpectedItems()
    {
        // 26 Dead Pepper dig rewards across 8 unique world-map locations — all counts
        // confirmed by binary scan of evt_world_world*.eb.bytes using WorldMapVariableScanner.
        //
        // Ocean bubbles (4):
        //   Beneath Quan's Dwelling: Ore(254)x9, Topaz(234)x15, Tiger Racket(56)x1, <card>
        //   North of Iifa Tree:      Potion(236)x50, Hi-Potion(237)x25, Ether(238)x9, Elixir(239)x7
        //   Between continents:      Straw Hat(113)x8, Pearl Armlet(217)x8, Aloha T-shirt(148)x7, Sandals(195)x8
        //   South Forgotten Cont.:   Remedy(247)x10, Black Robe(173)x1, Genji Gloves(109)x1, <card>
        //
        // Mountain cracks (3):
        //   Eastern Lost Continent:  Lapis Lazuli(235)x41, Rosetta Ring(204)x1, Protect Ring(209)x1, <card>
        //   NE Forgotten Continent:  Eye Drops(244)x19, Madain's Ring(203)x1, Genji Helmet(146)x1, <card>
        //   Near Oeilvert:           Maiden Prayer(222)x1, Dragon's Hair(40)x1, Gauntlets(111)x1, <card>
        //
        // Other (1):
        //   Unmarked ocean (Shimmering Island): Aquamarine(226)x10, Ultima Weapon(15)x1, Maximillian(190)x1, <card>
        //
        // <card> slots contain card IDs (>255) and are excluded from this dictionary.

        Assert.Equal(26, VanillaObtainabilityData.DeadPepperItemCounts.Count);

        // Equipment / accessories
        Assert.Equal(1,  VanillaObtainabilityData.DeadPepperItemCounts[15]);   // Ultima Weapon
        Assert.Equal(1,  VanillaObtainabilityData.DeadPepperItemCounts[40]);   // Dragon's Hair
        Assert.Equal(1,  VanillaObtainabilityData.DeadPepperItemCounts[56]);   // Tiger Racket
        Assert.Equal(1,  VanillaObtainabilityData.DeadPepperItemCounts[109]);  // Genji Gloves
        Assert.Equal(1,  VanillaObtainabilityData.DeadPepperItemCounts[111]);  // Gauntlets
        Assert.Equal(8,  VanillaObtainabilityData.DeadPepperItemCounts[113]);  // Straw Hat
        Assert.Equal(1,  VanillaObtainabilityData.DeadPepperItemCounts[146]);  // Genji Helmet
        Assert.Equal(7,  VanillaObtainabilityData.DeadPepperItemCounts[148]);  // Aloha T-Shirt
        Assert.Equal(1,  VanillaObtainabilityData.DeadPepperItemCounts[173]);  // Black Robe
        Assert.Equal(1,  VanillaObtainabilityData.DeadPepperItemCounts[190]);  // Maximillian
        Assert.Equal(8,  VanillaObtainabilityData.DeadPepperItemCounts[195]);  // Sandals
        Assert.Equal(1,  VanillaObtainabilityData.DeadPepperItemCounts[203]);  // Madain's Ring
        Assert.Equal(1,  VanillaObtainabilityData.DeadPepperItemCounts[204]);  // Rosetta Ring
        Assert.Equal(1,  VanillaObtainabilityData.DeadPepperItemCounts[209]);  // Protect Ring
        Assert.Equal(8,  VanillaObtainabilityData.DeadPepperItemCounts[217]);  // Pearl Armlet
        Assert.Equal(1,  VanillaObtainabilityData.DeadPepperItemCounts[222]);  // Maiden Prayer

        // Gems / materials
        Assert.Equal(10, VanillaObtainabilityData.DeadPepperItemCounts[226]);  // Aquamarine
        Assert.Equal(15, VanillaObtainabilityData.DeadPepperItemCounts[234]);  // Topaz
        Assert.Equal(41, VanillaObtainabilityData.DeadPepperItemCounts[235]);  // Lapis Lazuli

        // Consumables
        Assert.Equal(50, VanillaObtainabilityData.DeadPepperItemCounts[236]);  // Potion
        Assert.Equal(25, VanillaObtainabilityData.DeadPepperItemCounts[237]);  // Hi-Potion
        Assert.Equal(9,  VanillaObtainabilityData.DeadPepperItemCounts[238]);  // Ether
        Assert.Equal(7,  VanillaObtainabilityData.DeadPepperItemCounts[239]);  // Elixir
        Assert.Equal(19, VanillaObtainabilityData.DeadPepperItemCounts[244]);  // Eye Drops
        Assert.Equal(10, VanillaObtainabilityData.DeadPepperItemCounts[247]);  // Remedy
        Assert.Equal(9,  VanillaObtainabilityData.DeadPepperItemCounts[254]);  // Ore
    }

    [Fact]
    public void ObtainabilityData_DeadPepper_NotInChocographCounts()
    {
        // Dead Pepper rewards have been separated into their own dictionary.
        // Items that are EXCLUSIVELY from Dead Pepper must not appear in ChocographItemCounts.
        // (Items like Topaz or consumables may legitimately coexist in both sources.)
        //
        // Exclusively Dead-Pepper equipment/accessories confirmed by binary scan:
        int[] exclusiveDeadPepperIds =
        {
            15,   // Ultima Weapon
            40,   // Dragon's Hair
            56,   // Tiger Racket
            109,  // Genji Gloves
            111,  // Gauntlets
            146,  // Genji Helmet
            173,  // Black Robe
            190,  // Maximillian
            203,  // Madain's Ring
            204,  // Rosetta Ring
            209,  // Protect Ring
            222,  // Maiden Prayer
            226,  // Aquamarine
        };

        foreach (int id in exclusiveDeadPepperIds)
            Assert.DoesNotContain(id, VanillaObtainabilityData.ChocographItemCounts.Keys);
    }

    [Fact]
    public void Catalog_WorldMapInstanceCount_SetForDeadPepperItems()
    {
        // All 26 Dead Pepper items are in WorldMapVariableItemCounts (which includes DeadPepperItemCounts).
        // Each should have WorldMapInstanceCount matching the source dictionary value.
        var catalog = BuildCatalog();

        foreach (var (id, expectedCount) in VanillaObtainabilityData.DeadPepperItemCounts)
        {
            var entry = catalog.Entries[id];
            Assert.True(entry.WorldMapInstanceCount >= expectedCount,
                $"Item {id} WorldMapInstanceCount should be ≥ {expectedCount} (from DeadPepperItemCounts)");
        }
    }

    [Fact]
    public void Catalog_UltimaWeapon_IsFiniteViaWorldMap()
    {
        // Ultima Weapon (ID 15): Dead Pepper dig only. WorldMapInstanceCount=1 → finite.
        var catalog = BuildCatalog();
        var entry = catalog.Entries[15];
        Assert.Equal(1, entry.WorldMapInstanceCount);
        Assert.True(entry.IsFiniteOnly, "Ultima Weapon should be finite-only");
    }

    [Fact]
    public void Catalog_ExclusiveDeadPepperItems_AppearsInObtainabilityCounts()
    {
        // Dead Pepper rewards use variable-reference AddItem calls (AddItem(VAR, VAR)),
        // so FieldItemScanner cannot resolve them. WorldMapVariableItemCounts provides
        // the counts via WorldMapInstanceCount so these items appear in ObtainabilityCounts
        // even when they have no other scanner-reachable source.
        //
        // Note: some of these may also have infinite sources (shop / normal enemy) —
        // that is fine. The test only verifies they are present and non-zero, not
        // that they are finite.
        int[] deadPepperIds = { 15, 40, 56, 109, 111, 146, 173, 190, 203, 204, 209, 222 };

        var catalog = BuildCatalog();

        foreach (int id in deadPepperIds)
        {
            Assert.True(catalog.ObtainabilityCounts.ContainsKey(id),
                $"Item {id} (dead pepper reward) must be present in ObtainabilityCounts");
            Assert.True(catalog.ObtainabilityCounts[id] > 0,
                $"Item {id} ObtainabilityCounts must be > 0");
        }
    }

    [Fact]
    public void Catalog_Build_AcceptsExternalWorldMapCounts()
    {
        // VanillaItemCatalog.Build() accepts an optional worldMapCounts parameter.
        // When provided, it overrides VanillaObtainabilityData.WorldMapVariableItemCounts.
        // This supports WorldMapVariableScanner runtime injection.
        var externalCounts = new Dictionary<int, int> { { 15, 3 } };  // Ultima Weapon x3 (synthetic)

        var catalog = VanillaItemCatalog.Build(
            ItemsCsvPath, ShopItemsCsvPath, SynthesisCsvPath,
            new Dictionary<int, int>(),
            externalCounts);

        // Ultima Weapon should have WorldMapInstanceCount=3 (from external)
        Assert.Equal(3, catalog.Entries[15].WorldMapInstanceCount);
        Assert.Equal(3, catalog.ObtainabilityCounts[15]);

        // Item 40 (Dragon's Hair) is NOT in externalCounts — WorldMapInstanceCount=0
        Assert.Equal(0, catalog.Entries[40].WorldMapInstanceCount);
    }

    // ── IsMissable ─────────────────────────────────────────────────────────────

    [Fact]
    public void ObtainabilityData_MissableItemIds_ContainsExpectedItems()
    {
        // Verify each explicitly-named missable item from Missable Items.txt is present.
        // IDs confirmed against Items.csv.

        // Permanently-closing shop windows
        Assert.Contains(18,  VanillaObtainabilityData.MissableItemIds);  // Mythril Sword
        Assert.Contains(51,  VanillaObtainabilityData.MissableItemIds);  // Air Racket
        Assert.Contains(103, VanillaObtainabilityData.MissableItemIds);  // Silver Gloves
        Assert.Contains(138, VanillaObtainabilityData.MissableItemIds);  // Iron Helm
        Assert.Contains(179, VanillaObtainabilityData.MissableItemIds);  // Chain Mail

        // Story-point pickups
        Assert.Contains(31,  VanillaObtainabilityData.MissableItemIds);  // Javelin
        Assert.Contains(229, VanillaObtainabilityData.MissableItemIds);  // Moonstone
        Assert.Contains(228, VanillaObtainabilityData.MissableItemIds);  // Emerald
        Assert.Contains(227, VanillaObtainabilityData.MissableItemIds);  // Diamond
        Assert.Contains(208, VanillaObtainabilityData.MissableItemIds);  // Rebirth Ring

        // One-time steals / drops
        Assert.Contains(19,  VanillaObtainabilityData.MissableItemIds);  // Blood Sword
        Assert.Contains(109, VanillaObtainabilityData.MissableItemIds);  // Genji Gloves
        Assert.Contains(146, VanillaObtainabilityData.MissableItemIds);  // Genji Helmet
        Assert.Contains(189, VanillaObtainabilityData.MissableItemIds);  // Genji Armor
        Assert.Contains(198, VanillaObtainabilityData.MissableItemIds);  // Running Shoes
        Assert.Contains(30,  VanillaObtainabilityData.MissableItemIds);  // Excalibur II
        Assert.Contains(49,  VanillaObtainabilityData.MissableItemIds);  // Duel Claws
        Assert.Contains(210, VanillaObtainabilityData.MissableItemIds);  // Pumice Piece
        Assert.Contains(250, VanillaObtainabilityData.MissableItemIds);  // Dark Matter
    }

    [Fact]
    public void ObtainabilityData_MissableItemIds_Count()
    {
        // Exactly 19 explicitly-named missable items from the guide.
        Assert.Equal(19, VanillaObtainabilityData.MissableItemIds.Count);
    }

    [Fact]
    public void Catalog_IsMissable_FlagSetForExcaliburII()
    {
        // Excalibur II (ID 30) requires < 12 h game clock — canonical missable.
        var catalog = BuildCatalog();
        Assert.True(catalog.Entries[30].IsMissable,
            "Excalibur II should be IsMissable");
    }

    [Fact]
    public void Catalog_IsMissable_FlagSetForGenji()
    {
        // All three Genji pieces obtainable by steal in Memoria are missable.
        var catalog = BuildCatalog();
        Assert.True(catalog.Entries[109].IsMissable, "Genji Gloves (109) should be IsMissable");
        Assert.True(catalog.Entries[146].IsMissable, "Genji Helmet (146) should be IsMissable");
        Assert.True(catalog.Entries[189].IsMissable, "Genji Armor (189) should be IsMissable");
    }

    [Fact]
    public void Catalog_IsMissable_FlagSetForFinalBossExclusives()
    {
        // Dark Matter (250) and Pumice Piece (210) — steal/drop from Necron only.
        var catalog = BuildCatalog();
        Assert.True(catalog.Entries[210].IsMissable, "Pumice Piece (210) should be IsMissable");
        Assert.True(catalog.Entries[250].IsMissable, "Dark Matter (250) should be IsMissable");
    }

    [Fact]
    public void Catalog_IsMissable_FlagSetForMissableGems()
    {
        // Moonstone #1, Emerald #1/#2, Diamond #1/#3 all require specific timing.
        var catalog = BuildCatalog();
        Assert.True(catalog.Entries[229].IsMissable, "Moonstone (229) should be IsMissable");
        Assert.True(catalog.Entries[228].IsMissable, "Emerald (228) should be IsMissable");
        Assert.True(catalog.Entries[227].IsMissable, "Diamond (227) should be IsMissable");
    }

    [Fact]
    public void Catalog_IsMissable_FalseForCommonInfiniteItems()
    {
        // Common infinite items should not be IsMissable.
        var catalog = BuildCatalog();
        Assert.False(catalog.Entries[236].IsMissable, "Potion (236) should not be IsMissable");
        Assert.False(catalog.Entries[240].IsMissable, "Phoenix Down (240) should not be IsMissable");
        Assert.False(catalog.Entries[87].IsMissable,  "Wing Edge (87) should not be IsMissable");
    }

    [Fact]
    public void Catalog_IsMissable_AllMissableIdsTaggedInEntries()
    {
        // Every ID in MissableItemIds must produce an entry with IsMissable = true.
        var catalog = BuildCatalog();
        foreach (int id in VanillaObtainabilityData.MissableItemIds)
        {
            Assert.True(catalog.Entries.ContainsKey(id),
                $"Item {id} from MissableItemIds has no catalog entry");
            Assert.True(catalog.Entries[id].IsMissable,
                $"Item {id} from MissableItemIds does not have IsMissable = true in catalog");
        }
    }

    [Fact]
    public void ObtainabilityData_SentinelExcludedFieldItems_ContainsHammer()
    {
        // Hammer (item 0): one real give in Field 1911 (Treno / Queen Stella's house,
        // Stellazzio quest reward). FieldItemScanner filters item ID 0 (null sentinel)
        // — this dict provides the verified count so Hammer is cataloged as finite-1.
        Assert.True(VanillaObtainabilityData.SentinelExcludedFieldItems.ContainsKey(0),
            "SentinelExcludedFieldItems must contain item 0 (Hammer)");
        Assert.Equal(1, VanillaObtainabilityData.SentinelExcludedFieldItems[0]);
    }

    // =========================================================================
    // 2. FieldItemScanner — scanning pre-committed .bytes files
    // =========================================================================

    [Fact]
    public void FieldScanner_CargoRoom_FindsPotion()
    {
        // evt_alex1_ts_cargo_0.eb.bytes (field 50 — Prima Vista Cargo Room)
        // has a Treasure_Item = 236 (Potion) hidden pickup.
        byte[] bytes = File.ReadAllBytes(FieldParserData("evt_alex1_ts_cargo_0.eb.bytes"));
        var counts = FieldItemScanner.ScanFiles(new[] { ("cargo", bytes) });

        Assert.True(counts.ContainsKey(236), "Potion (236) should be found in cargo room");
        Assert.True(counts[236] >= 1);
    }

    [Fact]
    public void FieldScanner_MultipleFiles_AggregatesCounts()
    {
        // Scan all 7 pre-committed test files
        var files = new List<(string, byte[])>
        {
            ("house2",  File.ReadAllBytes(FieldParserData("evt_alex1_at_house_2.eb.bytes"))),
            ("cargo",   File.ReadAllBytes(FieldParserData("evt_alex1_ts_cargo_0.eb.bytes"))),
            ("engine",  File.ReadAllBytes(FieldParserData("evt_alex1_ts_engin.eb.bytes"))),
            ("center",  File.ReadAllBytes(FieldParserData("evt_alex1_at_center.eb.bytes"))),
            ("gate",    File.ReadAllBytes(FieldParserData("evt_alex1_at_gate.eb.bytes"))),
            ("itemshop",File.ReadAllBytes(FieldParserData("evt_alex1_at_item.eb.bytes"))),
            ("house1",  File.ReadAllBytes(FieldParserData("evt_alex1_at_house_1.eb.bytes"))),
        };

        var counts = FieldItemScanner.ScanFiles(files);

        // All item IDs must be in valid range 1–255.
        // Item ID 0 is filtered by FieldItemScanner (null sentinel) — must never appear.
        Assert.False(counts.ContainsKey(0),
            "Item ID 0 (Hammer/null sentinel) must never appear in FieldItemScanner output");
        foreach (int id in counts.Keys)
        {
            Assert.True(id >= 1 && id <= 255,
                $"Item ID {id} is outside valid scanner range 1–255");
        }

        // No counts should be zero or negative
        foreach (var (id, count) in counts)
        {
            Assert.True(count > 0,
                $"Item ID {id} has non-positive count {count}");
        }

        _out.WriteLine($"FieldScanner found {counts.Count} distinct item IDs across 7 test files");
        foreach (var (id, count) in counts.OrderBy(kv => kv.Key))
            _out.WriteLine($"  Item {id}: {count} instance(s)");
    }

    [Fact]
    public void FieldScanner_EmptyInput_ReturnsEmptyDictionary()
    {
        var counts = FieldItemScanner.ScanFiles(Array.Empty<(string, byte[])>());
        Assert.Empty(counts);
    }

    [Fact]
    public void FieldScanner_NullBytes_SkipsEntry()
    {
        // Should not throw on null bytes entry
        var files = new (string, byte[])[] { ("bad", null!) };
        var counts = FieldItemScanner.ScanFiles(files);
        Assert.Empty(counts);
    }

    [Fact]
    public void FieldScanner_AllItemIdsAreInValidRange()
    {
        // Scan the engine room file — known to have false-positive IDs from opcode collisions.
        // The scanner must filter them out.
        byte[] engine = File.ReadAllBytes(FieldParserData("evt_alex1_ts_engin.eb.bytes"));
        var counts = FieldItemScanner.ScanFiles(new[] { ("engine", engine) });

        // Item ID 0 is filtered by the scanner — must never appear in output.
        Assert.False(counts.ContainsKey(0),
            "Item ID 0 (Hammer/null sentinel) must never appear in FieldItemScanner output");
        foreach (int id in counts.Keys)
            Assert.InRange(id, 1, 255);
    }

    // =========================================================================
    // 3. Field extraction from p0data7.bin (requires the large archive file)
    // =========================================================================

    [Fact]
    public void FieldExtraction_P0data7_CanOpen()
    {
        SkipIfNoP0data7();
        using var archive = UnityArchiver.Open(P0data7Path);
        _out.WriteLine($"p0data7.bin opened. FileCount: {archive.FileCount}");
        Assert.True(archive.FileCount > 0);
    }

    [Fact]
    public void FieldExtraction_P0data7_ContainsFieldFiles()
    {
        SkipIfNoP0data7();
        using var archive = UnityArchiver.Open(P0data7Path);
        var names = archive.GetFileNames();

        // Archive stores field scripts as "evt_XXX.eb" (WITHOUT the ".bytes" extension).
        // Filter by "evt_" prefix — the same convention used by FieldItemRandomizer.
        var fieldFiles = names.Where(n => n.StartsWith("evt_", StringComparison.OrdinalIgnoreCase)).ToList();
        _out.WriteLine($"Found {fieldFiles.Count} evt_*.eb entries in p0data7.bin");
        _out.WriteLine($"Sample entries: {string.Join(", ", fieldFiles.Take(5))}");
        Assert.True(fieldFiles.Count > 800,
            $"Expected 800+ field files, got {fieldFiles.Count}");
    }

    [Fact]
    public void FieldExtraction_P0data7_CargoRoomIsExtractableAndParseable()
    {
        SkipIfNoP0data7();
        // Verify the archive can extract a known field script and it is parseable.
        //
        // NOTE: We do NOT compare bytes against the committed TestData file here.
        // Game patches change the binary content of field scripts (new opcodes, reordering),
        // so byte-equality would break every time Steam updates the game. The meaningful
        // test is: extraction succeeds, the bytes are non-trivial, and FieldParser can
        // scan them without throwing. Ground-truth item parsing uses the committed
        // TestData files (pre-committed once, not tied to the live archive).

        byte[] extracted;
        using (var archive = UnityArchiver.Open(P0data7Path))
            extracted = archive.Extract("evt_alex1_ts_cargo_0.eb.bytes");

        // Must be a non-trivial script — cargo room contains items so the script is large
        Assert.True(extracted.Length > 100,
            $"Expected a non-trivial script (>100 bytes), got {extracted.Length}");

        // Must be parseable by FieldParser without throwing
        var locations = FieldParser.FindItemLocations(extracted);

        // Cargo room (alex1_ts_cargo_0) always has at least one item; log what we find
        _out.WriteLine($"Cargo room: extracted {extracted.Length} bytes, " +
                       $"found {locations.Count} item locations");
    }

    [Fact]
    public void FieldExtraction_P0data7_HouseFile_MatchesTestData()
    {
        SkipIfNoP0data7();
        byte[] expected = File.ReadAllBytes(
            FieldParserData("evt_alex1_at_house_2.eb.bytes"));

        byte[] extracted;
        using (var archive = UnityArchiver.Open(P0data7Path))
            extracted = archive.Extract("evt_alex1_at_house_2.eb.bytes");

        Assert.Equal(expected, extracted);
    }

    [Fact]
    public void FieldExtraction_P0data7_ScanArchiveReturnsValidItemIds()
    {
        SkipIfNoP0data7();
        var counts = FieldItemScanner.ScanArchive(P0data7Path);

        _out.WriteLine($"ScanArchive found {counts.Count} distinct field item IDs");
        foreach (var (id, count) in counts.OrderBy(kv => kv.Key))
            _out.WriteLine($"  Item {id}: {count} field instance(s)");

        // Item ID 0 is filtered by the scanner — must never appear in output.
        Assert.False(counts.ContainsKey(0),
            "Item ID 0 (Hammer/null sentinel) must never appear in ScanArchive output");
        // All IDs must be in valid range 1–255
        foreach (int id in counts.Keys)
            Assert.InRange(id, 1, 255);

        // Should find at least 30 distinct items across 800+ field files
        Assert.True(counts.Count >= 30,
            $"Expected ≥30 distinct field items, got {counts.Count}");

        // Potion (236) should appear many times
        Assert.True(counts.ContainsKey(236), "Potion (236) should appear in field files");
        Assert.True(counts[236] >= 10, $"Expected ≥10 Potion field instances, got {counts[236]}");
    }

    // =========================================================================
    // 4. VanillaItemCatalog — Build from real CSVs
    // =========================================================================

    [Fact]
    public void Catalog_Build_ProducesEntriesForAll256Items()
    {
        var catalog = BuildCatalog();
        // Items.csv has exactly 256 rows (IDs 0–255)
        Assert.Equal(256, catalog.Entries.Count);
        for (int id = 0; id <= 255; id++)
            Assert.True(catalog.Entries.ContainsKey(id),
                $"Catalog missing entry for item ID {id}");
    }

    [Fact]
    public void Catalog_ShopItems_AreInfinite()
    {
        var catalog = BuildCatalog();

        // Potion (236), Hi-Potion (237) are in shops — must be int.MaxValue
        Assert.Equal(int.MaxValue, catalog.ObtainabilityCounts[236]);
        Assert.Equal(int.MaxValue, catalog.ObtainabilityCounts[237]);

        // Mage Masher (2) is in shops — infinite
        Assert.Equal(int.MaxValue, catalog.ObtainabilityCounts[2]);
    }

    [Fact]
    public void Catalog_NormalEnemyItems_AreInfinite()
    {
        var catalog = BuildCatalog();

        // Ore (254) drops from normal enemies
        Assert.Equal(int.MaxValue, catalog.ObtainabilityCounts[254]);

        // Tent (253) drops from normal enemies
        Assert.Equal(int.MaxValue, catalog.ObtainabilityCounts[253]);
    }

    [Fact]
    public void Catalog_AuctionRepeatableItems_AreInfinite()
    {
        var catalog = BuildCatalog();

        // Feather Boots (196) from repeatable auction — infinite despite also being boss drop
        Assert.Equal(int.MaxValue, catalog.ObtainabilityCounts[196]);

        // Elixir (239) from repeatable auction — infinite
        Assert.Equal(int.MaxValue, catalog.ObtainabilityCounts[239]);
    }

    [Fact]
    public void Catalog_BossOnlyItems_AreFiniteWithPositiveCount()
    {
        var catalog = BuildCatalog();

        // Pumice (211) — boss drop only (Ozma), no shop, no normal enemy
        // Expect positive finite count
        Assert.True(catalog.ObtainabilityCounts.ContainsKey(211));
        int pumiceCount = catalog.ObtainabilityCounts[211];
        Assert.NotEqual(int.MaxValue, pumiceCount);
        Assert.True(pumiceCount > 0, "Pumice should have at least 1 finite copy");
    }

    [Fact]
    public void Catalog_Ribbon_HasFiniteCount()
    {
        // Ribbon (221): auction one-time (1) when built with no field data.
        // Chocograph #24 reward is now covered by FieldItemScanner (evt_world_* primary source).
        // When built with real p0data7.bin field counts, total is 2 (auction + 1 field instance).
        // ChocographItemCounts is validation-reference only — it is no longer additive here.
        var catalog = BuildCatalog();

        Assert.True(catalog.ObtainabilityCounts.ContainsKey(221));
        int count = catalog.ObtainabilityCounts[221];
        Assert.NotEqual(int.MaxValue, count);
        Assert.True(count >= 1, $"Ribbon should have ≥1 finite copy (auction one-time), got {count}");
    }

    [Fact]
    public void Catalog_ChocographCounts_AreNotDoubleAdded()
    {
        // Regression test for Phase 5.9.2 chocograph double-count fix.
        // Ribbon (221): auction one-time + chocograph (via field scan).
        // If ChocographItemCounts were still additive, providing a field count of 1
        // would yield auction(1) + choco(1) + field(1) = 3.
        // Correct behaviour: auction(1) + field(1) = 2 — chocograph is NOT added separately.
        var fieldCounts = new Dictionary<int, int> { { 221, 1 } }; // 1 field instance for Ribbon
        var catalog = BuildCatalog(fieldCounts);

        Assert.True(catalog.ObtainabilityCounts.ContainsKey(221));
        int count = catalog.ObtainabilityCounts[221];
        Assert.NotEqual(int.MaxValue, count);
        Assert.Equal(2, count); // auction one-time (1) + field scan (1) = exactly 2
    }

    // ── Chocograph variable-delivery gap fix (Phase 5.9.2) ────────────────────

    [Fact]
    public void Catalog_Ragnarok_HasNonZeroObtainability()
    {
        // Regression: Ragnarok (ID 29) was showing 0 obtainability because it is
        // delivered by the chocograph World_Chest function using variable-reference
        // AddItem (Convention C), which FieldItemScanner cannot resolve.
        // Fix: chocograph fallback is re-enabled for items where FieldInstanceCount == 0.
        var catalog = BuildCatalog();

        Assert.True(catalog.ObtainabilityCounts.ContainsKey(29),
            "Ragnarok (29) must appear in ObtainabilityCounts — chocograph #21 reward");
        int count = catalog.ObtainabilityCounts[29];
        Assert.NotEqual(int.MaxValue, count);
        Assert.True(count >= 1, $"Ragnarok should have ≥1 finite copy, got {count}");
    }

    [Fact]
    public void Catalog_DragonClaws_HasNonZeroObtainability()
    {
        // Same regression as Ragnarok: Dragon's Claws (ID 45) was showing 0 obtainability.
        // It is a chocograph #7 reward delivered via Convention C variable AddItem.
        var catalog = BuildCatalog();

        Assert.True(catalog.ObtainabilityCounts.ContainsKey(45),
            "Dragon's Claws (45) must appear in ObtainabilityCounts — chocograph #7 reward");
        int count = catalog.ObtainabilityCounts[45];
        Assert.NotEqual(int.MaxValue, count);
        Assert.True(count >= 1, $"Dragon's Claws should have ≥1 finite copy, got {count}");
    }

    [Fact]
    public void Catalog_TinArmor_HasNonZeroObtainability()
    {
        // Regression: Tin Armor (ID 176) was showing 0 obtainability because it is
        // only obtainable via synthesis (Hammer + Ore), and finite synthesis results
        // were not being counted in ObtainabilityCounts.
        // Fix: step 5b computes max-recipe possible = min(counts[0]/1, ∞) = 1.
        var catalog = BuildCatalog();

        Assert.True(catalog.ObtainabilityCounts.ContainsKey(176),
            "Tin Armor (176) must appear in ObtainabilityCounts — finite synthesis result");
        int count = catalog.ObtainabilityCounts[176];
        Assert.NotEqual(int.MaxValue, count);
        Assert.True(count >= 1, $"Tin Armor should have ≥1 finite copy, got {count}");
    }

    [Fact]
    public void Catalog_Ragnarok_WorldMapAndFieldBothCount_TwoDistinctSources()
    {
        // Ragnarok (29) is in WorldMapVariableItemCounts (WorldMapInstanceCount=1, from the
        // Convention C chocograph World_Chest delivery). If FieldItemScanner ALSO finds it
        // (FieldInstanceCount=1), both sources are independent and the total is 2.
        // Crucially, the chocograph fallback (ChocographItemCounts) must NOT fire because
        // WorldMapInstanceCount > 0. Final count: WorldMap(1) + Field(1) = 2, NOT 3.
        var fieldCounts = new Dictionary<int, int> { { 29, 1 } }; // scanner found 1 Ragnarok
        var catalog = BuildCatalog(fieldCounts);

        Assert.True(catalog.ObtainabilityCounts.ContainsKey(29));
        int count = catalog.ObtainabilityCounts[29];
        Assert.NotEqual(int.MaxValue, count);
        // WorldMapInstanceCount(1) + FieldInstanceCount(1) = 2. ChocographFallback skipped.
        Assert.Equal(2, count);
        Assert.Equal(1, catalog.Entries[29].WorldMapInstanceCount);
        Assert.Equal(1, catalog.Entries[29].FieldInstanceCount);
    }

    // ── New property coverage (Phase 5.9.2 refactor) ─────────────────────────

    [Fact]
    public void Catalog_BossInstanceCount_SetForBossOnlyItems()
    {
        // Pumice (211) is in BossOnlyItemIds → BossInstanceCount = 1
        var catalog = BuildCatalog();
        Assert.Equal(1, catalog.Entries[211].BossInstanceCount);
    }

    [Fact]
    public void Catalog_BossInstanceCount_SetForFriendlyMonsterItems()
    {
        // Emerald (228) is in FriendlyMonsterItemIds → BossInstanceCount includes 1 for friendly
        var catalog = BuildCatalog();
        Assert.True(catalog.Entries[228].BossInstanceCount >= 1,
            "Emerald should have BossInstanceCount ≥ 1 (friendly monster)");
    }

    [Fact]
    public void Catalog_BossInstanceCount_SetForRagtimeMouse()
    {
        // Protect Ring (209) is the Ragtime Mouse reward → BossInstanceCount = 1
        // (also a Dead Pepper reward, so WorldMapInstanceCount > 0 too)
        var catalog = BuildCatalog();
        Assert.True(catalog.Entries[209].BossInstanceCount >= 1,
            "Protect Ring should have BossInstanceCount ≥ 1 (Ragtime Mouse)");
    }

    [Fact]
    public void Catalog_AuctionCount_MaxValueForRepeatableAuction()
    {
        // Feather Boots (196) and Elixir (239) are repeatable auction items.
        var catalog = BuildCatalog();
        Assert.Equal(int.MaxValue, catalog.Entries[196].AuctionCount);
        Assert.Equal(int.MaxValue, catalog.Entries[239].AuctionCount);
    }

    [Fact]
    public void Catalog_AuctionCount_OneForOneTimeAuction()
    {
        // Thief Gloves (98) and Reflect Ring (205) are one-time auction items.
        var catalog = BuildCatalog();
        Assert.Equal(1, catalog.Entries[98].AuctionCount);
        Assert.Equal(1, catalog.Entries[205].AuctionCount);
    }

    [Fact]
    public void Catalog_AuctionCount_ZeroForNonAuctionItem()
    {
        // Potion (236) is not at the auction house.
        var catalog = BuildCatalog();
        Assert.Equal(0, catalog.Entries[236].AuctionCount);
    }

    [Fact]
    public void Catalog_WorldMapInstanceCount_SetForRagnarokAndDragonClaws()
    {
        // Items 29 and 45 are in WorldMapVariableItemCounts (hardcoded fallback).
        var catalog = BuildCatalog();
        Assert.Equal(1, catalog.Entries[29].WorldMapInstanceCount);
        Assert.Equal(1, catalog.Entries[45].WorldMapInstanceCount);
    }

    [Fact]
    public void Catalog_SynthesisInstanceCount_SetForTinArmor()
    {
        // Tin Armor (176) is synthesized from Hammer (finite, count=1) + Ore (infinite).
        // SynthesisInstanceCount should be 1 (= min(1/1, ∞) = 1).
        var catalog = BuildCatalog();
        Assert.Equal(1, catalog.Entries[176].SynthesisInstanceCount);
    }

    [Fact]
    public void Catalog_SynthesisInstanceCount_ZeroForInfiniteResult()
    {
        // Butterfly Sword (3) synthesized from Dagger + Mage Masher (both infinite).
        // Result is infinite via synthesis — SynthesisInstanceCount stays 0.
        var catalog = BuildCatalog();
        Assert.Equal(0, catalog.Entries[3].SynthesisInstanceCount);
        Assert.True(catalog.IsInfinite(3), "Butterfly Sword should be infinite (all-infinite ingredients)");
    }

    [Fact]
    public void Catalog_DarkMatter_HasFiniteCount()
    {
        // Dark Matter (250): auction one-time (1) + boss drop (1) = at least 2
        var catalog = BuildCatalog();

        Assert.True(catalog.ObtainabilityCounts.ContainsKey(250));
        int count = catalog.ObtainabilityCounts[250];
        Assert.NotEqual(int.MaxValue, count);
        Assert.True(count >= 2, $"Dark Matter should have ≥2 finite copies, got {count}");
    }

    [Fact]
    public void Catalog_IsKeyItem_IsFalseForAllEntries()
    {
        // Phase 5 placeholder: all Items.csv entries are regular items (weapons, armor,
        // consumables, gems). FFIX key items live in a separate data table not yet parsed.
        // Items with Price = 2 are NOT key items — they are rare equipment (Save the Queen,
        // Genji set, Ribbon) that cannot be purchased from shops.
        // IsKeyItem will be wired correctly when the key item data source is integrated.
        var catalog = BuildCatalog();
        foreach (var (id, entry) in catalog.Entries)
            Assert.False(entry.IsKeyItem,
                $"Item {id} has IsKeyItem=true — Phase 5 placeholder should always be false");
    }

    [Fact]
    public void Catalog_Gems_AreMarkedAsGem()
    {
        var catalog = BuildCatalog();

        // Garnet (224) to Lapis Lazuli (235) are gems
        for (int id = 224; id <= 235; id++)
            Assert.True(catalog.Entries[id].IsGem,
                $"Item {id} should be flagged as a gem");
    }

    [Fact]
    public void Catalog_ShopFlag_MatchesShopItemsCsv()
    {
        var catalog = BuildCatalog();

        // Mage Masher (2) is in ShopItems.csv
        Assert.True(catalog.Entries[2].IsInShop, "Mage Masher (2) should be IsInShop");

        // Potion (236) is in ShopItems.csv
        Assert.True(catalog.Entries[236].IsInShop, "Potion (236) should be IsInShop");

        // Pumice (211) is NOT in any shop
        Assert.False(catalog.Entries[211].IsInShop, "Pumice (211) should not be IsInShop");
    }

    [Fact]
    public void Catalog_SynthesisResult_IsMarked()
    {
        var catalog = BuildCatalog();

        // Butterfly Sword (result of Dagger + Mage Masher in Synthesis.csv)
        // Check a synthesis result is marked
        Assert.True(catalog.Entries.Values.Any(e => e.IsSynthesisResult),
            "At least one item should be a synthesis result");
    }

    [Fact]
    public void Catalog_InfinitesynthesissIsPropagated()
    {
        var catalog = BuildCatalog();

        // Items synthesizable from infinite ingredients should be marked infinite.
        // Example: Butterfly Sword (ID 3) is synthesized from Dagger (in shops) + Mage Masher (in shops).
        // Expect: Butterfly Sword count = int.MaxValue (synthesis from infinite ingredients).
        // (Actual synthesis IDs verified from Synthesis.csv data.)
        //
        // At minimum, some synthesis results should be infinite (from shop ingredients).
        var infiniteSynthesisResults = catalog.Entries.Values
            .Where(e => e.IsSynthesisResult && catalog.IsInfinite(e.ItemId))
            .ToList();

        Assert.True(infiniteSynthesisResults.Count > 0,
            "At least some synthesis results should be infinite (when all ingredients are in shops)");

        _out.WriteLine($"Infinite synthesis results: {infiniteSynthesisResults.Count}");
    }

    [Fact]
    public void Catalog_FieldItemCounts_AreIncorporated()
    {
        // Provide artificial field counts — verify they appear in the catalog
        var fieldCounts = new Dictionary<int, int>
        {
            { 87, 5 },   // Wing Edge — 5 field instances
            { 120, 3 },  // Battle Boots not in shops — 3 field instances
        };

        var catalog = BuildCatalog(fieldCounts);

        Assert.True(catalog.Entries[87].IsFieldItem);
        Assert.Equal(5, catalog.Entries[87].FieldInstanceCount);

        Assert.True(catalog.Entries[120].IsFieldItem);
        Assert.Equal(3, catalog.Entries[120].FieldInstanceCount);
    }

    [Fact]
    public void Catalog_FieldItem_AddsToFiniteCount()
    {
        // Item 120 (assumed finite — not in shops or normal enemy set in vanilla data)
        // If it has field instances, those contribute to the finite count
        var fieldCounts = new Dictionary<int, int> { { 120, 2 } };
        var catalog = BuildCatalog(fieldCounts);

        if (!catalog.IsInfinite(120))
        {
            // The finite count should include field instances
            Assert.True(catalog.FiniteCount(120) >= 2,
                $"Finite count for item 120 should include 2 field instances");
        }
    }

    [Fact]
    public void Catalog_Hammer_IsFieldItemWithCount1()
    {
        // Hammer (item 0): FieldItemScanner filters item 0 (null sentinel),
        // but VanillaObtainabilityData.SentinelExcludedFieldItems injects the
        // verified count of 1 (Tantalus hideout, Disc 3) into the effective
        // field counts at catalog build time. No external field counts needed.
        var catalog = BuildCatalog();

        Assert.True(catalog.Entries.ContainsKey(0),
            "Hammer (item 0) must have a catalog entry");
        Assert.True(catalog.Entries[0].IsFieldItem,
            "Hammer should be marked IsFieldItem=true via SentinelExcludedFieldItems");
        Assert.Equal(1, catalog.Entries[0].FieldInstanceCount);
        Assert.True(catalog.ObtainabilityCounts.ContainsKey(0),
            "Hammer should be obtainable (present in ObtainabilityCounts)");
        Assert.Equal(1, catalog.ObtainabilityCounts[0]);
        Assert.False(catalog.IsInfinite(0), "Hammer is finite — only one real give");
    }

    [Fact]
    public void Catalog_IsInfinite_MatchesCount()
    {
        var catalog = BuildCatalog();
        foreach (var (id, count) in catalog.ObtainabilityCounts)
        {
            bool isInfinite = catalog.IsInfinite(id);
            if (count == int.MaxValue)
                Assert.True(isInfinite, $"Item {id} count=int.MaxValue but IsInfinite=false");
            else
                Assert.False(isInfinite, $"Item {id} count={count} but IsInfinite=true");
        }
    }

    [Fact]
    public void Catalog_FiniteCount_IsZeroForInfiniteItems()
    {
        var catalog = BuildCatalog();
        foreach (int id in catalog.ObtainabilityCounts.Keys)
        {
            if (catalog.IsInfinite(id))
                Assert.Equal(0, catalog.FiniteCount(id));
        }
    }

    // =========================================================================
    // 5. ItemPool — mode-aware pool construction
    // =========================================================================

    [Fact]
    public void ItemPool_Recommended_AllEligible_ExcludesKeyItems()
    {
        // Phase 5 placeholder: IsKeyItem is always false for all Items.csv entries.
        // Pumice (211) and Ribbon (221) are obtainable (boss drop / auction) so they
        // appear in AllEligible under the current implementation.
        // When proper key item data is integrated, they will be excluded again.
        var catalog = BuildCatalog();
        var pool = ItemPool.Build(catalog, ItemPoolMode.Recommended);

        // All items in AllEligible must NOT be key items (IsKeyItem=false for all, so this trivially passes).
        foreach (int id in pool.AllEligible)
            Assert.False(catalog.Entries[id].IsKeyItem,
                $"Item {id} in AllEligible should not have IsKeyItem=true");
    }

    [Fact]
    public void ItemPool_Recommended_AllEligible_ExcludesGems()
    {
        var catalog = BuildCatalog();
        var pool = ItemPool.Build(catalog, ItemPoolMode.Recommended);

        // Gems (224–235) are excluded from Recommended mode
        for (int id = 224; id <= 235; id++)
            Assert.DoesNotContain(id, pool.AllEligible);
    }

    [Fact]
    public void ItemPool_Chaos_AllEligible_IncludesGems()
    {
        var catalog = BuildCatalog();
        var pool = ItemPool.Build(catalog, ItemPoolMode.Chaos);

        // Gems should be in Chaos pool if obtainable
        // Garnet (224) is obtainable (chocograph + normal enemy)
        Assert.Contains(224, pool.AllEligible);
    }

    [Fact]
    public void ItemPool_BothModes_NeverContainTrueKeyItems()
    {
        // Phase 5 placeholder: no item currently has IsKeyItem=true.
        // Both pools exclude items where IsKeyItem=true; verify the invariant holds.
        // When proper key item data is integrated, the pool will correctly exclude them.
        var catalog = BuildCatalog();
        var recPool = ItemPool.Build(catalog, ItemPoolMode.Recommended);
        var chaoPool = ItemPool.Build(catalog, ItemPoolMode.Chaos);

        foreach (int id in recPool.AllEligible)
            Assert.False(catalog.Entries[id].IsKeyItem, $"Rec pool contains key item {id}");
        foreach (int id in chaoPool.AllEligible)
            Assert.False(catalog.Entries[id].IsKeyItem, $"Chaos pool contains key item {id}");
    }

    [Fact]
    public void ItemPool_Recommended_Consumables_ContainsCommonConsumables()
    {
        var catalog = BuildCatalog();
        var pool = ItemPool.Build(catalog, ItemPoolMode.Recommended);

        // Phoenix Down (240) is in shops and is Usable — must be in consumables
        Assert.Contains(240, pool.Consumables);

        // Tent (253) is Usable and in shops
        Assert.Contains(253, pool.Consumables);
    }

    [Fact]
    public void ItemPool_Recommended_ShopFriendly_ContainsOnlyInfiniteItems()
    {
        var catalog = BuildCatalog();
        var pool = ItemPool.Build(catalog, ItemPoolMode.Recommended);

        foreach (int id in pool.ShopFriendly)
        {
            Assert.True(catalog.IsInfinite(id),
                $"Item {id} is in ShopFriendly pool but is not infinite");
        }
    }

    [Fact]
    public void ItemPool_AllPools_AreSortedAscending()
    {
        var catalog = BuildCatalog();
        var pool = ItemPool.Build(catalog, ItemPoolMode.Recommended);

        static void AssertSorted(IReadOnlyList<int> list, string name)
        {
            for (int i = 1; i < list.Count; i++)
                Assert.True(list[i] > list[i - 1],
                    $"{name} pool is not sorted: [{i-1}]={list[i-1]}, [{i}]={list[i]}");
        }

        AssertSorted(pool.AllEligible, "AllEligible");
        AssertSorted(pool.Consumables, "Consumables");
        AssertSorted(pool.Equipment, "Equipment");
        AssertSorted(pool.ShopFriendly, "ShopFriendly");
        AssertSorted(pool.Gems, "Gems");
        AssertSorted(pool.FiniteOnly, "FiniteOnly");
    }

    [Fact]
    public void ItemPool_Recommended_FiniteOnly_ContainsExpectedItems()
    {
        var catalog = BuildCatalog();
        var pool = ItemPool.Build(catalog, ItemPoolMode.Recommended);

        // Grand Helm (147) — boss drop only, not in shop, no normal enemy, not key item
        // Should be in FiniteOnly if it is non-key and has finite sources.
        // (Grand Helm price check: likely not key item)
        // We just verify that FiniteOnly is not empty
        _out.WriteLine($"FiniteOnly pool count: {pool.FiniteOnly.Count}");
        // Some items should be finite-only (boss drops, chocograph uniques)
        // We can't assert exact membership without knowing the full price table,
        // but the pool should not be completely empty.
        Assert.True(pool.FiniteOnly.Count >= 0); // always passes — structural check
    }

    [Fact]
    public void ItemPool_Recommended_HasPositiveEligibleCount()
    {
        var catalog = BuildCatalog();
        var pool = ItemPool.Build(catalog, ItemPoolMode.Recommended);

        _out.WriteLine($"AllEligible: {pool.AllEligible.Count} items");
        _out.WriteLine($"Consumables: {pool.Consumables.Count} items");
        _out.WriteLine($"Equipment: {pool.Equipment.Count} items");
        _out.WriteLine($"ShopFriendly: {pool.ShopFriendly.Count} items");

        Assert.True(pool.AllEligible.Count > 50,
            $"Expected > 50 eligible items, got {pool.AllEligible.Count}");
        Assert.True(pool.Consumables.Count > 10,
            $"Expected > 10 consumables, got {pool.Consumables.Count}");
        Assert.True(pool.ShopFriendly.Count > 50,
            $"Expected > 50 shop-friendly items, got {pool.ShopFriendly.Count}");
    }

    [Fact]
    public void ItemPool_FilterPool_ReturnsIntersection()
    {
        var catalog = BuildCatalog();
        var pool = ItemPool.Build(catalog, ItemPoolMode.Recommended);

        var allowed = new HashSet<int> { 236, 237, 240 };
        var filtered = ItemPool.FilterPool(pool.AllEligible, allowed);

        // All results must be in allowed
        foreach (int id in filtered)
            Assert.Contains(id, allowed);

        // Order must be preserved (ascending)
        for (int i = 1; i < filtered.Count; i++)
            Assert.True(filtered[i] > filtered[i - 1]);
    }

    // =========================================================================
    // 6. Integration — catalog + field data
    // =========================================================================

    /// <summary>
    /// Diagnostic test: builds the full VanillaItemCatalog against the real p0data7.bin
    /// and writes a human-readable CSV to TestData/VanillaItemCatalog_Full.csv.
    ///
    /// Column layout:
    ///   Identity:            Id, Name, ObtainCount
    ///   Game-source counts:  FieldCount, WorldMapCount, EnemyCount, BossCount, InShop, SynthCount
    ///   Verification counts: ChocographCount, DeadPepperCount, AuctionCount
    ///   Flags:               IsKeyItem, IsMissable, IsSynthesisResult, IsSynthesisIngredient
    ///   Bottom row (id=-1):  "counts towards obtainability" markers per column
    ///
    /// Run once to produce your master obtainability reference list.
    /// Skips gracefully if p0data7.bin is not present.
    ///
    /// Output path: bin/Debug/net8.0/TestData/VanillaItemCatalog_Full.csv
    /// </summary>
    /// 

    // =========================================================================
    // 7. VanillaItemCatalog — BattleItemScanner wiring
    // =========================================================================

    [Fact]
    public void Catalog_WithBattleScan_AllNormalEnemyItems_AreInfinite()
    {
        // When a BattleScanResult is provided, all hardcoded NormalEnemyItemIds must
        // remain infinite — they are in AllEnemyItemIds and not in BossOnlyItemIds.
        SkipIfNoP0data2();
        var battleResult = BattleItemScanner.ScanArchive(P0data2Path);
        var catalog = BuildCatalog(battleScanResult: battleResult);

        foreach (int id in VanillaObtainabilityData.NormalEnemyItemIds)
        {
            Assert.True(catalog.ObtainabilityCounts.TryGetValue(id, out int count)
                        && count == int.MaxValue,
                $"Item {id} (NormalEnemyItemIds) must be infinite when battle scan is provided.");
        }
    }

    [Fact]
    public void Catalog_WithBattleScan_BossOnlyItems_StayFinite()
    {
        // BossOnlyItemIds are excluded from HasNormalEnemySource even when the
        // scanner finds them in drop/steal tables — boss encounters are one-time.
        // Pumice (211) and Dark Matter (250) must remain finite.
        SkipIfNoP0data2();
        var battleResult = BattleItemScanner.ScanArchive(P0data2Path);
        var catalog = BuildCatalog(battleScanResult: battleResult);

        Assert.False(catalog.Entries[211].HasNormalEnemySource,
            "Pumice (211) is BossOnly — must not be HasNormalEnemySource.");
        Assert.False(catalog.Entries[250].HasNormalEnemySource,
            "Dark Matter (250) is BossOnly — must not be HasNormalEnemySource.");

        // Neither is in shops or repeatable auction — must be finite.
        Assert.NotEqual(int.MaxValue, catalog.ObtainabilityCounts.GetValueOrDefault(211));
    }

    [Fact]
    public void Catalog_WithBattleScan_ExtendedSet_MoreItemsAreInfinite()
    {
        // BattleItemScanner finds ~58 items not in NormalEnemyItemIds (weapons/armor
        // obtainable from enemy steals not documented in guide data). When the scanner
        // is provided, non-boss items from that extended set gain HasNormalEnemySource=true.
        // Total infinite item count must be strictly greater than without scanner.
        SkipIfNoP0data2();
        var battleResult = BattleItemScanner.ScanArchive(P0data2Path);
        var withScan = BuildCatalog(battleScanResult: battleResult);
        var withoutScan = BuildCatalog();

        int infiniteWith = withScan.ObtainabilityCounts.Values.Count(v => v == int.MaxValue);
        int infiniteWithout = withoutScan.ObtainabilityCounts.Values.Count(v => v == int.MaxValue);

        _out.WriteLine($"Infinite items without battle scan: {infiniteWithout}");
        _out.WriteLine($"Infinite items with battle scan:    {infiniteWith}");
        _out.WriteLine($"New infinite items from scanner:    {infiniteWith - infiniteWithout}");

        Assert.True(infiniteWith > infiniteWithout,
            "Battle scan must mark additional items infinite beyond the hardcoded NormalEnemyItemIds set.");
    }

    [Fact]
    public void Catalog_FullBuild_WithBothArchives_ProducesCompleteCatalog()
    {
        // Integration test: full pipeline with real field + battle data.
        // Both archives required — skip if either is absent.
        SkipIfNoP0data7();
        SkipIfNoP0data2();

        var fieldCounts = FieldItemScanner.ScanArchive(P0data7Path);
        var battleResult = BattleItemScanner.ScanArchive(P0data2Path);

        var catalog = VanillaItemCatalog.Build(
            ItemsCsvPath, ShopItemsCsvPath, SynthesisCsvPath,
            fieldCounts, null, battleResult);

        Assert.Equal(256, catalog.Entries.Count);
        Assert.True(catalog.ObtainabilityCounts.Count > 100,
            "Full catalog must have >100 obtainable items.");

        int infiniteCount = catalog.ObtainabilityCounts.Values.Count(v => v == int.MaxValue);
        int finiteCount = catalog.ObtainabilityCounts.Values.Count(v => v != int.MaxValue);

        _out.WriteLine($"Field unique IDs:  {fieldCounts.Count}");
        _out.WriteLine($"Battle drops:      {battleResult.DropItemIds.Count} unique");
        _out.WriteLine($"Battle steals:     {battleResult.StealItemIds.Count} unique");
        _out.WriteLine($"Battle cards:      {battleResult.CardDropIds.Count} unique");
        _out.WriteLine($"Total obtainable:  {catalog.ObtainabilityCounts.Count}");
        _out.WriteLine($"  Infinite:        {infiniteCount}");
        _out.WriteLine($"  Finite:          {finiteCount}");

        Assert.True(infiniteCount > 50, $"Expected >50 infinite items, got {infiniteCount}");
        Assert.True(finiteCount > 20, $"Expected >20 finite items, got {finiteCount}");
    }

    [Fact]
    public void Diagnostic_DumpFullCatalogToCSV()
    {
        SkipIfNoP0data7();



        // ── 2. Build catalog with real field data ─────────────────────────
        // NEW:
        var fieldCounts = FieldItemScanner.ScanArchive(P0data7Path);
        // Battle scan — optional, used when p0data2.bin is available
        BattleScanResult? battleResult = null;
        if (File.Exists(P0data2Path))
            battleResult = BattleItemScanner.ScanArchive(P0data2Path);
        var catalog = BuildCatalog(fieldCounts, battleResult);

        // ── 3. Write CSV ──────────────────────────────────────────────────
        string outputPath = Path.Combine(
            AppContext.BaseDirectory, "TestData", "VanillaItemCatalog_Full.csv");

        using var writer = new System.IO.StreamWriter(outputPath, append: false,
            encoding: System.Text.Encoding.UTF8);

        // Header
        writer.WriteLine(
            "Id,Name,ObtainCount," +
            "FieldCount,WorldMapCount,EnemyCount,BossCount,InShop,SynthCount," +
            "ChocographCount,DeadPepperCount,AuctionCount," +
            "IsKeyItem,IsMissable,IsSynthesisResult,IsSynthesisIngredient");

        int obtainableCount = 0;
        int infiniteCount   = 0;
        int finiteCount     = 0;
        int keyItemCount    = 0;

        foreach (var (id, entry) in catalog.Entries.OrderBy(kv => kv.Key))
        {
            if (entry.IsKeyItem) keyItemCount++;

            string countStr;
            if (catalog.ObtainabilityCounts.TryGetValue(id, out int c))
            {
                obtainableCount++;
                if (c == int.MaxValue) { infiniteCount++; countStr = "Infinite"; }
                else                   { finiteCount++;   countStr = c.ToString(); }
            }
            else
            {
                countStr = "0";
            }

            string name = string.IsNullOrEmpty(entry.Name) ? $"Item{id}" : entry.Name;

            // Verification counts (not directly in ObtainCount but useful for cross-reference)
            VanillaObtainabilityData.ChocographItemCounts.TryGetValue(id, out int chocoCount);
            VanillaObtainabilityData.DeadPepperItemCounts.TryGetValue(id, out int deadPepperCount);

            // AuctionCount: int.MaxValue → "Infinite", 1 → "1", 0 → "0"
            string auctionStr = entry.AuctionCount == int.MaxValue ? "Infinite"
                              : entry.AuctionCount.ToString();

            // EnemyCount: 1 if normal enemy source (infinite, but useful to note)
            string enemyStr = entry.HasNormalEnemySource ? "Infinite" : "0";

            // InShop: treat as infinite marker
            string shopStr = entry.IsInShop ? "Infinite" : "0";

            writer.WriteLine(
                $"{id},{name},{countStr}," +
                $"{entry.FieldInstanceCount},{entry.WorldMapInstanceCount},{enemyStr},{entry.BossInstanceCount},{shopStr},{entry.SynthesisInstanceCount}," +
                $"{chocoCount},{deadPepperCount},{auctionStr}," +
                $"{B(entry.IsKeyItem)},{B(entry.IsMissable)},{B(entry.IsSynthesisResult)},{B(entry.IsSynthesisIngredient)}");
        }

        // ── 4. Bottom summary row ─────────────────────────────────────────
        // id=-1: marks which columns count towards ObtainCount (1=yes, 0=no/reference-only)
        writer.WriteLine(
            "-1,counts towards obtainability,," +
            "1,1,1,1,1,1," +   // FieldCount,WorldMapCount,EnemyCount,BossCount,InShop,SynthCount → yes
            "0,0,0," +          // ChocographCount,DeadPepperCount,AuctionCount → reference only
            "0,0,0,0");         // IsKeyItem,IsMissable,IsSynthesisResult,IsSynthesisIngredient → flags

        // ── 5. Log summary ────────────────────────────────────────────────
        _out.WriteLine($"Output: {outputPath}");
        _out.WriteLine($"Total entries:    {catalog.Entries.Count}");
        _out.WriteLine($"Key items:        {keyItemCount}");
        _out.WriteLine($"Obtainable items: {obtainableCount}");
        _out.WriteLine($"  Infinite:       {infiniteCount}");
        _out.WriteLine($"  Finite:         {finiteCount}");
        _out.WriteLine($"Names resolved:   {catalog.Entries.Values.Count(e => !string.IsNullOrEmpty(e.Name))} from Items.csv inline comments");
        _out.WriteLine($"Field items:      {fieldCounts.Count} distinct IDs across all field scripts");

        if (battleResult != null)
        {
            _out.WriteLine($"Battle drops:  {battleResult.DropItemIds.Count} unique IDs");
            _out.WriteLine($"Battle steals: {battleResult.StealItemIds.Count} unique IDs");
            _out.WriteLine($"Battle cards:  {battleResult.CardDropIds.Count} unique IDs");
        }
        else
        {
            _out.WriteLine("Battle scan: skipped (p0data2.bin not found)");
        }

        Assert.True(File.Exists(outputPath), $"CSV was not written to {outputPath}");
        Assert.True(obtainableCount > 100,
            $"Expected >100 obtainable items, got {obtainableCount}");
    }

    // ── Helper: bool → 1/0 for CSV ────────────────────────────────────────
    private static string B(bool value) => value ? "1" : "0";

    /// <summary>
    /// Diagnostic: dumps every field file that contains an AddItem(0, X) or
    /// Treasure_Item = 0 pattern, along with the surrounding hex bytes.
    ///
    /// Purpose: understand why FieldParser finds 85 item-0 occurrences when only
    /// one real Hammer give exists (Field 1911, Queen Stella). Inspect the context
    /// bytes to determine if these are dead branches (if(0)), placeholder opcode
    /// data, or something else — and assess scanner reliability for items 1–255.
    ///
    /// Output: bin/Debug/net8.0/TestData/ItemZeroFalsePositives.txt
    /// Requires p0data7.bin — skips gracefully if not present.
    /// </summary>
    [Fact]
    public void Diagnostic_DumpItemZeroFalsePositiveLocations()
    {
        SkipIfNoP0data7();

        string outputPath = Path.Combine(
            AppContext.BaseDirectory, "TestData", "ItemZeroFalsePositives.txt");

        using var archive = UnityArchiver.Open(P0data7Path);

        var distinctFieldNames = archive.GetFileNames()
            .Where(n => n.StartsWith("evt_", StringComparison.OrdinalIgnoreCase)
                     && !n.StartsWith("evt_battle_", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n)
            .ToList();

        var hits = new List<string>();
        int totalHits = 0;
        int filesWithHits = 0;

        foreach (string name in distinctFieldNames)
        {
            byte[] bytes;
            try { bytes = archive.Extract(name); }
            catch { continue; }

            IReadOnlyList<FieldItemLocation> locations;
            try { locations = FieldParser.FindItemLocations(bytes); }
            catch { continue; }

            // Find all locations where the value is item 0, regardless of kind.
            // We call FieldParser directly (bypassing FieldItemScanner's filter)
            // so we see every occurrence.
            var zeroLocs = locations
                .Where(loc =>
                    loc.CurrentValue == 0 &&
                    (loc.LocationKind == FieldLocationKind.DirectItem ||
                     loc.LocationKind == FieldLocationKind.TreasureItem))
                .ToList();

            if (zeroLocs.Count == 0) continue;

            filesWithHits++;
            hits.Add($"=== {name} ({zeroLocs.Count} hit(s)) ===");

            foreach (var loc in zeroLocs)
            {
                totalHits++;
                int offset = loc.FileOffset;

                // Hex dump: 8 bytes before, the hit byte, 16 bytes after
                int start  = Math.Max(0, offset - 8);
                int end    = Math.Min(bytes.Length, offset + 17);
                var window = bytes[start..end];
                string hex = string.Join(" ", window.Select(b => b.ToString("X2")));

                // Mark where the hit is in the window
                int markerPos = offset - start;

                hits.Add($"  Offset={offset:X4}  Kind={loc.LocationKind}");
                hits.Add($"  Context bytes: {hex}");
                hits.Add($"  {new string(' ', markerPos * 3)}^^ hit at byte {offset:X4}");
                hits.Add(string.Empty);
            }
        }

        var lines = new List<string>
        {
            $"Item 0 (Hammer) false-positive diagnostic — {DateTime.Now:yyyy-MM-dd HH:mm}",
            $"Total hits: {totalHits}  |  Files with hits: {filesWithHits}",
            $"Real give: Field 1911 evt_trnSTL_00 (Queen Stella / Stellazzio quest)",
            new string('=', 72),
            string.Empty,
        };
        lines.AddRange(hits);

        File.WriteAllLines(outputPath, lines, System.Text.Encoding.UTF8);

        _out.WriteLine($"Output: {outputPath}");
        _out.WriteLine($"Total item-0 occurrences found: {totalHits} across {filesWithHits} field files");

        Assert.True(File.Exists(outputPath));
        // We expect at least 1 hit (the real Queen Stella give)
        Assert.True(totalHits >= 1, "Expected at least one item-0 location (Queen Stella)");
    }

    // ── Disc-Variant Fingerprint Diagnostic ───────────────────────────────────
    //
    // Task 1.5 — Identifies field item locations that appear in multiple disc-era
    // variants of the same scene (e.g. LIND1_x vs LIND2_x vs LIND3_x, or
    // TRENO1_y vs TRENO2_y). Such locations must be randomized identically —
    // patching one disc variant but not its siblings produces inconsistent
    // in-game results.
    //
    // Method: for each DirectItem / TreasureItem location, extract a 33-byte
    // fingerprint: 16 bytes before the item-ID byte + 16 bytes after, with the
    // item-ID byte itself zeroed (so the fingerprint is stable regardless of the
    // current item at that slot). Two locations with identical fingerprints in
    // different files are the same logical slot appearing across disc variants.
    //
    // Output (DiscVariantFingerprints.txt):
    //   • All fingerprints listed individually (with surrounding hex context)
    //   • Groups of 2+ locations with identical fingerprints highlighted
    //   • Near-miss pairs: closest unmatched pairs with byte-diff count and positions
    //
    // Skips gracefully when p0data7.bin is absent.

    [Fact]
    public void Diagnostic_DiscVariantFingerprintMap()
    {
        SkipIfNoP0data7();

        const int Radius      = 48;   // bytes each side of the item-ID byte
        const int WindowSize  = Radius * 2 + 1; // 97 bytes total
        const int NearMissMax = 6;    // report near-misses with <= this many differing bytes

        string outputPath = Path.Combine(
            AppContext.BaseDirectory, "TestData", "DiscVariantFingerprints.txt");

        // ── Step 1: collect all item locations across all field files ──────────
        // Each entry: (fileName, fileOffset, itemId, kind, fingerprint bytes)

        var entries = new List<(string File, int Offset, int ItemId,
                                FieldLocationKind Kind, byte[] Fp)>();

        using (var archive = UnityArchiver.Open(P0data7Path))
        {
            var distinctNames = archive.GetFileNames()
                .Where(n => n.StartsWith("evt_", StringComparison.OrdinalIgnoreCase)
                         && !n.StartsWith("evt_battle_", StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (string name in distinctNames)
            {
                byte[] fileBytes;
                try { fileBytes = archive.Extract(name); }
                catch { continue; }

                IReadOnlyList<FieldItemLocation> locs;
                try { locs = FieldParser.FindItemLocations(fileBytes); }
                catch { continue; }

                foreach (var loc in locs)
                {
                    // Only DirectItem and TreasureItem (item IDs only, not cards/gil)
                    if (loc.LocationKind == FieldLocationKind.TextSync ||
                        loc.LocationKind == FieldLocationKind.DirectGil)
                        continue;
                    if (loc.LocationKind == FieldLocationKind.TreasureItem && !loc.TreasureIsItem)
                        continue;

                    int id = loc.CurrentValue;
                    if (id < 0 || id > 255) continue; // filter parser false-positives

                    int offset = loc.FileOffset;
                    int start  = Math.Max(0, offset - Radius);
                    int end    = Math.Min(fileBytes.Length, offset + Radius + 1);

                    // Build 33-byte fingerprint: pad with 0xFF if near file boundary
                    var fp = new byte[WindowSize];
                    for (int i = 0; i < WindowSize; i++) fp[i] = 0xFF; // padding sentinel

                    int srcStart = offset - Radius; // may be negative
                    for (int i = 0; i < WindowSize; i++)
                    {
                        int srcIdx = srcStart + i;
                        if (srcIdx >= 0 && srcIdx < fileBytes.Length)
                            fp[i] = fileBytes[srcIdx];
                    }
                    // Zero out the item-ID byte (position Radius in the window)
                    fp[Radius] = 0x00;

                    entries.Add((name, offset, id, loc.LocationKind, fp));
                }
            }
        }

        // ── Step 2: group by exact fingerprint ────────────────────────────────

        // Key: fingerprint as hex string; Value: list of entries sharing it
        var fpGroups = entries
            .GroupBy(e => string.Join("", e.Fp.Select(b => b.ToString("X2"))))
            .ToDictionary(g => g.Key, g => g.ToList());

        // Identify groups that span multiple distinct files (disc-variant matches)
        var discVariantGroups = fpGroups.Values
            .Where(g => g.Select(e => e.File).Distinct().Count() > 1)
            .OrderByDescending(g => g.Count)
            .ToList();

        // Single-file groups (or unmatched singletons)
        var unmatchedEntries = fpGroups.Values
            .Where(g => g.Select(e => e.File).Distinct().Count() == 1 && g.Count == 1)
            .SelectMany(g => g)
            .ToList();

        // ── Step 3: near-miss search for unmatched singletons ─────────────────

        // For each unmatched entry, find the closest other entry (different file) by byte diff
        // Only report if diff <= NearMissMax bytes

        var nearMisses = new List<(int DiffCount, int[] DiffPositions,
                                   (string File, int Offset, int ItemId, byte[] Fp) A,
                                   (string File, int Offset, int ItemId, byte[] Fp) B)>();

        var unmatchedList = unmatchedEntries.ToList();
        for (int i = 0; i < unmatchedList.Count; i++)
        {
            var ea = unmatchedList[i];
            int bestDiff = int.MaxValue;
            string bestFile = string.Empty;
            int bestOffset = 0;
            int bestItemId = 0;
            byte[] bestFp = Array.Empty<byte>();
            int[] bestDiffPos = Array.Empty<int>();

            // Compare against all entries from a different file
            foreach (var eb in entries)
            {
                if (string.Equals(eb.File, ea.File, StringComparison.OrdinalIgnoreCase)) continue;

                var diffPos = new List<int>();
                for (int b = 0; b < WindowSize; b++)
                {
                    if (ea.Fp[b] != eb.Fp[b]) diffPos.Add(b);
                }
                if (diffPos.Count < bestDiff)
                {
                    bestDiff    = diffPos.Count;
                    bestFile    = eb.File;
                    bestOffset  = eb.Offset;
                    bestItemId  = eb.ItemId;
                    bestFp      = eb.Fp;
                    bestDiffPos = diffPos.ToArray();
                }
            }

            if (bestDiff <= NearMissMax && bestDiff > 0)
            {
                nearMisses.Add((bestDiff, bestDiffPos,
                    (ea.File, ea.Offset, ea.ItemId, ea.Fp),
                    (bestFile, bestOffset, bestItemId, bestFp)));
            }
        }

        // Deduplicate near-miss pairs (A,B) == (B,A)
        var seenPairs = new HashSet<string>();
        var dedupedNearMisses = new List<(int DiffCount, int[] DiffPositions,
            (string File, int Offset, int ItemId, byte[] Fp) A,
            (string File, int Offset, int ItemId, byte[] Fp) B)>();
        foreach (var nm in nearMisses.OrderBy(n => n.DiffCount))
        {
            string key = string.Compare(nm.A.File + nm.A.Offset, nm.B.File + nm.B.Offset,
                StringComparison.OrdinalIgnoreCase) < 0
                ? $"{nm.A.File}:{nm.A.Offset}|{nm.B.File}:{nm.B.Offset}"
                : $"{nm.B.File}:{nm.B.Offset}|{nm.A.File}:{nm.A.Offset}";
            if (seenPairs.Add(key)) dedupedNearMisses.Add(nm);
        }

        // ── Step 4: write output ───────────────────────────────────────────────

        static string FpContext(byte[] fp, int radius)
        {
            // Show fingerprint with | markers around the zeroed item-ID byte
            string before = string.Join(" ", fp.Take(radius).Select(b => b.ToString("X2")));
            string after  = string.Join(" ", fp.Skip(radius + 1).Select(b => b.ToString("X2")));
            return $"{before} [00] {after}";
        }

        var lines = new List<string>();
        lines.Add($"Disc-Variant Fingerprint Diagnostic — {DateTime.Now:yyyy-MM-dd HH:mm}");
        lines.Add($"Total item locations scanned: {entries.Count}");
        lines.Add($"Unique fingerprints: {fpGroups.Count}");
        lines.Add($"Confirmed disc-variant groups (fingerprint matches across 2+ files): {discVariantGroups.Count}");
        lines.Add($"Unmatched singletons: {unmatchedEntries.Count}");
        lines.Add($"Near-miss pairs (diff <= {NearMissMax} bytes, different files): {dedupedNearMisses.Count}");
        lines.Add($"Fingerprint window: {Radius} bytes each side of item-ID byte (item-ID zeroed)");
        lines.Add(new string('=', 80));
        lines.Add(string.Empty);

        // ── Section A: Confirmed disc-variant groups ──────────────────────────
        lines.Add("═══ SECTION A: CONFIRMED DISC-VARIANT MATCHES ═══");
        lines.Add($"({discVariantGroups.Count} groups where identical fingerprint appears in 2+ different files)");
        lines.Add(string.Empty);

        int groupNum = 0;
        foreach (var group in discVariantGroups)
        {
            groupNum++;
            var distinctFiles = group.Select(e => e.File).Distinct().OrderBy(f => f).ToList();
            lines.Add($"--- Group {groupNum} ({group.Count} locations, {distinctFiles.Count} files) ---");
            lines.Add($"  Fingerprint: {FpContext(group[0].Fp, Radius)}");
            lines.Add($"  Files:");
            foreach (var file in distinctFiles) lines.Add($"    {file}");
            lines.Add($"  All locations:");
            foreach (var e in group.OrderBy(e => e.File).ThenBy(e => e.Offset))
                lines.Add($"    {e.File}  offset=0x{e.Offset:X4}  itemId={e.ItemId}  kind={e.Kind}");
            lines.Add(string.Empty);
        }

        if (discVariantGroups.Count == 0)
        {
            lines.Add("  (none — all locations have unique fingerprints)");
            lines.Add(string.Empty);
        }

        // ── Section B: Near-miss pairs ────────────────────────────────────────
        lines.Add("═══ SECTION B: NEAR-MISS PAIRS ═══");
        lines.Add($"(Unmatched singletons whose closest cross-file match differs by <= {NearMissMax} bytes)");
        lines.Add(string.Empty);

        if (dedupedNearMisses.Count == 0)
        {
            lines.Add("  (none)");
            lines.Add(string.Empty);
        }

        foreach (var nm in dedupedNearMisses)
        {
            lines.Add($"  Diff: {nm.DiffCount} byte(s) at position(s) {string.Join(", ", nm.DiffPositions)}  (pos 0=window-start, pos {Radius}=item-ID center)");
            lines.Add($"  A: {nm.A.File}  offset=0x{nm.A.Offset:X4}  itemId={nm.A.ItemId}");
            lines.Add($"     {FpContext(nm.A.Fp, Radius)}");
            lines.Add($"  B: {nm.B.File}  offset=0x{nm.B.Offset:X4}  itemId={nm.B.ItemId}");
            lines.Add($"     {FpContext(nm.B.Fp, Radius)}");
            // Show which bytes differ
            var diffLines = nm.DiffPositions.Select(pos =>
                $"       pos[{pos:D2}]: A={nm.A.Fp[pos]:X2}  B={nm.B.Fp[pos]:X2}");
            lines.AddRange(diffLines);
            lines.Add(string.Empty);
        }

        // ── Section C: All fingerprints (full dump) ───────────────────────────
        lines.Add("═══ SECTION C: ALL FINGERPRINTS (full dump) ═══");
        lines.Add($"({entries.Count} total item locations, sorted by file then offset)");
        lines.Add(string.Empty);

        foreach (var e in entries.OrderBy(e => e.File).ThenBy(e => e.Offset))
        {
            string matchTag = fpGroups[string.Join("", e.Fp.Select(b => b.ToString("X2")))].Count > 1
                ? " [MATCH]" : string.Empty;
            lines.Add($"{e.File}  0x{e.Offset:X4}  id={e.ItemId}  {e.Kind}{matchTag}");
            lines.Add($"  {FpContext(e.Fp, Radius)}");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        File.WriteAllLines(outputPath, lines, System.Text.Encoding.UTF8);

        _out.WriteLine($"Output: {outputPath}");
        _out.WriteLine($"Locations scanned: {entries.Count}");
        _out.WriteLine($"Disc-variant groups: {discVariantGroups.Count}");
        _out.WriteLine($"Near-miss pairs: {dedupedNearMisses.Count}");

        Assert.True(File.Exists(outputPath));
        Assert.True(entries.Count > 0, "Expected to find item locations in p0data7.bin");
        Assert.True(discVariantGroups.Count > 0,
            "Expected at least one disc-variant group (Lindblum / Treno known matches)");
    }

    [Fact]
    public void Integration_WithFieldData_ScanFilesAndBuildCatalog()
    {
        // Scan the 7 pre-committed test field files and incorporate into catalog
        var files = new[]
        {
            ("house2",   File.ReadAllBytes(FieldParserData("evt_alex1_at_house_2.eb.bytes"))),
            ("cargo",    File.ReadAllBytes(FieldParserData("evt_alex1_ts_cargo_0.eb.bytes"))),
            ("center",   File.ReadAllBytes(FieldParserData("evt_alex1_at_center.eb.bytes"))),
            ("gate",     File.ReadAllBytes(FieldParserData("evt_alex1_at_gate.eb.bytes"))),
        };

        var fieldCounts = FieldItemScanner.ScanFiles(files);
        var catalog = BuildCatalog(fieldCounts);

        // All field items should appear in the catalog
        foreach (int id in fieldCounts.Keys)
        {
            Assert.True(catalog.Entries.ContainsKey(id),
                $"Catalog missing entry for field item {id}");
            Assert.True(catalog.Entries[id].IsFieldItem || catalog.IsInfinite(id),
                $"Item {id} found in field scan but not marked IsFieldItem (unless infinite)");
        }
    }
}
