
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
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus;

[AutoRegister]
public class Svirfneblin
{
    public static string GetConditionName()
    {
        return "Svirfneblin";
    }
    private const RaceId Id = RaceId.gnome + (1 << 5);

    public static readonly RaceSpec RaceSpec = new RaceSpec(Id, RaceBase.gnome, Subrace.svirfneblin)
    {
        effectiveLevel = 3,
        helpTopic = "TAG_SVIRFNEBLIN",
        flags = 0,
        conditionName = "Svirfneblin",
        heightMale = (36, 44),
        heightFemale = (34, 42),
        weightMale = (42, 44),
        weightFemale = (37, 39),
        statModifiers = {(Stat.strength, -2), (Stat.dexterity, 2), (Stat.wisdom, 2),
            (Stat.charisma, -4)},
        ProtoId = 13034,
        materialOffset = 8, // offset into rules/material_ext.mes file
        spellLikeAbilities =
        {
            {new SpellStoreData(WellKnownSpells.BlindnessDeafness, 2, SpellSystem.GetSpellClass(DomainId.Special)), 1},
            {new SpellStoreData(WellKnownSpells.Blur, 2, SpellSystem.GetSpellClass(DomainId.Special)), 1}
        }
    };

    public static readonly ConditionSpec Condition = ConditionSpec.Create(RaceSpec.conditionName)
        .AddAbilityModifierHooks(RaceSpec)
        .AddSaveThrowBonusHook(SavingThrowType.Will, 2)
        .AddSkillBonuses(
            (SkillId.hide,6),
            (SkillId.listen,2)
        )
        .AddBaseMoveSpeed(20)
        .AddHandler(DispatcherType.SpellResistanceMod, OnGetSpellResistance)
        .AddHandler(DispatcherType.Tooltip, OnGetSpellResistanceTooltip)
        .AddFavoredClassHook(Stat.level_rogue)
        .AddHandler(DispatcherType.ToHitBonus2, OnGetToHitBonusVsOrcsAndKobolds)
        .AddHandler(DispatcherType.GetAC, OnGetDodgeBonus)
        .AddHandler(DispatcherType.SpellDcMod, OnIllusionDCBonus)
        .Build();

    public static void OnGetSpellResistance(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIOBonusListAndSpellEntry();
        var classLvl = evt.objHndCaller.GetStat(Stat.level);
        dispIo.bonList.AddBonus(11 + classLvl, 36, "Racial Bonus (Svirfneblin)");
    }
    public static void OnGetSpellResistanceTooltip(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoTooltip();
        var classLvl = evt.objHndCaller.GetStat(Stat.level);
        dispIo.Append("Spell Resistance [" + (11 + classLvl).ToString() + "]");
    }

    public static void OnGetToHitBonusVsOrcsAndKobolds(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoAttackBonus();
        var target = dispIo.attackPacket.victim;
        if (target == null)
        {
            return;
        }

        if (target.IsMonsterSubtype(MonsterSubtype.goblinoid))
        {
            dispIo.bonlist.AddBonus(1, 0, 139); // Racial Bonus
        }
    }

    public static void OnGetDodgeBonus(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoAttackBonus();
        dispIo.bonlist.AddBonus(4, 0, 139); // Racial Bonus
    }

    public static void OnIllusionDCBonus(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIOBonusListAndSpellEntry();
        var school = dispIo.spellEntry.spellSchoolEnum;
        if (school == SchoolOfMagic.Illusion)
        {
            dispIo.bonList.AddBonus(1, 31, 139); // Racial Bonus
        }
    }
    // Note:  Adding the size +4 bonus to hide as a racial bonus since setting size to small does not grant the bonus

}