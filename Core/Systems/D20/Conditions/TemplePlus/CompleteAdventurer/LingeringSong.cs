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

// Lingering Song, Complete Adventurer: p. 111
public class LingeringSong
{
    public static void QueryMaxBardicMusicExtraRounds(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20Query();
        // 5 Extra Rounds from this feat
        dispIo.return_val += 5;
    }

    // Extra, Extra
    [FeatCondition("Lingering Song")]
    [AutoRegister]
    public static readonly ConditionSpec Condition = ConditionSpec.Create("Lingering Song", 2, UniquenessType.Unique)
        .Configure(builder => builder
            .AddQueryHandler("Bardic Ability Duration Bonus", QueryMaxBardicMusicExtraRounds)
        );
}