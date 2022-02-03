
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

[ObjectScript(164)]
public class OgreForFire : BaseObjectScript
{
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() == 5114))
        {
            if ((GetQuestState(53) == 0))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
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

        SetGlobalVar(14, GetGlobalVar(14) + 1);
        if ((attachee.GetStat(Stat.subdual_damage) >= attachee.GetStat(Stat.hp_current)))
        {
            SetGlobalVar(13, GetGlobalVar(13) - 1);
        }

        return RunDefault;
    }
    public override bool OnExitCombat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetStat(Stat.subdual_damage) >= attachee.GetStat(Stat.hp_current)))
        {
            SetGlobalVar(13, GetGlobalVar(13) + 1);
        }

        if (GetGlobalVar(13) > 5)
        {
            SetGlobalVar(13, 5);
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalVar(14, GetGlobalVar(14) - 1);
        return RunDefault;
    }

}