
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

[ObjectScript(112)]
public class Murfles : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() != null))
        {
            triggerer.BeginDialog(attachee, 190); // murfles in party
        }
        else if ((GetGlobalVar(906) == 32 && attachee.GetMap() != 5054))
        {
            triggerer.BeginDialog(attachee, 240); // have attacked 3 or more farm animals with murfles in party and not in screng's herb shop 2nd floor
        }
        else if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8021))))
        {
            triggerer.BeginDialog(attachee, 320); // already have ydey in party
        }
        else if ((GetGlobalFlag(100)))
        {
            triggerer.BeginDialog(attachee, 250); // ydey has travelled with you
        }
        else if ((!attachee.HasMet(triggerer)))
        {
            triggerer.BeginDialog(attachee, 1); // have not met
        }
        else
        {
            triggerer.BeginDialog(attachee, 350); // have met
        }

        return SkipDefault;
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
        else
        {
            SetGlobalFlag(368, true);
        }

        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        attachee.FloatLine(12057, triggerer);
        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(368, false);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive()))
        {
            if ((GetGlobalVar(906) >= 3))
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
        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            attachee.AIRemoveFromShitlist(pc);
            attachee.SetReaction(pc, 50);
        }

        attachee.RunOff();
        return RunDefault;
    }
    public static bool buttin(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 8021);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(attachee);
            attachee.TurnTowards(npc);
        }
        else
        {
            triggerer.BeginDialog(attachee, 230);
        }

        return SkipDefault;
    }
    public static bool make_hate(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetReaction(triggerer) >= 20))
        {
            attachee.SetReaction(triggerer, 20);
        }

        return SkipDefault;
    }

}