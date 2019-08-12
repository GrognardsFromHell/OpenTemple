using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Utils;
using SpicyTemple.Core.Systems.RadialMenus;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
namespace SpicyTemple.Core.Systems.D20.Conditions {

public static class RaceConditions {

private static readonly ILogger Logger = new ConsoleLogger();
[TempleDllLocation(0x102ef310)]
  public static readonly ConditionSpec Human = ConditionSpec.Create("Human", 0)
.Prevents(Human)
.AddHandler(DispatcherType.GetMoveSpeedBase, RacialMoveSpeed30)
.AddQueryHandler(D20DispatcherKey.QUE_FavoredClass, Dispatcher_Favored_Class_related)
                    .Build();


[TempleDllLocation(0x102ef368)]
  public static readonly ConditionSpec Dwarf = ConditionSpec.Create("Dwarf", 0)
.Prevents(Dwarf)
.AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CONSTITUTION, RacialStatModifier_callback, 2)
.AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CHARISMA, RacialStatModifier_callback, -2)
.AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_CONSTITUTION, RacialStatModifier_callback, 2)
.AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_CHARISMA, RacialStatModifier_callback, -2)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_FORTITUDE, sub_100FD900)
.AddHandler(DispatcherType.SaveThrowLevel, DwarfSaveBonusVsSpells)
.AddHandler(DispatcherType.ToHitBonus2, DwarfBonusToHitOrcsAndGoblins)
.AddHandler(DispatcherType.GetAC, ArmorBonusVsGiants)
.AddSkillLevelHandler(SkillId.appraise, DwarfAppraiseBonus, 2)
.AddHandler(DispatcherType.GetMoveSpeedBase, BaseMoveSpeed20)
.AddHandler(DispatcherType.GetMoveSpeed, DwarfMoveSpeed, 20, 139)
.AddHandler(DispatcherType.AbilityCheckModifier, CommonConditionCallbacks.AbilityModCheckStabilityBonus)
.AddQueryHandler(D20DispatcherKey.QUE_FavoredClass, CommonConditionCallbacks.D20QueryConditionHasHandler, 11)
                    .Build();


[TempleDllLocation(0x102ef4a0)]
  public static readonly ConditionSpec Elf = ConditionSpec.Create("Elf", 0)
.Prevents(Elf)
.AddHandler(DispatcherType.ConditionAddPre, ElvenConditionImmunity, SpellEffects.SpellSleep)
.AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_DEXTERITY, RacialStatModifier_callback, 2)
.AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CONSTITUTION, RacialStatModifier_callback, -2)
.AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_DEXTERITY, RacialStatModifier_callback, 2)
.AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_CONSTITUTION, RacialStatModifier_callback, -2)
.AddHandler(DispatcherType.SaveThrowLevel, ElfSavingThrowBonus)
.AddSkillLevelHandler(SkillId.listen, SkillBonusRacial, 2)
.AddSkillLevelHandler(SkillId.search, SkillBonusRacial, 2)
.AddSkillLevelHandler(SkillId.spot, SkillBonusRacial, 2)
.AddHandler(DispatcherType.GetMoveSpeedBase, RacialMoveSpeed30)
.AddQueryHandler(D20DispatcherKey.QUE_FavoredClass, CommonConditionCallbacks.D20QueryConditionHasHandler, 17)
                    .Build();


[TempleDllLocation(0x102ef5b0)]
  public static readonly ConditionSpec Gnome = ConditionSpec.Create("Gnome", 0)
.Prevents(Gnome)
.AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CONSTITUTION, RacialStatModifier_callback, 2)
.AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_STRENGTH, RacialStatModifier_callback, -2)
.AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_CONSTITUTION, RacialStatModifier_callback, 2)
.AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_STRENGTH, RacialStatModifier_callback, -2)
.AddHandler(DispatcherType.SaveThrowLevel, GnomeIllusionSaveBonus)
.AddHandler(DispatcherType.SpellDcMod, SpellDcMod_GnomeIllusionBonus_Callback)
.AddHandler(DispatcherType.GetAC, ArmorBonusVsGiants)
.AddSkillLevelHandler(SkillId.hide, sub_100FD8D0)
.AddSkillLevelHandler(SkillId.listen, SkillBonusRacial, 2)
.AddHandler(DispatcherType.ToHitBonus2, sub_100FDBA0)
.AddHandler(DispatcherType.GetMoveSpeedBase, BaseMoveSpeed20)
.AddQueryHandler(D20DispatcherKey.QUE_FavoredClass, CommonConditionCallbacks.D20QueryConditionHasHandler, 8)
                    .Build();


[TempleDllLocation(0x102ef6d0)]
  public static readonly ConditionSpec Halfelf = ConditionSpec.Create("Halfelf", 0)
.Prevents(Halfelf)
.AddHandler(DispatcherType.ConditionAddPre, ElvenConditionImmunity, SpellEffects.SpellSleep)
.AddHandler(DispatcherType.SaveThrowLevel, ElfSavingThrowBonus)
.AddSkillLevelHandler(SkillId.listen, SkillBonusRacial, 1)
.AddSkillLevelHandler(SkillId.search, SkillBonusRacial, 1)
.AddSkillLevelHandler(SkillId.spot, SkillBonusRacial, 1)
.AddSkillLevelHandler(SkillId.diplomacy, SkillBonusRacial, 2)
.AddSkillLevelHandler(SkillId.gather_information, SkillBonusRacial, 2)
.AddHandler(DispatcherType.GetMoveSpeedBase, RacialMoveSpeed30)
.AddQueryHandler(D20DispatcherKey.QUE_FavoredClass, Dispatcher_Favored_Class_related)
                    .Build();


[TempleDllLocation(0x102ef7b8)]
  public static readonly ConditionSpec Halforc = ConditionSpec.Create("Halforc", 0)
.Prevents(Halforc)
.AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_STRENGTH, RacialStatModifier_callback, 2)
.AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_INTELLIGENCE, RacialStatModifier_callback, -2)
.AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CHARISMA, RacialStatModifier_callback, -2)
.AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_STRENGTH, RacialStatModifier_callback, 2)
.AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_INTELLIGENCE, RacialStatModifier_callback, -2)
.AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_CHARISMA, RacialStatModifier_callback, -2)
.AddHandler(DispatcherType.GetMoveSpeedBase, RacialMoveSpeed30)
.AddQueryHandler(D20DispatcherKey.QUE_FavoredClass, CommonConditionCallbacks.D20QueryConditionHasHandler, 7)
                    .Build();


[TempleDllLocation(0x102ef888)]
  public static readonly ConditionSpec Halfling = ConditionSpec.Create("Halfling", 0)
.Prevents(Halfling)
.AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_DEXTERITY, RacialStatModifier_callback, 2)
.AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_STRENGTH, RacialStatModifier_callback, -2)
.AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_DEXTERITY, RacialStatModifier_callback, 2)
.AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_STRENGTH, RacialStatModifier_callback, -2)
.AddSkillLevelHandler(SkillId.hide, sub_100FD8D0)
.AddHandler(DispatcherType.SaveThrowLevel, HalflingSaveBonus)
.AddHandler(DispatcherType.SaveThrowLevel, HalflingWillSaveFear)
.AddSkillLevelHandler(SkillId.move_silently, SkillBonusRacial, 2)
.AddSkillLevelHandler(SkillId.listen, SkillBonusRacial, 2)
.AddHandler(DispatcherType.GetMoveSpeedBase, BaseMoveSpeed20)
.AddQueryHandler(D20DispatcherKey.QUE_FavoredClass, CommonConditionCallbacks.D20QueryConditionHasHandler, 15)
.AddHandler(DispatcherType.ToHitBonus2, HalflingThrownWeaponBonus)
                    .Build();


[TempleDllLocation(0x102ef9a8)]
  public static readonly ConditionSpec MonsterUndead = ConditionSpec.Create("Monster Undead", 0)
.Prevents(MonsterUndead)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_RACIAL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_RACIAL)
.AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 0, 0)
.Prevents(StatusEffects.Poisoned)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Poison, true)
.Prevents(StatusEffects.Paralyzed)
.Prevents(StatusEffects.Stunned)
.Prevents(StatusEffects.IncubatingDisease)
.Prevents(StatusEffects.NSDiseased)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Critical_Hits, true)
.AddHandler(DispatcherType.TakingDamage2, CommonConditionCallbacks.SubdualImmunityDamageCallback)
.Prevents(StatusEffects.TempAbilityLoss)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Energy_Drain, true)
.AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, UndeadHpChange)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Has_No_Con_Score, true)
                    .Build();


[TempleDllLocation(0x102efbe8)]
  public static readonly ConditionSpec MonsterSubtypeFire = ConditionSpec.Create("Monster Subtype Fire", 0)
.AddHandler(DispatcherType.TakingDamage2, SubtypeFireReductionAndVulnerability, 10, 8)
                    .Build();


[TempleDllLocation(0x102efaf0)]
  public static readonly ConditionSpec MonsterOoze = ConditionSpec.Create("Monster Ooze", 0)
.Prevents(MonsterOoze)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_RACIAL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_RACIAL)
.AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 1, 0)
.Prevents(StatusEffects.Poisoned)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Poison, true)
.Prevents(StatusEffects.Paralyzed)
.Prevents(StatusEffects.Stunned)
.Prevents(StatusEffects.IncubatingDisease)
.Prevents(StatusEffects.NSDiseased)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Critical_Hits, true)
.SetQueryResult(D20DispatcherKey.QUE_CanBeFlanked, false)
                    .Build();


public static IReadOnlyList<ConditionSpec> Conditions {get;} = new List<ConditionSpec>
{
MonsterSubtypeFire,
Dwarf,
Halfelf,
Halfling,
Elf,
MonsterUndead,
Halforc,
Human,
MonsterOoze,
Gnome,
};

[DispTypes(DispatcherType.GetMoveSpeed)]
[TempleDllLocation(0x100efec0)]
[TemplePlusLocation("generalfixes.cpp:76")]
public static void   DwarfMoveSpeed(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DwarfMoveSpeedSthg_0/*0x11eb68ea*/(evt);
}


[DispTypes(DispatcherType.StatBaseGet, DispatcherType.AbilityScoreLevel)]
[TempleDllLocation(0x100fd850)]
public static void   RacialStatModifier_callback(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  BonusList *v2;
  int v3;

  v1 = data;
  if ( !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Polymorphed) || evt.dispKey != D20DispatcherKey.STAT_STRENGTH && evt.dispKey != D20DispatcherKey.STAT_DEXTERITY && evt.dispKey != D20DispatcherKey.STAT_CONSTITUTION )
  {
    v2 = &evt.GetDispIoBonusList().bonlist;
    v3 = v1 + v2.OverallBonus();
    if ( v1 < 0 && v3 < 3 )
    {
      v1 = v3 - 3;
    }
    v2.AddBonus(v1, 0, 139);
  }
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100fdc00)]
public static void   HalflingSaveBonus(in DispatcherCallbackArgs evt)
{
  DispIoSavingThrow dispIo;

  dispIo = evt.GetDispIoSavingThrow();
  dispIo.bonlist.AddBonus(1, 0, 139);
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x100fd9f0)]
public static void   SkillBonusRacial(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  DispIoObjBonus dispIo;

  v1 = data;
  dispIo = evt.GetDispIoObjBonus();
  dispIo.bonOut.AddBonus(v1, 0, 139);
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100fddb0)]
public static void   UndeadHpChange(in DispatcherCallbackArgs evt)
{
  

  if ( (int)evt.objHndCaller.GetStat(Stat.hp_current, 0) <= 0 )
  {
    GameSystems.D20.Combat.Kill(evt.objHndCaller, null);
    GameSystems.ParticleSys.CreateAtObj("sp-Destroy Undead", evt.objHndCaller);
  }
}


[DispTypes(DispatcherType.GetMoveSpeedBase)]
[TempleDllLocation(0x100fdca0)]
public static void   BaseMoveSpeed20(in DispatcherCallbackArgs evt)
{
  DispIoMoveSpeed dispIo;

  dispIo = evt.GetDispIoMoveSpeed();
  dispIo.bonlist.AddBonus(20, 1, 139);
}


[DispTypes(DispatcherType.SpellDcMod)]
[TempleDllLocation(0x100fdb70)]
public static void   SpellDcMod_GnomeIllusionBonus_Callback(in DispatcherCallbackArgs evt)
{
  DispIOBonusListAndSpellEntry dispIo;

  dispIo = evt.GetDispIOBonusListAndSpellEntry();
  if ( dispIo.spellEntry.spellSchoolEnum == 6 )
  {
    dispIo.bonList.AddBonus(1, 0, 139);
  }
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100fd930)]
public static void   DwarfBonusToHitOrcsAndGoblins(in DispatcherCallbackArgs args)
{
  DispIoAttackBonus dispIo;
  int v2;
  int v3;

  dispIo = args.GetDispIoAttackBonus();
  v2 = dispIo.attackPacket.victim;
  v3 = HIDWORD(dispIo.attackPacket.victim);
  if ( dispIo.attackPacket.victim
    && GameSystems.Critter.IsCategory(__PAIR__(v3, v2), MonsterCategory.humanoid)
    && (GameSystems.Critter.IsCategorySubtype(__PAIR__(v3, v2), 0x10000) || GameSystems.Critter.IsCategorySubtype(__PAIR__(v3, v2), 0x80000)) )
  {
    dispIo.bonlist.AddBonus(1, 0, 139);
  }
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100fdc30)]
public static void   HalflingWillSaveFear(in DispatcherCallbackArgs evt)
{
  DispIoSavingThrow dispIo;

  dispIo = evt.GetDispIoSavingThrow();
  if ( dispIo.flags & 0x100000 )
  {
    dispIo.bonlist.AddBonus(2, 13, 139);
  }
}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100fde00)]
[TemplePlusLocation("ability_fixes.cpp:74")]
public static void   SubtypeFireReductionAndVulnerability(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoDamage dispIo;
  int damType;

  dispIo = evt.GetDispIoDamage();
  damType = data1;
  if ( !(dispIo.attackPacket.flags & D20CAF.SAVE_SUCCESSFUL) )
  {
    dispIo.damage.AddModFactor(2.0, (D20DT)data2, 129);
  }
  dispIo.damage.AddModFactor(0.0, (D20DT)damType, 104);
}
/* Orphan comments:
TP Replaced @ ability_fixes.cpp:74
*/


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100fdc70)]
[TemplePlusLocation("condition.cpp:517")]
public static void   HalflingThrownWeaponBonus(in DispatcherCallbackArgs args)
{
  DispIoAttackBonus dispIo;

  dispIo = args.GetDispIoAttackBonus();
  if ( dispIo.attackPacket.flags & D20CAF.THROWN )
  {
    dispIo.bonlist.AddBonus(1, 0, 139);
  }
}
/* Orphan comments:
TP Replaced @ condition.cpp:517
*/


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100fd900)]
public static void   sub_100FD900(in DispatcherCallbackArgs evt)
{
  DispIoSavingThrow dispIo;

  dispIo = evt.GetDispIoSavingThrow();
  if ( dispIo.flags & 8 )
  {
    dispIo.bonlist.AddBonus(2, 0, 139);
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100fdd40)]
public static void   Dispatcher_Favored_Class_related(in DispatcherCallbackArgs evt)
{
  int v1;
  int highest;
  Stat cls;
  int v4;
  DispIoD20Query dispIo;

  v1 = -1;
  dispIo = evt.GetDispIoD20Query();
  highest = (int)dispIo;
  cls = 7;
  do
  {
    v4 = evt.objHndCaller.GetStat(cls);
    if ( v4 > v1 )
    {
      v1 = v4;
      highest = cls;
    }
    ++cls;
  }
  while ( (int)cls < 18 );
  if ( *(_QWORD *)&dispIo.data1 == highest )
  {
    dispIo.return_val = 1;
  }
}


[DispTypes(DispatcherType.ConditionAddPre)]
[TempleDllLocation(0x100fdaa0)]
public static void   ElvenConditionImmunity(in DispatcherCallbackArgs evt, ConditionSpec data)
{
  DispIoCondStruct dispIo;

  dispIo = evt.GetDispIoCondStruct();
  if ( dispIo.condStruct == (ConditionSpec )data )
  {
    dispIo.outputFlag = 0;
    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 5059);
    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x1F, evt.objHndCaller, null);
  }
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100fdba0)]
public static void   sub_100FDBA0(in DispatcherCallbackArgs evt)
{
  DispIoAttackBonus dispIo;
  int v2;
  int v3;

  dispIo = evt.GetDispIoAttackBonus();
  v2 = dispIo.attackPacket.victim;
  v3 = HIDWORD(dispIo.attackPacket.victim);
  if ( dispIo.attackPacket.victim && GameSystems.Critter.IsCategory(__PAIR__(v3, v2), MonsterCategory.humanoid) )
  {
    if ( GameSystems.Critter.IsCategorySubtype(__PAIR__(v3, v2), 0x10000) )
    {
      dispIo.bonlist.AddBonus(1, 0, 139);
    }
  }
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100fdb30)]
public static void   GnomeIllusionSaveBonus(in DispatcherCallbackArgs evt)
{
  DispIoSavingThrow dispIo;

  dispIo = evt.GetDispIoSavingThrow();
  if ( dispIo.flags & 0x400 )
  {
    dispIo.bonlist.AddBonus(2, 31, 139);
  }
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x100fda20)]
public static void   DwarfAppraiseBonus(in DispatcherCallbackArgs args, int data)
{
  DispIoObjBonus dispIo;
  DispIoObjBonus evtObj_;
  int item;
  int v4;

  dispIo = args.GetDispIoObjBonus();
  evtObj_ = dispIo;
  item = dispIo.obj;
  if ( __PAIR__(HIDWORD(evtObj_.obj), item) )
  {
    v4 = __PAIR__(HIDWORD(evtObj_.obj), item).GetInt32(obj_f.material);
    if ( !v4 || v4 == 5 )
    {
      evtObj_.bonOut.AddBonus(2, 0, 139);
    }
  }
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100fdaf0)]
public static void   ElfSavingThrowBonus(in DispatcherCallbackArgs evt)
{
  DispIoSavingThrow dispIo;

  dispIo = evt.GetDispIoSavingThrow();
  if ( dispIo.flags & 0x100 )
  {
    dispIo.bonlist.AddBonus(2, 31, 139);
  }
}


[DispTypes(DispatcherType.GetMoveSpeedBase)]
[TempleDllLocation(0x100fdcd0)]
public static void   RacialMoveSpeed30(in DispatcherCallbackArgs args)
{
  DispIoMoveSpeed dispIo;

  dispIo = args.GetDispIoMoveSpeed();
  dispIo.bonlist.AddBonus(30, 1, 139);
}


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x100fd9a0)]
public static void   ArmorBonusVsGiants(in DispatcherCallbackArgs args)
{
  DispIoAttackBonus dispIo;
  DispIoAttackBonus v2;
  int v3;

  dispIo = args.GetDispIoAttackBonus();
  v2 = dispIo;
  v3 = dispIo.attackPacket.attacker;
  if ( __PAIR__(HIDWORD(v2.attackPacket.attacker), v3) )
  {
    if ( GameSystems.Critter.IsCategory(__PAIR__(HIDWORD(v2.attackPacket.attacker), v3), MonsterCategory.giant) )
    {
      v2.bonlist.AddBonus(4, 8, 139);
    }
  }
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x100fd8d0)]
public static void   sub_100FD8D0(in DispatcherCallbackArgs evt)
{
  DispIoObjBonus dispIo;

  dispIo = evt.GetDispIoObjBonus();
  dispIo.bonOut.AddBonus(4, 0, 139);
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100fda70)]
public static void   DwarfSaveBonusVsSpells(in DispatcherCallbackArgs args)
{
  DispIoSavingThrow dispIo;

  dispIo = args.GetDispIoSavingThrow();
  if ( dispIo.flags & 0x10 )
  {
    dispIo.bonlist.AddBonus(2, 0, 139);
  }
}

}
}
/*

DwarfMoveSpeedSthg_0 @ 0x11eb68ea = 1
*/