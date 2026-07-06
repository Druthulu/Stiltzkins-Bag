# PhaseEnd — Phase 9.2.5: Project Architect 2.0 Governance Migration

**Date:** 2026-07-06 · **Project Version:** unchanged (code at Gen1 / Phase 9.2, 879 tests) · **Phase Status:** Complete

> The interphase that migrated Stiltzkin's Bag from its pre-2.0 Chat-era governance (a single `StiltzkinsBag_ProjectContext.md` + flat PhaseEnds) onto the **Project Architect 2.0** system installed by `project-architect-2.0/SETUP.md` (Path C). No product code changed — this is a governance/documentation migration, version-sorted between the completed Phase 9.2 and the planned Phase 9.3.

## Build Log

**Files created/changed and complete — do not recreate:**
- `CLAUDE.md` — auto-loaded entry point (protocols + failsafes); generation-time placeholders filled for this project.
- `PROJECT_CONTEXT.md` — regenerated permanent constitution (v2.0.0, Gen1, next Phase 9.3). **Never edit** (P1).
- `RULES_REGISTRY.md` — seeded P/E/H/X/M + **§E** (12 project rules G1–G12) + **§F** (40 per-phase rules R1–R40) + §E′ memory pointers.
- `docs/project-architect.md` — the methodology spec (survives package deletion).
- `docs/effort-map.md` · `docs/stiltzkins-bag-cookbook.md` · `docs/ops-setup.md` — evolvable references, generation-time content filled.
- `docs/AbilityTierClassification_Rev4.md` — moved from repo root (domain implementation contract; G11/R12).
- `phase-ends/` — `README.md`, `CURRENT_PHASE.template.md`, `PhaseEnd.template.md`, `logs/`, all 18 migrated PhaseEnds, and this file. `CURRENT_PHASE.md` archived to `logs/PhaseLog_9.2.5.md`.
- `tools/backup-claude-state.sh` — H8 state-sweep tool (executable).
- `.claude/settings.json` (SessionEnd backup hook) · `.claude/settings.local.json` (repo-local `autoMemoryDirectory`, machine-local, gitignored).
- `.claude-state/memory/` — 16 seeded memories + `MEMORY.md`; `who-is-dev` + `project-governance-system` filled. `.claude-state/transcripts/` — swept session transcripts (H8).
- `.gitignore` — appended `.run/`, `project-architect-2.0/`, `.claude/settings.local.json`.

**Path-C move mapping (via `git mv`, history preserved — `git log --follow` confirmed):**

| From (repo root) | To | Reason |
|---|---|---|
| `PhaseEnd_Phase1.md` … `Phase8.md` (14 dot-named) | `phase-ends/` (same names) | into the version-sorted load-order dir |
| `PhaseEnd_Phase5_7.md` / `5_8.md` / `9_1.md` / `9_2.md` | `phase-ends/PhaseEnd_Phase5.7/5.8/9.1/9.2.md` | separators normalized `_`→`.` for correct `sort -V` |
| `AbilityTierClassification_Rev4.md` | `docs/` | domain reference/contract doc |
| `Phase 5.9 Task1 Lost Chat Session.json` | `_migration/` | lost-session artifact quarantined (never deleted) |
| `StiltzkinsBag_ProjectContext.md` | `…-old-dont-use` | old constitution retired (kept as the §8 generation source) |

Legacy `~/.claude` project memory dir was empty — no memories to import. Left in place: `README.md`, `LICENSE`, `.gitattributes`, the `FFIX …/` + `StiltzkinsBag/` data/source dirs.

**Legacy-rule consolidation (Mode 4 step 4 — swept the old context's "AI Collaboration Rules" + every PhaseEnd rule-bearing heading):**

| Legacy rule | Disposition |
|---|---|
| Explain before coding | → seed **X1** |
| Preserve comments / Document disabled logic | → seed **H3** |
| One task at a time — strict | → seed **P4** |
| Mid-phase rules check | → seed **P6** |
| Verify every checkbox before closing | → seed **P7** |
| Confirm milestone before PhaseEnd | → seed **P3/P8/M1** |
| Main context file permanent & static | → seed **P1** |
| Single Random instance | → **§E G2** |
| RNG call order is sacred | → **§E G3** |
| ItemRemapTable before bytecode | → **§E G4** |
| Pipeline order is enforced | → **§E G5** |
| Hades Workshop port — port logic, not GUI | → **§E G6** |
| CSV round-trip is sacred | → **§E G7** (generalized to all codecs) |
| Service registration placement | → **§E G12** (Core/UI separation) |
| Per-phase PhaseEnd rules (18 phases) | → **§F R1–R40** (origin phases noted); technical facts → cookbook |
| **Extended Thinking Protocol** (Chat) | **retired/adapted** → `/effort max` + the Reasoning Protocol tiers (CLAUDE.md §B, E1–E3) |
| **"Everything outside chat goes in a file"** (Chat) | **retired/adapted** → CLAUDE.md Mandatory Behavior #4 (Claude Code writes to disk natively) |
| **"Never overwrite blind — paste the current version"** (Chat) | **adapted** → H-group / CLAUDE.md #6 (the agent Reads the file itself before overwriting) |
| **"Attach all PhaseEnd files at session start"** (Chat) | **retired** → CLAUDE.md Session Start auto-loads `phase-ends/PhaseEnd_Phase*.md` in `sort -V` order |
| **"PhaseEnd output is always a file"** (Chat) | native → **P8** |

**Tools/packages installed:** none (governance files only; no code/deps changed).

**Verification results:** every SETUP.md §-verify passed — five §2 files present + install checkboxes; §3 `sort -V` load order correct (integer `Phase5` precedes `Phase5.5`) + `git log --follow` shows pre-move history + old context `…-old-dont-use`; §4 five docs installed, 0 `{{PROJECT_NAME}}` leftovers, `.run` gitignored; §5 16 memories, both settings JSONs parse, `autoMemoryDirectory` ends `/.claude-state/memory`, no leftover interview comment; §6 statusline smoke-test `Test max` OK, user settings parse with `model` preserved; §7 commit `7aba1a1` trailer-free, no untracked governance; §8 §E non-empty (12) + §F populated (40) + **F9 placeholder audit clean** (only `docs/project-architect.md` hits).

**Milestone achieved:** the new constitution + registry exist and are consistent, the placeholder audit is clean, both install commits are trailer-free, and the developer confirmed the constitution at Gate 2 — Stiltzkin's Bag now runs under Project Architect 2.0 governance.

**Next:** Phase 9.3 — wire the Phase-9.2 `FieldScript` AST into `FieldParser.FindItemLocations` (replace the ad-hoc scanner; eliminate SetRegion/extended/`0x0D`/Pattern-B scan stops; complete the catalog). Start in a **fresh session** with plan mode + `/effort max`.

## Deviations

| Item | Plan | Actual | Reason |
|---|---|---|---|
| `.claude/` config writes (§5) | Write directly | Required explicit developer authorization | Auto-mode self-modification guard denied persistence/config writes until the dev authorized them |
| User `permissions` allowlist (§6) | Add read-only-git allowlist | Added only after a second, finer-grained approval | The guard treats widening the permission allowlist as more sensitive than hook/memory writes |
| Statusline (§6) | Copy package copy if absent | Kept the existing one | The dev's `~/.claude/statusline.sh` is newer/richer than the package; SETUP default is keep-existing |
| §8 PhaseEnd rule sweep | Read serially | Fanned out to 4 read-only subagents | Breadth-shaped (18 files); kept ~300KB of history out of the main context (dev opted into breadth) |
| §F granularity | All per-phase rules verbatim | 40 curated norms; technical facts → cookbook/PhaseEnds | Keeps session-start recitation tractable; PhaseEnds (loaded every session) retain full detail |
| Roadmap | Old context's Phase 9 = "Polish" | Superseded by the 9.1/9.2 codec effort; forward roadmap 9.3→9.5→10 | The ad-hoc scanner proved untrustworthy; a real codec port was required first (documented in Lessons Learned) |

## Commit Message

```
feat: Project Architect 2.0 installed — migrated + constitution regenerated
```
*(No `Co-Authored-By` / AI-attribution trailer — H5.)*

## Rules Added This Phase

No product rules were *authored* this phase. The registry was **populated by migration**: legacy general rules consolidated onto the seeded §A–§M IDs, project rules landed in §E (G1–G12), and per-phase rules in §F (R1–R40) — see the consolidation table above. Chat-era mechanics rules were retired with adaptation notes. Full operative text lives in `RULES_REGISTRY.md`.

## PhaseEnd Changelog

`Chat-era governance → PA 2.0` — constitution regenerated on the 2.0 skeleton; single canonical `RULES_REGISTRY.md` replaces rules scattered across PhaseEnds; `.claude-state/` agent memory + transcript hook added; `CURRENT_PHASE.md` crash-recovery introduced; effort doctrine (xHigh/Max/Ultracode) replaces the Extended-Thinking toggle.

## Plain-English Recap

Stiltzkin's Bag already had a working "rulebook" for how the AI should help build it, but that rulebook was written for the old Claude **Chat** way of working — you had to paste files in by hand every session, and the rules were scattered across nineteen documents. This phase moved everything onto **Project Architect 2.0**, the version built for Claude **Code**: the project now auto-loads its own constitution, a single always-recited rules file, and its full phase-by-phase history, and it remembers your preferences between sessions. Nothing about the actual randomizer changed — no code, no tests — only how the project governs itself. The real work picks up next at **Phase 9.3** (making the field-item scanner use the new, fully-tested script decoder), which should be started in a fresh session.
