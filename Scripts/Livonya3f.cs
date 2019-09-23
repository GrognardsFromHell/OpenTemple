
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
    [ObjectScript(310)]
    public class Livonya3f : BaseObjectScript
    {
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(777) >= 1))
            {
                attachee.Destroy();
            }

            if ((GetGlobalVar(777) == 0))
            {
                DetachScript();
                SetGlobalVar(777, 1);
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (attachee.GetMap() == 5067 && (new[] { 14174, 14175, 14177, 13002 }).Contains(attachee.GetNameId())) // ToEE level 2 - big bugbear room scripting
            {
                var (xx, yy) = attachee.GetLocation();
                if (xx >= 416 && xx <= 434 && yy >= 350 && yy <= 398) // big bugbear room
                {
                    if (ScriptDaemon.get_v("bugbear_room_timevent_count") < 5)
                    {
                        StartTimer(750, () => bugbear_room_increment_turn_counter(ScriptDaemon.get_v("bugbear_room_turn_counter") + 1)); // reset flag in 750ms; in this version, time is frozen, so it will only take place next turn
                        ScriptDaemon.inc_v("bugbear_room_timevent_count");
                    }

                    var pcs_in_east_hallway = 0;
                    var pcs_in_south_hallway = 0;
                    var pcs_in_inner_south_hallway = 0;
                    var yyp_east_max = 355;
                    var xxp_inner_max = 416;
                    foreach (var obj in PartyLeader.GetPartyMembers())
                    {
                        var (xxp, yyp) = obj.GetLocation();
                        if (yyp >= 355 && yyp <= 413 && xxp >= 405 && xxp <= 415 && !obj.IsUnconscious())
                        {
                            pcs_in_east_hallway += 1;
                            if (yyp > yyp_east_max)
                            {
                                yyp_east_max = yyp;
                            }

                        }

                        if (yyp >= 414 && yyp <= 422 && xxp >= 405 && xxp <= 455)
                        {
                            pcs_in_south_hallway += 1;
                        }

                        if (yyp >= 391 && yyp <= 413 && xxp >= 416 && xxp <= 455)
                        {
                            pcs_in_inner_south_hallway += 1;
                            if (xxp > xxp_inner_max)
                            {
                                xxp_inner_max = xxp;
                            }

                        }

                    }

                    var bugbears_near_door = new List<GameObjectBody>();
                    var bugbears_near_south_entrance = new List<GameObjectBody>();
                    foreach (var bugbear_dude in ObjList.ListVicinity(new locXY(416, 359), ObjectListFilter.OLC_NPC))
                    {
                        if ((new[] { 14174, 14175, 14177 }).Contains(bugbear_dude.GetNameId()) && Utilities.willing_and_capable(bugbear_dude))
                        {
                            var (xxb, yyb) = bugbear_dude.GetLocation();
                            if (xxb >= 416 && xxb <= 434 && yyb >= 350 && yyb < 372)
                            {
                                bugbears_near_door.Add(bugbear_dude);
                            }

                        }

                    }

                    // TODO - fear
                    foreach (var bugbear_dude in ObjList.ListVicinity(new locXY(425, 383), ObjectListFilter.OLC_NPC))
                    {
                        if ((new[] { 14174, 14175, 14177 }).Contains(bugbear_dude.GetNameId()) && Utilities.willing_and_capable(bugbear_dude))
                        {
                            var (xxb, yyb) = bugbear_dude.GetLocation();
                            if (xxb >= 416 && xxb <= 434 && yyb >= 372 && yyb <= 399)
                            {
                                bugbears_near_south_entrance.Add(bugbear_dude);
                            }

                        }

                    }

                    if (pcs_in_inner_south_hallway == 0 && pcs_in_south_hallway == 0 && pcs_in_east_hallway > 0)
                    {
                        // PCs in east hallway only - take 3 turns to get there
                        if (ScriptDaemon.get_v("bugbear_room_turn_counter") >= 3)
                        {
                            if (yyp_east_max <= 395)
                            {
                                var yyb_base = yyp_east_max + 20;
                                var xxb_base = 406;
                            }
                            else
                            {
                                var xxb_base = 416;
                                var yyb_base = 415;
                            }

                            var bb_index = 0;
                            var bb_x_offset_array = new[] { 0, 0, 1, 1, 2, 2, -1, -1 };
                            var bb_y_offset_array = new[] { 0, 1, 0, 1, 0, 1, 0, 1 };
                            foreach (var bugbear_dude in bugbears_near_south_entrance)
                            {
                                if (bugbear_dude != attachee && bb_index <= 7)
                                {
                                    bugbear_dude.Move(new locXY(xxb_base + bb_x_offset_array[bb_index], yyb_base + bb_y_offset_array[bb_index]), 0f, 0f);
                                    bugbear_dude.Attack(SelectedPartyLeader);
                                    bb_index += 1;
                                }

                            }

                        }

                    }
                    else if (pcs_in_inner_south_hallway > 0 && pcs_in_south_hallway == 0 && pcs_in_east_hallway == 0)
                    {
                        // PCs in inner south hallway only - take 3 turns to reach
                        if (ScriptDaemon.get_v("bugbear_room_turn_counter") >= 3)
                        {
                            if (xxp_inner_max <= 440)
                            {
                                var xxb_base = xxp_inner_max + 15;
                                var yyb_base = 406;
                                var bb_x_offset_array = new[] { 0, 0, 1, 1, 2, 2, 0, 0 };
                                var bb_y_offset_array = new[] { 0, -1, 0, -1, 0, -1, 1, 2 };
                            }
                            else
                            {
                                var xxb_base = 450;
                                var yyb_base = 415;
                                var bb_x_offset_array = new[] { 0, 0, 1, 1, 2, 2, -1, -1 };
                                var bb_y_offset_array = new[] { 0, 1, 0, 1, 0, 1, 0, 1 };
                            }

                            var bb_index = 0;
                            foreach (var bugbear_dude in bugbears_near_door)
                            {
                                if (bugbear_dude != attachee && bb_index <= 7)
                                {
                                    bugbear_dude.Move(new locXY(xxb_base + bb_x_offset_array[bb_index], yyb_base + bb_y_offset_array[bb_index]), 0f, 0f);
                                    bugbear_dude.Attack(SelectedPartyLeader);
                                    bb_index += 1;
                                }

                            }

                        }

                    }

                }

            }

            // THIS IS USED FOR BREAK FREE
            // found_nearby = 0
            // for obj in game.party[0].group_list():
            // if (obj.distance_to(attachee) <= 3 and obj.stat_level_get(stat_hp_current) >= -9):
            // found_nearby = 1
            // if found_nearby == 0:
            // while(attachee.item_find(8903) != OBJ_HANDLE_NULL):
            // attachee.item_find(8903).destroy()
            // if (attachee.d20_query(Q_Is_BreakFree_Possible)): # workaround no longer necessary!
            // create_item_in_inventory( 8903, attachee )
            // attachee.d20_send_signal(S_BreakFree)
            // Spiritual Weapon Shenanigens	#
            CombatStandardRoutines.Spiritual_Weapon_Begone(attachee);
            return RunDefault;
        }
        public static void bugbear_room_increment_turn_counter(int turn_num)
        {
            ScriptDaemon.set_v("bugbear_room_turn_counter", turn_num);
            ScriptDaemon.set_v("bugbear_room_timevent_count", 0);
        }

    }
}
