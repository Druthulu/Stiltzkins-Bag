using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StiltzkinsBag.Core.Parsing;
using Xunit;

namespace StiltzkinsBag.Tests.Parsing
{
    /// <summary>
    /// Phase 9.2 milestone tests: FieldScript.Decode → Encode must produce
    /// byte-identical output for every FF9 field script binary fixture.
    ///
    /// Two layers:
    ///   1. Named [Theory] cases — one xunit result per FieldParser fixture.
    ///      These give immediate, per-file visibility in the test explorer.
    ///
    ///   2. Comprehensive [Fact] sweep — iterates every *.eb.bytes file found
    ///      recursively under TestData/, collects all failures, and asserts
    ///      zero failures with a full diagnostic report.
    ///      Files that fail to decode are counted separately (not silently skipped)
    ///      so any new opcode coverage gaps are visible.
    /// </summary>
    public sealed class FieldScriptRoundTripTests
    {
        // ── Paths ──────────────────────────────────────────────────────────────

        private static string TestDataDir =>
            Path.Combine(AppContext.BaseDirectory, "TestData");

        private static string FieldParserDir =>
            Path.Combine(TestDataDir, "FieldParser");

        // ── Named [Theory] fixtures ────────────────────────────────────────────

        /// <summary>Discovers the registered FieldParser fixture files at runtime.</summary>
        public static TheoryData<string> NamedFixtures()
        {
            var data = new TheoryData<string>();
            if (!Directory.Exists(FieldParserDir)) return data;
            foreach (string f in Directory.GetFiles(FieldParserDir, "*.eb.bytes")
                                          .OrderBy(x => x))
                data.Add(f);
            return data;
        }

        /// <summary>
        /// Each named FieldParser fixture must round-trip to byte-identical output.
        /// On failure xunit reports the file name and the exact first mismatching byte.
        /// </summary>
        [Theory]
        [MemberData(nameof(NamedFixtures))]
        public void Decode_ThenEncode_IsIdentical(string filePath)
        {
            byte[] original = File.ReadAllBytes(filePath);
            FieldScript fs   = FieldScript.Decode(original);   // throws on bad decode
            byte[] encoded   = fs.Encode();
            AssertBytesIdentical(original, encoded, Path.GetFileName(filePath));
        }

        // ── Comprehensive sweep ────────────────────────────────────────────────

        /// <summary>
        /// Every *.eb.bytes file under TestData/ must decode without throwing.
        /// This is the hard coverage gate: zero opcode-table gaps, zero malformed
        /// reads.  Failure message names every file that threw and its exception.
        /// </summary>
        [Fact]
        public void AllAvailableFieldFiles_DecodeWithoutException()
        {
            if (!Directory.Exists(TestDataDir)) return;

            string[] files = Directory.GetFiles(TestDataDir, "*.eb.bytes",
                                                SearchOption.AllDirectories)
                                      .OrderBy(x => x)
                                      .ToArray();

            if (files.Length == 0) return;

            var failures = new List<string>();

            foreach (string filePath in files)
            {
                byte[] data;
                try { data = File.ReadAllBytes(filePath); }
                catch (Exception ex)
                {
                    failures.Add($"  [IO]     {RelativeName(filePath)}: {ex.Message}");
                    continue;
                }

                try { FieldScript.Decode(data); }
                catch (Exception ex)
                {
                    failures.Add(
                        $"  [DECODE] {RelativeName(filePath)}: " +
                        $"{ex.GetType().Name}: {ex.Message}");
                }
            }

            Assert.True(failures.Count == 0,
                $"{failures.Count} of {files.Length} files failed to decode:\n" +
                string.Join("\n", failures));
        }

        /// <summary>
        /// Sweep every *.eb.bytes file found anywhere under TestData/.
        /// Collects all decode exceptions and encode mismatches in one pass so a
        /// single run gives a complete picture of coverage.
        ///
        /// Fail condition:  any file that decodes successfully but encodes differently.
        /// Info condition:  files that throw on decode are listed but do NOT fail this
        ///                  test — they indicate opcode coverage gaps to fix later.
        ///                  (Phase 9.2 goal: zero round-trip failures on decodable files.)
        /// </summary>
        [Fact]
        public void RoundTrip_AllAvailableFieldFiles_ZeroEncodeMismatches()
        {
            if (!Directory.Exists(TestDataDir))
                return; // no fixture data available — skip gracefully

            string[] files = Directory.GetFiles(TestDataDir, "*.eb.bytes",
                                                SearchOption.AllDirectories)
                                      .OrderBy(x => x)
                                      .ToArray();

            if (files.Length == 0) return;

            var decodeFailures  = new List<string>();
            var encodeMismatches = new List<string>();

            foreach (string filePath in files)
            {
                string name = RelativeName(filePath);
                byte[] original;
                try { original = File.ReadAllBytes(filePath); }
                catch (Exception ex)
                {
                    decodeFailures.Add($"  [IO]     {name}: {ex.Message}");
                    continue;
                }

                FieldScript fs;
                try { fs = FieldScript.Decode(original); }
                catch (Exception ex)
                {
                    decodeFailures.Add($"  [DECODE] {name}: {ex.GetType().Name}: {ex.Message}");
                    continue;
                }

                byte[] encoded;
                try { encoded = fs.Encode(); }
                catch (Exception ex)
                {
                    encodeMismatches.Add($"  [ENCODE] {name}: {ex.GetType().Name}: {ex.Message}");
                    continue;
                }

                if (original.Length != encoded.Length)
                {
                    encodeMismatches.Add(
                        $"  [LEN]    {name}: {original.Length} → {encoded.Length} bytes");
                    continue;
                }

                int firstDiff = FirstDifference(original, encoded);
                if (firstDiff >= 0)
                {
                    encodeMismatches.Add(
                        $"  [DIFF]   {name}: first diff @ 0x{firstDiff:X}" +
                        $" (expected 0x{original[firstDiff]:X2}, got 0x{encoded[firstDiff]:X2})");
                }
            }

            int total    = files.Length;
            int decoded  = total - decodeFailures.Count;
            int perfect  = decoded - encodeMismatches.Count;

            // Decode failures are informational — printed but do NOT fail the test.
            // Encode mismatches ARE failures.
            string report = BuildReport(total, decoded, perfect,
                                         decodeFailures, encodeMismatches);

            Assert.True(encodeMismatches.Count == 0, report);

            // If there were decode failures, emit them as a skipped-files note via
            // a passing assertion so they appear in the test output.
            if (decodeFailures.Count > 0)
            {
                // Not a hard failure — just surface the count so it's visible.
                // (Use Output.WriteLine if xunit ITestOutputHelper is wired in;
                //  for now the report string is embedded in a trivially-true assertion.)
                Assert.True(true, report);
            }
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static void AssertBytesIdentical(byte[] expected, byte[] actual, string name)
        {
            // Guard: do NOT embed array accesses in the message argument of Assert.True —
            // C# evaluates both arguments eagerly before the call, so expected[-1] would
            // throw IndexOutOfRangeException when diff == -1 (all bytes matched).
            if (expected.Length != actual.Length)
                Assert.Fail($"{name}: encoded length {actual.Length} ≠ original {expected.Length}");

            int diff = FirstDifference(expected, actual);
            if (diff >= 0)
                Assert.Fail($"{name}: first diff @ 0x{diff:X}" +
                            $" (expected 0x{expected[diff]:X2}, got 0x{actual[diff]:X2})");
        }

        private static int FirstDifference(byte[] a, byte[] b)
        {
            int len = Math.Min(a.Length, b.Length);
            for (int i = 0; i < len; i++)
                if (a[i] != b[i]) return i;
            return -1;
        }

        private static string RelativeName(string fullPath)
        {
            string anchor = TestDataDir;
            return fullPath.StartsWith(anchor, StringComparison.OrdinalIgnoreCase)
                ? fullPath.Substring(anchor.Length).TrimStart(Path.DirectorySeparatorChar,
                                                               Path.AltDirectorySeparatorChar)
                : Path.GetFileName(fullPath);
        }

        private static string BuildReport(int total, int decoded, int perfect,
                                          List<string> decodeFailures,
                                          List<string> encodeMismatches)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Round-trip sweep: {total} files, {decoded} decoded, " +
                          $"{perfect} byte-identical, {encodeMismatches.Count} encode failures.");

            if (encodeMismatches.Count > 0)
            {
                sb.AppendLine($"\nENCODE MISMATCHES ({encodeMismatches.Count}):");
                foreach (string m in encodeMismatches) sb.AppendLine(m);
            }

            if (decodeFailures.Count > 0)
            {
                sb.AppendLine($"\nDECODE SKIPS ({decodeFailures.Count} files — informational):");
                int shown = Math.Min(decodeFailures.Count, 30);
                foreach (string d in decodeFailures.Take(shown)) sb.AppendLine(d);
                if (decodeFailures.Count > shown)
                    sb.AppendLine($"  ... and {decodeFailures.Count - shown} more.");
            }

            return sb.ToString();
        }
    }
}
