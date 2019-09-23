
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
    [ObjectScript(378)]
    public class Cousins : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5158))
            {
                if ((GetGlobalFlag(989)))
                {
                    triggerer.BeginDialog(attachee, 130);
                }
                else if ((!attachee.HasMet(triggerer)))
                {
                    triggerer.BeginDialog(attachee, 1);
                }
                else if ((GetGlobalFlag(982)))
                {
                    triggerer.BeginDialog(attachee, 180);
                }
                else if ((attachee.HasMet(triggerer)) && (!GetGlobalFlag(983)) && (GetGlobalVar(930) == 0))
                {
                    triggerer.BeginDialog(attachee, 140);
                }
                else if ((attachee.HasMet(triggerer)) && (GetGlobalFlag(983)) && (GetGlobalVar(930) == 0))
                {
                    triggerer.BeginDialog(attachee, 150);
                }
                else if ((attachee.HasMet(triggerer)) && (!GetGlobalFlag(983)) && (GetGlobalVar(930) == 2))
                {
                    triggerer.BeginDialog(attachee, 160);
                }
                else if ((attachee.HasMet(triggerer)) && (!GetGlobalFlag(983)) && (GetGlobalVar(930) == 1))
                {
                    triggerer.BeginDialog(attachee, 280);
                }

            }
            else if ((attachee.GetMap() == 5071 || attachee.GetMap() == 5072 || attachee.GetMap() == 5073 || attachee.GetMap() == 5075 || attachee.GetMap() == 5076 || attachee.GetMap() == 5077))
            {
                triggerer.BeginDialog(attachee, 150);
            }
            else
            {
                return RunDefault;
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((GetGlobalVar(959) == 1) && (attachee.GetMap() == 5052)))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            if (((GetGlobalVar(978) == 1) && (attachee.GetMap() == 5121)))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                if ((!GetGlobalFlag(961)))
                {
                    StartTimer(864000000, () => term_surv()); // 10 days
                    SetGlobalFlag(961, true);
                }

            }

            if (((GetGlobalVar(978) == 2) && (GetGlobalFlag(967)) && (attachee.GetMap() == 5174)))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                attachee.RunOff();
                SetGlobalVar(978, 3);
            }

            if (((GetGlobalVar(978) == 2) && (GetGlobalFlag(997)) && (attachee.GetMap() == 5152)))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                attachee.RunOff();
                SetGlobalVar(978, 4);
            }

            if (((GetGlobalVar(978) == 2) && (GetGlobalFlag(966)) && (attachee.GetMap() == 5146)))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                attachee.RunOff();
                SetGlobalVar(978, 5);
            }

            if (((GetGlobalVar(978) >= 2) && (attachee.GetMap() == 5121)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            if ((attachee.GetMap() == 5158 || attachee.GetMap() == 5159 || attachee.GetMap() == 5160))
            {
                if ((GetGlobalVar(945) >= 25))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
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

            var gloves = attachee.FindItemByName(6201);
            gloves.Destroy();
            Utilities.create_item_in_inventory(6046, attachee);
            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5071 || attachee.GetMap() == 5072 || attachee.GetMap() == 5073 || attachee.GetMap() == 5075 || attachee.GetMap() == 5076 || attachee.GetMap() == 5077))
            {
                SetGlobalVar(943, GetGlobalVar(943) + 1);
                if ((attachee.GetNameId() == 14653))
                {
                    if ((GetGlobalVar(943) == 0))
                    {
                        return RunDefault;
                    }
                    else if ((GetGlobalVar(943) == 1))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 418);
                        return RunDefault;
                    }
                    else if ((GetGlobalVar(943) == 2))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 419);
                        return RunDefault;
                    }
                    else if ((GetGlobalVar(943) == 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 420);
                        return RunDefault;
                    }
                    else if ((GetGlobalVar(943) == 4))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 417);
                        return RunDefault;
                    }
                    else if ((GetGlobalVar(943) >= 5))
                    {
                        run_off_2(attachee, triggerer);
                        return SkipDefault;
                    }

                }
                else if ((attachee.GetNameId() == 14652))
                {
                    if ((GetGlobalVar(943) >= 5))
                    {
                        run_off_2(attachee, triggerer);
                        return SkipDefault;
                    }
                    else
                    {
                        return RunDefault;
                    }

                }

            }

        }
        public override bool OnEndCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(943) <= 4))
            {
                Sound(4116, 1);
                foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    AttachParticles("Trap-Spores", obj);
                    obj.AddCondition("Poisoned", 3, 0);
                    obj.AddCondition("Poisoned", 25, 0);
                    obj.AddCondition("Prone", 4, 0);
                }

                foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if ((obj.GetNameId() == 8000 || obj.GetNameId() == 8001 || obj.GetNameId() == 8002 || obj.GetNameId() == 8003 || obj.GetNameId() == 8004 || obj.GetNameId() == 8005 || obj.GetNameId() == 8010 || obj.GetNameId() == 8014 || obj.GetNameId() == 8015 || obj.GetNameId() == 8020 || obj.GetNameId() == 8021 || obj.GetNameId() == 8022 || obj.GetNameId() == 8023 || obj.GetNameId() == 8025 || obj.GetNameId() == 8026 || obj.GetNameId() == 8029 || obj.GetNameId() == 8030 || obj.GetNameId() == 8031 || obj.GetNameId() == 8034 || obj.GetNameId() == 8039 || obj.GetNameId() == 8040 || obj.GetNameId() == 8050 || obj.GetNameId() == 8054 || obj.GetNameId() == 8056 || obj.GetNameId() == 8057 || obj.GetNameId() == 8058 || obj.GetNameId() == 8060 || obj.GetNameId() == 8061 || obj.GetNameId() == 8062 || obj.GetNameId() == 8067 || obj.GetNameId() == 8069 || obj.GetNameId() == 8070 || obj.GetNameId() == 8071 || obj.GetNameId() == 8072 || obj.GetNameId() == 8714 || obj.GetNameId() == 8716 || obj.GetNameId() == 8717 || obj.GetNameId() == 8718 || obj.GetNameId() == 8730))
                    {
                        AttachParticles("Trap-Spores", obj);
                        obj.AddCondition("Poisoned", 3, 0);
                        obj.AddCondition("Poisoned", 25, 0);
                        obj.AddCondition("Prone", 4, 0);
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5052))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    if ((is_better_to_talk(attachee, PartyLeader)))
                    {
                        if ((GetGlobalVar(959) == 1))
                        {
                            PartyLeader.TurnTowards(attachee);
                            PartyLeader.BeginDialog(attachee, 230);
                            SetGlobalVar(959, 2);
                        }

                    }

                }

            }
            else if ((attachee.GetMap() == 5071 || attachee.GetMap() == 5075)) // re forest
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    if ((attachee.GetNameId() == 14651))
                    {
                        if ((GetGlobalVar(945) == 13 || GetGlobalVar(945) == 14 || GetGlobalVar(945) == 15))
                        {
                            if ((is_better_to_talk(attachee, PartyLeader)))
                            {
                                Co8.StopCombat(attachee, 0);
                                attachee.FloatLine(10000, triggerer);
                                DetachScript();
                            }

                        }
                        else if ((GetGlobalVar(945) == 19 || GetGlobalVar(945) == 20 || GetGlobalVar(945) == 21))
                        {
                            if ((is_better_to_talk(attachee, PartyLeader)))
                            {
                                SetGlobalVar(945, 22);
                                PartyLeader.BeginDialog(attachee, 200);
                            }

                        }

                    }

                }

            }
            else if ((attachee.GetMap() == 5072 || attachee.GetMap() == 5076)) // re swamp
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    if ((attachee.GetNameId() == 14651))
                    {
                        if ((GetGlobalVar(945) == 13 || GetGlobalVar(945) == 14 || GetGlobalVar(945) == 15))
                        {
                            if ((is_better_to_talk(attachee, PartyLeader)))
                            {
                                Co8.StopCombat(attachee, 0);
                                attachee.FloatLine(20000, triggerer);
                                DetachScript();
                            }

                        }
                        else if ((GetGlobalVar(945) == 19 || GetGlobalVar(945) == 20 || GetGlobalVar(945) == 21))
                        {
                            if ((is_better_to_talk(attachee, PartyLeader)))
                            {
                                SetGlobalVar(945, 23);
                                PartyLeader.BeginDialog(attachee, 200);
                            }

                        }

                    }

                }

            }
            else if ((attachee.GetMap() == 5073 || attachee.GetMap() == 5077)) // re river
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    if ((attachee.GetNameId() == 14651))
                    {
                        if ((GetGlobalVar(945) == 13 || GetGlobalVar(945) == 14 || GetGlobalVar(945) == 15))
                        {
                            if ((is_better_to_talk(attachee, PartyLeader)))
                            {
                                Co8.StopCombat(attachee, 0);
                                attachee.FloatLine(30000, triggerer);
                                DetachScript();
                            }

                        }
                        else if ((GetGlobalVar(945) == 19 || GetGlobalVar(945) == 20 || GetGlobalVar(945) == 21))
                        {
                            if ((is_better_to_talk(attachee, PartyLeader)))
                            {
                                SetGlobalVar(945, 24);
                                PartyLeader.BeginDialog(attachee, 200);
                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(989)))
            {
                return RunDefault;
            }

            return SkipDefault;
        }
        public static bool wait_a_day(GameObjectBody attachee, GameObjectBody triggerer)
        {
            StartTimer(86400000, () => wait(attachee)); // 1 day
            return RunDefault;
        }
        public static bool wait(GameObjectBody attachee)
        {
            SetGlobalVar(930, 2);
            return RunDefault;
        }
        public static bool term_surv()
        {
            SetGlobalVar(978, 2);
            return RunDefault;
        }
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.DistanceTo(listener) <= 50))
            {
                return true;
            }

            return false;
        }
        public static bool go_away(GameObjectBody attachee)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.RunOff();
            StartTimer(5000, () => go_away(attachee));
            return RunDefault;
        }
        public static bool run_off_2(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.RunOff();
            var nephew1 = Utilities.find_npc_near(attachee, 14653);
            nephew1.RunOff(attachee.GetLocation() - 3);
            var nephew2 = Utilities.find_npc_near(attachee, 14653);
            nephew2.RunOff(attachee.GetLocation() - 3);
            var nephew3 = Utilities.find_npc_near(attachee, 14653);
            nephew3.RunOff(attachee.GetLocation() - 3);
            var cousin1 = Utilities.find_npc_near(attachee, 14652);
            cousin1.RunOff(attachee.GetLocation() - 3);
            if ((!GetGlobalFlag(866)))
            {
                SetGlobalFlag(866, true);
                if ((attachee.GetMap() == 5071 || attachee.GetMap() == 5075))
                {
                    StartTimer(2000, () => pass_out_1_a(attachee, triggerer)); // 2 seconds
                    foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        obj.AddCondition("Prone", 8200, 0);
                        obj.AddCondition("Paralyzed", 8200, 0);
                    }

                    foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_NPC))
                    {
                        if ((obj.GetNameId() == 8000 || obj.GetNameId() == 8001 || obj.GetNameId() == 8002 || obj.GetNameId() == 8003 || obj.GetNameId() == 8004 || obj.GetNameId() == 8005 || obj.GetNameId() == 8010 || obj.GetNameId() == 8014 || obj.GetNameId() == 8015 || obj.GetNameId() == 8020 || obj.GetNameId() == 8021 || obj.GetNameId() == 8022 || obj.GetNameId() == 8023 || obj.GetNameId() == 8025 || obj.GetNameId() == 8026 || obj.GetNameId() == 8029 || obj.GetNameId() == 8030 || obj.GetNameId() == 8031 || obj.GetNameId() == 8034 || obj.GetNameId() == 8039 || obj.GetNameId() == 8040 || obj.GetNameId() == 8050 || obj.GetNameId() == 8054 || obj.GetNameId() == 8056 || obj.GetNameId() == 8057 || obj.GetNameId() == 8058 || obj.GetNameId() == 8060 || obj.GetNameId() == 8061 || obj.GetNameId() == 8062 || obj.GetNameId() == 8067 || obj.GetNameId() == 8069 || obj.GetNameId() == 8070 || obj.GetNameId() == 8071 || obj.GetNameId() == 8072 || obj.GetNameId() == 8714 || obj.GetNameId() == 8716 || obj.GetNameId() == 8717 || obj.GetNameId() == 8718 || obj.GetNameId() == 8730))
                        {
                            obj.AddCondition("Prone", 8200, 0);
                            obj.AddCondition("Paralyzed", 8200, 0);
                        }

                    }

                }
                else if ((attachee.GetMap() == 5072 || attachee.GetMap() == 5073 || attachee.GetMap() == 5076 || attachee.GetMap() == 5077))
                {
                    StartTimer(2000, () => pass_out_1_b(attachee, triggerer)); // 2 seconds
                    foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        obj.AddCondition("Prone", 8200, 0);
                        obj.AddCondition("Paralyzed", 8200, 0);
                    }

                    foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_NPC))
                    {
                        if ((obj.GetNameId() == 8000 || obj.GetNameId() == 8001 || obj.GetNameId() == 8002 || obj.GetNameId() == 8003 || obj.GetNameId() == 8004 || obj.GetNameId() == 8005 || obj.GetNameId() == 8010 || obj.GetNameId() == 8014 || obj.GetNameId() == 8015 || obj.GetNameId() == 8020 || obj.GetNameId() == 8021 || obj.GetNameId() == 8022 || obj.GetNameId() == 8023 || obj.GetNameId() == 8025 || obj.GetNameId() == 8026 || obj.GetNameId() == 8029 || obj.GetNameId() == 8030 || obj.GetNameId() == 8031 || obj.GetNameId() == 8034 || obj.GetNameId() == 8039 || obj.GetNameId() == 8040 || obj.GetNameId() == 8050 || obj.GetNameId() == 8054 || obj.GetNameId() == 8056 || obj.GetNameId() == 8057 || obj.GetNameId() == 8058 || obj.GetNameId() == 8060 || obj.GetNameId() == 8061 || obj.GetNameId() == 8062 || obj.GetNameId() == 8067 || obj.GetNameId() == 8069 || obj.GetNameId() == 8070 || obj.GetNameId() == 8071 || obj.GetNameId() == 8072 || obj.GetNameId() == 8714 || obj.GetNameId() == 8716 || obj.GetNameId() == 8717 || obj.GetNameId() == 8718 || obj.GetNameId() == 8730))
                        {
                            obj.AddCondition("Prone", 8200, 0);
                            obj.AddCondition("Paralyzed", 8200, 0);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool run_off_3(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            AttachParticles("sp-Invisibility", attachee);
            var cousin1 = Utilities.find_npc_near(attachee, 14651);
            cousin1.SetObjectFlag(ObjectFlag.OFF);
            AttachParticles("sp-Invisibility", cousin1);
            var cousin2 = Utilities.find_npc_near(attachee, 14652);
            cousin2.SetObjectFlag(ObjectFlag.OFF);
            AttachParticles("sp-Invisibility", cousin2);
            var cousin3 = Utilities.find_npc_near(attachee, 14652);
            cousin3.SetObjectFlag(ObjectFlag.OFF);
            AttachParticles("sp-Invisibility", cousin3);
            var cousin4 = Utilities.find_npc_near(attachee, 14652);
            cousin4.SetObjectFlag(ObjectFlag.OFF);
            AttachParticles("sp-Invisibility", cousin4);
            var cousin5 = Utilities.find_npc_near(attachee, 14652);
            cousin5.SetObjectFlag(ObjectFlag.OFF);
            AttachParticles("sp-Invisibility", cousin5);
            var cousin6 = Utilities.find_npc_near(attachee, 14652);
            cousin6.SetObjectFlag(ObjectFlag.OFF);
            AttachParticles("sp-Invisibility", cousin6);
            var cousin7 = Utilities.find_npc_near(attachee, 14652);
            cousin7.SetObjectFlag(ObjectFlag.OFF);
            AttachParticles("sp-Invisibility", cousin7);
            var cousin8 = Utilities.find_npc_near(attachee, 14652);
            cousin8.SetObjectFlag(ObjectFlag.OFF);
            AttachParticles("sp-Invisibility", cousin8);
            Sound(4031, 1);
            return RunDefault;
        }
        public static void pass_out_1_a(GameObjectBody attachee, GameObjectBody triggerer)
        {
            Fade(43200, 4119, 0, 12);
            Sound(4119, 1);
            StartTimer(12000, () => start_snowing()); // 12 seconds
            if ((GetGlobalVar(945) == 7))
            {
                SetGlobalVar(945, 10);
                if ((attachee.GetMap() == 5071 || attachee.GetMap() == 5075))
                {
                    StartTimer(14000, () => spawn_cousins_circle_forest()); // 14 seconds
                }
                else if ((attachee.GetMap() == 5072 || attachee.GetMap() == 5073 || attachee.GetMap() == 5076 || attachee.GetMap() == 5077))
                {
                    StartTimer(14000, () => spawn_cousins_circle_swamp_river()); // 14 seconds
                }

            }
            else if ((GetGlobalVar(945) == 8))
            {
                SetGlobalVar(945, 11);
                if ((attachee.GetMap() == 5071 || attachee.GetMap() == 5075))
                {
                    StartTimer(14000, () => spawn_cousins_circle_forest()); // 14 seconds
                }
                else if ((attachee.GetMap() == 5072 || attachee.GetMap() == 5073 || attachee.GetMap() == 5076 || attachee.GetMap() == 5077))
                {
                    StartTimer(14000, () => spawn_cousins_circle_swamp_river()); // 14 seconds
                }

            }
            else if ((GetGlobalVar(945) == 9))
            {
                SetGlobalVar(945, 12);
                if ((attachee.GetMap() == 5071 || attachee.GetMap() == 5075))
                {
                    StartTimer(14000, () => spawn_cousins_circle_forest()); // 14 seconds
                }
                else if ((attachee.GetMap() == 5072 || attachee.GetMap() == 5073 || attachee.GetMap() == 5076 || attachee.GetMap() == 5077))
                {
                    StartTimer(14000, () => spawn_cousins_circle_swamp_river()); // 14 seconds
                }

            }

            return;
        }
        public static void pass_out_1_b(GameObjectBody attachee, GameObjectBody triggerer)
        {
            Fade(43200, 4117, 0, 12);
            Sound(4117, 1);
            StartTimer(12000, () => start_raining()); // 12 seconds
            if ((GetGlobalVar(945) == 7))
            {
                SetGlobalVar(945, 10);
                if ((attachee.GetMap() == 5071 || attachee.GetMap() == 5075))
                {
                    StartTimer(14000, () => spawn_cousins_circle_forest()); // 14 seconds
                }
                else if ((attachee.GetMap() == 5072 || attachee.GetMap() == 5073 || attachee.GetMap() == 5076 || attachee.GetMap() == 5077))
                {
                    StartTimer(14000, () => spawn_cousins_circle_swamp_river()); // 14 seconds
                }

            }
            else if ((GetGlobalVar(945) == 8))
            {
                SetGlobalVar(945, 11);
                if ((attachee.GetMap() == 5071 || attachee.GetMap() == 5075))
                {
                    StartTimer(14000, () => spawn_cousins_circle_forest()); // 14 seconds
                }
                else if ((attachee.GetMap() == 5072 || attachee.GetMap() == 5073 || attachee.GetMap() == 5076 || attachee.GetMap() == 5077))
                {
                    StartTimer(14000, () => spawn_cousins_circle_swamp_river()); // 14 seconds
                }

            }
            else if ((GetGlobalVar(945) == 9))
            {
                SetGlobalVar(945, 12);
                if ((attachee.GetMap() == 5071 || attachee.GetMap() == 5075))
                {
                    StartTimer(14000, () => spawn_cousins_circle_forest()); // 14 seconds
                }
                else if ((attachee.GetMap() == 5072 || attachee.GetMap() == 5073 || attachee.GetMap() == 5076 || attachee.GetMap() == 5077))
                {
                    StartTimer(14000, () => spawn_cousins_circle_swamp_river()); // 14 seconds
                }

            }

            return;
        }
        public static void start_snowing()
        {
            SpawnParticles("ef-air node", new locXY(460, 460));
            SpawnParticles("ef-air node", new locXY(500, 500));
            SpawnParticles("ef-air node", new locXY(500, 460));
            SpawnParticles("ef-air node", new locXY(460, 500));
            return;
        }
        public static void start_raining()
        {
            SpawnParticles("Rain Test", new locXY(460, 460));
            SpawnParticles("Rain Test", new locXY(500, 500));
            SpawnParticles("Rain Test", new locXY(500, 460));
            SpawnParticles("Rain Test", new locXY(460, 500));
            return;
        }
        public static void spawn_cousins_circle_forest()
        {
            var cousin1 = GameSystems.MapObject.CreateObject(14651, new locXY(472, 472));
            cousin1.TurnTowards(PartyLeader);
            cousin1.SetConcealed(true);
            cousin1.Unconceal();
            cousin1.ClearNpcFlag(NpcFlag.KOS);
            var cousin2 = GameSystems.MapObject.CreateObject(14652, new locXY(488, 488));
            cousin2.TurnTowards(PartyLeader);
            cousin2.SetConcealed(true);
            cousin2.Unconceal();
            cousin2.ClearNpcFlag(NpcFlag.KOS);
            var cousin3 = GameSystems.MapObject.CreateObject(14652, new locXY(488, 472));
            cousin3.TurnTowards(PartyLeader);
            cousin3.SetConcealed(true);
            cousin3.Unconceal();
            cousin3.ClearNpcFlag(NpcFlag.KOS);
            var cousin4 = GameSystems.MapObject.CreateObject(14652, new locXY(472, 488));
            cousin4.TurnTowards(PartyLeader);
            cousin4.SetConcealed(true);
            cousin4.Unconceal();
            cousin4.ClearNpcFlag(NpcFlag.KOS);
            var cousin5 = GameSystems.MapObject.CreateObject(14652, new locXY(480, 470));
            cousin5.TurnTowards(PartyLeader);
            cousin5.SetConcealed(true);
            cousin5.Unconceal();
            cousin5.ClearNpcFlag(NpcFlag.KOS);
            var cousin6 = GameSystems.MapObject.CreateObject(14652, new locXY(480, 490));
            cousin6.TurnTowards(PartyLeader);
            cousin6.SetConcealed(true);
            cousin6.Unconceal();
            cousin6.ClearNpcFlag(NpcFlag.KOS);
            var cousin7 = GameSystems.MapObject.CreateObject(14652, new locXY(490, 480));
            cousin7.TurnTowards(PartyLeader);
            cousin7.SetConcealed(true);
            cousin7.Unconceal();
            cousin7.ClearNpcFlag(NpcFlag.KOS);
            var cousin8 = GameSystems.MapObject.CreateObject(14652, new locXY(470, 480));
            cousin8.TurnTowards(PartyLeader);
            cousin8.SetConcealed(true);
            cousin8.Unconceal();
            cousin8.ClearNpcFlag(NpcFlag.KOS);
            if ((GetGlobalVar(945) == 10))
            {
                SetGlobalVar(945, 13);
            }
            else if ((GetGlobalVar(945) == 11))
            {
                SetGlobalVar(945, 14);
            }
            else if ((GetGlobalVar(945) == 12))
            {
                SetGlobalVar(945, 15);
            }

            StartTimer(4000, () => pass_out_2_a()); // 4 seconds
            return;
        }
        public static void spawn_cousins_circle_swamp_river()
        {
            var cousin1 = GameSystems.MapObject.CreateObject(14651, new locXY(472, 472));
            cousin1.TurnTowards(PartyLeader);
            cousin1.SetConcealed(true);
            cousin1.Unconceal();
            cousin1.ClearNpcFlag(NpcFlag.KOS);
            var cousin2 = GameSystems.MapObject.CreateObject(14652, new locXY(488, 488));
            cousin2.TurnTowards(PartyLeader);
            cousin2.SetConcealed(true);
            cousin2.Unconceal();
            cousin2.ClearNpcFlag(NpcFlag.KOS);
            var cousin3 = GameSystems.MapObject.CreateObject(14652, new locXY(488, 472));
            cousin3.TurnTowards(PartyLeader);
            cousin3.SetConcealed(true);
            cousin3.Unconceal();
            cousin3.ClearNpcFlag(NpcFlag.KOS);
            var cousin4 = GameSystems.MapObject.CreateObject(14652, new locXY(472, 488));
            cousin4.TurnTowards(PartyLeader);
            cousin4.SetConcealed(true);
            cousin4.Unconceal();
            cousin4.ClearNpcFlag(NpcFlag.KOS);
            var cousin5 = GameSystems.MapObject.CreateObject(14652, new locXY(480, 470));
            cousin5.TurnTowards(PartyLeader);
            cousin5.SetConcealed(true);
            cousin5.Unconceal();
            cousin5.ClearNpcFlag(NpcFlag.KOS);
            var cousin6 = GameSystems.MapObject.CreateObject(14652, new locXY(480, 490));
            cousin6.TurnTowards(PartyLeader);
            cousin6.SetConcealed(true);
            cousin6.Unconceal();
            cousin6.ClearNpcFlag(NpcFlag.KOS);
            var cousin7 = GameSystems.MapObject.CreateObject(14652, new locXY(490, 480));
            cousin7.TurnTowards(PartyLeader);
            cousin7.SetConcealed(true);
            cousin7.Unconceal();
            cousin7.ClearNpcFlag(NpcFlag.KOS);
            var cousin8 = GameSystems.MapObject.CreateObject(14652, new locXY(470, 480));
            cousin8.TurnTowards(PartyLeader);
            cousin8.SetConcealed(true);
            cousin8.Unconceal();
            cousin8.ClearNpcFlag(NpcFlag.KOS);
            if ((GetGlobalVar(945) == 10))
            {
                SetGlobalVar(945, 13);
            }
            else if ((GetGlobalVar(945) == 11))
            {
                SetGlobalVar(945, 14);
            }
            else if ((GetGlobalVar(945) == 12))
            {
                SetGlobalVar(945, 15);
            }

            StartTimer(4000, () => pass_out_2_b()); // 4 seconds
            return;
        }
        public static void pass_out_2_a()
        {
            Fade(21600, 0, 0, 6);
            if ((GetGlobalVar(945) == 13))
            {
                SetGlobalVar(945, 16);
                StartTimer(6000, () => spawn_boss_forest()); // 6 seconds
            }
            else if ((GetGlobalVar(945) == 14))
            {
                SetGlobalVar(945, 17);
                StartTimer(6000, () => spawn_boss_forest()); // 6 seconds
            }
            else if ((GetGlobalVar(945) == 15))
            {
                SetGlobalVar(945, 18);
                StartTimer(6000, () => spawn_boss_forest()); // 6 seconds
            }

            return;
        }
        public static void pass_out_2_b()
        {
            Fade(21600, 0, 0, 6);
            if ((GetGlobalVar(945) == 13))
            {
                SetGlobalVar(945, 16);
                StartTimer(6000, () => spawn_boss_swamp_river()); // 6 seconds
            }
            else if ((GetGlobalVar(945) == 14))
            {
                SetGlobalVar(945, 17);
                StartTimer(6000, () => spawn_boss_swamp_river()); // 6 seconds
            }
            else if ((GetGlobalVar(945) == 15))
            {
                SetGlobalVar(945, 18);
                StartTimer(6000, () => spawn_boss_swamp_river()); // 6 seconds
            }

            return;
        }
        public static void strip_forest(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_PC))
            {
                unequip_forest(3, obj);
                unequip_forest(5, obj);
            }

            foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((obj.GetNameId() == 8000 || obj.GetNameId() == 8001 || obj.GetNameId() == 8002 || obj.GetNameId() == 8003 || obj.GetNameId() == 8004 || obj.GetNameId() == 8005 || obj.GetNameId() == 8010 || obj.GetNameId() == 8014 || obj.GetNameId() == 8015 || obj.GetNameId() == 8020 || obj.GetNameId() == 8021 || obj.GetNameId() == 8022 || obj.GetNameId() == 8023 || obj.GetNameId() == 8025 || obj.GetNameId() == 8026 || obj.GetNameId() == 8029 || obj.GetNameId() == 8030 || obj.GetNameId() == 8031 || obj.GetNameId() == 8034 || obj.GetNameId() == 8039 || obj.GetNameId() == 8040 || obj.GetNameId() == 8050 || obj.GetNameId() == 8054 || obj.GetNameId() == 8056 || obj.GetNameId() == 8057 || obj.GetNameId() == 8058 || obj.GetNameId() == 8060 || obj.GetNameId() == 8061 || obj.GetNameId() == 8062 || obj.GetNameId() == 8067 || obj.GetNameId() == 8069 || obj.GetNameId() == 8070 || obj.GetNameId() == 8071 || obj.GetNameId() == 8072 || obj.GetNameId() == 8714 || obj.GetNameId() == 8716 || obj.GetNameId() == 8717 || obj.GetNameId() == 8718 || obj.GetNameId() == 8730))
                {
                    unequip_forest(3, obj);
                    unequip_forest(5, obj);
                }

            }

            return;
        }
        public static void unequip_forest(FIXME slot, GameObjectBody npc)
        {
            // doesn't work for npcs with no transfer flag on items
            var random_x = RandomRange(513, 528);
            var random_y = RandomRange(477, 492);
            var container = GameSystems.MapObject.CreateObject(1003, new locXY(random_x, random_y));
            container.Rotation = 1f;
            var item = npc.ItemWornAt(slot);
            container.GetItem(item);
            // npc.item_get(item)
            // container.destroy()
            return;
        }
        public static void strip_swamp(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_PC))
            {
                unequip_swamp(3, obj);
                unequip_swamp(5, obj);
            }

            foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((obj.GetNameId() == 8000 || obj.GetNameId() == 8001 || obj.GetNameId() == 8002 || obj.GetNameId() == 8003 || obj.GetNameId() == 8004 || obj.GetNameId() == 8005 || obj.GetNameId() == 8010 || obj.GetNameId() == 8014 || obj.GetNameId() == 8015 || obj.GetNameId() == 8020 || obj.GetNameId() == 8021 || obj.GetNameId() == 8022 || obj.GetNameId() == 8023 || obj.GetNameId() == 8025 || obj.GetNameId() == 8026 || obj.GetNameId() == 8029 || obj.GetNameId() == 8030 || obj.GetNameId() == 8031 || obj.GetNameId() == 8034 || obj.GetNameId() == 8039 || obj.GetNameId() == 8040 || obj.GetNameId() == 8050 || obj.GetNameId() == 8054 || obj.GetNameId() == 8056 || obj.GetNameId() == 8057 || obj.GetNameId() == 8058 || obj.GetNameId() == 8060 || obj.GetNameId() == 8061 || obj.GetNameId() == 8062 || obj.GetNameId() == 8067 || obj.GetNameId() == 8069 || obj.GetNameId() == 8070 || obj.GetNameId() == 8071 || obj.GetNameId() == 8072 || obj.GetNameId() == 8714 || obj.GetNameId() == 8716 || obj.GetNameId() == 8717 || obj.GetNameId() == 8718 || obj.GetNameId() == 8730))
                {
                    unequip_swamp(3, obj);
                    unequip_swamp(5, obj);
                }

            }

            return;
        }
        public static void unequip_swamp(FIXME slot, GameObjectBody npc)
        {
            // doesn't work for npcs with no transfer flag on items
            var random_x = RandomRange(467, 478);
            var random_y = RandomRange(444, 455);
            var container = GameSystems.MapObject.CreateObject(1003, new locXY(random_x, random_y));
            container.Rotation = 2.5f;
            var item = npc.ItemWornAt(slot);
            container.GetItem(item);
            // npc.item_get(item)
            // container.destroy()
            return;
        }
        public static void strip_river(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_PC))
            {
                unequip_river(3, obj);
                unequip_river(5, obj);
            }

            foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((obj.GetNameId() == 8000 || obj.GetNameId() == 8001 || obj.GetNameId() == 8002 || obj.GetNameId() == 8003 || obj.GetNameId() == 8004 || obj.GetNameId() == 8005 || obj.GetNameId() == 8010 || obj.GetNameId() == 8014 || obj.GetNameId() == 8015 || obj.GetNameId() == 8020 || obj.GetNameId() == 8021 || obj.GetNameId() == 8022 || obj.GetNameId() == 8023 || obj.GetNameId() == 8025 || obj.GetNameId() == 8026 || obj.GetNameId() == 8029 || obj.GetNameId() == 8030 || obj.GetNameId() == 8031 || obj.GetNameId() == 8034 || obj.GetNameId() == 8039 || obj.GetNameId() == 8040 || obj.GetNameId() == 8050 || obj.GetNameId() == 8054 || obj.GetNameId() == 8056 || obj.GetNameId() == 8057 || obj.GetNameId() == 8058 || obj.GetNameId() == 8060 || obj.GetNameId() == 8061 || obj.GetNameId() == 8062 || obj.GetNameId() == 8067 || obj.GetNameId() == 8069 || obj.GetNameId() == 8070 || obj.GetNameId() == 8071 || obj.GetNameId() == 8072 || obj.GetNameId() == 8714 || obj.GetNameId() == 8716 || obj.GetNameId() == 8717 || obj.GetNameId() == 8718 || obj.GetNameId() == 8730))
                {
                    unequip_river(3, obj);
                    unequip_river(5, obj);
                }

            }

            return;
        }
        public static void unequip_river(FIXME slot, GameObjectBody npc)
        {
            // doesn't work for npcs with no transfer flag on items
            var random_x = RandomRange(469, 484);
            var random_y = RandomRange(511, 526);
            var container = GameSystems.MapObject.CreateObject(1049, new locXY(random_x, random_y));
            container.Rotation = 5.5f;
            var item = npc.ItemWornAt(slot);
            container.GetItem(item);
            // npc.item_get(item)
            // container.destroy()
            return;
        }
        public static void spawn_boss_forest()
        {
            var cousin9 = GameSystems.MapObject.CreateObject(14651, new locXY(475, 475));
            cousin9.TurnTowards(PartyLeader);
            cousin9.ClearNpcFlag(NpcFlag.KOS);
            var wind_sound_1 = GameSystems.MapObject.CreateObject(2145, new locXY(460, 460));
            var wind_sound_2 = GameSystems.MapObject.CreateObject(2145, new locXY(500, 500));
            var wind_sound_3 = GameSystems.MapObject.CreateObject(2145, new locXY(500, 460));
            var wind_sound_4 = GameSystems.MapObject.CreateObject(2145, new locXY(460, 500));
            if ((GetGlobalVar(945) == 16))
            {
                SetGlobalVar(945, 19);
            }
            else if ((GetGlobalVar(945) == 17))
            {
                SetGlobalVar(945, 20);
            }
            else if ((GetGlobalVar(945) == 18))
            {
                SetGlobalVar(945, 21);
            }

            return;
        }
        public static void spawn_boss_swamp_river()
        {
            var cousin9 = GameSystems.MapObject.CreateObject(14651, new locXY(475, 475));
            cousin9.TurnTowards(PartyLeader);
            cousin9.ClearNpcFlag(NpcFlag.KOS);
            var wind_sound_1 = GameSystems.MapObject.CreateObject(2144, new locXY(460, 460));
            var wind_sound_2 = GameSystems.MapObject.CreateObject(2144, new locXY(500, 500));
            var wind_sound_3 = GameSystems.MapObject.CreateObject(2144, new locXY(500, 460));
            var wind_sound_4 = GameSystems.MapObject.CreateObject(2144, new locXY(460, 500));
            if ((GetGlobalVar(945) == 16))
            {
                SetGlobalVar(945, 19);
            }
            else if ((GetGlobalVar(945) == 17))
            {
                SetGlobalVar(945, 20);
            }
            else if ((GetGlobalVar(945) == 18))
            {
                SetGlobalVar(945, 21);
            }

            return;
        }
        public static void spawn_attackers_for_snitch(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5071 || attachee.GetMap() == 5075)) // forests
            {
                var tyrant1 = GameSystems.MapObject.CreateObject(14648, new locXY(495, 480));
                tyrant1.TurnTowards(PartyLeader);
                tyrant1.SetConcealed(true);
                tyrant1.Unconceal();
                tyrant1.Attack(triggerer);
                var tyrant2 = GameSystems.MapObject.CreateObject(14648, new locXY(465, 480));
                tyrant2.TurnTowards(PartyLeader);
                tyrant2.SetConcealed(true);
                tyrant2.Unconceal();
                tyrant2.Attack(triggerer);
                var tyrant3 = GameSystems.MapObject.CreateObject(14648, new locXY(480, 465));
                tyrant3.TurnTowards(PartyLeader);
                tyrant3.SetConcealed(true);
                tyrant3.Unconceal();
                tyrant3.Attack(triggerer);
                var tyrant4 = GameSystems.MapObject.CreateObject(14648, new locXY(480, 495));
                tyrant4.TurnTowards(PartyLeader);
                tyrant4.SetConcealed(true);
                tyrant4.Unconceal();
                tyrant4.Attack(triggerer);
                var umberhulk = GameSystems.MapObject.CreateObject(14260, new locXY(520, 484));
                umberhulk.TurnTowards(PartyLeader);
                var ogre1 = GameSystems.MapObject.CreateObject(14689, new locXY(472, 465));
                ogre1.TurnTowards(PartyLeader);
                ogre1.SetConcealed(true);
                ogre1.Unconceal();
                ogre1.Attack(triggerer);
                var ogre2 = GameSystems.MapObject.CreateObject(14689, new locXY(465, 472));
                ogre2.TurnTowards(PartyLeader);
                ogre2.SetConcealed(true);
                ogre2.Unconceal();
                ogre2.Attack(triggerer);
                var ogre3 = GameSystems.MapObject.CreateObject(14689, new locXY(495, 488));
                ogre3.TurnTowards(PartyLeader);
                ogre3.SetConcealed(true);
                ogre3.Unconceal();
                ogre3.Attack(triggerer);
                var ogre4 = GameSystems.MapObject.CreateObject(14689, new locXY(488, 495));
                ogre4.TurnTowards(PartyLeader);
                ogre4.SetConcealed(true);
                ogre4.Unconceal();
                ogre4.Attack(triggerer);
                var owlbear3 = GameSystems.MapObject.CreateObject(14693, new locXY(488, 465));
                owlbear3.TurnTowards(PartyLeader);
                owlbear3.SetConcealed(true);
                owlbear3.Unconceal();
                owlbear3.Attack(triggerer);
                var owlbear4 = GameSystems.MapObject.CreateObject(14693, new locXY(495, 472));
                owlbear4.TurnTowards(PartyLeader);
                owlbear4.SetConcealed(true);
                owlbear4.Unconceal();
                owlbear4.Attack(triggerer);
                var hillgiant3 = GameSystems.MapObject.CreateObject(14696, new locXY(465, 488));
                hillgiant3.TurnTowards(PartyLeader);
                hillgiant3.SetConcealed(true);
                hillgiant3.Unconceal();
                hillgiant3.Attack(triggerer);
                var hillgiant4 = GameSystems.MapObject.CreateObject(14696, new locXY(472, 495));
                hillgiant4.TurnTowards(PartyLeader);
                hillgiant4.SetConcealed(true);
                hillgiant4.Unconceal();
                hillgiant4.Attack(triggerer);
            }
            else if ((attachee.GetMap() == 5072 || attachee.GetMap() == 5076)) // swamps
            {
                var tyrant1 = GameSystems.MapObject.CreateObject(14650, new locXY(465, 480));
                tyrant1.TurnTowards(PartyLeader);
                tyrant1.SetConcealed(true);
                tyrant1.Unconceal();
                tyrant1.Attack(triggerer);
                var tyrant2 = GameSystems.MapObject.CreateObject(14650, new locXY(495, 480));
                tyrant2.TurnTowards(PartyLeader);
                tyrant2.SetConcealed(true);
                tyrant2.Unconceal();
                tyrant2.Attack(triggerer);
                var tyrant3 = GameSystems.MapObject.CreateObject(14650, new locXY(480, 465));
                tyrant3.TurnTowards(PartyLeader);
                tyrant3.SetConcealed(true);
                tyrant3.Unconceal();
                tyrant3.Attack(triggerer);
                var tyrant4 = GameSystems.MapObject.CreateObject(14650, new locXY(480, 495));
                tyrant4.TurnTowards(PartyLeader);
                tyrant4.SetConcealed(true);
                tyrant4.Unconceal();
                tyrant4.Attack(triggerer);
                var kingfrog = GameSystems.MapObject.CreateObject(14445, new locXY(472, 449));
                kingfrog.TurnTowards(PartyLeader);
                var troll1 = GameSystems.MapObject.CreateObject(14691, new locXY(472, 465));
                troll1.TurnTowards(PartyLeader);
                troll1.SetConcealed(true);
                troll1.Unconceal();
                troll1.Attack(triggerer);
                var troll2 = GameSystems.MapObject.CreateObject(14691, new locXY(465, 472));
                troll2.TurnTowards(PartyLeader);
                troll2.SetConcealed(true);
                troll2.Unconceal();
                troll2.Attack(triggerer);
                var giantgar1 = GameSystems.MapObject.CreateObject(14692, new locXY(495, 488));
                giantgar1.TurnTowards(PartyLeader);
                giantgar1.SetConcealed(true);
                giantgar1.Unconceal();
                giantgar1.Attack(triggerer);
                var giantgar2 = GameSystems.MapObject.CreateObject(14692, new locXY(488, 495));
                giantgar2.TurnTowards(PartyLeader);
                giantgar2.SetConcealed(true);
                giantgar2.Unconceal();
                giantgar2.Attack(triggerer);
                var troll3 = GameSystems.MapObject.CreateObject(14691, new locXY(488, 465));
                troll3.TurnTowards(PartyLeader);
                troll3.SetConcealed(true);
                troll3.Unconceal();
                troll3.Attack(triggerer);
                var troll4 = GameSystems.MapObject.CreateObject(14691, new locXY(495, 472));
                troll4.TurnTowards(PartyLeader);
                troll4.SetConcealed(true);
                troll4.Unconceal();
                troll4.Attack(triggerer);
                var ettin3 = GameSystems.MapObject.CreateObject(14697, new locXY(465, 488));
                ettin3.TurnTowards(PartyLeader);
                ettin3.SetConcealed(true);
                ettin3.Unconceal();
                ettin3.Attack(triggerer);
                var ettin4 = GameSystems.MapObject.CreateObject(14697, new locXY(472, 495));
                ettin4.TurnTowards(PartyLeader);
                ettin4.SetConcealed(true);
                ettin4.Unconceal();
                ettin4.Attack(triggerer);
            }
            else if ((attachee.GetMap() == 5073 || attachee.GetMap() == 5077)) // rivers
            {
                var tyrant1 = GameSystems.MapObject.CreateObject(14649, new locXY(465, 480));
                tyrant1.TurnTowards(PartyLeader);
                tyrant1.SetConcealed(true);
                tyrant1.Unconceal();
                tyrant1.Attack(triggerer);
                var tyrant2 = GameSystems.MapObject.CreateObject(14649, new locXY(495, 480));
                tyrant2.TurnTowards(PartyLeader);
                tyrant2.SetConcealed(true);
                tyrant2.Unconceal();
                tyrant2.Attack(triggerer);
                var tyrant3 = GameSystems.MapObject.CreateObject(14649, new locXY(480, 465));
                tyrant3.TurnTowards(PartyLeader);
                tyrant3.SetConcealed(true);
                tyrant3.Unconceal();
                tyrant3.Attack(triggerer);
                var tyrant4 = GameSystems.MapObject.CreateObject(14649, new locXY(480, 495));
                tyrant4.TurnTowards(PartyLeader);
                tyrant4.SetConcealed(true);
                tyrant4.Unconceal();
                tyrant4.Attack(triggerer);
                var vodyanoi = GameSystems.MapObject.CreateObject(14261, new locXY(476, 518));
                vodyanoi.TurnTowards(PartyLeader);
                var merrow1 = GameSystems.MapObject.CreateObject(14690, new locXY(472, 465));
                merrow1.TurnTowards(PartyLeader);
                merrow1.SetConcealed(true);
                merrow1.Unconceal();
                merrow1.Attack(triggerer);
                var merrow2 = GameSystems.MapObject.CreateObject(14690, new locXY(465, 472));
                merrow2.TurnTowards(PartyLeader);
                merrow2.SetConcealed(true);
                merrow2.Unconceal();
                merrow2.Attack(triggerer);
                var merrow3 = GameSystems.MapObject.CreateObject(14690, new locXY(495, 488));
                merrow3.TurnTowards(PartyLeader);
                merrow3.SetConcealed(true);
                merrow3.Unconceal();
                merrow3.Attack(triggerer);
                var merrow4 = GameSystems.MapObject.CreateObject(14690, new locXY(488, 495));
                merrow4.TurnTowards(PartyLeader);
                merrow4.SetConcealed(true);
                merrow4.Unconceal();
                merrow4.Attack(triggerer);
                var stonegiant3 = GameSystems.MapObject.CreateObject(14695, new locXY(488, 465));
                stonegiant3.TurnTowards(PartyLeader);
                stonegiant3.SetConcealed(true);
                stonegiant3.Unconceal();
                stonegiant3.Attack(triggerer);
                var stonegiant4 = GameSystems.MapObject.CreateObject(14695, new locXY(495, 472));
                stonegiant4.TurnTowards(PartyLeader);
                stonegiant4.SetConcealed(true);
                stonegiant4.Unconceal();
                stonegiant4.Attack(triggerer);
                var crawler3 = GameSystems.MapObject.CreateObject(14694, new locXY(465, 488));
                crawler3.TurnTowards(PartyLeader);
                crawler3.SetConcealed(true);
                crawler3.Unconceal();
                crawler3.Attack(triggerer);
                var crawler4 = GameSystems.MapObject.CreateObject(14694, new locXY(472, 495));
                crawler4.TurnTowards(PartyLeader);
                crawler4.SetConcealed(true);
                crawler4.Unconceal();
                crawler4.Attack(triggerer);
            }

            SetGlobalVar(945, 28);
            return;
        }
        public static void spawn_attackers_for_narc(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5071 || attachee.GetMap() == 5075)) // forests
            {
                var tyrant1 = GameSystems.MapObject.CreateObject(14648, new locXY(495, 480));
                tyrant1.TurnTowards(PartyLeader);
                tyrant1.SetConcealed(true);
                tyrant1.Unconceal();
                tyrant1.Attack(triggerer);
                var tyrant2 = GameSystems.MapObject.CreateObject(14648, new locXY(465, 480));
                tyrant2.TurnTowards(PartyLeader);
                tyrant2.SetConcealed(true);
                tyrant2.Unconceal();
                tyrant2.Attack(triggerer);
                var tyrant3 = GameSystems.MapObject.CreateObject(14648, new locXY(480, 465));
                tyrant3.TurnTowards(PartyLeader);
                tyrant3.SetConcealed(true);
                tyrant3.Unconceal();
                tyrant3.Attack(triggerer);
                var tyrant4 = GameSystems.MapObject.CreateObject(14648, new locXY(480, 495));
                tyrant4.TurnTowards(PartyLeader);
                tyrant4.SetConcealed(true);
                tyrant4.Unconceal();
                tyrant4.Attack(triggerer);
                var umberhulk = GameSystems.MapObject.CreateObject(14260, new locXY(520, 484));
                umberhulk.TurnTowards(PartyLeader);
                var ogre1 = GameSystems.MapObject.CreateObject(14689, new locXY(472, 465));
                ogre1.TurnTowards(PartyLeader);
                ogre1.SetConcealed(true);
                ogre1.Unconceal();
                ogre1.Attack(triggerer);
                var ogre2 = GameSystems.MapObject.CreateObject(14689, new locXY(465, 472));
                ogre2.TurnTowards(PartyLeader);
                ogre2.SetConcealed(true);
                ogre2.Unconceal();
                ogre2.Attack(triggerer);
                var owlbear1 = GameSystems.MapObject.CreateObject(14693, new locXY(495, 488));
                owlbear1.TurnTowards(PartyLeader);
                owlbear1.SetConcealed(true);
                owlbear1.Unconceal();
                owlbear1.Attack(triggerer);
                var owlbear2 = GameSystems.MapObject.CreateObject(14693, new locXY(488, 495));
                owlbear2.TurnTowards(PartyLeader);
                owlbear2.SetConcealed(true);
                owlbear2.Unconceal();
                owlbear2.Attack(triggerer);
                var owlbear3 = GameSystems.MapObject.CreateObject(14693, new locXY(488, 465));
                owlbear3.TurnTowards(PartyLeader);
                owlbear3.SetConcealed(true);
                owlbear3.Unconceal();
                owlbear3.Attack(triggerer);
                var owlbear4 = GameSystems.MapObject.CreateObject(14693, new locXY(495, 472));
                owlbear4.TurnTowards(PartyLeader);
                owlbear4.SetConcealed(true);
                owlbear4.Unconceal();
                owlbear4.Attack(triggerer);
                var hillgiant3 = GameSystems.MapObject.CreateObject(14696, new locXY(465, 488));
                hillgiant3.TurnTowards(PartyLeader);
                hillgiant3.SetConcealed(true);
                hillgiant3.Unconceal();
                hillgiant3.Attack(triggerer);
                var hillgiant4 = GameSystems.MapObject.CreateObject(14696, new locXY(472, 495));
                hillgiant4.TurnTowards(PartyLeader);
                hillgiant4.SetConcealed(true);
                hillgiant4.Unconceal();
                hillgiant4.Attack(triggerer);
            }
            else if ((attachee.GetMap() == 5072 || attachee.GetMap() == 5076)) // swamps
            {
                var tyrant1 = GameSystems.MapObject.CreateObject(14650, new locXY(465, 480));
                tyrant1.TurnTowards(PartyLeader);
                tyrant1.SetConcealed(true);
                tyrant1.Unconceal();
                tyrant1.Attack(triggerer);
                var tyrant2 = GameSystems.MapObject.CreateObject(14650, new locXY(495, 480));
                tyrant2.TurnTowards(PartyLeader);
                tyrant2.SetConcealed(true);
                tyrant2.Unconceal();
                tyrant2.Attack(triggerer);
                var tyrant3 = GameSystems.MapObject.CreateObject(14650, new locXY(480, 465));
                tyrant3.TurnTowards(PartyLeader);
                tyrant3.SetConcealed(true);
                tyrant3.Unconceal();
                tyrant3.Attack(triggerer);
                var tyrant4 = GameSystems.MapObject.CreateObject(14650, new locXY(480, 495));
                tyrant4.TurnTowards(PartyLeader);
                tyrant4.SetConcealed(true);
                tyrant4.Unconceal();
                tyrant4.Attack(triggerer);
                var kingfrog = GameSystems.MapObject.CreateObject(14445, new locXY(472, 449));
                kingfrog.TurnTowards(PartyLeader);
                var troll1 = GameSystems.MapObject.CreateObject(14691, new locXY(472, 465));
                troll1.TurnTowards(PartyLeader);
                troll1.SetConcealed(true);
                troll1.Unconceal();
                troll1.Attack(triggerer);
                var troll2 = GameSystems.MapObject.CreateObject(14691, new locXY(465, 472));
                troll2.TurnTowards(PartyLeader);
                troll2.SetConcealed(true);
                troll2.Unconceal();
                troll2.Attack(triggerer);
                var giantgar1 = GameSystems.MapObject.CreateObject(14692, new locXY(495, 488));
                giantgar1.TurnTowards(PartyLeader);
                giantgar1.SetConcealed(true);
                giantgar1.Unconceal();
                giantgar1.Attack(triggerer);
                var giantgar2 = GameSystems.MapObject.CreateObject(14692, new locXY(488, 495));
                giantgar2.TurnTowards(PartyLeader);
                giantgar2.SetConcealed(true);
                giantgar2.Unconceal();
                giantgar2.Attack(triggerer);
                var ettin3 = GameSystems.MapObject.CreateObject(14697, new locXY(488, 465));
                ettin3.TurnTowards(PartyLeader);
                ettin3.SetConcealed(true);
                ettin3.Unconceal();
                ettin3.Attack(triggerer);
                var ettin4 = GameSystems.MapObject.CreateObject(14697, new locXY(495, 472));
                ettin4.TurnTowards(PartyLeader);
                ettin4.SetConcealed(true);
                ettin4.Unconceal();
                ettin4.Attack(triggerer);
                var giantgar3 = GameSystems.MapObject.CreateObject(14692, new locXY(465, 488));
                giantgar3.TurnTowards(PartyLeader);
                giantgar3.SetConcealed(true);
                giantgar3.Unconceal();
                giantgar3.Attack(triggerer);
                var giantgar4 = GameSystems.MapObject.CreateObject(14692, new locXY(472, 495));
                giantgar4.TurnTowards(PartyLeader);
                giantgar4.SetConcealed(true);
                giantgar4.Unconceal();
                giantgar4.Attack(triggerer);
            }
            else if ((attachee.GetMap() == 5073 || attachee.GetMap() == 5077)) // rivers
            {
                var tyrant1 = GameSystems.MapObject.CreateObject(14649, new locXY(465, 480));
                tyrant1.TurnTowards(PartyLeader);
                tyrant1.SetConcealed(true);
                tyrant1.Unconceal();
                tyrant1.Attack(triggerer);
                var tyrant2 = GameSystems.MapObject.CreateObject(14649, new locXY(495, 480));
                tyrant2.TurnTowards(PartyLeader);
                tyrant2.SetConcealed(true);
                tyrant2.Unconceal();
                tyrant2.Attack(triggerer);
                var tyrant3 = GameSystems.MapObject.CreateObject(14649, new locXY(480, 465));
                tyrant3.TurnTowards(PartyLeader);
                tyrant3.SetConcealed(true);
                tyrant3.Unconceal();
                tyrant3.Attack(triggerer);
                var tyrant4 = GameSystems.MapObject.CreateObject(14649, new locXY(480, 495));
                tyrant4.TurnTowards(PartyLeader);
                tyrant4.SetConcealed(true);
                tyrant4.Unconceal();
                tyrant4.Attack(triggerer);
                var vodyanoi = GameSystems.MapObject.CreateObject(14261, new locXY(476, 518));
                vodyanoi.TurnTowards(PartyLeader);
                var merrow1 = GameSystems.MapObject.CreateObject(14690, new locXY(472, 465));
                merrow1.TurnTowards(PartyLeader);
                merrow1.SetConcealed(true);
                merrow1.Unconceal();
                merrow1.Attack(triggerer);
                var merrow2 = GameSystems.MapObject.CreateObject(14690, new locXY(465, 472));
                merrow2.TurnTowards(PartyLeader);
                merrow2.SetConcealed(true);
                merrow2.Unconceal();
                merrow2.Attack(triggerer);
                var stonegiant1 = GameSystems.MapObject.CreateObject(14695, new locXY(495, 488));
                stonegiant1.TurnTowards(PartyLeader);
                stonegiant1.SetConcealed(true);
                stonegiant1.Unconceal();
                stonegiant1.Attack(triggerer);
                var stonegiant2 = GameSystems.MapObject.CreateObject(14695, new locXY(488, 495));
                stonegiant2.TurnTowards(PartyLeader);
                stonegiant2.SetConcealed(true);
                stonegiant2.Unconceal();
                stonegiant2.Attack(triggerer);
                var crawler1 = GameSystems.MapObject.CreateObject(14694, new locXY(488, 465));
                crawler1.TurnTowards(PartyLeader);
                crawler1.SetConcealed(true);
                crawler1.Unconceal();
                crawler1.Attack(triggerer);
                var crawler2 = GameSystems.MapObject.CreateObject(14694, new locXY(495, 472));
                crawler2.TurnTowards(PartyLeader);
                crawler2.SetConcealed(true);
                crawler2.Unconceal();
                crawler2.Attack(triggerer);
                var crawler3 = GameSystems.MapObject.CreateObject(14694, new locXY(465, 488));
                crawler3.TurnTowards(PartyLeader);
                crawler3.SetConcealed(true);
                crawler3.Unconceal();
                crawler3.Attack(triggerer);
                var crawler4 = GameSystems.MapObject.CreateObject(14694, new locXY(472, 495));
                crawler4.TurnTowards(PartyLeader);
                crawler4.SetConcealed(true);
                crawler4.Unconceal();
                crawler4.Attack(triggerer);
            }

            SetGlobalVar(945, 29);
            return;
        }
        public static void spawn_attackers_for_whistleblower(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5071 || attachee.GetMap() == 5075)) // forests
            {
                var tyrant1 = GameSystems.MapObject.CreateObject(14648, new locXY(495, 480));
                tyrant1.TurnTowards(PartyLeader);
                tyrant1.SetConcealed(true);
                tyrant1.Unconceal();
                tyrant1.Attack(triggerer);
                var tyrant2 = GameSystems.MapObject.CreateObject(14648, new locXY(465, 480));
                tyrant2.TurnTowards(PartyLeader);
                tyrant2.SetConcealed(true);
                tyrant2.Unconceal();
                tyrant2.Attack(triggerer);
                var tyrant3 = GameSystems.MapObject.CreateObject(14648, new locXY(480, 465));
                tyrant3.TurnTowards(PartyLeader);
                tyrant3.SetConcealed(true);
                tyrant3.Unconceal();
                tyrant3.Attack(triggerer);
                var tyrant4 = GameSystems.MapObject.CreateObject(14648, new locXY(480, 495));
                tyrant4.TurnTowards(PartyLeader);
                tyrant4.SetConcealed(true);
                tyrant4.Unconceal();
                tyrant4.Attack(triggerer);
                var umberhulk = GameSystems.MapObject.CreateObject(14260, new locXY(520, 484));
                umberhulk.TurnTowards(PartyLeader);
                var ogre1 = GameSystems.MapObject.CreateObject(14689, new locXY(472, 465));
                ogre1.TurnTowards(PartyLeader);
                ogre1.SetConcealed(true);
                ogre1.Unconceal();
                ogre1.Attack(triggerer);
                var ogre2 = GameSystems.MapObject.CreateObject(14689, new locXY(465, 472));
                ogre2.TurnTowards(PartyLeader);
                ogre2.SetConcealed(true);
                ogre2.Unconceal();
                ogre2.Attack(triggerer);
                var owlbear1 = GameSystems.MapObject.CreateObject(14693, new locXY(495, 488));
                owlbear1.TurnTowards(PartyLeader);
                owlbear1.SetConcealed(true);
                owlbear1.Unconceal();
                owlbear1.Attack(triggerer);
                var owlbear2 = GameSystems.MapObject.CreateObject(14693, new locXY(488, 495));
                owlbear2.TurnTowards(PartyLeader);
                owlbear2.SetConcealed(true);
                owlbear2.Unconceal();
                owlbear2.Attack(triggerer);
                var hillgiant1 = GameSystems.MapObject.CreateObject(14696, new locXY(488, 465));
                hillgiant1.TurnTowards(PartyLeader);
                hillgiant1.SetConcealed(true);
                hillgiant1.Unconceal();
                hillgiant1.Attack(triggerer);
                var hillgiant2 = GameSystems.MapObject.CreateObject(14696, new locXY(495, 472));
                hillgiant2.TurnTowards(PartyLeader);
                hillgiant2.SetConcealed(true);
                hillgiant2.Unconceal();
                hillgiant2.Attack(triggerer);
                var hillgiant3 = GameSystems.MapObject.CreateObject(14696, new locXY(465, 488));
                hillgiant3.TurnTowards(PartyLeader);
                hillgiant3.SetConcealed(true);
                hillgiant3.Unconceal();
                hillgiant3.Attack(triggerer);
                var hillgiant4 = GameSystems.MapObject.CreateObject(14696, new locXY(472, 495));
                hillgiant4.TurnTowards(PartyLeader);
                hillgiant4.SetConcealed(true);
                hillgiant4.Unconceal();
                hillgiant4.Attack(triggerer);
            }
            else if ((attachee.GetMap() == 5072 || attachee.GetMap() == 5076)) // swamps
            {
                var tyrant1 = GameSystems.MapObject.CreateObject(14650, new locXY(465, 480));
                tyrant1.TurnTowards(PartyLeader);
                tyrant1.SetConcealed(true);
                tyrant1.Unconceal();
                tyrant1.Attack(triggerer);
                var tyrant2 = GameSystems.MapObject.CreateObject(14650, new locXY(495, 480));
                tyrant2.TurnTowards(PartyLeader);
                tyrant2.SetConcealed(true);
                tyrant2.Unconceal();
                tyrant2.Attack(triggerer);
                var tyrant3 = GameSystems.MapObject.CreateObject(14650, new locXY(480, 465));
                tyrant3.TurnTowards(PartyLeader);
                tyrant3.SetConcealed(true);
                tyrant3.Unconceal();
                tyrant3.Attack(triggerer);
                var tyrant4 = GameSystems.MapObject.CreateObject(14650, new locXY(480, 495));
                tyrant4.TurnTowards(PartyLeader);
                tyrant4.SetConcealed(true);
                tyrant4.Unconceal();
                tyrant4.Attack(triggerer);
                var kingfrog = GameSystems.MapObject.CreateObject(14445, new locXY(472, 449));
                kingfrog.TurnTowards(PartyLeader);
                var troll1 = GameSystems.MapObject.CreateObject(14691, new locXY(472, 465));
                troll1.TurnTowards(PartyLeader);
                troll1.SetConcealed(true);
                troll1.Unconceal();
                troll1.Attack(triggerer);
                var troll2 = GameSystems.MapObject.CreateObject(14691, new locXY(465, 472));
                troll2.TurnTowards(PartyLeader);
                troll2.SetConcealed(true);
                troll2.Unconceal();
                troll2.Attack(triggerer);
                var giantgar1 = GameSystems.MapObject.CreateObject(14692, new locXY(495, 488));
                giantgar1.TurnTowards(PartyLeader);
                giantgar1.SetConcealed(true);
                giantgar1.Unconceal();
                giantgar1.Attack(triggerer);
                var giantgar2 = GameSystems.MapObject.CreateObject(14692, new locXY(488, 495));
                giantgar2.TurnTowards(PartyLeader);
                giantgar2.SetConcealed(true);
                giantgar2.Unconceal();
                giantgar2.Attack(triggerer);
                var ettin1 = GameSystems.MapObject.CreateObject(14697, new locXY(488, 465));
                ettin1.TurnTowards(PartyLeader);
                ettin1.SetConcealed(true);
                ettin1.Unconceal();
                ettin1.Attack(triggerer);
                var ettin2 = GameSystems.MapObject.CreateObject(14697, new locXY(495, 472));
                ettin2.TurnTowards(PartyLeader);
                ettin2.SetConcealed(true);
                ettin2.Unconceal();
                ettin2.Attack(triggerer);
                var ettin3 = GameSystems.MapObject.CreateObject(14697, new locXY(465, 488));
                ettin3.TurnTowards(PartyLeader);
                ettin3.SetConcealed(true);
                ettin3.Unconceal();
                ettin3.Attack(triggerer);
                var ettin4 = GameSystems.MapObject.CreateObject(14697, new locXY(472, 495));
                ettin4.TurnTowards(PartyLeader);
                ettin4.SetConcealed(true);
                ettin4.Unconceal();
                ettin4.Attack(triggerer);
            }
            else if ((attachee.GetMap() == 5073 || attachee.GetMap() == 5077)) // rivers
            {
                var tyrant1 = GameSystems.MapObject.CreateObject(14649, new locXY(465, 480));
                tyrant1.TurnTowards(PartyLeader);
                tyrant1.SetConcealed(true);
                tyrant1.Unconceal();
                tyrant1.Attack(triggerer);
                var tyrant2 = GameSystems.MapObject.CreateObject(14649, new locXY(495, 480));
                tyrant2.TurnTowards(PartyLeader);
                tyrant2.SetConcealed(true);
                tyrant2.Unconceal();
                tyrant2.Attack(triggerer);
                var tyrant3 = GameSystems.MapObject.CreateObject(14649, new locXY(480, 465));
                tyrant3.TurnTowards(PartyLeader);
                tyrant3.SetConcealed(true);
                tyrant3.Unconceal();
                tyrant3.Attack(triggerer);
                var tyrant4 = GameSystems.MapObject.CreateObject(14649, new locXY(480, 495));
                tyrant4.TurnTowards(PartyLeader);
                tyrant4.SetConcealed(true);
                tyrant4.Unconceal();
                tyrant4.Attack(triggerer);
                var vodyanoi = GameSystems.MapObject.CreateObject(14261, new locXY(476, 518));
                vodyanoi.TurnTowards(PartyLeader);
                var merrow1 = GameSystems.MapObject.CreateObject(14690, new locXY(472, 465));
                merrow1.TurnTowards(PartyLeader);
                merrow1.SetConcealed(true);
                merrow1.Unconceal();
                merrow1.Attack(triggerer);
                var merrow2 = GameSystems.MapObject.CreateObject(14690, new locXY(465, 472));
                merrow2.TurnTowards(PartyLeader);
                merrow2.SetConcealed(true);
                merrow2.Unconceal();
                merrow2.Attack(triggerer);
                var stonegiant1 = GameSystems.MapObject.CreateObject(14695, new locXY(495, 488));
                stonegiant1.TurnTowards(PartyLeader);
                stonegiant1.SetConcealed(true);
                stonegiant1.Unconceal();
                stonegiant1.Attack(triggerer);
                var stonegiant2 = GameSystems.MapObject.CreateObject(14695, new locXY(488, 495));
                stonegiant2.TurnTowards(PartyLeader);
                stonegiant2.SetConcealed(true);
                stonegiant2.Unconceal();
                stonegiant2.Attack(triggerer);
                var stonegiant3 = GameSystems.MapObject.CreateObject(14695, new locXY(488, 465));
                stonegiant3.TurnTowards(PartyLeader);
                stonegiant3.SetConcealed(true);
                stonegiant3.Unconceal();
                stonegiant3.Attack(triggerer);
                var stonegiant4 = GameSystems.MapObject.CreateObject(14695, new locXY(495, 472));
                stonegiant4.TurnTowards(PartyLeader);
                stonegiant4.SetConcealed(true);
                stonegiant4.Unconceal();
                stonegiant4.Attack(triggerer);
                var crawler3 = GameSystems.MapObject.CreateObject(14694, new locXY(465, 488));
                crawler3.TurnTowards(PartyLeader);
                crawler3.SetConcealed(true);
                crawler3.Unconceal();
                crawler3.Attack(triggerer);
                var crawler4 = GameSystems.MapObject.CreateObject(14694, new locXY(472, 495));
                crawler4.TurnTowards(PartyLeader);
                crawler4.SetConcealed(true);
                crawler4.Unconceal();
                crawler4.Attack(triggerer);
            }

            SetGlobalVar(945, 30);
            return;
        }

    }
}
