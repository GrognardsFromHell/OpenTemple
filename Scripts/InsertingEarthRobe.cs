
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
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(257)]
    public class InsertingEarthRobe : BaseObjectScript
    {
        public override bool OnInsertItem(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((triggerer.GetNameId() == 8065))
            {
                if ((triggerer.GetStat(Stat.hp_current) <= -10))
                {
                    SetGlobalFlag(114, true);
                }

            }

            return RunDefault;
        }

    }
}
