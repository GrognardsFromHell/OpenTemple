
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

[ObjectScript(38)]
public class HommletRefugees : BaseObjectScript
{
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() == 5006 || attachee.GetMap() == 5013 || attachee.GetMap() == 5014 || attachee.GetMap() == 5042))
        {
            if ((GetGlobalVar(510) != 2))
            {
                if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }
            else
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        SetGlobalVar(533, GetGlobalVar(533) + 1);
        if ((GetGlobalVar(533) >= 90))
        {
            SetGlobalVar(510, 2);
            SetGlobalFlag(504, true);
            PartyLeader.AddReputation(61);
        }

        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        return SkipDefault;
    }
    public override bool OnWillKos(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetNameId() == 20001))
        {
            return RunDefault;
        }

        if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8736))))
        {
            return RunDefault;
        }

        return SkipDefault;
    }

}