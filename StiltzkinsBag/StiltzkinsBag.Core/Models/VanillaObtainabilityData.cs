// StiltzkinsBag.Core/Models/VanillaObtainabilityData.cs
//
// Static, hardcoded repository of every vanilla item source in FFIX PC (Steam).
// This is the ground truth used by VanillaItemCatalog to classify items and build
// the master obtainability dictionary.
//
// Sources encoded here:
//   • Enemy drops and steals — classified as Normal (repeatable) or Boss-only (one-time)
//   • Chocograph rewards — finite counts per chocograph (24 total)
//   • Treno Auction House — repeatable auctions vs. one-time lots
//   • Friendly Monster chain — 9 one-time encounters (Mu → Yan)
//   • Ragtime Mouse — 1 one-time reward
//   • Blue magic — spell name per enemy ID (Quina Eat results)
//
// NOT encoded here (handled elsewhere):
//   • Shops (ShopItems.csv — all infinite, loaded at runtime)
//   • Synthesis recipes (Synthesis.csv — loaded at runtime; depends on ingredient availability)
//   • Field chests and scripted item rewards (FieldItemScanner — runtime binary scan)
//   • Stiltzkin encounters (Phase 5.9)
//
// Methodology for enemy data:
//   Source: "Final Fantasy IX Reference Guide - Stolen Items + Blue Magic.csv"
//   • Normal enemy items = steals + drops from enemies of type Normal or Ragtime Mouse
//   • Boss-only items    = steals + drops from Boss/Stage/Prison Cage/Crystal types
//                         MINUS anything already in Normal set or any shop
//   Name variants resolved manually (Pheonix Down→Phoenix Down, Wedge Wing→Wing Edge, etc.)
//
// Methodology for chocograph data:
//   Source: "Final Fantasy IX Reference Guide - Chocobo Side Quests.csv"
//   Counts represent total instances across all 24 chocographs combined.
//   Ability unlocks (Reef/Mountain/Ocean/Sky) and cards are excluded.
//
// Methodology for auction data:
//   Source: "Final Fantasy IX Reference Guide - Treno Auction House.csv"
//   Key items / resellable quest items (Griffin's Heart, Doga's Artifact, etc.) excluded.
//
// Methodology for blue magic data:
//   Source: "Final Fantasy IX Reference Guide - Blue Magic.csv"
//   Columns: Enemy Name, Enemy # (1-based battle binary index), Eat (spell name or status).
//   Only entries where Eat is a real learnable spell are included — entries with
//   "I no can eat!", "Taste Bad!", or "Nothing" are excluded.
//   Enemy # maps directly to the battle binary enemy index used by EnemyRandomizer
//   and BattleItemScanner. Spell names are stored as-is from the guide for spoiler log use.
//
// All item IDs are verified against Items.csv (IDs 0–255).

using System.Collections.Generic;

namespace StiltzkinsBag.Core.Models;

/// <summary>
/// Static repository of all vanilla item obtainability sources that require
/// hardcoded game knowledge (enemy tables, sidequest rewards, etc.).
/// </summary>
public static class VanillaObtainabilityData
{
    // ── Enemy sources ──────────────────────────────────────────────────────────

    /// <summary>
    /// Item IDs obtainable from Normal-type or Stage-type enemies (steals + drops).
    /// Normal enemies are repeatable — any item in this set has an infinite source.
    ///
    /// Note: Ragtime Mouse is classified as Normal-type in the enemy guide, but its
    /// quiz completion reward (Protect Ring, ID 209) is one-time — not a repeatable
    /// enemy drop. Protect Ring is therefore NOT in this set; it lives exclusively in
    /// RagtimeMouseItemIds, which contributes a finite count of 1.
    /// </summary>
    public static readonly IReadOnlySet<int> NormalEnemyItemIds = new HashSet<int>
    {
         48,  // Kaiser Knuckles
         80,  // Needle Fork
         86,  // Rising Sun
         87,  // Wing Edge
        104,  // Mythril Gloves
        141,  // Gold Helm
        154,  // Mythril Vest
        188,  // Dragon Mail
        224,  // Garnet
        225,  // Amethyst
        231,  // Peridot
        232,  // Sapphire
        233,  // Opal
        234,  // Topaz
        235,  // Lapis Lazuli
        236,  // Potion
        237,  // Hi-Potion
        238,  // Ether
        239,  // Elixir
        240,  // Phoenix Down
        241,  // Echo Screen
        242,  // Soft
        243,  // Antidote
        244,  // Eye Drops
        245,  // Magic Tag
        246,  // Vaccine
        247,  // Remedy
        248,  // Annoyntment
        249,  // Phoenix Pinion
        253,  // Tent
        254,  // Ore
    };

    /// <summary>
    /// Item IDs obtainable ONLY from Boss-type enemies (Boss/Stage/Prison Cage/Crystal).
    /// These items are NOT in any shop and NOT from Normal enemies — making them
    /// finite (each boss can only be fought once).
    ///
    /// Note: Stage-type enemies (e.g. King Leo at the play) yield items but are
    /// effectively one-time and are treated here as boss-like for classification.
    /// </summary>
    public static readonly IReadOnlySet<int> BossOnlyItemIds = new HashSet<int>
    {
         13,  // Masamune
         19,  // Blood Sword
         49,  // Duel Claws
        109,  // Genji Gloves
        146,  // Genji Helmet
        147,  // Grand Helm
        166,  // Rubber Suit
        171,  // Glutton's Robe
        172,  // White Robe
        173,  // Black Robe
        174,  // Light Robe
        175,  // Robe of Lords
        189,  // Genji Armor
        191,  // Grand Armor
        196,  // Feather Boots   (also Auction repeatable — effectively infinite overall)
        197,  // Battle Boots
        198,  // Running Shoes
        201,  // Black Belt
        205,  // Reflect Ring    (also Auction one-time — catalog totals both)
        208,  // Rebirth Ring    (also Chocograph #24 — catalog totals both)
        210,  // Pumice Piece    (also Chocograph #22 — catalog totals both)
        211,  // Pumice
        250,  // Dark Matter     (also Auction one-time — catalog totals both)
    };

    // ── Chocograph rewards ────────────────────────────────────────────────────

    /// <summary>
    /// Total item counts across all 24 chocographs combined.
    /// Each chocograph is a one-time event — these counts are finite.
    ///
    /// VALIDATION-REFERENCE ONLY (Phase 5.9.2, 2026-03-28).
    /// This dictionary is no longer additive in VanillaItemCatalog.Build().
    /// FieldItemScanner is the authoritative primary source for these items — it reads
    /// the same AddItem opcodes from evt_world_* scripts that chocograph digs fire.
    /// This dictionary is retained for:
    ///   • Cross-validation: scanner output should match these counts
    ///   • Cross-validation against WorldMapVariableScanner runtime output
    ///   • Diagnostic CSV output: ChocographCount column in the full dump
    ///
    /// Counts verified from: Final Fantasy IX Reference Guide - Chocobo Side Quests.csv
    /// Cards, ability unlocks (Reef/Mountain/Ocean/Sky), and quest-only items are excluded.
    /// Plural typos in the source CSV (Potions, Hi-Potions, Antidotes, Cotton Robes) resolved.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, int> ChocographItemCounts = new Dictionary<int, int>
    {
        {  29, 1 },   // Ragnarok
        {  45, 1 },   // Dragon's Claws
        {  48, 1 },   // Kaiser Knuckles
        {  74, 1 },   // Oak Staff
        {  77, 1 },   // High Mage Staff
        {  86, 8 },   // Rising Sun
        {  87, 13 },  // Wing Edge
        {  97, 1 },   // Jade Armlet
        { 106, 1 },   // Diamond Gloves
        { 168, 1 },   // Cotton Robe
        { 170, 1 },   // Magician Robe
        { 172, 1 },   // White Robe
        { 174, 1 },   // Light Robe
        { 183, 1 },   // Shield Armor
        { 184, 1 },   // Demon's Mail
        { 189, 1 },   // Genji Armor
        { 194, 2 },   // Germinas Boots
        { 196, 1 },   // Feather Boots
        { 208, 1 },   // Rebirth Ring
        { 210, 1 },   // Pumice Piece
        { 221, 1 },   // Ribbon
        { 224, 16 },  // Garnet
        { 225, 34 },  // Amethyst
        { 227, 1 },   // Diamond
        { 231, 23 },  // Peridot
        { 232, 26 },  // Sapphire
        { 233, 23 },  // Opal
        { 234, 19 },  // Topaz
        { 236, 78 },  // Potion
        { 237, 18 },  // Hi-Potion
        { 238, 28 },  // Ether
        { 239, 6 },   // Elixir
        { 240, 21 },  // Phoenix Down
        { 241, 7 },   // Echo Screen
        { 242, 6 },   // Soft
        { 243, 10 },  // Antidote
        { 245, 6 },   // Magic Tag
        { 247, 4 },   // Remedy
        { 248, 15 },  // Annoyntment
        { 249, 24 },  // Phoenix Pinion
        { 253, 3 },   // Tent
        { 254, 58 },  // Ore
    };

    // ── Dead Pepper dig rewards ────────────────────────────────────────────────

    /// <summary>
    /// Item counts rewarded by using a Dead Pepper consumable at specific world-map
    /// locations, requiring a dark blue (ocean/sky) or gold (all) chocobo.
    /// Each of the 8 dig sites is a unique one-time event — totals are finite.
    ///
    /// VALIDATION-REFERENCE ONLY (Phase 5.9.2, 2026-03-28).
    /// WorldMapVariableScanner is the authoritative binary source; this dictionary
    /// exists for cross-validation with scanner output and as the DeadPepperCount
    /// verification column in the catalog CSV dump.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, int> DeadPepperItemCounts = new Dictionary<int, int>
    {
        {  15,  1 },   // Ultima Weapon
        {  40,  1 },   // Dragon's Hair
        {  56,  1 },   // Tiger Racket
        { 109,  1 },   // Genji Gloves
        { 111,  1 },   // Gauntlets
        { 113,  8 },   // Straw Hat
        { 146,  1 },   // Genji Helmet
        { 148,  7 },   // Aloha T-shirt
        { 173,  1 },   // Black Robe
        { 190,  1 },   // Maximillian
        { 195,  8 },   // Sandals
        { 203,  1 },   // Madain's Ring
        { 204,  1 },   // Rosetta Ring
        { 209,  1 },   // Protect Ring
        { 217,  8 },   // Pearl Armlet
        { 222,  1 },   // Maiden Prayer
        { 226, 10 },   // Aquamarine
        { 234, 15 },   // Topaz
        { 235, 41 },   // Lapis Lazuli
        { 236, 50 },   // Potion
        { 237, 25 },   // Hi-Potion
        { 238,  9 },   // Ether
        { 239,  7 },   // Elixir
        { 244, 19 },   // Eye Drops
        { 247, 10 },   // Remedy
        { 254,  9 },   // Ore
    };

    // ── World map variable-delivery items (canonical fallback) ────────────────

    /// <summary>
    /// Hardcoded fallback for WorldMapVariableScanner when the real .eb.bytes files
    /// are not available (unit tests, offline mode).
    /// </summary>
    public static readonly IReadOnlyDictionary<int, int> WorldMapVariableItemCounts =
        new Dictionary<int, int>(DeadPepperItemCounts)
        {
            {  29, 1 },  // Ragnarok       — Convention C World_Chest
            {  45, 1 },  // Dragon's Claws — Convention C World_Chest
        };

    // ── Treno Auction House ────────────────────────────────────────────────────

    /// <summary>
    /// Items available at the Treno Auction House that can be won multiple times.
    /// These are effectively infinite obtainability sources.
    /// </summary>
    public static readonly IReadOnlySet<int> AuctionRepeatableItemIds = new HashSet<int>
    {
        170,  // Magician Robe
        196,  // Feather Boots
        199,  // Anklet
        203,  // Madain's Ring
        207,  // Promist Ring
        214,  // Fairy Earrings
        216,  // Pearl Rouge
        239,  // Elixir
    };

    /// <summary>
    /// Items available at the Treno Auction House exactly once.
    /// Finite — each appears at most once per playthrough.
    /// </summary>
    public static readonly IReadOnlySet<int> AuctionOneTimeItemIds = new HashSet<int>
    {
         98,  // Thief Gloves
        205,  // Reflect Ring
        221,  // Ribbon
        250,  // Dark Matter
    };

    // ── Friendly Monster chain ─────────────────────────────────────────────────

    /// <summary>
    /// Item IDs rewarded by the 9-encounter Friendly Monster chain (Mu → Yan).
    /// Each reward is a one-time gift from completing one link in the chain.
    /// </summary>
    public static readonly IReadOnlySet<int> FriendlyMonsterItemIds = new HashSet<int>
    {
        236,  // Potion        (Mu)
        237,  // Hi-Potion     (Ghost)
        238,  // Ether         (Lady Bug)
        239,  // Elixir        (Yeti)
        228,  // Emerald       (Nymph)
        229,  // Moonstone     (Jabberwock)
        235,  // Lapis Lazuli  (Feather Circle)
        227,  // Diamond       (Garuda)
        204,  // Rosetta Ring  (Yan)
    };

    // ── Ragtime Mouse ─────────────────────────────────────────────────────────

    /// <summary>
    /// The single reward for completing all 16 Ragtime Mouse quiz questions.
    /// One-time per playthrough — finite source (1 copy).
    /// </summary>
    public static readonly IReadOnlySet<int> RagtimeMouseItemIds = new HashSet<int>
    {
        209,  // Protect Ring
    };

    // ── Sentinel-excluded field items ──────────────────────────────────────────

    /// <summary>
    /// Items that FieldItemScanner cannot count accurately because their item ID
    /// is used as a null/default sentinel in many field scripts.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, int> SentinelExcludedFieldItems = new Dictionary<int, int>
    {
        { 0, 1 },  // Hammer — Field 1911 (Treno / Queen Stella's house), Stellazzio quest reward.
    };

    // ── Kupo Nut rewards ───────────────────────────────────────────────────────

    /// <summary>
    /// Item IDs rewarded by the Mognet/Kupo Nut quest chain.
    /// VALIDATION-REFERENCE ONLY — see full comment in prior version.
    /// </summary>
    public static readonly IReadOnlySet<int> KupoNutItemIds = new HashSet<int>
    {
        239,  // Elixir        (Disc 2)
        220,  // Extension     (Disc 3)
        148,  // Aloha T-Shirt (Disc 4)
    };

    // ── Missable items ─────────────────────────────────────────────────────────

    /// <summary>
    /// Item IDs that have at least one missable acquisition opportunity.
    /// Tagged from <c>FFIX GUIDE DATA/Missable Items.txt</c>.
    /// </summary>
    public static readonly IReadOnlySet<int> MissableItemIds = new HashSet<int>
    {
        18,   // Mythril Sword
        51,   // Air Racket
        103,  // Silver Gloves
        138,  // Iron Helm
        179,  // Chain Mail
        31,   // Javelin
        229,  // Moonstone
        228,  // Emerald
        227,  // Diamond
        208,  // Rebirth Ring
        19,   // Blood Sword
        109,  // Genji Gloves
        146,  // Genji Helmet
        189,  // Genji Armor
        198,  // Running Shoes
        30,   // Excalibur II
        49,   // Duel Claws
        210,  // Pumice Piece
        250,  // Dark Matter
    };

    // ── Stiltzkin visit locations ──────────────────────────────────────────────

    /// <summary>
    /// Confirmed evt_ field script names for each of Stiltzkin's 8 visits.
    /// Key = package price in Gil. Value = array of script names to patch.
    /// Cleyra 444G is a paired field — both scripts must be patched identically.
    /// Script names confirmed by StiltzkinFieldDiagnosticTests (2026-03-31).
    /// </summary>
    public static readonly IReadOnlyDictionary<int, string[]> StiltzkinVisitLocations =
        new Dictionary<int, string[]>
        {
        {  333, ["EVT_BURMECIA_SQUARE_1.eb"] },
        {  444, ["EVT_CLEYRA3_ANTRION.eb", "EVT_CLEYRA3_INN.eb"] },
        {  555, ["EVT_FOSSIL_FR_DN1_0.eb"] },
        {  666, ["EVT_PATA_M_CM_MP3_0.eb"] },
        {  777, ["EVT_ALEX3_AT_SENTOU.eb"] },
        {  888, ["EVT_OEIL_UV_DEP_0.eb"] },
        { 2222, ["EVT_BAL_BB_WPS_0.eb"] },
        { 5555, ["EVT_ALEX5_AT_SENTOU.eb"] },
        };

    // ── Blue magic (Quina Eat results) ────────────────────────────────────────

    /// <summary>
    /// Maps each enemy's battle binary index (1-based "Enemy #") to the blue magic
    /// spell name Quina learns by eating that enemy.
    ///
    /// Source: "Final Fantasy IX Reference Guide - Blue Magic.csv"
    /// Only enemies with a real learnable spell are included. Enemies with
    /// "I no can eat!", "Taste Bad!", or "Nothing" are omitted.
    ///
    /// The key matches the enemy index used by EnemyRandomizer and BattleItemScanner.
    /// Spell names are stored as-is from the guide for use in the Phase 8 spoiler log.
    ///
    /// Note: Some spells appear on multiple enemy indices (e.g. Matra Magic on
    /// Trick Sparrow/10, Zaghnol/80, Land Worm/109, Ogre/111, Armstrong/114, Ogre/116).
    /// All instances are retained — the spoiler log may report which enemy now carries
    /// a given spell after EnemyRandomizer.ShuffleBlueMagic() runs.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, string> BlueMagicByEnemyId =
        new Dictionary<int, string>
        {
            {  10, "Goblin Punch" },    // Goblin
            {  21, "Pumpkin Head" },    // Python
            {  27, "LV3 Def-less" },    // Cave Spider
            {  28, "Roulette" },        // Ghost
            {  29, "Vanish" },          // Vice
            {  32, "Matra Magic" },     // Trick Sparrow
            {  36, "Mustard Bomb" },    // Bomb
            {  38, "Limit Glove" },     // Axe Beak
            {  39, "Mighty Guard" },    // Serpion
            {  40, "Pumpkin Head" },    // Ladybug
            {  41, "Aqua Breath" },     // Clipper
            {  42, "Limit Glove" },     // Mandragora
            {  43, "Angel's Snack" },   // Ironite
            {  44, "Frog Drop" },       // Gigan Toad
            {  45, "Frog Drop" },       // Gigan Toad (variant)
            {  46, "Aqua Breath" },     // Axolotl
            {  47, "Vanish" },          // Hornet
            {  48, "Pumpkin Head" },    // Skeleton
            {  50, "LV3 Def-less" },    // Lamia
            {  54, "Night" },           // Nymph
            {  55, "Pumpkin Head" },    // Basilisk
            {  56, "Magic Hammer" },    // Magic Vice
            {  66, "White Wind" },      // Zuu
            {  67, "Auto-Life" },       // Carrion Worm
            {  80, "Matra Magic" },     // Zaghnol
            {  81, "Night" },           // Seeker Bat
            {  84, "White Wind" },      // Griffin
            {  85, "LV4 Holy" },        // Feather Circle
            {  86, "Night" },           // Abomination
            {  87, "1,000 Needles" },   // Cactuar
            {  88, "Goblin Punch" },    // Goblin Mage
            {  89, "Aqua Breath" },     // Sahagin
            {  90, "Mighty Guard" },    // Myconid
            {  91, "White Wind" },      // Zemzelett
            {  92, "Vanish" },          // Gnoll
            {  93, "LV3 Def-less" },    // Ochu
            {  94, "Vanish" },          // Troll
            {  96, "Limit Glove" },     // Blazer Beetle
            {  97, "Roulette" },        // Zombie
            {  98, "LV5 Death" },       // Stroper
            {  99, "LV5 Death" },       // Dracozombie
            { 102, "Angel's Snack" },   // Mistodon
            { 103, "Mighty Guard" },    // Gigan Octopus
            { 104, "Earth Shake" },     // Adamantoise
            { 105, "LV5 Death" },       // Whale Zombie
            { 106, "LV3 Def-less" },    // Grand Dragon
            { 107, "Auto-Life" },       // Gimme Cat
            { 108, "Bad Breath" },      // Anenome
            { 109, "Matra Magic" },     // Land Worm
            { 110, "Mighty Guard" },    // Antlion
            { 111, "Matra Magic" },     // Ogre
            { 112, "Night" },           // Grimlock
            { 113, "Limit Glove" },     // Jabberwock
            { 114, "Matra Magic" },     // Armstrong
            { 115, "Limit Glove" },     // Catoblepas
            { 116, "Matra Magic" },     // Ogre (variant)
            { 117, "Angel's Snack" },   // Epitaph
            { 126, "White Wind" },      // Garuda
            { 128, "LV4 Holy" },        // Torama
            { 129, "Vanish" },          // Drakan
            { 131, "Aqua Breath" },     // Vepal Green
            { 132, "Mustard Bomb" },    // Vepal Red
            { 133, "Mustard Bomb" },    // Grenade
            { 134, "Bad Breath" },      // Worm Hydra
            { 135, "Frost" },           // Wraith (Blue Fire)
            { 136, "Mustard Bomb" },    // Wraith (Red Fire)
            { 137, "Twister" },         // Red Dragon
            { 141, "Auto-Life" },       // Yan
            { 142, "LV4 Holy" },        // Amdusias
            { 143, "Doom" },            // Veteran
            { 144, "Auto-Life" },       // Cerberus
            { 146, "Mighty Guard" },    // Gargoyle
            { 149, "Earth Shake" },     // Earth Guardian
            { 150, "Magic Hammer" },    // Ring Leader
            { 151, "Roulette" },        // Hecteyes
            { 156, "Bad Breath" },      // Malboro
            { 157, "Earth Shake" },     // Shell Dragon
            { 158, "Twister" },         // Abadon
            { 169, "Pumpkin Head" },    // Yeti (variant)
            { 175, "Angel's Snack" },   // Behemoth
            { 179, "Frost" },           // Chimera
            { 181, "Doom" },            // Ash
            { 185, "Auto-Life" },       // Stilva
            { 191, "Mustard Bomb" },    // Maliris
            { 192, "Twister" },         // Tiamat
            { 193, "Frost" },           // Kraken
            { 194, "LV5 Death" },       // Lich
        };

    // ── Stiltzkin Recommended item pools ──────────────────────────────────────
    //
    // Three modes: Stiltzkin's Junk, Fun, Challenging.
    // All pools draw across the full item universe (weapons, armor, accessories,
    // consumables) — NOT limited to consumables or gems.
    //
    // Excluded from all pools regardless of price:
    //   • Save the Queen (26)   — Beatrix's personal weapon, not a player item
    //   • Pumice (211)          — Ozma drop only, story significance
    //   • Ancient Aroma (223)   — synthesis key ingredient, story-gated
    //   • Hammer (0)            — sentinel item
    //   • Slot 255              — empty/unused
    //
    // All IDs verified against Items.csv (IDs 0–255, Phase 5.99, 2026-04-01).

    // ── Stiltzkin's Junk pool ────────────────────────────────────────────────

    /// <summary>
    /// "Stiltzkin's Junk" — the lowest-tier items in the game.
    /// Starting weapons, first armor sets, and cheap consumables.
    /// Comedy comes from paying 2222G for three Daggers and a Potion.
    /// All 3 package slots draw from this pool independently.
    /// </summary>
    public static readonly IReadOnlyList<int> StiltzkinJunkPool = new[]
    {
        // ── Lowest-tier weapons ──────────────────────────────────────────────
          1,  // Dagger           (320G)
          2,  // Mage Masher      (500G)
         16,  // Broadsword       (330G)
         57,  // Rod              (260G)
         70,  // Mage Staff       (320G)
         79,  // Fork             (1100G)
         85,  // Pinwheel         (200G)
         86,  // Rising Sun       (500G)

        // ── Lowest-tier armlets / gloves ────────────────────────────────────
         88,  // Wrist            (130G)
         89,  // Leather Wrist    (200G)
         90,  // Glass Armlet     (250G)
         91,  // Bone Wrist       (330G)
        102,  // Bronze Gloves    (480G)

        // ── Lowest-tier helms ────────────────────────────────────────────────
        112,  // Leather Hat      (150G)
        114,  // Feather Hat      (200G)
        115,  // Steepled Hat     (260G)
        116,  // Headgear         (330G)
        117,  // Magus Hat        (400G)
        118,  // Bandana          (500G)
        136,  // Rubber Helm      (250G)
        137,  // Bronze Helm      (330G)
        138,  // Iron Helm        (450G)

        // ── Lowest-tier body armor ───────────────────────────────────────────
        149,  // Leather Shirt    (270G)
        150,  // Silk Shirt       (400G)

        // ── Common consumables ───────────────────────────────────────────────
        236,  // Potion           (50G)
        240,  // Phoenix Down     (150G)
        241,  // Echo Screen      (50G)
        242,  // Soft             (100G)
        243,  // Antidote         (50G)
        244,  // Eye Drops        (50G)
        246,  // Vaccine          (100G)
        248,  // Annoyntment      (150G)
        251,  // Gysahl Greens    (60G)
    };

    // ── Fun pool — Low tier ───────────────────────────────────────────────────

    /// <summary>
    /// "Fun" mode low-tier draw pool.
    /// StiltzkinRandomizer draws exactly 1 item from this list per package.
    /// Entry-level gear and basic consumables — the "floor" of what you could get.
    /// </summary>
    public static readonly IReadOnlyList<int> StiltzkinFunPoolLow = new[]
    {
        // Weapons
          1,  // Dagger           (320G)
          2,  // Mage Masher      (500G)
         16,  // Broadsword       (330G)
         17,  // Iron Sword       (660G)
         31,  // Javelin          (880G)
         57,  // Rod              (260G)
         70,  // Mage Staff       (320G)
         79,  // Fork             (1100G)
         85,  // Pinwheel         (200G)
         86,  // Rising Sun       (500G)

        // Armlets / gloves
         88,  // Wrist            (130G)
         89,  // Leather Wrist    (200G)
         90,  // Glass Armlet     (250G)
         91,  // Bone Wrist       (330G)
         92,  // Mythril Armlet   (500G)
        102,  // Bronze Gloves    (480G)
        103,  // Silver Gloves    (720G)

        // Helms
        112,  // Leather Hat      (150G)
        114,  // Feather Hat      (200G)
        115,  // Steepled Hat     (260G)
        116,  // Headgear         (330G)
        117,  // Magus Hat        (400G)
        118,  // Bandana          (500G)
        119,  // Mage's Hat       (600G)
        136,  // Rubber Helm      (250G)
        137,  // Bronze Helm      (330G)
        138,  // Iron Helm        (450G)
        139,  // Barbut           (600G)

        // Body
        149,  // Leather Shirt    (270G)
        150,  // Silk Shirt       (400G)
        151,  // Leather Plate    (530G)
        152,  // Bronze Vest      (670G)
        177,  // Bronze Armor     (650G)

        // Consumables
        236,  // Potion           (50G)
        237,  // Hi-Potion        (200G)
        240,  // Phoenix Down     (150G)
        241,  // Echo Screen      (50G)
        242,  // Soft             (100G)
        243,  // Antidote         (50G)
        244,  // Eye Drops        (50G)
        245,  // Magic Tag        (100G)
        246,  // Vaccine          (100G)
        247,  // Remedy           (300G)
        248,  // Annoyntment      (150G)
        253,  // Tent             (800G)
        254,  // Ore              (300G)
    };

    // ── Fun pool — Mid tier ───────────────────────────────────────────────────

    /// <summary>
    /// "Fun" mode mid-tier draw pool.
    /// StiltzkinRandomizer draws exactly 1 item from this list per package.
    /// Solid mid-game gear — something you'd actually use.
    /// </summary>
    public static readonly IReadOnlyList<int> StiltzkinFunPoolMid = new[]
    {
        // Weapons
          3,  // Mythril Dagger   (950G)
          4,  // Gladius          (2300G)
          7,  // Butterfly Sword  (1300G)
          8,  // The Ogre         (1700G)
          9,  // Exploda          (2800G)
         10,  // Rune Tooth       (3800G)
         18,  // Mythril Sword    (1300G)
         20,  // Ice Brand        (3780G)
         21,  // Coral Sword      (4000G)
         22,  // Diamond Sword    (4700G)
         23,  // Flame Saber      (5190G)
         32,  // Mythril Spear    (1100G)
         33,  // Partisan         (1600G)
         34,  // Ice Lance        (2430G)
         35,  // Trident          (3580G)
         36,  // Heavy Lance      (4700G)
         37,  // Obelisk          (6000G)
         41,  // Cat's Claws      (4000G)
         42,  // Poison Knuckles  (5000G)
         43,  // Mythril Claws    (6500G)
         52,  // Multina Racket   (750G)
         53,  // Magic Racket     (1350G)
         54,  // Mythril Racket   (2250G)
         60,  // Healing Rod      (1770G)
         61,  // Asura's Rod      (3180G)
         62,  // Wizard Rod       (3990G)
         64,  // Golem's Flute    (2700G)
         65,  // Lamia's Flute    (3800G)
         66,  // Fairy Flute      (4500G)
         67,  // Hamelin          (5700G)
         71,  // Flame Staff      (1100G)
         72,  // Ice Staff        (980G)
         73,  // Lightning Staff  (1200G)
         74,  // Oak Staff        (2400G)
         75,  // Cypress Pile     (3200G)
         76,  // Octagon Rod      (4500G)
         77,  // High Mage Staff  (6000G)
         80,  // Needle Fork      (3100G)
         81,  // Mythril Fork     (4700G)
         87,  // Wing Edge        (3000G)

        // Armlets / gloves
         93,  // Magic Armlet     (1000G)
         94,  // Chimera Armlet   (1200G)
         95,  // Egoist's Armlet  (2000G)
         96,  // N-Kai Armlet     (3000G)
         97,  // Jade Armlet      (3400G)
         99,  // Dragon Wrist     (4800G)
        100,  // Power Wrist      (5100G)
        101,  // Bracer           (8000G)
        104,  // Mythril Gloves   (980G)
        105,  // Thunder Gloves   (1200G)
        106,  // Diamond Gloves   (2000G)
        107,  // Venetia Shield   (2800G)
        108,  // Defense Gloves   (6000G)

        // Helms
        121,  // Ritual Hat       (1000G)
        122,  // Twist Headband   (1200G)
        123,  // Mantra Band      (1500G)
        124,  // Dark Hat         (1800G)
        125,  // Green Beret      (2180G)
        126,  // Black Hood       (2550G)
        127,  // Red Hat          (3000G)
        128,  // Golden Hairpin   (3700G)
        129,  // Coronet          (4400G)
        130,  // Flash Hat        (5200G)
        131,  // Adaman Hat       (6100G)
        132,  // Thief Hat        (7100G)
        140,  // Mythril Helm     (1000G)
        141,  // Gold Helm        (1800G)
        142,  // Cross Helm       (2200G)
        143,  // Diamond Helm     (3000G)
        144,  // Platinum Helm    (4600G)
        145,  // Kaiser Helm      (7120G)

        // Body armor
        153,  // Chain Plate      (810G)
        154,  // Mythril Vest     (1180G)
        155,  // Adaman Vest      (1600G)
        156,  // Magician Cloak   (1850G)
        157,  // Survival Vest    (2900G)
        158,  // Brigandine       (4300G)
        159,  // Judo Uniform     (5000G)
        160,  // Power Vest       (7200G)
        168,  // Cotton Robe      (4000G)
        169,  // Silk Robe        (5800G)
        170,  // Magician Robe    (8000G)
        178,  // Linen Cuirass    (800G)
        179,  // Chain Mail       (1200G)
        180,  // Mythril Armor    (1830G)
        181,  // Plate Mail       (2320G)
        182,  // Gold Armor       (2950G)
        183,  // Shield Armor     (4300G)
        184,  // Demon's Mail     (5900G)
        185,  // Diamond Armor    (8800G)

        // Accessories
        192,  // Desert Boots     (1500G)
        194,  // Germinas Boots   (4000G)
        195,  // Sandals          (1200G)
        199,  // Anklet           (3200G)
        200,  // Power Belt       (7000G)
        202,  // Glass Buckle     (1600G)
        203,  // Madain's Ring    (7500G)
        206,  // Coral Ring       (4000G)
        212,  // Yellow Scarf     (1800G)
        213,  // Gold Choker      (4000G)
        214,  // Fairy Earrings   (6000G)
        216,  // Pearl Rouge      (4000G)
        217,  // Pearl Armlet     (980G)
        218,  // Cachusha         (3000G)
        219,  // Barette          (7000G)

        // Consumables
        238,  // Ether            (2000G)
        249,  // Phoenix Pinion   (2000G)
    };

    // ── Fun pool — High tier ──────────────────────────────────────────────────

    /// <summary>
    /// "Fun" mode high-tier draw pool.
    /// StiltzkinRandomizer draws exactly 1 item from this list per package.
    /// End-game and rare gear — the "jackpot" slot of the package.
    /// </summary>
    public static readonly IReadOnlyList<int> StiltzkinFunPoolHigh = new[]
    {
        // Late-game weapons
          6,  // Orichalcon       (17000G)
         11,  // Angel Bless      (7000G)
         12,  // Sargatanas       (9500G)
         13,  // Masamune         (13000G)
         14,  // The Tower        (30000G)
         15,  // Ultima Weapon    (40000G)
         24,  // Rune Blade       (8900G)
         25,  // Defender         (9340G)
         27,  // Ultima Sword     (14000G)
         28,  // Excalibur        (19000G)
         29,  // Ragnarok         (29000G)
         38,  // Holy Lance       (11000G)
         39,  // Kain's Lance     (15000G)
         40,  // Dragon's Hair    (23500G)
         44,  // Scissor Fangs    (8000G)
         45,  // Dragon's Claws   (10360G)
         46,  // Tiger Fangs      (13500G)
         47,  // Avenger          (16000G)
         48,  // Kaiser Knuckles  (18000G)
         50,  // Rune Claws       (28800G)
         55,  // Priest's Racket  (8000G)
         56,  // Tiger Racket     (5800G)
         63,  // Whale Whisker    (10280G)
         68,  // Siren's Flute    (7000G)
         69,  // Angel Flute      (8300G)
         78,  // Mace of Zeus     (10000G)
         82,  // Silver Fork      (7400G)
         83,  // Bistro Fork      (10300G)
         84,  // Gastro Fork      (13300G)

        // Late-game armlets / gloves
         98,  // Thief Gloves     (50000G)
        109,  // Genji Gloves     (missable steal)
        110,  // Aegis Gloves     (7000G)
        111,  // Gauntlets        (8800G)

        // Late-game helms
        133,  // Holy Miter       (8300G)
        134,  // Golden Skullcap  (12000G)
        135,  // Circlet          (13000G)
        146,  // Genji Helmet     (missable steal)
        147,  // Grand Helm       (14000G)

        // Late-game body armor
        161,  // Gaia Gear        (8700G)
        162,  // Demon's Vest     (10250G)
        163,  // Minerva's Plate  (12200G)
        164,  // Ninja Gear       (14000G)
        165,  // Dark Gear        (16300G)
        166,  // Rubber Suit      (20000G)
        167,  // Brave Suit       (22500G)
        171,  // Glutton's Robe   (16000G)
        172,  // White Robe       (29000G)
        173,  // Black Robe       (29000G)
        174,  // Light Robe       (40000G)
        175,  // Robe of Lords    (52000G)
        186,  // Platina Armor    (10500G)
        187,  // Carabini Mail    (12300G)
        188,  // Dragon Mail      (14000G)
        189,  // Genji Armor      (missable steal)
        190,  // Maximillian      (22600G)
        191,  // Grand Armor      (28000G)

        // Accessories
        197,  // Battle Boots     (21000G)
        198,  // Running Shoes    (33000G)
        201,  // Black Belt       (11000G)
        204,  // Rosetta Ring     (36000G)
        205,  // Reflect Ring     (7000G)
        207,  // Promist Ring     (9000G)
        208,  // Rebirth Ring     (10000G)
        209,  // Protect Ring     (40000G)
        215,  // Angel Earrings   (20000G)
        220,  // Extension        (10000G)
        221,  // Ribbon           (rare/chocograph)

        // Rare consumables and gems
        224,  // Garnet
        225,  // Amethyst
        226,  // Aquamarine
        227,  // Diamond
        228,  // Emerald
        229,  // Moonstone
        230,  // Ruby
        231,  // Peridot
        232,  // Sapphire
        233,  // Opal
        234,  // Topaz
        235,  // Lapis Lazuli
        239,  // Elixir
        250,  // Dark Matter
    };

    // ── Stiltzkin price ranges ────────────────────────────────────────────────
    //
    // Gil bounds for each StiltzkinPriceMode. All values are int16-safe (≤ 32,767)
    // because Stiltzkin's price is encoded as a signed int16 in the field bytecode
    // (SetTextVariable opcode for display + RemoveGil opcode for the actual charge).
    // StiltzkinRandomizer must patch BOTH opcodes when randomizing prices.

    /// <summary>Min price for <see cref="StiltzkinPriceMode.ClearanceSale"/>.</summary>
    public const int StiltzkinClearanceSaleMin = 1;

    /// <summary>Max price for <see cref="StiltzkinPriceMode.ClearanceSale"/>.</summary>
    public const int StiltzkinClearanceSaleMax = 99;

    /// <summary>Min price for <see cref="StiltzkinPriceMode.StiltzkinsMood"/>.</summary>
    public const int StiltzkinsMoodMin = 100;

    /// <summary>Max price for <see cref="StiltzkinPriceMode.StiltzkinsMood"/>.</summary>
    public const int StiltzkinsMoodMax = 10_000;

    /// <summary>Min price for <see cref="StiltzkinPriceMode.HighwayRobbery"/>.</summary>
    public const int HighwayRobberyMin = 5_000;

    /// <summary>Max price for <see cref="StiltzkinPriceMode.HighwayRobbery"/>.</summary>
    public const int HighwayRobberyMax = 15_000;

    // ── Challenging pool ──────────────────────────────────────────────────────

    /// <summary>
    /// "Challenging" pool — all 12 ability gems plus Phoenix Pinion and Dark Matter.
    /// Items in this pool are powerful but require AP investment to use — creating
    /// decision pressure: equip now and grind AP, or hold the gem for later?
    /// All 3 package slots draw from this pool independently.
    /// This is about strategic pressure, not price pressure (price is a separate axis).
    /// </summary>
    public static readonly IReadOnlyList<int> StiltzkinChallengingPool = new[]
    {
        224,  // Garnet
        225,  // Amethyst
        226,  // Aquamarine
        227,  // Diamond
        228,  // Emerald
        229,  // Moonstone
        230,  // Ruby
        231,  // Peridot
        232,  // Sapphire
        233,  // Opal
        234,  // Topaz
        235,  // Lapis Lazuli
        249,  // Phoenix Pinion
        250,  // Dark Matter
    };
}