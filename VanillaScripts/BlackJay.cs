
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
    [ObjectScript(2)]
    public class BlackJay : BaseObjectScript
    {

        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if (((GetQuestState(3) == QuestState.Completed) || (GetQuestState(4) == QuestState.Completed)))
            {
                triggerer.BeginDialog(attachee, 20);
            }
            else if (((attachee.HasMet(triggerer)) || (GetQuestState(3) == QuestState.Accepted) || (GetQuestState(4) == QuestState.Accepted)))
            {
                triggerer.BeginDialog(attachee, 10);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            SetGlobalVar(23, GetGlobalVar(23) + 1);
            if ((GetGlobalVar(23) >= 2))
            {
                PartyLeader.AddReputation(1);
            }

            return RunDefault;
        }


    }
}
