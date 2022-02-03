using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Particles.Instances;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Utils;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.Script;

namespace OpenTemple.Core.Systems.D20.Conditions;

public static class ClassConditions
{

    public static void GrantClassLevelAsCasterLevel(in DispatcherCallbackArgs evt, Stat classEnum)
    {
        var dispIo = evt.GetEvtObjSpellCaster();
        if (dispIo.arg0 == classEnum)
        {
            var classLevels = evt.objHndCaller.GetStat(classEnum);
            dispIo.bonlist.AddBonus(classLevels, 0, 137);
        }
    }

    public static void GetRangerOrPaladinCasterLevel(in DispatcherCallbackArgs evt, Stat classEnum)
    {
        var dispIo = evt.GetEvtObjSpellCaster();

        if (dispIo.arg0 == classEnum)
        {
            var classLvl = evt.objHndCaller.GetStat(classEnum);
            if (classLvl >= 4)
            {
                dispIo.bonlist.AddBonus(classLvl / 2, 0, 137);
            }
        }
    }

    [TempleDllLocation(0x102f0604)]
    public static readonly ConditionSpec SchoolSpecialization = ConditionSpec.Create("School Specialization")
        .AddSkillLevelHandler(SkillId.spellcraft, SchoolSpecializationSkillLevel)
        .Build();

    [DispTypes(DispatcherType.GetAC)]
    [TempleDllLocation(0x100feac0)]
    public static void TrapSenseDodgeBonus(in DispatcherCallbackArgs evt, Stat classStat)
    {
        var classLvl = evt.objHndCaller.GetStat(classStat) / 3;
        if (classLvl >= 1)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if ((dispIo.attackPacket.flags & D20CAF.TRAP) != 0)
            {
                dispIo.bonlist.AddBonus(classLvl, 8, 280);
            }
        }
    }

    [DispTypes(DispatcherType.GetAC)]
    [TempleDllLocation(0x100feb30)]
    public static void TrapSenseRefSaveBonus(in DispatcherCallbackArgs evt, Stat classStat)
    {
        var dispIo = evt.GetDispIoSavingThrow();
        var classLevel = evt.objHndCaller.GetStat(classStat);
        if (classLevel / 3 >= 1)
        {
            if ((dispIo.flags & D20SavingThrowFlag.TRAP) != 0)
            {
                dispIo.bonlist.AddBonus(classLevel / 3, 8, 280);
            }
        }
    }

    [DispTypes(DispatcherType.SkillLevel)]
    [TempleDllLocation(0x100febf0)]
    public static void SchoolSpecializationSkillLevel(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoObjBonus();
        GameSystems.Spell.GetSchoolSpecialization(evt.objHndCaller, out var specializedSchool,
            out var forbiddenSchool1, out var forbiddenSchool2);

        var specializedCheckFlags = GameSystems.Skill.GetSkillCheckFlagsForSchool(specializedSchool);
        if ((dispIo.flags & specializedCheckFlags) != 0)
        {
            var name = GameSystems.Spell.GetSchoolOfMagicName(specializedSchool);
            dispIo.bonOut.AddBonus(2, 0, 306, name);
        }

        var forbiddenFlags1 = GameSystems.Skill.GetSkillCheckFlagsForSchool(forbiddenSchool1);
        if ((dispIo.flags & forbiddenFlags1) != 0)
        {
            var name = GameSystems.Spell.GetSchoolOfMagicName(forbiddenSchool1);
            dispIo.bonOut.AddBonus(-5, 0, 307, name);
        }

        var forbiddenFlags2 = GameSystems.Skill.GetSkillCheckFlagsForSchool(forbiddenSchool2);
        if ((dispIo.flags & forbiddenFlags2) != 0)
        {
            var name = GameSystems.Spell.GetSchoolOfMagicName(forbiddenSchool2);
            dispIo.bonOut.AddBonus(-5, 0, 307, name);
        }
    }
}