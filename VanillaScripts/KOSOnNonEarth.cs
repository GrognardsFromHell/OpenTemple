
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

[ObjectScript(207)]
public class KOSOnNonEarth : BaseObjectScript
{

    public override bool OnWillKos(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(347)))
        {
            return SkipDefault;
        }

        var saw_ally_robe = false;

        var saw_greater_robe = false;

        var saw_enemy_robe = false;

        foreach (var obj in triggerer.GetPartyMembers())
        {
            if ((obj.FindItemByName(3010) != null))
            {
                saw_ally_robe = true;

            }
            else if ((obj.FindItemByName(3021) != null))
            {
                saw_greater_robe = true;

                break;

            }
            else if (((obj.FindItemByName(3020) != null) || (obj.FindItemByName(3016) != null) || (obj.FindItemByName(3017) != null)))
            {
                saw_enemy_robe = true;

            }

        }

        if (saw_greater_robe)
        {
            return SkipDefault;
        }
        else if (saw_ally_robe && !saw_enemy_robe)
        {
            return SkipDefault;
        }
        else
        {
            return RunDefault;
        }

    }


}