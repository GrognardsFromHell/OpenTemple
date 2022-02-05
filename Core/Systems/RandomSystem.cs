using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.Systems;

public class RandomSystem : IGameSystem
{
    private static Random _random = new ();

    public void Dispose()
    {
    }

    [TempleDllLocation(0x10038DF0)]
    public int GetInt(int fromInclusive, int toInclusive)
    {
        return _random.Next(fromInclusive, toInclusive + 1);
    }

    public bool GetBool() => GetInt(0, 1) == 1;

    public T PickRandom<T>(IReadOnlyList<T> collection)
    {
        Trace.Assert(collection.Count > 0);
        var idx = _random.Next(0, collection.Count);
        return collection[idx];
    }

    [TempleDllLocation(0x10038db0)]
    public void SetSeed(int seed)
    {
        _random = new Random(seed);
    }

    [TempleDllLocation(0x10038dc0)]
    public int SetRandomSeed()
    {
        var seed = (int) TimePoint.Now.Milliseconds;
        SetSeed(seed);
        return seed;
    }

}