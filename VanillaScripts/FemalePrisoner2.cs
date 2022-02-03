
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

[ObjectScript(162)]
public class FemalePrisoner2 : BaseObjectScript
{

    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(131)))
        {
            triggerer.BeginDialog(attachee, 80);
        }
        else
        {
            triggerer.BeginDialog(attachee, 1);
        }

        return SkipDefault;
    }
    public static bool get_rep(GameObject attachee, GameObject triggerer)
    {
        if (!triggerer.HasReputation(7))
        {
            triggerer.AddReputation(7);
        }

        SetGlobalVar(25, GetGlobalVar(25) + 1);
        if ((GetGlobalVar(25) >= 3 && !triggerer.HasReputation(8)))
        {
            triggerer.AddReputation(8);
        }

        return RunDefault;
    }
    public static bool free_rep(GameObject attachee, GameObject triggerer)
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