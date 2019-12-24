
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace VanillaScripts.Dialog
{
    [DialogScript(125)]
    public class BelsornigDialog : Belsornig, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 22:
                case 23:
                    originalScript = "game.global_flags[91] == 1 or game.global_flags[92] ==1 or game.quests[15].state == qs_completed or game.quests[16].state == qs_completed or game.quests[17].state == qs_completed";
                    return GetGlobalFlag(91) || GetGlobalFlag(92) || GetQuestState(15) == QuestState.Completed || GetQuestState(16) == QuestState.Completed || GetQuestState(17) == QuestState.Completed;
                case 31:
                    originalScript = "game.quests[45].state == qs_unknown and game.quests[51].state == qs_unknown";
                    return GetQuestState(45) == QuestState.Unknown && GetQuestState(51) == QuestState.Unknown;
                case 32:
                    originalScript = "game.quests[45].state >= qs_mentioned and game.quests[51].state == qs_unknown";
                    return GetQuestState(45) >= QuestState.Mentioned && GetQuestState(51) == QuestState.Unknown;
                case 33:
                    originalScript = "game.quests[45].state == qs_unknown and game.quests[51].state > qs_mentioned";
                    return GetQuestState(45) == QuestState.Unknown && GetQuestState(51) > QuestState.Mentioned;
                case 34:
                    originalScript = "game.quests[45].state >= qs_mentioned and game.quests[51].state >= qs_mentioned";
                    return GetQuestState(45) >= QuestState.Mentioned && GetQuestState(51) >= QuestState.Mentioned;
                case 41:
                case 45:
                    originalScript = "game.global_flags[110] == 1";
                    return GetGlobalFlag(110);
                case 42:
                case 46:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 8023 )";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8023));
                case 43:
                case 47:
                    originalScript = "game.global_flags[110] == 0 and game.quests[52].state == qs_completed";
                    return !GetGlobalFlag(110) && GetQuestState(52) == QuestState.Completed;
                case 44:
                case 48:
                    originalScript = "game.global_flags[110] == 0 and game.quests[52].state != qs_completed and not anyone( pc.group_list(), \"has_follower\", 8023 )";
                    return !GetGlobalFlag(110) && GetQuestState(52) != QuestState.Completed && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8023));
                case 51:
                case 52:
                case 95:
                case 96:
                case 261:
                case 262:
                case 411:
                case 412:
                    originalScript = "game.global_flags[107] == 0";
                    return !GetGlobalFlag(107);
                case 53:
                case 54:
                case 93:
                case 94:
                case 263:
                case 264:
                case 357:
                case 358:
                case 413:
                case 414:
                    originalScript = "game.global_flags[107] == 1";
                    return GetGlobalFlag(107);
                case 71:
                    originalScript = "game.global_flags[104] == 0 and game.global_flags[106] == 0";
                    return !GetGlobalFlag(104) && !GetGlobalFlag(106);
                case 72:
                    originalScript = "game.global_flags[104] == 0 and game.global_flags[106] == 1";
                    return !GetGlobalFlag(104) && GetGlobalFlag(106);
                case 73:
                    originalScript = "game.global_flags[104] == 1";
                    return GetGlobalFlag(104);
                case 121:
                case 122:
                case 191:
                case 192:
                case 461:
                case 462:
                case 581:
                case 582:
                    originalScript = "game.global_vars[12] == 0";
                    return GetGlobalVar(12) == 0;
                case 123:
                case 124:
                case 193:
                case 194:
                case 463:
                case 464:
                case 583:
                case 584:
                    originalScript = "game.global_vars[12] > 0";
                    return GetGlobalVar(12) > 0;
                case 181:
                case 182:
                    originalScript = "game.global_vars[12] > 1";
                    return GetGlobalVar(12) > 1;
                case 183:
                case 184:
                    originalScript = "game.global_vars[12] == 1";
                    return GetGlobalVar(12) == 1;
                case 203:
                case 204:
                case 273:
                case 274:
                    originalScript = "pc.skill_level_get(npc, skill_gather_information) >= 6";
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 6;
                case 331:
                case 334:
                    originalScript = "game.quests[45].state == qs_mentioned";
                    return GetQuestState(45) == QuestState.Mentioned;
                case 332:
                case 335:
                    originalScript = "game.quests[45].state == qs_accepted";
                    return GetQuestState(45) == QuestState.Accepted;
                case 351:
                case 354:
                    originalScript = "game.quests[51].state == qs_mentioned";
                    return GetQuestState(51) == QuestState.Mentioned;
                case 352:
                case 355:
                    originalScript = "game.quests[51].state == qs_accepted";
                    return GetQuestState(51) == QuestState.Accepted;
                case 371:
                case 376:
                    originalScript = "(game.quests[46].state == qs_mentioned or game.quests[46].state == qs_accepted) and game.global_flags[110] == 1";
                    return (GetQuestState(46) == QuestState.Mentioned || GetQuestState(46) == QuestState.Accepted) && GetGlobalFlag(110);
                case 372:
                case 377:
                    originalScript = "(game.quests[46].state == qs_mentioned or game.quests[46].state == qs_accepted) and anyone( pc.group_list(), \"has_follower\", 8023 )";
                    return (GetQuestState(46) == QuestState.Mentioned || GetQuestState(46) == QuestState.Accepted) && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8023));
                case 373:
                case 378:
                    originalScript = "(game.quests[46].state == qs_mentioned or game.quests[46].state == qs_accepted) and game.global_flags[110] == 0 and game.quests[52].state == qs_completed";
                    return (GetQuestState(46) == QuestState.Mentioned || GetQuestState(46) == QuestState.Accepted) && !GetGlobalFlag(110) && GetQuestState(52) == QuestState.Completed;
                case 374:
                case 379:
                    originalScript = "game.global_flags[110] == 0 and game.global_flags[112] == 1 and game.quests[46].state == qs_accepted";
                    return !GetGlobalFlag(110) && GetGlobalFlag(112) && GetQuestState(46) == QuestState.Accepted;
                case 375:
                case 380:
                    originalScript = "game.quests[46].state == qs_mentioned and game.global_flags[110] == 0 and game.quests[52].state != qs_completed and not anyone( pc.group_list(), \"has_follower\", 8023 )";
                    return GetQuestState(46) == QuestState.Mentioned && !GetGlobalFlag(110) && GetQuestState(52) != QuestState.Completed && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8023));
                case 381:
                case 387:
                    originalScript = "game.quests[47].state == qs_mentioned and game.global_flags[107] == 0";
                    return GetQuestState(47) == QuestState.Mentioned && !GetGlobalFlag(107);
                case 382:
                case 388:
                    originalScript = "game.quests[47].state == qs_mentioned and game.global_flags[107] == 1";
                    return GetQuestState(47) == QuestState.Mentioned && GetGlobalFlag(107);
                case 383:
                case 389:
                    originalScript = "game.quests[47].state == qs_accepted and game.global_flags[107] == 1 and game.global_flags[113] == 0 and game.global_flags[104] == 0 and game.global_flags[106] == 0";
                    return GetQuestState(47) == QuestState.Accepted && GetGlobalFlag(107) && !GetGlobalFlag(113) && !GetGlobalFlag(104) && !GetGlobalFlag(106);
                case 384:
                case 390:
                    originalScript = "game.quests[47].state == qs_accepted and game.global_flags[107] == 1 and game.global_flags[104] == 0 and game.global_flags[106] == 1";
                    return GetQuestState(47) == QuestState.Accepted && GetGlobalFlag(107) && !GetGlobalFlag(104) && GetGlobalFlag(106);
                case 385:
                case 391:
                    originalScript = "game.quests[47].state == qs_accepted and game.global_flags[107] == 1 and game.global_flags[104] == 1";
                    return GetQuestState(47) == QuestState.Accepted && GetGlobalFlag(107) && GetGlobalFlag(104);
                case 386:
                case 392:
                    originalScript = "game.quests[47].state == qs_accepted and game.global_flags[107] == 1 and game.global_flags[113] == 1 and game.global_flags[104] == 0 and game.global_flags[106] == 0";
                    return GetQuestState(47) == QuestState.Accepted && GetGlobalFlag(107) && GetGlobalFlag(113) && !GetGlobalFlag(104) && !GetGlobalFlag(106);
                case 393:
                case 397:
                    originalScript = "game.quests[48].state == qs_mentioned";
                    return GetQuestState(48) == QuestState.Mentioned;
                case 394:
                case 398:
                    originalScript = "game.quests[48].state == qs_accepted and game.global_vars[12] < 2 and game.global_flags[114] == 1";
                    return GetQuestState(48) == QuestState.Accepted && GetGlobalVar(12) < 2 && GetGlobalFlag(114);
                case 395:
                case 399:
                    originalScript = "game.quests[48].state == qs_accepted and game.global_vars[12] == 2 and game.global_flags[114] == 0";
                    return GetQuestState(48) == QuestState.Accepted && GetGlobalVar(12) == 2 && !GetGlobalFlag(114);
                case 396:
                case 400:
                    originalScript = "game.quests[48].state == qs_accepted and game.global_vars[12] == 2 and game.global_flags[114] == 1";
                    return GetQuestState(48) == QuestState.Accepted && GetGlobalVar(12) == 2 && GetGlobalFlag(114);
                case 401:
                case 402:
                    originalScript = "game.quests[48].state == qs_completed";
                    return GetQuestState(48) == QuestState.Completed;
                case 441:
                case 442:
                    originalScript = "anyone( pc.group_list(), \"item_find\", 3602 )";
                    return pc.GetPartyMembers().Any(o => o.FindItemByName(3602) != null);
                case 443:
                case 444:
                    originalScript = "not anyone( pc.group_list(), \"item_find\", 3602 )";
                    return !pc.GetPartyMembers().Any(o => o.FindItemByName(3602) != null);
                case 453:
                case 454:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 10";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 10;
                case 481:
                case 482:
                    originalScript = "anyone( pc.group_list(), \"item_find\", 3010 )";
                    return pc.GetPartyMembers().Any(o => o.FindItemByName(3010) != null);
                case 483:
                case 484:
                    originalScript = "not anyone( pc.group_list(), \"item_find\", 3010 )";
                    return !pc.GetPartyMembers().Any(o => o.FindItemByName(3010) != null);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 20:
                    originalScript = "game.story_state = 5";
                    StoryState = 5;
                    break;
                case 50:
                case 220:
                case 260:
                case 270:
                    originalScript = "game.quests[46].state = qs_mentioned";
                    SetQuestState(46, QuestState.Mentioned);
                    break;
                case 51:
                case 52:
                case 53:
                case 54:
                case 261:
                case 262:
                case 263:
                case 264:
                    originalScript = "game.quests[46].state = qs_botched";
                    SetQuestState(46, QuestState.Botched);
                    break;
                case 70:
                case 200:
                    originalScript = "game.quests[47].state = qs_mentioned";
                    SetQuestState(47, QuestState.Mentioned);
                    break;
                case 100:
                    originalScript = "game.quests[47].state = qs_accepted; npc.item_transfer_to(pc,3602); game.global_flags[345] = 1";
                    SetQuestState(47, QuestState.Accepted);
                    npc.TransferItemByNameTo(pc, 3602);
                    SetGlobalFlag(345, true);
                    ;
                    break;
                case 120:
                case 190:
                case 460:
                case 580:
                    originalScript = "game.quests[47].state = qs_completed; pc.reputation_add( 12 )";
                    SetQuestState(47, QuestState.Completed);
                    pc.AddReputation(12);
                    ;
                    break;
                case 130:
                case 180:
                    originalScript = "game.quests[48].state = qs_mentioned";
                    SetQuestState(48, QuestState.Mentioned);
                    break;
                case 160:
                    originalScript = "game.quests[48].state = qs_accepted; create_item_in_inventory(6110,pc); game.global_flags[345] = 1";
                    SetQuestState(48, QuestState.Accepted);
                    Utilities.create_item_in_inventory(6110, pc);
                    SetGlobalFlag(345, true);
                    ;
                    break;
                case 300:
                    originalScript = "game.quests[46].state = qs_accepted; game.global_flags[345] = 1";
                    SetQuestState(46, QuestState.Accepted);
                    SetGlobalFlag(345, true);
                    ;
                    break;
                case 333:
                case 336:
                case 353:
                case 356:
                case 363:
                case 364:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 410:
                    originalScript = "game.quests[46].state = qs_completed; pc.reputation_add( 12 )";
                    SetQuestState(46, QuestState.Completed);
                    pc.AddReputation(12);
                    ;
                    break;
                case 500:
                    originalScript = "game.quests[48].state = qs_completed";
                    SetQuestState(48, QuestState.Completed);
                    break;
                case 540:
                    originalScript = "game.map_flags( 5067, 2, 1 )";
                    // FIXME: map_flags;
                    break;
                case 550:
                    originalScript = "game.map_flags( 5078, 3, 1 )";
                    // FIXME: map_flags;
                    break;
                case 560:
                    originalScript = "game.map_flags( 5078, 4, 1 )";
                    // FIXME: map_flags;
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
                case 203:
                case 204:
                case 273:
                case 274:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 6);
                    return true;
                case 453:
                case 454:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
