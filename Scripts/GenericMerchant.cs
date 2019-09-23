
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
    [ObjectScript(54)]
    public class GenericMerchant : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetNameId() == 14453)) // burne assistant
            {
                if ((GetGlobalVar(510) == 2))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                    if ((!GetGlobalFlag(901)))
                    {
                        StartTimer(86400000, () => respawn_burne_assistant(attachee)); // 86400000ms is 24 hours
                        SetGlobalFlag(901, true);
                    }

                }

            }
            else if ((attachee.GetNameId() == 14454)) // otis assistant
            {
                if ((!GetGlobalFlag(923)))
                {
                    StartTimer(604800000, () => respawn_otis_assistant(attachee)); // 604800000ms is 1 week
                    SetGlobalFlag(923, true);
                }

            }
            else if ((attachee.GetNameId() == 14595)) // screng assistant
            {
                if ((!GetGlobalFlag(911)))
                {
                    StartTimer(604800000, () => respawn_screng_assistant(attachee)); // 604800000ms is 1 week
                    SetGlobalFlag(911, true);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public static void respawn_burne_assistant(GameObjectBody attachee)
        {
            var box = Utilities.find_container_near(attachee, 1001);
            InventoryRespawn.RespawnInventory(box);
            StartTimer(86400000, () => respawn_burne_assistant(attachee)); // 86400000ms is 24 hours
            return;
        }
        public static void respawn_otis_assistant(GameObjectBody attachee)
        {
            var box = Utilities.find_container_near(attachee, 1001);
            InventoryRespawn.RespawnInventory(box);
            StartTimer(604800000, () => respawn_otis_assistant(attachee)); // 604800000ms is 1 week
            return;
        }
        public static void respawn_screng_assistant(GameObjectBody attachee)
        {
            var box = Utilities.find_container_near(attachee, 1001);
            InventoryRespawn.RespawnInventory(box);
            StartTimer(604800000, () => respawn_screng_assistant(attachee)); // 604800000ms is 1 week
            return;
        }

    }
}
