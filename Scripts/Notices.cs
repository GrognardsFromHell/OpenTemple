
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
    [ObjectScript(468)]
    public class Notices : BaseObjectScript
    {
        public override bool OnUse(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetNameId() == 11063))
            {
                SetQuestState(110, QuestState.Mentioned);
                DetachScript();
            }
            else if ((attachee.GetNameId() == 11064))
            {
                SetQuestState(90, QuestState.Mentioned);
                DetachScript();
            }
            else if ((attachee.GetNameId() == 11065))
            {
                SetQuestState(111, QuestState.Mentioned);
                DetachScript();
            }
            else if ((attachee.GetNameId() == 11066))
            {
                SetQuestState(112, QuestState.Mentioned);
                DetachScript();
            }
            else if ((attachee.GetNameId() == 11067))
            {
                SetQuestState(108, QuestState.Mentioned);
                SetGlobalVar(939, 1);
                DetachScript();
            }
            else if ((attachee.GetNameId() == 11068))
            {
                if ((GetQuestState(97) != QuestState.Botched))
                {
                    SetQuestState(97, QuestState.Botched);
                }

                if ((!PartyLeader.HasReputation(53)))
                {
                    PartyLeader.AddReputation(53);
                }

                SetGlobalVar(510, 2);
                SetGlobalFlag(504, true);
                DetachScript();
            }
            else if ((attachee.GetNameId() == 11069))
            {
                triggerer.AdjustMoney(-10000);
                attachee.Destroy();
            }
            else if ((attachee.GetNameId() == 11070))
            {
                SetQuestState(106, QuestState.Mentioned);
                DetachScript();
            }
            else if ((attachee.GetNameId() == 11071))
            {
                SetQuestState(95, QuestState.Completed);
                DetachScript();
            }
            else if ((attachee.GetNameId() == 11072))
            {
                SetQuestState(105, QuestState.Mentioned);
                set_bethany();
                DetachScript();
            }
            else if ((attachee.GetNameId() == 11073))
            {
                SetQuestState(105, QuestState.Mentioned);
                set_bethany();
                DetachScript();
            }

            return RunDefault;
        }
        public static bool set_bethany()
        {
            QueueRandomEncounter(3447);
            ScriptDaemon.set_f("s_bethany_scheduled");
            return RunDefault;
        }

    }
}
