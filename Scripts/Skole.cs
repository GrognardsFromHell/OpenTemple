
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
    [ObjectScript(118)]
    public class Skole : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if (((GetGlobalFlag(202)) && (GetQuestState(42) != QuestState.Completed)))
            {
                triggerer.BeginDialog(attachee, 360);
            }
            else if ((PartyLeader.HasReputation(23) && GetGlobalFlag(94) && !GetGlobalFlag(851) && attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 400);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(201)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
                SetGlobalFlag(202, false);
            }

            if ((!GetGlobalFlag(913)))
            {
                StartTimer(604800000, () => respawn(attachee)); // 604800000ms is 1 week
                SetGlobalFlag(913, true);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(202, false);
            ScriptDaemon.set_f("skole_dead");
            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(202, true);
            ScriptDaemon.set_f("skole_dead", false);
            return RunDefault;
        }
        public static bool prepare_goons(GameObject attachee)
        {
            // This script schedules Skole's Goons
            // Reworked to use global timing system
            StartTimer(259200000, () => goons_attack(attachee));
            ScriptDaemon.record_time_stamp("s_skole_goons");
            return RunDefault;
        }
        public static bool goons_attack(GameObject attachee)
        {
            if (GetQuestState(42) != QuestState.Completed && !ScriptDaemon.get_f("s_skole_goons_scheduled") && !ScriptDaemon.get_f("skole_dead"))
            {
                ScriptDaemon.set_f("s_skole_goons_scheduled");
                SetQuestState(42, QuestState.Botched);
                SetGlobalFlag(202, true);
                QueueRandomEncounter(3004);
            }

            return RunDefault;
        }
        public static void respawn(GameObject attachee)
        {
            var box = Utilities.find_container_near(attachee, 1001);
            InventoryRespawn.RespawnInventory(box);
            StartTimer(604800000, () => respawn(attachee)); // 604800000ms is 1 week
            return;
        }

    }
}
