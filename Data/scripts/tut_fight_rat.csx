
var leader = GameSystems.Party.GetLeader();
var rat = FindByName("Rat");
var leaderPos = leader.GetLocationFull();
leaderPos.location.locx += 1;
GameSystems.MapObject.Move(rat, leaderPos);
