
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
    // Lingering Song, Complete Adventurer: p. 111
    public class LingeringSong
    {
        public static void QueryMaxBardicMusicExtraRounds(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoD20Query();
            // 5 Extra Rounds from this feat
            dispIo.return_val += 5;
        }

        // Extra, Extra
        [FeatCondition("Lingering Song")]
        [AutoRegister]
        public static readonly ConditionSpec Condition = ConditionSpec.Create("Lingering Song", 2)
            .SetUnique()
            .AddQueryHandler("Bardic Ability Duration Bonus", QueryMaxBardicMusicExtraRounds)
            .Build();
    }
}
