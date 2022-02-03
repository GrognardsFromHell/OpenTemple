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
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Ui.PartyCreation.Systems;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus;

[AutoRegister]
public class Rogue
{
    public static readonly Stat ClassId = Stat.level_rogue;

    private static readonly ImmutableList<SelectableFeat> BonusFeats = new [] {
        FeatId.DEFENSIVE_ROLL, FeatId.IMPROVED_EVASION, FeatId.CRIPPLING_STRIKE, FeatId.OPPORTUNIST,
        FeatId.SKILL_MASTERY, FeatId.SLIPPERY_MIND
    }.Select(f => new SelectableFeat(f)).ToImmutableList();

    public static readonly D20ClassSpec ClassSpec = new D20ClassSpec("rogue")
    {
        classEnum = ClassId,
        helpTopic = "TAG_ROGUES",
        conditionName = "Rogue",
        flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
        BaseAttackBonusProgression = BaseAttackProgressionType.SemiMartial,
        hitDice = 6,
        FortitudeSaveProgression = SavingThrowProgressionType.LOW,
        ReflexSaveProgression = SavingThrowProgressionType.HIGH,
        WillSaveProgression = SavingThrowProgressionType.LOW,
        skillPts = 8,
        hasArmoredArcaneCasterFeature = false,
        classSkills = new HashSet<SkillId>
        {
            SkillId.appraise,
            SkillId.bluff,
            SkillId.diplomacy,
            SkillId.disable_device,
            SkillId.gather_information,
            SkillId.hide,
            SkillId.intimidate,
            SkillId.listen,
            SkillId.move_silently,
            SkillId.open_lock,
            SkillId.pick_pocket,
            SkillId.search,
            SkillId.sense_motive,
            SkillId.spot,
            SkillId.tumble,
            SkillId.use_magic_device,
            SkillId.perform,
            SkillId.alchemy,
            SkillId.balance,
            SkillId.climb,
            SkillId.craft,
            SkillId.decipher_script,
            SkillId.disguise,
            SkillId.escape_artist,
            SkillId.forgery,
            SkillId.jump,
            SkillId.profession,
            SkillId.swim,
            SkillId.use_rope,
        }.ToImmutableHashSet(),
        classFeats = new Dictionary<FeatId, int>
        {
            {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
            {FeatId.SIMPLE_WEAPON_PROFICIENCY_ROGUE, 1},
            {FeatId.SNEAK_ATTACK, 1},
            {FeatId.TRAPS, 1},
            {FeatId.EVASION, 2},
            {FeatId.UNCANNY_DODGE, 4},
            {FeatId.IMPROVED_UNCANNY_DODGE, 8},
        }.ToImmutableDictionary(),
        IsSelectingFeatsOnLevelUp = critter =>
        {
            var newLvl = critter.GetStat(ClassSpec.classEnum) + 1;
            return newLvl >= 10 && (newLvl - 10) % 3 == 0;
        },
        LevelupGetBonusFeats = critter => BonusFeats
    };

    [TempleDllLocation(0x102f0360)]
    public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
        .SetQueryResult(D20DispatcherKey.QUE_Critter_Can_Find_Traps, true)
        .AddHandler(DispatcherType.GetAC, ClassConditions.TrapSenseDodgeBonus, Stat.level_rogue)
        .AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_REFLEX,
            ClassConditions.TrapSenseRefSaveBonus, Stat.level_rogue)
        .AddQueryHandler("Sneak Attack Dice", RogueSneakAttackDice)
        .Build();

    public static void RogueSneakAttackDice(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20Query();
        var rogLvl = evt.objHndCaller.GetStat(ClassId);
        if (rogLvl > 0)
        {
            dispIo.return_val += 1 + (rogLvl - 1) / 2;
        }
    }
}