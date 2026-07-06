# PhaseEnd — Phase 5.5: Remaining CSV Randomizers (Ability Costs + Gear Stats)
**Date:** 2026-03-26
**Project Version:** 1.5.0 → 1.5.5
**Phase Status:** ✅ Complete

---

## Checklist Verification

| Item | Status | Notes |
|---|---|---|
| Settings.cs audit | ✅ | 6 iterations total — StartingItemMode, AbilityGemMode, AbilityApMode, GearStatMode, GearStatWeighting enums + 18 new properties |
| `StatsRow.cs` + `StatsRowMap` | ✅ | New model — not in Phase 2 typed model set. 11 columns, UTF-8 |
| `InitialItemsRandomizer.cs` | ✅ | 5 modes: ConsumablesRandom, GearRandom, AbilityStarter, SpeedRunner, AllItems (debug-gated) |
| `InitialItemsRandomizerTests.cs` | ✅ | 29 tests passing |
| `AbilityGemsRandomizer.cs` | ✅ | 3 modes: Shuffle, BoundedRandom, AllCheap (debug-gated) |
| `AbilityGemsRandomizerTests.cs` | ✅ | 26 tests passing |
| `AbilityApRandomizer.cs` | ✅ | 4 modes: ProportionalScale, Inverse, FlatCost, Amnesia (debug-gated) |
| `AbilityApRandomizerTests.cs` | ✅ | 24 tests passing |
| GearStatRandomizer — analysis | ✅ | Vanilla distribution computed; geometric calibration; 2-tier constraint model approved |
| `GearStatRandomizer.cs` | ✅ | 3 modes × 4 weightings; AllStatsMaxed debug gate; epic roll system |
| `GearStatRandomizerTests.cs` | ✅ | 26 tests passing including statistical weighting assertions |
| `CsvRoundTripTests.cs` — Stats round-trip | ✅ | 2 tests added (byte-identical + Rebirth Ring known-value) |
| **Milestone: all Phase 5.5 CSV randomizers complete and deterministic** | ✅ | 424/424 tests passing |

---

## Build Log

**Files created or replaced this phase:**

- `StiltzkinsBag.Core/Models/Settings.cs` — 6 cumulative revisions. Final additions:
  - `StartingItemMode` enum: ConsumablesRandom, GearRandom, AbilityStarter, SpeedRunner, AllItems
  - `AbilityGemMode` enum: Shuffle, BoundedRandom, AllCheap
  - `AbilityApMode` enum: ProportionalScale, Inverse, FlatCost, Amnesia
  - `GearStatMode` enum: Proportional, Shuffle, Chaos
  - `GearStatWeighting` enum: Geometric, VanillaWeighted, Linear, Uniform
  - New properties: StartingItemMode, RandomizeStartingCounts, AbilityGemMode, AbilityGemMinCost,
    AbilityGemMaxCost, AbilityApMode, ApScaleMinPercent, ApScaleMaxPercent, ApFlatCost,
    GearStatMode, GearStatWeighting, GearStatMax, GearStatEpicMax, GearStatEpicChancePercent,
    GearStatHardCap, ZeroStatItemsCanGainStats, AllStatsMaxed
  - `IsDebugMode` flag added — gates AllItems, AllCheap, Amnesia, AllStatsMaxed

- `StiltzkinsBag.Core/Models/Csv/StatsRow.cs` — New model for Stats.csv.
  Columns: Comment, Id, Dexterity, Strength, Magic, Will (stat columns, randomizable),
  AttackElement, GuardElement, AbsorbElement, HalfElement, WeakElement (element flags, never modified).
  `HasAnyStat` convenience property used by GearStatRandomizer for row qualification.

- `StiltzkinsBag.Core/Randomizers/InitialItemsRandomizer.cs` — Starting item randomizer.
  - ConsumablesRandom: pool 236–253, Fisher-Yates shuffle, 17 RNG calls
  - GearRandom: consumables + obtainable gear (Price > 2 filter), pool.Count−1 RNG calls
  - AbilityStarter: consumables + gems 224–235, pool.Count−1 RNG calls
  - SpeedRunner: hardcoded [Potion×1, Phoenix Down×1], 0 RNG calls
  - AllItems: IDs 0–254, count=1 each, 0 RNG calls, debug-gated
  - RandomizeStartingCounts: [1, 7] range, 1 RNG call per slot; ignored by SpeedRunner/AllItems
  - GearRandom falls back to ConsumablesRandom gracefully when allItems=null
  - AllItems downgrades to ConsumablesRandom when IsDebugMode=false

- `StiltzkinsBag.Core/Randomizers/AbilityGemsRandomizer.cs` — Gem equip cost randomizer.
  - Shuffle: Fisher-Yates of vanilla costs, total budget preserved, 63 RNG calls
  - BoundedRandom: independent draw per ability in [min, max], 64 RNG calls
  - AllCheap: all costs = 1, debug-gated, downgrades to Shuffle, 0 RNG calls
  - Only Gems column modified; Id, Comment, BoostedVersions always preserved

- `StiltzkinsBag.Core/Randomizers/AbilityApRandomizer.cs` — AP cost randomizer.
  Post-processes CharacterRandomizerResult.AbilityTables (must run after CharacterRandomizer).
  Per-ability scaling: same AbilityRef → same AP on all characters. AbilityRefs sorted
  alphabetically before RNG calls for deterministic order.
  - ProportionalScale: scale% per unique ref in [min%, max%], 1 RNG call per unique ref
  - Inverse: cost = vanillaMin + vanillaMax − vanillaCost, 0 RNG calls
  - FlatCost: all costs = ApFlatCost (clamped to 1), 0 RNG calls
  - Amnesia: all costs = 1, debug-gated, downgrades to ProportionalScale, 0 RNG calls
  - Input never mutated; new dictionary always returned

- `StiltzkinsBag.Core/Randomizers/GearStatRandomizer.cs` — Gear stat bonus randomizer.
  Operates on Stats.csv rows. Only Dexterity/Strength/Magic/Will modified; element columns preserved.
  Row eligibility: ID 0 skipped, gems 137–148 skipped, padding 157+ skipped,
  zero-stat rows skipped unless ZeroStatItemsCanGainStats=true. Rows processed in ID order.
  - Proportional: DrawStat(GearStatMax) per column + epic roll (epicChancePct% → one stat DrawStat(EpicMax)); all clamped to HardCap
  - Shuffle: Fisher-Yates of all stat values across qualifying rows; total budget preserved
  - Chaos: DrawStat(HardCap) per column; no sum constraint
  - AllStatsMaxed: all qualifying stats = HardCap, debug-gated
  - Weightings: Geometric (p=0.5), VanillaWeighted (p=0.35), Linear, Uniform
  - DrawGeometric uses integer weight table (scale=100,000) for precision without floats in RNG

**Key discovery — GearStat vanilla distribution (from Stats.csv analysis):**
- 144 qualifying rows (101 stat-bearing, 43 zero-stat)
- Individual stat value distribution across all slots: +0=75.3%, +1=15.8%, +2=6.9%, +3=1.4%, +4=0.3%, +5=0.2%
- Vanilla is approximately geometric with p=0.35 (VanillaWeighted preset)
- Geometric preset (p=0.5) produces ~2× more high-stat gear than vanilla — intentionally more exciting
- Save The Queen has vanilla total stat sum of 10 (outlier legendary)

---

## Test Count

| File | Tests | Notes |
|---|---|---|
| SeedEngineTests | 7 | Phase 1 |
| CsvRoundTripTests | 46 | Phase 2 + 2 Stats tests added this phase |
| EnemyFileTests | 22 | Phase 3 |
| UnityArchiverTests | 11 | Phase 3 |
| EnemyCatalogAndResolverTests | 10 | Phase 3 |
| Misc (Phase 1) | 4 | Phase 1 |
| FieldParserTests | 53 | Phase 4 |
| ItemRemapperTests | 58 | Phase 4 |
| EnemyRandomizerTests | 39 | Phase 4 |
| FieldItemRandomizerTests | 29 | Phase 4 |
| CharacterRandomizerTests | 41 | Phase 5 |
| InitialItemsRandomizerTests | 29 | Phase 5.5 |
| AbilityGemsRandomizerTests | 26 | Phase 5.5 |
| AbilityApRandomizerTests | 24 | Phase 5.5 |
| GearStatRandomizerTests | 26 | Phase 5.5 |
| **Total** | **424** | **All passing** |

---

## Deviations

| Item | Plan | Actual | Reason |
|---|---|---|---|
| TetraMaster | Phase 5.5 scope | Deferred to Phase 5.7 | TetraMaster is bytecode, not CSV — architecturally belongs in its own phase |
| "Bonus Sets" in project context | Listed under SynthesisRandomizer | Was actually GearStatRandomizer (Stats.csv bonuses) | Naming confusion in original project context — clarified this phase |
| InitialItemsRandomizer | Simple Fisher-Yates consumable shuffle | Full 5-mode enum system with debug gate | Sub-options rule applied; modes discovered during design session |
| AbilityGemsRandomizer | Tier 3 single-file | Tier 2 with AbilityGemMode enum | Sub-options rule applied |
| AbilityApRandomizer | ProportionalScale only | 4-mode enum (ProportionalScale, Inverse, FlatCost, Amnesia) | Sub-options rule applied; ApRoundToNearest5 dropped by developer |
| GearStatRandomizer | Simple bounded random | Full 3-mode × 4-weighting system with epic roll and debug gate | Vanilla distribution analysis informed constraint model design |
| StatsRow model | Phase 2 scope | Created this phase | Was not in Phase 2 typed model set — discovered as prerequisite |
| CsvRoundTripTests | No changes planned | 2 Stats tests added | Pre-close checklist review caught missing round-trip coverage |
| Settings.cs | 1 audit pass | 6 cumulative revisions | Each randomizer's sub-options required Settings additions |

---

## Rules Added This Phase

| Rule | Reason |
|---|---|
| As we iterate through randomizers, consider sub-options for fun, difficulty spikes, and silly ideas. Backtrack on earlier randomizers after all Phase 5.x tasks are complete. | Pattern established this phase — every randomizer has richer sub-options than originally planned; earlier randomizers need the same treatment |

---

## Parking Lot Additions This Phase

| Topic | Target | Notes |
|---|---|---|
| FilthyRich StartingItemMode | Phase 5.6 | High-value sellables as starting items — interacts with shop economy; design during Phase 5.6 |
| JunkDrawer StartingItemMode | Phase 5.6 | Low-tier items only — hard mode flavour; design during Phase 5.6 |
| AaVsSaSplit AbilityApMode | Future | Separate AP scale ranges for AA vs SA abilities; deferred — too many Settings properties for niche option |
| StatTheme GearStatMode | Future | Each item assigned a random "theme" (pure Str, balanced, etc.); needs theme table design |
| GearStatGeometricBase configurable float | Future | Let advanced users tune the geometric base manually (default 0.5); deferred for now |
| Sub-option backtrack for Phase 1–5 randomizers | After Phase 5.x complete | Developer confirmed: review earlier randomizers (EnemyRandomizer, FieldItemRandomizer, CharacterRandomizer, etc.) for sub-option additions after all Phase 5.x work is done |

---

## Commit Message

```
feat: Phase 5.5 complete — ability cost + gear stat randomizers

Settings.cs:
  - StartingItemMode enum: ConsumablesRandom, GearRandom, AbilityStarter,
    SpeedRunner, AllItems
  - AbilityGemMode enum: Shuffle, BoundedRandom, AllCheap
  - AbilityApMode enum: ProportionalScale, Inverse, FlatCost, Amnesia
  - GearStatMode enum: Proportional, Shuffle, Chaos
  - GearStatWeighting enum: Geometric, VanillaWeighted, Linear, Uniform
  - IsDebugMode flag gates: AllItems, AllCheap, Amnesia, AllStatsMaxed

StatsRow.cs: new model for Stats.csv (11 columns, UTF-8)

InitialItemsRandomizer:
  - 5 modes; GearRandom uses Price > 2 filter from Items.csv
  - RandomizeStartingCounts flag; AllItems debug-gated
  - Graceful fallback for null allItems in GearRandom

AbilityGemsRandomizer:
  - Shuffle: total gem budget preserved
  - BoundedRandom: configurable [min, max] range with swap/clamp validation
  - AllCheap: debug-gated, downgrades to Shuffle

AbilityApRandomizer:
  - Per-ability scaling: AbilityRefs sorted alphabetically for determinism
  - ProportionalScale: configurable [min%, max%] with validation
  - Inverse: vanillaMin + vanillaMax - vanillaCost (0 RNG calls)
  - FlatCost: configurable flat value (0 RNG calls)
  - Amnesia: debug-gated, downgrades to ProportionalScale

GearStatRandomizer:
  - Row filter: ID 0, gems 137-148, padding 157+ always skipped
  - ZeroStatItemsCanGainStats flag
  - Proportional: DrawStat(GearStatMax) + epic roll + HardCap clamp
  - Shuffle: Fisher-Yates, total stat budget preserved
  - Chaos: DrawStat(HardCap), no sum constraint
  - Geometric (p=0.5): ~2x more generous than vanilla
  - VanillaWeighted (p=0.35): matches vanilla rarity curve
  - AllStatsMaxed: debug-gated
  - Integer weight table (scale=100,000) for geometric precision

CsvRoundTripTests: Stats round-trip + Rebirth Ring known-value added
424/424 tests passing (+107 this phase)
New rule: consider sub-options for all randomizers
```

---

## PhaseEnd Changelog

```
v1.5.0 → v1.6.0
- Build Log: Phase 5.5 entry added
- Key Discoveries: Stats.csv vanilla distribution analysis, geometric calibration,
  per-ability AP scaling determinism requirement
- Deviations: TetraMaster deferral, StatsRow model creation, Settings iterations,
  sub-option expansion on all randomizers, CsvRoundTrip Stats addition
- Rules: 1 new rule (sub-options consideration for all randomizers)
- Parking Lot: FilthyRich, JunkDrawer, AaVsSaSplit, StatTheme, GeometricBase,
  Phase 1-5 sub-option backtrack
- Phase 5.5 marked complete
- Current phase: 5.6 — Shop + Synthesis Randomizers
```

---

## 🛑 Stop Here

This chat session is complete. Do the following before starting Phase 5.6:

1. Add `PhaseEnd_Phase5.5.md` to your Claude Project
2. Start a new chat session
3. Say: "Continue building Stiltzkin's Bag. Read all attached markdown files."

Do not continue development in this session. Do not delete this chat — keep it for
posterity and back-reference. Large chats in a project will not slow down chat or use more tokens.
