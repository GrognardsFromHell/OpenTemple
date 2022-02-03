
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
    [ObjectScript(210)]
    public class KOSOnNonTempleRobes : BaseObjectScript
    {
        public override bool OnWillKos(GameObject attachee, GameObject triggerer)
        {
            var saw_robe = false;
            foreach (var obj in triggerer.GetPartyMembers())
            {
                var robe = obj.ItemWornAt(EquipSlot.Robes);
                if ((robe != null))
                {
                    if (((robe.GetNameId() == 3020) || (robe.GetNameId() == 3016) || (robe.GetNameId() == 3017) || (robe.GetNameId() == 3010) || (robe.GetNameId() == 3021)))
                    {
                        saw_robe = true;
                        break;

                    }

                }

            }

            if ((saw_robe))
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
