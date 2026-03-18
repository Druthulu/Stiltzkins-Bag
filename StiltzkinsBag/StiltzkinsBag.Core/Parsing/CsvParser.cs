using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace StiltzkinsBag.Parsing
{
    /// <summary>
    /// Generic round-trip CSV parser for Memoria Engine CSV files.
    ///
    /// Memoria CSV format rules:
    ///   - Semicolon-delimited
    ///   - Leading lines starting with '#' or '#!' are comment/directive lines — preserved verbatim
    ///   - No real header row; columns are mapped positionally via ClassMap
    ///   - Data rows may end with ;# inline comment — stripped before parsing, re-appended on write
    ///   - Some files have UTF-8 BOM; some are cp1252 — caller passes encoding
    ///   - Array columns use ", " separated values in a single field (e.g. "1, 2, 3" or "AA:101, SA:3")
    ///   - Tab-padded whitespace in some files (CommandSets) — TrimFields handles this
    /// </summary>
    public static class MemoriaCsvParser
    {
        // -----------------------------------------------------------------------------------------
        // Read
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Reads a Memoria CSV file into a list of typed rows using the provided ClassMap.
        /// Preserves the comment block and inline comments internally via <see cref="ParsedCsv{T}"/>.
        /// </summary>
        public static ParsedCsv<T> Read<T, TMap>(string filePath, Encoding? encoding = null)
            where TMap : ClassMap<T>
        {
            encoding ??= DetectEncoding(filePath);
            var lineEnding = DetectLineEnding(filePath, encoding);

            var allLines = File.ReadAllLines(filePath, encoding);

            // Split into comment block and data lines
            var commentLines = new List<string>();
            var dataLines = new List<string>();
            bool inData = false;

            foreach (var line in allLines)
            {
                if (!inData && (line.StartsWith("#") || line.Length == 0))
                    commentLines.Add(line);
                else
                {
                    inData = true;
                    dataLines.Add(line);
                }
            }

            // Strip trailing empty lines from data block
            while (dataLines.Count > 0 && string.IsNullOrWhiteSpace(dataLines[^1]))
                dataLines.RemoveAt(dataLines.Count - 1);

            // Extract inline comments from each data line
            // A Memoria inline comment is the last ;# ... segment on the line
            var inlineComments = new List<string?>();
            var cleanDataLines = new List<string>();

            foreach (var line in dataLines)
            {
                var (clean, comment) = StripInlineComment(line);
                cleanDataLines.Add(clean);
                inlineComments.Add(comment);
            }

            // Parse clean data lines via CsvHelper
            var config = BuildConfig();
            var csvText = string.Join("\n", cleanDataLines);
            var rows = new List<T>();

            using var reader = new StringReader(csvText);
            using var csvReader = new CsvReader(reader, config);
            csvReader.Context.RegisterClassMap<TMap>();

            while (csvReader.Read())
                rows.Add(csvReader.GetRecord<T>());

            return new ParsedCsv<T>(
                filePath,
                encoding,
                lineEnding,
                commentLines,
                rows,
                inlineComments
            );
        }

        // -----------------------------------------------------------------------------------------
        // Write
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Writes a <see cref="ParsedCsv{T}"/> back to disk, preserving comment block and
        /// inline comments. Output is byte-identical to input if no rows were modified.
        /// </summary>
        public static void Write<T, TMap>(ParsedCsv<T> parsed, string? outputPath = null)
            where TMap : ClassMap<T>
        {
            var path = outputPath ?? parsed.SourcePath;
            var newLine = parsed.LineEnding;

            // Build config with NewLine set so CsvWriter never emits its own line ending.
            // We control all newlines manually so inline comments land on the correct line.
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = false,
                TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null,
                BadDataFound = null,
                NewLine = newLine,
            };

            // Use the encoding as-is — it was captured precisely (BOM or no BOM) during Read.
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, parsed.Encoding, leaveOpen: true)
            {
                NewLine = newLine
            };

            // 1. Emit comment block verbatim
            foreach (var line in parsed.CommentLines)
            {
                writer.Write(line);
                writer.Write(newLine);
            }

            writer.Flush();

            // 2. Emit data rows via CsvWriter.
            //    We use WriteRecord then immediately overwrite the trailing newline CsvHelper emits
            //    by tracking stream position before/after and re-seeking to insert the inline comment.
            //    Simpler: serialize each row to a temp string, strip its trailing newline, append comment, then write.
            using var csvWriter = new CsvWriter(writer, config);
            csvWriter.Context.RegisterClassMap<TMap>();

            for (int i = 0; i < parsed.Rows.Count; i++)
            {
                // Serialize row to a temporary string buffer
                using var rowMs = new MemoryStream();
                using var rowWriter = new StreamWriter(rowMs, parsed.Encoding, leaveOpen: true)
                {
                    NewLine = newLine
                };
                using (var tempCsv = new CsvWriter(rowWriter, config))
                {
                    tempCsv.Context.RegisterClassMap<TMap>();
                    tempCsv.WriteRecord(parsed.Rows[i]);
                    tempCsv.NextRecord();
                }
                rowWriter.Flush();

                rowMs.Position = 0;
                var rowStr = new StreamReader(rowMs, parsed.Encoding).ReadToEnd();

                // Strip the trailing newline CsvHelper appended — we control it
                if (rowStr.EndsWith(newLine))
                    rowStr = rowStr[..^newLine.Length];

                // Re-append inline comment if present, then our newline
                var comment = i < parsed.InlineComments.Count ? parsed.InlineComments[i] : null;
                writer.Write(rowStr);
                if (comment != null)
                    writer.Write(comment);
                writer.Write(newLine);
            }

            writer.Flush();

            ms.Position = 0;
            File.WriteAllBytes(path, ms.ToArray());
        }

        // -----------------------------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------------------------

        /// <summary>
        /// Strips the trailing ;# inline comment from a data line.
        /// Returns (cleanLine, ";# comment text") or (line, null) if no comment present.
        /// Only the LAST occurrence of ;# is treated as the comment delimiter.
        /// </summary>
        private static (string clean, string? comment) StripInlineComment(string line)
        {
            // Find last ";#" that is not inside a quoted field
            // Memoria CSVs do not use quoting, so a simple last-index search is safe
            int idx = line.LastIndexOf(";#", StringComparison.Ordinal);
            if (idx < 0)
                return (line, null);

            return (line[..idx], line[idx..]);
        }

        /// <summary>
        /// Detects file encoding by checking for UTF-8 BOM first, then validating as UTF-8.
        /// Falls back to Windows-1252 (cp1252) if the file contains invalid UTF-8 sequences.
        /// </summary>
        private static Encoding DetectEncoding(string filePath)
        {
            var raw = File.ReadAllBytes(filePath);

            // UTF-8 BOM: EF BB BF
            if (raw.Length >= 3 && raw[0] == 0xEF && raw[1] == 0xBB && raw[2] == 0xBF)
                return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

            // Try to decode as strict UTF-8 (no replacement characters)
            try
            {
                var strictUtf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
                strictUtf8.GetString(raw);
                return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            }
            catch (DecoderFallbackException)
            {
                // Contains bytes that are not valid UTF-8 — treat as cp1252.
                // RegisterProvider is idempotent; safe to call multiple times.
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                return Encoding.GetEncoding(1252);
            }
        }

        /// <summary>
        /// Detects whether the file uses CRLF or LF line endings by scanning raw bytes.
        /// Returns "\r\n" or "\n".
        /// </summary>
        private static string DetectLineEnding(string filePath, Encoding encoding)
        {
            var raw = File.ReadAllBytes(filePath);
            for (int i = 0; i < raw.Length - 1; i++)
            {
                if (raw[i] == 0x0D && raw[i + 1] == 0x0A)
                    return "\r\n";
                if (raw[i] == 0x0A)
                    return "\n";
            }
            return "\n"; // default if file has only one line
        }

        private static CsvConfiguration BuildConfig() => new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = false,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            BadDataFound = null,
        };
    }

    // =============================================================================================
    // ParsedCsv<T> — the round-trip carrier
    // =============================================================================================

    /// <summary>
    /// Holds the result of a Memoria CSV parse, including all metadata needed to write
    /// a byte-identical file. Modify <see cref="Rows"/> freely; everything else is preserved.
    /// </summary>
    public sealed class ParsedCsv<T>
    {
        /// <summary>Original file path — used as default output path on write.</summary>
        public string SourcePath { get; }

        /// <summary>Encoding detected from the original file (UTF-8, UTF-8-BOM, or cp1252).</summary>
        public Encoding Encoding { get; }

        /// <summary>Line ending detected from the original file — "\r\n" or "\n".</summary>
        public string LineEnding { get; }

        /// <summary>All leading #/#!/ comment and directive lines, preserved verbatim.</summary>
        public IReadOnlyList<string> CommentLines { get; }

        /// <summary>Parsed data rows. Safe to modify, reorder, add, or remove.</summary>
        public List<T> Rows { get; }

        /// <summary>
        /// Per-row inline comment strings (e.g. ";# 000 - Hammer").
        /// Index matches <see cref="Rows"/>. Null entry = no comment on that row.
        /// If rows are added, append null to this list to suppress comment for new rows.
        /// </summary>
        public List<string?> InlineComments { get; }

        internal ParsedCsv(
            string sourcePath,
            Encoding encoding,
            string lineEnding,
            IEnumerable<string> commentLines,
            IEnumerable<T> rows,
            IEnumerable<string?> inlineComments)
        {
            SourcePath = sourcePath;
            Encoding = encoding;
            LineEnding = lineEnding;
            CommentLines = commentLines.ToList().AsReadOnly();
            Rows = rows.ToList();
            InlineComments = inlineComments.ToList();
        }
    }

    // =============================================================================================
    // Type converters for array columns
    // =============================================================================================

    /// <summary>
    /// Converts Memoria's boolean columns (stored as 0/1 integers) to/from C# bool.
    /// CsvHelper's default bool converter writes "True"/"False" which breaks round-trip.
    /// </summary>
    public sealed class MemoriaBoolConverter : CsvHelper.TypeConversion.DefaultTypeConverter
    {
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            var t = text.Trim();
            return t == "1" || t.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public override string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
            => value is true ? "1" : "0";
    }

    /// <summary>
    /// Converts a semicolon-field containing a comma-separated int list to int[].
    /// Handles empty fields (returns empty array).
    /// Example field value: "1, 2, 16, 17"
    /// </summary>
    public sealed class IntArrayConverter : CsvHelper.TypeConversion.DefaultTypeConverter
    {
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<int>();

            return text
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.Parse(s.Trim(), CultureInfo.InvariantCulture))
                .ToArray();
        }

        public override string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value is not int[] arr || arr.Length == 0)
                return string.Empty;

            return string.Join(", ", arr);
        }
    }

    /// <summary>
    /// Converts a semicolon-field containing a comma-separated ability ref list to string[].
    /// Handles empty fields (returns empty array).
    /// Example field value: "AA:101, SA:3, AA:102"
    /// </summary>
    public sealed class AbilityRefArrayConverter : CsvHelper.TypeConversion.DefaultTypeConverter
    {
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<string>();

            return text
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToArray();
        }

        public override string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value is not string[] arr || arr.Length == 0)
                return string.Empty;

            return string.Join(", ", arr);
        }
    }

    /// <summary>
    /// Converts a semicolon-field containing a comma-separated string list to string[].
    /// Used for columns like CustomTexture in Weapons.csv.
    /// </summary>
    public sealed class StringArrayConverter : CsvHelper.TypeConversion.DefaultTypeConverter
    {
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<string>();

            return text
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToArray();
        }

        public override string? ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value is not string[] arr || arr.Length == 0)
                return string.Empty;

            return string.Join(", ", arr);
        }
    }
}