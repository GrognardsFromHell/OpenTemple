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
using JetBrains.Annotations;
using SpicyTemple.Core.Startup.Discovery;
using SpicyTemple.Core.Systems.RadialMenus;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{
    // Sudden Empower:  Complete Arcane, p. 83
    public class SuddenEmpower
    {
        private static void ApplyEmpower(ref MetaMagicData metaMagicData)
        {
            // Don't Empower more than once
            if (metaMagicData.metaMagicEmpowerSpellCount < 1)
            {
                metaMagicData.metaMagicEmpowerSpellCount = 1;
            }
        }

        // TODO GameSystems.Feats.AddMetamagicFeat("Sudden Empower");

        [AutoRegister, FeatCondition("Sudden Empower")]
        public static readonly ConditionSpec Condition = SuddenMetamagic
            .Create("Sudden Empower Feat", "Sudden Empower", ApplyEmpower)
            .Build();
    }
}