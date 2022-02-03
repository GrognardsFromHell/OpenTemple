
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

[ObjectScript(390)]
public class CanonThaddeus : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        triggerer.BeginDialog(attachee, 1);
        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() == 5093 && GetGlobalVar(960) == 3))
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
            attachee.CastSpell(WellKnownSpells.DeathWard, attachee);
        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            pc.AddCondition("fallen_paladin");
        }

        SetGlobalFlag(844, true);
        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalVar(956) == 1))
        {
            attachee.SetInt(obj_f.critter_strategy, 409);
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(844, false);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive()))
        {
            if (((attachee.GetMap() == 5093) && (GetGlobalVar(960) == 4)))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((is_better_to_talk(attachee, obj)))
                    {
                        attachee.TurnTowards(obj);
                        obj.BeginDialog(attachee, 1);
                        SetGlobalVar(960, 5);
                    }

                }

            }

        }
        else if ((GameSystems.Combat.IsCombatActive()))
        {
            if (((attachee.GetMap() == 5093) && (GetGlobalVar(957) >= 8)))
            {
                attachee.FloatLine(1000, triggerer);
                SetGlobalVar(956, 1);
                SetGlobalVar(957, 0);
            }

        }

        return RunDefault;
    }
    public static bool is_better_to_talk(GameObject speaker, GameObject listener)
    {
        if ((speaker.DistanceTo(listener) <= 60))
        {
            return true;
        }

        return false;
    }

}