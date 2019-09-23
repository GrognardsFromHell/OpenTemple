
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
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(95)]
    public class BrauApprentice2 : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((SelectedPartyLeader.HasReputation(32) || SelectedPartyLeader.HasReputation(30) || SelectedPartyLeader.HasReputation(29)))
            {
                attachee.FloatLine(11004, triggerer);
            }
            else
            {
                if ((attachee.GetMap() == 5007))
                {
                    if ((GetQuestState(19) == QuestState.Unknown))
                    {
                        triggerer.BeginDialog(attachee, 1);
                    }
                    else if ((GetQuestState(19) == QuestState.Mentioned))
                    {
                        triggerer.BeginDialog(attachee, 40);
                    }
                    else if ((GetQuestState(19) == QuestState.Accepted || GetQuestState(19) == QuestState.Botched))
                    {
                        triggerer.BeginDialog(attachee, 50);
                    }
                    else
                    {
                        triggerer.BeginDialog(attachee, 60);
                    }

                }
                else
                {
                    triggerer.BeginDialog(attachee, 90);
                }

            }

            return SkipDefault;
        }
        public static bool find_ostler(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var npc = Utilities.find_npc_near(attachee, 8008);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, 230);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 80);
            }

            return SkipDefault;
        }

    }
}
