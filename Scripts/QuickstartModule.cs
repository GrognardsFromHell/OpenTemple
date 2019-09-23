
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

    public class QuickstartModule
    {
        // from itt import *

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
        public static void quickstart(FIXME simulated_game_state = 0, FIXME cheat_items = 1, FIXME autokill_on = 1)
        {
            gearup(simulated_game_state, 1);
            Logger.Info("{0}", simulated_game_state.ToString());
            if (simulated_game_state >= 0)
            {
                SetQuestState(18, QuestState.Completed); // Catch Furnok quest
                SetQuestState(100, QuestState.Accepted); // Fetch Giant's head
                SetGlobalFlag(21, true); // Enable Terjon
                MakeAreaKnown(2); // Moathouse
                MakeAreaKnown(5); // Emridy
            }

            if (simulated_game_state >= 1)
            {
                Logger.Info("Executing WB...");
                // Having just completed Welkwood Bog, going on Moathouse + Emridy
                StoryState = 1;
                MakeAreaKnown(7); // Welkwood Bog
                SetGlobalVar(970, 2); // Talked to Smyth about WB
                                      // game.global_flags[66] = 1 # Paid Elmo - do NOT set this flag, else he won't get his better gear
                SetGlobalFlag(67, true); // Have spoken to vignette's relevant figure
                SetGlobalFlag(605, true); // WB description box fired
                SetGlobalFlag(976, true); // Mathel dead
                SetQuestState(73, QuestState.Completed); // Welkwood Bog quest
                if (PartyAlignment == Alignment.NEUTRAL)
                {
                    SetQuestState(27, QuestState.Accepted); // Find Terjon's pendant
                }

                // Kill section #
                if ((ScriptDaemon.get_v("qs_welkwood") & ((Math.Pow(2, 11)) - 1)) != ((Math.Pow(2, 11)) - 1))
                {
                    ScriptDaemon.set_v("qs_welkwood", ScriptDaemon.get_v("qs_welkwood") | Math.Pow(2, 0));
                    if ((ScriptDaemon.get_v("qs_welkwood") & Math.Pow(2, 1)) == 0)
                    {
                        ScriptDaemon.cnk(14785); // Mathel
                        ScriptDaemon.set_v("qs_welkwood", ScriptDaemon.get_v("qs_welkwood") | Math.Pow(2, 1));
                    }

                    if ((ScriptDaemon.get_v("qs_welkwood") & Math.Pow(2, 2)) == 0)
                    {
                        ScriptDaemon.cnk(14183); // Goblin Leader
                        ScriptDaemon.set_v("qs_welkwood", ScriptDaemon.get_v("qs_welkwood") | Math.Pow(2, 2));
                    }

                    if ((ScriptDaemon.get_v("qs_welkwood") & Math.Pow(2, 3)) == 0)
                    {
                        ScriptDaemon.cnk(14641); // Kobold Sergeant
                        ScriptDaemon.set_v("qs_welkwood", ScriptDaemon.get_v("qs_welkwood") | Math.Pow(2, 3));
                    }

                    if ((ScriptDaemon.get_v("qs_welkwood") & Math.Pow(2, 4)) == 0)
                    {
                        ScriptDaemon.cnk(14631); // Gnoll
                        ScriptDaemon.set_v("qs_welkwood", ScriptDaemon.get_v("qs_welkwood") | Math.Pow(2, 4));
                    }

                    if ((ScriptDaemon.get_v("qs_welkwood") & Math.Pow(2, 5)) == 0)
                    {
                        ScriptDaemon.cnk(14081); // Skeleton Gnoll
                        ScriptDaemon.set_v("qs_welkwood", ScriptDaemon.get_v("qs_welkwood") | Math.Pow(2, 5));
                    }

                    if ((ScriptDaemon.get_v("qs_welkwood") & Math.Pow(2, 6)) == 0)
                    {
                        ScriptDaemon.cnk(14640, 10, 200); // Kobolds
                        ScriptDaemon.set_v("qs_welkwood", ScriptDaemon.get_v("qs_welkwood") | Math.Pow(2, 6));
                    }

                    if ((ScriptDaemon.get_v("qs_welkwood") & Math.Pow(2, 7)) == 0)
                    {
                        ScriptDaemon.cnk(14187, 18, 800); // Goblins
                        ScriptDaemon.set_v("qs_welkwood", ScriptDaemon.get_v("qs_welkwood") | Math.Pow(2, 7));
                    }

                    if ((ScriptDaemon.get_v("qs_welkwood") & Math.Pow(2, 8)) == 0)
                    {
                        ScriptDaemon.cnk(14183); // Goblin Leader
                        ScriptDaemon.set_v("qs_welkwood", ScriptDaemon.get_v("qs_welkwood") | Math.Pow(2, 8));
                    }

                    if ((ScriptDaemon.get_v("qs_welkwood") & Math.Pow(2, 9)) == 0)
                    {
                        ScriptDaemon.cnk(14640, 9, 1800); // Kobolds
                        ScriptDaemon.set_v("qs_welkwood", ScriptDaemon.get_v("qs_welkwood") | Math.Pow(2, 9));
                    }

                    if ((ScriptDaemon.get_v("qs_welkwood") & Math.Pow(2, 10)) == 0)
                    {
                        ScriptDaemon.cnk(14641); // Kobold Sergeant
                        ScriptDaemon.set_v("qs_welkwood", ScriptDaemon.get_v("qs_welkwood") | Math.Pow(2, 10));
                    }

                    Logger.Info("WB executed!");
                }

            }

            // for pc in game.party[0].group_list():
            // if pc.stat_level_get(stat_experience) <= 820:
            // pc.stat_base_set(stat_experience, 820)
            if (simulated_game_state >= 2)
            {
                if (autokill_on == 1)
                {
                    ScriptDaemon.set_f("qs_autokill_moathouse", 1);
                }

                // Having just completed Moathouse + Emridy + Welkwood Bog
                // for pc in game.party[0].group_list():
                // if pc.stat_level_get(stat_experience) <= 6000:
                // pc.stat_base_set(stat_experience, 6000)
                StoryState = 2;
                MakeAreaKnown(8); // Moathouse Cave Exit
                MakeAreaKnown(10); // Deklo
                Logger.Info("Executing Moathouse + Emridy Meadows...");
            }

            if (simulated_game_state >= 3)
            {
                // Having Finished Nulb + HB
                // I.E. auto-kill Nulb and HB
                // preparing for "legitimate" AoH + Revenge Encounter + Moathouse Respawn ( + Temple )
                // for pc in game.party[0].group_list():
                // if pc.stat_level_get(stat_experience) <= 16000:
                // pc.stat_base_set(stat_experience, 16000)
                Logger.Info("Executing Nulb, HB");
                StoryState = 3;
                MakeAreaKnown(3); // Nulb
                MakeAreaKnown(6); // Imeryds
                MakeAreaKnown(9); // HB
                SetQuestState(35, QuestState.Accepted); // Grud's story
                SetQuestState(41, QuestState.Accepted); // Preston's tooth ache
                SetQuestState(42, QuestState.Accepted); // Assassinate Lodriss
                SetQuestState(59, QuestState.Accepted); // Free Serena
                SetQuestState(60, QuestState.Accepted); // Mona's Orb
                SetQuestState(63, QuestState.Accepted); // Bribery for justice
                if (autokill_on == 1)
                {
                    ScriptDaemon.set_f("qs_autokill_nulb", 1);
                }

            }

            if (simulated_game_state >= 3.5f)
            {
                SetQuestState(65, QuestState.Accepted); // Hero's Prize Quest
                SetGlobalVar(972, 2); // Have talked to Kent about Witch
                ScriptDaemon.set_f("qs_arena_of_heroes_enable");
            }

            if (simulated_game_state >= 4)
            {
                // Autokill Temple, AoH, Revenge Encounter, MR
                Logger.Info("Executing Temple, AoH, Moathouse Respawn, Revenge Encounter");
                if (autokill_on == 1)
                {
                    ScriptDaemon.set_f("qs_autokill_temple");
                }

                StoryState = 4;
                MakeAreaKnown(4); // Temple
                SetQuestState(65, QuestState.Accepted); // Hero's Prize Quest
                SetGlobalFlag(944, true);
            }

            if (simulated_game_state >= 5)
            {
                // Autokill Greater Temple, Verbobonc (minus slavers)
                Logger.Info("Executing Greater Temple, Verbobonc");
                if (autokill_on == 1)
                {
                    ScriptDaemon.set_f("qs_autokill_greater_temple");
                }

                StoryState = 5;
                MakeAreaKnown(11); // Temple Burnt Farmhouse
                MakeAreaKnown(14); // Verbobonc
            }

            if (simulated_game_state >= 6)
            {
                Logger.Info("Executing Nodes, WotGS");
                if (autokill_on == 1)
                {
                    ScriptDaemon.set_f("qs_autokill_nodes");
                }

                StoryState = 6;
            }

        }
        public static void gearup(FIXME o_ride, FIXME cheat_items = 1)
        {
            var s_rogue_items = new List<GameObjectBody>();
            var s_tank_weapons_2 = new List<GameObjectBody>();
            var s_tank_armor_2 = new List<GameObjectBody>();
            GameObjectBody figh_pc = null;
            GameObjectBody barb_pc = null;
            GameObjectBody bard_pc = null;
            GameObjectBody rogu_pc = null;
            GameObjectBody cler_pc = null;
            GameObjectBody drui_pc = null;
            GameObjectBody monk_pc = null;
            GameObjectBody sorc_pc = null;
            GameObjectBody wiza_pc = null;
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if (which_class(pc) == "fighter")
                {
                    figh_pc = pc;
                }

                if (which_class(pc) == "barbarian")
                {
                    barb_pc = pc;
                }

                if (which_class(pc) == "bard")
                {
                    bard_pc = pc;
                }

                if (which_class(pc) == "rogue")
                {
                    rogu_pc = pc;
                }

                if (which_class(pc) == "cleric")
                {
                    cler_pc = pc;
                }

                if (which_class(pc) == "druid")
                {
                    drui_pc = pc;
                }

                if (which_class(pc) == "monk")
                {
                    monk_pc = pc;
                }

                if (which_class(pc) == "sorcerer")
                {
                    sorc_pc = pc;
                }

                if (which_class(pc) == "wizard")
                {
                    wiza_pc = pc;
                }

                var brown_farmer_garb = pc.FindItemByProto(6142);
                if (brown_farmer_garb != null)
                {
                    brown_farmer_garb.Destroy();
                }

            }

            if (StoryState == 2)
            {
                var dummy = 1;
            }

            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if (StoryState <= 1 || pc.GetMap() == 5107 || o_ride == 0)
                {
                    if (which_class(pc) == "fighter")
                    {
                        if (pc.ItemWornAt(EquipSlot.WeaponPrimary) == null)
                        {
                            ScriptDaemon.giv(pc, 4010); // Greatsword
                        }

                        if (pc.ItemWornAt(EquipSlot.Armor) == null)
                        {
                            ScriptDaemon.giv(pc, 6093); // Chain Shirt
                        }

                    }

                    if (which_class(pc) == "barbarian")
                    {
                        if (pc.ItemWornAt(EquipSlot.WeaponPrimary) == null)
                        {
                            ScriptDaemon.giv(pc, 4064); // Greataxe
                        }

                        if (pc.ItemWornAt(EquipSlot.Armor) == null)
                        {
                            ScriptDaemon.giv(pc, 6055); // Barbarian Armor
                        }

                    }

                    if (which_class(pc) == "cleric")
                    {
                        if (pc.ItemWornAt(EquipSlot.WeaponPrimary) == null)
                        {
                            ScriptDaemon.giv(pc, 4070); // Morningstar
                        }

                        ScriptDaemon.giv(pc, 6070); // Large Wooden Shield
                        if (pc.ItemWornAt(EquipSlot.Armor) == null)
                        {
                            ScriptDaemon.giv(pc, 6153); // Fine Scalemail
                        }

                        if (cheat_items == 1)
                        {
                            ScriptDaemon.giv(pc, 12231); // Wand of Holy Smite
                            ScriptDaemon.giv(pc, 12178); // Wand of Flame Strike
                        }

                    }

                    if (which_class(pc) == "druid")
                    {
                        if (pc.ItemWornAt(EquipSlot.WeaponPrimary) == null)
                        {
                            ScriptDaemon.giv(pc, 4045); // Scimitar
                        }

                        ScriptDaemon.giv(pc, 6070); // Large Wooden Shield
                        if (pc.ItemWornAt(EquipSlot.Armor) == null)
                        {
                            ScriptDaemon.giv(pc, 6009); // Druid Hide
                        }

                        if (cheat_items == 1)
                        {
                            ScriptDaemon.giv(pc, 12178); // Wand of Flame Strike
                        }

                    }

                    if (which_class(pc) == "monk")
                    {
                        ScriptDaemon.giv(pc, 4243); // Quarterstaff
                    }

                    if (which_class(pc) == "bard")
                    {
                        if (pc.ItemWornAt(EquipSlot.WeaponPrimary) == null)
                        {
                            ScriptDaemon.giv(pc, 4060); // Dagger
                        }

                        if (pc.ItemWornAt(EquipSlot.Armor) == null)
                        {
                            ScriptDaemon.giv(pc, 6013); // Brown Leather Armor
                        }

                        ScriptDaemon.giv(pc, 12564); // Mandolin
                        ScriptDaemon.giv(pc, 12677, 1); // Spyglass
                    }

                    if (which_class(pc) == "sorcerer")
                    {
                        if (pc.ItemWornAt(EquipSlot.WeaponPrimary) == null)
                        {
                            ScriptDaemon.giv(pc, 4243); // Quarterstaff
                        }

                        if (cheat_items == 1)
                        {
                            ScriptDaemon.giv(pc, 12620); // Wand of Fireball (10th)
                            ScriptDaemon.giv(pc, 12262); // Wand of Knock
                        }

                    }

                    if (which_class(pc) == "rogue")
                    {
                        if (pc.ItemWornAt(EquipSlot.WeaponPrimary) == null)
                        {
                            ScriptDaemon.giv(pc, 4060); // Dagger
                        }

                        ScriptDaemon.giv(pc, 6031); // Eyeglasses
                        if (pc.ItemWornAt(EquipSlot.Armor) == null)
                        {
                            ScriptDaemon.giv(pc, 6042); // Black Leather Armor
                        }

                        ScriptDaemon.giv(pc, 12012); // Thieves Tools
                        ScriptDaemon.giv(pc, 12767); // Lockslip Grease
                        ScriptDaemon.giv(pc, 12677, 1); // Spyglass
                    }

                    if (which_class(pc) == "wizard")
                    {
                        if (pc.ItemWornAt(EquipSlot.Armor) == null)
                        {
                            ScriptDaemon.giv(pc, 6151); // Red Mystic Garb
                        }

                        if (pc.ItemWornAt(EquipSlot.WeaponPrimary) == null)
                        {
                            ScriptDaemon.giv(pc, 4243); // Quarterstaff
                        }

                        ScriptDaemon.giv(pc, 9229); // Grease Scroll
                        ScriptDaemon.giv(pc, 12848); // Scholar's kit
                        if (cheat_items == 1)
                        {
                            ScriptDaemon.giv(pc, 12620); // Wand of Fireball (10th)
                            ScriptDaemon.giv(pc, 12262); // Wand of Knock
                        }

                    }

                }

                if (StoryState == 2 || o_ride == 2)
                {
                    // End of Moathouse - Tackling Nulb and HB
                    // Scrolls: Stinking Cloud, Knock, Ray of Enfeeb, Animate Dead, Magic Missile, Color Spray, Obscuring Mist, Cause Fear, Sleep
                    // Wooden Elvish Chain 6073
                    // Shield + 1
                    // Silver Banded Mail 6120
                    // Lareth's Breastplate 6097
                    // Lareth's Staff 4120
                    // Lareth's Plate Boots 6098
                    // Lareth's Ring 6099
                    // Fungus Figurine 12024
                    // Shortsword +1 4126
                    // MW Xbow 4177
                    // Cloak of Elvenkind x2 6058
                    // ~10k GP before spending
                    // Shopping:
                    // MW Scimitar 400GP - 4048
                    // MW Greataxe 400GP  - 4065
                    // MW Greatsword 400GP  - 4012
                    // Prot from arrows 180GP - 9367
                    // Resist Energy 180GP - 9400
                    // Web 180 GP- 9531
                    // Summon Mon 1 30GP - 9467
                    // if pc.money_get() < 10000 * 100:
                    // pc.money_adj(10000 * 100 - pc.money_get())
                    if (which_class(pc) == "fighter")
                    {
                        if (pc.ItemWornAt(EquipSlot.WeaponPrimary) == null)
                        {
                            ScriptDaemon.giv(pc, 4010); // Greatsword
                        }

                    }

                    if (which_class(pc) == "barbarian")
                    {
                        if (pc.ItemWornAt(EquipSlot.WeaponPrimary) == null)
                        {
                            ScriptDaemon.giv(pc, 4064); // Greataxe
                        }

                        if (pc.ItemWornAt(EquipSlot.Armor) == null)
                        {
                            ScriptDaemon.giv(pc, 6055); // Barbarian Armor
                        }

                    }

                    if (which_class(pc) == "cleric")
                    {
                        if (pc.ItemWornAt(EquipSlot.WeaponPrimary) == null)
                        {
                            ScriptDaemon.giv(pc, 4070); // Morningstar
                        }

                        if (ScriptDaemon.giv(pc, 6073, 1) == 0)
                        {
                            ScriptDaemon.giv(pc, 6070); // Large Wooden Shield
                        }

                        if (pc.ItemWornAt(EquipSlot.Armor) == null)
                        {
                            ScriptDaemon.giv(pc, 6153); // Fine Scalemail
                        }

                    }

                    if (which_class(pc) == "druid")
                    {
                        if (pc.ItemWornAt(EquipSlot.WeaponPrimary) == null)
                        {
                            ScriptDaemon.giv(pc, 4045); // Scimitar
                        }

                        if (ScriptDaemon.giv(pc, 6073, 1) == 0)
                        {
                            ScriptDaemon.giv(pc, 6070); // Large Wooden Shield
                        }

                        if (pc.ItemWornAt(EquipSlot.Armor) == null)
                        {
                            ScriptDaemon.giv(pc, 6009); // Druid Hide
                        }

                    }

                    if (which_class(pc) == "monk")
                    {
                        if (pc.ItemWornAt(EquipSlot.WeaponPrimary) == null)
                        {
                            ScriptDaemon.giv(pc, 4243); // Quarterstaff
                        }

                    }

                    if (which_class(pc) == "bard")
                    {
                        if (pc.ItemWornAt(EquipSlot.WeaponPrimary) == null)
                        {
                            ScriptDaemon.giv(pc, 4060); // Dagger
                        }

                        if (pc.ItemWornAt(EquipSlot.Armor) == null)
                        {
                            ScriptDaemon.giv(pc, 6013); // Brown Leather Armor
                        }

                        ScriptDaemon.giv(pc, 12564); // Mandolin
                        ScriptDaemon.giv(pc, 6031, 1); // Eyeglasses
                        ScriptDaemon.giv(pc, 12675, 1); // Merchant's Scale
                    }

                    if (which_class(pc) == "sorcerer")
                    {
                        if (pc.ItemWornAt(EquipSlot.WeaponPrimary) == null)
                        {
                            ScriptDaemon.giv(pc, 4243); // Quarterstaff
                        }

                    }

                    if (which_class(pc) == "rogue")
                    {
                        if (pc.ItemWornAt(EquipSlot.WeaponPrimary) == null)
                        {
                            ScriptDaemon.giv(pc, 4060); // Dagger
                        }

                        ScriptDaemon.giv(pc, 6031, 1); // Eyeglasses
                        ScriptDaemon.giv(pc, 12675, 1); // Merchant's Scale
                        if (pc.ItemWornAt(EquipSlot.Armor) == null)
                        {
                            ScriptDaemon.giv(pc, 6042); // Black Leather Armor
                        }

                        ScriptDaemon.giv(pc, 12012); // Thieves Tools
                        ScriptDaemon.giv(pc, 12767); // Lockslip Grease
                    }

                    if (which_class(pc) == "wizard")
                    {
                        if (pc.ItemWornAt(EquipSlot.WeaponPrimary) == null)
                        {
                            ScriptDaemon.giv(pc, 4243); // Quarterstaff
                        }

                        ScriptDaemon.giv(pc, 9229); // Grease Scroll
                        ScriptDaemon.giv(pc, 12848); // Scholar's kit
                        ScriptDaemon.giv(pc, 6031, 1); // Eyeglasses
                        ScriptDaemon.giv(pc, 12675, 1); // Merchant's Scale
                    }

                }

                pc.IdentifyAll();
                pc.WieldBestInAllSlots();
            }

            ScriptDaemon.giv(pc, 6031, 1); // Eyeglasses
            ScriptDaemon.giv(pc, 12675, 1); // Merchant's Scale
            ScriptDaemon.giv(pc, 12012, 1); // Thieves Tools
            ScriptDaemon.giv(pc, 12767, 1); // Lockslip Grease
            return;
        }

    }
}
