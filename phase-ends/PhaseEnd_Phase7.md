# Phase 7 — PhaseEnd

**Completed:** 2026-04-03
**Tests at phase end:** 819 total — all green, 0 warnings (no backend changes this phase)
**Next phase:** 8 (RandomizerEngine pipeline wiring + Mod Output + Memoria Integration)

---

## Phase 7 Task Checklist

| # | Task | Status |
|---|------|--------|
| 1 | GamePathLocator — registry auto-detect, FF9_Launcher.exe + Memoria.ini | ✅ Done |
| 2 | MainViewModel skeleton + all Settings properties mirrored | ✅ Done |
| 3 | MainWindow.xaml full layout | ✅ Done |
| 4 | RandomSeedCommand | ✅ Done |
| 5 | BrowsePathCommand (WinForms FolderBrowserDialog) | ✅ Done |
| 6 | Mode toggle mutual exclusion (Recommended/Chaos RadioButtons) | ✅ Done |
| 7 | GenerateCommand + IProgress + stub | ✅ Done |
| 8 | Settings string (JSON→base64url, Copy/Import) | ✅ Done |
| 9 | Named presets (Recommended/Chaos/Casual/Competitive) | ✅ Done |
| 10 | Custom chrome + FF9 theme | ✅ Done |
| 7a | ViewModel — all missing Settings properties + enum sources | ✅ Done |
| 7b | BuildSettings() + ApplySettings() fully wired | ✅ Done |
| 7c | Presets updated for all new Settings properties | ✅ Done |
| 7d | XAML — Characters section expanded (Ability Gems, AP Costs, Gear Stats) | ✅ Done |
| 7e | XAML — Items section expanded (Shops, Synthesis, Challenge Modifiers) | ✅ Done |
| 7f | XAML — Tetramaster section expanded (Card Stats, Types, Arrows, Sets, NPC) | ✅ Done |
| 10.5 | UI details — master toggles, computed enable properties, UX polish | ✅ Done |
| 11 | Sprite animation — SB-loading.gif overlay during generation | ✅ Done |

---

## What Was Built

### Tasks 1–3 — Foundation

**`GamePathLocator.cs`** (`StiltzkinsBag.App/Services/`):
- `TryAutoDetect()` — checks 3 Windows registry keys for FFIX Steam install path
- `Validate(path)` → `PathValidationResult` enum: `NotFound`, `ValidNoMemoria`, `ValidWithMemoria`
- Detects `FF9_Launcher.exe` (required) + `Memoria.ini` (optional but strongly recommended)
- Validation message shown in UI: ✓ green / ⚠ orange / ✗ red

**`MainWindow.xaml.cs`** (`StiltzkinsBag.App/Views/`):
- Custom chrome: `TitleBar_MouseLeftButtonDown` handles drag + double-click maximize
- `ToggleMaximize()` — manually sets `Left/Top/Width/Height` to `SystemParameters.WorkArea`
  — never uses `WindowState = Maximized` (8px offset bug with `AllowsTransparency=True`)
- `MinimizeButton_Click`, `CloseButton_Click`

---

### Tasks 4–9 — Commands + Features

**Seed:** `RandomSeedCommand` generates 8-digit numeric string via `Random.Shared`
(NOT the seeded game RNG — session convenience only).

**Path browsing:** `BrowsePathCommand` uses `System.Windows.Forms.FolderBrowserDialog`
with `UseWindowsForms=true` in App csproj. Pre-populated with current path.

**Mode toggle:** `IsRecommendedMode` / `IsChaosMode` backed by private `_mode` field.
Hand-written properties (not `[ObservableProperty]`) — setting one fires INPC on both.
No public `Mode` property to avoid a third binding surface.

**GenerateCommand:** `[RelayCommand(CanExecute = nameof(CanGenerate))]` gates on
`!IsGenerating && IsGamePathValid && !string.IsNullOrWhiteSpace(SeedString)`.
Phase 8 stub: 500ms delay, sets progress to 1.0. Replace with `RandomizerEngine.RunAsync`.

**Settings string (Copy/Import):**
- Copy: `BuildSettings().ToSettingsString()` (base64url JSON) → clipboard → StatusText confirmation
- Import: `Clipboard.GetText()` → `Settings.FromSettingsString()` → `ApplySettings()`
- TextBox is display-only (`IsReadOnly=True`, `IsHitTestVisible=False`) — clipboard-only workflow
- Both operations confirmed via StatusText: "Settings string copied to clipboard." / "Settings imported from clipboard."
- NOT auto-updated on property changes — generated fresh on Copy only

**Presets:** `Presets.cs` (`StiltzkinsBag.App/Models/`) — static factory class:
- `Get(string name)` → `Settings?`
- Four presets: Recommended / Chaos / Casual / Competitive
- All 40+ Settings properties set explicitly in each preset
- Game path and seed string always blank in preset — caller restores them

---

### Task 10 — Custom Chrome + FF9 Theme

**`FF9Palette.xaml`** (`StiltzkinsBag.App/Resources/Themes/`):
- Core palette: `SB.Primary` (#2A4090), `SB.Accent` (#CC9933 amber gold), `SB.Background` (#0D1018)
- Bevel system: `SB.Bevel.Mid` (#DCE8F8 silver), `SB.Bevel.MidDark` (#182848 shadow),
  `SB.Bevel.Inner` (#6080B8 separator), `SB.Bevel.Outer` (#141420 near-black)
- Font resources: `FF9Font` (Century Gothic), `FF9FontBold` (Century Gothic Bold)
  — TTFs at `Resources/Fonts/`, Build Action=Resource
- MaterialDesign overrides: `PrimaryHueMidBrush` (#2A4090), `MaterialDesignBody` (#FFFFFF),
  `MaterialDesignBodyLight` (#A0BCD8)
- Implicit styles: CheckBox, RadioButton, ComboBox, TextBox, ComboBoxItem — all white Foreground
- Tile brushes: `SB.TileBrush.Surface` (bgc.png 76×76), `SB.TileBrush.Background` (bgc2.png 76×76)

**`MainWindow.xaml` outer bevel (3-layer, no separate outer dark ring):**
```
Layer 1: Silver ring  CornerRadius=11  BorderThickness=4  #DCE8F8
Layer 2: Shadow ring  CornerRadius=8   BorderThickness=1  #182848 (Background + Border)
Layer 3: Separator    CornerRadius=7   BorderThickness=1  #6080B8  ClipToBounds=True
         Border.Background = bgc2.png tile (clips to CornerRadius automatically)
```

**`FF9Panel` style (section cards, same 3-layer bevel):**
```
Layer 1: Silver ring  CornerRadius=13  BorderThickness=4
Layer 2: Shadow ring  CornerRadius=10  BorderThickness=1  (Background + Border)
Layer 3: Separator    CornerRadius=9   BorderThickness=1
         Border.Background = bgc.png tile
         Grid (Background=Transparent) → ContentPresenter Margin=14,12
```

**Critical WPF discovery:** `Border.Background` clips to `CornerRadius` naturally.
`Grid.Background` and `Rectangle.Fill` do NOT — even with `ClipToBounds=True` on parent.
The Grid wrapper around ContentPresenter is required; direct ContentPresenter-as-Border-child
causes layout measurement failure.

**Title bar:** bgc.png tile via `Border.Background`. Clipped by outer `ClipToBounds=True`.
Top corners issue noted (parking lot) — WPF limitation with `AllowsTransparency`.

**Scrollbar:** `FF9ScrollBar.xaml` — custom blue/white FF9 style.

**Section headers:** `SectionHeaderRule` style — FF9FontBold, FontSize=12, horizontal rule.

**Maximize fix:** `AllowsTransparency=True` + `WindowStyle=None` causes WPF to add 8px
offset when using `WindowState=Maximized`. Fix: manually set `Left/Top/Width/Height` to
`SystemParameters.WorkArea`. Track restore bounds in `_restoreBounds`. Never call
`WindowState=Maximized`.

---

### Tasks 7a–7f — All Backend Settings Exposed

**ViewModel properties added (~45 new `[ObservableProperty]` fields):**

Characters:
- `StartingItemMode`, `RandomizeStartingCounts`
- `RandomizeAbilityGems`, `AbilityGemMode`, `AbilityGemMinCost/MaxCost`
- `RandomizeAbilityAp`, `AbilityApMode`, `ApScaleMinPercent/MaxPercent`, `ApFlatCost`
- `RandomizeGearStatBonuses`, `GearStatMode`, `GearStatWeighting`, `ZeroStatItemsCanGainStats`

Items:
- `ShopMode`, `ShopSizeMode`, `ShopFixedSize`, `ShopItemPool`, `ShopMedicMinShops`
- `RandomizeSynthesisResults`, `RandomizeSynthesisIngredients`, `AllowNewSynthesisResults`
- `SynthesisPriceMode`, `SynthPriceMin/Max`, `SynthPriceScaleMin/MaxPercent`
- `BadEconomy`, `BadEconomyPriceMultiplierMin/Max`, `ShortSupply`, `ShortSupplyMaxItems`

Tetramaster:
- `CardStatMode`, `CardStatMin/Max`, `CardTypeMode`, `ArrowMode`
- `RandomizeCardSets`, `CardSetMode`, `NpcDifficultyMode`

All numeric Settings fields stored as `string` in ViewModel — `ParseInt`/`ParseFloat` helpers
in `BuildSettings()` convert with fallbacks. Avoids converter boilerplate for TextBox binding.

---

### Task 10.5 — UI Details

**Startup:** Constructor loads `Presets.Recommended()` via `ApplySettings()`, then restores
auto-detected path. Ensures UI defaults always match Recommended preset exactly.

**SeedString fix:** `ApplySettings` uses `string.IsNullOrWhiteSpace` not `??` — guarantees
a generated seed on startup even when preset `SeedString` is `string.Empty`.

**Computed enable properties (all read-only, no backing field, notified from partial methods):**

| Property | Condition |
|---|---|
| `IsEquipmentOptionsActive` | RandomizeCharacters AND RandomizeEquipment |
| `IsStartingItemOptionsActive` | RandomizeCharacters AND RandomizeStartingItems |
| `IsShopMedicOptionsActive` | RandomizeShops AND ShopIncludeMedicItems |
| `IsAllowNewResultsActive` | RandomizeSynthesis AND RandomizeSynthesisResults |
| `IsStiltzkinPoolActive` | RandomizeStiltzkin AND StiltzkinMode == Recommended |
| `IsStiltzkinPriceActive` | RandomizeStiltzkin AND mode ≠ Off AND mode ≠ IncludeInFieldPool |
| `IsCardStatsActive` | RandomizeTetraMaster AND RandomizeCardStats |
| `IsCardSetsActive` | RandomizeTetraMaster AND RandomizeCardSets |
| `IsNpcDecksActive` | RandomizeTetraMaster AND RandomizeDecks |

**UI-only master toggles (no Settings counterpart):**
- `RandomizeCharacters` — convenience toggle; synced from sub-option values in `ApplySettings`
- `RandomizeEnemies` — same
- `RandomizeStiltzkin` — master for Stiltzkin section; derived from `StiltzkinMode != Off` in `ApplySettings`

**Status text:** `SB.TextPrimaryBrush` (white), `FontSize=13`.

**Settings String box:** `IsReadOnly=True`, `IsHitTestVisible=False`. Display-only.
Tooltip explains clipboard-only Copy/Import workflow.

---

### Task 11 — Sprite Animation

**Package:** `XamlAnimatedGif` NuGet added to `StiltzkinsBag.App`.

**Asset:** `SB-loading.gif` — 538×530, 36 frames, 8-bit, transparent background.
Build Action=Resource at `Resources/Sprites/SB-loading.gif`.

**Implementation:** Centered overlay Grid in `Grid.Row="1"` (content area only, not title bar).
- `Visibility="{Binding IsGenerating, Converter={StaticResource BoolToVis}}"`
- `Background="#AA0A0F1C"` — semi-transparent navy matching FF9 palette
- `Image Width=220 Height=220` with `gif:AnimationBehavior.SourceUri`
- `RepeatBehavior=Forever`, `RenderOptions.BitmapScalingMode=HighQuality`
- Renders on top via z-order; no explicit z-index needed

---

## Files Produced / Modified

| File | Status |
|------|--------|
| `StiltzkinsBag.App/Services/GamePathLocator.cs` | New |
| `StiltzkinsBag.App/ViewModels/MainViewModel.cs` | New |
| `StiltzkinsBag.App/Views/MainWindow.xaml` | New |
| `StiltzkinsBag.App/Views/MainWindow.xaml.cs` | New |
| `StiltzkinsBag.App/Resources/Themes/FF9Palette.xaml` | New |
| `StiltzkinsBag.App/Resources/Themes/FF9ScrollBar.xaml` | New |
| `StiltzkinsBag.App/Resources/Sprites/bgc.png` | New (76×76, Build Action=Resource) |
| `StiltzkinsBag.App/Resources/Sprites/bgc2.png` | New (76×76, Build Action=Resource) |
| `StiltzkinsBag.App/Resources/Sprites/SB-loading.gif` | New (538×530, 36 frames) |
| `StiltzkinsBag.App/Resources/Fonts/centurygothic.ttf` | New (Build Action=Resource) |
| `StiltzkinsBag.App/Resources/Fonts/centurygothic-bold.ttf` | New (Build Action=Resource) |
| `StiltzkinsBag.App/Models/Presets.cs` | New |
| `StiltzkinsBag.App/StiltzkinsBag.App.csproj` | Modified (UseWindowsForms=true, XamlAnimatedGif package) |

---

## Key Discoveries

| Discovery | Impact |
|---|---|
| `Border.Background` clips to `CornerRadius`; `Grid.Background` / `Rectangle.Fill` do NOT even with `ClipToBounds=True` | All tile brush backgrounds must be set on `Border.Background`, not on child elements |
| `ContentPresenter` as direct child of `Border` with `Border.Background` as property element causes layout measurement failure | Always wrap `ContentPresenter` in a `<Grid Background="Transparent">` inside the styled Border |
| `WindowState=Maximized` with `AllowsTransparency=True` + `WindowStyle=None` adds 8px offset (WPF compensates for non-existent chrome) | Never use `WindowState=Maximized` on transparent frameless windows; manually set `Left/Top/Width/Height` to `WorkArea` |
| Outer dark ring + silver ring: sub-pixel corner gap appears in transparent windows due to separate arc rendering | Removed outer dark ring entirely — silver ring is the outermost edge |
| MaterialDesign sets Foreground internally on CheckBox, ComboBox, RadioButton — overrides Window-level `TextElement.Foreground` | Must add implicit styles in `FF9Palette.xaml` for each control type |
| `??` null-check on `SeedString` in `ApplySettings` passes through `string.Empty` — `CanGenerate` fails | Use `string.IsNullOrWhiteSpace` in `ApplySettings` for all string fields that must be non-empty |

---

## Deviations

| Item | Plan | Actual | Reason |
|---|---|---|---|
| Settings string auto-update | Live-update on every property change | Copy-on-demand only | ComboBox/dropdown changes not reliably triggering PropertyChanged in some configurations; clipboard-only workflow is simpler and clearer |
| Stiltzkin master toggle | Not in original plan | `RandomizeStiltzkin` bool added | UX requirement: master checkbox for entire section; derived from `StiltzkinMode != Off` in ApplySettings |
| Title bar top corners | Rounded via ClipToBounds | Partially rounded — outer corners still show as square | WPF limitation: `ClipToBounds` on inner border doesn't clip title bar's outermost painted corners with `AllowsTransparency=True` |
| 4-layer outer bevel | Outer dark ring + silver + shadow + separator | 3-layer (silver + shadow + separator) | 4-layer produced visible corner gap in transparent window due to separate arc rendering |
| Maximize button | None planned | Double-click title bar only | Developer did not request a maximize toggle button |

---

## Parking Lot

### Phase 8

| Item | Notes |
|------|-------|
| Wire `GenerateCommand` to `RandomizerEngine.RunAsync()` | Replace Phase 7 stub (500ms delay). See stub comment in `GenerateAsync()`. |
| Spoiler log UI | Show output path and summary after successful generation |
| Memoria Engine installation prompt with link | Show when `IsGamePathValid=True` but `IsMemoriaDetected=False` |

### Phase 9 / Polish

| Item | Notes |
|------|-------|
| **Task 10.6 — Preset tuning** | Preset values are developer's best guesses, not playtested. Recommended especially needs to reflect the developer's actual intent. Review all four presets after playtesting Phase 8 output. |
| **Treasure Chests sub-options** | `FieldItemRandomizer` has no mode enum — flat Fisher-Yates only. Backend work required before UI can expose sub-options. See Phase 5.5 parking lot "sub-option backtrack". |
| Title bar corner rendering | Top-left and top-right corners of title bar background not perfectly rounded. WPF limitation with `AllowsTransparency=True` + `Border.Background`. |
| `ShopEnsureMedicItems` default mismatch | ViewModel default `false`, Settings default `true`. Low priority — Recommended preset overrides on startup. |
| `EnemyCatalogTests` still uses JSON | Carry from Phase 5.9.3. |
| `ModSourceResolverTests` require Memoria.ini | Carry from Phase 5.9.3. |
| `ShopRandomizer Price > 2` filter | Replace with `ItemPool.ShopFriendly`. Carry from Phase 4. |

---

## Rules Added This Phase

| Rule | Reason |
|------|--------|
| When ViewModel property name differs from Settings property name, document the mapping in comments at both `BuildSettings()` and `ApplySettings()` | Several mappings (RandomizeStartingItems→RandomizeInitialItems, etc.) are non-obvious and caused silent bugs during development |
| Never use `WindowState=Maximized` with `AllowsTransparency=True` + `WindowStyle=None` — use manual `WorkArea` sizing | WPF adds 8px frame offset to compensate for non-existent chrome |
| Use `Border.Background` (not `Grid.Background` or child `Rectangle`) for tile brushes requiring `CornerRadius` corner clipping | Only `Border.Background` is rendered with the border's own `CornerRadius` applied |
| Always use the pasted current file as base for replacements — never regenerate from stale session outputs | Multiple regressions occurred when AI str_replaced its own old output instead of the developer's local version |
| Settings string is generated on demand (Copy button) only — not auto-updated on property changes | Auto-update was unreliable for dropdown/enum changes; Copy-on-demand is simpler and always correct |
| UI-only master toggles (`RandomizeCharacters`, `RandomizeEnemies`, `RandomizeStiltzkin`) do not map to Settings properties — they are convenience UI toggles only; `ApplySettings` must derive their state from sub-option values | These toggles exist for UX grouping only; including them in Settings would create redundant/conflicting state |
| Computed enable properties must be notified from ALL contributing properties — every `partial void On[X]Changed` for every input bool must call `OnPropertyChanged(nameof(IsComputedProperty))` | Missing a notification hook causes the computed property to never update when one of its inputs changes |
| `string.IsNullOrWhiteSpace` not `??` for seed/path string restoration in `ApplySettings` | `??` passes through `string.Empty`, which fails `CanGenerate` silently on startup |

---

## Commit Message

```
feat: Phase 7 complete — WPF UI

GamePathLocator.cs (new):
- TryAutoDetect: 3 registry keys for FFIX Steam path
- Validate: NotFound / ValidNoMemoria / ValidWithMemoria
- Detects FF9_Launcher.exe + Memoria.ini

MainViewModel.cs (new):
- 45+ [ObservableProperty] fields mirroring all Settings properties
- Hand-written IsRecommendedMode/IsChaosMode (RadioButton mutual exclusion)
- 9 computed enable properties (IsEquipmentOptionsActive, IsCardStatsActive, etc.)
- BuildSettings() + ApplySettings() fully wired; numeric string ↔ int/float helpers
- Constructor: loads Recommended preset, restores auto-detected path
- Copy: generates fresh string + copies + StatusText confirmation
- Import: clipboard → FromSettingsString → ApplySettings
- GenerateCommand stub (Phase 8 TODO clearly marked)

MainWindow.xaml (new):
- WindowStyle=None, AllowsTransparency=True, Background=Transparent
- 3-layer outer bevel: silver(4px) → shadow → separator + ClipToBounds
- FF9Panel style: same 3-layer bevel; Border.Background for tile clipping
- Title bar with FF9 tile, chrome buttons, Century Gothic bold text
- Full left panel: Mode, Characters (+ Ability Gems, AP Costs, Gear Stats),
  Items (+ Shops, Synthesis, Challenge Modifiers), Stiltzkin, Enemies, Tetramaster
- Full right panel: Seed, Game Path, Presets, Settings String, Generate
- All IsEnabled bindings for master toggles and computed enable properties
- Loading overlay: XamlAnimatedGif centered during generation (IsGenerating)

MainWindow.xaml.cs (new):
- Manual maximize: WorkArea sizing, _restoreBounds, no WindowState=Maximized

FF9Palette.xaml (new):
- FF9 color palette, bevel brushes, Century Gothic fonts
- MaterialDesign primary/secondary overrides (#2A4090 royal blue)
- Implicit styles: CheckBox, RadioButton, ComboBox, TextBox, ComboBoxItem

Presets.cs (new):
- Recommended / Chaos / Casual / Competitive factories
- All 40+ Settings properties set explicitly per preset

Resources (new):
- bgc.png / bgc2.png (76×76 tiles, Build Action=Resource)
- centurygothic.ttf / centurygothic-bold.ttf
- SB-loading.gif (538×530, 36 frames, transparent bg, XamlAnimatedGif)

8 new rules added. 819 tests unchanged.
```

---

## PhaseEnd Changelog

```
v1.6.0 → v1.7.0
- Build Log: Phase 7 entry added
- Key Additions: Full WPF UI; all backend settings exposed; FF9 theme;
  computed enable properties; preset factories; sprite animation overlay
- Key Discoveries: Border.Background corner clipping, ContentPresenter layout issue,
  WindowState=Maximized offset bug, MaterialDesign Foreground override,
  string.IsNullOrWhiteSpace vs null-coalescing for seed
- Deviations: Settings string copy-on-demand, Stiltzkin master toggle, 3-layer
  vs 4-layer bevel, title bar corner partial limitation
- Rules: 8 new rules added
- Parking Lot: Phase 8 (wire GenerateCommand, spoiler log, Memoria prompt),
  Phase 9 (preset tuning, chest sub-options, title bar corners, polish items)
- Phase 7 marked complete
- Current phase: 8 (RandomizerEngine + Mod Output + Memoria Integration)
```

---

## 🛑 Stop Here

This chat session is complete. Do the following before starting Phase 8:

1. Add `PhaseEnd_Phase7.md` to your Claude Project
2. Start a new chat session
3. Say: "Continue building Stiltzkin's Bag. Read all attached markdown files."

Do not continue development in this session. Do not delete this chat — keep it
for posterity and back-reference. Large chats in a project will not slow down
future sessions or use more tokens.
