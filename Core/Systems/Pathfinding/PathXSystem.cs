using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;

namespace SpicyTemple.Core.Systems.Pathfinding
{
    [Flags]
    public enum PathQueryFlags
    {
        /*
            The pathfinder seems to ignore offset x and y of the destination if this flag
            is not set.
        */
        PQF_TO_EXACT = 1,

        /*
            Indicates that the query is on behalf of a critter and the critter is set.
        */
        PQF_HAS_CRITTER = 2,

        /*
            if not set, then sets maxShortPathFindLength to 200 initially
        */
        PQF_MAX_PF_LENGTH_STHG = 4,


        PQF_STRAIGHT_LINE = 8, // appears to indicate a straight line path from->to

        PQF_10 = 0x10,
        PQF_20 = 0x20,
        PQF_40 = 0x40,
        PQF_IGNORE_CRITTERS = 0x80, // path (pass) through critters (flag is set when pathing out of combat)
        PQF_100 = 0x100,
        PQF_STRAIGHT_LINE_ONLY_FOR_SANS_NODE = 0x200,
        PQF_DOORS_ARE_BLOCKING = 0x400, // if set, it will consider doors to block the path
        PQF_800 = 0x800,
        PQF_TARGET_OBJ = 0x1000, // Indicates that the query is to move to a target object.

        /*
        Indicates that the destination should be adjusted for the critter and target
        radius.
        makes PathInit add the radii of the targets to fields tolRadius and distanceToTargetMin
        */
        PQF_ADJUST_RADIUS = 0x2000,

        PQF_DONT_USE_PATHNODES = 0x4000,
        PQF_DONT_USE_STRAIGHT_LINE = 0x8000,
        PQF_FORCED_STRAIGHT_LINE = 0x10000,
        PQF_ADJ_RADIUS_REQUIRE_LOS = 0x20000,

        /*
            if the target destination is not cleared, and PQF_ADJUST_RADIUS is off,
            it will search in a 5x5 tile neighbourgood around the original target tile
            for a clear tile (i.e. one that the critter can fit in without colliding with anything)
        */
        PQF_ALLOW_ALTERNATIVE_TARGET_TILE = 0x40000,

        // Appears to mean that pathfinding should obey the time limit
        PQF_A_STAR_TIME_CAPPED = 0x80000, // it is set when the D20 action has the flag D20CAF_UNNECESSARY

        PQF_IGNORE_CRITTERS_ON_DESTINATION = 0x800000, // NEW! makes it ignored critters on the PathDestIsClear function

        PQF_AVOID_AOOS =
            0x1000000 // NEW! Make the PF attempt avoid Aoos (using the ShouldIgnore function in combat.py to ignore insiginificant threats)
    }

    public class PathQuery
    {
        public PathQueryFlags flags;
        public int field_4;
        public LocAndOffsets from;
        public LocAndOffsets to;
        public int maxShortPathFindLength;
        public int field2c;
        public GameObjectBody critter; // Set PQF_HAS_CRITTER

        public GameObjectBody targetObj; // Set PQF_TARGET_OBJ

        /*
         When ADJ_RADIUS is set, (usually when there's a TARGET_OBJ)
         this is the minimum distance required. Should be equal
         to the sum of radii of critter + target.
        */
        public float distanceToTargetMin;
        public float tolRadius; // Tolerance (How far away from the exact destination you are allowed to be)
        public int flags2;
        public int field_4c;
    }

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

    public class Path
    {
        public PathFlags flags;
        public int field4;
        public LocAndOffsets from;
        public LocAndOffsets to;
        public GameObjectBody mover;
        public List<CompassDirection> directions = new List<CompassDirection>();
        public int nodeCount3; // TODO REMOVE, Replace with directions.Count
        public int initTo1;
        public List<LocAndOffsets> tempNodes = new List<LocAndOffsets>();
        public int nodeCount2; // TODO REMOVE, Replace with tempNodes.Count
        public int fieldd84;
        public List<LocAndOffsets> nodes = new List<LocAndOffsets>();
        public int nodeCount; // TODO REMOVE, Replace with nodes.Count
        public int currentNode;
        public int field_1a10;
        public int field_1a14;

        [TempleDllLocation(0x10040130)]
        public float GetPathResultLength()
        {
            if (flags.HasFlag(PathFlags.PF_STRAIGHT_LINE_SUCCEEDED))
            {
                return from.DistanceTo(to) / 12.0f;
            }

            var distanceSum = 0.0f;
            var nodeFrom = from;
            for (var i = 0; i < nodeCount; i++)
            {
                var nodeTo = nodes[i];
                distanceSum += nodeFrom.DistanceTo(nodeTo);
                nodeFrom = nodeTo;
            }

            return distanceSum / 12.0f;
        }

        public bool IsComplete => flags.HasFlag(PathFlags.PF_COMPLETE);

        // Returns the next node along this path, or empty in case the path is not completely built
        [TempleDllLocation(0x1003ff30)]
        public LocAndOffsets? GetNextNode()
        {
            if (!IsComplete)
            {
                return null;
            }

            if (flags.HasFlag(PathFlags.PF_STRAIGHT_LINE_SUCCEEDED))
            {
                return to;
            }
            else
            {
                if (currentNode + 1 < nodeCount)
                {
                    return nodes[currentNode];
                }

                return to;
            }
        }
    }

    public class PathQueryResult : Path
    {
        // Sometimes, a pointer to the following two values is passed as "pPauseTime" (see 100131F0)
        public int occupiedFlag;
        public int someDelay;
    }

    public class PathXSystem : IGameSystem, IResetAwareSystem
    {
        public void Dispose()
        {
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public bool FindPath(PathQuery query, out PathQueryResult result)
        {
            // TODO: TemplePlus has an implementation for this
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1003fb40)]
        public int RasterizeLineBetweenLocsScreenspace(locXY from, locXY to, sbyte[] deltas)
        {
            throw new NotImplementedException();
        }
    }
}