
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

[ObjectScript(58)]
public class Wainwright : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8001))))
        {
            triggerer.BeginDialog(attachee, 150);
        }
        else if ((GetGlobalFlag(933)))
        {
            triggerer.BeginDialog(attachee, 200);
        }
        else if ((attachee.GetMap() == 5007))
        {
            triggerer.BeginDialog(attachee, 340);
        }
        else if ((GetGlobalFlag(38)))
        {
            triggerer.BeginDialog(attachee, 200);
        }
        else if ((GetGlobalFlag(149)))
        {
            triggerer.BeginDialog(attachee, 220);
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
            if ((GetQuestState(20) == QuestState.Completed) && (attachee.GetMap() == 5007))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
                SetGlobalFlag(933, true);
            }
            else if ((GetQuestState(20) != QuestState.Completed) && (attachee.GetMap() == 5007))
            {
                // contingency for turning valden back on after he hides during a fight in the inn, other stuff doesn't do it like normal
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5044) && (GetGlobalFlag(933)))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

        }

        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() != 5044))
        {
            foreach (var pc in SelectedPartyLeader.GetPartyMembers())
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 80);
            }

        }

        return RunDefault;
    }
    public override bool OnExitCombat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() != 5044))
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
        }

        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() != 5044))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            AttachParticles("mon-Chicken-white-hit", attachee);
            attachee.FloatMesFileLine("mes/float.mes", 5);
            return SkipDefault;
        }

        return RunDefault;
    }
    public static bool make_hate(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetReaction(triggerer) >= 20))
        {
            attachee.SetReaction(triggerer, 20);
        }

        return SkipDefault;
    }

}