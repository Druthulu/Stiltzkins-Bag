# Stiltzkin's Bag — FFIX Binary Codec & Randomization Cookbook

> **Evolvable reference (docs/ layer). Created 2026-07-06.** The compounding knowledge base for this project's recurring craft — the distilled techniques, idioms, and gotchas of the work you do over and over. Separate from the constitution (permanent) and the phase logs (granular history). **Consult this at session start before recurring work, and grow it after** (registry X4 — the flywheel). Append an entry every time you find a reusable nuance; the whole point is that each unit of work makes the next one cheaper and safer.

## Pinned context (the invariants every entry assumes)

- **Stack:** C# / .NET 8 — `net8.0` (Core, Tests), `net8.0-windows` (App/WPF). xUnit 2.5.3, CsvHelper 33.1.0, CommunityToolkit.Mvvm, MaterialDesignInXaml. Windows-only.
- **Domain:** a deterministic seed-based randomizer for **Final Fantasy IX (PC/Steam)** that outputs a **Memoria Engine mod-folder overlay** (modified CSVs + individually patched raw `.bytes` binaries).
- **Determinism is the master contract:** one `Random` seeded once (`SeedEngine.Resolve`, platform-independent hash), threaded through the pipeline in a fixed draw order → same seed + settings = **byte-identical** output. Every entry assumes RNG call order/count is sacred (§E G1–G3).
- **Data sources & oracles:** Memoria CSVs under the user's `StreamingAssets/Data/`; `p0data2.bin` (enemy/battle) and `p0data7.bin` (field/world scripts) Unity archives; Hades Workshop C++ as the parser/codec reference (§E G11). Game files are source of truth; guide CSVs are validation-only (§E G8).
- **Binary conventions:** output raw `.bytes` (no Unity repacking); patch **all 7 language locale variants** (`es/fr/gr/it/jp/uk/us`) — their bytecode is byte-identical, only AT_TEXT string IDs differ (a separate asset). Item IDs occupy 0–255; **item ID 0 is a null sentinel** (filter it); prices/gil (>255) disambiguate from item IDs by range.
- **Test discipline:** `dotnet test` is the gate; **0-warnings** target; parity tests assert byte-identity where reconstructable, else functional properties (patch-fragile game-derived fixtures use property assertions, not byte-equality).

## How to use this

1. **Before** a unit of recurring work, consult the entries below and shape the work toward the known idioms/recipes *first* — it saves rounds.
2. When you discover a new correspondence, technique, or "what makes X happen" trick, **add it here** — and, when applicable, also feed it back into the tooling (a flag, a script, a config entry) so the next similar case is even cheaper (X4). Don't over-encode genuine one-offs; the target is the *average* case trending toward one-shot plus a shrinking hard tail.
3. When a residual is a hard tail that no technique here reaches, note it, route it to the right escalation (permuter/tool-source research/human), and move on — don't grind (registry M8: exhaust the lever ladder, but don't force it past the machine-checkable gate).
4. Write entries **during the session that produced the insight** (X5) — a summary-only future session loses the detail that makes the entry correct.

*Entry shape: a short title, the trigger, the technique, a minimal example. Number sections `§1, §2, …`; tag each with the phase that produced it. Seeded at the PA 2.0 migration (Phase 9.2.5) from the legacy PhaseEnds — full origin detail lives in `phase-ends/PhaseEnd_Phase*.md`.*

## §1 — Deterministic RNG idioms *(Phases 1, 5, 5.5, 5.99)*

- **Platform-independent seed hash:** never `string.GetHashCode()` (unstable across runtimes) — use a djb2-style polynomial hash so a seed reproduces everywhere. `SeedEngine.Resolve` is the one entry point; `CreateRandom()` hands out the single instance.
- **Document RNG call counts per sub-step** (e.g. "5×11 column shuffle, then 11 summon-split draws") so any reordering is caught by a test. Prefer **0-RNG deterministic** operations for biases (a swap with the highest-Dex char consumes no draws).
- **Float-free weighted RNG:** integer weight table scaled ×100,000 (`DrawGeometric`) gives geometric probabilities without floats in the deterministic stream.
- **Sort before you shuffle:** dedupe and sort inputs (e.g. AbilityRefs alphabetically) before any `Random` call, so output is independent of dictionary/traversal order. Feed collections to shuffle methods in a stable, consistent order.
- **Pool-split idiom:** Fisher-Yates the merged pool once, then slice halves — guarantees no entry is shared between two recipients. **Budget-preserving shuffle:** Fisher-Yates redistributes values while keeping the total (gem costs, stat sums) fixed.
- **Item-draws-before-price-draws:** pin an exact per-mode call order (all item selections, then all price selections; never interleave). `Off` modes consume zero RNG.

## §2 — Binary codec & Unity-archive porting *(Phases 3, 4, 9.1, 9.2)*

- **Sequential opcode decoding beats byte-pattern matching.** Pattern-matching on opcode bytes fires on argument data in unrelated opcodes (the whole `0x48`-in-arg-data false-positive class). Decode sequentially from the ported HW opcode table (`FieldScriptOpcodeTable`).
- **Field-width bugs cascade:** `AddItem` (0x48) count is **uint8**, not uint16 — reading 2 bytes consumes one extra and corrupts every downstream position. Get widths from the HW table verbatim.
- **Byte-identical round-trip codec pattern:** decode into structs that retain the original `entry_offset`/`entry_size`/`function_point`; encode into a **zero-filled, pre-sized buffer** and write those stored values verbatim → all padding reproduces exactly.
- **Derive skip/length tables from the real decoder's type list** (`VarOpList`), **not** GUI metadata (`VarOpTypeList`) — the two diverged and silently mis-sized 18+ tokens. Validate every binary offset against a **second** independent source (HW macro + prior v2.2 research).
- **`Length == 0` bodies are legal** (empty world-map functions) — guard on `< 0` only. **`0xFF` prefix accumulation** decodes extended opcodes (0x100–0x111). Variable-length dispatch opcodes are still computable: read case count N, advance a formula (`5+N*2` for 0x0B, `3+N*4` for 0x06), and **continue**.
- **Unity AssetBundle full-path extraction:** when many entries share a short name (`dbfile0000.raw16`), leaf-name lookup returns the wrong entry — parse the type-142 AssetBundle entry to build `full-path → info → index` tables and `ExtractByPath`. **Pin locale** with an explicit `/field/us/` prefix (short-name extract can return a JP variant of different size). Archive entry names are stored **without** `.bytes`.
- **No repacking:** write patched binaries as raw `.bytes` into the mod folder; the game reads them directly. Always defensive-copy on construct and on `ToBytes()`.

## §3 — FFIX data model & item-obtainability *(Phases 4, 5.6, 5.8, 5.9.x)*

- **FF9 field item encoding (Treasure variable):** `X<512` = item ID; `512≤X<1000` = card (`X−512`); `1000≤X<29999` = gil (`X−1000`); `X≥29999` = disabled sentinel. Use a **range guard (`>=`)** for sentinel skips, never `==` (dead-code placeholders like 64776 slip through `==`).
- **Item ID 0 is a null sentinel** (`AddItem(0,…)` = "give nothing") — filter `itemId < 1` before aggregating, or a real ID-0 count inflates. Verify by reading one offending script first.
- **Call-sites ≠ quantities:** a scanner counting AddItem instructions measures a different thing than guide data counting total quantity received — never sum both into one counter. Guard hardcoded reference counts to fire only when the scanner count is 0.
- **Sentinel-encoded availability map:** one `Dictionary<int,int>` — `int.MaxValue` = infinite source, `N` = summed finite copies, **absent = not obtainable**. Collapse boolean flag-pairs into one int (`AuctionCount`). Transitive infinite resolution: a synthesis result is infinite only if **all** ingredients trace to infinite sources.
- **`Price ≤ 2` means "not shop-purchasable," NOT "key item."** Gems (224–235) are Price=2 but not key items — exclude them (`isKey = Price≤2 && !isGem`). Real FFIX key items live outside Items.csv (IDs 0–255) → Gen2+.
- **Parse item display names from `ParsedCsv.InlineComments` at Build() time** (strip `#`/`;`, split on first ` - `) — the catalog is the single source of names; never hardcode them elsewhere.

## §4 — Memoria CSV & mod output *(Phases 2, 8)*

- **cp1252 on .NET Core is absent by default** — `Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)` must run in **every** assembly that touches it (parser **and** tests) or reads throw. Auto-detect: strict UTF-8 decode, fall back to cp1252.
- **Memoria bool convention is 0/1**, not True/False (`MemoriaBoolConverter`). Preserve BOM, line-ending style, comment blocks (`#`/`#!`/inline `;#`) and re-emit them to keep round-trip diffs clean. Where formatting isn't reconstructable (CommandSets tab padding), downgrade to **content-identical** and assert values.
- **Manage `Memoria.ini` via `FolderNames`, not `Priorities`** (launcher-only). Line-by-line INI edit that preserves all non-target lines byte-identically; prepend the seed folder, strip old `StiltzkinsBag-Seed-*`, keep every non-SB entry. Idempotent.
- **Determinism test pattern:** run generation twice, byte-compare — **exclude timestamp-bearing files** (`ModDescription.xml`, spoiler log); back up/restore `Memoria.ini` around each run.
- **Let a discovery reshape the contract:** when ShopItems.csv turned out to have no price column, widening the return type to a result record carrying both shops and modified Items.csv beat hacking per-shop prices.

## §5 — WPF / MVVM *(Phase 7)*

- **Frameless transparent window:** never `WindowState=Maximized` with `AllowsTransparency=True`+`WindowStyle=None` (WPF adds an 8px phantom-chrome offset) — size to `SystemParameters.WorkArea` manually and track restore bounds.
- **Corner clipping:** only `Border.Background` clips to `CornerRadius`; `Grid.Background`/`Rectangle.Fill` do not, even with `ClipToBounds`. MaterialDesign sets `Foreground` per-control, overriding window-level `TextElement.Foreground` — add implicit per-control styles.
- **Computed enable-properties must be notified from ALL contributing inputs** (`OnPropertyChanged(nameof(IsComputed))` in every contributor's `On[X]Changed`), or they never update. Master toggles gate sub-option **visibility only** — a disabled checkbox retains its last value, so AND every sub-option with its master in `BuildSettings()`.
- **Settings string** = base64url JSON, generated on-demand (Copy button), not auto-updated. Use `IsNullOrWhiteSpace` (not `??`) restoring seed/path — `??` lets `string.Empty` through and silently fails `CanGenerate`.

<!-- Grow me. Each entry = a reusable lesson, captured in-session, cross-linked to the rule or tool it informs. -->
