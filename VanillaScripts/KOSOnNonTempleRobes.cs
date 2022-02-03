
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(210)]
    public class KOSOnNonTempleRobes : BaseObjectScript
    {

        public override bool OnWillKos(GameObject attachee, GameObject triggerer)
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
