# CURRENT_PHASE — Phase 9.2.5: Project Architect 2.0 Governance Migration

> The in-flight phase working file: the approved plan, the per-task checkpoint log, and blockers. **Copied from this template at Phase Start** (so the template is never overwritten); updated after every task; it is the crash/compaction recovery point and is in the Session Start load order. At phase close it is absorbed into `PhaseEnd_Phase[N].md` and `git mv`'d to `phase-ends/logs/PhaseLog_[N].md` (outside the load order). **Work accumulates commit-by-commit** — each task commits its changes together with its checkpoint entry here (P4), so this file never lags the code, and any task boundary is a clean handoff to a fresh session.

- **Started:** 2026-07-06
- **Effort baseline:** xHigh — this is a scripted installer (SETUP.md §1–§9), not a build phase; effort is fixed at the SETUP contract
- **Context budget:** single-session install; no mid-phase reset expected (the §9 hard stop mandates a fresh session)
- **Plan approved:** 2026-07-06 — plan is exactly "execute SETUP.md §1–§9 in order" (SETUP.md §0.2)
- **Baseline state:** Path C detected — latest completed Phase 9.2 (879 tests green, HW field script codec port); 18 PhaseEnds + StiltzkinsBag_ProjectContext.md present; migrating into Project Architect 2.0 governance

## Milestone (the phase's machine-checkable gate)

New `PROJECT_CONTEXT.md` constitution exists (all skeleton sections + per-phase Milestones) AND `RULES_REGISTRY.md` §E is non-empty and §F populated from the legacy PhaseEnds AND the placeholder audit `grep -rn "{{" --include="*.md" .` is clean (only the reference spec + phase-end templates hit) AND both install commits exist trailer-free.

## Task checklist

*Mark `[x]` done · `[~]` in progress · `[ ]` pending. Annotate each task with its effort level and a coarse **main-session context-cost bucket** `ctx:S|M|L|U` (Small / Medium / Large / Unpredictable — how much this task will grow THIS session's context, which drives reset decisions). Note: a breadth task routed to sub-agents is `ctx:S` even when large in total work, because the work happens in the isolated agents; a deep in-session debugging task is often `ctx:L` or `ctx:U`. E1.*

- [x] §3 — Governance file moves: git mv PhaseEnds → phase-ends/ (normalize separators), domain docs → docs/, lost-session → _migration/, old context → -old-dont-use *(xHigh · ctx:S)*
- [x] §4 — Docs layer, registry, tools, gitignore *(xHigh · ctx:S)*
- [x] §5 — CLAUDE.md, agent-state wiring, memory seeding, dev interview *(xHigh · ctx:M)*
- [x] §6 — Global assets (statusline + user settings) — skip-if-present *(xHigh · ctx:S)*
- [~] §7 — Install checkpoint commit *(xHigh · ctx:S)*
- [ ] §8 — Path C migration: consolidate rules into registry + generate new constitution + fill generation-time placeholders (interactive) *(Max · ctx:L)*
- [ ] §9 — Close install phase: PhaseEnd_Phase9.2.5, backup, archive CURRENT_PHASE, final commit, hard stop *(xHigh · ctx:S)*

## Current task

**NOW:** §7 — Install checkpoint commit
**NEXT:** §8 — Path C migration + constitution generation (interactive)
**Last rules check:** n/a (scripted install; SETUP.md is the checklist)

## Per-task checkpoint log

*On completing each task, append a rich checkpoint (2 to 4 sentences, not one line): what the task did, the key decision or WHY, any deviation from the plan, and any gotcha or finding worth remembering. Write every entry as if this task is the last thing before a fresh session picks the phase up, because it might be — the final session synthesises the PhaseEnd's what-and-why from this log, the commits, and the code, so the reasoning that would otherwise live only in this session's conversation has to be captured here now (X5). Keep it concise: push long evidence, logs, or code dumps into the commit body or the cookbook, never here, because this file reloads at every session start. Each entry commits with that task's changes (P4).*

- **§3** — Moved 21 governance artifacts via `git mv` (history preserved): 18 PhaseEnds → `phase-ends/` (normalized 4 underscore names `_`→`.`: 5.7, 5.8, 9.1, 9.2), `AbilityTierClassification_Rev4.md` → `docs/`, lost-session JSON → `_migration/`, old context → `…-old-dont-use`. Verified: `sort -V` load order correct (Phase5 before Phase5.5). Mapping table saved for the PhaseEnd.
- **§4** — Installed docs layer (`docs/project-architect.md` methodology, `effort-map.md`, `stiltzkins-bag-cookbook.md`, `ops-setup.md`), `RULES_REGISTRY.md` seed, `tools/backup-claude-state.sh` (+x), gitignore append (`.run/`, `project-architect-2.0/`). Filled copy-time placeholders only; generation-time ones deferred to §8. Verified: 0 `{{PROJECT_NAME}}` leftovers, `.run` ignored.
- **§5** — CLAUDE.md installed + copy-time filled (name/tagline/cookbook); `.claude/settings.json` (SessionEnd backup hook) + `settings.local.json` (repo-local `autoMemoryDirectory`) written after explicit dev authorization (auto-mode self-modification guard denied first). Seeded 16 memories; filled `who-is-dev` (Drew, solo, Advanced, autonomous-within-phases, breadth-welcome) + `project-governance-system`. Legacy import: no legacy memories (empty dir). Verified: 16 memories, JSON parses, no leftover interview comment.
- **§6** — Statusline: kept existing `~/.claude/statusline.sh` (newer/richer than package; smoke-test passed). User settings merge: added ONLY the read-only-git `permissions` allowlist after explicit dev approval (finer-grained guard); `model`/`statusLine`/`effortLevel` already present, untouched. Verified: settings parse, model preserved.

## Blockers / open questions

- none

## Notes / deviations

- [anything worth carrying into the PhaseEnd's Deviations table]
