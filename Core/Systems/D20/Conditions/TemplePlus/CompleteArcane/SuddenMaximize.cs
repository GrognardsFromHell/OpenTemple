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
    // Sudden Maximize:  Complete Arcane, p. 83
    public static class SuddenMaximize
    {
        private static void ApplyMaximize(ref MetaMagicData metaMagicData)
        {
            // Don't Maximize more than once
            metaMagicData.IsMaximize = true;
        }

        // TODO GameSystems.Feats.AddMetamagicFeat("Sudden Maximize");

        [AutoRegister, FeatCondition("Sudden Maximize")]
        public static readonly ConditionSpec Condition = SuddenMetamagic
            .Create("Sudden Maximize Feat", "Sudden Maximize", ApplyMaximize)
            .Build();
    }
}