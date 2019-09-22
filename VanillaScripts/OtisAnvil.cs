
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(263)]
    public class OtisAnvil : BaseObjectScript
    {

        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((GetQuestState(32) == QuestState.Accepted) && (!(triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8014))))))
            {
                SetQuestState(32, QuestState.Completed);
            }

            return RunDefault;
        }


    }
}
