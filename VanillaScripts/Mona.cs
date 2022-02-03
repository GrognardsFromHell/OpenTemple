
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
    [ObjectScript(213)]
    public class Mona : BaseObjectScript
    {

        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((!attachee.HasMet(triggerer)))
            {
                if (GetQuestState(35) == QuestState.Completed)
                {
                    triggerer.BeginDialog(attachee, 300);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 1);
                }

            }
            else if ((GetQuestState(60) == QuestState.Botched))
            {
                triggerer.BeginDialog(attachee, 260);
            }
            else if ((GetQuestState(60) == QuestState.Completed && GetGlobalFlag(315)))
            {
                triggerer.BeginDialog(attachee, 400);
            }
            else if ((GetQuestState(60) == QuestState.Accepted))
            {
                triggerer.BeginDialog(attachee, 200);
            }
            else if ((GetQuestState(60) <= QuestState.Mentioned && !GetGlobalFlag(317)))
            {
                triggerer.BeginDialog(attachee, 470);
            }
            else
            {
                triggerer.BeginDialog(attachee, 330);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            SetQuestState(60, QuestState.Botched);
            return RunDefault;
        }
        public static bool buttin(GameObject attachee, GameObject triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8015);

            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 150);
            }

            return SkipDefault;
        }


    }
}
