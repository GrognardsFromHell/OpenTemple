using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Utils;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;

namespace OpenTemple.Core.Systems.D20.Conditions;

[AutoRegister]
public static class RaceConditions
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    [TempleDllLocation(0x102ef310)]
    public static readonly ConditionSpec Human = ConditionSpec.Create("Human", 0)
        .SetUnique()
        .AddHandler(DispatcherType.GetMoveSpeedBase, RacialMoveSpeed30)
        .AddQueryHandler(D20DispatcherKey.QUE_FavoredClass, HighestClassIsFavoredClass)
        .Build();


    [TempleDllLocation(0x102ef368)]
    public static readonly ConditionSpec Dwarf = ConditionSpec.Create("Dwarf", 0)
        .SetUnique()
        .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CONSTITUTION,
            RacialStatModifier_callback, 2)
        .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CHARISMA, RacialStatModifier_callback,
            -2)
        .AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_CONSTITUTION, RacialStatModifier_callback, 2)
        .AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_CHARISMA, RacialStatModifier_callback, -2)
        .AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_FORTITUDE,
            ApplyDwarvenSaveBonusAgainstPoison)
        .AddHandler(DispatcherType.SaveThrowLevel, DwarfSaveBonusVsSpells)
        .AddHandler(DispatcherType.ToHitBonus2, DwarfBonusToHitOrcsAndGoblins)
        .AddHandler(DispatcherType.GetAC, ArmorBonusVsGiants)
        .AddSkillLevelHandler(SkillId.appraise, DwarfAppraiseBonus, 2)
        .AddHandler(DispatcherType.GetMoveSpeedBase, BaseMoveSpeed20)
        .AddHandler(DispatcherType.GetMoveSpeed, DwarfMoveSpeed, 20, 139)
        .AddHandler(DispatcherType.AbilityCheckModifier, CommonConditionCallbacks.AbilityModCheckStabilityBonus)
        .AddQueryHandler(D20DispatcherKey.QUE_FavoredClass, CommonConditionCallbacks.D20QueryConditionHasHandler,
            11)
        .Build();


    [TempleDllLocation(0x102ef4a0)]
    public static readonly ConditionSpec Elf = ConditionSpec.Create("Elf", 0)
        .SetUnique()
        .AddHandler(DispatcherType.ConditionAddPre, ElvenConditionImmunity, SpellEffects.SpellSleep)
        .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_DEXTERITY, RacialStatModifier_callback,
            2)
        .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CONSTITUTION,
            RacialStatModifier_callback, -2)
        .AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_DEXTERITY, RacialStatModifier_callback, 2)
        .AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_CONSTITUTION, RacialStatModifier_callback, -2)
        .AddHandler(DispatcherType.SaveThrowLevel, ElfSavingThrowBonus)
        .AddSkillLevelHandler(SkillId.listen, SkillBonusRacial, 2)
        .AddSkillLevelHandler(SkillId.search, SkillBonusRacial, 2)
        .AddSkillLevelHandler(SkillId.spot, SkillBonusRacial, 2)
        .AddHandler(DispatcherType.GetMoveSpeedBase, RacialMoveSpeed30)
        .AddQueryHandler(D20DispatcherKey.QUE_FavoredClass, CommonConditionCallbacks.D20QueryConditionHasHandler,
            17)
        .Build();


    [TempleDllLocation(0x102ef5b0)]
    public static readonly ConditionSpec Gnome = ConditionSpec.Create("Gnome", 0)
        .SetUnique()
        .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CONSTITUTION,
            RacialStatModifier_callback, 2)
        .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_STRENGTH, RacialStatModifier_callback,
            -2)
        .AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_CONSTITUTION, RacialStatModifier_callback, 2)
        .AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_STRENGTH, RacialStatModifier_callback, -2)
        .AddHandler(DispatcherType.SaveThrowLevel, GnomeIllusionSaveBonus)
        .AddHandler(DispatcherType.SpellDcMod, SpellDcMod_GnomeIllusionBonus_Callback)
        .AddHandler(DispatcherType.GetAC, ArmorBonusVsGiants)
        .AddSkillLevelHandler(SkillId.hide, RacialHideBonus)
        .AddSkillLevelHandler(SkillId.listen, SkillBonusRacial, 2)
        .AddHandler(DispatcherType.ToHitBonus2, GnomeToHitBonusAgainstGoblins)
        .AddHandler(DispatcherType.GetMoveSpeedBase, BaseMoveSpeed20)
        .AddQueryHandler(D20DispatcherKey.QUE_FavoredClass, CommonConditionCallbacks.D20QueryConditionHasHandler, 8)
        .Build();


    [TempleDllLocation(0x102ef6d0)]
    public static readonly ConditionSpec Halfelf = ConditionSpec.Create("Halfelf", 0)
        .SetUnique()
        .AddHandler(DispatcherType.ConditionAddPre, ElvenConditionImmunity, SpellEffects.SpellSleep)
        .AddHandler(DispatcherType.SaveThrowLevel, ElfSavingThrowBonus)
        .AddSkillLevelHandler(SkillId.listen, SkillBonusRacial, 1)
        .AddSkillLevelHandler(SkillId.search, SkillBonusRacial, 1)
        .AddSkillLevelHandler(SkillId.spot, SkillBonusRacial, 1)
        .AddSkillLevelHandler(SkillId.diplomacy, SkillBonusRacial, 2)
        .AddSkillLevelHandler(SkillId.gather_information, SkillBonusRacial, 2)
        .AddHandler(DispatcherType.GetMoveSpeedBase, RacialMoveSpeed30)
        .AddQueryHandler(D20DispatcherKey.QUE_FavoredClass, HighestClassIsFavoredClass)
        .Build();


    [TempleDllLocation(0x102ef7b8)]
    public static readonly ConditionSpec Halforc = ConditionSpec.Create("Halforc", 0)
        .SetUnique()
        .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_STRENGTH, RacialStatModifier_callback,
            2)
        .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_INTELLIGENCE,
            RacialStatModifier_callback, -2)
        .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CHARISMA, RacialStatModifier_callback,
            -2)
        .AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_STRENGTH, RacialStatModifier_callback, 2)
        .AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_INTELLIGENCE, RacialStatModifier_callback, -2)
        .AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_CHARISMA, RacialStatModifier_callback, -2)
        .AddHandler(DispatcherType.GetMoveSpeedBase, RacialMoveSpeed30)
        .AddQueryHandler(D20DispatcherKey.QUE_FavoredClass, CommonConditionCallbacks.D20QueryConditionHasHandler, 7)
        .Build();


    [TempleDllLocation(0x102ef888)]
    public static readonly ConditionSpec Halfling = ConditionSpec.Create("Halfling", 0)
        .SetUnique()
        .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_DEXTERITY, RacialStatModifier_callback,
            2)
        .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_STRENGTH, RacialStatModifier_callback,
            -2)
        .AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_DEXTERITY, RacialStatModifier_callback, 2)
        .AddHandler(DispatcherType.StatBaseGet, D20DispatcherKey.STAT_STRENGTH, RacialStatModifier_callback, -2)
        .AddSkillLevelHandler(SkillId.hide, RacialHideBonus)
        .AddHandler(DispatcherType.SaveThrowLevel, HalflingSaveBonus)
        .AddHandler(DispatcherType.SaveThrowLevel, HalflingWillSaveFear)
        .AddSkillLevelHandler(SkillId.move_silently, SkillBonusRacial, 2)
        .AddSkillLevelHandler(SkillId.listen, SkillBonusRacial, 2)
        .AddHandler(DispatcherType.GetMoveSpeedBase, BaseMoveSpeed20)
        .AddQueryHandler(D20DispatcherKey.QUE_FavoredClass, CommonConditionCallbacks.D20QueryConditionHasHandler,
            15)
        .AddHandler(DispatcherType.ToHitBonus2, HalflingThrownWeaponBonus)
        .Build();


    [TempleDllLocation(0x102ef9a8)]
    public static readonly ConditionSpec MonsterUndead = ConditionSpec.Create("Monster Undead", 0)
        .SetUnique()
        .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_RACIAL,
            CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_RACIAL)
        .AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 0, 0)
        .Prevents(StatusEffects.Poisoned)
        .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Poison, true)
        .Prevents(StatusEffects.Paralyzed)
        .Prevents(StatusEffects.Stunned)
        .Prevents(StatusEffects.IncubatingDisease)
        .Prevents(StatusEffects.NSDiseased)
        .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Critical_Hits, true)
        .AddHandler(DispatcherType.TakingDamage2, CommonConditionCallbacks.SubdualImmunityDamageCallback)
        .Prevents(StatusEffects.TempAbilityLoss)
        .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Energy_Drain, true)
        .AddSignalHandler(D20DispatcherKey.SIG_HP_Changed, UndeadHpChange)
        .SetQueryResult(D20DispatcherKey.QUE_Critter_Has_No_Con_Score, true)
        .Build();


    [TempleDllLocation(0x102efbe8)]
    public static readonly ConditionSpec MonsterSubtypeFire = ConditionSpec.Create("Monster Subtype Fire", 0)
        .AddHandler(DispatcherType.TakingDamage2, SubtypeFireReductionAndVulnerability, DamageType.Fire,
            DamageType.Cold)
        .Build();


    [TempleDllLocation(0x102efaf0)]
    public static readonly ConditionSpec MonsterOoze = ConditionSpec.Create("Monster Ooze", 0)
        .SetUnique()
        .AddHandler(DispatcherType.ImmunityTrigger, D20DispatcherKey.IMMUNITY_RACIAL,
            CommonConditionCallbacks.ImmunityTriggerCallback, D20DispatcherKey.IMMUNITY_RACIAL)
        .AddHandler(DispatcherType.SpellImmunityCheck, CommonConditionCallbacks.ImmunityCheckHandler, 1, 0)
        .Prevents(StatusEffects.Poisoned)
        .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Poison, true)
        .Prevents(StatusEffects.Paralyzed)
        .Prevents(StatusEffects.Stunned)
        .Prevents(StatusEffects.IncubatingDisease)
        .Prevents(StatusEffects.NSDiseased)
        .SetQueryResult(D20DispatcherKey.QUE_Critter_Is_Immune_Critical_Hits, true)
        .SetQueryResult(D20DispatcherKey.QUE_CanBeFlanked, false)
        .Build();

    [DispTypes(DispatcherType.GetMoveSpeed)]
    [TempleDllLocation(0x100efec0)]
    [TemplePlusLocation("generalfixes.cpp:76")]
    public static void DwarfMoveSpeed(in DispatcherCallbackArgs evt, int data1, int data2)
    {
        var dispIo = evt.GetDispIoMoveSpeed();

        // in case the cap has already been set (e.g. by web/entangle) - recreating the spellslinger fix
        if ((dispIo.bonlist.bonFlags & 2) != 0)
        {
            return;
        }

        dispIo.bonlist.SetOverallCap(2, data1, 0, data2);
    }

    [DispTypes(DispatcherType.StatBaseGet, DispatcherType.AbilityScoreLevel)]
    [TempleDllLocation(0x100fd850)]
    public static void RacialStatModifier_callback(in DispatcherCallbackArgs evt, int data)
    {
        // Physical attributes are not affected by the original race while polymorphed
        if (GameSystems.D20.D20Query(evt.objHndCaller, D20DispatcherKey.QUE_Polymorphed))
        {
            if (evt.dispKey == D20DispatcherKey.STAT_STRENGTH
                || evt.dispKey == D20DispatcherKey.STAT_DEXTERITY
                || evt.dispKey == D20DispatcherKey.STAT_CONSTITUTION)
            {
                return;
            }
        }

        var bonusValue = data;
        var dispIo = evt.GetDispIoBonusList();

        // Make sure the racial bonus does not reduce below 3
        var effectiveVal = bonusValue + dispIo.bonlist.OverallBonus;
        if (bonusValue < 0 && effectiveVal < 3)
        {
            bonusValue = effectiveVal - 3;
        }

        dispIo.bonlist.AddBonus(bonusValue, 0, 139);
    }


    [DispTypes(DispatcherType.SaveThrowLevel)]
    [TempleDllLocation(0x100fdc00)]
    public static void HalflingSaveBonus(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoSavingThrow();
        dispIo.bonlist.AddBonus(1, 0, 139);
    }


    [DispTypes(DispatcherType.SkillLevel)]
    [TempleDllLocation(0x100fd9f0)]
    public static void SkillBonusRacial(in DispatcherCallbackArgs evt, int data)
    {
        var dispIo = evt.GetDispIoObjBonus();
        dispIo.bonOut.AddBonus(data, 0, 139);
    }


    [DispTypes(DispatcherType.D20Signal)]
    [TempleDllLocation(0x100fddb0)]
    public static void UndeadHpChange(in DispatcherCallbackArgs evt)
    {
        if (evt.objHndCaller.GetStat(Stat.hp_current) <= 0)
        {
            GameSystems.D20.Combat.Kill(evt.objHndCaller, null);
            GameSystems.ParticleSys.CreateAtObj("sp-Destroy Undead", evt.objHndCaller);
        }
    }

    [DispTypes(DispatcherType.GetMoveSpeedBase)]
    [TempleDllLocation(0x100fdca0)]
    public static void BaseMoveSpeed20(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoMoveSpeed();
        dispIo.bonlist.AddBonus(20, 1, 139);
    }


    [DispTypes(DispatcherType.SpellDcMod)]
    [TempleDllLocation(0x100fdb70)]
    public static void SpellDcMod_GnomeIllusionBonus_Callback(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIOBonusListAndSpellEntry();
        if (dispIo.spellEntry.spellSchoolEnum == SchoolOfMagic.Illusion)
        {
            dispIo.bonList.AddBonus(1, 0, 139);
        }
    }

    [DispTypes(DispatcherType.ToHitBonus2)]
    [TempleDllLocation(0x100fd930)]
    public static void DwarfBonusToHitOrcsAndGoblins(in DispatcherCallbackArgs args)
    {
        var dispIo = args.GetDispIoAttackBonus();
        var victim = dispIo.attackPacket.victim;
        if (victim != null
            && GameSystems.Critter.IsCategory(victim, MonsterCategory.humanoid)
            && (GameSystems.Critter.IsCategorySubtype(victim, MonsterSubtype.goblinoid) ||
                GameSystems.Critter.IsCategorySubtype(victim, MonsterSubtype.half_orc)))
        {
            dispIo.bonlist.AddBonus(1, 0, 139);
        }
    }

    [DispTypes(DispatcherType.SaveThrowLevel)]
    [TempleDllLocation(0x100fdc30)]
    public static void HalflingWillSaveFear(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoSavingThrow();
        if ((dispIo.flags & D20SavingThrowFlag.SPELL_DESCRIPTOR_FEAR) != 0)
        {
            dispIo.bonlist.AddBonus(2, 13, 139);
        }
    }

    [DispTypes(DispatcherType.TakingDamage2)]
    [TempleDllLocation(0x100fde00)]
    [TemplePlusLocation("ability_fixes.cpp:74")]
    public static void SubtypeFireReductionAndVulnerability(in DispatcherCallbackArgs evt,
        DamageType immuneType, DamageType vulnerableType)
    {
        var dispIo = evt.GetDispIoDamage();
        if ((dispIo.attackPacket.flags & D20CAF.SAVE_SUCCESSFUL) == 0)
        {
            // TemplePlus fix: Take 1.5x damage if vulnerable
            dispIo.damage.AddModFactor(1.5f, vulnerableType, 129);
        }

        dispIo.damage.AddModFactor(0, immuneType, 104);
    }

    [DispTypes(DispatcherType.ToHitBonus2)]
    [TempleDllLocation(0x100fdc70)]
    [TemplePlusLocation("condition.cpp:517")]
    public static void HalflingThrownWeaponBonus(in DispatcherCallbackArgs args)
    {
        var dispIo = args.GetDispIoAttackBonus();
        if ((dispIo.attackPacket.flags & D20CAF.THROWN) != 0)
        {
            dispIo.bonlist.AddBonus(1, 0, 139);
        }
    }

    [DispTypes(DispatcherType.SaveThrowLevel)]
    [TempleDllLocation(0x100fd900)]
    public static void ApplyDwarvenSaveBonusAgainstPoison(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoSavingThrow();
        if ((dispIo.flags & D20SavingThrowFlag.POISON) != 0)
        {
            dispIo.bonlist.AddBonus(2, 0, 139);
        }
    }

    [DispTypes(DispatcherType.D20Query)]
    [TempleDllLocation(0x100fdd40)]
    public static void HighestClassIsFavoredClass(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20Query();
        var requestedClass = (Stat) dispIo.data1;
        var critter = evt.objHndCaller;

        Stat highest = 0;
        var highestLevels = -1;
        foreach (var statClass in D20ClassSystem.AllClasses)
        {
            var levels = critter.GetStat(statClass);
            if (levels > highestLevels)
            {
                highestLevels = levels;
                highest = statClass;
            }
        }

        if (requestedClass == highest)
        {
            dispIo.return_val = 1;
        }
    }

    [DispTypes(DispatcherType.ConditionAddPre)]
    [TempleDllLocation(0x100fdaa0)]
    public static void ElvenConditionImmunity(in DispatcherCallbackArgs evt, ConditionSpec data)
    {
        var dispIo = evt.GetDispIoCondStruct();
        if (dispIo.condStruct == data)
        {
            dispIo.outputFlag = false;
            GameSystems.D20.Combat.FloatCombatLine(evt.objHndCaller, 5059);
            GameSystems.RollHistory.CreateRollHistoryLineFromMesfile(31, evt.objHndCaller, null);
        }
    }


    [DispTypes(DispatcherType.ToHitBonus2)]
    [TempleDllLocation(0x100fdba0)]
    public static void GnomeToHitBonusAgainstGoblins(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoAttackBonus();
        var victim = dispIo.attackPacket.victim;
        if (victim != null && GameSystems.Critter.IsCategory(victim, MonsterCategory.humanoid))
        {
            if (GameSystems.Critter.IsCategorySubtype(victim, MonsterSubtype.goblinoid))
            {
                dispIo.bonlist.AddBonus(1, 0, 139);
            }
        }
    }

    [DispTypes(DispatcherType.SaveThrowLevel)]
    [TempleDllLocation(0x100fdb30)]
    public static void GnomeIllusionSaveBonus(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoSavingThrow();
        if ((dispIo.flags & D20SavingThrowFlag.SPELL_SCHOOL_ILLUSION) != 0)
        {
            dispIo.bonlist.AddBonus(2, 31, 139);
        }
    }

    [DispTypes(DispatcherType.SkillLevel)]
    [TempleDllLocation(0x100fda20)]
    public static void DwarfAppraiseBonus(in DispatcherCallbackArgs args, int data)
    {
        var dispIo = args.GetDispIoObjBonus();
        var item = dispIo.obj;
        if (item != null)
        {
            var material = item.GetMaterial();
            if (material == Material.stone || material == Material.metal)
            {
                dispIo.bonOut.AddBonus(2, 0, 139);
            }
        }
    }

    [DispTypes(DispatcherType.SaveThrowLevel)]
    [TempleDllLocation(0x100fdaf0)]
    public static void ElfSavingThrowBonus(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoSavingThrow();
        if ((dispIo.flags & D20SavingThrowFlag.SPELL_SCHOOL_ENCHANTMENT) != 0)
        {
            dispIo.bonlist.AddBonus(2, 31, 139);
        }
    }

    [DispTypes(DispatcherType.GetMoveSpeedBase)]
    [TempleDllLocation(0x100fdcd0)]
    public static void RacialMoveSpeed30(in DispatcherCallbackArgs args)
    {
        var dispIo = args.GetDispIoMoveSpeed();
        dispIo.bonlist.AddBonus(30, 1, 139);
    }

    [DispTypes(DispatcherType.GetAC)]
    [TempleDllLocation(0x100fd9a0)]
    public static void ArmorBonusVsGiants(in DispatcherCallbackArgs args)
    {
        var dispIo = args.GetDispIoAttackBonus();
        var attacker = dispIo.attackPacket.attacker;
        if (attacker != null)
        {
            if (GameSystems.Critter.IsCategory(attacker, MonsterCategory.giant))
            {
                dispIo.bonlist.AddBonus(4, 8, 139);
            }
        }
    }

    [DispTypes(DispatcherType.SkillLevel)]
    [TempleDllLocation(0x100fd8d0)]
    public static void RacialHideBonus(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoObjBonus();
        dispIo.bonOut.AddBonus(4, 0, 139);
    }

    [DispTypes(DispatcherType.SaveThrowLevel)]
    [TempleDllLocation(0x100fda70)]
    public static void DwarfSaveBonusVsSpells(in DispatcherCallbackArgs args)
    {
        var dispIo = args.GetDispIoSavingThrow();
        if ((dispIo.flags & D20SavingThrowFlag.SPELL_LIKE_EFFECT) != 0)
        {
            dispIo.bonlist.AddBonus(2, 0, 139);
        }
    }
}