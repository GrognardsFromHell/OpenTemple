
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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
