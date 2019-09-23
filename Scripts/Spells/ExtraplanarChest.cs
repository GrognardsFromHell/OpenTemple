
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

namespace Scripts.Spells
{
    [SpellScript(761)]
    public class ExtraplanarChest : BaseSpellScript
    {
        // Writen By Cerulean the Blue
        // Modified by Sitra Achara 04-2011
        // Miniature Chest internal flags:
        // obj_f_item_pad_i_2 - miniature chest ID
        // obj_f_item_pad_i_3 - indicates whether the chest can be summoned ("chest is in the Ethereal Plane (1) or in the Prime Plane (0) ")
        // obj_f_item_pad_i_4 - Map # where the chest was summoned

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info(" Extraplanar Chest OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info(" Extraplanar Chest OnSpellEffect");
            spell.duration = 1;
            var bgfilename = "modules\\ToEE\\Bag_of_Holding.mes";
            var proto = 1100;
            var mini = spell.caster.FindItemByName(12105);
            if (mini == null) // Caster hasn't used spell before.  Create miniature chest and subtract 5050 gold from caster
            {
                if (spell.caster.GetMoney() >= 505000)
                {
                    mini = Utilities.create_item_in_inventory(12105, spell.caster);
                    set_flag(mini, 0); // sets flag for chest on the Prime Material Plane
                    mini.SetItemFlag(ItemFlag.IDENTIFIED); // Makes the mini identified.
                    spell.caster.AdjustMoney(-505000);
                    var chest = GameSystems.MapObject.CreateObject(proto, spell.caster.GetLocation());
                    Utilities.create_item_in_inventory(11300, chest); // Create Leomund's Secret Chest note in new chest
                    AttachParticles("Orb-Summon-Balor", chest);
                    var bagnum = Co8.boh_newbag(); // boh-newbag() is in Co8.py
                    Set_ID(mini, chest, bagnum);
                }
                else
                {
                    AttachParticles("Fizzle", spell.caster);
                }

            }
            else if ((Get_ID_Mini(mini) == 0)) // Miniature found but never used before
            {
                set_flag(mini, 0); // sets flag for chest on the Prime Material Plane
                var bagnum = Co8.boh_newbag(); // boh-newbag() is in Co8.py
                var chest = GameSystems.MapObject.CreateObject(proto, spell.caster.GetLocation());
                Utilities.create_item_in_inventory(11300, chest); // Create Leomund's Secret Chest note in new chest
                AttachParticles("Orb-Summon-Balor", chest);
                Set_ID(mini, chest, bagnum);
                set_mini_map(mini, chest); // record map where the chest was summoned - for forgetful players
            }
            else
            {
                // Mini found and has been used before.
                var chest_here = 0; // flag for whether or not the right chest is in the casters vicinity
                foreach (var chest in ObjList.ListVicinity(spell.caster.GetLocation(), ObjectListFilter.OLC_CONTAINER))
                {
                    // Find correct chest for that mini
                    if ((chest.GetNameId() == 1100 && Compare_ID(mini, chest)))
                    {
                        chest_here = 1;
                        var (cxx, cyy) = chest.GetLocation();
                        AttachParticles("Orb-Summon-Balor", chest);
                        var allBagDict = Co8.readMes(bgfilename); // readMes is in Co8.py.
                        var bagnum = Co8.boh_newbag(); // boh-newbag() is in Co8.py
                        Set_ID_Mini(mini, bagnum);
                        var contents = GetContents(chest);
                        allBagDict[bagnum] = contents;
                        Co8.writeMes(bgfilename, allBagDict); // writeMes is in Co8.py
                        set_flag(mini, 1); // Sets fkag flag for chest on Ethereal Plane
                                           // Time event added for chest destruction to allow time for game particles to fire
                        Co8.Timed_Destroy(chest, 500); // 500 = 1/2 second
                    }

                }

                if ((!(chest_here) && get_flag(mini))) // Chest not on this plane:  create chest and fill it.
                {
                    var chest = GameSystems.MapObject.CreateObject(proto, spell.caster.GetLocation());
                    var bagnum = Get_ID_Mini(mini);
                    Set_ID(mini, chest, bagnum);
                    set_flag(mini, 0); // sets flag for chest on the Prime Material Plane
                    set_mini_map(mini, chest); // record map where the chest was summoned - for forgetful players
                    AttachParticles("Orb-Summon-Balor", chest);
                    var contents = Co8.boh_getContents(bagnum); // boh_getContents is in Co8.py
                    Create_Contents(contents, chest);
                }
                else if ((!(chest_here) && !get_flag(mini)))
                {
                    var miniature_chest_map_number = get_mini_map(mini); // retrieve map where the chest was summoned - for forgetful players
                    spell.caster.FloatMesFileLine("mes/spell.mes", 16015, TextFloaterColor.Red); // "Chest left at:"
                    spell.caster.FloatMesFileLine("mes/map_names.mes", miniature_chest_map_number, TextFloaterColor.Red);
                    spell.caster.FloatMesFileLine("mes/map_numbers.mes", miniature_chest_map_number, TextFloaterColor.Red);
                    // failsaife:
                    // if you are on the same map, and your X,Y coordinates are close enough to where the chest was supposed to be, yet no chest was found, then it's probably a bug - reset the miniature to "Ethereal Plane"
                    // likewise, if for some reason the chest has no recorded X,Y at all, allow respawning it (mainly catering to r0gershrubber here :) )
                    var (pxx, pyy) = spell.caster.GetLocation();
                    var (cxx, cyy) = get_mini_xy(mini);
                    if (((Math.Pow((pxx - cxx), 2)) + (Math.Pow((pyy - cyy), 2)) < 81 || cxx == 0) && SelectedPartyLeader.GetMap() == miniature_chest_map_number)
                    {
                        set_flag(mini, 1);
                        // error try again message
                        spell.caster.FloatMesFileLine("mes/spell.mes", 16017, TextFloaterColor.Red); // "Failsafe activated!"
                        spell.caster.FloatMesFileLine("mes/spell.mes", 16018, TextFloaterColor.Red); // "Try again."
                    }
                    else if ((range(5070, 5079)).Contains(miniature_chest_map_number))
                    {
                        // lastly, if you lost it in the wilds, here's your second chance (exploitable, but it's easy enough to cheat in this game anyway)
                        spell.caster.FloatMesFileLine("mes/spell.mes", 16016, TextFloaterColor.Red); // "LOST IN WILDERNESS!"
                        if (!(SelectedPartyLeader.GetMap() == miniature_chest_map_number))
                        {
                            set_flag(mini, 1);
                            spell.caster.FloatMesFileLine("mes/spell.mes", 16017, TextFloaterColor.Red); // "Failsafe activated!"
                            spell.caster.FloatMesFileLine("mes/spell.mes", 16018, TextFloaterColor.Red); // "Try again."
                        }

                    }

                    AttachParticles("Fizzle", spell.caster);
                }
                else
                {
                    AttachParticles("Fizzle", spell.caster);
                }

            }

            Co8.End_Spell(spell);
            spell.EndSpell(true); // fix - adding the endDespiteTargetList flag to force the spell_end and prevent the spell trigger from going on indefinitely
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info(" Extraplanar Chest OnBeginRound");
            return;
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info(" Extraplanar Chest OnEndSpellCast");
        }
        public static List<GameObjectBody> GetContents(FIXME chest)
        {
            // Gets contents of the chest by proto number and adds them to an ordered list.  The list format is dynamic.
            // Reads Exclude_List.mes.  Items in the exclude list are not preserved in the contents of the chest.
            // This is to prevent charged magic items from being recharged by being in the chest.  Such items are lost if sent to the Ethereal Plane in the chest.
            // Exclued list is not decoded because we only care about the dictionary keys (proto numbers).  There is no need to decode the descriptions in the dictionary entries.
            var ExcludeDict = Co8.readMes("modules\\ToEE\\Exclude_List.mes");
            var exclude_list = ExcludeDict.keys/*Unknown*/();
            var contents = new List<GameObjectBody>();
            // Look for proto number 4000-12999.  These are the proto numbers for items that could be in the chest.
            var num = 4000;
            while (num <= 12999)
            {
                // Check if proto number is on the exclude list
                if (!(exclude_list).Contains(num))
                {
                    var item = chest.item_find_by_proto/*Unknown*/(num);
                    // Loop finding item to check for multiple occurences of the same item
                    while ((item != null))
                    {
                        // add the item to the list of contents
                        contents.Add(num);
                        // check if item is stackable, and if so get the quantity stacked
                        var quantity = 0;
                        var type = item.type/*Unknown*/;
                        if ((type == ObjectType.ammo))
                        {
                            quantity = item.obj_get_int/*Unknown*/(obj_f.ammo_quantity);
                        }
                        else
                        {
                            quantity = item.obj_get_int/*Unknown*/(obj_f.item_quantity);
                        }

                        // if there is more than one in the stack, add the quantity to the contents list.  Max quantity 3999 to keep quantity from matching any proto number in the list.
                        if ((quantity > 1))
                        {
                            if (quantity >= 4000)
                            {
                                quantity = 3999;
                            }

                            contents.Add(quantity);
                        }

                        // check to see if item is identified.  If so, add the identified flag 1 to the list.
                        var FLAGS = item.item_flags_get/*Unknown*/();
                        if (((FLAGS & ItemFlag.IDENTIFIED)) != 0)
                        {
                            contents.Add(1);
                        }

                        // destroy the item and check if there is another with the same proto number
                        item.destroy/*Unknown*/();
                        item = chest.item_find_by_proto/*Unknown*/(num);
                    }

                }

                num += 1;
            }

            // add "end of list" number to the end of the list.
            contents.Add(99999);
            return contents;
        }
        public static void Create_Contents(FIXME contents, FIXME chest)
        {
            // Recreates the contents of the chest from the ordered list.
            // Uses a "while" statement rather than a "for" statement in order to be able to step through the dynamic list.
            var i = 0;
            while ((range(contents.Count)).Contains(i))
            {
                // check to make sure we are looking at a proto number and not a quantity, identified flag, or end of list marker.
                if ((contents[i] >= 4000 && contents[i] != 99999))
                {
                    // create item in chest
                    var item = Utilities.create_item_in_inventory(contents[i], chest);
                    // step to the next number on the list
                    i += 1;
                    if ((range(contents.Count)).Contains(i)) // probably not necessary here, but keeps us safe.
                    {
                        // check to see if next number on the list is a quantity or identified flag
                        if (contents[i] < 4000)
                        {
                            var quantity = contents[i];
                            // check if item is ammo
                            if (item.type == ObjectType.ammo)
                            {
                                // check if "quantity" is actually a quantity and not an identified flag
                                if (quantity > 1)
                                {
                                    // "quantity" is a quantity,  Set item quantity and step to next number on the list
                                    item.SetInt(obj_f.ammo_quantity, quantity);
                                    i += 1;
                                }
                                else
                                {
                                    // "quantity" is an identified flag.  Set item quantity to 1.
                                    item.SetInt(obj_f.ammo_quantity, 1);
                                }

                            }
                            else
                            {
                                // check if item is a potion, scroll or other stackable item.
                                // check if "quantity" is actually a quantity and not an identified flag
                                if (quantity > 1)
                                {
                                    // "quantity" is a quantity,  Set item quantity and step to next number on the list
                                    item.SetInt(obj_f.item_quantity, quantity);
                                    i += 1;
                                }
                                else
                                {
                                    // "quantity" is an identified flag.  Set item quantity to 1.
                                    item.SetInt(obj_f.item_quantity, 1);
                                }

                            }

                        }

                    }

                    if ((range(contents.Count)).Contains(i)) // is necessary here
                    {
                        // check if contents[i] is an identified flag.
                        if (contents[i] == 1)
                        {
                            // flag item as identified and step to next number on the list.
                            item.SetItemFlag(ItemFlag.IDENTIFIED);
                            i += 1;
                        }

                    }

                }
                else
                {
                    i += 1;
                }

            }

            return;
        }
        // Generates a random number and sets a field in the item, chest and caster to that number.

        public static FIXME Set_ID(FIXME mini, FIXME chest, FIXME num)
        {
            // def Set_ID(mini, chest, num): # Generates a random number and sets a field in the item, chest and caster to that number.
            // ID_number = game.random_range( 1,2147483647 )
            // ID_number = ID_number^game.random_range( 1,2147483647 )#xor with next "random" number in line, should be more random
            mini.obj_set_int/*Unknown*/(obj_f.item_pad_i_2, num);
            chest.obj_set_int/*Unknown*/(obj_f.container_pad_i_1, num);
            return num;
        }
        public static FIXME Set_ID_Mini(FIXME mini, FIXME num)
        {
            mini.obj_set_int/*Unknown*/(obj_f.item_pad_i_2, num);
            return mini.obj_get_int/*Unknown*/(obj_f.item_pad_i_2);
        }
        // Reads the ID number of the miniature chest.

        public static FIXME Get_ID_Mini(FIXME mini)
        {
            // def Get_ID_Mini(mini): # Reads the ID number of the miniature chest.
            return mini.obj_get_int/*Unknown*/(obj_f.item_pad_i_2);
        }
        // Compares the ID number of the large chest and the miniature chest.  Returns 1 if they match, otherwise returns o.

        public static int Compare_ID(FIXME mini, FIXME chest)
        {
            // def Compare_ID(mini, chest): # Compares the ID number of the large chest and the miniature chest.  Returns 1 if they match, otherwise returns o.
            if ((mini.obj_get_int/*Unknown*/(obj_f.item_pad_i_2) == chest.obj_get_int/*Unknown*/(obj_f.container_pad_i_1)))
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }
        public static FIXME set_flag(FIXME mini, int x)
        {
            // def set_flag(mini, x): # Store a flag in a field of the miniature chest.  1 means on the Ethereal Plane, 0 means on the Prime Material Plane.
            mini.obj_set_int/*Unknown*/(obj_f.item_pad_i_3, x);
            return mini.obj_get_int/*Unknown*/(obj_f.item_pad_i_3);
        }
        // Reads a flag from a field of the miniature chest.

        public static FIXME get_flag(FIXME mini)
        {
            // def get_flag(mini): # Reads a flag from a field of the miniature chest.
            return mini.obj_get_int/*Unknown*/(obj_f.item_pad_i_3);
        }
        public static FIXME set_mini_map(FIXME mini, FIXME chest)
        {
            var (cxx, cyy) = chest.location/*Unknown*/;
            mini.obj_set_int/*Unknown*/(obj_f.item_pad_i_4, mini.map/*Unknown*/);
            mini.obj_set_int/*Unknown*/(obj_f.item_pad_i_5, (cxx + (cyy << 10)));
            return mini.obj_get_int/*Unknown*/(obj_f.item_pad_i_4);
        }
        public static FIXME get_mini_map(FIXME mini)
        {
            return mini.obj_get_int/*Unknown*/(obj_f.item_pad_i_4);
        }
        public static int get_mini_xy(FIXME mini)
        {
            return (mini.obj_get_int/*Unknown*/(obj_f.item_pad_i_5) >> 10, mini.obj_get_int/*Unknown*/(obj_f.item_pad_i_5) & ((Math.Pow(2, 10)) - 1));
        }

    }
}
