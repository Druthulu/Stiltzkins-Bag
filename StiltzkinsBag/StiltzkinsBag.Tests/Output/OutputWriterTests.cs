using System;
using System.IO;
using System.Text;
using System.Xml;
using StiltzkinsBag.Core.Output;
using StiltzkinsBag.Models;
using Xunit;

namespace StiltzkinsBag.Tests.Output
{
    // =========================================================================
    // Shared temp-folder fixture
    // =========================================================================

    /// <summary>
    /// Creates a unique temp directory for a test class and deletes it on dispose.
    /// </summary>
    public sealed class TempDirectory : IDisposable
    {
        public string Path { get; }

        public TempDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "SBTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }

    // =========================================================================
    // ModOutputWriter tests
    // =========================================================================

    public class ModOutputWriterTests : IDisposable
    {
        private readonly TempDirectory _tmp = new();
        public void Dispose() => _tmp.Dispose();

        [Fact]
        public void EnsureCleanSeedFolder_CreatesExpectedSubfolders()
        {
            var writer = new ModOutputWriter(_tmp.Path, 42069);
            writer.EnsureCleanSeedFolder();

            Assert.True(Directory.Exists(writer.GetAbsolutePath(ModOutputWriter.CharactersRelPath)));
            Assert.True(Directory.Exists(writer.GetAbsolutePath(ModOutputWriter.ItemsRelPath)));
            Assert.True(Directory.Exists(writer.GetAbsolutePath(ModOutputWriter.BattleRelPath)));
        }

        [Fact]
        public void EnsureCleanSeedFolder_DeletesExistingFolderFirst()
        {
            var writer = new ModOutputWriter(_tmp.Path, 42069);
            writer.EnsureCleanSeedFolder();

            // Plant a sentinel file inside the seed folder
            string sentinel = System.IO.Path.Combine(writer.SeedFolderPath, "old_file.txt");
            File.WriteAllText(sentinel, "stale");

            // Re-generate — sentinel must be gone
            writer.EnsureCleanSeedFolder();

            Assert.False(File.Exists(sentinel));
            Assert.True(Directory.Exists(writer.SeedFolderPath));
        }

        [Fact]
        public void SeedFolderPath_ContainsPrefixAndSeedInt()
        {
            var writer = new ModOutputWriter(_tmp.Path, 99999);
            Assert.Contains("StiltzkinsBag-Seed-99999", writer.SeedFolderPath);
        }

        [Fact]
        public void WritePatchedBinaryFile_WritesCorrectBytes()
        {
            var writer = new ModOutputWriter(_tmp.Path, 1);
            writer.EnsureCleanSeedFolder();

            byte[] data = { 0xDE, 0xAD, 0xBE, 0xEF };
            string relPath = "StreamingAssets/Data/Battle/testfile.bytes";
            writer.WritePatchedBinaryFile(relPath, data);

            byte[] readBack = File.ReadAllBytes(writer.GetAbsolutePath(relPath));
            Assert.Equal(data, readBack);
        }

        [Fact]
        public void ValidateRelativePath_RejectsRootedPath()
        {
            var writer = new ModOutputWriter(_tmp.Path, 1);
            writer.EnsureCleanSeedFolder();

            Assert.Throws<ArgumentException>(() =>
                writer.WritePatchedBinaryFile(@"C:\absolute\path.bytes", new byte[1]));
        }
    }

    // =========================================================================
    // MemoriaLoadOrder tests
    // =========================================================================

    public class MemoriaLoadOrderTests : IDisposable
    {
        private readonly TempDirectory _tmp = new();
        public void Dispose() => _tmp.Dispose();

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// Writes a minimal mock Memoria.ini to a temp file and returns its path.
        /// The FolderNames line mirrors the real file format (leading tab, quoted names).
        /// </summary>
        private string WriteMockIni(string folderNamesValue, string extraLines = "")
        {
            string path = System.IO.Path.Combine(_tmp.Path, "Memoria.ini");
            var content = new StringBuilder();
            content.AppendLine("[Mod]");
            content.AppendLine($"\tFolderNames = {folderNamesValue}");
            content.AppendLine("\tPriorities = \"SomeMod\"");
            content.AppendLine("UseFileList = 1");
            if (!string.IsNullOrEmpty(extraLines))
                content.AppendLine(extraLines);
            File.WriteAllText(path, content.ToString(), Encoding.UTF8);
            return path;
        }

        // ── ParseFolderNames ─────────────────────────────────────────────────

        [Fact]
        public void ParseFolderNames_ExtractsAllNames()
        {
            string line = "\tFolderNames = \"AlternateFantasy\", \"PlaystationSounds\", \"MogAddons\"";
            var names = MemoriaLoadOrder.ParseFolderNames(line);

            Assert.Equal(3, names.Count);
            Assert.Equal("AlternateFantasy", names[0]);
            Assert.Equal("PlaystationSounds", names[1]);
            Assert.Equal("MogAddons", names[2]);
        }

        [Fact]
        public void ParseFolderNames_HandlesSubpathEntries()
        {
            string line = "\tFolderNames = \"MogAddons/Features/Maps\", \"MoguriMain\"";
            var names = MemoriaLoadOrder.ParseFolderNames(line);

            Assert.Equal(2, names.Count);
            Assert.Equal("MogAddons/Features/Maps", names[0]);
            Assert.Equal("MoguriMain", names[1]);
        }

        [Fact]
        public void ParseFolderNames_ReturnsEmptyListForNoValues()
        {
            string line = "\tFolderNames = ";
            var names = MemoriaLoadOrder.ParseFolderNames(line);
            Assert.Empty(names);
        }

        // ── BuildFolderNamesLine ──────────────────────────────────────────────

        [Fact]
        public void BuildFolderNamesLine_ProducesCorrectFormat()
        {
            var names = new[] { "AlternateFantasy", "PlaystationSounds", "MogAddons" };
            string line = MemoriaLoadOrder.BuildFolderNamesLine(names);

            Assert.Contains("\"AlternateFantasy\"", line);
            Assert.Contains("\"PlaystationSounds\"", line);
            Assert.Contains("\"MogAddons\"", line);
            Assert.StartsWith("\tFolderNames =", line);
        }

        // ── InjectSeedFolder ─────────────────────────────────────────────────

        [Fact]
        public void InjectSeedFolder_PrependsSeedFolderAsFirst()
        {
            string iniPath = WriteMockIni("\"AlternateFantasy\", \"PlaystationSounds\"");
            MemoriaLoadOrder.InjectSeedFolder(iniPath, 42069);

            string updated = File.ReadAllText(iniPath);
            int sbIdx = updated.IndexOf("StiltzkinsBag-Seed-42069");
            int altIdx = updated.IndexOf("AlternateFantasy");

            Assert.True(sbIdx >= 0, "Seed folder entry not found.");
            Assert.True(sbIdx < altIdx, "Seed folder must appear before AlternateFantasy.");
        }

        [Fact]
        public void InjectSeedFolder_RemovesExistingSbEntries()
        {
            string iniPath = WriteMockIni(
                "\"StiltzkinsBag-Seed-11111\", \"StiltzkinsBag-Seed-22222\", \"AlternateFantasy\"");

            MemoriaLoadOrder.InjectSeedFolder(iniPath, 33333);

            string updated = File.ReadAllText(iniPath);
            Assert.DoesNotContain("StiltzkinsBag-Seed-11111", updated);
            Assert.DoesNotContain("StiltzkinsBag-Seed-22222", updated);
            Assert.Contains("StiltzkinsBag-Seed-33333", updated);
            Assert.Contains("AlternateFantasy", updated);
        }

        [Fact]
        public void InjectSeedFolder_IsIdempotent()
        {
            string iniPath = WriteMockIni("\"AlternateFantasy\"");
            MemoriaLoadOrder.InjectSeedFolder(iniPath, 42069);
            MemoriaLoadOrder.InjectSeedFolder(iniPath, 42069);

            string updated = File.ReadAllText(iniPath);
            int count = 0;
            int idx = 0;
            while ((idx = updated.IndexOf("StiltzkinsBag-Seed-42069", idx)) >= 0)
            {
                count++;
                idx++;
            }
            Assert.Equal(1, count);
        }

        [Fact]
        public void InjectSeedFolder_PreservesAllOtherLines()
        {
            string iniPath = WriteMockIni("\"AlternateFantasy\"");
            MemoriaLoadOrder.InjectSeedFolder(iniPath, 1);

            string updated = File.ReadAllText(iniPath);
            Assert.Contains("[Mod]", updated);
            Assert.Contains("Priorities", updated);
            Assert.Contains("UseFileList = 1", updated);
        }

        // ── RemoveAllSbEntries ────────────────────────────────────────────────

        [Fact]
        public void RemoveAllSbEntries_RemovesAllSbAndPreservesOthers()
        {
            string iniPath = WriteMockIni(
                "\"StiltzkinsBag-Seed-1\", \"AlternateFantasy\", \"StiltzkinsBag-Seed-2\"");

            MemoriaLoadOrder.RemoveAllSbEntries(iniPath);

            string updated = File.ReadAllText(iniPath);
            Assert.DoesNotContain("StiltzkinsBag-Seed-1", updated);
            Assert.DoesNotContain("StiltzkinsBag-Seed-2", updated);
            Assert.Contains("AlternateFantasy", updated);
        }

        [Fact]
        public void RemoveAllSbEntries_NoOpWhenNoSbEntriesPresent()
        {
            string iniPath = WriteMockIni("\"AlternateFantasy\", \"PlaystationSounds\"");
            string before = File.ReadAllText(iniPath);
            MemoriaLoadOrder.RemoveAllSbEntries(iniPath);
            string after = File.ReadAllText(iniPath);

            // FolderNames content unchanged
            Assert.Contains("AlternateFantasy", after);
            Assert.Contains("PlaystationSounds", after);
        }

        // ── Error cases ───────────────────────────────────────────────────────

        [Fact]
        public void InjectSeedFolder_ThrowsFileNotFoundWhenIniMissing()
        {
            string missingPath = System.IO.Path.Combine(_tmp.Path, "does_not_exist.ini");
            Assert.Throws<FileNotFoundException>(() =>
                MemoriaLoadOrder.InjectSeedFolder(missingPath, 1));
        }
    }

    // =========================================================================
    // ModDescriptionWriter tests
    // =========================================================================

    public class ModDescriptionWriterTests : IDisposable
    {
        private readonly TempDirectory _tmp = new();
        public void Dispose() => _tmp.Dispose();

        private Settings MakeSettings(RandomizerMode mode) =>
            new Settings { Mode = mode, SeedString = "test" };

        [Fact]
        public void Write_CreatesModDescriptionXml()
        {
            ModDescriptionWriter.Write(_tmp.Path, 42069, MakeSettings(RandomizerMode.Recommended));

            string expectedPath = System.IO.Path.Combine(_tmp.Path, "ModDescription.xml");
            Assert.True(File.Exists(expectedPath));
        }

        [Fact]
        public void Write_XmlContainsAuthorAndVersion()
        {
            ModDescriptionWriter.Write(_tmp.Path, 1, MakeSettings(RandomizerMode.Chaos));

            var doc = new XmlDocument();
            doc.Load(System.IO.Path.Combine(_tmp.Path, "ModDescription.xml"));

            Assert.Equal("Stiltzkin's Bag", doc.SelectSingleNode("//Author")?.InnerText);
            Assert.Equal("1.0", doc.SelectSingleNode("//Version")?.InnerText);
        }

        [Fact]
        public void Write_DescriptionContainsSeedAndMode()
        {
            ModDescriptionWriter.Write(_tmp.Path, 99999, MakeSettings(RandomizerMode.Recommended));

            var doc = new XmlDocument();
            doc.Load(System.IO.Path.Combine(_tmp.Path, "ModDescription.xml"));
            string? desc = doc.SelectSingleNode("//Description")?.InnerText;

            Assert.NotNull(desc);
            Assert.Contains("99999", desc);
            Assert.Contains("Recommended mode", desc);
        }
    }

    // =========================================================================
    // ModMemoriaIniWriter tests
    // =========================================================================

    public class ModMemoriaIniWriterTests : IDisposable
    {
        private readonly TempDirectory _tmp = new();
        public void Dispose() => _tmp.Dispose();

        [Fact]
        public void Write_ReturnsFalseAndNoFileWhenTetraMasterOff()
        {
            var settings = new Settings { RandomizeTetraMaster = false };
            bool result = ModMemoriaIniWriter.Write(_tmp.Path, settings);

            Assert.False(result);
            Assert.False(File.Exists(System.IO.Path.Combine(_tmp.Path, "Memoria.ini")));
        }

        [Fact]
        public void Write_ReturnsTrueAndCreatesFileWhenTetraMasterOn()
        {
            var settings = new Settings { RandomizeTetraMaster = true };
            bool result = ModMemoriaIniWriter.Write(_tmp.Path, settings);

            Assert.True(result);
            Assert.True(File.Exists(System.IO.Path.Combine(_tmp.Path, "Memoria.ini")));
        }

        [Fact]
        public void Write_FileContainsTetraMasterSection()
        {
            var settings = new Settings { RandomizeTetraMaster = true };
            ModMemoriaIniWriter.Write(_tmp.Path, settings);

            string content = File.ReadAllText(System.IO.Path.Combine(_tmp.Path, "Memoria.ini"));
            Assert.Contains("[TetraMaster]", content);
        }

        [Fact]
        public void Write_FileContainsTripleTriadZero()
        {
            var settings = new Settings { RandomizeTetraMaster = true };
            ModMemoriaIniWriter.Write(_tmp.Path, settings);

            string content = File.ReadAllText(System.IO.Path.Combine(_tmp.Path, "Memoria.ini"));
            Assert.Contains("TripleTriad = 0", content);
        }
    }

    // =========================================================================
    // SettingsFileWriter tests
    // =========================================================================

    public class SettingsFileWriterTests : IDisposable
    {
        private readonly TempDirectory _tmp = new();
        public void Dispose() => _tmp.Dispose();

        [Fact]
        public void Write_CreatesFileWithCorrectName()
        {
            var settings = new Settings { SeedString = "test", Mode = RandomizerMode.Recommended };
            SettingsFileWriter.Write(_tmp.Path, 42069, settings);

            string expectedPath = System.IO.Path.Combine(_tmp.Path, "Settings-Seed-42069.json");
            Assert.True(File.Exists(expectedPath));
        }

        [Fact]
        public void Write_ContentIsValidJson()
        {
            var settings = new Settings { SeedString = "hello", Mode = RandomizerMode.Chaos };
            SettingsFileWriter.Write(_tmp.Path, 1, settings);

            string json = File.ReadAllText(System.IO.Path.Combine(_tmp.Path, "Settings-Seed-1.json"));

            // Must not throw — valid JSON
            var deserialized = Settings.FromJson(json);
            Assert.NotNull(deserialized);
        }

        [Fact]
        public void Write_RoundTripsSettingsMode()
        {
            var settings = new Settings { SeedString = "race", Mode = RandomizerMode.Chaos };
            SettingsFileWriter.Write(_tmp.Path, 7, settings);

            string json = File.ReadAllText(System.IO.Path.Combine(_tmp.Path, "Settings-Seed-7.json"));
            var deserialized = Settings.FromJson(json);

            Assert.Equal(RandomizerMode.Chaos, deserialized!.Mode);
        }
    }
}