
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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
    [ObjectScript(331)]
    public class ArmorMerchant : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
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
            if ((!GetGlobalFlag(915)))
            {
                StartTimer(86400000, () => respawn(attachee)); // 86400000ms is 24 hours
                SetGlobalFlag(915, true);
            }

            return RunDefault;
        }
        public static void respawn(GameObjectBody attachee)
        {
            var box = Utilities.find_container_near(attachee, 1001);
            InventoryRespawn.RespawnInventory(box);
            StartTimer(86400000, () => respawn(attachee)); // 86400000ms is 24 hours
            return;
        }

    }
}
