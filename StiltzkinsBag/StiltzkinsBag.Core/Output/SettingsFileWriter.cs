using System.IO;
using System.Text;
using StiltzkinsBag.Models;

namespace StiltzkinsBag.Core.Output
{
    /// <summary>
    /// Writes the Settings-Seed-[seedInt].json file to the root of the seed folder.
    ///
    /// This file records the exact settings used to produce the randomization run,
    /// enabling full reproduction of any seed. It is human-readable and includes
    /// all Settings properties serialized as formatted JSON with string enum names.
    ///
    /// Output: [SeedFolderPath]/Settings-Seed-[seedInt].json
    ///
    /// Serialization is delegated to <see cref="Settings.ToJson()"/>, which uses
    /// System.Text.Json with WriteIndented=true and JsonStringEnumConverter so
    /// enum values appear as readable names rather than integers.
    /// </summary>
    public static class SettingsFileWriter
    {
        /// <summary>
        /// Writes the settings JSON file to the seed folder root.
        /// </summary>
        /// <param name="seedFolderPath">
        /// Absolute path to the root of the seed folder
        /// (i.e. <see cref="ModOutputWriter.SeedFolderPath"/>).
        /// </param>
        /// <param name="seedInt">Deterministic seed integer produced by SeedEngine.</param>
        /// <param name="settings">The settings used for this generation run.</param>
        public static void Write(string seedFolderPath, int seedInt, Settings settings)
        {
            string fileName = $"Settings-Seed-{seedInt}.json";
            string outputPath = Path.Combine(seedFolderPath, fileName);

            string json = settings.ToJson();
            File.WriteAllText(outputPath, json, Encoding.UTF8);
        }
    }
}