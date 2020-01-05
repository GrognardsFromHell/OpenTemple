using System.Collections.Generic;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;

namespace OpenTemple.Core.Systems
{
    public static class MapListParser
    {

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public static Dictionary<int, MapListEntry> Parse(IFileSystem fs, bool alwaysFog = false, bool alwaysUnfog = false)
        {
            var mapList = fs.ReadMesFile("rules/MapList.mes");
            var mapNames = fs.ReadMesFile("mes/map_names.mes");

            var mMaps = new Dictionary<int, MapListEntry>(mapList.Count);

            foreach (var (mapId, line) in mapList)
            {
                var entry = new MapListEntry();

                var parts = line.Split(',');

                entry.id = mapId;
                entry.name = parts[0];
                entry.startPosX = int.Parse(parts[1]);
                entry.startPosY = int.Parse(parts[2]);
                entry.flags = 0;
                if (alwaysUnfog)
                    entry.flags |= 4;

                // The rest are key value pairs
                for (var i = 3; i < parts.Length; ++i)
                {
                    var subParts = parts[i].Split(':');
                    var key = subParts[0].Trim();
                    var value = subParts[1].Trim();

                    switch (key)
                    {
                        case "Type":
                            switch (value)
                            {
                                case "NONE":
                                    entry.type = MapType.None;
                                    break;
                                case "START_MAP":
                                    entry.type = MapType.StartMap;
                                    break;
                                case "SHOPPING_MAP":
                                    entry.type = MapType.ShoppingMap;
                                    break;
                                case "TUTORIAL_MAP":
                                    entry.type = MapType.TutorialMap;
                                    break;
                            }

                            break;
                        case "WorldMap":
                            entry.worldmap = int.Parse(value);
                            break;
                        case "Area":
                            entry.area = int.Parse(value);
                            break;
                        case "Movie":
                            entry.movie = int.Parse(value);
                            break;
                        case "Flag":
                            switch (value)
                            {
                                case "DAYNIGHT_XFER":
                                    entry.flags |= 1;
                                    break;
                                case "OUTDOOR":
                                    entry.flags |= 2;
                                    break;
                                case "UNFOGGED":
                                {
                                    if (!alwaysFog)
                                        entry.flags |= 4;
                                    break;
                                }

                                case "BEDREST":
                                    entry.flags |= 8;
                                    break;
                            }

                            break;
                        default:
                            Logger.Warn("Unknown map key '{1}' for map {0}", mapId, key);
                            break;
                    }
                }

                // Copy the description from map_names.mes
                if (mapNames.TryGetValue(mapId, out var mapName))
                {
                    entry.description = mapName;
                }
                else
                {
                    Logger.Warn("Missing map description for {0}", mapId);
                    entry.description = entry.name;
                }

                mMaps[mapId] = entry;
            }

            // Get info from hardcoded map areas table (bleh)
            foreach (var entry in mMaps.Values)
            {
                if (entry.area == 0)
                {
                    entry.area = GetAreaForVanillaMap(entry.id);
                }
            }

            return mMaps;

        }

        private static int GetAreaForVanillaMap(int mapId)
        {
            int result;
            switch (mapId)
            {
                case 5000:
                case 5001:
                case 5006:
                case 5007:
                case 5008:
                case 5009:
                case 5010:
                case 5011:
                case 5012:
                case 5013:
                case 5014:
                case 5015:
                case 5016:
                case 5017:
                case 5018:
                case 5019:
                case 5020:
                case 5021:
                case 5022:
                case 5023:
                case 5024:
                case 5025:
                case 5026:
                case 5027:
                case 5028:
                case 5029:
                case 5030:
                case 5031:
                case 5032:
                case 5033:
                case 5034:
                case 5035:
                case 5036:
                case 5037:
                case 5038:
                case 5039:
                case 5040:
                case 5041:
                case 5042:
                case 5043:
                case 5044:
                case 5045:
                case 5046:
                case 5047:
                case 5048:
                case 5049:
                case 5063:
                case 5096:
                case 5097:
                case 5098:
                case 5099:
                case 5100:
                case 5101:
                case 5102:
                case 5103:
                case 5104:
                case 5115:
                    result = 1;
                    break;
                case 5002:
                case 5003:
                case 5004:
                case 5005:
                    result = 2;
                    break;
                case 5051:
                case 5052:
                case 5053:
                case 5054:
                case 5055:
                case 5056:
                case 5057:
                case 5058:
                case 5059:
                case 5060:
                case 5061:
                case 5085:
                case 5086:
                case 5087:
                case 5088:
                    result = 3;
                    break;
                case 5062:
                case 5064:
                case 5065:
                case 5066:
                case 5067:
                case 5078:
                case 5079:
                case 5080:
                case 5081:
                case 5082:
                case 5083:
                case 5084:
                case 5106:
                    result = 4;
                    break;
                case 5094:
                    result = 5;
                    break;
                case 5068:
                    result = 6;
                    break;
                case 5092:
                case 5093:
                    result = 7;
                    break;
                case 5091:
                    result = 8;
                    break;
                case 5095:
                case 5114:
                    result = 9;
                    break;
                case 5069:
                    result = 10;
                    break;
                case 5112:
                    result = 11;
                    break;
                case 5113:
                    result = 12;
                    break;
                default:
                    result = 0;
                    break;
            }

            return result;
        }

    }
}