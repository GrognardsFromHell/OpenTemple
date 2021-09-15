using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using OpenTemple.Core.Utils;

#nullable enable

namespace OpenTemple.Core.IO.TroikaArchives
{
    public sealed class TroikaVfs : IFileSystem, IDisposable
    {
        private readonly List<TroikaArchive> _archives = new();

        private readonly List<string> _dataDirs = new ();

        private readonly MemoryPool<byte> _pool = MemoryPool<byte>.Shared;

        public void Dispose()
        {
            _archives.DisposeAndClear();
        }

        public static TroikaVfs CreateFromInstallationDir(string path)
        {
            var dataDir = Path.Join(path, "data");
            var moduleDataDir = Path.Join(path, @"Modules\ToEE");

            var vfs = new TroikaVfs();
            vfs.AddDataDir(moduleDataDir);
            vfs.AddDataDir(dataDir);

            var moduleArchive = vfs.AddArchive(Path.Join(path, @"Modules\ToEE.dat"));
            IgnoreIncorrectSoundSchemes(moduleArchive);

            vfs.AddArchive(Path.Join(path, @"ToEE4.dat"));
            vfs.AddArchive(Path.Join(path, @"ToEE3.dat"));
            vfs.AddArchive(Path.Join(path, @"ToEE2.dat"));
            vfs.AddArchive(Path.Join(path, @"ToEE1.dat"));
            vfs.AddArchive(Path.Join(path, @"tig.dat"));

            return vfs;
        }

        private static void IgnoreIncorrectSoundSchemes(TroikaArchive archive)
        {
            // These files are actually not correct and not used in Vanilla. Due to how vanilla loads data,
            // they are only picked up from ToEE[1-4].dat, and the data in Modules\ToEE.dat is never used.
            // Due to OT simplifying how archives are loaded, our game system load routine will pick up
            // the files from Modules\ToEE.dat, which are old files from Arkanum, and not ToEE.
            archive.SetDeleted("sound/schemeindex.mes");
            archive.SetDeleted("sound/schemelist.mes");
        }

        /// <summary>
        /// List direct descendants of a directory.
        /// </summary>
        /// <returns></returns>
        public ISet<string> ListDirectory(string path)
        {
            var result = new HashSet<string>();

            // Check the data directories first
            foreach (var dataDir in _dataDirs)
            {
                var fullPath = Path.Join(dataDir, path);
                if (Directory.Exists(fullPath))
                {
                    foreach (var item in Directory.GetFileSystemEntries(fullPath))
                    {
                        result.Add(Path.GetFileName(item));
                    }
                }
            }

            foreach (var archive in _archives)
            {
                foreach (var entry in archive.ListDirectory(path))
                {
                    result.Add(entry);
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if a given file exists.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>True if the given path exists, false otherwise.</returns>
        public bool FileExists(string path)
        {
            // Check the data directories first
            foreach (var dataDir in _dataDirs)
            {
                var fullPath = Path.Join(dataDir, path);
                if (File.Exists(fullPath))
                {
                    return true;
                }
            }

            foreach (var archive in _archives)
            {
                if (archive.FileExists(path))
                {
                    return true;
                }
            }

            return false;
        }

        public bool DirectoryExists(string path)
        {
            // Check the data directories first
            foreach (var dataDir in _dataDirs)
            {
                var fullPath = Path.Join(dataDir, path);
                if (Directory.Exists(fullPath))
                {
                    return true;
                }
            }

            foreach (var archive in _archives)
            {
                if (archive.DirectoryExists(path))
                {
                    return true;
                }
            }

            return false;
        }

        public string ReadTextFile(string path)
        {
            using var content = ReadFile(path);
            return Encoding.ASCII.GetString(content.Memory.Span);
        }

        public BinaryReader OpenBinaryReader(string path)
        {
            return new BinaryReader(new MemoryStream(ReadBinaryFile(path)));
        }

        public TextReader OpenTextReader(string path, Encoding encoding)
        {
            return new StreamReader(new MemoryStream(ReadBinaryFile(path)), encoding);
        }

        public byte[] ReadBinaryFile(string path)
        {
            using var memory = ReadFile(path);
            if (memory == null)
            {
                throw new FileNotFoundException(path);
            }
            return memory.Memory.ToArray();
        }

        public IMemoryOwner<byte> ReadFile(string path)
        {
            var result = ReadOptionalFile(path);
            if (result == null)
            {
                throw new FileNotFoundException(path);
            }
            return result;
        }

        public byte[]? ReadOptionalBinaryFile(string path)
        {
            using var memory = ReadOptionalFile(path);
            return memory?.Memory.ToArray();
        }

        public IMemoryOwner<byte> ReadOptionalFile(string path)
        {
            // Check the data directories first
            foreach (var dataDir in _dataDirs)
            {
                var fullPath = Path.Join(dataDir, path);
                if (File.Exists(fullPath))
                {
                    using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                    var buffer = _pool.Rent((int) stream.Length);
                    stream.Read(buffer.Memory.Span);
                    return new ConstrainedMemoryOwner(buffer, (int) stream.Length);
                }
            }

            foreach (var archive in _archives)
            {
                var result = archive.ReadFile(path);
                if (result != null)
                {
                    return result;
                }
            }

            throw new FileNotFoundException(path);
        }

        public bool TryGetRealPath(string path, [NotNullWhen(true)] out string? realPath)
        {
            foreach (var dataDir in _dataDirs)
            {
                var fullPath = Path.Join(dataDir, path);
                if (File.Exists(fullPath))
                {
                    realPath = fullPath;
                    return true;
                }
            }

            realPath = null;
            return false;
        }

        public void AddDataDir(string path, bool prepend = false)
        {
            if (prepend)
            {
                _dataDirs.Insert(0, path);
            }
            else
            {
                _dataDirs.Add(path);
            }
        }

        public TroikaArchive AddArchive(string path)
        {
            var archive = new TroikaArchive(path);
            _archives.Add(archive);
            return archive;
        }

        /// <summary>
        /// Mark the given files as excluded for the given archive. Can be used to prevent files from a specific
        /// archive from interfering with other files.
        /// </summary>
        /// <param name="archiveGuid">The Troika Archive GUID found in the .dat file header (or rather, footer).</param>
        /// <param name="files">The list of files or folders to hide.</param>
        public void HideArchiveContent(Guid archiveGuid, IReadOnlyList<string> files)
        {
            foreach (var archive in _archives)
            {
                if (archive.ArchiveGuid == archiveGuid)
                {
                    foreach (var file in files)
                    {
                        archive.SetDeleted(file);
                    }
                }
            }
        }
    }

    public sealed class ConstrainedMemoryOwner : IMemoryOwner<byte>
    {
        private readonly IMemoryOwner<byte> _delegateTo;

        private readonly int _length;

        public ConstrainedMemoryOwner(IMemoryOwner<byte> delegateTo, int length)
        {
            _delegateTo = delegateTo;
            _length = length;
        }

        public void Dispose()
        {
            _delegateTo.Dispose();
        }

        public Memory<byte> Memory => _delegateTo.Memory[.._length];
    }

}
