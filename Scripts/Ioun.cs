
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

[ObjectScript(283)]
public class Ioun : BaseObjectScript
{
    public override bool OnInsertItem(GameObject attachee, GameObject triggerer)
    {
        if ((triggerer.type == ObjectType.pc || triggerer.type == ObjectType.npc))
        {
            AttachParticles("sp-Magic Stone", triggerer);
        }

        return RunDefault;
    }
    // Ron after moathouse

    public override bool OnNewMap(GameObject attachee, GameObject triggerer)
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