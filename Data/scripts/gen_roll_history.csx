
var leader = GameSystems.Party.GetLeader();
var rat = FindByName("Rat");
if (rat == null) {
    rat = leader;
}
var leaderPos = leader.GetLocationFull();
leaderPos.location.locx += 1;

var attackerBonus = BonusList.Default;
attackerBonus.AddBonus(4, 0, 318, "Some Bonus");
var defenseBonus = BonusList.Default;
defenseBonus.AddBonus(4, 0, 318, "Some Bonus");
int histId;
histId = GameSystems.RollHistory.AddAttackRoll(15, 19, leader, rat, attackerBonus, defenseBonus, D20CAF.HIT);
GameSystems.RollHistory.CreateRollHistoryString(histId);

var dmg = new DamagePacket();
dmg.AddDamageDice(new Dice(1, 3), DamageType.Subdual, 113, "Nonlethal");
dmg.CalcFinalDamage();
histId = GameSystems.RollHistory.AddDamageRoll(leader, rat, dmg);
GameSystems.RollHistory.CreateRollHistoryString(histId);

histId = GameSystems.RollHistory.AddMiscBonus(leader, attackerBonus, 34, 15);
GameSystems.RollHistory.CreateRollHistoryString(histId);

histId = GameSystems.RollHistory.AddMiscCheck(leader, 20, "Misc Check", Dice.D20, 14, BonusList.Default);
GameSystems.RollHistory.CreateRollHistoryString(histId);

histId = GameSystems.RollHistory.AddOpposedCheck(leader, rat, 14, 20 ,in attackerBonus, in defenseBonus, 34, D20CombatMessage.spell_disrupted, 0);
GameSystems.RollHistory.CreateRollHistoryString(histId);

histId = GameSystems.RollHistory.AddPercentageCheck(leader, rat, 80, 34, 100, 34, 34);
GameSystems.RollHistory.CreateRollHistoryString(histId);

histId = GameSystems.RollHistory.AddSavingThrow(leader, 20, SavingThrowType.Fortitude, D20SavingThrowFlag.CHARM, Dice.D20, 19, attackerBonus);
GameSystems.RollHistory.CreateRollHistoryString(histId);

histId = GameSystems.RollHistory.AddSkillCheck(leader, null, SkillId.open_lock, Dice.D20, 19, 18, attackerBonus);
GameSystems.RollHistory.CreateRollHistoryString(histId);

var acBonus = BonusList.Default;
acBonus.AddBonus(10, 0, 102); // Initial Value
acBonus.AddBonus(7, 0, 124); // Armor
histId = GameSystems.RollHistory.AddTrapAttack(17, 0, 5, leader, acBonus, D20CAF.HIT);
GameSystems.RollHistory.CreateRollHistoryString(histId);
