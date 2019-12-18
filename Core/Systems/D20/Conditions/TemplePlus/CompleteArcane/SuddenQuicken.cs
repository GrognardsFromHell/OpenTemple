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
    // Sudden Quicken:  Complete Arcane, p. 83
    public class SuddenQuicken
    {
        private static void ApplyQuicken(ref MetaMagicData metaMagicData)
        {
            metaMagicData.IsQuicken = true;
        }

        // TODO GameSystems.Feats.AddMetamagicFeat("Sudden Quicken");

        [AutoRegister, FeatCondition("Sudden Quicken")]
        public static readonly ConditionSpec Condition = SuddenMetamagic
            .Create("Sudden Quicken Feat", "Sudden Quicken", ApplyQuicken)
            .Build();
    }
}