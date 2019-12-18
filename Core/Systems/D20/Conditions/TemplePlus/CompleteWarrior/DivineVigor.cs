
using System;
using System.Collections.Generic;
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
using SpicyTemple.Core.Systems.RadialMenus;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{

    // Divine Vigor:  Complete Warrior, p. 108
    public class DivineVigor
    {
        private static readonly D20DispatcherKey divineVigorEnum = (D20DispatcherKey) 2601;

        public static void DivineVigorRadial(in DispatcherCallbackArgs evt)
        {
            var isAdded = evt.objHndCaller.AddCondition("Divine Vigor Effect", 0, 0); // adds the "Divine Vigor" condition on first radial menu build
            var radialAction = RadialMenuEntry.CreatePythonAction(divineVigorEnum, 0, "TAG_INTERFACE_HELP");
            radialAction.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Feats);
        }

        public static void OnDivineVigorCheck(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            // Get the current number of turn charges
            var TurnCharges = evt.objHndCaller.GetTurnUndeadCharges();
            // Check for remaining turn undead attempts
            if (TurnCharges < 1)
            {
                dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_CHARGES;
                return;
            }

            // Check that the character is not a fallen paladin without black guard levels
            if (evt.objHndCaller.D20Query(D20DispatcherKey.QUE_IsFallenPaladin) && (evt.objHndCaller.GetStat(Stat.level_blackguard) == 0))
            {
                dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
            }
        }

        public static void OnDivineVigorPerform(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            // Deduct a turn undead charge
            evt.objHndCaller.D20SendSignal("Deduct Turn Undead Charge");
            // Set to active (arg 1)
            evt.SetConditionArg1(1);
            // Duration (arg 2) = Charisma bonus minutes (1 minute = 10 rounds)
            var numRounds = 10 * evt.objHndCaller.GetStat(Stat.cha_mod);
            evt.SetConditionArg2(numRounds);
            // Temporary Hit Points (arg 3) = 2 * character level
            var tempHP = evt.objHndCaller.GetStat(Stat.level) * 2;
            evt.SetConditionArg3(tempHP);
            return;
        }
        public static void DivineVigorBeginRound(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            // calculate the number of rounds left
            var numRounds = evt.GetConditionArg2();
            var roundsToReduce = dispIo.data1;
            if (numRounds - roundsToReduce > 0)
            {
                evt.SetConditionArg2(numRounds - roundsToReduce); // decrement the number of rounds
                return;
            }
            else
            {
                evt.SetConditionArg1(0); // set inactive
                evt.SetConditionArg2(0); // set to zero rounds
                evt.SetConditionArg3(0); // set to zero temporary hit points
            }

            return;
        }
        public static void DivineVigorTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoTooltip();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            var tempHP = evt.GetConditionArg3();
            // Set the tooltip (showing temporary hit points if applicable)
            if (tempHP > 0)
            {
                dispIo.Append("Divine Vigor Temp HP: " + tempHP.ToString());
            }
            else
            {
                dispIo.Append("Divine Vigor");
            }

            return;
        }
        public static void DivineVigorEffectTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoEffectTooltip();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            var tempHP = evt.GetConditionArg3();
            // Set the tooltip (showing temporary hit points if applicable)
            if (tempHP > 0)
            {
                dispIo.bdb.AddEntry(ElfHash.Hash("DIVINE_VIGOR"), " (" + evt.GetConditionArg2().ToString() + " rounds, Temp HP:" + tempHP.ToString() + ")", -2);
            }
            else
            {
                dispIo.bdb.AddEntry(ElfHash.Hash("DIVINE_VIGOR"), " (" + evt.GetConditionArg2().ToString() + " rounds)", -2);
            }

            return;
        }
        public static void DivineVigorMoveSpeed(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoMoveSpeed();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            // Movement speed increased by 10 feet (enhancement bonus)
            dispIo.bonlist.AddBonus(10, 12, "Divine Vigor Feat");
            return;
        }
        public static void DivineVigorTakingDamage2(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            var tempHP = evt.GetConditionArg3();
            if (tempHP <= 0)
            {
                return;
            }

            var finalDam = dispIo.damage.finalDamage;
            var hpLeft = tempHP - finalDam;
            // Zero out if hitpoins are less than zero
            if (hpLeft < 0)
            {
                hpLeft = 0;
            }

            evt.SetConditionArg3(hpLeft);
            if (hpLeft <= 0)
            {
                dispIo.damage.finalDamage -= tempHP;
                dispIo.damage.AddDamageBonus(-tempHP, 0, 154);
                evt.objHndCaller.FloatLine("Damage Absorbed " + tempHP.ToString() + ".");
                evt.objHndCaller.D20SendSignal(D20DispatcherKey.SIG_Temporary_Hit_Points_Removed, evt.GetConditionArg1());
                return;
            }

            evt.objHndCaller.FloatLine("Damage Absorbed " + finalDam.ToString() + ".");
            dispIo.damage.finalDamage = 0;
            dispIo.damage.AddDamageBonus(-finalDam, 0, 154);
            return;
        }
        public static void DivineVigorHasTemporaryHitpoints(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = 0; // Default to false
                                                          // Set to true if divine vigor is active and hit points remain
            if (evt.GetConditionArg1() > 0 && evt.GetConditionArg3() > 0)
            {
                dispIo.return_val = 1;
                return;
            }

            return;
        }

        // Setup the feat
        [FeatCondition("Divine Vigor")]
        [AutoRegister] public static readonly ConditionSpec divineVigorFeat = ConditionSpec.Create("Divine Vigor Feat", 2)
            .SetUnique()
            .AddHandler(DispatcherType.RadialMenuEntry, DivineVigorRadial)
            .Build();

        // Setup the effect
        [AutoRegister] public static readonly ConditionSpec divineVigorEffect = ConditionSpec.Create("Divine Vigor Effect", 3)
            .SetUnique()
            .AddHandler(DispatcherType.PythonActionCheck, divineVigorEnum, OnDivineVigorCheck)
            .AddHandler(DispatcherType.PythonActionPerform, divineVigorEnum, OnDivineVigorPerform)
            .AddHandler(DispatcherType.BeginRound, DivineVigorBeginRound)
            .AddHandler(DispatcherType.Tooltip, DivineVigorTooltip)
            .AddHandler(DispatcherType.EffectTooltip, DivineVigorEffectTooltip)
            .AddHandler(DispatcherType.GetMoveSpeed, DivineVigorMoveSpeed)
            .AddHandler(DispatcherType.TakingDamage2, DivineVigorTakingDamage2)
            .AddQueryHandler(D20DispatcherKey.QUE_Has_Temporary_Hit_Points, DivineVigorHasTemporaryHitpoints)
            .Build();
    }
}
