# Phase 9.1 — PhaseEnd

**Completed:** 2026-04-05
**Tests at phase end:** 848 total — all green, 0 warnings
**Next phase:** 9.2 — Full Field Script AST Decode/Encode

---

## Phase 9.1 Task Checklist

| # | Task | Status |
|---|------|--------|
| 1 | `LoadEnemyFiles` (RandomizerEngine stub) | ✅ Done |
| 2 | `ExtractTetraMasterData` (RandomizerEngine stub) + WriteTetraMasterOutput path fix | ✅ Done |
| 3 | `FieldParser` defensive skip in `FieldItemRandomizer` | ✅ Done |
| 4 | `FieldScriptOpcodeTable.cs` — full HW opcode table port | ✅ Done |
| 5 | `FieldParser.cs` — complete sequential rewrite | ✅ Done |
| 6 | Switch opcode fix — `0x0B`/`0x06` computed advance instead of stop | ✅ Done |
| 7 | VarOp skip table fix — `GetItemCount` (0x64) = 0 bytes, not 1 | ✅ Done |
| 8 | `ModSourceResolver` — `ResolveByPath` + `ResolveEmbeddedAsset` | ✅ Done |
| 9 | `FieldItemScanner` — `FieldFileScanResult` + `ScanArchiveDetailed` | ✅ Done |
| 10 | `FieldScannerDiagnosticTests.cs` — new diagnostic | ✅ Done |
| 11 | `IndirectItemGiveDiagnosticTests.cs` — new diagnostic | ✅ Done |
| 12 | `Diagnostic_DumpItemZeroFalsePositiveLocations` test assertion fix | ✅ Done |
| — | All original Phase 9 tasks (4–16) | ⏩ Deferred → Phase 9.5 |

---

## What Was Built

### Task 1 — `LoadEnemyFiles` (RandomizerEngine)

Replaced `NotImplementedException` stub. Uses vanilla `p0data2.bin` for path enumeration
(`GetFullPaths()` filtered to `battlescene/*/dbfile0000.raw16.bytes`, sorted Ordinal), then
resolves each file through the `ModSourceResolver` mod stack.

Added `ResolveByPath(string archiveRelPath)` to `ModSourceResolver` — resolves a full-path
asset by walking the mod stack, falling back to vanilla archive if not overridden.

**Output:** `ModSourceResolver.cs` (modified), `RandomizerEngine_LoadEnemyFiles.cs`

---

### Task 2 — `ExtractTetraMasterData` + WriteTetraMasterOutput path fix

Replaced `NotImplementedException` stub. `resources.assets` confirmed at
`x64/FF9_Data/resources.assets` (not under `StreamingAssets`).

Added `ResolveEmbeddedAsset(archiveRelPath, assetFullPath, shortName?)` to
`ModSourceResolver`. Three binary files use `Extract(shortName)` (unique short names).
`minista.mes` uses `ExtractByPath` (7 language copies, not unique by short name).

**WriteTetraMasterOutput path bug fixed:**
- Was: `StreamingAssets/assets/resources/cardgame/...`
- Corrected to: `embeddedasset/quadmist/...` and `embeddedasset/text/{lang}/etc/minista.mes`

**Output:** `ModSourceResolver.cs` (full replacement with all 3 resolve methods)

---

### Task 3 — FieldParser Defensive Skip

Wrapped `FieldParser.FindItemLocations(bytes)` in `try/catch ArgumentException` in
`FieldItemRandomizer.Randomize()` Pass 1 loop. Skipped files added to `SkippedFields`
list on `FieldRandomizationResult`. `ScanAll()` (public static, used by tests) deliberately
does NOT get the skip — lets tests assert on parser behavior.

**Output:** `FieldItemRandomizer.cs`

---

### Tasks 4–7 — Sequential FieldParser Rewrite (major)

#### Task 4 — `FieldScriptOpcodeTable.cs` (NEW file)

Complete port of `HADES_STRING_SCRIPT_OPCODE` table and `VarOpList` from Hades Workshop's
`Database_Script.cpp`. Full coverage of opcodes `0x00`–`0x111`. Two components:

**`OpcodeInfo` struct:** `UseVarArg` (bool), `ArgLengths` (int[]), `IsSwitch` (bool).
- `UseVarArg=true`: a vararg_flag byte precedes arguments. Bit N=0 → read `ArgLengths[N]`
  bytes; bit N=1 → skip VarOp expression.
- `UseVarArg=false`: each arg uses `ArgLengths[i]` directly. 0 = VarOp expression.
- `IsSwitch=true`: variable-length opcode with no computable extent — stop scan.

**VarOp skip table:** `byte token → int extraBytes`. Maps each VarOp token to the number
of inline data bytes that follow it. Key values:
- `0x7D` (const short) → 2 bytes
- `0x7E` (const long) → 4 bytes
- `0x7F` (terminate) → handled by caller, not this table
- `0xC0–0xDF` (1-byte array var refs) → 1 byte
- `0xE0–0xFF` (2-byte array var refs) → 2 bytes
- `0x64` (GetItemCount) → **0 bytes** (stack-based, corrected from initial value of 1)
- `0x79` (SV_), `0x7A` (GetData_), `0x78` (GetEntryProperty) → 1 byte

#### Task 5 — `FieldParser.cs` — Complete Rewrite

**Root causes eliminated:**

1. **AddItem count arg was 2 bytes, should be 1.** HW opcode table: `{ 0x48, use_vararg=true,
   ArgLengths=[2, 1] }` — item=uint16, count=**uint8**. Old parser read count as uint16,
   consuming the next opcode byte as part of count, corrupting all subsequent positions.

2. **Pattern matching on 0x48 fired inside argument data.** Any opcode whose constant
   arguments contained the byte `0x48` produced a false AddItem detection. Sequential
   decoding from the opcode table eliminates this entirely.

3. **`set Treasure_Item = X` pattern needed `[2C][7F]` suffix verification.**
   Without checking for assignment operator `[2C]` and terminate `[7F]`, read expressions
   (`if Treasure_Item == X`) also matched, causing false TreasureItem recordings and
   position corruption.

**Key design:** `ScanFunction` walks bytecode sequentially. Each opcode is looked up in
`FieldScriptOpcodeTable`. Arguments are consumed per `UseVarArg`/`ArgLengths`. VarOp
expressions are skipped using `SkipVarOpExpression` (token-by-token per skip table).
Functions containing `IsSwitch` opcodes stop at that point — items before the stop
are still reported.

#### Task 6 — Switch Opcode Computed Advance

`JMP_SWITCH (0x0B)` and `JMP_SWITCHEX (0x06)` changed from `Sw()` (stop scan) to `F()`
(no args) in the opcode table, with explicit handling in `ScanFunction`:

- **`0x0B JMP_SWITCH`:** `[N][2-byte start][2-byte default][N × 2-byte jump]`
  → advance `5 + N*2` bytes, **continue** scanning.
  Confirmed from field 656 (Marsh/Pond): `0B 08 00 00 65 00 [16 bytes]` ✓

- **`0x06 JMP_SWITCHEX`:** `[N][2-byte default][N × 4-byte (case+jump)]`
  → advance `3 + N*4` bytes, **continue** scanning.
  Confirmed from field 656: `06 04 60 02 [16 bytes]` ✓

**Impact:** Gastro Fork (item 84) now found correctly in all 4 paired marsh scripts
(fields 656–659). Scanner no longer stops at switch dispatch in field functions.

`0x0D` (unknown switch variant with unconfirmed format) and `0x29` (SetRegion, variable
polygon vertex count) remain as `Sw()` — stop scan. Deferred to Phase 9.2.

#### Task 7 — VarOp Skip Table Fix

`table[0x64]` (GetItemCount) corrected from `1` to `0`.

**Evidence from field 656 byte sequence** `05 7D 54 00 64 7D 63 00 18 7F`:
- `7D 54 00` = const short 84 pushed onto stack BEFORE `64` (GetItemCount token)
- `64` = GetItemCount — pops argument from stack, 0 inline bytes
- `7D 63 00` = const short 99

GetItemCount is stack-based: the item ID argument is pushed by the preceding const-short,
not encoded inline after the token. The previous value of 1 consumed the next token byte
(`7D` = const short) as a spurious extra byte. In practice this often cancelled out (both
options happened to terminate at `7F` in the specific sequences encountered), but the
misidentification was semantically wrong and could corrupt scanning in other expressions.

---

### Task 8 — ModSourceResolver New Methods

Two new resolve methods added to `ModSourceResolver.cs`:

**`ResolveByPath(string archiveRelPath)`** — resolves a full-path archive asset through
the mod stack. Used by `LoadEnemyFiles` for battle stat files that share a short name.

**`ResolveEmbeddedAsset(string archiveRelPath, string assetFullPath, string? shortName)`**
— resolves a Unity `resources.assets` embedded asset. When `shortName` is provided and
unique, uses `Extract(shortName)`. When `shortName` is null (e.g., `minista.mes` with
7 language copies), uses `ExtractByPath(assetFullPath)`.

---

### Tasks 9–12 — Diagnostics and Test Fixes

**`FieldItemScanner.cs` additions:**
- `FieldFileScanResult` record: `FileName`, `TotalLocations`, `DirectItemCount`,
  `TreasureItemCount`, `TextSyncCount`, `DirectGilCount`, `Locations`
- `ScanArchiveDetailed(string archivePath)` → `IReadOnlyList<FieldFileScanResult>`
  (used by diagnostic tests, not by production randomizer)

**`FieldScannerDiagnosticTests.cs`** (new): Writes `TestData/Diagnostics/FieldScanDiagnostic.txt`.
Per-file location breakdown, per-item counts across all fields.

**`IndirectItemGiveDiagnosticTests.cs`** (new): Scans for:
- Pattern A: variable-arg AddItem (item ID is variable, not constant)
- Pattern B: local variable assignments with item-range values 1–255
- Pattern C: function scan stops (by reason)
- Cross-reference: files with both Pattern A and Pattern B (indirect item give candidates)

**`Diagnostic_DumpItemZeroFalsePositiveLocations` test fix:**
Old assertion `Assert.True(totalHits >= 1)` was written against the old parser behavior
(85+ false positives). New sequential parser correctly finds 0 item-0 false positives.
Assertion changed to `Assert.True(File.Exists(outputPath))`.

---

## Scanner Status at Phase 9.1 End

### Remaining Scan Stops

| Reason | Count | Status |
|--------|-------|--------|
| `0x29` SetRegion | 2226 | Deferred → Phase 9.2 |
| `0x0D` unknown switch | 280 | Deferred → Phase 9.2 |
| Extended opcodes (0x12C, 0x1FF, 0x160, 0x130, etc.) | ~85 | Deferred → Phase 9.2 |

**Note on SetRegion:** Initial assessment that this was "unfixable" was incorrect. HW
decodes SetRegion correctly from the bytecode. The polygon vertex count IS deterministic
from adjacent field structure data in the archive — it is not runtime-variable. Fix requires
Phase 9.2 full field data access.

### Item Catalog Accuracy (Confirmed Issues)

| Item | Scanner | Catalog | Issue |
|------|---------|---------|-------|
| Gastro Fork (84) | 1 per field × 4 fields | 4 ✅ | Correct after switch fix |
| Whale Whisker (63) | 5 found | 0 ❌ | Catalog feed broken — deferred 9.2 |
| Mace of Zeus | false positives + miss | 5 ❌ | Over-counts AND misses real location |
| Ultima Weapon (worldwide) | correct | 1 (WorldMap) ✅ | Correct |

**Conclusion:** Catalog cannot be trusted as authoritative. Phase 9.2 full decode
is required before the catalog can drive randomizer decisions.

### Pattern B Investigation Result

Pattern B (indirect item gives via local variable) was investigated and found to be
**fundamentally noisy without data-flow correlation.** Any integer 1–255 assigned to any
variable produces a false hit — animation IDs, coordinates, model codes, flag values all
use the same bytecode pattern. The only correct approach is:

1. Scan for `set VAR_X = N` where N is 1–255
2. Continue scanning the **same function only**
3. Look for `AddItem(VAR_X, count)` where the item arg is VAR_X
4. Only if matched in the same function → real indirect item give

This requires full function AST representation. Deferred to Phase 9.2.

---

## Deferred Tasks

### → Phase 9.2 (Full Field Script AST Decode/Encode)

Full port of HW field script compiler to C#. Goal: zero scan stops, trustworthy catalog.

| Task | Notes |
|------|-------|
| Full opcode decode/encode for all opcodes | Port `Database_Script.cpp` logic completely |
| SetRegion (0x29) | Polygon vertex count from adjacent field structure data in archive |
| `0x0D` switch variant | Format: likely `[N-lo][N-hi][2-byte start][2-byte default][N×2-byte jump]` — needs HW source confirmation |
| Extended opcodes 0x12C–0x1FF | ~85 stops across ~29 distinct opcode IDs |
| Pattern B variable tracking | Assign + AddItem correlation in same function |
| Whale Whisker catalog feed | Root cause: catalog build does not see field scanner results correctly |
| Mace of Zeus accurate count | Requires zero scan stops before this is trustworthy |
| Field entrance randomization | Requires full AST — Gen2 feature unlocked by 9.2 |
| Scenario counter edits | Requires full AST — Gen2 feature unlocked by 9.2 |

### → Phase 9.5 (Original Phase 9 Tasks)

All original Phase 9 tasks deferred, pending Phase 9.2 accurate catalog.

| Task | Notes |
|------|-------|
| 4 | `WireEnforceSynthesisReachability` — needs `VanillaItemCatalog.Build()` + accurate `itemIdToMinFieldId` |
| 5 | `StartingEquipmentRandomizer` — MatchNewGear / Random / RichStart / None sub-options |
| 6 | Delete redundant `SameSeedAndSettings_ProducesByteIdenticalCsvOutput_AcrossTwoRuns` test |
| 9 | Structured error messages + edge case handling |
| 10 | `LegendaryItemList` re-audit — needs accurate scanner |
| 11 | Boundary value verification |
| 12 | Final determinism regression (10 seeds × all feature combinations) |
| 13 | `DebugWindow.xaml` |
| 14 | Logging for debug builds |
| 15 | Preset tuning |
| 16 | Pattern B variable tracking (same-function assign+AddItem correlation) |

---

## Key Discoveries This Phase

| Discovery | Impact |
|-----------|--------|
| AddItem (0x48) count arg is uint8 (1 byte), not uint16 | Old parser consumed 1 byte too many, corrupting all subsequent opcode positions |
| Pattern matching for 0x48 is fundamentally broken | Sequential decoding from HW opcode table is the only correct approach |
| `set Treasure_Item = X` needs `[2C][7F]` suffix verification | Without it, read expressions also match → false TreasureItem recordings and position corruption |
| GetItemCount (0x64) VarOp token is stack-based — 0 inline bytes | Item ID is pushed onto stack BEFORE the token, not encoded after it. Table had wrong value of 1. |
| JMP_SWITCH/JMP_SWITCHEX have computable extents | Gastro Fork now found after switch fix. N cases encoded as 1-byte count immediately after opcode. |
| SetRegion (0x29) IS computable, not "unfixable" | Polygon vertex count is static in field structure data. Initial assessment was wrong. Deferred to 9.2. |
| resources.assets at `x64/FF9_Data/resources.assets` | Not under StreamingAssets. Confirmed from developer directory listing. |
| WriteTetraMasterOutput paths were wrong | Was `StreamingAssets/assets/resources/cardgame/`. Corrected to `embeddedasset/quadmist/` and `embeddedasset/text/{lang}/etc/`. |
| Pattern B variable tracking requires same-function data-flow correlation | Value range 1–255 alone is useless — animation IDs, coordinates, flags all overlap |
| Field 1758 = EVT_EVA2_IF_PUG_1 (Iifa Tree/Tree Roots) | Confirmed via `Database_Steam.cpp`: `{ 1758, 6, "EVT_EVA2_IF_PUG_1", ... }` |
| Field 656 = EVT_KUINA_KM_SWP_0 (Marsh/Pond) | Confirmed Quale reward field. AddItem(84,1) is a CONSTANT call, not variable. |
| Whale Whisker (63) catalog feed broken | Scanner finds 5, catalog shows 0. Root cause unknown — deferred to 9.2. |
| Mace of Zeus catalog unreliable | Simultaneously over-counts (false positives) and misses real location |

---

## Rules Added This Phase

**Rule: Field script scanning must use sequential opcode decoding from the HW opcode table.**
Pattern matching on opcode bytes produces false positives whenever those bytes appear as
argument data in unrelated opcodes. The only correct approach is sequential decoding using
`FieldScriptOpcodeTable` (ported from HW's `Database_Script.cpp`).

**Rule: AddItem (0x48) count argument is uint8 (1 byte), not uint16.**
HW opcode table: `{ 0x48, use_vararg=true, ArgLengths=[2, 1] }` — item=uint16, count=uint8.
Never read count as 2 bytes.

**Rule: `set Treasure_Item = X` pattern requires `[2C][7F]` suffix to distinguish write from read.**
Without the assignment operator `[2C]` and terminate `[7F]` as required suffix, read
expressions containing Treasure_Item also match, producing false TreasureItem recordings.

**Rule: JMP_SWITCH (0x0B) and JMP_SWITCHEX (0x06) have computable byte extents.**
Read N (1 byte), compute: `0x0B` → advance `5 + N*2` bytes; `0x06` → advance `3 + N*4`
bytes. Continue scanning. Do NOT stop. Confirmed from field 656 hex.

**Rule: GetItemCount VarOp token (0x64) is stack-based — 0 inline bytes.**
The item ID argument is pushed onto the VarOp stack before the `0x64` token, not encoded
inline after it. VarOp skip table entry must be 0.

**Rule: TetraMaster mod output paths are `embeddedasset/quadmist/{filename}` and
`embeddedasset/text/{lang}/etc/minista.mes`.**
NOT `StreamingAssets/assets/resources/cardgame/`. Confirmed from Phase 5.7 PhaseEnd rules
and developer install. `resources.assets` is at `x64/FF9_Data/resources.assets`.

**Rule: `resources.assets` is at `x64/FF9_Data/resources.assets`, not under `StreamingAssets`.**
Confirmed from developer directory listing (Phase 9.1). `ModSourceResolver.ResolveEmbeddedAsset`
uses this path. Do not assume StreamingAssets contains this file.

**Rule: Pattern B indirect item give scanning requires same-function data-flow correlation.**
Scanning for `set VAR_X = N` (N in range 1–255) alone is meaningless — animation IDs,
coordinates, model codes, flag values all use identical bytecode patterns. Valid approach:
(1) detect `set VAR_X = N` in a function, (2) log variable ID X, (3) continue scanning
that same function only, (4) look for `AddItem(VAR_X, count)` where the item arg is VAR_X.
Only a matched pair in the same function is a real indirect item give.

---

## Files Produced This Phase

| File | Location | Status |
|------|----------|--------|
| `ModSourceResolver.cs` | `StiltzkinsBag.Core/Parsing/` | Modified — added `ResolveByPath`, `ResolveEmbeddedAsset` |
| `FieldItemRandomizer.cs` | `StiltzkinsBag.Randomizers/` | Modified — defensive skip, `SkippedFields` property |
| `FieldScriptOpcodeTable.cs` | `StiltzkinsBag.Core/Parsing/` | **NEW** — full HW opcode table + VarOp skip table |
| `FieldParser.cs` | `StiltzkinsBag.Core/Parsing/` | Complete rewrite — sequential decoding, switch computed advance |
| `FieldItemScanner.cs` | `StiltzkinsBag.Core/Models/` | Modified — `FieldFileScanResult`, `ScanArchiveDetailed` |
| `RandomizerEngine.cs` | `StiltzkinsBag.Core/` | Modified — `LoadEnemyFiles`, `ExtractTetraMasterData`, `WriteTetraMasterOutput` paths fixed |
| `FieldScannerDiagnosticTests.cs` | `StiltzkinsBag.Tests/Diagnostics/` | **NEW** |
| `IndirectItemGiveDiagnosticTests.cs` | `StiltzkinsBag.Tests/Diagnostics/` | **NEW** |
| `VanillaItemCatalogTests.cs` | `StiltzkinsBag.Tests/` | Modified — item-0 assertion fixed |

---

## Test Count

| Milestone | Tests |
|-----------|-------|
| Phase 8 end | 847 |
| Phase 9.1 — item-0 assertion fix + new diagnostic | **848** |

**Final: 848 — all green, 0 warnings**

---

## Commit Message

```
feat: Phase 9.1 — FieldParser sequential rewrite, switch fix, RandomizerEngine stubs

FieldScriptOpcodeTable.cs (StiltzkinsBag.Core/Parsing/) — NEW:
- Full port of HADES_STRING_SCRIPT_OPCODE from HW Database_Script.cpp
- Covers opcodes 0x00-0x111 with UseVarArg, ArgLengths, IsSwitch per entry
- VarOp skip table: token byte → inline byte count (0x7D=2, 0x7E=4, 0xC0-0xDF=1,
  0xE0-0xFF=2, GetItemCount=0, SV_/GetData_=1, GetEntryProperty=1)

FieldParser.cs (StiltzkinsBag.Core/Parsing/) — complete rewrite:
- Sequential opcode decoding replaces pattern matching entirely
- Root cause 1 fixed: AddItem count arg was uint16, is uint8 per HW table
- Root cause 2 fixed: Treasure_Item pattern requires [2C][7F] suffix (write vs read)
- Root cause 3 fixed: 0x48 in arg data no longer fires false AddItem detections
- JMP_SWITCH (0x0B): advance 5+N*2 bytes, continue (was: stop)
- JMP_SWITCHEX (0x06): advance 3+N*4 bytes, continue (was: stop)
- GetItemCount VarOp (0x64): 0 inline bytes (was: 1 — stack-based, confirmed from hex)
- 0x0D and 0x29 (SetRegion) still stop — format unconfirmed / requires adjacent data

ModSourceResolver.cs:
- ResolveByPath(archiveRelPath) — mod-stack resolution for full-path assets
- ResolveEmbeddedAsset(archiveRelPath, assetFullPath, shortName?) — resources.assets

RandomizerEngine.cs:
- LoadEnemyFiles: p0data2.bin via GetFullPaths, sorted Ordinal, mod-stack resolved
- ExtractTetraMasterData: resources.assets at x64/FF9_Data/, ResolveEmbeddedAsset
- WriteTetraMasterOutput paths fixed: embeddedasset/quadmist/ not StreamingAssets/cardgame/

FieldItemRandomizer.cs: try/catch ArgumentException in Randomize() Pass 1 loop
FieldItemScanner.cs: FieldFileScanResult record + ScanArchiveDetailed()
VanillaItemCatalogTests.cs: item-0 assertion fixed (0 false positives is correct)

New diagnostic tests:
- FieldScannerDiagnosticTests.cs: per-file location breakdown
- IndirectItemGiveDiagnosticTests.cs: Pattern A/B/C + cross-reference

Scanner status: 2226 SetRegion stops + 280 0x0D stops + ~85 extended opcode stops
Catalog status: NOT trustworthy — Whale Whisker feed broken, Mace of Zeus unreliable

8 new rules added. 847 → 848 tests (+1).
Phase 9.2 scope: full AST decode/encode (SetRegion, 0x0D, extended opcodes, Pattern B).
Phase 9.5 scope: original Phase 9 tasks 4-16 (pending accurate catalog from 9.2).
```

---

## PhaseEnd Changelog

```
v1.8.0 → v1.9.0
- Build Log: Phase 9.1 entry added
- Key Additions: FieldScriptOpcodeTable (new), FieldParser rewrite (sequential),
  switch computed advance (0x0B/0x06), LoadEnemyFiles, ExtractTetraMasterData,
  ModSourceResolver new methods, FieldScannerDiagnostic, IndirectItemGiveDiagnostic
- Key Discoveries: AddItem count=1 byte, GetItemCount VarOp=0 bytes, switch extents
  computable, SetRegion IS computable (not unfixable), Pattern B needs data-flow
  correlation, resources.assets path, WriteTetraMasterOutput paths wrong
- Scanner: Gastro Fork found correctly. Whale Whisker feed broken. Mace of Zeus unreliable.
  2226+280+85 scan stops remain.
- 8 new rules added
- Deferred: Phase 9.2 (full AST decode/encode), Phase 9.5 (original Phase 9 tasks 4-16)
- Current phase: 9.2 — Full Field Script AST Decode/Encode
```

---

## 🛑 Stop Here

This chat session is complete. Do the following before starting Phase 9.2:

1. Add `PhaseEnd_Phase9_1.md` to your Claude Project
2. Start a new chat session
3. Say: "Continue building Stiltzkin's Bag. Read all attached markdown files."

Do not continue development in this session. Do not delete this chat — keep it for
posterity and back-reference. Large chats in a project will not slow down future sessions
or use more tokens.
