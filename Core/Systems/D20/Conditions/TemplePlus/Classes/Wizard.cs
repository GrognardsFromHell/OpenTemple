
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
public class Wizard
{
    public static readonly Stat ClassId = Stat.level_wizard;

    private static readonly ImmutableList<SelectableFeat> BonusFeats = new [] {
        FeatId.BREW_POTION, FeatId.CRAFT_MAGIC_ARMS_AND_ARMOR, FeatId.CRAFT_ROD, FeatId.CRAFT_STAFF,
        FeatId.CRAFT_WAND, FeatId.CRAFT_WONDROUS_ITEM, FeatId.EMPOWER_SPELL, FeatId.ENLARGE_SPELL,
        FeatId.EXTEND_SPELL, FeatId.FORGE_RING, FeatId.HEIGHTEN_SPELL, FeatId.MAXIMIZE_SPELL, FeatId.QUICKEN_SPELL,
        FeatId.SCRIBE_SCROLL, FeatId.SILENT_SPELL, FeatId.STILL_SPELL, FeatId.WIDEN_SPELL
    }.Select(f => new SelectableFeat(f)).ToImmutableList();

    public static readonly D20ClassSpec ClassSpec = new("wizard")
    {
        classEnum = ClassId,
        helpTopic = "TAG_WIZARDS",
        conditionName = "Wizard",
        flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
        BaseAttackBonusProgression = BaseAttackProgressionType.NonMartial,
        hitDice = 4,
        FortitudeSaveProgression = SavingThrowProgressionType.LOW,
        ReflexSaveProgression = SavingThrowProgressionType.LOW,
        WillSaveProgression = SavingThrowProgressionType.HIGH,
        skillPts = 2,
        spellListType = SpellListType.Arcane,
        hasArmoredArcaneCasterFeature = false,
        spellMemorizationType = SpellReadyingType.Vancian,
        spellSourceType = SpellSourceType.Arcane,
        spellCastingConditionName = null,
        spellStat = Stat.intelligence,
        spellDcStat = Stat.intelligence,
        spellsPerDay = new Dictionary<int, IImmutableList<int>>
        {
            [1] = ImmutableList.Create(3, 1),
            [2] = ImmutableList.Create(4, 2),
            [3] = ImmutableList.Create(4, 2, 1),
            [4] = ImmutableList.Create(4, 3, 2),
            [5] = ImmutableList.Create(4, 3, 2, 1),
            [6] = ImmutableList.Create(4, 3, 3, 2),
            [7] = ImmutableList.Create(4, 4, 3, 2, 1),
            [8] = ImmutableList.Create(4, 4, 3, 3, 2),
            [9] = ImmutableList.Create(4, 4, 4, 3, 2, 1),
            [10] = ImmutableList.Create(4, 4, 4, 3, 3, 2),
            [11] = ImmutableList.Create(4, 4, 4, 4, 3, 2, 1),
            [12] = ImmutableList.Create(4, 4, 4, 4, 3, 3, 2),
            [13] = ImmutableList.Create(4, 4, 4, 4, 4, 3, 2, 1),
            [14] = ImmutableList.Create(4, 4, 4, 4, 4, 3, 3, 2),
            [15] = ImmutableList.Create(4, 4, 4, 4, 4, 4, 3, 2, 1),
            [16] = ImmutableList.Create(4, 4, 4, 4, 4, 4, 3, 3, 2),
            [17] = ImmutableList.Create(4, 4, 4, 4, 4, 4, 4, 3, 2, 1),
            [18] = ImmutableList.Create(4, 4, 4, 4, 4, 4, 4, 3, 3, 2),
            [19] = ImmutableList.Create(4, 4, 4, 4, 4, 4, 4, 4, 3, 3),
            [20] = ImmutableList.Create(4, 4, 4, 4, 4, 4, 4, 4, 4, 4)
        }.ToImmutableDictionary(),
        classSkills = new HashSet<SkillId>
        {
            SkillId.concentration,
            SkillId.spellcraft,
            SkillId.alchemy,
            SkillId.craft,
            SkillId.decipher_script,
            SkillId.knowledge_nature,
            SkillId.knowledge_all,
            SkillId.profession,
        }.ToImmutableHashSet(),
        classFeats = new Dictionary<FeatId, int>
        {
            {FeatId.SCRIBE_SCROLL, 1},
            {FeatId.SIMPLE_WEAPON_PROFICIENCY_WIZARD, 1},
            {FeatId.CALL_FAMILIAR, 1},
        }.ToImmutableDictionary(),
        IsSelectingFeatsOnLevelUp = critter =>
        {
            var newLvl = critter.GetStat(ClassSpec.classEnum) + 1;
            return newLvl % 5 == 0;
        },
        LevelupGetBonusFeats = critter => BonusFeats
    };

    [TempleDllLocation(0x102f04a0)]
    public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
        .AddHandler(DispatcherType.GetBaseCasterLevel, ClassConditions.GrantClassLevelAsCasterLevel, Stat.level_wizard)
        .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Activate, OnInitLevelupSpellSelection)
        .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Finalize, OnLevelupSpellsFinalize)
        .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Check_Complete, OnLevelupSpellsCheckComplete)
        .AddQueryHandler(D20DispatcherKey.QUE_Get_Arcane_Spell_Failure, ArcaneSpellFailure)
        .Build();

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

    public static void ArcaneSpellFailure(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20Query();
        if (dispIo.data1 != (int) ClassId && dispIo.data1 != (int) Stat.level_sorcerer)
        {
            return;
        }

        var equip_slot = (EquipSlot) dispIo.data2;
        var item = evt.objHndCaller.ItemWornAt(equip_slot);
        if (item == null)
        {
            return;
        }

        dispIo.return_val += item.GetInt(obj_f.armor_arcane_spell_failure);
    }
}