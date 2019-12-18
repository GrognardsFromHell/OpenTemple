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
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.D20.Classes;
using SpicyTemple.Core.Systems.RadialMenus;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{
    [AutoRegister]
    public class Blackguard
    {
        public const Stat ClassId = Stat.level_blackguard;

        public const string DetectGood = "Detect Good";

        public static readonly FeatId DetectGoodId = (FeatId) ElfHash.Hash(DetectGood);

        public const string SmiteGoodName = "Smite Good";

        public static readonly FeatId SmiteGoodId = (FeatId) ElfHash.Hash(SmiteGoodName);

        public const string DarkBlessingName = "Dark Blessing";

        public static readonly FeatId DarkBlessingId = (FeatId) ElfHash.Hash(DarkBlessingName);

        public const string AuraOfDespairName = "Aura of Despair";

        public static readonly FeatId AuraOfDespairId = (FeatId) ElfHash.Hash(AuraOfDespairName);

        public static readonly D20ClassSpec ClassSpec = new D20ClassSpec
        {
            classEnum = ClassId,
            helpTopic = "TAG_BLACKGUARDS",
            conditionName = "Blackguard",
            flags = ClassDefinitionFlag.CDF_CoreClass,
            BaseAttackBonusProgression = BaseAttackProgressionType.Martial,
            hitDice = 10,
            FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
            ReflexSaveProgression = SavingThrowProgressionType.LOW,
            WillSaveProgression = SavingThrowProgressionType.LOW,
            skillPts = 2,
            spellListType = SpellListType.Special,
            hasArmoredArcaneCasterFeature = false,
            spellMemorizationType = SpellReadyingType.Vancian,
            spellSourceType = SpellSourceType.Divine,
            spellCastingConditionName = "Blackguard Spellcasting",
            spellsPerDay = new Dictionary<int, IImmutableList<int>>
            {
                [1] = ImmutableList.Create(-1, 0),
                [2] = ImmutableList.Create(-1, 1),
                [3] = ImmutableList.Create(-1, 1, 0),
                [4] = ImmutableList.Create(-1, 1, 1),
                [5] = ImmutableList.Create(-1, 1, 1, 0),
                [6] = ImmutableList.Create(-1, 1, 1, 1),
                [7] = ImmutableList.Create(-1, 2, 1, 1, 0),
                [8] = ImmutableList.Create(-1, 2, 1, 1, 1),
                [9] = ImmutableList.Create(-1, 2, 2, 1, 1),
                [10] = ImmutableList.Create(-1, 2, 2, 2, 1)
            }.ToImmutableDictionary(),
            classSkills = new HashSet<SkillId>
            {
                SkillId.concentration,
                SkillId.diplomacy,
                SkillId.heal,
                SkillId.hide,
                SkillId.intimidate,
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
                {FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1},
                {DetectGoodId, 1},
                {SmiteGoodId, 2},
                {DarkBlessingId, 2},
                {FeatId.REBUKE_UNDEAD, 3},
                {AuraOfDespairId, 3},
                {FeatId.SNEAK_ATTACK, 4},
            }.ToImmutableDictionary(),
        };

        public static void BlackguardSneakAttackDice(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var blg_lvl = evt.objHndCaller.GetStat(ClassId);
            var palLvl = evt.objHndCaller.GetStat(Stat.level_paladin);
            if (blg_lvl < 4 && palLvl < 5)
            {
                return;
            }

            dispIo.return_val += (blg_lvl - 1) / 3;
            if (palLvl >= 5)
            {
                dispIo.return_val += 1;
            }
        }

        public static void BlackguardRebukeUndeadLevel(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if (dispIo.data1 != 1) // rebuke undead
            {
                return;
            }

            var blg_lvl = evt.objHndCaller.GetStat(ClassId);
            if (blg_lvl < 3)
            {
                return;
            }

            dispIo.return_val += blg_lvl - 2;
        }

        public static void BlackguardFallenPaladin(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            var palLvl = evt.objHndCaller.GetStat(Stat.level_paladin);
            if (palLvl != 0)
            {
                dispIo.return_val = 1;
            }
        }

        public static void BlackguardFallenIndicator(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoEffectTooltip();
            var palLvl = evt.objHndCaller.GetStat(Stat.level_paladin);
            if (palLvl != 0)
            {
                dispIo.bdb.AddEntry(BuffDebuffType.Condition, 175, ": Blackguard", -1);
            }
        }

        public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
            .AddQueryHandler("Sneak Attack Dice", BlackguardSneakAttackDice)
            .AddQueryHandler("Turn Undead Level", BlackguardRebukeUndeadLevel)
            // forces "Fallen Paladin" status no matter what. There's no atoning for this one!
            .AddHandler(DispatcherType.D20Query, D20DispatcherKey.QUE_IsFallenPaladin, BlackguardFallenPaladin)
            .AddHandler(DispatcherType.EffectTooltip, BlackguardFallenIndicator)
            .AddHandler(DispatcherType.GetBaseCasterLevel, OnGetBaseCasterLevel)
            .AddHandler(DispatcherType.LevelupSystemEvent, D20DispatcherKey.LVL_Spells_Finalize,
                OnLevelupSpellsFinalize)
            .Build();

        // Spell casting
        public static void OnGetBaseCasterLevel(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if (dispIo.arg0 == ClassId)
            {
                var classLvl = evt.objHndCaller.GetStat(ClassId);
                dispIo.bonlist.AddBonus(classLvl, 0, 137);
            }
        }

        public static void OnLevelupSpellsFinalize(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpellCaster();
            if (dispIo.arg0 == ClassId)
            {
                throw new NotImplementedException();
                // classSpecModule.LevelupSpellsFinalize (evt.objHndCaller);
            }
        }

        #region Dark Blessing

        public static void BlackguardDarkBlessing(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            // print "Dark Blessing save throw"
            var cha_score = evt.objHndCaller.GetStat(Stat.charisma);
            if (cha_score < 10)
            {
                return;
            }

            var cha_mod = (cha_score - 10) / 2;
            // print "adding bonus " + str(cha_mod)
            dispIo.bonlist.AddBonusFromFeat(cha_mod, 0, 114, (FeatId) ElfHash.Hash("Dark Blessing"));
        }

        [FeatCondition(DarkBlessingName)]
        public static readonly ConditionSpec darkBless = ConditionSpec.Create("Dark Blessing Feat", 0)
            .SetUnique()
            .AddHandler(DispatcherType.SaveThrowLevel, BlackguardDarkBlessing)
            .Build();

        #endregion

        #region Smite Good

        public static void SmiteGoodReset(in DispatcherCallbackArgs evt)
        {
            var classLvl = evt.objHndCaller.GetStat(ClassId);
            var palLvl = evt.objHndCaller.GetStat(Stat.level_paladin);
            var bonusFromPal = (palLvl + 3) / 4;
            if (classLvl < 2 && bonusFromPal == 0)
            {
                return;
            }

            var timesPerDay = 0;
            if (classLvl >= 2)
            {
                timesPerDay += 1;
            }

            if (classLvl >= 5)
            {
                timesPerDay += 1;
            }

            if (classLvl >= 10)
            {
                timesPerDay += 1;
            }

            timesPerDay += bonusFromPal;
            // Add bonus uses from extra smiting feat
            if ((timesPerDay > 0))
            {
                timesPerDay += evt.objHndCaller.HasFeat((FeatId) ElfHash.Hash("Extra Smiting")) ? 2 : 0;
            }

            evt.SetConditionArg1(timesPerDay);
        }

        private static readonly D20DispatcherKey smiteGoodEnum = (D20DispatcherKey) 2201;

        public static void SmiteGoodRadial(in DispatcherCallbackArgs evt)
        {
            var timesPerDay = evt.GetConditionArg1();
            if (timesPerDay <= 0)
            {
                return;
            }

            var radial_action = RadialMenuEntry.CreatePythonAction("Smite Good", D20ActionType.PYTHON_ACTION,
                smiteGoodEnum, 0, "TAG_BLACKGUARDS"); // TODO add help entry
            radial_action.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Class);
        }

        public static void SmiteGoodCheck(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var timesPerDay = evt.GetConditionArg1();
            if (timesPerDay <= 0)
            {
                dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_CHARGES;
            }
        }

        public static void SmiteGoodPerform(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var d20a = dispIo.action;
            var tgt = d20a.d20ATarget;
            var ok_to_add = false;
            if (tgt == null)
            {
                ok_to_add = true;
            }
            else
            {
                if (tgt.IsCritter())
                {
                    if (GameSystems.D20.D20QueryWithObject(tgt, D20DispatcherKey.QUE_CanBeAffected_PerformAction,
                            d20a) != 0)
                    {
                        if (tgt.HasGoodAlignment())
                        {
                            ok_to_add = true;
                        }
                    }
                }
            }

            if (ok_to_add)
            {
                evt.SetConditionArg1(evt.GetConditionArg1() - 1); // decrease remaining usages
                evt.objHndCaller.AddCondition("Smiting Good");
            }
        }

        [FeatCondition("Smite Good")]
        public static readonly ConditionSpec smiteGood = ConditionSpec.Create("Smite Good Feat", 2)
            .SetUnique()
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, SmiteGoodReset)
            .AddHandler(DispatcherType.ConditionAdd, SmiteGoodReset)
            .AddHandler(DispatcherType.RadialMenuEntry, SmiteGoodRadial)
            .AddHandler(DispatcherType.PythonActionPerform, smiteGoodEnum, SmiteGoodPerform)
            .AddHandler(DispatcherType.PythonActionCheck, smiteGoodEnum, SmiteGoodCheck)
            .Build();

        // Smiting Good effect

        public static void SmitingGoodDamage(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            var classLvl = evt.objHndCaller.GetStat(ClassId);
            var tgt = dispIo.attackPacket.victim;
            if (tgt == null)
            {
                return;
            }

            var ok_to_add = false;
            if (tgt.IsCritter())
            {
                if (tgt.HasGoodAlignment())
                {
                    ok_to_add = true;
                }
            }

            var flags = dispIo.attackPacket.flags;
            if ((flags & D20CAF.RANGED) != D20CAF.NONE)
            {
                ok_to_add = false;
            }

            if (ok_to_add)
            {
                dispIo.damage.bonuses.AddBonusFromFeat(classLvl, 0, 114, SmiteGoodId);
                evt.RemoveThisCondition();
            }
        }

        public static void SmitingGoodToHit(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var cha_score = evt.objHndCaller.GetStat(Stat.charisma);
            var cha_mod = (cha_score - 10) / 2;
            var tgt = dispIo.attackPacket.victim;
            if (tgt == null)
            {
                return;
            }

            var ok_to_add = false;
            if (tgt.type == ObjectType.npc || tgt.type == ObjectType.pc)
            {
                if (tgt.HasGoodAlignment())
                {
                    ok_to_add = true;
                }
            }

            var flags = dispIo.attackPacket.flags;
            if ((flags & D20CAF.RANGED) != D20CAF.NONE)
            {
                ok_to_add = false;
            }

            if (!ok_to_add)
            {
                return;
            }

            if (cha_mod > 0)
            {
                dispIo.bonlist.AddBonusFromFeat(cha_mod, 0, 114, SmiteGoodId);
            }
        }

        public static void SmitingGoodRemove(in DispatcherCallbackArgs evt)
        {
            evt.RemoveThisCondition();
        }

        public static void SmitingGoodEffectTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoEffectTooltip();
            dispIo.bdb.AddEntry(BuffDebuffType.Buff, 7, "Smiting Good", -2);
        }

        public static readonly ConditionSpec smitingGoodEffect = ConditionSpec.Create("Smiting Good")
            .AddHandler(DispatcherType.DealingDamage, SmitingGoodDamage)
            .AddHandler(DispatcherType.ToHitBonus2, SmitingGoodToHit)
            .AddHandler(DispatcherType.D20Signal, D20DispatcherKey.SIG_Killed, SmitingGoodRemove)
            .AddHandler(DispatcherType.BeginRound, SmitingGoodRemove)
            .AddHandler(DispatcherType.EffectTooltip, SmitingGoodEffectTooltip)
            .Build();

        #endregion

        #region Aura of Despair

        public static void AuraOfDespairBeginRound(in DispatcherCallbackArgs evt)
        {
            var ppl = ObjList.ListVicinity(evt.objHndCaller, ObjectListFilter.OLC_CRITTERS);
            foreach (var o in ppl)
            {
                if (!o.AllegianceShared(evt.objHndCaller) && !o.IsFriendly(evt.objHndCaller))
                {
                    o.AddCondition("Despaired_Aura");
                }
            }
        }

        public static void AuraOfDespairBegin(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            // print "Aura of Despair Begin"
            var radius_feet = 10f + (evt.objHndCaller.GetRadius() / 12f);
            // print "Effect radius (ft): " + str(radius_feet)
            var obj_evt_id = GameSystems.ObjectEvent.AddEvent(evt.objHndCaller, 0, 1, ObjectListFilter.OLC_CRITTERS,
                radius_feet * locXY.INCH_PER_FEET);
            evt.SetConditionArg3(obj_evt_id);
            // print "Aura of Despair: New Object Event ID: " + str(obj_evt_id)
        }

        public static void AuraOfDespairAoEEntered(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoObjEvent();
            // print "Aura of Despair entered event"
            var obj_evt_id = evt.GetConditionArg3();
            if (obj_evt_id != dispIo.evtId)
            {
                // print "Aura of Despair Entered: ID mismatch " + str(dispIo.evt_id) + ", stored was: " + str(obj_evt_id)
                return;
            }

            // print "Aura of Despair Entered, event ID: " + str(obj_evt_id)
            var tgt = dispIo.tgt;
            if (tgt == null)
            {
                return;
            }

            if (evt.objHndCaller == null)
            {
                return;
            }

            if (!tgt.AllegianceShared(evt.objHndCaller) && !tgt.IsFriendly(evt.objHndCaller))
            {
                tgt.AddCondition(auraDespEffect, 0, 0, obj_evt_id);
            }
        }

        [FeatCondition(AuraOfDespairName)]
        public static readonly ConditionSpec auraDesp = ConditionSpec.Create("Feat Aura of Despair", 4)
            .SetUnique()
            .AddHandler(DispatcherType.ConditionAdd, AuraOfDespairBegin)
            .AddHandler(DispatcherType.D20Signal, D20DispatcherKey.SIG_Teleport_Reconnect, AuraOfDespairBegin)
            .AddHandler(DispatcherType.ObjectEvent, D20DispatcherKey.OnEnterAoE, AuraOfDespairAoEEntered)
            .Build();
        // auraDesp.AddHook(ET_OnBeginRound, EK_NONE, AuraOfDespairBeginRound, ())

        public static void AuraDespairEffSavingThrow(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            dispIo.bonlist.AddBonus(-2, 0, 0, "Aura of Despair");
        }

        public static void AuraOfDespairAoEExited(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoObjEvent();
            var obj_evt_id = evt.GetConditionArg3();
            if (obj_evt_id != dispIo.evtId)
            {
                // print "Aura of Despair Exited: ID mismatch " + str(dispIo.evt_id) + ", stored was: " + str(obj_evt_id)
                return;
            }

            // print "Aura of Despair (ID " + str(obj_evt_id) +") Exited, critter: " + attachee.description + " "
            evt.RemoveThisCondition();
        }

        public static void AuraDespTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoTooltip();
            dispIo.Append("Despaired");
        }

        public static void AuraOfDespairRemove(in DispatcherCallbackArgs evt)
        {
            evt.RemoveThisCondition();
        }

        public static readonly ConditionSpec auraDespEffect = ConditionSpec.Create("Despaired_Aura", 4)
            .SetUnique()
            .AddHandler(DispatcherType.SaveThrowLevel, AuraDespairEffSavingThrow)
            .AddHandler(DispatcherType.ObjectEvent, D20DispatcherKey.OnLeaveAoE, AuraOfDespairAoEExited)
            .AddHandler(DispatcherType.Tooltip, AuraDespTooltip)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, AuraOfDespairRemove)
            .AddHandler(DispatcherType.D20Signal, D20DispatcherKey.SIG_Teleport_Prepare, AuraOfDespairRemove)
            .Build();

        #endregion
    }
}