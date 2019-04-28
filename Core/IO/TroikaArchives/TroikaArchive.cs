using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace SpicyTemple.Core.IO.TroikaArchives
{
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

        public bool IsDirectory;

        public bool IsCompressed;
    }

    /// <summary>
    ///     This class allows access to Troika .dat files, which are essentially optimized ZIP files.
    /// </summary>
    public sealed class TroikaArchive : IDisposable
    {
        private static readonly uint SignatureV0 = 0x44_41_54_31; // "DAT1" in little-endian

        private static readonly uint SignatureV1 = 0x44_41_54_20; // "DAT " in little-endian

        private readonly MemoryMappedFile _file;

        /// <summary>
        ///     We have to query the actual on-disk file length because the memory mapped region might end on
        ///     the end of the memory page, rather than ending at the actual end of the file!
        /// </summary>
        private readonly long _fileLength;

        private readonly MemoryPool<byte> _pool = MemoryPool<byte>.Shared;

        private readonly MemoryMappedViewAccessor _view;

        private ArchiveEntry[] _entries;

        private char[] _stringHeap;

        private int _stringHeapUsed;

        public TroikaArchive(string path)
        {
            Path = path;

            _file = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            _view = _file.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
            _fileLength = new FileInfo(path).Length;

            // Read the file table
            try
            {
                ReadFileTable(_file);
            }
            catch
            {
                _view.Dispose();
                _file.Dispose();
                throw;
            }
        }

        public Guid ArchiveGuid { get; private set; }

        public string Path { get; }

        public void Dispose()
        {
            _view.Dispose();
            _file.Dispose();
        }

        private void ReadFileTable(MemoryMappedFile file)
        {
            using (var stream = file.CreateViewStream(0, _fileLength, MemoryMappedFileAccess.Read))
            {
                stream.Seek(-12, SeekOrigin.End);

                var reader = new BinaryReader(stream, Encoding.ASCII, true);

                var signature = reader.ReadUInt32();
                reader.ReadUInt32(); // Skip the string heap, we'll deduce it later

                var fileTableSize = reader.ReadUInt32();

                Debug.Assert(fileTableSize < stream.Length);

                // Seek back to read the archive GUID if it's a newer version of the file format
                if (signature == SignatureV0)
                {
                    ArchiveGuid = Guid.Empty;
                }
                else if (signature == SignatureV1)
                {
                    stream.Seek(-24, SeekOrigin.End);

                    Span<byte> guidData = stackalloc byte[16];
                    ArchiveGuid = new Guid(guidData);
                }
                else
                {
                    throw new Exception("Corrupted header in Troika archive " + Path);
                }

                // Seek to the start of the file allocation table
                stream.Seek(-(int) fileTableSize, SeekOrigin.End);

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
        }

        private string GetFullPath(in ArchiveEntry entry)
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

        private bool FindEntry(int startAt, ReadOnlySpan<char> path, out ArchiveEntry entryOut)
        {
            var nextSegment = GetNextPathSegment(ref path);
            if (nextSegment.IsEmpty)
            {
                entryOut = default;
                return false;
            }

            var currentEntryIdx = startAt;
            while (currentEntryIdx != -1)
            {
                if (IsEntryNameEqualTo(currentEntryIdx, nextSegment))
                {
                    if (path.IsEmpty)
                    {
                        entryOut = _entries[currentEntryIdx];
                        return true;
                    }

                    var firstChild = _entries[currentEntryIdx].FirstChildEntry;
                    if (firstChild != -1)
                    {
                        return FindEntry(firstChild, path, out entryOut);
                    }

                    entryOut = default;
                    return false;
                }

                currentEntryIdx = _entries[currentEntryIdx].NextSiblingEntry;
            }

            entryOut = default;
            return false;
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
            reader.Read(nameRaw);
            if (_stringHeap.Length - _stringHeapUsed < nameLength)
            {
                throw new Exception("Not enough space left in string heap!");
            }

            // Strip trailing null-bytes
            while (!nameRaw.IsEmpty && nameRaw[nameRaw.Length - 1] == 0)
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

        public IMemoryOwner<byte> ReadFile(string path)
        {
            if (!FindEntry(0, path, out var entry))
            {
                return null;
            }

            if (entry.IsDirectory)
            {
                throw new Exception($"Archive entry {path} is a directory");
            }

            // Uncompressed entries are trivial
            if (!entry.IsCompressed)
            {
                return new MappedFileMemoryManager(_view.SafeMemoryMappedViewHandle, entry.StartOfData,
                    entry.SizeOnDisk);
            }

            // Compressed data is much harder to handle
            return DecompressEntry(path, in entry);
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

                result.Add(GetName(current).ToString());

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
        private IMemoryOwner<byte> DecompressEntry(string path, in ArchiveEntry entry)
        {
            var originalSize = (int) entry.OriginalSize;
            var uncompressedData = new ConstrainedMemoryOwner(_pool.Rent(originalSize), originalSize);

            try
            {
                using (var compressedStream = new UnmanagedMemoryStream(_view.SafeMemoryMappedViewHandle,
                    entry.StartOfData, entry.SizeOnDisk, FileAccess.Read))
                {
                    // C#'s DeflateStream operates on raw deflate data, while ToEE
                    // uses the ZLib storage format (detailed in RFC1950).
                    // Essentially deflate has a 2-byte header (when not using dictionaries,
                    // which ToEE doesn't), and a 4-byte Adler32 footer.
                    // The footer is ignored by DeflateStream however, so we only need to skip the header.
                    compressedStream.Seek(2, SeekOrigin.Current);

                    using (var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress, true))
                    {
                        var uncompressedOut = uncompressedData.Memory.Span;
                        var actualRead = deflateStream.Read(uncompressedOut);
                        if (actualRead != uncompressedOut.Length)
                        {
                            throw new Exception(
                                $"Failed to read {uncompressedOut.Length} bytes for {path}, only got {actualRead}"
                            );
                        }

                        // check that we are actually at EOF
                        Span<byte> tmp = stackalloc byte[1];
                        if (deflateStream.Read(tmp) != 0)
                        {
                            throw new Exception(
                                $"The decompressed stream for {path} is longer than the {uncompressedOut.Length} bytes indicated in the archive!"
                            );
                        }
                    }
                }
            }
            catch
            {
                uncompressedData.Dispose();
                throw;
            }

            return uncompressedData;
        }
    }

    public unsafe class MappedFileMemoryManager : MemoryManager<byte>
    {
        private readonly int _length;

        private readonly SafeMemoryMappedViewHandle _viewHandle;

        private byte* _data = null;

        public MappedFileMemoryManager(SafeMemoryMappedViewHandle viewHandle, uint offset, uint length)
        {
            _viewHandle = viewHandle;

            if (viewHandle.ByteLength < offset + length)
            {
                throw new ArgumentException(
                    $"The file has length {viewHandle.ByteLength}, but requesting range {offset} to {offset + length}"
                );
            }

            // Span takes only an int in it's constructor, so we need to bounds check
            if (length > int.MaxValue)
            {
                throw new ArgumentException($"Length {length} cannot be > {int.MaxValue}");
            }

            _length = (int) length;

            // Acquire the raw pointer to the underlying view, this increments
            // the internal ref-count for the mapped view
            viewHandle.AcquirePointer(ref _data);

            if (_data == null)
            {
                throw new Exception("Failed to obtain a pointer to the underlying memory mapped file");
            }

            _data += offset;
        }

        public Stream CreateStream()
        {
            return new UnmanagedMemoryStream(_data, _length, _length, FileAccess.Read);
        }

        public override Span<byte> GetSpan()
        {
            if (_data == null)
            {
                throw new InvalidOperationException("The underlying memory has already been released.");
            }

            return new Span<byte>(_data, _length);
        }

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            if (_data == null)
            {
                throw new InvalidOperationException("The underlying memory has already been released.");
            }

            if (elementIndex < 0 || elementIndex >= _length)
            {
                throw new ArgumentException("elementIndex was out of range.", nameof(elementIndex));
            }

            // Memory mapped files are unmanaged resources and thus don't need pinning
            return new MemoryHandle(_data + elementIndex);
        }

        public override void Unpin()
        {
            // The underlying pointer is in unmanaged memory, so there's nothing to unpin.
        }

        protected override void Dispose(bool disposing)
        {
            if (_data != null)
            {
                _viewHandle.ReleasePointer();
                _data = null;
            }
        }
    }
}