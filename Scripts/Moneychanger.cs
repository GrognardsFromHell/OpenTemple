
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

[ObjectScript(55)]
public class Moneychanger : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        triggerer.BeginDialog(attachee, 1);
        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
        }
        else
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
            if ((!GetGlobalFlag(905)))
            {
                StartTimer(86400000, () => respawn(attachee)); // 86400000ms is 24 hours
                SetGlobalFlag(905, true);
            }

        }

        return RunDefault;
    }
    public static void respawn(GameObject attachee)
    {
        var box = Utilities.find_container_near(attachee, 1001);
        InventoryRespawn.RespawnInventory(box);
        StartTimer(86400000, () => respawn(attachee)); // 86400000ms is 24 hours
        return;
    }

}