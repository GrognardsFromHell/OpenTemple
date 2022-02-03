
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

[ObjectScript(62)]
public class Rannosdavl : BaseObjectScript
{

    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((GetQuestState(16) == QuestState.Completed && GetQuestState(15) == QuestState.Completed))
        {
            triggerer.BeginDialog(attachee, 20);
        }
        else if ((GetGlobalFlag(41) || GetQuestState(16) == QuestState.Completed))
        {
            triggerer.BeginDialog(attachee, 290);
        }
        else if ((GetQuestState(17) == QuestState.Completed))
        {
            triggerer.BeginDialog(attachee, 30);
        }
        else if ((GetGlobalFlag(31)))
        {
            triggerer.BeginDialog(attachee, 50);
        }
        else if ((attachee.HasMet(triggerer)))
        {
            triggerer.BeginDialog(attachee, 70);
        }
        else
        {
            triggerer.BeginDialog(attachee, 1);
        }

        return SkipDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        if ((triggerer.type == ObjectType.pc))
        {
            var raimol = Utilities.find_npc_near(attachee, 8050);

            if ((raimol != null))
            {
                attachee.FloatLine(380, triggerer);
                var leader = raimol.GetLeader();

                if ((leader != null))
                {
                    leader.RemoveFollower(raimol);
                }

                raimol.Attack(triggerer);
            }

        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (!PartyLeader.HasReputation(9))
        {
            PartyLeader.AddReputation(9);
        }

        return RunDefault;
    }


}