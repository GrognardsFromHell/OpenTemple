using System;
using OpenTemple.Core.IO;
using OpenTemple.Core.Logging;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems;

public class GameInitSystem : IGameSystem, IModuleAwareSystem, IResetAwareSystem
{
    [TempleDllLocation(0x10aa327c)]
    public bool EnableStartMap { get; set; } = true;

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
        SetupStartingTime();
        OpenStartMap();
    }

    [TempleDllLocation(0x1004c850)]
    public void UnloadModule()
    {
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x1004c660)]
    public void Reset()
    {
        OpenStartMap();
    }

    public void SetupStartingTime()
    {
        var gameinitMes = Tig.FS.ReadMesFile("rules/gameinit.mes");

        var startingYear = int.Parse(gameinitMes[4]);
        var startingHourOfDay = int.Parse(gameinitMes[5]);
        var startingDay = int.Parse(gameinitMes[6]);

        GameSystems.TimeEvent.SetStartingTime(startingYear, startingDay, startingHourOfDay);
    }

    public void OpenStartMap(bool force = false)
    {
        if (!EnableStartMap && !force)
        {
            GameSystems.Map.CloseMap();
            return;
        }

        var mapId = GameSystems.Map.GetMapIdByType(MapType.ShoppingMap);
        if (mapId == 0)
        {
            mapId = GameSystems.Map.GetMapIdByType(MapType.StartMap);
        }

        GameSystems.Map.OpenMap(mapId, false, true);
    }
}