
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace VanillaScripts.Dialog;

[DialogScript(15)]
public class ProsperousElderDDialog : ProsperousElderD, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 3:
                originalScript = "not npc.has_met( pc ) and game.quests[5].state != qs_completed";
                return !npc.HasMet(pc) && GetQuestState(5) != QuestState.Completed;
            case 4:
            case 5:
                originalScript = "game.quests[5].state != qs_completed and game.global_flags[17] == 0";
                return GetQuestState(5) != QuestState.Completed && !GetGlobalFlag(17);
            case 6:
            case 7:
                originalScript = "npc.has_met( pc ) and game.quests[5].state == qs_completed";
                return npc.HasMet(pc) && GetQuestState(5) == QuestState.Completed;
            case 8:
            case 9:
                originalScript = "game.quests[5].state == qs_accepted and game.global_flags[17] == 1";
                return GetQuestState(5) == QuestState.Accepted && GetGlobalFlag(17);
            case 10:
            case 11:
                originalScript = "not npc.has_met( pc ) and game.quests[5].state == qs_completed";
                return !npc.HasMet(pc) && GetQuestState(5) == QuestState.Completed;
            case 21:
            case 22:
            case 23:
            case 31:
            case 32:
            case 33:
                originalScript = "game.quests[5].state <= qs_mentioned";
                return GetQuestState(5) <= QuestState.Mentioned;
            case 24:
            case 25:
            case 34:
            case 35:
                originalScript = "game.quests[5].state == qs_accepted";
                return GetQuestState(5) == QuestState.Accepted;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 80:
                originalScript = "make_like( npc, pc )";
                make_like(npc, pc);
                break;
            default:
                originalScript = null;
                return;
        }
    }
    public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
    {
        switch (lineNumber)
        {
            default:
                skillChecks = default;
                return false;
        }
    }
}