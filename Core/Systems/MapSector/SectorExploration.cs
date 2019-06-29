using System;
using System.Diagnostics;
using System.IO;

namespace SpicyTemple.Core.Systems.MapSector
{
    public class SectorExploration
    {
        private const int SectorExplorationDataSize = 64 * 64 * 3 * 3 / 8;

        public SectorExplorationState State { get; private set; } = SectorExplorationState.Unexplored;

        private readonly byte[] _subtileBitmap = new byte[SectorExplorationDataSize];

        // The number of bytes per row of explored sector data (it's one bit per subtile)
        private const int BitmapStride = (Sector.SectorSideSize * 3) / 8;

        public bool IsExplored(int x, int y)
        {
            Debug.Assert(x >= 0 && x < 192);
            Debug.Assert(y >= 0 && y < 192);

            if (State == SectorExplorationState.AllExplored)
            {
                return true;
            }

            var mask = (byte) (1 << (x % 8));

            var index = y * BitmapStride + x / 8;
            return (_subtileBitmap[index] & mask) != 0;
        }

        public void Load(Stream stream, string filename)
        {
            var overallStatus = stream.ReadByte();
            if (overallStatus == 1)
            {
                _subtileBitmap.AsSpan().Fill(0xFF);
                State = SectorExplorationState.AllExplored;
                return;
            }

            State = SectorExplorationState.PartiallyExplored;
            if (stream.Read(_subtileBitmap) != _subtileBitmap.Length)
            {
                throw new InvalidOperationException($"Failed to load sector exploration {filename}.");
            }
        }

        public void Save(Stream stream)
        {
            Trace.Assert(State != SectorExplorationState.Unexplored);
            Trace.Assert(_subtileBitmap.Length == SectorExplorationDataSize);

            // First determine whether the entirety of the sector has been explored
            var allExplored = true;
            foreach (var i in _subtileBitmap)
            {
                if (i != 0xFF)
                {
                    allExplored = false;
                    break;
                }
            }

            stream.WriteByte((byte) (allExplored ? 1 : 0));

            if (!allExplored)
            {
                stream.Write(_subtileBitmap);
            }
        }
    }
}