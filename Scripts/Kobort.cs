
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

[ObjectScript(68)]
public class Kobort : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetLeader() == null))
        {
            if ((GetGlobalFlag(45)))
            {
                triggerer.BeginDialog(attachee, 100);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

        }
        else if ((GetGlobalFlag(45)))
        {
            triggerer.BeginDialog(attachee, 250);
        }
        else
        {
            triggerer.BeginDialog(attachee, 200);
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

        SetGlobalFlag(44, true);
        attachee.FloatLine(12014, triggerer);
        if ((attachee.GetLeader() != null && attachee.GetMap() != 5091 && attachee.GetMap() != 5002))
        {
            SetGlobalVar(29, GetGlobalVar(29) + 1);
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(44, false);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if (GetGlobalFlag(44))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return SkipDefault;
        }

        return RunDefault;
    }
    public override bool OnJoin(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive()))
        {
            var obj = Utilities.find_npc_near(attachee, 8004);
            if ((obj != null && !GetGlobalFlag(806)))
            {
                triggerer.AddFollower(obj);
            }

        }

        var itemA = attachee.FindItemByName(4205);
        if ((itemA != null))
        {
            itemA.SetItemFlag(ItemFlag.NO_TRANSFER);
        }

        var itemD = attachee.FindItemByName(6120);
        if ((itemD != null))
        {
            itemD.SetItemFlag(ItemFlag.NO_TRANSFER);
        }

        var itemE = attachee.FindItemByName(6059);
        if ((itemE != null))
        {
            itemE.SetItemFlag(ItemFlag.NO_TRANSFER);
        }

        return RunDefault;
    }
    public override bool OnDisband(GameObject attachee, GameObject triggerer)
    {
        foreach (var obj in triggerer.GetPartyMembers())
        {
            if ((obj.GetNameId() == 8004 && !GetGlobalFlag(806)))
            {
                triggerer.RemoveFollower(obj);
            }

        }

        foreach (var pc in GameSystems.Party.PartyMembers)
        {
            attachee.AIRemoveFromShitlist(pc);
            attachee.SetReaction(pc, 50);
        }

        return RunDefault;
    }
    public static bool switch_to_turuko(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 8004);
        if ((npc != null))
        {
            triggerer.BeginDialog(npc, line);
        }

        return SkipDefault;
    }
    public static bool equip_transfer(GameObject attachee, GameObject triggerer)
    {
        var itemA = attachee.FindItemByName(4205);
        if ((itemA != null))
        {
            itemA.Destroy();
            Utilities.create_item_in_inventory(4205, triggerer);
        }

        var itemB = attachee.FindItemByName(6026);
        if ((itemB != null))
        {
            itemB.Destroy();
            Utilities.create_item_in_inventory(6026, triggerer);
        }

        var itemC = attachee.FindItemByName(6233);
        if ((itemC != null))
        {
            itemC.Destroy();
            Utilities.create_item_in_inventory(6233, triggerer);
        }

        var itemD = attachee.FindItemByName(6120);
        if ((itemD != null))
        {
            itemD.Destroy();
            Utilities.create_item_in_inventory(6120, triggerer);
        }

        var itemE = attachee.FindItemByName(6059);
        if ((itemE != null))
        {
            itemE.Destroy();
            Utilities.create_item_in_inventory(6059, triggerer);
        }

        var itemF = attachee.FindItemByName(6045);
        if ((itemF != null))
        {
            itemF.Destroy();
            Utilities.create_item_in_inventory(6045, triggerer);
        }

        var itemG = attachee.FindItemByName(4060);
        if ((itemG != null))
        {
            itemG.Destroy();
            Utilities.create_item_in_inventory(4060, triggerer);
        }

        Utilities.create_item_in_inventory(7002, attachee);
        Utilities.create_item_in_inventory(7002, attachee);
        Utilities.create_item_in_inventory(7002, attachee);
        Utilities.create_item_in_inventory(7002, attachee);
        Utilities.create_item_in_inventory(7002, attachee);
        Utilities.create_item_in_inventory(7002, attachee);
        return RunDefault;
    }

}