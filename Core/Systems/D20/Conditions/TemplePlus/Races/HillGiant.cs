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
public class HillGiant
{
    public const RaceId Id = RaceId.hill_giant;

    // standard callbacks - BAB and Save values

    public static readonly RaceSpec RaceSpec = new(Id, RaceBase.hill_giant)
    {
        hitDice = new Dice(12, 8),
        effectiveLevel = 4,
        helpTopic = "TAG_HILL_GIANT",
        flags = RaceDefinitionFlags.RDF_Monstrous,
        conditionName = "Hill Giant",
        heightMale = (120, 140),
        heightFemale = (110, 130),
        weightMale = (1700, 2210),
        weightFemale = (1600, 2200),
        statModifiers =
        {
            (Stat.strength, 14), (Stat.dexterity, -2), (Stat.constitution, 8),
            (Stat.intelligence, -4), (Stat.charisma, -4)
        },
        naturalArmor = 9,
        ProtoId = 13018,
        materialOffset = 0, // offset into rules/material_ext.mes file,
        feats =
        {
            FeatId.SIMPLE_WEAPON_PROFICIENCY, FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL,
            FeatId.ARMOR_PROFICIENCY_LIGHT, FeatId.ARMOR_PROFICIENCY_MEDIUM,
            FeatId.SHIELD_PROFICIENCY
        },
    };

    public static readonly ConditionSpec Condition = ConditionSpec.Create(RaceSpec.conditionName, 0, UniquenessType.NotUnique)
        .Configure(builder => builder
            .AddAbilityModifierHooks(RaceSpec)
            .AddFavoredClassHook(Stat.level_barbarian)
            .AddSaveThrowBonusHook(SavingThrowType.Fortitude, 8)
            .AddSaveThrowBonusHook(SavingThrowType.Will, 4)
            .AddSaveThrowBonusHook(SavingThrowType.Reflex, 4)
            .AddBaseMoveSpeed(30)
        );
}