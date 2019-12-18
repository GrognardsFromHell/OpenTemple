
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

    // Complete Warrior p.106
    public class DivineShield
    {
        private static readonly D20DispatcherKey divineShieldEnum = (D20DispatcherKey) 2602;

        // Check that the the PC has a shield
        public static bool HasShield(GameObjectBody pc)
        {
            return pc.ItemWornAt(EquipSlot.Shield) == null;
        }

        public static void DivineShieldRadial(in DispatcherCallbackArgs evt)
        {
            var isAdded = evt.objHndCaller.AddCondition("Divine Shield Effect", 0, 0); // adds the "Divine Shield" condition on first radial menu build
            var radialAction = RadialMenuEntry.CreatePythonAction(divineShieldEnum, 0, "TAG_INTERFACE_HELP");
            radialAction.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Feats);
        }

        public static void OnDivineShieldCheck(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            // First, a shield must be wielded
            if (!HasShield(evt.objHndCaller))
            {
                dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                return;
            }

            var TurnCharges = evt.objHndCaller.GetTurnUndeadCharges();
            // Second, check for remaining turn undead attempts
            if ((TurnCharges < 1))
            {
                dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_CHARGES;
                return;
            }

            // Third, check that the character is not a fallen paladin without black guard levels
            if (evt.objHndCaller.D20Query(D20DispatcherKey.QUE_IsFallenPaladin) && (evt.objHndCaller.GetStat(Stat.level_blackguard) == 0))
            {
                dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
            }
        }

        public static void OnDivineShieldPerform(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            // Set to active
            evt.SetConditionArg1(1);
            // Deduct a turn undead charge
            evt.objHndCaller.D20SendSignal("Deduct Turn Undead Charge");
            // Duration is half the character level in rounds
            var numRounds = evt.objHndCaller.GetStat(Stat.level) / 2;
            if (numRounds < 1)
            {
                numRounds = 1;
            }

            evt.SetConditionArg2(numRounds);
        }

        public static void DivineShieldAcBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            var item = evt.objHndCaller.ItemWornAt(EquipSlot.Shield);
            // No shield, no bonus
            if (item == null)
            {
                return;
            }

            // Is the shield a buckler?
            var hasBuckler = GameSystems.Item.IsBuckler(item);
            // If the buckler bonus is disabled, no bonus
            if (hasBuckler)
            {
                var bucklerDisabled = evt.objHndCaller.D20Query("Buckler Bonus Disabled");
                if (bucklerDisabled)
                {
                    return;
                }

            }

            // Add the charisma modifier to the shield's bonus
            var charisma = evt.objHndCaller.GetStat(Stat.charisma);
            var charismaBonus = (charisma - 10) / 2;
            // This doesn't work exactly like the feat.  Instead of adding the bonus to the shield's AC bonus,
            // the bonus is added seperately as a bonus that always stacks.  This should work out the same in almost
            // all cases.
            if (charismaBonus > 0)
            {
                dispIo.bonlist.AddBonus(charismaBonus, 0, "Divine Shield Feat");
            }
        }

        public static void DivineShieldInventoryUpdate(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            if (!HasShield(evt.objHndCaller))
            {
                evt.SetConditionArg1(0); // set inactive
                evt.SetConditionArg2(0); // set to zero rounds
                return;
            }
        }

        public static void DivineShieldBeginRound(in DispatcherCallbackArgs evt)
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
            if ((numRounds - roundsToReduce) > 0)
            {
                evt.SetConditionArg2(numRounds - roundsToReduce); // decrement the number of rounds
            }
            else
            {
                evt.SetConditionArg1(0); // set inactive
                evt.SetConditionArg2(0); // set to zero rounds
            }

            return;
        }
        public static void DivineShieldTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoTooltip();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            // Set the tooltip
            dispIo.Append($"Divine Shield ({evt.GetConditionArg2()} rounds)");
        }

        public static void DivineShieldEffectTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoEffectTooltip();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            // Set the tooltip
            dispIo.bdb.AddEntry(ElfHash.Hash("DIVINE_SHIELD"), $" ({evt.GetConditionArg2()} rounds)", -2);
            return;
        }

        // Setup the feat
        [FeatCondition("Divine Shield")]
        [AutoRegister] public static readonly ConditionSpec divineShieldFeat = ConditionSpec.Create("Divine Shield Feat", 2)
            .SetUnique()
            .AddHandler(DispatcherType.RadialMenuEntry, DivineShieldRadial)
            .Build();

            // Setup the condition added by the feat
            [AutoRegister] public static readonly ConditionSpec divineShieldEffect = ConditionSpec.Create("Divine Shield Effect", 2)
                .SetUnique()
                .AddHandler(DispatcherType.PythonActionCheck, divineShieldEnum, OnDivineShieldCheck)
                .AddHandler(DispatcherType.PythonActionPerform, divineShieldEnum, OnDivineShieldPerform)
                .AddHandler(DispatcherType.GetAC, DivineShieldAcBonus)
                .AddHandler(DispatcherType.BeginRound, DivineShieldBeginRound)
                .AddHandler(DispatcherType.Tooltip, DivineShieldTooltip)
                .AddHandler(DispatcherType.EffectTooltip, DivineShieldEffectTooltip)
                .AddHandler(DispatcherType.D20Signal, D20DispatcherKey.SIG_Inventory_Update,
                    DivineShieldInventoryUpdate)
                .Build();
    }
}
