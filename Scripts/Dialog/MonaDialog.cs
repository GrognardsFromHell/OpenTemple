
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
    [DialogScript(213)]
    public class MonaDialog : Mona, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 6:
                case 473:
                case 474:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == NEUTRAL_GOOD) or (game.party_alignment == LAWFUL_NEUTRAL) or (game.party_alignment == TRUE_NEUTRAL) or (game.party_alignment == LAWFUL_EVIL)";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.NEUTRAL_GOOD) || (PartyAlignment == Alignment.LAWFUL_NEUTRAL) || (PartyAlignment == Alignment.NEUTRAL) || (PartyAlignment == Alignment.LAWFUL_EVIL);
                case 3:
                case 7:
                    originalScript = "(game.party_alignment == CHAOTIC_GOOD) or (game.party_alignment == CHAOTIC_NEUTRAL) or (game.party_alignment == TRUE_NEUTRAL) or (game.party_alignment == CHAOTIC_EVIL) or (game.party_alignment == NEUTRAL_EVIL)";
                    return (PartyAlignment == Alignment.CHAOTIC_GOOD) || (PartyAlignment == Alignment.CHAOTIC_NEUTRAL) || (PartyAlignment == Alignment.NEUTRAL) || (PartyAlignment == Alignment.CHAOTIC_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL);
                case 5:
                case 9:
                    originalScript = "game.quests[59].state == qs_accepted";
                    return GetQuestState(59) == QuestState.Accepted;
                case 23:
                case 24:
                case 31:
                case 32:
                case 45:
                case 46:
                case 269:
                case 270:
                case 303:
                case 304:
                    originalScript = "game.quests[59].state == qs_accepted and game.global_flags[315] == 0";
                    return GetQuestState(59) == QuestState.Accepted && !GetGlobalFlag(315);
                case 81:
                case 82:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 8";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 8;
                case 103:
                case 104:
                case 207:
                case 208:
                case 336:
                case 337:
                    originalScript = "game.global_flags[314] == 0 and game.quests[59].state == qs_accepted";
                    return !GetGlobalFlag(314) && GetQuestState(59) == QuestState.Accepted;
                case 105:
                case 106:
                    originalScript = "game.global_flags[314] == 1 and game.quests[59].state == qs_accepted";
                    return GetGlobalFlag(314) && GetQuestState(59) == QuestState.Accepted;
                case 107:
                case 108:
                case 333:
                case 334:
                case 475:
                case 476:
                    originalScript = "game.quests[60].state == qs_mentioned";
                    return GetQuestState(60) == QuestState.Mentioned;
                case 143:
                case 144:
                case 381:
                case 382:
                case 383:
                case 384:
                    originalScript = "pc.money_get() >= 50000";
                    return pc.GetMoney() >= 50000;
                case 145:
                case 146:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 8015 ) and (game.party_alignment == LAWFUL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL)";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8015)) && (PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL);
                case 191:
                case 192:
                    originalScript = "game.global_flags[356] == 1";
                    return GetGlobalFlag(356);
                case 193:
                case 194:
                    originalScript = "game.global_flags[356] == 0";
                    return !GetGlobalFlag(356);
                case 203:
                case 204:
                case 205:
                case 206:
                case 235:
                case 236:
                case 263:
                case 264:
                case 265:
                case 266:
                case 281:
                case 282:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 5815 )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(5815));
                case 209:
                case 210:
                case 338:
                case 339:
                    originalScript = "game.global_flags[314] == 1 and game.quests[59].state == qs_accepted and game.global_flags[315] == 0";
                    return GetGlobalFlag(314) && GetQuestState(59) == QuestState.Accepted && !GetGlobalFlag(315);
                case 212:
                case 213:
                    originalScript = "game.quests[60].state == qs_accepted and anyone( pc.group_list(), \"has_item\", 5815 ) == 0";
                    return GetQuestState(60) == QuestState.Accepted && !pc.GetPartyMembers().Any(o => o.HasItemByName(5815));
                case 214:
                case 271:
                case 307:
                case 348:
                case 411:
                case 485:
                    originalScript = "game.quests[90].state == qs_mentioned";
                    return GetQuestState(90) == QuestState.Mentioned;
                case 215:
                case 272:
                case 308:
                case 349:
                case 412:
                case 486:
                    originalScript = "game.quests[90].state == qs_accepted and game.global_flags[929] == 1";
                    return GetQuestState(90) == QuestState.Accepted && GetGlobalFlag(929);
                case 231:
                case 232:
                    originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 10";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 10;
                case 237:
                case 238:
                case 261:
                case 262:
                    originalScript = "(game.party_alignment == CHAOTIC_EVIL) or (game.party_alignment == NEUTRAL_EVIL)";
                    return (PartyAlignment == Alignment.CHAOTIC_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL);
                case 267:
                case 268:
                    originalScript = "game.quests[60].state == qs_botched and anyone( pc.group_list(), \"has_item\", 5815 ) == 0";
                    return GetQuestState(60) == QuestState.Botched && !pc.GetPartyMembers().Any(o => o.HasItemByName(5815));
                case 331:
                case 332:
                    originalScript = "game.quests[60].state == qs_unknown and game.quests[35].state == qs_completed";
                    return GetQuestState(60) == QuestState.Unknown && GetQuestState(35) == QuestState.Completed;
                case 335:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 5815 ) and game.quests[60].state == qs_accepted";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(5815)) && GetQuestState(60) == QuestState.Accepted;
                case 340:
                case 341:
                    originalScript = "game.quests[60].state == qs_completed and game.global_flags[315] == 0";
                    return GetQuestState(60) == QuestState.Completed && !GetGlobalFlag(315);
                case 342:
                case 343:
                    originalScript = "game.quests[33].state == qs_completed and game.global_flags[315] == 1 and game.global_flags[318] == 0";
                    return GetQuestState(33) == QuestState.Completed && GetGlobalFlag(315) && !GetGlobalFlag(318);
                case 344:
                case 345:
                    originalScript = "game.quests[33].state == qs_completed and game.global_flags[315] == 1 and game.global_flags[318] == 1";
                    return GetQuestState(33) == QuestState.Completed && GetGlobalFlag(315) && GetGlobalFlag(318);
                case 346:
                case 347:
                    originalScript = "game.quests[60].state == qs_unknown and game.quests[35].state != qs_completed";
                    return GetQuestState(60) == QuestState.Unknown && GetQuestState(35) != QuestState.Completed;
                case 403:
                case 404:
                    originalScript = "game.global_flags[316] == 0 and game.story_state <= 4";
                    return !GetGlobalFlag(316) && StoryState <= 4;
                case 405:
                case 406:
                    originalScript = "game.global_flags[316] == 0 and game.story_state == 5";
                    return !GetGlobalFlag(316) && StoryState == 5;
                case 407:
                case 408:
                    originalScript = "game.global_flags[316] == 0 and game.story_state >= 6";
                    return !GetGlobalFlag(316) && StoryState >= 6;
                case 409:
                case 410:
                    originalScript = "game.global_flags[316] == 1";
                    return GetGlobalFlag(316);
                case 431:
                case 432:
                    originalScript = "game.story_state == 3";
                    return StoryState == 3;
                case 433:
                case 434:
                    originalScript = "game.story_state == 4 and (game.global_flags[133] == 0 and game.global_flags[355] == 0)";
                    return StoryState == 4 && (!GetGlobalFlag(133) && !GetGlobalFlag(355));
                case 441:
                case 442:
                    originalScript = "game.global_flags[133] == 0 and game.global_flags[355] == 0";
                    return !GetGlobalFlag(133) && !GetGlobalFlag(355);
                case 471:
                case 472:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 8 and game.global_flags[317] == 0";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 8 && !GetGlobalFlag(317);
                case 477:
                case 478:
                    originalScript = "game.quests[59].state == qs_accepted and game.global_flags[314] == 0";
                    return GetQuestState(59) == QuestState.Accepted && !GetGlobalFlag(314);
                case 479:
                case 480:
                    originalScript = "game.quests[59].state == qs_accepted and game.global_flags[314] == 1 and game.global_flags[315] == 0";
                    return GetQuestState(59) == QuestState.Accepted && GetGlobalFlag(314) && !GetGlobalFlag(315);
                case 481:
                case 482:
                    originalScript = "game.quests[35].state == qs_completed";
                    return GetQuestState(35) == QuestState.Completed;
                case 505:
                case 511:
                    originalScript = "(game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == NEUTRAL_GOOD) or (game.party_alignment == LAWFUL_NEUTRAL) or (game.party_alignment == CHAOTIC_GOOD)";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.NEUTRAL_GOOD) || (PartyAlignment == Alignment.LAWFUL_NEUTRAL) || (PartyAlignment == Alignment.CHAOTIC_GOOD);
                case 506:
                case 512:
                    originalScript = "(game.party_alignment == LAWFUL_EVIL) or (game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL) or (game.party_alignment == CHAOTIC_NEUTRAL)";
                    return (PartyAlignment == Alignment.LAWFUL_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL) || (PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    originalScript = "game.global_flags[321] = 1";
                    SetGlobalFlag(321, true);
                    break;
                case 50:
                    originalScript = "game.global_flags[314] = 1";
                    SetGlobalFlag(314, true);
                    break;
                case 107:
                case 108:
                case 191:
                case 192:
                case 193:
                case 194:
                case 333:
                case 334:
                case 475:
                case 476:
                    originalScript = "game.quests[60].state = qs_accepted";
                    SetQuestState(60, QuestState.Accepted);
                    break;
                case 143:
                case 144:
                case 381:
                case 382:
                case 383:
                case 384:
                    originalScript = "pc.money_adj(-50000)";
                    pc.AdjustMoney(-50000);
                    break;
                case 145:
                case 146:
                    originalScript = "buttin(npc,pc,320)";
                    buttin(npc, pc, 320);
                    break;
                case 160:
                case 460:
                    originalScript = "game.global_flags[315] = 1";
                    SetGlobalFlag(315, true);
                    break;
                case 180:
                    originalScript = "game.quests[60].state = qs_mentioned";
                    SetQuestState(60, QuestState.Mentioned);
                    break;
                case 205:
                case 206:
                case 235:
                case 236:
                case 263:
                case 264:
                case 281:
                case 282:
                    originalScript = "party_transfer_to( npc, 5815 )";
                    Utilities.party_transfer_to(npc, 5815);
                    break;
                case 212:
                case 213:
                case 230:
                    originalScript = "game.quests[60].state = qs_botched";
                    SetQuestState(60, QuestState.Botched);
                    break;
                case 237:
                case 238:
                case 261:
                case 262:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 286:
                    originalScript = "game.quests[60].unbotch(); game.quests[60].state = qs_completed; game.global_flags[317] = 1";
                    UnbotchQuest(60);
                    SetQuestState(60, QuestState.Completed);
                    SetGlobalFlag(317, true);
                    ;
                    break;
                case 290:
                    originalScript = "game.quests[60].state = qs_completed; game.global_flags[317] = 1";
                    SetQuestState(60, QuestState.Completed);
                    SetGlobalFlag(317, true);
                    ;
                    break;
                case 300:
                case 481:
                case 482:
                    originalScript = "game.global_flags[317] = 1";
                    SetGlobalFlag(317, true);
                    break;
                case 430:
                case 440:
                case 450:
                    originalScript = "game.global_flags[316] = 1";
                    SetGlobalFlag(316, true);
                    break;
                case 490:
                    originalScript = "game.areas[4] = 1; game.story_state = 4";
                    MakeAreaKnown(4);
                    StoryState = 4;
                    ;
                    break;
                case 503:
                case 509:
                    originalScript = "game.global_flags[352] = 1";
                    SetGlobalFlag(352, true);
                    break;
                case 504:
                case 510:
                    originalScript = "game.global_flags[351] = 1";
                    SetGlobalFlag(351, true);
                    break;
                case 505:
                case 511:
                    originalScript = "game.global_flags[353] = 1";
                    SetGlobalFlag(353, true);
                    break;
                case 506:
                case 512:
                    originalScript = "game.global_flags[354] = 1";
                    SetGlobalFlag(354, true);
                    break;
                case 541:
                    originalScript = "gremlich_movie_setup(npc,pc)";
                    gremlich_movie_setup(npc, pc);
                    break;
                case 601:
                    originalScript = "game.quests[90].state = qs_accepted";
                    SetQuestState(90, QuestState.Accepted);
                    break;
                case 611:
                case 612:
                case 613:
                    originalScript = "schedule_gremlich(npc,pc)";
                    schedule_gremlich(npc, pc);
                    break;
                case 620:
                    originalScript = "game.quests[90].state = qs_completed";
                    SetQuestState(90, QuestState.Completed);
                    break;
                case 640:
                    originalScript = "pc.money_adj(800)";
                    pc.AdjustMoney(800);
                    break;
                case 650:
                    originalScript = "game.global_vars[924] = 1; create_item_in_inventory(11010,pc)";
                    SetGlobalVar(924, 1);
                    Utilities.create_item_in_inventory(11010, pc);
                    ;
                    break;
                case 660:
                    originalScript = "encounter_picker(npc,pc)";
                    encounter_picker(npc, pc);
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
                case 81:
                case 82:
                case 471:
                case 472:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 8);
                    return true;
                case 231:
                case 232:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
