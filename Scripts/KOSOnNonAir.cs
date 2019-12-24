
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
    [ObjectScript(206)]
    public class KOSOnNonAir : BaseObjectScript
    {
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(346)))
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
                    if ((robe.GetNameId() == 3020))
                    {
                        saw_ally_robe = true;
                    }
                    else if ((robe.GetNameId() == 3021))
                    {
                        saw_greater_robe = true;
                        break;

                    }
                    else if (((robe.GetNameId() == 3010) || (robe.GetNameId() == 3016) || (robe.GetNameId() == 3017)))
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
