using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenTemple.Core.IO.TroikaArchives;

namespace ExtractTroikaArchive
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = args[0];
            var output = args[1];
            if (!output.EndsWith("/") && !output.EndsWith("\\"))
            {
                output += "/";
            }

            using var archive = new TroikaArchive(path);

            // Precreate all directories
            var entries = archive.Entries.ToArray();
            foreach (var entry in entries)
            {
                if (entry.IsDirectory)
                {
                    var fullPath = output + archive.GetFullPath(in entry);
                    Directory.CreateDirectory(fullPath);
                }
            }

            Parallel.ForEach(
                entries.Where(entry => !entry.IsDirectory),
                entry =>
                {
                    var fullPath = output + archive.GetFullPath(in entry);
                    using var data = archive.ReadFile(in entry);
                    File.WriteAllBytes(fullPath, data.Memory.ToArray());
                }
            );
        }
    }
}