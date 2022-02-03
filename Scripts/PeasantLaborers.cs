
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

[ObjectScript(87)]
public class PeasantLaborers : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(triggerer);
        if ((GetQuestState(96) == QuestState.Completed))
        {
            triggerer.BeginDialog(attachee, 110);
        }
        else
        {
            triggerer.BeginDialog(attachee, 1);
        }

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
            if ((attachee.GetNameId() != 8073 && attachee.GetNameId() != 8074 && attachee.GetNameId() != 14019))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else if ((attachee.GetNameId() == 8073))
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
            else if ((attachee.GetNameId() == 14019)) // otello of the field
            {
                if ((GetQuestState(9) == QuestState.Accepted))
                {
                    if ((!Utilities.is_daytime()))
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }
                    else
                    {
                        attachee.SetObjectFlag(ObjectFlag.OFF);
                    }

                }
                else
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }

        }

        return RunDefault;
    }

}