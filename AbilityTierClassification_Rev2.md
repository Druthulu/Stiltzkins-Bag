# Stiltzkin's Bag — Command Slot & Ability Tier Classification
**For Developer Review — Edit Before Implementation**
**Rev 2 — reflects full conversation through 2026-03-24**

---

## Part 1 — Command Slot Assignment Algorithm

### How CommandSets.csv Works in Option B

Each of the 8 main character rows (IDs 0–7) in `CommandSets.csv` is **rewritten** during
generation. The row ID stays equal to the character ID — `DefaultCommandSet` values in
`CharacterParameters.csv` are never changed. The engine always maps char N → set row N.

Fixed columns that never change per row:
`Attack=1, Defend=4, Item=14, Change=7`
`Attack(Trance)=1, Defend(Trance)=4, Item(Trance)=14, Change(Trance)=7`

Only `Regular1, Regular2, Trance1, Trance2` are written by the randomizer.

---

### Command Slot Type Registry

| Slot Type | Regular ID | Trance ID | Notes |
|---|---|---|---|
| Steal | 2 | 2 | Always Zidane R1 |
| Skill | 25 | 26 (Dyne) | Free pool |
| Blk Mag | 22 | 23 (Dbl Blk) | Free pool |
| Focus | 13 | 13 | Free pool — Recommended constraint applies |
| Summon-A | 16 | 18 (Eidolon) | Garnet-style summon — first summoner char |
| Summon-B | 20 | 20 | Eiko-style summon — second summoner char |
| Wht Mag-A | 17 | 17 | Garnet-style white magic — first white mage char |
| Wht Mag-B | 19 | 21 (Dbl Wht) | Eiko-style white magic — second white mage char |
| Swd Art | 30 | 30 | Locked pair with Swd Mag |
| Swd Mag | 31 | 31 | Locked pair with Swd Art |
| Jump | 3 | 12 (Jump 2) | Free pool |
| Dragon | 27 | 27 | Free pool |
| Eat | 8 | 9 (Cook) | Locked pair with Blu Mag |
| Blu Mag | 24 | 24 | Locked pair with Eat |
| Flair | 28 | 29 (Elan) | Free pool |
| Throw | 15 | 15 | Free pool |

> **DEV NOTE:** Summon-A vs Summon-B and Wht Mag-A vs Wht Mag-B may or may not differ
> functionally beyond the trance variant. Verify whether command IDs 16 vs 20 unlock
> different eidolons or whether they share the same underlying spell list. If they're
> functionally identical, we can use A for the first character sorted by ID and B for
> the second, which is deterministic without needing a separate RNG call.

---

### Assignment Steps (in order, RNG calls documented)

**Step 1 — Locked pairs (1 RNG call each)**

From chars {1,2,3,4,5,6,7}:
- Draw one char → **Blue Mage** (R1=Eat, R2=Blu Mag, Trance1=Cook, Trance2=Blu Mag)
- From remaining → draw one char → **Knight** (R1=Swd Art, R2=Swd Mag, Trance1=Swd Art, Trance2=Swd Mag)

RNG: 2 calls total (one per locked pair pick).

**Step 2 — Zidane R1 (0 RNG calls)**

Zidane (char 0) always gets R1=Steal, Trance1=Steal. No RNG needed.

**Step 3 — Summoner and White Mage assignment (2 RNG calls)**

Remaining chars with both slots open: 5 chars (chars 1–7 minus Blue Mage minus Knight).

From these 5:
- Draw 2 → **Summoners**. First drawn (by char ID sort if tied) gets Summon-A; second gets Summon-B.
- Remaining 3 → draw 2 → **White Mages**. First gets Wht Mag-A; second gets Wht Mag-B.
- Remaining 1 → **Unassigned primary** (both slots come from free pool).

RNG: 2 calls (one pick of 2 from 5 for Summoners, one pick of 2 from 3 for White Mages).

Actually implementation note: cleanest is Fisher-Yates shuffle of the 5 remaining chars,
take indices 0–1 as Summoners, indices 2–3 as White Mages, index 4 as Unassigned.
That costs 4 RNG calls (Fisher-Yates on 5 elements: i from 4 down to 1).

**Step 4 — Free pool shuffle and assignment (7 RNG calls + possible retry)**

Free pool (always exactly 7 items):
`Skill, Jump, Dragon, Flair, Throw, Focus, Blk Mag`

Fisher-Yates shuffle of 7 items = 6 RNG calls.

Assign shuffled items to the 7 open positions in this order:
1. Summoner-A R2 / Trance2
2. Summoner-B R2 / Trance2
3. Wht Mag-A R2 / Trance2
4. Wht Mag-B R2 / Trance2
5. Unassigned char R1 / Trance1
6. Unassigned char R2 / Trance2
7. Zidane R2 / Trance2

**Step 5 — Focus constraint (Recommended mode only)**

After assignment, check if the character who received Focus has at least one magic-type
command slot (Blk Mag, Wht Mag-A, Wht Mag-B, Summon-A, Summon-B, Blu Mag).

If Focus is on a non-magic character:
- Scan all other characters' slots for a magic-type command.
- Swap Focus with a randomly selected non-Focus, non-magic slot on a magic character.
  RNG: 1 call to pick which qualifying swap target.
- If no valid swap exists (all magic chars already have Focus, which is impossible since
  only 1 Focus exists): force Focus onto the magic character with the lowest total AP
  currently pre-assigned. Zero RNG.

**Step 6 — AbilityFeatures.txt patch**

Find which character received Blk Mag (always exactly one).
Rewrite `>CMD 31 Magic Sword` HardDisable to reference that character's `CharacterId_*`.
(Same logic as current Task 3 implementation — just changed from "Black Mage command set"
to "Blk Mag slot holder".)

**Step 7 — Zidane Dex bias (Recommended mode only, 0 RNG calls)**

After stats shuffle: if Zidane's Dexterity is not the maximum value across all 8 main chars,
find whoever has the highest Dex and swap values with Zidane. Deterministic correction.

---

### Result: New field needed in CharacterRandomizerResult

`CommandSetRows` — `IReadOnlyList<CommandSetsRow>` — the 8 rewritten rows (IDs 0–7).
`CharacterParameters.DefaultCommandSet` values do not change.

---

## Part 2 — Active Ability Assignment

### Principle

Whoever receives a command slot gets the **complete** AA pool for that slot type.
AAs arrive as an atomic package — no individual AA randomization within a class.

---

### AA Pool Per Slot Type

| Slot Type | AAs | Notes |
|---|---|---|
| Steal | AA:101–108 | Flee, Detect, What's That!?, Soul Blade, Annoy, Sacrifice, Lucky Seven, Thievery |
| Blk Mag | AA:25–48 | Full 24-spell black magic list |
| Swd Art / Swd Mag | AA:141–152 | Full 12 Sword Arts |
| Jump / Dragon | See note | Jump and Dragon are split — see below |
| Eat / Blu Mag | AA:77–100 | Full 24 blue magic spells. Learned by eating enemies |
| Flair | AA:125–132 | Chakra, Spare Change, No Mercy, Aura, Curse, Revive, Demi Shock, Countdown |
| Throw | None | Throw has no character-specific AAs — it uses items from inventory |
| Skill | AA:105–108 | Annoy, Sacrifice, Lucky Seven, Thievery (Skill sub-abilities) |
| Summon-A | From summon pool split | See summon pool section |
| Summon-B | From summon pool split | See summon pool section |
| Wht Mag-A | From white magic pool split | See white magic pool section |
| Wht Mag-B | From white magic pool split | See white magic pool section |
| Focus | None | Focus is a buff command, no AAs |

> **DEV NOTE — Jump vs Dragon:** You said Jump and Dragon can be split. But in vanilla,
> Freya's Dragon pool (AA:117–124) includes both jumping abilities (Lancer, Dragon Breath)
> and dragon-themed spells (Dragon's Crest, Reis's Wind). Suggest: Jump slot holder
> gets no AAs (Jump is a pure command), Dragon slot holder gets AA:117–124.
> Alternatively: Dragon holder gets AA:119 Dragon Breath, AA:120 White Draw, AA:122 Six Dragons,
> AA:124 Dragon's Crest only (the non-jump abilities), and Jump holder gets AA:117 Lancer,
> AA:118 Reis's Wind, AA:121 Luna, AA:123 Cherry Blossom (jump-support abilities).
> **Needs your decision before implementation.**

> **DEV NOTE — Skill:** Skill in Zidane's command set accesses AA:105–108 as sub-abilities.
> But Steal is locked to Zidane. If someone else gets Skill, do they also get those AAs?
> Or does Skill have no AAs for other characters and it's just a command?
> **Needs your decision.**

> **DEV NOTE — Throw:** Throw has no learnable AAs in vanilla. SA:28 Power Throw enhances it.
> Confirm Throw holders get zero AAs.

---

### Summon Pool Split

**Merged pool (12 summons, no overlap):**

Garnet's 8: AA:49 Shiva, AA:51 Ifrit, AA:53 Ramuh, AA:55 Atomos,
            AA:58 Odin, AA:60 Leviathan, AA:62 Bahamut, AA:64 Ark

Eiko's 4:   AA:66 Fenrir, AA:68 Carbuncle, AA:72 Phoenix, AA:74 Madeen

**Split method:** Fisher-Yates shuffle all 12. Summoner-A gets first 6. Summoner-B gets last 6.
Each summon appears on exactly one summoner per run.

> **CRITICAL — Odin + SA:60 pairing:** SA:60 Odin's Sword is only functional if the
> character who holds it also received AA:58 Odin in the split. During SA assignment,
> look up which summoner character received Odin and force SA:60 onto them (T1 pre-seed).

---

### White Magic Pool Split

**Merged pool (24 spells, no overlap):**
AA:1 Cure, AA:2 Cura, AA:3 Curaga, AA:4 Regen, AA:5 Life, AA:6 Full-Life,
AA:7 Scan, AA:8 Panacea, AA:9 Stona, AA:10 Esuna, AA:11 Shell, AA:12 Protect,
AA:13 Haste, AA:14 Silence, AA:15 Mini, AA:16 Reflect, AA:17 Confuse, AA:18 Berserk,
AA:19 Blind, AA:20 Float, AA:21 Dispel, AA:22 Might, AA:23 Jewel, AA:24 Holy

**Split method:** Fisher-Yates shuffle all 24. Wht Mag-A gets first 12. Wht Mag-B gets last 12.
Each white magic spell appears on exactly one white mage per run.

> **DEV NOTE — Eiko's unique spells:** In vanilla, AA:68 Carbuncle, AA:66 Fenrir,
> AA:72 Phoenix, AA:74 Madeen are Eiko's summons, not white magic. They go into the
> summon pool. AA:23 Jewel and AA:24 Holy are Eiko-exclusive white magic and are
> included in the white magic pool split above.

---

## Part 3 — Supporting Ability Tier Classification

### Tier Key

| Tier | Behavior |
|---|---|
| **T1 — Guaranteed** | Pre-assigned before pool shuffle in ALL modes. Removed from pool. Non-negotiable. |
| **T2 — Recommended** | Pre-assigned before pool shuffle in Recommended mode only. Removed from pool in Recommended, stays in pool for Chaos. |
| **T3 — Free** | Always enters the shuffle pool. Assigned randomly in all modes. |

### Pre-Assignment Process

1. Identify which character holds each slot type (result of Part 1)
2. For each T1 entry: assign directly to the mapped character; mark as consumed
3. In Recommended mode: for each T2 entry: assign directly to the mapped character; mark as consumed
4. Build pool from all SA entries across all 8 character files; remove all consumed entries
5. Fisher-Yates shuffle the remaining pool
6. Round-robin assignment with duplicate-skip (see pool assignment rules)

### Pool Assignment Rules

- Pool contains duplicates (e.g. 8 copies of SA:49 Antibody — one per vanilla character file)
- Maintain a `HashSet<string>` of AbilityRefs already assigned per character
- When assigning from pool: if character already has this AbilityRef, skip and try next
- Since universals appear 8 times, every character will eventually receive each universal

### Gender Filter

SA:38 Protect Girls checks `CharacterCategory_Female` at runtime. It is cosmetically
awkward on a female character (protecting themselves). Only assign SA:38 to characters
whose `DefaultCategory != 6` (i.e., male characters only).

If SA:38 is drawn for a female character during pool assignment: skip and try next entry.
If all male characters already have SA:38 (impossible since only 1 copy exists): assign anyway.

---

### T1 — Guaranteed Per Slot Type

| SA | Name | Assigned to | Reason |
|---|---|---|---|
| SA:62 | Bandit | Steal holder (always Zidane) | Core Steal mechanic. Dramatically improves steal success rate |
| SA:61 | Mug | Steal holder (always Zidane) | Adds physical damage to successful steal. Core Thief combat identity |
| SA:22 | Master Thief | Steal holder (always Zidane) | Guarantees rare steal over common. Unique to Thief class |
| SA:32 | Mag Elem Null | Blk Mag holder | Negates elemental weaknesses — exclusive, changes how magic works |
| SA:31 | Reflectx2 | Blk Mag holder | Reflected spells deal double damage — only on Vivi in vanilla |
| SA:37 | Cover | Knight holder | Iconic Knight ability — intercepts attacks on critical-HP allies |
| SA:21 | High Jump | Jump holder | Extends Jump air time and damage — directly extends the Jump command |
| SA:42 | Initiative | Jump holder | Guarantees preemptive strikes — exclusive to Dragoon in vanilla |
| SA:28 | Power Throw | Throw holder | Doubles Throw power based on item count — directly extends Throw |
| SA:29 | Power Up | Flair holder | Doubles physical damage at critical HP — core Monk berserker identity |
| SA:59 | Boost | Summon-A holder | Enhances summon power — core summoner identity |
| SA:59 | Boost | Summon-B holder | Same — both summoner characters always get Boost |
| SA:47 | Guardian Mog | Summon-B holder | Post-battle status clear via Mog — historically Eiko's ability |
| SA:60 | Odin's Sword | Summoner who received AA:58 Odin | Only functional if Odin is in this character's summon pool |

> **DEV NOTE — SA:59 Boost appears twice:** Both summoner characters get it. Since there
> are 2 copies of SA:59 in the combined pool (Garnet + Eiko each have it), pre-seeding
> both removes both copies from the pool, preventing any third character from getting Boost.
> Confirm this is intended — Boost only makes sense on a summoner.

> **DEV NOTE — SA:47 Guardian Mog on Summon-B specifically:** This is assigned to
> Summon-B because it was Eiko's ability in vanilla. If you want it on whichever summoner
> received the most summons, or simply on any random summoner, adjust here.

---

### T2 — Recommended Per Slot Type

#### Magic slot types (Blk Mag, Wht Mag-A, Wht Mag-B, Summon-A, Summon-B, Blu Mag)

| SA | Name | Notes |
|---|---|---|
| SA:8 | MP+20% | More MP = more spells. Essential for all magic classes |
| SA:34 | Half MP | Halves all MP costs. Critical for high-frequency casting |
| SA:24 | Healer | Increases healing magic potency. Valuable on any mage |
| SA:25 | Add Status | Spells inflict status effects. Powerful for offensive and support magic |
| SA:54 | Return Magic | Returns offensive magic at caster — valid on any magic type |
| SA:55 | Absorb MP | Drains MP from physical hits — funds magic casting |
| SA:30 | Reflect-Null | Cast through Reflect — primarily useful for healing white magic through Reflect |

**T2 for Summon-A and Summon-B specifically:**

| SA | Name | Notes |
|---|---|---|
| SA:33 | Concentrate | Increases magic accuracy. Prevents misses on expensive summons |

> **DEV NOTE — SA:30 Reflect-Null vs Reflectx2:** Reflect-Null is T2 for White Mage because
> it enables healing through Reflect barriers. The Reflectx2 + Reflect-Null combo was Vivi's
> vanilla strategy, but since they're now on separate characters by design, a Black Mage
> doesn't need Reflect-Null for Reflectx2 to function — they just need something to apply
> Reflect to the target first. Confirm you're happy with Reflect-Null being White Mage T2
> rather than also Black Mage T2.

#### Physical slot types (Swd Art, Jump, Dragon, Flair, Throw, Skill, Steal)

| SA | Name | Assigned to | Notes |
|---|---|---|---|
| SA:6 | HP+20% | Knight, Flair | Tanks and brawlers need maximum HP |
| SA:9 | Accuracy+ | Steal, Knight, Throw | Physical attacks and steals both need accuracy |
| SA:12 | MP Attack | Knight | Consumes MP for bonus physical damage. Synergizes with Sword Arts |
| SA:36 | Counter | Knight, Flair | Retaliates against physical attacks. Core martial identity |
| SA:17 | Dragon Killer | Dragon, Jump | Thematic for dragon-hunters. Deals bonus damage to dragons |
| SA:35 | High Tide | Jump | Fills Trance faster. Unlocks Jump 2 (trance) sooner |
| SA:52 | Restore HP | Flair | Auto-restores HP from critical — synergizes with Power Up risk/reward |
| SA:23 | Steal Gil | Steal | Auto-steals gil on each attack. Thematic for Thief class |
| SA:46 | Flee-Gil | Steal | Gets gil when fleeing. Pairs with Flee command thematically |
| SA:38 | Protect Girls | Steal | Male characters only (see gender filter) |

---

### T3 — Free Pool (all modes)

All SA entries not consumed by T1 or T2 pre-seeding enter the shuffle pool.
This includes all universal abilities (see below), all remaining killers,
and all other utilities.

---

### Universal SAs (8 copies in pool — every character eventually receives one)

These appear on all 8 vanilla main character files. Every run: every character gets each
of these, just in a randomly ordered slot position.

| SA | Name |
|---|---|
| SA:0 | Auto-Reflect |
| SA:1 | Auto-Float |
| SA:2 | Auto-Haste |
| SA:3 | Auto-Regen |
| SA:4 | Auto-Life |
| SA:35 | High Tide |
| SA:40 | Body Temp |
| SA:43 | Level Up |
| SA:44 | Ability Up |
| SA:48 | Insomniac |
| SA:49 | Antibody |
| SA:53 | Jelly |
| SA:56 | Auto-Potion |
| SA:57 | Locomotion |
| SA:58 | Clear Headed |

> **DEV NOTE — SA:35 High Tide appears as both T2 for Jump (Dragoon) and Universal (T3).**
> If Jump holder gets High Tide via T2 pre-seeding, one of the 8 universal copies is
> consumed. The remaining 7 copies still make it into the pool. Every other character
> still receives it eventually. No conflict — the math works.

---

## Part 4 — Open Questions Requiring Decision Before Implementation

| # | Question | Default if not answered |
|---|---|---|
| 1 | **Jump AA split:** Does Jump holder get all of AA:117–124, or only jump-support abilities (117,118,121,123)? Does Dragon holder get the rest (119,120,122,124)? Or does Jump holder get no AAs and Dragon holder gets all 8? | Dragon holder gets all AA:117–124. Jump holder gets no AAs. |
| 2 | **Skill AAs:** Does whoever receives the Skill command slot also receive AA:105–108? Or does Skill have no AAs outside of Zidane? | Skill has no AAs for non-Zidane characters. The AAs are locked to Steal via the Thief class package. |
| 3 | **Throw AAs:** Confirm Throw holders get zero learnable AAs. | Yes — zero. |
| 4 | **Summon-A vs Summon-B command IDs:** Are command IDs 16 and 20 functionally identical or do they unlock different eidolons? If identical, does assignment order matter? | Assign Summon-A (16/18) to lower char ID, Summon-B (20/20) to higher char ID. |
| 5 | **SA:33 Concentrate on both summoner sets:** Confirm T2 for both. The first markdown had it T1 on Summon-A only, but you said "possible on both summoner sets." T2 on both means both summoners are recommended to have it. | T2 on both Summon-A and Summon-B holders. |
| 6 | **T2 magic SAs assigned to which specific characters:** The T2 list says "all magic classes." With 6 magic-type slot holders (Blk Mag, Wht Mag-A, Wht Mag-B, Summon-A, Summon-B, Blu Mag) and only a few copies of each SA in the pool, not all magic chars can receive all T2 magic SAs. Should T2 magic SAs be distributed round-robin across magic chars, or first-come-first-served from the shuffle? | First-come-first-served: T2 SAs pre-seeded in character ID order across magic-type chars until copies are exhausted. |
| 7 | **SA:38 Protect Girls in Chaos mode:** In Chaos mode, SA:38 enters the free pool. Does the gender filter still apply (skip female characters), or does Chaos mode ignore all filters? | Gender filter still applies in all modes — it prevents a broken runtime effect. |

---

## Parking Lot

| Topic | Notes |
|---|---|
| Garnet story-restricted summons | Characters who receive Summon may have story-gated eidolons during early game if they are Garnet. This follows the *character* not the class. Non-Garnet summoners have no such restriction. Discuss Phase 6 / Gen2. |
| CommandSets.csv guest rows (8–19) | Stage sets and guest-join sets are never rewritten. Only rows 0–7 are modified. |
| Option B phasing | This document describes the full Option B design. All of it is targeted at Task 3 implementation. Nothing is deferred to Phase 6 except constraint validation in Recommended Logic Engine. |
| Zidane speed bias | Post-shuffle swap in Recommended mode: if Zidane doesn't have max Dex, swap with whoever does. Zero RNG. Ensures steal success rate is highest on the locked Steal character. |
