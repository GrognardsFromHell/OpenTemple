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

public class ExtendRage
{
    public static void AddCondition(in DispatcherCallbackArgs evt)
    {
        // Add 5 rounds for the extend rage feat
        if (evt.objHndCaller.HasFeat((FeatId) ElfHash.Hash("Extend Rage")))
        {
            evt.SetConditionArg1(evt.GetConditionArg1() + 5);
        }
    }

    [AutoRegister]
    public static readonly ConditionSpec ExtendRageExtension = StatusEffects.BarbarianRaged.Extend(builder => builder
        .AddHandler(DispatcherType.ConditionAdd, AddCondition)
    );
}