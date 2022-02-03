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
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus;

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