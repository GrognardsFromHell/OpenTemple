
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

[ObjectScript(207)]
public class KOSOnNonEarth : BaseObjectScript
{
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((new[] { 14337, 14338 }).Contains(attachee.GetNameId()))
        {
            if (attachee.GetScriptId(ObjScriptEvent.StartCombat) == 0)
            {
                attachee.SetScriptId(ObjScriptEvent.StartCombat, 2);
            }

        }

        remove_dagger(attachee);
        DetachScript();
        return RunDefault;
    }
    public override bool OnWillKos(GameObject attachee, GameObject triggerer)
    {
        if ((ScriptDaemon.get_v(454) & 1) != 0 && GetGlobalFlag(104))
        {
            // Earth Temple was on alert, and you killed Romag; don't expect to be safe in the Earth Temple anymore!
            return RunDefault;
        }

        if ((GetGlobalFlag(347)))
        {
            // KOS override
            return SkipDefault;
        }

        var saw_ally_robe = false;
        var saw_greater_robe = false;
        var saw_enemy_robe = false;
        foreach (var obj in triggerer.GetPartyMembers())
        {
            var robe = obj.ItemWornAt(EquipSlot.Robes);
            if ((robe != null))
            {
                if ((robe.GetNameId() == 3010))
                {
                    // Earth robe
                    saw_ally_robe = true;
                }
                else if ((robe.GetNameId() == 3021))
                {
                    saw_greater_robe = true;
                    break;

                }
                else if (((robe.GetNameId() == 3020) || (robe.GetNameId() == 3016) || (robe.GetNameId() == 3017)))
                {
                    saw_enemy_robe = true;
                }

            }

        }

        if ((saw_greater_robe))
        {
            return SkipDefault;
        }
        else if (((saw_ally_robe) && (!saw_enemy_robe)))
        {
            return SkipDefault;
        }
        else
        {
            return RunDefault;
        }

    }
    public static void remove_dagger(GameObject npc)
    {
        var dagger = npc.FindItemByProto(4060);
        while (dagger != null)
        {
            dagger.Destroy();
            dagger = npc.FindItemByProto(4060);
        }

        return;
    }

}