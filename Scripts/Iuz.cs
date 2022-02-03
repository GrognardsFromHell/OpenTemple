
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(172)]
    public class Iuz : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((!GetGlobalFlag(371)))
            {
                // iuz has not talked
                if ((triggerer.FindItemByName(2203) != null))
                {
                    triggerer.BeginDialog(attachee, 1);
                }
                else if ((Utilities.find_npc_near(attachee, 8032) != null))
                {
                    triggerer.BeginDialog(attachee, 100);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 130);
                }

            }

            return SkipDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            // if should_modify_CR( attachee ):
            // modify_CR( attachee, get_av_level() )
            SetGlobalFlag(327, true);
            return RunDefault;
        }
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            Logger.Info("Iuz start combat");
            SetGlobalVar(32, GetGlobalVar(32) + 1);
            if ((GetGlobalFlag(328)))
            {
                // cuthbert has already talked and iuz shouldn't be there
                attachee.RemoveFromInitiative();
                attachee.SetObjectFlag(ObjectFlag.OFF);
                AttachParticles("sp-Magic Circle against Good-END", attachee);
                Sound(4043, 1);
                return SkipDefault;
            }
            else
            {
                // cuthbert has not appeared
                if ((Utilities.find_npc_near(triggerer, 8032) != null))
                {
                    Logger.Info("{0}", ("py00172iuz: Found Hedrack nearby"));
                    // hedrack is near
                    if ((GetGlobalVar(32) >= 4 && !attachee.D20Query(D20DispatcherKey.QUE_Prone)))
                    {
                        Logger.Info("{0}", ("py00172iuz: 4th round or higher and is not prone"));
                        // 4th round of combat or higher and Iuz is not prone
                        var ST_CUTHBERT_PROTO = 14267;
                        var ST_CUTHBERT_NAME = 8043;
                        var cuthbert = Utilities.find_npc_near(attachee, ST_CUTHBERT_NAME);
                        if ((cuthbert == null))
                        {
                            Logger.Info("{0}", ("py00172iuz: Cuthbert not nearby, spawning"));
                            cuthbert = GameSystems.MapObject.CreateObject(
                                ST_CUTHBERT_PROTO,
                                attachee.GetLocation().OffsetTiles(-2, 0)
                            );
                        }

                        AttachParticles("hit-LAW-medium", cuthbert);
                        attachee.TurnTowards(cuthbert);
                        cuthbert.TurnTowards(attachee);
                        Co8.StopCombat(attachee, 0);
                        foreach (var pc in GameSystems.Party.PartyMembers)
                        {
                            if (pc.type == ObjectType.pc)
                            {
                                attachee.AIRemoveFromShitlist(pc);
                            }

                        }

                        var delegatePc = Utilities.GetDelegatePc(attachee, 35);
                        Sound(4134, 1);
                        if ((delegatePc != null))
                        {
                            delegatePc.TurnTowards(cuthbert);
                            attachee.TurnTowards(delegatePc);
                            delegatePc.BeginDialog(cuthbert, 1);
                            DetachScript();
                            return SkipDefault;
                        }

                    }

                }

                var strategy = RandomRange(453, 460);
                if ((strategy == 453))
                {
                    attachee.SetInt(obj_f.critter_strategy, 453);
                }
                else if ((strategy == 454))
                {
                    attachee.SetInt(obj_f.critter_strategy, 454);
                }
                else if ((strategy == 455))
                {
                    attachee.SetInt(obj_f.critter_strategy, 455);
                }
                else if ((strategy == 456))
                {
                    attachee.SetInt(obj_f.critter_strategy, 456);
                }
                else if ((strategy == 457))
                {
                    attachee.SetInt(obj_f.critter_strategy, 457);
                }
                else if ((strategy == 458))
                {
                    attachee.SetInt(obj_f.critter_strategy, 458);
                }
                else if ((strategy == 459))
                {
                    attachee.SetInt(obj_f.critter_strategy, 459);
                }
                else if ((strategy == 460))
                {
                    attachee.SetInt(obj_f.critter_strategy, 460);
                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((!GetGlobalFlag(361)))
            {
                SetGlobalFlag(361, true);
            }

            // game.particles( "mon-iuz", attachee )
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    if (pc.type == ObjectType.pc)
                    {
                        if ((pc.GetPartyMembers().Any(o => o.HasItemByName(2203))))
                        {
                            // party has golden skull
                            if ((PartyLeader.GetStat(Stat.hp_current) >= 1 && !PartyLeader.D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                PartyLeader.TurnTowards(attachee);
                                attachee.TurnTowards(PartyLeader);
                                PartyLeader.BeginDialog(attachee, 1);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(1).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(1).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(1).TurnTowards(attachee);
                                attachee.TurnTowards(GameSystems.Party.GetPartyGroupMemberN(1));
                                GameSystems.Party.GetPartyGroupMemberN(1).BeginDialog(attachee, 1);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(2).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(2).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(2).TurnTowards(attachee);
                                attachee.TurnTowards(GameSystems.Party.GetPartyGroupMemberN(2));
                                GameSystems.Party.GetPartyGroupMemberN(2).BeginDialog(attachee, 1);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(3).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(3).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(3).TurnTowards(attachee);
                                attachee.TurnTowards(GameSystems.Party.GetPartyGroupMemberN(3));
                                GameSystems.Party.GetPartyGroupMemberN(3).BeginDialog(attachee, 1);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(4).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(4).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(4).TurnTowards(attachee);
                                attachee.TurnTowards(GameSystems.Party.GetPartyGroupMemberN(4));
                                GameSystems.Party.GetPartyGroupMemberN(4).BeginDialog(attachee, 1);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(5).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(5).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(5).TurnTowards(attachee);
                                attachee.TurnTowards(GameSystems.Party.GetPartyGroupMemberN(5));
                                GameSystems.Party.GetPartyGroupMemberN(5).BeginDialog(attachee, 1);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(6).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(6).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(6).TurnTowards(attachee);
                                attachee.TurnTowards(GameSystems.Party.GetPartyGroupMemberN(6));
                                GameSystems.Party.GetPartyGroupMemberN(6).BeginDialog(attachee, 1);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(7).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(7).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(7).TurnTowards(attachee);
                                attachee.TurnTowards(GameSystems.Party.GetPartyGroupMemberN(7));
                                GameSystems.Party.GetPartyGroupMemberN(7).BeginDialog(attachee, 1);
                                DetachScript();
                                return SkipDefault;
                            }

                        }
                        else if ((Utilities.find_npc_near(attachee, 8032) != null))
                        {
                            // hedrack is alive and near
                            if ((PartyLeader.GetStat(Stat.hp_current) >= 1 && !PartyLeader.D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                PartyLeader.TurnTowards(attachee);
                                PartyLeader.BeginDialog(attachee, 100);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(1).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(1).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(1).TurnTowards(attachee);
                                GameSystems.Party.GetPartyGroupMemberN(1).BeginDialog(attachee, 100);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(2).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(2).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(2).TurnTowards(attachee);
                                GameSystems.Party.GetPartyGroupMemberN(2).BeginDialog(attachee, 100);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(3).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(3).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(3).TurnTowards(attachee);
                                GameSystems.Party.GetPartyGroupMemberN(3).BeginDialog(attachee, 100);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(4).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(4).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(4).TurnTowards(attachee);
                                GameSystems.Party.GetPartyGroupMemberN(4).BeginDialog(attachee, 100);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(5).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(5).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(5).TurnTowards(attachee);
                                GameSystems.Party.GetPartyGroupMemberN(5).BeginDialog(attachee, 100);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(6).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(6).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(6).TurnTowards(attachee);
                                GameSystems.Party.GetPartyGroupMemberN(6).BeginDialog(attachee, 100);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(7).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(7).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(7).TurnTowards(attachee);
                                GameSystems.Party.GetPartyGroupMemberN(7).BeginDialog(attachee, 100);
                                DetachScript();
                                return SkipDefault;
                            }

                        }
                        else
                        {
                            // hedrack is dead or not near and party does not have golden skull
                            if ((PartyLeader.GetStat(Stat.hp_current) >= 1 && !PartyLeader.D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                PartyLeader.TurnTowards(attachee);
                                attachee.TurnTowards(PartyLeader);
                                PartyLeader.BeginDialog(attachee, 130);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(1).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(1).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(1).TurnTowards(attachee);
                                attachee.TurnTowards(GameSystems.Party.GetPartyGroupMemberN(1));
                                GameSystems.Party.GetPartyGroupMemberN(1).BeginDialog(attachee, 130);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(2).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(2).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(2).TurnTowards(attachee);
                                attachee.TurnTowards(GameSystems.Party.GetPartyGroupMemberN(2));
                                GameSystems.Party.GetPartyGroupMemberN(2).BeginDialog(attachee, 130);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(3).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(3).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(3).TurnTowards(attachee);
                                attachee.TurnTowards(GameSystems.Party.GetPartyGroupMemberN(3));
                                GameSystems.Party.GetPartyGroupMemberN(3).BeginDialog(attachee, 130);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(4).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(4).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(4).TurnTowards(attachee);
                                attachee.TurnTowards(GameSystems.Party.GetPartyGroupMemberN(4));
                                GameSystems.Party.GetPartyGroupMemberN(4).BeginDialog(attachee, 130);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(5).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(5).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(5).TurnTowards(attachee);
                                attachee.TurnTowards(GameSystems.Party.GetPartyGroupMemberN(5));
                                GameSystems.Party.GetPartyGroupMemberN(5).BeginDialog(attachee, 130);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(6).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(6).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(6).TurnTowards(attachee);
                                attachee.TurnTowards(GameSystems.Party.GetPartyGroupMemberN(6));
                                GameSystems.Party.GetPartyGroupMemberN(6).BeginDialog(attachee, 130);
                                DetachScript();
                                return SkipDefault;
                            }
                            else if ((GameSystems.Party.GetPartyGroupMemberN(7).GetStat(Stat.hp_current) >= 1 && !GameSystems.Party.GetPartyGroupMemberN(7).D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                GameSystems.Party.GetPartyGroupMemberN(7).TurnTowards(attachee);
                                attachee.TurnTowards(GameSystems.Party.GetPartyGroupMemberN(7));
                                GameSystems.Party.GetPartyGroupMemberN(7).BeginDialog(attachee, 130);
                                DetachScript();
                                return SkipDefault;
                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool iuz_pc_persuade(GameObject iuz, GameObject pc, int success, int failure)
        {
            if ((!pc.SavingThrow(10, SavingThrowType.Will, D20SavingThrowFlag.NONE)))
            {
                pc.BeginDialog(iuz, failure);
            }
            else
            {
                pc.BeginDialog(iuz, success);
            }

            return SkipDefault;
        }
        public static bool iuz_pc_charm(GameObject iuz, GameObject pc)
        {
            // auto dire charm the PC
            pc.Dominate(iuz);
            if ((GameSystems.Party.PlayerCharactersSize == 1))
            {
                Utilities.set_end_slides(iuz, pc);
                GameSystems.Movies.MovieQueuePlayAndEndGame();
            }
            else
            {
                iuz.Attack(pc);
            }

            return SkipDefault;
        }
        public bool switch_to_hedrack(GameObject iuz, GameObject pc)
        {
            Logger.Info("Iuz: Switching to Hedrack");
            var hedrack = Utilities.find_npc_near(iuz, 8032);
            if ((hedrack != null))
            {
                pc.BeginDialog(hedrack, 200);
                hedrack.TurnTowards(iuz);
                iuz.TurnTowards(hedrack);
            }
            else
            {
                pc.BeginDialog(iuz, 120);
            }

            return SkipDefault;
        }
        public static bool switch_to_cuthbert(GameObject iuz, GameObject pc, int line)
        {
            var cuthbert = Utilities.find_npc_near(iuz, 8043);
            if ((cuthbert != null))
            {
                pc.BeginDialog(cuthbert, line);
                cuthbert.TurnTowards(iuz);
                iuz.TurnTowards(cuthbert);
            }
            else
            {
                iuz.SetObjectFlag(ObjectFlag.OFF);
            }

            return SkipDefault;
        }
        public static bool find_ron(GameObject attachee, GameObject triggerer, int line)
        {
            var ron = Utilities.find_npc_near(attachee, 8730);
            var cuthbert = Utilities.find_npc_near(attachee, 8043);
            if (((ron != null) && (ron.DistanceTo(attachee) <= 12)))
            {
                triggerer.BeginDialog(ron, line);
                ron.TurnTowards(cuthbert);
            }
            else
            {
                triggerer.BeginDialog(cuthbert, 30);
                cuthbert.TurnTowards(attachee);
                attachee.TurnTowards(cuthbert);
            }

            return SkipDefault;
        }
        public static bool iuz_animate_troops(GameObject iuz, GameObject triggerer)
        {
            // raise or heal Hedrack and gargoyles
            AttachParticles("sp-Unholy Blight", iuz);
            Sound(4016, 1);
            if ((Utilities.find_npc_near(iuz, 8032) != null))
            {
                var hedrack = Utilities.find_npc_near(iuz, 8032);
                if ((hedrack.GetStat(Stat.hp_current) <= -10))
                {
                    hedrack.Resurrect(ResurrectionType.CuthbertResurrection, 0);
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        hedrack.AIRemoveFromShitlist(pc);
                        hedrack.SetReaction(pc, 50);
                    }

                    SetGlobalVar(780, 0);
                    SetGlobalVar(781, 0);
                }
                else
                {
                    var dice = Dice.Parse("1d10+1000");
                    hedrack.Heal(null, dice);
                    hedrack.HealSubdual(null, dice);
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        hedrack.AIRemoveFromShitlist(pc);
                        hedrack.SetReaction(pc, 50);
                    }

                }

            }

            if ((Utilities.find_npc_near(iuz, 8085) != null))
            {
                var gargoyle_1 = Utilities.find_npc_near(iuz, 8085);
                if ((gargoyle_1.GetStat(Stat.hp_current) <= -10))
                {
                    gargoyle_1.Resurrect(ResurrectionType.CuthbertResurrection, 0);
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        gargoyle_1.AIRemoveFromShitlist(pc);
                        gargoyle_1.SetReaction(pc, 50);
                    }

                }
                else
                {
                    var dice = Dice.Parse("1d10+1000");
                    gargoyle_1.Heal(null, dice);
                    gargoyle_1.HealSubdual(null, dice);
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        gargoyle_1.AIRemoveFromShitlist(pc);
                        gargoyle_1.SetReaction(pc, 50);
                    }

                }

            }

            if ((Utilities.find_npc_near(iuz, 8086) != null))
            {
                var gargoyle_2 = Utilities.find_npc_near(iuz, 8086);
                if ((gargoyle_2.GetStat(Stat.hp_current) <= -10))
                {
                    gargoyle_2.Resurrect(ResurrectionType.CuthbertResurrection, 0);
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        gargoyle_2.AIRemoveFromShitlist(pc);
                        gargoyle_2.SetReaction(pc, 50);
                    }

                }
                else
                {
                    var dice = Dice.Parse("1d10+1000");
                    gargoyle_2.Heal(null, dice);
                    gargoyle_2.HealSubdual(null, dice);
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        gargoyle_2.AIRemoveFromShitlist(pc);
                        gargoyle_2.SetReaction(pc, 50);
                    }

                }

            }

            if ((Utilities.find_npc_near(iuz, 8087) != null))
            {
                var gargoyle_3 = Utilities.find_npc_near(iuz, 8087);
                if ((gargoyle_3.GetStat(Stat.hp_current) <= -10))
                {
                    gargoyle_3.Resurrect(ResurrectionType.CuthbertResurrection, 0);
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        gargoyle_3.AIRemoveFromShitlist(pc);
                        gargoyle_3.SetReaction(pc, 50);
                    }

                }
                else
                {
                    var dice = Dice.Parse("1d10+1000");
                    gargoyle_3.Heal(null, dice);
                    gargoyle_3.HealSubdual(null, dice);
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        gargoyle_3.AIRemoveFromShitlist(pc);
                        gargoyle_3.SetReaction(pc, 50);
                    }

                }

            }

            if ((Utilities.find_npc_near(iuz, 8088) != null))
            {
                var gargoyle_4 = Utilities.find_npc_near(iuz, 8088);
                if ((gargoyle_4.GetStat(Stat.hp_current) <= -10))
                {
                    gargoyle_4.Resurrect(ResurrectionType.CuthbertResurrection, 0);
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        gargoyle_4.AIRemoveFromShitlist(pc);
                        gargoyle_4.SetReaction(pc, 50);
                    }

                }
                else
                {
                    var dice = Dice.Parse("1d10+1000");
                    gargoyle_4.Heal(null, dice);
                    gargoyle_4.HealSubdual(null, dice);
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        gargoyle_4.AIRemoveFromShitlist(pc);
                        gargoyle_4.SetReaction(pc, 50);
                    }

                }

            }

            // create zombies for dead allies by type and size or heal living
            if ((Utilities.find_npc_near(iuz, 8075) != null))
            {
                var wizard_1 = Utilities.find_npc_near(iuz, 8075);
                if ((wizard_1.GetStat(Stat.hp_current) <= -10))
                {
                    var wizard_zombie_1 = GameSystems.MapObject.CreateObject(14820, wizard_1.GetLocation());
                    wizard_zombie_1.Rotation = wizard_1.Rotation;
                    wizard_zombie_1.AddCondition("Prone", 1, 0);
                    AttachParticles("sp-Command Undead-Hit", wizard_zombie_1);
                    wizard_1.SetObjectFlag(ObjectFlag.OFF);
                    SetGlobalVar(782, GetGlobalVar(782) + 1);
                }
                else
                {
                    var dice = Dice.Parse("1d10+1000");
                    wizard_1.Heal(null, dice);
                    wizard_1.HealSubdual(null, dice);
                }

            }

            if ((Utilities.find_npc_near(iuz, 8076) != null))
            {
                var wizard_2 = Utilities.find_npc_near(iuz, 8076);
                if ((wizard_2.GetStat(Stat.hp_current) <= -10))
                {
                    var wizard_zombie_2 = GameSystems.MapObject.CreateObject(14820, wizard_2.GetLocation());
                    wizard_zombie_2.Rotation = wizard_2.Rotation;
                    wizard_zombie_2.AddCondition("Prone", 1, 0);
                    AttachParticles("sp-Command Undead-Hit", wizard_zombie_2);
                    wizard_2.SetObjectFlag(ObjectFlag.OFF);
                    SetGlobalVar(782, GetGlobalVar(782) + 1);
                }
                else
                {
                    var dice = Dice.Parse("1d10+1000");
                    wizard_2.Heal(null, dice);
                    wizard_2.HealSubdual(null, dice);
                }

            }

            if ((Utilities.find_npc_near(iuz, 8077) != null))
            {
                var bugbear_archer_1 = Utilities.find_npc_near(iuz, 8077);
                if ((bugbear_archer_1.GetStat(Stat.hp_current) <= -10))
                {
                    var bugbear_archer_zombie_1 = GameSystems.MapObject.CreateObject(14821, bugbear_archer_1.GetLocation());
                    bugbear_archer_zombie_1.Rotation = bugbear_archer_1.Rotation;
                    bugbear_archer_zombie_1.AddCondition("Prone", 1, 0);
                    AttachParticles("sp-Command Undead-Hit", bugbear_archer_zombie_1);
                    bugbear_archer_1.SetObjectFlag(ObjectFlag.OFF);
                    SetGlobalVar(782, GetGlobalVar(782) + 1);
                }
                else
                {
                    var dice = Dice.Parse("1d10+1000");
                    bugbear_archer_1.Heal(null, dice);
                    bugbear_archer_1.HealSubdual(null, dice);
                }

            }

            if ((Utilities.find_npc_near(iuz, 8078) != null))
            {
                var bugbear_archer_2 = Utilities.find_npc_near(iuz, 8078);
                if ((bugbear_archer_2.GetStat(Stat.hp_current) <= -10))
                {
                    var bugbear_archer_zombie_2 = GameSystems.MapObject.CreateObject(14821, bugbear_archer_2.GetLocation());
                    bugbear_archer_zombie_2.Rotation = bugbear_archer_2.Rotation;
                    bugbear_archer_zombie_2.AddCondition("Prone", 1, 0);
                    AttachParticles("sp-Command Undead-Hit", bugbear_archer_zombie_2);
                    bugbear_archer_2.SetObjectFlag(ObjectFlag.OFF);
                    SetGlobalVar(782, GetGlobalVar(782) + 1);
                }
                else
                {
                    var dice = Dice.Parse("1d10+1000");
                    bugbear_archer_2.Heal(null, dice);
                    bugbear_archer_2.HealSubdual(null, dice);
                }

            }

            if ((Utilities.find_npc_near(iuz, 8079) != null))
            {
                var bugbear_fighter_1 = Utilities.find_npc_near(iuz, 8079);
                if ((bugbear_fighter_1.GetStat(Stat.hp_current) <= -10))
                {
                    var bugbear_fighter_zombie_1 = GameSystems.MapObject.CreateObject(14822, bugbear_fighter_1.GetLocation());
                    bugbear_fighter_zombie_1.Rotation = bugbear_fighter_1.Rotation;
                    bugbear_fighter_zombie_1.AddCondition("Prone", 1, 0);
                    AttachParticles("sp-Command Undead-Hit", bugbear_fighter_zombie_1);
                    bugbear_fighter_1.SetObjectFlag(ObjectFlag.OFF);
                    SetGlobalVar(782, GetGlobalVar(782) + 1);
                }
                else
                {
                    var dice = Dice.Parse("1d10+1000");
                    bugbear_fighter_1.Heal(null, dice);
                    bugbear_fighter_1.HealSubdual(null, dice);
                }

            }

            if ((Utilities.find_npc_near(iuz, 8080) != null))
            {
                var bugbear_fighter_2 = Utilities.find_npc_near(iuz, 8080);
                if ((bugbear_fighter_2.GetStat(Stat.hp_current) <= -10))
                {
                    var bugbear_fighter_zombie_2 = GameSystems.MapObject.CreateObject(14822, bugbear_fighter_2.GetLocation());
                    bugbear_fighter_zombie_2.Rotation = bugbear_fighter_2.Rotation;
                    bugbear_fighter_zombie_2.AddCondition("Prone", 1, 0);
                    AttachParticles("sp-Command Undead-Hit", bugbear_fighter_zombie_2);
                    bugbear_fighter_2.SetObjectFlag(ObjectFlag.OFF);
                    SetGlobalVar(782, GetGlobalVar(782) + 1);
                }
                else
                {
                    var dice = Dice.Parse("1d10+1000");
                    bugbear_fighter_2.Heal(null, dice);
                    bugbear_fighter_2.HealSubdual(null, dice);
                }

            }

            if ((Utilities.find_npc_near(iuz, 8081) != null))
            {
                var bugbear_fighter_3 = Utilities.find_npc_near(iuz, 8081);
                if ((bugbear_fighter_3.GetStat(Stat.hp_current) <= -10))
                {
                    var bugbear_fighter_zombie_3 = GameSystems.MapObject.CreateObject(14823, bugbear_fighter_3.GetLocation());
                    bugbear_fighter_zombie_3.Rotation = bugbear_fighter_3.Rotation;
                    bugbear_fighter_zombie_3.AddCondition("Prone", 1, 0);
                    AttachParticles("sp-Command Undead-Hit", bugbear_fighter_zombie_3);
                    bugbear_fighter_3.SetObjectFlag(ObjectFlag.OFF);
                    SetGlobalVar(782, GetGlobalVar(782) + 1);
                }
                else
                {
                    var dice = Dice.Parse("1d10+1000");
                    bugbear_fighter_3.Heal(null, dice);
                    bugbear_fighter_3.HealSubdual(null, dice);
                }

            }

            if ((Utilities.find_npc_near(iuz, 8082) != null))
            {
                var bugbear_fighter_4 = Utilities.find_npc_near(iuz, 8082);
                if ((bugbear_fighter_4.GetStat(Stat.hp_current) <= -10))
                {
                    var bugbear_fighter_zombie_4 = GameSystems.MapObject.CreateObject(14823, bugbear_fighter_4.GetLocation());
                    bugbear_fighter_zombie_4.Rotation = bugbear_fighter_4.Rotation;
                    bugbear_fighter_zombie_4.AddCondition("Prone", 1, 0);
                    AttachParticles("sp-Command Undead-Hit", bugbear_fighter_zombie_4);
                    bugbear_fighter_4.SetObjectFlag(ObjectFlag.OFF);
                    SetGlobalVar(782, GetGlobalVar(782) + 1);
                }
                else
                {
                    var dice = Dice.Parse("1d10+1000");
                    bugbear_fighter_4.Heal(null, dice);
                    bugbear_fighter_4.HealSubdual(null, dice);
                }

            }

            if ((Utilities.find_npc_near(iuz, 8083) != null))
            {
                var ettin_1 = Utilities.find_npc_near(iuz, 8083);
                if ((ettin_1.GetStat(Stat.hp_current) <= -10))
                {
                    var ettin_zombie_1 = GameSystems.MapObject.CreateObject(14824, ettin_1.GetLocation());
                    ettin_zombie_1.Rotation = ettin_1.Rotation;
                    ettin_zombie_1.AddCondition("Prone", 1, 0);
                    AttachParticles("sp-Command Undead-Hit", ettin_zombie_1);
                    ettin_1.SetObjectFlag(ObjectFlag.OFF);
                    SetGlobalVar(782, GetGlobalVar(782) + 1);
                }
                else
                {
                    var dice = Dice.Parse("1d10+1000");
                    ettin_1.Heal(null, dice);
                    ettin_1.HealSubdual(null, dice);
                }

            }

            if ((Utilities.find_npc_near(iuz, 8084) != null))
            {
                var ettin_2 = Utilities.find_npc_near(iuz, 8084);
                if ((ettin_2.GetStat(Stat.hp_current) <= -10))
                {
                    var ettin_zombie_2 = GameSystems.MapObject.CreateObject(14824, ettin_2.GetLocation());
                    ettin_zombie_2.Rotation = ettin_2.Rotation;
                    ettin_zombie_2.AddCondition("Prone", 1, 0);
                    AttachParticles("sp-Command Undead-Hit", ettin_zombie_2);
                    ettin_2.SetObjectFlag(ObjectFlag.OFF);
                    SetGlobalVar(782, GetGlobalVar(782) + 1);
                }
                else
                {
                    var dice = Dice.Parse("1d10+1000");
                    ettin_2.Heal(null, dice);
                    ettin_2.HealSubdual(null, dice);
                }

            }

            if ((Utilities.find_npc_near(iuz, 8089) != null))
            {
                var hill_giant = Utilities.find_npc_near(iuz, 8089);
                if ((hill_giant.GetStat(Stat.hp_current) <= -10))
                {
                    var hill_giant_zombie = GameSystems.MapObject.CreateObject(14825, hill_giant.GetLocation());
                    hill_giant_zombie.Rotation = hill_giant.Rotation;
                    hill_giant_zombie.AddCondition("Prone", 1, 0);
                    AttachParticles("sp-Command Undead-Hit", hill_giant_zombie);
                    hill_giant.SetObjectFlag(ObjectFlag.OFF);
                    SetGlobalVar(782, GetGlobalVar(782) + 1);
                }
                else
                {
                    var dice = Dice.Parse("1d10+1000");
                    hill_giant.Heal(null, dice);
                    hill_giant.HealSubdual(null, dice);
                }

            }

            StartTimer(2000, () => make_hostile());
            return SkipDefault;
        }
        public static bool orientation(GameObject attachee, GameObject triggerer)
        {
            attachee.TurnTowards(triggerer);
            return RunDefault;
        }
        public static bool unshit(GameObject attachee, GameObject triggerer)
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 50);
            }

            return RunDefault;
        }
        public static bool make_hostile()
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if ((Utilities.find_npc_near(pc, 8032) != null))
                {
                    var hedrack = Utilities.find_npc_near(pc, 8032);
                    hedrack.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 8085) != null))
                {
                    var gargoyle_1 = Utilities.find_npc_near(pc, 8085);
                    gargoyle_1.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 8086) != null))
                {
                    var gargoyle_1 = Utilities.find_npc_near(pc, 8086);
                    gargoyle_1.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 8087) != null))
                {
                    var gargoyle_1 = Utilities.find_npc_near(pc, 8087);
                    gargoyle_1.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 8088) != null))
                {
                    var gargoyle_1 = Utilities.find_npc_near(pc, 8088);
                    gargoyle_1.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 8075) != null))
                {
                    var wizard_1 = Utilities.find_npc_near(pc, 8075);
                    wizard_1.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 8076) != null))
                {
                    var wizard_2 = Utilities.find_npc_near(pc, 8076);
                    wizard_2.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 8077) != null))
                {
                    var bugarcher_1 = Utilities.find_npc_near(pc, 8077);
                    bugarcher_1.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 8078) != null))
                {
                    var bugarcher_2 = Utilities.find_npc_near(pc, 8078);
                    bugarcher_2.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 8079) != null))
                {
                    var bugfighter_1 = Utilities.find_npc_near(pc, 8079);
                    bugfighter_1.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 8080) != null))
                {
                    var bugfighter_2 = Utilities.find_npc_near(pc, 8080);
                    bugfighter_2.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 8081) != null))
                {
                    var bugfighter_3 = Utilities.find_npc_near(pc, 8081);
                    bugfighter_3.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 8082) != null))
                {
                    var bugfighter_4 = Utilities.find_npc_near(pc, 8082);
                    bugfighter_4.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 8083) != null))
                {
                    var ettin_1 = Utilities.find_npc_near(pc, 8083);
                    ettin_1.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 8084) != null))
                {
                    var ettin_2 = Utilities.find_npc_near(pc, 8084);
                    ettin_2.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 8089) != null))
                {
                    var giant = Utilities.find_npc_near(pc, 8089);
                    giant.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 14820) != null))
                {
                    var wizombie_1 = Utilities.find_npc_near(pc, 14820);
                    wizombie_1.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 14820) != null))
                {
                    var wizombie_2 = Utilities.find_npc_near(pc, 14820);
                    wizombie_2.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 14821) != null))
                {
                    var zombugarcher_1 = Utilities.find_npc_near(pc, 14821);
                    zombugarcher_1.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 14821) != null))
                {
                    var zombugarcher_2 = Utilities.find_npc_near(pc, 14821);
                    zombugarcher_2.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 14822) != null))
                {
                    var zombugfighter_1 = Utilities.find_npc_near(pc, 14822);
                    zombugfighter_1.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 14822) != null))
                {
                    var zombugfighter_2 = Utilities.find_npc_near(pc, 14822);
                    zombugfighter_2.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 14823) != null))
                {
                    var zombugfighter_3 = Utilities.find_npc_near(pc, 14823);
                    zombugfighter_3.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 14823) != null))
                {
                    var zombugfighter_4 = Utilities.find_npc_near(pc, 14823);
                    zombugfighter_4.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 14824) != null))
                {
                    var zombettin_1 = Utilities.find_npc_near(pc, 14824);
                    zombettin_1.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 14824) != null))
                {
                    var zombettin_2 = Utilities.find_npc_near(pc, 14824);
                    zombettin_2.SetNpcFlag(NpcFlag.KOS);
                }

                if ((Utilities.find_npc_near(pc, 14825) != null))
                {
                    var zombgiant = Utilities.find_npc_near(pc, 14825);
                    zombgiant.SetNpcFlag(NpcFlag.KOS);
                }

            }

            return RunDefault;
        }

    }
}
