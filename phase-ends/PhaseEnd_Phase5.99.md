# Phase 5.99 — PhaseEnd

**Completed:** 2026-04-01
**Tests at phase end:** 754 total — all green, 0 warnings
**Next phase:** 6 (RecommendedLogicEngine) or Phase 8 (RandomizerEngine pipeline wiring)

---

## Phase 5.99 Task Checklist

| # | Task | Status |
|---|------|--------|
| 1 | Diagnostic — FieldItemScanner on all 9 Stiltzkin scripts; confirm DirectItem structure | ✅ Done |
| 2 | Settings audit — StiltzkinMode, StiltzkinRecommendedSubMode, StiltzkinPriceMode enums | ✅ Done |
| 3 | Item pool definitions — Stiltzkin's Junk, Fun (Low/Mid/High), Challenging | ✅ Done |
| 3.5 | Catalog name fix — real item names in VanillaItemCatalog CSV output | ✅ Done |
| 4 | Price range definitions — ClearanceSale, Stiltzkin's Mood, Highway Robbery constants | ✅ Done |
| 5 | FieldItemRandomizer exclusion — Stiltzkin scripts excluded when mode is Shuffle/Recommended | ✅ Done |
| 6 | StiltzkinRandomizer.cs — core class; all modes; Cleyra pairing; ItemRemapTable | ✅ Done |
| 7 | StiltzkinRandomizerTests.cs — pure method tests + integration tests | ✅ Done |
| 8 | Wire into pipeline | ⏩ Deferred to Phase 8 — RandomizerEngine does not exist yet |

---

## What Was Built

### Task 1 — Diagnostic + algorithm confirmation

`StiltzkinItemScanDiagnosticTests.cs` — 2 tests:
- `Diagnostic_StiltzkinScripts_ItemLocationDump` — full location dump for all 9 scripts
- `Verification_PackageItemIds_MatchVanillaData` — confirms algorithm returns correct IDs for all 8 visits

**Key findings:**
- All Stiltzkin items appear as `DirectItem` (AddItem opcode) — FieldItemRandomizer already scans them
- Each item appears as exactly 2 hits ~22 bytes apart — both offsets must always be patched
- Algorithm: price TextSync offset → first 3 valid-ID (1–255) pairs after it → correct for all 9 scripts
- All edge cases handled: Alex5 Ribbon (before price TextSync, excluded), Oeilvert extra item 161 (4th pair, excluded), Bran Bal extra item 87 (4th pair, excluded)
- Cleyra pair (ANTRION + INN): both scripts yield [237, 238, 249] independently ✓

---

### Task 2 — Settings

Three new enums added to `Settings.cs`:

**`StiltzkinMode`:**
- `Off` — scripts untouched (default)
- `Shuffle` — Fisher-Yates across all 24 vanilla items, 3 per visit
- `IncludeInFieldPool` — Stiltzkin scripts included in FieldItemRandomizer's normal pool
- `Recommended` — curated per-visit draws using StiltzkinRecommendedSubMode

**`StiltzkinRecommendedSubMode`:**
- `StiltzkinsJunk` — low-utility consumables and cheap weapons/armor; comedy mode
- `Fun` — structured: 1 low-tier + 1 mid-tier + 1 high-tier item per package (the true vision)
- `Challenging` — all 12 ability gems + Phoenix Pinion + Dark Matter; AP investment pressure

**`StiltzkinPriceMode`:** (independent of item mode)
- `Off` — vanilla prices (default)
- `ClearanceSale` — 1–99G
- `StiltzkinsMood` — 100–10,000G (full chaos)
- `HighwayRobbery` — 5,000–15,000G

Three new Settings properties:
- `StiltzkinMode StiltzkinMode` (default: Off)
- `StiltzkinRecommendedSubMode StiltzkinRecommendedSubMode` (default: Fun)
- `StiltzkinPriceMode StiltzkinPriceMode` (default: Off)

---

### Task 3 — Item pool definitions

Added to `VanillaObtainabilityData.cs`:

| Field | Type | Description |
|-------|------|-------------|
| `StiltzkinJunkPool` | `IReadOnlyList<int>` | 33 items — lowest-tier weapons, armor, common consumables |
| `StiltzkinFunPoolLow` | `IReadOnlyList<int>` | ~42 items — entry-level gear and basic consumables |
| `StiltzkinFunPoolMid` | `IReadOnlyList<int>` | ~85 items — solid mid-game gear across all categories |
| `StiltzkinFunPoolHigh` | `IReadOnlyList<int>` | ~75 items — late-game/rare gear, accessories, gems |
| `StiltzkinChallengingPool` | `IReadOnlyList<int>` | 14 items — all 12 gems + Phoenix Pinion + Dark Matter |

Pools span the full item universe (weapons, armor, accessories, consumables) — not limited to consumables or gems.

---

### Task 3.5 — Catalog name fix

`ItemObtainabilityEntry` gains `Name { get; init; }` property.

`VanillaItemCatalog.Build()` gains `ParseItemNames()` private static helper:
- Reads `ParsedCsv<ItemsRow>.InlineComments` (index-aligned with Rows)
- Strips leading `#`/`;` and whitespace, splits on first ` - `
- Builds `Dictionary<int, string>` of item ID → display name
- Zero file re-reads, zero new dependencies

`Diagnostic_DumpFullCatalogToCSV` updated to use `entry.Name` instead of its own local parsing block. Catalog CSV now shows real item names (Hammer, Dagger, etc.) instead of Item0, Item1 placeholders.

---

### Task 4 — Price range constants

Added to `VanillaObtainabilityData.cs`:

```
StiltzkinClearanceSaleMin = 1
StiltzkinClearanceSaleMax = 99
StiltzkinsMoodMin         = 100
StiltzkinsMoodMax         = 10,000
HighwayRobberyMin         = 5,000
HighwayRobberyMax         = 15,000
```

All values confirmed int16-safe (≤ 32,767). Stiltzkin's price is encoded as a signed int16 in SetTextVariable bytecode.

---

### Task 5 — FieldItemRandomizer exclusion

`FieldItemRandomizer.Randomize()` gains `Settings settings` second parameter.

New public static `BuildStiltzkinExclusionSet(StiltzkinMode mode)`:
- Returns empty `HashSet<string>` for `Off` and `IncludeInFieldPool`
- Returns all 9 confirmed script names for `Shuffle` and `Recommended`
- Case-insensitive (OrdinalIgnoreCase)

`fieldNames` list is filtered against the exclusion set before Pass 1. `FieldRandomizationResult` gains `StiltzkinScriptsExcluded` counter.

---

### Task 6 — StiltzkinRandomizer

`StiltzkinsBag.Core/Randomizers/StiltzkinRandomizer.cs` — new static class.

**Public surface:**
- `Randomize(p0data7Path, settings, rng, itemRemapTable, modOutputRoot)` → `StiltzkinRandomizerResult`
- `FindItemPairs(locations, packagePrice)` — finds 3 DirectItem pairs after price TextSync
- `FindPriceLocation(locations, packagePrice)` — finds last TextSync with value == packagePrice
- `DrawFromPool(pool, rng)` — single pool draw
- `DrawPrice(mode, vanillaPrice, rng)` — price draw or vanilla passthrough
- `GetOutputPath(modOutputRoot, language, scriptFileName)` — output path construction

**RNG call order (strictly enforced):**
1. Item selection — all 8 visits in ascending price order
   - Shuffle: 23 Fisher-Yates calls (flat 24-item array)
   - Junk/Challenging: 3 × `rng.Next(poolSize)` × 8 visits = 24 calls
   - Fun: 3 draws per visit (1 Low + 1 Mid + 1 High) × 8 visits = 24 calls
2. Price selection — 1 call per visit when active; 0 when Off

**Cleyra pairing:** One RNG draw set covers both ANTRION and INN. ANTRION is the authoritative vanilla item source for Shuffle mode. Both scripts patched independently at their own byte offsets with identical values.

**ItemRemapTable:** Applied to all selected item IDs after RNG draws, before patching bytecode.

**Patching:** Both DirectItem locations per item pair are patched. Price TextSync patched only when new price ≠ vanilla price. Patched US bytes written to all 7 language output paths.

**`StiltzkinRandomizerResult`:**
- `WasRun` — false for Off/IncludeInFieldPool
- `VisitsProcessed` — number of visits processed (8)
- `ScriptsPatched` — 9 for full run (8 visits + Cleyra extra)
- `NewItemsByVanillaPrice` — `Dictionary<int, int[]>` (vanilla price → 3 item IDs post-remap)
- `NewPriceByVanillaPrice` — `Dictionary<int, int>` (vanilla price → new price)

---

### Task 7 — Tests

`StiltzkinRandomizerTests.cs` — 46 tests across 6 test classes:

| Class | Tests | Coverage |
|-------|-------|----------|
| `FindPriceLocationTests` | 5 | Burmecia, Cleyra last-occurrence, null guard, missing, empty |
| `FindItemPairsTests` | 9 | 3-pair structure, correct IDs, item 73 exclusion, pair ordering, null/missing/empty |
| `DrawFromPoolTests` | 5 | Single-element, pool membership, null guards, empty pool |
| `DrawPriceTests` | 6 | Off passthrough (no RNG), range bounds for all 3 modes, int16 safety, null guard |
| `BuildStiltzkinExclusionSetTests` | 4 | Off/IncludeInFieldPool empty, Shuffle/Recommended count=9, all names present, case-insensitive |
| `ItemRemapTableTests` | 2 | Remap applied to drawn items, passthrough leaves IDs unchanged |
| Integration tests | 6 | Determinism, pool integrity (all 24 vanilla items in shuffle), Cleyra pair identical, Off/IncludeInFieldPool NotRun, Fun pool membership |

**Test count progression:**
- Phase 5.9.3 end: 706
- Task 1 (diagnostic + verification): +2 → 708
- Task 5.99 work (Settings, pools, names, price constants): +0 new tests
- Task 7 (StiltzkinRandomizerTests): +46 → **754 total**

---

## Key Discoveries

**DirectItem confirmed for all Stiltzkin scripts:**
Stiltzkin's packages use `AddItem` (DirectItem opcode), not `SetTreasureItem` (TreasureItem opcode). This means FieldItemRandomizer was already processing Stiltzkin scripts — the Task 5 exclusion was not optional.

**Pair structure is universal:**
Every Stiltzkin item appears as exactly 2 DirectItem hits with the same ID, ~22 bytes (0x16) apart. This is consistent across all 9 scripts. Both hits represent two conditional branches of the give logic. Both must always be patched.

**Algorithm edge cases:**
- Alex5 Ribbon (item 221) appears before the price TextSync → correctly excluded by offset filter
- Oeilvert item 161 and Bran Bal item 87 appear as 4th pairs → correctly excluded by 3-pair limit
- Cleyra's extra TextSync at value 1970 → handled by `LastOrDefault()` on the price scan

**Price encoding is int16:**
Stiltzkin's price is stored as a SetTextVariable value (int16). All three price modes have max values ≤ 15,000, well within signed int16 range (32,767).

**Catalog names were placeholder:**
`VanillaItemCatalog` was writing `Item0`, `Item1` etc. because `ItemObtainabilityEntry` had no Name field. Fixed as Task 3.5 — names now parsed from Items.csv inline comments at Build() time.

---

## Deviations

| Item | Plan | Actual | Reason |
|------|------|--------|--------|
| Task 8 — Pipeline wiring | Wire StiltzkinRandomizer into generation pipeline | Deferred to Phase 8 | RandomizerEngine does not exist yet; wiring now would create premature stub infrastructure |
| StiltzkinRecommendedSubMode item pools | Initial design used only health items and gems | Redesigned to span full item universe (weapons, armor, accessories, consumables) | Original pools were too narrow; pools should reflect the full game item variety |
| Price patching — RemoveGil | Initially assumed RemoveGil needed patching | Only SetTextVariable (TextSync) needs patching | Game engine reads the text variable for both display and Gil deduction; RemoveGil confirmed irrelevant from Phase 5.9.3 dump analysis |
| Task 3.5 | Not in original plan | Added mid-phase | Catalog showing Item0/Item1 placeholder names was blocking diagnostic readability |

---

## Rules Added This Phase

**Rule: Stiltzkin items are DirectItem (AddItem opcode), not TreasureItem.**
All 9 Stiltzkin scripts deliver items via `AddItem` (opcode `0x48`). FieldItemRandomizer processes these as DirectItem locations. Any active Stiltzkin mode (Shuffle/Recommended) MUST exclude Stiltzkin scripts from FieldItemRandomizer via `BuildStiltzkinExclusionSet`. Failure to exclude results in double-randomization.

**Rule: Both DirectItem locations per Stiltzkin item pair must be patched.**
Each Stiltzkin item appears as 2 DirectItem hits ~22 bytes apart in bytecode. These represent two conditional branches of the purchase give logic. Always patch both offsets. Patching only one leaves one branch giving the old item.

**Rule: Stiltzkin price is SetTextVariable only — do not patch RemoveGil.**
The game engine reads `SetTextVariable(0, price)` (TextSync) for both display and Gil deduction. The RemoveGil opcode visible in field dumps is unrelated to the package price. Patching it would corrupt unrelated script logic.

**Rule: StiltzkinRandomizer item draws precede price draws.**
RNG call order: all 8 item selections first (ascending price order), then all 8 price selections. Never interleave. This is consistent with the project-wide rule that RNG call order is sacred.

**Rule: ItemObtainabilityEntry.Name is parsed at Build() time from Items.csv inline comments.**
Names come from `ParsedCsv<ItemsRow>.InlineComments` via `ParseItemNames()`. Never hardcode item names elsewhere. The VanillaItemCatalog is the single source of item display names.

---

## Parking Lot

### Carry to Phase 8

**Task 8 — StiltzkinRandomizer pipeline wiring:**
When `RandomizerEngine` is built in Phase 8, `StiltzkinRandomizer.Randomize()` slots in at step 4b — after `FieldItemRandomizer` (step 4), after `ItemRemapper` produces `ItemRemapTable` (step 2).

Call signature:
```csharp
var stiltzkinResult = StiltzkinRandomizer.Randomize(
    p0data7Path,
    settings,
    rng,
    itemRemapTable,   // from ItemRemapper, or Passthrough() if item shuffle inactive
    modOutputRoot);
```

Condition: only call when `settings.StiltzkinMode != Off` and `settings.StiltzkinMode != IncludeInFieldPool` — though `Randomize()` handles both gracefully by returning `NotRun`.

### Carry from Phase 5.9.3 (still pending)

**Multi-locale field patching:**
144 field scripts have different bytecode sizes across locales (mostly JP shorter). Current approach writes US bytes to all 7 locales — incorrect for non-US players on those 144 scripts. Correct approach: per-locale extraction + patch application at same byte offsets. This affects both FieldItemRandomizer and StiltzkinRandomizer.

**EnemyCatalogTests still uses JSON:**
`EnemyCatalogTests` still references `StockEnemyBytesJsonNoZeros.json`. Should be rewritten to use `.bytes` files from disk.

**ModSourceResolverTests require Memoria.ini:**
Several tests fail without a Memoria-enabled install.

**ShopRandomizer update:**
Replace `Price > 2` filter with `ItemPool.ShopFriendly`. Deferred since Phase 4.

---

## Test Count

| Phase | Tests |
|-------|-------|
| Phase 5.9.3 end | 706 |
| Phase 5.99 Task 1 | +2 → 708 |
| Phase 5.99 Tasks 2–6 | +0 |
| Phase 5.99 Task 7 | +46 → **754** |

**Final: 754 — all green, 0 warnings**

---

## Commit Message

```
feat: Phase 5.99 complete — StiltzkinRandomizer

Settings.cs:
- StiltzkinMode enum: Off / Shuffle / IncludeInFieldPool / Recommended
- StiltzkinRecommendedSubMode enum: StiltzkinsJunk / Fun / Challenging
- StiltzkinPriceMode enum: Off / ClearanceSale / StiltzkinsMood / HighwayRobbery
- Three new Settings properties (all default Off/Fun)

VanillaObtainabilityData.cs:
- StiltzkinJunkPool (33 items): lowest-tier weapons, armor, consumables
- StiltzkinFunPoolLow/Mid/High (42/85/75 items): full item universe by tier
- StiltzkinChallengingPool (14 items): all 12 gems + Phoenix Pinion + Dark Matter
- Price range constants: ClearanceSale(1-99), Mood(100-10000), Highway(5000-15000)
- All prices int16-safe (≤ 32,767)

ItemObtainabilityEntry.cs:
- Name property added (parsed from Items.csv inline comments at Build() time)

VanillaItemCatalog.cs:
- ParseItemNames() helper: reads ParsedCsv InlineComments, strips #/; prefix,
  splits on " - ", builds id→name map at zero extra file I/O cost
- Name stamped onto each ItemObtainabilityEntry in step 3

FieldItemRandomizer.cs:
- Randomize() gains Settings parameter
- BuildStiltzkinExclusionSet(StiltzkinMode): returns all 9 script names for
  Shuffle/Recommended; empty set for Off/IncludeInFieldPool
- fieldNames filtered before Pass 1 when exclusions are active
- FieldRandomizationResult gains StiltzkinScriptsExcluded counter

StiltzkinRandomizer.cs (new):
- Static class; Randomize() + 5 public pure helpers + result type
- Shuffle: FY of 24 vanilla items (23 RNG calls), 3 per visit
- Recommended: 24 pool draws (3/visit); Fun draws 1 from each tier
- Price: 1 draw per visit for active modes; Off = vanilla price, 0 RNG calls
- RNG order: all item draws first (ascending price), then all price draws
- Cleyra: ANTRION authoritative for vanilla scan; both scripts patched identically
- ItemRemapTable applied post-draw, pre-write
- Patching: both DirectItem pair offsets + price TextSync; 7 language output paths
- NotRun returned for Off/IncludeInFieldPool without consuming any RNG calls

StiltzkinItemScanDiagnosticTests.cs (updated):
- FindPackageItemIds algorithm added and verified
- Verification_PackageItemIds_MatchVanillaData: all 8 visits + Cleyra pair + Ribbon guard

StiltzkinRandomizerTests.cs (new):
- 46 tests: FindPriceLocation, FindItemPairs, DrawFromPool, DrawPrice,
  BuildStiltzkinExclusionSet, ItemRemapTable, 6 integration tests
- Integration: determinism, pool integrity (all 24 vanilla items preserved in Shuffle),
  Cleyra pair identical items+price, Off/IncludeInFieldPool NotRun, Fun pool membership

Tests: 706 → 754 (+48), all green, 0 warnings
```

---

## PhaseEnd Changelog

```
v1.5.9.3 → v1.5.99
- Build Log: Phase 5.99 entry added
- Key Discoveries: DirectItem opcode for all Stiltzkin scripts, pair structure universal,
  algorithm edge cases (Alex5 Ribbon, Oeilvert/Bran Bal 4th pairs), price int16 encoding,
  catalog placeholder name fix
- Deviations: Task 8 deferred, pool redesign (full item universe), RemoveGil non-issue,
  Task 3.5 added
- Rules: 5 new rules added
- Parking Lot: Task 8 pipeline wiring (Phase 8), multi-locale patching,
  EnemyCatalogTests JSON, ModSourceResolver tests, ShopRandomizer filter
- Phase 5.99 marked complete
- Current phase: 6 — RecommendedLogicEngine
```

---

## 🛑 Stop Here

This chat session is complete. Do the following before starting Phase 6:

1. Add `PhaseEnd_Phase5_99.md` to your Claude Project
2. Start a new chat session
3. Say: "Continue building Stiltzkin's Bag. Read all attached markdown files."

Do not continue development in this session. Do not delete this chat — keep it for
posterity and back-reference. Large chats in a project will not slow down chat or use more tokens.
