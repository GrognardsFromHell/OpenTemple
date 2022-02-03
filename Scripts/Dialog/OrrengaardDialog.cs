
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

[DialogScript(399)]
public class OrrengaardDialog : Orrengaard, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 12:
            case 32:
            case 52:
            case 72:
            case 92:
            case 112:
            case 132:
            case 152:
            case 172:
            case 192:
            case 212:
            case 232:
            case 282:
            case 302:
            case 322:
                originalScript = "not npc_get(npc,1)";
                return !ScriptDaemon.npc_get(npc, 1);
            case 13:
            case 33:
            case 53:
            case 73:
            case 93:
            case 113:
            case 133:
            case 153:
            case 173:
            case 193:
            case 213:
            case 233:
            case 283:
            case 303:
            case 323:
                originalScript = "not npc_get(npc,2)";
                return !ScriptDaemon.npc_get(npc, 2);
            case 14:
            case 34:
            case 54:
            case 74:
            case 94:
            case 114:
            case 134:
            case 154:
            case 174:
            case 194:
            case 214:
            case 234:
            case 284:
            case 304:
            case 324:
                originalScript = "not npc_get(npc,3)";
                return !ScriptDaemon.npc_get(npc, 3);
            case 15:
            case 35:
            case 55:
            case 75:
            case 95:
            case 115:
            case 135:
            case 155:
            case 175:
            case 195:
            case 215:
            case 235:
            case 285:
            case 305:
            case 325:
                originalScript = "not npc_get(npc,4)";
                return !ScriptDaemon.npc_get(npc, 4);
            case 16:
            case 36:
            case 56:
            case 76:
            case 96:
            case 116:
            case 136:
            case 156:
            case 176:
            case 196:
            case 216:
            case 236:
            case 286:
            case 306:
            case 326:
                originalScript = "not npc_get(npc,5)";
                return !ScriptDaemon.npc_get(npc, 5);
            case 17:
            case 37:
            case 57:
            case 77:
            case 97:
            case 117:
            case 137:
            case 157:
            case 177:
            case 197:
            case 217:
            case 237:
            case 287:
            case 307:
            case 327:
                originalScript = "not npc_get(npc,6)";
                return !ScriptDaemon.npc_get(npc, 6);
            case 18:
            case 38:
            case 58:
            case 78:
            case 98:
            case 118:
            case 138:
            case 158:
            case 178:
            case 198:
            case 218:
            case 238:
            case 288:
            case 308:
            case 328:
                originalScript = "not npc_get(npc,7)";
                return !ScriptDaemon.npc_get(npc, 7);
            case 19:
            case 39:
            case 59:
            case 79:
            case 99:
            case 119:
            case 139:
            case 159:
            case 179:
            case 199:
            case 219:
            case 239:
            case 289:
            case 309:
            case 329:
                originalScript = "not npc_get(npc,8)";
                return !ScriptDaemon.npc_get(npc, 8);
            case 20:
            case 40:
            case 60:
            case 80:
            case 100:
            case 120:
            case 140:
            case 160:
            case 180:
            case 200:
            case 220:
            case 240:
            case 290:
            case 310:
            case 330:
                originalScript = "not npc_get(npc,9)";
                return !ScriptDaemon.npc_get(npc, 9);
            case 21:
            case 41:
            case 61:
            case 81:
            case 101:
            case 121:
            case 141:
            case 161:
            case 181:
            case 201:
            case 221:
            case 241:
            case 291:
            case 311:
            case 331:
                originalScript = "not npc_get(npc,10)";
                return !ScriptDaemon.npc_get(npc, 10);
            case 23:
            case 43:
            case 63:
            case 83:
            case 103:
            case 123:
            case 143:
            case 163:
            case 183:
            case 203:
            case 223:
            case 243:
            case 293:
            case 313:
            case 333:
                originalScript = "not npc_get(npc,12)";
                return !ScriptDaemon.npc_get(npc, 12);
            case 24:
            case 44:
            case 64:
            case 84:
            case 104:
            case 124:
            case 144:
            case 164:
            case 184:
            case 204:
            case 224:
            case 244:
            case 294:
            case 314:
            case 334:
                originalScript = "npc_get(npc,12) and not npc_get(npc,13)";
                return ScriptDaemon.npc_get(npc, 12) && !ScriptDaemon.npc_get(npc, 13);
            case 25:
            case 45:
            case 65:
            case 85:
            case 105:
            case 125:
            case 145:
            case 165:
            case 185:
            case 205:
            case 225:
            case 245:
            case 295:
            case 315:
            case 335:
                originalScript = "game.global_vars[938] == 1 and not npc_get(npc,14)";
                return GetGlobalVar(938) == 1 && !ScriptDaemon.npc_get(npc, 14);
            case 26:
            case 46:
            case 66:
            case 86:
            case 106:
            case 126:
            case 146:
            case 166:
            case 186:
            case 206:
            case 226:
            case 246:
            case 296:
            case 316:
            case 336:
                originalScript = "npc_get(npc,10)";
                return ScriptDaemon.npc_get(npc, 10);
            case 251:
                originalScript = "not npc_get(npc,11)";
                return !ScriptDaemon.npc_get(npc, 11);
            case 362:
                originalScript = "game.global_vars[101] == 1";
                return GetGlobalVar(101) == 1;
            case 363:
                originalScript = "game.global_vars[102] == 1";
                return GetGlobalVar(102) == 1;
            case 364:
                originalScript = "game.global_vars[103] == 1";
                return GetGlobalVar(103) == 1;
            case 372:
                originalScript = "game.global_vars[104] == 1";
                return GetGlobalVar(104) == 1;
            case 373:
                originalScript = "game.global_vars[105] == 1";
                return GetGlobalVar(105) == 1;
            case 374:
                originalScript = "game.global_vars[106] == 1";
                return GetGlobalVar(106) == 1;
            case 382:
                originalScript = "game.global_vars[107] == 1";
                return GetGlobalVar(107) == 1;
            case 383:
                originalScript = "game.global_vars[108] == 1";
                return GetGlobalVar(108) == 1;
            case 384:
                originalScript = "game.global_vars[109] == 1";
                return GetGlobalVar(109) == 1;
            case 392:
                originalScript = "game.global_vars[110] == 1";
                return GetGlobalVar(110) == 1;
            case 393:
                originalScript = "game.global_vars[111] == 1";
                return GetGlobalVar(111) == 1;
            case 394:
                originalScript = "game.global_vars[112] == 1";
                return GetGlobalVar(112) == 1;
            case 402:
                originalScript = "game.global_vars[113] == 1";
                return GetGlobalVar(113) == 1;
            case 403:
                originalScript = "game.global_vars[114] == 1";
                return GetGlobalVar(114) == 1;
            case 404:
                originalScript = "game.global_vars[115] == 1";
                return GetGlobalVar(115) == 1;
            case 411:
                originalScript = "game.global_vars[116] == 1";
                return GetGlobalVar(116) == 1;
            case 412:
                originalScript = "game.global_vars[117] == 1";
                return GetGlobalVar(117) == 1;
            case 413:
                originalScript = "game.global_vars[118] == 1";
                return GetGlobalVar(118) == 1;
            case 414:
                originalScript = "game.global_vars[146] == 1";
                return GetGlobalVar(146) == 1;
            case 422:
                originalScript = "game.global_vars[119] == 1";
                return GetGlobalVar(119) == 1;
            case 423:
                originalScript = "game.global_vars[120] == 1";
                return GetGlobalVar(120) == 1;
            case 424:
                originalScript = "game.global_vars[121] == 1";
                return GetGlobalVar(121) == 1;
            case 432:
                originalScript = "game.global_vars[122] == 1";
                return GetGlobalVar(122) == 1;
            case 433:
                originalScript = "game.global_vars[123] == 1";
                return GetGlobalVar(123) == 1;
            case 434:
                originalScript = "game.global_vars[124] == 1";
                return GetGlobalVar(124) == 1;
            case 442:
                originalScript = "game.global_vars[125] == 1";
                return GetGlobalVar(125) == 1;
            case 443:
                originalScript = "game.global_vars[126] == 1";
                return GetGlobalVar(126) == 1;
            case 444:
                originalScript = "game.global_vars[127] == 1";
                return GetGlobalVar(127) == 1;
            case 452:
                originalScript = "game.global_vars[128] == 1";
                return GetGlobalVar(128) == 1;
            case 453:
                originalScript = "game.global_vars[129] == 1";
                return GetGlobalVar(129) == 1;
            case 454:
                originalScript = "game.global_vars[130] == 1";
                return GetGlobalVar(130) == 1;
            case 462:
                originalScript = "game.global_vars[131] == 1";
                return GetGlobalVar(131) == 1;
            case 463:
                originalScript = "game.global_vars[132] == 1";
                return GetGlobalVar(132) == 1;
            case 464:
                originalScript = "game.global_vars[133] == 1";
                return GetGlobalVar(133) == 1;
            case 472:
                originalScript = "game.global_vars[134] == 1";
                return GetGlobalVar(134) == 1;
            case 473:
                originalScript = "game.global_vars[135] == 1";
                return GetGlobalVar(135) == 1;
            case 474:
                originalScript = "game.global_vars[136] == 1";
                return GetGlobalVar(136) == 1;
            case 482:
                originalScript = "game.global_vars[137] == 1";
                return GetGlobalVar(137) == 1;
            case 483:
                originalScript = "game.global_vars[138] == 1";
                return GetGlobalVar(138) == 1;
            case 484:
                originalScript = "game.global_vars[139] == 1";
                return GetGlobalVar(139) == 1;
            case 492:
                originalScript = "game.global_vars[140] == 1";
                return GetGlobalVar(140) == 1;
            case 493:
                originalScript = "game.global_vars[141] == 1";
                return GetGlobalVar(141) == 1;
            case 501:
                originalScript = "game.global_vars[145] == 1";
                return GetGlobalVar(145) == 1;
            case 502:
                originalScript = "game.global_vars[142] == 1";
                return GetGlobalVar(142) == 1;
            case 503:
                originalScript = "game.global_vars[143] == 1";
                return GetGlobalVar(143) == 1;
            case 504:
                originalScript = "game.global_vars[144] == 1";
                return GetGlobalVar(144) == 1;
            case 521:
                originalScript = "(game.quests[109].state == qs_mentioned or game.quests[109].state == qs_accepted) and not npc_get(npc,21)";
                return (GetQuestState(109) == QuestState.Mentioned || GetQuestState(109) == QuestState.Accepted) && !ScriptDaemon.npc_get(npc, 21);
            case 522:
                originalScript = "game.quests[78].state == qs_accepted and not npc_get(npc,15)";
                return GetQuestState(78) == QuestState.Accepted && !ScriptDaemon.npc_get(npc, 15);
            case 523:
                originalScript = "game.global_flags[869] == 1 and not npc_get(npc,16)";
                return GetGlobalFlag(869) && !ScriptDaemon.npc_get(npc, 16);
            case 524:
                originalScript = "game.quests[90].state == qs_accepted and not npc_get(npc,17)";
                return GetQuestState(90) == QuestState.Accepted && !ScriptDaemon.npc_get(npc, 17);
            case 532:
                originalScript = "not anyone( pc.group_list(), \"has_follower\", 8716 ) and game.quests[79].state == qs_accepted and not npc_get(npc,18)";
                return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8716)) && GetQuestState(79) == QuestState.Accepted && !ScriptDaemon.npc_get(npc, 18);
            case 533:
                originalScript = "not anyone( pc.group_list(), \"has_follower\", 8717 ) and game.quests[80].state == qs_accepted and not npc_get(npc,19)";
                return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8717)) && GetQuestState(80) == QuestState.Accepted && !ScriptDaemon.npc_get(npc, 19);
            case 534:
                originalScript = "not anyone( pc.group_list(), \"has_follower\", 8718 ) and game.quests[81].state == qs_accepted and not npc_get(npc,20)";
                return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8718)) && GetQuestState(81) == QuestState.Accepted && !ScriptDaemon.npc_get(npc, 20);
            case 541:
                originalScript = "pc.money_get() >= 1000000";
                return pc.GetMoney() >= 1000000;
            case 551:
            case 1631:
                originalScript = "pc.money_get() >= 400000";
                return pc.GetMoney() >= 400000;
            case 561:
                originalScript = "pc.money_get() >= 800000";
                return pc.GetMoney() >= 800000;
            case 571:
            case 581:
            case 591:
                originalScript = "pc.money_get() >= 600000";
                return pc.GetMoney() >= 600000;
            case 661:
            case 671:
            case 681:
            case 691:
            case 701:
            case 711:
            case 721:
            case 731:
            case 741:
            case 751:
            case 761:
            case 771:
            case 781:
            case 791:
            case 801:
            case 811:
            case 821:
            case 831:
            case 841:
            case 851:
            case 861:
            case 871:
            case 881:
            case 891:
            case 901:
            case 911:
            case 921:
            case 931:
            case 941:
            case 951:
            case 961:
            case 971:
            case 981:
            case 991:
            case 1001:
            case 1011:
            case 1021:
            case 1031:
            case 1041:
            case 1051:
            case 1061:
            case 1071:
            case 1081:
            case 1091:
            case 1101:
            case 1621:
                originalScript = "pc.money_get() >= 100000";
                return pc.GetMoney() >= 100000;
            case 892:
                originalScript = "pc.money_get() <= 99900";
                return pc.GetMoney() <= 99900;
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
                originalScript = "game.global_flags[886] = 1";
                SetGlobalFlag(886, true);
                break;
            case 12:
            case 32:
            case 52:
            case 72:
            case 92:
            case 112:
            case 132:
            case 152:
            case 172:
            case 192:
            case 212:
            case 232:
            case 282:
            case 302:
            case 322:
                originalScript = "npc_set(npc,1)";
                ScriptDaemon.npc_set(npc, 1);
                break;
            case 13:
            case 33:
            case 53:
            case 73:
            case 93:
            case 113:
            case 133:
            case 153:
            case 173:
            case 193:
            case 213:
            case 233:
            case 283:
            case 303:
            case 323:
                originalScript = "npc_set(npc,2)";
                ScriptDaemon.npc_set(npc, 2);
                break;
            case 14:
            case 34:
            case 54:
            case 74:
            case 94:
            case 114:
            case 134:
            case 154:
            case 174:
            case 194:
            case 214:
            case 234:
            case 284:
            case 304:
            case 324:
                originalScript = "npc_set(npc,3)";
                ScriptDaemon.npc_set(npc, 3);
                break;
            case 15:
            case 35:
            case 55:
            case 75:
            case 95:
            case 115:
            case 135:
            case 155:
            case 175:
            case 195:
            case 215:
            case 235:
            case 285:
            case 305:
            case 325:
                originalScript = "npc_set(npc,4)";
                ScriptDaemon.npc_set(npc, 4);
                break;
            case 16:
            case 36:
            case 56:
            case 76:
            case 96:
            case 116:
            case 136:
            case 156:
            case 176:
            case 196:
            case 216:
            case 236:
            case 286:
            case 306:
            case 326:
                originalScript = "npc_set(npc,5)";
                ScriptDaemon.npc_set(npc, 5);
                break;
            case 17:
            case 37:
            case 57:
            case 77:
            case 97:
            case 117:
            case 137:
            case 157:
            case 177:
            case 197:
            case 217:
            case 237:
            case 287:
            case 307:
            case 327:
                originalScript = "npc_set(npc,6)";
                ScriptDaemon.npc_set(npc, 6);
                break;
            case 18:
            case 38:
            case 58:
            case 78:
            case 98:
            case 118:
            case 138:
            case 158:
            case 178:
            case 198:
            case 218:
            case 238:
            case 288:
            case 308:
            case 328:
                originalScript = "npc_set(npc,7)";
                ScriptDaemon.npc_set(npc, 7);
                break;
            case 19:
            case 39:
            case 59:
            case 79:
            case 99:
            case 119:
            case 139:
            case 159:
            case 179:
            case 199:
            case 219:
            case 239:
            case 289:
            case 309:
            case 329:
                originalScript = "npc_set(npc,8)";
                ScriptDaemon.npc_set(npc, 8);
                break;
            case 20:
            case 40:
            case 60:
            case 80:
            case 100:
            case 120:
            case 140:
            case 160:
            case 180:
            case 200:
            case 220:
            case 240:
            case 290:
            case 310:
            case 330:
                originalScript = "npc_set(npc,9)";
                ScriptDaemon.npc_set(npc, 9);
                break;
            case 21:
            case 41:
            case 61:
            case 81:
            case 101:
            case 121:
            case 141:
            case 161:
            case 181:
            case 201:
            case 221:
            case 241:
            case 291:
            case 311:
            case 331:
                originalScript = "npc_set(npc,10)";
                ScriptDaemon.npc_set(npc, 10);
                break;
            case 23:
            case 43:
            case 63:
            case 83:
            case 103:
            case 123:
            case 143:
            case 163:
            case 183:
            case 203:
            case 223:
            case 243:
            case 293:
            case 313:
            case 333:
                originalScript = "npc_set(npc,12)";
                ScriptDaemon.npc_set(npc, 12);
                break;
            case 24:
            case 44:
            case 64:
            case 84:
            case 104:
            case 124:
            case 144:
            case 164:
            case 184:
            case 204:
            case 224:
            case 244:
            case 294:
            case 314:
            case 334:
                originalScript = "npc_set(npc,13)";
                ScriptDaemon.npc_set(npc, 13);
                break;
            case 25:
            case 45:
            case 65:
            case 85:
            case 105:
            case 125:
            case 145:
            case 165:
            case 185:
            case 205:
            case 225:
            case 245:
            case 295:
            case 315:
            case 335:
                originalScript = "npc_set(npc,14)";
                ScriptDaemon.npc_set(npc, 14);
                break;
            case 190:
                originalScript = "game.quests[89].state = qs_completed";
                SetQuestState(89, QuestState.Completed);
                break;
            case 251:
                originalScript = "npc_set(npc,11)";
                ScriptDaemon.npc_set(npc, 11);
                break;
            case 252:
            case 261:
                originalScript = "pc.follower_add( npc )";
                pc.AddFollower(npc);
                break;
            case 270:
                originalScript = "pc.follower_remove( npc ); regenerate(npc,pc)";
                pc.RemoveFollower(npc);
                // TODO regenerate(npc, pc);
                break;
            case 341:
                originalScript = "game.fade_and_teleport(0,0,0,5142,386,380)";
                FadeAndTeleport(0, 0, 0, 5142, 386, 380);
                break;
            case 521:
                originalScript = "npc_set(npc,21)";
                ScriptDaemon.npc_set(npc, 21);
                break;
            case 522:
                originalScript = "npc_set(npc,15)";
                ScriptDaemon.npc_set(npc, 15);
                break;
            case 523:
                originalScript = "npc_set(npc,16)";
                ScriptDaemon.npc_set(npc, 16);
                break;
            case 524:
                originalScript = "npc_set(npc,17)";
                ScriptDaemon.npc_set(npc, 17);
                break;
            case 532:
                originalScript = "npc_set(npc,18)";
                ScriptDaemon.npc_set(npc, 18);
                break;
            case 533:
                originalScript = "npc_set(npc,19)";
                ScriptDaemon.npc_set(npc, 19);
                break;
            case 534:
                originalScript = "npc_set(npc,20)";
                ScriptDaemon.npc_set(npc, 20);
                break;
            case 541:
                originalScript = "pc.money_adj(-1000000)";
                pc.AdjustMoney(-1000000);
                break;
            case 551:
            case 1631:
                originalScript = "pc.money_adj(-400000)";
                pc.AdjustMoney(-400000);
                break;
            case 561:
                originalScript = "pc.money_adj(-800000)";
                pc.AdjustMoney(-800000);
                break;
            case 571:
            case 581:
            case 591:
                originalScript = "pc.money_adj(-600000)";
                pc.AdjustMoney(-600000);
                break;
            case 661:
            case 671:
            case 681:
            case 691:
            case 701:
            case 711:
            case 721:
            case 731:
            case 741:
            case 751:
            case 761:
            case 771:
            case 781:
            case 791:
            case 801:
            case 811:
            case 821:
            case 831:
            case 841:
            case 851:
            case 861:
            case 871:
            case 881:
            case 891:
            case 901:
            case 911:
            case 921:
            case 931:
            case 941:
            case 951:
            case 961:
            case 971:
            case 981:
            case 991:
            case 1001:
            case 1011:
            case 1021:
            case 1031:
            case 1041:
            case 1051:
            case 1061:
            case 1071:
            case 1081:
            case 1091:
            case 1101:
            case 1621:
                originalScript = "pc.money_adj(-100000)";
                pc.AdjustMoney(-100000);
                break;
            case 1110:
                originalScript = "game.global_vars[101] = 2";
                SetGlobalVar(101, 2);
                break;
            case 1111:
            case 1112:
                originalScript = "create_item_in_inventory( 11011, pc )";
                Utilities.create_item_in_inventory(11011, pc);
                break;
            case 1120:
                originalScript = "game.global_vars[102] = 2";
                SetGlobalVar(102, 2);
                break;
            case 1121:
            case 1122:
                originalScript = "create_item_in_inventory( 11012, pc )";
                Utilities.create_item_in_inventory(11012, pc);
                break;
            case 1130:
                originalScript = "game.global_vars[103] = 2";
                SetGlobalVar(103, 2);
                break;
            case 1131:
            case 1132:
                originalScript = "create_item_in_inventory( 11013, pc )";
                Utilities.create_item_in_inventory(11013, pc);
                break;
            case 1140:
                originalScript = "game.global_vars[104] = 2";
                SetGlobalVar(104, 2);
                break;
            case 1141:
            case 1142:
                originalScript = "create_item_in_inventory( 11014, pc )";
                Utilities.create_item_in_inventory(11014, pc);
                break;
            case 1150:
                originalScript = "game.global_vars[105] = 2";
                SetGlobalVar(105, 2);
                break;
            case 1151:
            case 1152:
                originalScript = "create_item_in_inventory( 11015, pc )";
                Utilities.create_item_in_inventory(11015, pc);
                break;
            case 1160:
                originalScript = "game.global_vars[106] = 2";
                SetGlobalVar(106, 2);
                break;
            case 1161:
            case 1162:
                originalScript = "create_item_in_inventory( 11016, pc )";
                Utilities.create_item_in_inventory(11016, pc);
                break;
            case 1170:
                originalScript = "game.global_vars[107] = 2";
                SetGlobalVar(107, 2);
                break;
            case 1171:
            case 1172:
                originalScript = "create_item_in_inventory( 11017, pc )";
                Utilities.create_item_in_inventory(11017, pc);
                break;
            case 1180:
                originalScript = "game.global_vars[108] = 2";
                SetGlobalVar(108, 2);
                break;
            case 1181:
            case 1182:
                originalScript = "create_item_in_inventory( 11018, pc )";
                Utilities.create_item_in_inventory(11018, pc);
                break;
            case 1190:
                originalScript = "game.global_vars[109] = 2";
                SetGlobalVar(109, 2);
                break;
            case 1191:
            case 1192:
                originalScript = "create_item_in_inventory( 11019, pc )";
                Utilities.create_item_in_inventory(11019, pc);
                break;
            case 1200:
                originalScript = "game.global_vars[110] = 2";
                SetGlobalVar(110, 2);
                break;
            case 1201:
            case 1202:
                originalScript = "create_item_in_inventory( 11020, pc )";
                Utilities.create_item_in_inventory(11020, pc);
                break;
            case 1210:
                originalScript = "game.global_vars[111] = 2";
                SetGlobalVar(111, 2);
                break;
            case 1211:
            case 1212:
                originalScript = "create_item_in_inventory( 11021, pc )";
                Utilities.create_item_in_inventory(11021, pc);
                break;
            case 1220:
                originalScript = "game.global_vars[112] = 2";
                SetGlobalVar(112, 2);
                break;
            case 1221:
            case 1222:
                originalScript = "create_item_in_inventory( 11022, pc )";
                Utilities.create_item_in_inventory(11022, pc);
                break;
            case 1230:
                originalScript = "game.global_vars[113] = 2";
                SetGlobalVar(113, 2);
                break;
            case 1231:
            case 1232:
                originalScript = "create_item_in_inventory( 11023, pc )";
                Utilities.create_item_in_inventory(11023, pc);
                break;
            case 1240:
                originalScript = "game.global_vars[114] = 2";
                SetGlobalVar(114, 2);
                break;
            case 1241:
            case 1242:
                originalScript = "create_item_in_inventory( 11024, pc )";
                Utilities.create_item_in_inventory(11024, pc);
                break;
            case 1250:
                originalScript = "game.global_vars[115] = 2";
                SetGlobalVar(115, 2);
                break;
            case 1251:
            case 1252:
                originalScript = "create_item_in_inventory( 11025, pc )";
                Utilities.create_item_in_inventory(11025, pc);
                break;
            case 1260:
                originalScript = "game.global_vars[116] = 2";
                SetGlobalVar(116, 2);
                break;
            case 1261:
            case 1262:
                originalScript = "create_item_in_inventory( 11026, pc )";
                Utilities.create_item_in_inventory(11026, pc);
                break;
            case 1270:
                originalScript = "game.global_vars[117] = 2";
                SetGlobalVar(117, 2);
                break;
            case 1271:
            case 1272:
                originalScript = "create_item_in_inventory( 11027, pc )";
                Utilities.create_item_in_inventory(11027, pc);
                break;
            case 1280:
                originalScript = "game.global_vars[118] = 2";
                SetGlobalVar(118, 2);
                break;
            case 1281:
            case 1282:
                originalScript = "create_item_in_inventory( 11028, pc )";
                Utilities.create_item_in_inventory(11028, pc);
                break;
            case 1290:
                originalScript = "game.global_vars[119] = 2";
                SetGlobalVar(119, 2);
                break;
            case 1291:
            case 1292:
                originalScript = "create_item_in_inventory( 11029, pc )";
                Utilities.create_item_in_inventory(11029, pc);
                break;
            case 1300:
                originalScript = "game.global_vars[120] = 2";
                SetGlobalVar(120, 2);
                break;
            case 1301:
            case 1302:
                originalScript = "create_item_in_inventory( 11030, pc )";
                Utilities.create_item_in_inventory(11030, pc);
                break;
            case 1310:
                originalScript = "game.global_vars[121] = 2";
                SetGlobalVar(121, 2);
                break;
            case 1311:
            case 1312:
                originalScript = "create_item_in_inventory( 11031, pc )";
                Utilities.create_item_in_inventory(11031, pc);
                break;
            case 1320:
                originalScript = "game.global_vars[122] = 2";
                SetGlobalVar(122, 2);
                break;
            case 1321:
            case 1322:
                originalScript = "create_item_in_inventory( 11032, pc )";
                Utilities.create_item_in_inventory(11032, pc);
                break;
            case 1330:
                originalScript = "game.global_vars[123] = 2";
                SetGlobalVar(123, 2);
                break;
            case 1331:
            case 1332:
                originalScript = "create_item_in_inventory( 11033, pc )";
                Utilities.create_item_in_inventory(11033, pc);
                break;
            case 1340:
                originalScript = "game.global_vars[124] = 2";
                SetGlobalVar(124, 2);
                break;
            case 1341:
            case 1342:
                originalScript = "create_item_in_inventory( 11034, pc )";
                Utilities.create_item_in_inventory(11034, pc);
                break;
            case 1350:
                originalScript = "game.global_vars[125] = 2";
                SetGlobalVar(125, 2);
                break;
            case 1351:
            case 1352:
                originalScript = "create_item_in_inventory( 11035, pc )";
                Utilities.create_item_in_inventory(11035, pc);
                break;
            case 1360:
                originalScript = "game.global_vars[126] = 2";
                SetGlobalVar(126, 2);
                break;
            case 1361:
            case 1362:
                originalScript = "create_item_in_inventory( 11036, pc )";
                Utilities.create_item_in_inventory(11036, pc);
                break;
            case 1370:
                originalScript = "game.global_vars[127] = 2";
                SetGlobalVar(127, 2);
                break;
            case 1371:
            case 1372:
                originalScript = "create_item_in_inventory( 11037, pc )";
                Utilities.create_item_in_inventory(11037, pc);
                break;
            case 1380:
                originalScript = "game.global_vars[128] = 2";
                SetGlobalVar(128, 2);
                break;
            case 1381:
            case 1382:
                originalScript = "create_item_in_inventory( 11038, pc )";
                Utilities.create_item_in_inventory(11038, pc);
                break;
            case 1390:
                originalScript = "game.global_vars[129] = 2";
                SetGlobalVar(129, 2);
                break;
            case 1391:
            case 1392:
                originalScript = "create_item_in_inventory( 11039, pc )";
                Utilities.create_item_in_inventory(11039, pc);
                break;
            case 1400:
                originalScript = "game.global_vars[130] = 2";
                SetGlobalVar(130, 2);
                break;
            case 1401:
            case 1402:
                originalScript = "create_item_in_inventory( 11040, pc )";
                Utilities.create_item_in_inventory(11040, pc);
                break;
            case 1410:
                originalScript = "game.global_vars[131] = 2";
                SetGlobalVar(131, 2);
                break;
            case 1411:
            case 1412:
                originalScript = "create_item_in_inventory( 11041, pc )";
                Utilities.create_item_in_inventory(11041, pc);
                break;
            case 1420:
                originalScript = "game.global_vars[132] = 2";
                SetGlobalVar(132, 2);
                break;
            case 1421:
            case 1422:
                originalScript = "create_item_in_inventory( 11042, pc )";
                Utilities.create_item_in_inventory(11042, pc);
                break;
            case 1430:
                originalScript = "game.global_vars[133] = 2";
                SetGlobalVar(133, 2);
                break;
            case 1431:
            case 1432:
                originalScript = "create_item_in_inventory( 11043, pc )";
                Utilities.create_item_in_inventory(11043, pc);
                break;
            case 1440:
                originalScript = "game.global_vars[134] = 2";
                SetGlobalVar(134, 2);
                break;
            case 1441:
            case 1442:
                originalScript = "create_item_in_inventory( 11044, pc )";
                Utilities.create_item_in_inventory(11044, pc);
                break;
            case 1450:
                originalScript = "game.global_vars[135] = 2";
                SetGlobalVar(135, 2);
                break;
            case 1451:
            case 1452:
                originalScript = "create_item_in_inventory( 11045, pc )";
                Utilities.create_item_in_inventory(11045, pc);
                break;
            case 1460:
                originalScript = "game.global_vars[136] = 2";
                SetGlobalVar(136, 2);
                break;
            case 1461:
            case 1462:
                originalScript = "create_item_in_inventory( 11046, pc )";
                Utilities.create_item_in_inventory(11046, pc);
                break;
            case 1470:
                originalScript = "game.global_vars[137] = 2";
                SetGlobalVar(137, 2);
                break;
            case 1471:
            case 1472:
                originalScript = "create_item_in_inventory( 11047, pc )";
                Utilities.create_item_in_inventory(11047, pc);
                break;
            case 1480:
                originalScript = "game.global_vars[138] = 2";
                SetGlobalVar(138, 2);
                break;
            case 1481:
            case 1482:
                originalScript = "create_item_in_inventory( 11048, pc )";
                Utilities.create_item_in_inventory(11048, pc);
                break;
            case 1490:
                originalScript = "game.global_vars[139] = 2";
                SetGlobalVar(139, 2);
                break;
            case 1491:
            case 1492:
                originalScript = "create_item_in_inventory( 11049, pc )";
                Utilities.create_item_in_inventory(11049, pc);
                break;
            case 1500:
                originalScript = "game.global_vars[140] = 2";
                SetGlobalVar(140, 2);
                break;
            case 1501:
            case 1502:
                originalScript = "create_item_in_inventory( 11051, pc )";
                Utilities.create_item_in_inventory(11051, pc);
                break;
            case 1510:
                originalScript = "game.global_vars[141] = 2";
                SetGlobalVar(141, 2);
                break;
            case 1511:
            case 1512:
                originalScript = "create_item_in_inventory( 11052, pc )";
                Utilities.create_item_in_inventory(11052, pc);
                break;
            case 1520:
                originalScript = "game.global_vars[142] = 2";
                SetGlobalVar(142, 2);
                break;
            case 1521:
            case 1522:
                originalScript = "create_item_in_inventory( 11054, pc )";
                Utilities.create_item_in_inventory(11054, pc);
                break;
            case 1530:
                originalScript = "game.global_vars[143] = 2";
                SetGlobalVar(143, 2);
                break;
            case 1531:
            case 1532:
                originalScript = "create_item_in_inventory( 11055, pc )";
                Utilities.create_item_in_inventory(11055, pc);
                break;
            case 1540:
                originalScript = "game.global_vars[144] = 2";
                SetGlobalVar(144, 2);
                break;
            case 1541:
            case 1542:
                originalScript = "create_item_in_inventory( 11056, pc )";
                Utilities.create_item_in_inventory(11056, pc);
                break;
            case 1550:
                originalScript = "game.global_vars[145] = 2";
                SetGlobalVar(145, 2);
                break;
            case 1551:
            case 1552:
                originalScript = "create_item_in_inventory( 11053, pc )";
                Utilities.create_item_in_inventory(11053, pc);
                break;
            case 1560:
                originalScript = "game.global_vars[146] = 2";
                SetGlobalVar(146, 2);
                break;
            case 1561:
            case 1562:
                originalScript = "create_item_in_inventory( 11057, pc )";
                Utilities.create_item_in_inventory(11057, pc);
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