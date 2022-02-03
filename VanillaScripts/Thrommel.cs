
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts;

[ObjectScript(149)]
public class Thrommel : BaseObjectScript
{

    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() != null))
        {
            triggerer.BeginDialog(attachee, 120);
        }
        else
        {
            triggerer.BeginDialog(attachee, 1);
        }

        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(150, true);
        if ((attachee.GetLeader() != null))
        {
            SetGlobalVar(29, GetGlobalVar(29) + 1);
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(150, false);
        return RunDefault;
    }
    public override bool OnNewMap(GameObject attachee, GameObject triggerer)
    {
        if (((attachee.GetMap() == 5062) || (attachee.GetMap() == 5093) || (attachee.GetMap() == 5113) || (attachee.GetMap() == 5001)))
        {
            var leader = attachee.GetLeader();

            if ((leader != null))
            {
                leader.BeginDialog(attachee, 130);
            }

        }

        return RunDefault;
    }
    public static bool run_off(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(151, true);
        attachee.RunOff();
        return RunDefault;
    }
    public static bool check_follower_thrommel_comments(GameObject attachee, GameObject triggerer)
    {
        var npc = Utilities.find_npc_near(attachee, 8014);

        if ((npc != null))
        {
            triggerer.BeginDialog(npc, 490);
        }
        else
        {
            npc = Utilities.find_npc_near(attachee, 8000);

            if ((npc != null))
            {
                triggerer.BeginDialog(npc, 550);
            }
            else
            {
                triggerer.BeginDialog(attachee, 10);
            }

        }

        return RunDefault;
    }
    public static bool schedule_reward(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(152, true);
        attachee.SetObjectFlag(ObjectFlag.OFF);
        StartTimer(1209600000, () => give_reward());
        return RunDefault;
    }
    public static bool give_reward()
    {
        QueueRandomEncounter(3001);
        return RunDefault;
    }


}