
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
using SpicyTemple.Core.Startup.Discovery;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{

    [AutoRegister]
    public class HillGiant
    {
        
        public const RaceId Id = RaceId.hill_giant;

        // standard callbacks - BAB and Save values

        public static readonly RaceSpec RaceSpec = new RaceSpec
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
            statModifiers = {(Stat.strength, 14), (Stat.dexterity, -2), (Stat.constitution, 8),
                (Stat.intelligence, -4), (Stat.charisma, -4)},
            naturalArmor = 9,
            protoId = 13018,
            materialOffset = 0, // offset into rules/material_ext.mes file,
            feats =
            {
                FeatId.SIMPLE_WEAPON_PROFICIENCY, FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL,
                FeatId.ARMOR_PROFICIENCY_LIGHT, FeatId.ARMOR_PROFICIENCY_MEDIUM,
                FeatId.SHIELD_PROFICIENCY
            },
        };

        public static readonly ConditionSpec Condition = ConditionSpec.Create(RaceSpec.conditionName)
            .AddAbilityModifierHooks(RaceSpec)
            .AddFavoredClassHook(Stat.level_barbarian)
            .AddSaveThrowBonusHook(SavingThrowType.Fortitude, 8)
            .AddSaveThrowBonusHook(SavingThrowType.Will, 4)
            .AddSaveThrowBonusHook(SavingThrowType.Reflex, 4)
            .AddBaseMoveSpeed(30)
            .Build();

    }
}
