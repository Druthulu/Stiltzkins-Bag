# Stiltzkin's Bag — Command Slot & Ability Tier Classification
**LOCKED — Rev 3 — 2026-03-24**
**This document is the implementation contract for Tasks 3 and 4.**

---

## Part 1 — Command Slot Assignment Algorithm

### How CommandSets.csv Works (Option B)

Each of the 8 main character rows (IDs 0–7) in `CommandSets.csv` is **rewritten** during
generation. Row ID always equals character ID — `DefaultCommandSet` values in
`CharacterParameters.csv` are never changed. The engine maps char N → set row N.

Fixed columns that never change:
`Attack=1, Defend=4, Item=14, Change=7`
`Attack(Trance)=1, Defend(Trance)=4, Item(Trance)=14, Change(Trance)=7`

Only `Regular1, Regular2, Trance1, Trance2` are written by the randomizer.

---

### Command Slot Type Registry

| Slot Type | Regular ID | Trance ID | Notes |
|---|---|---|---|
| Steal | 2 | 2 | Always Zidane R1 |
| Skill | 25 | 26 (Dyne) | Always Zidane R2 — see Zidane lock |
| Blk Mag | 22 | 23 (Dbl Blk) | Free pool |
| Focus | 13 | 13 | Free pool — Recommended constraint applies |
| Summon-A | 16 | 18 (Eidolon) | First summoner char (lower ID) |
| Summon-B | 20 | 20 | Second summoner char (higher ID). Functionally same pool as A, stronger multi-hit |
| Wht Mag-A | 17 | 17 | First white mage char (lower ID) |
| Wht Mag-B | 19 | 21 (Dbl Wht) | Second white mage char (higher ID) |
| Swd Art | 30 | 30 | Locked pair with Swd Mag |
| Swd Mag | 31 | 31 | Locked pair with Swd Art |
| Jump | 3 | 12 (Jump 2) | Free pool |
| Dragon | 27 | 27 | Free pool |
| Eat | 8 | 9 (Cook) | Locked pair with Blu Mag |
| Blu Mag | 24 | 24 | Locked pair with Eat |
| Flair | 28 | 29 (Elan) | Free pool |
| Throw | 15 | 15 | Free pool |

---

### Zidane Full Lock

Zidane (char 0) always receives:
- R1 = Steal (2), Trance1 = Steal (2)
- R2 = Skill (25), Trance2 = Dyne (26)

His command slots are **never randomized**. His AA table (AA:101–108) is always his.
His SA pool entries still enter the general shuffle pool (minus T1 pre-seeds).

Reason: several story-mandatory boss fights are Zidane-only. Steal is the only way
to obtain certain items in those fights. Skill carries Flee — always available to the
player as a strategic escape option.

**Recommended mode Dex bias:** After stats shuffle, if Zidane does not have the highest
Dexterity among all 8 main chars, find whoever does and swap Dex values with Zidane.
Zero RNG calls. Ensures Steal success rate is maximized on the locked Steal character.

---

### Class Assignment — Three Paths

| Mode | RandomizeBaseStats | Class Assignment Method |
|---|---|---|
| Recommended | true | **Stat-biased** — score each character per slot type, assign greedily |
| Recommended | false | **Random** — Fisher-Yates, no stat weighting |
| Chaos | either | **Random** — Fisher-Yates, no constraints |

---

### Stat-Bias Score Formula (Recommended + RandomizeBaseStats = true)

| Slot Type | Score Formula |
|---|---|
| Blk Mag | Magic×2 + Will |
| Wht Mag-A / Wht Mag-B | Magic×2 + Will |
| Summon-A / Summon-B | Magic×2 + Will |
| Blu Mag (Eat/Blu Mag pair) | Magic×2 + Will |
| Knight (Swd Art/Swd Mag pair) | Strength×2 + Will |
| Jump | Dexterity + Strength + Will |
| Dragon | Will×2 + Strength |
| Flair | Strength + Will×2 |
| Throw | Dexterity×2 + Strength |
| Focus | Magic×2 + Will |

**Assignment process (stat-biased path):**

1. Exclude Zidane (char 0) — his slots are locked.
2. Assign locked pairs first (Blue Mage, Knight) — score each remaining char for each
   pair's formula, assign the highest scorer. Ties broken by character ID ascending.
3. From remaining chars, assign Summon-A to highest scorer, Summon-B to second highest.
4. From remaining chars, assign Wht Mag-A to highest scorer, Wht Mag-B to second highest.
5. One char remains unassigned — their two slots come entirely from the free pool.
6. Free pool (Blk Mag, Focus, Jump, Dragon, Flair, Throw) assigned by score to remaining
   open R2 slots (both summoners, both white mages, Zidane R2 excluded since locked,
   unassigned char R1+R2) — best scorer per slot type fills that slot.

**RNG consumption (stat-biased path):** 0 calls for class assignment.
Tie-breaking by char ID is deterministic. All randomness came from the stats shuffle.

**RNG consumption (random path):**
- Fisher-Yates shuffle of chars {1–7}: 6 calls
- Assignment is positional from the shuffled order

---

### Slot Assignment Steps — Random Path

1. Fisher-Yates shuffle chars {1,2,3,4,5,6,7} — 6 RNG calls
2. Slot 0 (shuffled char) → Blue Mage (R1=Eat, R2=Blu Mag)
3. Slot 1 → Knight (R1=Swd Art, R2=Swd Mag)
4. Slot 2 → Summon-A, Slot 3 → Summon-B
5. Slot 4 → Wht Mag-A, Slot 5 → Wht Mag-B
6. Slot 6 → Unassigned (both slots from free pool)
7. Fisher-Yates shuffle free pool [Blk Mag, Focus, Jump, Dragon, Flair, Throw] — 5 RNG calls
8. Assign shuffled free pool items to 6 open R2/secondary slots in order:
   Summon-A R2, Summon-B R2, Wht Mag-A R2, Wht Mag-B R2, Unassigned R1, Unassigned R2

**Total RNG (random path): 11 calls.**

---

### Focus Constraint (Recommended mode only)

After assignment, verify the character who received Focus has at least one magic-type
command (Blk Mag, Wht Mag-A, Wht Mag-B, Summon-A, Summon-B, Blu Mag).

If not:
- Find all characters with a magic-type command slot.
- Randomly select one qualifying character — 1 RNG call.
- Swap their non-Focus, non-magic R2 slot with Focus.
- Fallback (no valid swap): force Focus onto the magic character with lowest AP pre-assigned.
  Zero RNG.

In Chaos mode: no constraint. Focus can land anywhere.

---

### AbilityFeatures.txt Patch

After slot assignment, find which character received the Blk Mag slot.
Rewrite `>CMD 31 Magic Sword` HardDisable condition to reference that character's
`CharacterId_*` name.

If no character received Blk Mag (impossible under this algorithm — it is always in the
free pool and always gets assigned): leave the file unchanged and log a warning.

---

### CharacterRandomizerResult additions required

- `CommandSetRows` — `IReadOnlyList<CommandSetsRow>` — 8 rewritten rows (IDs 0–7)
- `SlotAssignment` — `IReadOnlyDictionary<int, List<string>>` — maps char ID → list of
  assigned slot type names (e.g. `{0: ["Steal","Skill"], 3: ["Summon-A","Dragon"]}`)
  Required by Task 4 (AA assignment) and SA pre-seeding to know which T1/T2 SAs to assign.

---

## Part 2 — Active Ability Assignment

### Principle

Each command slot type delivers a **complete, atomic AA package** to its holder.
No individual AA randomization within a class.

### AA Package Per Slot Type

| Slot Type | AAs |
|---|---|
| Steal + Skill (Zidane only) | AA:101–108 (Flee, Detect, What's That!?, Soul Blade, Annoy, Sacrifice, Lucky Seven, Thievery). Always Zidane's. Never pooled. |
| Blk Mag | AA:25–48 (24 black magic spells) |
| Swd Art + Swd Mag (Knight) | AA:141–152 (12 Sword Arts) |
| Jump | No AAs — Jump is a pure command |
| Dragon | AA:117–124 (Lancer, Reis's Wind, Dragon Breath, White Draw, Luna, Six Dragons, Cherry Blossom, Dragon's Crest) |
| Eat + Blu Mag (Blue Mage) | AA:77–100 (24 blue magic spells, learned by eating enemies) |
| Flair | AA:125–132 (Chakra, Spare Change, No Mercy, Aura, Curse, Revive, Demi Shock, Countdown) |
| Throw | No AAs — uses inventory items |
| Focus | No AAs — buff command only |
| Summon-A / Summon-B | From summon pool split (see below) |
| Wht Mag-A / Wht Mag-B | From white magic pool split (see below) |

---

### Summon Pool Split

**Merged pool (12 summons, no overlap):**

Garnet's 8: AA:49 Shiva, AA:51 Ifrit, AA:53 Ramuh, AA:55 Atomos,
            AA:58 Odin, AA:60 Leviathan, AA:62 Bahamut, AA:64 Ark

Eiko's 4:   AA:66 Fenrir, AA:68 Carbuncle, AA:72 Phoenix, AA:74 Madeen

**Split:** Fisher-Yakes shuffle all 12 — 11 RNG calls.
Summon-A holder gets indices 0–5 (6 summons).
Summon-B holder gets indices 6–11 (6 summons).

**Odin tracking:** After split, record which character (Summon-A or Summon-B holder's
char ID) received AA:58 Odin. This char ID is passed to the SA pre-seeding step to
force SA:60 Odin's Sword onto them.

---

### White Magic Pool Split

**Merged pool (24 spells — Garnet's 18 unique + Eiko's 6 unique, no overlap):**

AA:1 Cure, AA:2 Cura, AA:3 Curaga, AA:4 Regen, AA:5 Life, AA:6 Full-Life,
AA:7 Scan, AA:8 Panacea, AA:9 Stona, AA:10 Esuna, AA:11 Shell, AA:12 Protect,
AA:13 Haste, AA:14 Silence, AA:15 Mini, AA:16 Reflect, AA:17 Confuse, AA:18 Berserk,
AA:19 Blind, AA:20 Float, AA:21 Dispel, AA:22 Might, AA:23 Jewel, AA:24 Holy

Note: AA:23 Jewel and AA:24 Holy were Eiko-exclusive in vanilla but are white magic
spells — they belong in this pool and may land on either white mage.

**Split:** Fisher-Yates shuffle all 24 — 23 RNG calls.
Wht Mag-A holder gets indices 0–11 (12 spells).
Wht Mag-B holder gets indices 12–23 (12 spells).

Note: Garnet and Eiko each had duplicate spells (Cure, Cura, etc.) in vanilla. The merged
pool has exactly one copy of each spell. Both white mages get 12 spells but they will
never share a spell. One white mage might get all the healing, another all the buffs —
this is intentional and desirable randomization.

---

## Part 3 — Supporting Ability Pre-Seeding and Pool Assignment

### Pre-Seeding Process

1. Resolve slot assignment (Part 1 result) and Odin tracking (Part 2 result).
2. Apply all T1 pre-seeds — assign directly, mark as consumed, remove from pool.
3. In Recommended mode only: apply T2 pre-seeds — assign directly, mark as consumed,
   remove from pool.
4. Collect all SA entries from all 8 character files into pool (preserving duplicates).
   Remove all consumed entries (matched by AbilityRef string).
5. Fisher-Yates shuffle the pool.
6. Round-robin assignment with duplicate-skip.

### Pool Assignment Rules

- Pool preserves all duplicates from vanilla character files.
  Example: SA:49 Antibody appears 8 times — every character will eventually receive it,
  just in a randomly ordered slot position.
- Per-character `HashSet<string>` tracks already-assigned AbilityRefs.
- When a pool entry is drawn for a character: if already in their set, skip and try next.
- No character can receive the same SA twice.

### Gender Filter — SA:38 Protect Girls

SA:38 checks `CharacterCategory_Female` at runtime. Assigning it to a female character
produces a broken/useless effect (protecting themselves).

Assignment rule — all modes: if SA:38 is drawn for a character whose `DefaultCategory == 6`
(female), skip and try next pool entry. This is a runtime-correctness filter, not a
Recommended-mode-only constraint.

---

### T1 — Guaranteed (all modes, pre-seeded before pool shuffle)

| SA | Name | Assigned to | Reason |
|---|---|---|---|
| SA:62 | Bandit | Steal holder (Zidane) | Core Steal success rate. Unique to Thief |
| SA:61 | Mug | Steal holder (Zidane) | Adds physical damage to successful steal |
| SA:22 | Master Thief | Steal holder (Zidane) | Guarantees rare steal over common |
| SA:32 | Mag Elem Null | Blk Mag holder | Negates elemental weaknesses — Black Mage identity |
| SA:31 | Reflectx2 | Blk Mag holder | Reflected spells deal double damage |
| SA:37 | Cover | Knight holder | Iconic Knight interception ability |
| SA:21 | High Jump | Jump holder | Directly extends Jump command |
| SA:42 | Initiative | Jump holder | Preemptive strikes — Dragoon identity |
| SA:28 | Power Throw | Throw holder | Directly extends Throw command |
| SA:29 | Power Up | Flair holder | Core Monk berserker identity |
| SA:59 | Boost | Summon-A holder | Core summoner identity |
| SA:59 | Boost | Summon-B holder | Core summoner identity (2nd copy consumed) |
| SA:47 | Guardian Mog | Either summoner (random pick — 1 RNG call) | Post-battle status clear via Mog |
| SA:60 | Odin's Sword | Summoner who received AA:58 Odin | Only functional with Odin in pool |

---

### T2 — Recommended Mode Only (pre-seeded, removed from pool in Recommended)

#### All magic slot holders (Blk Mag, Wht Mag-A, Wht Mag-B, Summon-A, Summon-B, Blu Mag)

Distributed first-come-first-served across magic chars in character ID ascending order
until pool copies are exhausted.

| SA | Name | Copies in pool | Notes |
|---|---|---|---|
| SA:8 | MP+20% | 3 (Vivi, Garnet, Eiko files) | More MP for casting |
| SA:34 | Half MP | 4 (Vivi, Garnet, Quina, Eiko) | Halves casting costs |
| SA:24 | Healer | 5 (Vivi, Garnet, Quina, Amarant, Eiko) | Healing potency |
| SA:25 | Add Status | 6 (Zidane, Vivi, Steiner, Freya, Quina, Amarant) | Spells inflict statuses |
| SA:54 | Return Magic | 2 (Vivi, Amarant) | Returns offensive magic at caster |
| SA:55 | Absorb MP | 1 (Quina only) | Drains MP from physical hits — funds casting |
| SA:30 | Reflect-Null | 2 (Vivi, Eiko) | White mage priority — heals through Reflect |
| SA:33 | Concentrate | 2 (Garnet, Eiko) | Magic accuracy — summoner priority, but any magic char qualifies |

#### Summoner slot holders (Summon-A, Summon-B) — additional T2

| SA | Name | Notes |
|---|---|---|
| SA:33 | Concentrate | Summoner priority within the magic T2 distribution above |

#### Physical slot holders

| SA | Name | Assigned to | Notes |
|---|---|---|---|
| SA:6 | HP+20% | Knight, Flair | Tanks and brawlers need HP |
| SA:9 | Accuracy+ | Knight, Throw | Physical attacks need accuracy |
| SA:12 | MP Attack | Knight | Bonus physical damage via MP — synergizes with Sword Arts |
| SA:36 | Counter | Knight, Flair | Retaliates against physical attacks |
| SA:17 | Dragon Killer | Dragon, Jump | Bonus damage to dragons — thematic for dragoon archetypes |
| SA:35 | High Tide | Jump | Fills Trance faster for Jump 2 |
| SA:52 | Restore HP | Flair | Synergizes with Power Up risk/reward |
| SA:23 | Steal Gil | Steal (Zidane) | Auto-steals gil — thematic for Thief |
| SA:46 | Flee-Gil | Steal (Zidane) | Gil on flee — thematic for Thief |
| SA:38 | Protect Girls | Steal (Zidane) | Male chars only (gender filter) |

---

### T3 — Free Pool (all modes)

All SA entries from all 8 character files not consumed by T1 or T2 pre-seeding.
This includes all universals (listed below), all killer SAs, and all remaining utilities.

### Universal SAs (8 copies — all characters eventually receive one)

SA:0 Auto-Reflect, SA:1 Auto-Float, SA:2 Auto-Haste, SA:3 Auto-Regen, SA:4 Auto-Life,
SA:35 High Tide, SA:40 Body Temp, SA:43 Level Up, SA:44 Ability Up, SA:48 Insomniac,
SA:49 Antibody, SA:53 Jelly, SA:56 Auto-Potion, SA:57 Locomotion, SA:58 Clear Headed

Note: SA:35 High Tide appears as T2 for Jump holder. One of the 8 copies is consumed
by that pre-seed. Remaining 7 copies still cover all other characters via pool.

---

## Part 4 — Parking Lot

| Topic | Notes |
|---|---|
| Garnet story-restricted summons | A non-Garnet character who receives Summon has no story restriction. Garnet's restriction follows her character ID, not the command slot. Flag for Phase 6. |
| CommandSets.csv guest rows 8–19 | Never rewritten. Stage sets and guest-join sets are always preserved. |
| Zidane Dex bias | Post-stats-shuffle swap in Recommended mode. Zero RNG. |
| SA:60 + Odin pairing | Tracked via summon pool split result. Odin's char ID passed explicitly to SA pre-seeding. |
| Focus retry RNG | At most 1 extra RNG call in Recommended mode if Focus needs swapping. |
| SA:47 Guardian Mog summoner pick | 1 RNG call to decide which summoner gets it (random between the two). |
| White magic pool has no duplicates | Garnet and Eiko shared spells (Cure, Cura, etc.) appear exactly once in the merged pool. One white mage may be stronger at healing than the other — intentional. |
