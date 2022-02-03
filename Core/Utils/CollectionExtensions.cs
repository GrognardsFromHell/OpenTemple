using System;
using System.Collections.Generic;

namespace OpenTemple.Core.Utils;

public static class CollectionExtensions
{
    /// <summary>
    /// Dispose all elements in the collection and clear it.
    /// </summary>
    public static void DisposeAndClear<T>(this ICollection<T> collection) where T : IDisposable
    {
        foreach (var item in collection)
        {
            item?.Dispose();
        }

        collection.Clear();
    }

    /// <summary>
    /// Dispose all elements in the collection and set them to null.
    /// </summary>
    public static void DisposeAndNull<T>(this IList<T> collection) where T : class, IDisposable
    {
        for (var i = 0; i < collection.Count; i++)
        {
            collection[i]?.Dispose();
            collection[i] = null;
        }
    }

    /// <summary>
    /// Dispose all elements in the enumeration.
    /// </summary>
    public static void DisposeAll<T>(this IEnumerable<T> enumerable) where T : IDisposable
    {
        foreach (var item in enumerable)
        {
            item.Dispose();
        }
    }
}