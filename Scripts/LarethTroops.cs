
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
    [ObjectScript(450)]
    public class LarethTroops : BaseObjectScript
    {
        // IMPORTANT NOTICE!
        // I have directly copied many scripts instead of using functions.
        // This is because when you call another script, it often fails to execute.
        // Thus I wished to cut back on them as much as possible.
        // Flags / Vars documentation:
        // ggv400: Bitwise flags
        // 2**0 - Seleucas engaged in battle
        // 2**1 - Seleucas dead
        // 2**2 - Lareth has been called upon by his men
        // 2**3 - Lareth has entered the fray
        // 2**4 - Abort Lareth's villain speech
        // 2**5 - Entered combat with Lareth without having entered combat with Seleucas
        // ggv401: Mini variables
        // 0-2: Lareth Float Comment Stage
        // 3-6: Seleucas Battle Death count
        // 7-10: Seleucas Battle Knockout count
        // 11-14: Seleucas Battle Charm count
        // ggv403: Moathouse  Reactive behavior flags
        // 0 - Moathouse regrouped;
        // 1 - New guard guy shouts out to Seleucas - enables his KOS;
        // 2 - KOS thing played out
        // pad3:
        // 2**1 - marked as having contributed to Seleucas Battle Knockout count
        // 2**2 - marked as having contributed to Seleucas Battle Charm count
        // 2**5 - marked as having contributed to Seleucas Battle Death count
        // 2**6 - solo guard who went to alert Seleucas
        // 2**7 - have had Sleep successfully cast upon (see Spell438 - Sleep.py)
        // 2**8 - Beacon designator
        // 2**9 - Archer guy put to sleep - skip turn when woken up to prevent approach, then reset

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
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
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer, GameObjectBody generated_from_timed_event_call = 0, GameObjectBody kill_count)
        {
            if (GetGlobalFlag(403) && GetGlobalFlag(405))
            {
                attachee.FloatMesFileLine("mes/script_activated.mes", 12, TextFloaterColor.Red);
            }

            var ggv400 = GetGlobalVar(400);
            var ggv401 = GetGlobalVar(401);
            var pad3 = attachee.GetInt(obj_f.npc_pad_i_3);
            if (generated_from_timed_event_call == 0)
            {
                GetGlobalVar(756) += 1;
                StartTimer(100 + RandomRange(0, 50), () => san_dying(attachee, triggerer, 1), true);
            }

            if ((ggv400 & (Math.Pow(2, 0))) != 0) // Fighting Seleucas
            {
                if (attachee.IsUnconscious() && (pad3 & (Math.Pow(2, 5))) == 0)
                {
                    pad3 += (Math.Pow(2, 5));
                    attachee.SetInt(obj_f.npc_pad_i_3, pad3); // Mark him as having contributed to the dying count
                    ggv401 += (1 << 3);
                    SetGlobalVar(401, ggv401);
                }

                // Was this guy unconscious before dying?
                // (thus having contributed to the unconscious count)
                // Unmark his internal flag and reduce the count of unconscious guys
                if ((pad3 & (Math.Pow(2, 1))) != 0)
                {
                    pad3 ^= (Math.Pow(2, 1));
                    attachee.SetInt(obj_f.npc_pad_i_3, pad3);
                    if (((ggv401 >> 7) & 15) > 0)
                    {
                        ggv401 -= (1 << 7);
                    }

                    SetGlobalVar(401, ggv401);
                }

            }

            if (attachee.GetNameId() == 14077)
            {
                // Seleucas dead
                ggv400 |= Math.Pow(2, 1);
                SetGlobalVar(400, ggv400);
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (GetGlobalFlag(403) && GetGlobalFlag(405))
            {
                attachee.FloatMesFileLine("mes/script_activated.mes", 13, TextFloaterColor.Red);
            }

            if (attachee.GetNameId() == 14077) // Seleucas
            {
                var ggv400 = GetGlobalVar(400);
                var ggv403 = GetGlobalVar(403);
                ggv400 = ggv400 | (Math.Pow(2, 0)); // indicate that you have entered combat with Seleucas' group
                SetGlobalVar(400, ggv400);
                if ((ggv403 & (Math.Pow(2, 1))) != 0) // Moathouse regroup scenario; Seleucas is warned by guard of approaching party
                {
                    ggv403 |= Math.Pow(2, 2);
                    SetGlobalVar(403, ggv403);
                }

                attachee.RemoveScript(ObjScriptEvent.Heartbeat); // His heartbeat script - disabled, in case hostilies momentarily cease / fight ends with him charmed
                if (GetGlobalFlag(403) && GetGlobalFlag(405))
                {
                    SelectedPartyLeader.FloatMesFileLine("mes/script_activated.mes", 10001, TextFloaterColor.Red);
                }

            }
            else if (attachee.GetNameId() == 14074)
            {
                var pad3 = attachee.GetInt(obj_f.npc_pad_i_3);
                if ((pad3 & (Math.Pow(2, 6))) != 0)
                {
                    attachee.FloatLine(15002, triggerer);
                    attachee.RemoveScript(ObjScriptEvent.Dialog);
                    pad3 ^= Math.Pow(2, 6);
                    attachee.SetInt(obj_f.npc_pad_i_3, pad3);
                }

            }

            var a123 = attachee.ItemWornAt(EquipSlot.WeaponPrimary).GetInt(obj_f.weapon_type);
            if ((new[] { 14, 17, 46, 48, 68 }).Contains(a123))
            {
                // 14 - light crossbow
                // 17 - heavy crossbow
                // 46 - shortbow
                // 48 - longbow
                // 68 - repeating crossbow
                var dummy = 1;
            }
            else
            {
                Livonya.tag_strategy(attachee);
                if ((new[] { 37, 41, 43, 44, 61 }).Contains(a123))
                {
                    // Reach Weapons
                    Livonya.get_melee_reach_strategy(attachee);
                }
                else
                {
                    Livonya.get_melee_strategy(attachee);
                }

            }

        }
        public override bool OnExitCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (GetGlobalFlag(403) && GetGlobalFlag(405))
            {
                attachee.FloatMesFileLine("mes/script_activated.mes", 14, TextFloaterColor.Red);
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer, GameObjectBody generated_from_timed_event_call = 0)
        {
            if (attachee.GetBaseStat(Stat.dexterity) == -30)
            {
                // Beacon object - skip its turn in case it somehow survives
                attachee.Destroy();
                return SkipDefault;
            }

            if (GetGlobalFlag(403) && GetGlobalFlag(405))
            {
                attachee.FloatMesFileLine("mes/script_activated.mes", 15, TextFloaterColor.Red);
            }

            var ggv400 = GetGlobalVar(400);
            var ggv401 = GetGlobalVar(401);
            var pad3 = attachee.GetInt(obj_f.npc_pad_i_3);
            if ((ggv400 & (Math.Pow(2, 0))) != 0)
            {
                if (attachee.IsUnconscious() && (pad3 & (Math.Pow(2, 1))) == 0)
                {
                    // Troop knocked unconscious - mark him as such and increase the KOed counter
                    pad3 |= Math.Pow(2, 1);
                    attachee.SetInt(obj_f.npc_pad_i_3, pad3);
                    ggv401 += 1 << 7;
                    SetGlobalVar(401, ggv401);
                }
                else if ((pad3 & (Math.Pow(2, 1))) != 0 && !attachee.IsUnconscious())
                {
                    // Attachee has contributed to unconscious count, but is no longer unconscious
                    // E.g. healed
                    pad3 &= ~(Math.Pow(2, 1));
                    if (((ggv401 >> 7) & 15) > 0)
                    {
                        ggv401 -= (1 << 7);
                    }

                    attachee.SetInt(obj_f.npc_pad_i_3, pad3);
                    SetGlobalVar(401, ggv401);
                }

                if ((new[] { 14074, 14075, 14076, 14077 }).Contains(attachee.GetNameId()))
                {
                    // The "Call Lareth" section
                    // Calculates how many troops are down etc. and decides whether to call Lareth
                    GameObjectBody seleucas = null;
                    GameObjectBody lareth_sarge = null;
                    foreach (var obj1 in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                    {
                        if (obj1.GetNameId() == 14077)
                        {
                            seleucas = obj1;
                        }
                        else if (obj1.GetNameId() == 14076)
                        {
                            lareth_sarge = obj1;
                        }

                    }

                    if ((seleucas != null && seleucas.IsUnconscious()) || (ggv400 & (Math.Pow(2, 1))) != 0 || (seleucas.GetLeader() != null))
                    {
                        var seleucas_down = 1;
                    }
                    else
                    {
                        var seleucas_down = 0;
                    }

                    if ((lareth_sarge != null && lareth_sarge.IsUnconscious()))
                    {
                        var lareth_sarge_down = 1;
                    }
                    else
                    {
                        var lareth_sarge_down = 0;
                    }

                    var troops_down = ((ggv401 >> 3) & 15) + ((ggv401 >> 7) & 15);
                    var should_call_lareth = 0;
                    if (seleucas_down && (troops_down >= 2)) // Seleucas + 1 other troop
                    {
                        should_call_lareth = 1;
                    }
                    else if (seleucas_down && lareth_sarge_down) // Seleucas + Sergeant
                    {
                        should_call_lareth = 1;
                    }
                    else if (troops_down >= 2)
                    {
                        should_call_lareth = 1;
                    }

                    if (should_call_lareth)
                    {
                        if ((ggv400 & ((Math.Pow(2, 2)) + (Math.Pow(2, 5)))) == 0)
                        {
                            // Lareth has not been called upon
                            // And have not initiated combat with Lareth
                            if (seleucas_down && !attachee.IsUnconscious() && (attachee.GetLeader() == null))
                            {
                                // Seleucas is down - the soldier calls Lareth
                                var temppp = attachee.GetScriptId(ObjScriptEvent.Dialog);
                                attachee.SetScriptId(ObjScriptEvent.Dialog, 450);
                                attachee.FloatLine(15001, triggerer);
                                attachee.SetScriptId(ObjScriptEvent.Dialog, temppp);
                                ggv400 |= Math.Pow(2, 2);
                                SetGlobalVar(400, ggv400);
                            }
                            else if (seleucas_down == 0)
                            {
                                // Seleucas is alive - Seleucas calls Lareth
                                seleucas.FloatLine(15000, triggerer);
                                ggv400 |= Math.Pow(2, 2);
                                SetGlobalVar(400, ggv400);
                            }

                        }
                        else if ((ggv400 & ((Math.Pow(2, 3)) + (Math.Pow(2, 5)))) == 0)
                        {
                            var lareth = Utilities.find_npc_near(attachee, 8002);
                            if (lareth != null && !lareth.IsUnconscious())
                            {
                                // if can_see_party(lareth) == 0:
                                lareth.SetInt(obj_f.critter_strategy, 81); // Lareth's "Join the Fray" strategy - start with shield of faith, then target hurt friendly, heal, and commence attack
                                lareth.SetScriptId(ObjScriptEvent.EndCombat, 60); // san_end_combat
                                lareth.Move(new locXY(476, 546), -60, 10); // This is to decrease the chances of Lareth skipping his turn
                                var closest_party_member = SelectedPartyLeader;
                                foreach (var pcc in GameSystems.Party.PartyMembers)
                                {
                                    if (pcc.DistanceTo(attachee) < closest_party_member.DistanceTo(attachee))
                                    {
                                        closest_party_member = pcc;
                                    }

                                }

                                lareth.CastSpell(WellKnownSpells.ShieldOfFaith, lareth);
                                lareth.Attack(closest_party_member);
                                var init_current = -100;
                                foreach (var pc in GameSystems.Party.PartyMembers)
                                {
                                    if (pc.GetInitiative() < attachee.GetInitiative() && (pc.GetInitiative() - 2) > init_current)
                                    {
                                        init_current = pc.GetInitiative() - 2;
                                    }

                                }

                                lareth.SetInitiative(init_current); // make sure he gets to act on the same round
                                ggv400 |= Math.Pow(2, 3);
                                SetGlobalVar(400, ggv400);
                            }

                        }

                    }

                }

                if (attachee.GetLeader() != null && (pad3 & (Math.Pow(2, 2))) == 0)
                {
                    // Attachee charmed
                    pad3 |= Math.Pow(2, 2);
                    attachee.SetInt(obj_f.npc_pad_i_3, pad3);
                    ggv401 += (1 << 11);
                    SetGlobalVar(401, ggv401);
                }

            }

            // Copied from script 310 :	#
            // THIS IS USED FOR BREAK FREE
            while ((attachee.FindItemByName(8903) != null))
            {
                attachee.FindItemByName(8903).Destroy();
            }

            if ((attachee.D20Query(D20DispatcherKey.QUE_Is_BreakFree_Possible))) // workaround no longer necessary!
            {
                return RunDefault;
            }

            // create_item_in_inventory( 8903, attachee )
            // attachee.d20_send_signal(S_BreakFree)
            // Wake up from Sleep Scripting	#
            "\n\n\tif generated_from_timed_event_call == 0 and attachee.is_unconscious() == 0 and attachee.d20_query(Q_Prone) == 0 and attachee.leader_get() == OBJ_HANDLE_NULL:\n\t\t# checks if attachee is capable of waking someone up (i.e. not prone, KOed or charmed)\n\t\tfor obj in game.obj_list_cone(attachee, OLC_NPC, 10, 0, 360):\n\t\t\tif obj.name in range(14074, 14078) and obj.distance_to(attachee) <= 4.2 and obj != attachee:\n\t\t\t\t# Scan all NPCs in arm's reach\n\t\t\t\tobj_pad3 = obj.obj_get_int(obj_f_npc_pad_i_3)\n\t\t\t\tif obj_pad3 & (2**7) != 0:\n\t\t\t\t\t# NPC was victim of \"Sleep\" spell\n\t\t\t\t\tobj_pad3 ^= 2**7\n\t\t\t\t\tobj.obj_set_int(obj_f_npc_pad_i_3, obj_pad3)\n\t\t\t\t\tif attachee.scripts[9] == 0:\n\t\t\t\t\t\tattachee.scripts[9] = 450 ##Set the correct san_dialog\n\t\t\t\t\tattachee.turn_towards(obj)\n\t\t\t\t\tattachee.float_line( 16500 + game.random_range(0, 3), attachee )\n\t\t\t\t\tobj.damage(OBJ_HANDLE_NULL, D20DT_SUBDUAL, dice_new('1d1') )\n\t\t\t\t\t#obj_pad3 ^= 2**9 #marks obj as having just been woken up\n\t\t\t\t\t#obj.obj_set_int(obj_f_npc_pad_i_3, obj_pad3)\n\t\t\t\t\treturn SKIP_DEFAULT\n\t\t\t\n\tif (pad3 & (2**9) != 0) and attachee.is_unconscious() == 0:\n\t\t# Restore woken up archer's strategey\n\t\t# Note: woken up critters don't fire their san_start_combat script on the turn they get up\n\t\tpad3 ^= 2**9\n\t\tattachee.obj_set_int(obj_f_npc_pad_i_3, pad3)\n\t\tattachee.obj_set_int(obj_f_critter_strategy, attachee.obj_get_int(obj_f_pad_i_0))\n\n\t";
            var a123 = attachee.ItemWornAt(EquipSlot.WeaponPrimary).GetInt(obj_f.weapon_type);
            if ((new[] { 14, 17, 46, 48, 68 }).Contains(a123)) // (Is archer)
            {
                // 14 - light crossbow
                // 17 - heavy crossbow
                // 46 - shortbow
                // 48 - longbow
                // 68 - repeating crossbow
                if (generated_from_timed_event_call == 0 && !attachee.IsUnconscious() && !attachee.D20Query(D20DispatcherKey.QUE_Prone) && attachee.GetLeader() == null)
                {
                    var player_in_obscuring_mist = 0;
                    var grease_detected = 0;
                    var should_wake_up_comrade = 0;
                    // First, find closest party member - the most likely target for an archer
                    var closest_pc_1 = SelectedPartyLeader;
                    foreach (var pc in PartyLeader.GetPartyMembers())
                    {
                        if (pc.DistanceTo(attachee) < closest_pc_1.DistanceTo(attachee))
                        {
                            closest_pc_1 = pc;
                        }

                    }

                    foreach (var spell_obj in ObjList.ListCone(closest_pc_1, ObjectListFilter.OLC_GENERIC, 30, 0, 360))
                    {
                        // Check for active OBSCURING MIST spell objects
                        if (spell_obj.GetInt(obj_f.secretdoor_dc) == 333 + (1 << 15) && spell_obj.DistanceTo(closest_pc_1) <= 17.5f)
                        {
                            player_in_obscuring_mist = 1;
                        }

                    }

                    foreach (var spell_obj in ObjList.ListCone(closest_pc_1, ObjectListFilter.OLC_GENERIC, 40, 0, 360))
                    {
                        // Check for active GREASE spell object
                        if (spell_obj.GetInt(obj_f.secretdoor_dc) == 200 + (1 << 15))
                        {
                            grease_detected = 1;
                            var grease_obj = spell_obj;
                        }

                    }

                    // Scripting for approaching sleeping friend
                    if (player_in_obscuring_mist == 1 || RandomRange(0, 1) == 0)
                    {
                        // Player staying back in Obscuring Mist - thus it's more useful to use the archer to wake someone up
                        // Otherwise, 50% chance
                        should_wake_up_comrade = 1;
                    }

                    if (closest_pc_1.DistanceTo(attachee) <= 8)
                    {
                        // Player is close - Attachee will probably get hit trying to approach friend, so abort
                        should_wake_up_comrade = 0;
                    }

                    "\n\t\t\tif should_wake_up_comrade == 1:\n\t\t\t\tfor obj in game.obj_list_cone(attachee, OLC_NPC, 45, 0, 360):\n\t\t\t\t\tif obj.distance_to(attachee) <= 17 and  (   (obj.obj_get_int(obj_f_npc_pad_i_3) & 2**7)  !=  0   ):\n\t\t\t\t\t\t# Draw a line between attachee and obj (the wake-up target)\n\t\t\t\t\t\t# If a PC is detected inbetween, abort!\n\t\t\t\t\t\tx0, y0 = location_to_axis(attachee.location)\n\t\t\t\t\t\tx1, y1 = location_to_axis(obj.location)\n\t\t\t\t\t\tsomeone_elses_problem = 0\n\t\t\t\t\t\tfor qq in range(0, 26):\n\t\t\t\t\t\t\txs = int( x0 + ( (qq * (x1-x0))/25 ) )\n\t\t\t\t\t\t\tys = int( y0 + ( (qq * (y1-y0))/25 ) )\n\t\t\t\t\t\t\tif grease_detected:\t\t\t\t\n\t\t\t\t\t\t\t\tdummy =1\n\t\t\t\t\t\t\t\tgx,gy = location_to_axis(grease_obj.location)\n\t\t\t\t\t\t\t\tif abs(xs - gx) + abs(ys - gy) <= 9:\n\t\t\t\t\t\t\t\t\tsomeone_elses_problem = 1\n\t\t\t\t\t\t\tfor pc in game.party:\n\t\t\t\t\t\t\t\tpcx, pcy = location_to_axis(pc.location)\n\t\t\t\t\t\t\t\tif abs(pcx-xs) + abs(pcy-ys) <= 2:\n\t\t\t\t\t\t\t\t\tsomeone_elses_problem = 1\n\t\t\t\t\t\tif someone_elses_problem == 0:\n\t\t\t\t\t\t\tobj_beacon = game.obj_create(14074, obj.location)\n\t\t\t\t\t\t\tobj_beacon.object_flag_set(OF_DONTDRAW)\n\t\t\t\t\t\t\tobj_beacon.object_flag_set(OF_CLICK_THROUGH)\n\t\t\t\t\t\t\tobj_beacon.move(obj.location,0,0)\n\t\t\t\t\t\t\tobj_beacon.stat_base_set( stat_dexterity, -30 )\n\t\t\t\t\t\t\tobj_beacon.obj_set_int(obj_f_npc_pad_i_3, 2**8 )\n\t\t\t\t\t\t\t\n\t\t\t\t\t\t\tattachee.obj_set_int(obj_f_pad_i_0, attachee.obj_get_int(obj_f_critter_strategy) ) # Record original strategy\n\t\t\t\t\t\t\tattachee.obj_set_int(obj_f_critter_strategy, 84) # \"Seek low AC ally\" strat\n\t\t\t\t\t\t\tif attachee.scripts[9] == 0:\n\t\t\t\t\t\t\t\tattachee.scripts[9] = 450\n\t\t\t\t\t\t\tif game.random_range(0, 5) == 0:\n\t\t\t\t\t\t\t\tif attachee.scripts[9] == 0:\n\t\t\t\t\t\t\t\t\tattachee.scripts[9] = 450\n\t\t\t\t\t\t\t\tattachee.float_line( 16510 + game.random_range(0,1), triggerer)\n\t\t\t\t\t\t\treturn RUN_DEFAULT\n\t\t\t\t\t\t\n\t\t\t";
                    if (player_in_obscuring_mist == 1 && RandomRange(1, 12) == 1)
                    {
                        // Player Cast Obscuring Mist -
                        // Float a comment about not being able to see the player (1 in 12 chance)
                        if (attachee.GetScriptId(ObjScriptEvent.Dialog) == 0)
                        {
                            attachee.SetScriptId(ObjScriptEvent.Dialog, 450);
                        }

                        attachee.FloatLine(16520 + RandomRange(0, 2), attachee);
                    }

                }

            }
            else
            {
                Livonya.tag_strategy(attachee);
                if ((new[] { 37, 41, 43, 44, 61 }).Contains(a123))
                {
                    // Reach Weapons
                    Livonya.get_melee_reach_strategy(attachee);
                }
                else
                {
                    SetGlobalVar(499, 19);
                    Livonya.get_melee_strategy(attachee);
                }

            }

            // Spiritual Weapon Shenanigens	#
            CombatStandardRoutines.Spiritual_Weapon_Begone(attachee);
        }
        public override bool OnEndCombat(GameObjectBody attachee, GameObjectBody triggerer, GameObjectBody generated_from_timed_event_call = 0)
        {
            var ggv400 = GetGlobalVar(400);
            var ggv401 = GetGlobalVar(401);
            var pad3 = attachee.GetInt(obj_f.npc_pad_i_3);
            if (GetGlobalFlag(403) && GetGlobalFlag(405))
            {
                attachee.FloatMesFileLine("mes/script_activated.mes", 16, TextFloaterColor.Red);
            }

            if ((ggv400 & (Math.Pow(2, 0))) != 0)
            {
                if (attachee.IsUnconscious() && (pad3 & (Math.Pow(2, 1))) == 0)
                {
                    pad3 |= Math.Pow(2, 1);
                    attachee.SetInt(obj_f.npc_pad_i_3, pad3);
                    ggv401 += 1 << 7;
                    SetGlobalVar(401, ggv401);
                }
                else if ((pad3 & (Math.Pow(2, 1))) != 0 && !attachee.IsUnconscious())
                {
                    // Attachee has contributed to unconscious count, but is no longer unconscious
                    // E.g. healed
                    pad3 &= ~(Math.Pow(2, 1));
                    if (((ggv401 >> 7) & 15) > 0)
                    {
                        ggv401 -= (1 << 7);
                    }

                    attachee.SetInt(obj_f.npc_pad_i_3, pad3);
                    SetGlobalVar(401, ggv401);
                }

                if ((new[] { 14074, 14075, 14076, 14077 }).Contains(attachee.GetNameId()))
                {
                    GameObjectBody seleucas = null;
                    GameObjectBody lareth_sarge = null;
                    foreach (var obj1 in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                    {
                        if (obj1.GetNameId() == 14077)
                        {
                            seleucas = obj1;
                        }
                        else if (obj1.GetNameId() == 14076)
                        {
                            lareth_sarge = obj1;
                        }

                    }

                    if ((seleucas != null && seleucas.IsUnconscious()) || (ggv400 & (Math.Pow(2, 1))) != 0 || seleucas.GetLeader() != null)
                    {
                        var seleucas_down = 1;
                    }
                    else
                    {
                        var seleucas_down = 0;
                    }

                    if ((lareth_sarge != null && lareth_sarge.IsUnconscious()))
                    {
                        var lareth_sarge_down = 1;
                    }
                    else
                    {
                        var lareth_sarge_down = 0;
                    }

                    var troops_down = ((ggv401 >> 3) & 15) + ((ggv401 >> 7) & 15);
                    var should_call_lareth = 0;
                    if (seleucas_down && (troops_down >= 2)) // Seleucas + 1 other troop
                    {
                        should_call_lareth = 1;
                    }
                    else if (seleucas_down && lareth_sarge_down) // Seleucas + Sergeant
                    {
                        should_call_lareth = 1;
                    }
                    else if (troops_down >= 2)
                    {
                        should_call_lareth = 1;
                    }

                    if (should_call_lareth)
                    {
                        if ((ggv400 & ((Math.Pow(2, 2)) + (Math.Pow(2, 5)))) == 0)
                        {
                            // Lareth has not been called upon
                            // And have not initiated combat with Lareth
                            if (seleucas_down && !attachee.IsUnconscious() && attachee.GetLeader() == null)
                            {
                                var temppp = attachee.GetScriptId(ObjScriptEvent.Dialog);
                                attachee.SetScriptId(ObjScriptEvent.Dialog, 450);
                                attachee.FloatLine(15001, triggerer);
                                attachee.SetScriptId(ObjScriptEvent.Dialog, temppp);
                                ggv400 |= Math.Pow(2, 2);
                                SetGlobalVar(400, ggv400);
                            }
                            else if (seleucas_down == 0)
                            {
                                seleucas.FloatLine(15000, triggerer);
                                ggv400 |= Math.Pow(2, 2);
                                SetGlobalVar(400, ggv400);
                            }

                        }
                        else if ((ggv400 & (Math.Pow(2, 3))) == 0)
                        {
                            var lareth = Utilities.find_npc_near(attachee, 8002);
                            if (lareth != null && !lareth.IsUnconscious())
                            {
                                // if can_see_party(lareth) == 0:
                                lareth.SetInt(obj_f.critter_strategy, 81); // Lareth's "Join the Fray" strategy - start with shield of faith, then target hurt friendly, heal, and commence attack
                                lareth.SetScriptId(ObjScriptEvent.EndCombat, 60); // san_end_combat
                                lareth.Move(new locXY(476, 546), -60, 10); // This is to decrease the chances of Lareth skipping his turn
                                var closest_party_member = SelectedPartyLeader;
                                foreach (var pcc in GameSystems.Party.PartyMembers)
                                {
                                    if (pcc.DistanceTo(attachee) < closest_party_member.DistanceTo(attachee))
                                    {
                                        closest_party_member = pcc;
                                    }

                                }

                                lareth.CastSpell(WellKnownSpells.ShieldOfFaith, lareth);
                                lareth.Attack(closest_party_member);
                                var init_current = -100;
                                foreach (var pc in GameSystems.Party.PartyMembers)
                                {
                                    if (pc.GetInitiative() < attachee.GetInitiative() && (pc.GetInitiative() - 2) > init_current)
                                    {
                                        init_current = pc.GetInitiative() - 2;
                                    }

                                }

                                lareth.SetInitiative(init_current); // make sure he gets to act on the same round
                                ggv400 |= Math.Pow(2, 3);
                                SetGlobalVar(400, ggv400);
                            }

                        }

                    }

                }

                if (attachee.GetLeader() != null && (pad3 & (Math.Pow(2, 2))) == 0)
                {
                    // Attachee charmed
                    pad3 |= Math.Pow(2, 2);
                    attachee.SetInt(obj_f.npc_pad_i_3, pad3);
                    ggv401 += (1 << 11);
                    SetGlobalVar(401, ggv401);
                }

            }

            if (generated_from_timed_event_call == 0 && !attachee.IsUnconscious() && !attachee.D20Query(D20DispatcherKey.QUE_Prone))
            {
                // Wake up sleeping guy script
                var bbb = attachee.GetInt(obj_f.critter_strategy);
                if (bbb == 84) // if using the 'seek beacon' strategy
                {
                    bbb = attachee.GetInt(obj_f.pad_i_0); // retrieve previous strat
                    attachee.SetInt(obj_f.critter_strategy, bbb);
                    var has_woken_someone_up = 0;
                    foreach (var obj in ObjList.ListCone(attachee, ObjectListFilter.OLC_NPC, 10, 0, 360))
                    {
                        if ((range(14074, 14078)).Contains(obj.GetNameId()) && obj != attachee)
                        {
                            var obj_pad3 = obj.GetInt(obj_f.npc_pad_i_3);
                            if ((obj_pad3 & (Math.Pow(2, 8))) != 0) // is a beacon object
                            {
                                obj.Destroy();
                            }
                            else if ((obj_pad3 & (Math.Pow(2, 7))) != 0 && obj.DistanceTo(attachee) <= 4.2f && has_woken_someone_up == 0)
                            {
                                obj_pad3 &= ~(Math.Pow(2, 7)); // remove "sleepig flag"
                                obj.SetInt(obj_f.npc_pad_i_3, obj_pad3);
                                if (attachee.GetScriptId(ObjScriptEvent.Dialog) == 0)
                                {
                                    attachee.SetScriptId(ObjScriptEvent.Dialog, 450); // Set the correct san_dialog
                                }

                                attachee.TurnTowards(obj);
                                attachee.FloatLine(16500 + RandomRange(0, 3), attachee);
                                obj.Damage(null, DamageType.Subdual, Dice.Parse("1d1"));
                                has_woken_someone_up = 1;
                            }

                        }

                    }

                }

            }

        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()) && (!GetGlobalFlag(363)) && (attachee.GetLeader() == null))
            {
                if ((attachee.DistanceTo(PartyLeader) <= 22 && attachee.HasLineOfSight(PartyLeader) && !Utilities.critter_is_unconscious(PartyLeader)))
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
                else
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((attachee.DistanceTo(obj) <= 20 && attachee.HasLineOfSight(obj) && !Utilities.critter_is_unconscious(obj)))
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

            // Don't scrap the script - because of the "bring the leader here" treatment
            return RunDefault;
        }
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (attachee.DistanceTo(triggerer) <= 31 && (GetGlobalVar(403) & ((Math.Pow(2, 1)) + (Math.Pow(2, 2)))) == 2)
            {
                return RunDefault;
            }

            return SkipDefault;
        }
        public override bool OnSpellCast(GameObjectBody attachee, GameObjectBody triggerer, SpellPacketBody spell)
        {
            if (spell.spellEnum == WellKnownSpells.Fireball)
            {
                StartTimer(1250 + RandomRange(0, 1000), () => san_start_combat(attachee, triggerer, 1), true);
                if (RandomRange(0, 5) == 0 && attachee.GetNameId() == 14074)
                {
                    if (attachee.GetScriptId(ObjScriptEvent.Dialog) == 0)
                    {
                        attachee.SetScriptId(ObjScriptEvent.Dialog, 450);
                    }

                    if (attachee.GetScriptId(ObjScriptEvent.Dialog) == 450 && !attachee.IsUnconscious() && !attachee.D20Query(D20DispatcherKey.QUE_Prone))
                    {
                        attachee.FloatLine(16000 + RandomRange(0, 6), triggerer);
                    }

                }

            }
            else
            {
                StartTimer(4500 + RandomRange(0, 1000), () => san_start_combat(attachee, triggerer, 1), true);
            }

            return RunDefault;
        }
        public static void call_leader(GameObjectBody npc, GameObjectBody pc)
        {
            var leader = PartyLeader;
            leader.Move(pc.GetLocation() - 2);
            leader.BeginDialog(npc, 1);
            return;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var loc = new locXY(526, 569);
            attachee.RunOff(loc);
            return RunDefault;
        }
        public static bool run_off_to_back(GameObjectBody attachee, GameObjectBody triggerer, FIXME generated_from_timevent = 0)
        {
            if (generated_from_timevent == 0)
            {
                attachee.SetNpcFlag(NpcFlag.WAYPOINTS_DAY);
                attachee.SetNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
                attachee.RunOff(attachee.GetLocation() - 10);
                GetGlobalVar(403) |= Math.Pow(2, 1);
                var obj = GameSystems.MapObject.CreateObject(14074, new locXY(485, 538));
                obj.SetScriptId(ObjScriptEvent.Dialog, 450);
                obj.SetScriptId(ObjScriptEvent.Dying, 450);
                obj.SetScriptId(ObjScriptEvent.EnterCombat, 450);
                obj.SetScriptId(ObjScriptEvent.ExitCombat, 450);
                obj.SetScriptId(ObjScriptEvent.StartCombat, 450);
                obj.SetScriptId(ObjScriptEvent.EndCombat, 450);
                obj.SetScriptId(ObjScriptEvent.SpellCast, 450);
                var pad3 = obj.GetInt(obj_f.npc_pad_i_3);
                pad3 |= Math.Pow(2, 6);
                obj.SetInt(obj_f.npc_pad_i_3, pad3);
                obj.Move(new locXY(485, 538), 0, 0);
                obj.TurnTowards(SelectedPartyLeader); // just to correct the animation glitch
                obj.Rotation = 3.3f;
                StartTimer(1000, () => run_off_to_back(attachee, triggerer, 1), true);
            }
            else
            {
                attachee.Destroy();
            }

            return RunDefault;
        }
        public static bool move_pc(GameObjectBody attachee, GameObjectBody triggerer)
        {
            FadeAndTeleport(0, 0, 0, 5005, 537, 545);
            // triggerer.move( location_from_axis( 537, 545 ) )
            return RunDefault;
        }
        public static bool deliver_pc(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.Move(new locXY(491, 541));
            return RunDefault;
        }

    }
}
