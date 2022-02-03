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
using JetBrains.Annotations;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
{
    // Sudden Empower:  Complete Arcane, p. 83
    public class SuddenEmpower
    {

        public static readonly FeatId Id = (FeatId) ElfHash.Hash("Sudden Empower");

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