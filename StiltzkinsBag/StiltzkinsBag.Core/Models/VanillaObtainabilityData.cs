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
        // ── Standard 24 chocograph rewards ──────────────────────────────────────
        {  29, 1 },   // Ragnarok           (Chocograph #21, Outer Island)        [was missing]
        {  45, 1 },   // Dragon's Claws     (Chocograph #7,  Forgotten Lagoon)    [was missing]
        {  48, 1 },   // Kaiser Knuckles    (Chocograph #15)
        {  74, 1 },   // Oak Staff          (Chocograph #11)
        {  77, 1 },   // High Mage Staff    (Chocograph #13)
        {  86, 8 },   // Rising Sun         (Chocograph #11)
        {  87, 13 },  // Wing Edge          (Chocos #3, #13)
        {  97, 1 },   // Jade Armlet        (Chocograph #3)
        { 106, 1 },   // Diamond Gloves     (Chocograph #5)
        { 168, 1 },   // Cotton Robe        (Chocograph #2)
        { 170, 1 },   // Magician Robe      (Chocograph #10)
        { 172, 1 },   // White Robe         (Chocograph #17)
        { 174, 1 },   // Light Robe         (Chocograph #18)
        { 183, 1 },   // Shield Armor       (Chocograph #8)
        { 184, 1 },   // Demon's Mail       (Chocograph #16)
        { 189, 1 },   // Genji Armor        (Chocograph #21)
        { 194, 2 },   // Germinas Boots     (Chocograph #1)
        { 196, 1 },   // Feather Boots      (Chocograph #9)
        { 208, 1 },   // Rebirth Ring       (Chocograph #24)
        { 210, 1 },   // Pumice Piece       (Chocograph #22)
        { 221, 1 },   // Ribbon             (Chocograph #24)
        { 224, 16 },  // Garnet             (Chocos #21, #5 area)
        { 225, 34 },  // Amethyst           (Chocos #21, #24)
        { 227, 1 },   // Diamond            (Chocograph #17)
        { 231, 23 },  // Peridot            (Chocos #5, #19)
        { 232, 26 },  // Sapphire           (Chocos #19, #22)
        { 233, 23 },  // Opal               (Chocos #16, #19)
        { 234, 19 },  // Topaz              (Chocograph #19)
        { 236, 78 },  // Potion             (Chocos #2 as "Potions", #8, #23)
        { 237, 18 },  // Hi-Potion          (Chocos #1, #2 as "Hi-Potions", #6)
        { 238, 28 },  // Ether              (Chocos #1, #7, #9, #13)
        { 239, 6 },   // Elixir             (Chocos #1, #11)
        { 240, 21 },  // Phoenix Down       (Chocos #5, #15, as "Phoenix Down")
        { 241, 7 },   // Echo Screen        (Chocograph #6)
        { 242, 6 },   // Soft               (Chocograph #9)
        { 243, 10 },  // Antidote           (Chocograph #3 as "Antidotes")
        { 245, 6 },   // Magic Tag          (Chocograph #8)
        { 247, 4 },   // Remedy             (Chocograph #11)
        { 248, 15 },  // Annoyntment        (Chocograph #23)
        { 249, 24 },  // Phoenix Pinion     (Chocos #5, #15)
        { 253, 3 },   // Tent               (Chocos #2 as "Tents", #6)
        { 254, 58 },  // Ore                (Chocos #15, #16, #18)
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
    ///
    /// All 8 events confirmed by WorldMapVariableScanner scanning the 13 world map
    /// .eb.bytes files (evt_world_world00 – evt_world_world12):
    ///
    ///   Convention-A blocks (vars 37–40/42–45) in world00/03/05/07/08/09:
    ///     [Ocean] Between Mist Continent and Outer Continent:
    ///       Straw Hat×8, Pearl Armlet×8, Aloha T-shirt×7, Sandals×8
    ///     [Ocean] Directly North of Iifa Tree:
    ///       Potion×50, Hi-Potion×25, Ether×9, Elixir×7
    ///     [Ocean] South Tip of Forgotten Continent:
    ///       Remedy×10, Black Robe×1, Genji Gloves×1, Blue Narciss Card (excluded)
    ///     [Crack] Northeastern Forgotten Continent:
    ///       Eye Drops×19, Madain's Ring×1, Genji Helmet×1, Hilda Garde I Card (excluded)
    ///     [Crack] Near Oeilvert:
    ///       Maiden Prayer×1, Dragon's Hair×1, Gauntlets×1, Odin Card (excluded)
    ///     [Crack] Eastern Lost Continent:
    ///       Lapis Lazuli×41, Rosetta Ring×1, Protect Ring×1, Airship Card (excluded)
    ///     [Other] Unmarked ocean (Shimmering Island area):
    ///       Aquamarine×10, Ultima Weapon×1, Maximillian×1, Invincible Card (excluded)
    ///
    ///   Convention-B block (vars 2–5/7–10) in world12 only:
    ///     [Ocean] Beneath Quan's Dwelling:
    ///       Ore×9, Topaz×15, Tiger Racket×1, Red Rose Card (excluded)
    ///
    /// Card IDs (>255) are excluded from all counts.
    /// Items that are also infinite from other sources (shops, auctions) are still
    /// listed here for flag completeness; their catalog availability is int.MaxValue.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, int> DeadPepperItemCounts = new Dictionary<int, int>
    {
        //  ID    Count   Item name              Event / dig site
        {  15,  1 },   // Ultima Weapon         Unmarked ocean (Shimmering Island area)
        {  40,  1 },   // Dragon's Hair         Near Oeilvert crack
        {  56,  1 },   // Tiger Racket          Beneath Quan's Dwelling ocean
        { 109,  1 },   // Genji Gloves          South Tip of Forgotten Continent ocean
        { 111,  1 },   // Gauntlets             Near Oeilvert crack
        { 113,  8 },   // Straw Hat             Between Mist Continent/Outer Continent ocean
        { 146,  1 },   // Genji Helmet          Northeastern Forgotten Continent crack
        { 148,  7 },   // Aloha T-shirt         Between Mist Continent/Outer Continent ocean
        { 173,  1 },   // Black Robe            South Tip of Forgotten Continent ocean
        { 190,  1 },   // Maximillian           Unmarked ocean (Shimmering Island area)
        { 195,  8 },   // Sandals               Between Mist Continent/Outer Continent ocean
        { 203,  1 },   // Madain's Ring         Northeastern Forgotten Continent crack
        { 204,  1 },   // Rosetta Ring          Eastern Lost Continent crack
        { 209,  1 },   // Protect Ring          Eastern Lost Continent crack
        { 217,  8 },   // Pearl Armlet          Between Mist Continent/Outer Continent ocean
        { 222,  1 },   // Maiden Prayer         Near Oeilvert crack
        { 226, 10 },   // Aquamarine            Unmarked ocean (Shimmering Island area)
        { 234, 15 },   // Topaz                 Beneath Quan's Dwelling ocean
        { 235, 41 },   // Lapis Lazuli          Eastern Lost Continent crack
        { 236, 50 },   // Potion                Directly North of Iifa Tree ocean
        { 237, 25 },   // Hi-Potion             Directly North of Iifa Tree ocean
        { 238,  9 },   // Ether                 Directly North of Iifa Tree ocean
        { 239,  7 },   // Elixir                Directly North of Iifa Tree ocean
        { 244, 19 },   // Eye Drops             Northeastern Forgotten Continent crack
        { 247, 10 },   // Remedy                South Tip of Forgotten Continent ocean
        { 254,  9 },   // Ore                   Beneath Quan's Dwelling ocean
    };

    // ── World map variable-delivery items (canonical fallback) ────────────────

    /// <summary>
    /// Hardcoded fallback for WorldMapVariableScanner when the real .eb.bytes files
    /// are not available (unit tests, offline mode).
    ///
    /// Covers ALL items delivered via variable-reference AddItem in world map scripts
    /// (FieldItemScanner cannot resolve these):
    ///   • Convention A / B — the 26 Dead Pepper dig-site rewards (see DeadPepperItemCounts)
    ///   • Convention C     — chocograph World_Chest function rewards confirmed by binary
    ///                        analysis: Ragnarok (29) and Dragon's Claws (45)
    ///
    /// When WorldMapVariableScanner is run against the actual game files, its output
    /// supersedes this dictionary via the worldMapCounts parameter of Build().
    /// </summary>
    public static readonly IReadOnlyDictionary<int, int> WorldMapVariableItemCounts =
        new Dictionary<int, int>(DeadPepperItemCounts)
        {
            // ── Convention C: chocograph World_Chest variable-delivery items ─────
            {  29, 1 },  // Ragnarok      — Chocograph #21, Outer Island  (World_Chest case +20)
            {  45, 1 },  // Dragon's Claws — Chocograph #7,  Forgotten Lagoon (World_Chest case +6)
        };

    // ── Treno Auction House ────────────────────────────────────────────────────

    /// <summary>
    /// Items available at the Treno Auction House that can be won multiple times.
    /// These are effectively infinite obtainability sources.
    ///
    /// Includes: Magician Robe (170), Fairy Earrings (214), Madain's Ring (203),
    ///           Pearl Rouge (216), Elixir (239), Feather Boots (196), Anklet (199),
    ///           Promist Ring (207).
    ///
    /// Note: Feather Boots also appear as a boss drop and chocograph reward.
    ///       Elixir also appears as a friendly monster reward.
    ///       The infinite auction source dominates — these will be int.MaxValue overall.
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
    ///
    /// Includes: Reflect Ring (205), Dark Matter (250), Thief Gloves (98), Ribbon (221).
    /// Note: Reflect Ring also appears as a boss drop.
    ///       Ribbon also appears in Chocograph #24 and Stiltzkin's final reward (Phase 5.9).
    ///       Dark Matter also appears as a boss drop.
    ///       The catalog sums all finite instances.
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
    ///
    /// Note: Several rewards (Potion, Hi-Potion, Ether, Elixir) are also available
    /// from repeatable sources, so those specific item IDs will be int.MaxValue overall.
    /// The gems (Emerald/Moonstone/Lapis Lazuli/Diamond) are one-time here.
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
    ///
    /// Ragtime Mouse is classified as Normal-type in the enemy guide, but the
    /// quiz reward is a scripted one-time give, NOT a repeatable enemy drop.
    /// Protect Ring (209) is intentionally EXCLUDED from NormalEnemyItemIds to
    /// prevent it from being incorrectly treated as an infinite source.
    /// VanillaItemCatalog adds exactly 1 to the finite total for this reward.
    /// </summary>
    public static readonly IReadOnlySet<int> RagtimeMouseItemIds = new HashSet<int>
    {
        209,  // Protect Ring  (Ragtime Mouse quiz completion reward — one-time)
    };

    // ── Sentinel-excluded field items ──────────────────────────────────────────

    /// <summary>
    /// Items that FieldItemScanner cannot count accurately because their item ID
    /// is used as a null/default sentinel in many field scripts.
    /// Each entry maps item ID → verified real count from game knowledge.
    ///
    /// <para>
    /// Item 0 (Hammer): given once in the Tantalus hideout on Disc 3.
    /// <c>AddItem(0, X)</c> appears as a placeholder in many field scripts and
    /// produces a false count of 85. FieldItemScanner filters item ID 0 entirely.
    /// VanillaItemCatalog merges this map into effective field counts so the
    /// Hammer still appears correctly as a finite-1 field item in the catalog.
    /// </para>
    /// </summary>
    public static readonly IReadOnlyDictionary<int, int> SentinelExcludedFieldItems = new Dictionary<int, int>
    {
        { 0, 1 },  // Hammer — Field 1911 (Treno / Queen Stella's house), Stellazzio quest reward.
                   // Item ID 0 is also used as a null/placeholder sentinel in many other field
                   // scripts, causing 84 false-positive AddItem(0,X) detections. FieldItemScanner
                   // filters item 0 entirely; this entry provides the one verified real give.
    };

    // ── Kupo Nut rewards ───────────────────────────────────────────────────────

    /// <summary>
    /// Item IDs rewarded by the Mognet/Kupo Nut quest chain.
    /// One reward is given per disc (4 discs, 4 rewards), each one-time finite.
    ///
    /// Disc 1: Holy Bell — key item, no standard ID in Items.csv (0–255); not cataloged.
    /// Disc 2: Elixir (239) — also infinite via AuctionRepeatable; flag retained for metadata.
    /// Disc 3: Extension (220) — finite; expected in evt_ field scripts (deliver sequence).
    /// Disc 4: Aloha T-Shirt (148) — also obtainable via Dead Pepper bundle (7×).
    ///
    /// VALIDATION-REFERENCE ONLY (Phase 5.9.2, 2026-03-28).
    /// VanillaItemCatalog does NOT count these additively — game files are the source
    /// of truth. If these items are delivered via AddItem in evt_ field scripts, the
    /// FieldItemScanner will find them. This set is retained for:
    ///   • Future cross-validation: scanner output should include these IDs
    ///   • Metadata reference for Phase 6 constraint logic
    ///
    /// Re-enable additive counting if: Kupo Nut rewards are confirmed NOT to appear
    /// in any evt_ field script and have no other scanner-reachable source.
    ///
    /// Source: FFIX GUIDE DATA/KupoNutRewards.csv
    /// </summary>
    public static readonly IReadOnlySet<int> KupoNutItemIds = new HashSet<int>
    {
        239,  // Elixir        (Disc 2)
        220,  // Extension     (Disc 3)
        148,  // Aloha T-Shirt (Disc 4)
    };

    // ── Missable items ─────────────────────────────────────────────────────────
    //
    // Items where at least one obtainable copy requires specific story timing,
    // a one-time steal/drop, or a permanently-closing shop.
    //
    // Source: FFIX GUIDE DATA/Missable Items.txt (guide-derived — game binary
    // cannot determine missability from opcodes alone).
    //
    // Scope: explicitly-named items only. Location-based missables ("all chests
    // in Crash Site/Evil Forest/Ice Cavern/Cleyra/Fossil Roo/Gargan Roo") are
    // tracked at the field-location level in Phase 6 — not listed here.
    // Key items (Autograph, Moogle Suit, Master Hunter, Athlete Queen,
    // Mini-Brahne, Mayor's Key) are inherently missable but excluded from all
    // randomization pools regardless, so they are not listed here either.
    //
    // NOTE: IsMissable is metadata only — does not affect ObtainabilityCounts,
    // IsFiniteOnly, or pool candidacy. RecommendedLogicEngine (Phase 6) uses
    // this flag for placement constraint enforcement and cap warnings.
    /// <summary>
    /// Item IDs that have at least one missable acquisition opportunity.
    /// Tagged from <c>FFIX GUIDE DATA/Missable Items.txt</c>.
    /// </summary>
    public static readonly IReadOnlySet<int> MissableItemIds = new HashSet<int>
    {
        // ── Permanently-closing shop windows ──────────────────────────────────
        18,   // Mythril Sword    — last chance: Esto Gaza shop (before Desert Palace)
        51,   // Air Racket       — last chance: Treno Weapon Shop disc 2 (before Gargant)
        103,  // Silver Gloves    — last chance: Summit Station shop
        138,  // Iron Helm        — last chance: Summit Station shop
        179,  // Chain Mail       — last chance: Lindblum Weapon Shop disc 2 (before Dragon's Gate)

        // ── Story-point pickups ────────────────────────────────────────────────
        31,   // Javelin          — buy from Lindblum Weapon Shop before entering Cleyra
        229,  // Moonstone        — Sword Fight: impress 100 nobles, then speak to Queen Brahne
        228,  // Emerald          — #1: Cleyra cathedral (speak to High Priest before settlement);
              //                    #2: Stiltzkin package #6 (888 Gil, Oeilvert)
        227,  // Diamond          — #1: Conde Petie kirkboat;
              //                    #3: Stiltzkin package #7 (2,222 Gil, Bran Bal)
        208,  // Rebirth Ring     — win Treno Card Tournament (disc 2)

        // ── One-time steals / drops ────────────────────────────────────────────
        19,   // Blood Sword      — steal from Tiamat (Memoria)
        109,  // Genji Gloves     — steal from Lich (Memoria)
        146,  // Genji Helmet     — steal from Kraken (Memoria)
        189,  // Genji Armor      — steal from Maliris (Memoria)
        198,  // Running Shoes    — steal from Tantarian (Alexandria Library);
              //                    steal from Hades (Memoria side quest)
        30,   // Excalibur II     — reach "Gate to Space" / defeat Lich in < 12 h game clock
        49,   // Duel Claws       — steal from Deathguise (Memoria)
        210,  // Pumice Piece     — steal from Necron (final boss)
        250,  // Dark Matter      — steal / drop from Necron (final boss)
    };

    // ── Stiltzkin encounters ───────────────────────────────────────────────────
    // Phase 5.9 — intentionally deferred.
    // StiltzkinRandomizer will register Stiltzkin's 8 locations and their items here,
    // or pass them directly to VanillaItemCatalog via its own scanner.

    // ── Stiltzkin visit locations ──────────────────────────────────────────────

    /// <summary>
    /// Confirmed evt_ field script names for each of Stiltzkin's 8 visits.
    /// Key = package price in Gil. Value = array of script names to patch
    /// (most visits have 1 script; Cleyra 444G is a paired field — both scripts
    /// must be patched identically, same architecture as disc-variant field pairs).
    ///
    /// Script names confirmed by StiltzkinFieldDiagnosticTests (2026-03-31)
    /// via SetTextVariable(0, price) scan of p0data7.bin.
    ///
    /// Package contents from Final_Fantasy_IX_Reference_Guide_-_Slitzkin_Locations.csv:
    ///   333G  Burmecia              — Soft, Hi-Potion, Ether
    ///   444G  Cleyra                — Ether, Hi-Potion, Phoenix Pinion   (missable)
    ///   555G  Fossil Roo            — Phoenix Pinion, Remedy, Ether
    ///   666G  Conde Petie Mtn Path  — Magic Tag, Tent, Ether
    ///   777G  Alexandria (1st)      — Phoenix Pinion, Hi-Potion, Elixir
    ///   888G  Oeilvert              — Hi-Potion, Emerald, Elixir
    ///  2222G  Bran Bal              — Diamond, Ether, Elixir
    ///  5555G  Alexandria (final)    — Moonstone, Ruby, Elixir
    ///          → Bonus Ribbon if all 8 packages purchased
    ///
    /// Used by StiltzkinRandomizer (Phase 5.99) to locate and patch package items.
    /// </summary>
    public static readonly IReadOnlyDictionary<int, string[]> StiltzkinVisitLocations =
        new Dictionary<int, string[]>
        {
        {  333, ["EVT_BURMECIA_SQUARE_1.eb"] },                               // Burmecia
        {  444, ["EVT_CLEYRA3_ANTRION.eb", "EVT_CLEYRA3_INN.eb"] },          // Cleyra (paired)
        {  555, ["EVT_FOSSIL_FR_DN1_0.eb"] },                                 // Fossil Roo
        {  666, ["EVT_PATA_M_CM_MP3_0.eb"] },                                 // Conde Petie Mtn Path
        {  777, ["EVT_ALEX3_AT_SENTOU.eb"] },                                 // Alexandria (1st)
        {  888, ["EVT_OEIL_UV_DEP_0.eb"] },                                   // Oeilvert
        { 2222, ["EVT_BAL_BB_WPS_0.eb"] },                                    // Bran Bal
        { 5555, ["EVT_ALEX5_AT_SENTOU.eb"] },                                 // Alexandria (2nd/final)
        };
}
