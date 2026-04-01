// StiltzkinsBag.Core/Parsing/UnityArchiver.cs
//
// Read-only port of Hades Workshop's UnityArchiveMetaData::Load and GetFileOffsetByIndex.
// Used to extract individual files from p0data2.bin (and similar archives) as byte arrays.
// We never write back to archives — patched files are always written as raw .bytes files
// in the mod output folder.
//
// Binary layout (confirmed against UnityArchiver.cpp):
//
//   [0..7]    magic string — "UnityRaw" or other
//   If "UnityRaw": actual header begins at 0x70; otherwise at 0.
//
//   Header (big-endian uint32s):
//     header_size         4 bytes BE
//     header_file_size    4 bytes BE
//     header_id           4 bytes BE  — must be 0x0F
//     header_file_offset  4 bytes BE  — data section start (relative to archive start)
//     header_unknown1     4 bytes BE
//
//   [+20]  header_version   8 bytes (ASCII string, not null-terminated)
//   [+28]  header_unknown2  4 bytes LE
//   [+32]  header_unknown3  1 byte  — if 1, unkstruct follows each type entry
//   [+33]  header_file_info_amount  4 bytes LE
//
//   For each type entry (header_file_info_amount entries):
//     file_info_type     4 bytes LE  — if >= 0: skip 0x10 bytes; if < 0: skip 0x20 bytes
//     [if header_unknown3 == 1]:
//       unkstruct_amount     4 bytes LE
//       unkstruct_text_size  4 bytes LE
//       skip unkstruct_amount * 0x18 + unkstruct_text_size bytes
//
//   header_file_amount   4 bytes LE
//   [align to 4-byte boundary]
//
//   For each file entry (header_file_amount entries), each 0x1C bytes:
//     file_info           8 bytes LE (int64)
//     file_offset_start   4 bytes LE — offset of file data relative to header_file_offset
//     file_size           4 bytes LE
//     file_type1          4 bytes LE — Unity asset type ID
//     file_type2          4 bytes LE
//     file_flags          4 bytes LE
//
//   For files whose type has a name (types 21,28,43,48,49,109,115,213), the data
//   at (archiveStart + header_file_offset + file_offset_start) begins with:
//     file_name_len  4 bytes LE
//     file_name      file_name_len bytes (ASCII, no null terminator)
//     [if type == 49]: align to 4-byte boundary, then text_file_size 4 bytes LE
//
//   GetFileOffsetByIndex returns:
//     archiveStart + header_file_offset + file_offset_start
//     + (if has name): file_name_len + align(file_name_len) + 4
//     + (if type == 49): + 4
//
//   This is the byte offset at which the actual file payload begins.
//
// AssetBundle (type 142) — full path table:
//   Archives like p0data2.bin contain a type-142 AssetBundle entry whose payload
//   maps full asset paths (e.g. "assets/resources/battlemap/.../dbfile0000.raw16.bytes")
//   to file_info int64 values. Since many entries share the same short name
//   (e.g. all battle stats are named "dbfile0000.raw16"), ExtractByPath must use
//   this full-path → file_info → entry-index chain to find the correct entry.
//
//   AssetBundle payload layout (from UnityArchiveAssetBundle::Write in UnityArchiver.cpp):
//     4 bytes LE: unknown (always 0)
//     4 bytes LE: bundle_amount
//     bundle_amount × 12 bytes: bundle entries (flag=4 bytes, info=8 bytes each)
//     4 bytes LE: amount  — number of path entries
//     For each path entry:
//       4 bytes LE: path_len
//       path_len bytes: path string (ASCII, forward slashes, no null terminator)
//       [align to 4-byte boundary]
//       4 bytes LE: index  — 1-based file entry index (informational)
//       4 bytes LE: unk1
//       4 bytes LE: unk2
//       8 bytes LE: info   — matches file_info in the main entry table
//
//   Type 142 is NOT in HasFileTypeName — its payload begins directly at
//   archiveStart + header_file_offset + file_offset_start with no name prefix.

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StiltzkinsBag.Core.Parsing;

/// <summary>
/// Read-only extractor for FF9 Steam Unity archive files (p0data2.bin, p0data7.bin, etc.).
/// Parses the archive header table, then extracts named files as byte arrays on demand.
/// </summary>
public sealed class UnityArchiver : IDisposable
{
    // ── Unity asset type IDs that carry an embedded name string ───────────
    // Matches HasFileTypeName() in UnityArchiver.cpp
    private static readonly HashSet<uint> NamedTypes = [21, 28, 43, 48, 49, 109, 115, 213];
    private const uint TextAssetType = 49;   // TextAsset — has extra text_file_size field
    private const uint AssetBundleType = 142;  // AssetBundle — contains the full path table
    private const uint RequiredHeaderId = 0x0F;
    private const int UnityRawPreambleSize = 0x70;

    // ── Per-file metadata parsed from the header table ────────────────────
    private readonly record struct FileEntry(
        long Info,
        uint OffsetStart,   // relative to header_file_offset
        uint DataSize,      // raw size from header
        uint Type1,
        uint NameLen,
        string Name,
        uint TextSize);     // only populated for TextAsset (type 49)

    // ── State ──────────────────────────────────────────────────────────────
    private readonly Stream _stream;
    private readonly uint _archiveStart;   // 0x70 for UnityRaw, 0 otherwise
    private readonly uint _fileOffset;     // header_file_offset
    private readonly FileEntry[] _entries;

    // Built from the AssetBundle entry (type 142) if present.
    // Maps full asset path (forward slashes, no leading slash) → file_info int64.
    // e.g. "assets/resources/battlemap/battlescene/evt_battle_ac_e028f/dbfile0000.raw16.bytes"
    private readonly Dictionary<string, long> _pathToInfo;

    // Reverse map: file_info → index in _entries. Built unconditionally.
    private readonly Dictionary<long, int> _infoToIndex;

    private bool _disposed;

    // ── Public surface ─────────────────────────────────────────────────────

    /// <summary>Number of file entries in this archive.</summary>
    public int FileCount => _entries.Length;

    /// <summary>Archive file path, for diagnostics.</summary>
    public string ArchivePath { get; }

    // ── Factory ────────────────────────────────────────────────────────────

    /// <summary>
    /// Opens an archive file and parses its header table.
    /// </summary>
    /// <param name="archivePath">Full path to the archive (e.g. …\p0data2.bin).</param>
    /// <exception cref="InvalidDataException">If the archive magic or header ID is invalid.</exception>
    public static UnityArchiver Open(string archivePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(archivePath);
        if (!File.Exists(archivePath))
            throw new FileNotFoundException($"Archive not found: '{archivePath}'", archivePath);

        // Keep the stream open — we read file data lazily on Extract()
        var stream = new FileStream(archivePath, FileMode.Open, FileAccess.Read,
                                    FileShare.Read, bufferSize: 4096, useAsync: false);
        try
        {
            return new UnityArchiver(stream, archivePath);
        }
        catch
        {
            stream.Dispose();
            throw;
        }
    }

    // ── Constructor ────────────────────────────────────────────────────────

    private UnityArchiver(Stream stream, string archivePath)
    {
        _stream = stream;
        ArchivePath = archivePath;

        using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);

        // ── Detect archive type ────────────────────────────────────────────
        string magic = new string(reader.ReadChars(8));
        _archiveStart = magic == "UnityRaw" ? (uint)UnityRawPreambleSize : 0u;

        stream.Position = _archiveStart;

        // ── Header (big-endian) ────────────────────────────────────────────
        /*uint headerSize     =*/
        ReadUInt32BE(reader);
        /*uint headerFileSize =*/
        ReadUInt32BE(reader);
        uint headerId = ReadUInt32BE(reader);
        _fileOffset = ReadUInt32BE(reader);
        /*uint headerUnknown1 =*/
        ReadUInt32BE(reader);

        if (headerId != RequiredHeaderId)
            throw new InvalidDataException(
                $"Archive header ID mismatch in '{archivePath}': " +
                $"expected 0x{RequiredHeaderId:X2}, got 0x{headerId:X2}. " +
                "File is not a valid FF9 Unity archive.");

        // ── Version + unknowns ─────────────────────────────────────────────
        reader.ReadBytes(8);        // header_version (8-byte ASCII, unused by us)
        reader.ReadBytes(4);        // header_unknown2 (LE uint32)
        byte headerUnknown3 = reader.ReadByte();

        // ── Type info section (skip it — we don't need type metadata) ─────
        uint typeCount = ReadUInt32LE(reader);
        for (int i = 0; i < typeCount; i++)
        {
            int typeId = (int)ReadUInt32LE(reader);   // file_info_type
            stream.Position += typeId >= 0 ? 0x10 : 0x20;

            if (headerUnknown3 == 1)
            {
                uint unkAmount = ReadUInt32LE(reader);
                uint unkTextSize = ReadUInt32LE(reader);
                stream.Position += unkAmount * 0x18 + unkTextSize;
            }
        }

        // ── File entry table ───────────────────────────────────────────────
        uint fileCount = ReadUInt32LE(reader);
        AlignStream(stream, 4);

        var rawEntries = new (long Info, uint OffsetStart, uint DataSize, uint Type1, uint Type2, uint Flags)[fileCount];
        for (int i = 0; i < fileCount; i++)
        {
            long info = (long)ReadUInt64LE(reader);
            uint offsetStart = ReadUInt32LE(reader);
            uint dataSize = ReadUInt32LE(reader);
            uint type1 = ReadUInt32LE(reader);
            uint type2 = ReadUInt32LE(reader);
            uint flags = ReadUInt32LE(reader);
            rawEntries[i] = (info, offsetStart, dataSize, type1, type2, flags);
        }

        // ── Read embedded names for named-type files ───────────────────────
        _entries = new FileEntry[fileCount];
        for (int i = 0; i < fileCount; i++)
        {
            var (info, offsetStart, dataSize, type1, _, _) = rawEntries[i];

            string name = string.Empty;
            uint nameLen = 0;
            uint textSize = 0;

            if (NamedTypes.Contains(type1))
            {
                long dataPos = _archiveStart + _fileOffset + offsetStart;
                stream.Position = dataPos;

                nameLen = ReadUInt32LE(reader);
                name = new string(reader.ReadChars((int)nameLen));

                if (type1 == TextAssetType)
                {
                    AlignStream(stream, 4);
                    textSize = ReadUInt32LE(reader);
                }
            }

            _entries[i] = new FileEntry(info, offsetStart, dataSize, type1, nameLen, name, textSize);
        }

        // ── Build info → index reverse map ────────────────────────────────
        _infoToIndex = new Dictionary<long, int>(_entries.Length);
        for (int i = 0; i < _entries.Length; i++)
            _infoToIndex[_entries[i].Info] = i;

        // ── Parse AssetBundle to get full path table ───────────────────────
        // The type-142 AssetBundle entry maps full asset paths to file_info values.
        // This is required for ExtractByPath on archives like p0data2.bin where
        // many entries share the same short name (e.g. "dbfile0000.raw16").
        _pathToInfo = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < _entries.Length; i++)
        {
            if (_entries[i].Type1 == AssetBundleType)
            {
                ParseAssetBundle(i, reader);
                break;  // only one AssetBundle per archive
            }
        }
    }

    // ── AssetBundle parser ─────────────────────────────────────────────────

    /// <summary>
    /// Parses the type-142 AssetBundle payload to populate _pathToInfo.
    /// Layout from UnityArchiveAssetBundle::Write in UnityArchiver.cpp:
    ///   4 bytes LE: unknown (0)
    ///   4 bytes LE: bundle_amount
    ///   bundle_amount × 12 bytes: (flag=4, info=8) each — skipped
    ///   4 bytes LE: amount
    ///   For each path entry:
    ///     4 bytes LE: path_len
    ///     path_len bytes: path string
    ///     [align 4]
    ///     4 bytes LE: index (1-based, informational)
    ///     4 bytes LE: unk1
    ///     4 bytes LE: unk2
    ///     8 bytes LE: info  — matches file_info in the main entry table
    /// </summary>
    private void ParseAssetBundle(int entryIndex, BinaryReader reader)
    {
        ref readonly FileEntry e = ref _entries[entryIndex];

        // Type 142 is NOT in HasFileTypeName — payload starts directly at data position.
        long payloadStart = _archiveStart + _fileOffset + e.OffsetStart;
        _stream.Position = payloadStart;

        /*uint unknown =*/
        ReadUInt32LE(reader);
        uint bundleAmount = ReadUInt32LE(reader);

        // Skip bundle entries: each is flag(4) + info(8) = 12 bytes
        _stream.Position += bundleAmount * 12L;

        uint amount = ReadUInt32LE(reader);
        for (uint i = 0; i < amount; i++)
        {
            uint pathLen = ReadUInt32LE(reader);
            string path = new string(reader.ReadChars((int)pathLen));
            AlignStream(_stream, 4);

            /*uint index =*/
            ReadUInt32LE(reader);
            /*uint unk1  =*/
            ReadUInt32LE(reader);
            /*uint unk2  =*/
            ReadUInt32LE(reader);
            long info = (long)ReadUInt64LE(reader);

            // Store with forward slashes, no leading slash — matches archive convention
            // e.g. "assets/resources/battlemap/battlescene/evt_battle_ac_e028f/dbfile0000.raw16.bytes"
            _pathToInfo[path] = info;
        }
    }

    // ── Extraction API ─────────────────────────────────────────────────────

    /// <summary>
    /// Extracts a file by short name and returns its payload bytes.
    /// Name comparison is case-insensitive. Also tries stripping/adding ".bytes".
    /// Use this for archives like p0data7.bin where short names are unique
    /// (e.g. field script names like "evt_alex1_ac_ent_2f.eb").
    /// </summary>
    /// <param name="fileName">
    /// The short file name as stored in the archive (e.g. "evt_alex1_ac_ent_2f.eb").
    /// Do not include the ".bytes" extension — TextAsset names typically omit it.
    /// If not found, also tries with ".bytes" stripped from the input.
    /// </param>
    /// <exception cref="FileNotFoundException">If no entry with that name exists.</exception>
    public byte[] Extract(string fileName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        int index = FindIndexByShortName(fileName);
        if (index < 0)
        {
            // Try stripping .bytes extension (archive names often lack it)
            string stripped = fileName.EndsWith(".bytes", StringComparison.OrdinalIgnoreCase)
                ? fileName[..^6]
                : fileName + ".bytes";
            index = FindIndexByShortName(stripped);
        }

        if (index < 0)
            throw new FileNotFoundException(
                $"File '{fileName}' not found in archive '{ArchivePath}'. " +
                $"Archive contains {_entries.Length} entries.");

        return ExtractByIndex(index);
    }

    /// <summary>
    /// Extracts a file by its full relative path, using the AssetBundle path table
    /// when available. This is the correct method for archives like p0data2.bin
    /// where many entries share the same short name.
    ///
    /// Input path formats accepted (all equivalent):
    ///   \StreamingAssets\assets\resources\battlemap\...\dbfile0000.raw16.bytes
    ///   assets/resources/battlemap/.../dbfile0000.raw16.bytes
    ///
    /// Falls back to short-name Extract() if the AssetBundle table is absent.
    /// </summary>
    /// <exception cref="FileNotFoundException">If the path is not found.</exception>
    public byte[] ExtractByPath(string relativePath)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrEmpty(relativePath);

        // If we have the AssetBundle path table, use it for precise matching.
        if (_pathToInfo.Count > 0)
        {
            string normalized = NormalizePath(relativePath);

            // Try exact normalized path
            if (_pathToInfo.TryGetValue(normalized, out long info) &&
                _infoToIndex.TryGetValue(info, out int idx))
                return ExtractByIndex(idx);

            // Try stripping ".bytes" suffix (archive paths may omit it)
            if (normalized.EndsWith(".bytes", StringComparison.OrdinalIgnoreCase))
            {
                string noBytes = normalized[..^6];
                if (_pathToInfo.TryGetValue(noBytes, out info) &&
                    _infoToIndex.TryGetValue(info, out idx))
                    return ExtractByIndex(idx);
            }
            else
            {
                string withBytes = normalized + ".bytes";
                if (_pathToInfo.TryGetValue(withBytes, out info) &&
                    _infoToIndex.TryGetValue(info, out idx))
                    return ExtractByIndex(idx);
            }

            throw new FileNotFoundException(
                $"Path '{relativePath}' (normalized: '{normalized}') not found in " +
                $"AssetBundle path table of '{ArchivePath}'. " +
                $"Table contains {_pathToInfo.Count} entries.");
        }

        // No AssetBundle path table — fall back to leaf name (p0data7.bin style).
        string leaf = Path.GetFileName(relativePath.Replace('\\', '/'));
        return Extract(leaf);
    }

    /// <summary>
    /// Returns short embedded names for all named-type entries in this archive.
    /// For p0data7.bin field scripts these are unique (e.g. "evt_alex1_ac_ent_2f.eb").
    /// For p0data2.bin battle files many entries share the same short name
    /// ("dbfile0000.raw16") — use <see cref="GetFullPaths"/> and
    /// <see cref="ExtractByPath"/> when unique identification is needed.
    /// </summary>
    public IReadOnlyList<string> GetFileNames()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var names = new List<string>(_entries.Length);
        foreach (var e in _entries)
            if (e.Name.Length > 0)
                names.Add(e.Name);
        return names;
    }

    /// <summary>
    /// Returns full asset paths from the AssetBundle table (type 142) when present.
    /// e.g. "assets/resources/battlemap/battlescene/evt_battle_ac_e028f/dbfile0000.raw16.bytes"
    /// Returns an empty list if this archive has no AssetBundle entry (e.g. p0data7.bin
    /// field scripts, which use unique short names and don't need path disambiguation).
    /// Use this to enumerate all uniquely-identifiable files in archives like p0data2.bin.
    /// </summary>
    public IReadOnlyList<string> GetFullPaths()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _pathToInfo.Keys.ToList();
    }

    // ── Private helpers ────────────────────────────────────────────────────

    /// <summary>
    /// Normalizes a path to the format used in the AssetBundle table:
    /// forward slashes, no leading slash, no "StreamingAssets/" prefix.
    ///
    /// Input:  \StreamingAssets\assets\resources\battlemap\...\dbfile0000.raw16.bytes
    /// Output: assets/resources/battlemap/.../dbfile0000.raw16.bytes
    /// </summary>
    private static string NormalizePath(string path)
    {
        // Convert backslashes and strip leading slash
        string normalized = path.Replace('\\', '/').TrimStart('/');

        // Strip "StreamingAssets/" prefix if present — archive paths start with "assets/"
        const string streamingPrefix = "StreamingAssets/";
        if (normalized.StartsWith(streamingPrefix, StringComparison.OrdinalIgnoreCase))
            normalized = normalized[streamingPrefix.Length..];

        return normalized;
    }

    private int FindIndexByShortName(string name)
    {
        for (int i = 0; i < _entries.Length; i++)
            if (string.Equals(_entries[i].Name, name, StringComparison.OrdinalIgnoreCase))
                return i;
        return -1;
    }

    private byte[] ExtractByIndex(int index)
    {
        ref readonly FileEntry e = ref _entries[index];

        // Compute payload start — mirrors GetFileOffsetByIndex in UnityArchiver.cpp:
        //   archiveStart + header_file_offset + file_offset_start
        //   + (if named): nameLen + align(nameLen) + 4   (skips the name header)
        //   + (if TextAsset): + 4                        (skips text_file_size field)
        long payloadStart = _archiveStart + _fileOffset + e.OffsetStart;
        uint payloadSize = e.DataSize;

        if (NamedTypes.Contains(e.Type1))
        {
            uint nameSkip = e.NameLen + AlignPadding(e.NameLen, 4) + 4;  // +4 for nameLen field
            payloadStart += nameSkip;
            payloadSize -= nameSkip;

            if (e.Type1 == TextAssetType)
            {
                // text_file_size field (4 bytes); actual size comes from TextSize
                payloadStart += 4;
                payloadSize = e.TextSize;
            }
        }

        byte[] result = new byte[payloadSize];
        _stream.Position = payloadStart;
        int read = _stream.Read(result, 0, (int)payloadSize);

        if (read != (int)payloadSize)
            throw new InvalidDataException(
                $"Archive read underflow for entry '{e.Name}' in '{ArchivePath}': " +
                $"expected {payloadSize} bytes, got {read}.");

        return result;
    }

    // ── Binary reading helpers (matching HW's ReadLong/ReadLongBE etc.) ────

    private static uint ReadUInt32BE(BinaryReader r)
    {
        Span<byte> buf = stackalloc byte[4];
        r.BaseStream.ReadExactly(buf);
        return BinaryPrimitives.ReadUInt32BigEndian(buf);
    }

    private static uint ReadUInt32LE(BinaryReader r)
    {
        Span<byte> buf = stackalloc byte[4];
        r.BaseStream.ReadExactly(buf);
        return BinaryPrimitives.ReadUInt32LittleEndian(buf);
    }

    private static ulong ReadUInt64LE(BinaryReader r)
    {
        Span<byte> buf = stackalloc byte[8];
        r.BaseStream.ReadExactly(buf);
        return BinaryPrimitives.ReadUInt64LittleEndian(buf);
    }

    /// <summary>Advances stream position to the next multiple of <paramref name="alignment"/>.</summary>
    private static void AlignStream(Stream s, int alignment)
    {
        long pos = s.Position;
        long rem = pos % alignment;
        if (rem != 0)
            s.Position = pos + (alignment - rem);
    }

    /// <summary>Returns the number of padding bytes needed to align <paramref name="value"/>.</summary>
    private static uint AlignPadding(uint value, uint alignment)
    {
        uint rem = value % alignment;
        return rem == 0 ? 0 : alignment - rem;
    }

    // ── IDisposable ────────────────────────────────────────────────────────

    public void Dispose()
    {
        if (!_disposed)
        {
            _stream.Dispose();
            _disposed = true;
        }
    }
}