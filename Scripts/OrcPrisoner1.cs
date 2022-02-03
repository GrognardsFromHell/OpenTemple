
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

[ObjectScript(131)]
public class OrcPrisoner1 : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() != null))
        {
            if ((!GetGlobalFlag(134)))
            {
                triggerer.BeginDialog(attachee, 100);
            }
            else
            {
                triggerer.BeginDialog(attachee, 170);
            }

        }
        else if (((!attachee.HasMet(triggerer)) || (!GetGlobalFlag(131))))
        {
            if ((GetGlobalFlag(131)))
            {
                triggerer.BeginDialog(attachee, 90);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

        }
        else if ((!GetGlobalFlag(134)))
        {
            triggerer.BeginDialog(attachee, 150);
        }
        else
        {
            triggerer.BeginDialog(attachee, 190);
        }

        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        attachee.FloatLine(12014, triggerer); // added by ShiningTed
        SetGlobalFlag(135, true);
        if ((attachee.GetLeader() != null))
        {
            SetGlobalVar(29, GetGlobalVar(29) + 1);
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(135, false);
        return RunDefault;
    }
    // added by ShiningTed

    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        // def san_heartbeat( attachee, triggerer ): # added by ShiningTed
        if ((!GameSystems.Combat.IsCombatActive()))
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((obj.GetNameId() == 8730) && (attachee.GetMap() == 5066))
                {
                    if ((attachee.HasLineOfSight(obj)))
                    {
                        PartyLeader.BeginDialog(obj, 700);
                        DetachScript();
                    }

                }

            }

        }

        return RunDefault;
    }
    public override bool OnJoin(GameObject attachee, GameObject triggerer)
    {
        var obj = Utilities.find_npc_near(attachee, 8025);
        if ((obj != null))
        {
            triggerer.AddFollower(obj);
        }

        return RunDefault;
    }
    public override bool OnDisband(GameObject attachee, GameObject triggerer)
    {
        foreach (var obj in triggerer.GetPartyMembers())
        {
            if ((obj.GetNameId() == 8025))
            {
                triggerer.RemoveFollower(obj);
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    obj.AIRemoveFromShitlist(pc);
                    obj.SetReaction(pc, 50);
                }

            }

        }

        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            attachee.AIRemoveFromShitlist(pc);
            attachee.SetReaction(pc, 50);
        }

        return RunDefault;
    }
    // added by ShiningTed

    public static bool argue_ron(GameObject attachee, GameObject triggerer, int line)
    {
        // def argue_ron( attachee, triggerer, line): # added by ShiningTed
        var npc = Utilities.find_npc_near(attachee, 8730);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(triggerer);
            triggerer.TurnTowards(npc);
        }
        else
        {
            triggerer.BeginDialog(attachee, 300);
        }

        return SkipDefault;
    }

}