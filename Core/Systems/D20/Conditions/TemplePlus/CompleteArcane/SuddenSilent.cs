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
    // Sudden Silent:  Complete Arcane, p. 83
    public class SuddenSilent
    {
        private static void ApplySilent(ref MetaMagicData metaMagicData)
        {
            metaMagicData.IsSilent = true;
        }

        // TODO GameSystems.Feats.AddMetamagicFeat("Sudden Silent");

        [AutoRegister, FeatCondition("Sudden Silent")]
        public static readonly ConditionSpec Condition = SuddenMetamagic
            .Create("Sudden Silent Feat", "Sudden Silent", ApplySilent)
            .Build();
    }
}