
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

[ObjectScript(547)]
public class Mother : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(triggerer);
        if ((attachee.HasMet(triggerer)))
        {
            triggerer.BeginDialog(attachee, 200);
        }
        else
        {
            triggerer.BeginDialog(attachee, 100);
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
        if ((GetQuestState(95) == QuestState.Mentioned && GetGlobalVar(764) >= 8))
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
            DetachScript();
        }

        return RunDefault;
    }
    public static void behave(GameObject attachee, GameObject triggerer)
    {
        attachee.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
        attachee.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
        {
            if (obj.GetNameId() == 14686)
            {
                obj.ClearNpcFlag(NpcFlag.WAYPOINTS_DAY);
                obj.ClearNpcFlag(NpcFlag.WAYPOINTS_NIGHT);
            }

        }

        return;
    }
    public static int bling(GameObject attachee, GameObject triggerer)
    {
        Sound(4048, 1);
        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            AttachParticles("sp-Neutralize Poison", pc);
        }

        return 1;
    }

}