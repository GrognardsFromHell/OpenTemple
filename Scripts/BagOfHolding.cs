
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
    [ObjectScript(380)]
    public class BagOfHolding : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var loc = triggerer.GetLocation();
            // if attachee.name == 11300:
            // npc = game.obj_create( 14456, loc )
            // triggerer.begin_dialog(npc,1)
            // if attachee.name == 11301:
            // npc = game.obj_create( 14457, loc )
            // triggerer.begin_dialog(npc,100)
            // if attachee.name == 11302:
            // npc = game.obj_create( 14457, loc )
            // triggerer.begin_dialog(npc,300)
            return SkipDefault;
        }
        public static bool create_store(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var loc = attachee.GetLocation();
            var target = GameSystems.MapObject.CreateObject(14456, loc);
            // triggerer.barter(target)
            triggerer.BeginDialog(target, 700);
            return SkipDefault;
        }

    }
}
