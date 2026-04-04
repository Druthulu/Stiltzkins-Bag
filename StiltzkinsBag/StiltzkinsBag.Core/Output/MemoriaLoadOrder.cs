using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StiltzkinsBag.Core.Output
{
    /// <summary>
    /// Manages the FolderNames entry in the base Memoria.ini file.
    ///
    /// Memoria loads mod folders in the order they appear in FolderNames — first entry
    /// has highest priority. Stiltzkin's Bag must always be prepended so its files
    /// take precedence over all other active mods.
    ///
    /// FolderNames format (single line, comma-separated quoted names):
    ///   FolderNames = "AlternateFantasy", "PlaystationSounds", "MogAddons"
    ///
    /// All non-FolderNames lines are preserved byte-identically. Only the FolderNames
    /// line is modified; all other section data, comments, and settings are untouched.
    ///
    /// Usage:
    ///   // On generate:
    ///   MemoriaLoadOrder.InjectSeedFolder(memoriaIniPath, seedInt);
    ///
    ///   // On "Remove from Load Order":
    ///   MemoriaLoadOrder.RemoveAllSbEntries(memoriaIniPath);
    /// </summary>
    public static class MemoriaLoadOrder
    {
        /// <summary>Prefix that identifies all Stiltzkin's Bag seed folder entries.</summary>
        public const string SbPrefix = ModOutputWriter.FolderPrefix; // "StiltzkinsBag-Seed-"

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Prepends the new seed folder to FolderNames in the base Memoria.ini, removing
        /// any existing Stiltzkin's Bag entries first. The new seed folder will load with
        /// highest priority (first in list).
        ///
        /// Idempotent: calling twice with the same seedInt produces the same result.
        /// </summary>
        /// <param name="iniPath">Absolute path to the base Memoria.ini file.</param>
        /// <param name="seedInt">The seed integer whose folder should be injected.</param>
        /// <exception cref="FileNotFoundException">Thrown if iniPath does not exist.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the file contains no FolderNames line (unexpected Memoria.ini format).
        /// </exception>
        public static void InjectSeedFolder(string iniPath, int seedInt)
        {
            string newFolderName = ModOutputWriter.SeedFolderName(seedInt);
            UpdateFolderNames(iniPath, existing =>
            {
                var withoutSb = existing.Where(name => !IsSbEntry(name)).ToList();
                withoutSb.Insert(0, newFolderName);
                return withoutSb;
            });
        }

        /// <summary>
        /// Removes all Stiltzkin's Bag seed folder entries from FolderNames in the base
        /// Memoria.ini without adding a new one. Used by "Remove from Load Order".
        ///
        /// No-op if no SB entries are present.
        /// </summary>
        /// <param name="iniPath">Absolute path to the base Memoria.ini file.</param>
        public static void RemoveAllSbEntries(string iniPath)
        {
            UpdateFolderNames(iniPath, existing =>
                existing.Where(name => !IsSbEntry(name)).ToList());
        }

        // ── Parsing helpers (public for unit tests) ──────────────────────────────

        /// <summary>
        /// Parses the folder names from a raw FolderNames line.
        /// Handles the format: FolderNames = "A", "B/sub", "C"
        /// Returns only the name values (without surrounding quotes).
        /// Returns an empty list if the line has no quoted values.
        /// </summary>
        public static List<string> ParseFolderNames(string folderNamesLine)
        {
            // Everything after the '=' sign is the value
            int eqIdx = folderNamesLine.IndexOf('=');
            if (eqIdx < 0)
                return new List<string>();

            string valueSection = folderNamesLine[(eqIdx + 1)..];

            var names = new List<string>();

            // Split on ',' then strip surrounding whitespace and quotes from each token
            foreach (string token in valueSection.Split(','))
            {
                string trimmed = token.Trim();
                if (trimmed.StartsWith('"') && trimmed.EndsWith('"') && trimmed.Length >= 2)
                    names.Add(trimmed[1..^1]);
            }

            return names;
        }

        /// <summary>
        /// Reconstructs a FolderNames line from an ordered list of folder name values.
        /// Output format: FolderNames = "A", "B/sub", "C"
        /// Preserves the leading tab that Memoria uses before the key.
        /// </summary>
        public static string BuildFolderNamesLine(IEnumerable<string> names)
        {
            // Memoria.ini uses a tab before FolderNames (inside the [Mod] section)
            string value = string.Join(", ", names.Select(n => $"\"{n}\""));
            return $"\tFolderNames = {value}";
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        /// <summary>
        /// Reads the ini file, applies the transform to the parsed folder name list,
        /// reconstructs the FolderNames line, and writes the file back.
        /// All other lines are passed through verbatim.
        /// </summary>
        private static void UpdateFolderNames(
            string iniPath,
            Func<List<string>, List<string>> transform)
        {
            if (!File.Exists(iniPath))
                throw new FileNotFoundException("Memoria.ini not found.", iniPath);

            // Read preserving line endings — detect from file
            string lineEnding = DetectLineEnding(iniPath);
            string[] lines = File.ReadAllLines(iniPath, Encoding.UTF8);

            bool found = false;
            var outputLines = new List<string>(lines.Length);

            foreach (string line in lines)
            {
                // Match the FolderNames line — handles leading whitespace/tab
                if (line.TrimStart().StartsWith("FolderNames", StringComparison.Ordinal)
                    && line.Contains('='))
                {
                    found = true;
                    var current = ParseFolderNames(line);
                    var updated = transform(current);
                    outputLines.Add(BuildFolderNamesLine(updated));
                }
                else
                {
                    outputLines.Add(line);
                }
            }

            if (!found)
                throw new InvalidOperationException(
                    $"No FolderNames line found in Memoria.ini at: {iniPath}");

            // Write back with original line ending
            string content = string.Join(lineEnding, outputLines) + lineEnding;
            File.WriteAllText(iniPath, content, Encoding.UTF8);
        }

        private static bool IsSbEntry(string name) =>
            name.StartsWith(SbPrefix, StringComparison.OrdinalIgnoreCase);

        private static string DetectLineEnding(string filePath)
        {
            byte[] raw = File.ReadAllBytes(filePath);
            for (int i = 0; i < raw.Length - 1; i++)
            {
                if (raw[i] == 0x0D && raw[i + 1] == 0x0A) return "\r\n";
                if (raw[i] == 0x0A) return "\n";
            }
            return "\n";
        }
    }
}