using System.Collections.Generic;
using System.Collections.Immutable;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Feats;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    [AutoRegister]
    public static class Barbarian
    {
        public const Stat ClassId = Stat.level_barbarian;

        public static readonly D20ClassSpec ClassSpec = new D20ClassSpec("barbarian")
        {
            classEnum = ClassId,
            helpTopic = "TAG_BARBARIANS",
            conditionName = "Barbarian",
            flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
            BaseAttackBonusProgression = BaseAttackProgressionType.Martial,
            hitDice = 12,
            FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
            ReflexSaveProgression = SavingThrowProgressionType.LOW,
            WillSaveProgression = SavingThrowProgressionType.LOW,
            skillPts = 4,
            hasArmoredArcaneCasterFeature = false,
            classSkills = new HashSet<SkillId>
            {
                SkillId.intimidate,
                SkillId.listen,
                SkillId.wilderness_lore,
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
                {FeatId.SHIELD_PROFICIENCY, 1},
                {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                {FeatId.BARBARIAN_RAGE, 1},
                {FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1},
                {FeatId.FAST_MOVEMENT, 1},
                {FeatId.UNCANNY_DODGE, 2},
                {FeatId.IMPROVED_UNCANNY_DODGE, 5},
                {FeatId.GREATER_RAGE, 11},
                {FeatId.INDOMITABLE_WILL, 14},
                {FeatId.TIRELESS_RAGE, 17},
                {FeatId.MIGHTY_RAGE, 20},
            }.ToImmutableDictionary(),
        };

        [TempleDllLocation(0x102eff08)]
        public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
            .AddHandler(DispatcherType.GetAC, ClassConditions.TrapSenseDodgeBonus, Stat.level_barbarian)
            .AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_REFLEX, ClassConditions.TrapSenseRefSaveBonus,
                Stat.level_barbarian)
            .AddHandler(DispatcherType.TakingDamage2, BarbarianDRDamageCallback)
            .Build();

        [DispTypes(DispatcherType.TakingDamage2)]
        [TempleDllLocation(0x100feba0)]
        [TemplePlusLocation("condition.cpp:422")]
        private static void BarbarianDRDamageCallback(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            int barbLvl = evt.objHndCaller.GetStat(Stat.level_barbarian);
            if (barbLvl >= 7)
            {
                var damRes = 1 + (barbLvl - 7) / 3;
                dispIo.damage.AddPhysicalDR(damRes, D20AttackPower.UNSPECIFIED, 126);
            }
        }
    }
}