
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

namespace Scripts
{
    [ObjectScript(51)]
    public class SmigmalAssassin : BaseObjectScript
    {
        // Notes on item wielding:
        // 4500 - Rapier +2, 4701 - Wakizashi +1
        // 4112 is dagger of venom, useless for AI

        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
        {
            // if (not attachee.has_wielded(4500) or not attachee.has_wielded(4112)):
            if ((!attachee.HasEquippedByName(4500) || !attachee.HasEquippedByName(4701)))
            {
                attachee.WieldBestInAllSlots();
            }

            // game.new_sid = 0
            return RunDefault;
        }
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
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
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
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
