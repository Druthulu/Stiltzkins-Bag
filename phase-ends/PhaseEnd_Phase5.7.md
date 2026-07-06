# PhaseEnd — Phase 5.7: TetraMaster Randomizer
**Date:** 2026-03-28
**Project Version:** 1.5.6 → 1.5.7
**Phase Status:** ✅ Complete

---

## Checklist Verification

| Item | Status | Notes |
|---|---|---|
| Settings.cs audit | ✅ | 5 new enums, 14 new properties, full TetraMaster section replacing stub |
| `TetraMasterFile.cs` | ✅ | Typed structs + static read/write for all 3 binary files + per-language name files |
| Terminal: copy test data to TestData/TetraMaster/ | ✅ | 4 files: 3 binary + 1 mes |
| `TetraMasterRandomizerResult.cs` | ✅ | Sealed record with CardStats, CardSets, NpcDecks, CardNames (nullable) |
| `TetraMasterRandomizer.cs` | ✅ | 7 sub-steps in strict RNG order, all modes, fully deterministic |
| `TetraMasterRandomizerTests.cs` | ✅ | 52 tests, all passing |
| **Milestone: TetraMasterRandomizer complete and deterministic** | ✅ | All modes, all three binary files + 7-language name files |

---

## Build Log

**Files created or replaced this phase:**

- `StiltzkinsBag.Core/Models/Settings.cs` — Full replacement. Old stub
  (`RandomizeTetraMaster`, `RandomizeCardStats`, `RandomizeCardOrder`,
  `RandomizeDecks`) replaced with 14-property TetraMaster section organized
  into 6 sub-sections. 5 new enums added alongside existing enum block:
  `CardStatMode`, `CardTypeMode`, `ArrowMode`, `CardSetMode`, `NpcDifficultyMode`.
  `IsDebugMode` doc comment updated to include `CardStatMode.AllCardsMaxed`.

- `StiltzkinsBag.Core/Models/TetraMaster/TetraMasterFile.cs` — New file.
  Typed structs and static pure read/write methods for all TetraMaster binary formats.

  **Structs:**
  - `TetraMasterCardEntry` — sealed record; fields: `Attack`, `Type`, `Defence`,
    `MagicDefence`, `Arrows` (5 bytes). Fifth byte is directional arrow bitmask,
    not a point value — named accordingly.
  - `TetraMasterSetEntry` — sealed class with `byte[] _cardIds` (16 slots);
    `CardIds` property returns a defensive copy. Constructor validates count = 16.
  - `TetraMasterDeckEntry` — sealed record; fields: `SetIndex`, `Difficulty` (2 bytes).

  **Constants:** `CardCount=100`, `SetCount=64`, `SetCapacity=16`, `DeckCount=256`.

  **Read/Write methods (all static, inputs never mutated):**
  - `ReadCardStats` / `WriteCardStats` — 100 entries, 500 bytes. Validates size.
  - `ReadCardSets` / `WriteCardSets` — 64 entries × 16 slots, 1024 bytes.
  - `ReadNpcDecks` / `WriteNpcDecks` — 256 entries, 512 bytes.
  - `ReadCardNames` / `WriteCardNames` — splits/joins on literal ASCII `[ENDN]`
    (6 bytes: 0x5B 0x45 0x4E 0x44 0x4E 0x5D). Name byte arrays never decoded —
    opaque byte sequences preserved for all 7 language files without conversion.

  **Archive notes:**
  - The 3 binary files have unique short names in resources.assets →
    use `UnityArchiver.Extract(shortName)`.
  - `minista.mes` has 7 copies (one per language) — short name not unique →
    use `UnityArchiver.ExtractByPath("embeddedasset/text/{lang}/etc/minista.mes")`.
  - Mod output: binary files → `{modRoot}\embeddedasset\quadmist\{filename}`;
    name files → `{modRoot}\embeddedasset\text\{lang}\etc\minista.mes`.
  - No archive repacking — raw file write, same as enemies and field scripts.

- `StiltzkinsBag.Core/Models/TetraMasterRandomizerResult.cs` — New file.
  Sealed record: `(byte[] CardStats, byte[] CardSets, byte[] NpcDecks,
  IReadOnlyDictionary<string, byte[]>? CardNames)`.
  `CardNames` is null when `ShuffleCardOrder = false`. When non-null, keyed by
  language code (e.g. `"us"`, `"jp"`) — one entry per language supplied to the randomizer.

- `StiltzkinsBag.Core/Randomizers/TetraMasterRandomizer.cs` — New file.
  Constructor: `(Random rng, Settings settings)`.
  Public method: `Randomize(byte[] cardStats, byte[] cardSets, byte[] npcDecks,
  IReadOnlyDictionary<string, byte[]> cardNames) → TetraMasterRandomizerResult`.
  Master flag (`RandomizeTetraMaster`) is the caller's responsibility — not checked here.

  **RNG call order (strictly enforced):**

  | Step | Condition | Mode | RNG calls |
  |---|---|---|---|
  | 1 — Card stats | `RandomizeCardStats` | Shuffle: FY attack(99)+defence(99)+magicdefence(99) | 297 |
  | | | BoundedRandom: Next(min,max+1) ×3 per card (attack→defence→magicdefence) | 300 |
  | | | TierLock: sort by total sum ascending, assign to IDs 0→99 | 0 |
  | | | AllCardsMaxed (debug): all=255; downgrades to Shuffle if IsDebugMode=false | 0 |
  | 2 — Card types | `CardTypeMode != Preserve` | Shuffle: FY of type bytes | 99 |
  | | | AllP / AllM: set all to 0 / 1 | 0 |
  | 3 — Arrows | `ArrowMode != Preserve` | Random: Next(1,256) per card (≥1 bit guaranteed) | 100 |
  | | | Chaos: Next(0,256) per card (0x00 possible) | 100 |
  | | | AllDirections / NoArrows: 0xFF / 0x00 | 0 |
  | 4 — Card order | `ShuffleCardOrder` | FY permutation of [0..99], applied to stats + all language name arrays | 99 |
  | 5 — Card sets | `RandomizeCardSets` | Shuffle: FY of flat 1024-byte pool | 1023 |
  | | | BuildFromScratch: Next(0,100) per slot (outer=sets 0→63, inner=slots 0→15) | 1024 |
  | 6 — NPC deck set indices | `ShuffleNpcDecks` | FY of SetIndex across 256 entries | 255 |
  | 7 — NPC difficulty | `NpcDifficultyMode != Preserve` | Shuffle: FY of Difficulty bytes | 255 |
  | | | RaiseAll / LowerAll: all=3 / all=0 | 0 |

  **ShuffleCardOrder co-permutation:** `perm[newIndex] = oldIndex`. Same permutation
  applied to both card stat entries and every supplied language name byte array.
  Card set and NPC deck ID references remain valid after reorder — they index into
  the reordered stat table by position.

  **TierLock:** stable sort of all 100 entries by `Attack + Defence + MagicDefence`
  ascending. Stats from slot i of sorted result are assigned to card slot i. `Type`
  and `Arrows` of the original card at each slot are preserved (not relocated).

- `StiltzkinsBag.Tests/TetraMasterRandomizerTests.cs` — 52 new tests.

- `StiltzkinsBag.Tests/TestData/TetraMaster/` — 4 test data files (binary + mes):
  - `minigame_card_data_address` — 500 bytes (AlternateFantasy-modded install; stats reach 255)
  - `minigame_card_level_address` — 1024 bytes
  - `minigame_stage_address` — 512 bytes
  - `minista.mes` — 1369 bytes (US language)

---

## Key Discoveries This Phase

| Discovery | Impact |
|---|---|
| All three TetraMaster binary files live inside resources.assets, not standalone | UnityArchiver required for extraction. Short names for the 3 binary files are unique — `Extract(shortName)` works. `minista.mes` is NOT unique (7 language copies) — `ExtractByPath` with full `embeddedasset/text/{lang}/etc/` path required. |
| The fifth byte of each card entry is the arrow directional bitmask, not a point value | Hades Workshop calls it `points`, but all 100 cards in the test data have value `0x0A = 0b00001010` (bits 1 and 3 = two specific directions), which was set uniformly by AlternateFantasy. Vanilla PSX cards had varied arrow patterns. Field renamed to `Arrows` throughout. |
| AlternateFantasy raised all card stats from vanilla [0–15] range to values up to 255 | Test data is not vanilla. Tests do not hardcode specific stat values — they verify properties (multiset preservation, range bounds, monotonic ordering) that hold regardless of the input distribution. |
| NPC deck structure (minigame_stage_address) is not a simple `set*4+difficulty` table | Decks 0–127 use all 64 sets with a non-uniform difficulty distribution. Decks 128–255 repeat all 64 sets at difficulty 0 (rematch table). No simple formula maps deck index to NPC identity — the in-game NPC → deck assignment requires further research for the Phase 8 spoiler log. |
| TetraMasterSetEntry uses a class, not a record | `byte[]` fields break record equality. Class with defensive copy in constructor and `CardIds` property returning a clone is the correct pattern. `WriteCardSets` uses the public `CardIds` property (clone cost: 1024 bytes, once per run). |
| Texts.cpp/h not needed | The `.mes` format is raw byte sequences delimited by literal ASCII `[ENDN]` — no complex text encoding machinery required. Read/write is pure byte manipulation; decoding to string is never performed. |

---

## Test Count

| File | Tests | Notes |
|---|---|---|
| SeedEngineTests | 7 | Phase 1 |
| CsvRoundTripTests | 46 | Phase 2 + 2 Stats tests (Phase 5.5) |
| EnemyFileTests | 22 | Phase 3 |
| UnityArchiverTests | 11 | Phase 3 |
| EnemyCatalogAndResolverTests | 10 | Phase 3 |
| Misc (Phase 1) | 4 | Phase 1 |
| FieldParserTests | 53 | Phase 4 |
| ItemRemapperTests | 58 | Phase 4 |
| EnemyRandomizerTests | 39 | Phase 4 |
| FieldItemRandomizerTests | 29 | Phase 4 |
| CharacterRandomizerTests | 41 | Phase 5 |
| InitialItemsRandomizerTests | 39 | Phase 5.5 (29) + Phase 5.6 (10) |
| AbilityGemsRandomizerTests | 26 | Phase 5.5 |
| AbilityApRandomizerTests | 24 | Phase 5.5 |
| GearStatRandomizerTests | 26 | Phase 5.5 |
| ShopRandomizerTests | 37 | Phase 5.6 |
| SynthesisRandomizerTests | 40 | Phase 5.6 |
| TetraMasterRandomizerTests | 52 | Phase 5.7 |
| **Total** | **563** | **All passing** |

---

## Deviations

| Item | Plan | Actual | Reason |
|---|---|---|---|
| TetraMaster files are standalone (not in archives) | Initial analysis based on file listing before Cards.cpp was provided | All files are inside resources.assets — same UnityArchiver pattern as enemies/fields | Unity Assets Viewer screenshots confirmed archive location; Cards.cpp Load() method confirmed `resources.assets` as the source file |
| "Points" byte | Named `points` following HW source | Renamed `Arrows` — it is a directional bitmask | All 100 cards in test data have identical value `0x0A`; AF set them uniformly. Bit pattern analysis confirmed arrow semantics |
| NPC deck structure | Assumed simple `set*4+difficulty` ordering for all 256 entries | Structure is non-uniform: decks 0–127 use varied difficulty, decks 128–255 are a rematch table at difficulty 0 | Binary analysis of minigame_stage_address revealed the pattern; "perfect ordering" assertion failed during pre-planning analysis |
| Texts.cpp/h required for mes handling | Provided as reference expecting complex text format | Not needed — mes format is trivial byte split/join on `[ENDN]` | Reading the actual bytes revealed pure ASCII delimiter with no encoding complexity |
| GoblinArmy, MirrorStats, AllX, AllA sub-options | Proposed in planning | Removed by developer preference | Replaced with TierLock, BuildFromScratch, AllP/AllM, combined arrow/type approach |
| 52 tests | Not explicitly estimated | 52 | 4 round-trip + 11 CardStats + 4 CardType + 6 Arrow + 5 CardOrder + 6 CardSets + 9 NpcDecks + 1 passthrough + 2 combinability |

---

## Rules Added This Phase

| Rule | Reason |
|---|---|
| TetraMaster binary files (minigame_card_data_address, minigame_card_level_address, minigame_stage_address) are inside resources.assets. Extract using `UnityArchiver.Extract(shortName)` — short names are unique for these three files. | Confirmed from Unity Assets Viewer screenshots and Cards.cpp source. Mod output goes to `{modRoot}\embeddedasset\quadmist\` as raw files — no repacking. |
| minista.mes card name files are inside resources.assets but are NOT unique by short name (7 language copies). Always use `UnityArchiver.ExtractByPath("embeddedasset/text/{lang}/etc/minista.mes")`. | Short name "minista.mes" appears 7 times in the archive — one per language. ExtractByPath with the full asset path is required for disambiguation. |
| The fifth byte of each TetraMaster card entry is the arrow directional bitmask (8 bits = 8 directions), not a point value. Never name or treat it as "points". | HW names it `points` but binary analysis confirms directional semantics. All AF cards set to 0x0A; vanilla PSX cards had varied patterns. |

---

## Parking Lot Additions This Phase

| Topic | Target | Notes |
|---|---|---|
| NPC deck → in-game NPC mapping | Phase 8 | The 256 deck indices do not map to NPCs via a simple formula. Research which NPC encounters correspond to which deck indices before implementing the spoiler log entry for TetraMaster. |
| Sub-option backtrack for Phase 1–5 randomizers | After Phase 5.x complete | Carried from Phase 5.5. Review EnemyRandomizer, FieldItemRandomizer, CharacterRandomizer, etc. for sub-option additions once all Phase 5.x work is done. |
| Phase 5.8 — VanillaItemCatalog + ItemPool | Phase 5.8 | Carried from Phase 5.6. Full obtainability system. Boss battle list prerequisite still pending from developer. |
| Boss battle list research | Phase 5.8 prerequisite | Carried from Phase 5.6. Developer to provide list of all single-encounter / boss battle enemy folder paths. |

---

## Commit Message

```
feat: Phase 5.7 complete — TetraMaster randomizer

Settings.cs:
  - CardStatMode enum: Shuffle, BoundedRandom, TierLock, AllCardsMaxed (debug)
  - CardTypeMode enum: Preserve, Shuffle, AllP, AllM
  - ArrowMode enum: Preserve, Random, AllDirections, NoArrows, Chaos
  - CardSetMode enum: Shuffle, BuildFromScratch
  - NpcDifficultyMode enum: Preserve, Shuffle, RaiseAll, LowerAll
  - 14 new properties in TetraMaster section (6 sub-sections)
  - Old stub (4 flat properties) replaced

TetraMasterFile:
  - TetraMasterCardEntry record (attack|type|defence|magicdefence|arrows — 5 bytes)
  - TetraMasterSetEntry class (16 card IDs, defensive copy)
  - TetraMasterDeckEntry record (setIndex|difficulty — 2 bytes)
  - ReadCardStats/WriteCardStats (100 entries, 500 bytes)
  - ReadCardSets/WriteCardSets (64 sets × 16 slots, 1024 bytes)
  - ReadNpcDecks/WriteNpcDecks (256 entries, 512 bytes)
  - ReadCardNames/WriteCardNames ([ENDN] byte delimiter, language-agnostic)

TetraMasterRandomizerResult: CardStats + CardSets + NpcDecks + CardNames (nullable)

TetraMasterRandomizer — 7 sub-steps in strict RNG order:
  1. CardStats: Shuffle(297) | BoundedRandom(300) | TierLock(0) | AllCardsMaxed(0,debug)
  2. CardTypes: Shuffle(99) | AllP/AllM(0)
  3. Arrows: Random(100,≥1bit) | Chaos(100) | AllDirections/NoArrows(0)
  4. CardOrder: FY permutation(99) applied to stats + all language name arrays
  5. CardSets: Shuffle(1023) | BuildFromScratch(1024)
  6. NpcDecks set indices: FY(255)
  7. NpcDifficulty: Shuffle(255) | RaiseAll/LowerAll(0)

Key discoveries:
  - All files inside resources.assets (not standalone)
  - Fifth byte = arrow bitmask, not points
  - NPC deck structure non-uniform (not set*4+diff)
  - minista.mes not unique by short name — ExtractByPath required

TetraMasterRandomizerTests: 52 tests
TestData/TetraMaster/: 4 files (3 binary + 1 mes, AF-modded install)
563/563 tests passing (+52 this phase)
3 new rules added
```

---

## PhaseEnd Changelog

```
v1.5.6 → v1.5.7
- Build Log: Phase 5.7 entry added
- Key Discoveries: resources.assets archive, arrow bitmask, AF stat range,
  non-uniform deck structure, TetraMasterSetEntry class pattern, Texts.h not needed
- Deviations: archive location, arrows rename, deck structure, Texts.h, removed
  sub-options, test count
- Rules: 3 new rules (binary files in resources.assets, minista.mes ExtractByPath,
  fifth byte is arrows)
- Parking Lot: NPC deck mapping, sub-option backtrack (carried), Phase 5.8 (carried),
  boss battle list (carried)
- Phase 5.7 marked complete
- Current phase: 5.8 — VanillaItemCatalog + ItemPool
```

---

## 🛑 Stop Here

This chat session is complete. Do the following before starting Phase 5.8:

1. Add `PhaseEnd_Phase5_7.md` to your Claude Project
2. Start a new chat session
3. Say: "Continue building Stiltzkin's Bag. Read all attached markdown files."

Do not continue development in this session. Do not delete this chat — keep it for
posterity and back-reference. Large chats in a project will not slow down chat or use more tokens.
