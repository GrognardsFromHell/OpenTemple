
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

[ObjectScript(109)]
public class Dala : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if (ScriptDaemon.get_f("dala_ran_off"))
        {
            triggerer.BeginDialog(attachee, 200);
        }
        else if ((GetGlobalFlag(88)))
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
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee != null && !Utilities.critter_is_unconscious(attachee) && !attachee.D20Query(D20DispatcherKey.QUE_Prone)))
        {
            run_off(attachee, triggerer);
            // attachee.float_mesfile_line( 'mes\\test.mes', 4, 0 )
            return SkipDefault;
            var dummy = 1;
        }

        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee != null && !Utilities.critter_is_unconscious(attachee) && !attachee.D20Query(D20DispatcherKey.QUE_Prone)))
        {
            run_off(attachee, triggerer);
            // attachee.float_mesfile_line( 'mes\\test.mes', 3, 0 )
            return SkipDefault;
            var dummy = 1;
        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        attachee.SetScriptId(ObjScriptEvent.EnterCombat, 109); // assign enter_combat script
        if ((GameSystems.Combat.IsCombatActive()))
        {
            if ((attachee != null && !attachee.IsUnconscious() && !attachee.D20Query(D20DispatcherKey.QUE_Prone)))
            {
                run_off(attachee, triggerer);
                return SkipDefault;
            }

        }

        if ((!GameSystems.Combat.IsCombatActive() && ScriptDaemon.tpsts("dala_buggered_off", 1)))
        {
            var downed_bozos = 0;
            foreach (var hostel_patron in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((new[] { 8018, 14145, 14074 }).Contains(hostel_patron.GetNameId()))
                {
                    if (hostel_patron.IsUnconscious() || hostel_patron.GetLeader() != null)
                    {
                        downed_bozos += 1;
                    }

                }

            }

            if (downed_bozos >= 2)
            {
                // attachee.float_mesfile_line( 'mes\\test.mes', 1, 0 )
                attachee.FadeTo(255, 1, 50);
                foreach (var hostel_patron in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    foreach (var pc in SelectedPartyLeader.GetPartyMembers())
                    {
                        hostel_patron.AIRemoveFromShitlist(pc);
                        hostel_patron.SetReaction(pc, 80);
                    }

                }

                attachee.ClearObjectFlag(ObjectFlag.CLICK_THROUGH);
                attachee.ClearObjectFlag(ObjectFlag.OFF);
                foreach (var hostel_patron in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    foreach (var pc in SelectedPartyLeader.GetPartyMembers())
                    {
                        hostel_patron.AIRemoveFromShitlist(pc);
                        hostel_patron.SetReaction(pc, 80);
                    }

                }

                attachee.SetBaseStat(Stat.strength, 10);
                if (!ScriptDaemon.get_f("have_talked_to_dala_post_battle")) // initiate Dala monologue where she faints
                {
                    foreach (var pc in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if (ScriptDaemon.is_safe_to_talk_rfv(attachee, pc, 40))
                        {
                            ScriptDaemon.set_f("have_talked_to_dala_post_battle");
                            pc.BeginDialog(attachee, 200);
                        }

                    }

                }

            }

        }
        else if ((!GameSystems.Combat.IsCombatActive())) // this takes care of the infinite battle loop
        {
            // attachee.float_mesfile_line( 'mes\\test.mes', 2, 0 )
            foreach (var hostel_patron in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if (!((new[] { 8018, 14145, 14074 }).Contains(hostel_patron.GetNameId()))) // added condition because apparently sometimes combat doesn't start before this heartbeat fires and thus it sets them to non-hostile status and no combat actually commences
                {
                    foreach (var pc in SelectedPartyLeader.GetPartyMembers())
                    {
                        hostel_patron.AIRemoveFromShitlist(pc);
                        hostel_patron.SetReaction(pc, 80);
                    }

                }

            }

        }

        if ((GetQuestState(37) == QuestState.Completed))
        {
            // game.new_sid = 0 # commented by S.A. - the heartbeat is now needed
            var dummy = 1;
        }
        else if ((!GetGlobalFlag(89)))
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                var (xx, yy) = attachee.GetLocation();
                if (xx == 478 && yy == 504 && !attachee.HasLineOfSight(SelectedPartyLeader))
                {
                    attachee.TurnTowards(SelectedPartyLeader);
                }

                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((attachee.HasLineOfSight(obj)))
                    {
                        SetGlobalFlag(89, true);
                        StartTimer(7200000, () => reset_global_flag_89(attachee)); // call reset_global_flag_89 in 2 hours
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
    public static bool run_off(GameObject attachee, GameObject triggerer)
    {
        var lfa = new locXY(501, 490);
        attachee.FadeTo(0, 1, 15);
        // attachee.condition_add_with_args( "prone", 0, 0 )
        // attachee.critter_flag_set(OCF_SLEEPING)
        // attachee.critter_flag_set(OCF_BLINDED)
        // prevent her from taking AoO
        attachee.SetBaseStat(Stat.strength, -5);
        attachee.AddCondition("Paralyzed - ability score", 0, 2, 0);
        StartTimer(1600, () => set_to_of_off(attachee), true);
        StartTimer(3500, () => set_to_of_off(attachee), true); // call it a 2nd time because it takes time for striking/casting on her to register
        if (!ScriptDaemon.tpsts("dala_buggered_off", 0))
        {
            attachee.FloatLine(220, triggerer);
        }

        // attachee.runoff(lfa)
        // attachee.scripts[19] = 109
        return RunDefault;
    }
    public static void set_to_of_off(GameObject obj)
    {
        if ((obj != null && !Utilities.critter_is_unconscious(obj) && obj.GetLeader() == null && !obj.D20Query(D20DispatcherKey.QUE_Critter_Is_Held))) // mainly in case the player initiates the fight with her e.g. one-shotting or charming her etc.
        {
            // and not obj.d20_query(Q_Prone) # removed this one - I think it's reasonable that she crawls under something even after being tripped
            obj.SetObjectFlag(ObjectFlag.CLICK_THROUGH);
            // obj.move(obj.location + 50 , 0.0, 0.0)
            ScriptDaemon.record_time_stamp("dala_buggered_off");
            obj.SetObjectFlag(ObjectFlag.OFF);
        }
        else
        {
            obj.ClearObjectFlag(ObjectFlag.CLICK_THROUGH);
            // obj.move(obj.location - 50 , 0.0, 0.0)
            obj.ClearObjectFlag(ObjectFlag.OFF);
            obj.FadeTo(255, 1, 35);
        }

    }

}