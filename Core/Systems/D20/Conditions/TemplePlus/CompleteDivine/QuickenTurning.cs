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
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus;

// Quicken Turning:  Complete Divine, p. 84
public class QuickenTurning
{
    private static void QuickenTurningBeginRound(in DispatcherCallbackArgs evt)
    {
        evt.SetConditionArg1(0); // reset turn undead used
    }

    private static void TurnUndeadDisabled(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20Query();
        dispIo.return_val = evt.GetConditionArg1(); // Returns 1 if turn undead has already been used
    }

    private static void TurnUndeadPerform(in DispatcherCallbackArgs evt)
    {
        evt.SetConditionArg1(1); // Set the flag that turn undead has been used this round
    }

    public static void QuickenTurningCostMod(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetEvtObjActionCost();
        if (dispIo.d20a.d20ActType == D20ActionType.TURN_UNDEAD)
        {
            // Always a free action with the feat
            dispIo.acpCur.hourglassCost = ActionCostType.Null;
        }
    }

    // First argument is whether or not turn undead is used, second is extra
    [FeatCondition("Quicken Turning")]
    [AutoRegister]
    public static readonly ConditionSpec Condition = ConditionSpec.Create("Quicken Turning Feat", 2, UniquenessType.Unique)
        .Configure(builder => builder
            .AddHandler(DispatcherType.BeginRound, QuickenTurningBeginRound)
            .AddQueryHandler("Turn Undead Disabled", TurnUndeadDisabled)
            .AddSignalHandler("Turn Undead Perform", TurnUndeadPerform)
            .AddHandler(DispatcherType.ActionCostMod, QuickenTurningCostMod)
        );
}