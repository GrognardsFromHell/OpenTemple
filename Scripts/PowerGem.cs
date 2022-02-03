
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
    [ObjectScript(219)]
    public class PowerGem : BaseObjectScript
    {
        public override bool OnInsertItem(GameObject attachee, GameObject triggerer)
        {
            var orb = triggerer.FindItemByName(2203);
            if ((orb != null))
            {
                triggerer.D20SendSignal(D20DispatcherKey.SIG_Golden_Skull_Combine, attachee);
                attachee.Destroy();
            }

            return SkipDefault;
        }

    }
}
