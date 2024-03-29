using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Ui.PartyCreation.Systems;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus;

[AutoRegister]
public class Monk
{
    public static readonly Stat ClassId = Stat.level_monk;

    public static readonly D20ClassSpec ClassSpec = new("monk")
    {
        classEnum = ClassId,
        helpTopic = "TAG_MONKS",
        conditionName = "Monk",
        flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
        BaseAttackBonusProgression = BaseAttackProgressionType.SemiMartial,
        hitDice = 8,
        FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
        ReflexSaveProgression = SavingThrowProgressionType.HIGH,
        WillSaveProgression = SavingThrowProgressionType.HIGH,
        skillPts = 4,
        hasArmoredArcaneCasterFeature = false,
        classSkills = new HashSet<SkillId>
        {
            SkillId.concentration,
            SkillId.diplomacy,
            SkillId.hide,
            SkillId.listen,
            SkillId.move_silently,
            SkillId.sense_motive,
            SkillId.spot,
            SkillId.tumble,
            SkillId.perform,
            SkillId.alchemy,
            SkillId.balance,
            SkillId.climb,
            SkillId.craft,
            SkillId.escape_artist,
            SkillId.jump,
            SkillId.knowledge_arcana,
            SkillId.knowledge_religion,
            SkillId.profession,
            SkillId.swim,
        }.ToImmutableHashSet(),
        classFeats = new Dictionary<FeatId, int>
        {
            {FeatId.IMPROVED_UNARMED_STRIKE, 1},
            {FeatId.STUNNING_FIST, 1},
            {FeatId.STUNNING_ATTACKS, 1},
            {FeatId.SIMPLE_WEAPON_PROFICIENCY_MONK, 1},
            {FeatId.FLURRY_OF_BLOWS, 1},
            {FeatId.EVASION, 2},
            {FeatId.FAST_MOVEMENT, 3},
            {FeatId.STILL_MIND, 3},
            {FeatId.KI_STRIKE, 4},
            {FeatId.PURITY_OF_BODY, 5},
            {FeatId.WHOLENESS_OF_BODY, 7},
            {FeatId.IMPROVED_EVASION, 9},
            {FeatId.MONK_DIAMOND_BODY, 11},
            {FeatId.MONK_ABUNDANT_STEP, 12},
            {FeatId.MONK_DIAMOND_SOUL, 13},
            {FeatId.MONK_QUIVERING_PALM, 15},
            {FeatId.MONK_EMPTY_BODY, 19},
            {FeatId.MONK_PERFECT_SELF, 20},
        }.ToImmutableDictionary(),
        IsSelectingFeatsOnLevelUp = critter =>
        {
            var newLvl = critter.GetStat(ClassSpec.classEnum) + 1;
            if (newLvl == 2)
            {
                return !critter.HasFeat(FeatId.COMBAT_REFLEXES) || !critter.HasFeat(FeatId.DEFLECT_ARROWS);
            }

            if (newLvl == 6)
            {
                return !critter.HasFeat(FeatId.IMPROVED_TRIP) || !critter.HasFeat(FeatId.IMPROVED_DISARM);
            }

            return false;
        },
        LevelupGetBonusFeats = GetBonusFeats
    };

    private static IEnumerable<SelectableFeat> GetBonusFeats(GameObject critter)
    {
        var newLvl = critter.GetStat(ClassSpec.classEnum) + 1;
        if (newLvl == 2)
        {
            yield return new SelectableFeat(FeatId.COMBAT_REFLEXES);
            yield return new SelectableFeat(FeatId.DEFLECT_ARROWS);
        }
        else if (newLvl == 6)
        {
            yield return new SelectableFeat(FeatId.IMPROVED_TRIP);
            yield return new SelectableFeat(FeatId.IMPROVED_DISARM);
        }
    }

    [TempleDllLocation(0x102f01c8)]
    public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec, builder => builder
        .AddHandler(DispatcherType.GetAC, MonkAcBonus, Stat.level_monk)
    );

    [DispTypes(DispatcherType.GetAC)]
    [TempleDllLocation(0x100fedd0)]
    public static void MonkAcBonus(in DispatcherCallbackArgs evt, Stat classStat)
    {
        var dispIo = evt.GetDispIoAttackBonus();
        var critter = evt.objHndCaller;

        if (!FulfillsArmorAndLoadRequirement(critter))
        {
            return;
        }

        var wisdomMod = critter.GetStat(Stat.wis_mod);
        if (wisdomMod != 0)
        {
            dispIo.bonlist.AddBonus(wisdomMod, 0, 310);
        }

        //  Potential fix here: Add monk bonus even if wisdom mod != 0
        var monkLevelBonus = critter.GetStat(classStat) / 5;

        var hasMonkBelt = GameSystems.Item.IsProtoWornAt(critter, EquipSlot.Lockpicks, WellKnownProtos.MonksBelt);
        if (hasMonkBelt)
        {
            monkLevelBonus += 1;
        }

        if (monkLevelBonus > 0)
        {
            dispIo.bonlist.AddBonus(monkLevelBonus, 0, 311);
        }
    }

    /// <summary>
    /// Tests whether the given critter fullfils the requirements laid out in the Monk class description for
    /// gaining the AC bonus and further abilities.
    /// </summary>
    [TempleDllLocation(0x100fece0)]
    public static bool FulfillsArmorAndLoadRequirement(GameObject critter)
    {
        var armor = GameSystems.Item.ItemWornAt(critter, EquipSlot.Armor);
        if (armor != null && GameSystems.D20.D20QueryItem(armor, D20DispatcherKey.QUE_Armor_Get_AC_Bonus) != 0)
        {
            return false;
        }

        bool IsShieldInSlot(EquipSlot slot)
        {
            var item = GameSystems.Item.ItemWornAt(critter, slot);
            return item != null && item.type == ObjectType.armor && item.GetArmorFlags().IsShield();
        }

        if (IsShieldInSlot(EquipSlot.WeaponPrimary)
            || IsShieldInSlot(EquipSlot.WeaponSecondary)
            || IsShieldInSlot(EquipSlot.Shield))
        {
            return false;
        }

        return (EncumbranceType) critter.GetStat(Stat.load) == EncumbranceType.LightLoad;
    }
}