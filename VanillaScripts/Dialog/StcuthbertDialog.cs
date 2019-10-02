
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
    [DialogScript(173)]
    public class StcuthbertDialog : Stcuthbert, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    Trace.Assert(originalScript == "switch_to_iuz(npc,pc,200)");
                    switch_to_iuz(npc, pc, 200);
                    break;
                case 11:
                    Trace.Assert(originalScript == "switch_to_iuz(npc,pc,210)");
                    switch_to_iuz(npc, pc, 210);
                    break;
                case 21:
                    Trace.Assert(originalScript == "switch_to_iuz(npc,pc,220)");
                    switch_to_iuz(npc, pc, 220);
                    break;
                case 30:
                    Trace.Assert(originalScript == "cuthbert_raise_good(npc,pc)");
                    cuthbert_raise_good(npc, pc);
                    break;
                case 31:
                    Trace.Assert(originalScript == "turn_off_gods(npc,pc)");
                    turn_off_gods(npc, pc);
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
