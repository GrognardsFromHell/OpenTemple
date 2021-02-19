using System;
using System.Diagnostics;
using System.Numerics;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.Systems.Raycast;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.Systems.Pathfinding
{
    public class PathXSystem : IGameSystem, IResetAwareSystem
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private AnimPathFinder _animPathFinder = new AnimPathFinder();

        #region Debug stuff for diagnostic render

        private LocAndOffsets pdbgFrom, pdbgTo;
        private GameObjectBody pdbgMover, pdbgTargetObj;
        private bool pdbgGotPath;
        private int pdbgShortRangeError;
        private bool pdbgUsingNodes, pdbgAbortedSansNodes;
        private int pdbgNodeNum;
        private int pdbgDirectionsCount;

        #endregion

        [TempleDllLocation(0x10042a90)]
        public PathXSystem()
        {
            aStarTimeIdx = -1;
        }

        public void Dispose()
        {
        }

        [TempleDllLocation(0x10042aa0)]
        public void Reset()
        {
            ClearCache();
            aStarTimeIdx = -1;
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
                    Logger.Info("Starting path attempt for {0}", pdbgMover);
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

            var toSubtile = new Subtile(pqr.to);
            if (new Subtile(pqr.from) == toSubtile || !GetAlternativeTargetLocation(pqr, pq))
            {
                if (Globals.Config.pathfindingDebugMode || !GameSystems.Combat.IsCombatActive())
                {
                    if (new Subtile(pqr.from) == toSubtile)
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

        private const int MAX_PATH_NODE_CHAIN_LENGTH = 30;

        [TempleDllLocation(0x10042B50)]
        private bool FindPathUsingNodes(PathQuery pq, PathQueryResult path)
        {
            if (Globals.Config.pathfindingDebugMode)
            {
                Logger.Debug("Attempting PF using nodes");
            }

            // TODO: Clean this up
            var pathQueryLocal = pq.Copy();
            pathQueryLocal.to = path.to;
            pathQueryLocal.from = path.from;
            pathQueryLocal.flags |= PathQueryFlags.PQF_ALLOW_ALTERNATIVE_TARGET_TILE
                                    | PathQueryFlags.PQF_STRAIGHT_LINE_ONLY_FOR_SANS_NODE;

            PathInit(out var pathLocal, pathQueryLocal);
            pathQueryLocal.to = path.to;
            pathQueryLocal.from = path.from;
            pathQueryLocal.flags |= PathQueryFlags.PQF_ALLOW_ALTERNATIVE_TARGET_TILE
                                    | PathQueryFlags.PQF_STRAIGHT_LINE_ONLY_FOR_SANS_NODE;
            pathLocal.from = path.from;
            pathLocal.to = pathQueryLocal.to;

            if (FindPathStraightLine(pathLocal, pathQueryLocal))
            {
                pathLocal.nodeCount = 0;
                pathLocal.nodeCount2 = 0;
                pathLocal.nodeCount3 = 0;
                PathNodesAddByDirections(pathLocal, pq);
                pathLocal.CopyTo(path);
                return path.nodeCount > 0;
            }

            var from = path.from;

            if (!GameSystems.PathNode.FindClosestPathNode(path.from, out var fromClosestId))
                return false;

            if (!GameSystems.PathNode.FindClosestPathNode(path.to, out var toClosestId))
                return false;

            Span<int> nodeIds = stackalloc int[MAX_PATH_NODE_CHAIN_LENGTH];
            var chainLength = GameSystems.PathNode.FindPathBetweenNodes(fromClosestId, toClosestId, nodeIds);
            if (chainLength == 0)
            {
                return false;
            }

            if (Globals.Config.pathfindingDebugMode)
            {
                pdbgUsingNodes = true;
                pdbgNodeNum = chainLength;
            }

            int i0 = 0;
            if (chainLength > 1)
            {
                GameSystems.PathNode.GetPathNode(nodeIds[1], out var nodeTemp1);
                GameSystems.PathNode.GetPathNode(nodeIds[0], out var nodeTemp0);
                float distFromSecond = from.DistanceTo(nodeTemp1.nodeLoc);
                // TODO: Where is this magic number coming from?
                if (distFromSecond < 614.0)
                {
                    if (nodeTemp0.nodeLoc.DistanceTo(nodeTemp1.nodeLoc) > distFromSecond)
                    {
                        i0 = 1;
                    }
                }

                // attempt straight line from 2nd last pathnode to destination
                // if this is possible, it will shorten the path and avoid a zigzag going from the last node to destination
                GameSystems.PathNode.GetPathNode(nodeIds[chainLength - 2], out nodeTemp1);
                GameSystems.PathNode.GetPathNode(nodeIds[chainLength - 1], out nodeTemp0);
                if (!pathQueryLocal.flags.HasFlag(PathQueryFlags.PQF_TARGET_OBJ))
                {
                    pathQueryLocal = pq.Copy();
                    pathQueryLocal.to = path.to;
                    pathQueryLocal.from = nodeTemp1.nodeLoc;
                    pathQueryLocal.flags &= ~(PathQueryFlags.PQF_ADJUST_RADIUS | PathQueryFlags.PQF_TARGET_OBJ);
                    pathQueryLocal.flags |= PathQueryFlags.PQF_ALLOW_ALTERNATIVE_TARGET_TILE;
                    if (!GameSystems.PathNode.HasClearanceData)
                    {
                        pathQueryLocal.flags |= PathQueryFlags.PQF_STRAIGHT_LINE_ONLY_FOR_SANS_NODE;
                    }

                    PathInit(out pathLocal, pathQueryLocal);
                    pathQueryLocal.to = path.to;
                    pathQueryLocal.from = nodeTemp1.nodeLoc;
                    pathQueryLocal.flags &= ~(PathQueryFlags.PQF_ADJUST_RADIUS | PathQueryFlags.PQF_TARGET_OBJ);
                    pathQueryLocal.flags |= PathQueryFlags.PQF_ALLOW_ALTERNATIVE_TARGET_TILE;
                    if (!GameSystems.PathNode.HasClearanceData)
                    {
                        pathQueryLocal.flags |= PathQueryFlags.PQF_STRAIGHT_LINE_ONLY_FOR_SANS_NODE;
                    }

                    pathLocal.from = nodeTemp1.nodeLoc;
                    pathLocal.to = pathQueryLocal.to;
                    if (FindPathSansNodes(pathQueryLocal, pathLocal))
                    {
                        chainLength--;
                    }
                }
                else
                {
                    float distSecondLastToDestination = path.to.DistanceTo(nodeTemp1.nodeLoc);
                    if (distSecondLastToDestination < 400.0)
                    {
                        if (nodeTemp0.nodeLoc.DistanceTo(nodeTemp1.nodeLoc) > distSecondLastToDestination)
                        {
                            chainLength--;
                        }
                    }
                }
            }

            // add paths from node to node
            bool destinationReached = false;
            for (int i = i0; i < chainLength; i++)
            {
                // define the queries, init etc.
                GameSystems.PathNode.GetPathNode(nodeIds[i], out var nodeTemp1);
                pathQueryLocal = pq.Copy();
                pathQueryLocal.flags &= ~(PathQueryFlags.PQF_ADJUST_RADIUS | PathQueryFlags.PQF_TARGET_OBJ);
                pathQueryLocal.flags |= PathQueryFlags.PQF_ALLOW_ALTERNATIVE_TARGET_TILE;
                pathQueryLocal.from = from;
                pathQueryLocal.to = nodeTemp1.nodeLoc;
                PathInit(out pathLocal, pathQueryLocal);

                pathQueryLocal = pq.Copy();
                pathQueryLocal.flags &= ~(PathQueryFlags.PQF_ADJUST_RADIUS | PathQueryFlags.PQF_TARGET_OBJ);
                pathQueryLocal.flags |= PathQueryFlags.PQF_ALLOW_ALTERNATIVE_TARGET_TILE;

                pathLocal.from = pathQueryLocal.from = from;
                pathLocal.to = pathQueryLocal.to = nodeTemp1.nodeLoc;
                pathLocal.nodeCount = pathLocal.nodeCount2 = pathLocal.nodeCount3 = 0;

                // verifies that the destination is clear, and if not, tries to get an available tile
                if (!GetAlternativeTargetLocation(pathLocal, pathQueryLocal))
                {
                    Logger.Warn("Warning: pathnode not clear");
                }

                if (pathLocal.to != nodeTemp1.nodeLoc) //  path "To" location has been adjusted
                {
                    nodeTemp1.nodeLoc = pathLocal.to;
                }

                // attempt PF
                var foundPath = FindPathSansNodes(pathQueryLocal, pathLocal);
                if (!foundPath)
                {
                    pathQueryLocal = pq.Copy();
                    pathQueryLocal.flags &= ~PathQueryFlags.PQF_ADJUST_RADIUS;
                    pathQueryLocal.flags |= PathQueryFlags.PQF_ALLOW_ALTERNATIVE_TARGET_TILE;
                    pathQueryLocal.from = from;
                    pathQueryLocal.to = nodeTemp1.nodeLoc;
                    pathLocal.from = from;
                    pathLocal.nodeCount = 0;
                    pathLocal.nodeCount2 = 0;
                    pathLocal.nodeCount3 = 0;
                    pathLocal.to = pathQueryLocal.to;

                    foundPath = FindPathSansNodes(pathQueryLocal, pathLocal);
                }

                if (!foundPath || (pathLocal.nodes.Count + path.nodes.Count > pq.maxShortPathFindLength))
                {
                    return false;
                }

                path.nodes.AddRange(pathLocal.nodes);
                path.nodeCount = path.nodes.Count;
                from = nodeTemp1.nodeLoc;

                // the before last node - try bypassing and going directly to critter
                if (i == chainLength - 2 && pq.flags.HasFlag(PathQueryFlags.PQF_TARGET_OBJ))
                {
                    pathQueryLocal = pq.Copy();
                    pathQueryLocal.from = from;
                    pathQueryLocal.to = path.to;

                    PathInit(out pathLocal, pathQueryLocal);

                    pathQueryLocal = pq.Copy();
                    pathLocal.from = pathQueryLocal.from = from;
                    pathLocal.to = pathQueryLocal.to = path.to;

                    if (FindPathSansNodes(pathQueryLocal, pathLocal))
                    {
                        path.nodes.AddRange(pathLocal.nodes);
                        path.nodeCount = path.nodes.Count;
                        path.to = pathLocal.to;
                        return path.nodes.Count > 0;
                    }

                    if (pathLocal.to == pathLocal.from)
                    {
                        path.nodes.Add(pathLocal.to);
                        path.nodeCount = path.nodes.Count;
                        path.to = pathLocal.to;
                        return path.nodes.Count > 0;
                    }
                }
            }

            if (destinationReached)
            {
                return path.nodes.Count > 0;
            }

            // now path from the last location (can be an adjusted path node) to the final destination
            pathQueryLocal = pq.Copy();
            pathQueryLocal.from = from;
            pathQueryLocal.to = path.to;

            PathInit(out pathLocal, pathQueryLocal);

            pathQueryLocal = pq.Copy();
            pathLocal.from = pathQueryLocal.from = from;
            pathLocal.to = pathQueryLocal.to = path.to;

            var foundLastPath = FindPathSansNodes(pathQueryLocal, pathLocal);
            if ((!foundLastPath &&
                 (pathLocal.to != pathLocal.from)
                ) // there's a possibility that the "from" is within reach, in which case the search will set the To same as From
                || (pathLocal.nodes.Count + path.nodes.Count > pq.maxShortPathFindLength))
            {
                return false;
            }

            if (foundLastPath)
            {
                path.nodes.AddRange(pathLocal.nodes);
                path.nodeCount = path.nodes.Count;
                path.to = path.nodes[path.nodes.Count - 1];
            }
            else
            {
                path.nodes.Add(pathLocal.to);
                path.nodeCount = path.nodes.Count;
                path.to = pathLocal.to;
            }

            return path.nodes.Count > 0;
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
            // TODO: This function does nothing
            var critter = pq.critter;
            if (critter != null)
            {
                if (critter.type == ObjectType.npc)
                {
                    if (npcPathFindRefTime != default &&
                        TimePoint.Now - npcPathFindRefTime < TimeSpan.FromMilliseconds(1000))
                    {
                        if (npcPathFindAttemptCount > 10 || npcPathTimeCumulative > TimeSpan.FromMilliseconds(250))
                        {
                            return 0;
                        }

                        npcPathFindAttemptCount++;
                    }
                    else
                    {
                        npcPathFindAttemptCount = 1;
                        npcPathTimeCumulative = TimeSpan.Zero;
                        npcPathFindRefTime = TimePoint.Now;
                    }
                }
            }

            return 0;
        }

        [TempleDllLocation(0x10041e30)]
        private int FindPathShortDistanceAdjRadius(PathQuery pq, PathQueryResult pqr)
        {
            return FindPathShortDistanceSansTarget(pq, pqr, true);
        }

        private TimePoint npcPathFindRefTime;
        private int npcPathFindAttemptCount;
        private TimeSpan npcPathTimeCumulative;

        private struct PathSubtile
        {
            public int length;
            public int refererIdx;
            public int idxPreviousChain; // is actually +1 (0 means none); i.e. subtract 1 to get the actual idx
            public int idxNextChain; // same as above
        }

        [TempleDllLocation(0x10041730)]
        private int FindPathShortDistanceSansTarget(PathQuery pq, PathQueryResult pqr, bool adjust = false)
        {
            // uses a form of A*
            // pathfinding heuristic:
            // taxicab metric h(dx,dy)=max(dx, dy), wwhere  dx,dy is the subtile difference

            #region Preamble

            const int gridSize = 160; // number of subtiles spanned in the grid (vanilla: 128)
            TimePoint referenceTime = default;

            var fromSubtile = new Subtile(pqr.from);
            var toSubtile = new Subtile(pqr.to);

            if (pq.critter != null && pq.critter.type == ObjectType.npc)
            {
                // limits the number of attempt to 10 per second and cumulative time to 250 sec
                int attemptCount;
                if (npcPathFindRefTime != default &
                    (TimePoint.Now - npcPathFindRefTime) < TimeSpan.FromMilliseconds(1000))
                {
                    attemptCount = npcPathFindAttemptCount;
                    var maxAttempts = GameSystems.PathNode.HasClearanceData ? 50 : 10;
                    if (npcPathFindAttemptCount > maxAttempts || npcPathTimeCumulative > TimeSpan.FromMilliseconds(250))
                    {
                        if (Globals.Config.pathfindingDebugMode)
                        {
                            pdbgDirectionsCount = 0;
                            if (npcPathFindAttemptCount > maxAttempts)
                            {
                                Logger.Info("NPC pathing attempt count exceeded, aborting.");
                                pdbgShortRangeError = -maxAttempts;
                            }
                            else
                            {
                                pdbgShortRangeError = -250;
                                Logger.Info("NPC pathing cumulative time exceeded, aborting.");
                            }
                        }

                        return 0;
                    }
                }
                else
                {
                    npcPathFindRefTime = TimePoint.Now;
                    attemptCount = 0;
                    npcPathTimeCumulative = default;
                }

                npcPathFindAttemptCount = attemptCount + 1;
                referenceTime = TimePoint.Now;
            }

            int fromSubtileX = fromSubtile.X;
            int fromSubtileY = fromSubtile.Y;
            int toSubtileX = toSubtile.X;
            int toSubtileY = toSubtile.Y;

            int deltaSubtileX = Math.Abs(toSubtileX - fromSubtileX);
            int deltaSubtileY = Math.Abs(toSubtileY - fromSubtileY);
            if (deltaSubtileX > gridSize / 2 || deltaSubtileY > gridSize / 2)
            {
                pdbgDirectionsCount = 0;
                pdbgShortRangeError = gridSize;
                Logger.Info("Desitnation too far for short distance PF grid! Aborting.");
                return 0;
            }


            int lowerSubtileX = Math.Min(fromSubtileX, toSubtileX);
            int lowerSubtileY = Math.Min(fromSubtileY, toSubtileY);

            int cornerX = lowerSubtileX + deltaSubtileX / 2 - gridSize / 2;
            int cornerY = lowerSubtileY + deltaSubtileY / 2 - gridSize / 2;

            int curIdx = fromSubtileX - cornerX + ((fromSubtileY - cornerY) * gridSize);
            int idxTarget = toSubtileX - cornerX + ((toSubtileY - cornerY) * gridSize);

            int idxTgtX = idxTarget % gridSize;
            int idxTgtY = idxTarget / gridSize;

            Span<PathSubtile> pathFindAstar = stackalloc PathSubtile[gridSize * gridSize];

            pathFindAstar[curIdx].length = 1;
            pathFindAstar[curIdx].refererIdx = -1;


            int lastChainIdx = curIdx;
            int firstChainIdx = curIdx;
            int deltaIdxX;
            int distanceMetric;

            int refererIdx, heuristic;
            int minHeuristic = 0x7FFFffff;
            int idxPrevChain, idxNextChain;

            int shiftedXidx, shiftedYidx, newIdx;

            float requisiteClearance = 0.0f;
            if (pq.critter != null)
                requisiteClearance = pq.critter.GetRadius();
            float diagonalClearance = requisiteClearance * 0.7f;
            float requisiteClearanceCritters = requisiteClearance * 0.7f;
            if (requisiteClearance > 12)
                requisiteClearance *= 0.85f;

            if (curIdx == -1)
            {
                if (referenceTime != default)
                {
                    npcPathTimeCumulative += TimePoint.Now - referenceTime;
                }

                if (Globals.Config.pathfindingDebugMode)
                {
                    pdbgDirectionsCount = 0;
                    pdbgShortRangeError = -1;
                    Logger.Info("curIdx is -1, aborting.");
                }

                return 0;
            }

            var proxList = new ProximityList();
            proxList.Populate(pq, pqr, locXY.INCH_PER_TILE * 40);

            #endregion

            if (Globals.Config.pathfindingDebugMode)
            {
                Logger.Info("*** START OF PF ATTEMPT SANS TARGET - DESTINATION {0} ***", pqr.to);
                pdbgDirectionsCount = 0;
                pdbgShortRangeError = 0;
            }

            LocAndOffsets subPathFrom = LocAndOffsets.Zero;
            while (true)
            {
                refererIdx = -1;
                do // loop over the chain to find the node with minimal heuristic; initially the chain is just the "from" node
                {
                    deltaIdxX = Math.Abs((int) (curIdx % gridSize - idxTgtX));
                    distanceMetric = Math.Abs((int) (curIdx / gridSize - idxTgtY));
                    if (deltaIdxX > distanceMetric)
                        distanceMetric = deltaIdxX;

                    heuristic = pathFindAstar[curIdx].length + 10 * distanceMetric;
                    if ((heuristic / 10) <= pq.maxShortPathFindLength)
                    {
                        if (refererIdx == -1 || heuristic < minHeuristic)
                        {
                            minHeuristic = pathFindAstar[curIdx].length + 10 * distanceMetric;
                            refererIdx = curIdx;
                        }

                        idxNextChain = pathFindAstar[curIdx].idxNextChain;
                    }
                    else
                    {
                        idxPrevChain = pathFindAstar[curIdx].idxPreviousChain;
                        pathFindAstar[curIdx].length = int.MinValue;
                        if (idxPrevChain != 0)
                            pathFindAstar[idxPrevChain - 1].idxNextChain = pathFindAstar[curIdx].idxNextChain;
                        else
                            firstChainIdx = pathFindAstar[curIdx].idxNextChain - 1;
                        idxNextChain = pathFindAstar[curIdx].idxNextChain;
                        if (idxNextChain != 0)
                            pathFindAstar[idxNextChain - 1].idxPreviousChain = pathFindAstar[curIdx].idxPreviousChain;
                        else
                            lastChainIdx = pathFindAstar[curIdx].idxPreviousChain - 1;
                        pathFindAstar[curIdx].idxPreviousChain = 0;
                        pathFindAstar[curIdx].idxNextChain = 0;
                    }

                    curIdx = idxNextChain - 1;
                } while (curIdx >= 0);

                if (refererIdx == -1)
                {
                    if (referenceTime != default)
                        npcPathTimeCumulative += TimePoint.Now - referenceTime;
                    if (Globals.Config.pathfindingDebugMode)
                    {
                        pdbgShortRangeError = -999;
                        Logger.Info("*** END OF PF ATTEMPT SANS TARGET - OPEN SET EMPTY; from {0} to {1} ***", pqr.from,
                            pqr.to);
                    }

                    return 0;
                }

                // halt condition
                if (refererIdx == idxTarget) break;

                var _fromSubtile = new Subtile(cornerX + (refererIdx % gridSize), cornerY + (refererIdx / gridSize));

                // Halt condition - within reach of the target
                if (adjust)
                {
                    subPathFrom = _fromSubtile.ToLocAndOffset();
                    var subPathTo = toSubtile.ToLocAndOffset();
                    float distToTgt = subPathFrom.DistanceTo(pqr.to);

                    if (distToTgt >= pq.distanceToTargetMin && distToTgt <= pq.tolRadius)
                    {
                        if (!pq.flags.HasFlag(PathQueryFlags.PQF_ADJ_RADIUS_REQUIRE_LOS)
                            || PathAdjRadiusLosClear(subPathFrom, subPathTo))
                        {
                            if ((pq.flags & (PathQueryFlags.PQF_20 | PathQueryFlags.PQF_10)) != 0
                                || PathDestIsClear(pq, pqr.mover, subPathFrom))
                            {
                                idxTarget = refererIdx;
                                break;
                            }
                        }
                    }
                }

                // loop over all possible directions for better path
                for (var direction = 0; direction < 8; direction++)
                {
                    if (!_fromSubtile.OffsetByOne((CompassDirection) direction, out var shiftedSubtile))
                        continue;
                    shiftedXidx = shiftedSubtile.X - cornerX;
                    shiftedYidx = shiftedSubtile.Y - cornerY;
                    if (shiftedXidx >= 0 && shiftedXidx < gridSize && shiftedYidx >= 0 && shiftedYidx < gridSize)
                    {
                        newIdx = shiftedXidx + (shiftedYidx * gridSize);
                    }
                    else
                        continue;

                    if (pathFindAstar[newIdx].length == int.MinValue)
                        continue;

                    subPathFrom = _fromSubtile.ToLocAndOffset();
                    var subPathTo = shiftedSubtile.ToLocAndOffset();

                    int oldLength = pathFindAstar[newIdx].length;
                    int newLength =
                        pathFindAstar[refererIdx].length + 14 -
                        4 * (direction % 2); // +14 for diagonal, +10 for straight

                    if (oldLength == 0 || Math.Abs(oldLength) > newLength)
                    {
                        if (GameSystems.PathNode.HasClearanceData)
                        {
                            var secLoc = new SectorLoc(subPathTo.location);
                            //secLoc.GetFromLoc(subPathTo.location);
                            var sectorClearance = GameSystems.PathNode.ClearanceData.GetSectorClearance(secLoc);

                            var clearance = sectorClearance.GetClearance(shiftedSubtile.Y % 192, shiftedSubtile.X % 192);
                            if (direction % 2 != 0) // xy straight
                            {
                                if (clearance < requisiteClearance)
                                {
                                    continue;
                                }
                            }
                            else // xy diagonal
                            {
                                if (clearance < diagonalClearance)
                                {
                                    continue;
                                }
                            }

                            if (proxList.FindNear(subPathTo, requisiteClearanceCritters))
                            {
                                continue;
                            }
                        }
                        else if (!PathStraightLineIsClear(pqr, pq, subPathFrom, subPathTo))
                        {
                            continue;
                        }

                        pathFindAstar[newIdx].length = newLength;
                        pathFindAstar[newIdx].refererIdx = refererIdx;
                        if (pathFindAstar[newIdx].idxPreviousChain == 0 && pathFindAstar[newIdx].idxNextChain == 0
                        ) //  if node is not part of chain
                        {
                            pathFindAstar[lastChainIdx].idxNextChain = newIdx + 1;
                            pathFindAstar[newIdx].idxPreviousChain = lastChainIdx + 1;
                            pathFindAstar[newIdx].idxNextChain = 0;
                            lastChainIdx = newIdx;
                        }
                    }
                }

                pathFindAstar[refererIdx].length = -pathFindAstar[refererIdx].length; // mark the referer as used
                // remove the referer from the chain
                // connect its previous to its next
                idxPrevChain = pathFindAstar[refererIdx].idxPreviousChain - 1;
                if (idxPrevChain != -1)
                {
                    pathFindAstar[idxPrevChain].idxNextChain = pathFindAstar[refererIdx].idxNextChain;
                    curIdx = firstChainIdx;
                }
                else // if no "previous" exists, set the First Chain as the referer's next
                {
                    curIdx = pathFindAstar[refererIdx].idxNextChain - 1;
                    firstChainIdx = curIdx;
                }

                //connect its next to its previous
                if (pathFindAstar[refererIdx].idxNextChain != 0)
                    pathFindAstar[pathFindAstar[refererIdx].idxNextChain - 1].idxPreviousChain
                        = pathFindAstar[refererIdx].idxPreviousChain;
                else // if no "next" exists, set the last chain as its previous
                    lastChainIdx = pathFindAstar[refererIdx].idxPreviousChain - 1;
                pathFindAstar[refererIdx].idxPreviousChain = 0;
                pathFindAstar[refererIdx].idxNextChain = 0;
                if (curIdx == -1)
                {
                    if (referenceTime != default)
                        npcPathTimeCumulative += TimePoint.Now - referenceTime;
                    if (Globals.Config.pathfindingDebugMode)
                    {
                        Logger.Info("*** END OF PF ATTEMPT SANS TARGET - A* OPTIONS EXHAUSTED; from {0} to {1} ***",
                            pqr.from, pqr.to);
                    }

                    return 0;
                }

                idxTgtX = idxTarget % gridSize;
            }


            // count the directions
            var refIdx = pathFindAstar[refererIdx].refererIdx;
            int directionsCount = 0;
            while (refIdx != -1)
            {
                directionsCount++;
                refIdx = pathFindAstar[refIdx].refererIdx;
            }


            if (directionsCount > pq.maxShortPathFindLength)
            {
                if (referenceTime != default)
                    npcPathTimeCumulative += TimePoint.Now - referenceTime;
                return 0;
            }

            int lastIdx = idxTarget;

            pqr.directions = new CompassDirection[directionsCount];
            for (int i = directionsCount - 1; i >= 0; --i)
            {
                var prevIdx = pathFindAstar[lastIdx].refererIdx;
                pqr.directions[i] = GetDirection(prevIdx, gridSize, lastIdx);
                lastIdx = prevIdx;
            }

            if (pq.flags.HasFlag(PathQueryFlags.PQF_10))
            {
                --directionsCount;
            }

            if (referenceTime != default)
                npcPathTimeCumulative += TimePoint.Now - referenceTime;

            if (adjust)
            { // modify the destination to the found location
                pqr.to = subPathFrom;
            }
            if (Globals.Config.pathfindingDebugMode)
            {
                Logger.Info("*** END OF PF ATTEMPT SANS TARGET - {0} DIRECTIONS USED ***", directionsCount);
                pdbgDirectionsCount = directionsCount;
                pdbgShortRangeError = 0;
            }


            return directionsCount;
        }


        private bool PathAdjRadiusLosClear(LocAndOffsets from, LocAndOffsets to)
        {
            using var objIt = new RaycastPacket();
            objIt.origin = from;
            objIt.targetLoc = to;
            objIt.flags = RaycastFlag.StopAfterFirstBlockerFound
                          | RaycastFlag.StopAfterFirstFlyoverFound
                          | RaycastFlag.ExcludeItemObjects;
            objIt.radius = 0.01f;

            foreach (var result in objIt)
            {
                if (result.obj == null && result.flags.HasFlag(RaycastResultFlag.BlockerSubtile))
                {
                    return false;
                }
            }

            return true;
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


        private CompassDirection GetDirection(int idxFrom, int gridSize, int idxTo)
        {
            int deltaX;
            int deltaY;
            CompassDirection result;

            deltaX = (idxTo % gridSize) - (idxFrom % gridSize);
            deltaY = (idxTo / gridSize) - (idxFrom / gridSize);
            Trace.Assert(Math.Abs(deltaX) + Math.Abs(deltaY) > 0);
            if (deltaY < 0)
            {
                if (deltaX < 0)
                    result = CompassDirection.Top;
                else if (deltaX > 0)
                    result = CompassDirection.Left;
                else // deltaX == 0
                    result = CompassDirection.TopLeft;
            }
            else if (deltaY > 0)
            {
                if (deltaX > 0)
                    result = CompassDirection.Bottom;
                else if (deltaX < 0)
                    result = CompassDirection.Right;
                else // deltaX == 0
                    result = CompassDirection.BottomRight;
            }
            else // deltaY == 0
            {
                if (deltaX < 0)
                    result = CompassDirection.TopRight;
                else // deltaX > 0
                    result = CompassDirection.BottomLeft;
            }

            return result;
        }

        [TempleDllLocation(0x10040a90)]
        private bool PathStraightLineIsClear(PathQueryResult pqr, PathQuery pq, LocAndOffsets from, LocAndOffsets to)
        {
            using var objIt = new RaycastPacket();
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
                if (fromLocDirection != pqr.directions[i - 1])
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
                    }
                    else
                    {
                        int dummy = 1;
                    }
                }
                else
                {
                    if (lastIdx == i - 2)
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

            // TODO This is super weird... (unclera what tempNodes is actually used for)
            while (idx >= pqr.tempNodes.Count)
                pqr.tempNodes.Add(default);

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
                var fromSubtileCache = new Subtile(pathCacheQ.query.from);
                var fromSubtile = new Subtile(pq.from);
                if (fromSubtileCache != fromSubtile)
                    continue;

                if (pq.flags.HasFlag(PathQueryFlags.PQF_TARGET_OBJ))
                {
                    if (pq.targetObj != pathCacheQ.query.targetObj)
                        continue;
                }
                else
                {
                    var toSubtileCache = new Subtile(pathCacheQ.query.to);
                    var toSubtile = new Subtile(pq.to);
                    if (toSubtileCache != toSubtile)
                    {
                        continue;
                    }
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
        public bool PathDestIsClear(PathQuery pq, GameObjectBody mover, LocAndOffsets destLoc)
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
            if (!pathCacheCleared)
            {
                pathCacheIdx = 0;
                for (var i = 0; i < pathCache.Length; i++)
                {
                    pathCache[i] = default;
                }
            }

            pathCacheCleared = true;
        }

        [TempleDllLocation(0x10041630)]
        public void GetPartialPath(Path path, out PathQueryResult trimmedResult, float startDistFeet, float endDistFeet)
        {
            trimmedResult = new PathQueryResult();
            path.CopyTo(trimmedResult);
            TruncatePathToDistance(path, out trimmedResult.from, startDistFeet);
            TruncatePathToDistance(path, out trimmedResult.to, endDistFeet);

            if (path.flags.HasFlag(PathFlags.PF_STRAIGHT_LINE_SUCCEEDED))
            {
                return; // For straight lines, the adjusted from/to are enough
            }

            FindNodeDistanceGreaterThan(path, out var startIndex, startDistFeet * 12.0f);
            FindNodeDistanceGreaterThan(path, out var endIndex, endDistFeet * 12.0f);

            trimmedResult.nodes.Clear();
            for (var i = startIndex; i < endIndex; i++)
            {
                trimmedResult.nodes.Add(path.nodes[i]);
            }

            trimmedResult.nodes.Add(trimmedResult.to);
            trimmedResult.nodeCount = trimmedResult.nodes.Count;
        }

        [TempleDllLocation(0x100409e0)]
        private void FindNodeDistanceGreaterThan(Path path, out int idxExceeded, float maxNodeDist)
        {
            var remainingDistance = maxNodeDist;

            var prevLoc = path.from;

            int i;
            for (i = 0; i < path.nodes.Count; i++)
            {
                var nodeLoc = path.nodes[i];
                var newRemainingDistance = remainingDistance - prevLoc.DistanceTo(nodeLoc);
                if (newRemainingDistance <= 0.0)
                {
                    break;
                }

                remainingDistance = newRemainingDistance;
                prevLoc = nodeLoc;
            }

            if (i == path.nodeCount && path.nodeCount > 0)
            {
                --i;
            }

            idxExceeded = i;
        }

        /// <summary>
        /// Computes the position on the path that is at the given distance from the start of the path.
        /// </summary>
        [TempleDllLocation(0x10040200)]
        public void TruncatePathToDistance(Path path, out LocAndOffsets truncatedPos, float truncateLengthFeet)
        {
            var remainingDistance = truncateLengthFeet * locXY.INCH_PER_FEET; // Convert to inches
            if (path.flags.HasFlag(PathFlags.PF_STRAIGHT_LINE_SUCCEEDED))
            {
                var fromPos = path.from.ToInches2D();
                var toPos = path.to.ToInches2D();
                var normal = Vector2.Normalize(toPos - fromPos);
                var pos = fromPos + normal * remainingDistance;
                truncatedPos = LocAndOffsets.FromInches(pos);
            }
            else
            {
                truncatedPos = path.from;

                int i;
                for (i = 0; i < path.nodeCount; i++)
                {
                    var node = path.nodes[i];
                    var newRemainingDistance = remainingDistance - truncatedPos.DistanceTo(node);
                    if (newRemainingDistance < 0.0f)
                    {
                        break;
                    }

                    remainingDistance = newRemainingDistance;
                    truncatedPos = node;
                }

                if (i < path.nodeCount)
                {
                    var worldPos = truncatedPos.ToInches2D();
                    var nextWorldPos = path.nodes[i].ToInches2D();
                    var normal = Vector2.Normalize(nextWorldPos - worldPos);
                    var pos = worldPos + normal * remainingDistance;
                    truncatedPos = LocAndOffsets.FromInches(pos);
                }
            }
        }

        [TempleDllLocation(0x1003fca0)]
        public int AnimPathSearch(ref AnimPathData pathData)
        {
            return _animPathFinder.AnimPathSearch(ref pathData);
        }

        private const PathQueryFlags DefaultPathToFlags =
            PathQueryFlags.PQF_HAS_CRITTER | PathQueryFlags.PQF_TO_EXACT | PathQueryFlags.PQF_800 |
            PathQueryFlags.PQF_ADJ_RADIUS_REQUIRE_LOS | PathQueryFlags.PQF_ADJUST_RADIUS |
            PathQueryFlags.PQF_TARGET_OBJ;

        public bool CanPathTo(GameObjectBody obj, GameObjectBody target, PathQueryFlags flags = DefaultPathToFlags,
            float maxDistanceFeet = -1)
        {
            var from = obj.GetLocationFull();

            var pathQ = new PathQuery();
            pathQ.from = from;
            pathQ.flags = flags;
            var reach = obj.GetReach();
            pathQ.tolRadius = reach * 12.0f - 8.0f;
            pathQ.targetObj = target;
            if (Globals.Config.pathfindingDebugMode)
                Logger.Info("PF attempt to party member: {0}", target);
            pathQ.critter = obj;
            pathQ.distanceToTargetMin = reach;

            // TODO: This is a change vs. the old behavior and might break things???
            // Don't find a path if we're already in reach and have LOS
            if (obj.DistanceToObjInFeet(target) <= reach)
            {
                Logger.Info("Not performing pathfinding, because we're already in reach of the target.");
                return true;
            }

            if (!FindPath(pathQ, out var path))
            {
                return false;
            }

            if (maxDistanceFeet > 0)
            {
                var pathDist = path.GetPathResultLength();
                if (pathDist > maxDistanceFeet)
                {
                    return false;
                }
            }

            return true;
        }

        [TempleDllLocation(0x10057F80)]
        public GameObjectBody CanPathToParty(GameObjectBody obj, bool excludeUnconscious = true)
        {
            if (GameSystems.Party.IsInParty(obj))
                return null;
            foreach (var partyMember in GameSystems.Party.PartyMembers)
            {
                if (excludeUnconscious && GameSystems.Critter.IsDeadOrUnconscious(partyMember))
                    continue;
                if (CanPathTo(obj, partyMember))
                {
                    return partyMember;
                }
            }

            return null;
        }
    }
}