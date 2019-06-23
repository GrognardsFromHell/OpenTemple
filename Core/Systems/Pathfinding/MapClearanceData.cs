using System.IO;
using SpicyTemple.Core.Systems.MapSector;

namespace SpicyTemple.Core.Systems.Pathfinding
{
    public class MapClearanceData
    {
        private ClearanceIndex clrIdx;
        private SectorClearanceData[] secClr;

        public SectorClearanceData GetSectorClearance(SectorLoc loc)
        {
            var sectorIdx = clrIdx.clrAddr[loc.X, loc.Y];
            return secClr[sectorIdx];
        }

        public void Load(BinaryReader reader)
        {
            clrIdx = new ClearanceIndex();
            clrIdx.Load(reader);

            secClr = new SectorClearanceData[clrIdx.numSectors];
            for (int i = 0; i < secClr.Length; i++)
            {
                secClr[i] = new SectorClearanceData();
                secClr[i].Load(reader);
            }
        }
    }
}