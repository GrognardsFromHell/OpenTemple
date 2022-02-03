
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
    [ObjectScript(25)]
    public class Woodcutter : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            attachee.TurnTowards(triggerer);
            if ((GetQuestState(99) == QuestState.Completed || GetQuestState(99) == QuestState.Botched))
            {
                triggerer.BeginDialog(attachee, 600);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(875) && !GetGlobalFlag(876) && GetQuestState(99) != QuestState.Completed && !triggerer.GetPartyMembers().Any(o => o.HasItemByName(12900))))
            {
                SetGlobalFlag(876, true);
                StartTimer(140000000, () => amii_dies());
            }

            return RunDefault;
        }
        public static bool amii_dies()
        {
            SetQuestState(99, QuestState.Botched);
            SetGlobalFlag(862, true);
            return RunDefault;
        }

        private const int MsPerHour = 60 * 60 * 1000;
        public static bool letter_written()
        {
            SetGlobalVar(767, 2);
            StartTimer(76 * MsPerHour, () => give_reward());
            return RunDefault;
        }
        public static bool give_reward()
        {
            SetGlobalVar(767, 3);
            return RunDefault;
        }
        public static bool order_item()
        {
            SetGlobalVar(769, 1);
            StartTimer(409 * MsPerHour, () => item_arrived());
            return RunDefault;
        }
        public static bool item_arrived()
        {
            SetGlobalVar(769, 2);
            return RunDefault;
        }
        public static bool give_item(GameObject pc)
        {
            SetGlobalVar(769, 0);
            var item = GetGlobalVar(768);
            Utilities.create_item_in_inventory(item, pc);
            SetGlobalVar(768, 0);
            SetGlobalVar(776, GetGlobalVar(776) + 1);
            return RunDefault;
        }

    }
}
