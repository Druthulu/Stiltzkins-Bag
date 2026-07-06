# Stiltzkin's Bag — Project Context & Roadmap

> **Version:** 2.0.0 (constitution regenerated at the Project Architect 2.0 migration, Phase 9.2.5)
> **Generated:** 2026-07-06
> **Generation:** Gen1 (mid-flight — Phase 9.3 next) | **Tech Stack:** C# / .NET 8 (`net8.0` Core+Tests, `net8.0-windows` App/WPF), MaterialDesignInXaml + CommunityToolkit.Mvvm, xUnit, CsvHelper; output = Memoria Engine mod-folder overlay

---

## For Humans — Quick Guide

This is your project's **permanent constitution** — the vision, decisions, architecture, and roadmap. It is written once and **never edited** (rules and state evolve elsewhere). A fresh Claude Code session auto-loads `CLAUDE.md`, which reads this file first.

**The document system (where everything lives):**
- **This file** — permanent vision/decisions/architecture/roadmap. Never edited.
- **`RULES_REGISTRY.md`** — every rule, full text, recited each session.
- **`phase-ends/`** — the append-only build history (`PhaseEnd_Phase*.md`) + the in-flight `CURRENT_PHASE.md`.
- **`docs/`** — evolvable references (effort map, cookbook, ops-setup, the methodology spec, the AbilityTier contract).

**To work on this project:** open Claude Code in the repo and say "continue" — the Session Start Protocol takes over. Governance methodology: `docs/project-architect.md`.

*(This constitution replaced the pre-2.0 `StiltzkinsBag_ProjectContext.md`, retained as `…-old-dont-use`. The migration is recorded in `phase-ends/PhaseEnd_Phase9.2.5.md`.)*

---

## Rules & Protocols

The AI collaboration rules and the Session Start / Phase Start / Phase Boundary protocols are **not** in this file (a permanent-static file would freeze them). They live in:
- **`RULES_REGISTRY.md`** — the canonical, full-text rule set (P/E/H/X/M groups + this project's §E rules G1–G12 + accumulated §F phase rules R1–R40). Recited every session.
- **`CLAUDE.md`** — the operating protocols (session start, mandatory behavior, phase boundary, reasoning/effort).

**Precedence:** where this permanent file's wording ages out of step with them, `CLAUDE.md` + `RULES_REGISTRY.md` govern; `docs/effort-map.md` governs effort language.

---

## Quick Reference Card

- **Project:** Stiltzkin's Bag — a seed-based randomizer for **Final Fantasy IX (PC / Steam)** that outputs a Memoria Engine mod folder.
- **Goal:** every seed completable, every run surprising, every output **byte-for-byte deterministic** (same seed + settings → identical mod).
- **Definition of done (the milestone gate):** `dotnet test StiltzkinsBag/StiltzkinsBag.sln` green (879 tests as of Phase 9.2, 0-warnings target), including the byte-identical determinism + full-corpus codec round-trip sweeps (§E G1/G7).
- **Stack:** C# .NET 8, WPF + MaterialDesignInXaml + CommunityToolkit.Mvvm, xUnit, CsvHelper. Windows-only. Output overlays via Memoria's `FolderNames`.
- **Environment:** see `docs/ops-setup.md`.
- **Current generation:** Gen1 — full mod-folder randomizer (CSV + bytecode features, WPF UI, Hades-Workshop-ported parsers). **Next: Phase 9.3.**

---

## Project Overview

Stiltzkin's Bag is a ground-up rewrite of *rand9er v2.2* (WinForms, .NET 4.7.2). It reads the user's installed FFIX game data, applies randomization from user-selected settings and a numeric seed, and writes a Memoria Engine mod folder of modified CSVs and individually patched raw `.bytes` field/battle files. Players load the folder via Memoria's overlay to play a randomized run. The same seed + settings always produces byte-identical output, enabling competitive races and shareable community seeds.

The engine (`StiltzkinsBag.Core`) has zero UI dependencies and is fully test-covered; the WPF app (`StiltzkinsBag.App`) is a thin MVVM shell over it. Binary game data is accessed through C# parsers **ported from Hades Workshop's C++ source** (used with the author's personal permission; GPL) rather than brittle byte-pattern matching — giving structured, surgical edits and small mod output.

### Generation Map

| Generation | Focus | Status |
|---|---|---|
| **Gen1** | Full mod-folder randomizer — all CSV + bytecode features, WPF UI, HW-ported parsers/codec | **In progress — Phase 9.3 next** |
| Gen2 | Runtime companion DLL (BepInEx + HarmonyX), field-entrance randomizer (maze solver), enemy stat randomization | Pending Gen1 public release + 30 days stable |
| Gen3 | Enemy-encounter randomization, level-scaling integration, story-cutscene item randomization, race/blind mode | Pending Gen2 field randomizer complete |

**Gen1 build history (complete — detail in `phase-ends/`):** Phase 1 foundation (SeedEngine, Settings, ItemRemapTable, 3-project solution) · Phase 2 typed Memoria-CSV round-trip layer (`MemoriaCsvParser` + 13 models) · Phase 3 structured binary layer (`EnemyFile`, `UnityArchiver`, `ModSourceResolver`) · Phase 4 item remap + enemy/field bytecode randomizers (`FieldParser`, `ItemRemapper`, `EnemyRandomizer`, `FieldItemRandomizer`) · Phase 5 `CharacterRandomizer` (stats→speciality→abilities→equipment) · Phases 5.5–5.7 remaining CSV + TetraMaster randomizers · Phases 5.8–5.9.3 `VanillaItemCatalog` obtainability model + data-integrity hardening (`FieldItemScanner`, `BattleItemScanner`, disc-variant fingerprinting) · Phase 5.99 `StiltzkinRandomizer` · Phase 6 `RecommendedLogicEngine` · Phase 7 WPF UI (`MainViewModel`, FF9 theme, presets, `GamePathLocator`) · Phase 8 `RandomizerEngine` pipeline + mod output (`ModOutputWriter`, `MemoriaLoadOrder`, spoiler log) · Phase 9.1 `FieldParser` sequential rewrite + `FieldScriptOpcodeTable` · Phase 9.2 full HW field-script codec port (838 files round-trip byte-identical; 879 tests).

### Project Assumptions

- **Developer:** solo; QA engineer by trade; advanced (see the `who-is-dev` memory — autonomous-within-phases, breadth welcome, recommendation-first).
- **Target audience:** speedrunners, racers, challenge runners, casual players.
- **Platform:** Windows only (FFIX PC/Steam is Windows-primary; the App is WPF/`net8.0-windows`).
- **Seed reproducibility is non-negotiable** — same seed + settings must always produce byte-identical output.
- **Memoria Engine is a prerequisite** — users install/configure it themselves; Stiltzkin's Bag does not.
- **Hades Workshop** source is C++ (GNU GPL); the dev has the author's personal permission; the project must stay GPL-compatible.
- **No runtime patching in Gen1** — all randomization is mod-folder output only.

---

## Lessons Learned / Known Risks

| Problem / Risk | Detail | Mitigation |
|---|---|---|
| Broken RNG in v2.2 | `new Random(derived_formula)` per cell — adding/removing a feature shifted every other output for the same seed | One `Random` seeded once, called in a fixed deterministic order (§E G2/G3) — the founding rule |
| Global static state on the WinForms class | Untestable, hard to extend | Clean `Settings` model + pure engine with zero UI dependencies (§E G12) |
| Raw string CSV manipulation | Magic index offsets everywhere, fragile on Memoria updates | Typed model per CSV + `MemoriaCsvParser` (CsvHelper) |
| Byte-string matching for binary data | Brittle; needed a 435KB enemy dictionary | Port HW parsers/codec to C# for structured access (§E G6) |
| Memoria CSV format drift | File structure changed between versions | Read from the user's live `StreamingAssets` at generation time; reference data is validation-only (§E G8) |
| Item shuffle breaks bytecode references | Shuffled `items.csv` invalidates enemy/chest ID references | Item randomization runs first, produces `ItemRemapTable`, consumed before any binary write (§E G4) |
| **Roadmap divergence — Phase 9 "polish" became a codec effort** | The original Phase 9 was "Polish/Debug/Edge Cases," but the ad-hoc `FieldParser` scanner proved untrustworthy (0x48-in-arg false positives, scan stops on switch/SetRegion/extended opcodes), corrupting the item catalog. A proper **sequential decoder + full HW codec port** (Phases 9.1–9.2) was required before the catalog could be trusted — a far deeper effort than "polish." | Phase 9.3 wires the new AST into `FieldParser.FindItemLocations`; the original polish/debug/edge tasks were deferred to **Phase 9.5**. |
| Field-count-derived item tiers unverified | `FieldParser` false positives inflated chest counts; some Unique/Legendary classifications marked `// UNVERIFIED` | Re-audit after the Phase 9.3 scanner fix (§F R30) |
| Guide vs game-data disagreement | Community guide CSVs diverge from the actual game binaries | Game files are source of truth; guides validate only (§E G8, §F R18) |

---

## Project Philosophy

> **Every seed should be completable, every run should be surprising, and every output should be deterministic.**

The randomizer must never produce an unwinnable state. Legendary items stay rare; mechanically required items stay obtainable; randomized characters feel coherent (speciality, abilities, and equipment tell a consistent story). **Recommended mode** enforces this automatically via the constraint engine; **Chaos mode** removes the guardrails for players who want maximum chaos.

**North Star (one sentence):** a seed + a settings string reproduces a byte-identical, always-completable randomized FFIX run on any machine.

---

## Key Decisions

| Decision | Choice | Rejected alternative | Why |
|---|---|---|---|
| Output method | Mod-folder file overlay | Runtime DLL (deferred to Gen2) | Proven, testable output; no Unity process debugging; existing bytecode work ports directly |
| UI framework | WPF + MaterialDesignInXaml | WinForms / Electron / Avalonia | Modern feel, custom FF9 theming, pure C# XAML, no JS runtime |
| Binary parsing | Port Hades Workshop C++ parsers/codec to C# | Ship an enemy byte dictionary; raw byte matching | Structured surgical edits, small output, no brittle pattern matching (§E G6) |
| Field scanning | Sequential opcode decode from a ported HW opcode table | Byte-pattern matching on opcode bytes | Pattern matching false-positives on argument data (Phase 9.1 rewrite; §F R35) |
| RNG architecture | Single `Random` seeded once, sequential calls | Per-cell `new Random(derived_formula)` | Deterministic reproducibility across feature combinations (§E G2/G3) |
| Data source of truth | The user's live game binaries; guides validate only | Trust community guide CSVs; embed copies | Game data is authoritative; guides have gaps/errors (§E G8, §F R18) |
| Project structure | One solution, two projects + tests (Core / App / Tests) | Monolith or many assemblies | Core testable without UI; clean engine/UI split (§E G12) |
| Target framework | .NET 8 | .NET 4.7.2 (old project) | Modern C#, current LTS |

---

## Core Logic / Strategy

### The mod-folder overlay
Memoria loads files from mod folders ahead of the base game's `StreamingAssets`. Stiltzkin's Bag writes a named seed folder (`StiltzkinsBag-Seed-[int]/…`) under the game's Mods directory, populates it with only the CSVs and raw `.bytes` files the active settings actually modify (untouched files fall through to the base game), and registers it first in Memoria's `FolderNames`.

### Randomization pipeline (strict, enforced order — §E G4/G5)
1. **Seed resolution** — seed string → deterministic `SeedInt` (`SeedEngine.Resolve`, platform-independent hash); one `Random(SeedInt)` for the whole run.
2. **Item remap** (if enabled) — assign new item IDs; produce `ItemRemapTable` for all downstream consumers.
3. **Enemy binary patches** — drops, steals, blue magic, card drops; consume `ItemRemapTable`.
4. **Field binary patches** — chest/field pickups (all 7 locales); consume `ItemRemapTable`.
5. **Character pipeline** — base stats → speciality (`CommandSets`) → abilities → equipment → starting items (Recommended: ordered; Chaos: independent).
6. **Gear stat bonuses** → **shops** → **synthesis** (+ Bonus Sets) → **ability gems** → **Tetramaster cards** (bytecode) → **Stiltzkin**.
7. **Recommended-Logic validation** — constraint pass over all outputs; correct violations.
8. **Spoiler log** → **mod-folder output** (CSVs + patched binaries + `Memoria.ini` load order + metadata).

`AbilityGemsRandomizer` runs after `CharacterRandomizer` so AP-cost overrides land on the final tables. Character speciality assignment is greedy most-constrained-first over class archetypes with separable/locked command pairs (full tables + algorithm in `docs/AbilityTierClassification_Rev4.md`, the implementation contract — §E G11 / §F R12).

### Component Inventory (as-built — namespaces under `StiltzkinsBag.Core`)
- **Models / Models/Csv / Models/Battle / Models/TetraMaster:** `Settings`, `GenerationResult`, `ItemRemapTable`, 13+ typed CSV row/map models, `ItemObtainabilityEntry`, catalog models.
- **Parsing:** `MemoriaCsvParser`, `UnityArchiver`, `EnemyFile`, `FieldParser`, `FieldScriptOpcodeTable`, `FieldScript`/`FieldScriptFunction`/`FieldScriptOperation`/`FieldScriptArgument` (the ported codec), `FieldItemScanner`, `BattleItemScanner`, `ModSourceResolver`, `FieldScriptExclusions`.
- **Randomizers:** `SeedEngine`, `ItemRemapper`, `EnemyRandomizer`, `FieldItemRandomizer`, `CharacterRandomizer`, `GearStatRandomizer`, `ShopRandomizer`, `SynthesisRandomizer`, `AbilityGemsRandomizer`, `AbilityApRandomizer`, `InitialItemsRandomizer`, `TetraMasterRandomizer`, `StiltzkinRandomizer`, `RecommendedLogicEngine`, `VanillaItemCatalog`, `VanillaObtainabilityData`, `ItemPool`, `LegendaryItemList`, `SynthesisShopData`.
- **Output:** `RandomizerEngine` (the orchestrator), `ModOutputWriter`, `MemoriaLoadOrder`, `ModDescriptionWriter`, `ModMemoriaIniWriter`, `SettingsFileWriter`, `SpoilerLogWriter`.
- **App (`StiltzkinsBag.App`):** `MainViewModel`, `MainWindow`, `GamePathLocator`, `Presets`, FF9 theme resources.

---

## Safety / Guardrails / Error Handling

| Mechanism | What it does | Trigger |
|---|---|---|
| Legendary rarity guard | No legendary/unique items in shops or as common drops; ≤1 per chest pool | Recommended mode (§F R15) |
| Synthesis reachability | Removes a shop from a recipe when an ingredient isn't reachable before that shop's story point | Recommended mode (§F R29) |
| Equipment/ability coherence | Equipment carries abilities appropriate to a character's randomized speciality (greedy pairwise swap, zero RNG) | Recommended mode (§F R28) |
| Uniqueness / obtainability | Unique (finite=1) items never placed in infinite-source slots; pools traceable to an obtainability source | Recommended mode (§F R15) |
| Gear bonus budget | Randomized `BonusId` within ±20% of the base-game per-item average; hard cap +3/stat for non-legendary | All modes |
| Consumable-only starting items | `InitialItems` drawn only from the consumable pool | All modes |
| Remap propagation | `ItemRemapTable` applied to every binary editor before any binary write | When item shuffle active (§E G4) |
| Mod load-order safety | Verify the mod folder is writable; never overwrite a non-SB mod; manage `Memoria.ini` via `FolderNames` | Always (§E G10) |
| Key-item note | FFIX key items are outside Items.csv (IDs 0–255); key-item protection is Gen2+ | Placeholder (§F R27) |

---

## Architecture

### Project Structure (as-built)

```
StiltzkinsBag/StiltzkinsBag.sln
├── StiltzkinsBag.Core/            # engine — zero UI deps (net8.0)
│   ├── Models/  ├─ Csv/  ├─ Battle/  └─ TetraMaster/
│   ├── Parsing/                   # CSV + Unity archive + field/enemy codec + scanners
│   ├── Randomizers/               # all randomizers + catalog/obtainability + RecommendedLogicEngine
│   └── Output/                    # RandomizerEngine orchestrator + mod-folder writers
├── StiltzkinsBag.App/             # WPF UI — depends on Core only (net8.0-windows)
│   ├── ViewModels/  ├─ Views/  ├─ Services/  └─ Resources/{Themes,Sprites,Fonts}
└── StiltzkinsBag.Tests/           # xUnit — Core only (net8.0)
    ├── Integration/  ├─ Diagnostics/  ├─ Output/  └─ TestData/   (879 tests)
```

Namespaces: `StiltzkinsBag` (Core root), `StiltzkinsBag.App` (App), `StiltzkinsBag.Tests` (Tests).

### Services / Dependency Wiring
`RandomizerEngine` (Core) takes a `Settings` + game path, runs the pipeline in deterministic order, threads the single `Random` and `ItemRemapTable` downstream. All randomizers are pure functions. `MainViewModel` builds `Settings` from UI state and calls `RandomizerEngine.RunAsync()` on a background thread, reporting via `IProgress<(double,string)>`. `ModSourceResolver` walks the Memoria mod stack (raw `.bytes` → mod archive → vanilla archive → catalog) to resolve source bytes.

### Config / Settings
`Settings` is a flat, serializable toggle model (seed, game path, mode, and per-feature enums/bools for character/item/enemy/Tetramaster/Stiltzkin sub-options). It round-trips to a base64url **settings string** (share/reproduce a run) and to `Settings-Seed-[int].json` in the output folder.

---

## Testing & Validation Strategy

| Stage | What | Purpose |
|---|---|---|
| Unit — seed engine | Same string → same int; same int → same RNG sequence | Determinism core |
| Unit — CSV round-trip | Read→write→diff each CSV type | Parser fidelity (byte- or content-identical) |
| Unit — binary round-trip | Decode→re-encode→byte-diff = 0 over the full `.eb.bytes` corpus | Codec fidelity (§E G7) |
| Unit — randomizer determinism | Each randomizer twice, same seed → identical output | Per-module reproducibility |
| Integration — full pipeline | Full generation twice, same seed → byte-identical mod (excl. timestamped files) | End-to-end determinism (§E G1) |
| Integration — remap propagation | Item shuffle on → all binary patches reference remapped IDs | No stale references |
| Manual — Recommended | Load in game: no legendaries in shops, coherent builds, completable | Logic-engine correctness |
| Manual — Chaos | Load in game: no crash on load | Minimum sanity |

**Pass criteria:** same seed+settings → identical output (100%, zero tolerance, excluding timestamp-bearing files); binary & CSV round-trip diff = 0; no legendaries in Recommended shops; all protected key content reachable; game loads without crash in all modes; **0 build warnings**. The gate is `dotnet test` — including the two full-corpus `[Fact]` field-script sweeps.

---

## Build Roadmap

*Forward phases only (Gen1 is at Phase 9.3). Completed Phases 1–9.2 are summarized in the Generation Map and detailed in `phase-ends/`. Each phase ends with an observable, machine-checkable Milestone (§E G1 / registry M1/P9).*

### Phase 9.3 — Wire the FieldScript AST into `FieldParser.FindItemLocations`
- [ ] Replace the ad-hoc sequential scanner in `FindItemLocations` with the Phase-9.2 `FieldScript` AST.
- [ ] Eliminate all current scan stops (SetRegion `0x29`, extended opcodes, `0x0D`, Pattern-B indirect gives) using the AST.
- [ ] Complete the Whale Whisker / remaining catalog gaps; re-audit the `// UNVERIFIED` Unique/Legendary tiers (§F R30).
- [ ] Clear the remaining Phase-9.1 deferred scanner tasks.
- [ ] **Milestone:** every field item location is resolved via the AST with zero scan stops across all 838 field files; the item catalog is trustworthy; `dotnet test` green (determinism + full-corpus round-trip sweeps pass).

### Phase 9.5 — Polish, Debug Tooling, Edge Cases *(the deferred original Phase-9 tasks 4–16)*
- [ ] Structured error messages for common failure cases (missing CSVs, read-only game dir, partial mod folder).
- [ ] Port the debug window to WPF, gated behind a dev flag; add debug-build logging.
- [ ] Verify every randomizer handles boundary values without exceptions.
- [ ] The deferred **sub-option backtrack** pass (§F R14) across the Phase 1–5 randomizers.
- [ ] Seed-determinism regression: 10 seeds × all feature combinations → byte-identical.
- [ ] **Milestone:** the app handles all common error states gracefully; the 10-seed × feature-combo determinism regression is green.

### Phase 10 — Gen1 Packaging & First Public Release
- [ ] Packaging/installer; user-facing docs; final theming/UX pass.
- [ ] Cross-machine determinism verification (byte-identical output on two machines).
- [ ] **Milestone:** the mod generates, loads in Memoria, and a Recommended seed is completable start-to-finish; determinism verified byte-identical across two machines — ready for the first public race.

---

## Enhancement Backlog
*(Toggleable, feature-flagged modules added one at a time and measured before the next.)*

| Priority | Enhancement | Target |
|---|---|---|
| High | Sub-option backtrack (enrich early randomizers to parity — §F R14) | Phase 9.5 |
| High | Named presets refinement (Recommended/Chaos/Casual/Competitive) | shipped Phase 7; tune |
| Medium | Disc-variant field-pair synchronizer (position-61 chest-counter handling — §F R21) | Phase 9.3/9.5 |
| Medium | Richer spoiler-log routing detail | Phase 9.5 |
| Low | Stiltzkin walking animation / loading polish | Phase 9.5/10 |

---

## Future Generations
*(Evolutionary leaps, sketched — not phase-detailed.)*

- **Gen2 — Runtime DLL + Field-Entrance Randomizer.** BepInEx + HarmonyX companion DLL for live patching (enemy stat randomization: HP/MP/attack/defense; possible persistent seed state). Field-entrance randomizer over the `CityExits` (79 fields) / `OverWorldConnectors` graph, requiring a `MazeSolver` to guarantee completability and story-progression bytecode injection. New components: `FieldGraphBuilder`, `MazeSolver`, `ProgressionInjector`. **Gate:** do not start until Gen1 is publicly released and stable (no critical bugs for 30 days).
- **Gen3 — Enemy Encounters + Advanced.** Enemy-encounter randomization, level-scaling mod integration, story-cutscene item randomization, race/blind mode, possible cross-platform (Avalonia). **Gate:** do not start until the Gen2 field randomizer is complete and validated.

---

## What Success Looks Like

| Milestone | Measure |
|---|---|
| Gen1 release | Mod generates, loads in Memoria, and a Recommended seed plays start-to-finish |
| Determinism verified | 20 seeds × full feature set → byte-identical output across two machines |
| Community adoption | First public race event on Stiltzkin's Bag seeds |
| Gen2 field randomizer | All field types classified; maze solver confirms completability before output |

---

## Feature & Architecture Inventory

Seed-based deterministic randomization · shareable settings string · Recommended (constraint-enforced) and Chaos modes · full character pipeline (stats→speciality→abilities→equipment) · gear-stat-bonus randomization (budget-constrained) · enemy/field/chest bytecode editing via HW-ported parsers (no raw byte matching) · TetraMaster card randomization via bytecode (TripleTriad/FF8 mode explicitly unsupported) · Stiltzkin package randomization · vanilla obtainability catalog (game-data-derived) · surgical individual-file mod output · spoiler log · registry-based game-path auto-detect · WPF + MaterialDesign custom FF9 theme.

---

## Data Sources / External Dependencies

| Source | Provides | Notes |
|---|---|---|
| Memoria CSVs (`StreamingAssets/Data`) | All editable game data | Read from the user's install at generation time (§E G8) |
| `p0data7.bin` (Unity archive) | Field + world-map scripts (`evt_*.eb`) | Extracted at generation; scanned/patched |
| `p0data2.bin` (Unity archive) | Enemy/battle bytecode | Extracted at generation; `BattleItemScanner` reads it (§F R22) |
| Hades Workshop source (C++) | Parser/codec reference (UnityArchiver, Enemies, Fields/codec, MIPS) | GNU GPL; author's personal permission |
| `FFIX GUIDE DATA/` | Obtainability cross-reference (missables, chocographs, Kupo Nuts) | **Validation-only** — game data wins (§E G8, §F R18) |
| Windows Registry | FFIX install-path auto-detect | `GamePathLocator`, 3 key locations |

---

## Libraries / Dependencies

| Purpose | Package | Project |
|---|---|---|
| CSV parsing | CsvHelper 33.1.0 | Core |
| MVVM | CommunityToolkit.Mvvm 8.4.2 | App |
| WPF theming | MaterialDesignThemes / MaterialDesignColors 5.3.1 | App |
| Animated loading GIF | XamlAnimatedGif 2.3.1 | App |
| Unit testing | xUnit 2.5.3 + Microsoft.NET.Test.Sdk 17.8.0 + coverlet.collector 6.0.0 | Tests |
| JSON | System.Text.Json (built-in) | Core/App |
| Binary parsing | none — pure C#, ported from HW | Core |

---

## Parking Lot
*(Captured, explicitly not committed.)*

TripleTriad.csv (FF8 card mode — out of scope, separate mod) · enemy stat randomization (Gen2) · enemy-encounter randomization (Gen3) · level-scaling integration (Gen3) · story-cutscene item randomization (Gen3) · race/blind mode (Gen3) · cross-platform Avalonia port (Gen3 if warranted) · boss-specific stat scaling (Gen3) · `BattleParameters.csv` randomization (out of scope — high crash risk) · `MagicSwordSets.csv` randomization (Gen2 if feasible) · runtime companion DLL + persistent seed state (Gen2).

---

## Notes for Future Phases

- **Phase 9.3 is the current front:** the codec (Phase 9.2) is complete and round-trips all 838 files, but `FieldParser.ScanFunction` still stops on `IsSwitch` opcodes (0x06/0x0B/0x0D/0x29) because it isn't yet on the new AST — that wiring is the whole of 9.3. Custom Memoria opcodes beyond 0x111 (via `ScriptAPI.txt`) are the only uncovered codec edge case; none appear in the current install.
- `0x0D` (unknown switch) is decoded as a best-guess uint16-count JMP_SWITCH, flagged UNCONFIRMED — validated only by the round-trip sweep (§F R40). Revisit if a future install exposes it.
- The disc-variant synchronizer (position-61 chest-counter near-misses in ALEX1/3, CLEYRA2/3) is still pending (§F R21).
- `docs/AbilityTierClassification_Rev4.md` is a first-class implementation contract — keep it in `docs/`, referenced from here, never folded into this constitution.
