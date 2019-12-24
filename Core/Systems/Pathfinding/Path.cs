using System;
using System.Collections.Generic;
using System.Linq;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Location;

namespace OpenTemple.Core.Systems.Pathfinding
{
    public class Path
    {
        public PathFlags flags;
        public int field4;
        public LocAndOffsets from;
        public LocAndOffsets to;
        public GameObjectBody mover;
        public CompassDirection[] directions = Array.Empty<CompassDirection>();
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
                return from.DistanceTo(to) / locXY.INCH_PER_FEET;
            }

            var distanceSum = 0.0f;
            var nodeFrom = from;
            for (var i = 0; i < nodeCount; i++)
            {
                var nodeTo = nodes[i];
                distanceSum += nodeFrom.DistanceTo(nodeTo);
                nodeFrom = nodeTo;
            }

            return distanceSum / locXY.INCH_PER_FEET;
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

        public void CopyTo(Path pathOut)
        {
            pathOut.flags = flags;
            pathOut.field4 = field4;
            pathOut.from = from;
            pathOut.to = to;
            pathOut.mover = mover;
            pathOut.directions = directions.ToArray();
            pathOut.nodeCount3 = nodeCount3;
            pathOut.initTo1 = initTo1;
            pathOut.tempNodes = new List<LocAndOffsets>(tempNodes);
            pathOut.nodeCount2 = nodeCount2;
            pathOut.fieldd84 = fieldd84;
            pathOut.nodes = new List<LocAndOffsets>(nodes);
            pathOut.nodeCount = nodeCount;
            pathOut.currentNode = currentNode;
            pathOut.field_1a10 = field_1a10;
            pathOut.field_1a14 = field_1a14;
        }
    }
}