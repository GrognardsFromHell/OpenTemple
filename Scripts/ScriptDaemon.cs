
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
    [ObjectScript(439)]
    public class ScriptDaemon : BaseObjectScript
    {
        // Contained in this script
        // KOS monster on Temple Level 1

        private static readonly int TS_CRITTER_KILLED_FIRST_TIME = 504;
        // Robe-friendly monster on Temple Level 1

        private static readonly int TS_EARTH_CRITTER_KILLED_FIRST_TIME = 505;
        // Earth Temple human troop

        private static readonly int TS_EARTH_TROOP_KILLED_FIRST_TIME = 506;
        // Time when you crossed the threshold from killing a monster

        private static readonly int TS_CRITTER_THRESHOLD_CROSSED = 509;
        // Persistent flags/vars/strs		#
        // Uses keys starting with		#
        // 'Flaggg', 'Varrr', 'Stringgg' 	#

        public static bool get_f(string flagkey)
        {
            var flagkey_stringized = "Flaggg" + flagkey.ToString();
            var tempp = Co8PersistentData.getData/*Unknown*/(flagkey_stringized);
            if (isNone(tempp))
            {
                return false;
            }
            else
            {
                return (int)(tempp) != 0;
            }

        }
        public static void set_f(string flagkey, bool new_value = 1)
        {
            var flagkey_stringized = "Flaggg" + flagkey.ToString();
            Co8PersistentData.setData/*Unknown*/(flagkey_stringized, new_value);
        }
        public static int get_v(string varkey)
        {
            var varkey_stringized = "Varrr" + varkey.ToString();
            var tempp = Co8PersistentData.getData/*Unknown*/(varkey_stringized);
            if (isNone(tempp))
            {
                return 0;
            }
            else
            {
                return (int)(tempp);
            }

        }
        public static int set_v(string varkey, int new_value)
        {
            var varkey_stringized = "Varrr" + varkey.ToString();
            Co8PersistentData.setData/*Unknown*/(varkey_stringized, new_value);
            return get_v(varkey);
        }
        public static int inc_v(string varkey, int inc_amount = 1)
        {
            var varkey_stringized = "Varrr" + varkey.ToString();
            Co8PersistentData.setData/*Unknown*/(varkey_stringized, get_v(varkey) + inc_amount);
            return get_v(varkey);
        }
        public static string get_s(string strkey)
        {
            var strkey_stringized = "Stringgg" + strkey.ToString();
            var tempp = Co8PersistentData.getData/*Unknown*/(strkey_stringized);
            if (isNone(tempp))
            {
                return "";
            }
            else
            {
                return tempp.ToString();
            }

        }
        public static void set_s(string strkey, string new_value)
        {
            var new_value_stringized = new_value.ToString();
            var strkey_stringized = "Stringgg" + strkey.ToString();
            Co8PersistentData.setData/*Unknown*/(strkey_stringized, new_value_stringized);
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
            if (exponent > 30 || exponent < 0)
            {
                Logger.Info("error!");
            }
            else
            {
                var abc = (1 << exponent);
            }

            var tempp = attachee.GetInt(obj_f.npc_pad_i_4) | abc;
            attachee.SetInt(obj_f.npc_pad_i_4, tempp);
            return;
        }
        public static void npc_unset(GameObjectBody attachee, int flagno)
        {
            // flagno is assumed to be from 1 to 31
            var exponent = flagno - 1;
            if (exponent > 30 || exponent < 0)
            {
                Logger.Info("error!");
            }
            else
            {
                var abc = (1 << exponent);
            }

            var tempp = (attachee.GetInt(obj_f.npc_pad_i_4) | abc) - abc;
            attachee.SetInt(obj_f.npc_pad_i_4, tempp);
            return;
        }
        public static bool npc_get(GameObjectBody attachee, int flagno)
        {
            // flagno is assumed to be from 1 to 31
            var exponent = flagno - 1;
            if (exponent > 30 || exponent < 0)
            {
                Logger.Info("error!");
            }
            else
            {
                var abc = (1 << exponent);
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
                    return;
                }

            }

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

                    if (harpies_alive == 0 && (!(grate_obj == null)) && (GetGlobalVar(455) & Math.Pow(2, 6)) == 0)
                    {
                        GetGlobalVar(455) |= Math.Pow(2, 6);
                        // grate_obj.object_flag_set(OF_OFF)
                        var grate_npc = GameSystems.MapObject.CreateObject(14913, grate_obj.GetLocation());
                        grate_npc.Move(grate_obj.GetLocation(), 0, 11f);
                        grate_npc.Rotation = grate_obj.Rotation;
                    }

                }

            }

            // grate_npc.begin_dialog(game.leader, 1000)
            return;
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
            if (tpsts("s_skole_goons", 3 * 24 * 60 * 60) == 1 && !get_f("s_skole_goons_scheduled") && !get_f("skole_dead"))
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
            if (tpsts("s_thrommel_reward", 14 * 24 * 60 * 60) == 1 && !get_f("s_thrommel_reward_scheduled"))
            {
                set_f("s_thrommel_reward_scheduled");
                if (!GetGlobalFlag(278) && !((EncounterQueue).Contains(3001)))
                {
                    // ggf278 - have had Thrommel Reward encounter
                    QueueRandomEncounter(3001);
                }

            }

            // Tillahi Reward Encounter - 10 days
            if (tpsts("s_tillahi_reward", 10 * 24 * 60 * 60) == 1 && !get_f("s_tillahi_reward_scheduled"))
            {
                set_f("s_tillahi_reward_scheduled");
                if (!GetGlobalFlag(279) && !((EncounterQueue).Contains(3002)))
                {
                    // ggf279 - have had Tillahi Reward encounter
                    QueueRandomEncounter(3002);
                }

            }

            // Sargen Reward Encounter - 3 weeks
            if (tpsts("s_sargen_reward", 21 * 24 * 60 * 60) == 1 && !get_f("s_sargen_reward_scheduled"))
            {
                set_f("s_sargen_reward_scheduled");
                if (!GetGlobalFlag(280) && !((EncounterQueue).Contains(3003)))
                {
                    // ggf280 - have had Sargen Reward encounter
                    QueueRandomEncounter(3003);
                }

            }

            // Ranth's Bandits Encounter 1 - random amount of days (normal distribution, average of 24 days, stdev = 8 days)
            if (tpsts("s_ranths_bandits_1", GetGlobalVar(923) * 24 * 60 * 60) == 1 && !get_f("s_ranths_bandits_scheduled"))
            {
                set_f("s_ranths_bandits_scheduled");
                if (!GetGlobalFlag(711) && !((EncounterQueue).Contains(3434)))
                {
                    // ggf711 - have had Ranth's Bandits Encounter
                    QueueRandomEncounter(3434);
                }

            }

            // Scarlet Brotherhood Retaliation for Snitch Encounter - 10 days
            if (tpsts("s_sb_retaliation_for_snitch", 10 * 24 * 60 * 60) == 1 && !get_f("s_sb_retaliation_for_snitch_scheduled"))
            {
                set_f("s_sb_retaliation_for_snitch_scheduled");
                if (!GetGlobalFlag(712) && !((EncounterQueue).Contains(3435)))
                {
                    // ggf712 - have had Scarlet Brotherhood Encounter
                    QueueRandomEncounter(3435);
                }

            }

            // Scarlet Brotherhood Retaliation for Narc Encounter - 6 days
            if (tpsts("s_sb_retaliation_for_narc", 6 * 24 * 60 * 60) == 1 && !get_f("s_sb_retaliation_for_narc_scheduled"))
            {
                set_f("s_sb_retaliation_for_narc_scheduled");
                if (!GetGlobalFlag(712) && !((EncounterQueue).Contains(3435)))
                {
                    // ggf712 - have had Scarlet Brotherhood Encounter
                    QueueRandomEncounter(3435);
                }

            }

            // Scarlet Brotherhood Retaliation for Whistelblower Encounter - 14 days
            if (tpsts("s_sb_retaliation_for_whistleblower", 14 * 24 * 60 * 60) == 1 && !get_f("s_sb_retaliation_for_whistleblower_scheduled"))
            {
                set_f("s_sb_retaliation_for_whistleblower_scheduled");
                if (!GetGlobalFlag(712) && !((EncounterQueue).Contains(3435)))
                {
                    // ggf712 - have had Scarlet Brotherhood Encounter
                    QueueRandomEncounter(3435);
                }

            }

            // Gremlich Scream Encounter 1 - 1 day
            if (tpsts("s_gremlich_1", 1 * 24 * 60 * 60) == 1 && !get_f("s_gremlich_1_scheduled"))
            {
                set_f("s_gremlich_1_scheduled");
                if (!GetGlobalFlag(713) && !((EncounterQueue).Contains(3436)))
                {
                    // ggf713 - have had Gremlich Scream Encounter 1
                    QueueRandomEncounter(3436);
                }

            }

            // Gremlich Reset Encounter - 5 days
            if (tpsts("s_gremlich_2", 5 * 24 * 60 * 60) == 1 && !get_f("s_gremlich_2_scheduled"))
            {
                set_f("s_gremlich_2_scheduled");
                if (!GetGlobalFlag(717) && !((EncounterQueue).Contains(3440)))
                {
                    // ggf717 - have had Gremlich Reset Encounter
                    QueueRandomEncounter(3440);
                }

            }

            // Mona Sport Encounter 1 (pirates vs. brigands) - 3 days
            if (tpsts("s_sport_1", 3 * 24 * 60 * 60) == 1 && !get_f("s_sport_1_scheduled"))
            {
                set_f("s_sport_1_scheduled");
                if (!GetGlobalFlag(718) && !((EncounterQueue).Contains(3441)))
                {
                    // ggf718 - have had Mona Sport Encounter 1
                    QueueRandomEncounter(3441);
                }

            }

            // Mona Sport Encounter 2 (bugbears vs. orcs melee) - 3 days
            if (tpsts("s_sport_2", 3 * 24 * 60 * 60) == 1 && !get_f("s_sport_2_scheduled"))
            {
                set_f("s_sport_2_scheduled");
                if (!GetGlobalFlag(719) && !((EncounterQueue).Contains(3442)))
                {
                    // ggf719 - have had Mona Sport Encounter 2
                    QueueRandomEncounter(3442);
                }

            }

            // Mona Sport Encounter 3 (bugbears vs. orcs ranged) - 3 days
            if (tpsts("s_sport_3", 3 * 24 * 60 * 60) == 1 && !get_f("s_sport_3_scheduled"))
            {
                set_f("s_sport_3_scheduled");
                if (!GetGlobalFlag(720) && !((EncounterQueue).Contains(3443)))
                {
                    // ggf720 - have had Mona Sport Encounter 3
                    QueueRandomEncounter(3443);
                }

            }

            // Mona Sport Encounter 4 (hill giants vs. ettins) - 3 days
            if (tpsts("s_sport_4", 3 * 24 * 60 * 60) == 1 && !get_f("s_sport_4_scheduled"))
            {
                set_f("s_sport_4_scheduled");
                if (!GetGlobalFlag(721) && !((EncounterQueue).Contains(3444)))
                {
                    // ggf721 - have had Mona Sport Encounter 4
                    QueueRandomEncounter(3444);
                }

            }

            // Mona Sport Encounter 5 (female vs. male bugbears) - 3 days
            if (tpsts("s_sport_5", 3 * 24 * 60 * 60) == 1 && !get_f("s_sport_5_scheduled"))
            {
                set_f("s_sport_5_scheduled");
                if (!GetGlobalFlag(722) && !((EncounterQueue).Contains(3445)))
                {
                    // ggf722 - have had Mona Sport Encounter 5
                    QueueRandomEncounter(3445);
                }

            }

            // Mona Sport Encounter 6 (zombies vs. lacedons) - 3 days
            if (tpsts("s_sport_6", 3 * 24 * 60 * 60) == 1 && !get_f("s_sport_6_scheduled"))
            {
                set_f("s_sport_6_scheduled");
                if (!GetGlobalFlag(723) && !((EncounterQueue).Contains(3446)))
                {
                    // ggf723 - have had Mona Sport Encounter 6
                    QueueRandomEncounter(3446);
                }

            }

            // Bethany Encounter - 2 days
            if (tpsts("s_bethany", 2 * 24 * 60 * 60) == 1 && !get_f("s_bethany_scheduled"))
            {
                set_f("s_bethany_scheduled");
                if (!GetGlobalFlag(724) && !((EncounterQueue).Contains(3447)))
                {
                    // ggf724 - have had Bethany Encounter
                    QueueRandomEncounter(3447);
                }

            }

            if (tpsts("s_zuggtmoy_banishment_initiate", 4 * 24 * 60 * 60) == 1 && !get_f("s_zuggtmoy_gone") && GetGlobalFlag(326) && attachee.GetMap() != 5016 && attachee.GetMap() != 5019)
            {
                set_f("s_zuggtmoy_gone");
                import py00262burne_apprentice
            py00262burne_apprentice.return_Zuggtmoy/*Unknown*/(SelectedPartyLeader, SelectedPartyLeader);
            }

            // End of Global Event Scheduling System  ###
            if ((GetGlobalVar(449) & (Math.Pow(2, 0) + Math.Pow(2, 1) + Math.Pow(2, 2))) != 0) // If set preference for speed
            {
                Batch.speedup(GetGlobalVar(449) & (Math.Pow(2, 0) + Math.Pow(2, 1) + Math.Pow(2, 2)), GetGlobalVar(449) & (Math.Pow(2, 0) + Math.Pow(2, 1) + Math.Pow(2, 2)));
            }

            if (GetGlobalFlag(403)) // Test mode enabled; autokill critters!
            {
                // game.particles( "sp-summon monster I", game.leader)
                // game.timevent_add( autokill, (cur_map, 1), 150 )
                autokill(cur_map, 1);
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    pc.IdentifyAll();
                }

            }

            if ((cur_map == 5004)) // Moathouse Upper floor
            {
                if ((GetGlobalVar(455) & Math.Pow(2, 7)) != 0) // Secret Door Reveal
                {
                    foreach (var obj in ObjList.ListVicinity(Itt.lfa(464, 470), ObjectListFilter.OLC_PORTAL | ObjectListFilter.OLC_SCENERY))
                    {
                        if ((obj.GetInt(obj_f.secretdoor_flags) & Math.Pow(2, 16)) != 0) // OSDF_SECRET_DOOR
                        {
                            obj.SetInt(obj_f.secretdoor_flags, obj.GetInt(obj_f.secretdoor_flags) | Math.Pow(2, 17));
                        }

                    }

                }

            }
            else if ((cur_map == 5005))
            {
                // Moathouse Dungeon
                var ggv402 = GetGlobalVar(402);
                var ggv403 = GetGlobalVar(403);
                if ((ggv402 & (Math.Pow(2, 0))) == 0)
                {
                    Logger.Info("modifying moathouse... 
                    ");
                    modify_moathouse();
                    ggv402 |= Math.Pow(2, 0);
                    SetGlobalVar(402, ggv402);
                }

                if (moathouse_alerted() && (ggv403 & (Math.Pow(2, 0))) == 0)
                {
                    moathouse_reg();
                    ggv403 |= Math.Pow(2, 0);
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

                set_f("pc_dropoff", 0);
            }
            else if ((cur_map == 5110)) // Temple Ruined Building
            {
                GetGlobalVar(491) |= Math.Pow(2, 0);
            }
            else if ((cur_map == 5111)) // Temple Broken Tower - Exterior
            {
                GetGlobalVar(491) |= Math.Pow(2, 1);
            }
            else if ((cur_map == 5065)) // Temple Broken Tower - Interior
            {
                GetGlobalVar(491) |= Math.Pow(2, 2);
            }
            else if ((cur_map == 5092)) // Temple Escape Tunnel
            {
                GetGlobalVar(491) |= Math.Pow(2, 3);
            }
            else if ((cur_map == 5112)) // Temple Burnt Farmhouse
            {
                GetGlobalVar(491) |= Math.Pow(2, 4);
            }
            else if ((cur_map == 5064)) // Temple entrance level
            {
                var found_map_obj = 0;
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    if (pc.FindItemByName(11299))
                    {
                        found_map_obj = 1;
                    }

                }

                if (!found_map_obj)
                {
                    var map_obj = GameSystems.MapObject.CreateObject(11299, SelectedPartyLeader.GetLocation());
                    var got_map_obj = 0;
                    var pc_index = 0;
                    while (got_map_obj == 0 && pc_index < GameSystems.Party.PartyMembers.Count)
                    {
                        if (!GameSystems.Party.GetPartyGroupMemberN(pc_index).IsUnconscious() && GameSystems.Party.GetPartyGroupMemberN(pc_index).type == ObjectType.pc)
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

                if ((GetGlobalVar(455) & Math.Pow(2, 7)) != 0)
                {
                    foreach (var obj in ObjList.ListVicinity(Itt.lfa(500, 500), ObjectListFilter.OLC_SCENERY | ObjectListFilter.OLC_PORTAL))
                    {
                        if ((obj.GetInt(obj_f.secretdoor_flags) & Math.Pow(2, 16)) != 0) // OSDF_SECRET_DOOR
                        {
                            obj.SetInt(obj_f.secretdoor_flags, obj.GetInt(obj_f.secretdoor_flags) | Math.Pow(2, 17));
                        }

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

                if (earth_alerted() && ((get_v(454) & 1) == 0) && ((GetGlobalVar(450) & Math.Pow(2, 0)) == 0) && ((GetGlobalVar(450) & (Math.Pow(2, 13))) == 0))
                {
                    set_v(454, get_v(454) | 1);
                    Itt.earth_reg();
                }

                var (xx, yy) = SelectedPartyLeader.GetLocation();
                if (Math.Pow((xx - 421), 2) + Math.Pow((yy - 589), 2) <= 400)
                {
                    GetGlobalVar(491) |= Math.Pow(2, 5);
                }

                if (Math.Pow((xx - 547), 2) + Math.Pow((yy - 589), 2) <= 400)
                {
                    GetGlobalVar(491) |= Math.Pow(2, 6);
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

                if (water_alerted() && ((get_v(454) & 2) == 0 || ((get_v(454) & (Math.Pow(2, 6) + Math.Pow(2, 7))) == Math.Pow(2, 6))) && ((GetGlobalVar(450) & Math.Pow(2, 0)) == 0) && ((GetGlobalVar(450) & (Math.Pow(2, 13))) == 0))
                {
                    set_v(454, get_v(454) | 2);
                    if ((get_v(454) & (Math.Pow(2, 6) + Math.Pow(2, 7))) == Math.Pow(2, 6))
                    {
                        set_v(454, get_v(454) | Math.Pow(2, 7)); // indicate that Oohlgrist and co have been moved to Water
                    }

                    Itt.water_reg();
                }

                if (air_alerted() && ((get_v(454) & 4) == 0) && ((GetGlobalVar(450) & Math.Pow(2, 0)) == 0) && ((GetGlobalVar(450) & (Math.Pow(2, 13))) == 0))
                {
                    set_v(454, get_v(454) | 4);
                    Itt.air_reg();
                }

                if (fire_alerted() && ((get_v(454) & Math.Pow(2, 3)) == 0 || ((get_v(454) & (Math.Pow(2, 4) + Math.Pow(2, 5))) == Math.Pow(2, 4))) && ((GetGlobalVar(450) & Math.Pow(2, 0)) == 0) && ((GetGlobalVar(450) & (Math.Pow(2, 13))) == 0))
                {
                    // Fire is on alert and haven't yet regrouped, or have already regrouped but Oohlgrist was recruited afterwards (2**5) and not transferred yet
                    set_v(454, get_v(454) | Math.Pow(2, 3));
                    if ((get_v(454) & (Math.Pow(2, 4) + Math.Pow(2, 5))) == Math.Pow(2, 4))
                    {
                        set_v(454, get_v(454) | Math.Pow(2, 5)); // indicate that Oohlgrist and co have been moved
                    }

                    SetGlobalFlag(154, true); // Make the Werewolf mirror shut up
                    Itt.fire_reg();
                }

                var (xx, yy) = SelectedPartyLeader.GetLocation();
                if (Math.Pow((xx - 564), 2) + Math.Pow((yy - 377), 2) <= 400)
                {
                    GetGlobalVar(491) |= Math.Pow(2, 7);
                }
                else if (Math.Pow((xx - 485), 2) + Math.Pow((yy - 557), 2) <= 1600)
                {
                    GetGlobalVar(491) |= Math.Pow(2, 8);
                }
                else if (Math.Pow((xx - 485), 2) + Math.Pow((yy - 503), 2) <= 400)
                {
                    GetGlobalVar(491) |= Math.Pow(2, 8);
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
                    GetGlobalVar(491) |= Math.Pow(2, 9);
                }
                else if (Math.Pow((xx - 517), 2) + Math.Pow((yy - 518), 2) <= 400) // Air Temple Access (troll keys)
                {
                    GetGlobalVar(491) |= Math.Pow(2, 10);
                }
                else if (Math.Pow((xx - 552), 2) + Math.Pow((yy - 489), 2) <= 400) // Air Temple Secret Door (Scorpp Area)
                {
                    GetGlobalVar(491) |= Math.Pow(2, 22);
                }
                else if (Math.Pow((xx - 616), 2) + Math.Pow((yy - 606), 2) <= 400) // Water Temple Access (lamia)
                {
                    GetGlobalVar(491) |= Math.Pow(2, 11);
                }
                else if (Math.Pow((xx - 639), 2) + Math.Pow((yy - 450), 2) <= 1600) // Falrinth area
                {
                    GetGlobalVar(491) |= Math.Pow(2, 12);
                    if ((GetGlobalVar(455) & Math.Pow(2, 7)) != 0) // Secret Door Reveal
                    {
                        foreach (var obj in ObjList.ListVicinity(Itt.lfa(622, 503), ObjectListFilter.OLC_PORTAL | ObjectListFilter.OLC_SCENERY))
                        {
                            if ((obj.GetInt(obj_f.secretdoor_flags) & Math.Pow(2, 16)) != 0) // OSDF_SECRET_DOOR
                            {
                                obj.SetInt(obj_f.secretdoor_flags, obj.GetInt(obj_f.secretdoor_flags) | Math.Pow(2, 17));
                            }

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
                    GetGlobalVar(491) |= Math.Pow(2, 13);
                }
                else if (Math.Pow((xx - 477), 2) + Math.Pow((yy - 340), 2) <= 400)
                {
                    GetGlobalVar(491) |= Math.Pow(2, 14);
                }

            }
            else if ((cur_map == 5106)) // secret spiral staircase
            {
                GetGlobalVar(491) |= Math.Pow(2, 15);
            }
            else if ((cur_map == 5081)) // Air Node
            {
                GetGlobalVar(491) |= Math.Pow(2, 16);
            }
            else if ((cur_map == 5082)) // Earth Node
            {
                GetGlobalVar(491) |= Math.Pow(2, 17);
            }
            else if ((cur_map == 5083)) // Fire Node
            {
                GetGlobalVar(491) |= Math.Pow(2, 18);
            }
            else if ((cur_map == 5084)) // Water Node
            {
                GetGlobalVar(491) |= Math.Pow(2, 19);
            }
            else if ((cur_map == 5079)) // Zuggtmoy Level
            {
                GetGlobalVar(491) |= Math.Pow(2, 20);
            }

            return RunDefault;
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
                gloc = gnoll.GetLocation();
                grot = gnoll.Rotation;
                gnoll.Destroy();
                newgnoll = GameSystems.MapObject.CreateObject(14632, gloc);
                newgnoll.Rotation = grot;
                newgnoll.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // newgnoll.destroy()
            foreach (var gnoll in vlistxyr(511, 549, 14079, 33))
            {
                gloc = gnoll.GetLocation();
                grot = gnoll.Rotation;
                gnoll.Destroy();
                newgnoll = GameSystems.MapObject.CreateObject(14631, gloc);
                newgnoll.Rotation = grot;
                newgnoll.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // newgnoll.destroy()
            foreach (var gnoll in vlistxyr(511, 549, 14080, 33))
            {
                gloc = gnoll.GetLocation();
                grot = gnoll.Rotation;
                gnoll.Destroy();
                newgnoll = GameSystems.MapObject.CreateObject(14632, gloc);
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
                gloc = gnoll.GetLocation();
                grot = gnoll.Rotation;
                gnoll.Destroy();
                newgnoll = GameSystems.MapObject.CreateObject(14631, gloc);
                newgnoll.Rotation = grot;
                newgnoll.SetScriptId(ObjScriptEvent.Dying, 442);
            }

            // newgnoll.destroy()
            foreach (var gnoll in vlistxyr(445, 538, 14080, 50))
            {
                gloc = gnoll.GetLocation();
                grot = gnoll.Rotation;
                gnoll.Destroy();
                newgnoll = GameSystems.MapObject.CreateObject(14632, gloc);
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
                gloc = goblin.GetLocation();
                grot = goblin.Rotation;
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

        public static int earth_alerted()
        {
            if (GetGlobalFlag(104)) // romag is dead
            {
                return 0;
            }

            if (tpsts(512, 1 * 60 * 60) == 1)
            {
                // an hour has passed since you defiled the Earth Altar
                return 1;
            }

            if (tpsts(507, 1) == 1)
            {
                // You've killed the Troop Commander
                return 1;
            }

            if (tpsts(TS_CRITTER_THRESHOLD_CROSSED, 1))
            {
                var also_killed_earth_member = (tpsts(TS_EARTH_TROOP_KILLED_FIRST_TIME, 3 * 60) == 1) || (tpsts(TS_EARTH_CRITTER_KILLED_FIRST_TIME, 6 * 60) == 1);
                var did_quest_1 = GetQuestState(43) >= QuestState.Completed;
                if ((!did_quest_1) || also_killed_earth_member)
                {
                    if (tpsts(TS_CRITTER_THRESHOLD_CROSSED, 2 * 60 * 60)) // two hours have passed since you passed critter deathcount threshold
                    {
                        return 1;
                    }

                    if (tpsts(TS_CRITTER_KILLED_FIRST_TIME, 48 * 60 * 60) == 1) // 48 hours have passed since you first killed a critter and you've passed the threshold
                    {
                        return 1;
                    }

                }

            }

            // The second condition is for the case you've killed a critter, left to rest somewhere, and returned later, and at some point crossed the kill count threshold
            if ((tpsts(510, 1) == 1 && tpsts(505, 24 * 60 * 60) == 1) || tpsts(510, 2 * 60 * 60))
            {
                // Either two hours have passed since you passed Earth critter deathcount threshold, or 24 hours have passed since you first killed an Earth critter and you've passed the threshold
                return 1;
            }

            if ((tpsts(511, 1) == 1 && tpsts(506, 12 * 60 * 60) == 1) || tpsts(511, 1 * 60 * 60))
            {
                // Either 1 hour has passed since you passed troop deathcount threshold, or 12 hours have passed since you first killed a troop and you've passed the threshold
                return 1;
            }

            if (tsc(457, 470) || tsc(458, 470) || tsc(459, 470)) // killed Belsornig, Kelno or Alrrem before completing 2nd earth quest
            {
                return 1;
            }

            return 0;
        }
        public static int water_alerted()
        {
            if (GetGlobalFlag(105))
            {
                // belsornig is dead
                return 0;
            }

            if (tsc(456, 475) == 1 || tsc(458, 475) == 1 || tsc(459, 475) == 1) // killed Romag, Kelno or Alrrem before accepting second water quest
            {
                return 1;
            }

            return 0;
        }
        public static int air_alerted()
        {
            if (GetGlobalFlag(106))
            {
                // kelno is dead
                return 0;
            }

            if (tsc(456, 483) || tsc(457, 483) || tsc(459, 483))
            {
                // any of the other faction leaders are dead, and he hasn't yet given you that quest
                // Kelno doesn't take any chances
                return 1;
            }

            return 0;
        }
        public static int fire_alerted()
        {
            if (GetGlobalFlag(107)) // alrrem is dead
            {
                return 0;
            }

            // if (game.global_flags[104] == 1 or game.global_flags[105] == 1 or game.global_flags[106] == 1):
            // For now - if one of the other Leaders is dead
            // return 1
            if (tsc(456, 517) || tsc(457, 517) || tsc(458, 517))
            {
                // Have killed another High Priest without even having talked to him
                // Should suffice for him, since he's kind of crazy
                return 1;
            }

            return 0;
        }
        public static int is_follower(FIXME name)
        {
            foreach (var obj in GameSystems.Party.PartyMembers)
            {
                if ((obj.GetNameId() == name))
                {
                    return 1;
                }

            }

            return 0;
        }
        public static void destroy_weapons(GameObjectBody npc, FIXME item1, FIXME item2, FIXME item3)
        {
            if ((item1 != 0))
            {
                var moshe = npc.FindItemByName(item1);
                if ((moshe != null))
                {
                    moshe.Destroy();
                }

            }

            if ((item2 != 0))
            {
                var moshe = npc.FindItemByName(item2);
                if ((moshe != null))
                {
                    moshe.Destroy();
                }

            }

            if ((item3 != 0))
            {
                var moshe = npc.FindItemByName(item3);
                if ((moshe != null))
                {
                    moshe.Destroy();
                }

            }

            return;
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
            if ((Utilities.critter_is_unconscious(PartyLeader) != 1 && PartyLeader.type == ObjectType.pc && !PartyLeader.D20Query(D20DispatcherKey.QUE_Prone) && npc.HasLineOfSight(PartyLeader)))
            {
                PartyLeader.BeginDialog(npc, line_no);
            }
            else
            {
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    npc.TurnTowards(pc);
                    if ((Utilities.critter_is_unconscious(pc) != 1 && pc.type == ObjectType.pc && !pc.D20Query(D20DispatcherKey.QUE_Prone) && npc.HasLineOfSight(pc)))
                    {
                        pc.BeginDialog(npc, line_no);
                    }

                }

            }

            return;
        }
        public static int tsc(FIXME var1, FIXME var2)
        {
            // time stamp compare
            // check if event associated with var1 happened before var2
            // if they happened in the same second, well... only so much I can do
            if ((get_v(var1) == 0))
            {
                return 0;
            }
            else if ((get_v(var2) == 0))
            {
                return 1;
            }
            else if ((get_v(var1) < get_v(var2)))
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }
        public static int tpsts(FIXME time_var, FIXME time_elapsed)
        {
            // type: (object, long) -> long
            // Has the time elapsed since [time stamp] greater than the specified amount?
            if (get_v(time_var) == 0)
            {
                return 0;
            }

            if (CurrentTime.time_game_in_seconds/*Time*/(CurrentTime) > get_v(time_var) + time_elapsed)
            {
                return 1;
            }

            return 0;
        }
        public static void record_time_stamp(FIXME tvar, FIXME time_stamp_overwrite = 0)
        {
            if (get_v(tvar.ToString()) == 0 || time_stamp_overwrite == 1)
            {
                set_v(tvar.ToString(), CurrentTime.time_game_in_seconds/*Time*/(CurrentTime));
            }

            return;
        }
        public static void pop_up_box(FIXME message_id)
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
            foreach (var npc in ObjList.ListVicinity(Itt.lfa(xx, yy), ObjectListFilter.OLC_NPC))
            {
                var (npc_x, npc_y) = Itt.lta(npc.GetLocation());
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
            var (nx, ny) = npc.GetLocation();
            var (px, py) = pc.GetLocation();
            var vx = px - nx;
            var vy = py - ny;
            // (vx, vy) is a vector pointing from the PC to the NPC.
            // Using its angle, we rotate the NPC and THEN check for sight.
            // After that, we return the NPC to its original facing.
            npc.Rotation = 3.14159f / 2 - (atan2(vy, vx) + 5 * 3.14159f / 4);
            if (npc.HasLineOfSight(pc))
            {
                npc.Rotation = orot;
                return true;
            }

            npc.Rotation = orot;
            return false;
        }
        public static int can_see_party(GameObjectBody npc)
        {
            foreach (var pc in PartyLeader.GetPartyMembers())
            {
                if (can_see2(npc, pc))
                {
                    return 1;
                }

            }

            return 0;
        }
        public static int is_far_from_party(GameObjectBody npc, FIXME dist = 20)
        {
            // Returns 1 if npc is farther than specified distance from party
            foreach (var pc in PartyLeader.GetPartyMembers())
            {
                if (npc.DistanceTo(pc) < dist)
                {
                    return 0;
                }

            }

            return 1;
        }
        public static int is_safe_to_talk_rfv(GameObjectBody npc, GameObjectBody pc, int radius = 20, FIXME facing_required = 0, FIXME visibility_required = 1)
        {
            // visibility_required - Capability of seeing PC required (i.e. PC is not invisibile / sneaking)
            // -> use can_see2(npc, pc)
            // facing_required - In addition, the NPC is actually looking at the PC's direction
            if (visibility_required == 0)
            {
                if ((pc.type == ObjectType.pc && Utilities.critter_is_unconscious(pc) != 1 && npc.DistanceTo(pc) <= radius))
                {
                    return 1;
                }

            }
            else if (visibility_required == 1 && facing_required == 1)
            {
                if ((npc.HasLineOfSight(pc) && pc.type == ObjectType.pc && Utilities.critter_is_unconscious(pc) != 1 && npc.DistanceTo(pc) <= radius))
                {
                    return 1;
                }

            }
            else if (visibility_required == 1 && facing_required != 1)
            {
                if ((can_see2(npc, pc) && pc.type == ObjectType.pc && Utilities.critter_is_unconscious(pc) != 1 && npc.DistanceTo(pc) <= radius))
                {
                    return 1;
                }

            }

            return 0;
        }
        public static int within_rect_by_corners(GameObjectBody obj, FIXME ulx, FIXME uly, FIXME brx, FIXME bry)
        {
            // refers to "visual" axes (edges parallel to your screen's edges rather than ToEE's native axes)
            var (xx, yy) = obj.GetLocation();
            if (((xx - yy) <= (ulx - uly)) && ((xx - yy) >= (brx - bry)) && ((xx + yy) >= (ulx + uly)) && ((xx + yy) <= (brx + bry)))
            {
                return 1;
            }

            return 0;
        }
        public static bool encroach(GameObjectBody a, GameObjectBody b)
        {
            // A primitive way of making distant AI combatants who don't close the distances by themselves move towards the player
            b.TurnTowards(a);
            if (a.DistanceTo(b) < 30)
            {
                return -1;
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
        public static GameObjectBody buffee(locXY location, int det_range, IList<int> buff_list, List<GameObjectBody> done_list)
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
        public static void modify_moathouse()
        {
            foreach (var obj in ObjList.ListVicinity(new locXY(490, 535), ObjectListFilter.OLC_NPC))
            {
                if ((range(14074, 14078)).Contains(obj.GetNameId()))
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
                if ((range(14074, 14078)).Contains(obj.GetNameId()))
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
        public static int moathouse_alerted()
        {
            if (GetGlobalFlag(363))
            {
                // Bullied or attacked Sergeant at the door
                return 1;
            }
            else
            {
                var bugbear_group_kill_ack = 0;
                var gnoll_group_kill_ack = 0;
                var lubash_kill_ack = 0;
                var ground_floor_brigands_kill_ack = 0;
                if (GetGlobalVar(404) != 0 && (CurrentTime.time_game_in_seconds/*Time*/(CurrentTime) > GetGlobalVar(404) + 12 * 60 * 60))
                {
                    bugbear_group_kill_ack = 1;
                }

                if (GetGlobalVar(405) != 0 && (CurrentTime.time_game_in_seconds/*Time*/(CurrentTime) > GetGlobalVar(405) + 12 * 60 * 60))
                {
                    gnoll_group_kill_ack = 1;
                }

                if (GetGlobalVar(406) != 0 && (CurrentTime.time_game_in_seconds/*Time*/(CurrentTime) > GetGlobalVar(406) + 12 * 60 * 60))
                {
                    lubash_kill_ack = 1;
                }

                if (GetGlobalVar(407) != 0 && (CurrentTime.time_game_in_seconds/*Time*/(CurrentTime) > GetGlobalVar(407) + 48 * 60 * 60))
                {
                    ground_floor_brigands_kill_ack = 1;
                }

                return ((ground_floor_brigands_kill_ack + lubash_kill_ack + gnoll_group_kill_ack + bugbear_group_kill_ack) >= 2);
            }

            return 0;
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
                        sps(obj, 639);
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
                        sps(obj, 637);
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
                        obj = GameSystems.MapObject.CreateObject(14076, new locXY(476, 541));
                        obj.Move(new locXY(476, 541), 0, 0);
                        obj.Rotation = 4;
                        obj.SetScriptId(ObjScriptEvent.Dying, 450);
                        obj.SetScriptId(ObjScriptEvent.EnterCombat, 450);
                        obj.SetScriptId(ObjScriptEvent.ExitCombat, 450);
                        obj.SetScriptId(ObjScriptEvent.StartCombat, 450);
                        obj.SetScriptId(ObjScriptEvent.EndCombat, 450);
                        obj.SetScriptId(ObjScriptEvent.SpellCast, 450);
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
        public static void lnk(FIXME loc_0, FIXME xx, FIXME yy, FIXME name_id, FIXME stun_name_id)
        {
            // Locate n' Kill!
            if (typeof(stun_name_id) == typeof(- 1))
{
                stun_name_id = new[] { stun_name_id };
            }

            if (typeof(name_id) == typeof(- 1))
{
                name_id = new[] { name_id };
            }

            if (loc_0 == -1 && xx == -1 && yy == -1)
            {
                loc_0 = SelectedPartyLeader.GetLocation();
            }
            else if (xx != -1 && yy != -1)
            {
                loc_0 = new locXY(xx, yy); // Needs location_from_axis from utilities.py
            }
            else
            {
                loc_0 = SelectedPartyLeader.GetLocation();
            }

            if (name_id == new[] { -1 })
            {
                foreach (var obj in ObjList.ListVicinity(loc_0, ObjectListFilter.OLC_NPC))
                {
                    if ((obj.GetReaction(PartyLeader) <= 0 || !obj.IsFriendly(PartyLeader)) && (obj.GetLeader() == null && (obj.GetObjectFlags() & ObjectFlag.DONTDRAW) == 0))
                    {
                        if (!((stun_name_id).Contains(obj.GetNameId())))
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
                foreach (var obj in ObjList.ListVicinity(loc_0, ObjectListFilter.OLC_NPC))
                {
                    if (((name_id + stun_name_id)).Contains(obj.GetNameId()) && (obj.GetReaction(PartyLeader) <= 0 || !obj.IsFriendly(PartyLeader)) && (obj.GetLeader() == null && (obj.GetObjectFlags() & ObjectFlag.DONTDRAW) == 0))
                    {
                        if (!((stun_name_id).Contains(obj.GetNameId())))
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
                            if (is_unconscious(obj) == 0)
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
        public static void loot_items(GameObjectBody loot_source, GameObjectBody pc, int loot_source_name, int xx, int yy, IList<int> item_proto_list, bool loot_money_and_jewels_also = 1, bool autoloot = 1, bool autoconvert_jewels = 1, IList<int> item_autoconvert_list)
        {
            if (get_f("qs_autoloot") != 1)
            {
                return;
            }

            if (get_f("qs_autoconvert_jewels") != 1)
            {
                autoconvert_jewels = 0;
            }

            var money_protos = range(7000, 7004); // Note that the range actually extends from 7000 to 7003
            var gem_protos = new[] { 12010 } + range(12034, 12045);
            var jewel_protos = range(6180, 6198);
            var potion_protos = new[] { 8006, 8007 };
            var tank_armor_0 = new List<GameObjectBody>();
            var barbarian_armor_0 = new List<GameObjectBody>();
            var druid_armor_0 = new List<GameObjectBody>();
            var wizard_items_0 = new List<GameObjectBody>();
            var autosell_list = new List<GameObjectBody>();
            autosell_list += range(4002, 4106);
            autosell_list += range(4113, 4120);
            autosell_list += range(4155, 4191);
            autosell_list += range(6001, 6048);
            autosell_list += new[] { 6055, 6056 } + new[] { 6059, 6060 } + range(6062, 6073);
            autosell_list += range(6074, 6082);
            autosell_list += new[] { 6093, 6096, 6103, 6120, 6123, 6124 };
            autosell_list += range(6142, 6153);
            autosell_list += range(6153, 6159);
            autosell_list += range(6163, 6180);
            autosell_list += range(6202, 6239);
            var autosell_exclude_list = new List<GameObjectBody>();
            autosell_exclude_list += new[] { 4016, 4017, 4025, 4028 }; // Frag, Scath, Excal, Flam Swo +1
            autosell_exclude_list += new[] { 4047, 4057, 4058 }; // Scimitar +1, Dagger +2, Dager +1
            autosell_exclude_list += new[] { 4078, 4079 }; // Warha +1, +2
            autosell_exclude_list += range(4081, 4087); // Longsword +1 ... +5, Unholy Orc ax+1
            autosell_exclude_list += new[] { 4098 }; // Battleaxe +1
            autosell_exclude_list += new[] { 4161 }; // Shortsword +2
            autosell_exclude_list += new[] { 5802 }; // Figurine name IDs - as per protos.tab
            autosell_exclude_list += new[] { 6015, 6017, 6031, 6039, 6058, 6073, 6214, 6215, 6219 };
            autosell_exclude_list += new[] { 6239, 12602 };
            autosell_exclude_list += new[] { 8006, 8007, 8008, 8101 }; // Potions of Cure mod, serious & Haste
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
            foreach (var qqq in autosell_exclude_list)
            {
                if ((autosell_list).Contains(qqq))
                {
                    autosell_list.remove/*ObjectList*/(qqq);
                }

            }

            if (loot_money_and_jewels_also)
            {
                if (typeof(item_proto_list) == typeof(new List<GameObjectBody>()))
{
                    item_proto_list = item_proto_list + money_protos + gem_protos + jewel_protos + potion_protos;
                }
else
                {
                    item_proto_list = new[] { item_proto_list } + money_protos + gem_protos + jewel_protos + potion_protos;
                }

            }
            else if (typeof(item_proto_list) == typeof(1))
{
                item_proto_list = new[] { item_proto_list };
            }

            // pc - Who will take the loot?
            if (pc == -1)
            {
                pc = SelectedPartyLeader;
            }

            // loc_0 - Where will the loot be sought?
            if (xx == -1 || yy == -1)
            {
                var loc_0 = pc.GetLocation();
            }
            else
            {
                var loc_0 = new locXY(xx, yy);
            }

            if (loot_source != null)
            {
                foreach (var pp in (item_proto_list + item_autoconvert_list))
                {
                    if (typeof(pp) == typeof(1))
{
                        if ((item_autoconvert_list).Contains(pp))
                        {
                            var pp_1 = loot_source.FindItemByProto(pp);
                            if (pp_1 != null)
                            {
                                if ((pp_1.GetItemFlags() & (ItemFlag.NO_DISPLAY + ItemFlag.NO_LOOT)) == 0)
                                {
                                    autosell(pp_1);
                                }

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

            }
            else
            {
                if (loot_source_name != -1)
                {
                    if (typeof(loot_source_name) == typeof(1))
{
                        loot_source_name = new[] { loot_source_name };
                    }

                }
                else
                {
                    loot_source_name = new[] { -1 };
                }

                foreach (var robee in ObjList.ListVicinity(loc_0, ObjectListFilter.OLC_NPC | ObjectListFilter.OLC_CONTAINER | ObjectListFilter.OLC_ARMOR | ObjectListFilter.OLC_WEAPON | ObjectListFilter.OLC_GENERIC))
                {
                    if (!((PartyLeader.GetPartyMembers()).Contains(robee)) && ((loot_source_name).Contains(robee.GetNameId()) || loot_source_name == new[] { -1 }))
                    {
                        if ((robee.type == ObjectType.weapon) || (robee.type == ObjectType.armor) || (robee.type == ObjectType.generic))
                        {
                            if ((robee.GetItemFlags() & (ItemFlag.NO_DISPLAY + ItemFlag.NO_LOOT)) == 0)
                            {
                                if ((autosell_list + item_autoconvert_list).Contains(robee.GetNameId()))
                                {
                                    autosell_item(robee);
                                }
                                else if ((autosell_exclude_list).Contains(robee.GetNameId()))
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
                            for (var qq = 0; qq < 16; qq++)
                            {
                                var qq_item_worn = robee.ItemWornAt(qq);
                                if (qq_item_worn != null && (qq_item_worn.GetItemFlags() & (ItemFlag.NO_DISPLAY + ItemFlag.NO_LOOT)) == 0)
                                {
                                    if (((autosell_list + item_autoconvert_list)).Contains(qq_item_worn.GetNameId()))
                                    {
                                        autosell_item(qq_item_worn);
                                    }

                                }

                            }

                        }

                        foreach (var item_proto in (item_proto_list + item_autoconvert_list))
                        {
                            var item_sought = robee.FindItemByProto(item_proto);
                            if (item_sought != null && (item_sought.GetItemFlags() & ItemFlag.NO_DISPLAY) == 0)
                            {
                                if (((((gem_protos + jewel_protos)).Contains(item_proto)) && autoconvert_jewels == 1) || ((item_autoconvert_list).Contains(item_proto)))
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

            return;
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
                return 0.4f + float(highest_appraise) * 0.03f;
            }

        }
        public static int appraise_tool(GameObjectBody obj)
        {
            // Returns what you'd get for selling it
            var aa = sell_modifier();
            return (int)(aa * obj.GetInt(obj_f.item_worth));
        }
        public static int s_roundoff(FIXME app_sum)
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

            if (app_sum > 100000 && app_sum <= 1000000)
            {
                return 1000 * (int)(((int)(app_sum) / 1000));
            }

        }
        public static void autosell_item(FIXME item_sought, FIXME item_proto, GameObjectBody pc, FIXME item_quantity = 1, FIXME display_float = 1)
        {
            if (item_sought == null)
            {
                return;
            }

            if (pc == -1)
            {
                pc = SelectedPartyLeader;
            }

            if (item_proto == -1)
            {
                item_proto = item_sought.name/*Unknown*/;
            }

            var autoconvert_copper = appraise_tool(item_sought) * item_sought.obj_get_int/*Unknown*/(obj_f.item_quantity);
            pc.AdjustMoney(autoconvert_copper);
            item_sought.object_flag_set/*Unknown*/(ObjectFlag.OFF);
            item_sought.item_flag_set/*Unknown*/(ItemFlag.NO_DISPLAY);
            item_sought.item_flag_set/*Unknown*/(ItemFlag.NO_LOOT);
            if (display_float == 1 && autoconvert_copper > 5000 || display_float == 2)
            {
                pc.FloatMesFileLine("mes/script_activated.mes", 10000, TextFloaterColor.Green);
                pc.FloatMesFileLine("mes/description.mes", item_proto, TextFloaterColor.Green);
                pc.FloatMesFileLine("mes/transaction_sum.mes", (s_roundoff(autoconvert_copper / 100)), TextFloaterColor.Green);
            }

            return;
        }
        public static void giv(GameObjectBody pc, FIXME proto_id, FIXME in_group = 0)
        {
            if (in_group == 0)
            {
                if (pc.FindItemByProto(proto_id) == null)
                {
                    Utilities.create_item_in_inventory(proto_id, pc);
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
                    return 1;
                }
                else
                {
                    return 0;
                }

            }

            return;
        }
        public static void cnk(FIXME proto_id, FIXME do_not_destroy = 0, FIXME how_many = 1, FIXME timer = 0)
        {
            // Create n' Kill
            // Meant to simulate actually killing the critter
            // if timer == 0:
            for (var pp = 0; pp < how_many; pp++)
            {
                var a = GameSystems.MapObject.CreateObject(proto_id, SelectedPartyLeader.GetLocation());
                var damage_dice = Dice.Parse("50d50");
                a.Damage(PartyLeader, DamageType.Bludgeoning, damage_dice);
                if (do_not_destroy != 1)
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

        public static void autokill(FIXME cur_map, FIXME autoloot = 1, FIXME is_timed_autokill = 0)
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
                            giv(SelectedPartyLeader, 11050, 1); // Book of Heroes
                            giv(SelectedPartyLeader, 12589, 1); // Horn of Fog
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
                    if (is_timed_autokill == 0)
                    {
                        StartTimer(100, () => autokill(cur_map, 1, 1));
                    }
                    else
                    {
                        // Barbarian Elf
                        lnk(482, 476, 8717);
                        loot_items(8717, new[] { 6396, 6045, 6046, 4204 });
                        SetGlobalVar(961, 4);
                    }

                }

            }

            // MOATHOUSE   #
            if ((cur_map == 5002)) // Moathouse Exterior
            {
                if (get_f("qs_autokill_moathouse"))
                {
                    lnk(469, 524, 14057); // giant frogs
                    lnk(492, 523, 14057); // giant frogs
                    lnk(475, 505, 14057); // giant frogs
                    loot_items(475, 505, new[] { 6270 }, 14057, autoloot); // Jay's Ring
                    lnk(475, 460, 14070); // courtyard brigands
                    loot_items(475, 460, autoloot);
                    if (get_v("qs_moathouse_ambush_time") == 0 && get_f("qs_lareth_dead"))
                    {
                        StartTimer(500, () => autokill(cur_map));
                        set_v("qs_moathouse_ambush_time", 500);
                    }
                    else if (get_v("qs_moathouse_ambush_time") == 500)
                    {
                        lnk(478, 460, new[] { 14078, 14079, 14080, 14313, 14314, 14642, 8010, 8004, 8005 }); // Ambush
                        lnk(430, 444, new[] { 14078, 14079, 14080, 14313, 14314, 14642, 8010, 8004, 8005 }); // Ambush
                        loot_items(478, 460);
                        loot_items(430, 444);
                        set_v("qs_moathouse_ambush_time", 1000);
                    }

                }

                if (get_f("qs_autokill_temple"))
                {
                    lnk(503, 506, new[] { 14507, 14522 }); // Boars
                    lnk(429, 437, new[] { 14052, 14053 }); // Bears
                    lnk(478, 448, new[] { 14600, 14674, 14615, 14603, 14602, 14601 }); // Undead
                    lnk(468, 470, new[] { 14674, 14615, 14603, 14602, 14601 }); // Undead
                }

            }

            if ((cur_map == 5003)) // Moathouse Tower
            {
                if (get_f("qs_autokill_moathouse"))
                {
                    lnk(14047); // giant spider
                }

            }

            if ((cur_map == 5004)) // Moathouse Upper floor
            {
                if (get_f("qs_autokill_moathouse"))
                {
                    lnk(476, 493, 14088); // Huge Viper
                    lnk(476, 493, 14182); // Stirges
                    lnk(473, 472, new[] { 14070, 14074, 14069 }); // Backroom brigands
                    loot_items(473, 472, autoloot);
                    lnk(502, 476, new[] { 14089, 14090 }); // Giant Tick & Lizard
                    loot_items(502, 472, autoloot, new[] { 6050 });
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
                        lnk(476, 493, new[] { 14138, 14344, 14391 }); // Lycanthropes
                        lnk(502, 476, new[] { 14295, 14142 }); // Basilisk & Ochre Jelly
                    }

                }

            }

            if ((cur_map == 5005)) // Moathouse Dungeon
            {
                if (get_f("qs_autokill_moathouse"))
                {
                    lnk(416, 439, 14065); // Lubash
                    loot_items(416, 439, new[] { 6058 }, 14065, autoloot);
                    SetGlobalFlag(55, true); // Freed Gnomes
                    SetGlobalFlag(991, true); // Flag For Verbobonc Gnomes
                    lnk(429, 413, new[] { 14123, 14124, 14092, 14126, 14091 }); // Zombies, Green Slime
                    lnk(448, 417, new[] { 14123, 14124, 14092, 14126 }); // Zombies
                    loot_items(448, 417, 12105, -1, autoloot);
                    lnk(450, 519, range(14170, 14174) + range(14213, 14217)); // Bugbears
                    lnk(430, 524, range(14170, 14174) + range(14213, 14217)); // Bugbears
                    loot_items(450, 519, autoloot);
                    loot_items(430, 524, autoloot);
                    if (GameSystems.Party.PartyMembers.Count < 4 && get_v("AK5005_Stage") < 1)
                    {
                        set_v("AK5005_Stage", get_v("AK5005_Stage") + 1);
                        return;
                    }

                    // Gnolls and below
                    lnk(484, 497, new[] { 14066, 14067, 14078, 14079, 14080 }); // Gnolls
                    lnk(484, 473, new[] { 14066, 14067, 14078, 14079, 14080 }); // Gnolls
                    loot_items(484, 497, autoloot);
                    loot_items(484, 473, autoloot);
                    lnk(543, 502, 14094); // Giant Crayfish
                    lnk(510, 447, new[] { 14128, 14129, 14095 }); // Ghouls
                    if (GameSystems.Party.PartyMembers.Count < 4 && get_v("AK5005_Stage") < 2 || (GameSystems.Party.PartyMembers.Count < 8 && get_v("AK5005_Stage") < 1))
                    {
                        set_v("AK5005_Stage", get_v("AK5005_Stage") + 1);
                        return;
                    }

                    lnk(515, 547, new[] { 14074, 14075 }); // Front Guardsmen
                    loot_items(515, 547, autoloot);
                    lnk(485, 536, new[] { 14074, 14075, 14076, 14077 }); // Back Guardsmen
                    loot_items(485, 536, new[] { 14074, 14075, 14076, 14077 }, autoloot); // Back guardsmen
                    from py00060lareth import create_spiders
if (!get_f("qs_lareth_spiders_spawned"))
                    {
                        create_spiders(SelectedPartyLeader, SelectedPartyLeader);
                        set_f("qs_lareth_spiders_spawned", 1);
                    }

                    lnk(480, 540, new[] { 8002, 14397, 14398, 14620 }); // Lareth & Spiders
                    set_f("qs_lareth_dead");
                    lnk(530, 550, new[] { 14417 }); // More Spiders
                    loot_items(480, 540, (new[] { 4120, 6097, 6098, 6099, 6100, 11003 } + range(9001, 9688)), new[] { 8002, 1045 }, autoloot); // Lareth & Lareth's Dresser
                    loot_items(480, 540, new[] { 4194 });
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
                        lnk(416, 439, 14141); // Crystal Oozes
                                              // Bodaks, Shadows and Groaning Spirit
                        lnk(436, 521, new[] { 14328, 14289, 14280 });
                        // Skeleton Gnolls
                        lnk(486, 480, new[] { 14616, 14081, 14082, 14083 });
                        lnk(486, 495, new[] { 14616, 14081, 14082, 14083 }); // Skeleton Gnolls
                                                                             // Witch
                        lnk(486, 540, new[] { 14603, 14674, 14601, 14130, 14137, 14328, 14125, 14110, 14680 });
                        loot_items(486, 540, new[] { 11098, 6273, 4057, 6263, 4498 }, new[] { 4226, 6333, 5099 });
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
                        lnk(500, 490, new[] { 14078, 14079, 14080, 14313, 14314, 14642, 8010, 8004, 8005 }); // Ambush
                        lnk(470, 485, new[] { 14078, 14079, 14080, 14313, 14314, 14642, 8010, 8004, 8005 }); // Ambush
                        loot_items(500, 490);
                        loot_items(470, 490);
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
                        lnk(467, 383, new[] { 14603, 14600 }); // NW Skeletons
                        loot_items(467, 380);
                        lnk(507, 443, new[] { 14603, 14600 }); // W Skeletons
                        lnk(515, 421, new[] { 14603, 14600 }); // W Skeletons
                        loot_items(507, 443);
                        loot_items(515, 421);
                        lnk(484, 487, new[] { 14603, 14600, 14616, 14615 }); // Rainbow Rock 1
                        lnk(471, 500, new[] { 14603, 14600, 14616, 14615 }); // Rainbow Rock 1
                        loot_items(484, 487);
                        loot_items(484, 487, new[] { 1031 }, new[] { 12024 });
                        if (!get_f("qs_rainbow_spawned"))
                        {
                            set_f("qs_rainbow_spawned", 1);
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

                        lnk(484, 487, new[] { 14602, 14601 }); // Rainbow Rock 2
                        loot_items(484, 487);
                        // game.timevent_add( autokill, (cur_map), 1500 )
                        lnk(532, 540, new[] { 14603, 14600 }); // SE Skeletons
                        loot_items(540, 540);
                        lnk(582, 514, new[] { 14221, 14053 }); // Hill Giant
                    }
                    else if (get_v("qs_emridy_time") == 1000)
                    {
                        set_v("qs_emridy_time", 1500);
                        loot_items(582, 514);
                        loot_items(582, 514, new[] { 12602 });
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
                        lnk(14303);
                        loot_items(14303, new[] { 6315, 6199, 4701, 4500, 8007, 11002 }, new[] { 6046 });
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
                    lnk(485, 455, (new[] { 14279 } + range(14084, 14088))); // Hag & Lizards
                                                                            // lnk(xx = 468, yy = 467, name_id = ([14279] + range(14084, 14088))  ) # Hag & Lizards
                    loot_items(485, 455);
                    lnk(460, 480, new[] { 14329 }); // Gar
                    loot_items(460, 480, new[] { 12005 }); // Gar Corpse + Lamia Figurine
                    loot_items(460, 500, new[] { 12005 }); // Lamia Figurine - bulletproof
                    set_f("qs_killed_gar");
                    lnk(new[] { 14445, 14057 }); // Kingfrog, Giant Frog
                    loot_items(476, 497, new[] { 4082, 6199, 6082, 4191, 6215, 5006 });
                }

            }

            if ((cur_map == 5052)) // Boatmen's Tavern
            {
                if (get_f("qs_autokill_nulb"))
                {
                    if (GetGlobalFlag(281)) // Have had Skole Goons Encounter
                    {
                        lnk(new[] { 14315, 14134 }); // Skole + Goon
                        loot_items(new[] { 14315, 14134 }, new[] { 6051, 4121 });
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
                    if (is_timed_autokill == 0)
                    {
                        StartTimer(100, () => autokill(cur_map, 1, 1));
                    }
                    else
                    {
                        lnk(508, 485, 8718);
                        loot_items(508, 485, 8718, new[] { 4443, 6040, 6229 });
                        SetGlobalVar(961, 6);
                    }

                }

            }

            if ((cur_map == 5060)) // Waterside Hostel
            {
                if (get_f("qs_autokill_nulb"))
                {
                    if (is_timed_autokill == 0)
                    {
                        StartTimer(100, () => autokill(cur_map, 1, 1));
                    }
                    else
                    {
                        // Thieving Dala
                        SetQuestState(37, QuestState.Completed);
                        lnk(480, 501, new[] { 14147, 14146, 14145, 8018, 14074 }, new[] { 14372, 14373 });
                        loot_items(480, 501, new[] { 14147, 14146, 14145, 8018, 14074 });
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
                    lnk(433, 538, new[] { 14467, 14469, 14470, 14468, 14185 });
                    loot_items(433, 538, new[] { 4201, 4209, 4116, 6321 }); // Shortbow, Spiked Chain, Short Spear, Marauder Armor
                                                                            // NW of Noblig
                    lnk(421, 492, new[] { 14467, 14469, 14470, 14468, 14185, 14050, 14391 });
                    loot_items(421, 492, new[] { 4201, 4209, 4116 });
                    // Wolf Trainer Group
                    lnk(366, 472, new[] { 14466, 14352, 14467, 14469, 14470, 14468, 14185, 14050, 14391 });
                    loot_items(366, 472, new[] { 4201, 4209, 4116 });
                    // Ogre Shaman Group
                    lnk(449, 455, new[] { 14249, 14482, 14093, 14067, 14466, 14352, 14467, 14469, 14470, 14468, 14185, 14050, 14391 });
                    loot_items(449, 455, new[] { 4201, 4209, 4116 });
                    // Orc Shaman Group
                    lnk(494, 436, new[] { 14743, 14747, 14749, 14745, 14746, 14482, 14093, 14067, 14466, 14352, 14467, 14469, 14470, 14468, 14185, 14050, 14391 });
                    loot_items(494, 436, new[] { 4201, 4209, 4116 });
                    // Cave Entrance Group
                    lnk(527, 380, new[] { 14465, 14249, 14743, 14747, 14749, 14745, 14746, 14482, 14093, 14067, 14466, 14352, 14467, 14469, 14470, 14468, 14185, 14050, 14391 });
                    loot_items(527, 380, new[] { 4201, 4209, 4116 });
                    // Dire Bear
                    lnk(548, 430, new[] { 14506 });
                    // Cliff archers
                    lnk(502, 479, new[] { 14465, 14249, 14743, 14747, 14749, 14745, 14746, 14482, 14093, 14067, 14466, 14352, 14467, 14469, 14470, 14468, 14185, 14050, 14391 });
                    loot_items(502, 479, new[] { 4201, 4209, 4116 });
                    // Giant Snakes
                    lnk(547, 500, new[] { 14449 });
                    loot_items(547, 500, new[] { 4201, 4209, 4116 });
                    // Owlbear
                    lnk(607, 463, new[] { 14046 });
                    // Dokolb area
                    lnk(450, 519, new[] { 14640, 14465, 14249, 14743, 14747, 14749, 14745, 14746, 14482, 14093, 14067, 14466, 14352, 14467, 14469, 14470, 14468, 14185, 14050, 14391 });
                    loot_items(450, 519, new[] { 4201, 4209, 4116 });
                    // South of Dokolb Area
                    lnk(469, 548, new[] { 14188, 14465, 14249, 14743, 14747, 14749, 14745, 14746, 14482, 14093, 14067, 14466, 14352, 14467, 14469, 14470, 14468, 14185, 14050, 14391 });
                    loot_items(469, 548, new[] { 4201, 4209, 4116 });
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
                        loot_items(new[] { 4086, 6106, 10023 }, new[] { 6143, 4110, 4241, 4242, 4243, 6066, 4201, 4209, 4116 });
                        loot_items(490, 453, new[] { 4078, 6252, 6339, 6091 }, new[] { 6304, 4240, 6161, 6160, 4087, 4204 });
                    }

                }

            }

            if ((cur_map == 5191)) // Minotaur Lair
            {
                if (get_f("qs_autokill_nulb"))
                {
                    lnk(492, 486);
                    loot_items(492, 490, new[] { 4238, 6486, 6487 });
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
                    lnk(484, 481, new[] { 14677, 14733, 14725, 14724, 14726 });
                    loot_items(484, 481, new[] { 4132, 6057, 4082, 4208, 6076 });
                }

            }

            // TEMPLE OF ELEMENTAL EVIL	 #
            if ((cur_map == 5111)) // Tower Sentinel
            {
                if (get_f("qs_autokill_temple"))
                {
                    lnk(480, 490, 14157);
                    loot_items(480, 490);
                }

            }

            if ((cur_map == 5065)) // Brigand Tower
            {
                if (get_f("qs_autokill_temple"))
                {
                    lnk(477, 490, new[] { 14314, 14313, 14312, 14310, 14424, 14311, 14425 });
                    lnk(490, 480, new[] { 14314, 14313, 14312, 14310, 14424, 14311, 14425 });
                    loot_items(new[] { 10005, 6051 }, new[] { 4081, 6398, 4067 });
                    loot_items(490, 480, new[] { 10005, 6051 }, new[] { 4081, 6398, 4067, 4070, 4117, 5011 });
                }

            }

            if ((cur_map == 5066)) // Temple Level 1 - Earth Floor
            {
                if (get_f("qs_autokill_temple"))
                {
                    if (is_timed_autokill == 0)
                    {
                        StartTimer(100, () => autokill(cur_map, 1, 1));
                    }
                    else
                    {
                        // Stirges
                        lnk(415, 490, new[] { 14182 });
                        // Harpies & Ghouls
                        lnk(418, 574, new[] { 14095, 14129, 14243, 14128, 14136, 14135 });
                        lnk(401, 554, new[] { 14095, 14129, 14243, 14128, 14136, 14135 });
                        lnk(401, 554, new[] { 14095, 14129, 14243, 14128, 14136, 14135 });
                        lnk(421, 544, new[] { 14095, 14129, 14243, 14128, 14136, 14135 });
                        lnk(413, 522, new[] { 14095, 14129, 14243, 14128, 14136, 14135 });
                        loot_items(401, 554);
                        // Gel Cube + Grey Ooze
                        lnk(407, 594, new[] { 14095, 14129, 14139, 14140 });
                        loot_items(407, 600, new[] { 14448, 1049 }, new[] { 4121, 4118, 4113, 4116, 5005, 5098 });
                        // Corridor Ghouls
                        lnk(461, 600, new[] { 14095, 14129 });
                        // Corridor Gnolls
                        lnk(563, 600, new[] { 14078, 14079, 14080 });
                        loot_items(563, 600, new[] { 14078, 14079, 14080, 1049 });
                        // Corridor Ogre
                        lnk(507, 600, new[] { 14448 });
                        loot_items(507, 600, new[] { 14448, 1049 }, new[] { 4121, 4118, 4113, 4116, 5005, 5098 });
                        // Bone Corridor Undead
                        lnk(497, 519, new[] { 14107, 14081, 14082 });
                        lnk(467, 519, new[] { 14083, 14107, 14081, 14082 });
                        loot_items(507, 600, new[] { 14107, 14081, 14082 });
                        // Wonnilon Undead
                        lnk(536, 414, new[] { 14127, 14126, 14125, 14124, 14092, 14123 });
                        lnk(536, 444, new[] { 14127, 14126, 14125, 14124, 14092, 14123 });
                        // Huge Viper
                        lnk(550, 494, new[] { 14088 });
                        // Ogre + Goblins
                        lnk(565, 508, new[] { 14185, 14186, 14187, 14448 });
                        lnk(565, 494, new[] { 14185, 14186, 14187, 14448 });
                        loot_items(565, 508, new[] { 14185, 14186, 14187, 14448 });
                        // Ghasts near prisoners
                        lnk(545, 553, new[] { 14128, 14129, 14136, 14095, 14137, 14135 });
                        loot_items(545, 553, new[] { 1040 });
                        // Black Widow Spiders
                        lnk(440, 395, new[] { 14417 });
                        // NW Ghast room near hideout
                        lnk(390, 390, new[] { 14128, 14129, 14136, 14095, 14137, 14135 });
                        if (get_v("qs_autokill_temple_level_1_stage") == 0)
                        {
                            set_v("qs_autokill_temple_level_1_stage", 1);
                        }
                        else if (get_v("qs_autokill_temple_level_1_stage") == 1)
                        {
                            set_v("qs_autokill_temple_level_1_stage", 2);
                            // Gnoll & Bugbear southern room
                            lnk(515, 535, new[] { 14078, 14249, 14066, 14632, 14164 });
                            lnk(515, 549, new[] { 14067, 14631, 14078, 14249, 14066, 14632, 14164 });
                            loot_items(515, 540);
                            // Gnoll & Bugbear northern room
                            lnk(463, 535, new[] { 14248, 14631, 14188, 14636, 14083, 14184, 14078, 14249, 14066, 14632, 14164 });
                            loot_items(463, 535);
                            // Earth Temple Fighter eastern room
                            lnk(438, 505, new[] { 14337, 14338 });
                            loot_items(438, 505, new[] { 6074, 6077, 5005, 4123, 4134 });
                            // Bugbear Central Outpost
                            lnk(505, 476, new[] { 14165, 14163, 14164, 14162 });
                            loot_items(505, 476);
                            // Bugbears nea r Wonnilon
                            lnk(555, 436, new[] { 14165, 14163, 14164, 14162 });
                            lnk(555, 410, new[] { 14165, 14163, 14164, 14162 });
                            lnk(519, 416, new[] { 14165, 14163, 14164, 14162 });
                            loot_items(519, 416, range(14162, 14166), new[] { 6174 });
                            loot_items(555, 436, new[] { 14164 }, new[] { 6174 });
                            loot_items(555, 410, new[] { 14164 }, new[] { 6174 });
                            // Bugbears North of Romag
                            lnk(416, 430, range(14162, 14166));
                            loot_items(416, 430, range(14162, 14166), new[] { 6174 });
                        }
                        else if (get_v("qs_autokill_temple_level_1_stage") == 2)
                        {
                            // Jailer room
                            lnk(568, 462, new[] { 14165, 14164, 14229 });
                            loot_items(568, 462, new[] { 6174 });
                            // Earth Altar
                            lnk(474, 396, new[] { 14381, 14337 });
                            lnk(494, 396, new[] { 14381, 14337 });
                            lnk(484, 423, new[] { 14296 });
                            loot_items(480, 400, range(1041, 1045), new[] { 6082, 12228, 12031 }, new[] { 4070, 4193, 6056, 8025 });
                            loot_items(480, 400, new[] { 6082, 12228, 12031 }, new[] { 4070, 4193, 6056, 8025 });
                            // Troop Commander room
                            lnk(465, 477, (range(14162, 14166) + new[] { 14337, 14156, 14339 }));
                            lnk(450, 477, (range(14162, 14166) + new[] { 14337, 14156, 14339 }));
                            loot_items(450, 476, new[] { 4098, 6074, 6077, 6174 });
                            // Romag Room
                            lnk(441, 442, (new[] { 8045, 14154 } + range(14162, 14166) + new[] { 14337, 14156, 14339 }));
                            loot_items(441, 442, new[] { 6164, 9359, 8907, 9011 }, new[] { 10006, 6094, 4109, 8008 });
                        }

                    }

                }

            }

            if ((cur_map == 5067)) // Temple Level 2 - Water, Fire & Air Floor
            {
                if (get_f("qs_autokill_temple"))
                {
                    if (is_timed_autokill == 0)
                    {
                        StartTimer(100, () => autokill(cur_map, 1, 1));
                    }
                    else
                    {
                        // Kelno regroup
                        lnk(480, 494, new[] { 8092, 14380, 14292, 14067, 14078, 14079, 14080, 14184, 14187, 14215, 14216, 14275, 14159, 14160, 14161, 14158 });
                        lnk(490, 494, new[] { 8092, 14380, 14292, 14067, 14078, 14079, 14080, 14184, 14187, 14215, 14216, 14275, 14159, 14160, 14161, 14158 });
                        lnk(490, 514, new[] { 8092, 14380, 14292, 14067, 14078, 14079, 14080, 14184, 14187, 14215, 14216, 14275, 14159, 14160, 14161, 14158 });
                        loot_items(480, 494, new[] { 10009, 6085, 4219 }, new[] { 6049, 4109, 6166, 4112 });
                        loot_items(480, 514, new[] { 10009, 6085, 4219 }, new[] { 6049, 4109, 6166, 4112 });
                        loot_items(490, 514, new[] { 10009, 6085, 4219 }, new[] { 6049, 4109, 6166, 4112 });
                        // Corridor Ogres
                        lnk(480, 452, new[] { 14249, 14353 });
                        loot_items(480, 452, new[] { 4134 });
                        // Minotaur
                        foreach (var m_stat in ObjList.ListVicinity(new locXY(566, 408), ObjectListFilter.OLC_SCENERY))
                        {
                            if (m_stat.GetNameId() == 1615)
                            {
                                m_stat.Destroy();
                                cnk(14241);
                                loot_items(566, 408);
                            }

                        }

                        // Greater Temple Guards
                        lnk(533, 398, new[] { 14349, 14348 });
                        lnk(550, 422, new[] { 14349, 14348 });
                        loot_items(533, 398);
                        // Littlest Troll
                        lnk(471, 425, new[] { 14350 });
                        // Carrion Crawler
                        lnk(451, 424, new[] { 14190 });
                        // Fire Temple Bugbears Outside
                        lnk(397, 460, new[] { 14169 });
                        loot_items(397, 460, new[] { 14169 });
                        if (get_v("qs_autokill_temple_level_2_stage") == 0)
                        {
                            set_v("qs_autokill_temple_level_2_stage", 1);
                        }
                        else if (get_v("qs_autokill_temple_level_2_stage") == 1)
                        {
                            set_v("qs_autokill_temple_level_2_stage", 2);
                            // Feldrin
                            lnk(562, 438, new[] { 14311, 14312, 14314, 8041, 14253 });
                            loot_items(562, 438, new[] { 6083, 10010, 4082, 6086, 8010 }, new[] { 6091, 4070, 4117, 4114, 4062, 9426, 8014 });
                            // Prisoner Guards - Ogre + Greater Temple Bugbear
                            lnk(410, 440, new[] { 8065 });
                            loot_items(410, 440, new[] { 8065 });
                        }
                        else if (get_v("qs_autokill_temple_level_2_stage") == 2)
                        {
                            set_v("qs_autokill_temple_level_2_stage", 3);
                            // Water Temple
                            lnk(541, 573, new[] { 14375, 14231, 8091, 14247, 8028, 8027, 14181, 14046, 14239, 14225 });
                            // Juggernaut
                            lnk(541, 573, new[] { 14244 });
                            loot_items(541, 573, new[] { 10008, 6104, 4124, 6105, 9327, 9178 }, new[] { 6039, 9508, 9400, 6178, 6170, 9546, 9038, 9536 });
                            // Oohlgrist
                            lnk(483, 614, new[] { 14262, 14195 });
                            loot_items(483, 614, new[] { 6101, 6107 }, new[] { 6106, 12014, 6108 });
                            // Salamanders
                            lnk(433, 583, new[] { 8063, 14384, 14111 });
                            lnk(423, 583, new[] { 8063, 14384, 14111 });
                            loot_items(433, 583, new[] { 4028, 12016, 6101, 4136 }, new[] { 6121, 8020 });
                        }
                        else if (get_v("qs_autokill_temple_level_2_stage") == 3)
                        {
                            set_v("qs_autokill_temple_level_2_stage", 4);
                            // Alrrem
                            lnk(415, 499, new[] { 14169, 14211, 8047, 14168, 14212, 14167, 14166, 14344, 14224, 14343 });
                            loot_items(415, 499, new[] { 10007, 4079, 6082 }, new[] { 6094, 6060, 6062, 6068, 6069, 6335, 6269, 6074, 6077, 6093, 6167, 6177, 6172, 8019, 6039, 4131, 6050, 4077, 6311 });
                        }
                        else if (get_v("qs_autokill_temple_level_2_stage") == 4)
                        {
                            set_v("qs_autokill_temple_level_2_stage", 5);
                            // Big Bugbear Room
                            lnk(430, 361, (range(14174, 14178) + new[] { 14213, 14214, 14215, 14216 }));
                            lnk(430, 391, (range(14174, 14178) + new[] { 14213, 14214, 14215, 14216 }));
                            loot_items(430, 361, new[] { 6093, 6173, 6168, 6163, 6056 });
                            loot_items(430, 391, new[] { 6093, 6173, 6168, 6163, 6056 });
                        }

                    }

                }

            }

            if ((cur_map == 5105)) // Temple Level 3 - Thrommel Floor
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    if (is_timed_autokill == 0)
                    {
                        StartTimer(100, () => autokill(cur_map, 1, 1));
                    }
                    else
                    {
                        // Northern Trolls
                        lnk(394, 401, new[] { 14262 });
                        // Shadows
                        lnk(369, 431, new[] { 14289 });
                        lnk(369, 451, new[] { 14289 });
                        // Ogres:
                        lnk(384, 465, new[] { 14249 });
                        loot_items(384, 465);
                        // Ettin:
                        lnk(437, 524, new[] { 14238 });
                        loot_items(437, 524);
                        // Yellow Molds:
                        lnk(407, 564, new[] { 14276 });
                        // Groaning Spirit:
                        lnk(441, 459, new[] { 14280 });
                        loot_items(441, 459, new[] { 4218, 6090 }, new[] { 9214, 4191, 6058, 9123, 6214, 9492, 9391, 4002 });
                        // Key Trolls:
                        lnk(489, 535, new[] { 14262 });
                        lnk(489, 504, new[] { 14262 });
                        loot_items(489, 504, range(10016, 10020));
                        loot_items(489, 535, range(10016, 10020));
                        // Will o Wisps:
                        lnk(551, 583, new[] { 14291 });
                        // Lamia:
                        lnk(584, 594, new[] { 14342, 14274 });
                        loot_items(584, 594, new[] { 4083 });
                        // Jackals, Werejackals & Gargoyles:
                        lnk(511, 578, new[] { 14051, 14239, 14138 });
                        lnk(528, 556, new[] { 14051, 14239, 14138 });
                        // UmberHulks
                        lnk(466, 565, new[] { 14260 });
                        if (get_v("qs_autokill_temple_level_3_stage") == 0)
                        {
                            set_v("qs_autokill_temple_level_3_stage", 1);
                        }
                        else if (get_v("qs_autokill_temple_level_3_stage") == 1)
                        {
                            set_v("qs_autokill_temple_level_3_stage", 2);
                            // Gel Cube
                            lnk(476, 478, new[] { 14139 });
                            // Black Pudding
                            lnk(442, 384, new[] { 14143 });
                            // Goblins:
                            lnk(491, 389, (range(14183, 14188) + new[] { 14219, 14217 }));
                            loot_items(491, 389);
                            // Carrion Crawler:
                            lnk(524, 401, new[] { 14190 });
                            // Ogres near thrommel:
                            lnk(569, 412, new[] { 14249, 14353 });
                            loot_items(569, 412, new[] { 14249, 14353 }, new[] { 4134 });
                            // Leucrottas:
                            lnk(405, 590, new[] { 14351 });
                        }
                        else if (get_v("qs_autokill_temple_level_3_stage") == 2)
                        {
                            set_v("qs_autokill_temple_level_3_stage", 3);
                            // Pleasure dome:
                            lnk(553, 492, new[] { 14346, 14174, 14249, 14176, 14353, 14175, 14352, 14177 });
                            lnk(540, 480, new[] { 14346, 14174, 14249, 14176, 14353, 14175, 14352, 14177 });
                            lnk(569, 485, new[] { 8034, 14346, 14249, 14174, 14176, 14353, 14175, 14352, 14177 });
                            loot_items(540, 480, new[] { 8034, 14346, 14249, 14174, 14176, 14353, 14175, 14352, 14177 }, new[] { 6334 });
                            loot_items(553, 492, new[] { 8034, 14346, 14249, 14174, 14176, 14353, 14175, 14352, 14177 }, new[] { 6334 });
                            loot_items(569, 485, new[] { 8034, 14346, 14249, 14174, 14176, 14353, 14175, 14352, 14177 }, new[] { 6334 });
                            SetGlobalFlag(164, true); // Turns on Bugbears
                        }
                        else if (get_v("qs_autokill_temple_level_3_stage") == 3)
                        {
                            set_v("qs_autokill_temple_level_3_stage", 4);
                            // Pleasure dome - make sure:
                            lnk(553, 492, new[] { 14346, 14174, 14249, 14176, 14353, 14175, 14352, 14177 });
                            lnk(540, 480, new[] { 14346, 14174, 14249, 14176, 14353, 14175, 14352, 14177 });
                            lnk(569, 485, new[] { 8034, 14346, 14249, 14174, 14176, 14353, 14175, 14352, 14177 });
                            // Smigmal & Falrinth
                            var ass1 = GameSystems.MapObject.CreateObject(14782, new locXY(614, 455));
                            var ass2 = GameSystems.MapObject.CreateObject(14783, new locXY(614, 455));
                            lnk(614, 455, new[] { 14232, 14782, 14783 });
                            loot_items(614, 455, new[] { 10011, 6125, 6088 }, new[] { 4126, 6073, 6335, 8025 });
                            lnk(614, 480, new[] { 14110, 14177, 14346, 20123 });
                            loot_items(619, 480, new[] { 12560, 10012, 6119 }, new[] { 4179, 9173 });
                            loot_items(612, 503, new[] { 1033 }, new[] { 12560, 10012, 6119 }, new[] { 4179, 9173 });
                        }

                    }

                }

            }

            if ((cur_map == 5080)) // Temple Level 4 - Greater Temple
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    if (is_timed_autokill == 0)
                    {
                        StartTimer(100, () => autokill(cur_map, 1, 1));
                    }
                    else
                    {
                        SetGlobalFlag(820, true); // Trap Disabled
                        SetGlobalFlag(148, true); // Paida Sane
                                                  // Eastern Trolls
                        lnk(452, 552, new[] { 14262 });
                        // Western Trolls
                        lnk(513, 552, new[] { 14262 });
                        // Troll + Ettin
                        lnk(522, 586, new[] { 14262, 14238 });
                        loot_items(522, 586);
                        // Hill Giants
                        lnk(570, 610, new[] { 14218, 14217, 14219 });
                        loot_items(570, 610);
                        // Ettins
                        lnk(587, 580, new[] { 14238 });
                        loot_items(587, 580);
                        // More Trolls
                        lnk(555, 546, new[] { 14262 });
                        if (get_v("qs_autokill_temple_level_4_stage") == 0)
                        {
                            set_v("qs_autokill_temple_level_4_stage", 1);
                        }
                        else if (get_v("qs_autokill_temple_level_4_stage") == 1)
                        {
                            set_v("qs_autokill_temple_level_4_stage", 2);
                            // Bugbear quarters
                            lnk(425, 591, new[] { 14174, 14175, 14176, 14177, 14249, 14347, 14346 });
                            lnk(435, 591, new[] { 14174, 14175, 14176, 14177, 14249, 14347, 14346 });
                            lnk(434, 603, new[] { 14174, 14175, 14176, 14177, 14249, 14347, 14346 });
                            lnk(405, 603, new[] { 14174, 14175, 14176, 14177, 14249, 14347, 14346 });
                            loot_items(435, 590);
                            loot_items(425, 590);
                            loot_items(435, 603);
                            loot_items(405, 603);
                        }
                        else if (get_v("qs_autokill_temple_level_4_stage") == 2)
                        {
                            set_v("qs_autokill_temple_level_4_stage", 3);
                            // Insane Ogres
                            lnk(386, 584, new[] { 14356, 14355, 14354 });
                            loot_items(386, 584);
                            // Senshock's Posse
                            lnk(386, 528, new[] { 14296, 14298, 14174, 14110, 14302, 14292 });
                            foreach (var obj_1 in ObjList.ListVicinity(new locXY(386, 528), ObjectListFilter.OLC_NPC))
                            {
                                foreach (var pc_1 in PartyLeader.GetPartyMembers())
                                {
                                    obj_1.AIRemoveFromShitlist(pc_1);
                                    obj_1.SetReaction(pc_1, 50);
                                }

                            }

                            loot_items(386, 528);
                        }
                        else if (get_v("qs_autokill_temple_level_4_stage") == 3)
                        {
                            set_v("qs_autokill_temple_level_4_stage", 4);
                            // Hedrack's Posse
                            lnk(493, 442, new[] { 14238, 14239, 14218, 14424, 14296, 14298, 14174, 14176, 14177, 14110, 14302, 14292 });
                            foreach (var obj_1 in ObjList.ListVicinity(new locXY(493, 442), ObjectListFilter.OLC_NPC))
                            {
                                foreach (var pc_1 in PartyLeader.GetPartyMembers())
                                {
                                    obj_1.AIRemoveFromShitlist(pc_1);
                                    obj_1.SetReaction(pc_1, 50);
                                }

                            }

                            loot_items(493, 442);
                            lnk(465, 442, new[] { 14238, 14239, 14218, 14424, 14296, 14298, 14174, 14176, 14177, 14110, 14302, 14292 });
                            foreach (var obj_1 in ObjList.ListVicinity(new locXY(493, 442), ObjectListFilter.OLC_NPC))
                            {
                                foreach (var pc_1 in PartyLeader.GetPartyMembers())
                                {
                                    obj_1.AIRemoveFromShitlist(pc_1);
                                    obj_1.SetReaction(pc_1, 50);
                                }

                            }

                            loot_items(493, 442);
                            // Fungi
                            lnk(480, 375, new[] { 14274, 14143, 14273, 14276, 14142, 14141, 14282 });
                            loot_items(484, 374);
                            loot_items(464, 374);
                            lnk(480, 353, new[] { 14277, 14140 });
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
                    lnk(535, 525, new[] { 14300 });
                    // Bodaks
                    lnk(540, 568, new[] { 14328 });
                    // Salamanders
                    lnk(430, 557, new[] { 14111 });
                    // Salamanders near Balor
                    lnk(465, 447, new[] { 14111 });
                    // Efreeti
                    lnk(449, 494, new[] { 14340 });
                    // Fire Elementals + Snakes
                    lnk(473, 525, new[] { 14298, 14626 });
                    lnk(462, 532, new[] { 14298, 14626 });
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
                    lnk(465, 471, new[] { 14399, 14397 });
                    lnk(451, 491, new[] { 14399, 14397 });
                    lnk(471, 491, new[] { 14399, 14397 });
                    lnk(437, 485, new[] { 14741, 14397 });
                    if (is_timed_autokill == 0)
                    {
                        StartTimer(100, () => autokill(cur_map, 1, 1));
                    }
                    else
                    {
                        // Key
                        loot_items(new[] { 10022 }, 0);
                    }

                }

                return;
            }

            if ((cur_map == 5127)) // Drow Caves II - 2nd spidersfest
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    // Spiders
                    lnk(488, 477, new[] { 14741, 14397, 14620 });
                    // Drow
                    lnk(455, 485, new[] { 14708, 14737, 14736, 14735 });
                    loot_items(455, 481, new[] { 4132, 6057, 4082, 4208, 6076, 6046, 6045, 5011, 6040, 6041, 6120, 4193, 6160, 6161, 6334, 4081, 6223, 6073 });
                }

            }

            if ((cur_map == 5128)) // Drow Caves III - Drowfest I
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    // Garg. Spider
                    lnk(497, 486, new[] { 14524 });
                    // Drow
                    lnk(473, 475, new[] { 14399, 14708, 14737, 14736, 14735 });
                    lnk(463, 485, new[] { 14399, 14708, 14737, 14736, 14735 });
                    loot_items(475, 471, new[] { 4132, 6057, 4082, 4208, 6076, 6046, 6045, 5011, 6040, 6041, 6120, 4193, 6160, 6161, 6334, 4081, 6223, 6073 });
                    lnk(456, 487, new[] { 14399, 14708, 14737, 14736, 14735, 14734 });
                    lnk(427, 487, new[] { 14399, 14708, 14737, 14736, 14735, 14734 });
                    loot_items(465, 486, new[] { 4132, 6057, 4082, 4208, 6076, 6046, 6045, 5011, 6040, 6041, 6120, 4193, 6160, 6161, 6334, 4081, 6223, 6073, 6058 });
                    loot_items(425, 481, new[] { 4132, 6057, 4082, 4208, 6076, 6046, 6045, 5011, 6040, 6041, 6120, 4193, 6160, 6161, 6334, 4081, 6223, 6073, 6058 });
                    loot_items(475, 471, new[] { 4132, 6057, 4082, 4208, 6076, 6046, 6045, 5011, 6040, 6041, 6120, 4193, 6160, 6161, 6334, 4081, 6223, 6073, 6058 });
                    loot_items(425, 481, new[] { 6051, 4139, 4137 });
                }

            }

            if ((cur_map == 5129)) // Drow Caves IV - Spiders cont'd
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    lnk(477, 464, new[] { 14524, 14399, 14397 });
                    lnk(497, 454, new[] { 14524, 14399, 14397 });
                    lnk(467, 474, new[] { 14524, 14399, 14397, 14741 });
                    lnk(469, 485, new[] { 14524, 14399, 14397 });
                }

            }

            if ((cur_map == 5130)) // Drow Caves V - Young White Dragons
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    lnk(489, 455, new[] { 14707 });
                }

            }

            if ((cur_map == 5131)) // Drow Caves VI - Adult White Dragon
            {
                if (get_f("qs_autokill_greater_temple"))
                {
                    lnk(480, 535, new[] { 14999 });
                    loot_items(480, 535);
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
                    if (is_timed_autokill == 0)
                    {
                        StartTimer(100, () => autokill(cur_map, 1, 1));
                    }
                    else
                    {
                        lnk(484, 479, 8716); // Guntur Gladstone
                        SetGlobalVar(961, 2); // Have discussed wreaking havoc
                        loot_items(8716, new[] { 6202, 6306, 4126, 4161 });
                    }

                }

            }

            return;
        }

    }
}
