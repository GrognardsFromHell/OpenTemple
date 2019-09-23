
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
    [ObjectScript(51)]
    public class SmigmalAssassin : BaseObjectScript
    {
        // Notes on item wielding:
        // 4500 - Rapier +2, 4701 - Wakizashi +1
        // 4112 is dagger of venom, useless for AI

        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // if (not attachee.has_wielded(4500) or not attachee.has_wielded(4112)):
            if ((!attachee.HasEquippedByName(4500) || !attachee.HasEquippedByName(4701)))
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
            // if (not attachee.has_wielded(4500) or not attachee.has_wielded(4112)):
            if ((!attachee.HasEquippedByName(4500) || !attachee.HasEquippedByName(4701)))
            {
                attachee.WieldBestInAllSlots();
            }

            // game.new_sid = 0
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // if (not attachee.has_wielded(4500) or not attachee.has_wielded(4112)):
            if ((GameSystems.Party.PartyMembers).Contains(attachee)) // fixes huge lag when charmed (due to doing repeated item_wield_best_all() at high frequency)
            {
                return RunDefault;
            }

            if ((!attachee.HasEquippedByName(4500) || !attachee.HasEquippedByName(4701)))
            {
                attachee.WieldBestInAllSlots();
                attachee.WieldBestInAllSlots();
            }

            return RunDefault;
        }

    }
}
