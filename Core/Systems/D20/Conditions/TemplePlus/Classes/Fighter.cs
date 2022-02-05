using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Ui.PartyCreation.Systems;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus;

[AutoRegister]
public class Fighter
{
    public static readonly Stat ClassId = Stat.level_fighter;

    public static readonly ImmutableList<SelectableFeat> CombatFeats = new [] {
        FeatId.BLIND_FIGHT, FeatId.POWER_ATTACK, FeatId.CLEAVE, FeatId.GREAT_CLEAVE, FeatId.COMBAT_EXPERTISE,
        FeatId.IMPROVED_DISARM, FeatId.IMPROVED_FEINT, FeatId.IMPROVED_TRIP, FeatId.WHIRLWIND_ATTACK,
        FeatId.COMBAT_REFLEXES, FeatId.DODGE, FeatId.MOBILITY, FeatId.SUPERIOR_EXPERTISE,
        FeatId.MARTIAL_WEAPON_PROFICIENCY, FeatId.IMPROVED_CRITICAL, FeatId.IMPROVED_INITIATIVE,
        FeatId.IMPROVED_UNARMED_STRIKE, FeatId.DEFLECT_ARROWS, FeatId.IMPROVED_GRAPPLE,
        FeatId.IMPROVED_OVERRUN, FeatId.IMPROVED_SHIELD_BASH, FeatId.IMPROVED_TWO_WEAPON_FIGHTING,
        FeatId.WEAPON_FINESSE, FeatId.GREATER_TWO_WEAPON_FIGHTING, FeatId.IMPROVED_SUNDER,
        FeatId.TWO_WEAPON_DEFENSE, FeatId.TWO_WEAPON_FIGHTING, FeatId.IMPROVED_PRECISE_SHOT,
        FeatId.TRAMPLE, FeatId.STUNNING_FIST, FeatId.SPRING_ATTACK, FeatId.SPIRITED_CHARGE,
        FeatId.SNATCH_ARROWS, FeatId.SHOT_ON_THE_RUN, FeatId.RIDE_BY_ATTACK, FeatId.RAPID_RELOAD,
        FeatId.RAPID_SHOT, FeatId.QUICK_DRAW, FeatId.PRECISE_SHOT, FeatId.POINT_BLANK_SHOT, FeatId.MANYSHOT,
        FeatId.MOUNTED_COMBAT, FeatId.MOUNTED_ARCHERY, FeatId.FAR_SHOT, FeatId.IMPROVED_BULL_RUSH,
        FeatId.WEAPON_FOCUS, FeatId.WEAPON_SPECIALIZATION, FeatId.GREATER_WEAPON_FOCUS,
        FeatId.GREATER_WEAPON_SPECIALIZATION,
        MeleeWeaponMastery.BaseFeatId, VexingFlanker.Id, RangedWeaponMastery.BaseFeatId
    }.Select(f => new SelectableFeat(f)).ToImmutableList();

    public static readonly D20ClassSpec ClassSpec = new("fighter")
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
        IsSelectingFeatsOnLevelUp = critter =>
        {
            var newLvl = critter.GetStat(ClassSpec.classEnum) + 1;
            return newLvl % 2 == 0 || newLvl == 1;
        },
        LevelupGetBonusFeats = critter => CombatFeats
    };

    [TempleDllLocation(0x102f0148)]
    public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
        .Build();
}