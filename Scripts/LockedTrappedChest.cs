
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
    [ObjectScript(312)]
    public class LockedTrappedChest : BaseObjectScript
    {
        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5002)) // This is to replace thieves tools
            {
                Utilities.create_item_in_inventory(12012, triggerer);
                attachee.Destroy();
                return SkipDefault;
            }

            if ((attachee.GetMap() == 5001))
            {
                var obj = Utilities.find_npc_near(triggerer, 20005);
                if ((obj != null && GetGlobalVar(705) != 2))
                {
                    triggerer.BeginDialog(obj, 330);
                    return SkipDefault;
                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5066))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if ((obj.GetNameId() == 14079))
                    {
                        var loc = obj.GetLocation();
                        var rot = obj.Rotation;
                        obj.Destroy();
                        var newNPC = GameSystems.MapObject.CreateObject(14631, loc);
                        newNPC.Rotation = rot;
                    }

                    if ((obj.GetNameId() == 14080))
                    {
                        var loc = obj.GetLocation();
                        var rot = obj.Rotation;
                        obj.Destroy();
                        var newNPC = GameSystems.MapObject.CreateObject(14632, loc);
                        newNPC.Rotation = rot;
                    }

                    if ((obj.GetNameId() == 14186))
                    {
                        var loc = obj.GetLocation();
                        var rot = obj.Rotation;
                        obj.Destroy();
                        var newNPC = GameSystems.MapObject.CreateObject(14636, loc);
                        newNPC.Rotation = rot;
                        newNPC = GameSystems.MapObject.CreateObject(14636, loc);
                        newNPC.Rotation = rot;
                    }

                }

                attachee.Destroy();
            }

            return RunDefault;
        }

    }
}
