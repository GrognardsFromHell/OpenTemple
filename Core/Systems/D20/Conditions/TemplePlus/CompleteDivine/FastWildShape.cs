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

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    // Fast Wild Shape:  Complete Divine, p. 81
    public class FastWildShape
    {
        public static void FastWildShapeCostMod(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetEvtObjActionCost();
            if (dispIo.d20a.d20ActType != D20ActionType.CLASS_ABILITY_SA)
            {
                return;
            }

            var WildshapeValue = 1 << 24;
            // Check for the wildshape bit
            if (((dispIo.d20a.data1 & WildshapeValue)) != 0)
            {
                // Always a move action with the feat
                dispIo.acpCur.hourglassCost = ActionCostType.Move;
            }
        }

        // First argument is the wildshape, second is extra
        [FeatCondition("Fast Wild Shape")]
        [AutoRegister] public static readonly ConditionSpec Condition = ConditionSpec.Create("Fast Wild Shape Feat", 2)
            .SetUnique()
            .AddHandler(DispatcherType.ActionCostMod, FastWildShapeCostMod)
            .Build();
    }
}