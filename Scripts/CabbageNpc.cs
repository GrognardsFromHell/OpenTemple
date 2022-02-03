
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
    [ObjectScript(430)]
    public class CabbageNpc : BaseObjectScript
    {
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
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
