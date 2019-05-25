using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.IO.TroikaArchives
{
    public sealed class TroikaVfs : IFileSystem, IDisposable
    {
        private readonly List<TroikaArchive> _archives = new List<TroikaArchive>();

        private readonly List<string> _dataDirs = new List<string>();

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

            vfs.AddArchive(Path.Join(path, @"Modules\ToEE.dat"));
            vfs.AddArchive(Path.Join(path, @"ToEE4.dat"));
            vfs.AddArchive(Path.Join(path, @"ToEE3.dat"));
            vfs.AddArchive(Path.Join(path, @"ToEE2.dat"));
            vfs.AddArchive(Path.Join(path, @"ToEE1.dat"));
            vfs.AddArchive(Path.Join(path, @"tig.dat"));

            return vfs;
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
                        result.Add(Path.GetFileName(fullPath));
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
            using (var content = ReadFile(path))
            {
                return Encoding.ASCII.GetString(content.Memory.Span);
            }
        }

        public BinaryReader OpenBinaryReader(string path)
        {
            return new BinaryReader(new MemoryStream(ReadBinaryFile(path)));
        }

        public byte[] ReadBinaryFile(string path)
        {
            using (var memory = ReadFile(path))
            {
                return memory.Memory.ToArray();
            }
        }

        public IMemoryOwner<byte> ReadFile(string path)
        {
            // Check the data directories first
            foreach (var dataDir in _dataDirs)
            {
                var fullPath = Path.Join(dataDir, path);
                if (File.Exists(fullPath))
                {
                    using (var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                    {
                        var buffer = _pool.Rent((int) stream.Length);
                        stream.Read(buffer.Memory.Span);
                        return new ConstrainedMemoryOwner(buffer, (int) stream.Length);
                    }
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

        public void AddDataDir(string path)
        {
            _dataDirs.Add(path);
        }

        public void AddArchive(string path)
        {
            _archives.Add(new TroikaArchive(path));
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

        public Memory<byte> Memory => _delegateTo.Memory.Slice(0, _length);
    }

}