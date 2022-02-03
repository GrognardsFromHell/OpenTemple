using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace OpenTemple.Core.IO.SaveGames.UiState;

public class SavedWorldmapUiState
{
    public List<SavedWorldmapLocation> Locations { get; set; } = new List<SavedWorldmapLocation>();

    // The x,y Position on the Worldmap *image* where the random encounter ocurred
    public Point RandomEncounterPoint { get; set; }

    public int RandomEncounterStatus { get; set; }

    public bool NeedToCleanEncounterMap { get; set; }

    public bool DontAskToExitEncounterMap { get; set; }

    [TempleDllLocation(0x1015e0f0)]
    public static SavedWorldmapUiState Read(BinaryReader reader)
    {
        var result = new SavedWorldmapUiState();

        var locationCount = DetectLocationCount(reader);

        var visitedCount = reader.ReadInt32();
        result.Locations.Capacity = visitedCount;

        for (var i = 0; i < locationCount; i++)
        {
            var visitedMap = SavedWorldmapLocation.Read(reader);

            if (i < visitedCount)
            {
                result.Locations.Add(visitedMap);
            }
        }

        result.RandomEncounterPoint = new Point(
            reader.ReadInt32(),
            reader.ReadInt32()
        );
        result.RandomEncounterStatus = reader.ReadInt32();
        result.NeedToCleanEncounterMap = reader.ReadInt32() != 0;
        result.DontAskToExitEncounterMap = reader.ReadInt32() != 0;

        return result;
    }

    private static int DetectLocationCount(BinaryReader reader)
    {
        // Sadly, Co8 uses a patched DLL with more locations, but there's no location count in the save itself
        // Co8 has 20 locations, Vanilla has 14.
        var pos = reader.BaseStream.Position;
        while (reader.ReadUInt32() != 0xBEEFCAFEu)
        {
        }

        var overallSize = reader.BaseStream.Position - pos;
        reader.BaseStream.Seek(pos, SeekOrigin.Begin);

        // 28 byte fixed size (see the reading routine above)
        var locationPayload = (int) (overallSize - 28);

        return locationPayload / 4;
    }

    [TempleDllLocation(0x101598b0)]
    public void Write(BinaryWriter writer, bool co8Extensions)
    {
        writer.WriteInt32(Locations.Count);

        // ToEE will always write out an array of a fixed length regardless of visited location count
        // The length of that array got patched by Co8 making the saves incompatible :-(
        var locationCount = co8Extensions ? 20 : 14;
        Trace.Assert(Locations.Count <= locationCount);

        foreach (var location in Locations)
        {
            location.Write(writer);
        }
        for (var i = Locations.Count; i < locationCount; i++)
        {
            writer.WriteInt32(0);
        }

        writer.WriteInt32(RandomEncounterPoint.X);
        writer.WriteInt32(RandomEncounterPoint.Y);
        writer.WriteInt32(RandomEncounterStatus);
        writer.WriteInt32(NeedToCleanEncounterMap ? 1 : 0);
        writer.WriteInt32(DontAskToExitEncounterMap ? 1 : 0);

    }
}

public readonly struct SavedWorldmapLocation
{
    // Location index of the worldmap location
    public readonly int Index;

    public readonly bool Discovered;

    public readonly bool Visited;

    public SavedWorldmapLocation(int index, bool discovered, bool visited)
    {
        Index = index;
        Discovered = discovered;
        Visited = visited;
    }

    public static SavedWorldmapLocation Read(BinaryReader reader)
    {
        var packed = reader.ReadInt32();

        return new SavedWorldmapLocation(
            packed >> 8,
            (packed & 1) != 0,
            (packed & 2) != 0
        );
    }

    public void Write(BinaryWriter writer)
    {
        // Pack the data into the 32-bit integer
        var packed = Index << 8;
        if (Discovered)
        {
            packed |= 1;
        }
        if (Visited)
        {
            packed |= 2;
        }

        writer.WriteInt32(packed);
    }
}