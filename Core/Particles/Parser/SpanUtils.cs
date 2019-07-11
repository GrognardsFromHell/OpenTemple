using System;
using System.Buffers;

namespace SpicyTemple.Core.Particles.Parser
{
    public static class SpanUtils
    {
        private static readonly MemoryPool<Range> Pool = MemoryPool<Range>.Shared;

        public static IMemoryOwner<Range> SplitList(ReadOnlySpan<byte> text, byte separator, out int rangeCount)
        {
            // 1st Pass: Count the number of elements
            int count = 1;
            foreach (var b in text)
            {
                if (b == separator)
                {
                    count++;
                }
            }

            rangeCount = count;

            // 2nd Pass: Determine ranges
            var rangesOwner = Pool.Rent(count);
            var ranges = rangesOwner.Memory.Span;

            var currentStart = 0;
            for (int i = 0; i < count - 1; i++)
            {
                var next = currentStart;
                while (text[next++] != separator)
                {
                }

                ranges[i] = new Range(currentStart, next - 1); // Don't include the sep
                currentStart = next; // Skip the sep
            }

            // Add the last element until the end of the string
            ranges[count - 1] = new Range(currentStart, text.Length);

            // Trim whitespace from all ranges
            for (var i = 0; i < count; i++)
            {
                // Trim the end
                int whitespaces = 0;
                for (int j = ranges[i].End.Value - 1; j >= ranges[i].Start.Value; j--)
                {
                    if (IsWhitespace(text[j]))
                    {
                        whitespaces++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (whitespaces > 0)
                {
                    ranges[i] = new Range(ranges[i].Start, ranges[i].End.Value - whitespaces);
                }

                // Trim the start
                whitespaces = 0;
                for (int j = ranges[i].Start.Value; j < ranges[i].End.Value; j++)
                {
                    if (IsWhitespace(text[j]))
                    {
                        whitespaces++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (whitespaces > 0)
                {
                    ranges[i] = new Range(ranges[i].Start.Value + whitespaces, ranges[i].End);
                }
            }

            if (ranges[count - 1].End.Value == ranges[count - 1].Start.Value)
            {
                rangeCount--;
            }

            return rangesOwner;
        }

        private static bool IsWhitespace(byte ch)
        {
            return ch == ' ' || ch == 11 /* vertical tab */;
        }
    }
}