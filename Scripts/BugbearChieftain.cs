
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

[ObjectScript(157)]
public class BugbearChieftain : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((ScriptDaemon.get_v(454) & 2) != 0 && !GetGlobalFlag(108)) // Water Temple regrouped, and he's stuck there
        {
            if ((!attachee.HasMet(triggerer)))
            {
                if ((triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3017))))
                {
                    triggerer.BeginDialog(attachee, 140);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 1000);
                }

            }
            else
            {
                if (ScriptDaemon.get_f("realized_grurz_new_situation"))
                {
                    if (ScriptDaemon.get_v("grurz_float_counter") < 5)
                    {
                        var rand1 = RandomRange(0, 1);
                        attachee.FloatLine(1100 + rand1, triggerer);
                        ScriptDaemon.inc_v("grurz_float_counter");
                    }
                    else if (ScriptDaemon.get_v("grurz_float_counter") == 5)
                    {
                        attachee.FloatLine(1102, triggerer);
                        ScriptDaemon.inc_v("grurz_float_counter");
                    }
                    else if (ScriptDaemon.get_v("grurz_float_counter") == 6)
                    {
                        attachee.FloatLine(1103, triggerer);
                        ScriptDaemon.inc_v("grurz_float_counter");
                    }
                    else if (ScriptDaemon.get_v("grurz_float_counter") == 7)
                    {
                        attachee.FloatLine(1104, triggerer);
                        ScriptDaemon.inc_v("grurz_float_counter");
                    }
                    else if (ScriptDaemon.get_v("grurz_float_counter") == 8)
                    {
                        attachee.FloatLine(1105, triggerer);
                        attachee.Attack(triggerer);
                    }

                }
                else
                {
                    triggerer.BeginDialog(attachee, 900);
                }

            }

        }
        else if ((!attachee.HasMet(triggerer)))
        {
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8023))))
            {
                triggerer.BeginDialog(attachee, 60);
            }
            else if ((triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3020))))
            {
                triggerer.BeginDialog(attachee, 120);
            }
            else if ((triggerer.GetPartyMembers().Any(o => o.HasEquippedByName(3017))))
            {
                triggerer.BeginDialog(attachee, 140);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

        }
        else
        {
            triggerer.BeginDialog(attachee, 100);
        }

        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        SetGlobalVar(11, GetGlobalVar(11) + 1);
        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalVar(11, GetGlobalVar(11) - 1);
        return RunDefault;
    }

}