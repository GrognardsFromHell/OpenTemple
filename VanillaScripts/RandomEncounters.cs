
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{

    public class RandomEncounters
    {
        
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public static bool encounter_exists(RandomEncounterQuery setup, RandomEncounter encounter)
        {
            Logger.Info("Testing encounter_exists");
            if (((setup.Type == RandomEncounterType.Resting)))
            {
                return check_sleep_encounter(setup, encounter);

            }
            else
            {
                return check_random_encounter(setup, encounter);
            }
        }
        public static void encounter_create(RandomEncounter encounter)
        {
            Logger.Info("Testing encounter_create with id={0}", encounter.Id);
            var is_sleep_encounter = false;

            LocAndOffsets location;

            if ((encounter.Id >= 4000))
            {
                location = SelectedPartyLeader.GetLocationFull();
                is_sleep_encounter = true;
            }
            else
            {
                location = new LocAndOffsets(encounter.Location);
            }

            var i = 0;

            var total = encounter.Enemies.Count;

            while ((i < total))
            {
                var j = 0;

                while ((j < encounter.Enemies[i].Count))
                {
                    if (is_sleep_encounter)
                    {
                        var party = new List<GameObjectBody>();

                        foreach (var pc in GameSystems.Party.PartyMembers)
                        {
                            if (!pc.IsUnconscious())
                            {
                                party.Add(pc);
                            }

                        }

                        if (party.Count > 0)
                        {
                            location = party[i % party.Count].GetLocationFull();
                        }

                    }

                    var npc = GameSystems.MapObject.CreateObject(encounter.Enemies[i].ProtoId, location);

                    if ((npc != null))
                    {
                        npc.TurnTowards(SelectedPartyLeader);
                        if (((encounter.Id < 2000) || (encounter.Id >= 4000)))
                        {
                            npc.Attack(SelectedPartyLeader);
                            npc.SetNpcFlag(NpcFlag.KOS);
                        }

                    }

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
                    return true;
                }
                else if ((check_unrepeatable_encounter(setup, encounter)))
                {
                    return true;
                }
                else
                {
                    return check_repeatable_encounter(setup, encounter);
                }

            }

            return false;
        }
        public static bool check_sleep_encounter(RandomEncounterQuery setup, RandomEncounter encounter)
        {
            if ((RandomRange(1, 10) == 1))
            {
                encounter.Id = 4000;

                if ((SelectedPartyLeader.GetArea() == 1))
                {
                    return false;
                }
                else if ((SelectedPartyLeader.GetArea() == 2))
                {
                    if ((SelectedPartyLeader.GetMap() == 5002))
                    {
                        var enemy_list = new[] {(14057, 1, 3, 1), (14089, 2, 4, 1), (14291, 1, 4, 6), (14050, 2, 3, 1), (14094, 1, 2, 3), (14090, 2, 4, 1), (14056, 4, 9, 1), (14088, 1, 3, 1), (14047, 2, 4, 1), (14070, 2, 5, 1)};

                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5004))
                    {
                        var enemy_list = new[] {(14056, 4, 9, 1), (14089, 2, 4, 1), (14090, 2, 4, 1), (14088, 1, 3, 1), (14070, 2, 5, 1)};

                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5005))
                    {
                        var enemy_list = new[] {(14056, 4, 9, 1), (14090, 2, 4, 1), (14092, 1, 3, 1), (14093, 1, 3, 2), (14067, 1, 3, 1), (14074, 2, 4, 1)};

                        var x = get_sleep_encounter_enemies(enemy_list, encounter);

                        if (x)
                        {
                            if ((encounter.Enemies[0].ProtoId == 14067))
                            {
                                if ((GetGlobalFlag(288)))
                                {
                                    return false;
                                }

                            }
                            else if ((encounter.Enemies[0].ProtoId == 14074))
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
                else if ((SelectedPartyLeader.GetArea() == 3))
                {
                    return false;
                }
                else if ((SelectedPartyLeader.GetArea() == 4))
                {
                    if ((SelectedPartyLeader.GetMap() == 5062))
                    {
                        var enemy_list = new[] {(14070, 2, 5, 1), (14275, 1, 1, 4), (14056, 4, 9, 1), (14088, 1, 3, 1), (14047, 2, 4, 1)};

                        var x = get_sleep_encounter_enemies(enemy_list, encounter);

                        if (x)
                        {
                            if ((encounter.Enemies[0].ProtoId == 14275))
                            {
                                if ((Utilities.is_daytime()))
                                {
                                    return false;
                                }

                            }

                        }

                        return x;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5064))
                    {
                        var enemy_list = new[] {(14070, 2, 5, 1), (14275, 1, 1, 4), (14056, 4, 9, 1), (14170, 2, 5, 2)};

                        var x = get_sleep_encounter_enemies(enemy_list, encounter);

                        if (x)
                        {
                            if ((encounter.Enemies[0].ProtoId == 14275))
                            {
                                if ((Utilities.is_daytime()))
                                {
                                    return false;
                                }

                            }

                        }

                        return x;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5065))
                    {
                        var enemy_list = new[] {(14070, 2, 5, 1), (14170, 2, 5, 2)};

                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5066))
                    {
                        var enemy_list = new[] {(14070, 2, 5, 1), (14078, 2, 5, 1), (14128, 2, 5, 1), (14139, 1, 1, 3), (14140, 1, 1, 4), (14448, 1, 1, 2), (14170, 2, 5, 2)};

                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5067))
                    {
                        var enemy_list = new[] {(14070, 2, 5, 1), (14170, 4, 6, 2), (14190, 1, 1, 4), (14142, 1, 1, 5), (14448, 2, 4, 2), (14262, 1, 2, 5)};

                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5078))
                    {
                        var enemy_list = new[] {(14143, 1, 1, 7), (14238, 1, 2, 5), (14239, 5, 8, 4), (14221, 2, 3, 7), (14448, 5, 8, 2), (14262, 2, 3, 5)};

                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5079))
                    {
                        return false;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5080))
                    {
                        var enemy_list = new[] {(14143, 1, 1, 7), (14238, 1, 1, 5), (14262, 1, 2, 5), (14239, 3, 6, 4), (14220, 1, 2, 7), (14448, 1, 4, 3)};

                        var x = get_sleep_encounter_enemies(enemy_list, encounter);

                        if (x)
                        {
                            if ((encounter.Enemies[0].ProtoId == 14448))
                            {
                                encounter.AddEnemies(14174, RandomRange(2, 5));
                            }

                        }

                        return x;
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5081))
                    {
                        var enemy_list = new[] {(14292, 1, 2, 5), (14192, 1, 2, 4), (14068, 1, 4, 2), (14293, 1, 2, 2), (14294, 1, 1, 4)};

                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5082))
                    {
                        var enemy_list = new[] {(14295, 1, 1, 5), (14191, 1, 4, 4), (14141, 1, 1, 4), (14296, 1, 2, 5)};

                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5083))
                    {
                        var enemy_list = new[] {(14297, 2, 5, 2), (14298, 1, 2, 5), (14299, 1, 2, 1), (14300, 1, 2, 3)};

                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }
                    else if ((SelectedPartyLeader.GetMap() == 5084))
                    {
                        var enemy_list = new[] {(14301, 1, 1, 1), (14109, 1, 1, 3), (14084, 2, 4, 1), (14261, 1, 1, 7), (14302, 1, 2, 5), (14240, 2, 3, 4), (14132, 3, 5, 1), (14108, 3, 5, 2)};

                        return get_sleep_encounter_enemies(enemy_list, encounter);
                    }

                    return false;
                }
                else
                {
                    var party_level = Utilities.group_average_level(SelectedPartyLeader);

                    get_repeatable_encounter_enemies(setup, encounter);
                    while ((encounter.DC > (party_level + 2)))
                    {
                        get_repeatable_encounter_enemies(setup, encounter);
                    }

                    encounter.Id = 4000;

                    return true;
                }

            }

            return false;
        }
        public static bool get_sleep_encounter_enemies(ValueTuple<int, int, int, int>[] enemy_list, RandomEncounter encounter)
        {
            var total = enemy_list.Length;

            var n = RandomRange(0, total - 1);

            encounter.DC = enemy_list[n].Item4;

            var party_level = Utilities.group_average_level(SelectedPartyLeader);

            if ((encounter.DC > (party_level + 2)))
            {
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

                    encounter.DC = 1000;

                    encounter.Map = get_map_from_terrain(setup.Terrain);

                    if ((id == 3000))
                    {
                        encounter.AddEnemies(14303, 1);
                    }
                    else if ((id == 3001))
                    {
                        encounter.AddEnemies(14307, 1);
                        encounter.AddEnemies(14308, 10);
                    }
                    else if ((id == 3002))
                    {
                        encounter.AddEnemies(14305, 1);
                        encounter.AddEnemies(14306, 6);
                    }
                    else if ((id == 3003))
                    {
                        encounter.AddEnemies(14304, 1);
                    }
                    else if ((id == 3004))
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
            if ((RandomRange(1, 10) == 1))
            {
                var id = RandomRange(2000, 2002);

                if ((!GetGlobalFlag(id - 2000 + 227)))
                {
                    encounter.Id = id;

                    encounter.Map = get_map_from_terrain(setup.Terrain);

                    if ((id == 2000))
                    {
                        encounter.DC = 9;

                        encounter.AddEnemies(14142, 4);
                    }
                    else if ((id == 2001))
                    {
                        encounter.DC = 5;

                        encounter.AddEnemies(14331, 1);
                    }
                    else if ((id == 2002))
                    {
                        encounter.DC = 9;

                        encounter.AddEnemies(14332, 1);
                        encounter.AddEnemies(14333, 1);
                        encounter.AddEnemies(14334, 1);
                        encounter.AddEnemies(14335, 1);
                        encounter.AddEnemies(14336, 1);
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

            var party_level = Utilities.group_average_level(SelectedPartyLeader);

            get_repeatable_encounter_enemies(setup, encounter);
            while ((encounter.DC > (party_level + 2)))
            {
                get_repeatable_encounter_enemies(setup, encounter);
            }

            return true;
        }
        public static int get_map_from_terrain(MapTerrain terrain)
        {
            int map; // DECL_PULL_UP
            if (((terrain & MapTerrain.Scrub)) != 0)
            {
                map = 5070;

            }
            else if (((terrain & MapTerrain.Forest)) != 0)
            {
                map = 5071;

            }
            else if (((terrain & MapTerrain.Swamp)) != 0)
            {
                map = 5072;

            }
            else
            {
                map = 5073;

            }

            if (((terrain & MapTerrain.RoadFlag)) != 0)
            {
                map = map + 4;

            }

            return map;
        }
        public static void get_repeatable_encounter_enemies(RandomEncounterQuery setup, RandomEncounter encounter)
        {
            if (((setup.Terrain & MapTerrain.Scrub)) != 0)
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
            else if (((setup.Terrain & MapTerrain.Forest)) != 0)
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
            else if (((setup.Terrain & MapTerrain.Swamp)) != 0)
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
        public static void get_scrub_daytime(RandomEncounter encounter)
        {
            var r = RandomRange(1, 10);

            if ((r == 1))
            {
                var n = RandomRange(2, 5);

                encounter.DC = 2 + (n / 2);

                encounter.AddEnemies(14069, 1);
                encounter.AddEnemies(14070, n);
                encounter.Id = 1000;

            }
            else if ((r == 2))
            {
                var n = RandomRange(3, 6);

                encounter.DC = 6;

                encounter.AddEnemies(14238, 1);
                encounter.AddEnemies(14188, n);
                encounter.Id = 1039;

            }
            else if ((r == 3))
            {
                var n = RandomRange(1, 2);

                encounter.DC = 7;

                encounter.AddEnemies(14238, 1);
                encounter.AddEnemies(14053, n);
                encounter.Id = 1040;

            }
            else if ((r == 4))
            {
                var n = RandomRange(1, 4);

                encounter.DC = 4 + n;

                encounter.AddEnemies(14069, n);
                encounter.Id = 1041;

            }
            else if ((r == 5))
            {
                var n = RandomRange(2, 4);

                encounter.DC = 3;

                encounter.AddEnemies(14067, n);
                encounter.Id = 1018;

            }
            else if ((r == 6))
            {
                var n = RandomRange(1, 3);

                var n2 = RandomRange(1, 3);

                encounter.DC = 4;

                encounter.AddEnemies(14067, n);
                encounter.AddEnemies(14050, n2);
                encounter.Id = 1019;

            }
            else if ((r == 7))
            {
                var n = RandomRange(3, 6);

                encounter.DC = 1;

                encounter.AddEnemies(14184, n);
                encounter.Id = 1003;

            }
            else if ((r == 8))
            {
                var n = RandomRange(3, 6);

                var n2 = RandomRange(1, 3);

                encounter.DC = 3;

                encounter.AddEnemies(14184, n);
                encounter.AddEnemies(14050, n2);
                encounter.Id = 1004;

            }
            else if ((r == 9))
            {
                var n = RandomRange(1, 4);

                encounter.DC = 6 + n;

                encounter.AddEnemies(14217, n);
                encounter.Id = 1045;

            }
            else
            {
                var n = RandomRange(2, 4);

                encounter.DC = 1;

                encounter.AddEnemies(14051, n);
                encounter.Id = 1006;

            }

            return;
        }
        public static void get_scrub_nighttime(RandomEncounter encounter)
        {
            var r = RandomRange(1, 10);

            if ((r == 1))
            {
                var n = RandomRange(3, 6);

                encounter.DC = 3;

                encounter.AddEnemies(14093, 1);
                encounter.AddEnemies(14184, n);
                encounter.Id = 1026;

            }
            else if ((r == 2))
            {
                var n = RandomRange(4, 9);

                encounter.DC = 4;

                encounter.AddEnemies(14093, 1);
                encounter.AddEnemies(14188, n);
                encounter.Id = 1027;

            }
            else if ((r == 3))
            {
                var n = RandomRange(2, 4);

                encounter.DC = 1 + n;

                encounter.AddEnemies(14093, n);
                encounter.Id = 1028;

            }
            else if ((r == 4))
            {
                var n = RandomRange(2, 4);

                var n2 = RandomRange(2, 4);

                encounter.DC = 7;

                encounter.AddEnemies(14093, n);
                encounter.AddEnemies(14050, n2);
                encounter.Id = 1029;

            }
            else if ((r == 5))
            {
                var n = RandomRange(2, 4);

                encounter.DC = 3;

                encounter.AddEnemies(14067, n);
                encounter.Id = 1018;

            }
            else if ((r == 6))
            {
                var n = RandomRange(1, 3);

                var n2 = RandomRange(1, 3);

                encounter.DC = 4;

                encounter.AddEnemies(14067, n);
                encounter.AddEnemies(14050, n2);
                encounter.Id = 1019;

            }
            else if ((r == 7))
            {
                var n = RandomRange(3, 6);

                encounter.DC = 1;

                encounter.AddEnemies(14184, n);
                encounter.Id = 1003;

            }
            else if ((r == 8))
            {
                var n = RandomRange(3, 6);

                var n2 = RandomRange(1, 3);

                encounter.DC = 3;

                encounter.AddEnemies(14184, n);
                encounter.AddEnemies(14050, n2);
                encounter.Id = 1004;

            }
            else if ((r == 9))
            {
                var n = RandomRange(1, 3);

                encounter.DC = 1;

                encounter.AddEnemies(14092, n);
                encounter.Id = 1014;

            }
            else
            {
                var n = RandomRange(3, 6);

                encounter.DC = 2;

                encounter.AddEnemies(14092, n);
                encounter.Id = 1015;

            }

            return;
        }
        public static void get_forest_daytime(RandomEncounter encounter)
        {
            var r = RandomRange(1, 14);

            if ((r == 1))
            {
                var n = RandomRange(2, 5);

                encounter.DC = 2 + (n / 2);

                encounter.AddEnemies(14069, 1);
                encounter.AddEnemies(14070, n);
                encounter.Id = 1000;

            }
            else if ((r == 2))
            {
                var n = RandomRange(1, 3);

                encounter.DC = 3;

                encounter.AddEnemies(14052, n);
                encounter.Id = 1025;

            }
            else if ((r == 3))
            {
                var n = RandomRange(1, 3);

                encounter.DC = 5;

                encounter.AddEnemies(14053, n);
                encounter.Id = 1036;

            }
            else if ((r == 4))
            {
                var n = RandomRange(1, 3);

                encounter.DC = 5;

                encounter.AddEnemies(14243, n);
                encounter.Id = 1037;

            }
            else if ((r == 5))
            {
                var n = RandomRange(1, 3);

                encounter.DC = 1;

                encounter.AddEnemies(14188, 1);
                encounter.AddEnemies(14184, n);
                encounter.Id = 1009;

            }
            else if ((r == 6))
            {
                var n = RandomRange(3, 6);

                encounter.DC = 2;

                encounter.AddEnemies(14188, n);
                encounter.Id = 1010;

            }
            else if ((r == 7))
            {
                var n = RandomRange(1, 3);

                var n2 = RandomRange(3, 6);

                encounter.DC = 2;

                encounter.AddEnemies(14188, n);
                encounter.AddEnemies(14184, n2);
                encounter.Id = 1011;

            }
            else if ((r == 8))
            {
                var n = RandomRange(1, 3);

                var n2 = RandomRange(1, 3);

                encounter.DC = 3;

                encounter.AddEnemies(14188, n);
                encounter.AddEnemies(14050, n2);
                encounter.Id = 1012;

            }
            else if ((r == 9))
            {
                var n = RandomRange(2, 4);

                encounter.DC = n;

                encounter.AddEnemies(14448, n);
                encounter.Id = 1031;

            }
            else if ((r == 10))
            {
                var n = RandomRange(1, 3);

                encounter.DC = 3 + n;

                encounter.AddEnemies(14046, n);
                encounter.Id = 1038;

            }
            else if ((r == 11))
            {
                var n = RandomRange(2, 4);

                encounter.DC = n;

                encounter.AddEnemies(14047, n);
                encounter.Id = 1030;

            }
            else if ((r == 12))
            {
                var n = RandomRange(3, 6);

                encounter.DC = 2;

                encounter.AddEnemies(14182, n);
                encounter.Id = 1013;

            }
            else if ((r == 13))
            {
                var n = RandomRange(2, 4);

                encounter.DC = n;

                encounter.AddEnemies(14089, n);
                encounter.Id = 1024;

            }
            else
            {
                var n = RandomRange(2, 3);

                encounter.DC = 2;

                encounter.AddEnemies(14050, n);
                encounter.Id = 1022;

            }

            return;
        }
        public static void get_forest_nighttime(RandomEncounter encounter)
        {
            var r = RandomRange(1, 6);

            if ((r == 1))
            {
                var n = RandomRange(1, 3);

                encounter.DC = 1;

                encounter.AddEnemies(14188, 1);
                encounter.AddEnemies(14184, n);
                encounter.Id = 1009;

            }
            else if ((r == 2))
            {
                var n = RandomRange(3, 6);

                encounter.DC = 2;

                encounter.AddEnemies(14188, n);
                encounter.Id = 1010;

            }
            else if ((r == 3))
            {
                var n = RandomRange(1, 3);

                var n2 = RandomRange(3, 6);

                encounter.DC = 2;

                encounter.AddEnemies(14188, n);
                encounter.AddEnemies(14184, n2);
                encounter.Id = 1011;

            }
            else if ((r == 4))
            {
                var n = RandomRange(1, 3);

                var n2 = RandomRange(1, 3);

                encounter.DC = 3;

                encounter.AddEnemies(14188, n);
                encounter.AddEnemies(14050, n2);
                encounter.Id = 1012;

            }
            else if ((r == 5))
            {
                var n = RandomRange(3, 6);

                encounter.DC = 2;

                encounter.AddEnemies(14182, n);
                encounter.Id = 1013;

            }
            else
            {
                var n = RandomRange(3, 6);

                encounter.DC = 2;

                encounter.AddEnemies(14092, n);
                encounter.Id = 1015;

            }

            return;
        }
        public static void get_swamp_daytime(RandomEncounter encounter)
        {
            var r = RandomRange(1, 10);

            if ((r == 1))
            {
                var n = RandomRange(1, 2);

                encounter.DC = 2 + n;

                encounter.AddEnemies(14094, n);
                encounter.Id = 1032;

            }
            else if ((r == 2))
            {
                var n = RandomRange(1, 3);

                encounter.DC = 2;

                encounter.AddEnemies(14057, n);
                encounter.Id = 1016;

            }
            else if ((r == 3))
            {
                var n = RandomRange(2, 4);

                encounter.DC = n;

                encounter.AddEnemies(14090, n);
                encounter.Id = 1023;

            }
            else if ((r == 4))
            {
                var n = RandomRange(2, 3);

                encounter.DC = 2;

                encounter.AddEnemies(14084, n);
                encounter.Id = 1020;

            }
            else if ((r == 5))
            {
                var n = RandomRange(1, 3);

                encounter.DC = 4;

                encounter.AddEnemies(14084, n);
                encounter.AddEnemies(14090, 1);
                encounter.Id = 1021;

            }
            else if ((r == 6))
            {
                var n = RandomRange(4, 9);

                encounter.DC = 1;

                encounter.AddEnemies(14056, n);
                encounter.Id = 1002;

            }
            else if ((r == 7))
            {
                var n = RandomRange(1, 3);

                encounter.DC = 3 + (n / 2);

                encounter.AddEnemies(14088, n);
                encounter.Id = 1033;

            }
            else if ((r == 8))
            {
                var n = RandomRange(3, 6);

                encounter.DC = 2;

                encounter.AddEnemies(14182, n);
                encounter.Id = 1013;

            }
            else if ((r == 9))
            {
                var n = RandomRange(2, 4);

                encounter.DC = n;

                encounter.AddEnemies(14089, n);
                encounter.Id = 1024;

            }
            else
            {
                var n = RandomRange(1, 4);

                encounter.DC = 4 + n;

                encounter.AddEnemies(14262, n);
                encounter.Id = 1042;

            }

            return;
        }
        public static void get_swamp_nighttime(RandomEncounter encounter)
        {
            var r = RandomRange(1, 7);

            if ((r == 1))
            {
                var n = RandomRange(2, 4);

                encounter.DC = 5;

                encounter.AddEnemies(14135, 1);
                encounter.AddEnemies(14128, n);
                encounter.Id = 1034;

            }
            else if ((r == 2))
            {
                var n = RandomRange(1, 3);

                encounter.DC = 1 + (n / 2);

                encounter.AddEnemies(14128, n);
                encounter.Id = 1017;

            }
            else if ((r == 3))
            {
                encounter.DC = 7;

                encounter.AddEnemies(14280, 1);
                encounter.Id = 1044;

            }
            else if ((r == 4))
            {
                var n = RandomRange(2, 5);

                encounter.DC = 1 + (n / 2);

                encounter.AddEnemies(14289, n);
                encounter.Id = 1035;

            }
            else if ((r == 5))
            {
                var n = RandomRange(3, 6);

                encounter.DC = 2;

                encounter.AddEnemies(14182, n);
                encounter.Id = 1013;

            }
            else if ((r == 6))
            {
                var n = RandomRange(1, 4);

                encounter.DC = 4 + n;

                encounter.AddEnemies(14262, n);
                encounter.Id = 1042;

            }
            else
            {
                var n = RandomRange(1, 4);

                encounter.DC = 5 + n;

                encounter.AddEnemies(14291, n);
                encounter.Id = 1043;

            }

            return;
        }
        public static void get_riverside_daytime(RandomEncounter encounter)
        {
            var r = RandomRange(1, 6);

            if ((r == 1))
            {
                var n = RandomRange(1, 2);

                encounter.DC = 2 + n;

                encounter.AddEnemies(14094, n);
                encounter.Id = 1032;

            }
            else if ((r == 2))
            {
                var n = RandomRange(2, 4);

                encounter.DC = n;

                encounter.AddEnemies(14090, n);
                encounter.Id = 1023;

            }
            else if ((r == 3))
            {
                var n = RandomRange(2, 3);

                encounter.DC = 2;

                encounter.AddEnemies(14084, n);
                encounter.Id = 1020;

            }
            else if ((r == 4))
            {
                var n = RandomRange(1, 3);

                encounter.DC = 4;

                encounter.AddEnemies(14084, n);
                encounter.AddEnemies(14090, 1);
                encounter.Id = 1021;

            }
            else if ((r == 5))
            {
                var n = RandomRange(2, 5);

                encounter.DC = 1 + (n / 2);

                encounter.AddEnemies(14290, n);
                encounter.Id = 1001;

            }
            else
            {
                var n = RandomRange(1, 4);

                encounter.DC = 4 + n;

                encounter.AddEnemies(14262, n);
                encounter.Id = 1042;

            }

            return;
        }
        public static void get_riverside_nighttime(RandomEncounter encounter)
        {
            var r = RandomRange(1, 4);

            if ((r == 1))
            {
                var n = RandomRange(1, 3);

                encounter.DC = 1 + (n / 2);

                encounter.AddEnemies(14130, n);
                encounter.Id = 1017;

            }
            else if ((r == 2))
            {
                var n = RandomRange(2, 4);

                encounter.DC = 1;

                encounter.AddEnemies(14081, n);
                encounter.Id = 1007;

            }
            else if ((r == 3))
            {
                var n = RandomRange(2, 4);

                encounter.DC = 1;

                encounter.AddEnemies(14107, n);
                encounter.Id = 1008;

            }
            else
            {
                var n = RandomRange(1, 4);

                encounter.DC = 4 + n;

                encounter.AddEnemies(14262, n);
                encounter.Id = 1042;

            }

            return;
        }
        public static SleepStatus can_sleep()
        {
            if ((SelectedPartyLeader.GetArea() == 1))
            {
                if (((SelectedPartyLeader.GetMap() == 5007) || (SelectedPartyLeader.GetMap() == 5008)))
                {
                    if (((GetGlobalFlag(56)) || (GetQuestState(18) == QuestState.Completed)))
                    {
                        return SleepStatus.Safe;
                    }

                }

                return SleepStatus.PassTimeOnly;
            }
            else if ((SelectedPartyLeader.GetArea() == 2))
            {
                if ((SelectedPartyLeader.GetMap() == 5003))
                {
                    return SleepStatus.Safe;
                }

                return SleepStatus.Dangerous;
            }
            else if ((SelectedPartyLeader.GetArea() == 3))
            {
                if (((SelectedPartyLeader.GetMap() == 5085) && (GetGlobalFlag(94))))
                {
                    return SleepStatus.Safe;
                }
                else if ((((SelectedPartyLeader.GetMap() == 5060) || (SelectedPartyLeader.GetMap() == 5061)) && (GetGlobalFlag(289))))
                {
                    return SleepStatus.Safe;
                }

                return SleepStatus.PassTimeOnly;
            }
            else if ((SelectedPartyLeader.GetArea() == 4))
            {
                return SleepStatus.Dangerous;
            }
            else if (((SelectedPartyLeader.GetMap() >= 5096) && (SelectedPartyLeader.GetMap() <= 5104)))
            {
                return SleepStatus.Impossible;
            }
            else if (SelectedPartyLeader.GetMap() == 5116 || SelectedPartyLeader.GetMap() == 5118)
            {
                return SleepStatus.Impossible;
            }
            else if (SelectedPartyLeader.GetMap() == 5117)
            {
                return SleepStatus.Safe;
            }

            return SleepStatus.Dangerous;
        }


    }
}
