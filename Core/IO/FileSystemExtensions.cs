using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpicyTemple.Core.IO.MesFiles;

namespace SpicyTemple.Core.IO
{
    /// <summary>
    /// Helper functions that wrap simple file readers for use with a Vfs.
    /// </summary>
    public static class FileSystemExtensions
    {
        private static readonly char[] DirSeparators = {'/', '\\'};

        public static Dictionary<int, string> ReadMesFile(this IFileSystem vfs, string path)
        {
            using var content = vfs.ReadFile(path);
            return MesFile.Read(path, content.Memory.Span);
        }

        /**
         * Searches for patterns such as directory1\directory2\*.txt.
         */
        public static IEnumerable<string> Search(this IFileSystem fs, string searchPattern)
        {
            var lastIdxOfSep = searchPattern.LastIndexOfAny(DirSeparators);

            var indexOfWildcard = searchPattern.IndexOf('*');
            if (indexOfWildcard == -1)
            {
                // No wildcard means it can only result in a single entry anyway
                if (fs.DirectoryExists(searchPattern) || fs.FileExists(searchPattern))
                {
                    yield return searchPattern;
                }

                yield break;
            }

            if (searchPattern.LastIndexOf('*') != indexOfWildcard)
            {
                throw new ArgumentException("Search only supports a single wildcard.");
            }

            if (indexOfWildcard < lastIdxOfSep)
            {
                throw new ArgumentException("Search only supports wildcards in the filename portion.");
            }

            var directoryPath = lastIdxOfSep != -1
                ? searchPattern.Substring(0, lastIdxOfSep)
                : "";
            var filenamePart = lastIdxOfSep != -1
                ? searchPattern.Substring(lastIdxOfSep + 1)
                : searchPattern;
            indexOfWildcard = filenamePart.IndexOf('*');
            Trace.Assert(indexOfWildcard != -1);

            // Extract the prefix/suffix of the pattern as spans
            var prefix = filenamePart.Substring(0, indexOfWildcard);
            var suffix = filenamePart.Substring(indexOfWildcard + 1);

            foreach (var filename in fs.ListDirectory(directoryPath))
            {
                if (filename.Length < prefix.Length + suffix.Length)
                {
                    continue; // Can't match
                }

                if (filename.StartsWith(prefix) && filename.EndsWith(suffix))
                {
                    yield return filename;
                }
            }
        }

    }
}