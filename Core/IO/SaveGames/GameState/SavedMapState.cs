using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenTemple.Core.Ui.InGameSelect;
using Encoding = System.Text.Encoding;

namespace OpenTemple.Core.IO.SaveGames.GameState;

public class SavedMapState
{
    public string CurrentMapName { get; set; }

    public ISet<int> VisitedMaps { get; set; } = new HashSet<int>();

    [TempleDllLocation(0x10072C40)]
    public static SavedMapState Read(BinaryReader reader)
    {
        var result = new SavedMapState();

        // For some godforsaken reason they wrote these as newline-terminated strings...
        var mapDataDir = ReadDirectory(reader);
        ReadDirectory(reader); // Current save-directory for the map, which should match the other one

        mapDataDir = mapDataDir.Replace('\\', '/');
        result.CurrentMapName = Path.GetFileName(mapDataDir);

        var visitedMaps = reader.ReadIndexTable<int>();
        foreach (var (mapId, visited) in visitedMaps)
        {
            if (visited != 0)
            {
                result.VisitedMaps.Add(mapId);
            }
        }

        return result;
    }

    [TempleDllLocation(0x10072050)]
    public void Write(BinaryWriter writer)
    {
        WriteDirectory(writer, "maps\\" + CurrentMapName);
        WriteDirectory(writer, "Save\\Current\\maps\\" + CurrentMapName);

        var visitedMaps = VisitedMaps.ToDictionary(mapId => mapId, _ => 1);
        writer.WriteIndexTable(visitedMaps);
    }

    private static string ReadDirectory(BinaryReader reader)
    {
        Span<byte> result = stackalloc byte[260];
        var length = 0;
        var b = reader.ReadByte();
        while (b != 10)
        {
            result[length++] = b;
            b = reader.ReadByte();
        }

        return Encoding.Default.GetString(result.Slice(0, length));
    }

    private static void WriteDirectory(BinaryWriter writer, string value)
    {
        var encodedDir = Encoding.Default.GetBytes(value);
        writer.Write(encodedDir);
        writer.WriteUInt8(10); // The terminating newline
    }

}