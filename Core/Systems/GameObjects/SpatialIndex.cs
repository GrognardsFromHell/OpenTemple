using System;
using System.Runtime.Remoting;
using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems.GameObjects
{
    public class SpatialIndex
    {
        [TempleDllLocation(0x100C1130)]
        public void Add(ObjHndl objHandle)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100C11F0)]
        public void Remove(ObjHndl objHandle, GameObjectBody body)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100c1280)]
        public void UpdateLocation(ObjHndl objHandle)
        {
            throw new NotImplementedException();
        }
    }
}