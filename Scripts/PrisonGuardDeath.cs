
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

[ObjectScript(267)]
public class PrisonGuardDeath : BaseObjectScript
{
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        SetGlobalVar(12, GetGlobalVar(12) + 1);
        if (attachee.GetNameId() == 8065)
        {
            var x = attachee.GetInt(obj_f.critter_flags2);
            x = x | 64;
            attachee.SetInt(obj_f.critter_flags2, x);
        }

        return RunDefault;
    }

}