
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
    [ObjectScript(435)]
    public class ThePostNPC : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5001))
            {
                triggerer.BeginDialog(attachee, 1);
            }

            if ((attachee.GetMap() == 5051))
            {
                triggerer.BeginDialog(attachee, 200);
            }

            if ((attachee.GetMap() == 5121))
            {
                triggerer.BeginDialog(attachee, 400);
            }

            if (attachee.GetMap() == 5064)
            {
                triggerer.BeginDialog(attachee, 1000);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return SkipDefault;
        }
        // def zombie_tele(chaim,moshe,yosef):
        // game.global_vars[830] = chaim
        // game.global_vars[831] = moshe
        // game.global_vars[832] = yosef
        // game.fade_and_teleport(0,0,0,5019,454,467)
        // return SKIP_DEFAULT

        public static bool tele2(FIXME dialer, FIXME town, int x, int y)
        {
            FadeAndTeleport(300, 0, 0, town, x, y);
            StartTimer(100, () => bananaphone(dialer, x, y));
            return SkipDefault;
        }
        public static bool bananaphone(FIXME dialer, int x, int y)
        {
            var operator = GameSystems.MapObject.CreateObject(14800, new locXY(x + 1, y + 1));
            dialer.begin_dialog/*Unknown*/(operator, 3000);
            // game.particles( "sp-summon monster I", operator )
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5051))
            {
                if ((GetGlobalVar(927) == 4 && !GetGlobalFlag(929) && !Utilities.is_daytime()))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }

            return SkipDefault;
        }
        // def flagtest():
        // game.global_vars[500]=0; #first flag set above 231
        // game.global_vars[501]=0; #first flag set above 380
        // game.global_vars[502]=0; #first flag set above 1001
        // flagno=231;
        // while (game.global_flags[flagno] == 0):
        // flagno = flagno + 1;
        // game.global_vars[500]=flagno;
        // flagno=380;
        // while (game.global_flags[flagno] == 0):
        // flagno = flagno + 1;
        // game.global_vars[501]=flagno;
        // flagno=1001;
        // while (game.global_flags[flagno] == 0):
        // flagno = flagno + 1;
        // game.global_vars[502]=flagno;
        // #check the vars in console!
        // return SKIP_DEFAULT

        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var leader = PartyLeader;
            Co8.StopCombat(attachee, 0);
            leader.BeginDialog(attachee, 4000);
            return RunDefault;
        }
        public static int should_display_dialog_node(FIXME node_name)
        {
            var cur_map = SelectedPartyLeader.GetMap();
            if (node_name == "Temple Entrance")
            {
                if (cur_map == 5064)
                {
                    return 0;
                }

            }
            else if (node_name == "Temple Tower Exterior")
            {
                if (cur_map == 5113)
                {
                    return 0;
                }

                if (ScriptDaemon.get_f("visited_temple_tower_exterior"))
                {
                    return 1;
                }

            }
            else if (node_name == "More Upper Proper")
            {
                var option_count = 0;
                if (cur_map != 5064)
                {
                    option_count += 1;
                }

                if (cur_map != 5113 && ScriptDaemon.get_f("visited_temple_tower_exterior"))
                {
                    option_count += 1;
                }

                if (cur_map != 5065 && ScriptDaemon.get_f("visited_temple_tower_interior"))
                {
                    option_count += 1;
                }

                if (cur_map != 5093 && ScriptDaemon.get_f("visited_temple_burnt_farmhouse"))
                {
                    option_count += 1;
                }

                if (cur_map != 5092 && ScriptDaemon.get_f("visited_temple_escape_tunnel"))
                {
                    option_count += 1;
                }

                if (cur_map != 5112 && ScriptDaemon.get_f("visited_temple_ruined_building"))
                {
                    option_count += 1;
                }

                return option_count >= 5;
            }
            else if (node_name == "Level 1")
            {
                if (cur_map == 5066)
                {
                    return 0;
                }

                if (ScriptDaemon.get_f("visited_level_1_north_entrance") || ScriptDaemon.get_f("visited_level_1_south_entrance") || ScriptDaemon.get_f("visited_level_1_south_entrance") || SelectedPartyLeader.HasReputation(11) || ScriptDaemon.get_f("visited_secret_spiral_staircase"))
                {
                    return 1;
                }

            }
            else if (node_name == "Level 1 - North")
            {
                if (ScriptDaemon.get_f("visited_level_1_north_entrance"))
                {
                    return 1;
                }

            }
            else if (node_name == "Level 1 - South")
            {
                if (ScriptDaemon.get_f("visited_level_1_south_entrance"))
                {
                    return 1;
                }

            }
            else if (node_name == "Level 1 - Romag")
            {
                if (SelectedPartyLeader.HasReputation(11))
                {
                    return 1;
                }

            }
            else if (node_name == "Secret Spiral Staircase")
            {
                if (ScriptDaemon.get_f("visited_secret_spiral_staircase"))
                {
                    return 1;
                }

            }
            else if (node_name == "Level 2")
            {
                if (cur_map == 5067)
                {
                    return 0;
                }

                if (ScriptDaemon.get_f("visited_level_2_north_west_entrance") || ScriptDaemon.get_f("visited_level_2_centre_entrance") || SelectedPartyLeader.HasReputation(13) || SelectedPartyLeader.HasReputation(12) || SelectedPartyLeader.HasReputation(10))
                {
                    return 1;
                }

            }
            else if (node_name == "Level 2 - North West")
            {
                if (ScriptDaemon.get_f("visited_level_2_north_west_entrance"))
                {
                    return 1;
                }

            }
            else if (node_name == "Level 2 - Centre")
            {
                if (ScriptDaemon.get_f("visited_level_2_centre_entrance"))
                {
                    return 1;
                }

            }
            else if (node_name == "Level 2 - Alrrem")
            {
                if (SelectedPartyLeader.HasReputation(13))
                {
                    return 1;
                }

            }
            else if (node_name == "Level 2 - Belsornig")
            {
                if (SelectedPartyLeader.HasReputation(12))
                {
                    return 1;
                }

            }
            else if (node_name == "Level 2 - Kelno")
            {
                if (SelectedPartyLeader.HasReputation(10))
                {
                    return 1;
                }

            }
            else if (node_name == "Level 2 More")
            {
                var option_count = 0;
                if (ScriptDaemon.get_f("visited_level_2_north_west_entrance"))
                {
                    option_count += 1;
                }

                if (ScriptDaemon.get_f("visited_level_2_centre_entrance"))
                {
                    option_count += 1;
                }

                if (SelectedPartyLeader.HasReputation(13))
                {
                    option_count += 1;
                }

                if (SelectedPartyLeader.HasReputation(12))
                {
                    option_count += 1;
                }

                if (SelectedPartyLeader.HasReputation(10))
                {
                    option_count += 1;
                }

                return option_count >= 5;
            }
            else if (node_name == "Secret Spiral Staircase")
            {
                if (ScriptDaemon.get_f("visited_secret_spiral_staircase"))
                {
                    return 1;
                }

            }
            else if (node_name == "Level 3")
            {
                if (cur_map == 5105)
                {
                    return 0;
                }

                if (ScriptDaemon.get_f("visited_level_3_east_entrance") || ScriptDaemon.get_f("visited_level_3_west_entrance") || ScriptDaemon.get_f("visited_level_3_south_west_entrance") || ScriptDaemon.get_f("visited_level_3_falrinth"))
                {
                    return 1;
                }

            }
            else if (node_name == "Level 3 - East")
            {
                if (ScriptDaemon.get_f("visited_level_3_east_entrance"))
                {
                    return 1;
                }

            }
            else if (node_name == "Level 3 - West")
            {
                if (ScriptDaemon.get_f("visited_level_3_west_entrance"))
                {
                    return 1;
                }

            }
            else if (node_name == "Level 3 - South")
            {
                if (ScriptDaemon.get_f("visited_level_3_south_west_entrance"))
                {
                    return 1;
                }

            }
            else if (node_name == "Level 3 - Falrinth")
            {
                if (ScriptDaemon.get_f("visited_level_3_falrinth"))
                {
                    return 1;
                }

            }
            else if (node_name == "Level 4")
            {
                if (cur_map == 5080)
                {
                    return 0;
                }

                if (ScriptDaemon.get_f("visited_level_4_main_entrance") || ScriptDaemon.get_f("visited_level_4_nexus") || ScriptDaemon.get_f("visited_level_4_hedrack"))
                {
                    return 1;
                }

            }
            else if (node_name == "Level 4 - Main")
            {
                if (ScriptDaemon.get_f("visited_level_4_main_entrance"))
                {
                    return 1;
                }

            }
            else if (node_name == "Level 4 - Nexus")
            {
                if (ScriptDaemon.get_f("visited_level_4_nexus"))
                {
                    return 1;
                }

            }
            else if (node_name == "Level 4 - Hedrack")
            {
                if (ScriptDaemon.get_f("visited_level_4_hedrack"))
                {
                    return 1;
                }

            }
            else if (node_name == "Nodes")
            {
                if (ScriptDaemon.get_f("visited_air_node") || ScriptDaemon.get_f("visited_earth_node") || ScriptDaemon.get_f("visited_fire_node") || ScriptDaemon.get_f("visited_water_node"))
                {
                    return 1;
                }

            }
            else if (node_name == "Air Node")
            {
                if (ScriptDaemon.get_f("visited_air_node"))
                {
                    return 1;
                }

            }
            else if (node_name == "Earth Node")
            {
                if (ScriptDaemon.get_f("visited_earth_node"))
                {
                    return 1;
                }

            }
            else if (node_name == "Fire Node")
            {
                if (ScriptDaemon.get_f("visited_fire_node"))
                {
                    return 1;
                }

            }
            else if (node_name == "Water Node")
            {
                if (ScriptDaemon.get_f("visited_water_node"))
                {
                    return 1;
                }

            }
            else if (node_name == "Zuggtmoy Level")
            {
                if (cur_map == 5079)
                {
                    return 0;
                }

                if (ScriptDaemon.get_f("visited_zuggtmoy_level"))
                {
                    return 1;
                }

            }

            return 1;
        }

    }
}
