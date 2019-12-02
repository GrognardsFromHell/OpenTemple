
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(439)]
    public class ScriptDaemon : BaseObjectScript
    {

        private static readonly ILogger Logger = new ConsoleLogger();

        // Contained in this script
        // KOS monster on Temple Level 1

        private static readonly string TS_CRITTER_KILLED_FIRST_TIME = "504";
        // Robe-friendly monster on Temple Level 1

        private static readonly string TS_EARTH_CRITTER_KILLED_FIRST_TIME = "505";
        // Earth Temple human troop

        private static readonly string TS_EARTH_TROOP_KILLED_FIRST_TIME = "506";
        // Time when you crossed the threshold from killing a monster

        private static readonly string TS_CRITTER_THRESHOLD_CROSSED = "509";
        // Persistent flags/vars/strs		#
        // Uses keys starting with		#
        // 'Flaggg', 'Varrr', 'Stringgg' 	#

        public static readonly ISet<int> Money = new HashSet<int> {7000, 7001, 7002, 7003}
            .ToImmutableHashSet();

        public static readonly ISet<int> Gems = new HashSet<int> {12010, 12034..12045}
            .ToImmutableHashSet();

        public static readonly ISet<int> Jewels = new HashSet<int> {6180..6198}
            .ToImmutableHashSet();

        public static readonly ISet<int> Potions = new HashSet<int> {8006..8007}
            .ToImmutableHashSet();

        public static readonly ISet<int> AutoSellExclude = new HashSet<int>
        {
            4016, 4017, 4025, 4028, // Frag, Scath, Excal, Flam Swo +1
            4047, 4057, 4058, // Scimitar +1, Dagger +2, Dager +1
            4078, 4079, // Warha +1, +2
            4081..4087, // Longsword +1 ... +5, Unholy Orc ax+1
            4098, // Battleaxe +1
            4161, // Shortsword +2
            5802, // Figurine name IDs - as per protos.tab
            6015, 6017, 6031, 6039, 6058, 6073, 6214, 6215, 6219,
            6239, 12602,
            8006, 8007, 8008, 8101 // Potions of Cure mod, serious & Haste
            // 6015 - eye of flame cloak
            // 6017 - gnome ring
            // 6031 - eyeglasses
            // 6039 - Full Plate
            // 6048 - Prince Thrommel's Plate
            // 6058 - Cloak of Elvenkind
            // 6073 - Wooden Elvish Shield
            // 6214, 6215 - Green & Purple (resp.) Elven chain
            // 6219 - Senshock robes
            // 6239 - Darley's Necklace
            // 12602 - Hill Giant's Head
        };

        public static readonly ISet<int> AutoSell = new HashSet<int>
        {
            4002..4106, 4113..4120, 4155..4191, 6001..6048, 6055, 6056,
            6059, 6060, 6062..6073, 6074..6082, 6093, 6096, 6103, 6120, 6123, 6124,
            6142..6153, 6153..6159, 6163..6180, 6202..6239
        }.Except(AutoSellExclude).ToImmutableHashSet();

        public static bool get_f(string flagkey)
        {
            return Co8PersistentData.GetBool(flagkey);
        }
        public static void set_f(string flagkey, bool new_value = true)
        {
            Co8PersistentData.SetBool(flagkey, new_value);
        }

        public static int get_v(int varkey) => get_v(varkey.ToString());

        public static int get_v(string varkey)
        {
            return Co8PersistentData.GetInt(varkey);
        }

        public static int set_v(int varkey, int new_value) => set_v(varkey.ToString(), new_value);

        public static int set_v(string varkey, int new_value)
        {
            Co8PersistentData.SetInt(varkey, new_value);
            return get_v(varkey);
        }

        public static int inc_v(string varkey, int inc_amount = 1)
        {
            return set_v(varkey, get_v(varkey) + inc_amount);
        }

        public static string get_s(string strkey)
        {
            return Co8PersistentData.GetString(strkey);
        }
        public static void set_s(string strkey, string new_value)
        {
            Co8PersistentData.SetString(strkey, new_value);
        }
        // Bitwise NPC internal flags			#
        // 1-31									#
        // Uses obj_f_npc_pad_i_4			 	#
        // obj_f_pad_i_3 is sometimes nonzero    #
        // pad_i_4, pad_i_5 tested clean on all  #
        // protos								#

        public static void npc_set(GameObjectBody attachee, int flagno)
        {
            // flagno is assumed to be from 1 to 31
            var exponent = flagno - 1;
            var abc = 0;
            if (exponent > 30 || exponent < 0)
            {
                Logger.Info("error!");
            }
            else
            {
                abc = (1 << exponent);
            }

            var tempp = attachee.GetInt(obj_f.npc_pad_i_4) | abc;
            attachee.SetInt(obj_f.npc_pad_i_4, tempp);
            return;
        }
        public static void npc_unset(GameObjectBody attachee, int flagno)
        {
            // flagno is assumed to be from 1 to 31
            var exponent = flagno - 1;
            int abc = 0;
            if (exponent > 30 || exponent < 0)
            {
                Logger.Info("error!");
            }
            else
            {
                abc = (1 << exponent);
            }

            var tempp = attachee.GetInt(obj_f.npc_pad_i_4) & ~abc;
            attachee.SetInt(obj_f.npc_pad_i_4, tempp);
        }
        public static bool npc_get(GameObjectBody attachee, int flagno)
        {
            // flagno is assumed to be from 1 to 31
            var exponent = flagno - 1;
            int abc = 0;
            if (exponent > 30 || exponent < 0)
            {
                Logger.Info("error!");
            }
            else
            {
                abc = (1 << exponent);
            }

            return (attachee.GetInt(obj_f.npc_pad_i_4) & abc) != 0;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // in case the 'script bearer' dies, pass the curse to someone else
            var not_found = 1;
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if (pc.GetStat(Stat.hp_current) > 0 && not_found == 1 && pc.type == ObjectType.pc)
                {
                    not_found = 0;
                    attachee.RemoveScript(ObjScriptEvent.Dying);
                    attachee.RemoveScript(ObjScriptEvent.NewMap);
                    pc.SetScriptId(ObjScriptEvent.Dying, 439); // san_dying
                    pc.SetScriptId(ObjScriptEvent.NewMap, 439); // san_new_map
                    pc.SetScriptId(ObjScriptEvent.ExitCombat, 439); // san_exit_combat - executes when exiting combat mode
                    return SkipDefault;
                }
            }
            return SkipDefault;
        }
        public override bool OnExitCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (attachee.GetMap() == 5066) // temple level 1
            {
                GameObjectBody grate_obj = null;
                foreach (var door_candidate in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PORTAL))
                {
                    if ((door_candidate.GetNameId() == 120))
                    {
                        grate_obj = door_candidate;
                    }

                }

                if (!GameSystems.Combat.IsCombatActive())
                {
                    var harpies_alive = 0;
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                    {
                        if (obj.GetNameId() == 14243 && obj.GetLeader() == null && !obj.IsUnconscious() && obj.GetStat(Stat.hp_current) > -10)
                        {
                            harpies_alive += 1;
                        }

                    }

                    if (harpies_alive == 0 && (grate_obj != null) && (GetGlobalVar(455) & 0x40) == 0)
                    {
                        SetGlobalVar(455, GetGlobalVar(455) | 0x40);
                        // grate_obj.object_flag_set(OF_OFF)
                        var grate_npc = GameSystems.MapObject.CreateObject(14913, grate_obj.GetLocation());
                        grate_npc.Move(grate_obj.GetLocation(), 0, 11f);
                        grate_npc.Rotation = grate_obj.Rotation;
                    }

                }

            }

            // grate_npc.begin_dialog(game.leader, 1000)
            return SkipDefault;
        }
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((SelectedPartyLeader.GetMap() == 5008)) // Welcome Wench upstairs - PC left behind
            {
                if (((GameSystems.Party.PartyMembers).Contains(attachee)))
                {
                    triggerer.BeginDialog(attachee, 150);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 200);
                }

            }

            return SkipDefault;
        }
        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var cur_map = attachee.GetMap();
            // PC Commentary (float lines/banter)  ###
            if (PartyLeader.type == ObjectType.npc) // leftmost portrait an NPC
            {
                daemon_float_comment(attachee, 1);
                StartTimer(5000, () => daemon_float_comment(attachee, 2), true);
            }

            // Global Event Scheduling System  ###
            // Skole Goons
            if (tpsts("s_skole_goons", 3 * 24 * 60 * 60) && !get_f("s_skole_goons_scheduled") && !get_f("skole_dead"))
            {
                set_f("s_skole_goons_scheduled");
                if (GetQuestState(42) != QuestState.Completed && !GetGlobalFlag(281))
                {
                    // ggf281 - have had Skole Goon encounter
                    SetQuestState(42, QuestState.Botched);
                    SetGlobalFlag(202, true);
                    QueueRandomEncounter(3004);
                }

            }

            // Thrommel Reward Encounter - 2 weeks
            if (tpsts("s_thrommel_reward", 14 * 24 * 60 * 60) && !get_f("s_thrommel_reward_scheduled"))
            {
                set_f("s_thrommel_reward_scheduled");
                if (!GetGlobalFlag(278) && !(GameSystems.RandomEncounter.IsEncounterQueued(3001)))
                {
                    // ggf278 - have had Thrommel Reward encounter
                    QueueRandomEncounter(3001);
                }

            }

            // Tillahi Reward Encounter - 10 days
            if (tpsts("s_tillahi_reward", 10 * 24 * 60 * 60) && !get_f("s_tillahi_reward_scheduled"))
            {
                set_f("s_tillahi_reward_scheduled");
                if (!GetGlobalFlag(279) && !(GameSystems.RandomEncounter.IsEncounterQueued(3002)))
                {
                    // ggf279 - have had Tillahi Reward encounter
                    QueueRandomEncounter(3002);
                }

            }

            // Sargen Reward Encounter - 3 weeks
            if (tpsts("s_sargen_reward", 21 * 24 * 60 * 60) && !get_f("s_sargen_reward_scheduled"))
            {
                set_f("s_sargen_reward_scheduled");
                if (!GetGlobalFlag(280) && !(GameSystems.RandomEncounter.IsEncounterQueued(3003)))
                {
                    // ggf280 - have had Sargen Reward encounter
                    QueueRandomEncounter(3003);
                }

            }

            // Ranth's Bandits Encounter 1 - random amount of days (normal distribution, average of 24 days, stdev = 8 days)
            if (tpsts("s_ranths_bandits_1", GetGlobalVar(923) * 24 * 60 * 60) && !get_f("s_ranths_bandits_scheduled"))
            {
                set_f("s_ranths_bandits_scheduled");
                if (!GetGlobalFlag(711) && !(GameSystems.RandomEncounter.IsEncounterQueued(3434)))
                {
                    // ggf711 - have had Ranth's Bandits Encounter
                    QueueRandomEncounter(3434);
                }

            }

            // Scarlet Brotherhood Retaliation for Snitch Encounter - 10 days
            if (tpsts("s_sb_retaliation_for_snitch", 10 * 24 * 60 * 60) && !get_f("s_sb_retaliation_for_snitch_scheduled"))
            {
                set_f("s_sb_retaliation_for_snitch_scheduled");
                if (!GetGlobalFlag(712) && !(GameSystems.RandomEncounter.IsEncounterQueued(3435)))
                {
                    // ggf712 - have had Scarlet Brotherhood Encounter
                    QueueRandomEncounter(3435);
                }

            }

            // Scarlet Brotherhood Retaliation for Narc Encounter - 6 days
            if (tpsts("s_sb_retaliation_for_narc", 6 * 24 * 60 * 60) && !get_f("s_sb_retaliation_for_narc_scheduled"))
            {
                set_f("s_sb_retaliation_for_narc_scheduled");
                if (!GetGlobalFlag(712) && !(GameSystems.RandomEncounter.IsEncounterQueued(3435)))
                {
                    // ggf712 - have had Scarlet Brotherhood Encounter
                    QueueRandomEncounter(3435);
                }

            }

            // Scarlet Brotherhood Retaliation for Whistelblower Encounter - 14 days
            if (tpsts("s_sb_retaliation_for_whistleblower", 14 * 24 * 60 * 60) && !get_f("s_sb_retaliation_for_whistleblower_scheduled"))
            {
                set_f("s_sb_retaliation_for_whistleblower_scheduled");
                if (!GetGlobalFlag(712) && !(GameSystems.RandomEncounter.IsEncounterQueued(3435)))
                {
                    // ggf712 - have had Scarlet Brotherhood Encounter
                    QueueRandomEncounter(3435);
                }

            }

            // Gremlich Scream Encounter 1 - 1 day
            if (tpsts("s_gremlich_1", 1 * 24 * 60 * 60) && !get_f("s_gremlich_1_scheduled"))
            {
                set_f("s_gremlich_1_scheduled");
                if (!GetGlobalFlag(713) && !(GameSystems.RandomEncounter.IsEncounterQueued(3436)))
                {
                    // ggf713 - have had Gremlich Scream Encounter 1
                    QueueRandomEncounter(3436);
                }

            }

            // Gremlich Reset Encounter - 5 days
            if (tpsts("s_gremlich_2", 5 * 24 * 60 * 60) && !get_f("s_gremlich_2_scheduled"))
            {
                set_f("s_gremlich_2_scheduled");
                if (!GetGlobalFlag(717) && !(GameSystems.RandomEncounter.IsEncounterQueued(3440)))
                {
                    // ggf717 - have had Gremlich Reset Encounter
                    QueueRandomEncounter(3440);
                }

            }

            // Mona Sport Encounter 1 (pirates vs. brigands) - 3 days
            if (tpsts("s_sport_1", 3 * 24 * 60 * 60) && !get_f("s_sport_1_scheduled"))
            {
                set_f("s_sport_1_scheduled");
                if (!GetGlobalFlag(718) && !(GameSystems.RandomEncounter.IsEncounterQueued(3441)))
                {
                    // ggf718 - have had Mona Sport Encounter 1
                    QueueRandomEncounter(3441);
                }

            }

            // Mona Sport Encounter 2 (bugbears vs. orcs melee) - 3 days
            if (tpsts("s_sport_2", 3 * 24 * 60 * 60) && !get_f("s_sport_2_scheduled"))
            {
                set_f("s_sport_2_scheduled");
                if (!GetGlobalFlag(719) && !(GameSystems.RandomEncounter.IsEncounterQueued(3442)))
                {
                    // ggf719 - have had Mona Sport Encounter 2
                    QueueRandomEncounter(3442);
                }

            }

            // Mona Sport Encounter 3 (bugbears vs. orcs ranged) - 3 days
            if (tpsts("s_sport_3", 3 * 24 * 60 * 60) && !get_f("s_sport_3_scheduled"))
            {
                set_f("s_sport_3_scheduled");
                if (!GetGlobalFlag(720) && !(GameSystems.RandomEncounter.IsEncounterQueued(3443)))
                {
                    // ggf720 - have had Mona Sport Encounter 3
                    QueueRandomEncounter(3443);
                }

            }

            // Mona Sport Encounter 4 (hill giants vs. ettins) - 3 days
            if (tpsts("s_sport_4", 3 * 24 * 60 * 60) && !get_f("s_sport_4_scheduled"))
            {
                set_f("s_sport_4_scheduled");
                if (!GetGlobalFlag(721) && !(GameSystems.RandomEncounter.IsEncounterQueued(3444)))
                {
                    // ggf721 - have had Mona Sport Encounter 4
                    QueueRandomEncounter(3444);
                }

            }

            // Mona Sport Encounter 5 (female vs. male bugbears) - 3 days
            if (tpsts("s_sport_5", 3 * 24 * 60 * 60) && !get_f("s_sport_5_scheduled"))
            {
                set_f("s_sport_5_scheduled");
                if (!GetGlobalFlag(722) && !(GameSystems.RandomEncounter.IsEncounterQueued(3445)))
                {
                    // ggf722 - have had Mona Sport Encounter 5
                    QueueRandomEncounter(3445);
                }

            }

            // Mona Sport Encounter 6 (zombies vs. lacedons) - 3 days
            if (tpsts("s_sport_6", 3 * 24 * 60 * 60) && !get_f("s_sport_6_scheduled"))
            {
                set_f("s_sport_6_scheduled");
                if (!GetGlobalFlag(723) && !(GameSystems.RandomEncounter.IsEncounterQueued(3446)))
                {
                    // ggf723 - have had Mona Sport Encounter 6
                    QueueRandomEncounter(3446);
                }

            }

            // Bethany Encounter - 2 days
            if (tpsts("s_bethany", 2 * 24 * 60 * 60) && !get_f("s_bethany_scheduled"))
            {
                set_f("s_bethany_scheduled");
                if (!GetGlobalFlag(724) && !(GameSystems.RandomEncounter.IsEncounterQueued(3447)))
                {
                    // ggf724 - have had Bethany Encounter
                    QueueRandomEncounter(3447);
                }

            }

            if (tpsts("s_zuggtmoy_banishment_initiate", 4 * 24 * 60 * 60) && !get_f("s_zuggtmoy_gone") && GetGlobalFlag(326) && attachee.GetMap() != 5016 && attachee.GetMap() != 5019)
            {
                set_f("s_zuggtmoy_gone");
                BurneApprentice.return_Zuggtmoy(SelectedPartyLeader, SelectedPartyLeader);
            }

            // End of Global Event Scheduling System  ###
            if ((Co8Settings.PartyRunSpeed) != 0) // If set preference for speed
            {
                Batch.speedup(Co8Settings.PartyRunSpeed, Co8Settings.PartyRunSpeed);
            }

            if (GetGlobalFlag(403)) // Test mode enabled; autokill critters!
            {
                // game.particles( "sp-summon monster I", game.leader)
                // game.timevent_add( autokill, (cur_map, 1), 150 )
                autokill(cur_map);
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    pc.IdentifyAll();
                }

            }

            if ((cur_map == 5004)) // Moathouse Upper floor
            {
                if ((GetGlobalVar(455) & (1 << 7)) != 0) // Secret Door Reveal
                {
                    foreach (var obj in ObjList.ListVicinity(new locXY(464, 470), ObjectListFilter.OLC_PORTAL | ObjectListFilter.OLC_SCENERY))
                    {
                        MarkSecretDoorFound(obj);
                    }

                }

            }
            else if ((cur_map == 5005))
            {
                // Moathouse Dungeon
                var ggv402 = GetGlobalVar(402);
                var ggv403 = GetGlobalVar(403);
                if ((ggv402 & 1) == 0)
                {
                    Logger.Info("modifying moathouse... ");
                    modify_moathouse();
                    ggv402 |= 1;
                    SetGlobalVar(402, ggv402);
                }

                if (moathouse_alerted() && (ggv403 & 1) == 0)
                {
                    moathouse_reg();
                    ggv403 |= 1;
                    SetGlobalVar(403, ggv403);
                }

            }
            else if ((cur_map == 5008))
            {
                Logger.Info("Welcome Wench upstairs");
                foreach (var dude in GameSystems.Party.PartyMembers)
                {
                    if (dude.type == ObjectType.pc && dude.GetScriptId(ObjScriptEvent.Dialog) == 439 && get_f("pc_dropoff"))
                    {
                        Logger.Info("{0}", "Attempting to remove " + dude.ToString());
                        StartTimer(150, () => remove_from_all_groups(dude), true);
                    }

                }

                set_f("pc_dropoff", false);
            }
            else if ((cur_map == 5110)) // Temple Ruined Building
            {
                SetGlobalVar(491, GetGlobalVar(491) | 0x1);
            }
            else if ((cur_map == 5111)) // Temple Broken Tower - Exterior
            {
                SetGlobalVar(491, GetGlobalVar(491) | 0x2);
            }
            else if ((cur_map == 5065)) // Temple Broken Tower - Interior
            {
                SetGlobalVar(491, GetGlobalVar(491) | 0x4);
            }
            else if ((cur_map == 5092)) // Temple Escape Tunnel
            {
                SetGlobalVar(491, GetGlobalVar(491) | 0x8);
            }
            else if ((cur_map == 5112)) // Temple Burnt Farmhouse
            {
                SetGlobalVar(491, GetGlobalVar(491) | 0x10);
            }
            else if ((cur_map == 5064)) // Temple entrance level
            {
                var found_map_obj = false;
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    if (pc.FindItemByName(11299) != null)
                    {
                        found_map_obj = true;
                    }

                }

                if (!found_map_obj)
                {
                    var map_obj = GameSystems.MapObject.CreateObject(11299, SelectedPartyLeader.GetLocation());
                    var got_map_obj = false;
                    var pc_index = 0;
                    while (!got_map_obj && pc_index < GameSystems.Party.PartySize)
                    {
                        if (!GameSystems.Party.GetPartyGroupMemberN(pc_index).IsUnconscious()
                            && GameSystems.Party.GetPartyGroupMemberN(pc_index).type == ObjectType.pc)
                        {
                            got_map_obj = GameSystems.Party.GetPartyGroupMemberN(pc_index).GetItem(map_obj);
                            if (!got_map_obj)
                            {
                                pc_index += 1;
                            }

                        }
                        else
                        {
                            pc_index += 1;
                        }

                    }

                    if (got_map_obj)
                    {
                        GameSystems.Party.GetPartyGroupMemberN(pc_index).SetScriptId(ObjScriptEvent.Dialog, 435);
                        GameSystems.Party.GetPartyGroupMemberN(pc_index).BeginDialog(GameSystems.Party.GetPartyGroupMemberN(pc_index), 1200);
                    }
                    else
                    {
                        map_obj.SetObjectFlag(ObjectFlag.OFF);
                    }

                }

                if ((GetGlobalVar(455) & (1 << 7)) != 0)
                {
                    foreach (var obj in ObjList.ListVicinity(new locXY(500, 500), ObjectListFilter.OLC_SCENERY | ObjectListFilter.OLC_PORTAL))
                    {
                        MarkSecretDoorFound(obj);
                    }

                }

            }
            else if ((cur_map == 5066)) // Temple Level 1 ##
            {
                if ((get_v(455) & 1) == 0)
                {
                    record_time_stamp(460);
                    set_v(455, get_v(455) | 1);
                    modify_temple_level_1(attachee);
                }

                if (earth_alerted() && ((get_v(454) & 1) == 0) && !Co8Settings.DisableNewPlots && ((GetGlobalVar(450) & 0x2000) == 0))
                {
                    set_v(454, get_v(454) | 1);
                    Itt.earth_reg();
                }

                var (xx, yy) = SelectedPartyLeader.GetLocation();
                if (Math.Pow((xx - 421), 2) + Math.Pow((yy - 589), 2) <= 400)
                {
                    SetGlobalVar(491, GetGlobalVar(491) | 0x20);
                }

                if (Math.Pow((xx - 547), 2) + Math.Pow((yy - 589), 2) <= 400)
                {
                    SetGlobalVar(491, GetGlobalVar(491) | 0x40);
                }

            }
            else if ((cur_map == 5067)) // Temple Level 2 ##
            {
                if ((get_v(455) & 2) == 0)
                {
                    record_time_stamp(461);
                    set_v(455, get_v(455) | 2);
                    modify_temple_level_2(attachee);
                }

                if (water_alerted() && ((get_v(454) & 2) == 0 || ((get_v(454) & (0x40 + (1 << 7))) == 0x40)) && !Co8Settings.DisableNewPlots && ((GetGlobalVar(450) & 0x2000) == 0))
                {
                    set_v(454, get_v(454) | 2);
                    if ((get_v(454) & (0x40 + (1 << 7))) == 0x40)
                    {
                        set_v(454, get_v(454) | (1 << 7)); // indicate that Oohlgrist and co have been moved to Water
                    }

                    Itt.water_reg();
                }

                if (air_alerted() && ((get_v(454) & 4) == 0) && !Co8Settings.DisableNewPlots && ((GetGlobalVar(450) & 0x2000) == 0))
                {
                    set_v(454, get_v(454) | 4);
                    Itt.air_reg();
                }

                if (fire_alerted() && ((get_v(454) & 0x8) == 0 || ((get_v(454) & (0x10 + 0x20)) == 0x10)) && !Co8Settings.DisableNewPlots && ((GetGlobalVar(450) & 0x2000) == 0))
                {
                    // Fire is on alert and haven't yet regrouped, or have already regrouped but Oohlgrist was recruited afterwards (2**5) and not transferred yet
                    set_v(454, get_v(454) | 0x8);
                    if ((get_v(454) & (0x10 + 0x20)) == 0x10)
                    {
                        set_v(454, get_v(454) | 0x20); // indicate that Oohlgrist and co have been moved
                    }

                    SetGlobalFlag(154, true); // Make the Werewolf mirror shut up
                    Itt.fire_reg();
                }

                var (xx, yy) = SelectedPartyLeader.GetLocation();
                if (Math.Pow((xx - 564), 2) + Math.Pow((yy - 377), 2) <= 400)
                {
                    SetGlobalVar(491, GetGlobalVar(491) | (1 << 7));
                }
                else if (Math.Pow((xx - 485), 2) + Math.Pow((yy - 557), 2) <= 1600)
                {
                    SetGlobalVar(491, GetGlobalVar(491) | 0x100);
                }
                else if (Math.Pow((xx - 485), 2) + Math.Pow((yy - 503), 2) <= 400)
                {
                    SetGlobalVar(491, GetGlobalVar(491) | 0x100);
                }

            }
            else if ((cur_map == 5105)) // Temple Level 3 Lower (Thrommel, Scorpp, Falrinth etc.)
            {
                if ((get_v(455) & 4) == 0)
                {
                    record_time_stamp(462);
                    set_v(455, get_v(455) | 4);
                }

                var (xx, yy) = SelectedPartyLeader.GetLocation();
                if (Math.Pow((xx - 406), 2) + Math.Pow((yy - 436), 2) <= 400) // Fire Temple Access (near groaning spirit)
                {
                    SetGlobalVar(491, GetGlobalVar(491) | 0x200);
                }
                else if (Math.Pow((xx - 517), 2) + Math.Pow((yy - 518), 2) <= 400) // Air Temple Access (troll keys)
                {
                    SetGlobalVar(491, GetGlobalVar(491) | 0x400);
                }
                else if (Math.Pow((xx - 552), 2) + Math.Pow((yy - 489), 2) <= 400) // Air Temple Secret Door (Scorpp Area)
                {
                    SetGlobalVar(491, GetGlobalVar(491) | 0x400000);
                }
                else if (Math.Pow((xx - 616), 2) + Math.Pow((yy - 606), 2) <= 400) // Water Temple Access (lamia)
                {
                    SetGlobalVar(491, GetGlobalVar(491) | 0x800);
                }
                else if (Math.Pow((xx - 639), 2) + Math.Pow((yy - 450), 2) <= 1600) // Falrinth area
                {
                    SetGlobalVar(491, GetGlobalVar(491) | 0x1000);
                    if ((GetGlobalVar(455) & (1 << 7)) != 0) // Secret Door Reveal
                    {
                        foreach (var obj in ObjList.ListVicinity(new locXY(622, 503), ObjectListFilter.OLC_PORTAL | ObjectListFilter.OLC_SCENERY))
                        {
                            MarkSecretDoorFound(obj);
                        }

                    }

                }

            }
            else if ((cur_map == 5080)) // Temple Level 4
            {
                if ((get_v(455) & 8) == 0)
                {
                    record_time_stamp(463);
                    set_v(455, get_v(455) | 8);
                }

                var (xx, yy) = SelectedPartyLeader.GetLocation();
                if (Math.Pow((xx - 479), 2) + Math.Pow((yy - 586), 2) <= 400)
                {
                    SetGlobalVar(491, GetGlobalVar(491) | 0x2000);
                }
                else if (Math.Pow((xx - 477), 2) + Math.Pow((yy - 340), 2) <= 400)
                {
                    SetGlobalVar(491, GetGlobalVar(491) | 0x4000);
                }

            }
            else if ((cur_map == 5106)) // secret spiral staircase
            {
                SetGlobalVar(491, GetGlobalVar(491) | 0x8000);
            }
            else if ((cur_map == 5081)) // Air Node
            {
                SetGlobalVar(491, GetGlobalVar(491) | 0x10000);
            }
            else if ((cur_map == 5082)) // Earth Node
            {
                SetGlobalVar(491, GetGlobalVar(491) | 0x20000);
            }
            else if ((cur_map == 5083)) // Fire Node
            {
                SetGlobalVar(491, GetGlobalVar(491) | 0x40000);
            }
            else if ((cur_map == 5084)) // Water Node
            {
                SetGlobalVar(491, GetGlobalVar(491) | 0x80000);
            }
            else if ((cur_map == 5079)) // Zuggtmoy Level
            {
                SetGlobalVar(491, GetGlobalVar(491) | 0x100000);
            }

            return RunDefault;
        }

        private static void MarkSecretDoorFound(GameObjectBody obj)
        {
            if ((obj.GetSecretDoorFlags() & SecretDoorFlag.SECRET_DOOR) != 0)
            {
                obj.SetSecretDoorFlags(obj.GetSecretDoorFlags() | SecretDoorFlag.SECRET_DOOR_FOUND);
            }
        }

        public static void remove_from_all_groups(GameObjectBody obj)
        {
            obj.RemoveFromAllGroups();
        }
        public static void modify_temple_level_1(GameObjectBody pc)
        {
            // Gives Temple monsters and NPCs san_dying scripts, so the game recognizes the player slaughtering mobs
            // gnolls near southern entrance
            foreach (var gnollol in vlistxyr(558, 600, 14080, 25))
            {
                gnollol.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // gnollol.destroy()
            foreach (var gnollol in vlistxyr(558, 600, 14079, 25))
            {
                gnollol.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // gnollol.destroy()
            foreach (var gnollol in vlistxyr(558, 600, 14078, 25))
            {
                gnollol.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // gnollol.destroy()
            // Rats
            foreach (var vaporrat in vlistxyr(497, 573, 14068, 30))
            {
                vaporrat.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // vaporrat.destroy()
            foreach (var direrat in vlistxyr(440, 571, 14056, 15))
            {
                direrat.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // direrat.destroy()
            foreach (var direrat in vlistxyr(534, 389, 14056, 15))
            {
                direrat.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // direrat.destroy()
            // undead near secret staircase
            foreach (var skellygnoll in vlistxyr(462, 520, 14083, 100))
            {
                skellygnoll.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // skellygnoll.destroy()
            foreach (var skellygnoll in vlistxyr(462, 520, 14082, 100))
            {
                skellygnoll.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // skellygnoll.destroy()
            foreach (var skellygnoll in vlistxyr(462, 520, 14081, 100))
            {
                skellygnoll.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // skellygnoll.destroy()
            foreach (var skellybone in vlistxyr(496, 515, 14107, 100))
            {
                skellybone.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // skellybone.destroy()
            // Gnoll Leader area
            foreach (var gnoll_leader in vlistxyr(509, 534, 14066, 100))
            {
                gnoll_leader.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // gnoll_leader.destroy()
            foreach (var gnoll in vlistxyr(518, 531, 14067, 66))
            {
                gnoll.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // gnoll.destroy()
            foreach (var gnoll in vlistxyr(518, 531, 14078, 66))
            {
                // Barbarian gnoll
                gnoll.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // gnoll.destroy()
            foreach (var gnoll in vlistxyr(518, 531, 14079, 66))
            {
                var gloc = gnoll.GetLocation();
                var grot = gnoll.Rotation;
                gnoll.Destroy(); // replaces gnoll with non-DR version
                var newgnoll = GameSystems.MapObject.CreateObject(14631, gloc);
                newgnoll.Rotation = grot;
                newgnoll.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // gnoll.destroy()
            foreach (var gnoll in vlistxyr(518, 531, 14080, 66))
            {
                var gloc = gnoll.GetLocation();
                var grot = gnoll.Rotation;
                gnoll.Destroy();
                var newgnoll = GameSystems.MapObject.CreateObject(14632, gloc);
                newgnoll.Rotation = grot;
                newgnoll.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // newgnoll.destroy()
            foreach (var gnoll in vlistxyr(511, 549, 14079, 33))
            {
                var gloc = gnoll.GetLocation();
                var grot = gnoll.Rotation;
                gnoll.Destroy();
                var newgnoll = GameSystems.MapObject.CreateObject(14631, gloc);
                newgnoll.Rotation = grot;
                newgnoll.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // newgnoll.destroy()
            foreach (var gnoll in vlistxyr(511, 549, 14080, 33))
            {
                var gloc = gnoll.GetLocation();
                var grot = gnoll.Rotation;
                gnoll.Destroy();
                var newgnoll = GameSystems.MapObject.CreateObject(14632, gloc);
                newgnoll.Rotation = grot;
                newgnoll.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // newgnoll.destroy()
            foreach (var ogre in vlistxyr(508, 536, 14249, 35))
            {
                ogre.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // ogre.destroy()
            foreach (var bugbear in vlistxyr(508, 536, 14164, 35))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            // Earth critters near Ogre Chief
            foreach (var gnoll in vlistxyr(445, 538, 14078, 50))
            {
                gnoll.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // gnoll.destroy()
            foreach (var gnoll in vlistxyr(445, 538, 14079, 50))
            {
                var gloc = gnoll.GetLocation();
                var grot = gnoll.Rotation;
                gnoll.Destroy();
                var newgnoll = GameSystems.MapObject.CreateObject(14631, gloc);
                newgnoll.Rotation = grot;
                newgnoll.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // newgnoll.destroy()
            foreach (var gnoll in vlistxyr(445, 538, 14080, 50))
            {
                var gloc = gnoll.GetLocation();
                var grot = gnoll.Rotation;
                gnoll.Destroy();
                var newgnoll = GameSystems.MapObject.CreateObject(14632, gloc);
                newgnoll.Rotation = grot;
                newgnoll.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // newgnoll.destroy()
            foreach (var ogrechief in vlistxyr(467, 535, 14248, 50))
            {
                ogrechief.SetScriptId(ObjScriptEvent.Dying, 444);
            }

            // ogrechief.destroy()
            foreach (var hobgoblin in vlistxyr(467, 535, 14188, 50))
            {
                hobgoblin.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // hobgoblin.destroy()
            foreach (var goblin in vlistxyr(467, 535, 14184, 27))
            {
                goblin.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // goblin.destroy()
            foreach (var goblin in vlistxyr(467, 535, 14186, 27))
            {
                var gloc = goblin.GetLocation();
                var grot = goblin.Rotation;
                goblin.Destroy();
                var newgob = GameSystems.MapObject.CreateObject(14636, gloc);
                newgob.Rotation = grot;
                newgob.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // newgob.destroy()
            foreach (var bugbear in vlistxyr(467, 535, 14164, 27))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            // Temple Troops near Ogre Chief
            foreach (var troop in vlistxyr(440, 500, 14337, 30))
            {
                troop.SetScriptId(ObjScriptEvent.Dying, 443);
            }

            // troop.destroy()
            foreach (var fighter in vlistxyr(440, 500, 14338, 30))
            {
                fighter.SetScriptId(ObjScriptEvent.Dying, 443);
            }

            // fighter.destroy()
            // ghouls and ghasts near prisoners (Morgan etc.)
            foreach (var ghast in vlistxyr(545, 535, 14137, 50))
            {
                ghast.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // ghast.destroy()
            foreach (var ghast in vlistxyr(550, 545, 14136, 50))
            {
                ghast.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // ghast.destroy()
            foreach (var ghast in vlistxyr(545, 553, 14135, 50))
            {
                ghast.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // ghast.destroy()
            foreach (var ghoul in vlistxyr(549, 554, 14095, 100))
            {
                ghoul.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // ghoul.destroy()
            foreach (var ghoul in vlistxyr(549, 554, 14128, 100))
            {
                ghoul.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // ghoul.destroy()
            foreach (var ghoul in vlistxyr(549, 554, 14129, 100))
            {
                ghoul.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // ghoul.destroy()
            // harpy area
            foreach (var harpy in vlistxyr(406, 564, 14243, 100))
            {
                harpy.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // harpy.destroy()
            foreach (var harpy in vlistxyr(407, 545, 14243, 100))
            {
                harpy.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // harpy.destroy()
            foreach (var ghast in vlistxyr(423, 541, 14135, 50))
            {
                ghast.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // ghast.destroy()
            foreach (var ghast in vlistxyr(420, 547, 14136, 50))
            {
                ghast.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // ghast.destroy()
            foreach (var ghoul in vlistxyr(413, 566, 14129, 100))
            {
                ghoul.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // ghoul.destroy()
            foreach (var ghoul in vlistxyr(413, 566, 14128, 100))
            {
                ghoul.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // ghoul.destroy()
            foreach (var ghoul in vlistxyr(413, 566, 14095, 100))
            {
                ghoul.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // ghoul.destroy()
            foreach (var ghoul in vlistxyr(410, 526, 14129, 100))
            {
                ghoul.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // ghoul.destroy()
            foreach (var ghoul in vlistxyr(410, 526, 14128, 100))
            {
                ghoul.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // ghoul.destroy()
            foreach (var ghoul in vlistxyr(410, 526, 14095, 100))
            {
                ghoul.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // ghoul.destroy()
            // Gray Ooze and Gelatinous Cube
            foreach (var gelatinouscube in vlistxyr(415, 599, 14139, 100))
            {
                gelatinouscube.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // gelatinouscube.destroy()
            foreach (var grayooze in vlistxyr(415, 599, 14140, 100))
            {
                grayooze.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // grayooze.destroy()
            // spiders near wonnilon hideout
            foreach (var spider in vlistxyr(438, 398, 14417, 50))
            {
                spider.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // spider.destroy()
            // ghouls near wonnilon hideout
            foreach (var ghoul in vlistxyr(387, 398, 14128, 100))
            {
                ghoul.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // ghoul.destroy()
            // ghouls near northern entrance
            foreach (var ghoul in vlistxyr(459, 600, 14129, 100))
            {
                ghoul.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // ghoul.destroy()
            // ogre near southern entrance
            foreach (var ogre in vlistxyr(511, 601, 14448, 100))
            {
                ogre.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // ogre.destroy()
            // Temple Troop and bugbear doormen near Earth Commander
            foreach (var troop in vlistxyr(470, 483, 14337, 25))
            {
                troop.SetScriptId(ObjScriptEvent.Dying, 443);
            }

            // troop.destroy()
            foreach (var bugbear in vlistxyr(470, 483, 14165, 25))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            // Temple Troops and bugbears near Earth Commander
            foreach (var earthcommander in vlistxyr(450, 470, 14156, 35))
            {
                earthcommander.SetScriptId(ObjScriptEvent.Dying, 444);
            }

            // earthcommander.destroy()
            foreach (var lieutenant in vlistxyr(450, 470, 14339, 35))
            {
                lieutenant.SetScriptId(ObjScriptEvent.Dying, 443);
            }

            // lieutenant.destroy()
            foreach (var troop in vlistxyr(450, 470, 14337, 35))
            {
                troop.SetScriptId(ObjScriptEvent.Dying, 443);
            }

            // troop.destroy()
            foreach (var bugbear in vlistxyr(450, 470, 14165, 35))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            // Earth Altar
            foreach (var worshippers in vlistxyr(482, 392, 14337, 50))
            {
                worshippers.SetScriptId(ObjScriptEvent.Dying, 443);
            }

            // worshippers.destroy()
            foreach (var earthelemental in vlistxyr(482, 392, 14381, 50))
            {
                earthelemental.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // earthelemental.destroy()
            foreach (var largeearthelemental in vlistxyr(483, 420, 14296, 50))
            {
                largeearthelemental.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // largeearthelemental.destroy()
            // Romag, Hartsch and their bugbear guards
            // for romag in vlistxyr(445, 445, 8045, 11):
            // romag.scripts[12] = 444
            // romag.destroy()
            foreach (var hartsch in vlistxyr(445, 445, 14154, 11))
            {
                hartsch.SetScriptId(ObjScriptEvent.Dying, 444);
            }

            // hartsch.destroy()
            foreach (var bugbear in vlistxyr(445, 445, 14162, 11))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            foreach (var bugbear in vlistxyr(445, 445, 14163, 11))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            // Bugbears north of Romag
            foreach (var bugbear in vlistxyr(427, 435, 14162, 15))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            foreach (var bugbear in vlistxyr(427, 435, 14164, 15))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            foreach (var bugbear in vlistxyr(427, 435, 14165, 15))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            foreach (var bugbear in vlistxyr(418, 443, 14163, 5))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            foreach (var bugbear in vlistxyr(435, 427, 14163, 5))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            foreach (var bugbear in vlistxyr(435, 427, 14164, 5))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            // Bugbear "Checkpoint"
            foreach (var bugbear in vlistxyr(504, 477, 14164, 15))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            foreach (var bugbear in vlistxyr(504, 477, 14162, 15))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            foreach (var bugbear in vlistxyr(504, 477, 14163, 15))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            foreach (var bugbear in vlistxyr(504, 477, 14165, 15))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            // Bugbear "reservists"
            foreach (var bugbear in vlistxyr(524, 416, 14164, 15))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            foreach (var bugbear in vlistxyr(524, 416, 14163, 15))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            foreach (var bugbear in vlistxyr(524, 416, 14162, 15))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            // Wonnilon area
            foreach (var zombie in vlistxyr(546, 418, 14092, 100))
            {
                zombie.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // zombie.destroy()
            foreach (var zombie in vlistxyr(546, 418, 14123, 100))
            {
                zombie.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // zombie.destroy()
            foreach (var zombie in vlistxyr(546, 418, 14127, 100))
            {
                zombie.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // zombie.destroy()
            foreach (var bugbear in vlistxyr(546, 418, 14164, 35))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            foreach (var zombie in vlistxyr(546, 435, 14092, 100))
            {
                zombie.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // zombie.destroy()
            foreach (var zombie in vlistxyr(546, 435, 14124, 100))
            {
                zombie.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // zombie.destroy()
            foreach (var zombie in vlistxyr(546, 435, 14125, 100))
            {
                zombie.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // zombie.destroy()
            foreach (var zombie in vlistxyr(546, 435, 14126, 100))
            {
                zombie.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // zombie.destroy()
            foreach (var zombie in vlistxyr(546, 435, 14127, 100))
            {
                zombie.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // zombie.destroy()
            foreach (var bugbear in vlistxyr(546, 435, 14164, 35))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            // Turnkey
            foreach (var bugbear in vlistxyr(570, 460, 14165, 15))
            {
                bugbear.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // bugbear.destroy()
            foreach (var turnkey in vlistxyr(570, 460, 14229, 15))
            {
                turnkey.SetScriptId(ObjScriptEvent.Dying, 443);
            }

            // turnkey.destroy()
            // Ogre and Goblins
            foreach (var goblin in vlistxyr(563, 501, 14186, 50))
            {
                goblin.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // goblin.destroy()
            foreach (var goblin in vlistxyr(563, 501, 14187, 50))
            {
                goblin.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // goblin.destroy()
            foreach (var goblin in vlistxyr(563, 501, 14185, 50))
            {
                goblin.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // goblin.destroy()
            foreach (var ogre in vlistxyr(563, 501, 14448, 50))
            {
                ogre.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // ogre.destroy()
            // Stirges
            foreach (var stirge in vlistxyr(410, 491, 14182, 50))
            {
                stirge.SetScriptId(ObjScriptEvent.Dying, 441);
            }

            // stirge.destroy()
            return;
        }
        public static void modify_temple_level_2(GameObjectBody pc)
        {
            var dummy = 1;
            return;
        }
        // 104 - romag dead
        // 105 - belsornig dead
        // 106 - kelno dead
        // 107 - alrrem dead

        public static bool earth_alerted()
        {
            if (GetGlobalFlag(104)) // romag is dead
            {
                return false;
            }

            if (tpsts(512, 1 * 60 * 60))
            {
                // an hour has passed since you defiled the Earth Altar
                return true;
            }

            if (tpsts(507, 1))
            {
                // You've killed the Troop Commander
                return true;
            }

            if (tpsts(TS_CRITTER_THRESHOLD_CROSSED, 1))
            {
                var also_killed_earth_member = (tpsts(TS_EARTH_TROOP_KILLED_FIRST_TIME, 3 * 60)) || (tpsts(TS_EARTH_CRITTER_KILLED_FIRST_TIME, 6 * 60));
                var did_quest_1 = GetQuestState(43) >= QuestState.Completed;
                if ((!did_quest_1) || also_killed_earth_member)
                {
                    if (tpsts(TS_CRITTER_THRESHOLD_CROSSED, 2 * 60 * 60)) // two hours have passed since you passed critter deathcount threshold
                    {
                        return true;
                    }

                    if (tpsts(TS_CRITTER_KILLED_FIRST_TIME, 48 * 60 * 60)) // 48 hours have passed since you first killed a critter and you've passed the threshold
                    {
                        return true;
                    }

                }

            }

            // The second condition is for the case you've killed a critter, left to rest somewhere, and returned later, and at some point crossed the kill count threshold
            if ((tpsts(510, 1) && tpsts(505, 24 * 60 * 60)) || tpsts(510, 2 * 60 * 60))
            {
                // Either two hours have passed since you passed Earth critter deathcount threshold, or 24 hours have passed since you first killed an Earth critter and you've passed the threshold
                return true;
            }

            if ((tpsts(511, 1) && tpsts(506, 12 * 60 * 60)) || tpsts(511, 1 * 60 * 60))
            {
                // Either 1 hour has passed since you passed troop deathcount threshold, or 12 hours have passed since you first killed a troop and you've passed the threshold
                return true;
            }

            if (tsc(457, 470) || tsc(458, 470) || tsc(459, 470)) // killed Belsornig, Kelno or Alrrem before completing 2nd earth quest
            {
                return true;
            }

            return false;
        }
        public static bool water_alerted()
        {
            if (GetGlobalFlag(105))
            {
                // belsornig is dead
                return false;
            }

            if (tsc(456, 475) || tsc(458, 475) || tsc(459, 475)) // killed Romag, Kelno or Alrrem before accepting second water quest
            {
                return true;
            }

            return false;
        }
        public static bool air_alerted()
        {
            if (GetGlobalFlag(106))
            {
                // kelno is dead
                return false;
            }

            if (tsc(456, 483) || tsc(457, 483) || tsc(459, 483))
            {
                // any of the other faction leaders are dead, and he hasn't yet given you that quest
                // Kelno doesn't take any chances
                return true;
            }

            return false;
        }
        public static bool fire_alerted()
        {
            if (GetGlobalFlag(107)) // alrrem is dead
            {
                return false;
            }

            // if (game.global_flags[104] or game.global_flags[105] or game.global_flags[106]):
            // For now - if one of the other Leaders is dead
            // return 1
            if (tsc(456, 517) || tsc(457, 517) || tsc(458, 517))
            {
                // Have killed another High Priest without even having talked to him
                // Should suffice for him, since he's kind of crazy
                return true;
            }

            return false;
        }
        public static bool is_follower(int name)
        {
            foreach (var obj in GameSystems.Party.PartyMembers)
            {
                if ((obj.GetNameId() == name))
                {
                    return true;
                }

            }

            return false;
        }
        public static void destroy_weapons(GameObjectBody npc, int item1, int item2, int item3)
        {
            if ((item1 != 0))
            {
                var moshe = npc.FindItemByName(item1);
                moshe?.Destroy();

            }

            if ((item2 != 0))
            {
                var moshe = npc.FindItemByName(item2);
                moshe?.Destroy();

            }

            if ((item3 != 0))
            {
                var moshe = npc.FindItemByName(item3);
                moshe?.Destroy();

            }
        }
        public static void float_comment(GameObjectBody attachee, int line)
        {
            attachee.FloatLine(line, SelectedPartyLeader);
            return;
        }
        public static void daemon_float_comment(GameObjectBody attachee, int line)
        {
            if (attachee.type == ObjectType.pc)
            {
                attachee.SetScriptId(ObjScriptEvent.Dialog, 439);
                attachee.FloatLine(line, SelectedPartyLeader);
                attachee.RemoveScript(ObjScriptEvent.Dialog);
            }

            return;
        }
        public static void proactivity(GameObjectBody npc, int line_no)
        {
            npc.TurnTowards(PartyLeader);
            if ((!Utilities.critter_is_unconscious(PartyLeader) && PartyLeader.type == ObjectType.pc && !PartyLeader.D20Query(D20DispatcherKey.QUE_Prone) && npc.HasLineOfSight(PartyLeader)))
            {
                PartyLeader.BeginDialog(npc, line_no);
            }
            else
            {
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    npc.TurnTowards(pc);
                    if ((!Utilities.critter_is_unconscious(pc) && pc.type == ObjectType.pc && !pc.D20Query(D20DispatcherKey.QUE_Prone) && npc.HasLineOfSight(pc)))
                    {
                        pc.BeginDialog(npc, line_no);
                    }

                }

            }

            return;
        }
        public static bool tsc(int var1, int var2)
        {
            // time stamp compare
            // check if event associated with var1 happened before var2
            // if they happened in the same second, well... only so much I can do
            if ((get_v(var1) == 0))
            {
                return false;
            }
            else if ((get_v(var2) == 0))
            {
                return true;
            }
            else if ((get_v(var1) < get_v(var2)))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static bool tpsts(int time_var, int time_elapsed) => tpsts(time_var.ToString(), time_elapsed);

        public static bool tpsts(string time_var, int time_elapsed)
        {
            // type: (object, long) -> long
            // Has the time elapsed since [time stamp] greater than the specified amount?
            if (get_v(time_var) == 0)
            {
                return false;
            }

            if (CurrentTimeSeconds > get_v(time_var) + time_elapsed)
            {
                return true;
            }

            return false;
        }

        public static void record_time_stamp(int tvar, bool time_stamp_overwrite = false)
        {
            record_time_stamp(tvar.ToString(), time_stamp_overwrite);
        }

        public static void record_time_stamp(string tvar, bool time_stamp_overwrite = false)
        {
            if (get_v(tvar) == 0 || time_stamp_overwrite)
            {
                set_v(tvar, CurrentTimeSeconds);
            }
        }

        public static void pop_up_box(int message_id)
        {
            // generates popup box ala tutorial (without messing with the tutorial entries...)
            var a = GameSystems.MapObject.CreateObject(11001, SelectedPartyLeader.GetLocation());
            a.SetInt(obj_f.written_text_start_line, message_id);
            // FIXME: written_ui_show;
            a.Destroy();
            return;
        }
        public static void paladin_fall()
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pc.AddCondition("fallen_paladin");
            }

        }
        public static List<GameObjectBody> vlistxyr(int xx, int yy, int name, int radius)
        {
            var greg = new List<GameObjectBody>();
            foreach (var npc in ObjList.ListVicinity(new locXY(xx, yy), ObjectListFilter.OLC_NPC))
            {
                var (npc_x, npc_y) = npc.GetLocation();
                var dist = MathF.Sqrt((npc_x - xx) * (npc_x - xx) + (npc_y - yy) * (npc_y - yy));
                if ((npc.GetNameId() == name && dist <= radius))
                {
                    greg.Add(npc);
                }

            }

            return greg;
        }
        public static bool can_see2(GameObjectBody npc, GameObjectBody pc)
        {
            // Checks if there's an obstruction in the way (i.e. LOS regardless of facing)
            var orot = npc.Rotation; // Original rotation
            // Using its angle, we rotate the NPC and THEN check for sight.
            // After that, we return the NPC to its original facing.
            npc.Rotation = npc.RotationTo(pc);
            if (npc.HasLineOfSight(pc))
            {
                npc.Rotation = orot;
                return true;
            }

            npc.Rotation = orot;
            return false;
        }
        public static bool can_see_party(GameObjectBody npc)
        {
            foreach (var pc in PartyLeader.GetPartyMembers())
            {
                if (can_see2(npc, pc))
                {
                    return true;
                }

            }

            return false;
        }
        public static bool is_far_from_party(GameObjectBody npc, int dist = 20)
        {
            // Returns 1 if npc is farther than specified distance from party
            foreach (var pc in PartyLeader.GetPartyMembers())
            {
                if (npc.DistanceTo(pc) < dist)
                {
                    return false;
                }

            }

            return true;
        }
        public static bool is_safe_to_talk_rfv(GameObjectBody npc, GameObjectBody pc, int radius = 20, bool facing_required = false, bool visibility_required = true)
        {
            // visibility_required - Capability of seeing PC required (i.e. PC is not invisibile / sneaking)
            // -> use can_see2(npc, pc)
            // facing_required - In addition, the NPC is actually looking at the PC's direction
            if (!visibility_required)
            {
                if ((pc.type == ObjectType.pc && !Utilities.critter_is_unconscious(pc) && npc.DistanceTo(pc) <= radius))
                {
                    return true;
                }

            }
            else if (visibility_required && facing_required)
            {
                if ((npc.HasLineOfSight(pc) && pc.type == ObjectType.pc && !Utilities.critter_is_unconscious(pc) && npc.DistanceTo(pc) <= radius))
                {
                    return true;
                }

            }
            else if (visibility_required && !facing_required)
            {
                if ((can_see2(npc, pc) && pc.type == ObjectType.pc && !Utilities.critter_is_unconscious(pc) && npc.DistanceTo(pc) <= radius))
                {
                    return true;
                }

            }

            return false;
        }
        public static bool within_rect_by_corners(GameObjectBody obj, int ulx, int uly, int brx, int bry)
        {
            // refers to "visual" axes (edges parallel to your screen's edges rather than ToEE's native axes)
            var (xx, yy) = obj.GetLocation();
            if (((xx - yy) <= (ulx - uly)) && ((xx - yy) >= (brx - bry)) && ((xx + yy) >= (ulx + uly)) && ((xx + yy) <= (brx + bry)))
            {
                return true;
            }

            return false;
        }
        public static bool encroach(GameObjectBody a, GameObjectBody b)
        {
            // A primitive way of making distant AI combatants who don't close the distances by themselves move towards the player
            b.TurnTowards(a);
            if (a.DistanceTo(b) < 30)
            {
                return false;
            }

            var (ax, ay) = a.GetLocation();
            var (bx, by) = b.GetLocation();
            var dx = 0;
            var dy = 0;
            if (bx > ax)
            {
                dx = 1;
            }
            else if (bx < ax)
            {
                dx = -1;
            }

            if (by > ay)
            {
                dy = 1;
            }
            else if (by < ay)
            {
                dy = -1;
            }

            if (Math.Pow((ax - bx), 2) > Math.Pow((ay - by), 2)) // if X distance is greater than Y distance, starting trying to encroach on the x axis
            {
                var aprobe = GameSystems.MapObject.CreateObject(14631, new locXY(ax + dx, ay)); // probe to see if I'm not going into a wall
                aprobe.Move(new locXY(ax + dx, ay), a.GetInt(obj_f.offset_x), a.GetInt(obj_f.offset_y));
                if (can_see2(aprobe, a))
                {
                    aprobe.Destroy();
                    a.Move(new locXY(ax + dx, ay), a.GetInt(obj_f.offset_x), a.GetInt(obj_f.offset_y));
                    return true;
                }
                else
                {
                    aprobe.Move(new locXY(ax + dx, ay + dy), a.GetInt(obj_f.offset_x), a.GetInt(obj_f.offset_y));
                    if (can_see2(aprobe, a))
                    {
                        aprobe.Destroy();
                        a.Move(new locXY(ax + dx, ay + dy), a.GetInt(obj_f.offset_x), a.GetInt(obj_f.offset_y));
                        return true;
                    }
                    else
                    {
                        aprobe.Move(new locXY(ax, ay + dy), a.GetInt(obj_f.offset_x), a.GetInt(obj_f.offset_y));
                        if (can_see2(aprobe, a))
                        {
                            aprobe.Destroy();
                            a.Move(new locXY(ax, ay + dy), a.GetInt(obj_f.offset_x), a.GetInt(obj_f.offset_y));
                            return true;
                        }
                        else
                        {
                            aprobe.Destroy();
                            return false;
                        }

                    }

                }

            }
            else
            {
                var aprobe = GameSystems.MapObject.CreateObject(14631, new locXY(ax + dx, ay)); // probe to see if I'm not going into a wall
                aprobe.Move(new locXY(ax, ay + dy), a.GetInt(obj_f.offset_x), a.GetInt(obj_f.offset_y));
                if (can_see2(aprobe, a))
                {
                    aprobe.Destroy();
                    a.Move(new locXY(ax, ay + dy), a.GetInt(obj_f.offset_x), a.GetInt(obj_f.offset_y));
                    return true;
                }
                else
                {
                    aprobe.Move(new locXY(ax + dx, ay + dy), a.GetInt(obj_f.offset_x), a.GetInt(obj_f.offset_y));
                    if (can_see2(aprobe, a))
                    {
                        aprobe.Destroy();
                        a.Move(new locXY(ax + dx, ay + dy), a.GetInt(obj_f.offset_x), a.GetInt(obj_f.offset_y));
                        return true;
                    }
                    else
                    {
                        aprobe.Move(new locXY(ax + dx, ay), a.GetInt(obj_f.offset_x), a.GetInt(obj_f.offset_y));
                        if (can_see2(aprobe, a))
                        {
                            aprobe.Destroy();
                            a.Move(new locXY(ax + dx, ay), a.GetInt(obj_f.offset_x), a.GetInt(obj_f.offset_y));
                            return true;
                        }
                        else
                        {
                            aprobe.Destroy();
                            return false;
                        }

                    }

                }

            }

            return false;
        }
        public static GameObjectBody buffee(locXY location, int det_range, IList<int> buff_list, IList<GameObjectBody> done_list)
        {
            // finds people that are on a 'to buff' list "buff_list" (name array), around location "location", at range "det_range", that are not mentioned in "done_list"
            // e.g. in Alrrem's script you can find something like buffee( attachee.location, 15, [14344], [handle_to_other_werewolf] )
            var (xx0, yy0) = location;
            foreach (var darling in buff_list)
            {
                foreach (var obj in ObjList.ListVicinity(location, ObjectListFilter.OLC_NPC))
                {
                    var (xx1, yy1) = obj.GetLocation();
                    if (obj.GetNameId() == darling && obj.GetLeader() == null && !((done_list).Contains(obj)) && (Math.Pow((xx1 - xx0), 2) + Math.Pow((yy1 - yy0), 2)) <= Math.Pow(det_range, 2))
                    {
                        return obj;
                    }

                }

            }

            return null;
        }

        private static readonly ISet<int> MoathouseGuards = new HashSet<int> { 14074, 14075, 14076, 14077 };

        public static void modify_moathouse()
        {
            foreach (var obj in ObjList.ListVicinity(new locXY(490, 535), ObjectListFilter.OLC_NPC))
            {
                if (MoathouseGuards.Contains(obj.GetNameId()))
                {
                    obj.SetScriptId(ObjScriptEvent.Dying, 450);
                    obj.SetScriptId(ObjScriptEvent.EnterCombat, 450);
                    obj.SetScriptId(ObjScriptEvent.ExitCombat, 450);
                    obj.SetScriptId(ObjScriptEvent.StartCombat, 450);
                    obj.SetScriptId(ObjScriptEvent.EndCombat, 450);
                    if (obj.GetNameId() == 14077)
                    {
                        obj.SetNpcFlag(NpcFlag.KOS);
                        obj.SetScriptId(ObjScriptEvent.WillKos, 450); // will kos
                    }

                    obj.SetScriptId(ObjScriptEvent.SpellCast, 450);
                    obj.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
                    obj.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
                }

            }

            foreach (var obj in ObjList.ListVicinity(new locXY(512, 549), ObjectListFilter.OLC_NPC))
            {
                if (MoathouseGuards.Contains(obj.GetNameId()))
                {
                    obj.SetScriptId(ObjScriptEvent.Dying, 450);
                    obj.SetScriptId(ObjScriptEvent.EnterCombat, 450);
                    obj.SetScriptId(ObjScriptEvent.ExitCombat, 450);
                    obj.SetScriptId(ObjScriptEvent.StartCombat, 450);
                    obj.SetScriptId(ObjScriptEvent.EndCombat, 450);
                    if (obj.GetNameId() == 14077)
                    {
                        obj.SetNpcFlag(NpcFlag.KOS);
                        obj.SetScriptId(ObjScriptEvent.WillKos, 450); // will kos
                    }

                    obj.SetScriptId(ObjScriptEvent.SpellCast, 450);
                    obj.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
                    obj.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
                }

            }

            return;
        }
        public static bool moathouse_alerted()
        {
            if (GetGlobalFlag(363))
            {
                // Bullied or attacked Sergeant at the door
                return true;
            }
            else
            {
                var bugbear_group_kill_ack = 0;
                var gnoll_group_kill_ack = 0;
                var lubash_kill_ack = 0;
                var ground_floor_brigands_kill_ack = 0;
                if (GetGlobalVar(404) != 0 && (CurrentTimeSeconds > GetGlobalVar(404) + 12 * 60 * 60))
                {
                    bugbear_group_kill_ack = 1;
                }

                if (GetGlobalVar(405) != 0 && (CurrentTimeSeconds > GetGlobalVar(405) + 12 * 60 * 60))
                {
                    gnoll_group_kill_ack = 1;
                }

                if (GetGlobalVar(406) != 0 && (CurrentTimeSeconds > GetGlobalVar(406) + 12 * 60 * 60))
                {
                    lubash_kill_ack = 1;
                }

                if (GetGlobalVar(407) != 0 && (CurrentTimeSeconds > GetGlobalVar(407) + 48 * 60 * 60))
                {
                    ground_floor_brigands_kill_ack = 1;
                }

                return ((ground_floor_brigands_kill_ack + lubash_kill_ack + gnoll_group_kill_ack + bugbear_group_kill_ack) >= 2);
            }

            return false;
        }
        public static void moathouse_reg()
        {
            var found_new_door_guy = 0;
            foreach (var obj in ObjList.ListVicinity(new locXY(512, 549), ObjectListFilter.OLC_NPC))
            {
                if (obj.GetLeader() != null || obj.IsUnconscious())
                {
                    continue;

                }

                var (xx, yy) = obj.GetLocation();
                if ((new[] { 14074, 14075 }).Contains(obj.GetNameId()) && xx > 496 && yy > 544)
                {
                    // Corridor guardsmen
                    if (xx == 497 && yy == 549)
                    {
                        // archer
                        Itt.sps(obj, 639);
                        obj.SetInt(obj_f.speed_walk, 1085353216);
                        obj.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
                        obj.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
                        obj.Move(new locXY(481, 530), 0, 0);
                        obj.Rotation = 2.35f;
                    }
                    else if (xx == 507 && yy == 549)
                    {
                        // swordsman
                        obj.Destroy();
                    }
                    else if (xx == 515 && yy == 548)
                    {
                        // spearbearer
                        Itt.sps(obj, 637);
                        obj.SetInt(obj_f.speed_walk, 1085353216);
                        obj.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
                        obj.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
                        obj.Move(new locXY(483, 541), 0, 0);
                        obj.Rotation = 4;
                    }
                    else if (obj.GetNameId() == 14075)
                    {
                        // Door Sergeant - replace with a quiet sergeant
                        obj.Destroy();
                        var quietSergeant = GameSystems.MapObject.CreateObject(14076, new locXY(476, 541));
                        quietSergeant.Move(new locXY(476, 541), 0, 0);
                        quietSergeant.Rotation = 4;
                        quietSergeant.SetScriptId(ObjScriptEvent.Dying, 450);
                        quietSergeant.SetScriptId(ObjScriptEvent.EnterCombat, 450);
                        quietSergeant.SetScriptId(ObjScriptEvent.ExitCombat, 450);
                        quietSergeant.SetScriptId(ObjScriptEvent.StartCombat, 450);
                        quietSergeant.SetScriptId(ObjScriptEvent.EndCombat, 450);
                        quietSergeant.SetScriptId(ObjScriptEvent.SpellCast, 450);
                    }

                }

            }

            // Create a new door guy instead of the Sergeant
            if (!GetGlobalFlag(37) && !SelectedPartyLeader.HasReputation(15)) // killed Lareth or cleared Moathouse
            {
                var obj = GameSystems.MapObject.CreateObject(14074, new locXY(521, 547));
                obj.Move(new locXY(521, 547), 0, 0);
                obj.Rotation = 4;
                obj.SetScriptId(ObjScriptEvent.Dialog, 450);
                obj.SetScriptId(ObjScriptEvent.Dying, 450);
                obj.SetScriptId(ObjScriptEvent.EnterCombat, 450);
                obj.SetScriptId(ObjScriptEvent.ExitCombat, 450);
                obj.SetScriptId(ObjScriptEvent.StartCombat, 450);
                obj.SetScriptId(ObjScriptEvent.EndCombat, 450);
                obj.SetScriptId(ObjScriptEvent.Heartbeat, 450);
                obj.SetScriptId(ObjScriptEvent.SpellCast, 450);
            }

            return;
        }
        public static void lnk(
            locXY? loc_0 = null,
            int xx = -1,
            int yy = -1,
            int name_id = -1,
            IEnumerable<int> name_ids = null,
            int stun_name_id = -1,
            IEnumerable<int> stun_name_ids = null)
        {
            var stunNameIds = new HashSet<int>(stun_name_ids ?? Array.Empty<int>());
            // Locate n' Kill!
            if (stun_name_id != -1)
            {
                stunNameIds.Add(stun_name_id);
            }

            var nameIds = new HashSet<int>(name_ids ?? Array.Empty<int>());
            if (name_id != -1)
            {
                nameIds.Add(name_id);
            }

            var killOrStun = new List<int>();
            killOrStun.AddRange(nameIds);
            killOrStun.AddRange(stunNameIds);

            locXY location;
            if (loc_0.HasValue)
            {
                location = loc_0.Value;
            }
            else if (xx != -1 && yy != -1)
            {
                location = new locXY(xx, yy);
            }
            else
            {
                location = SelectedPartyLeader.GetLocation();
            }

            if (nameIds.Count == 0)
            {
                foreach (var obj in ObjList.ListVicinity(location, ObjectListFilter.OLC_NPC))
                {
                    if ((obj.GetReaction(PartyLeader) <= 0 || !obj.IsFriendly(PartyLeader)) && (obj.GetLeader() == null && (obj.GetObjectFlags() & ObjectFlag.DONTDRAW) == 0))
                    {
                        if (!((stunNameIds).Contains(obj.GetNameId())))
                        {
                            var damage_dice = Dice.Parse("50d50");
                            obj.Damage(PartyLeader, DamageType.Bludgeoning, damage_dice);
                            obj.Damage(PartyLeader, DamageType.Fire, damage_dice);
                            obj.Damage(PartyLeader, DamageType.Cold, damage_dice);
                            obj.Damage(PartyLeader, DamageType.Magic, damage_dice);
                        }
                        else
                        {
                            var damage_dice = Dice.Parse("50d50");
                            obj.Damage(null, DamageType.Subdual, damage_dice);
                        }

                    }

                }

            }
            else
            {
                foreach (var obj in ObjList.ListVicinity(location, ObjectListFilter.OLC_NPC))
                {
                    if (killOrStun.Contains(obj.GetNameId()) && (obj.GetReaction(PartyLeader) <= 0 || !obj.IsFriendly(PartyLeader)) && (obj.GetLeader() == null && (obj.GetObjectFlags() & ObjectFlag.DONTDRAW) == 0))
                    {
                        if (!((stunNameIds).Contains(obj.GetNameId())))
                        {
                            var damage_dice = Dice.Parse("50d50");
                            obj.Damage(PartyLeader, DamageType.Bludgeoning, damage_dice);
                            obj.Damage(PartyLeader, DamageType.Fire, damage_dice);
                            obj.Damage(PartyLeader, DamageType.Cold, damage_dice);
                            obj.Damage(PartyLeader, DamageType.Magic, damage_dice);
                        }
                        else
                        {
                            var damage_dice = Dice.Parse("50d50");
                            if (!obj.IsUnconscious())
                            {
                                obj.Damage(null, DamageType.Subdual, damage_dice);
                            }

                            foreach (var pc in GameSystems.Party.PartyMembers)
                            {
                                obj.AIRemoveFromShitlist(pc);
                            }

                        }

                    }

                }

            }

            return;
        }

        private static void AddRange(ISet<int> protos, IEnumerable<int> moreProtos)
        {
            foreach (var protoId in moreProtos)
            {
                protos.Add(protoId);
            }
        }

        public static void loot_items(
            GameObjectBody loot_source = null,
            GameObjectBody pc = null,
            int loot_source_name = -1,
            IEnumerable<int> loot_source_names = null,
            int xx = -1, int yy = -1,
            IEnumerable<int> item_proto_list = null,
            bool loot_money_and_jewels_also = true,
            bool autoloot = true,
            bool autoconvert_jewels = true,
            IEnumerable<int> item_autoconvert_list = null)
        {
            if (!get_f("qs_autoloot"))
            {
                return;
            }

            if (!get_f("qs_autoconvert_jewels"))
            {
                autoconvert_jewels = false;
            }

            var tank_armor_0 = new List<GameObjectBody>();
            var barbarian_armor_0 = new List<GameObjectBody>();
            var druid_armor_0 = new List<GameObjectBody>();
            var wizard_items_0 = new List<GameObjectBody>();

            var itemProtos = new HashSet<int>(item_proto_list ?? Array.Empty<int>());

            if (loot_money_and_jewels_also)
            {
                AddRange(itemProtos, Money);
                AddRange(itemProtos, Gems);
                AddRange(itemProtos, Jewels);
                AddRange(itemProtos, Potions);
            }

            // pc - Who will take the loot?
            if (pc == null)
            {
                pc = SelectedPartyLeader;
            }

            // location - Where will the loot be sought?
            locXY location;
            if (xx == -1 || yy == -1)
            {
                location = pc.GetLocation();
            }
            else
            {
                location = new locXY(xx, yy);
            }

            var autoConvertProtos = new HashSet<int>(item_autoconvert_list ?? Array.Empty<int>());

            var allProtos = new HashSet<int>(itemProtos.Concat(autoConvertProtos));
            if (loot_source != null)
            {
                foreach (var pp in allProtos)
                {
                    if (autoConvertProtos.Contains(pp))
                    {
                        var pp_1 = loot_source.FindItemByProto(pp);
                        if (pp_1 != null && (pp_1.GetItemFlags() & (ItemFlag.NO_DISPLAY|ItemFlag.NO_LOOT)) == 0)
                        {
                            autosell_item(pp_1);
                        }
                    }
                    else if (!pc.GetItem(loot_source.FindItemByProto(pp)))
                    {
                        foreach (var obj in GameSystems.Party.PartyMembers)
                        {
                            if (obj.GetItem(loot_source.FindItemByProto(pp)))
                            {
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                var lootSourceNames = new HashSet<int>(loot_source_names ?? Array.Empty<int>());
                if (loot_source_name != -1)
                {
                    lootSourceNames.Add(loot_source_name);
                }

                foreach (var robee in ObjList.ListVicinity(location, ObjectListFilter.OLC_NPC | ObjectListFilter.OLC_CONTAINER | ObjectListFilter.OLC_ARMOR | ObjectListFilter.OLC_WEAPON | ObjectListFilter.OLC_GENERIC))
                {
                    if (!((PartyLeader.GetPartyMembers()).Contains(robee)) && (lootSourceNames.Contains(robee.GetNameId()) || lootSourceNames.Count == 0))
                    {
                        if ((robee.type == ObjectType.weapon) || (robee.type == ObjectType.armor) || (robee.type == ObjectType.generic))
                        {
                            if ((robee.GetItemFlags() & (ItemFlag.NO_DISPLAY|ItemFlag.NO_LOOT)) == 0)
                            {
                                if (allProtos.Contains(robee.ProtoId))
                                {
                                    autosell_item(robee);
                                }
                                else if (AutoSellExclude.Contains(robee.ProtoId))
                                {
                                    if (!pc.GetItem(robee))
                                    {
                                        foreach (var obj in GameSystems.Party.PartyMembers)
                                        {
                                            if (obj.GetItem(robee))
                                            {
                                                break;

                                            }

                                        }

                                    }

                                }

                            }

                        }

                        if (robee.type == ObjectType.npc)
                        {
                            for (var qq = EquipSlot.Helmet; qq < EquipSlot.Count; qq++)
                            {
                                var qq_item_worn = robee.ItemWornAt(qq);
                                if (qq_item_worn != null && (qq_item_worn.GetItemFlags() & (ItemFlag.NO_DISPLAY|ItemFlag.NO_LOOT)) == 0)
                                {
                                    if (allProtos.Contains(qq_item_worn.ProtoId))
                                    {
                                        autosell_item(qq_item_worn);
                                    }

                                }
                            }
                        }

                        foreach (var item_proto in allProtos)
                        {
                            var item_sought = robee.FindItemByProto(item_proto);
                            if (item_sought != null && (item_sought.GetItemFlags() & ItemFlag.NO_DISPLAY) == 0)
                            {
                                if (((Gems.Contains(item_proto) || Jewels.Contains(item_proto)) && autoconvert_jewels) || ((item_autoconvert_list).Contains(item_proto)))
                                {
                                    autosell_item(item_sought, item_proto, pc);
                                }
                                else if (!pc.GetItem(item_sought))
                                {
                                    foreach (var obj in GameSystems.Party.PartyMembers)
                                    {
                                        if (obj.GetItem(item_sought))
                                        {
                                            break;

                                        }

                                    }

                                }

                            }

                        }

                    }

                }

            }
        }

        public static float sell_modifier()
        {
            var highest_appraise = -999;
            foreach (var obj in GameSystems.Party.PartyMembers)
            {
                if (obj.GetSkillLevel(SkillId.appraise) > highest_appraise)
                {
                    highest_appraise = obj.GetSkillLevel(SkillId.appraise);
                }

            }

            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if (pc.GetStat(Stat.level_wizard) > 1)
                {
                    highest_appraise = highest_appraise + 2; // Heroism / Fox's Cunning bonus
                    break;

                }

            }

            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if (pc.GetStat(Stat.level_bard) > 1)
                {
                    highest_appraise = highest_appraise + 2; // Inspire Competence bonus
                    break;

                }

            }

            if (highest_appraise > 19)
            {
                return 0.97f;
            }
            else if (highest_appraise < -13)
            {
                return 0;
            }
            else
            {
                return 0.4f + highest_appraise * 0.03f;
            }

        }
        public static int appraise_tool(GameObjectBody obj)
        {
            // Returns what you'd get for selling it
            var aa = sell_modifier();
            return (int)(aa * obj.GetInt(obj_f.item_worth));
        }
        public static int s_roundoff(int app_sum)
        {
            if (app_sum <= 1000)
            {
                return app_sum;
            }

            if (app_sum > 1000 && app_sum <= 10000)
            {
                return 10 * (int)(((int)(app_sum) / 10));
            }

            if (app_sum > 10000 && app_sum <= 100000)
            {
                return 100 * (int)(((int)(app_sum) / 100));
            }

            
            return 1000 * (int)(((int)(app_sum) / 1000));
        }
        
        public static void autosell_item(GameObjectBody item_sought, int item_proto = -1, GameObjectBody pc = null, int item_quantity = 1, int display_float = 1)
        {
            if (item_sought == null)
            {
                return;
            }

            if (pc == null)
            {
                pc = SelectedPartyLeader;
            }

            if (item_proto == -1)
            {
                item_proto = item_sought.ProtoId;
            }

            var autoconvert_copper = appraise_tool(item_sought) * Math.Min(1, item_sought.GetQuantity());
            pc.AdjustMoney(autoconvert_copper);
            item_sought.SetFlag(ObjectFlag.OFF, true);
            item_sought.SetItemFlag(ItemFlag.NO_DISPLAY);
            item_sought.SetItemFlag(ItemFlag.NO_LOOT);
            if (display_float != 0 && autoconvert_copper > 5000 || display_float == 2)
            {
                pc.FloatMesFileLine("mes/script_activated.mes", 10000, TextFloaterColor.Green);
                pc.FloatMesFileLine("mes/description.mes", item_proto, TextFloaterColor.Green);
                pc.FloatMesFileLine("mes/transaction_sum.mes", (s_roundoff(autoconvert_copper / 100)), TextFloaterColor.Green);
            }

            return;
        }
        public static bool giv(GameObjectBody pc, int proto_id, bool in_group = false)
        {
            if (!in_group)
            {
                if (pc.FindItemByProto(proto_id) == null)
                {
                    Utilities.create_item_in_inventory(proto_id, pc);
                    return true;
                }
            }
            else
            {
                var foundit = 0;
                foreach (var obj in GameSystems.Party.PartyMembers)
                {
                    if (obj.FindItemByProto(proto_id) != null)
                    {
                        foundit = 1;
                    }

                }

                if (foundit == 0)
                {
                    Utilities.create_item_in_inventory(proto_id, pc);
                    return true;
                }

            }

            return false;
        }
        public static void cnk(int proto_id, bool do_not_destroy = false, int how_many = 1, int timer = 0)
        {
            // Create n' Kill
            // Meant to simulate actually killing the critter
            // if timer == 0:
            for (var pp = 0; pp < how_many; pp++)
            {
                var a = GameSystems.MapObject.CreateObject(proto_id, SelectedPartyLeader.GetLocation());
                var damage_dice = Dice.Parse("50d50");
                a.Damage(PartyLeader, DamageType.Bludgeoning, damage_dice);
                if (!do_not_destroy)
                {
                    a.Destroy();
                }

            }

            // else:
            // for pp in range(0, how_many):
            // game.timevent_add( cnk, (proto_id, do_not_destroy, 1, 0), (pp+1)*20 )
            return;
        }
        // AUTOKILL ###


        public static void autokill(int cur_map)
        {
            autokill(cur_map, true, false);
        }

        public static void autokill(int cur_map, bool autoloot, bool is_timed_autokill)
        {
            // if (cur_map in range(5069, 5078) ): #random encounter maps
            // ## Skole Goons
            // flash_signal(0)
            // if get_f('qs_autokill_nulb'):
            // if get_v('qs_skole_goon_time') == 0:
            // set_v('qs_skole_goon_time', 500)
            // game.timevent_add( autokill, (cur_map), 100 )
            // flash_signal(1)
            // if get_v('qs_skole_goon_time') == 500:
            // flash_signal(2)
            // lnk(name_id = [14315])
            // #14315 - Skole Goons
            // loot_items(loot_source_name = [14315]) # Skole goons
            // if get_f('qs_is_repeatable_encounter'):
            // lnk()
            // loot_items()
            // HOMMLET   #
            if ((cur_map == 5001)) // Hommlet Exterior
            {
                if (get_v("qs_emridy_time") == 1500)
                {
                    SetQuestState(100, QuestState.Completed);
                    GameObjectBody bro_smith = null;
                    foreach (var obj in ObjList.ListVicinity(new locXY(571, 434), ObjectListFilter.OLC_NPC))
                    {
                        if (obj.GetNameId() == 20005)
                        {
                            bro_smith = obj;
                        }

                    }

                    if (bro_smith != null)
                    {
                        Utilities.party_transfer_to(bro_smith, 12602);
                        SetGlobalFlag(979, true);
                    }

                    set_v("qs_emridy_time", 2000);
                }

                if (get_f("qs_arena_of_heroes_enable"))
                {
                    if (get_f("qs_lareth_dead"))
                    {
                        SetGlobalVar(974, 2); // Simulate having talked about chest
                        SetGlobalVar(705, 2); // Simulate having handled chest
                        if (!get_f("qs_book_of_heroes_given"))
                        {
                            giv(SelectedPartyLeader, 11050, true); // Book of Heroes
                            giv(SelectedPartyLeader, 12589, true); // Horn of Fog
                            set_f("qs_book_of_heroes_given");
                        }

                        SetGlobalVar(702, 1); // Make sure Kent doesn't pester
                    }

                    if (GetGlobalVar(994) == 0)
                    {
                        SetGlobalVar(994, 1); // Skip Master of the Arena chatter
                    }

                }

            }

            if ((cur_map == 5008)) // Welcome Wench Upstairs
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    if (!is_timed_autokill)
                    {
                        StartTimer(100, () => autokill(cur_map, true, true));
                    }
                    else
                    {
                        // Barbarian Elf
                        lnk(xx: 482, yy: 476, name_id: 8717);
                        loot_items(loot_source_name: 8717, item_autoconvert_list: new[] { 6396, 6045, 6046, 4204 });
                        SetGlobalVar(961, 4);
                    }

                }

            }

            // MOATHOUSE   #
            if ((cur_map == 5002)) // Moathouse Exterior
            {
                if (get_f("qs_autokill_moathouse"))
                {
                    lnk(xx: 469, yy: 524, name_id: 14057); // giant frogs
                    lnk(xx: 492, yy: 523, name_id: 14057); // giant frogs
                    lnk(xx: 475, yy: 505, name_id: 14057); // giant frogs
                    loot_items(xx: 475, yy: 505, item_proto_list: new[] { 6270 }, loot_source_name: 14057, autoloot: autoloot); // Jay's Ring
                    lnk(xx: 475, yy: 460, name_id: 14070); // courtyard brigands
                    loot_items(xx: 475, yy: 460, autoloot: autoloot);
                    if (get_v("qs_moathouse_ambush_time") == 0 && get_f("qs_lareth_dead"))
                    {
                        StartTimer(500, () => autokill(cur_map));
                        set_v("qs_moathouse_ambush_time", 500);
                    }
                    else if (get_v("qs_moathouse_ambush_time") == 500)
                    {
                        lnk(xx: 478, yy: 460, name_ids: new[] { 14078, 14079, 14080, 14313, 14314, 14642, 8010, 8004, 8005 }); // Ambush
                        lnk(xx: 430, yy: 444, name_ids: new[] { 14078, 14079, 14080, 14313, 14314, 14642, 8010, 8004, 8005 }); // Ambush
                        loot_items(xx: 478, yy: 460);
                        loot_items(xx: 430, yy: 444);
                        set_v("qs_moathouse_ambush_time", 1000);
                    }

                }

                if (get_f("qs_autokill_temple"))
                {
                    lnk(xx: 503, yy: 506, name_ids: new[] { 14507, 14522 }); // Boars
                    lnk(xx: 429, yy: 437, name_ids: new[] { 14052, 14053 }); // Bears
                    lnk(xx: 478, yy: 448, name_ids: new[] { 14600, 14674, 14615, 14603, 14602, 14601 }); // Undead
                    lnk(xx: 468, yy: 470, name_ids: new[] { 14674, 14615, 14603, 14602, 14601 }); // Undead
                }

            }

            if ((cur_map == 5003)) // Moathouse Tower
            {
                if (get_f("qs_autokill_moathouse"))
                {
                    lnk(name_id: 14047); // giant spider
                }

            }

            if ((cur_map == 5004)) // Moathouse Upper floor
            {
                if (get_f("qs_autokill_moathouse"))
                {
                    lnk(xx: 476, yy: 493, name_id: 14088); // Huge Viper
                    lnk(xx: 476, yy: 493, name_id: 14182); // Stirges
                    lnk(xx: 473, yy: 472, name_ids: new[] { 14070, 14074, 14069 }); // Backroom brigands
                    loot_items(xx: 473, yy: 472, autoloot: autoloot);
                    lnk(xx: 502, yy: 476, name_ids: new[] { 14089, 14090 }); // Giant Tick & Lizard
                    loot_items(xx: 502, yy: 472, autoloot: autoloot, item_proto_list: new[] { 6050 });
                }

                if (get_f("qs_autokill_temple") && GetGlobalVar(972) == 2)
                {
                    if (get_v("qs_moathouse_respawn__upper_time") == 0)
                    {
                        StartTimer(500, () => autokill(cur_map));
                        set_v("qs_moathouse_respawn__upper_time", 500);
                    }

                    if (get_v("qs_moathouse_respawn__upper_time") == 500)
                    {
                        lnk(xx: 476, yy: 493, name_ids: new[] { 14138, 14344, 14391 }); // Lycanthropes
                        lnk(xx: 502, yy: 476, name_ids: new[] { 14295, 14142 }); // Basilisk & Ochre Jelly
                    }

                }

            }

            if ((cur_map == 5005)) // Moathouse Dungeon
            {
                if (get_f("qs_autokill_moathouse"))
                {
                    lnk(xx: 416, yy: 439, name_id: 14065); // Lubash
                    loot_items(xx: 416, yy: 439, item_proto_list: new[] { 6058 }, loot_source_name: 14065, autoloot: autoloot);
                    SetGlobalFlag(55, true); // Freed Gnomes
                    SetGlobalFlag(991, true); // Flag For Verbobonc Gnomes
                    lnk(xx: 429, yy: 413, name_ids: new[] { 14123, 14124, 14092, 14126, 14091 }); // Zombies, Green Slime
                    lnk(xx: 448, yy: 417, name_ids: new[] { 14123, 14124, 14092, 14126 }); // Zombies
                    loot_items(xx: 448, yy: 417, item_proto_list: new[]{12105}, loot_source_name: -1, autoloot: autoloot);
                    var bugbears = new HashSet<int> {14170..14174, 14213..14217};
                    lnk(xx: 450, yy: 519, name_ids: bugbears); // Bugbears
                    lnk(xx: 430, yy: 524, name_ids: bugbears); // Bugbears
                    loot_items(xx: 450, yy: 519, autoloot: autoloot);
                    loot_items(xx: 430, yy: 524, autoloot: autoloot);
                    if (GameSystems.Party.PartySize < 4 && get_v("AK5005_Stage") < 1)
                    {
                        set_v("AK5005_Stage", get_v("AK5005_Stage") + 1);
                        return;
                    }

                    // Gnolls and below
                    lnk(xx: 484, yy: 497, name_ids: new[] { 14066, 14067, 14078, 14079, 14080 }); // Gnolls
                    lnk(xx: 484, yy: 473, name_ids: new[] { 14066, 14067, 14078, 14079, 14080 }); // Gnolls
                    loot_items(xx: 484, yy: 497, autoloot: autoloot);
                    loot_items(xx: 484, yy: 473, autoloot: autoloot);
                    lnk(xx: 543, yy: 502, name_id: 14094); // Giant Crayfish
                    lnk(xx: 510, yy: 447, name_ids: new[] { 14128, 14129, 14095 }); // Ghouls
                    if (GameSystems.Party.PartySize < 4 && get_v("AK5005_Stage") < 2 || (GameSystems.Party.PartySize < 8 && get_v("AK5005_Stage") < 1))
                    {
                        set_v("AK5005_Stage", get_v("AK5005_Stage") + 1);
                        return;
                    }

                    lnk(xx: 515, yy: 547, name_ids: new[] { 14074, 14075 }); // Front Guardsmen
                    loot_items(xx: 515, yy: 547, autoloot: autoloot);
                    lnk(xx: 485, yy: 536, name_ids: new[] { 14074, 14075, 14076, 14077 }); // Back Guardsmen
                    loot_items(xx: 485, yy: 536, loot_source_names: new[] { 14074, 14075, 14076, 14077 }, autoloot: autoloot); // Back guardsmen
                    if (!get_f("qs_lareth_spiders_spawned"))
                    {
                        Lareth1.create_spiders(SelectedPartyLeader, SelectedPartyLeader);
                        set_f("qs_lareth_spiders_spawned", true);
                    }

                    lnk(xx: 480, yy: 540, name_ids: new[] { 8002, 14397, 14398, 14620 }); // Lareth & Spiders
                    set_f("qs_lareth_dead");
                    lnk(xx: 530, yy: 550, name_ids: new[] { 14417 }); // More Spiders
                    var itemProtos = new HashSet<int> { 4120, 6097, 6098, 6099, 6100, 11003, 9001..9688 };
                    loot_items(xx: 480, yy: 540, item_proto_list: itemProtos, loot_source_names: new[] { 8002, 1045 }, autoloot: autoloot); // Lareth & Lareth's Dresser
                    loot_items(xx: 480, yy: 540, item_autoconvert_list: new[] { 4194 });
                }

                // RESPAWN
                if (get_f("qs_autokill_temple") && GetGlobalVar(972) == 2)
                {
                    if (get_v("qs_moathouse_respawn_dungeon_time") == 0)
                    {
                        StartTimer(500, () => autokill(cur_map));
                        set_v("qs_moathouse_respawn_dungeon_time", 500);
                    }

                    if (get_v("qs_moathouse_respawn__upper_time") == 500)
                    {
                        lnk(xx: 416, yy: 439, name_id: 14141); // Crystal Oozes
                                                               // Bodaks, Shadows and Groaning Spirit
                        lnk(xx: 436, yy: 521, name_ids: new[] { 14328, 14289, 14280 });
                        // Skeleton Gnolls
                        lnk(xx: 486, yy: 480, name_ids: new[] { 14616, 14081, 14082, 14083 });
                        lnk(xx: 486, yy: 495, name_ids: new[] { 14616, 14081, 14082, 14083 }); // Skeleton Gnolls
                                                                                              // Witch
                        lnk(xx: 486, yy: 540, name_ids: new[] { 14603, 14674, 14601, 14130, 14137, 14328, 14125, 14110, 14680 });
                        loot_items(xx: 486, yy: 540, item_proto_list: new[] { 11098, 6273, 4057, 6263, 4498 }, item_autoconvert_list: new[] { 4226, 6333, 5099 });
                    }

                }

            }

            if ((cur_map == 5091)) // Cave Exit
            {
                if (get_f("qs_autokill_moathouse"))
                {
                    if (get_v("qs_moathouse_ambush_time") == 0 && get_f("qs_lareth_dead"))
                    {
                        StartTimer(500, () => autokill(cur_map));
                        set_v("qs_moathouse_ambush_time", 500);
                    }
                    else if (get_v("qs_moathouse_ambush_time") == 500)
                    {
                        lnk(xx: 500, yy: 490, name_ids: new[] { 14078, 14079, 14080, 14313, 14314, 14642, 8010, 8004, 8005 }); // Ambush
                        lnk(xx: 470, yy: 485, name_ids: new[] { 14078, 14079, 14080, 14313, 14314, 14642, 8010, 8004, 8005 }); // Ambush
                        loot_items(xx: 500, yy: 490);
                        loot_items(xx: 470, yy: 490);
                        set_v("qs_moathouse_ambush_time", 1000);
                    }

                }

            }

            if ((cur_map == 5094)) // Emridy Meadows
            {
                if (get_f("qs_autokill_moathouse"))
                {
                    if (get_v("qs_emridy_time") == 0)
                    {
                        StartTimer(500, () => autokill(cur_map));
                        set_v("qs_emridy_time", 500);
                    }
                    else if (get_v("qs_emridy_time") == 500)
                    {
                        set_v("qs_emridy_time", 1000);
                        StartTimer(500, () => autokill(cur_map));
                        lnk(xx: 467, yy: 383, name_ids: new[] { 14603, 14600 }); // NW Skeletons
                        loot_items(xx: 467, yy: 380);
                        lnk(xx: 507, yy: 443, name_ids: new[] { 14603, 14600 }); // W Skeletons
                        lnk(xx: 515, yy: 421, name_ids: new[] { 14603, 14600 }); // W Skeletons
                        loot_items(xx: 507, yy: 443);
                        loot_items(xx: 515, yy: 421);
                        lnk(xx: 484, yy: 487, name_ids: new[] { 14603, 14600, 14616, 14615 }); // Rainbow Rock 1
                        lnk(xx: 471, yy: 500, name_ids: new[] { 14603, 14600, 14616, 14615 }); // Rainbow Rock 1
                        loot_items(xx: 484, yy: 487);
                        loot_items(xx: 484, yy: 487, loot_source_names: new[] { 1031 }, item_proto_list: new[] { 12024 });
                        if (!get_f("qs_rainbow_spawned"))
                        {
                            set_f("qs_rainbow_spawned", true);
                            // py00265rainbow_rock.san_use(game.leader, game.leader)
                            // san_use(game.leader, game.leader)
                            // game.particles( "sp-summon monster I", game.leader)
                            foreach (var qq in ObjList.ListVicinity(new locXY(484, 487), ObjectListFilter.OLC_CONTAINER))
                            {
                                if (qq.GetNameId() == 1031)
                                {
                                    qq.ExecuteObjectScript(qq, ObjScriptEvent.Use);
                                }

                            }

                        }

                        lnk(xx: 484, yy: 487, name_ids: new[] { 14602, 14601 }); // Rainbow Rock 2
                        loot_items(xx: 484, yy: 487);
                        // game.timevent_add( autokill, (cur_map), 1500 )
                        lnk(xx: 532, yy: 540, name_ids: new[] { 14603, 14600 }); // SE Skeletons
                        loot_items(xx: 540, yy: 540);
                        lnk(xx: 582, yy: 514, name_ids: new[] { 14221, 14053 }); // Hill Giant
                    }
                    else if (get_v("qs_emridy_time") == 1000)
                    {
                        set_v("qs_emridy_time", 1500);
                        loot_items(xx: 582, yy: 514);
                        loot_items(xx: 582, yy: 514, item_proto_list: new[] { 12602 });
                        if (SelectedPartyLeader.FindItemByProto(12602) == null)
                        {
                            Utilities.create_item_in_inventory(12602, SelectedPartyLeader);
                        }

                    }

                }

            }

            // NULB	 #
            if ((cur_map == 5051)) // Nulb Outdoors
            {
                if (get_f("qs_autokill_temple"))
                {
                    SetGlobalVar(972, 2); // Simulate Convo with Kent
                }

                if (get_f("qs_autokill_nulb"))
                {
                    // Spawn assassin
                    SetGlobalFlag(277, true); // Have met assassin
                    SetGlobalFlag(292, true);
                    if (!get_f("qs_assassin_spawned"))
                    {
                        var a = GameSystems.MapObject.CreateObject(14303, SelectedPartyLeader.GetLocation());
                        lnk(name_id: 14303);
                        loot_items(loot_source_name: 14303, item_proto_list: new[] { 6315, 6199, 4701, 4500, 8007, 11002 }, item_autoconvert_list: new[] { 6046 });
                        set_f("qs_assassin_spawned");
                    }

                    SetGlobalFlag(356, true); // Met Mickey
                    SetGlobalFlag(357, true); // Mickey confessed to taking Orb
                    SetGlobalFlag(321, true); // Met Mona
                    record_time_stamp("s_skole_goons");
                    SetQuestState(41, QuestState.Completed); // Preston's Tooth Ache
                    SetGlobalFlag(94, true); // Nulb House is yours
                    SetGlobalFlag(315, true); // Purchased Serena's Freedom
                    SetQuestState(60, QuestState.Completed); // Mona's Orb
                    SetQuestState(63, QuestState.Completed); // Bribery for justice
                    if (get_f("qs_killed_gar"))
                    {
                        SetQuestState(35, QuestState.Completed); // Grud's story
                        SelectedPartyLeader.AddReputation(25);
                    }

                }

            }

            if ((cur_map == 5068)) // Imeryd's Run
            {
                if (get_f("qs_autokill_nulb"))
                {
                    lnk(xx: 485, yy: 455, name_ids: new HashSet<int> { 14279, 14084..14088}); // Hag & Lizards
                                                                                             // lnk(xx = 468, yy = 467, name_id = ([14279] + range(14084, 14088))  ) # Hag & Lizards
                    loot_items(xx: 485, yy: 455);
                    lnk(xx: 460, yy: 480, name_ids: new[] { 14329 }); // Gar
                    loot_items(xx: 460, yy: 480, item_proto_list: new[] { 12005 }); // Gar Corpse + Lamia Figurine
                    loot_items(xx: 460, yy: 500, item_proto_list: new[] { 12005 }); // Lamia Figurine - bulletproof
                    set_f("qs_killed_gar");
                    lnk(name_ids: new[] { 14445, 14057 }); // Kingfrog, Giant Frog
                    loot_items(xx: 476, yy: 497, item_proto_list: new[] { 4082, 6199, 6082, 4191, 6215, 5006 });
                }

            }

            if ((cur_map == 5052)) // Boatmen's Tavern
            {
                if (get_f("qs_autokill_nulb"))
                {
                    if (GetGlobalFlag(281)) // Have had Skole Goons Encounter
                    {
                        lnk(name_ids: new[] { 14315, 14134 }); // Skole + Goon
                        loot_items(loot_source_names: new[] { 14315, 14134 }, item_proto_list: new[] { 6051, 4121 });
                        foreach (var obj_1 in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC))
                        {
                            foreach (var pc_1 in PartyLeader.GetPartyMembers())
                            {
                                obj_1.AIRemoveFromShitlist(pc_1);
                                obj_1.SetReaction(pc_1, 50);
                            }

                        }

                    }

                }

            }

            if ((cur_map == 5057)) // Snakepit Brothel
            {
                if (get_f("qs_autokill_nulb"))
                {
                    if (!is_timed_autokill)
                    {
                        StartTimer(100, () => autokill(cur_map, true, true));
                    }
                    else
                    {
                        lnk(xx: 508, yy: 485, name_id: 8718);
                        loot_items(xx: 508, yy: 485, loot_source_name: 8718, item_autoconvert_list: new[] { 4443, 6040, 6229 });
                        SetGlobalVar(961, 6);
                    }

                }

            }

            if ((cur_map == 5060)) // Waterside Hostel
            {
                if (get_f("qs_autokill_nulb"))
                {
                    if (!is_timed_autokill)
                    {
                        StartTimer(100, () => autokill(cur_map, true, true));
                    }
                    else
                    {
                        // Thieving Dala
                        SetQuestState(37, QuestState.Completed);
                        lnk(xx: 480, yy: 501, name_ids: new[] { 14147, 14146, 14145, 8018, 14074 }, stun_name_ids: new[] { 14372, 14373 });
                        loot_items(xx: 480, yy: 501, loot_source_names: new[] { 14147, 14146, 14145, 8018, 14074 });
                        foreach (var obj_1 in ObjList.ListVicinity(new locXY(480, 501), ObjectListFilter.OLC_NPC))
                        {
                            foreach (var pc_1 in PartyLeader.GetPartyMembers())
                            {
                                obj_1.AIRemoveFromShitlist(pc_1);
                                obj_1.SetReaction(pc_1, 50);
                            }

                        }

                    }

                }

            }

            // HICKORY BRANCH	 #
            if ((cur_map == 5095)) // Hickory Branch Exterior
            {
                if (get_f("qs_autokill_nulb"))
                {
                    // First party, near Noblig
                    lnk(xx: 433, yy: 538, name_ids: new[] { 14467, 14469, 14470, 14468, 14185 });
                    loot_items(xx: 433, yy: 538, item_autoconvert_list: new[] { 4201, 4209, 4116, 6321 }); // Shortbow, Spiked Chain, Short Spear, Marauder Armor
                                                                                                           // NW of Noblig
                    lnk(xx: 421, yy: 492, name_ids: new[] { 14467, 14469, 14470, 14468, 14185, 14050, 14391 });
                    loot_items(xx: 421, yy: 492, item_autoconvert_list: new[] { 4201, 4209, 4116 });
                    // Wolf Trainer Group
                    lnk(xx: 366, yy: 472, name_ids: new[] { 14466, 14352, 14467, 14469, 14470, 14468, 14185, 14050, 14391 });
                    loot_items(xx: 366, yy: 472, item_autoconvert_list: new[] { 4201, 4209, 4116 });
                    // Ogre Shaman Group
                    lnk(xx: 449, yy: 455, name_ids: new[] { 14249, 14482, 14093, 14067, 14466, 14352, 14467, 14469, 14470, 14468, 14185, 14050, 14391 });
                    loot_items(xx: 449, yy: 455, item_autoconvert_list: new[] { 4201, 4209, 4116 });
                    // Orc Shaman Group
                    lnk(xx: 494, yy: 436, name_ids: new[] { 14743, 14747, 14749, 14745, 14746, 14482, 14093, 14067, 14466, 14352, 14467, 14469, 14470, 14468, 14185, 14050, 14391 });
                    loot_items(xx: 494, yy: 436, item_autoconvert_list: new[] { 4201, 4209, 4116 });
                    // Cave Entrance Group
                    lnk(xx: 527, yy: 380, name_ids: new[] { 14465, 14249, 14743, 14747, 14749, 14745, 14746, 14482, 14093, 14067, 14466, 14352, 14467, 14469, 14470, 14468, 14185, 14050, 14391 });
                    loot_items(xx: 527, yy: 380, item_autoconvert_list: new[] { 4201, 4209, 4116 });
                    // Dire Bear
                    lnk(xx: 548, yy: 430, name_ids: new[] { 14506 });
                    // Cliff archers
                    lnk(xx: 502, yy: 479, name_ids: new[] { 14465, 14249, 14743, 14747, 14749, 14745, 14746, 14482, 14093, 14067, 14466, 14352, 14467, 14469, 14470, 14468, 14185, 14050, 14391 });
                    loot_items(xx: 502, yy: 479, item_autoconvert_list: new[] { 4201, 4209, 4116 });
                    // Giant Snakes
                    lnk(xx: 547, yy: 500, name_ids: new[] { 14449 });
                    loot_items(xx: 547, yy: 500, item_autoconvert_list: new[] { 4201, 4209, 4116 });
                    // Owlbear
                    lnk(xx: 607, yy: 463, name_ids: new[] { 14046 });
                    // Dokolb area
                    lnk(xx: 450, yy: 519, name_ids: new[] { 14640, 14465, 14249, 14743, 14747, 14749, 14745, 14746, 14482, 14093, 14067, 14466, 14352, 14467, 14469, 14470, 14468, 14185, 14050, 14391 });
                    loot_items(xx: 450, yy: 519, item_autoconvert_list: new[] { 4201, 4209, 4116 });
                    // South of Dokolb Area
                    lnk(xx: 469, yy: 548, name_ids: new[] { 14188, 14465, 14249, 14743, 14747, 14749, 14745, 14746, 14482, 14093, 14067, 14466, 14352, 14467, 14469, 14470, 14468, 14185, 14050, 14391 });
                    loot_items(xx: 469, yy: 548, item_autoconvert_list: new[] { 4201, 4209, 4116 });
                }

            }

            if ((cur_map == 5115)) // Hickory Branch Cave
            {
                if (get_f("qs_autokill_nulb"))
                {
                    if (get_v("qs_hickory_cave_timer") == 0)
                    {
                        set_v("qs_hickory_cave_timer", 500);
                        StartTimer(500, () => autokill(cur_map));
                    }

                    if (get_v("qs_hickory_cave_timer") == 500)
                    {
                        lnk();
                        loot_items(item_proto_list: new[] { 4086, 6106, 10023 }, item_autoconvert_list: new[] { 6143, 4110, 4241, 4242, 4243, 6066, 4201, 4209, 4116 });
                        loot_items(xx: 490, yy: 453, item_proto_list: new[] { 4078, 6252, 6339, 6091 }, item_autoconvert_list: new[] { 6304, 4240, 6161, 6160, 4087, 4204 });
                    }

                }

            }

            if ((cur_map == 5191)) // Minotaur Lair
            {
                if (get_f("qs_autokill_nulb"))
                {
                    lnk(xx: 492, yy: 486);
                    loot_items(xx:492, yy:490, item_proto_list: new[] { 4238, 6486, 6487 });
                }

            }

            // ARENA OF HEROES	 #
            if ((cur_map == 5119)) // AoH
            {
                if (get_f("qs_autokill_temple"))
                {
                    // game.global_vars[994] = 3
                    var dummy = 1;
                }

            }

            // MOATHOUSE RESPAWN	 #
            if ((cur_map == 5120)) // Forest Drow
            {
                // flash_signal(0)
                if (get_f("qs_autokill_temple"))
                {
                    lnk(xx: 484, yy: 481, name_ids: new[] { 14677, 14733, 14725, 14724, 14726 });
                    loot_items(xx: 484, yy: 481, item_autoconvert_list: new[] { 4132, 6057, 4082, 4208, 6076 });
                }

            }

            // TEMPLE OF ELEMENTAL EVIL	 #
            if ((cur_map == 5111)) // Tower Sentinel
            {
                if (get_f("qs_autokill_temple"))
                {
                    lnk(xx: 480, yy: 490, name_id: 14157);
                    loot_items(xx: 480, yy: 490);
                }

            }

            if ((cur_map == 5065)) // Brigand Tower
            {
                if (get_f("qs_autokill_temple"))
                {
                    lnk(xx: 477, yy: 490, name_ids: new[] { 14314, 14313, 14312, 14310, 14424, 14311, 14425 });
                    lnk(xx: 490, yy: 480, name_ids: new[] { 14314, 14313, 14312, 14310, 14424, 14311, 14425 });
                    loot_items(item_proto_list: new[] { 10005, 6051 }, item_autoconvert_list: new[] { 4081, 6398, 4067 });
                    loot_items(xx: 490, yy: 480, item_proto_list: new[] { 10005, 6051 }, item_autoconvert_list: new[] { 4081, 6398, 4067, 4070, 4117, 5011 });
                }

            }

            if ((cur_map == 5066)) // Temple Level 1 - Earth Floor
            {
                if (get_f("qs_autokill_temple"))
                {
                    if (!is_timed_autokill)
                    {
                        StartTimer(100, () => autokill(cur_map, true, true));
                    }
                    else
                    {
                        // Stirges
                        lnk(xx: 415, yy: 490, name_ids: new[] { 14182 });
                        // Harpies & Ghouls
                        lnk(xx: 418, yy: 574, name_ids: new[] { 14095, 14129, 14243, 14128, 14136, 14135 });
                        lnk(xx: 401, yy: 554, name_ids: new[] { 14095, 14129, 14243, 14128, 14136, 14135 });
                        lnk(xx: 401, yy: 554, name_ids: new[] { 14095, 14129, 14243, 14128, 14136, 14135 });
                        lnk(xx: 421, yy: 544, name_ids: new[] { 14095, 14129, 14243, 14128, 14136, 14135 });
                        lnk(xx: 413, yy: 522, name_ids: new[] { 14095, 14129, 14243, 14128, 14136, 14135 });
                        loot_items(xx: 401, yy: 554);
                        // Gel Cube + Grey Ooze
                        lnk(xx: 407, yy: 594, name_ids: new[] { 14095, 14129, 14139, 14140 });
                        loot_items(xx: 407, yy: 600, loot_source_names: new[] { 14448, 1049 }, item_autoconvert_list: new[] { 4121, 4118, 4113, 4116, 5005, 5098 });
                        // Corridor Ghouls
                        lnk(xx: 461, yy: 600, name_ids: new[] { 14095, 14129 });
                        // Corridor Gnolls
                        lnk(xx: 563, yy: 600, name_ids: new[] { 14078, 14079, 14080 });
                        loot_items(xx: 563, yy: 600, loot_source_names: new[] { 14078, 14079, 14080, 1049 });
                        // Corridor Ogre
                        lnk(xx: 507, yy: 600, name_ids: new[] { 14448 });
                        loot_items(xx: 507, yy: 600, loot_source_names: new[] { 14448, 1049 }, item_autoconvert_list: new[] { 4121, 4118, 4113, 4116, 5005, 5098 });
                        // Bone Corridor Undead
                        lnk(xx: 497, yy: 519, name_ids: new[] { 14107, 14081, 14082 });
                        lnk(xx: 467, yy: 519, name_ids: new[] { 14083, 14107, 14081, 14082 });
                        loot_items(xx: 507, yy: 600, loot_source_names: new[] { 14107, 14081, 14082 });
                        // Wonnilon Undead
                        lnk(xx: 536, yy: 414, name_ids: new[] { 14127, 14126, 14125, 14124, 14092, 14123 });
                        lnk(xx: 536, yy: 444, name_ids: new[] { 14127, 14126, 14125, 14124, 14092, 14123 });
                        // Huge Viper
                        lnk(xx: 550, yy: 494, name_ids: new[] { 14088 });
                        // Ogre + Goblins
                        lnk(xx: 565, yy: 508, name_ids: new[] { 14185, 14186, 14187, 14448 });
                        lnk(xx: 565, yy: 494, name_ids: new[] { 14185, 14186, 14187, 14448 });
                        loot_items(xx: 565, yy: 508, loot_source_names: new[] { 14185, 14186, 14187, 14448 });
                        // Ghasts near prisoners
                        lnk(xx: 545, yy: 553, name_ids: new[] { 14128, 14129, 14136, 14095, 14137, 14135 });
                        loot_items(xx: 545, yy: 553, loot_source_names: new[] { 1040 });
                        // Black Widow Spiders
                        lnk(xx: 440, yy: 395, name_ids: new[] { 14417 });
                        // NW Ghast room near hideout
                        lnk(xx: 390, yy: 390, name_ids: new[] { 14128, 14129, 14136, 14095, 14137, 14135 });
                        if (get_v("qs_autokill_temple_level_1_stage") == 0)
                        {
                            set_v("qs_autokill_temple_level_1_stage", 1);
                        }
                        else if (get_v("qs_autokill_temple_level_1_stage") == 1)
                        {
                            set_v("qs_autokill_temple_level_1_stage", 2);
                            // Gnoll & Bugbear southern room
                            lnk(xx: 515, yy: 535, name_ids: new[] { 14078, 14249, 14066, 14632, 14164 });
                            lnk(xx: 515, yy: 549, name_ids: new[] { 14067, 14631, 14078, 14249, 14066, 14632, 14164 });
                            loot_items(xx: 515, yy: 540);
                            // Gnoll & Bugbear northern room
                            lnk(xx: 463, yy: 535, name_ids: new[] { 14248, 14631, 14188, 14636, 14083, 14184, 14078, 14249, 14066, 14632, 14164 });
                            loot_items(xx: 463, yy: 535);
                            // Earth Temple Fighter eastern room
                            lnk(xx: 438, yy: 505, name_ids: new[] { 14337, 14338 });
                            loot_items(xx: 438, yy: 505, item_autoconvert_list: new[] { 6074, 6077, 5005, 4123, 4134 });
                            // Bugbear Central Outpost
                            lnk(xx: 505, yy: 476, name_ids: new[] { 14165, 14163, 14164, 14162 });
                            loot_items(xx: 505, yy: 476);
                            // Bugbears nea r Wonnilon
                            lnk(xx: 555, yy: 436, name_ids: new[] { 14165, 14163, 14164, 14162 });
                            lnk(xx: 555, yy: 410, name_ids: new[] { 14165, 14163, 14164, 14162 });
                            lnk(xx: 519, yy: 416, name_ids: new[] { 14165, 14163, 14164, 14162 });
                            loot_items(xx: 519, yy: 416, loot_source_names: new HashSet<int>{14162..14166}, item_autoconvert_list: new[] { 6174 });
                            loot_items(xx: 555, yy: 436, loot_source_names: new[] { 14164 }, item_autoconvert_list: new[] { 6174 });
                            loot_items(xx: 555, yy: 410, loot_source_names: new[] { 14164 }, item_autoconvert_list: new[] { 6174 });
                            // Bugbears North of Romag
                            lnk(xx: 416, yy: 430, name_ids: new HashSet<int>{14162..14166});
                            loot_items(xx: 416, yy: 430, loot_source_names: new HashSet<int>{14162..14166}, item_autoconvert_list: new[] { 6174 });
                        }
                        else if (get_v("qs_autokill_temple_level_1_stage") == 2)
                        {
                            // Jailer room
                            lnk(xx: 568, yy: 462, name_ids: new[] { 14165, 14164, 14229 });
                            loot_items(xx: 568, yy: 462, item_autoconvert_list: new[] { 6174 });
                            // Earth Altar
                            lnk(xx: 474, yy: 396, name_ids: new[] { 14381, 14337 });
                            lnk(xx: 494, yy: 396, name_ids: new[] { 14381, 14337 });
                            lnk(xx: 484, yy: 423, name_ids: new[] { 14296 });
                            loot_items(xx: 480, yy: 400, loot_source_names: new HashSet<int>{1041..1045}, item_proto_list: new[] { 6082, 12228, 12031 }, item_autoconvert_list: new[] { 4070, 4193, 6056, 8025 });
                            loot_items(xx: 480, yy: 400, item_proto_list: new[] { 6082, 12228, 12031 }, item_autoconvert_list: new[] { 4070, 4193, 6056, 8025 });
                            // Troop Commander room
                            lnk(xx: 465, yy: 477, name_ids: new HashSet<int>{14162..14166, 14337, 14156, 14339});
                            lnk(xx: 450, yy: 477, name_ids: new HashSet<int>{14162..14166, 14337, 14156, 14339});
                            loot_items(xx: 450, yy: 476, item_autoconvert_list: new[] { 4098, 6074, 6077, 6174 });
                            // Romag Room
                            lnk(xx: 441, yy: 442, name_ids: new HashSet<int> { 8045, 14154, 14162..14166, 14337, 14156, 14339 });
                            loot_items(xx: 441, yy: 442, item_autoconvert_list: new[] { 6164, 9359, 8907, 9011 }, item_proto_list: new[] { 10006, 6094, 4109, 8008 });
                        }

                    }

                }

            }

            if ((cur_map == 5067)) // Temple Level 2 - Water, Fire & Air Floor
            {
                if (get_f("qs_autokill_temple"))
                {
                    if (!is_timed_autokill)
                    {
                        StartTimer(100, () => autokill(cur_map, true, true));
                    }
                    else
                    {
                        // Kelno regroup
                        lnk(xx: 480, yy: 494, name_ids: new[] { 8092, 14380, 14292, 14067, 14078, 14079, 14080, 14184, 14187, 14215, 14216, 14275, 14159, 14160, 14161, 14158 });
                        lnk(xx: 490, yy: 494, name_ids: new[] { 8092, 14380, 14292, 14067, 14078, 14079, 14080, 14184, 14187, 14215, 14216, 14275, 14159, 14160, 14161, 14158 });
                        lnk(xx: 490, yy: 514, name_ids: new[] { 8092, 14380, 14292, 14067, 14078, 14079, 14080, 14184, 14187, 14215, 14216, 14275, 14159, 14160, 14161, 14158 });
                        loot_items(xx: 480, yy: 494, item_proto_list: new[] { 10009, 6085, 4219 }, item_autoconvert_list: new[] { 6049, 4109, 6166, 4112 });
                        loot_items(xx: 480, yy: 514, item_proto_list: new[] { 10009, 6085, 4219 }, item_autoconvert_list: new[] { 6049, 4109, 6166, 4112 });
                        loot_items(xx: 490, yy: 514, item_proto_list: new[] { 10009, 6085, 4219 }, item_autoconvert_list: new[] { 6049, 4109, 6166, 4112 });
                        // Corridor Ogres
                        lnk(xx: 480, yy: 452, name_ids: new[] { 14249, 14353 });
                        loot_items(xx: 480, yy: 452, item_autoconvert_list: new[] { 4134 });
                        // Minotaur
                        foreach (var m_stat in ObjList.ListVicinity(new locXY(566, 408), ObjectListFilter.OLC_SCENERY))
                        {
                            if (m_stat.GetNameId() == 1615)
                            {
                                m_stat.Destroy();
                                cnk(14241);
                                loot_items(xx: 566, yy: 408);
                            }

                        }

                        // Greater Temple Guards
                        lnk(xx: 533, yy: 398, name_ids: new[] { 14349, 14348 });
                        lnk(xx: 550, yy: 422, name_ids: new[] { 14349, 14348 });
                        loot_items(xx: 533, yy: 398);
                        // Littlest Troll
                        lnk(xx: 471, yy: 425, name_ids: new[] { 14350 });
                        // Carrion Crawler
                        lnk(xx: 451, yy: 424, name_ids: new[] { 14190 });
                        // Fire Temple Bugbears Outside
                        lnk(xx: 397, yy: 460, name_ids: new[] { 14169 });
                        loot_items(xx: 397, yy: 460, loot_source_names: new[] { 14169 });
                        if (get_v("qs_autokill_temple_level_2_stage") == 0)
                        {
                            set_v("qs_autokill_temple_level_2_stage", 1);
                        }
                        else if (get_v("qs_autokill_temple_level_2_stage") == 1)
                        {
                            set_v("qs_autokill_temple_level_2_stage", 2);
                            // Feldrin
                            lnk(xx: 562, yy: 438, name_ids: new[] { 14311, 14312, 14314, 8041, 14253 });
                            loot_items(xx: 562, yy: 438, item_proto_list: new[] { 6083, 10010, 4082, 6086, 8010 }, item_autoconvert_list: new[] { 6091, 4070, 4117, 4114, 4062, 9426, 8014 });
                            // Prisoner Guards - Ogre + Greater Temple Bugbear
                            lnk(xx: 410, yy: 440, name_ids: new[] { 8065 });
                            loot_items(xx: 410, yy: 440, loot_source_names: new[] { 8065 });
                        }
                        else if (get_v("qs_autokill_temple_level_2_stage") == 2)
                        {
                            set_v("qs_autokill_temple_level_2_stage", 3);
                            // Water Temple
                            lnk(xx: 541, yy: 573, name_ids: new[] { 14375, 14231, 8091, 14247, 8028, 8027, 14181, 14046, 14239, 14225 });
                            // Juggernaut
                            lnk(xx: 541, yy: 573, name_ids: new[] { 14244 });
                            loot_items(xx: 541, yy: 573, item_proto_list: new[] { 10008, 6104, 4124, 6105, 9327, 9178 }, item_autoconvert_list: new[] { 6039, 9508, 9400, 6178, 6170, 9546, 9038, 9536 });
                            // Oohlgrist
                            lnk(xx: 483, yy: 614, name_ids: new[] { 14262, 14195 });
                            loot_items(xx: 483, yy: 614, item_proto_list: new[] { 6101, 6107 }, item_autoconvert_list: new[] { 6106, 12014, 6108 });
                            // Salamanders
                            lnk(xx: 433, yy: 583, name_ids: new[] { 8063, 14384, 14111 });
                            lnk(xx: 423, yy: 583, name_ids: new[] { 8063, 14384, 14111 });
                            loot_items(xx: 433, yy: 583, item_proto_list: new[] { 4028, 12016, 6101, 4136 }, item_autoconvert_list: new[] { 6121, 8020 });
                        }
                        else if (get_v("qs_autokill_temple_level_2_stage") == 3)
                        {
                            set_v("qs_autokill_temple_level_2_stage", 4);
                            // Alrrem
                            lnk(xx: 415, yy: 499, name_ids: new[] { 14169, 14211, 8047, 14168, 14212, 14167, 14166, 14344, 14224, 14343 });
                            loot_items(xx: 415, yy: 499, item_proto_list: new[] { 10007, 4079, 6082 }, item_autoconvert_list: new[] { 6094, 6060, 6062, 6068, 6069, 6335, 6269, 6074, 6077, 6093, 6167, 6177, 6172, 8019, 6039, 4131, 6050, 4077, 6311 });
                        }
                        else if (get_v("qs_autokill_temple_level_2_stage") == 4)
                        {
                            set_v("qs_autokill_temple_level_2_stage", 5);
                            // Big Bugbear Room
                            lnk(xx: 430, yy: 361, name_ids: new HashSet<int>{14174..14178, 14213, 14214, 14215, 14216 });
                            lnk(xx: 430, yy: 391, name_ids: new HashSet<int>{14174..14178, 14213, 14214, 14215, 14216 });
                            loot_items(xx: 430, yy: 361, item_autoconvert_list: new[] { 6093, 6173, 6168, 6163, 6056 });
                            loot_items(xx: 430, yy: 391, item_autoconvert_list: new[] { 6093, 6173, 6168, 6163, 6056 });
                        }

                    }

                }

            }

            if ((cur_map == 5105)) // Temple Level 3 - Thrommel Floor
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    if (!is_timed_autokill)
                    {
                        StartTimer(100, () => autokill(cur_map, true, true));
                    }
                    else
                    {
                        // Northern Trolls
                        lnk(xx: 394, yy: 401, name_ids: new[] { 14262 });
                        // Shadows
                        lnk(xx: 369, yy: 431, name_ids: new[] { 14289 });
                        lnk(xx: 369, yy: 451, name_ids: new[] { 14289 });
                        // Ogres:
                        lnk(xx: 384, yy: 465, name_ids: new[] { 14249 });
                        loot_items(xx: 384, yy: 465);
                        // Ettin:
                        lnk(xx: 437, yy: 524, name_ids: new[] { 14238 });
                        loot_items(xx: 437, yy: 524);
                        // Yellow Molds:
                        lnk(xx: 407, yy: 564, name_ids: new[] { 14276 });
                        // Groaning Spirit:
                        lnk(xx: 441, yy: 459, name_ids: new[] { 14280 });
                        loot_items(xx: 441, yy: 459, item_proto_list: new[] { 4218, 6090 }, item_autoconvert_list: new[] { 9214, 4191, 6058, 9123, 6214, 9492, 9391, 4002 });
                        // Key Trolls:
                        lnk(xx: 489, yy: 535, name_ids: new[] { 14262 });
                        lnk(xx: 489, yy: 504, name_ids: new[] { 14262 });
                        loot_items(xx: 489, yy: 504, item_proto_list: new HashSet<int>{10016..10020});
                        loot_items(xx: 489, yy: 535, item_proto_list: new HashSet<int>{10016..10020});
                        // Will o Wisps:
                        lnk(xx: 551, yy: 583, name_ids: new[] { 14291 });
                        // Lamia:
                        lnk(xx: 584, yy: 594, name_ids: new[] { 14342, 14274 });
                        loot_items(xx: 584, yy: 594, item_proto_list: new[] { 4083 });
                        // Jackals, Werejackals & Gargoyles:
                        lnk(xx: 511, yy: 578, name_ids: new[] { 14051, 14239, 14138 });
                        lnk(xx: 528, yy: 556, name_ids: new[] { 14051, 14239, 14138 });
                        // UmberHulks
                        lnk(xx: 466, yy: 565, name_ids: new[] { 14260 });
                        if (get_v("qs_autokill_temple_level_3_stage") == 0)
                        {
                            set_v("qs_autokill_temple_level_3_stage", 1);
                        }
                        else if (get_v("qs_autokill_temple_level_3_stage") == 1)
                        {
                            set_v("qs_autokill_temple_level_3_stage", 2);
                            // Gel Cube
                            lnk(xx: 476, yy: 478, name_ids: new[] { 14139 });
                            // Black Pudding
                            lnk(xx: 442, yy: 384, name_ids: new[] { 14143 });
                            // Goblins:
                            lnk(xx: 491, yy: 389, name_ids: new HashSet<int>{14183..14188, 14219, 14217 });
                            loot_items(xx: 491, yy: 389);
                            // Carrion Crawler:
                            lnk(xx: 524, yy: 401, name_ids: new[] { 14190 });
                            // Ogres near thrommel:
                            lnk(xx: 569, yy: 412, name_ids: new[] { 14249, 14353 });
                            loot_items(xx: 569, yy: 412, loot_source_names: new[] { 14249, 14353 }, item_autoconvert_list: new[] { 4134 });
                            // Leucrottas:
                            lnk(xx: 405, yy: 590, name_ids: new[] { 14351 });
                        }
                        else if (get_v("qs_autokill_temple_level_3_stage") == 2)
                        {
                            set_v("qs_autokill_temple_level_3_stage", 3);
                            // Pleasure dome:
                            lnk(xx: 553, yy: 492, name_ids: new[] { 14346, 14174, 14249, 14176, 14353, 14175, 14352, 14177 });
                            lnk(xx: 540, yy: 480, name_ids: new[] { 14346, 14174, 14249, 14176, 14353, 14175, 14352, 14177 });
                            lnk(xx: 569, yy: 485, name_ids: new[] { 8034, 14346, 14249, 14174, 14176, 14353, 14175, 14352, 14177 });
                            loot_items(xx: 540, yy: 480, loot_source_names: new[] { 8034, 14346, 14249, 14174, 14176, 14353, 14175, 14352, 14177 }, item_autoconvert_list: new[] { 6334 });
                            loot_items(xx: 553, yy: 492, loot_source_names: new[] { 8034, 14346, 14249, 14174, 14176, 14353, 14175, 14352, 14177 }, item_autoconvert_list: new[] { 6334 });
                            loot_items(xx: 569, yy: 485, loot_source_names: new[] { 8034, 14346, 14249, 14174, 14176, 14353, 14175, 14352, 14177 }, item_autoconvert_list: new[] { 6334 });
                            SetGlobalFlag(164, true); // Turns on Bugbears
                        }
                        else if (get_v("qs_autokill_temple_level_3_stage") == 3)
                        {
                            set_v("qs_autokill_temple_level_3_stage", 4);
                            // Pleasure dome - make sure:
                            lnk(xx: 553, yy: 492, name_ids: new[] { 14346, 14174, 14249, 14176, 14353, 14175, 14352, 14177 });
                            lnk(xx: 540, yy: 480, name_ids: new[] { 14346, 14174, 14249, 14176, 14353, 14175, 14352, 14177 });
                            lnk(xx: 569, yy: 485, name_ids: new[] { 8034, 14346, 14249, 14174, 14176, 14353, 14175, 14352, 14177 });
                            // Smigmal & Falrinth
                            var ass1 = GameSystems.MapObject.CreateObject(14782, new locXY(614, 455));
                            var ass2 = GameSystems.MapObject.CreateObject(14783, new locXY(614, 455));
                            lnk(xx: 614, yy: 455, name_ids: new[] { 14232, 14782, 14783 });
                            loot_items(xx: 614, yy: 455, item_proto_list: new[] { 10011, 6125, 6088 }, item_autoconvert_list: new[] { 4126, 6073, 6335, 8025 });
                            lnk(xx: 614, yy: 480, name_ids: new[] { 14110, 14177, 14346, 20123 });
                            loot_items(xx: 619, yy: 480, item_proto_list: new[] { 12560, 10012, 6119 }, item_autoconvert_list: new[] { 4179, 9173 });
                            loot_items(xx: 612, yy: 503, loot_source_names: new[] { 1033 }, item_proto_list: new[] { 12560, 10012, 6119 }, item_autoconvert_list: new[] { 4179, 9173 });
                        }

                    }

                }

            }

            if ((cur_map == 5080)) // Temple Level 4 - Greater Temple
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    if (!is_timed_autokill)
                    {
                        StartTimer(100, () => autokill(cur_map, true, true));
                    }
                    else
                    {
                        SetGlobalFlag(820, true); // Trap Disabled
                        SetGlobalFlag(148, true); // Paida Sane
                                                  // Eastern Trolls
                        lnk(xx: 452, yy: 552, name_ids: new[] { 14262 });
                        // Western Trolls
                        lnk(xx: 513, yy: 552, name_ids: new[] { 14262 });
                        // Troll + Ettin
                        lnk(xx: 522, yy: 586, name_ids: new[] { 14262, 14238 });
                        loot_items(xx: 522, yy: 586);
                        // Hill Giants
                        lnk(xx: 570, yy: 610, name_ids: new[] { 14218, 14217, 14219 });
                        loot_items(xx: 570, yy: 610);
                        // Ettins
                        lnk(xx: 587, yy: 580, name_ids: new[] { 14238 });
                        loot_items(xx: 587, yy: 580);
                        // More Trolls
                        lnk(xx: 555, yy: 546, name_ids: new[] { 14262 });
                        if (get_v("qs_autokill_temple_level_4_stage") == 0)
                        {
                            set_v("qs_autokill_temple_level_4_stage", 1);
                        }
                        else if (get_v("qs_autokill_temple_level_4_stage") == 1)
                        {
                            set_v("qs_autokill_temple_level_4_stage", 2);
                            // Bugbear quarters
                            lnk(xx: 425, yy: 591, name_ids: new[] { 14174, 14175, 14176, 14177, 14249, 14347, 14346 });
                            lnk(xx: 435, yy: 591, name_ids: new[] { 14174, 14175, 14176, 14177, 14249, 14347, 14346 });
                            lnk(xx: 434, yy: 603, name_ids: new[] { 14174, 14175, 14176, 14177, 14249, 14347, 14346 });
                            lnk(xx: 405, yy: 603, name_ids: new[] { 14174, 14175, 14176, 14177, 14249, 14347, 14346 });
                            loot_items(xx: 435, yy: 590);
                            loot_items(xx: 425, yy: 590);
                            loot_items(xx: 435, yy: 603);
                            loot_items(xx: 405, yy: 603);
                        }
                        else if (get_v("qs_autokill_temple_level_4_stage") == 2)
                        {
                            set_v("qs_autokill_temple_level_4_stage", 3);
                            // Insane Ogres
                            lnk(xx: 386, yy: 584, name_ids: new[] { 14356, 14355, 14354 });
                            loot_items(xx: 386, yy: 584);
                            // Senshock's Posse
                            lnk(xx: 386, yy: 528, name_ids: new[] { 14296, 14298, 14174, 14110, 14302, 14292 });
                            foreach (var obj_1 in ObjList.ListVicinity(new locXY(386, 528), ObjectListFilter.OLC_NPC))
                            {
                                foreach (var pc_1 in PartyLeader.GetPartyMembers())
                                {
                                    obj_1.AIRemoveFromShitlist(pc_1);
                                    obj_1.SetReaction(pc_1, 50);
                                }

                            }

                            loot_items(xx: 386, yy: 528);
                        }
                        else if (get_v("qs_autokill_temple_level_4_stage") == 3)
                        {
                            set_v("qs_autokill_temple_level_4_stage", 4);
                            // Hedrack's Posse
                            lnk(xx: 493, yy: 442, name_ids: new[] { 14238, 14239, 14218, 14424, 14296, 14298, 14174, 14176, 14177, 14110, 14302, 14292 });
                            foreach (var obj_1 in ObjList.ListVicinity(new locXY(493, 442), ObjectListFilter.OLC_NPC))
                            {
                                foreach (var pc_1 in PartyLeader.GetPartyMembers())
                                {
                                    obj_1.AIRemoveFromShitlist(pc_1);
                                    obj_1.SetReaction(pc_1, 50);
                                }

                            }

                            loot_items(xx: 493, yy: 442);
                            lnk(xx: 465, yy: 442, name_ids: new[] { 14238, 14239, 14218, 14424, 14296, 14298, 14174, 14176, 14177, 14110, 14302, 14292 });
                            foreach (var obj_1 in ObjList.ListVicinity(new locXY(493, 442), ObjectListFilter.OLC_NPC))
                            {
                                foreach (var pc_1 in PartyLeader.GetPartyMembers())
                                {
                                    obj_1.AIRemoveFromShitlist(pc_1);
                                    obj_1.SetReaction(pc_1, 50);
                                }

                            }

                            loot_items(xx: 493, yy: 442);
                            // Fungi
                            lnk(xx: 480, yy: 375, name_ids: new[] { 14274, 14143, 14273, 14276, 14142, 14141, 14282 });
                            loot_items(xx: 484, yy: 374);
                            loot_items(xx: 464, yy: 374);
                            lnk(xx: 480, yy: 353, name_ids: new[] { 14277, 14140 });
                        }

                    }

                }

            }

            // NODES						 #
            if ((cur_map == 5083)) // Fire Node
            {
                if (get_f("qs_autokill_nodes"))
                {
                    // Fire Toads
                    lnk(xx: 535, yy: 525, name_ids: new[] { 14300 });
                    // Bodaks
                    lnk(xx: 540, yy: 568, name_ids: new[] { 14328 });
                    // Salamanders
                    lnk(xx: 430, yy: 557, name_ids: new[] { 14111 });
                    // Salamanders near Balor
                    lnk(xx: 465, yy: 447, name_ids: new[] { 14111 });
                    // Efreeti
                    lnk(xx: 449, yy: 494, name_ids: new[] { 14340 });
                    // Fire Elementals + Snakes
                    lnk(xx: 473, yy: 525, name_ids: new[] { 14298, 14626 });
                    lnk(xx: 462, yy: 532, name_ids: new[] { 14298, 14626 });
                }

            }

            // VERBOBONC		 #
            if ((cur_map == 5154)) // Scarlett Bro bottom floor
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    SetGlobalFlag(984, true); // Skip starter convo
                    SetGlobalFlag(982, true);
                }

            }

            if ((cur_map == 5152)) // Prince Zook quarters
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    SetGlobalFlag(969, true); // Met prince Zook
                    SetGlobalFlag(985, true); // Mention Drow Problem
                    SetQuestState(69, QuestState.Accepted);
                    SetGlobalFlag(981, true); // Zook said Lerrick mean
                    SetGlobalVar(977, 1); // Zook said talk to Absalom abt Lerrick
                    if (GetGlobalVar(999) >= 15)
                    {
                        SetQuestState(69, QuestState.Completed);
                    }

                }

            }

            if ((cur_map == 5126)) // Drow Caves I - spidersfest
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    // Spidors 1
                    lnk(xx: 465, yy: 471, name_ids: new[] { 14399, 14397 });
                    lnk(xx: 451, yy: 491, name_ids: new[] { 14399, 14397 });
                    lnk(xx: 471, yy: 491, name_ids: new[] { 14399, 14397 });
                    lnk(xx: 437, yy: 485, name_ids: new[] { 14741, 14397 });
                    if (!is_timed_autokill)
                    {
                        StartTimer(100, () => autokill(cur_map, true, true));
                    }
                    else
                    {
                        // Key
                        loot_items(item_proto_list: new[] { 10022 }, loot_money_and_jewels_also: false);
                    }

                }

                return;
            }

            if ((cur_map == 5127)) // Drow Caves II - 2nd spidersfest
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    // Spiders
                    lnk(xx: 488, yy: 477, name_ids: new[] { 14741, 14397, 14620 });
                    // Drow
                    lnk(xx: 455, yy: 485, name_ids: new[] { 14708, 14737, 14736, 14735 });
                    loot_items(xx: 455, yy: 481, item_autoconvert_list: new[] { 4132, 6057, 4082, 4208, 6076, 6046, 6045, 5011, 6040, 6041, 6120, 4193, 6160, 6161, 6334, 4081, 6223, 6073 });
                }

            }

            if ((cur_map == 5128)) // Drow Caves III - Drowfest I
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    // Garg. Spider
                    lnk(xx: 497, yy: 486, name_ids: new[] { 14524 });
                    // Drow
                    lnk(xx: 473, yy: 475, name_ids: new[] { 14399, 14708, 14737, 14736, 14735 });
                    lnk(xx: 463, yy: 485, name_ids: new[] { 14399, 14708, 14737, 14736, 14735 });
                    loot_items(xx: 475, yy: 471, item_autoconvert_list: new[] { 4132, 6057, 4082, 4208, 6076, 6046, 6045, 5011, 6040, 6041, 6120, 4193, 6160, 6161, 6334, 4081, 6223, 6073 });
                    lnk(xx: 456, yy: 487, name_ids: new[] { 14399, 14708, 14737, 14736, 14735, 14734 });
                    lnk(xx: 427, yy: 487, name_ids: new[] { 14399, 14708, 14737, 14736, 14735, 14734 });
                    loot_items(xx: 465, yy: 486, item_autoconvert_list: new[] { 4132, 6057, 4082, 4208, 6076, 6046, 6045, 5011, 6040, 6041, 6120, 4193, 6160, 6161, 6334, 4081, 6223, 6073, 6058 });
                    loot_items(xx: 425, yy: 481, item_autoconvert_list: new[] { 4132, 6057, 4082, 4208, 6076, 6046, 6045, 5011, 6040, 6041, 6120, 4193, 6160, 6161, 6334, 4081, 6223, 6073, 6058 });
                    loot_items(xx: 475, yy: 471, item_autoconvert_list: new[] { 4132, 6057, 4082, 4208, 6076, 6046, 6045, 5011, 6040, 6041, 6120, 4193, 6160, 6161, 6334, 4081, 6223, 6073, 6058 });
                    loot_items(xx: 425, yy: 481, item_proto_list: new[] { 6051, 4139, 4137 });
                }

            }

            if ((cur_map == 5129)) // Drow Caves IV - Spiders cont'd
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    lnk(xx: 477, yy: 464, name_ids: new[] { 14524, 14399, 14397 });
                    lnk(xx: 497, yy: 454, name_ids: new[] { 14524, 14399, 14397 });
                    lnk(xx: 467, yy: 474, name_ids: new[] { 14524, 14399, 14397, 14741 });
                    lnk(xx: 469, yy: 485, name_ids: new[] { 14524, 14399, 14397 });
                }

            }

            if ((cur_map == 5130)) // Drow Caves V - Young White Dragons
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    lnk(xx: 489, yy: 455, name_ids: new[] { 14707 });
                }

            }

            if ((cur_map == 5131)) // Drow Caves VI - Adult White Dragon
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    lnk(xx: 480, yy: 535, name_ids: new[] { 14999 });
                    loot_items(xx: 480, yy: 535);
                }

            }

            if ((cur_map == 5148)) // Verbobonc Jail
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    SetQuestState(79, QuestState.Accepted);
                    SetQuestState(80, QuestState.Accepted);
                    SetQuestState(81, QuestState.Accepted);
                    if (GetGlobalVar(964) == 0)
                    {
                        SetGlobalVar(964, 1);
                    }

                    if (GetGlobalFlag(956))
                    {
                        SetQuestState(79, QuestState.Completed);
                    }

                    if (GetGlobalFlag(957))
                    {
                        SetQuestState(80, QuestState.Completed);
                    }

                    if (GetGlobalFlag(958))
                    {
                        SetQuestState(81, QuestState.Completed);
                    }

                }

            }

            if ((cur_map == 5151)) // Verbobonc Great Hall
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    SetGlobalVar(979, 2); // Allows meeting with Mayor
                    SetGlobalFlag(980, true); // Got info about Verbobonc
                }

            }

            if ((cur_map == 5124)) // Spruce Goose Inn
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    if (!is_timed_autokill)
                    {
                        StartTimer(100, () => autokill(cur_map, true, true));
                    }
                    else
                    {
                        lnk(xx: 484, yy: 479, name_id: 8716); // Guntur Gladstone
                        SetGlobalVar(961, 2); // Have discussed wreaking havoc
                        loot_items(loot_source_name: 8716, item_autoconvert_list: new[] { 6202, 6306, 4126, 4161 });
                    }

                }

            }

            return;
        }
    }

}
