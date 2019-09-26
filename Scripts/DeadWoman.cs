
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
    [ObjectScript(194)]
    public class DeadWoman : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
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
        public static void loot_murderous_thief(GameObjectBody pc)
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
}
