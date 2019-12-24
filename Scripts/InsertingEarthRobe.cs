
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
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

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
