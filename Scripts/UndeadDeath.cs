
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
    [ObjectScript(292)]
    public class UndeadDeath : BaseObjectScript
    {
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.AddCondition("sp-Animate Dead", 127, 10, 3);
            AttachParticles("sp-summon monster I", PartyLeader);
            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return RunDefault;
        }
        public override bool OnUnlockAttempt(GameObjectBody attachee, GameObjectBody triggerer)
        {
            StartTimer(20000, () => get_rid_of_it(attachee));
            return RunDefault;
        }
        public override bool OnRemoveItem(GameObjectBody attachee, GameObjectBody triggerer)
        {
            StartTimer(20000, () => get_rid_of_it(attachee));
            return RunDefault;
        }
        public static void get_rid_of_it(GameObjectBody attachee)
        {
            attachee.Destroy();
            // game.particles( "sp-summon monster I", game.party[0] )
            return;
        }

    }
}
