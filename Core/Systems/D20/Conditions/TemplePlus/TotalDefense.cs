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

public class TotalDefense
{
    // The fighting defensively query needs to cover both fighting defensively and total defense
    public static void FightingDefensivelyQuery(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20Query();
        if ((evt.GetConditionArg1() != 0))
        {
            dispIo.return_val = 1;
        }
    }

    [AutoRegister]
    public static readonly ConditionSpec TotalDefenseExtension = ConditionSpec.Extend(StatusEffects.TotalDefense)
        .AddQueryHandler(D20DispatcherKey.QUE_FightingDefensively, FightingDefensivelyQuery)
        .Build();
}