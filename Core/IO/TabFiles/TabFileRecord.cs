using System;
using System.Diagnostics;

namespace SpicyTemple.Core.IO.TabFiles
{
    public ref struct TabFileRecord
    {
        public int LineNumber { get; }

        public int ColumnCount => _ranges.Length / 2;

        internal TabFileRecord(ReadOnlySpan<byte> line, Span<int> ranges, int lineNumber)
        {
            Trace.Assert(ranges.Length % 2 == 0);
            _line = line;
            _ranges = ranges;
            LineNumber = lineNumber;
        }

        public TabFileColumn this[int i]
        {
            get
            {
                var rangeIndex = 2 * i;

                if (rangeIndex + 1 >= _ranges.Length)
                {
                    return new TabFileColumn(LineNumber, i, -1, Span<byte>.Empty);
                }

                var start = _ranges[rangeIndex];
                var length = _ranges[rangeIndex + 1];
                var column = _line.Slice(start, length);
                return new TabFileColumn(LineNumber, i, start, column);
            }
        }

        private readonly ReadOnlySpan<byte> _line;
        private readonly ReadOnlySpan<int> _ranges;
    };
}