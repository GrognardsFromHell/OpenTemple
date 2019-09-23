
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
    [ObjectScript(441)]
    public class EarthMonsterDeath : BaseObjectScript
    {
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            ScriptDaemon.record_time_stamp(504);
            ScriptDaemon.set_v(498, ScriptDaemon.get_v(498) + 1);
            if (Math.Pow((ScriptDaemon.get_v(498) / 75f), 3) + Math.Pow((ScriptDaemon.get_v(499) / 38f), 3) + Math.Pow((ScriptDaemon.get_v(500) / 13f), 3) >= 1)
            {
                ScriptDaemon.record_time_stamp(509);
            }

            return RunDefault;
        }

    }
}
