using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using OpenTemple.Core;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;

namespace ConvertMapToText;

public static class MapMobileConverter
{
    public static void ConvertMap(string toeeDir, int mapId)
    {
        using var game = HeadlessGame.Start(new HeadlessGameOptions(toeeDir));
        var mapList = MapListParser.Parse(Tig.FS);

        if (!mapList.TryGetValue(mapId, out var mapEntry))
        {
            Console.WriteLine("Unknown map: " + mapId);
            return;
        }

        ConvertMap(mapEntry);
    }

    public static void ConvertAllMaps(string toeeDir)
    {
        using var game = HeadlessGame.Start(new HeadlessGameOptions(toeeDir));

        var mapList = MapListParser.Parse(Tig.FS);
        foreach (var (mapId, mapEntry) in mapList)
        {
            Console.WriteLine($"Converting {mapId} ({mapEntry.name})");
            ConvertMap(mapEntry);
        }
    }

    public static void ConvertMap(MapListEntry map)
    {
        var mobiles = new List<SavedGameObject>();
        var mapDir = $"maps/{map.name}";
        foreach (var mobFile in Tig.FS.Search($"{mapDir}/*.mob"))
        {
            using var reader = Tig.FS.OpenBinaryReader(mobFile);
            mobiles.Add(SavedGameObject.Load(reader));
        }

        mobiles.Sort((a, b) => a.Id.guid.CompareTo(b.Id.guid));

        using var stream = new FileStream($"{map.id}_{map.name}_mobiles.json", FileMode.Create);
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Indented = true
        });

        writer.WriteStartArray();

        foreach (var mobile in mobiles)
        {
            WriteMobile(writer, mobile);
        }

        writer.WriteEndArray();
    }

    private static void WriteMobile(Utf8JsonWriter writer, SavedGameObject mobile)
    {
        writer.WriteStartObject();

        var proto = GameSystems.Proto.GetProtoById(mobile.ProtoId);
        var displayName = GameSystems.MapObject.GetDisplayName(proto);
        writer.WriteString("$comment", displayName);

        writer.WriteString("type", mobile.Type.ToString());
        writer.WriteString("id", mobile.Id.guid.ToString());
        writer.WriteNumber("protoId", mobile.ProtoId);
        
        ObjectSerializer.WriteProperties(writer, mobile.Properties, $"'{displayName}' ({mobile.Type})");

        writer.WriteEndObject();
    }
}