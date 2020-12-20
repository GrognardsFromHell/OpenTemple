
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Classes;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    [AutoRegister]
    public class Ranger
    {

        public static readonly Stat ClassId = Stat.level_ranger;

        public static readonly D20ClassSpec ClassSpec = new D20ClassSpec("ranger")
            {
                classEnum = ClassId,
                helpTopic = "TAG_RANGERS",
                conditionName = "Ranger",
                flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.Martial,
                hitDice = 8,
                FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
                ReflexSaveProgression = SavingThrowProgressionType.LOW,
                WillSaveProgression = SavingThrowProgressionType.LOW,
                skillPts = 6,
                spellListType = SpellListType.Ranger,
                hasArmoredArcaneCasterFeature = false,
                spellMemorizationType = SpellReadyingType.Vancian,
                spellSourceType = SpellSourceType.Divine,
                spellCastingConditionName = null,
                spellsPerDay = new Dictionary<int, IImmutableList<int>>
                {
                    [1] = ImmutableList.Create(-1),
                    [2] = ImmutableList.Create(-1),
                    [3] = ImmutableList.Create(-1),
                    [4] = ImmutableList.Create(-1, 0),
                    [5] = ImmutableList.Create(-1, 0),
                    [6] = ImmutableList.Create(-1, 1),
                    [7] = ImmutableList.Create(-1, 1),
                    [8] = ImmutableList.Create(-1, 1, 0),
                    [9] = ImmutableList.Create(-1, 1, 0),
                    [10] = ImmutableList.Create(-1, 1, 1),
                    [11] = ImmutableList.Create(-1, 1, 1, 0),
                    [12] = ImmutableList.Create(-1, 1, 1, 1),
                    [13] = ImmutableList.Create(-1, 1, 1, 1),
                    [14] = ImmutableList.Create(-1, 2, 1, 1, 0),
                    [15] = ImmutableList.Create(-1, 2, 1, 1, 1),
                    [16] = ImmutableList.Create(-1, 2, 2, 1, 1),
                    [17] = ImmutableList.Create(-1, 2, 2, 2, 1),
                    [18] = ImmutableList.Create(-1, 3, 2, 2, 1),
                    [19] = ImmutableList.Create(-1, 3, 3, 3, 2),
                    [20] = ImmutableList.Create(-1, 3, 3, 3, 3)
                }.ToImmutableDictionary(),
                classSkills = new HashSet<SkillId>
                {
                    SkillId.concentration,
                    SkillId.heal,
                    SkillId.hide,
                    SkillId.listen,
                    SkillId.move_silently,
                    SkillId.search,
                    SkillId.spot,
                    SkillId.wilderness_lore,
                    SkillId.alchemy,
                    SkillId.climb,
                    SkillId.craft,
                    SkillId.handle_animal,
                    SkillId.jump,
                    SkillId.knowledge_nature,
                    SkillId.profession,
                    SkillId.ride,
                    SkillId.swim,
                    SkillId.use_rope,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                    {FeatId.SHIELD_PROFICIENCY, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                    {FeatId.TRACK, 1},
                    {FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1},
                    {FeatId.ANIMAL_COMPANION, 4},
                    {FeatId.EVASION, 9},
                }.ToImmutableDictionary(),
            };

        [TempleDllLocation(0x102f02e0)]
        public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
            .AddHandler(DispatcherType.GetBaseCasterLevel, ClassConditions.GetRangerOrPaladinCasterLevel, Stat.level_ranger)
            .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Finalize, OnLevelupSpellsFinalize)
            .Build();

        public static void OnLevelupSpellsFinalize(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if ((dispIo.arg0 != ClassId))
            {
                return;
            }

            throw new NotImplementedException();
            // classSpecModule.LevelupSpellsFinalize(evt.objHndCaller);
        }

    }
}
