
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

    public class RandomEncounters
    {
        FIXME
private class RE_entry
        {
            var enemies = ();
            var dc = 0;
            public FIXME __init__(Unknown self, Unknown dc_in, Unknown enemy_in, Unknown id_in)
            {
                self.dc_base/*Unknown*/ = dc_in;
                self.enemy_pool/*Unknown*/ = enemy_in;
                self.id/*Unknown*/ = id_in;
            }

            public FIXME get_enemies(Unknown self)
            {
                var enemy_list_output = ();
                var dc_mod = 0f;
                foreach (var tup_x in self.enemy_pool/*Unknown*/)
                {
                    var nn = RandomRange(tup_x[1], tup_x[2]);
                    if (nn > 0)
                    {
                        enemy_list_output += ((tup_x[0], nn));
                        if (tup_x.Count == 4) // DC modifier
                        {
                            dc_mod += tup_x[3] * nn;
                        }

                    }

                }

                self.enemies/*Unknown*/ = enemy_list_output;
                self.dc/*Unknown*/ = self.dc_base/*Unknown*/ + (int)(dc_mod);
                return enemy_list_output;
            }

        }
        public static FIXME encounter_exists(RandomEncounterQuery setup, RandomEncounter encounter)
        {
            Logger.Info("Testing encounter_exists");
            if (((setup.Type == RandomEncounterType.Resting)))
            {
                var check = check_sleep_encounter(setup, encounter);
            }
            else
            {
                var check = check_random_encounter(setup, encounter);
            }

            // Added by Sitra Achara
            if ((new[] { 5066, 5067, 5005 }).Contains(PartyLeader.GetMap()))
            {
                // If you rest inside the Temple or Moathouse, execute the Reactive Behavior scripts
                san_new_map(PartyLeader, PartyLeader);
            }

            if (PartyLeader.GetMap() == 5095)
            {
                // # Resting in Hickory Branch - execute reactive behavior
                GenericSpawner.hickory_branch_check(); // run scripting for enabling lieutenants inside the cave
                var hb_blockage_obj = refHandle(Co8PersistentData.GetSpellActiveList/*Unknown*/("HB_BLOCKAGE_SERIAL")); // get a handle on the cave blockage and then enable it
                if ((hb_blockage_obj != null) && (hb_blockage_obj.ToString().find/*String*/("Blockage") >= 0))
                {
                    hb_blockage_obj.object_script_execute/*Unknown*/(hb_blockage_obj, 10); // execute its san_first_heartbeat script
                }

            }

            Logger.Info("{0}", "Result: " + check.ToString());
            // encounter.map = 5074 # TESTING!!! REMOVE!!!
            return check;
        }
        public static void encounter_create(RandomEncounter encounter)
        {
            Logger.Info("{0}", "Testing encounter_create with id=" + encounter.Id.ToString() + ", map = " + encounter.Map.ToString());
            // encounter_create adds all the objects to the scene
            // WIP temp location for now
            var target = SelectedPartyLeader;
            if ((encounter.Id >= 4000 || encounter.Id == 3000))
            {
                var location = SelectedPartyLeader.GetLocation();
                var range = 1;
            }
            else
            {
                var location = Spawn_Point(encounter);
                var range = 6; // this is a "sub-range" actually, relative to the above spawn point
                var numP = GameSystems.Party.PartyMembers.Count - 1;
                var xxx = RandomRange(0, numP);
                target = GameSystems.Party.GetPartyGroupMemberN(xxx);
                if ((target == null))
                {
                    target = PartyLeader;
                }

            }

            if ((encounter.Map == 5078))
            {
                Slaughtered_Caravan();
            }

            if ((encounter.Map == 5072 && GetGlobalFlag(855) && !GetGlobalFlag(875)))
            {
                SetGlobalFlag(875, true);
                var flower = GameSystems.MapObject.CreateObject(12900, new locXY(499, 459));
            }

            var is_camp = RandomRange(1, 1); // Added by Cerulean the Blue
            var enemies = new List<GameObjectBody>();
            var i = 0;
            var total = encounter.Enemies.Count;
            // target = OBJ_HANDLE_NULL ## TESTING!!! REMOVE!!!
            // location = location_from_axis( 505, 479) # TESTING!!! REMOVE!!!
            Logger.Info("{0}", "Spawning encounter enemies, total: " + total.ToString());
            while ((i < total))
            {
                var j = 0;
                while ((j < encounter.Enemies[i].Count))
                {
                    // Random Encounter Encampment - idea and map by Shiningted, execution  by Cerulean the Blue
                    if (((encounter.Map == 5074) && !(Utilities.is_daytime()) && (is_camp == 1)))
                    {
                        target = null;
                        encounter.Location = new locXY(470, 480);
                        var location = new locXY(505, 479);
                        var range = 6;
                    }

                    var spawn_loc = random_location(location, range, target);
                    if ((encounter.Id >= 4000))
                    {
                        var numP = GameSystems.Party.PartyMembers.Count - 1;
                        var xxx = RandomRange(0, numP);
                        target = GameSystems.Party.GetPartyGroupMemberN(xxx);
                        var location = target.GetLocation();
                        if ((target == null))
                        {
                            target = PartyLeader;
                            location = PartyLeader.GetLocation();
                        }

                        if (SelectedPartyLeader.GetMap() == 5066)
                        {
                            // scripting wonnilon's hideout
                            var legit_list = new List<GameObjectBody>();
                            var barney = 0;
                            foreach (var moe in GameSystems.Party.PartyMembers)
                            {
                                var (xx, yy) = moe.GetLocation();
                                if (xx > 423 || xx < 410 || yy > 390 || yy < 369)
                                {
                                    barney = 1;
                                    legit_list.Add(moe.GetLocation());
                                }

                            }

                            if (barney == 0)
                            {
                                location = PartyLeader.GetLocation();
                            }
                            else
                            {
                                xxx = RandomRange(0, legit_list.Count - 1);
                                foreach (var moe in GameSystems.Party.PartyMembers)
                                {
                                    if (moe.GetLocation() == legit_list[xxx])
                                    {
                                        target = moe;
                                    }

                                }

                                location = legit_list[xxx];
                            }

                        }

                        spawn_loc = location;
                    }

                    var npc = GameSystems.MapObject.CreateObject(encounter.Enemies[i].ProtoId, spawn_loc);
                    if ((npc != null))
                    {
                        if ((target != null))
                        {
                            npc.TurnTowards(target);
                        }

                        if ((SelectedPartyLeader.GetArea() == 1 && SelectedPartyLeader.HasReputation(1) == 92) || ((encounter.Id < 2000) || (encounter.Id >= 4000)))
                        {
                            if (target != null)
                            {
                                npc.Attack(target);
                            }

                            npc.SetNpcFlag(NpcFlag.KOS);
                            enemies.Add(npc);
                        }

                    }

                    // m_count = m_count + 1
                    j = j + 1;
                }

                i = i + 1;
            }

            return;
        }
        public static int check_random_encounter(RandomEncounterQuery setup, RandomEncounter encounter)
        {
            if ((RandomRange(1, 20) == 1))
            {
                // encounter.location -- the location to teleport the player to
                var r = RandomRange(1, 3);
                if ((r == 1))
                {
                    encounter.Location = new locXY(470, 480);
                }
                else if ((r == 2))
                {
                    encounter.Location = new locXY(503, 478);
                }
                else
                {
                    encounter.Location = new locXY(485, 485);
                }

                if ((check_predetermined_encounter(setup, encounter)))
                {
                    ScriptDaemon.set_f("qs_is_repeatable_encounter", 0);
                    return 1;
                }
                else if ((check_unrepeatable_encounter(setup, encounter)))
                {
                    Survival_Check(encounter);
                    ScriptDaemon.set_f("qs_is_repeatable_encounter", 0);
                    return 1;
                }
                else
                {
                    if (GetGlobalFlag(403))
                    {
                        if (ScriptDaemon.get_f("qs_disable_random_encounters"))
                        {
                            return 0;
                        }

                    }

                    var check = check_repeatable_encounter(setup, encounter);
                    // encounter.location = location_from_axis( 503, 478 ) #for testing only
                    Survival_Check(encounter);
                    ScriptDaemon.set_f("qs_is_repeatable_encounter", 1);
                    return check;
                }

            }

            // print 'nope'
            return 0;
        }
        public static int check_sleep_encounter(RandomEncounterQuery setup, RandomEncounter encounter)
        {
            NPC_Self_Buff(); // THIS WAS ADDED TO AID IN NPC SELF BUFFING			##
                             // Revamped the chance system slightly.
                             // ran_factor determines chance of encounter. Translated to chance of encounter in an 8 hour rest period:
                             // 10 - 56 percent
                             // 11 - 60
                             // 12 - 64
                             // 13 - 67
                             // 14 - 70
                             // 15 - 72
                             // 16 - 75
                             // 17 - 77
                             // 18 - 80
                             // 19 - 81
                             // 20 - 83
                             // 21 - 85
                             // 22 - 86
                             // 23 - 87
                             // 24 - 89
                             // 25 - 90
                             // 26 - 91
                             // 27 - 91.9
                             // 28 - 93
                             // 29 - 93.5
                             // 30 - 94.2
                             // 31 - 94.8
                             // 32 - 95.4
                             // 33 - 95.9
                             // 34 - 96.4
                             // 35 - 96.8
                             // 36 - 97.1
            if (SelectedPartyLeader.GetMap() == 5015 || SelectedPartyLeader.GetMap() == 5016 || SelectedPartyLeader.GetMap() == 5017)
            {
                // resting in Burne's tower
                var ran_factor = 31;
            }
            else if (SelectedPartyLeader.GetMap() == 5001 || SelectedPartyLeader.GetMap() == 5007)
            {
                // resting in Hommlet Exterior
                var ran_factor = 18;
            }
            // elif game.leader.map == 5067:
            // resting in Temple level 2
            // if game.global_flags[144] == 1:
            // temple on alert
            // if game.global_flags[105] == 0 or game.global_flags[106] == 0 or game.global_flags[107] == 0 or game.global_flags[139] == 0:
            // any elemental high priest is alive
            // ran_factor = 16
            // else:
            // ran_factor = 10
            // elif game.global_flags[144] == 0:
            // temple not on alert
            // ran_factor = 10
            // elif game.leader.map == 5079 or game.leader.map == 5080 or game.leader.map == 5105:
            // resting in Temple level 3 or 4
            // if game.global_flags[144] == 1:
            // temple on alert
            // if game.global_flags[146] == 0 or game.global_flags[147] == 0:
            // hedrack or senshock is alive
            // ran_factor = 21
            // else:
            // ran_factor = 10
            // elif game.global_flags[144] == 0:
            // temple not on alert
            // ran_factor = 10
            else if (SelectedPartyLeader.GetMap() == 5143 || SelectedPartyLeader.GetMap() == 5144 || SelectedPartyLeader.GetMap() == 5145 || SelectedPartyLeader.GetMap() == 5146 || SelectedPartyLeader.GetMap() == 5147)
            {
                // resting in Verbobonc castle
                var ran_factor = 36;
            }
            else if (SelectedPartyLeader.GetMap() == 5128 || SelectedPartyLeader.GetMap() == 5129 || SelectedPartyLeader.GetMap() == 5130 || SelectedPartyLeader.GetMap() == 5131)
            {
                // resting in Verbobonc Underdark interior
                var ran_factor = 30;
            }
            else if (SelectedPartyLeader.GetMap() == 5127)
            {
                // resting in Verbobonc Underdark interior entryway - not safe but won't get attacked
                var ran_factor = 0;
            }
            else if (SelectedPartyLeader.GetMap() == 5191)
            {
                // resting in Hickory Branch Crypt
                var ran_factor = 25;
            }
            else if (SelectedPartyLeader.GetMap() == 5093 || SelectedPartyLeader.GetMap() == 5192 || SelectedPartyLeader.GetMap() == 5193)
            {
                // resting in Welkwood Bog
                if ((!GetGlobalFlag(976))) // Mathel alive - not safe but won't get attacked
                {
                    var ran_factor = 0;
                }
                else if ((GetGlobalFlag(976))) // Mathel dead
                {
                    var ran_factor = 10;
                }

            }
            else
            {
                // default - 56 percent chance of encounter in 8 hour rest
                var ran_factor = 10;
            }

            if ((RandomRange(1, 100) <= ran_factor))
            {
                encounter.Id = 4000;
                if ((SelectedPartyLeader.GetArea() == 1)) // ----- Hommlet
                {
                    if ((SelectedPartyLeader.HasReputation(29) || SelectedPartyLeader.HasReputation(30)) && !(SelectedPartyLeader.HasReputation(92) || SelectedPartyLeader.HasReputation(32)))
                    {
                        // Slightly naughty
                        var enemy_list = ((14371, 1, 2, 1));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        return x;
                    }
                    else if ((SelectedPartyLeader.HasReputation(92) || SelectedPartyLeader.HasReputation(32)) && ScriptDaemon.get_v(439) < 9)
                    {
                        // Moderately naughty
                        var enemy_list = ((14371, 2, 4, 1));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        return x;
                    }
                    else if ((SelectedPartyLeader.HasReputation(92) || SelectedPartyLeader.HasReputation(32)) && ScriptDaemon.get_v(439) >= 9 && ScriptDaemon.get_v(439) <= 19)
                    {
                        // Very Naughty
                        var num = RandomRange(4, 7);
                        var temp = new[] { (14371, num) };
                        if (!GetGlobalFlag(336))
                        {
                            temp.append/*Unknown*/((14004, 1));
                            // temp = (temp[0], (14004,1) )
                            ScriptDaemon.set_v(439, ScriptDaemon.get_v(439) | 256);
                        }

                        if (!GetGlobalFlag(437))
                        {
                            // p = len(temp)
                            // if p == 2:
                            // temp = ( temp[0], temp[1], (14006,1), )
                            // elif p == 1:
                            // temp = ( temp[0], (14006,1), )
                            temp.append/*Unknown*/((14006, 1));
                            ScriptDaemon.set_v(439, ScriptDaemon.get_v(439) | 512);
                        }

                        if ((ScriptDaemon.get_v(439) & 1024) == 0)
                        {
                            // p = len(temp)
                            // if p == 3:
                            // temp = ( temp[0], temp[1], temp[2], (14012,1), )
                            // elif p == 2:
                            // temp = ( temp[0], temp[1], (14012,1), )
                            // elif p == 1:
                            // temp = ( temp[0], (14012,1), )
                            temp.append/*Unknown*/((14012, 1));
                            ScriptDaemon.set_v(439, ScriptDaemon.get_v(439) | 1024);
                        }

                        ScriptDaemon.record_time_stamp(443);
                        encounter.Enemies = temp;
                        encounter.Title = 1;
                        return 1;
                    }
                    else if ((SelectedPartyLeader.HasReputation(92) || SelectedPartyLeader.HasReputation(32)) && ScriptDaemon.get_v(439) >= 20 && CurrentTimeSeconds < ScriptDaemon.get_v(443) + 2 * 30 * 24 * 60 * 60)
                    {
                        // NAUGHTINESS OVERWHELMING
                        // You've exterminated all the badgers!
                        // And not enough time has passed since you started massacring the badgers for a big revenge encounter
                        return 0;
                    }
                    else
                    {
                        return 0;
                    }

                }
                else if ((SelectedPartyLeader.GetArea() == 2)) // ----- Moat house
                {
                    if ((SelectedPartyLeader.GetMap() == 5002)) // moathouse ruins
                    {
                        // frog, tick, willowisp, wolf, crayfish, lizard, rat, snake, spider, brigand
                        var enemy_list = ((14057, 1, 3, 1), (14089, 2, 4, 1), (14291, 1, 4, 6), (14050, 2, 3, 1), (14094, 1, 2, 3), (14090, 2, 4, 1), (14056, 4, 9, 1), (14630, 1, 3, 1), (14047, 2, 4, 1), (14070, 2, 5, 1));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        return x;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5004)) // moathouse interior
                    {
                        // rat, tick, lizard, snake, brigand
                        var enemy_list = ((14056, 4, 9, 1), (14089, 2, 4, 1), (14090, 2, 4, 1), (14630, 1, 3, 1), (14070, 2, 5, 1));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        return x;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5005)) // moathouse dungeon
                    {
                        // rat, lizard, zombie, bugbear, gnoll (unless gone), Lareth guard (unless Lareth killed or in group)
                        var enemy_list = ((14056, 4, 9, 1), (14090, 2, 4, 1), (14092, 1, 3, 1), (14093, 1, 3, 2), (14067, 1, 3, 1), (14074, 2, 4, 1));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        if ((x == 1))
                        {
                            if ((encounter.Enemies[0].ProtoId == 14067)) // check gnolls
                            {
                                if ((GetGlobalFlag(288)))
                                {
                                    return 0;
                                }

                            }
                            else if ((encounter.Enemies[0].ProtoId == 14074)) // check Lareth
                            {
                                if (((GetGlobalFlag(37)) || (GetGlobalFlag(50))))
                                {
                                    return 0;
                                }

                            }

                        }

                        return x;
                    }

                    return 0;
                }
                else if ((SelectedPartyLeader.GetArea() == 3)) // ----- Nulb
                {
                    return 0; // WIP, thieves?
                }
                else if ((SelectedPartyLeader.GetArea() == 4 || SelectedPartyLeader.GetMap() == 5105)) // ----- Temple
                {
                    if ((SelectedPartyLeader.GetMap() == 5062)) // temple exterior
                    {
                        // bandit, drelb (night only), rat, snake, spider
                        var enemy_list = ((14070, 2, 5, 1), (14275, 1, 1, 4), (14056, 4, 9, 1), (14630, 1, 3, 1), (14047, 2, 4, 1));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        if ((x == 1))
                        {
                            if ((encounter.Enemies[0].ProtoId == 14275)) // check drelb
                            {
                                if ((Utilities.is_daytime()))
                                {
                                    return 0;
                                }

                            }

                        }

                        return x;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5064)) // temple interior
                    {
                        // bandit, drelb (night only), rat, GT patrol
                        var enemy_list = ((14070, 2, 5, 1), (14275, 1, 1, 4), (14056, 4, 9, 1), (14170, 2, 5, 2));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        if ((x == 1))
                        {
                            if ((encounter.Enemies[0].ProtoId == 14275)) // check drelb
                            {
                                if ((Utilities.is_daytime()))
                                {
                                    return 0;
                                }

                            }

                        }

                        return x;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5065)) // temple tower
                    {
                        // bandit, GT patrol
                        var enemy_list = ((14070, 2, 5, 1), (14170, 2, 5, 2));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        return x;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5066)) // temple dungeon 1
                    {
                        var enc_abort = 1;
                        foreach (var joe in GameSystems.Party.PartyMembers)
                        {
                            var (xx, yy) = joe.GetLocation();
                            if (xx > 423 || xx < 410 || yy > 390 || yy < 369)
                            {
                                enc_abort = 0;
                            }

                        }

                        if (enc_abort == 1)
                        {
                            return 0;
                        }

                        // bandit, gnoll, ghoul, gelatinous cube, gray ooze, ogre, GT patrol
                        var enemy_list = ((14070, 2, 5, 1), (14078, 2, 5, 1), (14128, 2, 5, 1), (14139, 1, 1, 3), (14140, 1, 1, 4), (14448, 1, 1, 2), (14170, 2, 5, 2));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        return x;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5067)) // temple dungeon 2
                    {
                        // bandit, bugbear, carrion crawler, ochre jelly, ogre, troll
                        var enemy_list = ((14070, 2, 5, 1), (14170, 4, 6, 2), (14190, 1, 1, 4), (14142, 1, 1, 5), (14448, 2, 4, 2), (14262, 1, 2, 5));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        return x;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5105)) // temple dungeon 3 lower
                    {
                        // black pudding, ettin, gargoyle, hill giant, ogre, troll
                        var enemy_list = ((14143, 1, 1, 7), (14697, 1, 2, 5), (14239, 5, 8, 4), (14221, 2, 3, 7), (14448, 5, 8, 2), (14262, 2, 3, 5));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        return x;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5079)) // temple dungeon 3 upper
                    {
                        return 0;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5080)) // temple dungeon 4
                    {
                        // black pudding, ettin, troll, gargoyle, hill giant, ogre + bugbear
                        var enemy_list = ((14143, 1, 1, 7), (14697, 1, 1, 5), (14262, 1, 2, 5), (14239, 3, 6, 4), (14220, 1, 2, 7), (14448, 1, 4, 3));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        if ((x == 1))
                        {
                            if ((encounter.Enemies[0].ProtoId == 14448)) // reinforce ogres with bugbears
                            {
                                encounter.Enemies.append/*RandomEncounterEnemies*/((14174, RandomRange(2, 5)));
                            }

                        }

                        return x;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5081)) // air node
                    {
                        // air elemental, ildriss grue, vapor rat, vortex, windwalker
                        var enemy_list = ((14292, 1, 2, 5), (14192, 1, 2, 4), (14068, 1, 4, 2), (14293, 1, 2, 2), (14294, 1, 1, 4));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        return x;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5082)) // earth node
                    {
                        // basilisk, chaggrin grue, crystal ooze, earth elemental
                        var enemy_list = ((14295, 1, 1, 5), (14191, 1, 4, 4), (14141, 1, 1, 4), (14296, 1, 2, 5));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        return x;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5083)) // fire node
                    {
                        // fire bats, fire elemental, fire snake, fire toad
                        var enemy_list = ((14297, 2, 5, 2), (14298, 1, 2, 5), (14299, 1, 2, 1), (14300, 1, 2, 3));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        return x;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5084)) // water node
                    {
                        // floating eye, ice lizard, lizard man, vodyanoi, water elemental, kopoacinth, lacedon, merrow
                        var enemy_list = ((14301, 1, 1, 1), (14109, 1, 1, 3), (14084, 2, 4, 1), (14261, 1, 1, 7), (14302, 1, 2, 5), (14240, 2, 3, 4), (14132, 3, 5, 1), (14108, 3, 5, 2));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        return x;
                    }

                    return 0;
                }
                else if ((SelectedPartyLeader.GetMap() == 5143 || SelectedPartyLeader.GetMap() == 5144 || SelectedPartyLeader.GetMap() == 5145 || SelectedPartyLeader.GetMap() == 5146 || SelectedPartyLeader.GetMap() == 5147)) // ----- Verbobonc castle
                {
                    // ghost
                    var enemy_list = ((14819, 1, 1, 1));
                    var x = get_sleep_encounter_enemies(enemy_list, encounter);
                    return x;
                }
                else if ((SelectedPartyLeader.GetMap() == 5128 || SelectedPartyLeader.GetMap() == 5129 || SelectedPartyLeader.GetMap() == 5130 || SelectedPartyLeader.GetMap() == 5131)) // ----- Verbobonc Underdark inside
                {
                    if ((GetQuestState(69) != QuestState.Completed) && (GetQuestState(74) != QuestState.Completed))
                    {
                        // large spider, fiendish small monstrous spider, fiendish medium monstrous spider, fiendish large monstrous spider
                        var enemy_list = ((14047, 5, 10, 1), (14672, 4, 8, 1), (14620, 3, 6, 1), (14671, 2, 4, 1));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        return x;
                    }
                    else if ((GetQuestState(69) == QuestState.Completed) && (GetQuestState(74) == QuestState.Completed))
                    {
                        // dire rat
                        var enemy_list = ((14056, 6, 12, 1));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        return x;
                    }

                }
                else if ((SelectedPartyLeader.GetMap() == 5132)) // ----- Verbobonc Underdark outside
                {
                    // wolf
                    var enemy_list = ((14050, 4, 8, 1));
                    var x = get_sleep_encounter_enemies(enemy_list, encounter);
                    return x;
                }
                else if ((SelectedPartyLeader.GetMap() == 5093)) // ----- Welkwood Bog outside
                {
                    // wolf, jackal, giant frog, giant lizard, carrion crawler, wild boar
                    var enemy_list = ((14050, 2, 6, 1), (14051, 2, 6, 1), (14057, 1, 3, 1), (14090, 1, 3, 1), (14190, 1, 1, 1), (14522, 2, 4, 1));
                    var x = get_sleep_encounter_enemies(enemy_list, encounter);
                    return x;
                }
                else if ((SelectedPartyLeader.GetMap() == 5192 || SelectedPartyLeader.GetMap() == 5193)) // ----- Welkwood Bog inside
                {
                    // dire rat
                    var enemy_list = ((14056, 6, 12, 1));
                    var x = get_sleep_encounter_enemies(enemy_list, encounter);
                    return x;
                }
                else if ((SelectedPartyLeader.GetMap() == 5095)) // ----- Hickory Branch
                {
                    if ((GetQuestState(62) != QuestState.Completed))
                    {
                        // hill giant, gnoll, orc fighter, orc bowman, bugbear, ogre
                        var enemy_list = ((14988, 1, 2, 1), (14475, 3, 6, 1), (14745, 3, 6, 1), (14467, 2, 4, 1), (14476, 2, 4, 1), (14990, 1, 2, 1));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        return x;
                    }
                    else if ((GetQuestState(62) == QuestState.Completed))
                    {
                        // black bear, brown bear, worg, dire wolf, dire bear, dire boar, wild boar
                        var enemy_list = ((14052, 1, 2, 1), (14053, 1, 1, 1), (14352, 1, 2, 1), (14391, 1, 2, 1), (14506, 1, 1, 1), (14507, 1, 1, 1), (14522, 2, 4, 1));
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        return x;
                    }

                }
                else if ((SelectedPartyLeader.GetMap() == 5191)) // ----- Hickory Branch Crypt
                {
                    // dire rat
                    var enemy_list = ((14056, 6, 12, 1));
                    var x = get_sleep_encounter_enemies(enemy_list, encounter);
                    return x;
                }
                else if ((SelectedPartyLeader.GetMap() == 5141)) // ----- Verbobonc Drainage Tunnels
                {
                    // dire rat
                    var enemy_list = ((14433, 9, 15, 1));
                    var x = get_sleep_encounter_enemies(enemy_list, encounter);
                    return x;
                }
                else if ((SelectedPartyLeader.GetMap() == 5120)) // ----- Gnarley Forest
                {
                    // stirge, will-o'-wisp, basilisk, dire lizard
                    var enemy_list = ((14182, 5, 10, 1), (14291, 4, 8, 1), (14295, 1, 3, 1), (14450, 1, 3, 1));
                    var x = get_sleep_encounter_enemies(enemy_list, encounter);
                    return x;
                }
                else
                {
                    var party_level = Utilities.group_average_level(SelectedPartyLeader);
                    get_repeatable_encounter_enemies(setup, encounter);
                    while ((encounter.Title > (party_level + 2)))
                    {
                        // while (encounter.dc > (party_level+2) or ( game.random_range(1, party_level) >  encounter.dc  ) ): # makes it more likely for high level parties to skip mundane encounters. Needs some adjustment so level 12 parties don't encounter 50 trolls all the time TODO
                        get_repeatable_encounter_enemies(setup, encounter);
                    }

                    encounter.Id = 4000;
                    return 1;
                }

            }

            return 0;
        }
        public static int get_sleep_encounter_enemies(FIXME enemy_list, RandomEncounter encounter)
        {
            var total = enemy_list.Count;
            var n = RandomRange(0, total - 1);
            encounter.Title = enemy_list[n][3];
            var party_level = Utilities.group_average_level(SelectedPartyLeader);
            if ((encounter.Title > (party_level + 2)))
            {
                // try again
                n = RandomRange(0, total - 1);
                encounter.Title = enemy_list[n][3];
                if ((encounter.Title > (party_level + 2)))
                {
                    return 0;
                }

            }

            var num = RandomRange(enemy_list[n][1], enemy_list[n][2]);
            encounter.AddEnemies(enemy_list[n][0], num);
            return 1;
        }
        public static int check_predetermined_encounter(RandomEncounterQuery setup, RandomEncounter encounter)
        {
            while ((EncounterQueue.Count > 0))
            {
                var id = EncounterQueue[0];
            FIXME: DEL EncounterQueue[0];

                if ((!GetGlobalFlag(id - 3000 + 277)))
                {
                    SetGlobalFlag(id - 3000 + 277, true);
                    encounter.Id = id;
                    encounter.Title = 1000; // unavoidable
                    encounter.Map = get_map_from_terrain(setup.Terrain);
                    if ((id == 3000)) // Assassin
                    {
                        encounter.AddEnemies(14303, 1);
                        if ((encounter.Map == 5074)) // 5074 is the encampment map - unsuitable for these encounters
                        {
                            encounter.Map = 5070;
                        }

                    }
                    else if ((id == 3001)) // Thrommel Reward
                    {
                        encounter.AddEnemies(14307, 1);
                        encounter.AddEnemies(14308, 10);
                        if ((encounter.Map == 5074))
                        {
                            encounter.Map = 5070;
                        }

                    }
                    else if ((id == 3002)) // Tillahi Reward
                    {
                        encounter.AddEnemies(14305, 1);
                        encounter.AddEnemies(14306, 6);
                        if ((encounter.Map == 5074))
                        {
                            encounter.Map = 5070;
                        }

                    }
                    else if ((id == 3003)) // Sargen's Courier
                    {
                        encounter.AddEnemies(14304, 1);
                        if ((encounter.Map == 5074))
                        {
                            encounter.Map = 5070;
                        }

                    }
                    else if ((id == 3004)) // Skole Goons
                    {
                        if ((GetGlobalFlag(202)))
                        {
                            encounter.AddEnemies(14315, 4);
                        }
                        else
                        {
                            return 0;
                        }

                    }
                    else if ((id == 3159))
                    {
                        // Following the trader's tracks, you find them camping
                        encounter.AddEnemies(14014, 1);
                        encounter.AddEnemies(14018, 1);
                        ScriptDaemon.set_v(437, 101);
                        encounter.Map = 5074;
                    }
                    else if ((id == 3434)) // ranth's bandits
                    {
                        encounter.AddEnemies(14485, 1);
                        encounter.AddEnemies(14489, 1);
                        encounter.AddEnemies(14490, 1);
                        encounter.AddEnemies(14486, 4);
                        encounter.AddEnemies(14487, 4);
                        encounter.AddEnemies(14488, 4);
                        encounter.Map = 5071;
                    }
                    else if ((id == 3435)) // Scarlet Brotherhood
                    {
                        encounter.AddEnemies(14653, 3);
                        encounter.AddEnemies(14652, 1);
                        if ((encounter.Map == 5070))
                        {
                            encounter.Map = 5071;
                        }

                        if ((encounter.Map == 5074))
                        {
                            encounter.Map = 5075;
                        }

                        encounter.Location = new locXY(480, 480);
                        if ((GetGlobalVar(945) == 4))
                        {
                            SetGlobalVar(945, 7);
                        }

                        if ((GetGlobalVar(945) == 5))
                        {
                            SetGlobalVar(945, 8);
                        }

                        if ((GetGlobalVar(945) == 6))
                        {
                            SetGlobalVar(945, 9);
                        }

                    }
                    else if ((id == 3436)) // gremlich 1
                    {
                        encounter.AddEnemies(2146, 1);
                        QueueRandomEncounter(3437);
                        SetGlobalVar(927, 1);
                    }
                    else if ((id == 3437)) // gremlich 2
                    {
                        encounter.AddEnemies(2148, 1);
                        QueueRandomEncounter(3438);
                        SetGlobalVar(927, 2);
                    }
                    else if ((id == 3438)) // gremlich 3
                    {
                        encounter.AddEnemies(2147, 1);
                        QueueRandomEncounter(3439);
                        SetGlobalVar(927, 3);
                    }
                    else if ((id == 3439)) // gremlich 4
                    {
                        encounter.AddEnemies(2149, 1);
                        SetGlobalVar(927, 4);
                    }
                    else if ((id == 3440)) // gremlich for real
                    {
                        encounter.AddEnemies(14752, 1);
                        if ((encounter.Map == 5074))
                        {
                            encounter.Map = 5070;
                        }

                        Sound(4126, 1);
                    }
                    else if ((id == 3441)) // sport - pirates vs brigands
                    {
                        if ((encounter.Map == 5074))
                        {
                            encounter.Map = 5070;
                        }

                        encounter.AddEnemies(14290, 8);
                    }
                    else if ((id == 3442)) // sport - bugbears vs orcs melee
                    {
                        if ((encounter.Map == 5074))
                        {
                            encounter.Map = 5070;
                        }

                        encounter.AddEnemies(14173, 8);
                    }
                    else if ((id == 3443)) // sport - bugbears vs orcs ranged
                    {
                        if ((encounter.Map == 5074))
                        {
                            encounter.Map = 5070;
                        }

                        encounter.AddEnemies(14912, 8);
                    }
                    else if ((id == 3444)) // sport - hill giants vs ettins
                    {
                        if ((encounter.Map == 5074))
                        {
                            encounter.Map = 5070;
                        }

                        encounter.AddEnemies(14572, 2);
                    }
                    else if ((id == 3445)) // sport - female vs male bugbears
                    {
                        if ((encounter.Map == 5074))
                        {
                            encounter.Map = 5070;
                        }

                        encounter.AddEnemies(14686, 8);
                    }
                    else if ((id == 3446)) // sport - zombies vs lacedons
                    {
                        if ((encounter.Map == 5074))
                        {
                            encounter.Map = 5070;
                        }

                        encounter.AddEnemies(14123, 8);
                    }
                    else if ((id == 3447)) // bethany
                    {
                        encounter.AddEnemies(14773, 1);
                        encounter.Map = 5071;
                        encounter.Location = new locXY(484, 462);
                    }
                    else if ((id == 3579)) // gnolls
                    {
                        encounter.AddEnemies(14066, 1);
                        encounter.AddEnemies(14078, 3);
                        encounter.AddEnemies(14079, 3);
                        encounter.AddEnemies(14080, 3);
                        encounter.AddEnemies(14067, 3);
                    }
                    else if ((id == 3605)) // slaughtered caravan
                    {
                        encounter.AddEnemies(14459, 1);
                        encounter.Map = 5078;
                        encounter.Location = new locXY(465, 475);
                    }
                    else
                    {
                        return 0;
                    }

                    return 1;
                }

            }

            return 0;
        }
        public static int check_unrepeatable_encounter(RandomEncounterQuery setup, RandomEncounter encounter)
        {
            // make some fraction of all encounters an unrepeatable encounter (i.e. Special Encounters)
            // The chance starts at 10% and gets lower as you use them up because two consecutive rolls are made...
            if ((RandomRange(1, 10) == 1))
            {
                var id = RandomRange(2000, 2002);
                if ((!GetGlobalFlag(id - 2000 + 227)))
                {
                    encounter.Id = id;
                    encounter.Map = get_map_from_terrain(setup.Terrain);
                    if ((id == 2000)) // ochre jellies
                    {
                        encounter.Title = 9;
                        encounter.AddEnemies(14142, 4);
                    }
                    else if ((id == 2001)) // zaxis
                    {
                        encounter.Title = 5;
                        encounter.AddEnemies(14331, 1);
                    }
                    else if ((id == 2002)) // adventuring party
                    {
                        encounter.Title = 9;
                        encounter.AddEnemies(14332, 1);
                        encounter.AddEnemies(14333, 1);
                        encounter.AddEnemies(14334, 1);
                        encounter.AddEnemies(14335, 1);
                        encounter.AddEnemies(14336, 1);
                        encounter.AddEnemies(14622, 1);
                    }
                    else
                    {
                        return 0;
                    }

                    var party_level = Utilities.group_average_level(SelectedPartyLeader);
                    if ((encounter.Title > (party_level + 2)))
                    {
                        return 0;
                    }

                    SetGlobalFlag(id - 2000 + 227, true);
                    return 1;
                }

            }

            return 0;
        }
        public static int check_repeatable_encounter(RandomEncounterQuery setup, RandomEncounter encounter)
        {
            encounter.Map = get_map_from_terrain(setup.Terrain);
            // encounter.map = 5074 #  for testing only
            if (((encounter.Map == 5074) && !(Utilities.is_daytime())))
            {
                encounter.Location = new locXY(470, 480);
            }

            if ((encounter.Map == 5072 || encounter.Map == 5076))
            {
                encounter.Location = new locXY(485, 485);
            }

            var party_level = Utilities.group_average_level(SelectedPartyLeader);
            get_repeatable_encounter_enemies(setup, encounter);
            // while (encounter.dc > (party_level+2)):
            var countt = 0;
            while (countt < 5 && (encounter.Title > (party_level + 2) || (party_level > 10 && encounter.Title < 5 && GetGlobalFlag(500) && RandomRange(1, 100) <= 87)))
            {
                // will reroll the encounter for higher levels (but still about 13% chance for lower level encounter, i.e. about 1/7 )
                // will also reroll if you're low level so that poor player doesn't get his ass kicked too early
                // note that party_level is referenced to party size 4 - bigger parties will have a higher effective level thus skewing the calculation
                // e.g. a size 7 + animal companion party will count as two levels higher I think
                get_repeatable_encounter_enemies(setup, encounter);
                countt += 1;
            }

            if (countt == 5)
            {
                return 0;
            }

            return 1;
        }
        // NEW MAP GENERATOR BY CERULEAN THE BLUE

        public static int get_map_from_terrain(FIXME terrain)
        {
            var map = 5069;
            if ((map == 5069))
            {
                map = 5069 + RandomRange(1, 8);
            }

            return map;
        }
        // NEW RANDOM ENCOUNTER ENEMY GENERATOR BY CERULEAN THE BLUE

        public static void get_repeatable_encounter_enemies(RandomEncounterQuery setup, RandomEncounter encounter)
        {
            if (((encounter.Map == 5070) || (encounter.Map == 5074)))
            {
                if ((Utilities.is_daytime()))
                {
                    get_scrub_daytime(encounter);
                }
                else
                {
                    get_scrub_nighttime(encounter);
                }

            }
            else if (((encounter.Map == 5071) || (encounter.Map == 5075)))
            {
                if ((Utilities.is_daytime()))
                {
                    get_forest_daytime(encounter);
                }
                else
                {
                    get_forest_nighttime(encounter);
                }

            }
            else if (((encounter.Map == 5072) || (encounter.Map == 5076)))
            {
                if ((Utilities.is_daytime()))
                {
                    get_swamp_daytime(encounter);
                }
                else
                {
                    get_swamp_nighttime(encounter);
                }

            }
            else
            {
                // else:	# TERRAIN_RIVERSIDE
                if ((Utilities.is_daytime()))
                {
                    get_riverside_daytime(encounter);
                }
                else
                {
                    get_riverside_nighttime(encounter);
                }

            }

            return;
        }
        // END NEW RANDOM ENCOUNTER ENEMY GENERATOR

        public static void get_scrub_daytime(RandomEncounter encounter)
        {
            var m = Get_Multiplier(encounter);
            var m2 = 1;
            if (m > 1)
            {
                m2 = m - 1;
            }

            var re_list = new List<object>();
            re_list.append/*UnknownList*/(new RE_entry(2, ((14069, m, m), (14070, 2 * m, 5 * m, 0.5f)), 1000)); // bandits
            re_list.append/*UnknownList*/(new RE_entry(3, ((14067, 2 * m, 4 * m)), 1018)); // gnolls
            re_list.append/*UnknownList*/(new RE_entry(4, ((14067, 1 * m, 3 * m), (14050, 1 * m, 3 * m)), 1019)); // gnolls and wolves
            re_list.append/*UnknownList*/(new RE_entry(1, ((14184, 3 * m, 6 * m)), 1003)); // goblins
            re_list.append/*UnknownList*/(new RE_entry(3, ((14184, 3 * m, 6 * m), (14050, 1 * m, 3 * m)), 1004)); // goblins and wolves
            re_list.append/*UnknownList*/(new RE_entry(1, ((14640, 3 * m, 10 * m), (14641, 1 * m, 1 * m)), 1003)); // kobolds and kobold sergeant
            re_list.append/*UnknownList*/(new RE_entry(1, ((14051, 2 * m, 4 * m)), 1006)); // jackals
                                                                                           // Higher level encounters
            re_list.append/*UnknownList*/(new RE_entry(4, ((14697, 1 * m, 4 * m, 1)), 1041)); // ettins (potentially)
            re_list.append/*UnknownList*/(new RE_entry(7, ((14217, 1 * m, 4 * m, 1)), 1045)); // hill giants
            re_list.append/*UnknownList*/(new RE_entry(7, ((14697, m, m), (14053, 1 * m, 2 * m)), 1040)); // ettin vs. brown bears
            re_list.append/*UnknownList*/(new RE_entry(6, ((14697, m, m), (14188, 3 * m, 6 * m)), 1039)); // ettin vs. hobgoblins
            if (GetGlobalFlag(500))
            {
                re_list.append/*UnknownList*/(new RE_entry(11, ((14892, 1 * m, 2 * m, 0.5f), (14891, 2 * m, 4 * m, 0.5f), (14890, 2 * m2, 3 * m2, 0.5f), (14889, 1 * m2, 2 * m2, 0.5f)), 1022)); // Lizardman battlegroup
                re_list.append/*UnknownList*/(new RE_entry(11, ((14888, 3, 5, 0.5f), (14891, 0 * m, 1 * m, 0.5f), (14696, 0 * m2, 1 * m2, 0.5f), (14896, 0 * m2, 1 * m2, 0.5f), (14506, 0 * m2, 1 * m2, 0.5f), (14527, 0 * m2, 1 * m2, 0.5f), (14525, 0 * m2, 1 * m2, 0.5f)), 1024)); // Cult of the Siren + Random Thrall
                re_list.append/*UnknownList*/(new RE_entry(10, ((14898, 2 * m2, 4 * m2, 0.5f), (14897, 2 * m2, 4 * m2, 0.5f)), 1022)); // Leucrottas + Jackalweres
                re_list.append/*UnknownList*/(new RE_entry(11, ((14248, 1, 1, 1), (14249, 3 * m, 5 * m)), 1016)); // Ogre chief and ogres
                re_list.append/*UnknownList*/(new RE_entry(11, ((14248, 1, 1, 1), (14249, 3 * m, 5 * m), (14697, 3 * m, 5 * m)), 1023)); // Ogre chief and ogres vs. ettins (clash of the titans! :) )
                if (PartyAlignment.IsEvil())
                {
                    re_list.append/*UnknownList*/(new RE_entry(11, ((14896, 2, 4, 1), (14895, 3, 5), (14894, 2, 3, 0.5f)), 1022)); // Holy Rollers
                }

            }

            var aaa = RandomRange(0, re_list.Count - 1);
            encounter.Enemies = re_list[aaa].get_enemies/*Unknown*/();
            encounter.Title = re_list[aaa].dc/*Unknown*/;
            encounter.Id = re_list[aaa].id/*Unknown*/;
            return;
        }
        public static void get_scrub_nighttime(RandomEncounter encounter)
        {
            var m = Get_Multiplier(encounter);
            var m2 = 1;
            if (m > 1)
            {
                m2 = m - 1;
            }

            var re_list = new List<object>();
            re_list.append/*UnknownList*/(new RE_entry(3, ((14093, m, m), (14184, 3 * m, 6 * m)), 1026)); // bugbears and goblins
            re_list.append/*UnknownList*/(new RE_entry(4, ((14093, m, m), (14188, 4 * m, 9 * m)), 1027)); // bugbears and hobgoblins
            re_list.append/*UnknownList*/(new RE_entry(1, ((14093, 2 * m, 4 * m, 1)), 1028)); // bugbears
            re_list.append/*UnknownList*/(new RE_entry(3, ((14067, 2 * m, 4 * m)), 1018)); // gnolls
            re_list.append/*UnknownList*/(new RE_entry(4, ((14067, 1 * m, 3 * m), (14050, 1 * m, 3 * m)), 1019)); // gnolls and wolves
            re_list.append/*UnknownList*/(new RE_entry(1, ((14184, 3 * m, 6 * m)), 1003)); // goblins
            re_list.append/*UnknownList*/(new RE_entry(3, ((14184, 3 * m, 6 * m), (14050, 1 * m, 3 * m)), 1004)); // goblins and wolves
            re_list.append/*UnknownList*/(new RE_entry(1, ((14640, 3 * m, 10 * m), (14641, 1 * m, 1 * m)), 1003)); // kobolds and kobold sergeant
            re_list.append/*UnknownList*/(new RE_entry(1, ((14092, 1 * m, 6 * m, 0.3f)), 1014)); // zombies
                                                                                                 // Higher level encounters
            re_list.append/*UnknownList*/(new RE_entry(7, ((14093, 2 * m, 4 * m), (14050, 2 * m, 4 * m)), 1029)); // bugbears and wolves
            if (GetGlobalFlag(500))
            {
                re_list.append/*UnknownList*/(new RE_entry(11, ((14892, 1 * m2, 2 * m2, 0.5f), (14891, 2 * m, 4 * m, 0.5f), (14890, 2 * m2, 3 * m2, 0.5f), (14889, 1 * m2, 2 * m2, 0.5f)), 1022)); // Lizardman battlegroup
                re_list.append/*UnknownList*/(new RE_entry(10, ((14898, 2 * m2, 4 * m2, 0.5f), (14897, 2 * m2, 4 * m2, 0.5f)), 1022)); // Leucrottas + Jackalweres
                re_list.append/*UnknownList*/(new RE_entry(9, ((14542, 2 * m, 4 * m, 1)), 1017)); // Invisible Stalkers
                re_list.append/*UnknownList*/(new RE_entry(11, ((14248, 1, 1, 1), (14249, 3 * m, 5 * m)), 1016)); // Ogre chief and ogres
                re_list.append/*UnknownList*/(new RE_entry(11, ((14510, 1 * m, 3 * m, 1), (14299, 1 * m, 3 * m)), 1028)); // Huge Fire elementals and fire snakes
                re_list.append/*UnknownList*/(new RE_entry(14, ((14958, 1 * m, 1 * m, 1), (14893, 2 * m, 4 * m, 1)), 1017)); // Nightwalker and Greater Shadows
            }

            var aaa = RandomRange(0, re_list.Count - 1);
            encounter.Enemies = re_list[aaa].get_enemies/*Unknown*/();
            encounter.Title = re_list[aaa].dc/*Unknown*/;
            encounter.Id = re_list[aaa].id/*Unknown*/;
            return;
        }
        public static void get_forest_daytime(RandomEncounter encounter)
        {
            var m = Get_Multiplier(encounter);
            var m2 = 1;
            if (m > 1)
            {
                m2 = m - 1;
            }

            var re_list = new List<object>();
            re_list.append/*UnknownList*/(new RE_entry(2, ((14069, m, m), (14070, 2 * m, 5 * m, 0.5f)), 1000)); // bandits
            re_list.append/*UnknownList*/(new RE_entry(3, ((14052, 1 * m, 3 * m)), 1001)); // black bears
            re_list.append/*UnknownList*/(new RE_entry(1, ((14188, 1 * m, 3 * m, 0.3f), (14184, 1 * m, 6 * m, 0.2f)), 1002)); // hobgoblins and goblins
            re_list.append/*UnknownList*/(new RE_entry(2, ((14188, 3 * m, 6 * m)), 1003)); // hobgoblins
            re_list.append/*UnknownList*/(new RE_entry(3, ((14188, 1 * m, 3 * m), (14050, 1 * m, 3 * m)), 1004)); // hobgoblins and wolves
            re_list.append/*UnknownList*/(new RE_entry(1, ((14448, 2 * m, 4 * m, 1)), 1005)); // ogres
            re_list.append/*UnknownList*/(new RE_entry(3, ((14046, 1 * m, 3 * m, 1)), 1006)); // owlbears
            re_list.append/*UnknownList*/(new RE_entry(0, ((14047, 2 * m, 4 * m, 1)), 1007)); // large spiders
            re_list.append/*UnknownList*/(new RE_entry(2, ((14182, 3 * m, 6 * m)), 1008)); // stirges
            re_list.append/*UnknownList*/(new RE_entry(0, ((14089, 2 * m, 4 * m, 1)), 1009)); // giant ticks
            re_list.append/*UnknownList*/(new RE_entry(2, ((14050, 2 * m, 3 * m)), 1010)); // wolves
                                                                                           // Higher Level Encounters
            re_list.append/*UnknownList*/(new RE_entry(5, ((14053, 1 * m, 3 * m)), 1011)); // brown bears
            re_list.append/*UnknownList*/(new RE_entry(5, ((14243, 1 * m, 3 * m)), 1012)); // harpies
            if (GetGlobalFlag(500))
            {
                re_list.append/*UnknownList*/(new RE_entry(11, ((14898, 2 * m2, 4 * m2, 0.5f), (14897, 2 * m2, 4 * m2, 0.5f)), 1012)); // Leucrottas + Jackalweres
                re_list.append/*UnknownList*/(new RE_entry(12, ((14888, 3, 5, 0.5f), (14891, 0 * m, 1 * m, 0.5f), (14696, 0 * m2, 1 * m2, 0.5f), (14896, 0 * m2, 1 * m2, 0.5f), (14506, 0 * m2, 1 * m2, 0.5f), (14527, 0 * m2, 1 * m2, 0.5f), (14525, 0 * m2, 1 * m2, 0.5f)), 1013)); // Cult of the Siren + Random Thrall
                re_list.append/*UnknownList*/(new RE_entry(10, ((14542, 2 * m, 4 * m, 1)), 1014)); // Invisible Stalkers
                re_list.append/*UnknownList*/(new RE_entry(12, ((14248, 1, 1, 1), (14249, 3 * m, 5 * m)), 1015)); // Ogre chief and ogres
                if (PartyAlignment.IsEvil())
                {
                    re_list.append/*UnknownList*/(new RE_entry(12, ((14896, 2 * m, 4 * m, 1), (14895, 3 * m, 5 * m), (14894, 2 * m, 3 * m, 0.5f)), 1016)); // Holy Rollers
                }

            }

            var aaa = RandomRange(0, re_list.Count - 1);
            encounter.Enemies = re_list[aaa].get_enemies/*Unknown*/();
            encounter.Title = re_list[aaa].dc/*Unknown*/;
            encounter.Id = re_list[aaa].id/*Unknown*/;
            return;
        }
        public static void get_forest_nighttime(RandomEncounter encounter)
        {
            var m = Get_Multiplier(encounter);
            var m2 = 1;
            if (m > 1)
            {
                m2 = m - 1;
            }

            var re_list = new List<object>();
            re_list.append/*UnknownList*/(new RE_entry(1, ((14188, 1 * m, 3 * m, 0.3f), (14184, 1 * m, 6 * m, 0.2f)), 1009)); // hobgoblins and goblins
            re_list.append/*UnknownList*/(new RE_entry(2, ((14188, 3 * m, 6 * m)), 1010)); // hobgoblins
            re_list.append/*UnknownList*/(new RE_entry(3, ((14188, 1 * m, 3 * m), (14050, 1 * m, 3 * m)), 1011)); // hobgoblins and wolves
            re_list.append/*UnknownList*/(new RE_entry(2, ((14182, 3 * m, 6 * m)), 1012)); // stirges
            re_list.append/*UnknownList*/(new RE_entry(1, ((14092, 1 * m, 6 * m, 0.3f)), 1013)); // zombies
                                                                                                 // higher level encounters
            re_list.append/*UnknownList*/(new RE_entry(6, ((14291, 2 * m, 3 * m, 1)), 1014)); // Will o' wisps
            if (GetGlobalFlag(500))
            {
                re_list.append/*UnknownList*/(new RE_entry(11, ((14898, 2 * m2, 4 * m2, 0.5f), (14897, 2 * m2, 4 * m2, 0.5f)), 1015)); // Leucrottas + Jackalweres
                re_list.append/*UnknownList*/(new RE_entry(10, ((14674, 2, 3, 1), (14280, 1, 2, 1), (14137, 2 * m, 4 * m)), 1016)); // mohrgs and groaning spirits and ghasts
                re_list.append/*UnknownList*/(new RE_entry(12, ((14248, 1, 1, 1), (14249, 3 * m, 5 * m)), 1017)); // Ogre chief and ogres
                re_list.append/*UnknownList*/(new RE_entry(9, ((14542, 2 * m, 4 * m, 1)), 1018)); // Invisible Stalkers
                re_list.append/*UnknownList*/(new RE_entry(14, ((14958, 1, 1, 1), (14893, 2 * m, 4 * m, 0.5f)), 1019)); // Nightwalker and Greater Shadows
            }

            var aaa = RandomRange(0, re_list.Count - 1);
            encounter.Enemies = re_list[aaa].get_enemies/*Unknown*/();
            encounter.Title = re_list[aaa].dc/*Unknown*/;
            encounter.Id = re_list[aaa].id/*Unknown*/;
            return;
        }
        public static void get_swamp_daytime(RandomEncounter encounter)
        {
            var m = Get_Multiplier(encounter);
            var m2 = 1;
            if (m > 1)
            {
                m2 = m - 1;
            }

            var re_list = new List<object>();
            re_list.append/*UnknownList*/(new RE_entry(2, ((14182, 3 * m, 6 * m)), 1013)); // stirges
            re_list.append/*UnknownList*/(new RE_entry(0, ((14089, 2 * m, 4 * m, 1)), 1014)); // giant ticks
            re_list.append/*UnknownList*/(new RE_entry(2, ((14094, 1 * m, 2 * m, 1)), 1015)); // crayfish
            re_list.append/*UnknownList*/(new RE_entry(2, ((14057, 1 * m, 2 * m)), 1016)); // frogs
            re_list.append/*UnknownList*/(new RE_entry(0, ((14090, 2 * m, 4 * m, 1)), 1017)); // lizards
            re_list.append/*UnknownList*/(new RE_entry(2, ((14084, 2 * m, 3 * m)), 1018)); // lizardmen
            re_list.append/*UnknownList*/(new RE_entry(4, ((14084, 1 * m, 3 * m), (14090, 1 * m, 1 * m)), 1019)); // lizardmen with lizard
            re_list.append/*UnknownList*/(new RE_entry(1, ((14056, 4 * m, 9 * m, 0.144f)), 1020)); // rats
            re_list.append/*UnknownList*/(new RE_entry(3, ((14630, 1 * m, 3 * m, 0.5f)), 1021)); // snakes
                                                                                                 // Higher Level Encounters
            re_list.append/*UnknownList*/(new RE_entry(4, ((14262, 1 * m, 4 * m, 1)), 1022)); // trolls
            if (GetGlobalFlag(500))
            {
                re_list.append/*UnknownList*/(new RE_entry(12, ((14892, 1 * m, 2 * m, 0.5f), (14891, 2 * m, 4 * m, 0.5f), (14890, 2 * m2, 3 * m2, 0.5f), (14889, 1 * m2, 2 * m2, 0.5f)), 1023)); // Lizardman battlegroup
                re_list.append/*UnknownList*/(new RE_entry(11, ((14892, 1, 2, 0.5f), (14891, 2, 4, 0.5f), (14890, 2 * m2, 3 * m2, 0.5f), (14889, 1 * m2, 2 * m2, 0.5f), (14343, 1 * m2, 2 * m2), (14090, 1 * m2, 2 * m2)), 1024)); // Lizardman battlegroup + lizards + hydras
                re_list.append/*UnknownList*/(new RE_entry(9, ((14343, 1 * m, 2 * m, 1)), 1025)); // Hydras
                re_list.append/*UnknownList*/(new RE_entry(12, ((14261, 1 * m, 4 * m, 1)), 1026)); // Vodyanoi
                re_list.append/*UnknownList*/(new RE_entry(9, ((14279, 2, 3, 1), (14375, 2, 4)), 1027)); // Seahags and watersnakes
            }

            var aaa = RandomRange(0, re_list.Count - 1);
            encounter.Enemies = re_list[aaa].get_enemies/*Unknown*/();
            encounter.Title = re_list[aaa].dc/*Unknown*/;
            encounter.Id = re_list[aaa].id/*Unknown*/;
            return;
        }
        public static void get_swamp_nighttime(RandomEncounter encounter)
        {
            var m = Get_Multiplier(encounter);
            var m2 = 1;
            if (m > 1)
            {
                m2 = m - 1;
            }

            var re_list = new List<object>();
            re_list.append/*UnknownList*/(new RE_entry(2, ((14052, 2 * m, 5 * m, 0.5f)), 1001)); // black bears
            re_list.append/*UnknownList*/(new RE_entry(2, ((14182, 3 * m, 6 * m)), 1002)); // stirges
                                                                                           // higher level encounters
            re_list.append/*UnknownList*/(new RE_entry(5, ((14291, 1 * m, 4 * m, 1)), 1003)); // willowisps
            re_list.append/*UnknownList*/(new RE_entry(4, ((14262, 1 * m, 4 * m, 1)), 1004)); // trolls
            re_list.append/*UnknownList*/(new RE_entry(8, ((14280, 1 * m, 1 * m)), 1005)); // groaning spirit
            re_list.append/*UnknownList*/(new RE_entry(4, ((14128, 1 * m, 3 * m, 0.5f)), 1006)); // ghouls
            re_list.append/*UnknownList*/(new RE_entry(5, ((14135, 1 * m, 1 * m), (14128, 2 * m, 4 * m)), 1007)); // ghasts and ghouls
            if (GetGlobalFlag(500))
            {
                re_list.append/*UnknownList*/(new RE_entry(12, ((14892, 1 * m2, 2 * m2, 0.5f), (14891, 2 * m, 4 * m, 0.5f), (14890, 2 * m2, 3 * m2, 0.5f), (14889, 1 * m2, 2 * m2, 0.5f)), 1008)); // Lizardman battlegroup
                re_list.append/*UnknownList*/(new RE_entry(12, ((14892, 1, 2, 0.5f), (14891, 2, 4, 0.5f), (14890, 2 * m2, 3 * m2, 0.5f), (14889, 1 * m2, 2 * m2, 0.5f), (14343, 1 * m2, 2 * m2), (14090, 1 * m2, 2 * m2)), 1009)); // Lizardman battlegroup + lizards + hydras
                re_list.append/*UnknownList*/(new RE_entry(14, ((14958, 1, 1, 1), (14893, 2 * m, 4 * m, 1)), 1010)); // Nightwalker and Greater Shadows
                re_list.append/*UnknownList*/(new RE_entry(9, ((14343, 1 * m, 2 * m, 1)), 1011)); // Hydras
                re_list.append/*UnknownList*/(new RE_entry(12, ((14261, 1 * m, 4 * m, 1)), 1012)); // Vodyanoi
                re_list.append/*UnknownList*/(new RE_entry(9, ((14279, 1 * m, 3 * m, 1), (14375, 1 * m, 3 * m)), 1013)); // Seahags and watersnakes
                re_list.append/*UnknownList*/(new RE_entry(9, ((14824, 1 * m, 3 * m, 1), (14825, 1 * m, 3 * m, 1)), 1014)); // Ettin & Hill giant zombies
            }

            var aaa = RandomRange(0, re_list.Count - 1);
            encounter.Enemies = re_list[aaa].get_enemies/*Unknown*/();
            encounter.Title = re_list[aaa].dc/*Unknown*/;
            encounter.Id = re_list[aaa].id/*Unknown*/;
            return;
        }
        public static void get_riverside_daytime(RandomEncounter encounter)
        {
            var m = Get_Multiplier(encounter);
            var m2 = 1;
            if (m > 1)
            {
                m2 = m - 1;
            }

            var re_list = new List<object>();
            re_list.append/*UnknownList*/(new RE_entry(2, ((14094, 1 * m, 2 * m, 1)), 1002)); // crayfish
            re_list.append/*UnknownList*/(new RE_entry(0, ((14090, 2 * m, 4 * m, 1)), 1003)); // lizards
            re_list.append/*UnknownList*/(new RE_entry(2, ((14084, 2 * m, 3 * m)), 1004)); // lizardmen
            re_list.append/*UnknownList*/(new RE_entry(4, ((14084, 1 * m, 3 * m), (14090, 1 * m, 1 * m)), 1005)); // lizardmen with lizard
            re_list.append/*UnknownList*/(new RE_entry(1, ((14290, 2 * m, 5 * m, 0.5f)), 1006)); // pirates
                                                                                                 // higher level encounters
            re_list.append/*UnknownList*/(new RE_entry(4, ((14262, 1 * m, 4 * m, 1)), 1007)); // trolls
            if (GetGlobalFlag(500))
            {
                re_list.append/*UnknownList*/(new RE_entry(12, ((14892, 1 * m, 2 * m, 0.5f), (14891, 2 * m, 4 * m, 0.5f), (14890, 2 * m2, 3 * m2, 0.5f), (14889, 1 * m2, 2 * m2, 0.5f)), 1008)); // Lizardman battlegroup
                re_list.append/*UnknownList*/(new RE_entry(12, ((14888, 3, 5, 0.5f), (14891, 0 * m, 1 * m, 0.5f), (14696, 0 * m2, 1 * m2, 0.5f), (14896, 0 * m2, 1 * m2, 0.5f), (14506, 0 * m2, 1 * m2, 0.5f), (14527, 0 * m2, 1 * m2, 0.5f), (14525, 0 * m2, 1 * m2, 0.5f)), 1009)); // Cult of the Siren + Random Thrall
                re_list.append/*UnknownList*/(new RE_entry(12, ((14892, 1 * m, 2 * m, 0.5f), (14891, 2 * m, 4 * m, 0.5f), (14890, 2 * m2, 3 * m2, 0.5f), (14889, 1 * m2, 2 * m2, 0.5f), (14279, 1 * m, 3 * m)), 1010)); // Lizardman battlegroup + seahag
                re_list.append/*UnknownList*/(new RE_entry(12, ((14261, 1 * m, 4 * m, 1)), 1011)); // Vodyanoi
                re_list.append/*UnknownList*/(new RE_entry(10, ((14279, 1 * m2, 3 * m2, 1), (14375, 1 * m, 3 * m), (14240, 1 * m, 3 * m)), 1012)); // Seahags and watersnakes and kapoacinths
                if (PartyAlignment.IsEvil())
                {
                    re_list.append/*UnknownList*/(new RE_entry(12, ((14896, 2 * m2, 4 * m2, 1), (14895, 3 * m2, 5 * m2), (14894, 2 * m2, 3 * m2, 0.5f)), 1013)); // Holy Rollers
                }

            }

            var aaa = RandomRange(0, re_list.Count - 1);
            encounter.Enemies = re_list[aaa].get_enemies/*Unknown*/();
            encounter.Title = re_list[aaa].dc/*Unknown*/;
            encounter.Id = re_list[aaa].id/*Unknown*/;
            return;
        }
        public static void get_riverside_nighttime(RandomEncounter encounter)
        {
            var m = Get_Multiplier(encounter);
            var m2 = 1;
            if (m > 1)
            {
                m2 = m - 1;
            }

            var re_list = new List<object>();
            re_list.append/*UnknownList*/(new RE_entry(1, ((14130, 1 * m, 3 * m, 0.5f)), 1007)); // lacedons
            re_list.append/*UnknownList*/(new RE_entry(1, ((14081, 2 * m, 4 * m)), 1008)); // skeleton gnolls
            re_list.append/*UnknownList*/(new RE_entry(1, ((14107, 2 * m, 4 * m)), 1009)); // skeletons
                                                                                           // Higher level encounters
            re_list.append/*UnknownList*/(new RE_entry(4, ((14262, 1 * m, 4 * m, 1)), 1010)); // trolls
            if (GetGlobalFlag(500))
            {
                re_list.append/*UnknownList*/(new RE_entry(12, ((14892, 1 * m, 2 * m, 0.5f), (14891, 2 * m, 4 * m, 0.5f), (14890, 2 * m2, 3 * m2, 0.5f), (14889, 1 * m2, 2 * m2, 0.5f)), 1011)); // Lizardman battlegroup
                re_list.append/*UnknownList*/(new RE_entry(12, ((14892, 1 * m, 2 * m, 0.5f), (14891, 2 * m, 4 * m, 0.5f), (14890, 2 * m2, 3 * m2, 0.5f), (14889, 1 * m2, 2 * m2, 0.5f), (14279, 1 * m, 3 * m)), 1012)); // Lizardman battlegroup + seahag
                re_list.append/*UnknownList*/(new RE_entry(12, ((14261, 1 * m, 4 * m, 1)), 1013)); // Vodyanoi
                re_list.append/*UnknownList*/(new RE_entry(9, ((14279, 1 * m, 3 * m, 1), (14375, 1 * m, 3 * m), (14240, 1 * m, 3 * m)), 1014)); // Seahags and watersnakes and kapoacinths
            }

            var aaa = RandomRange(0, re_list.Count - 1);
            encounter.Enemies = re_list[aaa].get_enemies/*Unknown*/();
            encounter.Title = re_list[aaa].dc/*Unknown*/;
            encounter.Id = re_list[aaa].id/*Unknown*/;
            return;
        }
        public static SleepStatus can_sleep()
        {
            // can_sleep is called to test the safety of sleeping
            // it must return one of the following
            // SLEEP_SAFE
            // * it is totally safe to sleep
            // SLEEP_DANGEROUS
            // * it may provoke a random encounter
            // SLEEP_IMPOSSIBLE
            // * rest is not possible here
            // SLEEP_PASS_TIME_ONLY
            // * resting here actually only passes time, no healing or spells are retrieved
            if ((SelectedPartyLeader.GetMap() == 5115))
            {
                // Hickory Branch Cave
                return SleepStatus.Safe;
            }
            else if ((SelectedPartyLeader.GetArea() == 1))
            {
                // Hommlet
                if ((PartyLeader.HasReputation(53) || PartyLeader.HasReputation(61)))
                {
                    // Hommlet Deserter or Hommlet Destroyer - town is empty
                    return SleepStatus.Safe;
                }

                if ((PartyLeader.HasReputation(92) || PartyLeader.HasReputation(29) || PartyLeader.HasReputation(30) || PartyLeader.HasReputation(32)) && PartyLeader.GetMap() != 5014)
                {
                    return SleepStatus.Dangerous;
                }

                if (((SelectedPartyLeader.GetMap() == 5007) || (SelectedPartyLeader.GetMap() == 5008)))
                {
                    // inn first or second floor
                    if (((GetGlobalFlag(56)) || (GetQuestState(18) == QuestState.Completed)))
                    {
                        return SleepStatus.Safe;
                    }

                }

                return SleepStatus.PassTimeOnly;
            }
            else if ((SelectedPartyLeader.GetMap() == 5050))
            {
                // Herdsman House
                return SleepStatus.PassTimeOnly;
            }
            else if ((SelectedPartyLeader.GetArea() == 2))
            {
                // Moathouse
                if ((SelectedPartyLeader.GetMap() == 5003))
                {
                    // tower
                    return SleepStatus.Safe;
                }

                return SleepStatus.Dangerous;
            }
            else if ((SelectedPartyLeader.GetArea() == 3))
            {
                // Nulb
                if (((SelectedPartyLeader.GetMap() == 5085) && (GetGlobalFlag(94))))
                {
                    return SleepStatus.Safe;
                }
                else if ((((SelectedPartyLeader.GetMap() == 5060) || (SelectedPartyLeader.GetMap() == 5061)) && (GetGlobalFlag(289))))
                {
                    return SleepStatus.Safe; // WIP, thieves?
                }

                return SleepStatus.PassTimeOnly;
            }
            else if ((SelectedPartyLeader.GetMap() == 5089))
            {
                // Mona's Store
                return SleepStatus.PassTimeOnly;
            }
            else if (((SelectedPartyLeader.GetMap() == 5090) && (GetGlobalFlag(94))))
            {
                // Nulb House Crazy
                return SleepStatus.Safe;
            }
            else if ((SelectedPartyLeader.GetArea() == 4))
            {
                // Temple
                if ((SelectedPartyLeader.GetMap() == 5065 && GetGlobalFlag(840)))
                {
                    // bandit hideout
                    return SleepStatus.Safe;
                }

                return SleepStatus.Dangerous;
            }
            else if ((SelectedPartyLeader.GetMap() == 5066))
            {
                // wonnilon hideout
                // if game.global_flags[404] == 1:
                // return SLEEP_SAFE
                // else:
                return SleepStatus.Dangerous;
            }
            else if (((SelectedPartyLeader.GetMap() >= 5096) && (SelectedPartyLeader.GetMap() <= 5104)))
            {
                // Vignettes
                return SleepStatus.Impossible;
            }
            else if ((SelectedPartyLeader.GetMap() == 5107))
            {
                // ShopMap
                return SleepStatus.Safe;
            }
            else if ((((SelectedPartyLeader.GetMap() >= 5121) && (SelectedPartyLeader.GetMap() <= 5126)) || ((SelectedPartyLeader.GetMap() >= 5133) && (SelectedPartyLeader.GetMap() <= 5140)) || (SelectedPartyLeader.GetMap() == 5142) || ((SelectedPartyLeader.GetMap() >= 5148) && (SelectedPartyLeader.GetMap() <= 5150)) || ((SelectedPartyLeader.GetMap() >= 5153) && (SelectedPartyLeader.GetMap() <= 5173)) || ((SelectedPartyLeader.GetMap() >= 5175) && (SelectedPartyLeader.GetMap() <= 5188))))
            {
                // Verbobonc maps
                return SleepStatus.PassTimeOnly;
            }
            else if (((SelectedPartyLeader.GetMap() == 5143) || (SelectedPartyLeader.GetMap() == 5144) || (SelectedPartyLeader.GetMap() == 5145) || (SelectedPartyLeader.GetMap() == 5146) || (SelectedPartyLeader.GetMap() == 5147)))
            {
                // Verbobonc Castle
                if ((!GetGlobalFlag(966)))
                {
                    return SleepStatus.PassTimeOnly;
                }
                else if ((GetGlobalFlag(966)))
                {
                    if ((GetGlobalVar(765) == 0))
                    {
                        return SleepStatus.Safe;
                    }
                    else if ((GetGlobalVar(765) >= 1))
                    {
                        if ((GetQuestState(83) == QuestState.Completed))
                        {
                            return SleepStatus.Safe;
                        }
                        else if ((GetGlobalFlag(869)))
                        {
                            return SleepStatus.Impossible;
                        }

                    }

                    return SleepStatus.Dangerous;
                }

            }
            else if (((SelectedPartyLeader.GetMap() == 5151) || (SelectedPartyLeader.GetMap() == 5152)))
            {
                // Verbobonc Inn by Allyx
                if ((GetGlobalFlag(997)))
                {
                    return SleepStatus.Safe;
                }

                return SleepStatus.PassTimeOnly;
            }
            else if ((SelectedPartyLeader.GetMap() == 5174))
            {
                // Jylee's Inn
                if ((GetGlobalFlag(967)))
                {
                    return SleepStatus.Safe;
                }

                return SleepStatus.PassTimeOnly;
            }
            else if ((SelectedPartyLeader.GetMap() == 5119))
            {
                // Arena of Heroes
                return SleepStatus.PassTimeOnly;
            }
            else if (SelectedPartyLeader.GetMap() == 5116 || SelectedPartyLeader.GetMap() == 5118) // Tutorial maps 1 & 3
            {
                return SleepStatus.Impossible;
            }
            else if (SelectedPartyLeader.GetMap() == 5117) // Tutorial map 2
            {
                return SleepStatus.Safe;
            }

            return SleepStatus.Dangerous;
        }
        // Random Encounter Check tweaking my Cerulean the Blue

        public static int Survival_Check(RandomEncounter encounter)
        {
            if (encounter.Title < 1000)
            {
                var PC_roll = RandomRange(1, 20);
                var NPC_roll = RandomRange(1, 20);
                var PC_mod = PC_Modifier();
                var NPC_mod = NPC_Modifier(encounter);
                Logger.Info("{0}", "PC roll: " + PC_roll.ToString() + " + " + (PC_mod / 3).ToString() + " vs  NPC roll: " + NPC_roll.ToString() + " + " + (NPC_mod / 3).ToString());
                if (PC_roll + (PC_mod / 3) >= NPC_roll + (NPC_mod / 3))
                {
                    encounter.Title = 1;
                }
                else
                {
                    encounter.Title = 1000;
                }

                SetGlobalVar(35, NPC_roll + NPC_mod / 3 - (PC_roll + PC_mod / 3));
            }

            return 1;
        }
        public static int PC_Modifier()
        {
            var high = 0;
            var level = 0;
            var wild = 0;
            var listen = 0;
            var spot = 0;
            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                listen = obj.GetSkillLevel(SkillId.listen);
                spot = obj.GetSkillLevel(SkillId.spot);
                wild = obj.GetSkillLevel(SkillId.wilderness_lore);
                level = spot + listen + wild;
                if (level > high)
                {
                    high = level;
                }

            }

            return high;
        }
        public static int NPC_Modifier(RandomEncounter encounter)
        {
            Logger.Info("Getting NPC survival modifier.");
            var high = 0;
            var level = 0;
            var wild = 0;
            var listen = 0;
            var spot = 0;
            foreach (var i in encounter.Enemies)
            {
                var obj = GameSystems.MapObject.CreateObject(i.ProtoId, PartyLeader.GetLocation());
                listen = obj.GetSkillLevel(SkillId.listen);
                spot = obj.GetSkillLevel(SkillId.spot);
                wild = obj.GetSkillLevel(SkillId.wilderness_lore);
                Logger.Info("{0}", "NPC " + i.ToString() + " , Listen = " + listen.ToString() + " , Spot = " + spot.ToString() + " , Survival = " + wild.ToString());
                level = spot + listen + wild;
                if (level > high)
                {
                    high = level;
                }

                obj.Destroy();
            }

            Logger.Info("{0}", "Highest NPC result was: " + high.ToString());
            return high;
        }
        public static FIXME Spawn_Point(RandomEncounter encounter)
        {
            var diff = GetGlobalVar(35);
            var distance = 10 - diff;
            if (distance < 0)
            {
                distance = 0;
            }

            if (distance > 15)
            {
                distance = 15;
            }

            if ((encounter.Location == new locXY(503, 478)))
            {
                if ((distance > 8))
                {
                    Logger.Info("{0}", "Reducing distance to 8 for encounter because it is at the edges. Old distance was " + distance.ToString());
                    distance = 8;
                }

            }

            Logger.Info("Distance = {0}", distance);
            var p_list = get_circle_point_list(PartyLeader.GetLocation(), distance, 16);
            return p_list[RandomRange(0, p_list.Count - 1)];
        }
        public static locXY random_location(locXY loc, FIXME range, GameObjectBody target)
        {
            Logger.Info("Generating Location");
            var (x, y) = loc;
            Logger.Info("{0}", "Target: " + target.ToString());
            var (t_x, t_y) = target.GetLocation();
            var rand_x = RandomRange(1, 3);
            var rand_y = RandomRange(1, 3);
            var loc_x = t_x;
            var loc_y = t_y;
            while (MathF.Sqrt(Math.Pow((loc_x - t_x), 2) + Math.Pow((loc_y - t_y), 2)) < MathF.Sqrt(Math.Pow((x - t_x), 2) + Math.Pow((y - t_y), 2)))
            {
                if (rand_x == 1)
                {
                    loc_x = (x + RandomRange(1, range));
                }
                else if (rand_x == 2)
                {
                    loc_x = (x - RandomRange(1, range));
                }
                else
                {
                    loc_x = x;
                }

                if (rand_y == 1)
                {
                    loc_y = (y + RandomRange(1, range));
                }
                else if (rand_y == 2)
                {
                    loc_y = (y - RandomRange(1, range));
                }
                else
                {
                    loc_y = y;
                }

                rand_x = RandomRange(1, 3);
                rand_y = RandomRange(1, 3);
            }

            var location = new locXY(loc_x, loc_y);
            Logger.Info("{0}", "Location: " + loc_x.ToString() + " " + loc_y.ToString());
            return location;
        }
        public static int group_skill_level(GameObjectBody pc, FIXME skill)
        {
            var high = 0;
            var level = 0;
            foreach (var obj in pc.GetPartyMembers())
            {
                level = obj.GetSkillLevel(skill);
                if ((level > high))
                {
                    high = level;
                }

            }

            if (PartyLeader.GetPartyMembers().Any(o => o.HasItemByName(12677)))
            {
                // If your party has the spyglass, double the roll!
                high = high * 2;
            }

            if ((high == 0))
            {
                return 1;
            }

            return high;
        }
        public static int Get_Multiplier(RandomEncounter encounter)
        {
            var m_range = (Utilities.group_average_level(SelectedPartyLeader) / 4);
            if (m_range < 1)
            {
                m_range = 1;
            }

            var multiplier = RandomRange(1, Math.Min(m_range, 3));
            if ((encounter.Map != 5074 || Utilities.is_daytime()))
            {
                var chance = 40 / Utilities.group_average_level(SelectedPartyLeader);
                if ((RandomRange(1, chance) != 1))
                {
                    multiplier = 1;
                }

            }

            if ((encounter.Id >= 2000))
            {
                var multipler = 1;
            }

            return multiplier;
        }
        // By Darmagon

        public static List<GameObjectBody> get_circle_point_list(FIXME center, int radius, FIXME num_points)
        {
            // def get_circle_point_list(center, radius,num_points): # By Darmagon
            var p_list = new List<GameObjectBody>();
            var (offx, offy) = center;
            var i = 0f;
            while (i < 2 * pi)
            {
                var posx = (int)(MathF.Cos(i) * radius) + offx;
                var posy = (int)(MathF.Sin(i) * radius) + offy;
                var loc = new locXY(posx, posy);
                p_list.Add(loc);
                i = i + pi / (num_points / 2);
            }

            return p_list;
        }
        // By Livonya

        public static int Slaughtered_Caravan()
        {
            // def Slaughtered_Caravan(): # By Livonya
            var dead = GameSystems.MapObject.CreateObject(2112, new locXY(477, 484));
            dead.Rotation = RandomRange(1, 20);
            dead = GameSystems.MapObject.CreateObject(2118, new locXY(486, 462));
            dead.Rotation = RandomRange(1, 20);
            dead = GameSystems.MapObject.CreateObject(2125, new locXY(465, 475));
            dead.Rotation = RandomRange(1, 20);
            dead = GameSystems.MapObject.CreateObject(2112, new locXY(477, 467));
            dead.Rotation = RandomRange(1, 20);
            dead = GameSystems.MapObject.CreateObject(2112, new locXY(481, 473));
            dead.Rotation = RandomRange(1, 20);
            dead = GameSystems.MapObject.CreateObject(2125, new locXY(489, 498));
            dead.Rotation = RandomRange(1, 20);
            var chest = GameSystems.MapObject.CreateObject(1004, new locXY(482, 465));
            chest.Rotation = 2;
            dead = GameSystems.MapObject.CreateObject(2118, new locXY(476, 506));
            dead.Rotation = RandomRange(1, 20);
            var ticker = GameSystems.MapObject.CreateObject(14638, new locXY(465, 475));
            var tree = GameSystems.MapObject.CreateObject(2017, new locXY(474, 455));
            tree.Rotation = RandomRange(1, 20);
            tree = GameSystems.MapObject.CreateObject(2017, new locXY(504, 489));
            tree.Rotation = RandomRange(1, 20);
            tree = GameSystems.MapObject.CreateObject(2017, new locXY(474, 496));
            tree.Rotation = RandomRange(1, 20);
            tree = GameSystems.MapObject.CreateObject(2017, new locXY(471, 469));
            tree.Rotation = RandomRange(1, 20);
            return 1;
        }
        // By Livonya

        public static int NPC_Self_Buff()
        {
            // def NPC_Self_Buff(): # By Livonya
            // THIS WAS ADDED TO AID IN NPC SELF BUFFING			##
            // ##
            for (var i = 712; i < 733; i++)
            {
                Logger.Info("{0}", i);
                SetGlobalVar(i, 0);
            }

            return 1;
        }

    }
}
