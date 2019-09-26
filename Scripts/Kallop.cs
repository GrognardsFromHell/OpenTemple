
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
    [ObjectScript(592)]
    public class Kallop : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5095))
            {
                if ((GetGlobalVar(982) == 1 && GetGlobalVar(986) == 3 && !GetGlobalFlag(569)))
                {
                    // turns on kallop outside cave
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5115))
            {
                if ((GetGlobalVar(982) == 2 && !GetGlobalFlag(569)))
                {
                    // turns on kallop inside cave
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
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

            SetGlobalVar(982, 3);
            SetGlobalFlag(569, true);
            return RunDefault;
        }
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5095) && (GetGlobalVar(986) == 3))
            {
                attachee.Attack(SelectedPartyLeader);
                DetachScript();
                return RunDefault;
            }
            else
            {
                return SkipDefault;
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5095))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    if ((attachee != null && !Utilities.critter_is_unconscious(attachee) && !attachee.D20Query(D20DispatcherKey.QUE_Prone) && attachee.GetLeader() == null))
                    {
                        if ((GetGlobalVar(986) != 3))
                        {
                            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                            {
                                if ((talk_40(attachee, obj)))
                                {
                                    if ((!ScriptDaemon.npc_get(attachee, 1)))
                                    {
                                        attachee.TurnTowards(PartyLeader);
                                        PartyLeader.BeginDialog(attachee, 1);
                                        ScriptDaemon.npc_set(attachee, 1);
                                    }

                                }
                                else if ((comment_20(attachee, obj)))
                                {
                                    if ((!ScriptDaemon.npc_get(attachee, 3)))
                                    {
                                        var comment = RandomRange(1, 400);
                                        if ((comment == 20))
                                        {
                                            attachee.FloatLine(10000, triggerer);
                                        }

                                        if ((comment == 60))
                                        {
                                            attachee.FloatLine(10001, triggerer);
                                        }

                                        if ((comment == 100))
                                        {
                                            attachee.FloatLine(10002, triggerer);
                                        }

                                        if ((comment == 140))
                                        {
                                            attachee.FloatLine(10003, triggerer);
                                        }

                                        if ((comment == 180))
                                        {
                                            attachee.FloatLine(10004, triggerer);
                                        }

                                        if ((comment == 220))
                                        {
                                            attachee.FloatLine(10005, triggerer);
                                        }

                                        if ((comment == 260))
                                        {
                                            attachee.FloatLine(10006, triggerer);
                                        }

                                        if ((comment == 300))
                                        {
                                            attachee.FloatLine(10007, triggerer);
                                        }

                                        if ((comment == 340))
                                        {
                                            attachee.FloatLine(10008, triggerer);
                                        }

                                        if ((comment == 380))
                                        {
                                            attachee.FloatLine(10009, triggerer);
                                        }

                                    }

                                }

                            }

                            if ((GetGlobalVar(982) == 1 && !ScriptDaemon.npc_get(attachee, 2)))
                            {
                                StartTimer(200, () => kallop_exit(attachee, triggerer));
                                ScriptDaemon.npc_set(attachee, 2);
                            }

                        }

                    }

                }

                if ((ScriptDaemon.npc_get(attachee, 3)))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }

            return RunDefault;
        }
        public static bool talk_40(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 40))
                {
                    return true;
                }

            }

            return false;
        }
        public static bool comment_20(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.DistanceTo(listener) <= 20))
            {
                return true;
            }

            return false;
        }
        public static bool switch_to_boonthag(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8816);
            if ((npc != null))
            {
                npc.TurnTowards(PartyLeader);
                triggerer.BeginDialog(npc, line);
            }

            return SkipDefault;
        }
        public static bool kallop_exit(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
            attachee.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
            attachee.SetStandpoint(StandPointType.Night, 424);
            attachee.SetStandpoint(StandPointType.Day, 424);
            // attachee.standpoint_set( STANDPOINT_SCOUT, 424 )
            // attachee.obj_set_int(obj_f_speed_walk, 1085353216)
            attachee.RunOff(new locXY(387, 466));
            StartTimer(8000, () => kallop_off(attachee, triggerer));
            return RunDefault;
        }
        public static bool kallop_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            ScriptDaemon.npc_set(attachee, 3);
            return RunDefault;
        }
        public static bool increment_rep(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((PartyLeader.HasReputation(81)))
            {
                PartyLeader.AddReputation(82);
                PartyLeader.RemoveReputation(81);
            }
            else if ((PartyLeader.HasReputation(82)))
            {
                PartyLeader.AddReputation(83);
                PartyLeader.RemoveReputation(82);
            }
            else if ((PartyLeader.HasReputation(83)))
            {
                PartyLeader.AddReputation(84);
                PartyLeader.RemoveReputation(83);
            }
            else if ((PartyLeader.HasReputation(84)))
            {
                PartyLeader.AddReputation(85);
                PartyLeader.RemoveReputation(84);
            }
            else if ((PartyLeader.HasReputation(85)))
            {
                PartyLeader.AddReputation(86);
                PartyLeader.RemoveReputation(85);
            }
            else if ((PartyLeader.HasReputation(86)))
            {
                PartyLeader.AddReputation(87);
                PartyLeader.RemoveReputation(86);
            }
            else if ((PartyLeader.HasReputation(87)))
            {
                PartyLeader.AddReputation(88);
                PartyLeader.RemoveReputation(87);
            }
            else if ((PartyLeader.HasReputation(88)))
            {
                PartyLeader.AddReputation(89);
                PartyLeader.RemoveReputation(88);
            }
            else
            {
                PartyLeader.AddReputation(81);
            }

            return RunDefault;
        }

    }
}
