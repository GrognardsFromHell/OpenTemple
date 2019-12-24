
using System;
using System.Collections.Generic;
using System.IO;
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

    public class Co8
    {

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private static readonly obj_f SPELL_FLAGS_BASE = obj_f.secretdoor_dc;
        private static readonly List<GameObjectBody> has_obj_list = new List<GameObjectBody>();
        private static readonly List<GameObjectBody> objs_to_destroy_list = new List<GameObjectBody>();
        private static GameObjectBody holder = null;
        private static GameObjectBody holder2 = null;
        private static readonly int ITEM_HOLDER = 1027;
        public static void set_spell_flag(GameObjectBody obj, Co8SpellFlag flag)
        {
            var val = (Co8SpellFlag) obj.GetInt(SPELL_FLAGS_BASE);
            obj.SetInt(SPELL_FLAGS_BASE, (int)(val | flag));
            obj.GetInt(SPELL_FLAGS_BASE);
        }
        public static Co8SpellFlag get_spell_flags(GameObjectBody obj)
        {
            return (Co8SpellFlag) obj.GetInt(SPELL_FLAGS_BASE);
        }
        public static bool is_spell_flag_set(GameObjectBody obj, Co8SpellFlag flag)
        {
            return (obj.GetInt(SPELL_FLAGS_BASE) & (int) flag) != 0;
        }
        public static void unset_spell_flag(GameObjectBody obj, Co8SpellFlag flag)
        {
            var val = get_spell_flags(obj);
            if ((val & flag) != 0)
            {
                val &= ~flag;
                obj.SetInt32(SPELL_FLAGS_BASE, (int) val);
            }
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
            holder = GameSystems.MapObject.CreateObject(14629, PartyLeader.GetLocation());
            holder2 = GameSystems.MapObject.CreateObject(14629, PartyLeader.GetLocation());
            var ret = false;
            foreach (var obj in t_list)
            {
                var prot_obj = obj.Object.FindItemByProto(6400);
                while (prot_obj != null && !is_spell_flag_set(prot_obj, Co8SpellFlag.ProtectionFromSpells))
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
                    ret = true;
                    has_obj_list.Add(obj.Object);
                    prot_obj.ClearItemFlag(ItemFlag.NO_DROP);
                    holder2.GetItem(prot_obj);
                    var new_obj = GameSystems.MapObject.CreateObject(6400, PartyLeader.GetLocation());
                    set_spell_flag(new_obj, Co8SpellFlag.ProtectionFromSpells);
                    objs_to_destroy_list.Add(new_obj);
                    new_obj.AddConditionToItem("Saving Throw Resistance Bonus", 0, 8);
                    new_obj.AddConditionToItem("Saving Throw Resistance Bonus", 1, 8);
                    new_obj.AddConditionToItem("Saving Throw Resistance Bonus", 2, 8);
                    obj.Object.GetItem(new_obj);
                }

            }

            holder.Destroy();
            if (!ret)
            {
                holder2.Destroy();
            }

            return ret;
        }
        public static void replace_protection_from_spells()
        {
            foreach (var obj in objs_to_destroy_list)
            {
                obj.Destroy();
            }
            objs_to_destroy_list.Clear();

            var count = 0;
            while (count < has_obj_list.Count)
            {
                var obj_to_get = holder2.FindItemByProto(6400);
                has_obj_list[count].GetItem(obj_to_get);
                obj_to_get.SetItemFlag(ItemFlag.NO_DROP);
                count = count + 1;
            }

            has_obj_list.Clear();
            holder2.Destroy();
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
        public static bool destroy_spell_obj_with_flag(GameObjectBody target, int proto_id, Co8SpellFlag flag)
        {
            var ret = false;
            var item_holder = GameSystems.MapObject.CreateObject(ITEM_HOLDER, target.GetLocation());
            var prot_item = target.FindItemByProto(proto_id);
            while (prot_item != null)
            {
                if (is_spell_flag_set(prot_item, flag))
                {
                    ret = true;
                    Logger.Info("found it");
                    break;

                }

                prot_item.ClearItemFlag(ItemFlag.NO_DROP);
                item_holder.GetItem(prot_item);
                prot_item = target.FindItemByProto(proto_id);
            }

            if (ret)
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
        public static void unequip(EquipSlot slot, GameObjectBody npc, bool whole_party = false)
        {
            IEnumerable<GameObjectBody> unequip_set;
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
                var i = false;
                var j =  false;
                var item = npc2.ItemWornAt(slot);
                if (item != null)
                {
                    if ((item.GetItemFlags() & ItemFlag.NO_DROP) != 0)
                    {
                        item.ClearItemFlag(ItemFlag.NO_DROP);
                        i = true;
                    }

                    if ((item.GetItemFlags() & ItemFlag.NO_TRANSFER) != 0)
                    {
                        item.ClearItemFlag(ItemFlag.NO_TRANSFER);
                        j = true;
                    }

                    holder = GameSystems.MapObject.CreateObject(1004, npc2.GetLocationFull());
                    holder.GetItem(item);
                    var tempp = npc2.GetItem(item);

                    foreach (var partyMember in GameSystems.Party.PartyMembers)
                    {
                        if (tempp)
                        {
                            break;
                        }

                        // this part is insurance against filled up inventory for any PCs
                        if (partyMember.IsPC())
                        {
                            tempp = partyMember.GetItem(item);
                        }
                    }

                    if (i)
                    {
                        item.SetItemFlag(ItemFlag.NO_DROP);
                    }

                    if (j)
                    {
                        item.SetItemFlag(ItemFlag.NO_TRANSFER);
                    }

                    holder.Destroy();
                }

            }

        }

        // End added by Shiningted                                        #
        // Added by Cerulean the Blue                                    #

        public static GameObjectBody GetCritterHandle(SpellPacketBody spell, int critter_name)
        {
            // Returns a handle that can be used to manipulate the summoned  creature object
            foreach (var critter in ObjList.ListVicinity(spell.aoeCenter.location, ObjectListFilter.OLC_CRITTERS))
            {
                if ((critter.GetNameId() == critter_name && !(is_spell_flag_set(critter, Co8SpellFlag.Summoned)) && critter.IsFriendly(spell.caster)))
                {
                    set_spell_flag(critter, Co8SpellFlag.Summoned);
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
        public static void Timed_Destroy(GameObjectBody obj, int time)
        {
            StartTimer(time, () => destroy(obj)); // 1000 = 1 second
            return;
        }

        // Destroys obj.  Neccessary for time event destruction to work.

        public static int destroy(GameObjectBody obj)
        {
            // def destroy(obj): # Destroys obj.  Neccessary for time event destruction to work.
            obj.Destroy();
            return 1;
        }

        public static void StopCombat(GameObjectBody obj, int flag)
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                foreach (var critter in ObjList.ListVicinity(obj, ObjectListFilter.OLC_CRITTERS))
                {
                    critter.AIRemoveFromShitlist(pc);
                    if (flag == 1)
                    {
                        critter.ClearNpcFlag(NpcFlag.KOS);
                    }
                }

                obj.AIRemoveFromShitlist(pc);
            }
        }

        public static float group_challenge_rating()
        {
            return (Utilities.group_average_level(SelectedPartyLeader) * (GameSystems.Party.PartySize / 4f));
        }

        // Util functions for getting & setting words, bytes and nibbles in object integers.
        // object = reference to the object containing the integer variable.
        // var	 = the variable to be used. e.g. obj_f_weapon_pad_i_2
        // idx	 = the index of the word (0-1), byte (0-3) or nibble (0-7) to use.
        // val	 = the value to be set.

        public static int getObjVarDWord(GameObjectBody obj, obj_f var)
        {
            return obj.GetInt32(var);
        }
        public static void setObjVarDWord(GameObjectBody obj, obj_f var, int val)
        {
            obj.SetInt32(var, val);
        }
        public static int getObjVarWord(GameObjectBody obj, obj_f var, int idx)
        {
            return getObjVar(obj, var, idx, 65535, 16);
        }
        public static void setObjVarWord(GameObjectBody obj, obj_f var, int idx, int val)
        {
            setObjVar(obj, var, idx, val, 65535, 16);
        }
        public static int getObjVarByte(GameObjectBody obj, obj_f var, int idx)
        {
            return getObjVar(obj, var, idx, 255, 8);
        }
        public static void setObjVarByte(GameObjectBody obj, obj_f var, int idx, int val)
        {
            setObjVar(obj, var, idx, val, 255, 8);
        }
        public static int getObjVarNibble(GameObjectBody obj, obj_f var, int idx)
        {
            return getObjVar(obj, var, idx, 15, 4);
        }
        public static void setObjVarNibble(GameObjectBody obj, obj_f var, int idx, int val)
        {
            setObjVar(obj, var, idx, val, 15, 4);
        }
        public static int getObjVar(GameObjectBody obj, obj_f var, int idx, int mask, int bits)
        {
            var bitMask = mask << (idx * bits);
            var val = obj.GetInt32(var) & bitMask;
            val = val >> (idx * bits);
            return (val & mask);
        }
        public static void setObjVar(GameObjectBody obj, obj_f var, int idx, int val, int mask, int bits)
        {
            // print "obj=", object, " var=", var, " idx=", idx, " val=", val
            var bitMask = mask << (idx * bits);
            val = val << (idx * bits);
            var oldVal = obj.GetInt32(var) & ~bitMask;
            obj.SetInt32(var, oldVal | val);
        }
        // added by dolio #
        // Temporarily renders a target invincible, and deals
        // some damage (which gets reduced to 0). This allows
        // the dealer to gain experience for killing the target.

        public static void plink(GameObjectBody critter, SpellPacketBody spell)
        {
            var invuln = (critter.GetObjectFlags() & ObjectFlag.INVULNERABLE) != 0;
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

        public static void disintegrate_critter(bool ensure_exp, GameObjectBody critter, SpellPacketBody spell)
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

        public static void config_override_bits(int var_no, IList<int> bit_range, bool new_value)
        {
            var bit_maskk = 0;
            foreach (var diggitt in bit_range)
            {
                bit_maskk += 1 << diggitt;
            }

            var new_bit = new_value ? 1 : 0;
            SetGlobalVar(var_no, GetGlobalVar(var_no) ^ (bit_maskk & (GetGlobalVar(var_no) ^ (new_bit << bit_range[0]))));
        }

        public static void get_Co8_options_from_ini()
        {
            try
            {
                var i_file_lines = File.ReadLines("Co8_config.ini"); // opens the file in ToEE main folder now

                foreach (var line in i_file_lines)
                {
                    var s2 = line.Split("{");
                    if (s2.Length >= 3)
                    {
                        // check if it's an actual entry line with the format:
                        // { Param name } { value } {description (this bracket is optional!) }
                        // will return an array ['','[Param name text]} ','[value]}, [Description text]}'] entry
                        var param_name = s2[1].Replace("}", "").Trim();
                        var par_lower = param_name.ToLowerInvariant();
                        var param_value = s2[2].Replace("}", "").Trim();
                        if (int.TryParse(param_value, out var param_value_int))
                        {
                            if (par_lower == "Party_Run_Speed".ToLowerInvariant())
                            {
                                Co8Settings.PartyRunSpeed = param_value_int;
                            }
                            else if (par_lower == "Disable_New_Plots".ToLowerInvariant())
                            {
                                Co8Settings.DisableNewPlots = param_value_int != 0;
                            }
                            else if (par_lower == "Disable_Target_Of_Revenge".ToLowerInvariant())
                            {
                                Co8Settings.DisableTargetOfRevenge = param_value_int != 0;
                            }
                            else if (par_lower == "Disable_Moathouse_Ambush".ToLowerInvariant())
                            {
                                Co8Settings.DisableMoathouseAmbush= param_value_int != 0;
                            }
                            else if (par_lower == "Disable_Arena_Of_Heroes".ToLowerInvariant())
                            {
                                Co8Settings.DisableArenaOfHeroes = param_value_int != 0;
                            }
                            else if (par_lower == "Disable_Reactive_Temple".ToLowerInvariant())
                            {
                                Co8Settings.DisableReactiveTemple = param_value_int != 0;
                            }
                            else if (par_lower == "Disable_Screng_Recruit_Special".ToLowerInvariant())
                            {
                                Co8Settings.DisableScrengRecruitSpecial = param_value_int != 0;
                            }
                            else if (par_lower == "Entangle_Outdoors_Only".ToLowerInvariant())
                            {
                                Co8Settings.EntangleOutdoorsOnly = param_value_int != 0;
                            }
                            else if (par_lower == "Elemental_Spells_At_Elemental_Nodes".ToLowerInvariant())
                            {
                                Co8Settings.ElementalSpellsAtElementalNodes = param_value_int != 0;
                            }
                            else if (par_lower == "Charm_Spell_DC_Modifier".ToLowerInvariant())
                            {
                                Co8Settings.CharmSpellDCModifier = param_value_int != 0;
                            }
                            else if (par_lower == "AI_Ignore_Summons".ToLowerInvariant())
                            {
                                Co8Settings.AIIgnoreSummons = param_value_int != 0;
                            }
                            else if (par_lower == "AI_Ignore_Spiritual_Weapons".ToLowerInvariant())
                            {
                                Co8Settings.AIIgnoreSpiritualWeapons = param_value_int != 0;
                            }
                            else if (par_lower == "Random_Encounter_XP_Reduction".ToLowerInvariant())
                            {
                                Co8Settings.RandomEncounterXPReduction = param_value_int;
                            }
                            else if (par_lower == "Stinking_Cloud_Duration_Nerf".ToLowerInvariant())
                            {
                                Co8Settings.StinkingCloudDurationNerf = param_value_int != 0;
                            }
                        }

                    }

                }

                if (Co8Settings.PartyRunSpeed != 0)
                {
                    Batch.speedup(Co8Settings.PartyRunSpeed, Co8Settings.PartyRunSpeed);
                }
            }
            catch (Exception e)
            {
                return;
            }

            return;
        }

    }
}
