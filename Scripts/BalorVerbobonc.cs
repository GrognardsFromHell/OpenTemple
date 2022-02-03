
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
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(364)]
    public class BalorVerbobonc : BaseObjectScript
    {
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(734) == 1))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                Sound(4139, 1);
            }
            else if ((GetGlobalVar(734) == 2))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            SetGlobalVar(749, GetGlobalVar(749) + 1);
            if ((GetGlobalVar(749) == 4))
            {
                SetQuestState(102, QuestState.Completed);
                PartyLeader.AddReputation(60);
                random_fate();
            }

            return RunDefault;
        }
        public override bool OnExitCombat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetMap() == 5121))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
                SetGlobalVar(734, 2);
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((obj.GetNameId() == 14263 || obj.GetNameId() == 14259 || obj.GetNameId() == 14286 || obj.GetNameId() == 14410))
                    {
                        obj.SetObjectFlag(ObjectFlag.OFF);
                    }

                }

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
