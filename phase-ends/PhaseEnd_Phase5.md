# PhaseEnd — Phase 5: CharacterRandomizer
**Date:** 2026-03-25
**Project Version:** 1.4.0 → 1.5.0
**Phase Status:** ✅ Complete

---

## Checklist Verification

| Item | Status | Notes |
|---|---|---|
| Settings.cs audit | ✅ | Added `RandomizeAbilityGems` |
| `CharacterRandomizerResult.cs` | ✅ | Record with BaseStats, CharacterParameters, CommandSetRows, SlotAssignment, AbilityTables, AbilityFeaturesText |
| CharacterRandomizer — Sub-step 1: Stats | ✅ | Column shuffle (Recommended) + ranged random (Chaos) + Zidane Dex bias |
| CharacterRandomizer — Sub-step 2: Speciality | ✅ | Full Option B slot assignment — stat-biased, random, and Chaos paths |
| CharacterRandomizer — Sub-step 3: Abilities | ✅ | AA packages, summon/white magic pool splits, T1/T2 pre-seeding, SA pool shuffle + round-robin |
| CharacterRandomizer — Sub-step 4: Equipment | ✅ | DefaultEquipmentSet Fisher-Yates shuffle |
| `CharacterRandomizerTests.cs` | ✅ | 41 new tests; 317/317 total passing |
| AbilityFeatures.txt patch | ✅ | CMD 31 HardDisable rewritten to reference Blk Mag slot holder |
| **Milestone: CharacterRandomizer complete and deterministic** | ✅ | All modes, all sub-steps, all seeds |

---

## Build Log

**Files created or replaced this phase:**

- `StiltzkinsBag.Core/Models/CharacterRandomizerResult.cs` — Immutable result record.
  Six output fields: `BaseStats`, `CharacterParameters`, `CommandSetRows`, `SlotAssignment`,
  `AbilityTables`, `AbilityFeaturesText`. `SlotAssignment` maps char ID → list of slot type
  name strings (e.g. `"Steal"`, `"Summon-A"`). `CommandSetRows` contains 8 rewritten rows
  (IDs 0–7) for `CommandSets.csv`.

- `StiltzkinsBag.Core/Randomizers/CharacterRandomizer.cs` — Full character pipeline.
  Constructor takes: `Random`, `Settings`, `BaseStatsRow` list, `CharacterParametersRow` list,
  `CommandSetsRow` list, `Dictionary<int, List<CharacterAbilityRow>>`, `string abilityFeaturesText`.

  **Sub-step 1 — Base Stats:**
  - Recommended: Fisher-Yates column shuffle (5 × 11 RNG calls). Zidane Dex bias applied
    post-shuffle (0 RNG — deterministic swap with highest-Dex main char).
  - Chaos: uniform random in vanilla [min, max] range per stat per character (5 × 12 RNG calls).
  - All 12 characters (IDs 0–11) participate.

  **Sub-step 2 — Speciality (Option B):**
  - Three assignment paths:
    - Recommended + RandomizeBaseStats=true → stat-biased greedy assignment, 0 RNG calls
    - Recommended + RandomizeBaseStats=false → random Fisher-Yates, 11 RNG calls
    - Chaos → random Fisher-Yates, 11 RNG calls
  - Zidane (char 0) always locked: R1=Steal, R2=Skill. Never randomized.
  - Locked pairs: Blue Mage (Eat+Blu Mag), Knight (Swd Art+Swd Mag) assigned atomically.
  - Always: exactly 1 Blue Mage, 1 Knight, 2 Summoners, 2 White Mages across chars 1–7.
  - Free pool (Blk Mag, Focus, Jump, Dragon, Flair, Throw) fills remaining 7 slots.
  - Focus constraint (Recommended mode): ensures Focus holder has a magic-type slot.
    At most 1 additional RNG call.
  - Stat-bias scoring: 12 distinct formulas per slot type, character-archetype-inspired
    (e.g. Blk Mag = Magic×2 − Dex, Dragon = Will×2 − Dex, Flair = Strength + Will×2 − Magic).
  - Produces `SlotAssignment` dict and 8 rewritten `CommandSetsRow` objects.
  - Patches `AbilityFeatures.txt` CMD 31 HardDisable to reference Blk Mag slot holder.

  **Sub-step 3 — Abilities:**
  - AA packages: fixed per slot type from static data tables. Zidane's AA:101–108 always his.
  - Summon pool split: Fisher-Yates shuffle of 12 summons (11 RNG calls). Summon-A gets
    indices 0–5, Summon-B gets 6–11. Tracks which character received AA:58 Odin.
  - White magic pool split: Fisher-Yates shuffle of 24 merged spells (23 RNG calls).
    Wht Mag-A gets 0–11, Wht Mag-B gets 12–23. No spell shared between white mages.
  - SA pre-seeding T1 (all modes): 12 class-guaranteed SAs removed from pool and assigned.
    Special cases: SA:47 Guardian Mog = random pick between summoners (1 RNG call);
    SA:60 Odin's Sword = always assigned to the Odin-holder from summon split.
  - SA pre-seeding T2 (Recommended only): magic SAs distributed first-come-first-served
    by char ID ascending; SA:33 Concentrate uses summoner priority. Physical SAs assigned
    to slot-type holders. SA:38 Protect Girls skipped for female chars (DefaultCategory=6).
  - SA pool shuffle: Fisher-Yates on remaining pool (~95–110 entries, N−1 RNG calls).
  - Round-robin SA assignment with duplicate-skip and gender filter. Every character
    eventually receives all universal SAs (8 copies each).

  **Sub-step 4 — Equipment:**
  - Fisher-Yates shuffle of `DefaultEquipmentSet` values for chars 0–7 (7 RNG calls).
  - Guest characters (IDs 8–15) never touched.

- `StiltzkinsBag.Tests/CharacterRandomizerTests.cs` — 41 tests covering all sub-steps,
  all modes, determinism, Zidane locks, Focus constraint, Odin's Sword tracking,
  summon/white magic pool splits, SA uniqueness per character, gender filter.

**Document produced this phase (add to Claude Project):**
- `AbilityTierClassification_Rev4.md` — implementation contract for Sub-steps 2 and 3.
  Contains command slot registry, stat-bias score formulas, AA packages per slot type,
  summon and white magic pool definitions, SA T1/T2/T3 tier tables.

---

## Test Count

| File | Tests | Notes |
|---|---|---|
| SeedEngineTests | 7 | Phase 1 |
| CsvRoundTripTests | 44 | Phase 2 |
| EnemyFileTests | 22 | Phase 3 |
| UnityArchiverTests | 11 | Phase 3 |
| EnemyCatalogAndResolverTests | 10 | Phase 3 |
| Misc (Phase 1) | 4 | Phase 1 |
| FieldParserTests | 53 | Phase 4 |
| ItemRemapperTests | 58 | Phase 4 |
| EnemyRandomizerTests | 39 | Phase 4 |
| FieldItemRandomizerTests | 29 | Phase 4 |
| CharacterRandomizerTests | 41 | Phase 5 |
| **Total** | **317** | **All passing** |

---

## Deviations

| Item | Plan | Actual | Reason |
|---|---|---|---|
| Phase 5 scope | All CSV randomizers | CharacterRandomizer only | CharacterRandomizer expanded into ~8 sub-tasks. Remaining randomizers deferred to Phase 5.5 |
| Phase numbering | Phase 6 = Recommended Logic Engine | Phase 5.5 added; no existing phase numbers shifted | Developer preference — no renumbering |
| CharacterRandomizer — Speciality | Whole-set Fisher-Yates shuffle of DefaultCommandSet | Full Option B slot-level rewrite of CommandSets.csv rows | Original plan was architecturally wrong; needed full redesign to support per-slot assignment |
| CharacterRandomizer — Abilities | "Shuffle ability pool entries" | Complete AA package system + summon/white magic pool splits + SA tier pre-seeding + round-robin pool assignment | Scope revealed during design — all of this is required for a coherent playable result |
| AbilityFeatures.txt patch trigger | "Black Mage command set holder" | "Blk Mag slot holder" | Command sets no longer have IDs that map to archetypes; slot type string is the correct identifier |
| CharacterParameters.DefaultCommandSet | Was to be shuffled | Never modified | Option B writes new CommandSets.csv rows directly; DefaultCommandSet always equals char ID |
| `SeedEngine.StringToInt` | Assumed method name | Actual name is `SeedEngine.Resolve` | Confirmed from source code |

---

## Rules Added This Phase

| Rule | Reason |
|---|---|
| Zidane's Steal slot is always locked — never randomized | Story-mandatory boss fights require Steal; items are unobtainable otherwise |
| Zidane's Skill slot is always locked — never randomized | Skill carries Flee — must always be available to the player as a strategic escape |
| Command slot assignment writes new CommandSets.csv rows (Option B) | DefaultCommandSet in CharacterParameters always equals char ID; slot assignment is expressed entirely through rewritten CommandSets rows |
| AbilityTierClassification_Rev4.md is the implementation contract | All T1/T2/T3 SA decisions, slot-to-AA mappings, and formula tables are defined there; do not derive them from memory |
| SA pre-seeding removes consumed entries from pool | TryPreSeedFromPool removes the first matching pool entry; subsequent requests for the same AbilityRef consume separate copies or fail gracefully |
| SA:38 Protect Girls gender filter applies in all modes | Runtime effect is broken/useless on female characters regardless of mode; this is a correctness filter, not a Recommended-only constraint |

---

## Parking Lot Additions This Phase

| Topic | Target | Notes |
|---|---|---|
| Garnet story-restricted summons | Phase 6 | Characters who receive Summon have no story restriction unless their char ID is Garnet. The restriction follows char ID, not class. A non-Garnet summoner can use all summons freely from game start. |
| Summon-A vs Summon-B command IDs (16 vs 20) | Phase 6 | Both command IDs access the same eidolon pool but Summon-B (ID 20) is the stronger multi-hit variant. Verify in-game whether the distinction matters for randomized characters before Phase 6. |
| Option B — Recommended Logic Engine validation | Phase 6 | Phase 6 should validate that the Blk Mag slot holder has a coherent ability set, that the Focus holder actually benefits from it, and that equipment sets are appropriate for the assigned class. |
| AP cost interaction between AbilityGemsRandomizer and CharacterRandomizer | Phase 5.5 | AbilityGemsRandomizer (Phase 5.5) randomizes AP costs. CharacterRandomizer assigns abilities with vanilla AP values from the source character files. The interaction order matters: AbilityGemsRandomizer should run after CharacterRandomizer so AP overrides apply to the final ability tables. |

---

## Commit Message

```
feat: Phase 5 complete — CharacterRandomizer full pipeline

- CharacterRandomizerResult: 6-field record (BaseStats, CharacterParameters,
  CommandSetRows, SlotAssignment, AbilityTables, AbilityFeaturesText)
- CharacterRandomizer: 4 sub-steps, all modes, fully deterministic

Sub-step 1 — Base Stats:
  - Recommended: Fisher-Yates column shuffle, Zidane Dex bias (0 RNG)
  - Chaos: uniform random in vanilla [min,max] range per stat
  - All 12 characters (guests included)

Sub-step 2 — Speciality (Option B):
  - 3 assignment paths: stat-biased (0 RNG), random (11 RNG), chaos (11 RNG)
  - Zidane always locked: Steal R1 + Skill R2, never randomized
  - Locked pairs: Blue Mage (Eat+Blu Mag), Knight (Swd Art+Swd Mag)
  - Always: 1 Blue Mage, 1 Knight, 2 Summoners, 2 White Mages per run
  - Free pool (Blk Mag, Focus, Jump, Dragon, Flair, Throw) fills 7 remaining slots
  - Focus constraint: always on magic-type char in Recommended mode
  - 12 distinct stat-bias formulas (character-archetype-inspired)
  - CommandSets.csv rows 0-7 rewritten; DefaultCommandSet pointer never modified
  - AbilityFeatures.txt CMD 31 HardDisable patched to Blk Mag slot holder

Sub-step 3 — Abilities:
  - AA packages: fixed per slot type from static data tables
  - Summon pool split: 12 summons Fisher-Yates, 6 per summoner, Odin tracked
  - White magic pool split: 24 merged spells Fisher-Yates, 12 per white mage
  - T1 pre-seeds: 12 class-guaranteed SAs, Guardian Mog random pick (1 RNG),
    SA:60 Odin's Sword → Odin holder
  - T2 pre-seeds (Recommended): magic SAs by char ID order, summoner priority
    for SA:33 Concentrate; physical SAs to slot-type holders
  - SA pool: Fisher-Yates shuffle, round-robin with duplicate-skip
  - SA:38 Protect Girls: gender filter in all modes (DefaultCategory != 6)

Sub-step 4 — Equipment:
  - DefaultEquipmentSet Fisher-Yates shuffle for chars 0-7 (7 RNG calls)
  - Guest characters never modified

- CharacterRandomizerTests: 41 tests, 317/317 total passing
- AbilityTierClassification_Rev4.md: implementation contract added to project
- SeedEngine.Resolve confirmed as correct method name (not StringToInt)
- Rules: 6 new rules added
```

---

## PhaseEnd Changelog

```
v1.4.0 → v1.5.0
- Build Log: Phase 5 entry added
- Deviations: Phase 5 scope split, Option B redesign, DefaultCommandSet unchanged,
  SeedEngine.Resolve name, CharacterParameters not modified for commands
- Rules: 6 new rules added
- Parking Lot: Garnet restriction, Summon command IDs, Phase 6 validation,
  AP cost interaction with AbilityGemsRandomizer
- Phase 5 marked complete
- Current phase: 5.5 — Remaining CSV Randomizers
```

---

## 🛑 Stop Here

This chat session is complete. Do the following before starting Phase 5.5:

1. Add `PhaseEnd_Phase5.md` to your Claude Project
2. Add `AbilityTierClassification_Rev4.md` to your Claude Project
3. Start a new chat session
4. Say: "Continue building Stiltzkin's Bag. Read all attached markdown files."

Do not continue development in this session. Do not delete this chat — keep it for
posterity and back-reference. Large chats in a project will not slow down chat or use more tokens.
