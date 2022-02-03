
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
    [ObjectScript(54)]
    public class GenericMerchant : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
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
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public static void respawn_burne_assistant(GameObject attachee)
        {
            var box = Utilities.find_container_near(attachee, 1001);
            InventoryRespawn.RespawnInventory(box);
            StartTimer(86400000, () => respawn_burne_assistant(attachee)); // 86400000ms is 24 hours
            return;
        }
        public static void respawn_otis_assistant(GameObject attachee)
        {
            var box = Utilities.find_container_near(attachee, 1001);
            InventoryRespawn.RespawnInventory(box);
            StartTimer(604800000, () => respawn_otis_assistant(attachee)); // 604800000ms is 1 week
            return;
        }
        public static void respawn_screng_assistant(GameObject attachee)
        {
            var box = Utilities.find_container_near(attachee, 1001);
            InventoryRespawn.RespawnInventory(box);
            StartTimer(604800000, () => respawn_screng_assistant(attachee)); // 604800000ms is 1 week
            return;
        }

    }
}
