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
        public void Remove(ObjHndl objHandle, GameObjectBody body)
        {
            // TODO
        }

        [TempleDllLocation(0x100c1280)]
        public void UpdateLocation(GameObjectBody obj)
        {
            // TODO
        }
    }

}