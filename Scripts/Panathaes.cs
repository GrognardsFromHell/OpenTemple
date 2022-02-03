
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
    [ObjectScript(326)]
    public class Panathaes : BaseObjectScript
    {
        public override bool OnUse(GameObject door, GameObject triggerer)
        {
            if ((door.GetNameId() == 1622))
            {
                if ((!GetGlobalFlag(532)))
                {
                    SetGlobalFlag(534, true);
                    return SkipDefault;
                }
                // if doors to tunnels are locked, disable door portal, flag for attempt and fine
                else if ((GetGlobalFlag(532)))
                {
                    if ((GetGlobalVar(548) <= 2))
                    {
                        SetGlobalVar(548, GetGlobalVar(548) + 1);
                        if ((GetGlobalVar(548) == 3))
                        {
                            PartyLeader.AddReputation(68);
                            SetGlobalVar(548, 4);
                        }

                    }

                    return RunDefault;
                }

            }

            return RunDefault;
        }
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(551) == 2))
            {
                triggerer.BeginDialog(attachee, 270);
            }
            else if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 240);
            }
            else if ((attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 40);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetMap() == 5169 && GetGlobalVar(551) == 2))
            {
                if ((attachee.GetLeader() == null))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5141))
            {
                if ((GetQuestState(109) == QuestState.Completed && GetGlobalVar(551) == 2))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(809, true);
            return RunDefault;
        }
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(538) != 4))
            {
                SetGlobalVar(538, GetGlobalVar(538) + 1);
            }

            if ((GetGlobalVar(538) == 3))
            {
                var leader = PartyLeader;
                Co8.StopCombat(attachee, 0);
                leader.BeginDialog(attachee, 100);
                SetGlobalVar(538, 4);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(809, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetQuestState(109) == QuestState.Mentioned || GetQuestState(109) == QuestState.Accepted))
            {
                if ((GetGlobalVar(536) == 2))
                {
                    if ((attachee.GetNameId() == 8791))
                    {
                        if ((GetGlobalVar(539) == 1))
                        {
                            if ((GetGlobalVar(542) == 1))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11058, attachee);
                            }
                            else if ((GetGlobalVar(542) == 2))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11009, attachee);
                                Utilities.create_item_in_inventory(11062, attachee);
                            }
                            else if ((GetGlobalVar(542) == 3))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11099, attachee);
                            }

                        }

                    }
                    else if ((attachee.GetNameId() == 8792))
                    {
                        if ((GetGlobalVar(539) == 2))
                        {
                            if ((GetGlobalVar(542) == 1))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11058, attachee);
                            }
                            else if ((GetGlobalVar(542) == 2))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11009, attachee);
                                Utilities.create_item_in_inventory(11062, attachee);
                            }
                            else if ((GetGlobalVar(542) == 3))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11099, attachee);
                            }

                        }

                    }
                    else if ((attachee.GetNameId() == 8793))
                    {
                        if ((GetGlobalVar(539) == 3))
                        {
                            if ((GetGlobalVar(542) == 1))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11058, attachee);
                            }
                            else if ((GetGlobalVar(542) == 2))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11009, attachee);
                                Utilities.create_item_in_inventory(11062, attachee);
                            }
                            else if ((GetGlobalVar(542) == 3))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11099, attachee);
                            }

                        }

                    }
                    else if ((attachee.GetNameId() == 8794))
                    {
                        if ((GetGlobalVar(539) == 4))
                        {
                            if ((GetGlobalVar(542) == 1))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11058, attachee);
                            }
                            else if ((GetGlobalVar(542) == 2))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11009, attachee);
                                Utilities.create_item_in_inventory(11062, attachee);
                            }
                            else if ((GetGlobalVar(542) == 3))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11099, attachee);
                            }

                        }

                    }
                    else if ((attachee.GetNameId() == 8795))
                    {
                        if ((GetGlobalVar(539) == 5))
                        {
                            if ((GetGlobalVar(542) == 1))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11058, attachee);
                            }
                            else if ((GetGlobalVar(542) == 2))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11009, attachee);
                                Utilities.create_item_in_inventory(11062, attachee);
                            }
                            else if ((GetGlobalVar(542) == 3))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11099, attachee);
                            }

                        }

                    }
                    else if ((attachee.GetNameId() == 8796))
                    {
                        if ((GetGlobalVar(539) == 6))
                        {
                            if ((GetGlobalVar(542) == 1))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11058, attachee);
                            }
                            else if ((GetGlobalVar(542) == 2))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11009, attachee);
                                Utilities.create_item_in_inventory(11062, attachee);
                            }
                            else if ((GetGlobalVar(542) == 3))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11099, attachee);
                            }

                        }

                    }
                    else if ((attachee.GetNameId() == 8797))
                    {
                        if ((GetGlobalVar(539) == 7))
                        {
                            if ((GetGlobalVar(542) == 1))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11058, attachee);
                            }
                            else if ((GetGlobalVar(542) == 2))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11009, attachee);
                                Utilities.create_item_in_inventory(11062, attachee);
                            }
                            else if ((GetGlobalVar(542) == 3))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11099, attachee);
                            }

                        }

                    }
                    else if ((attachee.GetNameId() == 8798))
                    {
                        if ((GetGlobalVar(539) == 8))
                        {
                            if ((GetGlobalVar(542) == 1))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11058, attachee);
                            }
                            else if ((GetGlobalVar(542) == 2))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11009, attachee);
                                Utilities.create_item_in_inventory(11062, attachee);
                            }
                            else if ((GetGlobalVar(542) == 3))
                            {
                                attachee.ClearObjectFlag(ObjectFlag.OFF);
                                // game.global_vars[539] = 9
                                SetGlobalVar(536, 3);
                                Utilities.create_item_in_inventory(11059, attachee);
                                Utilities.create_item_in_inventory(11099, attachee);
                            }

                        }

                    }

                }
                else if ((GetGlobalVar(536) == 3))
                {
                    if ((GetGlobalVar(539) == 1))
                    {
                        if ((attachee.GetNameId() == 8791))
                        {
                            if ((!GameSystems.Combat.IsCombatActive()))
                            {
                                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                                {
                                    if ((close_enough(attachee, obj)))
                                    {
                                        if ((obj.GetSkillLevel(attachee, SkillId.spot) >= 19))
                                        {
                                            attachee.SetConcealed(false);
                                            StartTimer(2000, () => talk_talk(attachee, obj));
                                            SetGlobalVar(536, 4);
                                        }

                                    }

                                }

                            }

                        }

                    }
                    else if ((GetGlobalVar(539) == 2))
                    {
                        if ((attachee.GetNameId() == 8792))
                        {
                            if ((!GameSystems.Combat.IsCombatActive()))
                            {
                                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                                {
                                    if ((close_enough(attachee, obj)))
                                    {
                                        if ((obj.GetSkillLevel(attachee, SkillId.spot) >= 19))
                                        {
                                            attachee.SetConcealed(false);
                                            StartTimer(2000, () => talk_talk(attachee, obj));
                                            SetGlobalVar(536, 4);
                                        }

                                    }

                                }

                            }

                        }

                    }
                    else if ((GetGlobalVar(539) == 3))
                    {
                        if ((attachee.GetNameId() == 8793))
                        {
                            if ((!GameSystems.Combat.IsCombatActive()))
                            {
                                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                                {
                                    if ((close_enough(attachee, obj)))
                                    {
                                        if ((obj.GetSkillLevel(attachee, SkillId.spot) >= 19))
                                        {
                                            attachee.SetConcealed(false);
                                            StartTimer(2000, () => talk_talk(attachee, obj));
                                            SetGlobalVar(536, 4);
                                        }

                                    }

                                }

                            }

                        }

                    }
                    else if ((GetGlobalVar(539) == 4))
                    {
                        if ((attachee.GetNameId() == 8794))
                        {
                            if ((!GameSystems.Combat.IsCombatActive()))
                            {
                                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                                {
                                    if ((close_enough(attachee, obj)))
                                    {
                                        if ((obj.GetSkillLevel(attachee, SkillId.spot) >= 19))
                                        {
                                            attachee.SetConcealed(false);
                                            StartTimer(2000, () => talk_talk(attachee, obj));
                                            SetGlobalVar(536, 4);
                                        }

                                    }

                                }

                            }

                        }

                    }
                    else if ((GetGlobalVar(539) == 5))
                    {
                        if ((attachee.GetNameId() == 8795))
                        {
                            if ((!GameSystems.Combat.IsCombatActive()))
                            {
                                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                                {
                                    if ((close_enough(attachee, obj)))
                                    {
                                        if ((obj.GetSkillLevel(attachee, SkillId.spot) >= 19))
                                        {
                                            attachee.SetConcealed(false);
                                            StartTimer(2000, () => talk_talk(attachee, obj));
                                            SetGlobalVar(536, 4);
                                        }

                                    }

                                }

                            }

                        }

                    }
                    else if ((GetGlobalVar(539) == 6))
                    {
                        if ((attachee.GetNameId() == 8796))
                        {
                            if ((!GameSystems.Combat.IsCombatActive()))
                            {
                                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                                {
                                    if ((close_enough(attachee, obj)))
                                    {
                                        if ((obj.GetSkillLevel(attachee, SkillId.spot) >= 19))
                                        {
                                            attachee.SetConcealed(false);
                                            StartTimer(2000, () => talk_talk(attachee, obj));
                                            SetGlobalVar(536, 4);
                                        }

                                    }

                                }

                            }

                        }

                    }
                    else if ((GetGlobalVar(539) == 7))
                    {
                        if ((attachee.GetNameId() == 8797))
                        {
                            if ((!GameSystems.Combat.IsCombatActive()))
                            {
                                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                                {
                                    if ((close_enough(attachee, obj)))
                                    {
                                        if ((obj.GetSkillLevel(attachee, SkillId.spot) >= 19))
                                        {
                                            attachee.SetConcealed(false);
                                            StartTimer(2000, () => talk_talk(attachee, obj));
                                            SetGlobalVar(536, 4);
                                        }

                                    }

                                }

                            }

                        }

                    }
                    else if ((GetGlobalVar(539) == 8))
                    {
                        if ((attachee.GetNameId() == 8798))
                        {
                            if ((!GameSystems.Combat.IsCombatActive()))
                            {
                                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                                {
                                    if ((close_enough(attachee, obj)))
                                    {
                                        if ((obj.GetSkillLevel(attachee, SkillId.spot) >= 19))
                                        {
                                            attachee.SetConcealed(false);
                                            StartTimer(2000, () => talk_talk(attachee, obj));
                                            SetGlobalVar(536, 4);
                                        }

                                    }

                                }

                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnDisband(GameObject attachee, GameObject triggerer)
        {
            SetGlobalVar(551, 2);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 50);
            }

            return RunDefault;
        }
        public static bool close_enough(GameObject speaker, GameObject listener)
        {
            if ((speaker.DistanceTo(listener) <= 10))
            {
                return true;
            }

            return false;
        }
        public static bool talk_talk(GameObject attachee, GameObject triggerer)
        {
            attachee.TurnTowards(triggerer);
            triggerer.BeginDialog(attachee, 1);
            return RunDefault;
        }
        public static void increment_var_543(GameObject attachee, GameObject triggerer)
        {
            SetGlobalVar(543, GetGlobalVar(543) + 1);
            return;
        }
        public static void increment_var_544(GameObject attachee, GameObject triggerer)
        {
            SetGlobalVar(544, GetGlobalVar(544) + 1);
            return;
        }
        public static void increment_var_545(GameObject attachee, GameObject triggerer)
        {
            SetGlobalVar(545, GetGlobalVar(545) + 1);
            return;
        }
        public static void go_to_sleep(GameObject attachee, GameObject triggerer)
        {
            attachee.AddCondition("Unconscious", 8200, 0);
            return;
        }

    }
}
