
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

[ObjectScript(188)]
public class CaravanBandit : BaseObjectScript
{
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        SetGlobalVar(27, GetGlobalVar(27) + 1);
        if ((GetGlobalVar(27) >= 3))
        {
            GameSystems.MapObject.CreateObject(14316, attachee.GetLocation());
            SetGlobalVar(27, -1000);
        }

        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(triggerer);
        attachee.FloatLine(RandomRange(1, 3), triggerer);
        return RunDefault;
    }
    public override bool OnExitCombat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetStat(Stat.subdual_damage) >= attachee.GetStat(Stat.hp_current)))
        {
            SetGlobalVar(27, GetGlobalVar(27) + 1);
            if ((GetGlobalVar(27) >= 3))
            {
                GameSystems.MapObject.CreateObject(14316, attachee.GetLocation());
                SetGlobalVar(27, -1000);
            }

        }

        return RunDefault;
    }

}