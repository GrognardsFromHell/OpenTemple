
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
    [ObjectScript(366)]
    public class StcuthbertVerbobonc : BaseObjectScript
    {
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(698) == 1))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                Sound(4138, 1);
            }
            else if ((GetGlobalVar(698) == 2))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(695, GetGlobalVar(695) + 1);
            if ((GetGlobalVar(695) == 4))
            {
                SetQuestState(102, QuestState.Completed);
                PartyLeader.AddReputation(59);
                random_fate();
            }

            return RunDefault;
        }
        public override bool OnExitCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5121))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
                SetGlobalVar(698, 2);
            }

            return RunDefault;
        }
        public static bool random_fate()
        {
            var pendulum = RandomRange(1, 5);
            if ((pendulum == 1 || pendulum == 2 || pendulum == 3))
            {
                SetGlobalVar(508, 1);
            }
            else if ((pendulum == 4))
            {
                SetGlobalVar(508, 2);
            }
            else if ((pendulum == 5))
            {
                SetGlobalVar(508, 3);
            }

            return RunDefault;
        }

    }
}
