
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
    [ObjectScript(213)]
    public class Mona : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
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
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetQuestState(60, QuestState.Botched);
            return RunDefault;
        }
        public static bool buttin(GameObjectBody attachee, GameObjectBody triggerer, int line)
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
