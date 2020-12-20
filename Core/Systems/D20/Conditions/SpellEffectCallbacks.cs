using System;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.Anim;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Utils;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Ui.InGameSelect;

namespace OpenTemple.Core.Systems.D20.Conditions
{
    public static partial class SpellEffects
    {
        [TempleDllLocation(0x100c37d0)]
        private static void SpellPktTriggerAoeHitScript(int spellId)
        {
            if (GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                GameSystems.Script.Spells.SpellSoundPlay(spellPkt, SpellEvent.SpellStruck);
            }

            GameSystems.Script.Spells.SpellTrigger(spellId, SpellEvent.AreaOfEffectHit);
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ca8a0)]
        public static void ShowConcealedMessage(in DispatcherCallbackArgs evt)
        {
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20030, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100dc920)]
        public static void DispCritterKilledRemoveSpellAndMod(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            if (dispIo.obj ==
                GameSystems.D20.D20QueryReturnObject(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Charmed, 0, 0)
                || dispIo.obj ==
                GameSystems.D20.D20QueryReturnObject(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Held, 0, 0))
            {
                SpellEffects.Spell_remove_spell(in evt, 0, 0);
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ce010)]
        public static void SpHealOnConditionAdd(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var v1 = (Stat) 0;
            do
            {
                var dispIo = new DispIoAbilityLoss();
                dispIo.flags |= 0x1B; /*INLINED:v2=evt.subDispNode.condNode*/
                dispIo.statDamaged = (Stat) v1;
                dispIo.fieldC = 1;
                dispIo.spellId = evt.GetConditionArg1();
                dispIo.result = 0;
                var v3 = evt.objHndCaller.DispatchGetAbilityLoss(dispIo);
                var v4 = -v3;
                if (-v3 >= 0 && v3 != 0)
                {
                    var v5 = GameSystems.Stat.GetStatName(dispIo.statDamaged);
                    Logger.Info(
                        "d20_mods_spells.c / _begin_spell_restoration(): healed {0} points of temporary ({1}) damage",
                        v4, v5);
                    var v6 = GameSystems.Stat.GetStatName(dispIo.statDamaged);
                    var suffix = String.Format(": {0} [{1}]", v6, v4);
                    // Restoration
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20035, TextFloaterColor.White, suffix: suffix);
                }

                ++v1;
            } while ((int) v1 < 5);

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                var v8 = spellPkt.spellId;
                var v9 = new Dice(0, 0, 10 * spellPkt.casterLevel);
                GameSystems.Combat.SpellHeal(evt.objHndCaller, spellPkt.caster, v9, D20ActionType.CAST_SPELL, v8);
                GameSystems.Critter.HealSubdualSub_100B9030(evt.objHndCaller, 10 * spellPkt.casterLevel);
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100c71c0)]
        public static void sub_100C71C0(in DispatcherCallbackArgs evt, int data)
        {
            var condArg1 = evt.GetConditionArg1();
            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, condArg1, 0);
        }


        [DispTypes(DispatcherType.TurnBasedStatusInit)]
        [TempleDllLocation(0x100c6690)]
        public static void ChaosHammerTurnBasedStatusInit(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIOTurnBasedStatus();
            if (dispIo != null)
            {
                if (dispIo.tbStatus.hourglassState < HourglassState.STD)
                {
                    dispIo.tbStatus.hourglassState = HourglassState.EMPTY;
                    dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
                }
                else
                {
                    dispIo.tbStatus.hourglassState = HourglassState.STD;
                    dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cc820)]
        public static void BeginSpellColorSprayBlind(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20019, TextFloaterColor.Red);
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                int v8 = Dice.D4.Roll();
                spellPkt.duration = v8;
                spellPkt.durationRemaining = v8;
                evt.SetConditionArg2(v8);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
            else
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_color_spray_blind(): unable to get spell_packet");
            }
        }

        /// <summary>
        /// For a given spell condition, query the critter for the spell id the condition originated from.
        /// </summary>
        private static int GetSpellConditionSpellId(GameObjectBody critter, ConditionSpec condition)
        {
            var dispatcher = critter.GetDispatcher();
            var dispIo = new DispIoD20Query();
            dispIo.obj = condition;
            dispatcher?.Process(DispatcherType.D20Query, D20DispatcherKey.QUE_Critter_Has_Condition, dispIo);
            if (dispIo.return_val != 1)
            {
                return 0;
            }

            return (int) dispIo.resultData;
        }

        private static void AllowSanctuarySave(D20Action action, int sanctuarySpellId)
        {
            var saveSucceededSpellId = GetSpellConditionSpellId(action.d20APerformer, SpellSanctuarySaveSucceeded);
            var saveFailedSpellId = GetSpellConditionSpellId(action.d20APerformer, SpellSanctuarySaveFailed);

            if (saveSucceededSpellId == sanctuarySpellId || saveFailedSpellId == sanctuarySpellId)
            {
                return; // Already saved against this specific sanctuary spell
            }

            GameSystems.D20.D20SendSignal(action.d20ATarget, D20DispatcherKey.SIG_Spell_Sanctuary_Attempt_Save, action);
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c9c60)]
        public static void SanctuaryCanBeAffectedPerform(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var dispIo = evt.GetDispIoD20Query();
            var action = (D20Action) dispIo.obj;

            // TODO: What this actually checks: The performer should have SpellSanctuarySaveFailed with same spell-id as Sanctuary on target, which is stored in arg1 in both cases
            var sanctuarySpellId = GetSpellConditionSpellId(action.d20ATarget, SpellSanctuary);
            if (sanctuarySpellId == 0)
            {
                return; // How did we get here in the first place...?
            }

            AllowSanctuarySave(action, sanctuarySpellId);

            // TODO: I think this will fail badly if two critters have sanctuary up and the performer alternates between them!
            var failedSaveForSpellId = GetSpellConditionSpellId(action.d20APerformer, SpellSanctuarySaveFailed);
            if (failedSaveForSpellId == sanctuarySpellId)
            {
                dispIo.return_val = 0;
                dispIo.data1 = evt.GetConditionArg3();
                GameSystems.D20.Combat.FloatCombatLine(action.d20ATarget, 123);
            }
        }


        [DispTypes(DispatcherType.TurnBasedStatusInit)]
        [TempleDllLocation(0x100dc9b0)]
        public static void HoldXTurnBasedStatusInit(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIOTurnBasedStatus();
            if (evt.GetConditionArg3() == 1)
            {
                if (dispIo != null)
                {
                    if (dispIo.tbStatus.hourglassState > HourglassState.EMPTY)
                    {
                        dispIo.tbStatus.hourglassState = HourglassState.EMPTY;
                        dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
                        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                        SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                        SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                    }
                }
            }
            else if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement))
            {
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x22, evt.objHndCaller, null);
                GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 40);
            }
        }


        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100c7ad0)]
        public static void SkillModifier_FindTraps_Callback(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            SpellPacketBody spellPkt;

            var dispIo = evt.GetDispIoObjBonus();
            var v2 = dispIo;
            if (evt.dispKey == D20DispatcherKey.D20A_CLEAVE)
            {
                if ((dispIo.flags & SkillCheckFlags.SearchForTraps) != 0)
                {
                    var condArg1 = evt.GetConditionArg1();
                    if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
                    {
                        var casterLvl = DispatcherExtensions.Dispatch35CasterLevelModify(evt.objHndCaller, spellPkt) /
                                        2;
                        if (casterLvl > 10)
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
        public static void Condition__36_ghoul_touch_stench_sthg(in DispatcherCallbackArgs evt, int data)
        {
            SpellPacketBody spellPkt;

            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
                {
                    SpellPktTriggerAoeHitScript(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE
                        && dispIo.tgt != spellPkt.caster
                        && !evt.objHndCaller.HasCondition(SpellEffects.SpellDelayPoison))
                    {
                        if (GameSystems.D20.D20Query(dispIo.tgt, D20DispatcherKey.QUE_Critter_Is_Immune_Poison))
                        {
                            GameSystems.Spell.PlayFizzle(evt.objHndCaller);
                            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 32000, TextFloaterColor.White);
                        }
                        else
                        {
                            var v3 = GameSystems.D20.Combat.SavingThrowSpell(dispIo.tgt, spellPkt.caster, spellPkt.dc,
                                SavingThrowType.Fortitude, 0, spellPkt.spellId);
                            if (v3)
                            {
                                GameSystems.Spell.FloatSpellLine(dispIo.tgt, 30001, TextFloaterColor.White);
                            }
                            else
                            {
                                GameSystems.Spell.FloatSpellLine(dispIo.tgt, 30002, TextFloaterColor.White);
                                var v8 = GameSystems.ParticleSys.CreateAtObj("Fizzle", dispIo.tgt);
                                spellPkt.AddTarget(dispIo.tgt, v8);
                                dispIo.tgt.AddCondition("sp-Ghoul Touch Stench Hit", spellPkt.spellId,
                                    spellPkt.durationRemaining, dispIo.evtId);
                            }
                        }
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100c9020)]
        public static void sub_100C9020(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            if (evt.dispKey == D20DispatcherKey.SAVE_WILL)
            {
                dispIo.bonlist.AddBonus(-data1, 34, data2);
            }
        }


        [DispTypes(DispatcherType.ConditionRemove)]
        [TempleDllLocation(0x100d2c80)]
        public static void sub_100D2C80(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            GameSystems.Spell.EndSpell(condArg1, true);
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100de560)]
        public static void ExpireSpell(in DispatcherCallbackArgs evt, int data)
        {
            int i;
            SpellPacketBody spellPkt;

            evt.GetDispIoD20Signal();
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                if (data == 1)
                {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                    SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                    SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                }
                else
                {
                    for (i = 0; i < spellPkt.Targets.Length; ++i)
                    {
                        // TODO: Is this a bug, should this be on the target instead???
                        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                        SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                        SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                    }
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cde90)]
        public static void sub_100CDE90(in DispatcherCallbackArgs evt)
        {
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20016, TextFloaterColor.White);
        }


        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100c6080)]
        public static void sub_100C6080(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            dispIo.bonlist.AddBonus(data1, 9, data2);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cc2c0)]
        public static void BlessOnAdd(in DispatcherCallbackArgs args)
        {
            GameSystems.Spell.FloatSpellLine(args.objHndCaller, 20008, TextFloaterColor.White);
        }


        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100c5c30)]
        public static void StatLevel_callback_SpellModifier(in DispatcherCallbackArgs evt, Stat data1, int data2)
        {
            var dispIo = evt.GetDispIoBonusList();
            var condArg3 = evt.GetConditionArg3();
            var queryAttribute = evt.GetAttributeFromDispatcherKey();
            if (queryAttribute != data1)
            {
                return;
            }

            var v3 = data2;
            if (v3 == 292 || v3 == 549 || v3 == 550)
            {
                if ((dispIo.flags & 2) != 0)
                {
                    return;
                }

                dispIo.bonlist.AddBonus(condArg3, 12, v3);
                return;
            }

            if (v3 > 292)
            {
                dispIo.bonlist.AddBonus(condArg3, 12, v3);
                return;
            }

            // 198 == Ray of enfeeblement...
            if (v3 == 198)
            {
                var v5 = dispIo.bonlist.bonusEntries[0].bonValue;
                if (v5 <= condArg3)
                {
                    condArg3 = v5 - 1;
                }

                dispIo.bonlist.AddBonus(-condArg3, 0, 198);
                return;
            }

            dispIo.bonlist.AddBonus(condArg3, 12, v3);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cd920)]
        public static void BeginSpellHold(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20001, TextFloaterColor.Red);
            if (!evt.objHndCaller.AddCondition("Held", condArg1, condArg2, condArg3))
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_hold(): unable to add condition");
            }

            var v6 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(v6, out spellPkt) && evt.objHndCaller == spellPkt.caster)
            {
                GameSystems.D20.Actions.curSeqGetTurnBasedStatus().hourglassState = 0;
            }
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100c7a90)]
        public static void SavingThrowPenalty_sp_Feeblemind_Callback(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            if (GameSystems.Spell.CanCastArcaneSpells(evt.objHndCaller))
            {
                dispIo.bonlist.AddBonus(-4, 0, 143);
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c70c0)]
        public static void ControlPlantsEntangleSpellInterruptedCheck(in DispatcherCallbackArgs args)
        {
            var dispIo = args.GetDispIoD20Query();
            if (dispIo.return_val != 1
                && !args.objHndCaller.HasCondition(SpellEffects.SpellControlPlantsDisentangle)
                && !GameSystems.Skill.SkillRoll(args.objHndCaller, SkillId.concentration, 15, out _,
                    SkillCheckFlags.UnderDuress))
            {
                GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x25, args.objHndCaller, null);
                GameSystems.D20.Combat.FloatCombatLine(args.objHndCaller, 54);
                dispIo.return_val = 1;
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cc5b0)]
        public static void BeginSpellCharmPerson(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            if (!evt.objHndCaller.AddCondition("Charmed", condArg1, condArg2, condArg3))
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_charm_person(): unable to add condition");
            }

            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20018, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100dba80)]
        public static void DispelMagicOnAdd(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = new DispIoDispelCheck();
            var condArg1 = evt.GetConditionArg1();
            dispIo.spellId = condArg1;
            dispIo.returnVal = 0;
            dispIo.flags = 1;
            if ((evt.GetConditionArg3()) != 0)
            {
                dispIo.returnVal = 1;
                dispIo.flags |= 0x80;
            }

            evt.objHndCaller.DispatchDispelCheck(dispIo);

            SpellEffects.Spell_remove_spell(in evt, 0, 0);
            SpellEffects.Spell_remove_mod(in evt, 0);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cce10)]
        public static void sub_100CCE10(in DispatcherCallbackArgs evt)
        {
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20027, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100c51d0)]
        public static void SavingThrowModifierCallback(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            if ((dispIo.flags & D20SavingThrowFlag.SPELL_DESCRIPTOR_FEAR) != 0)
            {
                /*INLINED:v2=evt.subDispNode.subDispDef*/
                switch (data2)
                {
                    case 0x98:
                    case 0xA9:
                        dispIo.bonlist.AddBonus(-data1, 13, data2);
                        break;
                    case 0xAC:
                        if (!evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions)
                            && !evt.objHndCaller.HasCondition(SpellEffects.SpellRemoveFear))
                        {
                            dispIo.bonlist.AddBonus(-data1, 13, data2);
                        }

                        break;
                    case 0x8E:
                    case 0xFC:
                        if (!evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions))
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
        public static void sub_100C92D0(in DispatcherCallbackArgs evt, int data)
        {
            var condArg3 = evt.GetConditionArg3();
            var v2 = data;
            var v3 = condArg3;
            if (v3 != -3)
            {
                v2 = -v2;
            }

            evt.GetDispIoDamage().damage.AddDamageBonus(v2, 14, 151);
        }


        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100c6900)]
        public static void CloudkillBeginRound(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) &&
                !evt.objHndCaller.HasCondition(SpellEffects.SpellDelayPoison))
            {
                if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Is_Immune_Poison))
                {
                    GameSystems.Spell.PlayFizzle(evt.objHndCaller);
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 32000, TextFloaterColor.White);
                }
                else if (!D20ModSpells.CheckSpellResistance(evt.objHndCaller, spellPkt))
                {
                    var v2 = GameSystems.Critter.GetHitDiceNum(evt.objHndCaller);
                    var v9 = Dice.D4.Roll();
                    if (v2 <= 6)
                    {
                        if (v2 <= 3)
                        {
                            GameSystems.D20.Combat.Kill(evt.objHndCaller, spellPkt.caster);
                        }
                        else if (GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc,
                            SavingThrowType.Fortitude, 0, spellPkt.spellId))
                        {
                            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30001, TextFloaterColor.White);
                            evt.objHndCaller.AddCondition("Temp_Ability_Loss", 2, v9);
                            var v16 = GameSystems.Stat.GetStatName(Stat.constitution);
                            var v17 = GameSystems.Spell.GetSpellName(25013);
                            var v18 = String.Format("{0}: {1} [{2}]", v17, v16, v9);
                            GameSystems.RollHistory.CreateFromFreeText(v18);
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
                        var v12 = GameSystems.Stat.GetStatName(Stat.constitution);
                        var v13 = GameSystems.Spell.GetSpellName(25013);
                        var v18 = String.Format("{0}: {1} [{2}]", v13, v12, v9);
                        GameSystems.RollHistory.CreateFromFreeText(v18);
                    }
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cfe70)]
        public static void BeginSpellSpikeGrowth(in DispatcherCallbackArgs evt)
        {
            SpellEntry a2;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
                var radiusInches = a2.radiusTarget * locXY.INCH_PER_FEET;
                spellPkt.aoeObj = evt.objHndCaller;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 40, 41, ObjectListFilter.OLC_CRITTERS,
                    radiusInches, 0F, 6.28318548F);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_spike_growth(): unable to save new spell_packet");
    }
*/
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cd7d0)]
        public static void BeginSpellFogCloud(in DispatcherCallbackArgs evt)
        {
            SpellEntry a2;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
                var radiusInches = a2.radiusTarget * locXY.INCH_PER_FEET;
                spellPkt.aoeObj = evt.objHndCaller;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 10, 11, ObjectListFilter.OLC_CRITTERS,
                    radiusInches, 0F, 6.28318548F);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_fog_cloud(): unable to save new spell_packet");
    }
*/
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100dbc60)]
        public static void MagicVestmentOnAdd(in DispatcherCallbackArgs evt)
        {
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Item_Has_Enhancement_Bonus))
            {
                var condArg1 = evt.GetConditionArg1();
                var condArg3 = evt.GetConditionArg3();
                evt.objHndCaller.AddConditionToItem(ItemEffects.ArmorEnhancementBonus, condArg3, 0, 0, 0, condArg1);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ced20)]
        public static void BeginSpellMordenkainensFaithfulHound(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20017, TextFloaterColor.Red);
            if (!evt.objHndCaller.AddCondition("Invisible", condArg1, condArg2, 128))
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_mordenkainens_faithful_hound(): unable to add condition");
            }

            var v5 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 26, 27, ObjectListFilter.OLC_CRITTERS, 360F);
            evt.SetConditionArg4(v5);
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100dc680)]
        public static void HoldTouchSpellTouchAttackHandler(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            var action = (D20Action) dispIo.obj;
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPktBody))
            {
                GameSystems.Script.Spells.SpellSoundPlay(spellPktBody, SpellEvent.AreaOfEffectHit);
                if ((action.d20Caf & D20CAF.HIT) != 0)
                {
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 68);
                    if (D20ModSpells.CheckSpellResistance(action.d20ATarget, spellPktBody))
                    {
                        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                        SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                        SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                    }
                    else
                    {
                        var condArg3 = evt.GetConditionArg3();
                        evt.SetConditionArg3(condArg3 - 1);
                        if (evt.GetConditionArg3() <= 0)
                        {
                            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                            SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                            SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
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
        public static void AttackDiceAnimalGrowth(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackDice();
            if (dispIo.weapon == null)
            {
                // TODO: Changing damage rolls based on size category should be extracted into a common function
                switch (dispIo.dicePacked.Sides)
                {
                    case 2:
                        dispIo.dicePacked = dispIo.dicePacked.WithSides(3);
                        return;
                    case 3:
                        dispIo.dicePacked = dispIo.dicePacked.WithSides(4);
                        return;
                    case 4:
                        dispIo.dicePacked = dispIo.dicePacked.WithSides(6);
                        return;
                    case 6:
                        dispIo.dicePacked = dispIo.dicePacked.WithSides(8);
                        return;
                    case 8:
                        dispIo.dicePacked = new Dice(2, 6, dispIo.dicePacked.Modifier);
                        return;
                    case 10:
                        dispIo.dicePacked = new Dice(2, 6, dispIo.dicePacked.Modifier);
                        return;
                    case 12:
                        dispIo.dicePacked = new Dice(2, 8, dispIo.dicePacked.Modifier);
                        return;
                }
            }
        }


        [DispTypes(DispatcherType.GetMoveSpeedBase)]
        [TempleDllLocation(0x100c86f0)]
        public static void HasteMoveSpeed(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            int v2 = 0; // TODO: This was badly patched in temple.dll and uses a register used for something else
            throw new NotImplementedException();

            evt.objHndCaller.GetBaseStat(Stat.movement_speed);
            var dispIo = evt.GetDispIoMoveSpeed();
            dispIo.bonlist.AddBonus(v2, 12, 174);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cd0a0)]
        public static void TouchAttackOnAdd(in DispatcherCallbackArgs evt)
        {
            var v1 = GameSystems.D20.Actions.curSeqGetTurnBasedStatus();
            v1.tbsFlags |= TurnBasedStatusFlags.TouchAttack;
            var condArg1 = evt.GetConditionArg1();
            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_TouchAttackAdded, condArg1, 0);
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100ca4f0)]
        public static void SilenceSpellFailure(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if (dispIo.return_val != 1)
            {
                var spellData = (D20SpellData) dispIo.obj;
                var spellComponents =
                    GameSystems.Spell.GetSpellComponentRegardMetamagic(spellData.SpellEnum, spellData.metaMagicData);
                if ((spellComponents & SpellComponent.Verbal) != 0)
                {
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 115);
                    var v2 = GameSystems.RollHistory.AddPercentageCheck(evt.objHndCaller, null, 100, 114, 0, 115, 192);
                    GameSystems.RollHistory.CreateRollHistoryString(v2);
                    dispIo.return_val = 1;
                }
                else
                {
                    var v3 = GameSystems.RollHistory.AddPercentageCheck(evt.objHndCaller, null, 0, 114, 0, 62, 192);
                    dispIo.return_val = 0;
                    GameSystems.RollHistory.CreateRollHistoryString(v3);
                }
            }
        }

        [DispTypes(DispatcherType.TurnBasedStatusInit, DispatcherType.D20Signal)]
        [TempleDllLocation(0x100d69f0)]
        public static void TurnBasedStatus_web_Callback(in DispatcherCallbackArgs evt, int data1, ConditionSpec data2)
        {
            var critter = evt.objHndCaller;


            if (critter.HasCondition(data2))
            {
                evt.SetConditionArg4(0);
            }

            var condArg1 = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
            {
                return;
            }

            var bonList = BonusList.Create();
            var v2 = critter.GetStat(Stat.str_mod);
            bonList.AddBonus(v2, 0, 103);

            var v4 = GameSystems.Stat.GetStatName(Stat.strength);
            var v5 = GameSystems.Spell.DispelRoll(critter, bonList, 0, 15, v4);
            if (v5 >= 0)
            {
                GameSystems.Spell.FloatSpellLine(critter, 21003, TextFloaterColor.White); // Escaped!
                if (critter.HasCondition(data2))
                {
                    evt.SetConditionArg4(v5 + 15);
                    return;
                }

                if (!spellPkt.RemoveTarget(critter))
                {
                    Logger.Info("d20_mods_spells.c / _web_break_free_check(): cannot remove target");
                    return;
                }

                Spell_remove_mod(evt.WithoutIO, 0);
                GameSystems.Spell.PlayFizzle(critter);
                spellPkt.AddTarget(critter, null, true);
                var condArg3 = evt.GetConditionArg3();
                critter.AddCondition("sp-Web Off", spellPkt.spellId, spellPkt.durationRemaining, condArg3, v5);
                goto LABEL_19;
            }

            if (critter.HasCondition(data2))
            {
                if (!spellPkt.RemoveTarget(critter))
                {
                    Logger.Info("d20_mods_spells.c / _web_break_free_check(): cannot remove target");
                    return;
                }

                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                GameSystems.Spell.FloatSpellLine(critter, 30002, TextFloaterColor.White);
                var v14 = GameSystems.ParticleSys.CreateAtObj("sp-Web Hit", critter);
                spellPkt.AddTarget(critter, v14, true);
                var v17 = evt.GetConditionArg3();
                critter.AddCondition("sp-Web On", spellPkt.spellId, spellPkt.durationRemaining, v17);
            }
            else
            {
                GameSystems.Spell.FloatSpellLine(critter, 20028, TextFloaterColor.Red);
            }

            LABEL_19:
            GameSystems.Spell.UpdateSpellPacket(spellPkt);
            GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
            return;
        }


        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100c9950)]
        public static void ReduceAbilityModifier(in DispatcherCallbackArgs evt, Stat data1, int bonusMesLine)
        {
            var dispIo = evt.GetDispIoBonusList();
            var condArg1 = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(condArg1, out _))
            {
                return;
            }

            var queryAttribute = evt.GetAttributeFromDispatcherKey();
            if (queryAttribute == data1)
            {
                switch (data1)
                {
                    case Stat.strength:
                        dispIo.bonlist.AddBonus(-2, 20, bonusMesLine);
                        break;
                    case Stat.dexterity:
                        dispIo.bonlist.AddBonus(2, 20, bonusMesLine);
                        break;
                    default:
                        return;
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cd5b0)]
        public static void BeginSpellEntangle(in DispatcherCallbackArgs evt)
        {
            SpellEntry a2;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
                var radiusInches = a2.radiusTarget * locXY.INCH_PER_FEET;
                spellPkt.aoeObj = evt.objHndCaller;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 8, 9, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_entangle(): unable to save new spell_packet");
    }
*/
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100dca80)]
        public static void sub_100DCA80(in DispatcherCallbackArgs evt, int data)
        {
            SpellPacketBody spellPkt;

            var v1 = evt.GetConditionArg3() - 1;
            evt.SetConditionArg3(v1);
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                spellPkt.durationRemaining -= 10;
                if (spellPkt.durationRemaining < 0)
                {
                    spellPkt.durationRemaining = 0;
                }

                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    Logger.Info("d20_mods_spells.c / _cloudkill_hit_trigger(): unable to save new spell_packet");
                    return;
                }
                GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
            }

            if ((v1) == 0)
            {
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
            }
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d62e0)]
        [TemplePlusLocation("spell_condition.cpp:350")]
        public static void SpikeGrowthHitTrigger(in DispatcherCallbackArgs evt, int data)
        {
            SpellPacketBody spellPkt;

            var dispIo = evt.GetDispIoObjEvent();
            var condEventId = evt.GetConditionArg3();
            if (dispIo.evtId != condEventId)
            {
                return;
            }

            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out spellPkt))
            {
                return;
            }

            SpellPktTriggerAoeHitScript(spellPkt.spellId);
            var target = dispIo.tgt;
            if (D20ModSpells.CheckSpellResistance(target, spellPkt))
            {
                return;
            }

            if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
            {
                var particles = GameSystems.ParticleSys.CreateAtObj("sp-Spike Growth-HIT", target);
                spellPkt.AddTarget(target, particles, true);
                target.AddCondition("sp-Spike Growth Hit", spellPkt.spellId, spellPkt.durationRemaining, condEventId);
            }
            else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
            {
                if (GameSystems.D20.Actions.IsCurrentlyPerforming(target, out var sequence))
                {
                    /*
                     * crash fix:
                     * isPerforming() is now retrieving the target's actual action sequence,
                     * rather than "current sequence" (which may be different than the target's
                     * action sequence due to simultaneous actions for several actors)
                     */
                    var distTraversed = sequence.CurrentAction.distTraversed;
                    GameSystems.D20.D20SendSignal(target, D20DispatcherKey.SIG_Combat_Critter_Moved, distTraversed);
                }

                if (!spellPkt.RemoveTarget(evt.objHndCaller))
                {
                    Logger.Info("d20_mods_spells.c / _spike_growth_hit_trigger(): cannot remove target");
                    return;
                }

                SpellEffects.Spell_remove_mod(in evt, 0);
            }

            GameSystems.Spell.UpdateSpellPacket(spellPkt);
            GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
        }


        [DispTypes(DispatcherType.GetAC)]
        [TempleDllLocation(0x100c4190)]
        public static void ChaosHammerAcBonus(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if (data2 == 282)
            {
                dispIo.bonlist.AddBonus(data1, 11, 282);
            }

            dispIo.bonlist.AddBonus(data1, 11, data2);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ce170)]
        public static void BeginSpellIceStorm(in DispatcherCallbackArgs evt)
        {
            SpellEntry a2;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
                var radiusInches = a2.radiusTarget * locXY.INCH_PER_FEET;
                spellPkt.aoeObj = evt.objHndCaller;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 16, 17, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_ice_storm(): unable to save new spell_packet");
    }
*/
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cf920)]
        public static void BeginSpellSleetStorm(in DispatcherCallbackArgs evt)
        {
            SpellEntry a2;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
                var radiusInches = a2.radiusTarget * locXY.INCH_PER_FEET;
                spellPkt.aoeObj = evt.objHndCaller;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 34, 35, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_sleet_storm(): unable to save new spell_packet");
    }
*/
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100d0960)]
        public static void SlipperyMindInit(in DispatcherCallbackArgs evt)
        {
            var spellId = evt.GetConditionArg1();
            var targetIdx = evt.GetConditionArg2();
            if (GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                var target = spellPkt.Targets[targetIdx];
                GameSystems.Spell.FloatSpellLine(target.Object, 20029,
                    TextFloaterColor.White); // Slippery Mind takes affect!
            }
        }


        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100d3a20)]
        public static void ControlPlantsEntangleBeginRound(in DispatcherCallbackArgs evt, int data)
        {
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
            {
                if (GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc,
                    SavingThrowType.Will, 0, spellPkt.spellId))
                {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30001, TextFloaterColor.White);
                }
                else
                {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30002, TextFloaterColor.White);

                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info(
                            "d20_mods_spells.c / _control_plants_entangled_pre_will_save(): cannot remove target");
                        return;
                    }

                    SpellEffects.Spell_remove_mod(in evt, 0);
                    var v5 = GameSystems.ParticleSys.CreateAtObj("sp-Entangle", evt.objHndCaller);
                    spellPkt.AddTarget(evt.objHndCaller, v5, true);
                    var condArg3 = evt.GetConditionArg3();
                    evt.objHndCaller.AddCondition("sp-Control Plants Entangle", spellPkt.spellId,
                        spellPkt.durationRemaining, condArg3);
                }
            }

            {
                GameSystems.Spell.UpdateSpellPacket(spellPkt);
                GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
            } /*  else
  {
    Logger.Info("d20_mods_spells.c / _control_plants_entangled_pre_will_save(): unable to save new spell_packet");
  }
*/
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cc240)]
        public static void sub_100CC240(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var v2 = GameSystems.Stat.GetStatName((Stat) condArg3);
            var suffix = String.Format(" [{0}]", v2);
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20022, TextFloaterColor.Red, suffix: suffix);
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100c4af0)]
        public static void sub_100C4AF0(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoDamage();
            var v2 = data2;
            var condArg3 = evt.GetConditionArg3();
            dispIo.damage.AddDamageBonus(condArg3, 14, v2);
        }


        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100d4440)]
        public static void sub_100D4440(in DispatcherCallbackArgs evt, int data)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                if (GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc,
                    SavingThrowType.Reflex, 0, spellPkt.spellId))
                {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30001, TextFloaterColor.White);
                }
                else
                {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 30002, TextFloaterColor.White);

                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _entangle_off_reflex_save(): cannot remove target");
                        return;
                    }

                    SpellEffects.Spell_remove_mod(in evt, 0);
                    var v5 = GameSystems.ParticleSys.CreateAtObj("sp-Entangle", evt.objHndCaller);
                    spellPkt.AddTarget(evt.objHndCaller, v5, true);
                    var condArg3 = evt.GetConditionArg3();
                    evt.objHndCaller.AddCondition("sp-Entangle On", spellPkt.spellId, spellPkt.durationRemaining,
                        condArg3);
                }
            }

            {
                GameSystems.Spell.UpdateSpellPacket(spellPkt);
                GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
            } /*  else
  {
    Logger.Info("d20_mods_spells.c / _entangle_off_reflex_save(): unable to save new spell_packet");
  }
*/
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100c5990)]
        public static void sub_100C5990(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoDamage();
            if (data2 == 173)
            {
                if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement))
                {
                    dispIo.damage.AddDamageBonus(-data1, 0, data2);
                }
            }
            else
            {
                DamagePacket v3 = dispIo.damage;
                if (data2 == 223)
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
        public static void SavingThrow_sp_ConsecrateHitUndead_Callback(in DispatcherCallbackArgs evt, int data1,
            int data2)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            dispIo.bonlist.AddBonus(-data1, 17, data2);
        }


        [DispTypes(DispatcherType.BaseCasterLevelMod)]
        [TempleDllLocation(0x100c71a0)]
        public static void sub_100C71A0(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val += data2;
        }


        [DispTypes(DispatcherType.TakingDamage)]
        [TempleDllLocation(0x100ddb90)]
        public static void StoneskinTakingDamage(in DispatcherCallbackArgs evt, int data1, D20AttackPower data2)
        {
            int condArg4;
            int v6;
            var dispIo = evt.GetDispIoDamage();
            DamagePacket v2 = dispIo.damage;
            var v10 = dispIo;
            var v3 = dispIo.damage.GetOverallDamageByType();
            if (v3 <= evt.GetConditionArg4())
            {
                condArg4 = data1;
            }
            else
            {
                condArg4 = evt.GetConditionArg4();
            }

            v2.AddPhysicalDR(condArg4, data2, 0x68);
            var v5 = v2.GetOverallDamageByType();
            v10.damage.finalDamage = v5;
            if (v3 > evt.GetConditionArg4())
            {
                v6 = -v10.damage.finalDamage;
            }
            else
            {
                v6 = v5 - v3 + evt.GetConditionArg4();
            }

            evt.SetConditionArg4(v6);
            Logger.Info("absorbed {0} points of damage, DR points left: {1}", v3 - v5, v6);
            if (v6 <= 0)
            {
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(in evt, 0, 0);
                SpellEffects.Spell_remove_mod(in evt, 0);
            }
            else
            {
                var suffix = String.Format(" {0} ({1}/{2:+#;-#;0})", v6, data1, data2);
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20009, TextFloaterColor.White, suffix: suffix);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cc400)]
        public static void StunnedFloatMessage(in DispatcherCallbackArgs args)
        {
            GameSystems.Spell.FloatSpellLine(args.objHndCaller, 20021, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c5d90)]
        public static void DeafnessSpellFailure(in DispatcherCallbackArgs evt)
        {
            int spellClassCode;
            int spellEnum;
            SpellStoreData spData;

            var spellLvl = 0;
            var mmData = 0;
            var dispIo = evt.GetDispIoD20Query();
            if (dispIo.return_val != 1)
            {
                var spellData = (D20SpellData) dispIo.obj;
                var components =
                    GameSystems.Spell.GetSpellComponentRegardMetamagic(spellData.SpellEnum, spellData.metaMagicData);

                if ((components & SpellComponent.Verbal) != 0)
                {
                    int v8 = Dice.D100.Roll();
                    if (v8 >= 20)
                    {
                        var v10 = GameSystems.RollHistory.AddPercentageCheck(evt.objHndCaller, null, 20, 79, v8, 62,
                            192);
                        GameSystems.RollHistory.CreateRollHistoryString(v10);
                    }
                    else
                    {
                        GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x23, evt.objHndCaller, null);
                        GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 78);
                        dispIo.return_val = 1;
                        var v9 = GameSystems.RollHistory.AddPercentageCheck(evt.objHndCaller, null, 20, 79, v8, 78,
                            192);
                        GameSystems.RollHistory.CreateRollHistoryString(v9);
                    }
                }
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100d4220)]
        public static void EntangleBreakFree(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoD20Signal();
            evt.GetConditionArg1();
            if ((dispIo.data2) == 0)
            {
                var spellId = evt.GetConditionArg1();
                SpellPacketBody spellPkt;
                if (!GameSystems.Spell.TryGetActiveSpell(spellId, out spellPkt))
                {
                    return;
                }

                var bonList = BonusList.Create();
                var strengthBonus = evt.objHndCaller.GetStat(Stat.str_mod);
                bonList.AddBonus(strengthBonus, 0, 103);
                var v6 = GameSystems.Stat.GetStatName(0);
                if (GameSystems.Spell.DispelRoll(evt.objHndCaller, bonList, 0, 15, v6) < 0)
                {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20027, TextFloaterColor.Red);
                }
                else
                {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 21003, TextFloaterColor.White);

                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Info("d20_mods_spells.c / _entangle_on_break_free_check(): cannot remove target");
                        return;
                    }

                    SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                    spellPkt.AddTarget(evt.objHndCaller, null, true);
                    var condArg3 = evt.GetConditionArg3();
                    evt.objHndCaller.AddCondition("sp-Entangle Off", spellPkt.spellId, spellPkt.durationRemaining,
                        condArg3);
                }

                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _entangle_on_break_free_check(): unable to save new spell_packet");
    }
*/
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cd8f0)]
        public static void sub_100CD8F0(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_TouchAttackAdded, condArg1, 0);
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c9e90)]
        public static void CanBeAffectedActionFrame_Sanctuary(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var dispIo = evt.GetDispIoD20Query();
            var action = (D20Action) dispIo.obj;
            if (action.d20ActType != D20ActionType.CAST_SPELL)
            {
                return;
            }

            if (GameSystems.Spell.TryGetActiveSpell(action.spellId, out var spellPkt))
            {
                if (!GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out var spellEntry)
                    || !spellEntry.IsBaseModeTarget(UiPickerType.Multi))
                {
                    // TODO: This check is WEIRD to say the least!
                    return;
                }
            }

            var sanctuarySpellId = GetSpellConditionSpellId(action.d20ATarget, SpellSanctuary);
            if (sanctuarySpellId == 0)
            {
                return;
            }

            AllowSanctuarySave(action, sanctuarySpellId);

            var saveFailedForSpell = GetSpellConditionSpellId(action.d20APerformer, SpellSanctuarySaveFailed);
            if (saveFailedForSpell == sanctuarySpellId)
            {
                dispIo.return_val = 0;
                dispIo.resultData = (ulong) evt.GetConditionArg3(); // TODO: Doesnt seem to be used!
                GameSystems.D20.Combat.FloatCombatLine(action.d20ATarget, D20CombatMessage.sanctuary);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cdc30)]
        public static void BeginSpellGrease(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
            {
                SpellEntry a2;
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
                var radiusInches = a2.radiusTarget * locXY.INCH_PER_FEET;
                spellPkt.aoeObj = evt.objHndCaller;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 14, 15, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_grease(): unable to save new spell_packet");
    }
*/
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cbf10)]
        public static void sub_100CBF10(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            if (!evt.objHndCaller.AddCondition("Charmed", condArg1, condArg2, condArg3))
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_animal_friendship(): unable to add condition");
            }

            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20018, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cc420)]
        public static void CalmEmotionsBeginSpell(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody v4;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out v4))
            {
                if (!v4.caster.AddCondition("sp-Concentrating", condArg1, 0, 0))
                {
                    Logger.Info(
                        "d20_mods_spells.c / _begin_spell_calm_emotions(): unable to add condition to spell_caster");
                }
            }

            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20031, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100dcb80)]
        public static void ChillTouchAttackHandler(in DispatcherCallbackArgs evt, int data)
        {
            GameObjectBody tgt;
            GameObjectBody caster;
            GameObjectBody v21;
            Dice dicePacked;
            D20ActionType actionType;
            int spellId_1;
            SpellPacketBody spellPktBody;

            var dispIo = evt.GetDispIoD20Signal();
            var action = (D20Action) dispIo.obj;
            if ((action.d20Caf & D20CAF.HIT) == 0)
            {
                GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 69);
                return;
            }

            var condArg1 = evt.GetConditionArg1();
            GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPktBody);
            GameSystems.Script.Spells.SpellSoundPlay(spellPktBody, SpellEvent.AreaOfEffectHit);
            GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 68);
            if (D20ModSpells.CheckSpellResistance(action.d20ATarget, spellPktBody))
            {
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                return;
            }

            if (GameSystems.Critter.IsCategory(action.d20ATarget, MonsterCategory.undead))
            {
                if (!GameSystems.D20.Combat.SavingThrowSpell(action.d20ATarget, spellPktBody.caster, spellPktBody.dc,
                    SavingThrowType.Will, 0, spellPktBody.spellId))
                {
                    GameSystems.Spell.FloatSpellLine(action.d20ATarget, 30002, TextFloaterColor.White);
                    GameSystems.Spell.FloatSpellLine(action.d20ATarget, 20013, TextFloaterColor.Red);
                    var v13 = Dice.D4.Roll();
                    action.d20ATarget.AddCondition("sp-Cause Fear", spellPktBody.spellId, v13, 0);
                    goto LABEL_15;
                }

                v21 = action.d20ATarget;
            }
            else
            {
                if ((action.d20Caf & D20CAF.CRITICAL) != 0)
                {
                    spellId_1 = spellPktBody.spellId;
                    actionType = action.d20ActType;
                    dicePacked = new Dice(2, 6);
                    caster = evt.objHndCaller;
                    tgt = action.d20ATarget;
                }
                else
                {
                    spellId_1 = spellPktBody.spellId;
                    actionType = action.d20ActType;
                    dicePacked = Dice.D6;
                    caster = evt.objHndCaller;
                    tgt = action.d20ATarget;
                }

                GameSystems.D20.Combat.SpellDamageFull(tgt, caster, dicePacked, DamageType.NegativeEnergy,
                    D20AttackPower.UNSPECIFIED, actionType, spellId_1, 0);
                if (!GameSystems.D20.Combat.SavingThrowSpell(action.d20ATarget, spellPktBody.caster, spellPktBody.dc,
                    SavingThrowType.Fortitude, 0, spellPktBody.spellId))
                {
                    GameSystems.Spell.FloatSpellLine(action.d20ATarget, 30002, TextFloaterColor.White);
                    GameSystems.Spell.FloatSpellLine(action.d20ATarget, 20022, TextFloaterColor.Red);
                    action.d20ATarget.AddCondition("Temp_Ability_Loss", 0, 1);
                    goto LABEL_15;
                }

                v21 = action.d20ATarget;
            }

            GameSystems.Spell.FloatSpellLine(v21, 30001, TextFloaterColor.White);
            LABEL_15:
            var condArg3 = evt.GetConditionArg3();
            evt.SetConditionArg3(condArg3 - 1);
            if (evt.GetConditionArg3() <= 0)
            {
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cc930)]
        public static void sub_100CC930(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20021, TextFloaterColor.Red);
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                /*INLINED:v2=evt.subDispNode.condNode*/
                spellPkt.duration = 1;
                spellPkt.durationRemaining = 1;
                evt.SetConditionArg2(1);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_color_spray_stun(): unable to save new spell_packet");
    }
*/
            }
            else
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_color_spray_stun(): unable to get spell_packet");
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cbe00)]
        [TemplePlusLocation("spell_condition.cpp:86")]
        public static void SpAidOnAdd(in DispatcherCallbackArgs evt)
        {
            var spellId = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_aid(): unable to get spell_packet");
                return;
            }

            // TemplePlus Fix: Add 1 HP per caster level (up to 10)
            int tempHpAmt = Dice.D8.Roll() + Math.Min(10, spellPkt.casterLevel);

            var extraText = $"[{tempHpAmt}] ";
            // Temporary Hit Points Gained.
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20005, TextFloaterColor.White, suffix: extraText);
            Logger.Info("d20_mods_spells.c / _begin_aid(): gained {0} temporary hit points", tempHpAmt);

            if (!evt.objHndCaller.AddCondition("Temporary_Hit_Points", spellId, condArg2, tempHpAmt))
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_aid(): unable to add condition");
            }
        }

        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100c4b30)]
        public static void EmotionToHitBonus2(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            switch (data2)
            {
                case 0x98:
                case 0xA9:
                    dispIo.bonlist.AddBonus(-data1, 13, data2);
                    break;
                case 0xAC:
                case 0x103:
                    if (!evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions)
                        && !evt.objHndCaller.HasCondition(SpellEffects.SpellRemoveFear))
                    {
                        dispIo.bonlist.AddBonus(-data1, 13, data2);
                    }

                    break;
                case 0x8E:
                case 0xFC:
                case 0x104:
                case 0x12A:
                case 0x12B:
                    if (!evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions))
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
        public static void GlibnessSkillLevel(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoObjBonus();
            if (!GameSystems.Combat.IsCombatActive())
            {
                dispIo.bonOut.AddBonus(data1, 0, data2);
            }
        }


        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100c9760)]
        public static void sub_100C9760(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoBonusList();
            dispIo.bonlist.AddBonus(data1, 13, data2);
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100d75d0)]
        public static void OnSpellEndRemoveMod(in DispatcherCallbackArgs evt, int data1, ConditionSpec data2)
        {
            var dispIo = evt.GetDispIoD20Signal();
            if (dispIo.data1 == evt.GetConditionArg1())
            {
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100dd240)]
        public static void sub_100DD240(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            if (evt.GetConditionArg3() > 0)
            {
                var dispIo = evt.GetDispIoAttackBonus();
                dispIo.bonlist.AddBonus(data1, 34, data2);
                if ((dispIo.attackPacket.flags & D20CAF.FINAL_ATTACK_ROLL) != 0)
                {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                    SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                    SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cfdb0)]
        public static void SpiritualWeaponBeginSpellDismiss(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Anim.Interrupt(evt.objHndCaller, AnimGoalPriority.AGP_HIGHEST);
                var v4 = evt.GetConditionArg1();
                if (!spellPkt.caster.AddCondition("Dismiss", v4, 0, 0))
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
        public static void FireShield_callback_31h(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoReflexThrow();
            var condArg3 = evt.GetConditionArg3();
            DamageType shieldResistance;
            if (condArg3 == 3)
            {
                shieldResistance = DamageType.Fire;
            }
            else if (condArg3 == 9)
            {
                shieldResistance = DamageType.Cold;
            }
            else
            {
                return;
            }

            if (dispIo.attackType == shieldResistance && dispIo.throwResult)
            {
                // TODO: throwResult was set to 4 here to indicate full evasion I'd guess
                dispIo.effectiveReduction = 0;
                dispIo.damageMesLine = 109;
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cd230)]
        public static void DominateAnimal(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            if (!evt.objHndCaller.AddCondition("Charmed", condArg1, condArg2, condArg3))
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_dominate_animal(): unable to add condition");
            }

            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20018, TextFloaterColor.Red);
            var v6 = GameSystems.Party.GetConsciousLeader();
            GameSystems.Critter.AddFollower(evt.objHndCaller, v6, true, false);
            GameUiBridge.UpdatePartyUi();
        }


        [DispTypes(DispatcherType.RadialMenuEntry)]
        [TempleDllLocation(0x100c3450)]
        public static void TouchAttackDischargeRadialMenu(in DispatcherCallbackArgs evt, int data)
        {
            var radMenuEntry = RadialMenuEntry.CreateAction(5036, D20ActionType.TOUCH_ATTACK, 0, "TAG_TOUCH_ATTACK");
            GameSystems.D20.RadialMenu.AddToStandardNode(evt.objHndCaller, ref radMenuEntry,
                RadialMenuStandardNode.Offense);
        }

        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100db9c0)]
        public static void sub_100DB9C0(in DispatcherCallbackArgs evt, ConditionSpec data1, int data2)
        {
            if (evt.GetDispIoCondStruct().condStruct == (ConditionSpec) data1)
            {
                SpellEffects.Spell_remove_spell(in evt, 0, 0);
                SpellEffects.Spell_remove_mod(in evt, 0);
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c8cc0)]
        public static void MagicCircleInwardAooPossible(in DispatcherCallbackArgs evt)
        {
            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                return;
            }

            var dispIo = evt.GetDispIoD20Query();

            // TODO: This is changed vs. vanilla, I thought it only applies to summoned creatures in the first place ?!
            if (evt.objHndCaller.HasCondition(SpellEffects.SpellSummoned))
            {
                dispIo.return_val = D20ModSpells.CheckSpellResistance(evt.objHndCaller, spellPkt) ? 1 : 0;
            }
            else
            {
                dispIo.return_val = 1;
            }
        }


        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100dd360)]
        public static void GuidanceSkillLevel(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoObjBonus();
            if (evt.GetConditionArg(4) > 0)
            {
                dispIo.bonOut.AddBonus(data1, 34, data2);
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
            }
        }


        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100c7a60)]
        public static void sub_100C7A60(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoBonusList();
            dispIo.bonlist.AddCap(0, data, 0x8F);
        }

        private static bool IsSummonedCreatureAffectedByDispelAlignment(GameObjectBody critter, SpellPacketBody spell,
            int dispelType)
        {
            if (D20ModSpells.CheckSpellResistance(critter, spell))
            {
                return false;
            }

            return dispelType switch
            {
                // Dispel Chaos
                69 => critter.HasChaoticAlignment(),
                // Dispel Evil
                70 => critter.HasEvilAlignment(),
                // Dispel Good
                71 => critter.HasGoodAlignment(),
                // Dispel Law
                72 => critter.HasLawfulAlignment(),
                _ => false
            };
        }

        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100dcf10)]
        public static void DispelAlignmentTouchAttackSignalHandler(in DispatcherCallbackArgs evt, int data)
        {
            var action = (D20Action) evt.GetDispIoD20Signal().obj;
            if ((action.d20Caf & D20CAF.HIT) == 0)
            {
                GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 69);
                return;
            }

            var condArg1 = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPktBody))
            {
                return;
            }

            GameSystems.Script.Spells.SpellSoundPlay(spellPktBody, SpellEvent.AreaOfEffectHit);
            GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 68);
            if (action.d20ATarget.IsPC())
            {
                GameSystems.Spell.PlayFizzle(action.d20ATarget);
            }
            else if (action.d20ATarget.HasCondition(SpellEffects.SpellSummoned))
            {
                if (IsSummonedCreatureAffectedByDispelAlignment(action.d20ATarget, spellPktBody, data))
                {
                    if (GameSystems.D20.Combat.SavingThrowSpell(action.d20ATarget, spellPktBody.caster, spellPktBody.dc,
                        SavingThrowType.Will, 0, spellPktBody.spellId))
                    {
                        GameSystems.Spell.FloatSpellLine(action.d20ATarget, 30001, TextFloaterColor.White);
                    }
                    else
                    {
                        GameSystems.Spell.FloatSpellLine(action.d20ATarget, 30002, TextFloaterColor.White);
                        GameSystems.D20.Combat.Kill(action.d20ATarget, evt.objHndCaller);
                    }
                }
            }
            else
            {
                var dispIo = new DispIoDispelCheck();
                dispIo.spellId = condArg1;
                dispIo.returnVal = 1;
                switch (data)
                {
                    // Dispel Chaos
                    case 69:
                        dispIo.flags = 2;
                        break;
                    // Dispel Evil
                    case 70:
                        dispIo.flags = 4;
                        break;
                    // Dispel Good
                    case 71:
                        dispIo.flags = 8;
                        break;
                    // Dispel Law
                    case 72:
                        dispIo.flags = 16;
                        break;
                    default:
                        dispIo.flags = 0;
                        break;
                }

                action.d20ATarget.DispatchDispelCheck(dispIo);
            }

            SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
            SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
        }


        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100c3fe0)]
        public static void sub_100C3FE0(in DispatcherCallbackArgs evt, int data)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                if (GameSystems.D20.Combat.SavingThrowSpell(evt.objHndCaller, spellPkt.caster, spellPkt.dc,
                    SavingThrowType.Will, 0, spellPkt.spellId))
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
        public static void EntangleAttributeMalus(in DispatcherCallbackArgs evt, Stat attribute, int bonusMesLine)
        {
            var dispIo = evt.GetDispIoBonusList();
            var queryAttribute = evt.GetAttributeFromDispatcherKey();
            if (queryAttribute == attribute)
            {
                dispIo.bonlist.AddBonus(-4, 0, bonusMesLine);
            }
        }


        [DispTypes(DispatcherType.AbilityCheckModifier)]
        [TempleDllLocation(0x100c5020)]
        public static void AbilityCheckModifierEmotion(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            int v3;

            var dispIo = evt.GetDispIoObjBonus();
            switch (data2)
            {
                case 0xA9:
                    v3 = -data1;
                    goto LABEL_11;
                case 0xAC:
                    if (!evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions)
                        && !evt.objHndCaller.HasCondition(SpellEffects.SpellRemoveFear))
                    {
                        dispIo.bonOut.AddBonus(-data1, 13, data2);
                    }

                    break;
                case 0x103:
                    if (!evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions))
                    {
                        dispIo.bonOut.AddBonus(-data1, 13, data2);
                    }

                    break;
                case 0x104:
                    if (!evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions))
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
        public static void sub_100C6280(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoMoveSpeed();
            dispIo.factor = dispIo.factor * 0.75F;
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100c89c0)]
        public static void IceStormDamage(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
            {
                // TODO: I believe the damage types are wrong and circumvent DR that should normally apply???
                var v2 = spellPkt.spellId;
                var v3 = new Dice(3, 6);
                GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v3, 0,
                    D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, v2, 0);
                var v4 = spellPkt.spellId;
                var v5 = new Dice(2, 6);
                GameSystems.D20.Combat.SpellDamageFull(evt.objHndCaller, spellPkt.caster, v5, DamageType.Cold,
                    D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, v4, 0);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100d03d0)]
        public static void sub_100D03D0(in DispatcherCallbackArgs evt)
        {
            var prefix = String.Format("[{0}] ", 1);
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20005, TextFloaterColor.White, prefix: prefix);
            Logger.Info("d20_mods_spells.c / _begin_spell_virtue(): gained {0} temporary hit points", 1);
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
            {
                var condArg2 = evt.GetConditionArg2();
                var v5 = evt.GetConditionArg1();
                if (!evt.objHndCaller.AddCondition("Temporary_Hit_Points", v5, condArg2, 1))
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
        public static void BeginSpellSolidFog(in DispatcherCallbackArgs evt)
        {
            SpellEntry a2;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
                var radiusInches = a2.radiusTarget * locXY.INCH_PER_FEET;
                spellPkt.aoeObj = evt.objHndCaller;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 38, 39, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.GetMoveSpeed)]
        [TempleDllLocation(0x100c7080)]
        public static void ControlPlantsEntangleMovementSpeed(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoMoveSpeed();
            if (!evt.objHndCaller.HasCondition(SpellEffects.SpellControlPlantsDisentangle))
            {
                dispIo.factor = 0;
            }
        }


        [DispTypes(DispatcherType.ConditionAdd, DispatcherType.NewDay)]
        [TempleDllLocation(0x100cc800)]
        public static void sub_100CC800(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg3(0);
        }


        [DispTypes(DispatcherType.DispelCheck)]
        [TempleDllLocation(0x100db690)]
        public static void DispelCheck(in DispatcherCallbackArgs evt, int data)
        {
            SpellPacketBody spPkt;
            SpellPacketBody dispellPkt;

            var bonlist = BonusList.Create();
            var dispIo = evt.GetDispIoDispelCheck();
            if ((dispIo.flags & 0x20) != 0)
            {
                // A spell has expired
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(in evt, 0, 0);
                SpellEffects.Spell_remove_mod(in evt, 0);
            }

            if (!GameSystems.Spell.TryGetActiveSpell(dispIo.spellId, out dispellPkt))
            {
                Logger.Info("d20_mods_spells.c / _dispel_check(): error getting spellid packet for dispel_packet");
                return;
            }

            var condArg1 = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(condArg1, out spPkt))
            {
                Logger.Info("d20_mods_spells.c / _dispel_check(): error getting spellid packet for spell_packet");
                return;
            }

            var flags = dispIo.flags;
            if ((flags & 1) != 0 && spPkt.spellKnownSlotLevel < 4 && (flags & 0x80) == 0
                || (flags & 0x80) != 0 && spPkt.spellKnownSlotLevel < 4 && dispIo.returnVal > 0
                || (flags & 0x40) != 0 || (flags & 0x1E) != 0 && dispIo.returnVal > 0)
            {
                bonlist.AddBonus(dispellPkt.casterLevel, 0, 203);
                var v6 = GameSystems.Spell.GetSpellName(dispellPkt.spellEnum);
                if (GameSystems.Spell.DispelRoll(dispellPkt.caster, bonlist, 0, spPkt.casterLevel + 11, v6) >= 0
                    || dispellPkt.caster == spPkt.caster)
                {
                    flags = dispIo.flags;
                    var isDispelMagic = dispIo.flags & 1;
                    if ((isDispelMagic) == 0 || (flags & 0x80) != 0)
                    {
                        --dispIo.returnVal;
                    }

                    if (isDispelMagic == 1
                        || (flags & 0x40) != 0 || (flags & 2) != 0 && spPkt.caster.HasChaoticAlignment()
                        || (dispIo.flags & 4) != 0 && spPkt.caster.HasEvilAlignment()
                        || (dispIo.flags & 8) != 0 && spPkt.caster.HasGoodAlignment()
                        || (dispIo.flags & 0x10) != 0 && spPkt.caster.HasLawfulAlignment())
                    {
                        var spellName = GameSystems.Spell.GetSpellName(spPkt.spellEnum);
                        var suffix = $" [{spellName}]";
                        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20002, TextFloaterColor.White,
                            suffix: suffix); // Dispel attempt successful
                        SpellEffects.Spell_remove_spell(in evt, 0, 0);
                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }
                }
                else
                {
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20003,
                        TextFloaterColor.Red); // Dispel attempt failed
                    GameSystems.ParticleSys.CreateAtObj("Fizzle", evt.objHndCaller);
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cc6d0)]
        public static void sub_100CC6D0(in DispatcherCallbackArgs evt)
        {
            GameSystems.MapFogging.Disable();
        }


        [DispTypes(DispatcherType.TakingDamage)]
        [TempleDllLocation(0x100c7fb0)]
        public static void GaseousFormTakingDamage(in DispatcherCallbackArgs evt, int amount, D20AttackPower bypass)
        {
            var dispIo = evt.GetDispIoDamage();
            dispIo.damage.AddPhysicalDR(amount, bypass, 104);
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100d32b0)]
        [TemplePlusLocation("condition.cpp:3527")]
        public static void ConcentratingActionRecipient(in DispatcherCallbackArgs evt, int data)
        {
            SpellPacketBody conceSpellPkt;
            SpellPacketBody spellPkt;

            var dispIo = evt.GetDispIoD20Signal();
            var action = (D20Action) dispIo.obj;
            if (action == null)
            {
                return;
            }

            var conceSpellId = evt.GetConditionArg1();

            if (action.d20ActType == D20ActionType.CAST_SPELL)
            {
                var d20aSpellId = action.spellId;
                if (d20aSpellId != conceSpellId)
                {
                    if (!GameSystems.Spell.TryGetActiveSpell(d20aSpellId, out spellPkt))
                    {
                        Logger.Info(
                            "d20_mods_spells.c / _concentrating_action_recipient(): error, unable to retrieve spell_packet");
                        return;
                    }

                    if (!GameSystems.Spell.TryGetActiveSpell(conceSpellId, out conceSpellPkt))
                    {
                        Logger.Info(
                            "d20_mods_spells.c / _concentrating_action_recipient(): error, unable to retrieve concentration_packet");
                        return;
                    }

                    // meld into stone hack
                    if (conceSpellPkt.spellEnum == WellKnownSpells.MeldIntoStone)
                    {
                        return;
                    }

                    // TemplePlus: DC is not increased by level of spell
                    if (GameSystems.Skill.SkillRoll(evt.objHndCaller, SkillId.concentration, spellPkt.dc, out _,
                        SkillCheckFlags.UnderDuress))
                    {
                        return;
                    }

                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(32, evt.objHndCaller, null);
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 54);
                    var removeEvt = new DispatcherCallbackArgs(evt.subDispNode, evt.objHndCaller,
                        DispatcherType.D20Signal,
                        D20DispatcherKey.SIG_Remove_Concentration, null);
                    SpellEffects.Spell_remove_mod(removeEvt, 0);
                }
            }
        }

        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100c68b0)]
        public static void CloudkillDamagePreAdd(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var dispIo = evt.GetDispIoCondStruct();
            if (dispIo.condStruct == (ConditionSpec) data
                && evt.objHndCaller.HasCondition(SpellEffects.SpellCloudkill))
            {
                dispIo.outputFlag = false;
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cdb10)]
        public static void GoodberryAdd(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            var v4 = evt.GetConditionArg1();
            if (!evt.objHndCaller.AddCondition("sp-Goodberry Tally", condArg1, condArg2, condArg3))
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_goodberry(): unable to add condition");
            }

            if (GameSystems.Spell.TryGetActiveSpell(v4, out var spellPkt))
            {
                Dice v7 = Dice.Constant(1);
                GameSystems.Combat.SpellHeal(evt.objHndCaller, spellPkt.caster, v7, D20ActionType.CAST_SPELL, v4);
                GameSystems.Critter.HealSubdualSub_100B9030(evt.objHndCaller, 1);
            }
        }

        private static void ApplyModelScale(GameObjectBody obj, float scale)
        {
            var newScale = (int) (obj.GetInt32(obj_f.model_scale) * scale);
            obj.SetInt32(obj_f.model_scale, newScale);

            GameSystems.Critter.UpdateModelEquipment(obj);
            var newRunSpeed = obj.GetFloat(obj_f.speed_run) / scale;
            obj.SetFloat(obj_f.speed_run, newRunSpeed);

            var newWalkSpeed = obj.GetFloat(obj_f.speed_walk) / scale;
            obj.SetFloat(obj_f.speed_walk, newWalkSpeed);
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cf2c0)]
        public static void SpellReduceSetModelScale(in DispatcherCallbackArgs args)
        {
            ApplyModelScale(args.objHndCaller, 1 / 1.8f);
        }

        [DispTypes(DispatcherType.GetDefenderConcealmentMissChance)]
        [TempleDllLocation(0x100c62d0)]
        [TemplePlusLocation("spell_condition.cpp:254")]
        public static void BlinkMissChance(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            // Fix for Blink defender miss chance (the conditions for true seeing reducing the chance to 20% was flipped)
            var dispIo = evt.GetDispIoAttackBonus();

            var missChance = 50;

            var attacker = dispIo.attackPacket.attacker;

            // reduce to 20% miss chance if attacker can see invisible/ethereal creatures
            if (GameSystems.D20.D20Query(attacker, D20DispatcherKey.QUE_Critter_Has_True_Seeing)
                || GameSystems.D20.D20Query(attacker, D20DispatcherKey.QUE_Critter_Can_See_Invisible))
            {
                missChance = 20;
            }

            if (GameSystems.D20.D20Query(attacker, D20DispatcherKey.QUE_Critter_Can_See_Ethereal))
            {
                // Further reduce to 20 or 0
                missChance = (missChance == 20) ? 0 : 20;
            }

            if (missChance > 0)
            {
                dispIo.bonlist.AddBonus(missChance, 19, data2);
            }
        }

        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d51e0)]
        public static void InvisibilitySphereAoeEvent(in DispatcherCallbackArgs evt, int data)
        {
            SpellPacketBody spellPkt;

            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
                {
                    SpellPktTriggerAoeHitScript(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        var v5 = dispIo.tgt;
                        var v7 = GameSystems.ParticleSys.CreateAtObj("sp-Invisibility Sphere-HIT", v5);
                        spellPkt.AddTarget(dispIo.tgt, v7, true);
                        dispIo.tgt.AddCondition("sp-Invisibility Sphere Hit", spellPkt.spellId,
                            spellPkt.durationRemaining, dispIo.evtId);
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        if (!spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info("d20_mods_spells.c / _invisibility_sphere_hit_trigger(): cannot remove target");
                            return;
                        }

                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                            spellPkt.spellId, 0);
                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }
                }

                GameSystems.Spell.UpdateSpellPacket(spellPkt);
                GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cda00)]
        public static void BeginSpellGhoulTouchStench(in DispatcherCallbackArgs evt)
        {
            SpellEntry a2;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
                spellPkt.aoeObj = evt.objHndCaller;
                var v2 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 12, 13, ObjectListFilter.OLC_CRITTERS,
                    120F);
                evt.SetConditionArg3(v2);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100c61d0)]
        public static void SavingThrow_sp_BestowCurseRolls_Callback(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            dispIo.bonlist.AddBonus(-data1, 0, data2);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cff90)]
        public static void BeginSpellSpikeStones(in DispatcherCallbackArgs evt)
        {
            SpellEntry a2;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
                var radiusInches = a2.radiusTarget * locXY.INCH_PER_FEET;
                spellPkt.aoeObj = evt.objHndCaller;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 42, 43, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_spike_stones(): unable to save new spell_packet");
    }
*/
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c4370)]
        public static void sub_100C4370(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var dispIo = evt.GetDispIoD20Query();
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                dispIo.return_val = 1;
                dispIo.obj = spellPkt.caster;
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cf460)]
        public static void sub_100CF460(in DispatcherCallbackArgs evt)
        {
            int condArg3;
            int condArg2;
            int v3;
            GameObjectBody v4;

            evt.SetConditionArg4(10);
            switch (evt.GetConditionArg2())
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
        public static void ShockingGraspTouchAttack(in DispatcherCallbackArgs evt, int data)
        {
            SpellPacketBody spellPktBody;

            var action = (D20Action) evt.GetDispIoD20Signal().obj;

            if ((action.d20Caf & D20CAF.HIT) == 0)
            {
                GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 69);
                return;
            }

            var condArg1 = evt.GetConditionArg1();
            GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPktBody);
            GameSystems.Script.Spells.SpellSoundPlay(spellPktBody, SpellEvent.AreaOfEffectHit);
            GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 68);
            if (D20ModSpells.CheckSpellResistance(action.d20ATarget, spellPktBody))
            {
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
            }
            else
            {
                var v5 = Math.Min(5, spellPktBody.casterLevel);

                if ((action.d20Caf & D20CAF.CRITICAL) != 0)
                {
                    v5 *= 2;
                }

                var v6 = new Dice(v5, 8);
                GameSystems.D20.Combat.SpellDamageFull(action.d20ATarget, evt.objHndCaller, v6, DamageType.Electricity,
                    D20AttackPower.UNSPECIFIED,
                    action.d20ActType, spellPktBody.spellId, 0);
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cd2d0)]
        public static void Dominate_Person(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            if (!evt.objHndCaller.AddCondition("Charmed", condArg1, condArg2, condArg3))
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_dominate_person(): unable to add condition");
            }

            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20018, TextFloaterColor.Red);
            var v6 = GameSystems.Party.GetConsciousLeader();
            GameSystems.Critter.AddFollower(evt.objHndCaller, v6, true, false);
            GameUiBridge.UpdatePartyUi();
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cc300)]
        public static void BreakEnchantmentInit(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = new DispIoDispelCheck();
            dispIo.spellId = evt.GetConditionArg1();
            dispIo.returnVal = 0;
            dispIo.flags = 64;
            evt.objHndCaller.DispatchDispelCheck(dispIo);
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100cb760)]
        public static void WebSpellInterrupted(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if (dispIo.return_val != 1
                && !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement)
                && !GameSystems.Skill.SkillRoll(evt.objHndCaller, SkillId.concentration, 15, out _,
                    SkillCheckFlags.UnderDuress))
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
        public static void OnBeginRoundDisableMovement(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIOTurnBasedStatus();
            var tbStatus = dispIo.tbStatus;
            if (tbStatus != null)
            {
                tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
            }
        }


        [DispTypes(DispatcherType.GetMoveSpeed)]
        [TempleDllLocation(0x100c7040)]
        public static void sub_100C7040(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoMoveSpeed();
            if (!evt.objHndCaller.HasCondition(SpellEffects.SpellControlPlantsDisentangle))
            {
                dispIo.factor = dispIo.factor * 0.5F;
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100d08a0)]
        public static void FrogTongueSwallowedOnAdd(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
            {
                return;
            }

            using var listResult = ObjList.ListVicinity(evt.objHndCaller, ObjectListFilter.OLC_NPC);

            // I believe this is done to prevent NPCs from attacking the player within the frog...
            foreach (var npc in listResult)
            {
                if (!GameSystems.Critter.IsFriendly(npc, evt.objHndCaller) && npc != spellPkt.caster)
                {
                    GameSystems.AI.AddToAllyList(npc, evt.objHndCaller);
                }
            }
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ccb10)]
        public static void BeginSpellConsecrate(in DispatcherCallbackArgs evt)
        {
            SpellEntry a2;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
                var radiusInches = a2.radiusTarget * locXY.INCH_PER_FEET;
                spellPkt.aoeObj = evt.objHndCaller;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 2, 3, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_consecrate(): unable to save new spell_packet");
    }
*/
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100d2e30)]
        public static void sub_100D2E30(in DispatcherCallbackArgs evt)
        {
            evt.GetConditionArg3();
        }


        [DispTypes(DispatcherType.EffectTooltip)]
        [TempleDllLocation(0x100c3e70)]
        public static void EffectTooltipBlindnessDeafness(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            SpellPacketBody spellPkt;

            var dispIo = evt.GetDispIoEffectTooltip();
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                var v3 = spellPkt.duration;
                var v4 = spellPkt.durationRemaining;
                var v5 = GameSystems.D20.Combat.GetCombatMesLine(0xAF);
                var v6 = GameSystems.D20.Combat.GetCombatMesLine(data2);
                var extraString = String.Format("{0} {1}: {2}/{3}", v6, v5, v4, v3);
                dispIo.bdb.AddEntry(data1, extraString, spellPkt.spellEnum);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100d0280)]
        public static void SummonSwarmBeginSpell(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            evt.SetConditionArg3(0);
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                if (!spellPkt.caster.AddCondition("sp-Concentrating", condArg1, 0, 0))
                {
                    Logger.Info(
                        "d20_mods_spells.c / _begin_spell_summon_swarm(): unable to add condition to spell_caster");
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cd6d0)]
        public static void Condition_sp_False_Life_Init(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                var v2 = spellPkt.casterLevel;
                if (spellPkt.casterLevel >= 10)
                {
                    v2 = 10;
                }

                var v3 = new Dice(1, 10, v2).Roll();
                var prefix = String.Format("[{0}] ", v3);
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20005, TextFloaterColor.White, prefix: prefix);
                Logger.Info("d20_mods_spells.c / _begin_spell_false_life(): gained {0} temporary hit points", v3);
                var condArg2 = evt.GetConditionArg2();
                var v7 = evt.GetConditionArg1();
                if (!evt.objHndCaller.AddCondition("Temporary_Hit_Points", v7, condArg2, v3))
                {
                    Logger.Info("d20_mods_spells.c / _begin_spell_false_life(): unable to add condition");
                }
            }
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100c9320)]
        public static void SavingThrowPenalty_sp_Prayer_Callback(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoSavingThrow();

            var condArg3 = evt.GetConditionArg3();
            var v2 = data;
            var v3 = condArg3;
            if (v3 != -3)
            {
                v2 = -v2;
            }

            dispIo.bonlist.AddBonus(v2, 14, 151);
        }


        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100cb330)]
        public static void vampiric_touch_taking_damage(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoDamage();
            var v2 = dispIo.damage.finalDamage;
            DamagePacket v3 = dispIo.damage;
            var condArg3 = evt.GetConditionArg3();
            var v5 = condArg3;
            if (v2 > 0)
            {
                Logger.Info("took {0} damage, temp_hp = {1}", v2, condArg3);
                var v6 = v5 - v2;
                if (v5 - v2 <= 0)
                {
                    v6 = 0;
                }

                Logger.Info("({0}) temp_hp left", v6);
                evt.SetConditionArg3(v6);
                if (v5 - v2 > 0)
                {
                    v3.AddDamageBonus(-v2, 0, 154);
                    v3.finalDamage = 0;
                    Logger.Info(", absorbed {0} points of damage", v2);
                    var extraString = String.Format("[{0}] ", v2);
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, D20CombatMessage.Points_of_Damage_Absorbed,
                        prefix: extraString);
                }
                else
                {
                    var v7 = v2 - v5;
                    Logger.Info(", taking modified damage {0}", v7);
                    var extraString = String.Format("[{0}] ", v5);
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, D20CombatMessage.Points_of_Damage_Absorbed,
                        prefix: extraString);
                    v3.AddDamageBonus(-v5, 0, 154);
                    v3.finalDamage = v7;
                }
            }
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d3850)]
        public static void Condition__36__consecrate_sthg(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    SpellPktTriggerAoeHitScript(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        spellPkt.AddTarget(dispIo.tgt, null, true);
                        if (GameSystems.Critter.IsCategory(dispIo.tgt, MonsterCategory.undead))
                        {
                        }
                        else
                        {
                        }

                        dispIo.tgt.AddCondition("sp-Consecrate Hit", spellPkt.spellId, spellPkt.durationRemaining,
                            dispIo.evtId);
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        if (!spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info("d20_mods_spells.c / _consecrate_hit_trigger(): cannot remove target");
                            return;
                        }

                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                            spellPkt.spellId, 0);
                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.TurnBasedStatusInit)]
        [TempleDllLocation(0x100ed0b0)]
        public static void StinkingCloudNausea_TurnbasedInit(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIOTurnBasedStatus();
            var tbStatus = dispIo.tbStatus;
            if (tbStatus.hourglassState > HourglassState.MOVE)
            {
                tbStatus.hourglassState = HourglassState.MOVE;
            }
        }


        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100cad50)]
        public static void sub_100CAD50(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            SpellPacketBody spellPkt;

            var dispIo = evt.GetDispIoDamage();
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                var v3 = spellPkt.casterLevel / 3;
                if (spellPkt.casterLevel / 3 >= 15)
                {
                    v3 = 15;
                }

                dispIo.damage.AddDamageBonus(v3, 0, data2);
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100c7330)]
        public static void sub_100C7330(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            dispIo.bonlist.AddBonus(data1, 17, data2);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cf340)]
        public static void BeginSpellRepelVermin(in DispatcherCallbackArgs evt)
        {
            SpellEntry a2;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
                var radiusInches = a2.radiusTarget * locXY.INCH_PER_FEET;
                spellPkt.aoeObj = evt.objHndCaller;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 30, 31, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_repel_vermin(): unable to save new spell_packet");
    }
*/
            }
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d45e0)]
        public static void ObjEventAoEEntangle(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    SpellPktTriggerAoeHitScript(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        if (GameSystems.D20.Combat.SavingThrowSpell(dispIo.tgt, spellPkt.caster, spellPkt.dc,
                            SavingThrowType.Reflex, 0, spellPkt.spellId))
                        {
                            GameSystems.Spell.FloatSpellLine(dispIo.tgt, 30001, TextFloaterColor.White);
                            var v8 = dispIo.tgt;
                            var v10 = GameSystems.ParticleSys.CreateAtObj("sp-Entangle", v8);
                            spellPkt.AddTarget(dispIo.tgt, v10, true);
                        }
                        else
                        {
                            GameSystems.Spell.FloatSpellLine(dispIo.tgt, 30002, TextFloaterColor.White);
                            var v5 = dispIo.tgt;
                            var v7 = GameSystems.ParticleSys.CreateAtObj("sp-Entangle", v5);
                            spellPkt.AddTarget(dispIo.tgt, v7, true);
                        }

                        dispIo.tgt.AddCondition("sp-Entangle On", spellPkt.spellId, spellPkt.durationRemaining,
                            dispIo.evtId);
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        if (!spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info("d20_mods_spells.c / _entangle_hit_trigger(): cannot remove target");
                            return;
                        }

                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.TakingDamage)]
        [TempleDllLocation(0x100ca3f0)]
        public static void sub_100CA3F0(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoDamage();
            var v4 = evt.objHndCaller.GetInt32(obj_f.critter_alignment_choice) != 0
                ? D20AttackPower.HOLY
                : D20AttackPower.UNHOLY;
            var v2 = evt.objHndCaller.GetStat(Stat.level_cleric);
            var v3 = 3;
            if (v2 >= 12)
            {
                v3 = 6;
                if (v2 >= 15)
                {
                    v3 = 9;
                }
            }

            dispIo.damage.AddPhysicalDR(v3, v4, 0x68);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cd4b0)]
        public static void sub_100CD4B0(in DispatcherCallbackArgs evt)
        {
            int condArg3;
            int condArg2;
            int v3;
            GameObjectBody v4;

            evt.SetConditionArg4(5);
            switch (evt.GetConditionArg2())
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
        public static void ApplyEnchantedAttackPower(in DispatcherCallbackArgs evt, int data1, int spellType)
        {
            var dispIo = evt.GetDispIoDamage();
            switch (spellType)
            {
                case 209:
                case 210:
                    if (IsUsingNaturalAttacks(dispIo.attackPacket))
                    {
                        dispIo.damage.AddAttackPower(D20AttackPower.MAGIC);
                    }

                    break;
                default:
                    dispIo.damage.AddAttackPower(D20AttackPower.MAGIC);
                    break;
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cea00)]
        public static void BeginSpellMinorGlobeOfInvulnerability(in DispatcherCallbackArgs evt)
        {
            SpellEntry a2;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
                var radiusInches = a2.radiusTarget * locXY.INCH_PER_FEET;
                spellPkt.aoeObj = evt.objHndCaller;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 22, 23, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_minor_globe_of_invulnerability(): unable to save new spell_packet");
    }
*/
            }
        }


        [DispTypes(DispatcherType.Tooltip)]
        [TempleDllLocation(0x100c3630)]
        public static void Tooltip2Callback(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            int v2;
            int condArg1;
            int condArg3;
            int v5;
            SpellPacketBody spellPkt;

            var dispIo = evt.GetDispIoTooltip();
            switch (data2)
            {
                case 0xBD:
                    v2 = 10;
                    condArg1 = evt.GetConditionArg1();
                    if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
                    {
                        if (spellPkt.casterLevel <= 10)
                        {
                            if (spellPkt.casterLevel > 6)
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

            var meslineKey = data1;
            var meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
            dispIo.Append($"{meslineValue}{v2}");
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cfa40)]
        public static void SleetStormBeginSpell(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            evt.SetConditionArg4(0);
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20040, TextFloaterColor.White);
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20017, TextFloaterColor.Red);
            if (!evt.objHndCaller.AddCondition("Invisible", condArg1, condArg2, condArg3))
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_sleet_storm(): unable to add condition");
            }
        }


        [DispTypes(DispatcherType.GetAC, DispatcherType.AcModifyByAttacker)]
        [TempleDllLocation(0x100c4440)]
        public static void SpellArmorBonus(in DispatcherCallbackArgs evt, int bonusType, int bonusMesLine)
        {
            var dispIo = evt.GetDispIoAttackBonus();

            var condArg1 = evt.GetConditionArg1();
            var condArg3 = evt.GetConditionArg3();

            if (!GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellEnum))
            {
                Logger.Info(
                    "d20_mods_spells.c / _spell_armor_bonus(): unable to retrieve spell_packet for spell_id=( {0} )",
                    condArg1);
                return;
            }

            switch (bonusMesLine)
            {
                // Slow
                case 173:
                    condArg3 = -condArg3;
                    dispIo.bonlist.AddBonus(condArg3, bonusType, bonusMesLine);
                    return;
                // Magic Circles
                case 205:
                case 206:
                // Protection from Alignment
                case 207:
                    if (DoesAlignmentProtectionApply(dispIo.attackPacket.attacker, spellEnum.spellEnum))
                    {
                        dispIo.bonlist.AddBonus(condArg3, bonusType, bonusMesLine);
                    }

                    return;
                // Magic Vestment
                case 213:
                    var condArg4 = evt.GetConditionArg4();
                    dispIo.bonlist.AddBonus(condArg4, bonusType, bonusMesLine);
                    return;
                // Protection from Monster
                case 264:
                    if (DoesMonsterTypeProtectionApply(dispIo.attackPacket.attacker, spellEnum.spellEnum))
                    {
                        condArg3 = -condArg3;
                        dispIo.bonlist.AddBonus(condArg3, bonusType, bonusMesLine);
                    }

                    break;

                default:
                    dispIo.bonlist.AddBonus(condArg3, bonusType, bonusMesLine);
                    return;
            }
        }


        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100d2b00)]
        public static void PotionOfGlibnessSkillLevel(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoObjBonus();
            dispIo.bonOut.AddBonus(30, 0, 113);
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d4c90)]
        public static void GreaseAoeEvent(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    SpellPktTriggerAoeHitScript(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        var v5 = dispIo.tgt;
                        var v7 = GameSystems.ParticleSys.CreateAtObj("sp-Grease-Hit", v5);
                        spellPkt.AddTarget(dispIo.tgt, v7, true);
                        dispIo.tgt.AddCondition("sp-Grease Hit", spellPkt.spellId, spellPkt.durationRemaining,
                            dispIo.evtId);
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        if (!spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info("d20_mods_spells.c / _grease_hit_trigger(): cannot remove target");
                            return;
                        }

                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                            spellPkt.spellId, 0);
                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.GetMoveSpeed)]
        [TempleDllLocation(0x100cabe0)]
        public static void sub_100CABE0(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            var dispIo = evt.GetDispIoMoveSpeed();
            dispIo.factor = dispIo.factor * 0.33F;
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d4050)]
        public static void ObjEventAoEDesecrate(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    SpellPktTriggerAoeHitScript(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        spellPkt.AddTarget(dispIo.tgt, null, true);
                        if (GameSystems.Critter.IsCategory(dispIo.tgt, MonsterCategory.undead))
                        {
                        }
                        else
                        {
                        }

                        dispIo.tgt.AddCondition("sp-Desecrate Hit", spellPkt.spellId, spellPkt.durationRemaining,
                            dispIo.evtId);
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        if (!spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info("d20_mods_spells.c / _desecrate_hit_trigger(): cannot remove target");
                            return;
                        }

                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                            spellPkt.spellId, 0);
                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.ToHitBonus2)]
        [TempleDllLocation(0x100d2b70)]
        public static void sub_100D2B70(in DispatcherCallbackArgs evt, int data)
        {
            var v1 = data;
            var dispIo = evt.GetDispIoAttackBonus();
            dispIo.bonlist.AddBonus(v1, 34, 113);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ccd80)]
        public static void sub_100CCD80(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            if (!evt.objHndCaller.AddCondition("Charmed", condArg1, condArg2, condArg3))
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_control_plants_charmed(): unable to add condition");
            }

            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20018, TextFloaterColor.Red);
        }

        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100d2b40)]
        public static void PotionOfHeroismSkillBonus(in DispatcherCallbackArgs evt, int data)
        {
            var v1 = data;
            var dispIo = evt.GetDispIoObjBonus();
            dispIo.bonOut.AddBonus(v1, 34, 113);
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100d01d0)]
        public static void SuggestionOnAdd(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            if (!evt.objHndCaller.AddCondition("Charmed", condArg1, condArg2, condArg3))
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_suggestion(): unable to add condition");
            }

            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20018, TextFloaterColor.Red);
            var v6 = GameSystems.Party.GetConsciousLeader();
            GameSystems.Critter.AddFollower(evt.objHndCaller, v6, true, false);
            GameUiBridge.UpdatePartyUi();
            evt.SetConditionArg3(1);
        }

        private static bool DoesAlignmentProtectionApply(GameObjectBody attacker, int spellEnum)
        {
            switch (spellEnum)
            {
                case WellKnownSpells.MagicCircleAgainstChaos:
                case WellKnownSpells.ProtectionFromChaos:
                    return attacker.HasChaoticAlignment();
                case WellKnownSpells.MagicCircleAgainstEvil:
                case WellKnownSpells.ProtectionFromEvil:
                    return attacker.HasEvilAlignment();
                case WellKnownSpells.MagicCircleAgainstGood:
                case WellKnownSpells.ProtectionFromGood:
                    return attacker.HasGoodAlignment();
                case WellKnownSpells.MagicCircleAgainstLaw:
                case WellKnownSpells.ProtectionFromLaw:
                    return attacker.HasLawfulAlignment();
                default:
                    Logger.Warn("Unknown protection from alignment spell: {0}", spellEnum);
                    return false;
            }
        }

        private static bool DoesMonsterTypeProtectionApply(GameObjectBody attacker, int spellEnum)
        {
            switch (spellEnum)
            {
                case WellKnownSpells.PotionOfProtectionFromOutsiders:
                    return GameSystems.Critter.IsCategory(attacker, MonsterCategory.outsider);
                case WellKnownSpells.PotionOfProtectionFromElementals:
                    return GameSystems.Critter.IsCategory(attacker, MonsterCategory.elemental);
                case WellKnownSpells.PotionOfProtectionFromEarth:
                    return GameSystems.Critter.IsCategorySubtype(attacker, MonsterSubtype.earth);
                case WellKnownSpells.PotionOfProtectionFromUndead:
                    return GameSystems.Critter.IsCategory(attacker, MonsterCategory.undead);
                default:
                    Logger.Warn("Unknown protection from monster type spell: {0}", spellEnum);
                    return false;
            }
        }

        [DispTypes(DispatcherType.SaveThrowSpellResistanceBonus, DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100c5490)]
        public static void SavingThrowSpellResistanceBonusCallback(in DispatcherCallbackArgs evt, int amount,
            int bonusMesLine)
        {
            var condArg1 = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
            {
                Logger.Info(
                    "d20_mods_spells.c / _spell_resistance_saving_bonus(): unable to retrieve spell_packet for spell_id=( {0} )",
                    condArg1);
                return;
            }

            var dispIo = evt.GetDispIoSavingThrow();
            switch (bonusMesLine)
            {
                // Magic Circle
                case 205:
                case 206:
                // Protection From Alignment
                case 207:
                    if (DoesAlignmentProtectionApply(dispIo.obj, spellPkt.spellEnum))
                    {
                        dispIo.bonlist.AddBonus(amount, 15, bonusMesLine);
                    }

                    return;
                // Protection from Monster Type
                case 264:
                    if (DoesMonsterTypeProtectionApply(dispIo.obj, spellPkt.spellEnum))
                    {
                        dispIo.bonlist.AddBonus(amount, 15, bonusMesLine);
                    }

                    break;
                // Spell Resistance
                case 199:
                // Animal Growth
                case 274:
                    dispIo.bonlist.AddBonus(amount, 13, bonusMesLine);
                    break;
                default:
                    dispIo.bonlist.AddBonus(amount, 13, bonusMesLine);
                    break;
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100d64d0)]
        public static void SpikeGrowthDamageHealingReceived(in DispatcherCallbackArgs evt)
        {
            SpellPacketBody spellPkt;

            var dispIo = evt.GetDispIoD20Signal();
            if (evt.dispKey != D20DispatcherKey.SIG_Spell_Cast)
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
                {
                    var bonlist = BonusList.Create();
                    var healName = GameSystems.Skill.GetSkillName(SkillId.heal);
                    if (GameSystems.Spell.DispelRoll(evt.objHndCaller, bonlist, 0, spellPkt.dc, healName) < 0)
                    {
                        return;
                    }
                }
                else
                {
                    var bonlist = BonusList.Create();
                    var healName = GameSystems.Skill.GetSkillName(SkillId.heal);
                    if (GameSystems.Spell.DispelRoll(evt.objHndCaller, bonlist, 0, 15, healName) < 0)
                    {
                        return;
                    }
                }

                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                return;
            }

            if (!GameSystems.Spell.TryGetActiveSpell(dispIo.data1, out spellPkt))
            {
                return;
            }

            switch (spellPkt.spellEnum)
            {
                case WellKnownSpells.Heal:
                case WellKnownSpells.HealingCircle:
                case WellKnownSpells.CureCriticalWounds:
                case WellKnownSpells.CureLightWounds:
                case WellKnownSpells.CureMinorWounds:
                case WellKnownSpells.CureModerateWounds:
                case WellKnownSpells.CureSeriousWounds:
                    SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                    break;
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cd390)]
        [TemplePlusLocation("spell_condition.cpp:92")]
        public static void EmotionBeginSpell(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            // Good Hope / Crushing Despair fix for concentration check requirement
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                // Emotion: Fear
                if (data2 == 82)
                {
                    GameSystems.AI.FleeFrom(evt.objHndCaller, spellPkt.caster);
                }

                // TemplePlus: removed the part of adding sp-Concentrating - was 3.0ed holdover
            }
        }

        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d6ff0)]
        public static void WindWallAoeEvent(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    SpellPktTriggerAoeHitScript(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        spellPkt.AddTarget(dispIo.tgt, null, true);
                        dispIo.tgt.AddCondition("sp-Wind Wall Hit", spellPkt.spellId, spellPkt.durationRemaining,
                            dispIo.evtId);
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        if (!spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info("d20_mods_spells.c / _wind_wall_hit_trigger(): cannot remove target");
                            return;
                        }

                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                            spellPkt.spellId, 0);
                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c6350)]
        public static void BlinkSpellFailure(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if (dispIo.return_val != 1)
            {
                var v8 = Dice.D100.Roll();
                if (v8 >= 20)
                {
                    var v11 = GameSystems.RollHistory.AddPercentageCheck(evt.objHndCaller, null, 20, 111, v8, 62, 192);
                    GameSystems.RollHistory.CreateRollHistoryString(v11);
                }
                else
                {
                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(0x24, evt.objHndCaller, null);
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 112);
                    var v10 = GameSystems.RollHistory.AddPercentageCheck(evt.objHndCaller, null, 20, 111, v8, 112, 192);
                    GameSystems.RollHistory.CreateRollHistoryString(v10);
                    dispIo.return_val = 1;
                }
            }
        }

        private static bool TryGetDamageTypeForResistanceSpell(int spellType, out DamageType damageType)
        {
            switch (spellType)
            {
                case 1:
                    damageType = DamageType.Acid;
                    return true;
                case 3:
                    damageType = DamageType.Cold;
                    return true;
                case 6:
                    damageType = DamageType.Electricity;
                    return true;
                case 9:
                    damageType = DamageType.Fire;
                    return true;
                case 16:
                    damageType = DamageType.Sonic;
                    return true;
                default:
                    damageType = default;
                    return false;
            }
        }

        [DispTypes(DispatcherType.TakingDamage)]
        [TempleDllLocation(0x100c9aa0)]
        public static void ResistElementsDamageResistance(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            var spellType = evt.GetConditionArg3();

            if (!TryGetDamageTypeForResistanceSpell(spellType, out var damType))
            {
                Logger.Warn("Elemental Resistance Spell has invalid type: {0}", spellType);
                return;
            }

            var damage = dispIo.damage;
            if (!damage.HasDamageOfType(damType))
            {
                return;
            }

            var remainingAbsorption = evt.GetConditionArg4();
            if (remainingAbsorption > 0)
            {
                var spellId = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
                {
                    if (spellPkt.casterLevel <= 10)
                    {
                        if (spellPkt.casterLevel > 6)
                        {
                            remainingAbsorption = 20;
                        }
                    }
                    else
                    {
                        remainingAbsorption = 30;
                    }
                }
                else
                {
                    remainingAbsorption = 10;
                }

                var totalDamage = dispIo.damage.GetOverallDamageByType();
                if (totalDamage > 0)
                {
                    int drLeft;
                    if (totalDamage <= remainingAbsorption)
                    {
                        dispIo.damage.AddDR(totalDamage, damType, 104);
                        drLeft = remainingAbsorption - totalDamage;
                    }
                    else
                    {
                        dispIo.damage.AddDR(remainingAbsorption, damType, 104);
                        drLeft = 0;
                    }

                    var newFinalDamage = dispIo.damage.GetOverallDamageByType();
                    dispIo.damage.finalDamage = newFinalDamage;

                    Logger.Info("absorbed {0} points of [{1}] damage, DR points left: {2}",
                        totalDamage - newFinalDamage, damType, drLeft);
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ce6d0)]
        public static void sub_100CE6D0(in DispatcherCallbackArgs evt)
        {
            bool v2;
            string v3;
            SpellPacketBody spellPkt;

            if (evt.objHndCaller.HasCondition(SpellEffects.SpellSummoned))
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt) &&
                    !D20ModSpells.CheckSpellResistance(evt.objHndCaller, spellPkt))
                {
                    switch (spellPkt.spellEnum)
                    {
                        case WellKnownSpells.MagicCircleAgainstChaos:
                            v2 = !(evt.objHndCaller.HasChaoticAlignment());
                            goto LABEL_9;
                        case WellKnownSpells.MagicCircleAgainstEvil:
                            v2 = !(evt.objHndCaller.HasEvilAlignment());
                            goto LABEL_9;
                        case WellKnownSpells.MagicCircleAgainstGood:
                            v2 = !(evt.objHndCaller.HasGoodAlignment());
                            goto LABEL_9;
                        case WellKnownSpells.MagicCircleAgainstLaw:
                            v2 = !(evt.objHndCaller.HasLawfulAlignment());
                            LABEL_9:
                            if (!v2)
                            {
                                GameSystems.D20.Actions.curSeqGetTurnBasedStatus().hourglassState = 0;
                            }

                            break;
                        default:
                            v3 = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
                            Logger.Info(
                                "d20_mods_spells.c / _begin_spell_magic_circle_outward(): invalid spell=( {0} )", v3);
                            break;
                    }
                }
            }
        }


        [DispTypes(DispatcherType.GetMoveSpeedBase)]
        [TempleDllLocation(0x100cb700)]
        [TemplePlusLocation("spell_condition.cpp:251")]
        public static void WebOnSpeedNull(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoMoveSpeed();
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement))
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
        public static void WebHit(in DispatcherCallbackArgs evt)
        {
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20028, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.SaveThrowLevel)]
        [TempleDllLocation(0x100c5380)]
        public static void SavingThrowEmotionModifierCallback(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            switch (data2)
            {
                case 0x103:
                    if (!evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions))
                    {
                        dispIo.bonlist.AddBonus(-data1, 13, data2);
                    }

                    break;
                case 0x104:
                case 0x12A:
                case 0x12B:
                    if (!evt.objHndCaller.HasCondition(SpellEffects.SpellCalmEmotions))
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
        public static void sub_100C4850(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var v2 = dispIo;
            switch (data2)
            {
                case 0xD2:
                    if ((dispIo.attackPacket.weaponUsed == null))
                    {
                        var v3 = data2;
                        var condArg3 = evt.GetConditionArg3();
                        v2.bonlist.AddBonus(condArg3, 12, v3);
                    }

                    break;
                case 0xD4:
                    if (dispIo.attackPacket.weaponUsed != null || (evt.objHndCaller.GetStat(Stat.level_monk)) != 0)
                    {
                        var v5 = data2;
                        var condArg4 = evt.GetConditionArg4();
                        v2.bonlist.AddBonus(condArg4, 12, v5);
                    }

                    break;
                case 0xD1:
                    if ((dispIo.attackPacket.weaponUsed == null))
                    {
                        goto LABEL_8;
                    }

                    break;
                default:
                    LABEL_8:
                    dispIo.bonlist.AddBonus(data1, 12, data2);
                    break;
                case 0xD0:
                    if (dispIo.attackPacket.weaponUsed != null || (evt.objHndCaller.GetStat(Stat.level_monk)) != 0)
                    {
                        v2.bonlist.AddBonus(data1, 12, data2);
                    }

                    break;
            }
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d53b0)]
        public static void d20_mods_spells__globe_of_inv_hit(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    SpellPktTriggerAoeHitScript(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        spellPkt.AddTarget(dispIo.tgt, null, true);
                        dispIo.tgt.AddCondition("sp-Minor Globe of Invulnerability Hit", spellPkt.spellId,
                            spellPkt.durationRemaining, dispIo.evtId);
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        if (!spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info(
                                "d20_mods_spells.c / _minor_globe_of_invulnerability_hit_trigger(): cannot remove target");
                            return;
                        }

                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                            spellPkt.spellId, 0);
                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100d31f0)]
        public static void ConcentratingOnDamage2(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoDamage();
            if (dispIo.attackPacket.d20ActnType != D20ActionType.CAST_SPELL)
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt)
                    && spellPkt.spellEnum != WellKnownSpells.MeldIntoStone
                    && !GameSystems.Skill.SkillRoll(evt.objHndCaller, SkillId.concentration,
                        dispIo.damage.finalDamage + spellPkt.spellKnownSlotLevel + 10, out _,
                        SkillCheckFlags.UnderDuress))
                {
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 54);
                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(32, evt.objHndCaller, null);

                    var removeEvt = new DispatcherCallbackArgs(evt.subDispNode, evt.objHndCaller,
                        DispatcherType.D20Signal, D20DispatcherKey.SIG_Remove_Concentration, null);
                    SpellEffects.Spell_remove_mod(removeEvt, 0);
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cba10)]
        public static void spSummonedOnAdd(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
            {
                if (GameSystems.Feat.HasFeat(spellPkt.caster, FeatId.AUGMENT_SUMMONING))
                {
                    evt.objHndCaller.AddCondition(StatusEffects.AugmentSummoningEnhancement);
                }

                var angleRadian = Angles.RotationTo(evt.objHndCaller, spellPkt.caster);
                GameSystems.MapObject.SetRotation(evt.objHndCaller, angleRadian);
                GameSystems.Anim.Interrupt(evt.objHndCaller, AnimGoalPriority.AGP_HIGHEST);
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100dda90)]
        public static void SleepHpChanged(in DispatcherCallbackArgs evt)
        {
            var v1 = evt.objHndCaller.GetStat(Stat.hp_current);
            var v7 = evt.objHndCaller.GetInt32(obj_f.critter_subdual_damage);
            var dispIo = evt.GetDispIoD20Signal();
            var v3 = dispIo.data2;
            if (v3 <= 0)
            {
                if (v3 >= 0)
                {
                    var v4 = dispIo.data1;
                    return;
                }

                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White);
                SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                if (v1 < 0)
                {
                    if (!GameSystems.Feat.HasFeat(evt.objHndCaller, FeatId.DIEHARD))
                    {
                        evt.objHndCaller.AddCondition(StatusEffects.Dying);
                        return;
                    }

                    evt.objHndCaller.AddCondition(StatusEffects.Disabled);
                    return;
                }

                if (v1 > 0 && v7 >= v1)
                {
                    evt.objHndCaller.AddCondition(StatusEffects.Unconscious);
                    return;
                }

                if ((v1) == 0)
                {
                    evt.objHndCaller.AddCondition(StatusEffects.Disabled);
                    return;
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cfb10)]
        public static void BeginSpellSoftenEarthAndStone(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out var a2);
                var radiusInches = a2.radiusTarget * locXY.INCH_PER_FEET;
                spellPkt.aoeObj = evt.objHndCaller;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 36, 37, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100d3bc0)]
        public static void ControlPlantsEntagleBreakFree(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoD20Signal();
            if (dispIo.data1 == evt.GetConditionArg1() && (dispIo.data2) == 0)
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    var bonList = BonusList.Create();
                    var strengthBonus = evt.objHndCaller.GetStat(Stat.str_mod);
                    bonList.AddBonus(strengthBonus, 0, 103);
                    var v5 = GameSystems.Stat.GetStatName(0);
                    if (GameSystems.Spell.DispelRoll(evt.objHndCaller, bonList, 0, 20, v5) < 0)
                    {
                        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20027, TextFloaterColor.Red);
                    }
                    else
                    {
                        GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 21003, TextFloaterColor.White);

                        if (spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info(
                                "d20_mods_spells.c / _control_plants_entangled_break_free_check(): cannot remove target");
                            return;
                        }

                        SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                        spellPkt.AddTarget(evt.objHndCaller, null, true);
                        var condArg3 = evt.GetConditionArg3();
                        evt.objHndCaller.AddCondition("sp-Control Plants Entangle Pre", spellPkt.spellId,
                            spellPkt.durationRemaining, condArg3);
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAddPre)]
        [TempleDllLocation(0x100dc0a0)]
        public static void SpellRemovedBy(in DispatcherCallbackArgs evt, ConditionSpec data)
        {
            if (evt.GetDispIoCondStruct().condStruct == (ConditionSpec) data)
            {
                SpellEffects.Spell_remove_spell(evt.WithoutIO, 0, 0);
            }

            CommonConditionCallbacks.CondOverrideBy(in evt, data);
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d5780)]
        public static void sub_100D5780(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    SpellPktTriggerAoeHitScript(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        var v5 = dispIo.tgt;
                        var v7 = GameSystems.ParticleSys.CreateAtObj("sp-Obscuring Mist-hit", v5);
                        spellPkt.AddTarget(dispIo.tgt, v7, true);
                        dispIo.tgt.AddCondition("sp-Obscuring Mist Hit", spellPkt.spellId, spellPkt.durationRemaining,
                            dispIo.evtId);
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        if (!spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info("d20_mods_spells.c / _obscuring_mist_hit_trigger(): cannot remove target");
                            return;
                        }

                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                            spellPkt.spellId, 0);
                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100d2bd0)]
        public static void PotionOfProtectionFromEnergyDamageCallback(in DispatcherCallbackArgs evt)
        {
            var remainingAbsorption = evt.GetConditionArg4();
            var damageType = (DamageType) evt.GetConditionArg3();
            var dispIo = evt.GetDispIoDamage();
            var damage = dispIo.damage;
            var overallDamage = damage.GetOverallDamageByType(damageType);
            if (overallDamage > 0 && remainingAbsorption > 0)
            {
                int damageReduced;
                if (remainingAbsorption < overallDamage)
                {
                    damageReduced = remainingAbsorption;
                    remainingAbsorption = 0;
                }
                else
                {
                    damageReduced = overallDamage;
                    remainingAbsorption -= overallDamage;
                }

                Logger.Info("({0}) damage reduced", damageReduced);
                evt.SetConditionArg4(remainingAbsorption);
                damage.AddDR(damageReduced, damageType, 124);
                if (remainingAbsorption <= 0)
                {
                    CommonConditionCallbacks.conditionRemoveCallback(in evt);
                }
            }
        }

        [DispTypes(DispatcherType.GetMoveSpeedBase)]
        [TempleDllLocation(0x100ca8c0)]
        public static void solidFogMoveRestriction(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoMoveSpeed();
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Critter_Has_Freedom_of_Movement))
            {
                dispIo.bonlist.SetOverallCap(1, 5, 0, data2);
                dispIo.bonlist.SetOverallCap(2, 5, 0, data2);
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100dbbd0)]
        public static void SpWeaponKeenOnAdd(in DispatcherCallbackArgs evt)
        {
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Item_Has_Keen_Bonus))
            {
                var condArg1 = evt.GetConditionArg1();
                evt.objHndCaller.AddConditionToItem(ItemEffects.WeaponKeen, 0, 0, 0, 0, condArg1);
            }
        }


        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100cb2e0)]
        public static void treeshapeStatRestriction(in DispatcherCallbackArgs evt, Stat attribute, int data2)
        {
            var dispIo = evt.GetDispIoBonusList();
            var queryAttribute = evt.GetAttributeFromDispatcherKey();
            if (queryAttribute == attribute)
            {
                dispIo.bonlist.SetOverallCap(1, 0, 0, data2);
                dispIo.bonlist.SetOverallCap(2, 0, 0, data2);
            }
        }


        [DispTypes(DispatcherType.SkillLevel)]
        [TempleDllLocation(0x100c61a0)]
        public static void SkillModifier_BestowCurseRolls_Callback(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoObjBonus();
            dispIo.bonOut.AddBonus(-data2, 0, data2);
        }


        [DispTypes(DispatcherType.ToHitBonusBase)]
        [TempleDllLocation(0x100cadd0)]
        public static void SpiritualWeaponBaseAttackBonus(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spell))
            {
                var baseAttackBonus = spell.caster.DispatchToHitBonusBase();
                dispIo.bonlist.AddBonus(baseAttackBonus - 1, 0, data2);
            }
        }


        [DispTypes(DispatcherType.D20Signal)]
        [TempleDllLocation(0x100c35f0)]
        public static void FreedomOfMovementRingDeactivate(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            // TODO: This was changed over vanilla and it's unclear how the item is actually deactivated ?!
            var condArg1 = evt.GetConditionArg1();
            if (dispIo.data1 == condArg1 && dispIo.data2 == 0)
            {
                evt.RemoveThisCondition();
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cce50)]
        public static void sub_100CCE50(in DispatcherCallbackArgs evt)
        {
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20012, TextFloaterColor.Red);
        }


        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100d2ac0)]
        public static void sub_100D2AC0(in DispatcherCallbackArgs evt)
        {
            var condArg2 = evt.GetConditionArg2();
            var dispIo = evt.GetDispIoBonusList();
            dispIo.bonlist.AddBonus(condArg2, 12, 113);
        }


        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100cb150)]
        public static void SuggestionIsAiControlledQuery(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if ((evt.GetConditionArg3()) == 0)
            {
                dispIo.return_val = 1;
                dispIo.data1 = evt.GetConditionArg1();
                dispIo.data2 = 0;
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100dbd00)]
        public static void MagicWeaponOnAdd(in DispatcherCallbackArgs evt)
        {
            if (!GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Item_Has_Enhancement_Bonus))
            {
                var condArg1 = evt.GetConditionArg1();
                evt.objHndCaller.AddConditionToItem(ItemEffects.WeaponEnhancementBonus, 1, 0, 0, 0, condArg1);
            }
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d5560)]
        public static void MindFogAoeEvent(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    SpellPktTriggerAoeHitScript(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        if (GameSystems.D20.Combat.SavingThrowSpell(dispIo.tgt, spellPkt.caster, spellPkt.dc,
                            SavingThrowType.Fortitude, 0, spellPkt.spellId))
                        {
                            GameSystems.Spell.FloatSpellLine(dispIo.tgt, 30001, TextFloaterColor.White);
                        }
                        else
                        {
                            GameSystems.Spell.FloatSpellLine(dispIo.tgt, 30002, TextFloaterColor.White);
                            spellPkt.AddTarget(dispIo.tgt, null, true);
                            dispIo.tgt.AddCondition("sp-Mind Fog Hit", spellPkt.spellId, spellPkt.durationRemaining,
                                dispIo.evtId);
                        }
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        if (!spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info("d20_mods_spells.c / _mind_fog_hit_trigger(): cannot remove target");
                            return;
                        }

                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                            spellPkt.spellId, 0);
                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ce290)]
        public static void SpellInvisibilityBegin(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var condArg2 = evt.GetConditionArg2();
            var condArg3 = evt.GetConditionArg3();
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20017, TextFloaterColor.Red);
            if (!evt.objHndCaller.AddCondition("Invisible", condArg1, condArg2, condArg3))
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_invisibility(): unable to add condition");
            }
        }


        [DispTypes(DispatcherType.ObjectEvent)]
        [TempleDllLocation(0x100d5b60)]
        public static void SilenceObjectEvent(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoObjEvent();
            if (dispIo.evtId == evt.GetConditionArg3())
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    SpellPktTriggerAoeHitScript(spellPkt.spellId);
                    if (D20ModSpells.CheckSpellResistance(dispIo.tgt, spellPkt))
                    {
                        return;
                    }

                    if (evt.dispKey == D20DispatcherKey.OnEnterAoE)
                    {
                        var v5 = GameSystems.D20.Combat.SavingThrowSpell(dispIo.tgt, spellPkt.caster, spellPkt.dc,
                            SavingThrowType.Will, 0, spellPkt.spellId);
                        var v6 = dispIo.tgt;
                        if (v5)
                        {
                            GameSystems.Spell.FloatSpellLine(v6, 30001, TextFloaterColor.White);
                        }
                        else
                        {
                            GameSystems.Spell.FloatSpellLine(v6, 30002, TextFloaterColor.White);
                            var v8 = dispIo.tgt;
                            var v10 = GameSystems.ParticleSys.CreateAtObj("Fizzle", v8);
                            spellPkt.AddTarget(dispIo.tgt, v10, true);
                            dispIo.tgt.AddCondition("sp-Silence Hit", spellPkt.spellId, spellPkt.durationRemaining,
                                dispIo.evtId);
                        }
                    }
                    else if (evt.dispKey == D20DispatcherKey.OnLeaveAoE)
                    {
                        if (!spellPkt.RemoveTarget(evt.objHndCaller))
                        {
                            Logger.Info("d20_mods_spells.c / _silence_hit_trigger(): cannot remove target");
                            return;
                        }

                        GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End,
                            spellPkt.spellId, 0);
                        SpellEffects.Spell_remove_mod(in evt, 0);
                    }

                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                }
            }
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100d0310)]
        public static void TashasHideousLaughterAdd(in DispatcherCallbackArgs evt)
        {
            evt.objHndCaller.AddCondition(StatusEffects.Prone);
            GameSystems.Anim.PushAnimate(evt.objHndCaller, NormalAnimType.Falldown);
        }

        [DispTypes(DispatcherType.TurnBasedStatusInit)]
        [TempleDllLocation(0x100c8b30)]
        public static void MagicCircleTurnBasedStatusInit(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIOTurnBasedStatus();
            if (evt.objHndCaller.HasCondition(SpellEffects.SpellSummoned))
            {
                var condArg1 = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt)
                    && !D20ModSpells.CheckSpellResistance(evt.objHndCaller, spellPkt)
                    && dispIo != null
                    && dispIo.tbStatus != null)
                {
                    if (DoesAlignmentProtectionApply(evt.objHndCaller, spellPkt.spellEnum))
                    {
                        dispIo.tbStatus.hourglassState = 0;
                        dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
                    }
                }
            }
        }


        [DispTypes(DispatcherType.TurnBasedStatusInit)]
        [TempleDllLocation(0x100c6c00)]
        public static void CommandTurnBasedStatusInit(in DispatcherCallbackArgs evt)
        {
            var condArg3 = evt.GetConditionArg3();
            var dispIo = evt.GetDispIOTurnBasedStatus();
            var condArg1 = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
            {
                return;
            }

            switch (condArg3)
            {
                case 2:
                    dispIo.tbStatus.hourglassState = HourglassState.EMPTY;
                    dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
                    evt.objHndCaller.AddCondition(StatusEffects.Prone);
                    GameSystems.Anim.PushAnimate(evt.objHndCaller, NormalAnimType.Falldown);
                    break;
                case 3:
                    GameSystems.AI.FleeProcess(evt.objHndCaller, spellPkt.caster);
                    break;
                case 1:
                case 4:
                    dispIo.tbStatus.hourglassState = HourglassState.EMPTY;
                    dispIo.tbStatus.tbsFlags |= TurnBasedStatusFlags.Moved;
                    break;
                default:
                    return;
            }
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cca00)]
        [TemplePlusLocation("spell_condition.cpp:261")]
        public static void ColorsprayUnconsciousOnAdd(in DispatcherCallbackArgs evt)
        {
            // Fix for Color Spray not knocking critters down
            var condArg1 = evt.GetConditionArg1();
            GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20024, TextFloaterColor.Red);
            if (!GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
            {
                Logger.Info("d20_mods_spells.c / _begin_spell_color_spray_unconscious(): unable to get spell_packet");
                return;
            }

            var duration = new Dice(2, 4).Roll();
            spellPkt.duration = duration;
            spellPkt.durationRemaining = duration;
            evt.SetConditionArg2(duration);

            evt.objHndCaller.AddCondition(StatusEffects.Prone);
            GameSystems.Anim.PushAnimate(evt.objHndCaller, NormalAnimType.Falldown);

            GameSystems.Spell.UpdateSpellPacket(spellPkt);
            GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
        }

        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100ccc30)]
        public static void BeginSpellControlPlants(in DispatcherCallbackArgs evt)
        {
            SpellEntry a2;
            SpellPacketBody spellPkt;

            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out spellPkt))
            {
                GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out a2);
                spellPkt.aoeObj = evt.objHndCaller;
                var radiusInches =
                    (float) (int) GameSystems.Spell.GetSpellRangeExact((SpellRangeType) (-a2.radiusTarget),
                        spellPkt.casterLevel, spellPkt.caster)
                    * locXY.INCH_PER_FEET;
                var v3 = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 4, 5, ObjectListFilter.OLC_CRITTERS,
                    radiusInches);
                evt.SetConditionArg3(v3);
                {
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    spellPkt.AddSpellObject(spellPkt.aoeObj, evt.GetConditionPartSysArg(3));
                    GameSystems.Spell.UpdateSpellPacket(spellPkt);
                    GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
                } /*    else
    {
      Logger.Info("d20_mods_spells.c / _begin_spell_control_plants(): unable to save new spell_packet");
    }
*/
            }
        }


        [DispTypes(DispatcherType.Tooltip)]
        [TempleDllLocation(0x100c6490)]
        public static void CallLightningTooltipCallback(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoTooltip();
            var condArg3 = evt.GetConditionArg3();
            var meslineKey = data1;
            var meslineValue = GameSystems.D20.Combat.GetCombatMesLine(meslineKey);
            dispIo.Append($"{meslineValue} [{condArg3}]");
        }

        [DispTypes(DispatcherType.AbilityScoreLevel)]
        [TempleDllLocation(0x100c74d0)]
        public static void EmotionRageAbilityScore(in DispatcherCallbackArgs evt, Stat attribute, int data2)
        {
            var dispIo = evt.GetDispIoBonusList();
            var queryAttribute = evt.GetAttributeFromDispatcherKey();
            if (queryAttribute == attribute)
            {
                dispIo.bonlist.AddBonus(2, 13, data2);
            }
        }

        [DispTypes(DispatcherType.BeginRound)]
        [TempleDllLocation(0x100dc100)]
        [TemplePlusLocation("condition.cpp:502")]
        public static void SpellModCountdownRemove(in DispatcherCallbackArgs evt, int spellIdentifier)
        {
            var condArg2 = evt.GetConditionArg2();
            var durNew = condArg2 - evt.GetDispIoD20Signal().data1;
            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                Logger.Info(
                    "d20_mods_spells.c / countdown_remove(): err.... why are we counting a spell that no longer exists? spell removed without removing the appropriate conditions?");
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                return;
            }

            if (durNew < 0)
            {
                // Magic fang
                if (spellIdentifier == 209)
                {
                    Spell_remove_mod(in evt, 0);
                    return;
                }

                // Stinking cloud hit
                if (spellIdentifier == 222)
                {
                    if (!spellPkt.RemoveTarget(evt.objHndCaller))
                    {
                        Logger.Debug("SpellModCountdownRemove: Cannot remove target");
                        return;
                    }

                    GameSystems.D20.D20SendSignal(evt.objHndCaller, D20DispatcherKey.SIG_Spell_End, spellPkt.spellId,
                        0);
                    Spell_remove_mod(in evt, 0);
                    return;
                }

                // Frog tongue
                if (spellIdentifier == 240 &&
                    !GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Unconscious))
                {
                    if (!spellPkt.Targets[0].Object.AddCondition("sp-Frog Tongue Swallowed", spellId, 1, 0))
                    {
                        Logger.Info("SpellModCountdownRemove: unable to add condition");
                    }

                    if (!evt.objHndCaller.AddCondition("sp-Frog Tongue Swallowing", spellId, 1, 0))
                    {
                        Logger.Info("SpellModCountdownRemove: unable to add condition");
                    }

                    FrogGrappleController.PlaySwallow(evt.objHndCaller);
                    Spell_remove_mod(in evt, 0);
                    return;
                }

                // else
                GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20000, TextFloaterColor.White); // Spell expired
                // TODO: remove spell has special code for data1 == 108, which ends up being GhoulTouchParalyzed
                SpellEffects.Spell_remove_spell(evt.WithoutIO, spellIdentifier, 0);
                SpellEffects.Spell_remove_mod(evt.WithoutIO, 0);
                return;
            }

            // Stinking cloud hit
            if (spellIdentifier == 222)
            {
                if (evt.GetConditionArg4() == 0)
                {
                    return;
                }
            }
            // Control plants icons
            else if (spellIdentifier == 226)
            {
                if (evt.GetConditionArg3() == 0)
                {
                    return;
                }
            }

            evt.SetConditionArg2(durNew);
            spellPkt.durationRemaining = durNew;
            GameSystems.Spell.UpdateSpellPacket(spellPkt);
            GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);

            switch (spellIdentifier)
            {
                case 48: // Dazed
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 20012, TextFloaterColor.Red);
                    return;
                case 156: // Acid
                    AcidDamage(in evt);
                    return;
                case 203:
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 47); // Sleeping
                    return;
                case 240:
                    if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Unconscious))
                        return;
                    GameSystems.Spell.FloatSpellLine(evt.objHndCaller, 21005, TextFloaterColor.White); // Grappling
                    FrogGrappleController.PlayPull(evt.objHndCaller);
                    return;
                case 244: // Stunned
                    GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(33, evt.objHndCaller, null);
                    GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 89); // Stunned
                    return;
            }
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c79e0)]
        public static void sub_100C79E0(in DispatcherCallbackArgs evt)
        {
            evt.GetDispIoD20Query();
        }


        [DispTypes(DispatcherType.ConditionAdd)]
        [TempleDllLocation(0x100cc360)]
        public static void BeginSpellCastLightning(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            if (GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
            {
                spellPkt.SetTargets(new[] {evt.objHndCaller});
                GameSystems.Spell.UpdateSpellPacket(spellPkt);
                GameSystems.Script.Spells.UpdateSpell(spellPkt.spellId);
            }
        }

        [DispTypes(DispatcherType.DealingDamage)]
        [TempleDllLocation(0x100c6f40)]
        public static void ConsecrateHitUndeadDealingDamage(in DispatcherCallbackArgs evt, int data1, int data2)
        {
            var dispIo = evt.GetDispIoDamage();
            dispIo.damage.AddDamageBonus(-data1, 17, data2);
        }

        [DispTypes(DispatcherType.D20Query)]
        [TempleDllLocation(0x100c4300)]
        public static void sub_100C4300(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoD20Query();
            if (dispIo.data1 == data && dispIo.data2 == 0)
            {
                var spellId = evt.GetConditionArg1();
                if (GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
                {
                    dispIo.return_val = 1;
                    dispIo.obj = spellPkt.caster;
                }
            }
        }


        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100c7c40)]
        public static void FireShieldCounterDamage(in DispatcherCallbackArgs evt, int data)
        {
            var spellId = evt.GetConditionArg1();
            if (!GameSystems.Spell.TryGetActiveSpell(spellId, out var spellPkt))
            {
                return;
            }

            DamageType resistanceType;
            var dispIo = evt.GetDispIoDamage();
            var condArg3 = evt.GetConditionArg3();
            if (condArg3 == 3)
            {
                resistanceType = DamageType.Cold;
            }
            else if (condArg3 == 9)
            {
                resistanceType = DamageType.Fire;
            }
            else
            {
                return;
            }

            var inCloseQuarters = dispIo.attackPacket.weaponUsed == null
                                  || dispIo.attackPacket.weaponUsed.GetInt32(obj_f.weapon_range) <= 5;

            switch (dispIo.attackPacket.d20ActnType)
            {
                case D20ActionType.UNSPECIFIED_ATTACK:
                case D20ActionType.STANDARD_ATTACK:
                case D20ActionType.FULL_ATTACK:
                case D20ActionType.CLEAVE:
                case D20ActionType.ATTACK_OF_OPPORTUNITY:
                case D20ActionType.WHIRLWIND_ATTACK:
                case D20ActionType.TOUCH_ATTACK:
                case D20ActionType.CHARGE:
                case D20ActionType.STAND_UP:
                case D20ActionType.TURN_UNDEAD:
                case D20ActionType.DEATH_TOUCH:
                case D20ActionType.BARDIC_MUSIC:
                case D20ActionType.COUP_DE_GRACE:
                case D20ActionType.BARBARIAN_RAGE:
                case D20ActionType.STUNNING_FIST:
                case D20ActionType.SMITE_EVIL:
                case D20ActionType.TRIP:
                case D20ActionType.REMOVE_DISEASE:
                case D20ActionType.SPELL_CALL_LIGHTNING:
                case D20ActionType.AOO_MOVEMENT:
                case D20ActionType.CLASS_ABILITY_SA:
                case D20ActionType.LAY_ON_HANDS_USE:
                    if (inCloseQuarters)
                    {
                        if (!D20ModSpells.CheckSpellResistance(dispIo.attackPacket.victim, spellPkt))
                        {
                            var dice = Dice.D6.WithModifier(Math.Min(15, spellPkt.casterLevel));
                            GameSystems.D20.Combat.SpellDamageFull(dispIo.attackPacket.attacker,
                                dispIo.attackPacket.victim, dice, resistanceType, D20AttackPower.UNSPECIFIED,
                                D20ActionType.CAST_SPELL, spellId, 0);
                        }
                    }

                    break;
                default:
                    return;
            }
        }


        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100d2ca0)]
        public static void ProtectionFromAlignmentDamageCallback(in DispatcherCallbackArgs evt, int data)
        {
            var dispIo = evt.GetDispIoDamage();
            var attacker = dispIo.attackPacket.attacker;
            if (IsUsingNaturalAttacks(dispIo.attackPacket))
            {
                var condArg1 = evt.GetConditionArg1();
                if (!GameSystems.Spell.TryGetActiveSpell(condArg1, out var spellPkt))
                {
                    Logger.Info(
                        "d20_mods_spells.c / _protection_from_alignment_prevent_damage(): unable to retrieve spell_packet");
                    return;
                }

                if (DoesMonsterTypeProtectionApply(attacker, spellPkt.spellEnum))
                {
                    if (!D20ModSpells.CheckSpellResistance(evt.objHndCaller, spellPkt))
                    {
                        dispIo.damage.AddPhysicalDR(dispIo.damage.finalDamage, D20AttackPower.UNSPECIFIED, 104);
                    }
                }
            }
        }

        private static bool IsUsingNaturalAttacks(AttackPacket attack)
        {
            return attack.GetWeaponUsed() == null;
        }
    }
}