using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.VisualBasic;
using OpenTemple.Core;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.IO;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Utils;

namespace ConvertMapToText
{
    public class MapMobileConverter
    {
        public static void Convert(string toeeDir, int mapId)
        {
            using var game = HeadlessGame.Start(toeeDir);

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

            WritePermanentModData(writer, mobile.Properties);
            WriteNpcStandpoints(writer, mobile.Properties);

            foreach (var kvp in mobile.Properties.OrderBy(kvp => kvp.Key))
            {
                if (IgnoredFields.Contains(kvp.Key))
                {
                    continue;
                }

                writer.WriteField(kvp.Key, kvp.Value);
            }

            writer.WriteEndObject();
        }

        private static void WritePermanentModData(Utf8JsonWriter writer, Dictionary<obj_f, object> properties)
        {
            if (!properties.Remove(obj_f.permanent_mods, out var permanentModsObj))
            {
                properties.Remove(obj_f.permanent_mod_data);
                return;
            }

            writer.WriteStartArray("permanent_mod_args");

            var modData = (IReadOnlyList<int>) properties.GetValueOrDefault(obj_f.permanent_mod_data, new int[0]);
            properties.Remove(obj_f.permanent_mod_data);
            var modNames = (IReadOnlyList<int>) permanentModsObj;
            var argIdx = 0;
            foreach (var modNameHash in modNames)
            {
                var condition = GameSystems.D20.Conditions.GetByHash(modNameHash);

                if (condition.numArgs == 0)
                {
                    writer.WriteStringValue(condition.condName);
                    continue;
                }

                writer.WriteStartArray();
                writer.WriteStringValue(condition.condName);
                for (var i = 0; i < condition.numArgs; i++)
                {
                    writer.WriteNumberValue(modData[argIdx++]);
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();
        }

        private static void WriteNpcStandpoints(Utf8JsonWriter writer, Dictionary<obj_f, object> properties)
        {
            if (!properties.Remove(obj_f.npc_standpoints, out var standpointsObj))
            {
                return;
            }

            var standpoints = (IReadOnlyList<long>) standpointsObj;

            var standPointDay = GameObjectBody.DeserializeStandpoint(standpoints, 0);
            var standPointNight = GameObjectBody.DeserializeStandpoint(standpoints, 1);
            var standPointScout = GameObjectBody.DeserializeStandpoint(standpoints, 2);

            writer.WriteStartObject("npc_standpoints");
            writer.WriteStartObject("day");
            WriteStandPoint(writer, standPointDay);
            writer.WriteEndObject();

            writer.WriteStartObject("night");
            WriteStandPoint(writer, standPointNight);
            writer.WriteEndObject();

            if (standPointScout.location != LocAndOffsets.Zero)
            {
                writer.WriteStartObject("scout");
                WriteStandPoint(writer, standPointScout);
                writer.WriteEndObject();
            }

            writer.WriteEndObject();
        }

        private static void WriteStandPoint(Utf8JsonWriter writer, StandPoint standPoint)
        {
            writer.WriteNumber("mapId", standPoint.mapId);
            writer.WritePropertyName("location");
            writer.WriteTile(standPoint.location);
            writer.WriteNumber("jumpPointId", standPoint.jumpPointId);
        }

        private static readonly ISet<obj_f> IgnoredFields = new HashSet<obj_f>
        {
            // This just contains stale data, no idea why this was ever saved
            obj_f.dispatcher,
            obj_f.critter_inventory_num
        };
    }
}