
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
    [ObjectScript(232)]
    public class JuggernautStatueController : BaseObjectScript
    {
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var statue in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_SCENERY))
            {
                if ((statue.GetNameId() == 1618))
                {
                    var loc = statue.GetLocation();
                    var rot = statue.Rotation;
                    statue.Destroy();
                    var juggernaut = GameSystems.MapObject.CreateObject(14426, loc);
                    juggernaut.Rotation = rot;
                    AttachParticles("ef-MinoCloud", juggernaut);
                    attachee.Destroy();
                    return SkipDefault;
                }

            }

            return SkipDefault;
        }

    }
}
