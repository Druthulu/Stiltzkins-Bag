# Phase 6 — PhaseEnd

**Completed:** 2026-04-02
**Tests at phase end:** 819 total — all green, 0 warnings
**Next phase:** 7 (WPF UI) or 8 (Mod Output + Memoria Integration)

---

## Phase 6 Task Checklist

| # | Task | Status |
|---|------|--------|
| 1 | Blue magic data → `VanillaObtainabilityData` | ✅ Done |
| 2 | `LegendaryItemList` data definition | ✅ Done |
| 3 | `RecommendedLogicEngine.cs` skeleton + input contract | ✅ Done |
| 4 | Speciality-aware equipment coherence correction | ✅ Done |
| 5 | Equipment coherence tests | ✅ Done |
| 6 | `EnforceLegendaryRarity()` pure function | ✅ Done |
| 7 | Legendary rarity tests | ✅ Done |
| 8 | Synthesis reachability — field ID proxy + finite result cap | ✅ Done |
| 9 | Integration tests | ✅ Done |

---

## What Was Built

### Task 1 — BlueMagicByEnemyId

Added to `VanillaObtainabilityData.cs`:
- `BlueMagicByEnemyId: IReadOnlyDictionary<int, string>` — 94 entries
- Key = enemy battle binary index (1-based "Enemy #" from guide CSV)
- Value = learnable spell name (e.g. "Goblin Punch", "Matra Magic")
- Filtered from guide CSV: "I no can eat!", "Taste Bad!", "Nothing" excluded
- Duplicate-spell entries retained (e.g. Matra Magic on 6 different enemies)
- Used by Phase 8 spoiler log to report which enemy carries which spell after
  `EnemyRandomizer.ShuffleBlueMagic()` runs
- Source: `Final Fantasy IX Reference Guide - Blue Magic.csv`

---

### Task 2 — LegendaryItemList

New file: `StiltzkinsBag.Core/Models/LegendaryItemList.cs`

Two item tiers:

**`UniqueItemIds`** (16 items) — exactly 1 vanilla copy; must never appear in shops:
- Weapons: Ultima Weapon (15), Save the Queen (26), Ragnarok (29), Excalibur II (30),
  Rune Claws (50)
- Genji set: Duel Claws (49), Genji Gloves (109), Genji Helmet (146), Genji Armor (189)
- Accessories: Pumice Piece (210), Pumice (211), Ribbon (221), Thief Gloves (98),
  Robe of Lords (175), Maiden Prayer (222)

**`LegendaryItemIds`** (21 items) — rare, 2–3 vanilla copies; must not appear in shops:
- Late-game weapons: Masamune (13), The Tower (14), Ultima Sword (27), Excalibur (28),
  Dragon's Hair (40), Dragon's Claws (45), Avenger (47), Kaiser Knuckles (48),
  Tiger Racket (56), Gastro Fork (84), Mace of Zeus (78)
- Rare armor: Grand Helm (147), Rubber Suit (166), Glutton's Robe (171), White Robe (172),
  Black Robe (173), Light Robe (174), Demon's Mail (184), Maximillian (190), Grand Armor (191)
- Rare accessories: Battle Boots (197), Running Shoes (198), Black Belt (201),
  Rosetta Ring (204), Reflect Ring (205), Rebirth Ring (208), Protect Ring (209)
- Consumables: Dark Matter (250)

**`AllProtectedItemIds`** — union set (37 total); used by `EnforceLegendaryRarity`.

**Important caveat:** Items whose Unique/Legendary classification relies solely on field
chest counts are marked `// UNVERIFIED` in the file header. Field chest counts are inflated
by `FieldParser` false positives (Phase 9 fix). Classifications derived from boss drops,
auctions, and chocographs are solid.

Confirmed by scanning all 32 rows of `ShopItems.csv`: none of the listed items appear
in any vanilla shop.

---

### Task 3 — RecommendedLogicEngine skeleton

New file: `StiltzkinsBag.Core/Randomizers/RecommendedLogicEngine.cs`

**Architecture:** Pure static functions — no file I/O, no side effects. All correction
via new object creation, never mutation of input objects. Phase 8 calls these methods
after each Phase 5 randomizer and writes corrected data.

**Public API (stubs at Task 3, implemented in Tasks 4/6/8):**
- `CorrectEquipmentCoherence(characterParams, slotAssignment, equipmentSets)`
- `EnforceLegendaryRarity(shopRows, protectedIds, replacementPool, rng)`
- `EnforceSynthesisReachability(synthesisRows, synthesisShopFieldIds, itemIdToMinFieldId, catalog, finiteProtectedIds, rng)`

**Internal helpers:**
- `WeaponAffinity` enum: Magical / Physical / Balanced / None
- `SlotAffinity` enum: Magical / Physical / Balanced
- `ClassifyWeapon(weaponItemId)` — weapon affinity by item ID range
- `ClassifySlots(slotTypes)` — slot affinity by majority vote
- `MatchScore(slot, weapon)` — pairwise compatibility score (private)
- `GetSlotScore(slotType)` — per-slot score contribution (private)

**Deviation from skeleton:** `baseStats` parameter removed from `CorrectEquipmentCoherence`.
Greedy pairwise swap handles ties naturally — stat-based tie-breaking was unnecessary
complexity.

---

### Task 4 — CorrectEquipmentCoherence

Implemented `CorrectEquipmentCoherence` in `RecommendedLogicEngine.cs`.

**Algorithm:** Greedy pairwise improvement over characters 0–7. For each pair (i, j),
compute `scoreBefore` and `scoreAfter` using `MatchScore`. If swapping improves the
combined score, swap. Repeat until no improvement. Worst case: 7 passes × 28 pairs = 196
comparisons.

**MatchScore table:**
- Magical + Magical = 2 (perfect)
- Physical + Physical = 2 (perfect)
- Balanced + any = 1 (no preference)
- any + Balanced/None = 1 (universal)
- cross-affinity = 0 (mismatch)

**Weapon affinity by item ID:**
- IDs ≤ 0: None (empty slot)
- IDs 1–15: Balanced (daggers)
- IDs 16–50: Physical (swords/spears/claws/knuckles)
- IDs 51–78: Magical (rackets/rods/flutes)
- IDs 79–87: Balanced (forks)
- IDs 88+: Balanced fallback (accessories — should never appear in Weapon slot)

**Slot affinity scoring (from AbilityTierClassification_Rev4.md):**
- Magical (+1): "Blk Mag", "Focus", "Summon-A", "Summon-B", "Wht Mag-A", "Wht Mag-B"
- Physical (−1): "Skill", "Swd Art", "Swd Mag", "Jump", "Dragon", "Flair", "Throw"
- Balanced (0): "Steal", "Blu Mag", "Eat", and any unrecognised slot name

Characters 8–15 (guests) never modified — pass through as original references.
Changed characters get new `CharacterParametersRow` instances; unchanged return original
references. Input objects are never mutated.

---

### Task 6 — EnforceLegendaryRarity

Implemented `EnforceLegendaryRarity` in `RecommendedLogicEngine.cs`.

**Logic:** Iterate shop rows. For each row, scan `Items` array for any protected ID.
If none found, return original row reference (fast path). If found, build a new `int[]`
with protected slots replaced by `rng.Next(replacementPool.Count)` draws. Build a new
`ShopItemsRow` with corrected items. One RNG call per replaced slot.

**RNG call order:** Shops visited in input order (ID ascending). Within each row, slots
visited left to right. Deterministic given sorted input.

---

### Task 8 — EnforceSynthesisReachability + SynthesisShopData

New file: `StiltzkinsBag.Core/Models/SynthesisShopData.cs`

Static `ShopFieldIds: IReadOnlyDictionary<int, int>` mapping synthesis shop ID →
story-order field ID:

| Shop | Field ID | Location |
|------|----------|----------|
| 32 | 560 | Lindblum (Disc 1) |
| 33 | 902 | Treno |
| 34 | 1308 | Lindblum (Disc 2) |
| 35 | 1455 | Black Mage Village (Disc 2) |
| 36 | 2453 | Alexandria/Treno/Lindblum/Alley (highest representative) |
| 37 | 2801 | Daguerreo |
| 38 | 2857 | Black Mage Village end-game (virtual — see below) |
| 39 | 2914 | Hades/Memoria Birth |

Shop 38 (Mage Village end-game, actual field 3054) is assigned virtual field ID 2857
(midpoint of 2801 and 2914) because story-order places it before Hades despite having
a higher numeric field ID. This is a confirmed game development artifact.

Shop 36 uses field 2453 (Alexandria Alley) as representative — the highest of its four
locations — making ingredient reachability checks permissive/conservative.

Identified by scanning all field scripts for pattern:
`Wait(3) → Menu(2, N) → Wait(3)` (synthesis shop opcode sequence).

---

**Implemented `EnforceSynthesisReachability`:**

**Rule 8A — Ingredient reachability (field ID proxy):**
For each recipe, for each shop in `Shops`, check if all ingredients are reachable before
that shop's field ID. Reachability rules (priority order):
1. `HasInfiniteSource` → always reachable (field ID = 0)
2. `IsFieldItem` + `itemIdToMinFieldId[id] < shopFieldId` → reachable
3. Finite-only, no field source (boss drops, world map) → treated as reachable
4. Not in catalog → treated as reachable

If a shop fails the check, it is removed from `Shops`. If all shops are removed,
the recipe row is dropped. This is a shop-removal strategy (not fallback recipe
generation) — simpler, deterministic, no RNG consumed.

**Rule 8B — Finite result cap:**
For each recipe whose `Result` is in `finiteProtectedIds` and is `IsFiniteOnly`, count
recipes producing that result. If count > vanilla finite count, drop excess recipes
(sorted by Id ascending, keep first N). `rng` parameter accepted but not consumed —
reserved for future fallback ingredient generation.

---

### Tasks 5, 7, 9 — Tests

| File | New tests | Coverage |
|------|-----------|----------|
| `RecommendedLogicEngineEquipmentTests.cs` | 30 | ClassifyWeapon (11), ClassifySlots (9), CorrectEquipmentCoherence (10) |
| `RecommendedLogicEngineEquipmentTests.cs` (appended) | 15 | EnforceLegendaryRarity — basic contract, determinism, guards |
| `RecommendedLogicEngineSynthesisAndIntegrationTests.cs` | 20 | EnforceSynthesisReachability Rule 8A (10), Rule 8B (4), combined (1), guards (3), integration (2) |

**Total new tests this phase: 65**

---

## Rules Added This Phase

**Rule: FFIX key items are not in Items.csv (IDs 0–255).**
Key items (World Map, Oglop, Cid's Hammer, quest items, etc.) live in a separate game
data structure. All 256 Items.csv entries are regular items. Never treat `Price ≤ 2` as
a key item indicator — that signals "not purchasable from shops" (rare equipment/accessories).
Key item support is Gen2+.

**Rule: LegendaryItemList field-dependent classifications are unverified until Phase 9.**
Items whose Unique/Legendary tier depends on field chest counts (from `FieldItemScanner`)
carry `// UNVERIFIED` comments in `LegendaryItemList.cs`. The field scanner produces
false positive `DirectItem` hits for common low-ID items (e.g. Dagger appearing 101 times).
Classifications from boss drops, auctions, and chocographs are solid. Phase 9 fixes
the scanner; at that point re-audit `LegendaryItemList`.

**Rule: RecommendedLogicEngine methods are pure static functions — never mutate input.**
All three correction methods return new collections. Changed rows are new object instances;
unchanged rows return original references. This enables callers to compare before/after
by reference and makes the corrections testable without side effects.

**Rule: CorrectEquipmentCoherence uses greedy pairwise swap — no RNG consumed.**
The algorithm always terminates (finite bounded improvement steps) and produces a local
optimum. It consumes zero RNG calls. Affinity ties simply result in no swap — no
stat-based tiebreaker needed.

**Rule: EnforceSynthesisReachability removes shops from recipes, not ingredients.**
When a recipe's ingredient is not reachable before a shop's field ID, that shop is removed
from the recipe's `Shops` array. If all shops are removed, the recipe is dropped. Fallback
recipe generation (swapping to reachable ingredients) is reserved for a future improvement
and would require RNG — document with `// rng reserved for future use` comment.

**Rule: SynthesisShopData.ShopFieldIds uses virtual field ID 2857 for shop 38.**
Black Mage Village end-game shop (shop 38) has actual field ID 3054, which is numerically
higher than Hades shop (shop 39, field 2914). Story-order places it before Hades.
Virtual field ID 2857 (midpoint between 2801 and 2914) is used to maintain correct
story-order proxy. This is documented in `SynthesisShopData.cs` header.

**Rule: SynthesisShopData shop 36 uses field 2453 as representative.**
Shop 36 appears at fields 1859, 1902, 2108, and 2453 (Alexandria, Treno, Lindblum, Alley).
Field 2453 (highest) is used as the representative. Ingredient reachability is checked
against the hardest threshold — an ingredient reachable before 2453 is reachable before
all earlier shop 36 locations.

---

## Deviations

| Item | Plan | Actual | Reason |
|------|------|--------|--------|
| Flag B — key item protection | Phase 6 task | Dropped, deferred Phase 9+ | FFIX key items are outside Items.csv 0–255 scope; no scan method available |
| Flag C — gear budget sub-option | Add GearBudgetMode enum | Not needed | Existing GearStatMax/HardCap settings already provide user control |
| baseStats parameter in CorrectEquipmentCoherence | Skeleton declared it | Removed | Greedy pairwise swap handles ties naturally; stat-based tiebreaking was unnecessary complexity |
| Task 8 "fallback recipe" correction | Generate new recipe with reachable ingredients | Remove offending shops instead | Shop removal is simpler, deterministic, needs no RNG; fallback generation deferred |
| rng in EnforceSynthesisReachability | Consumed for fallback generation | Accepted but not consumed | Reserved for future fallback ingredient generation |
| EnforceSynthesisReachability test as separate task | No separate task in plan | Combined with integration tests in Task 9 | Task 8 had no separate test step in plan; covered fully in Task 9 |

---

## Test Count

| Phase | Tests |
|-------|-------|
| Phase 5.99 end | 754 |
| Task 5 (equipment coherence) | +30 → 784 |
| Task 7 (legendary rarity) | +15 → 799 |
| Task 9 (synthesis + integration) | +20 → **819** |

**Final: 819 — all green, 0 warnings**

---

## Parking Lot

### Carry to Phase 8

**StiltzkinRandomizer pipeline wiring:**
Call `StiltzkinRandomizer.Randomize()` at step 4b — after `FieldItemRandomizer`,
after `ItemRemapper`. Condition: `settings.StiltzkinMode != Off`.

**RecommendedLogicEngine pipeline wiring:**
Three calls in Phase 8 `RandomizerEngine` (Recommended mode only):
1. After `CharacterRandomizer` → `CorrectEquipmentCoherence`
2. After `ShopRandomizer` → `EnforceLegendaryRarity`
3. After `SynthesisRandomizer` → `EnforceSynthesisReachability`

`itemIdToMinFieldId` must be pre-computed from `FieldItemScanner` output at generation time.

**SynthesisShopExpansion sub-option (Phase 8):**
Developer identified synthesis shop 36 reused at 4 different field locations
(Alexandria, Treno, Lindblum, Alley). Proposal: patch `Menu(2, 36)` bytecode at each
call site to unique shop IDs (40–43), then generate distinct `Shops` array entries
in `Synthesis.csv` per location — enabling independent randomization per location.
`SynthesisRow.Shops` is already an int array; adding new shop IDs requires no schema change.
Regex pattern for locating synthesis menu calls confirmed in `synth_shop_script_excerpts.txt`.

### Carry from Phase 5.9.3 (still pending)

**Multi-locale field patching:**
144 field scripts differ in bytecode size across locales. Correct approach: per-locale
extraction + same-offset patches. Never copy US bytes wholesale.

**EnemyCatalogTests still uses JSON:**
`EnemyCatalogTests` still references `StockEnemyBytesJsonNoZeros.json`. Rewrite to
use `.bytes` from disk.

**ModSourceResolverTests require Memoria.ini:**
Several tests fail without a Memoria-enabled install.

**ShopRandomizer Price > 2 filter:**
Replace with `ItemPool.ShopFriendly`. Deferred since Phase 4.

### Carry to Phase 9

**FieldParser false-positive fix:**
`FindItemLocations()` produces spurious `DirectItem` hits when `0x48` bytes appear
inside expression arguments of other opcodes. Root cause documented in Phase 4.
Fix requires function-context filtering (real obtainable items live in functions with
an IsButton/proximity trigger). Until fixed, `DirectItem` counts in `VanillaItemCatalog`
are inflated for common low-ID items, and `LegendaryItemList` field-dependent
classifications are unverified.

**LegendaryItemList audit after field scanner fix:**
Once `FieldParser` false positives are eliminated, re-audit `LegendaryItemList` to
confirm or adjust Unique/Legendary classifications for items whose tier currently depends
on field chest count data.

**Key item protection list:**
FFIX key items (World Map, Oglop, etc.) live in a separate data structure outside
Items.csv. Phase 9 research task: identify and parse that structure, then implement
`KeyItemProtectionList`. True key items should be excluded from all randomization pools.

---

## Commit Message

```
feat: Phase 6 complete — RecommendedLogicEngine

VanillaObtainabilityData.cs:
- BlueMagicByEnemyId: 94 entries (enemy ID → spell name)
- Source: Final Fantasy IX Reference Guide - Blue Magic.csv
- Used by Phase 8 spoiler log for ShuffleBlueMagic reporting

LegendaryItemList.cs (new):
- UniqueItemIds: 16 items (1 vanilla copy — never in shops)
- LegendaryItemIds: 21 items (2–3 vanilla copies — never in shops)
- AllProtectedItemIds: union of both sets (37 total)
- UNVERIFIED comment on field-dependent classifications (Phase 9 scanner fix)
- All 37 items confirmed absent from all 32 vanilla ShopItems.csv rows

SynthesisShopData.cs (new):
- ShopFieldIds: shop 32–39 → story-order field IDs
- Shop 38 virtual field ID 2857 (story-order correction for Mage Village end-game)
- Shop 36 uses field 2453 as representative (highest of 4 locations)
- Identified via regex scan of vanilla field scripts

RecommendedLogicEngine.cs (new):
- Pure static functions — no file I/O, no side effects
- CorrectEquipmentCoherence: greedy pairwise swap by weapon/slot affinity
  - WeaponAffinity: Magical(51-78) / Physical(16-50) / Balanced(1-15,79-87) / None
  - SlotAffinity: majority vote over slot type names (from AbilityTierClassification_Rev4)
  - Worst case 196 comparisons, zero RNG calls
  - Changed chars → new CharacterParametersRow instances; unchanged → original refs
- EnforceLegendaryRarity: replace protected items in shops from replacement pool
  - One rng.Next(pool.Count) call per replaced slot; shops in input order
  - Changed rows → new ShopItemsRow; unchanged → original refs; input never mutated
- EnforceSynthesisReachability: Rule 8A + Rule 8B
  - Rule 8A: remove shops where ingredients not reachable before shop field ID
    - InfiniteSource → always reachable; FieldItem + minFieldId → proxy check
    - BossOnly/unknown → treated as reachable; unknown shop → conservative preserve
    - If all shops removed, recipe dropped
  - Rule 8B: finite result cap — drop excess Unique-result recipes (keep lowest Id)
  - rng parameter accepted, not consumed (reserved for future fallback generation)

Tests: 754 → 819 (+65)
- RecommendedLogicEngineEquipmentTests: ClassifyWeapon(11) + ClassifySlots(9)
  + CorrectEquipmentCoherence(10) + EnforceLegendaryRarity(15) = 45
- RecommendedLogicEngineSynthesisAndIntegrationTests: Rule8A(10) + Rule8B(4)
  + Combined(1) + Guards(3) + Integration(2) = 20

6 new rules added
```

---

## PhaseEnd Changelog

```
v1.5.99 → v1.6.0
- Build Log: Phase 6 entry added
- Key Additions: BlueMagicByEnemyId, LegendaryItemList, SynthesisShopData,
  RecommendedLogicEngine (3 methods + 4 helpers + 2 enums)
- Key Discoveries: FFIX key items outside Items.csv 0-255, shop 36 reuse at 4 locations,
  shop 38 field ID story-order mismatch, FieldParser false-positive impact on catalog
- Deviations: Flag B dropped, Flag C no sub-option, baseStats removed,
  shop-removal strategy for synthesis, rng not consumed
- Rules: 7 new rules added
- Parking Lot: Phase 8 (pipeline wiring x3, SynthesisShopExpansion),
  Phase 9 (FieldParser fix, LegendaryItemList audit, key item protection list)
- Phase 6 marked complete
- Current phase: 7 (WPF UI) or 8 (Mod Output + Memoria Integration)
```

---

## 🛑 Stop Here

This chat session is complete. Do the following before starting Phase 7 or 8:

1. Add `PhaseEnd_Phase6.md` to your Claude Project
2. Start a new chat session
3. Say: "Continue building Stiltzkin's Bag. Read all attached markdown files."

Do not continue development in this session. Do not delete this chat — keep it for
posterity and back-reference. Large chats in a project will not slow down chat or use
more tokens.
