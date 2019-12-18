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
}