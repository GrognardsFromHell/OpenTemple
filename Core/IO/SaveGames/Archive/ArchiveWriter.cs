using System;
using System.IO;

namespace OpenTemple.Core.IO.SaveGames.Archive
{
    public static class ArchiveWriter
    {             

        public static void Compress(string indexFile, string dataFile, string directoryPath)
        {

            using (Stream indexStream = new FileStream(indexFile, FileMode.Create),
                 dataStream = new FileStream(dataFile, FileMode.Create)) 
            {
                var indexWriter = new BinaryWriter(indexStream);

                WriteDirectoryContent(indexWriter, dataStream, directoryPath);                

                indexWriter.Write(3);

            }

        }

        private static void WriteDirectoryContent(BinaryWriter indexWriter, Stream dataStream, string path)
        {
            var allFiles = Directory.EnumerateFileSystemEntries(path, "*.*", SearchOption.TopDirectoryOnly);

            foreach (var filePath in allFiles)
            {
                var name = Path.GetFileName(filePath);

                if (Directory.Exists(filePath))
                {
                    Console.WriteLine("Writing directory {0}", filePath);
                    indexWriter.Write(1);
                    indexWriter.WritePrefixedString(name);
                    WriteDirectoryContent(indexWriter, dataStream, filePath);
                    indexWriter.Write(2);
                }
                else if (File.Exists(filePath))
                {
                    Console.WriteLine("Writing file {0}", filePath);
                    indexWriter.Write(0);
                    indexWriter.WritePrefixedString(name);
                    using (var inStream = new FileStream(filePath, FileMode.Open))
                    {
                        indexWriter.Write((int)inStream.Length);
                        inStream.CopyTo(dataStream);
                    }
                }
            }
        }

    }
}
