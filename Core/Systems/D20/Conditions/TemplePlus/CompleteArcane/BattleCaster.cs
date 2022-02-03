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