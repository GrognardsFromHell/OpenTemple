
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

[ObjectScript(152)]
public class Kella : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() != null))
        {
            triggerer.BeginDialog(attachee, 80);
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
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() == null))
        {
            StartTimer(1, () => ask_to_rejoin(attachee, triggerer));
        }

        return RunDefault;
    }
    public static bool ask_to_rejoin(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() == null))
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((Utilities.is_safe_to_talk(attachee, obj)))
                {
                    obj.BeginDialog(attachee, 50);
                    return SkipDefault;
                }

            }

            StartTimer(1000, () => ask_to_rejoin(attachee, triggerer));
        }

        return SkipDefault;
    }
    public static bool run_off(GameObject attachee, GameObject triggerer)
    {
        attachee.RunOff();
        return SkipDefault;
    }
    public static bool switch_to_tarah(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 8805);
        var kella = Utilities.find_npc_near(attachee, 8070);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(kella);
            kella.TurnTowards(npc);
        }

        return SkipDefault;
    }

}