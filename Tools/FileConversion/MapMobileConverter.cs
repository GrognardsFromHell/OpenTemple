using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.VisualBasic;
using OpenTemple.Core;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Utils;

namespace ConvertMapToText;

public class MapMobileConverter
{
    public static void Convert(string toeeDir, int mapId)
    {
        using var game = HeadlessGame.Start(new HeadlessGameOptions(toeeDir));

        var mapList = MapListParser.Parse(Tig.FS);

        if (!mapList.TryGetValue(mapId, out var mapEntry))
        {
            Console.WriteLine("Unknown map: " + mapId);
            return;
        }

        var mobiles = new List<SavedGameObject>();
        var mapDir = $"maps/{mapEntry.name}";
        foreach (var mobFile in Tig.FS.Search($"{mapDir}/*.mob"))
        {
            using var reader = Tig.FS.OpenBinaryReader(mobFile);
            mobiles.Add(SavedGameObject.Load(reader));
        }

        mobiles.Sort((a, b) => a.Id.guid.CompareTo(b.Id.guid));

        using var stream = new FileStream($"{mapId}_mobiles.json", FileMode.Create);
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
        writer.WriteString("$comment", GameSystems.MapObject.GetDisplayName(proto));

        writer.WriteString("type", mobile.Type.ToString());
        writer.WriteString("id", mobile.Id.guid.ToString());
        writer.WriteNumber("protoId", mobile.ProtoId);

        ObjectSerializer.WriteProperties(writer, mobile.Type, mobile.Properties);

        writer.WriteEndObject();
    }

}