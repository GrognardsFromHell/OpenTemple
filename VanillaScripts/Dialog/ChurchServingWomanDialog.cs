
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

namespace VanillaScripts.Dialog
{
    [DialogScript(9)]
    public class ChurchServingWomanDialog : ChurchServingWoman, IDialogScript
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
                    Trace.Assert(originalScript == "npc.has_met(pc)");
                    return npc.HasMet(pc);
                case 61:
                case 62:
                    Trace.Assert(originalScript == "game.story_state >= 1");
                    return StoryState >= 1;
                case 161:
                case 162:
                    Trace.Assert(originalScript == "game.party_alignment == NEUTRAL_GOOD");
                    return PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 191:
                case 192:
                    Trace.Assert(originalScript == "game.quests[8].state == qs_unknown");
                    return GetQuestState(8) == QuestState.Unknown;
                case 193:
                case 194:
                    Trace.Assert(originalScript == "game.quests[8].state == qs_mentioned");
                    return GetQuestState(8) == QuestState.Mentioned;
                case 195:
                case 196:
                    Trace.Assert(originalScript == "game.quests[8].state == qs_completed");
                    return GetQuestState(8) == QuestState.Completed;
                case 197:
                case 198:
                    Trace.Assert(originalScript == "game.global_flags[11] == 0");
                    return !GetGlobalFlag(11);
                case 199:
                case 200:
                    Trace.Assert(originalScript == "game.quests[8].state >= qs_mentioned");
                    return GetQuestState(8) >= QuestState.Mentioned;
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
                    Trace.Assert(originalScript == "game.quests[8].state = qs_mentioned");
                    SetQuestState(8, QuestState.Mentioned);
                    break;
                case 110:
                case 121:
                case 141:
                    Trace.Assert(originalScript == "game.quests[8].state = qs_accepted; npc.reaction_adj( pc,+10)");
                    SetQuestState(8, QuestState.Accepted);
                    npc.AdjustReaction(pc, +10);
                    ;
                    break;
                case 170:
                    Trace.Assert(originalScript == "game.global_flags[11] = 1");
                    SetGlobalFlag(11, true);
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
