using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Linq;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.MapSector;

namespace OpenTemple.Core.IO.SaveGames.GameState;

public class SavedSectorState
{
    public List<SavedSectorTime> SectorTimes { get; set; } = new List<SavedSectorTime>();

    [TempleDllLocation(0x10081d20)]
    public static SavedSectorState Read(BinaryReader reader)
    {
        var result = new SavedSectorState();

        var timesCount = reader.ReadInt32();
        result.SectorTimes.Capacity = timesCount;

        for (var i = 0; i < timesCount; i++)
        {
            var sectorLoc = SectorLoc.Unpack(reader.ReadUInt64());
            var lastTime = reader.ReadGameTime();
            result.SectorTimes.Add(new SavedSectorTime(sectorLoc, lastTime));
        }

        return result;
    }

    [TempleDllLocation(0x10081d20)]
    [SuppressMessage("ReSharper", "RedundantCast")]
    public void Write(BinaryWriter writer)
    {
        writer.WriteInt32( SectorTimes.Count);
        foreach (var sectorTime in SectorTimes)
        {
            writer.Write(sectorTime.Sector.Pack());
            writer.WriteGameTime(sectorTime.Time);
        }
    }
}

public struct SavedSectorTime
{
    public readonly SectorLoc Sector;
    public readonly GameTime Time;

    public SavedSectorTime(SectorLoc sector, GameTime time)
    {
        Sector = sector;
        Time = time;
    }
}