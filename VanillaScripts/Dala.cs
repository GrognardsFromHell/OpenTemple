
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

[ObjectScript(109)]
public class Dala : BaseObjectScript
{

    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(88)))
        {
            triggerer.BeginDialog(attachee, 1);
        }
        else if ((GetQuestState(37) == QuestState.Completed))
        {
            triggerer.BeginDialog(attachee, 20);
        }
        else if ((attachee.HasMet(triggerer)))
        {
            triggerer.BeginDialog(attachee, 60);
        }
        else
        {
            triggerer.BeginDialog(attachee, 110);
        }

        return SkipDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetQuestState(37) == QuestState.Completed))
        {
            DetachScript();

        }
        else if ((!GetGlobalFlag(89)))
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((attachee.HasLineOfSight(obj)))
                    {
                        SetGlobalFlag(89, true);
                        StartTimer(7200000, () => reset_global_flag_89(attachee));
                        attachee.StealFrom(obj);
                        return RunDefault;
                    }

                }

            }

        }

        return RunDefault;
    }
    public override bool OnCaughtThief(GameObject attachee, GameObject triggerer)
    {
        triggerer.BeginDialog(attachee, 120);
        return RunDefault;
    }
    public static bool reset_global_flag_89(GameObject attachee)
    {
        SetGlobalFlag(89, false);
        return RunDefault;
    }
    public static bool make_dick_talk(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 8018);

        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(attachee);
            attachee.TurnTowards(npc);
        }
        else
        {
            triggerer.BeginDialog(attachee, 130);
        }

        return SkipDefault;
    }


}