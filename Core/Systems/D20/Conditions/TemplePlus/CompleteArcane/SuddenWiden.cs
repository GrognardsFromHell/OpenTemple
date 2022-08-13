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

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus;

public class SuddenWiden
{
    public static readonly FeatId Id = (FeatId) ElfHash.Hash("Sudden Widen");

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
        .Create("Sudden Widen Feat", "Sudden Widen", ApplyWiden);
}