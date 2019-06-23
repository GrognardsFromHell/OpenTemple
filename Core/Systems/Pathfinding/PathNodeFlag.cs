using System;

namespace SpicyTemple.Core.Systems.Pathfinding
{
    [Flags]
    public enum PathNodeFlag
    {
        NEIGHBOUR_STATUS_CHANGED = 1,
        NEIGHBOUR_DISTANCES_SET = 0x1000
    }
}