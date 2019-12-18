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
    // Versatile Unarmed Strike:  Player's Handbook II, p. 85
    public class VersatileUnarmedStrike
    {
        private static readonly D20DispatcherKey VersatileUnarmedStrikeBludgeoningEnum = (D20DispatcherKey) 2800;
        private static readonly D20DispatcherKey VersatileUnarmedStrikePiercingEnum = (D20DispatcherKey) 2801;
        private static readonly D20DispatcherKey VersatileUnarmedStrikeSlashingEnum = (D20DispatcherKey) 2802;

        public static DamageType GetDamageTypeFromEnum(D20DispatcherKey key)
        {
            // Determine the intended damage type
            if (key == VersatileUnarmedStrikePiercingEnum)
            {
                return DamageType.Piercing;
            }
            else if (key == VersatileUnarmedStrikeSlashingEnum)
            {
                return DamageType.Slashing;
            }

            return DamageType.Bludgeoning;
        }

        public static void VersatileUnarmedStrikeRadial(in DispatcherCallbackArgs evt)
        {
            var isAdded =
                evt.objHndCaller.AddCondition("Versatile Unarmed Strike", 0,
                    0); // adds the "Wolverine Rage" condition on first radial menu build
            var radial_parent = RadialMenuEntry.CreateParent("Versatile Unarmed Strike");
            var VersatileUnarmedStrikeId = radial_parent.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Feats);
            // 0 - Bludgeoning, 1 - Piercing, 2 - Slashing
            var radialAction = RadialMenuEntry.CreatePythonAction("Bludgeoning", D20ActionType.PYTHON_ACTION,
                VersatileUnarmedStrikeBludgeoningEnum, 0, "TAG_INTERFACE_HELP");
            radialAction.AddAsChild(evt.objHndCaller, VersatileUnarmedStrikeId);
            radialAction = RadialMenuEntry.CreatePythonAction("Piercing", D20ActionType.PYTHON_ACTION,
                VersatileUnarmedStrikePiercingEnum, 1, "TAG_INTERFACE_HELP");
            radialAction.AddAsChild(evt.objHndCaller, VersatileUnarmedStrikeId);
            radialAction = RadialMenuEntry.CreatePythonAction("Slashing", D20ActionType.PYTHON_ACTION,
                VersatileUnarmedStrikeSlashingEnum, 2, "TAG_INTERFACE_HELP");
            radialAction.AddAsChild(evt.objHndCaller, VersatileUnarmedStrikeId);
        }

        public static void OnVersatileUnarmedStrikeEffectCheck(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var damageType = GetDamageTypeFromEnum((D20DispatcherKey) dispIo.action.data1); // TODO I think this is wrong, treating it as the DK
            // Don't allow if it is a change to the same type as is already set
            if ((DamageType) evt.GetConditionArg1() == damageType)
            {
                dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                return;
            }

            // Don't allow if it has already been used this round
            if (evt.GetConditionArg2() != 0)
            {
                dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                return;
            }
        }

        public static void OnVersatileUnarmedStrikeEffectPerform(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            var damageType = GetDamageTypeFromEnum((D20DispatcherKey) dispIo.action.data1); // TODO I think this is wrong, treating it as the DK
            // Set to the appropriate damage type
            evt.SetConditionArg1((int) damageType);
            // Set the used this round flag
            evt.SetConditionArg2(1);
            return;
        }

        public static void VersatileUnarmedStrikeEffectBeginRound(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            // Clear the used this round flag
            evt.SetConditionArg2(0);
            return;
        }

        public static void VersatileUnarmedStrikeEffectTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoTooltip();
            // 0 is Bludgeoning since this is the default for unarmed combat, don't display
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            string damageType;
            if (evt.GetConditionArg1() == 1)
            {
                damageType = "Piercing";
            }
            else
            {
                damageType = "Slashing";
            }

            dispIo.Append("Versatile Unarmed Strike - " + damageType);
            return;
        }

        public static void VersatileUnarmedStrikeEffectTooltipEffect(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoEffectTooltip();
            // 0 is Bludgeoning since this is the default, don't display
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            string damageType;
            if (evt.GetConditionArg1() == 1)
            {
                damageType = "Piercing";
            }
            else
            {
                damageType = "Slashing";
            }

            // Set the tooltip
            dispIo.bdb.AddEntry(ElfHash.Hash("VERSATILE_UNARMED_STRIKE"), " - " + damageType, -2);
        }

        // Responds to the unarmed damage type query
        public static void VersatileUnarmedStrikeDamageType(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = evt.GetConditionArg1();
            return;
        }

        // Setup the feat
        // spare, spare
        [FeatCondition("Versatile Unarmed Strike")]
        [AutoRegister] public static readonly ConditionSpec VersatileUnarmedStrikeFeat = ConditionSpec
            .Create("Versatile Unarmed Strike Feat", 2)
            .SetUnique()
            .AddHandler(DispatcherType.RadialMenuEntry, VersatileUnarmedStrikeRadial)
            .Build();

        // Setup the effect
        // enabled, swapped this round, spare, spare
        [AutoRegister] public static readonly ConditionSpec VersatileUnarmedStrikeEffect = ConditionSpec
            .Create("Versatile Unarmed Strike", 4)
            .AddHandler(DispatcherType.BeginRound, VersatileUnarmedStrikeEffectBeginRound)
            .AddHandler(DispatcherType.PythonActionCheck, VersatileUnarmedStrikeBludgeoningEnum,
                OnVersatileUnarmedStrikeEffectCheck)
            .AddHandler(DispatcherType.PythonActionPerform, VersatileUnarmedStrikeBludgeoningEnum,
                OnVersatileUnarmedStrikeEffectPerform)
            .AddHandler(DispatcherType.PythonActionCheck, VersatileUnarmedStrikePiercingEnum,
                OnVersatileUnarmedStrikeEffectCheck)
            .AddHandler(DispatcherType.PythonActionPerform, VersatileUnarmedStrikePiercingEnum,
                OnVersatileUnarmedStrikeEffectPerform)
            .AddHandler(DispatcherType.PythonActionCheck, VersatileUnarmedStrikeSlashingEnum,
                OnVersatileUnarmedStrikeEffectCheck)
            .AddHandler(DispatcherType.PythonActionPerform, VersatileUnarmedStrikeSlashingEnum,
                OnVersatileUnarmedStrikeEffectPerform)
            .AddHandler(DispatcherType.Tooltip, VersatileUnarmedStrikeEffectTooltip)
            .AddHandler(DispatcherType.EffectTooltip, VersatileUnarmedStrikeEffectTooltipEffect)
            .AddQueryHandler("Unarmed Damage Type", VersatileUnarmedStrikeDamageType)
            .Build();
    }
}