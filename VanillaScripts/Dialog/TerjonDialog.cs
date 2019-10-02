
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace VanillaScripts.Dialog
{
    [DialogScript(23)]
    public class TerjonDialog : Terjon, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 4:
                case 5:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_GOOD");
                    return PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 23:
                case 24:
                case 25:
                case 26:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 31:
                case 32:
                case 41:
                case 42:
                    Trace.Assert(originalScript == "(game.party_alignment == NEUTRAL_GOOD or game.party_alignment == LAWFUL_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL ) and game.story_state == 0");
                    return (PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL) && StoryState == 0;
                case 33:
                case 34:
                case 43:
                case 44:
                case 1303:
                case 1304:
                    Trace.Assert(originalScript == "game.global_flags[24] == 1 and game.quests[10].state == qs_completed and game.global_flags[17] == 0");
                    return GetGlobalFlag(24) && GetQuestState(10) == QuestState.Completed && !GetGlobalFlag(17);
                case 35:
                case 36:
                case 45:
                case 46:
                case 1305:
                case 1306:
                    Trace.Assert(originalScript == "game.global_flags[16] == 1 and game.quests[11].state <= qs_accepted");
                    return GetGlobalFlag(16) && GetQuestState(11) <= QuestState.Accepted;
                case 37:
                case 38:
                case 47:
                case 48:
                case 1307:
                case 1308:
                    Trace.Assert(originalScript == "game.global_flags[26] == 1 and game.quests[12].state == qs_completed and game.global_flags[17] == 0");
                    return GetGlobalFlag(26) && GetQuestState(12) == QuestState.Completed && !GetGlobalFlag(17);
                case 51:
                case 52:
                    Trace.Assert(originalScript == "game.global_vars[5] <= 9");
                    return GetGlobalVar(5) <= 9;
                case 53:
                case 54:
                case 55:
                    Trace.Assert(originalScript == "game.global_vars[5] >= 10");
                    return GetGlobalVar(5) >= 10;
                case 61:
                case 62:
                case 1221:
                case 1222:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_item\", 3003 )");
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(3003));
                case 65:
                case 66:
                    Trace.Assert(originalScript == "not anyone( pc.group_list(), \"has_item\", 3003 )");
                    return !pc.GetPartyMembers().Any(o => o.HasItemByName(3003));
                case 91:
                case 92:
                case 109:
                case 110:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL and game.global_flags[67] == 0");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL && !GetGlobalFlag(67);
                case 93:
                case 94:
                    Trace.Assert(originalScript == "game.story_state <= 1");
                    return StoryState <= 1;
                case 95:
                case 96:
                    Trace.Assert(originalScript == "game.story_state >=  2 and game.areas[3] == 0");
                    return StoryState >= 2 && !IsAreaKnown(3);
                case 97:
                case 98:
                    Trace.Assert(originalScript == "(game.global_flags[24] == 1 and game.quests[10].state == qs_completed and game.global_flags[17] == 0) or (game.global_flags[16] == 1 and game.quests[11].state <= qs_accepted) or (game.global_flags[26] == 1 and game.quests[12].state == qs_completed and game.global_flags[17] == 0)");
                    return (GetGlobalFlag(24) && GetQuestState(10) == QuestState.Completed && !GetGlobalFlag(17)) || (GetGlobalFlag(16) && GetQuestState(11) <= QuestState.Accepted) || (GetGlobalFlag(26) && GetQuestState(12) == QuestState.Completed && !GetGlobalFlag(17));
                case 101:
                case 102:
                case 221:
                case 225:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_deity ) != 16");
                    return pc.GetStat(Stat.deity) != 16;
                case 103:
                case 104:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_deity ) == 16");
                    return pc.GetStat(Stat.deity) == 16;
                case 107:
                case 108:
                    Trace.Assert(originalScript == "game.global_flags[22] == 0 and game.party_alignment == NEUTRAL_GOOD");
                    return !GetGlobalFlag(22) && PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 111:
                case 112:
                    Trace.Assert(originalScript == "pc.d20_query(Q_IsFallenPaladin) == 1");
                    return pc.D20Query(D20DispatcherKey.QUE_IsFallenPaladin);
                case 115:
                case 116:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_item\", 3004 ) and ( game.quests[13].state == qs_mentioned or game.quests[13].state == qs_accepted )");
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(3004)) && (GetQuestState(13) == QuestState.Mentioned || GetQuestState(13) == QuestState.Accepted);
                case 117:
                case 118:
                    Trace.Assert(originalScript == "game.story_state >= 2 and game.party_alignment == CHAOTIC_NEUTRAL and anyone( pc.group_list(), \"has_item\", 5802 )");
                    return StoryState >= 2 && PartyAlignment == Alignment.CHAOTIC_NEUTRAL && pc.GetPartyMembers().Any(o => o.HasItemByName(5802));
                case 119:
                case 120:
                    Trace.Assert(originalScript == "game.story_state <= 1 and game.party_alignment == CHAOTIC_NEUTRAL and anyone( pc.group_list(), \"has_item\", 5802 )");
                    return StoryState <= 1 && PartyAlignment == Alignment.CHAOTIC_NEUTRAL && pc.GetPartyMembers().Any(o => o.HasItemByName(5802));
                case 121:
                case 122:
                    Trace.Assert(originalScript == "game.story_state >= 2 and game.party_alignment == NEUTRAL_GOOD and game.areas[3] == 0");
                    return StoryState >= 2 && PartyAlignment == Alignment.NEUTRAL_GOOD && !IsAreaKnown(3);
                case 123:
                case 124:
                case 403:
                case 404:
                case 413:
                case 414:
                    Trace.Assert(originalScript == "game.quests[11].state == qs_mentioned");
                    return GetQuestState(11) == QuestState.Mentioned;
                case 125:
                case 126:
                    Trace.Assert(originalScript == "game.party_alignment != CHAOTIC_NEUTRAL and anyone( pc.group_list(), \"has_item\", 5802 )");
                    return PartyAlignment != Alignment.CHAOTIC_NEUTRAL && pc.GetPartyMembers().Any(o => o.HasItemByName(5802));
                case 127:
                case 128:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_item\", 3004 ) and game.quests[13].state == qs_unknown");
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(3004)) && GetQuestState(13) == QuestState.Unknown;
                case 141:
                case 142:
                    Trace.Assert(originalScript == "game.global_flags[23] == 1 and game.global_flags[24] == 0 and game.global_flags[25] == 0 and game.global_flags[26] == 0");
                    return GetGlobalFlag(23) && !GetGlobalFlag(24) && !GetGlobalFlag(25) && !GetGlobalFlag(26);
                case 145:
                case 146:
                    Trace.Assert(originalScript == "game.global_flags[24] == 0 and game.global_flags[15] == 1");
                    return !GetGlobalFlag(24) && GetGlobalFlag(15);
                case 147:
                case 148:
                    Trace.Assert(originalScript == "game.global_flags[25] == 0 and game.global_flags[16] == 1 and game.quests[11] != qs_completed");
                    return !GetGlobalFlag(25) && GetGlobalFlag(16) && GetQuestState(11) != QuestState.Completed;
                case 151:
                case 160:
                    Trace.Assert(originalScript == "game.quests[10].state == qs_completed and game.quests[11].state == qs_completed and game.quests[12].state == qs_completed");
                    return GetQuestState(10) == QuestState.Completed && GetQuestState(11) == QuestState.Completed && GetQuestState(12) == QuestState.Completed;
                case 152:
                case 161:
                    Trace.Assert(originalScript == "game.quests[10].state == qs_completed and game.quests[11].state == qs_completed and game.quests[12].state != qs_completed");
                    return GetQuestState(10) == QuestState.Completed && GetQuestState(11) == QuestState.Completed && GetQuestState(12) != QuestState.Completed;
                case 153:
                case 162:
                    Trace.Assert(originalScript == "game.quests[10].state != qs_completed and game.quests[11].state == qs_completed and game.quests[12].state == qs_completed");
                    return GetQuestState(10) != QuestState.Completed && GetQuestState(11) == QuestState.Completed && GetQuestState(12) == QuestState.Completed;
                case 154:
                case 163:
                    Trace.Assert(originalScript == "game.quests[10].state == qs_completed and game.quests[11].state != qs_completed and game.quests[12].state == qs_completed");
                    return GetQuestState(10) == QuestState.Completed && GetQuestState(11) != QuestState.Completed && GetQuestState(12) == QuestState.Completed;
                case 155:
                case 164:
                    Trace.Assert(originalScript == "game.quests[10].state != qs_completed and game.quests[11].state == qs_completed and game.quests[12].state != qs_completed");
                    return GetQuestState(10) != QuestState.Completed && GetQuestState(11) == QuestState.Completed && GetQuestState(12) != QuestState.Completed;
                case 156:
                case 165:
                    Trace.Assert(originalScript == "game.quests[10].state != qs_completed and game.quests[11].state != qs_completed and game.quests[12].state == qs_completed");
                    return GetQuestState(10) != QuestState.Completed && GetQuestState(11) != QuestState.Completed && GetQuestState(12) == QuestState.Completed;
                case 157:
                case 166:
                    Trace.Assert(originalScript == "game.quests[10].state == qs_completed and game.quests[11].state != qs_completed and game.quests[12].state != qs_completed");
                    return GetQuestState(10) == QuestState.Completed && GetQuestState(11) != QuestState.Completed && GetQuestState(12) != QuestState.Completed;
                case 158:
                case 167:
                    Trace.Assert(originalScript == "game.quests[10].state != qs_completed and game.quests[11].state != qs_completed and game.quests[12].state != qs_completed");
                    return GetQuestState(10) != QuestState.Completed && GetQuestState(11) != QuestState.Completed && GetQuestState(12) != QuestState.Completed;
                case 169:
                case 170:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 7 and game.global_vars[27] == 1");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 7 && GetGlobalVar(27) == 1;
                case 211:
                case 212:
                    Trace.Assert(originalScript == "game.quests[11].state == qs_unknown or game.quests[11].state == qs_mentioned");
                    return GetQuestState(11) == QuestState.Unknown || GetQuestState(11) == QuestState.Mentioned;
                case 222:
                case 223:
                case 231:
                case 232:
                case 241:
                case 242:
                    Trace.Assert(originalScript == "game.quests[13].state <= qs_mentioned");
                    return GetQuestState(13) <= QuestState.Mentioned;
                case 224:
                    Trace.Assert(originalScript == "game.party_alignment != NEUTRAL_GOOD");
                    return PartyAlignment != Alignment.NEUTRAL_GOOD;
                case 263:
                case 264:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_item\", 3004 )");
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(3004));
                case 273:
                case 274:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_NEUTRAL");
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 341:
                case 342:
                    Trace.Assert(originalScript == "game.global_flags[15] == 1");
                    return GetGlobalFlag(15);
                case 343:
                case 344:
                    Trace.Assert(originalScript == "game.global_flags[15] == 0");
                    return !GetGlobalFlag(15);
                case 345:
                case 346:
                case 363:
                case 364:
                    Trace.Assert(originalScript == "game.global_flags[23] == 0");
                    return !GetGlobalFlag(23);
                case 351:
                case 352:
                    Trace.Assert(originalScript == "game.global_flags[25] == 1");
                    return GetGlobalFlag(25);
                case 353:
                case 354:
                    Trace.Assert(originalScript == "(game.global_flags[23] == 0) or (game.global_flags[24] == 1) or (game.global_flags[26] ==1)");
                    return (!GetGlobalFlag(23)) || (GetGlobalFlag(24)) || (GetGlobalFlag(26));
                case 385:
                case 388:
                    Trace.Assert(originalScript == "game.quests[12].state == qs_mentioned or game.quests[12].state == qs_accepted");
                    return GetQuestState(12) == QuestState.Mentioned || GetQuestState(12) == QuestState.Accepted;
                case 395:
                case 398:
                    Trace.Assert(originalScript == "game.quests[10].state == qs_mentioned or game.quests[10].state == qs_accepted");
                    return GetQuestState(10) == QuestState.Mentioned || GetQuestState(10) == QuestState.Accepted;
                case 405:
                case 406:
                case 407:
                case 408:
                case 415:
                case 416:
                case 417:
                case 418:
                    Trace.Assert(originalScript == "game.quests[11].state == qs_accepted");
                    return GetQuestState(11) == QuestState.Accepted;
                case 424:
                    Trace.Assert(originalScript == "game.global_flags[24] == 1 and game.global_flags[15] == 1 and game.quests[10].state != qs_completed");
                    return GetGlobalFlag(24) && GetGlobalFlag(15) && GetQuestState(10) != QuestState.Completed;
                case 451:
                case 452:
                    Trace.Assert(originalScript == "game.story_state == 0");
                    return StoryState == 0;
                case 502:
                    Trace.Assert(originalScript == "game.global_flags[33] == 1 and game.quests[11].state != qs_completed");
                    return GetGlobalFlag(33) && GetQuestState(11) != QuestState.Completed;
                case 505:
                    Trace.Assert(originalScript == "game.global_flags[33] == 1");
                    return GetGlobalFlag(33);
                case 507:
                case 508:
                case 951:
                case 952:
                case 971:
                case 972:
                    Trace.Assert(originalScript == "game.global_vars[5] == 1");
                    return GetGlobalVar(5) == 1;
                case 509:
                case 510:
                case 953:
                case 954:
                case 973:
                case 974:
                    Trace.Assert(originalScript == "game.global_vars[5] == 2");
                    return GetGlobalVar(5) == 2;
                case 511:
                case 512:
                case 955:
                case 956:
                case 975:
                case 976:
                    Trace.Assert(originalScript == "game.global_vars[5] == 3");
                    return GetGlobalVar(5) == 3;
                case 513:
                case 514:
                case 957:
                case 958:
                case 977:
                case 978:
                    Trace.Assert(originalScript == "game.global_vars[5] == 4");
                    return GetGlobalVar(5) == 4;
                case 515:
                case 516:
                case 959:
                case 960:
                case 979:
                case 980:
                    Trace.Assert(originalScript == "game.global_vars[5] == 5");
                    return GetGlobalVar(5) == 5;
                case 517:
                case 518:
                case 961:
                case 962:
                case 981:
                case 982:
                    Trace.Assert(originalScript == "game.global_vars[5] == 6");
                    return GetGlobalVar(5) == 6;
                case 519:
                case 520:
                case 963:
                case 964:
                case 983:
                case 984:
                    Trace.Assert(originalScript == "game.global_vars[5] >= 7");
                    return GetGlobalVar(5) >= 7;
                case 541:
                case 543:
                    Trace.Assert(originalScript == "pc.money_get() >= 55000");
                    return pc.GetMoney() >= 55000;
                case 561:
                case 563:
                    Trace.Assert(originalScript == "pc.money_get() >= 24000");
                    return pc.GetMoney() >= 24000;
                case 581:
                case 583:
                    Trace.Assert(originalScript == "pc.money_get() >= 9000");
                    return pc.GetMoney() >= 9000;
                case 601:
                case 603:
                    Trace.Assert(originalScript == "pc.money_get() >= 75000");
                    return pc.GetMoney() >= 75000;
                case 621:
                case 623:
                case 761:
                case 763:
                    Trace.Assert(originalScript == "pc.money_get() >= 27000");
                    return pc.GetMoney() >= 27000;
                case 641:
                case 643:
                    Trace.Assert(originalScript == "pc.money_get() >= 12000");
                    return pc.GetMoney() >= 12000;
                case 661:
                case 663:
                case 1021:
                case 1022:
                    Trace.Assert(originalScript == "pc.money_get() >= 100000");
                    return pc.GetMoney() >= 100000;
                case 681:
                case 683:
                case 821:
                case 823:
                    Trace.Assert(originalScript == "pc.money_get() >= 30000");
                    return pc.GetMoney() >= 30000;
                case 701:
                case 703:
                    Trace.Assert(originalScript == "pc.money_get() >= 20000");
                    return pc.GetMoney() >= 20000;
                case 721:
                case 723:
                    Trace.Assert(originalScript == "pc.money_get() >= 120000");
                    return pc.GetMoney() >= 120000;
                case 741:
                case 743:
                    Trace.Assert(originalScript == "pc.money_get() >= 36000");
                    return pc.GetMoney() >= 36000;
                case 781:
                case 783:
                    Trace.Assert(originalScript == "pc.money_get() >= 140000");
                    return pc.GetMoney() >= 140000;
                case 801:
                case 803:
                    Trace.Assert(originalScript == "pc.money_get() >= 40000");
                    return pc.GetMoney() >= 40000;
                case 841:
                case 843:
                    Trace.Assert(originalScript == "pc.money_get() >= 175000");
                    return pc.GetMoney() >= 175000;
                case 861:
                case 863:
                    Trace.Assert(originalScript == "pc.money_get() >= 45000");
                    return pc.GetMoney() >= 45000;
                case 881:
                case 883:
                    Trace.Assert(originalScript == "pc.money_get() >= 35000");
                    return pc.GetMoney() >= 35000;
                case 901:
                case 903:
                case 1031:
                case 1032:
                    Trace.Assert(originalScript == "pc.money_get() >= 200000");
                    return pc.GetMoney() >= 200000;
                case 921:
                case 923:
                    Trace.Assert(originalScript == "pc.money_get() >= 50000");
                    return pc.GetMoney() >= 50000;
                case 941:
                case 943:
                    Trace.Assert(originalScript == "pc.money_get() >= 41000");
                    return pc.GetMoney() >= 41000;
                case 991:
                case 992:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 1");
                    return pc.GetStat(Stat.level_paladin) == 1;
                case 993:
                case 994:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 2");
                    return pc.GetStat(Stat.level_paladin) == 2;
                case 995:
                case 996:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 3");
                    return pc.GetStat(Stat.level_paladin) == 3;
                case 997:
                case 998:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 4");
                    return pc.GetStat(Stat.level_paladin) == 4;
                case 999:
                case 1000:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 5");
                    return pc.GetStat(Stat.level_paladin) == 5;
                case 1001:
                case 1002:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 6");
                    return pc.GetStat(Stat.level_paladin) == 6;
                case 1003:
                case 1004:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 7");
                    return pc.GetStat(Stat.level_paladin) == 7;
                case 1005:
                case 1006:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 8");
                    return pc.GetStat(Stat.level_paladin) == 8;
                case 1007:
                case 1008:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 9");
                    return pc.GetStat(Stat.level_paladin) == 9;
                case 1009:
                case 1010:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 10");
                    return pc.GetStat(Stat.level_paladin) == 10;
                case 1041:
                case 1042:
                    Trace.Assert(originalScript == "pc.money_get() >= 300000");
                    return pc.GetMoney() >= 300000;
                case 1051:
                case 1052:
                    Trace.Assert(originalScript == "pc.money_get() >= 400000");
                    return pc.GetMoney() >= 400000;
                case 1061:
                case 1062:
                    Trace.Assert(originalScript == "pc.money_get() >= 500000");
                    return pc.GetMoney() >= 500000;
                case 1071:
                case 1072:
                    Trace.Assert(originalScript == "pc.money_get() >= 600000");
                    return pc.GetMoney() >= 600000;
                case 1081:
                case 1082:
                    Trace.Assert(originalScript == "pc.money_get() >= 700000");
                    return pc.GetMoney() >= 700000;
                case 1091:
                case 1092:
                    Trace.Assert(originalScript == "pc.money_get() >= 800000");
                    return pc.GetMoney() >= 800000;
                case 1101:
                case 1102:
                    Trace.Assert(originalScript == "pc.money_get() >= 900000");
                    return pc.GetMoney() >= 900000;
                case 1111:
                case 1112:
                    Trace.Assert(originalScript == "pc.money_get() >= 1000000");
                    return pc.GetMoney() >= 1000000;
                case 1173:
                case 1174:
                case 1203:
                case 1204:
                    Trace.Assert(originalScript == "game.areas[5] == 0");
                    return !IsAreaKnown(5);
                case 1251:
                case 1252:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 4 or pc.skill_level_get(npc,skill_bluff) >= 2");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 4 || pc.GetSkillLevel(npc, SkillId.bluff) >= 2;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 23:
                case 24:
                case 1253:
                case 1254:
                case 1301:
                case 1302:
                    Trace.Assert(originalScript == "game.global_vars[5] = 20");
                    SetGlobalVar(5, 20);
                    break;
                case 53:
                case 75:
                    Trace.Assert(originalScript == "game.fade_and_teleport(0,0,0,5001,494,226)");
                    FadeAndTeleport(0, 0, 0, 5001, 494, 226);
                    break;
                case 54:
                case 55:
                case 74:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
                    break;
                case 60:
                case 280:
                case 1250:
                    Trace.Assert(originalScript == "game.global_flags[67] = 1");
                    SetGlobalFlag(67, true);
                    break;
                case 61:
                case 62:
                case 1221:
                case 1222:
                    Trace.Assert(originalScript == "party_transfer_to( npc, 3003 )");
                    Utilities.party_transfer_to(npc, 3003);
                    break;
                case 65:
                case 66:
                    Trace.Assert(originalScript == "game.global_flags[22] = 1; game.quests[23].state = qs_botched");
                    SetGlobalFlag(22, true);
                    SetQuestState(23, QuestState.Botched);
                    ;
                    break;
                case 67:
                    Trace.Assert(originalScript == "game.global_flags[22] = 1; game.quests[23].state = qs_completed");
                    SetGlobalFlag(22, true);
                    SetQuestState(23, QuestState.Completed);
                    ;
                    break;
                case 70:
                    Trace.Assert(originalScript == "game.global_flags[67] = 1; game.quests[29].state = qs_completed");
                    SetGlobalFlag(67, true);
                    SetQuestState(29, QuestState.Completed);
                    ;
                    break;
                case 80:
                case 290:
                case 455:
                case 1290:
                    Trace.Assert(originalScript == "game.areas[2] = 1; game.story_state = 1");
                    MakeAreaKnown(2);
                    StoryState = 1;
                    ;
                    break;
                case 82:
                case 84:
                case 293:
                case 294:
                case 1151:
                case 1152:
                case 1161:
                case 1162:
                case 1293:
                case 1294:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(2)");
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 119:
                case 120:
                    Trace.Assert(originalScript == "game.quests[27].state = qs_completed");
                    SetQuestState(27, QuestState.Completed);
                    break;
                case 123:
                case 124:
                case 301:
                case 302:
                case 401:
                case 402:
                case 411:
                case 412:
                    Trace.Assert(originalScript == "game.quests[11].state = qs_accepted");
                    SetQuestState(11, QuestState.Accepted);
                    break;
                case 127:
                case 128:
                case 251:
                case 252:
                    Trace.Assert(originalScript == "game.quests[13].state = qs_accepted");
                    SetQuestState(13, QuestState.Accepted);
                    break;
                case 130:
                case 460:
                case 470:
                case 1270:
                    Trace.Assert(originalScript == "game.areas[3] = 1; game.story_state = 3");
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 131:
                case 132:
                case 461:
                case 462:
                case 471:
                case 472:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(3)");
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 190:
                    Trace.Assert(originalScript == "game.global_flags[27] = 1");
                    SetGlobalFlag(27, true);
                    break;
                case 200:
                case 350:
                    Trace.Assert(originalScript == "game.quests[11].state = qs_completed; game.global_vars[5] = game.global_vars[5] - 1");
                    SetQuestState(11, QuestState.Completed);
                    SetGlobalVar(5, GetGlobalVar(5) - 1);
                    ;
                    break;
                case 250:
                    Trace.Assert(originalScript == "game.quests[13].state = qs_mentioned");
                    SetQuestState(13, QuestState.Mentioned);
                    break;
                case 265:
                case 1190:
                    Trace.Assert(originalScript == "game.areas[5] = 1");
                    MakeAreaKnown(5);
                    break;
                case 275:
                case 276:
                case 283:
                case 284:
                case 291:
                case 292:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(5)");
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 300:
                    Trace.Assert(originalScript == "game.quests[11].state = qs_mentioned");
                    SetQuestState(11, QuestState.Mentioned);
                    break;
                case 305:
                case 1230:
                    Trace.Assert(originalScript == "game.global_vars[5] = game.global_vars[5] + 2");
                    SetGlobalVar(5, GetGlobalVar(5) + 2);
                    break;
                case 330:
                    Trace.Assert(originalScript == "game.global_vars[5] = game.global_vars[5] - 1");
                    SetGlobalVar(5, GetGlobalVar(5) - 1);
                    break;
                case 331:
                case 332:
                case 333:
                case 334:
                    Trace.Assert(originalScript == "party_transfer_to( npc, 3004 ); game.quests[13].state = qs_completed");
                    Utilities.party_transfer_to(npc, 3004);
                    SetQuestState(13, QuestState.Completed);
                    ;
                    break;
                case 335:
                case 336:
                    Trace.Assert(originalScript == "game.global_vars[5] = game.global_vars[5] + 3; npc.reaction_adj( pc,-10 ); game.quests[13].state = qs_botched");
                    SetGlobalVar(5, GetGlobalVar(5) + 3);
                    npc.AdjustReaction(pc, -10);
                    SetQuestState(13, QuestState.Botched);
                    ;
                    break;
                case 370:
                case 420:
                    Trace.Assert(originalScript == "game.global_flags[17] = 1");
                    SetGlobalFlag(17, true);
                    break;
                case 380:
                    Trace.Assert(originalScript == "game.global_flags[26] = 1");
                    SetGlobalFlag(26, true);
                    break;
                case 390:
                    Trace.Assert(originalScript == "game.global_flags[24] = 1");
                    SetGlobalFlag(24, true);
                    break;
                case 400:
                case 410:
                    Trace.Assert(originalScript == "game.quests[11].state = qs_mentioned; game.global_flags[25] = 1");
                    SetQuestState(11, QuestState.Mentioned);
                    SetGlobalFlag(25, true);
                    ;
                    break;
                case 530:
                    Trace.Assert(originalScript == "game.picker( npc, spell_raise_dead, should_resurrect_on, [ 585, 500, 540 ] )");
                    // FIXME: picker;
                    break;
                case 541:
                case 543:
                    Trace.Assert(originalScript == "pc.money_adj(-55000); npc.cast_spell( spell_raise_dead, picker_obj )");
                    pc.AdjustMoney(-55000);
                    npc.CastSpell(WellKnownSpells.RaiseDead, PickedObject);
                    ;
                    break;
                case 545:
                    Trace.Assert(originalScript == "npc.spells_pending_to_memorized()");
                    npc.PendingSpellsToMemorized();
                    break;
                case 550:
                    Trace.Assert(originalScript == "game.picker( npc, spell_neutralize_poison, should_heal_poison_on, [ 585, 500, 560 ] )");
                    // FIXME: picker;
                    break;
                case 561:
                case 563:
                    Trace.Assert(originalScript == "pc.money_adj(-24000); npc.cast_spell( spell_cure_neutralize_poison, picker_obj )");
                    pc.AdjustMoney(-24000);
                    npc.CastSpell(WellKnownSpells.NeutralizePoison, PickedObject);
                    ;
                    break;
                case 570:
                    Trace.Assert(originalScript == "game.picker( npc, spell_cure_serious_wounds, should_heal_hp_on, [ 585, 500, 580 ] )");
                    // FIXME: picker;
                    break;
                case 581:
                case 583:
                    Trace.Assert(originalScript == "pc.money_adj(-9000); npc.cast_spell( spell_cure_serious_wounds, picker_obj )");
                    pc.AdjustMoney(-9000);
                    npc.CastSpell(WellKnownSpells.CureSeriousWounds, PickedObject);
                    ;
                    break;
                case 590:
                    Trace.Assert(originalScript == "game.picker( npc, spell_raise_dead, should_resurrect_on, [ 585, 500, 600 ] )");
                    // FIXME: picker;
                    break;
                case 601:
                case 603:
                    Trace.Assert(originalScript == "pc.money_adj(-75000); npc.cast_spell( spell_raise_dead, picker_obj )");
                    pc.AdjustMoney(-75000);
                    npc.CastSpell(WellKnownSpells.RaiseDead, PickedObject);
                    ;
                    break;
                case 610:
                    Trace.Assert(originalScript == "game.picker( npc, spell_neutralize_poison, should_heal_poison_on, [ 585, 500, 620 ] )");
                    // FIXME: picker;
                    break;
                case 621:
                case 623:
                    Trace.Assert(originalScript == "pc.money_adj(-27000); npc.cast_spell( spell_cure_neutralize_poison, picker_obj )");
                    pc.AdjustMoney(-27000);
                    npc.CastSpell(WellKnownSpells.NeutralizePoison, PickedObject);
                    ;
                    break;
                case 630:
                    Trace.Assert(originalScript == "game.picker( npc, spell_cure_serious_wounds, should_heal_hp_on, [ 585, 500, 640 ] )");
                    // FIXME: picker;
                    break;
                case 641:
                case 643:
                    Trace.Assert(originalScript == "pc.money_adj(-12000); npc.cast_spell( spell_cure_serious_wounds, picker_obj )");
                    pc.AdjustMoney(-12000);
                    npc.CastSpell(WellKnownSpells.CureSeriousWounds, PickedObject);
                    ;
                    break;
                case 650:
                    Trace.Assert(originalScript == "game.picker( npc, spell_raise_dead, should_resurrect_on, [ 585, 500, 660 ] )");
                    // FIXME: picker;
                    break;
                case 661:
                case 663:
                    Trace.Assert(originalScript == "pc.money_adj(-100000); npc.cast_spell( spell_raise_dead, picker_obj )");
                    pc.AdjustMoney(-100000);
                    npc.CastSpell(WellKnownSpells.RaiseDead, PickedObject);
                    ;
                    break;
                case 670:
                    Trace.Assert(originalScript == "game.picker( npc, spell_neutralize_poison, should_heal_poison_on, [ 585, 500, 680 ] )");
                    // FIXME: picker;
                    break;
                case 681:
                case 683:
                    Trace.Assert(originalScript == "pc.money_adj(-30000); npc.cast_spell( spell_cure_neutralize_poison, picker_obj )");
                    pc.AdjustMoney(-30000);
                    npc.CastSpell(WellKnownSpells.NeutralizePoison, PickedObject);
                    ;
                    break;
                case 690:
                    Trace.Assert(originalScript == "game.picker( npc, spell_cure_serious_wounds, should_heal_hp_on, [ 585, 500, 700 ] )");
                    // FIXME: picker;
                    break;
                case 701:
                case 703:
                    Trace.Assert(originalScript == "pc.money_adj(-20000); npc.cast_spell( spell_cure_serious_wounds, picker_obj )");
                    pc.AdjustMoney(-20000);
                    npc.CastSpell(WellKnownSpells.CureSeriousWounds, PickedObject);
                    ;
                    break;
                case 710:
                    Trace.Assert(originalScript == "game.picker( npc, spell_raise_dead, should_resurrect_on, [ 585, 500, 720 ] )");
                    // FIXME: picker;
                    break;
                case 721:
                case 723:
                    Trace.Assert(originalScript == "pc.money_adj(-120000); npc.cast_spell( spell_raise_dead, picker_obj )");
                    pc.AdjustMoney(-120000);
                    npc.CastSpell(WellKnownSpells.RaiseDead, PickedObject);
                    ;
                    break;
                case 730:
                    Trace.Assert(originalScript == "game.picker( npc, spell_neutralize_poison, should_heal_poison_on, [ 585, 500, 740 ] )");
                    // FIXME: picker;
                    break;
                case 741:
                case 743:
                    Trace.Assert(originalScript == "pc.money_adj(-36000); npc.cast_spell( spell_cure_neutralize_poison, picker_obj )");
                    pc.AdjustMoney(-36000);
                    npc.CastSpell(WellKnownSpells.NeutralizePoison, PickedObject);
                    ;
                    break;
                case 750:
                    Trace.Assert(originalScript == "game.picker( npc, spell_cure_serious_wounds, should_heal_hp_on, [ 585, 500, 760 ] )");
                    // FIXME: picker;
                    break;
                case 761:
                case 763:
                    Trace.Assert(originalScript == "pc.money_adj(-27000); npc.cast_spell( spell_cure_serious_wounds, picker_obj )");
                    pc.AdjustMoney(-27000);
                    npc.CastSpell(WellKnownSpells.CureSeriousWounds, PickedObject);
                    ;
                    break;
                case 770:
                    Trace.Assert(originalScript == "game.picker( npc, spell_raise_dead, should_resurrect_on, [ 585, 500, 780 ] )");
                    // FIXME: picker;
                    break;
                case 781:
                case 783:
                    Trace.Assert(originalScript == "pc.money_adj(-140000); npc.cast_spell( spell_raise_dead, picker_obj )");
                    pc.AdjustMoney(-140000);
                    npc.CastSpell(WellKnownSpells.RaiseDead, PickedObject);
                    ;
                    break;
                case 790:
                    Trace.Assert(originalScript == "game.picker( npc, spell_neutralize_poison, should_heal_poison_on, [ 585, 500, 800 ] )");
                    // FIXME: picker;
                    break;
                case 801:
                case 803:
                    Trace.Assert(originalScript == "pc.money_adj(-40000); npc.cast_spell( spell_cure_neutralize_poison, picker_obj )");
                    pc.AdjustMoney(-40000);
                    npc.CastSpell(WellKnownSpells.NeutralizePoison, PickedObject);
                    ;
                    break;
                case 810:
                    Trace.Assert(originalScript == "game.picker( npc, spell_cure_serious_wounds, should_heal_hp_on, [ 585, 500, 820 ] )");
                    // FIXME: picker;
                    break;
                case 821:
                case 823:
                    Trace.Assert(originalScript == "pc.money_adj(-30000); npc.cast_spell( spell_cure_serious_wounds, picker_obj )");
                    pc.AdjustMoney(-30000);
                    npc.CastSpell(WellKnownSpells.CureSeriousWounds, PickedObject);
                    ;
                    break;
                case 830:
                    Trace.Assert(originalScript == "game.picker( npc, spell_raise_dead, should_resurrect_on, [ 585, 500, 840 ] )");
                    // FIXME: picker;
                    break;
                case 841:
                case 843:
                    Trace.Assert(originalScript == "pc.money_adj(-175000); npc.cast_spell( spell_raise_dead, picker_obj )");
                    pc.AdjustMoney(-175000);
                    npc.CastSpell(WellKnownSpells.RaiseDead, PickedObject);
                    ;
                    break;
                case 850:
                    Trace.Assert(originalScript == "game.picker( npc, spell_neutralize_poison, should_heal_poison_on, [ 585, 500, 860 ] )");
                    // FIXME: picker;
                    break;
                case 861:
                case 863:
                    Trace.Assert(originalScript == "pc.money_adj(-45000); npc.cast_spell( spell_cure_neutralize_poison, picker_obj )");
                    pc.AdjustMoney(-45000);
                    npc.CastSpell(WellKnownSpells.NeutralizePoison, PickedObject);
                    ;
                    break;
                case 870:
                    Trace.Assert(originalScript == "game.picker( npc, spell_cure_serious_wounds, should_heal_hp_on, [ 585, 500, 880 ] )");
                    // FIXME: picker;
                    break;
                case 881:
                case 883:
                    Trace.Assert(originalScript == "pc.money_adj(-35000); npc.cast_spell( spell_cure_serious_wounds, picker_obj )");
                    pc.AdjustMoney(-35000);
                    npc.CastSpell(WellKnownSpells.CureSeriousWounds, PickedObject);
                    ;
                    break;
                case 890:
                    Trace.Assert(originalScript == "game.picker( npc, spell_raise_dead, should_resurrect_on, [ 585, 500, 900 ] )");
                    // FIXME: picker;
                    break;
                case 901:
                case 903:
                    Trace.Assert(originalScript == "pc.money_adj(-200000); npc.cast_spell( spell_raise_dead, picker_obj )");
                    pc.AdjustMoney(-200000);
                    npc.CastSpell(WellKnownSpells.RaiseDead, PickedObject);
                    ;
                    break;
                case 910:
                    Trace.Assert(originalScript == "game.picker( npc, spell_neutralize_poison, should_heal_poison_on, [ 585, 500, 920 ] )");
                    // FIXME: picker;
                    break;
                case 921:
                case 923:
                    Trace.Assert(originalScript == "pc.money_adj(-50000); npc.cast_spell( spell_cure_neutralize_poison, picker_obj )");
                    pc.AdjustMoney(-50000);
                    npc.CastSpell(WellKnownSpells.NeutralizePoison, PickedObject);
                    ;
                    break;
                case 930:
                    Trace.Assert(originalScript == "game.picker( npc, spell_cure_serious_wounds, should_heal_hp_on, [ 585, 500, 940 ] )");
                    // FIXME: picker;
                    break;
                case 941:
                case 943:
                    Trace.Assert(originalScript == "pc.money_adj(-41000); npc.cast_spell( spell_cure_serious_wounds, picker_obj )");
                    pc.AdjustMoney(-41000);
                    npc.CastSpell(WellKnownSpells.CureSeriousWounds, PickedObject);
                    ;
                    break;
                case 1021:
                case 1022:
                    Trace.Assert(originalScript == "pc.money_adj(-100000)");
                    pc.AdjustMoney(-100000);
                    break;
                case 1031:
                case 1032:
                    Trace.Assert(originalScript == "pc.money_adj(-200000)");
                    pc.AdjustMoney(-200000);
                    break;
                case 1041:
                case 1042:
                    Trace.Assert(originalScript == "pc.money_adj(-300000)");
                    pc.AdjustMoney(-300000);
                    break;
                case 1051:
                case 1052:
                    Trace.Assert(originalScript == "pc.money_adj(-400000)");
                    pc.AdjustMoney(-400000);
                    break;
                case 1061:
                case 1062:
                    Trace.Assert(originalScript == "pc.money_adj(-500000)");
                    pc.AdjustMoney(-500000);
                    break;
                case 1071:
                case 1072:
                    Trace.Assert(originalScript == "pc.money_adj(-600000)");
                    pc.AdjustMoney(-600000);
                    break;
                case 1081:
                case 1082:
                    Trace.Assert(originalScript == "pc.money_adj(-700000)");
                    pc.AdjustMoney(-700000);
                    break;
                case 1091:
                case 1092:
                    Trace.Assert(originalScript == "pc.money_adj(-800000)");
                    pc.AdjustMoney(-800000);
                    break;
                case 1101:
                case 1102:
                    Trace.Assert(originalScript == "pc.money_adj(-900000)");
                    pc.AdjustMoney(-900000);
                    break;
                case 1111:
                case 1112:
                    Trace.Assert(originalScript == "pc.money_adj(-1000000)");
                    pc.AdjustMoney(-1000000);
                    break;
                case 1120:
                    Trace.Assert(originalScript == "pc.has_atoned()");
                    pc.AtoneFallenPaladin();
                    break;
                case 1241:
                case 1242:
                    Trace.Assert(originalScript == "game.quests[29].state = qs_completed");
                    SetQuestState(29, QuestState.Completed);
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
        {
            switch (lineNumber)
            {
                case 169:
                case 170:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 7);
                    return true;
                case 1251:
                case 1252:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 4, SkillId.bluff, 2);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
