using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using OpenTemple.Core.IO;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems.Pathfinding
{
	public class PathNodeSystem : IGameSystem, IResetAwareSystem
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private string _dataDir;
        private string _saveDir;

        private MapPathNode[] _pathNodes = Array.Empty<MapPathNode>();

        public void Dispose()
        {
        }

        [TempleDllLocation(0x100a9720)]
        public void SetDataDirs(string dataDir, string saveDir)
        {
            _dataDir = dataDir;
            _saveDir = saveDir;
        }

        [TempleDllLocation(0x100a9da0)]
        public void Reset()
        {
            _pathNodes = Array.Empty<MapPathNode>();
            ClearanceData = null;
        }

        [TempleDllLocation(0x100a9dd0)]
        public void Load(string dataDir, string saveDir)
        {
            Reset();
            SetDataDirs(dataDir, saveDir);

            LoadClearanceData();

            var nodesPath = $"{dataDir}/pathnodenew.pnd";
            if (!Tig.FS.FileExists(nodesPath))
            {
                nodesPath = $"{dataDir}/pathnode.pnd";
                if (!Tig.FS.FileExists(nodesPath))
                {
                    return;
                }
            }

            using var reader = Tig.FS.OpenBinaryReader(nodesPath);

            var distPath = $"{dataDir}/pathnodedist.pnd";
            using var distReader = Tig.FS.FileExists(distPath) ? Tig.FS.OpenBinaryReader(distPath) : null;

            int nodeCountSupplem = 0;
            if (distReader != null)
            {
                nodeCountSupplem = distReader.ReadInt32();
            }

            var nodeCount = reader.ReadInt32();
            _pathNodes = new MapPathNode[nodeCount];
            for (var i = 0; i < nodeCount; i++)
            {
                LoadNodeFromFile(reader, out _pathNodes[i]);

                if (nodeCountSupplem == nodeCount)
                {
                    LoadNeighDistFromFile(distReader, ref _pathNodes[i]);
                }
            }
        }

        private static void LoadNeighDistFromFile(BinaryReader reader, ref MapPathNode node)
        {
            var nodeId = reader.ReadInt32();

            if (nodeId != node.id)
            {
                throw new InvalidOperationException("Node distance data is corrupted (wrong node id).");
            }

            var neighbourCount = reader.ReadInt32();
            if (neighbourCount != node.neighbours.Length)
            {
                throw new InvalidOperationException("Node distance data is corrupted (wrong neighbour count).");
            }

            if (node.neighbours.Length == 0)
            {
                return;
            }

            var distancesRaw = MemoryMarshal.Cast<float, byte>(node.neighDistances.AsSpan());
            if (reader.Read(distancesRaw) != distancesRaw.Length)
            {
                throw new InvalidOperationException("Node distance data is corrupted (not enough distance data).");
            }

            node.flags |= PathNodeFlag.NEIGHBOUR_DISTANCES_SET;
        }

        private const int MaxNeighbours = 64;

        private static void LoadNodeFromFile(BinaryReader reader, out MapPathNode node)
        {
            node = new MapPathNode();
            node.id = reader.ReadInt32();
            node.nodeLoc = reader.ReadLocationAndOffsets();
            var neighCnt = reader.ReadInt32();

            if (neighCnt == 0)
            {
                // why are we holding nodes without neighbours? they should probably be culled...
                return;
            }

            if (neighCnt > MaxNeighbours)
            {
                Logger.Info("Too many neighbours for node {0}", node.id);
                Trace.Assert(neighCnt <= MaxNeighbours);
            }

            var neighbours = new int[neighCnt];
            var neighboursRaw = MemoryMarshal.Cast<int, byte>(neighbours);
            if (reader.Read(neighboursRaw) != neighboursRaw.Length)
            {
                throw new InvalidOperationException("Failed to read node neighbours");
            }

            node.neighbours = neighbours;

            node.neighDistances = new float[neighCnt];
        }

        private void LoadClearanceData()
        {
            var path = $"{_dataDir}/clearance.bin";
            if (!Tig.FS.FileExists(path))
            {
                return;
            }

            using var reader = Tig.FS.OpenBinaryReader(path);

            ClearanceData = new MapClearanceData();
            ClearanceData.Load(reader);
        }

        public bool HasClearanceData => ClearanceData != null;

        public MapClearanceData ClearanceData { get; private set; }

        [TempleDllLocation(0x100a96a0)]
        public bool FindClosestPathNode(LocAndOffsets location, out int nodeId)
        {
            var closestDist = float.MaxValue;
            var closestIndex = -1;

            for (var i = 0; i < _pathNodes.Length; i++)
            {
                ref var node = ref _pathNodes[i];
                var dist = node.nodeLoc.DistanceTo(location);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestIndex = i;
                }
            }

            if (closestIndex != -1)
            {
	            nodeId = _pathNodes[closestIndex].id;
	            return true;
            }
            else
            {
	            nodeId = -1;
	            return false;
            }
        }

        private struct FindPathNodeData
        {
	        public int nodeId;
	        public int refererId; // for the From node is -1; together with the "distCumul = -1" nodes will form a chain leading From -> To
	        public float distFrom; // distance to the From node; is set to 0 for the from node naturally :)
	        public float distTo; // distance to the To node;
	        public float distCumul; // can be set to -1
	        public float distActualTotal;
	        public float heuristic;
	        public bool usingActualDistance;
        }

        private const int MaxPathNodes = 30000; // hommlet has about 1000, so that should be enough! that would be 0.6MB

        private int fpbnCount = 0;
        private readonly FindPathNodeData[] fpbnData = new FindPathNodeData[MaxPathNodes];

        private void FindPathNodeAppend(ref FindPathNodeData node) {
	        fpbnData[fpbnCount++] = node;
        }

        private bool PopMinHeuristicNode(out FindPathNodeData fpndOut, bool useActualDist)
        {

	        if (!useActualDist)
	        {
		        return PopMinHeuristicNodeLegacy(out fpndOut);
	        }

	        int idxMin;
	        // find a node with a positive actual distance
	        for (idxMin = 0; idxMin < fpbnCount; idxMin++)
		        if (fpbnData[idxMin].distActualTotal >= 0.0)
			        break;
	        if (idxMin == fpbnCount)
	        {
		        fpndOut = default;
		        return false;
	        }

	        // search for minimum cumulative distance
	        float minDistCumul = fpbnData[idxMin].distActualTotal + fpbnData[idxMin].distTo;
	        for (int i = idxMin + 1; i < fpbnCount; i++)
	        {
		        if (fpbnData[i].distActualTotal >= 0.0
		            && ( fpbnData[i].distActualTotal + fpbnData[i].distTo ) < minDistCumul)
		        {
			        idxMin = i;
			        minDistCumul = fpbnData[idxMin].distActualTotal + fpbnData[idxMin].distTo;
		        }
	        }

	        // copy it out
	        fpndOut = fpbnData[idxMin];

	        // pop the found entry
	        fpbnData[idxMin] = fpbnData[fpbnCount - 1];
	        fpbnCount--;
	        return true;
        }

        private bool PopMinHeuristicNodeLegacy(out FindPathNodeData fpndOut)
        {
	        int idxMinCumul;
	        // find a node with a positive cumulative distance
	        for (idxMinCumul = 0; idxMinCumul < fpbnCount; idxMinCumul++)
		        if (fpbnData[idxMinCumul].distCumul >= 0.0)
			        break;
	        if (idxMinCumul == fpbnCount)
	        {
		        fpndOut = default;
		        return false;
	        }

	        // search for minimum cumulative distance
	        float minDistCumul = fpbnData[idxMinCumul].distCumul;
	        for (int i = idxMinCumul + 1; i < fpbnCount; i++)
	        {
		        if (fpbnData[i].distCumul >= 0.0
		            && fpbnData[i].distCumul < minDistCumul)
		        {
			        idxMinCumul = i;
			        minDistCumul = fpbnData[idxMinCumul].distCumul;
		        }
	        }

	        // copy it out
	        fpndOut = fpbnData[idxMinCumul];

	        // pop the found entry
	        fpbnData[idxMinCumul] = fpbnData[fpbnCount - 1];
	        fpbnCount--;
	        return true;
        }

        [TempleDllLocation(0x100a9e30)]
        public int FindPathBetweenNodes(int fromNodeId, int toNodeId, Span<int> nodeIds)
        {
			int chainLength = 0;

			fpbnCount = 0;

			// find the from/to nodes
			if (!GetPathNode(fromNodeId, out var fromNode))
			{
				return 0;
			}
			if (!GetPathNode(toNodeId, out var toNode))
			{
				return 0;
			}

			// determine if the pathnodes are using the supplemental information of Actual Travel Distance (NEW! TemplePlus only)
			// if so, the node distance evaluation algorithm will be quite different, taking into acconut the actual travel distance rather than the strange method employed by troika
			var useActualDistances = toNode.flags.HasFlag(PathNodeFlag.NEIGHBOUR_DISTANCES_SET) &&
			                         fromNode.flags.HasFlag(PathNodeFlag.NEIGHBOUR_DISTANCES_SET);

			// begin the A* algorithm
			float distFromTo = fromNode.nodeLoc.DistanceTo(toNode.nodeLoc) / locXY.INCH_PER_FEET;

			FindPathNodeData fpMinCumul;
			fpMinCumul.nodeId = fromNodeId;
			fpMinCumul.refererId = -1;
			fpMinCumul.distFrom = 0;
			fpMinCumul.distTo = distFromTo;
			fpMinCumul.distCumul = distFromTo;
			fpMinCumul.distActualTotal = 0;
			fpMinCumul.heuristic = 0;
			fpMinCumul.usingActualDistance = useActualDistances;
			FindPathNodeAppend(ref fpMinCumul);

			if (!PopMinHeuristicNode(out fpMinCumul, useActualDistances))
			{
				return 0;
			}

			FindPathNodeData fpTemp = default;

			while(fpMinCumul.nodeId != toNodeId)
			{
				// find the matching Path Node
				GetPathNode(fpMinCumul.nodeId, out var minCumulNode);

				// loop thru its neighbours, searching
				for (int i = 0; i < minCumulNode.neighbours.Length; i++)
				{
					fpTemp.refererId = fpMinCumul.nodeId;
					fpTemp.nodeId = minCumulNode.neighbours[i];

					// find the neighbour node
					int neighbourId = minCumulNode.neighbours[i];
					GetPathNode(neighbourId, out var neighNode);

					// calculate its heuristic
					fpTemp.distTo = neighNode.nodeLoc.DistanceTo(toNode.nodeLoc) / locXY.INCH_PER_FEET;
					fpTemp.distFrom = fromNode.nodeLoc.DistanceTo(neighNode.nodeLoc) / locXY.INCH_PER_FEET;
					fpTemp.distCumul = fpMinCumul.distCumul + fpTemp.distTo + fpTemp.distFrom;
					if (useActualDistances)
					{
						fpTemp.distActualTotal = fpMinCumul.distActualTotal + minCumulNode.neighDistances[i];
						fpTemp.heuristic = fpTemp.distActualTotal + fpTemp.distTo;
					}

					bool foundNode = false;
					for (var j = 0; j < fpbnCount; ++j) {
						ref var node = ref fpbnData[j];
						if (node.nodeId != fpTemp.nodeId) {
							continue;
						}

						foundNode = true;

						// Effectively the same path segment (TODO: Is getting here a bug already? Is it searching in cycles?)
						if (node.refererId == fpTemp.refererId || true) {
							if (useActualDistances) {
								if (fpTemp.distActualTotal + fpTemp.distTo < node.distActualTotal + node.distTo) {
									node = fpTemp;
									break;
								}
							}
							else {
								if (fpTemp.distCumul < node.distCumul) {
									node = fpTemp;
									break;
								}
							}
						}
						/*
						if (useActualDistances) {
							if (fpTemp.distActualTotal + fpTemp.distTo < node.distActualTotal + node.distTo) {
								FindPathNodeAppend(fpTemp);
								break;
							}
						} else {
							if (fpTemp.distCumul < node.distCumul) {
								FindPathNodeAppend(fpTemp);
								break;
							}
						}
						*/
					}

					// append node if it's not in the list yet
					if (!foundNode) {
						FindPathNodeAppend(ref fpTemp);
					}

				}

				fpMinCumul.distCumul = -1.0f;
				fpMinCumul.distActualTotal = -1.0f;
				FindPathNodeAppend(ref fpMinCumul);

				if (!PopMinHeuristicNode(out fpMinCumul, useActualDistances))
				{
					return 0;
				}
			}

			fpTemp = fpMinCumul;

			int refId0 = fpMinCumul.refererId; // the node leading up to the last node
			chainLength = 1;

			// get the chain length
			int refererId = fpTemp.refererId;
			while (refererId != -1) {
				var refererFound = false;
				for (int i = 0; i < fpbnCount; i++) {
					if (fpbnData[i].nodeId == refererId) {
						fpTemp = fpbnData[i];
						refererFound = true;
						break;
					}

				}
				if (!refererFound) {
					break;
					//shit
				}
				chainLength++;
				refererId = fpTemp.refererId;
			}

			if (chainLength >= nodeIds.Length)
			{
				return 0;
			}

			// dump node chain into nodeIds
			fpTemp = fpMinCumul;
			refererId = fpMinCumul.refererId;
			for (int i = chainLength -1 ; i >= 0 ; i--)
			{
				nodeIds[i] = fpTemp.nodeId;
				int refererFound = 0;
				for (int refIdx = 0; refIdx < fpbnCount; refIdx++)
				{
					if (fpbnData[refIdx].nodeId == refererId)
					{
						fpTemp = fpbnData[refIdx];
						refererId = fpTemp.refererId;
						refererFound = 1;
						break;
					}

				}
				if (i == 0 && refererId != -1)
				{
					int breakPointDummy = 1;
				}
			}

			return chainLength;
        }

        // TODO: This seems stupid. We already have indices and the loops dealing with IDs do not mutate the array...
        public bool GetPathNode(int nodeId, out MapPathNode nodeOut)
        {
	        foreach (var node in _pathNodes)
	        {
		        if (node.id == nodeId)
		        {
			        nodeOut = node;
			        return true;
		        }
	        }

	        nodeOut = default;
	        return false;
        }

    }
}