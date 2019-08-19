
await UiSystems.MainMenu.LaunchTutorial();
var leader = GameSystems.Party.GetLeader();
var rat = FindByName("Rat");
GameSystems.MapObject.Move(rat, leader.GetLocationFull());
