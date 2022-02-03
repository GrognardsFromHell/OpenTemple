
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

namespace Scripts;

[ObjectScript(127)]
public class Tillahi : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((!GetGlobalFlag(127)))
        {
            triggerer.BeginDialog(attachee, 1);
        }
        else
        {
            triggerer.BeginDialog(attachee, 140);
        }

        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        return RunDefault;
    }
    public static bool run_off(GameObject attachee, GameObject triggerer)
    {
        attachee.RunOff();
        return RunDefault;
    }
    public static bool set_reward(GameObject attachee, GameObject triggerer)
    {
        attachee.RunOff();
        StartTimer(864000000, () => give_reward()); // 864000000ms is 10 days
        ScriptDaemon.record_time_stamp("s_tillahi_reward"); // bulletproofed with Global Scheduling System - see py00439script_daemon.py
        return RunDefault;
    }
    public static bool give_reward()
    {
        QueueRandomEncounter(3002);
        ScriptDaemon.set_f("s_tillahi_reward_scheduled");
        return RunDefault;
    }
    public static bool get_rep(GameObject attachee, GameObject triggerer)
    {
        if (!triggerer.HasReputation(16))
        {
            triggerer.AddReputation(16);
        }

        SetGlobalVar(26, GetGlobalVar(26) + 1);
        if ((GetGlobalVar(26) >= 3 && !triggerer.HasReputation(17)))
        {
            triggerer.AddReputation(17);
        }

        return RunDefault;
    }

}