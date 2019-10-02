
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
    [DialogScript(101)]
    public class SammyDialog : Sammy, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 41:
                case 42:
                    Trace.Assert(originalScript == "pc.money_get() < 50000");
                    return pc.GetMoney() < 50000;
                case 43:
                case 44:
                case 221:
                case 222:
                    Trace.Assert(originalScript == "pc.money_get() >= 50000");
                    return pc.GetMoney() >= 50000;
                case 45:
                case 46:
                case 223:
                case 224:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_diplomacy) >= 6 and pc.money_get() >= 20000");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 6 && pc.GetMoney() >= 20000;
                case 101:
                case 104:
                    Trace.Assert(originalScript == "(game.quests[32].state == qs_mentioned) and (game.quests[63].state != qs_mentioned)");
                    return (GetQuestState(32) == QuestState.Mentioned) && (GetQuestState(63) != QuestState.Mentioned);
                case 102:
                case 105:
                    Trace.Assert(originalScript == "game.quests[32].state == qs_accepted");
                    return GetQuestState(32) == QuestState.Accepted;
                case 103:
                case 106:
                    Trace.Assert(originalScript == "game.quests[32].state == qs_completed");
                    return GetQuestState(32) == QuestState.Completed;
                case 107:
                case 108:
                    Trace.Assert(originalScript == "(game.quests[32].state == qs_mentioned) and (game.quests[63].state == qs_mentioned)");
                    return (GetQuestState(32) == QuestState.Mentioned) && (GetQuestState(63) == QuestState.Mentioned);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 40:
                    Trace.Assert(originalScript == "game.quests[32].state = qs_mentioned");
                    SetQuestState(32, QuestState.Mentioned);
                    break;
                case 43:
                case 44:
                case 221:
                case 222:
                    Trace.Assert(originalScript == "pc.money_adj(-50000)");
                    pc.AdjustMoney(-50000);
                    break;
                case 45:
                case 46:
                case 223:
                case 224:
                    Trace.Assert(originalScript == "pc.money_adj(-20000)");
                    pc.AdjustMoney(-20000);
                    break;
                case 60:
                    Trace.Assert(originalScript == "game.quests[32].state = qs_accepted");
                    SetQuestState(32, QuestState.Accepted);
                    break;
                case 70:
                    Trace.Assert(originalScript == "game.quests[63].state = qs_mentioned");
                    SetQuestState(63, QuestState.Mentioned);
                    break;
                case 180:
                case 190:
                    Trace.Assert(originalScript == "game.quests[63].state = qs_accepted");
                    SetQuestState(63, QuestState.Accepted);
                    break;
                case 191:
                case 192:
                    Trace.Assert(originalScript == "pc.money_adj(5000)");
                    pc.AdjustMoney(5000);
                    break;
                case 200:
                    Trace.Assert(originalScript == "game.quests[32].state = qs_botched");
                    SetQuestState(32, QuestState.Botched);
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
                case 45:
                case 46:
                case 223:
                case 224:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 6);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
