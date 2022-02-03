
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

[ObjectScript(434)]
public class TradersTrail : BaseObjectScript
{
    // pad_i_3 bit flags:

    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() == 5001))
        {
            attachee.TurnTowards(triggerer);
            var a = trail_check();
            if ((a == 1))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((a == 2))
            {
                triggerer.BeginDialog(attachee, 100);
            }
            else if ((a == 3))
            {
                triggerer.BeginDialog(attachee, 200);
            }
            else if ((a == 50))
            {
                triggerer.BeginDialog(attachee, 500);
            }
            else if ((a == 60))
            {
                triggerer.BeginDialog(attachee, 600);
            }
            else if ((a == 65))
            {
                triggerer.BeginDialog(attachee, 650);
            }
            else
            {
                triggerer.BeginDialog(attachee, 700);
            }

        }

        if ((attachee.GetMap() == 5051))
        {
            // tracked them to Nulb
            triggerer.BeginDialog(attachee, 2000);
        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() == 5049))
        {
            // stonemason
            var a = attachee.GetInt(obj_f.npc_pad_i_5);
            if ((ScriptDaemon.get_v(435) == 5 && ScriptDaemon.get_v(436) != 5 && ScriptDaemon.get_v(436) != 8))
            {
                foreach (var npc in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if (((npc.GetNameId() == 14801 || npc.GetNameId() == 14039) && npc.GetLeader() == null))
                    {
                        npc.Destroy();
                    }

                }

            }
            else if ((ScriptDaemon.get_v(435) == 4 && a == 0))
            {
                attachee.SetInt(obj_f.npc_pad_i_5, 1);
                GameObject gister = null;
                foreach (var npc in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if (npc.GetNameId() == 14039)
                    {
                        gister = npc;
                    }

                }

                var bad1 = GameSystems.MapObject.CreateObject(14801, new locXY(471, 485));
                bad1.Move(new locXY(471, 485), 0f, 0f);
                bad1.Rotation = 4;
                var bad2 = GameSystems.MapObject.CreateObject(14801, new locXY(474, 481));
                bad2.Move(new locXY(474, 481), 0f, 0f);
                bad2.Rotation = 3.5f;
                var bad3 = GameSystems.MapObject.CreateObject(14801, new locXY(473, 483));
                bad3.Move(new locXY(473, 483), 7f, 0f);
                bad3.Rotation = 3.9f;
                heavily_damage(gister);
                StartTimer(200, () => proactivity(bad3, 400));
            }

        }
        else if (attachee.GetMap() == 5009)
        {
            // traders' barn
            var ggv435 = ScriptDaemon.get_v(435);
            AttachParticles("sp-Bless Water", attachee);
            Council.council_heartbeat();
            var a = attachee.GetInt(obj_f.npc_pad_i_3);
            if ((ggv435 >= 3 || (ggv435 == 2 && Council.council_time() == 3)) && (a & 1) == 0)
            {
                a = a | 1;
                ScriptDaemon.npc_set(attachee, 1);
                var courier = Utilities.find_npc_near(attachee, 14063);
                courier.Destroy();
                if (GetQuestState(17) == QuestState.Mentioned || GetQuestState(17) == QuestState.Accepted)
                {
                    SetQuestState(17, QuestState.Botched);
                }

            }

            if ((a & 2) == 0 && (GetGlobalFlag(438)) && !(GetGlobalFlag(814) && GetGlobalFlag(815)))
            {
                a = a | 2;
                ScriptDaemon.npc_set(attachee, 2);
                var badger = GameSystems.MapObject.CreateObject(14371, new locXY(493, 487));
                GameObject pc = null;
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((obj.DistanceTo(badger) <= 25 && !Utilities.critter_is_unconscious(obj)))
                    {
                        pc = obj;
                    }

                }

                pc?.BeginDialog(badger, 5000);
            }

        }
        else if (attachee.GetMap() == 5051 && ScriptDaemon.get_v(437) == 100)
        {
            // Nulb exterior
            AttachParticles("sp-Bless Water", attachee);
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((obj.DistanceTo(attachee) <= 25 && !Utilities.critter_is_unconscious(obj)))
                {
                    DetachScript();
                    obj.BeginDialog(attachee, 2000);
                }

            }

        }
        else if (attachee.GetMap() == 5007)
        {
            // Inn, first floor
            var ggv435 = ScriptDaemon.get_v(435);
            var c = attachee.GetInt(obj_f.npc_pad_i_3);
            // c & 1 - spawned mickey
            // c & 2 - Glora, the new innkeeper
            // c & 4 - switched gundigoot off
            // c & 8 - destroyed gundigoot
            // c & 16 - switched Turuko off (since you killed him)
            // c & 32 - switched Kobort off (since you killed him)
            AttachParticles("sp-Bless Water", attachee);
            var c_time = Council.council_time();
            Council.council_heartbeat();
            if (GetGlobalFlag(45) && (c & 16) == 0)
            {
                c = c | 16;
                ScriptDaemon.npc_set(attachee, 5);
                var turuko = Utilities.find_npc_near(attachee, 8004);
                if (turuko != null)
                {
                    turuko.Destroy();
                }

            }

            if (GetGlobalFlag(44) && (c & 32) == 0)
            {
                c = c | 32;
                ScriptDaemon.npc_set(attachee, 6);
                var kobort = Utilities.find_npc_near(attachee, 8005);
                if (kobort != null)
                {
                    kobort.Destroy();
                    AttachParticles("orb-summon-water-elemental", GameSystems.Party.GetPartyGroupMemberN(1));
                }

            }

            if ((c & 1) == 0)
            {
                c = c | 1;
                ScriptDaemon.npc_set(attachee, 1);
                var mecr = GameSystems.MapObject.CreateObject(14637, new locXY(475, 475));
                mecr.Rotation = 2.5f;
                mecr.SetObjectFlag(ObjectFlag.OFF);
            }

            if ((c & 2) == 0 && (GetGlobalVar(436) == 6 || GetGlobalVar(436) == 7))
            {
                c = c | 2;
                ScriptDaemon.npc_set(attachee, 2);
                var glora = Utilities.find_npc_near(attachee, 14100);
                if (glora != null)
                {
                    glora.SetStandpoint(StandPointType.Day, 340);
                    glora.SetStandpoint(StandPointType.Night, 340);
                    glora.Rotation = 5f;
                }

            }

            if ((c_time == 1 || c_time == 2 || c_time == 5) && ((c & 4) == 0 || Utilities.find_npc_near(attachee, 8008) != null) && (c & 8) == 0)
            {
                c = c | 4;
                ScriptDaemon.npc_set(attachee, 3);
                var gundi = Utilities.find_npc_near(attachee, 8008);
                if (GetGlobalVar(436) == 6 || GetGlobalVar(436) == 7)
                {
                    gundi.Destroy();
                    c = c | 8;
                    ScriptDaemon.npc_set(attachee, 4);
                }
                else
                {
                    // gundi.move(location_from_axis(476,445),0.0,0.0)
                    gundi.SetScriptId(ObjScriptEvent.FirstHeartbeat, 73);
                    gundi.SetScriptId(ObjScriptEvent.Heartbeat, 73);
                    gundi.SetObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((c & 4) == 4 && (c & 8) == 0 && c_time != 1 && c_time != 2 && c_time != 5)
            {
                var gundi = Utilities.find_npc_near(attachee, 8008);
                if ((GetGlobalVar(436) == 6 || GetGlobalVar(436) == 7) && gundi != null)
                {
                    gundi.Destroy();
                    c = c | 8;
                    c = c - 4;
                    attachee.SetInt(obj_f.npc_pad_i_3, c);
                }
                else if (gundi != null)
                {
                    gundi.RemoveScript(ObjScriptEvent.FirstHeartbeat);
                    gundi.RemoveScript(ObjScriptEvent.Heartbeat);
                }

            }

        }
        else if (attachee.GetMap() == 5063)
        {
            // The Renton Residence (village militia captain)
            AttachParticles("orb-summon-water-elemental", attachee);
            var c_time = Council.council_time();
            Council.council_heartbeat();
            if ((c_time == 1 || c_time == 2 || c_time == 5) && Utilities.find_npc_near(attachee, 20007) != null)
            {
                var renton = Utilities.find_npc_near(attachee, 20007);
                renton.SetScriptId(ObjScriptEvent.FirstHeartbeat, 6);
                renton.SetScriptId(ObjScriptEvent.Heartbeat, 6);
                renton.SetObjectFlag(ObjectFlag.OFF);
            }

        }

        return SkipDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        var c = attachee.GetInt(obj_f.npc_pad_i_3);
        if (attachee.GetMap() == 5001)
        {
            // Hommlet exterior
            AttachParticles("sp-Bless Water", attachee);
            if ((c & 1) == 0 && GetGlobalVar(435) == 4)
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((obj.DistanceTo(attachee) <= 25 && !Utilities.critter_is_unconscious(obj)))
                    {
                        c = c | 1;
                        attachee.SetInt(obj_f.npc_pad_i_3, c);
                        var a = trail_check();
                        SetGlobalVar(437, a);
                        if ((a == 1))
                        {
                            obj.BeginDialog(attachee, 1);
                        }
                        else if ((a == 2))
                        {
                            obj.BeginDialog(attachee, 100);
                        }
                        else if ((a == 3))
                        {
                            obj.BeginDialog(attachee, 200);
                        }
                        else if ((a == 50))
                        {
                            obj.BeginDialog(attachee, 500);
                        }
                        else if ((a == 60))
                        {
                            obj.BeginDialog(attachee, 600);
                        }
                        else if ((a == 65))
                        {
                            obj.BeginDialog(attachee, 650);
                        }
                        else
                        {
                            obj.BeginDialog(attachee, 700);
                        }

                    }

                }

            }
            else if ((c & 2) == 2)
            {
                c = c - 2;
                attachee.SetInt(obj_f.npc_pad_i_3, c);
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((obj.DistanceTo(attachee) <= 25 && !Utilities.critter_is_unconscious(obj)))
                    {
                        obj.BeginDialog(attachee, 710);
                    }

                }

            }
            else if ((c & 4) == 4)
            {
                c = c - 4;
                attachee.SetInt(obj_f.npc_pad_i_3, c);
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((obj.DistanceTo(attachee) <= 25 && !Utilities.critter_is_unconscious(obj)))
                    {
                        obj.BeginDialog(attachee, 620);
                    }

                }

            }

        }
        else if (attachee.GetMap() == 5007)
        {
            // Inn, first floor
            // c & 1 - spawned mickey
            // c & 2 - Glora, the new innkeeper
            // c & 4 - switched gundigoot off
            AttachParticles("sp-Bless Water", attachee);
            var c_time = Council.council_time();
            Council.council_heartbeat();
            if ((c & 2) == 0 && (GetGlobalVar(436) == 6 || GetGlobalVar(436) == 7))
            {
                c = c | 2;
                attachee.SetInt(obj_f.npc_pad_i_3, c);
                var glora = Utilities.find_npc_near(attachee, 14100);
                if (glora != null)
                {
                    glora.SetStandpoint(StandPointType.Day, 340);
                    glora.SetStandpoint(StandPointType.Night, 340);
                    glora.Rotation = 5f;
                }

            }
            else if ((c_time == 1 || c_time == 2 || c_time == 5) && ((c & 4) == 0 || Utilities.find_npc_near(attachee, 8008) != null) && (c & 8) == 0)
            {
                c = c | 4;
                attachee.SetInt(obj_f.npc_pad_i_3, c);
                var gundi = Utilities.find_npc_near(attachee, 8008);
                if (GetGlobalVar(436) == 6 || GetGlobalVar(436) == 7)
                {
                    gundi.Destroy();
                    c = c | 8;
                    attachee.SetInt(obj_f.npc_pad_i_3, c);
                }
                else
                {
                    // gundi.move(location_from_axis(476,445),0.0,0.0)
                    gundi.SetScriptId(ObjScriptEvent.FirstHeartbeat, 73);
                    gundi.SetScriptId(ObjScriptEvent.Heartbeat, 73);
                    gundi.SetObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((c & 4) == 4 && (c & 8) == 0 && c_time != 1 && c_time != 2 && c_time != 5)
            {
                var gundi = Utilities.find_npc_near(attachee, 8008);
                if ((GetGlobalVar(436) == 6 || GetGlobalVar(436) == 7) && gundi != null)
                {
                    gundi.Destroy();
                    c = c | 8;
                    c = c - 4;
                    attachee.SetInt(obj_f.npc_pad_i_3, c);
                }
                else if (gundi != null)
                {
                    gundi.RemoveScript(ObjScriptEvent.FirstHeartbeat);
                    gundi.RemoveScript(ObjScriptEvent.Heartbeat);
                }

            }

        }
        else if (attachee.GetMap() == 5051 && GetGlobalVar(437) == 100)
        {
            // Nulb exterior
            AttachParticles("sp-Bless Water", attachee);
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((obj.DistanceTo(attachee) <= 25 && !Utilities.critter_is_unconscious(obj)))
                {
                    DetachScript();
                    obj.BeginDialog(attachee, 2000);
                }

            }

        }
        else if (attachee.GetMap() == 5009)
        {
            // traders' barn
            AttachParticles("sp-Bless Water", attachee);
            Council.council_heartbeat();
            var a = attachee.GetInt(obj_f.npc_pad_i_3);
            if ((GetGlobalVar(435) >= 3 || (GetGlobalVar(435) == 2 && Council.council_time() == 3)) && (a & 1) == 0)
            {
                a = a | 1;
                ScriptDaemon.npc_set(attachee, 1);
                var courier = Utilities.find_npc_near(attachee, 14063);
                courier.Destroy();
                if (GetQuestState(17) == QuestState.Mentioned || GetQuestState(17) == QuestState.Accepted)
                {
                    SetQuestState(17, QuestState.Botched);
                }

            }

            if ((a & 2) == 0 && (GetGlobalFlag(438)) && !(GetGlobalFlag(814) && GetGlobalFlag(815)))
            {
                a = a | 2;
                ScriptDaemon.npc_set(attachee, 2);
                var badger = GameSystems.MapObject.CreateObject(14371, new locXY(493, 487));
                GameObject pc = null;
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((obj.DistanceTo(badger) <= 25 && !Utilities.critter_is_unconscious(obj)))
                    {
                        pc = obj;
                    }

                }

                pc?.BeginDialog(badger, 5000);
            }

        }
        else if (attachee.GetMap() == 5063)
        {
            // The Renton Residence (village militia captain)
            AttachParticles("orb-summon-water-elemental", attachee);
            var c_time = Council.council_time();
            Council.council_heartbeat();
            if ((c_time == 1 || c_time == 2 || c_time == 5) && Utilities.find_npc_near(attachee, 20007) != null)
            {
                var renton = Utilities.find_npc_near(attachee, 20007);
                renton.SetScriptId(ObjScriptEvent.FirstHeartbeat, 6);
                renton.SetScriptId(ObjScriptEvent.Heartbeat, 6);
                renton.SetObjectFlag(ObjectFlag.OFF);
            }

        }

        return RunDefault;
    }
    public static int trail_check()
    {
        // returned number, meaning
        // 1 - has attack dog companion
        // 2 - has wolf companion
        // 3 - has jackal companion
        // 50 - made successful listen check
        // 60 - spotted tracks, but no tracking feat
        // 65 - spotted tracks, got tracking feat
        // look for canine animal companions
        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            if ((pc.HasFeat(FeatId.ANIMAL_COMPANION)))
            {
                foreach (var npc in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if ((is_canine_companion(npc) > 0 && npc.GetLeader() != null))
                    {
                        var abcd = is_canine_companion(npc);
                        return abcd;
                    }

                }

            }

        }

        // failing that, make listen check
        var listen_chk = RandomRange(1, 20);
        var highest_listen_modifier = 0;
        foreach (var obj in GameSystems.Party.PartyMembers)
        {
            if ((obj.GetSkillLevel(SkillId.listen) > highest_listen_modifier))
            {
                listen_chk = RandomRange(1, 20);
                if ((listen_chk + obj.GetSkillLevel(SkillId.listen) >= 20))
                {
                    return 50;
                }

            }

        }

        // failing that, the traders got away while you were searching (and looking away); but after a while you find their tracks
        // if you have the tracking feat, you can follow the tracks; otherwise it's all over in Hommlet
        var spot_highest = 0;
        foreach (var obj in GameSystems.Party.PartyMembers)
        {
            var spot_chk = RandomRange(1, 20);
            if ((spot_chk + obj.GetSkillLevel(SkillId.spot) >= spot_highest))
            {
                spot_highest = spot_chk + obj.GetSkillLevel(SkillId.spot);
            }

        }

        if ((spot_highest >= 20))
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if ((pc.HasFeat(FeatId.TRACK)))
                {
                    return 65;
                }

            }

            return 60;
        }

        return 0;
    }
    public static int is_canine_companion(GameObject testee)
    {
        // 14049 - attack dog
        // 14050 - wolf
        // 14051 - jackal
        if ((testee.GetNameId() == 14049))
        {
            return 1;
        }

        if ((testee.GetNameId() == 14050))
        {
            return 2;
        }

        if ((testee.GetNameId() == 14051))
        {
            return 3;
        }

        return 0;
    }
    public static void traders_reveal(GameObject triggerer, int rev_id)
    {
        GameObject gremag = null;
        GameObject rannos = null;
        foreach (var obj in ObjList.ListVicinity(triggerer.GetLocation(), ObjectListFilter.OLC_NPC))
        {
            if ((obj.GetNameId() == 8049))
            {
                gremag = obj;
            }
            else if ((obj.GetNameId() == 8048))
            {
                rannos = obj;
            }

        }

        if ((rev_id == 1 || rev_id == 2 || rev_id == 3))
        {
            gremag.AddCondition("prone", 0, 0);
        }
        else
        {
            gremag.TurnTowards(triggerer);
        }

        gremag.ClearObjectFlag(ObjectFlag.DONTDRAW);
        gremag.ClearObjectFlag(ObjectFlag.CLICK_THROUGH);
        rannos.ClearObjectFlag(ObjectFlag.DONTDRAW);
        rannos.ClearObjectFlag(ObjectFlag.CLICK_THROUGH);
        rannos.TurnTowards(triggerer);
        if ((rev_id == 1 || rev_id == 2 || rev_id == 3 || rev_id == 4))
        {
            StartTimer(700, () => traders_reveal_pt2(triggerer, rev_id, gremag, rannos, 800));
        }

        return;
    }
    public static void traders_reveal_pt2(GameObject triggerer, int rev_id, GameObject gremag, GameObject rannos, int line)
    {
        triggerer.BeginDialog(rannos, line);
        return;
    }
    public static void traders_runoff(GameObject attachee)
    {
        AttachParticles("orb-summon-fire-elemental", PartyLeader);
        var gremag = Utilities.find_npc_near(attachee, 8049);
        var rannos = Utilities.find_npc_near(attachee, 8048);
        if (gremag != null)
        {
            gremag.RunOff();
        }

        if (rannos != null)
        {
            rannos.RunOff(gremag.GetLocation().OffsetTiles(-3, 0));
        }

        if (!PartyLeader.HasReputation(23))
        {
            PartyLeader.AddReputation(23);
        }

        if (SelectedPartyLeader.GetMap() == 5051)
        {
            GameSystems.RandomEncounter.RemoveQueuedEncounter(3159);
        }

        attachee.Destroy();
        return;
    }
    public static void proactivity(GameObject npc, int line_no)
    {
        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            if ((!Utilities.critter_is_unconscious(pc) && pc.type == ObjectType.pc))
            {
                pc.BeginDialog(npc, line_no);
            }

        }

        return;
    }
    public static void heavily_damage(GameObject npc)
    {
        // note: this script kills an NPC
        // since the san_dying is triggered, it makes the game think you killed him
        // so to avoid problems, reduce global_vars[23] (which counts the # of Hommeletans killed) beforehand
        bool flag;
        if ((GetGlobalVar(23) == 0))
        {
            flag = false;
        }
        else
        {
            flag = true;
            SetGlobalVar(23, GetGlobalVar(23) - 1);
        }

        npc.Damage(null, DamageType.Poison, Dice.Parse("30d1"));
        npc.Damage(null, DamageType.Subdual, Dice.Parse("15d1"));
        if ((!flag && GetGlobalVar(23) > 0))
        {
            SetGlobalVar(23, GetGlobalVar(23) - 1);
        }

        if ((GetGlobalVar(23) < 2 && PartyLeader.HasReputation(92)))
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pc.RemoveReputation(92);
            }

        }

        return;
    }

}