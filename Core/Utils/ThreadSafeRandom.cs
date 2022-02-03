using System;
using System.Threading;

namespace OpenTemple.Core.Utils;

internal static class ThreadSafeRandom
{
    private static readonly ThreadLocal<Random> LocalRandom = new(() => new Random());

    /// <summary>
    /// Reinitializes the random number generator for the current thread and sets it to the given seed.
    /// </summary>
    public static int Seed
    {
        set => LocalRandom.Value = new Random(value);
    }

    public static Random Random => LocalRandom.Value;

    /// <inheritdoc cref="System.Random.Next()"/>
    public static int Next() => Random.Next();

    /// <inheritdoc cref="System.Random.Next(int)"/>
    public static int Next(int maxValue) => Random.Next(maxValue);

    /// <inheritdoc cref="System.Random.Next(int, int)"/>
    public static int Next(int minValue, int maxValue) => Random.Next(minValue, maxValue);

    /// <inheritdoc cref="System.Random.NextDouble()"/>
    public static double NextDouble() => Random.NextDouble();
}