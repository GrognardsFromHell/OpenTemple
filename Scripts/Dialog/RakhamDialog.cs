
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
    [DialogScript(329)]
    public class RakhamDialog : Rakham, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
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
                    Trace.Assert(originalScript == "not npc_get(npc,1)");
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
                    Trace.Assert(originalScript == "game.quests[109].state == qs_accepted and not npc_get(npc,2)");
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
                    Trace.Assert(originalScript == "(game.quests[109].state == qs_mentioned or game.quests[109].state == qs_accepted) and not npc_get(npc,3)");
                    return (GetQuestState(109) == QuestState.Mentioned || GetQuestState(109) == QuestState.Accepted) && !ScriptDaemon.npc_get(npc, 3);
                case 14:
                case 24:
                case 34:
                case 44:
                case 64:
                case 74:
                case 84:
                case 114:
                    Trace.Assert(originalScript == "(game.quests[109].state == qs_mentioned or game.quests[109].state == qs_accepted) and not npc_get(npc,4)");
                    return (GetQuestState(109) == QuestState.Mentioned || GetQuestState(109) == QuestState.Accepted) && !ScriptDaemon.npc_get(npc, 4);
                case 121:
                case 161:
                case 171:
                    Trace.Assert(originalScript == "(game.quests[109].state == qs_mentioned or game.quests[109].state == qs_accepted) and not npc_get(npc,5)");
                    return (GetQuestState(109) == QuestState.Mentioned || GetQuestState(109) == QuestState.Accepted) && !ScriptDaemon.npc_get(npc, 5);
                case 122:
                case 132:
                case 152:
                case 162:
                case 172:
                    Trace.Assert(originalScript == "not npc_get(npc,6)");
                    return !ScriptDaemon.npc_get(npc, 6);
                case 123:
                case 133:
                case 143:
                case 153:
                case 173:
                    Trace.Assert(originalScript == "not npc_get(npc,7)");
                    return !ScriptDaemon.npc_get(npc, 7);
                case 124:
                case 134:
                case 144:
                case 154:
                case 164:
                    Trace.Assert(originalScript == "not npc_get(npc,8)");
                    return !ScriptDaemon.npc_get(npc, 8);
                case 125:
                    Trace.Assert(originalScript == "not npc_get(npc,9) and npc_get(npc,1) and npc_get(npc,2) and npc_get(npc,3) and npc_get(npc,4) and npc_get(npc,5) and npc_get(npc,6) and npc_get(npc,7) and npc_get(npc,8)");
                    return !ScriptDaemon.npc_get(npc, 9) && ScriptDaemon.npc_get(npc, 1) && ScriptDaemon.npc_get(npc, 2) && ScriptDaemon.npc_get(npc, 3) && ScriptDaemon.npc_get(npc, 4) && ScriptDaemon.npc_get(npc, 5) && ScriptDaemon.npc_get(npc, 6) && ScriptDaemon.npc_get(npc, 7) && ScriptDaemon.npc_get(npc, 8);
                case 141:
                    Trace.Assert(originalScript == "(game.quests[109].state == qs_mentioned or game.quests[109].state == qs_accepted) not npc_get(npc,5)");
                    throw new NotSupportedException("Conversion failed.");
                case 182:
                    Trace.Assert(originalScript == "not npc_get(npc,3)");
                    return !ScriptDaemon.npc_get(npc, 3);
                case 183:
                    Trace.Assert(originalScript == "not npc_get(npc,4)");
                    return !ScriptDaemon.npc_get(npc, 4);
                case 211:
                    Trace.Assert(originalScript == "game.global_vars[559] == 0 and game.global_flags[541] == 0");
                    return GetGlobalVar(559) == 0 && !GetGlobalFlag(541);
                case 212:
                    Trace.Assert(originalScript == "game.global_vars[559] == 1 and game.global_flags[541] == 0");
                    return GetGlobalVar(559) == 1 && !GetGlobalFlag(541);
                case 213:
                    Trace.Assert(originalScript == "game.global_vars[559] == 2 and game.global_flags[541] == 0");
                    return GetGlobalVar(559) == 2 && !GetGlobalFlag(541);
                case 214:
                    Trace.Assert(originalScript == "game.global_vars[559] == 0 and game.global_flags[541] == 1");
                    return GetGlobalVar(559) == 0 && GetGlobalFlag(541);
                case 222:
                case 231:
                case 321:
                case 331:
                case 341:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) >= 30");
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
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_sense_motive) <= 29");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) <= 29;
                case 241:
                    Trace.Assert(originalScript == "game.global_vars[540] == 1 and game.global_vars[541] == 1 and pc.skill_level_get(npc,skill_sense_motive) >= 30");
                    return GetGlobalVar(540) == 1 && GetGlobalVar(541) == 1 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 242:
                    Trace.Assert(originalScript == "game.global_vars[540] == 2 and game.global_vars[541] == 2 and pc.skill_level_get(npc,skill_sense_motive) >= 30");
                    return GetGlobalVar(540) == 2 && GetGlobalVar(541) == 2 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 243:
                    Trace.Assert(originalScript == "game.global_vars[540] == 3 and game.global_vars[541] == 3 and pc.skill_level_get(npc,skill_sense_motive) >= 30");
                    return GetGlobalVar(540) == 3 && GetGlobalVar(541) == 3 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 244:
                    Trace.Assert(originalScript == "game.global_vars[540] == 4 and game.global_vars[541] == 4 and pc.skill_level_get(npc,skill_sense_motive) >= 30");
                    return GetGlobalVar(540) == 4 && GetGlobalVar(541) == 4 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 251:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 19");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 19;
                case 252:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 19");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 19;
                case 253:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 19");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 19;
                case 254:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) <= 18");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) <= 18;
                case 255:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) <= 18");
                    return pc.GetSkillLevel(npc, SkillId.bluff) <= 18;
                case 256:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) <= 18");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) <= 18;
                case 261:
                    Trace.Assert(originalScript == "game.global_vars[540] == 1 and game.global_vars[541] == 1 and pc.skill_level_get(npc,skill_diplomacy) >= 20");
                    return GetGlobalVar(540) == 1 && GetGlobalVar(541) == 1 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 20;
                case 262:
                    Trace.Assert(originalScript == "game.global_vars[540] == 2 and game.global_vars[541] == 2 and pc.skill_level_get(npc,skill_diplomacy) >= 20");
                    return GetGlobalVar(540) == 2 && GetGlobalVar(541) == 2 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 20;
                case 263:
                    Trace.Assert(originalScript == "game.global_vars[540] == 3 and game.global_vars[541] == 3 and pc.skill_level_get(npc,skill_diplomacy) >= 20");
                    return GetGlobalVar(540) == 3 && GetGlobalVar(541) == 3 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 20;
                case 264:
                    Trace.Assert(originalScript == "game.global_vars[540] == 4 and game.global_vars[541] == 4 and pc.skill_level_get(npc,skill_diplomacy) >= 20");
                    return GetGlobalVar(540) == 4 && GetGlobalVar(541) == 4 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 20;
                case 265:
                    Trace.Assert(originalScript == "game.global_vars[540] == 1 and game.global_vars[541] == 1 and pc.skill_level_get(npc,skill_bluff) >= 20");
                    return GetGlobalVar(540) == 1 && GetGlobalVar(541) == 1 && pc.GetSkillLevel(npc, SkillId.bluff) >= 20;
                case 266:
                    Trace.Assert(originalScript == "game.global_vars[540] == 2 and game.global_vars[541] == 2 and pc.skill_level_get(npc,skill_bluff) >= 20");
                    return GetGlobalVar(540) == 2 && GetGlobalVar(541) == 2 && pc.GetSkillLevel(npc, SkillId.bluff) >= 20;
                case 267:
                    Trace.Assert(originalScript == "game.global_vars[540] == 3 and game.global_vars[541] == 3 and pc.skill_level_get(npc,skill_bluff) >= 20");
                    return GetGlobalVar(540) == 3 && GetGlobalVar(541) == 3 && pc.GetSkillLevel(npc, SkillId.bluff) >= 20;
                case 268:
                    Trace.Assert(originalScript == "game.global_vars[540] == 4 and game.global_vars[541] == 4 and pc.skill_level_get(npc,skill_bluff) >= 20");
                    return GetGlobalVar(540) == 4 && GetGlobalVar(541) == 4 && pc.GetSkillLevel(npc, SkillId.bluff) >= 20;
                case 269:
                    Trace.Assert(originalScript == "game.global_vars[540] == 1 and game.global_vars[541] == 1 and pc.skill_level_get(npc,skill_intimidate) >= 20");
                    return GetGlobalVar(540) == 1 && GetGlobalVar(541) == 1 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 20;
                case 270:
                    Trace.Assert(originalScript == "game.global_vars[540] == 2 and game.global_vars[541] == 2 and pc.skill_level_get(npc,skill_intimidate) >= 20");
                    return GetGlobalVar(540) == 2 && GetGlobalVar(541) == 2 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 20;
                case 271:
                    Trace.Assert(originalScript == "game.global_vars[540] == 3 and game.global_vars[541] == 3 and pc.skill_level_get(npc,skill_intimidate) >= 20");
                    return GetGlobalVar(540) == 3 && GetGlobalVar(541) == 3 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 20;
                case 272:
                    Trace.Assert(originalScript == "game.global_vars[540] == 4 and game.global_vars[541] == 4 and pc.skill_level_get(npc,skill_intimidate) >= 20");
                    return GetGlobalVar(540) == 4 && GetGlobalVar(541) == 4 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 20;
                case 273:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) <= 19");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) <= 19;
                case 274:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) <= 19");
                    return pc.GetSkillLevel(npc, SkillId.bluff) <= 19;
                case 275:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) <= 19");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) <= 19;
                case 281:
                case 291:
                case 301:
                case 311:
                    Trace.Assert(originalScript == "game.global_vars[542] == 1 and pc.skill_level_get(npc,skill_sense_motive) >= 30");
                    return GetGlobalVar(542) == 1 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 282:
                case 292:
                case 302:
                case 312:
                    Trace.Assert(originalScript == "game.global_vars[542] == 2 and pc.skill_level_get(npc,skill_sense_motive) >= 30");
                    return GetGlobalVar(542) == 2 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 283:
                case 293:
                case 303:
                case 313:
                    Trace.Assert(originalScript == "game.global_vars[542] == 3 and pc.skill_level_get(npc,skill_sense_motive) >= 30");
                    return GetGlobalVar(542) == 3 && pc.GetSkillLevel(npc, SkillId.sense_motive) >= 30;
                case 351:
                    Trace.Assert(originalScript == "game.global_vars[542] == 1 and pc.skill_level_get(npc,skill_diplomacy) >= 21");
                    return GetGlobalVar(542) == 1 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 21;
                case 352:
                    Trace.Assert(originalScript == "game.global_vars[542] == 2 and pc.skill_level_get(npc,skill_diplomacy) >= 21");
                    return GetGlobalVar(542) == 2 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 21;
                case 353:
                    Trace.Assert(originalScript == "game.global_vars[542] == 3 and pc.skill_level_get(npc,skill_diplomacy) >= 21");
                    return GetGlobalVar(542) == 3 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 21;
                case 354:
                    Trace.Assert(originalScript == "game.global_vars[542] == 1 and pc.skill_level_get(npc,skill_bluff) >= 21");
                    return GetGlobalVar(542) == 1 && pc.GetSkillLevel(npc, SkillId.bluff) >= 21;
                case 355:
                    Trace.Assert(originalScript == "game.global_vars[542] == 2 and pc.skill_level_get(npc,skill_bluff) >= 21");
                    return GetGlobalVar(542) == 2 && pc.GetSkillLevel(npc, SkillId.bluff) >= 21;
                case 356:
                    Trace.Assert(originalScript == "game.global_vars[542] == 3 and pc.skill_level_get(npc,skill_bluff) >= 21");
                    return GetGlobalVar(542) == 3 && pc.GetSkillLevel(npc, SkillId.bluff) >= 21;
                case 357:
                    Trace.Assert(originalScript == "game.global_vars[542] == 1 and pc.skill_level_get(npc,skill_intimidate) >= 21");
                    return GetGlobalVar(542) == 1 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 21;
                case 358:
                    Trace.Assert(originalScript == "game.global_vars[542] == 2 and pc.skill_level_get(npc,skill_intimidate) >= 21");
                    return GetGlobalVar(542) == 2 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 21;
                case 359:
                    Trace.Assert(originalScript == "game.global_vars[542] == 3 and pc.skill_level_get(npc,skill_intimidate) >= 21");
                    return GetGlobalVar(542) == 3 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 21;
                case 360:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) <= 20");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) <= 20;
                case 361:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) <= 20");
                    return pc.GetSkillLevel(npc, SkillId.bluff) <= 20;
                case 362:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) <= 20");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) <= 20;
                case 372:
                case 382:
                case 392:
                case 422:
                    Trace.Assert(originalScript == "not anyone( pc.group_list(), \"has_follower\", 8766 ) and game.global_vars[545] == 3 and pc.follower_atmax() == 0");
                    return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8766)) && GetGlobalVar(545) == 3 && !pc.HasMaxFollowers();
                case 431:
                    Trace.Assert(originalScript == "game.global_vars[542] == 3 and not npc_get(npc,10)");
                    return GetGlobalVar(542) == 3 && !ScriptDaemon.npc_get(npc, 10);
                case 432:
                    Trace.Assert(originalScript == "game.global_vars[542] == 2 and not npc_get(npc,10)");
                    return GetGlobalVar(542) == 2 && !ScriptDaemon.npc_get(npc, 10);
                case 433:
                    Trace.Assert(originalScript == "game.global_vars[542] == 1 and not npc_get(npc,10)");
                    return GetGlobalVar(542) == 1 && !ScriptDaemon.npc_get(npc, 10);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 10:
                case 400:
                    Trace.Assert(originalScript == "increment_var_536(npc,pc); gen_panathaes_loc(npc,pc); pick_kidnapper(npc,pc); gen_kids_loc(npc,pc)");
                    increment_var_536(npc, pc);
                    gen_panathaes_loc(npc, pc);
                    pick_kidnapper(npc, pc);
                    gen_kids_loc(npc, pc);
                    ;
                    break;
                case 11:
                    Trace.Assert(originalScript == "npc_set(npc,1); check_for_locket(npc,pc)");
                    ScriptDaemon.npc_set(npc, 1);
                    check_for_locket(npc, pc);
                    ;
                    break;
                case 12:
                    Trace.Assert(originalScript == "npc_set(npc,2); check_for_locket(npc,pc)");
                    ScriptDaemon.npc_set(npc, 2);
                    check_for_locket(npc, pc);
                    ;
                    break;
                case 13:
                    Trace.Assert(originalScript == "npc_set(npc,3); check_for_locket(npc,pc)");
                    ScriptDaemon.npc_set(npc, 3);
                    check_for_locket(npc, pc);
                    ;
                    break;
                case 14:
                    Trace.Assert(originalScript == "npc_set(npc,4); check_for_locket(npc,pc)");
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
                    Trace.Assert(originalScript == "npc_set(npc,2)");
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
                    Trace.Assert(originalScript == "npc_set(npc,3)");
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
                    Trace.Assert(originalScript == "npc_set(npc,4)");
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
                    Trace.Assert(originalScript == "npc_set(npc,1)");
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 121:
                case 141:
                case 161:
                case 171:
                    Trace.Assert(originalScript == "npc_set(npc,5)");
                    ScriptDaemon.npc_set(npc, 5);
                    break;
                case 122:
                case 132:
                case 152:
                case 162:
                case 172:
                    Trace.Assert(originalScript == "npc_set(npc,6)");
                    ScriptDaemon.npc_set(npc, 6);
                    break;
                case 123:
                case 133:
                case 143:
                case 153:
                case 173:
                    Trace.Assert(originalScript == "npc_set(npc,7)");
                    ScriptDaemon.npc_set(npc, 7);
                    break;
                case 124:
                case 134:
                case 144:
                case 154:
                case 164:
                    Trace.Assert(originalScript == "npc_set(npc,8)");
                    ScriptDaemon.npc_set(npc, 8);
                    break;
                case 125:
                    Trace.Assert(originalScript == "npc_set(npc,9)");
                    ScriptDaemon.npc_set(npc, 9);
                    break;
                case 311:
                    Trace.Assert(originalScript == "game.global_vars[559] = 1");
                    SetGlobalVar(559, 1);
                    break;
                case 312:
                    Trace.Assert(originalScript == "game.global_vars[559] = 2");
                    SetGlobalVar(559, 2);
                    break;
                case 313:
                    Trace.Assert(originalScript == "game.global_flags[541] = 1");
                    SetGlobalFlag(541, true);
                    break;
                case 320:
                    Trace.Assert(originalScript == "increment_var_544(npc,pc); increment_var_556(npc,pc); check_evidence_rep_bor(npc,pc)");
                    increment_var_544(npc, pc);
                    increment_var_556(npc, pc);
                    check_evidence_rep_bor(npc, pc);
                    ;
                    break;
                case 330:
                    Trace.Assert(originalScript == "increment_var_543(npc,pc); increment_var_555(npc,pc); check_evidence_rep_pan(npc,pc)");
                    increment_var_543(npc, pc);
                    increment_var_555(npc, pc);
                    check_evidence_rep_pan(npc, pc);
                    ;
                    break;
                case 340:
                    Trace.Assert(originalScript == "increment_var_545(npc,pc); check_evidence_rep_rak(npc,pc)");
                    increment_var_545(npc, pc);
                    check_evidence_rep_rak(npc, pc);
                    ;
                    break;
                case 372:
                case 382:
                case 392:
                case 422:
                    Trace.Assert(originalScript == "pc.follower_add( npc )");
                    pc.AddFollower(npc);
                    break;
                case 410:
                    Trace.Assert(originalScript == "game.global_flags[536] = 1");
                    SetGlobalFlag(536, true);
                    break;
                case 431:
                case 432:
                case 433:
                    Trace.Assert(originalScript == "npc_set(npc,10)");
                    ScriptDaemon.npc_set(npc, 10);
                    break;
                case 440:
                case 480:
                    Trace.Assert(originalScript == "game.global_vars[558] = 2");
                    SetGlobalVar(558, 2);
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
