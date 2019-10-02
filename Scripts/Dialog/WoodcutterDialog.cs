
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
    [DialogScript(25)]
    public class WoodcutterDialog : Woodcutter, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 5:
                case 601:
                case 602:
                    Trace.Assert(originalScript == "not npc.has_met( pc )");
                    return !npc.HasMet(pc);
                case 3:
                case 625:
                case 651:
                case 661:
                case 673:
                case 681:
                case 713:
                case 801:
                    Trace.Assert(originalScript == "npc.has_met( pc ) and game.quests[2].state != qs_completed and game.global_vars[3] <= 1");
                    return npc.HasMet(pc) && GetQuestState(2) != QuestState.Completed && GetGlobalVar(3) <= 1;
                case 4:
                case 626:
                case 652:
                case 662:
                case 674:
                case 682:
                case 714:
                case 802:
                    Trace.Assert(originalScript == "npc.has_met( pc ) and game.quests[2].state == qs_completed");
                    return npc.HasMet(pc) && GetQuestState(2) == QuestState.Completed;
                case 6:
                case 7:
                case 603:
                case 604:
                    Trace.Assert(originalScript == "npc.has_met( pc ) and game.quests[2].state != qs_completed and game.global_vars[3] >= 2");
                    return npc.HasMet(pc) && GetQuestState(2) != QuestState.Completed && GetGlobalVar(3) >= 2;
                case 15:
                case 16:
                    Trace.Assert(originalScript == "game.global_flags[67] == 0");
                    return !GetGlobalFlag(67);
                case 23:
                case 27:
                case 33:
                case 37:
                    Trace.Assert(originalScript == "game.global_vars[3] <= 1");
                    return GetGlobalVar(3) <= 1;
                case 24:
                case 28:
                case 34:
                case 38:
                    Trace.Assert(originalScript == "game.global_vars[3] >= 2");
                    return GetGlobalVar(3) >= 2;
                case 25:
                case 35:
                    Trace.Assert(originalScript == "game.global_flags[1] == 1 and game.global_vars[3] <= 1");
                    return GetGlobalFlag(1) && GetGlobalVar(3) <= 1;
                case 62:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == LAWFUL_EVIL or game.party_alignment == CHAOTIC_GOOD");
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 66:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL");
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL;
                case 101:
                case 102:
                case 103:
                case 106:
                    Trace.Assert(originalScript == "game.quests[2].state == qs_unknown");
                    return GetQuestState(2) == QuestState.Unknown;
                case 104:
                case 107:
                    Trace.Assert(originalScript == "game.quests[2].state == qs_mentioned");
                    return GetQuestState(2) == QuestState.Mentioned;
                case 113:
                case 115:
                    Trace.Assert(originalScript == "game.story_state >= 2 and game.areas[3] == 0");
                    return StoryState >= 2 && !IsAreaKnown(3);
                case 116:
                case 117:
                    Trace.Assert(originalScript == "game.quests[99].state == qs_mentioned");
                    return GetQuestState(99) == QuestState.Mentioned;
                case 118:
                case 119:
                    Trace.Assert(originalScript == "game.quests[99].state != qs_completed and game.quests[99].state != qs_botched and game.quests[99].state != qs_mentioned");
                    return GetQuestState(99) != QuestState.Completed && GetQuestState(99) != QuestState.Botched && GetQuestState(99) != QuestState.Mentioned;
                case 120:
                case 121:
                case 621:
                case 622:
                    Trace.Assert(originalScript == "game.quests[99].state == qs_botched");
                    return GetQuestState(99) == QuestState.Botched;
                case 122:
                case 123:
                case 623:
                case 624:
                    Trace.Assert(originalScript == "game.quests[99].state == qs_completed");
                    return GetQuestState(99) == QuestState.Completed;
                case 173:
                case 174:
                    Trace.Assert(originalScript == "game.quests[99].state == qs_accepted");
                    return GetQuestState(99) == QuestState.Accepted;
                case 175:
                case 176:
                case 201:
                case 202:
                    Trace.Assert(originalScript == "game.quests[99].state != qs_accepted");
                    return GetQuestState(99) != QuestState.Accepted;
                case 192:
                case 193:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_deity ) == 16");
                    return pc.GetStat(Stat.deity) == 16;
                case 501:
                case 510:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_GOOD");
                    return PartyAlignment == Alignment.LAWFUL_GOOD;
                case 502:
                case 511:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_GOOD");
                    return PartyAlignment == Alignment.CHAOTIC_GOOD;
                case 503:
                case 512:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_EVIL");
                    return PartyAlignment == Alignment.LAWFUL_EVIL;
                case 504:
                case 513:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_EVIL");
                    return PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 505:
                case 514:
                    Trace.Assert(originalScript == "game.party_alignment == TRUE_NEUTRAL");
                    return PartyAlignment == Alignment.NEUTRAL;
                case 506:
                case 515:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_GOOD");
                    return PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 507:
                case 516:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 508:
                case 517:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_NEUTRAL");
                    return PartyAlignment == Alignment.LAWFUL_NEUTRAL;
                case 509:
                case 518:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_NEUTRAL");
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
                case 605:
                case 606:
                    Trace.Assert(originalScript == "game.quests[99].state == qs_completed and game.global_vars[767] == 0");
                    return GetQuestState(99) == QuestState.Completed && GetGlobalVar(767) == 0;
                case 607:
                case 608:
                    Trace.Assert(originalScript == "game.quests[99].state == qs_completed and game.global_vars[767] == 3 and game.global_vars[769] == 0");
                    return GetQuestState(99) == QuestState.Completed && GetGlobalVar(767) == 3 && GetGlobalVar(769) == 0;
                case 609:
                case 610:
                    Trace.Assert(originalScript == "game.quests[99].state == qs_completed and game.global_vars[767] == 2 and game.global_vars[769] == 0");
                    return GetQuestState(99) == QuestState.Completed && GetGlobalVar(767) == 2 && GetGlobalVar(769) == 0;
                case 611:
                case 612:
                    Trace.Assert(originalScript == "game.quests[99].state == qs_completed and game.global_vars[767] == 4 and game.global_vars[769] == 0 and game.global_vars[776] <= 2");
                    return GetQuestState(99) == QuestState.Completed && GetGlobalVar(767) == 4 && GetGlobalVar(769) == 0 && GetGlobalVar(776) <= 2;
                case 613:
                case 614:
                    Trace.Assert(originalScript == "game.quests[99].state == qs_completed and game.global_vars[767] == 4 and game.global_vars[769] == 1");
                    return GetQuestState(99) == QuestState.Completed && GetGlobalVar(767) == 4 && GetGlobalVar(769) == 1;
                case 615:
                case 616:
                    Trace.Assert(originalScript == "game.quests[99].state == qs_completed and game.global_vars[767] == 4 and game.global_vars[769] == 2 and game.global_flags[857] == 0");
                    return GetQuestState(99) == QuestState.Completed && GetGlobalVar(767) == 4 && GetGlobalVar(769) == 2 && !GetGlobalFlag(857);
                case 617:
                case 618:
                    Trace.Assert(originalScript == "game.quests[99].state == qs_completed and game.global_vars[767] == 4 and game.global_vars[769] == 0 and game.global_vars[776] == 3");
                    return GetQuestState(99) == QuestState.Completed && GetGlobalVar(767) == 4 && GetGlobalVar(769) == 0 && GetGlobalVar(776) == 3;
                case 619:
                case 620:
                    Trace.Assert(originalScript == "game.quests[99].state == qs_completed and game.global_vars[767] == 4 and game.global_vars[769] == 2 and game.global_flags[857] == 1");
                    return GetQuestState(99) == QuestState.Completed && GetGlobalVar(767) == 4 && GetGlobalVar(769) == 2 && GetGlobalFlag(857);
                case 635:
                case 636:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL");
                    return PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 692:
                    Trace.Assert(originalScript == "game.global_vars[771] == 0");
                    return GetGlobalVar(771) == 0;
                case 693:
                    Trace.Assert(originalScript == "game.global_vars[772] == 0");
                    return GetGlobalVar(772) == 0;
                case 694:
                    Trace.Assert(originalScript == "game.global_vars[773] == 0");
                    return GetGlobalVar(773) == 0;
                case 695:
                    Trace.Assert(originalScript == "game.global_vars[770] == 0");
                    return GetGlobalVar(770) == 0;
                case 696:
                    Trace.Assert(originalScript == "game.global_vars[774] == 0");
                    return GetGlobalVar(774) == 0;
                case 703:
                    Trace.Assert(originalScript == "pc.money_get() >= 1650000");
                    return pc.GetMoney() >= 1650000;
                case 704:
                    Trace.Assert(originalScript == "pc.money_get() <= 1649999");
                    return pc.GetMoney() <= 1649999;
                case 705:
                    Trace.Assert(originalScript == "pc.money_get() <= 1659999");
                    return pc.GetMoney() <= 1659999;
                case 723:
                    Trace.Assert(originalScript == "pc.money_get() >= 330000");
                    return pc.GetMoney() >= 330000;
                case 724:
                case 725:
                    Trace.Assert(originalScript == "pc.money_get() <= 329999");
                    return pc.GetMoney() <= 329999;
                case 733:
                    Trace.Assert(originalScript == "pc.money_get() >= 600000");
                    return pc.GetMoney() >= 600000;
                case 734:
                case 735:
                    Trace.Assert(originalScript == "pc.money_get() <= 599999");
                    return pc.GetMoney() <= 599999;
                case 763:
                    Trace.Assert(originalScript == "pc.money_get() >= 80000");
                    return pc.GetMoney() >= 80000;
                case 764:
                case 765:
                    Trace.Assert(originalScript == "pc.money_get() <= 79999");
                    return pc.GetMoney() <= 79999;
                case 773:
                    Trace.Assert(originalScript == "pc.money_get() >= 1807500");
                    return pc.GetMoney() >= 1807500;
                case 774:
                case 775:
                    Trace.Assert(originalScript == "pc.money_get() <= 1807499");
                    return pc.GetMoney() <= 1807499;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 6:
                case 7:
                case 24:
                case 28:
                case 34:
                case 38:
                case 603:
                case 604:
                    Trace.Assert(originalScript == "game.quests[2].state = qs_completed");
                    SetQuestState(2, QuestState.Completed);
                    break;
                case 20:
                case 30:
                    Trace.Assert(originalScript == "game.quests[2].state = qs_mentioned");
                    SetQuestState(2, QuestState.Mentioned);
                    break;
                case 23:
                case 27:
                case 33:
                case 37:
                case 41:
                case 42:
                case 51:
                case 52:
                case 64:
                case 65:
                case 104:
                case 107:
                    Trace.Assert(originalScript == "game.quests[2].state = qs_accepted");
                    SetQuestState(2, QuestState.Accepted);
                    break;
                case 70:
                    Trace.Assert(originalScript == "game.areas[10] = 1");
                    MakeAreaKnown(10);
                    break;
                case 72:
                case 73:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(10)");
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 118:
                case 119:
                    Trace.Assert(originalScript == "game.quests[99].state = qs_mentioned");
                    SetQuestState(99, QuestState.Mentioned);
                    break;
                case 140:
                    Trace.Assert(originalScript == "npc.reaction_adj( pc,+30)");
                    npc.AdjustReaction(pc, +30);
                    break;
                case 160:
                    Trace.Assert(originalScript == "game.areas[3] = 1; game.story_state = 3");
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 161:
                case 162:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(3)");
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 175:
                case 176:
                case 201:
                case 202:
                    Trace.Assert(originalScript == "game.quests[99].state = qs_accepted");
                    SetQuestState(99, QuestState.Accepted);
                    break;
                case 605:
                case 606:
                    Trace.Assert(originalScript == "game.global_vars[767] = 1");
                    SetGlobalVar(767, 1);
                    break;
                case 607:
                case 608:
                    Trace.Assert(originalScript == "game.global_vars[767] = 4");
                    SetGlobalVar(767, 4);
                    break;
                case 650:
                    Trace.Assert(originalScript == "letter_written()");
                    letter_written();
                    break;
                case 703:
                    Trace.Assert(originalScript == "pc.money_adj(-1650000); game.global_vars[768] = 6317; game.global_vars[772] = 1");
                    pc.AdjustMoney(-1650000);
                    SetGlobalVar(768, 6317);
                    SetGlobalVar(772, 1);
                    ;
                    break;
                case 723:
                    Trace.Assert(originalScript == "pc.money_adj(-330000); game.global_vars[768] = 6316; game.global_vars[770] = 1");
                    pc.AdjustMoney(-330000);
                    SetGlobalVar(768, 6316);
                    SetGlobalVar(770, 1);
                    ;
                    break;
                case 733:
                    Trace.Assert(originalScript == "pc.money_adj(-600000); game.global_vars[768] = 6257; game.global_vars[771] = 1");
                    pc.AdjustMoney(-600000);
                    SetGlobalVar(768, 6257);
                    SetGlobalVar(771, 1);
                    ;
                    break;
                case 763:
                    Trace.Assert(originalScript == "pc.money_adj(-80000); game.global_vars[768] = 4195; game.global_vars[773] = 1");
                    pc.AdjustMoney(-80000);
                    SetGlobalVar(768, 4195);
                    SetGlobalVar(773, 1);
                    ;
                    break;
                case 773:
                    Trace.Assert(originalScript == "pc.money_adj(-1807500); game.global_vars[768] = 4108; game.global_vars[774] = 1");
                    pc.AdjustMoney(-1807500);
                    SetGlobalVar(768, 4108);
                    SetGlobalVar(774, 1);
                    ;
                    break;
                case 800:
                    Trace.Assert(originalScript == "order_item()");
                    order_item();
                    break;
                case 810:
                    Trace.Assert(originalScript == "give_item(pc)");
                    give_item(pc);
                    break;
                case 841:
                case 842:
                    Trace.Assert(originalScript == "game.encounter_queue.append(3579)");
                    QueueRandomEncounter(3579);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
