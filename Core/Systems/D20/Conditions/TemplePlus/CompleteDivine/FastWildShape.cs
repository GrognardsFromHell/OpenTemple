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
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
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