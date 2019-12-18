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
    public class TellingBlow
    {
        // Used by temple+.  Returns 1 if sneak attack damage should be done on a critial.
        // Always returns true when the character has the telling blow feat.
        public static void SneakAttackOnCritical(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            dispIo.return_val = 1; // Turn On For Telling Blow
        }

        // args are just-in-case placeholders
        [FeatCondition("Telling Blow")]
        [AutoRegister] public static readonly ConditionSpec tellingBlow = ConditionSpec.Create("Telling Blow", 2)
            .SetUnique()
            .AddQueryHandler("Sneak Attack Critical", SneakAttackOnCritical)
            .Build();
    }
}