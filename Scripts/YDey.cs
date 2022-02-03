
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

[ObjectScript(111)]
public class YDey : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() != null))
        {
            triggerer.BeginDialog(attachee, 210); // ydey in party
        }
        else if ((GetGlobalVar(905) == 32 && attachee.GetMap() != 5053))
        {
            triggerer.BeginDialog(attachee, 420); // have attacked 3 or more farm animals with ydey in party and not in mother screngs herb shop first floor
        }
        else if ((GetQuestState(31) == QuestState.Completed))
        {
            triggerer.BeginDialog(attachee, 250); // have completed a second trip for otis quest
        }
        else
        {
            triggerer.BeginDialog(attachee, 1); // none of the above
        }

        return SkipDefault;
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
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        attachee.FloatLine(12014, triggerer);
        if ((attachee.GetLeader() != null))
        {
            SetGlobalVar(29, GetGlobalVar(29) + 1);
        }

        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        attachee.FloatLine(12057, triggerer);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive()))
        {
            if ((GetGlobalVar(905) >= 3))
            {
                if ((attachee != null))
                {
                    var leader = attachee.GetLeader();
                    if ((leader != null))
                    {
                        leader.RemoveFollower(attachee);
                        attachee.FloatLine(22000, triggerer);
                    }

                }

            }

        }

        return RunDefault;
    }
    public override bool OnDisband(GameObject attachee, GameObject triggerer)
    {
        if (ScriptDaemon.npc_get(attachee, 2))
        {
            foreach (var obj in triggerer.GetPartyMembers())
            {
                if ((obj.GetNameId() == 8022))
                {
                    triggerer.RemoveFollower(obj);
                }

            }

            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 50);
            }

            attachee.RunOff();
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
    public static void test_adding_two_followers(GameObject pc, GameObject npc)
    {
        if (((GetGlobalVar(450) & 0x4000) == 0) && ((GetGlobalVar(450) & (1)) == 0))
        {
            pc.AddFollower(npc);
            if (!pc.HasMaxFollowers())
            {
                pc.RemoveFollower(npc);
                ScriptDaemon.npc_set(npc, 1);
            }
            else
            {
                pc.RemoveFollower(npc);
                ScriptDaemon.npc_unset(npc, 1);
            }

        }
        else
        {
            if (GameSystems.Party.NPCFollowersSize <= 1) // original condition - only have 1 NPC (Otis, presumeably) (or less, just in case...)
            {
                ScriptDaemon.npc_set(npc, 1);
            }
            else
            {
                ScriptDaemon.npc_unset(npc, 1);
            }

        }

        return;
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