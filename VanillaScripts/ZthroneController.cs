
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(225)]
    public class ZthroneController : BaseObjectScript
    {

        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
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
}
