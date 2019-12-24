
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
    [ObjectScript(313)]
    public class ThugDualwield : BaseObjectScript
    {
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // if (not attachee.has_wielded(4082) or not attachee.has_wielded(4112)):
            while ((!attachee.HasEquippedByName(4156) || !attachee.HasEquippedByName(4159)))
            {
                attachee.WieldBestInAllSlots();
            }

            // game.new_sid = 0
            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            while ((attachee.FindItemByName(8903) != null))
            {
                attachee.FindItemByName(8903).Destroy();
            }

            // if (attachee.d20_query(Q_Is_BreakFree_Possible)): # workaround no longer necessary!
            // create_item_in_inventory( 8903, attachee )
            // if (not attachee.has_wielded(4082) or not attachee.has_wielded(4112)):
            if ((!attachee.HasEquippedByName(4500) || !attachee.HasEquippedByName(4112)))
            {
                attachee.WieldBestInAllSlots();
            }

            // game.new_sid = 0
            return RunDefault;
        }

    }
}
