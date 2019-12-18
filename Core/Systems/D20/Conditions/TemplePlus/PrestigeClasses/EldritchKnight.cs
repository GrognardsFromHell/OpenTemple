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
    public class EldritchKnight
    {
        public const Stat ClassId = Stat.level_eldritch_knight;

        public static readonly D20ClassSpec ClassSpec = new D20ClassSpec
        {
            classEnum = Stat.level_eldritch_knight,
            helpTopic = "TAG_ELDRITCH_KNIGHTS",
            conditionName = "Eldritch Knight",
            flags = ClassDefinitionFlag.CDF_CoreClass,
            BaseAttackBonusProgression = BaseAttackProgressionType.Martial,
            hitDice = 6,
            FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
            ReflexSaveProgression = SavingThrowProgressionType.LOW,
            WillSaveProgression = SavingThrowProgressionType.LOW,
            skillPts = 2,
            spellListType = SpellListType.Extender,
            hasArmoredArcaneCasterFeature = false,
            spellMemorizationType = SpellReadyingType.Vancian,
            spellSourceType = SpellSourceType.Ability,
            spellCastingConditionName = "Eldritch Knight Spellcasting",
            classSkills = new HashSet<SkillId>
            {
                SkillId.concentration,
                SkillId.sense_motive,
                SkillId.spellcraft,
                SkillId.alchemy,
                SkillId.craft,
                SkillId.decipher_script,
                SkillId.jump,
                SkillId.knowledge_arcana,
                SkillId.ride,
                SkillId.swim,
            }.ToImmutableHashSet(),
        };

        // Spell casting configure the spell casting condition to hold the highest Arcane classs
        public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
            .Build();

        public static void OnAddSpellCasting(in DispatcherCallbackArgs evt)
        {
            // arg0 holds the arcane class
            if ((evt.GetConditionArg1() == 0))
            {
                evt.SetConditionArg1((int) D20ClassSystem.GetHighestArcaneCastingClass(evt.objHndCaller));
            }
        }

        // Extend caster level for base casting class
        public static void OnGetBaseCasterLevel(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            var class_extended_1 = (Stat) evt.GetConditionArg1();
            var class_code = dispIo.arg0;
            if ((class_code != class_extended_1))
            {
                if (dispIo.arg1 == 0) // arg1 != 0 means you're looking for this particular class's contribution
                {
                    return;
                }
            }

            var classLvl = evt.objHndCaller.GetStat(ClassId);
            if (classLvl > 1)
            {
                dispIo.bonlist.AddBonus(classLvl - 1, 0, 137);
            }

            return;
        }

        public static void OnSpellListExtensionGet(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            var class_extended_1 = (Stat) evt.GetConditionArg1();
            var class_code = dispIo.arg0;
            if ((class_code != class_extended_1))
            {
                if (dispIo.arg1 == 0) // arg1 != 0 means you're looking for this particular class's contribution
                {
                    return;
                }
            }

            var classLvl = evt.objHndCaller.GetStat(ClassId);
            if (classLvl > 1)
            {
                dispIo.bonlist.AddBonus(classLvl - 1, 0, 137);
            }

            return;
        }

        public static void OnInitLevelupSpellSelection(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if ((dispIo.arg0 != ClassId))
            {
                return;
            }

            var classLvl = evt.objHndCaller.GetStat(ClassId);
            if ((classLvl == 0))
            {
                return;
            }

            var class_extended_1 = evt.GetConditionArg1();
            throw new NotImplementedException();
            // classSpecModule.InitSpellSelection (evt.objHndCaller, class_extended_1);
        }

        public static void OnLevelupSpellsCheckComplete(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if ((dispIo.arg0 != ClassId))
            {
                return;
            }

            var class_extended_1 = evt.GetConditionArg1();
            throw new NotImplementedException();
            // if ((!classSpecModule.LevelupCheckSpells (evt.objHndCaller, class_extended_1)))
            // {
            //     dispIo.bonlist.AddBonus(-1, 0, 137); // denotes incomplete spell selection
            // }
        }

        public static void OnLevelupSpellsFinalize(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if ((dispIo.arg0 != ClassId))
            {
                return;
            }

            var classLvl = evt.objHndCaller.GetStat(ClassId);
            if ((classLvl == 0))
            {
                return;
            }

            var class_extended_1 = evt.GetConditionArg1();
            throw new NotImplementedException();
            // TODO classSpecModule.LevelupSpellsFinalize (evt.objHndCaller, class_extended_1);
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