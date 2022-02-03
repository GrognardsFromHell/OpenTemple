
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
    public static class Bard
    {
        public const Stat ClassId = Stat.level_bard;

        public static readonly D20ClassSpec ClassSpec = new D20ClassSpec("bard")
            {
                classEnum = ClassId,
                helpTopic = "TAG_BARDS",
                conditionName = "Bard",
                flags = ClassDefinitionFlag.CDF_BaseClass | ClassDefinitionFlag.CDF_CoreClass,
                BaseAttackBonusProgression = BaseAttackProgressionType.SemiMartial,
                hitDice = 6,
                FortitudeSaveProgression = SavingThrowProgressionType.LOW,
                ReflexSaveProgression = SavingThrowProgressionType.HIGH,
                WillSaveProgression = SavingThrowProgressionType.HIGH,
                skillPts = 6,
                spellListType = SpellListType.Bardic,
                hasArmoredArcaneCasterFeature = true,
                spellMemorizationType = SpellReadyingType.Innate,
                spellSourceType = SpellSourceType.Arcane,
                spellCastingConditionName = null,
                spellsPerDay = new Dictionary<int, IImmutableList<int>>
                {
                    [1] = ImmutableList.Create(2),
                    [2] = ImmutableList.Create(3, 0),
                    [3] = ImmutableList.Create(3, 1),
                    [4] = ImmutableList.Create(3, 2, 0),
                    [5] = ImmutableList.Create(3, 3, 1),
                    [6] = ImmutableList.Create(3, 3, 2),
                    [7] = ImmutableList.Create(3, 3, 2, 0),
                    [8] = ImmutableList.Create(3, 3, 3, 1),
                    [9] = ImmutableList.Create(3, 3, 3, 2),
                    [10] = ImmutableList.Create(3, 3, 3, 2, 0),
                    [11] = ImmutableList.Create(3, 3, 3, 3, 1),
                    [12] = ImmutableList.Create(3, 3, 3, 3, 2),
                    [13] = ImmutableList.Create(3, 3, 3, 3, 2, 0),
                    [14] = ImmutableList.Create(4, 3, 3, 3, 3, 1),
                    [15] = ImmutableList.Create(4, 4, 3, 3, 3, 2),
                    [16] = ImmutableList.Create(4, 4, 4, 3, 3, 2, 0),
                    [17] = ImmutableList.Create(4, 4, 4, 4, 3, 3, 1),
                    [18] = ImmutableList.Create(4, 4, 4, 4, 4, 3, 2),
                    [19] = ImmutableList.Create(4, 4, 4, 4, 4, 4, 3),
                    [20] = ImmutableList.Create(4, 4, 4, 4, 4, 4, 4)
                }.ToImmutableDictionary(),
                classSkills = new HashSet<SkillId>
                {
                    SkillId.appraise,
                    SkillId.bluff,
                    SkillId.concentration,
                    SkillId.diplomacy,
                    SkillId.gather_information,
                    SkillId.hide,
                    SkillId.listen,
                    SkillId.move_silently,
                    SkillId.pick_pocket,
                    SkillId.sense_motive,
                    SkillId.spellcraft,
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
                    SkillId.jump,
                    SkillId.knowledge_nature,
                    SkillId.knowledge_all,
                    SkillId.profession,
                    SkillId.swim,
                }.ToImmutableHashSet(),
                classFeats = new Dictionary<FeatId, int>
                {
                    {FeatId.SHIELD_PROFICIENCY, 1},
                    {FeatId.BARDIC_MUSIC, 1},
                    {FeatId.BARDIC_KNOWLEDGE, 1},
                    {FeatId.SIMPLE_WEAPON_PROFICIENCY_BARD, 1},
                }.ToImmutableDictionary(),
            };

        [TempleDllLocation(0x102effc8)]
        public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
            .AddHandler(DispatcherType.GetBaseCasterLevel, ClassConditions.GrantClassLevelAsCasterLevel, Stat.level_bard)
            .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Activate, OnInitLevelupSpellSelection)
            .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Finalize, OnLevelupSpellsFinalize)
            .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Check_Complete, OnLevelupSpellsCheckComplete)
            .AddQueryHandler(D20DispatcherKey.QUE_Get_Arcane_Spell_Failure, BardSpellFailure)
            .Build();

        public static void OnInitLevelupSpellSelection(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            // print "Bard OnInitLevelupSpellSelection: Called with arg0 " + str(dispIo.arg0)
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
            // print "Bard OnLevelupSpellsCheckComplete: Called with arg0 " + str(dispIo.arg0)
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
            // print "Bard OnLevelupSpellsFinalize: Called with arg0 " + str(dispIo.arg0)
            if (dispIo.arg0 != ClassId)
            {
                return;
            }

            throw new NotImplementedException();
            // classSpecModule.LevelupSpellsFinalize(evt.objHndCaller);
        }

        public static void BardSpellFailure(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if (dispIo.data1 != (int) ClassId)
            {
                return;
            }

            var equip_slot = (EquipSlot) dispIo.data2;
            var item = evt.objHndCaller.ItemWornAt(equip_slot);
            if (item == null)
            {
                return;
            }

            if (equip_slot == EquipSlot.Armor) // bards can cast in light armor with no spell failure
            {
                var armor_flags = item.GetArmorFlags().GetArmorType();
                if (armor_flags == ArmorFlag.TYPE_NONE || armor_flags == ArmorFlag.TYPE_LIGHT)
                {
                    return;
                }

                if (evt.objHndCaller.D20Query("Improved Armored Casting") && armor_flags == ArmorFlag.TYPE_MEDIUM)
                {
                    return;
                }
            }

            dispIo.return_val += item.GetInt(obj_f.armor_arcane_spell_failure);
        }

    }
}
