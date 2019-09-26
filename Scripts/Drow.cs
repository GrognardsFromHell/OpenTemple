
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
    [ObjectScript(381)]
    public class Drow : BaseObjectScript
    {
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            if ((attachee.GetNameId() == 14708))
            {
                SetGlobalVar(999, GetGlobalVar(999) + 1);
                Utilities.create_item_in_inventory(6120, attachee);
            }
            else if ((attachee.GetNameId() == 14724))
            {
                SetGlobalVar(999, GetGlobalVar(999) + 1);
                Utilities.create_item_in_inventory(6120, attachee);
            }
            else if ((attachee.GetNameId() == 14725))
            {
                SetGlobalVar(999, GetGlobalVar(999) + 1);
                Utilities.create_item_in_inventory(6093, attachee);
            }
            else if ((attachee.GetNameId() == 14726))
            {
                SetGlobalVar(999, GetGlobalVar(999) + 1);
                Utilities.create_item_in_inventory(6334, attachee);
            }
            else if ((attachee.GetNameId() == 14733))
            {
                SetGlobalVar(999, GetGlobalVar(999) + 1);
            }
            else if ((attachee.GetNameId() == 14734))
            {
                SetGlobalVar(999, GetGlobalVar(999) + 1);
                Utilities.create_item_in_inventory(6120, attachee);
            }
            else if ((attachee.GetNameId() == 14735))
            {
                SetGlobalVar(999, GetGlobalVar(999) + 1);
                Utilities.create_item_in_inventory(6223, attachee);
            }
            else if ((attachee.GetNameId() == 14736))
            {
                SetGlobalVar(999, GetGlobalVar(999) + 1);
                Utilities.create_item_in_inventory(6334, attachee);
            }
            else if ((attachee.GetNameId() == 14737))
            {
                SetGlobalVar(999, GetGlobalVar(999) + 1);
            }

            return RunDefault;
        }

    }
}
