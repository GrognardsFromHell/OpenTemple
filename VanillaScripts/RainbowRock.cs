
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
    [ObjectScript(265)]
    public class RainbowRock : BaseObjectScript
    {

        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetQuestState(27) == QuestState.Mentioned) || (GetQuestState(27) == QuestState.Accepted))
            {
                SetQuestState(27, QuestState.Completed);
            }

            return RunDefault;
        }
        public override bool OnRemoveItem(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetQuestState(27) == QuestState.Mentioned) || (GetQuestState(27) == QuestState.Accepted))
            {
                SetQuestState(27, QuestState.Completed);
            }

            return RunDefault;
        }


    }
}
