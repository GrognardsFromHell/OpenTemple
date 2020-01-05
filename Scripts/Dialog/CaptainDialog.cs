
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

namespace Scripts.Dialog
{
    [DialogScript(6)]
    public class CaptainDialog : Captain, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 161:
                case 162:
                case 171:
                case 172:
                    originalScript = "not npc.has_met( pc )";
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                case 163:
                case 164:
                case 173:
                case 174:
                    originalScript = "game.quests[1].state == qs_accepted and npc.has_met( pc )";
                    return GetQuestState(1) == QuestState.Accepted && npc.HasMet(pc);
                case 6:
                    originalScript = "npc.has_met( pc )";
                    return npc.HasMet(pc);
                case 21:
                case 28:
                case 261:
                    originalScript = "anyone( pc.group_list(), \"has_follower\", 8000 )";
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000));
                case 22:
                case 26:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 9";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 9;
                case 23:
                case 25:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 11";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 11;
                case 24:
                case 27:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 6";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 6;
                case 41:
                case 42:
                    originalScript = "game.global_flags[70] == 0";
                    return !GetGlobalFlag(70);
                case 43:
                    originalScript = "game.quests[73].state == qs_mentioned";
                    return GetQuestState(73) == QuestState.Mentioned;
                case 44:
                    originalScript = "game.global_vars[990] == 1 and not get_1(npc)";
                    return GetGlobalVar(990) == 1 && !Scripts.get_1(npc);
                case 45:
                    originalScript = "game.global_vars[991] == 1 and not get_2(npc)";
                    return GetGlobalVar(991) == 1 && !Scripts.get_2(npc);
                case 46:
                    originalScript = "game.global_vars[989] != 0 and game.global_vars[989] != 6 and not get_3(npc)";
                    return GetGlobalVar(989) != 0 && GetGlobalVar(989) != 6 && !Scripts.get_3(npc);
                case 47:
                    originalScript = "game.global_flags[976] == 1 and game.quests[73].state == qs_accepted";
                    return GetGlobalFlag(976) && GetQuestState(73) == QuestState.Accepted;
                case 101:
                case 102:
                    originalScript = "game.global_flags[67] == 0";
                    return !GetGlobalFlag(67);
                case 103:
                case 104:
                    originalScript = "game.quests[1].state == qs_accepted";
                    return GetQuestState(1) == QuestState.Accepted;
                case 151:
                case 152:
                    originalScript = "not anyone( pc.group_list(), \"has_follower\", 8000 )";
                    return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000));
                case 252:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 262:
                    originalScript = "not anyone( pc.group_list(), \"has_follower\", 8000 ) and anyone( pc.group_list(), \"has_follower\", 8014 )";
                    return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000)) && pc.GetPartyMembers().Any(o => o.HasFollowerByName(8014));
                case 263:
                    originalScript = "not anyone( pc.group_list(), \"has_follower\", 8000 ) and not anyone( pc.group_list(), \"has_follower\", 8014 )";
                    return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000)) && !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8014));
                case 301:
                case 311:
                case 471:
                case 481:
                    originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD";
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 302:
                case 312:
                case 472:
                case 482:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 303:
                case 313:
                case 473:
                case 483:
                    originalScript = "game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 491:
                    originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 492:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 501:
                case 510:
                    originalScript = "game.party_alignment == LAWFUL_GOOD";
                    return PartyAlignment == Alignment.LAWFUL_GOOD;
                case 502:
                case 511:
                    originalScript = "game.party_alignment == CHAOTIC_GOOD";
                    return PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 503:
                case 512:
                    originalScript = "game.party_alignment == LAWFUL_EVIL";
                    return PartyAlignment == Alignment.LAWFUL_EVIL;
                case 504:
                case 513:
                    originalScript = "game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 505:
                case 514:
                    originalScript = "game.party_alignment == TRUE_NEUTRAL";
                    return PartyAlignment == Alignment.NEUTRAL;
                case 506:
                case 515:
                    originalScript = "game.party_alignment == NEUTRAL_GOOD";
                    return PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 507:
                case 516:
                    originalScript = "game.party_alignment == NEUTRAL_EVIL";
                    return PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 508:
                case 517:
                    originalScript = "game.party_alignment == LAWFUL_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL;
                case 509:
                case 518:
                    originalScript = "game.party_alignment == CHAOTIC_NEUTRAL";
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 522:
                case 532:
                case 542:
                case 552:
                case 562:
                case 572:
                case 582:
                case 592:
                case 602:
                    originalScript = "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == LAWFUL_NEUTRAL";
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL;
                case 523:
                case 533:
                case 543:
                case 553:
                case 563:
                case 573:
                case 583:
                case 593:
                case 603:
                    originalScript = "game.party_alignment == CHAOTIC_GOOD or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == LAWFUL_EVIL";
                    return PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL;
                case 524:
                case 534:
                case 544:
                case 554:
                case 564:
                case 574:
                case 584:
                case 594:
                case 604:
                    originalScript = "game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 641:
                    originalScript = "game.global_vars[989] == 1";
                    return GetGlobalVar(989) == 1;
                case 642:
                    originalScript = "game.global_vars[989] == 2";
                    return GetGlobalVar(989) == 2;
                case 643:
                    originalScript = "game.global_vars[989] == 3";
                    return GetGlobalVar(989) == 3;
                case 644:
                    originalScript = "game.global_vars[989] == 4";
                    return GetGlobalVar(989) == 4;
                case 645:
                    originalScript = "game.global_vars[989] == 5";
                    return GetGlobalVar(989) == 5;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 30:
                case 60:
                    originalScript = "game.quests[1].state = qs_completed";
                    SetQuestState(1, QuestState.Completed);
                    break;
                case 43:
                    originalScript = "game.quests[104].state = qs_completed";
                    SetQuestState(104, QuestState.Completed);
                    break;
                case 44:
                    originalScript = "npc_1(npc)";
                    Scripts.npc_1(npc);
                    break;
                case 45:
                    originalScript = "npc_2(npc)";
                    Scripts.npc_2(npc);
                    break;
                case 46:
                    originalScript = "npc_3(npc)";
                    Scripts.npc_3(npc);
                    break;
                case 80:
                    originalScript = "game.quests[1].state = qs_completed; npc.reaction_adj( pc,-10)";
                    SetQuestState(1, QuestState.Completed);
                    npc.AdjustReaction(pc, -10);
                    ;
                    break;
                case 90:
                    originalScript = "game.quests[1].state = qs_completed; game.global_vars[1] = 1";
                    SetQuestState(1, QuestState.Completed);
                    SetGlobalVar(1, 1);
                    ;
                    break;
                case 221:
                    originalScript = "argue(npc,pc,52)";
                    argue(npc, pc, 52);
                    break;
                case 231:
                    originalScript = "argue(npc,pc,54)";
                    argue(npc, pc, 54);
                    break;
                case 241:
                    originalScript = "argue(npc,pc,56)";
                    argue(npc, pc, 56);
                    break;
                case 251:
                case 252:
                    originalScript = "argue(npc,pc,60)";
                    argue(npc, pc, 60);
                    break;
                case 261:
                    originalScript = "make_elmo_talk( npc,pc,400 )";
                    make_elmo_talk(npc, pc, 400);
                    break;
                case 262:
                    originalScript = "make_otis_talk( npc,pc,600)";
                    make_otis_talk(npc, pc, 600);
                    break;
                case 263:
                    originalScript = "argue(npc,pc,95)";
                    argue(npc, pc, 95);
                    break;
                case 610:
                    originalScript = "game.areas[7] = 1; game.quests[73].state = qs_accepted; game.global_vars[970] = 2";
                    MakeAreaKnown(7);
                    SetQuestState(73, QuestState.Accepted);
                    SetGlobalVar(970, 2);
                    ;
                    break;
                case 612:
                    originalScript = "game.worldmap_travel_by_dialog(7)";
                    WorldMapTravelByDialog(7);
                    break;
                case 680:
                    originalScript = "game.quests[73].state = qs_completed";
                    SetQuestState(73, QuestState.Completed);
                    break;
                case 681:
                    originalScript = "pc.reputation_add( 33 ); pc.money_adj(10000)";
                    pc.AddReputation(33);
                    pc.AdjustMoney(10000);
                    ;
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
                case 22:
                case 26:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 9);
                    return true;
                case 23:
                case 25:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 11);
                    return true;
                case 24:
                case 27:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 6);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
