
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
    [DialogScript(337)]
    public class ElysiaDialog : Elysia, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    Trace.Assert(originalScript == "not npc.has_met(pc) and (game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL)");
                    return !npc.HasMet(pc) && (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL);
                case 3:
                    Trace.Assert(originalScript == "not npc.has_met(pc) and (game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL)");
                    return !npc.HasMet(pc) && (PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL);
                case 4:
                    Trace.Assert(originalScript == "not npc.has_met(pc) and (game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL)");
                    return !npc.HasMet(pc) && (PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 5:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.quests[75].state == qs_mentioned");
                    return npc.HasMet(pc) && GetQuestState(75) == QuestState.Mentioned;
                case 6:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.quests[75].state == qs_accepted and (game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL)");
                    return npc.HasMet(pc) && GetQuestState(75) == QuestState.Accepted && (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL);
                case 7:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.quests[75].state == qs_completed and (game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL)");
                    return npc.HasMet(pc) && GetQuestState(75) == QuestState.Completed && (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL);
                case 8:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.quests[75].state == qs_accepted and (game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL)");
                    return npc.HasMet(pc) && GetQuestState(75) == QuestState.Accepted && (PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL);
                case 9:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.quests[75].state == qs_completed and (game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL)");
                    return npc.HasMet(pc) && GetQuestState(75) == QuestState.Completed && (PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL);
                case 10:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.quests[75].state == qs_accepted and (game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL)");
                    return npc.HasMet(pc) && GetQuestState(75) == QuestState.Accepted && (PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 11:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.quests[75].state == qs_completed and (game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL)");
                    return npc.HasMet(pc) && GetQuestState(75) == QuestState.Completed && (PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 12:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.quests[75].state == qs_botched and game.global_flags[987] == 0");
                    return npc.HasMet(pc) && GetQuestState(75) == QuestState.Botched && !GetGlobalFlag(987);
                case 13:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.quests[75].state == qs_botched and game.global_flags[987] == 1");
                    return npc.HasMet(pc) && GetQuestState(75) == QuestState.Botched && GetGlobalFlag(987);
                case 14:
                    Trace.Assert(originalScript == "npc.has_met(pc) and game.global_vars[997] == 0 and not get_1(npc)");
                    return npc.HasMet(pc) && GetGlobalVar(997) == 0 && !Scripts.get_1(npc);
                case 21:
                case 311:
                    Trace.Assert(originalScript == "game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_NEUTRAL or game.party_alignment == TRUE_NEUTRAL");
                    return PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL || PartyAlignment == Alignment.NEUTRAL;
                case 22:
                case 312:
                    Trace.Assert(originalScript == "game.party_alignment == CHAOTIC_NEUTRAL or game.party_alignment == LAWFUL_EVIL");
                    return PartyAlignment == Alignment.CHAOTIC_NEUTRAL || PartyAlignment == Alignment.LAWFUL_EVIL;
                case 23:
                case 313:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL");
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 321:
                    Trace.Assert(originalScript == "pc.money_get() >= 100000");
                    return pc.GetMoney() >= 100000;
                case 322:
                    Trace.Assert(originalScript == "pc.money_get() <= 99900");
                    return pc.GetMoney() <= 99900;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 14:
                    Trace.Assert(originalScript == "npc_1(npc)");
                    Scripts.npc_1(npc);
                    break;
                case 30:
                    Trace.Assert(originalScript == "game.global_flags[987] = 1");
                    SetGlobalFlag(987, true);
                    break;
                case 100:
                case 110:
                case 120:
                    Trace.Assert(originalScript == "game.quests[75].state = qs_mentioned");
                    SetQuestState(75, QuestState.Mentioned);
                    break;
                case 190:
                    Trace.Assert(originalScript == "pc.money_adj(50000)");
                    pc.AdjustMoney(50000);
                    break;
                case 191:
                case 201:
                case 211:
                    Trace.Assert(originalScript == "game.quests[75].state = qs_accepted");
                    SetQuestState(75, QuestState.Accepted);
                    break;
                case 321:
                    Trace.Assert(originalScript == "pc.money_adj(-100000)");
                    pc.AdjustMoney(-100000);
                    break;
                case 330:
                    Trace.Assert(originalScript == "game.global_vars[997] = 1");
                    SetGlobalVar(997, 1);
                    break;
                case 331:
                case 341:
                case 351:
                    Trace.Assert(originalScript == "run_off( npc, pc )");
                    run_off(npc, pc);
                    break;
                case 340:
                    Trace.Assert(originalScript == "game.global_vars[997] = 2");
                    SetGlobalVar(997, 2);
                    break;
                case 350:
                    Trace.Assert(originalScript == "game.global_vars[997] = 3");
                    SetGlobalVar(997, 3);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
