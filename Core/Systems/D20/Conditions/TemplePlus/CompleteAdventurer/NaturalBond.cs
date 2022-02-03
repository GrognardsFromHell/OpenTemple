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

public class NaturalBond
{
    // Natural Bond:   Complete Adventurer, p. 111

    public static void QueryNaturalBond(in DispatcherCallbackArgs evt)
    {
        var dispIo = evt.GetDispIoD20Query();
        var animalCompanionLevel = 0;
        // Add Druid Level to Ranger effective druid level
        var rangerLevel = evt.objHndCaller.GetStat(Stat.level_ranger);
        if (rangerLevel >= 4)
        {
            animalCompanionLevel = rangerLevel / 2;
        }

        animalCompanionLevel += evt.objHndCaller.GetStat(Stat.level_druid);
        var characterLevel = evt.objHndCaller.GetStat(Stat.level);
        var nonAnimalCompanionLevel = characterLevel - animalCompanionLevel;
        // Level bonus is up to 3 levels but not more than the character level
        var levelBonus = Math.Min(nonAnimalCompanionLevel, 3);
        // Return the bonus
        dispIo.return_val += levelBonus;
    }
    // spare, spare

    [FeatCondition("Natural Bond"), AutoRegister]
    public static readonly ConditionSpec Condition = ConditionSpec.Create("Natural Bond Feat", 2)
        .SetUnique()
        .AddQueryHandler("Animal Companion Level Bonus", QueryNaturalBond)
        .Build();
}