using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;

namespace SpicyTemple.Core.Systems.Raycast
{
    public class RaycastPacket : IDisposable, IReadOnlyList<RaycastResultItem>
    {
        public RaycastFlag flags;
        public int field4;
        public LocAndOffsets origin;
        public LocAndOffsets targetLoc;
        public float radius;
        public int field2C;
        public GameObjectBody sourceObj;
        public GameObjectBody target;
        public float rayRangeInches; // limits the distance from the origin
        public List<RaycastResultItem> results;

        [TempleDllLocation(0x100babb0)]
        public RaycastPacket()
        {
            flags = RaycastFlag.HasToBeCleared;
            results = new List<RaycastResultItem>();
        }

        [TempleDllLocation(0x100bace0)]
        public int Raycast()
        {
            Stub.TODO();
            return 0;
        }

        [TempleDllLocation(0x100bc750)]
        public int RaycastShortRange()
        {
            Stub.TODO();
            return 0;
        }

        [TempleDllLocation(0x100BABE0)]
        public void Dispose()
        {
            flags &= RaycastFlag.HasToBeCleared;
            if (results != null)
            {
                results.Clear();
                results = null;
            }
        }

        public IEnumerable<GameObjectBody> EnumerateObjects()
        {
            foreach (var resultItem in results)
            {
                yield return resultItem.obj;
            }
        }

        public IEnumerator<RaycastResultItem> GetEnumerator() => results.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => results.Count;

        public RaycastResultItem this[int index] => results[index];
    }
}