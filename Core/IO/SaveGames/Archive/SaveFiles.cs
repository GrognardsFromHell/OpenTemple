using System;
using System.IO;

namespace OpenTemple.Core.IO.SaveGames.Archive
{
    public static class ExtractSaveArchive
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1 && args.Length != 2)
            {
                Console.WriteLine("Usage: ExtractSaveGame <file> [targetDir]");
                Console.WriteLine("Both the tfai and tfaf file can be passed to this utility.");
                return;
            }

            var saveFile = args[0];

            var targetDir = @"Save\Current";
            if (args.Length == 2)
            {
                targetDir = args[1];
            }

            Directory.CreateDirectory(targetDir);

            var tfaiFile = Path.ChangeExtension(saveFile, ".tfai");
            var tfafFile = Path.ChangeExtension(saveFile, ".tfaf");

            if (!File.Exists(tfaiFile))
            {
                Console.WriteLine(tfaiFile + " does not exist!");
                return;
            }

            if (!File.Exists(tfafFile))
            {
                Console.WriteLine(tfafFile + " does not exist!");
                return;
            }

            Extract(tfaiFile, tfafFile, targetDir);

            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
        }

        private static void Extract(string tfaiFile, string tfafFile, string targetDir)
        {
            Console.WriteLine("Extracting to {0}", targetDir);

            // Read the index file first
            var entries = ArchiveIndexReader.ReadIndex(tfaiFile);

            using (var stream = new FileStream(tfafFile, FileMode.Open))
            {
                foreach (var entry in entries)
                {
                    Console.Write("Extracting {0}...", entry.Path);

                    var fullPath = Path.Combine(targetDir, entry.Path);
                    if (entry.Directory)
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                    else
                    {
                        using (var outStream = new FileStream(fullPath, FileMode.Create))
                        {
                            byte[] buffer = new byte[entry.Size];
                            stream.Read(buffer, 0, buffer.Length);
                            outStream.Write(buffer, 0, buffer.Length);
                        }
                    }

                    Console.WriteLine();
                }
            }
        }
    }
}