using System;
using System.Linq;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Location;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems
{
    public class AreaSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem
    {
        private struct AreaEntry
        {
            public string Name;
            public string Description;
            public bool Known;
        }

        [TempleDllLocation(0x10aa94c4)]
        private AreaEntry[] _areas;

        // TODO: May be unused
        [TempleDllLocation(0x10aa94dc)]
        private int _areaLastDiscovered = -1;

        [TempleDllLocation(0x1006e550)]
        public AreaSystem()
        {
        }

        public void Dispose()
        {
        }

        [TempleDllLocation(0x1006e590)]
        public void LoadModule()
        {
            var areaMes = Tig.FS.ReadMesFile("mes/gamearea.mes");

            int areaCount = areaMes.Count;
            _areas = new AreaEntry[areaCount];

            // We're only reading the parts we think we need
            for (int i = 0; i < areaCount; i++)
            {
                var line = areaMes[i];
                var startOfName = line.IndexOf('/');
                var startOfDesc = line.IndexOf('/', startOfName + 1);
                if (startOfName == -1 || startOfDesc == -1)
                {
                    throw new Exception($"Area {i} has an invalid name or description.");
                }

                _areas[i].Name = line.Substring(startOfName + 1, startOfDesc - startOfName - 1).Trim();
                _areas[i].Description = line.Substring(startOfDesc + 1);
            }
        }

        [TempleDllLocation(0x1006e860)]
        public void UnloadModule()
        {
            _areas = null;
        }

        [TempleDllLocation(0x1006e560)]
        public void Reset()
        {
            for (var i = 0; i < _areas.Length; i++)
            {
                _areas[i].Known = false;
            }

            _areaLastDiscovered = -1;
        }

        [TempleDllLocation(0x1006e920)]
        public void SaveGame(SavedGameState savedGameState)
        {
            savedGameState.AreaState = new SavedAreaState
            {
                AreaDiscoveredLast = _areaLastDiscovered
            };
            for (var i = 0; i < _areas.Length; i++)
            {
                if (_areas[i].Known)
                {
                    savedGameState.AreaState.DiscoveredAreas.Add(i);
                }
            }
        }

        [TempleDllLocation(0x1006e8d0)]
        public void LoadGame(SavedGameState savedGameState)
        {
            var areaState = savedGameState.AreaState;

            _areaLastDiscovered = areaState.AreaDiscoveredLast;
            for (var i = 0; i < _areas.Length; i++)
            {
                _areas[i].Known = areaState.DiscoveredAreas.Contains(i);
            }
        }

        [TempleDllLocation(0x1006e9b0)]
        public bool IsAreaKnown(int area)
        {
            return _areas[area].Known;
        }

        [TempleDllLocation(0x1006ea00)]
        public void MakeAreaKnown(int area)
        {
            if (!_areas[area].Known)
            {
                GameUiBridge.AreaDiscovered(area);

                _areas[area].Known = true;
                _areaLastDiscovered = area;
            }
        }

        // TODO: I believe this is a Co8 patched version
        [TempleDllLocation(0x1006ec30)]
        public int GetAreaFromMap(int mapId)
        {
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
                    return 1;
                case 5002:
                case 5003:
                case 5004:
                case 5005:
                    return 2;
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
                    return 3;
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
                    return 4;
                case 5094:
                    return 5;
                case 5068:
                    return 6;
                case 5092:
                case 5093:
                    return 7;
                case 5091:
                    return 8;
                case 5095:
                case 5114:
                    return 9;
                case 5069:
                    return 10;
                case 5112:
                    return 11;
                case 5113:
                    return 12;
                case 5121:
                    return 14;
                case 5132:
                    return 15;
                case 5108:
                    return 16;
                default:
                    return 0;
            }
        }

        [TempleDllLocation(0x1006ed50)]
        public int GetCurrentArea()
        {
            var currentMap = GameSystems.Map.GetCurrentMapId();
            return GetAreaFromMap(currentMap);
        }

        [TempleDllLocation(0x1006e990)]
        public string GetAreaDescription(int id)
        {
            if (id < 0 || id >= _areas.Length)
            {
                return _areas[0].Description;
            }

            return _areas[id].Description;
        }
    }
}