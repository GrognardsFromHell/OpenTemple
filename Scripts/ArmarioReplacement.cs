
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

[ObjectScript(36)]
public class ArmarioReplacement : BaseObjectScript
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
        else if ((GetGlobalVar(967) == 2)) // turns on substitute armario
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
            if ((!GetGlobalFlag(910)))
            {
                StartTimer(604800000, () => respawn_armario(attachee)); // 604800000ms is 1 week
                SetGlobalFlag(910, true);
            }

        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(297, true);
        SetGlobalVar(23, GetGlobalVar(23) + 1);
        if ((GetGlobalVar(23) >= 2))
        {
            PartyLeader.AddReputation(92);
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(297, false);
        return RunDefault;
    }
    public static void respawn_armario(GameObject attachee)
    {
        var box = Utilities.find_container_near(attachee, 1004);
        InventoryRespawn.RespawnInventory(box);
        StartTimer(604800000, () => respawn_armario(attachee)); // 604800000ms is 1 week
        return;
    }

}