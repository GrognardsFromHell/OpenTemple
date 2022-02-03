using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenTemple.Core.Utils;

public static class ListExtensions
{
    /// <summary>
    /// Shuffles a list. Vanilla has this algorithm in 0x10087220, but it's generally known as:
    /// https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle.
    /// Vanilla implemented the naive version.
    /// </summary>
    public static void Shuffle<T>(this Span<T> list)
    {
        var random = ThreadSafeRandom.Random;

        for (var i = list.Length - 1; i > 0; i--)
        {
            var j = random.Next(i); // j is [0, i)

            // Swap the two elements
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}