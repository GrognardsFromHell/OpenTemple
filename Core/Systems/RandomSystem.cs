using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Systems
{
    public class RandomSystem : IGameSystem
    {
        private static readonly Random _random = new Random();

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
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10038dc0)]
        public int SetRandomSeed()
        {
            var seed = (int) TimePoint.Now.Milliseconds;
            SetSeed(seed);
            return seed;
        }

    }
}