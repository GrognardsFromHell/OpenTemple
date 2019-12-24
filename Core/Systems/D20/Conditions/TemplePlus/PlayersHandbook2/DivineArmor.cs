using System;
using System.Collections.Generic;
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
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    // Divine Armor:  Player's Handbook II, p. 88
    public class DivineArmor
    {
        private static readonly D20DispatcherKey divineArmorEnum = (D20DispatcherKey) 2600;

        public static void DivineArmorRadial(in DispatcherCallbackArgs evt)
        {
            // adds the "Divine Armor" condition on first radial menu build
            evt.objHndCaller.AddCondition("Divine Armor Effect", 0, 0);
            var radialAction = RadialMenuEntry.CreatePythonAction(divineArmorEnum, 0, "TAG_INTERFACE_HELP");
            radialAction.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Feats);
        }

        public static void OnDivineArmorCheck(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            // Get the current number of turn charges
            var TurnCharges = evt.objHndCaller.GetTurnUndeadCharges();
            // Check for remaining turn undead attempts
            if ((TurnCharges < 1))
            {
                dispIo.returnVal = ActionErrorCode.AEC_OUT_OF_CHARGES;
                return;
            }

            // Check that the character is not a fallen paladin without black guard levels
            if (evt.objHndCaller.D20Query(D20DispatcherKey.QUE_IsFallenPaladin) &&
                (evt.objHndCaller.GetStat(Stat.level_blackguard) == 0))
            {
                dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
            }
        }

        public static void OnDivineArmorPerform(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            // Set to active
            evt.SetConditionArg1(1);
            // Deduct a turn undead charge
            evt.objHndCaller.D20SendSignal("Deduct Turn Undead Charge");
            // Lasts one round always
            evt.SetConditionArg2(1);
            return;
        }

        public static void DivineArmorBeginRound(in DispatcherCallbackArgs evt)
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
            }

            return;
        }

        public static void DivineArmorDamageReduction(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoDamage();
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            var dr = 5; // Divine Armor Provides DR 5
            dispIo.damage.AddPhysicalDR(dr, D20AttackPower.UNSPECIFIED, 126); // type 1 - will always apply
        }

        public static void DivineArmorTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoTooltip();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            // Set the tooltip
            dispIo.Append("Divine Armor");
        }

        public static void DivineArmorEffectTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoEffectTooltip();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            // Set the tooltip
            dispIo.bdb.AddEntry(ElfHash.Hash("DIVINE_ARMOR"), "", -2);
        }

        // Setup the feat
        [FeatCondition("Divine Armor")]
        [AutoRegister] public static readonly ConditionSpec divineArmorFeat = ConditionSpec.Create("Divine Armor Feat", 2)
            .SetUnique()
            .AddHandler(DispatcherType.RadialMenuEntry, DivineArmorRadial)
            .Build();

        // Setup the effect
        [AutoRegister] public static readonly ConditionSpec divineArmorEffect = ConditionSpec.Create("Divine Armor Effect", 2)
            .SetUnique()
            .AddHandler(DispatcherType.PythonActionCheck, divineArmorEnum, OnDivineArmorCheck)
            .AddHandler(DispatcherType.PythonActionPerform, divineArmorEnum, OnDivineArmorPerform)
            .AddHandler(DispatcherType.BeginRound, DivineArmorBeginRound)
            .AddHandler(DispatcherType.Tooltip, DivineArmorTooltip)
            .AddHandler(DispatcherType.TakingDamage2, DivineArmorDamageReduction)
            .AddHandler(DispatcherType.EffectTooltip, DivineArmorEffectTooltip)
            .Build();
    }
}