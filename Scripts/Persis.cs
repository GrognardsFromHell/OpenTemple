
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

[ObjectScript(348)]
public class Persis : BaseObjectScript
{
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalVar(993) == 2))
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
        }
        else if ((GetGlobalVar(993) == 3))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        SetGlobalFlag(954, true);
        if ((GetGlobalFlag(948) && GetGlobalFlag(949) && GetGlobalFlag(950) && GetGlobalFlag(951) && GetGlobalFlag(952) && GetGlobalFlag(953)))
        {
            PartyLeader.AddReputation(40);
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(954, false);
        PartyLeader.RemoveReputation(40);
        return RunDefault;
    }

}