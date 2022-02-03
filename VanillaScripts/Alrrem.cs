
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts;

[ObjectScript(122)]
public class Alrrem : BaseObjectScript
{

    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8031))))
        {
            triggerer.BeginDialog(attachee, 700);
        }
        else if (((GetGlobalFlag(115)) && (GetGlobalFlag(116)) && (!GetGlobalFlag(125))))
        {
            triggerer.BeginDialog(attachee, 400);
        }
        else if ((!attachee.HasMet(triggerer)))
        {
            if ((GetGlobalFlag(92)))
            {
                triggerer.BeginDialog(attachee, 200);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

        }
        else
        {
            triggerer.BeginDialog(attachee, 300);
        }

        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(107, true);
        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(107, false);
        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(344, false);
        return RunDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(312)))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            SetGlobalFlag(107, true);
        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((!GameSystems.Combat.IsCombatActive()))
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((Utilities.is_safe_to_talk(attachee, obj)))
                {
                    if ((!attachee.HasMet(obj)))
                    {
                        if ((GetGlobalFlag(92)))
                        {
                            obj.BeginDialog(attachee, 200);
                        }
                        else
                        {
                            obj.BeginDialog(attachee, 1);
                            DetachScript();

                        }

                    }

                }

            }

        }

        return RunDefault;
    }
    public static bool escort_below(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(144, true);
        FadeAndTeleport(0, 0, 0, 5080, 478, 451);
        return RunDefault;
    }


}