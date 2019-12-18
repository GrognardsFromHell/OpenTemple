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
    public class BattleCaster
    {
        // Query is to be made from any class that allows a caster to wear some armor without arcane failure

        public static void ImprovedArcaneFailure(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = 1; // Return 1 to improve the class's arcane failure resistance for armor
        }

        // args are just-in-case placeholders
        [AutoRegister, FeatCondition("Battle Caster")]
        public static readonly ConditionSpec Condition = ConditionSpec.Create("Battle Caster", 2)
            .SetUnique()
            .AddQueryHandler("Improved Armored Casting", ImprovedArcaneFailure)
            .Build();
    }
}