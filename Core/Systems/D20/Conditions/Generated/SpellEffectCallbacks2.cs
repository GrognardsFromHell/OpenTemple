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
[TempleDllLocation(0x100cc500)]
public static void   sub_100CC500(in DispatcherCallbackArgs evt)
{
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20014, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100cb4f0)]
public static void   WebBreakfreeRadial(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  int v2;
  string meslineValue;
int meslineKey;
  RadialMenuEntry radMenuEntry;

  if ( GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Is_BreakFree_Possible) )
  {
    radMenuEntry = RadialMenuEntry.Create();
    radMenuEntry.spellIdMaybe = evt.GetConditionArg1();
    radMenuEntry.d20ActionData1 = radMenuEntry.spellIdMaybe;
    radMenuEntry.d20ActionType = D20ActionType.BREAK_FREE;
    meslineKey = 5061;
    meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
    radMenuEntry.text = (string )meslineValue;
    radMenuEntry.helpSystemHashkey = "TAG_RADIAL_MENU_BREAK_FREE"/*ELFHASH*/;
    if ( !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement) )
    {
      v2 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Movement);
      GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v2);
    }
  }
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d6d70)]
public static void   WebObjEvent(in DispatcherCallbackArgs evt, int data)
{
  DispIoObjEvent dispIo;
  int condArg1;
  string v3;
    GameObjectBody v5;

  int v7;


  GameObjectBody v10;

  int v12;


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
          v10 = dispIo.tgt;
          v12 = GameSystems.ParticleSys.CreateAtObj("Fizzle", v10);
          spellPkt.AddTarget(dispIo.tgt, v12, true);
          dispIo.tgt.AddCondition("sp-Web Off", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId, 20);
        }
        else
        {
          GameSystems.Spell.FloatSpellLine(dispIo.tgt, 30002, TextFloaterColor.White);
          v5 = dispIo.tgt;
          v7 = GameSystems.ParticleSys.CreateAtObj("sp-Web Hit", v5);
          spellPkt.AddTarget(dispIo.tgt, v7, true);
          dispIo.tgt.AddCondition("sp-Web On", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
        }
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        v3 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v3);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _web_hit_trigger(): cannot remove target");
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
    Logger.Info("d20_mods_spells.c / _web_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x100c41f0)]
public static void   DispelAlignmentAcBonus(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;
  int v2;
  bool v3;
  int bonAmt;

  dispIo = evt.GetDispIoAttackBonus();
  v2 = data2;
  switch ( v2 )
  {
    case 175:
      v3 = !(dispIo.attackPacket.attacker.HasChaoticAlignment()) ;
      goto LABEL_8;
    case 176:
      if ( !(dispIo.attackPacket.attacker.HasEvilAlignment()) )
      {
        return;
      }
      v2 = data2;
      bonAmt = data1;
      goto LABEL_11;
    case 177:
      if ( dispIo.attackPacket.attacker.HasGoodAlignment() )
      {
        dispIo.bonlist.AddBonus(data1, 11, data2);
      }
      break;
    case 178:
      v3 = !(dispIo.attackPacket.attacker.HasLawfulAlignment()) ;
LABEL_8:
      if ( !v3 )
      {
        dispIo.bonlist.AddBonus(data1, 11, data2);
      }
      break;
    default:
      bonAmt = data1;
LABEL_11:
      dispIo.bonlist.AddBonus(bonAmt, 11, v2);
      break;
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c6440)]
public static void   sub_100C6440(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;

  dispIo = evt.GetDispIoD20Query();
  if ( evt.GetConditionArg3() > 0 )
  {
    dispIo.return_val = 1;
  }
  dispIo.data1 = evt.GetConditionArg1();
  dispIo.data2 = 0;
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cf760)]
public static void   BeginSpellSilence(in DispatcherCallbackArgs evt)
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
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 32, 33, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_silence(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.AbilityScoreLevel)]
[TempleDllLocation(0x100c7450)]
public static void   DivinePowerStrengthBonus(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoBonusList dispIo;

  dispIo = evt.GetDispIoBonusList();
  if ( evt.dispKey - 1 == data1 )
  {
    dispIo.bonlist.AddBonus(6, 12, data2);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cd430)]
public static void   enlargeModelScaleInc(in DispatcherCallbackArgs evt)
{
  double v1;
  int n32Data;
  int v3;

  v1 = (float)evt.objHndCaller.GetInt32(obj_f.model_scale) * 1.8F;
  evt.objHndCaller.SetInt32(obj_f.model_scale, (ulong)v1);
  GameSystems.Critter.UpdateModelEquipment(evt.objHndCaller);
  *(float *)&n32Data = evt.objHndCaller.GetFloat(obj_f.speed_run) * 0.5555556F;
  evt.objHndCaller.SetInt32(obj_f.speed_run, n32Data);
  *(float *)&v3 = evt.objHndCaller.GetFloat(obj_f.speed_walk) * 0.5555556F;
  evt.objHndCaller.SetInt32(obj_f.speed_walk, v3);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ce470)]
public static void   InvisibilitySphereBegin(in DispatcherCallbackArgs evt)
{
  int condArg1;
  float radiusInches;
  int v3;  SpellEntry spEntry;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out spEntry);
    radiusInches = (float)(int)spEntry.radiusTarget * 12F;
    spellPkt.aoeObj = evt.objHndCaller;
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 20, 21, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_invisibility_sphere(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.TurnBasedStatusInit)]
[TempleDllLocation(0x100c6160)]
public static void   BestowCurseActionsTurnBasedStatusInit(in DispatcherCallbackArgs evt)
{
  DispIOTurnBasedStatus dispIo;
  int *v2;

  dispIo = evt.GetDispIOTurnBasedStatus();
  if ( GameSystems.Random.GetInt(0, 1) == 1 && dispIo !=null)
  {
    v2 = &dispIo.tbStatus.hourglassState;
    if ( v2 )
    {
      *v2 = 0;
      dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cc2e0)]
public static void   sub_100CC2E0(in DispatcherCallbackArgs evt)
{
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20019, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100c8340)]
public static void   Guidance_RadialMenuEntry_Callback(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  int v2;
  int v3;  int v5;
  string v6;  int v8;
  string v9;  int v11;
  string v12;
  string meslineValue;
int meslineKey;
  RadialMenuEntry radMenuEntry;

  radMenuEntry = RadialMenuEntry.Create();
  radMenuEntry.d20ActionType = D20ActionType.CAST_SPELL;
  radMenuEntry.d20ActionData1 = 0;
  meslineKey = 5055;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  radMenuEntry.text = (string )meslineValue;
  v2 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Spells);
  v3 = GameSystems.D20.RadialMenu.AddParentChildNode(evt.objHndCaller, ref radMenuEntry, v2);
  radMenuEntry = RadialMenuEntry.Create();
  radMenuEntry.minArg = 0;/*INLINED:v4=evt.subDispNode.condNode*/  radMenuEntry.maxArg = 1;
  radMenuEntry.type = (RadialMenuEntryType.Slider|RadialMenuEntryType.Toggle);
  radMenuEntry.actualArg = (int)CondNodeGetArgPtr/*0x100e1af0*/(evt.subDispNode.condNode, 2);
  radMenuEntry.callback = GameSystems.D20.RadialMenu.RadialMenuCheckboxOrSliderCallback;
  meslineKey = 5056;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  radMenuEntry.text = (string )meslineValue;
  v6 = GameSystems.Spell.GetSpellHelpTopic(213);
  radMenuEntry.helpSystemHashkey = v6/*ELFHASH*/;
  GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v3);
  radMenuEntry = RadialMenuEntry.Create();/*INLINED:v7=evt.subDispNode.condNode*/  radMenuEntry.maxArg = 1;
  radMenuEntry.minArg = 0;
  radMenuEntry.type = (RadialMenuEntryType.Slider|RadialMenuEntryType.Toggle);
  radMenuEntry.actualArg = (int)CondNodeGetArgPtr/*0x100e1af0*/(evt.subDispNode.condNode, 3);
  radMenuEntry.callback = GameSystems.D20.RadialMenu.RadialMenuCheckboxOrSliderCallback;
  meslineKey = 5057;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  radMenuEntry.text = (string )meslineValue;
  v9 = GameSystems.Spell.GetSpellHelpTopic(213);
  radMenuEntry.helpSystemHashkey = v9/*ELFHASH*/;
  GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v3);
  radMenuEntry = RadialMenuEntry.Create();/*INLINED:v10=evt.subDispNode.condNode*/  radMenuEntry.maxArg = 1;
  radMenuEntry.minArg = 0;
  radMenuEntry.type = (RadialMenuEntryType.Slider|RadialMenuEntryType.Toggle);
  radMenuEntry.actualArg = (int)CondNodeGetArgPtr/*0x100e1af0*/(evt.subDispNode.condNode, 4);
  radMenuEntry.callback = GameSystems.D20.RadialMenu.RadialMenuCheckboxOrSliderCallback;
  meslineKey = 5058;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  radMenuEntry.text = (string )meslineValue;
  v12 = GameSystems.Spell.GetSpellHelpTopic(213);
  radMenuEntry.helpSystemHashkey = v12/*ELFHASH*/;
  GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v3);
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c6b80)]
public static void   sub_100C6B80(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;
  int condArg1;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoD20Query();
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) && evt.GetConditionArg3() == 3 )
  {
    dispIo.return_val = 1;
    dispIo.obj = spellPkt.caster;
  }
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d5010)]
public static void   Condition__36__invisibility_purge(in DispatcherCallbackArgs evt, int data)
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
        v7 = GameSystems.ParticleSys.CreateAtObj("Fizzle", v5);
        spellPkt.AddTarget(dispIo.tgt, v7, true);
        dispIo.tgt.AddCondition("sp-Invisibility Purge Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        v3 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v3);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _invisibility_purge_hit_trigger(): cannot remove target");
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
    Logger.Info("d20_mods_spells.c / _invisibility_purge_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x100c6050)]
public static void   sub_100C6050(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;

  dispIo = evt.GetDispIoAttackBonus();
  dispIo.bonlist.AddBonus(data1, 9, data2);
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d5d90)]
public static void   SleetStormAoE(in DispatcherCallbackArgs evt, int data)
{
  DispIoObjEvent dispIo;
  int condArg1;
  string v3;
    GameObjectBody v5;

  int partsysId;


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
        partsysId = GameSystems.ParticleSys.CreateAtObj("sp-Sleet Storm-Hit", v5);
        spellPkt.AddTarget(dispIo.tgt, partsysId, true);
        dispIo.tgt.AddCondition("sp-Sleet Storm Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        v3 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v3);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _sleet_storm_hit_trigger(): cannot remove target");
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
    Logger.Info("d20_mods_spells.c / _sleet_storm_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.SkillLevel, DispatcherType.AbilityCheckModifier)]
[TempleDllLocation(0x100c5a30)]
public static void   sub_100C5A30(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoObjBonus dispIo;
  dispIo = evt.GetDispIoObjBonus();/*INLINED:v2=evt.subDispNode.subDispDef*/  if ( data2 == 223 )
  {
    dispIo.bonOut.AddBonus(-data1, 0, 223);
  }
  else
  {
    dispIo.bonOut.AddBonus(data1, 0, data2);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100caed0)]
public static void   StinkingCloudRemoveConcentration(in DispatcherCallbackArgs evt)
{
  long v1;

  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20026, TextFloaterColor.Red);
  if ( GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Concentrating) )
  {
    v1 = GameSystems.D20.D20QueryReturnObject(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Concentrating, 0, 0);
    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Remove_Concentration, v1, SHIDWORD(v1));
  }
}


[DispTypes(DispatcherType.Tooltip)]
[TempleDllLocation(0x100c3390)]
public static void   TooltipGeneralCallback(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoTooltip dispIo;
  int v2;
  int condArg3;
  int v4;
  int v5;
  string meslineValue;
int meslineKey;
  CHAR v7;

  dispIo = evt.GetDispIoTooltip();
  v2 = data2;
  if ( v2 == 29 )
  {
    condArg3 = evt.GetConditionArg3();
  }
  else
  {
    condArg3 = v2 != 232;
  }
  meslineKey = data1;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  v7 = String.Format("{0}{1}", meslineValue, condArg3);
  v5 = dispIo.numStrings;
  if ( v5 < 10 )
  {
    dispIo.numStrings = v5 + 1;
    strncpy(dispIo.strings[v5].text, &v7, 0x100);
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c43d0)]
public static void   QueryCritterHasCondition(in DispatcherCallbackArgs evt, ConditionSpec data)
{
  DispIoD20Query dispIo;
  DispIoD20Query evtObj_;

  dispIo = evt.GetDispIoD20Query();
  evtObj_ = dispIo;
  if ( dispIo.data1 == data && (dispIo.data2 )==0)
  {
    dispIo.return_val = 1;
    dispIo.data1 = evt.GetConditionArg1();
    evtObj_.data2 = 0;
  }
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100c7490)]
public static void   sub_100C7490(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoSavingThrow dispIo;

  dispIo = evt.GetDispIoSavingThrow();
  if ( evt.dispKey == D20DispatcherKey.SAVE_REFLEX )
  {
    dispIo.bonlist.AddBonus(data1, 0, data2);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cd100)]
public static void   BeginSpellDivinePower(in DispatcherCallbackArgs evt)
{
  string v1;
  int condArg1;
  int v3;


  int condArg2;
  int v7;
  CHAR extraText2;
  SpellPacketBody spellPkt;

  v1 = GameSystems.Stat.GetStatName(0);
  extraText2 = String.Format(" [{0}]", v1);
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20023, TextFloaterColor.White);
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    v3 = spellPkt.casterLevel;
    extraText2 = String.Format("[{0}] ", spellPkt.casterLevel);
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20005, TextFloaterColor.White, &extraText2);
    Logger.Info("d20_mods_spells.c / _begin_aid(): gained {0} temporary hit points", v3);
    condArg2 = evt.GetConditionArg2();
    v7 = evt.GetConditionArg1();
    if ( !evt.objHndCaller.AddCondition("Temporary_Hit_Points", v7, condArg2, v3) )
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_divine_power(): unable to add condition");
    }
  }
  else
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_divine_power(): unable to get spell_packet");
  }
}


[DispTypes(DispatcherType.AbilityScoreLevel)]
[TempleDllLocation(0x100ca440)]
public static void   sub_100CA440(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoBonusList dispIo;  int v3;

  dispIo = evt.GetDispIoBonusList();/*INLINED:v2=evt.subDispNode.subDispDef*/  v3 = data1;
  if ( evt.dispKey - 1 == v3 )
  {
    if ( (v3 )!=0)
    {
      if ( v3 == 2 )
      {
        dispIo.bonlist.AddBonus(2, 35, data2);
      }
    }
    else
    {
      dispIo.bonlist.AddBonus(4, 35, data2);
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cf070)]
public static void   RageBeginSpell(in DispatcherCallbackArgs evt)
{
  int condArg1;


  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    if ( !spellPkt.caster.AddCondition("sp-Concentrating", condArg1, 0, 0) )
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_rage(): unable to add condition to spell_caster");
    }
  }
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20046, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.DealingDamage2)]
[TempleDllLocation(0x100cb480)]
public static void   d20_mods_spells_vampiric_touch_add_temp_hp(in DispatcherCallbackArgs evt, int data)
{
  DispIoDamage dispIo;
  int condArg3;
  int v3;
  int v4;

  if ( evt.GetDispIoDamage().attackPacket.d20ActnType == D20ActionType.TOUCH_ATTACK)
  {
    dispIo = evt.GetDispIoDamage();
    condArg3 = evt.GetConditionArg3();
    v3 = dispIo.damage.finalDamage;
    if ( v3 > 0 )
    {
      evt.SetConditionArg3(v3 + condArg3);
    }
    v4 = evt.GetConditionArg3();
    Logger.Info("d20_mods_spells.c / _vampiric_touch_add_temp_hp(): took ({0}) damage, temp_hp=( {1} )", v3, v4);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100d00b0)]
public static void   BeginSpellStinkingCloud(in DispatcherCallbackArgs evt)
{
  int condArg1;
  float radiusInches;
  int objEvtId;  SpellEntry spEntry;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out spEntry);
    radiusInches = (float)(int)spEntry.radiusTarget * 12F;
    spellPkt.aoeObj = evt.objHndCaller;
    objEvtId = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 44, 45, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(objEvtId);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_stinking_cloud(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cc4c0)]
public static void   FloatMessageAfraid(in DispatcherCallbackArgs evt)
{
  if ( (evt.GetConditionArg3() )==0)
  {
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20013, TextFloaterColor.Red);
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c6d60)]
public static void   sub_100C6D60(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;

  dispIo = evt.GetDispIoD20Query();
  if ( (evt.GetConditionArg3() )!=0)
  {
    dispIo.return_val = 1;
    dispIo.obj = evt.GetConditionArg1();
  }
  else
  {
    dispIo.return_val = 0;
  }
}


[DispTypes(DispatcherType.GetDefenderConcealmentMissChance)]
[TempleDllLocation(0x100c7da0)]
public static void   FogCloudConcealmentMissChance(in DispatcherCallbackArgs evt)
{
  DispIoAttackBonus dispIo;
  BonusList *v2;
  float v3;

  dispIo = evt.GetDispIoAttackBonus();
  v3 = LocationExtensions.DistanceToObjInFeet(dispIo.attackPacket.attacker, dispIo.attackPacket.victim);
  if ( !GameSystems.D20.D20Query(dispIo.attackPacket.attacker, D20DispatcherKey.QUE_Critter_Has_True_Seeing) )
  {
    v2 = &dispIo.bonlist;
    if ( v3 <= 5F)
    {
      v2.AddBonus(20, 19, 233);
    }
    else
    {
      v2.AddBonus(50, 19, 233);
    }
  }
}


[DispTypes(DispatcherType.AbilityScoreLevel)]
[TempleDllLocation(0x100c6230)]
public static void   sub_100C6230(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoBonusList dispIo;

  dispIo = evt.GetDispIoBonusList();
  if ( evt.dispKey - 1 == evt.GetConditionArg3() )
  {
    dispIo.bonlist.AddBonus(-6, 0, data2);
  }
}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100c9580)]
public static void   d20_mods_spells__protection_from_alignment_prevent_damage(in DispatcherCallbackArgs evt, int data)
{
  DispIoDamage dispIo;
  DispIoDamage v2;
  int v3;
  GameObjectBody v4;
  GameObjectBody v5;
  int condArg1;
  string v7;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoDamage();
  v2 = dispIo;
  v3 = dispIo.attackPacket.attacker;
  if ( __PAIR__(HIDWORD(v2.attackPacket.attacker), v3) )
  {
    v4 = GameSystems.Item.GetItemAtInvIdx(__PAIR__(HIDWORD(v2.attackPacket.attacker), v3), 203);
    v5 = GameSystems.Item.GetItemAtInvIdx(v2.attackPacket.attacker, 204);
    if ( (v4 == null)&& (v5 == null))
    {
      condArg1 = evt.GetConditionArg1();
      if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
      {
        if ( v2.attackPacket.attacker.HasCondition(SpellEffects.SpellSummoned) )
        {
          switch ( spellPkt.spellEnum )
          {
            case WellKnownSpells.ProtectionFromChaos:
              if ( v2.attackPacket.attacker.HasLawfulAlignment() )
              {
                goto LABEL_14;
              }
              break;
            case WellKnownSpells.ProtectionFromEvil:
              if ( !(v2.attackPacket.attacker.HasGoodAlignment()) )
              {
                goto LABEL_14;
              }
              break;
            case WellKnownSpells.ProtectionFromGood:
              if ( !(v2.attackPacket.attacker.HasEvilAlignment()) )
              {
                goto LABEL_14;
              }
              break;
            case WellKnownSpells.ProtectionFromLaw:
              if ( v2.attackPacket.attacker.HasChaoticAlignment() )
              {
LABEL_14:
                if ( !D20ModSpells.CheckSpellResistance(&spellPkt, evt.objHndCaller) )
                {
                  v2.damage.AddModFactor(0F, DamageType.Unspecified, 0x68);
                }
              }
              break;
            default:
              v7 = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
              Logger.Info("d20_mods_spells.c / _protection_from_alignment_prevent_damage(): invalid spell=( {0} )", v7);
              break;
          }
        }
      }
      else
      {
        Logger.Info("d20_mods_spells.c / _protection_from_alignment_prevent_damage(): unable to retrieve spell_packet");
      }
    }
  }
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100c34c0)]
public static void   ConcentratingRadialMenu(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  int v2;
  string meslineValue;
int meslineKey;
  RadialMenuEntry radMenuEntry;

  radMenuEntry = RadialMenuEntry.Create();
  radMenuEntry.d20ActionType = D20ActionType.STOP_CONCENTRATION;
  radMenuEntry.d20ActionData1 = 0;
  meslineKey = 5060;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  radMenuEntry.text = (string )meslineValue;
  radMenuEntry.helpSystemHashkey = "TAG_STOP_CONCENTRATION"/*ELFHASH*/;
  v2 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Spells);
  GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v2);
}


[DispTypes(DispatcherType.TakingDamage)]
[TempleDllLocation(0x100c9220)]
public static void   OtilukesSphereOnDamage(in DispatcherCallbackArgs evt)
{
  DispIoDamage dispIo;

  int condArg1;

  dispIo = evt.GetDispIoDamage();
  GameSystems.ParticleSys.CreateAtObj("sp-Otilukes Resilient Sphere-Hit", evt.objHndCaller);
  condArg1 = evt.GetConditionArg1();
  GameSystems.Script.Spells.SpellTrigger(condArg1, OnSpellStruck);
  dispIo.damage.AddModFactor(0F, DamageType.Unspecified, 0x68);
}


[DispTypes(DispatcherType.GetBonusAttacks)]
[TempleDllLocation(0x100c87c0)]
[TemplePlusLocation("spell_condition.cpp:257")]
public static void   HasteBonusAttack(in DispatcherCallbackArgs evt)
{
  DispIoD20ActionTurnBased dispIo;

  dispIo = evt.GetDispIoD20ActionTurnBased();
  ++dispIo.returnVal;
}
/* Orphan comments:
TP Replaced @ spell_condition.cpp:257
*/


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100dd3f0)]
public static void   sub_100DD3F0(in DispatcherCallbackArgs evt, int data)
{
  int condArg3;
  int condArg1;

  int v4;
      SpellPacketBody spellPkt;

  condArg3 = evt.GetConditionArg3();
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.ParticleSys.CreateAtObj("sp-Mirror Image Loss", evt.objHndCaller);
    GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnSpellStruck);
    v4 = condArg3 - 1;
    evt.SetConditionArg3(v4);
    if ( v4 <= 0 )
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                        SpellEffects.Spell_remove_spell(evt.WithoutIO);
                        SpellEffects.Spell_remove_mod(evt.WithoutIO);
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cde50)]
public static void   sub_100CDE50(in DispatcherCallbackArgs evt)
{
  evt.SetConditionArg3(0);
  evt.SetConditionArg4(0);
  evt.SetConditionArg(4, 0);
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100c40a0)]
public static void   ChaosHammer_ToHit_Callback(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;  BonusList *v3;

  dispIo = evt.GetDispIoAttackBonus();/*INLINED:v2=evt.subDispNode.subDispDef*/  v3 = &dispIo.bonlist;
  if ( data2 == 282 )
  {
    v3.AddBonus(-data1, 0, 282);
  }
  else
  {
    v3.AddBonus(data1, 0, data2);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ce940)]
[TemplePlusLocation("spell_condition.cpp:245")]
public static void   AcidDamage(in DispatcherCallbackArgs evt)
{
  int condArg3;
  int condArg1;
  int v3;
  int v4;
  Dice damDice;
  SpellPacketBody spellPkt;

  condArg3 = evt.GetConditionArg3();
  condArg1 = evt.GetConditionArg1();
  GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt);
  if ( condArg3 >= 0 )
  {
    v3 = condArg3 + 1;
    do
    {
      GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 46);
      v4 = spellPkt.spellId;
      damDice = 2.new Dice(4, 0);
      GameSystems.D20.Combat.SpellDamageFull(spellPkt.targetListHandles[0], spellPkt.caster, damDice, DamageType.Acid, 1, D20ActionType.CAST_SPELL, v4, 0);
      evt.SetConditionArg3(0);
      --v3;
    }
    while ( v3 );
  }
}
/* Orphan comments:
TP Replaced @ spell_condition.cpp:245
*/


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100dbd90)]
public static void   sub_100DBD90(in DispatcherCallbackArgs evt)
{

  GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Remove_Disease, 0, 0);
        SpellEffects.Spell_remove_spell(in evt);
        SpellEffects.Spell_remove_mod(in evt);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ceb20)]
public static void   BeginSpellMindFog(in DispatcherCallbackArgs evt)
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
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 24, 25, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_mind_fog(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c6630)]
[TemplePlusLocation("spell_condition.cpp:89")]
public static void   CalmEmotionsActionInvalid(in DispatcherCallbackArgs evt, ConditionSpec data)
{
  DispIoD20Query dispIo;
  D20Action d20a;

  dispIo = evt.GetDispIoD20Query();
  d20a = (D20Action )dispIo.data1;
  if ( IsActionOffensive/*0x1008acc0*/(d20a.d20ActType, d20a.d20ATarget) )
  {
    if ( GameSystems.Critter.IsFriendly(d20a.d20APerformer, d20a.d20ATarget) )
    {
      dispIo.return_val = 1;
      dispIo.data1 = 0;
      dispIo.data2 = 0;
    }
  }
}
/* Orphan comments:
TP Replaced @ spell_condition.cpp:89
*/


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cc2a0)]
public static void   sub_100CC2A0(in DispatcherCallbackArgs evt)
{
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20032, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.InitiativeMod)]
[TempleDllLocation(0x100c5b00)]
public static void   DeafnessInitiativeMod(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoObjBonus dispIo;
  dispIo = evt.GetDispIoObjBonus();/*INLINED:v2=evt.subDispNode.subDispDef*/  if ( data2 == 190 )
  {
    dispIo.bonOut.AddBonus(-data1, 0, 190);
  }
  else
  {
    dispIo.bonOut.AddBonus(data1, 0, data2);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ce320)]
public static void   BeginSpellInvisibilityPurge(in DispatcherCallbackArgs evt)
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
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 18, 19, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_invisibility_purge(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c5cd0)]
[TemplePlusLocation("spell_condition.cpp:344")]
public static void   IsCritterAfraidQuery(in DispatcherCallbackArgs evt)
{
  int condArg1;
  DispIoD20Query dispIo;
  int v3;
  SpellPacketBody spellPkt;

  if ( (evt.GetConditionArg3() )==0)
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      if ( !evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions) )
      {
        dispIo = evt.GetDispIoD20Query();
        dispIo.return_val = 1;
        dispIo.obj = spellPkt.caster;
      }
    }
    else
    {
      v3 = evt.GetConditionArg1();
      Logger.Info("d20_mods_spells.c / _spell_query_cause_fear(): unable to get spell_packet for spell_id {0}", v3);
    }
  }
}
/* Orphan comments:
TP Replaced @ spell_condition.cpp:344
*/


[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x100c7510)]
public static void   sub_100C7510(in DispatcherCallbackArgs evt, int data)
{
  evt.SetConditionArg4(5);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cd0e0)]
public static void   sub_100CD0E0(in DispatcherCallbackArgs evt)
{
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20011, TextFloaterColor.White);
}


[DispTypes(DispatcherType.TurnBasedStatusInit)]
[TempleDllLocation(0x100ca620)]
public static void   SleetStormTurnBasedStatusInit(in DispatcherCallbackArgs evt)
{
  string v1;
  int v2;
  string v3;
  CHAR extraText2;
  BonusList bonlist;

  bonlist = BonusList.Create();
  DispatcherExtensions.dispatch1ESkillLevel(evt.objHndCaller, SkillId.balance, &bonlist, null, 1);
  v1 = GameSystems.Skill.GetSkillName(22);
  v2 = GameSystems.Spell.DispelRoll(evt.objHndCaller, &bonlist, 0, 10, v1, 0);
  v3 = GameSystems.Skill.GetSkillName(22);
  extraText2 = String.Format(" [{0}]", v3);
  if ( v2 < 0 )
  {
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20042, TextFloaterColor.White);
    evt.SetConditionArg4(-v2);
    if ( v2 <= -5 )
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20041, TextFloaterColor.Red);
      evt.objHndCaller.AddCondition(StatusEffects.Prone);
      GameSystems.Anim.PushAnimate(evt.objHndCaller, 64);
    }
  }
  else
  {
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20043, TextFloaterColor.White);
    evt.SetConditionArg4(0);
  }
  nullsub_1/*0x100027f0*/();
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ccf80)]
public static void   BeginSpellDesecrate(in DispatcherCallbackArgs evt)
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
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 6, 7, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_desecrate(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100dd2d0)]
public static void   SavingThrow_sp_Guidance_Callback(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoSavingThrow dispIo;

  if ( evt.GetConditionArg4() > 0 )
  {
    dispIo = evt.GetDispIoSavingThrow();
    dispIo.bonlist.AddBonus(data1, 34, data2);
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(evt.WithoutIO);
                SpellEffects.Spell_remove_mod(evt.WithoutIO);
  }
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100d71a0)]
public static void   FrogTongue_breakfree_callback(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  int v2;
  int condArg1;
  string v4;
  int v5;
  int v6;
  int v7;
  int v8;
    SpellPacketBody spellPkt;
  BonusList bonList;
  DispIoBonusList a1;
  BonusList bonlist;

  evt.GetDispIoD20Signal();
  bonlist = BonusList.Create();
  bonList = BonusList.Create();
  a1 = new DispIoBonusList();
  v1 = evt.objHndCaller.GetStat(0, &a1);
  v2 = D20StatSystem.GetModifierForAbilityScore(v1);
  condArg1 = evt.GetConditionArg1();
  if ( !GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    goto LABEL_9;
  }
  bonList.AddBonus(v2, 0, 103);
  v4 = GameSystems.Stat.GetStatName(0);
  if ( GameSystems.Spell.DispelRoll(evt.objHndCaller, &bonList, 0, 20, v4, 0) < 0 )
  {
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 21002, TextFloaterColor.Red);
LABEL_9:
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _frog_tongue_break_free_check(): unable to save new spell_packet");
    }
*/    return;
  }
  GameSystems.D20.Actions.PerformOnAnimComplete(evt.objHndCaller, -1);
  sub_10020A60/*0x10020a60*/(spellPkt.caster);
  GameObjectRenderExtensions.GetIdleAnimId(spellPkt.caster);
  v5 = GameSystems.Critter.GetAnimId(spellPkt.caster, 9);
  spellPkt.caster.SetAnimId(v5);
  v6 = GameObjectRenderExtensions.GetIdleAnimId(evt.objHndCaller);
  v7 = GameSystems.Critter.GetAnimId(evt.objHndCaller, v6);
  evt.objHndCaller.SetAnimId(v7);
  sub_100D26F0/*0x100d26f0*/(evt.objHndCaller);
  if ( spellPkt.targetListHandles[0] )
  {
    GameSystems.D20.D20SendSignal(spellPkt.targetListHandles[0], D20DispatcherKey.SIG_Spell_Grapple_Removed, spellPkt.spellId, 0);
    GameSystems.D20.D20SendSignal(spellPkt.targetListHandles[0], D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
  }
  GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_Grapple_Removed, spellPkt.spellId, 0);
  GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
  if ( spellPkt.RemoveTarget(evt.objHndCaller) )
  {
    v8 = evt.GetConditionArg1();
    GameSystems.Spell.EndSpell(v8, 0);
                SpellEffects.Spell_remove_mod(evt.WithoutIO);
  }
  else
  {
    Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
  }
}


[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x100d7450)]
public static void   SlipperyMindActivate(in DispatcherCallbackArgs evt, int data)
{
  int condArg2;
  int condArg1;
  GameObjectBody v3;
  string v4;
  string v5;
  int v6;
  GameObjectBody v7;
    DispIoDispelCheck dispIo;
  SpellEntry a2;
  CHAR extraText2;
  SpellPacketBody spellPkt;

  condArg2 = evt.GetConditionArg2();
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
    HIDWORD(v3) = HIDWORD(spellPkt.targetListHandles[condArg2]);
    LODWORD(v3) = spellPkt.targetListHandles[condArg2];
    v4 = (string )&spellPkt.targetListHandles[condArg2];
    if ( GameSystems.D20.Combat.SavingThrowSpell(v3, evt.objHndCaller, spellPkt.dc, a2.savingThrowType, 0, spellPkt.spellId) <= 0 )
    {
      GameSystems.Spell.FloatSpellLine(*(_QWORD *)v4, 30002, TextFloaterColor.White);
    }
    else
    {
      v5 = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
      extraText2 = String.Format(" [{0}]", v5);
      GameSystems.Spell.FloatSpellLine(*(_QWORD *)v4, 30000, TextFloaterColor.White);
      dispIo = new DispIoDispelCheck();
      v6 = *((_DWORD *)v4 + 1);
      dispIo.spellId = spellPkt.spellId;
      HIDWORD(v7) = v6;
      LODWORD(v7) = *(_DWORD *)v4;
      dispIo.returnVal = 0;
      dispIo.flags = 32;
      v7.DispatchDispelCheck(&dispIo);
      nullsub_1/*0x100027f0*/();
    }
  }
        SpellEffects.Spell_remove_mod(in evt);
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c6d10)]
public static void   sub_100C6D10(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;
  int condArg3;

  dispIo = evt.GetDispIoD20Query();
  condArg3 = evt.GetConditionArg3();
  if ( (condArg3 )!=0&& condArg3 != 15 )
  {
    dispIo.data1 = condArg3;
    dispIo.return_val = 1;
    dispIo.data2 = (ulong)condArg3 >> 32;
  }
  else
  {
    dispIo.return_val = 0;
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100c99e0)]
public static void   RepelVerminOnAdd(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int v2;
  int v3;
  Dice v4;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    v2 = GameSystems.Critter.GetHitDiceNum(evt.objHndCaller);
    if ( v2 >= spellPkt.casterLevel / 3 )
    {
      v3 = spellPkt.spellId;
      v4 = 2.new Dice(6, 0);
      GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v4, DamageType.Magic, 1, D20ActionType.CAST_SPELL, v3, 0);
    }
    else
    {
      GameSystems.AI.FleeFrom(evt.objHndCaller, spellPkt.caster);
    }
  }
}


[DispTypes(DispatcherType.GetMoveSpeedBase)]
[TempleDllLocation(0x100c7e70)]
public static void   sub_100C7E70(in DispatcherCallbackArgs evt, int data1, int data2)
{
  int v1;
  DispIoMoveSpeed dispIo;

  v1 = data1;
  dispIo = evt.GetDispIoMoveSpeed();
  dispIo.bonlist.SetOverallCap(1, v1, 0, data2);
  dispIo.bonlist.SetOverallCap(2, v1, 0, data2);
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100caf30)]
[TemplePlusLocation("spell_condition.cpp:264")]
public static void   AoeObjEventStinkingCloud(in DispatcherCallbackArgs evt, int data)
{
  DispIoObjEvent dispIo;
  int condArg1;
  Dice v3;
  int v4;
  unsigned Dice v5;
  int v6;
  Dice v7;
  int v8;
  int v9;
  GameObjectBody v10;

  int v12;




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
        v10 = dispIo.tgt;
        v12 = GameSystems.ParticleSys.CreateAtObj("sp-Stinking Cloud Hit", v10);
        spellPkt.AddTarget(dispIo.tgt, v12, true);
        if ( GameSystems.D20.Combat.SavingThrowSpell(dispIo.tgt, spellPkt.caster, spellPkt.dc, 0, 0, spellPkt.spellId) >= 1 )
        {
          dispIo.tgt.AddCondition("sp-Stinking Cloud Hit Pre", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
        }
        else
        {
          dispIo.tgt.AddCondition("sp-Stinking Cloud Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId, 0);
        }
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE && data == 222 )
      {
        evt.SetConditionArg4(1);
        v3 = 1.new Dice(4, 1);
        v4 = GetPackedDiceBonus/*0x10038c90*/(v3);
        v5 = 1.new Dice(4, 1);
        v6 = GetPackedDiceType/*0x10038c40*/(v5);
        v7 = 1.new Dice(4, 1);
        v8 = GetPackedDiceNumDice/*0x10038c30*/(v7);
        v9 = DiceRoller/*0x10038b60*/(v8, v6, v4);
        evt.SetConditionArg2(v9);
      }
    }
  }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*  else
  {
    Logger.Info("d20_mods_spells.c / _stinking_cloud_hit_trigger(): unable to save new spell_packet");
  }
*/}
/* Orphan comments:
TP Replaced @ spell_condition.cpp:264
*/


[DispTypes(DispatcherType.ConditionAdd, DispatcherType.D20Signal)]
[TempleDllLocation(0x100c8270)]
[TemplePlusLocation("spell_condition.cpp:267")]
public static void   GreaseSlippage(in DispatcherCallbackArgs evt)
{
  int condArg1;
  SpellEntry spEntry;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out spEntry);
    if ( GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc, spEntry.savingThrowType, 0, spellPkt.spellId) < 1 )
    {
      GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x30, evt.objHndCaller, null);
      GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 104);
      evt.objHndCaller.AddCondition(StatusEffects.Prone);
      GameSystems.Anim.PushAnimate(evt.objHndCaller, 64);
    }
  }
}
/* Orphan comments:
TP Replaced @ spell_condition.cpp:267
*/


[DispTypes(DispatcherType.ToHitBonusFromDefenderCondition)]
[TempleDllLocation(0x100cb8e0)]
public static void   sub_100CB8E0(in DispatcherCallbackArgs evt, int data)
{
  DispIoAttackBonus dispIo;

  dispIo = evt.GetDispIoAttackBonus();
  dispIo.bonlist.AddBonus(data, 0, 211);
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100c8780)]
public static void   sub_100C8780(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoSavingThrow dispIo;

  dispIo = evt.GetDispIoSavingThrow();
  if ( evt.dispKey == D20DispatcherKey.SAVE_REFLEX )
  {
    dispIo.bonlist.AddBonus(data1, 8, data2);
  }
}


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x100c97c0)]
public static void   sub_100C97C0(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;

  dispIo = evt.GetDispIoAttackBonus();
  dispIo.bonlist.AddBonus(-data1, 13, data2);
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100de3f0)]
[TemplePlusLocation("condition.cpp:3591")]
public static void   SpellDismissSignalHandler(in DispatcherCallbackArgs evt, int data)
{
  DispIoD20Signal dispIo;
  int condArg1;
  int i;
          SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoD20Signal();
  if ( dispIo !=null)
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt)       && (evt.objHndCaller == spellPkt.caster || dispIo.data1 == spellPkt.spellId && (dispIo.data2)==0) )
    {
      if ( spellPkt.spellEnum == WellKnownSpells.MirrorImage|| data == 1 )
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
}
/* Orphan comments:
TP Replaced @ condition.cpp:3591
*/


[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x100d6850)]
public static void   StinkingCloudPreBeginRound(in DispatcherCallbackArgs evt, int data)
{
  int condArg1;
  string v2;

  int v5;


  int condArg3;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    if ( GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc, 0, 0, spellPkt.spellId) >= 1 )
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
        Logger.Info("d20_mods_spells.c / _stinking_cloud_pre_fort_save(): cannot remove target");
        return;
      }
                        SpellEffects.Spell_remove_mod(in evt);
      v5 = GameSystems.ParticleSys.CreateAtObj("sp-Stinking Cloud Hit", evt.objHndCaller);
      spellPkt.AddTarget(evt.objHndCaller, v5, true);
      condArg3 = evt.GetConditionArg3();
      evt.objHndCaller.AddCondition("sp-Stinking Cloud Hit", spellPkt.spellId, spellPkt.durationRemaining, condArg3);
    }
  }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*  else
  {
    Logger.Info("d20_mods_spells.c / _stinking_cloud_pre_fort_save(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.AbilityScoreLevel)]
[TempleDllLocation(0x100c60e0)]
public static void   StatLevel_callback_AnimalGrowth(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoBonusList dispIo;  int v3;
  BonusList *v4;

  dispIo = evt.GetDispIoBonusList();/*INLINED:v2=evt.subDispNode.subDispDef*/  v3 = data1;
  if ( evt.dispKey - 1 == v3 )
  {
    v4 = &dispIo.bonlist;
    if ( v3 == 1 )
    {
      v4.AddBonus(-data2, 35, 274);
    }
    else
    {
      v4.AddBonus(data2, 35, 274);
    }
  }
}


[DispTypes(DispatcherType.Tooltip)]
[TempleDllLocation(0x100c3530)]
public static void   ConcentratingTooltipCallback(in DispatcherCallbackArgs evt, int data)
{
  DispIoTooltip dispIo;
  int condArg1;
  int v3;
  string spellLine;
  int v5;
  string meslineValue;
int meslineKey;
  CHAR textbuf[256];
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoTooltip();
  meslineKey = data;
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
    spellLine = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
    textbuf = String.Format("{0}[{1}]", meslineValue, spellLine);
    v5 = dispIo.numStrings;
    if ( v5 < 10 )
    {
      dispIo.numStrings = v5 + 1;
      strncpy(dispIo.strings[v5].text, textbuf, 0x100);
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cdd50)]
public static void   sub_100CDD50(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int v2;
  int v3;


  int condArg2;
  int v7;
  CHAR extraText;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    v2 = spellPkt.casterLevel;
    if ( spellPkt.casterLevel >= 20 )
    {
      v2 = 20;
    }
    v3 = DiceRoller/*0x10038b60*/(0, 0, v2);
    extraText = String.Format("[{0}] ", v3);
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20005, TextFloaterColor.White, &extraText);
    Logger.Info("d20_mods_spells.c / _begin_spell_greater_heroism(): gained {0} temporary hit points", v3);
    condArg2 = evt.GetConditionArg2();
    v7 = evt.GetConditionArg1();
    if ( !evt.objHndCaller.AddCondition("Temporary_Hit_Points", v7, condArg2, v3) )
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_greater_heroism(): unable to add condition");
    }
  }
}


[DispTypes(DispatcherType.GetDefenderConcealmentMissChance)]
[TempleDllLocation(0x100c91a0)]
public static void   ObscuringMist_Concealment_Callback(in DispatcherCallbackArgs evt)
{
  DispIoAttackBonus dispIo;
  BonusList *v2;
  float v3;

  dispIo = evt.GetDispIoAttackBonus();
  v3 = LocationExtensions.DistanceToObjInFeet(dispIo.attackPacket.attacker, dispIo.attackPacket.victim);
  if ( !GameSystems.D20.D20Query(dispIo.attackPacket.attacker, D20DispatcherKey.QUE_Critter_Has_True_Seeing) )
  {
    v2 = &dispIo.bonlist;
    if ( v3 <= 5F)
    {
      v2.AddBonus(20, 19, 238);
    }
    else
    {
      v2.AddBonus(50, 19, 238);
    }
  }
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100c4140)]
public static void   SavingThrowPenaltyCallback(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoSavingThrow dispIo;  BonusList *v3;

  dispIo = evt.GetDispIoSavingThrow();/*INLINED:v2=evt.subDispNode.subDispDef*/  v3 = &dispIo.bonlist;
  if ( data2 == 169 )
  {
    v3.AddBonus(-data1, 0, 169);
  }
  else
  {
    v3.AddBonus(data1, 0, data2);
  }
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100cac00)]
public static void   SpikeStonesHitCombatCritterMovedHandler(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int v2;
  int v3;

  int v5;
  Dice v6;


  int condArg3;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  condArg3 = evt.GetConditionArg3();
  v2 = evt.GetDispIoD20Signal().data1;
  if ( v2 > 0 )
  {
    v3 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(v3, out spellPkt) )
    {
      GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
      GameSystems.SoundGame.PositionalSound(15127, 1, evt.objHndCaller);
      v5 = spellPkt.spellId;
      v6 = v2 / 5.new Dice(8, 0);
      GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v6, DamageType.Magic, 1, D20ActionType.CAST_SPELL, v5, 0);
      if ( !GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc, SavingThrowType.Reflex, 0, spellPkt.spellId) )
      {
        if ( !evt.objHndCaller.AddCondition("sp-Spike Stones Damage", condArg1, 14400, condArg3) )
        {
          Logger.Info("d20_mods_spells.c / _spike_stones_hit(): unable to add condition");
        }
      }
    }
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100c4970)]
public static void   sub_100C4970(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoDamage dispIo;
  DispIoDamage v2;
  int v3;
  int condArg3;
  int v5;
  int condArg4;

  dispIo = evt.GetDispIoDamage();
  v2 = dispIo;
  switch ( data2 )
  {
    case 0xD2:
      if ( (dispIo.attackPacket.weaponUsed == null))
      {
        v3 = data2;
        condArg3 = evt.GetConditionArg3();
        v2.damage.AddDamageBonus(condArg3, 12, v3);
      }
      break;
    case 0xD4:
      if ( dispIo.attackPacket.weaponUsed !=null|| (evt.objHndCaller.GetStat(Stat.level_monk) )!=0)
      {
        v5 = data2;
        condArg4 = evt.GetConditionArg4();
        v2.damage.AddDamageBonus(condArg4, 12, v5);
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
      dispIo.damage.AddDamageBonus(data1, 12, data2);
      break;
    case 0xD0:
      if ( dispIo.attackPacket.weaponUsed !=null|| (evt.objHndCaller.GetStat(Stat.level_monk) )!=0)
      {
        v2.damage.AddDamageBonus(data1, 12, data2);
      }
      break;
  }
}


[DispTypes(DispatcherType.GetMoveSpeedBase)]
[TempleDllLocation(0x100ca5e0)]
public static void   SleetStormHitMovementSpeed(in DispatcherCallbackArgs evt, int data1, int data2)
{
  int condArg4;
  DispIoMoveSpeed dispIo;
  double previousFactor;

  condArg4 = evt.GetConditionArg4();
  dispIo = evt.GetDispIoMoveSpeed();
  previousFactor = dispIo.factor;
  if ( (condArg4 )!=0)
  {
    dispIo.factor = previousFactor * 0F;
  }
  else
  {
    dispIo.factor = previousFactor * 0.5F;
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100c8a70)]
public static void   InvisibilitySphereHitBegin(in DispatcherCallbackArgs evt)
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
    Logger.Info("d20_mods_spells.c / _invisibility_sphere_hit(): unable to add condition");
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100dbb30)]
public static void   WeaponEnhBonusOnAdd(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg3;

  GameObjectBody parent;

  parent = null;
  if ( !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Item_Has_Enhancement_Bonus) )
  {
    condArg1 = evt.GetConditionArg1();
    condArg3 = evt.GetConditionArg3();
    AddItemConditionToWielder/*0x100d2f30*/(evt.objHndCaller, "Weapon Enhancement Bonus", condArg3, 0, 0, 0, condArg1);
    GameSystems.Item.GetParent(evt.objHndCaller, &parent);
    GameSystems.D20.Status.initItemConditions(parent);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cc640)]
public static void   sub_100CC640(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg2;
  int condArg3;



  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  condArg3 = evt.GetConditionArg3();
  if ( !evt.objHndCaller.AddCondition("Charmed", condArg1, condArg2, condArg3) )
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_charm_person_or_animal(): unable to add condition");
  }
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20018, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cc040)]
public static void   AnimateDeadOnAdd(in DispatcherCallbackArgs evt)
{
  int condArg3;  int v3;
  int condArg1;
  GameObjectBody v5;
  int v6;
  locXY v7;
  GameObjectBody *v8;
  GameObjectBody handleNew;
  SpellPacketBody spellPkt;

  condArg3 = evt.GetConditionArg3();/*INLINED:v2=evt.subDispNode.condNode*/  v3 = condArg3;
  handleNew = null;
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.D20.Initiative.RemoveFromInitiative(spellPkt.targetListHandles[0]);
    if ( GameSystems.Party.IsInParty(spellPkt.targetListHandles[0]) )
    {
      GameSystems.Critter.RemoveFollower(spellPkt.targetListHandles[0], 1);
      GameUiBridge.UpdatePartyUi();
    }
    GameSystems.Item.PoopInventory(spellPkt.targetListHandles[0], 1);
    GameSystems.MapObject.SetFlags(spellPkt.targetListHandles[0], ObjectFlag.OFF);
    if ( v3 == 1 )
    {
      v8 = &handleNew;
      v7 = spellPkt.aoeCenter.location;
      v5 = GameSystems.Proto.GetProtoById(14107);
    }
    else
    {
      if ( v3 != 2 )
      {
        return;
      }
      v8 = &handleNew;
      v7 = spellPkt.aoeCenter.location;
      v5 = GameSystems.Proto.GetProtoById(14123);
    }
    if ( (GameSystems.MapObject.CreateObject(v5, v7, v8) == null))
    {
      handleNew = null;
    }
    if ( GameSystems.Critter.AddFollower(handleNew, spellPkt.caster, 1, 1) )
    {
      GameSystems.D20.Initiative.AddToInitiative(handleNew);
      v6 = GameSystems.D20.Initiative.GetInitiative(spellPkt.caster);
      GameSystems.D20.Initiative.SetInitiative(handleNew, v6);
      GameUiBridge.UpdateInitiativeUi();
      GameUiBridge.UpdatePartyUi();
      Logger.Info("animate dead: new_obj=( {0} )", handleNew);
    }
    else
    {
      Logger.Info("animate dead: failed to add obj to party!");
      GameSystems.Object.Destroy(handleNew);
    }
  }
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100c58a0)]
public static void   sub_100C58A0(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;  int v3;

  dispIo = evt.GetDispIoAttackBonus();/*INLINED:v2=evt.subDispNode.subDispDef*/  switch ( data2 )
  {
    case 0xAB:
    case 0xDF:
    case 0x102:
      v3 = -data1;
      goto LABEL_6;
    case 0xAD:
      if ( !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement) )
      {
        dispIo.bonlist.AddBonus(-data1, 0, data2);
      }
      break;
    default:
      v3 = data1;
LABEL_6:
      dispIo.bonlist.AddBonus(v3, 0, data2);
      break;
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ce590)]
public static void   SpLesserRestorationOnConditionAdd(in DispatcherCallbackArgs evt)
{
  long v1;
  int condArg3;  Dice v4;
  int v5;
  unsigned Dice v6;
  int v7;
  Dice v8;
  int v9;
  int v10;
  int v11;
  string v12;
  string v13;
    DispIoAbilityLoss dispIo;
  CHAR extraText2;

  dispIo = new DispIoAbilityLoss();
  LODWORD(v1) = evt.subDispNode;
  condArg3 = evt.GetConditionArg3();/*INLINED:v3=evt.subDispNode.condNode*/  dispIo.statDamaged = (Stat)condArg3;
  dispIo.flags |= 9;
  dispIo.field_C = 1;
  dispIo.spellId = evt.GetConditionArg1();
  v4 = 1.new Dice(4, 0);
  v5 = GetPackedDiceBonus/*0x10038c90*/(v4);
  v6 = 1.new Dice(4, 0);
  v7 = GetPackedDiceType/*0x10038c40*/(v6);
  v8 = 1.new Dice(4, 0);
  v9 = GetPackedDiceNumDice/*0x10038c30*/(v8);
  dispIo.result = DiceRoller/*0x10038b60*/(v9, v7, v5);
  v10 = dispIo.result;
  v11 = v10 - evt.objHndCaller.DispatchGetAbilityLoss(&dispIo);
  v12 = GameSystems.Stat.GetStatName(dispIo.statDamaged);
  Logger.Info("d20_mods_spells.c / _begin_spell_lesser_restoration(): used {0}/{1} points to heal ({2}) damage", v11, v10, v12);
  v13 = GameSystems.Stat.GetStatName(dispIo.statDamaged);
  extraText2 = String.Format(": {0} [{1}]", v13, v11);
  HIDWORD(v1) = evt.objHndCaller;
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20035, TextFloaterColor.White);
  nullsub_1/*0x100027f0*/();
  *(_QWORD *)&v14[16] = (int)evt.dispKey;
  *(_QWORD *)&v14[8] = *(GameObjectBody *)((string )&evt.objHndCaller + 4);
  *(_QWORD *)v14 = v1;
  SpellEffects.Spell_remove_mod(in evt);
}


[DispTypes(DispatcherType.ConditionAddPre)]
[TempleDllLocation(0x100dba20)]
public static void   RemoveSpellWhenPreAddThis(in DispatcherCallbackArgs evt, ConditionSpec data)
{
  DispIoCondStruct dispIo;

  dispIo = evt.GetDispIoCondStruct();
  if ( dispIo.condStruct == (ConditionSpec )data )
  {
        dispIo.outputFlag = 0;
            SpellEffects.Spell_remove_spell(in evt);
                    SpellEffects.Spell_remove_mod(in evt);
  }
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100c7300)]
public static void   sub_100C7300(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoSavingThrow dispIo;

  dispIo = evt.GetDispIoSavingThrow();
  dispIo.bonlist.AddBonus(data1, 17, data2);
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100c4a90)]
public static void   DivineFavorToHitBonus2(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;
  int condArg3;

  dispIo = evt.GetDispIoAttackBonus();
  if ( data2 == 170 )
  {
    condArg3 = evt.GetConditionArg3();
    dispIo.bonlist.AddBonus(condArg3, 14, 170);
  }
  else
  {
    dispIo.bonlist.AddBonus(data1, 14, data2);
  }
}


[DispTypes(DispatcherType.ConditionAddPre)]
[TempleDllLocation(0x100c7e20)]
public static void   AoESpellPreAddCheck(in DispatcherCallbackArgs evt, ConditionSpec data)
{
  DispIoCondStruct dispIo;
  int v2;

  dispIo = evt.GetDispIoCondStruct();
  v2 = data;
  if ( dispIo.condStruct == (ConditionSpec )v2
    && evt.objHndCaller.HasCondition(v2) )
  {
    dispIo.outputFlag = 0;
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cdc00)]
public static void   sub_100CDC00(in DispatcherCallbackArgs evt)
{
  evt.GetConditionArg3();
  evt.SetConditionArg3(1);
}


[DispTypes(DispatcherType.GetAttackDice)]
[TempleDllLocation(0x100ca2b0)]
[TemplePlusLocation("condition.cpp:447")]
public static void   AttackDiceEnlargePerson(in DispatcherCallbackArgs evt, int data1, int data2)
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
  if ( dispIo.weapon !=null)
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
/* Orphan comments:
TP Replaced @ condition.cpp:447
*/


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cbfa0)]
public static void   AnimalTranceBeginSpell(in DispatcherCallbackArgs evt)
{
  int condArg1;


  SpellPacketBody spPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spPkt) )
  {
    if ( !spPkt.caster.AddCondition("sp-Concentrating", condArg1, 0, 0) )
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_animal_trance(): unable to add condition to spell_caster");
    }
  }
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20021, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.TakingDamage)]
[TempleDllLocation(0x100c8f40)]
public static void   sub_100C8F40(in DispatcherCallbackArgs evt)
{
  DispIoDamage dispIo;


  dispIo = evt.GetDispIoDamage();
  GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
  dispIo.damage.AddModFactor(0F, DamageType.Unspecified, 0x68);
}


[DispTypes(DispatcherType.EffectTooltip)]
[TempleDllLocation(0x100c3dd0)]
public static void   EffectTooltip_Duration_Callback(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoEffectTooltip dispIo;
  int condArg1;
  int v3;
  int v4;
  string v5;
  CHAR extraString;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoEffectTooltip();
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    v3 = spellPkt.duration;
    v4 = spellPkt.durationRemaining;
    v5 = GameSystems.D20.Combat.GetCombatMesLine(combat_mes_duration);
    extraString = String.Format(" {0}: {1}/{2}", v5, v4, v3);
    EffectTooltipAppend/*0x100f4680*/(dispIo.bdb, data1, spellPkt.spellEnum, &extraString);
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c9160)]
public static void   sub_100C9160(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;

  dispIo = evt.GetDispIoD20Query();
  dispIo.return_val = evt.GetConditionArg3();
  dispIo.data1 = evt.GetConditionArg1();
  dispIo.data2 = 0;
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100c65b0)]
public static void   CallLightningStormRadial(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  string v2;
  int v3;
  string meslineValue;
int meslineKey;
  RadialMenuEntry radMenuEntry;

  radMenuEntry = RadialMenuEntry.Create();
  radMenuEntry.d20ActionType = D20ActionType.SPELL_CALL_LIGHTNING;
  radMenuEntry.d20ActionData1 = 0;
  meslineKey = 108;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  radMenuEntry.text = (string )meslineValue;
  v2 = GameSystems.Spell.GetSpellHelpTopic(560);
  radMenuEntry.helpSystemHashkey = v2/*ELFHASH*/;
  v3 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Spells);
  GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v3);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cedd0)]
public static void   BeginSpellObscuringMist(in DispatcherCallbackArgs evt)
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
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 28, 29, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_obscuring_mist(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cc220)]
public static void   sub_100CC220(in DispatcherCallbackArgs evt)
{
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20007, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100c5a80)]
public static void   SavingThrow_sp_Slow_Callback(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoSavingThrow dispIo;

  dispIo = evt.GetDispIoSavingThrow();
  if ( evt.dispKey == D20DispatcherKey.SAVE_REFLEX )
  {
    if ( data2 == 173 )
    {
      if ( !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement) )
      {
        dispIo.bonlist.AddBonus(-data1, 13, data2);
      }
    }
    else
    {
      dispIo.bonlist.AddBonus(data1, 13, data2);
    }
  }
}


[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x100de2a0)]
public static void   FrongTongueSwallowedDamage(in DispatcherCallbackArgs evt, int data)
{
  int condArg1;
      int v4;
  int v5;
  int v6;
  Dice v7;
  int v8;
  Dice v9;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    if ( GameSystems.Critter.IsDeadNullDestroyed(spellPkt.caster) )
    {
      *(_DWORD *)&v2[20] = 0;
      *(_QWORD *)&v2[12] = *(_QWORD *)&evt.dispType;
      *(_QWORD *)&v2[4] = evt.objHndCaller;
      *(_DWORD *)v2 = evt.subDispNode;
      SpellEffects.Spell_remove_spell(in evt);
                        SpellEffects.Spell_remove_mod(evt.WithoutIO);
    }
    else
    {
      v4 = 1;
      v5 = 3;
      if ( spellPkt.caster.GetStat(Stat.size) > 6 )
      {
        v4 = 2;
        v5 = 4;
      }
      v6 = spellPkt.spellId;
      v7 = v4.new Dice(v5, 0);
      GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v7, 0, 1, D20ActionType.CAST_SPELL, v6, 0);
      v8 = spellPkt.spellId;
      v9 = v4.new Dice(v5, 0);
      GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v9, DamageType.Acid, 1, D20ActionType.CAST_SPELL, v8, 0);
    }
  }
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d3de0)]
public static void   Condition__36__control_plants_sthg(in DispatcherCallbackArgs evt, int data)
{
  DispIoObjEvent dispIo;
  int condArg1;


  int v6;
  int v7;
  int v8;
  GameObjectBody v9;

  int v11;




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
        if ( GameSystems.Critter.IsFriendly(spellPkt.caster, dispIo.tgt) )
        {
          dispIo.tgt.AddCondition("sp-Control Plants Disentangle", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
        }
        else
        {
          v6 = GameSystems.D20.Combat.SavingThrowSpell(dispIo.tgt, spellPkt.caster, spellPkt.dc, SavingThrowType.Will, 0, spellPkt.spellId);
          v7 = dispIo.tgt;
          v8 = HIDWORD(dispIo.tgt);
          if ( (v6 )!=0)
          {
            GameSystems.Spell.FloatSpellLine(__PAIR__(v8, v7), 30001, TextFloaterColor.White);
            spellPkt.AddTarget(dispIo.tgt, 0, true);
            dispIo.tgt.AddCondition("sp-Control Plants Entangle Pre", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
          }
          else
          {
            GameSystems.Spell.FloatSpellLine(__PAIR__(v8, v7), 30002, TextFloaterColor.White);
            v9 = dispIo.tgt;
            v11 = GameSystems.ParticleSys.CreateAtObj("sp-Entangle", v9);
            spellPkt.AddTarget(dispIo.tgt, v11, true);
            dispIo.tgt.AddCondition("sp-Control Plants Entangle", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
          }
        }
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        *(_DWORD *)&v3[20] = evt.dispIO;
        *(_QWORD *)&v3[12] = __PAIR__(19, evt.dispType);
        *(_QWORD *)&v3[4] = evt.objHndCaller;
        *(_DWORD *)v3 = evt.subDispNode;
        SpellEffects.Spell_remove_mod(in evt);
      }
    }
  }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*  else
  {
    Logger.Info("d20_mods_spells.c / _control_plants_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.AbilityScoreLevel)]
[TempleDllLocation(0x100cb650)]
public static void   sub_100CB650(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoBonusList dispIo;

  dispIo = evt.GetDispIoBonusList();
  if ( evt.dispKey - 1 == data1
    && !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement) )
  {
    dispIo.bonlist.AddBonus(-4, 0, data2);
  }
}


[DispTypes(DispatcherType.GetMoveSpeedBase)]
[TempleDllLocation(0x100cabc0)]
public static void   sub_100CABC0(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoMoveSpeed dispIo;

  dispIo = evt.GetDispIoMoveSpeed();
  dispIo.factor = dispIo.factor * 0.5F;
}


[DispTypes(DispatcherType.GetSizeCategory)]
[TempleDllLocation(0x100c6140)]
public static void   EnlargeSizeCategory(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoD20Query dispIo;
  int v2;

  dispIo = evt.GetDispIoD20Query();
  v2 = dispIo.return_val;
  if ( v2 < 10 )
  {
    dispIo.return_val = v2 + 1;
  }
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x100c9370)]
public static void   SkillLevelPrayer(in DispatcherCallbackArgs evt, int data)
{
  int condArg3;
  int v2;
  int v3;
  DispIoObjBonus dispIo;

  condArg3 = evt.GetConditionArg3();
  v2 = data;
  v3 = condArg3;
  dispIo = evt.GetDispIoObjBonus();
  if ( v3 == -3 )
  {
    dispIo.bonOut.AddBonus(v2, 14, 151);
  }
  else
  {
    dispIo.bonOut.AddBonus(-v2, 14, 151);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100dbdf0)]
public static void   RemoveSpellOnAdd(in DispatcherCallbackArgs evt)
{

          SpellEffects.Spell_remove_spell(in evt);
        SpellEffects.Spell_remove_mod(in evt);
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100ddf40)]
public static void   WebBurningDamage(in DispatcherCallbackArgs evt, int data)
{
  int condArg1;
  GameObjectBody v2;

  int i;
  int v5;
  Dice v6;
  GameObjectBody v7;
  GameObjectBody v8;

      SpellPacketBody spellPkt;

  evt.GetDispIoD20Signal();
  if ( evt.GetConditionArg(4) != 1 )
  {
    evt.SetConditionArg(4, 1);
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      v2 = spellPkt.aoeObj;
      GameSystems.ParticleSys.CreateAtObj("sp-Web Flamed", v2);
      for ( i = 0; i < spellPkt.targetCount; ++i )
      {
        v5 = spellPkt.spellId;
        v6 = 2.new Dice(4, 0);
        HIDWORD(v7) = HIDWORD(spellPkt.targetListHandles[i]);
        LODWORD(v7) = spellPkt.targetListHandles[i];
        GameSystems.D20.Combat.SpellDamageFull(v7, null, v6, DamageType.Fire, 1, D20ActionType.CAST_SPELL, v5, 0);
        HIDWORD(v8) = HIDWORD(spellPkt.targetListHandles[i]);
        LODWORD(v8) = spellPkt.targetListHandles[i];
        GameSystems.ParticleSys.CreateAtObj("sp-Flame Tongue-hit", v8);
      }
    }
    *(_DWORD *)&v10[20] = 0;
    *(_QWORD *)&v10[12] = *(_QWORD *)&evt.dispType;
    *(_QWORD *)&v10[4] = evt.objHndCaller;
    *(_DWORD *)v10 = evt.subDispNode;
    SpellEffects.Spell_remove_spell(in evt);
                SpellEffects.Spell_remove_mod(evt.WithoutIO);
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c7f00)]
public static void   GaseousFormSpellInterruptedQuery(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;
  int mmData;
  int a5;
  int spellClassCode;
  int spellEnum;
  SpellStoreData spellData;

  a5 = 0;
  mmData = 0;
  dispIo = evt.GetDispIoD20Query();
  if ( dispIo.return_val != 1 )
  {
    GameSystems.D20.RadialMenu.SelectedRadialMenuEntry.d20SpellData.SpellEnum((D20SpellData *)dispIo.data1, &spellEnum, 0, &spellClassCode, &a5, 0, &mmData);
    EncodeSpellData/*0x10075280*/(spellEnum, a5, spellClassCode, 0, mmData, &spellData);
    if ( GameSystems.Spell.GetSpellComponentRegardMetamagic(&spellData) & 0xB )
    {
      GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x26, evt.objHndCaller, null);
      GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 100);
      dispIo.return_val = 1;
    }
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100c4d00)]
public static void   EmotionDamageBonus(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoDamage dispIo;
  dispIo = evt.GetDispIoDamage();/*INLINED:v2=evt.subDispNode.subDispDef*/  switch ( data2 )
  {
    case 0xA9:
      dispIo.damage.AddDamageBonus(-data1, 13, data2);
      break;
    case 0xAC:
    case 0x103:
      if ( !evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions)
        && !evt.objHndCaller.HasCondition(SpellEffects.SpellRemoveFear) )
      {
        dispIo.damage.AddDamageBonus(-data1, 13, data2);
      }
      break;
    case 0x104:
      dispIo.damage.AddDamageBonus(data1, 13, data2);
      break;
    default:
      dispIo.damage.AddDamageBonus(data1, 13, data2);
      break;
  }
}


[DispTypes(DispatcherType.TurnBasedStatusInit)]
[TempleDllLocation(0x100cb910)]
public static void   VrockSporesDamage(in DispatcherCallbackArgs evt, int data)
{
  int condArg1;
  int v2;
  Dice v3;
  SpellPacketBody spellPkt;

  evt.GetDispIOTurnBasedStatus();
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt)     && !evt.objHndCaller.HasCondition(SpellEffects.SpellDelayPoison) )
  {
    if ( GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Immune_Poison) )
    {
      GameSystems.Spell.PlayFizzle(evt.objHndCaller);
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 32000, TextFloaterColor.White);
    }
    else
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20501, TextFloaterColor.Red);
      v2 = spellPkt.spellId;
      v3 = 1.new Dice(2, 0);
      GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v3, DamageType.Poison, 1, D20ActionType.CAST_SPELL, v2, 0);
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100d0610)]
public static void   BeginSpellWindWall(in DispatcherCallbackArgs evt)
{
  int condArg1;
  float radiusInches;
  int v3;  SpellEntry a2;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
    radiusInches = (float)spellPkt.casterLevel * 36F;
    spellPkt.aoeObj = evt.objHndCaller;
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 48, 49, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_wind_wall(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.TakingDamage)]
[TempleDllLocation(0x100ca9a0)]
public static void   SolidFogDamageResistanceVsRanged(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoDamage dispIo;
  int damTotal;

  dispIo = evt.GetDispIoDamage();
  if ( (dispIo.attackPacket.flags & D20CAF.RANGED )!=0)
  {
    damTotal = dispIo.damage.GetOverallDamageByType();
    dispIo.damage.AddDR(damTotal, DamageType.Unspecified, 104);
    dispIo.damage.finalDamage = dispIo.damage.GetOverallDamageByType();
  }
}


[DispTypes(DispatcherType.SpellImmunityCheck)]
[TempleDllLocation(0x100ede16)]
public static void   ImmunityCheckHandler(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoImmunity dispIo;
  int condArg1;
  int v3;
  bool v4;
  SpellPacketBody v5;
  string v6;
  int v7;
  int v8;
  string v9;

  string v11;
  string v12;
  string v13;
  string v14;
  int v15;
  string v16;
  string v17;
  int v18;
  string v19;
  string v20;
  SpellPacketBody v21;
  int v22;
  int v23;
  string v24;

  string v26;
  int v27;
  SpellPacketBody v28;
  int v29;
  int v30;
  string v31;

  SpellPacketBody v33;
  bool v34;
  SpellPacketBody v35;
  int v36;
  int v37;
  string v38;

  int v40;
  string v41;
  int rollHistoryIdxOut;
  DispIoBonusAndSpellEntry v43;
  DispIoTypeImmunityTrigger pDispIO;
  SpellEntry v45;
  CHAR suffix;
  SpellEntry a2;
  BonusList bonlist;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoImmunity();
  if ( dispIo.returnVal != 1 )
  {
    DispIoTypeImmunityTrigger.Default(&pDispIO);
    pDispIO.condNode = evt.subDispNode.condNode;
    switch ( DispatcherExtensions.DispatchHasImmunityTrigger(evt.objHndCaller, &pDispIO) )
    {
      case 10:
        condArg1 = evt.GetConditionArg1();
        if ( !GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
        {
          goto LABEL_91;
        }
        GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
        GameSystems.Spell.TryGetSpellEntry(dispIo.spellPkt.spellEnum, out v45);
        v3 = spellPkt.spellEnum;
        if ( spellPkt.spellEnum > WellKnownSpells.ProtectionFromLaw)
        {
          switch ( spellPkt.spellEnum )
          {
            case WellKnownSpells.GreaterHeroism:
              if ( dispIo.flag == 1 )
              {
                if ( (GameSystems.Spell.GetSpellDescriptor(dispIo.spellPkt.spellEnum) & 0x80) != 0 )
                {
                  dispIo.returnVal = 1;
                }
                goto LABEL_32;
              }
              break;
            default:
              goto LABEL_33;
            case WellKnownSpells.Shield:
              if ( dispIo.flag == 1 && dispIo.spellPkt.spellEnum == WellKnownSpells.MagicMissile)
              {
                goto LABEL_40;
              }
              break;
            case WellKnownSpells.SpellResistance:
              if ( dispIo.flag == 1 )
              {
                v43 = new DispIOBonusListAndSpellEntry();
                bonlist = BonusList.Create();
                v15 = DispatcherExtensions.Dispatch35CasterLevelModify(dispIo.spellPkt.caster, dispIo.spellPkt);
                bonlist.AddBonus(v15, 0, 203);
                if ( GameSystems.Feat.HasFeat(dispIo.spellPkt.caster, FeatId.SPELL_PENETRATION) )
                {
                  v16 = GameSystems.Feat.GetFeatName(FeatId.SPELL_PENETRATION);
                  bonlist.AddBonus(2, 0, 114, v16);
                }
                if ( GameSystems.Feat.HasFeat(dispIo.spellPkt.caster, FeatId.GREATER_SPELL_PENETRATION) )
                {
                  v17 = GameSystems.Feat.GetFeatName(FeatId.GREATER_SPELL_PENETRATION);
                  bonlist.AddBonus(2, 0, 114, v17);
                }
                v43.spellEntry = &a2;
                v18 = evt.objHndCaller.Dispatch45SpellResistanceMod(&v43);
                if ( v18 > 0 )
                {
                  if ( GameSystems.Critter.IsFriendly(dispIo.spellPkt.caster, evt.objHndCaller)
                    && !GameSystems.Spell.IsSpellHarmful(dispIo.spellPkt.spellEnum, dispIo.spellPkt.caster, evt.objHndCaller) )
                  {
                    v18 = 0;
                  }
                  v19 = (string )GameSystems.D20.Combat.GetCombatMesLine(0x13B8);
                  if ( GameSystems.Spell.DispelRoll(dispIo.spellPkt.caster, &bonlist, 0, v18, v19, &rollHistoryIdxOut) >= 0 )
                  {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30009, TextFloaterColor.Red);
                    v41 = GameSystems.D20.Combat.GetCombatMesLine(0x7A);
                    v40 = rollHistoryIdxOut;
                    v20 = GameSystems.D20.Combat.GetCombatMesLine(0x79);
                  }
                  else
                  {
                    dispIo.returnVal = 1;
                    v41 = GameSystems.D20.Combat.GetCombatMesLine(0x78);
                    v40 = rollHistoryIdxOut;
                    v20 = GameSystems.D20.Combat.GetCombatMesLine(0x77);
                  }
                  suffix = String.Format("{0}{1}{2}", v20, v40, v41);
                  GameSystems.RollHistory.CreateFromFreeText(&suffix);
                }
LABEL_32:
                v3 = spellPkt.spellEnum;
              }
              break;
            case WellKnownSpells.SpiritualWeapon:
              if ( dispIo.flag == 1 )
              {
                if ( dispIo.spellPkt.spellEnum != WellKnownSpells.DispelMagic)
                {
                  dispIo.returnVal = 1;
                }
              }
              else
              {
LABEL_40:
                dispIo.returnVal = 1;
              }
              break;
          }
          goto LABEL_33;
        }
        if ( spellPkt.spellEnum >= WellKnownSpells.ProtectionFromEvil)
        {
          goto LABEL_25;
        }
        if ( spellPkt.spellEnum > WellKnownSpells.LesserGlobeOfInvulnerability)
        {
          v4 = spellPkt.spellEnum == WellKnownSpells.ProtectionFromChaos;
        }
        else
        {
          if ( spellPkt.spellEnum == WellKnownSpells.LesserGlobeOfInvulnerability)
          {
            if ( dispIo.flag == 1 && dispIo.spellPkt.spellKnownSlotLevel < 4 )
            {
              dispIo.returnVal = 1;
            }
            goto LABEL_33;
          }
          if ( spellPkt.spellEnum == WellKnownSpells.DeathWard)
          {
            if ( dispIo.flag == 1 )
            {
              if ( SLOBYTE(spellPkt.spellClass) < 0 )
              {
                if ( v45.spellDescriptorBitmask & 0x10 )
                {
                  dispIo.returnVal = 1;
                }
              }
              else if ( (spellPkt.spellClass & 0x7F) == 4 )
              {
                dispIo.returnVal = 1;
              }
            }
            goto LABEL_33;
          }
          if ( spellPkt.spellEnum <= WellKnownSpells.MageHand|| spellPkt.spellEnum > WellKnownSpells.MagicCircleAgainstLaw)
          {
LABEL_33:
            if ( dispIo.returnVal == 1 && dispIo.flag == 1 && v3 != 311 )
            {
              v5 = dispIo.spellPkt;
              v6 = GameSystems.Spell.GetSpellName(v3);
              v7 = HIDWORD(v5.caster);
              v8 = v5.caster;
              v9 = GameSystems.Spell.GetSpellName(v5.spellEnum);
              Logger.Info("d20_mods_global.c / D20MF_immunity_check_handler(): spell ({0}) cast by obj( {1} ) resisted by target( {2} ) via ( {3} )", v9, v8, v7, evt.objHndCaller, v6);
              GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
              v11 = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
              suffix = String.Format(" [{0}]", v11);
              GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30019, TextFloaterColor.White);
              if ( spellPkt.spellEnum != WellKnownSpells.SpellResistance)
              {
                v12 = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
                v13 = GameSystems.Spell.GetSpellName(30019);
                v14 = GameSystems.MapObject.GetDisplayName(evt.objHndCaller, evt.objHndCaller);
                suffix = String.Format("{0} {1} [{2}]", v14, v13, v12);
                GameSystems.RollHistory.CreateFromFreeText(&suffix);
              }
            }
LABEL_91:
            nullsub_1/*0x100027f0*/();
            return;
          }
          v4 = evt.subDispNode.subDispDef == (SubDispDef *)SpellEffects.SpellMagicCircleOutward;
        }
        if ( v4 )
        {
LABEL_25:
          if ( dispIo.flag == 1 )
          {
            if ( a2.spellSubSchoolEnum == 5 )
            {
              dispIo.returnVal = 1;
            }
            else if ( (v45.spellDescriptorBitmask & 0x4000) )
            {
              dispIo.returnVal = 1;
            }
          }
        }
        goto LABEL_33;
      case 13:
        GameSystems.Spell.TryGetSpellEntry(dispIo.spellPkt.spellEnum, out a2);
        if ( (data1 )==0&& SLOBYTE(a2.spellDescriptorBitmask) < 0 )
        {
          dispIo.returnVal = 1;
        }
        if ( dispIo.returnVal == 1 )
        {
          v21 = dispIo.spellPkt;
          v22 = HIDWORD(v21.caster);
          v23 = v21.caster;
          v24 = GameSystems.Spell.GetSpellName(v21.spellEnum);
          Logger.Info("d20_mods_global.c / D20MF_immunity_check_handler(): spell ({0}) cast by obj( {1} ) resisted by target because of immunity.( {2} )", v24, v23, v22, evt.objHndCaller);
          GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
          v26 = GameSystems.Feat.GetFeatName((FeatId)data2);
          suffix = String.Format(" {0}", v26);
          GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30019, TextFloaterColor.White);
        }
        goto LABEL_91;
      case 14:
        GameSystems.Spell.TryGetSpellEntry(dispIo.spellPkt.spellEnum, out v45);
        v27 = data1;
        if ( (v27 )!=0)
        {
          if ( v27 == 1 && (v45.spellSubSchoolEnum == 5 || (v45.spellDescriptorBitmask & 0x4000)) )
          {
LABEL_70:
            dispIo.returnVal = 1;
            goto LABEL_71;
          }
        }
        else if ( v45.spellSubSchoolEnum == 5 || (v45.spellDescriptorBitmask & 0x4000) || v45.savingThrowType == 3 )
        {
          goto LABEL_70;
        }
LABEL_71:
        if ( dispIo.returnVal == 1 )
        {
          v28 = dispIo.spellPkt;
          v29 = HIDWORD(v28.caster);
          v30 = v28.caster;
          v31 = GameSystems.Spell.GetSpellName(v28.spellEnum);
          Logger.Info("d20_mods_global.c / D20MF_immunity_check_handler(): spell ({0}) cast by obj( {1} ) resisted by target because of immunity.( {2} )", v31, v30, v29, evt.objHndCaller);
          GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
          bonMesLinePrintf/*0x100e63d0*/(319, &suffix);
          (string )&a2 = String.Format(" {0}", &suffix);
          GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30019, TextFloaterColor.White);
        }
        goto LABEL_91;
      case 16:
        GameSystems.Spell.TryGetSpellEntry(dispIo.spellPkt.spellEnum, out v45);
        switch ( data1 )
        {
          case 0:
            if ( v45.spellSubSchoolEnum == 5 )
            {
              goto LABEL_88;
            }
            if ( (v45.spellDescriptorBitmask & 0x4000) )
            {
              goto LABEL_88;
            }
            v33 = dispIo.spellPkt;
            if ( v33.spellEnum == WellKnownSpells.Sleep              || v33.spellEnum == WellKnownSpells.DeepSlumber              || (v33.spellClass & 0x7F) == 4
              || v45.spellDescriptorBitmask & 0x10
              || v45.spellSchoolEnum == 7 )
            {
              goto LABEL_88;
            }
            v34 = v45.savingThrowType == 3;
            goto LABEL_87;
          case 1:
            if ( v45.spellSubSchoolEnum == 5 || (v45.spellDescriptorBitmask & 0x4000) )
            {
              goto LABEL_88;
            }
            v34 = dispIo.spellPkt.spellEnum == WellKnownSpells.Entangle;
            goto LABEL_87;
          case 2:
            v34 = v45.spellEnum == WellKnownSpells.Web;
            goto LABEL_87;
          case 3:
            v34 = dispIo.spellPkt.spellEnum == WellKnownSpells.Confusion;
LABEL_87:
            if ( v34 )
            {
LABEL_88:
              dispIo.returnVal = 1;
            }
            break;
          default:
            break;
        }
        if ( dispIo.returnVal == 1 )
        {
          v35 = dispIo.spellPkt;
          v36 = HIDWORD(v35.caster);
          v37 = v35.caster;
          v38 = GameSystems.Spell.GetSpellName(v35.spellEnum);
          Logger.Info("d20_mods_global.c / D20MF_immunity_check_handler(): spell ({0}) cast by obj( {1} ) resisted by target because of immunity.( {2} )", v38, v37, v36, evt.objHndCaller);
          GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
          bonMesLinePrintf/*0x100e63d0*/(318, &suffix);
          (string )&a2 = String.Format(" {0}", &suffix);
          GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30019, TextFloaterColor.White);
        }
        goto LABEL_91;
      default:
        goto LABEL_91;
    }
  }
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100d3620)]
public static void   D20QHasSpellEffectActive(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;
  int v2;
  int spellEnum;
  int i;
  ConditionSpec condStruct;

  dispIo = evt.GetDispIoD20Query();
  if ( dispIo.return_val != 1 )
  {
    if ( (v2 = dispIo.data2, spellEnum = dispIo.data1, v2 < 0)
      || v2 <= 0 && spellEnum < 282
      || v2 >= 0 && (v2 > 0 || spellEnum > 285) )
    {
      i = 0;
      while ( SpellCondStructPtrArray/*0x102e2600*/[i].spellEnum != dispIo.data1 )
      {
        if ( (int)++i >= 261 )
        {
          condStruct = 0;
          goto LABEL_12;
        }
      }
      condStruct = SpellCondStructPtrArray/*0x102e2600*/[i].cond;
LABEL_12:
      if ( evt.objHndCaller.HasCondition((int)condStruct) )
      {
        dispIo.return_val = 1;
      }
    }
  }
}


[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x100c6790)]
public static void   ChillMetalDamage(in DispatcherCallbackArgs evt, int data)
{
  int condArg1;
  int condArg3;
  unsigned Dice v3;
  int v4;
  int v5;
  int v6;
  int v7;
  int v8;
  int v9;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    if ( evt.GetConditionArg3() <= 0 )
    {
      v3 = v9;
    }
    else
    {
      condArg3 = evt.GetConditionArg3();
      v3 = Dice.Constant(condArg3);
      GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v3, DamageType.Cold, 1, D20ActionType.CAST_SPELL, spellPkt.spellId, 0);
    }
    switch ( spellPkt.durationRemaining )
    {
      case 2:
      case 6:
        v8 = 1;
        goto LABEL_8;
      case 3:
      case 4:
      case 5:
        v8 = 2;
LABEL_8:
        v3 = v8.new Dice(4, 0);
        goto LABEL_9;
      case 1:
      case 7:
        return;
      default:
LABEL_9:
        v4 = GetPackedDiceBonus/*0x10038c90*/(v3);
        v5 = GetPackedDiceType/*0x10038c40*/(v3);
        v6 = GetPackedDiceNumDice/*0x10038c30*/(v3);
        v7 = DiceRoller/*0x10038b60*/(v6, v5, v4);
        evt.SetConditionArg3(v7);
        break;
    }
  }
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100d2ba0)]
public static void   sub_100D2BA0(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  DispIoSavingThrow dispIo;

  v1 = data;
  dispIo = evt.GetDispIoSavingThrow();
  dispIo.bonlist.AddBonus(v1, 34, 113);
}


[DispTypes(DispatcherType.ConditionAddPre)]
[TempleDllLocation(0x100c77d0)]
public static void   sub_100C77D0(in DispatcherCallbackArgs evt, ConditionSpec data)
{
  DispIoCondStruct dispIo;

  dispIo = evt.GetDispIoCondStruct();
  if ( dispIo.condStruct == (ConditionSpec )data
    && evt.GetConditionArg3() == dispIo.arg2 )
  {
    dispIo.outputFlag = 0;
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100c40f0)]
public static void   sub_100C40F0(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoDamage dispIo;  DamagePacket v3;

  dispIo = evt.GetDispIoDamage();/*INLINED:v2=evt.subDispNode.subDispDef*/  v3 = &dispIo.damage;
  if ( data2 == 282 )
  {
    v3.AddDamageBonus(-data1, 0, 282);
  }
  else
  {
    v3.AddDamageBonus(data1, 0, data2);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cce30)]
public static void   DeafenedFloatMsg(in DispatcherCallbackArgs args)
{
  GameSystems.Spell.FloatSpellLine(args.objHndCaller, 20020, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100d2a90)]
public static void   sub_100D2A90(in DispatcherCallbackArgs evt)
{
  evt.GetConditionArg1();
  evt.GetConditionArg2();
  evt.GetConditionArg3();
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100dbe40)]
public static void   sub_100DBE40(in DispatcherCallbackArgs evt, int data)
{
  DispIoD20Signal dispIo;

  dispIo = evt.GetDispIoD20Signal();
  if ( dispIo.data1 != evt.GetConditionArg1() )
  {
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, spell_mes_A_Spell_has_expired, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(in evt);
                SpellEffects.Spell_remove_mod(in evt);
  }
}


[DispTypes(DispatcherType.TakingDamage)]
[TempleDllLocation(0x100c7bd0)]
public static void   FireShieldDamageResistance(in DispatcherCallbackArgs evt)
{
  DispIoDamage dispIo;
  int condArg3;
  int v3;
  int v4;
  int v5;
  D20DT v6;
  DamagePacket v7;
  double v8;

  dispIo = evt.GetDispIoDamage();
  condArg3 = evt.GetConditionArg3();
  if ( condArg3 == 3 )
  {
    v3 = 10;
    v4 = 110;
  }
  else
  {
    if ( condArg3 != 9 )
    {
      return;
    }
    v3 = 8;
    v4 = 111;
  }
  v5 = v4;
  v6 = v3;
  v7 = &dispIo.damage;
  v8 = (float)v7.GetOverallDamageByType() * 0.5F;
  v7.AddDR((ulong)v8, v6, v5);
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100dc800)]
public static void   sub_100DC800(in DispatcherCallbackArgs evt, int data)
{
  DispIoD20Signal dispIo;
  int condArg1;
  int v3;
  int v4;
  int v5;
      SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoD20Signal();
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    v3 = data;
    if ( v3 >= 0xF0 && v3 <= 0xF3 )
    {
      if ( (v4 = dispIo.data1, v5 = dispIo.data2, v4 == LODWORD(spellPkt.targetListHandles[0]))
        && v5 == HIDWORD(spellPkt.targetListHandles[0])
        || __PAIR__(v5, v4) == evt.objHndCaller )
      {
        if ( __PAIR__(spellPkt.targetListHandles[0], HIDWORD(spellPkt.targetListHandles[0])) )
        {
          GameSystems.D20.D20SendSignal(spellPkt.targetListHandles[0], D20DispatcherKey.SIG_Spell_Grapple_Removed, spellPkt.spellId, 0);
        }
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_Grapple_Removed, spellPkt.spellId, 0);
                                SpellEffects.Spell_remove_spell(in evt);
                                SpellEffects.Spell_remove_mod(evt.WithoutIO);
      }
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100d0340)]
public static void   TreeShapeBeginSpell(in DispatcherCallbackArgs evt)
{
  int condArg1;


  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    if ( !spellPkt.caster.AddCondition("sp-Concentrating", condArg1, 0, 0) )
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_tree_shape(): unable to add condition to spell_caster");
    }
  }
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d4830)]
public static void   Condition__36__fog_cloud_sthg(in DispatcherCallbackArgs evt, int data)
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
        v7 = GameSystems.ParticleSys.CreateAtObj("sp-Fog Cloud-hit", v5);
        spellPkt.AddTarget(dispIo.tgt, v7, true);
        dispIo.tgt.AddCondition("sp-Fog Cloud Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        v3 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v3);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _fog_cloud_hit_trigger(): cannot remove target");
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
    Logger.Info("d20_mods_spells.c / _fog_cloud_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100cb1a0)]
public static void   sub_100CB1A0(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;

  dispIo = evt.GetDispIoD20Query();
  if ( evt.GetConditionArg3() == 1 )
  {
    dispIo.return_val = 1;
    dispIo.data1 = evt.GetConditionArg1();
    dispIo.data2 = 0;
  }
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100d4a00)]
[TemplePlusLocation("spell_condition.cpp:83")]
public static void   GhoulTouchAttackHandler(in DispatcherCallbackArgs evt, int data)
{
  D20Action d20a;
  int condArg1;
    Dice dice1d6_plus2;
  int v5;
  unsigned Dice v6;
  int v7;
  Dice v8;
  int v9;


  GameObjectBody v12;

  int v14;


    string v18;
  SpellPacketBody spellPktBody;

  d20a = (D20Action )evt.GetDispIoD20Signal().data1;
  if ( (d20a.d20Caf & D20CAF.HIT)!=0)
  {
    condArg1 = evt.GetConditionArg1();
    GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPktBody);
    GameSystems.Script.Spells.SpellSoundPlay(&spellPktBody, OnAreaOfEffectHit);
    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 68);
    GameSystems.Script.Spells.SpellSoundPlay(&spellPktBody, OnSpellStruck);
    if ( D20ModSpells.CheckSpellResistance(&spellPktBody, d20a.d20ATarget) )
    {
                        SpellEffects.Spell_remove_mod(evt.WithoutIO);
    }
    else
    {
      dice1d6_plus2 = 1.new Dice(6, 2);
      v5 = GetPackedDiceBonus/*0x10038c90*/(dice1d6_plus2);
      v6 = 1.new Dice(6, 2);
      v7 = GetPackedDiceType/*0x10038c40*/(v6);
      v8 = 1.new Dice(6, 2);
      v9 = GetPackedDiceNumDice/*0x10038c30*/(v8);
      spellPktBody.duration = DiceRoller/*0x10038b60*/(v9, v7, v5);
      if ( !d20a.d20ATarget.AddCondition("sp-Ghoul Touch Paralyzed", spellPktBody.spellId, spellPktBody.duration, 0) )
      {
        Logger.Info("d20_mods_spells.c / _ghoul_touch_stench_hit(): unable to add condition");
      }
      v12 = d20a.d20ATarget;
      v14 = GameSystems.ParticleSys.CreateAtObj("sp-Ghoul Touch", v12);
      if ( !d20a.d20ATarget.AddCondition("sp-Ghoul Touch Stench", spellPktBody.spellId, spellPktBody.duration, 0, v14) )
      {
        Logger.Info("d20_mods_spells.c / _ghoul_touch_stench_hit(): unable to add condition");
      }
      v18 = (string )spellPktBody.targetListPartsysIds[0];
      spellPktBody.targetListHandles[0] = d20a.d20ATarget;
      spellPktBody.targetListPartsysIds[0] = v14;
{
    GameSystems.Spell.UpdateSpellPacket(spellPktBody);
    GameSystems.Script.Spells.UpdateSpell(spellPktBody.spellId);
    SpellEffects.Spell_remove_mod(evt.WithoutIO);
    GameSystems.ParticleSys.End(v18);
    if (spellPktBody.RemoveTarget(evt.objHndCaller) != 1)
    {
        Logger.Info("d20_mods_spells.c / _ghoul_touch_hit_trigger(): cannot remove target");
    }
} /*      else
      {
        Logger.Info("d20_mods_spells.c / _ghoul_touch_hit_trigger(): unable to save new spell_packet");
      }
*/    }
  }
  else
  {
    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 69);
  }
}
/* Orphan comments:
TP Replaced @ spell_condition.cpp:83
*/


[DispTypes(DispatcherType.Tooltip)]
[TempleDllLocation(0x100d2e90)]
public static void   SpellResistanceTooltipCallback(in DispatcherCallbackArgs evt, int data)
{
  DispIoTooltip dispIo;
  int v2;
  int condArg3;
  int v4;
  string meslineValue;
int meslineKey;
  CHAR v6;

  dispIo = evt.GetDispIoTooltip();
  meslineKey = data;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  condArg3 = evt.GetConditionArg3();
  v6 = String.Format("{0} [{1}]", meslineValue, condArg3);
  v4 = dispIo.numStrings;
  if ( v4 < 10 )
  {
    dispIo.numStrings = v4 + 1;
    strncpy(dispIo.strings[v4].text, &v6, 0x100);
  }
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100cb6b0)]
public static void   sub_100CB6B0(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;

  dispIo = evt.GetDispIoAttackBonus();
  if ( !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement) )
  {
    dispIo.bonlist.AddBonus(-data1, 0, data2);
  }
}


[DispTypes(DispatcherType.GetMoveSpeed, DispatcherType.GetMoveSpeedBase)]
[TempleDllLocation(0x100c8b00)]
public static void   sub_100C8B00(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoMoveSpeed dispIo;

  dispIo = evt.GetDispIoMoveSpeed();
  dispIo.bonlist.AddBonus(data1, 12, data2);
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d4e60)]
public static void   IceStormHitTrigger(in DispatcherCallbackArgs evt, int data)
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
        dispIo.tgt.AddCondition("sp-Ice Storm Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        v3 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v3);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _ice_storm_hit_trigger(): cannot remove target");
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
    Logger.Info("d20_mods_spells.c / _ice_storm_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d6110)]
public static void   SolidFogAoEEvent(in DispatcherCallbackArgs evt, int data)
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
        v7 = GameSystems.ParticleSys.CreateAtObj("sp-Solid Fog-hit", v5);
        spellPkt.AddTarget(dispIo.tgt, v7, true);
        dispIo.tgt.AddCondition("sp-Solid Fog Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        v3 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v3);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _solid_fog_hit_trigger(): cannot remove target");
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
    Logger.Info("d20_mods_spells.c / _solid_fog_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.GetMoveSpeedBase)]
[TempleDllLocation(0x100cb5a0)]
public static void   WebOffMovementSpeed(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoMoveSpeed dispIo;
  int condArg4;
  int v3;
  int v4;

  dispIo = evt.GetDispIoMoveSpeed();
  condArg4 = evt.GetConditionArg4();
  v3 = 10;
  if ( condArg4 >= 10 )
  {
    v3 = condArg4;
  }
  if ( v3 - 10 <= 5 )
  {
    v4 = 5;
  }
  else
  {
    if ( condArg4 < 10 )
    {
      condArg4 = 10;
    }
    v4 = condArg4 - 10;
  }
  if ( !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement)
    && !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement) )
  {
    dispIo.bonlist.SetOverallCap(1, v4, 0, data2);
    dispIo.bonlist.SetOverallCap(2, v4, 0, data2);
  }
}


[DispTypes(DispatcherType.GetDefenderConcealmentMissChance)]
[TempleDllLocation(0x100ca920)]
public static void   sub_100CA920(in DispatcherCallbackArgs evt)
{
  DispIoAttackBonus dispIo;
  BonusList *v2;
  float v3;

  dispIo = evt.GetDispIoAttackBonus();
  v3 = LocationExtensions.DistanceToObjInFeet(dispIo.attackPacket.attacker, dispIo.attackPacket.victim);
  if ( !GameSystems.D20.D20Query(dispIo.attackPacket.attacker, D20DispatcherKey.QUE_Critter_Has_True_Seeing) )
  {
    v2 = &dispIo.bonlist;
    if ( v3 <= 5F)
    {
      v2.AddBonus(50, 19, 258);
    }
    else
    {
      v2.AddBonus(100, 19, 258);
    }
  }
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100caa70)]
public static void   SpikeGrowthHit(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int v2;
  int v3;

  int v5;
  Dice v6;


  int condArg3;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  condArg3 = evt.GetConditionArg3();
  v2 = evt.GetDispIoD20Signal().data1;
  if ( v2 >= 5 )
  {
    v3 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(v3, out spellPkt) )
    {
      GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
      GameSystems.SoundGame.PositionalSound(15107, 1, evt.objHndCaller);
      v5 = spellPkt.spellId;
      v6 = v2 / 5.new Dice(4, 0);
      GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v6, DamageType.Magic, 1, D20ActionType.CAST_SPELL, v5, 0);
      if ( !GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc, SavingThrowType.Reflex, 0, spellPkt.spellId) )
      {
        if ( !evt.objHndCaller.AddCondition("sp-Spike Growth Damage", condArg1, 14400, condArg3) )
        {
          Logger.Info("d20_mods_spells.c / _spike_growth_hit(): unable to add condition");
        }
      }
    }
  }
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100cbab0)]
[TemplePlusLocation("condition.cpp:501")]
public static void   Spell_remove_mod(in DispatcherCallbackArgs evt, int data)
{
  DispIoD20Signal dispIo;
  int condArg1;
  int i;
  int v4;
  int v5;
  SpellPacketBody spellPkt;

  dispIo = 0;
  if ( (evt.dispIO == null)|| evt.dispType != DispatcherType.D20Signal )
  {
    goto LABEL_6;
  }
  dispIo = evt.GetDispIoD20Signal();
  if ( evt.dispKey != D20DispatcherKey.SIG_Killed )
  {
    if ( evt.dispKey == D20DispatcherKey.SIG_Sequence )
    {
      Logger.Info("d20_mods_spells.c / _remove_mod: [WARNING:] caught a D20est_S_Sequence, make sure we are removing spell_mod properly...");
      goto LABEL_17;
    }
LABEL_6:
    if ( evt.dispKey != D20DispatcherKey.SIG_Killed
      && evt.dispKey != D20DispatcherKey.SIG_Critter_Killed
      && evt.dispKey != D20DispatcherKey.SIG_Sequence
      && evt.dispKey != D20DispatcherKey.SIG_Spell_Cast
      && evt.dispKey != D20DispatcherKey.SIG_Action_Recipient
      && evt.dispKey != D20DispatcherKey.SIG_Remove_Concentration
      && evt.dispKey != D20DispatcherKey.SIG_TouchAttackAdded
      && evt.dispKey != D20DispatcherKey.SIG_Teleport_Prepare
      && evt.dispKey != D20DispatcherKey.SIG_Teleport_Reconnect
      && dispIo
!=null      && dispIo.data1 != evt.GetConditionArg1() )
    {
      return;
    }
  }
LABEL_17:
  switch ( data )
  {
    case 2:
      if ( evt.dispKey == D20DispatcherKey.SIG_Remove_Concentration )
      {
        condArg1 = evt.GetConditionArg1();
        if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
        {
          GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 5060);
          GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Concentration_Broken, spellPkt.spellId, 0);
          for ( i = 0; i < spellPkt.targetCount; ++i )
          {
            v4 = spellPkt.targetListHandles[i];
            v5 = HIDWORD(spellPkt.targetListHandles[i]);
            if ( LODWORD(evt.objHndCaller) != v4 || HIDWORD(evt.objHndCaller) != v5 )
            {
              GameSystems.D20.D20SendSignal(__PAIR__(v5, v4), D20DispatcherKey.SIG_Concentration_Broken, spellPkt.spellId, 0);
            }
          }
        }
      }
      break;
    case 29:
    case 69:
    case 70:
    case 71:
    case 72:
    case 121:
    case 171:
    case 232:
      GameSystems.Critter.BuildRadialMenu(evt.objHndCaller);
      break;
    default:
      break;
  }
  evt.RemoveThisCondition();
}
/* Orphan comments:
TP Replaced @ condition.cpp:501
*/


[DispTypes(DispatcherType.GetAC)]
[TempleDllLocation(0x100c7ec0)]
public static void   GaseousFormAcBonusCapper(in DispatcherCallbackArgs evt)
{
  BonusList *v1;

  v1 = &evt.GetDispIoAttackBonus().bonlist;
  v1.AddCap(28, 0, 0xE3);
  v1.AddCap(9, 0, 0xE3);
}


[DispTypes(DispatcherType.ToHitBonusBase)]
[TempleDllLocation(0x100c7390)]
[TemplePlusLocation("spell_condition.cpp:80")]
public static void   DivinePowerToHitBonus(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;
  int v2;
  BonusList *bonlist;
  int v4;
  int fighterBAB;
  int v6;
  LevelPacket lvlPkt;

  dispIo = evt.GetDispIoAttackBonus();
  LevelPacketInit/*0x100f5520*/(&lvlPkt);
  v2 = evt.objHndCaller.GetArrayLength(obj_f.critter_level_idx);
  GetLevelPacket/*0x100f5140*/(Stat.level_fighter, evt.objHndCaller, 0, v2, &lvlPkt);
  bonlist = &dispIo.bonlist;
  bonlist.AddCap(1, 0, data2);
  v4 = bonlist.OverallBonus();
  fighterBAB = lvlPkt.baseAttackBonus;
  if ( v4 < lvlPkt.baseAttackBonus )
  {
    v6 = bonlist.OverallBonus();
    fighterBAB = lvlPkt.baseAttackBonus - v6;
  }
  bonlist.AddBonus(fighterBAB, 12, data2);
  LevelPacketDealloc/*0x100f4780*/(&lvlPkt);
}
/* Orphan comments:
TP Replaced @ spell_condition.cpp:80
*/


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100c9410)]
public static void   ProduceFlameTouchAttackHandler(in DispatcherCallbackArgs evt, int data)
{
  D20Action v1;
  int v2;
  int v3;
  int condArg1;

  Dice v6;
  int condArg2;
  int v8;
  int v9;
  SpellPacketBody spellPktBody;

  v1 = (D20Action )evt.GetDispIoD20Signal().data1;
  v2 = v1.d20ATarget;
  v3 = HIDWORD(v1.d20ATarget);
  if ( (v1.d20Caf & D20CAF.HIT )!=0)
  {
    condArg1 = evt.GetConditionArg1();
    GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPktBody);
    GameSystems.Script.Spells.SpellSoundPlay(&spellPktBody, OnAreaOfEffectHit);
    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 68);
    GameSystems.ParticleSys.CreateAtObj("sp-Produce Flame-Hit", __PAIR__(v3, v2));
    if ( D20ModSpells.CheckSpellResistance(&spellPktBody, __PAIR__(v3, v2)) )
    {
      return;
    }
    if ( (v1.d20Caf & D20CAF.CRITICAL )!=0)
    {
      v9 = spellPktBody.casterLevel;
      v8 = 2;
    }
    else
    {
      v9 = spellPktBody.casterLevel;
      v8 = 1;
    }
    v6 = v8.new Dice(6, v9);
    GameSystems.D20.Combat.SpellDamageFull(__PAIR__(v3, v2), evt.objHndCaller, v6, DamageType.Fire, 1, v1.d20ActType, spellPktBody.spellId, 0);
  }
  else
  {
    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 69);
  }
  condArg2 = evt.GetConditionArg2();
  evt.SetConditionArg2(condArg2 - 10);
  spellPktBody.durationRemaining = evt.GetConditionArg2();
  GameSystems.Spell.UpdateSpellPacket(spellPktBody);
  GameSystems.Script.Spells.UpdateSpell(spellPktBody.spellId);
}


[DispTypes(DispatcherType.GetCriticalHitRange)]
[TempleDllLocation(0x100cae70)]
public static void   SpiritualWeapon_Callback23(in DispatcherCallbackArgs evt)
{
  DispIoAttackBonus dispIo;
  GameObjectBody v2;

  dispIo = evt.GetDispIoAttackBonus();
  v2 = dispIo.attackPacket.GetWeaponUsed();
  if ( v2 !=null)
  {
    v2.GetInt32(obj_f.weapon_crit_range);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cce70)]
public static void   sub_100CCE70(in DispatcherCallbackArgs evt)
{
  int v1;
  int condArg1;


  int condArg2;
  int v6;
  CHAR extraText;
  SpellPacketBody spellPkt;

  v1 = DiceRoller/*0x10038b60*/(1, 8, 0);
  extraText = String.Format("[{0}] ", v1);
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20005, TextFloaterColor.White, &extraText);
  Logger.Info("d20_mods_spells.c / _begin_death_knell(): gained {0} temporary hit points", v1);
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    condArg2 = evt.GetConditionArg2();
    v6 = evt.GetConditionArg1();
    if ( !evt.objHndCaller.AddCondition("Temporary_Hit_Points", v6, condArg2, v1) )
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_death_knell(): unable to add condition");
    }
  }
  else
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_death_knell(): unable to get spell_packet");
  }
}


[DispTypes(DispatcherType.GetCriticalHitExtraDice)]
[TempleDllLocation(0x100caea0)]
public static void   sub_100CAEA0(in DispatcherCallbackArgs evt)
{
  DispIoAttackBonus dispIo;
  GameObjectBody v2;

  dispIo = evt.GetDispIoAttackBonus();
  v2 = dispIo.attackPacket.GetWeaponUsed();
  if ( v2 !=null)
  {
    v2.GetInt32(obj_f.weapon_crit_hit_chart);
  }
}


[DispTypes(DispatcherType.DispelCheck)]
[TempleDllLocation(0x100db380)]
public static void   BreakEnchantmentDispelCheck(in DispatcherCallbackArgs evt, int data)
{
  DispIoDispelCheck dispIo;
      int condArg1;
  string v5;

  int v7;
  int v8;
  string v9;
      BonusList bonlist;
  SpellPacketBody v13;
  CHAR suffix;
  SpellPacketBody spellPkt;

  bonlist = BonusList.Create();
  dispIo = evt.GetDispIoDispelCheck();
  if ( (dispIo.flags & 0x20 )!=0)
  {
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(in evt);
                SpellEffects.Spell_remove_mod(in evt);
  }
  if ( (dispIo.flags & 0x40 )!=0)
  {
    if ( !GameSystems.Spell.TryGetActiveSpell(dispIo.spellId, out spellPkt) )
    {
      Logger.Info("d20_mods_spells.c / _break_enchantment_dispel_check(): error getting spellid packet for dispel_packet");
      return;
    }
    condArg1 = evt.GetConditionArg1();
    if ( !GameSystems.Spell.TryGetActiveSpell(condArg1, out v13) )
    {
      Logger.Info("d20_mods_spells.c / _break_enchantment_dispel_check(): error getting spellid packet for spell_packet");
      return;
    }
    bonlist.AddBonus(spellPkt.casterLevel, 0, 203);
    v5 = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
    if ( GameSystems.Spell.DispelRoll(spellPkt.caster, &bonlist, 0, v13.casterLevel + 11, v5, 0) >= 0 || spellPkt.caster == v13.caster )
    {
      v7 = dispIo.flags;
      v8 = dispIo.flags & 1;
      if ( (v8 )==0)
      {
        --dispIo.returnVal;
      }
      if ( v8 == 1
        || (v7 & 0x40
)!=0        || (v7 & 2 )!=0&& v13.caster.HasChaoticAlignment()
        || (dispIo.flags & 4 )!=0&& v13.caster.HasEvilAlignment()
        || (dispIo.flags & 8 )!=0&& v13.caster.HasGoodAlignment()
        || (dispIo.flags & 0x10 )!=0&& v13.caster.HasLawfulAlignment() )
      {
        v9 = GameSystems.Spell.GetSpellName(v13.spellEnum);
        suffix = String.Format(" [{0}]", v9);
        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20002, TextFloaterColor.White);
                                SpellEffects.Spell_remove_spell(in evt);
                                SpellEffects.Spell_remove_mod(in evt);
      }
    }
    else
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20003, TextFloaterColor.Red);
      GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
    }
  }
  nullsub_1/*0x100027f0*/();
}


[DispTypes(DispatcherType.GetAttackDice)]
[TempleDllLocation(0x100c9810)]
public static void   AttackDiceReducePerson(in DispatcherCallbackArgs evt, int data1, int data2)
{
  Dice v1;
  DispIoAttackDice dispIo;
  int v3;
  int v4;
  int v5;
  int v6;

  v1 = 0;
  dispIo = evt.GetDispIoAttackDice();
  v3 = data2;
  if ( v3 == 245 && dispIo.weapon !=null|| v3 == 295 && (dispIo.weapon == null))
  {
    switch ( GetPackedDiceType/*0x10038c40*/(dispIo.dicePacked) )
    {
      case 2:
        v6 = GetPackedDiceBonus/*0x10038c90*/(dispIo.dicePacked);
        v5 = 1;
        goto LABEL_13;
      case 3:
        v6 = GetPackedDiceBonus/*0x10038c90*/(dispIo.dicePacked);
        v5 = 2;
        v4 = GetPackedDiceNumDice/*0x10038c30*/(dispIo.dicePacked);
        goto LABEL_14;
      case 4:
        v6 = GetPackedDiceBonus/*0x10038c90*/(dispIo.dicePacked);
        v5 = 3;
        v4 = GetPackedDiceNumDice/*0x10038c30*/(dispIo.dicePacked);
        goto LABEL_14;
      case 6:
        v6 = GetPackedDiceBonus/*0x10038c90*/(dispIo.dicePacked);
        v5 = 4;
        goto LABEL_13;
      case 8:
        v6 = GetPackedDiceBonus/*0x10038c90*/(dispIo.dicePacked);
        v5 = 6;
        v4 = GetPackedDiceNumDice/*0x10038c30*/(dispIo.dicePacked);
        goto LABEL_14;
      case 0xA:
        v6 = GetPackedDiceBonus/*0x10038c90*/(dispIo.dicePacked);
        v5 = 8;
        v4 = GetPackedDiceNumDice/*0x10038c30*/(dispIo.dicePacked);
        goto LABEL_14;
      case 0xC:
        v6 = GetPackedDiceBonus/*0x10038c90*/(dispIo.dicePacked);
        v5 = 10;
LABEL_13:
        v4 = GetPackedDiceNumDice/*0x10038c30*/(dispIo.dicePacked);
LABEL_14:
        v1 = v4.new Dice(v5, v6);
        break;
      default:
        break;
    }
    dispIo.dicePacked = v1;
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100d04c0)]
public static void   BeginSpellWeb(in DispatcherCallbackArgs evt)
{
  int condArg1;
  float radiusInches;
  int v3;  SpellEntry a2;
  SpellPacketBody spellPkt;

  evt.SetConditionArg(4, 0);
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
    radiusInches = (float)(int)a2.radiusTarget * 12F;
    spellPkt.aoeObj = evt.objHndCaller;
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 46, 47, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_web(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d6660)]
[TemplePlusLocation("spell_condition.cpp:348")]
public static void   SpikeStonesHitTrigger(in DispatcherCallbackArgs evt, int data)
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
        v8 = GameSystems.ParticleSys.CreateAtObj("sp-Spike Stones-HIT", v6);
        spellPkt.AddTarget(dispIo.tgt, v8, true);
        dispIo.tgt.AddCondition("sp-Spike Stones Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
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
          Logger.Info("d20_mods_spells.c / _spike_stones_hit_trigger(): cannot remove target");
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
    Logger.Info("d20_mods_spells.c / _spike_stones_hit_trigger(): unable to save new spell_packet");
  }
*/}
/* Orphan comments:
TP Replaced @ spell_condition.cpp:348
*/


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d36b0)]
public static void   AoeObjEventCloudkill(in DispatcherCallbackArgs evt, int data)
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
      if ( evt.dispKey == D20DispatcherKey.OnEnterAoE )
      {
        v5 = dispIo.tgt;
        v7 = GameSystems.ParticleSys.CreateAtObj("Fizzle", v5);
        spellPkt.AddTarget(dispIo.tgt, v7, true);
        dispIo.tgt.AddCondition("sp-Cloudkill-Damage", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        v3 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v3);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _cloudkill_hit_trigger(): cannot remove target");
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
    Logger.Info("d20_mods_spells.c / _cloudkill_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100dbec0)]
public static void   d20_mods_spells__teleport_prepare(in DispatcherCallbackArgs evt)
{
  int condArg1;
  string v2;
  string v3;
  string v4;
  string v5;
      string v8;
  string v9;
  string v10;
  string v11;
      SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    v2 = GameSystems.MapObject.GetDisplayName(evt.objHndCaller, evt.objHndCaller);
    v3 = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
    Logger.Info("d20_mods_spells.c / _d20_mods_spells_teleport_prepare(): preparing spell=( {0} ) on obj=( {1} ) for teleport", v3, v2);
    if ( !GameSystems.Party.IsInParty(spellPkt.caster) )
    {
      v4 = GameSystems.MapObject.GetDisplayName(evt.objHndCaller, evt.objHndCaller);
      v5 = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
      Logger.Info("d20_mods_spells.c / _d20_mods_spells_teleport_prepare(): ending spell=( {0} ) on obj=( {1} ) because caster is not in party!", v5, v4);
                        SpellEffects.Spell_remove_spell(evt.WithoutIO);
                        SpellEffects.Spell_remove_mod(evt.WithoutIO);
      return;
    }
    if ( !GameSystems.Party.IsInParty(evt.objHndCaller) )
    {
      if ( spellPkt.targetCount <= 1 )
      {
        v10 = GameSystems.MapObject.GetDisplayName(evt.objHndCaller, evt.objHndCaller);
        v11 = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
        Logger.Info("d20_mods_spells.c / _d20_mods_spells_teleport_prepare(): ending spell=( {0} ) on obj=( {1} ) because target is not in party!", v11, v10);
                                SpellEffects.Spell_remove_spell(evt.WithoutIO);
                                SpellEffects.Spell_remove_mod(evt.WithoutIO);
        return;
      }
      v8 = GameSystems.MapObject.GetDisplayName(evt.objHndCaller, evt.objHndCaller);
      v9 = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
      Logger.Info("d20_mods_spells.c / _d20_mods_spells_teleport_prepare(): processing spell=( {0} ), removing obj=( {1} ) from target_list because target is not in party!", v9, v8);
      spellPkt.RemoveTarget(evt.objHndCaller);
    }
  }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*  else
  {
    Logger.Info("d20_mods_spells.c / _d20_mods_spells_teleport_prepare(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100ddd20)]
public static void   TrueStrikeAttackBonus(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;

  dispIo = evt.GetDispIoAttackBonus();
  dispIo.bonlist.AddBonus(data1, 18, data2);
  if ( (dispIo.attackPacket.flags & D20CAF.FINAL_ATTACK_ROLL )!=0)
  {
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(evt.WithoutIO);
                SpellEffects.Spell_remove_mod(evt.WithoutIO);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100d03b0)]
public static void   sub_100D03B0(in DispatcherCallbackArgs evt)
{
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20026, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.TakingDamage)]
[TempleDllLocation(0x100c6020)]
public static void   sub_100C6020(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoDamage dispIo;

  dispIo = evt.GetDispIoDamage();
  dispIo.damage.AddPhysicalDR(data1, 4, 0x68);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cf880)]
public static void   SleepOnAdd(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg2;



  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  evt.subDispNode = (SubDispNode *)evt.GetConditionArg3();
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20004, TextFloaterColor.Red);
  evt.objHndCaller.AddCondition(StatusEffects.Prone);
  GameSystems.Anim.PushAnimate(evt.objHndCaller, 64);
  if ( !evt.objHndCaller.AddCondition("Sleeping", condArg1, condArg2, (int)evt.subDispNode) )
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_sleep(): unable to add condition");
  }
}


[DispTypes(DispatcherType.RadialMenuEntry)]
[TempleDllLocation(0x100c6530)]
public static void   CallLightningRadial(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  string v2;
  int v3;
  string meslineValue;
int meslineKey;
  RadialMenuEntry radMenuEntry;

  radMenuEntry = RadialMenuEntry.Create();
  radMenuEntry.d20ActionType = D20ActionType.SPELL_CALL_LIGHTNING;
  radMenuEntry.d20ActionData1 = 0;
  meslineKey = 108;
  meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
  radMenuEntry.text = (string )meslineValue;
  v2 = GameSystems.Spell.GetSpellHelpTopic(46);
  radMenuEntry.helpSystemHashkey = v2/*ELFHASH*/;
  v3 = GameSystems.D20.RadialMenu.GetStandardNode(RadialMenuStandardNode.Spells);
  GameSystems.D20.RadialMenu.AddChildNode(evt.objHndCaller, ref radMenuEntry, v3);
}


[DispTypes(DispatcherType.TurnBasedStatusInit)]
[TempleDllLocation(0x100c85b0)]
public static void   GustOfWindTurnBasedStatusInit(in DispatcherCallbackArgs evt)
{
  int v1;
  DispIOTurnBasedStatus dispIo;
  int condArg1;
  Dice v4;
  int v5;
  unsigned Dice v6;
  int v7;
  Dice v8;
  int v9;
  int v10;
  int v11;
  Dice v12;
  int *v13;
  SpellPacketBody spellPkt;

  v1 = evt.objHndCaller.GetStat(Stat.size);
  dispIo = evt.GetDispIOTurnBasedStatus();
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) && v1 < 6 )
  {
    if ( v1 < 5 )
    {
      if ( v1 < 4 )
      {
        v4 = 1.new Dice(4, 0);
        v5 = GetPackedDiceBonus/*0x10038c90*/(v4);
        v6 = 1.new Dice(4, 0);
        v7 = GetPackedDiceType/*0x10038c40*/(v6);
        v8 = 1.new Dice(4, 0);
        v9 = GetPackedDiceNumDice/*0x10038c30*/(v8);
        v10 = DiceRoller/*0x10038b60*/(v9, v7, v5);
        v11 = spellPkt.spellId;
        v12 = 10 * v10.new Dice(4, 0);
        GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v12, 0, 1, D20ActionType.CAST_SPELL, v11, 0);
      }
      evt.objHndCaller.AddCondition(StatusEffects.Prone);
      GameSystems.Anim.PushAnimate(evt.objHndCaller, 64);
    }
    if ( dispIo !=null)
    {
      v13 = &dispIo.tbStatus.hourglassState;
      if ( v13 )
      {
        *v13 = 0;
        dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
      }
    }
  }
}


[DispTypes(DispatcherType.SpellResistanceMod)]
[TempleDllLocation(0x100d2e50)]
public static void   SpellResistanceMod_ProtFromMagic_Callback(in DispatcherCallbackArgs evt, int data)
{
  int condArg3;
  DispIOBonusListAndSpellEntry dispIo;

  condArg3 = evt.GetConditionArg3();
  dispIo = evt.GetDispIOBonusListAndSpellEntry();
  dispIo.bonList.AddBonus(condArg3, 36, 203);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cf260)]
public static void   RayOfEnfeeblementOnAdd(in DispatcherCallbackArgs evt)
{
  int condArg3;
  string strengthLabel;
  string v3;
  CHAR v4;

  condArg3 = evt.GetConditionArg3();
  strengthLabel = GameSystems.Stat.GetStatName(0);
  v3 = GameSystems.Spell.GetSpellName(25013);
  v4 = String.Format("{0} [{1}: {2}]", v3, strengthLabel, condArg3);
  GameSystems.RollHistory.CreateFromFreeText(&v4);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cc6e0)]
public static void   BeginSpellCloudkill(in DispatcherCallbackArgs evt)
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
    v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 0, 1, ObjectListFilter.OLC_CRITTERS, radiusInches, 0F, 6.28318548F);
    evt.SetConditionArg3(v3);
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    spellPkt.spellObjs[0].obj = spellPkt.aoeObj;
    spellPkt.spellObjs[0].partSysId = evt.GetConditionArg4();
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_cloudkill(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ca4d0)]
public static void   sub_100CA4D0(in DispatcherCallbackArgs evt)
{
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20039, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cec40)]
public static void   sub_100CEC40(in DispatcherCallbackArgs evt)
{
  int condArg3;
  int condArg1;
  int v3;
  SpellPacketBody spellPkt;

  condArg3 = evt.GetConditionArg3();
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    spellPkt.RemoveTarget(evt.objHndCaller);
    v3 = 0;
    for ( spellPkt.targetCount = condArg3; v3 < condArg3; ++v3 )
    {
      LODWORD(spellPkt.targetListHandles[v3]) = evt.objHndCaller;
      HIDWORD(spellPkt.targetListHandles[v3]) = HIDWORD(evt.objHndCaller);
      spellPkt.targetListPartsysIds[v3] = 0;
    }
{
    GameSystems.Spell.UpdateSpellPacket(spellPkt);
    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
} /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_mirror_image(): unable to save new spell_packet");
    }
*/  }
}


[DispTypes(DispatcherType.SpellResistanceMod)]
[TempleDllLocation(0x100caa30)]
public static void   SpellResistanceMod_spSpellResistance_Callback(in DispatcherCallbackArgs evt, int data)
{
  int condArg3;
  DispIOBonusListAndSpellEntry dispIo;

  condArg3 = evt.GetConditionArg3();
  dispIo = evt.GetDispIOBonusListAndSpellEntry();
  dispIo.bonList.AddBonus(condArg3, 36, 203);
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c7860)]
public static void   sub_100C7860(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;

  dispIo = evt.GetDispIoD20Query();
  if ( evt.GetConditionArg3() == 9 )
  {
    dispIo.return_val = 1;
  }
}


[DispTypes(DispatcherType.TurnBasedStatusInit)]
[TempleDllLocation(0x100c88a0)]
public static void   HeatMetalTurnBasedStatusInit(in DispatcherCallbackArgs evt, int data)
{
  int condArg1;
  int condArg3;
  unsigned Dice v3;
  int v4;
  int v5;
  int v6;
  int v7;
  int v8;
  int v9;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    if ( evt.GetConditionArg3() <= 0 )
    {
      v3 = v9;
    }
    else
    {
      condArg3 = evt.GetConditionArg3();
      v3 = Dice.Constant(condArg3);
      GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v3, DamageType.Fire, 1, D20ActionType.CAST_SPELL, spellPkt.spellId, 0);
    }
    switch ( spellPkt.durationRemaining )
    {
      case 2:
      case 6:
        v8 = 1;
        goto LABEL_8;
      case 3:
      case 4:
      case 5:
        v8 = 2;
LABEL_8:
        v3 = v8.new Dice(4, 0);
        goto LABEL_9;
      case 1:
      case 7:
        return;
      default:
LABEL_9:
        v4 = GetPackedDiceBonus/*0x10038c90*/(v3);
        v5 = GetPackedDiceType/*0x10038c40*/(v3);
        v6 = GetPackedDiceNumDice/*0x10038c30*/(v3);
        v7 = DiceRoller/*0x10038b60*/(v6, v5, v4);
        evt.SetConditionArg3(v7);
        break;
    }
  }
}


[DispTypes(DispatcherType.TakingDamage)]
[TempleDllLocation(0x100dd4d0)]
public static void   sub_100DD4D0(in DispatcherCallbackArgs evt, int data1, int data2)
{

  DispIoDamage dispIo;
  int condArg1;
  DamagePacket v4;
  int v5;
  int condArg3;
  int v7;
  int v8;
      DispIoDamage v11;
  CHAR extraText2;
  SpellPacketBody spellPkt;
  GameSystems.ParticleSys.CreateAtObj("sp-Protection from Arrows-Hit", evt.objHndCaller);
  dispIo = evt.GetDispIoDamage();
  v11 = dispIo;
  if ( (dispIo.attackPacket.flags & D20CAF.RANGED)!=0)
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      v4 = &dispIo.damage;
      v5 = v4.GetOverallDamageByType();
      if ( v5 <= evt.GetConditionArg3() )
      {
        v4.AddPhysicalDR(data1, 4, 0x68);
      }
      else
      {
        condArg3 = evt.GetConditionArg3();
        v4.AddPhysicalDR(condArg3, 4, 0x68);
      }
      v7 = v4.GetOverallDamageByType();
      v11.damage.finalDamage = v7;
      if ( v5 > evt.GetConditionArg3() )
      {
        v8 = -v11.damage.finalDamage;
      }
      else
      {
        v8 = v7 - v5 + evt.GetConditionArg3();
      }
      evt.SetConditionArg3(v8);
      Logger.Info("absorbed {0} points of damage, DR points left: {1}", v5 - v7, v8);
      if ( v8 <= 0 )
      {
        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                                SpellEffects.Spell_remove_spell(in evt);
                                SpellEffects.Spell_remove_mod(in evt);
      }
      else
      {
        extraText2 = String.Format(" {0} ({1}/{2:+#;-#;0})", v8, data1, 4);
        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20009, TextFloaterColor.White);
      }
    }
  }
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100c6200)]
public static void   sub_100C6200(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;

  dispIo = evt.GetDispIoAttackBonus();
  dispIo.bonlist.AddBonus(-data1, 0, data2);
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x100d2b20)]
public static void   PotionOfHidingSneaking(in DispatcherCallbackArgs evt)
{
  DispIoObjBonus dispIo;

  dispIo = evt.GetDispIoObjBonus();
  dispIo.bonOut.AddBonus(10, 21, 113);
}


[DispTypes(DispatcherType.GetDefenderConcealmentMissChance)]
[TempleDllLocation(0x100c5be0)]
public static void   sub_100C5BE0(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;

  dispIo = evt.GetDispIoAttackBonus();
  if ( !GameSystems.D20.D20Query(dispIo.attackPacket.attacker, D20DispatcherKey.QUE_Critter_Has_True_Seeing) )
  {
    dispIo.bonlist.AddBonus(data1, 19, data2);
  }
}


[DispTypes(DispatcherType.GetDefenderConcealmentMissChance)]
[TempleDllLocation(0x100cb850)]
public static void   WindWall_Concealment_Chance(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;

  dispIo = evt.GetDispIoAttackBonus();
  if ( (dispIo.attackPacket.flags & D20CAF.RANGED)!=0)
  {
    dispIo.bonlist.AddBonus(data1, 19, data2);
  }
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d5f60)]
public static void   sub_100D5F60(in DispatcherCallbackArgs evt, int data)
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
        v7 = GameSystems.ParticleSys.CreateAtObj("sp-Soften Earth-hit", v5);
        spellPkt.AddTarget(dispIo.tgt, v7, true);
        dispIo.tgt.AddCondition("sp-Soften Earth and Stone Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        v3 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v3);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _soften_earth_and_stone_hit_trigger(): cannot remove target");
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
    Logger.Info("d20_mods_spells.c / _soften_earth_and_stone_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cbd60)]
[TemplePlusLocation("condition.cpp:3588")]
public static void   SpellAddDismissCondition(in DispatcherCallbackArgs evt)
{
  int condArg1;


  int spellId_1;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    spellId_1 = evt.GetConditionArg1();
    if ( !spellPkt.caster.AddCondition("Dismiss", spellId_1, 0, 0) )
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_dismiss(): unable to add condition");
    }
  }
  else
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_dismiss(): unable to get spell_packet");
  }
}
/* Orphan comments:
TP Replaced @ condition.cpp:3588
*/


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c6f70)]
public static void   sub_100C6F70(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoD20Query dispIo;

  dispIo = evt.GetDispIoD20Query();
  dispIo.return_val = 1;
  dispIo.data1 = data1;
  dispIo.data2 = 0;
}


[DispTypes(DispatcherType.TurnBasedStatusInit)]
[TempleDllLocation(0x100ca740)]
public static void   SlowTurnBasedStatusInit(in DispatcherCallbackArgs evt)
{
  DispIOTurnBasedStatus dispIo;
  int *v2;

  dispIo = evt.GetDispIOTurnBasedStatus();
  if ( !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement) )
  {
    v2 = &dispIo.tbStatus.hourglassState;
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


[DispTypes(DispatcherType.TakingDamage)]
[TempleDllLocation(0x100c87e0)]
public static void   HeatMetalDamageResistance(in DispatcherCallbackArgs evt)
{
  int condArg3;
  int v2;
  DispIoDamage dispIo;
  int v4;
  int *v5;
  int v6;
  int v7;
  DamagePacket v8;
  DispIoDamage v9;

  condArg3 = evt.GetConditionArg3();
  v2 = 0;
  if ( condArg3 > 0 )
  {
    dispIo = evt.GetDispIoDamage();
    v9 = dispIo;
    if ( dispIo.damage.diceCount > 0 )
    {
      v4 = dispIo.damage.diceCount;
      v5 = &dispIo.damage.dice[0].rolledDamage;
      do
      {
        if ( *(v5 - 1) == 8 )
        {
          v2 += *v5;
        }
        v5 += 5;
        --v4;
      }
      while ( v4 );
    }
    if ( v2 <= condArg3 )
    {
      v7 = condArg3 - v2;
      v6 = 0;
    }
    else
    {
      v6 = v2 - condArg3;
      v7 = 0;
    }
    if ( v2 > 0 )
    {
      v8 = &dispIo.damage;
      if ( dispIo.damage.GetOverallDamageByType() > 0 )
      {
        v8.AddDR(v2 - v6, DamageType.Cold, 104);
        v9.damage.finalDamage = v8.GetOverallDamageByType();
        evt.SetConditionArg3(v7);
      }
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ce800)]
public static void   MagicMissileOnAdd(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int v2;
  Dice v3;
    SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    if ( GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Spell_Active, 426, 0) )
    {
      GameSystems.Spell.PlayFizzle(evt.objHndCaller);
    }
    else
    {
      v2 = spellPkt.spellId;
      v3 = 1.new Dice(4, 1);
      GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v3, DamageType.Force, 1, D20ActionType.CAST_SPELL, v2, 0);
    }
  }
        SpellEffects.Spell_remove_mod(in evt);
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100d3430)]
[TemplePlusLocation("condition.cpp:503")]
public static void   AoESpellRemove(in DispatcherCallbackArgs evt, int data)
{
  int condArg1;

  int condArg3;
    GameObjectBody v5;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    switch ( data )
    {
      case 0x26:
        v5 = evt.objHndCaller;
        goto LABEL_11;
      case 0x35:
        v5 = evt.objHndCaller;
        goto LABEL_11;
      case 0x66:
        v5 = evt.objHndCaller;
        goto LABEL_11;
      case 0x8B:
        v5 = evt.objHndCaller;
        goto LABEL_11;
      case 0x9D:
        v5 = evt.objHndCaller;
        goto LABEL_11;
      case 0x9F:
        v5 = evt.objHndCaller;
        goto LABEL_11;
      case 0xD2:
        v5 = evt.objHndCaller;
        goto LABEL_11;
      case 0xED:
        v5 = evt.objHndCaller;
LABEL_11:
        GameSystems.ParticleSys.CreateAtObj("sp-Wind Wall-END", v5);
        break;
      default:
        break;
    }
    GameSystems.ParticleSys.End((string )spellPkt.spellObjs[0].partSysId);
    condArg3 = evt.GetConditionArg3();
    RemoveFromObjectEventTable/*0x10044a10*/(condArg3);
                SpellEffects.Spell_remove_mod(evt.WithoutIO);
  }
}
/* Orphan comments:
TP Replaced @ condition.cpp:503
*/


[DispTypes(DispatcherType.Tooltip)]
[TempleDllLocation(0x100c90a0)]
public static void   MirrorImageTooltipCallback(in DispatcherCallbackArgs evt, int data)
{
  DispIoTooltip dispIo;  DispIoTooltip v3;
  int condArg1;
  int v5;
  int condArg3;
  int v7;
  string meslineValue;
int meslineKey;
  CHAR v9;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoTooltip();/*INLINED:v2=evt.subDispNode.condNode*/  v3 = dispIo;
  meslineKey = data;
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
    condArg3 = evt.GetConditionArg3();
    v9 = String.Format("{0} [{1}]", meslineValue, condArg3);
    v7 = v3.numStrings;
    if ( v7 < 10 )
    {
      v3.numStrings = v7 + 1;
      strncpy(v3.strings[v7].text, &v9, 0x100);
    }
  }
}


[DispTypes(DispatcherType.TakingDamage)]
[TempleDllLocation(0x100c7530)]
public static void   EndureElementsDamageResistance(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoDamage dispIo;
  int v2;
  D20DT *v3;
  int condArg4;
  int v5;
  bool v6;
  int v7;
  DamagePacket damPkt;
  int damAmt;
  int v10;
  int v11;
  int v12;
  int condArg3;
  D20DT damType;
  DispIoDamage v15;
  CHAR extraText2;

  dispIo = evt.GetDispIoDamage();
  v15 = dispIo;
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
    if ( !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Protection_From_Elements)
      && !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Resist_Elements)
      || GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Protection_From_Elements)       && (v5 = GameSystems.D20.D20QueryReturnObject(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Protection_From_Elements, 0, 0),
          v6 = v5 == evt.GetConditionArg3(),
          dispIo = v15,
          !v6)
      || GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Resist_Elements)       && (v7 = GameSystems.D20.D20QueryReturnObject(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Resist_Elements, 0, 0),
          v7 != evt.GetConditionArg3()) )
    {
      damPkt = &dispIo.damage;
      damAmt = damPkt.GetOverallDamageByType();
      v10 = damAmt;
      if ( damAmt > 0 )
      {
        if ( damAmt <= condArg4 )
        {
          damPkt.AddDR(damAmt, damType, 104);
          v11 = condArg4 - v10;
        }
        else
        {
          damPkt.AddDR(condArg4, damType, 104);
          v11 = 0;
        }
        v12 = damPkt.GetOverallDamageByType();
        v15.damage.finalDamage = v12;
        evt.SetConditionArg4(v11);
        condArg3 = evt.GetConditionArg3();
        Logger.Info("absorbed {0} points of [{1}] damage, DR points left: {2}", v10 - v12, (&pSpellEntry_Descriptor_Strings/*0x102bfa90*/)[4 * condArg3], v11);
        extraText2 = String.Format(" {0} ({1}/{2:+#;-#;0})", v11, data1, data2);
        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20025, TextFloaterColor.White);
      }
    }
  }
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100c8570)]
public static void   sub_100C8570(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;

  dispIo = evt.GetDispIoAttackBonus();
  if ( (dispIo.attackPacket.flags & D20CAF.RANGED)!=0)
  {
    dispIo.bonlist.AddBonus(-data1, 0, data2);
  }
}


[DispTypes(DispatcherType.TakingDamage)]
[TempleDllLocation(0x100dd6b0)]
public static void   ProtFromElementsDamageResistance(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoDamage dispIo;
  int v2;
  D20DT *v3;
  int condArg4;
  DamagePacket v5;
  int v6;
  int v7;
  int v8;
  int v9;
  int condArg3;
      D20DT damType;
  DispIoDamage v14;
  CHAR extraText2;

  dispIo = evt.GetDispIoDamage();
  v14 = dispIo;
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
    v5 = &dispIo.damage;
    v6 = v5.GetOverallDamageByType();
    v7 = v6;
    if ( v6 > 0 )
    {
      if ( v6 <= condArg4 )
      {
        v5.AddDR(v6, damType, 104);
        v8 = condArg4 - v7;
      }
      else
      {
        v5.AddDR(condArg4, damType, 104);
        v8 = 0;
      }
      v9 = v5.GetOverallDamageByType();
      v14.damage.finalDamage = v9;
      evt.SetConditionArg4(v8);
      condArg3 = evt.GetConditionArg3();
      Logger.Info("absorbed {0} points of [{1}] damage, DR points left: {2}", v7 - v9, (&pSpellEntry_Descriptor_Strings/*0x102bfa90*/)[4 * condArg3], v8);
      if ( v8 <= 0 )
      {
        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                                SpellEffects.Spell_remove_spell(in evt);
                                SpellEffects.Spell_remove_mod(in evt);
      }
      else
      {
        extraText2 = String.Format(" {0} ({1}/{2:+#;-#;0})", v8, data1, data2);
        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20025, TextFloaterColor.White);
      }
    }
  }
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100c6f10)]
public static void   sub_100C6F10(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;

  dispIo = evt.GetDispIoAttackBonus();
  dispIo.bonlist.AddBonus(-data1, 17, data2);
}


[DispTypes(DispatcherType.Unused63)]
[TempleDllLocation(0x100c8f90)]
public static void   MinorGlobeCallback3F(in DispatcherCallbackArgs evt, ConditionSpec data)
{
  DispIoTypeImmunityTrigger dispIo;
  int v2;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoTypeImmunityTrigger();
  v2 = dispIo.okToAdd;
  if ( v2 != evt.GetConditionArg1()
    && GameSystems.Spell.TryGetActiveSpell(v2, out spellPkt)     && spellPkt.spellKnownSlotLevel < 4
    && dispIo.field_C != 48
    && spellPkt.spellEnum != WellKnownSpells.BestowCurse)
  {
    dispIo.interrupt = 1;
    dispIo.val2 = 10;
    dispIo.okToAdd = evt.GetConditionArg1();
  }
}


[DispTypes(DispatcherType.AbilityScoreLevel)]
[TempleDllLocation(0x100c7950)]
public static void   EnlargeStatLevelGet(in DispatcherCallbackArgs evt, int data1, int data2)
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
      if ( (v4 )!=0)
      {
        if ( v4 != 1 )
        {
          return;
        }
        v6 = data2;
        v5 = -2;
      }
      else
      {
        v6 = data2;
        v5 = 2;
      }
      dispIo.bonlist.AddBonus(v5, 20, v6);
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cf110)]
public static void   RaiseDeadOnConditionAdd(in DispatcherCallbackArgs evt)
{
  int condArg1;

  int v3;
    int idxOut;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    idxOut = 0;
    if ( evt.objHndCaller.GetStat(Stat.hp_current) > -10 )
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30016, TextFloaterColor.White);
      GameSystems.Spell.PlayFizzle(evt.objHndCaller);
    }
    else
    {
      FindInObjlist/*0x10075600*/(evt.objHndCaller, spellPkt.targetListHandles, spellPkt.targetCount, &idxOut);
      v3 = GameSystems.ParticleSys.CreateAtObj("sp-Raise Dead", evt.objHndCaller);
      InsertToPartsysIdList/*0x10075510*/(idxOut, spellPkt.targetListPartsysIds, spellPkt.targetCount, v3);
      if ( Resurrection.Resurrect(evt.objHndCaller, HIDWORD(evt.objHndCaller), 0, spellPkt.casterLevel) )
      {
        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20037, TextFloaterColor.White);
        GameSystems.Anim.PushAnimate(evt.objHndCaller, AnimGoalType.animate_door_closed);
      }
      else
      {
        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20036, TextFloaterColor.Red);
      }
    }
  }
        SpellEffects.Spell_remove_mod(in evt);
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100ca9f0)]
public static void   sub_100CA9F0(in DispatcherCallbackArgs evt)
{
  int condArg3;
  DispIoD20Query dispIo;

  condArg3 = evt.GetConditionArg3();
  dispIo = evt.GetDispIoD20Query();
  dispIo.data1 = condArg3;
  dispIo.return_val = 1;
  dispIo.data2 = (ulong)condArg3 >> 32;
}


[DispTypes(DispatcherType.ObjectEvent)]
[TempleDllLocation(0x100d5950)]
public static void   sub_100D5950(in DispatcherCallbackArgs evt, int data)
{
  DispIoObjEvent dispIo;
  int condArg1;
  string v3;


  GameObjectBody v7;

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
        if ( GameSystems.Critter.IsCategory(dispIo.tgt, MonsterCategory.animal) && dispIo.tgt.GetStat(Stat.size) < 6 )
        {
          spellPkt.AddTarget(dispIo.tgt, 0, true);
          dispIo.tgt.AddCondition("sp-Repel Vermin Hit", spellPkt.spellId, spellPkt.durationRemaining, dispIo.evtId);
        }
        else
        {
          v7 = dispIo.tgt;
          GameSystems.ParticleSys.CreateAtObj("Fizzle", v7);
        }
      }
      else if ( evt.dispKey == D20DispatcherKey.OnLeaveAoE )
      {
        v3 = (string )spellPkt.GetPartSysForTarget(evt.objHndCaller);
        GameSystems.ParticleSys.End(v3);
        if ( !spellPkt.RemoveTarget(evt.objHndCaller) )
        {
          Logger.Info("d20_mods_spells.c / _repel_vermin_hit_trigger(): cannot remove target");
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
    Logger.Info("d20_mods_spells.c / _repel_vermin_hit_trigger(): unable to save new spell_packet");
  }
*/}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x100c7140)]
public static void   sub_100C7140(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoObjBonus dispIo;

  dispIo = evt.GetDispIoObjBonus();
  if ( evt.dispKey == D20DispatcherKey.D20A_FALL_TO_PRONE )
  {
    dispIo.bonOut.AddBonus(data1, 0, data2);
  }
}


[DispTypes(DispatcherType.DealingDamage)]
[TempleDllLocation(0x100c7360)]
public static void   AddBonusType17(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoDamage dispIo;

  dispIo = evt.GetDispIoDamage();
  dispIo.damage.AddDamageBonus(data1, 17, data2);
}


[DispTypes(DispatcherType.D20Query)]
[TempleDllLocation(0x100c7820)]
public static void   sub_100C7820(in DispatcherCallbackArgs evt)
{
  DispIoD20Query dispIo;

  dispIo = evt.GetDispIoD20Query();
  dispIo.return_val = 1;
  dispIo.obj = evt.GetConditionArg3();
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100d3100)]
[TemplePlusLocation("condition.cpp:3526")]
public static void   OnSequenceConcentrating(in DispatcherCallbackArgs evt, int data)
{
  ActionSequence actSeq;
  int condArg1;
  int i;
  D20Action d20a;
    SpellPacketBody spellPkt;

  actSeq = (ActionSequence )evt.GetDispIoD20Signal().data1;
  if ( actSeq !=null)
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      if ( spellPkt.spellEnum != WellKnownSpells.MeldIntoStone)
      {
        i = 0;
        if ( actSeq.d20ActArrayNum > 0 )
        {
          d20a = (D20Action )actSeq;
          do
          {
            if ( D20ActionBreaksConcentration/*0x1008a180*/(d20a.d20ActType) )
            {
              if ( d20a.d20ActType == D20ActionType.CAST_SPELL&& d20a.spellId == evt.GetConditionArg1() )
              {
                return;
              }
              *(_DWORD *)&v5[20] = 0;
              *(_QWORD *)&v5[0xC] = 0x9C0000001Ci64;
              *(_QWORD *)&v5[4] = evt.objHndCaller;
              *(_DWORD *)v5 = evt.subDispNode;
              SpellEffects.Spell_remove_mod(in evt);
            }
            ++i;
            ++d20a;
          }
          while ( i < actSeq.d20ActArrayNum );
        }
      }
    }
    else
    {
      Logger.Info("d20_mods_spells.c / _concentrating_action_recipient(): error, unable to retrieve concentration_packet");
    }
  }
}
/* Orphan comments:
TP Replaced @ condition.cpp:3526
*/


[DispTypes(DispatcherType.ConditionAddPre)]
[TempleDllLocation(0x100c8240)]
public static void   sub_100C8240(in DispatcherCallbackArgs evt, ConditionSpec data)
{
  int condArg3;

  condArg3 = evt.GetConditionArg3();
  evt.GetDispIoCondStruct().outputFlag = condArg3 <= 8;
}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100c8d60)]
[TemplePlusLocation("spell_condition.cpp:77")]
public static void   MagicCircleTakingDamage(in DispatcherCallbackArgs evt, int data)
{
  DispIoDamage dispIo;
  GameObjectBody itemMain;
  int weapMainLo;
  int weapMainHi;
  GameObjectBody weaponSec;
  int condArg1;
  string v7;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoDamage();
  itemMain = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, 203);
  weapMainLo = HIDWORD(itemMain);
  weapMainHi = itemMain;
  weaponSec = GameSystems.Item.GetItemAtInvIdx(evt.objHndCaller, 204);
  if ( (weapMainLo | weapMainHi)==0&& (weaponSec == null))
  {
    condArg1 = evt.GetConditionArg1();
    if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
    {
      if ( dispIo.attackPacket.attacker.HasCondition(SpellEffects.SpellSummoned) )
      {
        switch ( spellPkt.spellEnum )
        {
          case WellKnownSpells.ProtectionFromChaos:
            if ( dispIo.attackPacket.attacker.HasLawfulAlignment() )
            {
              goto LABEL_13;
            }
            break;
          case WellKnownSpells.ProtectionFromEvil:
            if ( !(dispIo.attackPacket.attacker.HasGoodAlignment()) )
            {
              goto LABEL_13;
            }
            break;
          case WellKnownSpells.ProtectionFromGood:
            if ( !(dispIo.attackPacket.attacker.HasEvilAlignment()) )
            {
              goto LABEL_13;
            }
            break;
          case WellKnownSpells.ProtectionFromLaw:
            if ( dispIo.attackPacket.attacker.HasChaoticAlignment() )
            {
LABEL_13:
              if ( !D20ModSpells.CheckSpellResistance(&spellPkt, evt.objHndCaller) )
              {
                dispIo.damage.AddModFactor(0F, DamageType.Unspecified, 104);
              }
            }
            break;
          default:
            v7 = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
            Logger.Info("d20_mods_spells.c / _protection_from_alignment_prevent_damage(): invalid spell=( {0} )", v7);
            break;
        }
      }
    }
    else
    {
      Logger.Info("d20_mods_spells.c / _magic_circle_prevent_damage(): unable to retrieve spell_packet");
    }
  }
}
/* Orphan comments:
TP Replaced @ spell_condition.cpp:77
*/


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100c9280)]
public static void   sub_100C9280(in DispatcherCallbackArgs evt, int data)
{
  int condArg3;
  int v2;
  int v3;
  BonusList *v4;

  condArg3 = evt.GetConditionArg3();
  v2 = data;
  v3 = condArg3;
  v4 = &evt.GetDispIoAttackBonus().bonlist;
  if ( v3 != -3 )
  {
    v2 = -v2;
  }
  v4.AddBonus(v2, 14, 151);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cdeb0)]
public static void   HarmOnAdd(in DispatcherCallbackArgs evt)
{
  int condArg1;
  signed Dice v2;
  int v3;
  int v4;

  GameObjectBody v6;
  int v7;
  int v8;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    v2 = Dice.Constant(10 * spellPkt.casterLevel);
    v3 = evt.objHndCaller.GetStat(Stat.hp_current);
    if ( 10 * spellPkt.casterLevel >= v3 )
    {
      v4 = evt.objHndCaller.GetStat(Stat.hp_current);
      v2 = Dice.Constant(v4 - 1);
    }
    if ( GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc, SavingThrowType.Will, 0, spellPkt.spellId) )
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30001, TextFloaterColor.White);
      GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
      evt.SetConditionArg3(1);
      v8 = spellPkt.spellId;
      v7 = v2 / 2;
      v6 = spellPkt.caster;
    }
    else
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30002, TextFloaterColor.White);
      v8 = spellPkt.spellId;
      v7 = v2;
      v6 = spellPkt.caster;
    }
    GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, v6, v7, DamageType.NegativeEnergy, 4, D20ActionType.CAST_SPELL, v8, 0);
  }
}


[DispTypes(DispatcherType.TakingDamage)]
[TempleDllLocation(0x100c66d0)]
public static void   ChillMetalDamageResistance(in DispatcherCallbackArgs evt)
{
  int condArg3;
  int v2;
  DispIoDamage dispIo;
  int v4;
  int *v5;
  int v6;
  int v7;
  DamagePacket v8;
  DispIoDamage v9;

  condArg3 = evt.GetConditionArg3();
  v2 = 0;
  if ( condArg3 > 0 )
  {
    dispIo = evt.GetDispIoDamage();
    v9 = dispIo;
    if ( dispIo.damage.diceCount > 0 )
    {
      v4 = dispIo.damage.diceCount;
      v5 = &dispIo.damage.dice[0].rolledDamage;
      do
      {
        if ( *(v5 - 1) == 10 )
        {
          v2 += *v5;
        }
        v5 += 5;
        --v4;
      }
      while ( v4 );
    }
    if ( v2 <= condArg3 )
    {
      v7 = condArg3 - v2;
      v6 = 0;
    }
    else
    {
      v6 = v2 - condArg3;
      v7 = 0;
    }
    if ( v2 > 0 )
    {
      v8 = &dispIo.damage;
      if ( dispIo.damage.GetOverallDamageByType() > 0 )
      {
        v8.AddDR(v2 - v6, DamageType.Fire, 104);
        v9.damage.finalDamage = v8.GetOverallDamageByType();
        evt.SetConditionArg3(v7);
      }
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ceef0)]
public static void   D20ModsSpells_ProtectionElementsDamageReductionRestore(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int v2;
  int condArg3;
  int condArg2;
  int v5;
  GameObjectBody v6;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    v2 = 12 * spellPkt.casterLevel;
    if ( 12 * spellPkt.casterLevel > 120 )
    {
      v2 = 120;
    }
    evt.SetConditionArg4(v2);
    switch ( evt.GetConditionArg2() )
    {
      case 1:
        v6 = evt.objHndCaller;
        v5 = 13384;
        goto LABEL_10;
      case 3:
        v6 = evt.objHndCaller;
        v5 = 13386;
        goto LABEL_10;
      case 6:
        v6 = evt.objHndCaller;
        v5 = 13392;
        goto LABEL_10;
      case 9:
        v6 = evt.objHndCaller;
        v5 = 13388;
        goto LABEL_10;
      case 16:
        v6 = evt.objHndCaller;
        v5 = 13390;
LABEL_10:
        GameSystems.SoundGame.PositionalSound(v5, 1, v6);
        condArg3 = evt.GetConditionArg3();
        condArg2 = evt.GetConditionArg2();
        evt.SetConditionArg3(condArg2);
        evt.SetConditionArg2(condArg3);
        break;
      default:
        return;
    }
  }
  else
  {
    Logger.Info("d20_mods_spells.c / _protection_elements_damage_reduction_restore(): unable to get spell_packet");
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ca7a0)]
public static void   sub_100CA7A0(in DispatcherCallbackArgs evt)
{
  int condArg1;


  Dice v4;
  int v5;
  unsigned Dice v6;
  int v7;
  Dice v8;
  int v9;
  int v10;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt)     && !GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc, SavingThrowType.Reflex, 0, spellPkt.spellId) )
  {
    v4 = 1.new Dice(2, 0);
    v5 = GetPackedDiceBonus/*0x10038c90*/(v4);
    v6 = 1.new Dice(2, 0);
    v7 = GetPackedDiceType/*0x10038c40*/(v6);
    v8 = 1.new Dice(2, 0);
    v9 = GetPackedDiceNumDice/*0x10038c30*/(v8);
    v10 = DiceRoller/*0x10038b60*/(v9, v7, v5);
    if ( !evt.objHndCaller.AddCondition("sp-Soften Earth and Stone Hit Save Failed", spellPkt.spellId, v10, 0) )
    {
      Logger.Info("d20_mods_spells.c / _soften_earth_and_stone_hit(): unable to add condition");
    }
  }
}


[DispTypes(DispatcherType.SaveThrowLevel)]
[TempleDllLocation(0x100c9790)]
public static void   AddBonusType13(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoSavingThrow dispIo;

  dispIo = evt.GetDispIoSavingThrow();
  dispIo.bonlist.AddBonus(data1, 13, data2);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100c71f0)]
public static void   d20_mods_spells__desecrate_undead_temp_hp(in DispatcherCallbackArgs evt, int data1, int data2)
{
  int condArg1;
  int v2;


  int condArg2;
  CHAR extraText;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt)     && evt.objHndCaller.HasCondition(SpellEffects.SpellSummoned) )
  {
    v2 = GameSystems.Critter.GetHitDiceNum(evt.objHndCaller) * data1;
    extraText = String.Format("[{0}] ", v2);
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20005, TextFloaterColor.White, &extraText);
    Logger.Info("d20_mods_spells.c / _desecrate_undead_temp_hp(): gained {0} temporary hit points", v2);
    condArg2 = evt.GetConditionArg2();
    if ( !evt.objHndCaller.AddCondition("Temporary_Hit_Points", spellPkt.spellId, condArg2, v2) )
    {
      Logger.Info("d20_mods_spells.c / _desecrate_undead_temp_hp(): unable to add condition");
    }
  }
}


[DispTypes(DispatcherType.ToHitBonus2)]
[TempleDllLocation(0x100c60b0)]
public static void   RighteousMightToHitBonus(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;

  dispIo = evt.GetDispIoAttackBonus();
  dispIo.bonlist.AddBonus(data1, 35, data2);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100ce8d0)]
public static void   MeldIntoStoneBeginSpell(in DispatcherCallbackArgs evt)
{
  int condArg1;


  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    if ( !spellPkt.caster.AddCondition("sp-Concentrating", condArg1, 0, 0) )
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_meld_into_stone(): unable to add condition to spell_caster");
    }
  }
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100ddda0)]
public static void   VampiricTouchSignalTouchAttack(in DispatcherCallbackArgs evt, int data)
{
  int v1;
  int condArg1;
      Dice v5;
      int v8;
  int v9;
  SpellPacketBody spellPkt;

  v1 = evt.GetDispIoD20Signal().data1;
  if ( *(_BYTE *)(v1 + 8) & 1 )
  {
    condArg1 = evt.GetConditionArg1();
    GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt);
    GameSystems.Script.Spells.SpellSoundPlay(&spellPkt, OnAreaOfEffectHit);
    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 68);
    if ( D20ModSpells.CheckSpellResistance(&spellPkt, *(_QWORD *)(v1 + 24)) )
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, spell_mes_A_Spell_has_expired, TextFloaterColor.White);
                        SpellEffects.Spell_remove_spell(evt.WithoutIO);
                        SpellEffects.Spell_remove_mod(evt.WithoutIO);
    }
    else
    {
      if ( *(_BYTE *)(v1 + 8) & 2 )
      {
        v9 = spellPkt.casterLevel;
        v8 = 2;
      }
      else
      {
        v9 = spellPkt.casterLevel;
        v8 = 1;
      }
      v5 = v8.new Dice(6, v9);
      GameSystems.D20.Combat.SpellDamageFull(*(_QWORD *)(v1 + 24), evt.objHndCaller, v5, DamageType.Magic, 1, D20ActionType.CAST_SPELL, spellPkt.spellId, 0);
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


[DispTypes(DispatcherType.BeginRound)]
[TempleDllLocation(0x100cb1f0)]
public static void   sub_100CB1F0(in DispatcherCallbackArgs evt, int data)
{
  int condArg1;

  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    if ( GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc, SavingThrowType.Will, 0, spellPkt.spellId) )
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30001, TextFloaterColor.White);
      GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
      evt.SetConditionArg3(0);
      GameSystems.AI.AiProcess(evt.objHndCaller);
    }
    else
    {
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30002, TextFloaterColor.White);
      evt.SetConditionArg3(1);
    }
  }
}


[DispTypes(DispatcherType.EffectTooltip)]
[TempleDllLocation(0x100c3f20)]
public static void   EffectTooltipBestowCurse(in DispatcherCallbackArgs evt, int data)
{
  DispIoEffectTooltip dispIo;
  int condArg1;
  int condArg3;
  string v4;
  int v5;
  int v6;
  string v7;
  string v8;
  CHAR extraString;
  SpellPacketBody spellPkt;

  dispIo = evt.GetDispIoEffectTooltip();
  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    condArg3 = evt.GetConditionArg3();
    v4 = GameSystems.D20.RadialMenu.GetAbilityReducedName(condArg3 + 161);
    v5 = spellPkt.duration;
    v6 = spellPkt.durationRemaining;
    v7 = v4;
    v8 = GameSystems.D20.Combat.GetCombatMesLine(0xAF);
    extraString = String.Format("({0}) {1}: {2}/{3}", v7, v8, v6, v5);
    EffectTooltipAppend/*0x100f4680*/(dispIo.bdb, data, spellPkt.spellEnum, &extraString);
  }
}


[DispTypes(DispatcherType.GetAttackerConcealmentMissChance)]
[TempleDllLocation(0x100c62a0)]
public static void   sub_100C62A0(in DispatcherCallbackArgs evt, int data)
{
  DispIoObjBonus dispIo;

  dispIo = evt.GetDispIoObjBonus();
  dispIo.bonOut.AddBonus(data, 19, 254);
}


[DispTypes(DispatcherType.AbilityCheckModifier)]
[TempleDllLocation(0x100c9060)]
public static void   sub_100C9060(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoBonusList dispIo;

  dispIo = evt.GetDispIoBonusList();
  if ( evt.dispKey == D20DispatcherKey.STAT_WISDOM )
  {
    dispIo.bonlist.AddBonus(-data1, 34, data2);
  }
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x100c8530)]
public static void   sub_100C8530(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoObjBonus dispIo;

  dispIo = evt.GetDispIoObjBonus();
  if ( evt.dispKey == D20DispatcherKey.D20A_DOUBLE_MOVE )
  {
    dispIo.bonOut.AddBonus(-data1, 0, data2);
  }
}


[DispTypes(DispatcherType.TakingDamage2)]
[TempleDllLocation(0x100cb7d0)]
public static void   WebOnBurningCallback(in DispatcherCallbackArgs evt, int data)
{
  int condArg1;
  DispIoDamage dispIo;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    dispIo = evt.GetDispIoDamage();
    if ( GetDamageTypeOverallDamage/*0x100e1210*/(&dispIo.damage, DamageType.Fire) > 0 )
    {
      GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Web_Burning, spellPkt.spellId, 0);
    }
  }
}


[DispTypes(DispatcherType.GetDefenderConcealmentMissChance)]
[TempleDllLocation(0x100c7a00)]
public static void   sub_100C7A00(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoAttackBonus dispIo;


  dispIo = evt.GetDispIoAttackBonus();
  if ( (dispIo.attackPacket.flags & D20CAF.RANGED)!=0)
  {
    GameSystems.ParticleSys.CreateAtObj("sp-Entropic Shield-HIT", evt.objHndCaller);
    dispIo.bonlist.AddBonus(data1, 19, data2);
  }
}


[DispTypes(DispatcherType.SkillLevel)]
[TempleDllLocation(0x100c4e50)]
public static void   EmotionSkillBonus(in DispatcherCallbackArgs evt, int data1, int data2)
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
    case 0x12A:
    case 0x12B:
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


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100d0730)]
public static void   FrogTongueOnAdd(in DispatcherCallbackArgs evt)
{
  int condArg1;
  GameObjectBody tgt;




  int v8;


  int v11;
  SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    tgt = spellPkt.targetListHandles[0];
    if ( !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Unconscious) )
    {
      condArg1 = evt.GetConditionArg1();
      if ( !tgt.AddCondition("Grappled", condArg1, 1, 0) )
      {
        Logger.Info("d20_mods_spells.c / _begin_spell_frog_tongue(): unable to add condition");
      }
      v8 = evt.GetConditionArg1();
      if ( !tgt.AddCondition("sp-Frog Tongue Grappled", v8, 0, 0) )
      {
        Logger.Info("d20_mods_spells.c / _begin_spell_frog_tongue(): unable to add condition");
      }
      FrogGrappleController.PlayLatch(evt.objHndCaller);
      GameSystems.D20.Actions.PerformOnAnimComplete(evt.objHndCaller, -1);
      v11 = evt.GetConditionArg1();
      if ( !evt.objHndCaller.AddCondition("Grappled", v11, 0, 0) )
      {
        Logger.Info("d20_mods_spells.c / _begin_spell_frog_tongue(): unable to add condition");
      }
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cfd50)]
public static void   sub_100CFD50(in DispatcherCallbackArgs evt)
{
  int condArg3;
  CHAR extraText2;

  condArg3 = evt.GetConditionArg3();
  extraText2 = String.Format(" [{0}]", condArg3);
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20034, TextFloaterColor.White);
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100de090)]
public static void   sub_100DE090(in DispatcherCallbackArgs evt, int data)
{
  int condArg1;
  GameObjectBody v2;
  int v3;
  int v4;
  int v5;
      SpellPacketBody spellPkt;

  condArg1 = evt.GetConditionArg1();
  if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
  {
    LODWORD(v2) = evt.objHndCaller;
    if ( LODWORD(spellPkt.caster) == LODWORD(evt.objHndCaller) )
    {
      HIDWORD(v2) = HIDWORD(evt.objHndCaller);
      if ( HIDWORD(spellPkt.caster) == HIDWORD(evt.objHndCaller)
        && GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Unconscious) )
      {
        GameSystems.D20.Actions.PerformOnAnimComplete(v2, -1);
        sub_10020A60/*0x10020a60*/(spellPkt.caster);
        GameObjectRenderExtensions.GetIdleAnimId(spellPkt.caster);
        v3 = GameSystems.Critter.GetAnimId(spellPkt.caster, 9);
        spellPkt.caster.SetAnimId(v3);
        v4 = GameObjectRenderExtensions.GetIdleAnimId(v2);
        v5 = GameSystems.Critter.GetAnimId(v2, v4);
        v2.SetAnimId(v5);
        sub_100D26F0/*0x100d26f0*/(v2);
        if ( spellPkt.targetListHandles[0] )
        {
          GameSystems.D20.D20SendSignal(spellPkt.targetListHandles[0], D20DispatcherKey.SIG_Spell_Grapple_Removed, spellPkt.spellId, 0);
          GameSystems.D20.D20SendSignal(spellPkt.targetListHandles[0], D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          if ( spellPkt.targetListHandles[0] )
          {
            GameSystems.D20.D20SendSignal(spellPkt.targetListHandles[0], D20DispatcherKey.SIG_Spell_Grapple_Removed, spellPkt.spellId, 0);
            GameSystems.D20.D20SendSignal(spellPkt.targetListHandles[0], D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
          }
        }
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_Grapple_Removed, spellPkt.spellId, 0);
        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                                SpellEffects.Spell_remove_spell(evt.WithoutIO);
                                SpellEffects.Spell_remove_mod(evt.WithoutIO);
      }
    }
  }
}


[DispTypes(DispatcherType.TakingDamage)]
[TempleDllLocation(0x100ca4a0)]
public static void   sub_100CA4A0(in DispatcherCallbackArgs evt)
{

  GameSystems.ParticleSys.CreateAtObj("sp-Shield-hit", evt.objHndCaller);
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cc520)]
public static void   BeginSpellCharmMonster(in DispatcherCallbackArgs evt)
{
  int condArg1;
  int condArg2;
  int condArg3;



  condArg1 = evt.GetConditionArg1();
  condArg2 = evt.GetConditionArg2();
  condArg3 = evt.GetConditionArg3();
  if ( !evt.objHndCaller.AddCondition("Charmed", condArg1, condArg2, condArg3) )
  {
    Logger.Info("d20_mods_spells.c / _begin_spell_charm_monster(): unable to add condition");
  }
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20018, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.GetSizeCategory)]
[TempleDllLocation(0x100c97f0)]
public static void   sub_100C97F0(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoD20Query dispIo;
  int v2;

  dispIo = evt.GetDispIoD20Query();
  v2 = dispIo.return_val;
  if ( v2 > 1 )
  {
    dispIo.return_val = v2 - 1;
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cf560)]
public static void   SpRestorationOnConditionAdd(in DispatcherCallbackArgs evt)
{  int v2;
  int v3;
  string v4;
  string v5;
  bool v6;
  byte v7;
  int condArg3;  int v10;
  int v11;
  string v12;
  string v13;
  int v14;
  int v15;
  int v16;
  int v17;
  int v18;
  Stat v19;
  DispIoAbilityLoss dispIo;
  CHAR extraText2;

  v19 = (Stat)0;
  do
  {
    dispIo = new DispIoAbilityLoss();/*INLINED:v1=evt.subDispNode.condNode*/    dispIo.statDamaged = (Stat)v19;
    dispIo.flags |= 0x19;
    dispIo.field_C = 1;
    dispIo.spellId = evt.GetConditionArg1();
    dispIo.result = 0;
    v2 = evt.objHndCaller.DispatchGetAbilityLoss(&dispIo);
    v3 = -v2;
    if ( -v2 >= 0 && v2 != 0 )
    {
      v4 = GameSystems.Stat.GetStatName(dispIo.statDamaged);
      Logger.Info("d20_mods_spells.c / _begin_spell_restoration(): healed {0} points of temporary ({1}) damage", v3, v4);
      v5 = GameSystems.Stat.GetStatName(dispIo.statDamaged);
      extraText2 = String.Format(": {0} [{1}]", v5, v3);
      GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20035, TextFloaterColor.White);
    }
    v7 = __OFSUB__(v19 + 1, 5);
    v6 = v19++ - 4 < 0;
  }
  while ( v6 ^ v7 );
  dispIo = new DispIoAbilityLoss();
  condArg3 = evt.GetConditionArg3();/*INLINED:v9=evt.subDispNode.condNode*/  dispIo.statDamaged = (Stat)condArg3;
  dispIo.flags |= 0x1A;
  dispIo.field_C = 1;
  dispIo.spellId = evt.GetConditionArg1();
  dispIo.result = 0;
  v10 = evt.objHndCaller.DispatchGetAbilityLoss(&dispIo);
  v11 = -v10;
  if ( -v10 >= 0 && v10 != 0 )
  {
    v12 = GameSystems.Stat.GetStatName(dispIo.statDamaged);
    Logger.Info("d20_mods_spells.c / _begin_spell_restoration(): healed {0} points of permanent({1}) damage", v11, v12);
    v13 = GameSystems.Stat.GetStatName(dispIo.statDamaged);
    extraText2 = String.Format(": {0} [{1}]", v13, v11);
    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20035, TextFloaterColor.White);
  }
  nullsub_1/*0x100027f0*/();
  v14 = evt.objHndCaller.GetArrayLength(obj_f.critter_level_idx);
  v15 = evt.objHndCaller.GetInt32(obj_f.critter_experience);
  v16 = NextLevel/*0x10080300*/(v15);
  if ( v16 < v14 )
  {
    v17 = GameSystems.Level.GetExperienceForLevel(v16 + 1);
    evt.objHndCaller.SetInt32(obj_f.critter_experience, v17);
    v18 = evt.objHndCaller.GetInt32(obj_f.critter_experience);
    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Experience_Awarded, v18, (ulong)v18 >> 32);
  }
}


[DispTypes(DispatcherType.TurnBasedStatusInit)]
[TempleDllLocation(0x100c6dc0)]
public static void   sub_100C6DC0(in DispatcherCallbackArgs evt)
{
  Dice v1;
  int v2;
  unsigned Dice v3;
  int v4;
  Dice v5;
  int v6;
  int v7;
  int condArg1;
  int v9;
  ConditionAttachment v10;
  int v11;
  SpellPacketBody spellPkt;

  v1 = 1.new Dice(100, 0);
  v2 = GetPackedDiceBonus/*0x10038c90*/(v1);
  v3 = 1.new Dice(100, 0);
  v4 = GetPackedDiceType/*0x10038c40*/(v3);
  v5 = 1.new Dice(100, 0);
  v6 = GetPackedDiceNumDice/*0x10038c30*/(v5);
  v7 = DiceRoller/*0x10038b60*/(v6, v4, v2);
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20038, TextFloaterColor.Red);
  condArg1 = evt.GetConditionArg1();
  GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt);
  v9 = evt.GetConditionArg3() - 5;
  if ( (v9 )==0|| v9 == 2 )
  {
    GameSystems.AI.StopFleeing(evt.objHndCaller);
  }
  if ( v7 <= 10 )
  {
    v11 = 8;
    v10 = evt.subDispNode.condNode;
LABEL_15:
    CondNodeSetArg/*0x100e1ad0*/(v10, 2, v11);
    goto LABEL_16;
  }
  if ( v7 <= 20 )
  {
    v11 = 0;
LABEL_14:
    v10 = evt.subDispNode.condNode;
    goto LABEL_15;
  }
  if ( v7 <= 50 )
  {
    v11 = 6;
    v10 = evt.subDispNode.condNode;
    goto LABEL_15;
  }
  if ( v7 <= 70 )
  {
    v11 = 5;
    v10 = evt.subDispNode.condNode;
    goto LABEL_15;
  }
  if ( v7 <= 100 )
  {
    v11 = 9;
    goto LABEL_14;
  }
LABEL_16:
  GameSystems.AI.AiProcess(evt.objHndCaller);
}


[DispTypes(DispatcherType.D20Signal)]
[TempleDllLocation(0x100ca140)]
public static void   SanctuaryAttemptSave(in DispatcherCallbackArgs evt)
{
  D20Action v1;
  int condArg1;
  int v3;
  int v4;
  int v5;


  int spellEnum;
  SpellPacketBody spellPkt;

  spellEnum = 0;
  v1 = (D20Action )evt.GetDispIoD20Signal().data1;
  if ( v1 !=null)
  {
    if ( v1.d20APerformer != evt.objHndCaller )
    {
      if ( IsActionOffensive/*0x1008acc0*/(v1.d20ActType, v1.d20ATarget) )
      {
        if ( v1.d20ActType != D20ActionType.CAST_SPELL          || (GameSystems.D20.RadialMenu.SelectedRadialMenuEntry.d20SpellData.SpellEnum(&v1.d20SpellData, &spellEnum, 0, 0, 0, 0, 0),
              GameSystems.Spell.IsSpellHarmful(spellEnum, v1.d20APerformer, v1.d20ATarget)) )
        {
          condArg1 = evt.GetConditionArg1();
          if ( GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) )
          {
            v3 = GameSystems.D20.Combat.SavingThrowSpell(v1.d20APerformer, spellPkt.caster, spellPkt.dc, SavingThrowType.Will, 0, spellPkt.spellId);
            v4 = v1.d20APerformer;
            v5 = HIDWORD(v1.d20APerformer);
            if ( (v3 )!=0)
            {
              GameSystems.Spell.FloatSpellLine(__PAIR__(v5, v4), 30001, TextFloaterColor.White);
            }
            else
            {
              GameSystems.Spell.FloatSpellLine(__PAIR__(v5, v4), 30002, TextFloaterColor.White);
            }
            v1.d20APerformer.AddCondition("sp-Sanctuary Save Failed", spellPkt.spellId, 0, 0);
          }
        }
      }
    }
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cd370)]
public static void   sub_100CD370(in DispatcherCallbackArgs evt)
{
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20010, TextFloaterColor.Red);
}


[DispTypes(DispatcherType.GetMoveSpeedBase)]
[TempleDllLocation(0x100c78d0)]
public static void   entangleMoveRestrict(in DispatcherCallbackArgs evt, int data1, int data2)
{
  DispIoMoveSpeed dispIo;

  dispIo = evt.GetDispIoMoveSpeed();
  if ( !evt.objHndCaller.HasCondition(SpellEffects.SpellControlPlantsDisentangle)
    && !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement) )
  {
    dispIo.bonlist.SetOverallCap(1, 0, 0, data2);
    dispIo.bonlist.SetOverallCap(2, 0, 0, data2);
  }
}


[DispTypes(DispatcherType.ConditionAdd)]
[TempleDllLocation(0x100cfaf0)]
public static void   sub_100CFAF0(in DispatcherCallbackArgs evt)
{
  GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20015, TextFloaterColor.Red);
}


    }
}