using System.IO;
using System.Text;
using StiltzkinsBag.Models;

namespace StiltzkinsBag.Core.Output
{
    /// <summary>
    /// Writes a mod-local Memoria.ini override file to the root of the seed folder.
    ///
    /// Memoria Engine merges a mod folder's own Memoria.ini on top of the base ini,
    /// allowing mods to force specific settings. This file contains only the keys
    /// Stiltzkin's Bag needs to override — it is NOT a copy of the base Memoria.ini.
    ///
    /// Output: [SeedFolderPath]/Memoria.ini
    ///
    /// Current overrides:
    ///   [TetraMaster]
    ///     TripleTriad = 0   — forced when card randomization is active.
    ///                         Prevents Memoria from switching to TripleTriad.csv mode,
    ///                         which would bypass our bytecode-patched Tetramaster cards.
    ///                         (TripleTriad.csv is not supported by Stiltzkin's Bag.)
    ///
    /// No file is written if there are no active overrides for the given settings.
    /// </summary>
    public static class ModMemoriaIniWriter
    {
        private const string FileName = "Memoria.ini";

        /// <summary>
        /// Writes the mod-local Memoria.ini to the seed folder root, if any overrides
        /// are required by the active settings.
        /// </summary>
        /// <param name="seedFolderPath">
        /// Absolute path to the root of the seed folder
        /// (i.e. <see cref="ModOutputWriter.SeedFolderPath"/>).
        /// </param>
        /// <param name="settings">The settings used for this generation run.</param>
        /// <returns>
        /// True if a Memoria.ini file was written; false if no overrides were needed
        /// and no file was created.
        /// </returns>
        public static bool Write(string seedFolderPath, Settings settings)
        {
            bool needsTetraMasterSection = settings.RandomizeTetraMaster;

            // No overrides required — do not create the file.
            if (!needsTetraMasterSection)
                return false;

            var sb = new StringBuilder();

            if (needsTetraMasterSection)
            {
                sb.AppendLine("[TetraMaster]");

                // Force TripleTriad = 0 so Memoria does not switch to the FF8-style
                // TripleTriad.csv card game. Stiltzkin's Bag randomizes Tetramaster
                // exclusively via bytecode edits; TripleTriad.csv is not supported.
                sb.AppendLine("\tTripleTriad = 0");
            }

            string outputPath = Path.Combine(seedFolderPath, FileName);
            File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);

            return true;
        }
    }
}