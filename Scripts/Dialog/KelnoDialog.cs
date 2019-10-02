
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
    [DialogScript(124)]
    public class KelnoDialog : Kelno, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 14:
                    Trace.Assert(originalScript == "game.quests[46].state == qs_unknown");
                    return GetQuestState(46) == QuestState.Unknown;
                case 12:
                case 13:
                case 15:
                case 16:
                    Trace.Assert(originalScript == "game.quests[46].state != qs_unknown");
                    return GetQuestState(46) != QuestState.Unknown;
                case 41:
                case 42:
                    Trace.Assert(originalScript == "game.quests[46].state == qs_unknown and game.global_flags[105] == 0");
                    return GetQuestState(46) == QuestState.Unknown && !GetGlobalFlag(105);
                case 43:
                case 44:
                    Trace.Assert(originalScript == "game.quests[52].state == qs_unknown and game.global_flags[107] == 0");
                    return GetQuestState(52) == QuestState.Unknown && !GetGlobalFlag(107);
                case 61:
                case 62:
                case 91:
                case 92:
                    Trace.Assert(originalScript == "(   get_v(454) & (2**1) == 0   )");
                    return ((ScriptDaemon.get_v(454) & (0x2)) == 0);
                case 93:
                case 94:
                    Trace.Assert(originalScript == "(   get_v(454) & (2**1) != 0   )");
                    return ((ScriptDaemon.get_v(454) & (0x2)) != 0);
                case 161:
                case 162:
                case 1601:
                case 1602:
                    Trace.Assert(originalScript == "game.global_flags[105] == 0");
                    return !GetGlobalFlag(105);
                case 163:
                case 1603:
                    Trace.Assert(originalScript == "game.global_flags[105] == 1");
                    return GetGlobalFlag(105);
                case 164:
                case 165:
                case 1604:
                case 1605:
                    Trace.Assert(originalScript == "game.global_flags[108] == 1 and game.global_vars[11] < 5");
                    return GetGlobalFlag(108) && GetGlobalVar(11) < 5;
                case 171:
                case 172:
                    Trace.Assert(originalScript == "get_v(454) & (2**2) == 0");
                    return (ScriptDaemon.get_v(454) & (0x4)) == 0;
                case 173:
                    Trace.Assert(originalScript == "(   get_v(454) & (2**2) != 0   ) and should_open_sw_doors() == 0");
                    return ((ScriptDaemon.get_v(454) & (0x4)) != 0) && should_open_sw_doors() == 0;
                case 174:
                    Trace.Assert(originalScript == "(  get_v(454) & (2**2) != 0  ) and should_open_sw_doors() == 0");
                    return ((ScriptDaemon.get_v(454) & (0x4)) != 0) && should_open_sw_doors() == 0;
                case 175:
                    Trace.Assert(originalScript == "(   get_v(454) & (2**2) != 0   ) and should_open_sw_doors() == 1");
                    return ((ScriptDaemon.get_v(454) & (0x4)) != 0) && should_open_sw_doors() == 1;
                case 176:
                    Trace.Assert(originalScript == "(  get_v(454) & (2**2) != 0  ) and should_open_sw_doors() == 1");
                    return ((ScriptDaemon.get_v(454) & (0x4)) != 0) && should_open_sw_doors() == 1;
                case 181:
                case 185:
                case 1201:
                case 1205:
                    Trace.Assert(originalScript == "game.global_vars[11] > 0 and game.global_vars[11] < 5");
                    return GetGlobalVar(11) > 0 && GetGlobalVar(11) < 5;
                case 182:
                case 186:
                case 1202:
                case 1206:
                    Trace.Assert(originalScript == "game.global_vars[11] == 5");
                    return GetGlobalVar(11) == 5;
                case 231:
                case 232:
                    Trace.Assert(originalScript == "game.global_flags[104] == 0");
                    return !GetGlobalFlag(104);
                case 233:
                    Trace.Assert(originalScript == "game.global_flags[104] == 1");
                    return GetGlobalFlag(104);
                case 281:
                case 282:
                    Trace.Assert(originalScript == "(game.quests[49].state == qs_mentioned or game.quests[49].state == qs_accepted) and game.global_vars[11] == 5");
                    return (GetQuestState(49) == QuestState.Mentioned || GetQuestState(49) == QuestState.Accepted) && GetGlobalVar(11) == 5;
                case 283:
                case 284:
                case 285:
                case 286:
                    Trace.Assert(originalScript == "game.quests[49].state == qs_mentioned");
                    return GetQuestState(49) == QuestState.Mentioned;
                case 287:
                case 288:
                    Trace.Assert(originalScript == "game.quests[49].state == qs_accepted and game.global_flags[108] == 1 and game.global_vars[11] < 5");
                    return GetQuestState(49) == QuestState.Accepted && GetGlobalFlag(108) && GetGlobalVar(11) < 5;
                case 289:
                case 290:
                    Trace.Assert(originalScript == "game.quests[50].state == qs_mentioned");
                    return GetQuestState(50) == QuestState.Mentioned;
                case 291:
                case 292:
                    Trace.Assert(originalScript == "game.quests[50].state == qs_accepted and game.global_flags[109] == 1");
                    return GetQuestState(50) == QuestState.Accepted && GetGlobalFlag(109);
                case 293:
                case 294:
                case 295:
                case 296:
                    Trace.Assert(originalScript == "game.quests[51].state == qs_mentioned");
                    return GetQuestState(51) == QuestState.Mentioned;
                case 297:
                case 298:
                    Trace.Assert(originalScript == "game.quests[51].state == qs_accepted and game.global_flags[105] == 1 and game.global_flags[107] == 1");
                    return GetQuestState(51) == QuestState.Accepted && GetGlobalFlag(105) && GetGlobalFlag(107);
                case 299:
                case 300:
                    Trace.Assert(originalScript == "game.quests[51].state == qs_accepted and game.global_flags[105] == 1 and game.global_flags[107] == 0");
                    return GetQuestState(51) == QuestState.Accepted && GetGlobalFlag(105) && !GetGlobalFlag(107);
                case 301:
                case 302:
                    Trace.Assert(originalScript == "game.quests[51].state == qs_accepted and game.global_flags[105] == 0 and game.global_flags[107] == 1");
                    return GetQuestState(51) == QuestState.Accepted && !GetGlobalFlag(105) && GetGlobalFlag(107);
                case 303:
                case 304:
                    Trace.Assert(originalScript == "game.quests[51].state == qs_completed");
                    return GetQuestState(51) == QuestState.Completed;
                case 305:
                case 306:
                    Trace.Assert(originalScript == "get_f('realized_grurz_new_situation') and (game.quests[49].state == qs_accepted or game.quests[49].state == qs_mentioned)");
                    return ScriptDaemon.get_f("realized_grurz_new_situation") && (GetQuestState(49) == QuestState.Accepted || GetQuestState(49) == QuestState.Mentioned);
                case 341:
                case 346:
                    Trace.Assert(originalScript == "game.global_flags[105] == 1 and game.global_flags[107] == 1");
                    return GetGlobalFlag(105) && GetGlobalFlag(107);
                case 342:
                case 347:
                    Trace.Assert(originalScript == "game.global_flags[105] == 1 and game.global_flags[107] == 0");
                    return GetGlobalFlag(105) && !GetGlobalFlag(107);
                case 343:
                case 348:
                    Trace.Assert(originalScript == "game.global_flags[105] == 0 and game.global_flags[107] == 1");
                    return !GetGlobalFlag(105) && GetGlobalFlag(107);
                case 344:
                case 345:
                case 349:
                case 350:
                    Trace.Assert(originalScript == "game.global_flags[105] == 0 and game.global_flags[107] == 0");
                    return !GetGlobalFlag(105) && !GetGlobalFlag(107);
                case 413:
                case 414:
                    Trace.Assert(originalScript == "game.global_vars[11] > 0");
                    return GetGlobalVar(11) > 0;
                case 415:
                case 416:
                    Trace.Assert(originalScript == "game.global_vars[11] == 0");
                    return GetGlobalVar(11) == 0;
                case 442:
                case 591:
                case 592:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_listen) <= 5");
                    throw new NotSupportedException("Conversion failed.");
                case 443:
                case 593:
                case 594:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_listen) >= 6");
                    throw new NotSupportedException("Conversion failed.");
                case 471:
                case 484:
                case 485:
                    Trace.Assert(originalScript == "game.global_flags[104] == 1 and game.global_flags[107] == 0 and game.global_flags[105] == 0");
                    return GetGlobalFlag(104) && !GetGlobalFlag(107) && !GetGlobalFlag(105);
                case 472:
                case 493:
                case 495:
                    Trace.Assert(originalScript == "game.global_flags[104] == 0 and game.global_flags[107] == 1 and game.global_flags[105] == 0");
                    return !GetGlobalFlag(104) && GetGlobalFlag(107) && !GetGlobalFlag(105);
                case 473:
                case 501:
                case 505:
                    Trace.Assert(originalScript == "game.global_flags[104] == 0 and game.global_flags[107] == 0 and game.global_flags[105] == 1");
                    return !GetGlobalFlag(104) && !GetGlobalFlag(107) && GetGlobalFlag(105);
                case 474:
                case 494:
                case 496:
                    Trace.Assert(originalScript == "game.global_flags[104] == 1 and game.global_flags[107] == 1 and game.global_flags[105] == 0");
                    return GetGlobalFlag(104) && GetGlobalFlag(107) && !GetGlobalFlag(105);
                case 475:
                case 502:
                case 506:
                    Trace.Assert(originalScript == "game.global_flags[104] == 1 and game.global_flags[107] == 0 and game.global_flags[105] == 1");
                    return GetGlobalFlag(104) && !GetGlobalFlag(107) && GetGlobalFlag(105);
                case 476:
                case 491:
                case 503:
                case 507:
                    Trace.Assert(originalScript == "game.global_flags[104] == 0 and game.global_flags[107] == 1 and game.global_flags[105] == 1");
                    return !GetGlobalFlag(104) && GetGlobalFlag(107) && GetGlobalFlag(105);
                case 477:
                case 492:
                case 504:
                case 508:
                    Trace.Assert(originalScript == "game.global_flags[104] == 1 and game.global_flags[107] == 1 and game.global_flags[105] == 1");
                    return GetGlobalFlag(104) && GetGlobalFlag(107) && GetGlobalFlag(105);
                case 481:
                    Trace.Assert(originalScript == "game.global_flags[107] == 1 and game.global_flags[105] == 0");
                    return GetGlobalFlag(107) && !GetGlobalFlag(105);
                case 482:
                    Trace.Assert(originalScript == "game.global_flags[107] == 0 and game.global_flags[105] == 1");
                    return !GetGlobalFlag(107) && GetGlobalFlag(105);
                case 483:
                    Trace.Assert(originalScript == "game.global_flags[107] == 1 and game.global_flags[105] == 1");
                    return GetGlobalFlag(107) && GetGlobalFlag(105);
                case 521:
                    Trace.Assert(originalScript == "get_v(454) & 2**2 == 0");
                    return (ScriptDaemon.get_v(454) & 0x4) == 0;
                case 522:
                    Trace.Assert(originalScript == "get_v(454) & 2**2 != 0");
                    return (ScriptDaemon.get_v(454) & 0x4) != 0;
                case 561:
                case 562:
                    Trace.Assert(originalScript == "( get_v(454) & (2**1) == 0)");
                    return ((ScriptDaemon.get_v(454) & (0x2)) == 0);
                case 563:
                case 564:
                    Trace.Assert(originalScript == "( get_v(454) & (2**1) != 0)");
                    return ((ScriptDaemon.get_v(454) & (0x2)) != 0);
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
                case 36:
                case 522:
                case 541:
                case 553:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 60:
                case 470:
                    Trace.Assert(originalScript == "game.story_state = 5");
                    StoryState = 5;
                    break;
                case 160:
                case 1600:
                    Trace.Assert(originalScript == "game.quests[49].state = qs_mentioned");
                    SetQuestState(49, QuestState.Mentioned);
                    break;
                case 175:
                case 176:
                    Trace.Assert(originalScript == "unlock_sw_doors()");
                    unlock_sw_doors();
                    break;
                case 190:
                    Trace.Assert(originalScript == "game.quests[49].state = qs_accepted; game.global_flags[346] = 1; record_time_stamp(479)");
                    SetQuestState(49, QuestState.Accepted);
                    SetGlobalFlag(346, true);
                    ScriptDaemon.record_time_stamp(479);
                    ;
                    break;
                case 220:
                case 310:
                    Trace.Assert(originalScript == "game.quests[49].state = qs_completed; pc.reputation_add( 10 )");
                    SetQuestState(49, QuestState.Completed);
                    pc.AddReputation(10);
                    ;
                    break;
                case 240:
                    Trace.Assert(originalScript == "game.quests[50].state = qs_mentioned");
                    SetQuestState(50, QuestState.Mentioned);
                    break;
                case 250:
                    Trace.Assert(originalScript == "game.quests[50].state = qs_accepted; npc.item_transfer_to(pc,3601); game.global_flags[346] = 1");
                    SetQuestState(50, QuestState.Accepted);
                    npc.TransferItemByNameTo(pc, 3601);
                    SetGlobalFlag(346, true);
                    ;
                    break;
                case 305:
                case 306:
                    Trace.Assert(originalScript == "game.quests[49].state = qs_botched");
                    SetQuestState(49, QuestState.Botched);
                    break;
                case 320:
                    Trace.Assert(originalScript == "game.quests[50].state = qs_completed");
                    SetQuestState(50, QuestState.Completed);
                    break;
                case 340:
                    Trace.Assert(originalScript == "game.quests[51].state = qs_mentioned");
                    SetQuestState(51, QuestState.Mentioned);
                    break;
                case 360:
                case 380:
                case 390:
                    Trace.Assert(originalScript == "game.quests[51].state = qs_completed");
                    SetQuestState(51, QuestState.Completed);
                    break;
                case 400:
                    Trace.Assert(originalScript == "record_time_stamp(483); game.quests[51].state = qs_accepted");
                    ScriptDaemon.record_time_stamp(483);
                    SetQuestState(51, QuestState.Accepted);
                    ;
                    break;
                case 443:
                case 593:
                case 594:
                    Trace.Assert(originalScript == "game.global_flags[813] = 1");
                    SetGlobalFlag(813, true);
                    break;
                case 451:
                case 601:
                    Trace.Assert(originalScript == "escort_below(npc, pc)");
                    escort_below(npc, pc);
                    break;
                case 1621:
                case 1641:
                    Trace.Assert(originalScript == "pc.reputation_add( 10 )");
                    pc.AddReputation(10);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
