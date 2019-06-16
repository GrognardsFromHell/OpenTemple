using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.MapSector;
using SpicyTemple.Core.Systems.Raycast;
using SpicyTemple.Core.Time;

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

        public PathQuery Copy()
        {
            return (PathQuery) MemberwiseClone();
        }
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

        public void CopyTo(Path pathOut)
        {
            pathOut.flags = flags;
            pathOut.field4 = field4;
            pathOut.from = from;
            pathOut.to = to;
            pathOut.mover = mover;
            pathOut.directions = new List<CompassDirection>(directions);
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

    public class PathQueryResult : Path
    {
        // Sometimes, a pointer to the following two values is passed as "pPauseTime" (see 100131F0)
        public int occupiedFlag; // TODO: This may be a TimePoint
        public int someDelay;
    }

    public class PathXSystem : IGameSystem, IResetAwareSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        #region Debug stuff for diagnostic render

        private LocAndOffsets pdbgFrom, pdbgTo;
        private GameObjectBody pdbgMover, pdbgTargetObj;
        private bool pdbgGotPath;
        private int pdbgShortRangeError;
        private bool pdbgUsingNodes, pdbgAbortedSansNodes;
        private int pdbgNodeNum;
        private int pdbgDirectionsCount;

        #endregion

        public PathXSystem()
        {
            aStarTimeIdx = -1;
        }

        public void Dispose()
        {
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10040570)]
        private void PathInit(out PathQueryResult pqr, PathQuery pq)
        {
            pqr = new PathQueryResult();
            pqr.flags = 0;
            pqr.field_1a10 = 0;

            var pqFlags = pq.flags;
            if (pqFlags.HasFlag(PathQueryFlags.PQF_HAS_CRITTER))
                pqr.mover = pq.critter;
            else
                pqr.mover = null;

            pqr.from = pq.from;

            if (pqFlags.HasFlag(PathQueryFlags.PQF_TARGET_OBJ))
            {
                pqr.to = pq.targetObj.GetLocationFull();
                if (pqFlags.HasFlag(PathQueryFlags.PQF_ADJUST_RADIUS))
                {
                    float tgtRadius = pq.targetObj.GetRadius();
                    pq.distanceToTargetMin = tgtRadius + pq.distanceToTargetMin;
                    pq.tolRadius = tgtRadius + pq.tolRadius;
                    if (pqFlags.HasFlag(PathQueryFlags.PQF_HAS_CRITTER))
                    {
                        float critterRadius = pq.critter.GetRadius();
                        pq.distanceToTargetMin = critterRadius + pq.distanceToTargetMin;
                        pq.tolRadius = critterRadius + pq.tolRadius;
                    }
                }
            }
            else
            {
                pqr.to = pq.to;
                if (!pqFlags.HasFlag(PathQueryFlags.PQF_TO_EXACT))
                {
                    pqr.to.off_x = 0;
                    pqr.to.off_y = 0;
                }
            }

            if (!(pqFlags.HasFlag(PathQueryFlags.PQF_MAX_PF_LENGTH_STHG)))
            {
                pq.maxShortPathFindLength = 200;
            }

            pqr.initTo1 = 1;
            pqr.currentNode = 0;
        }

        [TempleDllLocation(0x10043070)]
        public bool FindPath(PathQuery pq, out PathQueryResult pqr)
        {
            var gotPath = false;
            var triedPathNodes = false;
            TimePoint refTime;

            PathInit(out pqr, pq);
            if (Globals.Config.pathfindingDebugMode || !GameSystems.Combat.IsCombatActive())
            {
                pdbgUsingNodes = false;
                pdbgAbortedSansNodes = false;
                pdbgNodeNum = 0;
                pdbgGotPath = false;
                if (pq.critter != null)
                {
                    pdbgMover = pq.critter;
                    Logger.Info("Starting path attempt for {0}", GameSystems.MapObject.GetDisplayName(pdbgMover));
                }

                pdbgFrom = pq.from;
                if (pq.flags.HasFlag(PathQueryFlags.PQF_TARGET_OBJ) && pq.targetObj != null)
                {
                    pdbgTargetObj = pq.targetObj;
                    pdbgTo = pdbgTargetObj.GetLocationFull();
                }

                else
                {
                    pdbgTargetObj = null;
                    pdbgTo = pqr.to;
                }
            }

            var toSubtile = pqr.to.GetSubtile();
            if (pqr.from.GetSubtile() == toSubtile || !GetAlternativeTargetLocation(pqr, pq))
            {
                if (Globals.Config.pathfindingDebugMode || !GameSystems.Combat.IsCombatActive())
                {
                    if (pqr.from.GetSubtile() == toSubtile)
                        Logger.Info("Pathfinding: Aborting because from = to.");
                    else
                        Logger.Info(
                            "Pathfinding: Aborting because target tile is occupied and cannot find alternative tile.");
                }

                pdbgGotPath = false;
                return false;
            }


            if (TargetSurrounded(pqr, pq))
            {
                Logger.Info("Pathfinding: Aborting because target is surrounded.");
                return false;
            }

            if (PathCacheGet(pq, pqr))
            {
                // has this query been done before? if so copies it and returns the result
                if (Globals.Config.pathfindingDebugMode || !GameSystems.Combat.IsCombatActive())
                {
                    Logger.Info("Query found in cache, fetching result.");
                }

                return pqr.nodeCount > 0;
            }

            if (pq.flags.HasFlag(PathQueryFlags.PQF_A_STAR_TIME_CAPPED) && PathSumTime() >= Globals.Config.AStarMaxTime)
            {
                Logger.Info("Astar timed out, aborting.");
                pqr.flags |= PathFlags.PF_TIMED_OUT;
                return false;
            }

            refTime = TimePoint.Now;
            if (ShouldUsePathnodes(pqr, pq))
            {
                if (Globals.Config.pathfindingDebugMode || !GameSystems.Combat.IsCombatActive())
                {
                    Logger.Info("Attempting using nodes...");
                }

                triedPathNodes = true;
                gotPath = FindPathUsingNodes(pq, pqr);
                if (Globals.Config.pathfindingDebugMode || !GameSystems.Combat.IsCombatActive())
                {
                    Logger.Info("Nodes attempt result: {0}...", gotPath);
                }
            }
            else
            {
                if (Globals.Config.pathfindingDebugMode || !GameSystems.Combat.IsCombatActive())
                {
                    Logger.Info("Attempting sans nodes...");
                }

                gotPath = FindPathSansNodes(pq, pqr);
            }

            if (!gotPath)
            {
                if (!pq.flags.HasFlag(PathQueryFlags.PQF_DONT_USE_PATHNODES) && !triedPathNodes)
                {
                    if (Globals.Config.pathfindingDebugMode || !GameSystems.Combat.IsCombatActive())
                    {
                        pdbgAbortedSansNodes = true;
                        Logger.Info("Failed Sans Nodes attempt... trying nodes.");
                    }

                    gotPath = FindPathUsingNodes(pq, pqr);
                    if (!gotPath)
                    {
                        int dummy = 1;
                    }
                }
            }

            if (gotPath)
            {
                if (Globals.Config.pathfindingDebugMode || !GameSystems.Combat.IsCombatActive())
                {
                    if (pq.critter != null)
                    {
                        Logger.Info("{0} pathed successfully to {1}", GameSystems.MapObject.GetDisplayName(pq.critter),
                            pqr.to);
                    }
                }

                pqr.flags |= PathFlags.PF_COMPLETE;
            }
            else
            {
                if (Globals.Config.pathfindingDebugMode || !GameSystems.Combat.IsCombatActive())
                {
                    Logger.Info("PF to {0} failed!", pqr.to);
                }
            }

            PathCachePush(pq, pqr);
            PathRecordTimeElapsed(refTime);
            if (Globals.Config.pathfindingDebugMode || !GameSystems.Combat.IsCombatActive())
            {
                pdbgGotPath = gotPath;
                pdbgTo = pqr.to;
            }

            return gotPath;
        }

        [TempleDllLocation(0x10043070)]
        private bool FindPathUsingNodes(PathQuery pq, PathQueryResult pqr)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10042ab0)]
        private bool FindPathSansNodes(PathQuery pq, PathQueryResult pqr)
        {
            var pqFlags = pq.flags;
            if (!(pqFlags.HasFlag(PathQueryFlags.PQF_DONT_USE_STRAIGHT_LINE)))
            {
                var straightLineOk = FindPathStraightLine(pqr, pq);
                pqr.nodeCount = 0;
                pqr.nodeCount2 = 0;
                pqr.nodeCount3 = 0;
                if (straightLineOk)
                {
                    if (Globals.Config.pathfindingDebugMode)
                    {
                        Logger.Info("Straight line succeeded.");
                    }

                    PathNodesAddByDirections(pqr, pq);
                    return pqr.nodeCount > 0;
                }
            }

            pqFlags = pq.flags;

            if (!pqFlags.HasFlag(PathQueryFlags.PQF_STRAIGHT_LINE_ONLY_FOR_SANS_NODE))
            {
                int result;
                if (pqFlags.HasFlag(PathQueryFlags.PQF_100))
                    result = FindPathShortDistanceSansTarget(pq, pqr);
                else if (pqFlags.HasFlag(PathQueryFlags.PQF_ADJUST_RADIUS))
                    result = FindPathShortDistanceAdjRadius(pq, pqr);
                else if (!(pqFlags.HasFlag(PathQueryFlags.PQF_FORCED_STRAIGHT_LINE)))
                {
                    result = FindPathShortDistanceSansTarget(pq, pqr);
                }

                else
                    result = FindPathForcecdStraightLine(pqr, pq); // does nothing - looks like some WIP

                pqr.nodeCount = result;
                pqr.nodeCount2 = result;
                pqr.nodeCount3 = result;
                if (result > 0)
                {
                    PathNodesAddByDirections(pqr, pq);
                }
            }

            return pqr.nodeCount > 0;
        }

        [TempleDllLocation(0x10042700)]
        private int FindPathForcecdStraightLine(PathQueryResult pqr, PathQuery pq)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10041e30)]
        private int FindPathShortDistanceAdjRadius(PathQuery pq, PathQueryResult pqr)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10041730)]
        private int FindPathShortDistanceSansTarget(PathQuery pq, PathQueryResult pqr)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100427f0)]
        private bool FindPathStraightLine(PathQueryResult pqr, PathQuery pq)
        {
            var pqFlags = pq.flags;
            if (!pqFlags.HasFlag(PathQueryFlags.PQF_ADJUST_RADIUS))
            {
                if (!PathStraightLineIsClear(pqr, pq, pqr.from, pqr.to))
                {
                    return false;
                }

                pqr.flags |= PathFlags.PF_STRAIGHT_LINE_SUCCEEDED;
                return true;
            }

            // get an adjust To location (in a very hackneyed way :P)
            var delta = pqr.to.ToInches2D() - pqr.from.ToInches2D();
            var distFromTo = delta.Length();
            if (distFromTo <= pq.tolRadius - 2.0f)
            {
                return false;
            }

            var adjustFactor = (distFromTo - (pq.tolRadius - 2.0f)) / distFromTo;
            var adjToLoc = pqr.from;
            adjToLoc.off_x += delta.X * adjustFactor;
            adjToLoc.off_y += delta.Y * adjustFactor;
            adjToLoc.Regularize();

            if (!PathDestIsClear(pq, pqr.mover, adjToLoc))
            {
                return false;
            }

            if (!PathStraightLineIsClear(pqr, pq, pqr.from, adjToLoc))
            {
                return false;
            }

            pqr.to = adjToLoc;
            pqr.flags |= PathFlags.PF_STRAIGHT_LINE_SUCCEEDED;

            return true;
        }

        [TempleDllLocation(0x10040a90)]
        private bool PathStraightLineIsClear(PathQueryResult pqr, PathQuery pq, LocAndOffsets from, LocAndOffsets to)
        {
            var objIt = new RaycastPacket();
            objIt.origin = from;
            objIt.targetLoc = to;
            var dx = Math.Abs(to.location.locx - from.location.locx);
            var dy = Math.Abs(to.location.locy - from.location.locy);

            if (Math.Max(dx, dy) >= Sector.SectorSideSize * 3 - 1
                || dx + dy >= 5 * Sector.SectorSideSize - 1) // RayCast supports up to a span of 4 sectors
            {
                return false;
            }

            objIt.flags = RaycastFlag.StopAfterFirstFlyoverFound
                          | RaycastFlag.StopAfterFirstBlockerFound
                          | RaycastFlag.ExcludeItemObjects;

            if (pqr.mover != null)
            {
                objIt.flags |= RaycastFlag.HasRadius | RaycastFlag.HasSourceObj;
                objIt.sourceObj = pqr.mover;
                objIt.radius = pqr.mover.GetRadius() * 0.7f;
            }

            if (objIt.Raycast() <= 0)
            {
                return true;
            }

            foreach (var result in objIt.results)
            {
                var obj = result.obj;
                if (obj == null)
                {
                    return false; // Sector blocker!
                }

                if (!obj.HasFlag(ObjectFlag.NO_BLOCK))
                {
                    var pathFlags = pq.flags;
                    if (pathFlags.HasFlag(PathQueryFlags.PQF_DOORS_ARE_BLOCKING) || obj.type != ObjectType.portal)
                    {
                        if (!obj.IsCritter())
                        {
                            return false;
                        }

                        if (!pathFlags.HasFlag(PathQueryFlags.PQF_IGNORE_CRITTERS)
                            && !GameSystems.Critter.IsDeadOrUnconscious(obj))
                        {
                            if (pqr.mover == null)
                            {
                                return false;
                            }

                            if (!GameSystems.Critter.IsFriendly(pqr.mover, obj))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        [TempleDllLocation(0x10041490)]
        private void PathNodesAddByDirections(PathQueryResult pqr, PathQuery pq)
        {


            var pqFlags = pqr.flags;
            // check if straight line succeeded
            if (pqFlags.HasFlag(PathFlags.PF_STRAIGHT_LINE_SUCCEEDED))
            {
                pqr.flags &= ~PathFlags.PF_STRAIGHT_LINE_SUCCEEDED;
                pqr.nodes.Add(pqr.to);
                pqr.nodeCount = 1;
                pqr.currentNode = 0;
                return;
            }

            // else use the directions to build the nodes
            LocAndOffsets newNode;
            LocAndOffsets fromLoc = pqr.from;
            PathTempNodeAddByDirections(pqr.nodeCount3, pqr, out var curNode);
            newNode = curNode;
            pqr.nodeCount = 0;
            int lastIdx = 0;
            CompassDirection fromLocDirection = pqr.directions[0];
            bool directionChanged = false;

            for (int i = 2; i < pqr.nodeCount2; i++)
            {
                PathTempNodeAddByDirections(i, pqr, out curNode);
                if (fromLocDirection != pqr.directions[i-1])
                {
                    directionChanged = true;
                }
                if (!PathStraightLineIsClear(pqr, pq, fromLoc, curNode))
                {
                    // unable to go from the intermediate fromLoc to curNode
                    // first, check if the direction has change. If it hasn't, it's not logical and may be due to the discrepancy in the PathStraightLineIsClear and the new clearance based method.
                    if (directionChanged)
                    {
                        // get the previous node, which was the last "successful" one
                        newNode = PathTempNodeAddByDirections(i - 1, pqr, out newNode);
                        fromLoc = newNode;
                        // append it to nodes
                        pqr.nodes.Add(newNode);
                        pqr.nodeCount++;
                        lastIdx = i - 1;
                        fromLocDirection = pqr.directions[i - 1];
                        directionChanged = false;
                    } else
                    {
                        int dummy = 1;
                    }
                }
                else
                {
                    if (lastIdx == i-2)
                    {
                        int dummy = 1;
                    }
                }

            }

            pqr.nodes.Add(pqr.to);
            pqr.nodeCount++;
            pqr.currentNode = 0;

        }

        [TempleDllLocation(0x100407f0)]
        private LocAndOffsets PathTempNodeAddByDirections(int idx, Path pqr, out LocAndOffsets newNode)
        {
            if (idx > pqr.nodeCount3)
            {
                idx = pqr.nodeCount3;
            }

            if (pqr.tempNodes[idx] == LocAndOffsets.Zero)
            {
                var loc = pqr.from;
                for (var i = 0; i < idx; i++)
                {
                    loc = loc.OffsetSubtile(pqr.directions[i]);
                }

                pqr.tempNodes[idx] = loc;
            }

            newNode = pqr.tempNodes[idx];
            return newNode;
        }

        [TempleDllLocation(0x1003fe90)]
        private void PathCachePush(PathQuery pq, PathQueryResult pqr)
        {
            ref var cacheEntry = ref pathCache[pathCacheIdx++];
            cacheEntry.path = new Path();
            pqr.CopyTo(cacheEntry.path);
            cacheEntry.timeCached = TimePoint.Now;
            cacheEntry.query = pq.Copy();

            pathCacheCleared = false;
            if (pathCacheIdx >= PATH_RESULT_CACHE_SIZE)
            {
                pathCacheIdx = 0;
            }
        }

        private const float MinDistanceForPathNodes = 600.0f;

        [TempleDllLocation(0x10040520)]
        private bool ShouldUsePathnodes(Path pathQueryResult, PathQuery pathQuery)
        {
            if (pathQuery.flags.HasFlag(PathQueryFlags.PQF_DONT_USE_PATHNODES))
            {
                return false;
            }

            return pathQueryResult.@from.DistanceTo(pathQueryResult.to) > MinDistanceForPathNodes;
        }

        private bool TargetSurrounded(PathQueryResult pqr, PathQuery pq)
        {
            if (pq.flags.HasFlag(PathQueryFlags.PQF_IGNORE_CRITTERS)
                || !pq.flags.HasFlag(PathQueryFlags.PQF_TARGET_OBJ)
                || !pq.flags.HasFlag(PathQueryFlags.PQF_HAS_CRITTER)
                || GameSystems.Party.IsPlayerControlled(pq.critter))
            {
                // do this only in combat and only for paths with target critter
                return false;
            }

            var tgtObj = pq.targetObj;

            float maxDist = pq.tolRadius, minDist = pq.distanceToTargetMin;
            int maxSubtileDist = (int) (maxDist / locXY.INCH_PER_SUBTILE),
                minSubtileDistSqr = (int) (minDist / locXY.INCH_PER_SUBTILE * minDist / locXY.INCH_PER_SUBTILE),
                maxSubtileDistSqr = (int) (maxDist / locXY.INCH_PER_SUBTILE * maxDist / locXY.INCH_PER_SUBTILE);

            LocAndOffsets tgtLoc, tweakedLoc;
            tgtLoc = tgtObj.GetLocationFull();

            float INCH_PER_SUBTILE = locXY.INCH_PER_SUBTILE;
            for (int i = 1; i <= maxSubtileDist; i++)
            {
                float iOff = i * INCH_PER_SUBTILE;
                for (int j = -i; j <= i; j++)
                {
                    float jOff = j * INCH_PER_SUBTILE;

                    int digitalDistSqr = (i * i + j * j);
                    if (digitalDistSqr < minSubtileDistSqr
                        || digitalDistSqr > maxSubtileDistSqr)
                        continue;
                    tweakedLoc = tgtLoc;
                    tweakedLoc.off_x += jOff;
                    tweakedLoc.off_y -= iOff;
                    tweakedLoc.Regularize();
                    if (PathDestIsClear(pq, pqr.mover, tweakedLoc))
                    {
                        return false;
                    }

                    tweakedLoc = tgtLoc;
                    tweakedLoc.off_x -= jOff;
                    tweakedLoc.off_y += iOff;
                    tweakedLoc.Regularize();
                    if (PathDestIsClear(pq, pqr.mover, tweakedLoc))
                    {
                        return false;
                    }

                    tweakedLoc = tgtLoc;
                    tweakedLoc.off_x += jOff;
                    tweakedLoc.off_y += iOff;
                    tweakedLoc.Regularize();
                    if (PathDestIsClear(pq, pqr.mover, tweakedLoc))
                    {
                        return false;
                    }

                    tweakedLoc = tgtLoc;
                    tweakedLoc.off_x -= jOff;
                    tweakedLoc.off_y -= iOff;
                    tweakedLoc.Regularize();
                    if (PathDestIsClear(pq, pqr.mover, tweakedLoc))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private int aStarTimeIdx;
        private TimeSpan[] aStarTimeElapsed = new TimeSpan[20]; // array 20
        private TimePoint[] aStarTimeEnded = new TimePoint[20]; //  array 20

        private TimeSpan PathSumTime()
        {
            var totalTime = TimeSpan.Zero;
            var now = TimePoint.Now;
            for (int i = 0; i < 20; i++)
            {
                int astarIdx = aStarTimeIdx - i;
                if (astarIdx < 0)
                    astarIdx += 20;
                if (aStarTimeEnded[astarIdx] == default)
                    break;
                if (now - aStarTimeEnded[astarIdx] > Globals.Config.AStarMaxWindow)
                    break;
                totalTime += aStarTimeElapsed[i];
            }

            return totalTime;
        }

        private void PathRecordTimeElapsed(TimePoint refTime)
        {
            if (++aStarTimeIdx >= 20)
                aStarTimeIdx = 0;
            aStarTimeEnded[aStarTimeIdx] = TimePoint.Now;
            aStarTimeElapsed[aStarTimeIdx] = aStarTimeEnded[aStarTimeIdx] - refTime;
        }

        // cache used for storing paths for specific action sequences
        private const int PQR_CACHE_SIZE = 0x20;

        // different from the above, used for storing past results to minimize PF time
        private const int PATH_RESULT_CACHE_SIZE = 0x28;

        struct PathResultCache
        {
            public PathQuery query;
            public Path path;
            public TimePoint timeCached;
        }

        private static readonly TimeSpan PATH_CACHE_EXPIRATION_TIME = TimeSpan.FromSeconds(5);
        private PathResultCache[] pathCache = new PathResultCache[PATH_RESULT_CACHE_SIZE]; // used as a ring buffer
        private int pathCacheIdx;
        private bool pathCacheCleared;

        [TempleDllLocation(0x10041040)]
        private bool PathCacheGet(PathQuery pq, Path pathOut)
        {
            if (pathCacheCleared)
            {
                return false;
            }

            var now = TimePoint.Now;

            for (int i = 0; i < PATH_RESULT_CACHE_SIZE; i++)
            {
                ref var pathCacheQ = ref pathCache[i];

                if (pathCacheQ.timeCached < now - PATH_CACHE_EXPIRATION_TIME)
                {
                    continue;
                }

                if (pq.flags != pathCacheQ.query.flags || pq.critter != pathCacheQ.query.critter)
                    continue;
                var fromSubtileCache = pathCacheQ.query.from.GetSubtile();
                var fromSubtile = pq.from.GetSubtile();
                if (fromSubtileCache != fromSubtile)
                    continue;

                if (pq.flags.HasFlag(PathQueryFlags.PQF_TARGET_OBJ))
                {
                    if (pq.targetObj != pathCacheQ.query.targetObj)
                        continue;
                }
                else
                {
                    var toSubtileCache = pathCacheQ.query.to.GetSubtile();
                    var toSubtile = pq.to.GetSubtile();
                    if (toSubtileCache != toSubtile)
                        continue;
                }

                if (MathF.Abs(pq.tolRadius - pathCacheQ.query.tolRadius) > 0.0001
                    || MathF.Abs(pq.distanceToTargetMin - pathCacheQ.query.distanceToTargetMin) > 0.0001
                    || pq.maxShortPathFindLength != pathCacheQ.query.maxShortPathFindLength
                )
                    continue;

                pathCacheQ.path.CopyTo(pathOut);
                return true;
            }

            return false;
        }

        [TempleDllLocation(0x10041180)]
        private bool GetAlternativeTargetLocation(PathQueryResult pqr, PathQuery pq)
        {
            var pqFlags = pq.flags;
            if (!(pqFlags.HasFlag(PathQueryFlags.PQF_ADJUST_RADIUS))
                && !pqFlags.HasFlag(PathQueryFlags.PQF_10) && !pqFlags.HasFlag(PathQueryFlags.PQF_20))
            {
                var toLoc = pqr.to;
                if (!PathDestIsClear(pq, pqr.mover, toLoc))
                {
                    if (!pqFlags.HasFlag(PathQueryFlags.PQF_ALLOW_ALTERNATIVE_TARGET_TILE))
                    {
                        return false;
                    }

                    for (int i = 1; i <= 18; i++)
                    {
                        var iOff = i * locXY.INCH_PER_SUBTILE;
                        for (int j = -i; j < i; j++)
                        {
                            var jOff = j * locXY.INCH_PER_SUBTILE;
                            var toLocTweaked = toLoc;
                            toLocTweaked.off_x += jOff;
                            toLocTweaked.off_y -= iOff;
                            toLocTweaked.Regularize();
                            if (PathDestIsClear(pq, pqr.mover, toLocTweaked))
                            {
                                pqr.to = toLocTweaked;
                                return true;
                            }

                            toLocTweaked = toLoc;
                            toLocTweaked.off_x -= jOff;
                            toLocTweaked.off_y += iOff;
                            toLocTweaked.Regularize();
                            if (PathDestIsClear(pq, pqr.mover, toLocTweaked))
                            {
                                pqr.to = toLocTweaked;
                                return true;
                            }

                            toLocTweaked = toLoc;
                            toLocTweaked.off_x -= jOff;
                            toLocTweaked.off_y -= iOff;
                            toLocTweaked.Regularize();
                            if (PathDestIsClear(pq, pqr.mover, toLocTweaked))
                            {
                                pqr.to = toLocTweaked;
                                return true;
                            }

                            toLocTweaked = toLoc;
                            toLocTweaked.off_x += jOff;
                            toLocTweaked.off_y += iOff;
                            toLocTweaked.Regularize();
                            if (PathDestIsClear(pq, pqr.mover, toLocTweaked))
                            {
                                pqr.to = toLocTweaked;
                                return true;
                            }
                        }
                    }

                    return false;
                }
            }

            return true;
        }

        [TempleDllLocation(0x10040c30)]
        private bool PathDestIsClear(PathQuery pq, GameObjectBody mover, LocAndOffsets destLoc)
        {
            var objIt = new RaycastPacket();
            objIt.origin = destLoc;
            objIt.targetLoc = destLoc;
            objIt.flags |= RaycastFlag.ExcludeItemObjects
                           | RaycastFlag.StopAfterFirstBlockerFound
                           | RaycastFlag.StopAfterFirstFlyoverFound;

            if (mover != null)
            {
                objIt.flags |= RaycastFlag.HasSourceObj | RaycastFlag.HasRadius;
                objIt.sourceObj = mover;
                objIt.radius = mover.GetRadius();
            }

            var pqFlags = pq.flags;
            if (objIt.RaycastShortRange() > 0)
            {
                foreach (var result in objIt.results)
                {
                    if (result.obj == null)
                    {
                        // means it's a sector blocker
                        return false;
                    }

                    var objType = result.obj.type;

                    if (pqFlags.HasFlag(PathQueryFlags.PQF_DOORS_ARE_BLOCKING) || objType != ObjectType.portal)
                    {
                        var objFlags = result.obj.GetFlags();
                        if (!objFlags.HasFlag(ObjectFlag.NO_BLOCK))
                        {
                            if (objType.IsCritter() && !GameSystems.Critter.IsDeadOrUnconscious(result.obj))
                            {
                                if (!pqFlags.HasFlag(PathQueryFlags.PQF_IGNORE_CRITTERS_ON_DESTINATION))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        [TempleDllLocation(0x1003fb40)]
        public int RasterizeLineBetweenLocsScreenspace(locXY from, locXY to, Span<sbyte> deltas)
        {
            // implementation of the Bresenham line algorithm
            GameSystems.Location.GetTranslation(from.locx, from.locy, out var locTransX, out var locTransY);
            locTransX += 20;
            locTransY += 14;
            GameSystems.Location.GetTranslation(to.locx, to.locy, out var tgtTransX, out var tgtTransY);
            tgtTransX += 20;
            tgtTransY += 14;

            var rast = new LineRasterPacket(deltas);
            rast.X = locTransX;
            rast.Y = locTransY;
            rast.deltaXY = deltas;

            RasterizeLineScreenspace(locTransX, locTransY, tgtTransX, tgtTransY, ref rast);

            if (rast.deltaIdx == -1)
                return 0;

            while (rast.counter > 0)
            {
                RasterPoint(tgtTransX, tgtTransY, ref rast);
            }

            return rast.deltaIdx == -1 ? 0 : rast.deltaIdx;
        }

        [TempleDllLocation(0x1003fb40)]
        private static void RasterizeLineScreenspace(int x0, int y0, int tgtX, int tgtY, ref LineRasterPacket s300)
        {
            var x = x0;
            var y = y0;
            var deltaX = tgtX - x0;
            var deltaY = tgtY - y0;
            var deltaXAbs = Math.Abs(deltaX);
            var deltaYAbs = Math.Abs(deltaY);

            var extentX = 2 * deltaXAbs;
            var extentY = 2 * deltaYAbs;

            var deltaXSign = 0;
            var deltaYSign = 0;
            if (deltaX > 0)
                deltaXSign = 1;
            else if (deltaX < 0)
                deltaXSign = -1;

            if (deltaY > 0)
                deltaYSign = 1;
            else if (deltaY < 0)
                deltaYSign = -1;

            if (extentX <= extentY)
            {
                long D = extentX - (extentY / 2);
                RasterPoint(x0, y0, ref s300);
                while (y != tgtY)
                {
                    if (D >= 0)
                    {
                        x += deltaXSign;
                        D -= extentY;
                    }

                    D += extentX;
                    y += deltaYSign;
                    RasterPoint(x, y, ref s300);
                }
            }
            else
            {
                long D = extentY - (extentX / 2);
                RasterPoint(x0, y0, ref s300);
                while (x != tgtX)
                {
                    if (D >= 0)
                    {
                        y += deltaYSign;
                        D -= extentX;
                    }

                    D += extentY;
                    x += deltaXSign;
                    RasterPoint(x, y, ref s300);
                }
            }
        }

        [TempleDllLocation(0x1003df30)]
        private static void RasterPoint(int x, int y, ref LineRasterPacket rast)
        {
            var someIdx = rast.deltaIdx;
            if (someIdx == -1 || someIdx >= 200)
            {
                rast.deltaIdx = -1;
                return;
            }

            rast.counter++;

            if (rast.counter == rast.interval)
            {
                rast.deltaXY[someIdx] = (sbyte) (x - rast.X);
                rast.deltaXY[rast.deltaIdx + 1] = (sbyte) (y - rast.Y);
                rast.X = x;
                rast.Y = y;
                rast.deltaIdx += 2;
                rast.counter = 0;
            }
        }

        private ref struct LineRasterPacket
        {
            public int counter;
            public int interval;
            public int deltaIdx;
            public int X;
            public int Y;
            public Span<sbyte> deltaXY;

            public LineRasterPacket(Span<sbyte> deltaXY)
            {
                counter = 0;
                interval = 10;
                deltaIdx = 0;
                X = 0;
                Y = 0;
                this.deltaXY = deltaXY;
            }
        }

        [TempleDllLocation(0x1003ff00)]
        public void ClearCache()
        {
            Stub.TODO();
        }
    }
}