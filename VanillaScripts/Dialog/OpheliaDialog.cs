
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

namespace VanillaScripts.Dialog;

[DialogScript(98)]
public class OpheliaDialog : Ophelia, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 3:
            case 9:
            case 73:
            case 74:
            case 431:
            case 439:
            case 447:
            case 448:
            case 482:
            case 483:
            case 584:
            case 591:
                originalScript = "pc.stat_level_get( stat_gender ) == gender_male and game.party_alignment != LAWFUL_GOOD";
                return pc.GetGender() == Gender.Male && PartyAlignment != Alignment.LAWFUL_GOOD;
            case 4:
            case 10:
            case 75:
            case 76:
            case 432:
                originalScript = "pc.stat_level_get( stat_gender ) == gender_female and game.party_alignment != LAWFUL_GOOD";
                return pc.GetGender() == Gender.Female && PartyAlignment != Alignment.LAWFUL_GOOD;
            case 6:
            case 12:
            case 433:
            case 434:
            case 705:
            case 711:
                originalScript = "game.party_alignment == LAWFUL_GOOD";
                return PartyAlignment == Alignment.LAWFUL_GOOD;
            case 33:
            case 36:
                originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 6";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 6;
            case 51:
            case 52:
                originalScript = "game.party_alignment != LAWFUL_GOOD";
                return PartyAlignment != Alignment.LAWFUL_GOOD;
            case 93:
            case 94:
                originalScript = "pc.money_get() >= 20000";
                return pc.GetMoney() >= 20000;
            case 95:
            case 96:
                originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 6 and pc.money_get() >= 10000";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 6 && pc.GetMoney() >= 10000;
            case 125:
            case 130:
                originalScript = "game.global_flags[77] == 0";
                return !GetGlobalFlag(77);
            case 131:
            case 133:
                originalScript = "game.global_flags[77] == 1 and game.global_flags[76] == 0";
                return GetGlobalFlag(77) && !GetGlobalFlag(76);
            case 132:
            case 134:
                originalScript = "game.global_flags[77] == 1 and game.global_flags[76] == 1";
                return GetGlobalFlag(77) && GetGlobalFlag(76);
            case 143:
            case 144:
            case 231:
            case 232:
            case 341:
            case 342:
            case 362:
            case 363:
            case 391:
            case 392:
            case 401:
            case 411:
            case 601:
            case 602:
                originalScript = "pc.money_get() >= 5000";
                return pc.GetMoney() >= 5000;
            case 204:
            case 208:
                originalScript = "game.quests[34].state <= qs_mentioned";
                return GetQuestState(34) <= QuestState.Mentioned;
            case 261:
            case 262:
            case 501:
            case 502:
                originalScript = "game.global_flags[94] == 1";
                return GetGlobalFlag(94);
            case 321:
            case 322:
                originalScript = "pc.money_get() >= 10000";
                return pc.GetMoney() >= 10000;
            case 333:
            case 353:
                originalScript = "game.global_flags[87] == 0";
                return !GetGlobalFlag(87);
            case 361:
                originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 4 and pc.money_get() >= 3000 and game.global_flags[87] == 0";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 4 && pc.GetMoney() >= 3000 && !GetGlobalFlag(87);
            case 441:
            case 442:
                originalScript = "game.quests[33].state <= qs_accepted or game.quests[34].state == qs_unknown";
                return GetQuestState(33) <= QuestState.Accepted || GetQuestState(34) == QuestState.Unknown;
            case 445:
            case 446:
                originalScript = "game.quests[33].state >= qs_completed and game.quests[34].state >= qs_mentioned";
                return GetQuestState(33) >= QuestState.Completed && GetQuestState(34) >= QuestState.Mentioned;
            case 451:
            case 452:
            case 701:
            case 704:
            case 707:
            case 710:
            case 795:
            case 796:
                originalScript = "game.quests[33].state == qs_unknown";
                return GetQuestState(33) == QuestState.Unknown;
            case 453:
            case 454:
                originalScript = "game.quests[33].state == qs_mentioned";
                return GetQuestState(33) == QuestState.Mentioned;
            case 455:
            case 456:
                originalScript = "game.quests[33].state == qs_accepted";
                return GetQuestState(33) == QuestState.Accepted;
            case 457:
            case 459:
                originalScript = "game.quests[33].state == qs_completed and game.quests[34].state == qs_unknown and game.global_flags[76] == 1";
                return GetQuestState(33) == QuestState.Completed && GetQuestState(34) == QuestState.Unknown && GetGlobalFlag(76);
            case 458:
            case 460:
                originalScript = "game.quests[33].state == qs_completed and game.quests[34].state == qs_unknown and game.global_flags[77] == 1";
                return GetQuestState(33) == QuestState.Completed && GetQuestState(34) == QuestState.Unknown && GetGlobalFlag(77);
            case 474:
            case 478:
                originalScript = "game.quests[34].state == qs_unknown";
                return GetQuestState(34) == QuestState.Unknown;
            case 479:
                originalScript = "game.quests[34].state == qs_mentioned";
                return GetQuestState(34) == QuestState.Mentioned;
            case 492:
            case 494:
                originalScript = "pc.stat_level_get( stat_gender ) == gender_male";
                return pc.GetGender() == Gender.Male;
            case 581:
            case 588:
                originalScript = "game.global_flags[84] == 1 and game.quests[33].state != qs_completed";
                return GetGlobalFlag(84) && GetQuestState(33) != QuestState.Completed;
            case 582:
            case 589:
                originalScript = "game.global_flags[85] == 1 and game.quests[33].state != qs_completed";
                return GetGlobalFlag(85) && GetQuestState(33) != QuestState.Completed;
            case 583:
            case 590:
                originalScript = "game.global_flags[86] == 1 and ( game.quests[34].state == qs_mentioned or game.quests[34].state == qs_accepted )";
                return GetGlobalFlag(86) && (GetQuestState(34) == QuestState.Mentioned || GetQuestState(34) == QuestState.Accepted);
            case 586:
            case 593:
                originalScript = "game.quests[33].state = qs_completed and game.global_flags[76] == 1 and pc.stat_level_get( stat_gender ) == gender_female and game.global_flags[83] == 0";
                return GetQuestState(33) == QuestState.Completed && GetGlobalFlag(76) && pc.GetGender() == Gender.Female && !GetGlobalFlag(83);
            case 611:
            case 612:
                originalScript = "pc.money_get() >= 2000";
                return pc.GetMoney() >= 2000;
            case 613:
            case 614:
            case 661:
            case 662:
                originalScript = "pc.money_get() >= 50000";
                return pc.GetMoney() >= 50000;
            case 663:
            case 664:
                originalScript = "anyone( pc.group_list(), \"has_follower\", 8015 ) and (game.party_alignment == LAWFUL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == NEUTRAL_EVIL)";
                return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8015)) && (PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL);
            case 693:
            case 694:
                originalScript = "pc.stat_level_get( stat_gender ) == gender_female";
                return pc.GetGender() == Gender.Female;
            case 702:
            case 708:
                originalScript = "pc.stat_level_get( stat_gender ) == gender_male and game.party_alignment != LAWFUL_GOOD and game.quests[33].state == qs_unknown";
                return pc.GetGender() == Gender.Male && PartyAlignment != Alignment.LAWFUL_GOOD && GetQuestState(33) == QuestState.Unknown;
            case 703:
            case 709:
            case 792:
            case 798:
                originalScript = "pc.stat_level_get( stat_gender ) == gender_female and game.party_alignment != LAWFUL_GOOD and game.quests[33].state == qs_unknown";
                return pc.GetGender() == Gender.Female && PartyAlignment != Alignment.LAWFUL_GOOD && GetQuestState(33) == QuestState.Unknown;
            case 713:
            case 714:
            case 799:
            case 800:
                originalScript = "game.quests[33].state >= qs_mentioned";
                return GetQuestState(33) >= QuestState.Mentioned;
            case 742:
            case 743:
            case 752:
            case 753:
                originalScript = "game.quests[33].state <= qs_mentioned";
                return GetQuestState(33) <= QuestState.Mentioned;
            case 744:
            case 745:
            case 754:
            case 755:
                originalScript = "game.quests[33].state >= qs_accepted";
                return GetQuestState(33) >= QuestState.Accepted;
            case 791:
            case 797:
                originalScript = "pc.stat_level_get( stat_gender ) == gender_male and game.party_alignment != LAWFUL_GOOD nd game.quests[33].state == qs_unknown";
                throw new NotSupportedException("Conversion failed.");
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 1:
                originalScript = "game.global_flags[320] = 1";
                SetGlobalFlag(320, true);
                break;
            case 33:
            case 36:
            case 110:
            case 150:
            case 430:
            case 790:
                originalScript = "game.global_flags[75] = 1";
                SetGlobalFlag(75, true);
                break;
            case 93:
            case 94:
                originalScript = "pc.money_adj(-20000)";
                pc.AdjustMoney(-20000);
                break;
            case 95:
            case 96:
            case 321:
            case 322:
                originalScript = "pc.money_adj(-10000)";
                pc.AdjustMoney(-10000);
                break;
            case 140:
            case 220:
            case 490:
                originalScript = "game.quests[33].state = qs_mentioned";
                SetQuestState(33, QuestState.Mentioned);
                break;
            case 143:
            case 144:
            case 231:
            case 232:
            case 341:
            case 342:
            case 411:
            case 601:
            case 602:
                originalScript = "pc.money_adj(-5000)";
                pc.AdjustMoney(-5000);
                break;
            case 161:
                originalScript = "game.fade_and_teleport( 14400, 0, 0, 5051, 557, 531 )";
                FadeAndTeleport(14400, 0, 0, 5051, 557, 531);
                break;
            case 171:
            case 172:
                originalScript = "game.fade_and_teleport( 28800, 0, 0, 5051, 557, 531 )";
                FadeAndTeleport(28800, 0, 0, 5051, 557, 531);
                break;
            case 240:
                originalScript = "game.quests[33].state = qs_accepted; game.global_flags[76] = 1";
                SetQuestState(33, QuestState.Accepted);
                SetGlobalFlag(76, true);
                ;
                break;
            case 250:
                originalScript = "game.quests[33].state = qs_accepted; game.global_flags[77] = 1";
                SetQuestState(33, QuestState.Accepted);
                SetGlobalFlag(77, true);
                ;
                break;
            case 300:
            case 530:
                originalScript = "game.quests[34].state = qs_mentioned";
                SetQuestState(34, QuestState.Mentioned);
                break;
            case 310:
            case 540:
                originalScript = "game.quests[34].state = qs_accepted";
                SetQuestState(34, QuestState.Accepted);
                break;
            case 330:
                originalScript = "game.global_flags[78] = 1";
                SetGlobalFlag(78, true);
                break;
            case 333:
            case 353:
            case 361:
                originalScript = "game.global_flags[87] = 1";
                SetGlobalFlag(87, true);
                break;
            case 350:
                originalScript = "game.global_flags[80] = 1";
                SetGlobalFlag(80, true);
                break;
            case 362:
            case 363:
                originalScript = "pc.money_adj(-5000); game.global_flags[79] = 1";
                pc.AdjustMoney(-5000);
                SetGlobalFlag(79, true);
                ;
                break;
            case 370:
                originalScript = "game.global_flags[79] = 1";
                SetGlobalFlag(79, true);
                break;
            case 371:
                originalScript = "pc.money_adj(-3000)";
                pc.AdjustMoney(-3000);
                break;
            case 391:
            case 392:
                originalScript = "pc.money_adj(-5000); game.global_flags[77] = 1";
                pc.AdjustMoney(-5000);
                SetGlobalFlag(77, true);
                ;
                break;
            case 401:
                originalScript = "pc.money_adj(-5000); game.global_flags[76] = 1";
                pc.AdjustMoney(-5000);
                SetGlobalFlag(76, true);
                ;
                break;
            case 611:
            case 612:
                originalScript = "game.global_flags[82] = 1";
                SetGlobalFlag(82, true);
                break;
            case 655:
                originalScript = "game.global_flags[76] = 1";
                SetGlobalFlag(76, true);
                break;
            case 656:
            case 657:
                originalScript = "pc.money_adj(-50000); game.global_flags[83] = 1";
                pc.AdjustMoney(-50000);
                SetGlobalFlag(83, true);
                ;
                break;
            case 661:
            case 662:
                originalScript = "pc.money_adj(-50000)";
                pc.AdjustMoney(-50000);
                break;
            case 663:
            case 664:
                originalScript = "buttin(npc,pc,200)";
                buttin(npc, pc, 200);
                break;
            case 670:
            case 675:
            case 720:
                originalScript = "game.quests[33].state = qs_completed";
                SetQuestState(33, QuestState.Completed);
                break;
            case 740:
                originalScript = "game.quests[34].state = qs_completed; npc.reaction_adj( pc,-10)";
                SetQuestState(34, QuestState.Completed);
                npc.AdjustReaction(pc, -10);
                ;
                break;
            case 750:
                originalScript = "game.quests[34].state = qs_completed";
                SetQuestState(34, QuestState.Completed);
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
            case 33:
            case 36:
            case 95:
            case 96:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 6);
                return true;
            case 361:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 4);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}