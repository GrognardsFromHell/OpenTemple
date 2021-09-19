using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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
using JetBrains.Annotations;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Systems.Script.Hooks;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    internal readonly struct EncounterTemplateEnemy
    {
        public readonly int ProtoId;

        public readonly int MinCount;

        public readonly int MaxCount;

        public readonly float CRFactor;

        public EncounterTemplateEnemy(int protoId, int minCount, int maxCount, float crFactor)
        {
            ProtoId = protoId;
            MinCount = minCount;
            MaxCount = maxCount;
            CRFactor = crFactor;
        }
    }

    internal class EncounterTemplate
    {
        public int Id { get; }

        private readonly int _challengeRating;

        private readonly List<EncounterTemplateEnemy> _enemies = new List<EncounterTemplateEnemy>();

        public EncounterTemplate(int challengeRating, int id)
        {
            _challengeRating = challengeRating;
            Id = id;
        }

        public EncounterTemplate AddEnemies(int protoId, int minCount, int maxCount, float dcModifier)
        {
            _enemies.Add(new EncounterTemplateEnemy(protoId, minCount, maxCount, dcModifier));
            return this;
        }

        public EncounterTemplate AddEnemies(int protoId, int minCount, int maxCount)
        {
            _enemies.Add(new EncounterTemplateEnemy(protoId, minCount, maxCount, 1.0f));
            return this;
        }

        public List<RandomEncounterEnemy> Instantiate(out int dc)
        {
            var enemy_list_output = new List<RandomEncounterEnemy>();
            var dc_mod = 0f;
            foreach (var tup_x in _enemies)
            {
                var nn = RandomRange(tup_x.MinCount, tup_x.MaxCount);
                if (nn > 0)
                {
                    enemy_list_output.Add(new RandomEncounterEnemy(tup_x.ProtoId, nn));
                    if (tup_x.CRFactor > 0) // DC modifier
                    {
                        dc_mod += tup_x.CRFactor * nn;
                    }
                }
            }

            dc = _challengeRating + (int) dc_mod;
            return enemy_list_output;
        }
    }

    internal class EncounterTable
    {
        private readonly List<EncounterTemplate> _encounters = new List<EncounterTemplate>();

        public EncounterTemplate AddEncounter(int challengeRating, int id)
        {
            var encounter = new EncounterTemplate(challengeRating, id);
            _encounters.Add(encounter);
            return encounter;
        }

        public void Pick(RandomEncounter encounter)
        {
            var template = GameSystems.Random.PickRandom(_encounters);
            encounter.Enemies = template.Instantiate(out var dc);
            encounter.DC = dc;
            encounter.Id = template.Id;
        }
    }

    [UsedImplicitly]
    public class RandomEncountersHook : IRandomEncountersHook
    {
        public SleepStatus CalculateSleepStatus() => RandomEncounters.can_sleep();
    }

    public class RandomEncounters
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public static bool encounter_exists(RandomEncounterQuery setup, RandomEncounter encounter)
        {
            Logger.Info("Testing encounter_exists");
            bool check;
            if (((setup.Type == RandomEncounterType.Resting)))
            {
                check = check_sleep_encounter(setup, encounter);
            }
            else
            {
                check = check_random_encounter(setup, encounter);
            }

            // Added by Sitra Achara
            if ((new[] {5066, 5067, 5005}).Contains(PartyLeader.GetMap()))
            {
                // If you rest inside the Temple or Moathouse, execute the Reactive Behavior scripts
                PartyLeader.ExecuteObjectScript(PartyLeader, ObjScriptEvent.NewMap);
            }

            Logger.Info("{0}", "Result: " + check);
            // encounter.map = 5074 # TESTING!!! REMOVE!!!
            return check;
        }

        public static void encounter_create(RandomEncounter encounter)
        {
            Logger.Info("{0}",
                "Testing encounter_create with id=" + encounter.Id + ", map = " + encounter.Map);
            // encounter_create adds all the objects to the scene
            // WIP temp location for now
            var target = SelectedPartyLeader;
            locXY location;
            int range;
            if ((encounter.Id >= 4000 || encounter.Id == 3000))
            {
                location = SelectedPartyLeader.GetLocation();
                range = 1;
            }
            else
            {
                location = Spawn_Point(encounter);
                range = 6; // this is a "sub-range" actually, relative to the above spawn point
                var numP = GameSystems.Party.PartySize - 1;
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
            Logger.Info("{0}", "Spawning encounter enemies, total: " + total);
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
                        location = new locXY(505, 479);
                        range = 6;
                    }

                    var spawn_loc = random_location(location, range, target);
                    if ((encounter.Id >= 4000))
                    {
                        var numP = GameSystems.Party.PartySize - 1;
                        var xxx = RandomRange(0, numP);
                        target = GameSystems.Party.GetPartyGroupMemberN(xxx);
                        location = target.GetLocation();
                        if ((target == null))
                        {
                            target = PartyLeader;
                            location = PartyLeader.GetLocation();
                        }

                        if (SelectedPartyLeader.GetMap() == 5066)
                        {
                            // scripting wonnilon's hideout
                            var legit_list = new List<locXY>();
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

                        if ((SelectedPartyLeader.GetArea() == 1 && SelectedPartyLeader.HasReputation(92)) ||
                            ((encounter.Id < 2000) || (encounter.Id >= 4000)))
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

        public static bool check_random_encounter(RandomEncounterQuery setup, RandomEncounter encounter)
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
                    ScriptDaemon.set_f("qs_is_repeatable_encounter", false);
                    return true;
                }
                else if ((check_unrepeatable_encounter(setup, encounter)))
                {
                    Survival_Check(encounter);
                    ScriptDaemon.set_f("qs_is_repeatable_encounter", false);
                    return true;
                }
                else
                {
                    if (GetGlobalFlag(403))
                    {
                        if (ScriptDaemon.get_f("qs_disable_random_encounters"))
                        {
                            return false;
                        }
                    }

                    var check = check_repeatable_encounter(setup, encounter);
                    // encounter.location = location_from_axis( 503, 478 ) #for testing only
                    Survival_Check(encounter);
                    ScriptDaemon.set_f("qs_is_repeatable_encounter", true);
                    return check;
                }
            }

            // print 'nope'
            return false;
        }

        public static bool check_sleep_encounter(RandomEncounterQuery setup, RandomEncounter encounter)
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
            int ran_factor = 10; // default - 56 percent chance of encounter in 8 hour rest

            if (SelectedPartyLeader.GetMap() == 5015 || SelectedPartyLeader.GetMap() == 5016 ||
                SelectedPartyLeader.GetMap() == 5017)
            {
                // resting in Burne's tower
                ran_factor = 31;
            }
            else if (SelectedPartyLeader.GetMap() == 5001 || SelectedPartyLeader.GetMap() == 5007)
            {
                // resting in Hommlet Exterior
                ran_factor = 18;
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
            else if (SelectedPartyLeader.GetMap() == 5143 || SelectedPartyLeader.GetMap() == 5144 ||
                     SelectedPartyLeader.GetMap() == 5145 || SelectedPartyLeader.GetMap() == 5146 ||
                     SelectedPartyLeader.GetMap() == 5147)
            {
                // resting in Verbobonc castle
                ran_factor = 36;
            }
            else if (SelectedPartyLeader.GetMap() == 5128 || SelectedPartyLeader.GetMap() == 5129 ||
                     SelectedPartyLeader.GetMap() == 5130 || SelectedPartyLeader.GetMap() == 5131)
            {
                // resting in Verbobonc Underdark interior
                ran_factor = 30;
            }
            else if (SelectedPartyLeader.GetMap() == 5127)
            {
                // resting in Verbobonc Underdark interior entryway - not safe but won't get attacked
                ran_factor = 0;
            }
            else if (SelectedPartyLeader.GetMap() == 5191)
            {
                // resting in Hickory Branch Crypt
                ran_factor = 25;
            }
            else if (SelectedPartyLeader.GetMap() == 5093 || SelectedPartyLeader.GetMap() == 5192 ||
                     SelectedPartyLeader.GetMap() == 5193)
            {
                // resting in Welkwood Bog
                if ((!GetGlobalFlag(976))) // Mathel alive - not safe but won't get attacked
                {
                    ran_factor = 0;
                }
                else if ((GetGlobalFlag(976))) // Mathel dead
                {
                    ran_factor = 10;
                }
            }

            if ((RandomRange(1, 100) <= ran_factor))
            {
                encounter.Id = 4000;
                if ((SelectedPartyLeader.GetArea() == 1)) // ----- Hommlet
                {
                    if ((SelectedPartyLeader.HasReputation(29) || SelectedPartyLeader.HasReputation(30)) &&
                        !(SelectedPartyLeader.HasReputation(92) || SelectedPartyLeader.HasReputation(32)))
                    {
                        // Slightly naughty
                        var enemy_list = new[] {(14371, 1, 2, 1)};
                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((SelectedPartyLeader.HasReputation(92) || SelectedPartyLeader.HasReputation(32)) &&
                             ScriptDaemon.get_v(439) < 9)
                    {
                        // Moderately naughty
                        var enemy_list = new[] {(14371, 2, 4, 1)};
                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((SelectedPartyLeader.HasReputation(92) || SelectedPartyLeader.HasReputation(32)) &&
                             ScriptDaemon.get_v(439) >= 9 && ScriptDaemon.get_v(439) <= 19)
                    {
                        // Very Naughty
                        var num = RandomRange(4, 7);
                        var temp = new List<RandomEncounterEnemy>
                        {
                            new RandomEncounterEnemy(14371, num)
                        };
                        if (!GetGlobalFlag(336))
                        {
                            temp.Add(new RandomEncounterEnemy(14004, 1));
                            ScriptDaemon.set_v(439, ScriptDaemon.get_v(439) | 256);
                        }

                        if (!GetGlobalFlag(437))
                        {
                            temp.Add(new RandomEncounterEnemy(14006, 1));
                            ScriptDaemon.set_v(439, ScriptDaemon.get_v(439) | 512);
                        }

                        if ((ScriptDaemon.get_v(439) & 1024) == 0)
                        {
                            temp.Add(new RandomEncounterEnemy(14012, 1));
                            ScriptDaemon.set_v(439, ScriptDaemon.get_v(439) | 1024);
                        }

                        ScriptDaemon.record_time_stamp(443);
                        encounter.Enemies = temp;
                        encounter.DC = 1;
                        return true;
                    }
                    else if ((SelectedPartyLeader.HasReputation(92) || SelectedPartyLeader.HasReputation(32)) &&
                             ScriptDaemon.get_v(439) >= 20 &&
                             CurrentTimeSeconds < ScriptDaemon.get_v(443) + 2 * 30 * 24 * 60 * 60)
                    {
                        // NAUGHTINESS OVERWHELMING
                        // You've exterminated all the badgers!
                        // And not enough time has passed since you started massacring the badgers for a big revenge encounter
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if ((SelectedPartyLeader.GetArea() == 2)) // ----- Moat house
                {
                    if ((SelectedPartyLeader.GetMap() == 5002)) // moathouse ruins
                    {
                        // frog, tick, willowisp, wolf, crayfish, lizard, rat, snake, spider, brigand
                        var enemy_list = new[]
                        {
                            (14057, 1, 3, 1), (14089, 2, 4, 1), (14291, 1, 4, 6), (14050, 2, 3, 1),
                            (14094, 1, 2, 3), (14090, 2, 4, 1), (14056, 4, 9, 1), (14630, 1, 3, 1), (14047, 2, 4, 1),
                            (14070, 2, 5, 1)
                        };
                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5004)) // moathouse interior
                    {
                        // rat, tick, lizard, snake, brigand
                        var enemy_list = new[]
                        {
                            (14056, 4, 9, 1), (14089, 2, 4, 1), (14090, 2, 4, 1), (14630, 1, 3, 1),
                            (14070, 2, 5, 1)
                        };
                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5005)) // moathouse dungeon
                    {
                        // rat, lizard, zombie, bugbear, gnoll (unless gone), Lareth guard (unless Lareth killed or in group)
                        var enemy_list = new[]
                        {
                            (14056, 4, 9, 1), (14090, 2, 4, 1), (14092, 1, 3, 1), (14093, 1, 3, 2),
                            (14067, 1, 3, 1), (14074, 2, 4, 1)
                        };
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        if (x)
                        {
                            if ((encounter.Enemies[0].ProtoId == 14067)) // check gnolls
                            {
                                if ((GetGlobalFlag(288)))
                                {
                                    return false;
                                }
                            }
                            else if ((encounter.Enemies[0].ProtoId == 14074)) // check Lareth
                            {
                                if (((GetGlobalFlag(37)) || (GetGlobalFlag(50))))
                                {
                                    return false;
                                }
                            }
                        }

                        return x;
                    }

                    return false;
                }
                else if ((SelectedPartyLeader.GetArea() == 3)) // ----- Nulb
                {
                    return false; // WIP, thieves?
                }
                else if ((SelectedPartyLeader.GetArea() == 4 || SelectedPartyLeader.GetMap() == 5105)) // ----- Temple
                {
                    if ((SelectedPartyLeader.GetMap() == 5062)) // temple exterior
                    {
                        // bandit, drelb (night only), rat, snake, spider
                        var enemy_list = new[]
                        {
                            (14070, 2, 5, 1), (14275, 1, 1, 4), (14056, 4, 9, 1), (14630, 1, 3, 1),
                            (14047, 2, 4, 1)
                        };
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        if (x)
                        {
                            if ((encounter.Enemies[0].ProtoId == 14275)) // check drelb
                            {
                                if ((Utilities.is_daytime()))
                                {
                                    return false;
                                }
                            }
                        }

                        return x;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5064)) // temple interior
                    {
                        // bandit, drelb (night only), rat, GT patrol
                        var enemy_list = new[] {(14070, 2, 5, 1), (14275, 1, 1, 4), (14056, 4, 9, 1), (14170, 2, 5, 2)};
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        if (x)
                        {
                            if ((encounter.Enemies[0].ProtoId == 14275)) // check drelb
                            {
                                if ((Utilities.is_daytime()))
                                {
                                    return false;
                                }
                            }
                        }

                        return x;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5065)) // temple tower
                    {
                        // bandit, GT patrol
                        var enemy_list = new[] {(14070, 2, 5, 1), (14170, 2, 5, 2)};
                        return get_sleep_encounter_enemies(enemy_list, encounter);
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
                            return false;
                        }

                        // bandit, gnoll, ghoul, gelatinous cube, gray ooze, ogre, GT patrol
                        var enemy_list = new[]
                        {
                            (14070, 2, 5, 1), (14078, 2, 5, 1), (14128, 2, 5, 1), (14139, 1, 1, 3),
                            (14140, 1, 1, 4), (14448, 1, 1, 2), (14170, 2, 5, 2)
                        };
                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5067)) // temple dungeon 2
                    {
                        // bandit, bugbear, carrion crawler, ochre jelly, ogre, troll
                        var enemy_list = new[]
                        {
                            (14070, 2, 5, 1), (14170, 4, 6, 2), (14190, 1, 1, 4), (14142, 1, 1, 5),
                            (14448, 2, 4, 2), (14262, 1, 2, 5)
                        };
                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5105)) // temple dungeon 3 lower
                    {
                        // black pudding, ettin, gargoyle, hill giant, ogre, troll
                        var enemy_list = new[]
                        {
                            (14143, 1, 1, 7), (14697, 1, 2, 5), (14239, 5, 8, 4), (14221, 2, 3, 7),
                            (14448, 5, 8, 2), (14262, 2, 3, 5)
                        };
                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5079)) // temple dungeon 3 upper
                    {
                        return false;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5080)) // temple dungeon 4
                    {
                        // black pudding, ettin, troll, gargoyle, hill giant, ogre + bugbear
                        var enemy_list = new[]
                        {
                            (14143, 1, 1, 7), (14697, 1, 1, 5), (14262, 1, 2, 5), (14239, 3, 6, 4),
                            (14220, 1, 2, 7), (14448, 1, 4, 3)
                        };
                        var x = get_sleep_encounter_enemies(enemy_list, encounter);
                        if (x)
                        {
                            if ((encounter.Enemies[0].ProtoId == 14448)) // reinforce ogres with bugbears
                            {
                                encounter.Enemies.Add(new RandomEncounterEnemy(14174, RandomRange(2, 5)));
                            }
                        }

                        return x;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5081)) // air node
                    {
                        // air elemental, ildriss grue, vapor rat, vortex, windwalker
                        var enemy_list = new[]
                        {
                            (14292, 1, 2, 5), (14192, 1, 2, 4), (14068, 1, 4, 2), (14293, 1, 2, 2),
                            (14294, 1, 1, 4)
                        };
                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5082)) // earth node
                    {
                        // basilisk, chaggrin grue, crystal ooze, earth elemental
                        var enemy_list = new[] {(14295, 1, 1, 5), (14191, 1, 4, 4), (14141, 1, 1, 4), (14296, 1, 2, 5)};
                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5083)) // fire node
                    {
                        // fire bats, fire elemental, fire snake, fire toad
                        var enemy_list = new[] {(14297, 2, 5, 2), (14298, 1, 2, 5), (14299, 1, 2, 1), (14300, 1, 2, 3)};
                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5084)) // water node
                    {
                        // floating eye, ice lizard, lizard man, vodyanoi, water elemental, kopoacinth, lacedon, merrow
                        var enemy_list = new[]
                        {
                            (14301, 1, 1, 1), (14109, 1, 1, 3), (14084, 2, 4, 1), (14261, 1, 1, 7),
                            (14302, 1, 2, 5), (14240, 2, 3, 4), (14132, 3, 5, 1), (14108, 3, 5, 2)
                        };
                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }

                    return false;
                }
                else if ((SelectedPartyLeader.GetMap() == 5143 || SelectedPartyLeader.GetMap() == 5144 ||
                          SelectedPartyLeader.GetMap() == 5145 || SelectedPartyLeader.GetMap() == 5146 ||
                          SelectedPartyLeader.GetMap() == 5147)) // ----- Verbobonc castle
                {
                    // ghost
                    var enemy_list = new[] {(14819, 1, 1, 1)};
                    return get_sleep_encounter_enemies(enemy_list, encounter);
                }
                else if ((SelectedPartyLeader.GetMap() == 5128 || SelectedPartyLeader.GetMap() == 5129 ||
                          SelectedPartyLeader.GetMap() == 5130 ||
                          SelectedPartyLeader.GetMap() == 5131)) // ----- Verbobonc Underdark inside
                {
                    if ((GetQuestState(69) != QuestState.Completed) && (GetQuestState(74) != QuestState.Completed))
                    {
                        // large spider, fiendish small monstrous spider, fiendish medium monstrous spider, fiendish large monstrous spider
                        var enemy_list = new[]
                            {(14047, 5, 10, 1), (14672, 4, 8, 1), (14620, 3, 6, 1), (14671, 2, 4, 1)};
                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((GetQuestState(69) == QuestState.Completed) && (GetQuestState(74) == QuestState.Completed))
                    {
                        // dire rat
                        var enemy_list = new[] {(14056, 6, 12, 1)};
                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                }
                else if ((SelectedPartyLeader.GetMap() == 5132)) // ----- Verbobonc Underdark outside
                {
                    // wolf
                    var enemy_list = new[] {(14050, 4, 8, 1)};
                    return get_sleep_encounter_enemies(enemy_list, encounter);
                }
                else if ((SelectedPartyLeader.GetMap() == 5093)) // ----- Welkwood Bog outside
                {
                    // wolf, jackal, giant frog, giant lizard, carrion crawler, wild boar
                    var enemy_list = new[]
                    {
                        (14050, 2, 6, 1), (14051, 2, 6, 1), (14057, 1, 3, 1), (14090, 1, 3, 1),
                        (14190, 1, 1, 1), (14522, 2, 4, 1)
                    };
                    return get_sleep_encounter_enemies(enemy_list, encounter);
                }
                else if ((SelectedPartyLeader.GetMap() == 5192 || SelectedPartyLeader.GetMap() == 5193)
                ) // ----- Welkwood Bog inside
                {
                    // dire rat
                    var enemy_list = new[] {(14056, 6, 12, 1)};
                    return get_sleep_encounter_enemies(enemy_list, encounter);
                }
                else if ((SelectedPartyLeader.GetMap() == 5095)) // ----- Hickory Branch
                {
                    if ((GetQuestState(62) != QuestState.Completed))
                    {
                        // hill giant, gnoll, orc fighter, orc bowman, bugbear, ogre
                        var enemy_list = new[]
                        {
                            (14988, 1, 2, 1), (14475, 3, 6, 1), (14745, 3, 6, 1), (14467, 2, 4, 1),
                            (14476, 2, 4, 1), (14990, 1, 2, 1)
                        };
                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((GetQuestState(62) == QuestState.Completed))
                    {
                        // black bear, brown bear, worg, dire wolf, dire bear, dire boar, wild boar
                        var enemy_list = new[]
                        {
                            (14052, 1, 2, 1), (14053, 1, 1, 1), (14352, 1, 2, 1), (14391, 1, 2, 1),
                            (14506, 1, 1, 1), (14507, 1, 1, 1), (14522, 2, 4, 1)
                        };
                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                }
                else if ((SelectedPartyLeader.GetMap() == 5191)) // ----- Hickory Branch Crypt
                {
                    // dire rat
                    var enemy_list = new[] {(14056, 6, 12, 1)};
                    return get_sleep_encounter_enemies(enemy_list, encounter);
                }
                else if ((SelectedPartyLeader.GetMap() == 5141)) // ----- Verbobonc Drainage Tunnels
                {
                    // dire rat
                    var enemy_list = new[] {(14433, 9, 15, 1)};
                    return get_sleep_encounter_enemies(enemy_list, encounter);
                }
                else if ((SelectedPartyLeader.GetMap() == 5120)) // ----- Gnarley Forest
                {
                    // stirge, will-o'-wisp, basilisk, dire lizard
                    var enemy_list = new[] {(14182, 5, 10, 1), (14291, 4, 8, 1), (14295, 1, 3, 1), (14450, 1, 3, 1)};
                    return get_sleep_encounter_enemies(enemy_list, encounter);
                }
                else
                {
                    var party_level = Utilities.group_average_level(SelectedPartyLeader);
                    get_repeatable_encounter_enemies(setup, encounter);
                    while ((encounter.DC > (party_level + 2)))
                    {
                        // while (encounter.dc > (party_level+2) or ( game.random_range(1, party_level) >  encounter.dc  ) ): # makes it more likely for high level parties to skip mundane encounters. Needs some adjustment so level 12 parties don't encounter 50 trolls all the time TODO
                        get_repeatable_encounter_enemies(setup, encounter);
                    }

                    encounter.Id = 4000;
                    return true;
                }
            }

            return false;
        }

        private static bool get_sleep_encounter_enemies(IList<ValueTuple<int, int, int, int>> enemy_list,
            RandomEncounter encounter)
        {
            var total = enemy_list.Count;
            var n = RandomRange(0, total - 1);
            encounter.DC = enemy_list[n].Item4;
            var party_level = Utilities.group_average_level(SelectedPartyLeader);
            if ((encounter.DC > (party_level + 2)))
            {
                // try again
                n = RandomRange(0, total - 1);
                encounter.DC = enemy_list[n].Item4;
                if ((encounter.DC > (party_level + 2)))
                {
                    return false;
                }
            }

            var num = RandomRange(enemy_list[n].Item2, enemy_list[n].Item3);
            encounter.AddEnemies(enemy_list[n].Item1, num);
            return true;
        }

        public static bool check_predetermined_encounter(RandomEncounterQuery setup, RandomEncounter encounter)
        {
            while (GameSystems.RandomEncounter.TryTakeQueuedEncounter(out var id))
            {
                if ((!GetGlobalFlag(id - 3000 + 277)))
                {
                    SetGlobalFlag(id - 3000 + 277, true);
                    encounter.Id = id;
                    encounter.DC = 1000; // unavoidable
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
                            return false;
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
                        return false;
                    }

                    return true;
                }
            }

            return false;
        }

        public static bool check_unrepeatable_encounter(RandomEncounterQuery setup, RandomEncounter encounter)
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
                        encounter.DC = 9;
                        encounter.AddEnemies(14142, 4);
                    }
                    else if ((id == 2001)) // zaxis
                    {
                        encounter.DC = 5;
                        encounter.AddEnemies(14331, 1);
                    }
                    else if ((id == 2002)) // adventuring party
                    {
                        encounter.DC = 9;
                        encounter.AddEnemies(14332, 1);
                        encounter.AddEnemies(14333, 1);
                        encounter.AddEnemies(14334, 1);
                        encounter.AddEnemies(14335, 1);
                        encounter.AddEnemies(14336, 1);
                        encounter.AddEnemies(14622, 1);
                    }
                    else
                    {
                        return false;
                    }

                    var party_level = Utilities.group_average_level(SelectedPartyLeader);
                    if ((encounter.DC > (party_level + 2)))
                    {
                        return false;
                    }

                    SetGlobalFlag(id - 2000 + 227, true);
                    return true;
                }
            }

            return false;
        }

        public static bool check_repeatable_encounter(RandomEncounterQuery setup, RandomEncounter encounter)
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
            while (countt < 5 && (encounter.DC > (party_level + 2) ||
                                  (party_level > 10 && encounter.DC < 5 && GetGlobalFlag(500) &&
                                   RandomRange(1, 100) <= 87)))
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
                return false;
            }

            return true;
        }
        // NEW MAP GENERATOR BY CERULEAN THE BLUE

        public static int get_map_from_terrain(MapTerrain terrain)
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

            var re_list = new EncounterTable();
            re_list.AddEncounter(2, 1000)
                .AddEnemies(14069, m, m)
                .AddEnemies(14070, 2 * m, 5 * m, 0.5f); // bandits
            re_list.AddEncounter(3, 1018)
                .AddEnemies(14067, 2 * m, 4 * m); // gnolls
            re_list.AddEncounter(4, 1019)
                .AddEnemies(14067, 1 * m, 3 * m)
                .AddEnemies(14050, 1 * m, 3 * m); // gnolls and wolves
            re_list.AddEncounter(1, 1003)
                .AddEnemies(14184, 3 * m, 6 * m); // goblins
            re_list.AddEncounter(3, 1004)
                .AddEnemies(14184, 3 * m, 6 * m)
                .AddEnemies(14050, 1 * m, 3 * m); // goblins and wolves
            re_list.AddEncounter(1, 1003)
                .AddEnemies(14640, 3 * m, 10 * m)
                .AddEnemies(14641, 1 * m, 1 * m); // kobolds and kobold sergeant
            re_list.AddEncounter(1, 1006)
                .AddEnemies(14051, 2 * m, 4 * m); // jackals

            // Higher level encounters
            re_list.AddEncounter(4, 1041)
                .AddEnemies(14697, 1 * m, 4 * m, 1); // ettins (potentially)
            re_list.AddEncounter(7, 1045)
                .AddEnemies(14217, 1 * m, 4 * m, 1); // hill giants
            re_list.AddEncounter(7, 1040)
                .AddEnemies(14697, m, m)
                .AddEnemies(14053, 1 * m, 2 * m); // ettin vs. brown bears
            re_list.AddEncounter(6, 1039)
                .AddEnemies(14697, m, m)
                .AddEnemies(14188, 3 * m, 6 * m); // ettin vs. hobgoblins

            if (GetGlobalFlag(500))
            {
                re_list.AddEncounter(11, 1022)
                    .AddEnemies(14892, 1 * m, 2 * m, 0.5f)
                    .AddEnemies(14891, 2 * m, 4 * m, 0.5f)
                    .AddEnemies(14890, 2 * m2, 3 * m2, 0.5f)
                    .AddEnemies(14889, 1 * m2, 2 * m2, 0.5f); // Lizardman battlegroup
                re_list.AddEncounter(11, 1024)
                    .AddEnemies(14888, 3, 5, 0.5f)
                    .AddEnemies(14891, 0 * m, 1 * m, 0.5f)
                    .AddEnemies(14696, 0 * m2, 1 * m2, 0.5f)
                    .AddEnemies(14896, 0 * m2, 1 * m2, 0.5f)
                    .AddEnemies(14506, 0 * m2, 1 * m2, 0.5f)
                    .AddEnemies(14527, 0 * m2, 1 * m2, 0.5f)
                    .AddEnemies(14525, 0 * m2, 1 * m2, 0.5f); // Cult of the Siren + Random Thrall
                re_list.AddEncounter(10, 1022)
                    .AddEnemies(14898, 2 * m2, 4 * m2, 0.5f)
                    .AddEnemies(14897, 2 * m2, 4 * m2, 0.5f); // Leucrottas + Jackalweres
                re_list.AddEncounter(11, 1016)
                    .AddEnemies(14248, 1, 1, 1)
                    .AddEnemies(14249, 3 * m, 5 * m); // Ogre chief and ogres
                re_list.AddEncounter(11, 1023)
                    .AddEnemies(14697, 3 * m, 5 * m)
                    .AddEnemies(14248, 1, 1, 1)
                    .AddEnemies(14249, 3 * m, 5 * m); // Ogre chief and ogres vs. ettins (clash of the titans! :) )
                if (PartyAlignment.IsEvil())
                {
                    re_list.AddEncounter(11, 1022)
                        .AddEnemies(14894, 2, 3, 0.5f)
                        .AddEnemies(14896, 2, 4, 1)
                        .AddEnemies(14895, 3, 5); // Holy Rollers
                }
            }

            re_list.Pick(encounter);
        }

        public static void get_scrub_nighttime(RandomEncounter encounter)
        {
            var m = Get_Multiplier(encounter);
            var m2 = 1;
            if (m > 1)
            {
                m2 = m - 1;
            }

            var re_list = new EncounterTable();
            re_list.AddEncounter(3, 1026).AddEnemies(14093, m, m)
                .AddEnemies(14184, 3 * m, 6 * m); // bugbears and goblins
            re_list.AddEncounter(4, 1027).AddEnemies(14093, m, m)
                .AddEnemies(14188, 4 * m, 9 * m); // bugbears and hobgoblins
            re_list.AddEncounter(1, 1028).AddEnemies(14093, 2 * m, 4 * m, 1); // bugbears
            re_list.AddEncounter(3, 1018).AddEnemies(14067, 2 * m, 4 * m); // gnolls
            re_list.AddEncounter(4, 1019).AddEnemies(14067, 1 * m, 3 * m)
                .AddEnemies(14050, 1 * m, 3 * m); // gnolls and wolves
            re_list.AddEncounter(1, 1003).AddEnemies(14184, 3 * m, 6 * m); // goblins
            re_list.AddEncounter(3, 1004).AddEnemies(14184, 3 * m, 6 * m)
                .AddEnemies(14050, 1 * m, 3 * m); // goblins and wolves
            re_list.AddEncounter(1, 1003).AddEnemies(14640, 3 * m, 10 * m)
                .AddEnemies(14641, 1 * m, 1 * m); // kobolds and kobold sergeant
            re_list.AddEncounter(1, 1014).AddEnemies(14092, 1 * m, 6 * m, 0.3f); // zombies
            // Higher level encounters
            re_list.AddEncounter(7, 1029).AddEnemies(14093, 2 * m, 4 * m)
                .AddEnemies(14050, 2 * m, 4 * m); // bugbears and wolves
            if (GetGlobalFlag(500))
            {
                re_list.AddEncounter(11, 1022)
                    .AddEnemies(14892, 1 * m2, 2 * m2, 0.5f)
                    .AddEnemies(14891, 2 * m, 4 * m, 0.5f)
                    .AddEnemies(14890, 2 * m2, 3 * m2, 0.5f)
                    .AddEnemies(14889, 1 * m2, 2 * m2, 0.5f); // Lizardman battlegroup
                re_list.AddEncounter(10, 1022)
                    .AddEnemies(14898, 2 * m2, 4 * m2, 0.5f)
                    .AddEnemies(14897, 2 * m2, 4 * m2, 0.5f); // Leucrottas + Jackalweres
                re_list.AddEncounter(9, 1017)
                    .AddEnemies(14542, 2 * m, 4 * m, 1); // Invisible Stalkers
                re_list.AddEncounter(11, 1016)
                    .AddEnemies(14248, 1, 1, 1)
                    .AddEnemies(14249, 3 * m, 5 * m); // Ogre chief and ogres
                re_list.AddEncounter(11, 1028)
                    .AddEnemies(14510, 1 * m, 3 * m, 1)
                    .AddEnemies(14299, 1 * m, 3 * m); // Huge Fire elementals and fire snakes
                re_list.AddEncounter(14, 1017)
                    .AddEnemies(14958, 1 * m, 1 * m, 1)
                    .AddEnemies(14893, 2 * m, 4 * m, 1); // Nightwalker and Greater Shadows
            }

            re_list.Pick(encounter);
        }

        public static void get_forest_daytime(RandomEncounter encounter)
        {
            var m = Get_Multiplier(encounter);
            var m2 = 1;
            if (m > 1)
            {
                m2 = m - 1;
            }

            var re_list = new EncounterTable();
            re_list.AddEncounter(2, 1000)
                .AddEnemies(14069, m, m)
                .AddEnemies(14070, 2 * m, 5 * m, 0.5f); // bandits
            re_list.AddEncounter(3, 1001)
                .AddEnemies(14052, 1 * m, 3 * m); // black bears
            re_list.AddEncounter(1, 1002)
                .AddEnemies(14188, 1 * m, 3 * m, 0.3f)
                .AddEnemies(14184, 1 * m, 6 * m, 0.2f); // hobgoblins and goblins
            re_list.AddEncounter(2, 1003)
                .AddEnemies(14188, 3 * m, 6 * m); // hobgoblins
            re_list.AddEncounter(3, 1004)
                .AddEnemies(14188, 1 * m, 3 * m)
                .AddEnemies(14050, 1 * m, 3 * m); // hobgoblins and wolves
            re_list.AddEncounter(1, 1005)
                .AddEnemies(14448, 2 * m, 4 * m, 1); // ogres
            re_list.AddEncounter(3, 1006)
                .AddEnemies(14046, 1 * m, 3 * m, 1); // owlbears
            re_list.AddEncounter(0, 1007)
                .AddEnemies(14047, 2 * m, 4 * m, 1); // large spiders
            re_list.AddEncounter(2, 1008)
                .AddEnemies(14182, 3 * m, 6 * m); // stirges
            re_list.AddEncounter(0, 1009)
                .AddEnemies(14089, 2 * m, 4 * m, 1); // giant ticks
            re_list.AddEncounter(2, 1010)
                .AddEnemies(14050, 2 * m, 3 * m); // wolves
            // Higher Level Encounters
            re_list.AddEncounter(5, 1011)
                .AddEnemies(14053, 1 * m, 3 * m); // brown bears
            re_list.AddEncounter(5, 1012)
                .AddEnemies(14243, 1 * m, 3 * m); // harpies
            if (GetGlobalFlag(500))
            {
                re_list.AddEncounter(11, 1012)
                    .AddEnemies(14898, 2 * m2, 4 * m2, 0.5f)
                    .AddEnemies(14897, 2 * m2, 4 * m2, 0.5f); // Leucrottas + Jackalweres
                re_list.AddEncounter(12, 1013)
                    .AddEnemies(14888, 3, 5, 0.5f)
                    .AddEnemies(14891, 0 * m, 1 * m, 0.5f)
                    .AddEnemies(14696, 0 * m2, 1 * m2, 0.5f)
                    .AddEnemies(14896, 0 * m2, 1 * m2, 0.5f)
                    .AddEnemies(14506, 0 * m2, 1 * m2, 0.5f)
                    .AddEnemies(14527, 0 * m2, 1 * m2, 0.5f)
                    .AddEnemies(14525, 0 * m2, 1 * m2, 0.5f); // Cult of the Siren + Random Thrall
                re_list.AddEncounter(10, 1014)
                    .AddEnemies(14542, 2 * m, 4 * m, 1); // Invisible Stalkers
                re_list.AddEncounter(12, 1015)
                    .AddEnemies(14248, 1, 1, 1)
                    .AddEnemies(14249, 3 * m, 5 * m); // Ogre chief and ogres
                if (PartyAlignment.IsEvil())
                {
                    re_list.AddEncounter(12, 1016)
                        .AddEnemies(14894, 2 * m, 3 * m, 0.5f)
                        .AddEnemies(14896, 2 * m, 4 * m, 1)
                        .AddEnemies(14895, 3 * m, 5 * m); // Holy Rollers
                }
            }

            re_list.Pick(encounter);
        }

        public static void get_forest_nighttime(RandomEncounter encounter)
        {
            var m = Get_Multiplier(encounter);
            var m2 = 1;
            if (m > 1)
            {
                m2 = m - 1;
            }

            var re_list = new EncounterTable();
            re_list.AddEncounter(1, 1009)
                .AddEnemies(14188, 1 * m, 3 * m, 0.3f)
                .AddEnemies(14184, 1 * m, 6 * m, 0.2f); // hobgoblins and goblins
            re_list.AddEncounter(2, 1010)
                .AddEnemies(14188, 3 * m, 6 * m); // hobgoblins
            re_list.AddEncounter(3, 1011)
                .AddEnemies(14188, 1 * m, 3 * m)
                .AddEnemies(14050, 1 * m, 3 * m); // hobgoblins and wolves
            re_list.AddEncounter(2, 1012)
                .AddEnemies(14182, 3 * m, 6 * m); // stirges
            re_list.AddEncounter(1, 1013)
                .AddEnemies(14092, 1 * m, 6 * m, 0.3f); // zombies
            // higher level encounters
            re_list.AddEncounter(6, 1014)
                .AddEnemies(14291, 2 * m, 3 * m, 1); // Will o' wisps
            if (GetGlobalFlag(500))
            {
                re_list.AddEncounter(11, 1015)
                    .AddEnemies(14898, 2 * m2, 4 * m2, 0.5f)
                    .AddEnemies(14897, 2 * m2, 4 * m2, 0.5f); // Leucrottas + Jackalweres
                re_list.AddEncounter(10, 1016)
                    .AddEnemies(14137, 2 * m, 4 * m)
                    .AddEnemies(14674, 2, 3, 1)
                    .AddEnemies(14280, 1, 2, 1); // mohrgs and groaning spirits and ghasts
                re_list.AddEncounter(12, 1017)
                    .AddEnemies(14248, 1, 1, 1)
                    .AddEnemies(14249, 3 * m, 5 * m); // Ogre chief and ogres
                re_list.AddEncounter(9, 1018)
                    .AddEnemies(14542, 2 * m, 4 * m, 1); // Invisible Stalkers
                re_list.AddEncounter(14, 1019)
                    .AddEnemies(14958, 1, 1, 1)
                    .AddEnemies(14893, 2 * m, 4 * m, 0.5f); // Nightwalker and Greater Shadows
            }

            re_list.Pick(encounter);
        }

        public static void get_swamp_daytime(RandomEncounter encounter)
        {
            var m = Get_Multiplier(encounter);
            var m2 = 1;
            if (m > 1)
            {
                m2 = m - 1;
            }

            var re_list = new EncounterTable();
            re_list.AddEncounter(2, 1013)
                .AddEnemies(14182, 3 * m, 6 * m); // stirges
            re_list.AddEncounter(0, 1014)
                .AddEnemies(14089, 2 * m, 4 * m, 1); // giant ticks
            re_list.AddEncounter(2, 1015)
                .AddEnemies(14094, 1 * m, 2 * m, 1); // crayfish
            re_list.AddEncounter(2, 1016)
                .AddEnemies(14057, 1 * m, 2 * m); // frogs
            re_list.AddEncounter(0, 1017)
                .AddEnemies(14090, 2 * m, 4 * m, 1); // lizards
            re_list.AddEncounter(2, 1018)
                .AddEnemies(14084, 2 * m, 3 * m); // lizardmen
            re_list.AddEncounter(4, 1019)
                .AddEnemies(14084, 1 * m, 3 * m).AddEnemies(14090, 1 * m, 1 * m); // lizardmen with lizard
            re_list.AddEncounter(1, 1020)
                .AddEnemies(14056, 4 * m, 9 * m, 0.144f); // rats
            re_list.AddEncounter(3, 1021)
                .AddEnemies(14630, 1 * m, 3 * m, 0.5f); // snakes
            // Higher Level Encounters
            re_list.AddEncounter(4, 1022)
                .AddEnemies(14262, 1 * m, 4 * m, 1); // trolls
            if (GetGlobalFlag(500))
            {
                re_list.AddEncounter(12, 1023)
                    .AddEnemies(14892, 1 * m, 2 * m, 0.5f)
                    .AddEnemies(14891, 2 * m, 4 * m, 0.5f)
                    .AddEnemies(14890, 2 * m2, 3 * m2, 0.5f)
                    .AddEnemies(14889, 1 * m2, 2 * m2, 0.5f); // Lizardman battlegroup
                re_list.AddEncounter(11, 1024)
                    .AddEnemies(14892, 1, 2, 0.5f)
                    .AddEnemies(14891, 2, 4, 0.5f)
                    .AddEnemies(14890, 2 * m2, 3 * m2, 0.5f)
                    .AddEnemies(14889, 1 * m2, 2 * m2, 0.5f)
                    .AddEnemies(14343, 1 * m2, 2 * m2)
                    .AddEnemies(14090, 1 * m2, 2 * m2); // Lizardman battlegroup + lizards + hydras
                re_list.AddEncounter(9, 1025)
                    .AddEnemies(14343, 1 * m, 2 * m, 1); // Hydras
                re_list.AddEncounter(12, 1026)
                    .AddEnemies(14261, 1 * m, 4 * m, 1); // Vodyanoi
                re_list.AddEncounter(9, 1027)
                    .AddEnemies(14279, 2, 3, 1)
                    .AddEnemies(14375, 2, 4); // Seahags and watersnakes
            }

            re_list.Pick(encounter);
        }

        public static void get_swamp_nighttime(RandomEncounter encounter)
        {
            var m = Get_Multiplier(encounter);
            var m2 = 1;
            if (m > 1)
            {
                m2 = m - 1;
            }

            var re_list = new EncounterTable();
            re_list.AddEncounter(2, 1001).AddEnemies(14052, 2 * m, 5 * m, 0.5f); // black bears
            re_list.AddEncounter(2, 1002).AddEnemies(14182, 3 * m, 6 * m); // stirges
            // higher level encounters
            re_list.AddEncounter(5, 1003).AddEnemies(14291, 1 * m, 4 * m, 1); // willowisps
            re_list.AddEncounter(4, 1004).AddEnemies(14262, 1 * m, 4 * m, 1); // trolls
            re_list.AddEncounter(8, 1005).AddEnemies(14280, 1 * m, 1 * m); // groaning spirit
            re_list.AddEncounter(4, 1006).AddEnemies(14128, 1 * m, 3 * m, 0.5f); // ghouls
            re_list.AddEncounter(5, 1007).AddEnemies(14135, 1 * m, 1 * m)
                .AddEnemies(14128, 2 * m, 4 * m); // ghasts and ghouls
            if (GetGlobalFlag(500))
            {
                re_list.AddEncounter(12, 1008)
                    .AddEnemies(14892, 1 * m2, 2 * m2, 0.5f)
                    .AddEnemies(14891, 2 * m, 4 * m, 0.5f)
                    .AddEnemies(14890, 2 * m2, 3 * m2, 0.5f)
                    .AddEnemies(14889, 1 * m2, 2 * m2, 0.5f); // Lizardman battlegroup
                re_list.AddEncounter(12, 1009)
                    .AddEnemies(14892, 1, 2, 0.5f)
                    .AddEnemies(14891, 2, 4, 0.5f)
                    .AddEnemies(14890, 2 * m2, 3 * m2, 0.5f)
                    .AddEnemies(14889, 1 * m2, 2 * m2, 0.5f)
                    .AddEnemies(14343, 1 * m2, 2 * m2)
                    .AddEnemies(14090, 1 * m2, 2 * m2); // Lizardman battlegroup + lizards + hydras
                re_list.AddEncounter(14, 1010)
                    .AddEnemies(14958, 1, 1, 1)
                    .AddEnemies(14893, 2 * m, 4 * m, 1); // Nightwalker and Greater Shadows
                re_list.AddEncounter(9, 1011)
                    .AddEnemies(14343, 1 * m, 2 * m, 1); // Hydras
                re_list.AddEncounter(12, 1012)
                    .AddEnemies(14261, 1 * m, 4 * m, 1); // Vodyanoi
                re_list.AddEncounter(9, 1013)
                    .AddEnemies(14279, 1 * m, 3 * m, 1)
                    .AddEnemies(14375, 1 * m, 3 * m); // Seahags and watersnakes
                re_list.AddEncounter(9, 1014)
                    .AddEnemies(14824, 1 * m, 3 * m, 1)
                    .AddEnemies(14825, 1 * m, 3 * m, 1); // Ettin & Hill giant zombies
            }

            re_list.Pick(encounter);
        }

        public static void get_riverside_daytime(RandomEncounter encounter)
        {
            var m = Get_Multiplier(encounter);
            var m2 = 1;
            if (m > 1)
            {
                m2 = m - 1;
            }

            var re_list = new EncounterTable();
            re_list.AddEncounter(2, 1002)
                .AddEnemies(14094, 1 * m, 2 * m, 1); // crayfish
            re_list.AddEncounter(0, 1003)
                .AddEnemies(14090, 2 * m, 4 * m, 1); // lizards
            re_list.AddEncounter(2, 1004)
                .AddEnemies(14084, 2 * m, 3 * m); // lizardmen
            re_list.AddEncounter(4, 1005)
                .AddEnemies(14084, 1 * m, 3 * m)
                .AddEnemies(14090, 1 * m, 1 * m); // lizardmen with lizard
            re_list.AddEncounter(1, 1006)
                .AddEnemies(14290, 2 * m, 5 * m, 0.5f); // pirates
            // higher level encounters
            re_list.AddEncounter(4, 1007)
                .AddEnemies(14262, 1 * m, 4 * m, 1); // trolls
            if (GetGlobalFlag(500))
            {
                re_list.AddEncounter(12, 1008)
                    .AddEnemies(14892, 1 * m, 2 * m, 0.5f)
                    .AddEnemies(14891, 2 * m, 4 * m, 0.5f)
                    .AddEnemies(14890, 2 * m2, 3 * m2, 0.5f)
                    .AddEnemies(14889, 1 * m2, 2 * m2, 0.5f); // Lizardman battlegroup
                re_list.AddEncounter(12, 1009)
                    .AddEnemies(14888, 3, 5, 0.5f)
                    .AddEnemies(14891, 0 * m, 1 * m, 0.5f)
                    .AddEnemies(14696, 0 * m2, 1 * m2, 0.5f)
                    .AddEnemies(14896, 0 * m2, 1 * m2, 0.5f)
                    .AddEnemies(14506, 0 * m2, 1 * m2, 0.5f)
                    .AddEnemies(14527, 0 * m2, 1 * m2, 0.5f)
                    .AddEnemies(14525, 0 * m2, 1 * m2, 0.5f); // Cult of the Siren + Random Thrall
                re_list.AddEncounter(12, 1010)
                    .AddEnemies(14892, 1 * m, 2 * m, 0.5f)
                    .AddEnemies(14891, 2 * m, 4 * m, 0.5f)
                    .AddEnemies(14890, 2 * m2, 3 * m2, 0.5f)
                    .AddEnemies(14889, 1 * m2, 2 * m2, 0.5f)
                    .AddEnemies(14279, 1 * m, 3 * m); // Lizardman battlegroup + seahag
                re_list.AddEncounter(12, 1011)
                    .AddEnemies(14261, 1 * m, 4 * m, 1); // Vodyanoi
                re_list.AddEncounter(10, 1012)
                    .AddEnemies(14240, 1 * m, 3 * m)
                    .AddEnemies(14279, 1 * m2, 3 * m2, 1)
                    .AddEnemies(14375, 1 * m, 3 * m); // Seahags and watersnakes and kapoacinths
                if (PartyAlignment.IsEvil())
                {
                    re_list.AddEncounter(12, 1013)
                        .AddEnemies(14894, 2 * m2, 3 * m2, 0.5f)
                        .AddEnemies(14896, 2 * m2, 4 * m2, 1)
                        .AddEnemies(14895, 3 * m2, 5 * m2); // Holy Rollers
                }
            }

            re_list.Pick(encounter);
        }

        public static void get_riverside_nighttime(RandomEncounter encounter)
        {
            var m = Get_Multiplier(encounter);
            var m2 = 1;
            if (m > 1)
            {
                m2 = m - 1;
            }

            var re_list = new EncounterTable();
            re_list.AddEncounter(1, 1007).AddEnemies(14130, 1 * m, 3 * m, 0.5f); // lacedons
            re_list.AddEncounter(1, 1008).AddEnemies(14081, 2 * m, 4 * m); // skeleton gnolls
            re_list.AddEncounter(1, 1009).AddEnemies(14107, 2 * m, 4 * m); // skeletons
            // Higher level encounters
            re_list.AddEncounter(4, 1010).AddEnemies(14262, 1 * m, 4 * m, 1); // trolls
            if (GetGlobalFlag(500))
            {
                re_list.AddEncounter(12, 1011)
                    .AddEnemies(14892, 1 * m, 2 * m, 0.5f)
                    .AddEnemies(14891, 2 * m, 4 * m, 0.5f)
                    .AddEnemies(14890, 2 * m2, 3 * m2, 0.5f)
                    .AddEnemies(14889, 1 * m2, 2 * m2, 0.5f); // Lizardman battlegroup
                re_list.AddEncounter(12, 1012)
                    .AddEnemies(14892, 1 * m, 2 * m, 0.5f)
                    .AddEnemies(14891, 2 * m, 4 * m, 0.5f)
                    .AddEnemies(14890, 2 * m2, 3 * m2, 0.5f)
                    .AddEnemies(14889, 1 * m2, 2 * m2, 0.5f)
                    .AddEnemies(14279, 1 * m, 3 * m); // Lizardman battlegroup + seahag
                re_list.AddEncounter(12, 1013)
                    .AddEnemies(14261, 1 * m, 4 * m, 1); // Vodyanoi
                re_list.AddEncounter(9, 1014)
                    .AddEnemies(14240, 1 * m, 3 * m)
                    .AddEnemies(14279, 1 * m, 3 * m, 1)
                    .AddEnemies(14375, 1 * m, 3 * m); // Seahags and watersnakes and kapoacinths
            }

            re_list.Pick(encounter);
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

                if ((PartyLeader.HasReputation(92) || PartyLeader.HasReputation(29) || PartyLeader.HasReputation(30) ||
                     PartyLeader.HasReputation(32)) && PartyLeader.GetMap() != 5014)
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
                else if ((((SelectedPartyLeader.GetMap() == 5060) || (SelectedPartyLeader.GetMap() == 5061)) &&
                          (GetGlobalFlag(289))))
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
            else if ((((SelectedPartyLeader.GetMap() >= 5121) && (SelectedPartyLeader.GetMap() <= 5126)) ||
                      ((SelectedPartyLeader.GetMap() >= 5133) && (SelectedPartyLeader.GetMap() <= 5140)) ||
                      (SelectedPartyLeader.GetMap() == 5142) ||
                      ((SelectedPartyLeader.GetMap() >= 5148) && (SelectedPartyLeader.GetMap() <= 5150)) ||
                      ((SelectedPartyLeader.GetMap() >= 5153) && (SelectedPartyLeader.GetMap() <= 5173)) ||
                      ((SelectedPartyLeader.GetMap() >= 5175) && (SelectedPartyLeader.GetMap() <= 5188))))
            {
                // Verbobonc maps
                return SleepStatus.PassTimeOnly;
            }
            else if (((SelectedPartyLeader.GetMap() == 5143) || (SelectedPartyLeader.GetMap() == 5144) ||
                      (SelectedPartyLeader.GetMap() == 5145) || (SelectedPartyLeader.GetMap() == 5146) ||
                      (SelectedPartyLeader.GetMap() == 5147)))
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
            else if (SelectedPartyLeader.GetMap() == 5116 || SelectedPartyLeader.GetMap() == 5118
            ) // Tutorial maps 1 & 3
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

        public static bool Survival_Check(RandomEncounter encounter)
        {
            if (encounter.DC < 1000)
            {
                var PC_roll = RandomRange(1, 20);
                var NPC_roll = RandomRange(1, 20);
                var PC_mod = PC_Modifier();
                var NPC_mod = NPC_Modifier(encounter);
                Logger.Info("{0}",
                    "PC roll: " + PC_roll + " + " + (PC_mod / 3) + " vs  NPC roll: " +
                    NPC_roll + " + " + (NPC_mod / 3));
                if (PC_roll + (PC_mod / 3) >= NPC_roll + (NPC_mod / 3))
                {
                    encounter.DC = 1;
                }
                else
                {
                    encounter.DC = 1000;
                }

                SetGlobalVar(35, NPC_roll + NPC_mod / 3 - (PC_roll + PC_mod / 3));
            }

            return true;
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
                Logger.Info("{0}",
                    "NPC " + i + " , Listen = " + listen + " , Spot = " + spot +
                    " , Survival = " + wild);
                level = spot + listen + wild;
                if (level > high)
                {
                    high = level;
                }

                obj.Destroy();
            }

            Logger.Info("{0}", "Highest NPC result was: " + high);
            return high;
        }

        public static locXY Spawn_Point(RandomEncounter encounter)
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
                    Logger.Info("{0}",
                        "Reducing distance to 8 for encounter because it is at the edges. Old distance was " +
                        distance);
                    distance = 8;
                }
            }

            Logger.Info("Distance = {0}", distance);
            var p_list = get_circle_point_list(PartyLeader.GetLocation(), distance, 16);
            return p_list[RandomRange(0, p_list.Count - 1)];
        }

        public static locXY random_location(locXY loc, int range, GameObjectBody target)
        {
            Logger.Info("Generating Location");
            var (x, y) = loc;
            Logger.Info("{0}", "Target: " + target);
            var (t_x, t_y) = target.GetLocation();
            var rand_x = RandomRange(1, 3);
            var rand_y = RandomRange(1, 3);
            var loc_x = t_x;
            var loc_y = t_y;
            while (MathF.Sqrt(MathF.Pow((loc_x - t_x), 2) + MathF.Pow((loc_y - t_y), 2)) <
                   MathF.Sqrt(MathF.Pow((x - t_x), 2) + MathF.Pow((y - t_y), 2)))
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
            Logger.Info("Location: {0} {1}", loc_x, loc_y);
            return location;
        }

        public static int group_skill_level(GameObjectBody pc, SkillId skill)
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

        public static List<locXY> get_circle_point_list(locXY center, int radius, int num_points)
        {
            // def get_circle_point_list(center, radius,num_points): # By Darmagon
            var p_list = new List<locXY>();
            var (offx, offy) = center;
            var i = 0f;
            while (i < 2 * MathF.PI)
            {
                var posx = (int) (MathF.Cos(i) * radius) + offx;
                var posy = (int) (MathF.Sin(i) * radius) + offy;
                var loc = new locXY(posx, posy);
                p_list.Add(loc);
                i = i + MathF.PI / (num_points / 2);
            }

            return p_list;
        }
        // By Livonya

        public static void Slaughtered_Caravan()
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
        }
        // By Livonya

        public static void NPC_Self_Buff()
        {
            // def NPC_Self_Buff(): # By Livonya
            // THIS WAS ADDED TO AID IN NPC SELF BUFFING			##
            // ##
            for (var i = 712; i < 733; i++)
            {
                Logger.Info("{0}", i);
                SetGlobalVar(i, 0);
            }
        }
    }
}