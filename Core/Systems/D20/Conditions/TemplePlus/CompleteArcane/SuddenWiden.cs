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
    public class SuddenWiden
    {
        // Sudden Widen:  Complete Arcane, p. 83
        private static void ApplyWiden(ref MetaMagicData metaMagicData)
        {
            // Don't widen more than once
            if (metaMagicData.metaMagicWidenSpellCount < 1)
            {
                metaMagicData.metaMagicWidenSpellCount = 1;
            }
        }

        // TODO GameSystems.Feats.AddMetamagicFeat("Sudden Widen");

        [AutoRegister, FeatCondition("Sudden Widen")]
        public static readonly ConditionSpec Condition = SuddenMetamagic
            .Create("Sudden Widen Feat", "Sudden Widen", ApplyWiden)
            .Build();
    }
}