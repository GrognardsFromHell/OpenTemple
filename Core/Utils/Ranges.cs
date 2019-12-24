using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenTemple.Core.Utils
{

    public static class RangeExtensions
    {

        public static bool Contains(this Range range, int value)
        {
            Trace.Assert(!range.Start.IsFromEnd);
            Trace.Assert(!range.End.IsFromEnd);
            return value >= range.Start.Value && value < range.End.Value;
        }

        public static void Add(this ICollection<int> list, Range range)
        {
            Trace.Assert(!range.Start.IsFromEnd);
            Trace.Assert(!range.End.IsFromEnd);
            for (int i = range.Start.Value; i < range.End.Value; i++)
            {
                list.Add(i);
            }
        }

    }

}