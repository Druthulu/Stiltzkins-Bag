# Stiltzkin's Bag — Project Context & Roadmap

> **Version:** 1.0.1 (static — see PhaseEnd files for current version and phase status)
> **Generated:** 2026-03-17
> **Generation:** Gen1 | **Tech Stack:** C# .NET 8, WPF + MaterialDesignInXaml, mod-folder output for Memoria Engine

---

## For Humans — Quick Guide

This is your project's permanent reference document. It contains everything an AI assistant
needs to understand your project from any point.

**How to use it:**
1. Attach this file at the start of every new AI chat session
2. Attach ALL PhaseEnd markdown files (PhaseEnd_Phase1.md, PhaseEnd_Phase2.md, etc.) alongside it
3. The AI reads all PhaseEnd files in order to reconstruct build history, current version, phase status, and rules added over time
4. This file is never edited or replaced — it is a static reference for the life of the project

**Session start prompt:**
> "Continue building Stiltzkin's Bag. We are currently on Phase [X], working on [specific task]."

---

## Project Overview

**Project name:** Stiltzkin's Bag
**Origin:** Complete ground-up rewrite of rand9er v2.2 (WinForms, .NET 4.7.2)
**Namespace convention:** `StiltzkinsBag` (Core), `StiltzkinsBag.App` (App/UI)

Stiltzkin's Bag is a seed-based randomizer for Final Fantasy IX (PC / Steam version). It reads game data files, applies randomization according to user-selected settings and a numeric seed, and outputs a Memoria Engine mod folder containing modified CSV data files and individually patched binary field/battle files. Players load the mod folder via Memoria's overlay system to play a randomized run. The same seed + settings always produces the exact same output, enabling competitive races and community seeds.

### Generation Map

| Generation | Focus | Status |
|---|---|---|
| Gen1 | Full mod-folder randomizer — all CSV + bytecode features, WPF UI, Hades Workshop parser integration | In Progress |
| Gen2 | Runtime companion DLL, field entrance randomizer, enemy encounter randomizer | Pending Gen1 stable release |
| Gen3 | Level-scaling integration, story cutscene item acquisition randomization, race/blind mode | Pending Gen2 field map complete |

### Project Assumptions

- **Developer:** Solo, intermediate C# experience, comfortable with WPF learning curve. QA engineer by trade.
- **Target audience:** Speedrunners, racers, challenge runners, casual players
- **Platform:** Windows only (FF9 PC/Steam is Windows-primary)
- **Seed reproducibility is non-negotiable** — same seed + same settings must always produce byte-identical output
- **Memoria Engine is a prerequisite** — users must have Memoria installed; Stiltzkin's Bag does not install or configure Memoria
- **Hades Workshop** source is C++ (GNU GPL); dev has personal permission from author to use freely; project must remain GPL-compatible
- **No runtime patching in Gen1** — all randomization is mod-folder output only

---

## Lessons Learned / Known Risks

| Problem / Risk | Detail | Mitigation |
|---|---|---|
| Broken RNG in v2 | `new Random(derived_formula)` per cell — adding/removing features shifts all other outputs for same seed | Single `Random` instance seeded once, called sequentially in fixed deterministic order |
| Global static state on Form class | All data was static fields on the WinForms class — untestable, hard to extend | Clean settings model + pure engine class with zero UI dependencies |
| Raw string CSV manipulation | Split/join with magic index offsets baked in everywhere — fragile on Memoria updates | Typed model classes per CSV, CsvHelper for parsing |
| Byte string matching for binary data | Pattern-match approach works but is brittle and requires shipping a 435KB enemy dictionary | Port Hades Workshop parsers (UnityArchiver, Enemies, Fields, MIPS) to C# for structured access |
| Memoria CSV format changes | File structure has already changed between old and new versions (new files added) | Always read from game's actual StreamingAssets files at generation time, never rely on embedded copies |
| Item shuffle breaks bytecode references | If items.csv order is shuffled, all bytecode references to item IDs in enemies/chests become wrong | Item randomization runs first and produces a remap table; all bytecode editors consume that table |

---

## Project Philosophy

> **Every seed should be completable, every run should be surprising, and every output should be deterministic.**

The randomizer must never produce an unwinnable state. Legendary items must remain rare. Key items required to complete the game must always be obtainable. Characters should feel coherent even when randomized — their speciality, abilities, and equipment should tell a consistent story. The Recommended mode enforces this automatically; Chaos mode removes the guardrails for experienced players who want maximum chaos.

---

## Key Decisions

| Decision | Chosen | Rejected | Why |
|---|---|---|---|
| Output method | Mod-folder file overlay | Runtime DLL (Gen1) | Proven approach, testable output, no Unity process debugging, existing bytecode work ports directly |
| UI framework | WPF + MaterialDesignInXaml | WinForms, Electron, Avalonia | Modern feel, custom FF9 theming possible, pure C# XAML, no JS runtime overhead |
| Binary parsing | Port Hades Workshop C++ parsers to C# | Ship enemy byte dictionary, raw byte matching | Structured access, smaller mod output, no brittle pattern matching, surgical edits |
| RNG architecture | Single `Random` seeded once, sequential calls | Per-cell `new Random(derived_formula)` | Deterministic seed reproducibility across feature combinations |
| Project structure | Single solution, two projects (Core + App) | Monolithic or many assemblies | Core is testable without UI; keeps engine/UI cleanly separated at intermediate scale |
| Target framework | .NET 8 | .NET 4.7.2 (old project) | Modern C#, better performance, current LTS |

---

## Core Logic / Strategy

### The Mod Folder Overlay System

Memoria Engine loads files from mod folders before the base game's StreamingAssets. A mod folder mirrors the game's directory structure. Any file present in the mod folder at the correct relative path will be used instead of the base game version. Stiltzkin's Bag creates a named seed folder (e.g. `StiltzkinsBag-Seed-42069`) under the game's Mods directory, populates it with randomized CSVs and individually patched binary files, and registers it in Memoria's mod load order.

### Randomization Pipeline (in strict order)

1. **Seed resolution** — user's seed string is hashed/compressed to a deterministic integer `SeedInt`; a single `Random(SeedInt)` instance is created and used for the entire run
2. **Item remap** (if item shuffle enabled) — assign new item ID mapping; produce `ItemRemapTable` for all downstream consumers
3. **Enemy binary patches** — enemy drops, steals, blue magic, card drops; consume `ItemRemapTable` if active
4. **Field binary patches** — chest contents; consume `ItemRemapTable` if active
5. **Character pipeline** (Recommended: in order; Chaos: independent)
   - Base stats → determine class/speciality assignment
   - Speciality → update `CharacterParameters.csv` + `CommandSets.csv`
   - Abilities → update per-character ability CSVs (`Zidane.csv`, etc.)
   - Equipment → update `DefaultEquipment.csv`, respecting ability assignments
   - Starting items → update `InitialItems.csv` using validated consumable pool
6. **Gear stat bonuses** — update `BonusId` values in `Weapons.csv`, `Armors.csv`, `Items.csv`
7. **Shops** — update `ShopItems.csv`
8. **Synthesis** — update `Synthesis.csv`; includes new unique synthesis recipes (Bonus Sets)
9. **Ability gems** — update `AbilityGems.csv`
10. **Tetramaster cards** — randomize card stats and NPC deck contents via bytecode
11. **Recommended Logic validation** — constraint pass over all outputs; correct violations
12. **Spoiler log generation** — write human-readable log of all changes
13. **Mod folder output** — write all modified CSVs and patched binary files to seed folder

### Character Speciality Assignment (Recommended Mode)

After base stats are randomized, each of the 8 main party characters is assigned a **speciality** by filling their 4 command slots (`Regular1, Regular2, Trance1, Trance2` in `CommandSets.csv`) from the class archetype list below.

**Key design principle — decoupled command pairs:**
Each class archetype owns one or two battle commands. Some archetypes' command pairs are **inseparable** (locked together by game mechanics); others are **separable** (can be split between two different characters). This allows a character to hold two class identities if their 4 slots are filled from two different archetypes — creating interesting hybrid builds.

#### Class Archetypes

| Nickname | Commands (CSV index) | Separable? | Stat Assignment Rule |
|---|---|---|---|
| **Thief** | Steal (2) | ✅ separable from Barbarian | Highest Dex |
| **Barbarian** | Skill (25), Dyne (26) | ✅ separable from Thief | Highest Str |
| **Wizard1** | Focus (12) | ✅ solo command | High Magic, 3rd rank |
| **Wizard2** | BlkMag (22), DblBlk (23) | ✅ separable from Wizard1 | Lowest Str, med Magic |
| **Cleric** | WhtMag (17) | ✅ separable from KonDar | High Magic |
| **KonDar** | Summon (16), Eidolon (18) | ✅ separable from Cleric | Highest Magic, 2nd lowest Str |
| **Fighter** | SwdArt (30) | ✅ separable from Warlock1 | High Magic, 3rd highest Will |
| **Warlock1** | SwdMag (31) | ✅ separable from Fighter | Med Magic + physical attack |
| **Druid** | Dragon (27) | ✅ solo command | 2nd highest Will, high Magic |
| **Rogue1** | Jump (3), Jump2 (12) | ✅ separable from Rogue2 | 3rd highest Str, 2nd highest Dex |
| **BaChingChing** | BluMag (24), Eat (8), Cook (9) | 🔒 **LOCKED** — cannot separate | Lowest Will, med Magic |
| **Summoner** | Summon (20) | ✅ separable from WhiteMage | 2nd highest Magic (Eiko-style summons) |
| **WhiteMage** | WhtMag (19), DblWht (21) | ✅ separable from Summoner | Med Magic |
| **Rogue2** | Throw (15) | ✅ solo command | 2nd lowest Magic |
| **Warlock2** | Flair (28), Elan (29) | ✅ separable from Warlock1 | Med Str/Dex/Will |

#### Assignment Algorithm (Recommended Mode)

The stat ranking engine produces five ordered arrays (`dexOrder[]`, `strOrder[]`, `magOrder[]`, `willOrder[]`, `gemsOrder[]`) where index `[0]` is the highest-ranked character and `[7]` is the lowest. Assignment is **greedy, most-constrained-first** — the most specific stat requirements are locked in first.

**Assignment priority order:**

1. **BaChingChing** (locked) → `willOrder[7]` — lowest Will; owns all 4 slots (Eat, BluMag, Cook, BluMag). This character is fully consumed and removed from the pool.
2. **Thief** → `dexOrder[0]` — highest Dex; fills `Regular1` slot. Character remains available for a second class in `Regular2/Trance` slots.
3. **KonDar** → `magOrder[0]` where `strOrder rank >= 6` — highest Magic AND near-lowest Str; fills `Regular1+Trance1`. Character removed.
4. **Summoner** → `magOrder[1]` where available — 2nd highest Magic; fills `Regular2` slot.
5. **Cleric** → `magOrder[2]` or `willOrder[0]` — high Magic or highest Will; fills a slot.
6. **Barbarian** → `strOrder[0]` — highest Str; fills `Regular1+Trance2` slots. Can share a character with Thief if dex-order and str-order happen to align.
7. **Druid** → `willOrder[1]` with high Magic — fills `Regular2` slot.
8. **Fighter** → next available med-Will/high-Magic — fills `Regular1`.
9. **Warlock1 or Warlock2** → remaining med Str/Dex/Will characters.
10. **Wizard2** → `strOrder[7]` (lowest Str) with med Magic.
11. **WhiteMage** → next available med-Magic character.
12. **Rogue1** → 3rd highest Str / 2nd highest Dex.
13. **Rogue2** → 2nd lowest Magic.
14. **Wizard1** → last remaining (filler Focus command).
15. **Warlock1/2 remainder** → any unfilled command slots on already-assigned characters.

**Hybrid example:** A character assigned Thief (`Regular1 = Steal`) and Barbarian (`Regular2 = Skill, Trance1 = Dyne`) is a legitimate hybrid if they have both high Dex and high Str. Their ability pool and equipment in subsequent pipeline steps will draw from both class profiles.

**Chaos mode:** Class assignment is fully random — each of the 8 characters gets 4 command slots drawn from the archetype pool without any stat constraint. BaChingChing's locked triplet (Eat/Cook/BluMag) is treated as a single atomic unit and assigned as a block to one random character.

### Component Inventory

| Component | Role | Status |
|---|---|---|
| `SeedEngine` | Seed string → deterministic int, single `Random` instance | ✅ Phase 1 |
| `Settings` | All user toggle state, serializable | ✅ Phase 1 |
| `GenerationResult` | Output summary, error list | ✅ Phase 1 |
| `ItemRemapTable` | Item ID before/after shuffle | ✅ Phase 1 |
| `CsvParser<T>` / `CsvWriter<T>` | Typed CSV round-trip for all Memoria CSV formats | Phase 2 |
| `UnityArchiver` (ported from HW) | Extract/pack individual files from Unity archives | Phase 3 |
| `EnemyParser` (ported from HW) | Parse enemy bytecode structs from raw bytes | Phase 3 |
| `FieldParser` (ported from HW) | Parse field scripts, locate chest/item opcodes | Phase 3 |
| `MipsEditor` (ported from HW) | Edit MIPS opcodes in parsed field/enemy data | Phase 3 |
| `ItemRemapper` | Produces and applies item ID remap table | Phase 4 |
| `EnemyRandomizer` | Drops, steals, blue magic, card drops | Phase 4 |
| `ChestRandomizer` | Chest contents in field files | Phase 4 |
| `CharacterRandomizer` | Full character pipeline (stats→class→abilities→equipment) | Phase 5 |
| `GearStatRandomizer` | BonusId values with balance constraints | Phase 5 |
| `ShopRandomizer` | ShopItems.csv | Phase 5 |
| `SynthesisRandomizer` | Synthesis.csv + Bonus Sets | Phase 5 |
| `AbilityGemsRandomizer` | AbilityGems.csv | Phase 5 |
| `TetraMasterRandomizer` | Tetramaster card stats + order + NPC decks via bytecode | Phase 5 |
| `InitialItemsRandomizer` | InitialItems.csv (consumable pool only) | Phase 5 |
| `RecommendedLogicEngine` | Constraint validation and correction pass | Phase 6 |
| `ModOutputWriter` | Writes seed folder, manages Memoria.ini load order, writes ModDescription.xml, mod-local Memoria.ini overrides | Phase 8 |
| `SpoilerLogWriter` | Generates human-readable change log | Phase 8 |
| `SettingsFileWriter` | Writes seed + settings to Settings.json in seed folder | Phase 8 |
| `ModDescriptionWriter` | Writes ModDescription.xml in seed folder | Phase 8 |
| `ModMemoriaIniWriter` | Writes mod-local Memoria.ini overrides in seed folder | Phase 8 |
| `MemoriaLoadOrder` | Reads/writes base Memoria.ini FolderNames | Phase 8 |
| `MainViewModel` | WPF MVVM binding layer | Phase 7 |
| `DebugWindow` | Port of debug form, gated by dev flag | Phase 9 |

---

## Safety / Guardrails / Error Handling

| Mechanism | What It Does | Trigger |
|---|---|---|
| Key item protection list | Prevents mechanically required items from being removed or made unobtainable | Recommended mode, always enforced |
| Legendary item rarity guard | Prevents Ultima Weapon, Excalibur II, etc. from appearing in shops or as common drops | Recommended mode |
| Synthesis ingredient reachability | Validates that synthesis recipe ingredients are obtainable before that recipe's shop is unlocked | Recommended mode, Phase 6 |
| Character equipment ability alignment | Ensures equipment assigned to a character carries abilities appropriate to their randomized speciality | Recommended mode |
| Gear bonus budget constraint | Randomized BonusId values must stay within ±20% of the base game's per-item average total bonus points; hard cap of +3 per stat per item for non-legendary items | All modes |
| Consumable-only starting items | InitialItems randomization draws only from items 236–253 (consumable pool); no equipment in starting inventory | All modes |
| Remap table propagation | If item shuffle is enabled, `ItemRemapTable` must be applied to all bytecode editors before any binary output is written | Always when item shuffle is active |
| Duplicate unique item prevention | Unique/legendary items (quantity-1 items) cannot appear in more than one chest, enemy drop, or shop listing | All modes |
| Mod load order safety | Before writing, verify Memoria mod folder structure exists and is writable; never overwrite a non-SB mod | Always |

---

## Architecture

### Namespace Conventions

| Project | Root Namespace | Example |
|---|---|---|
| `StiltzkinsBag.Core` | `StiltzkinsBag` | `StiltzkinsBag.Models`, `StiltzkinsBag.Randomizers` |
| `StiltzkinsBag.App` | `StiltzkinsBag.App` | `StiltzkinsBag.App.Views`, `StiltzkinsBag.App.ViewModels` |
| `StiltzkinsBag.Tests` | `StiltzkinsBag.Tests` | `StiltzkinsBag.Tests` |

### Project Structure

```
StiltzkinsBag.sln
├── StiltzkinsBag.Core/               # Engine — zero UI dependencies
│   ├── Models/
│   │   ├── Settings.cs               # ✅ All user toggles, seed, game path
│   │   ├── GenerationResult.cs       # ✅ Output summary, error list
│   │   ├── ItemRemapTable.cs         # ✅ Item ID before/after shuffle
│   │   └── Csv/                      # Typed models for each CSV file
│   │       ├── BaseStatsRow.cs
│   │       ├── ItemsRow.cs
│   │       ├── ShopItemsRow.cs
│   │       ├── SynthesisRow.cs
│   │       ├── CommandSetsRow.cs
│   │       ├── CharacterAbilityRow.cs
│   │       ├── DefaultEquipmentRow.cs
│   │       ├── AbilityGemsRow.cs
│   │       ├── InitialItemsRow.cs
│   │       └── (etc. — one per Memoria CSV)
│   ├── Parsing/
│   │   ├── CsvParser.cs              # Generic typed CSV reader/writer
│   │   ├── UnityArchiver.cs          # Ported from HW: pack/unpack unity archives
│   │   ├── EnemyParser.cs            # Ported from HW: enemy bytecode structs
│   │   ├── FieldParser.cs            # Ported from HW: field scripts, chest opcodes
│   │   └── MipsEditor.cs             # Ported from HW: MIPS opcode editing
│   ├── Randomizers/
│   │   ├── SeedEngine.cs             # ✅ Seed string → int, single Random instance
│   │   ├── ItemRemapper.cs
│   │   ├── EnemyRandomizer.cs
│   │   ├── ChestRandomizer.cs
│   │   ├── CharacterRandomizer.cs
│   │   ├── GearStatRandomizer.cs
│   │   ├── ShopRandomizer.cs
│   │   ├── SynthesisRandomizer.cs
│   │   ├── AbilityGemsRandomizer.cs
│   │   ├── TetraMasterRandomizer.cs
│   │   └── InitialItemsRandomizer.cs
│   ├── Logic/
│   │   ├── RecommendedLogicEngine.cs
│   │   ├── KeyItemProtectionList.cs
│   │   └── LegendaryItemList.cs
│   └── Output/
│       ├── ModOutputWriter.cs
│       ├── SpoilerLogWriter.cs
│       ├── SettingsFileWriter.cs
│       ├── ModDescriptionWriter.cs
│       ├── ModMemoriaIniWriter.cs
│       └── MemoriaLoadOrder.cs
│
├── StiltzkinsBag.App/                # WPF UI — depends on Core only
│   ├── ViewModels/
│   │   └── MainViewModel.cs
│   ├── Views/
│   │   ├── MainWindow.xaml           # ✅ Shell window
│   │   └── DebugWindow.xaml
│   ├── Resources/
│   │   ├── Themes/
│   │   │   └── FF9Palette.xaml       # ✅ Stiltzkin color tokens
│   │   ├── Sprites/
│   │   └── Fonts/
│   └── App.xaml                      # ✅ MaterialDesign bootstrap
│
└── StiltzkinsBag.Tests/              # xUnit — tests Core only
    └── SeedEngineTests.cs            # ✅ 7 tests passing
```

### Mod Folder CSV Structure

All CSV output goes under `[SeedFolderName]/StreamingAssets/Data/` using the following subfolder layout:

```
StreamingAssets/Data/
├── Battle/
│   ├── Actions.csv
│   ├── MagicSwordSets.csv
│   ├── StatusData.csv
│   └── StatusSets.csv
├── Characters/
│   ├── BaseStats.csv
│   ├── BattleParameters.csv
│   ├── CharacterParameters.csv
│   ├── Commands.csv
│   ├── CommandSets.csv
│   ├── DefaultEquipment.csv
│   ├── Leveling.csv
│   └── Abilities/
│       ├── AbilityFeatures.txt
│       ├── AbilityGems.csv
│       ├── Amarant.csv
│       ├── Beatrix1.csv
│       ├── Beatrix2.csv
│       ├── Blank1.csv
│       ├── Blank2.csv
│       ├── Cinna1.csv
│       ├── Cinna2.csv
│       ├── Eiko.csv
│       ├── Freya.csv
│       ├── Garnet.csv
│       ├── Marcus1.csv
│       ├── Marcus2.csv
│       ├── Quina.csv
│       ├── Steiner.csv
│       ├── Vivi.csv
│       └── Zidane.csv
└── Items/
    ├── Armors.csv
    ├── InitialItems.csv
    ├── ItemEffects.csv
    ├── Items.csv
    ├── MixItems.csv
    ├── ShopItems.csv
    ├── Stats.csv
    ├── Synthesis.csv
    └── Weapons.csv
```

Only files actually modified by the active randomizer settings are written to the mod folder. Files not touched by any enabled randomizer are omitted — the overlay system falls through to the base game version.

### Service / Dependency Architecture

The `RandomizerEngine` class (in Core) accepts a `Settings` object and a game path string. It orchestrates the pipeline in deterministic order, passing the single `Random` instance and the `ItemRemapTable` downstream. All randomizers are pure functions: given the same inputs they produce the same outputs. The WPF `MainViewModel` constructs `Settings` from UI state and calls `RandomizerEngine.RunAsync()` on a background thread, reporting progress via `IProgress<string>`.

### Config / Settings Structure

```csharp
public class Settings
{
    // Core
    public string SeedString { get; set; }
    public int SeedInt { get; set; }          // resolved from SeedString
    public string GamePath { get; set; }
    public RandomizerMode Mode { get; set; }  // Recommended, Chaos

    // Character
    public bool RandomizeCharacters { get; set; }
    public bool RandomizeBaseStats { get; set; }
    public bool RandomizeSpeciality { get; set; }
    public bool RandomizeAbilities { get; set; }
    public bool RandomizeEquipment { get; set; }
    public EquipmentMode EquipmentMode { get; set; } // Random, ShareAll, Stock
    public bool RandomizeStartingItems { get; set; }

    // Items
    public bool RandomizeTreasureChests { get; set; }
    public bool RandomizeShops { get; set; }
    public bool RandomizeSynthesis { get; set; }
    public bool RandomizeBonusSets { get; set; }
    public bool ShopIncludeMedicItems { get; set; }
    public bool ShopOverrideMedicShops { get; set; }
    public bool RandomizeGearStatBonuses { get; set; }

    // Enemies
    public bool RandomizeEnemies { get; set; }
    public bool RandomizeItemDrops { get; set; }
    public bool RandomizeItemSteals { get; set; }
    public bool RandomizeBlueMagic { get; set; }
    public bool RandomizeCardDrops { get; set; }

    // Tetramaster (bytecode — NOT TripleTriad CSV)
    public bool RandomizeTetraMaster { get; set; }
    public bool RandomizeCardStats { get; set; }
    public bool RandomizeCardOrder { get; set; }
    public bool RandomizeDecks { get; set; }
}
```

### Mod Folder Output Structure

```
[FF9 Root]/
├── Memoria.ini                          ← base game file; SB seed folder injected FIRST in FolderNames
└── StiltzkinsBag-Seed-[int]/
    ├── ModDescription.xml
    ├── Memoria.ini                      ← mod-local overrides
    ├── SpoilerLog/
    │   └── SpoilerLog-Seed-[int].txt
    ├── Settings-Seed-[int].json
    └── StreamingAssets/
        └── Data/
            ├── Battle/
            ├── Characters/
            └── Items/
```

**Memoria.ini FolderNames management:** On generate, read base `Memoria.ini`, prepend new seed folder name so it loads first, remove old SB seed entries, preserve all non-SB entries exactly. "Remove from load order" strips all `StiltzkinsBag-Seed-*` entries without deleting folders.

---

## Testing & Validation Strategy

### Validation Stages

| Stage | What | Purpose |
|---|---|---|
| Unit — Seed engine | Same seed string always produces same int; same int always produces same RNG sequence | Core correctness guarantee |
| Unit — CSV round-trip | Read → write → diff each CSV type against original | Verify parser doesn't corrupt data |
| Unit — Binary round-trip | Read binary → rewrite unchanged → binary diff = 0 | Verify HW-ported parsers don't corrupt data |
| Unit — Randomizer determinism | Run each randomizer twice with same seed+settings → identical output | Seed reproducibility per module |
| Integration — Full pipeline | Run full generation twice with same seed+settings → identical mod folder (byte-level) | End-to-end seed reproducibility |
| Integration — Remap propagation | Enable item shuffle, verify all binary patches reference remapped IDs | No stale item references |
| Manual — Recommended mode | Generate, load in game, verify: no legendary items in shops, characters have coherent ability/equipment sets, game is completable | Recommended Logic Engine correctness |
| Manual — Chaos mode | Generate, load in game, verify game doesn't crash on load | Minimum sanity |

### Pass Criteria — Integration

| Metric | Required |
|---|---|
| Same seed + settings → identical output | 100% — zero tolerance |
| Binary round-trip diff | Zero bytes changed |
| CSV round-trip diff | Zero bytes changed |
| Recommended mode: legendary items in shops | 0 |
| Recommended mode: key items obtainable | 100% of protected list reachable |
| Game loads without crash | All modes |

---

## Build Roadmap

### Phase 1 — Foundation ✅ COMPLETE
See `PhaseEnd_Phase1.md`

### Phase 2 — CSV I/O Layer (Est. 2–3 sessions)
**Goal:** Every Memoria CSV file has a typed C# model and clean round-trip read/write. No randomization yet.

- [ ] Create typed model classes for every CSV (see Component Inventory / Project Structure)
- [ ] Implement `CsvParser<T>` using CsvHelper — handles Memoria's comment header format (`#` lines, semicolon delimited)
- [ ] Read + write each CSV type; verify round-trip produces byte-identical output
- [ ] Implement `GamePathLocator` — registry auto-detect (port from old `path_search`; check 3 registry keys)
- [ ] Expose game path in UI; auto-detect on launch
- [ ] Unit tests: round-trip each CSV type → diff == empty
- [ ] **Milestone:** All CSVs round-trip cleanly; game path auto-detected and displayed in UI

### Phase 3 — Structured Binary Data Layer (Est. 3–4 sessions)
**Goal:** Port the relevant Hades Workshop C++ parsers to C#. Extract, parse, edit, and re-pack individual enemy and field files from Unity archives without raw byte matching.

- [ ] Study `UnityArchiver.cpp/h` — port unity archive extraction to `UnityArchiver.cs`
- [ ] Study `Enemies.cpp/h` + `BattleScenes.cpp/h` — port enemy data structures to `EnemyParser.cs`
- [ ] Study `MIPS.cpp/h` — port MIPS opcode parsing to `MipsEditor.cs`
- [ ] Study `Fields.cpp/h` — port field script structure to `FieldParser.cs`
- [ ] Implement binary round-trip: extract → parse → rewrite unchanged → re-pack → binary diff == 0
- [ ] Validate new parser against existing byte-string match research
- [ ] Unit tests: binary round-trip for 5+ representative enemy files and 5+ field files
- [ ] **Milestone:** Can extract an enemy file, parse its drop/steal/blue magic data into typed structs, modify a value, re-pack, and produce a valid mod output file.

> **Scope creep warning:** Only parse structs needed for Phase 4 randomizers. Mapping all field exit/entrance data belongs in Gen2.

### Phase 4 — Item Randomization + Enemy/Chest Bytecode (Est. 2–3 sessions)
**Goal:** Item remap table and all bytecode-dependent randomizers.

- [ ] Implement `ItemRemapper` — optionally shuffles item IDs, produces `ItemRemapTable`
- [ ] Implement `EnemyRandomizer` — randomize drops, steals, blue magic, card drops; apply `ItemRemapTable`
- [ ] Implement `ChestRandomizer` — randomize chest contents in field files; apply `ItemRemapTable`; respect uniqueness constraints
- [ ] Integration tests: generate twice with same seed → identical patched binary files
- [ ] Verify: unique/legendary items appear at most once across all chests and enemy drops
- [ ] **Milestone:** Full generation run produces deterministic patched enemy and field binary files.

> **Scope creep warning:** Enemy stat randomization (HP, MP, attack values) is Gen2.

### Phase 5 — CSV Randomizers (Est. 3–4 sessions)
**Goal:** All CSV-based randomizers implemented. Each runs independently and deterministically.

- [ ] Implement `CharacterRandomizer` — full pipeline: base stats → speciality → abilities → equipment → starting items
- [ ] Implement `InitialItemsRandomizer` — randomize starting items from consumable pool (items 236–253)
- [ ] Implement `GearStatRandomizer` — randomize `BonusId`; stay within ±20% of base game average; hard cap +3 per stat
- [ ] Implement `ShopRandomizer` — maintain shop item count; medic item option; override medic shops option
- [ ] Implement `SynthesisRandomizer` — randomize prices, results, ingredients; add Bonus Sets
- [ ] Implement `AbilityGemsRandomizer` — randomize AP costs
- [ ] Implement `TetraMasterRandomizer` — randomize card stats and NPC deck contents via bytecode only
- [ ] Integration tests: full pipeline twice with same seed → identical CSV outputs
- [ ] **Milestone:** Full generation produces a complete, playable randomized mod.

> **Scope creep warning:** Recommended Logic Engine is Phase 6. Do NOT add constraint validation to individual randomizers here.

### Phase 6 — Recommended Logic Engine (Est. 3–4 sessions)
**Goal:** Constraint system that makes Recommended mode coherent and completable.

- [ ] Define `KeyItemProtectionList` — hardcoded list of plot-required items
- [ ] Define `LegendaryItemList` — hardcoded list of unique/legendary items with rarity rules
- [ ] Implement key item validation: verify all protected items appear exactly once; re-roll placement if missing
- [ ] Implement legendary rarity enforcement: no legendary items in shops; max 1 per chest pool
- [ ] Implement character coherence validation: equipment carries abilities compatible with randomized speciality
- [ ] Implement synthesis reachability validation
- [ ] Implement gear bonus budget validation
- [ ] Implement speciality-aware equipment bias
- [ ] Integration test: Recommended mode full run → no legendary items in shops; all key items reachable
- [ ] **Milestone:** Recommended mode passes all constraint checks. Game is completable on a Recommended seed.

### Phase 7 — WPF UI (Est. 2–3 sessions)
**Goal:** Full UI bound to settings model. All features exposed. Custom FF9 theme.

- [ ] Implement `MainViewModel` with full property binding to `Settings`
- [ ] Build `MainWindow.xaml` — all feature checkboxes, seed field, path field, generate button
- [ ] Implement Stiltzkin sprite animation (sprite sheet → `DispatcherTimer` frame swap)
- [ ] Implement Recommended / Chaos mode toggle with mutual exclusion
- [ ] Implement feature group enable/disable
- [ ] Implement seed field — manual entry + Random Seed button
- [ ] Implement game path — auto-detect button + manual browse button
- [ ] Implement progress feedback — progress bar + status text via `IProgress<string>`
- [ ] Implement shareable settings string — encode/decode seed + settings; copy-to-clipboard button
- [ ] Implement named presets — Recommended, Chaos, Casual, Competitive
- [ ] Apply full custom FF9 theme (Stiltzkin blue palette, custom window chrome, FFIX-style typography)
- [ ] **Milestone:** UI fully functional and themed. Settings string roundtrips correctly.

### Phase 8 — Mod Output + Memoria Integration (Est. 1–2 sessions)
**Goal:** Correct mod folder structure, Memoria.ini management, spoiler log, settings file, mod metadata.

- [ ] Implement `ModOutputWriter` — creates seed folder, writes all CSV and binary files
- [ ] Implement `MemoriaLoadOrder` — parse/write `FolderNames` in base `Memoria.ini`
- [ ] Implement `ModDescriptionWriter` — write `ModDescription.xml`
- [ ] Implement `ModMemoriaIniWriter` — write mod-local `Memoria.ini`; force `TripleTriad = 0` if card randomizer active
- [ ] Implement `SpoilerLogWriter` — document every change
- [ ] Implement `SettingsFileWriter` — write `Settings-Seed-[int].json`
- [ ] Implement "Remove Stiltzkin's Bag from Load Order"
- [ ] Unit tests: output folder structure correct; `FolderNames` update correctly
- [ ] **Milestone:** Generated mod loads in game via Memoria. Spoiler log accurately reflects all changes.

### Phase 9 — Polish, Debug Tooling, Edge Cases (Est. 1–2 sessions)
**Goal:** Ship-ready quality.

- [ ] Port debug window to WPF (`DebugWindow.xaml`) — gated behind dev flag
- [ ] Add structured error messages for common failure cases
- [ ] Add logging for debug builds
- [ ] Handle edge cases: missing CSVs, read-only game directory, partial mod folder
- [ ] Verify all randomizers handle boundary values without exceptions
- [ ] Final seed determinism regression test: 10 seeds × all feature combinations
- [ ] **Milestone:** App handles all common error states gracefully. Ready for first public release.

---

## Enhancement Backlog

| Priority | Enhancement | Effort | Phase |
|---|---|---|---|
| High | Spoiler log | Low | Phase 8 |
| High | Shareable settings string | Low | Phase 7 |
| High | Key item protection | Medium | Phase 6 |
| Medium | Named presets (Recommended/Chaos/Casual/Competitive) | Low | Phase 7 |
| Medium | Tetramaster card stats + NPC deck randomization (bytecode) | Medium | Phase 5 |
| Medium | Gear stat bonus randomization | Medium | Phase 5 |
| Medium | InitialItems randomization | Low | Phase 5 |
| Low | Stiltzkin walking animation on loading | Medium | Phase 7 |
| Low | Race/blind mode (settings hidden until password) | Medium | Gen3 |
| Low | Story cutscene item acquisition randomization | High | Gen3 |

---

## Future Generations

### Gen2 — Runtime DLL + Field Entrance Randomizer

> **Rule:** Do not start until Gen1 is publicly released and stable (no critical bugs reported for 30 days).

**Runtime Companion DLL (BepInEx + HarmonyX)**
- Intercept runtime data calls for features that benefit from live patching
- Enemy stat randomization (HP, MP, attack, defense)
- Potential: persistent seed state across save/load

**Field Entrance Randomizer**
- `CityExits` list (79 fields with world map exits) — complete in prior research
- `OverWorldConnectors` — complete
- `ProgressionComplete` (field ID → story counter requirements) — partially complete
- Requires maze solver to guarantee game completability
- Requires story progression variable injection via bytecode

**Architecture additions:**
- `FieldGraphBuilder` — traversable graph from field connection data
- `MazeSolver` — validates randomized entrance configuration is completable
- `ProgressionInjector` — injects story counter checks into field bytecode

### Gen3 — Enemy Encounters + Advanced Features

> **Rule:** Do not start until Gen2 field entrance randomizer is complete and validated.

- Enemy encounter randomization
- Level-scaling mod integration
- Story cutscene item acquisition randomization
- Race/blind mode
- Cross-platform consideration (Avalonia if warranted)

---

## What Success Looks Like

| Milestone | Measure |
|---|---|
| Gen1 Phase 8 complete | Mod generates, loads in Memoria, game plays from start to finish on a Recommended seed |
| Seed determinism verified | 20 test seeds × full feature set → byte-identical outputs across two separate machines |
| Community adoption | First public race event using Stiltzkin's Bag seeds |
| Gen2 field randomizer | All field types correctly classified; maze solver confirms completability before output |

---

## Feature & Architecture Inventory

- Seed-based deterministic randomization — every seed+settings combo always produces the same output
- Shareable settings string — paste a code to reproduce any exact run configuration
- Recommended mode — constraint-enforced coherent randomization
- Chaos mode — unconstrained maximum randomness
- Full character pipeline — stats → speciality → abilities → equipment in dependency order
- Gear stat bonus randomization — balance-constrained
- Enemy bytecode editing via ported Hades Workshop parsers — no raw byte matching
- Individual file mod output — surgical mod overlay, small output size
- Spoiler log — full documentation of every change for race routing
- Tetramaster card randomization via bytecode only — TripleTriad.csv (FF8 mode) explicitly not supported
- Registry-based game path auto-detection
- WPF + MaterialDesignInXaml — custom FF9 Stiltzkin theme, sprite animations

---

## Data Sources / External Dependencies

| Source | Provides | Notes |
|---|---|---|
| Memoria Engine CSVs (StreamingAssets\Data) | All editable game data | Read from user's installed game at generation time |
| p0data2.bin / p0data7.bin (via Unity archives) | Field scripts, enemy bytecode | Extracted at generation time using ported UnityArchiver |
| Hades Workshop source (C++) | Parser reference for UnityArchiver, Enemies, Fields, MIPS | GNU GPL; personal permission from author; port to C# |
| Windows Registry | FFIX install path auto-detection | 3 registry key locations checked |

---

## Libraries / Dependencies

| Purpose | Package | Notes |
|---|---|---|
| CSV parsing | CsvHelper | Core project |
| WPF theming | MaterialDesignThemes + MaterialDesignColors | App project |
| Unit testing | xUnit | Tests project — Core only |
| JSON serialization | System.Text.Json | Built-in .NET 8 |
| Binary parsing | None — pure C# | Ported from Hades Workshop C++ source |

---

## AI Collaboration Rules

**Rule: Extended Thinking Mode.**
If the next task is very complex, stop the chat and suggest the user enable Extended Thinking Mode for this next message only. Proceed to use Extended thinking on the complex task. After your thinking and output, remind the user to disable Extended Thinking. use yellow triangle emojis to alert them to this message

**Rule: Explain before coding.**
Before writing any class, module, or function, explain the design approach first: what the component needs to do, any non-obvious decisions, and why this solution over alternatives. Only write code after that reasoning is stated.

**Rule: Everything intended for use outside chat goes in a file.**
Code, PhaseEnd markdown, documentation — if it is meant to be saved or committed, it is created using the file creation tool and never pasted inline in chat. The chat contains reasoning, explanations, and instructions only.

**Rule: Never overwrite blind.**
Before overwriting any existing file, ask the developer to paste the current version. Use **bold TODO:** to indicate an action required from the developer.

**Rule: Preserve comments and documentation.**
When rewriting an existing file, preserve all existing comments and doc-style headers. Never silently drop comments.

**Rule: Document disabled logic.**
```
// DISABLED: [name] — [date or phase]
// Original intent: [what it was supposed to do]
// Why disabled: [specific evidence]
// Re-enable if: [specific observable condition]
```

**Rule: One task at a time — with judgment.**
Work one meaningful task at a time and wait for confirmation before moving to the next. Exception: small mechanical setup steps (creating projects, installing NuGet packages, creating 2–3 related files) may be grouped into a short checklist of 2–4 items. Dumping an entire phase at once is never acceptable.

**Rule: Verify every checkbox before closing a phase.**
Before marking any phase complete, explicitly verify every checkbox — including wiring steps like registrations, config bindings, and integration tasks.

**Rule: Confirm milestone before PhaseEnd.**
The phase milestone must be explicitly confirmed by the developer before the PhaseEnd file is created.

**Rule: PhaseEnd output is always a file.**
PhaseEnd content is always output as `PhaseEnd_Phase[N].md` using the file creation tool. Never dumped in chat. Ends with a 🛑 stop sign instructing the developer to close this session, add the file to the Claude Project, and start a new session with all context files attached. Remind the developer that they do not need to delete this chat, and its recommmended to keep it for posterity and back refference. Old chats do not affect speed/token usage.

**Rule: The main context file is permanent and static.**
This file is never edited, rewritten, or replaced. Current version and phase status live in the most recent PhaseEnd file. The AI reconstructs project state by reading this file plus all PhaseEnd files at session start.

**Rule: Attach all PhaseEnd files at session start.**
All PhaseEnd files are kept for the life of the project. At session start, attach this file and every PhaseEnd file produced so far.

**Rule: Service registration placement.**
When instructing changes to entry points or startup files, always indicate exactly where in the existing code the new lines belong — using surrounding lines as anchors.

**Rule: Single Random instance — never create new Random in randomizers.**
All randomizers receive the seeded `Random` instance from `SeedEngine`. Never call `new Random()` or `new Random(anySeed)` inside any randomizer. This is the single most important architectural rule in this codebase.

**Rule: RNG call order is sacred.**
The order in which the shared `Random` instance is called must be strictly deterministic. If a feature is disabled, skip its RNG calls entirely — do not consume and discard. Feature flags gate entire RNG sequences, not individual calls.

**Rule: ItemRemapTable before bytecode.**
If item shuffle is enabled, `ItemRemapper` must complete and produce `ItemRemapTable` before any binary editor writes output. Never write binary patches with stale item IDs.

**Rule: Pipeline order is enforced.**
The randomization pipeline must always execute in the order defined in Core Logic. CharacterRandomizer sub-steps must run in order: stats → speciality → abilities → equipment → starting items.

**Rule: Hades Workshop port — port logic, not GUI.**
Port only data parsing and manipulation logic. Never port GUI, rendering, or platform-specific code. Ported classes must be pure C# with no external dependencies beyond the standard library.

**Rule: CSV round-trip is sacred.**
Any change to `CsvParser<T>` must be followed immediately by re-running all CSV round-trip tests. A CSV parser that corrupts header comment lines, trailing semicolons, or column order is worse than no parser.

---

## Parking Lot

| Idea | Potential Generation |
|---|---|
| TripleTriad.csv support (FF8 card game mode) | Out of scope — separate optional Memoria mod |
| Field entrance randomizer — full maze solver + bytecode injection | Gen2 |
| Enemy stat randomization (HP, MP, attack, defense) | Gen2 |
| Enemy encounter randomization | Gen3 |
| Level-scaling mod integration | Gen3 |
| Story cutscene item acquisition randomization | Gen3 |
| Race/blind mode | Gen3 |
| Cross-platform port (Avalonia) | Gen3 if warranted |
| Boss-specific stat scaling | Gen3 |
| `BattleParameters.csv` randomization | Out of scope — high crash risk |
| `MagicSwordSets.csv` randomization | Gen2 if feasible |
| Runtime companion DLL (BepInEx + HarmonyX) | Gen2 |
| Persistent seed state across save/load | Gen2 DLL |

---

## Notes for Future Phases

- Phase 8: Review `MergeScripts` behavior in Memoria.ini before implementing `ModMemoriaIniWriter`

---

## Quick Reference Card

```
Project:     Stiltzkin's Bag — FFIX PC Randomizer
Stack:       C# .NET 8, WPF + MaterialDesignInXaml, xUnit
Namespaces:  Core → StiltzkinsBag | App → StiltzkinsBag.App | Tests → StiltzkinsBag.Tests
Output:      Memoria Engine mod-folder overlay (CSVs + patched binary files)
             Folder: [FF9Root]/StiltzkinsBag-Seed-[int]/StreamingAssets/Data/
Mem.ini:     Seed folder injected FIRST in FolderNames; old SB entries removed; non-SB untouched
Cards:       Tetramaster randomized via bytecode ONLY. TripleTriad.csv NOT supported.
Generation:  Gen1 — mod-folder randomizer
Philosophy:  Every seed completable, every run surprising, every output deterministic
Key rules:   Single Random instance. RNG call order is sacred. ItemRemap before bytecode.
Session:     Attach this file + ALL PhaseEnd files. AI reads all of them to reconstruct state.
```
