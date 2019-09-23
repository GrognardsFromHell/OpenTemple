
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(304)]
    public class Skeleton : BaseObjectScript
    {
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
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
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
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
}
