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

public static class DomainConditions {

private static readonly ILogger Logger = new ConsoleLogger();
[TempleDllLocation(0x102b1620)]
  public static readonly ConditionSpec AnimalDomain = ConditionSpec.Create("Animal Domain", 1)
.SetUnique()
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 0)
.AddHandler(DispatcherType.RadialMenuEntry, AnimalDomainRadial)
.AddHandler(DispatcherType.D20ActionPerform, (D20DispatcherKey)122, AnimalDomainPerformAction)
                    .Build();


[TempleDllLocation(0x102b0ec0)]
  public static readonly ConditionSpec DeathDomain = ConditionSpec.Create("Death Domain", 1)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
.AddHandler(DispatcherType.RadialMenuEntry, DeathTouchRadial)
.AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_READY_COUNTERSPELL, CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
.AddHandler(DispatcherType.D20ActionOnActionFrame, D20DispatcherKey.D20A_READY_COUNTERSPELL, DeathDomainD20ACheck)
                    .Build();


[TempleDllLocation(0x102b0d48)]
  public static readonly ConditionSpec TurnUndead = ConditionSpec.Create("Turn Undead", 2)
.SetUniqueWithKeyArg1()
.AddHandler(DispatcherType.ConditionAdd, TurnUndeadInitNumPerDay)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, TurnUndeadInitNumPerDay)
.AddHandler(DispatcherType.RadialMenuEntry, TurnUndeadRadialMenuEntry)
.AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_READY_SPELL, TurnUndead_Check)
.AddHandler(DispatcherType.D20ActionOnActionFrame, D20DispatcherKey.D20A_READY_SPELL, TurnUndeadPerform)
                    .Build();


[TempleDllLocation(0x102b0e78)]
  public static readonly ConditionSpec ChaosDomain = ConditionSpec.Create("Chaos Domain", 0)
.SetUnique()
.AddHandler(DispatcherType.BaseCasterLevelMod, Alignment_Domain_SpellCasterLevel_Modify, 2, 1)
                    .Build();


[TempleDllLocation(0x102b12b8)]
  public static readonly ConditionSpec ProtectionDomain = ConditionSpec.Create("Protection Domain", 1)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
.AddHandler(DispatcherType.RadialMenuEntry, ProtectiveWardRadial)
.AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_READY_ENTER, CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
.AddHandler(DispatcherType.D20ActionOnActionFrame, D20DispatcherKey.D20A_READY_ENTER, sub_1004BAE0)
                    .Build();


[TempleDllLocation(0x102b10f0)]
  public static readonly ConditionSpec GoodDomain = ConditionSpec.Create("Good Domain", 0)
.SetUnique()
.AddHandler(DispatcherType.BaseCasterLevelMod, Alignment_Domain_SpellCasterLevel_Modify, 1024, 1)
                    .Build();


[TempleDllLocation(0x102b10a8)]
  public static readonly ConditionSpec EvilDomain = ConditionSpec.Create("Evil Domain", 0)
.SetUnique()
.AddHandler(DispatcherType.BaseCasterLevelMod, Alignment_Domain_SpellCasterLevel_Modify, 64, 1)
                    .Build();


[TempleDllLocation(0x102b1180)]
  public static readonly ConditionSpec LawDomain = ConditionSpec.Create("Law Domain", 0)
.SetUnique()
.AddHandler(DispatcherType.BaseCasterLevelMod, Alignment_Domain_SpellCasterLevel_Modify, 4096, 1)
                    .Build();


[TempleDllLocation(0x102b15a0)]
  public static readonly ConditionSpec TravelDomain = ConditionSpec.Create("Travel Domain", 3)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, sub_1004BC90)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, sub_1004BC90)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement, LuckDomainFreedomOfMovement)
.AddHandler(DispatcherType.BeginRound, sub_1004BCF0, 1)
                    .Build();


[TempleDllLocation(0x102b0de0)]
  public static readonly ConditionSpec GreaterTurning = ConditionSpec.Create("Greater Turning", 2)
.PreventsWithSameArg1(TurnUndead)
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg1FromSubDispDef, 1)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, TurnUndeadInitNumPerDay)
.AddHandler(DispatcherType.RadialMenuEntry, TurnUndeadRadialMenuEntry)
.AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_READY_SPELL, TurnUndead_Check)
.AddHandler(DispatcherType.D20ActionOnActionFrame, D20DispatcherKey.D20A_READY_SPELL, TurnUndeadPerform)
                    .Build();


[TempleDllLocation(0x102b1210)]
  public static readonly ConditionSpec LuckDomain = ConditionSpec.Create("Luck Domain", 4)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
.AddHandler(DispatcherType.RadialMenuEntry, GoodFortune_RadialMenuEntry_Callback)
.AddQueryHandler(D20DispatcherKey.QUE_RerollAttack, sub_1004B9C0)
.AddQueryHandler(D20DispatcherKey.QUE_RerollSavingThrow, sub_1004B9C0)
.AddQueryHandler(D20DispatcherKey.QUE_RerollCritical, sub_1004B9C0)
                    .Build();


[TempleDllLocation(0x102b1438)]
  public static readonly ConditionSpec StrengthDomain = ConditionSpec.Create("Strength Domain", 1)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
.AddHandler(DispatcherType.RadialMenuEntry, FeatOfStrengthRadial)
.AddHandler(DispatcherType.D20ActionCheck, D20DispatcherKey.D20A_READY_EXIT, CommonConditionCallbacks.D20ActionCheckRemainingCharges, 0)
.AddHandler(DispatcherType.D20ActionOnActionFrame, D20DispatcherKey.D20A_READY_EXIT, StrengthDomainFeatOfStrengthActivate)
                    .Build();


[TempleDllLocation(0x102b0f58)]
  public static readonly ConditionSpec DestructionDomain = ConditionSpec.Create("Destruction Domain", 1)
.SetUnique()
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
.AddHandler(DispatcherType.NewDay, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 1)
.AddHandler(DispatcherType.RadialMenuEntry, DestructionDomainRadialMenu)
.AddHandler(DispatcherType.DestructionDomain, (D20DispatcherKey)322, sub_1004B700)
                    .Build();


[TempleDllLocation(0x102b1138)]
  public static readonly ConditionSpec HealingDomain = ConditionSpec.Create("Healing Domain", 0)
.SetUnique()
.AddHandler(DispatcherType.BaseCasterLevelMod, HealingDomainCasterLvlBonus, 3, 1)
                    .Build();


[TempleDllLocation(0x102b0fd8)]
  public static readonly ConditionSpec DestructionDomainSmite = ConditionSpec.Create("Destruction Domain Smite", 1)
.SetUnique()
.AddHandler(DispatcherType.ToHitBonus2, sub_1004B750)
.AddHandler(DispatcherType.DealingDamage, sub_1004B780)
.AddHandler(DispatcherType.DealingDamage, CommonConditionCallbacks.conditionRemoveCallback)
.RemoveOnSignal(D20DispatcherKey.SIG_Killed)
.AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.conditionRemoveCallback)
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 0, "sp-smite")
.AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.PlayParticlesSavePartsysId, 0, "sp-smite")
.AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 0)
                    .Build();


[TempleDllLocation(0x102b1350)]
  public static readonly ConditionSpec ProtectionDomainWard = ConditionSpec.Create("Protection Domain Ward", 1)
.SetUnique()
.AddHandler(DispatcherType.DealingDamage, CommonConditionCallbacks.conditionRemoveCallback)
.AddHandler(DispatcherType.SaveThrowLevel, ProtectionDomainWard_SavingThrowCallback)
.RemoveOnSignal(D20DispatcherKey.SIG_Killed)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, CommonConditionCallbacks.conditionRemoveCallback)
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 0, "sp-Protective Ward")
.AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.PlayParticlesSavePartsysId, 0, "sp-Protective Ward")
.AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 0)
.AddHandler(DispatcherType.ConditionRemove2, CommonConditionCallbacks.PlayParticlesSavePartsysId, 0, "sp-Protective Ward-END")
.AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 79)
                    .Build();


[TempleDllLocation(0x102b14d0)]
  public static readonly ConditionSpec StrengthDomainFeat = ConditionSpec.Create("Strength Domain Feat", 1)
.SetUnique()
.AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_STRENGTH, FeatOfStrengthStatBonus)
.RemoveOnSignal(D20DispatcherKey.SIG_Killed)
.AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.conditionRemoveCallback)
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 0, "sp-Feat of Strength")
.AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.PlayParticlesSavePartsysId, 0, "sp-Feat of Strength")
.AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 0)
.AddHandler(DispatcherType.ConditionRemove2, CommonConditionCallbacks.PlayParticlesSavePartsysId, 0, "sp-Feat of Strength-END")
.AddHandler(DispatcherType.EffectTooltip, CommonConditionCallbacks.EffectTooltipGeneral, 80)
                    .Build();


[TempleDllLocation(0x102b0bc0)]
  public static readonly ConditionSpec Turned = ConditionSpec.Create("Turned", 4)
.SetUnique()
.RemovedBy(Cowering)
.RemovedBy(Commanded)
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 10)
.AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 0)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, CommonConditionCallbacks.conditionRemoveCallback)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Is_Afraid, sub_1004AC90)
.AddSignalHandler(D20DispatcherKey.SIG_Pack, CommonConditionCallbacks.D20SignalPackHandler, 0)
.AddSignalHandler(D20DispatcherKey.SIG_Unpack, CommonConditionCallbacks.D20SignalUnpackHandler, 0)
.SetQueryResult(D20DispatcherKey.QUE_Turned, true)
.SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
.AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 85, 0)
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 3, "sp-Turn Undead-Hit")
.AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.PlayParticlesSavePartsysId, 3, "sp-Turn Undead-Hit")
.AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 3)
.AddHandler(DispatcherType.ConditionRemove2, CommonConditionCallbacks.PlayParticlesSavePartsysId, 3, "sp-Turn Undead-END")
.AddHandler(DispatcherType.ConditionRemove2, sub_1004BEB0)
.RemoveOnSignal(D20DispatcherKey.SIG_Killed)
                    .Build();


[TempleDllLocation(0x102b0a28)]
  public static readonly ConditionSpec Cowering = ConditionSpec.Create("Cowering", 2)
.SetUnique()
.RemovedBy(Turned)
.RemovedBy(Commanded)
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.CondNodeSetArg0FromSubDispDef, 10)
.AddHandler(DispatcherType.BeginRound, CommonConditionCallbacks.ConditionDurationTicker, 0)
.AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitNoActions)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, CommonConditionCallbacks.conditionRemoveCallback)
.SetQueryResult(D20DispatcherKey.QUE_SneakAttack, true)
.SetQueryResult(D20DispatcherKey.QUE_AOOPossible, false)
.AddHandler(DispatcherType.GetAC, CommonConditionCallbacks.AcBonusCapper, 187)
.AddHandler(DispatcherType.GetAC, sub_100ED330, -2, 187)
.AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 86, 0)
.SetQueryResult(D20DispatcherKey.QUE_Rebuked, true)
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 1, "sp-Rebuke Undead-Hit")
.AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.PlayParticlesSavePartsysId, 1, "sp-Rebuke Undead-Hit")
.AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 1)
.AddHandler(DispatcherType.ConditionRemove2, CommonConditionCallbacks.PlayParticlesSavePartsysId, 1, "sp-Rebuke Undead-END")
.AddHandler(DispatcherType.ConditionRemove2, sub_1004BEB0)
.RemoveOnSignal(D20DispatcherKey.SIG_Killed)
                    .Build();


[TempleDllLocation(0x102b0940)]
  public static readonly ConditionSpec Commanded = ConditionSpec.Create("Commanded", 1)
.SetUnique()
.RemovedBy(Cowering)
.RemovedBy(Turned)
.AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipNoRepetitionCallback, 87, 0)
.SetQueryResult(D20DispatcherKey.QUE_Commanded, true)
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.PlayParticlesSavePartsysId, 0, "sp-Commnad Undead-Hit")
.AddHandler(DispatcherType.ConditionAddFromD20StatusInit, CommonConditionCallbacks.PlayParticlesSavePartsysId, 0, "sp-Commnad Undead-Hit")
.AddHandler(DispatcherType.ConditionRemove, CommonConditionCallbacks.EndParticlesFromArg, 0)
.AddHandler(DispatcherType.ConditionRemove2, CommonConditionCallbacks.PlayParticlesSavePartsysId, 0, "sp-Commnad Undead-END")
.RemoveOnSignal(D20DispatcherKey.SIG_Killed)
                    .Build();


public static IReadOnlyList<ConditionSpec> Conditions {get;} = new List<ConditionSpec>
{
GoodDomain,
LuckDomain,
StrengthDomainFeat,
ProtectionDomain,
ChaosDomain,
TravelDomain,
Turned,
TurnUndead,
StrengthDomain,
HealingDomain,
Cowering,
LawDomain,
DestructionDomain,
AnimalDomain,
DeathDomain,
DestructionDomainSmite,
ProtectionDomainWard,
EvilDomain,
Commanded,
GreaterTurning,
};

[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x1004bb40)]
public static void   ProtectionDomainWard_SavingThrowCallback(in DispatcherCallbackArgs evt)
{
  int v1;
  DispIoSavingThrow dispIo;

  v1 = evt.objHndCaller.GetStat(Stat.level_cleric);
  dispIo = evt.GetDispIoSavingThrow();
  dispIo.bonlist.AddBonus(v1, 0, 182);
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x1004bd30)]
public static void   LuckDomainFreedomOfMovement(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;
  int condArg1;
  
  int v4;

  dispIo = evt.GetDispIoD20Query();
  if ( (evt.GetConditionArg2() )!=0)
  {
    dispIo.return_val = 1;
  }
  else
  {
    condArg1 = evt.GetConditionArg1();
    if ( condArg1 > 0 )
    {
      GameSystems.Deity.DeityPraiseFloatMessage(evt.objHndCaller);
      evt.SetConditionArg1(condArg1 - 1);
      evt.SetConditionArg2(1);
      v4 = GameSystems.ParticleSys.CreateAtObj("sp-Luck Domain Reroll", evt.objHndCaller);
      evt.SetConditionArg3(v4);
      dispIo.return_val = 1;
    }
  }
}


[DispTypes(DispatcherType.BaseCasterLevelMod)]
[TempleDllLocation(0x1004b430)]
public static void   HealingDomainCasterLvlBonus(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoD20Query dispIo;
  DispIoD20Query v2;
  int spEnum;
  dispIo = evt.GetDispIoD20Query();
  v2 = dispIo;
  spEnum = *(_DWORD *)dispIo.data1;
  if ( (spEnum )!=0)
  {/*INLINED:v4=evt.subDispNode.subDispDef*/    if ( GetSpellSubschool/*0x10075340*/(spEnum) & data1 )
    {
      v2.return_val += data2;
    }
  }
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x1004b690)]
public static void   DestructionDomainRadialMenu(in DispatcherCallbackArgs evt)
{
  int v1;
  int v2;
  string meslineValue;
int meslineKey;
  RadialMenuEntry radMenuEntry;

  radMenuEntry = RadialMenuEntry.Create();
  radMenuEntry.callback = DestructionDomainRadialCallback/*0x100f02c0*/;
  radMenuEntry.dispKey = D20DispatcherKey.SIG_DestructionDomainSmite;
  meslineKey = 5021;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  radMenuEntry.text = (string )meslineValue;
  radMenuEntry.helpSystemHashkey = "TAG_DESTRUCTION_D"/*ELFHASH*/;
  v2 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Class);
  GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v2);
}


[DispTypes(DispatcherType.D20ActionOnActionFrame)]
[TempleDllLocation(0x1004bbf0)]
public static void   StrengthDomainFeatOfStrengthActivate(in DispatcherCallbackArgs evt)
{
  DispIoD20ActionTurnBased dispIo;
  int condArg1;

  dispIo = evt.GetDispIoD20ActionTurnBased();
  condArg1 = evt.GetConditionArg1();
  evt.SetConditionArg1(condArg1 - 1);
  GameSystems.Deity.DeityPraiseFloatMessage(evt.objHndCaller);
  evt.objHndCaller.AddCondition(DomainConditions.StrengthDomainFeat);
  dispIo.returnVal = 0;
}


[DispTypes(DispatcherType.D20ActionOnActionFrame)]
[TempleDllLocation(0x1004bae0)]
public static void   sub_1004BAE0(in DispatcherCallbackArgs evt)
{
  DispIoD20ActionTurnBased dispIo;
  int condArg1;

  dispIo = evt.GetDispIoD20ActionTurnBased();
  condArg1 = evt.GetConditionArg1();
  evt.SetConditionArg1(condArg1 - 1);
  GameSystems.Deity.DeityPraiseFloatMessage(evt.objHndCaller);
  dispIo.action.d20ATarget.AddCondition(DomainConditions.ProtectionDomainWard);
  dispIo.returnVal = 0;
}


[DispTypes(DispatcherType.D20ActionOnActionFrame)]
[TempleDllLocation(0x1004b4b0)]
public static void   DeathDomainD20ACheck(in DispatcherCallbackArgs evt)
{
  DispIoD20ActionTurnBased dispIo;  DispIoD20ActionTurnBased v3;
  D20Action v4;
  int condArg1;
  int v6;
  int v7;
  int v8;
  int v9;
  int v10;
  int v11;
  int v12;
  string v13;
  GameObjectBody v14;
  GameObjectBody v15;
  string meslineValue;
int meslineKey;

  dispIo = evt.GetDispIoD20ActionTurnBased();/*INLINED:v2=evt.subDispNode.condNode*/  v3 = dispIo;
  v4 = dispIo.action;
  meslineKey = (int)dispIo;
  condArg1 = evt.GetConditionArg1();
  evt.SetConditionArg1(condArg1 - 1);
  if ( (v4.d20Caf & D20CAF.HIT)!=0)
  {
    if ( GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Immune_Death_Touch) )
    {
      meslineKey = 7001;
      meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
      GameSystems.RollHistory.CreateFromFreeText((string )meslineValue);
      GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x28, evt.objHndCaller, null);
      GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 156);
    }
    else
    {
      v7 = evt.objHndCaller.GetStat(Stat.level_cleric);
      v8 = DiceRoller/*0x10038b60*/(v7, 6, 0);
      v9 = v4.d20ATarget.GetStat(Stat.hp_current, 0);
      v14 = v4.d20ATarget;
      if ( v8 <= v9 )
      {
        GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x2A, evt.objHndCaller, v14);
        GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 71);
        v15 = v4.d20ATarget;
        v13 = "fizzle";
      }
      else
      {
        GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x29, evt.objHndCaller, v14);
        GameSystems.D20.Combat.KillWithDeathEffect(v4.d20ATarget, evt.objHndCaller, v10, v4.d20ATarget >> 32);
        GameSystems.Deity.DeityPraiseFloatMessage(evt.objHndCaller);
        v11 = v4.d20ATarget;
        meslineKey = HIDWORD(v4.d20ATarget);
      }
      v12 = v13/*ELFHASH*/;
      GameSystems.ParticleSys.CreateAtObj(v12, v15);
      *(_DWORD *)(meslineKey + 4) = 0;
    }
  }
  else
  {
    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 29);
    v3.returnVal = 0;
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x1004b9c0)]
public static void   sub_1004B9C0(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;
  int condArg1;
  int v3;
  

  dispIo = evt.GetDispIoD20Query();
  condArg1 = evt.GetConditionArg1();
  if ( condArg1 > 0 )
  {
    switch ( evt.dispKey )
    {
      case 266:
        v3 = 2;
        break;
      case 267:
        v3 = 1;
        break;
      case 268:
        v3 = 3;
        break;
      default:
        v3 = (int)evt.subDispNode;
        break;
    }
    if ( (evt.GetConditionArg(v3) )!=0)
    {
      dispIo.return_val = 1;
      evt.SetConditionArg1(condArg1 - 1);
      GameSystems.Deity.DeityPraiseFloatMessage(evt.objHndCaller);
      GameSystems.ParticleSys.CreateAtObj("sp-Luck Domain Reroll", evt.objHndCaller);
    }
  }
}


[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x1004bcf0)]
public static void   sub_1004BCF0(in DispatcherCallbackArgs evt, int data)
{
  int condArg3;

  if ( (evt.GetConditionArg2() )!=0)
  {
    condArg3 = evt.GetConditionArg3();
    GameSystems.ParticleSys.Remove(condArg3);
  }
  evt.SetConditionArg2(0);
}


[DispTypes(DispatcherType.D20ActionCheck)]
[TempleDllLocation(0x1004ade0)]
[TemplePlusLocation("condition.cpp:511")]
public static void   TurnUndead_Check(in DispatcherCallbackArgs evt)
{
  DispIoD20ActionTurnBased dispIo;
  D20Action v2;
  int condArg2;

  dispIo = evt.GetDispIoD20ActionTurnBased();
  v2 = dispIo.action;
  condArg2 = evt.GetConditionArg2();
  if ( evt.GetConditionArg1() == v2.data1 )
  {
    dispIo.returnVal = condArg2 > 0 ? 0 : 0x10;
  }
}
/* Orphan comments:
TP Replaced @ condition.cpp:511
*/


[DispTypes(DispatcherType.BaseCasterLevelMod)]
[TempleDllLocation(0x1004b3f0)]
public static void   Alignment_Domain_SpellCasterLevel_Modify(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoD20Query dispIo;
  DispIoD20Query v2;
  int v3;
  dispIo = evt.GetDispIoD20Query();
  v2 = dispIo;
  v3 = *(_DWORD *)dispIo.data1;
  if ( (v3 )!=0)
  {/*INLINED:spellDescriptor=evt.subDispNode.subDispDef*/    if ( SpellEntry_Get_Descriptor/*0x10075380*/(v3) & data1 )
    {
      v2.return_val += data2;
    }
  }
}


[DispTypes(DispatcherType.D20ActionOnActionFrame)]
[TempleDllLocation(0x1004aeb0)]
[TemplePlusLocation("condition.cpp:509")]
public static void   TurnUndeadPerform(in DispatcherCallbackArgs evt)
{
  D20Action evtObj;
  DispIoD20ActionTurnBased dispIo;
  int turnUndeadType;
  int palLvlAdj;
  
  int condArg2;
  long v7;
  ObjectNode *v8;
  int (  *v9)(GameObjectBody);
  bool (  *v10)(GameObjectBody);
  int v11;
  int turnModifier;
  GameObjectBody v13;
  int v14;
  int v15;
  int npcHd;
  BOOL (  *v17)(GameObjectBody);
  
  GameObjectBody v19;
  int hitdieTot;
  int turningLvl;
  int turnUndeadType_;
  int turnRoll;
  int i;
  TileRect xyRect;
  GroupArray pGroup_Array;
  ObjListResult objlist;

  evtObj = evt.GetDispIoD20ActionTurnBased().action;
  GameSystems.SoundGame.PositionalSound(9302, 1, evtObj.d20APerformer);
  if ( (evtObj.d20ATarget == null)    || evtObj.d20ATarget.type != 13 && evtObj.d20ATarget.type != 14
    || D20QueryWithDataDefault1/*0x1004ccd0*/(
         evtObj.d20ATarget,
         D20DispatcherKey.QUE_CanBeAffected_PerformAction,
         (int)evtObj,
         (ulong)(int)evtObj >> 32) )
  {
    dispIo = evt.GetDispIoD20ActionTurnBased();
    turnUndeadType = dispIo.action.data1;
    turnUndeadType_ = dispIo.action.data1;
    turningLvl = evt.objHndCaller.GetStat(Stat.level_cleric);
    palLvlAdj = evt.objHndCaller.GetStat(Stat.level_paladin) - 2;
    if ( palLvlAdj > 0 && ((turnUndeadType )==0|| turnUndeadType == 7) )
    {
      turningLvl += palLvlAdj;
    }
    if ( GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.IMPROVED_TURNING) )
    {
      ++turningLvl;
    }
    if ( evt.GetConditionArg1() == turnUndeadType )
    {
      GameSystems.Deity.DeityPraiseFloatMessage(evt.objHndCaller);
      if ( evt.objHndCaller.GetInt32(obj_f.critter_alignment_choice) == 1 )
      {
      }
      else
      {
      }
      GameSystems.ParticleSys.CreateAtObj("sp-Rebuke Undead", evt.objHndCaller);
      condArg2 = evt.GetConditionArg2();
      evt.SetConditionArg2(condArg2 - 1);
      v7 = evt.objHndCaller.GetInt64(obj_f.location);
      xyRect.x1 = (int)v7 - (int)turnUndeadRange/*0x102b17e4*/;
      xyRect.y1 = HIDWORD(v7) - (int)turnUndeadRange/*0x102b17e4*/;
      xyRect.x2 = (int)v7 + (int)turnUndeadRange/*0x102b17e4*/;
      xyRect.y2 = HIDWORD(v7) + (int)turnUndeadRange/*0x102b17e4*/;
      ObjList.ListRect(&xyRect, ObjectListFilter.OLC_CRITTERS, &objlist);
      GroupArrayReset/*0x100df930*/(&pGroup_Array);
      turnUndeadInvoker/*0x11e61540*/ = evt.objHndCaller;
      GroupArraySortFuncSetAndSort/*0x100dfa00*/(&pGroup_Array, (int (  *)(void *, void *))TurnUndeadSorter_ByDistance/*0x1004ae30*/);
      v8 = objlist.ObjectHandles.
      if ( objlist.ObjectHandles.)
      {
        do
        {
          if ( (LODWORD(v8.item.handle) != LODWORD(evt.objHndCaller)
             || HIDWORD(v8.item.handle) != HIDWORD(evt.objHndCaller))
            && !GameSystems.Critter.IsDeadNullDestroyed(v8.item.handle) )
          {
            if ( (v9 = (int (  *)(GameObjectBody))off_102B17A4/*0x102b17a4*/[turnUndeadType_]) != 0 && v9(v8.item.handle)
              || (v10 = (bool (  *)(GameObjectBody))rebukeUndeadTypeCheck/*0x102b17c4*/[turnUndeadType_]) != 0
              && ((int (  *)(_DWORD, _DWORD))v10)(v8.item.handle, HIDWORD(v8.item.handle)) )
            {
              GroupArrayAdd/*0x100df990*/(&pGroup_Array, v8.item.handle);
            }
          }
          v8 = v8.item.next;
        }
        while ( v8 );
      }
      ObjListFree/*0x1001f2c0*/(&objlist);
      GroupArraySort/*0x100df900*/(&pGroup_Array);
      v11 = evt.objHndCaller.GetStat(Stat.charisma);
      turnModifier = D20StatSystem.GetModifierForAbilityScore(v11);
      if ( GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_On_Consecrate_Ground) )
      {
        turnModifier += GameSystems.D20.D20QueryReturnObject(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_On_Consecrate_Ground, 0, 0);
      }
      if ( GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_On_Desecrate_Ground) )
      {
        turnModifier -= GameSystems.D20.D20QueryReturnObject(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_On_Desecrate_Ground, 0, 0);
      }
      turnRoll = turningLvl + (DiceRoller/*0x10038b60*/(1, 20, turnModifier) - 10) / 3;
      hitdieTot = DiceRoller/*0x10038b60*/(2, 6, turningLvl + turnModifier);
      i = 0;
      if ( GetGroupLength/*0x100df750*/(&pGroup_Array) )
      {
        do
        {
          v13 = GetGroupMember/*0x100df760*/(&pGroup_Array, i);
          v14 = HIDWORD(v13);
          v15 = v13;
          npcHd = v13.GetInt32(obj_f.npc_hitdice_idx, 0);
          if ( npcHd <= turnRoll && npcHd <= hitdieTot )
          {
            v17 = rebukeUndeadTypeCheck/*0x102b17c4*/[turnUndeadType_];
            if ( v17 && ((int (  *)(_DWORD, _DWORD))v17)(v15, v14) )
            {
              if ( turnUndeadType_ != 7 )
              {
                if ( !GameSystems.D20.D20Query(__PAIR__(v14, v15), D20DispatcherKey.QUE_Commanded) )
                {
                  if ( 2 * npcHd <= turningLvl && GameSystems.Critter.FollowerAdd(__PAIR__(v14, v15), evt.objHndCaller, 1, 1) )
                  {
                    hitdieTot -= npcHd;
                    __PAIR__(v14, v15).AddCondition(DomainConditions.Commanded);
                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x34, __PAIR__(v14, v15), null);
                  }
                  else if ( !GameSystems.D20.D20Query(__PAIR__(v14, v15), D20DispatcherKey.QUE_Rebuked) )
                  {
                    hitdieTot -= npcHd;
                    __PAIR__(v14, v15).AddCondition(DomainConditions.Cowering);
                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x36, __PAIR__(v14, v15), null);
                  }
                }
                goto LABEL_49;
              }
              hitdieTot -= npcHd;
              HIDWORD(v19) = HIDWORD(evt.objHndCaller);
            }
            else if ( turnUndeadType_ == 7 )
            {
              hitdieTot -= npcHd;
              HIDWORD(v19) = HIDWORD(evt.objHndCaller);
            }
            else
            {
              if ( 2 * npcHd > turningLvl )
              {
                if ( !GameSystems.D20.D20Query(__PAIR__(v14, v15), D20DispatcherKey.QUE_Turned) )
                {
                  hitdieTot -= npcHd;
                  __PAIR__(v14, v15).AddCondition(DomainConditions.Turned, 10, SHIDWORD(evt.objHndCaller), evt.objHndCaller);
                  GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x35, __PAIR__(v14, v15), null);
                }
                goto LABEL_49;
              }
              hitdieTot -= npcHd;
              HIDWORD(v19) = HIDWORD(evt.objHndCaller);
            }
            LODWORD(v19) = evt.objHndCaller;
            CritterKill_Simple/*0x100b8990*/(__PAIR__(v14, v15), v19);
            GameSystems.ParticleSys.CreateAtObj("sp-Destroy Undead", __PAIR__(v14, v15));
          }
LABEL_49:
          ++i;
        }
        while ( i < GetGroupLength/*0x100df750*/(&pGroup_Array) );
      }
    }
  }
}
/* Orphan comments:
TP Replaced @ condition.cpp:509
*/


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x1004bde0)]
public static void   AnimalDomainRadial(in DispatcherCallbackArgs evt)
{
  string v1;
  int v2;
  RadialMenuEntry radMenuEntry;

  if ( (evt.GetConditionArg1() )==0)
  {
    radMenuEntry = RadialMenuEntry.Create();
    radMenuEntry.d20ActionType = D20ActionType.ACTIVATE_DEVICE_SPELL;
    radMenuEntry.d20ActionData1 = 23;
    radMenuEntry.d20SpellData.SetSpellData(57, 23, 1, 255, 0);
    radMenuEntry.text = GameSystems.Spell.GetSpellName(57);
    v1 = GameSystems.Spell.GetSpellHelpTopic(57);
    radMenuEntry.helpSystemHashkey = v1/*ELFHASH*/;
    v2 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Class);
    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v2);
  }
}


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x100ed330)]
public static void   sub_100ED330(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;

  dispIo = evt.GetDispIoAttackBonus();
  dispIo.bonlist.AddBonus(data1, 0, data2);
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x1004ac90)]
public static void   sub_1004AC90(in DispatcherCallbackArgs evt)
{
  int condArg2;
  int condArg3;
  DispIoD20Query dispIo;

  condArg2 = evt.GetConditionArg2();
  condArg3 = evt.GetConditionArg3();
  dispIo = evt.GetDispIoD20Query();
  dispIo.data2 = condArg2;
  dispIo.return_val = 1;
  dispIo.data1 = condArg3;
}


[DispTypes(DispatcherType.D20ActionPerform)]
[TempleDllLocation(0x1004be80)]
public static void   AnimalDomainPerformAction(in DispatcherCallbackArgs evt)
{
  if ( evt.GetDispIoD20ActionTurnBased().action.data1 == 23 )
  {
    evt.SetConditionArg1(1);
  }
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x1004b620)]
public static void   DeathTouchRadial(in DispatcherCallbackArgs evt)
{
  int v1;
  int v2;
  string meslineValue;
int meslineKey;
  RadialMenuEntry radMenuEntry;

  radMenuEntry = RadialMenuEntry.Create();
  radMenuEntry.d20ActionType = D20ActionType.DEATH_TOUCH;
  radMenuEntry.d20ActionData1 = 0;
  meslineKey = 5020;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  radMenuEntry.text = (string )meslineValue;
  radMenuEntry.helpSystemHashkey = "TAG_DEATH_D"/*ELFHASH*/;
  v2 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Class);
  GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v2);
}


[DispTypes(DispatcherType.ConditionRemove2)]
[TempleDllLocation(0x1004beb0)]
public static void   sub_1004BEB0(in DispatcherCallbackArgs evt)
{
  GameSystems.AI.StopFleeing(evt.objHndCaller);
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x1004b780)]
public static void   sub_1004B780(in DispatcherCallbackArgs evt)
{
  int v1;
  DispIoDamage dispIo;
  GameObjectBody v3;
  

  v1 = evt.objHndCaller.GetStat(Stat.level_cleric);
  dispIo = evt.GetDispIoDamage();
  dispIo.damage.AddDamageBonus(v1, 0, 181);
  v3 = dispIo.attackPacket.victim;
  GameSystems.ParticleSys.CreateAtObj("sp-Smite-Hit", v3);
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x1004b750)]
public static void   sub_1004B750(in DispatcherCallbackArgs evt)
{
  DispIoAttackBonus dispIo;

  dispIo = evt.GetDispIoAttackBonus();
  dispIo.bonlist.AddBonus(4, 0, 181);
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x1004bb80)]
public static void   FeatOfStrengthRadial(in DispatcherCallbackArgs evt)
{
  int v1;
  int v2;
  string meslineValue;
int meslineKey;
  RadialMenuEntry radMenuEntry;

  radMenuEntry = RadialMenuEntry.Create();
  radMenuEntry.d20ActionType = D20ActionType.FEAT_OF_STRENGTH;
  radMenuEntry.d20ActionData1 = 0;
  meslineKey = 5023;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  radMenuEntry.text = (string )meslineValue;
  radMenuEntry.helpSystemHashkey = "TAG_STRENGTH_D"/*ELFHASH*/;
  v2 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Class);
  GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v2);
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x1004ba70)]
public static void   ProtectiveWardRadial(in DispatcherCallbackArgs evt)
{
  int v1;
  int v2;
  string meslineValue;
int meslineKey;
  RadialMenuEntry radMenuEntry;

  radMenuEntry = RadialMenuEntry.Create();
  radMenuEntry.d20ActionType = D20ActionType.PROTECTIVE_WARD;
  radMenuEntry.d20ActionData1 = 0;
  meslineKey = 5022;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  radMenuEntry.text = (string )meslineValue;
  radMenuEntry.helpSystemHashkey = "TAG_PROTECTION_D"/*ELFHASH*/;
  v2 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Class);
  GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v2);
}


[DispTypes(DispatcherType.AbilityScoreLevel)]
[TempleDllLocation(0x1004bc50)]
public static void   FeatOfStrengthStatBonus(in DispatcherCallbackArgs evt)
{
  int v1;
  DispIoBonusList dispIo;

  v1 = evt.objHndCaller.GetStat(Stat.level_cleric);
  dispIo = evt.GetDispIoBonusList();
  dispIo.bonlist.AddBonus(v1, 0, 183);
}


[DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
[TempleDllLocation(0x1004ace0)]
public static void   TurnUndeadInitNumPerDay(in DispatcherCallbackArgs evt)
{
  int v1;
  int v2;
  int v3;
  uint v4;

  v1 = 0;
  GameSystems.Feat.FeatListElective(evt.objHndCaller, 0);
  v2 = evt.objHndCaller.GetStat(Stat.charisma);
  v3 = D20StatSystem.GetModifierForAbilityScore(v2);
  v4 = GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.EXTRA_TURNING);
  if ( v4 )
  {
    v1 = 4 * v4;
  }
  evt.SetConditionArg2(v1 + v3 + 3);
}


[DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
[TempleDllLocation(0x1004bc90)]
public static void   sub_1004BC90(in DispatcherCallbackArgs evt)
{
  int condArg3;
  int v2;

  if ( (evt.GetConditionArg2() )!=0)
  {
    condArg3 = evt.GetConditionArg3();
    GameSystems.ParticleSys.Remove(condArg3);
  }
  v2 = evt.objHndCaller.GetStat(Stat.level_cleric);
  evt.SetConditionArg1(v2);
  evt.SetConditionArg2(0);
}


[DispTypes(DispatcherType.DestructionDomain)]
[TempleDllLocation(0x1004b700)]
public static void   sub_1004B700(in DispatcherCallbackArgs evt)
{
  int condArg1;

  condArg1 = evt.GetConditionArg1();
  if ( condArg1 > 0 )
  {
    evt.SetConditionArg1(condArg1 - 1);
    evt.objHndCaller.AddCondition(DomainConditions.DestructionDomainSmite);
    GameSystems.Deity.DeityPraiseFloatMessage(evt.objHndCaller);
  }
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x1004ad40)]
public static void   TurnUndeadRadialMenuEntry(in DispatcherCallbackArgs evt)
{  int v2;
  int v3;
  string meslineValue;
int meslineKey;
  RadialMenuEntry radMenuEntry;

  if ( !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_IsFallenPaladin) )
  {
    radMenuEntry = RadialMenuEntry.Create();/*INLINED:v1=evt.subDispNode.condNode*/    radMenuEntry.d20ActionType = D20ActionType.TURN_UNDEAD;
    radMenuEntry.d20ActionData1 = evt.GetConditionArg1();
    meslineKey = radMenuEntry.d20ActionData1 + 5028;
    meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
    radMenuEntry.text = (string )meslineValue;
    radMenuEntry.helpSystemHashkey = "TAG_TURN"/*ELFHASH*/;
    v3 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Class);
    GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v3);
  }
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x1004b7e0)]
public static void   GoodFortune_RadialMenuEntry_Callback(in DispatcherCallbackArgs evt)
{
  int v1;
  int v2;
  int v3;  int v5;  int v7;  int v9;
  string meslineValue;
int meslineKey;
  RadialMenuEntry radMenuEntry;

  meslineKey = 5024;
  radMenuEntry = RadialMenuEntry.Create();
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  radMenuEntry.text = (string )meslineValue;
  radMenuEntry.type = 0;
  radMenuEntry.minArg = 0;
  radMenuEntry.maxArg = 0;
  v2 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Class);
  v3 = GameSystems.D20.RadialMenu.AddParentChildNode(evt.objHndCaller, ref radMenuEntry, v2);
  radMenuEntry = RadialMenuEntry.Create();
  radMenuEntry.minArg = 0;/*INLINED:v4=evt.subDispNode.condNode*/  radMenuEntry.maxArg = 1;
  radMenuEntry.type = RadialMenuEntryType.Toggle;
  radMenuEntry.actualArg = (int)CondNodeGetArgPtr/*0x100e1af0*/(evt.subDispNode.condNode, 1);
  radMenuEntry.callback = GameSystems.D20.RadialMenu.RadialMenuCheckboxOrSliderCallback;
  meslineKey = 5025;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  radMenuEntry.text = (string )meslineValue;
  radMenuEntry.helpSystemHashkey = "TAG_LUCK_D"/*ELFHASH*/;
  GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v3);
  radMenuEntry = RadialMenuEntry.Create();/*INLINED:v6=evt.subDispNode.condNode*/  radMenuEntry.maxArg = 1;
  radMenuEntry.minArg = 0;
  radMenuEntry.type = RadialMenuEntryType.Toggle;
  radMenuEntry.actualArg = (int)CondNodeGetArgPtr/*0x100e1af0*/(evt.subDispNode.condNode, 2);
  radMenuEntry.callback = GameSystems.D20.RadialMenu.RadialMenuCheckboxOrSliderCallback;
  meslineKey = 5026;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  radMenuEntry.helpSystemHashkey = "TAG_LUCK_D"/*ELFHASH*/;
  radMenuEntry.text = (string )meslineValue;
  GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v3);
  radMenuEntry = RadialMenuEntry.Create();/*INLINED:v8=evt.subDispNode.condNode*/  radMenuEntry.maxArg = 1;
  radMenuEntry.minArg = 0;
  radMenuEntry.type = RadialMenuEntryType.Toggle;
  radMenuEntry.actualArg = (int)CondNodeGetArgPtr/*0x100e1af0*/(evt.subDispNode.condNode, 3);
  radMenuEntry.callback = GameSystems.D20.RadialMenu.RadialMenuCheckboxOrSliderCallback;
  meslineKey = 5027;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  radMenuEntry.text = (string )meslineValue;
  radMenuEntry.helpSystemHashkey = "TAG_LUCK_D"/*ELFHASH*/;
  GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v3);
}

}
}
/*

TurnUndeadSorter_ByDistance @ 0x1004ae30 = 1
GetGroupMember @ 0x100df760 = 1
GroupArraySortFuncSetAndSort @ 0x100dfa00 = 1
ObjListFree @ 0x1001f2c0 = 1
SpellEntry_Get_Descriptor @ 0x10075380 = 1
GetSpellSubschool @ 0x10075340 = 1
GroupArraySort @ 0x100df900 = 1
GroupArrayAdd @ 0x100df990 = 1
DestructionDomainRadialCallback @ 0x100f02c0 = 1
turnUndeadInvoker @ 0x11e61540 = 1
CritterKill_Simple @ 0x100b8990 = 1
GroupArrayReset @ 0x100df930 = 1
D20QueryWithDataDefault1 @ 0x1004ccd0 = 1
off_102B17A4 @ 0x102b17a4 = 1
rebukeUndeadTypeCheck @ 0x102b17c4 = 2
GetGroupLength @ 0x100df750 = 2
CondNodeGetArgPtr @ 0x100e1af0 = 3
DiceRoller @ 0x10038b60 = 3
turnUndeadRange @ 0x102b17e4 = 4
*/