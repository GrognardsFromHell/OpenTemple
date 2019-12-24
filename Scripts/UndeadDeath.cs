
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
