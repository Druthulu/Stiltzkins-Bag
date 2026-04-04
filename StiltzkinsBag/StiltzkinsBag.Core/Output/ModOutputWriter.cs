using System;
using System.IO;
using StiltzkinsBag.Parsing;

namespace StiltzkinsBag.Core.Output
{
    /// <summary>
    /// Creates and populates the seed mod folder that Memoria Engine loads as an overlay.
    ///
    /// Folder structure produced:
    ///   [gamePath]/Mods/StiltzkinsBag-Seed-[seedInt]/
    ///     StreamingAssets/
    ///       Data/
    ///         Battle/              ← patched enemy + field binary files
    ///         Characters/          ← BaseStats, CharacterParameters, CommandSets, DefaultEquipment
    ///           Abilities/         ← AbilityGems + all per-character ability CSVs
    ///         Items/               ← Items, ShopItems, Stats, Synthesis, Weapons, Armors
    ///
    /// Usage:
    ///   var writer = new ModOutputWriter(gamePath, seedInt);
    ///   writer.EnsureCleanSeedFolder();
    ///   writer.WriteModifiedCsv&lt;ZidaneRow, ZidaneRowMap&gt;("Characters/Abilities/Zidane.csv", parsedCsv);
    ///   writer.WritePatchedBinaryFile("Battle/evt_battle_ac_e001f/dbfile0000.raw16.bytes", data);
    /// </summary>
    public class ModOutputWriter
    {
        // ── Subfolder constants ──────────────────────────────────────────────────

        /// <summary>Relative path from SeedFolderPath to the StreamingAssets root.</summary>
        public const string StreamingAssetsRelPath = "StreamingAssets";

        /// <summary>Relative path from SeedFolderPath to the Data root.</summary>
        public const string DataRelPath = "StreamingAssets/Data";

        /// <summary>
        /// Relative path for top-level character CSVs:
        /// BaseStats, CharacterParameters, CommandSets, DefaultEquipment, AbilityFeatures.txt.
        /// </summary>
        public const string CharactersRelPath = "StreamingAssets/Data/Characters";

        /// <summary>
        /// Relative path for per-character ability CSVs (Zidane.csv, Vivi.csv, etc.)
        /// and AbilityGems.csv. These live in the Abilities subfolder in the game.
        /// </summary>
        public const string AbilitiesRelPath = "StreamingAssets/Data/Characters/Abilities";

        /// <summary>Relative path for item CSVs: Items, ShopItems, Stats, Synthesis, Weapons, Armors.</summary>
        public const string ItemsRelPath = "StreamingAssets/Data/Items";

        /// <summary>Relative path for battle binary files.</summary>
        public const string BattleRelPath = "StreamingAssets/Data/Battle";

        // ── Mod folder naming ────────────────────────────────────────────────────

        /// <summary>Prefix for all Stiltzkin's Bag seed folder names.</summary>
        public const string FolderPrefix = "StiltzkinsBag-Seed-";

        /// <summary>Constructs the seed folder name for a given seed integer.</summary>
        public static string SeedFolderName(int seedInt) => $"{FolderPrefix}{seedInt}";

        // ── Instance state ───────────────────────────────────────────────────────

        private readonly string _gamePath;
        private readonly int _seedInt;

        /// <summary>
        /// Absolute path to the root of the generated seed folder.
        /// e.g. C:\SteamLibrary\...\FINAL FANTASY IX\Mods\StiltzkinsBag-Seed-42069\
        /// </summary>
        public string SeedFolderPath { get; }

        /// <param name="gamePath">
        /// Root of the FF9 Steam installation (the folder containing FF9_Launcher.exe).
        /// </param>
        /// <param name="seedInt">Deterministic seed integer produced by SeedEngine.</param>
        public ModOutputWriter(string gamePath, int seedInt)
        {
            if (string.IsNullOrWhiteSpace(gamePath))
                throw new ArgumentException("Game path must not be empty.", nameof(gamePath));

            _gamePath = gamePath;
            _seedInt = seedInt;
            SeedFolderPath = Path.Combine(gamePath, "Mods", SeedFolderName(seedInt));
        }

        // ── Folder management ────────────────────────────────────────────────────

        /// <summary>
        /// Creates the full seed folder tree, deleting any existing folder at the same path
        /// first so re-generates are always clean.
        ///
        /// Call this once at the start of a generation run before writing any files.
        /// </summary>
        public void EnsureCleanSeedFolder()
        {
            if (Directory.Exists(SeedFolderPath))
                Directory.Delete(SeedFolderPath, recursive: true);

            Directory.CreateDirectory(SeedFolderPath);
            Directory.CreateDirectory(GetAbsolutePath(AbilitiesRelPath)); // also creates Characters/
            Directory.CreateDirectory(GetAbsolutePath(ItemsRelPath));
            Directory.CreateDirectory(GetAbsolutePath(BattleRelPath));
        }

        // ── CSV output ───────────────────────────────────────────────────────────

        /// <summary>
        /// Writes a randomized CSV to the correct location inside the seed folder.
        /// The caller passes a <see cref="ParsedCsv{T}"/> whose Rows have already been
        /// modified by a randomizer. Comment headers and inline comments are preserved
        /// automatically because they live on the ParsedCsv object.
        /// </summary>
        /// <typeparam name="T">Typed CSV row model.</typeparam>
        /// <typeparam name="TMap">CsvHelper ClassMap for T.</typeparam>
        /// <param name="relativeSubPath">
        /// Path relative to SeedFolderPath, e.g.
        /// "StreamingAssets/Data/Characters/Abilities/Zidane.csv".
        /// Use the subfolder constants above as prefixes.
        /// </param>
        /// <param name="parsed">
        /// ParsedCsv produced by MemoriaCsvParser.Read() with rows already updated by a randomizer.
        /// </param>
        public void WriteModifiedCsv<T, TMap>(string relativeSubPath, ParsedCsv<T> parsed)
            where TMap : CsvHelper.Configuration.ClassMap<T>
        {
            ValidateRelativePath(relativeSubPath);

            string outputPath = GetAbsolutePath(relativeSubPath);
            EnsureDirectoryExists(outputPath);

            MemoriaCsvParser.Write<T, TMap>(parsed, outputPath);
        }

        // ── Binary output ────────────────────────────────────────────────────────

        /// <summary>
        /// Writes a patched binary file to the correct location inside the seed folder.
        /// Typically used for enemy bytecode files and field script files extracted from
        /// p0data2.bin / p0data7.bin by UnityArchiver.
        /// </summary>
        /// <param name="relativeSubPath">
        /// Path relative to SeedFolderPath, e.g.
        /// "StreamingAssets/assets/.../dbfile0000.raw16.bytes".
        /// </param>
        /// <param name="data">Patched file bytes to write.</param>
        public void WritePatchedBinaryFile(string relativeSubPath, byte[] data)
        {
            ValidateRelativePath(relativeSubPath);

            string outputPath = GetAbsolutePath(relativeSubPath);
            EnsureDirectoryExists(outputPath);

            File.WriteAllBytes(outputPath, data);
        }

        // ── Path helpers ─────────────────────────────────────────────────────────

        /// <summary>
        /// Resolves a relative sub-path to an absolute path within the seed folder.
        /// e.g. "StreamingAssets/Data/Characters/Abilities/Zidane.csv"
        ///   → "[SeedFolderPath]/StreamingAssets/Data/Characters/Abilities/Zidane.csv"
        /// </summary>
        public string GetAbsolutePath(string relativeSubPath)
        {
            string normalized = relativeSubPath.Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(SeedFolderPath, normalized);
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private static void EnsureDirectoryExists(string filePath)
        {
            string? dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }

        private static void ValidateRelativePath(string relativeSubPath)
        {
            if (string.IsNullOrWhiteSpace(relativeSubPath))
                throw new ArgumentException("Relative sub-path must not be empty.", nameof(relativeSubPath));

            if (Path.IsPathRooted(relativeSubPath))
                throw new ArgumentException(
                    "Expected a relative path (no drive letter or leading slash). " +
                    $"Got: {relativeSubPath}", nameof(relativeSubPath));
        }
    }
}