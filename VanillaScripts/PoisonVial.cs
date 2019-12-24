
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
    [ObjectScript(266)]
    public class PoisonVial : BaseObjectScript
    {

        public override bool OnInsertItem(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((triggerer.GetNameId() == 8047))
            {
                SetGlobalFlag(113, true);
            }
            else
            {
                SetGlobalFlag(113, false);
            }

            return RunDefault;
        }


    }
}
