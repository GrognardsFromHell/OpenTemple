
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

[ObjectScript(194)]
public class DeadWoman : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        triggerer.BeginDialog(attachee, 1);
        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
        {
            if ((!Utilities.critter_is_unconscious(obj)))
            {
                if ((!attachee.HasMet(obj)))
                {
                    attachee.TurnTowards(obj);
                    obj.BeginDialog(attachee, 1);
                }

                return RunDefault;
            }

        }

        return RunDefault;
    }
    public static void loot_murderous_thief(GameObject pc)
    {
        foreach (var obj in ObjList.ListVicinity(pc.GetLocation(), ObjectListFilter.OLC_NPC))
        {
            if (obj.GetNameId() == 14323) // The murderous thief
            {
                foreach (var item_number in new[] { 6043, 6045, 6046, 4071, 4096, 7001 })
                {
                    var countt = 0;
                    while (obj.FindItemByName(item_number) != null && countt <= 20)
                    {
                        // while obj.item_find(item_number) != OBJ_HANDLE_NULL and countt <= 20: ## count <= added as failsafe (in case PC is overloaded and something freaky happens...)
                        pc.GetItem(obj.FindItemByName(item_number));
                        countt += 1;
                    }

                }

            }

        }

        Fade(0, 0, 1010, 0);
        Utilities.start_game_with_quest(23);
    }

}