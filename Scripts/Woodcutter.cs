
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
    [ObjectScript(25)]
    public class Woodcutter : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
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
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
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
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
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
        public static bool letter_written()
        {
            SetGlobalVar(767, 2);
            timedEventAdd(give_reward, (), 76);
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
            timedEventAdd(item_arrived, (), 409);
            return RunDefault;
        }
        public static bool item_arrived()
        {
            SetGlobalVar(769, 2);
            return RunDefault;
        }
        public static bool give_item(GameObjectBody pc)
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
