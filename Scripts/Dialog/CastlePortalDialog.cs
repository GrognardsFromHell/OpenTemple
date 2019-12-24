
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
    [DialogScript(400)]
    public class CastlePortalDialog : CastlePortal, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 112:
                case 212:
                case 312:
                case 412:
                case 512:
                case 612:
                    originalScript = "game.global_vars[931] != 1 and game.global_vars[932] != 1 and game.global_vars[933] != 1 and game.global_vars[934] != 1 and game.global_vars[935] != 1 and game.global_vars[936] != 1";
                    return GetGlobalVar(931) != 1 && GetGlobalVar(932) != 1 && GetGlobalVar(933) != 1 && GetGlobalVar(934) != 1 && GetGlobalVar(935) != 1 && GetGlobalVar(936) != 1;
                case 113:
                case 213:
                case 313:
                case 413:
                case 513:
                case 613:
                    originalScript = "game.areas[3] == 1 and game.global_vars[931] != 2 and game.global_vars[932] != 2 and game.global_vars[933] != 2 and game.global_vars[934] != 2 and game.global_vars[935] != 2 and game.global_vars[936] != 2";
                    return IsAreaKnown(3) && GetGlobalVar(931) != 2 && GetGlobalVar(932) != 2 && GetGlobalVar(933) != 2 && GetGlobalVar(934) != 2 && GetGlobalVar(935) != 2 && GetGlobalVar(936) != 2;
                case 114:
                case 214:
                case 314:
                case 414:
                case 514:
                case 614:
                    originalScript = "game.areas[14] == 1 and game.global_vars[931] != 3 and game.global_vars[932] != 3 and game.global_vars[933] != 3 and game.global_vars[934] != 3 and game.global_vars[935] != 3 and game.global_vars[936] != 3";
                    return IsAreaKnown(14) && GetGlobalVar(931) != 3 && GetGlobalVar(932) != 3 && GetGlobalVar(933) != 3 && GetGlobalVar(934) != 3 && GetGlobalVar(935) != 3 && GetGlobalVar(936) != 3;
                case 122:
                case 222:
                case 322:
                case 422:
                case 522:
                case 622:
                    originalScript = "game.global_vars[931] != 4 and game.global_vars[932] != 4 and game.global_vars[933] != 4 and game.global_vars[934] != 4 and game.global_vars[935] != 4 and game.global_vars[936] != 4";
                    return GetGlobalVar(931) != 4 && GetGlobalVar(932) != 4 && GetGlobalVar(933) != 4 && GetGlobalVar(934) != 4 && GetGlobalVar(935) != 4 && GetGlobalVar(936) != 4;
                case 123:
                case 223:
                case 323:
                case 423:
                case 523:
                case 623:
                    originalScript = "game.global_vars[931] != 5 and game.global_vars[932] != 5 and game.global_vars[933] != 5 and game.global_vars[934] != 5 and game.global_vars[935] != 5 and game.global_vars[936] != 5";
                    return GetGlobalVar(931) != 5 && GetGlobalVar(932) != 5 && GetGlobalVar(933) != 5 && GetGlobalVar(934) != 5 && GetGlobalVar(935) != 5 && GetGlobalVar(936) != 5;
                case 124:
                case 224:
                case 324:
                case 424:
                case 524:
                case 624:
                    originalScript = "game.global_vars[931] != 6 and game.global_vars[932] != 6 and game.global_vars[933] != 6 and game.global_vars[934] != 6 and game.global_vars[935] != 6 and game.global_vars[936] != 6";
                    return GetGlobalVar(931) != 6 && GetGlobalVar(932) != 6 && GetGlobalVar(933) != 6 && GetGlobalVar(934) != 6 && GetGlobalVar(935) != 6 && GetGlobalVar(936) != 6;
                case 132:
                case 232:
                case 332:
                case 432:
                case 532:
                case 632:
                    originalScript = "game.global_vars[931] != 7 and game.global_vars[932] != 7 and game.global_vars[933] != 7 and game.global_vars[934] != 7 and game.global_vars[935] != 7 and game.global_vars[936] != 7";
                    return GetGlobalVar(931) != 7 && GetGlobalVar(932) != 7 && GetGlobalVar(933) != 7 && GetGlobalVar(934) != 7 && GetGlobalVar(935) != 7 && GetGlobalVar(936) != 7;
                case 133:
                case 233:
                case 333:
                case 433:
                case 533:
                case 633:
                    originalScript = "game.global_vars[931] != 8 and game.global_vars[932] != 8 and game.global_vars[933] != 8 and game.global_vars[934] != 8 and game.global_vars[935] != 8 and game.global_vars[936] != 8";
                    return GetGlobalVar(931) != 8 && GetGlobalVar(932) != 8 && GetGlobalVar(933) != 8 && GetGlobalVar(934) != 8 && GetGlobalVar(935) != 8 && GetGlobalVar(936) != 8;
                case 134:
                case 234:
                case 334:
                case 434:
                case 534:
                case 634:
                    originalScript = "game.global_vars[931] != 9 and game.global_vars[932] != 9 and game.global_vars[933] != 9 and game.global_vars[934] != 9 and game.global_vars[935] != 9 and game.global_vars[936] != 9";
                    return GetGlobalVar(931) != 9 && GetGlobalVar(932) != 9 && GetGlobalVar(933) != 9 && GetGlobalVar(934) != 9 && GetGlobalVar(935) != 9 && GetGlobalVar(936) != 9;
                case 142:
                case 242:
                case 342:
                case 442:
                case 542:
                case 642:
                    originalScript = "game.areas[3] == 1 and game.global_vars[931] != 10 and game.global_vars[932] != 10 and game.global_vars[933] != 10 and game.global_vars[934] != 10 and game.global_vars[935] != 10 and game.global_vars[936] != 10 and game.global_flags[94] == 1";
                    return IsAreaKnown(3) && GetGlobalVar(931) != 10 && GetGlobalVar(932) != 10 && GetGlobalVar(933) != 10 && GetGlobalVar(934) != 10 && GetGlobalVar(935) != 10 && GetGlobalVar(936) != 10 && GetGlobalFlag(94);
                case 143:
                case 243:
                case 343:
                case 443:
                case 543:
                case 643:
                    originalScript = "game.areas[3] == 1 and game.global_vars[931] != 10 and game.global_vars[932] != 10 and game.global_vars[933] != 10 and game.global_vars[934] != 10 and game.global_vars[935] != 10 and game.global_vars[936] != 10 and game.global_flags[94] == 0";
                    return IsAreaKnown(3) && GetGlobalVar(931) != 10 && GetGlobalVar(932) != 10 && GetGlobalVar(933) != 10 && GetGlobalVar(934) != 10 && GetGlobalVar(935) != 10 && GetGlobalVar(936) != 10 && !GetGlobalFlag(94);
                case 144:
                case 244:
                case 344:
                case 444:
                case 544:
                case 644:
                    originalScript = "game.areas[3] == 1 and game.global_vars[931] != 11 and game.global_vars[932] != 11 and game.global_vars[933] != 11 and game.global_vars[934] != 11 and game.global_vars[935] != 11 and game.global_vars[936] != 11";
                    return IsAreaKnown(3) && GetGlobalVar(931) != 11 && GetGlobalVar(932) != 11 && GetGlobalVar(933) != 11 && GetGlobalVar(934) != 11 && GetGlobalVar(935) != 11 && GetGlobalVar(936) != 11;
                case 145:
                case 245:
                case 345:
                case 445:
                case 545:
                case 645:
                    originalScript = "game.areas[3] == 1 and game.global_vars[931] != 12 and game.global_vars[932] != 12 and game.global_vars[933] != 12 and game.global_vars[934] != 12 and game.global_vars[935] != 12 and game.global_vars[936] != 12";
                    return IsAreaKnown(3) && GetGlobalVar(931) != 12 && GetGlobalVar(932) != 12 && GetGlobalVar(933) != 12 && GetGlobalVar(934) != 12 && GetGlobalVar(935) != 12 && GetGlobalVar(936) != 12;
                case 152:
                case 252:
                case 352:
                case 452:
                case 552:
                case 652:
                    originalScript = "game.areas[14] == 1 and game.global_vars[931] != 13 and game.global_vars[932] != 13 and game.global_vars[933] != 13 and game.global_vars[934] != 13 and game.global_vars[935] != 13 and game.global_vars[936] != 13";
                    return IsAreaKnown(14) && GetGlobalVar(931) != 13 && GetGlobalVar(932) != 13 && GetGlobalVar(933) != 13 && GetGlobalVar(934) != 13 && GetGlobalVar(935) != 13 && GetGlobalVar(936) != 13;
                case 153:
                case 253:
                case 353:
                case 453:
                case 553:
                case 653:
                    originalScript = "game.areas[14] == 1 and game.global_vars[931] != 14 and game.global_vars[932] != 14 and game.global_vars[933] != 14 and game.global_vars[934] != 14 and game.global_vars[935] != 14 and game.global_vars[936] != 14";
                    return IsAreaKnown(14) && GetGlobalVar(931) != 14 && GetGlobalVar(932) != 14 && GetGlobalVar(933) != 14 && GetGlobalVar(934) != 14 && GetGlobalVar(935) != 14 && GetGlobalVar(936) != 14;
                case 154:
                case 254:
                case 354:
                case 454:
                case 554:
                case 654:
                    originalScript = "game.areas[14] == 1 and game.global_vars[931] != 15 and game.global_vars[932] != 15 and game.global_vars[933] != 15 and game.global_vars[934] != 15 and game.global_vars[935] != 15 and game.global_vars[936] != 15";
                    return IsAreaKnown(14) && GetGlobalVar(931) != 15 && GetGlobalVar(932) != 15 && GetGlobalVar(933) != 15 && GetGlobalVar(934) != 15 && GetGlobalVar(935) != 15 && GetGlobalVar(936) != 15;
                case 162:
                case 262:
                case 362:
                case 462:
                case 562:
                case 662:
                    originalScript = "game.areas[14] == 1 and game.global_vars[931] != 16 and game.global_vars[932] != 16 and game.global_vars[933] != 16 and game.global_vars[934] != 16 and game.global_vars[935] != 16 and game.global_vars[936] != 16";
                    return IsAreaKnown(14) && GetGlobalVar(931) != 16 && GetGlobalVar(932) != 16 && GetGlobalVar(933) != 16 && GetGlobalVar(934) != 16 && GetGlobalVar(935) != 16 && GetGlobalVar(936) != 16;
                case 163:
                case 263:
                case 363:
                case 463:
                case 563:
                case 663:
                    originalScript = "game.areas[14] == 1 and game.global_vars[931] != 17 and game.global_vars[932] != 17 and game.global_vars[933] != 17 and game.global_vars[934] != 17 and game.global_vars[935] != 17 and game.global_vars[936] != 17";
                    return IsAreaKnown(14) && GetGlobalVar(931) != 17 && GetGlobalVar(932) != 17 && GetGlobalVar(933) != 17 && GetGlobalVar(934) != 17 && GetGlobalVar(935) != 17 && GetGlobalVar(936) != 17;
                case 164:
                case 264:
                case 364:
                case 464:
                case 564:
                case 664:
                    originalScript = "game.areas[14] == 1 and game.global_vars[931] != 18 and game.global_vars[932] != 18 and game.global_vars[933] != 18 and game.global_vars[934] != 18 and game.global_vars[935] != 18 and game.global_vars[936] != 18";
                    return IsAreaKnown(14) && GetGlobalVar(931) != 18 && GetGlobalVar(932) != 18 && GetGlobalVar(933) != 18 && GetGlobalVar(934) != 18 && GetGlobalVar(935) != 18 && GetGlobalVar(936) != 18;
                case 172:
                case 272:
                case 372:
                case 472:
                case 572:
                case 672:
                    originalScript = "game.areas[5] == 1 and game.global_vars[931] != 19 and game.global_vars[932] != 19 and game.global_vars[933] != 19 and game.global_vars[934] != 19 and game.global_vars[935] != 19 and game.global_vars[936] != 19";
                    return IsAreaKnown(5) && GetGlobalVar(931) != 19 && GetGlobalVar(932) != 19 && GetGlobalVar(933) != 19 && GetGlobalVar(934) != 19 && GetGlobalVar(935) != 19 && GetGlobalVar(936) != 19;
                case 173:
                case 273:
                case 373:
                case 473:
                case 573:
                case 673:
                    originalScript = "game.areas[9] == 1 and game.global_vars[931] != 20 and game.global_vars[932] != 20 and game.global_vars[933] != 20 and game.global_vars[934] != 20 and game.global_vars[935] != 20 and game.global_vars[936] != 20";
                    return IsAreaKnown(9) && GetGlobalVar(931) != 20 && GetGlobalVar(932) != 20 && GetGlobalVar(933) != 20 && GetGlobalVar(934) != 20 && GetGlobalVar(935) != 20 && GetGlobalVar(936) != 20;
                case 174:
                case 274:
                case 374:
                case 474:
                case 574:
                case 674:
                    originalScript = "game.areas[7] == 1 and game.global_vars[931] != 21 and game.global_vars[932] != 21 and game.global_vars[933] != 21 and game.global_vars[934] != 21 and game.global_vars[935] != 21 and game.global_vars[936] != 21";
                    return IsAreaKnown(7) && GetGlobalVar(931) != 21 && GetGlobalVar(932) != 21 && GetGlobalVar(933) != 21 && GetGlobalVar(934) != 21 && GetGlobalVar(935) != 21 && GetGlobalVar(936) != 21;
                case 182:
                case 282:
                case 382:
                case 482:
                case 582:
                case 682:
                    originalScript = "game.areas[2] == 1 and game.global_vars[931] != 22 and game.global_vars[932] != 22 and game.global_vars[933] != 22 and game.global_vars[934] != 22 and game.global_vars[935] != 22 and game.global_vars[936] != 22";
                    return IsAreaKnown(2) && GetGlobalVar(931) != 22 && GetGlobalVar(932) != 22 && GetGlobalVar(933) != 22 && GetGlobalVar(934) != 22 && GetGlobalVar(935) != 22 && GetGlobalVar(936) != 22;
                case 183:
                case 283:
                case 383:
                case 483:
                case 583:
                case 683:
                    originalScript = "game.areas[2] == 1 and game.global_vars[931] != 23 and game.global_vars[932] != 23 and game.global_vars[933] != 23 and game.global_vars[934] != 23 and game.global_vars[935] != 23 and game.global_vars[936] != 23";
                    return IsAreaKnown(2) && GetGlobalVar(931) != 23 && GetGlobalVar(932) != 23 && GetGlobalVar(933) != 23 && GetGlobalVar(934) != 23 && GetGlobalVar(935) != 23 && GetGlobalVar(936) != 23;
                case 184:
                case 284:
                case 384:
                case 484:
                case 584:
                case 684:
                    originalScript = "game.areas[4] == 1 and game.global_vars[931] != 24 and game.global_vars[932] != 24 and game.global_vars[933] != 24 and game.global_vars[934] != 24 and game.global_vars[935] != 24 and game.global_vars[936] != 24";
                    return IsAreaKnown(4) && GetGlobalVar(931) != 24 && GetGlobalVar(932) != 24 && GetGlobalVar(933) != 24 && GetGlobalVar(934) != 24 && GetGlobalVar(935) != 24 && GetGlobalVar(936) != 24;
                case 192:
                case 292:
                case 392:
                case 492:
                case 592:
                case 692:
                    originalScript = "game.areas[4] == 1 and game.global_vars[931] != 25 and game.global_vars[932] != 25 and game.global_vars[933] != 25 and game.global_vars[934] != 25 and game.global_vars[935] != 25 and game.global_vars[936] != 25";
                    return IsAreaKnown(4) && GetGlobalVar(931) != 25 && GetGlobalVar(932) != 25 && GetGlobalVar(933) != 25 && GetGlobalVar(934) != 25 && GetGlobalVar(935) != 25 && GetGlobalVar(936) != 25;
                case 193:
                case 293:
                case 393:
                case 493:
                case 593:
                case 693:
                    originalScript = "game.areas[4] == 1 and game.global_vars[931] != 26 and game.global_vars[932] != 26 and game.global_vars[933] != 26 and game.global_vars[934] != 26 and game.global_vars[935] != 26 and game.global_vars[936] != 26";
                    return IsAreaKnown(4) && GetGlobalVar(931) != 26 && GetGlobalVar(932) != 26 && GetGlobalVar(933) != 26 && GetGlobalVar(934) != 26 && GetGlobalVar(935) != 26 && GetGlobalVar(936) != 26;
                case 194:
                case 294:
                case 394:
                case 494:
                case 594:
                case 694:
                    originalScript = "game.areas[4] == 1 and game.global_vars[931] != 27 and game.global_vars[932] != 27 and game.global_vars[933] != 27 and game.global_vars[934] != 27 and game.global_vars[935] != 27 and game.global_vars[936] != 27";
                    return IsAreaKnown(4) && GetGlobalVar(931) != 27 && GetGlobalVar(932) != 27 && GetGlobalVar(933) != 27 && GetGlobalVar(934) != 27 && GetGlobalVar(935) != 27 && GetGlobalVar(936) != 27;
                case 195:
                case 295:
                case 395:
                case 495:
                case 595:
                case 695:
                    originalScript = "game.areas[4] == 1 and game.global_vars[931] != 28 and game.global_vars[932] != 28 and game.global_vars[933] != 28 and game.global_vars[934] != 28 and game.global_vars[935] != 28 and game.global_vars[936] != 28";
                    return IsAreaKnown(4) && GetGlobalVar(931) != 28 && GetGlobalVar(932) != 28 && GetGlobalVar(933) != 28 && GetGlobalVar(934) != 28 && GetGlobalVar(935) != 28 && GetGlobalVar(936) != 28;
                case 711:
                    originalScript = "game.global_vars[931] == 1";
                    return GetGlobalVar(931) == 1;
                case 712:
                    originalScript = "game.global_vars[931] == 2";
                    return GetGlobalVar(931) == 2;
                case 713:
                    originalScript = "game.global_vars[931] == 3";
                    return GetGlobalVar(931) == 3;
                case 714:
                    originalScript = "game.global_vars[931] == 4";
                    return GetGlobalVar(931) == 4;
                case 715:
                    originalScript = "game.global_vars[931] == 5";
                    return GetGlobalVar(931) == 5;
                case 716:
                    originalScript = "game.global_vars[931] == 6";
                    return GetGlobalVar(931) == 6;
                case 717:
                    originalScript = "game.global_vars[931] == 7";
                    return GetGlobalVar(931) == 7;
                case 718:
                    originalScript = "game.global_vars[931] == 8";
                    return GetGlobalVar(931) == 8;
                case 719:
                    originalScript = "game.global_vars[931] == 9";
                    return GetGlobalVar(931) == 9;
                case 720:
                    originalScript = "game.global_vars[931] == 10 and game.global_flags[94] == 1";
                    return GetGlobalVar(931) == 10 && GetGlobalFlag(94);
                case 721:
                    originalScript = "game.global_vars[931] == 10 and game.global_flags[94] == 0";
                    return GetGlobalVar(931) == 10 && !GetGlobalFlag(94);
                case 722:
                    originalScript = "game.global_vars[931] == 11";
                    return GetGlobalVar(931) == 11;
                case 723:
                    originalScript = "game.global_vars[931] == 12";
                    return GetGlobalVar(931) == 12;
                case 724:
                    originalScript = "game.global_vars[931] == 13";
                    return GetGlobalVar(931) == 13;
                case 725:
                    originalScript = "game.global_vars[931] == 14";
                    return GetGlobalVar(931) == 14;
                case 726:
                    originalScript = "game.global_vars[931] == 15";
                    return GetGlobalVar(931) == 15;
                case 727:
                    originalScript = "game.global_vars[931] == 16";
                    return GetGlobalVar(931) == 16;
                case 728:
                    originalScript = "game.global_vars[931] == 17";
                    return GetGlobalVar(931) == 17;
                case 729:
                    originalScript = "game.global_vars[931] == 18";
                    return GetGlobalVar(931) == 18;
                case 730:
                    originalScript = "game.global_vars[931] == 19";
                    return GetGlobalVar(931) == 19;
                case 731:
                    originalScript = "game.global_vars[931] == 20";
                    return GetGlobalVar(931) == 20;
                case 732:
                    originalScript = "game.global_vars[931] == 21";
                    return GetGlobalVar(931) == 21;
                case 733:
                    originalScript = "game.global_vars[931] == 22";
                    return GetGlobalVar(931) == 22;
                case 734:
                    originalScript = "game.global_vars[931] == 23";
                    return GetGlobalVar(931) == 23;
                case 735:
                    originalScript = "game.global_vars[931] == 24";
                    return GetGlobalVar(931) == 24;
                case 736:
                    originalScript = "game.global_vars[931] == 25";
                    return GetGlobalVar(931) == 25;
                case 737:
                    originalScript = "game.global_vars[931] == 26";
                    return GetGlobalVar(931) == 26;
                case 738:
                    originalScript = "game.global_vars[931] == 27";
                    return GetGlobalVar(931) == 27;
                case 739:
                    originalScript = "game.global_vars[931] == 28";
                    return GetGlobalVar(931) == 28;
                case 811:
                    originalScript = "game.global_vars[932] == 1";
                    return GetGlobalVar(932) == 1;
                case 812:
                    originalScript = "game.global_vars[932] == 2";
                    return GetGlobalVar(932) == 2;
                case 813:
                    originalScript = "game.global_vars[932] == 3";
                    return GetGlobalVar(932) == 3;
                case 814:
                    originalScript = "game.global_vars[932] == 4";
                    return GetGlobalVar(932) == 4;
                case 815:
                    originalScript = "game.global_vars[932] == 5";
                    return GetGlobalVar(932) == 5;
                case 816:
                    originalScript = "game.global_vars[932] == 6";
                    return GetGlobalVar(932) == 6;
                case 817:
                    originalScript = "game.global_vars[932] == 7";
                    return GetGlobalVar(932) == 7;
                case 818:
                    originalScript = "game.global_vars[932] == 8";
                    return GetGlobalVar(932) == 8;
                case 819:
                    originalScript = "game.global_vars[932] == 9";
                    return GetGlobalVar(932) == 9;
                case 820:
                    originalScript = "game.global_vars[932] == 10 and game.global_flags[94] == 1";
                    return GetGlobalVar(932) == 10 && GetGlobalFlag(94);
                case 821:
                    originalScript = "game.global_vars[932] == 10 and game.global_flags[94] == 0";
                    return GetGlobalVar(932) == 10 && !GetGlobalFlag(94);
                case 822:
                    originalScript = "game.global_vars[932] == 11";
                    return GetGlobalVar(932) == 11;
                case 823:
                    originalScript = "game.global_vars[932] == 12";
                    return GetGlobalVar(932) == 12;
                case 824:
                    originalScript = "game.global_vars[932] == 13";
                    return GetGlobalVar(932) == 13;
                case 825:
                    originalScript = "game.global_vars[932] == 14";
                    return GetGlobalVar(932) == 14;
                case 826:
                    originalScript = "game.global_vars[932] == 15";
                    return GetGlobalVar(932) == 15;
                case 827:
                    originalScript = "game.global_vars[932] == 16";
                    return GetGlobalVar(932) == 16;
                case 828:
                    originalScript = "game.global_vars[932] == 17";
                    return GetGlobalVar(932) == 17;
                case 829:
                    originalScript = "game.global_vars[932] == 18";
                    return GetGlobalVar(932) == 18;
                case 830:
                    originalScript = "game.global_vars[932] == 19";
                    return GetGlobalVar(932) == 19;
                case 831:
                    originalScript = "game.global_vars[932] == 20";
                    return GetGlobalVar(932) == 20;
                case 832:
                    originalScript = "game.global_vars[932] == 21";
                    return GetGlobalVar(932) == 21;
                case 833:
                    originalScript = "game.global_vars[932] == 22";
                    return GetGlobalVar(932) == 22;
                case 834:
                    originalScript = "game.global_vars[932] == 23";
                    return GetGlobalVar(932) == 23;
                case 835:
                    originalScript = "game.global_vars[932] == 24";
                    return GetGlobalVar(932) == 24;
                case 836:
                    originalScript = "game.global_vars[932] == 25";
                    return GetGlobalVar(932) == 25;
                case 837:
                    originalScript = "game.global_vars[932] == 26";
                    return GetGlobalVar(932) == 26;
                case 838:
                    originalScript = "game.global_vars[932] == 27";
                    return GetGlobalVar(932) == 27;
                case 839:
                    originalScript = "game.global_vars[932] == 28";
                    return GetGlobalVar(932) == 28;
                case 911:
                    originalScript = "game.global_vars[933] == 1";
                    return GetGlobalVar(933) == 1;
                case 912:
                    originalScript = "game.global_vars[933] == 2";
                    return GetGlobalVar(933) == 2;
                case 913:
                    originalScript = "game.global_vars[933] == 3";
                    return GetGlobalVar(933) == 3;
                case 914:
                    originalScript = "game.global_vars[933] == 4";
                    return GetGlobalVar(933) == 4;
                case 915:
                    originalScript = "game.global_vars[933] == 5";
                    return GetGlobalVar(933) == 5;
                case 916:
                    originalScript = "game.global_vars[933] == 6";
                    return GetGlobalVar(933) == 6;
                case 917:
                    originalScript = "game.global_vars[933] == 7";
                    return GetGlobalVar(933) == 7;
                case 918:
                    originalScript = "game.global_vars[933] == 8";
                    return GetGlobalVar(933) == 8;
                case 919:
                    originalScript = "game.global_vars[933] == 9";
                    return GetGlobalVar(933) == 9;
                case 920:
                    originalScript = "game.global_vars[933] == 10 and game.global_flags[94] == 1";
                    return GetGlobalVar(933) == 10 && GetGlobalFlag(94);
                case 921:
                    originalScript = "game.global_vars[933] == 10 and game.global_flags[94] == 0";
                    return GetGlobalVar(933) == 10 && !GetGlobalFlag(94);
                case 922:
                    originalScript = "game.global_vars[933] == 11";
                    return GetGlobalVar(933) == 11;
                case 923:
                    originalScript = "game.global_vars[933] == 12";
                    return GetGlobalVar(933) == 12;
                case 924:
                    originalScript = "game.global_vars[933] == 13";
                    return GetGlobalVar(933) == 13;
                case 925:
                    originalScript = "game.global_vars[933] == 14";
                    return GetGlobalVar(933) == 14;
                case 926:
                    originalScript = "game.global_vars[933] == 15";
                    return GetGlobalVar(933) == 15;
                case 927:
                    originalScript = "game.global_vars[933] == 16";
                    return GetGlobalVar(933) == 16;
                case 928:
                    originalScript = "game.global_vars[933] == 17";
                    return GetGlobalVar(933) == 17;
                case 929:
                    originalScript = "game.global_vars[933] == 18";
                    return GetGlobalVar(933) == 18;
                case 930:
                    originalScript = "game.global_vars[933] == 19";
                    return GetGlobalVar(933) == 19;
                case 931:
                    originalScript = "game.global_vars[933] == 20";
                    return GetGlobalVar(933) == 20;
                case 932:
                    originalScript = "game.global_vars[933] == 21";
                    return GetGlobalVar(933) == 21;
                case 933:
                    originalScript = "game.global_vars[933] == 22";
                    return GetGlobalVar(933) == 22;
                case 934:
                    originalScript = "game.global_vars[933] == 23";
                    return GetGlobalVar(933) == 23;
                case 935:
                    originalScript = "game.global_vars[933] == 24";
                    return GetGlobalVar(933) == 24;
                case 936:
                    originalScript = "game.global_vars[933] == 25";
                    return GetGlobalVar(933) == 25;
                case 937:
                    originalScript = "game.global_vars[933] == 26";
                    return GetGlobalVar(933) == 26;
                case 938:
                    originalScript = "game.global_vars[933] == 27";
                    return GetGlobalVar(933) == 27;
                case 939:
                    originalScript = "game.global_vars[933] == 28";
                    return GetGlobalVar(933) == 28;
                case 1011:
                    originalScript = "game.global_vars[934] == 1";
                    return GetGlobalVar(934) == 1;
                case 1012:
                    originalScript = "game.global_vars[934] == 2";
                    return GetGlobalVar(934) == 2;
                case 1013:
                    originalScript = "game.global_vars[934] == 3";
                    return GetGlobalVar(934) == 3;
                case 1014:
                    originalScript = "game.global_vars[934] == 4";
                    return GetGlobalVar(934) == 4;
                case 1015:
                    originalScript = "game.global_vars[934] == 5";
                    return GetGlobalVar(934) == 5;
                case 1016:
                    originalScript = "game.global_vars[934] == 6";
                    return GetGlobalVar(934) == 6;
                case 1017:
                    originalScript = "game.global_vars[934] == 7";
                    return GetGlobalVar(934) == 7;
                case 1018:
                    originalScript = "game.global_vars[934] == 8";
                    return GetGlobalVar(934) == 8;
                case 1019:
                    originalScript = "game.global_vars[934] == 9";
                    return GetGlobalVar(934) == 9;
                case 1020:
                    originalScript = "game.global_vars[934] == 10 and game.global_flags[94] == 1";
                    return GetGlobalVar(934) == 10 && GetGlobalFlag(94);
                case 1021:
                    originalScript = "game.global_vars[934] == 10 and game.global_flags[94] == 0";
                    return GetGlobalVar(934) == 10 && !GetGlobalFlag(94);
                case 1022:
                    originalScript = "game.global_vars[934] == 11";
                    return GetGlobalVar(934) == 11;
                case 1023:
                    originalScript = "game.global_vars[934] == 12";
                    return GetGlobalVar(934) == 12;
                case 1024:
                    originalScript = "game.global_vars[934] == 13";
                    return GetGlobalVar(934) == 13;
                case 1025:
                    originalScript = "game.global_vars[934] == 14";
                    return GetGlobalVar(934) == 14;
                case 1026:
                    originalScript = "game.global_vars[934] == 15";
                    return GetGlobalVar(934) == 15;
                case 1027:
                    originalScript = "game.global_vars[934] == 16";
                    return GetGlobalVar(934) == 16;
                case 1028:
                    originalScript = "game.global_vars[934] == 17";
                    return GetGlobalVar(934) == 17;
                case 1029:
                    originalScript = "game.global_vars[934] == 18";
                    return GetGlobalVar(934) == 18;
                case 1030:
                    originalScript = "game.global_vars[934] == 19";
                    return GetGlobalVar(934) == 19;
                case 1031:
                    originalScript = "game.global_vars[934] == 20";
                    return GetGlobalVar(934) == 20;
                case 1032:
                    originalScript = "game.global_vars[934] == 21";
                    return GetGlobalVar(934) == 21;
                case 1033:
                    originalScript = "game.global_vars[934] == 22";
                    return GetGlobalVar(934) == 22;
                case 1034:
                    originalScript = "game.global_vars[934] == 23";
                    return GetGlobalVar(934) == 23;
                case 1035:
                    originalScript = "game.global_vars[934] == 24";
                    return GetGlobalVar(934) == 24;
                case 1036:
                    originalScript = "game.global_vars[934] == 25";
                    return GetGlobalVar(934) == 25;
                case 1037:
                    originalScript = "game.global_vars[934] == 26";
                    return GetGlobalVar(934) == 26;
                case 1038:
                    originalScript = "game.global_vars[934] == 27";
                    return GetGlobalVar(934) == 27;
                case 1039:
                    originalScript = "game.global_vars[934] == 28";
                    return GetGlobalVar(934) == 28;
                case 1111:
                    originalScript = "game.global_vars[935] == 1";
                    return GetGlobalVar(935) == 1;
                case 1112:
                    originalScript = "game.global_vars[935] == 2";
                    return GetGlobalVar(935) == 2;
                case 1113:
                    originalScript = "game.global_vars[935] == 3";
                    return GetGlobalVar(935) == 3;
                case 1114:
                    originalScript = "game.global_vars[935] == 4";
                    return GetGlobalVar(935) == 4;
                case 1115:
                    originalScript = "game.global_vars[935] == 5";
                    return GetGlobalVar(935) == 5;
                case 1116:
                    originalScript = "game.global_vars[935] == 6";
                    return GetGlobalVar(935) == 6;
                case 1117:
                    originalScript = "game.global_vars[935] == 7";
                    return GetGlobalVar(935) == 7;
                case 1118:
                    originalScript = "game.global_vars[935] == 8";
                    return GetGlobalVar(935) == 8;
                case 1119:
                    originalScript = "game.global_vars[935] == 9";
                    return GetGlobalVar(935) == 9;
                case 1120:
                    originalScript = "game.global_vars[935] == 10 and game.global_flags[94] == 1";
                    return GetGlobalVar(935) == 10 && GetGlobalFlag(94);
                case 1121:
                    originalScript = "game.global_vars[935] == 10 and game.global_flags[94] == 0";
                    return GetGlobalVar(935) == 10 && !GetGlobalFlag(94);
                case 1122:
                    originalScript = "game.global_vars[935] == 11";
                    return GetGlobalVar(935) == 11;
                case 1123:
                    originalScript = "game.global_vars[935] == 12";
                    return GetGlobalVar(935) == 12;
                case 1124:
                    originalScript = "game.global_vars[935] == 13";
                    return GetGlobalVar(935) == 13;
                case 1125:
                    originalScript = "game.global_vars[935] == 14";
                    return GetGlobalVar(935) == 14;
                case 1126:
                    originalScript = "game.global_vars[935] == 15";
                    return GetGlobalVar(935) == 15;
                case 1127:
                    originalScript = "game.global_vars[935] == 16";
                    return GetGlobalVar(935) == 16;
                case 1128:
                    originalScript = "game.global_vars[935] == 17";
                    return GetGlobalVar(935) == 17;
                case 1129:
                    originalScript = "game.global_vars[935] == 18";
                    return GetGlobalVar(935) == 18;
                case 1130:
                    originalScript = "game.global_vars[935] == 19";
                    return GetGlobalVar(935) == 19;
                case 1131:
                    originalScript = "game.global_vars[935] == 20";
                    return GetGlobalVar(935) == 20;
                case 1132:
                    originalScript = "game.global_vars[935] == 21";
                    return GetGlobalVar(935) == 21;
                case 1133:
                    originalScript = "game.global_vars[935] == 22";
                    return GetGlobalVar(935) == 22;
                case 1134:
                    originalScript = "game.global_vars[935] == 23";
                    return GetGlobalVar(935) == 23;
                case 1135:
                    originalScript = "game.global_vars[935] == 24";
                    return GetGlobalVar(935) == 24;
                case 1136:
                    originalScript = "game.global_vars[935] == 25";
                    return GetGlobalVar(935) == 25;
                case 1137:
                    originalScript = "game.global_vars[935] == 26";
                    return GetGlobalVar(935) == 26;
                case 1138:
                    originalScript = "game.global_vars[935] == 27";
                    return GetGlobalVar(935) == 27;
                case 1139:
                    originalScript = "game.global_vars[935] == 28";
                    return GetGlobalVar(935) == 28;
                case 1211:
                    originalScript = "game.global_vars[936] == 1";
                    return GetGlobalVar(936) == 1;
                case 1212:
                    originalScript = "game.global_vars[936] == 2";
                    return GetGlobalVar(936) == 2;
                case 1213:
                    originalScript = "game.global_vars[936] == 3";
                    return GetGlobalVar(936) == 3;
                case 1214:
                    originalScript = "game.global_vars[936] == 4";
                    return GetGlobalVar(936) == 4;
                case 1215:
                    originalScript = "game.global_vars[936] == 5";
                    return GetGlobalVar(936) == 5;
                case 1216:
                    originalScript = "game.global_vars[936] == 6";
                    return GetGlobalVar(936) == 6;
                case 1217:
                    originalScript = "game.global_vars[936] == 7";
                    return GetGlobalVar(936) == 7;
                case 1218:
                    originalScript = "game.global_vars[936] == 8";
                    return GetGlobalVar(936) == 8;
                case 1219:
                    originalScript = "game.global_vars[936] == 9";
                    return GetGlobalVar(936) == 9;
                case 1220:
                    originalScript = "game.global_vars[936] == 10 and game.global_flags[94] == 1";
                    return GetGlobalVar(936) == 10 && GetGlobalFlag(94);
                case 1221:
                    originalScript = "game.global_vars[936] == 10 and game.global_flags[94] == 0";
                    return GetGlobalVar(936) == 10 && !GetGlobalFlag(94);
                case 1222:
                    originalScript = "game.global_vars[936] == 11";
                    return GetGlobalVar(936) == 11;
                case 1223:
                    originalScript = "game.global_vars[936] == 12";
                    return GetGlobalVar(936) == 12;
                case 1224:
                    originalScript = "game.global_vars[936] == 13";
                    return GetGlobalVar(936) == 13;
                case 1225:
                    originalScript = "game.global_vars[936] == 14";
                    return GetGlobalVar(936) == 14;
                case 1226:
                    originalScript = "game.global_vars[936] == 15";
                    return GetGlobalVar(936) == 15;
                case 1227:
                    originalScript = "game.global_vars[936] == 16";
                    return GetGlobalVar(936) == 16;
                case 1228:
                    originalScript = "game.global_vars[936] == 17";
                    return GetGlobalVar(936) == 17;
                case 1229:
                    originalScript = "game.global_vars[936] == 18";
                    return GetGlobalVar(936) == 18;
                case 1230:
                    originalScript = "game.global_vars[936] == 19";
                    return GetGlobalVar(936) == 19;
                case 1231:
                    originalScript = "game.global_vars[936] == 20";
                    return GetGlobalVar(936) == 20;
                case 1232:
                    originalScript = "game.global_vars[936] == 21";
                    return GetGlobalVar(936) == 21;
                case 1233:
                    originalScript = "game.global_vars[936] == 22";
                    return GetGlobalVar(936) == 22;
                case 1234:
                    originalScript = "game.global_vars[936] == 23";
                    return GetGlobalVar(936) == 23;
                case 1235:
                    originalScript = "game.global_vars[936] == 24";
                    return GetGlobalVar(936) == 24;
                case 1236:
                    originalScript = "game.global_vars[936] == 25";
                    return GetGlobalVar(936) == 25;
                case 1237:
                    originalScript = "game.global_vars[936] == 26";
                    return GetGlobalVar(936) == 26;
                case 1238:
                    originalScript = "game.global_vars[936] == 27";
                    return GetGlobalVar(936) == 27;
                case 1239:
                    originalScript = "game.global_vars[936] == 28";
                    return GetGlobalVar(936) == 28;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 110:
                case 210:
                case 310:
                case 410:
                case 510:
                case 610:
                    originalScript = "game.particles( 'hit-CHAOTIC-medium', npc ); game.sound( 4023 )";
                    AttachParticles("hit-CHAOTIC-medium", npc);
                    Sound(4023);
                    ;
                    break;
                case 112:
                    originalScript = "game.global_vars[931] = 1";
                    SetGlobalVar(931, 1);
                    break;
                case 113:
                    originalScript = "game.global_vars[931] = 2";
                    SetGlobalVar(931, 2);
                    break;
                case 114:
                    originalScript = "game.global_vars[931] = 3";
                    SetGlobalVar(931, 3);
                    break;
                case 122:
                    originalScript = "game.global_vars[931] = 4";
                    SetGlobalVar(931, 4);
                    break;
                case 123:
                    originalScript = "game.global_vars[931] = 5";
                    SetGlobalVar(931, 5);
                    break;
                case 124:
                    originalScript = "game.global_vars[931] = 6";
                    SetGlobalVar(931, 6);
                    break;
                case 132:
                    originalScript = "game.global_vars[931] = 7";
                    SetGlobalVar(931, 7);
                    break;
                case 133:
                    originalScript = "game.global_vars[931] = 8";
                    SetGlobalVar(931, 8);
                    break;
                case 134:
                    originalScript = "game.global_vars[931] = 9";
                    SetGlobalVar(931, 9);
                    break;
                case 142:
                case 143:
                    originalScript = "game.global_vars[931] = 10";
                    SetGlobalVar(931, 10);
                    break;
                case 144:
                    originalScript = "game.global_vars[931] = 11";
                    SetGlobalVar(931, 11);
                    break;
                case 145:
                    originalScript = "game.global_vars[931] = 12";
                    SetGlobalVar(931, 12);
                    break;
                case 152:
                    originalScript = "game.global_vars[931] = 13";
                    SetGlobalVar(931, 13);
                    break;
                case 153:
                    originalScript = "game.global_vars[931] = 14";
                    SetGlobalVar(931, 14);
                    break;
                case 154:
                    originalScript = "game.global_vars[931] = 15";
                    SetGlobalVar(931, 15);
                    break;
                case 162:
                    originalScript = "game.global_vars[931] = 16";
                    SetGlobalVar(931, 16);
                    break;
                case 163:
                    originalScript = "game.global_vars[931] = 17";
                    SetGlobalVar(931, 17);
                    break;
                case 164:
                    originalScript = "game.global_vars[931] = 18";
                    SetGlobalVar(931, 18);
                    break;
                case 172:
                    originalScript = "game.global_vars[931] = 19";
                    SetGlobalVar(931, 19);
                    break;
                case 173:
                    originalScript = "game.global_vars[931] = 20";
                    SetGlobalVar(931, 20);
                    break;
                case 174:
                    originalScript = "game.global_vars[931] = 21";
                    SetGlobalVar(931, 21);
                    break;
                case 182:
                    originalScript = "game.global_vars[931] = 22";
                    SetGlobalVar(931, 22);
                    break;
                case 183:
                    originalScript = "game.global_vars[931] = 23";
                    SetGlobalVar(931, 23);
                    break;
                case 184:
                    originalScript = "game.global_vars[931] = 24";
                    SetGlobalVar(931, 24);
                    break;
                case 192:
                    originalScript = "game.global_vars[931] = 25";
                    SetGlobalVar(931, 25);
                    break;
                case 193:
                    originalScript = "game.global_vars[931] = 26";
                    SetGlobalVar(931, 26);
                    break;
                case 194:
                    originalScript = "game.global_vars[931] = 27";
                    SetGlobalVar(931, 27);
                    break;
                case 195:
                    originalScript = "game.global_vars[931] = 28";
                    SetGlobalVar(931, 28);
                    break;
                case 212:
                    originalScript = "game.global_vars[932] = 1";
                    SetGlobalVar(932, 1);
                    break;
                case 213:
                    originalScript = "game.global_vars[932] = 2";
                    SetGlobalVar(932, 2);
                    break;
                case 214:
                    originalScript = "game.global_vars[932] = 3";
                    SetGlobalVar(932, 3);
                    break;
                case 222:
                    originalScript = "game.global_vars[932] = 4";
                    SetGlobalVar(932, 4);
                    break;
                case 223:
                    originalScript = "game.global_vars[932] = 5";
                    SetGlobalVar(932, 5);
                    break;
                case 224:
                    originalScript = "game.global_vars[932] = 6";
                    SetGlobalVar(932, 6);
                    break;
                case 232:
                    originalScript = "game.global_vars[932] = 7";
                    SetGlobalVar(932, 7);
                    break;
                case 233:
                    originalScript = "game.global_vars[932] = 8";
                    SetGlobalVar(932, 8);
                    break;
                case 234:
                    originalScript = "game.global_vars[932] = 9";
                    SetGlobalVar(932, 9);
                    break;
                case 242:
                case 243:
                    originalScript = "game.global_vars[932] = 10";
                    SetGlobalVar(932, 10);
                    break;
                case 244:
                    originalScript = "game.global_vars[932] = 11";
                    SetGlobalVar(932, 11);
                    break;
                case 245:
                    originalScript = "game.global_vars[932] = 12";
                    SetGlobalVar(932, 12);
                    break;
                case 252:
                    originalScript = "game.global_vars[932] = 13";
                    SetGlobalVar(932, 13);
                    break;
                case 253:
                    originalScript = "game.global_vars[932] = 14";
                    SetGlobalVar(932, 14);
                    break;
                case 254:
                    originalScript = "game.global_vars[932] = 15";
                    SetGlobalVar(932, 15);
                    break;
                case 262:
                    originalScript = "game.global_vars[932] = 16";
                    SetGlobalVar(932, 16);
                    break;
                case 263:
                    originalScript = "game.global_vars[932] = 17";
                    SetGlobalVar(932, 17);
                    break;
                case 264:
                    originalScript = "game.global_vars[932] = 18";
                    SetGlobalVar(932, 18);
                    break;
                case 272:
                    originalScript = "game.global_vars[932] = 19";
                    SetGlobalVar(932, 19);
                    break;
                case 273:
                    originalScript = "game.global_vars[932] = 20";
                    SetGlobalVar(932, 20);
                    break;
                case 274:
                    originalScript = "game.global_vars[932] = 21";
                    SetGlobalVar(932, 21);
                    break;
                case 282:
                    originalScript = "game.global_vars[932] = 22";
                    SetGlobalVar(932, 22);
                    break;
                case 283:
                    originalScript = "game.global_vars[932] = 23";
                    SetGlobalVar(932, 23);
                    break;
                case 284:
                    originalScript = "game.global_vars[932] = 24";
                    SetGlobalVar(932, 24);
                    break;
                case 292:
                    originalScript = "game.global_vars[932] = 25";
                    SetGlobalVar(932, 25);
                    break;
                case 293:
                    originalScript = "game.global_vars[932] = 26";
                    SetGlobalVar(932, 26);
                    break;
                case 294:
                    originalScript = "game.global_vars[932] = 27";
                    SetGlobalVar(932, 27);
                    break;
                case 295:
                    originalScript = "game.global_vars[932] = 28";
                    SetGlobalVar(932, 28);
                    break;
                case 312:
                    originalScript = "game.global_vars[933] = 1";
                    SetGlobalVar(933, 1);
                    break;
                case 313:
                    originalScript = "game.global_vars[933] = 2";
                    SetGlobalVar(933, 2);
                    break;
                case 314:
                    originalScript = "game.global_vars[933] = 3";
                    SetGlobalVar(933, 3);
                    break;
                case 322:
                    originalScript = "game.global_vars[933] = 4";
                    SetGlobalVar(933, 4);
                    break;
                case 323:
                    originalScript = "game.global_vars[933] = 5";
                    SetGlobalVar(933, 5);
                    break;
                case 324:
                    originalScript = "game.global_vars[933] = 6";
                    SetGlobalVar(933, 6);
                    break;
                case 332:
                    originalScript = "game.global_vars[933] = 7";
                    SetGlobalVar(933, 7);
                    break;
                case 333:
                    originalScript = "game.global_vars[933] = 8";
                    SetGlobalVar(933, 8);
                    break;
                case 334:
                    originalScript = "game.global_vars[933] = 9";
                    SetGlobalVar(933, 9);
                    break;
                case 342:
                case 343:
                    originalScript = "game.global_vars[933] = 10";
                    SetGlobalVar(933, 10);
                    break;
                case 344:
                    originalScript = "game.global_vars[933] = 11";
                    SetGlobalVar(933, 11);
                    break;
                case 345:
                    originalScript = "game.global_vars[933] = 12";
                    SetGlobalVar(933, 12);
                    break;
                case 352:
                    originalScript = "game.global_vars[933] = 13";
                    SetGlobalVar(933, 13);
                    break;
                case 353:
                    originalScript = "game.global_vars[933] = 14";
                    SetGlobalVar(933, 14);
                    break;
                case 354:
                    originalScript = "game.global_vars[933] = 15";
                    SetGlobalVar(933, 15);
                    break;
                case 362:
                    originalScript = "game.global_vars[933] = 16";
                    SetGlobalVar(933, 16);
                    break;
                case 363:
                    originalScript = "game.global_vars[933] = 17";
                    SetGlobalVar(933, 17);
                    break;
                case 364:
                    originalScript = "game.global_vars[933] = 18";
                    SetGlobalVar(933, 18);
                    break;
                case 372:
                    originalScript = "game.global_vars[933] = 19";
                    SetGlobalVar(933, 19);
                    break;
                case 373:
                    originalScript = "game.global_vars[933] = 20";
                    SetGlobalVar(933, 20);
                    break;
                case 374:
                    originalScript = "game.global_vars[933] = 21";
                    SetGlobalVar(933, 21);
                    break;
                case 382:
                    originalScript = "game.global_vars[933] = 22";
                    SetGlobalVar(933, 22);
                    break;
                case 383:
                    originalScript = "game.global_vars[933] = 23";
                    SetGlobalVar(933, 23);
                    break;
                case 384:
                    originalScript = "game.global_vars[933] = 24";
                    SetGlobalVar(933, 24);
                    break;
                case 392:
                    originalScript = "game.global_vars[933] = 25";
                    SetGlobalVar(933, 25);
                    break;
                case 393:
                    originalScript = "game.global_vars[933] = 26";
                    SetGlobalVar(933, 26);
                    break;
                case 394:
                    originalScript = "game.global_vars[933] = 27";
                    SetGlobalVar(933, 27);
                    break;
                case 395:
                    originalScript = "game.global_vars[933] = 28";
                    SetGlobalVar(933, 28);
                    break;
                case 412:
                    originalScript = "game.global_vars[934] = 1";
                    SetGlobalVar(934, 1);
                    break;
                case 413:
                    originalScript = "game.global_vars[934] = 2";
                    SetGlobalVar(934, 2);
                    break;
                case 414:
                    originalScript = "game.global_vars[934] = 3";
                    SetGlobalVar(934, 3);
                    break;
                case 422:
                    originalScript = "game.global_vars[934] = 4";
                    SetGlobalVar(934, 4);
                    break;
                case 423:
                    originalScript = "game.global_vars[934] = 5";
                    SetGlobalVar(934, 5);
                    break;
                case 424:
                    originalScript = "game.global_vars[934] = 6";
                    SetGlobalVar(934, 6);
                    break;
                case 432:
                    originalScript = "game.global_vars[934] = 7";
                    SetGlobalVar(934, 7);
                    break;
                case 433:
                    originalScript = "game.global_vars[934] = 8";
                    SetGlobalVar(934, 8);
                    break;
                case 434:
                    originalScript = "game.global_vars[934] = 9";
                    SetGlobalVar(934, 9);
                    break;
                case 442:
                case 443:
                    originalScript = "game.global_vars[934] = 10";
                    SetGlobalVar(934, 10);
                    break;
                case 444:
                    originalScript = "game.global_vars[934] = 11";
                    SetGlobalVar(934, 11);
                    break;
                case 445:
                    originalScript = "game.global_vars[934] = 12";
                    SetGlobalVar(934, 12);
                    break;
                case 452:
                    originalScript = "game.global_vars[934] = 13";
                    SetGlobalVar(934, 13);
                    break;
                case 453:
                    originalScript = "game.global_vars[934] = 14";
                    SetGlobalVar(934, 14);
                    break;
                case 454:
                    originalScript = "game.global_vars[934] = 15";
                    SetGlobalVar(934, 15);
                    break;
                case 462:
                    originalScript = "game.global_vars[934] = 16";
                    SetGlobalVar(934, 16);
                    break;
                case 463:
                    originalScript = "game.global_vars[934] = 17";
                    SetGlobalVar(934, 17);
                    break;
                case 464:
                    originalScript = "game.global_vars[934] = 18";
                    SetGlobalVar(934, 18);
                    break;
                case 472:
                    originalScript = "game.global_vars[934] = 19";
                    SetGlobalVar(934, 19);
                    break;
                case 473:
                    originalScript = "game.global_vars[934] = 20";
                    SetGlobalVar(934, 20);
                    break;
                case 474:
                    originalScript = "game.global_vars[934] = 21";
                    SetGlobalVar(934, 21);
                    break;
                case 482:
                    originalScript = "game.global_vars[934] = 22";
                    SetGlobalVar(934, 22);
                    break;
                case 483:
                    originalScript = "game.global_vars[934] = 23";
                    SetGlobalVar(934, 23);
                    break;
                case 484:
                    originalScript = "game.global_vars[934] = 24";
                    SetGlobalVar(934, 24);
                    break;
                case 492:
                    originalScript = "game.global_vars[934] = 25";
                    SetGlobalVar(934, 25);
                    break;
                case 493:
                    originalScript = "game.global_vars[934] = 26";
                    SetGlobalVar(934, 26);
                    break;
                case 494:
                    originalScript = "game.global_vars[934] = 27";
                    SetGlobalVar(934, 27);
                    break;
                case 495:
                    originalScript = "game.global_vars[934] = 28";
                    SetGlobalVar(934, 28);
                    break;
                case 512:
                    originalScript = "game.global_vars[935] = 1";
                    SetGlobalVar(935, 1);
                    break;
                case 513:
                    originalScript = "game.global_vars[935] = 2";
                    SetGlobalVar(935, 2);
                    break;
                case 514:
                    originalScript = "game.global_vars[935] = 3";
                    SetGlobalVar(935, 3);
                    break;
                case 522:
                    originalScript = "game.global_vars[935] = 4";
                    SetGlobalVar(935, 4);
                    break;
                case 523:
                    originalScript = "game.global_vars[935] = 5";
                    SetGlobalVar(935, 5);
                    break;
                case 524:
                    originalScript = "game.global_vars[935] = 6";
                    SetGlobalVar(935, 6);
                    break;
                case 532:
                    originalScript = "game.global_vars[935] = 7";
                    SetGlobalVar(935, 7);
                    break;
                case 533:
                    originalScript = "game.global_vars[935] = 8";
                    SetGlobalVar(935, 8);
                    break;
                case 534:
                    originalScript = "game.global_vars[935] = 9";
                    SetGlobalVar(935, 9);
                    break;
                case 542:
                case 543:
                    originalScript = "game.global_vars[935] = 10";
                    SetGlobalVar(935, 10);
                    break;
                case 544:
                    originalScript = "game.global_vars[935] = 11";
                    SetGlobalVar(935, 11);
                    break;
                case 545:
                    originalScript = "game.global_vars[935] = 12";
                    SetGlobalVar(935, 12);
                    break;
                case 552:
                    originalScript = "game.global_vars[935] = 13";
                    SetGlobalVar(935, 13);
                    break;
                case 553:
                    originalScript = "game.global_vars[935] = 14";
                    SetGlobalVar(935, 14);
                    break;
                case 554:
                    originalScript = "game.global_vars[935] = 15";
                    SetGlobalVar(935, 15);
                    break;
                case 562:
                    originalScript = "game.global_vars[935] = 16";
                    SetGlobalVar(935, 16);
                    break;
                case 563:
                    originalScript = "game.global_vars[935] = 17";
                    SetGlobalVar(935, 17);
                    break;
                case 564:
                    originalScript = "game.global_vars[935] = 18";
                    SetGlobalVar(935, 18);
                    break;
                case 572:
                    originalScript = "game.global_vars[935] = 19";
                    SetGlobalVar(935, 19);
                    break;
                case 573:
                    originalScript = "game.global_vars[935] = 20";
                    SetGlobalVar(935, 20);
                    break;
                case 574:
                    originalScript = "game.global_vars[935] = 21";
                    SetGlobalVar(935, 21);
                    break;
                case 582:
                    originalScript = "game.global_vars[935] = 22";
                    SetGlobalVar(935, 22);
                    break;
                case 583:
                    originalScript = "game.global_vars[935] = 23";
                    SetGlobalVar(935, 23);
                    break;
                case 584:
                    originalScript = "game.global_vars[935] = 24";
                    SetGlobalVar(935, 24);
                    break;
                case 592:
                    originalScript = "game.global_vars[935] = 25";
                    SetGlobalVar(935, 25);
                    break;
                case 593:
                    originalScript = "game.global_vars[935] = 26";
                    SetGlobalVar(935, 26);
                    break;
                case 594:
                    originalScript = "game.global_vars[935] = 27";
                    SetGlobalVar(935, 27);
                    break;
                case 595:
                    originalScript = "game.global_vars[935] = 28";
                    SetGlobalVar(935, 28);
                    break;
                case 612:
                    originalScript = "game.global_vars[936] = 1";
                    SetGlobalVar(936, 1);
                    break;
                case 613:
                    originalScript = "game.global_vars[936] = 2";
                    SetGlobalVar(936, 2);
                    break;
                case 614:
                    originalScript = "game.global_vars[936] = 3";
                    SetGlobalVar(936, 3);
                    break;
                case 622:
                    originalScript = "game.global_vars[936] = 4";
                    SetGlobalVar(936, 4);
                    break;
                case 623:
                    originalScript = "game.global_vars[936] = 5";
                    SetGlobalVar(936, 5);
                    break;
                case 624:
                    originalScript = "game.global_vars[936] = 6";
                    SetGlobalVar(936, 6);
                    break;
                case 632:
                    originalScript = "game.global_vars[936] = 7";
                    SetGlobalVar(936, 7);
                    break;
                case 633:
                    originalScript = "game.global_vars[936] = 8";
                    SetGlobalVar(936, 8);
                    break;
                case 634:
                    originalScript = "game.global_vars[936] = 9";
                    SetGlobalVar(936, 9);
                    break;
                case 642:
                case 643:
                    originalScript = "game.global_vars[936] = 10";
                    SetGlobalVar(936, 10);
                    break;
                case 644:
                    originalScript = "game.global_vars[936] = 11";
                    SetGlobalVar(936, 11);
                    break;
                case 645:
                    originalScript = "game.global_vars[936] = 12";
                    SetGlobalVar(936, 12);
                    break;
                case 652:
                    originalScript = "game.global_vars[936] = 13";
                    SetGlobalVar(936, 13);
                    break;
                case 653:
                    originalScript = "game.global_vars[936] = 14";
                    SetGlobalVar(936, 14);
                    break;
                case 654:
                    originalScript = "game.global_vars[936] = 15";
                    SetGlobalVar(936, 15);
                    break;
                case 662:
                    originalScript = "game.global_vars[936] = 16";
                    SetGlobalVar(936, 16);
                    break;
                case 663:
                    originalScript = "game.global_vars[936] = 17";
                    SetGlobalVar(936, 17);
                    break;
                case 664:
                    originalScript = "game.global_vars[936] = 18";
                    SetGlobalVar(936, 18);
                    break;
                case 672:
                    originalScript = "game.global_vars[936] = 19";
                    SetGlobalVar(936, 19);
                    break;
                case 673:
                    originalScript = "game.global_vars[936] = 20";
                    SetGlobalVar(936, 20);
                    break;
                case 674:
                    originalScript = "game.global_vars[936] = 21";
                    SetGlobalVar(936, 21);
                    break;
                case 682:
                    originalScript = "game.global_vars[936] = 22";
                    SetGlobalVar(936, 22);
                    break;
                case 683:
                    originalScript = "game.global_vars[936] = 23";
                    SetGlobalVar(936, 23);
                    break;
                case 684:
                    originalScript = "game.global_vars[936] = 24";
                    SetGlobalVar(936, 24);
                    break;
                case 692:
                    originalScript = "game.global_vars[936] = 25";
                    SetGlobalVar(936, 25);
                    break;
                case 693:
                    originalScript = "game.global_vars[936] = 26";
                    SetGlobalVar(936, 26);
                    break;
                case 694:
                    originalScript = "game.global_vars[936] = 27";
                    SetGlobalVar(936, 27);
                    break;
                case 695:
                    originalScript = "game.global_vars[936] = 28";
                    SetGlobalVar(936, 28);
                    break;
                case 710:
                    originalScript = "game.particles( 'ef-NODE-Air portal_1', npc ); game.sound( 4008 )";
                    AttachParticles("ef-NODE-Air portal_1", npc);
                    Sound(4008);
                    ;
                    break;
                case 711:
                case 811:
                case 911:
                case 1011:
                case 1111:
                case 1211:
                    originalScript = "teleport_hommlet(npc,pc)";
                    teleport_hommlet(npc, pc);
                    break;
                case 712:
                case 812:
                case 912:
                case 1012:
                case 1112:
                case 1212:
                    originalScript = "teleport_nulb(npc,pc)";
                    teleport_nulb(npc, pc);
                    break;
                case 713:
                case 813:
                case 913:
                case 1013:
                case 1113:
                case 1213:
                    originalScript = "teleport_verbobonc(npc,pc)";
                    teleport_verbobonc(npc, pc);
                    break;
                case 714:
                case 814:
                case 914:
                case 1014:
                case 1114:
                case 1214:
                    originalScript = "teleport_tower(npc,pc)";
                    teleport_tower(npc, pc);
                    break;
                case 715:
                case 815:
                case 915:
                case 1015:
                case 1115:
                case 1215:
                    originalScript = "teleport_cuthbert(npc,pc)";
                    teleport_cuthbert(npc, pc);
                    break;
                case 716:
                case 816:
                case 916:
                case 1016:
                case 1116:
                case 1216:
                    originalScript = "teleport_grove(npc,pc)";
                    teleport_grove(npc, pc);
                    break;
                case 717:
                case 817:
                case 917:
                case 1017:
                case 1117:
                case 1217:
                    originalScript = "teleport_wench(npc,pc)";
                    teleport_wench(npc, pc);
                    break;
                case 718:
                case 818:
                case 918:
                case 1018:
                case 1118:
                case 1218:
                    originalScript = "teleport_smyth(npc,pc)";
                    teleport_smyth(npc, pc);
                    break;
                case 719:
                case 819:
                case 919:
                case 1019:
                case 1119:
                case 1219:
                    originalScript = "teleport_trader(npc,pc)";
                    teleport_trader(npc, pc);
                    break;
                case 720:
                case 820:
                case 920:
                case 1020:
                case 1120:
                case 1220:
                    originalScript = "teleport_residence(npc,pc)";
                    teleport_residence(npc, pc);
                    break;
                case 721:
                case 821:
                case 921:
                case 1021:
                case 1121:
                case 1221:
                    originalScript = "teleport_hostel(npc,pc)";
                    teleport_hostel(npc, pc);
                    break;
                case 722:
                case 822:
                case 922:
                case 1022:
                case 1122:
                case 1222:
                    originalScript = "teleport_otis(npc,pc)";
                    teleport_otis(npc, pc);
                    break;
                case 723:
                case 823:
                case 923:
                case 1023:
                case 1123:
                case 1223:
                    originalScript = "teleport_fong(npc,pc)";
                    teleport_fong(npc, pc);
                    break;
                case 724:
                case 824:
                case 924:
                case 1024:
                case 1124:
                case 1224:
                    originalScript = "teleport_cityhall(npc,pc)";
                    teleport_cityhall(npc, pc);
                    break;
                case 725:
                case 825:
                case 925:
                case 1025:
                case 1125:
                case 1225:
                    originalScript = "teleport_constabulary(npc,pc)";
                    teleport_constabulary(npc, pc);
                    break;
                case 726:
                case 826:
                case 926:
                case 1026:
                case 1126:
                case 1226:
                    originalScript = "teleport_pelor(npc,pc)";
                    teleport_pelor(npc, pc);
                    break;
                case 727:
                case 827:
                case 927:
                case 1027:
                case 1127:
                case 1227:
                    originalScript = "teleport_goose(npc,pc)";
                    teleport_goose(npc, pc);
                    break;
                case 728:
                case 828:
                case 928:
                case 1028:
                case 1128:
                case 1228:
                    originalScript = "teleport_archers(npc,pc)";
                    teleport_archers(npc, pc);
                    break;
                case 729:
                case 829:
                case 929:
                case 1029:
                case 1129:
                case 1229:
                    originalScript = "teleport_bazaar(npc,pc)";
                    teleport_bazaar(npc, pc);
                    break;
                case 730:
                case 830:
                case 930:
                case 1030:
                case 1130:
                case 1230:
                    originalScript = "teleport_emridy(npc,pc)";
                    teleport_emridy(npc, pc);
                    break;
                case 731:
                case 831:
                case 931:
                case 1031:
                case 1131:
                case 1231:
                    originalScript = "teleport_hickory(npc,pc)";
                    teleport_hickory(npc, pc);
                    break;
                case 732:
                case 832:
                case 932:
                case 1032:
                case 1132:
                case 1232:
                    originalScript = "teleport_welkwood(npc,pc)";
                    teleport_welkwood(npc, pc);
                    break;
                case 733:
                case 833:
                case 933:
                case 1033:
                case 1133:
                case 1233:
                    originalScript = "teleport_moathouse(npc,pc)";
                    teleport_moathouse(npc, pc);
                    break;
                case 734:
                case 834:
                case 934:
                case 1034:
                case 1134:
                case 1234:
                    originalScript = "teleport_moatdung(npc,pc)";
                    teleport_moatdung(npc, pc);
                    break;
                case 735:
                case 835:
                case 935:
                case 1035:
                case 1135:
                case 1235:
                    originalScript = "teleport_toee(npc,pc)";
                    teleport_toee(npc, pc);
                    break;
                case 736:
                case 836:
                case 936:
                case 1036:
                case 1136:
                case 1236:
                    originalScript = "teleport_toee1(npc,pc)";
                    teleport_toee1(npc, pc);
                    break;
                case 737:
                case 837:
                case 937:
                case 1037:
                case 1137:
                case 1237:
                    originalScript = "teleport_toee2(npc,pc)";
                    teleport_toee2(npc, pc);
                    break;
                case 738:
                case 838:
                case 938:
                case 1038:
                case 1138:
                case 1238:
                    originalScript = "teleport_toee3(npc,pc)";
                    teleport_toee3(npc, pc);
                    break;
                case 739:
                case 839:
                case 939:
                case 1039:
                case 1139:
                case 1239:
                    originalScript = "teleport_toee4(npc,pc)";
                    teleport_toee4(npc, pc);
                    break;
                case 810:
                    originalScript = "game.particles( 'ef-NODE-Air portal_2', npc ); game.sound( 4008 )";
                    AttachParticles("ef-NODE-Air portal_2", npc);
                    Sound(4008);
                    ;
                    break;
                case 910:
                    originalScript = "game.particles( 'ef-NODE-Air portal_3', npc ); game.sound( 4008 )";
                    AttachParticles("ef-NODE-Air portal_3", npc);
                    Sound(4008);
                    ;
                    break;
                case 1010:
                    originalScript = "game.particles( 'ef-NODE-Air portal_4', npc ); game.sound( 4008 )";
                    AttachParticles("ef-NODE-Air portal_4", npc);
                    Sound(4008);
                    ;
                    break;
                case 1110:
                    originalScript = "game.particles( 'ef-NODE-Air portal_5', npc ); game.sound( 4008 )";
                    AttachParticles("ef-NODE-Air portal_5", npc);
                    Sound(4008);
                    ;
                    break;
                case 1210:
                    originalScript = "game.particles( 'ef-NODE-Air portal_6', npc ); game.sound( 4008 )";
                    AttachParticles("ef-NODE-Air portal_6", npc);
                    Sound(4008);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
