using System;

namespace SpicyTemple.Core.Systems.Pathfinding
{
    [Flags]
    public enum PathFlags
    {
        PF_COMPLETE = 0x1, // Seems to indicate that the path is complete (or valid?)
        PF_2 = 0x2,
        PF_4 = 0x4,
        PF_STRAIGHT_LINE_SUCCEEDED = 0x8,
        PF_TIMED_OUT = 0x10, // Seems to be set in response to query flag PQF_A_STAR_TIME_CAPPED (maybe timeout flag?)
        PF_20 = 0x20
    }
}