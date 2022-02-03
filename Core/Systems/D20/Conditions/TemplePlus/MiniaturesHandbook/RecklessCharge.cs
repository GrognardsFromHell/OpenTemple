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
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
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