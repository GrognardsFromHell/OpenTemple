
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
    [ObjectScript(3)]
    public class AverageFarmer : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5001) && (GetGlobalVar(4) <= 4))
            {
                triggerer.BeginDialog(attachee, 110);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetQuestState(9) >= QuestState.Accepted))
            {
                SetGlobalVar(4, 1);
            }

            if (!PartyLeader.HasReputation(9))
            {
                PartyLeader.AddReputation(9);
            }

            return RunDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5001) && (GetGlobalVar(4) <= 1))
            {
                if ((GetQuestState(9) == QuestState.Accepted))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }
                else
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((GetGlobalVar(4) == 5))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else if ((GetGlobalVar(4) == 3))
            {
                SetGlobalVar(4, 4);
                attachee.SetStandpoint(StandPointType.Night, 230);
                attachee.SetStandpoint(StandPointType.Day, 230);
                attachee.SetObjectFlag(ObjectFlag.OFF);
                SetGlobalFlag(204, true);
                SetGlobalVar(24, GetGlobalVar(24) + 1);
                if ((!PartyLeader.HasReputation(5)))
                {
                    PartyLeader.AddReputation(5);
                }

                if (((GetGlobalVar(24) >= 3) && (!PartyLeader.HasReputation(6))))
                {
                    PartyLeader.AddReputation(6);
                }

            }
            else if ((GetQuestState(9) >= QuestState.Completed))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }


    }
}
