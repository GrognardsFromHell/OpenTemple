
// await UiSystems.MainMenu.LaunchTutorial();

// OpenTemple.Core.DebugUI.ObjectEditors.Edit(GameSystems.Party.GetLeader());

// Make all areas known
//foreach (var areaId in GameSystems.Area.EnumerateAreaIds()) {
//    GameSystems.Area.MakeAreaKnown(areaId);
//}

GameSystems.Map.MarkVisitedMap(5001);
GameSystems.Map.MarkVisitedMap(5080);

try {
    UiSystems.TownMap.Show();
    } catch {
    }
UiSystems.TownMap.ChangeCurrentMap(5080);
