
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

namespace Scripts.Dialog
{
    [DialogScript(228)]
    public class KidsOffDialog : KidsOff, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 11:
                case 12:
                case 21:
                case 22:
                case 31:
                case 32:
                case 41:
                case 42:
                case 51:
                case 52:
                case 61:
                case 62:
                case 71:
                case 72:
                case 81:
                case 82:
                case 91:
                case 92:
                case 101:
                case 102:
                case 111:
                case 112:
                case 301:
                case 302:
                case 401:
                case 402:
                case 411:
                case 412:
                case 501:
                case 511:
                case 512:
                case 521:
                case 522:
                case 631:
                case 632:
                case 801:
                case 802:
                case 1301:
                case 1302:
                case 2101:
                case 2102:
                    originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD";
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 4:
                case 5:
                case 13:
                case 14:
                case 23:
                case 24:
                case 33:
                case 34:
                case 43:
                case 44:
                case 53:
                case 54:
                case 63:
                case 64:
                case 73:
                case 74:
                case 83:
                case 84:
                case 93:
                case 94:
                case 103:
                case 104:
                case 113:
                case 114:
                case 303:
                case 304:
                case 403:
                case 404:
                case 413:
                case 414:
                case 502:
                case 513:
                case 514:
                case 523:
                case 524:
                case 633:
                case 634:
                case 803:
                case 804:
                case 1303:
                case 1304:
                case 2103:
                case 2104:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 6:
                case 7:
                case 15:
                case 16:
                case 25:
                case 26:
                case 35:
                case 36:
                case 45:
                case 46:
                case 55:
                case 56:
                case 65:
                case 66:
                case 75:
                case 76:
                case 85:
                case 86:
                case 95:
                case 96:
                case 105:
                case 106:
                case 115:
                case 116:
                case 305:
                case 306:
                case 313:
                case 314:
                case 405:
                case 406:
                case 415:
                case 416:
                case 503:
                case 515:
                case 516:
                case 525:
                case 526:
                case 635:
                case 636:
                case 1305:
                case 1306:
                case 2105:
                case 2106:
                case 2113:
                case 2114:
                    originalScript = "game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 311:
                case 312:
                    originalScript = "(pc.item_find(12045) != OBJ_HANDLE_NULL or pc.item_find(12046) != OBJ_HANDLE_NULL or pc.item_find(12047) != OBJ_HANDLE_NULL or pc.item_find(12048) != OBJ_HANDLE_NULL or pc.item_find(12049) != OBJ_HANDLE_NULL or pc.item_find(12050) != OBJ_HANDLE_NULL or pc.item_find(12051) != OBJ_HANDLE_NULL or pc.item_find(12052) != OBJ_HANDLE_NULL or pc.item_find(12053) != OBJ_HANDLE_NULL or c.item_find(12054) != OBJ_HANDLE_NULL) and (game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL)";
                    return (pc.FindItemByName(12045) != null || pc.FindItemByName(12046) != null || pc.FindItemByName(12047) != null || pc.FindItemByName(12048) != null || pc.FindItemByName(12049) != null || pc.FindItemByName(12050) != null || pc.FindItemByName(12051) != null || pc.FindItemByName(12052) != null || pc.FindItemByName(12053) != null || pc.FindItemByName(12054) != null) && (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                case 901:
                case 902:
                case 1001:
                case 1002:
                case 1011:
                case 1012:
                case 1021:
                case 1022:
                case 1031:
                case 1032:
                case 1041:
                case 1042:
                case 1051:
                case 1052:
                case 1061:
                case 1062:
                case 1071:
                case 1072:
                case 1201:
                case 1202:
                    originalScript = "game.global_flags[801] == 0";
                    return !GetGlobalFlag(801);
                case 903:
                case 904:
                case 1003:
                case 1004:
                case 1013:
                case 1014:
                case 1023:
                case 1024:
                case 1033:
                case 1034:
                case 1043:
                case 1044:
                case 1053:
                case 1054:
                case 1063:
                case 1064:
                case 1073:
                case 1074:
                case 1401:
                case 1402:
                    originalScript = "game.global_flags[802] == 0";
                    return !GetGlobalFlag(802);
                case 905:
                case 906:
                case 1203:
                case 1204:
                case 1403:
                case 1404:
                    originalScript = "game.quests[23].state == qs_accepted and game.global_flags[804] == 0";
                    return GetQuestState(23) == QuestState.Accepted && !GetGlobalFlag(804);
                case 907:
                case 908:
                case 1205:
                case 1206:
                case 1405:
                case 1406:
                    originalScript = "game.quests[27].state == qs_accepted and game.global_flags[804] == 0";
                    return GetQuestState(27) == QuestState.Accepted && !GetGlobalFlag(804);
                case 909:
                case 910:
                case 1207:
                case 1208:
                case 1407:
                case 1408:
                    originalScript = "game.quests[22].state == qs_accepted and game.global_flags[804] == 0";
                    return GetQuestState(22) == QuestState.Accepted && !GetGlobalFlag(804);
                case 911:
                case 912:
                case 1209:
                case 1210:
                case 1409:
                case 1410:
                    originalScript = "game.quests[25].state == qs_accepted and game.global_flags[804] == 0";
                    return GetQuestState(25) == QuestState.Accepted && !GetGlobalFlag(804);
                case 913:
                case 914:
                case 1211:
                case 1212:
                case 1411:
                case 1412:
                    originalScript = "game.quests[24].state == qs_accepted and game.global_flags[804] == 0";
                    return GetQuestState(24) == QuestState.Accepted && !GetGlobalFlag(804);
                case 915:
                case 916:
                case 1213:
                case 1214:
                case 1413:
                case 1414:
                    originalScript = "game.quests[28].state == qs_accepted and game.global_flags[804] == 0";
                    return GetQuestState(28) == QuestState.Accepted && !GetGlobalFlag(804);
                case 917:
                case 918:
                case 1215:
                case 1216:
                case 1415:
                case 1416:
                    originalScript = "game.quests[26].state == qs_accepted and game.global_flags[804] == 0";
                    return GetQuestState(26) == QuestState.Accepted && !GetGlobalFlag(804);
                case 919:
                case 920:
                case 1217:
                case 1218:
                case 1417:
                case 1418:
                    originalScript = "game.quests[30].state == qs_accepted and game.global_flags[804] == 0";
                    return GetQuestState(30) == QuestState.Accepted && !GetGlobalFlag(804);
                case 921:
                case 922:
                case 1219:
                case 1220:
                case 1419:
                case 1420:
                    originalScript = "game.quests[29].state == qs_accepted and game.global_flags[804] == 0";
                    return GetQuestState(29) == QuestState.Accepted && !GetGlobalFlag(804);
                case 1521:
                    originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == LAWFUL_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.LAWFUL_NEUTRAL;
                case 1522:
                    originalScript = "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL";
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL;
                case 2111:
                case 2112:
                    originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 3002:
                    originalScript = "game.global_flags[434] == 1 or game.global_vars[436] == 6 or game.global_vars[436] == 7 or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == LAWFUL_EVIL";
                    return GetGlobalFlag(434) || GetGlobalVar(436) == 6 || GetGlobalVar(436) == 7 || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL;
                case 3033:
                case 3034:
                    originalScript = "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == LAWFUL_EVIL";
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 202:
                    originalScript = "210";
                    break;
                case 1000:
                case 1010:
                case 1020:
                case 1030:
                case 1040:
                case 1050:
                case 1060:
                case 1070:
                    originalScript = "game.global_flags[804] = 1";
                    SetGlobalFlag(804, true);
                    break;
                case 1200:
                    originalScript = "game.global_flags[802] = 1";
                    SetGlobalFlag(802, true);
                    break;
                case 1300:
                    originalScript = "game.global_flags[801] = 1";
                    SetGlobalFlag(801, true);
                    break;
                case 1500:
                    originalScript = "game.global_flags[378] = 1";
                    SetGlobalFlag(378, true);
                    break;
                case 2100:
                    originalScript = "game.global_flags[857] = 1";
                    SetGlobalFlag(857, true);
                    break;
                case 2121:
                    originalScript = "talk_nps(npc, pc); game.areas[1] = 1; npc.destroy()";
                    talk_nps(npc, pc);
                    MakeAreaKnown(1);
                    npc.Destroy();
                    ;
                    break;
                case 3011:
                case 3031:
                case 3032:
                case 3033:
                case 3034:
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
}
