
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
    [ObjectScript(42)]
    public class PotionBugbear : BaseObjectScript
    {
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee != null && !Utilities.critter_is_unconscious(attachee) && !attachee.D20Query(D20DispatcherKey.QUE_Prone)))
            {
                if ((Utilities.obj_percent_hp(attachee) <= 50))
                {
                    if ((attachee.FindItemByName(8014) != null || attachee.FindItemByName(8006) != null || attachee.FindItemByName(8007) != null || attachee.FindItemByName(8101) != null))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 448);
                    }
                    else
                    {
                        attachee.SetInt(obj_f.critter_strategy, 449);
                    }

                }
                else
                {
                    attachee.SetInt(obj_f.critter_strategy, 449);
                }

            }

            return RunDefault;
        }

    }
}
