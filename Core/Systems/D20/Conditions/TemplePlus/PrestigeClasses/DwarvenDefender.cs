
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using OpenTemple.Core.GameObject;
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
using OpenTemple.Core.Logging;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{

    [AutoRegister]
    public class DwarvenDefender
    {
        
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        private static readonly Stat ClassId = Stat.level_dwarven_defender;

        private static readonly D20DispatcherKey defensiveStanceEnum = (D20DispatcherKey) 2500;
        private static readonly D20DispatcherKey defensiveStanceWindedEnum = (D20DispatcherKey) 2501;

        public static readonly D20ClassSpec ClassSpec = new D20ClassSpec
        {
            classEnum = Stat.level_dwarven_defender,
            helpTopic = "TAG_DWARVEN_DEFENDERS",
            conditionName = "Dwarven Defender",
            flags = ClassDefinitionFlag.CDF_CoreClass,
            BaseAttackBonusProgression = BaseAttackProgressionType.Martial,
            hitDice = 12,
            FortitudeSaveProgression = SavingThrowProgressionType.HIGH,
            ReflexSaveProgression = SavingThrowProgressionType.LOW,
            WillSaveProgression = SavingThrowProgressionType.HIGH,
            skillPts = 2,
            hasArmoredArcaneCasterFeature = false,
            classSkills = new HashSet<SkillId>
            {
                SkillId.listen,
                SkillId.sense_motive,
                SkillId.spot,
                SkillId.alchemy,
                SkillId.craft,
            }.ToImmutableHashSet(),
            classFeats = new Dictionary<FeatId, int>
            {
                {FeatId.ARMOR_PROFICIENCY_LIGHT, 1},
                {FeatId.ARMOR_PROFICIENCY_MEDIUM, 1},
                {FeatId.ARMOR_PROFICIENCY_HEAVY, 1},
                {FeatId.SHIELD_PROFICIENCY, 1},
                {FeatId.SIMPLE_WEAPON_PROFICIENCY, 1},
                {FeatId.MARTIAL_WEAPON_PROFICIENCY_ALL, 1},
                {FeatId.UNCANNY_DODGE, 2},
                {FeatId.TRAPS, 4},
                {FeatId.IMPROVED_UNCANNY_DODGE, 6},
            }.ToImmutableDictionary(),
        };

        public static readonly ConditionSpec ClassCondition = TemplePlusClassConditions.Create(ClassSpec)
            .AddHandler(DispatcherType.GetAC, DwarvenDefenderAcBonus)
            .AddHandler(DispatcherType.GetAC, DwDTrapSenseDodgeBonus)
            .AddHandler(DispatcherType.SaveThrowLevel, D20DispatcherKey.SAVE_REFLEX, DwDTrapSenseReflexBonus)
            .AddHandler(DispatcherType.TakingDamage2, DwDDamageReduction)
            .AddHandler(DispatcherType.RadialMenuEntry, DefensiveStanceRadial)
            .Build();

        public static void DwarvenDefenderAcBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var dwdLvl = evt.objHndCaller.GetStat(ClassId);
            var bonval = 1 + (dwdLvl - 1) / 3;
            dispIo.bonlist.AddBonus(bonval, 8, 137); // Dodge bonus,  ~Class~[TAG_LEVEL_BONUSES]
        }
        public static void DwDTrapSenseDodgeBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            var dwdLvl = evt.objHndCaller.GetStat(ClassId);
            if (dwdLvl < 4)
            {
                return;
            }

            var bonval = 1 + (dwdLvl - 4) / 4;
            if ((dispIo.attackPacket.flags & D20CAF.TRAP) != D20CAF.NONE)
            {
                dispIo.bonlist.AddBonus(bonval, 8, 137);
            }
        }
        public static void DwDTrapSenseReflexBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            var dwdLvl = evt.objHndCaller.GetStat(ClassId);
            if (dwdLvl < 4)
            {
                return;
            }

            var bonval = 1 + (dwdLvl - 4) / 4;
            if ((dispIo.flags & D20SavingThrowFlag.TRAP) != 0)
            {
                dispIo.bonlist.AddBonus(bonval, 8, 137);
            }
        }

        public static void DwDDamageReduction(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            var dwdLvl = evt.objHndCaller.GetStat(ClassId);
            if (dwdLvl < 6)
            {
                return;
            }

            var bonval = 3 * (1 + (dwdLvl - 6) / 4);
            dispIo.damage.AddPhysicalDR(bonval, D20AttackPower.UNSPECIFIED, 126); // type 1 - will always apply
        }

        public static void DefensiveStanceRadial(in DispatcherCallbackArgs evt)
        {
            // adds the "Defensive Stance" condition on first radial menu build
            var isAdded = evt.objHndCaller.AddCondition(DefensiveStance, 0, 0);
            var isWinded = false;
            var isActive = false;
            // add radial menu action Defensive Stanch
            if (!isAdded) // means it's not a newly added condition
            {
                isWinded = evt.objHndCaller.D20Query("Defensive Stance Is Winded");
                isActive = evt.objHndCaller.D20Query("Defensive Stance Is Active");
            }

            // print "is active: " + str(isActive) + "  is winded: " + str(isWinded)
            RadialMenuEntry radialAction;
            if (isActive && (!isWinded)) // if already active, show the "Winded" option a la Barbarian Rage
            {
                radialAction = RadialMenuEntry.CreatePythonAction(defensiveStanceWindedEnum, 0, "TAG_INTERFACE_HELP");
            }
            else
            {
                // print str(D20A_PYTHON_ACTION) + "  " + str(defensiveStanceEnum)
                radialAction = RadialMenuEntry.CreatePythonAction(defensiveStanceEnum, 0, "TAG_INTERFACE_HELP");
            }

            radialAction.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Class);
        }

        public static void DefensiveStanceIsWinded(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if (evt.GetConditionArg4() != 0)
            {
                dispIo.return_val = 1;
            }
        }

        public static bool IsActive(in DispatcherCallbackArgs evt) => evt.GetConditionArg1() != 0;

        public static bool IsWinded(in DispatcherCallbackArgs evt)
        {
            if (!IsActive(in evt))
            {
                return false;
            }

            return evt.GetConditionArg4() != 0;
        }

        public static void DefensiveStanceIsActive(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if (IsActive(in evt))
            {
                dispIo.return_val = 1;
            }
        }

        public static void OnDefensiveStanceCheck(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            if (IsActive(in evt)) // if already active
            {
                if (IsWinded(in evt))
                {
                    dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                    
                    return;
                }
                else
                {
                    // else: # action is possible (will make attachee winded)
                    return;
                }

            }

            var dwdLvl = evt.objHndCaller.GetStat(ClassId);
            var maxNumPerDay = 1 + (dwdLvl - 1) / 2;
            if (evt.GetConditionArg2() >= maxNumPerDay)
            {
                dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_CHARGES;
            }
        }
        public static void OnDefensiveStancePerform(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            Logger.Info("Performing def stance");
            if (IsActive(in evt)) // change to winded
            {
                Logger.Info("changing to winded");
                evt.SetConditionArg4(1);
                evt.SetConditionArg3(10); // for 10 rounds
                return;
            }

            evt.SetConditionArg1(1); // set to active
            evt.SetConditionArg2(evt.GetConditionArg2() + 1); // increase number of times used today
            var conLvl = evt.objHndCaller.GetStat(Stat.constitution);
            conLvl += 4; // constitution is increased by 4 and this counts towards the number of rounds
            var conMod = (conLvl - 10) / 2;
            var numRounds = Math.Max(1, 3 + conMod);
            evt.SetConditionArg3(numRounds); // set the number of rounds remaining
            evt.SetConditionArg4(0); // set isWinded to 0
        }
        public static void DefStanceConMod(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoBonusList();
            if (!IsActive(in evt))
            {
                return;
            }

            if (IsWinded(in evt))
            {
                return;
            }

            dispIo.bonlist.AddBonus(4, 0, 137);
        }
        public static void DefStanceStrMod(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoBonusList();
            if (!IsActive(in evt))
            {
                return;
            }

            if (IsWinded(in evt))
            {
                dispIo.bonlist.AddBonus(-2, 0, 137);
                return;
            }

            dispIo.bonlist.AddBonus(2, 0, 137);
        }
        public static void DefStanceMoveSpeed(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoMoveSpeed();
            if (!IsActive(in evt))
            {
                return;
            }

            if (IsWinded(in evt))
            {
                return;
            }

            var movespeedCap = 0;
            if (evt.objHndCaller.GetStat(ClassId) >= 8)
            {
                movespeedCap = 5;
            }

            dispIo.bonlist.SetOverallCap(1, movespeedCap, 0, 137); // set upper cap
            dispIo.bonlist.SetOverallCap(6, 0, 0, 137); // lower cap... set with the override flag (4) because dwarves can have a lower racial cap of 20
        }
        public static void DefStanceSaveBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoSavingThrow();
            if (!IsActive(in evt))
            {
                return;
            }

            if (IsWinded(in evt))
            {
                return;
            }

            dispIo.bonlist.AddBonus(2, 0, 137);
        }
        public static void DefStanceAcBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            if (!IsActive(in evt))
            {
                return;
            }

            if (IsWinded(in evt))
            {
                return;
            }

            dispIo.bonlist.AddBonus(4, 8, 137);
        }
        public static void DefStanceBeginRound(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            if (!IsActive(in evt)) // not active
            {
                return;
            }

            var numRounds = evt.GetConditionArg3();
            var roundsToReduce = dispIo.data1;
            if (numRounds - roundsToReduce >= 0)
            {
                // print "beginround: new rounds remaining: " + str(numRounds - roundsToReduce )
                evt.SetConditionArg3(numRounds - roundsToReduce);
                return;
            }
            else
            {
                if (!IsWinded(in evt))
                {
                    // print "beginround: setting to winded"
                    evt.SetConditionArg4(1); // set winded
                    evt.SetConditionArg3(10); // setting winded to 10 rounds instead of the nebulous "duration of the encounter"
                }
                else
                {
                    // print "beginround: finishing winded"
                    evt.SetConditionArg4(0); // unset winded
                    evt.SetConditionArg1(0); // set inactive
                    evt.SetConditionArg3(0); // reset remaining rounds
                }

            }
        }
        public static void DefStanceNewday(in DispatcherCallbackArgs evt)
        {
            evt.SetConditionArg1(0);
            evt.SetConditionArg2(0);
            evt.SetConditionArg3(0);
            evt.SetConditionArg4(0);
        }
        public static void DefStanceTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoTooltip();
            if (!IsActive(in evt))
            {
                return;
            }

            if (IsWinded(in evt))
            {
                dispIo.Append("Winded (" + evt.GetConditionArg3().ToString() + " rounds)");
                return;
            }

            dispIo.Append("Defensive Stance (" + evt.GetConditionArg3().ToString() + " rounds)");
        }
        public static void DefStanceEffectTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoEffectTooltip();
            if (!IsActive(in evt))
            {
                return;
            }

            if (IsWinded(in evt))
            {
                dispIo.bdb.AddEntry(BuffDebuffType.Debuff, 154, $"Winded ({evt.GetConditionArg3()} rounds)", -2);
                return;
            }

            dispIo.bdb.AddEntry(BuffDebuffType.Condition, ElfHash.Hash("DWARVEN_DEFENDER_STANCE"),
                $" ({evt.GetConditionArg3()} rounds)", -2);
        }

        // arg0 - is active ; arg1 - number of times used this day ; arg2 - rounds remaining ; arg3 - is in winded state
        public static readonly ConditionSpec DefensiveStance = ConditionSpec.Create("Defensive Stance", 4)
            .SetUnique()
            .AddQueryHandler("Defensive Stance Is Winded", DefensiveStanceIsWinded)
            .AddQueryHandler("Defensive Stance Is Active", DefensiveStanceIsActive)
            .AddHandler(DispatcherType.PythonActionCheck, defensiveStanceEnum, OnDefensiveStanceCheck)
            .AddHandler(DispatcherType.PythonActionPerform, defensiveStanceEnum, OnDefensiveStancePerform)
            .AddHandler(DispatcherType.PythonActionCheck, defensiveStanceWindedEnum, OnDefensiveStanceCheck)
            .AddHandler(DispatcherType.PythonActionPerform, defensiveStanceWindedEnum, OnDefensiveStancePerform)
            .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CONSTITUTION, DefStanceConMod)
            .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_STRENGTH, DefStanceStrMod)
            .AddHandler(DispatcherType.GetMoveSpeed, DefStanceMoveSpeed)
            .AddHandler(DispatcherType.SaveThrowLevel, DefStanceSaveBonus)
            .AddHandler(DispatcherType.GetAC, DefStanceAcBonus)
            .AddHandler(DispatcherType.BeginRound, DefStanceBeginRound)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, DefStanceNewday)
            .AddHandler(DispatcherType.Tooltip, DefStanceTooltip)
            .AddHandler(DispatcherType.EffectTooltip, DefStanceEffectTooltip)
            .Build();
    }
}
