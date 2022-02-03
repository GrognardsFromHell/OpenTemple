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

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    [AutoRegister]
    public class Tiefling
    {
        public const RaceId Id = RaceId.human + (3 << 5);

        public static readonly RaceSpec RaceSpec = new RaceSpec(Id, RaceBase.human, Subrace.tiefling)
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
            ProtoId = 13032,
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