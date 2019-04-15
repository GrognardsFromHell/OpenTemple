using System.Collections.Generic;
using SpicyTemple.Core.IO.MesFiles;

namespace SpicyTemple.Core.IO.TroikaArchives
{
    /// <summary>
    /// Helper functions that wrap simple file readers for use with a Vfs.
    /// </summary>
    public static class TroikaVfsExtensions
    {

        public static Dictionary<int, string> ReadMesFile(this TroikaVfs vfs, string path)
        {
            using (var content = vfs.ReadFile(path))
            {
                return MesFile.Read(path, content.Memory.Span);
            }
        }

    }
}
