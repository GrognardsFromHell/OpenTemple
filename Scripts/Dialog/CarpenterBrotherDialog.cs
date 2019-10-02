
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
    [DialogScript(7)]
    public class CarpenterBrotherDialog : CarpenterBrother, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                case 121:
                case 122:
                    Trace.Assert(originalScript == "not npc.has_met( pc )");
                    return !npc.HasMet(pc);
                case 4:
                    Trace.Assert(originalScript == "npc.has_met( pc )");
                    return npc.HasMet(pc);
                case 21:
                case 23:
                    Trace.Assert(originalScript == "game.quests[5].state == qs_mentioned");
                    return GetQuestState(5) == QuestState.Mentioned;
                case 22:
                case 24:
                    Trace.Assert(originalScript == "game.quests[5].state == qs_completed and game.global_flags[32] == 1");
                    return GetQuestState(5) == QuestState.Completed && GetGlobalFlag(32);
                case 27:
                case 28:
                    Trace.Assert(originalScript == "game.quests[5].state == qs_completed and game.global_flags[32] == 0");
                    return GetQuestState(5) == QuestState.Completed && !GetGlobalFlag(32);
                case 29:
                case 30:
                    Trace.Assert(originalScript == "game.quests[5].state == qs_accepted and game.global_flags[17] == 1");
                    return GetQuestState(5) == QuestState.Accepted && GetGlobalFlag(17);
                case 41:
                case 42:
                    Trace.Assert(originalScript == "game.global_flags[1] == 0");
                    return !GetGlobalFlag(1);
                case 43:
                case 44:
                    Trace.Assert(originalScript == "game.global_flags[1] == 1");
                    return GetGlobalFlag(1);
                case 173:
                case 174:
                case 183:
                case 184:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 5");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 5;
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
                    Trace.Assert(originalScript == "game.quests[5].state = qs_mentioned");
                    SetQuestState(5, QuestState.Mentioned);
                    break;
                case 71:
                    Trace.Assert(originalScript == "game.quests[5].state = qs_accepted; npc.reaction_adj( pc,+10)");
                    SetQuestState(5, QuestState.Accepted);
                    npc.AdjustReaction(pc, +10);
                    ;
                    break;
                case 72:
                case 74:
                    Trace.Assert(originalScript == "game.quests[5].state = qs_accepted; npc.reaction_adj( pc,-10); buttin(npc,pc,210)");
                    SetQuestState(5, QuestState.Accepted);
                    npc.AdjustReaction(pc, -10);
                    buttin(npc, pc, 210);
                    ;
                    break;
                case 73:
                case 81:
                case 82:
                case 151:
                case 152:
                    Trace.Assert(originalScript == "game.quests[5].state = qs_accepted");
                    SetQuestState(5, QuestState.Accepted);
                    break;
                case 90:
                case 120:
                    Trace.Assert(originalScript == "npc.reaction_adj( pc,+10)");
                    npc.AdjustReaction(pc, +10);
                    break;
                case 100:
                    Trace.Assert(originalScript == "game.global_flags[32] = 1");
                    SetGlobalFlag(32, true);
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
                case 173:
                case 174:
                case 183:
                case 184:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 5);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
