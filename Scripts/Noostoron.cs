
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

[ObjectScript(621)]
public class Noostoron : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(triggerer);
        triggerer.TurnTowards(attachee);
        triggerer.BeginDialog(attachee, 1);
        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        SetGlobalFlag(561, true);
        if ((GetGlobalFlag(560) && GetGlobalFlag(562)))
        {
            PartyLeader.AddReputation(62);
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        return SkipDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() == null))
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((is_30_and_under(attachee, obj)))
                    {
                        attachee.TurnTowards(obj);
                        obj.BeginDialog(attachee, 1);
                        DetachScript();
                    }

                }

            }

        }

        return RunDefault;
    }
    public static bool is_30_and_under(GameObject speaker, GameObject listener)
    {
        if ((speaker.HasLineOfSight(listener)))
        {
            if ((speaker.DistanceTo(listener) <= 30))
            {
                return true;
            }

        }

        return false;
    }
    public static bool increment_rep(GameObject attachee, GameObject triggerer)
    {
        if ((PartyLeader.HasReputation(81)))
        {
            PartyLeader.AddReputation(82);
            PartyLeader.RemoveReputation(81);
        }
        else if ((PartyLeader.HasReputation(82)))
        {
            PartyLeader.AddReputation(83);
            PartyLeader.RemoveReputation(82);
        }
        else if ((PartyLeader.HasReputation(83)))
        {
            PartyLeader.AddReputation(84);
            PartyLeader.RemoveReputation(83);
        }
        else if ((PartyLeader.HasReputation(84)))
        {
            PartyLeader.AddReputation(85);
            PartyLeader.RemoveReputation(84);
        }
        else if ((PartyLeader.HasReputation(85)))
        {
            PartyLeader.AddReputation(86);
            PartyLeader.RemoveReputation(85);
        }
        else if ((PartyLeader.HasReputation(86)))
        {
            PartyLeader.AddReputation(87);
            PartyLeader.RemoveReputation(86);
        }
        else if ((PartyLeader.HasReputation(87)))
        {
            PartyLeader.AddReputation(88);
            PartyLeader.RemoveReputation(87);
        }
        else if ((PartyLeader.HasReputation(88)))
        {
            PartyLeader.AddReputation(89);
            PartyLeader.RemoveReputation(88);
        }
        else
        {
            PartyLeader.AddReputation(81);
        }

        return RunDefault;
    }

}