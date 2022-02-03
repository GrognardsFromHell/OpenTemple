
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

[ObjectScript(86)]
public class AgentOfEvil : BaseObjectScript
{

    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        triggerer.BeginDialog(attachee, 1);
        return SkipDefault;
    }
    public static bool run_off(GameObject attachee, GameObject triggerer)
    {
        attachee.RunOff();
        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (!PartyLeader.HasReputation(9))
        {
            PartyLeader.AddReputation(9);
        }

        return RunDefault;
    }


}