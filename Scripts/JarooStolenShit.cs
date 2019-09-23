
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
    [ObjectScript(417)]
    public class JarooStolenShit : BaseObjectScript
    {
        public override bool OnInsertItem(GameObjectBody attachee, GameObjectBody triggerer)
        {
            StartTimer(1000, () => check_theft(attachee, triggerer)); // 1 second
            return RunDefault;
        }
        // gremag = find_npc_near( game.party[0], 8049 )
        // game.party[0].begin_dialog( gremag, 450 )
        // game.global_flags[500] = 1
        // return SKIP_DEFAULT

        public static bool check_theft(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(243)))
            {
                SetGlobalFlag(243, false);
            }
            else if ((!GetGlobalFlag(243)))
            {
                if ((triggerer.FindItemByName(12661) != null))
                {
                    triggerer.FindItemByName(12661).Destroy();
                    Utilities.create_item_in_inventory(12664, triggerer);
                    SetGlobalFlag(243, false);
                }

            }

            return RunDefault;
        }

    }
}
