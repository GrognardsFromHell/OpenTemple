using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.Feats;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus.Races
{
    [AutoRegister]
    public class Aasimar
    {
        public static readonly RaceId Id = RaceId.human + (2 << 5);

        public static readonly RaceSpec RaceSpec = new RaceSpec(Id, RaceBase.human, Subrace.aasumar)
        {
            effectiveLevel  = 1,
            helpTopic      = "TAG_AASIMARS",
            flags           = 0,
            conditionName   = "Aasimar",
            heightMale     = (58, 78),
            heightFemale   = (53, 73),
            weightMale     = (124, 200),
            weightFemale   = (89, 165),
            statModifiers = {(Stat.wisdom, 2), (Stat.charisma, 2)},
            ProtoId        = 13022,
            materialOffset = 0, // offset into rules/material_ext.mes file,
            feats           = {FeatId.SIMPLE_WEAPON_PROFICIENCY, FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL},
            useBaseRaceForDeity = true,
        };

        public static readonly ConditionSpec Condition = ConditionSpec.Create(RaceSpec.conditionName)
            .AddAbilityModifierHooks(RaceSpec)
            .AddSkillBonuses((SkillId.listen, 2), (SkillId.spot, 2))
            .AddBaseMoveSpeed(30)
            .AddFavoredClassHook(Stat.level_paladin)
            .AddDamageResistances(
                (DamageType.Acid, 5),
                (DamageType.Cold, 5),
                (DamageType.Electricity, 5)
            )
            .Build();
    }
}