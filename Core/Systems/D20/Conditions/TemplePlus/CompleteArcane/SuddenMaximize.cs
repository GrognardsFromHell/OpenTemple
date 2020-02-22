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
    // Sudden Maximize:  Complete Arcane, p. 83
    public static class SuddenMaximize
    {
        public static readonly FeatId Id = (FeatId) ElfHash.Hash("Sudden Maximize");

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