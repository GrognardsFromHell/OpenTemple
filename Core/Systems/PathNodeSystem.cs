using System;
using SpicyTemple.Core.Systems.MapSector;

namespace SpicyTemple.Core.Systems
{

    public class ClearanceIndex
    {
        private byte numSectors;
        public ushort[,] clrAddr; // sectorY, sectorX

        public ClearanceIndex()
        {
            Reset();
        }

        public void Reset()
        {
            numSectors = 0;
            for (int i = 0; i < 16 * 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    clrAddr[i,j] = 0xFFFF;
                }
            }
        }

    }

    public class SectorClearanceData
    {
        // in feet (matched to the Distance3d function return value)
        private float[,] val = new float[64 * 3, 64 * 3];

        public float GetClearance(int subtileX, int subtileY)
        {
            return val[subtileX, subtileY];
        }
    }

    public class MapClearanceData
    {
        private ClearanceIndex clrIdx;
        private SectorClearanceData[] secClr;

        public SectorClearanceData GetSectorClearance(SectorLoc loc)
        {
            var sectorIdx = clrIdx.clrAddr[loc.X, loc.Y];
            return secClr[sectorIdx];
        }
    }

    public class PathNodeSystem : IGameSystem
    {
        public void Dispose()
        {
        }

        [TempleDllLocation(0x100a9720)]
        public void SetDataDirs(string dataDir, string saveDir)
        {
            // TODO
        }

        public void Load(string dataDir, string saveDir)
        {
            // TODO
        }

        public bool HasClearanceData => false;

        public MapClearanceData ClearanceData { get; }

    }
}