
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

[ObjectScript(158)]
public class Jaer : BaseObjectScript
{

    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() != null))
        {
            triggerer.BeginDialog(attachee, 100);
        }
        else if ((!attachee.HasMet(triggerer)))
        {
            triggerer.BeginDialog(attachee, 1);
        }
        else
        {
            triggerer.BeginDialog(attachee, 90);
        }

        return SkipDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive()))
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((Utilities.is_safe_to_talk(attachee, obj)))
                {
                    DetachScript();

                    obj.BeginDialog(attachee, 1);
                    return RunDefault;
                }

            }

        }

        return RunDefault;
    }
    public override bool OnNewMap(GameObject attachee, GameObject triggerer)
    {
        if (((attachee.GetArea() == 1) || (attachee.GetArea() == 3)))
        {
            var obj = attachee.GetLeader();

            if ((obj != null))
            {
                obj.BeginDialog(attachee, 140);
            }

        }

        return RunDefault;
    }
    public static bool transfer_fire_balls(GameObject attachee, GameObject triggerer)
    {
        while ((attachee.FindItemByName(2207) != null))
        {
            if (!attachee.TransferItemByNameTo(triggerer, 2207))
            {
                return RunDefault;
            }

        }

        return RunDefault;
    }
    public static bool run_off(GameObject attachee, GameObject triggerer)
    {
        attachee.RunOff();
        return RunDefault;
    }


}