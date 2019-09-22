
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(227)]
    public class EarthAltar : BaseObjectScript
    {

        public override bool OnInsertItem(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((triggerer.GetNameId() == 1203) && (!GetGlobalFlag(109))))
            {
                UiSystems.CharSheet.Hide();
                attachee.Destroy();
                SetGlobalFlag(109, true);
                AttachParticles("DesecrateEarth", triggerer);
            }

            return RunDefault;
        }


    }
}
