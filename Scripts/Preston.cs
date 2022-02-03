
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

[ObjectScript(107)]
public class Preston : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((!attachee.HasMet(triggerer)))
        {
            triggerer.BeginDialog(attachee, 1);
        }
        else if ((GetGlobalFlag(93)))
        {
            triggerer.BeginDialog(attachee, 180);
        }
        else
        {
            triggerer.BeginDialog(attachee, 240);
        }

        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive() && PartyLeader.HasReputation(23) && GetGlobalFlag(94) && GetGlobalVar(706) == 0))
        {
            StartTimer(172800000, () => set_heads_up_var(attachee, triggerer)); // 2 days
            SetGlobalVar(706, 1);
        }
        else if ((!GameSystems.Combat.IsCombatActive() && PartyLeader.HasReputation(23) && GetGlobalFlag(94) && GetGlobalVar(706) == 2))
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((Utilities.is_safe_to_talk(attachee, obj)))
                {
                    obj.TurnTowards(attachee);
                    attachee.TurnTowards(obj);
                    obj.BeginDialog(attachee, 400);
                    SetGlobalVar(706, 3);
                }

            }

        }

        return RunDefault;
    }
    public static bool buttin(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 8020);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(attachee);
            attachee.TurnTowards(npc);
        }
        else
        {
            triggerer.BeginDialog(attachee, 220);
        }

        return SkipDefault;
    }
    public static bool set_heads_up_var(GameObject attachee, GameObject triggerer)
    {
        SetGlobalVar(706, 2);
        return RunDefault;
    }

}