
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

    public class Q
    {
        public static void qs()
        {
            Batch.partyhpset(800);
            Batch.partyabset(1, 40);
            Batch.partyabset(2, 40);
            Batch.partyabset(3, 40);
            Batch.partyabset(4, 40);
            Batch.partyabset(5, 40);
            Batch.partyabset(6, 40);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if ((pc.GetStat(Stat.level_cleric)) > 0) // CLERIC
                {
                    Utilities.create_item_in_inventory(6396, pc); // armor
                    Utilities.create_item_in_inventory(6020, pc); // boots
                    Utilities.create_item_in_inventory(6021, pc); // gloves
                    Utilities.create_item_in_inventory(6018, pc); // helm
                    Utilities.create_item_in_inventory(6234, pc); // cloak - white
                    Utilities.create_item_in_inventory(6050, pc); // shield
                    Utilities.create_item_in_inventory(4109, pc); // weapon
                    Utilities.create_item_in_inventory(12625, pc); // ring
                    Utilities.create_item_in_inventory(6242, pc); // amulet health
                    Utilities.create_item_in_inventory(12089, pc); // wand of cure critical
                    Utilities.create_item_in_inventory(12089, pc); // wand of cure critical
                    Utilities.create_item_in_inventory(12089, pc); // wand of cure critical
                    Utilities.create_item_in_inventory(12379, pc); // wand of raise dead
                    pc.WieldBestInAllSlots();
                    pc.AdjustMoney(2000000);
                }

                if ((pc.GetStat(Stat.level_fighter)) > 0) // FIGHTER
                {
                    Utilities.create_item_in_inventory(6104, pc); // armor
                    Utilities.create_item_in_inventory(6040, pc); // boots
                    Utilities.create_item_in_inventory(6041, pc); // gloves
                    Utilities.create_item_in_inventory(6033, pc); // helm
                    Utilities.create_item_in_inventory(6338, pc); // cloak - blue
                    Utilities.create_item_in_inventory(6050, pc); // shield
                    Utilities.create_item_in_inventory(4081, pc); // weapon
                    Utilities.create_item_in_inventory(12625, pc); // ring
                    Utilities.create_item_in_inventory(6242, pc); // amulet health
                    pc.WieldBestInAllSlots();
                    pc.AdjustMoney(2000000);
                }

                if ((pc.GetStat(Stat.level_paladin)) > 0) // PALADIN
                {
                    Utilities.create_item_in_inventory(6104, pc); // armor
                    Utilities.create_item_in_inventory(6040, pc); // boots
                    Utilities.create_item_in_inventory(6041, pc); // gloves
                    Utilities.create_item_in_inventory(6035, pc); // helm
                    Utilities.create_item_in_inventory(6428, pc); // cloak - violet
                    Utilities.create_item_in_inventory(6050, pc); // shield
                    Utilities.create_item_in_inventory(4081, pc); // weapon
                    Utilities.create_item_in_inventory(12625, pc); // ring
                    Utilities.create_item_in_inventory(6242, pc); // amulet health
                    pc.WieldBestInAllSlots();
                    pc.AdjustMoney(2000000);
                }

                if ((pc.GetStat(Stat.level_barbarian)) > 0) // BARBARIAN
                {
                    Utilities.create_item_in_inventory(6298, pc); // armor
                    Utilities.create_item_in_inventory(6011, pc); // boots
                    Utilities.create_item_in_inventory(6012, pc); // gloves
                    Utilities.create_item_in_inventory(6026, pc); // helm
                    Utilities.create_item_in_inventory(6124, pc); // cloak - red
                    Utilities.create_item_in_inventory(6059, pc); // shield
                    Utilities.create_item_in_inventory(4254, pc); // weapon
                    Utilities.create_item_in_inventory(12625, pc); // ring
                    Utilities.create_item_in_inventory(6242, pc); // amulet health
                    pc.WieldBestInAllSlots();
                    pc.AdjustMoney(2000000);
                }

                if ((pc.GetStat(Stat.level_druid)) > 0) // DRUID
                {
                    Utilities.create_item_in_inventory(6306, pc); // armor
                    Utilities.create_item_in_inventory(6011, pc); // boots
                    Utilities.create_item_in_inventory(6012, pc); // gloves
                    Utilities.create_item_in_inventory(6217, pc); // helm
                    Utilities.create_item_in_inventory(6421, pc); // cloak - fur
                    Utilities.create_item_in_inventory(6064, pc); // shield
                    Utilities.create_item_in_inventory(4047, pc); // weapon
                    Utilities.create_item_in_inventory(12625, pc); // ring
                    Utilities.create_item_in_inventory(6242, pc); // amulet health
                    Utilities.create_item_in_inventory(12089, pc); // wand of cure critical
                    Utilities.create_item_in_inventory(12089, pc); // wand of cure critical
                    Utilities.create_item_in_inventory(12089, pc); // wand of cure critical
                    Utilities.create_item_in_inventory(12379, pc); // wand of raise dead
                    pc.WieldBestInAllSlots();
                    pc.AdjustMoney(2000000);
                }

                if ((pc.GetStat(Stat.level_ranger)) > 0) // RANGER
                {
                    Utilities.create_item_in_inventory(6091, pc); // armor
                    Utilities.create_item_in_inventory(6011, pc); // boots
                    Utilities.create_item_in_inventory(6012, pc); // gloves
                                                                  // helm
                    Utilities.create_item_in_inventory(6269, pc); // cloak - green
                    Utilities.create_item_in_inventory(6059, pc); // shield
                    Utilities.create_item_in_inventory(4191, pc); // weapon
                    Utilities.create_item_in_inventory(12625, pc); // ring
                    Utilities.create_item_in_inventory(6242, pc); // amulet health
                    pc.WieldBestInAllSlots();
                    pc.AdjustMoney(2000000);
                }

                if ((pc.GetStat(Stat.level_bard)) > 0) // BARD
                {
                    Utilities.create_item_in_inventory(6297, pc); // armor
                    Utilities.create_item_in_inventory(6023, pc); // boots
                    Utilities.create_item_in_inventory(6024, pc); // gloves
                    Utilities.create_item_in_inventory(6335, pc); // helm
                    Utilities.create_item_in_inventory(6427, pc); // cloak - orange
                    Utilities.create_item_in_inventory(6050, pc); // shield
                    Utilities.create_item_in_inventory(4126, pc); // weapon
                    Utilities.create_item_in_inventory(12625, pc); // ring
                    Utilities.create_item_in_inventory(6242, pc); // amulet health
                    Utilities.create_item_in_inventory(12587, pc); // mw mandolin
                    pc.WieldBestInAllSlots();
                    pc.AdjustMoney(2000000);
                }

                if ((pc.GetStat(Stat.level_rogue)) > 0) // ROGUE
                {
                    Utilities.create_item_in_inventory(6299, pc); // armor
                    Utilities.create_item_in_inventory(6045, pc); // boots
                    Utilities.create_item_in_inventory(6046, pc); // gloves
                                                                  // helm
                    Utilities.create_item_in_inventory(6233, pc); // cloak - black
                    Utilities.create_item_in_inventory(6059, pc); // shield
                    Utilities.create_item_in_inventory(4126, pc); // weapon
                    Utilities.create_item_in_inventory(12625, pc); // ring
                    Utilities.create_item_in_inventory(6242, pc); // amulet health
                    Utilities.create_item_in_inventory(12013, pc); // mw thieves tools
                    pc.WieldBestInAllSlots();
                    pc.AdjustMoney(2000000);
                }

                if ((pc.GetStat(Stat.level_monk)) > 0) // MONK
                {
                    Utilities.create_item_in_inventory(6115, pc); // armor
                    Utilities.create_item_in_inventory(6023, pc); // boots
                    Utilities.create_item_in_inventory(6024, pc); // gloves
                    Utilities.create_item_in_inventory(6025, pc); // helm
                    Utilities.create_item_in_inventory(6205, pc); // robes - brown
                                                                  // shield
                    Utilities.create_item_in_inventory(4427, pc); // weapon
                    Utilities.create_item_in_inventory(12625, pc); // ring
                    Utilities.create_item_in_inventory(6242, pc); // amulet health
                    pc.WieldBestInAllSlots();
                    pc.AdjustMoney(2000000);
                }

                if ((pc.GetStat(Stat.level_sorcerer)) > 0) // SORCERER
                {
                    if ((pc.GetGender() == Gender.Female)) // FEMALE
                    {
                        Utilities.create_item_in_inventory(6115, pc); // armor
                        Utilities.create_item_in_inventory(6045, pc); // boots
                        Utilities.create_item_in_inventory(6046, pc); // gloves
                                                                      // helm
                        Utilities.create_item_in_inventory(6637, pc); // garb - red
                                                                      // shield
                        Utilities.create_item_in_inventory(4242, pc); // weapon
                        Utilities.create_item_in_inventory(12625, pc); // ring
                        Utilities.create_item_in_inventory(6242, pc); // amulet health
                        Utilities.create_item_in_inventory(12262, pc); // wand of knock
                        Utilities.create_item_in_inventory(12238, pc); // wand of identify
                        Utilities.create_item_in_inventory(12581, pc); // wand of fireball
                        Utilities.create_item_in_inventory(12581, pc); // wand of fireball
                        Utilities.create_item_in_inventory(12581, pc); // wand of fireball
                        pc.WieldBestInAllSlots();
                        pc.AdjustMoney(2000000);
                    }
                    else if ((pc.GetGender() == Gender.Male)) // MALE
                    {
                        Utilities.create_item_in_inventory(6115, pc); // armor
                        Utilities.create_item_in_inventory(6045, pc); // boots
                        Utilities.create_item_in_inventory(6046, pc); // gloves
                                                                      // helm
                        Utilities.create_item_in_inventory(6637, pc); // garb - red
                                                                      // shield
                        Utilities.create_item_in_inventory(4242, pc); // weapon
                        Utilities.create_item_in_inventory(12625, pc); // ring
                        Utilities.create_item_in_inventory(6242, pc); // amulet health
                        Utilities.create_item_in_inventory(12262, pc); // wand of knock
                        Utilities.create_item_in_inventory(12238, pc); // wand of identify
                        Utilities.create_item_in_inventory(12581, pc); // wand of fireball
                        Utilities.create_item_in_inventory(12581, pc); // wand of fireball
                        Utilities.create_item_in_inventory(12581, pc); // wand of fireball
                        pc.WieldBestInAllSlots();
                        pc.AdjustMoney(2000000);
                    }

                }

                if ((pc.GetStat(Stat.level_wizard)) > 0) // WIZARD
                {
                    if ((pc.GetGender() == Gender.Female)) // FEMALE
                    {
                        Utilities.create_item_in_inventory(6115, pc); // armor
                        Utilities.create_item_in_inventory(6045, pc); // boots
                        Utilities.create_item_in_inventory(6046, pc); // gloves
                                                                      // helm
                        Utilities.create_item_in_inventory(6638, pc); // garb - blue
                                                                      // shield
                        Utilities.create_item_in_inventory(4243, pc); // weapon
                        Utilities.create_item_in_inventory(12625, pc); // ring
                        Utilities.create_item_in_inventory(6242, pc); // amulet health
                        Utilities.create_item_in_inventory(12262, pc); // wand of knock
                        Utilities.create_item_in_inventory(12238, pc); // wand of identify
                        Utilities.create_item_in_inventory(12581, pc); // wand of fireball
                        Utilities.create_item_in_inventory(12581, pc); // wand of fireball
                        Utilities.create_item_in_inventory(12581, pc); // wand of fireball
                        pc.WieldBestInAllSlots();
                        pc.AdjustMoney(2000000);
                    }
                    else if ((pc.GetGender() == Gender.Male)) // MALE
                    {
                        Utilities.create_item_in_inventory(6115, pc); // armor
                        Utilities.create_item_in_inventory(6045, pc); // boots
                        Utilities.create_item_in_inventory(6046, pc); // gloves
                        Utilities.create_item_in_inventory(6630, pc); // wizard hat - blue
                        Utilities.create_item_in_inventory(6627, pc); // robes - blue
                                                                      // shield
                        Utilities.create_item_in_inventory(4243, pc); // weapon
                        Utilities.create_item_in_inventory(12625, pc); // ring
                        Utilities.create_item_in_inventory(6242, pc); // amulet health
                        Utilities.create_item_in_inventory(12262, pc); // wand of knock
                        Utilities.create_item_in_inventory(12238, pc); // wand of identify
                        Utilities.create_item_in_inventory(12581, pc); // wand of fireball
                        Utilities.create_item_in_inventory(12581, pc); // wand of fireball
                        Utilities.create_item_in_inventory(12581, pc); // wand of fireball
                        pc.WieldBestInAllSlots();
                        pc.AdjustMoney(2000000);
                    }

                }

            }

            FadeAndTeleport(0, 0, 0, 5001, 622, 418);
            MakeAreaKnown(2);
            MakeAreaKnown(3);
            MakeAreaKnown(4);
            MakeAreaKnown(5);
            MakeAreaKnown(6);
            MakeAreaKnown(7);
            MakeAreaKnown(8);
            MakeAreaKnown(9);
            MakeAreaKnown(10);
            MakeAreaKnown(11);
            MakeAreaKnown(12);
            MakeAreaKnown(14);
            MakeAreaKnown(15);
            MakeAreaKnown(16);
            return;
        }

    }
}
