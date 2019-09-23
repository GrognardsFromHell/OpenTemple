
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(174)]
    public class Hedrack : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(144)))
            {
                if ((!attachee.HasMet(triggerer)))
                {
                    triggerer.BeginDialog(attachee, 10);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 290);
                }

            }
            else if ((GetQuestState(58) >= QuestState.Accepted))
            {
                triggerer.BeginDialog(attachee, 480);
            }
            else if ((attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 490);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            Logger.Info("Hedrack First Heartbeat");
            if ((GetGlobalFlag(372)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else
            {
                if ((GetGlobalVar(754) == 1))
                {
                    return RunDefault;
                }

                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    SetGlobalVar(719, 0);
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((Utilities.is_safe_to_talk(attachee, obj)))
                        {
                            if ((GetGlobalVar(691) == 3))
                            {
                                obj.TurnTowards(attachee); // added by Livonya
                                attachee.TurnTowards(obj); // added by Livonya
                                obj.BeginDialog(attachee, 40);
                            }
                            else if ((GetGlobalVar(691) == 2))
                            {
                                obj.TurnTowards(attachee); // added by Livonya
                                attachee.TurnTowards(obj); // added by Livonya
                                obj.BeginDialog(attachee, 30);
                            }
                            else if ((GetGlobalVar(691) == 1))
                            {
                                obj.TurnTowards(attachee); // added by Livonya
                                attachee.TurnTowards(obj); // added by Livonya
                                obj.BeginDialog(attachee, 20);
                            }
                            else
                            {
                                obj.TurnTowards(attachee); // added by Livonya
                                attachee.TurnTowards(obj); // added by Livonya
                                obj.BeginDialog(attachee, 1);
                            }

                            SetGlobalVar(754, 1);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(146, true);
            return RunDefault;
        }
        public override bool OnEndCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var npc = Utilities.find_npc_near(attachee, 8001);
            if ((npc != null))
            {
                SetGlobalFlag(325, true);
            }

            npc = Utilities.find_npc_near(attachee, 8059);
            if ((npc != null))
            {
                SetGlobalFlag(325, true);
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            Logger.Info("Hedrack san_start_combat");
            if ((attachee != null && Utilities.critter_is_unconscious(attachee) != 1 && !attachee.D20Query(D20DispatcherKey.QUE_Prone)))
            {
                SetGlobalVar(744, GetGlobalVar(744) + 1);
                if ((GetGlobalVar(744) == 3 && !GetGlobalFlag(823) && !GetGlobalFlag(147) && !GetGlobalFlag(990)))
                {
                    var shocky_backup = GameSystems.MapObject.CreateObject(14233, attachee.GetLocation() - 8);
                    shocky_backup.TurnTowards(attachee);
                    Sound(4035, 1);
                    AttachParticles("sp-Teleport", shocky_backup);
                    var racky = attachee.GetInitiative();
                    shocky_backup.AddToInitiative();
                    shocky_backup.SetInitiative(racky);
                    UiSystems.Combat.Initiative.UpdateIfNeeded();
                    foreach (var obj in ObjList.ListVicinity(shocky_backup.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        shocky_backup.Attack(obj);
                    }

                    SetGlobalFlag(823, true);
                }

                if ((Utilities.obj_percent_hp(attachee) <= 50))
                {
                    if ((!GetGlobalFlag(377)))
                    {
                        Co8.StopCombat(attachee, 0);
                        var delegatePc = Utilities.GetDelegatePc(attachee);
                        Logger.Info("{0}", "Hedrack: Stopping combat. Delegate PC selected for dialog is " + delegatePc.ToString());
                        foreach (var pc in GameSystems.Party.PartyMembers)
                        {
                            attachee.AIRemoveFromShitlist(pc);
                        }

                        if ((delegatePc != null))
                        {
                            delegatePc.TurnTowards(attachee);
                            attachee.TurnTowards(delegatePc);
                            delegatePc.BeginDialog(attachee, 190);
                            SetGlobalFlag(377, true);
                            return SkipDefault;
                        }

                    }
                    else
                    {
                        if ((GetGlobalVar(781) <= 5))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 469);
                            SetGlobalVar(781, GetGlobalVar(781) + 1);
                        }
                        else if ((GetGlobalVar(780) <= 8))
                        {
                            if ((GetGlobalVar(782) >= 5))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 470);
                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 468);
                            }

                            SetGlobalVar(780, GetGlobalVar(780) + 1);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 472);
                        }

                    }

                }
                else if ((Utilities.obj_percent_hp(attachee) >= 51 && Utilities.obj_percent_hp(attachee) <= 75))
                {
                    if ((!GetGlobalFlag(377)))
                    {
                        if ((GetGlobalVar(781) <= 5))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 469);
                            SetGlobalVar(781, GetGlobalVar(781) + 1);
                        }
                        else if ((GetGlobalVar(780) <= 8))
                        {
                            if ((GetGlobalVar(782) >= 5))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 470);
                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 468);
                            }

                            SetGlobalVar(780, GetGlobalVar(780) + 1);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 472);
                        }

                    }
                    else
                    {
                        if ((GetGlobalVar(781) <= 5))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 469);
                            SetGlobalVar(781, GetGlobalVar(781) + 1);
                        }
                        else if ((GetGlobalVar(780) <= 8))
                        {
                            if ((GetGlobalVar(782) >= 5))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 470);
                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 468);
                            }

                            SetGlobalVar(780, GetGlobalVar(780) + 1);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 472);
                        }

                    }

                }
                else
                {
                    if ((!GetGlobalFlag(377)))
                    {
                        if ((GetGlobalVar(780) <= 8))
                        {
                            if ((GetGlobalVar(782) >= 5))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 470);
                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 468);
                            }

                            SetGlobalVar(780, GetGlobalVar(780) + 1);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 472);
                        }

                    }
                    else
                    {
                        if ((GetGlobalVar(780) <= 8))
                        {
                            if ((GetGlobalVar(782) >= 5))
                            {
                                attachee.SetInt(obj_f.critter_strategy, 470);
                            }
                            else
                            {
                                attachee.SetInt(obj_f.critter_strategy, 468);
                            }

                            SetGlobalVar(780, GetGlobalVar(780) + 1);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 472);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(146, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GameSystems.Combat.IsCombatActive()))
            {
                return RunDefault;
            }
            else
            {
                Logger.Info("Hedrack Heartbeat");
                var closest_jones = Utilities.party_closest(attachee);
                if ((attachee.DistanceTo(closest_jones) <= 100))
                {
                    SetGlobalVar(719, GetGlobalVar(719) + 1);
                    if ((attachee.GetLeader() == null))
                    {
                        if ((GetGlobalVar(719) == 4))
                        {
                            attachee.CastSpell(WellKnownSpells.FreedomOfMovement, attachee);
                            attachee.PendingSpellsToMemorized();
                        }

                        if ((GetGlobalVar(719) == 8))
                        {
                            attachee.CastSpell(WellKnownSpells.OwlsWisdom, attachee);
                            attachee.PendingSpellsToMemorized();
                        }

                        if ((GetGlobalVar(719) == 12))
                        {
                            attachee.CastSpell(WellKnownSpells.ShieldOfFaith, attachee);
                            attachee.PendingSpellsToMemorized();
                        }

                        if ((GetGlobalVar(719) == 16))
                        {
                            attachee.CastSpell(WellKnownSpells.ProtectionFromGood, attachee);
                            attachee.PendingSpellsToMemorized();
                        }

                        if ((GetGlobalVar(719) == 20))
                        {
                            attachee.CastSpell(WellKnownSpells.ProtectionFromLaw, attachee);
                            attachee.PendingSpellsToMemorized();
                        }

                    }

                    if ((GetGlobalVar(719) >= 400))
                    {
                        SetGlobalVar(719, 0);
                    }

                }

                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((is_28_and_under(attachee, obj) && !GetGlobalFlag(812)))
                    {
                        if ((GetQuestState(58) != QuestState.Unknown))
                        {
                            SetGlobalFlag(812, true);
                            return SkipDefault;
                        }
                        else if ((GetGlobalVar(691) == 3))
                        {
                            if ((PartyLeader.GetStat(Stat.hp_current) >= 1 && !PartyLeader.D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                PartyLeader.TurnTowards(attachee);
                                attachee.TurnTowards(PartyLeader);
                                PartyLeader.BeginDialog(attachee, 40);
                            }
                            else
                            {
                                obj.TurnTowards(attachee);
                                attachee.TurnTowards(obj);
                                obj.BeginDialog(attachee, 40);
                            }

                        }
                        else if ((GetGlobalVar(691) == 2))
                        {
                            if ((PartyLeader.GetStat(Stat.hp_current) >= 1 && !PartyLeader.D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                PartyLeader.TurnTowards(attachee);
                                attachee.TurnTowards(PartyLeader);
                                PartyLeader.BeginDialog(attachee, 30);
                            }
                            else
                            {
                                obj.TurnTowards(attachee);
                                attachee.TurnTowards(obj);
                                obj.BeginDialog(attachee, 30);
                            }

                        }
                        else if ((GetGlobalVar(691) == 1))
                        {
                            if ((PartyLeader.GetStat(Stat.hp_current) >= 1 && !PartyLeader.D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                PartyLeader.TurnTowards(attachee);
                                attachee.TurnTowards(PartyLeader);
                                PartyLeader.BeginDialog(attachee, 20);
                            }
                            else
                            {
                                obj.TurnTowards(attachee);
                                attachee.TurnTowards(obj);
                                obj.BeginDialog(attachee, 20);
                            }

                        }
                        else if ((GetGlobalFlag(144)))
                        {
                            if ((!attachee.HasMet(obj)))
                            {
                                if ((PartyLeader.GetStat(Stat.hp_current) >= 1 && !PartyLeader.D20Query(D20DispatcherKey.QUE_Prone)))
                                {
                                    PartyLeader.TurnTowards(attachee);
                                    attachee.TurnTowards(PartyLeader);
                                    PartyLeader.BeginDialog(attachee, 10);
                                }
                                else
                                {
                                    obj.TurnTowards(attachee);
                                    attachee.TurnTowards(obj);
                                    obj.BeginDialog(attachee, 10);
                                }

                            }
                            else
                            {
                                if ((PartyLeader.GetStat(Stat.hp_current) >= 1 && !PartyLeader.D20Query(D20DispatcherKey.QUE_Prone)))
                                {
                                    PartyLeader.TurnTowards(attachee);
                                    attachee.TurnTowards(PartyLeader);
                                    PartyLeader.BeginDialog(attachee, 290);
                                }
                                else
                                {
                                    obj.TurnTowards(attachee);
                                    attachee.TurnTowards(obj);
                                    obj.BeginDialog(attachee, 290);
                                }

                            }

                        }
                        else if ((GetQuestState(58) >= QuestState.Accepted))
                        {
                            if ((PartyLeader.GetStat(Stat.hp_current) >= 1 && !PartyLeader.D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                PartyLeader.TurnTowards(attachee);
                                attachee.TurnTowards(PartyLeader);
                                PartyLeader.BeginDialog(attachee, 480);
                            }
                            else
                            {
                                obj.TurnTowards(attachee);
                                attachee.TurnTowards(obj);
                                obj.BeginDialog(attachee, 480);
                            }

                        }
                        else if ((attachee.HasMet(obj)))
                        {
                            if ((PartyLeader.GetStat(Stat.hp_current) >= 1 && !PartyLeader.D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                PartyLeader.TurnTowards(attachee);
                                attachee.TurnTowards(PartyLeader);
                                PartyLeader.BeginDialog(attachee, 490);
                            }
                            else
                            {
                                obj.TurnTowards(attachee);
                                attachee.TurnTowards(obj);
                                obj.BeginDialog(attachee, 490);
                            }

                        }
                        else
                        {
                            if ((PartyLeader.GetStat(Stat.hp_current) >= 1 && !PartyLeader.D20Query(D20DispatcherKey.QUE_Prone)))
                            {
                                PartyLeader.TurnTowards(attachee);
                                attachee.TurnTowards(PartyLeader);
                                PartyLeader.BeginDialog(attachee, 1);
                            }
                            else
                            {
                                obj.TurnTowards(attachee);
                                attachee.TurnTowards(obj);
                                obj.BeginDialog(attachee, 1);
                            }

                        }

                        SetGlobalFlag(812, true);
                    }

                }

            }

            return RunDefault;
        }
        public static bool talk_Romag(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8037);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 520);
            }

            return SkipDefault;
        }
        public static bool summon_Iuz(GameObjectBody attachee, GameObjectBody triggerer)
        {
            Logger.Info("Hedrack: Summoning Iuz");
            // needs to make Iuz appear near him
            var Iuz = GameSystems.MapObject.CreateObject(14266, attachee.GetLocation() - 4);
            attachee.TurnTowards(Iuz);
            Iuz.TurnTowards(attachee);
            return SkipDefault;
        }
        public static bool talk_Iuz(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var Iuz = Utilities.find_npc_near(attachee, 8042);
            if ((Iuz != null))
            {
                triggerer.BeginDialog(Iuz, line);
                Iuz.TurnTowards(attachee);
                attachee.TurnTowards(Iuz);
            }
            else
            {
                triggerer.BeginDialog(attachee, 30);
            }

            return SkipDefault;
        }
        public static bool end_game(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(339, true);
            // play slides and end game
            Utilities.set_join_slides(attachee, triggerer);
            GameSystems.Movies.MovieQueuePlayAndEndGame();
            return SkipDefault;
        }
        public static bool give_robes(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                Utilities.create_item_in_inventory(6113, pc);
            }

            return SkipDefault;
        }
        public static int is_28_and_under(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 28))
                {
                    return 1;
                }

            }

            return 0;
        }
        public static bool unshit(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 50);
            }

            return RunDefault;
        }

    }
}
