
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
    [DialogScript(72)]
    public class BraumeisterDialog : Braumeister, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met( pc )";
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                case 6:
                    originalScript = "npc.has_met( pc )";
                    return npc.HasMet(pc);
                case 43:
                case 44:
                case 61:
                case 62:
                case 212:
                case 215:
                    originalScript = "game.quests[14].state == qs_completed and game.global_flags[30] == 1";
                    return GetQuestState(14) == QuestState.Completed && GetGlobalFlag(30);
                case 47:
                case 48:
                    originalScript = "game.quests[97].state == qs_mentioned or game.quests[97].state == qs_accepted or game.quests[97].state == qs_completed";
                    return GetQuestState(97) == QuestState.Mentioned || GetQuestState(97) == QuestState.Accepted || GetQuestState(97) == QuestState.Completed;
                case 53:
                case 54:
                case 81:
                case 82:
                    originalScript = "pc.stat_level_get( stat_deity ) == 16";
                    return pc.GetStat(Stat.deity) == 16;
                case 65:
                case 66:
                    originalScript = "game.quests[14].state <= qs_mentioned";
                    return GetQuestState(14) <= QuestState.Mentioned;
                case 281:
                    originalScript = "game.global_flags[694] == 1";
                    return GetGlobalFlag(694);
                case 282:
                    originalScript = "game.global_flags[694] == 0";
                    return !GetGlobalFlag(694);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 130:
                case 150:
                    originalScript = "npc.reaction_adj( pc,-5)";
                    npc.AdjustReaction(pc, -5);
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
