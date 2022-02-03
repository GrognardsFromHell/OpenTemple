
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
    [DialogScript(340)]
    public class AssassinLeaderDialog : AssassinLeader, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    originalScript = "not npc.has_met( pc ) and pc.stat_level_get( stat_gender ) == gender_male";
                    return !npc.HasMet(pc) && pc.GetGender() == Gender.Male;
                case 3:
                    originalScript = "npc.has_met( pc ) and pc.stat_level_get( stat_gender ) == gender_male";
                    return npc.HasMet(pc) && pc.GetGender() == Gender.Male;
                case 4:
                    originalScript = "not npc.has_met( pc ) and pc.stat_level_get( stat_gender ) == gender_female";
                    return !npc.HasMet(pc) && pc.GetGender() == Gender.Female;
                case 5:
                    originalScript = "npc.has_met( pc ) and pc.stat_level_get( stat_gender ) == gender_female";
                    return npc.HasMet(pc) && pc.GetGender() == Gender.Female;
                case 21:
                case 121:
                    originalScript = "game.global_vars[704] == 1 and game.quests[77].state == qs_accepted";
                    return GetGlobalVar(704) == 1 && GetQuestState(77) == QuestState.Accepted;
                case 22:
                case 122:
                    originalScript = "game.global_flags[992] == 1 and game.global_flags[935] == 0 and game.quests[77].state == qs_accepted";
                    return GetGlobalFlag(992) && !GetGlobalFlag(935) && GetQuestState(77) == QuestState.Accepted;
                case 23:
                case 123:
                    originalScript = "game.quests[76].state == qs_accepted";
                    return GetQuestState(76) == QuestState.Accepted;
                case 24:
                case 124:
                    originalScript = "game.quests[76].state == qs_mentioned";
                    return GetQuestState(76) == QuestState.Mentioned;
                case 25:
                case 125:
                    originalScript = "game.global_flags[965] == 1 and game.quests[66].state == qs_accepted";
                    return GetGlobalFlag(965) && GetQuestState(66) == QuestState.Accepted;
                case 26:
                case 126:
                    originalScript = "game.global_flags[964] == 1 and game.global_flags[963] == 0 and game.quests[67].state == qs_accepted";
                    return GetGlobalFlag(964) && !GetGlobalFlag(963) && GetQuestState(67) == QuestState.Accepted;
                case 27:
                case 127:
                    originalScript = "game.global_flags[964] == 1 and game.global_flags[963] == 1 and game.quests[67].state == qs_accepted and game.party[0].reputation_has(35)";
                    return GetGlobalFlag(964) && GetGlobalFlag(963) && GetQuestState(67) == QuestState.Accepted && PartyLeader.HasReputation(35);
                case 28:
                case 128:
                    originalScript = "game.quests[66].state == qs_mentioned";
                    return GetQuestState(66) == QuestState.Mentioned;
                case 29:
                case 129:
                    originalScript = "game.quests[67].state == qs_mentioned";
                    return GetQuestState(67) == QuestState.Mentioned;
                case 30:
                case 130:
                    originalScript = "game.quests[77].state == qs_mentioned";
                    return GetQuestState(77) == QuestState.Mentioned;
                case 31:
                case 131:
                    originalScript = "game.global_vars[978] == 1 and game.global_vars[965] == 0 and (pc.skill_level_get(npc,skill_spot) >= 11 or party_spot_check() >= 11)";
                    throw new NotSupportedException("Conversion failed.");
                case 32:
                case 132:
                    originalScript = "game.global_vars[978] == 3 and game.global_vars[965] == 1";
                    return GetGlobalVar(978) == 3 && GetGlobalVar(965) == 1;
                case 33:
                case 133:
                    originalScript = "game.global_vars[978] == 4 and game.global_vars[965] == 1";
                    return GetGlobalVar(978) == 4 && GetGlobalVar(965) == 1;
                case 34:
                case 134:
                    originalScript = "game.global_vars[978] == 5 and game.global_vars[965] == 1";
                    return GetGlobalVar(978) == 5 && GetGlobalVar(965) == 1;
                case 35:
                case 135:
                    originalScript = "game.global_vars[958] == 8";
                    return GetGlobalVar(958) == 8;
                case 36:
                case 136:
                    originalScript = "game.global_vars[959] == 4";
                    return GetGlobalVar(959) == 4;
                case 37:
                case 137:
                    originalScript = "game.global_vars[704] == 2 and game.quests[77].state == qs_accepted and is_daytime()";
                    return GetGlobalVar(704) == 2 && GetQuestState(77) == QuestState.Accepted && Utilities.is_daytime();
                case 38:
                case 138:
                    originalScript = "game.global_flags[964] == 1 and game.global_flags[963] == 1 and game.quests[67].state == qs_accepted and not game.party[0].reputation_has(35)";
                    return GetGlobalFlag(964) && GetGlobalFlag(963) && GetQuestState(67) == QuestState.Accepted && !PartyLeader.HasReputation(35);
                case 39:
                case 139:
                    originalScript = "game.global_vars[704] == 20";
                    return GetGlobalVar(704) == 20;
                case 40:
                case 140:
                    originalScript = "game.global_flags[992] == 0 and game.global_flags[935] == 0 and game.quests[77].state == qs_accepted and game.global_vars[704] == 9";
                    return !GetGlobalFlag(992) && !GetGlobalFlag(935) && GetQuestState(77) == QuestState.Accepted && GetGlobalVar(704) == 9;
                case 41:
                case 141:
                    originalScript = "game.global_flags[960] == 1 and game.quests[78].state == qs_accepted";
                    return GetGlobalFlag(960) && GetQuestState(78) == QuestState.Accepted;
                case 46:
                case 146:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 10 and pc.reputation_has( 24 ) == 0";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 10 && !pc.HasReputation(24);
                case 47:
                case 147:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) <= 9 and pc.reputation_has( 24 ) == 0";
                    return pc.GetSkillLevel(npc, SkillId.bluff) <= 9 && !pc.HasReputation(24);
                case 48:
                case 148:
                    originalScript = "pc.reputation_has( 24 ) == 1";
                    return pc.HasReputation(24);
                case 71:
                case 171:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) <= 9 and pc.reputation_has( 24 ) == 0 and pc.reputation_has( 21 ) == 0";
                    return pc.GetSkillLevel(npc, SkillId.bluff) <= 9 && !pc.HasReputation(24) && !pc.HasReputation(21);
                case 72:
                case 172:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 10 and pc.reputation_has( 24 ) == 0 and pc.reputation_has( 21 ) == 0";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 10 && !pc.HasReputation(24) && !pc.HasReputation(21);
                case 73:
                case 173:
                    originalScript = "pc.reputation_has( 24 ) == 1 and pc.reputation_has( 21 ) == 0";
                    return pc.HasReputation(24) && !pc.HasReputation(21);
                case 74:
                case 174:
                    originalScript = "pc.reputation_has( 24 ) == 1 and pc.reputation_has( 21 ) == 1";
                    return pc.HasReputation(24) && pc.HasReputation(21);
                case 261:
                    originalScript = "(anyone( pc.group_list(), \"item_find\", 12036 ))";
                    return (pc.GetPartyMembers().Any(o => o.FindItemByName(12036) != null));
                case 281:
                case 561:
                    originalScript = "game.global_flags[992] == 0";
                    return !GetGlobalFlag(992);
                case 282:
                case 562:
                    originalScript = "game.global_flags[992] == 1";
                    return GetGlobalFlag(992);
                case 311:
                    originalScript = "game.global_flags[94] == 1";
                    return GetGlobalFlag(94);
                case 312:
                    originalScript = "game.global_flags[94] == 0";
                    return !GetGlobalFlag(94);
                case 351:
                case 361:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 9";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 9;
                case 352:
                case 362:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) <= 8";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) <= 8;
                case 392:
                case 402:
                    originalScript = "game.global_flags[704] == 1";
                    return GetGlobalFlag(704);
                case 441:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 8";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 8;
                case 444:
                case 462:
                case 471:
                    originalScript = "game.global_flags[974] == 0";
                    return !GetGlobalFlag(974);
                case 445:
                case 463:
                case 473:
                    originalScript = "game.global_flags[974] == 1";
                    return GetGlobalFlag(974);
                case 621:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 13";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 13;
                case 632:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 16 and game.quests[77].state == qs_botched";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 16 && GetQuestState(77) == QuestState.Botched;
                case 633:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 16 and game.quests[77].state == qs_completed";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 16 && GetQuestState(77) == QuestState.Completed;
                case 641:
                    originalScript = "game.quests[77].state == qs_botched";
                    return GetQuestState(77) == QuestState.Botched;
                case 642:
                    originalScript = "game.quests[77].state == qs_completed";
                    return GetQuestState(77) == QuestState.Completed;
                case 661:
                    originalScript = "game.global_vars[945] == 1";
                    return GetGlobalVar(945) == 1;
                case 662:
                    originalScript = "game.global_vars[945] == 2";
                    return GetGlobalVar(945) == 2;
                case 663:
                    originalScript = "game.global_vars[945] == 3";
                    return GetGlobalVar(945) == 3;
                case 751:
                    originalScript = "not npc_get(npc,1)";
                    return !ScriptDaemon.npc_get(npc, 1);
                case 752:
                    originalScript = "not npc_get(npc,2)";
                    return !ScriptDaemon.npc_get(npc, 2);
                case 753:
                    originalScript = "not npc_get(npc,3)";
                    return !ScriptDaemon.npc_get(npc, 3);
                case 754:
                    originalScript = "not npc_get(npc,4)";
                    return !ScriptDaemon.npc_get(npc, 4);
                case 762:
                    originalScript = "not npc_get(npc,5)";
                    return !ScriptDaemon.npc_get(npc, 5);
                case 763:
                    originalScript = "not npc_get(npc,6)";
                    return !ScriptDaemon.npc_get(npc, 6);
                case 764:
                    originalScript = "not npc_get(npc,7)";
                    return !ScriptDaemon.npc_get(npc, 7);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 23:
                case 123:
                case 600:
                case 610:
                    originalScript = "game.quests[77].state = qs_botched";
                    SetQuestState(77, QuestState.Botched);
                    break;
                case 28:
                case 128:
                case 341:
                    originalScript = "game.quests[66].state = qs_accepted";
                    SetQuestState(66, QuestState.Accepted);
                    break;
                case 29:
                case 129:
                case 271:
                    originalScript = "game.quests[67].state = qs_accepted; create_item_in_inventory( 4244, pc )";
                    SetQuestState(67, QuestState.Accepted);
                    Utilities.create_item_in_inventory(4244, pc);
                    ;
                    break;
                case 31:
                case 131:
                    originalScript = "game.global_vars[965] = 1";
                    SetGlobalVar(965, 1);
                    break;
                case 32:
                case 33:
                case 34:
                case 132:
                case 133:
                case 134:
                    originalScript = "game.global_vars[978] = 6; game.global_vars[965] = 2";
                    SetGlobalVar(978, 6);
                    SetGlobalVar(965, 2);
                    ;
                    break;
                case 35:
                case 135:
                    originalScript = "game.global_vars[958] = 9";
                    SetGlobalVar(958, 9);
                    break;
                case 36:
                case 136:
                    originalScript = "game.global_vars[959] = 5";
                    SetGlobalVar(959, 5);
                    break;
                case 37:
                case 137:
                    originalScript = "game.global_vars[704] = 3";
                    SetGlobalVar(704, 3);
                    break;
                case 39:
                case 139:
                    originalScript = "game.global_vars[704] = 21";
                    SetGlobalVar(704, 21);
                    break;
                case 40:
                case 140:
                    originalScript = "game.global_vars[704] = 10";
                    SetGlobalVar(704, 10);
                    break;
                case 52:
                case 152:
                case 162:
                case 241:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
                    break;
                case 210:
                    originalScript = "game.quests[77].state = qs_mentioned";
                    SetQuestState(77, QuestState.Mentioned);
                    break;
                case 230:
                    originalScript = "pc.reputation_add( 19 ); pc.money_adj(1000000); game.quests[77].state = qs_completed";
                    pc.AddReputation(19);
                    pc.AdjustMoney(1000000);
                    SetQuestState(77, QuestState.Completed);
                    ;
                    break;
                case 240:
                case 740:
                    originalScript = "game.party[0].reputation_add( 50 )";
                    PartyLeader.AddReputation(50);
                    break;
                case 250:
                    originalScript = "game.quests[66].state = qs_mentioned; game.global_flags[960] = 1";
                    SetQuestState(66, QuestState.Mentioned);
                    SetGlobalFlag(960, true);
                    ;
                    break;
                case 261:
                    originalScript = "game.quests[66].state = qs_completed";
                    SetQuestState(66, QuestState.Completed);
                    break;
                case 262:
                    originalScript = "game.quests[66].state = qs_botched";
                    SetQuestState(66, QuestState.Botched);
                    break;
                case 270:
                    originalScript = "game.quests[67].state = qs_mentioned";
                    SetQuestState(67, QuestState.Mentioned);
                    break;
                case 280:
                case 560:
                    originalScript = "pc.money_adj(200000); game.quests[67].state = qs_completed";
                    pc.AdjustMoney(200000);
                    SetQuestState(67, QuestState.Completed);
                    ;
                    break;
                case 300:
                    originalScript = "pc.money_adj(100000)";
                    pc.AdjustMoney(100000);
                    break;
                case 301:
                    originalScript = "party_transfer_to( npc, 12036 )";
                    Utilities.party_transfer_to(npc, 12036);
                    break;
                case 310:
                    originalScript = "game.quests[67].state = qs_botched";
                    SetQuestState(67, QuestState.Botched);
                    break;
                case 320:
                    originalScript = "game.global_vars[958] = 1";
                    SetGlobalVar(958, 1);
                    break;
                case 330:
                    originalScript = "game.global_vars[959] = 1";
                    SetGlobalVar(959, 1);
                    break;
                case 370:
                    originalScript = "game.quests[68].state = qs_mentioned";
                    SetQuestState(68, QuestState.Mentioned);
                    break;
                case 410:
                case 420:
                    originalScript = "game.quests[68].state = qs_completed";
                    SetQuestState(68, QuestState.Completed);
                    break;
                case 412:
                case 422:
                case 432:
                case 443:
                case 461:
                case 472:
                case 631:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 523:
                    originalScript = "game.quests[77].state = qs_botched; npc.attack( pc )";
                    SetQuestState(77, QuestState.Botched);
                    npc.Attack(pc);
                    ;
                    break;
                case 530:
                    originalScript = "game.quests[77].state = qs_accepted; game.global_vars[704] = 1";
                    SetQuestState(77, QuestState.Accepted);
                    SetGlobalVar(704, 1);
                    ;
                    break;
                case 531:
                    originalScript = "wilfrick_countdown(npc,pc)";
                    wilfrick_countdown(npc, pc);
                    break;
                case 540:
                    originalScript = "game.global_vars[950] = 1";
                    SetGlobalVar(950, 1);
                    break;
                case 640:
                    originalScript = "game.global_vars[704] = 11";
                    SetGlobalVar(704, 11);
                    break;
                case 641:
                    originalScript = "pc.money_adj(2000000)";
                    pc.AdjustMoney(2000000);
                    break;
                case 642:
                    originalScript = "pc.money_adj(1000000)";
                    pc.AdjustMoney(1000000);
                    break;
                case 661:
                    originalScript = "schedule_sb_retaliation_for_snitch(npc,pc)";
                    schedule_sb_retaliation_for_snitch(npc, pc);
                    break;
                case 662:
                    originalScript = "schedule_sb_retaliation_for_narc(npc,pc)";
                    schedule_sb_retaliation_for_narc(npc, pc);
                    break;
                case 663:
                    originalScript = "schedule_sb_retaliation_for_whistleblower(npc,pc)";
                    schedule_sb_retaliation_for_whistleblower(npc, pc);
                    break;
                case 690:
                    originalScript = "darlia_release(npc,pc)";
                    darlia_release(npc, pc);
                    break;
                case 700:
                    originalScript = "game.party[0].reputation_add( 49 )";
                    PartyLeader.AddReputation(49);
                    break;
                case 710:
                    originalScript = "game.party[0].reputation_add( 48 )";
                    PartyLeader.AddReputation(48);
                    break;
                case 751:
                    originalScript = "npc_set(npc,1)";
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 752:
                    originalScript = "npc_set(npc,2)";
                    ScriptDaemon.npc_set(npc, 2);
                    break;
                case 753:
                    originalScript = "npc_set(npc,3)";
                    ScriptDaemon.npc_set(npc, 3);
                    break;
                case 754:
                    originalScript = "npc_set(npc,4)";
                    ScriptDaemon.npc_set(npc, 4);
                    break;
                case 762:
                    originalScript = "npc_set(npc,5)";
                    ScriptDaemon.npc_set(npc, 5);
                    break;
                case 763:
                    originalScript = "npc_set(npc,6)";
                    ScriptDaemon.npc_set(npc, 6);
                    break;
                case 764:
                    originalScript = "npc_set(npc,7)";
                    ScriptDaemon.npc_set(npc, 7);
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
                case 46:
                case 72:
                case 146:
                case 172:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 10);
                    return true;
                case 351:
                case 361:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 9);
                    return true;
                case 441:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 8);
                    return true;
                case 621:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 13);
                    return true;
                case 632:
                case 633:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 16);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
