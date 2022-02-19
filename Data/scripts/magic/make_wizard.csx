
var leader = GameSystems.Party.GetLeader();

for (var i = 0; i < 6; i++) {
    AutoLevelUp.LevelUp(leader, Stat.level_wizard);
}
