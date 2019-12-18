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
using SpicyTemple.Core.Systems.RadialMenus;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{
    public class RecklessCharge
    {
        public static void RecklessChargeRadial(in DispatcherCallbackArgs evt)
        {
            var toggle = evt.CreateToggleForArg(0);
            toggle.text = "Reckless Charge";
            toggle.helpSystemHashkey = "TAG_INTERFACE_HELP";
            toggle.AddAsChild(evt.objHndCaller, RadialMenuStandardNode.Feats);
        }

        public static void RecklessChargeHitBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            // Check if the feat is enabled
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            var charging = evt.objHndCaller.D20Query("Charging");
            // If charging apply the attack bonus
            if (charging)
            {
                dispIo.bonlist.AddBonus(2, 0, "Reckless Charge"); // +2 Bonus makes up for the -2 Rapid shot penalty
            }

            return;
        }

        public static void RecklessChargeACPenalty(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoAttackBonus();
            // Check if the feat is enabled
            if (evt.GetConditionArg1() == 0)
            {
                return;
            }

            var charging = evt.objHndCaller.D20Query("Charging");
            // If charging apply the ac penatly
            if (charging)
            {
                dispIo.bonlist.AddBonus(-2, 0, "Reckless Charge"); // Dodge bonus,  ~Class~[TAG_LEVEL_BONUSES]
            }

            return;
        }

        // Enabled, Place Holder
        [FeatCondition("Reckless Charge")]
        [AutoRegister] public static readonly ConditionSpec recklessCharge = ConditionSpec.Create("Reckless Charge", 2)
            .SetUnique()
            .AddHandler(DispatcherType.ToHitBonus2, RecklessChargeHitBonus)
            .AddHandler(DispatcherType.RadialMenuEntry, RecklessChargeRadial)
            .AddHandler(DispatcherType.GetAC, RecklessChargeACPenalty)
            .Build();
    }
}