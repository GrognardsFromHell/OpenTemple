
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
    [ObjectScript(157)]
    public class BugbearChieftain : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!attachee.HasMet(triggerer)))
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
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(11, GetGlobalVar(11) + 1);
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(11, GetGlobalVar(11) - 1);
            return RunDefault;
        }


    }
}
