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
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    [AutoRegister]
    public class Cleric
    {
        public static readonly Stat ClassId = Stat.level_cleric;

        public static readonly D20ClassSpec ClassSpec = new D20ClassSpec("cleric")
        {
            classEnum = ClassId,
            helpTopic = "TAG_CLERICS",
            conditionName = "Cleric",
            flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
            BaseAttackBonusProgression = BaseAttackProgressionType.SemiMartial,
            hitDice = 8,
            FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
            ReflexSaveProgression = SavingThrowProgressionType.LOW,
            WillSaveProgression = SavingThrowProgressionType.HIGH,
            skillPts = 2,
            spellListType = SpellListType.Clerical,
            hasArmoredArcaneCasterFeature = false,
            spellMemorizationType = SpellReadyingType.Vancian,
            spellSourceType = SpellSourceType.Divine,
            spellCastingConditionName = null,
            spellStat = Stat.wisdom,
            spellDcStat = Stat.wisdom,
            spellsPerDay = new Dictionary<int, IImmutableList<int>>
            {
                [1] = ImmutableList.Create(3, 1),
                [2] = ImmutableList.Create(4, 2),
                [3] = ImmutableList.Create(4, 2, 1),
                [4] = ImmutableList.Create(5, 3, 2),
                [5] = ImmutableList.Create(5, 3, 2, 1),
                [6] = ImmutableList.Create(5, 3, 3, 2),
                [7] = ImmutableList.Create(6, 4, 3, 2, 1),
                [8] = ImmutableList.Create(6, 4, 3, 3, 2),
                [9] = ImmutableList.Create(6, 4, 4, 3, 2, 1),
                [10] = ImmutableList.Create(6, 4, 4, 3, 3, 2),
                [11] = ImmutableList.Create(6, 5, 4, 4, 3, 2, 1),
                [12] = ImmutableList.Create(6, 5, 4, 4, 3, 3, 2),
                [13] = ImmutableList.Create(6, 5, 5, 4, 4, 3, 2, 1),
                [14] = ImmutableList.Create(6, 5, 5, 4, 4, 3, 3, 2),
                [15] = ImmutableList.Create(6, 5, 5, 5, 4, 4, 3, 2, 1),
                [16] = ImmutableList.Create(6, 5, 5, 5, 4, 4, 3, 3, 2),
                [17] = ImmutableList.Create(6, 5, 5, 5, 5, 4, 4, 3, 2, 1),
                [18] = ImmutableList.Create(6, 5, 5, 5, 5, 4, 4, 3, 3, 2),
                [19] = ImmutableList.Create(6, 5, 5, 5, 5, 5, 4, 4, 3, 3),
                [20] = ImmutableList.Create(6, 5, 5, 5, 5, 5, 4, 4, 4, 4)
            }.ToImmutableDictionary(),
            classSkills = new HashSet<SkillId>
            {
                SkillId.concentration,
                SkillId.diplomacy,
                SkillId.heal,
                SkillId.spellcraft,
                SkillId.alchemy,
                SkillId.craft,
                SkillId.knowledge_arcana,
                SkillId.knowledge_religion,
                SkillId.profession,
            }.ToImmutableHashSet(),
            classFeats = new Dictionary<FeatId, int>
            {
                {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                {FeatId.ARMOR_PROFICIENCY_MEDIUM, 1},
                {FeatId.ARMOR_PROFICIENCY_HEAVY, 1},
                {FeatId.SHIELD_PROFICIENCY, 1},
                {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                {FeatId.DOMAIN_POWER, 1},
            }.ToImmutableDictionary(),
        };

        [TempleDllLocation(0x102f0048)]
        public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
            .AddHandler(DispatcherType.GetBaseCasterLevel, ClassConditions.GrantClassLevelAsCasterLevel,
                Stat.level_cleric)
            .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Finalize,
                OnLevelupSpellsFinalize)
            .Build();

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
    }
}