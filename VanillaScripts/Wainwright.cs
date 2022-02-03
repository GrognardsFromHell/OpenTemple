
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

[ObjectScript(58)]
public class Wainwright : BaseObjectScript
{

    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8001))))
        {
            triggerer.BeginDialog(attachee, 150);
        }
        else if ((attachee.GetMap() == 5007))
        {
            triggerer.BeginDialog(attachee, 340);
        }
        else if ((GetGlobalFlag(38)))
        {
            triggerer.BeginDialog(attachee, 200);
        }
        else if ((GetGlobalFlag(149)))
        {
            triggerer.BeginDialog(attachee, 220);
        }
        else
        {
            triggerer.BeginDialog(attachee, 1);
        }

        return SkipDefault;
    }
    public static bool make_hate(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetReaction(triggerer) >= 20))
        {
            attachee.SetReaction(triggerer, 20);
        }

        return SkipDefault;
    }


}