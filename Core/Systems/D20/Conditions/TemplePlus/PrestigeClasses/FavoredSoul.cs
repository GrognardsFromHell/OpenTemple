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
using OpenTemple.Core.AAS;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Ui.PartyCreation.Systems;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus;

[AutoRegister]
public class FavoredSoul
{
    private static readonly Stat ClassId = Stat.level_favored_soul;

    public const string AcidResistance = "Favored Soul Acid Resistance";
    public static readonly FeatId AcidResistanceId = (FeatId) ElfHash.Hash(AcidResistance);

    public const string ColdResistance = "Favored Soul Cold Resistance";
    public static readonly FeatId ColdResistanceId = (FeatId) ElfHash.Hash(ColdResistance);

    public const string ElectricityResistance = "Favored Soul Electricity Resistance";
    public static readonly FeatId ElectricityResistanceId = (FeatId) ElfHash.Hash(ElectricityResistance);

    public const string FireResistance = "Favored Soul Fire Resistance";
    public static readonly FeatId FireResistanceId = (FeatId) ElfHash.Hash(FireResistance);

    public const string SonicResistance = "Favored Soul Sonic Resistance";
    public static readonly FeatId SonicResistanceId = (FeatId) ElfHash.Hash(SonicResistance);

    private static readonly ImmutableList<SelectableFeat> ResistanceBonusFeats = new[]
    {
        AcidResistanceId,
        ColdResistanceId,
        ElectricityResistanceId,
        FireResistanceId,
        SonicResistanceId
    }.Select(featId => new SelectableFeat(featId)
    {
        IsIgnoreRequirements = true
    }).ToImmutableList();

    public static readonly D20ClassSpec ClassSpec = new("favored_soul")
    {
        classEnum = ClassId,
        helpTopic = "TAG_FAVORED_SOULS",
        conditionName = "Favored Soul",
        flags = ClassDefinitionFlag.CDF_BaseClass,
        BaseAttackBonusProgression = BaseAttackProgressionType.SemiMartial,
        hitDice = 8,
        FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
        ReflexSaveProgression = SavingThrowProgressionType.HIGH,
        WillSaveProgression = SavingThrowProgressionType.HIGH,
        skillPts = 2,
        spellListType = SpellListType.Clerical,
        hasArmoredArcaneCasterFeature = false,
        spellMemorizationType = SpellReadyingType.Innate,
        spellSourceType = SpellSourceType.Divine,
        spellCastingConditionName = null,
        spellsPerDay = new Dictionary<int, IImmutableList<int>>
        {
            [1] = ImmutableList.Create(5, 3),
            [2] = ImmutableList.Create(6, 4),
            [3] = ImmutableList.Create(6, 5),
            [4] = ImmutableList.Create(6, 6, 3),
            [5] = ImmutableList.Create(6, 6, 4),
            [6] = ImmutableList.Create(6, 6, 5, 3),
            [7] = ImmutableList.Create(6, 6, 6, 4),
            [8] = ImmutableList.Create(6, 6, 6, 5, 3),
            [9] = ImmutableList.Create(6, 6, 6, 6, 4),
            [10] = ImmutableList.Create(6, 6, 6, 6, 5, 3),
            [11] = ImmutableList.Create(6, 6, 6, 6, 6, 4),
            [12] = ImmutableList.Create(6, 6, 6, 6, 6, 5, 3),
            [13] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 4),
            [14] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 5, 3),
            [15] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 4),
            [16] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 5, 3),
            [17] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 4),
            [18] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 5, 3),
            [19] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 6, 4),
            [20] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 6, 6)
        }.ToImmutableDictionary(),
        classSkills = new HashSet<SkillId>
        {
            SkillId.concentration,
            SkillId.diplomacy,
            SkillId.heal,
            SkillId.sense_motive,
            SkillId.spellcraft,
            SkillId.alchemy,
            SkillId.craft,
            SkillId.knowledge_arcana,
            SkillId.profession,
        }.ToImmutableHashSet(),
        classFeats = new Dictionary<FeatId, int>
        {
            {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
            {FeatId.ARMOR_PROFICIENCY_MEDIUM, 1},
            {FeatId.SHIELD_PROFICIENCY, 1},
            {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
            {FeatId.DOMAIN_POWER, 1},
            // TODO: Investigate these feats
            {(FeatId) ElfHash.Hash("Deity's Weapon Focus"), 1},
            {(FeatId) ElfHash.Hash("Energy Resistance (Favored Soul)"), 1},
            {(FeatId) ElfHash.Hash("Deity's Weapon Specialization"), 1},
            {(FeatId) ElfHash.Hash("Damage Reduction (Favored Soul)"), 1}
        }.ToImmutableDictionary(),
        deityClass = Stat.level_cleric,
        IsSelectingFeatsOnLevelUp = critter =>
        {
            var newLvl = critter.GetStat(ClassSpec.classEnum) + 1;
            if ((newLvl == 5) || (newLvl == 10) || (newLvl == 15))
            {
                return true;
            }

            if (newLvl != 3 && newLvl != 12)
            {
                return false;
            }

            // At level three they may choose a feat if they already have the weapon focus for their
            // deities favored weapon
            var deity = critter.GetDeity();
            var deityWeapon = GameSystems.Deity.GetFavoredWeapon(deity);
            if (newLvl == 3 &&
                GameSystems.Feat.TryGetFeatForWeaponType(FeatId.WEAPON_FOCUS, deityWeapon, out var focusFeat))
            {
                if (critter.HasFeat(focusFeat))
                {
                    return true;
                }
            }

            // Same as above, this time for weapon specialization
            if (newLvl == 12 &&
                GameSystems.Feat.TryGetFeatForWeaponType(FeatId.WEAPON_SPECIALIZATION, deityWeapon,
                    out var specFeat))
            {
                if (critter.HasFeat(specFeat))
                {
                    return true;
                }
            }

            return false;
        },
        LevelupGetBonusFeats = GetBonusFeats
    };

    private static IEnumerable<SelectableFeat> GetBonusFeats(GameObject critter)
    {
        var newLvl = critter.GetStat(ClassSpec.classEnum) + 1;
        if (newLvl == 5 || newLvl == 10 || newLvl == 15)
        {
            return ResistanceBonusFeats;
        }

        if (newLvl == 3 || newLvl == 12)
        {
            var deity = critter.GetDeity();
            var deityWeapon = GameSystems.Deity.GetFavoredWeapon(deity);
            if (newLvl == 3 &&
                GameSystems.Feat.TryGetFeatForWeaponType(FeatId.WEAPON_FOCUS, deityWeapon, out var focusFeat))
            {
                if (critter.HasFeat(focusFeat))
                {
                    return new[] {new SelectableFeat(FeatId.WEAPON_FOCUS)};
                }
            }

            // TODO: Level 12 feat is missing
        }

        return Enumerable.Empty<SelectableFeat>();
    }

    public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec, builder => builder
        .AddHandler(DispatcherType.GetBaseCasterLevel, OnGetBaseCasterLevel)
        .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Activate,
            OnInitLevelupSpellSelection)
        .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Finalize,
            OnLevelupSpellsFinalize)
        .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Check_Complete,
            OnLevelupSpellsCheckComplete)
        .AddHandler(DispatcherType.TakingDamage2, FavoredSoulDR)
    );

    // Spell casting
    public static void OnGetBaseCasterLevel(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetEvtObjSpellCaster();
        if (dispIo.arg0 != ClassId)
        {
            return;
        }

        var classLvl = evt.objHndCaller.GetStat(ClassId);
        dispIo.bonlist.AddBonus(classLvl, 0, 137);
        return;
    }

    public static void OnInitLevelupSpellSelection(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetEvtObjSpellCaster();
        if (dispIo.arg0 != ClassId)
        {
            return;
        }

        throw new NotImplementedException();
        // classSpecModule.InitSpellSelection(evt.objHndCaller);
    }

    public static void OnLevelupSpellsCheckComplete(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetEvtObjSpellCaster();
        if (dispIo.arg0 != ClassId)
        {
            return;
        }

        throw new NotImplementedException();
        // if (!classSpecModule.LevelupCheckSpells(evt.objHndCaller))
        // {
        //     dispIo.bonlist.AddBonus(-1, 0, 137); // denotes incomplete spell selection
        // }
    }

    public static void OnLevelupSpellsFinalize(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetEvtObjSpellCaster();
        if (dispIo.arg0 != ClassId)
        {
            return;
        }

        throw new NotImplementedException();
        // classSpecModule.LevelupSpellsFinalize(evt.objHndCaller);
    }

    // Damage reduction
    public static void FavoredSoulDR(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoDamage();
        var fav_soul_lvl = evt.objHndCaller.GetStat(ClassId);
        if (fav_soul_lvl < 20)
        {
            return;
        }

        var bonval = 10;
        var align = evt.objHndCaller.GetAlignment();
        if ((align.IsChaotic()))
        {
            dispIo.damage.AddPhysicalDR(bonval, D20AttackPower.COLD, 126); // DR 10/Cold Iron
        }
        else
        {
            dispIo.damage.AddPhysicalDR(bonval, D20AttackPower.SILVER, 126); // DR 10/Silver
        }

        // skipped implementing the choice for neutrals, partly because there's no Cold Iron in ToEE (and Silver is pretty rare too)
        return;
    }

    #region Energy Resistance

    public static void FavSoulEnergyRes(in DispatcherCallbackArgs evt, DamageType damageType)
    {
        var dispIo = evt.GetDispIoDamage();
        dispIo.damage.AddDR(10, damageType, 124);
    }

    [FeatCondition(AcidResistance)]
    public static readonly ConditionSpec EnergyResistanceAcidCondition = ConditionSpec.Create("Favored Soul Acid Resistance", 3, UniquenessType.Unique)
        .Configure(builder => builder
            .AddHandler(DispatcherType.TakingDamage, FavSoulEnergyRes, DamageType.Acid)
        );

    [FeatCondition(ColdResistance)]
    public static readonly ConditionSpec EnergyResistanceColdCondition = ConditionSpec.Create("Favored Soul Cold Resistance", 3, UniquenessType.Unique)
        .Configure(builder => builder
            .AddHandler(DispatcherType.TakingDamage, FavSoulEnergyRes, DamageType.Cold)
        );

    [FeatCondition(ElectricityResistance)]
    public static readonly ConditionSpec EnergyResistanceElectricityCondition = ConditionSpec.Create("Favored Soul Electricity Resistance", 3, UniquenessType.Unique)
        .Configure(builder => builder
            .AddHandler(DispatcherType.TakingDamage, FavSoulEnergyRes, DamageType.Electricity)
        );

    [FeatCondition(FireResistance)]
    public static readonly ConditionSpec EnergyResistanceFireCondition = ConditionSpec.Create("Favored Soul Fire Resistance", 3, UniquenessType.Unique)
        .Configure(builder => builder
            .AddHandler(DispatcherType.TakingDamage, FavSoulEnergyRes, DamageType.Fire)
        );

    [FeatCondition(SonicResistance)]
    public static readonly ConditionSpec EnergyResistanceSonicCondition = ConditionSpec.Create("Favored Soul Sonic Resistance", 3, UniquenessType.Unique)
        .Configure(builder => builder
            .AddHandler(DispatcherType.TakingDamage, FavSoulEnergyRes, DamageType.Sonic)
        );

    #endregion
}