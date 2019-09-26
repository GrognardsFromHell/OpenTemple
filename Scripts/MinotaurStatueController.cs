
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
    [ObjectScript(226)]
    public class MinotaurStatueController : BaseObjectScript
    {
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetNameId() == 14416))
            {
                // Minotaur controller.
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((obj != attachee))
                    {
                        if ((attachee.DistanceTo(obj) <= 15))
                        {
                            foreach (var statue in ObjList.ListVicinity(obj.GetLocation(), ObjectListFilter.OLC_SCENERY))
                            {
                                if ((statue.GetNameId() == 1615))
                                {
                                    var loc = statue.GetLocation();
                                    var rot = statue.Rotation;
                                    statue.Destroy();
                                    var minotaur = GameSystems.MapObject.CreateObject(14241, loc);
                                    minotaur.Rotation = rot;
                                    AttachParticles("ef-MinoCloud", minotaur);
                                    attachee.Destroy();
                                    return SkipDefault;
                                }

                            }

                        }

                    }

                }

                return SkipDefault;
            }
            else if ((attachee.GetNameId() == 14241))
            {
                // Minotaur.
                Logger.Info("Minotaur heartbeat, casting stoneskin.");
                attachee.CastSpell(WellKnownSpells.Stoneskin, attachee);
                DetachScript();
                return SkipDefault;
            }

            return RunDefault;

        }

    }
}
