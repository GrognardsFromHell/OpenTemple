
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts;

[ObjectScript(198)]
public class Chest : BaseObjectScript
{

    public override bool OnUse(GameObject attachee, GameObject triggerer)
    {
        var obj = Utilities.find_npc_near(attachee, 8053);

        if ((obj != null))
        {
            foreach (var pc in ObjList.ListVicinity(obj.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((Utilities.is_safe_to_talk(obj, pc)))
                {
                    pc.BeginDialog(obj, 1);
                    return SkipDefault;
                }

            }

        }

        return SkipDefault;
    }


}