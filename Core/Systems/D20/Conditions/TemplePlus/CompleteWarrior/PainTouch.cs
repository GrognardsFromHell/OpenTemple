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
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{
    // Complete Warrior: p. 103
    public class PainTouch
    {
        public static void PainTouchFeatOnSpecialAttack(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjSpecialAttack();
            // Check that the attack is a stunning fist
            if (dispIo.attack != 1)
            {
                return;
            }

            var tgt = dispIo.target;
            var sizeTarget = (SizeCategory) tgt.GetStat(Stat.size);
            var sizeAttacker = GameSystems.Stat.DispatchGetSizeCategory(evt.objHndCaller);
            // Target must be no more than one size category greater than the attacker
            var targetName = GameSystems.MapObject.GetDisplayNameForParty(tgt);
            if (sizeAttacker + 1 >= sizeTarget)
            {
                GameSystems.RollHistory.CreateFromFreeText($"{targetName} effected by pain touch!\n\n");
                // Note:  The condition lasts for 2 rounds but the first round that victim is stunned so it in effect lasts 1 round
                tgt.AddCondition(PainTouchEffect, 2);
            }
            else
            {
                GameSystems.RollHistory.CreateFromFreeText(targetName + " too large, uneffected by pain touch!\n\n");
            }
        }

        public static void PainTouchEffectBeginRound(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            var duration = evt.GetConditionArg1();
            // If zero rounds remaining remove the effect (to avoid it lasting forever outside of combat)
            if (duration < 1)
            {
                evt.RemoveThisCondition();
            }

            // Decrement the duration
            var roundsToReduce = dispIo.data1;
            duration = duration - roundsToReduce;
            duration = Math.Max(duration, 0);
            evt.SetConditionArg1(duration);
        }

        public static void PainTouchEffectTurnBasedStatusInit(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIOTurnBasedStatus();
            // Remove the target's standard action
            if (dispIo.tbStatus.hourglassState > HourglassState.MOVE)
            {
                dispIo.tbStatus.hourglassState = HourglassState.MOVE; // sets to Move Action Only
            }

            // Duration is decremented in On Begin Round Just check it here
            var duration = evt.GetConditionArg1();
            if (duration < 1)
            {
                evt.RemoveThisCondition();
            }

            return;
        }

        public static void PainTouchEffectGetTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoTooltip();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            // Set the tooltip
            dispIo.Append("Nauseated!");
        }

        public static void PainTouchEffectGetEffectTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoEffectTooltip();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            // Set the tooltip
            dispIo.bdb.AddEntry(ElfHash.Hash("PAIN_TOUCH"), "", -2);
        }

        public static void PainTouchEffectAOOPossible(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // No making AOOs when Nauseated
            dispIo.return_val = 0;
            return;
        }

        // Setup the feat
        // Extra, Extra
        [FeatCondition("Pain Touch")]
        [AutoRegister] public static readonly ConditionSpec PainTouchFeat = ConditionSpec.Create("Pain Touch Feat", 2)
            .SetUnique()
            .AddHandler(DispatcherType.SpecialAttack, PainTouchFeatOnSpecialAttack)
            .Build();

        // Setup the effect
        // Rounds, Extra
        [AutoRegister] public static readonly ConditionSpec PainTouchEffect = ConditionSpec.Create("Pain Touch Effect", 2)
            .SetUnique()
            .AddHandler(DispatcherType.TurnBasedStatusInit, PainTouchEffectTurnBasedStatusInit)
            .AddHandler(DispatcherType.BeginRound, PainTouchEffectBeginRound)
            .AddHandler(DispatcherType.Tooltip, PainTouchEffectGetTooltip)
            .AddHandler(DispatcherType.EffectTooltip, PainTouchEffectGetEffectTooltip)
            .AddHandler(DispatcherType.D20Query, D20DispatcherKey.QUE_AOOPossible, PainTouchEffectAOOPossible)
            .Build();
    }
}