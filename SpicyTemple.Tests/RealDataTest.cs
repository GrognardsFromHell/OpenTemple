using System;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.TroikaArchives;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Tests
{
    /// <summary>
    /// This is a test fixture for tests that depend on an actual ToEE installation directory.
    /// These are mostly used to test the use of certain classes against all data found in a
    /// normal ToEE installation.
    /// </summary>
    public class RealDataTest : IDisposable
    {
        private IFileSystem _oldVfs;

        private TroikaVfs _vfs;

        public RealDataTest()
        {
            _oldVfs = Tig.FS;
            var toeeDir = Environment.GetEnvironmentVariable("TOEE_DIR");
            if (toeeDir == null)
            {
                throw new NotSupportedException(
                    "Cannot run a test based on real data because TOEE_DIR environment variable is not set."
                );
            }

            _vfs = TroikaVfs.CreateFromInstallationDir(toeeDir);
            Tig.FS = _vfs;
        }

        public void Dispose()
        {
            _vfs.Dispose();
            Tig.FS = _oldVfs;
        }
    }
}
