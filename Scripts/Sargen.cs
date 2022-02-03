
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

[ObjectScript(166)]
public class Sargen : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() != null))
        {
            triggerer.BeginDialog(attachee, 100);
        }
        else if ((!attachee.HasMet(triggerer)))
        {
            triggerer.BeginDialog(attachee, 1);
        }
        else
        {
            triggerer.BeginDialog(attachee, 90);
        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
        {
            SetGlobalVar(731, 0);
        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        if ((attachee.GetLeader() != null))
        {
            SetGlobalVar(29, GetGlobalVar(29) + 1);
        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalVar(731) == 0 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
        {
            attachee.CastSpell(WellKnownSpells.MageArmor, attachee);
            attachee.PendingSpellsToMemorized();
            SetGlobalVar(731, 1);
        }

        if ((!GameSystems.Combat.IsCombatActive()))
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((Utilities.is_safe_to_talk(attachee, obj)))
                {
                    DetachScript();
                    obj.BeginDialog(attachee, 1);
                    return RunDefault;
                }

            }

        }

        return RunDefault;
    }
    public override bool OnNewMap(GameObject attachee, GameObject triggerer)
    {
        if (((attachee.GetArea() == 1) || (attachee.GetArea() == 3) || (attachee.GetArea() == 14)))
        {
            var obj = attachee.GetLeader();
            if ((obj != null))
            {
                obj.BeginDialog(attachee, 140);
            }

        }

        return RunDefault;
    }
    public static bool run_off(GameObject attachee, GameObject triggerer)
    {
        attachee.RunOff();
        return RunDefault;
    }
    public static bool schedule_reward(GameObject attachee, GameObject triggerer)
    {
        StartTimer(1814400000, () => give_reward()); // 1814400000ms is 3 weeks
        ScriptDaemon.record_time_stamp("s_sargen_reward"); // bulletproofed with Global Scheduling System - see py00439script_daemon.py
        return RunDefault;
    }
    public static bool give_reward()
    {
        QueueRandomEncounter(3003);
        ScriptDaemon.set_f("s_sargen_reward_scheduled");
        return RunDefault;
    }

}