
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
    [DialogScript(122)]
    public class AlrremDialog : Alrrem, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 4:
                case 5:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 8";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 8;
                case 7:
                case 8:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 11";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 11;
                case 9:
                case 10:
                    originalScript = "game.quests[47].state == qs_mentioned or game.quests[47].state == qs_accepted";
                    return GetQuestState(47) == QuestState.Mentioned || GetQuestState(47) == QuestState.Accepted;
                case 11:
                case 12:
                    originalScript = "game.quests[51].state == qs_mentioned or game.quests[51].state == qs_accepted";
                    return GetQuestState(51) == QuestState.Mentioned || GetQuestState(51) == QuestState.Accepted;
                case 41:
                case 42:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_female";
                    return pc.GetGender() == Gender.Female;
                case 81:
                case 82:
                    originalScript = "game.global_flags[110] == 0 and game.quests[46].state != qs_completed and not anyone( pc.group_list(), \"has_follower\", 8023 )";
                    return !GetGlobalFlag(110) && GetQuestState(46) != QuestState.Completed && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8023));
                case 83:
                case 84:
                case 191:
                case 192:
                    originalScript = "game.global_flags[110] == 1";
                    return GetGlobalFlag(110);
                case 85:
                case 86:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 8023 )";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8023));
                case 87:
                case 88:
                    originalScript = "game.global_flags[110] == 0 and game.quests[46].state == qs_completed";
                    return !GetGlobalFlag(110) && GetQuestState(46) == QuestState.Completed;
                case 151:
                case 152:
                    originalScript = "game.global_flags[168] == 0 and game.global_flags[167] == 0";
                    return !GetGlobalFlag(168) && !GetGlobalFlag(167);
                case 157:
                case 158:
                    originalScript = "game.global_flags[168] == 1 and game.global_flags[167] == 0";
                    return GetGlobalFlag(168) && !GetGlobalFlag(167);
                case 193:
                case 194:
                    originalScript = "game.global_flags[110] == 0";
                    return !GetGlobalFlag(110);
                case 301:
                    originalScript = "game.global_flags[110] == 1 and ( game.quests[52].state == qs_mentioned or game.quests[52].state == qs_accepted )";
                    return GetGlobalFlag(110) && (GetQuestState(52) == QuestState.Mentioned || GetQuestState(52) == QuestState.Accepted);
                case 302:
                case 303:
                    originalScript = "game.global_flags[110] == 0 and game.quests[52].state == qs_mentioned and not anyone( pc.group_list(), \"has_follower\", 8023 )";
                    return !GetGlobalFlag(110) && GetQuestState(52) == QuestState.Mentioned && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8023));
                case 306:
                case 307:
                    originalScript = "( anyone( pc.group_list(), \"has_follower\", 8023 ) ) and ( game.quests[52].state == qs_mentioned or game.quests[52].state == qs_accepted )";
                    return (pc.GetPartyMembers().Any(o => o.HasFollowerByName(8023))) && (GetQuestState(52) == QuestState.Mentioned || GetQuestState(52) == QuestState.Accepted);
                case 308:
                case 309:
                    originalScript = "game.global_flags[118] == 1 and ( game.quests[52].state == qs_mentioned or game.quests[52].state == qs_accepted )";
                    return GetGlobalFlag(118) && (GetQuestState(52) == QuestState.Mentioned || GetQuestState(52) == QuestState.Accepted);
                case 310:
                case 311:
                    originalScript = "game.global_flags[110] == 0 and game.quests[46].state == qs_completed and ( game.quests[52].state == qs_mentioned or game.quests[52].state == qs_accepted )";
                    return !GetGlobalFlag(110) && GetQuestState(46) == QuestState.Completed && (GetQuestState(52) == QuestState.Mentioned || GetQuestState(52) == QuestState.Accepted);
                case 312:
                case 313:
                    originalScript = "game.quests[53].state == qs_mentioned";
                    return GetQuestState(53) == QuestState.Mentioned;
                case 314:
                case 315:
                    originalScript = "( game.quests[53].state == qs_mentioned or game.quests[53].state == qs_accepted ) and ( game.global_flags[126] == 1 or game.global_vars[13] >= 1 )";
                    return (GetQuestState(53) == QuestState.Mentioned || GetQuestState(53) == QuestState.Accepted) && (GetGlobalFlag(126) || GetGlobalVar(13) >= 1);
                case 316:
                case 317:
                    originalScript = "( game.quests[53].state == qs_mentioned or game.quests[53].state == qs_accepted ) and ( game.global_vars[14] == 5 ) and ( game.quests[52].state != qs_botched )";
                    return (GetQuestState(53) == QuestState.Mentioned || GetQuestState(53) == QuestState.Accepted) && (GetGlobalVar(14) == 5) && (GetQuestState(52) != QuestState.Botched);
                case 318:
                case 319:
                    originalScript = "game.quests[54].state == qs_mentioned";
                    return GetQuestState(54) == QuestState.Mentioned;
                case 320:
                case 321:
                    originalScript = "( game.quests[54].state == qs_mentioned or game.quests[54].state == qs_accepted ) and anyone( pc.group_list(), \"has_item\", 2203 )";
                    return (GetQuestState(54) == QuestState.Mentioned || GetQuestState(54) == QuestState.Accepted) && pc.GetPartyMembers().Any(o => o.HasItemByName(2203));
                case 322:
                case 323:
                    originalScript = "( game.quests[54].state == qs_accepted ) and ( anyone( pc.group_list(), \"has_item\", 2203 ) )";
                    return (GetQuestState(54) == QuestState.Accepted) && (pc.GetPartyMembers().Any(o => o.HasItemByName(2203)));
                case 324:
                case 325:
                    originalScript = "game.quests[54].state == qs_completed";
                    return GetQuestState(54) == QuestState.Completed;
                case 326:
                case 327:
                    originalScript = "( game.quests[53].state == qs_mentioned or game.quests[53].state == qs_accepted ) and ( game.global_vars[14] == 5 ) and ( game.quests[52].state == qs_botched )";
                    return (GetQuestState(53) == QuestState.Mentioned || GetQuestState(53) == QuestState.Accepted) && (GetGlobalVar(14) == 5) && (GetQuestState(52) == QuestState.Botched);
                case 328:
                case 329:
                    originalScript = "game.global_flags[348] == 1";
                    return GetGlobalFlag(348);
                case 403:
                    originalScript = "not npc.has_met( pc )";
                    return !npc.HasMet(pc);
                case 406:
                case 409:
                    originalScript = "game.global_flags[119] == 1 and game.quests[44].state >= qs_accepted";
                    return GetGlobalFlag(119) && GetQuestState(44) >= QuestState.Accepted;
                case 407:
                case 408:
                    originalScript = "pc.skill_level_get(npc, skill_bluff) >= 11 and npc.has_met( pc )";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 11 && npc.HasMet(pc);
                case 461:
                    originalScript = "game.global_vars[13] == 1 and game.global_flags[126] == 0";
                    return GetGlobalVar(13) == 1 && !GetGlobalFlag(126);
                case 462:
                    originalScript = "game.global_vars[13] == 2 and game.global_flags[126] == 0";
                    return GetGlobalVar(13) == 2 && !GetGlobalFlag(126);
                case 463:
                    originalScript = "game.global_vars[13] == 3 and game.global_flags[126] == 0";
                    return GetGlobalVar(13) == 3 && !GetGlobalFlag(126);
                case 464:
                    originalScript = "game.global_vars[13] == 4 and game.global_flags[126] == 0";
                    return GetGlobalVar(13) == 4 && !GetGlobalFlag(126);
                case 465:
                    originalScript = "game.global_vars[13] == 5 or game.global_flags[126] == 1";
                    return GetGlobalVar(13) == 5 || GetGlobalFlag(126);
                case 531:
                case 532:
                    originalScript = "game.global_flags[167] == 0";
                    return !GetGlobalFlag(167);
                case 673:
                case 674:
                case 693:
                case 694:
                    originalScript = "game.quests[52].state == qs_unknown";
                    return GetQuestState(52) == QuestState.Unknown;
                case 675:
                case 676:
                case 695:
                case 696:
                    originalScript = "game.quests[52].state >= qs_mentioned";
                    return GetQuestState(52) >= QuestState.Mentioned;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
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
                case 712:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 50:
                case 60:
                case 70:
                case 100:
                    originalScript = "game.story_state = 5";
                    StoryState = 5;
                    break;
                case 190:
                case 210:
                case 250:
                    originalScript = "game.quests[52].state = qs_mentioned";
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
                    originalScript = "game.quests[52].state = qs_botched";
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
                    originalScript = "game.quests[52].state = qs_accepted";
                    SetQuestState(52, QuestState.Accepted);
                    break;
                case 230:
                    originalScript = "game.global_flags[344] = 1";
                    SetGlobalFlag(344, true);
                    break;
                case 308:
                case 309:
                    originalScript = "game.quests[52].state = qs_completed";
                    SetQuestState(52, QuestState.Completed);
                    break;
                case 314:
                case 315:
                    originalScript = "game.quests[53].state = qs_completed";
                    SetQuestState(53, QuestState.Completed);
                    break;
                case 316:
                case 317:
                    originalScript = "game.quests[53].state = qs_botched";
                    SetQuestState(53, QuestState.Botched);
                    break;
                case 318:
                case 319:
                case 533:
                case 534:
                case 541:
                case 542:
                    originalScript = "game.quests[54].state = qs_accepted";
                    SetQuestState(54, QuestState.Accepted);
                    break;
                case 340:
                    originalScript = "pc.reputation_add( 13 )";
                    pc.AddReputation(13);
                    break;
                case 360:
                    originalScript = "game.quests[53].state = qs_mentioned";
                    SetQuestState(53, QuestState.Mentioned);
                    break;
                case 371:
                case 372:
                    originalScript = "game.quests[53].state = qs_accepted; game.global_flags[344] = 1";
                    SetQuestState(53, QuestState.Accepted);
                    SetGlobalFlag(344, true);
                    ;
                    break;
                case 381:
                case 382:
                    originalScript = "game.quests[53].state = qs_accepted";
                    SetQuestState(53, QuestState.Accepted);
                    break;
                case 390:
                    originalScript = "game.areas[9] = 1";
                    MakeAreaKnown(9);
                    break;
                case 391:
                case 392:
                    originalScript = "game.worldmap_travel_by_dialog(9)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 407:
                case 408:
                    originalScript = "game.global_flags[125] = 1";
                    SetGlobalFlag(125, true);
                    break;
                case 530:
                    originalScript = "game.quests[54].state = qs_mentioned";
                    SetQuestState(54, QuestState.Mentioned);
                    break;
                case 580:
                    originalScript = "game.map_flags( 5078, 0, 1 )";
                    // FIXME: map_flags;
                    break;
                case 603:
                case 604:
                    originalScript = "party_transfer_to( npc, 2203 )";
                    Utilities.party_transfer_to(npc, 2203);
                    break;
                case 620:
                    originalScript = "game.quests[54].state = qs_completed; pc.reputation_add( 13 )";
                    SetQuestState(54, QuestState.Completed);
                    pc.AddReputation(13);
                    ;
                    break;
                case 641:
                case 642:
                    originalScript = "escort_below(npc, pc)";
                    escort_below(npc, pc);
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
                case 4:
                case 5:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 8);
                    return true;
                case 7:
                case 8:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 11);
                    return true;
                case 407:
                case 408:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 11);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
