
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
    [ObjectScript(600)]
    public class NaigLliht : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            attachee.TurnTowards(triggerer);
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetMap() == 5095))
            {
                if ((GetGlobalVar(980) == 1 && GetGlobalVar(986) == 3 && !GetGlobalFlag(570)))
                {
                    // turns on naig lliht outside cave
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5115))
            {
                if ((GetGlobalVar(980) == 2 && !GetGlobalFlag(570)))
                {
                    // turns on naig lliht inside cave
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
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

            SetGlobalVar(980, 3);
            SetGlobalFlag(570, true);
            return RunDefault;
        }
        public override bool OnWillKos(GameObject attachee, GameObject triggerer)
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
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
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
                                if ((talk_30(attachee, obj)))
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

                            if ((GetGlobalVar(980) == 1 && !ScriptDaemon.npc_get(attachee, 2)))
                            {
                                StartTimer(200, () => naig_lliht_exit(attachee, triggerer));
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
        public static bool talk_30(GameObject speaker, GameObject listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 30))
                {
                    return true;
                }

            }

            return false;
        }
        public static bool comment_20(GameObject speaker, GameObject listener)
        {
            return speaker.DistanceTo(listener) <= 20;
        }
        public static bool set_alarm(GameObject attachee, GameObject triggerer)
        {
            StartTimer(5000, () => play_alarm(attachee, triggerer));
            return RunDefault;
        }
        public static bool play_alarm(GameObject attachee, GameObject triggerer)
        {
            Sound(4041);
            return RunDefault;
        }
        public static bool naig_lliht_exit(GameObject attachee, GameObject triggerer)
        {
            attachee.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
            attachee.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
            attachee.SetStandpoint(StandPointType.Night, 629);
            attachee.SetStandpoint(StandPointType.Day, 629);
            // attachee.standpoint_set( STANDPOINT_SCOUT, 629 )
            // attachee.obj_set_int(obj_f_speed_walk, 1075353216)
            attachee.RunOff(new locXY(453, 571));
            StartTimer(8000, () => naig_lliht_off(attachee, triggerer));
            return RunDefault;
        }
        public static bool naig_lliht_off(GameObject attachee, GameObject triggerer)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            ScriptDaemon.npc_set(attachee, 3);
            return RunDefault;
        }
        public static bool increment_rep(GameObject attachee, GameObject triggerer)
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
