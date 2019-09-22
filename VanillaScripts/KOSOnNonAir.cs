
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(206)]
    public class KOSOnNonAir : BaseObjectScript
    {

        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(346)))
            {
                return SkipDefault;
            }

            var saw_ally_robe = false;

            var saw_greater_robe = false;

            var saw_enemy_robe = false;

            foreach (var obj in triggerer.GetPartyMembers())
            {
                if ((obj.FindItemByName(3020) != null))
                {
                    saw_ally_robe = true;

                }
                else if ((obj.FindItemByName(3021) != null))
                {
                    saw_greater_robe = true;

                    break;

                }
                else if (((obj.FindItemByName(3010) != null) || (obj.FindItemByName(3016) != null) || (obj.FindItemByName(3017) != null)))
                {
                    saw_enemy_robe = true;

                }

            }

            if ((saw_greater_robe))
            {
                return SkipDefault;
            }
            else if (((saw_ally_robe) && (!saw_enemy_robe)))
            {
                return SkipDefault;
            }
            else
            {
                return RunDefault;
            }

        }


    }
}
