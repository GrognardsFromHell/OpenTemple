
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

[ObjectScript(67)]
public class Turuko : BaseObjectScript
{

    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() == null))
        {
            if ((GetGlobalFlag(44)))
            {
                triggerer.BeginDialog(attachee, 200);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

        }
        else if ((GetGlobalFlag(44)))
        {
            triggerer.BeginDialog(attachee, 400);
        }
        else
        {
            triggerer.BeginDialog(attachee, 300);
        }

        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() != null))
        {
            SetGlobalVar(29, GetGlobalVar(29) + 1);
        }

        SetGlobalFlag(45, true);
        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(45, false);
        return RunDefault;
    }
    public override bool OnJoin(GameObject attachee, GameObject triggerer)
    {
        var obj = Utilities.find_npc_near(attachee, 8005);

        if ((obj != null))
        {
            triggerer.AddFollower(obj);
        }

        return RunDefault;
    }
    public override bool OnDisband(GameObject attachee, GameObject triggerer)
    {
        foreach (var obj in triggerer.GetPartyMembers())
        {
            if ((obj.GetNameId() == 8005))
            {
                triggerer.RemoveFollower(obj);
            }

        }

        return RunDefault;
    }
    public override bool OnNewMap(GameObject attachee, GameObject triggerer)
    {
        if (((attachee.GetMap() == 5062) || (attachee.GetMap() == 5113) || (attachee.GetMap() == 5093) || (attachee.GetMap() == 5002) || (attachee.GetMap() == 5091)))
        {
            var leader = attachee.GetLeader();

            if ((leader != null))
            {
                var percent = Utilities.group_percent_hp(leader);

                if ((percent < 30))
                {
                    if ((Utilities.obj_percent_hp(attachee) > 70))
                    {
                        leader.BeginDialog(attachee, 420);
                    }

                }

            }

        }

        return RunDefault;
    }
    public static bool run_off(GameObject attachee, GameObject triggerer)
    {
        attachee.RunOff();
        var obj = Utilities.find_npc_near(attachee, 8005);

        if ((obj != null))
        {
            obj.RunOff();
        }

        return RunDefault;
    }


}