
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

[ObjectScript(69)]
public class Spinster : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() != null))
        {
            if ((attachee.GetArea() <= 5))
            {
                triggerer.BeginDialog(attachee, ((attachee.GetArea() * 50) + 150 + (RandomRange(0, 2) * 2)));
            }
            else
            {
                triggerer.BeginDialog(attachee, 252); // "this place smells" used as a generic
            }

        }
        else if ((attachee.HasMet(triggerer)))
        {
            triggerer.BeginDialog(attachee, 50);
        }
        else
        {
            triggerer.BeginDialog(attachee, 1);
        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() == null))
        {
            if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
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

        SetGlobalFlag(364, true);
        attachee.FloatLine(12014, triggerer);
        if ((!GetGlobalFlag(234)))
        {
            SetGlobalVar(23, GetGlobalVar(23) + 1);
            if ((GetGlobalVar(23) >= 2))
            {
                PartyLeader.AddReputation(92);
            }

        }
        else
        {
            SetGlobalVar(29, GetGlobalVar(29) + 1);
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(364, false);
        return RunDefault;
    }
    public override bool OnJoin(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(234, true);
        var cleaver = attachee.FindItemByName(4213);
        cleaver.SetItemFlag(ItemFlag.NO_TRANSFER);
        return RunDefault;
    }
    public override bool OnDisband(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(234, false);
        var cleaver = attachee.FindItemByName(4213);
        cleaver.ClearItemFlag(ItemFlag.NO_TRANSFER);
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
        if (((attachee.GetMap() == 5014 || attachee.GetMap() == 5061) && randy1 >= 8))
        {
            attachee.FloatLine(12103, triggerer);
        }
        else if (((attachee.GetMap() == 5087 || attachee.GetMap() == 5017) && randy1 >= 6))
        {
            attachee.FloatLine(12200, triggerer);
        }

        if (((attachee.GetMap() == 5067) && randy1 >= 9))
        {
            attachee.FloatLine(12095, triggerer);
        }

        return RunDefault;
    }
    public static bool buttin(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 8000);
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
        var fruella = Utilities.find_npc_near(attachee, 8067);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(fruella);
            fruella.TurnTowards(npc);
        }

        return SkipDefault;
    }

}