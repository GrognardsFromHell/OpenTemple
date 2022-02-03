
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

[ObjectScript(86)]
public class AgentOfEvil : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        triggerer.BeginDialog(attachee, 1);
        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        if ((!PartyLeader.HasReputation(9)))
        {
            PartyLeader.AddReputation(9);
        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
        }
        else
        {
            if ((!GetGlobalFlag(528)))
            {
                if ((attachee.GetNameId() == 8073))
                {
                    if ((!Utilities.is_daytime()))
                    {
                        attachee.SetObjectFlag(ObjectFlag.OFF);
                    }
                    else if ((Utilities.is_daytime()))
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }

                }
                else if ((attachee.GetNameId() == 8074))
                {
                    if ((!Utilities.is_daytime()))
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }
                    else if ((Utilities.is_daytime()))
                    {
                        attachee.SetObjectFlag(ObjectFlag.OFF);
                    }

                }

            }

        }

        return RunDefault;
    }
    public static bool run_off(GameObject attachee, GameObject triggerer)
    {
        // loc = location_from_axis(427,406)
        // attachee.runoff(loc)
        attachee.RunOff();
        return RunDefault;
    }

}