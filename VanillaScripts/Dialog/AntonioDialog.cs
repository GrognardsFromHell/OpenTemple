
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

[DialogScript(139)]
public class AntonioDialog : Antonio, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 4:
            case 5:
            case 91:
            case 92:
            case 101:
            case 102:
            case 111:
            case 112:
                originalScript = "game.quests[52].state == qs_unknown and game.global_flags[107] == 0";
                return GetQuestState(52) == QuestState.Unknown && !GetGlobalFlag(107);
            case 6:
            case 7:
            case 95:
            case 96:
            case 105:
            case 106:
            case 115:
            case 116:
                originalScript = "game.quests[52].state >= qs_accepted and game.global_flags[107] == 0";
                return GetQuestState(52) >= QuestState.Accepted && !GetGlobalFlag(107);
            case 27:
            case 28:
            case 51:
            case 52:
            case 97:
            case 98:
            case 107:
            case 108:
            case 117:
            case 118:
                originalScript = "game.quests[57].state == qs_accepted and game.global_flags[142] == 0";
                return GetQuestState(57) == QuestState.Accepted && !GetGlobalFlag(142);
            case 29:
            case 30:
                originalScript = "game.global_flags[141] == 1 or pc.skill_level_get(npc, skill_bluff) >= 14";
                return GetGlobalFlag(141) || pc.GetSkillLevel(npc, SkillId.bluff) >= 14;
            case 31:
            case 32:
            case 53:
            case 54:
                originalScript = "pc.money_get() >= 20000 and game.quests[57].state == qs_accepted and game.global_flags[142] == 1";
                return pc.GetMoney() >= 20000 && GetQuestState(57) == QuestState.Accepted && GetGlobalFlag(142);
            case 41:
            case 42:
                originalScript = "game.global_flags[118] == 0 and ( game.quests[52].state == qs_mentioned or game.quests[52].state == qs_accepted )";
                return !GetGlobalFlag(118) && (GetQuestState(52) == QuestState.Mentioned || GetQuestState(52) == QuestState.Accepted);
            case 43:
            case 44:
                originalScript = "anyone( pc.group_list(), \"has_follower\", 8023 ) and game.global_flags[118] == 0 and ( game.quests[52].state == qs_mentioned or game.quests[52].state == qs_accepted )";
                return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8023)) && !GetGlobalFlag(118) && (GetQuestState(52) == QuestState.Mentioned || GetQuestState(52) == QuestState.Accepted);
            case 45:
            case 46:
                originalScript = "( game.quests[53].state == qs_mentioned or game.quests[53].state == qs_accepted )";
                return (GetQuestState(53) == QuestState.Mentioned || GetQuestState(53) == QuestState.Accepted);
            case 47:
            case 48:
                originalScript = "( game.quests[54].state == qs_mentioned or game.quests[54].state == qs_accepted ) and (not anyone( pc.group_list(), \"has_item\", 2203 ))";
                return (GetQuestState(54) == QuestState.Mentioned || GetQuestState(54) == QuestState.Accepted) && (!pc.GetPartyMembers().Any(o => o.HasItemByName(2203)));
            case 63:
            case 64:
                originalScript = "game.quests[57].state == qs_accepted and game.global_flags[142] = 0";
                throw new NotSupportedException("Conversion failed.");
            case 142:
            case 143:
                originalScript = "game.global_vars[13] >= 2";
                return GetGlobalVar(13) >= 2;
            case 144:
            case 145:
                originalScript = "game.global_vars[14] == 5";
                return GetGlobalVar(14) == 5;
            case 146:
            case 147:
                originalScript = "game.global_vars[13] == 1";
                return GetGlobalVar(13) == 1;
            case 213:
            case 214:
            case 511:
            case 512:
                originalScript = "pc.money_get() >= 20000";
                return pc.GetMoney() >= 20000;
            case 231:
                originalScript = "game.global_vars[16] <= 8";
                return GetGlobalVar(16) <= 8;
            case 232:
                originalScript = "game.global_vars[16] > 8 and game.global_vars[16] <= 16";
                return GetGlobalVar(16) > 8 && GetGlobalVar(16) <= 16;
            case 233:
                originalScript = "game.global_vars[16] > 16 and game.global_vars[16] <= 20";
                return GetGlobalVar(16) > 16 && GetGlobalVar(16) <= 20;
            case 234:
                originalScript = "game.global_vars[16] == 21";
                return GetGlobalVar(16) == 21;
            case 235:
                originalScript = "game.global_vars[16] == 22";
                return GetGlobalVar(16) == 22;
            case 236:
                originalScript = "game.global_vars[16] > 22 and game.global_vars[16] <= 32";
                return GetGlobalVar(16) > 22 && GetGlobalVar(16) <= 32;
            case 237:
                originalScript = "game.global_vars[16] > 32 and game.global_vars[16] <= 42";
                return GetGlobalVar(16) > 32 && GetGlobalVar(16) <= 42;
            case 238:
                originalScript = "game.global_vars[16] > 42 and game.global_vars[16] <= 50";
                return GetGlobalVar(16) > 42 && GetGlobalVar(16) <= 50;
            case 239:
                originalScript = "game.global_vars[16] > 50 and game.global_vars[16] <= 58";
                return GetGlobalVar(16) > 50 && GetGlobalVar(16) <= 58;
            case 240:
                originalScript = "game.global_vars[16] > 58 and game.global_vars[16] <= 62";
                return GetGlobalVar(16) > 58 && GetGlobalVar(16) <= 62;
            case 241:
                originalScript = "game.global_vars[16] > 62 and game.global_vars[16] <= 66";
                return GetGlobalVar(16) > 62 && GetGlobalVar(16) <= 66;
            case 242:
                originalScript = "game.global_vars[16] == 67";
                return GetGlobalVar(16) == 67;
            case 243:
                originalScript = "game.global_vars[16] == 68";
                return GetGlobalVar(16) == 68;
            case 244:
                originalScript = "game.global_vars[16] > 68 and game.global_vars[16] <= 75";
                return GetGlobalVar(16) > 68 && GetGlobalVar(16) <= 75;
            case 245:
                originalScript = "game.global_vars[16] > 75 and game.global_vars[16] <= 82";
                return GetGlobalVar(16) > 75 && GetGlobalVar(16) <= 82;
            case 246:
                originalScript = "game.global_vars[16] > 82 and game.global_vars[16] <= 86";
                return GetGlobalVar(16) > 82 && GetGlobalVar(16) <= 86;
            case 247:
                originalScript = "game.global_vars[16] > 86 and game.global_vars[16] <= 90";
                return GetGlobalVar(16) > 86 && GetGlobalVar(16) <= 90;
            case 248:
                originalScript = "game.global_vars[16] > 90 and game.global_vars[16] <= 92";
                return GetGlobalVar(16) > 90 && GetGlobalVar(16) <= 92;
            case 249:
                originalScript = "game.global_vars[16] > 92 and game.global_vars[16] <= 94";
                return GetGlobalVar(16) > 92 && GetGlobalVar(16) <= 94;
            case 250:
                originalScript = "game.global_vars[16] == 95";
                return GetGlobalVar(16) == 95;
            case 251:
                originalScript = "game.global_vars[16] == 96";
                return GetGlobalVar(16) == 96;
            case 252:
                originalScript = "game.global_vars[16] == 97";
                return GetGlobalVar(16) == 97;
            case 253:
                originalScript = "game.global_vars[16] == 98";
                return GetGlobalVar(16) == 98;
            case 254:
                originalScript = "game.global_vars[16] == 99";
                return GetGlobalVar(16) == 99;
            case 255:
                originalScript = "game.global_vars[16] == 100";
                return GetGlobalVar(16) == 100;
            case 313:
            case 314:
            case 333:
            case 334:
            case 353:
            case 354:
            case 373:
            case 374:
            case 393:
            case 394:
            case 413:
            case 414:
            case 433:
            case 434:
            case 463:
            case 464:
            case 473:
            case 474:
            case 503:
            case 504:
                originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 8";
                return pc.GetSkillLevel(npc, SkillId.intimidate) >= 8;
            case 315:
            case 316:
            case 335:
            case 336:
            case 355:
            case 356:
            case 375:
            case 376:
            case 395:
            case 396:
            case 415:
            case 416:
            case 435:
            case 436:
            case 465:
            case 466:
            case 475:
            case 476:
            case 505:
            case 506:
                originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 10";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 10;
            case 553:
            case 554:
                originalScript = "pc.skill_level_get(npc, skill_bluff) >= 9";
                return pc.GetSkillLevel(npc, SkillId.bluff) >= 9;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 3:
            case 21:
            case 22:
            case 321:
            case 322:
            case 341:
            case 342:
            case 361:
            case 362:
            case 381:
            case 382:
            case 401:
            case 402:
            case 421:
            case 422:
            case 441:
            case 442:
            case 451:
            case 452:
            case 481:
            case 482:
            case 491:
            case 492:
            case 522:
            case 523:
                originalScript = "npc.attack( pc )";
                npc.Attack(pc);
                break;
            case 230:
                originalScript = "game.global_vars[16] = game.random_range( 1, 100 )";
                SetGlobalVar(16, RandomRange(1, 100));
                break;
            case 320:
            case 340:
            case 360:
            case 380:
            case 400:
            case 420:
            case 440:
            case 450:
            case 480:
            case 490:
                originalScript = "game.global_flags[142] = 1";
                SetGlobalFlag(142, true);
                break;
            case 323:
            case 324:
            case 343:
            case 344:
            case 363:
            case 364:
            case 383:
            case 384:
            case 403:
            case 404:
            case 423:
            case 424:
            case 443:
            case 444:
            case 453:
            case 454:
            case 483:
            case 484:
            case 493:
            case 494:
                originalScript = "pc.money_adj(-20000)";
                pc.AdjustMoney(-20000);
                break;
            case 530:
                originalScript = "npc.item_transfer_to(pc,2206)";
                npc.TransferItemByNameTo(pc, 2206);
                break;
            case 560:
                originalScript = "kill_tubal(npc)";
                kill_tubal(npc);
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
            case 29:
            case 30:
                skillChecks = new DialogSkillChecks(SkillId.bluff, 14);
                return true;
            case 313:
            case 314:
            case 333:
            case 334:
            case 353:
            case 354:
            case 373:
            case 374:
            case 393:
            case 394:
            case 413:
            case 414:
            case 433:
            case 434:
            case 463:
            case 464:
            case 473:
            case 474:
            case 503:
            case 504:
                skillChecks = new DialogSkillChecks(SkillId.intimidate, 8);
                return true;
            case 315:
            case 316:
            case 335:
            case 336:
            case 355:
            case 356:
            case 375:
            case 376:
            case 395:
            case 396:
            case 415:
            case 416:
            case 435:
            case 436:
            case 465:
            case 466:
            case 475:
            case 476:
            case 505:
            case 506:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 10);
                return true;
            case 553:
            case 554:
                skillChecks = new DialogSkillChecks(SkillId.bluff, 9);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}