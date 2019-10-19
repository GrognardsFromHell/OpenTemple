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

    }
}