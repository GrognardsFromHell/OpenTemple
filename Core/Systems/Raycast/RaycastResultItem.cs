using System;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Location;

namespace OpenTemple.Core.Systems.Raycast
{
    public struct RaycastResultItem
    {
        public RaycastResultFlag flags;
        public int field4; // TODO: Probably padding
        public LocAndOffsets loc;
        public GameObjectBody obj;
        public LocAndOffsets intersectionPoint;
        public float intersectionDistance;
        public int field34; // TODO: Probably padding
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