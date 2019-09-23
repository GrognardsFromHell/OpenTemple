
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
    [ObjectScript(256)]
    public class Usefirechest : BaseObjectScript
    {
        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetNameId() == 1034))
            {
                Utilities.create_item_in_inventory(6311, attachee);
                DetachScript();
                return RunDefault;
            }

            var npc = Utilities.find_npc_near(attachee, 8063);
            DetachScript();
            if ((npc != null))
            {
                npc.TurnTowards(triggerer);
                triggerer.BeginDialog(npc, 60);
                return SkipDefault;
            }

            // for obj in game.obj_list_vicinity(npc.location,OLC_PC):
            // if (is_safe_to_talk(obj,npc)):
            // npc.turn_towards(obj)
            // obj.turn_towards(npc)
            // obj.begin_dialog( npc, 60 )
            // return SKIP_DEFAULT
            return RunDefault;
        }

    }
}
