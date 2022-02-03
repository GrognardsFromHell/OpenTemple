
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
    [ObjectScript(6)]
    public class Captain : BaseObjectScript
    {

        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8014)) && !GetGlobalFlag(309)))
            {
                SetGlobalFlag(309, true);
                triggerer.BeginDialog(attachee, 160);
            }
            else if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8000)) && !GetGlobalFlag(308)))
            {
                SetGlobalFlag(308, true);
                triggerer.BeginDialog(attachee, 170);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }


    }
}
