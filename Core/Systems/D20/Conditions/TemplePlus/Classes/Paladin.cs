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

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus;

[AutoRegister]
public class Paladin
{
    public static readonly Stat ClassId = Stat.level_paladin;

    public static readonly D20ClassSpec ClassSpec = new("paladin")
    {
        classEnum = ClassId,
        helpTopic = "TAG_PALADINS",
        conditionName = "Paladin",
        flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
        BaseAttackBonusProgression = BaseAttackProgressionType.Martial,
        hitDice = 10,
        FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
        ReflexSaveProgression = SavingThrowProgressionType.LOW,
        WillSaveProgression = SavingThrowProgressionType.LOW,
        skillPts = 2,
        spellListType = SpellListType.Paladin,
        hasArmoredArcaneCasterFeature = false,
        spellMemorizationType = SpellReadyingType.Vancian,
        spellSourceType = SpellSourceType.Divine,
        spellCastingConditionName = "Paladin Spellcasting",
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
            SkillId.diplomacy,
            SkillId.heal,
            SkillId.sense_motive,
            SkillId.alchemy,
            SkillId.craft,
            SkillId.handle_animal,
            SkillId.knowledge_religion,
            SkillId.profession,
            SkillId.ride,
        }.ToImmutableHashSet(),
        classFeats = new Dictionary<FeatId, int>
        {
            {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
            {FeatId.ARMOR_PROFICIENCY_MEDIUM, 1},
            {FeatId.ARMOR_PROFICIENCY_HEAVY, 1},
            {FeatId.SHIELD_PROFICIENCY, 1},
            {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
            {FeatId.SMITE_EVIL, 1},
            {FeatId.DETECT_EVIL, 1},
            {FeatId.CODE_OF_CONDUCT, 1},
            {FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1},
            {FeatId.LAY_ON_HANDS, 2},
            {FeatId.DIVINE_GRACE, 2},
            {FeatId.AURA_OF_COURAGE, 3},
            {FeatId.DIVINE_HEALTH, 3},
            {FeatId.TURN_UNDEAD, 4},
            {FeatId.SPECIAL_MOUNT, 5},
            {FeatId.REMOVE_DISEASE, 6},
        }.ToImmutableDictionary(),
    };

    [TempleDllLocation(0x102f0260)]
    public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
        .AddHandler(DispatcherType.GetBaseCasterLevel, ClassConditions.GetRangerOrPaladinCasterLevel,
            Stat.level_paladin)
        .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Finalize,
            OnLevelupSpellsFinalize)
        .Build();

    public static void OnLevelupSpellsFinalize(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetEvtObjSpellCaster();
        if ((dispIo.arg0 != ClassId))
        {
            return;
        }

        throw new NotImplementedException();
        // classSpecModule.LevelupSpellsFinalize (evt.objHndCaller);
    }
}