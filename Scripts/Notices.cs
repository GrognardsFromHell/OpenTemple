
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

[ObjectScript(468)]
public class Notices : BaseObjectScript
{
    public override bool OnUse(GameObject attachee, GameObject triggerer)
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