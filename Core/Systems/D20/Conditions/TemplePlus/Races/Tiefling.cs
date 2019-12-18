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
    public class Tiefling
    {
        public const RaceId Id = RaceId.human + (3 << 5);

        public static readonly RaceSpec RaceSpec = new RaceSpec
        {
            effectiveLevel = 1,
            helpTopic = "TAG_TIEFLING",
            flags = 0,
            conditionName = "Tiefling",
            heightMale = (58, 78),
            heightFemale = (53, 73),
            weightMale = (124, 200),
            weightFemale = (89, 165),
            statModifiers = {(Stat.dexterity, 2), (Stat.intelligence, 2), (Stat.charisma, -2)},
            protoId = 13032,
            materialOffset = 0, // offset into rules/material_ext.mes file
            feats = {FeatId.SIMPLE_WEAPON_PROFICIENCY, FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL},
            useBaseRaceForDeity = true
        };

        public static readonly ConditionSpec Condition = ConditionSpec.Create(RaceSpec.conditionName)
            .AddAbilityModifierHooks(RaceSpec)
            .AddSkillBonuses((SkillId.bluff, 2), (SkillId.hide, 2))
            .AddBaseMoveSpeed(30)
            .AddFavoredClassHook(Stat.level_rogue)
            .AddDamageResistances(
                (DamageType.Fire, 5),
                (DamageType.Cold, 5),
                (DamageType.Electricity, 5))
            .Build();
    }
}