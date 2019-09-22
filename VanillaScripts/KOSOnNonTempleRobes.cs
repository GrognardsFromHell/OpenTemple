
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
    [ObjectScript(210)]
    public class KOSOnNonTempleRobes : BaseObjectScript
    {

        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var saw_robe = false;

            foreach (var obj in triggerer.GetPartyMembers())
            {
                if (((obj.FindItemByName(3016) != null) || (obj.FindItemByName(3020) != null) || (obj.FindItemByName(3017) != null) || (obj.FindItemByName(3010) != null) || (obj.FindItemByName(3021) != null) || (GetQuestState(45) == QuestState.Completed) || (GetQuestState(48) == QuestState.Completed) || (GetQuestState(51) == QuestState.Completed) || (GetQuestState(54) == QuestState.Completed)))
                {
                    saw_robe = true;

                    break;

                }

            }

            if ((saw_robe))
            {
                return SkipDefault;
            }
            else
            {
                return RunDefault;
            }

        }


    }
}
