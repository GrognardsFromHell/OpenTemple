
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Dialog
{
    [DialogScript(9)]
    public class ChurchServingWomanDialog : ChurchServingWoman, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met(pc)";
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                    originalScript = "npc.has_met(pc)";
                    return npc.HasMet(pc);
                case 61:
                case 62:
                    originalScript = "game.story_state >= 1";
                    return StoryState >= 1;
                case 161:
                case 162:
                    originalScript = "game.party_alignment == NEUTRAL_GOOD";
                    return PartyAlignment == Alignment.NEUTRAL_GOOD;
                case 191:
                case 192:
                    originalScript = "game.quests[8].state == qs_unknown";
                    return GetQuestState(8) == QuestState.Unknown;
                case 193:
                case 194:
                    originalScript = "game.quests[8].state == qs_mentioned";
                    return GetQuestState(8) == QuestState.Mentioned;
                case 195:
                case 196:
                    originalScript = "game.quests[8].state == qs_completed";
                    return GetQuestState(8) == QuestState.Completed;
                case 197:
                case 198:
                    originalScript = "game.global_flags[11] == 0";
                    return !GetGlobalFlag(11);
                case 199:
                case 200:
                    originalScript = "game.quests[8].state >= qs_mentioned";
                    return GetQuestState(8) >= QuestState.Mentioned;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 40:
                    originalScript = "game.quests[8].state = qs_mentioned";
                    SetQuestState(8, QuestState.Mentioned);
                    break;
                case 110:
                case 121:
                case 141:
                    originalScript = "game.quests[8].state = qs_accepted; npc.reaction_adj( pc,+10)";
                    SetQuestState(8, QuestState.Accepted);
                    npc.AdjustReaction(pc, +10);
                    ;
                    break;
                case 170:
                    originalScript = "game.global_flags[11] = 1";
                    SetGlobalFlag(11, true);
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
