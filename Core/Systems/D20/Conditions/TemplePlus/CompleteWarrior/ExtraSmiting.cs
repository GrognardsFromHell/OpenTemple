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

public class ExtraSmiting
{
    public static readonly FeatId ExtraSmitingId = (FeatId) ElfHash.Hash("Extra Smiting");

    public static void ExtraSmitingNewDay(in DispatcherCallbackArgs evt)
    {
        var extraSmitingCount = GameSystems.Feat.HasFeatCount(evt.objHndCaller, ExtraSmitingId);

        // Extra Smiting grants 2 additional uses of smite each time the feat is taken
        evt.SetConditionArg1(evt.GetConditionArg1() + 2 * extraSmitingCount);
    }

    [AutoRegister]
    public static readonly ConditionSpec DestructionDomainExtension = DomainConditions.DestructionDomain.Extend(builder => builder
        .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, ExtraSmitingNewDay)
    );

    [AutoRegister]
    public static readonly ConditionSpec SmiteEvilExtension = FeatConditions.SmiteEvil.Extend(builder => builder
        .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, ExtraSmitingNewDay)
    );
}