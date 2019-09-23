
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
    [ObjectScript(384)]
    public class DrowNote : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var loc = triggerer.GetLocation();
            var n = 0;
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                var item = pc.FindItemByProto(9999);
                var FLAGS = item.GetItemFlags();
                if (((FLAGS & ItemFlag.IDENTIFIED)) != 0)
                {
                    n = 1;
                }

            }

            if ((attachee.GetNameId() == 9999))
            {
                if ((n == 1))
                {
                    var npc = GameSystems.MapObject.CreateObject(14643, loc);
                    triggerer.BeginDialog(npc, 1);
                }
                else
                {
                    var npc = GameSystems.MapObject.CreateObject(14643, loc);
                    triggerer.BeginDialog(npc, 100);
                }

            }

            return SkipDefault;
        }

    }
}
