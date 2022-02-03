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

public class ExtraMusic
{
    public static readonly FeatId ExtraMusicId = (FeatId) ElfHash.Hash("Extra Music");

    public static void EMNewDay(in DispatcherCallbackArgs evt)
    {
        var bardicMusicCount = GameSystems.Feat.HasFeatCount(evt.objHndCaller, ExtraMusicId);
        // Extra Music grants 4 additional uses of Bardic Music each time the feat is taken
        evt.SetConditionArg1(evt.GetConditionArg1() + bardicMusicCount * 4);
    }

    public static void QueryMaxBardicMusic(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20Query();
        // Total uses = bard level + extra music count * 4
        var bardicMusicCount = GameSystems.Feat.HasFeatCount(evt.objHndCaller, ExtraMusicId);
        dispIo.return_val += bardicMusicCount * 4;
    }

    [AutoRegister]
    public static readonly ConditionSpec eSF = ConditionSpec.Extend(BardicMusic.Condition)
        .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, EMNewDay)
        .AddQueryHandler("Max Bardic Music", QueryMaxBardicMusic)
        .Build();
}