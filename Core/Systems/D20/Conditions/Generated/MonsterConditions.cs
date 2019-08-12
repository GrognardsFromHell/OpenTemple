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

public static class MonsterConditions {

private static readonly ILogger Logger = new ConsoleLogger();
[TempleDllLocation(0x102eb620)]
  public static readonly ConditionSpec MonsterBansheeCharismaDrain = ConditionSpec.Create("Monster Banshee Charisma Drain", 0)
.Prevents(MonsterBansheeCharismaDrain)
.AddHandler(DispatcherType.DealingDamage2, BansheeCharismaDrainOnDamage)
                    .Build();


[TempleDllLocation(0x102eb668)]
  public static readonly ConditionSpec MonsterDamageType = ConditionSpec.Create("Monster Damage Type", 1)
.Prevents(MonsterDamageType)
.AddHandler(DispatcherType.DealingDamage, SetMonsterDamageType)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 0)
                    .Build();


[TempleDllLocation(0x102eb6c0)]
  public static readonly ConditionSpec MonsterBonusDamage = ConditionSpec.Create("Monster Bonus Damage", 2)
.Prevents(MonsterBonusDamage)
.AddHandler(DispatcherType.DealingDamage, MonsterDamageBonus)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 0)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 1)
                    .Build();


[TempleDllLocation(0x102eb730)]
  public static readonly ConditionSpec MonsterStirge = ConditionSpec.Create("Monster Stirge", 6)
.Prevents(MonsterStirge)
.AddHandler(DispatcherType.DealingDamage2, sub_100F69A0)
.AddHandler(DispatcherType.BeginRound, StirgeAttach_callback)
.AddSignalHandler(D20DispatcherKey.SIG_Pack, CommonConditionCallbacks.D20SignalPackHandler, 2)
.AddSignalHandler(D20DispatcherKey.SIG_Unpack, CommonConditionCallbacks.D20SignalUnpackHandler, 2)
.AddHandler(DispatcherType.TurnBasedStatusInit, sub_100F6FB0)
.AddHandler(DispatcherType.GetAC, MonsterStigeAcBonusCap, 2)
.AddQueryHandler(D20DispatcherKey.QUE_AOOPossible, sub_100F6F80)
                    .Build();


[TempleDllLocation(0x102eb7f0)]
  public static readonly ConditionSpec MonsterFireBats = ConditionSpec.Create("Monster Fire Bats", 6)
.Prevents(MonsterFireBats)
.AddHandler(DispatcherType.DealingDamage2, sub_100F6A70)
.AddHandler(DispatcherType.BeginRound, FireBats_callback)
.AddSignalHandler(D20DispatcherKey.SIG_Pack, CommonConditionCallbacks.D20SignalPackHandler, 2)
.AddSignalHandler(D20DispatcherKey.SIG_Unpack, CommonConditionCallbacks.D20SignalUnpackHandler, 2)
.AddHandler(DispatcherType.TurnBasedStatusInit, sub_100F6FB0)
.AddQueryHandler(D20DispatcherKey.QUE_AOOPossible, sub_100F6F80)
.AddHandler(DispatcherType.Tooltip, sub_100F6E80, 5045)
                    .Build();


[TempleDllLocation(0x102eb8b0)]
  public static readonly ConditionSpec MonsterMeleeDisease = ConditionSpec.Create("Monster Melee Disease", 1)
.Prevents(MonsterMeleeDisease)
.AddHandler(DispatcherType.DealingDamage2, ConditionAddOnDamage, StatusEffects.IncubatingDisease)
.AddQueryHandler((D20DispatcherKey)202, VerifyObjConditionsCallback, 0, 0)
                    .Build();


[TempleDllLocation(0x102eb908)]
  public static readonly ConditionSpec MonsterMeleePoison = ConditionSpec.Create("Monster Melee Poison", 1)
.Prevents(MonsterMeleePoison)
.AddHandler(DispatcherType.DealingDamage2, ConditionAddOnDamage, StatusEffects.Poisoned)
.AddQueryHandler((D20DispatcherKey)202, VerifyObjConditionsCallback, 0, 0)
                    .Build();


[TempleDllLocation(0x102eb960)]
  public static readonly ConditionSpec MonsterCarrionCrawler = ConditionSpec.Create("Monster Carrion Crawler", 0)
.Prevents(MonsterCarrionCrawler)
.AddHandler(DispatcherType.DealingDamage2, CarrionCrawlerParalysisOnDamage)
                    .Build();


[TempleDllLocation(0x102eb9a8)]
  public static readonly ConditionSpec MonsterMeleeParalysis = ConditionSpec.Create("Monster Melee Paralysis", 2)
.Prevents(MonsterMeleeParalysis)
.AddHandler(DispatcherType.DealingDamage2, MonsterMeleeParalysisOnDamage)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 0)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 1)
                    .Build();


[TempleDllLocation(0x102eba18)]
  public static readonly ConditionSpec MonsterMeleeParalysisNoElf = ConditionSpec.Create("Monster Melee Paralysis No Elf", 2)
.Prevents(MonsterMeleeParalysisNoElf)
.AddHandler(DispatcherType.DealingDamage2, MonsterMeleeParalysisNoElfPreAdd)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 0)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 1)
                    .Build();


[TempleDllLocation(0x102eba88)]
  public static readonly ConditionSpec MonsterEnergyImmunity = ConditionSpec.Create("Monster Energy Immunity", 1)
.Prevents(MonsterEnergyImmunity)
.AddHandler(DispatcherType.TakingDamage2, MonsterEnergyImmunityOnDamage)
.AddQueryHandler(D20DispatcherKey.QUE_AI_Fireball_OK, sub_100F72B0)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 0)
                    .Build();


[TempleDllLocation(0x102ebaf8)]
  public static readonly ConditionSpec MonsterEnergyResistance = ConditionSpec.Create("Monster Energy Resistance", 2)
.Prevents(MonsterEnergyImmunity)
.AddHandler(DispatcherType.TakingDamage2, MonsterEnergyResistanceOnDamage)
.AddQueryHandler(D20DispatcherKey.QUE_AI_Fireball_OK, sub_100F72B0)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 0)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 1)
                    .Build();


[TempleDllLocation(0x102ebb78)]
  public static readonly ConditionSpec MonsterRegeneration5 = ConditionSpec.Create("Monster Regeneration 5", 2)
.Prevents(MonsterRegeneration5)
.AddHandler(DispatcherType.TakingDamage2, MonsterRegenerationOnDamage)
.AddHandler(DispatcherType.BeginRound, sub_100F7350, 5)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 0)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 1)
                    .Build();


[TempleDllLocation(0x102ebbf8)]
  public static readonly ConditionSpec MonsterRegeneration2 = ConditionSpec.Create("Monster Regeneration 2", 2)
.Prevents(MonsterRegeneration5)
.AddHandler(DispatcherType.TakingDamage2, MonsterRegenerationOnDamage)
.AddHandler(DispatcherType.BeginRound, sub_100F7350, 2)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 0)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 1)
                    .Build();


[TempleDllLocation(0x102ebc78)]
  public static readonly ConditionSpec MonsterRegeneration1 = ConditionSpec.Create("Monster Regeneration 1", 2)
.Prevents(MonsterRegeneration5)
.AddHandler(DispatcherType.TakingDamage2, MonsterRegenerationOnDamage)
.AddHandler(DispatcherType.BeginRound, sub_100F7350, 1)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 0)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 1)
                    .Build();


[TempleDllLocation(0x102ebcf8)]
  public static readonly ConditionSpec MonsterSalamander = ConditionSpec.Create("Monster Salamander", 0)
.Prevents(MonsterSalamander)
.AddHandler(DispatcherType.TakingDamage2, SalamanderTakingDamageReactionDamage)
.SetQueryResult(D20DispatcherKey.QUE_AI_Fireball_OK, true)
                    .Build();


[TempleDllLocation(0x102ebd50)]
  public static readonly ConditionSpec MonsterOozeSplit = ConditionSpec.Create("Monster Ooze Split", 0)
.Prevents(MonsterSalamander)
.AddHandler(DispatcherType.TakingDamage2, MonsterOozeSplittingOnDamage)
.SetQueryResult(D20DispatcherKey.QUE_AI_Fireball_OK, true)
                    .Build();


[TempleDllLocation(0x102ebda8)]
  public static readonly ConditionSpec MonsterSplitting = ConditionSpec.Create("Monster Splitting", 0)
.Prevents(MonsterSplitting)
.AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, MonsterSplittingHpChange)
                    .Build();


[TempleDllLocation(0x102ebdf0)]
  public static readonly ConditionSpec MonsterJuggernaut = ConditionSpec.Create("Monster Juggernaut", 0)
.Prevents(MonsterJuggernaut)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPECIAL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPECIAL)
.AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 0, 0)
.Prevents(StatusEffects.Poisoned)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Poison, true)
.Prevents(StatusEffects.Paralyzed)
.Prevents(StatusEffects.Stunned)
.Prevents(StatusEffects.IncubatingDisease)
.Prevents(StatusEffects.NSDiseased)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Death_Touch, true)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Critical_Hits, true)
.AddHandler(DispatcherType.TakingDamage2, CommonConditionCallbacks.SubdualImmunityDamageCallback)
.Prevents(StatusEffects.TempAbilityLoss)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Energy_Drain, true)
.AddHandler(DispatcherType.TakingDamage2, ImmunityToAcidElectricityFireDamageCallback)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Has_No_Con_Score, true)
                    .Build();


[TempleDllLocation(0x102ebf50)]
  public static readonly ConditionSpec MonsterSpellResistance = ConditionSpec.Create("Monster Spell Resistance", 1)
.Prevents(MonsterSpellResistance)
.AddHandler(DispatcherType.ConditionAdd, CommonConditionCallbacks.SpellResistanceDebug)
.AddHandler(DispatcherType.SpellResistanceMod, CommonConditionCallbacks.SpellResistanceMod_Callback, 5048)
.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Spell_Resistance, CommonConditionCallbacks.SpellResistanceQuery)
.AddHandler(DispatcherType.Tooltip, CommonConditionCallbacks.TooltipSpellResistanceCallback, 5048)
.SetQueryResult(D20DispatcherKey.QUE_AI_Fireball_OK, true)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 0)
                    .Build();


[TempleDllLocation(0x102ebff8)]
  public static readonly ConditionSpec MonsterSmiting = ConditionSpec.Create("Monster Smiting", 1)
.Prevents(MonsterSmiting)
.AddHandler(DispatcherType.DealingDamage, sub_100F76C0)
.AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, CommonConditionCallbacks.CondNodeSetArgToZero)
                    .Build();


[TempleDllLocation(0x102ec050)]
  public static readonly ConditionSpec MonsterZombie = ConditionSpec.Create("Monster Zombie", 0)
.AddHandler(DispatcherType.TurnBasedStatusInit, CommonConditionCallbacks.turnBasedStatusInitSingleActionOnly)
                    .Build();


[TempleDllLocation(0x102ec080)]
  public static readonly ConditionSpec MonsterLamia = ConditionSpec.Create("Monster Lamia", 0)
.AddHandler(DispatcherType.DealingDamage2, sub_100F77B0)
                    .Build();


[TempleDllLocation(0x102ec0b0)]
  public static readonly ConditionSpec MonsterDRCold = ConditionSpec.Create("Monster DR Cold", 1)
.AddHandler(DispatcherType.TakingDamage2, MonsterDRDamageCallback, 4096)
.AddHandler(DispatcherType.DealingDamage, sub_100F7840, 4096)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 0)
                    .Build();


[TempleDllLocation(0x102ec108)]
  public static readonly ConditionSpec MonsterDRColdHoly = ConditionSpec.Create("Monster DR Cold-Holy", 1)
.AddHandler(DispatcherType.TakingDamage2, MonsterDRDamageCallback, 4104)
.AddHandler(DispatcherType.DealingDamage, sub_100F7840, 4104)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 0)
                    .Build();


[TempleDllLocation(0x102ec160)]
  public static readonly ConditionSpec MonsterDRMagic = ConditionSpec.Create("Monster DR Magic", 1)
.AddHandler(DispatcherType.TakingDamage2, MonsterDRDamageCallback, 4)
.AddHandler(DispatcherType.DealingDamage, sub_100F7840, 4)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 0)
                    .Build();


[TempleDllLocation(0x102ec1b8)]
  public static readonly ConditionSpec MonsterDRAll = ConditionSpec.Create("Monster DR All", 1)
.AddHandler(DispatcherType.TakingDamage2, MonsterDRDamageCallback, 1)
.AddHandler(DispatcherType.DealingDamage, sub_100F7840, 1)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 0)
                    .Build();


[TempleDllLocation(0x102ec210)]
  public static readonly ConditionSpec MonsterDRSilver = ConditionSpec.Create("Monster DR Silver", 1)
.AddHandler(DispatcherType.TakingDamage2, MonsterDRDamageCallback, 2)
.AddHandler(DispatcherType.DealingDamage, sub_100F7840, 2)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 0)
                    .Build();


[TempleDllLocation(0x102ec268)]
  public static readonly ConditionSpec MonsterDRHoly = ConditionSpec.Create("Monster DR Holy", 1)
.AddHandler(DispatcherType.TakingDamage2, MonsterDRDamageCallback, 8)
.AddHandler(DispatcherType.DealingDamage, sub_100F7840, 8)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 0)
                    .Build();


[TempleDllLocation(0x102ec34c)]
  public static readonly ConditionSpec MonsterSuperiorTwoWeaponFighting = ConditionSpec.Create("Monster Superior Two Weapon Fighting", 0)
.AddHandler(DispatcherType.ToHitBonus2, sub_100F7870)
                    .Build();


[TempleDllLocation(0x102ec37c)]
  public static readonly ConditionSpec MonsterStable = ConditionSpec.Create("Monster Stable", 0)
.AddHandler(DispatcherType.AbilityCheckModifier, CommonConditionCallbacks.AbilityModCheckStabilityBonus)
                    .Build();


[TempleDllLocation(0x102ec46c)]
  public static readonly ConditionSpec MonsterUntripable = ConditionSpec.Create("Monster Untripable", 0)
.SetQueryResult(D20DispatcherKey.QUE_Untripable, true)
                    .Build();


[TempleDllLocation(0x102ec3b0)]
  public static readonly ConditionSpec MonsterPlant = ConditionSpec.Create("Monster Plant", 0)
.Prevents(MonsterPlant)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPECIAL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPECIAL)
.AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 1, 0)
.Prevents(StatusEffects.Poisoned)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Poison, true)
.Prevents(StatusEffects.Paralyzed)
.Prevents(StatusEffects.Stunned)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Critical_Hits, true)
                    .Build();


[TempleDllLocation(0x102ec49c)]
  public static readonly ConditionSpec MonsterHootingFungi = ConditionSpec.Create("Monster Hooting Fungi", 0)
.AddHandler(DispatcherType.TakingDamage2, sub_100F7910)
                    .Build();


[TempleDllLocation(0x102ec4d0)]
  public static readonly ConditionSpec MonsterSpider = ConditionSpec.Create("Monster Spider", 0)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPECIAL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPECIAL)
.AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 2, 0)
                    .Build();


[TempleDllLocation(0x102ec514)]
  public static readonly ConditionSpec MonsterIncorporeal = ConditionSpec.Create("Monster Incorporeal", 0)
.AddHandler(DispatcherType.TakingDamage2, MonsterIncorporealDamageCallback)
                    .Build();


[TempleDllLocation(0x102ec548)]
  public static readonly ConditionSpec MonsterMinotaurCharge = ConditionSpec.Create("Monster Minotaur Charge", 0)
.AddHandler(DispatcherType.DealingDamage, MinotaurChargeCallback)
.AddQueryHandler(D20DispatcherKey.QUE_Play_Critical_Hit_Anim, MinotaurChargeCriticalQuery)
                    .Build();


[TempleDllLocation(0x102ec590)]
  public static readonly ConditionSpec MonsterFastHealing = ConditionSpec.Create("Monster Fast Healing", 1)
.AddHandler(DispatcherType.BeginRound, sub_100F7AA0)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 0)
                    .Build();


[TempleDllLocation(0x102ec5d8)]
  public static readonly ConditionSpec MonsterPoisonImmunity = ConditionSpec.Create("Monster Poison Immunity", 0)
.Prevents(StatusEffects.Poisoned)
.SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Poison, true)
                    .Build();


[TempleDllLocation(0x102ec2c0)]
  public static readonly ConditionSpec MonsterDRBludgeoning = ConditionSpec.Create("Monster DR Bludgeoning", 1)
.AddHandler(DispatcherType.TakingDamage2, MonsterDRDamageCallback, 256)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 0)
                    .Build();


[TempleDllLocation(0x102ec308)]
  public static readonly ConditionSpec MonsterDRSlashing = ConditionSpec.Create("Monster DR Slashing", 1)
.AddHandler(DispatcherType.TakingDamage2, MonsterDRDamageCallback, 1024)
.AddQueryHandler((D20DispatcherKey)202, sub_100EFDF0, 0)
                    .Build();


[TempleDllLocation(0x102ec620)]
  public static readonly ConditionSpec MonsterSubdualImmunity = ConditionSpec.Create("Monster Subdual Immunity", 0)
.Prevents(MonsterSubdualImmunity)
.AddHandler(DispatcherType.TakingDamage2, CommonConditionCallbacks.SubdualImmunityDamageCallback)
                    .Build();


[TempleDllLocation(0x102ec668)]
  public static readonly ConditionSpec MonsterSpecialFadeOut = ConditionSpec.Create("Monster Special Fade Out", 0)
.Prevents(MonsterSpecialFadeOut)
.AddHandler(DispatcherType.TakingDamage2, CommonConditionCallbacks.SubdualImmunityDamageCallback)
.AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, sub_100F7B30)
                    .Build();


[TempleDllLocation(0x102ec6c0)]
  public static readonly ConditionSpec MonsterConfusionImmunity = ConditionSpec.Create("Monster Confusion Immunity", 0)
.Prevents(MonsterConfusionImmunity)
.AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_SPECIAL, CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_SPECIAL)
.AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 3, 0)
                    .Build();


public static IReadOnlyList<ConditionSpec> Conditions {get;} = new List<ConditionSpec>
{
MonsterDRSlashing,
MonsterStable,
MonsterLamia,
MonsterSmiting,
MonsterRegeneration5,
MonsterMeleeDisease,
MonsterRegeneration1,
MonsterBansheeCharismaDrain,
MonsterMeleeParalysisNoElf,
MonsterRegeneration2,
MonsterSalamander,
MonsterOozeSplit,
MonsterPlant,
MonsterUntripable,
MonsterFastHealing,
MonsterDRHoly,
MonsterSpecialFadeOut,
MonsterDRMagic,
MonsterDRAll,
MonsterDRCold,
MonsterDRSilver,
MonsterJuggernaut,
MonsterMeleeParalysis,
MonsterStirge,
MonsterIncorporeal,
MonsterEnergyImmunity,
MonsterDamageType,
MonsterPoisonImmunity,
MonsterCarrionCrawler,
MonsterSubdualImmunity,
MonsterEnergyResistance,
MonsterHootingFungi,
MonsterSpider,
MonsterSplitting,
MonsterSuperiorTwoWeaponFighting,
MonsterSpellResistance,
MonsterFireBats,
MonsterZombie,
MonsterDRBludgeoning,
MonsterMinotaurCharge,
MonsterConfusionImmunity,
MonsterDRColdHoly,
MonsterMeleePoison,
MonsterBonusDamage,
};

[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100f6930)]
public static void   SetMonsterDamageType(in DispatcherCallbackArgs evt)
{
  DamagePacket v1;
  int condArg1;

  v1 = &evt.GetDispIoDamage().damage;
  condArg1 = evt.GetConditionArg1();
  SetDamageType/*0x100e0540*/(v1, (D20DT)condArg1);
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100f7870)]
public static void   sub_100F7870(in DispatcherCallbackArgs evt)
{
  DispIoAttackBonus dispIo;
  DispIoAttackBonus v2;
  int v3;
  int v4;

  dispIo = evt.GetDispIoAttackBonus();
  v2 = dispIo;
  v3 = dispIo.attackPacket.dispKey;
  if ( v3 == 6 || v3 == 8 )
  {
    v2.bonlist.AddBonus(10, 0, 114);
  }
  v4 = v2.attackPacket.dispKey;
  if ( v4 == 5 || v4 == 7 || v4 == 9 )
  {
    v2.bonlist.AddBonus(6, 0, 114);
  }
}


[DispTypes(DispatcherType.DealingDamage2)]
[TempleDllLocation(0x100f7030)]
public static void   ConditionAddOnDamage(in DispatcherCallbackArgs evt, ConditionSpec data)
{
  int condArg1;
  ConditionSpec v2;
  int v3;
  DispIoDamage dispIo;
  int v5;
  int v6;

  condArg1 = evt.GetConditionArg1();
  v2 = (ConditionSpec )data;
  v3 = condArg1;
  dispIo = evt.GetDispIoDamage();
  if ( !(dispIo.attackPacket.flags & 4) && dispIo.attackPacket.dispKey >= 10 )
  {
    v5 = dispIo.attackPacket.victim;
    v6 = HIDWORD(dispIo.attackPacket.victim);
    if ( v2 == StatusEffects.IncubatingDisease )
    {
      __PAIR__(v6, v5).AddCondition(v2, 0, v3, 0);
    }
    else
    {
      __PAIR__(v6, v5).AddCondition(v2, v3, 0);
    }
  }
}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100f72e0)]
[TemplePlusLocation("condition.cpp:505")]
public static void   MonsterRegenerationOnDamage(in DispatcherCallbackArgs evt)
{
  DispIoDamage dispIo;
  int condArg1;
  int condArg2;
  DamagePacket v4;

  dispIo = evt.GetDispIoDamage();
  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  if ( condArg1 < 0 )
  {
    if ( (condArg1 & 0x7FFFFFFF) == dispIo.damage.attackPowerType )
    {
      return;
    }
    condArg1 = condArg2;
  }
  v4 = &dispIo.damage;
  if ( ConvertDamageDice/*0x100e0560*/(v4, 13, condArg1, condArg2) )
  {
    DamageZeroBonusLineAdd/*0x100e11f0*/(v4, 322);
  }
}
/* Orphan comments:
TP Replaced @ condition.cpp:505
*/


[DispTypes(DispatcherType.DealingDamage2)]
[TempleDllLocation(0x100f71d0)]
public static void   MonsterMeleeParalysisNoElfPreAdd(in DispatcherCallbackArgs evt)
{
  DispIoDamage dispIo;
  
  dispIo = evt.GetDispIoDamage();
  if ( dispIo.attackPacket.victim.GetStat((Stat)0x120) != 2 )
  {
                    MonsterConditions.MonsterMeleeParalysisOnDamage(in evt);
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100efdf0)]
public static void   sub_100EFDF0(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  int v2;

  if ( evt.GetConditionArg(data) <= 0 )
  {
    v1 = evt.GetConditionArg(data);
    v2 = GetDisplayNameForDebug/*0x10021200*/(evt.objHndCaller);
    Logger.Info("Invalid value for {0} on {1}.  {2} is not greater than 0", evt.subDispNode.condNode.condStruct.condName, v2, v1);
  }
}


[DispTypes(DispatcherType.DealingDamage2)]
[TempleDllLocation(0x100f6a70)]
public static void   sub_100F6A70(in DispatcherCallbackArgs evt)
{
  
  if ( !evt.GetConditionArg1() )
  {
                    MonsterConditions.sub_100F69A0(in evt);
  }
}


[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x100f7aa0)]
public static void   sub_100F7AA0(in DispatcherCallbackArgs evt)
{
  int v1;
  Dice v2;

  evt.GetConditionArg1();
  v1 = 10 * evt.GetDispIoD20Signal().data1;
  if ( !GameSystems.Critter.IsDeadNullDestroyed(evt.objHndCaller)
    && evt.objHndCaller.GetInt32(obj_f.hp_damage) > 0
    && evt.objHndCaller.GetInt32(obj_f.critter_subdual_damage) > 0 )
  {
    v2 = Dice.Constant(v1);
    Heal/*0x100b7df0*/(evt.objHndCaller, evt.objHndCaller, v2, 0);
    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x39, evt.objHndCaller, null);
  }
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100f7550)]
[TemplePlusLocation("ability_fixes.cpp:76")]
public static void   MonsterSplittingHpChange(in DispatcherCallbackArgs evt)
{
  int protoId;
  int hpCur;
  GameObjectBody v3;
  int v4;
  
    GameObjectBody handleNew;
  LocAndOffsets locAndOffOut;

  protoId = evt.objHndCaller.ProtoId;
  evt.objHndCaller.GetLocationFull(&locAndOffOut);
  hpCur = evt.objHndCaller.GetStat(Stat.hp_current, 0);
  if ( hpCur > 10 )
  {
    v3 = GetProtoHandle/*0x1003ad70*/(protoId);
    GameSystems.MapObject.CreateObject(v3, locAndOffOut.location, &handleNew);
    GameSystems.Ai.ForceSpreadOut(handleNew, &locAndOffOut);
    evt.objHndCaller.SetInt32(obj_f.hp_pts, hpCur / 2);
    handleNew.SetInt32(obj_f.hp_pts, hpCur / 2);
    evt.objHndCaller.SetInt32(obj_f.hp_damage, 0);
    handleNew.SetInt32(obj_f.hp_damage, 0);
    v4 = handleNew.GetInt32(obj_f.critter_flags);
    handleNew.SetInt32(obj_f.critter_flags, v4 | CritterFlag.EXPERIENCE_AWARDED);
    GameSystems.ParticleSys.CreateAtObj("hit-Acid-medium", evt.objHndCaller);
                CommonConditionCallbacks.conditionRemoveCallback(in evt);
  }
}
/* Orphan comments:
TP Replaced @ ability_fixes.cpp:76
*/


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100f7840)]
public static void   sub_100F7840(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  DispIoDamage dispIo;

  v1 = data;
  dispIo = evt.GetDispIoDamage();
  AddAttackPowerType/*0x100e0520*/(&dispIo.damage, v1 | 4);
}


[DispTypes(DispatcherType.DealingDamage2)]
[TempleDllLocation(0x100f7110)]
public static void   MonsterMeleeParalysisOnDamage(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg2;
  DispIoDamage dispIo;
  int v4;
  int v5;
  int v6;
  int v7;

  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  dispIo = evt.GetDispIoDamage();
  if ( !(dispIo.attackPacket.flags & D20CAF.RANGED)
    && dispIo.attackPacket.victim.GetStat((Stat)288) != 2
    && !GameSystems.D20.Combat.SavingThrow(dispIo.attackPacket.victim, dispIo.attackPacket.attacker, condArg1, 0, 0) )
  {
    v4 = GetPackedDiceBonus/*0x10038c90*/(condArg2);
    v5 = GetPackedDiceType/*0x10038c40*/(condArg2);
    v6 = GetPackedDiceNumDice/*0x10038c30*/(condArg2);
    v7 = Dice.Roll(v6, v5, v4);
    dispIo.attackPacket.victim.AddCondition(StatusEffects.Paralyzed, v7, 0);
  }
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100f7b30)]
public static void   sub_100F7B30(in DispatcherCallbackArgs evt)
{
  if ( (int)evt.objHndCaller.GetStat(Stat.hp_current, 0) <= 0 )
  {
    GameSystems.D20.Combat.Kill(evt.objHndCaller, null);
  }
}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100f7a20)]
public static void   MonsterIncorporealDamageCallback(in DispatcherCallbackArgs evt)
{
  DispIoDamage dispIo;

  dispIo = evt.GetDispIoDamage();
  AddIncorporealImmunity/*0x100e0780*/(&dispIo.damage);
}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100f7800)]
public static void   MonsterDRDamageCallback(in DispatcherCallbackArgs evt, int data)
{
  int condArg1;
  int v2;
  int v3;
  DispIoDamage dispIo;

  condArg1 = evt.GetConditionArg1();
  v2 = data;
  v3 = condArg1;
  dispIo = evt.GetDispIoDamage();
  dispIo.damage.AddPhysicalDR(v3, v2, 0x7E);
}


[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x100f6ab0)]
public static void   StirgeAttach_callback(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg3;
  int v3;
  int v4;
  int v5;
  int i;
  GameObjectBody v7;
  int condArg2;
  int condArg4;
  DispIoBonusList a1;

  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  condArg3 = evt.GetConditionArg3();
  condArg4 = evt.GetConditionArg4();
  v3 = Dice.Roll(1, 4, 0);
  if ( condArg1 )
  {
    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x1E, evt.objHndCaller, __PAIR__(condArg3, condArg4));
    GameSystems.D20.Combat.FloatCombatLine(__PAIR__(condArg3, condArg4), 155);
    __PAIR__(condArg3, condArg4).AddCondition(StatusEffects.TempAbilityLoss, 2, v3);
    v4 = condArg2 + v3;
    evt.SetConditionArg2(v4);
    a1 = new DispIoBonusList();
    if ( (__PAIR__(condArg3, condArg4).GetStat(Stat.hp_current, &a1) & 0x80000000) == 0
      && (a1 = new DispIoBonusList(), (evt.objHndCaller.GetStat(Stat.hp_current, &a1) & 0x80000000) == 0) )
    {
      DispIoBonusListDebug/*0x1004da70*/(&a1);
      if ( v4 >= 4 )
      {
        evt.SetConditionArg1(0);
        if ( GameSystems.Party.IsInParty(__PAIR__(condArg3, condArg4)) )
        {
          v5 = 0;
          for ( i = FindInParty/*0x1002b1e0*/(__PAIR__(condArg3, condArg4)); v5 < GameSystems.Party.PartySize(); ++v5 )
          {
            if ( v5 != i )
            {
              v7 = GameSystems.Party.GetPartyGroupMemberN(v5);
              Obj_AI_Flee_Add/*0x1005de60*/(evt.objHndCaller, v7);
            }
          }
        }
        Obj_AI_Flee_Add/*0x1005de60*/(evt.objHndCaller, __PAIR__(condArg3, condArg4));
      }
    }
    else
    {
      evt.SetConditionArg1(0);
      DispIoBonusListDebug/*0x1004da70*/(&a1);
    }
  }
}


[DispTypes(DispatcherType.DealingDamage2)]
[TempleDllLocation(0x100f69a0)]
public static void   sub_100F69A0(in DispatcherCallbackArgs evt)
{
  int condArg1;
  DispIoDamage dispIo;
  int v3;
  int condArg3;
  int condArg4;
  string v6;
  string v7;

  condArg1 = evt.GetConditionArg1();
  dispIo = evt.GetDispIoDamage();
  if ( dispIo.attackPacket.dispKey >= 10 && !condArg1 )
  {
    v3 = Int64Shr/*0x10255e30*/(dispIo.attackPacket.victim, 0x20);
    evt.SetConditionArg3(v3);
    evt.SetConditionArg4(dispIo.attackPacket.victim);
    condArg3 = evt.GetConditionArg3();
    condArg4 = evt.GetConditionArg4();
    evt.SetConditionArg1(1);
    v6 = GameSystems.MapObject.GetDisplayName(__PAIR__(condArg3, condArg4), __PAIR__(condArg3, condArg4));
    v7 = GameSystems.MapObject.GetDisplayName(evt.objHndCaller, evt.objHndCaller);
    Float_Combat_mes_with_extra_Strings/*0x100b4bf0*/(evt.objHndCaller, combat_mes_attached, v7, (int)v6);
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100f72b0)]
public static void   sub_100F72B0(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;

  dispIo = evt.GetDispIoD20Query();
  if ( evt.GetConditionArg1() == 10 )
  {
    dispIo.return_val = 1;
  }
}


[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x100f6c80)]
public static void   FireBats_callback(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg4;
  Dice v3;
  Dice v4;
  int v5;
  GameObjectBody v6;
  int condArg2;
  int v8;
  int i;
  int condArg3;
  DispIoBonusList a1;

  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  condArg3 = evt.GetConditionArg3();
  condArg4 = evt.GetConditionArg4();
  if ( condArg1 )
  {
    v8 = condArg2 + 1;
    evt.SetConditionArg2(v8);
    a1 = new DispIoBonusList();
    if ( (__PAIR__(condArg3, condArg4).GetStat(Stat.hp_current, &a1) & 0x80000000) == 0
      && (DispIoBonusListDebug/*0x1004da70*/(&a1),
          a1 = new DispIoBonusList(),
          (evt.objHndCaller.GetStat(Stat.hp_current, &a1) & 0x80000000) == 0) )
    {
      DispIoBonusListDebug/*0x1004da70*/(&a1);
      if ( v8 > 3 )
      {
        evt.SetConditionArg1(0);
        if ( GameSystems.Party.IsInParty(__PAIR__(condArg3, condArg4)) )
        {
          v5 = 0;
          for ( i = FindInParty/*0x1002b1e0*/(__PAIR__(condArg3, condArg4)); v5 < GameSystems.Party.PartySize(); ++v5 )
          {
            if ( v5 != i )
            {
              v6 = GameSystems.Party.GetPartyGroupMemberN(v5);
              Obj_AI_Flee_Add/*0x1005de60*/(evt.objHndCaller, v6);
            }
          }
        }
        Obj_AI_Flee_Add/*0x1005de60*/(evt.objHndCaller, __PAIR__(condArg3, condArg4));
      }
      else
      {
        GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x1E, evt.objHndCaller, __PAIR__(condArg3, condArg4));
        GameSystems.D20.Combat.FloatCombatLine(__PAIR__(condArg3, condArg4), 155);
        v3 = Dice.D6;
        GameSystems.D20.Combat.DoDamage(__PAIR__(condArg3, condArg4), evt.objHndCaller, v3, DamageType.Fire, 1, 100, 0x80, D20ActionType.NONE);
        v4 = new Dice(1, 2, 0);
        GameSystems.D20.Combat.DoDamage(__PAIR__(condArg3, condArg4), evt.objHndCaller, v4, DamageType.Piercing, 1, 100, 0x82, D20ActionType.NONE);
      }
    }
    else
    {
      evt.SetConditionArg1(0);
      DispIoBonusListDebug/*0x1004da70*/(&a1);
    }
  }
}


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x100f6ff0)]
public static void   MonsterStigeAcBonusCap(in DispatcherCallbackArgs evt, int data)
{
  int condArg1;
  DispIoAttackBonus dispIo;

  condArg1 = evt.GetConditionArg1();
  dispIo = evt.GetDispIoAttackBonus();
  if ( condArg1 )
  {
    dispIo.bonlist.AddCap(3, 0, 0x119);
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100f6960)]
public static void   MonsterDamageBonus(in DispatcherCallbackArgs evt)
{
  int condArg2;
  int condArg1;
  DispIoDamage dispIo;

  condArg2 = evt.GetConditionArg2();
  condArg1 = evt.GetConditionArg1();
  dispIo = evt.GetDispIoDamage();
  dispIo.damage.AddDamageDice(condArg2, (D20DT)condArg1, 0x7F);
}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100f7670)]
public static void   ImmunityToAcidElectricityFireDamageCallback(in DispatcherCallbackArgs evt)
{
  DamagePacket v1;

  v1 = &evt.GetDispIoDamage().damage;
  v1.AddModFactor(0.0, DamageType.Acid, 0x84);
  v1.AddModFactor(0.0, DamageType.Electricity, 0x84);
  v1.AddModFactor(0.0, DamageType.Fire, 0x84);
}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100f7490)]
[TemplePlusLocation("ability_fixes.cpp:77")]
public static void   MonsterOozeSplittingOnDamage(in DispatcherCallbackArgs args)
{
  DispIoDamage dispIo;
  int isSplitting;

  dispIo = args.GetDispIoDamage();
  isSplitting = 0;
  if ( args.objHndCaller.ProtoId == 14142 && GameSystems.D20.Damage.GetOverallDamage(&dispIo.damage, DamageType.Electricity) > 0 )
  {
    dispIo.damage.AddModFactor(0.0, DamageType.Electricity, 132);
    isSplitting = 1;
  }
  if ( GameSystems.D20.Damage.GetOverallDamage(&dispIo.damage, DamageType.Slashing) > 0 )
  {
    dispIo.damage.AddModFactor(0.0, DamageType.Slashing, 0x84);
    isSplitting = 1;
  }
  if ( GameSystems.D20.Damage.GetOverallDamage(&dispIo.damage, DamageType.Piercing) <= 0 )
  {
    if ( !isSplitting )
    {
      return;
    }
  }
  else
  {
    dispIo.damage.AddModFactor(0.0, DamageType.Piercing, 0x84);
  }
  args.objHndCaller.AddCondition(MonsterConditions.MonsterSplitting);
}
/* Orphan comments:
TP Replaced @ ability_fixes.cpp:77
*/


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100f7a70)]
public static void   MinotaurChargeCriticalQuery(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;

  dispIo = evt.GetDispIoD20Query();
  if ( dispIo.data1 & D20CAF.CHARGE )
  {
    dispIo.return_val = 1;
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100efe50)]
public static void   VerifyObjConditionsCallback(in DispatcherCallbackArgs evt, int data1, int data2)
{  int v2;
  int v3;
  int v4;/*INLINED:v1=evt.subDispNode.subDispDef*/  v2 = data2;
  if ( evt.GetConditionArg(data1) <= v2 )
  {
    v3 = evt.GetConditionArg(data1);
    v4 = GetDisplayNameForDebug/*0x10021200*/(evt.objHndCaller);
    Logger.Info("Invalid value for {0} on {1}.  {2} is not at least {3}", evt.subDispNode.condNode.condStruct.condName, v4, v3, v2);
  }
}


[DispTypes(DispatcherType.DealingDamage2)]
[TempleDllLocation(0x100f77b0)]
public static void   sub_100F77B0(in DispatcherCallbackArgs evt)
{
  DispIoDamage dispIo;

  dispIo = evt.GetDispIoDamage();
  if ( !GameSystems.D20.GetAttackWeapon(dispIo.attackPacket.attacker, dispIo.attackPacket.dispKey, 0) )
  {
    dispIo.attackPacket.victim.AddCondition(StatusEffects.TempAbilityLoss, 4, 1);
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100f76c0)]
public static void   sub_100F76C0(in DispatcherCallbackArgs evt)
{  int condArg1;
  DispIoDamage dispIo;
  int evta;/*INLINED:v1=evt.subDispNode*/  condArg1 = evt.GetConditionArg1();
  evta = evt.objHndCaller.GetStat(Stat.level);
  if ( !condArg1 )
  {
    evt.SetConditionArg1(1);
    dispIo = evt.GetDispIoDamage();
    if ( evt.objHndCaller.GetStat(Stat.alignment) & 4
      && dispIo.attackPacket.victim.GetStat(Stat.alignment) & 8 )
    {
      dispIo.damage.AddDamageBonus(evta, 0, 300);
    }
    else if ( evt.objHndCaller.GetStat(Stat.alignment) & 8 )
    {
      if ( dispIo.attackPacket.victim.GetStat(Stat.alignment) & 4 )
      {
        dispIo.damage.AddDamageBonus(evta, 0, 301);
      }
    }
  }
}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100f7260)]
public static void   MonsterEnergyResistanceOnDamage(in DispatcherCallbackArgs evt)
{
  DispIoDamage dispIo;
  int condArg1;
  int condArg2;

  dispIo = evt.GetDispIoDamage();
  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  dispIo.damage.AddDR(condArg2, (D20DT)condArg1, 124);
}


[DispTypes(DispatcherType.DealingDamage2)]
[TempleDllLocation(0x100f70a0)]
public static void   CarrionCrawlerParalysisOnDamage(in DispatcherCallbackArgs args)
{
  DispIoDamage dispIo;
  DispIoDamage v2;
  int v3;
  int v4;

  dispIo = args.GetDispIoDamage();
  v2 = dispIo;
  v3 = dispIo.attackPacket.dispKey;
  if ( v3 >= 10 && v3 <= 17 && !GameSystems.D20.Combat.SavingThrow(v2.attackPacket.victim, v2.attackPacket.attacker, 13, 0, 0) )
  {
    v4 = Dice.Roll(2, 6, 0);
    v2.attackPacket.victim.AddCondition(StatusEffects.Paralyzed, v4, 0);
  }
}


[DispTypes(DispatcherType.Tooltip)]
[TempleDllLocation(0x100f6e80)]
public static void   sub_100F6E80(in DispatcherCallbackArgs evt, int data)
{
  int condArg3;
  int condArg4;
  DispIoTooltip dispIo;
  int v4;
  string v5;
  string v6;
  CHAR v7;
  int v8;
  string v9;
  string v10;
  CHAR v11;
  int v12;
  string meslineValue;
int meslineKey;
  CHAR v14;

  if ( evt.GetConditionArg1() )
  {
    condArg3 = evt.GetConditionArg3();
    condArg4 = evt.GetConditionArg4();
    dispIo = evt.GetDispIoTooltip();
    meslineKey = 154;
    meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
    strncpy(&v14, meslineValue, 0x100);
    v5 = GameSystems.MapObject.GetDisplayName(__PAIR__(condArg3, condArg4), __PAIR__(condArg3, condArg4));
    v6 = v5;
    do
    {
      v7 = *v5++;
    }
    while ( v7 );
    v8 = v5 - v6;
    v9 = v6;
    v10 = (string )&meslineValue + 3;
    do
    {
      v11 = (v10++)[1];
    }
    while ( v11 );
    qmemcpy(v10, v9, v8);
    v12 = dispIo.numStrings;
    if ( v12 < 10 )
    {
      dispIo.numStrings = v12 + 1;
      strncpy(dispIo.strings[v12].text, &v14, 0x100);
    }
  }
}


[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x100f7350)]
public static void   sub_100F7350(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  int v2;

  v1 = data;
  if ( !GameSystems.Critter.IsDeadNullDestroyed(evt.objHndCaller) )
  {
    v2 = v1 * evt.GetDispIoD20Signal().data1;
    if ( evt.objHndCaller.GetInt32(obj_f.critter_subdual_damage) > 0 )
    {
      if ( GameSystems.Combat.IsCombatActive() )
      {
        GameSystems.Critter.HealSubdualSub_100B9030(evt.objHndCaller, v2);
      }
      else
      {
        GameSystems.Critter.HealSubdualSub_100B9030(evt.objHndCaller, 1);
      }
      GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x38, evt.objHndCaller, null);
      GameSystems.Critter.CritterHpChanged(evt.objHndCaller, null, 0);
    }
  }
}


[DispTypes(DispatcherType.DealingDamage2)]
[TempleDllLocation(0x100f67d0)]
public static void   BansheeCharismaDrainOnDamage(in DispatcherCallbackArgs evt)
{
  DispIoDamage dispIo;
  int tempHpGain;
  uint curHp;
  Dice v4;
  int chaDrainAmt;
  uint maxHp;
  DispIoBonusList a1;

  dispIo = evt.GetDispIoDamage();
  tempHpGain = 10;
  if ( dispIo.attackPacket.dispKey >= 10 )
  {
    if ( GameSystems.D20.Combat.SavingThrow(dispIo.attackPacket.victim, dispIo.attackPacket.attacker, bansheeCharismaDrainDC/*0x102ec7c8*/, 0, 0) )
    {
      curHp = chaDrainAmt;
      tempHpGain = chaDrainAmt;
    }
    else
    {
      a1 = new DispIoBonusList();
      curHp = dispIo.attackPacket.attacker.GetStat(Stat.hp_current, &a1);
      a1 = new DispIoBonusList();
      maxHp = dispIo.attackPacket.attacker.GetStat(Stat.hp_max, &a1);
      DispIoBonusListDebug/*0x1004da70*/(&a1);
      if ( dispIo.attackPacket.flags & D20CAF.CRITICAL )
      {
        chaDrainAmt = Dice.Roll(2, 4, 0);
      }
      else
      {
        chaDrainAmt = Dice.Roll(1, 4, 0);
        tempHpGain = 5;
      }
    }
    if ( tempHpGain >= (int)(maxHp - curHp) )
    {
      dispIo.attackPacket.attacker.AddCondition(StatusEffects.TemporaryHitPoints, 0, 14400, curHp + tempHpGain - maxHp);
      tempHpGain -= curHp + tempHpGain - maxHp;
    }
    if ( tempHpGain > 0 )
    {
      v4 = Dice.Constant(tempHpGain);
      Heal/*0x100b7df0*/(dispIo.attackPacket.attacker, dispIo.attackPacket.victim, v4, D20ActionType.NONE);
    }
    dispIo.attackPacket.victim.AddCondition(StatusEffects.TempAbilityLoss, 5, chaDrainAmt);
  }
}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100f7220)]
public static void   MonsterEnergyImmunityOnDamage(in DispatcherCallbackArgs evt)
{
  DispIoDamage dispIo;
  int condArg1;

  dispIo = evt.GetDispIoDamage();
  condArg1 = evt.GetConditionArg1();
  dispIo.damage.AddModFactor(0.0, (D20DT)condArg1, 0x84);
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100f7a40)]
public static void   MinotaurChargeCallback(in DispatcherCallbackArgs evt)
{
  int v1;
  Dice v2;

  v1 = evt.dispIO[4].ioType;
  if ( (v1 & 0x400) )
  {
    v2 = new Dice(4, 6, 1);
    sub_100E0490/*0x100e0490*/((DamagePacket )&evt.dispIO[7], v2, DamageType.Piercing, 0x75);
  }
}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100f7910)]
public static void   sub_100F7910(in DispatcherCallbackArgs evt)
{
  
  ObjectNode *v2;
  Dice v3;
  int v4;
  unsigned Dice v5;
  int v6;
  Dice v7;
  int v8;
  int v9;
  ObjListResult listResult;

  if ( evt.GetDispIoDamage().damage.finalDamage > 0 )
  {
    GameSystems.ParticleSys.CreateAtObj("mon-Hooting Cloud", evt.objHndCaller);
    ObjList.ListVicinity(evt.objHndCaller, ObjectListFilter.OLC_CRITTERS, &listResult);
    v2 = listResult.ObjectHandles.
    if ( listResult.ObjectHandles.)
    {
      do
      {
        if ( evt.objHndCaller.DistanceToObjInFeet(v2.item.handle) <= 10.0 )
        {
          v3 = Dice.D4;
          v4 = GetPackedDiceBonus/*0x10038c90*/(v3);
          v5 = Dice.D4;
          v6 = GetPackedDiceType/*0x10038c40*/(v5);
          v7 = Dice.D4;
          v8 = GetPackedDiceNumDice/*0x10038c30*/(v7);
          v9 = Dice.Roll(v8, v6, v4);
          v2.item.handle.AddCondition(StatusEffects.Blindness, v9, 0);
        }
        v2 = v2.item.next;
      }
      while ( v2 );
    }
    ObjListFree/*0x1001f2c0*/(&listResult);
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100f6f80)]
public static void   sub_100F6F80(in DispatcherCallbackArgs evt)
{
  int condArg1;
  DispIoD20Query dispIo;

  condArg1 = evt.GetConditionArg1();
  dispIo = evt.GetDispIoD20Query();
  if ( condArg1 )
  {
    dispIo.return_val = 0;
  }
}


[DispTypes(DispatcherType.TurnBasedStatusInit)]
[TempleDllLocation(0x100f6fb0)]
public static void   sub_100F6FB0(in DispatcherCallbackArgs evt)
{
  
  if ( evt.GetConditionArg1() )
  {
                    CommonConditionCallbacks.turnBasedStatusInitNoActions(in evt);
  }
}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100f73e0)]
public static void   SalamanderTakingDamageReactionDamage(in DispatcherCallbackArgs evt)
{
  DispIoDamage dispIo;
  D20ActionType v2;
  long v3;
  GameObjectBody wpn;
  Dice v5;

  dispIo = evt.GetDispIoDamage();
  if ( !(dispIo.attackPacket.flags & 4) )
  {
    v2 = dispIo.attackPacket.d20ActnType;
    if ( v2 == 2 || v2 == 3 )
    {
      v3 = GameSystems.D20.GetAttackWeapon(dispIo.attackPacket.attacker, dispIo.attackPacket.dispKey, 0);
      wpn = v3;
      if ( !v3 || v3.type == 4 && wpn.GetInt32(obj_f.material) == 5 )
      {
        v5 = Dice.D6;
        GameSystems.D20.Combat.DoDamage(dispIo.attackPacket.attacker, evt.objHndCaller, v5, DamageType.Fire, 1, 100, 0x80, D20ActionType.NONE);
      }
    }
  }
}

}
}
/*

ConvertDamageDice @ 0x100e0560 = 1
DamageZeroBonusLineAdd @ 0x100e11f0 = 1
ObjListFree @ 0x1001f2c0 = 1
AddAttackPowerType @ 0x100e0520 = 1
sub_100E0490 @ 0x100e0490 = 1
bansheeCharismaDrainDC @ 0x102ec7c8 = 1
SetDamageType @ 0x100e0540 = 1
Float_Combat_mes_with_extra_Strings @ 0x100b4bf0 = 1
GetProtoHandle @ 0x1003ad70 = 1
Int64Shr @ 0x10255e30 = 1
AddIncorporealImmunity @ 0x100e0780 = 1
Heal @ 0x100b7df0 = 2
GetPackedDiceBonus @ 0x10038c90 = 2
GetPackedDiceNumDice @ 0x10038c30 = 2
GetDisplayNameForDebug @ 0x10021200 = 2
FindInParty @ 0x1002b1e0 = 2
GetPackedDiceType @ 0x10038c40 = 2
Obj_AI_Flee_Add @ 0x1005de60 = 4
DispIoBonusListDebug @ 0x1004da70 = 6
*/