
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
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 3";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 3;
                case 33:
                case 34:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 6";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 6;
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
                    originalScript = "game.quests[46].state >= qs_mentioned";
                    return GetQuestState(46) >= QuestState.Mentioned;
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
                case 273:
                case 274:
                case 385:
                case 386:
                case 393:
                case 394:
                    originalScript = "game.quests[52].state == qs_unknown";
                    return GetQuestState(52) == QuestState.Unknown;
                case 281:
                case 282:
                    originalScript = "game.global_flags[115] == 1 and game.global_flags[116] == 1";
                    return GetGlobalFlag(115) && GetGlobalFlag(116);
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
                    originalScript = "( ( game.global_flags[119] == 0 ) and ( game.global_flags[88] == 1 ) and ( game.quests[44].state == qs_accepted ) ) or ( game.global_flags[124] == 1 )";
                    return ((!GetGlobalFlag(119)) && (GetGlobalFlag(88)) && (GetQuestState(44) == QuestState.Accepted)) || (GetGlobalFlag(124));
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
                    originalScript = "game.quests[44].state == qs_mentioned and game.global_flags[119] == 1";
                    return GetQuestState(44) == QuestState.Mentioned && GetGlobalFlag(119);
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
                    originalScript = "game.quests[46].state == qs_unknown";
                    return GetQuestState(46) == QuestState.Unknown;
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
                case 403:
                case 404:
                    originalScript = "game.quests[44].state == qs_completed and game.global_flags[119] == 1";
                    return GetQuestState(44) == QuestState.Completed && GetGlobalFlag(119);
                case 407:
                case 408:
                    originalScript = "game.quests[44].state == qs_completed and game.global_flags[119] == 0";
                    return GetQuestState(44) == QuestState.Completed && !GetGlobalFlag(119);
                case 409:
                    originalScript = "game.global_flags[105] == 1";
                    return GetGlobalFlag(105);
                case 452:
                case 453:
                    originalScript = "game.quests[43].state == qs_unknown";
                    return GetQuestState(43) == QuestState.Unknown;
                case 463:
                case 464:
                    originalScript = "pc.skill_level_get(npc, skill_bluff) >= 10";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 10;
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
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 10:
                case 100:
                case 220:
                    originalScript = "game.story_state = 5";
                    StoryState = 5;
                    break;
                case 35:
                case 36:
                    originalScript = "game.global_flags[111] = 1";
                    SetGlobalFlag(111, true);
                    break;
                case 120:
                    originalScript = "game.quests[43].state = qs_mentioned";
                    SetQuestState(43, QuestState.Mentioned);
                    break;
                case 123:
                case 124:
                    originalScript = "game.quests[43].state = qs_completed; game.global_flags[347] = 1";
                    SetQuestState(43, QuestState.Completed);
                    SetGlobalFlag(347, true);
                    ;
                    break;
                case 131:
                case 141:
                case 171:
                case 183:
                case 184:
                case 307:
                case 308:
                    originalScript = "game.quests[43].state = qs_accepted";
                    SetQuestState(43, QuestState.Accepted);
                    break;
                case 160:
                    originalScript = "game.global_flags[347] = 1";
                    SetGlobalFlag(347, true);
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
                case 283:
                case 284:
                case 313:
                case 314:
                case 323:
                case 324:
                    originalScript = "game.quests[44].state = qs_accepted";
                    SetQuestState(44, QuestState.Accepted);
                    break;
                case 250:
                    originalScript = "npc.item_transfer_to( pc, 5807 )";
                    npc.TransferItemByNameTo(pc, 5807);
                    break;
                case 280:
                    originalScript = "game.global_flags[119] = 1";
                    SetGlobalFlag(119, true);
                    break;
                case 281:
                case 282:
                case 317:
                case 318:
                case 319:
                case 320:
                    originalScript = "game.quests[44].state = qs_completed";
                    SetQuestState(44, QuestState.Completed);
                    break;
                case 309:
                case 310:
                case 311:
                case 312:
                    originalScript = "game.quests[43].state = qs_completed";
                    SetQuestState(43, QuestState.Completed);
                    break;
                case 329:
                case 330:
                case 401:
                case 402:
                case 431:
                case 432:
                case 491:
                case 492:
                    originalScript = "game.quests[45].state = qs_accepted";
                    SetQuestState(45, QuestState.Accepted);
                    break;
                case 331:
                case 332:
                case 409:
                case 423:
                case 424:
                    originalScript = "game.quests[45].state = qs_completed";
                    SetQuestState(45, QuestState.Completed);
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
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 3);
                    return true;
                case 33:
                case 34:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 6);
                    return true;
                case 463:
                case 464:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
