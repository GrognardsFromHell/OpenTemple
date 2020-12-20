
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Classes;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{

    [AutoRegister]
    public class MysticTheurge
    {

        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public const Stat ClassId = Stat.level_mystic_theurge;

        public static readonly D20ClassSpec ClassSpec = new D20ClassSpec("mystic_theurge")
        {
            classEnum = ClassId,
            helpTopic = "TAG_MYSTIC_THEURGES",
            conditionName = "Mystic Theurge",
            flags = ClassDefinitionFlag.CDF_CoreClass,
            BaseAttackBonusProgression = BaseAttackProgressionType.NonMartial,
            hitDice = 4,
            FortitudeSaveProgression = SavingThrowProgressionType.LOW,
            ReflexSaveProgression = SavingThrowProgressionType.LOW,
            WillSaveProgression = SavingThrowProgressionType.HIGH,
            skillPts = 2,
            spellListType = SpellListType.Extender,
            hasArmoredArcaneCasterFeature = false,
            spellMemorizationType = SpellReadyingType.Vancian,
            spellSourceType = SpellSourceType.Ability,
            spellCastingConditionName = "Mystic Theurge Spellcasting",
            classSkills = new HashSet<SkillId>
            {
                SkillId.concentration,
                SkillId.sense_motive,
                SkillId.spellcraft,
                SkillId.alchemy,
                SkillId.craft,
                SkillId.decipher_script,
                SkillId.knowledge_arcana,
                SkillId.knowledge_religion,
                SkillId.profession,
            }.ToImmutableHashSet(),
        };

        public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
            .Build();

       // Spell casting
       // Mystic Theurge raises the caster level for its two base classes specified in Modifier args 0 & 1
       // configure the spell casting condition to hold the highest two Arcane/Divine classes as chosen-to-be-extended classes

        public static void OnAddSpellCasting(in DispatcherCallbackArgs evt)
        {
            // arg0 holds the arcane class
            if (evt.GetConditionArg1() == 0)
            {
                evt.SetConditionArg1((int) D20ClassSystem.GetHighestArcaneCastingClass(evt.objHndCaller));
            }

            // arg1 holds the divine class
            if (evt.GetConditionArg2() == 0)
            {
                evt.SetConditionArg2((int) D20ClassSystem.GetHighestDivineCastingClass(evt.objHndCaller));
            }
        }

        public static void OnGetBaseCasterLevel(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            var class_extended_1 = (Stat) evt.GetConditionArg1();
            var class_extended_2 = (Stat) evt.GetConditionArg2();
            var class_code = dispIo.arg0;
            if (class_code != class_extended_1 && class_code != class_extended_2)
            {
                if (dispIo.arg1 == 0) // are you specifically looking for the Mystic Theurge caster level?
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
            var class_extended_1 = (Stat) evt.GetConditionArg1();
            var class_extended_2 = (Stat) evt.GetConditionArg2();
            var class_code = dispIo.arg0;
            if ((class_code != class_extended_1 && class_code != class_extended_2))
            {
                if ((dispIo.arg1 == 0)) // are you specifically looking for the Mystic Theurge caster level?
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
            Logger.Info("Mystic Theurge Spell Selection Init");
            if (dispIo.arg0 != ClassId)
            {
                Logger.Info("{0}", dispIo.arg0.ToString());
                Logger.Info("{0}", ClassId.ToString());
                return;
            }

            Logger.Info("Mystic Theurge Spell Selection Init: Confirmed classEnum");
            var class_extended_1 = evt.GetConditionArg1();
            var class_extended_2 = evt.GetConditionArg2();
            Logger.Info("{0}", "Mystic Theurge Spell Selection: Class 1" + class_extended_1.ToString());
            Logger.Info("{0}", "Mystic Theurge Spell Selection: Class 2" + class_extended_2.ToString());

            throw new NotImplementedException();
            // classSpecModule.InitSpellSelection(evt.objHndCaller, class_extended_1, class_extended_2);
        }

        public static void OnLevelupSpellsCheckComplete(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if (dispIo.arg0 != ClassId)
            {
                return;
            }

            var class_extended_1 = evt.GetConditionArg1();
            var class_extended_2 = evt.GetConditionArg2();
            throw new NotImplementedException();
            // if (!classSpecModule.LevelupCheckSpells(evt.objHndCaller, class_extended_1, class_extended_2))
            // {
            //     dispIo.bonlist.AddBonus(-1, 0, 137); // denotes incomplete spell selection
            // }
        }

        public static void OnLevelupSpellsFinalize(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if (dispIo.arg0 != ClassId)
            {
                return;
            }

            var class_extended_1 = evt.GetConditionArg1();
            var class_extended_2 = evt.GetConditionArg2();
            throw new NotImplementedException();
            // classSpecModule.LevelupSpellsFinalize(evt.objHndCaller, class_extended_1, class_extended_2);
        }

        public static readonly ConditionSpec spellCasterSpecObj = ConditionSpec
            .Create(ClassSpec.spellCastingConditionName, 8)
            .SetUnique()
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
    }
}
