
var leader = GameSystems.Party.GetLeader();
var chest = FindByName("Tutorial Chest A");
var chestPos = chest.GetLocationFull();
chestPos.location.locx += 1;
chestPos.off_x += 6;
GameSystems.MapObject.Move(leader, chestPos);
UiSystems.InGame.CenterOnParty();

if (GameSystems.Anim.PushUseSkillOn(leader, chest, SkillId.open_lock)) {
    GameSystems.Anim.DebugLastPushedSlot();
} else {
    Print("Couldn't push goal!");
}
