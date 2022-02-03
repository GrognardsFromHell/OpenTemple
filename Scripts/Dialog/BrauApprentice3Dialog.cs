
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

namespace Scripts.Dialog;

[DialogScript(90)]
public class BrauApprentice3Dialog : BrauApprentice3, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 3:
            case 63:
            case 64:
            case 321:
            case 322:
                originalScript = "not npc.has_met( pc )";
                return !npc.HasMet(pc);
            case 4:
            case 5:
            case 8:
            case 323:
            case 324:
            case 327:
                originalScript = "npc.has_met( pc )";
                return npc.HasMet(pc);
            case 6:
            case 7:
            case 325:
            case 326:
                originalScript = "( game.quests[60].state == qs_mentioned or game.quests[60].state == qs_accepted ) and game.global_flags[858] == 0 and npc.item_find(5815) != OBJ_HANDLE_NULL";
                return (GetQuestState(60) == QuestState.Mentioned || GetQuestState(60) == QuestState.Accepted) && !GetGlobalFlag(858) && npc.FindItemByName(5815) != null;
            case 61:
            case 62:
                originalScript = "npc.has_met( pc ) and game.global_flags[357] == 0";
                return npc.HasMet(pc) && !GetGlobalFlag(357);
            case 65:
            case 66:
                originalScript = "( game.quests[34].state == qs_mentioned or game.quests[34].state == qs_accepted ) and npc.has_met( pc )";
                return (GetQuestState(34) == QuestState.Mentioned || GetQuestState(34) == QuestState.Accepted) && npc.HasMet(pc);
            case 67:
            case 68:
                originalScript = "( game.quests[60].state == qs_mentioned or game.quests[60].state == qs_accepted ) and npc.has_met( pc ) and game.global_flags[858] == 0 and npc.item_find(5815) != OBJ_HANDLE_NULL";
                return (GetQuestState(60) == QuestState.Mentioned || GetQuestState(60) == QuestState.Accepted) && npc.HasMet(pc) && !GetGlobalFlag(858) && npc.FindItemByName(5815) != null;
            case 69:
            case 70:
                originalScript = "game.global_flags[357] == 1 and game.global_flags[858] == 0 and npc.item_find(5815) != OBJ_HANDLE_NULL";
                return GetGlobalFlag(357) && !GetGlobalFlag(858) && npc.FindItemByName(5815) != null;
            case 81:
            case 82:
            case 95:
            case 96:
                originalScript = "game.global_flags[322] == 1";
                return GetGlobalFlag(322);
            case 83:
            case 84:
            case 97:
            case 98:
                originalScript = "game.global_flags[322] == 0";
                return !GetGlobalFlag(322);
            case 91:
            case 92:
            case 213:
            case 214:
            case 263:
            case 264:
                originalScript = "game.quests[34].state == qs_mentioned or game.quests[34].state == qs_accepted";
                return GetQuestState(34) == QuestState.Mentioned || GetQuestState(34) == QuestState.Accepted;
            case 93:
            case 94:
            case 163:
            case 164:
            case 185:
            case 186:
            case 203:
            case 204:
            case 401:
            case 402:
            case 553:
            case 554:
                originalScript = "(game.quests[60].state == qs_mentioned or game.quests[60].state == qs_accepted) and game.global_flags[858] == 0 and npc.item_find(5815) != OBJ_HANDLE_NULL";
                return (GetQuestState(60) == QuestState.Mentioned || GetQuestState(60) == QuestState.Accepted) && !GetGlobalFlag(858) && npc.FindItemByName(5815) != null;
            case 133:
            case 134:
            case 231:
            case 232:
                originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 9";
                return pc.GetSkillLevel(npc, SkillId.intimidate) >= 9;
            case 183:
            case 184:
                originalScript = "game.quests[60].state == qs_unknown";
                return GetQuestState(60) == QuestState.Unknown;
            case 235:
            case 236:
                originalScript = "pc.money_get() >= 50000";
                return pc.GetMoney() >= 50000;
            case 403:
            case 404:
                originalScript = "( game.quests[34].state == qs_mentioned or game.quests[34].state == qs_accepted )";
                return (GetQuestState(34) == QuestState.Mentioned || GetQuestState(34) == QuestState.Accepted);
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 1:
            case 60:
            case 320:
                originalScript = "game.global_flags[356] = 1";
                SetGlobalFlag(356, true);
                break;
            case 150:
            case 300:
            case 550:
                originalScript = "game.global_flags[86] = 1";
                SetGlobalFlag(86, true);
                break;
            case 161:
            case 162:
            case 261:
            case 262:
            case 281:
            case 601:
                originalScript = "run_off(npc,pc)";
                run_off(npc, pc);
                break;
            case 185:
            case 186:
            case 190:
                originalScript = "game.global_flags[357] = 1";
                SetGlobalFlag(357, true);
                break;
            case 260:
            case 290:
            case 500:
                originalScript = "game.global_flags[858] = 1; npc.item_transfer_to(pc,5815)";
                SetGlobalFlag(858, true);
                npc.TransferItemByNameTo(pc, 5815);
                ;
                break;
            case 263:
            case 264:
                originalScript = "game.global_flags[86] = 1; run_off(npc,pc)";
                SetGlobalFlag(86, true);
                run_off(npc, pc);
                ;
                break;
            case 271:
            case 272:
                originalScript = "pc.money_adj(-50000); npc.item_transfer_to(pc,5815); game.global_flags[858] = 1";
                pc.AdjustMoney(-50000);
                npc.TransferItemByNameTo(pc, 5815);
                SetGlobalFlag(858, true);
                ;
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
            case 133:
            case 134:
            case 231:
            case 232:
                skillChecks = new DialogSkillChecks(SkillId.intimidate, 9);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}