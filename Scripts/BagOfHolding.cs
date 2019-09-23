
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
