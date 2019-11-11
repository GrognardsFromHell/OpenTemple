
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

namespace Scripts.Dialog
{
    [DialogScript(119)]
    public class RomagDialog : Romag, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 4:
                case 6:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 3010 )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(3010));
                case 31:
                case 32:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 5";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 5;
                case 33:
                case 34:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 8";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 8;
                case 111:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_male";
                    return pc.GetGender() == Gender.Male;
                case 113:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_female";
                    return pc.GetGender() == Gender.Female;
                case 123:
                case 124:
                    originalScript = "game.global_flags[117] == 1";
                    return GetGlobalFlag(117);
                case 132:
                    originalScript = "game.quests[46].state >= qs_mentioned and game.global_flags[105] == 0";
                    return GetQuestState(46) >= QuestState.Mentioned && !GetGlobalFlag(105);
                case 201:
                case 202:
                case 203:
                case 204:
                    originalScript = "not npc.has_met( pc )";
                    return !npc.HasMet(pc);
                case 205:
                case 206:
                    originalScript = "npc.has_met( pc )";
                    return npc.HasMet(pc);
                case 245:
                case 246:
                    originalScript = "game.global_flags[88] == 1";
                    return GetGlobalFlag(88);
                case 271:
                case 272:
                case 391:
                case 392:
                    originalScript = "game.global_flags[107] == 0";
                    return !GetGlobalFlag(107);
                case 273:
                case 274:
                case 385:
                case 386:
                case 393:
                case 394:
                    originalScript = "game.quests[52].state == qs_unknown and game.global_flags[107] == 0";
                    return GetQuestState(52) == QuestState.Unknown && !GetGlobalFlag(107);
                case 275:
                case 287:
                case 387:
                case 395:
                    originalScript = "game.global_flags[107] == 1";
                    return GetGlobalFlag(107);
                case 281:
                case 282:
                    originalScript = "game.global_flags[115] == 1 and game.global_flags[116] == 1";
                    return GetGlobalFlag(115) && GetGlobalFlag(116);
                case 283:
                case 284:
                case 285:
                case 286:
                    originalScript = "game.global_flags[115] == 0 and game.global_flags[116] == 0";
                    return !GetGlobalFlag(115) && !GetGlobalFlag(116);
                case 301:
                case 302:
                    originalScript = "game.quests[43].state == qs_unknown and game.global_flags[111] == 0";
                    return GetQuestState(43) == QuestState.Unknown && !GetGlobalFlag(111);
                case 307:
                case 308:
                    originalScript = "game.quests[43].state == qs_mentioned and game.global_flags[117] == 0";
                    return GetQuestState(43) == QuestState.Mentioned && !GetGlobalFlag(117);
                case 309:
                case 310:
                    originalScript = "game.global_flags[117] == 1 and game.quests[43].state == qs_accepted";
                    return GetGlobalFlag(117) && GetQuestState(43) == QuestState.Accepted;
                case 311:
                case 312:
                    originalScript = "game.global_flags[117] == 1 and game.quests[43].state == qs_mentioned";
                    return GetGlobalFlag(117) && GetQuestState(43) == QuestState.Mentioned;
                case 313:
                case 314:
                    originalScript = "game.quests[44].state == qs_mentioned and game.global_flags[119] == 0";
                    return GetQuestState(44) == QuestState.Mentioned && !GetGlobalFlag(119);
                case 315:
                case 316:
                    originalScript = "( game.global_flags[119] == 0 and game.global_flags[88] == 1 and game.quests[44].state == qs_accepted ) or game.global_flags[124] == 1";
                    return (!GetGlobalFlag(119) && GetGlobalFlag(88) && GetQuestState(44) == QuestState.Accepted) || GetGlobalFlag(124);
                case 317:
                case 318:
                    originalScript = "game.global_flags[115] == 1 and game.global_flags[116] == 1 and game.global_flags[119] == 1 and game.quests[45].state == qs_unknown";
                    return GetGlobalFlag(115) && GetGlobalFlag(116) && GetGlobalFlag(119) && GetQuestState(45) == QuestState.Unknown;
                case 319:
                case 320:
                    originalScript = "game.global_flags[120] == 1 and game.quests[44].state == qs_accepted";
                    return GetGlobalFlag(120) && GetQuestState(44) == QuestState.Accepted;
                case 321:
                case 322:
                    originalScript = "game.global_flags[119] == 0 and game.global_flags[88] == 1 and game.quests[44].state == qs_mentioned";
                    return !GetGlobalFlag(119) && GetGlobalFlag(88) && GetQuestState(44) == QuestState.Mentioned;
                case 323:
                case 324:
                    originalScript = "game.quests[94].state == qs_mentioned and game.global_flags[119] == 1";
                    return GetQuestState(94) == QuestState.Mentioned && GetGlobalFlag(119);
                case 325:
                case 326:
                    originalScript = "game.quests[43].state == qs_unknown and game.global_flags[111] == 1";
                    return GetQuestState(43) == QuestState.Unknown && GetGlobalFlag(111);
                case 327:
                case 328:
                    originalScript = "( game.global_flags[119] == 0 ) and ( game.quests[44].state == qs_accepted ) and ( not anyone( pc.group_list(), \"has_item\", 5807 ) ) and ( game.global_flags[120] == 0 )";
                    return (!GetGlobalFlag(119)) && (GetQuestState(44) == QuestState.Accepted) && (!pc.GetPartyMembers().Any(o => o.HasItemByName(5807))) && (!GetGlobalFlag(120));
                case 329:
                case 330:
                    originalScript = "game.quests[45].state == qs_mentioned";
                    return GetQuestState(45) == QuestState.Mentioned;
                case 331:
                case 332:
                    originalScript = "(game.quests[45].state == qs_accepted or game.quests[45].state == qs_mentioned) and game.global_flags[105] == 1";
                    return (GetQuestState(45) == QuestState.Accepted || GetQuestState(45) == QuestState.Mentioned) && GetGlobalFlag(105);
                case 333:
                case 334:
                    originalScript = "game.quests[45].state == qs_completed";
                    return GetQuestState(45) == QuestState.Completed;
                case 351:
                case 354:
                    originalScript = "game.quests[46].state == qs_unknown and game.global_flags[105] == 0";
                    return GetQuestState(46) == QuestState.Unknown && !GetGlobalFlag(105);
                case 381:
                case 382:
                case 411:
                case 412:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 5807 )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(5807));
                case 383:
                case 384:
                    originalScript = "not anyone( pc.group_list(), \"has_item\", 5807 )";
                    return !pc.GetPartyMembers().Any(o => o.HasItemByName(5807));
                case 401:
                case 402:
                case 405:
                case 406:
                    originalScript = "game.global_flags[105] == 0";
                    return !GetGlobalFlag(105);
                case 403:
                case 404:
                    originalScript = "game.quests[94].state == qs_completed and game.global_flags[119] == 1 and game.global_flags[105] == 0";
                    return GetQuestState(94) == QuestState.Completed && GetGlobalFlag(119) && !GetGlobalFlag(105);
                case 407:
                case 408:
                    originalScript = "game.quests[44].state == qs_completed and game.global_flags[119] == 0 and game.global_flags[105] == 0";
                    return GetQuestState(44) == QuestState.Completed && !GetGlobalFlag(119) && !GetGlobalFlag(105);
                case 409:
                    originalScript = "game.global_flags[105] == 1";
                    return GetGlobalFlag(105);
                case 452:
                case 453:
                    originalScript = "game.quests[43].state == qs_unknown";
                    return GetQuestState(43) == QuestState.Unknown;
                case 463:
                case 464:
                    originalScript = "pc.skill_level_get(npc, skill_bluff) >= 12";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 12;
                case 541:
                    originalScript = "game.global_vars[454] & (2**8) == 0";
                    return (GetGlobalVar(454) & (0x100)) == 0;
                case 543:
                    originalScript = "game.global_vars[454] & (2**8) != 0";
                    return (GetGlobalVar(454) & (0x100)) != 0;
                case 591:
                    originalScript = "game.global_flags[107] == 1 or game.global_flags[105] == 1 or game.global_flags[106] == 1";
                    return GetGlobalFlag(107) || GetGlobalFlag(105) || GetGlobalFlag(106);
                case 592:
                    originalScript = "game.global_flags[107] == 0 and game.global_flags[105] == 0 and game.global_flags[106] == 0";
                    return !GetGlobalFlag(107) && !GetGlobalFlag(105) && !GetGlobalFlag(106);
                case 601:
                case 612:
                case 613:
                    originalScript = "game.global_flags[107] == 1 and game.global_flags[105] == 0";
                    return GetGlobalFlag(107) && !GetGlobalFlag(105);
                case 602:
                case 621:
                case 624:
                    originalScript = "game.global_flags[107] == 0 and game.global_flags[105] == 1";
                    return !GetGlobalFlag(107) && GetGlobalFlag(105);
                case 603:
                case 611:
                case 625:
                    originalScript = "game.global_flags[107] == 1 and game.global_flags[105] == 1";
                    return GetGlobalFlag(107) && GetGlobalFlag(105);
                case 604:
                    originalScript = "game.global_flags[107] == 0 and game.global_flags[105] == 0";
                    return !GetGlobalFlag(107) && !GetGlobalFlag(105);
                case 622:
                    originalScript = "game.global_flags[106] == 0 and game.global_flags[107] == 1 and game.global_flags[105] == 1";
                    return !GetGlobalFlag(106) && GetGlobalFlag(107) && GetGlobalFlag(105);
                case 623:
                    originalScript = "game.global_flags[106] == 1 and game.global_flags[107] == 1 and game.global_flags[105] == 1";
                    return GetGlobalFlag(106) && GetGlobalFlag(107) && GetGlobalFlag(105);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 7:
                case 8:
                case 25:
                case 26:
                case 41:
                case 42:
                case 53:
                case 54:
                case 71:
                case 72:
                case 81:
                case 82:
                case 101:
                case 102:
                case 153:
                case 154:
                case 201:
                case 202:
                case 205:
                case 206:
                case 211:
                case 212:
                case 305:
                case 306:
                case 481:
                case 482:
                case 641:
                case 651:
                case 652:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 10:
                case 100:
                case 220:
                case 600:
                    originalScript = "game.story_state = 5";
                    StoryState = 5;
                    break;
                case 35:
                case 36:
                    originalScript = "game.global_flags[111] = 1;  switch_to_gatekeeper(pc, 1800)";
                    SetGlobalFlag(111, true);
                    Earthcombat.switch_to_gatekeeper(pc, 1800);
                    ;
                    break;
                case 73:
                case 74:
                case 161:
                case 251:
                case 261:
                case 291:
                case 303:
                case 304:
                case 371:
                case 442:
                case 451:
                case 483:
                case 484:
                case 501:
                    originalScript = "switch_to_gatekeeper(pc, 1800)";
                    Earthcombat.switch_to_gatekeeper(pc, 1800);
                    break;
                case 120:
                    originalScript = "game.quests[43].state = qs_mentioned";
                    SetQuestState(43, QuestState.Mentioned);
                    break;
                case 123:
                case 124:
                    originalScript = "game.quests[43].state = qs_completed; game.global_flags[347] = 1; record_time_stamp(468)";
                    SetQuestState(43, QuestState.Completed);
                    SetGlobalFlag(347, true);
                    ScriptDaemon.record_time_stamp(468);
                    ;
                    break;
                case 131:
                    originalScript = "record_time_stamp(467)";
                    ScriptDaemon.record_time_stamp(467);
                    break;
                case 160:
                    originalScript = "game.global_flags[347] = 1; game.quests[43].state = qs_accepted; record_time_stamp(467)";
                    SetGlobalFlag(347, true);
                    SetQuestState(43, QuestState.Accepted);
                    ScriptDaemon.record_time_stamp(467);
                    ;
                    break;
                case 191:
                case 461:
                case 462:
                    originalScript = "game.global_vars[454] |= ( (game.global_vars[454] & (2**8)) != 0) * (2**9); switch_to_gatekeeper(pc, 1900)";
                    SetGlobalVar(454, GetGlobalVar(454) | (((GetGlobalVar(454) & (0x100)) != 0) ? 0x200 : 0));
                    Earthcombat.switch_to_gatekeeper(pc, 1900);
                    ;
                    break;
                case 230:
                    originalScript = "pc.reputation_add( 11 )";
                    pc.AddReputation(11);
                    break;
                case 240:
                    originalScript = "game.areas[3] = 1; game.quests[44].state = qs_mentioned";
                    MakeAreaKnown(3);
                    SetQuestState(44, QuestState.Mentioned);
                    ;
                    break;
                case 241:
                case 242:
                case 313:
                    originalScript = "game.quests[44].state = qs_accepted; record_time_stamp(469)";
                    SetQuestState(44, QuestState.Accepted);
                    ScriptDaemon.record_time_stamp(469);
                    ;
                    break;
                case 250:
                    originalScript = "npc.item_transfer_to( pc, 5807 )";
                    npc.TransferItemByNameTo(pc, 5807);
                    break;
                case 280:
                    originalScript = "game.global_flags[119] = 1; game.quests[94].state = qs_mentioned; game.quests[44].state = qs_botched";
                    SetGlobalFlag(119, true);
                    SetQuestState(94, QuestState.Mentioned);
                    SetQuestState(44, QuestState.Botched);
                    ;
                    break;
                case 281:
                case 282:
                case 317:
                case 318:
                    originalScript = "game.quests[94].state = qs_completed; record_time_stamp(470)";
                    SetQuestState(94, QuestState.Completed);
                    ScriptDaemon.record_time_stamp(470);
                    ;
                    break;
                case 283:
                case 284:
                case 323:
                case 324:
                    originalScript = "game.quests[94].state = qs_accepted; record_time_stamp(469)";
                    SetQuestState(94, QuestState.Accepted);
                    ScriptDaemon.record_time_stamp(469);
                    ;
                    break;
                case 307:
                case 308:
                    originalScript = "game.quests[43].state = qs_accepted; record_time_stamp(467)";
                    SetQuestState(43, QuestState.Accepted);
                    ScriptDaemon.record_time_stamp(467);
                    ;
                    break;
                case 309:
                case 310:
                case 311:
                case 312:
                    originalScript = "game.quests[43].state = qs_completed; record_time_stamp(468)";
                    SetQuestState(43, QuestState.Completed);
                    ScriptDaemon.record_time_stamp(468);
                    ;
                    break;
                case 314:
                    originalScript = "game.quests[44].state = qs_accepted; record_time_stamp(468)";
                    SetQuestState(44, QuestState.Accepted);
                    ScriptDaemon.record_time_stamp(468);
                    ;
                    break;
                case 319:
                case 320:
                    originalScript = "game.quests[44].state = qs_completed; record_time_stamp(470)";
                    SetQuestState(44, QuestState.Completed);
                    ScriptDaemon.record_time_stamp(470);
                    ;
                    break;
                case 329:
                case 330:
                case 401:
                case 402:
                case 431:
                case 432:
                case 491:
                case 492:
                    originalScript = "game.quests[45].state = qs_accepted; record_time_stamp(471)";
                    SetQuestState(45, QuestState.Accepted);
                    ScriptDaemon.record_time_stamp(471);
                    ;
                    break;
                case 331:
                case 332:
                    originalScript = "game.quests[45].state = qs_completed; record_time_stamp(472)";
                    SetQuestState(45, QuestState.Completed);
                    ScriptDaemon.record_time_stamp(472);
                    ;
                    break;
                case 381:
                case 382:
                case 411:
                case 412:
                    originalScript = "party_transfer_to( npc, 5807 )";
                    Utilities.party_transfer_to(npc, 5807);
                    break;
                case 400:
                case 430:
                    originalScript = "game.quests[45].state = qs_mentioned";
                    SetQuestState(45, QuestState.Mentioned);
                    break;
                case 409:
                    originalScript = "game.quests[45].state = qs_accepted; game.quests[45].state = qs_completed; record_time_stamp(472)";
                    SetQuestState(45, QuestState.Accepted);
                    SetQuestState(45, QuestState.Completed);
                    ScriptDaemon.record_time_stamp(472);
                    ;
                    break;
                case 551:
                    originalScript = "escort_below(npc, pc)";
                    escort_below(npc, pc);
                    break;
                case 571:
                    originalScript = "talk_Hedrack(npc,pc, 90)";
                    talk_Hedrack(npc, pc, 90);
                    break;
                case 581:
                case 582:
                    originalScript = "npc.object_flag_set(OF_OFF)";
                    npc.SetObjectFlag(ObjectFlag.OFF);
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
                case 31:
                case 32:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 5);
                    return true;
                case 33:
                case 34:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 8);
                    return true;
                case 463:
                case 464:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 12);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
