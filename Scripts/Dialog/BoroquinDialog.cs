
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
    [DialogScript(330)]
    public class BoroquinDialog : Boroquin, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 31:
                case 41:
                case 51:
                case 71:
                case 81:
                case 91:
                case 101:
                case 111:
                case 181:
                case 221:
                    originalScript = "not npc_get(npc,1)";
                    return !ScriptDaemon.npc_get(npc, 1);
                case 12:
                case 22:
                case 42:
                case 52:
                case 62:
                case 82:
                case 92:
                case 102:
                case 112:
                    originalScript = "game.quests[109].state == qs_accepted and not npc_get(npc,2)";
                    return GetQuestState(109) == QuestState.Accepted && !ScriptDaemon.npc_get(npc, 2);
                case 13:
                case 23:
                case 33:
                case 53:
                case 63:
                case 73:
                case 93:
                case 103:
                case 113:
                    originalScript = "(game.quests[109].state == qs_mentioned or game.quests[109].state == qs_accepted) and not npc_get(npc,3)";
                    return (GetQuestState(109) == QuestState.Mentioned || GetQuestState(109) == QuestState.Accepted) && !ScriptDaemon.npc_get(npc, 3);
                case 14:
                case 24:
                case 34:
                case 44:
                case 64:
                case 74:
                case 84:
                case 114:
                    originalScript = "(game.quests[109].state == qs_mentioned or game.quests[109].state == qs_accepted) and not npc_get(npc,4)";
                    return (GetQuestState(109) == QuestState.Mentioned || GetQuestState(109) == QuestState.Accepted) && !ScriptDaemon.npc_get(npc, 4);
                case 121:
                case 141:
                case 161:
                case 171:
                    originalScript = "(game.quests[109].state == qs_mentioned or game.quests[109].state == qs_accepted) and not npc_get(npc,5)";
                    return (GetQuestState(109) == QuestState.Mentioned || GetQuestState(109) == QuestState.Accepted) && !ScriptDaemon.npc_get(npc, 5);
                case 122:
                case 132:
                case 152:
                case 162:
                case 172:
                    originalScript = "not npc_get(npc,6)";
                    return !ScriptDaemon.npc_get(npc, 6);
                case 123:
                case 133:
                case 143:
                case 153:
                case 173:
                    originalScript = "not npc_get(npc,7)";
                    return !ScriptDaemon.npc_get(npc, 7);
                case 124:
                case 134:
                case 144:
                case 154:
                case 164:
                    originalScript = "not npc_get(npc,8)";
                    return !ScriptDaemon.npc_get(npc, 8);
                case 125:
                    originalScript = "not npc_get(npc,9) and npc_get(npc,1) and npc_get(npc,2) and npc_get(npc,3) and npc_get(npc,4) and npc_get(npc,5) and npc_get(npc,6) and npc_get(npc,7) and npc_get(npc,8)";
                    return !ScriptDaemon.npc_get(npc, 9) && ScriptDaemon.npc_get(npc, 1) && ScriptDaemon.npc_get(npc, 2) && ScriptDaemon.npc_get(npc, 3) && ScriptDaemon.npc_get(npc, 4) && ScriptDaemon.npc_get(npc, 5) && ScriptDaemon.npc_get(npc, 6) && ScriptDaemon.npc_get(npc, 7) && ScriptDaemon.npc_get(npc, 8);
                case 182:
                    originalScript = "not npc_get(npc,3)";
                    return !ScriptDaemon.npc_get(npc, 3);
                case 183:
                    originalScript = "not npc_get(npc,4)";
                    return !ScriptDaemon.npc_get(npc, 4);
                case 211:
                    originalScript = "game.global_vars[560] == 0 and game.global_flags[542] == 0";
                    return GetGlobalVar(560) == 0 && !GetGlobalFlag(542);
                case 212:
                    originalScript = "game.global_vars[560] == 1 and game.global_flags[542] == 0";
                    return GetGlobalVar(560) == 1 && !GetGlobalFlag(542);
                case 213:
                    originalScript = "game.global_vars[560] == 2 and game.global_flags[542] == 0";
                    return GetGlobalVar(560) == 2 && !GetGlobalFlag(542);
                case 214:
                    originalScript = "game.global_vars[560] == 0 and game.global_flags[542] == 1";
                    return GetGlobalVar(560) == 0 && GetGlobalFlag(542);
                case 222:
                case 231:
                case 321:
                case 331:
                case 341:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 30";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 223:
                case 232:
                case 284:
                case 294:
                case 304:
                case 314:
                case 322:
                case 332:
                case 342:
                    originalScript = "pc.skill_level_get(npc,skill_sense_motive) <= 29";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) <= 29;
                case 241:
                    originalScript = "game.global_vars[540] == 1 and game.global_vars[541] == 1 and pc.skill_level_get(npc,skill_sense_motive) >= 30";
                    return GetGlobalVar(540) == 1 && GetGlobalVar(541) == 1 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 242:
                    originalScript = "game.global_vars[540] == 2 and game.global_vars[541] == 2 and pc.skill_level_get(npc,skill_sense_motive) >= 30";
                    return GetGlobalVar(540) == 2 && GetGlobalVar(541) == 2 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 243:
                    originalScript = "game.global_vars[540] == 3 and game.global_vars[541] == 3 and pc.skill_level_get(npc,skill_sense_motive) >= 30";
                    return GetGlobalVar(540) == 3 && GetGlobalVar(541) == 3 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 244:
                    originalScript = "game.global_vars[540] == 4 and game.global_vars[541] == 4 and pc.skill_level_get(npc,skill_sense_motive) >= 30";
                    return GetGlobalVar(540) == 4 && GetGlobalVar(541) == 4 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 251:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 19";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 19;
                case 252:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 19";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 19;
                case 253:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 19";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 19;
                case 254:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) <= 18";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) <= 18;
                case 255:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) <= 18";
                    return pc.GetSkillLevel(npc, SkillId.bluff) <= 18;
                case 256:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) <= 18";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) <= 18;
                case 261:
                    originalScript = "game.global_vars[540] == 1 and game.global_vars[541] == 1 and pc.skill_level_get(npc,skill_diplomacy) >= 20";
                    return GetGlobalVar(540) == 1 && GetGlobalVar(541) == 1 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 20;
                case 262:
                    originalScript = "game.global_vars[540] == 2 and game.global_vars[541] == 2 and pc.skill_level_get(npc,skill_diplomacy) >= 20";
                    return GetGlobalVar(540) == 2 && GetGlobalVar(541) == 2 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 20;
                case 263:
                    originalScript = "game.global_vars[540] == 3 and game.global_vars[541] == 3 and pc.skill_level_get(npc,skill_diplomacy) >= 20";
                    return GetGlobalVar(540) == 3 && GetGlobalVar(541) == 3 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 20;
                case 264:
                    originalScript = "game.global_vars[540] == 4 and game.global_vars[541] == 4 and pc.skill_level_get(npc,skill_diplomacy) >= 20";
                    return GetGlobalVar(540) == 4 && GetGlobalVar(541) == 4 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 20;
                case 265:
                    originalScript = "game.global_vars[540] == 1 and game.global_vars[541] == 1 and pc.skill_level_get(npc,skill_bluff) >= 20";
                    return GetGlobalVar(540) == 1 && GetGlobalVar(541) == 1 && pc.GetSkillLevel(npc, SkillId.bluff) >= 20;
                case 266:
                    originalScript = "game.global_vars[540] == 2 and game.global_vars[541] == 2 and pc.skill_level_get(npc,skill_bluff) >= 20";
                    return GetGlobalVar(540) == 2 && GetGlobalVar(541) == 2 && pc.GetSkillLevel(npc, SkillId.bluff) >= 20;
                case 267:
                    originalScript = "game.global_vars[540] == 3 and game.global_vars[541] == 3 and pc.skill_level_get(npc,skill_bluff) >= 20";
                    return GetGlobalVar(540) == 3 && GetGlobalVar(541) == 3 && pc.GetSkillLevel(npc, SkillId.bluff) >= 20;
                case 268:
                    originalScript = "game.global_vars[540] == 4 and game.global_vars[541] == 4 and pc.skill_level_get(npc,skill_bluff) >= 20";
                    return GetGlobalVar(540) == 4 && GetGlobalVar(541) == 4 && pc.GetSkillLevel(npc, SkillId.bluff) >= 20;
                case 269:
                    originalScript = "game.global_vars[540] == 1 and game.global_vars[541] == 1 and pc.skill_level_get(npc,skill_intimidate) >= 20";
                    return GetGlobalVar(540) == 1 && GetGlobalVar(541) == 1 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 20;
                case 270:
                    originalScript = "game.global_vars[540] == 2 and game.global_vars[541] == 2 and pc.skill_level_get(npc,skill_intimidate) >= 20";
                    return GetGlobalVar(540) == 2 && GetGlobalVar(541) == 2 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 20;
                case 271:
                    originalScript = "game.global_vars[540] == 3 and game.global_vars[541] == 3 and pc.skill_level_get(npc,skill_intimidate) >= 20";
                    return GetGlobalVar(540) == 3 && GetGlobalVar(541) == 3 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 20;
                case 272:
                    originalScript = "game.global_vars[540] == 4 and game.global_vars[541] == 4 and pc.skill_level_get(npc,skill_intimidate) >= 20";
                    return GetGlobalVar(540) == 4 && GetGlobalVar(541) == 4 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 20;
                case 273:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) <= 19";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) <= 19;
                case 274:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) <= 19";
                    return pc.GetSkillLevel(npc, SkillId.bluff) <= 19;
                case 275:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) <= 19";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) <= 19;
                case 281:
                case 291:
                case 301:
                case 311:
                    originalScript = "game.global_vars[542] == 1 and pc.skill_level_get(npc,skill_sense_motive) >= 30";
                    return GetGlobalVar(542) == 1 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 282:
                case 292:
                case 302:
                case 312:
                    originalScript = "game.global_vars[542] == 2 and pc.skill_level_get(npc,skill_sense_motive) >= 30";
                    return GetGlobalVar(542) == 2 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 283:
                case 293:
                case 303:
                case 313:
                    originalScript = "game.global_vars[542] == 3 and pc.skill_level_get(npc,skill_sense_motive) >= 30";
                    return GetGlobalVar(542) == 3 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 351:
                    originalScript = "game.global_vars[542] == 1 and pc.skill_level_get(npc,skill_diplomacy) >= 21";
                    return GetGlobalVar(542) == 1 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 21;
                case 352:
                    originalScript = "game.global_vars[542] == 2 and pc.skill_level_get(npc,skill_diplomacy) >= 21";
                    return GetGlobalVar(542) == 2 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 21;
                case 353:
                    originalScript = "game.global_vars[542] == 3 and pc.skill_level_get(npc,skill_diplomacy) >= 21";
                    return GetGlobalVar(542) == 3 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 21;
                case 354:
                    originalScript = "game.global_vars[542] == 1 and pc.skill_level_get(npc,skill_bluff) >= 21";
                    return GetGlobalVar(542) == 1 && pc.GetSkillLevel(npc, SkillId.bluff) >= 21;
                case 355:
                    originalScript = "game.global_vars[542] == 2 and pc.skill_level_get(npc,skill_bluff) >= 21";
                    return GetGlobalVar(542) == 2 && pc.GetSkillLevel(npc, SkillId.bluff) >= 21;
                case 356:
                    originalScript = "game.global_vars[542] == 3 and pc.skill_level_get(npc,skill_bluff) >= 21";
                    return GetGlobalVar(542) == 3 && pc.GetSkillLevel(npc, SkillId.bluff) >= 21;
                case 357:
                    originalScript = "game.global_vars[542] == 1 and pc.skill_level_get(npc,skill_intimidate) >= 21";
                    return GetGlobalVar(542) == 1 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 21;
                case 358:
                    originalScript = "game.global_vars[542] == 2 and pc.skill_level_get(npc,skill_intimidate) >= 21";
                    return GetGlobalVar(542) == 2 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 21;
                case 359:
                    originalScript = "game.global_vars[542] == 3 and pc.skill_level_get(npc,skill_intimidate) >= 21";
                    return GetGlobalVar(542) == 3 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 21;
                case 360:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) <= 20";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) <= 20;
                case 361:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) <= 20";
                    return pc.GetSkillLevel(npc, SkillId.bluff) <= 20;
                case 362:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) <= 20";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) <= 20;
                case 372:
                case 382:
                case 392:
                case 422:
                    originalScript = "not anyone( pc.group_list(), \"has_follower\", 8767 ) and game.global_vars[544] == 3 and pc.follower_atmax() == 0";
                    return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8767)) && GetGlobalVar(544) == 3 && !pc.HasMaxFollowers();
                case 401:
                    originalScript = "not npc_get(npc,11)";
                    return !ScriptDaemon.npc_get(npc, 11);
                case 402:
                    originalScript = "npc_get(npc,11)";
                    return ScriptDaemon.npc_get(npc, 11);
                case 431:
                    originalScript = "game.global_vars[542] == 1 and not npc_get(npc,10)";
                    return GetGlobalVar(542) == 1 && !ScriptDaemon.npc_get(npc, 10);
                case 432:
                    originalScript = "game.global_vars[542] == 2 and not npc_get(npc,10)";
                    return GetGlobalVar(542) == 2 && !ScriptDaemon.npc_get(npc, 10);
                case 433:
                    originalScript = "game.global_vars[542] == 3 and not npc_get(npc,10)";
                    return GetGlobalVar(542) == 3 && !ScriptDaemon.npc_get(npc, 10);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 10:
                    originalScript = "increment_var_536(npc,pc); gen_panathaes_loc(npc,pc); pick_kidnapper(npc,pc); gen_kids_loc(npc,pc)";
                    increment_var_536(npc, pc);
                    gen_panathaes_loc(npc, pc);
                    pick_kidnapper(npc, pc);
                    gen_kids_loc(npc, pc);
                    ;
                    break;
                case 11:
                    originalScript = "npc_set(npc,1); check_for_locket(npc,pc)";
                    ScriptDaemon.npc_set(npc, 1);
                    check_for_locket(npc, pc);
                    ;
                    break;
                case 12:
                    originalScript = "npc_set(npc,2); check_for_locket(npc,pc)";
                    ScriptDaemon.npc_set(npc, 2);
                    check_for_locket(npc, pc);
                    ;
                    break;
                case 13:
                    originalScript = "npc_set(npc,3); check_for_locket(npc,pc)";
                    ScriptDaemon.npc_set(npc, 3);
                    check_for_locket(npc, pc);
                    ;
                    break;
                case 14:
                    originalScript = "npc_set(npc,4); check_for_locket(npc,pc)";
                    ScriptDaemon.npc_set(npc, 4);
                    check_for_locket(npc, pc);
                    ;
                    break;
                case 22:
                case 42:
                case 52:
                case 62:
                case 82:
                case 92:
                case 102:
                case 112:
                    originalScript = "npc_set(npc,2)";
                    ScriptDaemon.npc_set(npc, 2);
                    break;
                case 23:
                case 33:
                case 53:
                case 63:
                case 73:
                case 93:
                case 103:
                case 113:
                case 182:
                    originalScript = "npc_set(npc,3)";
                    ScriptDaemon.npc_set(npc, 3);
                    break;
                case 24:
                case 34:
                case 44:
                case 64:
                case 74:
                case 84:
                case 114:
                case 183:
                    originalScript = "npc_set(npc,4)";
                    ScriptDaemon.npc_set(npc, 4);
                    break;
                case 31:
                case 41:
                case 51:
                case 71:
                case 81:
                case 91:
                case 101:
                case 111:
                case 181:
                case 221:
                    originalScript = "npc_set(npc,1)";
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 121:
                case 141:
                case 161:
                case 171:
                    originalScript = "npc_set(npc,5)";
                    ScriptDaemon.npc_set(npc, 5);
                    break;
                case 122:
                case 132:
                case 152:
                case 162:
                case 172:
                    originalScript = "npc_set(npc,6)";
                    ScriptDaemon.npc_set(npc, 6);
                    break;
                case 123:
                case 133:
                case 143:
                case 153:
                case 173:
                    originalScript = "npc_set(npc,7)";
                    ScriptDaemon.npc_set(npc, 7);
                    break;
                case 124:
                case 134:
                case 144:
                case 154:
                case 164:
                    originalScript = "npc_set(npc,8)";
                    ScriptDaemon.npc_set(npc, 8);
                    break;
                case 125:
                    originalScript = "npc_set(npc,9)";
                    ScriptDaemon.npc_set(npc, 9);
                    break;
                case 311:
                    originalScript = "game.global_flags[542] = 1";
                    SetGlobalFlag(542, true);
                    break;
                case 312:
                    originalScript = "game.global_vars[560] = 2";
                    SetGlobalVar(560, 2);
                    break;
                case 313:
                    originalScript = "game.global_vars[560] = 1";
                    SetGlobalVar(560, 1);
                    break;
                case 320:
                    originalScript = "increment_var_544(npc,pc); check_evidence_rep_bor(npc,pc)";
                    increment_var_544(npc, pc);
                    check_evidence_rep_bor(npc, pc);
                    ;
                    break;
                case 330:
                    originalScript = "increment_var_543(npc,pc); increment_var_555(npc,pc); check_evidence_rep_pan(npc,pc)";
                    increment_var_543(npc, pc);
                    increment_var_555(npc, pc);
                    check_evidence_rep_pan(npc, pc);
                    ;
                    break;
                case 340:
                    originalScript = "increment_var_545(npc,pc); increment_var_557(npc,pc); check_evidence_rep_rak(npc,pc)";
                    increment_var_545(npc, pc);
                    increment_var_557(npc, pc);
                    check_evidence_rep_rak(npc, pc);
                    ;
                    break;
                case 372:
                case 382:
                case 392:
                case 422:
                    originalScript = "pc.follower_add( npc )";
                    pc.AddFollower(npc);
                    break;
                case 401:
                    originalScript = "npc_set(npc,11)";
                    ScriptDaemon.npc_set(npc, 11);
                    break;
                case 410:
                    originalScript = "game.global_flags[535] = 1";
                    SetGlobalFlag(535, true);
                    break;
                case 431:
                case 432:
                case 433:
                    originalScript = "npc_set(npc,10)";
                    ScriptDaemon.npc_set(npc, 10);
                    break;
                case 440:
                    originalScript = "game.global_vars[558] = 1";
                    SetGlobalVar(558, 1);
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
                case 222:
                case 231:
                case 241:
                case 242:
                case 243:
                case 244:
                case 281:
                case 282:
                case 283:
                case 291:
                case 292:
                case 293:
                case 301:
                case 302:
                case 303:
                case 311:
                case 312:
                case 313:
                case 321:
                case 331:
                case 341:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 30);
                    return true;
                case 251:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 19);
                    return true;
                case 252:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 19);
                    return true;
                case 253:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 19);
                    return true;
                case 261:
                case 262:
                case 263:
                case 264:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 20);
                    return true;
                case 265:
                case 266:
                case 267:
                case 268:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 20);
                    return true;
                case 269:
                case 270:
                case 271:
                case 272:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 20);
                    return true;
                case 351:
                case 352:
                case 353:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 21);
                    return true;
                case 354:
                case 355:
                case 356:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 21);
                    return true;
                case 357:
                case 358:
                case 359:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 21);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
