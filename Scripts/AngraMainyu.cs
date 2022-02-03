
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

[ObjectScript(590)]
public class AngraMainyu : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(triggerer);
        if ((attachee.GetLeader() != null))
        {
            triggerer.TurnTowards(attachee);
            triggerer.BeginDialog(attachee, 55);
        }
        else
        {
            triggerer.TurnTowards(attachee);
            triggerer.BeginDialog(attachee, 1);
        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalVar(980) == 3 && GetGlobalVar(981) == 3 && GetGlobalVar(982) == 3 && GetGlobalVar(983) == 3 && GetGlobalVar(984) == 3 && GetGlobalVar(985) == 3 && GetGlobalVar(986) == 3)) // turns on angra and co
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
            if ((!ScriptDaemon.npc_get(attachee, 4)))
            {
                Sound(4183, 1);
                ScriptDaemon.npc_set(attachee, 4);
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

        if ((attachee.GetNameId() == 8893))
        {
            AttachParticles("Orb-Summon-Glabrezu", attachee);
            SetGlobalFlag(562, true);
            if ((GetGlobalFlag(560) && GetGlobalFlag(561)))
            {
                PartyLeader.AddReputation(62);
            }

            if ((!GetGlobalFlag(564)))
            {
                PartyLeader.AddReputation(90);
            }

            attachee.SetObjectFlag(ObjectFlag.OFF);
            spawn_phylactery();
        }
        else if ((attachee.GetNameId() == 14949))
        {
            AttachParticles("hit-HOLY-medium", attachee);
            SetGlobalFlag(564, true);
            Sound(4184, 1);
            if ((GetGlobalFlag(562)))
            {
                if ((PartyLeader.HasReputation(90)))
                {
                    PartyLeader.RemoveReputation(90);
                }

            }

            attachee.SetObjectFlag(ObjectFlag.OFF);
        }

        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        var webbed = Livonya.break_free(attachee, 3);
        if ((Utilities.obj_percent_hp(attachee) >= 51))
        {
            if ((GetGlobalVar(783) == 1))
            {
                attachee.SetInt(obj_f.critter_strategy, 479);
            }
            else if ((GetGlobalVar(783) == 2))
            {
                attachee.SetInt(obj_f.critter_strategy, 480);
            }
            else if ((GetGlobalVar(783) == 3))
            {
                attachee.SetInt(obj_f.critter_strategy, 481);
            }
            else if ((GetGlobalVar(783) == 4))
            {
                attachee.SetInt(obj_f.critter_strategy, 482);
            }
            else if ((GetGlobalVar(783) == 5))
            {
                attachee.SetInt(obj_f.critter_strategy, 483);
            }
            else if ((GetGlobalVar(783) == 6))
            {
                attachee.SetInt(obj_f.critter_strategy, 484);
            }
            else if ((GetGlobalVar(783) == 7))
            {
                attachee.SetInt(obj_f.critter_strategy, 485);
            }
            else if ((GetGlobalVar(783) == 8))
            {
                attachee.SetInt(obj_f.critter_strategy, 486);
            }
            else if ((GetGlobalVar(783) == 9))
            {
                attachee.SetInt(obj_f.critter_strategy, 487);
            }
            else if ((GetGlobalVar(783) == 10))
            {
                attachee.SetInt(obj_f.critter_strategy, 488);
            }
            else if ((GetGlobalVar(783) == 11))
            {
                attachee.SetInt(obj_f.critter_strategy, 489);
            }
            else if ((GetGlobalVar(783) == 12))
            {
                attachee.SetInt(obj_f.critter_strategy, 490);
            }
            else if ((GetGlobalVar(783) == 13))
            {
                attachee.SetInt(obj_f.critter_strategy, 491);
            }
            else if ((GetGlobalVar(783) == 14))
            {
                attachee.SetInt(obj_f.critter_strategy, 492);
            }
            else if ((GetGlobalVar(783) == 15))
            {
                attachee.SetInt(obj_f.critter_strategy, 493);
            }
            else if ((GetGlobalVar(783) == 16))
            {
                attachee.SetInt(obj_f.critter_strategy, 494);
            }
            else if ((GetGlobalVar(783) == 17))
            {
                attachee.SetInt(obj_f.critter_strategy, 495);
            }
            else if ((GetGlobalVar(783) == 18))
            {
                attachee.SetInt(obj_f.critter_strategy, 496);
            }
            else if ((GetGlobalVar(783) == 19))
            {
                attachee.SetInt(obj_f.critter_strategy, 497);
            }
            else if ((GetGlobalVar(783) >= 20))
            {
                attachee.SetInt(obj_f.critter_strategy, 478);
            }

        }
        else if ((Utilities.obj_percent_hp(attachee) <= 50))
        {
            if ((GetGlobalVar(783) == 19))
            {
                attachee.SetInt(obj_f.critter_strategy, 497);
            }
            else if ((GetGlobalVar(783) == 20))
            {
                attachee.SetInt(obj_f.critter_strategy, 478);
            }
            else if ((GetGlobalVar(783) != 20))
            {
                attachee.SetInt(obj_f.critter_strategy, 486);
            }

        }

        AttachParticles("Trap-Spores", attachee);
        var fx = RandomRange(1, 3);
        if ((fx == 1))
        {
            Sound(4167, 1);
        }
        else if ((fx == 2))
        {
            Sound(4168, 1);
        }
        else if ((fx == 3))
        {
            Sound(4169, 1);
        }

        return RunDefault;
    }
    public override bool OnEndCombat(GameObject attachee, GameObject triggerer)
    {
        if ((Utilities.obj_percent_hp(attachee) >= 51))
        {
            if ((GetGlobalVar(783) == 0))
            {
                SetGlobalVar(783, 1);
            }
            else if ((GetGlobalVar(783) == 1))
            {
                SetGlobalVar(783, 2);
            }
            else if ((GetGlobalVar(783) == 2))
            {
                SetGlobalVar(783, 3);
            }
            else if ((GetGlobalVar(783) == 3))
            {
                SetGlobalVar(783, 4);
            }
            else if ((GetGlobalVar(783) == 4))
            {
                SetGlobalVar(783, 5);
            }
            else if ((GetGlobalVar(783) == 5))
            {
                SetGlobalVar(783, 6);
            }
            else if ((GetGlobalVar(783) == 6))
            {
                SetGlobalVar(783, 7);
            }
            else if ((GetGlobalVar(783) == 7))
            {
                SetGlobalVar(783, 8);
            }
            else if ((GetGlobalVar(783) == 8))
            {
                SetGlobalVar(783, 9);
            }
            else if ((GetGlobalVar(783) == 9))
            {
                SetGlobalVar(783, 10);
            }
            else if ((GetGlobalVar(783) == 10))
            {
                SetGlobalVar(783, 11);
            }
            else if ((GetGlobalVar(783) == 11))
            {
                SetGlobalVar(783, 12);
            }
            else if ((GetGlobalVar(783) == 12))
            {
                SetGlobalVar(783, 13);
            }
            else if ((GetGlobalVar(783) == 13))
            {
                SetGlobalVar(783, 14);
            }
            else if ((GetGlobalVar(783) == 14))
            {
                SetGlobalVar(783, 15);
            }
            else if ((GetGlobalVar(783) == 15))
            {
                SetGlobalVar(783, 16);
            }
            else if ((GetGlobalVar(783) == 16))
            {
                SetGlobalVar(783, 17);
            }
            else if ((GetGlobalVar(783) == 17))
            {
                SetGlobalVar(783, 18);
            }
            else if ((GetGlobalVar(783) == 18))
            {
                SetGlobalVar(783, 19);
            }
            else if ((GetGlobalVar(783) == 19))
            {
                SetGlobalVar(783, 20);
            }

        }
        else if ((Utilities.obj_percent_hp(attachee) <= 50))
        {
            if ((GetGlobalVar(783) == 19))
            {
                SetGlobalVar(783, 20);
            }
            else if ((GetGlobalVar(783) != 20))
            {
                SetGlobalVar(783, 19);
            }

        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetNameId() == 8893))
        {
            if ((attachee.GetLeader() == null))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    if ((GetGlobalVar(980) == 3 && GetGlobalVar(981) == 3 && GetGlobalVar(982) == 3 && GetGlobalVar(983) == 3 && GetGlobalVar(984) == 3 && GetGlobalVar(985) == 3 && GetGlobalVar(986) == 3)) // turns on angra
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                        if ((!ScriptDaemon.npc_get(attachee, 4)))
                        {
                            Sound(4183, 1);
                            ScriptDaemon.npc_set(attachee, 4);
                        }

                    }

                    var closest_jones = Utilities.party_closest(attachee);
                    if ((attachee.DistanceTo(closest_jones) <= 100))
                    {
                        if ((attachee != null && !Utilities.critter_is_unconscious(attachee) && !attachee.D20Query(D20DispatcherKey.QUE_Prone)))
                        {
                            if ((GetGlobalVar(980) == 3 && GetGlobalVar(981) == 3 && GetGlobalVar(982) == 3 && GetGlobalVar(983) == 3 && GetGlobalVar(984) == 3 && GetGlobalVar(985) == 3 && GetGlobalVar(986) == 3 && !GetGlobalFlag(563)))
                            {
                                SetGlobalVar(973, GetGlobalVar(973) + 1);
                                if ((GetGlobalVar(973) == 10 || GetGlobalVar(973) == 20 || GetGlobalVar(973) == 30 || GetGlobalVar(973) == 40))
                                {
                                    AttachParticles("Trap-Spores", attachee);
                                    var fx = RandomRange(1, 4);
                                    if ((fx == 1))
                                    {
                                        Sound(4167, 1);
                                    }
                                    else if ((fx == 2))
                                    {
                                        Sound(4168, 1);
                                    }
                                    else if ((fx == 3))
                                    {
                                        Sound(4169, 1);
                                    }

                                }

                                if ((GetGlobalVar(973) == 40))
                                {
                                    SetGlobalVar(973, 0);
                                }

                            }

                        }

                    }

                    if ((!GetGlobalFlag(984)))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((is_35_and_under(attachee, obj)))
                            {
                                attachee.TurnTowards(PartyLeader);
                                PartyLeader.BeginDialog(attachee, 1);
                                SetGlobalFlag(984, true);
                            }

                        }

                    }

                    if ((GetGlobalFlag(563) && !ScriptDaemon.npc_get(attachee, 2)))
                    {
                        StartTimer(200, () => angra_exit(attachee, triggerer));
                        ScriptDaemon.npc_set(attachee, 2);
                    }

                }

            }

            if ((ScriptDaemon.npc_get(attachee, 3)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

        }
        else if ((attachee.GetNameId() == 8614 || attachee.GetNameId() == 8615 || attachee.GetNameId() == 8616 || attachee.GetNameId() == 8617 || attachee.GetNameId() == 8618 || attachee.GetNameId() == 8619 || attachee.GetNameId() == 8620 || attachee.GetNameId() == 8621))
        {
            if ((attachee.GetLeader() == null))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    if ((GetGlobalVar(980) == 3 && GetGlobalVar(981) == 3 && GetGlobalVar(982) == 3 && GetGlobalVar(983) == 3 && GetGlobalVar(984) == 3 && GetGlobalVar(985) == 3 && GetGlobalVar(986) == 3)) // turns on angra co
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }

                }

            }

        }

        return RunDefault;
    }
    public static bool is_35_and_under(GameObject speaker, GameObject listener)
    {
        if ((speaker.HasLineOfSight(listener)))
        {
            if ((speaker.DistanceTo(listener) <= 35))
            {
                return true;
            }

        }

        return false;
    }
    public static bool angra_exit(GameObject attachee, GameObject triggerer)
    {
        attachee.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
        attachee.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
        attachee.SetStandpoint(StandPointType.Night, 434);
        attachee.SetStandpoint(StandPointType.Day, 434);
        // attachee.standpoint_set( STANDPOINT_SCOUT, 434 )
        attachee.RunOff(new locXY(481, 477));
        var bug1 = Utilities.find_npc_near(attachee, 8614);
        bug1.RunOff(new locXY(481, 477));
        var bug2 = Utilities.find_npc_near(attachee, 8615);
        bug2.RunOff(new locXY(481, 477));
        var ass1 = Utilities.find_npc_near(attachee, 8616);
        ass1.RunOff(new locXY(481, 477));
        var tra1 = Utilities.find_npc_near(attachee, 8617);
        tra1.RunOff(new locXY(481, 477));
        var ogr1 = Utilities.find_npc_near(attachee, 8618);
        ogr1.RunOff(new locXY(481, 477));
        var ett1 = Utilities.find_npc_near(attachee, 8619);
        ett1.RunOff(new locXY(481, 477));
        var sto1 = Utilities.find_npc_near(attachee, 8620);
        sto1.RunOff(new locXY(481, 477));
        var hil1 = Utilities.find_npc_near(attachee, 8621);
        hil1.RunOff(new locXY(481, 477));
        StartTimer(8000, () => angra_off(attachee, triggerer));
        StartTimer(8000, () => bug1_off(bug1, triggerer));
        StartTimer(8000, () => bug2_off(bug2, triggerer));
        StartTimer(8000, () => ass1_off(ass1, triggerer));
        StartTimer(8000, () => tra1_off(tra1, triggerer));
        StartTimer(8000, () => ogr1_off(ogr1, triggerer));
        StartTimer(8000, () => ett1_off(ett1, triggerer));
        StartTimer(8000, () => sto1_off(sto1, triggerer));
        StartTimer(8000, () => hil1_off(hil1, triggerer));
        return RunDefault;
    }
    public static bool angra_off(GameObject attachee, GameObject triggerer)
    {
        attachee.SetObjectFlag(ObjectFlag.OFF);
        ScriptDaemon.npc_set(attachee, 3);
        return RunDefault;
    }
    public static bool bug1_off(GameObject attachee, GameObject triggerer)
    {
        attachee.SetObjectFlag(ObjectFlag.OFF);
        return RunDefault;
    }
    public static bool bug2_off(GameObject attachee, GameObject triggerer)
    {
        attachee.SetObjectFlag(ObjectFlag.OFF);
        return RunDefault;
    }
    public static bool ass1_off(GameObject attachee, GameObject triggerer)
    {
        attachee.SetObjectFlag(ObjectFlag.OFF);
        return RunDefault;
    }
    public static bool tra1_off(GameObject attachee, GameObject triggerer)
    {
        attachee.SetObjectFlag(ObjectFlag.OFF);
        return RunDefault;
    }
    public static bool ogr1_off(GameObject attachee, GameObject triggerer)
    {
        attachee.SetObjectFlag(ObjectFlag.OFF);
        return RunDefault;
    }
    public static bool ett1_off(GameObject attachee, GameObject triggerer)
    {
        attachee.SetObjectFlag(ObjectFlag.OFF);
        return RunDefault;
    }
    public static bool sto1_off(GameObject attachee, GameObject triggerer)
    {
        attachee.SetObjectFlag(ObjectFlag.OFF);
        return RunDefault;
    }
    public static bool hil1_off(GameObject attachee, GameObject triggerer)
    {
        attachee.SetObjectFlag(ObjectFlag.OFF);
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
    public static void spawn_phylactery()
    {
        var loc = RandomRange(1, 12);
        if ((loc == 1))
        {
            var phyl_1 = GameSystems.MapObject.CreateObject(14949, new locXY(435, 368));
        }
        else if ((loc == 2))
        {
            var phyl_2 = GameSystems.MapObject.CreateObject(14949, new locXY(352, 470));
        }
        else if ((loc == 3))
        {
            var phyl_3 = GameSystems.MapObject.CreateObject(14949, new locXY(400, 456));
        }
        else if ((loc == 4))
        {
            var phyl_4 = GameSystems.MapObject.CreateObject(14949, new locXY(543, 377));
        }
        else if ((loc == 5))
        {
            var phyl_5 = GameSystems.MapObject.CreateObject(14949, new locXY(429, 509));
        }
        else if ((loc == 6))
        {
            var phyl_6 = GameSystems.MapObject.CreateObject(14949, new locXY(384, 525));
        }
        else if ((loc == 7))
        {
            var phyl_7 = GameSystems.MapObject.CreateObject(14949, new locXY(383, 553));
        }
        else if ((loc == 8))
        {
            var phyl_8 = GameSystems.MapObject.CreateObject(14949, new locXY(605, 438));
        }
        else if ((loc == 9))
        {
            var phyl_9 = GameSystems.MapObject.CreateObject(14949, new locXY(527, 515));
        }
        else if ((loc == 10))
        {
            var phyl_10 = GameSystems.MapObject.CreateObject(14949, new locXY(478, 579));
        }
        else if ((loc == 11))
        {
            var phyl_11 = GameSystems.MapObject.CreateObject(14949, new locXY(457, 632));
        }
        else if ((loc == 12))
        {
            var phyl_12 = GameSystems.MapObject.CreateObject(14949, new locXY(536, 565));
        }

        return;
    }

}