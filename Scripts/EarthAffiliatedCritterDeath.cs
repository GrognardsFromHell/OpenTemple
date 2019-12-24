
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
    [ObjectScript(442)]
    public class EarthAffiliatedCritterDeath : BaseObjectScript
    {
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            ScriptDaemon.record_time_stamp(505);
            ScriptDaemon.set_v(499, ScriptDaemon.get_v(499) + 1);
            if (Math.Pow((ScriptDaemon.get_v(498) / 75f), 3) + Math.Pow((ScriptDaemon.get_v(499) / 38f), 3) + Math.Pow((ScriptDaemon.get_v(500) / 13f), 3) >= 1)
            {
                ScriptDaemon.record_time_stamp(510);
            }

            return RunDefault;
        }

    }
}
