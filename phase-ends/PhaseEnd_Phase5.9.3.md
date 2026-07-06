# Phase 5.9.3 — PhaseEnd

**Completed:** 2026-03-31
**Tests at phase end:** 706 total — all green, 0 warnings
**Next phase:** 5.9.4 or 5.99 (StiltzkinRandomizer — all Phase 5.9 catalog tasks complete)

---

## Phase 5.9.3 Task Checklist

| # | Task | Status |
|---|------|--------|
| 3a | BattleItemScanner diagnostic | ✅ Done |
| 3b | BattleItemScanner production class | ✅ Done |
| 3c | BattleItemScanner tests + guide validation | ✅ Done |
| 3d | Wire BattleItemScanner into VanillaItemCatalog.Build() | ✅ Done |
| 7a | Stiltzkin field scan | ✅ Done |
| 7b | StiltzkinVisitLocations in VanillaObtainabilityData | ✅ Done |

---

## What Was Built

### UnityArchiver — AssetBundle full-path support (prerequisite for Task 3)

**Root cause discovered:** `ExtractByPath` was stripping the input to its leaf filename
and calling `Extract(shortName)`. Since all battle stat files share the short name
`dbfile0000.raw16`, every extraction returned the first match — wrong data for all but
the first battle file.

**Fix:** Parse the type-142 AssetBundle entry inside the archive. The AssetBundle
payload maps full asset paths (e.g.
`assets/resources/battlemap/battlescene/evt_battle_ac_e028f/dbfile0000.raw16.bytes`)
to `file_info` int64 values. Two new lookup tables are built during `Open()`:
- `_pathToInfo`: full path → file_info (from AssetBundle)
- `_infoToIndex`: file_info → entry index (built from all entries)

`ExtractByPath` now normalizes the input path (strips `\StreamingAssets\`, converts
backslashes) and looks up via `_pathToInfo → _infoToIndex → ExtractByIndex`.

New public API:
- `GetFullPaths()` — returns full asset paths from the AssetBundle table
- `GetFileNames()` — unchanged, returns short embedded names (backward compat for field scripts)

**Both p0data2.bin and p0data7.bin** have AssetBundle entries and benefit from this fix.
For p0data7.bin field scripts, `Extract(shortName)` still works correctly because field
script names are unique — `ExtractByPath` with full path is also available.

---

### Task 3a — BattleItemScanner diagnostic

`BattleItemDiagnosticTests.cs` completely rewritten — all JSON catalog references removed.
Ground truth now comes exclusively from extracted `.bytes` files and `p0data2.bin` archive.

Three scan sources:
- `Diagnostic_BattleItems_FromBytesFiles` — scans `.bytes` from disk, writes `BattleList.txt`
- `Diagnostic_BattleItems_FromArchive` — scans via `p0data2.bin`, uses `ExtractByPath`
- `CrossValidation_BytesAndArchive_ProduceSameItemSets` — confirms both sources identical

Results from clean Steam install:
- 562 battle files scanned
- Drop items: 34 unique IDs
- Steal items: 106 unique IDs
- Card drops: 62 unique IDs
- Combined enemy items: 150 unique IDs
- Cross-validation: ✓ identical after UnityArchiver fix

---

### Task 3b — BattleItemScanner production class

`StiltzkinsBag.Core/Models/Battle/BattleItemScanner.cs` — new class.

Two entry points:
- `ScanArchive(archivePath)` — opens `p0data2.bin`, uses `GetFullPaths()` filtered to
  `battlescene/*/dbfile0000.raw16.bytes`, extracts each via `ExtractByPath`
- `ScanFiles(IEnumerable<(string, byte[])>)` — for tests without live archive

Returns `BattleScanResult` with:
- `DropItemIds` — non-zero drop slot item IDs across all battles
- `StealItemIds` — non-zero steal slot item IDs
- `CardDropIds` — non-zero card drop IDs (Tetramaster cards, not consumables)
- `AllEnemyItemIds` — union of drops and steals; drives `HasNormalEnemySource`

---

### Task 3c — BattleItemScannerTests

`BattleItemScannerTests.cs` — 7 tests covering:
- ScanFiles/ScanArchive return non-empty results
- No zero IDs in any result set
- AllEnemyItemIds is exactly the union of drops and steals
- Empty input → empty result
- Cross-validation: disk files and archive produce identical sets

---

### Task 3d — BattleItemScanner wired into VanillaItemCatalog.Build()

`VanillaItemCatalog.Build()` gains new optional parameter:
```csharp
BattleScanResult? battleScanResult = null
```

When provided, `HasNormalEnemySource` is computed from scanner output:
```csharp
bool normalE = battleScanResult != null
    ? (battleScanResult.AllEnemyItemIds.Contains(item.Id)
       && !VanillaObtainabilityData.BossOnlyItemIds.Contains(item.Id))
    : VanillaObtainabilityData.NormalEnemyItemIds.Contains(item.Id);
```

This replaces the incomplete hardcoded `NormalEnemyItemIds` set (31 items, guide-derived)
with the full scanner output (150 items, binary-derived), minus `BossOnlyItemIds`
(boss encounters are one-time, not repeatable).

**6 items newly infinite from scanner** (had finite-only sources before):
- 204 Rosetta Ring — also in enemy steal tables
- 209 Protect Ring — also in enemy drop tables (not just Ragtime Mouse quiz + Dead Pepper)
- 227 Diamond — also in enemy drop tables
- 228 Emerald — also in enemy drop tables
- 229 Moonstone — also in enemy drop tables
- 255 — also in enemy drop tables

All existing `NormalEnemyItemIds` remain infinite — scanner confirmed all 31.
Fallback to hardcoded set when `battleScanResult` is null — full backward compatibility.

New `VanillaItemCatalogTests` section 7 (4 tests):
- `Catalog_WithBattleScan_AllNormalEnemyItems_AreInfinite`
- `Catalog_WithBattleScan_BossOnlyItems_StayFinite`
- `Catalog_WithBattleScan_ExtendedSet_MoreItemsAreInfinite`
- `Catalog_FullBuild_WithBothArchives_ProducesCompleteCatalog`

---

### Field Scanner updates (FieldScriptExclusions)

**EVT_ALEX3_AC_SEAT_N** — "unknown field" with no field ID in the game's script system.
Confirmed via Unity Assets Viewer. Permanently excluded from all field item scans,
randomization, and diagnostics.

New `FieldScriptExclusions` static class in `FieldItemScanner.cs`:
- `UnknownFieldNoId` constant = `"EVT_ALEX3_AC_SEAT_N"`
- `IsExcluded(string name)` — handles `.eb`, `.eb.bytes`, and bare name variants

Both `ScanArchive` and `ScanFiles` apply this exclusion.

---

### FieldItemDiagnosticTests — locale comparison

New test `Diagnostic_LocaleVariants_ByteComparison` uses `GetFullPaths()` to find all
7 locale copies of each field script, extracts all, byte-compares.

Results (confirmed on clean Steam install):
- 521 scripts: byte-identical across all 7 locales ✓
- 144 scripts: size differs (mostly JP shorter — genuinely different bytecode)
- 240 scripts: content differs, header-only (diff at byte < 128, bytecode identical)
- 487 scripts: content differs, BYTECODE region (diff at byte ≥ 128)

**Key finding:** The 487 "BYTECODE_DIFFERS" files differ only in AT_TEXT string reference
IDs (embedded in bytecode but not item opcodes). Item-giving opcode positions and arguments
are identical across all locales. Cross-validation (US disk vs US archive): 0 discrepancies.

**Corrected statement** (replaces "all locales byte-identical"):
> All locales share identical item-giving opcode positions and arguments.
> AT_TEXT string reference IDs differ per locale and live in the bytecode region (≥128).

**Cross-validation fix:** `CrossValidation_FieldBytesAndArchive` now uses `ExtractByPath`
with explicit `/field/us/` and `/world/us/` paths to guarantee US locale extraction.
Previously used `Extract(shortName)` which could return JP for some scripts → 4 false
discrepancies. Now: 0 discrepancies.

---

### Task 7 — Stiltzkin field identification

**Key discovery:** Stiltzkin does NOT use `AddGil` for his package price. He uses
`RemoveGil(price)` to charge the player and `SetTextVariable(0, price)` to display the
cost in dialog. `FieldParser` parses `SetTextVariable(0, X)` as `TextSync` with
`CurrentValue = X`. Package prices (333–5555) exceed valid item IDs (0–255) — unambiguous.

Scan approach: find `TextSync` locations with `CurrentValue > 255` matching
{333, 444, 555, 666, 777, 888, 2222, 5555}.

**Confirmed mapping** (all 8 visits, 9 scripts total):

| Price | Location | Script(s) |
|-------|----------|-----------|
| 333G | Burmecia | `EVT_BURMECIA_SQUARE_1.eb` |
| 444G | Cleyra | `EVT_CLEYRA3_ANTRION.eb` + `EVT_CLEYRA3_INN.eb` *(paired)* |
| 555G | Fossil Roo | `EVT_FOSSIL_FR_DN1_0.eb` |
| 666G | Conde Petie Mtn Path | `EVT_PATA_M_CM_MP3_0.eb` |
| 777G | Alexandria (1st) | `EVT_ALEX3_AT_SENTOU.eb` |
| 888G | Oeilvert | `EVT_OEIL_UV_DEP_0.eb` |
| 2222G | Bran Bal | `EVT_BAL_BB_WPS_0.eb` |
| 5555G | Alexandria (2nd/final) | `EVT_ALEX5_AT_SENTOU.eb` |

**Cleyra 444G is a paired field** — the same Stiltzkin encounter exists in both
`EVT_CLEYRA3_ANTRION.eb` and `EVT_CLEYRA3_INN.eb`. Both must be patched identically,
same architecture as disc-variant field pairs.

**Ribbon reward:** buying all 8 packages rewards a Ribbon. This is a scripted event in
`EVT_ALEX5_AT_SENTOU.eb` (the final Alexandria visit). StiltzkinRandomizer must account
for this when deciding which items to place in packages.

`StiltzkinVisitLocations` added to `VanillaObtainabilityData`:
```csharp
public static readonly IReadOnlyDictionary<int, string[]> StiltzkinVisitLocations
```
Maps price → `string[]` of script names. Ready for Phase 5.99 StiltzkinRandomizer.

---

## File Inventory (changed this phase)

| File | Change |
|------|--------|
| `StiltzkinsBag.Core/Parsing/UnityArchiver.cs` | AssetBundle parsing; `GetFullPaths()`; `ExtractByPath` fixed; `FindIndexByShortName` rename |
| `StiltzkinsBag.Core/Models/Battle/BattleItemScanner.cs` | New — production battle item scanner |
| `StiltzkinsBag.Core/Models/FieldItemScanner.cs` | `FieldScriptExclusions` class; EVT_ALEX3_AC_SEAT_N exclusion in both entry points |
| `StiltzkinsBag.Core/Models/VanillaItemCatalog.cs` | `BattleScanResult?` parameter; scanner-driven `normalE` logic; `using Battle` |
| `StiltzkinsBag.Core/Models/VanillaObtainabilityData.cs` | `StiltzkinVisitLocations` dictionary |
| `StiltzkinsBag.Tests/BattleItemDiagnosticTests.cs` | Complete rewrite — no JSON, disk+archive only |
| `StiltzkinsBag.Tests/BattleItemScannerTests.cs` | New — 7 scanner correctness tests |
| `StiltzkinsBag.Tests/FieldItemDiagnosticTests.cs` | New — field+world diagnostic, locale comparison, cross-validation fix |
| `StiltzkinsBag.Tests/FieldItemScanner.cs` | (via Core) exclusion applied |
| `StiltzkinsBag.Tests/StiltzkinFieldDiagnosticTests.cs` | New — 2 tests; confirmed all 9 Stiltzkin scripts |
| `StiltzkinsBag.Tests/UnityArchiverTests.cs` | Rewritten for new API; `GetFullPaths()` tests; disambiguation tests |
| `StiltzkinsBag.Tests/VanillaItemCatalogTests.cs` | `P0data2Path`; `FindArchive`; `SkipIfNoP0data2`; `BuildCatalog` battle param; Section 7 (4 tests); diagnostic battle scan |

---

## Key Discoveries

1. **p0data2.bin AssetBundle** — All battle files have unique full paths in the AssetBundle
   table. `GetFileNames()` returns only short names (`dbfile0000.raw16`). Must use
   `GetFullPaths()` + `ExtractByPath` for all battle file access.

2. **Field script locale differences** — 487/1395 scripts have bytecode-region differences
   across locales. These are AT_TEXT string reference substitutions, not item opcode
   differences. Item offsets and values are locale-identical. The previous assumption
   "all locales byte-identical" was incorrect.

3. **EVT_ALEX3_AC_SEAT_N** — "unknown field" with no field ID. Must be excluded from all
   field scans and randomization.

4. **Stiltzkin identification** — Package price is found via `SetTextVariable(0, price)`
   (TextSync, CurrentValue > 255), NOT `AddGil`. `RemoveGil` charges the player.

5. **Stiltzkin 444G paired fields** — Both `EVT_CLEYRA3_ANTRION.eb` and
   `EVT_CLEYRA3_INN.eb` contain the Cleyra Stiltzkin encounter and must be patched
   identically.

6. **Battle scanner extended enemy set** — BattleItemScanner found 150 unique item IDs
   vs. 31 in the hardcoded `NormalEnemyItemIds`. The scanner is now the authoritative
   source. 6 items (Rosetta Ring, Protect Ring, Diamond, Emerald, Moonstone, item 255)
   promoted from finite to infinite by the scanner.

7. **StockEnemyBytesJsonNoZeros.json deprecated** — This was build-time reference data
   with stripped trailing zeros from an unverified source. All battle data now comes
   from clean Steam install `.bytes` files and `p0data2.bin` archive directly.

---

## Rules Added This Phase

**Rule: p0data2.bin requires ExtractByPath with full path.**
All battle stat files share the short name `dbfile0000.raw16`. `Extract(shortName)` always
returns the first match. Always use `GetFullPaths()` to enumerate battle files and
`ExtractByPath(fullPath)` to extract them. `GetFileNames()` returns short names and is
correct for p0data7.bin field scripts (which have unique short names).

**Rule: Field script locale extraction must pin to US locale.**
When extracting from p0data7.bin for item scanning or randomization patching, always use
`ExtractByPath` with an explicit `/field/us/` or `/world/us/` prefix. `Extract(shortName)`
may return a non-US locale variant for scripts where JP has different bytecode size.

**Rule: Field script locale differences are AT_TEXT only, not item opcodes.**
487 field scripts differ across locales in the bytecode region (bytes ≥ 128). These
differences are AT_TEXT string reference ID substitutions. Item-giving opcode positions
and arguments are identical across all locales. The randomizer must write patched bytes
to all locale copies, preserving each locale's AT_TEXT substitutions.

**Rule: Multi-locale field patching (parking lot — not yet implemented).**
The correct approach is: scan US locale for item locations, compute patches, then for
each locale copy extract the locale-specific bytes, apply the same byte-offset patches
(item ID bytes only), and write the result. Do NOT copy US bytes to other locales — that
destroys AT_TEXT string IDs.

**Rule: EVT_ALEX3_AC_SEAT_N is permanently excluded.**
This field has no ID in the game's script system. Exclude from all scans, randomization,
and diagnostics. Use `FieldScriptExclusions.IsExcluded(name)` everywhere.

**Rule: Stiltzkin identification uses TextSync, not AddGil.**
Stiltzkin's package price appears as `SetTextVariable(0, price)` (TextSync,
CurrentValue = price). `AddGil` and `RemoveGil` use different opcodes. Package prices
(333–5555) are unambiguous because they exceed valid item IDs (0–255).

**Rule: Stiltzkin 444G Cleyra is a paired field.**
`EVT_CLEYRA3_ANTRION.eb` and `EVT_CLEYRA3_INN.eb` both host the same Stiltzkin encounter.
Patch both identically. Same architecture as disc-variant field pairs.

---

## Test Count

| Phase | Tests |
|-------|-------|
| Phase 5.9.2 end | 672 |
| Task 3a (BattleItemDiagnosticTests rewrite) | 685 → 688 |
| UnityArchiver fix + tests | 692 |
| Task 3b/3c (BattleItemScanner + tests) | 700 |
| Task 3d (catalog wiring + VanillaItemCatalogTests section 7) | 704 |
| FieldItemDiagnosticTests | 693 (some overlap with existing) |
| StiltzkinFieldDiagnosticTests | **706** |

**Final: 706 — all green, 0 warnings**

---

## Deviations

| Item | Plan | Actual | Reason |
|------|------|--------|--------|
| ExtractByPath | Assumed leaf-name lookup worked | Required AssetBundle parsing | p0data2.bin entries all named `dbfile0000.raw16`; AssetBundle holds full paths |
| "All locales byte-identical" | Assumed from Phase 4 note | Wrong — 487 scripts differ in bytecode region | AT_TEXT string IDs are in bytecode region; item opcodes are still locale-identical |
| StockEnemyBytesJsonNoZeros.json | Used as dev catalog | Deprecated — unverified source with trimmed bytes | Clean Steam install bytes are authoritative; JSON had wrong/modded data |
| BattleItemScanner guide validation | Planned as hard assertions | Demoted — scanner is now authoritative | Scanner supersedes guide data; guide-vs-scanner comparison is now informational |
| Stiltzkin scan via AddGil | Planned approach | Wrong opcode — uses SetTextVariable + RemoveGil | Stiltzkin charges player (RemoveGil), price displayed via SetTextVariable(0, price) |
| Cleyra Stiltzkin (1 script) | Expected single hit | 2 paired scripts for 444G | Same encounter in Cleyra Entrance + Cleyra Inn — must patch both |

---

## Parking Lot

### Carry to Phase 5.9.4 / 5.99

**Multi-locale field patching (critical for non-US gameplay correctness):**
144 field scripts have different bytecode sizes across locales (mostly JP shorter).
For these scripts, the current patch approach (compute US offsets, write to all locales)
may write patches at wrong offsets for non-US locales. Correct approach:
1. Group field files by short name across all 7 locale paths
2. For each group, scan US bytes for item locations
3. For each locale copy: extract, apply patches at same byte offsets, write output
4. Do NOT copy US bytes wholesale — preserve each locale's AT_TEXT string IDs

**EnemyCatalogTests still uses JSON:**
`EnemyCatalogTests` class in `EnemyCatalogAndResolverTests.cs` still references
`StockEnemyBytesJsonNoZeros.json`. Should be rewritten to use `.bytes` files from disk.

**ModSourceResolver tests require Memoria.ini:**
Several `ModSourceResolverTests` fail without a Memoria-enabled install. Set up a test
Memoria install (vanilla + 1 mod with a few battle overrides) to properly test mod stack
resolution.

**StiltzkinRandomizer (Phase 5.99):**
All prerequisites now complete:
- 9 confirmed Stiltzkin script names (8 visits, Cleyra paired)
- Package contents from guide CSV
- Ribbon reward in final Alexandria script
- `StiltzkinVisitLocations` ready in `VanillaObtainabilityData`

**ShopRandomizer update:**
Replace `Price > 2` filter with `ItemPool.ShopFriendly`. Deferred since Phase 4.

**Boss/normal battle distinction:**
`BattleItemScanner` returns all enemy items without boss/normal classification.
`BossOnlyItemIds` is still hardcoded. Phase 6 `RecommendedLogicEngine` may benefit
from per-battle boss classification (requires parsing battle script metadata).

---

## Commit Message

```
feat: Phase 5.9.3 complete — BattleItemScanner + Stiltzkin identification

UnityArchiver:
- Parse type-142 AssetBundle entry to build full-path → file_info lookup table
- ExtractByPath now uses full path via AssetBundle, not leaf-name fallback
- GetFullPaths() returns AssetBundle paths (new); GetFileNames() unchanged (short names)
- Fixes battle file extraction: all 562 files share short name "dbfile0000.raw16"

BattleItemScanner (Task 3):
- New production class: ScanArchive() + ScanFiles() entry points
- BattleScanResult: DropItemIds, StealItemIds, CardDropIds, AllEnemyItemIds
- 150 unique item IDs found across all battle files (34 drops, 106 steals, 62 cards)
- Wired into VanillaItemCatalog.Build() via BattleScanResult? parameter
- HasNormalEnemySource now scanner-driven (not hardcoded NormalEnemyItemIds)
- 6 items promoted to infinite: Rosetta Ring, Protect Ring, Diamond, Emerald, Moonstone, 255
- BattleItemDiagnosticTests rewritten — no JSON, disk + archive only
- StockEnemyBytesJsonNoZeros.json deprecated

FieldItemScanner:
- FieldScriptExclusions: EVT_ALEX3_AC_SEAT_N permanently excluded (unknown field, no ID)
- FieldItemDiagnosticTests: locale comparison, cross-validation fix (US-locale pinning)
- Locale finding: 487 scripts differ in AT_TEXT refs (not item opcodes); item offsets identical

Stiltzkin identification (Task 7):
- Price identified via SetTextVariable(0, price) TextSync scan (NOT AddGil)
- All 8 visits confirmed: 9 scripts total (Cleyra 444G is paired — 2 scripts)
- StiltzkinVisitLocations added to VanillaObtainabilityData (price → string[])
- StiltzkinFieldDiagnosticTests: 2 tests confirming all 9 scripts

Tests: 672 → 706 (+34), all green, 0 warnings
```

---

## PhaseEnd Changelog

```
v1.5.9.2 → v1.5.9.3
- Build Log: Phase 5.9.3 entry added
- Key Discoveries: AssetBundle path table, locale AT_TEXT differences,
  EVT_ALEX3_AC_SEAT_N exclusion, Stiltzkin TextSync identification,
  Cleyra paired fields, 6 new infinite items from scanner
- Deviations: ExtractByPath, locale assumption, JSON deprecated,
  guide validation demoted, Stiltzkin opcode, Cleyra pairing
- Rules: 7 new rules added
- Parking Lot: multi-locale patching, EnemyCatalogTests, ModSourceResolver,
  StiltzkinRandomizer (5.99), ShopRandomizer, boss/normal distinction
- Phase 5.9.3 marked complete
- Current phase: 5.99 — StiltzkinRandomizer
```

---

## 🛑 Stop Here

This chat session is complete. Do the following before starting Phase 5.99:

1. Add `PhaseEnd_Phase5_9_3.md` to your Claude Project
2. Start a new chat session
3. Say: "Continue building Stiltzkin's Bag. Read all attached markdown files."

Do not continue development in this session. Do not delete this chat — keep it for
posterity and back-reference. Large chats in a project will not slow down chat or use more tokens.
