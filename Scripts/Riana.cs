
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

[ObjectScript(100)]
public class Riana : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(triggerer);
        if ((attachee.GetLeader() != null))
        {
            triggerer.BeginDialog(attachee, 170);
        }
        else if ((GetGlobalFlag(930)))
        {
            triggerer.BeginDialog(attachee, 10011);
        }
        else if ((!attachee.HasMet(triggerer)))
        {
            triggerer.BeginDialog(attachee, 1);
        }
        else
        {
            triggerer.BeginDialog(attachee, 20);
        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() == 5058 && GetGlobalFlag(203) && !GetGlobalFlag(930)))
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
        }
        else if ((attachee.GetMap() != 5089) && (GetGlobalFlag(930)))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
        }
        else if ((attachee.GetMap() == 5089) && (GetGlobalFlag(930)))
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
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
    public override bool OnDisband(GameObject attachee, GameObject triggerer)
    {
        foreach (var obj in triggerer.GetPartyMembers())
        {
            if ((obj.GetNameId() == 8056 && !GetGlobalFlag(806)))
            {
                triggerer.RemoveFollower(obj);
                triggerer.BeginDialog(attachee, 1000);
            }

        }

        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            attachee.AIRemoveFromShitlist(pc);
            attachee.SetReaction(pc, 50);
        }

        return RunDefault;
    }
    public override bool OnNewMap(GameObject attachee, GameObject triggerer)
    {
        var randy1 = RandomRange(1, 12);
        if (((attachee.GetMap() == 5039) && randy1 >= 6))
        {
            attachee.FloatLine(12094, triggerer);
        }
        else if (((attachee.GetMap() == 5064) && randy1 >= 6))
        {
            attachee.FloatLine(12069, triggerer);
        }

        return RunDefault;
    }
    public static bool together_again(GameObject attachee, GameObject triggerer)
    {
        foreach (var obj in triggerer.GetPartyMembers())
        {
            if ((obj.GetNameId() == 8056 && !GetGlobalFlag(806)))
            {
                triggerer.BeginDialog(attachee, 1010);
            }

        }

        return RunDefault;
    }
    public static bool buttin2(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 8056);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(attachee);
            attachee.TurnTowards(npc);
        }

        return SkipDefault;
    }
    public static bool buttin3(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 8015);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(attachee);
            attachee.TurnTowards(npc);
        }

        return SkipDefault;
    }
    public static bool switch_to_tarah(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 8805);
        var riana = Utilities.find_npc_near(attachee, 8058);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(riana);
            riana.TurnTowards(npc);
        }

        return SkipDefault;
    }
    public static bool get_rep(GameObject attachee, GameObject triggerer)
    {
        if (!triggerer.HasReputation(7))
        {
            triggerer.AddReputation(7);
        }

        SetGlobalVar(25, GetGlobalVar(25) + 1);
        if ((GetGlobalVar(25) >= 3 && !triggerer.HasReputation(8)))
        {
            triggerer.AddReputation(8);
        }

        return RunDefault;
    }
    public static bool disappear(GameObject attachee)
    {
        attachee.SetObjectFlag(ObjectFlag.OFF);
        return RunDefault;
    }

}