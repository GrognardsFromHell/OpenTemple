
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts;

[ObjectScript(177)]
public class Scorpp : BaseObjectScript
{

    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() != null))
        {
            triggerer.BeginDialog(attachee, 200);
        }
        else if ((attachee.HasMet(triggerer)))
        {
            triggerer.BeginDialog(attachee, 100);
        }
        else if ((triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3021))))
        {
            triggerer.BeginDialog(attachee, 10);
        }
        else
        {
            triggerer.BeginDialog(attachee, 1);
        }

        return SkipDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        if ((Utilities.obj_percent_hp(attachee) < 50))
        {
            GameObject found_pc = null;

            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if (pc.type == ObjectType.pc)
                {
                    found_pc = pc;

                    attachee.AIRemoveFromShitlist(pc);
                }

            }

            if (found_pc != null)
            {
                if ((!GetGlobalFlag(194)))
                {
                    SetGlobalFlag(194, true);
                    found_pc.BeginDialog(attachee, 150);
                    DetachScript();

                    return SkipDefault;
                }

            }

        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(182, true);
        if ((attachee.GetLeader() != null))
        {
            SetGlobalVar(29, GetGlobalVar(29) + 1);
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(182, false);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive()))
        {
            var leader = attachee.GetLeader();

            if ((leader != null))
            {
                if ((Utilities.obj_percent_hp(attachee) > 70))
                {
                    if ((Utilities.group_percent_hp(leader) < 30))
                    {
                        attachee.FloatLine(510, leader);
                        attachee.Attack(leader);
                    }

                }

            }

        }

        return RunDefault;
    }


}