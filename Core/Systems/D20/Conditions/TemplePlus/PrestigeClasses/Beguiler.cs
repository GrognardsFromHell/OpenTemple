using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    [AutoRegister]
    public class Beguiler
    {
        public const string ArmoredMage = "Beguiler Armored Mage";
        public static readonly FeatId ArmoredMageId = (FeatId) ElfHash.Hash(ArmoredMage);
        public const string SurpriseCasting = "Surprise Casting";
        public static readonly FeatId SurpriseCastingId = (FeatId) ElfHash.Hash(SurpriseCasting);
        public const string CloakedCasting = "Cloaked Casting";
        public static readonly FeatId CloakedCastingId = (FeatId) ElfHash.Hash(CloakedCasting);

        private const Stat ClassId = Stat.level_beguilers;

        /*has advanced learning*/
        public static readonly D20ClassSpec ClassSpec = new D20ClassSpec("beguiler")
        {
            classEnum = ClassId,
            helpTopic = "TAG_BEGUILERS",
            conditionName = "Beguiler",
            flags = ClassDefinitionFlag.CDF_BaseClass,
            BaseAttackBonusProgression = BaseAttackProgressionType.NonMartial,
            hitDice = 6,
            FortitudeSaveProgression = SavingThrowProgressionType.LOW,
            ReflexSaveProgression = SavingThrowProgressionType.LOW,
            WillSaveProgression = SavingThrowProgressionType.HIGH,
            skillPts = 6,
            spellListType = SpellListType.Special,
            hasArmoredArcaneCasterFeature = true,
            spellMemorizationType = SpellReadyingType.Innate,
            spellSourceType = SpellSourceType.Arcane,
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
                [20] = ImmutableList.Create(6, 6, 6, 6, 6, 6, 6, 6, 6, 5)
            }.ToImmutableDictionary(),
            classSkills = new HashSet<SkillId>
            {
                SkillId.appraise,
                SkillId.bluff,
                SkillId.concentration,
                SkillId.diplomacy,
                SkillId.disable_device,
                SkillId.gather_information,
                SkillId.hide,
                SkillId.listen,
                SkillId.move_silently,
                SkillId.open_lock,
                SkillId.pick_pocket,
                SkillId.search,
                SkillId.sense_motive,
                SkillId.spellcraft,
                SkillId.spot,
                SkillId.tumble,
                SkillId.use_magic_device,
                SkillId.alchemy,
                SkillId.balance,
                SkillId.climb,
                SkillId.decipher_script,
                SkillId.disguise,
                SkillId.escape_artist,
                SkillId.forgery,
                SkillId.jump,
                SkillId.swim,
                SkillId.use_rope,
            }.ToImmutableHashSet(),
            classFeats = new Dictionary<FeatId, int>
            {
                {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                {FeatId.EXOTIC_WEAPON_PROFICIENCY_HAND_CROSSBOW, 1},
                {FeatId.MARTIAL_WEAPON_PROFICIENCY_SHORT_SWORD, 1},
                {FeatId.MARTIAL_WEAPON_PROFICIENCY_RAPIER, 1},
                {FeatId.MARTIAL_WEAPON_PROFICIENCY_SHORTBOW, 1},
                {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                {FeatId.TRAPS, 1},
                {ArmoredMageId, 1},
                {CloakedCastingId, 2},
                {SurpriseCastingId, 2},
                {FeatId.SILENT_SPELL, 5},
                {FeatId.STILL_SPELL, 10},
            }.ToImmutableDictionary(),
            deityClass = Stat.level_rogue
        };

        public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
            .AddHandler(DispatcherType.GetBaseCasterLevel, OnGetBaseCasterLevel)
            .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Finalize,
                OnLevelupSpellsFinalize)
            .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Activate,
                OnInitLevelupSpellSelection)
            .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Check_Complete,
                OnLevelupSpellsCheckComplete)
            .Build();

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

        public static void OnLevelupSpellsFinalize(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if (dispIo.arg0 != ClassId)
            {
                return;
            }

            throw new NotImplementedException();
            // classSpecModule.LevelupSpellsFinalize(evt.objHndCaller);
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
            return;
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

        #region Armored Mage Beguiler

        public static void BeguilerSpellFailure(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // Only effects spells cast as a beguiler
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

            // beguiler can cast in light armor (and medium armor at level 8 or greater) with no spell failure
            if (equip_slot == EquipSlot.Armor)
            {
                var armor_flags = item.GetArmorFlags().GetArmorType();
                if (armor_flags == ArmorFlag.TYPE_NONE || armor_flags == ArmorFlag.TYPE_LIGHT)
                {
                    return;
                }

                if (evt.objHndCaller.D20Query("Improved Armored Casting") && (armor_flags == ArmorFlag.TYPE_MEDIUM))
                {
                    return;
                }
            }

            dispIo.return_val += item.GetInt(obj_f.armor_arcane_spell_failure);
        }

        // Spare, Spare
        [FeatCondition(ArmoredMage)]
        public static readonly ConditionSpec ArmoredMageCondition = ConditionSpec.Create("Beguiler Armored Mage", 2)
            .SetUnique()
            .AddHandler(DispatcherType.D20Query, D20DispatcherKey.QUE_Get_Arcane_Spell_Failure, BeguilerSpellFailure)
            .Build();

        #endregion

        #region Cloaked Casting

        public static void CloakedCastingDCMod(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellTargetBonus();
            // Check for flat footed or can't sense
            if (GameSystems.Critter.CanSense(dispIo.target, evt.objHndCaller)
                && !dispIo.target.D20Query(D20DispatcherKey.QUE_Flatfooted))
            {
                return;
            }

            var classLvl = evt.objHndCaller.GetStat(ClassId);
            if (classLvl > 13)
            {
                dispIo.bonusList.AddBonus(2, 0, "Cloaked Casting");
            }
            else if (classLvl > 1)
            {
                dispIo.bonusList.AddBonus(1, 0, "Cloaked Casting");
            }
        }

        public static void CloakedCastingResistanceMod(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellTargetBonus();
            // Check for flat footed or can't sense
            if (GameSystems.Critter.CanSense(dispIo.target, evt.objHndCaller)
                && !dispIo.target.D20Query(D20DispatcherKey.QUE_Flatfooted))
            {
                return;
            }

            var classLvl = evt.objHndCaller.GetStat(ClassId);
            if (classLvl > 19)
            {
                dispIo.bonusList.AddBonus(1000, 0, "Cloaked Casting"); // Arbitrary value so the check never fails
            }
            else if (classLvl > 7)
            {
                dispIo.bonusList.AddBonus(2, 0, "Cloaked Casting");
            }

            return;
        }

        // Spare, Spare
        [FeatCondition(CloakedCasting)]
        public static readonly ConditionSpec CloakedCastingCondition = ConditionSpec.Create("Cloaked Casting", 2)
            .SetUnique()
            .AddHandler(DispatcherType.TargetSpellDCBonus, CloakedCastingDCMod)
            .AddHandler(DispatcherType.SpellResistanceCasterLevelCheck, CloakedCastingResistanceMod)
            .Build();

        #endregion

        #region Surprise Casting

        public static void QuickenFeintCostMod(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjActionCost();
            if (dispIo.d20a.d20ActType != D20ActionType.FEINT)
            {
                return;
            }

            // Change to a swift or move equivalent action as appropriate
            if (evt.objHndCaller.HasFeat(FeatId.IMPROVED_FEINT))
            {
                if ((dispIo.tbStat.tbsFlags & TurnBasedStatusFlags.FreeActionSpellPerformed) == 0)
                {
                    // Swift action uses the quickened spell action
                    dispIo.tbStat.tbsFlags |= TurnBasedStatusFlags.FreeActionSpellPerformed;
                    dispIo.acpCur.hourglassCost = ActionCostType.Null;
                }
            }
            else
            {
                dispIo.acpCur.hourglassCost = ActionCostType.Move;
            }
        }

        // Spare, Spare
        [FeatCondition(SurpriseCasting)]
        public static readonly ConditionSpec SurpriseCastingCondition = ConditionSpec.Create("Surprise Casting", 2)
            .SetUnique()
            .AddHandler(DispatcherType.ActionCostMod, QuickenFeintCostMod)
            .Build();

        #endregion
    }
}