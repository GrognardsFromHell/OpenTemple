
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
    [ObjectScript(283)]
    public class Ioun : BaseObjectScript
    {
        public override bool OnInsertItem(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((triggerer.type == ObjectType.pc || triggerer.type == ObjectType.npc))
            {
                AttachParticles("sp-Magic Stone", triggerer);
            }

            return RunDefault;
        }
        // Ron after moathouse

        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // def san_new_map( attachee, triggerer ):		# Ron after moathouse
            var st = attachee.GetInt(obj_f.npc_pad_i_5);
            if ((st == 0 && (attachee.GetStat(Stat.level_cleric) == 17)))
            {
                attachee.SetInt(obj_f.npc_pad_i_5, 1);
                PartyLeader.BeginDialog(attachee, 2000);
            }

            if ((st == 1 && attachee.GetMap() == 5011))
            {
                attachee.SetInt(obj_f.npc_pad_i_5, 2);
                PartyLeader.BeginDialog(attachee, 2070);
                DetachScript();
            }

            return RunDefault;
        }

    }
}
