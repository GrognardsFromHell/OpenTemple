
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
    [DialogScript(352)]
    public class CaptainAchanDialog : CaptainAchan, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    Trace.Assert(originalScript == "not npc.has_met(pc)");
                    return !npc.HasMet(pc);
                case 3:
                    Trace.Assert(originalScript == "npc.has_met(pc)");
                    return npc.HasMet(pc);
                case 11:
                case 71:
                case 211:
                case 261:
                case 271:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == LAWFUL_EVIL");
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL;
                case 12:
                case 72:
                case 212:
                case 262:
                case 272:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_GOOD or game.party_alignment == TRUE_NEUTRAL or game.party_alignment == NEUTRAL_EVIL");
                    return PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.NEUTRAL || PartyAlignment == Alignment.NEUTRAL_EVIL;
                case 13:
                case 73:
                case 213:
                case 263:
                case 273:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_GOOD or game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == CHAOTIC_EVIL");
                    return PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 14:
                    Trace.Assert(originalScript == "game.quests[74].state != qs_unknown");
                    return GetQuestState(74) != QuestState.Unknown;
                case 15:
                    Trace.Assert(originalScript == "game.quests[69].state != qs_unknown");
                    return GetQuestState(69) != QuestState.Unknown;
                case 21:
                    Trace.Assert(originalScript == "game.quests[79].state == qs_mentioned");
                    return GetQuestState(79) == QuestState.Mentioned;
                case 22:
                    Trace.Assert(originalScript == "game.quests[80].state == qs_mentioned");
                    return GetQuestState(80) == QuestState.Mentioned;
                case 23:
                    Trace.Assert(originalScript == "game.quests[81].state == qs_mentioned");
                    return GetQuestState(81) == QuestState.Mentioned;
                case 24:
                    Trace.Assert(originalScript == "game.global_vars[964] == 1");
                    return GetGlobalVar(964) == 1;
                case 25:
                    Trace.Assert(originalScript == "game.global_vars[964] == 0");
                    return GetGlobalVar(964) == 0;
                case 26:
                case 171:
                case 181:
                    Trace.Assert(originalScript == "game.global_flags[956] == 1 and game.quests[79].state != qs_completed");
                    return GetGlobalFlag(956) && GetQuestState(79) != QuestState.Completed;
                case 27:
                case 182:
                case 311:
                    Trace.Assert(originalScript == "game.global_flags[957] == 1 and game.quests[80].state != qs_completed");
                    return GetGlobalFlag(957) && GetQuestState(80) != QuestState.Completed;
                case 28:
                case 183:
                case 321:
                    Trace.Assert(originalScript == "game.global_flags[958] == 1 and game.quests[81].state != qs_completed");
                    return GetGlobalFlag(958) && GetQuestState(81) != QuestState.Completed;
                case 29:
                    Trace.Assert(originalScript == "game.global_vars[951] == 2");
                    return GetGlobalVar(951) == 2;
                case 30:
                    Trace.Assert(originalScript == "game.global_vars[952] == 2");
                    return GetGlobalVar(952) == 2;
                case 31:
                    Trace.Assert(originalScript == "game.global_vars[953] == 2");
                    return GetGlobalVar(953) == 2;
                case 32:
                    Trace.Assert(originalScript == "game.global_vars[964] == 4");
                    return GetGlobalVar(964) == 4;
                case 33:
                    Trace.Assert(originalScript == "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) or (game.quests[84].state == qs_accepted or game.quests[84].state == qs_mentioned) or (game.quests[66].state == qs_accepted or game.quests[66].state == qs_mentioned) or (game.quests[67].state == qs_accepted or game.quests[67].state == qs_mentioned)");
                    return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) || (GetQuestState(84) == QuestState.Accepted || GetQuestState(84) == QuestState.Mentioned) || (GetQuestState(66) == QuestState.Accepted || GetQuestState(66) == QuestState.Mentioned) || (GetQuestState(67) == QuestState.Accepted || GetQuestState(67) == QuestState.Mentioned);
                case 34:
                    Trace.Assert(originalScript == "game.quests[78].state == qs_accepted");
                    return GetQuestState(78) == QuestState.Accepted;
                case 35:
                    Trace.Assert(originalScript == "(game.global_flags[943] == 1 or game.global_vars[945] == 28) and not npc_get(npc,19)");
                    return (GetGlobalFlag(943) || GetGlobalVar(945) == 28) && !ScriptDaemon.npc_get(npc, 19);
                case 36:
                    Trace.Assert(originalScript == "game.global_vars[945] == 29 and not npc_get(npc,20)");
                    return GetGlobalVar(945) == 29 && !ScriptDaemon.npc_get(npc, 20);
                case 37:
                    Trace.Assert(originalScript == "game.global_vars[945] == 30 and not npc_get(npc,21)");
                    return GetGlobalVar(945) == 30 && !ScriptDaemon.npc_get(npc, 21);
                case 38:
                    Trace.Assert(originalScript == "game.quests[62].state == qs_accepted and (game.global_flags[560] == 0 or game.global_flags[561] == 0 or game.global_flags[562] == 0) and not npc_get(npc,22)");
                    return GetQuestState(62) == QuestState.Accepted && (!GetGlobalFlag(560) || !GetGlobalFlag(561) || !GetGlobalFlag(562)) && !ScriptDaemon.npc_get(npc, 22);
                case 61:
                    Trace.Assert(originalScript == "pc.reputation_has( 15 ) == 1");
                    return pc.HasReputation(15);
                case 62:
                    Trace.Assert(originalScript == "pc.reputation_has( 33 ) == 1");
                    return pc.HasReputation(33);
                case 63:
                    Trace.Assert(originalScript == "pc.reputation_has( 17 ) == 1");
                    return pc.HasReputation(17);
                case 64:
                    Trace.Assert(originalScript == "pc.reputation_has( 22 ) == 1");
                    return pc.HasReputation(22);
                case 91:
                case 111:
                case 121:
                case 201:
                    Trace.Assert(originalScript == "not npc_get(npc,1)");
                    return !ScriptDaemon.npc_get(npc, 1);
                case 92:
                case 101:
                case 122:
                case 202:
                    Trace.Assert(originalScript == "not npc_get(npc,2)");
                    return !ScriptDaemon.npc_get(npc, 2);
                case 93:
                case 102:
                case 112:
                case 203:
                    Trace.Assert(originalScript == "not npc_get(npc,3)");
                    return !ScriptDaemon.npc_get(npc, 3);
                case 131:
                    Trace.Assert(originalScript == "get_1(npc) and game.quests[79].state == qs_mentioned");
                    return Scripts.get_1(npc) && GetQuestState(79) == QuestState.Mentioned;
                case 132:
                    Trace.Assert(originalScript == "get_2(npc) and game.quests[80].state == qs_mentioned");
                    return Scripts.get_2(npc) && GetQuestState(80) == QuestState.Mentioned;
                case 133:
                    Trace.Assert(originalScript == "get_3(npc) and game.quests[81].state == qs_mentioned");
                    return Scripts.get_3(npc) && GetQuestState(81) == QuestState.Mentioned;
                case 191:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) >= 14");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 14;
                case 192:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) <= 13");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) <= 13;
                case 204:
                    Trace.Assert(originalScript == "not npc_get(npc,1) and not npc_get(npc,2) and not npc_get(npc,3)");
                    return !ScriptDaemon.npc_get(npc, 1) && !ScriptDaemon.npc_get(npc, 2) && !ScriptDaemon.npc_get(npc, 3);
                case 361:
                    Trace.Assert(originalScript == "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) and game.global_flags[989] == 0 and game.global_flags[946] == 0 and game.global_flags[863] == 0 and not npc_get(npc,7)");
                    return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) && !GetGlobalFlag(989) && !GetGlobalFlag(946) && !GetGlobalFlag(863) && !ScriptDaemon.npc_get(npc, 7);
                case 362:
                    Trace.Assert(originalScript == "(game.quests[84].state == qs_accepted or game.quests[84].state == qs_mentioned) and game.global_flags[973] == 0 and not npc_get(npc,6)");
                    return (GetQuestState(84) == QuestState.Accepted || GetQuestState(84) == QuestState.Mentioned) && !GetGlobalFlag(973) && !ScriptDaemon.npc_get(npc, 6);
                case 363:
                    Trace.Assert(originalScript == "(game.quests[67].state == qs_accepted or game.quests[67].state == qs_mentioned) and game.global_flags[989] == 0 and not npc_get(npc,5)");
                    return (GetQuestState(67) == QuestState.Accepted || GetQuestState(67) == QuestState.Mentioned) && !GetGlobalFlag(989) && !ScriptDaemon.npc_get(npc, 5);
                case 364:
                    Trace.Assert(originalScript == "(game.quests[66].state == qs_accepted or game.quests[66].state == qs_mentioned) and game.global_flags[989] == 0 and not npc_get(npc,4)");
                    return (GetQuestState(66) == QuestState.Accepted || GetQuestState(66) == QuestState.Mentioned) && !GetGlobalFlag(989) && !ScriptDaemon.npc_get(npc, 4);
                case 365:
                    Trace.Assert(originalScript == "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) and game.global_flags[989] == 0 and npc_get(npc,8) and not npc_get(npc,10)");
                    return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) && !GetGlobalFlag(989) && ScriptDaemon.npc_get(npc, 8) && !ScriptDaemon.npc_get(npc, 10);
                case 366:
                    Trace.Assert(originalScript == "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) and game.global_flags[989] == 0 and npc_get(npc,9) and not npc_get(npc,11)");
                    return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) && !GetGlobalFlag(989) && ScriptDaemon.npc_get(npc, 9) && !ScriptDaemon.npc_get(npc, 11);
                case 402:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 15");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 15;
                case 411:
                    Trace.Assert(originalScript == "game.quests[77].state == qs_mentioned");
                    return GetQuestState(77) == QuestState.Mentioned;
                case 412:
                    Trace.Assert(originalScript == "(game.global_vars[704] == 1 or game.global_vars[704] == 2) and game.quests[77].state == qs_accepted");
                    return (GetGlobalVar(704) == 1 || GetGlobalVar(704) == 2) && GetQuestState(77) == QuestState.Accepted;
                case 413:
                    Trace.Assert(originalScript == "game.global_vars[704] == 3 and game.quests[77].state == qs_accepted");
                    return GetGlobalVar(704) == 3 && GetQuestState(77) == QuestState.Accepted;
                case 421:
                    Trace.Assert(originalScript == "is_daytime()");
                    return Utilities.is_daytime();
                case 422:
                    Trace.Assert(originalScript == "not is_daytime()");
                    return !Utilities.is_daytime();
                case 491:
                    Trace.Assert(originalScript == "not npc_get(npc,12)");
                    return !ScriptDaemon.npc_get(npc, 12);
                case 492:
                    Trace.Assert(originalScript == "not npc_get(npc,13)");
                    return !ScriptDaemon.npc_get(npc, 13);
                case 493:
                    Trace.Assert(originalScript == "not npc_get(npc,14)");
                    return !ScriptDaemon.npc_get(npc, 14);
                case 494:
                    Trace.Assert(originalScript == "not npc_get(npc,15)");
                    return !ScriptDaemon.npc_get(npc, 15);
                case 502:
                    Trace.Assert(originalScript == "not npc_get(npc,16)");
                    return !ScriptDaemon.npc_get(npc, 16);
                case 503:
                    Trace.Assert(originalScript == "not npc_get(npc,17)");
                    return !ScriptDaemon.npc_get(npc, 17);
                case 504:
                    Trace.Assert(originalScript == "not npc_get(npc,18)");
                    return !ScriptDaemon.npc_get(npc, 18);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 35:
                    Trace.Assert(originalScript == "npc_set(npc,19)");
                    ScriptDaemon.npc_set(npc, 19);
                    break;
                case 36:
                    Trace.Assert(originalScript == "npc_set(npc,20)");
                    ScriptDaemon.npc_set(npc, 20);
                    break;
                case 37:
                    Trace.Assert(originalScript == "npc_set(npc,21)");
                    ScriptDaemon.npc_set(npc, 21);
                    break;
                case 38:
                    Trace.Assert(originalScript == "npc_set(npc,22)");
                    ScriptDaemon.npc_set(npc, 22);
                    break;
                case 90:
                    Trace.Assert(originalScript == "game.global_vars[964] = 1");
                    SetGlobalVar(964, 1);
                    break;
                case 91:
                case 111:
                case 121:
                case 201:
                    Trace.Assert(originalScript == "npc_set(npc,1)");
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 92:
                case 101:
                case 122:
                case 202:
                    Trace.Assert(originalScript == "npc_set(npc,2)");
                    ScriptDaemon.npc_set(npc, 2);
                    break;
                case 93:
                case 102:
                case 112:
                case 203:
                    Trace.Assert(originalScript == "npc_set(npc,3)");
                    ScriptDaemon.npc_set(npc, 3);
                    break;
                case 100:
                    Trace.Assert(originalScript == "game.quests[79].state = qs_mentioned");
                    SetQuestState(79, QuestState.Mentioned);
                    break;
                case 110:
                    Trace.Assert(originalScript == "game.quests[80].state = qs_mentioned");
                    SetQuestState(80, QuestState.Mentioned);
                    break;
                case 120:
                    Trace.Assert(originalScript == "game.quests[81].state = qs_mentioned");
                    SetQuestState(81, QuestState.Mentioned);
                    break;
                case 140:
                    Trace.Assert(originalScript == "game.quests[79].state = qs_accepted");
                    SetQuestState(79, QuestState.Accepted);
                    break;
                case 150:
                    Trace.Assert(originalScript == "game.quests[80].state = qs_accepted");
                    SetQuestState(80, QuestState.Accepted);
                    break;
                case 160:
                    Trace.Assert(originalScript == "game.quests[81].state = qs_accepted");
                    SetQuestState(81, QuestState.Accepted);
                    break;
                case 180:
                    Trace.Assert(originalScript == "game.global_vars[964] = game.global_vars[964] + 1");
                    SetGlobalVar(964, GetGlobalVar(964) + 1);
                    break;
                case 181:
                case 331:
                    Trace.Assert(originalScript == "pc.money_adj(30000); game.quests[79].state = qs_completed");
                    pc.AdjustMoney(30000);
                    SetQuestState(79, QuestState.Completed);
                    ;
                    break;
                case 182:
                case 341:
                    Trace.Assert(originalScript == "pc.money_adj(50000); game.quests[80].state = qs_completed");
                    pc.AdjustMoney(50000);
                    SetQuestState(80, QuestState.Completed);
                    ;
                    break;
                case 183:
                case 351:
                    Trace.Assert(originalScript == "pc.money_adj(40000); game.quests[81].state = qs_completed");
                    pc.AdjustMoney(40000);
                    SetQuestState(81, QuestState.Completed);
                    ;
                    break;
                case 190:
                case 260:
                    Trace.Assert(originalScript == "game.global_vars[964] = 5");
                    SetGlobalVar(964, 5);
                    break;
                case 230:
                    Trace.Assert(originalScript == "game.global_vars[969] = 1");
                    SetGlobalVar(969, 1);
                    break;
                case 232:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 240:
                    Trace.Assert(originalScript == "game.global_vars[969] = 2");
                    SetGlobalVar(969, 2);
                    break;
                case 241:
                    Trace.Assert(originalScript == "game.fade_and_teleport(0,0,0,5172,471,489)");
                    FadeAndTeleport(0, 0, 0, 5172, 471, 489);
                    break;
                case 250:
                    Trace.Assert(originalScript == "game.global_vars[962] = 2");
                    SetGlobalVar(962, 2);
                    break;
                case 290:
                case 300:
                    Trace.Assert(originalScript == "game.quests[79].state = qs_completed; game.quests[80].state = qs_completed; game.quests[81].state = qs_completed");
                    SetQuestState(79, QuestState.Completed);
                    SetQuestState(80, QuestState.Completed);
                    SetQuestState(81, QuestState.Completed);
                    ;
                    break;
                case 291:
                    Trace.Assert(originalScript == "pc.money_adj(120000)");
                    pc.AdjustMoney(120000);
                    break;
                case 330:
                    Trace.Assert(originalScript == "game.global_vars[951] = 3");
                    SetGlobalVar(951, 3);
                    break;
                case 340:
                    Trace.Assert(originalScript == "game.global_vars[952] = 3");
                    SetGlobalVar(952, 3);
                    break;
                case 350:
                    Trace.Assert(originalScript == "game.global_vars[953] = 3");
                    SetGlobalVar(953, 3);
                    break;
                case 361:
                    Trace.Assert(originalScript == "npc_set(npc,7)");
                    ScriptDaemon.npc_set(npc, 7);
                    break;
                case 362:
                    Trace.Assert(originalScript == "npc_set(npc,6)");
                    ScriptDaemon.npc_set(npc, 6);
                    break;
                case 363:
                    Trace.Assert(originalScript == "npc_set(npc,5)");
                    ScriptDaemon.npc_set(npc, 5);
                    break;
                case 364:
                    Trace.Assert(originalScript == "npc_set(npc,4)");
                    ScriptDaemon.npc_set(npc, 4);
                    break;
                case 365:
                    Trace.Assert(originalScript == "npc_set(npc,10)");
                    ScriptDaemon.npc_set(npc, 10);
                    break;
                case 366:
                    Trace.Assert(originalScript == "npc_set(npc,11)");
                    ScriptDaemon.npc_set(npc, 11);
                    break;
                case 402:
                    Trace.Assert(originalScript == "npc_set(npc,8)");
                    ScriptDaemon.npc_set(npc, 8);
                    break;
                case 403:
                    Trace.Assert(originalScript == "npc_set(npc,9)");
                    ScriptDaemon.npc_set(npc, 9);
                    break;
                case 410:
                    Trace.Assert(originalScript == "game.global_vars[979] = 2");
                    SetGlobalVar(979, 2);
                    break;
                case 420:
                    Trace.Assert(originalScript == "game.global_vars[946] = 1");
                    SetGlobalVar(946, 1);
                    break;
                case 421:
                    Trace.Assert(originalScript == "game.fade_and_teleport(0,0,0,5170,476,483)");
                    FadeAndTeleport(0, 0, 0, 5170, 476, 483);
                    break;
                case 422:
                    Trace.Assert(originalScript == "game.fade_and_teleport(0,0,0,5135,480,477)");
                    FadeAndTeleport(0, 0, 0, 5135, 480, 477);
                    break;
                case 440:
                    Trace.Assert(originalScript == "game.global_flags[940] = 1");
                    SetGlobalFlag(940, true);
                    break;
                case 460:
                    Trace.Assert(originalScript == "game.global_vars[946] = 2; record_time_stamp('achan_off_to_arrest')");
                    SetGlobalVar(946, 2);
                    ScriptDaemon.record_time_stamp("achan_off_to_arrest");
                    ;
                    break;
                case 461:
                    Trace.Assert(originalScript == "switch_to_wilfrick( npc, pc, 980)");
                    switch_to_wilfrick(npc, pc, 980);
                    break;
                case 470:
                    Trace.Assert(originalScript == "game.global_vars[944] = 3");
                    SetGlobalVar(944, 3);
                    break;
                case 471:
                    Trace.Assert(originalScript == "run_off(npc,pc)");
                    run_off(npc, pc);
                    break;
                case 491:
                    Trace.Assert(originalScript == "npc_set(npc,12)");
                    ScriptDaemon.npc_set(npc, 12);
                    break;
                case 492:
                    Trace.Assert(originalScript == "npc_set(npc,13)");
                    ScriptDaemon.npc_set(npc, 13);
                    break;
                case 493:
                    Trace.Assert(originalScript == "npc_set(npc,14)");
                    ScriptDaemon.npc_set(npc, 14);
                    break;
                case 494:
                    Trace.Assert(originalScript == "npc_set(npc,15)");
                    ScriptDaemon.npc_set(npc, 15);
                    break;
                case 502:
                    Trace.Assert(originalScript == "npc_set(npc,16)");
                    ScriptDaemon.npc_set(npc, 16);
                    break;
                case 503:
                    Trace.Assert(originalScript == "npc_set(npc,17)");
                    ScriptDaemon.npc_set(npc, 17);
                    break;
                case 504:
                    Trace.Assert(originalScript == "npc_set(npc,18)");
                    ScriptDaemon.npc_set(npc, 18);
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillCheck(int lineNumber, out DialogSkillChecks skillChecks)
        {
            switch (lineNumber)
            {
                case 191:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 14);
                    return true;
                case 402:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 15);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}