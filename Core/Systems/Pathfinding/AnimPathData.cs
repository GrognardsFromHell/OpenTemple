using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;

namespace SpicyTemple.Core.Systems.Pathfinding
{
    [Flags]
    public enum AnimPathDataFlags
    {
        UNK_1 = 1,
        CantOpenDoors = 2,
        UNK_4 = 4,
        UNK_8 = 4,
        MovingSilently = 0x200,
        UNK10 = 0x10,
        UNK20 = 0x20,
        UNK40 = 0x40,
        UNK_2000 = 0x2000
    }

        public ref struct AnimPathData
        {
            public GameObjectBody handle; // TODO: movingObj
            public locXY srcLoc;
            public locXY destLoc;
            public int size;
            public Span<sbyte> deltas;
            public AnimPathDataFlags flags; // Same as PathQuery.flags2
            public int distTiles;
        }
}