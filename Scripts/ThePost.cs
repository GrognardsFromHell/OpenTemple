
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
    [ObjectScript(436)]
    public class ThePost : BaseObjectScript
    {
        public override bool OnUse(GameObject attachee, GameObject triggerer)
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
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            // if (attachee.map == 5019):
            // game.fade_and_teleport(0,0,0,game.global_vars[830],game.global_vars[831],game.global_vars[832])
            var dummy = 1;
            return SkipDefault;
        }

    }
}
