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

// Fix greater turning so it is usable once per day and uses up a turn undead charge (bug 071).
// SRD:  Once per day, you can perform a greater turning against undead in place of a
// regular turning. The greater turning is like a normal turning except that the undead
// creatures that would be turned are destroyed instead.
public class GreaterTurnUndead
{
    // Called to deduct a turn undead charge
    public static void DeductGreaterTurnUndeadCharge(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20Signal();
        var NewCharges = evt.GetConditionArg2() - 1;
        if (NewCharges > 0)
        {
            evt.SetConditionArg2(NewCharges);
        }
        else
        {
            evt.SetConditionArg2(0);
        }
    }

    public static void OnNewDay(in DispatcherCallbackArgs evt)
    {
        evt.SetConditionArg2(1); // One Charge Per Day
    }

    [AutoRegister] public static readonly ConditionSpec GreaterTurningExtension = ConditionSpec
        .Extend(DomainConditions.GreaterTurning)
        .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, OnNewDay)
        .AddSignalHandler("Deduct Greater Turn Undead Charge", DeductGreaterTurnUndeadCharge)
        .Build();
}