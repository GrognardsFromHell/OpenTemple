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
using System.Runtime.CompilerServices;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Ui.InGameSelect;
using OpenTemple.Core.Ui.PartyCreation.Systems;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    [AutoRegister]
    public class Warmage
    {
        private static readonly Stat ClassId = Stat.level_warmage;

        public const string LightShieldProficiency = "Light Shield Proficiency";
        public static readonly FeatId LightShieldProficiencyId = (FeatId) ElfHash.Hash(LightShieldProficiency);
        public const string ArmoredMage = "Warmage Armored Mage";
        public static readonly FeatId ArmoredMageId = (FeatId) ElfHash.Hash(ArmoredMage);
        public const string Edge = "Warmage Edge";
        public static readonly FeatId EdgeId = (FeatId) ElfHash.Hash(Edge);

        /*has advanced learning*/
        public static readonly D20ClassSpec ClassSpec = new D20ClassSpec("warmage")
        {
            classEnum = ClassId,
            helpTopic = "TAG_WARMAGES",
            conditionName = "Warmage",
            flags = ClassDefinitionFlag.CDF_BaseClass,
            BaseAttackBonusProgression = BaseAttackProgressionType.NonMartial,
            hitDice = 6,
            FortitudeSaveProgression = SavingThrowProgressionType.LOW,
            ReflexSaveProgression = SavingThrowProgressionType.LOW,
            WillSaveProgression = SavingThrowProgressionType.HIGH,
            skillPts = 2,
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
                SkillId.concentration, SkillId.intimidate, SkillId.spellcraft, SkillId.alchemy,
            }.ToImmutableHashSet(),
            classFeats = new Dictionary<FeatId, int>
            {
                {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                {LightShieldProficiencyId, 1},
                {ArmoredMageId, 1},
                {EdgeId, 1},
                {FeatId.ARMOR_PROFICIENCY_MEDIUM, 8},
            }.ToImmutableDictionary(),
            deityClass = Stat.level_sorcerer,
            IsSelectingFeatsOnLevelUp = critter =>
            {
                var newLvl = critter.GetStat(ClassSpec.classEnum) + 1;
                return (newLvl == 7) || (newLvl == 10) || (newLvl == 15) || (newLvl == 20);
            },
            LevelupGetBonusFeats = GetBonusFeats
        };

        private static IEnumerable<SelectableFeat> GetBonusFeats(GameObject critter)
        {
            var newLvl = critter.GetStat(ClassSpec.classEnum) + 1;
            // Find the normal feat for each level
            FeatId featId;
            if (newLvl == 7)
            {
                featId = SuddenEmpower.Id;
            }
            else if (newLvl == 10)
            {
                featId = SuddenEnlarge.Id;
            }
            else if (newLvl == 15)
            {
                featId = SuddenWiden.Id;
            }
            else if (newLvl == 20)
            {
                featId = SuddenMaximize.Id;
            }
            else
            {
                return Enumerable.Empty<SelectableFeat>(); // No bonus feat this level
            }

            // The only option will be the normal feat if the chracter does not have it
            // TODO: It's not critter, but char editor's HasFeat
            if (!critter.HasFeat(featId))
            {
                return new[]
                {
                    new SelectableFeat(featId)
                    {
                        IsIgnoreRequirements = true
                    }
                };
            }
            else
            {
                // Any metamagic feat can be selected if the character does not have the normal feat
                // If the character already has the feat, he can select any metamagic feat
                return GameSystems.Feat.MetamagicFeats.Select(featId => new SelectableFeat(featId));
            }
        }

        // Spell casting
        public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
            .AddHandler(DispatcherType.GetBaseCasterLevel, OnGetBaseCasterLevel)
            .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Finalize,
                OnLevelupSpellsFinalize)
            .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Activate,
                OnInitLevelupSpellSelection)
            .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Check_Complete,
                OnLevelupSpellsCheckComplete)
            .Build();

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

        // Light Shield Proficiency
        public static void HasLightShieldProficency(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = 1;
            return;
        }

        // Spare, Spare
        [FeatCondition(LightShieldProficiency)]
        public static readonly ConditionSpec lightShieldProficiency = ConditionSpec
            .Create("Light Shield Proficiency", 2)
            .SetUnique()
            .AddQueryHandler("Has Light Shield Proficency", HasLightShieldProficency)
            .Build();

        // Warmage Edge
        // Here is how this complicated ability is implimented.  First it is increated by a critical but not empower
        // Second multi target spells will only get the benefit once.  This will effect the first target that takes damage.
        // Third area area effect spells get the benefit against each object in their area of effect one time (ice
        // storm for example only gets the bonus on the bludgeoning damage).  Fourth multi round spells can get the
        // benefit each round.
        public static void WarmageBeginRound(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            evt.SetConditionArg1(0);
            return;
        }

        public static void WarmageEdgeOnSpellDamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjDealingSpellDamage();
            // Only effects warmage spells
            if (dispIo.spellPkt.spellClass != SpellSystem.GetSpellClass(ClassId))
            {
                return;
            }

            var prevSpellID = evt.GetConditionArg1();
            var spellID = dispIo.spellPkt.spellId;
            var spEntry = GameSystems.Spell.GetSpellEntry(dispIo.spellPkt.spellEnum);
            var multiTarget = spEntry.IsBaseModeTarget(UiPickerType.Multi);
            var target = dispIo.target;
            if (multiTarget)
            {
                // If the same multi target is doing damage again, no bonus
                if (prevSpellID == spellID)
                {
                    return;
                }
            }
            else if (dispIo.spellPkt.spellEnum != WellKnownSpells.MelfsAcidArrow) // Always give the bonus to acid arrow
            {
                if (target.D20Query("Warmage Edge Damage Taken", spellID))
                {
                    return;
                }
            }

            var intMod = evt.objHndCaller.GetStat(Stat.int_mod);
            // Increase warmage edge damage on a critical hit
            if (dispIo.damage.critHitMultiplier > 1)
            {
                intMod = intMod * 2;
            }

            if (intMod > 0)
            {
                dispIo.damage.bonuses.AddBonusFromFeat(intMod, 0, 137, (FeatId) ElfHash.Hash("Warmage Edge"));
            }

            evt.SetConditionArg1(spellID);
            target.AddCondition(warmageEdgeDamage, spellID);
        }

        // Previous Spell ID, Spare
        [FeatCondition(Edge)]
        public static readonly ConditionSpec warmageEdge = ConditionSpec.Create("Warmage Edge", 2)
            .SetUnique()
            .AddHandler(DispatcherType.DealingDamageSpell, WarmageEdgeOnSpellDamage)
            .AddHandler(DispatcherType.BeginRound, WarmageBeginRound)
            .Build();

        // Warmage edge damage effect
        public static void WarmageDamageBeginRound(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            var spellID = evt.GetConditionArg1();
            evt.RemoveThisCondition(); // Always disapears at the begining of the round
            return;
        }

        public static void TakenWarmageEdgeDamageFromSpellQuery(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var spellID = evt.GetConditionArg1();
            var querySpellID = dispIo.data1;
            if (spellID == querySpellID)
            {
                dispIo.return_val = 1;
            }

            return;
        }

        // Previous Spell ID, Spare
        public static readonly ConditionSpec warmageEdgeDamage = ConditionSpec.Create("Warmage Edge Damage", 2)
            .AddHandler(DispatcherType.BeginRound, WarmageDamageBeginRound)
            .AddQueryHandler("Warmage Edge Damage Taken", TakenWarmageEdgeDamageFromSpellQuery)
            .Build();

        // Armored Mage

        public static void WarmageSpellFailure(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // Only effects spells cast as a warmage
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

            if (equip_slot == EquipSlot.Armor
            ) // warmage can cast in light armor (and medium armor at level 8 or greater) with no spell failure
            {
                var warmageLevel = evt.objHndCaller.GetStat(ClassId);
                var armorType = item.GetArmorFlags().GetArmorType();

                // Improved armor casting allows to wear one level heavier than normal
                if (evt.objHndCaller.D20Query("Improved Armored Casting"))
                {
                    if (armorType == ArmorFlag.TYPE_NONE || armorType == ArmorFlag.TYPE_LIGHT ||
                        armorType == ArmorFlag.TYPE_MEDIUM || warmageLevel > 7)
                    {
                        return;
                    }
                }
                else
                {
                    if ((armorType == ArmorFlag.TYPE_NONE) || (armorType == ArmorFlag.TYPE_LIGHT) ||
                        (armorType == ArmorFlag.TYPE_MEDIUM) && (warmageLevel > 7))
                    {
                        return;
                    }
                }
            }

            if (equip_slot == EquipSlot.Shield
            ) // warmage can cast with a light shield (or buclker) with no spell failure
            {
                var shieldFailure = item.GetInt(obj_f.armor_arcane_spell_failure);
                if (shieldFailure <= 5) // Light shields and bucklers have 5% spell failure
                {
                    return;
                }
            }

            dispIo.return_val += item.GetInt(obj_f.armor_arcane_spell_failure);
            return;
        }

        // Spare, Spare
        [FeatCondition(ArmoredMage)]
        public static readonly ConditionSpec armoredMage = ConditionSpec.Create("Warmage Armored Mage", 2)
            .SetUnique()
            .AddHandler(DispatcherType.D20Query, D20DispatcherKey.QUE_Get_Arcane_Spell_Failure, WarmageSpellFailure)
            .Build();
    }
}