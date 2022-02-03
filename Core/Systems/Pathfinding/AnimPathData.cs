using System;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;

namespace OpenTemple.Core.Systems.Pathfinding
{
    [Flags]
    public enum AnimPathDataFlags
    {
        // Don't include destination tile???
        UNK_1 = 1,
        CantOpenDoors = 2,
        UNK_4 = 4,
        UNK_8 = 8 /* Regard sinking tile ?  */,
        MovingSilently = 0x200,
        UNK10 = 0x10,
        UNK20 = 0x20,
        UNK40 = 0x40,
        UNK80 = 0x80,
        UNK100 = 0x100,
        UNK200 = 0x200,
        /* Avoid burning scenery... */
        UNK400 = 0x400,
        /* Backoff?? */
        UNK800 = 0x800,
        UNK1000 = 0x1000,
        /* Checks that the source obj's radius is free of objects at the target loc */
        UNK_2000 = 0x2000,
        UNK_4000 = 0x4000
    }

        public ref struct AnimPathData
        {
            public GameObject handle; // TODO: movingObj
            public locXY srcLoc;
            public locXY destLoc;
            public int size;
            public Span<sbyte> deltas;
            public AnimPathDataFlags flags; // Same as PathQuery.flags2
            public int distTiles;
        }
}