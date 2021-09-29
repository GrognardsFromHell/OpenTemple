using System;
using System.IO;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.TroikaArchives;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Tests.TestUtils
{
    /// <summary>
    /// This is a test fixture for tests that depend on an actual ToEE installation directory.
    /// These are mostly used to test the use of certain classes against all data found in a
    /// normal ToEE installation.
    /// </summary>
    public class RealGameFiles : IDisposable
    {
        private IFileSystem _oldVfs;

        protected TroikaVfs FS;

        public RealGameFiles()
        {
            _oldVfs = Tig.FS;
            var toeeDir = Environment.GetEnvironmentVariable("TOEE_DIR");
            if (toeeDir == null)
            {
                throw new NotSupportedException(
                    "Cannot run a test based on real data because TOEE_DIR environment variable is not set."
                );
            }

            var dataFolder = Path.Join(TestData.SolutionDir, "Data");
            FS = (TroikaVfs) Tig.CreateFileSystem(toeeDir, dataFolder);
            Tig.FS = FS;
        }

        public virtual void Dispose()
        {
            if (FS != null)
            {
                FS.Dispose();
                if (Tig.FS == FS)
                {
                    Tig.FS = _oldVfs;
                }
                _oldVfs = null;
                FS = null;
            }
        }
    }
}
