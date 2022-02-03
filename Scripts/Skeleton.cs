
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

[ObjectScript(304)]
public class Skeleton : BaseObjectScript
{
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        return RunDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetNameId() == 14083 && attachee.GetLeader() == null && attachee.GetMap() == 5094))
        {
            var loc = attachee.GetLocation();
            attachee.Destroy();
            var monsterD = GameSystems.MapObject.CreateObject(14615, loc);
            monsterD.SetConcealed(true);
            return RunDefault;
        }

        if ((attachee.GetNameId() == 14123 && attachee.GetLeader() == null && attachee.GetMap() == 5094))
        {
            var loc = attachee.GetLocation();
            attachee.Destroy();
            var monsterD = GameSystems.MapObject.CreateObject(14616, loc);
            monsterD.SetConcealed(true);
            return RunDefault;
        }

        var itemA = attachee.FindItemByName(4096);
        var itemB = attachee.FindItemByName(4097);
        var itemC = attachee.FindItemByName(4117);
        if ((attachee.GetNameId() == 14107 && attachee.GetLeader() == null && attachee.GetMap() == 5094 && (itemA != null || itemB != null || itemC != null)))
        {
            var loc = attachee.GetLocation();
            attachee.Destroy();
            var monsterD = GameSystems.MapObject.CreateObject(14603, loc);
            monsterD.SetConcealed(true);
            return RunDefault;
        }

        if ((attachee.GetNameId() == 14107 && attachee.GetLeader() == null && attachee.GetMap() == 5094))
        {
            var loc = attachee.GetLocation();
            attachee.Destroy();
            var monsterD = GameSystems.MapObject.CreateObject(14600, loc);
            monsterD.SetConcealed(true);
            return RunDefault;
        }

        if ((itemA != null && attachee.GetLeader() == null))
        {
            Utilities.create_item_in_inventory(5005, attachee);
            Utilities.create_item_in_inventory(5005, attachee);
        }

        if ((itemB != null && attachee.GetLeader() == null))
        {
            Utilities.create_item_in_inventory(5005, attachee);
            Utilities.create_item_in_inventory(5005, attachee);
        }

        if ((itemA != null && attachee.GetLeader() != null))
        {
            Utilities.create_item_in_inventory(5005, attachee);
            Utilities.create_item_in_inventory(5005, attachee);
            Utilities.create_item_in_inventory(5005, attachee);
            Utilities.create_item_in_inventory(5005, attachee);
        }

        if ((itemB != null && attachee.GetLeader() != null))
        {
            Utilities.create_item_in_inventory(5005, attachee);
            Utilities.create_item_in_inventory(5005, attachee);
            Utilities.create_item_in_inventory(5005, attachee);
            Utilities.create_item_in_inventory(5005, attachee);
        }

        DetachScript();
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetNameId() == 14083 && attachee.GetLeader() == null && attachee.GetMap() == 5094))
        {
            var loc = attachee.GetLocation();
            attachee.Destroy();
            var monsterD = GameSystems.MapObject.CreateObject(14615, loc);
            monsterD.SetConcealed(true);
            return RunDefault;
        }

        if ((attachee.GetNameId() == 14123 && attachee.GetLeader() == null && attachee.GetMap() == 5094))
        {
            var loc = attachee.GetLocation();
            attachee.Destroy();
            var monsterD = GameSystems.MapObject.CreateObject(14616, loc);
            monsterD.SetConcealed(true);
            return RunDefault;
        }

        var itemA = attachee.FindItemByName(4096);
        var itemB = attachee.FindItemByName(4097);
        var itemC = attachee.FindItemByName(4117);
        if ((attachee.GetNameId() == 14107 && attachee.GetLeader() == null && attachee.GetMap() == 5094 && (itemA != null || itemB != null || itemC != null)))
        {
            var loc = attachee.GetLocation();
            attachee.Destroy();
            var monsterD = GameSystems.MapObject.CreateObject(14603, loc);
            monsterD.SetConcealed(true);
            return RunDefault;
        }

        if ((attachee.GetNameId() == 14107 && attachee.GetLeader() == null && attachee.GetMap() == 5094))
        {
            var loc = attachee.GetLocation();
            attachee.Destroy();
            var monsterD = GameSystems.MapObject.CreateObject(14600, loc);
            monsterD.SetConcealed(true);
            return RunDefault;
        }

        if ((itemA != null && attachee.GetLeader() == null))
        {
            Utilities.create_item_in_inventory(5005, attachee);
            Utilities.create_item_in_inventory(5005, attachee);
        }

        if ((itemB != null && attachee.GetLeader() == null))
        {
            Utilities.create_item_in_inventory(5005, attachee);
            Utilities.create_item_in_inventory(5005, attachee);
        }

        if ((itemA != null && attachee.GetLeader() != null))
        {
            Utilities.create_item_in_inventory(5005, attachee);
            Utilities.create_item_in_inventory(5005, attachee);
            Utilities.create_item_in_inventory(5005, attachee);
            Utilities.create_item_in_inventory(5005, attachee);
        }

        if ((itemB != null && attachee.GetLeader() != null))
        {
            Utilities.create_item_in_inventory(5005, attachee);
            Utilities.create_item_in_inventory(5005, attachee);
            Utilities.create_item_in_inventory(5005, attachee);
            Utilities.create_item_in_inventory(5005, attachee);
        }

        DetachScript();
        return RunDefault;
    }

}