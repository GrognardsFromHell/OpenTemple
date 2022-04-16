
var leader = GameSystems.Party.GetLeader();

for (var i = 0; i < 6; i++) {
    AutoLevelUp.LevelUp(leader, Stat.level_wizard);
}

foreach (var featId in GameSystems.Spell.MetamagicFeats) {
    GameSystems.Feat.AddFeat(leader, featId);
}
