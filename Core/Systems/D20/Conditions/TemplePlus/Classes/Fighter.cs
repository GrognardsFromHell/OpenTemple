using System.Collections.Generic;
using System.Collections.Immutable;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Feats;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    [AutoRegister]
    public class Fighter
    {
        public static readonly Stat ClassId = Stat.level_fighter;

        public static readonly D20ClassSpec ClassSpec = new D20ClassSpec("fighter")
        {
            classEnum = ClassId,
            helpTopic = "TAG_FIGHTERS",
            conditionName = "Fighter",
            flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
            BaseAttackBonusProgression = BaseAttackProgressionType.Martial,
            hitDice = 10,
            FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
            ReflexSaveProgression = SavingThrowProgressionType.LOW,
            WillSaveProgression = SavingThrowProgressionType.LOW,
            skillPts = 2,
            hasArmoredArcaneCasterFeature = false,
            classSkills = new HashSet<SkillId>
            {
                SkillId.intimidate,
                SkillId.alchemy,
                SkillId.climb,
                SkillId.craft,
                SkillId.handle_animal,
                SkillId.jump,
                SkillId.ride,
                SkillId.swim,
            }.ToImmutableHashSet(),
            classFeats = new Dictionary<FeatId, int>
            {
                {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                {FeatId.ARMOR_PROFICIENCY_MEDIUM, 1},
                {FeatId.ARMOR_PROFICIENCY_HEAVY, 1},
                {FeatId.SHIELD_PROFICIENCY, 1},
                {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                {FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1},
            }.ToImmutableDictionary(),
        };

        [TempleDllLocation(0x102f0148)]
        public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
            .Build();
    }
}