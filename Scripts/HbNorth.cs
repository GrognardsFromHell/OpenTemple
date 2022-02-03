
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
    [ObjectScript(585)]
    public class HbNorth : BaseObjectScript
    {
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetNameId() == 8999))
            {
                if ((!GetGlobalFlag(556)))
                {
                    SetGlobalFlag(555, true);
                }

            }
            else if ((attachee.GetNameId() == 8994))
            {
                if ((!GetGlobalFlag(555)))
                {
                    SetGlobalFlag(556, true);
                }

            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            var webbed = Livonya.break_free(attachee, 3);
            // MELEE TROOPS  #
            // dumb guys with rage - orc fighters, ogre troops  #
            if ((attachee.GetNameId() == 8994 || attachee.GetNameId() == 8601 || attachee.GetNameId() == 8602 || attachee.GetNameId() == 8603 || attachee.GetNameId() == 8604 || attachee.GetNameId() == 8605 || attachee.GetNameId() == 8606))
            {
                var leader = attachee.GetLeader();
                if ((Utilities.group_percent_hp(leader) >= 51))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 560);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 559);
                        }

                    }

                }
                else if ((Utilities.group_percent_hp(leader) <= 50))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 560);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 561);
                        }

                    }

                }

                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((ScriptDaemon.within_rect_by_corners(obj, 538, 394, 531, 408)))
                    {
                        // obj.condition_add_with_args("Prone",0,0)
                        var coord_x = RandomRange(522, 526);
                        var coord_y = RandomRange(405, 409);
                        obj.Move(new locXY(coord_x, coord_y));
                        AttachParticles("Mon-Phycomid-10", obj);
                        obj.FloatMesFileLine("mes/float.mes", 2);
                        Sound(4177, 1);
                        SetGlobalFlag(557, true);
                    }

                    if ((ScriptDaemon.within_rect_by_corners(obj, 487, 398, 481, 412)))
                    {
                        // obj.condition_add_with_args("Prone",0,0)
                        var coord_x = RandomRange(499, 503);
                        var coord_y = RandomRange(405, 409);
                        obj.Move(new locXY(coord_x, coord_y));
                        AttachParticles("Mon-Phycomid-10", obj);
                        obj.FloatMesFileLine("mes/float.mes", 2);
                        Sound(4177, 1);
                        SetGlobalFlag(557, true);
                    }

                }

                if ((GetGlobalFlag(557) && GetGlobalVar(568) == 0))
                {
                    SetGlobalVar(568, 1);
                }
                else if ((GetGlobalFlag(557) && GetGlobalVar(568) == 1))
                {
                    Sound(4180, 1);
                    SetGlobalVar(568, 2);
                }
                else if ((GetGlobalFlag(557) && GetGlobalVar(568) == 2))
                {
                    spawn_hydra();
                }

            }
            // mage seekers with rage - orc murderer  #
            else if ((attachee.GetNameId() == 8992))
            {
                var leader = attachee.GetLeader();
                if ((Utilities.group_percent_hp(leader) >= 51))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 560);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 562);
                        }

                    }

                }
                else if ((Utilities.group_percent_hp(leader) <= 50))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 560);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 561);
                        }

                    }

                }

                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((ScriptDaemon.within_rect_by_corners(obj, 538, 394, 531, 408)))
                    {
                        // obj.condition_add_with_args("Prone",0,0)
                        var coord_x = RandomRange(522, 526);
                        var coord_y = RandomRange(405, 409);
                        obj.Move(new locXY(coord_x, coord_y));
                        AttachParticles("Mon-Phycomid-10", obj);
                        obj.FloatMesFileLine("mes/float.mes", 2);
                        Sound(4177, 1);
                        SetGlobalFlag(557, true);
                    }

                    if ((ScriptDaemon.within_rect_by_corners(obj, 487, 398, 481, 412)))
                    {
                        // obj.condition_add_with_args("Prone",0,0)
                        var coord_x = RandomRange(499, 503);
                        var coord_y = RandomRange(405, 409);
                        obj.Move(new locXY(coord_x, coord_y));
                        AttachParticles("Mon-Phycomid-10", obj);
                        obj.FloatMesFileLine("mes/float.mes", 2);
                        Sound(4177, 1);
                        SetGlobalFlag(557, true);
                    }

                }

                if ((GetGlobalFlag(557) && GetGlobalVar(568) == 0))
                {
                    SetGlobalVar(568, 1);
                }
                else if ((GetGlobalFlag(557) && GetGlobalVar(568) == 1))
                {
                    Sound(4180, 1);
                    SetGlobalVar(568, 2);
                }
                else if ((GetGlobalFlag(557) && GetGlobalVar(568) == 2))
                {
                    spawn_hydra();
                }

            }
            // archery seekers with rage - orc sergeant  #
            else if ((attachee.GetNameId() == 8993))
            {
                var leader = attachee.GetLeader();
                if ((Utilities.group_percent_hp(leader) >= 51))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 560);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 563);
                        }

                    }

                }
                else if ((Utilities.group_percent_hp(leader) <= 50))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 560);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 561);
                        }

                    }

                }

                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((ScriptDaemon.within_rect_by_corners(obj, 538, 394, 531, 408)))
                    {
                        // obj.condition_add_with_args("Prone",0,0)
                        var coord_x = RandomRange(522, 526);
                        var coord_y = RandomRange(405, 409);
                        obj.Move(new locXY(coord_x, coord_y));
                        AttachParticles("Mon-Phycomid-10", obj);
                        obj.FloatMesFileLine("mes/float.mes", 2);
                        Sound(4177, 1);
                        SetGlobalFlag(557, true);
                    }

                    if ((ScriptDaemon.within_rect_by_corners(obj, 487, 398, 481, 412)))
                    {
                        // obj.condition_add_with_args("Prone",0,0)
                        var coord_x = RandomRange(499, 503);
                        var coord_y = RandomRange(405, 409);
                        obj.Move(new locXY(coord_x, coord_y));
                        AttachParticles("Mon-Phycomid-10", obj);
                        obj.FloatMesFileLine("mes/float.mes", 2);
                        Sound(4177, 1);
                        SetGlobalFlag(557, true);
                    }

                }

                if ((GetGlobalFlag(557) && GetGlobalVar(568) == 0))
                {
                    SetGlobalVar(568, 1);
                }
                else if ((GetGlobalFlag(557) && GetGlobalVar(568) == 1))
                {
                    Sound(4180, 1);
                    SetGlobalVar(568, 2);
                }
                else if ((GetGlobalFlag(557) && GetGlobalVar(568) == 2))
                {
                    spawn_hydra();
                }

            }
            // flankers with rage - orc assassin  #
            else if ((attachee.GetNameId() == 8999))
            {
                var leader = attachee.GetLeader();
                if ((Utilities.group_percent_hp(leader) >= 51))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 560);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 564);
                        }

                    }

                }
                else if ((Utilities.group_percent_hp(leader) <= 50))
                {
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        if (obj.D20Query(D20DispatcherKey.QUE_Prone))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 560);
                        }
                        else
                        {
                            attachee.SetInt(obj_f.critter_strategy, 561);
                        }

                    }

                }

                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((ScriptDaemon.within_rect_by_corners(obj, 538, 394, 531, 408)))
                    {
                        // obj.condition_add_with_args("Prone",0,0)
                        var coord_x = RandomRange(522, 526);
                        var coord_y = RandomRange(405, 409);
                        obj.Move(new locXY(coord_x, coord_y));
                        AttachParticles("Mon-Phycomid-10", obj);
                        obj.FloatMesFileLine("mes/float.mes", 2);
                        Sound(4177, 1);
                        SetGlobalFlag(557, true);
                    }

                    if ((ScriptDaemon.within_rect_by_corners(obj, 487, 398, 481, 412)))
                    {
                        // obj.condition_add_with_args("Prone",0,0)
                        var coord_x = RandomRange(499, 503);
                        var coord_y = RandomRange(405, 409);
                        obj.Move(new locXY(coord_x, coord_y));
                        AttachParticles("Mon-Phycomid-10", obj);
                        obj.FloatMesFileLine("mes/float.mes", 2);
                        Sound(4177, 1);
                        SetGlobalFlag(557, true);
                    }

                }

                if ((GetGlobalFlag(557) && GetGlobalVar(568) == 0))
                {
                    SetGlobalVar(568, 1);
                }
                else if ((GetGlobalFlag(557) && GetGlobalVar(568) == 1))
                {
                    Sound(4180, 1);
                    SetGlobalVar(568, 2);
                }
                else if ((GetGlobalFlag(557) && GetGlobalVar(568) == 2))
                {
                    spawn_hydra();
                }

            }
            // RANGED TROOPS  #
            // mage seekers - orc snipers  #
            else if ((attachee.GetNameId() == 8995 || attachee.GetNameId() == 8996 || attachee.GetNameId() == 8607 || attachee.GetNameId() == 8608))
            {
                var leader = attachee.GetLeader();
                if ((Utilities.group_percent_hp(leader) >= 51))
                {
                    attachee.SetInt(obj_f.critter_strategy, 555);
                }
                else if ((Utilities.group_percent_hp(leader) <= 50))
                {
                    attachee.SetInt(obj_f.critter_strategy, 553);
                }

                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((ScriptDaemon.within_rect_by_corners(obj, 538, 394, 531, 408)))
                    {
                        // obj.condition_add_with_args("Prone",0,0)
                        var coord_x = RandomRange(522, 526);
                        var coord_y = RandomRange(405, 409);
                        obj.Move(new locXY(coord_x, coord_y));
                        AttachParticles("Mon-Phycomid-10", obj);
                        obj.FloatMesFileLine("mes/float.mes", 2);
                        Sound(4177, 1);
                        SetGlobalFlag(557, true);
                    }

                    if ((ScriptDaemon.within_rect_by_corners(obj, 487, 398, 481, 412)))
                    {
                        // obj.condition_add_with_args("Prone",0,0)
                        var coord_x = RandomRange(499, 503);
                        var coord_y = RandomRange(405, 409);
                        obj.Move(new locXY(coord_x, coord_y));
                        AttachParticles("Mon-Phycomid-10", obj);
                        obj.FloatMesFileLine("mes/float.mes", 2);
                        Sound(4177, 1);
                        SetGlobalFlag(557, true);
                    }

                }

                if ((GetGlobalFlag(557) && GetGlobalVar(568) == 0))
                {
                    SetGlobalVar(568, 1);
                }
                else if ((GetGlobalFlag(557) && GetGlobalVar(568) == 1))
                {
                    Sound(4180, 1);
                    SetGlobalVar(568, 2);
                }
                else if ((GetGlobalFlag(557) && GetGlobalVar(568) == 2))
                {
                    spawn_hydra();
                }

            }
            // spell responders - orc marksmen  #
            else if ((attachee.GetNameId() == 8997 || attachee.GetNameId() == 8609))
            {
                foreach (var obj in PartyLeader.GetPartyMembers())
                {
                    if ((obj.GetStat(Stat.level_wizard) >= 1 || obj.GetStat(Stat.level_sorcerer) >= 1 || obj.GetStat(Stat.level_druid) >= 1 || obj.GetStat(Stat.level_bard) >= 1))
                    {
                        var leader = attachee.GetLeader();
                        if ((Utilities.group_percent_hp(leader) >= 34))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 557);
                        }
                        else if ((Utilities.group_percent_hp(leader) <= 33))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 553);
                        }

                    }
                    else
                    {
                        var leader = attachee.GetLeader();
                        if ((Utilities.group_percent_hp(leader) >= 34))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 554);
                        }
                        else if ((Utilities.group_percent_hp(leader) <= 33))
                        {
                            attachee.SetInt(obj_f.critter_strategy, 553);
                        }

                    }

                }

                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((ScriptDaemon.within_rect_by_corners(obj, 538, 394, 531, 408)))
                    {
                        // obj.condition_add_with_args("Prone",0,0)
                        var coord_x = RandomRange(522, 526);
                        var coord_y = RandomRange(405, 409);
                        obj.Move(new locXY(coord_x, coord_y));
                        AttachParticles("Mon-Phycomid-10", obj);
                        obj.FloatMesFileLine("mes/float.mes", 2);
                        Sound(4177, 1);
                        SetGlobalFlag(557, true);
                    }

                    if ((ScriptDaemon.within_rect_by_corners(obj, 487, 398, 481, 412)))
                    {
                        // obj.condition_add_with_args("Prone",0,0)
                        var coord_x = RandomRange(499, 503);
                        var coord_y = RandomRange(405, 409);
                        obj.Move(new locXY(coord_x, coord_y));
                        AttachParticles("Mon-Phycomid-10", obj);
                        obj.FloatMesFileLine("mes/float.mes", 2);
                        Sound(4177, 1);
                        SetGlobalFlag(557, true);
                    }

                }

                if ((GetGlobalFlag(557) && GetGlobalVar(568) == 0))
                {
                    SetGlobalVar(568, 1);
                }
                else if ((GetGlobalFlag(557) && GetGlobalVar(568) == 1))
                {
                    Sound(4180, 1);
                    SetGlobalVar(568, 2);
                }
                else if ((GetGlobalFlag(557) && GetGlobalVar(568) == 2))
                {
                    spawn_hydra();
                }

            }
            // animals - hydra  #
            else if ((attachee.GetNameId() == 14982))
            {
                SetGlobalVar(568, GetGlobalVar(568) + 1);
                if ((GetGlobalVar(568) == 4))
                {
                    var picker = RandomRange(14978, 14981);
                    var animal = GameSystems.MapObject.CreateObject(picker, new locXY(511, 423));
                    animal.Move(new locXY(515, 424));
                    animal.Rotation = 5.49778714378f;
                    animal.SetConcealed(true);
                    animal.Unconceal();
                    AttachParticles("Mon-YellowMold-30", animal);
                    Sound(4181, 1);
                }
                else if ((GetGlobalVar(568) == 5))
                {
                    var picker = RandomRange(14978, 14981);
                    var animal = GameSystems.MapObject.CreateObject(picker, new locXY(511, 423));
                    animal.Move(new locXY(507, 424));
                    animal.Rotation = 5.49778714378f;
                    animal.SetConcealed(true);
                    animal.Unconceal();
                    AttachParticles("Mon-YellowMold-30", animal);
                    Sound(4181, 1);
                }
                else if ((GetGlobalVar(568) == 6))
                {
                    var picker = RandomRange(14978, 14981);
                    var animal = GameSystems.MapObject.CreateObject(picker, new locXY(511, 423));
                    animal.Move(new locXY(519, 424));
                    animal.Rotation = 5.49778714378f;
                    animal.SetConcealed(true);
                    animal.Unconceal();
                    AttachParticles("Mon-YellowMold-30", animal);
                    Sound(4181, 1);
                }
                else if ((GetGlobalVar(568) == 7))
                {
                    var picker = RandomRange(14978, 14981);
                    var animal = GameSystems.MapObject.CreateObject(picker, new locXY(511, 423));
                    animal.Move(new locXY(503, 424));
                    animal.Rotation = 5.49778714378f;
                    animal.SetConcealed(true);
                    animal.Unconceal();
                    AttachParticles("Mon-YellowMold-30", animal);
                    Sound(4181, 1);
                }
                else if ((GetGlobalVar(568) == 8))
                {
                    var picker = RandomRange(14978, 14981);
                    var animal = GameSystems.MapObject.CreateObject(picker, new locXY(511, 423));
                    animal.Move(new locXY(523, 424));
                    animal.Rotation = 5.49778714378f;
                    animal.SetConcealed(true);
                    animal.Unconceal();
                    AttachParticles("Mon-YellowMold-30", animal);
                    Sound(4181, 1);
                }
                else if ((GetGlobalVar(568) == 9))
                {
                    var picker = RandomRange(14978, 14981);
                    var animal = GameSystems.MapObject.CreateObject(picker, new locXY(511, 423));
                    animal.Move(new locXY(499, 424));
                    animal.Rotation = 5.49778714378f;
                    animal.SetConcealed(true);
                    animal.Unconceal();
                    AttachParticles("Mon-YellowMold-30", animal);
                    Sound(4181, 1);
                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetNameId() == 8999))
            {
                if ((GetGlobalFlag(555)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 406);
                    attachee.SetStandpoint(StandPointType.Day, 406);
                    attachee.ClearNpcFlag(NpcFlag.KOS);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 9000))
            {
                if ((GetGlobalFlag(555)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 405);
                    attachee.SetStandpoint(StandPointType.Day, 405);
                    attachee.ClearNpcFlag(NpcFlag.KOS);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8601))
            {
                if ((GetGlobalFlag(555)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 408);
                    attachee.SetStandpoint(StandPointType.Day, 408);
                    attachee.ClearNpcFlag(NpcFlag.KOS);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8602))
            {
                if ((GetGlobalFlag(555)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 409);
                    attachee.SetStandpoint(StandPointType.Day, 409);
                    attachee.ClearNpcFlag(NpcFlag.KOS);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8603))
            {
                if ((GetGlobalFlag(555)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 410);
                    attachee.SetStandpoint(StandPointType.Day, 410);
                    attachee.ClearNpcFlag(NpcFlag.KOS);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8604))
            {
                if ((GetGlobalFlag(555)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 411);
                    attachee.SetStandpoint(StandPointType.Day, 411);
                    attachee.ClearNpcFlag(NpcFlag.KOS);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8605))
            {
                if ((GetGlobalFlag(555)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 413);
                    attachee.SetStandpoint(StandPointType.Day, 413);
                    attachee.ClearNpcFlag(NpcFlag.KOS);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8606))
            {
                if ((GetGlobalFlag(555)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 417);
                    attachee.SetStandpoint(StandPointType.Day, 417);
                    attachee.ClearNpcFlag(NpcFlag.KOS);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8607))
            {
                if ((GetGlobalFlag(555)))
                {
                    attachee.Unconceal();
                    // attachee.move(location_from_axis (511L, 364L))
                    attachee.SetStandpoint(StandPointType.Night, 419);
                    attachee.SetStandpoint(StandPointType.Day, 419);
                    attachee.ClearNpcFlag(NpcFlag.KOS);
                    // attachee.rotation = 2.35619449019
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8608))
            {
                if ((GetGlobalFlag(555)))
                {
                    attachee.Unconceal();
                    // attachee.move(location_from_axis (506L, 364L))
                    attachee.SetStandpoint(StandPointType.Night, 420);
                    attachee.SetStandpoint(StandPointType.Day, 420);
                    attachee.ClearNpcFlag(NpcFlag.KOS);
                    // attachee.rotation = 2.35619449019
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8609))
            {
                if ((GetGlobalFlag(555)))
                {
                    attachee.Unconceal();
                    // attachee.move(location_from_axis (508L, 361L))
                    attachee.SetStandpoint(StandPointType.Night, 421);
                    attachee.SetStandpoint(StandPointType.Day, 421);
                    attachee.ClearNpcFlag(NpcFlag.KOS);
                    // attachee.rotation = 2.35619449019
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8992))
            {
                if ((GetGlobalFlag(556)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 297);
                    attachee.SetStandpoint(StandPointType.Day, 297);
                    attachee.ClearNpcFlag(NpcFlag.KOS);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8993))
            {
                if ((GetGlobalFlag(556)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 298);
                    attachee.SetStandpoint(StandPointType.Day, 298);
                    attachee.ClearNpcFlag(NpcFlag.KOS);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8994))
            {
                if ((GetGlobalFlag(556)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 299);
                    attachee.SetStandpoint(StandPointType.Day, 299);
                    attachee.ClearNpcFlag(NpcFlag.KOS);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8995))
            {
                if ((GetGlobalFlag(556)))
                {
                    attachee.Unconceal();
                    attachee.SetStandpoint(StandPointType.Night, 336);
                    attachee.SetStandpoint(StandPointType.Day, 336);
                    attachee.ClearNpcFlag(NpcFlag.KOS);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8996))
            {
                if ((GetGlobalFlag(556)))
                {
                    attachee.Unconceal();
                    attachee.SetStandpoint(StandPointType.Night, 404);
                    attachee.SetStandpoint(StandPointType.Day, 404);
                    attachee.ClearNpcFlag(NpcFlag.KOS);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8997))
            {
                if ((GetGlobalFlag(556)))
                {
                    attachee.Unconceal();
                    attachee.SetStandpoint(StandPointType.Night, 333);
                    attachee.SetStandpoint(StandPointType.Day, 333);
                    attachee.ClearNpcFlag(NpcFlag.KOS);
                    DetachScript();
                }

            }
            else if ((attachee.GetNameId() == 8998))
            {
                if ((GetGlobalFlag(556)))
                {
                    attachee.SetStandpoint(StandPointType.Night, 300);
                    attachee.SetStandpoint(StandPointType.Day, 300);
                    attachee.ClearNpcFlag(NpcFlag.KOS);
                    DetachScript();
                }

            }

            return RunDefault;
        }
        public static void spawn_hydra()
        {
            var hydra = GameSystems.MapObject.CreateObject(14982, new locXY(511, 423));
            hydra.Move(new locXY(511, 424));
            hydra.Rotation = 5.49778714378f;
            hydra.SetConcealed(true);
            hydra.Unconceal();
            AttachParticles("Mon-YellowMold-30", hydra);
            Sound(4179, 1);
            SetGlobalVar(568, 3);
            return;
        }

    }
}
