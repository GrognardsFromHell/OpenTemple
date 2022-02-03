
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

[ObjectScript(71)]
public class Zert : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() != null))
        {
            triggerer.BeginDialog(attachee, 120);
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
            if ((attachee.GetMap() == 5007 || attachee.GetMap() == 5008))
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

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        SetGlobalFlag(700, true);
        attachee.FloatLine(12014, triggerer);
        if ((attachee.GetLeader() != null && attachee.GetMap() != 5091 && attachee.GetMap() != 5002))
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
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(700, false);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if (GetGlobalFlag(700))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return SkipDefault;
        }

        return RunDefault;
    }
    public override bool OnJoin(GameObject attachee, GameObject triggerer)
    {
        var itemA = attachee.FindItemByName(6047);
        if ((itemA != null))
        {
            itemA.SetItemFlag(ItemFlag.NO_TRANSFER);
        }

        var itemB = attachee.FindItemByName(6060);
        if ((itemB != null))
        {
            itemB.SetItemFlag(ItemFlag.NO_TRANSFER);
        }

        var itemD = attachee.FindItemByName(4036);
        if ((itemD != null))
        {
            itemD.SetItemFlag(ItemFlag.NO_TRANSFER);
        }

        return RunDefault;
    }
    public override bool OnNewMap(GameObject attachee, GameObject triggerer)
    {
        if (((attachee.GetMap() == 5066) || (attachee.GetMap() == 5067) || (attachee.GetMap() == 5105) || (attachee.GetMap() == 5080)))
        {
            var leader = attachee.GetLeader();
            if ((leader != null))
            {
                var percent = Utilities.group_pc_percent_hp(attachee, leader);
                if ((percent < 30))
                {
                    if ((Utilities.obj_percent_hp(attachee) > 70))
                    {
                        leader.BeginDialog(attachee, 320);
                    }

                }

            }

        }

        return RunDefault;
    }
    public static bool switch_to_turuko(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 8004);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
            npc.TurnTowards(triggerer);
        }

        return SkipDefault;
    }
    public static void they_attack(GameObject attachee, GameObject triggerer)
    {
        foreach (var npc in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
        {
            if (npc.GetNameId() == 14642 || npc.GetNameId() == 14314 || npc.GetNameId() == 14078 || npc.GetNameId() == 14079 || npc.GetNameId() == 14080)
            {
                npc.Attack(triggerer);
            }

        }

        var kobort = Utilities.find_npc_near(attachee, 8005);
        var turuko = Utilities.find_npc_near(attachee, 8004);
        attachee.Attack(triggerer);
        kobort.Attack(triggerer);
        turuko.Attack(triggerer);
        return;
    }
    public static bool equip_transfer(GameObject attachee, GameObject triggerer)
    {
        var itemA = attachee.FindItemByName(6047);
        if ((itemA != null))
        {
            itemA.Destroy();
            Utilities.create_item_in_inventory(6047, triggerer);
        }

        var itemB = attachee.FindItemByName(6060);
        if ((itemB != null))
        {
            itemB.Destroy();
            Utilities.create_item_in_inventory(6060, triggerer);
        }

        var itemC = attachee.FindItemByName(6045);
        if ((itemC != null))
        {
            itemC.Destroy();
            Utilities.create_item_in_inventory(6045, triggerer);
        }

        var itemD = attachee.FindItemByName(4036);
        if ((itemD != null))
        {
            itemD.Destroy();
            Utilities.create_item_in_inventory(4036, triggerer);
        }

        var itemE = attachee.FindItemByName(4060);
        if ((itemE != null))
        {
            itemE.Destroy();
            Utilities.create_item_in_inventory(4060, triggerer);
        }

        Utilities.create_item_in_inventory(7001, attachee);
        Utilities.create_item_in_inventory(7001, attachee);
        Utilities.create_item_in_inventory(7001, attachee);
        Utilities.create_item_in_inventory(7001, attachee);
        Utilities.create_item_in_inventory(7001, attachee);
        Utilities.create_item_in_inventory(7001, attachee);
        Utilities.create_item_in_inventory(7001, attachee);
        Utilities.create_item_in_inventory(7001, attachee);
        Utilities.create_item_in_inventory(7002, attachee);
        return RunDefault;
    }

}