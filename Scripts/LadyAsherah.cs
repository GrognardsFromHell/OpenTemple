
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

[ObjectScript(363)]
public class LadyAsherah : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(triggerer);
        if ((attachee.GetMap() == 5122))
        {
            if ((GetQuestState(102) == QuestState.Completed))
            {
                if ((!GetGlobalFlag(864) && !GetGlobalFlag(807) && !GetGlobalFlag(699)))
                {
                    triggerer.BeginDialog(attachee, 920);
                }
                else if ((GetGlobalFlag(864) || GetGlobalFlag(807)))
                {
                    if ((PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD))
                    {
                        triggerer.BeginDialog(attachee, 1160);
                    }
                    else if ((PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL))
                    {
                        triggerer.BeginDialog(attachee, 1180);
                    }
                    else if ((PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD))
                    {
                        triggerer.BeginDialog(attachee, 1170);
                    }

                }
                else if ((GetGlobalFlag(699)))
                {
                    if ((PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD))
                    {
                        triggerer.BeginDialog(attachee, 1230);
                    }
                    else if ((PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL))
                    {
                        triggerer.BeginDialog(attachee, 1250);
                    }
                    else if ((PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD))
                    {
                        triggerer.BeginDialog(attachee, 1240);
                    }

                }

            }
            else if ((GetGlobalVar(506) == 1))
            {
                triggerer.BeginDialog(attachee, 270);
            }
            else if ((PartyLeader.HasReputation(56) || PartyLeader.HasReputation(57)))
            {
                if ((!GetGlobalFlag(865)))
                {
                    triggerer.BeginDialog(attachee, 130);
                }
                else if ((GetGlobalFlag(865)))
                {
                    if ((PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD))
                    {
                        triggerer.BeginDialog(attachee, 600);
                    }
                    else if ((PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL))
                    {
                        triggerer.BeginDialog(attachee, 710);
                    }
                    else if ((PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD))
                    {
                        triggerer.BeginDialog(attachee, 820);
                    }

                }

            }
            else
            {
                return RunDefault;
            }

        }
        else if ((attachee.GetMap() == 5145))
        {
            if ((GetGlobalVar(506) == 2))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                return RunDefault;
            }

        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetNameId() == 8801))
        {
            if ((GetQuestState(102) == QuestState.Completed))
            {
                if ((attachee.GetMap() == 5122))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }
                else
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5122 && GetGlobalVar(506) == 1))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else if ((attachee.GetMap() == 5145 && GetGlobalVar(506) == 2))
            {
                if ((GetGlobalVar(778) <= 2))
                {
                    SetGlobalVar(778, GetGlobalVar(778) + 1);
                    if ((GetGlobalVar(778) == 3))
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                        SetGlobalVar(778, 4);
                    }

                }

            }
            else if ((attachee.GetMap() == 5122 && (PartyLeader.HasReputation(56) || PartyLeader.HasReputation(57))))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else if ((attachee.GetMap() == 5151 && (PartyLeader.HasReputation(56) || PartyLeader.HasReputation(57))))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

        }
        else if ((attachee.GetNameId() == 8763))
        {
            if ((attachee.GetMap() == 5145 && GetGlobalVar(506) == 2 && GetGlobalVar(778) == 4))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else if ((GetQuestState(102) == QuestState.Completed))
            {
                if ((GetGlobalVar(508) == 2 || GetGlobalVar(508) == 3))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((GetQuestState(102) == QuestState.Accepted && (GetGlobalVar(734) == 2 || GetGlobalVar(697) == 2 || GetGlobalVar(698) == 2)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

        }
        else if ((attachee.GetNameId() == 8764))
        {
            if ((GetQuestState(102) == QuestState.Completed))
            {
                if ((GetGlobalVar(508) == 2 || GetGlobalVar(508) == 3))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                    attachee.SetInt(obj_f.hp_damage, 200);
                }

            }
            else if ((GetQuestState(102) == QuestState.Accepted && (GetGlobalVar(734) == 2 || GetGlobalVar(697) == 2 || GetGlobalVar(698) == 2)))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                attachee.SetInt(obj_f.hp_damage, 200);
            }

        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        SetGlobalFlag(884, true);
        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            pc.AddCondition("fallen_paladin");
        }

        SetGlobalVar(333, GetGlobalVar(333) + 1);
        if ((GetGlobalVar(333) >= 2))
        {
            PartyLeader.AddReputation(34);
        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() == 5145))
        {
            if ((GetGlobalVar(506) == 2 && GetGlobalVar(778) == 4 && GetGlobalVar(534) != 2))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((is_better_to_talk(attachee, obj)))
                        {
                            StartTimer(1000, () => start_talking_1(attachee, triggerer));
                            SetGlobalVar(534, 2);
                        }

                    }

                }

            }

        }
        else if ((attachee.GetMap() == 5122))
        {
            if ((GetQuestState(102) == QuestState.Completed))
            {
                if ((GetGlobalVar(508) == 1))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        Sound(4141, 1);
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((is_better_to_talk(attachee, obj)))
                            {
                                StartTimer(2000, () => start_talking_7(attachee, triggerer));
                                SetGlobalVar(508, 4);
                            }

                        }

                    }

                }
                else if ((GetGlobalVar(508) == 2))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        attachee.AddCondition("Prone", 4, 0);
                        Sound(4142, 1);
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((is_better_to_talk(attachee, obj)))
                            {
                                StartTimer(4000, () => start_talking_8(attachee, triggerer));
                                SetGlobalVar(508, 5);
                            }

                        }

                    }

                }
                else if ((GetGlobalVar(508) == 3))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        attachee.AddCondition("Prone", 4, 0);
                        attachee.SetInt(obj_f.hp_damage, 9);
                        Sound(4140, 1);
                        wreckage();
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((is_better_to_talk(attachee, obj)))
                            {
                                StartTimer(4000, () => start_talking_9(attachee, triggerer));
                                SetGlobalVar(508, 6);
                            }

                        }

                    }

                }

            }
            else if ((GetQuestState(102) == QuestState.Accepted && (GetGlobalVar(734) == 2 || GetGlobalVar(697) == 2 || GetGlobalVar(698) == 2)))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    attachee.AddCondition("Prone", 4, 0);
                    attachee.SetInt(obj_f.hp_damage, 9);
                    Sound(4140, 1);
                    wreckage();
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((is_better_to_talk(attachee, obj)))
                        {
                            StartTimer(4000, () => start_talking_10(attachee, triggerer));
                            SetQuestState(102, QuestState.Botched);
                        }

                    }

                }

            }
            else if ((GetGlobalVar(506) == 1 && GetGlobalVar(534) != 1))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((is_better_to_talk(attachee, obj)))
                        {
                            StartTimer(1000, () => start_talking_3(attachee, triggerer));
                            SetGlobalVar(534, 1);
                        }

                    }

                }

            }
            else if ((GetGlobalVar(506) == 4 && GetGlobalVar(534) != 4))
            {
                if ((!GetGlobalFlag(865)))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((is_better_to_talk(attachee, obj)))
                            {
                                StartTimer(1000, () => start_talking_2(attachee, triggerer));
                                SetGlobalVar(534, 4);
                            }

                        }

                    }

                }
                else if ((GetGlobalFlag(865)))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((is_better_to_talk(attachee, obj)))
                            {
                                if ((PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD))
                                {
                                    StartTimer(1000, () => start_talking_4(attachee, triggerer));
                                    SetGlobalVar(694, 4);
                                }
                                else if ((PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL))
                                {
                                    StartTimer(1000, () => start_talking_5(attachee, triggerer));
                                    SetGlobalVar(694, 4);
                                }
                                else if ((PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD))
                                {
                                    StartTimer(1000, () => start_talking_6(attachee, triggerer));
                                    SetGlobalVar(694, 4);
                                }

                            }

                        }

                    }

                }

            }

        }
        else if ((attachee.GetMap() == 5151))
        {
            if ((GetGlobalVar(506) == 3 && GetGlobalVar(534) != 3))
            {
                if ((!GetGlobalFlag(865)))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((is_better_to_talk(attachee, obj)))
                            {
                                StartTimer(1000, () => start_talking_2(attachee, triggerer));
                                SetGlobalVar(534, 3);
                            }

                        }

                    }

                }
                else if ((GetGlobalFlag(865)))
                {
                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((is_better_to_talk(attachee, obj)))
                            {
                                if ((PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD))
                                {
                                    StartTimer(1000, () => start_talking_4(attachee, triggerer));
                                    SetGlobalVar(534, 3);
                                }
                                else if ((PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL))
                                {
                                    StartTimer(1000, () => start_talking_5(attachee, triggerer));
                                    SetGlobalVar(534, 3);
                                }
                                else if ((PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD))
                                {
                                    StartTimer(1000, () => start_talking_6(attachee, triggerer));
                                    SetGlobalVar(534, 3);
                                }

                            }

                        }

                    }

                }

            }

        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(884, false);
        return RunDefault;
    }
    public static bool is_better_to_talk(GameObject speaker, GameObject listener)
    {
        if ((speaker.HasLineOfSight(listener)))
        {
            if ((speaker.DistanceTo(listener) <= 40))
            {
                return true;
            }

        }

        return false;
    }
    public static bool start_talking_1(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(PartyLeader);
        PartyLeader.BeginDialog(attachee, 1);
        return RunDefault;
    }
    public static bool start_talking_2(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(PartyLeader);
        PartyLeader.BeginDialog(attachee, 130);
        return RunDefault;
    }
    public static bool start_talking_3(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(PartyLeader);
        PartyLeader.BeginDialog(attachee, 270);
        return RunDefault;
    }
    public static bool start_talking_4(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(PartyLeader);
        PartyLeader.BeginDialog(attachee, 600);
        return RunDefault;
    }
    public static bool start_talking_5(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(PartyLeader);
        PartyLeader.BeginDialog(attachee, 710);
        return RunDefault;
    }
    public static bool start_talking_6(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(PartyLeader);
        PartyLeader.BeginDialog(attachee, 820);
        return RunDefault;
    }
    public static bool start_talking_7(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(PartyLeader);
        PartyLeader.BeginDialog(attachee, 920);
        return RunDefault;
    }
    public static bool start_talking_8(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(PartyLeader);
        PartyLeader.BeginDialog(attachee, 1080);
        return RunDefault;
    }
    public static bool start_talking_9(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(PartyLeader);
        PartyLeader.BeginDialog(attachee, 1120);
        return RunDefault;
    }
    public static bool start_talking_10(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(PartyLeader);
        PartyLeader.BeginDialog(attachee, 1270);
        return RunDefault;
    }
    public static bool schedule_transfer(GameObject attachee, GameObject triggerer)
    {
        StartTimer(2000, () => play_dinner(attachee, triggerer));
        if ((GetGlobalVar(699) == 0))
        {
            StartTimer(9000, () => play_bedsprings(attachee, triggerer));
            StartTimer(14000, () => go_to_asherahs(attachee, triggerer));
        }
        else
        {
            StartTimer(8000, () => go_to_asherahs_2(attachee, triggerer));
        }

        return RunDefault;
    }
    public static bool schedule_transfer_2(GameObject attachee, GameObject triggerer)
    {
        StartTimer(2000, () => play_dinner(attachee, triggerer));
        if ((GetGlobalVar(699) == 0))
        {
            StartTimer(9000, () => play_bedsprings(attachee, triggerer));
            StartTimer(14000, () => go_to_goose(attachee, triggerer));
        }
        else
        {
            StartTimer(8000, () => go_to_goose_2(attachee, triggerer));
        }

        return RunDefault;
    }
    public static bool play_dinner(GameObject attachee, GameObject triggerer)
    {
        Sound(4046, 1);
        if ((GetGlobalVar(699) == 3 || GetGlobalVar(699) == 4))
        {
            PartyLeader.AddReputation(57);
        }

        return RunDefault;
    }
    public static bool play_bedsprings(GameObject attachee, GameObject triggerer)
    {
        Sound(4047, 1);
        PartyLeader.AddReputation(56);
        return RunDefault;
    }
    public static bool go_to_asherahs(GameObject attachee, GameObject triggerer)
    {
        FadeAndTeleport(0, 0, 0, 5122, 474, 482);
        return RunDefault;
    }
    public static bool go_to_asherahs_2(GameObject attachee, GameObject triggerer)
    {
        FadeAndTeleport(0, 0, 0, 5122, 479, 479);
        return RunDefault;
    }
    public static bool go_to_goose(GameObject attachee, GameObject triggerer)
    {
        FadeAndTeleport(0, 0, 0, 5151, 497, 478);
        return RunDefault;
    }
    public static bool go_to_goose_2(GameObject attachee, GameObject triggerer)
    {
        FadeAndTeleport(0, 0, 0, 5151, 490, 478);
        return RunDefault;
    }
    public static bool run_off_castle(GameObject attachee, GameObject triggerer)
    {
        var runOffTo = attachee.RunOff();
        var guard = Utilities.find_npc_near(attachee, 8763);
        guard.RunOff(runOffTo);
        return RunDefault;
    }
    public static bool run_off_home(GameObject attachee, GameObject triggerer)
    {
        attachee.RunOff();
        return RunDefault;
    }
    public static bool ruin_asherah(GameObject attachee, GameObject triggerer)
    {
        run_off_home(attachee, triggerer);
        return RunDefault;
    }
    public static bool kill_asherah(GameObject attachee, GameObject triggerer)
    {
        attachee.KillWithDeathEffect();
        return RunDefault;
    }
    public static void wreckage()
    {
        SpawnParticles("ef-Embers Small", new locXY(473, 484));
        SpawnParticles("effect-chimney smoke", new locXY(473, 484));
        SpawnParticles("ef-ogrefire", new locXY(478, 470));
        SpawnParticles("ef-fire-lazy", new locXY(476, 464));
        SpawnParticles("ef-ogrefire", new locXY(485, 480));
        SpawnParticles("ef-ogrefire", new locXY(481, 463));
        SpawnParticles("effect-chimney smoke", new locXY(475, 476));
        SpawnParticles("effect-chimney smoke", new locXY(474, 467));
        return;
    }

}