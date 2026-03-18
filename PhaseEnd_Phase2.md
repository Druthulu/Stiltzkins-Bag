# PhaseEnd — Phase 2 CSV I/O Layer
**Date:** 2026-03-17
**Project Version:** 1.2.0
**Phase Status:** Complete

---

## Build Log

**Files created and complete — do not recreate:**

- `StiltzkinsBag.Core/Parsing/CsvParser.cs` — Generic round-trip CSV parser for all Memoria Engine CSV formats. `MemoriaCsvParser.Read<T,TMap>` and `Write<T,TMap>` methods. Handles: semicolon delimiter, leading `#`/`#!` comment block preservation, inline `;#` comment stripping and re-emission, UTF-8 BOM detection and preservation, UTF-8 no-BOM, cp1252 auto-detection via strict UTF-8 validation fallback, CRLF/LF line ending detection and preservation, tab-padded whitespace trimming (CommandSets), array columns via custom type converters. `ParsedCsv<T>` carrier holds SourcePath, Encoding, LineEnding, CommentLines, Rows, InlineComments. Type converters: `IntArrayConverter`, `AbilityRefArrayConverter`, `StringArrayConverter`, `MemoriaBoolConverter`.
- `StiltzkinsBag.Core/Models/Csv/BaseStatsRow.cs` — `BaseStatsRow` + `BaseStatsRowMap`. Columns: Comment, Id, Dexterity, Strength, Magic, Will, Gems.
- `StiltzkinsBag.Core/Models/Csv/CharacterParametersRow.cs` — `CharacterParametersRow` + `CharacterParametersRowMap`. Columns: Id, DefaultRow, DefaultWinPose, DefaultCategory, DefaultCommandSet, DefaultEquipmentSet, BattleParameterFormula, NameKeyword. Bool columns use `MemoriaBoolConverter`.
- `StiltzkinsBag.Core/Models/Csv/CommandSetsRow.cs` — `CommandSetsRow` + `CommandSetsRowMap`. Columns: Id, Attack, Defend, Regular1, Regular2, Item, Change, AttackTrance, DefendTrance, Trance1, Trance2, ItemTrance, ChangeTrance. Note: tab padding in source cannot be round-tripped; content round-trip is used instead of byte round-trip for this file.
- `StiltzkinsBag.Core/Models/Csv/DefaultEquipmentRow.cs` — `DefaultEquipmentRow` + `DefaultEquipmentRowMap`. Columns: Comment, Id, Weapon, Head, Wrist, Armor, Accessory. cp1252 encoded.
- `StiltzkinsBag.Core/Models/Csv/CharacterAbilityRow.cs` — `CharacterAbilityRow` + `CharacterAbilityRowMap`. Shared model for all 16 per-character ability CSVs. Columns: AbilityRef (string, e.g. "AA:101"), AP.
- `StiltzkinsBag.Core/Models/Csv/AbilityGemsRow.cs` — `AbilityGemsRow` + `AbilityGemsRowMap`. Columns: Comment, Id, Gems, BoostedVersions (int[]). cp1252 encoded.
- `StiltzkinsBag.Core/Models/Csv/InitialItemsRow.cs` — `InitialItemsRow` + `InitialItemsRowMap`. Columns: ItemID, Count. No Comment column. UTF-8 BOM.
- `StiltzkinsBag.Core/Models/Csv/ItemsRow.cs` — `ItemsRow` + `ItemsRowMap`. 32 columns including AbilityIds (string[] via `AbilityRefArrayConverter`), 20 bool columns via `MemoriaBoolConverter`, Quality and Order as float. UTF-8 BOM.
- `StiltzkinsBag.Core/Models/Csv/WeaponsRow.cs` — `WeaponsRow` + `WeaponsRowMap`. Columns: Comment, Id, Category, StatusIndex, Model, ScriptId, Power, Elements, Rate, Offset1, Offset2, HitSfx, CustomTexture (string[]). cp1252 encoded.
- `StiltzkinsBag.Core/Models/Csv/ArmorsRow.cs` — `ArmorsRow` + `ArmorsRowMap`. Columns: Comment, Id, PDef, PEva, MDef, MEva. cp1252 encoded.
- `StiltzkinsBag.Core/Models/Csv/ShopItemsRow.cs` — `ShopItemsRow` + `ShopItemsRowMap`. Columns: Comment, Id, Items (int[]). Empty shops parse to empty array.
- `StiltzkinsBag.Core/Models/Csv/SynthesisRow.cs` — `SynthesisRow` + `SynthesisRowMap`. Columns: Comment, Id, Shops (int[]), Price, Result, Ingredients (int[]). cp1252 encoded.
- `StiltzkinsBag.Tests/CsvRoundTripTests.cs` — 51 unit tests covering round-trip byte identity for all 13 CSV types (CommandSets uses content round-trip), known-value assertions for representative rows, inline comment preservation, comment block preservation, BOM handling, empty array columns (empty shops), encoding registration in constructor.
- `StiltzkinsBag.Tests/TestData/` — 33 CSV files copied from game installation for use by round-trip tests only. Never shipped with the app.

**Test results:** 51/51 passing (7 SeedEngineTests + 44 CsvRoundTripTests).

**Milestone achieved:** All Memoria CSV files required by Gen1 randomizers have typed models and clean round-trip read/write. Parser handles all encoding and format variants found in the real game files.

**Deferred to Phase 7:** `GamePathLocator` (registry auto-detect) and game path UI wiring. No Phase 3–6 component depends on the game path at build time.

**Next:** Phase 3 — Structured Binary Data Layer. Port Hades Workshop C++ parsers (UnityArchiver, EnemyParser, FieldParser, MipsEditor) to C#.

---

## Deviations

| Item | Plan | Actual | Reason |
|---|---|---|---|
| `GamePathLocator` | Phase 2 | Deferred to Phase 7 | No Phase 3–6 component needs game path; cleaner to bundle with full UI wiring |
| `CommandSets` round-trip | Byte-identical | Content-identical | Source file uses tab padding between fields which a CSV writer cannot preserve; all data values round-trip correctly |
| cp1252 encoding detection | Caller passes encoding | Auto-detected via strict UTF-8 validation fallback | Simpler API; detection is reliable for the Memoria file set |
| `Encoding.RegisterProvider` | Not anticipated | Required in both `CsvParser.cs` and test constructor | .NET Core does not include cp1252 by default; `CodePagesEncodingProvider.Instance` must be registered |

---

## Rules Added This Phase

| Rule | Reason |
|---|---|
| (none) | No new project-wide rules added this phase |

---

## Commit Message

```
feat: Phase 2 complete — CSV I/O layer, all typed models, round-trip tests

- CsvParser<T>: MemoriaCsvParser.Read/Write with full Memoria format support
  - Semicolon delimiter, # comment block preservation, inline ;# comment preservation
  - UTF-8 BOM / UTF-8 no-BOM / cp1252 auto-detection via strict UTF-8 fallback
  - CRLF/LF line ending detection and preservation
  - TrimOptions for tab-padded files (CommandSets)
  - Type converters: IntArrayConverter, AbilityRefArrayConverter,
    StringArrayConverter, MemoriaBoolConverter (0/1 not True/False)
- Typed CSV models (13 files): BaseStatsRow, CharacterParametersRow,
  CommandSetsRow, DefaultEquipmentRow, CharacterAbilityRow (shared x16),
  AbilityGemsRow, InitialItemsRow, ItemsRow, WeaponsRow, ArmorsRow,
  ShopItemsRow, SynthesisRow — all with ClassMaps
- CsvRoundTripTests: 44 tests, 51 total passing (7 seed + 44 csv)
  - Byte-identical round-trip for all CSV types except CommandSets (content)
  - Known-value assertions, BOM, inline comments, encoding, empty arrays
- TestData/: 33 game CSV files for test use only, never shipped
- GamePathLocator deferred to Phase 7 (no Phase 3-6 dependency)
- Encoding.RegisterProvider(CodePagesEncodingProvider.Instance) required
  for cp1252 on .NET Core — added to CsvParser and test constructor
```

---

## PhaseEnd Changelog

```
v1.1.0 → v1.2.0
- Build Log: Phase 2 entry added
- Deviations: GamePathLocator deferral, CommandSets tab padding,
  cp1252 auto-detection, RegisterProvider requirement
- Phase 2 marked complete
- Current phase: 3 — Structured Binary Data Layer
```

---

## 🛑 Stop Here

This chat session is complete. Do the following before starting Phase 3:

1. Add `PhaseEnd_Phase2.md` to your Claude Project
2. Start a new chat session
3. Say: "Continue building Stiltzkin's Bag. Read PhaseEnd Files to get up to date on next steps."

Do not continue development in this session.

You do not need to delete this chat — keep it for posterity and back-reference. Old chats do not affect speed or token usage.
