
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

[ObjectScript(213)]
public class Mona : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((GetQuestState(60) == QuestState.Completed && GetGlobalFlag(315)))
        {
            triggerer.BeginDialog(attachee, 400);
        }
        else if ((!attachee.HasMet(triggerer)))
        {
            if (GetQuestState(35) == QuestState.Completed)
            {
                triggerer.BeginDialog(attachee, 300);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

        }
        else if ((GetQuestState(60) == QuestState.Botched))
        {
            triggerer.BeginDialog(attachee, 260);
        }
        else if ((GetGlobalFlag(317)))
        {
            triggerer.BeginDialog(attachee, 330);
        }
        else if ((GetQuestState(60) <= QuestState.Mentioned))
        {
            triggerer.BeginDialog(attachee, 470);
        }
        else
        {
            triggerer.BeginDialog(attachee, 200);
        }

        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        SetQuestState(60, QuestState.Botched);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetQuestState(60) == QuestState.Completed && GetGlobalFlag(315)))
        {
            if ((attachee.GetMap() == 5051))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            if ((attachee.GetMap() == 5089))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

        }

        return RunDefault;
    }
    public static bool buttin(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 8015);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(attachee);
            attachee.TurnTowards(npc);
        }
        else
        {
            triggerer.BeginDialog(attachee, 150);
        }

        return SkipDefault;
    }
    public static bool schedule_gremlich(GameObject attachee, GameObject triggerer)
    {
        StartTimer(86400000, () => set_gremlich()); // 86400000ms is 1 day
        ScriptDaemon.record_time_stamp("s_gremlich_1");
        return RunDefault;
    }
    public static bool set_gremlich()
    {
        QueueRandomEncounter(3436);
        ScriptDaemon.set_f("s_gremlich_1_scheduled");
        return RunDefault;
    }
    public static bool encounter_picker(GameObject attachee, GameObject triggerer)
    {
        var enc = RandomRange(1, 6);
        if ((enc == 1))
        {
            StartTimer(259200000, () => set_sport_encounter_1()); // 259200000ms is 3 days
            ScriptDaemon.record_time_stamp("s_sport_1");
        }
        else if ((enc == 2))
        {
            StartTimer(259200000, () => set_sport_encounter_2()); // 259200000ms is 3 days
            ScriptDaemon.record_time_stamp("s_sport_2");
        }
        else if ((enc == 3))
        {
            StartTimer(259200000, () => set_sport_encounter_3()); // 259200000ms is 3 days
            ScriptDaemon.record_time_stamp("s_sport_3");
        }
        else if ((enc == 4))
        {
            StartTimer(259200000, () => set_sport_encounter_4()); // 259200000ms is 3 days
            ScriptDaemon.record_time_stamp("s_sport_4");
        }
        else if ((enc == 5))
        {
            StartTimer(259200000, () => set_sport_encounter_5()); // 259200000ms is 3 days
            ScriptDaemon.record_time_stamp("s_sport_5");
        }
        else if ((enc == 6))
        {
            StartTimer(259200000, () => set_sport_encounter_6()); // 259200000ms is 3 days
            ScriptDaemon.record_time_stamp("s_sport_6");
        }

        return RunDefault;
    }
    public static bool set_sport_encounter_1()
    {
        QueueRandomEncounter(3441);
        ScriptDaemon.set_f("s_sport_1_scheduled");
        SetGlobalVar(564, 1);
        return RunDefault;
    }
    public static bool set_sport_encounter_2()
    {
        QueueRandomEncounter(3442);
        ScriptDaemon.set_f("s_sport_2_scheduled");
        SetGlobalVar(564, 1);
        return RunDefault;
    }
    public static bool set_sport_encounter_3()
    {
        QueueRandomEncounter(3443);
        ScriptDaemon.set_f("s_sport_3_scheduled");
        SetGlobalVar(564, 1);
        return RunDefault;
    }
    public static bool set_sport_encounter_4()
    {
        QueueRandomEncounter(3444);
        ScriptDaemon.set_f("s_sport_4_scheduled");
        SetGlobalVar(564, 1);
        return RunDefault;
    }
    public static bool set_sport_encounter_5()
    {
        QueueRandomEncounter(3445);
        ScriptDaemon.set_f("s_sport_5_scheduled");
        SetGlobalVar(564, 1);
        return RunDefault;
    }
    public static bool set_sport_encounter_6()
    {
        QueueRandomEncounter(3446);
        ScriptDaemon.set_f("s_sport_6_scheduled");
        SetGlobalVar(564, 1);
        return RunDefault;
    }
    public static void gremlich_movie_setup(GameObject attachee, GameObject triggerer)
    {
        set_gremlich_slides();
        return;
    }
    public static bool set_gremlich_slides()
    {
        GameSystems.Movies.MovieQueueAdd(600);
        GameSystems.Movies.MovieQueueAdd(627);
        GameSystems.Movies.MovieQueuePlay();
        return RunDefault;
    }

}