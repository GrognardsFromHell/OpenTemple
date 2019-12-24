
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
    [ObjectScript(311)]
    public class Livonya9f : BaseObjectScript
    {
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            // attachee.destroy()
            // attachee.object_flag_set(OF_OFF)
            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // THIS IS USED FOR BREAK FREE
            // found_nearby = 0
            // for obj in game.party[0].group_list():
            // if (obj.distance_to(attachee) <= 9 and obj.stat_level_get(stat_hp_current) >= -9):
            // found_nearby = 1
            // if found_nearby == 0:
            // while(attachee.item_find(8903) != OBJ_HANDLE_NULL):
            // attachee.item_find(8903).destroy()
            // #if (attachee.d20_query(Q_Is_BreakFree_Possible)): # workaround no longer necessary!
            // #	create_item_in_inventory( 8903, attachee )
            // attachee.d20_send_signal(S_BreakFree)
            // Spiritual Weapon Shenanigens	#
            CombatStandardRoutines.Spiritual_Weapon_Begone(attachee);
            return RunDefault;
        }

    }
}
