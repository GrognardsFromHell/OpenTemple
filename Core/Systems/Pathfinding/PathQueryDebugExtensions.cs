using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;

namespace SpicyTemple.Core.Systems.Pathfinding
{
    public static class PathQueryDebugExtensions
    {

        // TODO: Remove later
        public static void Run()
        {
            PathQuery q = new PathQuery();
            q.critter = GameSystems.Party.GetLeader();
            q.@from = new LocAndOffsets(509, 457, -9.42809f, -9.42809f);
            q.to = new LocAndOffsets(511, 456, -1.4142139f, -1.4142139f);
            q.flags = (PathQueryFlags) 0xC0803;
            GameSystems.PathX.FindPath(q, out var r);
        }

        public static void SaveStateToFile(this PathQuery query, string path)
        {
            var root = new Dictionary<string, object>();
            var queryJson = new Dictionary<string, object>();
            queryJson["from"] = ToJson(query.from);
            queryJson["to"] = ToJson(query.to);
            queryJson["flags"] = (int) query.flags;
            queryJson["flags2"] = (int) query.flags2;
            if (query.critter != null)
            {
                queryJson["critterId"] = ToJson(query.critter.id);
                queryJson["critterName"] = GameSystems.MapObject.GetDisplayName(query.critter);
            }

            root["query"] = queryJson;

            var objPos = new List<object>();

            foreach (var obj in GameSystems.Object.EnumerateNonProtos())
            {
                if (obj.HasFlag(ObjectFlag.OFF) || !obj.id.IsPermanent || obj.HasFlag(ObjectFlag.INVENTORY))
                {
                    continue;
                }

                objPos.Add(new Dictionary<string, object>
                {
                    {"objectId", ToJson(obj.id)},
                    {"objectName", GameSystems.MapObject.GetDisplayName(obj)},
                    {"pos", ToJson(obj.GetLocationFull())}
                });
            }
            root["objects"] = objPos;

            var rootJson = JsonSerializer.SerializeToUtf8Bytes(root, new JsonSerializerOptions {WriteIndented = true});
            File.WriteAllBytes(path, rootJson);
        }

        private static string ToJson(ObjectId id)
        {
            return id.ToString();
        }

        private static Dictionary<string, object> ToJson(LocAndOffsets loc)
        {
            return new Dictionary<string, object>
            {
                {"x", loc.location.locx},
                {"y", loc.location.locy},
                {"offsetX", loc.off_y},
                {"offsetY", loc.off_y},
            };
        }
    }
}