
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

[ObjectScript(132)]
public class OrcPrisoner2 : BaseObjectScript
{

    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() != null))
        {
            if ((!GetGlobalFlag(135)))
            {
                triggerer.BeginDialog(attachee, 70);
            }
            else
            {
                triggerer.BeginDialog(attachee, 110);
            }

        }
        else if (((!attachee.HasMet(triggerer)) || (!GetGlobalFlag(131))))
        {
            if ((GetGlobalFlag(131)))
            {
                triggerer.BeginDialog(attachee, 30);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

        }
        else if ((!GetGlobalFlag(135)))
        {
            triggerer.BeginDialog(attachee, 50);
        }
        else
        {
            triggerer.BeginDialog(attachee, 120);
        }

        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(134, true);
        if ((attachee.GetLeader() != null))
        {
            SetGlobalVar(29, GetGlobalVar(29) + 1);
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(134, false);
        return RunDefault;
    }
    public static bool tuelk_talk(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 8026);

        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(attachee);
            attachee.TurnTowards(npc);
        }
        else
        {
            triggerer.BeginDialog(attachee, 40);
        }

        return SkipDefault;
    }


}