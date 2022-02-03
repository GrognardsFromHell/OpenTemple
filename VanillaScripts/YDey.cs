
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts;

[ObjectScript(111)]
public class YDey : BaseObjectScript
{

    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() != null))
        {
            triggerer.BeginDialog(attachee, 210);
        }
        else if ((GetQuestState(31) == QuestState.Completed))
        {
            triggerer.BeginDialog(attachee, 250);
        }
        else
        {
            triggerer.BeginDialog(attachee, 1);
        }

        return SkipDefault;
    }
    public override bool OnDisband(GameObject attachee, GameObject triggerer)
    {
        foreach (var obj in triggerer.GetPartyMembers())
        {
            if ((obj.GetNameId() == 8022))
            {
                triggerer.RemoveFollower(obj);
            }

        }

        attachee.RunOff();
        return RunDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(368)) || (GetGlobalFlag(313)))
        {
            if ((attachee.GetReaction(PartyLeader) >= 0))
            {
                attachee.SetReaction(PartyLeader, -20);
            }

        }

        return RunDefault;
    }
    public override bool OnNewMap(GameObject attachee, GameObject triggerer)
    {
        var leader = attachee.GetLeader();

        if ((leader != null))
        {
            if (((attachee.GetMap() == 5062) || (attachee.GetMap() == 5066) || (attachee.GetMap() == 5067)))
            {
                SetGlobalFlag(204, true);
            }

            if (((attachee.GetMap() == 5051) && (GetGlobalFlag(204))))
            {
                SetGlobalFlag(204, false);
                StartTimer(10000, () => leave_group(attachee, leader));
            }

        }

        return RunDefault;
    }
    public static bool leave_group(GameObject attachee, GameObject triggerer)
    {
        var leader = attachee.GetLeader();

        if ((attachee.GetMap() == 5051) && (leader != null))
        {
            triggerer.BeginDialog(attachee, 400);
        }

        return RunDefault;
    }
    public static bool buttin(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 8014);

        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(attachee);
            attachee.TurnTowards(npc);
        }
        else
        {
            triggerer.BeginDialog(attachee, 160);
        }

        return SkipDefault;
    }


}