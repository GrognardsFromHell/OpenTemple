
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

namespace Scripts.Dialog;

[DialogScript(363)]
public class LadyAsherahDialog : LadyAsherah, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 11:
            case 81:
            case 281:
            case 291:
                originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 20";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 20;
            case 12:
            case 82:
            case 282:
            case 292:
                originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 20";
                return pc.GetSkillLevel(npc, SkillId.intimidate) >= 20;
            case 13:
            case 83:
            case 283:
            case 293:
                originalScript = "pc.skill_level_get(npc,skill_bluff) >= 20";
                return pc.GetSkillLevel(npc, SkillId.bluff) >= 20;
            case 14:
            case 84:
            case 284:
            case 294:
                originalScript = "pc.skill_level_get(npc,skill_diplomacy) <= 19";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) <= 19;
            case 15:
            case 85:
            case 285:
            case 295:
                originalScript = "pc.skill_level_get(npc,skill_intimidate) <= 19";
                return pc.GetSkillLevel(npc, SkillId.intimidate) <= 19;
            case 16:
            case 86:
            case 286:
            case 296:
                originalScript = "pc.skill_level_get(npc,skill_bluff) <= 19";
                return pc.GetSkillLevel(npc, SkillId.bluff) <= 19;
            case 23:
            case 33:
            case 43:
            case 53:
            case 63:
            case 73:
            case 303:
            case 313:
            case 323:
            case 333:
            case 343:
            case 353:
                originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 20";
                return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 20;
            case 91:
            case 361:
            case 571:
            case 681:
            case 791:
            case 861:
            case 881:
            case 901:
                originalScript = "game.global_vars[699] == 0";
                return GetGlobalVar(699) == 0;
            case 92:
            case 362:
            case 572:
            case 682:
            case 792:
            case 862:
            case 882:
            case 902:
                originalScript = "game.global_vars[699] == 3";
                return GetGlobalVar(699) == 3;
            case 93:
            case 363:
            case 573:
            case 683:
            case 793:
            case 863:
            case 883:
            case 903:
                originalScript = "game.global_vars[699] == 4";
                return GetGlobalVar(699) == 4;
            case 101:
            case 102:
            case 103:
            case 121:
            case 122:
            case 371:
            case 372:
            case 373:
            case 391:
            case 392:
                originalScript = "pc.stat_level_get( stat_gender ) == gender_male";
                return pc.GetGender() == Gender.Male;
            case 104:
            case 105:
            case 106:
            case 123:
            case 124:
            case 374:
            case 375:
            case 376:
            case 393:
            case 394:
                originalScript = "pc.stat_level_get( stat_gender ) == gender_female";
                return pc.GetGender() == Gender.Female;
            case 111:
            case 381:
                originalScript = "game.global_vars[699] == 1";
                return GetGlobalVar(699) == 1;
            case 112:
            case 382:
                originalScript = "game.global_vars[699] == 2";
                return GetGlobalVar(699) == 2;
            case 141:
            case 521:
            case 921:
            case 1081:
            case 1121:
            case 1192:
            case 1271:
                originalScript = "game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD";
                return PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD;
            case 142:
            case 522:
            case 922:
            case 1082:
            case 1122:
            case 1194:
            case 1272:
                originalScript = "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL or game.party_alignment == CHAOTIC_NEUTRAL";
                return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL;
            case 143:
            case 523:
            case 923:
            case 1083:
            case 1123:
            case 1193:
            case 1273:
                originalScript = "game.party_alignment == LAWFUL_EVIL or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == CHAOTIC_GOOD";
                return PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.CHAOTIC_GOOD;
            case 172:
            case 232:
            case 462:
                originalScript = "game.global_vars[699] == 3 or game.global_vars[699] == 4";
                return GetGlobalVar(699) == 3 || GetGlobalVar(699) == 4;
            case 561:
            case 563:
            case 671:
            case 673:
            case 781:
            case 783:
                originalScript = "game.global_vars[506] == 4";
                return GetGlobalVar(506) == 4;
            case 562:
            case 564:
            case 672:
            case 674:
            case 782:
            case 784:
                originalScript = "game.global_vars[506] == 3";
                return GetGlobalVar(506) == 3;
            case 591:
            case 593:
            case 701:
            case 703:
            case 811:
            case 813:
                originalScript = "pc.stat_level_get( stat_gender ) == gender_male and game.global_vars[506] == 4";
                return pc.GetGender() == Gender.Male && GetGlobalVar(506) == 4;
            case 592:
            case 594:
            case 702:
            case 704:
            case 812:
            case 814:
                originalScript = "pc.stat_level_get( stat_gender ) == gender_male and game.global_vars[506] == 3";
                return pc.GetGender() == Gender.Male && GetGlobalVar(506) == 3;
            case 595:
            case 597:
            case 705:
            case 707:
            case 815:
            case 817:
                originalScript = "pc.stat_level_get( stat_gender ) == gender_female and game.global_vars[506] == 4";
                return pc.GetGender() == Gender.Female && GetGlobalVar(506) == 4;
            case 596:
            case 598:
            case 706:
            case 708:
            case 816:
            case 818:
                originalScript = "pc.stat_level_get( stat_gender ) == gender_female and game.global_vars[506] == 3";
                return pc.GetGender() == Gender.Female && GetGlobalVar(506) == 3;
            case 1162:
            case 1172:
            case 1182:
                originalScript = "game.global_flags[864] == 1";
                return GetGlobalFlag(864);
            case 1163:
            case 1173:
            case 1183:
                originalScript = "game.global_flags[807] == 1";
                return GetGlobalFlag(807);
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
                originalScript = "game.global_vars[506] = 4";
                SetGlobalVar(506, 4);
                break;
            case 91:
            case 571:
            case 681:
            case 791:
            case 881:
                originalScript = "game.fade(28800,0,0,14); schedule_transfer(npc,pc)";
                Fade(28800, 0, 0, 14);
                schedule_transfer(npc, pc);
                ;
                break;
            case 92:
            case 93:
            case 572:
            case 573:
            case 682:
            case 683:
            case 792:
            case 793:
            case 882:
            case 883:
                originalScript = "game.fade(14400,0,0,8); schedule_transfer(npc,pc)";
                Fade(14400, 0, 0, 8);
                schedule_transfer(npc, pc);
                ;
                break;
            case 101:
            case 122:
            case 371:
            case 392:
            case 593:
            case 594:
            case 703:
            case 704:
            case 813:
            case 814:
                originalScript = "game.global_vars[699] = 1";
                SetGlobalVar(699, 1);
                break;
            case 104:
            case 124:
            case 374:
            case 394:
            case 597:
            case 598:
            case 707:
            case 708:
            case 817:
            case 818:
                originalScript = "game.global_vars[699] = 2";
                SetGlobalVar(699, 2);
                break;
            case 111:
                originalScript = "game.party[0].reputation_add(54); run_off_castle(npc,pc)";
                PartyLeader.AddReputation(54);
                run_off_castle(npc, pc);
                ;
                break;
            case 112:
                originalScript = "game.party[0].reputation_add(55); run_off_castle(npc,pc)";
                PartyLeader.AddReputation(55);
                run_off_castle(npc, pc);
                ;
                break;
            case 121:
            case 391:
            case 591:
            case 592:
            case 701:
            case 702:
            case 811:
            case 812:
                originalScript = "game.global_vars[699] = 3";
                SetGlobalVar(699, 3);
                break;
            case 123:
            case 393:
            case 595:
            case 596:
            case 705:
            case 706:
            case 815:
            case 816:
                originalScript = "game.global_vars[699] = 4";
                SetGlobalVar(699, 4);
                break;
            case 130:
            case 600:
            case 710:
            case 820:
                originalScript = "game.quests[101].state = qs_completed";
                SetQuestState(101, QuestState.Completed);
                break;
            case 160:
            case 220:
            case 450:
            case 540:
            case 650:
            case 760:
                originalScript = "game.quests[102].state = qs_mentioned";
                SetQuestState(102, QuestState.Mentioned);
                break;
            case 191:
            case 251:
            case 481:
            case 621:
            case 731:
            case 841:
                originalScript = "game.quests[102].state = qs_accepted";
                SetQuestState(102, QuestState.Accepted);
                break;
            case 200:
            case 630:
                originalScript = "game.global_vars[697] = 1";
                SetGlobalVar(697, 1);
                break;
            case 201:
            case 631:
                originalScript = "game.fade_and_teleport( 0,0,0,5121,509,652 )";
                FadeAndTeleport(0, 0, 0, 5121, 509, 652);
                break;
            case 260:
            case 740:
                originalScript = "game.global_vars[698] = 1";
                SetGlobalVar(698, 1);
                break;
            case 261:
            case 741:
                originalScript = "game.fade_and_teleport( 0,0,0,5121,539,622 )";
                FadeAndTeleport(0, 0, 0, 5121, 539, 622);
                break;
            case 270:
                originalScript = "game.global_vars[506] = 3";
                SetGlobalVar(506, 3);
                break;
            case 361:
            case 861:
            case 901:
                originalScript = "game.fade(28800,0,0,14); schedule_transfer_2(npc,pc)";
                Fade(28800, 0, 0, 14);
                schedule_transfer_2(npc, pc);
                ;
                break;
            case 362:
            case 363:
            case 862:
            case 863:
            case 902:
            case 903:
                originalScript = "game.fade(14400,0,0,8); schedule_transfer_2(npc,pc)";
                Fade(14400, 0, 0, 8);
                schedule_transfer_2(npc, pc);
                ;
                break;
            case 381:
                originalScript = "game.party[0].reputation_add(54); run_off_home(npc,pc)";
                PartyLeader.AddReputation(54);
                run_off_home(npc, pc);
                ;
                break;
            case 382:
                originalScript = "game.party[0].reputation_add(55); run_off_home(npc,pc)";
                PartyLeader.AddReputation(55);
                run_off_home(npc, pc);
                ;
                break;
            case 490:
            case 850:
                originalScript = "game.global_vars[734] = 1";
                SetGlobalVar(734, 1);
                break;
            case 491:
            case 851:
                originalScript = "game.fade_and_teleport( 0,0,0,5121,509,622 )";
                FadeAndTeleport(0, 0, 0, 5121, 509, 622);
                break;
            case 520:
                originalScript = "game.global_flags[865] = 1";
                SetGlobalFlag(865, true);
                break;
            case 581:
            case 691:
            case 801:
                originalScript = "run_off_castle(npc,pc)";
                run_off_castle(npc, pc);
                break;
            case 871:
            case 891:
            case 911:
                originalScript = "run_off_home(npc,pc); game.quests[101].state = qs_botched";
                run_off_home(npc, pc);
                SetQuestState(101, QuestState.Botched);
                ;
                break;
            case 921:
                originalScript = "game.global_vars[898] = 1";
                SetGlobalVar(898, 1);
                break;
            case 922:
                originalScript = "game.global_vars[898] = 5";
                SetGlobalVar(898, 5);
                break;
            case 923:
                originalScript = "game.global_vars[898] = 9";
                SetGlobalVar(898, 9);
                break;
            case 931:
            case 941:
            case 951:
                originalScript = "game.global_flags[864] = 1; game.global_flags[807] = 1";
                SetGlobalFlag(864, true);
                SetGlobalFlag(807, true);
                ;
                break;
            case 932:
            case 942:
            case 952:
                originalScript = "game.global_flags[807] = 1";
                SetGlobalFlag(807, true);
                break;
            case 933:
            case 943:
            case 953:
                originalScript = "game.global_flags[864] = 1";
                SetGlobalFlag(864, true);
                break;
            case 934:
            case 944:
            case 954:
                originalScript = "game.global_flags[699] = 1";
                SetGlobalFlag(699, true);
                break;
            case 1081:
                originalScript = "game.global_vars[898] = 2";
                SetGlobalVar(898, 2);
                break;
            case 1082:
                originalScript = "game.global_vars[898] = 6";
                SetGlobalVar(898, 6);
                break;
            case 1083:
                originalScript = "game.global_vars[898] = 10";
                SetGlobalVar(898, 10);
                break;
            case 1091:
            case 1101:
            case 1111:
                originalScript = "ruin_asherah(npc,pc)";
                ruin_asherah(npc, pc);
                break;
            case 1121:
                originalScript = "game.global_vars[898] = 3";
                SetGlobalVar(898, 3);
                break;
            case 1122:
                originalScript = "game.global_vars[898] = 7";
                SetGlobalVar(898, 7);
                break;
            case 1123:
                originalScript = "game.global_vars[898] = 11";
                SetGlobalVar(898, 11);
                break;
            case 1131:
            case 1141:
            case 1151:
            case 1291:
                originalScript = "kill_asherah(npc,pc)";
                kill_asherah(npc, pc);
                break;
            case 1162:
            case 1172:
            case 1182:
                originalScript = "game.fade(14400,4047,0,4)";
                Fade(14400, 4047, 0, 4);
                break;
            case 1220:
                originalScript = "create_item_in_inventory(8005,pc)";
                Utilities.create_item_in_inventory(8005, pc);
                break;
            case 1271:
                originalScript = "game.global_vars[898] = 4";
                SetGlobalVar(898, 4);
                break;
            case 1272:
                originalScript = "game.global_vars[898] = 8";
                SetGlobalVar(898, 8);
                break;
            case 1273:
                originalScript = "game.global_vars[898] = 12";
                SetGlobalVar(898, 12);
                break;
            case 1280:
                originalScript = "game.party[0].reputation_add( 64 )";
                PartyLeader.AddReputation(64);
                break;
            case 1281:
                originalScript = "kill_asherah(npc,pc); game.sound( 4144 )";
                kill_asherah(npc, pc);
                Sound(4144);
                ;
                break;
            case 1290:
                originalScript = "game.party[0].reputation_add( 65 )";
                PartyLeader.AddReputation(65);
                break;
            case 1300:
                originalScript = "game.party[0].reputation_add( 63 )";
                PartyLeader.AddReputation(63);
                break;
            case 1301:
                originalScript = "kill_asherah(npc,pc); game.sound( 4143 )";
                kill_asherah(npc, pc);
                Sound(4143);
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
            case 11:
            case 81:
            case 281:
            case 291:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 20);
                return true;
            case 12:
            case 82:
            case 282:
            case 292:
                skillChecks = new DialogSkillChecks(SkillId.intimidate, 20);
                return true;
            case 13:
            case 83:
            case 283:
            case 293:
                skillChecks = new DialogSkillChecks(SkillId.bluff, 20);
                return true;
            case 23:
            case 33:
            case 43:
            case 53:
            case 63:
            case 73:
            case 303:
            case 313:
            case 323:
            case 333:
            case 343:
            case 353:
                skillChecks = new DialogSkillChecks(SkillId.sense_motive, 20);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}