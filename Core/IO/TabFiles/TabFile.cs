using System;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.IO.TabFiles
{
    /**
     * Parses tab delimited files such as protos.tab or partsys1.tab.
     */
    public class TabFile
    {
        // We will allow no more than this number of columns
        private const int MaxColumns = 1000;

        public delegate void Callback(TabFileRecord record);

        public static void ParseFile(
            IFileSystem fs,
            string filename,
            Callback callback
        )
        {
            var content = fs.ReadBinaryFile(filename);
            ParseSpan(content, callback);
        }

        public static void ParseFile(
            string filename,
            Callback callback
        ) => ParseFile(Tig.FS, filename, callback);

        public static void ParseSpan(
            ReadOnlySpan<byte> content,
            Callback callback
        )
        {
            Span<int> columnRanges = stackalloc int[MaxColumns * 2];
            var lineNumber = 1;

            var reader = new LineReader(content);
            while (reader.NextLine())
            {
                var line = reader.GetLine();

                var columnCount = FindRangesInLine(line, (byte) '\t', columnRanges,
                    false, true);

                // Post-Process the columns
                for (int i = 0; i < columnCount; i++)
                {
                    PostProcessColumn(line, ref columnRanges[i * 2], ref columnRanges[i * 2 + 1]);
                }

                var actualRanges = columnRanges.Slice(0, columnCount * 2);
                var record = new TabFileRecord(line, actualRanges, lineNumber++);
                callback(record);
            }
        }

        private static void PostProcessColumn(ReadOnlySpan<byte> line, ref int start, ref int length)
        {
            var column = line.Slice(start, length);

            // ToEE will remove trailing vertical tabs and spaces
            for (int i = length - 1; i >= 0; i--) {
                var ch = column[i];
                // Vertical tabs and spaces will be trimmed
                if (ch != ' ' && ch != '\x0b') {
                    break;
                }
                length--;
            }
        }

        /// <summary>
        /// Find ranges of items in a line separated by delimiters. A range consists of a start index
        /// and a length. The given span of ranges will receive twice as many items as there are ranges.
        /// The number of ranges is returned by this function.
        /// </summary>
        public static int FindRangesInLine(ReadOnlySpan<byte> s, byte delim, Span<int> ranges, bool trimItems,
            bool keepEmpty, byte secondaryDelim = 0)
        {
            var rangesCount = 0;

            var pos = 0;
            while (pos < s.Length)
            {
                var ch = s[pos++];

                // Empty item
                if (ch == delim || ch == secondaryDelim)
                {
                    if (keepEmpty)
                    {
                        ranges[rangesCount * 2] = pos;
                        ranges[rangesCount * 2 + 1] = 0;
                        rangesCount++;
                    }

                    continue;
                }

                if (trimItems && char.IsWhiteSpace((char) ch))
                {
                    // Skip all chars that are considered whitespace
                    continue;
                }

                var start = pos - 1; // Start of item
                var count = 1; // Size of item

                // Seek past all of the item's content
                while (pos < s.Length)
                {
                    ch = s[pos++];
                    if (ch == delim || ch == secondaryDelim)
                    {
                        break; // reached the end of the item
                    }

                    count++;
                }

                // Trim whitespace at the end of the string
                if (trimItems)
                {
                    while (count > 0 && char.IsWhiteSpace((char) s[start + count - 1]))
                    {
                        count--;
                    }
                }

                if (count > 0 || keepEmpty)
                {
                    ranges[rangesCount * 2] = start;
                    ranges[rangesCount * 2 + 1] = count;
                    rangesCount++;
                }
            }

            return rangesCount;
        }
    }
}