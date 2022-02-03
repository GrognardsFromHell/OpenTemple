
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
using System.Reflection.Metadata.Ecma335;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts;

[ObjectScript(446)]
public class Earthcombat : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if (attachee.GetNameId() == 14913) // grate
        {
            triggerer.BeginDialog(attachee, 1000);
        }
        else if (attachee.GetNameId() == 14914) // barrier
        {
            if (!GetGlobalFlag(104) && !triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8045)))
            {
                triggerer.BeginDialog(attachee, 1500);
            }
            else
            {
                triggerer.BeginDialog(attachee, 2100);
            }

        }

        return SkipDefault;
    }

    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        if (attachee.GetNameId() == 14913) // grate
        {
            attachee.Move(new locXY(415, 556), 0, 11f);
            attachee.Rotation = 1.57f * 3 / 2;
        }

        Livonya.tag_strategy(attachee);
        Livonya.get_melee_strategy(attachee);
        return RunDefault;
    }

    public override bool OnExitCombat(GameObject attachee, GameObject triggerer)
    {
        // attahcee.standpoint_set(STANDPOINT_DAY, attachee.obj_get_int(obj_f_npc_pad_i_3))
        // attahcee.standpoint_set(STANDPOINT_NIGHT, attachee.obj_get_int(obj_f_npc_pad_i_3))
        // set_v('Test666', get_v('Test666') | 2 )
        var dummy = 1;
        if (GetGlobalFlag(403))
        {
            attachee.FloatMesFileLine("mes/script_activated.mes", 14, TextFloaterColor.Red);
        }

        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        if (attachee.GetNameId() == 14913) // harpy grate
        {
            attachee.Move(new locXY(415, 556), 0, 11f);
            attachee.Rotation = 1.57f * 3 / 2;
            return SkipDefault;
        }
        else if (attachee.GetNameId() == 14914)
        {
            return SkipDefault;
        }
        else if (attachee.GetNameId() == 14811) // The Beacon
        {
            attachee.SetInt(obj_f.critter_strategy, 104);
            attachee.FloatMesFileLine("mes/script_activated.mes", 15, TextFloaterColor.Red);
            return RunDefault;
        }

        // Copied from script 310 :	#
        // THIS IS USED FOR BREAK FREE
        while ((attachee.FindItemByName(8903) != null))
        {
            attachee.FindItemByName(8903).Destroy();
        }

        // if (attachee.d20_query(Q_Is_BreakFree_Possible)): # workaround no longer necessary!
        // create_item_in_inventory( 8903, attachee )
        // attachee.d20_send_signal(S_BreakFree)
        // if game.global_flags[403] == 1:
        // attachee.float_mesfile_line( 'mes\\script_activated.mes', 15, 1 )
        if (attachee.GetNameId() == 14249 || attachee.GetNameId() == 14381 || attachee.GetNameId() == 14296) // Ogre, Medium Earth Elem, Large Earth Elem
        {
        }
        else if (attachee.GetNameId() == 14243 && attachee.GetMap() == 5066) // harpies	at Temple Level 1
        {
            ghouls_harpies_join_in(attachee);
            spawn_grate_object();
        }
        else
        {
            Livonya.get_melee_strategy(attachee);
        }

        return RunDefault;
    }

    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (attachee.GetNameId() == 14913) // grating
        {
            GameObject grate_obj = null;
            foreach (var door_candidate in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PORTAL))
            {
                if ((door_candidate.GetNameId() == 120))
                {
                    grate_obj = door_candidate;
                }

            }

            if (grate_obj != null)
            {
                grate_obj.SetObjectFlag(ObjectFlag.OFF);
                grate_obj.SetPortalFlag(PortalFlag.OPEN);
            }

            AttachParticles("ef-MinoCloud", attachee);
            AttachParticles("Orb-Summon-Earth Elemental", attachee);
            AttachParticles("Mon-EarthElem-Unconceal", attachee);
            Sound(4042, 1);
            attachee.SetObjectFlag(ObjectFlag.OFF);
        }
        else if (attachee.GetNameId() == 14914) // Earth Temple Barrier
        {
            GameObject barrier_obj = null;
            foreach (var door_candidate in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PORTAL))
            {
                if ((door_candidate.GetNameId() == 121))
                {
                    door_candidate.SetPortalFlag(PortalFlag.OPEN);
                    door_candidate.Destroy();
                }

            }

            // game.particles( 'ef-MinoCloud', attachee )
            AttachParticles("Orb-Summon-Earth Elemental", attachee);
            AttachParticles("Mon-EarthElem-Unconceal", attachee);
            Sound(4042, 1);
            attachee.SetObjectFlag(ObjectFlag.CLICK_THROUGH);
            StartTimer(100, () => barrier_away(attachee, 2));
            StartTimer(200, () => barrier_away(attachee, 3));
        }

        return RunDefault;
    }
    public override bool OnEndCombat(GameObject attachee, GameObject triggerer)
    {
        if (attachee.GetNameId() == 14811) // The Beacon
        {
            attachee.FloatMesFileLine("mes/script_activated.mes", 16, TextFloaterColor.Red);
            var countt_encroachers = 1;
            var countt_all = 1;
            foreach (var npc in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                attachee.FloatMesFileLine("mes/test.mes", countt_all, TextFloaterColor.Red);
                countt_all += 1;
                if (ScriptDaemon.is_far_from_party(npc, 48) && !npc.IsUnconscious())
                {
                    attachee.FloatMesFileLine("mes/test.mes", countt_encroachers, TextFloaterColor.Green);
                    countt_encroachers += 1;
                    var joe = Utilities.party_closest(npc);
                }

            }

            // encroach(npc, joe)
            attachee.FloatMesFileLine("mes/script_activated.mes", 16, TextFloaterColor.Red);
            if (ScriptDaemon.get_v("Beacon_Active") > 0)
            {
                ScriptDaemon.set_v("Beacon_Active", ScriptDaemon.get_v("Beacon_Active") - 1);
            }

            attachee.Destroy(); // so combat won't get stuck
            attachee.FloatMesFileLine("mes/script_activated.mes", 16, TextFloaterColor.Red);
            return RunDefault;
        }

        return RunDefault;
    }
    public static bool san_enter_combat_backup_with_beacon_shit(GameObject attachee, GameObject triggerer)
    {
        Livonya.tag_strategy(attachee);
        if (attachee.GetNameId() == 14811) // The Beacon
        {
            attachee.SetScriptId(ObjScriptEvent.EndCombat, 446); // end combat round script
            attachee.FloatMesFileLine("mes/script_activated.mes", 13, TextFloaterColor.Red);
            return RunDefault;
        }

        if ((!ScriptDaemon.can_see_party(attachee) && ScriptDaemon.is_far_from_party(attachee, 10)) || ScriptDaemon.is_far_from_party(attachee, 40))
        {
            if (ScriptDaemon.is_far_from_party(attachee, 70))
            {
                var joe = Utilities.party_closest(attachee);
                ScriptDaemon.encroach(attachee, joe);
            }

            attachee.SetInt(obj_f.critter_strategy, 119); // Seek out low ac beacon
            AttachParticles("sp-Hold Person", attachee);
            if (ScriptDaemon.get_v("Beacon_Active") == 0)
            {
                var top_path = 0;
                var bottom_path = 0;
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    if (ScriptDaemon.within_rect_by_corners(pc, 467, 360, 467, 388) && !pc.IsUnconscious())
                    {
                        top_path = top_path + 1;
                    }

                    if (ScriptDaemon.within_rect_by_corners(pc, 504, 355, 504, 385) && !pc.IsUnconscious())
                    {
                        bottom_path = bottom_path + 1;
                    }

                }

                int primary_beacon_x;
                int primary_beacon_y;
                int tertiary_beacon_x;
                int tertiary_beacon_y;
                if (top_path > bottom_path)
                {
                    primary_beacon_x = 470;
                    primary_beacon_y = 388;
                    tertiary_beacon_x = 492;
                    tertiary_beacon_y = 387;
                }
                else
                {
                    primary_beacon_x = 492;
                    primary_beacon_y = 387;
                    tertiary_beacon_x = 470;
                    tertiary_beacon_y = 388;
                }

                var beacon_loc = new locXY(primary_beacon_x, primary_beacon_y);
                var beacon3_loc = new locXY(tertiary_beacon_x, tertiary_beacon_y);
                var beacon = GameSystems.MapObject.CreateObject(14811, beacon_loc);
                beacon.Move(new locXY(470, 388), 0, 0);
                beacon.SetInt(obj_f.npc_ac_bonus, -50);
                beacon.SetBaseStat(Stat.dexterity, -70); // causes problems at end of round, or does it?
                // beacon.object_flag_set(OF_DONTDRAW) # this causes combat to lag at the beacon's turn
                beacon.SetObjectFlag(ObjectFlag.CLICK_THROUGH);
                beacon.AddToInitiative();
                beacon.SetInitiative(-20);
                UiSystems.Combat.Initiative.UpdateIfNeeded();
                beacon.SetScriptId(ObjScriptEvent.EndCombat, 446); // end combat round
                // beacon.scripts[14] = 446 # exit combat
                AttachParticles("sp-hold person", beacon);
                var beacon2 = GameSystems.MapObject.CreateObject(14811, new locXY(483, 395));
                beacon2.Move(new locXY(483, 395), 0, 0);
                beacon2.SetInt(obj_f.npc_ac_bonus, -40);
                // beacon2.object_flag_set(OF_DONTDRAW)
                beacon2.SetObjectFlag(ObjectFlag.CLICK_THROUGH);
                beacon2.AddToInitiative();
                beacon2.SetInitiative(-21);
                UiSystems.Combat.Initiative.UpdateIfNeeded();
                AttachParticles("sp-hold person", beacon2);
                var beacon3 = GameSystems.MapObject.CreateObject(14811, beacon3_loc);
                beacon3.Move(beacon3_loc, 0, 0);
                beacon3.SetInt(obj_f.npc_ac_bonus, -30);
                // beacon3.object_flag_set(OF_DONTDRAW)
                beacon3.SetObjectFlag(ObjectFlag.CLICK_THROUGH);
                beacon3.AddToInitiative();
                beacon3.SetInitiative(-23);
                UiSystems.Combat.Initiative.UpdateIfNeeded();
                AttachParticles("sp-hold person", beacon3);
                ScriptDaemon.set_v("Beacon_Active", 3);
            }

        }
        else if (ScriptDaemon.is_far_from_party(attachee, 75))
        {
            var joe = Utilities.party_closest(attachee);
            ScriptDaemon.encroach(attachee, joe);
        }
        else
        {
            Livonya.get_melee_strategy(attachee);
        }

        // Tried changing their standpoint midfight, didn't work.
        // attachee.standpoint_set(STANDPOINT_DAY, attachee.obj_get_int(obj_f_npc_pad_i_3) - 342 + 500)
        // attachee.standpoint_set(STANDPOINT_NIGHT, attachee.obj_get_int(obj_f_npc_pad_i_3) - 342 + 500)
        // if attachee.obj_get_int(obj_f_npc_pad_i_3) == 361 or attachee.obj_get_int(obj_f_npc_pad_i_3) == 362: #sentry standpoints
        // hl(attachee)
        // xx = 482
        // yy = 417
        // for npc in game.obj_list_vicinity(location_from_axis(xx,yy), OLC_NPC ):
        // if npc.leader_get() == OBJ_HANDLE_NULL:
        // npc.standpoint_set(STANDPOINT_DAY, npc.obj_get_int(obj_f_npc_pad_i_3) - 342 + 500)
        // npc.standpoint_set(STANDPOINT_NIGHT, npc.obj_get_int(obj_f_npc_pad_i_3) - 342 + 500)
        // npc.npc_flag_set(ONF_KOS)
        // npc.npc_flag_unset(ONF_KOS_OVERRIDE)
        return RunDefault;
    }
    public static bool san_start_combat_with_beacon_shit(GameObject attachee, GameObject triggerer)
    {
        if (attachee.GetNameId() == 14811) // The Beacon
        {
            attachee.SetInt(obj_f.critter_strategy, 104);
            attachee.FloatMesFileLine("mes/script_activated.mes", 15, TextFloaterColor.Red);
            return RunDefault;
        }

        // Copied from script 310 :	#
        // THIS IS USED FOR BREAK FREE
        while ((attachee.FindItemByName(8903) != null))
        {
            attachee.FindItemByName(8903).Destroy();
        }

        // if (attachee.d20_query(Q_Is_BreakFree_Possible)): # workaround no longer necessary!
        // create_item_in_inventory( 8903, attachee )
        // attachee.d20_send_signal(S_BreakFree)
        if (GetGlobalFlag(403))
        {
            attachee.FloatMesFileLine("mes/script_activated.mes", 15, TextFloaterColor.Red);
        }

        return RunDefault;
    }
    public static void ghouls_harpies_join_in(GameObject attachee)
    {
        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
        {
            if ((new[] { 14095, 14128, 14129 }).Contains(obj.GetNameId()) && ScriptDaemon.within_rect_by_corners(obj, 415, 556, 419, 590) && obj.GetLeader() == null)
            {
                // ghouls in the eastern room - should join the fray, per the module
                obj.Attack(SelectedPartyLeader);
            }

            if (obj.GetNameId() == 14243 && obj.GetLeader() == null)
            {
                // other harpies
                obj.Attack(SelectedPartyLeader);
            }

        }

        if (attachee.GetMap() == 5066 && (!ScriptDaemon.get_f("j_ghouls_corridor_temple_1"))) // temple level 1
        {
            ScriptDaemon.set_f("j_ghouls_corridor_temple_1");
            var yyp_max = 556;
            var yyp_o = 561;
            foreach (var pc in PartyLeader.GetPartyMembers())
            {
                var (xxp, yyp) = pc.GetLocation();
                if (xxp >= 433 && yyp >= 561)
                {
                    if (yyp > yyp_max)
                    {
                        yyp_max = yyp;
                    }

                }

            }

            var y_ghoul = yyp_max + 18;
            y_ghoul = Math.Min(603, y_ghoul);
            var y_ghoul_add = new[] { 0, 0, 2, 2, -2 };
            var x_ghoul_add = new[] { 0, 2, 0, 2, 0 };
            var x_ghoul = 433;
            var ghoul_counter = 0;
            foreach (var npc in ObjList.ListVicinity(new locXY(463, 603), ObjectListFilter.OLC_NPC))
            {
                if (npc.GetNameId() == 14129 && npc.GetLeader() == null && !npc.IsUnconscious())
                {
                    npc.Move(new locXY(x_ghoul + x_ghoul_add[ghoul_counter], y_ghoul + y_ghoul_add[ghoul_counter]), 0, 0);
                    ghoul_counter += 1;
                    npc.Attack(SelectedPartyLeader);
                }

            }

        }

    }
    public static void spawn_grate_object()
    {
        if (!ScriptDaemon.get_f("harpy_grate"))
        {
            var should_drop_grate = false;
            foreach (var pc in PartyLeader.GetPartyMembers())
            {
                var (xx, yy) = pc.GetLocation();
                if (xx <= 415 && yy < 580 && yy > 500)
                {
                    should_drop_grate = true;
                }

                if (should_drop_grate)
                {
                    ScriptDaemon.set_f("harpy_grate");
                    GameObject grate_obj = null;
                    foreach (var obj in ObjList.ListVicinity(new locXY(415, 556), ObjectListFilter.OLC_PORTAL))
                    {
                        if (obj.GetNameId() == 120)
                        {
                            grate_obj = obj;
                        }

                    }

                    if (grate_obj == null)
                    {
                        grate_obj = GameSystems.MapObject.CreateObject(120, new locXY(415, 556));
                        grate_obj.Move(new locXY(415, 556), 0, 11f);
                        grate_obj.Rotation = 1.57f * 3 / 2;
                        grate_obj.SetPortalFlag(PortalFlag.JAMMED);
                        grate_obj.SetObjectFlag(ObjectFlag.SHOOT_THROUGH);
                        grate_obj.SetObjectFlag(ObjectFlag.SEE_THROUGH);
                        grate_obj.SetObjectFlag(ObjectFlag.CLICK_THROUGH);
                        grate_obj.SetObjectFlag(ObjectFlag.TRANSLUCENT);
                        grate_obj.SetObjectFlag(ObjectFlag.NO_BLOCK);
                        grate_obj.SetObjectFlag(ObjectFlag.NOHEIGHT);
                    }

                    // game.timevent_add( grate_npc_timed_event, (  ), 250, 1 )
                    AttachParticles("Orb-Summon-Earth Elemental", grate_obj);
                    GameSystems.Scroll.ShakeScreen(50, 500);
                    Sound(4180, 1);
                }

            }

        }

    }
    public static int grate_strength()
    {
        var strength_array = new List<int>();
        foreach (var obj in PartyLeader.GetPartyMembers())
        {
            strength_array.Add(obj.GetStat(Stat.strength));
        }

        strength_array.Sort();
        strength_array.Reverse();
        if (strength_array[0] >= 24)
        {
            return 2;
        }

        if (strength_array.Count >= 3)
        {
            if (strength_array[0] + strength_array[1] + strength_array[2] >= 60)
            {
                return 1;
            }

        }

        return 0;
    }
    public static void grate_away(GameObject npc)
    {
        npc.ExecuteObjectScript(npc, ObjScriptEvent.Dying);
    }
    public static void grate_npc_timed_event()
    {
        var grate_npc = GameSystems.MapObject.CreateObject(14913, new locXY(415, 556));
        grate_npc.Move(new locXY(415, 556), 0, 11f);
        grate_npc.Rotation = 1.57f * 3 / 2;
        grate_npc.SetObjectFlag(ObjectFlag.SHOOT_THROUGH);
        grate_npc.SetObjectFlag(ObjectFlag.SEE_THROUGH);
    }
    public static void earth_temple_listen_check()
    {
        var highest_listen_mod = -10;
        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            if (pc.type == ObjectType.pc)
            {
                if (pc.GetSkillLevel(SkillId.listen) < highest_listen_mod)
                {
                    highest_listen_mod = pc.GetSkillLevel(SkillId.listen);
                }

            }

        }

        SetGlobalVar(35, RandomRange(1, 20) + highest_listen_mod);
    }
    public static void earth_temple_haul_inside(GameObject pc, GameObject npc, int line)
    {
        StartTimer(181150, () => move_party_inside(pc, npc, 1555, 480, 393));
        Fade(180, 0, 0, 1);
    }
    public static void move_party_inside(GameObject pc, GameObject npc, int line, int x, int y)
    {
        FadeAndTeleport(30, 0, 0, 5066, x, y);
        // game.global_vars[1] = 400
        StartTimer(100, () => talk_to_gatekeeper(pc, npc, line, x, y));
        return;
    }
    public static void talk_to_gatekeeper(GameObject pc, GameObject npc, int line, int x, int y)
    {
        var op = GameSystems.MapObject.CreateObject(14915, new locXY(x + 1, y + 1));
        op.SetScriptId(ObjScriptEvent.Dialog, 446);
        op.SetScriptId(ObjScriptEvent.FirstHeartbeat, 446);
        op.RemoveScript(ObjScriptEvent.Heartbeat);
        op.SetObjectFlag(ObjectFlag.DONTDRAW);
        // game.global_vars[1] = 401
        if (npc.GetNameId() == 14915)
        {
            // game.global_vars[1] = 402
            npc.Destroy();
        }

        // game.global_vars[1] = 403
        pc.BeginDialog(op, line);
        // game.global_vars[1] += 1000
        // game.particles( "sp-summon monster I", operator )
        return;
    }

    public static void switch_to_npc(GameObject pc, GameObject originator_npc, string npc_name = null,
        int dest_line = 0, int failsafe_line = 0)
    {
        int npc_name_num;
        if (npc_name == "romag")
        {
            npc_name_num = 8045;
        }
        else if (npc_name == "hartsch")
        {
            npc_name_num = 14154;
        }
        else if (npc_name == "earth troop commander")
        {
            npc_name_num = 14156;
        }
        else
        {
            npc_name_num = 0; // failure
        }

        switch_to_npc(pc, originator_npc, npc_name_num, dest_line, failsafe_line);
    }


    public static void switch_to_npc(GameObject pc, GameObject originator_npc, int npc_name = 0, int dest_line = 0, int failsafe_line = 0)
    {
        foreach (var obj in ObjList.ListVicinity(pc.GetLocation(), ObjectListFilter.OLC_NPC))
        {
            if (obj.GetNameId() == npc_name)
            {
                if (dest_line == 0)
                {
                    if (originator_npc.GetNameId() == 14915) // sentinel
                    {
                        originator_npc.Destroy();
                    }

                    obj.ExecuteObjectScript(pc, ObjScriptEvent.Dialog);
                    return;
                }
                else
                {
                    if (originator_npc.GetNameId() == 14915) // sentinel
                    {
                        originator_npc.Destroy();
                    }

                    pc.BeginDialog(obj, dest_line);
                    return;
                }

            }

        }

        return;
    }
    public static void earth_attack_party(GameObject pc, GameObject npc)
    {
        if (npc.GetNameId() == 14915)
        {
            StartTimer(100, () => destroy_npc(npc), true);
        }

        foreach (var obj in ObjList.ListVicinity(pc.GetLocation(), ObjectListFilter.OLC_NPC))
        {
            if (!((SelectedPartyLeader.GetPartyMembers()).Contains(obj)) && obj.GetNameId() != 14914 && obj.GetNameId() != 14915)
            {
                obj.Attack(pc);
            }

        }

        return;
    }
    public static void destroy_npc(GameObject npc)
    {
        npc.Destroy();
    }
    public static void call_dying_script(GameObject obj)
    {
        obj.ExecuteObjectScript(obj, ObjScriptEvent.Dying);
    }
    public static void set_barrier_off(GameObject obj)
    {
        obj.SetObjectFlag(ObjectFlag.OFF);
    }
    public static void barrier_away(GameObject npc, int barrier_smash_stage = 1)
    {
        if (barrier_smash_stage == 1)
        {
            var (xx, yy) = npc.GetLocation();
            int xxo;
            if (xx <= 480)
            {
                xxo = 500;
            }
            else
            {
                xxo = 460;
            }

            SetGlobalVar(2, GetGlobalVar(2) + 100);
            foreach (var other_barrier in ObjList.ListVicinity(new locXY(xxo, 378), ObjectListFilter.OLC_NPC))
            {
                if (other_barrier.GetNameId() == 14914)
                {
                    StartTimer(RandomRange(100, 200), () => call_dying_script(other_barrier), true);
                    StartTimer(RandomRange(200, 300), () => set_barrier_off(other_barrier), true);
                }

            }

            npc.ExecuteObjectScript(npc, ObjScriptEvent.Dying);
        }
        else if (barrier_smash_stage == 2)
        {
            // game.timevent_add( barrier_away, ( OBJ_HANDLE_NULL, 3), 70, 1 )
            var pcs_in_northern_access = 0;
            var pcs_in_southern_access = 0;
            var pcs_in_northern_hallway = 0;
            var pcs_in_southern_hallway = 0;
            var yy_south_max = 300;
            var yy_north_max = 300;
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                var (xx, yy) = pc.GetLocation();
                if ((xx >= 461 && xx <= 470 && yy <= 379 && yy >= 370) && Utilities.willing_and_capable(pc))
                {
                    pcs_in_northern_access += 1;
                }

                if ((xx >= 494 && xx <= 505 && yy >= 370 && yy <= 379))
                {
                    pcs_in_southern_access += 1;
                }

                if ((xx >= 451 && xx < 461 && yy >= 370 && yy <= 440))
                {
                    pcs_in_northern_hallway += 1;
                    if (yy > yy_north_max - 18)
                    {
                        yy_north_max = Math.Min(yy + 18, 442);
                    }

                }

                if ((xx > 505 && xx <= 515 && yy >= 370 && yy <= 442))
                {
                    pcs_in_southern_hallway += 1;
                    if (yy > yy_south_max - 18)
                    {
                        yy_south_max = Math.Min(yy + 18, 444);
                    }

                }

            }

            int y_troop;
            int x_troop;
            int[] x_troop_add;
            int[] y_troop_add;
            if (pcs_in_northern_access > 0 && pcs_in_southern_access == 0 && pcs_in_southern_hallway == 0 && pcs_in_northern_hallway == 0)
            {
                // PCs in northern access
                y_troop = 378 + 17;
                x_troop = 459;
                x_troop_add = new[] { 0, -2, 0, -2, -1, -3, -4, -5, -5, -1 };
                y_troop_add = new[] { 0, 0, -2, -2, -1, -1, -2, -3, -1, 1 };
            }
            else if (pcs_in_northern_access == 0 && pcs_in_southern_access > 0 && pcs_in_southern_hallway == 0 && pcs_in_northern_hallway == 0)
            {
                // PCs in southern access
                y_troop = 378 + 17;
                x_troop = 506;
                x_troop_add = new[] { 0, 2, 0, 2, 1, 3, 4, 5, 5, 1 };
                y_troop_add = new[] { 0, 0, -2, -2, -1, -1, -2, -3, -1, 1 };
            }
            else if (pcs_in_northern_hallway > 0 && pcs_in_northern_hallway >= pcs_in_southern_hallway)
            {
                // PCs in Northern hallway
                y_troop = yy_north_max;
                x_troop = 459;
                x_troop_add = new[] { 0, -2, 0, -2, -1, -3, -4, -5, -5, -1 };
                y_troop_add = new[] { 0, 0, -2, -2, -1, -1, -2, -3, -1, 1 };
            }
            else if (pcs_in_southern_hallway > 0)
            {
                // PCs in Southern hallway
                y_troop = yy_south_max;
                x_troop = 506;
                x_troop_add = new[] { 0, 2, 0, 2, 1, 3, 4, 5, 5, 1 };
                y_troop_add = new[] { 0, 0, -2, -2, -1, -1, -2, -3, -1, 1 };
            }
            else
            {
                return; // TODO Added this because of uninitialized vars
            }

            var N_max = y_troop_add.Length;
            var troop_counter = 0;
            if (x_troop < 470)
            {
                // use the ones near the south, or at the back
                foreach (var obj in ObjList.ListVicinity(new locXY(495, 389), ObjectListFilter.OLC_NPC))
                {
                    var (xx, yy) = obj.GetLocation();
                    if (obj.GetLeader() == null && (new[] { 8045, 14066, 14078, 14154, 14156, 14162, 14163, 14164, 14165, 14248, 14249, 14296, 14337, 14338, 14339, 14381 }).Contains(obj.GetNameId()))
                    {
                        if ((new[] { 14296, 14381 }).Contains(obj.GetNameId()))
                        {
                            obj.SetInt(obj_f.speed_run, 1075644444);
                            obj.SetInt(obj_f.speed_walk, 1075064444);
                            StartTimer(450, () => timed_unconceal(obj), true);
                        }

                        if (xx >= 483 && xx <= 506 && troop_counter < N_max)
                        {
                            obj.Move(new locXY(x_troop + x_troop_add[troop_counter], y_troop + y_troop_add[troop_counter]), 0, 0);
                            troop_counter += 1;
                        }
                        else if (xx >= 483 && xx <= 506)
                        {
                            obj.Move(new locXY(xx - 15, yy), 0, 0);
                        }
                        else if (xx >= 475 && xx < 483 && yy <= 392)
                        {
                            obj.Move(new locXY(xx - 4, yy), 0, 0);
                        }
                        else if (xx >= 475 && xx < 483 && yy > 392)
                        {
                            obj.Move(new locXY(xx - 4, yy - 10), 0, 0);
                        }
                        else if (xx < 475 && yy > 392)
                        {
                            obj.Move(new locXY(xx, yy - 10), 0, 0);
                        }

                        // obj.attack(game.leader)  # fucks up the script
                        if (RandomRange(1, 100) <= 40)
                        {
                            var mang = Utilities.party_closest(obj);
                            StartTimer(RandomRange(1, 1500), () => timed_attack(obj, mang), true);
                        }

                    }

                }

            }
            else
            {
                // use the ones near the north, or at the back
                foreach (var obj in ObjList.ListVicinity(new locXY(465, 389), ObjectListFilter.OLC_NPC))
                {
                    var (xx, yy) = obj.GetLocation();
                    if (obj.GetLeader() == null && (new[] { 8045, 14066, 14078, 14154, 14156, 14162, 14163, 14164, 14165, 14248, 14249, 14296, 14337, 14338, 14339, 14381 }).Contains(obj.GetNameId()))
                    {
                        if ((new[] { 14296, 14381 }).Contains(obj.GetNameId())) // Earth Elementals
                        {
                            obj.SetInt(obj_f.speed_run, 1075644444);
                            obj.SetInt(obj_f.speed_walk, 1075064444);
                            StartTimer(450, () => timed_unconceal(obj), true);
                        }

                        if (xx >= 459 && xx <= 480 && troop_counter < N_max)
                        {
                            obj.Move(new locXY(x_troop + x_troop_add[troop_counter], y_troop + y_troop_add[troop_counter]), 0, 0);
                            troop_counter += 1;
                        }
                        else if (xx >= 459 && xx <= 477)
                        {
                            obj.Move(new locXY(xx + 15, yy), 0, 0);
                        }
                        else if (xx > 477 && xx <= 485 && yy <= 392)
                        {
                            obj.Move(new locXY(xx + 4, yy), 0, 0);
                        }
                        else if (xx > 477 && xx <= 485 && yy > 392)
                        {
                            obj.Move(new locXY(xx + 4, yy - 10), 0, 0);
                        }
                        else if (xx > 477 && yy > 392)
                        {
                            obj.Move(new locXY(xx, yy - 10), 0, 0);
                        }

                        // obj.attack(game.leader)  # fucks up the script
                        if (RandomRange(1, 100) <= 40)
                        {
                            var mang = Utilities.party_closest(obj);
                            StartTimer(RandomRange(1, 1500), () => timed_attack(obj, mang), true);
                        }

                    }

                }

            }

        }
        else if (barrier_smash_stage == 3)
        {
            // game.timevent_add(barrier_away, (OBJ_HANDLE_NULL, 3), 120, 1)
            if (npc != null && npc.GetNameId() == 14914)
            {
                npc.SetObjectFlag(ObjectFlag.OFF);
            }

            foreach (var obj in ObjList.ListVicinity(new locXY(470, 389), ObjectListFilter.OLC_NPC))
            {
                var (xx, yy) = obj.GetLocation();
                if (RandomRange(1, 100) <= 40 && xx >= 453 && xx <= 513 && (new[] { 8045, 14078, 14066, 14162, 14163, 14164, 14165, 14337, 14339, 14156, 14381, 14153, 14154 }).Contains(obj.GetNameId()) && obj.GetLeader() == null)
                {
                    var mang = Utilities.party_closest(obj);
                    StartTimer(RandomRange(200, 2200), () => timed_attack(obj, mang), true);
                }

            }

        }
        else if (barrier_smash_stage == 4)
        {
            // game.timevent_add(barrier_away, (OBJ_HANDLE_NULL, 3), 120, 1)
            if (npc != null && npc.GetNameId() == 14914)
            {
                npc.SetObjectFlag(ObjectFlag.OFF);
            }

        }

    }
    public static void timed_unconceal(GameObject obj)
    {
        obj.Unconceal();
    }
    public static void timed_attack(GameObject npc, GameObject pc)
    {
        npc.Attack(pc);
    }
    public static void switch_to_gatekeeper(GameObject pc, int line)
    {
        var (xx, yy) = pc.GetLocation();
        if (xx >= 466 && xx <= 501 && yy >= 378 && yy <= 420)
        {
            var gate_keeper_npc = GameSystems.MapObject.CreateObject(14915, pc.GetLocation());
            gate_keeper_npc.SetScriptId(ObjScriptEvent.StartCombat, 446);
            gate_keeper_npc.SetScriptId(ObjScriptEvent.EnterCombat, 446);
            gate_keeper_npc.RemoveScript(ObjScriptEvent.Heartbeat);
            gate_keeper_npc.SetScriptId(ObjScriptEvent.Dialog, 446);
            gate_keeper_npc.SetObjectFlag(ObjectFlag.DONTDRAW);
            gate_keeper_npc.SetObjectFlag(ObjectFlag.CLICK_THROUGH);
            pc.BeginDialog(gate_keeper_npc, line);
        }

    }

}