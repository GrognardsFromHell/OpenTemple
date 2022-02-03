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

public class ExtraRage
{
    public static readonly FeatId ExtraRageId = (FeatId) ElfHash.Hash("Extra Rage");

    public static void ExtraRageNewDay(in DispatcherCallbackArgs evt)
    {
        var extraRageCount = GameSystems.Feat.HasFeatCount(evt.objHndCaller, ExtraRageId);
        // Extra Rage grands 2 additional uses or rage each time the feat is taken
        evt.SetConditionArg1(evt.GetConditionArg1() + 2 * extraRageCount);
    }

    [AutoRegister] public static readonly ConditionSpec BarbarianRageExtension = ConditionSpec
        .Extend(FeatConditions.BarbarianRage)
        .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, ExtraRageNewDay)
        .Build();
}