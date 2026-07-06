# Phase End — Phase 5.9.1.5: Data Integrity — Item-0 Sentinel + Disc-Variant Fingerprint Diagnostic

**Date completed:** 2026-03-28
**Tests at phase end:** 615 (Task 1) → **616 total — confirmed green, 0 warnings**
**Version tag:** 5.9.1.5
**Next phase:** 5.9.2 (Chocograph double-counting fix + BattleItemScanner + remaining data integrity tasks)

---

## What Was Built

### Task 1 — Item-0 Sentinel Fix

**Problem:** `FieldItemScanner` was reporting 85 occurrences of item ID 0 (Hammer). Only one real Hammer give exists: `AddItem(0, count=1)` in `TRENO1_TR_QHM_0.eb` and `TRENO2_TR_QHM_0.eb` (Queen Stella, Stellazzio quest, Field 1911). The other 84 hits were `AddItem(0, count=0)` — a scripting convention used as a "give nothing" null sentinel in dead branches and default-case handlers.

**Fix:** Changed `FieldItemScanner` filter from `itemId < 0` to `itemId < 1`. Item ID 0 is now excluded from scan results entirely. The Hammer's single real give is confirmed present via the diagnostic output and is tracked as a manually-coded source in `VanillaObtainabilityData`.

**Files changed:**
- `StiltzkinsBag.Core/Models/FieldItemScanner.cs` — filter updated; header comment updated to document item-0 sentinel exclusion and point to Queen Stella (Field 1911) as the real source
- `StiltzkinsBag.Tests/VanillaItemCatalogTests.cs` — `Diagnostic_DumpItemZeroFalsePositiveLocations` diagnostic test added; `evt_battle_` exclusion applied

**Tests:** 615 passing after Task 1.

---

### Task 1.5 — Disc-Variant Fingerprint Diagnostic

**Problem:** FFIX field scripts exist in per-disc variants — the same scene (e.g., Queen Stella's shop) has separate `.eb.bytes` files for each disc era it appears in (e.g., `TRENO1_TR_QHM_0.eb` vs `TRENO2_TR_QHM_0.eb`). The randomizer must patch all disc variants of the same location identically. Without a map of which files are variants of each other, the randomizer could patch disc 1 and silently leave disc 2 unpatched, breaking the game mid-playthrough.

**Solution:** Added `Diagnostic_DiscVariantFingerprintMap` test. For every DirectItem and TreasureItem location found by FieldParser (IDs 1–255), it extracts a 97-byte fingerprint: 48 bytes before the item-ID byte, the item-ID byte zeroed (so different items don't prevent a match), and 48 bytes after. Locations sharing an exact fingerprint across two or more different files are confirmed disc-variant pairs.

**Output:** `bin/Debug/net8.0/TestData/DiscVariantFingerprints.txt` — three sections:
- **Section A:** Confirmed disc-variant groups (exact fingerprint match across 2+ files)
- **Section B:** Near-miss pairs (≤6 differing bytes, cross-file singletons)
- **Section C:** Full fingerprint dump (all 1–255 locations)

**Files changed:**
- `StiltzkinsBag.Tests/VanillaItemCatalogTests.cs` — `Diagnostic_DiscVariantFingerprintMap` test added (616 total tests); both diagnostic scans updated to apply `&& !n.StartsWith("evt_battle_", ...)` exclusion, matching `FieldItemScanner` behavior exactly

**Tests:** 616 passing, 0 warnings.

---

## Key Findings from the Fingerprint Diagnostic

### Confirmed disc-variant file families (Section A — 98 groups)

These file pairs MUST be patched in sync by any randomizer that modifies field item locations.

| Location | File pair / family | Items confirmed |
|---|---|---|
| Queen Stella (Stellazzio) | `TRENO1_TR_QHM_0` ↔ `TRENO2_TR_QHM_0` | 249, 19, 239, 201, 204, 175 (+ item-0 sentinels) |
| Treno Fantasy Battle | `TRENO1_TR_FBH_0` ↔ `TRENO2_TR_FBH_0` | 3 |
| Treno Queen's House | `TRENO1_TR_FQH_0` ↔ `TRENO2_TR_FQH_0` | 238 |
| Treno restaurant chest | `TRENO1_TR_RES_0` ↔ `TRENO2_TR_RES_0` | 1 (TreasureItem) |
| Alexandria (shops/streets) | `ALEX1_AT_HOUSE_1` ↔ `ALEX3_AT_HOUSE_1`, `_AT_MAGIC`, `_AT_WEAPON` | 244, 238, 247 |
| Lindblum (disc 1/2/3) | `LIND1/2/3_CS_LB_CTM`, `_CS_LB_EXB`, `_CS_LB_LLP` | 34, 64 |
| Cleyra cathedral | `CLEYRA2_CATHE_1` ↔ `CLEYRA3_CATHE_1` | 212 |
| Cleyra waterfall | `CLEYRA2_WATER` ↔ `CLEYRA3_WATER` | 123 |
| Black Mage Village | `MAGE_BV_CSI/ENT/ITM_0` ↔ `MAGE2_BV_CSI/ENT/ITM_0` | 251, 239, 201 |
| Burmecia courtyard | `BURMECIA_PATIO_2` ↔ `BURMECIA_PATIO_3` | 240, 253 |
| Cleyra dungeon | `CLEYRA1_DUN_01A` ↔ `CLEYRA1_DUN_01B`, `CLEYRA1_DUN_13A` ↔ `CLEYRA1_DUN_13B` | 193, 251 |
| Cleyra (ANTRION/INN) | `CLEYRA3_ANTRION` ↔ `CLEYRA3_INN` | 237, 238, 249 |
| North Gate | `GATE_N_NG_BDA_0/1`, `_BMA_0/1`, `_MDA_0/1` | 238, 253, 237, 236, 244 |
| Pinnacle Rocks | `PINA_PR_ENT_0` ↔ `PINA_PR_ENT_1`, `_PW2_0` ↔ `_PW3_0` | 240, 239, 92, 8 |
| Qu's Marsh | `KUINA_KM_SWP_2` ↔ `KUINA_KM_SWP_3` | 84 |
| Lindblum occupation | 12× `LIND1_TN_LB_*` sub-areas | 74 (Ore — same scripted give across all sub-areas) |
| World map scripts | `EVT_WORLD_WORLD03–11`, `WORLDSV`, `WORLDTS` | 1, 11, 251, 252, 253 |
| Chocograph areas | `CHOCO_CH_FGD/FST/HLG_0` | 89, 175, 209, 87, 238, 254, 249, 251 |

### Near-miss pattern deferred (Section B)

**Position-61 chest-counter byte:** All `ALEX1_*` ↔ `ALEX3_*` and `CLEYRA2_*` ↔ `CLEYRA3_*` TreasureItem pairs show up as 1-byte near-misses at position 61 (13 bytes after the item-ID byte). The differing byte (`0x0B`–`0x11`) is a chest-sequence counter in the TreasureItem script template that incremented between disc 1 and disc 3 versions of each scene. These ARE real disc variants — they just fail the exact-match threshold because of this counter.

**Decision:** Do not add position-61 exemption to the diagnostic. Document here. The randomizer synchronizer (future phase) must handle these by treating same-location-prefix near-miss pairs with ≤1 byte diff at position 61 as confirmed disc variants.

---

## Calibration Notes — Fingerprint Radius

| Radius | Groups | Notes |
|---|---|---|
| 16 bytes | ~152 | Too loose — item 123 matched across 26 unrelated files (Burmecia, Cleyra, Gizamaluke, Lindblum). The `AddItem` opcode surroundings are a common scripting idiom at this width. |
| 48 bytes | 98 | Correct — all groups are geographically coherent (same location family). No cross-city false positives in Section A. |

**48 bytes is the validated radius.** Do not reduce without re-validating cross-location coherence.

---

## Architecture Decisions Confirmed

### Game data is source of truth
- Field scanner output → primary item counts
- Guide CSVs (chocograph list, steal/drop lists, etc.) → validation reference only
- Where scanner and guide disagree → investigate, do not assume either is correct

### `evt_battle_` exclusion is canonical
`FieldItemScanner` and all diagnostic scans filter out any file whose name starts with `evt_battle_`. This excludes:
- `EVT_BATTLE_WM_9900–9903` — world-map battle reward scripts (one per disc)
- `EVT_BATTLE_IC_*`, `EVT_BATTLE_IP_*`, `EVT_BATTLE_CY_*` — other battle event scripts

These will be handled by `BattleItemScanner` (Phase 5.9 Task 3), which reads p0data2.bin.

### Item 0 is permanently excluded from FieldItemScanner
`AddItem(0, X)` is a null-sentinel convention in FFIX field scripts. The Hammer's single real give (`AddItem(0, count=1)` in `TRENO1/TRENO2_TR_QHM_0`) is tracked manually in `VanillaObtainabilityData`. Do not re-enable item-0 scanning without isolating the sentinel filter (count > 0 AND within Queen Stella files only).

---

## Session Recovery Note

Phase 5.9 Task 1 was completed in a prior session that was lost due to context exhaustion. The lost session was recovered from `Phase 5.9 Task1 Lost Chat Session.json` (78-message JSON). Task 1 completion and the origin of Task 1.5 were reconstructed from that file. The JSON file is no longer needed.

Key facts from the recovered session:
- Task 1 complete before this session began; 615 tests green
- User coined "Task 1.5" to describe the disc-variant synchronization problem discovered when item-0 diagnostics showed Hammer appearing in both TRENO1 and TRENO2 versions of the same scene
- User requested: fingerprint 16 bytes each side initially, output all fingerprints, report mismatches with byte-level diff detail

---

## Project Design Decisions

### Data Architecture
- **Game files are the source of truth.** Field scans, battle scans, and world map scans produce primary data. Guide CSVs validate scanner accuracy and catch gaps — they do not override it.
- **When scanner and guide disagree:** investigate; do not assume either is correct. If a scanner finds something the guide missed, trust the scanner. If they conflict, dig into the encoding/opcodes.

### FieldItemScanner
- Filters files by `StartsWith("evt_")` — matches the FieldItemRandomizer convention.
- Excludes `evt_battle_*` files — those are battle scripts handled by BattleItemScanner, not field pickups.
- Excludes item ID 0 entirely. `AddItem(0, X)` is a null-sentinel convention in field scripts. The Hammer's single real give (`AddItem(0, count=1)` in TRENO1/TRENO2_TR_QHM_0, Queen Stella, Field 1911) is tracked manually in `VanillaObtainabilityData`.
- World map scripts (`evt_world_*`) live in p0data7.bin and are picked up by the field scanner — this is intentional and correct.
- Chocograph dig calls exist inside world map scripts and overlap with `IsChocographReward`. Do not double-count. Field scan is primary; `ChocographItemCounts` is validation reference only.
- 7 language locale variants of each `evt_` file are byte-identical (bytecode only; AT_TEXT string IDs differ separately). Deduplicate by leaf name and scan each unique name once.
- `p0data7.bin` archive: `GetFileNames()` returns names WITHOUT `.bytes`; `Extract()` handles the fallback strip automatically.

### Disc-Variant Synchronization
- Fingerprint radius: **48 bytes each side** (97-byte window, item-ID byte zeroed). Validated as correct — all confirmed groups are geographically coherent. Do not reduce.
- All confirmed disc-variant pairs (Section A of `DiscVariantFingerprints.txt`) must be patched in sync by any randomizer that touches field item locations.
- **Known gap — position-61 chest-counter byte:** ALEX1/3 and CLEYRA2/3 TreasureItem pairs are real disc variants that fail exact-match because of a single chest-sequence counter byte at position 61. They appear in Section B (near-misses). Do not add a position-61 exemption to the current diagnostic. Handle in the synchronizer phase.

### BattleItemScanner
- Reads **p0data2.bin**, not p0data7.bin, for steal/drop item IDs.
- Friendly Monster and Ragtime Mouse rewards come through battle scripts, not `evt_` field scripts — they are not double-counted by FieldItemScanner.

### Data Sources Still to Integrate
- **Kupo Nut rewards** (Mognet chain): finite, one per disc — Holy Bell / Elixir / Extension / Aloha T-Shirt. CSV in `FFIX GUIDE DATA/`.
- **Missable items:** `FFIX GUIDE DATA/Missable Items.txt` — source for `IsMissable` flag on `ItemObtainabilityEntry`.
- **Stiltzkin locations:** identified by Gil charge amounts (333, 444, 555, 666, 777, 888, 2222 Gil) via FieldParser `AddGil` scan. Known field IDs: 764 / 1105 / 1418 / 1553 / 1865 / 2259 / 2655 / 2456.

### Roadmap Decisions
- Sub-option backtrack for Phase 1–5 randomizers: deferred until all Phase 5.x work is complete.
- Phase 5.99 = StiltzkinRandomizer (after all Phase 5.9 tasks done).
- Phase 6 = RecommendedLogicEngine — uses validated VanillaItemCatalog + ItemPool for per-item copy-cap enforcement.
- ShopRandomizer filter update (`Price > 2` → `ItemPool.ShopFriendly`) deferred to Phase 6.
- Parking lot: Stiltzkin random locations (later generation feature, not current scope).

---

## Parking Lot — Remaining Phase 5.9 Tasks (pick up at Phase 5.9.2)

### Task 2 — Fix chocograph double-counting
World map field scan (`evt_world_*`) finds the same `AddItem` calls as chocograph digs. `IsChocographReward` (hardcoded guide) and `IsFieldItem` (live scan) currently both contribute to `ObtainabilityCounts`, inflating totals. Fix: make field scan the primary source; demote `ChocographItemCounts` to validation reference only (check that it matches what the scanner finds, but do not add its counts).

### Task 3 — BattleItemScanner
Scan p0data2.bin battle scripts for steal/drop item IDs. Validate against guide CSVs (`Steal.csv`, `Drop.csv` or equivalent). This is the authoritative source for `FriendlyMonsterItemIds`, `RagtimeMouseItemIds`, normal/boss enemy steal/drop sets. Output should feed into `VanillaObtainabilityData` validation.

### Task 4 — Add 8 missing chocograph/Dead Pepper items
Add to `VanillaObtainabilityData.ChocographItemCounts` (or equivalent validation set):

| ID | Item | Source |
|---|---|---|
| 29 | Ragnarok | Outer Island chocograph / Chocobo's Air Garden |
| 40 | Dragon's Hair | Forgotten Continent, Dead Pepper crack |
| 45 | Dragon's Claws | Forgotten Lagoon (light blue Choco) |
| 56 | Tiger Racket | Quan's Dwelling dive, Dead Pepper (dark blue Choco) |
| 113 | Straw Hat | Mist/Outer Continent dive (×8), deep blue/golden Choco |
| 190 | Maximillian | Shimmering Island area, Dead Pepper dive |
| 195 | Sandals | Same as 113 (×8) |
| 217 | Pearl Armlet | Same as 113 (×8) |

### Task 5 — Kupo nut rewards
Add new source type to `VanillaObtainabilityData`: Kupo Nut / Mognet quest chain, 4 items, finite, one per disc. CSV in `FFIX GUIDE DATA/`. Items: Holy Bell (D1), Elixir (D2), Extension (D3), Aloha T-Shirt (D4).

### Task 6 — IsMissable flag
Read `FFIX GUIDE DATA/Missable Items.txt`. Add `bool IsMissable` property to `ItemObtainabilityEntry`. Tag all items on the missable list. Key missables include: Moonstone #1, Autograph, Moogle Suit, Javelin, all 7 Stiltzkin packages, Emerald ×2, Diamond ×3, Running Shoes ×2 (Tantarian + Hades steal), all 4 Genji pieces, Excalibur II, Duel Claws, Dark Matter #2+.

### Task 7 — Stiltzkin field identification
Scan `p0data7.bin` archive for `evt_` field scripts containing `AddGil(X)` where X ∈ {333, 444, 555, 666, 777, 888, 2222}. This resolves the 8 exact `evt_` filenames for Stiltzkin's visit locations. Output: confirmed filename-to-visit mapping, ready for StiltzkinRandomizer (Phase 5.99).

---

## Test Count

| Phase | Tests |
|---|---|
| Phase 5.8 end | 611 |
| Phase 5.9.1 (Task 1 — item-0 fix) | 615 |
| Phase 5.9.1.5 (Task 1.5 — fingerprint diagnostic) | **616** |
