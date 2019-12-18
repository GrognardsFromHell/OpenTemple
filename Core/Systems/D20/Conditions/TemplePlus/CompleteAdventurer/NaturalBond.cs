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
}