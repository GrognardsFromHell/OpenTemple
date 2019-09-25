
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

    public class Scripts
    {
        public static void npc_1(GameObjectBody attachee)
        {
            attachee.SetInt(obj_f.npc_pad_i_3, 1);
            return;
        }
        public static void npc_2(GameObjectBody attachee)
        {
            attachee.SetInt(obj_f.npc_pad_i_4, 1);
            return;
        }
        public static void npc_3(GameObjectBody attachee)
        {
            attachee.SetInt(obj_f.npc_pad_i_5, 1);
            return;
        }
        public static void npc_1_undo(GameObjectBody attachee)
        {
            attachee.SetInt(obj_f.npc_pad_i_3, 0);
            return;
        }
        public static void npc_2_undo(GameObjectBody attachee)
        {
            attachee.SetInt(obj_f.npc_pad_i_4, 0);
            return;
        }
        public static void npc_3_undo(GameObjectBody attachee)
        {
            attachee.SetInt(obj_f.npc_pad_i_5, 0);
            return;
        }
        public static void get_1(GameObjectBody attachee)
        {
            var x1 = attachee.GetInt(obj_f.npc_pad_i_3);
            if (x1 != 0)
            {
                return 1;
            }

            return;
        }
        public static void get_2(GameObjectBody attachee)
        {
            var x2 = attachee.GetInt(obj_f.npc_pad_i_4);
            if (x2 != 0)
            {
                return 1;
            }

            return;
        }
        public static void get_3(GameObjectBody attachee)
        {
            var x3 = attachee.GetInt(obj_f.npc_pad_i_5);
            if (x3 != 0)
            {
                return 1;
            }

            return;
        }
        public static void oflag_1(GameObjectBody attachee)
        {
            attachee.SetInt(obj_f.item_pad_i_1, 1);
            return;
        }
        public static void get_of1(GameObjectBody attachee)
        {
            var x4 = attachee.SetInt(obj_f.item_pad_i_1);
            if (x4 != 0)
            {
                return 1;
            }

            return;
        }
        public static void npcvar_1(GameObjectBody attachee, FIXME var)
        {
            attachee.SetInt(obj_f.npc_pad_i_3, var);
            return;
        }
        public static void npcvar_2(GameObjectBody attachee, FIXME var)
        {
            attachee.SetInt(obj_f.npc_pad_i_4, var);
            return;
        }
        public static void npcvar_3(GameObjectBody attachee, FIXME var)
        {
            attachee.SetInt(obj_f.npc_pad_i_5, var);
            return;
        }
        public static int getvar_1(GameObjectBody attachee)
        {
            var x1 = attachee.GetInt(obj_f.npc_pad_i_3);
            if (x1 != 0)
            {
                return x1;
            }

            return 0;
        }
        public static int getvar_2(GameObjectBody attachee)
        {
            var x2 = attachee.GetInt(obj_f.npc_pad_i_4);
            if (x2 != 0)
            {
                return x2;
            }

            return 0;
        }
        public static int getvar_3(GameObjectBody attachee)
        {
            var x3 = attachee.GetInt(obj_f.npc_pad_i_5);
            if (x3 != 0)
            {
                return x3;
            }

            return 0;
        }
        public static int bluff(GameObjectBody attachee, GameObjectBody triggerer, int dc)
        {
            var x = RandomRange(1, 20);
            var y = x + triggerer.GetSkillLevel(attachee, SkillId.bluff);
            if (y >= dc)
            {
                return 1;
            }

            return 0;
        }
        public static int dipl(GameObjectBody attachee, GameObjectBody triggerer, int dc)
        {
            var a = RandomRange(1, 20);
            var b = a + triggerer.GetSkillLevel(attachee, SkillId.diplomacy);
            if (b >= dc)
            {
                return 1;
            }

            return 0;
        }
        public static int intim(GameObjectBody attachee, GameObjectBody triggerer, int dc)
        {
            var c = RandomRange(1, 20);
            var d = c + triggerer.GetSkillLevel(attachee, SkillId.intimidate);
            if (d >= dc)
            {
                return 1;
            }

            return 0;
        }
        public static int info(GameObjectBody attachee, GameObjectBody triggerer, int dc)
        {
            var e = RandomRange(1, 20);
            var f = e + triggerer.GetSkillLevel(attachee, SkillId.gather_information);
            if (f >= dc)
            {
                return 1;
            }

            return 0;
        }
        public static int sense(GameObjectBody attachee, GameObjectBody triggerer, int dc)
        {
            var g = RandomRange(1, 20);
            var h = g + triggerer.GetSkillLevel(attachee, SkillId.sense_motive);
            if (h >= dc)
            {
                return 1;
            }

            return 0;
        }
        public static void away(GameObjectBody attachee)
        {
            var x = attachee.Rotation;
            x = x + 3.1416f;
            if (x >= 6.28f)
            {
                x = x - 6.27f;
            }

            attachee.Rotation = x;
            return;
        }
        public static int pc_only(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var d = 0;
            foreach (var mem in GameSystems.Party.PartyMembers)
            {
                if (mem.type == ObjectType.npc)
                {
                    d = 1;
                }

            }

            if (d != 0)
            {
                return 0;
            }

            return 1;
        }
        public static int next_slot(FIXME wearer, FIXME slot)
        {
            slot = 35;
            var full = wearer.item_worn_at/*Unknown*/(slot);
            while (full != null)
            {
                slot = slot + 1;
            }

            return slot;
        }
        public static void give_a_ton()
        {
            foreach (var obj in SelectedPartyLeader.GetPartyMembers())
            {
                var curxp = obj.GetStat(Stat.experience);
                var newxp = curxp + 100;
                obj.SetBaseStat(Stat.experience, newxp);
                obj.FloatMesFileLine("mes/narrative.mes", 1010);
            }

            return;
        }
        public static void sense_roll(GameObjectBody attachee, GameObjectBody triggerer, int dc, FIXME ayup, FIXME nope)
        {
            var i = RandomRange(1, 20);
            var j = i + triggerer.GetSkillLevel(attachee, SkillId.sense_motive);
            if (j >= dc)
            {
                triggerer.BeginDialog(attachee, ayup);
            }
            else
            {
                triggerer.BeginDialog(attachee, nope);
            }

            return;
        }
        public static void intim_roll(GameObjectBody attachee, GameObjectBody triggerer, int dc, FIXME ayup, FIXME nope)
        {
            var k = RandomRange(1, 20);
            var l = k + triggerer.GetSkillLevel(attachee, SkillId.intimidate);
            if (l >= dc)
            {
                triggerer.BeginDialog(attachee, ayup);
            }
            else
            {
                triggerer.BeginDialog(attachee, nope);
            }

            return;
        }
        public static void bluff_roll(GameObjectBody attachee, GameObjectBody triggerer, int dc, FIXME ayup, FIXME nope)
        {
            var m = RandomRange(1, 20);
            var n = m + triggerer.GetSkillLevel(attachee, SkillId.bluff);
            if (n >= dc)
            {
                triggerer.BeginDialog(attachee, ayup);
            }
            else
            {
                triggerer.BeginDialog(attachee, nope);
            }

            return;
        }
        public static void dipl_roll(GameObjectBody attachee, GameObjectBody triggerer, int dc, FIXME ayup, FIXME nope)
        {
            var o = RandomRange(1, 20);
            var p = o + triggerer.GetSkillLevel(attachee, SkillId.diplomacy);
            if (p >= dc)
            {
                triggerer.BeginDialog(attachee, ayup);
            }
            else
            {
                triggerer.BeginDialog(attachee, nope);
            }

            return;
        }
        public static void gath_roll(GameObjectBody attachee, GameObjectBody triggerer, int dc, FIXME ayup, FIXME nope)
        {
            var q = RandomRange(1, 20);
            var r = q + triggerer.GetSkillLevel(attachee, SkillId.gather_information);
            if (r >= dc)
            {
                triggerer.BeginDialog(attachee, ayup);
            }
            else
            {
                triggerer.BeginDialog(attachee, nope);
            }

            return;
        }
        public static int how_many_ai()
        {
            var foll = GameSystems.Party.PartyMembers.Count;
            var aifoll = SelectedPartyLeader.GetPartyMembers().Count;
            return aifoll - foll;
        }
        public static void drop_all(GameObjectBody target)
        {
            var item = target.ItemWornAt(EquipSlot.WeaponPrimary);
            if (item != null)
            {
                var basil3 = GameSystems.MapObject.CreateObject(item.GetNameId(), target.GetLocation());
                basil3.Rotation = 2.3f;
                item.Destroy();
                target.SetCritterFlag(CritterFlag.UNUSED_00000400);
            }

            var item2 = target.ItemWornAt(EquipSlot.WeaponSecondary);
            if (item2 != null)
            {
                var basil1 = GameSystems.MapObject.CreateObject(item2.GetNameId(), target.GetLocation());
                basil1.Rotation = 2.3f;
                item2.Destroy();
            }

            var item3 = target.ItemWornAt(EquipSlot.Shield);
            if (item3 != null)
            {
                var basil2 = GameSystems.MapObject.CreateObject(item3.GetNameId(), target.GetLocation());
                basil2.Rotation = 2.3f;
                item3.Destroy();
            }

            return;
        }
        public static int nothing(GameObjectBody attachee)
        {
            if (attachee.IsMonsterCategory(MonsterCategory.humanoid))
            {
                attachee.WieldBestInAllSlots();
                var weap = attachee.ItemWornAt(EquipSlot.WeaponPrimary);
                if (weap == null)
                {
                    return 1;
                }
                else
                {
                    attachee.ClearCritterFlag(CritterFlag.UNUSED_00000400);
                }

            }

            return 0;
        }
        public static void get_something(GameObjectBody attachee)
        {
            if (Utilities.critter_is_unconscious(attachee) == 1 || attachee.HasCondition(SpellEffects.SpellSoundBurst) || attachee.D20Query(D20DispatcherKey.QUE_Critter_Is_Stunned))
            {
                attachee.FloatMesFileLine("mes/spell.mes", 20021);
                return;
            }

            // game.particles( "sp-summon monster I", game.party[0] )
            foreach (var weap in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_WEAPON))
            {
                if (weap != null)
                {
                    var sizeCat = attachee.GetInt(obj_f.size);
                    if (sizeCat <= SizeCategory.Medium && attachee.DistanceTo(weap) <= 10)
                    {
                        attachee.GetItem(weap);
                        attachee.WieldBestInAllSlots();
                    }
                    else if (sizeCat > SizeCategory.Medium && attachee.DistanceTo(weap) <= 17)
                    {
                        attachee.GetItem(weap);
                        attachee.WieldBestInAllSlots();
                    }

                }

            }

            if (nothing(attachee))
            {
                Livonya.get_melee_strategy(attachee);
                return;
            }
            else
            {
                attachee.ClearCritterFlag(CritterFlag.UNUSED_00000400);
                if (attachee.HasFeat(FeatId.QUICK_DRAW))
                {
                    Livonya.get_melee_strategy(attachee);
                    attachee.FloatMesFileLine("mes/skill_ui.mes", 199);
                }
                else if (attachee.HasFeat(FeatId.COMBAT_EXPERTISE))
                {
                    attachee.SetInt(obj_f.critter_strategy, 54);
                    attachee.FloatMesFileLine("mes/skill_ui.mes", 654);
                }
                else
                {
                    attachee.SetInt(obj_f.critter_strategy, 55);
                    attachee.FloatMesFileLine("mes/skill_ui.mes", 655);
                }

            }

            return;
        }
        public static int get_lvl(GameObjectBody attachee)
        {
            var lvl = 0;
            var la = attachee.GetStat(Stat.level_cleric);
            var lb = attachee.GetStat(Stat.level_paladin);
            var lc = attachee.GetStat(Stat.level_ranger);
            var ld = attachee.GetStat(Stat.level_druid);
            var le = attachee.GetStat(Stat.level_barbarian);
            var lf = attachee.GetStat(Stat.level_monk);
            var lg = attachee.GetStat(Stat.level_rogue);
            var li = attachee.GetStat(Stat.level_bard);
            var lj = attachee.GetStat(Stat.level_wizard);
            var lk = attachee.GetStat(Stat.level_sorcerer);
            var lh = attachee.GetStat(Stat.level_fighter);
            lvl = (la + lb + lc + ld + le + lf + lg + lh + li + lj + lk);
            return lvl;
        }
        public static int i_should_run(GameObjectBody attachee)
        {
            var p = 0;
            var q = 0;
            foreach (var co_com in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((co_com.IsFriendly(attachee) && (Utilities.critter_is_unconscious(co_com) == 1 || co_com.GetStat(Stat.hp_current) < -8)))
                {
                    p = p + 1;
                }
                else
                {
                    p = p - 1;
                }

            }

            q = RandomRange(1, 8);
            q = q + 2;
            if (p > q)
            {
                return 1;
            }

            return 0;
        }
        public static void run_away(GameObjectBody attachee)
        {
            attachee.AddCondition("sp-Fear", 0, 100, 0);
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 30);
            }

            attachee.ClearNpcFlag(NpcFlag.KOS);
            attachee.SetNpcFlag(NpcFlag.KOS_OVERRIDE);
            attachee.RemoveFromInitiative();
            UiSystems.Combat.Initiative.UpdateIfNeeded();
            return;
        }
        public static int speaks_drac(GameObjectBody speaker)
        {
            if (speaker.GetStat(Stat.level_wizard) >= 1)
            {
                return 1;
            }
            else if (speaker.GetRace() == RaceId.svirfneblin || speaker.GetRace() == RaceId.half_orc || speaker.GetRace() == RaceId.aquatic_elf)
            {
                return 1;
            }
            else if (((speaker.GetRace() == RaceId.tallfellow && speaker.GetSkillLevel(SkillId.decipher_script) >= 3) || (speaker.GetRace() == RaceId.halfelf && speaker.GetSkillLevel(SkillId.decipher_script) >= 5) || (speaker.GetRace() == RaceId.derro && speaker.GetSkillLevel(SkillId.decipher_script) >= 2) || (speaker.GetRace() == RaceId.human && speaker.GetSkillLevel(SkillId.decipher_script) >= 6)))
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }
        public static int speaks_goblin(GameObjectBody speaker)
        {
            if ((speaker.GetRace() == RaceId.halfelf && speaker.GetSkillLevel(SkillId.decipher_script) < 1) || (speaker.GetRace() == RaceId.human && speaker.GetSkillLevel(SkillId.decipher_script) < 2))
            {
                return 0;
            }
            else
            {
                return 1;
            }

        }
        public static int speaks_orc(GameObjectBody speaker)
        {
            if ((speaker.GetRace() == RaceId.halfelf && speaker.GetSkillLevel(SkillId.decipher_script) < 2) || (speaker.GetRace() == RaceId.human && speaker.GetSkillLevel(SkillId.decipher_script) < 3))
            {
                return 0;
            }
            else
            {
                return 1;
            }

        }
        public static int bard_know(GameObjectBody triggerer, int dc)
        {
            var take20 = RandomRange(1, 20);
            var lev = triggerer.GetStat(Stat.level_bard);
            lev = lev + triggerer.GetStat(Stat.int_mod);
            lev = lev + take20;
            if (lev >= dc)
            {
                return 1;
            }
            else
            {
                return 0;
            }

        }
        public static int is_married(GameObjectBody obj, FIXME who_is_spouse__option)
        {
            // DESCRIPTION:
            // This function determines whether obj is married.
            // Optionally, it also returns the spouse's name ID. (If there is more than 1 spouse, it returns 999)
            // Inputs:
            // obj - should be a PC's object handle
            // who_is_spouse__option - by default is -1
            // if you wish to return the name ID of the spouse (instead of 0 or 1), change this variable to something else
            // e.g. if obj is married to Fruella, it will return her name ID - 8067
            // if obj is married to more than one person, it will return 999 :)
            // Example usage in dialogue:
            // {11}{Sorry ma'am, I'm already married.}{}{1}{is_married(pc, 1) != 0 and is_married(pc, 1) != 8067 and is_married(pc, 1) != 999}{20}{}
            // {12}{I would never betray my beloved Fruella!}{}{1}{is_married(pc, 1) == 8067}{40}{}
            // {13}{I'm gonna fuck all y'all! Yeehaw!}{}{1}{is_married(pc, 1) == 999}{60}{}
            var marriage_array = new List<GameObjectBody>();
            foreach (var npc in GameSystems.Party.PartyMembers)
            {
                if (npc.GetLeader() == obj && (new[] { 8015, 8020, 8057, 8067 }).Contains(npc.GetNameId()))
                {
                    // 8015 - Meleny
                    // 8020 - Bertram
                    // 8057 - Bertram (straight version)
                    // 8067 - Fruella
                    marriage_array += new[] { npc.GetNameId() };
                }

            }

            if (marriage_array.Count == 0)
            {
                return 0;
            }
            else if (who_is_spouse__option == -1)
            {
                return 1;
            }
            else if (marriage_array.Count == 1)
            {
                return marriage_array[0];
            }
            else
            {
                return 999;
            }

            return 0;
        }

    }
}
