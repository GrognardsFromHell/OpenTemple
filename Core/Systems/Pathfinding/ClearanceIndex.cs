using System.IO;

namespace OpenTemple.Core.Systems.Pathfinding
{
    public class ClearanceIndex
    {
        internal byte numSectors;
        public ushort[,] clrAddr; // sectorY, sectorX

        public ClearanceIndex()
        {
            Reset();
        }

        public void Load(BinaryReader reader)
        {
            numSectors = reader.ReadByte();
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    clrAddr[i,j] = reader.ReadUInt16();
                }
            }
        }

        public void Reset()
        {
            numSectors = 0;
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    clrAddr[i,j] = 0xFFFF;
                }
            }
        }

    }
}