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
    public class ExtraStunning
    {
        public static readonly FeatId ExtraStunningId = (FeatId) ElfHash.Hash("Extra Stunning");

        public static void ExtraStunningNewDay(in DispatcherCallbackArgs evt)
        {
            // Add 3 extra stunning attacks per feat taken
            var extraSmitingCount = GameSystems.Feat.HasFeatCount(evt.objHndCaller, ExtraStunningId);
            evt.SetConditionArg1(evt.GetConditionArg1() + 3 * extraSmitingCount);
        }

        [AutoRegister]
        public static readonly ConditionSpec ExtraStunningExtension = ConditionSpec
            .Extend(FeatConditions.featstunningfist)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, ExtraStunningNewDay)
            .Build();
    }
}