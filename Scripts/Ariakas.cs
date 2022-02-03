
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

[ObjectScript(488)]
public class Ariakas : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalVar(501) == 8))
        {
            triggerer.BeginDialog(attachee, 1010);
        }
        else if ((GetGlobalVar(501) == 7 && attachee.GetLeader() == null))
        {
            triggerer.BeginDialog(attachee, 920);
        }
        else if ((GetGlobalVar(501) == 7 && attachee.GetLeader() != null))
        {
            triggerer.BeginDialog(attachee, 910);
        }
        else if ((GetGlobalVar(501) == 1 && GetQuestState(97) == QuestState.Unknown))
        {
            triggerer.BeginDialog(attachee, 1);
        }
        else if ((GetGlobalVar(501) >= 4 && GetGlobalVar(501) <= 6 && attachee.GetLeader() == null))
        {
            triggerer.BeginDialog(attachee, 370);
        }
        else if ((GetGlobalVar(501) >= 4 && GetGlobalVar(501) <= 6 && attachee.GetLeader() != null))
        {
            triggerer.BeginDialog(attachee, 620);
        }
        else if ((GetGlobalVar(501) == 2 && GetQuestState(97) == QuestState.Mentioned))
        {
            triggerer.BeginDialog(attachee, 890);
        }
        else if (((GetGlobalVar(501) == 2 || GetGlobalVar(501) == 3) && GetQuestState(97) == QuestState.Accepted))
        {
            triggerer.BeginDialog(attachee, 140);
        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalVar(501) == 1))
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
        }
        else if ((GetGlobalFlag(504) || GetQuestState(97) == QuestState.Botched))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() == null))
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

        }

        SetGlobalFlag(502, true);
        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(502, false);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(525) && !GetGlobalFlag(526)))
        {
            if ((GetGlobalFlag(527)))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((is_better_to_talk(attachee, obj)))
                    {
                        StartTimer(1500, () => wakefield_talk(attachee, triggerer));
                        SetGlobalFlag(526, true);
                    }

                }

            }
            else
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((is_better_to_talk(attachee, obj)))
                    {
                        StartTimer(1500, () => suspicious_talk(attachee, triggerer));
                        SetGlobalFlag(526, true);
                    }

                }

            }

        }
        else if ((GetGlobalVar(501) == 3 && Utilities.find_npc_near(attachee, 14496) != null && !GetGlobalFlag(503)))
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((is_better_to_talk(attachee, obj)))
                {
                    StartTimer(1500, () => talkie_talkie(attachee, triggerer));
                    SetGlobalFlag(503, true);
                }

            }

        }
        else if ((!PartyLeader.HasReputation(52) && GetGlobalVar(505) == 2 && attachee.GetLeader() == null))
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((is_better_to_talk(attachee, obj)))
                {
                    StartTimer(1500, () => bad_news(attachee, triggerer));
                    SetGlobalVar(505, 3);
                }

            }

        }

        return RunDefault;
    }
    public override bool OnJoin(GameObject attachee, GameObject triggerer)
    {
        var hammer = attachee.FindItemByName(4079);
        hammer.SetItemFlag(ItemFlag.NO_TRANSFER);
        var armor = attachee.FindItemByName(6397);
        armor.SetItemFlag(ItemFlag.NO_TRANSFER);
        var boots = attachee.FindItemByName(6020);
        boots.SetItemFlag(ItemFlag.NO_TRANSFER);
        var gloves = attachee.FindItemByName(6021);
        gloves.SetItemFlag(ItemFlag.NO_TRANSFER);
        var helm = attachee.FindItemByName(6335);
        helm.SetItemFlag(ItemFlag.NO_TRANSFER);
        var shield = attachee.FindItemByName(6051);
        shield.SetItemFlag(ItemFlag.NO_TRANSFER);
        var cloak = attachee.FindItemByName(6233);
        cloak.SetItemFlag(ItemFlag.NO_TRANSFER);
        return RunDefault;
    }
    public override bool OnDisband(GameObject attachee, GameObject triggerer)
    {
        var hammer = attachee.FindItemByName(4079);
        hammer.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var armor = attachee.FindItemByName(6397);
        armor.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var boots = attachee.FindItemByName(6020);
        boots.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var gloves = attachee.FindItemByName(6021);
        gloves.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var helm = attachee.FindItemByName(6335);
        helm.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var shield = attachee.FindItemByName(6051);
        shield.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var cloak = attachee.FindItemByName(6233);
        cloak.ClearItemFlag(ItemFlag.NO_TRANSFER);
        return RunDefault;
    }
    public static bool check_back(GameObject attachee, GameObject triggerer)
    {
        StartTimer(172800000, () => trouble()); // 2 days
        return RunDefault;
    }
    public static bool trouble()
    {
        SetGlobalVar(501, 3);
        return RunDefault;
    }
    public static bool is_better_to_talk(GameObject speaker, GameObject listener)
    {
        if ((speaker.HasLineOfSight(listener)))
        {
            if ((speaker.DistanceTo(listener) <= 25))
            {
                return true;
            }

        }

        return false;
    }
    public static bool switch_to_old_man(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 14496);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
        }

        return SkipDefault;
    }
    public static bool talkie_talkie(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(PartyLeader);
        PartyLeader.BeginDialog(attachee, 150);
        return RunDefault;
    }
    public static bool start_alarm(GameObject attachee, GameObject triggerer)
    {
        Sound(4131, 2);
        return RunDefault;
    }
    public static void set_inside_limiter(GameObject attachee, GameObject triggerer)
    {
        StartTimer(7200000, () => out_of_time(attachee, triggerer)); // 2 hours
        SetGlobalVar(505, 1);
        return;
    }
    public static void out_of_time(GameObject attachee, GameObject triggerer)
    {
        SetGlobalVar(505, 2);
        return;
    }
    public static bool bad_news(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(PartyLeader);
        PartyLeader.BeginDialog(attachee, 930);
        return RunDefault;
    }
    public static void very_bad_things(GameObject attachee, GameObject triggerer)
    {
        SetGlobalVar(510, 2);
        SetGlobalFlag(504, true);
        SetQuestState(97, QuestState.Botched);
        PartyLeader.AddReputation(53);
        return;
    }
    public static bool wakefield_talk(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(PartyLeader);
        PartyLeader.BeginDialog(attachee, 1150);
        return RunDefault;
    }
    public static bool suspicious_talk(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(PartyLeader);
        PartyLeader.BeginDialog(attachee, 1180);
        return RunDefault;
    }
    public static void hextor_movie_setup(GameObject attachee, GameObject triggerer)
    {
        set_hextor_slides();
        return;
    }
    public static bool set_hextor_slides()
    {
        GameSystems.Movies.MovieQueueAdd(611);
        GameSystems.Movies.MovieQueueAdd(612);
        GameSystems.Movies.MovieQueueAdd(613);
        GameSystems.Movies.MovieQueueAdd(614);
        GameSystems.Movies.MovieQueueAdd(615);
        GameSystems.Movies.MovieQueueAdd(616);
        GameSystems.Movies.MovieQueueAdd(617);
        GameSystems.Movies.MovieQueueAdd(618);
        GameSystems.Movies.MovieQueueAdd(619);
        GameSystems.Movies.MovieQueueAdd(620);
        GameSystems.Movies.MovieQueueAdd(621);
        GameSystems.Movies.MovieQueueAdd(622);
        GameSystems.Movies.MovieQueueAdd(623);
        GameSystems.Movies.MovieQueueAdd(624);
        GameSystems.Movies.MovieQueueAdd(625);
        GameSystems.Movies.MovieQueueAdd(626);
        GameSystems.Movies.MovieQueuePlay();
        return RunDefault;
    }

}