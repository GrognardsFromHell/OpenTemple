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
public class ExtraWildShape
{
    public static readonly FeatId Id = (FeatId) ElfHash.Hash("Extra Wild Shape");

    public static void ExtraWildShapeQuery(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20Query();
        // Add 2 wild shape uses per feat taken
        var featCount = GameSystems.Feat.HasFeatCount(evt.objHndCaller, Id);
        dispIo.return_val = 2 * featCount;
    }

    public static void ExtraWildShapeElementalQuery(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20Query();
        // If over druid level 16, add one use per feat taken
        var druidLevel = evt.objHndCaller.GetStat(Stat.level_druid);
        if (druidLevel >= 16)
        {
            var featCount = GameSystems.Feat.HasFeatCount(evt.objHndCaller, Id);
            dispIo.return_val = featCount;
        }
    }

    [FeatCondition("Extra Wild Shape")]
    public static readonly ConditionSpec extraWildShapeFeat = ConditionSpec.Create("Extra Wild Shape Feat", 2, UniquenessType.Unique)
        .Configure(builder => builder
            .AddQueryHandler("Extra Wildshape Uses", ExtraWildShapeQuery)
            .AddQueryHandler("Extra Wildshape Elemental Uses", ExtraWildShapeElementalQuery)
        );
}