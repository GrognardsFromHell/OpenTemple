
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

namespace Scripts;

[ObjectScript(77)]
public class Sergeant : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(37) && (GetGlobalFlag(49) || !GetGlobalFlag(48))))
        {
            triggerer.BeginDialog(attachee, 40);
        }
        else if ((GetGlobalFlag(49)))
        {
            triggerer.BeginDialog(attachee, 60);
        }
        else if ((GetGlobalFlag(48)))
        {
            triggerer.BeginDialog(attachee, 50);
        }
        else
        {
            triggerer.BeginDialog(attachee, 1);
        }

        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive()) && (!GetGlobalFlag(363)) && (attachee.GetLeader() == null))
        {
            if ((is_better_to_talk(attachee, PartyLeader)))
            {
                if ((!Utilities.critter_is_unconscious(PartyLeader)))
                {
                    if ((!attachee.HasMet(PartyLeader)))
                    {
                        attachee.TurnTowards(PartyLeader);
                        PartyLeader.BeginDialog(attachee, 1);
                        DetachScript();
                    }
                    else if ((!GetGlobalFlag(49) && GetGlobalFlag(48) && GetGlobalFlag(62)))
                    {
                        attachee.TurnTowards(PartyLeader);
                        PartyLeader.BeginDialog(attachee, 50);
                        DetachScript();
                    }
                    else if ((GetGlobalFlag(49)))
                    {
                        attachee.TurnTowards(PartyLeader);
                        PartyLeader.BeginDialog(attachee, 60);
                        DetachScript();
                    }
                    else
                    {
                        attachee.TurnTowards(PartyLeader);
                        PartyLeader.BeginDialog(attachee, 70);
                        DetachScript();
                    }

                }

            }
            else
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        if ((!attachee.HasMet(obj)))
                        {
                            attachee.TurnTowards(obj);
                            obj.BeginDialog(attachee, 1);
                        }
                        else if ((!GetGlobalFlag(49) && GetGlobalFlag(48) && GetGlobalFlag(62)))
                        {
                            attachee.TurnTowards(obj);
                            obj.BeginDialog(attachee, 50);
                        }
                        else if ((GetGlobalFlag(49)))
                        {
                            attachee.TurnTowards(obj);
                            obj.BeginDialog(attachee, 60);
                        }
                        else
                        {
                            attachee.TurnTowards(obj);
                            obj.BeginDialog(attachee, 70);
                        }

                    }

                }

            }

        }

        return RunDefault;
    }
    public static bool run_off(GameObject attachee, GameObject triggerer)
    {
        var loc = new locXY(526, 569);
        attachee.RunOff(loc);
        return RunDefault;
    }
    public static bool move_pc(GameObject attachee, GameObject triggerer)
    {
        FadeAndTeleport(0, 0, 0, 5005, 537, 545);
        // triggerer.move( location_from_axis( 537, 545 ) )
        return RunDefault;
    }
    public static bool deliver_pc(GameObject attachee, GameObject triggerer)
    {
        triggerer.Move(new locXY(491, 541));
        return RunDefault;
    }
    public static bool is_better_to_talk(GameObject speaker, GameObject listener)
    {
        if ((speaker.HasLineOfSight(listener)))
        {
            if ((speaker.DistanceTo(listener) <= 20))
            {
                return true;
            }

        }

        return false;
    }
    public static void call_leader(GameObject npc, GameObject pc)
    {
        var leader = PartyLeader;
        leader.Move(pc.GetLocation().OffsetTiles(-2, 0));
        leader.BeginDialog(npc, 1);
        return;
    }
    public static void real_time_regroup()
    {
        foreach (var obj in ObjList.ListVicinity(new locXY(512, 549), ObjectListFilter.OLC_NPC))
        {
            var (xx, yy) = obj.GetLocation();
            if (obj.GetNameId() == 14074 && xx > 496 && yy > 544)
            {
                // Corridor guardsmen
                if (xx == 497 && yy == 549)
                {
                    // archer
                    sps(obj, 639);
                    obj.SetInt(obj_f.speed_walk, 1085353216);
                    obj.SetNpcFlag(NpcFlag.WAYPOINTS_DAY);
                    obj.SetNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
                    StartTimer(4100, () => twitch_stop(obj, 3.14f));
                    StartTimer(5600, () => twitch_stop(obj, 3.14f));
                }
                else if (xx == 507 && yy == 549)
                {
                    // swordsman
                    sps(obj, 638);
                    obj.SetInt(obj_f.speed_walk, 1085353216);
                    obj.SetNpcFlag(NpcFlag.WAYPOINTS_DAY);
                    obj.SetNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
                    StartTimer(4200, () => twitch_stop(obj, 2.35f));
                    StartTimer(5500, () => twitch_stop(obj, 2.35f));
                }
                else if (xx == 515 && yy == 548)
                {
                    // spearbearer
                    sps(obj, 637);
                    obj.SetInt(obj_f.speed_walk, 1085353216);
                    obj.SetNpcFlag(NpcFlag.WAYPOINTS_DAY);
                    obj.SetNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
                    StartTimer(4300, () => twitch_stop(obj, 4));
                    StartTimer(5400, () => twitch_stop(obj, 4));
                }

            }

        }

    }
    public static void twitch_stop(GameObject obj, float rot_new)
    {
        obj.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
        obj.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
        obj.Rotation = rot_new;
    }
    // 495, 534, rot = 2.35 - melee guy
    // 481, 530, rot = 3.14 - archer
    // 483, 541, rot = 4 - spear

    public static void sps(GameObject object_to_be_transferred, int new_standpoint_ID)
    {
        // standpoint set
        object_to_be_transferred.SetStandpoint(StandPointType.Day, new_standpoint_ID);
        object_to_be_transferred.SetStandpoint(StandPointType.Night, new_standpoint_ID);
        return;
    }

}