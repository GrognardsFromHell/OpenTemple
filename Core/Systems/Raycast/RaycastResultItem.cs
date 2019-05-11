using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;

namespace SpicyTemple.Core.Systems.Raycast
{
    public struct RaycastResultItem
    {
        public RaycastResultFlag flags;
        public int field4;
        public LocAndOffsets loc;
        public GameObjectBody obj;
        public LocAndOffsets intersectionPoint;

        /// dist from origin along the line where the object/tile intersects with the ray
        public float intersectionDistance;

        public int field34;
    }

    [Flags]
    public enum RaycastResultFlag
    {
        RaycastResultFlag1 = 1,
        BlockerSubtile = 2,
        FlyoverSubtile = 4,
        RaycastResultFlag8 = 8
    }
}