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
        public static readonly ConditionSpec DestructionDomainExtension = ConditionSpec
            .Extend(DomainConditions.DestructionDomain)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, ExtraSmitingNewDay)
            .Build();

        [AutoRegister]
        public static readonly ConditionSpec SmiteEvilExtension = ConditionSpec.Extend(FeatConditions.SmiteEvil)
            .AddHandler(DispatcherType.NewDay, D20DispatcherKey.NEWDAY_REST, ExtraSmitingNewDay)
            .Build();
    }
}