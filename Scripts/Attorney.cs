
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

[ObjectScript(359)]
public class Attorney : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(triggerer);
        if ((GetGlobalVar(969) == 1))
        {
            triggerer.BeginDialog(attachee, 1);
        }
        else
        {
            return RunDefault;
        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalVar(969) == 2))
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
        }
        else if ((GetGlobalVar(969) != 2))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive()))
        {
            if ((is_better_to_talk(attachee, PartyLeader)))
            {
                if ((GetGlobalVar(969) == 2))
                {
                    attachee.TurnTowards(triggerer);
                    PartyLeader.BeginDialog(attachee, 1);
                    SetGlobalVar(969, 3);
                }
                else if ((GetGlobalVar(969) == 4 && GetGlobalVar(993) == 8))
                {
                    attachee.TurnTowards(triggerer);
                    PartyLeader.BeginDialog(attachee, 10);
                    SetGlobalVar(969, 5);
                }
                else if ((GetGlobalVar(969) == 4 && GetGlobalVar(993) != 8))
                {
                    attachee.TurnTowards(triggerer);
                    PartyLeader.BeginDialog(attachee, 240);
                    SetGlobalVar(969, 5);
                }

            }

        }

        return RunDefault;
    }
    public static bool is_better_to_talk(GameObject speaker, GameObject listener)
    {
        if ((speaker.DistanceTo(listener) <= 20))
        {
            return true;
        }

        return false;
    }
    public static bool clear_reps(GameObject attachee, GameObject triggerer)
    {
        PartyLeader.RemoveReputation(35); // Constable Killer
        PartyLeader.RemoveReputation(34); // Slaughterer of Verbobonc
        if ((GetGlobalVar(993) == 8))
        {
            SetGlobalVar(993, 9); // removes Dyvers rescuer murder arrest status
        }

        return RunDefault;
    }

}