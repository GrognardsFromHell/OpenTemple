
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
    [ObjectScript(21)]
    public class TeamsterSon : BaseObjectScript
    {

        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetMap() == 5001))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((GetGlobalFlag(6)))
            {
                triggerer.BeginDialog(attachee, 80);
            }
            else
            {
                return RunDefault;
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetMap() == 5001))
            {
                SetGlobalFlag(5, true);
                Utilities.create_item_in_inventory(12004, attachee);
                if (!triggerer.HasReputation(9))
                {
                    triggerer.AddReputation(9);
                }

            }
            else
            {
                SetGlobalFlag(300, true);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(5, false);
            SetGlobalFlag(300, false);
            return RunDefault;
        }
        public static bool discovered_and_leaves_field(GameObject attachee, GameObject triggerer)
        {
            attachee.SetStandpoint(StandPointType.Night, 164);
            attachee.RunOff();
            StartTimer(43200000, () => turn_back_on(attachee));
            StartTimer(86400000, () => assassinated(attachee));
            return RunDefault;
        }
        public static bool turn_back_on(GameObject attachee)
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }
        public static bool assassinated(GameObject attachee)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            SetGlobalFlag(7, true);
            return RunDefault;
        }


    }
}
