
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
    [ObjectScript(62)]
    public class Rannosdavl : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetQuestState(16) == QuestState.Completed && GetQuestState(15) == QuestState.Completed))
            {
                triggerer.BeginDialog(attachee, 20);
            }
            else if ((GetGlobalFlag(41) || GetQuestState(16) == QuestState.Completed))
            {
                triggerer.BeginDialog(attachee, 290);
            }
            else if ((GetQuestState(17) == QuestState.Completed))
            {
                triggerer.BeginDialog(attachee, 30);
            }
            else if ((GetGlobalFlag(31)))
            {
                triggerer.BeginDialog(attachee, 50);
            }
            else if ((attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 70);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((triggerer.type == ObjectType.pc))
            {
                var raimol = Utilities.find_npc_near(attachee, 8050);

                if ((raimol != null))
                {
                    attachee.FloatLine(380, triggerer);
                    var leader = raimol.GetLeader();

                    if ((leader != null))
                    {
                        leader.RemoveFollower(raimol);
                    }

                    raimol.Attack(triggerer);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (!PartyLeader.HasReputation(9))
            {
                PartyLeader.AddReputation(9);
            }

            return RunDefault;
        }


    }
}
