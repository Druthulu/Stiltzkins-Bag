using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StiltzkinsBag.App.Services;
using StiltzkinsBag.Models;
using StiltzkinsBag.Randomizers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;

using Clipboard = System.Windows.Clipboard;

namespace StiltzkinsBag.App.ViewModels;

/// <summary>
/// Central ViewModel for the Stiltzkin's Bag main window.
///
/// On construction: loads Recommended preset, then restores auto-detected path.
/// Settings string is generated on demand (Copy button only — not auto-updated).
///
/// Computed enable properties:
///   IsEquipmentOptionsActive    = RandomizeCharacters AND RandomizeEquipment
///   IsStartingItemOptionsActive = RandomizeCharacters AND RandomizeStartingItems
///   IsShopMedicOptionsActive    = RandomizeShops AND ShopIncludeMedicItems
///   IsAllowNewResultsActive     = RandomizeSynthesis AND RandomizeSynthesisResults
///   IsStiltzkinPoolActive       = RandomizeStiltzkin AND mode == Recommended
///   IsStiltzkinPriceActive      = RandomizeStiltzkin AND mode != Off/IncludeInFieldPool
///   IsCardStatsActive           = RandomizeTetraMaster AND RandomizeCardStats
///   IsCardSetsActive            = RandomizeTetraMaster AND RandomizeCardSets
///   IsNpcDecksActive            = RandomizeTetraMaster AND RandomizeDecks
///
/// ViewModel → Settings name mapping:
///   RandomizeStartingItems → RandomizeInitialItems
///   ShopIncludeMedicItems  → ShopEnsureMedicItems
///   RandomizeCardOrder     → ShuffleCardOrder
///   RandomizeDecks         → ShuffleNpcDecks
/// </summary>
public partial class MainViewModel : ObservableObject
{
    // =========================================================================
    // Constructor
    // =========================================================================

    public MainViewModel()
    {
        var detected = GamePathLocator.TryAutoDetect();
        ApplySettings(StiltzkinsBag.App.Models.Presets.Recommended());
        if (detected is not null)
            GamePath = detected;
    }

    // =========================================================================
    // Core — seed + path + mode
    // =========================================================================

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
    private string _seedString = string.Empty;

    [ObservableProperty]
    private string _gamePath = string.Empty;

    partial void OnGamePathChanged(string value) => ValidateGamePath(value);

    private RandomizerMode _mode = RandomizerMode.Recommended;

    public bool IsRecommendedMode
    {
        get => _mode == RandomizerMode.Recommended;
        set
        {
            if (value && _mode != RandomizerMode.Recommended)
            {
                _mode = RandomizerMode.Recommended;
                OnPropertyChanged(nameof(IsRecommendedMode));
                OnPropertyChanged(nameof(IsChaosMode));
            }
        }
    }

    public bool IsChaosMode
    {
        get => _mode == RandomizerMode.Chaos;
        set
        {
            if (value && _mode != RandomizerMode.Chaos)
            {
                _mode = RandomizerMode.Chaos;
                OnPropertyChanged(nameof(IsChaosMode));
                OnPropertyChanged(nameof(IsRecommendedMode));
            }
        }
    }

    // =========================================================================
    // Characters group
    // =========================================================================

    [ObservableProperty] private bool _randomizeCharacters = false;
    partial void OnRandomizeCharactersChanged(bool value)
    {
        OnPropertyChanged(nameof(IsEquipmentOptionsActive));
        OnPropertyChanged(nameof(IsStartingItemOptionsActive));
    }

    [ObservableProperty] private bool _randomizeBaseStats = false;
    [ObservableProperty] private bool _randomizeSpeciality = false;
    [ObservableProperty] private bool _randomizeAbilities = false;

    [ObservableProperty] private bool _randomizeEquipment = false;
    [ObservableProperty] private EquipmentMode _equipmentMode = EquipmentMode.Random;
    partial void OnRandomizeEquipmentChanged(bool value) =>
        OnPropertyChanged(nameof(IsEquipmentOptionsActive));

    public IEnumerable<EquipmentMode> EquipmentModes { get; } = Enum.GetValues<EquipmentMode>();

    public bool IsEquipmentOptionsActive => RandomizeCharacters && RandomizeEquipment;

    [ObservableProperty] private bool _randomizeStartingItems = false;
    [ObservableProperty] private StartingItemMode _startingItemMode = StartingItemMode.ConsumablesRandom;
    [ObservableProperty] private bool _randomizeStartingCounts = false;
    partial void OnRandomizeStartingItemsChanged(bool value) =>
        OnPropertyChanged(nameof(IsStartingItemOptionsActive));

    public IEnumerable<StartingItemMode> StartingItemModes { get; } = Enum.GetValues<StartingItemMode>();

    public bool IsStartingItemOptionsActive => RandomizeCharacters && RandomizeStartingItems;

    // ── Ability Gems ──────────────────────────────────────────────────────────
    [ObservableProperty] private bool _randomizeAbilityGems = false;
    [ObservableProperty] private AbilityGemMode _abilityGemMode = AbilityGemMode.Shuffle;
    [ObservableProperty] private string _abilityGemMinCost = "1";
    [ObservableProperty] private string _abilityGemMaxCost = "20";
    public IEnumerable<AbilityGemMode> AbilityGemModes { get; } = Enum.GetValues<AbilityGemMode>();

    // ── Ability AP ───────────────────────────────────────────────────────────
    [ObservableProperty] private bool _randomizeAbilityAp = false;
    [ObservableProperty] private AbilityApMode _abilityApMode = AbilityApMode.ProportionalScale;
    [ObservableProperty] private string _apScaleMinPercent = "50";
    [ObservableProperty] private string _apScaleMaxPercent = "200";
    [ObservableProperty] private string _apFlatCost = "20";
    public IEnumerable<AbilityApMode> AbilityApModes { get; } = Enum.GetValues<AbilityApMode>();

    // ── Gear Stat Bonuses ────────────────────────────────────────────────────
    [ObservableProperty] private bool _randomizeGearStatBonuses = false;
    [ObservableProperty] private GearStatMode _gearStatMode = GearStatMode.Proportional;
    [ObservableProperty] private GearStatWeighting _gearStatWeighting = GearStatWeighting.Geometric;
    [ObservableProperty] private bool _zeroStatItemsCanGainStats = false;
    public IEnumerable<GearStatMode> GearStatModes { get; } = Enum.GetValues<GearStatMode>();
    public IEnumerable<GearStatWeighting> GearStatWeightings { get; } = Enum.GetValues<GearStatWeighting>();

    // =========================================================================
    // Items group
    // =========================================================================

    [ObservableProperty] private bool _randomizeTreasureChests = false;

    // ── Shops ─────────────────────────────────────────────────────────────────
    [ObservableProperty] private bool _randomizeShops = false;
    [ObservableProperty] private ShopMode _shopMode = ShopMode.Shuffle;
    [ObservableProperty] private ShopSizeMode _shopSizeMode = ShopSizeMode.Maintain;
    [ObservableProperty] private string _shopFixedSize = "4";
    [ObservableProperty] private ShopItemPool _shopItemPool = ShopItemPool.ConsumablesOnly;
    [ObservableProperty] private bool _shopIncludeMedicItems = false;
    [ObservableProperty] private string _shopMedicMinShops = "2";

    partial void OnRandomizeShopsChanged(bool value) =>
        OnPropertyChanged(nameof(IsShopMedicOptionsActive));
    partial void OnShopIncludeMedicItemsChanged(bool value) =>
        OnPropertyChanged(nameof(IsShopMedicOptionsActive));

    /// <summary>Min Shops field requires both Shops master AND Ensure Medic Items checked.</summary>
    public bool IsShopMedicOptionsActive => RandomizeShops && ShopIncludeMedicItems;

    public IEnumerable<ShopMode> ShopModes { get; } = Enum.GetValues<ShopMode>();
    public IEnumerable<ShopSizeMode> ShopSizeModes { get; } = Enum.GetValues<ShopSizeMode>();
    public IEnumerable<ShopItemPool> ShopItemPools { get; } = Enum.GetValues<ShopItemPool>();

    // ── Synthesis ────────────────────────────────────────────────────────────
    [ObservableProperty] private bool _randomizeSynthesis = false;
    [ObservableProperty] private bool _randomizeSynthesisResults = false;
    [ObservableProperty] private bool _randomizeSynthesisIngredients = false;
    [ObservableProperty] private bool _allowNewSynthesisResults = false;
    [ObservableProperty] private SynthesisPriceMode _synthesisPriceMode = SynthesisPriceMode.Vanilla;
    [ObservableProperty] private string _synthPriceMin = "100";
    [ObservableProperty] private string _synthPriceMax = "5000";
    [ObservableProperty] private string _synthPriceScaleMinPercent = "50";
    [ObservableProperty] private string _synthPriceScaleMaxPercent = "200";

    partial void OnRandomizeSynthesisChanged(bool value) =>
        OnPropertyChanged(nameof(IsAllowNewResultsActive));
    partial void OnRandomizeSynthesisResultsChanged(bool value) =>
        OnPropertyChanged(nameof(IsAllowNewResultsActive));

    /// <summary>Allow New Results requires both Synthesis master AND Randomize Results checked.</summary>
    public bool IsAllowNewResultsActive => RandomizeSynthesis && RandomizeSynthesisResults;

    public IEnumerable<SynthesisPriceMode> SynthesisPriceModes { get; } = Enum.GetValues<SynthesisPriceMode>();

    // ── Challenge modifiers ──────────────────────────────────────────────────
    [ObservableProperty] private bool _badEconomy = false;
    [ObservableProperty] private string _badEconomyPriceMultiplierMin = "1.5";
    [ObservableProperty] private string _badEconomyPriceMultiplierMax = "4.0";
    [ObservableProperty] private bool _shortSupply = false;
    [ObservableProperty] private string _shortSupplyMaxItems = "3";

    // =========================================================================
    // Stiltzkin group
    // =========================================================================

    [ObservableProperty] private bool _randomizeStiltzkin = false;
    partial void OnRandomizeStiltzkinChanged(bool value)
    {
        OnPropertyChanged(nameof(IsStiltzkinPoolActive));
        OnPropertyChanged(nameof(IsStiltzkinPriceActive));
    }

    [ObservableProperty] private StiltzkinMode _stiltzkinMode = StiltzkinMode.Off;
    [ObservableProperty] private StiltzkinRecommendedSubMode _stiltzkinSubMode = StiltzkinRecommendedSubMode.Fun;
    [ObservableProperty] private StiltzkinPriceMode _stiltzkinPriceMode = StiltzkinPriceMode.Off;

    public IEnumerable<StiltzkinMode> StiltzkinModes { get; } = Enum.GetValues<StiltzkinMode>();
    public IEnumerable<StiltzkinRecommendedSubMode> StiltzkinSubModes { get; } = Enum.GetValues<StiltzkinRecommendedSubMode>();
    public IEnumerable<StiltzkinPriceMode> StiltzkinPriceModes { get; } = Enum.GetValues<StiltzkinPriceMode>();

    public bool IsStiltzkinPoolActive => RandomizeStiltzkin && StiltzkinMode == StiltzkinMode.Recommended;
    public bool IsStiltzkinPriceActive => RandomizeStiltzkin
                                        && StiltzkinMode != StiltzkinMode.Off
                                        && StiltzkinMode != StiltzkinMode.IncludeInFieldPool;

    partial void OnStiltzkinModeChanged(StiltzkinMode value)
    {
        OnPropertyChanged(nameof(IsStiltzkinPoolActive));
        OnPropertyChanged(nameof(IsStiltzkinPriceActive));
    }

    // =========================================================================
    // Enemies group
    // =========================================================================

    [ObservableProperty] private bool _randomizeEnemies = false;
    [ObservableProperty] private bool _randomizeItemDrops = false;
    [ObservableProperty] private bool _randomizeItemSteals = false;
    [ObservableProperty] private bool _randomizeBlueMagic = false;
    [ObservableProperty] private bool _randomizeCardDrops = false;

    // =========================================================================
    // Tetramaster group
    // =========================================================================

    [ObservableProperty] private bool _randomizeTetraMaster = false;
    partial void OnRandomizeTetraMasterChanged(bool value)
    {
        OnPropertyChanged(nameof(IsCardStatsActive));
        OnPropertyChanged(nameof(IsCardSetsActive));
        OnPropertyChanged(nameof(IsNpcDecksActive));
    }

    [ObservableProperty] private bool _randomizeCardStats = false;
    [ObservableProperty] private CardStatMode _cardStatMode = CardStatMode.Shuffle;
    [ObservableProperty] private string _cardStatMin = "1";
    [ObservableProperty] private string _cardStatMax = "15";
    public IEnumerable<CardStatMode> CardStatModes { get; } = Enum.GetValues<CardStatMode>();

    partial void OnRandomizeCardStatsChanged(bool value) =>
        OnPropertyChanged(nameof(IsCardStatsActive));

    public bool IsCardStatsActive => RandomizeTetraMaster && RandomizeCardStats;

    [ObservableProperty] private CardTypeMode _cardTypeMode = CardTypeMode.Preserve;
    public IEnumerable<CardTypeMode> CardTypeModes { get; } = Enum.GetValues<CardTypeMode>();

    [ObservableProperty] private ArrowMode _arrowMode = ArrowMode.Preserve;
    public IEnumerable<ArrowMode> ArrowModes { get; } = Enum.GetValues<ArrowMode>();

    [ObservableProperty] private bool _randomizeCardOrder = false;

    [ObservableProperty] private bool _randomizeCardSets = false;
    [ObservableProperty] private CardSetMode _cardSetMode = CardSetMode.Shuffle;
    public IEnumerable<CardSetMode> CardSetModes { get; } = Enum.GetValues<CardSetMode>();

    partial void OnRandomizeCardSetsChanged(bool value) =>
        OnPropertyChanged(nameof(IsCardSetsActive));

    /// <summary>Card Sets Mode dropdown requires both Tetramaster master AND Card Sets checked.</summary>
    public bool IsCardSetsActive => RandomizeTetraMaster && RandomizeCardSets;

    [ObservableProperty] private bool _randomizeDecks = false;
    [ObservableProperty] private NpcDifficultyMode _npcDifficultyMode = NpcDifficultyMode.Preserve;
    public IEnumerable<NpcDifficultyMode> NpcDifficultyModes { get; } = Enum.GetValues<NpcDifficultyMode>();

    partial void OnRandomizeDecksChanged(bool value) =>
        OnPropertyChanged(nameof(IsNpcDecksActive));

    /// <summary>NPC Difficulty dropdown requires both Tetramaster master AND Shuffle NPC Decks checked.</summary>
    public bool IsNpcDecksActive => RandomizeTetraMaster && RandomizeDecks;

    // =========================================================================
    // UI state — not part of Settings
    // =========================================================================

    [ObservableProperty] private string _statusText = "Ready.";
    [ObservableProperty] private double _generationProgress = 0.0;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
    private bool _isGenerating = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
    private bool _isGamePathValid = false;

    [ObservableProperty] private bool _isMemoriaDetected = false;
    [ObservableProperty] private string _gamePathValidationMessage = string.Empty;
    [ObservableProperty] private string _settingsString = string.Empty;

    // =========================================================================
    // Commands
    // =========================================================================

    [RelayCommand(CanExecute = nameof(CanGenerate))]
    private async Task GenerateAsync()
    {
        IsGenerating = true;
        GenerationProgress = 0.0;
        StatusText = "Starting generation…";

        try
        {
            var settings = BuildSettings();
            _ = settings;

            // --- STUB: remove in Phase 8 ---
            await Task.Delay(500);
            GenerationProgress = 1.0;
            StatusText = "Generation stub — wire RandomizerEngine in Phase 8.";
            // --- END STUB ---
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
            GenerationProgress = 0.0;
        }
        finally
        {
            IsGenerating = false;
            GenerateCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanGenerate() =>
        !IsGenerating && IsGamePathValid && !string.IsNullOrWhiteSpace(SeedString);

    [RelayCommand]
    private void RandomSeed() => SeedString = GenerateRandomSeedString();

    [RelayCommand]
    private void AutoDetectPath()
    {
        var detected = GamePathLocator.TryAutoDetect();
        if (detected is not null)
            GamePath = detected;
        else
            StatusText = "Auto-detect: FFIX installation not found. Please browse manually.";
    }

    [RelayCommand]
    private void BrowsePath()
    {
        var dlg = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select your Final Fantasy IX installation folder",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false,
        };
        if (!string.IsNullOrWhiteSpace(GamePath) && System.IO.Directory.Exists(GamePath))
            dlg.InitialDirectory = GamePath;
        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            GamePath = dlg.SelectedPath;
    }

    [RelayCommand]
    private void CopySettingsString()
    {
        SettingsString = BuildSettings().ToSettingsString();
        if (!string.IsNullOrWhiteSpace(SettingsString))
        {
            Clipboard.SetText(SettingsString);
            StatusText = "Settings string copied to clipboard.";
        }
    }

    [RelayCommand]
    private void ImportSettingsString()
    {
        var raw = Clipboard.GetText();
        if (string.IsNullOrWhiteSpace(raw)) { StatusText = "Nothing on clipboard to import."; return; }

        var imported = Settings.FromSettingsString(raw);
        if (imported is null) { StatusText = "Import failed — invalid or unrecognised settings string."; return; }

        var currentPath = GamePath;
        ApplySettings(imported);
        GamePath = currentPath;
        StatusText = "Settings imported from clipboard.";
    }

    [RelayCommand]
    private void LoadPreset(string presetName)
    {
        var preset = StiltzkinsBag.App.Models.Presets.Get(presetName);
        if (preset is null) { StatusText = $"Unknown preset '{presetName}'."; return; }

        var currentPath = GamePath;
        var currentSeed = SeedString;
        ApplySettings(preset);
        GamePath = currentPath;
        SeedString = currentSeed;
        StatusText = $"Preset '{presetName}' loaded.";
    }

    // =========================================================================
    // Private helpers
    // =========================================================================

    private Settings BuildSettings() => new()
    {
        SeedString = SeedString,
        SeedInt = SeedEngine.Resolve(SeedString),
        GamePath = GamePath,
        Mode = _mode,

        RandomizeBaseStats = RandomizeBaseStats,
        RandomizeSpeciality = RandomizeSpeciality,
        RandomizeAbilities = RandomizeAbilities,
        RandomizeEquipment = RandomizeEquipment,
        EquipmentMode = EquipmentMode,
        RandomizeInitialItems = RandomizeStartingItems,
        StartingItemMode = StartingItemMode,
        RandomizeStartingCounts = RandomizeStartingCounts,

        RandomizeAbilityGems = RandomizeAbilityGems,
        AbilityGemMode = AbilityGemMode,
        AbilityGemMinCost = ParseInt(_abilityGemMinCost, 1),
        AbilityGemMaxCost = ParseInt(_abilityGemMaxCost, 20),

        RandomizeAbilityAp = RandomizeAbilityAp,
        AbilityApMode = AbilityApMode,
        ApScaleMinPercent = ParseInt(_apScaleMinPercent, 50),
        ApScaleMaxPercent = ParseInt(_apScaleMaxPercent, 200),
        ApFlatCost = ParseInt(_apFlatCost, 20),

        RandomizeGearStatBonuses = RandomizeGearStatBonuses,
        GearStatMode = GearStatMode,
        GearStatWeighting = GearStatWeighting,
        ZeroStatItemsCanGainStats = ZeroStatItemsCanGainStats,

        RandomizeTreasureChests = RandomizeTreasureChests,
        RandomizeShops = RandomizeShops,
        ShopMode = ShopMode,
        ShopSizeMode = ShopSizeMode,
        ShopFixedSize = ParseInt(_shopFixedSize, 4),
        ShopItemPool = ShopItemPool,
        ShopEnsureMedicItems = ShopIncludeMedicItems,
        ShopMedicMinShops = ParseInt(_shopMedicMinShops, 2),

        RandomizeSynthesis = RandomizeSynthesis,
        RandomizeSynthesisResults = RandomizeSynthesisResults,
        RandomizeSynthesisIngredients = RandomizeSynthesisIngredients,
        AllowNewSynthesisResults = AllowNewSynthesisResults,
        SynthesisPriceMode = SynthesisPriceMode,
        SynthPriceMin = ParseInt(_synthPriceMin, 100),
        SynthPriceMax = ParseInt(_synthPriceMax, 5000),
        SynthPriceScaleMinPercent = ParseInt(_synthPriceScaleMinPercent, 50),
        SynthPriceScaleMaxPercent = ParseInt(_synthPriceScaleMaxPercent, 200),

        BadEconomy = BadEconomy,
        BadEconomyPriceMultiplierMin = ParseFloat(_badEconomyPriceMultiplierMin, 1.5f),
        BadEconomyPriceMultiplierMax = ParseFloat(_badEconomyPriceMultiplierMax, 4.0f),
        ShortSupply = ShortSupply,
        ShortSupplyMaxItems = ParseInt(_shortSupplyMaxItems, 3),

        StiltzkinMode = StiltzkinMode,
        StiltzkinRecommendedSubMode = StiltzkinSubMode,
        StiltzkinPriceMode = StiltzkinPriceMode,

        RandomizeItemDrops = RandomizeItemDrops,
        RandomizeItemSteals = RandomizeItemSteals,
        RandomizeBlueMagic = RandomizeBlueMagic,
        RandomizeCardDrops = RandomizeCardDrops,

        RandomizeTetraMaster = RandomizeTetraMaster,
        RandomizeCardStats = RandomizeCardStats,
        CardStatMode = CardStatMode,
        CardStatMin = ParseInt(_cardStatMin, 1),
        CardStatMax = ParseInt(_cardStatMax, 15),
        CardTypeMode = CardTypeMode,
        ArrowMode = ArrowMode,
        ShuffleCardOrder = RandomizeCardOrder,
        RandomizeCardSets = RandomizeCardSets,
        CardSetMode = CardSetMode,
        ShuffleNpcDecks = RandomizeDecks,
        NpcDifficultyMode = NpcDifficultyMode,
    };

    private void ApplySettings(Settings s)
    {
        SeedString = string.IsNullOrWhiteSpace(s.SeedString) ? GenerateRandomSeedString() : s.SeedString;
        GamePath = s.GamePath ?? string.Empty;

        _mode = s.Mode;
        OnPropertyChanged(nameof(IsRecommendedMode));
        OnPropertyChanged(nameof(IsChaosMode));

        RandomizeBaseStats = s.RandomizeBaseStats;
        RandomizeSpeciality = s.RandomizeSpeciality;
        RandomizeAbilities = s.RandomizeAbilities;
        RandomizeEquipment = s.RandomizeEquipment;
        EquipmentMode = s.EquipmentMode;
        RandomizeStartingItems = s.RandomizeInitialItems;
        StartingItemMode = s.StartingItemMode;
        RandomizeStartingCounts = s.RandomizeStartingCounts;

        RandomizeAbilityGems = s.RandomizeAbilityGems;
        AbilityGemMode = s.AbilityGemMode;
        AbilityGemMinCost = s.AbilityGemMinCost.ToString();
        AbilityGemMaxCost = s.AbilityGemMaxCost.ToString();

        RandomizeAbilityAp = s.RandomizeAbilityAp;
        AbilityApMode = s.AbilityApMode;
        ApScaleMinPercent = s.ApScaleMinPercent.ToString();
        ApScaleMaxPercent = s.ApScaleMaxPercent.ToString();
        ApFlatCost = s.ApFlatCost.ToString();

        RandomizeGearStatBonuses = s.RandomizeGearStatBonuses;
        GearStatMode = s.GearStatMode;
        GearStatWeighting = s.GearStatWeighting;
        ZeroStatItemsCanGainStats = s.ZeroStatItemsCanGainStats;

        RandomizeTreasureChests = s.RandomizeTreasureChests;
        RandomizeShops = s.RandomizeShops;
        ShopMode = s.ShopMode;
        ShopSizeMode = s.ShopSizeMode;
        ShopFixedSize = s.ShopFixedSize.ToString();
        ShopItemPool = s.ShopItemPool;
        ShopIncludeMedicItems = s.ShopEnsureMedicItems;
        ShopMedicMinShops = s.ShopMedicMinShops.ToString();

        RandomizeSynthesis = s.RandomizeSynthesis;
        RandomizeSynthesisResults = s.RandomizeSynthesisResults;
        RandomizeSynthesisIngredients = s.RandomizeSynthesisIngredients;
        AllowNewSynthesisResults = s.AllowNewSynthesisResults;
        SynthesisPriceMode = s.SynthesisPriceMode;
        SynthPriceMin = s.SynthPriceMin.ToString();
        SynthPriceMax = s.SynthPriceMax.ToString();
        SynthPriceScaleMinPercent = s.SynthPriceScaleMinPercent.ToString();
        SynthPriceScaleMaxPercent = s.SynthPriceScaleMaxPercent.ToString();

        BadEconomy = s.BadEconomy;
        BadEconomyPriceMultiplierMin = s.BadEconomyPriceMultiplierMin.ToString(CultureInfo.InvariantCulture);
        BadEconomyPriceMultiplierMax = s.BadEconomyPriceMultiplierMax.ToString(CultureInfo.InvariantCulture);
        ShortSupply = s.ShortSupply;
        ShortSupplyMaxItems = s.ShortSupplyMaxItems.ToString();

        StiltzkinMode = s.StiltzkinMode;
        StiltzkinSubMode = s.StiltzkinRecommendedSubMode;
        StiltzkinPriceMode = s.StiltzkinPriceMode;
        RandomizeStiltzkin = s.StiltzkinMode != StiltzkinMode.Off;

        RandomizeItemDrops = s.RandomizeItemDrops;
        RandomizeItemSteals = s.RandomizeItemSteals;
        RandomizeBlueMagic = s.RandomizeBlueMagic;
        RandomizeCardDrops = s.RandomizeCardDrops;

        RandomizeTetraMaster = s.RandomizeTetraMaster;
        RandomizeCardStats = s.RandomizeCardStats;
        CardStatMode = s.CardStatMode;
        CardStatMin = s.CardStatMin.ToString();
        CardStatMax = s.CardStatMax.ToString();
        CardTypeMode = s.CardTypeMode;
        ArrowMode = s.ArrowMode;
        RandomizeCardOrder = s.ShuffleCardOrder;
        RandomizeCardSets = s.RandomizeCardSets;
        CardSetMode = s.CardSetMode;
        RandomizeDecks = s.ShuffleNpcDecks;
        NpcDifficultyMode = s.NpcDifficultyMode;

        RandomizeCharacters = RandomizeBaseStats || RandomizeSpeciality ||
                              RandomizeAbilities || RandomizeEquipment ||
                              RandomizeStartingItems;
        RandomizeEnemies = RandomizeItemDrops || RandomizeItemSteals ||
                              RandomizeBlueMagic || RandomizeCardDrops;

        SettingsString = string.Empty;
    }

    private void ValidateGamePath(string path)
    {
        var result = GamePathLocator.Validate(path);
        IsGamePathValid = result != PathValidationResult.NotFound;
        IsMemoriaDetected = result == PathValidationResult.ValidWithMemoria;
        GamePathValidationMessage = result switch
        {
            PathValidationResult.ValidWithMemoria => "✓ FFIX + Memoria Engine detected",
            PathValidationResult.ValidNoMemoria => "⚠ FFIX found — Memoria Engine not detected. Install it before generating.",
            _ => string.IsNullOrWhiteSpace(path) ? string.Empty : "✗ FF9_Launcher.exe not found at this path"
        };
    }

    private static string GenerateRandomSeedString() =>
        Random.Shared.Next(10_000_000, 99_999_999).ToString();

    private static int ParseInt(string s, int fallback) =>
        int.TryParse(s, out var v) ? v : fallback;

    private static float ParseFloat(string s, float fallback) =>
        float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : fallback;
}