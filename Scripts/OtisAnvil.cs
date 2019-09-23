
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
    [ObjectScript(263)]
    public class OtisAnvil : BaseObjectScript
    {
        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((GetQuestState(32) == QuestState.Accepted) && (!(triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8014))))))
            {
                SetQuestState(32, QuestState.Completed);
            }
            else if ((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8014))))
            {
                // otis = find_npc_near( triggerer, 8014 )
                // attachee.item_transfer_to( otis, 2202 )
                // attachee.item_transfer_to( otis, 3008 )
                var mag_sword = attachee.FindItemByName(2202);
                mag_sword.Destroy();
                var mag_armor = attachee.FindItemByName(3008);
                mag_armor.Destroy();
                var amber = attachee.FindItemByName(12040);
                amber.Destroy();
                var blue_sapphire = attachee.FindItemByName(12038);
                blue_sapphire.Destroy();
            }

            return RunDefault;
        }

    }
}
