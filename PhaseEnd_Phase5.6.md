# PhaseEnd — Phase 5.6: Shop + Synthesis Randomizers
**Date:** 2026-03-27
**Project Version:** 1.5.5 → 1.5.6
**Phase Status:** ✅ Complete

---

## Checklist Verification

| Item | Status | Notes |
|---|---|---|
| Settings.cs audit | ✅ | 4 new enums: ShopItemPool, ShopMode, ShopSizeMode, SynthesisPriceMode. FilthyRich + JunkDrawer added to StartingItemMode. New shop/synthesis/challenge modifier property blocks. Old ShopIncludeMedicItems + ShopOverrideMedicShops + RandomizeBonusSets removed. |
| `ShopRandomizer.cs` + `ShopRandomizerResult` | ✅ | Shuffle, BoundedRandom, MegaMart (debug); ShopSizeMode; ShortSupply cap; EnsureMedicItems post-pass; BadEconomy global Items.csv price pass |
| `ShopRandomizerTests.cs` | ✅ | 37 tests passing |
| `SynthesisRandomizer.cs` + `SynthesisRandomizerResult` | ✅ | RandomizeSynthesisResults, RandomizeSynthesisIngredients, AllowNewSynthesisResults independent flags; 4 price modes; BadEconomy multiplier; Chaos override |
| `SynthesisRandomizerTests.cs` | ✅ | 40 tests passing |
| `InitialItemsRandomizer.cs` — FilthyRich + JunkDrawer | ✅ | Two new pool builders; KeyItemPriceThreshold constant added; graceful fallbacks for null/empty pools |
| `InitialItemsRandomizerTests.cs` — new mode tests | ✅ | 10 new tests added (39 total) |
| FilthyRich + JunkDrawer parking lot items resolved | ✅ | Both modes implemented and tested |
| BadEconomy discovery documented | ✅ | Global Items.csv price modifier — see Key Discoveries |
| Conservative pool documented | ✅ | Phase 5.8 VanillaItemCatalog will harden obtainability constraints |
| **Milestone: all Phase 5.6 randomizers complete and deterministic** | ✅ | 511/511 tests passing |

---

## Build Log

**Files created or replaced this phase:**

- `StiltzkinsBag.Core/Models/Settings.cs` — Phase 5.6 audit additions:
  - `ShopItemPool` enum: ConsumablesOnly, AllNonKeyItems
  - `ShopMode` enum: Shuffle, BoundedRandom, MegaMart (debug-gated)
  - `ShopSizeMode` enum: Maintain, Random, Fixed
  - `SynthesisPriceMode` enum: Vanilla, BoundedRandom, ProportionalScale, GearScored
  - `StartingItemMode` enum: FilthyRich + JunkDrawer inserted between SpeedRunner and AllItems
  - New properties — Shops section: ShopMode, ShopSizeMode, ShopFixedSize, ShopItemPool,
    ShopEnsureMedicItems (bool, default true), ShopMedicMinShops (int, default 2)
  - New properties — Synthesis section: RandomizeSynthesisResults, RandomizeSynthesisIngredients,
    AllowNewSynthesisResults, SynthesisPriceMode, SynthPriceMin (100), SynthPriceMax (5000),
    SynthPriceScaleMinPercent (50), SynthPriceScaleMaxPercent (200)
  - New section — Challenge Modifiers: BadEconomy (bool), BadEconomyPriceMultiplierMin (1.5f),
    BadEconomyPriceMultiplierMax (4.0f), ShortSupply (bool), ShortSupplyMaxItems (int, default 3)
  - New properties — Items section: FilthyRichThreshold (int, default 200),
    JunkDrawerThreshold (int, default 50)
  - Removed: ShopIncludeMedicItems, ShopOverrideMedicShops (superseded by EnsureMedicItems system),
    RandomizeBonusSets (resolved in Phase 5.5 as GearStatRandomizer)
  - IsDebugMode doc comment updated to include MegaMart

- `StiltzkinsBag.Core/Randomizers/ShopRandomizer.cs` — New file.
  - `ShopRandomizerResult` record: `(List<ShopItemsRow> Shops, List<ItemsRow> Items)`
  - Pipeline order: ComputeTargetSizes → ShortSupply cap → Populate (Shuffle/BoundedRandom/MegaMart)
    → EnsureMedicItems post-pass → ApplyBadEconomy
  - Shuffle: Fisher-Yates of global vanilla pool, distributed without within-shop duplicates.
    Uses usedPositions + usedInShop HashSets. globalPool.Count − 1 RNG calls.
  - BoundedRandom: per-shop draw without replacement from eligible pool (ConsumablesOnly or
    AllNonKeyItems). 1 RNG call per item slot drawn.
  - MegaMart: single Fisher-Yates shuffle, all shops get first N items. Debug-gated, downgrades
    to Shuffle. Always uses AllNonKeyItems pool. eligiblePool.Count − 1 RNG calls.
  - ShopSizeMode.Maintain: vanilla count preserved. Random: 1 RNG call per non-empty shop.
    Fixed: 0 RNG calls.
  - ShortSupply: caps target sizes after ShopSizeMode computation. 0 RNG calls.
  - EnsureMedicItems: appends Potion (236) + PhoenixDown (240) to largest non-compliant shops.
    0 RNG calls — deterministic. PotionItemId=236, PhoenixDownItemId=240 as internal consts.
  - ApplyBadEconomy: modifies Items.csv Price globally. Items processed in Id order ascending.
    1 RNG call per item with Price > KeyItemPriceThreshold (2). Returns new cloned ItemsRow list.
    Input never mutated.
  - internal: ComputeTargetSizes, BuildEligiblePool, EnsureMedicItems, ApplyBadEconomy, consts

- `StiltzkinsBag.Tests/ShopRandomizerTests.cs` — 37 tests covering all modes, size modes,
  ShortSupply, EnsureMedicItems, BadEconomy, passthrough, determinism, no-mutation.

- `StiltzkinsBag.Core/Randomizers/SynthesisRandomizer.cs` — New file.
  - `SynthesisRandomizerResult` record: `(List<SynthesisRow> Recipes)`
  - Recipe Id and Shops arrays never modified — only Result and/or Ingredients change.
  - Chaos mode: forces RandomizeSynthesisResults, RandomizeSynthesisIngredients,
    AllowNewSynthesisResults all true regardless of individual flag values.
  - Pipeline order: ShuffleResults → ShuffleIngredients → ApplyPriceMode → ApplyBadEconomy.
    RNG calls in this exact order.
  - ShuffleVanillaResults: Fisher-Yates of existing result IDs. recipes.Count − 1 RNG calls.
  - ShuffleResultsFromPool (AllowNew): draw without replacement from eligible pool.
    1 RNG call per recipe. Cycles pool if exhausted (small pool safety).
  - AllowNew pool — Recommended: Price > 2 filter. Chaos: all items, no filter.
    Phase 5.8 will add full obtainability constraints.
  - ShuffleIngredients: Fisher-Yates of ingredient sets as whole arrays. recipes.Count − 1 RNG calls.
  - PriceMode.Vanilla: 0 RNG calls.
  - PriceMode.BoundedRandom: _rng.Next(min, max+1) per recipe. 1 RNG call per recipe.
  - PriceMode.ProportionalScale: NextDouble() per recipe. 1 RNG call per recipe.
  - PriceMode.GearScored: statSum = Dex+Str+Mag+Will from Stats.csv via BonusId.
    Price = 500 + statSum × 500. Fallback to vanilla price when allItems/statsRows null,
    BonusId=0, or result item not found. 0 RNG calls.
  - ApplyBadEconomy: 1 RNG call per recipe, Id order ascending. Modifies Synthesis.csv
    Price column directly (independent of Items.csv — synthesis has its own price column).
  - internal: BuildResultPool, ComputeGearScoredPrice, consts

- `StiltzkinsBag.Tests/SynthesisRandomizerTests.cs` — 40 tests covering all flags,
  Chaos override, pool building, all price modes with fallback cases, BadEconomy,
  structure preservation (Ids/Shops never modified), determinism.

- `StiltzkinsBag.Core/Randomizers/InitialItemsRandomizer.cs` — Updated.
  - FilthyRich pool: all items with Price >= FilthyRichThreshold. Key items always excluded
    (threshold enforced at max(KeyItemPriceThreshold+1, configured threshold)). Any item type
    eligible — goal is gil value, not item type. Falls back to ConsumablesRandom if _allItems
    null or pool empty.
  - JunkDrawer pool: consumables (IDs 236–253) with Price <= JunkDrawerThreshold AND Price > 0.
    Falls back to full ConsumablesRandom if _allItems null or pool empty.
  - KeyItemPriceThreshold = 2 constant added (matches ShopRandomizer).
  - BuildPool switch updated with two new cases.
  - All existing methods and comments preserved.

- `StiltzkinsBag.Tests/InitialItemsRandomizerTests.cs` — Updated.
  - 10 new tests added: FilthyRich (5) + JunkDrawer (5).
  - FakePricedItems helper added for price-filter testing.
  - All 29 existing tests preserved exactly.

---

## Key Discoveries This Phase

| Discovery | Impact |
|---|---|
| ShopItems.csv has no price column | Shop prices are a global property of Items.csv. BadEconomy cannot be applied per-shop — it must modify Items.csv globally. ShopRandomizer.Randomize() returns ShopRandomizerResult carrying both modified shops and modified items list. Design pivot mid-Task 2; return type changed from List<ShopItemsRow> to ShopRandomizerResult. |
| BadEconomy applies globally to Items.csv buy prices AND sell prices | This is actually the correct behavior — economy inflation should affect everything consistently. Player sell values also increase, which creates interesting strategic interactions. |
| Synthesis.csv has its own Price column (independent of Items.csv) | SynthesisRandomizer.BadEconomy modifies Synthesis.csv Price directly. SynthesisPriceMode randomization is fully independent of item price randomization. No conflict between the two systems. |
| Conservative pool baseline is sufficient for Phase 5.6 | Price > 2 filter excludes story/key items reliably. Full obtainability constraints (unique item caps, boss-drop awareness, finite ingredient chain detection) require VanillaItemCatalog (Phase 5.8). Documented in both randomizer class XML docs. |
| Item equippability flags (per-character bool columns in Items.csv) are out of scope for ShopRandomizer | These columns (Zidane, Vivi, Garnet, etc.) control who can equip an item. Shuffling them belongs in the character/equipment pipeline, not shop randomization. Parked for future phase. |

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
| **Total** | **511** | **All passing** |

---

## Deviations

| Item | Plan | Actual | Reason |
|---|---|---|---|
| ShopRandomizer return type | `List<ShopItemsRow>` | `ShopRandomizerResult(Shops, Items)` | ShopItems.csv has no price column — BadEconomy must modify Items.csv globally. Return type expanded to carry both outputs. |
| BadEconomy scope | Shop-only price modifier | Global Items.csv price modifier | Developer confirmed global scope mid-Task 2 after discovery that ShopItems.csv has no price column. Synthesis has its own Price column and handles BadEconomy independently. |
| SynthesisRandomizer — "Bonus Sets" | Originally listed as Phase 5.6 scope | Not applicable | Resolved in Phase 5.5 deviation: "Bonus Sets" was GearStatRandomizer (Stats.csv), not a synthesis concept. |
| Phase 5.8 — VanillaItemCatalog | Originally unlisted | Formally added as new phase | Obtainability system design session this phase established that a full VanillaItemCatalog is required before Recommended mode constraints can be enforced on shops and synthesis. Phase 5.8 added to roadmap. |

---

## Rules Added This Phase

| Rule | Reason |
|---|---|
| All item pools used by randomizers must be traceable to an obtainability source. In Recommended mode, only items with known finite or infinite obtainability may be placed in chests, shops, enemy drops, or synthesis results. Unique items (finite obtainability = 1) may not be placed in infinite-source slots (shops, repeatable enemy drops, synthesis with common ingredients). This constraint is enforced by VanillaItemCatalog (Phase 5.8). Until Phase 5.8 is complete, all randomizers use conservative pools (Price > 2 filter) as a safe baseline. | Design session this phase established that item obtainability is a first-class constraint for Recommended mode. Documented here so all future randomizers are built with this in mind. |
| Item equippability flag shuffling (per-character bool columns in Items.csv: Zidane, Vivi, Garnet, etc.) belongs in the character/equipment pipeline, not in ShopRandomizer or any item-content randomizer. | Surfaced during ShopRandomizer design. These flags control game balance and character identity — they are character concerns, not item-content concerns. |

---

## Parking Lot Additions This Phase

| Topic | Target | Notes |
|---|---|---|
| Phase 5.8 — VanillaItemCatalog + ItemPool | Phase 5.8 (new) | Full obtainability system. Scan fields (FieldParser), battles (EnemyCatalog), shops (ShopItemsRow), synthesis (SynthesisRow) to build per-item MaxObtainable counts. Boss battle list to be provided by developer. RecommendedLogicEngine (Phase 6) consumes this catalog for guardrail enforcement. |
| Boss battle list research | Phase 5.8 prerequisite | Developer confirmed will provide a list of all single-encounter / boss battle enemy folder paths. Required for VanillaItemCatalog to distinguish finite boss drops from infinite regular enemy drops. |
| Item equippability flag randomization | Future (character pipeline phase) | Per-character bool columns in Items.csv (Zidane, Vivi, Garnet, etc.). Belongs with CharacterRandomizer or a dedicated EquipmentRandomizer pass. |
| Save the Queen weapon — add unique copy to pool | Weapon randomizer phase | Save the Queen is listed as an item but only Beatrix can equip it in vanilla. Add one unique copy to the weapon pool with MaxCopies=1 when weapon shuffling is implemented. |
| Sub-option backtrack for Phase 1–5 randomizers | After Phase 5.x complete | Carried from Phase 5.5. Review EnemyRandomizer, FieldItemRandomizer, CharacterRandomizer, etc. for sub-option additions once all Phase 5.x work is done. |

---

## Commit Message

```
feat: Phase 5.6 complete — shop + synthesis randomizers

Settings.cs:
  - ShopItemPool enum: ConsumablesOnly, AllNonKeyItems
  - ShopMode enum: Shuffle, BoundedRandom, MegaMart (debug-gated)
  - ShopSizeMode enum: Maintain, Random, Fixed
  - SynthesisPriceMode enum: Vanilla, BoundedRandom, ProportionalScale, GearScored
  - StartingItemMode: FilthyRich + JunkDrawer added
  - Challenge modifiers: BadEconomy (+min/max float), ShortSupply (+max items)
  - Shop section: ShopMode, ShopSizeMode, ShopFixedSize, ShopItemPool,
    ShopEnsureMedicItems, ShopMedicMinShops
  - Synthesis section: RandomizeSynthesisResults, RandomizeSynthesisIngredients,
    AllowNewSynthesisResults, SynthesisPriceMode, SynthPrice(Min/Max/ScaleMin/ScaleMax)
  - Removed: ShopIncludeMedicItems, ShopOverrideMedicShops, RandomizeBonusSets

ShopRandomizer + ShopRandomizerResult:
  - Returns (Shops, Items) — BadEconomy modifies Items.csv globally
  - Shuffle: Fisher-Yates global pool, no within-shop duplicates
  - BoundedRandom: per-shop independent draw without replacement
  - MegaMart: single shuffle, all shops same list, debug-gated
  - ShopSizeMode: Maintain/Random/Fixed; ShortSupply cap applied after
  - EnsureMedicItems: deterministic post-pass, 0 RNG calls
  - ApplyBadEconomy: global Items.csv price pass, 1 RNG/item, Id order
  - Key discovery: ShopItems.csv has no price column — global Items.csv only

SynthesisRandomizer + SynthesisRandomizerResult:
  - RandomizeSynthesisResults + RandomizeSynthesisIngredients: independent flags
  - AllowNewSynthesisResults: draws from item pool (Price>2 filter in Recommended)
  - Chaos mode: all three flags forced true regardless of settings
  - PriceMode.GearScored: 500 + statSum*500 from Stats.csv via BonusId
  - ApplyBadEconomy: Synthesis.csv Price column (independent of Items.csv)
  - Recipe Ids + Shops never modified

InitialItemsRandomizer:
  - FilthyRich: Price >= threshold pool, key items excluded, fallback on empty
  - JunkDrawer: consumables 236-253 with Price <= threshold AND Price > 0
  - KeyItemPriceThreshold=2 constant added

511/511 tests passing (+87 this phase)
New rules: item obtainability constraint, equippability flag placement
Phase 5.8 (VanillaItemCatalog) added to roadmap
```

---

## PhaseEnd Changelog

```
v1.6.0 → v1.7.0
- Build Log: Phase 5.6 entry added
- Key Discoveries: ShopItems.csv price discovery, BadEconomy global scope,
  synthesis independent price column, conservative pool baseline,
  item equippability flags placement
- Deviations: ShopRandomizerResult return type, BadEconomy global scope,
  Bonus Sets resolved, Phase 5.8 added
- Rules: 2 new rules (obtainability constraint, equippability flag placement)
- Parking Lot: Phase 5.8 VanillaItemCatalog, boss battle list, equippability
  flags, Save the Queen weapon, sub-option backtrack (carried)
- Phase 5.6 marked complete
- Current phase: 5.7 — TetraMaster Randomizer (bytecode)
```

---

## 🛑 Stop Here

This chat session is complete. Do the following before starting Phase 5.7:

1. Add `PhaseEnd_Phase5.6.md` to your Claude Project
2. Start a new chat session
3. Say: "Continue building Stiltzkin's Bag. Read all attached markdown files."

Do not continue development in this session. Do not delete this chat — keep it for
posterity and back-reference. Large chats in a project will not slow down chat or use more tokens.
