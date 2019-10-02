
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
    [DialogScript(351)]
    public class CaptainAbsalomDialog : CaptainAbsalom, IDialogScript
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
                case 13:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_gather_information) >= 11 and (game.global_flags[968] == 1 or game.global_flags[986] == 1 or game.global_flags[981] == 1)");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 11 && (GetGlobalFlag(968) || GetGlobalFlag(986) || GetGlobalFlag(981));
                case 14:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_gather_information) <= 10 and (game.global_flags[968] == 1 or game.global_flags[986] == 1 or game.global_flags[981] == 1)");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) <= 10 && (GetGlobalFlag(968) || GetGlobalFlag(986) || GetGlobalFlag(981));
                case 15:
                case 25:
                case 44:
                case 63:
                case 73:
                case 464:
                    Trace.Assert(originalScript == "game.global_vars[977] == 1");
                    return GetGlobalVar(977) == 1;
                case 16:
                case 27:
                    Trace.Assert(originalScript == "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) or (game.quests[84].state == qs_accepted or game.quests[84].state == qs_mentioned) or (game.quests[66].state == qs_accepted or game.quests[66].state == qs_mentioned) or (game.quests[67].state == qs_accepted or game.quests[67].state == qs_mentioned)");
                    return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) || (GetQuestState(84) == QuestState.Accepted || GetQuestState(84) == QuestState.Mentioned) || (GetQuestState(66) == QuestState.Accepted || GetQuestState(66) == QuestState.Mentioned) || (GetQuestState(67) == QuestState.Accepted || GetQuestState(67) == QuestState.Mentioned);
                case 21:
                case 41:
                case 61:
                case 71:
                    Trace.Assert(originalScript == "not npc_get(npc,1)");
                    return !ScriptDaemon.npc_get(npc, 1);
                case 22:
                case 62:
                case 72:
                case 461:
                    Trace.Assert(originalScript == "not npc_get(npc,2)");
                    return !ScriptDaemon.npc_get(npc, 2);
                case 23:
                case 42:
                case 462:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_gather_information) >= 11 and not npc_get(npc,3) and (game.global_flags[968] == 1 or game.global_flags[986] == 1 or game.global_flags[981] == 1)");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 11 && !ScriptDaemon.npc_get(npc, 3) && (GetGlobalFlag(968) || GetGlobalFlag(986) || GetGlobalFlag(981));
                case 24:
                case 43:
                case 463:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_gather_information) <= 10 and not npc_get(npc,3) and (game.global_flags[968] == 1 or game.global_flags[986] == 1 or game.global_flags[981] == 1)");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) <= 10 && !ScriptDaemon.npc_get(npc, 3) && (GetGlobalFlag(968) || GetGlobalFlag(986) || GetGlobalFlag(981));
                case 26:
                    Trace.Assert(originalScript == "game.global_vars[976] == 1 and game.quests[69].state != qs_completed");
                    return GetGlobalVar(976) == 1 && GetQuestState(69) != QuestState.Completed;
                case 28:
                    Trace.Assert(originalScript == "(game.global_flags[943] == 1 or game.global_vars[945] == 28) and not npc_get(npc,18)");
                    return (GetGlobalFlag(943) || GetGlobalVar(945) == 28) && !ScriptDaemon.npc_get(npc, 18);
                case 29:
                    Trace.Assert(originalScript == "game.global_vars[945] == 29 and not npc_get(npc,26)");
                    return GetGlobalVar(945) == 29 && !ScriptDaemon.npc_get(npc, 26);
                case 30:
                    Trace.Assert(originalScript == "game.global_vars[945] == 30 and not npc_get(npc,27)");
                    return GetGlobalVar(945) == 30 && !ScriptDaemon.npc_get(npc, 27);
                case 31:
                    Trace.Assert(originalScript == "game.quests[78].state == qs_accepted");
                    return GetQuestState(78) == QuestState.Accepted;
                case 32:
                    Trace.Assert(originalScript == "game.quests[62].state == qs_accepted and (game.global_flags[560] == 0 or game.global_flags[561] == 0 or game.global_flags[562] == 0) and not npc_get(npc,28)");
                    return GetQuestState(62) == QuestState.Accepted && (!GetGlobalFlag(560) || !GetGlobalFlag(561) || !GetGlobalFlag(562)) && !ScriptDaemon.npc_get(npc, 28);
                case 51:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_gather_information) >= 11");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 11;
                case 52:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_gather_information) >= 11 and game.global_vars[977] == 1");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 11 && GetGlobalVar(977) == 1;
                case 53:
                case 54:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_gather_information) <= 10");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) <= 10;
                case 93:
                case 103:
                    Trace.Assert(originalScript == "game.global_vars[999] >= 1 and game.quests[69].state != qs_completed");
                    return GetGlobalVar(999) >= 1 && GetQuestState(69) != QuestState.Completed;
                case 111:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 9");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 9;
                case 112:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) <= 8");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) <= 8;
                case 131:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 0");
                    return !pc.HasMaxFollowers();
                case 132:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 1");
                    return pc.HasMaxFollowers();
                case 251:
                    Trace.Assert(originalScript == "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) and game.global_flags[989] == 0 and game.global_flags[946] == 0 and game.global_flags[863] == 0 and not npc_get(npc,4)");
                    return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) && !GetGlobalFlag(989) && !GetGlobalFlag(946) && !GetGlobalFlag(863) && !ScriptDaemon.npc_get(npc, 4);
                case 252:
                    Trace.Assert(originalScript == "(game.quests[84].state == qs_accepted or game.quests[84].state == qs_mentioned) and game.global_flags[973] == 0 and not npc_get(npc,5)");
                    return (GetQuestState(84) == QuestState.Accepted || GetQuestState(84) == QuestState.Mentioned) && !GetGlobalFlag(973) && !ScriptDaemon.npc_get(npc, 5);
                case 253:
                    Trace.Assert(originalScript == "(game.quests[67].state == qs_accepted or game.quests[67].state == qs_mentioned) and game.global_flags[989] == 0 and not npc_get(npc,6)");
                    return (GetQuestState(67) == QuestState.Accepted || GetQuestState(67) == QuestState.Mentioned) && !GetGlobalFlag(989) && !ScriptDaemon.npc_get(npc, 6);
                case 254:
                    Trace.Assert(originalScript == "(game.quests[66].state == qs_accepted or game.quests[66].state == qs_mentioned) and game.global_flags[989] == 0 and not npc_get(npc,7)");
                    return (GetQuestState(66) == QuestState.Accepted || GetQuestState(66) == QuestState.Mentioned) && !GetGlobalFlag(989) && !ScriptDaemon.npc_get(npc, 7);
                case 255:
                    Trace.Assert(originalScript == "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) and game.global_flags[989] == 0 and npc_get(npc,8) and not npc_get(npc,10)");
                    return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) && !GetGlobalFlag(989) && ScriptDaemon.npc_get(npc, 8) && !ScriptDaemon.npc_get(npc, 10);
                case 256:
                    Trace.Assert(originalScript == "(game.quests[77].state == qs_accepted or game.quests[77].state == qs_mentioned) and game.global_flags[989] == 0 and npc_get(npc,9) and not npc_get(npc,11)");
                    return (GetQuestState(77) == QuestState.Accepted || GetQuestState(77) == QuestState.Mentioned) && !GetGlobalFlag(989) && ScriptDaemon.npc_get(npc, 9) && !ScriptDaemon.npc_get(npc, 11);
                case 257:
                    Trace.Assert(originalScript == "(game.quests[67].state == qs_accepted or game.quests[67].state == qs_mentioned) and npc_get(npc,10) and not npc_get(npc,14)");
                    return (GetQuestState(67) == QuestState.Accepted || GetQuestState(67) == QuestState.Mentioned) && ScriptDaemon.npc_get(npc, 10) && !ScriptDaemon.npc_get(npc, 14);
                case 258:
                    Trace.Assert(originalScript == "(game.quests[67].state == qs_accepted or game.quests[67].state == qs_mentioned) and npc_get(npc,11) and not npc_get(npc,15)");
                    return (GetQuestState(67) == QuestState.Accepted || GetQuestState(67) == QuestState.Mentioned) && ScriptDaemon.npc_get(npc, 11) && !ScriptDaemon.npc_get(npc, 15);
                case 259:
                    Trace.Assert(originalScript == "(game.quests[66].state == qs_accepted or game.quests[66].state == qs_mentioned) and npc_get(npc,12) and not npc_get(npc,16)");
                    return (GetQuestState(66) == QuestState.Accepted || GetQuestState(66) == QuestState.Mentioned) && ScriptDaemon.npc_get(npc, 12) && !ScriptDaemon.npc_get(npc, 16);
                case 260:
                    Trace.Assert(originalScript == "(game.quests[66].state == qs_accepted or game.quests[66].state == qs_mentioned) and npc_get(npc,13) and not npc_get(npc,17)");
                    return (GetQuestState(66) == QuestState.Accepted || GetQuestState(66) == QuestState.Mentioned) && ScriptDaemon.npc_get(npc, 13) && !ScriptDaemon.npc_get(npc, 17);
                case 282:
                case 292:
                case 412:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 15");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 15;
                case 301:
                    Trace.Assert(originalScript == "game.quests[77].state == qs_mentioned");
                    return GetQuestState(77) == QuestState.Mentioned;
                case 302:
                    Trace.Assert(originalScript == "(game.global_vars[704] == 1 or game.global_vars[704] == 2) and game.quests[77].state == qs_accepted");
                    return (GetGlobalVar(704) == 1 || GetGlobalVar(704) == 2) && GetQuestState(77) == QuestState.Accepted;
                case 303:
                    Trace.Assert(originalScript == "game.global_vars[704] == 3 and game.quests[77].state == qs_accepted");
                    return GetGlobalVar(704) == 3 && GetQuestState(77) == QuestState.Accepted;
                case 311:
                    Trace.Assert(originalScript == "is_daytime()");
                    return Utilities.is_daytime();
                case 312:
                    Trace.Assert(originalScript == "not is_daytime()");
                    return !Utilities.is_daytime();
                case 371:
                    Trace.Assert(originalScript == "game.quests[67].state == qs_mentioned");
                    return GetQuestState(67) == QuestState.Mentioned;
                case 372:
                    Trace.Assert(originalScript == "game.quests[67].state == qs_accepted");
                    return GetQuestState(67) == QuestState.Accepted;
                case 381:
                    Trace.Assert(originalScript == "game.quests[66].state == qs_mentioned");
                    return GetQuestState(66) == QuestState.Mentioned;
                case 382:
                    Trace.Assert(originalScript == "game.quests[66].state == qs_accepted");
                    return GetQuestState(66) == QuestState.Accepted;
                case 471:
                    Trace.Assert(originalScript == "not npc_get(npc,19)");
                    return !ScriptDaemon.npc_get(npc, 19);
                case 472:
                    Trace.Assert(originalScript == "not npc_get(npc,20)");
                    return !ScriptDaemon.npc_get(npc, 20);
                case 473:
                    Trace.Assert(originalScript == "not npc_get(npc,21)");
                    return !ScriptDaemon.npc_get(npc, 21);
                case 474:
                    Trace.Assert(originalScript == "not npc_get(npc,22)");
                    return !ScriptDaemon.npc_get(npc, 22);
                case 482:
                    Trace.Assert(originalScript == "not npc_get(npc,23)");
                    return !ScriptDaemon.npc_get(npc, 23);
                case 483:
                    Trace.Assert(originalScript == "not npc_get(npc,24)");
                    return !ScriptDaemon.npc_get(npc, 24);
                case 484:
                    Trace.Assert(originalScript == "not npc_get(npc,25)");
                    return !ScriptDaemon.npc_get(npc, 25);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 21:
                case 41:
                case 61:
                case 71:
                    Trace.Assert(originalScript == "npc_set(npc,1)");
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 12:
                case 22:
                case 62:
                case 72:
                case 461:
                    Trace.Assert(originalScript == "npc_set(npc,2)");
                    ScriptDaemon.npc_set(npc, 2);
                    break;
                case 13:
                case 14:
                case 23:
                case 24:
                case 42:
                case 43:
                case 462:
                case 463:
                    Trace.Assert(originalScript == "npc_set(npc,3)");
                    ScriptDaemon.npc_set(npc, 3);
                    break;
                case 15:
                case 25:
                case 44:
                case 63:
                case 73:
                case 464:
                    Trace.Assert(originalScript == "game.global_vars[977] = 2");
                    SetGlobalVar(977, 2);
                    break;
                case 28:
                    Trace.Assert(originalScript == "npc_set(npc,18)");
                    ScriptDaemon.npc_set(npc, 18);
                    break;
                case 29:
                    Trace.Assert(originalScript == "npc_set(npc,26)");
                    ScriptDaemon.npc_set(npc, 26);
                    break;
                case 30:
                    Trace.Assert(originalScript == "npc_set(npc,27)");
                    ScriptDaemon.npc_set(npc, 27);
                    break;
                case 32:
                    Trace.Assert(originalScript == "npc_set(npc,28)");
                    ScriptDaemon.npc_set(npc, 28);
                    break;
                case 130:
                    Trace.Assert(originalScript == "game.global_vars[976] = 1");
                    SetGlobalVar(976, 1);
                    break;
                case 150:
                    Trace.Assert(originalScript == "game.global_vars[976] = 2");
                    SetGlobalVar(976, 2);
                    break;
                case 180:
                    Trace.Assert(originalScript == "game.global_vars[969] = 1");
                    SetGlobalVar(969, 1);
                    break;
                case 182:
                case 232:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 190:
                    Trace.Assert(originalScript == "game.global_vars[969] = 2");
                    SetGlobalVar(969, 2);
                    break;
                case 191:
                    Trace.Assert(originalScript == "game.fade_and_teleport(0,0,0,5172,471,489)");
                    FadeAndTeleport(0, 0, 0, 5172, 471, 489);
                    break;
                case 251:
                    Trace.Assert(originalScript == "npc_set(npc,4)");
                    ScriptDaemon.npc_set(npc, 4);
                    break;
                case 252:
                    Trace.Assert(originalScript == "npc_set(npc,5)");
                    ScriptDaemon.npc_set(npc, 5);
                    break;
                case 253:
                    Trace.Assert(originalScript == "npc_set(npc,6)");
                    ScriptDaemon.npc_set(npc, 6);
                    break;
                case 254:
                    Trace.Assert(originalScript == "npc_set(npc,7)");
                    ScriptDaemon.npc_set(npc, 7);
                    break;
                case 255:
                case 282:
                    Trace.Assert(originalScript == "npc_set(npc,10)");
                    ScriptDaemon.npc_set(npc, 10);
                    break;
                case 256:
                case 283:
                    Trace.Assert(originalScript == "npc_set(npc,11)");
                    ScriptDaemon.npc_set(npc, 11);
                    break;
                case 257:
                    Trace.Assert(originalScript == "npc_set(npc,14)");
                    ScriptDaemon.npc_set(npc, 14);
                    break;
                case 258:
                    Trace.Assert(originalScript == "npc_set(npc,15)");
                    ScriptDaemon.npc_set(npc, 15);
                    break;
                case 259:
                    Trace.Assert(originalScript == "npc_set(npc,16)");
                    ScriptDaemon.npc_set(npc, 16);
                    break;
                case 260:
                    Trace.Assert(originalScript == "npc_set(npc,17)");
                    ScriptDaemon.npc_set(npc, 17);
                    break;
                case 292:
                    Trace.Assert(originalScript == "npc_set(npc,12)");
                    ScriptDaemon.npc_set(npc, 12);
                    break;
                case 293:
                    Trace.Assert(originalScript == "npc_set(npc,13)");
                    ScriptDaemon.npc_set(npc, 13);
                    break;
                case 300:
                    Trace.Assert(originalScript == "game.global_vars[979] = 2");
                    SetGlobalVar(979, 2);
                    break;
                case 310:
                    Trace.Assert(originalScript == "game.global_vars[947] = 1");
                    SetGlobalVar(947, 1);
                    break;
                case 311:
                    Trace.Assert(originalScript == "game.fade_and_teleport(0,0,0,5170,476,483)");
                    FadeAndTeleport(0, 0, 0, 5170, 476, 483);
                    break;
                case 312:
                    Trace.Assert(originalScript == "game.fade_and_teleport(0,0,0,5135,480,477)");
                    FadeAndTeleport(0, 0, 0, 5135, 480, 477);
                    break;
                case 320:
                    Trace.Assert(originalScript == "game.global_vars[947] = 2; record_time_stamp('absalom_off_to_arrest')");
                    SetGlobalVar(947, 2);
                    ScriptDaemon.record_time_stamp("absalom_off_to_arrest");
                    ;
                    break;
                case 321:
                    Trace.Assert(originalScript == "switch_to_wilfrick( npc, pc, 980)");
                    switch_to_wilfrick(npc, pc, 980);
                    break;
                case 340:
                    Trace.Assert(originalScript == "game.global_flags[940] = 1");
                    SetGlobalFlag(940, true);
                    break;
                case 350:
                    Trace.Assert(originalScript == "game.global_flags[942] = 1");
                    SetGlobalFlag(942, true);
                    break;
                case 370:
                    Trace.Assert(originalScript == "game.global_vars[944] = 2");
                    SetGlobalVar(944, 2);
                    break;
                case 371:
                    Trace.Assert(originalScript == "game.quests[88].state = qs_completed");
                    SetQuestState(88, QuestState.Completed);
                    break;
                case 372:
                    Trace.Assert(originalScript == "game.quests[88].state = qs_completed; game.quests[67].state = qs_botched");
                    SetQuestState(88, QuestState.Completed);
                    SetQuestState(67, QuestState.Botched);
                    ;
                    break;
                case 380:
                    Trace.Assert(originalScript == "game.global_vars[944] = 1");
                    SetGlobalVar(944, 1);
                    break;
                case 381:
                    Trace.Assert(originalScript == "game.quests[87].state = qs_completed");
                    SetQuestState(87, QuestState.Completed);
                    break;
                case 382:
                    Trace.Assert(originalScript == "game.quests[87].state = qs_completed; game.quests[66].state = qs_botched");
                    SetQuestState(87, QuestState.Completed);
                    SetQuestState(66, QuestState.Botched);
                    ;
                    break;
                case 412:
                    Trace.Assert(originalScript == "npc_set(npc,8)");
                    ScriptDaemon.npc_set(npc, 8);
                    break;
                case 413:
                    Trace.Assert(originalScript == "npc_set(npc,9)");
                    ScriptDaemon.npc_set(npc, 9);
                    break;
                case 450:
                    Trace.Assert(originalScript == "game.global_vars[944] = 3");
                    SetGlobalVar(944, 3);
                    break;
                case 451:
                    Trace.Assert(originalScript == "run_off(npc,pc)");
                    run_off(npc, pc);
                    break;
                case 471:
                    Trace.Assert(originalScript == "npc_set(npc,19)");
                    ScriptDaemon.npc_set(npc, 19);
                    break;
                case 472:
                    Trace.Assert(originalScript == "npc_set(npc,20)");
                    ScriptDaemon.npc_set(npc, 20);
                    break;
                case 473:
                    Trace.Assert(originalScript == "npc_set(npc,21)");
                    ScriptDaemon.npc_set(npc, 21);
                    break;
                case 474:
                    Trace.Assert(originalScript == "npc_set(npc,22)");
                    ScriptDaemon.npc_set(npc, 22);
                    break;
                case 482:
                    Trace.Assert(originalScript == "npc_set(npc,23)");
                    ScriptDaemon.npc_set(npc, 23);
                    break;
                case 483:
                    Trace.Assert(originalScript == "npc_set(npc,24)");
                    ScriptDaemon.npc_set(npc, 24);
                    break;
                case 484:
                    Trace.Assert(originalScript == "npc_set(npc,25)");
                    ScriptDaemon.npc_set(npc, 25);
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
                case 13:
                case 23:
                case 42:
                case 51:
                case 52:
                case 462:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 11);
                    return true;
                case 111:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 9);
                    return true;
                case 282:
                case 292:
                case 412:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 15);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
