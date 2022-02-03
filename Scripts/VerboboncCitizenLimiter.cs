
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
    [ObjectScript(324)]
    public class VerboboncCitizenLimiter : BaseObjectScript
    {
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetMap() == 5121 && GetGlobalVar(950) == 1))
            {
                StartTimer(43200000, () => twelve_hour_time_limit_to_kill_wilfrick()); // 12 hours
                SetGlobalVar(950, 2);
            }

            return RunDefault;
        }
        public static bool twelve_hour_time_limit_to_kill_wilfrick()
        {
            SetGlobalVar(704, 20);
            return RunDefault;
        }

    }
}
