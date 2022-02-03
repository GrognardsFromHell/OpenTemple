
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

[DialogScript(435)]
public class ThePostNPCDialog : ThePostNPC, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 472:
                originalScript = "game.quests[74].state == qs_completed and game.quests[109].state == qs_unknown and game.global_flags[550] == 0 and not npc_get(npc,1)";
                return GetQuestState(74) == QuestState.Completed && GetQuestState(109) == QuestState.Unknown && !GetGlobalFlag(550) && !ScriptDaemon.npc_get(npc, 1);
            case 473:
                originalScript = "(game.global_vars[949] >= 2 or game.party[0].reputation_has(47) == 1) and not npc_get(npc,2)";
                return (GetGlobalVar(949) >= 2 || PartyLeader.HasReputation(47)) && !ScriptDaemon.npc_get(npc, 2);
            case 474:
                originalScript = "game.quests[74].state == qs_completed and game.quests[78].state == qs_unknown and game.global_flags[935] == 0 and game.global_flags[992] == 0 and not npc_get(npc,3)";
                return GetQuestState(74) == QuestState.Completed && GetQuestState(78) == QuestState.Unknown && !GetGlobalFlag(935) && !GetGlobalFlag(992) && !ScriptDaemon.npc_get(npc, 3);
            case 475:
                originalScript = "game.quests[78].state == qs_completed and game.quests[107].state == qs_unknown and game.global_flags[935] == 0 and game.global_flags[992] == 0 and not npc_get(npc,4)";
                return GetQuestState(78) == QuestState.Completed && GetQuestState(107) == QuestState.Unknown && !GetGlobalFlag(935) && !GetGlobalFlag(992) && !ScriptDaemon.npc_get(npc, 4);
            case 476:
                originalScript = "game.global_flags[966] == 1 and game.global_vars[939] == 0 and game.global_flags[975] == 0 and game.global_flags[969] == 1 and not npc_get(npc,5)";
                return GetGlobalFlag(966) && GetGlobalVar(939) == 0 && !GetGlobalFlag(975) && GetGlobalFlag(969) && !ScriptDaemon.npc_get(npc, 5);
            case 477:
                originalScript = "((game.global_vars[510] == 1 or game.global_vars[510] == 2) or ((game.global_vars[501] == 4 or game.global_vars[501] == 5 or game.global_vars[501] == 6) and game.quests[97].state != qs_completed)) and not npc_get(npc,6)";
                return ((GetGlobalVar(510) == 1 || GetGlobalVar(510) == 2) || ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6) && GetQuestState(97) != QuestState.Completed)) && !ScriptDaemon.npc_get(npc, 6);
            case 478:
                originalScript = "game.global_flags[534] == 1";
                return GetGlobalFlag(534);
            case 479:
                originalScript = "(game.quests[79].state == qs_completed or game.quests[80].state == qs_completed or game.quests[81].state == qs_completed) and game.quests[106].state == qs_unknown and not npc_get(npc,7)";
                return (GetQuestState(79) == QuestState.Completed || GetQuestState(80) == QuestState.Completed || GetQuestState(81) == QuestState.Completed) && GetQuestState(106) == QuestState.Unknown && !ScriptDaemon.npc_get(npc, 7);
            case 480:
                originalScript = "game.global_flags[249] == 1 and game.quests[95].state != qs_completed and not npc_get(npc,8)";
                return GetGlobalFlag(249) && GetQuestState(95) != QuestState.Completed && !ScriptDaemon.npc_get(npc, 8);
            case 481:
                originalScript = "game.global_flags[962] == 0 and game.quests[95].state == qs_completed and game.quests[105].state == qs_unknown and not npc_get(npc,9)";
                return !GetGlobalFlag(962) && GetQuestState(95) == QuestState.Completed && GetQuestState(105) == QuestState.Unknown && !ScriptDaemon.npc_get(npc, 9);
            case 482:
                originalScript = "game.global_flags[962] == 1 and game.quests[95].state == qs_completed and game.quests[105].state == qs_unknown and not npc_get(npc,10)";
                return GetGlobalFlag(962) && GetQuestState(95) == QuestState.Completed && GetQuestState(105) == QuestState.Unknown && !ScriptDaemon.npc_get(npc, 10);
            case 1002:
                originalScript = "pc.map != 5066 and ( ( game.global_vars[491] & (2**5 + 2**6 + 2**15 ) ) != 0  or pc.reputation_has( 11 ) )";
                return pc.GetMap() != 5066 && ((GetGlobalVar(491) & (0x20 + 0x40 + 0x8000)) != 0 || pc.HasReputation(11));
            case 1003:
                originalScript = "pc.map != 5067 and ( ( game.global_vars[491] & (2**7 + 2**8 ) ) != 0  or pc.reputation_has( 10 ) or pc.reputation_has( 12 ) or pc.reputation_has( 13 ) )";
                return pc.GetMap() != 5067 && ((GetGlobalVar(491) & (0x80 + 0x100)) != 0 || pc.HasReputation(10) || pc.HasReputation(12) || pc.HasReputation(13));
            case 1004:
                originalScript = "pc.map != 5105 and ( game.global_vars[491] & (2**9 + 2**10 + 2**11 + 2**12) ) != 0";
                return pc.GetMap() != 5105 && (GetGlobalVar(491) & (0x200 + 0x400 + 0x800 + 0x1000)) != 0;
            case 1005:
                originalScript = "( (pc.map != 5080 and ( game.global_vars[491] & (2**13 + 2**14 + 2**21) ) != 0) + ( ( game.global_vars[491] & (2**16 + 2**17 + 2**18 + 2**19) ) != 0 ) + ( pc.map != 5079 and ( game.global_vars[491] & (2**20) ) != 0 ) ) >= 1";
                return ((pc.GetMap() != 5080 && (GetGlobalVar(491) & (0x2000 + 0x4000 + 0x200000)) != 0) || ((GetGlobalVar(491) & (0x10000 + 0x20000 + 0x40000 + 0x80000)) != 0) || (pc.GetMap() != 5079 && (GetGlobalVar(491) & (0x100000)) != 0));
            case 1012:
                originalScript = "pc.map != 5080 and ( game.global_vars[491] & (2**13 + 2**14 + 2**21) ) != 0";
                return pc.GetMap() != 5080 && (GetGlobalVar(491) & (0x2000 + 0x4000 + 0x200000)) != 0;
            case 1013:
                originalScript = "( game.global_vars[491] & (2**16 + 2**17 + 2**18 + 2**19) ) != 0";
                return (GetGlobalVar(491) & (0x10000 + 0x20000 + 0x40000 + 0x80000)) != 0;
            case 1014:
                originalScript = "pc.map != 5079 and ( game.global_vars[491] & (2**20) ) != 0";
                return pc.GetMap() != 5079 && (GetGlobalVar(491) & (0x100000)) != 0;
            case 1022:
                originalScript = "pc.map != 5064";
                return pc.GetMap() != 5064;
            case 1023:
                originalScript = "pc.map != 5111 and ( game.global_vars[491] & ( 2**1) ) != 0";
                return pc.GetMap() != 5111 && (GetGlobalVar(491) & (0x2)) != 0;
            case 1024:
                originalScript = "pc.map != 5065 and ( game.global_vars[491] & ( 2**2) ) != 0";
                return pc.GetMap() != 5065 && (GetGlobalVar(491) & (0x4)) != 0;
            case 1025:
                originalScript = "( ( pc.map != 5064 ) + ( pc.map != 5111 and ( game.global_vars[491] & ( 2**1) ) != 0 ) + ( pc.map != 5065 and ( game.global_vars[491] & ( 2**2) ) != 0 ) + ( pc.map != 5112 and ( game.global_vars[491] & ( 2**4) ) != 0 ) + ( pc.map != 5092 and ( game.global_vars[491] & ( 2**3) ) != 0 ) + ( pc.map != 5110 and ( game.global_vars[491] & ( 2**0)) ) ) >= 5";
                return ((pc.GetMap() != 5064) && (pc.GetMap() != 5111 && (GetGlobalVar(491) & (0x2)) != 0) && (pc.GetMap() != 5065 && (GetGlobalVar(491) & (0x4)) != 0) && (pc.GetMap() != 5112 && (GetGlobalVar(491) & (0x10)) != 0) && (pc.GetMap() != 5092 && (GetGlobalVar(491) & (0x8)) != 0) && (pc.GetMap() != 5110 && ((GetGlobalVar(491) & (0x1))) != 0));
            case 1026:
            case 1032:
                originalScript = "pc.map != 5112 and ( game.global_vars[491] & ( 2**4) ) != 0";
                return pc.GetMap() != 5112 && (GetGlobalVar(491) & (0x10)) != 0;
            case 1027:
            case 1033:
                originalScript = "pc.map != 5092 and ( game.global_vars[491] & ( 2**3) ) != 0";
                return pc.GetMap() != 5092 && (GetGlobalVar(491) & (0x8)) != 0;
            case 1028:
            case 1034:
                originalScript = "pc.map != 5110 and ( game.global_vars[491] & ( 2**0) ) != 0";
                return pc.GetMap() != 5110 && (GetGlobalVar(491) & (0x1)) != 0;
            case 1042:
                originalScript = "( game.global_vars[491] & ( 2**5) ) != 0";
                return (GetGlobalVar(491) & (0x20)) != 0;
            case 1043:
                originalScript = "( game.global_vars[491] & ( 2**6) ) != 0";
                return (GetGlobalVar(491) & (0x40)) != 0;
            case 1044:
                originalScript = "pc.reputation_has( 11 )";
                return pc.HasReputation(11);
            case 1045:
                originalScript = "( game.global_vars[491] & ( 2**15) ) != 0";
                return (GetGlobalVar(491) & (0x8000)) != 0;
            case 1062:
                originalScript = "( game.global_vars[491] & ( 2**7) ) != 0";
                return (GetGlobalVar(491) & (0x80)) != 0;
            case 1063:
                originalScript = "( game.global_vars[491] & ( 2**8) ) != 0";
                return (GetGlobalVar(491) & (0x100)) != 0;
            case 1064:
                originalScript = "pc.reputation_has( 13 )";
                return pc.HasReputation(13);
            case 1065:
                originalScript = "(  ( ( game.global_vars[491] & ( 2**7) ) != 0 ) + ( ( game.global_vars[491] & ( 2**8) ) != 0 ) + ( pc.reputation_has( 13 ) ) + ( pc.reputation_has( 12 ) ) + ( pc.reputation_has( 10 ) )  ) >= 5";
                return (((GetGlobalVar(491) & (0x80)) != 0) && ((GetGlobalVar(491) & (0x100)) != 0) && (pc.HasReputation(13)) && (pc.HasReputation(12)) && (pc.HasReputation(10)));
            case 1066:
            case 1072:
                originalScript = "pc.reputation_has( 12 )";
                return pc.HasReputation(12);
            case 1067:
            case 1073:
                originalScript = "pc.reputation_has( 10 )";
                return pc.HasReputation(10);
            case 1082:
                originalScript = "( game.global_vars[491] & ( 2**9 ) ) != 0";
                return (GetGlobalVar(491) & (0x200)) != 0;
            case 1083:
                originalScript = "( game.global_vars[491] & ( 2**10) ) != 0";
                return (GetGlobalVar(491) & (0x400)) != 0;
            case 1084:
                originalScript = "( game.global_vars[491] & ( 2**11) ) != 0";
                return (GetGlobalVar(491) & (0x800)) != 0;
            case 1085:
                originalScript = "(  ( ( game.global_vars[491] & ( 2**9) ) != 0 ) + ( ( game.global_vars[491] & ( 2**10) ) != 0 ) + ( ( game.global_vars[491] & ( 2**11) ) != 0 ) + ( ( game.global_vars[491] & ( 2**12) ) != 0 ) + ( ( game.global_vars[491] & ( 2**22) ) != 0 )  ) >= 5";
                return (((GetGlobalVar(491) & (0x200)) != 0) && ((GetGlobalVar(491) & (0x400)) != 0) && ((GetGlobalVar(491) & (0x800)) != 0) && ((GetGlobalVar(491) & (0x1000)) != 0) && ((GetGlobalVar(491) & (0x400000)) != 0));
            case 1086:
            case 1092:
                originalScript = "( game.global_vars[491] & ( 2**12) ) != 0";
                return (GetGlobalVar(491) & (0x1000)) != 0;
            case 1087:
            case 1093:
                originalScript = "( game.global_vars[491] & ( 2**22) ) != 0 and not ( game.global_flags[194] == 1 or game.global_flags[182] == 1)";
                return (GetGlobalVar(491) & (0x400000)) != 0 && !(GetGlobalFlag(194) || GetGlobalFlag(182));
            case 1088:
                originalScript = "( game.global_vars[491] & ( 2**22) ) != 0 and ( game.global_flags[194] == 1 or game.global_flags[182] == 1)";
                return (GetGlobalVar(491) & (0x400000)) != 0 && (GetGlobalFlag(194) || GetGlobalFlag(182));
            case 1094:
                originalScript = "( game.global_vars[491] & ( 2**22) ) != 0 and( game.global_flags[194] == 1 or game.global_flags[182] == 1)";
                return (GetGlobalVar(491) & (0x400000)) != 0 && (GetGlobalFlag(194) || GetGlobalFlag(182));
            case 1102:
                originalScript = "( game.global_vars[491] & (2**13 ) ) != 0";
                return (GetGlobalVar(491) & (0x2000)) != 0;
            case 1103:
                originalScript = "( game.global_vars[491] & (2**14 ) ) != 0";
                return (GetGlobalVar(491) & (0x4000)) != 0;
            case 1104:
                originalScript = "( game.global_vars[491] & (2**21 ) ) != 0";
                return (GetGlobalVar(491) & (0x200000)) != 0;
            case 1122:
                originalScript = "( game.global_vars[491] & ( 2**18) ) != 0";
                return (GetGlobalVar(491) & (0x40000)) != 0;
            case 1123:
                originalScript = "( game.global_vars[491] & ( 2**19) ) != 0";
                return (GetGlobalVar(491) & (0x80000)) != 0;
            case 1124:
                originalScript = "( game.global_vars[491] & ( 2**17) ) != 0";
                return (GetGlobalVar(491) & (0x20000)) != 0;
            case 1125:
                originalScript = "( game.global_vars[491] & ( 2**16) ) != 0";
                return (GetGlobalVar(491) & (0x10000)) != 0;
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
                originalScript = "game.fade_and_teleport(300,0,0,5042,492,475)";
                FadeAndTeleport(300, 0, 0, 5042, 492, 475);
                break;
            case 3:
                originalScript = "tele2(pc,5001,515,461)";
                tele2(pc, 5001, 515, 461);
                break;
            case 4:
                originalScript = "tele2(pc,5001,575,441)";
                tele2(pc, 5001, 575, 441);
                break;
            case 5:
                originalScript = "tele2(pc,5001,537,389)";
                tele2(pc, 5001, 537, 389);
                break;
            case 12:
                originalScript = "game.fade_and_teleport(300,0,0,5012,492,485)";
                FadeAndTeleport(300, 0, 0, 5012, 492, 485);
                break;
            case 13:
                originalScript = "tele2(pc,5001,527,330)";
                tele2(pc, 5001, 527, 330);
                break;
            case 14:
                originalScript = "tele2(pc,5001,570,285)";
                tele2(pc, 5001, 570, 285);
                break;
            case 22:
                originalScript = "game.fade_and_teleport(300,0,0,5016,479,485)";
                FadeAndTeleport(300, 0, 0, 5016, 479, 485);
                break;
            case 23:
                originalScript = "tele2(pc,5001,438,699)";
                tele2(pc, 5001, 438, 699);
                break;
            case 24:
                originalScript = "tele2(pc,5001,414,537)";
                tele2(pc, 5001, 414, 537);
                break;
            case 32:
                originalScript = "tele2(pc,5001,764,434)";
                tele2(pc, 5001, 764, 434);
                break;
            case 33:
                originalScript = "tele2(pc,5001,728,392)";
                tele2(pc, 5001, 728, 392);
                break;
            case 34:
                originalScript = "tele2(pc,5001,706,505)";
                tele2(pc, 5001, 706, 505);
                break;
            case 42:
                originalScript = "tele2(pc,5001,645,329)";
                tele2(pc, 5001, 645, 329);
                break;
            case 43:
                originalScript = "tele2(pc,5001,451,334)";
                tele2(pc, 5001, 451, 334);
                break;
            case 44:
                originalScript = "tele2(pc,5001,442,398)";
                tele2(pc, 5001, 442, 398);
                break;
            case 201:
                originalScript = "tele2(pc,5051,479,471)";
                tele2(pc, 5051, 479, 471);
                break;
            case 202:
                originalScript = "tele2(pc,5051,383,484)";
                tele2(pc, 5051, 383, 484);
                break;
            case 203:
                originalScript = "tele2(pc,5051,495,518)";
                tele2(pc, 5051, 495, 518);
                break;
            case 204:
                originalScript = "tele2(pc,5051,488,577)";
                tele2(pc, 5051, 488, 577);
                break;
            case 212:
                originalScript = "tele2(pc,5051,560,470)";
                tele2(pc, 5051, 560, 470);
                break;
            case 213:
                originalScript = "tele2(pc,5051,398,521)";
                tele2(pc, 5051, 398, 521);
                break;
            case 214:
                originalScript = "tele2(pc,5051,556,545)";
                tele2(pc, 5051, 556, 545);
                break;
            case 402:
                originalScript = "game.fade_and_teleport(300,0,0,5169,490,501)";
                FadeAndTeleport(300, 0, 0, 5169, 490, 501);
                break;
            case 403:
                originalScript = "game.fade_and_teleport(300,0,0,5170,501,484)";
                FadeAndTeleport(300, 0, 0, 5170, 501, 484);
                break;
            case 404:
                originalScript = "tele2(pc,5121,140,525)";
                tele2(pc, 5121, 140, 525);
                break;
            case 412:
                originalScript = "tele2(pc,5121,680,455)";
                tele2(pc, 5121, 680, 455);
                break;
            case 413:
            case 434:
                originalScript = "tele2(pc,5121,254,577)";
                tele2(pc, 5121, 254, 577);
                break;
            case 414:
                originalScript = "tele2(pc,5121,459,728)";
                tele2(pc, 5121, 459, 728);
                break;
            case 422:
                originalScript = "tele2(pc,5121,491,211)";
                tele2(pc, 5121, 491, 211);
                break;
            case 423:
                originalScript = "tele2(pc,5121,485,376)";
                tele2(pc, 5121, 485, 376);
                break;
            case 424:
                originalScript = "tele2(pc,5121,579,309)";
                tele2(pc, 5121, 579, 309);
                break;
            case 432:
                originalScript = "tele2(pc,5121,533,560)";
                tele2(pc, 5121, 533, 560);
                break;
            case 433:
                originalScript = "tele2(pc,5121,306,489)";
                tele2(pc, 5121, 306, 489);
                break;
            case 442:
                originalScript = "tele2(pc,5121,449,562)";
                tele2(pc, 5121, 449, 562);
                break;
            case 443:
                originalScript = "tele2(pc,5121,413,295)";
                tele2(pc, 5121, 413, 295);
                break;
            case 444:
                originalScript = "tele2(pc,5121,448,653)";
                tele2(pc, 5121, 448, 653);
                break;
            case 452:
                originalScript = "tele2(pc,5121,570,388)";
                tele2(pc, 5121, 570, 388);
                break;
            case 453:
                originalScript = "tele2(pc,5121,487,404)";
                tele2(pc, 5121, 487, 404);
                break;
            case 454:
                originalScript = "tele2(pc,5121,410,382)";
                tele2(pc, 5121, 410, 382);
                break;
            case 462:
                originalScript = "tele2(pc,5121,310,449)";
                tele2(pc, 5121, 310, 449);
                break;
            case 463:
                originalScript = "tele2(pc,5121,513,626)";
                tele2(pc, 5121, 513, 626);
                break;
            case 464:
                originalScript = "tele2(pc,5121,614,589)";
                tele2(pc, 5121, 614, 589);
                break;
            case 472:
                originalScript = "create_item_in_inventory(11063,pc); npc_set(npc,1)";
                Utilities.create_item_in_inventory(11063, pc);
                ScriptDaemon.npc_set(npc, 1);
                ;
                break;
            case 473:
                originalScript = "create_item_in_inventory(11064,pc); npc_set(npc,2)";
                Utilities.create_item_in_inventory(11064, pc);
                ScriptDaemon.npc_set(npc, 2);
                ;
                break;
            case 474:
                originalScript = "create_item_in_inventory(11065,pc); npc_set(npc,3)";
                Utilities.create_item_in_inventory(11065, pc);
                ScriptDaemon.npc_set(npc, 3);
                ;
                break;
            case 475:
                originalScript = "create_item_in_inventory(11066,pc); npc_set(npc,4)";
                Utilities.create_item_in_inventory(11066, pc);
                ScriptDaemon.npc_set(npc, 4);
                ;
                break;
            case 476:
                originalScript = "create_item_in_inventory(11067,pc); npc_set(npc,5)";
                Utilities.create_item_in_inventory(11067, pc);
                ScriptDaemon.npc_set(npc, 5);
                ;
                break;
            case 477:
                originalScript = "create_item_in_inventory(11068,pc); npc_set(npc,6)";
                Utilities.create_item_in_inventory(11068, pc);
                ScriptDaemon.npc_set(npc, 6);
                ;
                break;
            case 478:
                originalScript = "create_item_in_inventory(11069,pc); game.global_flags[534] = 0";
                Utilities.create_item_in_inventory(11069, pc);
                SetGlobalFlag(534, false);
                ;
                break;
            case 479:
                originalScript = "create_item_in_inventory(11070,pc); npc_set(npc,7)";
                Utilities.create_item_in_inventory(11070, pc);
                ScriptDaemon.npc_set(npc, 7);
                ;
                break;
            case 480:
                originalScript = "create_item_in_inventory(11071,pc); npc_set(npc,8)";
                Utilities.create_item_in_inventory(11071, pc);
                ScriptDaemon.npc_set(npc, 8);
                ;
                break;
            case 481:
                originalScript = "create_item_in_inventory(11072,pc); npc_set(npc,9)";
                Utilities.create_item_in_inventory(11072, pc);
                ScriptDaemon.npc_set(npc, 9);
                ;
                break;
            case 482:
                originalScript = "create_item_in_inventory(11073,pc); npc_set(npc,10)";
                Utilities.create_item_in_inventory(11073, pc);
                ScriptDaemon.npc_set(npc, 10);
                ;
                break;
            case 483:
                originalScript = "create_item_in_inventory(11074,pc)";
                Utilities.create_item_in_inventory(11074, pc);
                break;
            case 1000:
            case 1200:
            case 1210:
                originalScript = "pc.scripts[9] = 0";
                pc.RemoveScript(ObjScriptEvent.Dialog);
                break;
            case 1014:
                originalScript = "game.fade_and_teleport(300,0,0,5079,488,503)";
                FadeAndTeleport(300, 0, 0, 5079, 488, 503);
                break;
            case 1022:
                originalScript = "game.fade_and_teleport(300,0,0,5064,489,567)";
                FadeAndTeleport(300, 0, 0, 5064, 489, 567);
                break;
            case 1023:
                originalScript = "game.fade_and_teleport(300,0,0,5111,494,544)";
                FadeAndTeleport(300, 0, 0, 5111, 494, 544);
                break;
            case 1024:
                originalScript = "game.fade_and_teleport(300,0,0,5065,486,494)";
                FadeAndTeleport(300, 0, 0, 5065, 486, 494);
                break;
            case 1026:
            case 1032:
                originalScript = "game.fade_and_teleport(300,0,0,5112,495,477)";
                FadeAndTeleport(300, 0, 0, 5112, 495, 477);
                break;
            case 1027:
            case 1033:
                originalScript = "game.fade_and_teleport(300,0,0,5092,469,480)";
                FadeAndTeleport(300, 0, 0, 5092, 469, 480);
                break;
            case 1028:
            case 1034:
                originalScript = "game.fade_and_teleport(300,0,0,5110,466,528)";
                FadeAndTeleport(300, 0, 0, 5110, 466, 528);
                break;
            case 1042:
                originalScript = "game.fade_and_teleport(300,0,0,5066,421,589)";
                FadeAndTeleport(300, 0, 0, 5066, 421, 589);
                break;
            case 1043:
                originalScript = "game.fade_and_teleport(300,0,0,5066,547,588)";
                FadeAndTeleport(300, 0, 0, 5066, 547, 588);
                break;
            case 1044:
                originalScript = "game.fade_and_teleport(300,0,0,5066,444,443)";
                FadeAndTeleport(300, 0, 0, 5066, 444, 443);
                break;
            case 1045:
                originalScript = "game.fade_and_teleport(300,0,0,5106,480,473)";
                FadeAndTeleport(300, 0, 0, 5106, 480, 473);
                break;
            case 1062:
                originalScript = "game.fade_and_teleport(300,0,0,5067,564,377)";
                FadeAndTeleport(300, 0, 0, 5067, 564, 377);
                break;
            case 1063:
                originalScript = "game.fade_and_teleport(300,0,0,5067,485,557)";
                FadeAndTeleport(300, 0, 0, 5067, 485, 557);
                break;
            case 1064:
                originalScript = "game.fade_and_teleport(300,0,0,5067,421,520)";
                FadeAndTeleport(300, 0, 0, 5067, 421, 520);
                break;
            case 1066:
            case 1072:
                originalScript = "game.fade_and_teleport(300,0,0,5067,529,540)";
                FadeAndTeleport(300, 0, 0, 5067, 529, 540);
                break;
            case 1067:
            case 1073:
                originalScript = "game.fade_and_teleport(300,0,0,5067,524,484)";
                FadeAndTeleport(300, 0, 0, 5067, 524, 484);
                break;
            case 1082:
                originalScript = "game.fade_and_teleport(300,0,0,5105,406,436)";
                FadeAndTeleport(300, 0, 0, 5105, 406, 436);
                break;
            case 1083:
                originalScript = "game.fade_and_teleport(300,0,0,5105,517,518)";
                FadeAndTeleport(300, 0, 0, 5105, 517, 518);
                break;
            case 1084:
                originalScript = "game.fade_and_teleport(300,0,0,5105,616,606)";
                FadeAndTeleport(300, 0, 0, 5105, 616, 606);
                break;
            case 1086:
            case 1092:
                originalScript = "game.fade_and_teleport(300,0,0,5105,639,445)";
                FadeAndTeleport(300, 0, 0, 5105, 639, 445);
                break;
            case 1087:
            case 1088:
            case 1093:
            case 1094:
                originalScript = "game.fade_and_teleport(300,0,0,5105,552,489)";
                FadeAndTeleport(300, 0, 0, 5105, 552, 489);
                break;
            case 1102:
                originalScript = "game.fade_and_teleport(300,0,0,5080,479,586)";
                FadeAndTeleport(300, 0, 0, 5080, 479, 586);
                break;
            case 1103:
                originalScript = "game.fade_and_teleport(300,0,0,5080,477,352)";
                FadeAndTeleport(300, 0, 0, 5080, 477, 352);
                break;
            case 1104:
                originalScript = "game.fade_and_teleport(300,0,0,5080,479,451)";
                FadeAndTeleport(300, 0, 0, 5080, 479, 451);
                break;
            case 1122:
                originalScript = "game.fade_and_teleport(300,0,0,5083,504,495)";
                FadeAndTeleport(300, 0, 0, 5083, 504, 495);
                break;
            case 1123:
                originalScript = "game.fade_and_teleport(300,0,0,5084,452,472)";
                FadeAndTeleport(300, 0, 0, 5084, 452, 472);
                break;
            case 1124:
                originalScript = "game.fade_and_teleport(300,0,0,5082,483,472)";
                FadeAndTeleport(300, 0, 0, 5082, 483, 472);
                break;
            case 1125:
                originalScript = "game.fade_and_teleport(300,0,0,5081,476,483)";
                FadeAndTeleport(300, 0, 0, 5081, 476, 483);
                break;
            case 3001:
                originalScript = "npc.destroy()";
                npc.Destroy();
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