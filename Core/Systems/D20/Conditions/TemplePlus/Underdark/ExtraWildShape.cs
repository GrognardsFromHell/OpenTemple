using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Startup.Discovery;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{
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
        public static readonly ConditionSpec extraWildShapeFeat = ConditionSpec.Create("Extra Wild Shape Feat", 2)
            .SetUnique()
            .AddQueryHandler("Extra Wildshape Uses", ExtraWildShapeQuery)
            .AddQueryHandler("Extra Wildshape Elemental Uses", ExtraWildShapeElementalQuery)
            .Build();
    }
}