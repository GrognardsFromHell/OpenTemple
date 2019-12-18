
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
    public class Wizard
    {
        public static readonly Stat ClassId = Stat.level_wizard;

        public static readonly D20ClassSpec ClassSpec = new D20ClassSpec
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
}
