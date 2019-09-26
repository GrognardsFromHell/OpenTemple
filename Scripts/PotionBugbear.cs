
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
