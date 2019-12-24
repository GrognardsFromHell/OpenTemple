
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
    [ObjectScript(127)]
    public class Tillahi : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GetGlobalFlag(127)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 140);
            }

            return SkipDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.RunOff();
            return RunDefault;
        }
        public static bool set_reward(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.RunOff();
            StartTimer(1209600000, () => give_reward());
            return RunDefault;
        }
        public static bool give_reward()
        {
            QueueRandomEncounter(3002);
            return RunDefault;
        }
        public static bool get_rep(GameObjectBody attachee, GameObjectBody triggerer)
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


    }
}
