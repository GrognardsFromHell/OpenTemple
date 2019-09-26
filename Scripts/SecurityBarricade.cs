
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
    [ObjectScript(360)]
    public class SecurityBarricade : BaseObjectScript
    {
        public override bool OnUse(GameObjectBody door, GameObjectBody triggerer)
        {
            if ((door.GetNameId() == 1621))
            {
                if ((!GetGlobalFlag(966)))
                {
                    // if security barricade is active, disable outside door portal
                    return SkipDefault;
                }
                else
                {
                    // do normal transition
                    return RunDefault;
                }

            }
            else if ((door.GetNameId() == 1623))
            {
                if ((!GetGlobalFlag(966)))
                {
                    // if security barricade is active, disable inside door portal
                    return SkipDefault;
                }
                else
                {
                    // do regional patrol dues flag routine and normal transition
                    if ((!GetGlobalFlag(260)))
                    {
                        SetGlobalFlag(260, true);
                    }

                    return RunDefault;
                }

            }
            return RunDefault;
        }
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 10);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(966)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else if ((!GetGlobalFlag(966)))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var leader = PartyLeader;
            Co8.StopCombat(attachee, 0);
            leader.BeginDialog(attachee, 4000);
            return RunDefault;
        }

    }
}
