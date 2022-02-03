
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

[ObjectScript(386)]
public class GnomeGuard : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((Utilities.is_daytime()))
        {
            triggerer.BeginDialog(attachee, 10);
        }
        else
        {
            triggerer.BeginDialog(attachee, 20);
        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if (((PartyLeader.HasReputation(35) || PartyLeader.HasReputation(42)) && (attachee.GetMap() == 5121))) // turns on Verbobonc Exterior backup patrol
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
        }
        else if (((!PartyLeader.HasReputation(35)) && (attachee.GetMap() == 5121))) // turns off Verbobonc Exterior backup patrol
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
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

        SetGlobalVar(334, GetGlobalVar(334) + 1);
        if ((GetGlobalVar(334) >= 2))
        {
            PartyLeader.AddReputation(35);
        }

        if ((GetQuestState(67) == QuestState.Accepted))
        {
            SetGlobalFlag(964, true);
        }

        StartTimer(60000, () => go_away(attachee));
        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        var leader = SelectedPartyLeader;
        if (((GetQuestState(67) == QuestState.Accepted) && (!GetGlobalFlag(963))))
        {
            SetCounter(0, GetCounter(0) + 1);
            if ((GetCounter(0) >= 2))
            {
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    if (pc.type == ObjectType.pc)
                    {
                        attachee.AIRemoveFromShitlist(pc);
                    }

                }

                SetGlobalFlag(963, true);
                leader.BeginDialog(attachee, 1);
                return SkipDefault;
            }

        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((PartyLeader.HasReputation(34) || PartyLeader.HasReputation(35) || PartyLeader.HasReputation(42) || PartyLeader.HasReputation(44) || PartyLeader.HasReputation(35) || PartyLeader.HasReputation(43) || PartyLeader.HasReputation(46) || (GetGlobalVar(993) == 5 && !GetGlobalFlag(870))))
        {
            if (((GetGlobalVar(969) == 0) && (!GetGlobalFlag(955))))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((is_better_to_talk(attachee, obj)))
                        {
                            attachee.TurnTowards(obj);
                            obj.BeginDialog(attachee, 30);
                            SetGlobalVar(969, 1);
                        }

                    }

                }

            }

        }

        return RunDefault;
    }
    public override bool OnWillKos(GameObject attachee, GameObject triggerer)
    {
        if ((PartyLeader.HasReputation(34)) || (PartyLeader.HasReputation(35)))
        {
            return RunDefault;
        }
        else if ((!GetGlobalFlag(992)) || (!GetGlobalFlag(975)))
        {
            return SkipDefault;
        }

        return RunDefault;
    }
    public static bool guard_backup(GameObject attachee, GameObject triggerer)
    {
        var guard_1 = GameSystems.MapObject.CreateObject(14700, attachee.GetLocation().OffsetTiles(-4, 0));
        guard_1.TurnTowards(PartyLeader);
        var guard_2 = GameSystems.MapObject.CreateObject(14700, attachee.GetLocation().OffsetTiles(-4, 0));
        guard_2.TurnTowards(PartyLeader);
        var guard_3 = GameSystems.MapObject.CreateObject(14700, attachee.GetLocation().OffsetTiles(-4, 0));
        guard_3.TurnTowards(PartyLeader);
        return RunDefault;
    }
    public static bool is_better_to_talk(GameObject speaker, GameObject listener)
    {
        if ((speaker.HasLineOfSight(listener)))
        {
            if ((speaker.DistanceTo(listener) <= 35))
            {
                return true;
            }

        }

        return false;
    }
    public static int is_close(GameObject speaker, GameObject listener)
    {
        if ((speaker.HasLineOfSight(listener)))
        {
            if ((speaker.DistanceTo(listener) <= 15))
            {
                return 1;
            }

        }

        return 0;
    }
    public static bool go_away(GameObject attachee)
    {
        attachee.SetObjectFlag(ObjectFlag.OFF);
        return RunDefault;
    }

}