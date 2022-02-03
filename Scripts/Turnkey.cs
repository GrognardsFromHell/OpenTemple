
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

[ObjectScript(153)]
public class Turnkey : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(208)))
        {
            triggerer.BeginDialog(attachee, 110);
        }
        else
        {
            triggerer.BeginDialog(attachee, 1);
        }

        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        // if should_modify_CR( attachee ):
        // modify_CR( attachee, get_av_level() )
        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        // while(attachee.item_find(8903) != OBJ_HANDLE_NULL):
        // attachee.item_find(8903).destroy()
        // if (attachee.d20_query(Q_Is_BreakFree_Possible)): # workaround no longer necessary!
        // create_item_in_inventory( 8903, attachee )
        if (((Utilities.obj_percent_hp(attachee) < 30) && (!GetGlobalFlag(208))))
        {
            GameObject found_pc = null;
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if (pc.type == ObjectType.pc)
                {
                    found_pc = pc;
                }
                else
                {
                    pc.AIRemoveFromShitlist(attachee);
                }

                attachee.AIRemoveFromShitlist(pc);
            }

            if (found_pc != null)
            {
                found_pc.BeginDialog(attachee, 20);
                DetachScript();
                return SkipDefault;
            }

        }

        return RunDefault;
    }

}