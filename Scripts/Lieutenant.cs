
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

[ObjectScript(76)]
public class Lieutenant : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        var ggv400 = GetGlobalVar(400);
        if (attachee.GetLeader() != null)
        {
            return RunDefault;
        }
        else if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8002)))
        {
            attachee.FloatLine(16002, triggerer);
        }
        else if ((ggv400 & (1)) != 0)
        {
            attachee.FloatLine(15500, triggerer);
        }
        else if ((!attachee.HasMet(triggerer)))
        {
            triggerer.BeginDialog(attachee, 1);
        }
        else if ((GetGlobalFlag(48) && !GetGlobalFlag(49)))
        {
            triggerer.BeginDialog(attachee, 70);
        }
        else
        {
            triggerer.BeginDialog(attachee, 100);
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
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        var ggv400 = GetGlobalVar(400);
        var ggv403 = GetGlobalVar(403);
        if ((!GameSystems.Combat.IsCombatActive() && (ggv403 & (2)) == 0) && (ggv400 & (0x20)) == 0)
        {
            if ((is_better_to_talk(attachee, PartyLeader)))
            {
                if ((!Utilities.critter_is_unconscious(PartyLeader)))
                {
                    if ((!attachee.HasMet(PartyLeader)))
                    {
                        if ((is_better_to_talk(attachee, PartyLeader)))
                        {
                            attachee.TurnTowards(PartyLeader);
                            PartyLeader.BeginDialog(attachee, 1);
                            DetachScript();
                        }

                    }

                }

            }
            else
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        if ((!attachee.HasMet(obj)))
                        {
                            attachee.TurnTowards(obj);
                            obj.BeginDialog(attachee, 1);
                            DetachScript();
                        }

                    }

                }

            }

        }

        return RunDefault;
    }
    public static bool is_better_to_talk(GameObject speaker, GameObject listener)
    {
        if ((speaker.HasLineOfSight(listener)))
        {
            if ((speaker.DistanceTo(listener) <= 20))
            {
                return true;
            }

        }

        return false;
    }
    public static void call_leader(GameObject npc, GameObject pc)
    {
        var leader = PartyLeader;
        leader.Move(pc.GetLocation().OffsetTiles(-2, 0));
        leader.BeginDialog(npc, 1);
        return;
    }

}