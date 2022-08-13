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
    public static readonly ConditionSpec recklessRage = ConditionSpec.Create("Reckless Rage", 2, UniquenessType.Unique)
        .Configure(builder => builder
            .AddQueryHandler("Additional Rage Stat Bonus", RecklessRageAbilityBonus)
            .AddQueryHandler("Additional Rage AC Penalty", RecklessRageACPenalty)
        );
}