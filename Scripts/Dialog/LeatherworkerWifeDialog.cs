
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
    [DialogScript(56)]
    public class LeatherworkerWifeDialog : LeatherworkerWife, IDialogScript
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
                case 8:
                    Trace.Assert(originalScript == "npc.has_met( pc )");
                    return npc.HasMet(pc);
                case 6:
                case 7:
                    Trace.Assert(originalScript == "npc.has_met( pc ) and game.global_vars[996] >= 1");
                    return npc.HasMet(pc) && GetGlobalVar(996) >= 1;
                case 21:
                case 22:
                    Trace.Assert(originalScript == "game.quests[11].state >= qs_mentioned and game.global_vars[996] == 0");
                    return GetQuestState(11) >= QuestState.Mentioned && GetGlobalVar(996) == 0;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
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
