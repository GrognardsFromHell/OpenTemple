
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
    [ObjectScript(208)]
    public class KOSOnNonFire : BaseObjectScript
    {
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(344)))
            {
                return SkipDefault;
            }

            var saw_ally_robe = false;
            var saw_greater_robe = false;
            var saw_enemy_robe = false;
            foreach (var obj in triggerer.GetPartyMembers())
            {
                var robe = obj.ItemWornAt(EquipSlot.Robes);
                if ((robe != null))
                {
                    if ((robe.GetNameId() == 3017))
                    {
                        saw_ally_robe = true;
                    }
                    else if ((robe.GetNameId() == 3021))
                    {
                        saw_greater_robe = true;
                        break;

                    }
                    else if (((robe.GetNameId() == 3020) || (robe.GetNameId() == 3016) || (robe.GetNameId() == 3010)))
                    {
                        saw_enemy_robe = true;
                    }

                }

            }

            if ((saw_greater_robe))
            {
                return SkipDefault;
            }
            else if (((saw_ally_robe) && (!saw_enemy_robe)))
            {
                return SkipDefault;
            }
            else
            {
                return RunDefault;
            }

        }

    }
}
