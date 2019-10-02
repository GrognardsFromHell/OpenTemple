
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
    [DialogScript(72)]
    public class BraumeisterDialog : Braumeister, IDialogScript
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
                case 6:
                    Trace.Assert(originalScript == "npc.has_met( pc )");
                    return npc.HasMet(pc);
                case 43:
                case 44:
                case 61:
                case 62:
                case 212:
                case 215:
                    Trace.Assert(originalScript == "game.quests[14].state == qs_completed and game.global_flags[30] == 1");
                    return GetQuestState(14) == QuestState.Completed && GetGlobalFlag(30);
                case 53:
                case 54:
                case 81:
                case 82:
                    Trace.Assert(originalScript == "pc.stat_level_get( stat_deity ) == 16");
                    return pc.GetStat(Stat.deity) == 16;
                case 65:
                case 66:
                    Trace.Assert(originalScript == "game.quests[14].state <= qs_mentioned");
                    return GetQuestState(14) <= QuestState.Mentioned;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 130:
                case 150:
                    Trace.Assert(originalScript == "npc.reaction_adj( pc,-5)");
                    npc.AdjustReaction(pc, -5);
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
