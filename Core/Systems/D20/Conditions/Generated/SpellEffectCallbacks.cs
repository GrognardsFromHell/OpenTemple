using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Utils;
using SpicyTemple.Core.Systems.RadialMenus;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;

namespace SpicyTemple.Core.Systems.D20.Conditions
{
    public static partial class SpellEffects
    {

[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ca8a0)]
public static void   sub_100CA8A0(in DispatcherCallbackArgs evt)
{
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20030, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100dc920)]
public static void   DispCritterKilledRemoveSpellAndMod(in DispatcherCallbackArgs evt)
{
  DispIoD20Signal dispIo;

  dispIo = evt.GetDispIoD20Signal();
  if ( *(_QWORD *)&dispIo.data1 == GameSystems.D20.D20QueryReturnObject(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Charmed, 0, 0)
    || *(_QWORD *)&dispIo.data1 == GameSystems.D20.D20QueryReturnObject(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Held, 0, 0) )
  {
                SpellEffects.Spell_remove_spell(in evt);
                SpellEffects.Spell_remove_mod(evt.WithoutIO);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ce010)]
public static void   SpHealOnConditionAdd(in DispatcherCallbackArgs evt)
{
  Stat v1;  int v3;
  int v4;
  string v5;
  string v6;
  int condArg1;
  int v8;
  Dice v9;
  DispIoAbilityLoss dispIo;
  CHAR extraText2;
  SpellPacketBody spellPkt;

  v1 = (Stat)0;
  do
  {
    dispIo = new DispIoAbilityLoss();
    dispIo.flags |= 0x1B;/*INLINED:v2=evt.subDispNode.condNode*/    dispIo.statDamaged = (Stat)v1;
    dispIo.field_C = 1;
    dispIo.spellId = evt.GetConditionArg1();
    dispIo.result = 0;
    v3 = evt.objHndCaller.DispatchGetAbilityLoss(&dispIo);
    v4 = -v3;
    if ( -v3 >= 0 && v3 != 0 )
    {
      v5 = GameSystems.Stat.GetStatName(dispIo.statDamaged);
      Logger.Info("d20_mods_spells.c / _begin_spell_restoration(): healed {0} points of temporary ({1}) damage", v4, v5);
      v6 = GameSystems.Stat.GetStatName(dispIo.statDamaged);
      extraText2 = String.Format(": {0} [{1}]", v6, v4);
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20035, TextFloaterColor.White);
    }
    ++v1;
  }
  while ( (int)v1 < 5 );
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    v8 = spellPkt.spellId;
    v9 = 0.new Dice(0, 10 * spellPkt.casterLevel);
    GameSystems.Combat.SpellHeal(evt.objHndCaller, spellPkt.caster, v9, D20ActionType.CAST_SPELL, v8);
    GameSystems.Critter.HealSubdualSub_100B9030(evt.objHndCaller, 10 * spellPkt.casterLevel);
  }
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100c71c0)]
public static void   sub_100C71C0(in DispatcherCallbackArgs evt, int data)
{
  int condArg1;

  condArg1 = evt.GetConditionArg1();
  GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, condArg1, 0);
}


[DispTypes(DispatcherType.TurnBasedStatusInit)]
[TempleDllLocation(0x100c6690)]
public static void   ChaosHammerTurnBasedStatusInit(in DispatcherCallbackArgs evt)
{
  DispIOTurnBasedStatus dispIo;
  int *v2;

  dispIo = evt.GetDispIOTurnBasedStatus();
  if ( dispIo !=null)
  {
    v2 = &dispIo.tbStatus.hourglassState;
    if ( v2 )
    {
      if ( *v2 < 2 )
      {
        *v2 = 0;
        dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
      }
      else
      {
        *v2 = 2;
        dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
      }
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cc820)]
public static void   BeginSpellColorSprayBlind(in DispatcherCallbackArgs evt)
{
  int condArg1;
  Dice v2;
  int v3;
  unsigned Dice v4;
  int v5;
  Dice v6;
  int v7;
  int v8;  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20019, TextFloaterColor.Red);
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    v2 = 1.new Dice(4, 0);
    v3 = GetPackedDiceBonus/*0x10038c90*/(v2);
    v4 = 1.new Dice(4, 0);
    v5 = GetPackedDiceType/*0x10038c40*/(v4);
    v6 = 1.new Dice(4, 0);
    v7 = GetPackedDiceNumDice/*0x10038c30*/(v6);
    v8 = DiceRoller/*0x10038b60*/(v7, v5, v3);/*INLINED:v9=evt.subDispNode.condNode*/    spellPkt.duration = v8;
    spellPkt.durationRemaining = v8;
    evt.SetConditionArg2(v8);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_color_spray_blind(): unable to save new spell_packet");
    }
*/  }
  else
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_color_spray_blind(): unable to get spell_packet");
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c9c60)]
public static void   SanctuaryCanBeAffectedPerform(in DispatcherCallbackArgs evt, ConditionSpec data)
{
  DispIoD20Query dispIo;
  D20Action v2;
  int v3;
  int v4;
  int v5;
  GameObjectBody v6;
  long v7;
  DispIoD20Query v8;

  dispIo = evt.GetDispIoD20Query();
  v2 = (D20Action )dispIo.data1;
  v8 = dispIo;
  if ( !v2.d20APerformer.HasCondition(SpellEffects.SpellSanctuarySaveFailed)     && !v2.d20APerformer.HasCondition(SpellEffects.SpellSanctuarySaveSucceeded) )
  {
    v7 = (int)v2;
    v6 = v2.d20ATarget;
LABEL_12:
    GameSystems.D20.D20SendSignal(v6, D20DispatcherKey.SIG_Spell_Sanctuary_Attempt_Save, v7, SHIDWORD(v7));
    goto LABEL_13;
  }
  if ( !v2.d20APerformer.HasCondition(SpellEffects.SpellSanctuarySaveFailed)     && v2.d20APerformer.HasCondition(SpellEffects.SpellSanctuarySaveSucceeded) )
  {
    v3 = GameSystems.D20.D20QueryReturnObject(v2.d20ATarget, D20DispatcherKey.QUE_Critter_Has_Condition, (int)SpellEffects.SpellSanctuary, 0);
    if ( (int)GameSystems.D20.D20QueryReturnObject(v2.d20APerformer, D20DispatcherKey.QUE_Critter_Has_Condition, (int)SpellEffects.SpellSanctuarySaveSucceeded, 0) == v3 )
    {
      goto LABEL_13;
    }
    v7 = (int)v2;
    v6 = v2.d20ATarget;
    goto LABEL_12;
  }
  if ( !v2.d20APerformer.HasCondition(SpellEffects.SpellSanctuarySaveSucceeded)     && v2.d20APerformer.HasCondition(SpellEffects.SpellSanctuarySaveFailed) )
  {
    v4 = GameSystems.D20.D20QueryReturnObject(v2.d20ATarget, D20DispatcherKey.QUE_Critter_Has_Condition, (int)SpellEffects.SpellSanctuary, 0);
    if ( (int)GameSystems.D20.D20QueryReturnObject(v2.d20APerformer, D20DispatcherKey.QUE_Critter_Has_Condition, (int)SpellEffects.SpellSanctuarySaveFailed, 0) != v4 )
    {
      v7 = (int)v2;
      v6 = v2.d20ATarget;
      goto LABEL_12;
    }
  }
LABEL_13:
  if ( v2.d20APerformer.HasCondition(SpellEffects.SpellSanctuarySaveFailed) )
  {
    v5 = GameSystems.D20.D20QueryReturnObject(v2.d20ATarget, D20DispatcherKey.QUE_Critter_Has_Condition, (int)SpellEffects.SpellSanctuary, 0);
    if ( (int)GameSystems.D20.D20QueryReturnObject(v2.d20APerformer, D20DispatcherKey.QUE_Critter_Has_Condition, (int)SpellEffects.SpellSanctuarySaveFailed, 0) == v5 )
    {
      v8.return_val = 0;
      *(_QWORD *)&v8.data1 = evt.GetConditionArg3();
      GameSystems.D20.Combat.FloatCombatLine(v2.d20ATarget, 123);
    }
  }
}


[DispTypes(DispatcherType.TurnBasedStatusInit)]
[TempleDllLocation(0x100dc9b0)]
public static void   HoldXTurnBasedStatusInit(in DispatcherCallbackArgs evt)
{
  DispIOTurnBasedStatus dispIo;
  int *v2;

  dispIo = evt.GetDispIOTurnBasedStatus();
  if ( evt.GetConditionArg3() == 1 )
  {
    if ( dispIo !=null)
    {
      v2 = &dispIo.tbStatus.hourglassState;
      if ( v2 )
      {
        *v2 = 0;
        dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, spell_mes_A_Spell_has_expired, TextFloaterColor.White);
                                SpellEffects.Spell_remove_spell(evt.WithoutIO);
                                SpellEffects.Spell_remove_mod(evt.WithoutIO);
      }
    }
  }
  else if ( !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement) )
  {
    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x22, evt.objHndCaller, null);
    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 40);
  }
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x100c7ad0)]
public static void   SkillModifier_FindTraps_Callback(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoObjBonus dispIo;
  DispIoObjBonus v2;
  int condArg1;
  int casterLvl;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoObjBonus();
  v2 = dispIo;
  if ( evt.dispKey == D20DispatcherKey.D20A_CLEAVE )
  {
    if ( dispIo.flags & 8 )
    {
      condArg1 = evt.GetConditionArg1();
      if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
      {
        casterLvl = DispatcherExtensions.Dispatch35CasterLevelModify(evt.objHndCaller, &spellPkt) / 2;
        if ( casterLvl > 10 )
        {
          casterLvl = 10;
        }
        v2.bonOut.AddBonus(casterLvl, 0, data2);
      }
    }
  }
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100c7fe0)]
public static void   Condition__36_ghoul_touch_stench_sthg(in DispatcherCallbackArgs evt, int data)
{
  DispIoObjEvent dispIo;
  int condArg1;
  int v3;
  int v4;
  int v5;
  GameObjectBody v6;

  int v8;


  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoObjEvent();
  if ( dispIo.evtId == evt.GetConditionArg3() )
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      SpellPktTriggerAoeHitScript/*0x100c37d0*/(spellPkt.spellId);
      if ( D20ModSpells.CheckSpellResistance(&spellPkt, dispIo.tgt) )
      {
        return;
      }
      if ( evt.dispKey == D20DispatcherKey.OnEnterAoE
        && dispIo.tgt != spellPkt.caster
        && !evt.objHndCaller.HasCondition(SpellEffects.SpellDelayPoison) )
      {
        if ( GameSystems.D20.D20Query(dispIo.tgt, D20DispatcherKey.QUE_Critter_Is_Immune_Poison) )
        {
          GameSystems.Spell.PlayFizzle(evt.objHndCaller);
          GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 32000, TextFloaterColor.White);
        }
        else
        {
          v3 = GameSystems.D20.Combat.SavingThrowSpell(dispIo.tgt, spellPkt.caster, spellPkt.dc, 0, 0, spellPkt.spellId);
          v4 = dispIo.tgt;
          v5 = HIDWORD(dispIo.tgt);
          if ( (v3 )!=0)
          {
            GameSystems.Spell.FloatSpellLine(__PAIR__(v5, v4), 30001, TextFloaterColor.White);
          }
          else
          {
            GameSystems.Spell.FloatSpellLine(__PAIR__(v5, v4), 30002, TextFloaterColor.White);
            v6 = dispIo.tgt;
            v8 = GameSystems.ParticleSys.CreateAtObj("Fizzle", v6);
            spellPkt.AddTarget(dispIo.tgt, v8, false);
            dispIo.tgt.AddCondition("sp-Ghoul Touch Stench Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
          }
        }
      }
    }
  }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*  else
  {
    Logger.Info("d20_mods_spells.c / _ghoul_touch_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100c9020)]
public static void   sub_100C9020(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoSavingThrow dispIo;

  dispIo = evt.GetDispIoSavingThrow();
  if ( evt.dispKey == D20DispatcherKey.SAVE_WILL )
  {
    dispIo.bonlist.AddBonus(-data1, 34, data2);
  }
}


[DispTypes(DispatcherType.ConditionRemove)]
[TempleDllLocation(0x100d2c80)]
public static void   sub_100D2C80(in DispatcherCallbackArgs evt)
{
  int condArg1;

  condArg1 = evt.GetConditionArg1();
  GameSystems.Spell.EndSpell(condArg1, 1);
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100de560)]
public static void   ExpireSpell(in DispatcherCallbackArgs evt, int data)
{
  int condArg1;
          int i;
  SpellPacketBody spellPkt;

  evt.GetDispIoD20Signal();
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    if ( data == 1 )
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                        SpellEffects.Spell_remove_spell(evt.WithoutIO);
                        SpellEffects.Spell_remove_mod(evt.WithoutIO);
    }
    else
    {
      for ( i = 0; i < spellPkt.targetCount; ++i )
      {
        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                                SpellEffects.Spell_remove_spell(evt.WithoutIO);
                                SpellEffects.Spell_remove_mod(evt.WithoutIO);
      }
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cde90)]
public static void   sub_100CDE90(in DispatcherCallbackArgs evt)
{
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20016, TextFloaterColor.White);
}


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x100c6080)]
public static void   sub_100C6080(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;

  dispIo = evt.GetDispIoAttackBonus();
  dispIo.bonlist.AddBonus(data1, 9, data2);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cc2c0)]
public static void   BlessOnAdd(in DispatcherCallbackArgs args)
{
  GameSystems.Spell.FloatSpellLine(args.objHndCaller, 20008, TextFloaterColor.White);
}


[DispTypes(DispatcherType.AbilityScoreLevel)]
[TempleDllLocation(0x100c5c30)]
public static void   StatLevel_callback_SpellModifier(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoBonusList dispIo;
  int condArg3;
  int v3;
  BonusList *v4;
  int v5;

  dispIo = evt.GetDispIoBonusList();
  condArg3 = evt.GetConditionArg3();
  if ( evt.dispKey - 1 != data1 )
  {
    return;
  }
  v3 = data2;
  if ( v3 > 0x124 )
  {
    if ( v3 < 0x225 || v3 > 0x226 )
    {
      goto LABEL_11;
    }
LABEL_10:
    if ( dispIo.flags & 2 )
    {
      return;
    }
    goto LABEL_11;
  }
  if ( v3 == 292 )
  {
    goto LABEL_10;
  }
  if ( v3 != 198 )
  {
LABEL_11:
    dispIo.bonlist.AddBonus(condArg3, 12, v3);
    return;
  }
  v4 = &dispIo.bonlist;
  v5 = dispIo.bonlist.bonusEntries[0].bonValue;
  if ( v5 <= condArg3 )
  {
    condArg3 = v5 - 1;
  }
  v4.AddBonus(-condArg3, 0, 198);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cd920)]
public static void   BeginSpellHold(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg2;
  int condArg3;


  int v6;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  condArg3 = evt.GetConditionArg3();
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20001, TextFloaterColor.Red);
  if ( !evt.objHndCaller.AddCondition("Held", condArg1, condArg2, condArg3) )
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_hold(): unable to add condition");
  }
  v6 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(v6, out spellPkt) && evt.objHndCaller == spellPkt.caster )
  {
    GameSystems.D20.Actions.curSeqGetTurnBasedStatus().hourglassState = 0;
  }
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100c7a90)]
public static void   SavingThrowPenalty_sp_Feeblemind_Callback(in DispatcherCallbackArgs evt)
{
  DispIoSavingThrow dispIo;

  dispIo = evt.GetDispIoSavingThrow();
  if ( KnowsArcaneSpells/*0x100760e0*/(evt.objHndCaller) )
  {
    dispIo.bonlist.AddBonus(-4, 0, 143);
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c70c0)]
public static void   ControlPlantsEntangleSpellInterruptedCheck(in DispatcherCallbackArgs args)
{
  DispIoD20Query dispIo;

  dispIo = args.GetDispIoD20Query();
  if ( dispIo.return_val != 1
    && !args.objHndCaller.HasCondition(SpellEffects.SpellControlPlantsDisentangle)
    && !GameSystems.Skill.SkillRoll(args.objHndCaller, SkillId.concentration, 15, 0, 1) )
  {
    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x25, args.objHndCaller, null);
    GameSystems.D20.Combat.FloatCombatLine(args.objHndCaller, 54);
    dispIo.return_val = 1;
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cc5b0)]
public static void   BeginSpellCharmPerson(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg2;
  int condArg3;



  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  condArg3 = evt.GetConditionArg3();
  if ( !evt.objHndCaller.AddCondition("Charmed", condArg1, condArg2, condArg3) )
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_charm_person(): unable to add condition");
  }
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20018, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100dba80)]
public static void   DispelMagicOnAdd(in DispatcherCallbackArgs evt, int data)
{
  int condArg1;      DispIoDispelCheck dispIo;

  dispIo = new DispIoDispelCheck();
  condArg1 = evt.GetConditionArg1();/*INLINED:v2=evt.subDispNode.condNode*/  dispIo.spellId = condArg1;
  dispIo.returnVal = 0;
  dispIo.flags = 1;
  if ( (evt.GetConditionArg3() )!=0)
  {
    dispIo.returnVal = 1;
    dispIo.flags |= 0x80;
  }
  evt.objHndCaller.DispatchDispelCheck(&dispIo);
  nullsub_1/*0x100027f0*/();
        SpellEffects.Spell_remove_spell(in evt);
        SpellEffects.Spell_remove_mod(in evt);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cce10)]
public static void   sub_100CCE10(in DispatcherCallbackArgs evt)
{
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20027, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100c51d0)]
public static void   SavingThrowModifierCallback(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoSavingThrow dispIo;
  dispIo = evt.GetDispIoSavingThrow();
  if ( (dispIo.flags & D20SavingThrowFlag.SPELL_DESCRIPTOR_FEAR)!=0)
  {/*INLINED:v2=evt.subDispNode.subDispDef*/    switch ( data2 )
    {
      case 0x98:
      case 0xA9:
        dispIo.bonlist.AddBonus(-data1, 13, data2);
        break;
      case 0xAC:
        if ( !evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions)
          && !evt.objHndCaller.HasCondition(SpellEffects.SpellRemoveFear) )
        {
          dispIo.bonlist.AddBonus(-data1, 13, data2);
        }
        break;
      case 0x8E:
      case 0xFC:
        if ( !evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions) )
        {
          dispIo.bonlist.AddBonus(data1, 13, data2);
        }
        break;
      default:
        dispIo.bonlist.AddBonus(data1, 13, data2);
        break;
    }
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100c92d0)]
public static void   sub_100C92D0(in DispatcherCallbackArgs evt, int data)
{
  int condArg3;
  int v2;
  int v3;
  DamagePacket v4;

  condArg3 = evt.GetConditionArg3();
  v2 = data;
  v3 = condArg3;
  v4 = &evt.GetDispIoDamage().damage;
  if ( v3 != -3 )
  {
    v2 = -v2;
  }
  v4.AddDamageBonus(v2, 14, 151);
}


[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x100c6900)]
public static void   CloudkillBeginRound(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int v2;
  Dice v3;
  int v4;
  unsigned Dice v5;
  int v6;
  Dice v7;
  int v8;
  int v9;


  string v12;
  string v13;


  string v16;
  string v17;
  CHAR v18;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt)     && !evt.objHndCaller.HasCondition(SpellEffects.SpellDelayPoison) )
  {
    if ( GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Immune_Poison) )
    {
      GameSystems.Spell.PlayFizzle(evt.objHndCaller);
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 32000, TextFloaterColor.White);
    }
    else if ( !D20ModSpells.CheckSpellResistance(&spellPkt, evt.objHndCaller) )
    {
      v2 = GameSystems.Critter.GetHitDiceNum(evt.objHndCaller);
      v3 = 1.new Dice(4, 0);
      v4 = GetPackedDiceBonus/*0x10038c90*/(v3);
      v5 = 1.new Dice(4, 0);
      v6 = GetPackedDiceType/*0x10038c40*/(v5);
      v7 = 1.new Dice(4, 0);
      v8 = GetPackedDiceNumDice/*0x10038c30*/(v7);
      v9 = DiceRoller/*0x10038b60*/(v8, v6, v4);
      if ( v2 <= 6 )
      {
        if ( v2 <= 3 )
        {
          GameSystems.D20.Combat.Kill(evt.objHndCaller, spellPkt.caster);
        }
        else if ( GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc, 0, 0, spellPkt.spellId) )
        {
          GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30001, TextFloaterColor.White);
          evt.objHndCaller.AddCondition("Temp_Ability_Loss", 2, v9);
          v16 = GameSystems.Stat.GetStatName(Stat.constitution);
          v17 = GameSystems.Spell.GetSpellName(25013);
          v18 = String.Format("{0}: {1} [{2}]", v17, v16, v9);
          GameSystems.RollHistory.CreateFromFreeText(&v18);
        }
        else
        {
          GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30002, TextFloaterColor.White);
          GameSystems.D20.Combat.Kill(evt.objHndCaller, spellPkt.caster);
        }
      }
      else
      {
        evt.objHndCaller.AddCondition("Temp_Ability_Loss", 2, v9);
        v12 = GameSystems.Stat.GetStatName(Stat.constitution);
        v13 = GameSystems.Spell.GetSpellName(25013);
        v18 = String.Format("{0}: {1} [{2}]", v13, v12, v9);
        GameSystems.RollHistory.CreateFromFreeText(&v18);
      }
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cfe70)]
public static void   BeginSpellSpikeGrowth(in DispatcherCallbackArgs evt)
{
  int condArg1;
  float radiusInches;
  int v3;  SpellEntry a2;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
    radiusInches = (float)(int)a2.radiusTarget * 12F;
    spellPkt.aoeObj = evt.objHndCaller;
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 40, 41, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_spike_growth(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cd7d0)]
public static void   BeginSpellFogCloud(in DispatcherCallbackArgs evt)
{
  int condArg1;
  float radiusInches;
  int v3;  SpellEntry a2;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
    radiusInches = (float)(int)a2.radiusTarget * 12F;
    spellPkt.aoeObj = evt.objHndCaller;
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 10, 11, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_fog_cloud(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100dbc60)]
public static void   MagicVestmentOnAdd(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg3;

  GameObjectBody parent;

  parent = null;
  if ( !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Item_Has_Enhancement_Bonus) )
  {
    condArg1 = evt.GetConditionArg1();
    condArg3 = evt.GetConditionArg3();
    AddItemConditionToWielder/*0x100d2f30*/(evt.objHndCaller, "Armor Enhancement Bonus", condArg3, 0, 0, 0, condArg1);
    GameSystems.Item.GetParent(evt.objHndCaller, &parent);
    GameSystems.D20.Status.initItemConditions(parent);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ced20)]
public static void   BeginSpellMordenkainensFaithfulHound(in DispatcherCallbackArgs evt)
{  int condArg1;


  int v5;/*INLINED:v1=evt.subDispNode*/  condArg1 = evt.GetConditionArg1();
  evt.subDispNode = (SubDispNode *)evt.GetConditionArg2();
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20017, TextFloaterColor.Red);
  if ( !evt.objHndCaller.AddCondition("Invisible", condArg1, (int)evt.subDispNode, 128) )
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_mordenkainens_faithful_hound(): unable to add condition");
  }
  v5 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 26, 27, ObjectListFilter.OLC_CRITTERS, 360F, 0F, 6.28318548F);
  evt.SetConditionArg4(v5);
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100dc680)]
public static void   HoldTouchSpellTouchAttackHandler(in DispatcherCallbackArgs evt)
{
  D20Action d20a;
  int condArg1;
      int condArg3;
      SpellPacketBody spellPktBody;

  d20a = (D20Action )evt.GetDispIoD20Signal().data1;
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPktBody) )
  {
    GameSystems.Script.Spells.SpellSoundPlay(&spellPktBody, OnAreaOfEffectHit);
    if ( (d20a.d20Caf & D20CAF.HIT )!=0)
    {
      GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 68);
      if ( D20ModSpells.CheckSpellResistance(&spellPktBody, d20a.d20ATarget) )
      {
        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                                SpellEffects.Spell_remove_spell(evt.WithoutIO);
                                SpellEffects.Spell_remove_mod(evt.WithoutIO);
      }
      else
      {
        condArg3 = evt.GetConditionArg3();
        evt.SetConditionArg3(condArg3 - 1);
        if ( evt.GetConditionArg3() <= 0 )
        {
          GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                                        SpellEffects.Spell_remove_spell(evt.WithoutIO);
                                        SpellEffects.Spell_remove_mod(evt.WithoutIO);
        }
      }
    }
    else
    {
      GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 69);
    }
  }
}


[DispTypes(DispatcherType.GetAttackDice)]
[TempleDllLocation(0x100c5ee0)]
public static void   AttackDiceAnimalGrowth(in DispatcherCallbackArgs evt, int data1, int data2)
{
  Dice v1;
  DispIoAttackDice dispIo;
  int v3;
  int v4;
  int v5;
  int v6;
  int v7;
  int v8;

  v1 = 0;
  dispIo = evt.GetDispIoAttackDice();
  if ( (dispIo.weapon == null))
  {
    switch ( GetPackedDiceType/*0x10038c40*/(dispIo.dicePacked) )
    {
      case 2:
        v8 = GetPackedDiceBonus/*0x10038c90*/(dispIo.dicePacked);
        v7 = 3;
        goto LABEL_7;
      case 3:
        v8 = GetPackedDiceBonus/*0x10038c90*/(dispIo.dicePacked);
        v7 = 4;
        v3 = GetPackedDiceNumDice/*0x10038c30*/(dispIo.dicePacked);
        goto LABEL_8;
      case 4:
        v8 = GetPackedDiceBonus/*0x10038c90*/(dispIo.dicePacked);
        v7 = 6;
        v3 = GetPackedDiceNumDice/*0x10038c30*/(dispIo.dicePacked);
        goto LABEL_8;
      case 6:
        v8 = GetPackedDiceBonus/*0x10038c90*/(dispIo.dicePacked);
        v7 = 8;
LABEL_7:
        v3 = GetPackedDiceNumDice/*0x10038c30*/(dispIo.dicePacked);
LABEL_8:
        dispIo.dicePacked = v3.new Dice(v7, v8);
        return;
      case 8:
        v4 = GetPackedDiceBonus/*0x10038c90*/(dispIo.dicePacked);
        dispIo.dicePacked = 2.new Dice(6, v4);
        return;
      case 0xA:
        v5 = GetPackedDiceBonus/*0x10038c90*/(dispIo.dicePacked);
        dispIo.dicePacked = 2.new Dice(6, v5);
        return;
      case 0xC:
        v6 = GetPackedDiceBonus/*0x10038c90*/(dispIo.dicePacked);
        v1 = 2.new Dice(8, v6);
        break;
      default:
        break;
    }
    dispIo.dicePacked = v1;
  }
}


[DispTypes(DispatcherType.GetMoveSpeedBase)]
[TempleDllLocation(0x100c86f0)]
public static void   HasteMoveSpeed(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoMoveSpeed dispIo;
  int v2;

  evt.objHndCaller.GetBaseStat(Stat.movement_speed);
  dispIo = evt.GetDispIoMoveSpeed();
  dispIo.bonlist.AddBonus(v2, 12, 174);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cd0a0)]
public static void   TouchAttackOnAdd(in DispatcherCallbackArgs evt)
{
  TurnBasedStatus *v1;
  int condArg1;

  v1 = GameSystems.D20.Actions.curSeqGetTurnBasedStatus();
  v1.tbsFlags |= D20CAF.ACTIONFRAME_PROCESSED;
  condArg1 = evt.GetConditionArg1();
  GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_TouchAttackAdded, condArg1, 0);
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100ca4f0)]
public static void   SilenceSpellFailure(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;
  int v2;
  int v3;
  int mmData;
  int a5;
  int spellEnum;
  int spellClassCode;
  SpellStoreData spellData;

  spellEnum = 0;
  a5 = 0;
  mmData = 0;
  dispIo = evt.GetDispIoD20Query();
  if ( dispIo.return_val != 1 )
  {
    GameSystems.D20.RadialMenu.SelectedRadialMenuEntry.d20SpellData.SpellEnum((D20SpellData *)dispIo.data1, &spellEnum, 0, &spellClassCode, &a5, 0, &mmData);
    EncodeSpellData/*0x10075280*/(spellEnum, a5, spellClassCode, 0, mmData, &spellData);
    if ( GameSystems.Spell.GetSpellComponentRegardMetamagic(&spellData) & 1 )
    {
      GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 115);
      v2 = GameSystems.RollHistory.AddPercentageCheck(evt.objHndCaller, null, 100, 114, 0, 115, 192);
      GameSystems.RollHistory.CreateRollHistoryString(v2);
      dispIo.return_val = 1;
    }
    else
    {
      v3 = GameSystems.RollHistory.AddPercentageCheck(evt.objHndCaller, null, 0, 114, 0, 62, 192);
      dispIo.return_val = 0;
      GameSystems.RollHistory.CreateRollHistoryString(v3);
    }
  }
}


[DispTypes(DispatcherType.TurnBasedStatusInit, DispatcherType.D20Signal)]
[TempleDllLocation(0x100d69f0)]
public static void   TurnBasedStatus_web_Callback(in DispatcherCallbackArgs evt, int data1, ConditionSpec data2)
{
  int v1;
  int v2;
  int condArg1;
  string v4;
  int v5;
  string v6;


  int condArg3;
  string v11;

  int v14;


  int v17;
  int arg4;
  SpellPacketBody spellPkt;
  BonusList bonList;
  DispIoBonusList a1;
  BonusList bonlist;

  if ( evt.dispType != 28 )
  {
    if ( evt.dispType != 7 )
    {
      return;
    }
LABEL_4:
    bonlist = BonusList.Create();
    bonList = BonusList.Create();
    a1 = new DispIoBonusList();
    v1 = evt.objHndCaller.GetStat(0, &a1);
    v2 = D20StatSystem.GetModifierForAbilityScore(v1);
    if ( evt.objHndCaller.HasCondition(data2) )
    {
      evt.SetConditionArg4(0);
    }
    condArg1 = evt.GetConditionArg1();
    if ( !GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      goto LABEL_19;
    }
    bonList.AddBonus(v2, 0, 103);
    v4 = GameSystems.Stat.GetStatName(0);
    v5 = GameSystems.Spell.DispelRoll(evt.objHndCaller, &bonList, 0, 15, v4, 0);
    arg4 = v5;
    if ( v5 >= 0 )
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 21003, TextFloaterColor.White);
      if ( evt.objHndCaller.HasCondition(data2) )
      {
        evt.SetConditionArg4(v5 + 15);
        goto LABEL_19;
      }
      v6 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
      GameSystems.ParticleSys.End(v6);
      if ( spellPkt.RemoveTarget(evt.objHndCaller) )
      {
                                SpellEffects.Spell_remove_mod(evt.WithoutIO);
        GameSystems.Spell.PlayFizzle(evt.objHndCaller);
        spellPkt.AddTarget(evt.objHndCaller, 0, true);
        condArg3 = evt.GetConditionArg3();
        evt.objHndCaller.AddCondition("sp-Web Off", spellPkt.spellId, spellPkt.durationRemaining, condArg3, arg4);
        goto LABEL_19;
      }
LABEL_16:
      Logger.Info("d20_mods_spells.c / _web_break_free_check(): cannot remove target");
      return;
    }
    if ( evt.objHndCaller.HasCondition(data2) )
    {
      v11 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
      GameSystems.ParticleSys.End(v11);
      if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
      {
        goto LABEL_16;
      }
                        SpellEffects.Spell_remove_mod(evt.WithoutIO);
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30002, TextFloaterColor.White);
      v14 = GameSystems.ParticleSys.CreateAtObj("sp-Web Hit", evt.objHndCaller);
      spellPkt.AddTarget(evt.objHndCaller, v14, true);
      v17 = evt.GetConditionArg3();
      evt.objHndCaller.AddCondition("sp-Web On", spellPkt.spellId, spellPkt.durationRemaining, v17);
    }
    else
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20028, TextFloaterColor.Red);
    }
LABEL_19:
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _web_break_free_check(): unable to save new spell_packet");
    }
*/    return;
  }
  if ( evt.GetDispIoD20Signal() !=null&& evt.dispKey == D20DispatcherKey.SIG_BreakFree )
  {
    goto LABEL_4;
  }
}


[DispTypes(DispatcherType.AbilityScoreLevel)]
[TempleDllLocation(0x100c9950)]
public static void   sub_100C9950(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoBonusList dispIo;
  int condArg1;  int v4;
  int v5;
  int v6;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoBonusList();
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {/*INLINED:v3=evt.subDispNode.subDispDef*/    v4 = data1;
    if ( evt.dispKey - 1 == v4 )
    {
      if ( v4 == 1 )
      {
        v6 = data2;
        v5 = 2;
      }
      else
      {
        if ( (v4 )!=0)
        {
          return;
        }
        v6 = data2;
        v5 = -2;
      }
      dispIo.bonlist.AddBonus(v5, 20, v6);
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cd5b0)]
public static void   BeginSpellEntangle(in DispatcherCallbackArgs evt)
{
  int condArg1;
  float radiusInches;
  int v3;  SpellEntry a2;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
    radiusInches = (float)(int)a2.radiusTarget * 12F;
    spellPkt.aoeObj = evt.objHndCaller;
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 8, 9, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_entangle(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100dca80)]
public static void   sub_100DCA80(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  int condArg1;
      SpellPacketBody spellPkt;

  v1 = evt.GetConditionArg3() - 1;
  evt.SetConditionArg3(v1);
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    spellPkt.durationRemaining -= 10;
    if ( spellPkt.durationRemaining < 0 )
    {
      spellPkt.durationRemaining = 0;
    }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    Logger.Info("d20_mods_spells.c / _cloudkill_hit_trigger(): unable to save new spell_packet");
    return;
}    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
  }
  if ( (v1 )==0)
  {
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(evt.WithoutIO);
                SpellEffects.Spell_remove_mod(evt.WithoutIO);
  }
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d62e0)]
[TemplePlusLocation("spell_condition.cpp:350")]
public static void   SpikeGrowthHitTrigger(in DispatcherCallbackArgs evt, int data)
{
  DispIoObjEvent dispIo;
  int condArg1;
  ulong v3;
  string v4;
    GameObjectBody v6;

  int v8;


  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoObjEvent();
  if ( dispIo.evtId == evt.GetConditionArg3() )
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      SpellPktTriggerAoeHitScript/*0x100c37d0*/(spellPkt.spellId);
      if ( D20ModSpells.CheckSpellResistance(&spellPkt, dispIo.tgt) )
      {
        return;
      }
      if ( evt.dispKey == D20DispatcherKey.OnEnterAoE )
      {
        v6 = dispIo.tgt;
        v8 = GameSystems.ParticleSys.CreateAtObj("sp-Spike Growth-HIT", v6);
        spellPkt.AddTarget(dispIo.tgt, v8, true);
        dispIo.tgt.AddCondition("sp-Spike Growth Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        if ( GameSystems.D20.Actions.IsCurrentlyPerforming(dispIo.tgt) )
        {
          v3 = (ulong)ActSeqCurGetCurrentAction/*0x1008a090*/().distTraversed;
          GameSystems.D20.D20SendSignal(dispIo.tgt, D20DispatcherKey.SIG_Combat_Critter_Moved, v3, (ulong)(int)v3 >> 32);
        }
        v4 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v4);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _spike_growth_hit_trigger(): cannot remove target");
          return;
        }
        *(_DWORD *)&v5[20] = evt.dispIO;
        *(_DWORD *)&v5[16] = 19;
        *(_QWORD *)&v5[8] = *(GameObjectBody *)((string )&evt.objHndCaller + 4);
        *(_QWORD *)v5 = *(_QWORD *)&evt.subDispNode;
        SpellEffects.Spell_remove_mod(in evt);
      }
    }
  }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*  else
  {
    Logger.Info("d20_mods_spells.c / _spike_growth_hit_trigger(): unable to save new spell_packet");
  }
*/}
/* Orphan comments:
TP Replaced @ spell_condition.cpp:350
*/


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x100c4190)]
public static void   ChaosHammerAcBonus(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;

  dispIo = evt.GetDispIoAttackBonus();
  if ( data2 == 282 )
  {
    dispIo.bonlist.AddBonus(data1, 11, 282);
  }
  dispIo.bonlist.AddBonus(data1, 11, data2);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ce170)]
public static void   BeginSpellIceStorm(in DispatcherCallbackArgs evt)
{
  int condArg1;
  float radiusInches;
  int v3;  SpellEntry a2;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
    radiusInches = (float)(int)a2.radiusTarget * 12F;
    spellPkt.aoeObj = evt.objHndCaller;
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 16, 17, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_ice_storm(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cf920)]
public static void   BeginSpellSleetStorm(in DispatcherCallbackArgs evt)
{
  int condArg1;
  float radiusInches;
  int v3;  SpellEntry a2;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
    radiusInches = (float)(int)a2.radiusTarget * 12F;
    spellPkt.aoeObj = evt.objHndCaller;
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 34, 35, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_sleet_storm(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100d0960)]
public static void   SlipperyMindInit(in DispatcherCallbackArgs evt)
{
  int condArg2;
  int condArg1;
  SpellPacketBody spellPkt;

  condArg2 = evt.GetConditionArg2();
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.FloatSpellLine(__PAIR__(HIDWORD(spellPkt.targetListHandles[condArg2]), spellPkt.targetListHandles[condArg2]), 20029, TextFloaterColor.White);
  }
}


[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x100d3a20)]
public static void   sub_100D3A20(in DispatcherCallbackArgs evt, int data)
{
  int condArg1;
  string v2;

  int v5;


  int condArg3;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    if ( GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc, SavingThrowType.Will, 0, spellPkt.spellId) )
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30001, TextFloaterColor.White);
    }
    else
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30002, TextFloaterColor.White);
      v2 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
      GameSystems.ParticleSys.End(v2);
      if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
      {
        Logger.Info("d20_mods_spells.c / _control_plants_entangled_pre_will_save(): cannot remove target");
        return;
      }
                        SpellEffects.Spell_remove_mod(in evt);
      v5 = GameSystems.ParticleSys.CreateAtObj("sp-Entangle", evt.objHndCaller);
      spellPkt.AddTarget(evt.objHndCaller, v5, true);
      condArg3 = evt.GetConditionArg3();
      evt.objHndCaller.AddCondition("sp-Control Plants Entangle", spellPkt.spellId, spellPkt.durationRemaining, condArg3);
    }
  }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*  else
  {
    Logger.Info("d20_mods_spells.c / _control_plants_entangled_pre_will_save(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cc240)]
public static void   sub_100CC240(in DispatcherCallbackArgs evt)
{
  int condArg3;
  string v2;
  CHAR extraText2;

  condArg3 = evt.GetConditionArg3();
  v2 = GameSystems.Stat.GetStatName((Stat)condArg3);
  extraText2 = String.Format(" [{0}]", v2);
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20022, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100c4af0)]
public static void   sub_100C4AF0(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoDamage dispIo;
  int v2;
  int condArg3;

  dispIo = evt.GetDispIoDamage();
  v2 = data2;
  condArg3 = evt.GetConditionArg3();
  dispIo.damage.AddDamageBonus(condArg3, 14, v2);
}


[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x100d4440)]
public static void   sub_100D4440(in DispatcherCallbackArgs evt, int data)
{
  int condArg1;
  string v2;

  int v5;


  int condArg3;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    if ( GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc, SavingThrowType.Reflex, 0, spellPkt.spellId) )
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30001, TextFloaterColor.White);
    }
    else
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30002, TextFloaterColor.White);
      v2 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
      GameSystems.ParticleSys.End(v2);
      if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
      {
        Logger.Info("d20_mods_spells.c / _entangle_off_reflex_save(): cannot remove target");
        return;
      }
                        SpellEffects.Spell_remove_mod(in evt);
      v5 = GameSystems.ParticleSys.CreateAtObj("sp-Entangle", evt.objHndCaller);
      spellPkt.AddTarget(evt.objHndCaller, v5, true);
      condArg3 = evt.GetConditionArg3();
      evt.objHndCaller.AddCondition("sp-Entangle On", spellPkt.spellId, spellPkt.durationRemaining, condArg3);
    }
  }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*  else
  {
    Logger.Info("d20_mods_spells.c / _entangle_off_reflex_save(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100c5990)]
public static void   sub_100C5990(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoDamage dispIo;  DamagePacket v3;

  dispIo = evt.GetDispIoDamage();/*INLINED:v2=evt.subDispNode.subDispDef*/  if ( data2 == 173 )
  {
    if ( !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement) )
    {
      dispIo.damage.AddDamageBonus(-data1, 0, data2);
    }
  }
  else
  {
    v3 = &dispIo.damage;
    if ( data2 == 223 )
    {
      v3.AddDamageBonus(-data1, 0, 223);
    }
    else
    {
      v3.AddDamageBonus(data1, 0, data2);
    }
  }
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100c6ee0)]
public static void   SavingThrow_sp_ConsecrateHitUndead_Callback(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoSavingThrow dispIo;

  dispIo = evt.GetDispIoSavingThrow();
  dispIo.bonlist.AddBonus(-data1, 17, data2);
}


[DispTypes(DispatcherType.BaseCasterLevelMod)]
[TempleDllLocation(0x100c71a0)]
public static void   sub_100C71A0(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoD20Query dispIo;

  dispIo = evt.GetDispIoD20Query();
  dispIo.return_val += data2;
}


[DispTypes(DispatcherType.TakingDamage)]
[TempleDllLocation(0x100ddb90)]
public static void   sub_100DDB90(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoDamage dispIo;
  DamagePacket v2;
  int v3;
  int condArg4;
  int v5;
  int v6;
      int v9;
  DispIoDamage v10;
  CHAR extraText2;

  dispIo = evt.GetDispIoDamage();
  v2 = &dispIo.damage;
  v10 = dispIo;
  v3 = dispIo.damage.GetOverallDamageByType();
  if ( v3 <= evt.GetConditionArg4() )
  {
    condArg4 = data1;
    v9 = data2;
  }
  else
  {
    v9 = data2;
    condArg4 = evt.GetConditionArg4();
  }
  v2.AddPhysicalDR(condArg4, v9, 0x68);
  v5 = v2.GetOverallDamageByType();
  v10.damage.finalDamage = v5;
  if ( v3 > evt.GetConditionArg4() )
  {
    v6 = -v10.damage.finalDamage;
  }
  else
  {
    v6 = v5 - v3 + evt.GetConditionArg4();
  }
  evt.SetConditionArg4(v6);
  Logger.Info("absorbed {0} points of damage, DR points left: {1}", v3 - v5, v6);
  if ( v6 <= 0 )
  {
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(in evt);
                SpellEffects.Spell_remove_mod(in evt);
  }
  else
  {
    extraText2 = String.Format(" {0} ({1}/{2:+#;-#;0})", v6, data1, data2);
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20009, TextFloaterColor.White);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cc400)]
public static void   StunnedFloatMessage(in DispatcherCallbackArgs args)
{
  GameSystems.Spell.FloatSpellLine(args.objHndCaller, 20021, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c5d90)]
public static void   DeafnessSpellFailure(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;
  Dice v2;
  int v3;
  unsigned Dice v4;
  int v5;
  Dice v6;
  int v7;
  int v8;
  int v9;
  int v10;
  int mmData;
  int spellLvl;
  int spellClassCode;
  int spellEnum;
  SpellStoreData spData;

  spellLvl = 0;
  mmData = 0;
  dispIo = evt.GetDispIoD20Query();
  if ( dispIo.return_val != 1 )
  {
    GameSystems.D20.RadialMenu.SelectedRadialMenuEntry.d20SpellData.SpellEnum((D20SpellData *)dispIo.data1, &spellEnum, 0, &spellClassCode, &spellLvl, 0, &mmData);
    EncodeSpellData/*0x10075280*/(spellEnum, spellLvl, spellClassCode, 0, mmData, &spData);
    if ( GameSystems.Spell.GetSpellComponentRegardMetamagic(&spData) & 1 )
    {
      v2 = 1.new Dice(100, 0);
      v3 = GetPackedDiceBonus/*0x10038c90*/(v2);
      v4 = 1.new Dice(100, 0);
      v5 = GetPackedDiceType/*0x10038c40*/(v4);
      v6 = 1.new Dice(100, 0);
      v7 = GetPackedDiceNumDice/*0x10038c30*/(v6);
      v8 = DiceRoller/*0x10038b60*/(v7, v5, v3);
      if ( v8 >= 20 )
      {
        v10 = GameSystems.RollHistory.AddPercentageCheck(evt.objHndCaller, null, 20, 79, v8, 62, 192);
        GameSystems.RollHistory.CreateRollHistoryString(v10);
      }
      else
      {
        GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x23, evt.objHndCaller, null);
        GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 78);
        dispIo.return_val = 1;
        v9 = GameSystems.RollHistory.AddPercentageCheck(evt.objHndCaller, null, 20, 79, v8, 78, 192);
        GameSystems.RollHistory.CreateRollHistoryString(v9);
      }
    }
  }
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100d4220)]
public static void   EntangleBreakFree(in DispatcherCallbackArgs evt, int data)
{
  DispIoD20Signal dispIo;
  int v2;
  int v3;
  int v4;
  int condArg1;
  string v6;
  string v7;


  int condArg3;
  BonusList bonList;
  SpellPacketBody spellPkt;
  DispIoBonusList a1;
  BonusList bonlist;

  dispIo = evt.GetDispIoD20Signal();
  evt.GetConditionArg1();
  v2 = dispIo.data1;
  if ( (dispIo.data2 )==0)
  {
    bonlist = BonusList.Create();
    bonList = BonusList.Create();
    a1 = new DispIoBonusList();
    v3 = evt.objHndCaller.GetStat(0, &a1);
    v4 = D20StatSystem.GetModifierForAbilityScore(v3);
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      bonList.AddBonus(v4, 0, 103);
      v6 = GameSystems.Stat.GetStatName(0);
      if ( GameSystems.Spell.DispelRoll(evt.objHndCaller, &bonList, 0, 15, v6, 0) < 0 )
      {
        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20027, TextFloaterColor.Red);
      }
      else
      {
        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 21003, TextFloaterColor.White);
        v7 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v7);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _entangle_on_break_free_check(): cannot remove target");
          return;
        }
                                SpellEffects.Spell_remove_mod(evt.WithoutIO);
        spellPkt.AddTarget(evt.objHndCaller, 0, true);
        condArg3 = evt.GetConditionArg3();
        evt.objHndCaller.AddCondition("sp-Entangle Off", spellPkt.spellId, spellPkt.durationRemaining, condArg3);
      }
    }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _entangle_on_break_free_check(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cd8f0)]
public static void   sub_100CD8F0(in DispatcherCallbackArgs evt)
{
  int condArg1;

  condArg1 = evt.GetConditionArg1();
  GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_TouchAttackAdded, condArg1, 0);
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c9e90)]
public static void   CanBeAffectedActionFrame_Sanctuary(in DispatcherCallbackArgs evt, ConditionSpec data)
{
  DispIoD20Query dispIo;
  D20Action v2;
  int v3;
  int v4;
  int v5;
  GameObjectBody v6;
  long v7;
  DispIoD20Query v8;
  SpellEntry a2;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoD20Query();
  v2 = (D20Action )dispIo.data1;
  v8 = dispIo;
  if ( v2.d20ActType != D20ActionType.CAST_SPELL)
  {
    return;
  }
  if ( GameSystems.Spell.TryGetActiveSpell(v2.spellId, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
    if ( Get_First_Byte/*0x101ecc80*/(a2.modeTargetSemiBitmask) != 2 || Get_First_Byte/*0x101ecc80*/(a2.modeTargetSemiBitmask) == 1 )
    {
      return;
    }
  }
  if ( !v2.d20APerformer.HasCondition(SpellEffects.SpellSanctuarySaveFailed)     && !v2.d20APerformer.HasCondition(SpellEffects.SpellSanctuarySaveSucceeded) )
  {
    v7 = (int)v2;
    v6 = v2.d20ATarget;
LABEL_16:
    GameSystems.D20.D20SendSignal(v6, D20DispatcherKey.SIG_Spell_Sanctuary_Attempt_Save, v7, SHIDWORD(v7));
    goto LABEL_17;
  }
  if ( !v2.d20APerformer.HasCondition(SpellEffects.SpellSanctuarySaveFailed)     && v2.d20APerformer.HasCondition(SpellEffects.SpellSanctuarySaveSucceeded) )
  {
    v3 = GameSystems.D20.D20QueryReturnObject(v2.d20ATarget, D20DispatcherKey.QUE_Critter_Has_Condition, (int)SpellEffects.SpellSanctuary, 0);
    if ( (int)GameSystems.D20.D20QueryReturnObject(v2.d20APerformer, D20DispatcherKey.QUE_Critter_Has_Condition, (int)SpellEffects.SpellSanctuarySaveSucceeded, 0) == v3 )
    {
      goto LABEL_17;
    }
    v7 = (int)v2;
    v6 = v2.d20ATarget;
    goto LABEL_16;
  }
  if ( !v2.d20APerformer.HasCondition(SpellEffects.SpellSanctuarySaveSucceeded)     && v2.d20APerformer.HasCondition(SpellEffects.SpellSanctuarySaveFailed) )
  {
    v4 = GameSystems.D20.D20QueryReturnObject(v2.d20ATarget, D20DispatcherKey.QUE_Critter_Has_Condition, (int)SpellEffects.SpellSanctuary, 0);
    if ( (int)GameSystems.D20.D20QueryReturnObject(v2.d20APerformer, D20DispatcherKey.QUE_Critter_Has_Condition, (int)SpellEffects.SpellSanctuarySaveFailed, 0) != v4 )
    {
      v7 = (int)v2;
      v6 = v2.d20ATarget;
      goto LABEL_16;
    }
  }
LABEL_17:
  if ( v2.d20APerformer.HasCondition(SpellEffects.SpellSanctuarySaveFailed) )
  {
    v5 = GameSystems.D20.D20QueryReturnObject(v2.d20ATarget, D20DispatcherKey.QUE_Critter_Has_Condition, (int)SpellEffects.SpellSanctuary, 0);
    if ( (int)GameSystems.D20.D20QueryReturnObject(v2.d20APerformer, D20DispatcherKey.QUE_Critter_Has_Condition, (int)SpellEffects.SpellSanctuarySaveFailed, 0) == v5 )
    {
      v8.return_val = 0;
      *(_QWORD *)&v8.data1 = evt.GetConditionArg3();
      GameSystems.D20.Combat.FloatCombatLine(v2.d20ATarget, 123);
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cdc30)]
public static void   BeginSpellGrease(in DispatcherCallbackArgs evt)
{
  int condArg1;
  float radiusInches;
  int v3;  SpellEntry a2;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
    radiusInches = (float)(int)a2.radiusTarget * 12F;
    spellPkt.aoeObj = evt.objHndCaller;
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 14, 15, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_grease(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cbf10)]
public static void   sub_100CBF10(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg2;
  int condArg3;



  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  condArg3 = evt.GetConditionArg3();
  if ( !evt.objHndCaller.AddCondition("Charmed", condArg1, condArg2, condArg3) )
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_animal_friendship(): unable to add condition");
  }
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20018, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cc420)]
public static void   CalmEmotionsBeginSpell(in DispatcherCallbackArgs evt)
{
  int condArg1;


  SpellPacketBody v4;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out v4) )
  {
    if ( !v4.caster.AddCondition("sp-Concentrating", condArg1, 0, 0) )
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_calm_emotions(): unable to add condition to spell_caster");
    }
  }
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20031, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100dcb80)]
public static void   ChillTouchAttackHandler(in DispatcherCallbackArgs evt, int data)
{
  D20Action d20a;
  int condArg1;


  Dice v7;
  int v8;
  unsigned Dice v9;
  int v10;
  Dice v11;
  int v12;
  int v13;


  int condArg3;
      GameObjectBody tgt;
  GameObjectBody caster;
  GameObjectBody v21;
  Dice dicePacked;
  D20ActionType actionType;
  int spellId_1;
  SpellPacketBody spellPktBody;

  d20a = (D20Action )evt.GetDispIoD20Signal().data1;
  if ( (d20a.d20Caf & D20CAF.HIT)==0)
  {
    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 69);
    return;
  }
  condArg1 = evt.GetConditionArg1();
  GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPktBody);
  GameSystems.Script.Spells.SpellSoundPlay(&spellPktBody, OnAreaOfEffectHit);
  GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 68);
  if ( D20ModSpells.CheckSpellResistance(&spellPktBody, d20a.d20ATarget) )
  {
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(evt.WithoutIO);
                SpellEffects.Spell_remove_mod(evt.WithoutIO);
    return;
  }
  if ( GameSystems.Critter.IsCategory(d20a.d20ATarget, MonsterCategory.undead) )
  {
    if ( !GameSystems.D20.Combat.SavingThrowSpell(d20a.d20ATarget, spellPktBody.caster, spellPktBody.dc, SavingThrowType.Will, 0, spellPktBody.spellId) )
    {
      GameSystems.Spell.FloatSpellLine(d20a.d20ATarget, 30002, TextFloaterColor.White);
      GameSystems.Spell.FloatSpellLine(d20a.d20ATarget, 20013, TextFloaterColor.Red);
      v7 = 1.new Dice(4, spellPktBody.casterLevel);
      v8 = GetPackedDiceBonus/*0x10038c90*/(v7);
      v9 = 1.new Dice(4, spellPktBody.casterLevel);
      v10 = GetPackedDiceType/*0x10038c40*/(v9);
      v11 = 1.new Dice(4, spellPktBody.casterLevel);
      v12 = GetPackedDiceNumDice/*0x10038c30*/(v11);
      v13 = DiceRoller/*0x10038b60*/(v12, v10, v8);
      d20a.d20ATarget.AddCondition("sp-Cause Fear", spellPktBody.spellId, v13, 0);
      goto LABEL_15;
    }
    v21 = d20a.d20ATarget;
  }
  else
  {
    if ( (d20a.d20Caf & D20CAF.CRITICAL)!=0)
    {
      spellId_1 = spellPktBody.spellId;
      actionType = d20a.d20ActType;
      dicePacked = 2.new Dice(6, 0);
      caster = evt.objHndCaller;
      tgt = d20a.d20ATarget;
    }
    else
    {
      spellId_1 = spellPktBody.spellId;
      actionType = d20a.d20ActType;
      dicePacked = 1.new Dice(6, 0);
      caster = evt.objHndCaller;
      tgt = d20a.d20ATarget;
    }
    GameSystems.D20.Combat.SpellDamageFull(tgt, caster, dicePacked, DamageType.NegativeEnergy, 1, actionType, spellId_1, 0);
    if ( !GameSystems.D20.Combat.SavingThrowSpell(d20a.d20ATarget, spellPktBody.caster, spellPktBody.dc, 0, 0, spellPktBody.spellId) )
    {
      GameSystems.Spell.FloatSpellLine(d20a.d20ATarget, 30002, TextFloaterColor.White);
      GameSystems.Spell.FloatSpellLine(d20a.d20ATarget, 20022, TextFloaterColor.Red);
      d20a.d20ATarget.AddCondition("Temp_Ability_Loss", 0, 1);
      goto LABEL_15;
    }
    v21 = d20a.d20ATarget;
  }
  GameSystems.Spell.FloatSpellLine(v21, 30001, TextFloaterColor.White);
LABEL_15:
  condArg3 = evt.GetConditionArg3();
  evt.SetConditionArg3(condArg3 - 1);
  if ( evt.GetConditionArg3() <= 0 )
  {
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(evt.WithoutIO);
                SpellEffects.Spell_remove_mod(evt.WithoutIO);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cc930)]
public static void   sub_100CC930(in DispatcherCallbackArgs evt)
{
  int condArg1;  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20021, TextFloaterColor.Red);
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {/*INLINED:v2=evt.subDispNode.condNode*/    spellPkt.duration = 1;
    spellPkt.durationRemaining = 1;
    evt.SetConditionArg2(1);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_color_spray_stun(): unable to save new spell_packet");
    }
*/  }
  else
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_color_spray_stun(): unable to get spell_packet");
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cbe00)]
[TemplePlusLocation("spell_condition.cpp:86")]
public static void   SpAidOnAdd(in DispatcherCallbackArgs evt)
{
  int tempHpAmt;
  int condArg1;


  int condArg2;
  int arg0;
  CHAR extraText;
  SpellPacketBody spellPkt;

  tempHpAmt = DiceRoller/*0x10038b60*/(1, 8, 0);
  extraText = String.Format("[{0}] ", tempHpAmt);
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20005, TextFloaterColor.White, &extraText);
  Logger.Info("d20_mods_spells.c / _begin_aid(): gained {0} temporary hit points", tempHpAmt);
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    condArg2 = evt.GetConditionArg2();
    arg0 = evt.GetConditionArg1();
    if ( !evt.objHndCaller.AddCondition("Temporary_Hit_Points", arg0, condArg2, tempHpAmt) )
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_aid(): unable to add condition");
    }
  }
  else
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_aid(): unable to get spell_packet");
  }
}
/* Orphan comments:
TP Replaced @ spell_condition.cpp:86
*/


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100c4b30)]
public static void   EmotionToHitBonus2(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;
  dispIo = evt.GetDispIoAttackBonus();/*INLINED:v2=evt.subDispNode.subDispDef*/  switch ( data2 )
  {
    case 0x98:
    case 0xA9:
      dispIo.bonlist.AddBonus(-data1, 13, data2);
      break;
    case 0xAC:
    case 0x103:
      if ( !evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions)
        && !evt.objHndCaller.HasCondition(SpellEffects.SpellRemoveFear) )
      {
        dispIo.bonlist.AddBonus(-data1, 13, data2);
      }
      break;
    case 0x8E:
    case 0xFC:
    case 0x104:
    case 0x12A:
    case 0x12B:
      if ( !evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions) )
      {
        dispIo.bonlist.AddBonus(data1, 13, data2);
      }
      break;
    default:
      dispIo.bonlist.AddBonus(data1, 13, data2);
      break;
  }
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x100c8200)]
public static void   GlibnessSkillLevel(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoObjBonus dispIo;

  dispIo = evt.GetDispIoObjBonus();
  if ( !GameSystems.Combat.IsCombatActive() )
  {
    dispIo.bonOut.AddBonus(data1, 0, data2);
  }
}


[DispTypes(DispatcherType.AbilityScoreLevel)]
[TempleDllLocation(0x100c9760)]
public static void   sub_100C9760(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoBonusList dispIo;

  dispIo = evt.GetDispIoBonusList();
  dispIo.bonlist.AddBonus(data1, 13, data2);
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100d75d0)]
public static void   OnSpellEndRemoveMod(in DispatcherCallbackArgs evt, int data1, ConditionSpec data2)
{
  DispIoD20Signal dispIo;

  dispIo = evt.GetDispIoD20Signal();
  if ( dispIo.data1 == evt.GetConditionArg1() )
  {
    *(_DWORD *)&v2[20] = 0;
    *(_QWORD *)&v2[12] = *(_QWORD *)&evt.dispType;
    *(_QWORD *)&v2[4] = evt.objHndCaller;
    *(_DWORD *)v2 = evt.subDispNode;
    SpellEffects.Spell_remove_mod(in evt);
  }
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100dd240)]
public static void   sub_100DD240(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;

  if ( evt.GetConditionArg3() > 0 )
  {
    dispIo = evt.GetDispIoAttackBonus();
    dispIo.bonlist.AddBonus(data1, 34, data2);
    if ( (dispIo.attackPacket.flags & D20CAF.FINAL_ATTACK_ROLL)!=0)
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                        SpellEffects.Spell_remove_spell(evt.WithoutIO);
                        SpellEffects.Spell_remove_mod(evt.WithoutIO);
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cfdb0)]
public static void   SpiritualWeaponBeginSpellDismiss(in DispatcherCallbackArgs evt)
{
  int condArg1;


  int v4;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Anim.Interrupt(evt.objHndCaller, 6, 0);
    v4 = evt.GetConditionArg1();
    if ( !spellPkt.caster.AddCondition("Dismiss", v4, 0, 0) )
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_dismiss(): unable to add condition");
    }
  }
  else
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_dismiss(): unable to get spell_packet");
  }
}


[DispTypes(DispatcherType.ReflexThrow)]
[TempleDllLocation(0x100c7b70)]
public static void   FireShield_callback_31h(in DispatcherCallbackArgs evt)
{
  DispIoReflexThrow dispIo;
  int condArg3;
  int v3;

  dispIo = evt.GetDispIoReflexThrow();
  condArg3 = evt.GetConditionArg3();
  if ( condArg3 == 3 )
  {
    v3 = 10;
  }
  else
  {
    if ( condArg3 != 9 )
    {
      return;
    }
    v3 = 8;
  }
  if ( dispIo.attackType == v3 && dispIo.throwResult )
  {
    dispIo.throwResult = 4;
    dispIo.effectiveReduction = 0;
    dispIo.damageMesLine = 109;
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cd230)]
public static void   DominateAnimal(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg2;
  int condArg3;


  GameObjectBody v6;

  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  condArg3 = evt.GetConditionArg3();
  if ( !evt.objHndCaller.AddCondition("Charmed", condArg1, condArg2, condArg3) )
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_dominate_animal(): unable to add condition");
  }
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20018, TextFloaterColor.Red);
  v6 = GameSystems.Party.GetConsciousLeader();
  GameSystems.Critter.AddFollower(evt.objHndCaller, v6, 1, 0);
  GameUiBridge.UpdatePartyUi();
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100c3450)]
public static void   TouchAttackDischargeRadialMenu(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  int v2;
  string meslineValue;
int meslineKey;
  RadialMenuEntry radMenuEntry;

  radMenuEntry = RadialMenuEntry.Create();
  radMenuEntry.d20ActionType = D20ActionType.TOUCH_ATTACK;
  radMenuEntry.d20ActionData1 = 0;
  meslineKey = 5036;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  radMenuEntry.text = (string )meslineValue;
  radMenuEntry.helpSystemHashkey = "TAG_TOUCH_ATTACK"/*ELFHASH*/;
  v2 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Offense);
  GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v2);
}


[DispTypes(DispatcherType.ConditionAddPre)]
[TempleDllLocation(0x100db9c0)]
public static void   sub_100DB9C0(in DispatcherCallbackArgs evt, ConditionSpec data1, int data2)
{

  if ( evt.GetDispIoCondStruct().condStruct == (ConditionSpec )data1 )
  {
                    SpellEffects.Spell_remove_spell(in evt);
                    SpellEffects.Spell_remove_mod(in evt);
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c8cc0)]
public static void   sub_100C8CC0(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;
  int condArg1;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoD20Query();
  dispIo.return_val = evt.objHndCaller.HasCondition(SpellEffects.SpellSummoned)                     && (condArg1 = evt.GetConditionArg1(),
                        GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
                    && D20ModSpells.CheckSpellResistance(&spellPkt, evt.objHndCaller) ;
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x100dd360)]
public static void   GuidanceSkillLevel(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoObjBonus dispIo;

  dispIo = evt.GetDispIoObjBonus();
  if ( evt.GetConditionArg(4) > 0 )
  {
    dispIo.bonOut.AddBonus(data1, 34, data2);
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(evt.WithoutIO);
                SpellEffects.Spell_remove_mod(evt.WithoutIO);
  }
}


[DispTypes(DispatcherType.AbilityScoreLevel)]
[TempleDllLocation(0x100c7a60)]
public static void   sub_100C7A60(in DispatcherCallbackArgs evt, int data)
{
  DispIoBonusList dispIo;

  dispIo = evt.GetDispIoBonusList();
  dispIo.bonlist.AddCap(0, data, 0x8F);
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100dcf10)]
public static void   DispelAlignmentTouchAttackSignalHandler(in DispatcherCallbackArgs evt, int data)
{
  D20Action d20a;
  int condArg1;
  int v3;
  int v4;
  int v5;
  int v6;
  int v7;
  int v8;      DispIoDispelCheck dispIo;
  SpellPacketBody spellPktBody;

  d20a = (D20Action )evt.GetDispIoD20Signal().data1;
  if ( (d20a.d20Caf & D20CAF.HIT )!=0)
  {
    condArg1 = evt.GetConditionArg1();
    GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPktBody);
    GameSystems.Script.Spells.SpellSoundPlay(&spellPktBody, OnAreaOfEffectHit);
    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 68);
    if ( IsPc/*0x1007e570*/(d20a.d20ATarget) )
    {
      GameSystems.Spell.PlayFizzle(d20a.d20ATarget);
    }
    else if ( d20a.d20ATarget.HasCondition(SpellEffects.SpellSummoned) )
    {
      if ( !D20ModSpells.CheckSpellResistance(&spellPktBody, d20a.d20ATarget) )
      {
        switch ( data )
        {
          case 0x45:
            v3 = d20a.d20ATarget.GetStat(Stat.alignment) - 2;
            if ( (v3 )==0)
            {
              goto LABEL_19;
            }
            v4 = v3 - 4;
            if ( (v4 )==0|| v4 == 4 )
            {
              goto LABEL_19;
            }
            break;
          case 0x46:
            v5 = d20a.d20ATarget.GetStat(Stat.alignment);
            if ( v5 >= ALIGNMENT_EVIL && v5 <= ALIGNMENT_CHAOTIC_EVIL )
            {
              goto LABEL_19;
            }
            break;
          case 0x47:
            v6 = d20a.d20ATarget.GetStat(Stat.alignment);
            if ( v6 >= ALIGNMENT_GOOD && v6 <= ALIGNMENT_CHAOTIC_GOOD )
            {
              goto LABEL_19;
            }
            break;
          case 0x48:
            v7 = d20a.d20ATarget.GetStat(Stat.alignment) - 1;
            if ( (v7 )==0|| (v8 = v7 - 4) == 0 || v8 == 4 )
            {
LABEL_19:
              if ( GameSystems.D20.Combat.SavingThrowSpell(d20a.d20ATarget, spellPktBody.caster, spellPktBody.dc, SavingThrowType.Will, 0, spellPktBody.spellId) )
              {
                GameSystems.Spell.FloatSpellLine(d20a.d20ATarget, 30001, TextFloaterColor.White);
              }
              else
              {
                GameSystems.Spell.FloatSpellLine(d20a.d20ATarget, 30002, TextFloaterColor.White);
                GameSystems.D20.Combat.Kill(d20a.d20ATarget, evt.objHndCaller);
              }
            }
            break;
          default:
            break;
        }
      }
    }
    else
    {
      dispIo = new DispIoDispelCheck();
      condArg1 = evt.GetConditionArg1();/*INLINED:v10=evt.subDispNode.subDispDef*/      dispIo.spellId = condArg1;
      dispIo.returnVal = 1;
      switch ( data )
      {
        case 0x45:
          dispIo.flags = 2;
          break;
        case 0x46:
          dispIo.flags = 4;
          break;
        case 0x47:
          dispIo.flags = 8;
          break;
        case 0x48:
          dispIo.flags = 16;
          break;
        default:
          dispIo.flags = 0;
          break;
      }
      d20a.d20ATarget.DispatchDispelCheck(&dispIo);
      nullsub_1/*0x100027f0*/();
    }
                SpellEffects.Spell_remove_spell(evt.WithoutIO);
                SpellEffects.Spell_remove_mod(evt.WithoutIO);
  }
  else
  {
    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 69);
  }
}


[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x100c3fe0)]
public static void   sub_100C3FE0(in DispatcherCallbackArgs evt, int data)
{
  int condArg1;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    if ( GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc, SavingThrowType.Will, 0, spellPkt.spellId) )
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30001, TextFloaterColor.White);
      GameSystems.Spell.PlayFizzle(evt.objHndCaller);
      evt.SetConditionArg3(1);
    }
    else
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30002, TextFloaterColor.White);
    }
  }
}


[DispTypes(DispatcherType.AbilityScoreLevel)]
[TempleDllLocation(0x100c7890)]
public static void   sub_100C7890(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoBonusList dispIo;

  dispIo = evt.GetDispIoBonusList();
  if ( evt.dispKey - 1 == data1 )
  {
    dispIo.bonlist.AddBonus(-4, 0, data2);
  }
}


[DispTypes(DispatcherType.AbilityCheckModifier)]
[TempleDllLocation(0x100c5020)]
public static void   AbilityCheckModifierEmotion(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoObjBonus dispIo;  int v3;

  dispIo = evt.GetDispIoObjBonus();/*INLINED:v2=evt.subDispNode.subDispDef*/  switch ( data2 )
  {
    case 0xA9:
      v3 = -data1;
      goto LABEL_11;
    case 0xAC:
      if ( !evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions)
        && !evt.objHndCaller.HasCondition(SpellEffects.SpellRemoveFear) )
      {
        dispIo.bonOut.AddBonus(-data1, 13, data2);
      }
      break;
    case 0x103:
      if ( !evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions) )
      {
        dispIo.bonOut.AddBonus(-data1, 13, data2);
      }
      break;
    case 0x104:
      if ( !evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions) )
      {
        dispIo.bonOut.AddBonus(data1, 13, data2);
      }
      break;
    default:
      v3 = data1;
LABEL_11:
      dispIo.bonOut.AddBonus(v3, 13, data2);
      break;
  }
}


[DispTypes(DispatcherType.GetMoveSpeed)]
[TempleDllLocation(0x100c6280)]
public static void   sub_100C6280(in DispatcherCallbackArgs evt)
{
  DispIoMoveSpeed dispIo;

  dispIo = evt.GetDispIoMoveSpeed();
  dispIo.factor = dispIo.factor * 0.75F;
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100c89c0)]
public static void   IceStormDamage(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int v2;
  Dice v3;
  int v4;
  Dice v5;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    v2 = spellPkt.spellId;
    v3 = 3.new Dice(6, 0);
    GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v3, 0, 1, D20ActionType.CAST_SPELL, v2, 0);
    v4 = spellPkt.spellId;
    v5 = 2.new Dice(6, 0);
    GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v5, DamageType.Cold, 1, D20ActionType.CAST_SPELL, v4, 0);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100d03d0)]
public static void   sub_100D03D0(in DispatcherCallbackArgs evt)
{
  int condArg1;


  int condArg2;
  int v5;
  CHAR extraText;
  SpellPacketBody spellPkt;

  extraText = String.Format("[{0}] ", 1);
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20005, TextFloaterColor.White, &extraText);
  Logger.Info("d20_mods_spells.c / _begin_spell_virtue(): gained {0} temporary hit points", 1);
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    condArg2 = evt.GetConditionArg2();
    v5 = evt.GetConditionArg1();
    if ( !evt.objHndCaller.AddCondition("Temporary_Hit_Points", v5, condArg2, 1) )
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_virtue(): unable to add condition");
    }
  }
  else
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_virtue(): unable to get spell_packet");
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cfc30)]
public static void   BeginSpellSolidFog(in DispatcherCallbackArgs evt)
{
  int condArg1;
  float radiusInches;
  int v3;  SpellEntry a2;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
    radiusInches = (float)(int)a2.radiusTarget * 12F;
    spellPkt.aoeObj = evt.objHndCaller;
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 38, 39, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_solid_fog(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.GetMoveSpeed)]
[TempleDllLocation(0x100c7080)]
public static void   sub_100C7080(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoMoveSpeed dispIo;

  dispIo = evt.GetDispIoMoveSpeed();
  if ( !evt.objHndCaller.HasCondition(SpellEffects.SpellControlPlantsDisentangle) )
  {
    LODWORD(dispIo.factor) = 0;
  }
}


[DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
[TempleDllLocation(0x100cc800)]
public static void   sub_100CC800(in DispatcherCallbackArgs evt)
{
  evt.SetConditionArg3(0);
}


[DispTypes(DispatcherType.DispelCheck)]
[TempleDllLocation(0x100db690)]
public static void   DispelCheck(in DispatcherCallbackArgs evt, int data)
{
  DispIoDispelCheck dispIo;
      int condArg1;
  int flags;
  string v6;

  int isDispelMagic;
  string spellName;
      SpellPacketBody spPkt;
  BonusList bonlist;
  CHAR textBuffer[512];
  SpellPacketBody dispellPkt;

  bonlist = BonusList.Create();
  dispIo = evt.GetDispIoDispelCheck();
  if ( (dispIo.flags & 0x20 )!=0)
  {
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, spell_mes_A_Spell_has_expired, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(in evt);
                SpellEffects.Spell_remove_mod(in evt);
  }
  if ( GameSystems.Spell.TryGetActiveSpell(dispIo.spellId, out dispellPkt) )
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spPkt) )
    {
      flags = dispIo.flags;
      if ( (flags & 1 )!=0&& spPkt.spellKnownSlotLevel < 4 && (CHAR)flags >= 0
        || (CHAR)flags < 0 && spPkt.spellKnownSlotLevel < 4 && dispIo.returnVal > 0
        || (flags & 0x40
)!=0        || (flags & 0x1E )!=0&& dispIo.returnVal > 0 )
      {
        bonlist.AddBonus(dispellPkt.casterLevel, 0, 203);
        v6 = GameSystems.Spell.GetSpellName(dispellPkt.spellEnum);
        if ( GameSystems.Spell.DispelRoll(dispellPkt.caster, &bonlist, 0, spPkt.casterLevel + 11, v6, 0) >= 0
          || dispellPkt.caster == spPkt.caster )
        {
          flags = dispIo.flags;
          isDispelMagic = dispIo.flags & 1;
          if ( (isDispelMagic )==0|| (CHAR)flags < 0 )
          {
            --dispIo.returnVal;
          }
          if ( isDispelMagic == 1
            || (flags & 0x40
)!=0            || (flags & 2 )!=0&& spPkt.caster.HasChaoticAlignment()
            || (dispIo.flags & 4 )!=0&& spPkt.caster.HasEvilAlignment()
            || (dispIo.flags & 8 )!=0&& spPkt.caster.HasGoodAlignment()
            || (dispIo.flags & 0x10 )!=0&& spPkt.caster.HasLawfulAlignment() )
          {
            spellName = GameSystems.Spell.GetSpellName(spPkt.spellEnum);
            textBuffer = String.Format(" [{0}]", spellName);
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20002, TextFloaterColor.White);
                                                SpellEffects.Spell_remove_spell(in evt);
                                                SpellEffects.Spell_remove_mod(in evt);
          }
        }
        else
        {
          GameSystems.Spell.FloatSpellLine(evt.objHndCaller, spell_mes_Dispel_attempt_failed, TextFloaterColor.Red);
          GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
        }
      }
      nullsub_1/*0x100027f0*/();
    }
    else
    {
      Logger.Info("d20_mods_spells.c / _dispel_check(): error getting spellid packet for spell_packet");
    }
  }
  else
  {
    Logger.Info("d20_mods_spells.c / _dispel_check(): error getting spellid packet for dispel_packet");
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cc6d0)]
public static void   sub_100CC6D0(in DispatcherCallbackArgs evt)
{
  GameSystems.MapFogging.Disable();
}


[DispTypes(DispatcherType.TakingDamage)]
[TempleDllLocation(0x100c7fb0)]
public static void   sub_100C7FB0(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoDamage dispIo;

  dispIo = evt.GetDispIoDamage();
  dispIo.damage.AddPhysicalDR(data1, data2, 0x68);
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100d32b0)]
[TemplePlusLocation("condition.cpp:3527")]
public static void   ConcentratingActionRecipient(in DispatcherCallbackArgs evt, int data)
{
  D20Action d20a;
  int condArg1;
  int conceSpellId;
  int d20aSpellId;
    SpellPacketBody conceSpellPkt;
  SpellPacketBody spellPkt;

  if ( evt.dispIO !=null)
  {
    if ( evt.dispKey != D20DispatcherKey.SIG_Killed && evt.dispIO.ioType == 6 )
    {
      d20a = (D20Action )evt.GetDispIoD20Signal().data1;
      condArg1 = evt.GetConditionArg1();
      conceSpellId = condArg1;
      if ( d20a !=null)
      {
        if ( d20a.d20ActType == D20ActionType.CAST_SPELL)
        {
          d20aSpellId = d20a.spellId;
          if ( d20aSpellId != condArg1 )
          {
            if ( GameSystems.Spell.TryGetActiveSpell(d20aSpellId, out spellPkt) )
            {
              if ( GameSystems.Spell.TryGetActiveSpell(conceSpellId, out conceSpellPkt) )
              {
                if ( conceSpellPkt.spellEnum != WellKnownSpells.MeldIntoStone                  && !GameSystems.Skill.SkillRoll(evt.objHndCaller, SkillId.concentration, spellPkt.dc + conceSpellPkt.spellKnownSlotLevel, 0, 1) )
                {
                  GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(32, evt.objHndCaller, null);
                  GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 54);
                  *(_QWORD *)&v5[16] = 156;
                  *(_QWORD *)&v5[8] = __PAIR__(28, HIDWORD(evt.objHndCaller));
                  *(_QWORD *)v5 = *(_QWORD *)&evt.subDispNode;
                  SpellEffects.Spell_remove_mod(in evt);
                }
              }
              else
              {
                Logger.Info("d20_mods_spells.c / _concentrating_action_recipient(): error, unable to retrieve concentration_packet");
              }
            }
            else
            {
              Logger.Info("d20_mods_spells.c / _concentrating_action_recipient(): error, unable to retrieve spell_packet");
            }
          }
        }
      }
    }
  }
}
/* Orphan comments:
TP Replaced @ condition.cpp:3527
*/


[DispTypes(DispatcherType.ConditionAddPre)]
[TempleDllLocation(0x100c68b0)]
public static void   CloudkillDamagePreAdd(in DispatcherCallbackArgs evt, ConditionSpec data)
{
  DispIoCondStruct dispIo;

  dispIo = evt.GetDispIoCondStruct();
  if ( dispIo.condStruct == (ConditionSpec )data
    && evt.objHndCaller.HasCondition(SpellEffects.SpellCloudkill) )
  {
    dispIo.outputFlag = 0;
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cdb10)]
public static void   sub_100CDB10(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg2;
  int condArg3;
  int v4;


  Dice v7;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  condArg3 = evt.GetConditionArg3();
  v4 = evt.GetConditionArg1();
  if ( !evt.objHndCaller.AddCondition("sp-Goodberry Tally", condArg1, condArg2, condArg3) )
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_goodberry(): unable to add condition");
  }
  if ( GameSystems.Spell.TryGetActiveSpell(v4, out spellPkt) )
  {
    v7 = 1.new Dice(1, 0);
    GameSystems.Combat.SpellHeal(evt.objHndCaller, spellPkt.caster, v7, D20ActionType.CAST_SPELL, v4);
    GameSystems.Critter.HealSubdualSub_100B9030(evt.objHndCaller, 1);
  }
  else
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_goodberry(): unable to save new spell_packet");
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cf2c0)]
public static void   SpellReduceSetModelScale(in DispatcherCallbackArgs args)
{
  double v1;
  int n32Data;
  int v3;

  v1 = (float)args.objHndCaller.GetInt32(obj_f.model_scale) * 0.5555556F;
  args.objHndCaller.SetInt32(obj_f.model_scale, (ulong)v1);
  GameSystems.Critter.UpdateModelEquipment(args.objHndCaller);
  *(float *)&n32Data = args.objHndCaller.GetFloat(obj_f.speed_run) * 1.8F;
  args.objHndCaller.SetInt32(obj_f.speed_run, n32Data);
  *(float *)&v3 = args.objHndCaller.GetFloat(obj_f.speed_walk) * 1.8F;
  args.objHndCaller.SetInt32(obj_f.speed_walk, v3);
}


[DispTypes(DispatcherType.GetDefenderConcealmentMissChance)]
[TempleDllLocation(0x100c62d0)]
[TemplePlusLocation("spell_condition.cpp:254")]
public static void   BlinkMissChance(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;

  dispIo = evt.GetDispIoAttackBonus();
  if ( GameSystems.D20.D20Query(dispIo.attackPacket.attacker, D20DispatcherKey.QUE_Critter_Has_True_Seeing)
    && GameSystems.D20.D20Query(dispIo.attackPacket.victim, D20DispatcherKey.QUE_Critter_Can_See_Ethereal) )
  {
    dispIo.bonlist.AddBonus(50, 19, data2);
  }
  else
  {
    dispIo.bonlist.AddBonus(20, 19, data2);
  }
}
/* Orphan comments:
TP Replaced @ spell_condition.cpp:254
*/


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d51e0)]
public static void   InvisibilitySphereAoeEvent(in DispatcherCallbackArgs evt, int data)
{
  DispIoObjEvent dispIo;
  int condArg1;
  string v3;
    GameObjectBody v5;

  int v7;


  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoObjEvent();
  if ( dispIo.evtId == evt.GetConditionArg3() )
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      SpellPktTriggerAoeHitScript/*0x100c37d0*/(spellPkt.spellId);
      if ( D20ModSpells.CheckSpellResistance(&spellPkt, dispIo.tgt) )
      {
        return;
      }
      if ( evt.dispKey == D20DispatcherKey.OnEnterAoE )
      {
        v5 = dispIo.tgt;
        v7 = GameSystems.ParticleSys.CreateAtObj("sp-Invisibility Sphere-HIT", v5);
        spellPkt.AddTarget(dispIo.tgt, v7, true);
        dispIo.tgt.AddCondition("sp-Invisibility Sphere Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        v3 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v3);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _invisibility_sphere_hit_trigger(): cannot remove target");
          return;
        }
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        *(_DWORD *)&v4[20] = evt.dispIO;
        *(_DWORD *)&v4[16] = 19;
        *(_QWORD *)&v4[8] = *(GameObjectBody *)((string )&evt.objHndCaller + 4);
        *(_QWORD *)v4 = *(_QWORD *)&evt.subDispNode;
        SpellEffects.Spell_remove_mod(in evt);
      }
    }
  }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*  else
  {
    Logger.Info("d20_mods_spells.c / _invisibility_sphere_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cda00)]
public static void   BeginSpellGhoulTouchStench(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int v2;  SpellEntry a2;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
    spellPkt.aoeObj = evt.objHndCaller;
    v2 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 12, 13, ObjectListFilter.OLC_CRITTERS, 120F, 0F, 6.28318548F);
    evt.SetConditionArg3(v2);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_ghoul_touch_stench(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100c61d0)]
public static void   SavingThrow_sp_BestowCurseRolls_Callback(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoSavingThrow dispIo;

  dispIo = evt.GetDispIoSavingThrow();
  dispIo.bonlist.AddBonus(-data1, 0, data2);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cff90)]
public static void   BeginSpellSpikeStones(in DispatcherCallbackArgs evt)
{
  int condArg1;
  float radiusInches;
  int v3;  SpellEntry a2;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
    radiusInches = (float)(int)a2.radiusTarget * 12F;
    spellPkt.aoeObj = evt.objHndCaller;
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 42, 43, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_spike_stones(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c4370)]
public static void   sub_100C4370(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;
  int condArg1;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoD20Query();
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    dispIo.return_val = 1;
    *(_QWORD *)&dispIo.data1 = spellPkt.caster;
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cf460)]
public static void   sub_100CF460(in DispatcherCallbackArgs evt)
{
  int condArg3;
  int condArg2;
  int v3;
  GameObjectBody v4;

  evt.SetConditionArg4(10);
  switch ( evt.GetConditionArg2() )
  {
    case 1:
      v4 = evt.objHndCaller;
      v3 = 14004;
      goto LABEL_7;
    case 3:
      v4 = evt.objHndCaller;
      v3 = 14006;
      goto LABEL_7;
    case 6:
      v4 = evt.objHndCaller;
      v3 = 14012;
      goto LABEL_7;
    case 9:
      v4 = evt.objHndCaller;
      v3 = 14008;
      goto LABEL_7;
    case 16:
      v4 = evt.objHndCaller;
      v3 = 14010;
LABEL_7:
      GameSystems.SoundGame.PositionalSound(v3, 1, v4);
      condArg3 = evt.GetConditionArg3();
      condArg2 = evt.GetConditionArg2();
      evt.SetConditionArg3(condArg2);
      evt.SetConditionArg2(condArg3);
      break;
    default:
      return;
  }
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100dd8f0)]
public static void   ShockingGraspTouchAttack(in DispatcherCallbackArgs evt, int data)
{
  D20Action d20a;
  int condArg1;
      int v5;
  Dice v6;
      SpellPacketBody spellPktBody;

  d20a = (D20Action )evt.GetDispIoD20Signal().data1;
  if ( (d20a.d20Caf & D20CAF.HIT)!=0)
  {
    condArg1 = evt.GetConditionArg1();
    GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPktBody);
    GameSystems.Script.Spells.SpellSoundPlay(&spellPktBody, OnAreaOfEffectHit);
    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 68);
    if ( D20ModSpells.CheckSpellResistance(&spellPktBody, d20a.d20ATarget) )
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                        SpellEffects.Spell_remove_spell(evt.WithoutIO);
                        SpellEffects.Spell_remove_mod(evt.WithoutIO);
    }
    else
    {
      v5 = spellPktBody.casterLevel;
      if ( spellPktBody.casterLevel > 5 )
      {
        v5 = 5;
      }
      if ( (d20a.d20Caf & D20CAF.CRITICAL)!=0)
      {
        v5 *= 2;
      }
      v6 = v5.new Dice(8, 0);
      GameSystems.D20.Combat.SpellDamageFull(d20a.d20ATarget, evt.objHndCaller, v6, DamageType.Electricity, 1, d20a.d20ActType, spellPktBody.spellId, 0);
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                        SpellEffects.Spell_remove_spell(evt.WithoutIO);
                        SpellEffects.Spell_remove_mod(evt.WithoutIO);
    }
  }
  else
  {
    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 69);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cd2d0)]
public static void   Dominate_Person(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg2;
  int condArg3;


  GameObjectBody v6;

  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  condArg3 = evt.GetConditionArg3();
  if ( !evt.objHndCaller.AddCondition("Charmed", condArg1, condArg2, condArg3) )
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_dominate_person(): unable to add condition");
  }
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20018, TextFloaterColor.Red);
  v6 = GameSystems.Party.GetConsciousLeader();
  GameSystems.Critter.AddFollower(evt.objHndCaller, v6, 1, 0);
  GameUiBridge.UpdatePartyUi();
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cc300)]
public static void   BreakEnchantmentInit(in DispatcherCallbackArgs evt, int data)
{
  DispIoDispelCheck dispIo;

  dispIo = new DispIoDispelCheck();
  dispIo.spellId = evt.GetConditionArg1();
  dispIo.returnVal = 0;
  dispIo.flags = 64;
  evt.objHndCaller.DispatchDispelCheck(&dispIo);
  nullsub_1/*0x100027f0*/();
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100cb760)]
public static void   WebSpellInterrupted(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;

  dispIo = evt.GetDispIoD20Query();
  if ( dispIo.return_val != 1
    && !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement)
    && !GameSystems.Skill.SkillRoll(evt.objHndCaller, SkillId.concentration, 15, 0, 1) )
  {
    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 54);
    dispIo.return_val = 1;
  }
}
/* Orphan comments:
concentration
*/


[DispTypes(DispatcherType.TurnBasedStatusInit)]
[TempleDllLocation(0x100c4420)]
public static void   OnBeginRoundDisableMovement(in DispatcherCallbackArgs evt)
{
  DispIOTurnBasedStatus dispIo;
  TurnBasedStatus *v2;

  dispIo = evt.GetDispIOTurnBasedStatus();
  if ( dispIo !=null)
  {
    v2 = dispIo.tbStatus;
    if ( v2 )
    {
      v2.tbsFlags |= TurnBasedStatusFlags.Moved;
    }
  }
}


[DispTypes(DispatcherType.GetMoveSpeed)]
[TempleDllLocation(0x100c7040)]
public static void   sub_100C7040(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoMoveSpeed dispIo;

  dispIo = evt.GetDispIoMoveSpeed();
  if ( !evt.objHndCaller.HasCondition(SpellEffects.SpellControlPlantsDisentangle) )
  {
    dispIo.factor = dispIo.factor * 0.5F;
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100d08a0)]
public static void   FrogTongueSwallowedOnAdd(in DispatcherCallbackArgs evt)
{
  int condArg1;
  ObjectNode *objNode;
  ObjListResult listResult;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    ObjList.ListVicinity(evt.objHndCaller, ObjectListFilter.OLC_NPC, &listResult);
    objNode = listResult.ObjectHandles.
    if ( listResult.ObjectHandles.)
    {
      do
      {
        if ( !GameSystems.Critter.IsFriendly(objNode.item.handle, evt.objHndCaller) && objNode.item.handle != spellPkt.caster )
        {
          NpcAiListAppendType1/*0x1005cd30*/(objNode.item.handle, evt.objHndCaller);
        }
        objNode = objNode.item.next;
      }
      while ( objNode );
    }
    ObjListFree/*0x1001f2c0*/(&listResult);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ccb10)]
public static void   BeginSpellConsecrate(in DispatcherCallbackArgs evt)
{
  int condArg1;
  float radiusInches;
  int v3;  SpellEntry a2;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
    radiusInches = (float)(int)a2.radiusTarget * 12F;
    spellPkt.aoeObj = evt.objHndCaller;
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 2, 3, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_consecrate(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100d2e30)]
public static void   sub_100D2E30(in DispatcherCallbackArgs evt)
{
  evt.GetConditionArg3();
}


[DispTypes(DispatcherType.EffectTooltip)]
[TempleDllLocation(0x100c3e70)]
public static void   EffectTooltipBlindnessDeafness(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoEffectTooltip dispIo;
  int condArg1;
  int v3;
  int v4;
  string v5;
  string v6;
  CHAR extraString;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoEffectTooltip();
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    v3 = spellPkt.duration;
    v4 = spellPkt.durationRemaining;
    v5 = GameSystems.D20.Combat.GetCombatMesLine(0xAF);
    v6 = GameSystems.D20.Combat.GetCombatMesLine(data2);
    extraString = String.Format("{0} {1}: {2}/{3}", v6, v5, v4, v3);
    EffectTooltipAppend/*0x100f4680*/(dispIo.bdb, data1, spellPkt.spellEnum, &extraString);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100d0280)]
public static void   SummonSwarmBeginSpell(in DispatcherCallbackArgs evt)
{
  int condArg1;


  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  evt.SetConditionArg3(0);
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    if ( !spellPkt.caster.AddCondition("sp-Concentrating", condArg1, 0, 0) )
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_summon_swarm(): unable to add condition to spell_caster");
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cd6d0)]
public static void   Condition_sp_False_Life_Init(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int v2;
  int v3;


  int condArg2;
  int v7;
  CHAR prefix;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    v2 = spellPkt.casterLevel;
    if ( spellPkt.casterLevel >= 10 )
    {
      v2 = 10;
    }
    v3 = DiceRoller/*0x10038b60*/(1, 10, v2);
    prefix = String.Format("[{0}] ", v3);
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, spell_mes_Temporary_Hit_Points_Gained, TextFloaterColor.White, &prefix);
    Logger.Info("d20_mods_spells.c / _begin_spell_false_life(): gained {0} temporary hit points", v3);
    condArg2 = evt.GetConditionArg2();
    v7 = evt.GetConditionArg1();
    if ( !evt.objHndCaller.AddCondition("Temporary_Hit_Points", v7, condArg2, v3) )
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_false_life(): unable to add condition");
    }
  }
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100c9320)]
public static void   SavingThrowPenalty_sp_Prayer_Callback(in DispatcherCallbackArgs evt, int data)
{
  int condArg3;
  int v2;
  int v3;
  BonusList *v4;

  condArg3 = evt.GetConditionArg3();
  v2 = data;
  v3 = condArg3;
  v4 = &evt.GetDispIoSavingThrow().bonlist;
  if ( v3 != -3 )
  {
    v2 = -v2;
  }
  v4.AddBonus(v2, 14, 151);
}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100cb330)]
public static void   vampiric_touch_taking_damage(in DispatcherCallbackArgs evt, int data)
{
  DispIoDamage dispIo;
  int v2;
  DamagePacket v3;
  int condArg3;
  int v5;
  int v6;
  int v7;
  CHAR extraString;

  dispIo = evt.GetDispIoDamage();
  v2 = dispIo.damage.finalDamage;
  v3 = &dispIo.damage;
  condArg3 = evt.GetConditionArg3();
  v5 = condArg3;
  if ( v2 > 0 )
  {
    Logger.Info("took {0} damage, temp_hp = {1}", v2, condArg3);
    v6 = v5 - v2;
    if ( v5 - v2 <= 0 )
    {
      v6 = 0;
    }
    Logger.Info("({0}) temp_hp left", v6);
    evt.SetConditionArg3(v6);
    if ( v5 - v2 > 0 )
    {
      v3.AddDamageBonus(-v2, 0, 154);
      v3.finalDamage = 0;
      Logger.Info(", absorbed {0} points of damage", v2);
      extraString = String.Format("[{0}] ", v2);
      Float_Combat_mes_with_extra_Strings/*0x100b4bf0*/(evt.objHndCaller, combat_mes_Points_of_Damage_Absorbed, &extraString, 0);
    }
    else
    {
      v7 = v2 - v5;
      Logger.Info(", taking modified damage {0}", v7);
      extraString = String.Format("[{0}] ", v5);
      Float_Combat_mes_with_extra_Strings/*0x100b4bf0*/(evt.objHndCaller, combat_mes_Points_of_Damage_Absorbed, &extraString, 0);
      v3.AddDamageBonus(-v5, 0, 154);
      v3.finalDamage = v7;
    }
  }
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d3850)]
public static void   Condition__36__consecrate_sthg(in DispatcherCallbackArgs evt, int data)
{
  DispIoObjEvent dispIo;
  int condArg1;
  string v3;


  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoObjEvent();
  if ( dispIo.evtId == evt.GetConditionArg3() )
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      SpellPktTriggerAoeHitScript/*0x100c37d0*/(spellPkt.spellId);
      if ( D20ModSpells.CheckSpellResistance(&spellPkt, dispIo.tgt) )
      {
        return;
      }
      if ( evt.dispKey == D20DispatcherKey.OnEnterAoE )
      {
        spellPkt.AddTarget(dispIo.tgt, 0, true);
        if ( GameSystems.Critter.IsCategory(dispIo.tgt, MonsterCategory.undead) )
        {
        }
        else
        {
        }
        dispIo.tgt.AddCondition("sp-Consecrate Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        v3 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v3);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _consecrate_hit_trigger(): cannot remove target");
          return;
        }
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        *(_DWORD *)&v4[20] = evt.dispIO;
        *(_DWORD *)&v4[16] = 19;
        *(_QWORD *)&v4[8] = *(GameObjectBody *)((string )&evt.objHndCaller + 4);
        *(_QWORD *)v4 = *(_QWORD *)&evt.subDispNode;
        SpellEffects.Spell_remove_mod(in evt);
      }
    }
  }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*  else
  {
    Logger.Info("d20_mods_spells.c / _consecrate_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.TurnBasedStatusInit)]
[TempleDllLocation(0x100ed0b0)]
public static void   StinkingCloudNausea_TurnbasedInit(in DispatcherCallbackArgs evt)
{
  DispIOTurnBasedStatus dispIo;
  int *v2;

  dispIo = evt.GetDispIOTurnBasedStatus();
  if ( dispIo !=null)
  {
    v2 = &dispIo.tbStatus.hourglassState;
    if ( v2 )
    {
      if ( *v2 > 1 )
      {
        *v2 = 1;
      }
    }
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100cad50)]
public static void   sub_100CAD50(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoDamage dispIo;
  int condArg1;
  int v3;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoDamage();
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    v3 = spellPkt.casterLevel / 3;
    if ( spellPkt.casterLevel / 3 >= 15 )
    {
      v3 = 15;
    }
    dispIo.damage.AddDamageBonus(v3, 0, data2);
  }
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100c7330)]
public static void   sub_100C7330(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;

  dispIo = evt.GetDispIoAttackBonus();
  dispIo.bonlist.AddBonus(data1, 17, data2);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cf340)]
public static void   BeginSpellRepelVermin(in DispatcherCallbackArgs evt)
{
  int condArg1;
  float radiusInches;
  int v3;  SpellEntry a2;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
    radiusInches = (float)(int)a2.radiusTarget * 12F;
    spellPkt.aoeObj = evt.objHndCaller;
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 30, 31, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_repel_vermin(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d45e0)]
public static void   ObjEventAoEEntangle(in DispatcherCallbackArgs evt, int data)
{
  DispIoObjEvent dispIo;
  int condArg1;
  string v3;
    GameObjectBody v5;

  int v7;
  GameObjectBody v8;

  int v10;


  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoObjEvent();
  if ( dispIo.evtId == evt.GetConditionArg3() )
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      SpellPktTriggerAoeHitScript/*0x100c37d0*/(spellPkt.spellId);
      if ( D20ModSpells.CheckSpellResistance(&spellPkt, dispIo.tgt) )
      {
        return;
      }
      if ( evt.dispKey == D20DispatcherKey.OnEnterAoE )
      {
        if ( GameSystems.D20.Combat.SavingThrowSpell(dispIo.tgt, spellPkt.caster, spellPkt.dc, SavingThrowType.Reflex, 0, spellPkt.spellId) )
        {
          GameSystems.Spell.FloatSpellLine(dispIo.tgt, 30001, TextFloaterColor.White);
          v8 = dispIo.tgt;
          v10 = GameSystems.ParticleSys.CreateAtObj("sp-Entangle", v8);
          spellPkt.AddTarget(dispIo.tgt, v10, true);
        }
        else
        {
          GameSystems.Spell.FloatSpellLine(dispIo.tgt, 30002, TextFloaterColor.White);
          v5 = dispIo.tgt;
          v7 = GameSystems.ParticleSys.CreateAtObj("sp-Entangle", v5);
          spellPkt.AddTarget(dispIo.tgt, v7, true);
        }
        dispIo.tgt.AddCondition("sp-Entangle On", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        v3 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v3);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _entangle_hit_trigger(): cannot remove target");
          return;
        }
        *(_DWORD *)&v4[20] = evt.dispIO;
        *(_DWORD *)&v4[16] = 19;
        *(_QWORD *)&v4[8] = *(GameObjectBody *)((string )&evt.objHndCaller + 4);
        *(_QWORD *)v4 = *(_QWORD *)&evt.subDispNode;
        SpellEffects.Spell_remove_mod(in evt);
      }
    }
  }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*  else
  {
    Logger.Info("d20_mods_spells.c / _entangle_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.TakingDamage)]
[TempleDllLocation(0x100ca3f0)]
public static void   sub_100CA3F0(in DispatcherCallbackArgs evt, int data)
{
  DispIoDamage dispIo;
  int v2;
  int v3;
  int v4;

  dispIo = evt.GetDispIoDamage();
  v4 = evt.objHndCaller.GetInt32(obj_f.critter_alignment_choice) != 0 ? 8 : 16;
  v2 = evt.objHndCaller.GetStat(Stat.level_cleric);
  v3 = 3;
  if ( v2 >= 12 )
  {
    v3 = 6;
    if ( v2 >= 15 )
    {
      v3 = 9;
    }
  }
  dispIo.damage.AddPhysicalDR(v3, v4, 0x68);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cd4b0)]
public static void   sub_100CD4B0(in DispatcherCallbackArgs evt)
{
  int condArg3;
  int condArg2;
  int v3;
  GameObjectBody v4;

  evt.SetConditionArg4(5);
  switch ( evt.GetConditionArg2() )
  {
    case 1:
      v4 = evt.objHndCaller;
      v3 = 8984;
      goto LABEL_7;
    case 3:
      v4 = evt.objHndCaller;
      v3 = 8986;
      goto LABEL_7;
    case 6:
      v4 = evt.objHndCaller;
      v3 = 8992;
      goto LABEL_7;
    case 9:
      v4 = evt.objHndCaller;
      v3 = 8988;
      goto LABEL_7;
    case 16:
      v4 = evt.objHndCaller;
      v3 = 8990;
LABEL_7:
      GameSystems.SoundGame.PositionalSound(v3, 1, v4);
      condArg3 = evt.GetConditionArg3();
      condArg2 = evt.GetConditionArg2();
      evt.SetConditionArg3(condArg2);
      evt.SetConditionArg2(condArg3);
      break;
    default:
      return;
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100c5b50)]
public static void   sub_100C5B50(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoDamage dispIo;
  GameObjectBody v2;
  GameObjectBody v3;

  dispIo = evt.GetDispIoDamage();
  v2 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, 203);
  v3 = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, 204);
  switch ( data2 )
  {
    case 0xD1:
    case 0xD2:
      if ( (v2 == null)&& (v3 == null))
      {
        goto LABEL_4;
      }
      break;
    default:
LABEL_4:
      AddAttackPowerType/*0x100e0520*/(&dispIo.damage, 4);
      break;
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cea00)]
public static void   BeginSpellMinorGlobeOfInvulnerability(in DispatcherCallbackArgs args)
{
  int condArg1;
  float radiusInches;
  int v3;  SpellEntry a2;
  SpellPacketBody spellPkt;

  condArg1 = args.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
    radiusInches = (float)(int)a2.radiusTarget * 12F;
    spellPkt.aoeObj = args.objHndCaller;
    v3 = GameSystems.ObjectEvent.AddEvent(args.objHndCaller, 22, 23, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    args.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = args.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_minor_globe_of_invulnerability(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.Tooltip)]
[TempleDllLocation(0x100c3630)]
public static void   Tooltip2Callback(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoTooltip dispIo;
  int v2;
  int condArg1;
  int condArg3;
  int v5;
  int v6;
  string meslineValue;
int meslineKey;
  CHAR v8;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoTooltip();
  switch ( data2 )
  {
    case 0xBD:
      v2 = 10;
      condArg1 = evt.GetConditionArg1();
      if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
      {
        if ( spellPkt.casterLevel <= 10 )
        {
          if ( spellPkt.casterLevel > 6 )
          {
            v2 = 20;
          }
        }
        else
        {
          v2 = 30;
        }
      }
      break;
    case 0x58:
    case 0xAE:
    case 0xE0:
      condArg3 = evt.GetConditionArg4();
      goto LABEL_9;
    default:
      condArg3 = evt.GetConditionArg3();
LABEL_9:
      v2 = condArg3;
      break;
  }
  meslineKey = data1;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  v8 = String.Format("{0}{1}", meslineValue, v2);
  v6 = dispIo.numStrings;
  if ( v6 < 10 )
  {
    dispIo.numStrings = v6 + 1;
    strncpy(dispIo.strings[v6].text, &v8, 0x100);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cfa40)]
public static void   SleetStormBeginSpell(in DispatcherCallbackArgs evt)
{  int condArg1;
  int condArg2;/*INLINED:v1=evt.subDispNode*/  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  evt.subDispNode = (SubDispNode *)evt.GetConditionArg3();
  evt.SetConditionArg4(0);
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20040, TextFloaterColor.White);
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20017, TextFloaterColor.Red);
  if ( !evt.objHndCaller.AddCondition("Invisible", condArg1, condArg2, (int)evt.subDispNode) )
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_sleet_storm(): unable to add condition");
  }
}


[DispTypes(DispatcherType.GetAC, DispatcherType.AcModifyByAttacker)]
[TempleDllLocation(0x100c4440)]
public static void   d20_mods_spells__spell__bonus(in DispatcherCallbackArgs evt, int data1, int data2)
{
  int condArg1;
  DispIoAttackBonus dispIo;
  int condArg3;  int v5;
  bool v6;
  bool v7;
  int v8;
  int v9;
  int v10;
  int condArg4;
  int v12;
  string v13;
  int v14;
  int v15;
  SpellPacketBody spellEnum;

  condArg1 = evt.GetConditionArg1();
  dispIo = evt.GetDispIoAttackBonus();
  condArg3 = evt.GetConditionArg3();/*INLINED:v4=evt.subDispNode.subDispDef*/  switch ( data2 )
  {
    case 0xAD:
      v15 = data2;
      v14 = data1;
      condArg3 = -condArg3;
      goto LABEL_38;
    case 0xCD:
    case 0xCE:
      if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellEnum) )
      {
        v5 = spellEnum.spellEnum;
        switch ( spellEnum.spellEnum )
        {
          case WellKnownSpells.MagicCircleAgainstChaos:
            v6 = !(dispIo.attackPacket.attacker.HasChaoticAlignment()) ;
            goto LABEL_18;
          case WellKnownSpells.MagicCircleAgainstEvil:
            v7 = !(dispIo.attackPacket.attacker.HasEvilAlignment()) ;
            goto LABEL_21;
          case WellKnownSpells.MagicCircleAgainstGood:
            if ( !(dispIo.attackPacket.attacker.HasGoodAlignment()) )
            {
              return;
            }
            v8 = data1;
            v15 = data2;
            goto LABEL_37;
          case WellKnownSpells.MagicCircleAgainstLaw:
            v6 = !(dispIo.attackPacket.attacker.HasLawfulAlignment()) ;
            goto LABEL_18;
          default:
            break;
        }
LABEL_34:
        v13 = GameSystems.Spell.GetSpellName(v5);
        Logger.Info("d20_mods_spells.c / _spell_armor_bonus(): invalid spell=( {0} )", v13);
      }
      else
      {
        Logger.Info("d20_mods_spells.c / _spell_armor_bonus(): unable to retrieve spell_packet for spell_id=( {0} )", condArg1);
      }
      return;
    case 0xD5:
      v9 = data2;
      v10 = data1;
      condArg4 = evt.GetConditionArg4();
      dispIo.bonlist.AddBonus(condArg4, v10, v9);
      return;
    case 0xCF:
      if ( !GameSystems.Spell.TryGetActiveSpell(condArg1, out spellEnum) )
      {
        Logger.Info("d20_mods_spells.c / _spell_armor_bonus(): unable to retrieve spell_packet for spell_id=( {0} )", condArg1);
        return;
      }
      v5 = spellEnum.spellEnum;
      switch ( spellEnum.spellEnum )
      {
        case WellKnownSpells.ProtectionFromChaos:
          v7 = !(dispIo.attackPacket.attacker.HasChaoticAlignment()) ;
          goto LABEL_21;
        case WellKnownSpells.ProtectionFromEvil:
          if ( !(dispIo.attackPacket.attacker.HasEvilAlignment()) )
          {
            return;
          }
          v8 = data1;
          v15 = data2;
          goto LABEL_37;
        case WellKnownSpells.ProtectionFromGood:
          v6 = !(dispIo.attackPacket.attacker.HasGoodAlignment()) ;
LABEL_18:
          if ( v6 )
          {
            return;
          }
          v15 = data2;
          v14 = data1;
          goto LABEL_38;
        case WellKnownSpells.ProtectionFromLaw:
          v7 = !(dispIo.attackPacket.attacker.HasLawfulAlignment()) ;
LABEL_21:
          if ( v7 )
          {
            return;
          }
          v15 = data2;
          v14 = data1;
          break;
        default:
          goto LABEL_34;
      }
      goto LABEL_38;
    case 0x108:
      if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellEnum) )
      {
        v5 = spellEnum.spellEnum;
        switch ( spellEnum.spellEnum )
        {
          case WellKnownSpells.PotionOfProtectionFromOutsiders:
            v12 = GameSystems.Critter.IsCategory(dispIo.attackPacket.attacker, MonsterCategory.outsider);
            goto LABEL_32;
          case WellKnownSpells.PotionOfProtectionFromElementals:
            if ( !GameSystems.Critter.IsCategory(dispIo.attackPacket.attacker, MonsterCategory.elemental) )
            {
              return;
            }
            v8 = data1;
            v15 = data2;
            condArg3 = -condArg3;
            goto LABEL_37;
          case WellKnownSpells.PotionOfProtectionFromEarth:
            if ( !GameSystems.Critter.IsCategorySubtype(dispIo.attackPacket.attacker, 256) )
            {
              return;
            }
            v15 = data2;
            v14 = data1;
            condArg3 = -condArg3;
            break;
          case WellKnownSpells.PotionOfProtectionFromUndead:
            v12 = GameSystems.Critter.IsCategory(dispIo.attackPacket.attacker, MonsterCategory.undead);
LABEL_32:
            if ( (v12 )==0)
            {
              return;
            }
            v15 = data2;
            v14 = data1;
            condArg3 = -condArg3;
            break;
          default:
            goto LABEL_34;
        }
LABEL_38:
        dispIo.bonlist.AddBonus(condArg3, v14, v15);
      }
      else
      {
        Logger.Info("d20_mods_spells.c / _spell_resistance_saving_bonus(): unable to retrieve spell_packet for spell_id=( {0} )", condArg1);
      }
      return;
    default:
      v15 = data2;
      v8 = data1;
LABEL_37:
      v14 = v8;
      goto LABEL_38;
  }
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x100d2b00)]
public static void   PotionOfGlibnessSkillLevel(in DispatcherCallbackArgs evt)
{
  DispIoObjBonus dispIo;

  dispIo = evt.GetDispIoObjBonus();
  dispIo.bonOut.AddBonus(30, 0, 113);
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d4c90)]
public static void   GreaseAoeEvent(in DispatcherCallbackArgs evt, int data)
{
  DispIoObjEvent dispIo;
  int condArg1;
  string v3;
    GameObjectBody v5;

  int v7;


  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoObjEvent();
  if ( dispIo.evtId == evt.GetConditionArg3() )
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      SpellPktTriggerAoeHitScript/*0x100c37d0*/(spellPkt.spellId);
      if ( D20ModSpells.CheckSpellResistance(&spellPkt, dispIo.tgt) )
      {
        return;
      }
      if ( evt.dispKey == D20DispatcherKey.OnEnterAoE )
      {
        v5 = dispIo.tgt;
        v7 = GameSystems.ParticleSys.CreateAtObj("sp-Grease-Hit", v5);
        spellPkt.AddTarget(dispIo.tgt, v7, true);
        dispIo.tgt.AddCondition("sp-Grease Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        v3 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v3);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _grease_hit_trigger(): cannot remove target");
          return;
        }
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        *(_DWORD *)&v4[20] = evt.dispIO;
        *(_DWORD *)&v4[16] = 19;
        *(_QWORD *)&v4[8] = *(GameObjectBody *)((string )&evt.objHndCaller + 4);
        *(_QWORD *)v4 = *(_QWORD *)&evt.subDispNode;
        SpellEffects.Spell_remove_mod(in evt);
      }
    }
  }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*  else
  {
    Logger.Info("d20_mods_spells.c / _grease_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.GetMoveSpeed)]
[TempleDllLocation(0x100cabe0)]
public static void   sub_100CABE0(in DispatcherCallbackArgs evt, ConditionSpec data)
{
  DispIoMoveSpeed dispIo;

  dispIo = evt.GetDispIoMoveSpeed();
  dispIo.factor = dispIo.factor * 0.33F;
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d4050)]
public static void   ObjEventAoEDesecrate(in DispatcherCallbackArgs evt, int data)
{
  DispIoObjEvent dispIo;
  int condArg1;
  string v3;


  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoObjEvent();
  if ( dispIo.evtId == evt.GetConditionArg3() )
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      SpellPktTriggerAoeHitScript/*0x100c37d0*/(spellPkt.spellId);
      if ( D20ModSpells.CheckSpellResistance(&spellPkt, dispIo.tgt) )
      {
        return;
      }
      if ( evt.dispKey == D20DispatcherKey.OnEnterAoE )
      {
        spellPkt.AddTarget(dispIo.tgt, 0, true);
        if ( GameSystems.Critter.IsCategory(dispIo.tgt, MonsterCategory.undead) )
        {
        }
        else
        {
        }
        dispIo.tgt.AddCondition("sp-Desecrate Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        v3 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v3);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _desecrate_hit_trigger(): cannot remove target");
          return;
        }
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        *(_DWORD *)&v4[20] = evt.dispIO;
        *(_DWORD *)&v4[16] = 19;
        *(_QWORD *)&v4[8] = *(GameObjectBody *)((string )&evt.objHndCaller + 4);
        *(_QWORD *)v4 = *(_QWORD *)&evt.subDispNode;
        SpellEffects.Spell_remove_mod(in evt);
      }
    }
  }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*  else
  {
    Logger.Info("d20_mods_spells.c / _desecrate_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100d2b70)]
public static void   sub_100D2B70(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  DispIoAttackBonus dispIo;

  v1 = data;
  dispIo = evt.GetDispIoAttackBonus();
  dispIo.bonlist.AddBonus(v1, 34, 113);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ccd80)]
public static void   sub_100CCD80(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg2;
  int condArg3;



  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  condArg3 = evt.GetConditionArg3();
  if ( !evt.objHndCaller.AddCondition("Charmed", condArg1, condArg2, condArg3) )
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_control_plants_charmed(): unable to add condition");
  }
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20018, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.D20Signal, DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100d7620)]
[TemplePlusLocation("spell_condition.cpp:108")]
public static void   Spell_remove_spell(in DispatcherCallbackArgs evt, int data1, int data2)
{
  SubDispNode *sdn;
  DispIoD20Signal dispIo;
  Disp_Key v3;
  int condArg1;
  string n32Data;
  string v6;
  int v7;
  string v8;
  int v9;
  int v10;
  double v11;
  string v12;
  GameObjectBody v13;

  int i;
  GameObjectBody v16;
  int v17;
  string v18;
  int v19;
  int v20;
  string partsysId;
  GameObjectBody v22;

  int j;
  GameObjectBody v25;
  string v26;
  int removedObjSuccess;

  string v29;
  string v30;

  string v32;
  GameObjectBody *v33;
  GameObjectBody ST40_8_66;
  string v35;
  string v36;
  GameObjectBody *v37;
  GameObjectBody ST40_8_75;
  string v39;
  bool v40;
  byte v41;
  GameObjectBody *v42;
  GameObjectBody ST40_8_82;
  string v44;
  GameObjectBody *v45;
  GameObjectBody ST40_8_87;
  string v47;
  int v48;

  string v50;
  int k;
  string v52;





  int v58;
  int v59;
  GameObjectBody *v60;
  GameObjectBody v61;
  string v62;
  int v63;
  GameObjectBody *v64;
  GameObjectBody v65;
  string v66;
  GameObjectBody *v67;
  GameObjectBody v68;
  string v69;
  GameObjectBody *v70;
  GameObjectBody v71;
  string v72;
  int v73;
  string v74;
  GameObjectBody *v75;
  GameObjectBody v76;
  string v77;

  GameObjectBody *v79;
  GameObjectBody v80;
  string v81;
  GameObjectBody *v82;
  GameObjectBody v83;
  string v84;
  int v85;
  GameObjectBody v86;
  string v87;
  GameObjectBody v88;
  string v89;
  string v90;
  GameObjectBody v91;

  string v93;
  GameObjectBody *v94;
  GameObjectBody v95;
  string v96;
  int v97;
  GameObjectBody *v98;
  GameObjectBody v99;
  string v100;
  GameObjectBody *v101;
  GameObjectBody v102;
  string v103;





  string v109;
  GameObjectBody v110;

  string v112;
  int v113;
  int v114;
  int v115;
  int v116;

  GameObjectBody *v118;
  GameObjectBody v119;
  string v120;
  int v121;





  int v127;
  int v128;
  string v129;


  int v132;
  GameObjectBody *v133;
  GameObjectBody v134;
  string v135;
  int v136;
  GameObjectBody *v137;
  GameObjectBody v138;
  string v139;
    GameObjectBody *v141;
  GameObjectBody v142;
  string v143;
  int v144;
  GameObjectBody *v145;
  GameObjectBody v146;
  string v147;
  int v148;
  GameObjectBody *v149;
  GameObjectBody v150;
  string v151;
  GameObjectBody *v152;
  GameObjectBody v153;
  string v154;
  int v155;
  GameObjectBody *v156;
  GameObjectBody v157;
  string v158;
  int v159;
  int v160;
  string v161;
  string v162;
  int v163;
  GameObjectBody v164;
  GameObjectBody *v165;
  GameObjectBody v166;
  string v167;
  int v168;
    string v170;
  GameObjectBody *v171;
  GameObjectBody v172;
  string v173;
  int v174;
  string v175;
  int v176;
        int v180;
  GameObjectBody v181;
  GameObjectBody v182;
  int v183;
  int v184;
  GameObjectBody v185;
  Disp_Key v186;
  Disp_Key v187;
  GameObjectBody v188;
  GameObjectBody v189;
  int v190;
  int v191;
  ConditionAttachment v192;
  GameObjectBody v194;
  GameObjectBody v196;
  GameObjectBody v197;
  GameObjectBody v198;
  int v199;
  int v200;
  int v201;
  int v202;
  int v203;
  int v204;
  int v205;
  int v206;
  int v207;
  int v208;
  int v209;
  int v210;
  int v211;
  int v212;
  int v213;
  int v214;
  int v215;
  int v216;
  int v217;
  int v218;
  int v219;
  int v220;
  int v221;
  int v222;
  int v223;
  int v224;
  int v225;
  int v226;
  int v227;
  int v228;
  int v229;
  int v230;
  int v231;
  GameObjectBody parent;
  SpellPacketBody spellPkt;

  sdn = evt.subDispNode;
  dispIo = 0;
  v3 = evt.dispKey;
  if ( (evt.dispIO == null)|| evt.dispType != DispatcherType.D20Signal )
  {
    goto LABEL_342;
  }
  dispIo = evt.GetDispIoD20Signal();
  if ( evt.dispKey == D20DispatcherKey.SIG_Killed )
  {
    goto LABEL_17;
  }
  if ( evt.dispKey != D20DispatcherKey.SIG_Sequence )
  {
LABEL_342:
    if ( evt.dispKey != D20DispatcherKey.SIG_Killed
      && evt.dispKey != D20DispatcherKey.SIG_Critter_Killed
      && evt.dispKey != D20DispatcherKey.SIG_Sequence
      && evt.dispKey != D20DispatcherKey.SIG_Spell_Cast
      && evt.dispKey != D20DispatcherKey.SIG_Concentration_Broken
      && evt.dispKey != D20DispatcherKey.SIG_Action_Recipient
      && evt.dispKey != D20DispatcherKey.SIG_TouchAttackAdded
      && evt.dispKey != D20DispatcherKey.SIG_Teleport_Prepare
      && evt.dispKey != D20DispatcherKey.SIG_Teleport_Reconnect
      && dispIo
!=null      && dispIo.data1 != evt.GetConditionArg1() )
    {
      return;
    }
    goto LABEL_17;
  }
  Logger.Info("d20_mods_spells.c / _remove_spell: [WARNING:] caught a D20est_S_Sequence, make sure we are removing spell properly...");
LABEL_17:
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    n32Data = GameSystems.MapObject.GetDisplayName(evt.objHndCaller, evt.objHndCaller);
    v6 = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
    Logger.Info("d20_mods_spells.c / _remove_spell(): removing spell=( {0} ) on obj=( {1} )", v6, n32Data);
    switch ( spellPkt.spellEnum )
    {
      case WellKnownSpells.Aid:
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        End_Particles_And_Send_Spell_End_Signal/*0x100d09d0*/(evt.objHndCaller);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.AnimalFriendship:
        v7 = RemoveAnimalFriendship/*0x100d0a40*/(evt.dispIO, evt.subDispNode, evt.objHndCaller);
        goto LABEL_22;
      case WellKnownSpells.AnimalGrowth:
        LODWORD(parent) = evt.objHndCaller.GetInt32(obj_f.model_scale);
        evt.objHndCaller.SetInt32(obj_f.model_scale, (ulong)((float)(int)parent * 0.5555556F));
        GameSystems.Critter.UpdateModelEquipment(evt.objHndCaller);
        v11 = evt.objHndCaller.GetFloat(obj_f.speed_run);
        goto LABEL_233;
      case WellKnownSpells.AnimalTrance:
        if ( RemoveAnimalTrance/*0x100d0ca0*/(evt.dispIO) != 1 )
        {
          return;
        }
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v12 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v12);
        GameSystems.ParticleSys.End((string )spellPkt.casterPartsysId);
        v13 = spellPkt.caster;
        GameSystems.ParticleSys.CreateAtObj("sp-Animal Trance-END", v13);
        if ( evt.dispKey == D20DispatcherKey.SIG_Concentration_Broken )
        {
          GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          sdn = evt.subDispNode;
        }
        else
        {
          for ( i = 0; i < spellPkt.targetCount; ++i )
          {
            HIDWORD(v16) = HIDWORD(spellPkt.targetListHandles[i]);
            LODWORD(v16) = spellPkt.targetListHandles[i];
            GameSystems.D20.D20SendSignal(v16, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          }
          sdn = evt.subDispNode;
          v17 = evt.GetConditionArg1();
          GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Remove_Concentration, v17, 0);
        }
        v18 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v18);
        v19 = spellPkt.RemoveTarget(evt.objHndCaller);
        goto LABEL_33;
      case WellKnownSpells.BlindnessDeafness:
        condArg1 = spellPkt.spellId;
        v185 = spellPkt.caster;
        goto LABEL_212;
      case WellKnownSpells.BullsStrength:
        v194 = evt.objHndCaller;
        goto LABEL_238;
      case WellKnownSpells.CalmAnimals:
        v7 = RemoveSpellCalmAnimals/*0x100d0db0*/(evt.dispIO, evt.subDispNode, evt.objHndCaller);
LABEL_22:
        if ( v7 != 1 )
        {
          return;
        }
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v8 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v8);
        v9 = spellPkt.RemoveTarget(evt.objHndCaller);
        goto LABEL_24;
      case WellKnownSpells.CalmEmotions:
        if ( RemoveSpellCalmEmotions/*0x100d1030*/(
               (DispIoD20Signal )evt.dispIO,
               evt.dispType,
               evt.dispKey,
               evt.subDispNode,
               evt.objHndCaller) != 1 )
        {
          return;
        }
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        partsysId = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(partsysId);
        GameSystems.ParticleSys.End((string )spellPkt.casterPartsysId);
        v22 = spellPkt.caster;
        GameSystems.ParticleSys.CreateAtObj("sp-Calm Emotions-END", v22);
        for ( j = 0; j < spellPkt.targetCount; ++j )
        {
          HIDWORD(v25) = HIDWORD(spellPkt.targetListHandles[j]);
          LODWORD(v25) = spellPkt.targetListHandles[j];
          GameSystems.D20.D20SendSignal(v25, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        }
        sdn = evt.subDispNode;
        v190 = evt.GetConditionArg1();
        v186 = D20DispatcherKey.SIG_Remove_Concentration;
        v181 = spellPkt.caster;
        goto LABEL_44;
      case WellKnownSpells.CatsGrace:
        v196 = evt.objHndCaller;
        goto LABEL_50;
      case WellKnownSpells.CauseFear:
      case WellKnownSpells.Fear:
      case WellKnownSpells.Scare:
        if ( evt.objHndCaller.IsNPC() )
        {
          GameSystems.AI.StopFleeing(evt.objHndCaller);
        }
        goto LABEL_211;
      case WellKnownSpells.CharmMonster:
        if ( RemoveSpellCharmMonster/*0x100d1170*/(evt.objHndCaller, HIDWORD(evt.objHndCaller), evt.dispType, evt.dispKey) != 1 )
        {
          return;
        }
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        v29 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v29);
        v9 = spellPkt.RemoveTarget(evt.objHndCaller);
        goto LABEL_24;
      case WellKnownSpells.CharmPerson:
        if ( RemoveSpellCharmPerson/*0x100d13f0*/(evt.objHndCaller, HIDWORD(evt.objHndCaller), evt.dispType, evt.dispKey) != 1 )
        {
          return;
        }
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v191 = spellPkt.spellId;
        v187 = 159;
        v182 = evt.objHndCaller;
        goto LABEL_105;
      case WellKnownSpells.CharmPersonOrAnimal:
        if ( RemoveSpellCharmPersonOrAnimal/*0x100d1670*/(evt.objHndCaller, HIDWORD(evt.objHndCaller), evt.dispType, evt.dispKey) != 1 )
        {
          return;
        }
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v190 = spellPkt.spellId;
        v186 = 159;
        v181 = evt.objHndCaller;
LABEL_44:
        GameSystems.D20.D20SendSignal(v181, v186, v190, 0);
        v26 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v26);
        removedObjSuccess = spellPkt.RemoveTarget(evt.objHndCaller);
        goto LABEL_45;
      case WellKnownSpells.ChillMetal:
        v196 = evt.objHndCaller;
        goto LABEL_50;
      case WellKnownSpells.ChillTouch:
        if ( GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Afraid) && evt.objHndCaller.IsNPC() )
        {
          GameSystems.AI.StopFleeing(evt.objHndCaller);
        }
        v30 = (string )spellPkt.GetPartSysForTarget(spellPkt.caster);
        GameSystems.ParticleSys.End(v30);
        GameSystems.ParticleSys.End((string )spellPkt.casterPartsysId);
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.ClairaudienceClairvoyance:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.MapFogging.Enable();
        GameSystems.ParticleSys.CreateAtObj("sp-Clairaudience-Clairvoyance-END", evt.objHndCaller);
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        v32 = (string )evt.GetConditionArg3();
        GameSystems.ParticleSys.End(v32);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.Cloudkill:
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v199 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_285;
        }
        while ( 1 )
        {
          v33 = &spellPkt.targetListHandles[v199];
          HIDWORD(ST40_8_66) = HIDWORD(spellPkt.targetListHandles[v199]);
          LODWORD(ST40_8_66) = spellPkt.targetListHandles[v199];
          v35 = (string )spellPkt.GetPartSysForTarget(ST40_8_66);
          GameSystems.ParticleSys.End(v35);
          GameSystems.D20.D20SendSignal(*v33, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v33) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v199 < 0 )
          {
            goto LABEL_284;
          }
        }
      case WellKnownSpells.ColorSpray:
        if ( sub_100D18F0/*0x100d18f0*/(evt.dispKey, evt.dispType, evt.objHndCaller, SHIDWORD(evt.objHndCaller)) != 1 )
        {
          return;
        }
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v36 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v36);
        removedObjSuccess = spellPkt.RemoveTarget(evt.objHndCaller);
        goto LABEL_45;
      case WellKnownSpells.Command:
        if ( evt.GetConditionArg3() == 3 )
        {
          GameSystems.AI.StopFleeing(evt.objHndCaller);
        }
        goto Send_Signal_Spell_End__End_Particles_For_Caller;
      case WellKnownSpells.AnimateDead:
      case WellKnownSpells.DimensionalAnchor:
      case WellKnownSpells.DiscernLies:
      case WellKnownSpells.Endurance:
      case WellKnownSpells.FindTraps:
      case WellKnownSpells.Flare:
      case WellKnownSpells.Haste:
      case WellKnownSpells.HolySmite:
      case WellKnownSpells.Identify:
      case WellKnownSpells.NegativeEnergyProtection:
      case WellKnownSpells.NeutralizePoison:
      case WellKnownSpells.RaiseDead:
      case WellKnownSpells.RemoveCurse:
      case WellKnownSpells.RemoveDisease:
      case WellKnownSpells.RemoveFear:
      case WellKnownSpells.SoundBurst:
      case WellKnownSpells.SpellResistance:
      case WellKnownSpells.Heroism:
      case WellKnownSpells.Heal:
      case WellKnownSpells.Harm2:
      case WellKnownSpells.BootsOfSpeed:
Send_Signal_Spell_End__End_Particles_For_Caller:
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.Consecrate:
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v200 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_332;
        }
        while ( 1 )
        {
          v37 = &spellPkt.targetListHandles[v200];
          HIDWORD(ST40_8_75) = HIDWORD(spellPkt.targetListHandles[v200]);
          LODWORD(ST40_8_75) = spellPkt.targetListHandles[v200];
          v39 = (string )spellPkt.GetPartSysForTarget(ST40_8_75);
          GameSystems.ParticleSys.End(v39);
          GameSystems.D20.D20SendSignal(*v37, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v37) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v200 < 0 )
          {
            goto LABEL_331;
          }
        }
      case WellKnownSpells.ControlPlants:
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v201 = 0;
        if ( spellPkt.numSpellObjs > 0 )
        {
          LODWORD(parent) = (string )&spellPkt + 64;
          do
          {
            GameSystems.ParticleSys.End(*(string *)parent);
            v41 = __OFSUB__(v201 + 1, spellPkt.numSpellObjs);
            v40 = v201++ + 1 - spellPkt.numSpellObjs < 0;
            LODWORD(parent) = parent + 16;
          }
          while ( v40 ^ v41 );
        }
        v202 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_285;
        }
        while ( 1 )
        {
          v42 = &spellPkt.targetListHandles[v202];
          HIDWORD(ST40_8_82) = HIDWORD(spellPkt.targetListHandles[v202]);
          LODWORD(ST40_8_82) = spellPkt.targetListHandles[v202];
          v44 = (string )spellPkt.GetPartSysForTarget(ST40_8_82);
          GameSystems.ParticleSys.End(v44);
          GameSystems.D20.D20SendSignal(*v42, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v42) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v202 < 0 )
          {
            goto LABEL_284;
          }
        }
      case WellKnownSpells.DeathKnell:
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        End_Particles_And_Send_Spell_End_Signal/*0x100d09d0*/(evt.objHndCaller);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.Desecrate:
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v203 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_90;
        }
        while ( 1 )
        {
          v45 = &spellPkt.targetListHandles[v203];
          HIDWORD(ST40_8_87) = HIDWORD(spellPkt.targetListHandles[v203]);
          LODWORD(ST40_8_87) = spellPkt.targetListHandles[v203];
          v47 = (string )spellPkt.GetPartSysForTarget(ST40_8_87);
          GameSystems.ParticleSys.End(v47);
          GameSystems.D20.D20SendSignal(*v45, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v45) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v203 < 0 )
          {
            v3 = evt.dispKey;
LABEL_90:
            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
            v48 = evt.GetConditionArg1();
            GameSystems.Spell.EndSpell(v48, 0);
            *(_QWORD *)&v178[16] = __PAIR__((int)evt.dispIO, v3);
            goto LABEL_35;
          }
        }
      case WellKnownSpells.DispelAir:
        v197 = evt.objHndCaller;
        goto LABEL_92;
      case WellKnownSpells.DispelEarth:
        v197 = evt.objHndCaller;
        goto LABEL_92;
      case WellKnownSpells.DispelFire:
        v197 = evt.objHndCaller;
        goto LABEL_92;
      case WellKnownSpells.DispelWater:
        v197 = evt.objHndCaller;
        goto LABEL_92;
      case WellKnownSpells.DispelChaos:
        v197 = evt.objHndCaller;
        goto LABEL_92;
      case WellKnownSpells.DispelEvil:
        v197 = evt.objHndCaller;
        goto LABEL_92;
      case WellKnownSpells.DispelGood:
        v197 = evt.objHndCaller;
        goto LABEL_92;
      case WellKnownSpells.DispelLaw:
        v197 = evt.objHndCaller;
LABEL_92:
        GameSystems.ParticleSys.CreateAtObj("sp-Dispel Law-END", v197);
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.DominateAnimal:
      case WellKnownSpells.DominatePerson:
        GameSystems.Critter.RemoveFollower(evt.objHndCaller, 1);
        GameUiBridge.UpdatePartyUi();
        goto LABEL_239;
      case WellKnownSpells.Emotion:
        if ( sub_100D19A0/*0x100d19a0*/(evt.subDispNode, evt.objHndCaller, HIDWORD(evt.objHndCaller)) != 1 )
        {
          return;
        }
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v50 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v50);
        GameSystems.ParticleSys.End((string )spellPkt.casterPartsysId);
        for ( k = 0; k < spellPkt.targetCount; ++k )
        {
          GameSystems.D20.D20SendSignal(__PAIR__(HIDWORD(spellPkt.targetListHandles[k]), spellPkt.targetListHandles[k]), D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        }
        v191 = evt.GetConditionArg1();
        v187 = 156;
        v182 = spellPkt.caster;
LABEL_105:
        GameSystems.D20.D20SendSignal(v182, v187, v191, 0);
        v52 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v52);
        v19 = spellPkt.RemoveTarget(evt.objHndCaller);
        goto LABEL_33;
      case WellKnownSpells.EndureElements:
        switch ( evt.GetConditionArg3() )
        {
          case 1:
            GameSystems.ParticleSys.CreateAtObj("sp-Endure Elements-acid-END", evt.objHndCaller);
            v188 = evt.objHndCaller;
            v183 = 8985;
            goto LABEL_210;
          case 3:
            GameSystems.ParticleSys.CreateAtObj("sp-Endure Elements-cold-END", evt.objHndCaller);
            v188 = evt.objHndCaller;
            v183 = 8987;
            goto LABEL_210;
          case 6:
            GameSystems.ParticleSys.CreateAtObj("sp-Endure Elements-water-END", evt.objHndCaller);
            v188 = evt.objHndCaller;
            v183 = 8993;
            goto LABEL_210;
          case 9:
            GameSystems.ParticleSys.CreateAtObj("sp-Endure Elements-fire-END", evt.objHndCaller);
            v188 = evt.objHndCaller;
            v183 = 8989;
            goto LABEL_210;
          case 16:
            GameSystems.ParticleSys.CreateAtObj("sp-Endure Elements-Sonic-END", evt.objHndCaller);
            v188 = evt.objHndCaller;
            v183 = 8991;
            goto LABEL_210;
          default:
            return;
        }
        return;
      case WellKnownSpells.Enlarge:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        LODWORD(parent) = evt.objHndCaller.GetInt32(obj_f.model_scale);
        evt.objHndCaller.SetInt32(obj_f.model_scale, (ulong)((float)(int)parent * 0.5555556F));
        GameSystems.Critter.UpdateModelEquipment(evt.objHndCaller);
        *(float *)&v58 = evt.objHndCaller.GetFloat(obj_f.speed_run) * 1.8F;
        evt.objHndCaller.SetInt32(obj_f.speed_run, v58);
        *(float *)&v59 = evt.objHndCaller.GetFloat(obj_f.speed_walk) * 1.8F;
        evt.objHndCaller.SetInt32(obj_f.speed_walk, v59);
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.EntropicShield:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        v196 = evt.objHndCaller;
LABEL_50:
        GameSystems.ParticleSys.CreateAtObj("sp-Entropic Shield-END", v196);
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.Entangle:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v204 = 0;
        if ( spellPkt.numSpellObjs > 0 )
        {
          LODWORD(parent) = (string )&spellPkt + 64;
          do
          {
            GameSystems.ParticleSys.End(*(string *)parent);
            v41 = __OFSUB__(v204 + 1, spellPkt.numSpellObjs);
            v40 = v204++ + 1 - spellPkt.numSpellObjs < 0;
            LODWORD(parent) = parent + 16;
          }
          while ( v40 ^ v41 );
        }
        v205 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_121;
        }
        while ( 1 )
        {
          v60 = &spellPkt.targetListHandles[v205];
          HIDWORD(v61) = HIDWORD(spellPkt.targetListHandles[v205]);
          LODWORD(v61) = spellPkt.targetListHandles[v205];
          v62 = (string )spellPkt.GetPartSysForTarget(v61);
          GameSystems.ParticleSys.End(v62);
          GameSystems.D20.D20SendSignal(*v60, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v60) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v205 < 0 )
          {
            v3 = evt.dispKey;
LABEL_121:
            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
            v63 = evt.GetConditionArg1();
            GameSystems.Spell.EndSpell(v63, 0);
            *(_QWORD *)&v178[16] = __PAIR__((int)evt.dispIO, v3);
            goto LABEL_35;
          }
        }
      case WellKnownSpells.FogCloud:
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v206 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_332;
        }
        while ( 1 )
        {
          v64 = &spellPkt.targetListHandles[v206];
          HIDWORD(v65) = HIDWORD(spellPkt.targetListHandles[v206]);
          LODWORD(v65) = spellPkt.targetListHandles[v206];
          v66 = (string )spellPkt.GetPartSysForTarget(v65);
          GameSystems.ParticleSys.End(v66);
          GameSystems.D20.D20SendSignal(*v64, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v64) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v206 < 0 )
          {
            goto LABEL_331;
          }
        }
      case WellKnownSpells.GaseousForm:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        v194 = evt.objHndCaller;
        goto LABEL_238;
      case WellKnownSpells.GhoulTouch:
        if ( data1 == 108 || data2 == 108 )
        {
          GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        }
        else
        {
          GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        }
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v207 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_285;
        }
        while ( 1 )
        {
          v67 = &spellPkt.targetListHandles[v207];
          HIDWORD(v68) = HIDWORD(spellPkt.targetListHandles[v207]);
          LODWORD(v68) = spellPkt.targetListHandles[v207];
          v69 = (string )spellPkt.GetPartSysForTarget(v68);
          GameSystems.ParticleSys.End(v69);
          GameSystems.D20.D20SendSignal(*v67, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v67) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v207 < 0 )
          {
            goto LABEL_284;
          }
        }
      case WellKnownSpells.Grease:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v208 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_139;
        }
        while ( 1 )
        {
          v70 = &spellPkt.targetListHandles[v208];
          HIDWORD(v71) = HIDWORD(spellPkt.targetListHandles[v208]);
          LODWORD(v71) = spellPkt.targetListHandles[v208];
          v72 = (string )spellPkt.GetPartSysForTarget(v71);
          GameSystems.ParticleSys.End(v72);
          GameSystems.D20.D20SendSignal(*v70, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v70) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v208 < 0 )
          {
            v3 = evt.dispKey;
LABEL_139:
            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
            v73 = evt.GetConditionArg1();
            GameSystems.Spell.EndSpell(v73, 0);
            *(_QWORD *)&v178[16] = __PAIR__((int)evt.dispIO, v3);
            goto LABEL_35;
          }
        }
      case WellKnownSpells.FalseLife:
      case WellKnownSpells.GreaterHeroism:
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        End_Particles_And_Send_Spell_End_Signal/*0x100d09d0*/(evt.objHndCaller);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.GreaterMagicFang:
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        sub_100D1A50/*0x100d1a50*/(evt.objHndCaller);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.GreaterMagicWeapon:
        parent = null;
        GameSystems.Item.GetParent(evt.objHndCaller, &parent);
        "Weapon Enhancement Bonus"/*ELFHASH*/;
        sub_100D3020/*0x100d3020*/(evt.objHndCaller, HIDWORD(evt.objHndCaller));
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        goto LABEL_182;
      case WellKnownSpells.HaltUndead:
        if ( sub_100D1AC0/*0x100d1ac0*/(evt.subDispNode, evt.objHndCaller, HIDWORD(evt.objHndCaller), evt.dispKey) != 1 )
        {
          return;
        }
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v74 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v74);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          goto CANNOT_END_SPELL___RET0;
        }
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        goto LABEL_34;
      case WellKnownSpells.HeatMetal:
        v198 = evt.objHndCaller;
        goto LABEL_218;
      case WellKnownSpells.DetectChaos:
      case WellKnownSpells.DetectEvil:
      case WellKnownSpells.DetectGood:
      case WellKnownSpells.DetectLaw:
      case WellKnownSpells.DetectMagic:
      case WellKnownSpells.DetectSecretDoors:
      case WellKnownSpells.DetectUndead:
      case WellKnownSpells.ExpeditiousRetreat:
      case WellKnownSpells.FaerieFire:
      case WellKnownSpells.FireShield:
      case WellKnownSpells.HoldAnimal:
      case WellKnownSpells.HoldMonster:
      case WellKnownSpells.HoldPerson:
      case WellKnownSpells.HoldPortal:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.ImprovedInvisibility:
      case WellKnownSpells.DustOfDisappearance:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v19 = spellPkt.RemoveTarget(evt.objHndCaller);
        goto LABEL_33;
      case WellKnownSpells.IceStorm:
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v209 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_332;
        }
        while ( 1 )
        {
          v75 = &spellPkt.targetListHandles[v209];
          HIDWORD(v76) = HIDWORD(spellPkt.targetListHandles[v209]);
          LODWORD(v76) = spellPkt.targetListHandles[v209];
          v77 = (string )spellPkt.GetPartSysForTarget(v76);
          GameSystems.ParticleSys.End(v77);
          GameSystems.D20.D20SendSignal(*v75, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v75) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v209 < 0 )
          {
            goto LABEL_331;
          }
        }
      case WellKnownSpells.InvisibilityPurge:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        GameSystems.ParticleSys.CreateAtObj("sp-Invisibility Purge-END", evt.objHndCaller);
        v210 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_285;
        }
        while ( 1 )
        {
          v79 = &spellPkt.targetListHandles[v210];
          HIDWORD(v80) = HIDWORD(spellPkt.targetListHandles[v210]);
          LODWORD(v80) = spellPkt.targetListHandles[v210];
          v81 = (string )spellPkt.GetPartSysForTarget(v80);
          GameSystems.ParticleSys.End(v81);
          GameSystems.D20.D20SendSignal(*v79, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v79) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v210 < 0 )
          {
            goto LABEL_284;
          }
        }
      case WellKnownSpells.InvisibilitySphere:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v211 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_161;
        }
        while ( 1 )
        {
          v82 = &spellPkt.targetListHandles[v211];
          HIDWORD(v83) = HIDWORD(spellPkt.targetListHandles[v211]);
          LODWORD(v83) = spellPkt.targetListHandles[v211];
          v84 = (string )spellPkt.GetPartSysForTarget(v83);
          GameSystems.ParticleSys.End(v84);
          GameSystems.D20.D20SendSignal(*v82, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v82) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v211 < 0 )
          {
            v3 = evt.dispKey;
LABEL_161:
            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
            v85 = evt.GetConditionArg1();
            GameSystems.Spell.EndSpell(v85, 0);
            *(_QWORD *)&v178[16] = __PAIR__((int)evt.dispIO, v3);
            goto LABEL_35;
          }
        }
      case WellKnownSpells.InvisibilityToAnimals:
        if ( RemoveInvisibility/*0x100d1b00*/(evt.dispIO, evt.dispKey, evt.dispType, evt.subDispNode, evt.objHndCaller) != 1 )
        {
          return;
        }
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        v212 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_333;
        }
        while ( 1 )
        {
          HIDWORD(v86) = HIDWORD(spellPkt.targetListHandles[v212]);
          LODWORD(v86) = spellPkt.targetListHandles[v212];
          v87 = (string )spellPkt.GetPartSysForTarget(v86);
          GameSystems.ParticleSys.End(v87);
          GameSystems.D20.D20SendSignal(spellPkt.targetListHandles[v212], D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v212 < 0 )
          {
            v3 = evt.dispKey;
            goto LABEL_333;
          }
        }
      case WellKnownSpells.InvisibilityToUndead:
        if ( RemoveInvisibility/*0x100d1b00*/(evt.dispIO, evt.dispKey, evt.dispType, evt.subDispNode, evt.objHndCaller) != 1 )
        {
          return;
        }
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        v213 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_286;
        }
        while ( 1 )
        {
          HIDWORD(v88) = HIDWORD(spellPkt.targetListHandles[v213]);
          LODWORD(v88) = spellPkt.targetListHandles[v213];
          v89 = (string )spellPkt.GetPartSysForTarget(v88);
          GameSystems.ParticleSys.End(v89);
          GameSystems.D20.D20SendSignal(spellPkt.targetListHandles[v213], D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v213 < 0 )
          {
            v3 = evt.dispKey;
            goto LABEL_286;
          }
        }
      case WellKnownSpells.Blink:
      case WellKnownSpells.Invisibility:
        if ( RemoveInvisibility/*0x100d1b00*/(evt.dispIO, evt.dispKey, evt.dispType, evt.subDispNode, evt.objHndCaller) != 1 )
        {
          return;
        }
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        v9 = spellPkt.RemoveTarget(evt.objHndCaller);
        goto LABEL_24;
      case WellKnownSpells.PotionOfProtectionFromOutsiders:
      case WellKnownSpells.PotionOfProtectionFromElementals:
      case WellKnownSpells.PotionOfProtectionFromEarth:
      case WellKnownSpells.PotionOfProtectionFromMagic:
      case WellKnownSpells.PotionOfProtectionFromUndead:
      case WellKnownSpells.SummonBalor:
      case WellKnownSpells.SummonGlabrezu:
      case WellKnownSpells.SummonHezrou:
      case WellKnownSpells.SummonVrock:
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.KeenEdge:
        parent = null;
        GameSystems.Item.GetParent(evt.objHndCaller, &parent);
        "Weapon Keen"/*ELFHASH*/;
        sub_100D3020/*0x100d3020*/(evt.objHndCaller, HIDWORD(evt.objHndCaller));
        v180 = spellPkt.spellId;
        goto LABEL_181;
      case WellKnownSpells.MageArmor:
      case WellKnownSpells.Glibness:
      case WellKnownSpells.Longstrider:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.MagicFang:
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        sub_100D1D60/*0x100d1d60*/(evt.objHndCaller);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.MagicVestment:
        parent = null;
        GameSystems.Item.GetParent(evt.objHndCaller, &parent);
        "Armor Enhancement Bonus"/*ELFHASH*/;
        sub_100D3020/*0x100d3020*/(evt.objHndCaller, HIDWORD(evt.objHndCaller));
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        if ( parent !=null)
        {
          GameSystems.D20.Status.initItemConditions(parent);
        }
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.MagicWeapon:
        parent = null;
        GameSystems.Item.GetParent(evt.objHndCaller, &parent);
        "Weapon Enhancement Bonus"/*ELFHASH*/;
        sub_100D3020/*0x100d3020*/(evt.objHndCaller, HIDWORD(evt.objHndCaller));
        v180 = spellPkt.spellId;
LABEL_181:
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, v180, 0);
LABEL_182:
        if ( parent !=null)
        {
          GameSystems.D20.Status.initItemConditions(parent);
        }
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.MagicCircleAgainstChaos:
      case WellKnownSpells.MagicCircleAgainstEvil:
      case WellKnownSpells.MagicCircleAgainstGood:
      case WellKnownSpells.MagicCircleAgainstLaw:
        sub_100D1DD0/*0x100d1dd0*/(evt.objHndCaller);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.MeldIntoStone:
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v90 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v90);
        GameSystems.ParticleSys.End((string )spellPkt.casterPartsysId);
        v91 = spellPkt.caster;
        GameSystems.ParticleSys.CreateAtObj("sp-Meld Into Stone-END", v91);
        v93 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v93);
        removedObjSuccess = spellPkt.RemoveTarget(evt.objHndCaller);
LABEL_45:
        if ( removedObjSuccess != 1 )
        {
          goto CANNOT_END_SPELL___RET0;
        }
        condArg1 = CondNodeGetArg/*0x100e1ab0*/(sdn.condNode, 0);
        goto LABEL_47;
      case WellKnownSpells.LesserGlobeOfInvulnerability:
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v214 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_190;
        }
        while ( 1 )
        {
          v94 = &spellPkt.targetListHandles[v214];
          HIDWORD(v95) = HIDWORD(spellPkt.targetListHandles[v214]);
          LODWORD(v95) = spellPkt.targetListHandles[v214];
          v96 = (string )spellPkt.GetPartSysForTarget(v95);
          GameSystems.ParticleSys.End(v96);
          GameSystems.D20.D20SendSignal(*v94, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v94) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v214 < 0 )
          {
            v3 = evt.dispKey;
LABEL_190:
            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
            v97 = evt.GetConditionArg1();
            GameSystems.Spell.EndSpell(v97, 0);
            *(_QWORD *)&v178[16] = __PAIR__((int)evt.dispIO, v3);
            goto LABEL_35;
          }
        }
      case WellKnownSpells.MindFog:
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v215 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_332;
        }
        while ( 1 )
        {
          v98 = &spellPkt.targetListHandles[v215];
          HIDWORD(v99) = HIDWORD(spellPkt.targetListHandles[v215]);
          LODWORD(v99) = spellPkt.targetListHandles[v215];
          v100 = (string )spellPkt.GetPartSysForTarget(v99);
          GameSystems.ParticleSys.End(v100);
          GameSystems.D20.D20SendSignal(*v98, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v98) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v215 < 0 )
          {
            goto LABEL_331;
          }
        }
      case WellKnownSpells.MirrorImage:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        sub_100D1E80/*0x100d1e80*/(evt.objHndCaller, HIDWORD(evt.objHndCaller), evt.dispType, evt.dispKey, evt.dispIO);
        v192 = evt.subDispNode.condNode;
        goto LABEL_339;
      case WellKnownSpells.MordenkainensFaithfulHound:
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        sdn = evt.subDispNode;
        MordenkainensFaithfulHoundEnder/*0x100d1f60*/(evt.objHndCaller);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.ObscuringMist:
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v216 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_332;
        }
        while ( 1 )
        {
          v101 = &spellPkt.targetListHandles[v216];
          HIDWORD(v102) = HIDWORD(spellPkt.targetListHandles[v216]);
          LODWORD(v102) = spellPkt.targetListHandles[v216];
          v103 = (string )spellPkt.GetPartSysForTarget(v102);
          GameSystems.ParticleSys.End(v103);
          GameSystems.D20.D20SendSignal(*v101, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v101) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v216 < 0 )
          {
            goto LABEL_331;
          }
        }
      case WellKnownSpells.OtilukesResilientSphere:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        OtilukesResilientSphereEnd/*0x100d1fe0*/(evt.objHndCaller);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.ProtectionFromArrows:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        ProtectionFromArrowsEnd/*0x100d2050*/(evt.objHndCaller);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.ProtectionFromChaos:
      case WellKnownSpells.ProtectionFromEvil:
      case WellKnownSpells.ProtectionFromGood:
      case WellKnownSpells.ProtectionFromLaw:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.ProtectionFromElements:
        switch ( evt.GetConditionArg3() )
        {
          case 1:
            GameSystems.ParticleSys.CreateAtObj("sp-Protection from Elements-acid-END", evt.objHndCaller);
            v188 = evt.objHndCaller;
            v183 = 13385;
            goto LABEL_210;
          case 3:
            GameSystems.ParticleSys.CreateAtObj("sp-Protection from Elements-cold-END", evt.objHndCaller);
            v188 = evt.objHndCaller;
            v183 = 13387;
            goto LABEL_210;
          case 6:
            GameSystems.ParticleSys.CreateAtObj("sp-Protection from Elements-water-END", evt.objHndCaller);
            v188 = evt.objHndCaller;
            v183 = 13393;
            goto LABEL_210;
          case 9:
            GameSystems.ParticleSys.CreateAtObj("sp-Protection from Elements-fire-END", evt.objHndCaller);
            v188 = evt.objHndCaller;
            v183 = 13389;
            goto LABEL_210;
          case 16:
            GameSystems.ParticleSys.CreateAtObj("sp-Protection from Elements-Sonic-END", evt.objHndCaller);
            v188 = evt.objHndCaller;
            v183 = 13391;
LABEL_210:
            GameSystems.SoundGame.PositionalSound(v183, 1, v188);
            goto LABEL_211;
          default:
            return;
        }
        return;
      case WellKnownSpells.Bane:
      case WellKnownSpells.Bless:
      case WellKnownSpells.ChaosHammer:
      case WellKnownSpells.Darkvision:
      case WellKnownSpells.Daze:
      case WellKnownSpells.DeathWard:
      case WellKnownSpells.DelayPoison:
      case WellKnownSpells.Glitterdust:
      case WellKnownSpells.Guidance:
      case WellKnownSpells.GustOfWind:
      case WellKnownSpells.LesserRestoration:
      case WellKnownSpells.MagicStone:
      case WellKnownSpells.MelfsAcidArrow:
      case WellKnownSpells.OrdersWrath:
      case WellKnownSpells.Resistance:
      case WellKnownSpells.ShockingGrasp:
      case WellKnownSpells.Shout:
LABEL_211:
        condArg1 = spellPkt.spellId;
        v185 = evt.objHndCaller;
LABEL_212:
        GameSystems.D20.D20SendSignal(v185, D20DispatcherKey.SIG_Spell_End, condArg1, 0);
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.Rage:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        if ( sub_100D20C0/*0x100d20c0*/(evt.subDispNode, evt.objHndCaller, HIDWORD(evt.objHndCaller)) != 1 )
        {
          return;
        }
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v109 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v109);
        GameSystems.ParticleSys.End((string )spellPkt.casterPartsysId);
        v110 = spellPkt.caster;
        GameSystems.ParticleSys.CreateAtObj("sp-Calm Emotions-END", v110);
        v112 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v112);
        v19 = spellPkt.RemoveTarget(evt.objHndCaller);
LABEL_33:
        if ( v19 != 1 )
        {
          goto CANNOT_END_SPELL___RET0;
        }
LABEL_34:
        v20 = CondNodeGetArg/*0x100e1ab0*/(sdn.condNode, 0);
        GameSystems.Spell.EndSpell(v20, 0);
        *(_QWORD *)&v178[16] = *(_QWORD *)&evt.dispKey;
        goto LABEL_35;
      case WellKnownSpells.RayOfEnfeeblement:
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        sub_100D21F0/*0x100d21f0*/(evt.objHndCaller);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.ReduceAnimal:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        LODWORD(parent) = evt.objHndCaller.GetInt32(obj_f.model_scale);
        evt.objHndCaller.SetInt32(obj_f.model_scale, (ulong)((float)(int)parent * 1.8F));
        GameSystems.Critter.UpdateModelEquipment(evt.objHndCaller);
        *(float *)&v113 = evt.objHndCaller.GetFloat(obj_f.speed_run) * 0.5555556F;
        evt.objHndCaller.SetInt32(obj_f.speed_run, v113);
        *(float *)&v114 = evt.objHndCaller.GetFloat(obj_f.speed_walk) * 0.5555556F;
        evt.objHndCaller.SetInt32(obj_f.speed_walk, v114);
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.Reduce:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        LODWORD(parent) = evt.objHndCaller.GetInt32(obj_f.model_scale);
        evt.objHndCaller.SetInt32(obj_f.model_scale, (ulong)((float)(int)parent * 1.8F));
        GameSystems.Critter.UpdateModelEquipment(evt.objHndCaller);
        *(float *)&v115 = evt.objHndCaller.GetFloat(obj_f.speed_run) * 0.5555556F;
        evt.objHndCaller.SetInt32(obj_f.speed_run, v115);
        *(float *)&v116 = evt.objHndCaller.GetFloat(obj_f.speed_walk) * 0.5555556F;
        evt.objHndCaller.SetInt32(obj_f.speed_walk, v116);
        v198 = evt.objHndCaller;
LABEL_218:
        GameSystems.ParticleSys.CreateAtObj("sp-Reduce Person-END", v198);
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.RepelVermin:
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v217 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_223;
        }
        while ( 1 )
        {
          v118 = &spellPkt.targetListHandles[v217];
          HIDWORD(v119) = HIDWORD(spellPkt.targetListHandles[v217]);
          LODWORD(v119) = spellPkt.targetListHandles[v217];
          v120 = (string )spellPkt.GetPartSysForTarget(v119);
          GameSystems.ParticleSys.End(v120);
          GameSystems.D20.D20SendSignal(*v118, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v118) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v217 < 0 )
          {
            v3 = evt.dispKey;
LABEL_223:
            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
            v121 = evt.GetConditionArg1();
            GameSystems.Spell.EndSpell(v121, 0);
            *(_QWORD *)&v179[16] = __PAIR__((int)evt.dispIO, v3);
            goto LABEL_48;
          }
        }
      case WellKnownSpells.ResistElements:
        switch ( evt.GetConditionArg3() )
        {
          case 1:
            GameSystems.ParticleSys.CreateAtObj("sp-Resist Elements-acid-END", evt.objHndCaller);
            v189 = evt.objHndCaller;
            v184 = 14005;
            goto LABEL_230;
          case 3:
            GameSystems.ParticleSys.CreateAtObj("sp-Resist Elements-cold-END", evt.objHndCaller);
            v189 = evt.objHndCaller;
            v184 = 14007;
            goto LABEL_230;
          case 6:
            GameSystems.ParticleSys.CreateAtObj("sp-Resist Elements-water-END", evt.objHndCaller);
            v189 = evt.objHndCaller;
            v184 = 14013;
            goto LABEL_230;
          case 9:
            GameSystems.ParticleSys.CreateAtObj("sp-Resist Elements-fire-END", evt.objHndCaller);
            v189 = evt.objHndCaller;
            v184 = 14009;
            goto LABEL_230;
          case 16:
            GameSystems.ParticleSys.CreateAtObj("sp-Resist Elements-Sonic-END", evt.objHndCaller);
            v189 = evt.objHndCaller;
            v184 = 14011;
LABEL_230:
            GameSystems.SoundGame.PositionalSound(v184, 1, v189);
            goto LABEL_231;
          default:
            return;
        }
        return;
      case WellKnownSpells.BreakEnchantment:
      case WellKnownSpells.CallLightning:
      case WellKnownSpells.Confusion:
      case WellKnownSpells.Displacement:
      case WellKnownSpells.DivineFavor:
      case WellKnownSpells.Doom:
      case WellKnownSpells.FreedomOfMovement:
      case WellKnownSpells.Prayer:
      case WellKnownSpells.Resurrection:
      case WellKnownSpells.ShieldOfFaith:
      case WellKnownSpells.Slow:
      case WellKnownSpells.TashasHideousLaughter:
      case WellKnownSpells.TrueSeeing:
      case WellKnownSpells.TrueStrike:
      case WellKnownSpells.UnholyBlight:
      case WellKnownSpells.EaglesSplendor:
      case WellKnownSpells.FoxsCunning:
      case WellKnownSpells.OwlsWisdom:
      case WellKnownSpells.LesserConfusion:
      case WellKnownSpells.RingOfFreedomOfMovement:
      case WellKnownSpells.PotionOfEnlarge:
      case WellKnownSpells.PotionOfHaste:
      case WellKnownSpells.PotionOfProtectionFromAcid:
      case WellKnownSpells.PotionOfProtectionFromElectricity:
LABEL_231:
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.RighteousMight:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        LODWORD(parent) = evt.objHndCaller.GetInt32(obj_f.model_scale);
        evt.objHndCaller.SetInt32(obj_f.model_scale, (ulong)((float)(int)parent * 0.5555556F));
        GameSystems.Critter.UpdateModelEquipment(evt.objHndCaller);
        v11 = evt.objHndCaller.GetFloat(obj_f.speed_run);
LABEL_233:
        *(float *)&v127 = v11 * 1.8F;
        evt.objHndCaller.SetInt32(obj_f.speed_run, v127);
        *(float *)&v128 = evt.objHndCaller.GetFloat(obj_f.speed_walk) * 1.8F;
        evt.objHndCaller.SetInt32(obj_f.speed_walk, v128);
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.RingOfAnimalSummoningDog:
        sub_100D2A10/*0x100d2a10*/(evt.objHndCaller);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.Sanctuary:
        if ( RemoveSpellSanctuary/*0x100d2260*/(evt.dispIO, evt.dispKey, evt.dispType, (int)evt.subDispNode, evt.objHndCaller) != 1 )
        {
          return;
        }
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        GameSystems.D20.ObjectRegistry.SendSignalAll(D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        v129 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v129);
        v9 = spellPkt.RemoveTarget(evt.objHndCaller);
        goto LABEL_24;
      case WellKnownSpells.Shield:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        v194 = evt.objHndCaller;
LABEL_238:
        GameSystems.ParticleSys.CreateAtObj("sp-Shield-END", v194);
LABEL_239:
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.Shillelagh:
        GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
        v132 = spellPkt.targetListHandles[0].GetInt32(obj_f.item_inv_location);
        if ( v132 >= 200 && v132 <= 216 )
        {
          GameSystems.Item.UnequipItemInSlot(evt.objHndCaller, v132);
        }
        GameSystems.Item.Remove(spellPkt.targetListHandles[0]);
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        if ( !spellPkt.RemoveTarget(spellPkt.targetListHandles[0]) )
        {
          goto CANNOT_END_SPELL___RET0;
        }
        condArg1 = spellPkt.spellId;
LABEL_47:
        GameSystems.Spell.EndSpell(condArg1, 0);
        *(_QWORD *)&v179[16] = *(_QWORD *)&evt.dispKey;
        goto LABEL_48;
      case WellKnownSpells.Silence:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v218 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_249;
        }
        while ( 1 )
        {
          v133 = &spellPkt.targetListHandles[v218];
          HIDWORD(v134) = HIDWORD(spellPkt.targetListHandles[v218]);
          LODWORD(v134) = spellPkt.targetListHandles[v218];
          v135 = (string )spellPkt.GetPartSysForTarget(v134);
          GameSystems.ParticleSys.End(v135);
          GameSystems.D20.D20SendSignal(*v133, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v133) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v218 < 0 )
          {
            v3 = evt.dispKey;
LABEL_249:
            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
            v136 = evt.GetConditionArg1();
            GameSystems.Spell.EndSpell(v136, 0);
            *(_QWORD *)&v178[16] = __PAIR__((int)evt.dispIO, v3);
            goto LABEL_35;
          }
        }
      case WellKnownSpells.SleetStorm:
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v219 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_332;
        }
        while ( 1 )
        {
          v137 = &spellPkt.targetListHandles[v219];
          HIDWORD(v138) = HIDWORD(spellPkt.targetListHandles[v219]);
          LODWORD(v138) = spellPkt.targetListHandles[v219];
          v139 = (string )spellPkt.GetPartSysForTarget(v138);
          GameSystems.ParticleSys.End(v139);
          GameSystems.D20.D20SendSignal(*v137, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v137) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v219 < 0 )
          {
            goto LABEL_331;
          }
        }
      case WellKnownSpells.Sleep:
      case WellKnownSpells.DeepSlumber:
        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20006, TextFloaterColor.White);
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        if ( !spellPkt.RemoveTarget(spellPkt.targetListHandles[0]) )
        {
          goto CANNOT_END_SPELL___RET0;
        }
        if ( !spellPkt.targetCount )
        {
          EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        }
                                SpellEffects.Spell_remove_mod(evt.WithoutIO);
        return;
      case WellKnownSpells.SoftenEarthAndStone:
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v220 = 0;
        if ( spellPkt.numSpellObjs > 0 )
        {
          LODWORD(parent) = (string )&spellPkt + 64;
          do
          {
            GameSystems.ParticleSys.End(*(string *)parent);
            v41 = __OFSUB__(v220 + 1, spellPkt.numSpellObjs);
            v40 = v220++ + 1 - spellPkt.numSpellObjs < 0;
            LODWORD(parent) = parent + 16;
          }
          while ( v40 ^ v41 );
        }
        v221 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_265;
        }
        while ( 1 )
        {
          v141 = &spellPkt.targetListHandles[v221];
          HIDWORD(v142) = HIDWORD(spellPkt.targetListHandles[v221]);
          LODWORD(v142) = spellPkt.targetListHandles[v221];
          v143 = (string )spellPkt.GetPartSysForTarget(v142);
          GameSystems.ParticleSys.End(v143);
          GameSystems.D20.D20SendSignal(*v141, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v141) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v221 < 0 )
          {
            v3 = evt.dispKey;
LABEL_265:
            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
            v144 = evt.GetConditionArg1();
            GameSystems.Spell.EndSpell(v144, 0);
            *(_QWORD *)&v179[16] = __PAIR__((int)evt.dispIO, v3);
            goto LABEL_48;
          }
        }
      case WellKnownSpells.SolidFog:
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v222 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_270;
        }
        while ( 1 )
        {
          v145 = &spellPkt.targetListHandles[v222];
          HIDWORD(v146) = HIDWORD(spellPkt.targetListHandles[v222]);
          LODWORD(v146) = spellPkt.targetListHandles[v222];
          v147 = (string )spellPkt.GetPartSysForTarget(v146);
          GameSystems.ParticleSys.End(v147);
          GameSystems.D20.D20SendSignal(*v145, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v145) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v222 < 0 )
          {
            v3 = evt.dispKey;
LABEL_270:
            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
            v148 = evt.GetConditionArg1();
            GameSystems.Spell.EndSpell(v148, 0);
            *(_QWORD *)&v178[16] = __PAIR__((int)evt.dispIO, v3);
            goto LABEL_35;
          }
        }
      case WellKnownSpells.SpikeGrowth:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v223 = 0;
        if ( spellPkt.numSpellObjs > 0 )
        {
          LODWORD(parent) = (string )&spellPkt + 64;
          do
          {
            GameSystems.ParticleSys.End(*(string *)parent);
            v41 = __OFSUB__(v223 + 1, spellPkt.numSpellObjs);
            v40 = v223++ + 1 - spellPkt.numSpellObjs < 0;
            LODWORD(parent) = parent + 16;
          }
          while ( v40 ^ v41 );
        }
        v224 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_332;
        }
        while ( 1 )
        {
          v149 = &spellPkt.targetListHandles[v224];
          HIDWORD(v150) = HIDWORD(spellPkt.targetListHandles[v224]);
          LODWORD(v150) = spellPkt.targetListHandles[v224];
          v151 = (string )spellPkt.GetPartSysForTarget(v150);
          GameSystems.ParticleSys.End(v151);
          GameSystems.D20.D20SendSignal(*v149, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v149) )
          {
            goto CANNOT_END_SPELL___RET0;
          }
          if ( --v224 < 0 )
          {
            goto LABEL_331;
          }
        }
      case WellKnownSpells.SpikeStones:
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v225 = 0;
        if ( spellPkt.numSpellObjs > 0 )
        {
          LODWORD(parent) = (string )&spellPkt + 64;
          do
          {
            GameSystems.ParticleSys.End(*(string *)parent);
            v41 = __OFSUB__(v225 + 1, spellPkt.numSpellObjs);
            v40 = v225++ + 1 - spellPkt.numSpellObjs < 0;
            LODWORD(parent) = parent + 16;
          }
          while ( v40 ^ v41 );
        }
        v226 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_285;
        }
        while ( 1 )
        {
          v152 = &spellPkt.targetListHandles[v226];
          HIDWORD(v153) = HIDWORD(spellPkt.targetListHandles[v226]);
          LODWORD(v153) = spellPkt.targetListHandles[v226];
          v154 = (string )spellPkt.GetPartSysForTarget(v153);
          GameSystems.ParticleSys.End(v154);
          GameSystems.D20.D20SendSignal(*v152, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v152) )
          {
            break;
          }
          if ( --v226 < 0 )
          {
LABEL_284:
            v3 = evt.dispKey;
LABEL_285:
            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
LABEL_286:
            v155 = evt.GetConditionArg1();
            GameSystems.Spell.EndSpell(v155, 0);
            *(_QWORD *)&v179[16] = __PAIR__((int)evt.dispIO, v3);
LABEL_48:
            *(_QWORD *)&v179[8] = *(GameObjectBody *)((string )&evt.objHndCaller + 4);
            *(_QWORD *)v179 = __PAIR__(evt.objHndCaller, (int)sdn);
            SpellEffects.Spell_remove_mod(in evt);
            return;
          }
        }
        goto CANNOT_END_SPELL___RET0;
      case WellKnownSpells.SpiritualWeapon:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        SpiritualWeaponRemover/*0x100d2990*/(evt.objHndCaller);
        GameSystems.MapObject.SetFlags(evt.objHndCaller, ObjectFlag.OFF);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.StinkingCloud:
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v227 = 0;
        if ( spellPkt.numSpellObjs > 0 )
        {
          LODWORD(parent) = (string )&spellPkt + 64;
          do
          {
            GameSystems.ParticleSys.End(*(string *)parent);
            v41 = __OFSUB__(v227 + 1, spellPkt.numSpellObjs);
            v40 = v227++ + 1 - spellPkt.numSpellObjs < 0;
            LODWORD(parent) = parent + 16;
          }
          while ( v40 ^ v41 );
        }
        v228 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_295;
        }
        while ( 1 )
        {
          v156 = &spellPkt.targetListHandles[v228];
          HIDWORD(v157) = HIDWORD(spellPkt.targetListHandles[v228]);
          LODWORD(v157) = spellPkt.targetListHandles[v228];
          v158 = (string )spellPkt.GetPartSysForTarget(v157);
          GameSystems.ParticleSys.End(v158);
          GameSystems.D20.D20SendSignal(*v156, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v156) )
          {
            break;
          }
          if ( --v228 < 0 )
          {
            v3 = evt.dispKey;
LABEL_295:
            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
            v159 = evt.GetConditionArg1();
            GameSystems.Spell.EndSpell(v159, 0);
            *(_QWORD *)&v178[16] = __PAIR__((int)evt.dispIO, v3);
LABEL_35:
            *(_QWORD *)&v178[8] = *(GameObjectBody *)((string )&evt.objHndCaller + 4);
            *(_QWORD *)v178 = __PAIR__(evt.objHndCaller, (int)sdn);
            SpellEffects.Spell_remove_mod(in evt);
            return;
          }
        }
        goto CANNOT_END_SPELL___RET0;
      case WellKnownSpells.DispelMagic:
      case WellKnownSpells.RemoveParalysis:
      case WellKnownSpells.Stoneskin:
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.SpellMonsterFrogTongue:
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        if ( evt.GetConditionArg2() == 1 || evt.objHndCaller == spellPkt.caster )
        {
          sub_100D26F0/*0x100d26f0*/(evt.objHndCaller);
          if ( spellPkt.targetListHandles[0] )
          {
            spellPkt.RemoveTarget(spellPkt.targetListHandles[0]);
          }
          v160 = evt.GetConditionArg1();
          GameSystems.Spell.EndSpell(v160, 0);
          if ( spellPkt.targetListHandles[0] )
          {
            GameSystems.D20.D20SendSignal(spellPkt.targetListHandles[0], D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          }
          else
          {
            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          }
        }
        else if ( evt.GetConditionArg2() == 2 )
        {
          GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_Grapple_Removed, spellPkt.spellId, 0);
          GameSystems.D20.D20SendSignal(spellPkt.targetListHandles[0], D20DispatcherKey.SIG_Spell_Grapple_Removed, spellPkt.spellId, 0);
        }
        return;
      case WellKnownSpells.SpellMonsterVrockScreech:
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.SpellMonsterVrockSpores:
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.Suggestion:
        if ( RemoveSpellSuggestion/*0x100d2460*/(evt.dispIO, evt.dispType, evt.dispKey, evt.subDispNode, evt.objHndCaller) != 1 )
        {
          return;
        }
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        GameSystems.Critter.RemoveFollower(evt.objHndCaller, 1);
        GameUiBridge.UpdatePartyUi();
        v161 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v161);
        v9 = spellPkt.RemoveTarget(evt.objHndCaller);
LABEL_24:
        if ( v9 != 1 )
        {
          goto CANNOT_END_SPELL___RET0;
        }
        v10 = evt.GetConditionArg1();
        GameSystems.Spell.EndSpell(v10, 0);
                goto LABEL_334;
      case WellKnownSpells.GiantVermin:
      case WellKnownSpells.SummonMonsterI:
      case WellKnownSpells.SummonMonsterIi:
      case WellKnownSpells.SummonMonsterIii:
      case WellKnownSpells.SummonMonsterIv:
      case WellKnownSpells.SummonMonsterV:
      case WellKnownSpells.SummonNaturesAllyI:
      case WellKnownSpells.SummonNaturesAllyIi:
      case WellKnownSpells.SummonNaturesAllyIii:
      case WellKnownSpells.SummonNaturesAllyIv:
      case WellKnownSpells.SummonNaturesAllyV:
      case WellKnownSpells.SpellSummonFungi:
      case WellKnownSpells.SpellSummonLamia:
      case WellKnownSpells.SpellMonsterSummoned:
      case WellKnownSpells.SummonAirElemental:
      case WellKnownSpells.SummonEarthElemental:
      case WellKnownSpells.SummonFireElemental:
      case WellKnownSpells.SummonWaterElemental:
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        sub_100D2A10/*0x100d2a10*/(evt.objHndCaller);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.SummonSwarm:
        if ( sub_100D26A0/*0x100d26a0*/(evt.subDispNode, evt.objHndCaller, HIDWORD(evt.objHndCaller)) == 1 )
        {
          GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
          if ( evt.dispKey == D20DispatcherKey.SIG_Concentration_Broken )
          {
            evt.SetConditionArg2(2);
            evt.SetConditionArg3(1);
            spellPkt.duration = 2;
            spellPkt.durationRemaining = 2;
            GameSystems.Spell.UpdateSpellPacket(spellPkt);
            GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
          }
          else
          {
            v162 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
            GameSystems.ParticleSys.End(v162);
            GameSystems.ParticleSys.End((string )spellPkt.casterPartsysId);
            v163 = 0;
            JUMPOUT(spellPkt.targetCount, 0, sub_100DA9BC/*0x100da9bc*/);
            do
            {
              GameSystems.D20.D20SendSignal(__PAIR__(HIDWORD(spellPkt.targetListHandles[v163]), spellPkt.targetListHandles[v163]), D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
              HIDWORD(v164) = HIDWORD(spellPkt.targetListHandles[v163]);
              LODWORD(v164) = spellPkt.targetListHandles[v163];
              GameSystems.D20.Combat.Kill(v164, null);
              ++v163;
            }
            while ( v163 < spellPkt.targetCount );
            sub_100DA9BC/*0x100da9bc*/();
          }
        }
        return;
      case WellKnownSpells.Blur:
      case WellKnownSpells.ProduceFlame:
      case WellKnownSpells.SeeInvisibility:
      case WellKnownSpells.TreeShape:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.VampiricTouch:
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        End_Particles_And_Send_Spell_End_Signal/*0x100d09d0*/(evt.objHndCaller);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.Virtue:
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        End_Particles_And_Send_Spell_End_Signal/*0x100d09d0*/(evt.objHndCaller);
        goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
      case WellKnownSpells.Web:
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v229 = 0;
        if ( spellPkt.numSpellObjs > 0 )
        {
          LODWORD(parent) = (string )&spellPkt + 64;
          do
          {
            GameSystems.ParticleSys.End(*(string *)parent);
            v41 = __OFSUB__(v229 + 1, spellPkt.numSpellObjs);
            v40 = v229++ + 1 - spellPkt.numSpellObjs < 0;
            LODWORD(parent) = parent + 16;
          }
          while ( v40 ^ v41 );
        }
        v230 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_327;
        }
        while ( 1 )
        {
          v165 = &spellPkt.targetListHandles[v230];
          HIDWORD(v166) = HIDWORD(spellPkt.targetListHandles[v230]);
          LODWORD(v166) = spellPkt.targetListHandles[v230];
          v167 = (string )spellPkt.GetPartSysForTarget(v166);
          GameSystems.ParticleSys.End(v167);
          GameSystems.D20.D20SendSignal(*v165, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( !spellPkt.RemoveTarget(*v165) )
          {
            break;
          }
          if ( --v230 < 0 )
          {
            v3 = evt.dispKey;
LABEL_327:
            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
            v168 = evt.GetConditionArg1();
            GameSystems.Spell.EndSpell(v168, 0);
            *(_QWORD *)&v169[16] = __PAIR__((int)evt.dispIO, v3);
            *(_QWORD *)&v169[8] = *(GameObjectBody *)((string )&evt.objHndCaller + 4);
            *(_QWORD *)v169 = *(_QWORD *)&evt.subDispNode;
            SpellEffects.Spell_remove_mod(in evt);
            return;
          }
        }
        goto CANNOT_END_SPELL___RET0;
      case WellKnownSpells.WindWall:
        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        v170 = (string )evt.GetConditionArg4();
        GameSystems.ParticleSys.End(v170);
        v231 = spellPkt.targetCount - 1;
        if ( spellPkt.targetCount - 1 < 0 )
        {
          goto LABEL_332;
        }
        break;
      default:
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        EndSpellParticlesForTargetObj/*0x100d2930*/(evt.objHndCaller, evt.subDispNode);
        v175 = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
        Logger.Info("d20_mods_spells.c / _remove_spell(): WARNING - no remove function defined for spell=( {0} )", v175);
Play_OnEndSPellCast__Remove_Caller_From_Targets:
        GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnEndSpellCast);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          goto CANNOT_END_SPELL___RET0;
        }
        v192 = sdn.condNode;
LABEL_339:
        v176 = CondNodeGetArg/*0x100e1ab0*/(v192, 0);
        GameSystems.Spell.EndSpell(v176, 0);
        return;
    }
    do
    {
      v171 = &spellPkt.targetListHandles[v231];
      HIDWORD(v172) = HIDWORD(spellPkt.targetListHandles[v231]);
      LODWORD(v172) = spellPkt.targetListHandles[v231];
      v173 = (string )spellPkt.GetPartSysForTarget(v172);
      GameSystems.ParticleSys.End(v173);
      GameSystems.D20.D20SendSignal(*v171, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
      if ( !spellPkt.RemoveTarget(*v171) )
      {
CANNOT_END_SPELL___RET0:
        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
        return;
      }
      --v231;
    }
    while ( v231 >= 0 );
LABEL_331:
    v3 = evt.dispKey;
LABEL_332:
    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
LABEL_333:
    v174 = evt.GetConditionArg1();
    GameSystems.Spell.EndSpell(v174, 0);
    *(_QWORD *)&v177[16] = __PAIR__((int)evt.dispIO, v3);
LABEL_334:
            SpellEffects.Spell_remove_mod(in evt);
  }
  else
  {
    Logger.Info("d20_mods_spells.c / _remove_spell(): error getting spellid packet for spell_packet");
  }
}
/* Orphan comments:
TP Replaced @ spell_condition.cpp:108
*/


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x100d2b40)]
public static void   PotionOfHeroismSkillBonus(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  DispIoObjBonus dispIo;

  v1 = data;
  dispIo = evt.GetDispIoObjBonus();
  dispIo.bonOut.AddBonus(v1, 34, 113);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100d01d0)]
public static void   SuggestionOnAdd(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg2;
  int condArg3;


  GameObjectBody v6;

  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  condArg3 = evt.GetConditionArg3();
  if ( !evt.objHndCaller.AddCondition("Charmed", condArg1, condArg2, condArg3) )
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_suggestion(): unable to add condition");
  }
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20018, TextFloaterColor.Red);
  v6 = GameSystems.Party.GetConsciousLeader();
  GameSystems.Critter.AddFollower(evt.objHndCaller, v6, 1, 0);
  GameUiBridge.UpdatePartyUi();
  evt.SetConditionArg3(1);
}


[DispTypes(DispatcherType.SaveThrowSpellResistanceBonus, DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100c5490)]
public static void   SavingThrowSpellResistanceBonusCallback(in DispatcherCallbackArgs evt, int data1, int data2)
{
  int condArg1;
  DispIoSavingThrow dispIo;  int v4;
  bool v5;
  bool v6;
  bool v7;
  int v8;
  int v9;
  string v10;
  int v11;
  int v12;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  dispIo = evt.GetDispIoSavingThrow();/*INLINED:v3=evt.subDispNode.subDispDef*/  switch ( data2 )
  {
    case 0xCD:
      if ( !GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
      {
        goto LABEL_34;
      }
      v4 = spellPkt.spellEnum;
      switch ( spellPkt.spellEnum )
      {
        case WellKnownSpells.MagicCircleAgainstChaos:
          v5 = !(dispIo.obj.HasChaoticAlignment()) ;
          goto LABEL_31;
        case WellKnownSpells.MagicCircleAgainstEvil:
          v6 = !(dispIo.obj.HasEvilAlignment()) ;
          goto LABEL_24;
        case WellKnownSpells.MagicCircleAgainstGood:
          v7 = !(dispIo.obj.HasGoodAlignment()) ;
          goto LABEL_27;
        case WellKnownSpells.MagicCircleAgainstLaw:
          v5 = !(dispIo.obj.HasLawfulAlignment()) ;
          goto LABEL_31;
        default:
          goto LABEL_33;
      }
      goto LABEL_33;
    case 0xCE:
      if ( !GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
      {
        goto LABEL_34;
      }
      v4 = spellPkt.spellEnum;
      switch ( spellPkt.spellEnum )
      {
        case WellKnownSpells.MagicCircleAgainstChaos:
          v7 = !(evt.objHndCaller.HasChaoticAlignment()) ;
          goto LABEL_27;
        case WellKnownSpells.MagicCircleAgainstEvil:
          v5 = !(evt.objHndCaller.HasEvilAlignment()) ;
          goto LABEL_31;
        case WellKnownSpells.MagicCircleAgainstGood:
          v6 = !(evt.objHndCaller.HasGoodAlignment()) ;
          goto LABEL_24;
        case WellKnownSpells.MagicCircleAgainstLaw:
          v7 = !(evt.objHndCaller.HasLawfulAlignment()) ;
          goto LABEL_27;
        default:
          goto LABEL_33;
      }
      goto LABEL_33;
    case 0xCF:
      if ( !GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
      {
        goto LABEL_34;
      }
      v4 = spellPkt.spellEnum;
      switch ( spellPkt.spellEnum )
      {
        case WellKnownSpells.ProtectionFromChaos:
          v6 = !(dispIo.obj.HasChaoticAlignment()) ;
          goto LABEL_24;
        case WellKnownSpells.ProtectionFromEvil:
          v7 = !(dispIo.obj.HasEvilAlignment()) ;
          goto LABEL_27;
        case WellKnownSpells.ProtectionFromGood:
          v5 = !(dispIo.obj.HasGoodAlignment()) ;
          goto LABEL_31;
        case WellKnownSpells.ProtectionFromLaw:
          v6 = !(dispIo.obj.HasLawfulAlignment()) ;
          goto LABEL_24;
        default:
          break;
      }
LABEL_33:
      v10 = GameSystems.Spell.GetSpellName(v4);
      Logger.Info("d20_mods_spells.c / _spell_resistance_saving_bonus(): invalid spell=( {0} )", v10);
      break;
    case 0x108:
      if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
      {
        v4 = spellPkt.spellEnum;
        switch ( spellPkt.spellEnum )
        {
          case WellKnownSpells.PotionOfProtectionFromOutsiders:
            v9 = GameSystems.Critter.IsCategory(dispIo.obj, MonsterCategory.outsider);
            goto LABEL_30;
          case WellKnownSpells.PotionOfProtectionFromElementals:
            v6 = !GameSystems.Critter.IsCategory(dispIo.obj, MonsterCategory.elemental) ;
LABEL_24:
            if ( !v6 )
            {
              dispIo.bonlist.AddBonus(data1, 15, data2);
            }
            return;
          case WellKnownSpells.PotionOfProtectionFromEarth:
            v7 = !GameSystems.Critter.IsCategorySubtype(dispIo.obj, 256) ;
LABEL_27:
            if ( v7 )
            {
              return;
            }
            v8 = data1;
            v12 = data2;
            v11 = 15;
            goto LABEL_37;
          case WellKnownSpells.PotionOfProtectionFromUndead:
            v9 = GameSystems.Critter.IsCategory(dispIo.obj, MonsterCategory.undead);
LABEL_30:
            v5 = v9 == 0;
LABEL_31:
            if ( !v5 )
            {
              dispIo.bonlist.AddBonus(data1, 15, data2);
            }
            break;
          default:
            goto LABEL_33;
        }
      }
      else
      {
LABEL_34:
        Logger.Info("d20_mods_spells.c / _spell_resistance_saving_bonus(): unable to retrieve spell_packet for spell_id=( {0} )", condArg1);
      }
      break;
    case 0xC7:
    case 0x112:
      dispIo.bonlist.AddBonus(data1, 13, data2);
      break;
    default:
      v8 = data1;
      v12 = data2;
      v11 = 13;
LABEL_37:
      dispIo.bonlist.AddBonus(v8, v11, v12);
      break;
  }
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100d64d0)]
public static void   sub_100D64D0(in DispatcherCallbackArgs evt)
{
  DispIoD20Signal dispIo;
  int condArg1;
  string v3;
  string v4;
    BonusList bonlist;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoD20Signal();
  if ( evt.dispKey != D20DispatcherKey.NONEx9E )
  {
    bonlist = BonusList.Create();
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      v3 = GameSystems.Skill.GetSkillName(6);
      if ( GameSystems.Spell.DispelRoll(evt.objHndCaller, &bonlist, 0, spellPkt.dc, v3, 0) < 0 )
      {
        nullsub_1/*0x100027f0*/();
        return;
      }
    }
    else
    {
      v4 = GameSystems.Skill.GetSkillName(6);
      if ( GameSystems.Spell.DispelRoll(evt.objHndCaller, &bonlist, 0, 15, v4, 0) < 0 )
      {
        nullsub_1/*0x100027f0*/();
        return;
      }
    }
    nullsub_1/*0x100027f0*/();
    goto LABEL_16;
  }
  if ( !GameSystems.Spell.TryGetActiveSpell(dispIo.data1, out spellPkt) )
  {
    return;
  }
  if ( spellPkt.spellEnum > WellKnownSpells.HealingCircle)
  {
    if ( spellPkt.spellEnum != WellKnownSpells.Heal)
    {
      return;
    }
LABEL_16:
                SpellEffects.Spell_remove_mod(evt.WithoutIO);
    return;
  }
  if ( spellPkt.spellEnum == WellKnownSpells.HealingCircle|| spellPkt.spellEnum >= WellKnownSpells.CureCriticalWounds&& spellPkt.spellEnum <= WellKnownSpells.CureSeriousWounds)
  {
    goto LABEL_16;
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cd390)]
[TemplePlusLocation("spell_condition.cpp:92")]
public static void   EmotionBeginSpell(in DispatcherCallbackArgs evt, int data1, int data2)
{
  int condArg1;


  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    if ( data2 == 82 )
    {
      GameSystems.AI.FleeFrom(evt.objHndCaller, spellPkt.caster);
    }
    if ( !spellPkt.caster.AddCondition("sp-Concentrating", condArg1, 0, 0) )
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_emotion(): unable to add condition to spell_caster");
    }
  }
}
/* Orphan comments:
TP Replaced @ spell_condition.cpp:92
*/


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d6ff0)]
public static void   sub_100D6FF0(in DispatcherCallbackArgs evt, int data)
{
  DispIoObjEvent dispIo;
  int condArg1;
  string v3;


  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoObjEvent();
  if ( dispIo.evtId == evt.GetConditionArg3() )
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      SpellPktTriggerAoeHitScript/*0x100c37d0*/(spellPkt.spellId);
      if ( D20ModSpells.CheckSpellResistance(&spellPkt, dispIo.tgt) )
      {
        return;
      }
      if ( evt.dispKey == D20DispatcherKey.OnEnterAoE )
      {
        spellPkt.AddTarget(dispIo.tgt, 0, true);
        dispIo.tgt.AddCondition("sp-Wind Wall Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        v3 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v3);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _wind_wall_hit_trigger(): cannot remove target");
          return;
        }
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        *(_DWORD *)&v4[20] = evt.dispIO;
        *(_DWORD *)&v4[16] = 19;
        *(_QWORD *)&v4[8] = *(GameObjectBody *)((string )&evt.objHndCaller + 4);
        *(_QWORD *)v4 = *(_QWORD *)&evt.subDispNode;
        SpellEffects.Spell_remove_mod(in evt);
      }
    }
  }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*  else
  {
    Logger.Info("d20_mods_spells.c / _wind_wall_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c6350)]
public static void   BlinkSpellFailure(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;
  Dice v2;
  int v3;
  unsigned Dice v4;
  int v5;
  Dice v6;
  int v7;
  int v8;
  int v9;
  int v10;
  int v11;

  dispIo = evt.GetDispIoD20Query();
  if ( dispIo.return_val != 1 )
  {
    v2 = 1.new Dice(100, 0);
    v3 = GetPackedDiceBonus/*0x10038c90*/(v2);
    v4 = 1.new Dice(100, 0);
    v5 = GetPackedDiceType/*0x10038c40*/(v4);
    v6 = 1.new Dice(100, 0);
    v7 = GetPackedDiceNumDice/*0x10038c30*/(v6);
    v8 = DiceRoller/*0x10038b60*/(v7, v5, v3);
    v9 = v8;
    if ( v8 >= 20 )
    {
      v11 = GameSystems.RollHistory.AddPercentageCheck(evt.objHndCaller, null, 20, 111, v8, 62, 192);
      GameSystems.RollHistory.CreateRollHistoryString(v11);
    }
    else
    {
      GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x24, evt.objHndCaller, null);
      GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 112);
      v10 = GameSystems.RollHistory.AddPercentageCheck(evt.objHndCaller, null, 20, 111, v9, 112, 192);
      GameSystems.RollHistory.CreateRollHistoryString(v10);
      dispIo.return_val = 1;
    }
  }
}


[DispTypes(DispatcherType.TakingDamage)]
[TempleDllLocation(0x100c9aa0)]
public static void   ResistElementsDamageResistance(in DispatcherCallbackArgs evt)
{
  DispIoDamage dispIo;
  int v2;
  D20DT *v3;
  int condArg4;
  int condArg1;
  int v6;
  int v7;
  int v8;
  int v9;
  int condArg3;
  D20DT damType;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoDamage();
  switch ( evt.GetConditionArg3() )
  {
    case 1:
      damType = 7;
      goto LABEL_7;
    case 3:
      damType = 8;
      goto LABEL_7;
    case 6:
      damType = 9;
      goto LABEL_7;
    case 9:
      damType = 10;
      goto LABEL_7;
    case 16:
      damType = 11;
LABEL_7:
      v2 = 0;
      if ( dispIo.damage.diceCount <= 0 )
      {
        return;
      }
      v3 = &dispIo.damage.dice[0].damType;
      break;
    default:
      return;
  }
  while ( *v3 != damType )
  {
    ++v2;
    v3 += 5;
    if ( v2 >= dispIo.damage.diceCount )
    {
      return;
    }
  }
  condArg4 = evt.GetConditionArg4();
  if ( condArg4 > 0 )
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      if ( spellPkt.casterLevel <= 10 )
      {
        if ( spellPkt.casterLevel > 6 )
        {
          condArg4 = 20;
        }
      }
      else
      {
        condArg4 = 30;
      }
    }
    else
    {
      condArg4 = 10;
    }
    v6 = dispIo.damage.GetOverallDamageByType();
    v7 = v6;
    if ( v6 > 0 )
    {
      if ( v6 <= condArg4 )
      {
        dispIo.damage.AddDR(v6, damType, 104);
        v8 = condArg4 - v7;
      }
      else
      {
        dispIo.damage.AddDR(condArg4, damType, 104);
        v8 = 0;
      }
      v9 = dispIo.damage.GetOverallDamageByType();
      dispIo.damage.finalDamage = v9;
      condArg3 = evt.GetConditionArg3();
      Logger.Info("absorbed {0} points of [{1}] damage, DR points left: {2}", v7 - v9, (&pSpellEntry_Descriptor_Strings/*0x102bfa90*/)[4 * condArg3], v8);
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ce6d0)]
public static void   sub_100CE6D0(in DispatcherCallbackArgs evt)
{
  int condArg1;
  bool v2;
  string v3;
  SpellPacketBody spellPkt;

  if ( evt.objHndCaller.HasCondition(SpellEffects.SpellSummoned) )
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) && !D20ModSpells.CheckSpellResistance(&spellPkt, evt.objHndCaller) )
    {
      switch ( spellPkt.spellEnum )
      {
        case WellKnownSpells.MagicCircleAgainstChaos:
          v2 = !(evt.objHndCaller.HasChaoticAlignment()) ;
          goto LABEL_9;
        case WellKnownSpells.MagicCircleAgainstEvil:
          v2 = !(evt.objHndCaller.HasEvilAlignment()) ;
          goto LABEL_9;
        case WellKnownSpells.MagicCircleAgainstGood:
          v2 = !(evt.objHndCaller.HasGoodAlignment()) ;
          goto LABEL_9;
        case WellKnownSpells.MagicCircleAgainstLaw:
          v2 = !(evt.objHndCaller.HasLawfulAlignment()) ;
LABEL_9:
          if ( !v2 )
          {
            GameSystems.D20.Actions.curSeqGetTurnBasedStatus().hourglassState = 0;
          }
          break;
        default:
          v3 = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
          Logger.Info("d20_mods_spells.c / _begin_spell_magic_circle_outward(): invalid spell=( {0} )", v3);
          break;
      }
    }
  }
}


[DispTypes(DispatcherType.GetMoveSpeedBase)]
[TempleDllLocation(0x100cb700)]
[TemplePlusLocation("spell_condition.cpp:251")]
public static void   WebOnSpeedNull(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoMoveSpeed dispIo;

  dispIo = evt.GetDispIoMoveSpeed();
  if ( !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement) )
  {
    dispIo.bonlist.SetOverallCap(1, 0, 0, data2);
    dispIo.bonlist.SetOverallCap(2, 0, 0, data2);
  }
}
/* Orphan comments:
TP Replaced @ spell_condition.cpp:251
*/


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100d05f0)]
public static void   WebHit(in DispatcherCallbackArgs evt)
{
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20028, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100c5380)]
public static void   SavingThrowEmotionModifierCallback(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoSavingThrow dispIo;

  dispIo = evt.GetDispIoSavingThrow();
  switch ( data2 )
  {
    case 0x103:
      if ( !evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions) )
      {
        dispIo.bonlist.AddBonus(-data1, 13, data2);
      }
      break;
    case 0x104:
    case 0x12A:
    case 0x12B:
      if ( !evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions) )
      {
        dispIo.bonlist.AddBonus(data1, 13, data2);
      }
      break;
    default:
      dispIo.bonlist.AddBonus(data1, 13, data2);
      break;
  }
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100c4850)]
public static void   sub_100C4850(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;
  DispIoAttackBonus v2;
  int v3;
  int condArg3;
  int v5;
  int condArg4;

  dispIo = evt.GetDispIoAttackBonus();
  v2 = dispIo;
  switch ( data2 )
  {
    case 0xD2:
      if ( (dispIo.attackPacket.weaponUsed == null))
      {
        v3 = data2;
        condArg3 = evt.GetConditionArg3();
        v2.bonlist.AddBonus(condArg3, 12, v3);
      }
      break;
    case 0xD4:
      if ( dispIo.attackPacket.weaponUsed !=null|| (evt.objHndCaller.GetStat(Stat.level_monk) )!=0)
      {
        v5 = data2;
        condArg4 = evt.GetConditionArg4();
        v2.bonlist.AddBonus(condArg4, 12, v5);
      }
      break;
    case 0xD1:
      if ( (dispIo.attackPacket.weaponUsed == null))
      {
        goto LABEL_8;
      }
      break;
    default:
LABEL_8:
      dispIo.bonlist.AddBonus(data1, 12, data2);
      break;
    case 0xD0:
      if ( dispIo.attackPacket.weaponUsed !=null|| (evt.objHndCaller.GetStat(Stat.level_monk) )!=0)
      {
        v2.bonlist.AddBonus(data1, 12, data2);
      }
      break;
  }
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d53b0)]
public static void   d20_mods_spells__globe_of_inv_hit(in DispatcherCallbackArgs evt, int data)
{
  DispIoObjEvent dispIo;
  int condArg1;
  string v3;


  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoObjEvent();
  if ( dispIo.evtId == evt.GetConditionArg3() )
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      SpellPktTriggerAoeHitScript/*0x100c37d0*/(spellPkt.spellId);
      if ( D20ModSpells.CheckSpellResistance(&spellPkt, dispIo.tgt) )
      {
        return;
      }
      if ( evt.dispKey == D20DispatcherKey.OnEnterAoE )
      {
        spellPkt.AddTarget(dispIo.tgt, 0, true);
        dispIo.tgt.AddCondition("sp-Minor Globe of Invulnerability Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        v3 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v3);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _minor_globe_of_invulnerability_hit_trigger(): cannot remove target");
          return;
        }
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        *(_DWORD *)&v4[20] = evt.dispIO;
        *(_DWORD *)&v4[16] = 19;
        *(_QWORD *)&v4[8] = *(GameObjectBody *)((string )&evt.objHndCaller + 4);
        *(_QWORD *)v4 = *(_QWORD *)&evt.subDispNode;
        SpellEffects.Spell_remove_mod(in evt);
      }
    }
  }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*  else
  {
    Logger.Info("d20_mods_spells.c / _minor_globe_of_invulnerability_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100d31f0)]
public static void   ConcentratingOnDamage2(in DispatcherCallbackArgs evt, int data)
{
  DispIoDamage dispIo;
  int condArg1;
    SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoDamage();
  if ( dispIo.attackPacket.d20ActnType != D20ActionType.CAST_SPELL)
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt)       && spellPkt.spellEnum != WellKnownSpells.MeldIntoStone      && !GameSystems.Skill.SkillRoll(evt.objHndCaller, SkillId.concentration, dispIo.damage.finalDamage + spellPkt.spellKnownSlotLevel + 10, 0, 1) )
    {
      GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 54);
      GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x20, evt.objHndCaller, null);
      *(_QWORD *)&v3[16] = 156;
      *(_QWORD *)&v3[8] = __PAIR__(28, HIDWORD(evt.objHndCaller));
      *(_QWORD *)v3 = *(_QWORD *)&evt.subDispNode;
      SpellEffects.Spell_remove_mod(in evt);
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cba10)]
public static void   spSummonedOnAdd(in DispatcherCallbackArgs evt)
{
  int condArg1;
  float angleRadian;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    if ( GameSystems.Feat.HasFeat(spellPkt.caster, FeatId.AUGMENT_SUMMONING) )
    {
      evt.objHndCaller.AddCondition(StatusEffects.AugmentSummoningEnhancement);
    }
    angleRadian = Angles.RotationTo(evt.objHndCaller, spellPkt.caster);
    GameSystems.MapObject.SetRotation(evt.objHndCaller, angleRadian);
    GameSystems.Anim.Interrupt(evt.objHndCaller, 6, 0);
  }
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100dda90)]
public static void   SleepHpChanged(in DispatcherCallbackArgs evt)
{
  int v1;
  DispIoD20Signal dispIo;
  int v3;
  int v4;
      int v7;

  v1 = evt.objHndCaller.GetStat(Stat.hp_current);
  v7 = evt.objHndCaller.GetInt32(obj_f.critter_subdual_damage);
  dispIo = evt.GetDispIoD20Signal();
  v3 = dispIo.data2;
  if ( v3 <= 0 )
  {
    if ( v3 >= 0 )
    {
      v4 = dispIo.data1;
      return;
    }
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(evt.WithoutIO);
                SpellEffects.Spell_remove_mod(evt.WithoutIO);
    if ( v1 < 0 )
    {
      if ( !GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.DIEHARD) )
      {
        evt.objHndCaller.AddCondition(StatusEffects.Dying);
        return;
      }
LABEL_11:
      evt.objHndCaller.AddCondition(StatusEffects.Disabled);
      return;
    }
    if ( v1 > 0 && v7 >= v1 )
    {
      evt.objHndCaller.AddCondition(StatusEffects.Unconscious);
      return;
    }
    if ( (v1 )==0)
    {
      goto LABEL_11;
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cfb10)]
public static void   BeginSpellSoftenEarthAndStone(in DispatcherCallbackArgs evt)
{
  int condArg1;
  float radiusInches;
  int v3;  SpellEntry a2;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
    radiusInches = (float)(int)a2.radiusTarget * 12F;
    spellPkt.aoeObj = evt.objHndCaller;
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 36, 37, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_soften_earth_and_stone(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100d3bc0)]
public static void   sub_100D3BC0(in DispatcherCallbackArgs evt, int data)
{
  DispIoD20Signal dispIo;
  int v2;
  int v3;
  int condArg1;
  string v5;
  string v6;


  int condArg3;
  BonusList bonList;
  SpellPacketBody spellPkt;
  DispIoBonusList a1;
  BonusList bonlist;

  dispIo = evt.GetDispIoD20Signal();
  if ( dispIo.data1 == evt.GetConditionArg1() && (dispIo.data2 )==0)
  {
    bonlist = BonusList.Create();
    bonList = BonusList.Create();
    a1 = new DispIoBonusList();
    v2 = evt.objHndCaller.GetStat(0, &a1);
    v3 = D20StatSystem.GetModifierForAbilityScore(v2);
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      bonList.AddBonus(v3, 0, 103);
      v5 = GameSystems.Stat.GetStatName(0);
      if ( GameSystems.Spell.DispelRoll(evt.objHndCaller, &bonList, 0, 20, v5, 0) < 0 )
      {
        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20027, TextFloaterColor.Red);
      }
      else
      {
        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 21003, TextFloaterColor.White);
        v6 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v6);
        if ( spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _control_plants_entangled_break_free_check(): cannot remove target");
          return;
        }
                                SpellEffects.Spell_remove_mod(evt.WithoutIO);
        spellPkt.AddTarget(evt.objHndCaller, 0, true);
        condArg3 = evt.GetConditionArg3();
        evt.objHndCaller.AddCondition("sp-Control Plants Entangle Pre", spellPkt.spellId, spellPkt.durationRemaining, condArg3);
      }
    }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _control_plants_entangled_break_free_check(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.ConditionAddPre)]
[TempleDllLocation(0x100dc0a0)]
public static void   SpellRemovedBy(in DispatcherCallbackArgs evt, ConditionSpec data)
{

  if ( evt.GetDispIoCondStruct().condStruct == (ConditionSpec )data )
  {
                SpellEffects.Spell_remove_spell(evt.WithoutIO);
  }
        CommonConditionCallbacks.CondOverrideBy(in evt);
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d5780)]
public static void   sub_100D5780(in DispatcherCallbackArgs evt, int data)
{
  DispIoObjEvent dispIo;
  int condArg1;
  string v3;
    GameObjectBody v5;

  int v7;


  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoObjEvent();
  if ( dispIo.evtId == evt.GetConditionArg3() )
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      SpellPktTriggerAoeHitScript/*0x100c37d0*/(spellPkt.spellId);
      if ( D20ModSpells.CheckSpellResistance(&spellPkt, dispIo.tgt) )
      {
        return;
      }
      if ( evt.dispKey == D20DispatcherKey.OnEnterAoE )
      {
        v5 = dispIo.tgt;
        v7 = GameSystems.ParticleSys.CreateAtObj("sp-Obscuring Mist-hit", v5);
        spellPkt.AddTarget(dispIo.tgt, v7, true);
        dispIo.tgt.AddCondition("sp-Obscuring Mist Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        v3 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v3);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _obscuring_mist_hit_trigger(): cannot remove target");
          return;
        }
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        *(_DWORD *)&v4[20] = evt.dispIO;
        *(_DWORD *)&v4[16] = 19;
        *(_QWORD *)&v4[8] = *(GameObjectBody *)((string )&evt.objHndCaller + 4);
        *(_QWORD *)v4 = *(_QWORD *)&evt.subDispNode;
        SpellEffects.Spell_remove_mod(in evt);
      }
    }
  }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*  else
  {
    Logger.Info("d20_mods_spells.c / _obscuring_mist_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100d2bd0)]
public static void   PotionOfProtectionFromEnergyDamageCallback(in DispatcherCallbackArgs evt)
{  int condArg4;  D20DT v4;
  DispIoDamage dispIo;
  DamagePacket v6;
  int v7;
  int v8;
  int v9;
    D20DT damType;/*INLINED:v1=evt.subDispNode*/  condArg4 = evt.GetConditionArg4();/*INLINED:v3=(SubDispNode *)evt.GetConditionArg3()*/  v4 = (D20DT)(SubDispNode *)evt.GetConditionArg3();
  evt.subDispNode = (SubDispNode *)evt.GetConditionArg3();
  dispIo = evt.GetDispIoDamage();
  v6 = &dispIo.damage;
  v7 = GetDamageTypeOverallDamage/*0x100e1210*/(&dispIo.damage, v4);
  if ( v7 > 0 && condArg4 > 0 )
  {
    if ( condArg4 < v7 )
    {
      v8 = condArg4;
      v9 = 0;
    }
    else
    {
      v8 = v7;
      v9 = condArg4 - v7;
    }
    Logger.Info("({0}) damage reduced", v8);
    evt.SetConditionArg4(v9);
    v6.AddDR(v8, damType, 124);
    if ( (v9 )==0)
    {
      *(_QWORD *)&v10[16] = *(_QWORD *)&evt.dispKey;
      *(_QWORD *)&v10[8] = *(GameObjectBody *)((string )&evt.objHndCaller + 4);
      *(_QWORD *)v10 = __PAIR__(evt.objHndCaller, (int)evt.subDispNode);
      CommonConditionCallbacks.conditionRemoveCallback(in evt);
    }
  }
}


[DispTypes(DispatcherType.GetMoveSpeedBase)]
[TempleDllLocation(0x100ca8c0)]
public static void   solidFogMoveRestriction(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoMoveSpeed dispIo;

  dispIo = evt.GetDispIoMoveSpeed();
  if ( !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement) )
  {
    dispIo.bonlist.SetOverallCap(1, 5, 0, data2);
    dispIo.bonlist.SetOverallCap(2, 5, 0, data2);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100dbbd0)]
public static void   SpWeaponKeenOnAdd(in DispatcherCallbackArgs evt)
{
  int condArg1;

  GameObjectBody parent;

  parent = null;
  if ( !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Item_Has_Keen_Bonus) )
  {
    condArg1 = evt.GetConditionArg1();
    AddItemConditionToWielder/*0x100d2f30*/(evt.objHndCaller, "Weapon Keen", 0, 0, 0, 0, condArg1);
    GameSystems.Item.GetParent(evt.objHndCaller, &parent);
    GameSystems.D20.Status.initItemConditions(parent);
  }
}


[DispTypes(DispatcherType.AbilityScoreLevel)]
[TempleDllLocation(0x100cb2e0)]
public static void   treeshapeStatRestriction(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoBonusList dispIo;
  BonusList *v2;

  dispIo = evt.GetDispIoBonusList();
  if ( evt.dispKey - 1 == data1 )
  {
    v2 = &dispIo.bonlist;
    dispIo.bonlist.SetOverallCap(1, 0, 0, data2);
    v2.SetOverallCap(2, 0, 0, data2);
  }
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x100c61a0)]
public static void   SkillModifier_BestowCurseRolls_Callback(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoObjBonus dispIo;

  dispIo = evt.GetDispIoObjBonus();
  dispIo.bonOut.AddBonus(-data2, 0, data2);
}


[DispTypes(DispatcherType.ToHitBonusBase)]
[TempleDllLocation(0x100cadd0)]
public static void   sub_100CADD0(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus v1;
  int condArg1;
  int v3;
  DispIoAttackBonus dispIo;
  SpellPacketBody v5;

  v1 = evt.GetDispIoAttackBonus();
  dispIo = new DispIoAttackBonus();
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out v5) )
  {
    v3 = DispatcherExtensions.DispatchToHitBonusBase(v5.caster, &dispIo);
    v1.bonlist.AddBonus(v3 - 1, 0, data2);
  }
  DispIoAttackBonusDebug/*0x1004d9f0*/(&dispIo);
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100c35f0)]
public static void   sub_100C35F0(in DispatcherCallbackArgs evt)
{
  DispIoD20Signal dispIo;

  dispIo = evt.GetDispIoD20Signal();
  if ( (ConditionAttachment *)dispIo.data1 == &evt.subDispNode.condNode && (dispIo.data2 )==0)
  {
    evt.RemoveThisCondition();
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cce50)]
public static void   sub_100CCE50(in DispatcherCallbackArgs evt)
{
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20012, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.AbilityScoreLevel)]
[TempleDllLocation(0x100d2ac0)]
public static void   sub_100D2AC0(in DispatcherCallbackArgs evt)
{
  int condArg2;
  DispIoBonusList dispIo;

  condArg2 = evt.GetConditionArg2();
  dispIo = evt.GetDispIoBonusList();
  dispIo.bonlist.AddBonus(condArg2, 12, 113);
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100cb150)]
public static void   SuggestionIsAiControlledQuery(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;

  dispIo = evt.GetDispIoD20Query();
  if ( (evt.GetConditionArg3() )==0)
  {
    dispIo.return_val = 1;
    dispIo.data1 = evt.GetConditionArg1();
    dispIo.data2 = 0;
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100dbd00)]
public static void   MagicWeaponOnAdd(in DispatcherCallbackArgs evt)
{
  int condArg1;

  GameObjectBody parent;

  parent = null;
  if ( !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Item_Has_Enhancement_Bonus) )
  {
    condArg1 = evt.GetConditionArg1();
    AddItemConditionToWielder/*0x100d2f30*/(evt.objHndCaller, "Weapon Enhancement Bonus", 1, 0, 0, 0, condArg1);
    GameSystems.Item.GetParent(evt.objHndCaller, &parent);
    GameSystems.D20.Status.initItemConditions(parent);
  }
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d5560)]
public static void   MindFogAoeEvent(in DispatcherCallbackArgs evt, int data)
{
  DispIoObjEvent dispIo;
  int condArg1;
  string v3;


  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoObjEvent();
  if ( dispIo.evtId == evt.GetConditionArg3() )
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      SpellPktTriggerAoeHitScript/*0x100c37d0*/(spellPkt.spellId);
      if ( D20ModSpells.CheckSpellResistance(&spellPkt, dispIo.tgt) )
      {
        return;
      }
      if ( evt.dispKey == D20DispatcherKey.OnEnterAoE )
      {
        if ( GameSystems.D20.Combat.SavingThrowSpell(dispIo.tgt, spellPkt.caster, spellPkt.dc, 0, 0, spellPkt.spellId) )
        {
          GameSystems.Spell.FloatSpellLine(dispIo.tgt, 30001, TextFloaterColor.White);
        }
        else
        {
          GameSystems.Spell.FloatSpellLine(dispIo.tgt, 30002, TextFloaterColor.White);
          spellPkt.AddTarget(dispIo.tgt, 0, true);
          dispIo.tgt.AddCondition("sp-Mind Fog Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
        }
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        v3 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v3);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _mind_fog_hit_trigger(): cannot remove target");
          return;
        }
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        *(_DWORD *)&v4[20] = evt.dispIO;
        *(_DWORD *)&v4[16] = 19;
        *(_QWORD *)&v4[8] = *(GameObjectBody *)((string )&evt.objHndCaller + 4);
        *(_QWORD *)v4 = *(_QWORD *)&evt.subDispNode;
        SpellEffects.Spell_remove_mod(in evt);
      }
    }
  }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*  else
  {
    Logger.Info("d20_mods_spells.c / _mind_fog_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ce290)]
public static void   SpellInvisibilityBegin(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg2;
  int condArg3;



  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  condArg3 = evt.GetConditionArg3();
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20017, TextFloaterColor.Red);
  if ( !evt.objHndCaller.AddCondition("Invisible", condArg1, condArg2, condArg3) )
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_invisibility(): unable to add condition");
  }
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d5b60)]
public static void   sub_100D5B60(in DispatcherCallbackArgs evt, int data)
{
  DispIoObjEvent dispIo;
  int condArg1;
  string v3;
    int v5;
  int v6;
  int v7;
  GameObjectBody v8;

  int v10;


  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoObjEvent();
  if ( dispIo.evtId == evt.GetConditionArg3() )
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      SpellPktTriggerAoeHitScript/*0x100c37d0*/(spellPkt.spellId);
      if ( D20ModSpells.CheckSpellResistance(&spellPkt, dispIo.tgt) )
      {
        return;
      }
      if ( evt.dispKey == D20DispatcherKey.OnEnterAoE )
      {
        v5 = GameSystems.D20.Combat.SavingThrowSpell(dispIo.tgt, spellPkt.caster, spellPkt.dc, SavingThrowType.Will, 0, spellPkt.spellId);
        v6 = dispIo.tgt;
        v7 = HIDWORD(dispIo.tgt);
        if ( (v5 )!=0)
        {
          GameSystems.Spell.FloatSpellLine(__PAIR__(v7, v6), 30001, TextFloaterColor.White);
        }
        else
        {
          GameSystems.Spell.FloatSpellLine(__PAIR__(v7, v6), 30002, TextFloaterColor.White);
          v8 = dispIo.tgt;
          v10 = GameSystems.ParticleSys.CreateAtObj("Fizzle", v8);
          spellPkt.AddTarget(dispIo.tgt, v10, true);
          dispIo.tgt.AddCondition("sp-Silence Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
        }
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        v3 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v3);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _silence_hit_trigger(): cannot remove target");
          return;
        }
        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
        *(_DWORD *)&v4[20] = evt.dispIO;
        *(_DWORD *)&v4[16] = 19;
        *(_QWORD *)&v4[8] = *(GameObjectBody *)((string )&evt.objHndCaller + 4);
        *(_QWORD *)v4 = *(_QWORD *)&evt.subDispNode;
        SpellEffects.Spell_remove_mod(in evt);
      }
    }
  }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*  else
  {
    Logger.Info("d20_mods_spells.c / _silence_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100d0310)]
public static void   sub_100D0310(in DispatcherCallbackArgs evt)
{
  evt.objHndCaller.AddCondition(StatusEffects.Prone);
  GameSystems.Anim.PushAnimate(evt.objHndCaller, 64);
}


[DispTypes(DispatcherType.TurnBasedStatusInit)]
[TempleDllLocation(0x100c8b30)]
public static void   sub_100C8B30(in DispatcherCallbackArgs evt)
{
  DispIOTurnBasedStatus dispIo;
  int condArg1;
  bool v3;
  string v4;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIOTurnBasedStatus();
  if ( evt.objHndCaller.HasCondition(SpellEffects.SpellSummoned) )
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt)       && !D20ModSpells.CheckSpellResistance(&spellPkt, evt.objHndCaller)       && dispIo
!=null      && dispIo.tbStatus !=null)
    {
      switch ( spellPkt.spellEnum )
      {
        case WellKnownSpells.MagicCircleAgainstChaos:
          v3 = !(evt.objHndCaller.HasChaoticAlignment()) ;
          goto LABEL_13;
        case WellKnownSpells.MagicCircleAgainstEvil:
          if ( evt.objHndCaller.HasEvilAlignment() )
          {
            dispIo.tbStatus.hourglassState = 0;
            dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
          }
          break;
        case WellKnownSpells.MagicCircleAgainstGood:
          if ( evt.objHndCaller.HasGoodAlignment() )
          {
            dispIo.tbStatus.hourglassState = 0;
            dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
          }
          break;
        case WellKnownSpells.MagicCircleAgainstLaw:
          v3 = !(evt.objHndCaller.HasLawfulAlignment()) ;
LABEL_13:
          if ( !v3 )
          {
            dispIo.tbStatus.hourglassState = 0;
            dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
          }
          break;
        default:
          v4 = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
          Logger.Info("d20_mods_spells.c / _begin_spell_magic_circle_outward(): invalid spell=( {0} )", v4);
          break;
      }
    }
  }
}


[DispTypes(DispatcherType.TurnBasedStatusInit)]
[TempleDllLocation(0x100c6c00)]
public static void   sub_100C6C00(in DispatcherCallbackArgs evt)
{
  int condArg3;
  DispIOTurnBasedStatus dispIo;
  int condArg1;
  int *v4;
  SpellPacketBody spellPkt;

  condArg3 = evt.GetConditionArg3();
  dispIo = evt.GetDispIOTurnBasedStatus();
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    if ( dispIo !=null)
    {
      v4 = &dispIo.tbStatus.hourglassState;
      if ( v4 )
      {
        switch ( condArg3 )
        {
          case 2:
            *v4 = 0;
            dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
            evt.objHndCaller.AddCondition(StatusEffects.Prone);
            GameSystems.Anim.PushAnimate(evt.objHndCaller, 64);
            break;
          case 3:
            GameSystems.AI.FleeProcess(evt.objHndCaller, spellPkt.caster);
            break;
          case 1:
          case 4:
            *v4 = 0;
            dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
            break;
          default:
            return;
        }
      }
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cca00)]
[TemplePlusLocation("spell_condition.cpp:261")]
public static void   ColorsprayUnconsciousOnAdd(in DispatcherCallbackArgs evt)
{
  int condArg1;
  Dice v2;
  int v3;
  unsigned Dice v4;
  int v5;
  Dice v6;
  int v7;
  int v8;  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20024, TextFloaterColor.Red);
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    v2 = 2.new Dice(4, 0);
    v3 = GetPackedDiceBonus/*0x10038c90*/(v2);
    v4 = 2.new Dice(4, 0);
    v5 = GetPackedDiceType/*0x10038c40*/(v4);
    v6 = 2.new Dice(4, 0);
    v7 = GetPackedDiceNumDice/*0x10038c30*/(v6);
    v8 = DiceRoller/*0x10038b60*/(v7, v5, v3);/*INLINED:v9=evt.subDispNode.condNode*/    spellPkt.duration = v8;
    spellPkt.durationRemaining = v8;
    evt.SetConditionArg2(v8);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_color_spray_unconscious(): unable to save new spell_packet");
    }
*/  }
  else
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_color_spray_unconscious(): unable to get spell_packet");
  }
}
/* Orphan comments:
TP Replaced @ spell_condition.cpp:261
*/


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ccc30)]
public static void   BeginSpellControlPlants(in DispatcherCallbackArgs evt)
{
  int condArg1;
  float radiusInches;
  int v3;  SpellEntry a2;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
    spellPkt.aoeObj = evt.objHndCaller;
    radiusInches = (float)(int)GameSystems.Spell.GetSpellRangeExact(-a2.radiusTarget, spellPkt.casterLevel, spellPkt.caster)
                 * 12F;
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 4, 5, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_control_plants(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.Tooltip)]
[TempleDllLocation(0x100c6490)]
public static void   CallLightningTooltipCallback(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoTooltip dispIo;
  int condArg3;
  int v3;
  int v4;
  string meslineValue;
int meslineKey;
  CHAR v6;

  dispIo = evt.GetDispIoTooltip();
  condArg3 = evt.GetConditionArg3();
  meslineKey = data1;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  v6 = String.Format("{0} [{1}]", meslineValue, condArg3);
  v4 = dispIo.numStrings;
  if ( v4 < 10 )
  {
    dispIo.numStrings = v4 + 1;
    strncpy(dispIo.strings[v4].text, &v6, 0x100);
  }
}


[DispTypes(DispatcherType.AbilityScoreLevel)]
[TempleDllLocation(0x100c74d0)]
public static void   sub_100C74D0(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoBonusList dispIo;

  dispIo = evt.GetDispIoBonusList();
  if ( evt.dispKey - 1 == data1 )
  {
    dispIo.bonlist.AddBonus(2, 13, data2);
  }
}


[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x100dc100)]
[TemplePlusLocation("condition.cpp:502")]
public static void   SpellModCountdownRemove(in DispatcherCallbackArgs evt, int data)
{
  int condArg2;
  int durNew;
  int condArg1;
  int spellIdentifier;
  int v5;
  int v6;
  int spId;


  int spId_;


  int v13;
        string v17;
      int v20;
      SpellPacketBody spellPkt;
  SpellPacketBody v24;

  condArg2 = evt.GetConditionArg2();
  durNew = condArg2 - evt.GetDispIoD20Signal().data1;
  condArg1 = evt.GetConditionArg1();
  if ( !GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    Logger.Info("d20_mods_spells.c / countdown_remove(): err.... why are we counting a spell that no longer exists? spell removed without removing the appropriate conditions?");
    *(_DWORD *)&v22[20] = 0;
    *(_QWORD *)&v22[12] = *(_QWORD *)&evt.dispType;
    *(_QWORD *)&v22[4] = evt.objHndCaller;
    *(_DWORD *)v22 = evt.subDispNode;
    SpellEffects.Spell_remove_mod(in evt);
    return;
  }
  spellIdentifier = data;
  if ( durNew >= 0 )
  {
    v20 = spellIdentifier + 0xFFFFFF21;
    if ( (v20 )!=0)
    {
      if ( v20 == 4 && (evt.GetConditionArg3() )==0)
      {
        return;
      }
    }
    else if ( (evt.GetConditionArg4() )==0)
    {
      return;
    }
    evt.SetConditionArg2(durNew);
    spellPkt.durationRemaining = durNew;
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
    switch ( data )
    {
      case 48:
        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20012, TextFloaterColor.Red);
        break;
      case 156:
                                        SpellEffects.AcidDamage(in evt);
        break;
      case 203:
        GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 47);
        break;
      case 240:
        if ( !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Unconscious) )
        {
          GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 21005, TextFloaterColor.White);
          FrogGrappleController.PlayPull(evt.objHndCaller);
        }
        break;
      case 244:
        GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(33, evt.objHndCaller, null);
        GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 89);
        break;
      default:
        return;
    }
  }
  else
  {
    v5 = spellIdentifier + 0xFFFFFF2E;
    if ( (v5 )!=0)
    {
      v6 = v5 - 0xD;
      if ( (v6 )!=0)
      {
        if ( v6 != 0x12
          || (spId = evt.GetConditionArg1(), !GameSystems.Spell.TryGetActiveSpell(spId, out v24) )
          || GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Unconscious) )
        {
          GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                                        SpellEffects.Spell_remove_spell(evt.WithoutIO);
                                        SpellEffects.Spell_remove_mod(evt.WithoutIO);
        }
        else
        {
          spId_ = evt.GetConditionArg1();
          if ( !v24.targetListHandles[0].AddCondition("sp-Frog Tongue Swallowed", spId_, 1, 0) )
          {
            Logger.Info("d20_mods_spells.c / _countdown_remove(): unable to add condition");
          }
          v13 = evt.GetConditionArg1();
          if ( !evt.objHndCaller.AddCondition("sp-Frog Tongue Swallowing", v13, 2, 0) )
          {
            Logger.Info("d20_mods_spells.c / _countdown_remove(): unable to add condition");
          }
          FrogGrappleController.PlaySwallow(evt.objHndCaller);
                                        SpellEffects.Spell_remove_mod(in evt);
        }
      }
      else
      {
        v17 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v17);
        if ( spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                                        SpellEffects.Spell_remove_mod(in evt);
        }
        else
        {
          Logger.Info("d20_mods_spells.c / _countdown_remove(): cannot remove target");
        }
      }
    }
    else
    {
                              SpellEffects.Spell_remove_mod(in evt);
    }
  }
}
/* Orphan comments:
TP Replaced @ condition.cpp:502
*/


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c79e0)]
public static void   sub_100C79E0(in DispatcherCallbackArgs evt)
{
  evt.GetDispIoD20Query();
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cc360)]
public static void   BeginSpellCastLightning(in DispatcherCallbackArgs evt)
{
  int condArg1;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    spellPkt.targetListHandles[0] = evt.objHndCaller;
    spellPkt.targetListPartsysIds[0] = 0;
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_call_lightning(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100c6f40)]
public static void   sub_100C6F40(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoDamage dispIo;

  dispIo = evt.GetDispIoDamage();
  dispIo.damage.AddDamageBonus(-data1, 17, data2);
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c4300)]
public static void   sub_100C4300(in DispatcherCallbackArgs evt, int data)
{
  DispIoD20Query dispIo;
  int condArg1;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoD20Query();
  if ( dispIo.data1 == data && (dispIo.data2 )==0)
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      dispIo.return_val = 1;
      *(_QWORD *)&dispIo.data1 = spellPkt.caster;
    }
  }
}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100c7c40)]
public static void   FireShieldCounterDamage(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  DispIoDamage dispIo;
  int condArg3;
  D20DT v4;
  int condArg1;
  int v6;
  Dice v7;
  SpellPacketBody spellPkt;

  v1 = 0;
  dispIo = evt.GetDispIoDamage();
  condArg3 = evt.GetConditionArg3();
  if ( condArg3 == 3 )
  {
    v4 = 8;
  }
  else
  {
    if ( condArg3 != 9 )
    {
      return;
    }
    v4 = 10;
  }
  if ( (dispIo.attackPacket.weaponUsed == null)|| dispIo.attackPacket.weaponUsed.GetInt32(obj_f.weapon_range) <= 5 )
  {
    v1 = 1;
  }
  switch ( dispIo.attackPacket.d20ActnType )
  {
    case 1:
    case 2:
    case 3:
    case 0xC:
    case 0xD:
    case 0xE:
    case 0xF:
    case 0x11:
    case 0x13:
    case 0x14:
    case 0x15:
    case 0x18:
    case 0x1A:
    case 0x1C:
    case 0x1D:
    case 0x1E:
    case 0x23:
    case 0x24:
    case 0x2A:
    case 0x2B:
    case 0x2C:
    case 0x40:
      if ( v1 == 1 )
      {
        condArg1 = evt.GetConditionArg1();
        if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt)           && !D20ModSpells.CheckSpellResistance(&spellPkt, dispIo.attackPacket.victim) )
        {
          v6 = spellPkt.spellId;
          v7 = 1.new Dice(6, spellPkt.casterLevel);
          GameSystems.D20.Combat.SpellDamageFull(dispIo.attackPacket.attacker, dispIo.attackPacket.victim, v7, v4, 1, D20ActionType.CAST_SPELL, v6, 0);
        }
      }
      break;
    default:
      return;
  }
}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100d2ca0)]
public static void   ProtectionFromAlignmentDamageCallback(in DispatcherCallbackArgs evt, int data)
{
  DispIoDamage dispIo;
  GameObjectBody v2;
  GameObjectBody v3;
  int condArg1;
  int v5;
  string v6;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoDamage();
  v2 = GameSystems.Item.GetItemAtInvIdx(dispIo.attackPacket.attacker, 203);
  v3 = GameSystems.Item.GetItemAtInvIdx(dispIo.attackPacket.attacker, 204);
  if ( (v2 == null)&& (v3 == null))
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      switch ( spellPkt.spellEnum )
      {
        case WellKnownSpells.PotionOfProtectionFromOutsiders:
          v5 = GameSystems.Critter.IsCategory(dispIo.attackPacket.attacker, MonsterCategory.outsider);
          goto LABEL_7;
        case WellKnownSpells.PotionOfProtectionFromElementals:
          v5 = GameSystems.Critter.IsCategory(dispIo.attackPacket.attacker, MonsterCategory.elemental);
LABEL_7:
          if ( (v5 )==0)
          {
            return;
          }
          goto LABEL_12;
        case WellKnownSpells.PotionOfProtectionFromEarth:
          if ( !GameSystems.Critter.IsCategorySubtype(dispIo.attackPacket.attacker, 256) )
          {
            return;
          }
          goto LABEL_12;
        case WellKnownSpells.PotionOfProtectionFromUndead:
          if ( GameSystems.Critter.IsCategory(dispIo.attackPacket.attacker, MonsterCategory.undead) )
          {
LABEL_12:
            if ( !D20ModSpells.CheckSpellResistance(&spellPkt, evt.objHndCaller) )
            {
              dispIo.damage.AddPhysicalDR(dispIo.damage.finalDamage, 1, 0x68);
            }
          }
          break;
        default:
          v6 = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
          Logger.Info("d20_mods_spells.c / _protection_from_alignment_prevent_damage(): invalid spell=( {0} )", v6);
          break;
      }
    }
    else
    {
      Logger.Info("d20_mods_spells.c / _protection_from_alignment_prevent_damage(): unable to retrieve spell_packet");
    }
  }
}


    }
}