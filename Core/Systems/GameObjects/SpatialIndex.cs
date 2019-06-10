using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.Remoting;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.MapSector;

namespace SpicyTemple.Core.Systems.GameObjects
{
    public class SpatialIndex
    {

        [TempleDllLocation(0x100c10d0)]
        public SpatialIndex()
        {
        }

        [TempleDllLocation(0x100C1130)]
        public void Add(GameObjectBody obj)
        {
            // TODO
        }

        [TempleDllLocation(0x100C11F0)]
        public void Remove(GameObjectBody body)
        {
            // TODO
        }

        [TempleDllLocation(0x100c1280)]
        public void UpdateLocation(GameObjectBody obj)
        {
            // TODO
        }

        [TempleDllLocation(0x100c0f60)]
        [TempleDllLocation(0x100c0cd0)]
        public IEnumerable<GameObjectBody> EnumerateInSector(SectorLoc sectorLoc)
        {
            // TODO: Needs to be more efficient
            foreach (var obj in GameSystems.Object.EnumerateNonProtos())
            {
                var loc = obj.GetLocation();
                if (new SectorLoc(loc) == sectorLoc)
                {
                    yield return obj;
                }
            }
        }
    }

}