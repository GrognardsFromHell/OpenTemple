
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

namespace Scripts;

[ObjectScript(263)]
public class OtisAnvil : BaseObjectScript
{
    public override bool OnUse(GameObject attachee, GameObject triggerer)
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