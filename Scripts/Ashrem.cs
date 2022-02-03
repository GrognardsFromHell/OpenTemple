
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

[ObjectScript(175)]
public class Ashrem : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() != null))
        {
            triggerer.BeginDialog(attachee, 170);
        }
        else if ((!attachee.HasMet(triggerer)))
        {
            triggerer.BeginDialog(attachee, 30);
        }
        else
        {
            triggerer.BeginDialog(attachee, 150);
        }

        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        attachee.FloatLine(12014, triggerer);
        if ((attachee.GetLeader() != null))
        {
            SetGlobalVar(29, GetGlobalVar(29) + 1);
        }

        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        attachee.FloatLine(12057, triggerer);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive()))
        {
            var alrrem = Utilities.find_npc_near(attachee, 8047);
            if ((alrrem != null))
            {
                if ((!GetGlobalFlag(192)))
                {
                    triggerer.BeginDialog(attachee, 230);
                }

            }

        }

        return RunDefault;
    }
    public static bool talk_Taki(GameObject attachee, GameObject triggerer, int line)
    {
        var taki = Utilities.find_npc_near(attachee, 8039);
        if ((taki != null))
        {
            triggerer.BeginDialog(taki, line);
            taki.TurnTowards(attachee);
            attachee.TurnTowards(taki);
        }

        return SkipDefault;
    }
    public static bool talk_Alrrem(GameObject attachee, GameObject triggerer, int line)
    {
        var alrrem = Utilities.find_npc_near(attachee, 8047);
        if ((alrrem != null))
        {
            triggerer.BeginDialog(alrrem, line);
            alrrem.TurnTowards(attachee);
            attachee.TurnTowards(alrrem);
        }

        return SkipDefault;
    }

}