
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

    public class T
    {
        // from itt import *

        // Last update 2010 - 09 - 30

        public static void tiuz(FIXME noiuz = 0)
        {
            if (noiuz == 0)
            {
                SetGlobalVar(697, 1);
            }

            FadeAndTeleport(0, 0, 0, 5121, 509, 652);
        }
        public static FIXME tf()
        {
            var obj = s(14000 + GetGlobalVar(998));
            Logger.Info("{0}", obj.obj_get_int/*Unknown*/(obj_f.npc_pad_i_3).ToString() + " " + obj.obj_get_int/*Unknown*/(obj_f.npc_pad_i_4).ToString() + " " + obj.obj_get_int/*Unknown*/(obj_f.npc_pad_i_5).ToString());
            if (obj.obj_get_int/*Unknown*/(obj_f.npc_pad_i_3) != 0 || obj.obj_get_int/*Unknown*/(obj_f.npc_pad_i_4) != 0 || obj.obj_get_int/*Unknown*/(obj_f.npc_pad_i_5) != 0)
            {
                Logger.Info("{0}", "   " + (14000 + GetGlobalVar(998)).ToString() + "  " + obj.ToString());
            }
            else
            {
                obj.destroy/*Unknown*/();
            }

            GetGlobalVar(998) += 1;
            return obj;
        }
        // see batch.py; imports preference from speedup.ini

        public static void t_mode()
        {
            try
            {
                var ff_t_mode = open("modules\\ToEE\\test_mode.ini", "r");
                var asdf = ff_t_mode.readline/*Unknown*/();
                while (asdf != "")
                {
                    var asdf_s = asdf.split/*Unknown*/("=");
                    asdf_s[0] = asdf_s[0].strip/*Unknown*/().lower/*Unknown*/();
                    asdf_s[1] = asdf_s[1].strip/*Unknown*/().lower/*Unknown*/();
                    if (asdf_s[0] == "Test_Mode_Enabled".lower/*String*/().strip/*Unknown*/())
                    {
                        // Enables flag 403, which is sort of a master switch for a lot of things
                        if (asdf_s[1] == "1")
                        {
                            SetGlobalFlag(403, true);
                        }
                        else
                        {
                            SetGlobalFlag(403, false);
                        }

                    }
                    else if (asdf_s[0] == "Random_Encounters_Disabled".lower/*String*/().strip/*Unknown*/())
                    {
                        if (asdf_s[1] == "1")
                        {
                            ScriptDaemon.set_f("qs_disable_random_encounters");
                        }
                        else
                        {
                            ScriptDaemon.set_f("qs_disable_random_encounters", 0);
                        }

                    }
                    else if (asdf_s[0] == "Quickstart_Autoloot_Enabled".lower/*String*/().strip/*Unknown*/())
                    {
                        if (asdf_s[1] == "1")
                        {
                            ScriptDaemon.set_f("qs_autoloot", 1);
                        }
                        else
                        {
                            ScriptDaemon.set_f("qs_autoloot", 0);
                        }

                    }
                    else if (asdf_s[0] == "Quickstart_Autoloot_AutoConvert_Jewels_Enabled".lower/*String*/().strip/*Unknown*/())
                    {
                        if (asdf_s[1] == "1")
                        {
                            ScriptDaemon.set_f("qs_autoconvert_jewels", 1);
                        }
                        else
                        {
                            ScriptDaemon.set_f("qs_autoconvert_jewels", 0);
                        }

                    }

                    asdf = ff_t_mode.readline/*Unknown*/();
                }

                ff_t_mode.close/*Unknown*/();
            }
            finally
            {
                var dummy = 1;
            }

            return;
        }
        public static void list_flags()
        {
            var ff = open("flag_list.txt", "w");
            var f_lines = "";
            for (var pp = 0; pp < 999; pp++)
            {
                if (GetGlobalFlag(pp))
                {
                    f_lines = f_lines + pp.ToString() + "\n";
                    Logger.Info("{0}", pp.ToString());
                }

            }

            ff.write/*Unknown*/(f_lines);
            ff.close/*Unknown*/();
            return;
        }
        public static void list_vars()
        {
            var ff = open("var_list.txt", "w");
            var f_lines = "";
            for (var pp = 0; pp < 999; pp++)
            {
                if (GetGlobalVar(pp) != 0)
                {
                    f_lines = f_lines + pp.ToString() + "=" + GetGlobalVar(pp).ToString() + "\n";
                    Logger.Info("{0}", pp.ToString() + "=" + GetGlobalVar(pp).ToString());
                }

            }

            ff.write/*Unknown*/(f_lines);
            ff.close/*Unknown*/();
            return;
        }
        public static void list_quests()
        {
            var ff = open("completed_quest_list.txt", "w");
            var f_lines = "";
            for (var pp = 0; pp < 999; pp++)
            {
                if (GetQuestState(pp) == QuestState.Completed)
                {
                    f_lines = f_lines + pp.ToString() + "=" + GetQuestState(pp).ToString() + "\n";
                    Logger.Info("{0}", pp.ToString() + "=" + GetQuestState(pp).ToString());
                }

            }

            ff.write/*Unknown*/(f_lines);
            ff.close/*Unknown*/();
            return;
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
        public static void cnk(FIXME proto_id, FIXME do_not_destroy = 0, FIXME how_many = 1, FIXME timer = 0)
        {
            // Create n' Kill
            // Meant to simulate actually killing the critter
            // if timer == 0:
            for (var pp = 0; pp < how_many; pp++)
            {
                var a = s(proto_id);
                var damage_dice = Dice.Parse("50d50");
                a.damage/*Unknown*/(PartyLeader, 0, damage_dice);
                if (do_not_destroy != 1)
                {
                    a.destroy/*Unknown*/();
                }

            }

            // else:
            // for pp in range(0, how_many):
            // game.timevent_add( cnk, (proto_id, do_not_destroy, 1, 0), (pp+1)*20 )
            return;
        }
        public static void idall()
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pc.IdentifyAll();
            }

            return;
        }
        public static void hpav(FIXME force_av = 0)
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
                hp_min = (int)(hp_min + pc.GetStat(Stat.level) / 2);
                if ((pc.GetInt(obj_f.hp_pts) < hp_min) || (force_av != 0))
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
        public static void stat_items(FIXME game_stage = 6)
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
        public static void dummy_func(GameObjectBody pc)
        {
            if (pc == pc)
            {
                if (QuickstartModule.which_class(pc) == "fighter")
                {
                    var dumm = 1;
                }

                if (QuickstartModule.which_class(pc) == "barbarian")
                {
                    var dumm = 1;
                }

                if (QuickstartModule.which_class(pc) == "cleric")
                {
                    var dumm = 1;
                }

                if (QuickstartModule.which_class(pc) == "druid")
                {
                    var dumm = 1;
                }

                if (QuickstartModule.which_class(pc) == "monk")
                {
                    var dumm = 1;
                }

                if (QuickstartModule.which_class(pc) == "bard")
                {
                    var dumm = 1;
                }

                if (QuickstartModule.which_class(pc) == "sorcerer")
                {
                    var dumm = 1;
                }

                if (QuickstartModule.which_class(pc) == "rogue")
                {
                    var dumm = 1;
                }

                if (QuickstartModule.which_class(pc) == "wizard")
                {
                    var dumm = 1;
                }

            }

        }
        public static void tenc()
        {
            // def tenc(): #test encroachment gescheft
            GameObjectBody beac = null;
            foreach (var npc in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if (npc.GetNameId() == 14811)
                {
                    beac = npc;
                    break;

                }

            }

            var countt_encroachers = 1;
            var countt_all = 1;
            foreach (var npc in ObjList.ListVicinity(beac.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                beac.FloatMesFileLine("mes/test.mes", countt_all, TextFloaterColor.Red);
                countt_all += 1;
                if (ScriptDaemon.is_far_from_party(npc, 48) && !npc.IsUnconscious())
                {
                    attachee.float_mesfile_line/*Unknown*/("mes/test.mes", countt_encroachers, 2);
                    countt_encroachers += 1;
                    var joe = Utilities.party_closest(npc);
                    ScriptDaemon.encroach(npc, joe);
                }

            }

        }
        public static List<GameObjectBody> dsb(int radius, FIXME ci = 0, FIXME hndl = 0, FIXME nxy = 0, FIXME cx = 0, FIXME cy = 0, FIXME ec = normal)
        {
            // detect script bearers
            // floats their object name, description, and san_dying script
            // ec -> extra command
            // ec = 'strat' -> return strategy type, e.g.: type dsb(40, ec='s') in the console to reveal strategy type
            // ci -> center location    OR specify cx, cy
            var center = SelectedPartyLeader.GetLocation();
            if (ci != 0 && typeof(ci) == typeof(1))
{
                center = ci;
            }

            if (typeof(ci) == typeof("start") && ec == "normal")
{
                ec = ci; // because sometimes I forget the ec in "dsb(ec = '...')
            }

            if (cx == 0 || cy == 0)
            {
                (cx, cy) = center;
            }

            var scriptee_list = new List<GameObjectBody>();
            foreach (var dude in ObjList.ListVicinity(center, ObjectListFilter.OLC_NPC))
            {
                var (dudex, dudey) = dude.GetLocation();
                if ((Math.Pow((dudex - cx), 2) + Math.Pow((dudey - cy), 2)) <= Math.Pow(radius, 2))
                {
                    scriptee_list.Add((dude.GetNameId(), dudex, dudey, dude.GetScriptId(ObjScriptEvent.Dying)));
                    if (dude.GetNameId() >= 14000)
                    {
                        dude.FloatMesFileLine("mes/description.mes", dude.GetNameId(), TextFloaterColor.Red);
                    }
                    else if (dude.GetNameId() >= 8000 && dude.GetNameId() <= 9000)
                    {
                        dude.FloatMesFileLine("oemes\\oname.mes", dude.GetNameId(), TextFloaterColor.Red);
                    }

                    if (ec.ToString() == "s")
                    {
                        dude.FloatMesFileLine("mes/test.mes", dude.GetInt(obj_f.critter_strategy), TextFloaterColor.Red);
                    }
                    else if (ec.ToString() == "15" || ec.ToString() == "san_start_combat" || ec.ToString() == "start_combat")
                    {
                        if (dude.GetScriptId(ObjScriptEvent.StartCombat) != 0) // san_start_combat
                        {
                            dude.FloatMesFileLine("mes/test.mes", dude.GetScriptId(ObjScriptEvent.StartCombat), TextFloaterColor.Red);
                        }

                    }
                    else if (ec.ToString() == "19" || ec.ToString() == "san_heartbeat" || ec.ToString() == "heartbeat")
                    {
                        if (dude.GetScriptId(ObjScriptEvent.Heartbeat) != 0) // san_start_combat
                        {
                            dude.FloatMesFileLine("mes/test.mes", dude.GetScriptId(ObjScriptEvent.Heartbeat), TextFloaterColor.Red);
                        }

                    }
                    else
                    {
                        if (dude.GetScriptId(ObjScriptEvent.Dying) != 0) // san_dying
                        {
                            dude.FloatMesFileLine("mes/test.mes", dude.GetScriptId(ObjScriptEvent.Dying), TextFloaterColor.Red);
                        }

                    }

                }

            }

            return scriptee_list;
        }
        public static void generate_mes_file()
        {
            var f = open;
            f = open("transaction_sum.mes", "w");
            f.write/*Unknown*/("{" + 0.ToString() + "}{Less than 1 GP}\n");
            for (var pp = 1; pp < 1000; pp++)
            {
                f.write/*Unknown*/("{" + pp.ToString() + "}{" + pp.ToString() + " GP}\n");
            }

            for (var pp = 0; pp < 900; pp++)
            {
                f.write/*Unknown*/("{" + (1000 + 10 * pp).ToString() + "}{" + (1000 + 10 * pp).ToString() + " GP}\n");
            }

            for (var pp = 0; pp < 900; pp++)
            {
                f.write/*Unknown*/("{" + (10000 + 100 * pp).ToString() + "}{" + (10000 + 100 * pp).ToString() + " GP}\n");
            }

            for (var pp = 0; pp < 900; pp++)
            {
                f.write/*Unknown*/("{" + (100000 + 1000 * pp).ToString() + "}{" + (100000 + 1000 * pp).ToString() + " GP}\n");
            }

            f.close/*Unknown*/();
        }
        public static List<GameObjectBody> tgd(FIXME hp_desired, int radius)
        {
            // tough guy detector
            // returns a list of critters with HP greater than [hp_desired]
            // list includes the critters "name" , HP, and XY coordinates
            var gladius = new List<GameObjectBody>();
            foreach (var moshe in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if (moshe.DistanceTo(PartyLeader) <= radius && moshe.GetStat(Stat.hp_max) >= hp_desired)
                {
                    var (x, y) = lta(moshe.GetLocation());
                    gladius.Add((moshe.GetNameId(), "hp=" + moshe.GetStat(Stat.hp_max).ToString(), x, y));
                }

            }

            return gladius;
        }
        // AI tester, optionally select different proto (default - troll)

        public static GameObjectBody tai(FIXME strat, FIXME prot = 14262)
        {
            // def tai(strat, prot = 14262): # AI tester, optionally select different proto (default - troll)
            SetGlobalFlag(403, true); // Test mode flag
            var (xx, yy) = lta(SelectedPartyLeader.GetLocation());
            // prot = 14262
            var tro = GameSystems.MapObject.CreateObject(prot, lfa(xx + 3, yy + 3));
            tro.SetScriptId(ObjScriptEvent.StartCombat, 3);
            tro.SetInt(obj_f.critter_strategy, strat);
            tro.SetBaseStat(Stat.hp_max, 300); // so he doesn't die from AoOs too quickly
            return tro;
        }
        // AI tester, by proto, optionally alter strategy

        public static GameObjectBody ptai(FIXME prot, FIXME strat)
        {
            // def ptai(prot, strat = -999): # AI tester, by proto, optionally alter strategy
            SetGlobalFlag(403, true); // Test mode flag
            var (xx, yy) = lta(SelectedPartyLeader.GetLocation());
            var tro = GameSystems.MapObject.CreateObject(prot, lfa(xx + 3, yy + 3));
            // tro.scripts[15] = 0
            if (strat != -999)
            {
                tro.SetInt(obj_f.critter_strategy, 99 + strat);
            }

            tro.SetBaseStat(Stat.hp_max, 300);
            return tro;
        }
        public static void subd(FIXME party_index)
        {
            // Deal massive nonlethal damage to selected party member
            GameSystems.Party.GetPartyGroupMemberN(party_index).Damage(null, DamageType.Subdual, Dice.Parse("50d50"));
            return;
        }
        public static void kil(FIXME party_index)
        {
            // Deal massive LETHAL damage to selected party member
            GameSystems.Party.GetPartyGroupMemberN(party_index).Damage(null, DamageType.Bludgeoning, Dice.Parse("50d50"));
            return;
        }
        public static void pron(FIXME party_index)
        {
            GameSystems.Party.GetPartyGroupMemberN(party_index).AddCondition("prone", 0, 0);
            return;
        }
        // adjust base dex

        public static void dex_adj(FIXME party_index, FIXME new_dex)
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
            var (xx, yy) = lta(SelectedPartyLeader.GetLocation());
            // prot = 14262
            var tro = GameSystems.MapObject.CreateObject(14262, lfa(xx + 3, yy + 3));
            // tro.scripts[15] = 998 # py00998test_combat.py
            tro.SetInt(obj_f.critter_strategy, 99); // strategy
            tro.SetBaseStat(Stat.hp_max, 300); // so he doesn't die from AoOs too quickly
            return tro;
        }
        public static List<GameObjectBody> vrs(FIXME var_s, FIXME var_e)
        {
            var bloke = new List<GameObjectBody>();
            foreach (var snoike in new[] { var_s, var_e + 1 })
            {
                bloke.Add(GetGlobalVar(snoike));
            }

            return bloke;
        }
        public static List<GameObjectBody> fnl(GameObjectBody obj_name, int radius)
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
        public static GameObjectBody bsp(FIXME prot)
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
                    // moshe.critter_kill_by_effect()
                    var damage_dice = Dice.Parse("50d50");
                    moshe.Damage(PartyLeader, DamageType.Bludgeoning, damage_dice);
                }

            }

            return;
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
        public static void kuf(FIXME c_name)
        {
            // Kill unfriendlies
            // c_name - of particular name
            if (typeof(c_name) == typeof(1))
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
else if (typeof(c_name) == typeof("asdf"))
{
                foreach (var moshe in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if ((moshe.GetReaction(PartyLeader) <= 0 || !moshe.IsFriendly(PartyLeader)) && (!((GameSystems.Party.PartyMembers).Contains(moshe.GetLeader())) && (moshe.GetObjectFlags() & ObjectFlag.DONTDRAW) == 0) && (moshe.ToString().lower/*String*/().find/*Unknown*/(c_name.lower/*Unknown*/()) != -1))
                    {
                        // moshe.critter_kill_by_effect()
                        var damage_dice = Dice.Parse("50d50");
                        moshe.Damage(PartyLeader, DamageType.Bludgeoning, damage_dice);
                    }

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
        public static List<GameObjectBody> nxy(int radius)
        {
            var gladius = new List<GameObjectBody>();
            foreach (var moshe in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if (moshe.DistanceTo(PartyLeader) <= radius)
                {
                    var (x, y) = lta(moshe.GetLocation());
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
        public static bool fl(FIXME flag_no)
        {
            if (!GetGlobalFlag(flag_no))
            {
                SetGlobalFlag(flag_no, true);
            }

            return GetGlobalFlag(flag_no);
        }
        public static int vr(FIXME var_no)
        {
            return GetGlobalVar(var_no);
        }
        public static string tyme()
        {
            return ("month = " + CurrentTime.time_game_in_months/*Time*/(CurrentTime).ToString(), "day = " + CurrentTime.time_game_in_days/*Time*/(CurrentTime).ToString(), "hour = " + CurrentTime.time_game_in_hours/*Time*/(CurrentTime).ToString(), "minute = " + CurrentTime.time_game_in_minutes/*Time*/(CurrentTime).ToString());
        }
        public static GameObjectBody gp(FIXME name)
        {
            var a = fnn(name);
            if (a != null)
            {
                return a.obj_get_int/*Unknown*/(obj_f.npc_pad_i_5);
            }
            else
            {
                return null;
            }

        }
        public static GameObjectBody sp(FIXME name, FIXME num)
        {
            var a = fnn(name);
            if (a != null)
            {
                a.obj_set_int/*Unknown*/(obj_f.npc_pad_i_5, num);
            }
            else
            {
                return null;
            }

        }
        public static void dof(GameObjectBody obj)
        {
            // Display Object Flags
            // A handy little function to display an objects' object flags
            var col_size = 10;
            var object_flags_list = new[] { "OF_DESTROYED", "OF_OFF", "OF_FLAT", "OF_TEXT", "OF_SEE_THROUGH", "OF_SHOOT_THROUGH", "OF_TRANSLUCENT", "OF_SHRUNK", "OF_DONTDRAW", "OF_INVISIBLE", "OF_NO_BLOCK", "OF_CLICK_THROUGH", "OF_INVENTORY", "OF_DYNAMIC", "OF_PROVIDES_COVER", "OF_RANDOM_SIZE", "OF_NOHEIGHT", "OF_WADING", "OF_UNUSED_40000", "OF_STONED", "OF_DONTLIGHT", "OF_TEXT_FLOATER", "OF_INVULNERABLE", "OF_EXTINCT", "OF_TRAP_PC", "OF_TRAP_SPOTTED", "OF_DISALLOW_WADING", "OF_UNUSED_0800000", "OF_HEIGHT_SET", "OF_ANIMATED_DEAD", "OF_TELEPORTED", "OF_RADIUS_SET" };
            var lista = new List<GameObjectBody>();
            for (var p = 0; p < 31; p++)
            {
                lista.Add("".join/*String*/(new[] { object_flags_list[p], " - ", ((obj.GetObjectFlags() & (1 << p)) != 0).ToString() }));
            }

            lista.Add("".join/*String*/(new[] { object_flags_list[31], " - ", ((obj.GetObjectFlags() & ObjectFlag.RADIUS_SET) != 0).ToString() }));
            var lenmax = 1;
            for (var p = 0; p < 31; p++)
            {
                if (lista[p].Count > lenmax)
                {
                    lenmax = lista[p].Count;
                }

            }

            // print 'lenmax = ',str(lenmax)
            Logger.Info("");
            for (var p = 0; p < col_size + 1; p++)
            {
                var len1 = "".join/*String*/(new[] { object_flags_list[p], " - ", ((obj.GetObjectFlags() & (1 << p)) != 0).ToString() }).Count;
                var len2 = "".join/*String*/(new[] { object_flags_list[p + col_size + 1], " - ", ((obj.GetObjectFlags() & (1 << p + col_size + 1)) != 0).ToString() }).Count;
                if (p >= col_size - 1)
                {
                    var hau = ObjectFlag.RADIUS_SET;
                }
                else
                {
                    var hau = (1 << p + 2 * col_size + 2);
                }

                var har1 = "";
                var har2 = "";
                for (var p1 = 0; p1 < lenmax - len1 + 1; p1++)
                {
                    har1 += "  ";
                }

                for (var p2 = 0; p2 < lenmax - len2 + 1; p2++)
                {
                    har2 += "   ";
                }

                if (p < col_size)
                {
                    Logger.Info("{0}{1}{2}{3}{4}", "".join/*String*/(new[] { object_flags_list[p], " - ", ((obj.GetObjectFlags() & (1 << p)) != 0).ToString() }), har1, "".join/*String*/(new[] { object_flags_list[p + col_size + 1], " - ", ((obj.GetObjectFlags() & (1 << p + col_size + 1)) != 0).ToString() }), har2, "".join/*String*/(new[] { object_flags_list[p + 2 * col_size + 2], " - ", ((obj.GetObjectFlags() & hau) != 0).ToString() }));
                }
                else
                {
                    Logger.Info("{0}{1}{2}", "".join/*String*/(new[] { object_flags_list[p], " - ", ((obj.GetObjectFlags() & (1 << p)) != 0).ToString() }), har1, "".join/*String*/(new[] { object_flags_list[p + col_size + 1], " - ", ((obj.GetObjectFlags() & (1 << p + col_size + 1)) != 0).ToString() }));
                }

            }

            return;
        }
        public static void dnf(GameObjectBody obj)
        {
            // A handy little function to display an objects' NPC flags
            var col_size = 10;
            var npc_flags_list = new[] { "ONF_EX_FOLLOWER", "ONF_WAYPOINTS_DAY", "ONF_WAYPOINTS_NIGHT", "ONF_AI_WAIT_HERE", "ONF_AI_SPREAD_OUT", "ONF_JILTED", "ONF_LOGBOOK_IGNORES", "ONF_UNUSED_00000080", "ONF_KOS", "ONF_USE_ALERTPOINTS", "ONF_FORCED_FOLLOWER", "ONF_KOS_OVERRIDE", "ONF_WANDERS", "ONF_WANDERS_IN_DARK", "ONF_FENCE", "ONF_FAMILIAR", "ONF_CHECK_LEADER", "ONF_NO_EQUIP", "ONF_CAST_HIGHEST", "ONF_GENERATOR", "ONF_GENERATED", "ONF_GENERATOR_RATE1", "ONF_GENERATOR_RATE2", "ONF_GENERATOR_RATE3", "ONF_DEMAINTAIN_SPELLS", "ONF_UNUSED_02000000", "ONF_UNUSED_04000000", "ONF_UNUSED08000000", "ONF_BACKING_OFF", "ONF_NO_ATTACK", "ONF_BOSS_MONSTER", "ONF_EXTRAPLANAR" };
            var lista = new List<GameObjectBody>();
            for (var p = 0; p < 31; p++)
            {
                lista.Add("".join/*String*/(new[] { npc_flags_list[p], " - ", ((obj.GetNpcFlags() & (1 << p)) != 0).ToString() }));
            }

            lista.Add("".join/*String*/(new[] { npc_flags_list[31], " - ", ((obj.GetNpcFlags() & ObjectFlag.RADIUS_SET) != 0).ToString() }));
            var lenmax = 1;
            for (var p = 0; p < 31; p++)
            {
                if (lista[p].Count > lenmax)
                {
                    lenmax = lista[p].Count;
                }

            }

            // print 'lenmax = ',str(lenmax)
            Logger.Info("");
            for (var p = 0; p < col_size + 1; p++)
            {
                var len1 = "".join/*String*/(new[] { npc_flags_list[p], " - ", ((obj.GetNpcFlags() & (1 << p)) != 0).ToString() }).Count;
                var len2 = "".join/*String*/(new[] { npc_flags_list[p + col_size + 1], " - ", ((obj.GetNpcFlags() & (1 << p + col_size + 1)) != 0).ToString() }).Count;
                if (p >= col_size - 1)
                {
                    var hau = NpcFlag.EXTRAPLANAR;
                }
                else
                {
                    var hau = (1 << p + 2 * col_size + 2);
                }

                var har1 = "";
                var har2 = "";
                for (var p1 = 0; p1 < lenmax - len1 + 1; p1++)
                {
                    har1 += "  ";
                }

                for (var p2 = 0; p2 < lenmax - len2 + 1; p2++)
                {
                    har2 += "   ";
                }

                if (p < col_size)
                {
                    Logger.Info("{0}{1}{2}{3}{4}", "".join/*String*/(new[] { npc_flags_list[p], " - ", ((obj.GetNpcFlags() & (1 << p)) != 0).ToString() }), har1, "".join/*String*/(new[] { npc_flags_list[p + col_size + 1], " - ", ((obj.GetNpcFlags() & (1 << p + col_size + 1)) != 0).ToString() }), har2, "".join/*String*/(new[] { npc_flags_list[p + 2 * col_size + 2], " - ", ((obj.GetNpcFlags() & hau) != 0).ToString() }));
                }
                else
                {
                    Logger.Info("{0}{1}{2}", "".join/*String*/(new[] { npc_flags_list[p], " - ", ((obj.GetNpcFlags() & (1 << p)) != 0).ToString() }), har1, "".join/*String*/(new[] { npc_flags_list[p + col_size + 1], " - ", ((obj.GetNpcFlags() & (1 << p + col_size + 1)) != 0).ToString() }));
                }

            }

            return;
        }
        public static void te()
        {
            // test earth temple stuff
            Utilities.uberize();
            gimme(give_earth: 1);
            TeleportShortcuts.earthaltar();
            return;
        }
        public static void ta()
        {
            // test air temple stuff
            SetGlobalFlag(108, true); // makes water bugbears defect
            Utilities.uberize();
            gimme(give_air: 1);
            TeleportShortcuts.airaltar();
            return;
        }
        public static void tw()
        {
            // test water temple stuff
            Utilities.uberize();
            gimme(give_water: 1);
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
        public static void portraits_verify()
        {
            // this assumes portraits are laid out in {number} {file} format
            // Otherwise the script can get borked
            // Init
            var s = "initial value";
            var portrait_index = -1;
            var previous_portrait = -1;
            var found_error_flag = 0;
            var ff = open("modules\\ToEE\\portrait_checking_result.txt", "w");
            var i_file = open("portraits.mes", "r");
            while (s != "")
            {
                s = i_file.readline/*Unknown*/();
                var s2 = s.split/*Unknown*/("{");
                if (s2.Count == 3) // check if it's an actual portrait line with {number} {file} format - will return an array ['','number} ','file}'] entry
                {
                    var s3 = s2[1].replace/*Unknown*/("}", "").strip/*Unknown*/();
                    if (s3.isdigit/*Unknown*/())
                    {
                        if (portrait_index == -1)
                        {
                            portrait_index = (int)(s3);
                            previous_portrait = portrait_index;
                        }
                        else
                        {
                            portrait_index = (int)(s3);
                            // checks:
                            // 1. portrait in the same decimal range are not sequential
                            // 2. portrait '0' is identical to previous group's 2nd decimal
                            if (((!(portrait_index - previous_portrait == 1)) && (portrait_index % 10 != 0)))
                            {
                                ff.write/*Unknown*/("Error! Portrait number " + portrait_index.ToString() + " is not in sequence." + "\n");
                                Logger.Info("{0}", "Error! Portrait number " + portrait_index.ToString() + " is not in sequence." + "\n");
                                found_error_flag = 1;
                            }
                            else if ((portrait_index % 10 == 0 && (portrait_index - (portrait_index % 10)) == (previous_portrait - (previous_portrait % 10)) && previous_portrait != -1))
                            {
                                ff.write/*Unknown*/("Error! Portrait number " + portrait_index.ToString() + " is duplicate of previous group." + "\n");
                                Logger.Info("{0}", "Error! Portrait number " + portrait_index.ToString() + " is duplicate of previous group." + "\n");
                                found_error_flag = 1;
                            }

                            previous_portrait = portrait_index;
                        }

                    }
                    else
                    {
                        ff.write/*Unknown*/("Error! bracket with a non-number within! " + s3 + "\n");
                        Logger.Info("{0}", "Error! bracket with a non-number within! " + s3 + "\n");
                        found_error_flag = 1;
                    }

                }

            }

            if (!found_error_flag)
            {
                ff.write/*Unknown*/("Portraits file examined - no errors found.");
                Logger.Info("Portraits file examined - no errors found.");
            }

            ff.close/*Unknown*/();
            i_file.close/*Unknown*/();
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
        // ------------------------------------------------------------------------------------------
        // @	@	@	@	@	@	@	@	@	@
        // ------------------------------------------------------------------------------------------
        // @	@	@	@	@	@	@	@	@	@
        // ------------------------------------------------------------------------------------------

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
        public static void gimme(FIXME gversion = 5, FIXME give_earth = 0, FIXME give_air = 0, FIXME give_water = 0)
        {
            // Vanilla version has no wand of fireball (9th), so use gimme(1) for vanilla!
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if (pc.FindItemByProto(6105) == null && pc.ItemWornAt(EquipSlot.Cloak).GetNameId() != 3005) // Eye of Flame Cloak
                {
                    Utilities.create_item_in_inventory(6015, pc);
                }

                if (pc.FindItemByProto(6109) == null && give_air == 1)
                {
                    Utilities.create_item_in_inventory(6109, pc); // Air Temple robes
                }

                if (pc.FindItemByProto(6110) == null && give_earth == 1)
                {
                    Utilities.create_item_in_inventory(6110, pc); // Earth Temple robes
                }

                // if pc.item_find_by_proto(6111) == OBJ_HANDLE_NULL:
                // create_item_in_inventory( 6111, pc )
                if (pc.FindItemByProto(6112) == null && give_water == 1)
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

        public static FIXME s(FIXME prot)
        {
            var (x, y) = lta(SelectedPartyLeader.GetLocation());
            var a = spawn(prot, x, y + 1);
            return a;
        }
        public static FIXME det()
        {
            return (PartyLeader.GetMap(), loc());
        }
        public static void tp(FIXME map, FIXME X, FIXME Y)
        {
            FadeAndTeleport(0, 0, 0, map, X, Y);
            return;
        }
        public static FIXME loc()
        {
            return PartyLeader.GetLocation();
        }
        public static int lfa(int x, int y)
        {
            // initialize loc to be a LONG integer
            var loc = 0 + y;
            loc = (loc << 32) + x;
            return loc;
        }
        public static int lta(locXY loc)
        {
            var y = loc >> 32;
            var x = loc & 4294967295;
            return (x, y);
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
        public static GameObjectBody fnn(FIXME name, FIXME living_only = 1, FIXME multiple = 0)
        {
            var got_one = 0;
            foreach (var npc in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if (typeof(name) == typeof(1))
{
                    if ((npc.GetNameId() == name && multiple == 0))
                    {
                        return npc;
                    }
                    else if (name == -1 | multiple != 0)
                    {
                        if (living_only == 1 && npc.IsUnconscious())
                        {
                            continue;

                        }

                        if (npc.GetNameId() != name && multiple != 0)
                        {
                            continue;

                        }

                        got_one = 1;
                        var (xx, yy) = lta(npc.GetLocation());
                        Logger.Info("{0}", npc.ToString() + ",      name ID = " + npc.GetNameId().ToString() + ",    x = " + xx.ToString() + ",    y = " + yy.ToString());
                        npc.FloatMesFileLine("mes/test.mes", (int)(xx), TextFloaterColor.Red);
                        npc.FloatMesFileLine("mes/test.mes", (int)(yy), TextFloaterColor.Red);
                    }

                }
else if (typeof(name) == typeof("asdf"))
{
                    if ((npc.ToString().lower/*String*/().find/*Unknown*/(name.lower/*Unknown*/()) != -1))
                    {
                        return npc;
                    }

                }

            }

            if (got_one == 0 && name == -1)
            {
                fnn(living_only: 0);
            }

            return null;
        }
        // find PCs near

        public static GameObjectBody fpn(FIXME name, FIXME living_only = 1, FIXME multiple = 0)
        {
            // def fpn(name = -1, living_only = 1, multiple = 0):  ## find PCs near
            foreach (var pc in ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if (typeof(name) == typeof(1))
{
                    if ((pc.GetNameId() == name && multiple == 0))
                    {
                        return pc;
                    }
                    else if (name == -1 | multiple != 0)
                    {
                        if (living_only == 1 && pc.IsUnconscious())
                        {
                            continue;

                        }

                        if (pc.GetNameId() != name && multiple != 0)
                        {
                            continue;

                        }

                        var (xx, yy) = lta(pc.GetLocation());
                        Logger.Info("{0}", pc.ToString() + ",      name ID = " + pc.GetNameId().ToString() + ",    x = " + xx.ToString() + ",    y = " + yy.ToString());
                        pc.FloatMesFileLine("mes/test.mes", (int)(xx), TextFloaterColor.Red);
                        pc.FloatMesFileLine("mes/test.mes", (int)(yy), TextFloaterColor.Red);
                    }

                }
else if (typeof(name) == typeof("asdf"))
{
                    if ((pc.ToString().lower/*String*/().find/*Unknown*/(name.lower/*Unknown*/()) != -1))
                    {
                        return pc;
                    }

                }

            }

            return null;
        }
        // test adding PC via follower_add()

        public static void tpca()
        {
            // def tpca(): # test adding PC via follower_add()
            var a = fpn("va"); // vadania
            SelectedPartyLeader.AddFollower(a);
        }
        public static List<GameObjectBody> vlist(FIXME npc_name)
        {
            var moshe = ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_NPC);
            if (npc_name != -1)
            {
                var return_list = new List<GameObjectBody>();
                foreach (var obj in moshe)
                {
                    if (typeof(npc_name) == typeof(1))
{
                        if (npc_name == obj.GetNameId())
                        {
                            return_list.Add(obj);
                        }

                    }
else if (typeof(npc_name) == typeof(new[] { 1, 2 }))
{
                        if ((npc_name).Contains(obj.GetNameId()))
                        {
                            return_list.Add(obj);
                        }

                    }

                }

                return return_list;
            }
            else
            {
                return moshe;
            }

        }
        public static List<GameObjectBody> vlist2()
        {
            // looks for nearby containers
            var moshe = ObjList.ListVicinity(SelectedPartyLeader.GetLocation(), ObjectListFilter.OLC_CONTAINER);
            return moshe;
        }
        public static GameObjectBody spawn(FIXME prot, int x, int y)
        {
            var moshe = GameSystems.MapObject.CreateObject(prot, lfa(x, y));
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
