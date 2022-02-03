
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

[ObjectScript(354)]
public class Commonworth : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() != null))
        {
            triggerer.BeginDialog(attachee, 260);
        }
        else if ((GetGlobalFlag(956)) || (GetGlobalFlag(958)))
        {
            triggerer.BeginDialog(attachee, 210);
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
            if ((GetQuestState(80) == QuestState.Accepted && attachee.GetMap() == 5008))
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

        }
        else if ((GetGlobalVar(952) == 1 && attachee.GetMap() == 5172))
        {
            var obj = attachee.GetLeader();
            if ((obj != null))
            {
                obj.RemoveFollower(attachee);
                SetGlobalVar(952, 2);
            }

        }
        else if ((GetGlobalVar(952) == 3 && attachee.GetMap() == 5172))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            SetGlobalVar(952, 4);
        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        SetGlobalFlag(957, true);
        if ((attachee.GetMap() == 5008))
        {
            SetGlobalVar(961, 3);
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(957, false);
        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() == 5057 || attachee.GetMap() == 5152 || attachee.GetMap() == 5008))
        {
            var guntur = Utilities.find_npc_near(attachee, 8716);
            if ((guntur != null))
            {
                var leader = guntur.GetLeader();
                if ((leader != null))
                {
                    leader.RemoveFollower(guntur);
                }

                guntur.Attack(triggerer);
            }

            var quintus = Utilities.find_npc_near(attachee, 8718);
            if ((quintus != null))
            {
                var leader = quintus.GetLeader();
                if ((leader != null))
                {
                    leader.RemoveFollower(quintus);
                }

                quintus.Attack(triggerer);
            }

        }

        return RunDefault;
    }
    public override bool OnJoin(GameObject attachee, GameObject triggerer)
    {
        var boots = attachee.FindItemByName(6045);
        boots.SetItemFlag(ItemFlag.NO_TRANSFER);
        var gloves = attachee.FindItemByName(6046);
        gloves.SetItemFlag(ItemFlag.NO_TRANSFER);
        var coat = attachee.FindItemByName(6342);
        coat.SetItemFlag(ItemFlag.NO_TRANSFER);
        var armor = attachee.FindItemByName(6396);
        armor.SetItemFlag(ItemFlag.NO_TRANSFER);
        var hammer = attachee.FindItemByName(4204);
        hammer.SetItemFlag(ItemFlag.NO_TRANSFER);
        return RunDefault;
    }
    public override bool OnDisband(GameObject attachee, GameObject triggerer)
    {
        var boots = attachee.FindItemByName(6045);
        boots.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var gloves = attachee.FindItemByName(6046);
        gloves.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var coat = attachee.FindItemByName(6342);
        coat.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var armor = attachee.FindItemByName(6396);
        armor.ClearItemFlag(ItemFlag.NO_TRANSFER);
        var hammer = attachee.FindItemByName(4204);
        hammer.ClearItemFlag(ItemFlag.NO_TRANSFER);
        return RunDefault;
    }
    public override bool OnNewMap(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() != null))
        {
            if ((attachee.GetMap() == 5169) || (attachee.GetMap() == 5171))
            {
                if ((!attachee.IsUnconscious()))
                {
                    attachee.FloatLine(2000, triggerer);
                    attachee.RunOff();
                    StartTimer(5000, () => go_away(attachee));
                }
                else if ((attachee.IsUnconscious()))
                {
                    SetGlobalVar(952, 1);
                }

            }
            else if ((attachee.GetMap() == 5001))
            {
                if ((GetGlobalVar(941) == 0))
                {
                    SetGlobalVar(941, 1);
                    StartTimer(432000000, () => stopwatch_for_time_in_party(attachee)); // 5 days
                }

            }
            else if ((GetGlobalVar(941) == 2))
            {
                attachee.FloatLine(3000, triggerer);
                attachee.RunOff();
                StartTimer(5000, () => go_away(attachee));
            }

        }

        return RunDefault;
    }
    public static bool go_away(GameObject attachee)
    {
        attachee.SetObjectFlag(ObjectFlag.OFF);
        return RunDefault;
    }
    public static bool stopwatch_for_time_in_party(GameObject attachee)
    {
        SetGlobalVar(941, 2);
        return RunDefault;
    }

}