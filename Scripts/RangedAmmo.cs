
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
    [ObjectScript(282)]
    public class RangedAmmo : BaseObjectScript
    {
        public override bool OnInsertItem(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var done = attachee.GetInt(obj_f.weapon_pad_i_1);
            if ((triggerer.type == ObjectType.pc || triggerer.type == ObjectType.npc) && (triggerer.HasFeat(FeatId.FAR_SHOT)))
            {
                if (done == 1)
                {
                    return RunDefault;
                }
                else
                {
                    var curr = attachee.GetInt(obj_f.weapon_range);
                    curr = (int) (curr * 1.5f);
                    attachee.SetInt(obj_f.weapon_range, curr);
                    attachee.SetInt(obj_f.weapon_pad_i_1, 1);
                    Sound(3013, 1);
                }

            }
            else
            {
                if (done == 1)
                {
                    var curr = attachee.GetInt(obj_f.weapon_range);
                    curr = curr * 2 / 3;
                    attachee.SetInt(obj_f.weapon_range, curr);
                    attachee.SetInt(obj_f.weapon_pad_i_1, 0);
                    Sound(3013, 1);
                }

            }

            return RunDefault;
        }

    }
}
