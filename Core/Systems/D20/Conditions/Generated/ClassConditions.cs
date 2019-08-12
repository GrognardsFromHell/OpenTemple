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

public static class ClassConditions {

private static readonly ILogger Logger = new ConsoleLogger();
[TempleDllLocation(0x102eff08)]
  public static readonly ConditionSpec Barbarian = ConditionSpec.Create("Barbarian", 0)
.Prevents(Barbarian)
.AddHandler(DispatcherType.ToHitBonusBase, ClassBabMartial, 7)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_FORTITUDE, ClassSavingThrowGood, 7)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_REFLEX, ClassSavingThrowCrap, 7)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_WILL, ClassSavingThrowCrap, 7)
.AddHandler(DispatcherType.GetAC, TrapSenseDodgeBonus, 7)
.AddHandler(DispatcherType.GetAC, D20DispatcherKey.SAVE_REFLEX, TrapSenseRefSaveBonus, 7)
.AddHandler(DispatcherType.TakingDamage2, BarbarianDRDamageCallback)
                    .Build();


[TempleDllLocation(0x102effc8)]
  public static readonly ConditionSpec Bard = ConditionSpec.Create("Bard", 0)
.Prevents(Bard)
.AddHandler(DispatcherType.ToHitBonusBase, ClassSemiMartialToHitBonus, 8)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_FORTITUDE, ClassSavingThrowCrap, 8)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_REFLEX, ClassSavingThrowGood, 8)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_WILL, ClassSavingThrowGood, 8)
                    .Build();


[TempleDllLocation(0x102f0048)]
  public static readonly ConditionSpec Cleric = ConditionSpec.Create("Cleric", 0)
.Prevents(Cleric)
.AddHandler(DispatcherType.ToHitBonusBase, ClassSemiMartialToHitBonus, 9)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_FORTITUDE, ClassSavingThrowGood, 9)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_REFLEX, ClassSavingThrowCrap, 9)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_WILL, ClassSavingThrowGood, 9)
                    .Build();


[TempleDllLocation(0x102f00c8)]
  public static readonly ConditionSpec Druid = ConditionSpec.Create("Druid", 0)
.Prevents(Druid)
.AddHandler(DispatcherType.ToHitBonusBase, ClassSemiMartialToHitBonus, 10)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_FORTITUDE, ClassSavingThrowGood, 10)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_REFLEX, ClassSavingThrowCrap, 10)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_WILL, ClassSavingThrowGood, 10)
                    .Build();


[TempleDllLocation(0x102f0148)]
  public static readonly ConditionSpec Fighter = ConditionSpec.Create("Fighter", 0)
.Prevents(Fighter)
.AddHandler(DispatcherType.ToHitBonusBase, ClassBabMartial, 11)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_FORTITUDE, ClassSavingThrowGood, 11)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_REFLEX, ClassSavingThrowCrap, 11)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_WILL, ClassSavingThrowCrap, 11)
                    .Build();


[TempleDllLocation(0x102f01c8)]
  public static readonly ConditionSpec Monk = ConditionSpec.Create("Monk", 0)
.Prevents(Monk)
.AddHandler(DispatcherType.ToHitBonusBase, ClassSemiMartialToHitBonus, 12)
.AddHandler(DispatcherType.GetAC, MonkAcBonus, 12)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_FORTITUDE, ClassSavingThrowGood, 12)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_REFLEX, ClassSavingThrowGood, 12)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_WILL, ClassSavingThrowGood, 12)
                    .Build();


[TempleDllLocation(0x102f0260)]
  public static readonly ConditionSpec Paladin = ConditionSpec.Create("Paladin", 0)
.Prevents(Paladin)
.AddHandler(DispatcherType.ToHitBonusBase, ClassBabMartial, 13)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_FORTITUDE, ClassSavingThrowGood, 13)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_REFLEX, ClassSavingThrowCrap, 13)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_WILL, ClassSavingThrowCrap, 13)
                    .Build();


[TempleDllLocation(0x102f02e0)]
  public static readonly ConditionSpec Ranger = ConditionSpec.Create("Ranger", 0)
.Prevents(Ranger)
.AddHandler(DispatcherType.ToHitBonusBase, ClassBabMartial, 14)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_FORTITUDE, ClassSavingThrowGood, 14)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_REFLEX, ClassSavingThrowGood, 14)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_WILL, ClassSavingThrowCrap, 14)
                    .Build();


[TempleDllLocation(0x102f0360)]
  public static readonly ConditionSpec Rogue = ConditionSpec.Create("Rogue", 0)
.Prevents(Rogue)
.AddHandler(DispatcherType.ToHitBonusBase, ClassSemiMartialToHitBonus, 15)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_FORTITUDE, ClassSavingThrowCrap, 15)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_REFLEX, ClassSavingThrowGood, 15)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_WILL, ClassSavingThrowCrap, 15)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Can_Find_Traps, true)
.AddHandler(DispatcherType.GetAC, TrapSenseDodgeBonus, 15)
.AddHandler(DispatcherType.GetAC, D20DispatcherKey.SAVE_REFLEX, TrapSenseRefSaveBonus, 15)
                    .Build();


[TempleDllLocation(0x102f0420)]
  public static readonly ConditionSpec Sorcerer = ConditionSpec.Create("Sorcerer", 0)
.Prevents(Sorcerer)
.AddHandler(DispatcherType.ToHitBonusBase, sub_100FE020, 16)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_FORTITUDE, ClassSavingThrowCrap, 16)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_REFLEX, ClassSavingThrowCrap, 16)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_WILL, ClassSavingThrowGood, 16)
                    .Build();


[TempleDllLocation(0x102f04a0)]
  public static readonly ConditionSpec Wizard = ConditionSpec.Create("Wizard", 0)
.Prevents(Wizard)
.AddHandler(DispatcherType.ToHitBonusBase, sub_100FE020, 17)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_FORTITUDE, ClassSavingThrowCrap, 17)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_REFLEX, ClassSavingThrowCrap, 17)
.AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_WILL, ClassSavingThrowGood, 17)
                    .Build();


[TempleDllLocation(0x102f0520)]
  public static readonly ConditionSpec BardicMusic = ConditionSpec.Create("Bardic Music", 6)
.Prevents(BardicMusic)
.AddHandler(DispatcherType.ConditionAdd, BardicMusicInitCallback, 0)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, BardicMusicInitCallback, 1)
.AddHandler(DispatcherType.RadialMenuEntry, BardicMusicRadial)
.AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_COPY_SCROLL, CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
.AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_COPY_SCROLL, BardicMusicCheck, 0)
.AddHandler(DispatcherType.D20ActionOnActionFrame, D20DispatcherKey.D20A_COPY_SCROLL, BardicMusicActionFrame)
.AddHandler(DispatcherType.BeginRound, BardicMusicBeginRound)
.AddSignalHandler(D20DispatcherKey.SIG_Sequence, BardicMusicOnSequence)
.RemoveOnSignal(D20DispatcherKey.SIG_Killed)
                    .Build();


[TempleDllLocation(0x102f0604)]
  public static readonly ConditionSpec SchoolSpecialization = ConditionSpec.Create("School Specialization", 0)
.AddSkillLevelHandler(SkillId.spellcraft, SchoolSpecializationSkillLevel)
                    .Build();


public static IReadOnlyList<ConditionSpec> Conditions {get;} = new List<ConditionSpec>
{
Cleric,
Fighter,
SchoolSpecialization,
Wizard,
Druid,
Monk,
Ranger,
Rogue,
Bard,
Paladin,
Barbarian,
BardicMusic,
Sorcerer,
};

[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x100fedd0)]
public static void   MonkAcBonus(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  int v2;
  BonusList *v3;
  int v4;
  CHAR v5;
  int v6;
  int v7;

  if ( sub_100FECE0/*0x100fece0*/(evt.objHndCaller) )
  {
    v1 = evt.objHndCaller.GetStat(Stat.wisdom);
    v2 = D20StatSystem.GetModifierForAbilityScore(v1);
    if ( (v2 )!=0)
    {
      v3 = &evt.GetDispIoAttackBonus().bonlist;
      v3.AddBonus(v2, 0, 310);
      v4 = evt.objHndCaller.GetStat(Stat.level_monk) / 5;
      GameSystems.Item.IsProtoWornAt(evt.objHndCaller, 15, 12420, 0);
      v6 = v7;
      if ( v5 )
      {
        v6 = v7 + 1;
      }
      if ( v6 > 0 )
      {
        v3.AddBonus(v6, 0, 311);
        JUMPOUT(*(_DWORD *)&byte_100FEE4F/*0x100fee4f*/);
      }
    }
  }
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100fe070)]
public static void   ClassSavingThrowGood(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  DispIoSavingThrow dispIo;

  v1 = evt.objHndCaller.GetStat((Stat)data);
  dispIo = evt.GetDispIoSavingThrow();
  dispIo.bonlist.AddBonus((v1 + 4) / 2, 0, 137);
}


[DispTypes(DispatcherType.ToHitBonusBase)]
[TempleDllLocation(0x100fdfd0)]
public static void   ClassSemiMartialToHitBonus(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  DispIoAttackBonus dispIo;

  v1 = evt.objHndCaller.GetStat((Stat)data);
  dispIo = evt.GetDispIoAttackBonus();
  dispIo.bonlist.AddBonus(3 * v1 / 4, 0, 137);
}


[DispTypes(DispatcherType.D20ActionCheck)]
[TempleDllLocation(0x100fe470)]
[TemplePlusLocation("condition.cpp:486")]
public static void   BardicMusicCheck(in DispatcherCallbackArgs evt, int data)
{
  DispIoD20ActionTurnBased dispIo;
  D20Action v2;

  dispIo = evt.GetDispIoD20ActionTurnBased();
  v2 = dispIo.action;
  if ( PerformSkillLevelSufficient/*0x100fe110*/(evt.objHndCaller, v2.data1) )
  {
    if ( evt.GetConditionArg2() == v2.data1 )
    {
      dispIo.returnVal = 14;
    }
    else if ( evt.GetConditionArg1() <= data )
    {
      dispIo.returnVal = 16;
    }
  }
  else
  {
    dispIo.returnVal = 14;
  }
}
/* Orphan comments:
TP Replaced @ condition.cpp:486
*/


[DispTypes(DispatcherType.ToHitBonusBase)]
[TempleDllLocation(0x100fe020)]
public static void   sub_100FE020(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  DispIoAttackBonus dispIo;

  v1 = evt.objHndCaller.GetStat((Stat)data);
  dispIo = evt.GetDispIoAttackBonus();
  dispIo.bonlist.AddBonus(v1 / 2, 0, 137);
}


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x100feac0)]
public static void   TrapSenseDodgeBonus(in DispatcherCallbackArgs evt, int data)
{
  int classLvl;
  DispIoAttackBonus dispIo;
  D20CAF v3;

  classLvl = evt.objHndCaller.GetStat((Stat)data) / 3;
  if ( classLvl >= 1 )
  {
    dispIo = evt.GetDispIoAttackBonus();
    v3 = dispIo.attackPacket.flags;
    if ( ((v3 & D20CAF.TRAP)!=0) )
    {
      dispIo.bonlist.AddBonus(classLvl, 8, 280);
    }
  }
}


[DispTypes(DispatcherType.ToHitBonusBase)]
[TempleDllLocation(0x100fdf90)]
public static void   ClassBabMartial(in DispatcherCallbackArgs evt, int data)
{
  int classLvl;
  DispIoAttackBonus dispIo;

  classLvl = evt.objHndCaller.GetStat((Stat)data);
  dispIo = evt.GetDispIoAttackBonus();
  dispIo.bonlist.AddBonus(classLvl, 0, 137);
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100fe9b0)]
public static void   BardicMusicOnSequence(in DispatcherCallbackArgs evt)
{
  int interruptMusic;
  int condArg2;
  ActionSequence actSeq;
  int numActions;
  int v5;
  int condArg6;
  GameObjectBody v7;

  interruptMusic = 0;
  condArg2 = evt.GetConditionArg2();
  if ( (condArg2 )!=0)
  {
    actSeq = (ActionSequence )evt.GetDispIoD20Signal().data1;
    numActions = actSeq.d20ActArrayNum;
    if ( condArg2 == 3 )
    {
      if ( numActions <= 0 )
      {
        return;
      }
      v5 = actSeq.d20ActArrayNum;
      do
      {
        switch ( actSeq.d20ActArray[0].d20ActType )
        {
          default:
            interruptMusic = 1;
            break;
          case 0:
          case 6:
          case 7:
          case 8:
          case 9:
          case 0xD:
          case 0x18:
            break;
        }
        actSeq = (ActionSequence )((string )actSeq + 88);
        --v5;
      }
      while ( v5 );
    }
    else
    {
      if ( numActions <= 0 )
      {
        return;
      }
      do
      {
        if ( actSeq.d20ActArray[0].d20ActType == D20ActionType.CAST_SPELL)
        {
          interruptMusic = 1;
        }
        actSeq = (ActionSequence )((string )actSeq + 88);
        --numActions;
      }
      while ( numActions );
    }
    if ( (interruptMusic )!=0)
    {
      evt.SetConditionArg2(0);
      condArg6 = evt.GetConditionArg(5);
      GameSystems.ParticleSys.Remove(condArg6);
      v7 = evt.GetConditionObjArg(3);
      if ( v7 !=null)
      {
        GameSystems.D20.D20SendSignal(v7, D20DispatcherKey.SIG_Bardic_Music_Completed, 0, 0);
      }
      BardicMusicPlaySound/*0x100fe4f0*/(condArg2, evt.objHndCaller, 1);
    }
  }
}


[DispTypes(DispatcherType.D20ActionOnActionFrame)]
[TempleDllLocation(0x100fe570)]
[TemplePlusLocation("condition.cpp:487")]
public static void   BardicMusicActionFrame(in DispatcherCallbackArgs evt)
{
  DispIoD20ActionTurnBased dispIo;  DispIoD20ActionTurnBased v3;
  int condArg6;
  GameObjectBody v5;
  D20Action d20a;
  int condArg1;
  
  int v9;
  
  
  GameObjectBody v12;
  int partsysId;
  int DC;
  DispIoD20ActionTurnBased v15;

  partsysId = 0;
  dispIo = evt.GetDispIoD20ActionTurnBased();/*INLINED:v2=evt.subDispNode.condNode*/  v3 = dispIo;
  v15 = dispIo;
  if ( (evt.GetConditionArg2() )!=0)
  {
    evt.SetConditionArg2(0);
    condArg6 = evt.GetConditionArg(5);
    GameSystems.ParticleSys.Remove(condArg6);
    v5 = evt.GetConditionObjArg(3);
    if ( v5 !=null)
    {
      GameSystems.D20.D20SendSignal(v5, D20DispatcherKey.SIG_Bardic_Music_Completed, 0, 0);
    }
  }
  d20a = v3.action;
  condArg1 = evt.GetConditionArg1();
  evt.SetConditionArg1(condArg1 - 1);
  switch ( d20a.data1 )
  {
    case 1:
      ConditionExtensions.AddConditionToPartyAround(evt.objHndCaller, 30F, StatusEffects.InspiredCourage, null);
      v12 = evt.objHndCaller;
      goto LABEL_13;
    case 2:
      ConditionExtensions.AddConditionToPartyAround(evt.objHndCaller, 30F, StatusEffects.Countersong, null);
      v12 = evt.objHndCaller;
      goto LABEL_13;
    case 3:
      GameSystems.Skill.SkillRoll(evt.objHndCaller, SkillId.perform, 0, &DC, 1);
      partsysId = GameSystems.ParticleSys.CreateAtObj("Bardic-Fascinate", evt.objHndCaller);
      if ( !GameSystems.D20.Combat.SavingThrow(d20a.d20ATarget, d20a.d20APerformer, DC, SavingThrowType.Will, D20SavingThrowFlag.SPELL_DESCRIPTOR_SONIC) )
      {
        d20a.d20ATarget.AddCondition(StatusEffects.Fascinate, 0, 0);
      }
      break;
    case 4:
      d20a.d20ATarget.AddCondition(StatusEffects.Competence);
      v12 = evt.objHndCaller;
      goto LABEL_13;
    case 5:
      v9 = evt.objHndCaller.GetStat(Stat.charisma);
      DC = D20StatSystem.GetModifierForAbilityScore(v9) + 13;
      partsysId = GameSystems.ParticleSys.CreateAtObj("Bardic-Suggestion", evt.objHndCaller);
      if ( !GameSystems.D20.Combat.SavingThrow(d20a.d20ATarget, d20a.d20APerformer, DC, SavingThrowType.Will, D20SavingThrowFlag.SPELL_DESCRIPTOR_SONIC) )
      {
        d20a.d20ATarget.AddCondition(StatusEffects.Suggestion);
      }
      break;
    case 6:
      d20a.d20ATarget.AddCondition(StatusEffects.Greatness);
      v12 = evt.objHndCaller;
LABEL_13:
      partsysId = GameSystems.ParticleSys.CreateAtObj("Bardic-Inspire Greatness", v12);
      break;
    default:
      break;
  }
  BardicMusicPlaySound/*0x100fe4f0*/(d20a.data1, evt.objHndCaller, 0);
  BardicMusicSetAffectedHandle/*0x100e1b60*/(evt.subDispNode.condNode, 3, d20a.d20ATarget);
  evt.SetConditionArg2(d20a.data1);
  evt.SetConditionArg3(0);
  evt.SetConditionArg(5, partsysId);
  v15.returnVal = 0;
}
/* Orphan comments:
TP Replaced @ condition.cpp:487
*/


[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x100fe820)]
[TemplePlusLocation("condition.cpp:488")]
public static void   BardicMusicBeginRound(in DispatcherCallbackArgs evt)
{
  int condArg2;
  int condArg3;
  int condArg6;
  GameObjectBody v4;
  GameObjectBody v5;

  condArg2 = evt.GetConditionArg2();
  condArg3 = evt.GetConditionArg3();
  if ( (condArg2 )!=0)
  {
    if ( *(_QWORD *)&evt.GetDispIoD20Signal().data1 <= 1 )
    {
      evt.SetConditionArg3(condArg3 + 1);
      v5 = evt.GetConditionObjArg(3);
      switch ( condArg2 )
      {
        case 1:
          ConditionExtensions.AddConditionToPartyAround(evt.objHndCaller, 30F, StatusEffects.InspiredCourage, null);
          break;
        case 2:
          ConditionExtensions.AddConditionToPartyAround(evt.objHndCaller, 30F, StatusEffects.Countersong, null);
          break;
        case 3:
          v5.AddCondition(StatusEffects.Fascinate, -1, 0);
          break;
        case 4:
          v5.AddCondition(StatusEffects.Competence);
          break;
        case 5:
          evt.SetConditionArg2(0);
          break;
        case 6:
          v5.AddCondition(StatusEffects.Greatness);
          break;
        default:
          return;
      }
    }
    else
    {
      evt.SetConditionArg3(0);
      evt.SetConditionArg2(0);
      condArg6 = evt.GetConditionArg(5);
      GameSystems.ParticleSys.Remove(condArg6);
      v4 = evt.GetConditionObjArg(3);
      if ( v4 !=null)
      {
        GameSystems.D20.D20SendSignal(v4, D20DispatcherKey.SIG_Bardic_Music_Completed, 0, 0);
      }
    }
  }
}
/* Orphan comments:
TP Replaced @ condition.cpp:488
*/


[DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
[TempleDllLocation(0x100fe180)]
public static void   BardicMusicInitCallback(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  int condArg6;
  BonusList bonlist;

  bonlist = BonusList.Create();
  v1 = evt.objHndCaller.DispatchGetLevel(8, &bonlist, 0, 0);
  if ( v1 < 1 )
  {
    v1 = 1;
  }
  evt.SetConditionArg1(v1);
  if ( (evt.GetConditionArg2() )!=0)
  {
    condArg6 = evt.GetConditionArg(5);
    GameSystems.ParticleSys.Remove(condArg6);
  }
  evt.SetConditionArg2(0);
}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100feba0)]
[TemplePlusLocation("condition.cpp:422")]
public static void   BarbarianDRDamageCallback(in DispatcherCallbackArgs evt)
{
  DamagePacket damPkt;
  int v2;

  damPkt = &evt.GetDispIoDamage().damage;
  v2 = evt.objHndCaller.GetStat(Stat.level_barbarian);
  if ( v2 >= 7 )
  {
    damPkt.AddPhysicalDR((v2 >= 10) + 1, 1, 126);
  }
}
/* Orphan comments:
TP Replaced @ condition.cpp:422
*/


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100fe0c0)]
public static void   ClassSavingThrowCrap(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  DispIoSavingThrow dispIo;

  v1 = evt.objHndCaller.GetStat((Stat)data);
  dispIo = evt.GetDispIoSavingThrow();
  dispIo.bonlist.AddBonus(v1 / 3, 0, 137);
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100fe220)]
[TemplePlusLocation("condition.cpp:485")]
public static void   BardicMusicRadial(in DispatcherCallbackArgs evt)
{
  int v1;
  int v2;
  int v3;
  int v4;
  int v5;
  int v6;
  int v7;
  int v8;
  string meslineValue;
int meslineKey;
  RadialMenuEntry radMenuEntry;

  radMenuEntry = RadialMenuEntry.Create();
  if ( (evt.objHndCaller.GetStat(Stat.level_bard) )!=0&& GameSystems.Skill.GetSkillRanks(evt.objHndCaller, SkillId.perform) >= 3 )
  {
    meslineKey = 5039;
    meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
    radMenuEntry.text = (string )meslineValue;
    radMenuEntry.type = 0;
    radMenuEntry.minArg = 0;
    radMenuEntry.maxArg = 0;
    v2 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Class);
    v3 = GameSystems.D20.RadialMenu.AddParentChildNode(evt.objHndCaller, ref radMenuEntry, v2);
    radMenuEntry.d20ActionType = D20ActionType.BARDIC_MUSIC;
    if ( GameSystems.Skill.GetSkillRanks(evt.objHndCaller, SkillId.perform) > 3 )
    {
      radMenuEntry.d20ActionData1 = 1;
      meslineKey = 5040;
      meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
      radMenuEntry.text = (string )meslineValue;
      radMenuEntry.helpSystemHashkey = "TAG_CLASS_FEATURES_BARD_INSPIRE_COURAGE"/*ELFHASH*/;
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v3);
    }
    if ( GameSystems.Skill.GetSkillRanks(evt.objHndCaller, SkillId.perform) > 3 )
    {
      radMenuEntry.d20ActionData1 = 2;
      meslineKey = 5043;
      meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
      radMenuEntry.text = (string )meslineValue;
      radMenuEntry.helpSystemHashkey = "TAG_CLASS_FEATURES_BARD_COUNTERSONG"/*ELFHASH*/;
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v3);
    }
    if ( GameSystems.Skill.GetSkillRanks(evt.objHndCaller, SkillId.perform) > 3 )
    {
      radMenuEntry.d20ActionData1 = 3;
      meslineKey = 5042;
      meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
      radMenuEntry.text = (string )meslineValue;
      radMenuEntry.helpSystemHashkey = "TAG_CLASS_FEATURES_BARD_FASCINATE"/*ELFHASH*/;
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v3);
    }
    if ( GameSystems.Skill.GetSkillRanks(evt.objHndCaller, SkillId.perform) > 6 )
    {
      radMenuEntry.d20ActionData1 = 4;
      meslineKey = 5041;
      meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
      radMenuEntry.text = (string )meslineValue;
      radMenuEntry.helpSystemHashkey = "TAG_CLASS_FEATURES_BARD_INSPIRE_COMPETENCE"/*ELFHASH*/;
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v3);
    }
    if ( GameSystems.Skill.GetSkillRanks(evt.objHndCaller, SkillId.perform) > 12 )
    {
      radMenuEntry.d20ActionData1 = 6;
      meslineKey = 5044;
      meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
      radMenuEntry.text = (string )meslineValue;
      radMenuEntry.helpSystemHashkey = "TAG_CLASS_FEATURES_BARD_INSPIRE_GREATNESS"/*ELFHASH*/;
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v3);
    }
  }
}
/* Orphan comments:
TP Replaced @ condition.cpp:485
*/


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x100feb30)]
public static void   TrapSenseRefSaveBonus(in DispatcherCallbackArgs evt, int data)
{
  int classLvl;
  DispIoSavingThrow dispIo;

  classLvl = evt.objHndCaller.GetStat((Stat)data);
  dispIo = evt.GetDispIoSavingThrow();
  if ( classLvl / 3 >= 1 )
  {
    if ( (dispIo.flags & D20SavingThrowFlag.CHARM)!=0)
    {
      dispIo.bonlist.AddBonus(classLvl / 3, 8, 280);
    }
  }
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x100febf0)]
public static void   SchoolSpecializationSkillLevel(in DispatcherCallbackArgs evt)
{
  DispIoObjBonus dispIo;
  int v2;
  int v3;
  int v4;
  string v5;
  string v6;
  string v7;
  int v8;

  dispIo = evt.GetDispIoObjBonus();
  v2 = (byte)evt.objHndCaller.GetInt32(obj_f.critter_school_specialization);
  v3 = evt.objHndCaller.GetInt32(obj_f.critter_school_specialization);
  v8 = BYTE1(v3);
  v4 = (evt.objHndCaller.GetInt32(obj_f.critter_school_specialization) >> 16) & 0xFF;
  if ( ((1 << (v2 + 4)) & dispIo.flags )!=0)
  {
    v5 = (string )GameSystems.Spell.GetSchoolOfMagicName(v2);
    dispIo.bonOut.AddBonus(2, 0, 306, v5);
  }
  if ( ((1 << (v8 + 4)) & dispIo.flags )!=0)
  {
    v6 = (string )GameSystems.Spell.GetSchoolOfMagicName(v8);
    dispIo.bonOut.AddBonus(-5, 0, 307, v6);
  }
  if ( ((1 << (v4 + 4)) & dispIo.flags )!=0)
  {
    v7 = (string )GameSystems.Spell.GetSchoolOfMagicName(v4);
    dispIo.bonOut.AddBonus(-5, 0, 307, v7);
  }
}

}
}
/*

PerformSkillLevelSufficient @ 0x100fe110 = 1
BardicMusicSetAffectedHandle @ 0x100e1b60 = 1
sub_100FECE0 @ 0x100fece0 = 1
byte_100FEE4F @ 0x100fee4f = 1
BardicMusicPlaySound @ 0x100fe4f0 = 2
*/