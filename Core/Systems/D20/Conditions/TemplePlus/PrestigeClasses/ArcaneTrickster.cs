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
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.D20.Classes.Prereq;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    [AutoRegister]
    public class ArcaneTrickster
    {
        public const Stat ClassId = Stat.level_arcane_trickster;

        public static readonly D20ClassSpec ClassSpec = new D20ClassSpec("arcane_trickster")
        {
            classEnum = ClassId,
            helpTopic = "TAG_ARCANE_TRICKSTERS",
            conditionName = "Arcane Trickster",
            category = "Core 3.5 Ed Prestige Classes",
            flags = ClassDefinitionFlag.CDF_CoreClass,
            BaseAttackBonusProgression = BaseAttackProgressionType.NonMartial,
            hitDice = 4,
            FortitudeSaveProgression = SavingThrowProgressionType.LOW,
            ReflexSaveProgression = SavingThrowProgressionType.HIGH,
            WillSaveProgression = SavingThrowProgressionType.HIGH,
            skillPts = 4,
            spellListType = SpellListType.Extender,
            spellSourceType = SpellSourceType.Arcane,
            hasArmoredArcaneCasterFeature = false,
            spellMemorizationType = SpellReadyingType.Vancian,
            spellCastingConditionName = "Arcane Trickster Spellcasting",
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
                SkillId.alchemy,
                SkillId.balance,
                SkillId.climb,
                SkillId.craft,
                SkillId.decipher_script,
                SkillId.disguise,
                SkillId.escape_artist,
                SkillId.jump,
                SkillId.knowledge_all,
                SkillId.profession,
                SkillId.swim,
                SkillId.use_rope,
            }.ToImmutableHashSet(),
            classFeats = new Dictionary<FeatId, int> {{FeatId.SNEAK_ATTACK, 2}}.ToImmutableDictionary(),
            Requirements =
            {
                AlignmentPrereqs.NonLawful,
                ClassPrereqs.SkillRanks(SkillId.disable_device, 7),
                ClassPrereqs.SneakAttack(2),
                ClassPrereqs.ArcaneSpellCaster(3)
            }
        };

        private static readonly D20DispatcherKey impromptuSneakEnum = (D20DispatcherKey) 1900;

        public static void ArcTrkSneakAttackDice(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var arcTrkLvl = evt.objHndCaller.GetStat(ClassId);
            dispIo.return_val += arcTrkLvl / 2;
        }

        [PythonAction]
        public static PythonActionSpec ImpromptuSneakAttackAction = new PythonActionSpec
        {
            flags = D20ADF.D20ADF_None,
            tgtClass = D20TargetClassification.Target0,
            name = "Impromptu Sneak Attack",
            costType = ActionCostType.Null,
            OnAddToSequence = (action, sequence, status) =>
            {
                sequence.d20ActArray.Add(action);
                return ActionErrorCode.AEC_OK;
            }
        };

        public static void ImpromptuSneakAttackRadial(in DispatcherCallbackArgs evt)
        {
            evt.objHndCaller.AddCondition("Impromptu Sneak Attack", 0, 0);
            var arcTrkLvl = evt.objHndCaller.GetStat(ClassId);
            if (arcTrkLvl - 3 < 0)
            {
                return;
            }

            var radialAction = RadialMenuEntry.CreatePythonAction(impromptuSneakEnum,
                0, "TAG_INTERFACE_HELP");
            radialAction.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Class);
        }

        public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
            .AddQueryHandler("Sneak Attack Dice", ArcTrkSneakAttackDice)
            .AddHandler(DispatcherType.RadialMenuEntry, ImpromptuSneakAttackRadial)
            .Build();

        private static Stat GetExtendedClass(in DispatcherCallbackArgs evt)
        {
            if (evt.GetConditionArg1() == 0)
            {
                return D20ClassSystem.GetHighestArcaneCastingClass(evt.objHndCaller);
            }

            return (Stat) evt.GetConditionArg1();
        }

        // Spell casting configure the spell casting condition to hold the highest Arcane classs
        public static void OnAddSpellCasting(in DispatcherCallbackArgs evt)
        {
            // arg0 holds the arcane class
            if (evt.GetConditionArg1() == 0)
            {
                evt.SetConditionArg1((int) D20ClassSystem.GetHighestArcaneCastingClass(evt.objHndCaller));
            }
        }

        // Extend caster level for base casting class
        public static void OnGetBaseCasterLevel(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            var class_extended_1 = GetExtendedClass(in evt);
            var class_code = dispIo.arg0;
            if (class_code != class_extended_1)
            {
                if (dispIo.arg1 == 0) // arg1 != 0 means you're looking for this particular class's contribution
                {
                    return;
                }
            }

            var classLvl = evt.objHndCaller.GetStat(ClassId);
            dispIo.bonlist.AddBonus(classLvl, 0, 137);
        }

        public static void OnSpellListExtensionGet(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            var class_extended_1 = GetExtendedClass(in evt);
            var class_code = dispIo.arg0;
            if (class_code != class_extended_1)
            {
                // arg1 != 0 means you're looking for this particular class's contribution
                if (dispIo.arg1 == 0)
                {
                    return;
                }
            }

            var classLvl = evt.objHndCaller.GetStat(ClassId);
            dispIo.bonlist.AddBonus(classLvl, 0, 137);
        }

        public static void OnInitLevelupSpellSelection(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if (dispIo.arg0 == ClassId)
            {
                var class_extended_1 = GetExtendedClass(in evt);
                throw new NotImplementedException();
                // classSpecModule.InitSpellSelection(evt.objHndCaller, class_extended_1);
            }
        }

        public static void OnLevelupSpellsCheckComplete(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if (dispIo.arg0 == ClassId)
            {
                var class_extended_1 = GetExtendedClass(in evt);
                throw new NotImplementedException();
                // if (!classSpecModule.LevelupCheckSpells(evt.objHndCaller, class_extended_1))
                // {
                //     dispIo.bonlist.AddBonus(-1, 0, 137); // denotes incomplete spell selection
                // }
            }
        }

        public static void OnLevelupSpellsFinalize(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if (dispIo.arg0 == ClassId)
            {
                var class_extended_1 = GetExtendedClass(in evt);
                throw new NotImplementedException();
                // classSpecModule.LevelupSpellsFinalize(evt.objHndCaller, class_extended_1);
            }
        }

        public static readonly ConditionSpec spellCasterSpecObj = ConditionSpec
            .Create(ClassSpec.spellCastingConditionName, 8)
            .AddHandler(DispatcherType.ConditionAdd, OnAddSpellCasting)
            .AddHandler(DispatcherType.GetBaseCasterLevel, OnGetBaseCasterLevel)
            .AddHandler(DispatcherType.SpellListExtension, OnSpellListExtensionGet)
            .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Activate,
                OnInitLevelupSpellSelection)
            .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Check_Complete,
                OnLevelupSpellsCheckComplete)
            .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Finalize,
                OnLevelupSpellsFinalize)
            .Build();

        #region Impromptu Sneak Attack

        public static bool IsActive(in DispatcherCallbackArgs args) => args.GetConditionArg1() != 0;

        public static void ImpSneakDamageIsActive(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if (evt.GetConditionArg3() != 0)
            {
                dispIo.return_val = 1;
            }
        }

        public static void ImpSneakDamagedApplied(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            evt.SetConditionArg3(0); // unset
        }

        public static void ImpSneakAttackRollMade(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if (IsActive(in evt))
            {
                evt.SetConditionArg1(0);
                evt.SetConditionArg3(1);
            }
        }

        public static void ImpSneakDexterityNullifier(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if (IsActive(in evt))
            {
                dispIo.bonlist.AddCap(3, 0, 202);
            }
        }

        public static void ImpSneakNewday(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg1(0);
            evt.SetConditionArg2(0);
            evt.SetConditionArg3(0);
        }

        public static void OnImpSneakCheck(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            if (IsActive(in evt))
            {
                dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                return;
            }

            // check if enough usages / day left
            var arcTrkLvl = evt.objHndCaller.GetStat(ClassId);
            var maxNumPerDay = 1 + (arcTrkLvl - 3) / 4;
            if (evt.GetConditionArg2() >= maxNumPerDay)
            {
                dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_CHARGES;
            }
        }

        public static void OnImpSneakPerform(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            if (IsActive(in evt))
            {
                return;
            }

            evt.SetConditionArg1(1); // set to active
            evt.SetConditionArg2(evt.GetConditionArg2() + 1); // increment number used / day
            evt.SetConditionArg3(0); // reset expecting damage state
            evt.objHndCaller.FloatLine("Sneak Attacking", TextFloaterColor.Red);
        }
        // arg0 - is active; arg1 - times spent; arg2 - anticipate damage roll with sneak attack
        public static readonly ConditionSpec ImpromptuSneakAttack = ConditionSpec.Create("Impromptu Sneak Attack", 3)
            .AddHandler(DispatcherType.D20Query, D20DispatcherKey.QUE_OpponentSneakAttack,
                ImpSneakDamageIsActive) // gets triggered at the end of the damage calculation
            .AddHandler(DispatcherType.D20Signal, D20DispatcherKey.SIG_Attack_Made,
                ImpSneakDamagedApplied) // signifies that a to hit roll was made
            .AddHandler(DispatcherType.AcModifyByAttacker, ImpSneakAttackRollMade)
            .AddHandler(DispatcherType.AcModifyByAttacker, ImpSneakDexterityNullifier)
            .AddHandler(DispatcherType.PythonActionCheck, impromptuSneakEnum, OnImpSneakCheck)
            .AddHandler(DispatcherType.PythonActionPerform, impromptuSneakEnum, OnImpSneakPerform)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, ImpSneakNewday)
            .Build();

        #endregion
    }
}