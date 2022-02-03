
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

[DialogScript(107)]
public class PrestonDialog : Preston, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 6:
            case 7:
            case 247:
            case 248:
                originalScript = "game.quests[35].state == qs_completed";
                return GetQuestState(35) == QuestState.Completed;
            case 13:
            case 14:
                originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 5";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 5;
            case 15:
            case 16:
                originalScript = "pc.skill_level_get(npc, skill_gather_information) >= 8";
                return pc.GetSkillLevel(npc, SkillId.gather_information) >= 8;
            case 22:
            case 23:
            case 292:
            case 293:
                originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 6";
                return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 6;
            case 31:
            case 32:
            case 42:
            case 43:
            case 71:
            case 72:
            case 141:
            case 142:
                originalScript = "anyone( pc.group_list(), \"has_follower\", 8020 )";
                return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8020));
            case 83:
            case 84:
                originalScript = "game.global_flags[320] == 1";
                return GetGlobalFlag(320);
            case 85:
            case 86:
                originalScript = "game.global_flags[320] == 0";
                return !GetGlobalFlag(320);
            case 91:
            case 92:
                originalScript = "game.quests[33].state == qs_completed or game.quests[34].state == qs_completed";
                return GetQuestState(33) == QuestState.Completed || GetQuestState(34) == QuestState.Completed;
            case 93:
            case 94:
                originalScript = "game.quests[33].state == qs_accepted or game.quests[34].state == qs_accepted";
                return GetQuestState(33) == QuestState.Accepted || GetQuestState(34) == QuestState.Accepted;
            case 174:
            case 175:
            case 184:
            case 185:
            case 245:
            case 246:
                originalScript = "game.quests[41].state != qs_completed";
                return GetQuestState(41) != QuestState.Completed;
            case 178:
            case 179:
            case 186:
            case 187:
                originalScript = "game.quests[41].state == qs_completed and game.global_flags[94] == 0";
                return GetQuestState(41) == QuestState.Completed && !GetGlobalFlag(94);
            case 182:
            case 183:
            case 241:
            case 242:
                originalScript = "(game.quests[41].state == qs_accepted or game.quests[41].state == qs_mentioned) and (anyone( pc.group_list(), \"has_follower\", 8020 ))";
                return (GetQuestState(41) == QuestState.Accepted || GetQuestState(41) == QuestState.Mentioned) && (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8020)));
            case 188:
            case 190:
            case 243:
            case 244:
                originalScript = "game.quests[41].state != qs_completed and (game.quests[33].state == qs_completed or game.quests[34].state == qs_completed)";
                return GetQuestState(41) != QuestState.Completed && (GetQuestState(33) == QuestState.Completed || GetQuestState(34) == QuestState.Completed);
            case 191:
            case 192:
                originalScript = "game.story_state >= 4";
                return StoryState >= 4;
            case 193:
            case 194:
            case 249:
            case 250:
                originalScript = "game.quests[41].state != qs_completed and game.quests[60].state == qs_completed";
                return GetQuestState(41) != QuestState.Completed && GetQuestState(60) == QuestState.Completed;
            case 262:
            case 265:
                originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 7 and pc.money_get() >= 50000";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 7 && pc.GetMoney() >= 50000;
            case 271:
                originalScript = "pc.money_get() >= 80000";
                return pc.GetMoney() >= 80000;
            case 321:
            case 322:
                originalScript = "game.quests[60].state == qs_completed";
                return GetQuestState(60) == QuestState.Completed;
            case 323:
            case 324:
                originalScript = "game.quests[60].state == qs_accepted";
                return GetQuestState(60) == QuestState.Accepted;
            case 327:
            case 328:
                originalScript = "game.quests[41].state <= qs_mentioned";
                return GetQuestState(41) <= QuestState.Mentioned;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 30:
            case 40:
            case 70:
            case 140:
                originalScript = "game.quests[41].state = qs_mentioned";
                SetQuestState(41, QuestState.Mentioned);
                break;
            case 31:
            case 32:
            case 42:
            case 43:
            case 71:
            case 72:
            case 141:
            case 142:
            case 182:
            case 183:
            case 241:
            case 242:
                originalScript = "game.quests[41].state = qs_accepted; buttin(npc,pc,290)";
                SetQuestState(41, QuestState.Accepted);
                buttin(npc, pc, 290);
                ;
                break;
            case 91:
            case 92:
            case 93:
            case 94:
            case 97:
            case 98:
            case 321:
            case 322:
            case 323:
            case 324:
            case 327:
            case 328:
                originalScript = "game.quests[41].state = qs_accepted";
                SetQuestState(41, QuestState.Accepted);
                break;
            case 111:
                originalScript = "pc.money_adj(+100)";
                pc.AdjustMoney(+100);
                break;
            case 120:
                originalScript = "game.global_flags[93] = 1";
                SetGlobalFlag(93, true);
                break;
            case 130:
            case 210:
                originalScript = "game.quests[41].state = qs_completed; game.global_flags[93] = 1";
                SetQuestState(41, QuestState.Completed);
                SetGlobalFlag(93, true);
                ;
                break;
            case 201:
                originalScript = "buttin(npc,pc,195)";
                buttin(npc, pc, 195);
                break;
            case 271:
                originalScript = "pc.money_adj(-80000)";
                pc.AdjustMoney(-80000);
                break;
            case 281:
                originalScript = "pc.money_adj(-50000)";
                pc.AdjustMoney(-50000);
                break;
            case 300:
                originalScript = "game.global_flags[94] = 1";
                SetGlobalFlag(94, true);
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
            case 13:
            case 14:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 5);
                return true;
            case 15:
            case 16:
                skillChecks = new DialogSkillChecks(SkillId.gather_information, 8);
                return true;
            case 22:
            case 23:
            case 292:
            case 293:
                skillChecks = new DialogSkillChecks(SkillId.sense_motive, 6);
                return true;
            case 262:
            case 265:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 7);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}