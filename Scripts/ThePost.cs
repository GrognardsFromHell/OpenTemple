
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
    [ObjectScript(436)]
    public class ThePost : BaseObjectScript
    {
        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var loct = attachee.GetLocation();
            var npcboris = GameSystems.MapObject.CreateObject(14800, loct);
            if ((attachee.GetMap() == 5001))
            {
                triggerer.BeginDialog(npcboris, 1);
            }

            if ((attachee.GetMap() == 5051))
            {
                triggerer.BeginDialog(npcboris, 200);
            }

            if ((attachee.GetMap() == 5121))
            {
                triggerer.BeginDialog(npcboris, 400);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // if (attachee.map == 5019):
            // game.fade_and_teleport(0,0,0,game.global_vars[830],game.global_vars[831],game.global_vars[832])
            var dummy = 1;
            return SkipDefault;
        }

    }
}
