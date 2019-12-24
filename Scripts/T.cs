
using System;
using System.Collections.Generic;
using System.IO;
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
using OpenTemple.Core;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{

    public class T
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();
        
        // from itt import *

        // Last update 2010 - 09 - 30

        public static void tiuz(bool noiuz = false)
        {
            if (!noiuz)
            {
                SetGlobalVar(697, 1);
            }

            FadeAndTeleport(0, 0, 0, 5121, 509, 652);
        }
        
        public static GameObjectBody tf()
        {
            var obj = s(14000 + GetGlobalVar(998));
            Logger.Info("{0}", obj.GetInt32(obj_f.npc_pad_i_3) + " " + obj.GetInt32(obj_f.npc_pad_i_4) + " " + obj.GetInt32(obj_f.npc_pad_i_5));
            if (obj.GetInt32(obj_f.npc_pad_i_3) != 0 || obj.GetInt32(obj_f.npc_pad_i_4) != 0 || obj.GetInt32(obj_f.npc_pad_i_5) != 0)
            {
                Logger.Info("{0}", "   " + (14000 + GetGlobalVar(998)) + "  " + obj);
            }
            else
            {
                obj.Destroy();
            }

            SetGlobalVar(998, GetGlobalVar(998) + 1);
            return obj;
        }
        // see batch.py; imports preference from speedup.ini

        public static void t_mode()
        {
            SetGlobalFlag(403, Co8Settings.TestModeEnabled);
            ScriptDaemon.set_f("qs_disable_random_encounters", Co8Settings.RandomEncountersDisabled);
            ScriptDaemon.set_f("qs_autoloot", Co8Settings.QuickstartAutolootEnabled);
            ScriptDaemon.set_f("qs_autoconvert_jewels", Co8Settings.QuickstartAutolootAutoConvertJewels);
        }

        public static void list_flags()
        {
            var f_lines = "";
            for (var pp = 0; pp < 999; pp++)
            {
                if (GetGlobalFlag(pp))
                {
                    f_lines = f_lines + pp + "\n";
                    Logger.Info("{0}", pp.ToString());
                }

            }

            var path = Path.Join(Globals.GameFolders.UserDataFolder, "flag_list.txt");
            File.WriteAllText(path, f_lines);
        }

        public static void list_vars()
        {
            var f_lines = "";
            for (var pp = 0; pp < 999; pp++)
            {
                if (GetGlobalVar(pp) != 0)
                {
                    f_lines = f_lines + pp + "=" + GetGlobalVar(pp) + "\n";
                    Logger.Info("{0}", pp + "=" + GetGlobalVar(pp));
                }

            }

            var path = Path.Join(Globals.GameFolders.UserDataFolder, "var_list.txt");
            File.WriteAllText(path, f_lines);
        }
        public static void list_quests()
        {
            var f_lines = "";
            for (var pp = 0; pp < 999; pp++)
            {
                if (GetQuestState(pp) == QuestState.Completed)
                {
                    f_lines = f_lines + pp + "=" + GetQuestState(pp) + "\n";
                    Logger.Info("{0}", pp + "=" + GetQuestState(pp));
                }

            }

            var path = Path.Join(Globals.GameFolders.UserDataFolder, "completed_quest_list.txt");
            File.WriteAllText(path, f_lines);
        }
        public static void restup()
        {
            foreach (var pc in PartyLeader.GetPartyMembers())
            {
                pc.PendingSpellsToMemorized(); // Memorizes Spells
                pc.SetInt(obj_f.hp_damage, 0); // Removes all damage (doesn't work for companions?)
                if (pc.GetStat(Stat.level_bard) >= 1)
                {
                    pc.ResetCastSpells(Stat.level_bard);
                }

                if (pc.GetStat(Stat.level_sorcerer) >= 1)
                {
                    pc.ResetCastSpells(Stat.level_sorcerer);
                }

            }

        }
        public static void cnk(int proto_id, bool do_not_destroy = false, int how_many = 1)
        {
            // Create n' Kill
            // Meant to simulate actually killing the critter
            for (var pp = 0; pp < how_many; pp++)
            {
                var a = s(proto_id);
                var damage_dice = Dice.Constant(5000);
                a.Damage(PartyLeader, 0, damage_dice);
                if (!do_not_destroy)
                {
                    a.Destroy();
                }

            }
        }

        public static void idall()
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pc.IdentifyAll();
            }
        }

        public static void hpav(bool force_av = false)
        {
            // Checks for below avg HP
            // If HP is below avg, sets it to avg
            // If force_av is NOT 0, it forces average HP
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                var lev_barb = pc.GetStat(Stat.level_barbarian);
                var lev_bard = pc.GetStat(Stat.level_bard);
                var lev_figh = pc.GetStat(Stat.level_fighter);
                var lev_cler = pc.GetStat(Stat.level_cleric);
                var lev_drui = pc.GetStat(Stat.level_druid);
                var lev_wiza = pc.GetStat(Stat.level_wizard);
                var lev_sorc = pc.GetStat(Stat.level_sorcerer);
                var lev_monk = pc.GetStat(Stat.level_monk);
                var lev_rang = pc.GetStat(Stat.level_ranger);
                var lev_rogu = pc.GetStat(Stat.level_rogue);
                var lev_pala = pc.GetStat(Stat.level_paladin);
                var hp_min = 6 * lev_barb + 5 * (lev_figh + lev_pala) + 4 * (lev_cler + lev_rogu + lev_drui + lev_monk + lev_rang) + 3 * (lev_bard) + 2 * (lev_sorc + lev_wiza);
                hp_min = hp_min + pc.GetStat(Stat.level) / 2;
                if ((pc.GetInt(obj_f.hp_pts) < hp_min) || (force_av))
                {
                    pc.SetInt(obj_f.hp_pts, hp_min);
                }

            }

        }
        public static void stat_av()
        {
            // gives the PC "decent" / stereotypical stat scores (Point Buy 32)
            // For now, only "pure" characters
            // Stats are base stats, not accounting for racial mods
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                var lev_barb = pc.GetStat(Stat.level_barbarian);
                var lev_bard = pc.GetStat(Stat.level_bard);
                var lev_figh = pc.GetStat(Stat.level_fighter);
                var lev_cler = pc.GetStat(Stat.level_cleric);
                var lev_drui = pc.GetStat(Stat.level_druid);
                var lev_wiza = pc.GetStat(Stat.level_wizard);
                var lev_sorc = pc.GetStat(Stat.level_sorcerer);
                var lev_monk = pc.GetStat(Stat.level_monk);
                var lev_rang = pc.GetStat(Stat.level_ranger);
                var lev_rogu = pc.GetStat(Stat.level_rogue);
                var lev_pala = pc.GetStat(Stat.level_paladin);
                var lev_tot = pc.GetStat(Stat.level);
                if (lev_figh >= lev_tot * 3 / 4)
                {
                    pc.SetBaseStat(Stat.strength, 17 + lev_tot / 4);
                    pc.SetBaseStat(Stat.dexterity, 14);
                    pc.SetBaseStat(Stat.constitution, 14);
                    pc.SetBaseStat(Stat.intelligence, 13);
                    pc.SetBaseStat(Stat.wisdom, 10);
                    pc.SetBaseStat(Stat.charisma, 8);
                }

                if (lev_barb >= lev_tot * 3 / 4)
                {
                    pc.SetBaseStat(Stat.strength, 18 + lev_tot / 4);
                    pc.SetBaseStat(Stat.dexterity, 14);
                    pc.SetBaseStat(Stat.constitution, 16);
                    pc.SetBaseStat(Stat.intelligence, 8);
                    pc.SetBaseStat(Stat.wisdom, 8);
                    pc.SetBaseStat(Stat.charisma, 8);
                }

                if (lev_cler >= lev_tot * 3 / 4)
                {
                    pc.SetBaseStat(Stat.strength, 14);
                    pc.SetBaseStat(Stat.dexterity, 8);
                    pc.SetBaseStat(Stat.constitution, 12);
                    pc.SetBaseStat(Stat.intelligence, 8);
                    pc.SetBaseStat(Stat.wisdom, 18 + lev_tot / 4);
                    pc.SetBaseStat(Stat.charisma, 14);
                }

                if (lev_drui >= lev_tot * 3 / 4)
                {
                    pc.SetBaseStat(Stat.strength, 14);
                    pc.SetBaseStat(Stat.dexterity, 10);
                    pc.SetBaseStat(Stat.constitution, 14);
                    pc.SetBaseStat(Stat.intelligence, 10);
                    pc.SetBaseStat(Stat.wisdom, 18 + lev_tot / 4);
                    pc.SetBaseStat(Stat.charisma, 8);
                }

                if (lev_monk >= lev_tot * 3 / 4)
                {
                    pc.SetBaseStat(Stat.strength, 16 + lev_tot / 4);
                    pc.SetBaseStat(Stat.dexterity, 14);
                    pc.SetBaseStat(Stat.constitution, 12);
                    pc.SetBaseStat(Stat.intelligence, 10);
                    pc.SetBaseStat(Stat.wisdom, 16);
                    pc.SetBaseStat(Stat.charisma, 8);
                }

                if (lev_bard >= lev_tot * 3 / 4)
                {
                    pc.SetBaseStat(Stat.strength, 10);
                    pc.SetBaseStat(Stat.dexterity, 12);
                    pc.SetBaseStat(Stat.constitution, 10);
                    pc.SetBaseStat(Stat.intelligence, 16);
                    pc.SetBaseStat(Stat.wisdom, 12);
                    pc.SetBaseStat(Stat.charisma, 16 + lev_tot / 4);
                }

                if (lev_rogu >= lev_tot * 3 / 4)
                {
                    pc.SetBaseStat(Stat.strength, 10);
                    pc.SetBaseStat(Stat.dexterity, 16 + lev_tot / 4);
                    pc.SetBaseStat(Stat.constitution, 10);
                    pc.SetBaseStat(Stat.intelligence, 16);
                    pc.SetBaseStat(Stat.wisdom, 10);
                    pc.SetBaseStat(Stat.charisma, 14);
                }

                if (lev_wiza >= lev_tot * 3 / 4)
                {
                    pc.SetBaseStat(Stat.strength, 10);
                    pc.SetBaseStat(Stat.dexterity, 14);
                    pc.SetBaseStat(Stat.constitution, 14);
                    pc.SetBaseStat(Stat.intelligence, 18 + lev_tot / 4);
                    pc.SetBaseStat(Stat.wisdom, 10);
                    pc.SetBaseStat(Stat.charisma, 8);
                }

            }

            return;
        }
        public static string which_class(GameObjectBody pc)
        {
            var lev_barb = pc.GetStat(Stat.level_barbarian);
            var lev_bard = pc.GetStat(Stat.level_bard);
            var lev_figh = pc.GetStat(Stat.level_fighter);
            var lev_cler = pc.GetStat(Stat.level_cleric);
            var lev_drui = pc.GetStat(Stat.level_druid);
            var lev_wiza = pc.GetStat(Stat.level_wizard);
            var lev_sorc = pc.GetStat(Stat.level_sorcerer);
            var lev_monk = pc.GetStat(Stat.level_monk);
            var lev_rang = pc.GetStat(Stat.level_ranger);
            var lev_rogu = pc.GetStat(Stat.level_rogue);
            var lev_pala = pc.GetStat(Stat.level_paladin);
            var lev_tot = pc.GetStat(Stat.level);
            if ((lev_figh >= lev_tot * 3 / 4 && lev_tot > 1) || lev_figh == lev_tot)
            {
                return "fighter";
            }

            if ((lev_barb >= lev_tot * 3 / 4 && lev_tot > 1) || lev_barb == lev_tot)
            {
                return "barbarian";
            }

            if ((lev_cler >= lev_tot * 3 / 4 && lev_tot > 1) || lev_cler == lev_tot)
            {
                return "cleric";
            }

            if ((lev_drui >= lev_tot * 3 / 4 && lev_tot > 1) || lev_drui == lev_tot)
            {
                return "druid";
            }

            if ((lev_monk >= lev_tot * 3 / 4 && lev_tot > 1) || lev_monk == lev_tot)
            {
                return "monk";
            }

            if ((lev_bard >= lev_tot * 3 / 4 && lev_tot > 1) || lev_bard == lev_tot)
            {
                return "bard";
            }

            if ((lev_sorc >= lev_tot * 3 / 4 && lev_tot > 1) || lev_sorc == lev_tot)
            {
                return "sorcerer";
            }

            if ((lev_rogu >= lev_tot * 3 / 4 && lev_tot > 1) || lev_rogu == lev_tot)
            {
                return "rogue";
            }

            if ((lev_wiza >= lev_tot * 3 / 4 && lev_tot > 1) || lev_wiza == lev_tot)
            {
                return "wizard";
            }

            return "unknown";
        }
        public static void stat_items(int game_stage = 6)
        {
            // Gives stat boosting items
            var frostbrand = 0;
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                var lev_barb = pc.GetStat(Stat.level_barbarian);
                var lev_bard = pc.GetStat(Stat.level_bard);
                var lev_figh = pc.GetStat(Stat.level_fighter);
                var lev_cler = pc.GetStat(Stat.level_cleric);
                var lev_drui = pc.GetStat(Stat.level_druid);
                var lev_wiza = pc.GetStat(Stat.level_wizard);
                var lev_sorc = pc.GetStat(Stat.level_sorcerer);
                var lev_monk = pc.GetStat(Stat.level_monk);
                var lev_rang = pc.GetStat(Stat.level_ranger);
                var lev_rogu = pc.GetStat(Stat.level_rogue);
                var lev_pala = pc.GetStat(Stat.level_paladin);
                var lev_tot = pc.GetStat(Stat.level);
                if (QuickstartModule.which_class(pc) == "fighter")
                {
                    if (pc.FindItemByProto(6244) == null)
                    {
                        Utilities.create_item_in_inventory(6244, pc); // Belt of Str +6
                    }

                    if (pc.FindItemByProto(6201) == null)
                    {
                        Utilities.create_item_in_inventory(6201, pc); // Gloves of Dex +6
                    }

                    if (pc.FindItemByProto(6242) == null)
                    {
                        Utilities.create_item_in_inventory(6242, pc); // Amulet of Health +6
                    }

                    if (pc.FindItemByProto(6122) == null)
                    {
                        Utilities.create_item_in_inventory(6122, pc); // Full Plate +3
                    }

                    if (pc.FindItemByProto(6084) == null)
                    {
                        Utilities.create_item_in_inventory(6084, pc); // Ring +3
                    }

                }

                if (QuickstartModule.which_class(pc) == "barbarian")
                {
                    if (pc.FindItemByProto(6244) == null)
                    {
                        Utilities.create_item_in_inventory(6244, pc); // Belt of Str +6
                    }

                    if (pc.FindItemByProto(6201) == null)
                    {
                        Utilities.create_item_in_inventory(6201, pc); // Gloves of Dex +6
                    }

                    if (pc.FindItemByProto(6242) == null)
                    {
                        Utilities.create_item_in_inventory(6242, pc); // Amulet of Health +6
                    }

                    if (pc.FindItemByProto(6122) == null)
                    {
                        Utilities.create_item_in_inventory(6125, pc); // Elven Chain +3
                    }

                    if (frostbrand == 0)
                    {
                        if (pc.FindItemByProto(4136) == null)
                        {
                            Utilities.create_item_in_inventory(4136, pc); // Frostbrand
                        }

                    }

                    if (pc.FindItemByProto(6084) == null)
                    {
                        Utilities.create_item_in_inventory(6084, pc); // Ring +3
                    }

                }

                if (QuickstartModule.which_class(pc) == "cleric")
                {
                    if (pc.FindItemByProto(6251) == null)
                    {
                        Utilities.create_item_in_inventory(6251, pc); // Amulet of Wis +6
                    }

                    if (pc.FindItemByProto(6244) == null)
                    {
                        Utilities.create_item_in_inventory(6244, pc); // Belt of Str +6
                    }

                    if (pc.FindItemByProto(6122) == null)
                    {
                        Utilities.create_item_in_inventory(6122, pc); // Full Plate +3
                    }

                    if (pc.FindItemByProto(6084) == null)
                    {
                        Utilities.create_item_in_inventory(6084, pc); // Ring +3
                    }

                }

                if (QuickstartModule.which_class(pc) == "druid")
                {
                    if (pc.FindItemByProto(6251) == null)
                    {
                        Utilities.create_item_in_inventory(6251, pc); // Amulet of Wis +6
                    }

                    if (pc.FindItemByProto(6244) == null)
                    {
                        Utilities.create_item_in_inventory(6244, pc); // Belt of Str +6
                    }

                    if (pc.FindItemByProto(6084) == null)
                    {
                        Utilities.create_item_in_inventory(6084, pc); // Ring +3
                    }

                }

                if (QuickstartModule.which_class(pc) == "monk")
                {
                    if (pc.FindItemByProto(6242) == null)
                    {
                        Utilities.create_item_in_inventory(6242, pc); // Amulet of Health +6
                    }

                    if (pc.FindItemByProto(6244) == null)
                    {
                        Utilities.create_item_in_inventory(6244, pc); // Belt of Str +6
                    }

                    if (pc.FindItemByProto(6201) == null)
                    {
                        Utilities.create_item_in_inventory(6201, pc); // Gloves of Dex +6
                    }

                    if (pc.FindItemByProto(6084) == null)
                    {
                        Utilities.create_item_in_inventory(6084, pc); // Ring +3
                    }

                    if (pc.FindItemByProto(4125) == null)
                    {
                        Utilities.create_item_in_inventory(4125, pc); // Staff of Striking (+3)
                    }

                }

                if (QuickstartModule.which_class(pc) == "bard")
                {
                    if (pc.FindItemByProto(6242) == null)
                    {
                        Utilities.create_item_in_inventory(6242, pc); // Amulet of Health +6
                    }

                    if (pc.FindItemByProto(6254) == null)
                    {
                        Utilities.create_item_in_inventory(6254, pc); // Cloak of Cha +6
                    }

                }

                if (QuickstartModule.which_class(pc) == "sorcerer")
                {
                    if (pc.FindItemByProto(6254) == null)
                    {
                        Utilities.create_item_in_inventory(6254, pc); // Cloak of Cha +6
                    }

                    if (pc.FindItemByProto(6242) == null)
                    {
                        Utilities.create_item_in_inventory(6242, pc); // Amulet of Health +6
                    }

                    if (pc.FindItemByProto(6084) == null)
                    {
                        Utilities.create_item_in_inventory(6084, pc); // Ring +3
                    }

                }

                if (QuickstartModule.which_class(pc) == "rogue")
                {
                    if (pc.FindItemByProto(6201) == null)
                    {
                        Utilities.create_item_in_inventory(6201, pc); // Gloves of Dex +6
                    }

                    if (pc.FindItemByProto(6242) == null)
                    {
                        Utilities.create_item_in_inventory(6242, pc); // Amulet of Health +6
                    }

                    if (pc.FindItemByProto(6084) == null)
                    {
                        Utilities.create_item_in_inventory(6084, pc); // Ring +3
                    }

                }

                if (QuickstartModule.which_class(pc) == "wizard")
                {
                    if (pc.FindItemByProto(6248) == null)
                    {
                        Utilities.create_item_in_inventory(6248, pc); // Headbang of Int +6
                    }

                    if (pc.FindItemByProto(6242) == null)
                    {
                        Utilities.create_item_in_inventory(6242, pc); // Amulet of Health +6
                    }

                    if (pc.FindItemByProto(6084) == null)
                    {
                        Utilities.create_item_in_inventory(6084, pc); // Ring +3
                    }

                    if (pc.FindItemByProto(12580) == null)
                    {
                        Utilities.create_item_in_inventory(12580, pc); // Staff of False Life
                    }

                }

                pc.AdjustMoney(100 * 500000 - pc.GetMoney()); // Set to 500K GP
                pc.IdentifyAll();
                pc.WieldBestInAllSlots();
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
        public static List<ValueTuple<int, string, int, int>> tgd(int hp_desired, int radius)
        {
            // tough guy detector
            // returns a list of critters with HP greater than [hp_desired]
            // list includes the critters "name" , HP, and XY coordinates
            var gladius = new List<ValueTuple<int, string, int, int>>();
            foreach (var moshe in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if (moshe.DistanceTo(PartyLeader) <= radius && moshe.GetStat(Stat.hp_max) >= hp_desired)
                {
                    var (x, y) = moshe.GetLocation();
                    gladius.Add((moshe.GetNameId(), "hp=" + moshe.GetStat(Stat.hp_max), x, y));
                }

            }

            return gladius;
        }
        // AI tester, optionally select different proto (default - troll)

        public static GameObjectBody tai(int strat, int prot = 14262)
        {
            // def tai(strat, prot = 14262): # AI tester, optionally select different proto (default - troll)
            SetGlobalFlag(403, true); // Test mode flag
            var (xx, yy) = SelectedPartyLeader.GetLocation();
            // prot = 14262
            int y = yy + 3;
            var tro = GameSystems.MapObject.CreateObject(prot, new locXY(xx + 3, y));
            tro.SetScriptId(ObjScriptEvent.StartCombat, 3);
            tro.SetInt(obj_f.critter_strategy, strat);
            tro.SetBaseStat(Stat.hp_max, 300); // so he doesn't die from AoOs too quickly
            return tro;
        }
        // AI tester, by proto, optionally alter strategy

        public static GameObjectBody ptai(int prot, int strat)
        {
            // def ptai(prot, strat = -999): # AI tester, by proto, optionally alter strategy
            SetGlobalFlag(403, true); // Test mode flag
            var (xx, yy) = SelectedPartyLeader.GetLocation();
            int y = yy + 3;
            var tro = GameSystems.MapObject.CreateObject(prot, new locXY(xx + 3, y));
            // tro.scripts[15] = 0
            if (strat != -999)
            {
                tro.SetInt(obj_f.critter_strategy, 99 + strat);
            }

            tro.SetBaseStat(Stat.hp_max, 300);
            return tro;
        }
        public static void subd(int party_index)
        {
            // Deal massive nonlethal damage to selected party member
            GameSystems.Party.GetPartyGroupMemberN(party_index).Damage(null, DamageType.Subdual, Dice.Parse("50d50"));
            return;
        }
        public static void kil(int party_index)
        {
            // Deal massive LETHAL damage to selected party member
            GameSystems.Party.GetPartyGroupMemberN(party_index).Damage(null, DamageType.Bludgeoning, Dice.Parse("50d50"));
            return;
        }
        public static void pron(int party_index)
        {
            GameSystems.Party.GetPartyGroupMemberN(party_index).AddCondition("prone", 0, 0);
            return;
        }
        // adjust base dex

        public static void dex_adj(int party_index, int new_dex)
        {
            // def dex_adj(party_index,new_dex): # adjust base dex
            GameSystems.Party.GetPartyGroupMemberN(party_index).SetBaseStat(Stat.dexterity, new_dex);
            return;
        }
        // Test Spiritual Weapon Thingamajig

        public static GameObjectBody tsw()
        {
            // def tsw():	# Test Spiritual Weapon Thingamajig
            SetGlobalFlag(403, true); // Test mode flag
            var (xx, yy) = SelectedPartyLeader.GetLocation();
            // prot = 14262
            int y = yy + 3;
            var tro = GameSystems.MapObject.CreateObject(14262, new locXY(xx + 3, y));
            // tro.scripts[15] = 998 # py00998test_combat.py
            tro.SetInt(obj_f.critter_strategy, 99); // strategy
            tro.SetBaseStat(Stat.hp_max, 300); // so he doesn't die from AoOs too quickly
            return tro;
        }

        public static List<GameObjectBody> fnl(int obj_name, int radius)
        {
            var gladius = new List<GameObjectBody>();
            foreach (var moshe in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if (moshe.DistanceTo(PartyLeader) <= radius && moshe.GetNameId() == obj_name)
                {
                    gladius.Add(moshe);
                }

            }

            return gladius;
        }
        public static GameObjectBody bsp(int prot)
        {
            var a = GameSystems.MapObject.CreateObject(prot, SelectedPartyLeader.GetLocation());
            a.ClearNpcFlag(NpcFlag.KOS);
            a.Move(SelectedPartyLeader.GetLocation(), 0, 0);
            return a;
        }
        public static void killkos()
        {
            foreach (var moshe in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if (((moshe.GetNpcFlags() & NpcFlag.KOS) != 0 && (moshe.GetNpcFlags() & NpcFlag.KOS_OVERRIDE) == 0 && moshe.GetScriptId(ObjScriptEvent.WillKos) == 0 && moshe.GetLeader() == null))
                {
                    var damage_dice = Dice.Parse("50d50");
                    moshe.Damage(PartyLeader, DamageType.Bludgeoning, damage_dice);
                }

            }

        }
        public static void kf()
        {
            // Kill foes
            foreach (var moshe in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                var hostile = 0;
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    if (moshe.GetReaction(pc) <= 0)
                    {
                        hostile = 1;
                    }

                }

                if ((hostile == 1 && moshe.GetLeader() == null))
                {
                    // moshe.critter_kill_by_effect()
                    var damage_dice = Dice.Parse("50d50");
                    moshe.Damage(PartyLeader, DamageType.Bludgeoning, damage_dice);
                }

            }

        }

        // Kill unfriendlies
        public static void kuf(int c_name)
        {
            foreach (var moshe in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((moshe.GetReaction(PartyLeader) <= 0 || !moshe.IsFriendly(PartyLeader)) && (!((GameSystems.Party.PartyMembers).Contains(moshe.GetLeader())) && (moshe.GetObjectFlags() & ObjectFlag.DONTDRAW) == 0) && (moshe.GetNameId() == c_name || c_name == -1))
                {
                    // moshe.critter_kill_by_effect()
                    var damage_dice = Dice.Parse("50d50");
                    moshe.Damage(PartyLeader, DamageType.Bludgeoning, damage_dice);
                }

            }
        }

        // Kill unfriendlies
        public static void kuf(string c_name)
        {
            foreach (var moshe in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                var name = GameSystems.MapObject.GetDisplayName(moshe);
                if ((moshe.GetReaction(PartyLeader) <= 0 || !moshe.IsFriendly(PartyLeader)) && (!((GameSystems.Party.PartyMembers).Contains(moshe.GetLeader())) && (moshe.GetObjectFlags() & ObjectFlag.DONTDRAW) == 0) && name.Contains(c_name, StringComparison.CurrentCultureIgnoreCase))
                {
                    // moshe.critter_kill_by_effect()
                    var damage_dice = Dice.Parse("50d50");
                    moshe.Damage(PartyLeader, DamageType.Bludgeoning, damage_dice);
                }

            }
        }
        public static List<GameObjectBody> vlist3(int radius)
        {
            var gladius = new List<GameObjectBody>();
            foreach (var moshe in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if (moshe.DistanceTo(PartyLeader) <= radius)
                {
                    gladius.Add(moshe);
                }

            }

            return gladius;
        }
        public static List<(int, int, int)> nxy(int radius)
        {
            var gladius = new List<(int, int, int)>();
            foreach (var moshe in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if (moshe.DistanceTo(PartyLeader) <= radius)
                {
                    var (x, y) = moshe.GetLocation();
                    gladius.Add((moshe.GetNameId(), x, y));
                }

            }

            return gladius;
        }
        public static void hl(GameObjectBody obj)
        {
            // highlight the sucker
            AttachParticles("ef-minocloud", obj);
            return;
        }
        public static void make_slow()
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pc.SetBaseStat(Stat.dexterity, 4);
            }

            return;
        }
        public static bool fl(int flag_no)
        {
            if (!GetGlobalFlag(flag_no))
            {
                SetGlobalFlag(flag_no, true);
            }

            return GetGlobalFlag(flag_no);
        }
        public static int vr(int var_no)
        {
            return GetGlobalVar(var_no);
        }
        public static string tyme()
        {
            var calendar = GameSystems.TimeEvent.CampaignCalendar;
            return $"month = {calendar.Month},day = {calendar.Day}, hour = {calendar.Hour}, minute = {calendar.Minute}";
        }
        public static int gp(int name)
        {
            var a = fnn(name);
            if (a != null)
            {
                return a.GetInt32(obj_f.npc_pad_i_5);
            }
            else
            {
                return 0;
            }

        }
        public static void sp(int name, int num)
        {
            var a = fnn(name);
            if (a != null)
            {
                a.SetInt32(obj_f.npc_pad_i_5, num);
            }
        }
        public static void te()
        {
            // test earth temple stuff
            uberize();
            gimme(give_earth: true);
            TeleportShortcuts.earthaltar();
            return;
        }
        public static void ta()
        {
            // test air temple stuff
            SetGlobalFlag(108, true); // makes water bugbears defect
            uberize();
            gimme(give_air: true);
            TeleportShortcuts.airaltar();
            return;
        }
        public static void tw()
        {
            // test water temple stuff
            uberize();
            gimme(give_water: true);
            TeleportShortcuts.belsornig();
            return;
        }
        public static void t()
        {
            uberizeminor();
            gimme();
            TeleportShortcuts.hommlet();
            return;
        }

        public static void uberizeminor()
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pc.SetBaseStat(Stat.strength, 24);
                pc.SetBaseStat(Stat.hp_max, 70);
                pc.SetBaseStat(Stat.constitution, 24);
                pc.SetBaseStat(Stat.charisma, 24);
                pc.SetBaseStat(Stat.dexterity, 24);
                pc.SetBaseStat(Stat.wisdom, 24);
                if (pc.GetStat(Stat.level_wizard) > 0 || pc.GetStat(Stat.level_rogue) > 0 || pc.GetStat(Stat.level_bard) > 0)
                {
                    pc.SetBaseStat(Stat.intelligence, 18);
                }

            }

            return;
        }
        public static void uberize()
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pc.SetBaseStat(Stat.strength, 38);
                pc.SetBaseStat(Stat.hp_max, 300);
                pc.SetBaseStat(Stat.constitution, 45);
                pc.SetBaseStat(Stat.charisma, 100);
                pc.SetBaseStat(Stat.dexterity, 41);
                pc.SetBaseStat(Stat.wisdom, 100);
                if (pc.GetStat(Stat.level_wizard) > 0 || pc.GetStat(Stat.level_rogue) > 0 || pc.GetStat(Stat.level_bard) > 0)
                {
                    pc.SetBaseStat(Stat.intelligence, 18);
                }

            }

            return;
        }
        public static void uberizemajor()
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pc.SetBaseStat(Stat.strength, 210);
                pc.SetBaseStat(Stat.hp_max, 300);
                pc.SetBaseStat(Stat.constitution, 45);
                pc.SetBaseStat(Stat.charisma, 100);
                pc.SetBaseStat(Stat.dexterity, 100);
                pc.SetBaseStat(Stat.wisdom, 100);
                if (pc.GetStat(Stat.level_wizard) > 0 || pc.GetStat(Stat.level_rogue) > 0 || pc.GetStat(Stat.level_bard) > 0)
                {
                    pc.SetBaseStat(Stat.intelligence, 18);
                }

            }

            return;
        }

        public static void gimme_minor()
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if (pc.FindItemByProto(6105) == null)
                {
                    Utilities.create_item_in_inventory(6015, pc);
                }

                // if pc.item_find_by_proto(6109) == OBJ_HANDLE_NULL:
                // create_item_in_inventory( 6109, pc )
                // if pc.item_find_by_proto(6110) == OBJ_HANDLE_NULL:
                // create_item_in_inventory( 6110, pc )
                // if pc.item_find_by_proto(6111) == OBJ_HANDLE_NULL:
                // create_item_in_inventory( 6111, pc )
                // if pc.item_find_by_proto(6112) == OBJ_HANDLE_NULL:
                // create_item_in_inventory( 6112, pc )
                if (pc.FindItemByProto(6113) == null) // greater temple robes
                {
                    Utilities.create_item_in_inventory(6113, pc);
                }

                // the above are temple robes and eye of flame cloak
                if (pc.FindItemByProto(12262) == null)
                {
                    Utilities.create_item_in_inventory(12262, pc);
                }

                Utilities.create_item_in_inventory(6266, pc); // amulet AC +5
                Utilities.create_item_in_inventory(6101, pc); // ringa Fire Res 15
                Utilities.create_item_in_inventory(6115, pc); // bracers +5
                Utilities.create_item_in_inventory(5013, pc); // bolts +3
                Utilities.create_item_in_inventory(4085, pc); // sword +5
                var studded_leather = 0;
                if (pc.FindItemByProto(6056) != null)
                {
                    pc.FindItemByProto(6056).Destroy();
                    studded_leather = 1;
                }

                pc.WieldBestInAllSlots();
                if (studded_leather == 1)
                {
                    Utilities.create_item_in_inventory(6056, pc);
                }

                if (pc.FindItemByProto(4177) == null)
                {
                    Utilities.create_item_in_inventory(4177, pc);
                }

                // masterwork light xbow
                pc.IdentifyAll();
            }

        }
        public static void gimme(int gversion = 5, bool give_earth = false, bool give_air = false, bool give_water = false)
        {
            // Vanilla version has no wand of fireball (9th), so use gimme(1) for vanilla!
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if (pc.FindItemByProto(6105) == null && pc.ItemWornAt(EquipSlot.Cloak).GetNameId() != 3005) // Eye of Flame Cloak
                {
                    Utilities.create_item_in_inventory(6015, pc);
                }

                if (pc.FindItemByProto(6109) == null && give_air)
                {
                    Utilities.create_item_in_inventory(6109, pc); // Air Temple robes
                }

                if (pc.FindItemByProto(6110) == null && give_earth)
                {
                    Utilities.create_item_in_inventory(6110, pc); // Earth Temple robes
                }

                // if pc.item_find_by_proto(6111) == OBJ_HANDLE_NULL:
                // create_item_in_inventory( 6111, pc )
                if (pc.FindItemByProto(6112) == null && give_water)
                {
                    Utilities.create_item_in_inventory(6112, pc); // Water Temple robes
                }

                if (pc.FindItemByProto(6113) == null) // Greater Temple robes
                {
                    Utilities.create_item_in_inventory(6113, pc);
                }

                if (pc.FindItemByProto(12262) == null) // Wand of knock
                {
                    Utilities.create_item_in_inventory(12262, pc);
                }

                if (gversion >= 5)
                {
                    if (pc.FindItemByProto(12619) == null)
                    {
                        Utilities.create_item_in_inventory(12619, pc); // Wand of Fireball (9th)
                    }

                }

                if (pc.FindItemByProto(6266) == null)
                {
                    Utilities.create_item_in_inventory(6266, pc); // Amulet AC +5
                }

                if (pc.FindItemByProto(6101) == null)
                {
                    Utilities.create_item_in_inventory(6101, pc); // Ring of Fire Res 15
                }

                if (pc.FindItemByProto(6115) == null)
                {
                    Utilities.create_item_in_inventory(6115, pc); // Bracers +5
                }

                if (pc.FindItemByProto(4219) == null && pc.FindItemByProto(4118) == null)
                {
                    if ((!pc.GetAlignment().IsEvil()))
                    {
                        Utilities.create_item_in_inventory(4219, pc); // Holy Ranseur + 1
                    }
                    else
                    {
                        Utilities.create_item_in_inventory(4118, pc); // Glaive
                    }

                }

                Utilities.create_item_in_inventory(5013, pc); // Bolts +3 x 10
                if (pc.FindItemByProto(4085) == null)
                {
                    Utilities.create_item_in_inventory(4085, pc); // Longsword +5
                }

                // weapons and armor: xbow, holy ranseur
                var studded_leather = 0;
                if (pc.FindItemByProto(6056) != null)
                {
                    pc.FindItemByProto(6056).Destroy();
                    studded_leather = 1;
                }

                pc.WieldBestInAllSlots();
                if (studded_leather == 1)
                {
                    Utilities.create_item_in_inventory(6056, pc);
                }

                if (pc.FindItemByProto(4177) == null)
                {
                    Utilities.create_item_in_inventory(4177, pc);
                }

                // masterwork light xbow
                pc.IdentifyAll();
            }

            return;
        }
        // ------------------------------------------------------------------------------------------
        // @	@	@	@	@	@	@	@	@	@
        // ------------------------------------------------------------------------------------------

        public static GameObjectBody s(int prot)
        {
            var (x, y) = SelectedPartyLeader.GetLocation();
            var a = spawn(prot, x, y + 1);
            return a;
        }
        public static void tp(int map, int X, int Y)
        {
            FadeAndTeleport(0, 0, 0, map, X, Y);
            return;
        }
        public static locXY loc()
        {
            return PartyLeader.GetLocation();
        }

        // CB - sets entire groups experience points to xp

        public static int lvl()
        {
            // def lvl():  # CB - sets entire groups experience points to xp
            var pc = SelectedPartyLeader;
            foreach (var obj in pc.GetPartyMembers())
            {
                var curxp = obj.GetStat(Stat.experience);
                var newxp = curxp + 600000;
                obj.SetBaseStat(Stat.experience, newxp);
            }

            return 1;
        }
        
        public static GameObjectBody fnn(int name = -1, bool living_only = true, bool multiple = false)
        {
            var got_one = 0;
            foreach (var npc in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((npc.GetNameId() == name && !multiple))
                {
                    return npc;
                }
                else if (name == -1 | multiple)
                {
                    if (living_only && npc.IsUnconscious())
                    {
                        continue;

                    }

                    if (npc.GetNameId() != name && multiple)
                    {
                        continue;

                    }

                    got_one = 1;
                    var (xx, yy) = npc.GetLocation();
                    Logger.Info("{0}", npc + ",      name ID = " + npc.GetNameId() + ",    x = " + xx + ",    y = " + yy);
                    npc.FloatMesFileLine("mes/test.mes", xx, TextFloaterColor.Red);
                    npc.FloatMesFileLine("mes/test.mes", yy, TextFloaterColor.Red);
                }
            }

            if (got_one == 0 && name == -1 && living_only)
            {
                fnn(living_only: false);
            }

            return null;
        }
        public static GameObjectBody fnn(string name)
        {
            foreach (var npc in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                var npcName = GameSystems.MapObject.GetDisplayName(npc);
                if (npcName.Contains(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    return npc;
                }
            }

            return null;
        }
        // find PCs near

        public static GameObjectBody fpn(int name, bool living_only = true, bool multiple = false)
        {
            foreach (var pc in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((pc.GetNameId() == name && !multiple))
                {
                    return pc;
                }
                else if (name == -1 | multiple)
                {
                    if (living_only && pc.IsUnconscious())
                    {
                        continue;

                    }

                    if (pc.GetNameId() != name && multiple)
                    {
                        continue;

                    }

                    var (xx, yy) = pc.GetLocation();
                    Logger.Info("{0}", pc + ",      name ID = " + pc.GetNameId() + ",    x = " + xx + ",    y = " + yy);
                    pc.FloatMesFileLine("mes/test.mes", xx, TextFloaterColor.Red);
                    pc.FloatMesFileLine("mes/test.mes", yy, TextFloaterColor.Red);
                    return pc;
                }
            }

            return null;
        }

        public static GameObjectBody fpn(string name)
        {
            foreach (var pc in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_PC))
            {
                var pcName = GameSystems.MapObject.GetDisplayName(pc);
                if (pcName.Contains(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    return pc;
                }
            }

            return null;
        }
        // test adding PC via follower_add()

        public static void tpca()
        {
            var a = fpn("va"); // vadania
            SelectedPartyLeader.AddFollower(a);
        }

        public static List<GameObjectBody> vlist(int npc_name)
        {
            using var moshe = ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC);
            if (npc_name != -1)
            {
                var return_list = new List<GameObjectBody>();
                foreach (var obj in moshe)
                {
                        if (npc_name == obj.GetNameId())
                        {
                            return_list.Add(obj);
                        }
                }

                return return_list;
            }
            else
            {
                return new List<GameObjectBody>(moshe);
            }

        }
        public static List<GameObjectBody> vlist(List<int> npc_names)
        {
            using var moshe = ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC);
            var return_list = new List<GameObjectBody>();
            foreach (var obj in moshe)
            {
                if (npc_names.Contains(obj.GetNameId()))
                {
                    return_list.Add(obj);
                }
            }

            return return_list;
        }

        public static GameObjectBody spawn(int prot, int x, int y)
        {
            var moshe = GameSystems.MapObject.CreateObject(prot, new locXY(x, y));
            if ((moshe != null))
            {
                return moshe;
            }

            return null;
        }
        public static int alldie()
        {
            foreach (var obj in ObjList.ListVicinity(PartyLeader.GetLocation(), ObjectListFilter.OLC_CRITTERS))
            {
                if (!(PartyLeader.GetPartyMembers()).Contains(obj) && obj.GetNameId() != 14455)
                {
                    obj.KillWithDeathEffect();
                }

            }

            // damage_dice = dice_new( '104d20' )
            // obj.damage( OBJ_HANDLE_NULL, 0, damage_dice )
            return 1;
        }

    }
}
