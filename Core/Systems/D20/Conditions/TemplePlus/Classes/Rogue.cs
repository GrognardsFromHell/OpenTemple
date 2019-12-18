using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
using SpicyTemple.Core.Systems.D20.Classes;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{
    [AutoRegister]
    public class Rogue
    {
        public static readonly Stat ClassId = Stat.level_rogue;

        public static readonly D20ClassSpec ClassSpec = new D20ClassSpec
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
}