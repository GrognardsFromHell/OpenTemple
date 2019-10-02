
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
    [DialogScript(146)]
    public class KelleenDialog : Kelleen, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 25:
                case 26:
                    Trace.Assert(originalScript == "game.global_flags[110] == 1");
                    return GetGlobalFlag(110);
                case 27:
                case 28:
                    Trace.Assert(originalScript == "game.quests[52].state == qs_completed");
                    return GetQuestState(52) == QuestState.Completed;
                case 29:
                case 30:
                    Trace.Assert(originalScript == "game.quests[46].state == qs_completed");
                    return GetQuestState(46) == QuestState.Completed;
                case 31:
                case 32:
                    Trace.Assert(originalScript == "game.quests[52].state == qs_accepted");
                    return GetQuestState(52) == QuestState.Accepted;
                case 103:
                case 104:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_bluff) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 10;
                case 105:
                case 106:
                    Trace.Assert(originalScript == "game.quests[52].state >= qs_accepted");
                    return GetQuestState(52) >= QuestState.Accepted;
                case 107:
                case 108:
                    Trace.Assert(originalScript == "game.quests[46].state >= qs_accepted");
                    return GetQuestState(46) >= QuestState.Accepted;
                case 175:
                case 176:
                    Trace.Assert(originalScript == "game.quests[52].state != qs_completed");
                    return GetQuestState(52) != QuestState.Completed;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 43:
                case 44:
                case 91:
                case 101:
                case 102:
                case 111:
                case 161:
                case 162:
                case 173:
                case 174:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
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
                case 103:
                case 104:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
