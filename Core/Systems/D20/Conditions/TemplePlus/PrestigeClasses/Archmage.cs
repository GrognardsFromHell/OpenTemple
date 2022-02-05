
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using System.Reflection.Emit;
using System.Text;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.D20.Classes.Prereq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus;

[AutoRegister]
public class Archmage
{

    private const Stat ClassId = Stat.level_archmage;

    public static readonly D20ClassSpec ClassSpec = new("archmage")
    {
        classEnum = ClassId,
        helpTopic = "TAG_ARCHMAGES",
        conditionName = "Archmage",
        flags = ClassDefinitionFlag.CDF_CoreClass,
        BaseAttackBonusProgression = BaseAttackProgressionType.NonMartial,
        hitDice = 4,
        FortitudeSaveProgression = SavingThrowProgressionType.LOW,
        ReflexSaveProgression = SavingThrowProgressionType.LOW,
        WillSaveProgression = SavingThrowProgressionType.HIGH,
        skillPts = 2,
        spellListType = SpellListType.Arcane,
        hasArmoredArcaneCasterFeature = false,
        spellMemorizationType = SpellReadyingType.Vancian,
        spellSourceType = SpellSourceType.Arcane,
        spellCastingConditionName = "Archmage Spellcasting",
        classSkills = new HashSet<SkillId>
        {
            SkillId.concentration,
            SkillId.search,
            SkillId.spellcraft,
            SkillId.alchemy,
            SkillId.craft,
            SkillId.knowledge_all,
            SkillId.profession,
        }.ToImmutableHashSet(),
        Requirements =
        {
            ClassPrereqs.SkillRanks(SkillId.spellcraft, 15),
            ClassPrereqs.Feat(FeatId.SKILL_FOCUS_SPELLCRAFT),
            ClassPrereqs.ArcaneSpellCaster(7),
            new TwoSpellFocusRequirement()
        }
    };

    public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
        .Build();

    public static void OnGetBaseCasterLevel(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetEvtObjSpellCaster();
        var class_extended_1 = (Stat) evt.GetConditionArg1();
        var class_code = dispIo.arg0;
        if ((class_code != class_extended_1))
        {
            if ((dispIo.arg1 == 0)) // arg1 != 0 means you're looking for this particular class's contribution
            {
                return;
            }

        }

        var classLvl = evt.objHndCaller.GetStat(ClassId);
        dispIo.bonlist.AddBonus(classLvl, 0, 137);
    }
    public static void OnSpellListExtensionGet(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetEvtObjSpellCaster();
        var class_extended_1 = (Stat) evt.GetConditionArg1();
        var class_code = dispIo.arg0;
        if ((class_code != class_extended_1))
        {
            if ((dispIo.arg1 == 0)) // arg1 != 0 means you're looking for this particular class's contribution
            {
                return;
            }

        }

        var classLvl = evt.objHndCaller.GetStat(ClassId);
        dispIo.bonlist.AddBonus(classLvl, 0, 137);
    }
    public static readonly ConditionSpec spellCasterSpecObj = ConditionSpec.Create(ClassSpec.spellCastingConditionName, 8)
        .AddHandler(DispatcherType.GetBaseCasterLevel, OnGetBaseCasterLevel)
        .AddHandler(DispatcherType.SpellListExtension, OnSpellListExtensionGet)
        .Build();
}

// A requirement that checks the critter has two DIFFERENT spell focus feats
public class TwoSpellFocusRequirement : ICritterRequirement
{
    private static readonly FeatId[] SpellFocusFeats =
    {
        FeatId.SPELL_FOCUS_ABJURATION,
        FeatId.SPELL_FOCUS_CONJURATION,
        FeatId.SPELL_FOCUS_DIVINATION,
        FeatId.SPELL_FOCUS_ENCHANTMENT,
        FeatId.SPELL_FOCUS_EVOCATION,
        FeatId.SPELL_FOCUS_ILLUSION,
        FeatId.SPELL_FOCUS_NECROMANCY,
        FeatId.SPELL_FOCUS_TRANSMUTATION
    };

    public bool FullfillsRequirements(GameObject critter)
    {
        foreach (var firstFocusId in SpellFocusFeats)
        {
            if (critter.HasFeat(firstFocusId))
            {
                foreach (var secondFocusId in SpellFocusFeats)
                {
                    if (secondFocusId != firstFocusId && critter.HasFeat(secondFocusId))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void DescribeRequirement(StringBuilder builder)
    {
        builder.Append("Spell-Focus Feat in two different schools");
    }
}