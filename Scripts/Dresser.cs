
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
    [ObjectScript(301)]
    public class Dresser : BaseObjectScript
    {
        public override bool OnUse(GameObject attachee, GameObject triggerer)
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
