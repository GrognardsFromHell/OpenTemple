
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
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
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts;

public class Scripts
{
    public static void npc_1(GameObject attachee)
    {
        attachee.SetInt(obj_f.npc_pad_i_3, 1);
        return;
    }
    public static void npc_2(GameObject attachee)
    {
        attachee.SetInt(obj_f.npc_pad_i_4, 1);
        return;
    }
    public static void npc_3(GameObject attachee)
    {
        attachee.SetInt(obj_f.npc_pad_i_5, 1);
        return;
    }
    public static void npc_1_undo(GameObject attachee)
    {
        attachee.SetInt(obj_f.npc_pad_i_3, 0);
        return;
    }
    public static void npc_2_undo(GameObject attachee)
    {
        attachee.SetInt(obj_f.npc_pad_i_4, 0);
        return;
    }
    public static void npc_3_undo(GameObject attachee)
    {
        attachee.SetInt(obj_f.npc_pad_i_5, 0);
        return;
    }
    public static bool get_1(GameObject attachee)
    {
        var x1 = attachee.GetInt(obj_f.npc_pad_i_3);
        if (x1 != 0)
        {
            return true;
        }

        return false;
    }
    public static bool get_2(GameObject attachee)
    {
        var x2 = attachee.GetInt(obj_f.npc_pad_i_4);
        if (x2 != 0)
        {
            return true;
        }

        return false;
    }
    public static bool get_3(GameObject attachee)
    {
        var x3 = attachee.GetInt(obj_f.npc_pad_i_5);
        if (x3 != 0)
        {
            return true;
        }

        return false;
    }
    public static void oflag_1(GameObject attachee)
    {
        attachee.SetInt(obj_f.item_pad_i_1, 1);
        return;
    }
    public static bool get_of1(GameObject attachee)
    {
        var x4 = attachee.GetInt(obj_f.item_pad_i_1);
        if (x4 != 0)
        {
            return true;
        }

        return false;
    }
    public static void npcvar_1(GameObject attachee, int var)
    {
        attachee.SetInt(obj_f.npc_pad_i_3, var);
        return;
    }
    public static void npcvar_2(GameObject attachee, int var)
    {
        attachee.SetInt(obj_f.npc_pad_i_4, var);
        return;
    }
    public static void npcvar_3(GameObject attachee, int var)
    {
        attachee.SetInt(obj_f.npc_pad_i_5, var);
        return;
    }
    public static int getvar_1(GameObject attachee)
    {
        var x1 = attachee.GetInt(obj_f.npc_pad_i_3);
        if (x1 != 0)
        {
            return x1;
        }

        return 0;
    }
    public static int getvar_2(GameObject attachee)
    {
        var x2 = attachee.GetInt(obj_f.npc_pad_i_4);
        if (x2 != 0)
        {
            return x2;
        }

        return 0;
    }
    public static int getvar_3(GameObject attachee)
    {
        var x3 = attachee.GetInt(obj_f.npc_pad_i_5);
        if (x3 != 0)
        {
            return x3;
        }

        return 0;
    }
    public static bool bluff(GameObject attachee, GameObject triggerer, int dc)
    {
        var x = RandomRange(1, 20);
        var y = x + triggerer.GetSkillLevel(attachee, SkillId.bluff);
        if (y >= dc)
        {
            return true;
        }

        return false;
    }
    public static bool dipl(GameObject attachee, GameObject triggerer, int dc)
    {
        var a = RandomRange(1, 20);
        var b = a + triggerer.GetSkillLevel(attachee, SkillId.diplomacy);
        if (b >= dc)
        {
            return true;
        }

        return false;
    }
    public static bool intim(GameObject attachee, GameObject triggerer, int dc)
    {
        var c = RandomRange(1, 20);
        var d = c + triggerer.GetSkillLevel(attachee, SkillId.intimidate);
        if (d >= dc)
        {
            return true;
        }

        return false;
    }
    public static bool info(GameObject attachee, GameObject triggerer, int dc)
    {
        var e = RandomRange(1, 20);
        var f = e + triggerer.GetSkillLevel(attachee, SkillId.gather_information);
        if (f >= dc)
        {
            return true;
        }

        return false;
    }
    public static bool sense(GameObject attachee, GameObject triggerer, int dc)
    {
        var g = RandomRange(1, 20);
        var h = g + triggerer.GetSkillLevel(attachee, SkillId.sense_motive);
        if (h >= dc)
        {
            return true;
        }

        return false;
    }
    public static void away(GameObject attachee)
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
    public static bool pc_only(GameObject attachee, GameObject triggerer)
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
            return false;
        }

        return true;
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
    public static void sense_roll(GameObject attachee, GameObject triggerer, int dc, int ayup, int nope)
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
    public static void intim_roll(GameObject attachee, GameObject triggerer, int dc, int ayup, int nope)
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
    public static void bluff_roll(GameObject attachee, GameObject triggerer, int dc, int ayup, int nope)
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
    public static void dipl_roll(GameObject attachee, GameObject triggerer, int dc, int ayup, int nope)
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
    public static void gath_roll(GameObject attachee, GameObject triggerer, int dc, int ayup, int nope)
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
        return GameSystems.Party.AiFollowerCount;
    }
    public static void drop_all(GameObject target)
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
    public static bool nothing(GameObject attachee)
    {
        if (attachee.IsMonsterCategory(MonsterCategory.humanoid))
        {
            attachee.WieldBestInAllSlots();
            var weap = attachee.ItemWornAt(EquipSlot.WeaponPrimary);
            if (weap == null)
            {
                return true;
            }
            else
            {
                attachee.ClearCritterFlag(CritterFlag.UNUSED_00000400);
            }

        }

        return false;
    }
    public static void get_something(GameObject attachee)
    {
        if (Utilities.critter_is_unconscious(attachee) || attachee.HasCondition(SpellEffects.SpellSoundBurst) || attachee.D20Query(D20DispatcherKey.QUE_Critter_Is_Stunned))
        {
            attachee.FloatMesFileLine("mes/spell.mes", 20021);
            return;
        }

        // game.particles( "sp-summon monster I", game.party[0] )
        foreach (var weap in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_WEAPON))
        {
            if (weap != null)
            {
                var sizeCat = (SizeCategory) attachee.GetInt(obj_f.size);
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
    public static int get_lvl(GameObject attachee)
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
    public static bool i_should_run(GameObject attachee)
    {
        var p = 0;
        var q = 0;
        foreach (var co_com in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
        {
            if ((co_com.IsFriendly(attachee) && (Utilities.critter_is_unconscious(co_com) || co_com.GetStat(Stat.hp_current) < -8)))
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
            return true;
        }

        return false;
    }
    public static void run_away(GameObject attachee)
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
    public static bool speaks_drac(GameObject speaker)
    {
        if (speaker.GetStat(Stat.level_wizard) >= 1)
        {
            return true;
        }
        else if (speaker.GetRace() == RaceId.svirfneblin || speaker.GetRace() == RaceId.half_orc || speaker.GetRace() == RaceId.aquatic_elf)
        {
            return true;
        }
        else if (((speaker.GetRace() == RaceId.tallfellow && speaker.GetSkillLevel(SkillId.decipher_script) >= 3) || (speaker.GetRace() == RaceId.halfelf && speaker.GetSkillLevel(SkillId.decipher_script) >= 5) || (speaker.GetRace() == RaceId.derro && speaker.GetSkillLevel(SkillId.decipher_script) >= 2) || (speaker.GetRace() == RaceId.human && speaker.GetSkillLevel(SkillId.decipher_script) >= 6)))
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    public static bool speaks_goblin(GameObject speaker)
    {
        if ((speaker.GetRace() == RaceId.halfelf && speaker.GetSkillLevel(SkillId.decipher_script) < 1) || (speaker.GetRace() == RaceId.human && speaker.GetSkillLevel(SkillId.decipher_script) < 2))
        {
            return false;
        }
        else
        {
            return true;
        }

    }
    public static bool speaks_orc(GameObject speaker)
    {
        if ((speaker.GetRace() == RaceId.halfelf && speaker.GetSkillLevel(SkillId.decipher_script) < 2) || (speaker.GetRace() == RaceId.human && speaker.GetSkillLevel(SkillId.decipher_script) < 3))
        {
            return false;
        }
        else
        {
            return true;
        }

    }
    public static bool bard_know(GameObject triggerer, int dc)
    {
        var take20 = RandomRange(1, 20);
        var lev = triggerer.GetStat(Stat.level_bard);
        lev = lev + triggerer.GetStat(Stat.int_mod);
        lev = lev + take20;
        if (lev >= dc)
        {
            return true;
        }
        else
        {
            return false;
        }

    }
    public static int is_married(GameObject obj, int who_is_spouse__option)
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
        var marriage_array = new List<int>();
        foreach (var npc in GameSystems.Party.PartyMembers)
        {
            if (npc.GetLeader() == obj && (new[] { 8015, 8020, 8057, 8067 }).Contains(npc.GetNameId()))
            {
                // 8015 - Meleny
                // 8020 - Bertram
                // 8057 - Bertram (straight version)
                // 8067 - Fruella
                marriage_array.Add(npc.GetNameId());
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