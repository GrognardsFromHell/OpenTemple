
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

    public class Co8
    {
        // (18:55 20/04/06) A script written by Glen Wheeler (Ugignadl) for manipulating ToEE files.
        // Requested by Cerulean the Blue from Co8.
        // (13:05 22/04/06) Added a few functions which should enable the implementation of a bag of
        // holding.  Some of them use the already written functions here (just import this scripts as a library).
        // (08/1/2013) Added Co8 configuration options (Sitra Achara)

        public static int boh_newbag(FIXME bagcontents, FIXME bagnum = 0, FIXME bgfilename = modules\ToEE\Bag_of_Holding.mes)
        {
            /*  This function only takes keyword arguments.  bagnum of 0 will ask the function to use the lowest available, otherwise will use that number.  If it already exists the game will crash (deliberately, I can change that behaviour).  bgfilename is the filename for all the bags.  The contents should be a list (or tuple) of integers.   Also, if you want each BOH to start with something inside them here is where you can do it (say with a note on how to use them, about the charged things being broken and crafted stuff etc).  */
            " This function only takes keyword arguments.  bagnum of 0 will ask the function to use the lowest available, otherwise will use that number.  If it already exists the game will crash (deliberately, I can change that behaviour).  bgfilename is the filename for all the bags.  The contents should be a list (or tuple) of integers.   Also, if you want each BOH to start with something inside them here is where you can do it (say with a note on how to use them, about the charged things being broken and crafted stuff etc). ";
            // So we read in all the bags.
            var allBagDict = readMes(bgfilename);
            // Then insert the new bag.
            var linenumbers = allBagDict.keys/*Unknown*/();
            if (bagnum)
            {
                if ((allBagDict).Contains(bagnum))
                {
                    throw new Exception(); /*"BadbagnumError"*/
                }

            }
            else
            {
                bagnum = 1;
                while ((linenumbers).Contains(bagnum))
                {
                    bagnum += 1;
                }

            }

            allBagDict[bagnum] = bagcontents;
            // Then write it all back again.
            // writeMes(bgfilename, allBagDict)
            // print 'New Bag'
            return bagnum;
        }
        public static FIXME readMes(FIXME mesfile)
        {
            /*  Read the mesfile into a dictionary indexed by line number.  */
            " Read the mesfile into a dictionary indexed by line number. ";
            var mesFile = file(mesfile, "r");
            var mesDict = new Dictionary<FIXME, FIXME>
            {
            }
            ;
            foreach (var line in mesFile.readlines/*Unknown*/())
            {
                // Remove whitespace.
                line = line.strip/*Unknown*/();
                // Ignore empty lines.
                if (!line)
                {
                    continue;

                }

                // Ignore comment lines.
                if (line[0] != "{")
                {
                    continue;

                }

                // Decode the line.  Just standard python string processing.
                line = line.split/*Unknown*/("}")[..-1];
                for (var i = 0; i < line.Count; i++)
                {
                    line[i] = line[i].strip/*Unknown*/();
                    line[i] = line[i][1..];
                }

                var contents = line[1..];
                // Insert the line into the mesDict.
                mesDict[(int)(line[0])] = contents;
            }

            mesFile.close/*Unknown*/();
            // print 'File read'
            return mesDict;
        }
        public static int writeMes(FIXME mesfile, FIXME mesDict)
        {
            /*  Write the dictionary mesDict as a mesfile.  This does not presever comments (although it could if desired).  Overwrites mesfile if it already exists. */
            " Write the dictionary mesDict as a mesfile.  This does not presever comments (although it could if desired).  Overwrites mesfile if it already exists.";
            var mesFile = file(mesfile, "w");
            var linenumbers = mesDict.keys/*Unknown*/();
            linenumbers.sort/*Unknown*/();
            foreach (var linenumber in linenumbers)
            {
                mesFile.write/*Unknown*/("{" + linenumber.ToString() + "}{");
                foreach (var thing in mesDict[linenumber])
                {
                    if (thing == mesDict[linenumber][-1])
                    {
                        mesFile.write/*Unknown*/(thing.ToString());
                    }
                    else
                    {
                        mesFile.write/*Unknown*/(thing.ToString() + ", ");
                    }

                }

                mesFile.write/*Unknown*/("}\n");
            }

            mesFile.close/*Unknown*/();
            // print 'File written'
            return 1;
        }
        public static FIXME boh_getContents(FIXME bagnum, FIXME bgfilename = modules\ToEE\Bag_of_Holding.mes)
        {
            /*  This can be called when opening the bag.  The bag filename is (I am assuming) fixed, but can be set by passing a keyword argument.  bagnum is the line number to use for that bag in the bgfilename.  */
            " This can be called when opening the bag.  The bag filename is (I am assuming) fixed, but can be set by passing a keyword argument.  bagnum is the line number to use for that bag in the bgfilename. ";
            var allBagDict = readMes(bgfilename);
            // Note:  If bagnum is not an integer we will have a CTD.  I can fix this if it is the case by coercing the datatype.
            var contents = __boh_decode(allBagDict[bagnum]);
            // contents will be an ordered list of integers, ready to be turned into objects.
            // print ' Get Contents'
            return contents;
        }
        public static FIXME __boh_decode(FIXME bohcontents)
        {
            /*  bohcontents should just be a comma delimited series of integers.  We don't have any more support than that (CB: if we have more complex data types in the bag in future then this is the function to be changed.)   Sub-bags (i.e. BOH within BOH) could be implemented by decoding lists within lists.  However I'm not even sure that is in accordance with the rules.  */
            " bohcontents should just be a comma delimited series of integers.  We don't have any more support than that (CB: if we have more complex data types in the bag in future then this is the function to be changed.)   Sub-bags (i.e. BOH within BOH) could be implemented by decoding lists within lists.  However I'm not even sure that is in accordance with the rules. ";
            // [0] is there to comply with the output of the readMes function.
            var l = bohcontents[0].split/*Unknown*/(", ");
            for (var i = 0; i < l.Count; i++)
            {
                // This should cause a crash, but I am testing with non-integers.  If you want to be hardcore then just remove the try--except.
                try
                {
                    l[i] = (int)(l[i]);
                }
                catch (Exception e)
                {
                    Logger.Info("WARNING: NON-INTEGER FOUND IN BAG OF HOLDING!");
                    Logger.Info("Non-integer found is{0}", l[i]);
                }

            }

            // print 'Decoded'
            return l;
        }
        public static void _boh_removeitem(FIXME bagnum, FIXME itemnum, FIXME bgfilename = modules\ToEE\Bag_of_Holding.mes)
        {
            /*  Remove the item itemnum from the bag bagnum.  If it's not in there we get a (deliberate) crash.  */
            " Remove the item itemnum from the bag bagnum.  If it's not in there we get a (deliberate) crash. ";
            var allBagDict = readMes(bgfilename);
            contents.remove/*Unknown*/(itemnum);
            allBagDict[bagnum] = contents;
            writeMes(bgfilename, allBagDict);
        }
        public static void _boh_insertitem(FIXME bagnum, FIXME itemnum, FIXME bgfilename = modules\ToEE\Bag_of_Holding.mes)
        {
            /*  This function will insert the integer itemnum at the end of the list associated with bagnum in bgfilename.  */
            " This function will insert the integer itemnum at the end of the list associated with bagnum in bgfilename. ";
            var allBagDict = readMes(bgfilename);
            contents.append/*Unknown*/(itemnum);
            allBagDict[bagnum] = contents;
            writeMes(bgfilename, allBagDict);
        }
        // Added by Darmagon	                                              #

        private static readonly obj_f SPELL_FLAGS_BASE = obj_f.secretdoor_dc;
        private static readonly List<FIXME> has_obj_list = new List<GameObjectBody>();
        private static readonly List<FIXME> objs_to_destroy_list = new List<GameObjectBody>();
        private static readonly List<FIXME> active_spells = new List<GameObjectBody>();
        private static readonly GameObjectBody holder = null;
        private static readonly GameObjectBody holder2 = null;
        private static readonly int OSF_IS_IRON_BODY = 1;
        private static readonly int OSF_IS_TENSERS_TRANSFORMATION = 2;
        // also used by ShiningTed for Antidote

        private static readonly int OSF_IS_ANALYZE_DWEOMER = 4;
        // also used by ShiningTed for Foresight

        private static readonly int OSF_IS_HOLY_SWORD = 8;
        private static readonly int OSF_IS_PROTECTION_FROM_SPELLS = 16;
        private static readonly int OSF_IS_MORDENKAINENS_SWORD = 32;
        private static readonly int OSF_IS_FLAMING_SPHERE = 64;
        private static readonly int OSF_IS_SUMMONED = 128;
        private static readonly int OSF_IS_HEZROU_STENCH = 256;
        private static readonly int OSF_IS_TONGUES = 512;
        private static readonly int OSF_IS_DISGUISE_SELF = 1024;
        private static readonly int OSF_IS_DEATH_WARD = 2048;
        private static readonly int ITEM_HOLDER = 1027;
        public static void set_spell_flag(GameObjectBody obj, Co8SpellFlag flag)
        {
            var val = obj.GetInt(SPELL_FLAGS_BASE);
            obj.SetInt(SPELL_FLAGS_BASE, val | flag);
            return obj.GetInt(SPELL_FLAGS_BASE);
        }
        public static Co8SpellFlag get_spell_flags(GameObjectBody obj)
        {
            return obj.GetInt(SPELL_FLAGS_BASE);
        }
        public static bool is_spell_flag_set(GameObjectBody obj, Co8SpellFlag flag)
        {
            return obj.GetInt(SPELL_FLAGS_BASE) & flag;
        }
        public static void unset_spell_flag(GameObjectBody obj, Co8SpellFlag flag)
        {
            var val = obj.GetInt(SPELL_FLAGS_BASE);
            if ((val & flag) != 0)
            {
                obj.SetInt(SPELL_FLAGS_BASE, val - flag);
            }

            return obj.GetInt(SPELL_FLAGS_BASE);
        }
        public static bool are_spell_flags_null(GameObjectBody obj)
        {
            if (obj.GetInt(SPELL_FLAGS_BASE) == 0)
            {
                return true;
            }

            return false;
        }
        public static bool check_for_protection_from_spells(SpellTarget[] t_list, int check)
        {
        FIXME: GLOBAL has_obj_list;

        FIXME: GLOBAL objs_to_destroy_list;

        FIXME: GLOBAL holder;

        FIXME: GLOBAL holder2;

            holder = GameSystems.MapObject.CreateObject(14629, PartyLeader.GetLocation());
            holder2 = GameSystems.MapObject.CreateObject(14629, PartyLeader.GetLocation());
            var ret = 0;
            foreach (var obj in t_list)
            {
                var prot_obj = obj.Object.FindItemByProto(6400);
                while (prot_obj != null && !is_spell_flag_set(prot_obj, OSF_IS_PROTECTION_FROM_SPELLS))
                {
                    prot_obj.ClearItemFlag(ItemFlag.NO_DROP);
                    holder.GetItem(prot_obj);
                    prot_obj = obj.Object.FindItemByProto(6400);
                }

                var get_back_obj = holder.FindItemByProto(6400);
                while (get_back_obj != null)
                {
                    obj.Object.GetItem(get_back_obj);
                    get_back_obj.SetItemFlag(ItemFlag.NO_DROP);
                    get_back_obj = holder.FindItemByProto(6400);
                }

                if (prot_obj != null)
                {
                    ret = 1;
                    has_obj_list.append/*UnknownList*/(obj.Object);
                    prot_obj.ClearItemFlag(ItemFlag.NO_DROP);
                    holder2.GetItem(prot_obj);
                    var new_obj = GameSystems.MapObject.CreateObject(6400, PartyLeader.GetLocation());
                    set_spell_flag(new_obj, OSF_IS_PROTECTION_FROM_SPELLS);
                    objs_to_destroy_list.append/*UnknownList*/(new_obj);
                    new_obj.AddConditionToItem("Saving Throw Resistance Bonus", 0, 8);
                    new_obj.AddConditionToItem("Saving Throw Resistance Bonus", 1, 8);
                    new_obj.AddConditionToItem("Saving Throw Resistance Bonus", 2, 8);
                    obj.Object.GetItem(new_obj);
                }

            }

            holder.Destroy();
            if (ret == 0)
            {
                holder2.Destroy();
            }

            return ret;
        }
        public static void replace_protection_from_spells()
        {
        FIXME: GLOBAL has_obj_list;

        FIXME: GLOBAL objs_to_destroy_list;

        FIXME: GLOBAL holder2;

            foreach (var obj in objs_to_destroy_list)
            {
                obj.destroy/*Unknown*/();
            }

            var objs_to_destroy = new List<GameObjectBody>();
            var count = 0;
            while (count < has_obj_list.Count)
            {
                var obj_to_get = holder2.FindItemByProto(6400);
                has_obj_list[count].item_get/*Unknown*/(obj_to_get);
                obj_to_get.SetItemFlag(ItemFlag.NO_DROP);
                count = count + 1;
            }

            var holder_objs = new List<GameObjectBody>();
            has_obj_list = new List<GameObjectBody>();
            holder2.Destroy();
        }
        FIXME
private class obj_holder
        {
            GameObjectBody obj = null;
            public FIXME __init__(Unknown self, Object obj)
            {
                self.obj/*Unknown*/ = obj;
            }

        }
        FIXME
private class spell_with_objs_packet
        {
            public FIXME __init__(Unknown self, Unknown spell_obj, Unknown sentinel1, Unknown sentinel2, Spell spell)
            {
                self.spell_obj/*Unknown*/ = spell_obj;
                self.sentinel1/*Unknown*/ = sentinel1;
                self.sentinel2/*Unknown*/ = sentinel2;
                self.spell/*Unknown*/ = spell;
            }

        }
        public static GameObjectBody is_obj_in_active_spell_list(GameObjectBody obj)
        {
            var ret = null;
            foreach (var object in active_spells) {
                if (obj == object.sentinel1/*Unknown*/ || obj == object.sentinel2/*Unknown*/ || obj == object.spell_obj/*Unknown*/)
                {
                    return object;
                }

            }

            return ret;
        }
        public static List<FIXME> get_active_spells()
        {
            return active_spells;
        }
        public static void append_to_active_spells(SpellPacketBody spell, FIXME spell_obj, FIXME sent1, FIXME sent2)
        {
        FIXME: GLOBAL active_spells;

            var new_spell = spell_with_objs_packet(spell_obj, sent1, sent2, spell);
            var ofile = open("append.txt", "w");
            ofile.write/*Unknown*/(new_spell.sentinel1/*Unknown*/.ToString() + "\n");
            active_spells.append/*UnknownList*/(new_spell);
            ofile.write/*Unknown*/(CurrentTime.time_in_game/*Time*/().ToString() + "\n");
            ofile.write/*Unknown*/(get_active_spells().ToString() + "\n");
            ofile.close/*Unknown*/();
        }
        public static void remove_from_active_spells(FIXME spell_packet)
        {
        FIXME: GLOBAL active_spells;

            var count = 0;
            while (count < active_spells.Count)
            {
                if (active_spells[count] == spell_packet)
                {
                    active_spells[count].sentinel1/*Unknown*/.destroy/*Unknown*/();
                    active_spells[count].sentinel2/*Unknown*/.destroy/*Unknown*/();
                FIXME: DEL active_spells[count];

                    break;

                }

                count = count + 1;
            }

        }
        public static List<GameObjectBody> build_obj_list(FIXME list)
        {
            var t_list = new List<GameObjectBody>();
            foreach (var t in list)
            {
                t_list.Add(obj_holder(t));
            }

            return t_list;
        }
        public static GameObjectBody find_spell_obj_with_flag(GameObjectBody target, int item, Co8SpellFlag flag)
        {
            GameObjectBody ret = null;
            var item_holder = GameSystems.MapObject.CreateObject(ITEM_HOLDER, target.GetLocation());
            var prot_item = target.FindItemByProto(item);
            while (prot_item != null && ret == null)
            {
                if (is_spell_flag_set(prot_item, flag))
                {
                    ret = prot_item;
                }

                prot_item.ClearItemFlag(ItemFlag.NO_DROP);
                item_holder.GetItem(prot_item);
                prot_item = target.FindItemByProto(item);
            }

            prot_item = item_holder.FindItemByProto(item);
            while (prot_item != null)
            {
                target.GetItem(prot_item);
                prot_item.SetItemFlag(ItemFlag.NO_DROP);
                prot_item = item_holder.FindItemByProto(item);
            }

            item_holder.Destroy();
            return ret;
        }
        public static void destroy_spell_obj_with_flag(GameObjectBody target, int proto_id, Co8SpellFlag flag)
        {
            var ret = 0;
            var item_holder = GameSystems.MapObject.CreateObject(ITEM_HOLDER, target.GetLocation());
            var prot_item = target.FindItemByProto(proto_id);
            while (prot_item != null)
            {
                if (is_spell_flag_set(prot_item, flag))
                {
                    ret = 1;
                    Logger.Info("found it");
                    break;

                }

                prot_item.ClearItemFlag(ItemFlag.NO_DROP);
                item_holder.GetItem(prot_item);
                prot_item = target.FindItemByProto(proto_id);
            }

            if (ret == 1)
            {
                prot_item.Destroy();
                Logger.Info("destroyed it");
            }

            prot_item = item_holder.FindItemByProto(proto_id);
            while (prot_item != null)
            {
                target.GetItem(prot_item);
                if (proto_id == 6400)
                {
                    prot_item.SetItemFlag(ItemFlag.NO_DROP);
                }

                prot_item = item_holder.FindItemByProto(proto_id);
            }

            item_holder.Destroy();
            return ret;
        }
        public static void set_blast_delay(FIXME num)
        {
            // if num >= 0 and num <=5:
            // ifile = open("delayed_blast_fireball.txt", "w")
            // ifile.write(str(num))
            // ifile.close()
            Logger.Info("someone actually uses set_blast_delay?
            ");
        return; // was this even used?
        }
        public static bool is_in_party(GameObjectBody obj)
        {
            foreach (var x in GameSystems.Party.PartyMembers)
            {
                if (obj == x)
                {
                    return true;
                }

            }

            return false;
        }
        public static void unequip(FIXME slot, GameObjectBody npc, FIXME whole_party = 0)
        {
            var unequip_set = new List<GameObjectBody>();
            if (whole_party)
            {
                unequip_set = GameSystems.Party.PartyMembers;
            }
            else
            {
                unequip_set = new[] { npc };
            }

            foreach (var npc2 in unequip_set)
            {
                var i = 0;
                var j = 0;
                var item = npc2.item_worn_at/*Unknown*/(slot);
                if (item != null)
                {
                    if ((item.item_flags_get/*Unknown*/() & ItemFlag.NO_DROP) != 0)
                    {
                        item.item_flag_unset/*Unknown*/(ItemFlag.NO_DROP);
                        i = 1;
                    }

                    if ((item.item_flags_get/*Unknown*/() & ItemFlag.NO_TRANSFER) != 0)
                    {
                        item.item_flag_unset/*Unknown*/(ItemFlag.NO_TRANSFER);
                        j = 1;
                    }

                    holder = GameSystems.MapObject.CreateObject(1004, npc2.location/*Unknown*/);
                    holder.GetItem(item);
                    var tempp = npc2.item_get/*Unknown*/(item);
                    var pc_index = 0;
                    while (tempp == 0 && pc_index < GameSystems.Party.PartyMembers.Count)
                    {
                        // while tempp == 0 and pc_index < len(game.party): #this part is insurance against filled up inventory for any PCs
                        if (GameSystems.Party.GetPartyGroupMemberN(pc_index).type == ObjectType.pc)
                        {
                            tempp = GameSystems.Party.GetPartyGroupMemberN(pc_index).GetItem(item);
                        }

                        pc_index += 1;
                    }

                    if (i)
                    {
                        item.item_flag_set/*Unknown*/(ItemFlag.NO_DROP);
                    }

                    if (j)
                    {
                        item.item_flag_set/*Unknown*/(ItemFlag.NO_TRANSFER);
                    }

                    holder.Destroy();
                }

            }

        }
        public static void weap_too_big(FIXME weap_user)
        {
            if (weap_user.is_category_type/*Unknown*/(MonsterCategory.giant))
            {
                return;
            }

            var weap_1 = weap_user.item_worn_at/*Unknown*/(3);
            var weap_2 = weap_user.item_worn_at/*Unknown*/(4);
            var size_1 = weap_1.obj_get_int/*Unknown*/(obj_f.size);
            if (size_1 > SizeCategory.Medium && weap_2 != null)
            {
                unequip(3, weap_user);
            }

            if (weap_2 != null) // fix - added OBJ_HANDLE_NULL check
            {
                var size_2 = weap_2.obj_get_int/*Unknown*/(obj_f.size);
                if (size_2 > SizeCategory.Medium)
                {
                    unequip(4, weap_user);
                }

            }

            return;
        }
        // End added by Shiningted                                        #
        // Added by Cerulean the Blue                                    #

        public static FIXME read_field(FIXME object, FIXME field)
        {
            return object.obj_get_int/*Unknown*/(field);
        }
        public static FIXME write_field(FIXME object, FIXME field, FIXME value)
        {
            object.obj_set_int/*Unknown*/(field, value);
            return object.obj_get_int/*Unknown*/(field);
        }
        public static FIXME clear_field(FIXME object, FIXME field)
        {
            object.obj_set_int/*Unknown*/(field, 0);
            return object.obj_get_int/*Unknown*/(field);
        }
        public static GameObjectBody GetCritterHandle(SpellPacketBody spell, int critter_name)
        {
            // Returns a handle that can be used to manipulate the summoned  creature object
            foreach (var critter in ObjList.ListVicinity(spell.aoeCenter, ObjectListFilter.OLC_CRITTERS))
            {
                if ((critter.GetNameId() == critter_name && !(is_spell_flag_set(critter, OSF_IS_SUMMONED)) && critter.IsFriendly(spell.caster)))
                {
                    set_spell_flag(critter, OSF_IS_SUMMONED);
                    return critter;
                }

            }

            return null;
        }
        public static void End_Spell(SpellPacketBody spell)
        {
            // This is used to forcibly end Co8-made spells that are supposed to be of instantaneous duration
            // Previous version used Cerulean the Blue's workaround method (commented out below)
            // spell.summon_monsters( 1, 14456 )
            // critter = GetCritterHandle( spell, 14456)
            // spell.caster.follower_remove(critter)
            // critter.destroy()
            // proper method:
            spell.EndSpell(true); // this forces the spell_end to work even with a non-empty target list
            return;
        }
        public static void Timed_Destroy(GameObjectBody obj, FIXME time)
        {
            StartTimer(time, () => destroy(obj)); // 1000 = 1 second
            return;
        }
        public static void Timed_Runoff(GameObjectBody obj, FIXME runoff_time = 1000, FIXME runoff_location)
        {
            if (runoff_location == -1)
            {
                obj.RunOff();
            }
            else if (typeof(runoff_location) == typeof(obj.GetLocation()))
{
                obj.RunOff(runoff_location);
            }
else if (typeof(runoff_location) == typeof(new[] { 1, 2 }))
{
                obj.RunOff(new locXY(runoff_location[0], runoff_location[1]));
            }
else
            {
                obj.RunOff();
            }

            StartTimer(runoff_time, () => Timed_Runoff_Set_OF_OFF(obj)); // 1000 = 1 second, default
            return;
        }
        public static void Timed_Runoff_Set_OF_OFF(GameObjectBody obj)
        {
            obj.SetObjectFlag(ObjectFlag.OFF);
            return;
        }
        // Destroys object.  Neccessary for time event destruction to work.

        public static int destroy(GameObjectBody obj)
        {
            // def destroy(obj): # Destroys object.  Neccessary for time event destruction to work.
            obj.Destroy();
            return 1;
        }
        public static void StopCombat(GameObjectBody obj, int flag)
        {
            if (typeof(obj) == typeof(null.GetLocation()))
{
                var loc1 = obj;
            }
else
            {
                var loc1 = obj.GetLocation();
            }

            // game.particles( 'Orb-Summon-Air-Elemental', game.party[0] )
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                foreach (var critter in ObjList.ListVicinity(loc1, ObjectListFilter.OLC_CRITTERS))
                {
                    critter.AIRemoveFromShitlist(pc);
                    if (flag == 1)
                    {
                        critter.ClearNpcFlag(NpcFlag.KOS);
                    }

                }

                obj.AIRemoveFromShitlist(pc);
            }

            return;
        }
        public static float group_challenge_rating()
        {
            return (Utilities.group_average_level(SelectedPartyLeader) * (GameSystems.Party.PartyMembers.Count / 4f));
        }
        // Added by Hazelnut                                             #
        // Replacement for the D20STD_F_POISON flag for saving throws. The STD define contains
        // the enum index value, 4, which is incorrect as it's checked against the bitmask 8
        // in temple.dll.

        private static readonly int D20CO8_F_POISON = 8;
        // Util functions for getting & setting words, bytes and nibbles in object integers.
        // object = reference to the object containing the integer variable.
        // var	 = the variable to be used. e.g. obj_f_weapon_pad_i_2
        // idx	 = the index of the word (0-1), byte (0-3) or nibble (0-7) to use.
        // val	 = the value to be set.

        public static FIXME getObjVarDWord(FIXME object, FIXME var)
        {
            return object.obj_get_int/*Unknown*/(var);
        }
        public static void setObjVarDWord(FIXME object, FIXME var, FIXME val)
        {
            object.obj_set_int/*Unknown*/(var, val);
        }
        public static FIXME getObjVarWord(FIXME object, FIXME var, FIXME idx)
        {
            return getObjVar(object, var, idx, 65535, 16);
        }
        public static void setObjVarWord(FIXME object, FIXME var, FIXME idx, FIXME val)
        {
            setObjVar(object, var, idx, val, 65535, 16);
        }
        public static FIXME getObjVarByte(FIXME object, FIXME var, FIXME idx)
        {
            return getObjVar(object, var, idx, 255, 8);
        }
        public static void setObjVarByte(FIXME object, FIXME var, FIXME idx, FIXME val)
        {
            setObjVar(object, var, idx, val, 255, 8);
        }
        public static FIXME getObjVarNibble(FIXME object, FIXME var, FIXME idx)
        {
            return getObjVar(object, var, idx, 15, 4);
        }
        public static void setObjVarNibble(FIXME object, FIXME var, FIXME idx, FIXME val)
        {
            setObjVar(object, var, idx, val, 15, 4);
        }
        public static FIXME getObjVar(FIXME object, FIXME var, FIXME idx, FIXME mask, FIXME bits)
        {
            var bitMask = mask << (idx * bits);
            var val = object.obj_get_int/*Unknown*/(var) & bitMask;
            val = val >> (idx * bits);
            return (val & mask);
        }
        public static void setObjVar(FIXME object, FIXME var, FIXME idx, FIXME val, FIXME mask, FIXME bits)
        {
            // print "obj=", object, " var=", var, " idx=", idx, " val=", val
            var bitMask = mask << (idx * bits);
            val = val << (idx * bits);
            var oldVal = object.obj_get_int/*Unknown*/(var) & ~bitMask;
            object.obj_set_int/*Unknown*/(var, oldVal | val);
        }
        // added by dolio #
        // Temporarily renders a target invincible, and deals
        // some damage (which gets reduced to 0). This allows
        // the dealer to gain experience for killing the target.

        public static void plink(GameObjectBody critter, SpellPacketBody spell)
        {
            var invuln = critter.GetObjectFlags() & ObjectFlag.INVULNERABLE;
            var dice = Dice.Parse("1d1");
            critter.SetObjectFlag(ObjectFlag.INVULNERABLE);
            critter.DealSpellDamage(spell.caster, DamageType.Unspecified, dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, spell.spellId);
            if (!invuln)
            {
                critter.ClearObjectFlag(ObjectFlag.INVULNERABLE);
            }

        }
        public static void slay_critter(GameObjectBody critter, SpellPacketBody spell)
        {
            plink(critter, spell);
            critter.Kill();
        }
        public static void slay_critter_by_effect(GameObjectBody critter, SpellPacketBody spell)
        {
            plink(critter, spell);
            critter.KillWithDeathEffect();
        }
        // This kills a critter, and gets rid of the body, while
        // dropping the equipment.

        public static void disintegrate_critter(FIXME ensure_exp, GameObjectBody critter, SpellPacketBody spell)
        {
            spell.duration = 0;
            if (ensure_exp)
            {
                plink(critter, spell);
            }

            critter.Kill();
            critter.AddCondition("sp-Animate Dead", spell.spellId, spell.duration, 3);
        }
        // end dolio additions

        // Added by Sitra Achara
        public static void config_override_bits(int var_no, int bit, bool new_value)
        {
            var mask = 1 << bit;
            if (new_value)
            {
                SetGlobalVar(var_no, GetGlobalVar(var_no) | mask);
            }
            else
            {
                SetGlobalVar(var_no, GetGlobalVar(var_no) & ~mask);
            }
        }

        public static void config_override_bits(int var_no, IEnumerable<int> bit_range, bool new_value)
        {
            var bit_maskk = 0;
            foreach (var diggitt in bit_range)
            {
                bit_maskk += Math.Pow(2, diggitt);
            }

            GetGlobalVar(var_no) ^= (bit_maskk & (GetGlobalVar(var_no) ^ (new_value << bit_range[0])));
        }

        public static void get_Co8_options_from_ini()
        {
            try
            {
                var i_file = open("Co8_config.ini", "r"); // opens the file in ToEE main folder now
                var s = "initial value";
                var failsafe_count = 0;
                while (s != "" && failsafe_count < 1000)
                {
                    failsafe_count += 1;
                    s = i_file.readline/*Unknown*/();
                    var s2 = s.split/*Unknown*/("{");
                    if (s2.Count >= 3)
                    {
                        // check if it's an actual entry line with the format:
                        // { Param name } { value } {description (this bracket is optional!) }
                        // will return an array ['','[Param name text]} ','[value]}, [Description text]}'] entry
                        var param_name = s2[1].replace/*Unknown*/("}", "").strip/*Unknown*/();
                        var par_lower = param_name.ToLowerInvariant();
                        var param_value = s2[2].replace/*Unknown*/("}", "").strip/*Unknown*/();
                        if (param_value.isdigit/*Unknown*/())
                        {
                            param_value = (int)(param_value);
                            if (par_lower == "Party_Run_Speed".ToLowerInvariant())
                            {
                                config_override_bits(449, new []{0, 1, 2}, param_value);
                            }
                            else if (par_lower == "Disable_New_Plots".ToLowerInvariant())
                            {
                                config_override_bits(450, 0, param_value);
                            }
                            else if (par_lower == "Disable_Target_Of_Revenge".ToLowerInvariant())
                            {
                                config_override_bits(450, 10, param_value);
                            }
                            else if (par_lower == "Disable_Moathouse_Ambush".ToLowerInvariant())
                            {
                                config_override_bits(450, 11, param_value);
                            }
                            else if (par_lower == "Disable_Arena_Of_Heroes".ToLowerInvariant())
                            {
                                config_override_bits(450, 12, param_value);
                            }
                            else if (par_lower == "Disable_Reactive_Temple".ToLowerInvariant())
                            {
                                config_override_bits(450, 13, param_value);
                            }
                            else if (par_lower == "Disable_Screng_Recruit_Special".ToLowerInvariant())
                            {
                                config_override_bits(450, 14, param_value);
                            }
                            else if (par_lower == "Entangle_Outdoors_Only".ToLowerInvariant())
                            {
                                config_override_bits(451, 0, param_value);
                            }
                            else if (par_lower == "Elemental_Spells_At_Elemental_Nodes".ToLowerInvariant())
                            {
                                config_override_bits(451, 1, param_value);
                            }
                            else if (par_lower == "Charm_Spell_DC_Modifier".ToLowerInvariant())
                            {
                                config_override_bits(451, 2, param_value);
                            }
                            else if (par_lower == "AI_Ignore_Summons".ToLowerInvariant())
                            {
                                config_override_bits(451, 3, param_value);
                            }
                            else if (par_lower == "AI_Ignore_Spiritual_Weapons".ToLowerInvariant())
                            {
                                config_override_bits(451, 4, param_value);
                            }
                            else if (par_lower == "Random_Encounter_XP_Reduction".ToLowerInvariant())
                            {
                                config_override_bits(451, new []{5, 6, 7}, param_value); // bits 5-7
                            }
                            else if (par_lower == "Stinking_Cloud_Duration_Nerf".ToLowerInvariant())
                            {
                                config_override_bits(451, 8, param_value);
                            }
                        }

                    }

                }

                if ((GetGlobalVar(449) & (Math.Pow(2, 0) + Math.Pow(2, 1) + Math.Pow(2, 2))) != 0)
                {
                    Batch.speedup(GetGlobalVar(449) & (Math.Pow(2, 0) + Math.Pow(2, 1) + Math.Pow(2, 2)), GetGlobalVar(449) & (Math.Pow(2, 0) + Math.Pow(2, 1) + Math.Pow(2, 2)));
                }

                i_file.close/*Unknown*/();
            }
            catch (Exception e)
            {
                return;
            }

            return;
        }

    }
}
