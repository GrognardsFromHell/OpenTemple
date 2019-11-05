using System;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems
{
    public class GameInitSystem : IGameSystem, IModuleAwareSystem, IResetAwareSystem
    {
        [TempleDllLocation(0x10aa327c)]
        private const bool IsEditor = false;

        [TempleDllLocation(0x1004c610)]
        public GameInitSystem()
        {
        }

        [TempleDllLocation(0x1004c690)]
        public void Dispose()
        {
        }

        [TempleDllLocation(0x1004c6a0)]
        public void LoadModule()
        {
            if (IsEditor)
            {
                return;
            }

            var gameinitMes = Tig.FS.ReadMesFile("rules/gameinit.mes");

            var startingYear = int.Parse(gameinitMes[4]);
            var startingHourOfDay = int.Parse(gameinitMes[5]);
            var startingDay = int.Parse(gameinitMes[6]);

            GameSystems.TimeEvent.SetStartingTime(startingYear, startingDay, startingHourOfDay);

            Reset();
        }

        [TempleDllLocation(0x1004c850)]
        public void UnloadModule()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1004c660)]
        public void Reset()
        {
            if (!IsEditor)
            {
                var mapId = GameSystems.Map.GetMapIdByType(MapType.ShoppingMap);
                if (mapId == 0)
                {
                    mapId = GameSystems.Map.GetMapIdByType(MapType.StartMap);
                }

                GameSystems.Map.OpenMap(mapId, false, true);
            }
        }
    }
}