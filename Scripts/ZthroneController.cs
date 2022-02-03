
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

[ObjectScript(225)]
public class ZthroneController : BaseObjectScript
{
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(341)))
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_SCENERY))
            {
                if ((obj.GetNameId() == 1610))
                {
                    obj.ClearObjectFlag(ObjectFlag.DONTDRAW);
                }
                else if ((obj.GetNameId() == 1616))
                {
                    obj.Destroy();
                }
                else if ((obj.GetNameId() == 1617))
                {
                    obj.Destroy();
                }

            }

            var loc = new locXY(489, 396);
            var throne = GameSystems.MapObject.CreateObject(2090, loc);
            if ((throne != null))
            {
                throne.Rotation = 2.3561945f;
            }

            attachee.Destroy();
        }

        return SkipDefault;
    }

}