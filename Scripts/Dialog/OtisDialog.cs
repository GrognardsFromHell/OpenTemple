
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

[DialogScript(97)]
public class OtisDialog : Otis, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 3:
                originalScript = "not npc.has_met( pc )";
                return !npc.HasMet(pc);
            case 4:
                originalScript = "npc.has_met( pc ) and is_daytime()";
                return npc.HasMet(pc) && Utilities.is_daytime();
            case 5:
                originalScript = "npc.has_met( pc ) and not is_daytime()";
                return npc.HasMet(pc) && !Utilities.is_daytime();
            case 6:
            case 7:
                originalScript = "npc.has_met( pc )";
                return npc.HasMet(pc);
            case 15:
                originalScript = "(pc.item_find(6318) != OBJ_HANDLE_NULL and pc.stat_level_get( stat_gender ) == gender_female) or (pc.item_find(6320) != OBJ_HANDLE_NULL and pc.stat_level_get( stat_gender ) == gender_female)";
                return (pc.FindItemByName(6318) != null && pc.GetGender() == Gender.Female) || (pc.FindItemByName(6320) != null && pc.GetGender() == Gender.Female);
            case 21:
            case 103:
            case 104:
                originalScript = "game.story_state == 3";
                return StoryState == 3;
            case 22:
            case 23:
            case 24:
            case 25:
                originalScript = "game.story_state >= 4";
                return StoryState >= 4;
            case 31:
            case 33:
                originalScript = "npc.leader_get() == OBJ_HANDLE_NULL";
                return npc.GetLeader() == null;
            case 35:
                originalScript = "npc.leader_get() != OBJ_HANDLE_NULL";
                return npc.GetLeader() != null;
            case 41:
            case 42:
            case 109:
            case 110:
                originalScript = "(game.global_flags[73] == 0) and (npc.leader_get() == OBJ_HANDLE_NULL) and (game.global_flags[360] == 0)";
                return (!GetGlobalFlag(73)) && (npc.GetLeader() == null) && (!GetGlobalFlag(360));
            case 43:
            case 44:
            case 111:
            case 112:
                originalScript = "(game.global_flags[73] == 0) and (npc.leader_get() == OBJ_HANDLE_NULL) and (game.global_flags[360] == 1) and (pc.follower_atmax())";
                return (!GetGlobalFlag(73)) && (npc.GetLeader() == null) && (GetGlobalFlag(360)) && (pc.HasMaxFollowers());
            case 45:
            case 46:
            case 113:
            case 114:
                originalScript = "(game.global_flags[73] == 0) and (npc.leader_get() == OBJ_HANDLE_NULL) and (game.global_flags[360] == 1) and (not pc.follower_atmax())";
                return (!GetGlobalFlag(73)) && (npc.GetLeader() == null) && (GetGlobalFlag(360)) && (!pc.HasMaxFollowers());
            case 51:
            case 52:
                originalScript = "pc.money_get() >= 17500 and not pc.follower_atmax()";
                return pc.GetMoney() >= 17500 && !pc.HasMaxFollowers();
            case 53:
            case 54:
                originalScript = "pc.money_get() < 17500";
                return pc.GetMoney() < 17500;
            case 55:
            case 56:
            case 455:
            case 456:
            case 465:
            case 466:
            case 824:
            case 828:
                originalScript = "pc.follower_atmax()";
                return pc.HasMaxFollowers();
            case 101:
            case 102:
                originalScript = "((game.party_alignment == LAWFUL_EVIL) or (game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL)) and (game.global_flags[74] == 1) and (npc.leader_get() == OBJ_HANDLE_NULL)";
                return ((PartyAlignment == Alignment.LAWFUL_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL)) && (GetGlobalFlag(74)) && (npc.GetLeader() == null);
            case 105:
            case 106:
                originalScript = "game.story_state != 3";
                return StoryState != 3;
            case 121:
            case 124:
                originalScript = "((game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == NEUTRAL_GOOD) or (game.party_alignment == CHAOTIC_GOOD)) and (game.story_state >= 5)";
                return ((PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.NEUTRAL_GOOD) || (PartyAlignment == Alignment.CHAOTIC_GOOD)) && (StoryState >= 5);
            case 122:
            case 125:
                originalScript = "((game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == NEUTRAL_GOOD) or (game.party_alignment == CHAOTIC_GOOD)) and (game.story_state == 4)";
                return ((PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.NEUTRAL_GOOD) || (PartyAlignment == Alignment.CHAOTIC_GOOD)) && (StoryState == 4);
            case 123:
            case 126:
                originalScript = "(game.party_alignment == LAWFUL_EVIL) or (game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL)";
                return (PartyAlignment == Alignment.LAWFUL_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL);
            case 127:
            case 128:
                originalScript = "(game.party_alignment == LAWFUL_NEUTRAL) or (game.party_alignment == TRUE_NEUTRAL) or (game.party_alignment == CHAOTIC_NEUTRAL)";
                return (PartyAlignment == Alignment.LAWFUL_NEUTRAL) || (PartyAlignment == Alignment.NEUTRAL) || (PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
            case 130:
            case 206:
                originalScript = "is_daytime()";
                return Utilities.is_daytime();
            case 131:
            case 207:
                originalScript = "not is_daytime()";
                return !Utilities.is_daytime();
            case 165:
            case 166:
                originalScript = "((game.party_alignment == LAWFUL_EVIL) or (game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL)) and (game.global_flags[74] == 1)";
                return ((PartyAlignment == Alignment.LAWFUL_EVIL) || (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL)) && (GetGlobalFlag(74));
            case 201:
            case 202:
                originalScript = "game.story_state == 5";
                return StoryState == 5;
            case 203:
            case 204:
                originalScript = "game.story_state == 6";
                return StoryState == 6;
            case 211:
            case 213:
                originalScript = "game.global_flags[74] == 0";
                return !GetGlobalFlag(74);
            case 212:
            case 214:
                originalScript = "game.global_flags[74] == 1";
                return GetGlobalFlag(74);
            case 251:
            case 252:
                originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 10 and not pc.follower_atmax()";
                return pc.GetSkillLevel(npc, SkillId.intimidate) >= 10 && !pc.HasMaxFollowers();
            case 262:
            case 263:
                originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 6";
                return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 6;
            case 271:
            case 272:
                originalScript = "pc.skill_level_get(npc, skill_bluff) >= 10";
                return pc.GetSkillLevel(npc, SkillId.bluff) >= 10;
            case 291:
            case 293:
                originalScript = "(game.party_alignment == LAWFUL_GOOD) or (game.party_alignment == LAWFUL_NEUTRAL) or (game.party_alignment == LAWFUL_EVIL)";
                return (PartyAlignment == Alignment.LAWFUL_GOOD) || (PartyAlignment == Alignment.LAWFUL_NEUTRAL) || (PartyAlignment == Alignment.LAWFUL_EVIL);
            case 292:
            case 294:
                originalScript = "(game.party_alignment != LAWFUL_GOOD) and (game.party_alignment != LAWFUL_NEUTRAL) and (game.party_alignment != LAWFUL_EVIL)";
                return (PartyAlignment != Alignment.LAWFUL_GOOD) && (PartyAlignment != Alignment.LAWFUL_NEUTRAL) && (PartyAlignment != Alignment.LAWFUL_EVIL);
            case 351:
            case 352:
                originalScript = "npc.area == 3";
                return npc.GetArea() == 3;
            case 353:
            case 354:
                originalScript = "npc.area != 3";
                return npc.GetArea() != 3;
            case 453:
            case 454:
            case 463:
            case 464:
                originalScript = "not pc.follower_atmax()";
                return !pc.HasMaxFollowers();
            case 531:
            case 532:
                originalScript = "game.global_flags[94] == 1";
                return GetGlobalFlag(94);
            case 662:
            case 672:
                originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 10";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 10;
            case 663:
            case 673:
                originalScript = "pc.skill_level_get(npc,skill_intimidate) >=10";
                return pc.GetSkillLevel(npc, SkillId.intimidate) >= 10;
            case 681:
            case 691:
                originalScript = "not anyone( pc.group_list(), \"has_follower\", 8000)";
                return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000));
            case 682:
            case 692:
                originalScript = "anyone( pc.group_list(), \"has_follower\", 8000)";
                return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8000));
            case 711:
            case 712:
                originalScript = "game.quests[32].state != qs_botched";
                return GetQuestState(32) != QuestState.Botched;
            case 713:
            case 714:
                originalScript = "(game.quests[32].state == qs_botched) and (pc.skill_level_get(npc, skill_bluff) <= 9)";
                return (GetQuestState(32) == QuestState.Botched) && (pc.GetSkillLevel(npc, SkillId.bluff) <= 9);
            case 715:
            case 716:
                originalScript = "(game.quests[32].state == qs_botched) and (pc.skill_level_get(npc, skill_bluff) >= 10)";
                return (GetQuestState(32) == QuestState.Botched) && (pc.GetSkillLevel(npc, SkillId.bluff) >= 10);
            case 721:
            case 722:
            case 731:
            case 732:
            case 741:
            case 742:
            case 751:
            case 752:
                originalScript = "(game.party_alignment != NEUTRAL_EVIL) and (game.party_alignment != CHAOTIC_EVIL) and (game.party_alignment != CHAOTIC_NEUTRAL)";
                return (PartyAlignment != Alignment.NEUTRAL_EVIL) && (PartyAlignment != Alignment.CHAOTIC_EVIL) && (PartyAlignment != Alignment.CHAOTIC_NEUTRAL);
            case 723:
            case 724:
            case 733:
            case 734:
            case 743:
            case 744:
            case 753:
            case 754:
                originalScript = "(game.party_alignment == NEUTRAL_EVIL) or (game.party_alignment == CHAOTIC_EVIL) or (game.party_alignment == CHAOTIC_NEUTRAL)";
                return (PartyAlignment == Alignment.NEUTRAL_EVIL) || (PartyAlignment == Alignment.CHAOTIC_EVIL) || (PartyAlignment == Alignment.CHAOTIC_NEUTRAL);
            case 821:
            case 825:
                originalScript = "(group_average_level(pc) > 5) and (not pc.follower_atmax())";
                return (Utilities.group_average_level(pc) > 5) && (!pc.HasMaxFollowers());
            case 822:
            case 826:
                originalScript = "(group_average_level(pc) <= 5) and (not pc.follower_atmax())";
                return (Utilities.group_average_level(pc) <= 5) && (!pc.HasMaxFollowers());
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
                originalScript = "game.global_vars[125] = 1";
                SetGlobalVar(125, 1);
                break;
            case 30:
                originalScript = "game.areas[4] = 1; game.story_state = 4";
                MakeAreaKnown(4);
                StoryState = 4;
                ;
                break;
            case 51:
            case 52:
                originalScript = "pc.money_adj(-17500); game.global_flags[360] = 1";
                pc.AdjustMoney(-17500);
                SetGlobalFlag(360, true);
                ;
                break;
            case 60:
            case 260:
                originalScript = "pc.follower_add( npc )";
                pc.AddFollower(npc);
                break;
            case 190:
                originalScript = "chain_it(npc, pc); pc.follower_add( npc )";
                chain_it(npc, pc);
                pc.AddFollower(npc);
                ;
                break;
            case 300:
                originalScript = "game.global_flags[74] = 1";
                SetGlobalFlag(74, true);
                break;
            case 331:
            case 334:
            case 341:
                originalScript = "npc.attack(pc)";
                npc.Attack(pc);
                break;
            case 360:
                originalScript = "pc.follower_remove(npc)";
                pc.RemoveFollower(npc);
                break;
            case 400:
                originalScript = "game.global_flags[72] = 1";
                SetGlobalFlag(72, true);
                break;
            case 401:
                originalScript = "make_elmo_talk(npc,pc,310)";
                make_elmo_talk(npc, pc, 310);
                break;
            case 421:
                originalScript = "make_elmo_talk(npc,pc,340)";
                make_elmo_talk(npc, pc, 340);
                break;
            case 431:
                originalScript = "make_elmo_talk(npc,pc,350)";
                make_elmo_talk(npc, pc, 350);
                break;
            case 451:
            case 452:
                originalScript = "talk_to_screng(npc,pc,390)";
                talk_to_screng(npc, pc, 390);
                break;
            case 453:
            case 454:
            case 463:
            case 464:
                originalScript = "talk_to_screng(npc,pc,260)";
                talk_to_screng(npc, pc, 260);
                break;
            case 455:
            case 456:
            case 465:
            case 466:
                originalScript = "talk_to_screng(npc,pc,270)";
                talk_to_screng(npc, pc, 270);
                break;
            case 501:
            case 502:
                originalScript = "switch_to_thrommel(npc,pc)";
                switch_to_thrommel(npc, pc);
                break;
            case 601:
                originalScript = "make_lila_talk(npc,pc,95)";
                make_lila_talk(npc, pc, 95);
                break;
            case 651:
                originalScript = "make_saduj_talk(npc,pc,40)";
                make_saduj_talk(npc, pc, 40);
                break;
            case 661:
            case 681:
            case 691:
                originalScript = "make_saduj_talk(npc,pc,90)";
                make_saduj_talk(npc, pc, 90);
                break;
            case 671:
            case 682:
            case 692:
                originalScript = "make_saduj_talk(npc,pc,50)";
                make_saduj_talk(npc, pc, 50);
                break;
            case 760:
                originalScript = "game.quests[63].state = qs_completed";
                SetQuestState(63, QuestState.Completed);
                break;
            case 790:
                originalScript = "game.quests[63].state = qs_botched";
                SetQuestState(63, QuestState.Botched);
                break;
            case 820:
                originalScript = "game.quests[31].state = qs_mentioned";
                SetQuestState(31, QuestState.Mentioned);
                break;
            case 821:
                originalScript = "chain_it(npc, pc); game.quests[31].state = qs_accepted";
                chain_it(npc, pc);
                SetQuestState(31, QuestState.Accepted);
                ;
                break;
            case 822:
            case 825:
            case 826:
                originalScript = "game.quests[31].state = qs_accepted";
                SetQuestState(31, QuestState.Accepted);
                break;
            case 22000:
                originalScript = "game.global_vars[903] = 32";
                SetGlobalVar(903, 32);
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
            case 251:
            case 252:
            case 663:
            case 673:
                skillChecks = new DialogSkillChecks(SkillId.intimidate, 10);
                return true;
            case 262:
            case 263:
                skillChecks = new DialogSkillChecks(SkillId.sense_motive, 6);
                return true;
            case 271:
            case 272:
            case 715:
            case 716:
                skillChecks = new DialogSkillChecks(SkillId.bluff, 10);
                return true;
            case 662:
            case 672:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 10);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}