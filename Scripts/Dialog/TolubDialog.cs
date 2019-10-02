
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
    [DialogScript(116)]
    public class TolubDialog : Tolub, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    Trace.Assert(originalScript == "not npc.has_met(pc)");
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                case 202:
                case 206:
                case 210:
                case 214:
                    Trace.Assert(originalScript == "game.global_vars[10] == 1");
                    return GetGlobalVar(10) == 1;
                case 6:
                case 7:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.quests[36].state == qs_accepted");
                    return npc.HasMet(pc) && GetQuestState(36) == QuestState.Accepted;
                case 8:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.global_flags[290] == 0");
                    return npc.HasMet(pc) && !GetGlobalFlag(290);
                case 11:
                case 12:
                    Trace.Assert(originalScript == "game.global_flags[290] == 0");
                    return !GetGlobalFlag(290);
                case 13:
                case 14:
                case 21:
                case 22:
                case 32:
                case 33:
                case 221:
                case 222:
                case 231:
                case 232:
                case 241:
                case 242:
                case 265:
                case 266:
                case 333:
                case 334:
                    Trace.Assert(originalScript == "game.quests[36].state == qs_accepted");
                    return GetQuestState(36) == QuestState.Accepted;
                case 41:
                case 45:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_diplomacy) >= 11");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 11;
                case 71:
                case 72:
                case 531:
                case 532:
                    Trace.Assert(originalScript == "pc.money_get() >= 100000");
                    return pc.GetMoney() >= 100000;
                case 73:
                case 74:
                case 533:
                case 534:
                    Trace.Assert(originalScript == "pc.money_get() < 100000");
                    return pc.GetMoney() < 100000;
                case 141:
                case 142:
                    Trace.Assert(originalScript == "game.global_vars[10] <= 1");
                    return GetGlobalVar(10) <= 1;
                case 201:
                case 205:
                case 209:
                case 213:
                case 351:
                case 352:
                    Trace.Assert(originalScript == "game.global_vars[10] == 0");
                    return GetGlobalVar(10) == 0;
                case 203:
                case 207:
                case 211:
                case 215:
                    Trace.Assert(originalScript == "game.global_vars[10] == 2");
                    return GetGlobalVar(10) == 2;
                case 204:
                case 208:
                case 212:
                case 216:
                    Trace.Assert(originalScript == "game.global_vars[10] == 3");
                    return GetGlobalVar(10) == 3;
                case 301:
                    Trace.Assert(originalScript == "game.global_flags[101] == 0");
                    return !GetGlobalFlag(101);
                case 302:
                case 303:
                    Trace.Assert(originalScript == "game.global_flags[101] == 1");
                    return GetGlobalFlag(101);
                case 501:
                case 503:
                    Trace.Assert(originalScript == "game.global_flags[101] == 0 and game.quests[36].state == qs_accepted");
                    return !GetGlobalFlag(101) && GetQuestState(36) == QuestState.Accepted;
                case 502:
                case 504:
                    Trace.Assert(originalScript == "game.global_flags[101] == 1 and game.quests[36].state == qs_accepted");
                    return GetGlobalFlag(101) && GetQuestState(36) == QuestState.Accepted;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 71:
                case 72:
                case 531:
                case 532:
                    Trace.Assert(originalScript == "pc.money_adj(-100000)");
                    pc.AdjustMoney(-100000);
                    break;
                case 80:
                case 130:
                case 321:
                    Trace.Assert(originalScript == "game.quests[36].state = qs_completed");
                    SetQuestState(36, QuestState.Completed);
                    break;
                case 150:
                case 200:
                    Trace.Assert(originalScript == "game.quests[40].state = qs_mentioned");
                    SetQuestState(40, QuestState.Mentioned);
                    break;
                case 161:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 240:
                    Trace.Assert(originalScript == "game.global_vars[10] = 1");
                    SetGlobalVar(10, 1);
                    break;
                case 261:
                case 262:
                case 281:
                case 282:
                    Trace.Assert(originalScript == "game.quests[40].state = qs_accepted; brawl(npc,pc)");
                    SetQuestState(40, QuestState.Accepted);
                    brawl(npc, pc);
                    ;
                    break;
                case 280:
                    Trace.Assert(originalScript == "game.global_flags[101] = 1");
                    SetGlobalFlag(101, true);
                    break;
                case 290:
                    Trace.Assert(originalScript == "game.quests[40].state = qs_botched");
                    SetQuestState(40, QuestState.Botched);
                    break;
                case 300:
                    Trace.Assert(originalScript == "game.global_vars[10] = 3; game.quests[40].state = qs_completed; pc.reputation_add( 14 )");
                    SetGlobalVar(10, 3);
                    SetQuestState(40, QuestState.Completed);
                    pc.AddReputation(14);
                    ;
                    break;
                case 311:
                    Trace.Assert(originalScript == "pc.money_adj(10000)");
                    pc.AdjustMoney(10000);
                    break;
                case 330:
                    Trace.Assert(originalScript == "game.global_vars[10] = 2; game.quests[40].state = qs_botched");
                    SetGlobalVar(10, 2);
                    SetQuestState(40, QuestState.Botched);
                    ;
                    break;
                case 350:
                    Trace.Assert(originalScript == "game.global_flags[290] = 1");
                    SetGlobalFlag(290, true);
                    break;
                case 400:
                    Trace.Assert(originalScript == "game.global_vars[10] = 4");
                    SetGlobalVar(10, 4);
                    break;
                case 431:
                    Trace.Assert(originalScript == "game.quests[40].state = qs_botched; npc.attack(pc)");
                    SetQuestState(40, QuestState.Botched);
                    npc.Attack(pc);
                    ;
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
                case 41:
                case 45:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 11);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
