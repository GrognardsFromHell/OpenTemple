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
    public class TotalDefense
    {
        // The fighting defensively query needs to cover both fighting defensively and total defense
        public static void FightingDefensivelyQuery(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            if ((evt.GetConditionArg1() != 0))
            {
                dispIo.return_val = 1;
            }
        }

        [AutoRegister]
        public static readonly ConditionSpec TotalDefenseExtension = ConditionSpec.Extend(StatusEffects.TotalDefense)
            .AddQueryHandler(D20DispatcherKey.QUE_FightingDefensively, FightingDefensivelyQuery)
            .Build();
    }
}