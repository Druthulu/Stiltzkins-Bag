# Ops & Setup — Stiltzkin's Bag

> **Evolvable reference (docs/ layer). Last revised 2026-07-06.** The single document a fresh session — or a fresh machine after an OS reinstall — rebuilds the working environment from. **Keep it current in the SAME change as any tooling/env/hook/path change** (registry H7); it rots silently otherwise. Mark not-yet-verified items `# TODO:` and fill them the next time you run the command.

## Version pins

| Component | Version | Notes / why pinned |
|---|---|---|
| Language/runtime | **.NET 8** (`net8.0` Core/Tests, `net8.0-windows` App). SDK 10.0.301 installed — builds net8.0 fine | Modern C#/LTS; App is WPF → Windows-only |
| Build tool | `dotnet` CLI / MSBuild; Visual Studio 2022 (17.13) also used | `.vs/` is VS-local (gitignored) |
| Test runner | xUnit 2.5.3 + Microsoft.NET.Test.Sdk 17.8.0 (coverlet.collector 6.0.0) | Tests target Core only |
| Key deps (Core) | CsvHelper 33.1.0 | Typed Memoria CSV round-trip |
| Key deps (App) | CommunityToolkit.Mvvm 8.4.2 · MaterialDesignThemes/Colors 5.3.1 · XamlAnimatedGif 2.3.1 | `[ObservableProperty]` MVVM; FF9 theme; loading GIF |
| Binary parsing | none (pure C#, ported from Hades Workshop C++) | GPL; author's personal permission |

## Environment

- **OS / shell:** Windows 11 (FFIX PC/Steam is Windows-primary; the WPF App is `net8.0-windows`). Shell: PowerShell (primary) + git-bash.
- **Project root:** `Z:\Storage\git\Stiltzkins-Bag` (solution at `StiltzkinsBag/StiltzkinsBag.sln`).
- **Machine notes:** solo dev machine; **no CI** — the local `dotnet test` run is the gate. Game data (StreamingAssets CSVs, `p0data2.bin`/`p0data7.bin`, TestData `.bytes` fixtures) is user-supplied and mostly gitignored.

## Build / run / test

```
# build
dotnet build StiltzkinsBag/StiltzkinsBag.sln
# run (WPF app — Windows only)
dotnet run --project StiltzkinsBag/StiltzkinsBag.App
# test gate (the machine-checkable milestone gate — registry M1 / §E G1)
dotnet test StiltzkinsBag/StiltzkinsBag.sln
```

- **The gate:** green = all tests pass (**879 as of Phase 9.2**) with a **0-warnings** target. Hard gates inside the suite: the two full-corpus `[Fact]` sweeps in `FieldScriptRoundTripTests` — `AllAvailableFieldFiles_DecodeWithoutException` and `RoundTrip_AllAvailableFieldFiles_ZeroEncodeMismatches` over all 838 `.eb.bytes` files. Determinism tests exclude timestamp-bearing outputs (`ModDescription.xml`, spoiler logs). Environment-dependent tests (modded/clean install) skip-and-log via a shared `SkipException` instead of hard-failing. `# TODO:` re-run `dotnet test` on this machine to reconfirm the 879 baseline after the migration.

## Claude Code integration (this project's agent wiring)

- **Statusline:** `~/.claude/statusline.sh` (model · effort · context · rate limits) — global; re-copy from the package on a fresh machine.
- **Effort baseline:** `effortLevel: "xhigh"` in `~/.claude/settings.json` (Max/Ultracode are session-only — see `docs/effort-map.md`).
- **Agent state (H8):** memory at `.claude-state/memory/` via `autoMemoryDirectory` in **`.claude/settings.local.json`** — *this is an absolute path and is machine-local; on a fresh machine, re-point it at `<this repo's absolute path>/.claude-state/memory`.* Transcripts: the `SessionEnd` hook in `.claude/settings.json` copies them into `.claude-state/transcripts/`; `tools/backup-claude-state.sh` sweeps stragglers (run at every phase boundary).
- **MCP / hooks:** No project MCP servers. One hook only: the `SessionEnd` command in `.claude/settings.json` runs `bash tools/backup-claude-state.sh --hook` (transcript sweep into `.claude-state/`, H8).

## Tooling inventory

| Tool | Location | Purpose |
|---|---|---|
| `tools/backup-claude-state.sh` | `tools/` | sweep agent state → `.claude-state/` (H8) |
| xUnit `Diagnostics/` tests | `StiltzkinsBag.Tests/Diagnostics/` | binary-analysis / disc-variant-fingerprint diagnostics that emit `.txt` reports (run on demand, not part of the gate) |

## Backup & git posture

- Commit cadence: per completed task, after updating `CURRENT_PHASE.md` (P4); the developer pushes (H6); no AI-attribution trailers (H5).
- Generated/regenerable outputs are gitignored; decision records are committed (phase-close gitignore discipline).
- `.claude-state/` is committed while the repo is private; **excluded from any public mirror** (transcripts capture full tool output).
- **User game data is gitignored** (large, user-supplied, and copyrighted): `FFIX Vanilla field data/`, `FFIX Vanilla enemy data/`, `**/TestData/StreamingAssets/`, `*raw16.bytes`, `**/Hades-Workshop-master/**`, the two `p0data2.bin` copies. Never commit extracted game bytes. The `_migration/` quarantine and `StiltzkinsBag_ProjectContext.md-old-dont-use` are retained but out of the load order.

## Fresh-machine restore checklist

1. Clone the repo to a fast local filesystem.
2. Install the pinned toolchain (Version pins table above).
3. Copy `project-architect-2.0/statusline/statusline.sh` → `~/.claude/statusline.sh`; merge the settings snippet into `~/.claude/settings.json`.
4. Re-point `.claude/settings.local.json`'s `autoMemoryDirectory` at this clone's absolute `.claude-state/memory` path.
5. Run the test gate to confirm the environment is live.
6. Provide the FFIX PC/Steam install (registry auto-detect finds the path via `GamePathLocator`) and copy the gitignored TestData `.bytes`/CSV fixtures from a clean install — without them, environment-dependent tests skip-and-log rather than run.
