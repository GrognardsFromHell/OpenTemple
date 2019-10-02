
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
    [DialogScript(134)]
    public class MerrolanDialog : Merrolan, IDialogScript
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
                case 21:
                case 22:
                    Trace.Assert(originalScript == "switch_dialog(npc,pc,30)");
                    switch_dialog(npc, pc, 30);
                    break;
                case 41:
                case 42:
                    Trace.Assert(originalScript == "switch_dialog(npc,pc,50)");
                    switch_dialog(npc, pc, 50);
                    break;
                case 51:
                case 52:
                    Trace.Assert(originalScript == "switch_dialog(npc,pc,60)");
                    switch_dialog(npc, pc, 60);
                    break;
                case 61:
                case 62:
                    Trace.Assert(originalScript == "switch_dialog(npc,pc,70)");
                    switch_dialog(npc, pc, 70);
                    break;
                case 81:
                case 82:
                    Trace.Assert(originalScript == "switch_dialog(npc,pc,90)");
                    switch_dialog(npc, pc, 90);
                    break;
                case 101:
                case 102:
                    Trace.Assert(originalScript == "switch_dialog(npc,pc,100)");
                    switch_dialog(npc, pc, 100);
                    break;
                case 111:
                case 112:
                    Trace.Assert(originalScript == "switch_dialog(npc,pc,110)");
                    switch_dialog(npc, pc, 110);
                    break;
                case 121:
                case 122:
                    Trace.Assert(originalScript == "switch_dialog(npc,pc,120)");
                    switch_dialog(npc, pc, 120);
                    break;
                case 131:
                case 132:
                    Trace.Assert(originalScript == "switch_dialog(npc,pc,130)");
                    switch_dialog(npc, pc, 130);
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
