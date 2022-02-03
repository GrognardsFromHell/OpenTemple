
using System;
using System.Collections.Generic;
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
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{

    // Wolverine's Rage:  Complete Divine, p. 86
    public class WolverineRage
    {
        private static readonly D20DispatcherKey WolverineRageEnum = (D20DispatcherKey) 2700;

        public static void WolverineRageRadial(in DispatcherCallbackArgs evt)
        {
            var isAdded = evt.objHndCaller.AddCondition("Wolverine Rage Effect", 0, 0); // adds the "Wolverine Rage" condition on first radial menu build
            var radialAction = RadialMenuEntry.CreatePythonAction(WolverineRageEnum, 0, "TAG_INTERFACE_HELP");
            radialAction.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Feats);
        }

        public static void WolverineRageEffectTakingDamage2(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            var finalDamage = dispIo.damage.finalDamage;
            // Set damage taken flag for next turn
            if (finalDamage > 0)
            {
                // AOO handling - set enabled flag directly
                if ((dispIo.attackPacket.flags & D20CAF.ATTACK_OF_OPPORTUNITY) != D20CAF.NONE)
                {
                    evt.SetConditionArg2(1);
                }
                else
                {
                    evt.SetConditionArg1(1);
                }

            }
        }
        public static void OnWolverineRageEffectCheck(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            // Don't allow if the effect is already enabled
            if (evt.GetConditionArg3() != 0)
            {
                dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                return;
            }

            // Check that the character has taken damage this round
            if (evt.GetConditionArg2() == 0)
            {
                dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                return;
            }

            // Get the current number of wild shape charges
            var WildShapeCharges = evt.objHndCaller.GetWildshapeCharges();
            // Check for remaining wild shape attempts
            if ((WildShapeCharges < 1))
            {
                dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_CHARGES;
            }
        }

        public static void OnWolverineRageEffectPerform(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            // Deduct a wild shape charge
            evt.objHndCaller.D20SendSignal("Deduct Wild Shape Charge");
            // Set to active
            evt.SetConditionArg3(1);
            // Lasts 5 rounds
            evt.SetConditionArg4(5);
        }
        public static void WolverineRageEffectBeginRound(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            // Update the enabled flag for this round and clear the damage counter
            var damageThisTurnFlag = evt.GetConditionArg1();
            evt.SetConditionArg2(damageThisTurnFlag);
            evt.SetConditionArg1(0);
            // not active, nothing more to do
            if (evt.GetConditionArg3() == 0)
            {
                return;
            }

            // calculate the number of rounds left
            var numRounds = evt.GetConditionArg4();
            var roundsToReduce = dispIo.data1;
            if (numRounds - roundsToReduce > 0)
            {
                evt.SetConditionArg4(numRounds - roundsToReduce); // decrement the number of rounds
            }
            else
            {
                evt.SetConditionArg3(0); // set effect to inactive
            }
        }
        public static void WolverineRageEffectTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoTooltip();
            // not active, do nothing
            if (evt.GetConditionArg3() == 0)
            {
                return;
            }

            dispIo.Append("Wolverine's Rage");
        }
        public static void WolverineRageEffectTooltipEffect(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoEffectTooltip();
            // not active, do nothing
            if (evt.GetConditionArg3() == 0)
            {
                return;
            }

            // Set the tooltip
            dispIo.bdb.AddEntry(BuffDebuffType.Condition, ElfHash.Hash("WOLVERINE_RAGE"), "", -2);
        }
        public static void WolverineRageEffectConMod(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoBonusList();
            // not active, do nothing
            if (evt.GetConditionArg3() == 0)
            {
                return;
            }

            dispIo.bonlist.AddBonus(2, 0, "Wolverine's Rage"); // Unnamed bonus
        }
        public static void WolverineRageEffectStrMod(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoBonusList();
            // not active, do nothing
            if (evt.GetConditionArg3() == 0)
            {
                return;
            }

            dispIo.bonlist.AddBonus(2, 0, "Wolverine's Rage"); // Unnamed bonus
        }
        public static void WolverineRageEffectACPenalty(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            // not active, do nothing
            if (evt.GetConditionArg3() == 0)
            {
                return;
            }

            dispIo.bonlist.AddBonus(-2, 0, "Wolverine's Rage");
        }

        // Setup the feat
        // spare, spare
        [FeatCondition("Wolverine's Rage")]
        [AutoRegister] public static readonly ConditionSpec WolverineRageFeat = ConditionSpec.Create("Wolverine Rage Feat", 2)
            .SetUnique()
            .AddHandler(DispatcherType.RadialMenuEntry, WolverineRageRadial)
            .Build();

        // Setup the effect
        // damage taken this round flag, feat enabled flag, effect active, rounds left, spare

        [AutoRegister] public static readonly ConditionSpec WolverineRageEffect = ConditionSpec.Create("Wolverine Rage Effect", 5)
            .AddHandler(DispatcherType.TakingDamage2, WolverineRageEffectTakingDamage2)
            .AddHandler(DispatcherType.BeginRound, WolverineRageEffectBeginRound)
            .AddHandler(DispatcherType.PythonActionCheck, WolverineRageEnum, OnWolverineRageEffectCheck)
            .AddHandler(DispatcherType.PythonActionPerform, WolverineRageEnum, OnWolverineRageEffectPerform)
            .AddHandler(DispatcherType.Tooltip, WolverineRageEffectTooltip)
            .AddHandler(DispatcherType.EffectTooltip, WolverineRageEffectTooltipEffect)
            .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_CONSTITUTION, WolverineRageEffectConMod)
            .AddHandler(DispatcherType.AbilityScoreLevel, D20DispatcherKey.STAT_STRENGTH, WolverineRageEffectStrMod)
            .AddHandler(DispatcherType.GetAC, WolverineRageEffectACPenalty)
            .Build();
    }
}
