
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
    [ObjectScript(430)]
    public class CabbageNpc : BaseObjectScript
    {
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5008))
            {
                if ((GetGlobalFlag(56)))
                {
                    StartTimer(86390000, () => wench_no_longer_available());
                }

            }

            if ((attachee.GetMap() == 5061))
            {
                if ((GetGlobalFlag(289)))
                {
                    StartTimer(86390000, () => hostel_no_longer_available());
                }

            }

            if ((attachee.GetMap() == 5152))
            {
                if ((GetGlobalFlag(997)))
                {
                    StartTimer(86390000, () => goose_no_longer_available());
                }

            }

            return RunDefault;
        }
        public static bool wench_no_longer_available()
        {
            SetGlobalFlag(56, false);
            GameSystems.RandomEncounter.UpdateSleepStatus();
            return RunDefault;
        }
        public static bool hostel_no_longer_available()
        {
            SetGlobalFlag(289, false);
            GameSystems.RandomEncounter.UpdateSleepStatus();
            return RunDefault;
        }
        public static bool goose_no_longer_available()
        {
            SetGlobalFlag(997, false);
            GameSystems.RandomEncounter.UpdateSleepStatus();
            return RunDefault;
        }

    }
}
