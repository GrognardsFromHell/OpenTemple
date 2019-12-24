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
    // Divine Spell Power:  Complete Divine, p. 80
    public class DivineSpellPower
    {
        private static readonly D20DispatcherKey divineSpellPowerEnum = (D20DispatcherKey) 2603;

        public static void DivineSpellPowerRadial(in DispatcherCallbackArgs evt)
        {
            var isAdded =
                evt.objHndCaller.AddCondition("Divine Spell Power Effect", 0, 0, 0,
                    0); // adds the "Divine Spell Power" condition on first radial menu build
            var radialAction = RadialMenuEntry.CreatePythonAction(divineSpellPowerEnum, 0, "TAG_INTERFACE_HELP");
            radialAction.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Feats);
        }

        public static void OnDivineSpellPowerCheck(in DispatcherCallbackArgs evt)
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
                return;
            }

            // Don't allow a second use in a single round
            if (evt.GetConditionArg1() != 0)
            {
                dispIo.returnVal = ActionErrorCode.AEC_INVALID_ACTION;
                return;
            }
        }

        public static void OnDivineSpellPowerPerform(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20ActionTurnBased();
            // Set to active
            evt.SetConditionArg1(1);
            // Deduct a turn undead charge
            evt.objHndCaller.D20SendSignal("Deduct Turn Undead Charge");
            // Roll a turn undead check (charisma check) with a + 3 modifier
            var cha_bonus = (evt.objHndCaller.GetStat(Stat.charisma) - 10) / 2;
            var roll = RandomRange(1, 20) + cha_bonus + 3;
            // Calculate the level bonus
            var LevelBonus = (int) ((roll + 2) / 3) - 4;
            // Turn Undead Level Maxes out at + or - 4 levels
            LevelBonus = Math.Min(LevelBonus, 4);
            LevelBonus = Math.Max(LevelBonus, -4);
            // Set the Bonus
            evt.SetConditionArg2(LevelBonus);
        }

        public static void DivineSpellPowerBeginRound(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            evt.SetConditionArg1(0); // always remove at the begining of the round
            evt.SetConditionArg2(0); // set to zero bonus
            evt.SetConditionArg3(0); // set spell cast count to zero
        }

        public static void DivineSpellPowerCasterLevelBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            // If this is the second spell cast with spellpower, don't apply the bonus
            if (evt.GetConditionArg3() > 0)
            {
                return;
            }

            var spellPkt = (SpellPacketBody) dispIo.obj;
            if (spellPkt.IsDivine())
            {
                // Prevent invalid caster levels
                var casterBonus = evt.GetConditionArg2();
                var casterLevel = dispIo.return_val + casterBonus;
                dispIo.return_val = Math.Max(casterLevel, 1);
            }
        }

        public static void DivineSpellPowerTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoTooltip();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            // Set the tooltip
            dispIo.Append("Divine Spell Power");
            return;
        }

        public static void DivineSpellPowerEffectTooltip(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoEffectTooltip();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            // Once a spell has been cast disable the tooltip
            if (evt.GetConditionArg3() != 0)
            {
                return;
            }

            // Set the tooltip
            dispIo.bdb.AddEntry(ElfHash.Hash("DIVINE_SPELL_POWER"), $" (caster level bonus: {evt.GetConditionArg2()})", -2);
        }

        public static void DivineSpellPowerCastSpell(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Signal();
            // not active, do nothing
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            // Incriment the spell cast with spell power count
            var sepllCastCount = evt.GetConditionArg3();
            evt.SetConditionArg3(sepllCastCount + 1);
            return;
        }

        // Setup the feat
        [FeatCondition("Divine Spell Power")]
        [AutoRegister] public static readonly ConditionSpec DivineSpellPowerFeat = ConditionSpec.Create("Divine Spell Power Feat", 4)
            .AddHandler(DispatcherType.RadialMenuEntry, DivineSpellPowerRadial)
            .Build();

        // Setup the effect
        // Enabled, Bonus, Extra, Extra
        [AutoRegister] public static readonly ConditionSpec DivineSpellPowerEffect = ConditionSpec
            .Create("Divine Spell Power Effect", 4)
            .SetUnique()
            .AddHandler(DispatcherType.PythonActionCheck, divineSpellPowerEnum, OnDivineSpellPowerCheck)
            .AddHandler(DispatcherType.PythonActionPerform, divineSpellPowerEnum, OnDivineSpellPowerPerform)
            .AddHandler(DispatcherType.BeginRound, DivineSpellPowerBeginRound)
            .AddHandler(DispatcherType.Tooltip, DivineSpellPowerTooltip)
            .AddHandler(DispatcherType.BaseCasterLevelMod, DivineSpellPowerCasterLevelBonus)
            .AddHandler(DispatcherType.EffectTooltip, DivineSpellPowerEffectTooltip)
            .AddHandler(DispatcherType.D20Signal, D20DispatcherKey.SIG_Spell_Cast, DivineSpellPowerCastSpell)
            .Build();
    }
}