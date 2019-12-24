
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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
    [ObjectScript(98)]
    public class Ophelia : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!attachee.HasMet(triggerer)))
            {
                if ((GetQuestState(35) <= QuestState.Accepted))
                {
                    triggerer.BeginDialog(attachee, 1);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 430);
                }

            }
            else if ((GetGlobalFlag(75)))
            {
                triggerer.BeginDialog(attachee, 580);
            }
            else if ((GetQuestState(35) == QuestState.Completed))
            {
                triggerer.BeginDialog(attachee, 790);
            }
            else
            {
                triggerer.BeginDialog(attachee, 700);
            }

            return SkipDefault;
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
                triggerer.BeginDialog(attachee, 760);
            }

            return SkipDefault;
        }


    }
}
