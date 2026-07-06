# PhaseEnd — Phase 4: Item Randomization + Enemy/Chest Bytecode

## Status: ✅ COMPLETE

**Version:** 1.3.0 → 1.4.0
**Tests:** 276/276 passing

---

## Checklist Verification

| Item | Status | Notes |
|---|---|---|
| Implement `ItemRemapper` | ✅ | `ShuffleTreasurePool` + `ShuffleItemPool`, Fisher-Yates, deterministic |
| Implement `EnemyRandomizer` | ✅ | Drops, steals, blue magic, card drops |
| Implement `ChestRandomizer` | ✅ | Delivered as `FieldItemRandomizer` — scope expanded (see Deviations) |
| Integration tests (same seed → identical output) | ⏩ Deferred to Phase 8 | Unit-level determinism fully verified. End-to-end diff requires `ModOutputWriter` (Phase 8). Listed in Phase 4 checklist by mistake. |
| Unique/legendary item validation | ⏩ Deferred to Phase 6 | Explicitly scoped to `RecommendedLogicEngine`. Project context confirms: "Do NOT add constraint validation to individual randomizers." |
| **Milestone: deterministic patched binary files** | ✅ | All randomizers are deterministic and patch-correct. End-to-end mod folder write is Phase 8. |

---

## Deliverables

### `FieldParser.cs` + `FieldParserTests.cs`
Parser for FF9 Steam field script `.eb.bytes` files. Supports:
- `ParseHeader()` — full entry/function navigation table from 128-byte header
- `FindItemLocations()` — scans all function bodies for 4 opcode patterns
- `ApplyPatches()` — defensive copy + write, never mutates input

**Four location kinds confirmed against binary files:**

| Kind | Opcode | Pattern | Notes |
|---|---|---|---|
| `TreasureItem` | `0x05` SET | `D8 E0 7D [lo][hi] 2C 7F` | Hidden items / exclamation-point pickups |
| `DirectItem` | `0x48` AddItem | `[flag][lo][hi]`, bit0=0 | Chests, direct story gives |
| `TextSync` | `0x66` SetTextVariable | `[flag][00][lo][hi]`, both args constant | Display sync for both above |
| `DirectGil` | `0xCE` AddGil | `[flag][3B signed]`, bit0=0 | Scripted floor gil |

**TreasureItem value encoding** (confirmed from generic pickup handler):
- `X < 512` → item ID
- `512 ≤ X < 1000` → card slot (card ID = X − 512)
- `1000 ≤ X < 29999` → gil amount = X − 1000
- `X ≥ 29999` → disabled / sentinel — scanner skips (`>= 29999`, not `== 29999`)

**False positives:** `0x48` and `0xCE` bytes can appear inside variable expressions of other opcodes, producing spurious `DirectItem`/`DirectGil` hits with impossible item IDs (e.g. 10008, 18241). These are harmless — `ItemRemapTable` passthrough returns unknown IDs unchanged.

**Test data:** 7 `.eb.bytes` files in `Tests/TestData/FieldParser/`. Ground-truth offsets validated via Python simulation + HW script exports (with and without readability comments).

---

### `ItemRemapper.cs` + `ItemRemapperTests.cs`
Pure static shuffle utility. No file I/O, no CSV knowledge.

- `ShuffleTreasurePool(pool, rng)` — full cross-shuffle of items + cards + gil encodings. Any value can map to any other value in the pool.
- `ShuffleItemPool(pool, rng)` — item IDs only (`< 512`). Card slots and gil encodings in input are silently filtered — they passthrough via `ItemRemapTable.Remap()`.

Both methods: Fisher-Yates, deduplicate input, sort before shuffling (input-order-independent), return `ItemRemapTable`.

---

### `EnemyRandomizer.cs` + `EnemyRandomizerTests.cs`
Three independent methods, each opt-in:

- `RemapDropsAndSteals(files, table)` — applies `ItemRemapTable` to all 4 drop + 4 steal slots per stat block. Skips value 0 ("no item") explicitly. Pass `Passthrough()` when feature is disabled.
- `ShuffleBlueMagic(files, rng)` — two-pass cross-file shuffle of non-zero blue magic ability IDs.
- `ShuffleCardDrops(files, rng)` — two-pass cross-file shuffle of non-zero card drop IDs.

Blue magic and card IDs are ability/card namespaces, not item IDs — they are never routed through `ItemRemapTable`.

Files must be passed in consistent order for deterministic output.

---

### `FieldItemRandomizer.cs` + `FieldItemRandomizerTests.cs`
Two-pass orchestrator for all 817 field files.

**Pass 1 — Scan:** Extract `us` language bytes for all fields, run `FieldParser.FindItemLocations`, accumulate global treasure + direct item pools.

**Pass 2 — Shuffle + Patch + Write:** Build both `ItemRemapTable` instances via `ItemRemapper`. Apply patches per-field. Write same patched bytes to all 7 language output paths.

**TextSync resolution rule:**
- Value `≥ 512` → treasureTable (cards/gil only live in treasure system)
- Value `< 512` AND appears as a TreasureItem value in this field → treasureTable
- Value `< 512` AND only appears as a DirectItem value in this field → directTable

**Language handling:** Scripts are byte-identical across all 7 languages (es, fr, gr, it, jp, uk, us). Scan `us` once, write same patched bytes to all 7 output paths.

**Mod stack resolution:** mirrors `ModSourceResolver` — raw .bytes override → mod archive → vanilla `p0data7.bin`.

**Output paths:**
```
{modOutputRoot}\StreamingAssets\assets\resources\commonasset\eventengine\eventbinary\field\{lang}\{fieldFileName}
```

**Public static methods** (testable without file system):
- `ScanAll(fields)` — scan a collection of (name, bytes) pairs
- `BuildPools(locationsByFile)` — extract treasure + direct pools
- `BuildPatches(locations, treasureTable, directTable)` — apply TextSync resolution rule
- `GetOutputPath(modOutputRoot, language, fieldFileName)` — construct output path

---

## Key Discoveries

| Discovery | Impact |
|---|---|
| FF9 hidden items use a Treasure variable system, not direct AddItem | Primary patchable value is in `set Treasure_Item = X` (opcode `0x05` var expression), not in `AddItem`. The generic pickup handler reads the variable and dispatches to item/card/gil based on value range. |
| All 7 language field scripts are byte-identical | Opcodes and byte offsets are the same across all languages. Only AT_TEXT string IDs differ (separate asset). Scan once, write 7 times. |
| `>= 29999` filter required, not `== 29999` | Dead-code branches in engine room scripts (field 57) write `set Treasure_Item = 64776/64785` as "empty chest" placeholders. The `==` check would have passed these through as patchable locations. |
| DirectItem chests cannot hold cards or gil without script rewriting | `AddItem(X, 1)` passes the value directly to the inventory system as a raw item ID. The card/gil encoding only has meaning inside the Treasure variable handler. Chests are limited to item IDs for Phase 4. |
| Item 282 in field 103 is likely a key item | Jump Rope Master prize. `DirectItem(282)` is in the field scanner output but outside the valid consumable range. Must be added to the Phase 6 key item protection list. |

---

## Deviations

| Item | Plan | Actual | Reason |
|---|---|---|---|
| `ChestRandomizer` | Chest contents only | `FieldItemRandomizer` — all field item pickups (hidden items, chests, gil) | Full scan of field scripts revealed the Treasure variable system dominates. "Chest" was too narrow a name for what the scanner actually covers. |
| Integration tests in Phase 4 | Listed in Phase 4 checklist | Deferred to Phase 8 | Requires `ModOutputWriter` to produce a mod folder to diff. Was a planning error in the original checklist. |
| DirectItem cross-type shuffle (items → cards/gil) | Not originally scoped | Explicitly deferred — documented | Would require rewriting chest handler bytecode (function 14 → function 12 on player character entry) for all chest scripts. Tagged as Gen2 candidate. |
| MipsEditor | Phase 3/4 | Removed from Gen1 scope | MIPS = battle AI scripts only. No Gen1 randomizer needs MIPS opcode editing. |

---

## Parking Lot Additions This Phase

| Topic | Target | Notes |
|---|---|---|
| Key item protection list | Phase 6 | World Map, Kupo Nut, Grotto Bell, all scripted-give items must be hardcoded as unshuffable |
| Scripted event detection heuristic | Phase 6 | If AddItem/set Treasure_Item appears in a function with no IsButton/proximity check, it's a scripted give. Auto-generate candidate protected list, manually review. |
| Stellazio shuffle option | Phase 6 | Feature flag. Add Stellazio item IDs to treasure pool. Final Stellazio (only reachable after collecting all others) needs placement validation. |
| Rare coffee shuffle option | Phase 6 | Feature flag. 3 rare coffee key items added to pool. |
| Key item shuffle option | Phase 6 | Broader option. Requires full protection list finalized first. |
| Merge field + enemy into single global pool | Phase 6 orchestrator | When both features are enabled, build one combined pool before calling ItemRemapper, so field items and enemy drops can swap. |
| Auction house randomization | Phase 5/6 | Catalog unique (one-time) vs repeatable auction items. Unique items feed into legendary rarity guardrails. |
| Legendary item stat scaling by field progression | Gen2 | Use field number as progression proxy. If Ultima Weapon lands early, its stats scale down. Complex; Gen2 candidate. |
| Scenario counter soft-lock fix | Gen2 (field entrance randomizer dependency) | All field scripts must be rewritten to check `>= lastEvent AND < nextEvent` instead of one-sided comparisons. Required before field entrance randomization is viable. |
| Enemy level scaling mod integration | Gen2 (field entrance randomizer dependency) | Reference existing level-scaling mod. Prevents soft-locks from high-level enemies in early fields. |

---

## Test Count

| File | Tests | Notes |
|---|---|---|
| SeedEngineTests | 7 | Phase 1 |
| CsvRoundTripTests | 44 | Phase 2 |
| EnemyFileTests | 22 | Phase 3 |
| UnityArchiverTests | 11 | Phase 3 |
| EnemyCatalogAndResolverTests | 10 | Phase 3 |
| Misc (Phase 1) | 4 | Phase 1 |
| FieldParserTests | 53 | Phase 4 |
| ItemRemapperTests | 58 | Phase 4 |
| EnemyRandomizerTests | 39 | Phase 4 |
| FieldItemRandomizerTests | 29 | Phase 4 |
| **Total** | **276** | **All passing** |

---

## Rules Added This Phase

| Rule | Reason |
|---|---|
| Never shuffle DirectItem locations into card/gil values | AddItem opcode only understands raw item IDs. Card/gil encoding only has meaning in the Treasure variable handler (function 12). Chest handler (function 14) does not dispatch. |
| Field scripts must be patched in all 7 language variants | Scripts are byte-identical; patch once, write all 7. Missing a language variant leaves that language's pickups unchanged. |
| TreasureItem sentinel filter must use `>= 29999`, not `== 29999` | Dead-code placeholders (64776, 64785) would pass through a `==` check. |
| Files passed to EnemyRandomizer shuffle methods must be in consistent order | Fisher-Yates result depends on traversal order. Inconsistent order breaks seed determinism across runs. |

---

## Commit Message

```
feat: Phase 4 complete — item randomization + field/enemy bytecode patching

- FieldParser: field script binary parser
  - ParseHeader: entry/function navigation table from 128-byte header
  - FindItemLocations: 4 opcode patterns (TreasureItem, DirectItem, TextSync, DirectGil)
  - ApplyPatches: defensive copy, never mutates input
  - Confirmed binary pattern for Treasure variable system (05 D8 E0 7D)
  - Sentinel filter >= 29999 (not == 29999) blocks dead-code placeholders
- ItemRemapper: pure Fisher-Yates shuffle utility
  - ShuffleTreasurePool: full cross-shuffle items/cards/gil
  - ShuffleItemPool: item IDs only (< 512), filters cards/gil
  - Input-order-independent (sort before shuffle)
- EnemyRandomizer: in-place modification of EnemyFile instances
  - RemapDropsAndSteals: applies ItemRemapTable, skips zero slots
  - ShuffleBlueMagic: two-pass cross-file shuffle
  - ShuffleCardDrops: two-pass cross-file shuffle
- FieldItemRandomizer: two-pass orchestrator for all 817 field files
  - Scan us language bytes, build global treasure + direct pools
  - TextSync resolution rule (>= 512 always treasure; < 512 by context)
  - Write patched bytes to all 7 language output paths
  - Mod stack resolution mirrors ModSourceResolver (p0data7.bin)
- 276/276 tests passing (+178 this phase)
- Key discovery: Treasure variable system is primary hidden item mechanism
- Key discovery: 7 language scripts are byte-identical — scan once, write 7
- DirectItem → card/gil shuffle deferred (requires bytecode rewrite of chest handler)
- Integration test (full mod folder diff) deferred to Phase 8
```

---

## PhaseEnd Changelog

```
v1.3.0 → v1.4.0
- Build Log: Phase 4 entry added
- Key Discoveries: Treasure variable system, 7-language identity,
  >= 29999 filter, DirectItem chest constraint, item 282 key item candidate
- Deviations: ChestRandomizer → FieldItemRandomizer, integration test deferral,
  DirectItem cross-type shuffle deferred, MipsEditor removed
- Rules: 4 new rules added
- Parking Lot: 10 new items (key item protection, Stellazio, rare coffee,
  global pool merge, auction, stat scaling, scenario counter fix, level scaling mod)
- Phase 4 marked complete
- Current phase: 5 — CSV Randomizers
```

---

## 🛑 Stop Here

This chat session is complete. Do the following before starting Phase 5:

1. Add `PhaseEnd_Phase4.md` to your Claude Project
2. Start a new chat session
3. Say: "Continue building Stiltzkin's Bag. Read PhaseEnd Files to get up to date on next steps."

You do not need to delete this chat — keep it for posterity and back-reference. Old chats do not affect speed or token usage.
