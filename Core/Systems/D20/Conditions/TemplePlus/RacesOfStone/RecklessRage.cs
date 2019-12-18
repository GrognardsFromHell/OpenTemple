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
    [AutoRegister]
    public class RecklessRage
    {
        public static void RecklessRageAbilityBonus(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = 2; // +2 to con and str (used by the C++ side)
        }

        public static void RecklessRageACPenalty(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = 2; // -2 to AC (Value returned must be positive, it will be negated by the C++ side)
        }

        // args are just-in-case placeholders
        [FeatCondition("Reckless Rage")]
        public static readonly ConditionSpec recklessRage = ConditionSpec.Create("Reckless Rage", 2)
            .SetUnique()
            .AddQueryHandler("Additional Rage Stat Bonus", RecklessRageAbilityBonus)
            .AddQueryHandler("Additional Rage AC Penalty", RecklessRageACPenalty)
            .Build();

    }
}