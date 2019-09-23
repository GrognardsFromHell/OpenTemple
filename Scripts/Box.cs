
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
    [ObjectScript(285)]
    public class Box : BaseObjectScript
    {
        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var loc = triggerer.GetLocation();
            var npc = GameSystems.MapObject.CreateObject(14687, loc);
            triggerer.BeginDialog(npc, 1);
            return SkipDefault;
        }
        // zombies

        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // def san_start_combat( attachee, triggerer ): ## zombies
            var randy1 = RandomRange(1, 22);
            var randy2 = RandomRange(1100, 1102);
            if (randy1 >= 21)
            {
                attachee.FloatMesFileLine("mes/narrative.mes", randy2);
            }

            // ALSO, THIS IS USED FOR BREAK FREE
            while ((attachee.FindItemByName(8903) != null))
            {
                attachee.FindItemByName(8903).Destroy();
            }

            // if (attachee.d20_query(Q_Is_BreakFree_Possible)): # workaround no longer necessary!
            // create_item_in_inventory( 8903, attachee )
            // attachee.d20_send_signal(S_BreakFree)
            // Spiritual_Weapon_Begone( attachee )
            return RunDefault;
        }
        public override bool OnInsertItem(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // print "py000285Box Insert Item. Attachee: " + str(attachee) + " Triggerer: " + str(triggerer)
            // Bonus now doesn't stack in Temple+ - so this workaround is no longer necessary -SA
            // glasses = triggerer.item_find(6031)
            // if glasses != OBJ_HANDLE_NULL and (triggerer.type == obj_t_pc or triggerer.type == obj_t_npc):
            // triggerer.float_mesfile_line( 'mes\\narrative.mes', 19 )
            // glasses.destroy()
            return RunDefault;
        }

    }
}
