
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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

namespace VanillaScripts
{
    [ObjectScript(180)]
    public class BrauApprenticeBeggar : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                if ((GetGlobalFlag(207)))
                {
                    triggerer.BeginDialog(attachee, 80);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 100);
                }

            }
            else if ((GetGlobalFlag(207)))
            {
                triggerer.BeginDialog(attachee, 50);
            }
            else if ((attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 30);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(205)))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public static bool get_drunk(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            StartTimer(3600000, () => comeback_drunk(attachee));
            return RunDefault;
        }
        public static bool comeback_drunk(GameObjectBody attachee)
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
            SetGlobalFlag(207, true);
            var time = (3600000 * GetGlobalVar(21));

            StartTimer(time, () => get_sober(attachee));
            return RunDefault;
        }
        public static bool get_sober(GameObjectBody attachee)
        {
            SetGlobalFlag(207, false);
            return RunDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var loc = new locXY(427, 406);

            attachee.RunOff(loc);
            return RunDefault;
        }


    }
}
