
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
    [ObjectScript(178)]
    public class ErliterBeggar : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((SelectedPartyLeader.HasReputation(32) || SelectedPartyLeader.HasReputation(30) || SelectedPartyLeader.HasReputation(29)))
            {
                attachee.FloatLine(11004, triggerer);
            }
            else
            {
                if ((attachee.HasMet(triggerer)))
                {
                    triggerer.BeginDialog(attachee, 60);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 1);
                }

            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(199)))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            if ((ScriptDaemon.get_v(436) == 1 || ScriptDaemon.get_v(436) == 2 || ScriptDaemon.get_v(436) == 6 || ScriptDaemon.get_v(436) == 7))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public static bool leave_for_city(GameObject attachee, GameObject triggerer)
        {
            var loc = new locXY(625, 420);
            attachee.RunOff(loc);
            return RunDefault;
        }

    }
}
