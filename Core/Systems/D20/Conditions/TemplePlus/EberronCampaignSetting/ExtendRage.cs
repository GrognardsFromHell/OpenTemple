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
    public class ExtendRage
    {
        public static void AddCondition(in DispatcherCallbackArgs evt)
        {
            // Add 5 rounds for the extend rage feat
            if (evt.objHndCaller.HasFeat((FeatId) ElfHash.Hash("Extend Rage")))
            {
                evt.SetConditionArg1(evt.GetConditionArg1() + 5);
            }
        }

        [AutoRegister] public static readonly ConditionSpec ExtendRageExtension = ConditionSpec.Extend(StatusEffects.BarbarianRaged)
            .AddHandler(DispatcherType.ConditionAdd, AddCondition)
            .Build();
    }
}