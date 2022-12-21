using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Win32.SafeHandles;
using OpenTemple.Interop;

namespace OpenTemple.Core.IO.TroikaArchives;

public struct ArchiveEntry
{
    /// <summary>
    ///     Funnily, this is the actual in-memory pointer of the name string, when
    ///     the entry was serialized to disk.
    /// </summary>
    public int NameStart;

    public int NameLength;

    public uint OriginalSize;

    public uint SizeOnDisk;

    public uint StartOfData;

    public int ParentEntry;

    public int FirstChildEntry;

    public int NextSiblingEntry;

    private byte _flags;

    private const byte FlagDirectory = 0x01;
    private const byte FlagCompressed = 0x02;
    private const byte FlagDeleted = 0x04;

    private bool GetFlag(byte flag) => (_flags & flag) != 0;
    private void SetFlag(byte flag, bool enabled)
    {
        if (enabled)
        {
            _flags |= flag;
        }
        else
        {
            _flags &= unchecked((byte)~flag);
        }
    }

    public bool IsDirectory
    {
        get => GetFlag(FlagDirectory);
        set => SetFlag(FlagDirectory, value);
    }

    public bool IsCompressed
    {
        get => GetFlag(FlagCompressed);
        set => SetFlag(FlagCompressed, value);
    }

    public bool IsDeleted
    {
        get => GetFlag(FlagDeleted);
        set => SetFlag(FlagDeleted, value);
    }
}

/// <summary>
///     This class allows access to Troika .dat files, which are essentially optimized ZIP files.
/// </summary>
public sealed class TroikaArchive : IDisposable
{
    private const uint SignatureV0 = 0x44_41_54_20; // "DAT " in little-endian

    private const uint SignatureV1 = 0x44_41_54_31; // "DAT1" in little-endian

    private readonly SafeFileHandle _fileHandle;

    private readonly FileStream _fileStream;

    /// <summary>
    /// We query this when we open the file.
    /// </summary>
    private readonly long _fileLength;

    private readonly MemoryPool<byte> _pool = MemoryPool<byte>.Shared;

    private ArchiveEntry[] _entries;

    private char[] _stringHeap;

    private int _stringHeapUsed;

    public TroikaArchive(string path)
    {
        Path = path;

        _fileHandle = File.OpenHandle(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            FileOptions.RandomAccess
        );
        _fileStream = new FileStream(_fileHandle, FileAccess.Read);
        _fileLength = RandomAccess.GetLength(_fileHandle);
            
        // Read the file table
        try
        {
            ReadFileTable(_fileHandle);
        }
        catch
        {
            _fileHandle.Dispose();
            throw;
        }
    }

    public Guid ArchiveGuid { get; private set; }

    public string Path { get; }

    public ReadOnlySpan<ArchiveEntry> Entries => _entries;

    public void Dispose()
    {
        _fileStream.Dispose();
    }

    private void ReadFileTable(SafeFileHandle handle)
    {
        // Read the 12 byte footer at the end of the file
        Span<byte> fileFooter = stackalloc byte[12];
        if (RandomAccess.Read(handle, fileFooter, _fileLength - fileFooter.Length) < fileFooter.Length)
        {
            throw new InvalidDataException("Couldn't read file footer");
        }

        var signature = BitConverter.ToUInt32(fileFooter[..4]);
        var fileTableSize = BitConverter.ToUInt32(fileFooter[8..12]);

        Debug.Assert(fileTableSize <= _fileLength);

        // Seek back to read the archive GUID if it's a newer version of the file format
        if (signature == SignatureV0)
        {
            ArchiveGuid = Guid.Empty;
        }
        else if (signature == SignatureV1)
        {
            Span<byte> guidData = stackalloc byte[16];
            if (RandomAccess.Read(_fileHandle, guidData, _fileLength - 24) != guidData.Length)
            {
                throw new InvalidDataException("Failed to read GUID from archive");
            }

            ArchiveGuid = new Guid(guidData);
        }
        else
        {
            throw new InvalidDataException("Corrupted header in Troika archive " + Path);
        }

        // Seek to the start of the file allocation table
        using var reader = new BinaryReader(_fileStream, Encoding.ASCII, true);
        _fileStream.Seek(-(int) fileTableSize, SeekOrigin.End);

        // Read number of entries and allocate memory for them
        var entryCount = reader.ReadUInt32();

        // When looking at the file table, the space for strings can be deduced from the entry count,
        // since the only dynamic component of an entry is the file name.
        var fixedEntrySize = 8 * sizeof(uint);
        var stringHeapEstimate = fileTableSize - entryCount * fixedEntrySize;
        _stringHeap = new char[stringHeapEstimate];

        _entries = new ArchiveEntry[entryCount];

        for (var i = 0; i < entryCount; i++)
        {
            ReadEntry(reader, ref _entries[i]);
        }
    }

    public string GetFullPath(in ArchiveEntry entry)
    {
        var builder = new StringBuilder(260);
        builder.Append(GetName(entry));

        for (var parentIdx = entry.ParentEntry; parentIdx != -1; parentIdx = _entries[parentIdx].ParentEntry)
        {
            var parentName = GetName(in _entries[parentIdx]);
            builder.Insert(0, '/');
            builder.Insert(0, parentName);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Marks a file as deleted so that it will no longer be found by operations on this archive.
    /// </summary>
    public void SetDeleted(ReadOnlySpan<char> path)
    {
        var index = FindEntryIndex(0, path);
        if (index != -1)
        {
            _entries[index].IsDeleted = true;
        }
    }

    private int FindEntryIndex(int startAt, ReadOnlySpan<char> path, bool hideDeleted = true)
    {
        var nextSegment = GetNextPathSegment(ref path);
        if (nextSegment.IsEmpty || _entries.Length == 0)
        {
            return -1;
        }

        var currentEntryIdx = startAt;
        while (currentEntryIdx != -1)
        {
            if (IsEntryNameEqualTo(currentEntryIdx, nextSegment))
            {
                if (hideDeleted && _entries[currentEntryIdx].IsDeleted)
                {
                    return -1;
                }

                if (path.IsEmpty)
                {
                    return currentEntryIdx;
                }

                var firstChild = _entries[currentEntryIdx].FirstChildEntry;
                if (firstChild != -1)
                {
                    return FindEntryIndex(firstChild, path);
                }

                return -1;
            }

            currentEntryIdx = _entries[currentEntryIdx].NextSiblingEntry;
        }

        return -1;
    }

    private bool FindEntry(int startAt, ReadOnlySpan<char> path, out ArchiveEntry entryOut, bool hideDeleted = true)
    {
        var index = FindEntryIndex(startAt, path, hideDeleted);
        if (index != -1)
        {
            entryOut = _entries[index];
            return true;
        }
        else
        {
            entryOut = default;
            return false;
        }
    }

    private bool IsEntryNameEqualTo(int entryIdx, in ReadOnlySpan<char> pathSegment)
    {
        var name = GetName(in _entries[entryIdx]);
        return name.Equals(pathSegment, StringComparison.OrdinalIgnoreCase);
    }

    private ReadOnlySpan<char> GetName(in ArchiveEntry entry)
    {
        return _stringHeap.AsSpan(entry.NameStart, entry.NameLength);
    }

    private ReadOnlySpan<char> GetNextPathSegment(ref ReadOnlySpan<char> path)
    {
        // Skip path separators
        path = path.Trim("/\\");

        var nextSeparator = path.IndexOfAny("/\\");
        if (nextSeparator >= 0)
        {
            var segment = path.Slice(0, nextSeparator);
            path = path.Slice(nextSeparator + 1);
            return segment;
        }
        else
        {
            var segment = path;
            path = path.Slice(0, 0);
            return segment;
        }
    }

    private void ReadEntry(BinaryReader reader, ref ArchiveEntry entry)
    {
        var nameLength = reader.ReadInt32();
        if (nameLength < 0 || nameLength > 260)
        {
            throw new Exception($"Expected file name length {nameLength} to be between 0 and 260");
        }

        Span<byte> nameRaw = stackalloc byte[nameLength];
        var nameBytesRead = reader.Read(nameRaw);
        if (nameBytesRead != nameLength)
        {
            throw new Exception($"Read fewer bytes than length of name entry: {nameBytesRead} != {nameLength}");
        }
        if (_stringHeap.Length - _stringHeapUsed < nameLength)
        {
            throw new Exception("Not enough space left in string heap!");
        }

        // Strip trailing null-bytes
        while (!nameRaw.IsEmpty && nameRaw[^1] == 0)
        {
            nameRaw = nameRaw.Slice(0, nameRaw.Length - 1);
        }

        var decoder = Encoding.ASCII.GetDecoder();
        decoder.GetChars(nameRaw, _stringHeap.AsSpan(_stringHeapUsed), true);

        // Save the info we need to recreate the Span within the string heap
        entry.NameStart = _stringHeapUsed;
        entry.NameLength = nameRaw.Length;
        _stringHeapUsed += nameRaw.Length;

        reader.ReadUInt32(); // Skip the crippled name pointer
        var attributes = reader.ReadUInt32();
        entry.IsCompressed = (attributes & 2) == 2;
        entry.IsDirectory = (attributes & 0x400) == 0x400;
        entry.OriginalSize = reader.ReadUInt32();
        entry.SizeOnDisk = reader.ReadUInt32();
        entry.StartOfData = reader.ReadUInt32();
        entry.ParentEntry = reader.ReadInt32();
        entry.FirstChildEntry = reader.ReadInt32();
        entry.NextSiblingEntry = reader.ReadInt32();
    }

    public IMemoryOwner<byte>? ReadFile(string path)
    {
        return !FindEntry(0, path, out var entry) ? null : ReadFile(in entry);
    }

    public IMemoryOwner<byte> ReadFile(in ArchiveEntry entry)
    {
        if (entry.IsDirectory)
        {
            throw new Exception($"Archive entry {GetFullPath(in entry)} is a directory");
        }

        // Uncompressed entries are trivial
        if (!entry.IsCompressed)
        {
            var memory = new ConstrainedMemoryOwner(_pool.Rent((int) entry.SizeOnDisk), (int) entry.SizeOnDisk);
            var data = memory.Memory.Span;
            if (RandomAccess.Read(_fileHandle, data, entry.StartOfData) != data.Length)
            {
                throw new IOException($"Failed to read {data.Length} @ {entry.StartOfData} from {Path}");
            }

            return memory;
        }

        // Compressed data is much harder to handle
        return DecompressEntry(in entry);
    }

    /// <summary>
    /// List the names of files and directories that are directly within the given directory.
    /// This function will NOT recursively list entries.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public List<string> ListDirectory(ReadOnlySpan<char> path)
    {
        if (!FindEntry(0, path, out var entry) || !entry.IsDirectory)
        {
            return new List<string>();
        }

        var result = new List<string>();
        var currentIdx = entry.FirstChildEntry;

        while (currentIdx != -1)
        {
            ref var current = ref _entries[currentIdx];

            if (!current.IsDeleted)
            {
                result.Add(GetName(current).ToString());
            }

            currentIdx = current.NextSiblingEntry;
        }

        return result;
    }

    public bool FileExists(string path)
    {
        return FindEntry(0, path, out var entry) && !entry.IsDirectory;
    }

    public bool DirectoryExists(string path)
    {
        return FindEntry(0, path, out var entry) && entry.IsDirectory;
    }

    /// <summary>
    ///     Decompresses an entry from the archive using the inflate method.
    ///     Returns memory from a memory pool to avoid allocations as much as possible.
    /// </summary>
    private IMemoryOwner<byte> DecompressEntry(in ArchiveEntry entry)
    {
        var compressedSize = (int) entry.SizeOnDisk;
        using var compressedDataOwner = _pool.Rent(compressedSize);
        var compressedData = compressedDataOwner.Memory.Span[..compressedSize];
        var uncompressedSize = (int) entry.OriginalSize;
        var uncompressedDataOwner = new ConstrainedMemoryOwner(_pool.Rent(uncompressedSize), uncompressedSize);
        var uncompressedData = uncompressedDataOwner.Memory.Span[..uncompressedSize];

        if (RandomAccess.Read(_fileHandle, compressedData, entry.StartOfData) != compressedSize)
        {
            throw new IOException($"Couldn't read uncompressed data for {GetFullPath(entry)}");
        }

        try
        {
            Inflate.Uncompress(compressedData, uncompressedData);
        }
        catch (Exception e)
        {
            uncompressedDataOwner.Dispose();
            throw new IOException($"Failed to decompress entry ${GetFullPath(entry)}", e);
        }

        return uncompressedDataOwner;
    }
}