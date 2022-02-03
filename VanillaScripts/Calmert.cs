
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

namespace VanillaScripts
{
    [ObjectScript(5)]
    public class Calmert : BaseObjectScript
    {

        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(28) && !triggerer.HasReputation(2)))
            {
                triggerer.AddReputation(2);
            }

            if ((attachee.HasMet(triggerer)))
            {
                if ((GetGlobalVar(5) <= 7))
                {
                    triggerer.BeginDialog(attachee, 10);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 20);
                }

            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public static bool beggar_cavanaugh(GameObject attachee, GameObject triggerer)
        {
            StartTimer(86400000, () => beggar_now(attachee, triggerer));
            return RunDefault;
        }
        public static bool beggar_now(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(205, true);
            SetGlobalVar(24, GetGlobalVar(24) + 1);
            if ((!triggerer.HasReputation(5)))
            {
                triggerer.AddReputation(5);
            }

            if ((GetGlobalVar(24) >= 3 && !triggerer.HasReputation(6)))
            {
                triggerer.AddReputation(6);
            }

            return RunDefault;
        }


    }
}
