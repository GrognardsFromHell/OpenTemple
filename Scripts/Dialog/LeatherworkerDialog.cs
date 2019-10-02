
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
    [DialogScript(53)]
    public class LeatherworkerDialog : Leatherworker, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    Trace.Assert(originalScript == "not npc.has_met( pc )");
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                case 106:
                case 107:
                    Trace.Assert(originalScript == "game.global_flags[34] == 1 and game.quests[11].state == qs_accepted and game.global_flags[16] == 0");
                    return GetGlobalFlag(34) && GetQuestState(11) == QuestState.Accepted && !GetGlobalFlag(16);
                case 6:
                    Trace.Assert(originalScript == "npc.has_met( pc ) and game.quests[11].state <= qs_accepted");
                    return npc.HasMet(pc) && GetQuestState(11) <= QuestState.Accepted;
                case 7:
                    Trace.Assert(originalScript == "npc.has_met( pc ) and game.quests[11].state == qs_completed");
                    return npc.HasMet(pc) && GetQuestState(11) == QuestState.Completed;
                case 8:
                    Trace.Assert(originalScript == "npc.has_met( pc )");
                    return npc.HasMet(pc);
                case 11:
                case 12:
                    Trace.Assert(originalScript == "game.quests[11].state == qs_accepted");
                    return GetQuestState(11) == QuestState.Accepted;
                case 16:
                case 45:
                case 108:
                case 126:
                    Trace.Assert(originalScript == "game.global_vars[996] >= 1");
                    return GetGlobalVar(996) >= 1;
                case 17:
                case 46:
                case 109:
                case 127:
                    Trace.Assert(originalScript == "game.global_vars[996] == 0");
                    return GetGlobalVar(996) == 0;
                case 103:
                case 104:
                    Trace.Assert(originalScript == "game.global_flags[34] == 0 and game.global_flags[16] == 0 and game.quests[11].state == qs_accepted");
                    return !GetGlobalFlag(34) && !GetGlobalFlag(16) && GetQuestState(11) == QuestState.Accepted;
                case 124:
                case 125:
                    Trace.Assert(originalScript == "game.story_state >= 2 and game.areas[3] == 0");
                    return StoryState >= 2 && !IsAreaKnown(3);
                case 211:
                    Trace.Assert(originalScript == "game.global_flags[33] == 0");
                    return !GetGlobalFlag(33);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 30:
                    Trace.Assert(originalScript == "game.global_flags[33] = 1");
                    SetGlobalFlag(33, true);
                    break;
                case 130:
                    Trace.Assert(originalScript == "game.global_flags[16] = 1; npc.reaction_adj( pc,+30)");
                    SetGlobalFlag(16, true);
                    npc.AdjustReaction(pc, +30);
                    ;
                    break;
                case 140:
                    Trace.Assert(originalScript == "game.areas[3] = 1; game.story_state = 3");
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 141:
                case 142:
                    Trace.Assert(originalScript == "game.worldmap_travel_by_dialog(3)");
                    // FIXME: worldmap_travel_by_dialog;
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
