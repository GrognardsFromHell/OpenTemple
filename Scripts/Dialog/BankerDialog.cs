
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
    [DialogScript(335)]
    public class BankerDialog : Banker, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 4:
                    originalScript = "game.global_vars[963] == 1";
                    return GetGlobalVar(963) == 1;
                case 5:
                case 16:
                    originalScript = "game.global_vars[963] == 0";
                    return GetGlobalVar(963) == 0;
                case 6:
                case 21:
                    originalScript = "game.global_flags[807] == 1";
                    return GetGlobalFlag(807);
                case 13:
                    originalScript = "game.global_vars[963] == 8 and not get_2(npc)";
                    return GetGlobalVar(963) == 8 && !Scripts.get_2(npc);
                case 14:
                    originalScript = "(game.quests[83].state == qs_mentioned or game.quests[83].state == qs_botched) and not get_1(npc)";
                    return (GetQuestState(83) == QuestState.Mentioned || GetQuestState(83) == QuestState.Botched) && !Scripts.get_1(npc);
                case 15:
                    originalScript = "game.global_vars[963] == 1 and game.leader.reputation_has(37) == 0 and game.leader.reputation_has(38) == 0";
                    return GetGlobalVar(963) == 1 && !SelectedPartyLeader.HasReputation(37) && !SelectedPartyLeader.HasReputation(38);
                case 17:
                    originalScript = "game.global_vars[963] == 2 or game.global_vars[963] == 3";
                    return GetGlobalVar(963) == 2 || GetGlobalVar(963) == 3;
                case 18:
                    originalScript = "game.global_vars[963] == 5";
                    return GetGlobalVar(963) == 5;
                case 19:
                    originalScript = "game.global_vars[963] == 7";
                    return GetGlobalVar(963) == 7;
                case 20:
                    originalScript = "game.global_vars[963] == 8";
                    return GetGlobalVar(963) == 8;
                case 101:
                    originalScript = "pc.money_get() >= 10000000";
                    return pc.GetMoney() >= 10000000;
                case 102:
                    originalScript = "pc.money_get() <= 9999900";
                    return pc.GetMoney() <= 9999900;
                case 151:
                case 171:
                    originalScript = "pc.money_get() >= 5500000";
                    return pc.GetMoney() >= 5500000;
                case 152:
                case 172:
                    originalScript = "pc.money_get() <= 5499900";
                    return pc.GetMoney() <= 5499900;
                case 211:
                    originalScript = "pc.money_get() >= 8250000";
                    return pc.GetMoney() >= 8250000;
                case 212:
                    originalScript = "pc.money_get() <= 8249900";
                    return pc.GetMoney() <= 8249900;
                case 271:
                case 311:
                case 321:
                case 331:
                case 341:
                case 351:
                case 361:
                case 371:
                case 381:
                case 391:
                case 401:
                case 411:
                case 421:
                case 431:
                case 441:
                case 451:
                case 461:
                case 471:
                case 481:
                case 491:
                case 501:
                case 511:
                case 521:
                case 531:
                case 541:
                case 601:
                case 641:
                case 681:
                    originalScript = "game.global_vars[899] <= 24 and game.global_flags[810] == 0";
                    return GetGlobalVar(899) <= 24 && !GetGlobalFlag(810);
                case 272:
                case 312:
                case 322:
                case 332:
                case 342:
                case 352:
                case 362:
                case 372:
                case 382:
                case 392:
                case 402:
                case 412:
                case 422:
                case 432:
                case 442:
                case 452:
                case 462:
                case 472:
                case 482:
                case 492:
                case 502:
                case 512:
                case 522:
                case 532:
                case 542:
                case 602:
                case 642:
                case 682:
                    originalScript = "game.global_vars[899] <= 24 and game.global_flags[810] == 1";
                    return GetGlobalVar(899) <= 24 && GetGlobalFlag(810);
                case 273:
                case 573:
                case 603:
                case 643:
                case 693:
                    originalScript = "game.global_vars[899] == 0";
                    return GetGlobalVar(899) == 0;
                case 274:
                case 574:
                case 604:
                case 644:
                case 694:
                    originalScript = "game.global_vars[899] == 1";
                    return GetGlobalVar(899) == 1;
                case 275:
                case 575:
                case 605:
                case 645:
                case 695:
                    originalScript = "game.global_vars[899] == 2";
                    return GetGlobalVar(899) == 2;
                case 276:
                case 576:
                case 606:
                case 646:
                case 696:
                    originalScript = "game.global_vars[899] == 3";
                    return GetGlobalVar(899) == 3;
                case 277:
                case 577:
                case 607:
                case 647:
                case 697:
                    originalScript = "game.global_vars[899] == 4";
                    return GetGlobalVar(899) == 4;
                case 278:
                case 578:
                case 608:
                case 648:
                case 698:
                    originalScript = "game.global_vars[899] == 5";
                    return GetGlobalVar(899) == 5;
                case 279:
                case 579:
                case 609:
                case 649:
                case 699:
                    originalScript = "game.global_vars[899] == 6";
                    return GetGlobalVar(899) == 6;
                case 280:
                case 580:
                case 610:
                case 650:
                case 700:
                    originalScript = "game.global_vars[899] == 7";
                    return GetGlobalVar(899) == 7;
                case 281:
                case 581:
                case 611:
                case 651:
                case 701:
                    originalScript = "game.global_vars[899] == 8";
                    return GetGlobalVar(899) == 8;
                case 282:
                case 582:
                case 612:
                case 652:
                case 702:
                    originalScript = "game.global_vars[899] == 9";
                    return GetGlobalVar(899) == 9;
                case 283:
                case 583:
                case 613:
                case 653:
                case 703:
                    originalScript = "game.global_vars[899] == 10";
                    return GetGlobalVar(899) == 10;
                case 284:
                case 584:
                case 614:
                case 654:
                case 704:
                    originalScript = "game.global_vars[899] == 11";
                    return GetGlobalVar(899) == 11;
                case 285:
                case 585:
                case 615:
                case 655:
                case 705:
                    originalScript = "game.global_vars[899] == 12";
                    return GetGlobalVar(899) == 12;
                case 286:
                case 586:
                case 616:
                case 656:
                case 706:
                    originalScript = "game.global_vars[899] == 13";
                    return GetGlobalVar(899) == 13;
                case 287:
                case 587:
                case 617:
                case 657:
                case 707:
                    originalScript = "game.global_vars[899] == 14";
                    return GetGlobalVar(899) == 14;
                case 288:
                case 588:
                case 618:
                case 658:
                case 708:
                    originalScript = "game.global_vars[899] == 15";
                    return GetGlobalVar(899) == 15;
                case 289:
                case 589:
                case 619:
                case 659:
                case 709:
                    originalScript = "game.global_vars[899] == 16";
                    return GetGlobalVar(899) == 16;
                case 290:
                case 590:
                case 620:
                case 660:
                case 710:
                    originalScript = "game.global_vars[899] == 17";
                    return GetGlobalVar(899) == 17;
                case 291:
                case 591:
                case 621:
                case 661:
                case 711:
                    originalScript = "game.global_vars[899] == 18";
                    return GetGlobalVar(899) == 18;
                case 292:
                case 592:
                case 622:
                case 662:
                case 712:
                    originalScript = "game.global_vars[899] == 19";
                    return GetGlobalVar(899) == 19;
                case 293:
                case 593:
                case 623:
                case 663:
                case 713:
                    originalScript = "game.global_vars[899] == 20";
                    return GetGlobalVar(899) == 20;
                case 294:
                case 594:
                case 624:
                case 664:
                case 714:
                    originalScript = "game.global_vars[899] == 21";
                    return GetGlobalVar(899) == 21;
                case 295:
                case 595:
                case 625:
                case 665:
                case 715:
                    originalScript = "game.global_vars[899] == 22";
                    return GetGlobalVar(899) == 22;
                case 296:
                case 596:
                case 626:
                case 666:
                case 716:
                    originalScript = "game.global_vars[899] == 23";
                    return GetGlobalVar(899) == 23;
                case 297:
                case 597:
                case 627:
                case 667:
                case 717:
                    originalScript = "game.global_vars[899] == 24";
                    return GetGlobalVar(899) == 24;
                case 298:
                case 299:
                case 313:
                case 323:
                case 333:
                case 343:
                case 353:
                case 363:
                case 373:
                case 383:
                case 393:
                case 403:
                case 413:
                case 423:
                case 433:
                case 443:
                case 453:
                case 463:
                case 473:
                case 483:
                case 493:
                case 503:
                case 513:
                case 523:
                case 533:
                case 543:
                case 598:
                case 628:
                case 629:
                case 668:
                case 669:
                case 683:
                case 718:
                    originalScript = "game.global_vars[899] == 25";
                    return GetGlobalVar(899) == 25;
                case 721:
                    originalScript = "(game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD) and game.global_flags[807] == 1";
                    return (PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD) && GetGlobalFlag(807);
                case 722:
                    originalScript = "(game.party_alignment == LAWFUL_EVIL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_GOOD) and game.global_flags[807] == 1";
                    return (PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD) && GetGlobalFlag(807);
                case 723:
                    originalScript = "(game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == CHAOTIC_NEUTRAL) and game.global_flags[807] == 1";
                    return (PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL) && GetGlobalFlag(807);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 13:
                    originalScript = "npc_2(npc)";
                    Scripts.npc_2(npc);
                    break;
                case 14:
                    originalScript = "npc_1(npc)";
                    Scripts.npc_1(npc);
                    break;
                case 25:
                    originalScript = "game.global_vars[963] = 2";
                    SetGlobalVar(963, 2);
                    break;
                case 81:
                    originalScript = "game.global_vars[963] = 3";
                    SetGlobalVar(963, 3);
                    break;
                case 101:
                    originalScript = "pc.money_adj(-10000000); game.global_vars[963] = 4";
                    pc.AdjustMoney(-10000000);
                    SetGlobalVar(963, 4);
                    ;
                    break;
                case 102:
                case 152:
                    originalScript = "game.global_vars[963] = 5";
                    SetGlobalVar(963, 5);
                    break;
                case 130:
                    originalScript = "game.global_flags[966] = 1; game.party[0].reputation_add(37)";
                    SetGlobalFlag(966, true);
                    PartyLeader.AddReputation(37);
                    ;
                    break;
                case 151:
                    originalScript = "pc.money_adj(-5500000); game.global_vars[963] = 6";
                    pc.AdjustMoney(-5500000);
                    SetGlobalVar(963, 6);
                    ;
                    break;
                case 160:
                    originalScript = "game.global_flags[966] = 1; game.quests[82].state = qs_accepted; game.party[0].reputation_add(37)";
                    SetGlobalFlag(966, true);
                    SetQuestState(82, QuestState.Accepted);
                    PartyLeader.AddReputation(37);
                    ;
                    break;
                case 171:
                    originalScript = "pc.money_adj(-5500000)";
                    pc.AdjustMoney(-5500000);
                    break;
                case 180:
                    originalScript = "game.global_vars[963] = 4; game.quests[82].state = qs_completed";
                    SetGlobalVar(963, 4);
                    SetQuestState(82, QuestState.Completed);
                    ;
                    break;
                case 211:
                    originalScript = "pc.money_adj(-8250000); game.quests[82].unbotch(); game.quests[82].state = qs_completed; game.party[0].reputation_remove(38)";
                    pc.AdjustMoney(-8250000);
                    UnbotchQuest(82);
                    SetQuestState(82, QuestState.Completed);
                    PartyLeader.RemoveReputation(38);
                    ;
                    break;
                case 561:
                    originalScript = "make_withdrawal(npc,pc)";
                    make_withdrawal(npc, pc);
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
