using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
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