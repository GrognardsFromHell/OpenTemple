
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
    [ObjectScript(133)]
    public class BrigandPrisoner : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 100);
            }
            else if (((!attachee.HasMet(triggerer)) || (!GetGlobalFlag(130))))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 130);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            // if (game.global_flags[343] == 1):
            // game.global_flags[343] = 0
            // attachee.object_flag_set(OF_OFF)
            // else:
            // attachee.object_flag_unset(OF_OFF)
            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            attachee.FloatLine(12014, triggerer);
            if ((attachee.GetLeader() != null))
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            return RunDefault;
        }
        public static bool run_off(GameObject attachee, GameObject triggerer)
        {
            var loc = new locXY(440, 416);
            attachee.SetStandpoint(StandPointType.Night, 258);
            attachee.SetStandpoint(StandPointType.Day, 258);
            attachee.RunOff(loc);
            return RunDefault;
        }
        public static bool get_rep(GameObject attachee, GameObject triggerer)
        {
            if (!triggerer.HasReputation(16))
            {
                triggerer.AddReputation(16);
            }

            SetGlobalVar(26, GetGlobalVar(26) + 1);
            if ((GetGlobalVar(26) >= 3 && !triggerer.HasReputation(17)))
            {
                triggerer.AddReputation(17);
            }

            return RunDefault;
        }
        public static bool move_wicked(GameObject attachee, GameObject triggerer)
        {
            attachee.SetStandpoint(StandPointType.Night, 258);
            attachee.SetStandpoint(StandPointType.Day, 258);
            // game.global_flags[343] = 1
            // attachee.runoff(attachee.location-3)
            return RunDefault;
        }

    }
}
