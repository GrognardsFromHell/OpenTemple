
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

[ObjectScript(119)]
public class Romag : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((!attachee.HasMet(triggerer)))
        {
            ScriptDaemon.record_time_stamp(501);
            if ((GetGlobalVar(454) & 0x100) != 0)
            {
                triggerer.BeginDialog(attachee, 590);
            }
            else if ((GetGlobalFlag(91)))
            {
                triggerer.BeginDialog(attachee, 100);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

        }
        else
        {
            triggerer.BeginDialog(attachee, 300);
        }

        return RunDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(372)))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
        }
        else
        {
            if ((attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
            {
                SetGlobalVar(727, 0);
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

        SetGlobalFlag(104, true);
        ScriptDaemon.record_time_stamp(456);
        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(104, false);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive()))
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((!attachee.HasMet(obj)))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        ScriptDaemon.record_time_stamp(501);
                        if ((GetGlobalFlag(91)))
                        {
                            obj.TurnTowards(attachee); // added by Livonya
                            attachee.TurnTowards(obj); // added by Livonya
                            obj.BeginDialog(attachee, 100);
                        }
                        else if (((GetGlobalFlag(107)) || (GetGlobalFlag(105))))
                        {
                            obj.TurnTowards(attachee); // added by Livonya
                            attachee.TurnTowards(obj); // added by Livonya
                            obj.BeginDialog(attachee, 590);
                        }
                        else
                        {
                            obj.TurnTowards(attachee); // added by Livonya
                            attachee.TurnTowards(obj); // added by Livonya
                            obj.BeginDialog(attachee, 1);
                        }

                    }

                }

            }

        }

        // game.new_sid = 0		## removed by Livonya
        if ((GetGlobalVar(727) == 0 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
        {
            attachee.CastSpell(WellKnownSpells.OwlsWisdom, attachee);
            attachee.PendingSpellsToMemorized();
        }

        if ((GetGlobalVar(727) == 4 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
        {
            attachee.CastSpell(WellKnownSpells.ShieldOfFaith, attachee);
            attachee.PendingSpellsToMemorized();
        }

        SetGlobalVar(727, GetGlobalVar(727) + 1);
        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        romag_call_help(attachee);
        SetGlobalFlag(347, false);
        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        romag_call_help(attachee);
        if ((Utilities.obj_percent_hp(attachee) < 50 && (!attachee.HasMet(triggerer))))
        {
            GameObject found_pc = null;
            foreach (var pc in PartyLeader.GetPartyMembers())
            {
                attachee.AIRemoveFromShitlist(pc);
                if (pc.type == ObjectType.pc)
                {
                    found_pc = pc;
                }

            }

            if (found_pc != null)
            {
                ScriptDaemon.record_time_stamp(501);
                foreach (var pc in PartyLeader.GetPartyMembers())
                {
                    foreach (var npc in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                    {
                        if ((new[] { 14162, 14163, 14165, 14337, 14339, 14156 }).Contains(npc.GetNameId()) && npc.GetLeader() == null)
                        {
                            npc.AIRemoveFromShitlist(pc);
                        }

                    }

                }

                found_pc.BeginDialog(attachee, 200);
                return SkipDefault;
            }

        }

        return RunDefault;
    }
    public static bool talk_Hedrack(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 8046);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(attachee);
            attachee.TurnTowards(npc);
        }
        else
        {
            triggerer.BeginDialog(attachee, 580);
        }

        return SkipDefault;
    }
    public static bool escort_below(GameObject attachee, GameObject triggerer)
    {
        // game.global_flags[144] = 1
        SetGlobalVar(691, 1);
        attachee.SetStandpoint(StandPointType.Day, 267);
        attachee.SetStandpoint(StandPointType.Night, 267);
        FadeAndTeleport(0, 0, 0, 5080, 478, 451);
        return RunDefault;
    }
    public static void romag_call_help(GameObject attachee)
    {
        if (attachee.GetMap() == 5066 && (!ScriptDaemon.get_f("j_earth_commander_troops_temple_1")) && ScriptDaemon.within_rect_by_corners(attachee, 440, 432, 440, 451)) // temple level 1
        {
            attachee.FloatLine(1000, attachee);
            ScriptDaemon.set_f("j_earth_commander_troops_temple_1");
            var yyp_max = 439;
            var party_in_troop_room = false;
            var party_in_romag_room = false;
            var party_outside_romag_room = false;
            foreach (var pc in PartyLeader.GetPartyMembers())
            {
                var (xxp, yyp) = pc.GetLocation();
                if ((xxp >= 440 && xxp <= 452 && yyp >= 439 && yyp <= 451))
                {
                    party_in_romag_room = true;
                    if (yyp > yyp_max)
                    {
                        yyp_max = yyp;
                    }

                }

                if (xxp >= 453 && yyp <= 451)
                {
                    party_outside_romag_room = true;
                }

                if ((yyp >= 455 && yyp <= 471 && xxp >= 439 && xxp <= 457) || ((xxp - yyp) <= 1 && yyp <= 471 && xxp >= 456))
                {
                    party_in_troop_room = true;
                }

            }

            var y_troop = yyp_max + 18;
            y_troop = Math.Min(465, y_troop);
            var y_troop_add = new[] { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 3 };
            var x_troop_add = new[] { 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 4, 5, -1, -2, -3 };
            var x_troop = 446;
            var troop_counter = 0;
            if (party_outside_romag_room && !party_in_romag_room)
            {
                y_troop = 446;
                x_troop = 445;
                y_troop_add = new[] { 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 0, 1, 2, 3, 0, 1 };
                x_troop_add = new[] { 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, -1, -1, -1, -1, -2, -2 };
            }

            if (!party_in_troop_room)
            {
                foreach (var npc in ObjList.ListVicinity(new locXY(453, 463), ObjectListFilter.OLC_NPC))
                {
                    if ((new[] { 14162, 14163, 14165, 14337, 14339, 14156 }).Contains(npc.GetNameId()) && npc.GetLeader() == null && !npc.IsUnconscious())
                    {
                        var (xx, yy) = npc.GetLocation();
                        if (yy > y_troop)
                        {
                            npc.Move(new locXY(x_troop + x_troop_add[troop_counter], y_troop + y_troop_add[troop_counter]), 0, 0);
                            troop_counter += 1;
                        }

                    }

                }

            }

            // npc.attack(game.leader)
            foreach (var npc in ObjList.ListVicinity(new locXY(453, 463), ObjectListFilter.OLC_NPC))
            {
                if ((new[] { 14162, 14163, 14165, 14337, 14339, 14156 }).Contains(npc.GetNameId()) && npc.GetLeader() == null)
                {
                    npc.Attack(SelectedPartyLeader);
                }

            }

        }

    }

}