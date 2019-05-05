using System;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems
{
    public class AreaSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem
    {
        private struct AreaEntry
        {
            public string Name;
            public string Description;
            public bool Known;
        }

        private AreaEntry[] _areas;

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
        }

        [TempleDllLocation(0x1006e920)]
        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1006e8d0)]
        public bool LoadGame()
        {
            throw new NotImplementedException();
        }
    }
}