using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
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