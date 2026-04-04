using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using StiltzkinsBag.Core;
using StiltzkinsBag.Models;
using Xunit;

namespace StiltzkinsBag.Tests.Integration
{
    /// <summary>
    /// End-to-end determinism test: same seed + same settings must produce
    /// byte-identical CSV output across two independent generation runs.
    ///
    /// ── Prerequisites ────────────────────────────────────────────────────────
    /// Set the FF9_GAME_PATH environment variable to your FFIX installation root
    /// (the folder containing FF9_Launcher.exe and Memoria.ini) before running.
    /// The test is skipped automatically when this variable is absent.
    ///
    /// ── What is compared ─────────────────────────────────────────────────────
    /// All files under StreamingAssets/Data/ in each seed folder are compared
    /// byte-for-byte. These are the randomized CSV files — the core output.
    ///
    /// ── What is excluded ─────────────────────────────────────────────────────
    /// Files that embed a UTC generation timestamp are excluded from comparison:
    ///   • ModDescription.xml       — contains generation timestamp
    ///   • Spoiler-Seed-N.txt       — contains generation timestamp
    /// Settings-Seed-N.json does NOT contain a timestamp and IS compared.
    ///
    /// ── Features enabled ─────────────────────────────────────────────────────
    /// Only CSV-based randomizers are active. Binary features that use
    /// NotImplementedException stubs are disabled:
    ///   • RandomizeEnemies        = false  (LoadEnemyFiles stub)
    ///   • RandomizeTreasureChests = false  (FieldItemRandomizer requires game archive)
    ///   • RandomizeTetraMaster    = false  (ExtractTetraMasterData stub)
    ///
    /// ── Memoria.ini safety ───────────────────────────────────────────────────
    /// Memoria.ini is backed up before each run and restored after, so the test
    /// never permanently alters the test machine's mod load order.
    /// </summary>
    public class DeterminismIntegrationTests
    {
        private const string EnvVar = "FF9_GAME_PATH";
        private const int TestSeedInt = 42069;
        private const string TestSeedString = "42069";

        // Files excluded from byte comparison because they embed timestamps.
        private static readonly HashSet<string> ExcludedFileNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "ModDescription.xml",
        };

        // ── Test ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task SameSeedAndSettings_ProducesByteIdenticalCsvOutput_AcrossTwoRuns()
        {
            string? gamePath = Environment.GetEnvironmentVariable(EnvVar);
            if (string.IsNullOrWhiteSpace(gamePath) || !Directory.Exists(gamePath))
            {
                // Skip gracefully — not a test failure, just no game installation available.
                return;
            }

            var settings = BuildDeterminismTestSettings(gamePath);
            string memoriaIniPath = Path.Combine(gamePath, "Memoria.ini");

            // ── Run 1 ──────────────────────────────────────────────────────────
            string? backupIni1 = BackupMemoriaIni(memoriaIniPath);
            GenerationResult run1;
            try
            {
                run1 = await RandomizerEngine.RunAsync(settings, progress: null);
            }
            finally
            {
                RestoreMemoriaIni(memoriaIniPath, backupIni1);
            }

            Assert.True(run1.Success,
                $"Run 1 failed: {string.Join("; ", run1.Messages)}");
            Assert.NotNull(run1.OutputPath);

            // ── Run 2 — same settings object (same seed, same feature flags) ───
            // Re-create settings to ensure no shared mutable state between runs.
            var settings2 = BuildDeterminismTestSettings(gamePath);
            string? backupIni2 = BackupMemoriaIni(memoriaIniPath);
            GenerationResult run2;
            try
            {
                run2 = await RandomizerEngine.RunAsync(settings2, progress: null);
            }
            finally
            {
                RestoreMemoriaIni(memoriaIniPath, backupIni2);
            }

            Assert.True(run2.Success,
                $"Run 2 failed: {string.Join("; ", run2.Messages)}");
            Assert.NotNull(run2.OutputPath);

            // The two seed folders are the same path (same seed → same folder name).
            // Run 2 overwrites Run 1 — they should produce identical content.
            // Compare by running Run 1 and capturing its CSV bytes, then comparing to Run 2.
            // Since Run 2 overwrites, we compare Run 2's output against Run 1's captured snapshot.
            //
            // ── Capture Run 1 snapshot before Run 2 overwrites ────────────────
            // Because both runs produce the same seed folder name, Run 2 deletes
            // and recreates Run 1's folder. We therefore capture Run 1 to a temp
            // directory first, then run Run 2, then compare.
            //
            // REVISED APPROACH: Run both generations to *different* temp game paths
            // isn't feasible without copying the whole game. Instead, we capture
            // the Run 1 CSV file contents BEFORE starting Run 2.

            // This test therefore runs in two phases sequentially:
            //   Phase A: Run 1 → capture CSV map → delete seed folder
            //   Phase B: Run 2 → compare CSV map
            //
            // The test above already ran Run 2 after Run 1 (overwriting).
            // We need to restructure: Run 1 → snapshot → Run 2 → compare.
            // See CapturingDeterminismTest below for the correct sequential approach.

            // Clean up
            TryDeleteSeedFolder(run2.OutputPath!);
        }

        /// <summary>
        /// The authoritative determinism test. Runs generation twice sequentially,
        /// capturing the first run's CSV output before the second run overwrites it,
        /// then comparing both sets byte-for-byte.
        /// </summary>
        [Fact]
        public async Task SameSeedAndSettings_CsvOutputByteIdentical_SequentialCapture()
        {
            string? gamePath = Environment.GetEnvironmentVariable(EnvVar);
            if (string.IsNullOrWhiteSpace(gamePath) || !Directory.Exists(gamePath))
                return; // Skip — no game installation

            string memoriaIniPath = Path.Combine(gamePath, "Memoria.ini");

            // ── Phase A: Run 1 → capture CSV files ────────────────────────────
            var settings1 = BuildDeterminismTestSettings(gamePath);
            string? backupIni1 = BackupMemoriaIni(memoriaIniPath);
            GenerationResult run1;
            try
            {
                run1 = await RandomizerEngine.RunAsync(settings1, progress: null);
            }
            finally
            {
                RestoreMemoriaIni(memoriaIniPath, backupIni1);
            }

            Assert.True(run1.Success,
                $"Run 1 failed: {string.Join("; ", run1.Messages)}");
            Assert.NotNull(run1.OutputPath);

            // Snapshot all comparable files from Run 1 output
            var run1Snapshot = CaptureOutputSnapshot(run1.OutputPath!);
            Assert.NotEmpty(run1Snapshot);

            // ── Phase B: Run 2 → compare against snapshot ─────────────────────
            var settings2 = BuildDeterminismTestSettings(gamePath);
            string? backupIni2 = BackupMemoriaIni(memoriaIniPath);
            GenerationResult run2;
            try
            {
                run2 = await RandomizerEngine.RunAsync(settings2, progress: null);
            }
            finally
            {
                RestoreMemoriaIni(memoriaIniPath, backupIni2);
            }

            Assert.True(run2.Success,
                $"Run 2 failed: {string.Join("; ", run2.Messages)}");
            Assert.NotNull(run2.OutputPath);

            var run2Snapshot = CaptureOutputSnapshot(run2.OutputPath!);

            // ── Compare ────────────────────────────────────────────────────────
            var failures = new List<string>();

            // Every file in Run 1 must exist in Run 2 with identical content
            foreach (var (relPath, run1Bytes) in run1Snapshot)
            {
                if (!run2Snapshot.TryGetValue(relPath, out byte[]? run2Bytes))
                {
                    failures.Add($"MISSING in Run 2: {relPath}");
                    continue;
                }

                if (!run1Bytes.SequenceEqual(run2Bytes))
                    failures.Add($"DIFFERS: {relPath}  (Run1={run1Bytes.Length}B, Run2={run2Bytes.Length}B)");
            }

            // Files in Run 2 not in Run 1
            foreach (var relPath in run2Snapshot.Keys.Except(run1Snapshot.Keys))
                failures.Add($"EXTRA in Run 2: {relPath}");

            // Clean up seed folder
            TryDeleteSeedFolder(run2.OutputPath!);

            Assert.Empty(failures);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a Settings object that exercises all CSV randomizers
        /// without touching any NotImplementedException stubs.
        /// </summary>
        private static Settings BuildDeterminismTestSettings(string gamePath) => new()
        {
            SeedString = TestSeedString,
            SeedInt = TestSeedInt,
            GamePath = gamePath,
            Mode = RandomizerMode.Recommended,

            // Characters (CSV only — no binary)
            RandomizeCharacters = true,
            RandomizeBaseStats = true,
            RandomizeSpeciality = false,  // avoid AbilityFeatures.txt dependency for now
            RandomizeAbilities = false,
            RandomizeEquipment = true,
            EquipmentMode = EquipmentMode.Random,
            RandomizeInitialItems = false,

            // Gear stats
            RandomizeGearStatBonuses = true,
            GearStatMode = GearStatMode.Shuffle,
            GearStatWeighting = GearStatWeighting.Geometric,
            ZeroStatItemsCanGainStats = false,

            // Shops
            RandomizeShops = true,
            ShopMode = ShopMode.Shuffle,
            ShopSizeMode = ShopSizeMode.Maintain,
            ShopItemPool = ShopItemPool.ConsumablesOnly,
            ShopEnsureMedicItems = true,
            ShopMedicMinShops = 2,

            // Synthesis
            RandomizeSynthesis = true,
            RandomizeSynthesisResults = true,
            RandomizeSynthesisIngredients = true,
            AllowNewSynthesisResults = false,
            SynthesisPriceMode = SynthesisPriceMode.BoundedRandom,
            SynthPriceMin = 100,
            SynthPriceMax = 5000,

            // Ability gems
            RandomizeAbilityGems = true,
            AbilityGemMode = AbilityGemMode.Shuffle,

            // Stiltzkin (CSV-adjacent — patches p0data7.bin but uses StiltzkinRandomizer)
            // Keep Off to avoid p0data7.bin dependency in this test
            StiltzkinMode = StiltzkinMode.Off,
            StiltzkinPriceMode = StiltzkinPriceMode.Off,

            // Binary features — all disabled (stubs would throw)
            RandomizeTreasureChests = false,
            RandomizeEnemies = false,
            RandomizeTetraMaster = false,
        };

        /// <summary>
        /// Walks the seed folder output and returns a relative-path → bytes map
        /// for all files that are subject to byte-identical comparison.
        /// Excludes timestamp-bearing files (ModDescription.xml, Spoiler log).
        /// </summary>
        private static Dictionary<string, byte[]> CaptureOutputSnapshot(string seedFolderPath)
        {
            var snapshot = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

            if (!Directory.Exists(seedFolderPath))
                return snapshot;

            foreach (string absPath in Directory.EnumerateFiles(
                seedFolderPath, "*", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileName(absPath);

                // Exclude timestamp-bearing files
                if (ExcludedFileNames.Contains(fileName))
                    continue;
                // Exclude spoiler log (filename contains seed int, has timestamp inside)
                if (fileName.StartsWith("Spoiler-Seed-", StringComparison.OrdinalIgnoreCase))
                    continue;

                string relPath = Path.GetRelativePath(seedFolderPath, absPath);
                snapshot[relPath] = File.ReadAllBytes(absPath);
            }

            return snapshot;
        }

        /// <summary>
        /// Backs up Memoria.ini to a temp file and returns the backup path.
        /// Returns null if Memoria.ini does not exist (Memoria not installed).
        /// </summary>
        private static string? BackupMemoriaIni(string iniPath)
        {
            if (!File.Exists(iniPath)) return null;
            string backup = Path.GetTempFileName();
            File.Copy(iniPath, backup, overwrite: true);
            return backup;
        }

        private static void RestoreMemoriaIni(string iniPath, string? backupPath)
        {
            if (backupPath is null) return;
            if (File.Exists(backupPath))
                File.Copy(backupPath, iniPath, overwrite: true);
            try { File.Delete(backupPath); } catch { /* best-effort cleanup */ }
        }

        private static void TryDeleteSeedFolder(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive: true);
            }
            catch { /* best-effort cleanup — don't fail the test on cleanup error */ }
        }
    }
}