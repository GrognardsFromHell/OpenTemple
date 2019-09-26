
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
    [ObjectScript(301)]
    public class Dresser : BaseObjectScript
    {
        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var obj = Utilities.find_npc_near(attachee, 8002);
            if ((obj != null && !Utilities.critter_is_unconscious(obj)))
            {
                if ((obj.GetLeader() == null))
                {
                    if (!obj.HasLineOfSight(triggerer))
                    {
                        return RunDefault;
                    }
                    else
                    {
                        triggerer.BeginDialog(obj, 1);
                        return SkipDefault;
                    }

                }
                else if ((GetGlobalFlag(198)))
                {
                    triggerer.BeginDialog(obj, 260);
                    return SkipDefault;
                }
                else if ((obj.GetLeader() != null))
                {
                    if ((GetGlobalFlag(53)))
                    {
                        triggerer.BeginDialog(obj, 320);
                        return SkipDefault;
                    }
                    else
                    {
                        triggerer.BeginDialog(obj, 220);
                        return SkipDefault;
                    }

                }
                else if ((GetGlobalFlag(52)))
                {
                    triggerer.BeginDialog(obj, 20);
                    return SkipDefault;
                }
                else if ((GetGlobalFlag(48)))
                {
                    triggerer.BeginDialog(obj, 1);
                    return SkipDefault;
                }
                else
                {
                    triggerer.BeginDialog(obj, 10);
                    return SkipDefault;
                }

            }

            return RunDefault;
        }
    }
}
