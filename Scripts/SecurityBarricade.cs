
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

[ObjectScript(360)]
public class SecurityBarricade : BaseObjectScript
{
    public override bool OnUse(GameObject door, GameObject triggerer)
    {
        if ((door.GetNameId() == 1621))
        {
            if ((!GetGlobalFlag(966)))
            {
                // if security barricade is active, disable outside door portal
                return SkipDefault;
            }
            else
            {
                // do normal transition
                return RunDefault;
            }

        }
        else if ((door.GetNameId() == 1623))
        {
            if ((!GetGlobalFlag(966)))
            {
                // if security barricade is active, disable inside door portal
                return SkipDefault;
            }
            else
            {
                // do regional patrol dues flag routine and normal transition
                if ((!GetGlobalFlag(260)))
                {
                    SetGlobalFlag(260, true);
                }

                return RunDefault;
            }

        }
        return RunDefault;
    }
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.HasMet(triggerer)))
        {
            triggerer.BeginDialog(attachee, 10);
        }
        else
        {
            triggerer.BeginDialog(attachee, 1);
        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(966)))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
        }
        else if ((!GetGlobalFlag(966)))
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
        }

        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        var leader = PartyLeader;
        Co8.StopCombat(attachee, 0);
        leader.BeginDialog(attachee, 4000);
        return RunDefault;
    }

}