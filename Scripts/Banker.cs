
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
    [ObjectScript(335)]
    public class Banker : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
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
            if ((!GetGlobalFlag(917)))
            {
                StartTimer(86400000, () => respawn(attachee)); // 86400000ms is 24 hours
                SetGlobalFlag(917, true);
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(963) == 6))
            {
                SetGlobalVar(963, 7);
                StartTimer(1814400000, () => repo_man()); // 21 days
            }

            return RunDefault;
        }
        public static bool repo_man()
        {
            if ((GetGlobalVar(963) == 7))
            {
                SetQuestState(82, QuestState.Botched);
                SetGlobalVar(963, 8);
                SetGlobalFlag(966, false);
                PartyLeader.AddReputation(38);
                PartyLeader.RemoveReputation(37);
            }

            return RunDefault;
        }
        public static void respawn(GameObjectBody attachee)
        {
            var box = Utilities.find_container_near(attachee, 1077);
            InventoryRespawn.RespawnInventory(box);
            StartTimer(86400000, () => respawn(attachee)); // 86400000ms is 24 hours
            return;
        }
        public static bool make_withdrawal(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.AdjustMoney(2000000);
            SetGlobalVar(899, GetGlobalVar(899) + 1);
            SetGlobalFlag(810, true);
            time_limit(attachee, triggerer);
            return RunDefault;
        }
        public static bool time_limit(GameObjectBody attachee, GameObjectBody triggerer)
        {
            StartTimer(86400000, () => reset_flag_806(attachee)); // 86400000ms is 24 hours
            return RunDefault;
        }
        public static bool reset_flag_806(GameObjectBody attachee)
        {
            SetGlobalFlag(810, false);
            return RunDefault;
        }

    }
}
