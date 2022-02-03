
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
    [ObjectScript(282)]
    public class RangedAmmo : BaseObjectScript
    {
        public override bool OnInsertItem(GameObject attachee, GameObject triggerer)
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
