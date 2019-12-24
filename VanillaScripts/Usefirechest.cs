
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(256)]
    public class Usefirechest : BaseObjectScript
    {

        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var npc = Utilities.find_npc_near(attachee, 8063);

            if ((npc != null))
            {
                foreach (var obj in ObjList.ListVicinity(npc.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(npc, obj)))
                    {
                        npc.TurnTowards(obj);
                        obj.TurnTowards(npc);
                        obj.BeginDialog(npc, 1);
                    }

                }

            }

            return SkipDefault;
        }


    }
}
