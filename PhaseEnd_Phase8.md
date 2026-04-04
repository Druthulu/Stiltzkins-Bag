# Phase 8 — PhaseEnd

**Completed:** 2026-04-03
**Tests at phase end:** 847 total — all green, 0 warnings
**Next phase:** 9 (Polish, Debug Tooling, Edge Cases)

---

## Phase 8 Task Checklist

| # | Task | Status |
|---|------|--------|
| 1 | `ModOutputWriter.cs` | ✅ Done |
| 2 | `MemoriaLoadOrder.cs` | ✅ Done |
| 3 | `ModDescriptionWriter.cs` | ✅ Done |
| 4 | `ModMemoriaIniWriter.cs` | ✅ Done |
| 5 | `SettingsFileWriter.cs` | ✅ Done |
| 6 | Tests for tasks 1–5 (`OutputWriterTests.cs`) | ✅ Done |
| 7 | `SpoilerLogData.cs` + `SpoilerLogWriter.cs` | ✅ Done |
| 8 | `RandomizerEngine.cs` — full pipeline orchestrator | ✅ Done (2 stubs remain) |
| 9 | Wire `GenerateCommand` to `RandomizerEngine.RunAsync` | ✅ Done |
| 10 | UI polish — Memoria prompt + Remove from Load Order | ✅ Done |
| 11 | Determinism integration test | ✅ Done |

---

## What Was Built

### Tasks 1–5 — Output Infrastructure

**`ModOutputWriter.cs`** (`StiltzkinsBag.Core/Output/`):
- Seed folder path: `[gamePath]/Mods/StiltzkinsBag-Seed-[seedInt]/`
- Subfolder constants: `CharactersRelPath`, `AbilitiesRelPath`, `ItemsRelPath`, `BattleRelPath`
- `EnsureCleanSeedFolder()` — delete + recreate on re-generate
- `WriteModifiedCsv<T, TMap>(relPath, ParsedCsv<T>)` — delegates to `MemoriaCsvParser.Write`
- `WritePatchedBinaryFile(relPath, byte[])` — for enemy/field binary output
- `GetAbsolutePath(relPath)` — resolves relative paths within seed folder

**`MemoriaLoadOrder.cs`** (`StiltzkinsBag.Core/Output/`):
- Custom line-by-line parser for Memoria.ini `FolderNames` (single quoted CSV string)
- `InjectSeedFolder(iniPath, seedInt)` — prepend + strip old SB entries
- `RemoveAllSbEntries(iniPath)` — strip without adding
- `ParseFolderNames(line)` / `BuildFolderNamesLine(names)` — public for tests
- All non-FolderNames lines preserved byte-identically

**`ModDescriptionWriter.cs`** (`StiltzkinsBag.Core/Output/`):
- Writes `ModDescription.xml` to seed folder root
- Content: Author (Stiltzkin's Bag), Version (1.0), Description (seed + mode + UTC timestamp)

**`ModMemoriaIniWriter.cs`** (`StiltzkinsBag.Core/Output/`):
- Writes mod-local `Memoria.ini` override to seed folder root
- Only written when overrides are needed; returns `bool` indicating whether file was created
- Current override: `[TetraMaster] TripleTriad = 0` when `settings.RandomizeTetraMaster == true`

**`SettingsFileWriter.cs`** (`StiltzkinsBag.Core/Output/`):
- Writes `Settings-Seed-[seedInt].json` using `settings.ToJson()`

---

### Task 6 — Tests (`OutputWriterTests.cs`)

26 new tests across 5 test classes:
- `ModOutputWriterTests` — folder structure, re-generate clean, path resolution, binary write
- `MemoriaLoadOrderTests` — ParseFolderNames, BuildFolderNamesLine, InjectSeedFolder (prepend,
  idempotent, old SB removal), RemoveAllSbEntries, non-SB line preservation, error cases
- `ModDescriptionWriterTests` — file created, XML elements, description content
- `ModMemoriaIniWriterTests` — no file when flag off, file created, [TetraMaster] section, TripleTriad=0
- `SettingsFileWriterTests` — correct filename, valid JSON, Settings round-trip

---

### Task 7 — `SpoilerLogData.cs` + `SpoilerLogWriter.cs`

**`SpoilerLogData`** — init-property bundle passed from RandomizerEngine to SpoilerLogWriter:
- Required: `SeedInt`, `Settings`
- Optional: `ItemNames`, `CharacterResult`, `FieldResult`, `StiltzkinResult`,
  `ShopResult`, `SynthesisResult`, `GemsResult`, `InitialItemsResult`
- Flags: `EnemyDropsRemapped`, `EnemyBlueMagicShuffled`, `EnemyCardDropsShuffled`,
  `TetraMasterRandomizationApplied`

**`SpoilerLogWriter`** — writes `Spoiler-Seed-[seedInt].txt`:
- Sections: Header → Characters → Field Items → Stiltzkin → Shops →
  Synthesis → Ability Gems → Starting Items → Enemies → Tetramaster → Settings Summary
- Item IDs resolved to names via `ItemNames` dictionary (falls back to "Item #N")
- `CleanAbilityComment()` — strips `;# NNN - ` prefix from AbilityGemsRow.Comment
- Timestamp-bearing sections excluded from determinism comparison

---

### Task 8 — `RandomizerEngine.cs`

Full pipeline orchestrator. `static Task<GenerationResult> RunAsync(Settings, IProgress?, CancellationToken)`.

**Game data path structure (confirmed against actual install):**
```
[gamePath]/StreamingAssets/Data/Characters/          ← BaseStats, CharacterParameters,
                                                       CommandSets, DefaultEquipment
[gamePath]/StreamingAssets/Data/Characters/Abilities/ ← AbilityGems + all per-char CSVs
[gamePath]/StreamingAssets/Data/Items/               ← Items, ShopItems, Stats, Synthesis, etc.
```

**Pipeline steps (20 total):**
1. Seed resolution + `SeedEngine.CreateRandom`
2. Mod output folder creation
3. Read all game CSVs (Characters/, Characters/Abilities/, Items/)
4. Build item name map from Items.csv inline comments
5. Item remap table (passthrough — global item shuffle not yet implemented)
6. Enemy binary patches (`RandomizeEnemies` gate)
7. Field binary patches (`FieldItemRandomizer.FromGameRoot`)
8. Stiltzkin patches (`StiltzkinRandomizer.Randomize`)
9. Character pipeline (`CharacterRandomizer` → `AbilityApRandomizer`)
10. Starting items (`InitialItemsRandomizer`)
11. Gear stats (`GearStatRandomizer`)
12. Shops (`ShopRandomizer`)
13. Synthesis (`SynthesisRandomizer`)
14. Ability gems (`AbilityGemsRandomizer`)
15. Tetramaster (`TetraMasterRandomizer`)
16. Recommended Logic Engine (CorrectEquipmentCoherence, EnforceLegendaryRarity)
17. Write all modified CSVs
18. Spoiler log
19. Metadata (Settings JSON, ModDescription.xml, mod-local Memoria.ini)
20. Update base Memoria.ini load order

**Two `NotImplementedException` stubs — implement before Phase 9 testing:**
- `LoadEnemyFiles(gamePath)` — use BattleItemScanner/EnemyFile API on p0data2.bin
- `ExtractTetraMasterData(gamePath)` — use TetraMasterFile read methods

**One TODO comment:**
- `WireEnforceSynthesisReachability` — requires VanillaItemCatalog.Build()

---

### Tasks 9–10 — UI Wiring + Polish

**GenerateCommand wired:**
- Stub (500ms delay) replaced with `RandomizerEngine.RunAsync(settings, progress)`
- `Progress<(double, string)>` captures WPF sync context — UI-safe without Dispatcher
- Result displayed in StatusText: `"Done! Seed folder: [path]"` or failure messages
- `using StiltzkinsBag.Core` + `using System.Threading` added to MainViewModel

**BuildSettings() fixed:**
- `RandomizeCharacters` and `RandomizeEnemies` now derived from sub-options AND gated by
  their master toggle: `RandomizeEnemies && (RandomizeItemDrops || ...)`
- All sub-options gated by master in Settings: `RandomizeBaseStats = RandomizeCharacters && RandomizeBaseStats`
- Fixes silent bug where UI master toggle off still passed sub-options as true to engine

**UI additions:**
- "Get Memoria Engine ↗" button — `Visibility` bound to `IsMemoriaPromptVisible`
  (`IsGamePathValid && !IsMemoriaDetected`), orange colour, opens GitHub in browser
- "Remove from Load Order" button — below GENERATE, calls `MemoriaLoadOrder.RemoveAllSbEntries`
- `OnPropertyChanged(nameof(IsMemoriaPromptVisible))` added to `ValidateGamePath()`

---

### Task 11 — Determinism Integration Test

**`DeterminismIntegrationTests.cs`** (`StiltzkinsBag.Tests/Integration/`):
- Reads game path from `FF9_GAME_PATH` env var; skips silently when absent
- `SameSeedAndSettings_CsvOutputByteIdentical_SequentialCapture` — authoritative test:
  Run 1 → snapshot all comparable output files → Run 2 → compare byte-for-byte
- Excluded from comparison: `ModDescription.xml` (timestamp), `Spoiler-Seed-N.txt` (timestamp)
- Backup/restore `Memoria.ini` around each run
- Cleanup: seed folder deleted after test
- Features enabled: BaseStats, Equipment, GearStats, Shops, Synthesis, AbilityGems
- Features disabled: Enemies, TreasureChests, Tetramaster (stub methods)
- **Result: PASS — two runs produced byte-identical CSV output**

---

## Milestone

✅ Generated mod folder loads in Memoria for CSV-based features.
✅ Spoiler log accurately reflects all changes.
✅ Same seed + settings → byte-identical output confirmed by automated test.

---

## Known Issues Discovered This Phase

| Issue | Status |
|-------|--------|
| `FieldParser` throws `ArgumentException` on small field files (< expected size) | Deferred to Phase 9 — defensive skip in `FieldItemRandomizer` |
| `DefaultEquipment.csv` not written to mod output | Deferred to Phase 9 — evaluate if needed (DefaultEquipmentSet pointer shuffled, not the set contents) |

---

## Deviations

| Item | Plan | Actual | Reason |
|------|------|--------|--------|
| `FINAL FANTASY IX_Data` intermediate folder | Assumed from FieldItemRandomizer constant | Does not exist in developer's install | Install structure varies; fixed `StreamingAssetsPath` to not include this folder |
| `AbilityGems.csv` location | Assumed `Items/` | `Characters/Abilities/` | Discovered during integration test run |
| Per-character ability CSVs | Assumed `Characters/` | `Characters/Abilities/` | Same discovery; added `AbilitiesRelPath` constant and `AbilitiesCsvPath` helper |
| `AbilityFeatures.txt` location | Assumed `Characters/` | `Characters/Abilities/` | Confirmed by developer |
| `BooleanToVisibilityConverter` key | Assumed MaterialDesign default | App registered as `BoolToVis` | Existing XAML resources used a custom key name |
| Master checkbox gate in BuildSettings | Not implemented | Required — sub-options passed as true even when master off | UI `IsEnabled` only disables the control visually; does not uncheck it |
| SpoilerLog first redundant test | Kept for design explanation | Should be deleted | `SameSeedAndSettings_ProducesByteIdenticalCsvOutput_AcrossTwoRuns` — delete in Phase 9 cleanup |

---

## Parking Lot

### Phase 9 — Implementation Required

| Item | Notes |
|------|-------|
| `LoadEnemyFiles` stub | Implement using BattleItemScanner + EnemyFile API on p0data2.bin. Sort by SourcePath (Ordinal). |
| `ExtractTetraMasterData` stub | Implement using TetraMasterFile read methods. Source: resources.assets. |
| `WireEnforceSynthesisReachability` | Requires VanillaItemCatalog.Build() threaded through engine. See TODO comment in RandomizerEngine step 16c. |
| `FieldParser` defensive skip | Wrap `FindItemLocations` call in try/catch `ArgumentException` inside `FieldItemRandomizer.Randomize()`. Skip file, log warning, continue. |
| `DefaultEquipment.csv` write | Evaluate: DefaultEquipmentSet pointer (CharacterParameters) IS written. Actual slot contents (DefaultEquipment) are NOT. Decide if mod needs to include the file. |
| Delete redundant determinism test | `SameSeedAndSettings_ProducesByteIdenticalCsvOutput_AcrossTwoRuns` — the sequential capture test supersedes it. |

### Carried from Prior Phases

| Item | Notes |
|------|-------|
| Multi-locale field patching | 144 field scripts differ in size across locales. Current: US bytes written to all 7. |
| EnemyCatalogTests JSON rewrite | Still references StockEnemyBytesJsonNoZeros.json |
| ModSourceResolverTests require Memoria.ini | Several tests fail without Memoria-enabled install |
| ShopRandomizer Price > 2 filter | Replace with ItemPool.ShopFriendly. Deferred since Phase 4. |
| LegendaryItemList field-dependent classifications | UNVERIFIED — re-audit after FieldParser false-positive fix |
| Preset tuning | Preset values not playtested. Recommended preset especially needs review after first real play session. |

---

## Rules Added This Phase

**Rule: Always verify game file structure with the developer before assuming directory layout.**
The `FINAL FANTASY IX_Data` intermediate folder and the `Characters/Abilities/` subfolder for
ability CSVs were both wrong assumptions derived from source code constants. Ask the developer
to run a directory listing before implementing any file path helper.

**Rule: UI master toggles gate sub-option visibility only — they do not prevent sub-options from
passing their state to BuildSettings().**
In `BuildSettings()`, every sub-option that has a master toggle must be explicitly gated:
`RandomizeItemDrops = RandomizeEnemies && RandomizeItemDrops`. Never assume that a disabled
checkbox passes false — it retains whatever value it was last set to.

---

## Test Count

| Phase | Tests |
|-------|-------|
| Phase 7 end | 819 |
| Phase 8 Task 6 (OutputWriterTests) | +26 → 845 |
| Phase 8 Task 11 (DeterminismIntegrationTests) | +2 → **847** |

**Final: 847 — all green, 0 warnings**

---

## Commit Message

```
feat: Phase 8 complete — RandomizerEngine pipeline + mod output + Memoria integration

Output infrastructure (StiltzkinsBag.Core/Output/):
- ModOutputWriter: seed folder creation, CSV + binary write helpers
  Subfolder constants: Characters/, Characters/Abilities/, Items/, Battle/
- MemoriaLoadOrder: FolderNames parse/inject/remove; custom line-by-line INI parser
- ModDescriptionWriter: ModDescription.xml (author, version, seed, mode, timestamp)
- ModMemoriaIniWriter: mod-local Memoria.ini; TripleTriad=0 gate for Tetramaster
- SettingsFileWriter: Settings-Seed-N.json via Settings.ToJson()
- SpoilerLogData: result bundle (all randomizer outputs + item names + flags)
- SpoilerLogWriter: Spoiler-Seed-N.txt; 10 sections; item name resolution

RandomizerEngine.cs (StiltzkinsBag.Core/):
- static RunAsync(Settings, IProgress?, CancellationToken) → Task<GenerationResult>
- 20-step pipeline; correct RNG call order throughout
- Reads from confirmed path structure: StreamingAssets/Data/Characters/Abilities/
- 2 NotImplementedException stubs: LoadEnemyFiles, ExtractTetraMasterData
- CorrectEquipmentCoherence + EnforceLegendaryRarity wired (Recommended mode)
- EnforceSynthesisReachability deferred (requires VanillaItemCatalog)

MainViewModel.cs (StiltzkinsBag.App/):
- GenerateCommand stub replaced with RandomizerEngine.RunAsync
- Progress<(double, string)> wires fraction + message to UI
- BuildSettings(): RandomizeCharacters/RandomizeEnemies derived AND gated by master toggles
- All sub-options gated by their master in Settings output
- IsMemoriaPromptVisible computed property + ValidateGamePath notification
- OpenMemoriaLinkCommand: opens https://github.com/Albeoris/Memoria
- RemoveFromLoadOrderCommand: MemoriaLoadOrder.RemoveAllSbEntries

MainWindow.xaml:
- "Get Memoria Engine ↗" button (orange, Visibility=BoolToVis, IsMemoriaPromptVisible)
- "Remove from Load Order" button (below GENERATE, IsEnabled=IsGamePathValid)

OutputWriterTests.cs (StiltzkinsBag.Tests/Output/): 26 new tests
DeterminismIntegrationTests.cs (StiltzkinsBag.Tests/Integration/): 2 new tests
- Reads FF9_GAME_PATH env var; skips when absent
- Two-run byte-identical CSV comparison confirmed passing

Path discoveries:
- No FINAL FANTASY IX_Data intermediate folder in developer's install
- AbilityGems.csv + per-char CSVs in Characters/Abilities/ not Characters/ or Items/
- AbilityFeatures.txt in Characters/Abilities/

2 new rules added. 819 → 847 tests (+28).
```

---

## PhaseEnd Changelog

```
v1.7.0 → v1.8.0
- Build Log: Phase 8 entry added
- Key Additions: RandomizerEngine, all output writers, SpoilerLog, wired GenerateCommand,
  UI polish (Memoria prompt, Remove from Load Order), determinism test passing
- Key Discoveries: No FINAL FANTASY IX_Data folder, Characters/Abilities/ subfolder,
  UI master toggle gate bug in BuildSettings
- Stubs: LoadEnemyFiles, ExtractTetraMasterData (Phase 9)
- Deviations: path assumptions wrong ×3, BoolToVis key, master toggle gate missing
- Rules: 2 new rules added
- Parking Lot: 2 stubs, FieldParser defensive skip, DefaultEquipment evaluation,
  EnforceSynthesisReachability, redundant test cleanup, all Phase 5.9.3 carries
- Phase 8 marked complete
- Current phase: 9 — Polish, Debug Tooling, Edge Cases
```

---

## 🛑 Stop Here

This chat session is complete. Do the following before starting Phase 9:

1. Add `PhaseEnd_Phase8.md` to your Claude Project
2. Start a new chat session
3. Say: "Continue building Stiltzkin's Bag. Read all attached markdown files."

Do not continue development in this session. Do not delete this chat — keep it
for posterity and back-reference. Large chats in a project will not slow down
future sessions or use more tokens.
