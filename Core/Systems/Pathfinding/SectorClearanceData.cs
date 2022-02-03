using System;
using System.IO;
using System.Runtime.InteropServices;
using OpenTemple.Core.Systems.MapSector;

namespace OpenTemple.Core.Systems.Pathfinding;

public class SectorClearanceData
{
    private const int Dimension = Sector.SectorSideSize * 3;

    // in feet (matched to the Distance3d function return value)
    private readonly float[] _clearance = new float[Dimension * Dimension];

    public float GetClearance(int subtileX, int subtileY)
    {
        return _clearance[subtileY * Dimension + subtileX];
    }

    public void Load(BinaryReader reader)
    {
        var rawClearance = MemoryMarshal.Cast<float, byte>(_clearance.AsSpan());
        if (reader.Read(rawClearance) != rawClearance.Length)
        {
            throw new InvalidOperationException("Clearance data is corrupted.");
        }
    }
}