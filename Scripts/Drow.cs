
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

[ObjectScript(381)]
public class Drow : BaseObjectScript
{
    public override bool OnDying(GameObject attachee, GameObject triggerer)
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