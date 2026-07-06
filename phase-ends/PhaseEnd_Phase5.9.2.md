# Phase 5.9.2 — PhaseEnd

**Completed:** 2026-03-30
**Tests at phase end:** 672 total — all green, 0 warnings
**Next phase:** 5.9.3 (BattleItemScanner + Stiltzkin field identification)

---

## Phase 5.9.2 Task Checklist

Tasks carried in from Phase 5.9.1.5 parking lot, plus refactoring tasks added during this session.

### From the 5.9.1.5 Parking Lot

| # | Task | Status |
|---|------|--------|
| Task 2 | Fix chocograph double-counting | ✅ Done |
| Task 3 | BattleItemScanner | ❌ Carried to 5.9.3 |
| Task 4 | Add 8 missing chocograph/Dead Pepper items | ✅ Done |
| Task 5 | Kupo Nut rewards | ✅ Done (prior phase) |
| Task 6 | IsMissable flag | ✅ Done (prior phase) |
| Task 7 | Stiltzkin field identification | ❌ Carried to 5.9.3 |

### Refactoring Tasks (added this session)

| # | Task | Status |
|---|------|--------|
| R1 | Fix key item bug — IsKeyItem = false always (Price ≤ 2 is not key item heuristic) | ✅ Done |
| R2 | Split fieldCount → FieldInstanceCount + WorldMapInstanceCount | ✅ Done |
| R3 | Replace boolean boss flags → BossInstanceCount (int, combines boss + friendly + Ragtime) | ✅ Done |
| R4 | Combine IsAuctionRepeatable + IsAuctionOneTime → AuctionCount (int.MaxValue / 1 / 0) | ✅ Done |
| R5 | Remove redundant flags from ItemObtainabilityEntry | ✅ Done |
| R6 | Add SynthesisInstanceCount (stamped in step 5b) | ✅ Done |
| R7 | Add WorldMapVariableItemCounts to VanillaObtainabilityData | ✅ Done |
| R8 | Redesign catalog CSV — new columns, item names, bottom summary row | ✅ Done |
| R9 | Update VanillaItemCatalog.Build() for new entry structure | ✅ Done |
| R10 | Update test suite — remove stale, add new coverage, rewrite CSV dump | ✅ Done |

---

## What Was Done

### From 5.9.1 (prior context — included for continuity)

**WorldMapVariableScanner: Convention C support**
- Added `ConventionCSequence` static field (vars 9,14,10,15,11,16,12(Int16),17)
- Updated `ScanFiles()` routing: non–world12 files now scan Convention A + Convention C blocks
- 6 new scanner tests added; total scanner tests: 28

**VanillaItemCatalog.Build(): chocograph additive counting re-enabled**
- Re-enabled `ChocographItemCounts` as a fallback, guarded by `!entry.IsFieldItem`
- Fixes Ragnarok (29) and Dragon's Claws (45) showing 0 obtainability

**VanillaItemCatalog.Build(): Step 5b — finite synthesis result counting**
- Added `max over recipes of min(available / qty_needed)` computation
- Fixes Tin Armor (176) showing 0 obtainability (Hammer×1 + Ore×∞ → 1 copy)

**VanillaObtainabilityData: ChocographItemCounts updated**
- Added Ragnarok (29) and Dragon's Claws (45) which were previously missing

---

### From 5.9.2 (this session)

**Task 2 — Fix chocograph double-counting**
- Convention C scanner covers `World_Chest` variable AddItem deliveries (Ragnarok, Dragon's Claws)
- ChocographItemCounts fallback updated: only fires when `WorldMapInstanceCount == 0 && !IsFieldItem`
- WorldMapVariableItemCounts = DeadPepperItemCounts + {29:1, 45:1} serves as runtime fallback
- ChocographItemCounts demoted to validation-reference only; no longer additive when scanner has the item

**Task 4 — Add 8 missing items**
- Items 29 (Ragnarok) and 45 (Dragon's Claws) added to ChocographItemCounts
- Items 40, 56, 113, 190, 195, 217 were already present in DeadPepperItemCounts from prior work
- All 8 items now properly accounted for across WorldMapVariableItemCounts and ChocographItemCounts

**R1 — Fix key item bug**
- Root cause: `Price ≤ 2` in Items.csv means "not purchasable from shops" (rare equipment: Save the Queen, Genji set, Ribbon), NOT FFIX key items. Real key items live in a separate data structure not yet parsed.
- `IsKeyItem` is now always `false` — placeholder until proper key item data source is integrated
- All pool tests updated to reflect the placeholder state

**R2–R6 — ItemObtainabilityEntry complete refactor**
- Removed: `IsChocographReward`, `IsDeadPepperReward`, `DeadPepperCount`, `IsAuctionRepeatable`, `IsAuctionOneTime`, `IsFriendlyMonsterReward`, `IsRagtimeMouseReward`, `IsKupoNutReward`, `HasBossOnlyEnemySource`
- Added: `BossInstanceCount` (boss + friendly + Ragtime, each ×1), `WorldMapInstanceCount`, `AuctionCount` (MaxValue/1/0), `SynthesisInstanceCount`
- Retained as pool construction helpers (not in CSV): `IsGem`, `IsEquipment`

**R7 — WorldMapVariableItemCounts**
- `WorldMapVariableItemCounts = new Dictionary(DeadPepperItemCounts) { {29,1}, {45,1} }`
- Replaces `DeadPepperItemCounts` as the fallback for `Build(worldMapCounts: null)`
- `DeadPepperItemCounts` retained as a verification/reference dictionary

**R9 — VanillaItemCatalog.Build() full refactor**
- Parameter: `deadPepperCounts` → `worldMapCounts`; fallback → `WorldMapVariableItemCounts`
- Step 3: computes `bossCount`, `worldMapCount`, `auctionCount` directly; no old flag variables
- Step 5: uses new entry properties throughout; chocograph fallback condition updated
- Step 5b: stamps `SynthesisInstanceCount` on entry after writing counts

**R8 + R10 — Catalog CSV + test suite**
- CSV new layout: `Id, Name, ObtainCount | FieldCount, WorldMapCount, EnemyCount, BossCount, InShop, SynthCount | ChocographCount, DeadPepperCount, AuctionCount | IsKeyItem, IsMissable, IsSynthesisResult, IsSynthesisIngredient`
- Item names extracted from Items.csv comment lines (`# NNN - Name`)
- Bottom row id=-1: "counts towards obtainability" markers per column
- 5 stale tests removed, 10 new tests added, 3 updated; 84 total in VanillaItemCatalogTests

---

## File Inventory (changed this phase)

| File | Change |
|------|--------|
| `StiltzkinsBag.Core/Models/ItemObtainabilityEntry.cs` | Complete rewrite — new property set |
| `StiltzkinsBag.Core/Models/VanillaItemCatalog.cs` | Build() refactored; param rename; entry construction; step 5 rewrite; step 5b stamps entry |
| `StiltzkinsBag.Core/Models/VanillaObtainabilityData.cs` | Added `WorldMapVariableItemCounts`; updated stale comments |
| `StiltzkinsBag.Core/Models/WorldMapVariableScanner.cs` | Convention C added (prior context) |
| `StiltzkinsBag.Tests/VanillaItemCatalogTests.cs` | 5 stale tests removed; 10 added; 3 updated; CSV dump rewritten |
| `StiltzkinsBag.Tests/WorldMapVariableScannerTests.cs` | Convention C tests added (prior context) |

---

## Key Design Decisions

1. **IsKeyItem = false always** — `Price ≤ 2` in Items.csv signals "not purchasable from shops" (rare equipment), not FFIX key items. Real key items (Mist, Oglop, etc.) are in a separate data structure not yet integrated. Placeholder until then.

2. **BossInstanceCount combines boss + friendly + Ragtime** — all are one-time encounters yielding at most 1 item each. The count encodes the number of such opportunities for this item.

3. **AuctionCount encoding** — `int.MaxValue` = repeatable (infinite), `1` = one-time, `0` = not at auction. This allows `HasInfiniteSource` to use a single property check.

4. **WorldMapVariableItemCounts = DeadPepper + Chocograph World_Chest** — unified world map source. The scanner (Convention A/B/C) covers all three conventions; the hardcoded dict covers items confirmed by either source.

5. **IsGem and IsEquipment retained** — needed by `ItemPool.Build()` to populate the `Gems` and `Equipment` sub-pools. Not shown in the catalog CSV export but remain in the entry as pool construction helpers.

6. **ChocographItemCounts fallback** — retained as a guide-verified cross-reference. Only fires when `WorldMapInstanceCount == 0 && !IsFieldItem`, preventing double-counting from WorldMapVariableScanner.

---

## Entry Properties Summary (after refactor)

```
Identity:     ItemId, IsKeyItem (always false — placeholder)
Category:     IsConsumable, IsGem*, IsEquipment*           (* pool helpers, not in CSV)
Infinite:     IsInShop, HasNormalEnemySource
Finite:       BossInstanceCount, WorldMapInstanceCount, AuctionCount (MaxValue/1/0)
              IsFieldItem, FieldInstanceCount, SynthesisInstanceCount
Type flags:   IsSynthesisResult, IsSynthesisIngredient
Metadata:     IsMissable
```

---

## Project Design Decisions

### Data Source Hierarchy
- Game binary files are the source of truth; guide CSVs are validation references only
- Where scanner and guide disagree — investigate; do not assume either is correct
- If a scanner finds something the guide missed — trust the scanner; if they conflict — dig into encoding/opcodes

### FieldItemScanner
- Filters to files matching `StartsWith("evt_")` — matches FieldItemRandomizer convention
- Excludes `evt_battle_*` — those are battle scripts handled by BattleItemScanner
- Item ID 0 is permanently excluded — it is a null-sentinel convention in FFIX scripts; the Hammer's single real give is tracked manually in `VanillaObtainabilityData.SentinelExcludedFieldItems`
- World map scripts (`evt_world_*`) are in p0data7.bin and are picked up by the field scanner
- Chocograph dig calls live in world map scripts — they overlap with `ChocographItemCounts`; the `WorldMapInstanceCount == 0 && !IsFieldItem` guard prevents double-counting

### p0data7.bin Archive
- `GetFileNames()` returns names WITHOUT `.bytes`; `Extract()` handles the fallback stripping
- All 7 language locale variants of each `evt_` file are byte-identical in bytecode (AT_TEXT strings differ separately); deduplicate by leaf name, scan each once

### Disc-Variant Synchronization
- Fingerprint radius is 48 bytes each side (97-byte window, item-ID byte zeroed) — validated at this width; do not reduce
- Position-61 near-miss (chest-counter byte) in ALEX1/3 and CLEYRA2/3 TreasureItem pairs is a confirmed real disc-variant gap — deferred to the synchronizer phase; do not add an exemption to the current diagnostic
- All confirmed disc-variant pairs must be patched in sync by the randomizer

### BattleItemScanner
- Reads p0data2.bin (not p0data7.bin)
- Friendly monster and Ragtime Mouse rewards come through battle scripts, not field evt_ scripts — not double-counted by FieldItemScanner

### Catalog & Item Data
- Kupo Nut rewards are finite, one per disc: Holy Bell / Elixir / Extension / Aloha T-Shirt — source CSV in `FFIX GUIDE DATA/`
- Missable items list is in `FFIX GUIDE DATA/Missable Items.txt` — source for the `IsMissable` flag
- `IsKeyItem` is always false for Phase 5 — `Price ≤ 2` in Items.csv signals "not purchasable from shops" (rare equipment), not FFIX key items; real key items are in a separate data structure not yet parsed

### Stiltzkin
- 8 visit locations are identified by Gil charge amounts: 333, 444, 555, 666, 777, 888, 2222 Gil — use FieldParser AddGil scan to find the exact `evt_` filenames
- Known field IDs: 764 (Burmecia/333G), 1105 (Cleyra/444G), 1418 (Fossil Roo/555G), 1553 (Mountain Path/666G), 1865 (Alexandria/777G), 2259 (Oeilvert/888G), 2655 (Bran Bal/2222G), 2456 (Alexandria 2nd)
- Stiltzkin random locations is a parking lot idea for a later generation feature

### Deferred Work
- Sub-option backtrack for Phase 1–5 randomizers deferred until all Phase 5.x work is done
- ShopRandomizer update deferred to Phase 6 — replace `Price > 2` filter with `ItemPool.ShopFriendly`
- Phase 5.99 = StiltzkinRandomizer (after all Phase 5.9 tasks are complete)
- Phase 6 = RecommendedLogicEngine (uses validated `VanillaItemCatalog` + `ItemPool` for copy-cap enforcement)

---

## Parking Lot — Remaining Phase 5.9 Tasks (carry to Phase 5.9.3)

### Task 3 — BattleItemScanner

Scan `p0data2.bin` battle scripts for steal/drop item IDs. Validate against guide CSVs (`Steal.csv`, `Drop.csv` or equivalent). This is the authoritative source for `FriendlyMonsterItemIds`, `RagtimeMouseItemIds`, normal/boss enemy steal/drop sets. Output should feed into `VanillaObtainabilityData` validation.

### Task 7 — Stiltzkin field identification

Scan `p0data7.bin` archive for `evt_` field scripts containing `AddGil(X)` where X ∈ {333, 444, 555, 666, 777, 888, 2222}. Resolves the 8 exact `evt_` filenames for Stiltzkin's visit locations. Output: confirmed filename-to-visit mapping, ready for StiltzkinRandomizer (Phase 5.99).

Known Stiltzkin field IDs from prior research: 764 (Burmecia/333G), 1105 (Cleyra/444G), 1418 (Fossil Roo/555G), 1553 (Mountain Path/666G), 1865 (Alexandria/777G), 2259 (Oeilvert/888G), 2655 (Bran Bal/2222G), 2456 (Alexandria 2nd).

---

## Test Count

| Phase | Tests |
|---|---|
| Phase 5.8 end | 611 |
| Phase 5.9.1 (Task 1 — item-0 fix) | 615 |
| Phase 5.9.1.5 (Task 1.5 — fingerprint diagnostic) | 616 |
| Phase 5.9.2 (refactor + new coverage) | **672** |

---

