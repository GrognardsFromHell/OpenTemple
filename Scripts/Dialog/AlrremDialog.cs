
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
    [DialogScript(122)]
    public class AlrremDialog : Alrrem, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 4:
                case 5:
                case 803:
                case 804:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 10;
                case 7:
                case 8:
                case 806:
                case 807:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_diplomacy) >= 13");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 13;
                case 9:
                case 10:
                case 808:
                case 809:
                    Trace.Assert(originalScript == "game.quests[47].state == qs_mentioned or game.quests[47].state == qs_accepted");
                    return GetQuestState(47) == QuestState.Mentioned || GetQuestState(47) == QuestState.Accepted;
                case 11:
                case 12:
                case 810:
                case 811:
                    Trace.Assert(originalScript == "game.quests[51].state == qs_mentioned or game.quests[51].state == qs_accepted");
                    return GetQuestState(51) == QuestState.Mentioned || GetQuestState(51) == QuestState.Accepted;
                case 41:
                case 42:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_gender ) == gender_female");
                    return pc.GetGender() == Gender.Female;
                case 81:
                case 82:
                    Trace.Assert(originalScript == "game.global_flags[110] == 0 and game.quests[46].state != qs_completed and not anyone( pc.group_list(), \"has_follower\", 8023 )");
                    return !GetGlobalFlag(110) && GetQuestState(46) != QuestState.Completed && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8023));
                case 83:
                case 84:
                case 191:
                case 192:
                    Trace.Assert(originalScript == "game.global_flags[110] == 1");
                    return GetGlobalFlag(110);
                case 85:
                case 86:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_follower\", 8023 )");
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8023));
                case 87:
                case 88:
                    Trace.Assert(originalScript == "game.global_flags[110] == 0 and game.quests[46].state == qs_completed");
                    return !GetGlobalFlag(110) && GetQuestState(46) == QuestState.Completed;
                case 151:
                case 152:
                    Trace.Assert(originalScript == "game.global_flags[168] == 0 and game.global_flags[167] == 0");
                    return !GetGlobalFlag(168) && !GetGlobalFlag(167);
                case 157:
                case 158:
                    Trace.Assert(originalScript == "game.global_flags[168] == 1 and game.global_flags[167] == 0");
                    return GetGlobalFlag(168) && !GetGlobalFlag(167);
                case 193:
                case 194:
                    Trace.Assert(originalScript == "game.global_flags[110] == 0");
                    return !GetGlobalFlag(110);
                case 301:
                    Trace.Assert(originalScript == "game.global_flags[110] == 1 and ( game.quests[52].state == qs_mentioned or game.quests[52].state == qs_accepted )");
                    return GetGlobalFlag(110) && (GetQuestState(52) == QuestState.Mentioned || GetQuestState(52) == QuestState.Accepted);
                case 302:
                case 303:
                    Trace.Assert(originalScript == "game.global_flags[110] == 0 and game.quests[52].state == qs_mentioned and not anyone( pc.group_list(), \"has_follower\", 8023 )");
                    return !GetGlobalFlag(110) && GetQuestState(52) == QuestState.Mentioned && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8023));
                case 306:
                case 307:
                    Trace.Assert(originalScript == "( anyone( pc.group_list(), \"has_follower\", 8023 ) ) and ( game.quests[52].state == qs_mentioned or game.quests[52].state == qs_accepted )");
                    return (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8023))) && (GetQuestState(52) == QuestState.Mentioned || GetQuestState(52) == QuestState.Accepted);
                case 308:
                case 309:
                    Trace.Assert(originalScript == "game.global_flags[118] == 1 and ( game.quests[52].state == qs_mentioned or game.quests[52].state == qs_accepted )");
                    return GetGlobalFlag(118) && (GetQuestState(52) == QuestState.Mentioned || GetQuestState(52) == QuestState.Accepted);
                case 310:
                case 311:
                    Trace.Assert(originalScript == "game.global_flags[110] == 0 and game.quests[46].state == qs_completed and game.global_flags[107] == 0 and ( game.quests[52].state == qs_mentioned or game.quests[52].state == qs_accepted )");
                    return !GetGlobalFlag(110) && GetQuestState(46) == QuestState.Completed && !GetGlobalFlag(107) && (GetQuestState(52) == QuestState.Mentioned || GetQuestState(52) == QuestState.Accepted);
                case 312:
                case 313:
                    Trace.Assert(originalScript == "game.quests[53].state == qs_mentioned");
                    return GetQuestState(53) == QuestState.Mentioned;
                case 314:
                case 315:
                    Trace.Assert(originalScript == "( game.quests[53].state == qs_mentioned or game.quests[53].state == qs_accepted ) and ( game.global_flags[126] == 1 or game.global_vars[13] >= 1 )");
                    return (GetQuestState(53) == QuestState.Mentioned || GetQuestState(53) == QuestState.Accepted) && (GetGlobalFlag(126) || GetGlobalVar(13) >= 1);
                case 316:
                case 317:
                    Trace.Assert(originalScript == "( game.quests[53].state == qs_mentioned or game.quests[53].state == qs_accepted ) and ( game.global_vars[14] >= 5 ) and ( game.quests[52].state != qs_botched )");
                    return (GetQuestState(53) == QuestState.Mentioned || GetQuestState(53) == QuestState.Accepted) && (GetGlobalVar(14) >= 5) && (GetQuestState(52) != QuestState.Botched);
                case 318:
                case 319:
                    Trace.Assert(originalScript == "game.quests[54].state == qs_mentioned");
                    return GetQuestState(54) == QuestState.Mentioned;
                case 320:
                case 321:
                    Trace.Assert(originalScript == "( game.quests[54].state == qs_mentioned or game.quests[54].state == qs_accepted ) and anyone( pc.group_list(), \"has_item\", 2203 )");
                    return (GetQuestState(54) == QuestState.Mentioned || GetQuestState(54) == QuestState.Accepted) && pc.GetPartyMembers().Any(o => o.HasItemByName(2203));
                case 322:
                case 323:
                    Trace.Assert(originalScript == "( game.quests[54].state == qs_accepted ) and ( anyone( pc.group_list(), \"has_item\", 2203 ) )");
                    return (GetQuestState(54) == QuestState.Accepted) && (pc.GetPartyMembers().Any(o => o.HasItemByName(2203)));
                case 324:
                case 325:
                    Trace.Assert(originalScript == "game.quests[54].state == qs_completed");
                    return GetQuestState(54) == QuestState.Completed;
                case 326:
                case 327:
                    Trace.Assert(originalScript == "( game.quests[53].state == qs_mentioned or game.quests[53].state == qs_accepted ) and ( game.global_vars[14] >= 5 ) and ( game.quests[52].state == qs_botched )");
                    return (GetQuestState(53) == QuestState.Mentioned || GetQuestState(53) == QuestState.Accepted) && (GetGlobalVar(14) >= 5) && (GetQuestState(52) == QuestState.Botched);
                case 328:
                case 329:
                    Trace.Assert(originalScript == "game.global_flags[348] == 1 and game.quests[52].state != qs_botched");
                    return GetGlobalFlag(348) && GetQuestState(52) != QuestState.Botched;
                case 403:
                    Trace.Assert(originalScript == "not npc.has_met( pc )");
                    return !npc.HasMet(pc);
                case 406:
                case 409:
                    Trace.Assert(originalScript == "game.global_flags[119] == 1 and game.quests[94].state >= qs_accepted");
                    return GetGlobalFlag(119) && GetQuestState(94) >= QuestState.Accepted;
                case 407:
                case 408:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_bluff) >= 13 and npc.has_met( pc ) and game.global_flags[105] == 0");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 13 && npc.HasMet(pc) && !GetGlobalFlag(105);
                case 461:
                    Trace.Assert(originalScript == "game.global_vars[13] == 1 and game.global_flags[126] == 0");
                    return GetGlobalVar(13) == 1 && !GetGlobalFlag(126);
                case 462:
                    Trace.Assert(originalScript == "game.global_vars[13] == 2 and game.global_flags[126] == 0");
                    return GetGlobalVar(13) == 2 && !GetGlobalFlag(126);
                case 463:
                    Trace.Assert(originalScript == "game.global_vars[13] == 3 and game.global_flags[126] == 0");
                    return GetGlobalVar(13) == 3 && !GetGlobalFlag(126);
                case 464:
                    Trace.Assert(originalScript == "game.global_vars[13] == 4 and game.global_flags[126] == 0");
                    return GetGlobalVar(13) == 4 && !GetGlobalFlag(126);
                case 465:
                    Trace.Assert(originalScript == "game.global_vars[13] == 5 or game.global_flags[126] == 1");
                    return GetGlobalVar(13) == 5 || GetGlobalFlag(126);
                case 531:
                case 532:
                    Trace.Assert(originalScript == "game.global_flags[167] == 0");
                    return !GetGlobalFlag(167);
                case 633:
                case 653:
                case 654:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_listen) <= 5");
                    throw new NotSupportedException("Conversion failed.");
                case 634:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_listen) >= 6");
                    throw new NotSupportedException("Conversion failed.");
                case 655:
                case 656:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_listen) > 5");
                    throw new NotSupportedException("Conversion failed.");
                case 673:
                case 674:
                case 693:
                case 694:
                    Trace.Assert(originalScript == "game.quests[52].state == qs_unknown");
                    return GetQuestState(52) == QuestState.Unknown;
                case 675:
                case 676:
                case 695:
                case 696:
                    Trace.Assert(originalScript == "game.quests[52].state >= qs_mentioned");
                    return GetQuestState(52) >= QuestState.Mentioned;
                case 741:
                case 754:
                case 755:
                    Trace.Assert(originalScript == "game.global_flags[104] == 1 and game.global_flags[106] == 0 and game.global_flags[105] == 0");
                    return GetGlobalFlag(104) && !GetGlobalFlag(106) && !GetGlobalFlag(105);
                case 742:
                case 763:
                case 765:
                    Trace.Assert(originalScript == "game.global_flags[104] == 0 and game.global_flags[106] == 1 and game.global_flags[105] == 0");
                    return !GetGlobalFlag(104) && GetGlobalFlag(106) && !GetGlobalFlag(105);
                case 743:
                case 771:
                case 775:
                    Trace.Assert(originalScript == "game.global_flags[104] == 0 and game.global_flags[106] == 0 and game.global_flags[105] == 1");
                    return !GetGlobalFlag(104) && !GetGlobalFlag(106) && GetGlobalFlag(105);
                case 744:
                case 764:
                case 766:
                    Trace.Assert(originalScript == "game.global_flags[104] == 1 and game.global_flags[106] == 1 and game.global_flags[105] == 0");
                    return GetGlobalFlag(104) && GetGlobalFlag(106) && !GetGlobalFlag(105);
                case 745:
                case 772:
                case 776:
                    Trace.Assert(originalScript == "game.global_flags[104] == 1 and game.global_flags[106] == 0 and game.global_flags[105] == 1");
                    return GetGlobalFlag(104) && !GetGlobalFlag(106) && GetGlobalFlag(105);
                case 746:
                case 761:
                case 773:
                case 777:
                    Trace.Assert(originalScript == "game.global_flags[104] == 0 and game.global_flags[106] == 1 and game.global_flags[105] == 1");
                    return !GetGlobalFlag(104) && GetGlobalFlag(106) && GetGlobalFlag(105);
                case 747:
                case 762:
                case 774:
                case 778:
                    Trace.Assert(originalScript == "game.global_flags[104] == 1 and game.global_flags[106] == 1 and game.global_flags[105] == 1");
                    return GetGlobalFlag(104) && GetGlobalFlag(106) && GetGlobalFlag(105);
                case 751:
                    Trace.Assert(originalScript == "game.global_flags[106] == 1 and game.global_flags[105] == 0");
                    return GetGlobalFlag(106) && !GetGlobalFlag(105);
                case 752:
                    Trace.Assert(originalScript == "game.global_flags[106] == 0 and game.global_flags[105] == 1");
                    return !GetGlobalFlag(106) && GetGlobalFlag(105);
                case 753:
                    Trace.Assert(originalScript == "game.global_flags[106] == 1 and game.global_flags[105] == 1");
                    return GetGlobalFlag(106) && GetGlobalFlag(105);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 21:
                case 22:
                case 91:
                case 94:
                case 111:
                case 112:
                case 121:
                case 122:
                case 201:
                case 202:
                case 401:
                case 402:
                case 411:
                case 412:
                case 421:
                case 422:
                case 432:
                case 441:
                case 442:
                case 611:
                case 612:
                case 661:
                case 662:
                case 671:
                case 672:
                case 681:
                case 682:
                case 691:
                case 692:
                case 701:
                case 711:
                case 791:
                case 801:
                case 802:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
                    break;
                case 50:
                case 60:
                case 70:
                case 100:
                case 200:
                case 740:
                    Trace.Assert(originalScript == "game.story_state = 5");
                    StoryState = 5;
                    break;
                case 190:
                case 210:
                case 250:
                    Trace.Assert(originalScript == "game.quests[52].state = qs_mentioned");
                    SetQuestState(52, QuestState.Mentioned);
                    break;
                case 191:
                case 192:
                case 251:
                case 252:
                case 301:
                case 328:
                case 329:
                case 351:
                case 352:
                    Trace.Assert(originalScript == "game.quests[52].state = qs_botched");
                    SetQuestState(52, QuestState.Botched);
                    break;
                case 213:
                case 214:
                case 223:
                case 224:
                case 291:
                case 292:
                case 302:
                case 303:
                    Trace.Assert(originalScript == "game.quests[52].state = qs_accepted");
                    SetQuestState(52, QuestState.Accepted);
                    break;
                case 230:
                    Trace.Assert(originalScript == "game.global_flags[344] = 1");
                    SetGlobalFlag(344, true);
                    break;
                case 308:
                case 309:
                    Trace.Assert(originalScript == "game.quests[52].state = qs_completed");
                    SetQuestState(52, QuestState.Completed);
                    break;
                case 314:
                case 315:
                    Trace.Assert(originalScript == "game.quests[53].state = qs_completed");
                    SetQuestState(53, QuestState.Completed);
                    break;
                case 316:
                case 317:
                    Trace.Assert(originalScript == "game.quests[53].state = qs_botched");
                    SetQuestState(53, QuestState.Botched);
                    break;
                case 318:
                case 319:
                case 533:
                case 534:
                case 541:
                case 542:
                case 551:
                case 552:
                    Trace.Assert(originalScript == "game.quests[54].state = qs_accepted");
                    SetQuestState(54, QuestState.Accepted);
                    break;
                case 340:
                    Trace.Assert(originalScript == "pc.reputation_add( 13 )");
                    pc.AddReputation(13);
                    break;
                case 360:
                    Trace.Assert(originalScript == "game.quests[53].state = qs_mentioned");
                    SetQuestState(53, QuestState.Mentioned);
                    break;
                case 371:
                case 372:
                    Trace.Assert(originalScript == "game.quests[53].state = qs_accepted; game.global_flags[344] = 1");
                    SetQuestState(53, QuestState.Accepted);
                    SetGlobalFlag(344, true);
                    ;
                    break;
                case 381:
                case 382:
                    Trace.Assert(originalScript == "game.quests[53].state = qs_accepted");
                    SetQuestState(53, QuestState.Accepted);
                    break;
                case 390:
                    Trace.Assert(originalScript == "game.areas[12] = 1");
                    MakeAreaKnown(12);
                    break;
                case 391:
                case 392:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(12)");
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 407:
                case 408:
                    Trace.Assert(originalScript == "game.global_flags[125] = 1");
                    SetGlobalFlag(125, true);
                    break;
                case 530:
                    Trace.Assert(originalScript == "game.quests[54].state = qs_mentioned");
                    SetQuestState(54, QuestState.Mentioned);
                    break;
                case 580:
                    Trace.Assert(originalScript == "game.map_flags( 5105, 0, 1 )");
                    // FIXME: map_flags;
                    break;
                case 603:
                case 604:
                    Trace.Assert(originalScript == "party_transfer_to( npc, 2203 )");
                    Utilities.party_transfer_to(npc, 2203);
                    break;
                case 620:
                    Trace.Assert(originalScript == "game.quests[54].state = qs_completed; pc.reputation_add( 13 )");
                    SetQuestState(54, QuestState.Completed);
                    pc.AddReputation(13);
                    ;
                    break;
                case 634:
                case 655:
                case 656:
                    Trace.Assert(originalScript == "game.global_flags[813] = 1");
                    SetGlobalFlag(813, true);
                    break;
                case 641:
                case 642:
                case 721:
                case 722:
                    Trace.Assert(originalScript == "escort_below(npc, pc)");
                    escort_below(npc, pc);
                    break;
                case 860:
                    Trace.Assert(originalScript == "game.global_flags[192] = 1");
                    SetGlobalFlag(192, true);
                    break;
                case 861:
                    Trace.Assert(originalScript == "talk_Ashrem( npc, pc, 230)");
                    talk_Ashrem(npc, pc, 230);
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
                case 4:
                case 5:
                case 803:
                case 804:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 10);
                    return true;
                case 7:
                case 8:
                case 806:
                case 807:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 13);
                    return true;
                case 407:
                case 408:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 13);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
