using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Particles.Render;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Utils;
using SpicyTemple.Core.Systems.RadialMenus;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.Script;

namespace SpicyTemple.Core.Systems.D20.Conditions
{
    public static partial class SpellEffects
    {
        [DispTypes(DispatcherType.D20Signal, DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100d7620)]
        [TemplePlusLocation("spell_condition.cpp:108")]
        public static void Spell_remove_spell(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var spellId = evt.GetConditionArg1();
            DispIoD20Signal dispIo = null;
            if (evt.dispIO is DispIoD20Signal)
            {
                dispIo = evt.GetDispIoD20Signal();
            }

            if (evt.dispKey == D20DispatcherKey.SIG_Sequence)
            {
                Logger.Info(
                    "d20_mods_spells.c / _remove_spell: [WARNING:] caught a D20est_S_Sequence, make sure we are removing spell properly...");
            }
            else
            {
                if (evt.dispKey != D20DispatcherKey.SIG_Killed
                    && evt.dispKey != D20DispatcherKey.SIG_Critter_Killed
                    && evt.dispKey != D20DispatcherKey.SIG_Sequence
                    && evt.dispKey != D20DispatcherKey.SIG_Spell_Cast
                    && evt.dispKey != D20DispatcherKey.SIG_Concentration_Broken
                    && evt.dispKey != D20DispatcherKey.SIG_Action_Recipient
                    && evt.dispKey != D20DispatcherKey.SIG_TouchAttackAdded
                    && evt.dispKey != D20DispatcherKey.SIG_Teleport_Prepare
                    && evt.dispKey != D20DispatcherKey.SIG_Teleport_Reconnect
                    && dispIo != null
                    && dispIo.data1 != spellId)
                {
                    Logger.Info("Not removing spell mod for spell {0} because dispatch was for spell {1}", spellId,
                        dispIo.data1);
                    return;
                }
            }

            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                Logger.Info("d20_mods_spells.c / _remove_spell(): error getting spellid packet for spell_packet");
                return;
            }

            var critterName = GameSystems.MapObject.GetDisplayName(evt.objHndCaller);
            var spellName = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
            Logger.Info("d20_mods_spells.c / _remove_spell(): removing spell=( {0} ) on obj=( {1} )", spellName,
                critterName);

            switch (spellPkt.spellEnum)
            {
                case WellKnownSpells.Aid:
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.AnimalFriendship:
                    if (!RemoveAnimalFriendship(in evt))
                    {
                        return;
                    }

                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.AnimalGrowth:
                    ResetModelScaleAndSpeed(evt.objHndCaller, 1.8f);

                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.AnimalTrance:
                    if (!RemoveAnimalTrance(in evt))
                    {
                        return;
                    }

                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    spellPkt.EndPartSysForCaster();
                    GameSystems.ParticleSys.CreateAtObj("sp-Animal Trance-END", spellPkt.caster);
                    if (evt.dispKey == D20DispatcherKey.SIG_Concentration_Broken)
                    {
                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                            spellPkt.spellId);
                    }
                    else
                    {
                        foreach (var target in spellPkt.Targets)
                        {
                            GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End,
                                spellPkt.spellId);
                        }

                        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Remove_Concentration,
                            spellId);
                    }

                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.BlindnessDeafness:
                    spellId = spellPkt.spellId;
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.BullsStrength:
                    GameSystems.ParticleSys.CreateAtObj("sp-Shield-END", evt.objHndCaller);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.CalmAnimals:
                    if (!RemoveSpellCalmAnimals(in evt))
                    {
                        return;
                    }

                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.CalmEmotions:
                    if (!RemoveSpellCalmEmotions(in evt))
                    {
                        return;
                    }

                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    spellPkt.EndPartSysForCaster();
                    GameSystems.ParticleSys.CreateAtObj("sp-Calm Emotions-END", spellPkt.caster);
                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                            0);
                    }

                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Remove_Concentration, spellId,
                        0);
                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.CatsGrace:
                    GameSystems.ParticleSys.CreateAtObj("sp-Entropic Shield-END", evt.objHndCaller);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.CauseFear:
                case WellKnownSpells.Fear:
                case WellKnownSpells.Scare:
                    if (evt.objHndCaller.IsNPC())
                    {
                        GameSystems.AI.StopFleeing(evt.objHndCaller);
                    }

                    spellId = spellPkt.spellId;
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.CharmMonster:
                    if (!RemoveSpellCharmMonster(in evt))
                    {
                        return;
                    }

                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.CharmPerson:
                    if (!RemoveSpellCharmPerson(in evt))
                    {
                        return;
                    }

                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.CharmPersonOrAnimal:
                    if (!RemoveSpellCharmPersonOrAnimal(in evt))
                    {
                        return;
                    }

                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.ChillMetal:
                    GameSystems.ParticleSys.CreateAtObj("sp-Entropic Shield-END", evt.objHndCaller);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.ChillTouch:
                    if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Afraid) &&
                        evt.objHndCaller.IsNPC())
                    {
                        GameSystems.AI.StopFleeing(evt.objHndCaller);
                    }

                    // TODO: I doubt this is needed
                    spellPkt.EndPartSysForTarget(spellPkt.caster);
                    spellPkt.EndPartSysForCaster();
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.ClairaudienceClairvoyance:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.MapFogging.Enable();
                    GameSystems.ParticleSys.CreateAtObj("sp-Clairaudience-Clairvoyance-END", evt.objHndCaller);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);

                    // TODO: Why is this done here and not in a ConditionRemove subdispatcher???
                    evt.EndPartSysInArg(2);

                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.Cloudkill:
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);

                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.ColorSpray:
                    if (!RemoveColorSpray(in evt, data1))
                    {
                        return;
                    }

                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.Command:
                    if (evt.GetConditionArg3() == 3)
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
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.Consecrate:
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);

                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.ControlPlants:
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);

                    spellPkt.EndPartSysForSpellObjects();

                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.DeathKnell:
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.Desecrate:
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();

                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.DispelAir:
                case WellKnownSpells.DispelEarth:
                case WellKnownSpells.DispelFire:
                case WellKnownSpells.DispelWater:
                case WellKnownSpells.DispelChaos:
                case WellKnownSpells.DispelEvil:
                case WellKnownSpells.DispelGood:
                case WellKnownSpells.DispelLaw:
                    GameSystems.ParticleSys.CreateAtObj("sp-Dispel Law-END", evt.objHndCaller);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.DominateAnimal:
                case WellKnownSpells.DominatePerson:
                    GameSystems.Critter.RemoveFollower(evt.objHndCaller, true);
                    GameUiBridge.UpdatePartyUi();
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.Emotion:
                    if (!RemoveEmotion(in evt))
                    {
                        return;
                    }

                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    spellPkt.EndPartSysForCaster();

                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                            0);
                    }

                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.EndureElements:
                    switch (evt.GetConditionArg3())
                    {
                        case 1:
                            GameSystems.ParticleSys.CreateAtObj("sp-Endure Elements-acid-END", evt.objHndCaller);
                            GameSystems.SoundGame.PositionalSound(8985, 1, evt.objHndCaller);
                            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                            goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                        case 3:
                            GameSystems.ParticleSys.CreateAtObj("sp-Endure Elements-cold-END", evt.objHndCaller);
                            GameSystems.SoundGame.PositionalSound(8987, 1, evt.objHndCaller);
                            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                            goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                        case 6:
                            GameSystems.ParticleSys.CreateAtObj("sp-Endure Elements-water-END", evt.objHndCaller);
                            GameSystems.SoundGame.PositionalSound(8993, 1, evt.objHndCaller);
                            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                            goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                        case 9:
                            GameSystems.ParticleSys.CreateAtObj("sp-Endure Elements-fire-END", evt.objHndCaller);
                            GameSystems.SoundGame.PositionalSound(8989, 1, evt.objHndCaller);
                            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                            goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                        case 16:
                            GameSystems.ParticleSys.CreateAtObj("sp-Endure Elements-Sonic-END", evt.objHndCaller);
                            GameSystems.SoundGame.PositionalSound(8991, 1, evt.objHndCaller);
                            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                            goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                        default:
                            return;
                    }

                    return;
                case WellKnownSpells.Enlarge:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId);

                    ResetModelScaleAndSpeed(evt.objHndCaller, 1.8f);

                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.EntropicShield:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.ParticleSys.CreateAtObj("sp-Entropic Shield-END", evt.objHndCaller);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.Entangle:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);

                    spellPkt.EndPartSysForSpellObjects();

                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();

                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.FogCloud:
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.GaseousForm:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.ParticleSys.CreateAtObj("sp-Shield-END", evt.objHndCaller);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.GhoulTouch:
                    if (data1 == 108 || data2 == 108)
                    {
                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                            spellPkt.spellId, 0);
                    }
                    else
                    {
                        GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                            0);
                    }

                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);

                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.Grease:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();

                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.FalseLife:
                case WellKnownSpells.GreaterHeroism:
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.GreaterMagicFang:
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    EndGreaterMagicFang(evt.objHndCaller);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.GreaterMagicWeapon:
                    evt.objHndCaller.RemoveConditionFromItem(ItemEffects.WeaponEnhancementBonus);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.HaltUndead:
                    if (!RemoveHaltUndead(in evt))
                    {
                        return;
                    }

                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.HeatMetal:
                    GameSystems.ParticleSys.CreateAtObj("sp-Reduce Person-END", evt.objHndCaller);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
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
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.ImprovedInvisibility:
                case WellKnownSpells.DustOfDisappearance:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.IceStorm:
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.InvisibilityPurge:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    GameSystems.ParticleSys.CreateAtObj("sp-Invisibility Purge-END", evt.objHndCaller);
                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.InvisibilitySphere:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();

                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.InvisibilityToAnimals:
                    if (!RemoveInvisibility(in evt))
                    {
                        return;
                    }

                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.InvisibilityToUndead:
                    if (!RemoveInvisibility(in evt))
                    {
                        return;
                    }

                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);


                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.Blink:
                case WellKnownSpells.Invisibility:
                    if (!RemoveInvisibility(in evt))
                    {
                        return;
                    }

                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.PotionOfProtectionFromOutsiders:
                case WellKnownSpells.PotionOfProtectionFromElementals:
                case WellKnownSpells.PotionOfProtectionFromEarth:
                case WellKnownSpells.PotionOfProtectionFromMagic:
                case WellKnownSpells.PotionOfProtectionFromUndead:
                case WellKnownSpells.SummonBalor:
                case WellKnownSpells.SummonGlabrezu:
                case WellKnownSpells.SummonHezrou:
                case WellKnownSpells.SummonVrock:
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.KeenEdge:
                    evt.objHndCaller.RemoveConditionFromItem(ItemEffects.WeaponKeen);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.MageArmor:
                case WellKnownSpells.Glibness:
                case WellKnownSpells.Longstrider:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.MagicFang:
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId);
                    EndMagicFang(evt.objHndCaller);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.MagicVestment:
                    evt.objHndCaller.RemoveConditionFromItem(ItemEffects.ArmorEnhancementBonus);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.MagicWeapon:
                    evt.objHndCaller.RemoveConditionFromItem(ItemEffects.WeaponEnhancementBonus);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.MagicCircleAgainstChaos:
                case WellKnownSpells.MagicCircleAgainstEvil:
                case WellKnownSpells.MagicCircleAgainstGood:
                case WellKnownSpells.MagicCircleAgainstLaw:
                    EndMagicCircle(evt.objHndCaller, spellPkt.spellEnum);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.MeldIntoStone:
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    spellPkt.EndPartSysForCaster();
                    GameSystems.ParticleSys.CreateAtObj("sp-Meld Into Stone-END", spellPkt.caster);
                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.LesserGlobeOfInvulnerability:
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();

                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.MindFog:
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.MirrorImage:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    RemoveMirrorImage(in evt);
                    GameSystems.Spell.EndSpell(spellId);
                    return;
                case WellKnownSpells.MordenkainensFaithfulHound:
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId);
                    MordenkainensFaithfulHoundEnder(in evt);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.ObscuringMist:
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.OtilukesResilientSphere:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    GameSystems.ParticleSys.CreateAtObj("sp-Otilukes Resilient Sphere-END", evt.objHndCaller);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.ProtectionFromArrows:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    GameSystems.ParticleSys.CreateAtObj("sp-Protection from Arrows-END", evt.objHndCaller);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.ProtectionFromChaos:
                case WellKnownSpells.ProtectionFromEvil:
                case WellKnownSpells.ProtectionFromGood:
                case WellKnownSpells.ProtectionFromLaw:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.ProtectionFromElements:
                    switch (evt.GetConditionArg3())
                    {
                        case 1:
                            GameSystems.ParticleSys.CreateAtObj("sp-Protection from Elements-acid-END",
                                evt.objHndCaller);
                            GameSystems.SoundGame.PositionalSound(13385, 1, evt.objHndCaller);
                            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                            goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                        case 3:
                            GameSystems.ParticleSys.CreateAtObj("sp-Protection from Elements-cold-END",
                                evt.objHndCaller);
                            GameSystems.SoundGame.PositionalSound(13387, 1, evt.objHndCaller);
                            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                            goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                        case 6:
                            GameSystems.ParticleSys.CreateAtObj("sp-Protection from Elements-water-END",
                                evt.objHndCaller);
                            GameSystems.SoundGame.PositionalSound(13393, 1, evt.objHndCaller);
                            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                            goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                        case 9:
                            GameSystems.ParticleSys.CreateAtObj("sp-Protection from Elements-fire-END",
                                evt.objHndCaller);
                            GameSystems.SoundGame.PositionalSound(13389, 1, evt.objHndCaller);
                            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                            goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                        case 16:
                            GameSystems.ParticleSys.CreateAtObj("sp-Protection from Elements-Sonic-END",
                                evt.objHndCaller);
                            GameSystems.SoundGame.PositionalSound(13391, 1, evt.objHndCaller);
                            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                            goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
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
                    spellId = spellPkt.spellId;
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.Rage:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    if (!ShouldEndRage(in evt))
                    {
                        return;
                    }

                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);

                    spellPkt.EndPartSysForCaster();
                    GameSystems.ParticleSys.CreateAtObj("sp-Calm Emotions-END", spellPkt.caster);

                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.RayOfEnfeeblement:
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    GameSystems.ParticleSys.CreateAtObj("sp-Ray of Enfeeblement-END", evt.objHndCaller);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.ReduceAnimal:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    ResetModelScaleAndSpeed(evt.objHndCaller, 1 / 1.8f);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.Reduce:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    ResetModelScaleAndSpeed(evt.objHndCaller, 1 / 1.8f);
                    GameSystems.ParticleSys.CreateAtObj("sp-Reduce Person-END", evt.objHndCaller);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.RepelVermin:
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();

                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.ResistElements:
                    switch (evt.GetConditionArg3())
                    {
                        case 1:
                            GameSystems.ParticleSys.CreateAtObj("sp-Resist Elements-acid-END", evt.objHndCaller);
                            GameSystems.SoundGame.PositionalSound(14005, 1, evt.objHndCaller);
                            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                                0);
                            goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                        case 3:
                            GameSystems.ParticleSys.CreateAtObj("sp-Resist Elements-cold-END", evt.objHndCaller);
                            GameSystems.SoundGame.PositionalSound(14007, 1, evt.objHndCaller);
                            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                                0);
                            goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                        case 6:
                            GameSystems.ParticleSys.CreateAtObj("sp-Resist Elements-water-END", evt.objHndCaller);
                            GameSystems.SoundGame.PositionalSound(14013, 1, evt.objHndCaller);
                            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                                0);
                            goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                        case 9:
                            GameSystems.ParticleSys.CreateAtObj("sp-Resist Elements-fire-END", evt.objHndCaller);
                            GameSystems.SoundGame.PositionalSound(14009, 1, evt.objHndCaller);
                            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                                0);
                            goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                        case 16:
                            GameSystems.ParticleSys.CreateAtObj("sp-Resist Elements-Sonic-END", evt.objHndCaller);
                            GameSystems.SoundGame.PositionalSound(14011, 1, evt.objHndCaller);
                            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                                0);
                            goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
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
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.RighteousMight:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);

                    ResetModelScaleAndSpeed(evt.objHndCaller, 1 / 1.8f);

                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.RingOfAnimalSummoningDog:
                    EndSummon(evt.objHndCaller);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.Sanctuary:
                    if (!RemoveSpellSanctuary(in evt))
                    {
                        return;
                    }

                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    GameSystems.D20.ObjectRegistry.SendSignalAll(D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.Shield:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.ParticleSys.CreateAtObj("sp-Shield-END", evt.objHndCaller);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.Shillelagh:
                    GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);

                    var item = spellPkt.Targets[0].Object;
                    if (!spellPkt.RemoveTarget(spellPkt.Targets[0].Object))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    GameSystems.Item.UnequipItem(item);
                    GameSystems.Item.Remove(item);

                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.Silence:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();

                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.SleetStorm:
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.Sleep:
                case WellKnownSpells.DeepSlumber:
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20006, TextFloaterColor.White);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);

                    if (!spellPkt.RemoveTarget(spellPkt.Targets[0].Object))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                    return;
                case WellKnownSpells.SoftenEarthAndStone:
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    spellPkt.EndPartSysForSpellObjects();

                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();

                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.SolidFog:
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();

                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.SpikeGrowth:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    spellPkt.EndPartSysForSpellObjects();

                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.SpikeStones:
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    spellPkt.EndPartSysForSpellObjects();

                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.SpiritualWeapon:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    EndSpiritualWeapon(evt.objHndCaller);
                    // TODO: Is this sensible? Shouldn't it fade out???
                    GameSystems.MapObject.SetFlags(evt.objHndCaller, ObjectFlag.OFF);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.StinkingCloud:
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    spellPkt.EndPartSysForSpellObjects();

                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();

                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.DispelMagic:
                case WellKnownSpells.RemoveParalysis:
                case WellKnownSpells.Stoneskin:
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.SpellMonsterFrogTongue:
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    if (evt.GetConditionArg2() == 1 || evt.objHndCaller == spellPkt.caster)
                    {
                        FrogGrappleEnding(spellPkt, evt.objHndCaller);
                        spellPkt.ClearTargets();

                        GameSystems.Spell.EndSpell(spellId);
                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                            spellPkt.spellId, 0);
                    }
                    else if (evt.GetConditionArg2() == 2)
                    {
                        GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_Grapple_Removed,
                            spellPkt.spellId);
                        foreach (var target in spellPkt.Targets)
                        {
                            GameSystems.D20.D20SendSignal(target.Object,
                                D20DispatcherKey.SIG_Spell_Grapple_Removed,
                                spellPkt.spellId, 0);
                        }
                    }

                    return;
                case WellKnownSpells.SpellMonsterVrockScreech:
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.SpellMonsterVrockSpores:
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.Suggestion:
                    if (!RemoveSpellSuggestion(in evt))
                    {
                        return;
                    }

                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    GameSystems.Critter.RemoveFollower(evt.objHndCaller, true);
                    GameUiBridge.UpdatePartyUi();
                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
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
                    EndSummon(evt.objHndCaller);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.SummonSwarm:
                    if (!ShouldRemoveSummonSwarm(in evt))
                    {
                        return;
                    }

                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    if (evt.dispKey == D20DispatcherKey.SIG_Concentration_Broken)
                    {
                        evt.SetConditionArg2(2);
                        evt.SetConditionArg3(1);
                        spellPkt.duration = 2;
                        spellPkt.durationRemaining = 2;
                        GameSystems.Spell.UpdateSpellPacket(spellPkt);
                        GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                        return;
                    }

                    spellPkt.EndPartSysForCaster();

                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId);
                        GameSystems.D20.Combat.Kill(target.Object, null);
                    }

                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.Blur:
                case WellKnownSpells.ProduceFlame:
                case WellKnownSpells.SeeInvisibility:
                case WellKnownSpells.TreeShape:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.VampiricTouch:
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.Virtue:
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    goto Play_OnEndSPellCast__Remove_Caller_From_Targets;
                case WellKnownSpells.Web:
                    GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);

                    spellPkt.EndPartSysForSpellObjects();

                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();

                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                case WellKnownSpells.WindWall:
                    GameSystems.D20.D20SendSignal(spellPkt.aoeObj, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    evt.EndPartSysInArg(3);

                    foreach (var target in spellPkt.Targets)
                    {
                        GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    }

                    spellPkt.ClearTargets();
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                        spellPkt.spellId, 0);
                    GameSystems.Spell.EndSpell(spellId);
                    SpellEffects.Spell_remove_mod(in evt, 0);
                    return;
                default:
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    Logger.Warn("Removal of spell '{0}' is not handled!", spellName);
                    Play_OnEndSPellCast__Remove_Caller_From_Targets:
                    GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.EndSpellCast);
                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _remove_spell(): cannot END spell");
                        return;
                    }

                    GameSystems.Spell.EndSpell(spellId);
                    return;
            }
        }

        [TempleDllLocation(0x100d0a40)]
        private static bool RemoveAnimalFriendship(in DispatcherCallbackArgs evt)
        {
            var objHnd = evt.objHndCaller;
            var spellId = evt.GetConditionArg1();

            void EndNormally()
            {
                GameSystems.D20.D20SendSignal(objHnd, D20DispatcherKey.SIG_Spell_End, spellId);
                GameSystems.Critter.RemoveFollower(objHnd, true);
                GameUiBridge.UpdatePartyUi();
            }

            if (evt.dispIO == null)
            {
                if (evt.dispType == DispatcherType.BeginRound
                    || evt.dispKey == D20DispatcherKey.SIG_Dismiss_Spells
                    || evt.dispType == DispatcherType.ConditionAddPre)
                {
                    EndNormally();
                    return true;
                }

                return false;
            }

            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out _))
            {
                Logger.Info("d20_mods_spells.c / _remove_animal_friendship(): error, unable to retrieve spell_packet");
                return false;
            }

            if (evt.dispKey == D20DispatcherKey.SIG_Killed)
            {
                EndNormally();
                return true;
            }

            if (evt.dispType == DispatcherType.D20Signal)
            {
                var dispIo = evt.GetDispIoD20Signal();

                if (!(dispIo.obj is D20Action action))
                {
                    return true;
                }

                if (action.d20ActType == D20ActionType.CAST_SPELL)
                {
                    if (objHnd.HasCondition(SpellEffects.SpellAnimalFriendship) && action.spellId != spellId)
                    {
                        EndNormally();
                        return true;
                    }
                }
                else if (GameSystems.D20.Actions.IsOffensive(action.d20ActType, action.d20ATarget)
                         && GameSystems.Critter.IsFriendly(action.d20APerformer, action.d20ATarget)
                         && objHnd.HasCondition(SpellEffects.SpellAnimalFriendship))
                {
                    EndNormally();
                    return true;
                }
            }
            else if (evt.dispType == DispatcherType.DispelCheck)
            {
                GameSystems.D20.D20SendSignal(objHnd, D20DispatcherKey.SIG_Spell_End, 0, 0);
                return true;
            }

            return false;
        }

        [TempleDllLocation(0x100d0ca0)]
        private static bool RemoveAnimalTrance(in DispatcherCallbackArgs evt)
        {
            if (evt.dispIO == null)
            {
                if (evt.dispType == DispatcherType.BeginRound || evt.dispKey == D20DispatcherKey.SIG_Dismiss_Spells ||
                    evt.dispType == DispatcherType.ConditionAddPre)
                {
                    return true;
                }

                return false;
            }

            if (evt.dispKey == D20DispatcherKey.SIG_Killed)
            {
                return true;
            }

            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out _))
            {
                Logger.Info("d20_mods_spells.c / _remove_animal_trance(): error, unable to retrieve spell_packet");
                return false;
            }

            if (evt.dispType == DispatcherType.D20Signal)
            {
                var dispIo = evt.GetDispIoD20Signal();
                if (evt.dispKey == D20DispatcherKey.SIG_Concentration_Broken)
                {
                    return true;
                }

                if (!(dispIo.obj is D20Action action))
                {
                    return true;
                }

                if (action.d20ActType == D20ActionType.CAST_SPELL)
                {
                    if (action.spellId != spellId)
                    {
                        return true;
                    }
                }
                else if (GameSystems.D20.Actions.IsOffensive(action.d20ActType, action.d20ATarget)
                         && GameSystems.Critter.IsFriendly(action.d20APerformer, action.d20ATarget))
                {
                    return true;
                }

                return false;
            }
            else if (evt.dispType == DispatcherType.DispelCheck)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [TempleDllLocation(0x100d0db0)]
        private static bool RemoveSpellCalmAnimals(in DispatcherCallbackArgs evt)
        {
            var spellId = evt.GetConditionArg1();

            if (evt.dispIO == null)
            {
                if (evt.dispType == DispatcherType.BeginRound || evt.dispKey == D20DispatcherKey.SIG_Dismiss_Spells ||
                    evt.dispType == DispatcherType.ConditionAddPre)
                {
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId);
                    return true;
                }

                return false;
            }

            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out _))
            {
                Logger.Info("d20_mods_spells.c / _remove_spell_calm_animals(): error, unable to retrieve spell_packet");
                return false;
            }

            if (evt.dispKey == D20DispatcherKey.SIG_Killed)
            {
                GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                return true;
            }

            if (evt.dispType == DispatcherType.D20Signal)
            {
                var dispIo = evt.GetDispIoD20Signal();

                if (!(dispIo.obj is D20Action action))
                {
                    return true;
                }

                if (action.d20ActType == D20ActionType.CAST_SPELL)
                {
                    if (evt.objHndCaller.HasCondition(SpellEffects.SpellCalmAnimals) && action.spellId != spellId)
                    {
                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId);
                        return true;
                    }
                }
                else if (GameSystems.D20.Actions.IsOffensive(action.d20ActType, action.d20ATarget)
                         && GameSystems.Critter.IsFriendly(action.d20APerformer, action.d20ATarget)
                         && evt.objHndCaller.HasCondition(SpellEffects.SpellCalmAnimals))
                {
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    return true;
                }
            }
            else if (evt.dispType == DispatcherType.DispelCheck)
            {
                GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, 0, 0);
                return true;
            }
            else if (evt.dispType == DispatcherType.TurnBasedStatusInit)
            {
                // NOTE: This previously checked for the type of DispIO, which seems wrong...
                GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, 0, 0);
                return true;
            }

            return false;
        }

        [TempleDllLocation(0x100d1030)]
        private static bool RemoveSpellCalmEmotions(in DispatcherCallbackArgs evt)
        {
            if (evt.dispIO == null)
            {
                if (evt.dispType == DispatcherType.BeginRound || evt.dispKey == D20DispatcherKey.SIG_Dismiss_Spells ||
                    evt.dispType == DispatcherType.ConditionAddPre)
                {
                    return true;
                }

                return false;
            }

            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                Logger.Info(
                    "d20_mods_spells.c / _remove_spell_calm_emotions(): error, unable to retrieve spell_packet");
                return false;
            }

            if (evt.dispKey == D20DispatcherKey.SIG_Killed)
            {
                GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId, 0);
                return true;
            }

            if (evt.dispIO is DispIoD20Signal dispIo)
            {
                if (evt.dispKey == D20DispatcherKey.SIG_Concentration_Broken)
                {
                    return true;
                }

                if (!(dispIo.obj is D20Action action))
                {
                    return true;
                }

                if (action.d20ActType == D20ActionType.CAST_SPELL)
                {
                    if (action.spellId != spellId)
                    {
                        return true;
                    }
                }
                else if (GameSystems.D20.Actions.IsOffensive(action.d20ActType, action.d20ATarget)
                         && GameSystems.Critter.IsFriendly(action.d20APerformer, action.d20ATarget))
                {
                    return true;
                }
            }
            else if (evt.dispIO is DispIoDispelCheck)
            {
                return true;
            }

            return false;
        }

        private static bool RemoveCharmSpell(in DispatcherCallbackArgs evt, ConditionSpec charmCondition)
        {
            var critter = evt.objHndCaller;
            var spellId = evt.GetConditionArg1();

            void Cleanup()
            {
                GameSystems.D20.D20SendSignal(critter, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                GameSystems.Critter.RemoveFollower(critter, true);
                GameUiBridge.UpdatePartyUi();
            }

            if (evt.dispIO == null)
            {
                if (evt.dispType == DispatcherType.BeginRound || evt.dispKey == D20DispatcherKey.SIG_Dismiss_Spells ||
                    evt.dispType == DispatcherType.ConditionAddPre)
                {
                    Cleanup();
                    return true;
                }

                return false;
            }

            if (evt.dispKey == D20DispatcherKey.SIG_Killed)
            {
                Cleanup();
                return true;
            }

            if (!(evt.dispIO is DispIoD20Signal dispIo))
            {
                if (evt.dispIO is DispIoDispelCheck || evt.dispIO is DispIOTurnBasedStatus)
                {
                    Cleanup();
                    return true;
                }

                return false;
            }

            if (evt.dispKey == D20DispatcherKey.SIG_Critter_Killed)
            {
                Cleanup();
                return true;
            }

            if (!(dispIo.obj is D20Action action))
            {
                return true;
            }

            if (action.d20ActType == D20ActionType.CAST_SPELL)
            {
                if (critter.HasCondition(charmCondition) && action.spellId != spellId)
                {
                    var spellEnum = action.d20SpellData.SpellEnum;
                    var charmedBy =
                        GameSystems.D20.D20QueryReturnObject(critter, D20DispatcherKey.QUE_Critter_Is_Charmed);
                    if (charmedBy == action.d20APerformer ||
                        GameSystems.Critter.IsFriendly(charmedBy, action.d20APerformer))
                    {
                        if (GameSystems.Spell.IsSpellHarmful(spellEnum, charmedBy, action.d20APerformer))
                        {
                            Cleanup();
                            return true;
                        }
                    }
                }
            }
            else if (GameSystems.D20.Actions.IsOffensive(action.d20ActType, action.d20ATarget))
            {
                if (GameSystems.Critter.IsFriendly(action.d20APerformer, action.d20ATarget))
                {
                    if (critter.HasCondition(charmCondition))
                    {
                        var charmedBy =
                            GameSystems.D20.D20QueryReturnObject(critter, D20DispatcherKey.QUE_Critter_Is_Charmed);
                        if (charmedBy == action.d20APerformer ||
                            GameSystems.Critter.IsFriendly(charmedBy, action.d20APerformer))
                        {
                            Cleanup();
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        [TempleDllLocation(0x100d1170)]
        private static bool RemoveSpellCharmMonster(in DispatcherCallbackArgs evt)
        {
            return RemoveCharmSpell(in evt, SpellCharmMonster);
        }

        [TempleDllLocation(0x100d13f0)]
        private static bool RemoveSpellCharmPerson(in DispatcherCallbackArgs evt)
        {
            return RemoveCharmSpell(in evt, SpellCharmPerson);
        }

        [TempleDllLocation(0x100d1670)]
        private static bool RemoveSpellCharmPersonOrAnimal(in DispatcherCallbackArgs evt)
        {
            return RemoveCharmSpell(in evt, SpellCharmPersonorAnimal);
        }

        [TempleDllLocation(0x100d18f0)]
        private static bool RemoveColorSpray(in DispatcherCallbackArgs evt, int spellType)
        {
            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                Logger.Info("d20_mods_spells.c / _remove_spell_color_spray(): unable to retrieve spell_packet!");
                return false;
            }

            // This models the order of events for color spray (first unconscious, then blinded, and then just stunned)
            ConditionSpec condToAdd;
            if (spellType == 33 || evt.GetConditionName() == SpellColorSprayBlind.condName)
            {
                condToAdd = SpellColorSprayStun;
            }
            else if (spellType == 35 || evt.GetConditionName() == SpellColorSprayUnconscious.condName)
            {
                condToAdd = SpellColorSprayBlind;
            }
            else
            {
                return true;
            }

            if (!evt.objHndCaller.AddCondition(condToAdd, spellId, spellPkt.duration, 0))
            {
                Logger.Info("d20_mods_spells.c / _remove_spell_color_spray(): unable to add condition");
            }

            return false;
        }

        [TempleDllLocation(0x100d19a0)]
        private static bool RemoveEmotion(in DispatcherCallbackArgs evt)
        {
            if (evt.dispIO == null)
            {
                if (evt.dispType == DispatcherType.BeginRound || evt.dispKey == D20DispatcherKey.SIG_Dismiss_Spells ||
                    evt.dispType == DispatcherType.ConditionAddPre)
                {
                    return true;
                }

                return false;
            }

            if (evt.dispKey != D20DispatcherKey.SIG_Killed)
            {
                if (evt.dispIO is DispIoD20Signal dispIo)
                {
                    if (evt.dispKey != D20DispatcherKey.SIG_Concentration_Broken)
                    {
                        return false;
                    }
                }
                else if (!(evt.dispIO is DispIoDispelCheck))
                {
                    return false;
                }
            }

            return true;
        }

        [TempleDllLocation(0x100d1ac0)]
        private static bool RemoveHaltUndead(in DispatcherCallbackArgs evt)
        {
            if (evt.dispIO == null)
            {
                if (evt.dispType == DispatcherType.BeginRound || evt.dispKey == D20DispatcherKey.SIG_Dismiss_Spells ||
                    evt.dispType == DispatcherType.ConditionAddPre)
                {
                    return true;
                }

                return false;
            }

            if (evt.dispIO is DispIoD20Signal)
            {
                return false;
            }

            if (evt.dispIO is DispIoDispelCheck || evt.dispIO is DispIoDamage)
            {
                return true;
            }

            return evt.dispIO is DispIOTurnBasedStatus;
        }

        [TempleDllLocation(0x100d1b00)]
        private static bool RemoveInvisibility(in DispatcherCallbackArgs evt)
        {
            var spellId = evt.GetConditionArg1();

            if (evt.dispIO == null)
            {
                if (evt.dispType == DispatcherType.BeginRound || evt.dispKey == D20DispatcherKey.SIG_Dismiss_Spells ||
                    evt.dispType == DispatcherType.ConditionAddPre)
                {
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    return true;
                }

                return false;
            }

            if (evt.dispKey == D20DispatcherKey.SIG_Killed
                || evt.dispKey == D20DispatcherKey.SIG_Dismiss_Spells
                || evt.dispType == DispatcherType.BeginRound
                || evt.dispType == DispatcherType.ConditionAddPre)
            {
                return true;
            }

            if (!(evt.dispIO is DispIoD20Signal dispIo))
            {
                if (evt.dispType == DispatcherType.DispelCheck)
                {
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    return true;
                }

                return false;
            }

            var seq = (ActionSequence) dispIo.obj;
            if (seq == null)
            {
                return true;
            }

            var objIsInvisible = evt.objHndCaller.HasCondition(SpellInvisibility);
            if (!objIsInvisible)
            {
                return false;
            }

            for (var i = 0; i < seq.d20ActArrayNum; i++)
            {
                var action = seq.d20ActArray[i];
                if (action.d20ActType != D20ActionType.CAST_SPELL)
                {
                    if (GameSystems.D20.Actions.IsOffensive(action.d20ActType, action.d20ATarget))
                    {
                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId);
                        return true;
                    }
                }
                else if (action.spellId != spellId)
                {
                    if (!GameSystems.Spell.TryGetActiveSpell(action.spellId, out var spellPkt))
                    {
                        Logger.Warn("RemoveInvisibility: Error, unable to retrieve spell.");
                        return false;
                    }

                    foreach (var target in spellPkt.Targets)
                    {
                        if (GameSystems.Spell.IsSpellHarmful(spellPkt.spellEnum, spellPkt.caster, target.Object))
                        {
                            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        [TempleDllLocation(0x100d1a50)]
        private static void EndGreaterMagicFang(GameObjectBody critter)
        {
            GameSystems.ParticleSys.CreateAtObj("sp-Greater Magic Fang-END", critter);
        }

        [TempleDllLocation(0x100d1d60)]
        private static void EndMagicFang(GameObjectBody critter)
        {
            GameSystems.ParticleSys.CreateAtObj("sp-Magic Fang-END", critter);
        }

        [TempleDllLocation(0x100d1dd0)]
        private static void EndMagicCircle(GameObjectBody critter, int spellEnum)
        {
            switch (spellEnum)
            {
                case WellKnownSpells.MagicCircleAgainstChaos:
                    GameSystems.ParticleSys.CreateAtObj("sp-Magic Circle against Chaos-END", critter);
                    break;
                case WellKnownSpells.MagicCircleAgainstEvil:
                    GameSystems.ParticleSys.CreateAtObj("sp-Magic Circle against Evil-END", critter);
                    break;
                case WellKnownSpells.MagicCircleAgainstGood:
                    GameSystems.ParticleSys.CreateAtObj("sp-Magic Circle against Good-END", critter);
                    break;
                case WellKnownSpells.MagicCircleAgainstLaw:
                    GameSystems.ParticleSys.CreateAtObj("sp-Magic Circle against Law-END", critter);
                    break;
            }
        }

        [TempleDllLocation(0x100d1e80)]
        private static void RemoveMirrorImage(in DispatcherCallbackArgs evt)
        {
            var spellId = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                spellPkt.ClearTargets();
                GameSystems.Spell.UpdateSpellPacket(spellPkt);
                GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
            }
        }

        [TempleDllLocation(0x100d1f60)]
        private static void MordenkainensFaithfulHoundEnder(in DispatcherCallbackArgs evt)
        {
            var spellId = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(spellId, out _))
            {
                GameSystems.ParticleSys.CreateAtObj("sp-Mordenkainens Faithful Hound-END", evt.objHndCaller);
            }

            var condArg4 = evt.GetConditionArg4();
            GameSystems.ObjectEvent.Remove(condArg4);
            evt.SetConditionArg4(0);
        }

        [TempleDllLocation(0x100d2260)]
        private static bool RemoveSpellSanctuary(in DispatcherCallbackArgs evt)
        {
            if (evt.dispType == DispatcherType.BeginRound
                || evt.dispKey == D20DispatcherKey.SIG_Dismiss_Spells
                || evt.dispType == DispatcherType.ConditionAddPre
                || evt.dispKey == D20DispatcherKey.SIG_Killed)
            {
                return true;
            }

            int spellId = evt.GetConditionArg1();

            if (!(evt.dispIO is DispIoD20Signal dispIo))
            {
                if (evt.dispIO is DispIoDispelCheck)
                {
                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                    return true;
                }

                return false;
            }

            if (!(dispIo.obj is ActionSequence sequence))
            {
                return true;
            }

            foreach (var action in sequence.d20ActArray)
            {
                D20ActionType actionType = action.d20ActType;
                if (actionType != D20ActionType.CAST_SPELL)
                {
                    if (GameSystems.D20.Actions.IsOffensive(actionType, action.d20ATarget)
                        && evt.objHndCaller.HasCondition(SpellSanctuary))
                    {
                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                        return true;
                    }

                    continue;
                }

                if (evt.objHndCaller.HasCondition(SpellSanctuary) && action.spellId != spellId)
                {
                    if (!GameSystems.Spell.TryGetActiveSpell(action.spellId, out var spellPkt))
                    {
                        Logger.Info(
                            "d20_mods_spells.c / _remove_spell_sanctuary(): error, unable to retrieve spell_packet");
                        return false;
                    }

                    foreach (var target in spellPkt.Targets)
                    {
                        if (GameSystems.Spell.IsSpellHarmful(spellPkt.spellEnum, spellPkt.caster, target.Object))
                        {
                            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                                spellPkt.spellId, 0);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        [TempleDllLocation(0x100d2a10)]
        private static void EndSummon(GameObjectBody critter)
        {
            GameSystems.ParticleSys.CreateAtObj("Fizzle", critter);
            GameSystems.Critter.RemoveFollower(critter, true);
            GameUiBridge.UpdatePartyUi();
        }

        [TempleDllLocation(0x100d2990)]
        private static void EndSpiritualWeapon(GameObjectBody swCritter)
        {
            GameSystems.ParticleSys.CreateAtObj("Fizzle", swCritter);
            GameSystems.ObjFade.FadeTo(swCritter, 0, 2, 5, FadeOutResult.Destroy);
        }

        [TempleDllLocation(0x100d26f0)]
        private static void FrogGrappleEnding(SpellPacketBody spellPkt, GameObjectBody critter)
        {
            if (critter == spellPkt.caster)
            {
                GameSystems.D20.D20SendSignal(spellPkt.caster, D20DispatcherKey.SIG_Spell_Grapple_Removed,
                    spellPkt.spellId, 0);
                foreach (var target in spellPkt.Targets)
                {
                    GameSystems.D20.D20SendSignal(target.Object, D20DispatcherKey.SIG_Spell_Grapple_Removed,
                        spellPkt.spellId, 0);
                }
            }
            else if (spellPkt.Targets.Length > 0)
            {
                // Escaped!
                GameSystems.Spell.FloatSpellLine(critter, 21003, TextFloaterColor.White);
            }

            FrogGrappleController.PlayRetractTongue(spellPkt.caster);

            spellPkt.EndPartSysForTarget(critter);

            GameSystems.ObjFade.FadeTo(critter, 255, 0, 16, 0);
            GameSystems.D20.Actions.PerformOnAnimComplete(critter, -1);
            spellPkt.caster.SetAnimId(spellPkt.caster.GetIdleAnimId());
            critter.SetAnimId(critter.GetIdleAnimId());

            using var nearbyNpcs = ObjList.ListVicinity(spellPkt.caster, ObjectListFilter.OLC_NPC);
            foreach (var npc in nearbyNpcs)
            {
                if (!GameSystems.Critter.IsFriendly(npc, critter) && npc != spellPkt.caster)
                {
                    GameSystems.AI.RemoveFromAllyList(npc, critter);
                }
            }

            if (GameSystems.Critter.IsDeadNullDestroyed(spellPkt.caster) ||
                GameSystems.Critter.IsDeadOrUnconscious(spellPkt.caster))
            {
                FrogGrappleController.Reset(spellPkt.caster);
            }
        }

        [TempleDllLocation(0x100d2460)]
        [TemplePlusLocation("generalfixes.cpp:423")]
        private static bool RemoveSpellSuggestion(in DispatcherCallbackArgs evt)
        {
            var spellId = evt.GetConditionArg1();
            var critter = evt.objHndCaller;

            void Cleanup()
            {
                GameSystems.D20.D20SendSignal(critter, D20DispatcherKey.SIG_Spell_End, spellId, 0);
                GameSystems.Critter.RemoveFollower(critter, true);
                GameUiBridge.UpdatePartyUi();
            }

            if (evt.dispIO == null)
            {
                if (evt.dispType == DispatcherType.BeginRound || evt.dispType == DispatcherType.ConditionAddPre ||
                    evt.dispKey == D20DispatcherKey.SIG_Dismiss_Spells)
                {
                    Cleanup();
                    return true;
                }

                return false;
            }

            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out _))
            {
                Logger.Warn("RemoveSpellSuggestion: Unable to retrieve spell!");
                return false;
            }

            if (evt.dispKey == D20DispatcherKey.SIG_Killed || evt.dispKey == D20DispatcherKey.SIG_Critter_Killed)
            {
                Cleanup();
                return true;
            }

            if (!(evt.dispIO is DispIoD20Signal dispIo))
            {
                if (evt.dispIO is DispIoDispelCheck || evt.dispIO is DispIOTurnBasedStatus)
                {
                    Cleanup();
                }

                return false;
            }

            if (!(dispIo.obj is D20Action action))
                return true;

            if (action.d20ActType == D20ActionType.CAST_SPELL)
            {
                if (evt.objHndCaller.HasCondition(SpellSuggestion) && action.spellId != spellId)
                {
                    Cleanup();
                    return true;
                }
            }
            else if (GameSystems.D20.Actions.IsOffensive(action.d20ActType, action.d20ATarget))
            {
                if (GameSystems.Critter.IsFriendly(action.d20APerformer, action.d20ATarget))
                {
                    if (evt.objHndCaller.HasCondition(SpellSuggestion))
                    {
                        Cleanup();
                        return true;
                    }
                }
            }

            return false;
        }

        [TempleDllLocation(0x100d26a0)]
        private static bool ShouldRemoveSummonSwarm(in DispatcherCallbackArgs evt)
        {
            if (evt.dispIO != null)
            {
                if (evt.dispKey == D20DispatcherKey.SIG_Killed)
                {
                    return true;
                }

                if (evt.dispIO is DispIoD20Signal dispIo)
                {
                    if (evt.dispKey == D20DispatcherKey.SIG_Concentration_Broken)
                    {
                        return true;
                    }

                    return false;
                }

                if (evt.dispIO is DispIoDispelCheck)
                {
                    return true;
                }
            }
            else if (evt.dispType == DispatcherType.BeginRound || evt.dispKey == D20DispatcherKey.SIG_Dismiss_Spells ||
                     evt.dispType == DispatcherType.ConditionAddPre)
            {
                return true;
            }

            return false;
        }

        [TempleDllLocation(0x100d20c0)]
        private static bool ShouldEndRage(in DispatcherCallbackArgs evt)
        {
            if (evt.dispIO == null)
            {
                if (evt.dispType == DispatcherType.BeginRound
                    || evt.dispType == DispatcherType.ConditionAddPre
                    || evt.dispKey == D20DispatcherKey.SIG_Dismiss_Spells)
                {
                    return true;
                }

                return false;
            }

            if (evt.dispKey == D20DispatcherKey.SIG_Killed)
            {
                return true;
            }

            if (evt.dispIO is DispIoD20Signal dispIo)
            {
                if (evt.dispKey == D20DispatcherKey.SIG_Concentration_Broken)
                {
                    return true;
                }

                var sequence = dispIo.obj as ActionSequence;

                var spellId = evt.GetConditionArg1();
                if (sequence == null)
                {
                    return true;
                }

                foreach (var action in sequence.d20ActArray)
                {
                    if (action.d20ActType == D20ActionType.CAST_SPELL)
                    {
                        if (action.spellId != spellId)
                        {
                            if (!GameSystems.Spell.TryGetActiveSpell(action.spellId, out _))
                            {
                                return false;
                            }

                            return true;
                        }
                    }
                    else if (GameSystems.D20.Actions.IsOffensive(action.d20ActType, action.d20ATarget))
                    {
                        return true;
                    }
                }

                return false;
            }
            else if (evt.dispIO is DispIoDispelCheck)
            {
                return true;
            }

            return false;
        }

        private static void ResetModelScaleAndSpeed(GameObjectBody critter, float factorToRemove)
        {
            var scale = critter.GetInt32(obj_f.model_scale);
            critter.SetInt32(obj_f.model_scale, (int) (scale / factorToRemove));
            GameSystems.Critter.UpdateModelEquipment(critter);

            var speedRun = critter.GetFloat(obj_f.speed_run) * factorToRemove;
            critter.SetFloat(obj_f.speed_run, speedRun);

            var speedWalk = critter.GetFloat(obj_f.speed_walk) * factorToRemove;
            critter.SetFloat(obj_f.speed_walk, speedWalk);
        }
    }
}