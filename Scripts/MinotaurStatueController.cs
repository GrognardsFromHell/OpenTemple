
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
    [ObjectScript(226)]
    public class MinotaurStatueController : BaseObjectScript
    {
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
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
