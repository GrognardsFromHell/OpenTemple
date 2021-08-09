using System;
using System.IO;

namespace OpenTemple.Tests.TestUtils
{
    public class TempDirectory : IDisposable
    {
        public string Path { get; }

        public TempDirectory()
        {
            var randomFolderName = System.IO.Path.GetRandomFileName();
            Path = System.IO.Path.Join(System.IO.Path.GetTempPath(), randomFolderName);
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            Directory.Delete(Path, true);
        }
    }
}