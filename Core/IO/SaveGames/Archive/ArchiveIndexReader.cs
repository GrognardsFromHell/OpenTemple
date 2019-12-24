using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenTemple.Core.IO.SaveGames.Archive
{

    enum ArchiveIndexEntryType
    {
        FILE = 0,
        DIRECTORY = 1,        
        END_OF_DIRECTORY = 2,
        END_OF_FILE = 3
    }
 
    public static class ArchiveIndexReader
    {

        public static IList<ArchiveIndexEntry> ReadIndex(string filename)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                return ReadIndex(stream);
            }
        }

        public static IList<ArchiveIndexEntry> ReadIndex(Stream stream)
        {
            // Read fully into memory since the files are miniscule
            var bufferedStream = new BufferedStream(stream, 32 * 1024);
            return ReadIndex(new BinaryReader(bufferedStream, Encoding.ASCII, true));
        }

        public static IList<ArchiveIndexEntry> ReadIndex(BinaryReader reader)
        {
            var result = new List<ArchiveIndexEntry>();
            var entryType = (ArchiveIndexEntryType) reader.ReadUInt32();
            var pathStack = new List<string>();
            string path = "";

            // Entry type 3 denotes end of index file
            for (; entryType != ArchiveIndexEntryType.END_OF_FILE; entryType = (ArchiveIndexEntryType) reader.ReadUInt32())
            {

                switch (entryType)
                {
                    case ArchiveIndexEntryType.DIRECTORY:
                        result.Add(ReadDirectory(reader, path, pathStack));
                        path = string.Join(@"\", pathStack);
                        break;
                    case ArchiveIndexEntryType.FILE:
                        result.Add(ReadFile(reader, path));
                        break;
                    case ArchiveIndexEntryType.END_OF_DIRECTORY:
                        if (pathStack.Count == 0)
                        {
                            throw new InvalidDataException("Cannot end directory that was never started.");
                        }
                        pathStack.RemoveAt(pathStack.Count - 1);
                        path = string.Join(@"\", pathStack);
                        break;
                }

            }

            return result.ToArray();
        }

        private static ArchiveIndexEntry ReadFile(BinaryReader reader, string path)
        {
            var name = reader.ReadPrefixedString();
            var size = reader.ReadInt32();

            return new ArchiveIndexEntry()
            {
                Directory = false,
                Name = name,
                ParentPath = path,
                Size = size
            };
        }

        private static ArchiveIndexEntry ReadDirectory(BinaryReader reader, string path, IList<string> pathStack)
        {
            var name = reader.ReadPrefixedString();
            pathStack.Add(name);

            return new ArchiveIndexEntry()
            {
                Directory = true,
                Name = name,
                ParentPath = path
            };
        }

    }

}
