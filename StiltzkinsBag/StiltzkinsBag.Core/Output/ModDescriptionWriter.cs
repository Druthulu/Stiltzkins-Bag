using System;
using System.Globalization;
using System.IO;
using System.Xml;
using StiltzkinsBag.Models;

namespace StiltzkinsBag.Core.Output
{
    /// <summary>
    /// Writes the ModDescription.xml file to the root of the seed folder.
    /// Memoria Engine reads this file to display mod info in its launcher UI.
    ///
    /// Output: [SeedFolderPath]/ModDescription.xml
    ///
    /// Example output:
    ///   &lt;ModDescription&gt;
    ///     &lt;Author&gt;Stiltzkin's Bag&lt;/Author&gt;
    ///     &lt;Version&gt;1.0&lt;/Version&gt;
    ///     &lt;Description&gt;Randomized seed 42069 (Recommended mode) — generated 2026-04-03 14:32:07 UTC&lt;/Description&gt;
    ///   &lt;/ModDescription&gt;
    /// </summary>
    public static class ModDescriptionWriter
    {
        private const string FileName = "ModDescription.xml";
        private const string AuthorName = "Stiltzkin's Bag";
        private const string ModVersion = "1.0";

        /// <summary>
        /// Writes ModDescription.xml to the seed folder root.
        /// </summary>
        /// <param name="seedFolderPath">
        /// Absolute path to the root of the seed folder
        /// (i.e. <see cref="ModOutputWriter.SeedFolderPath"/>).
        /// </param>
        /// <param name="seedInt">Deterministic seed integer produced by SeedEngine.</param>
        /// <param name="settings">The settings used for this generation run.</param>
        public static void Write(string seedFolderPath, int seedInt, Settings settings)
        {
            string outputPath = Path.Combine(seedFolderPath, FileName);
            string description = BuildDescription(seedInt, settings);

            var xmlSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = System.Text.Encoding.UTF8,
                OmitXmlDeclaration = false,
            };

            using var writer = XmlWriter.Create(outputPath, xmlSettings);

            writer.WriteStartDocument();
            writer.WriteStartElement("ModDescription");

            writer.WriteElementString("Author", AuthorName);
            writer.WriteElementString("Version", ModVersion);
            writer.WriteElementString("Description", description);

            writer.WriteEndElement(); // </ModDescription>
            writer.WriteEndDocument();
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private static string BuildDescription(int seedInt, Settings settings)
        {
            string mode = settings.Mode == RandomizerMode.Recommended
                ? "Recommended mode"
                : "Chaos mode";

            string timestamp = DateTime.UtcNow.ToString(
                "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            return $"Randomized seed {seedInt} ({mode}) — generated {timestamp} UTC";
        }
    }
}