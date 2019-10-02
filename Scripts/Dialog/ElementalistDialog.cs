
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
    [DialogScript(482)]
    public class ElementalistDialog : Elementalist, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 11:
                case 21:
                case 31:
                    Trace.Assert(originalScript == "not npc_get(npc,2)");
                    return !ScriptDaemon.npc_get(npc, 2);
                case 3:
                case 12:
                case 22:
                case 32:
                    Trace.Assert(originalScript == "not npc_get(npc,3)");
                    return !ScriptDaemon.npc_get(npc, 3);
                case 4:
                case 13:
                case 23:
                case 33:
                    Trace.Assert(originalScript == "not npc_get(npc,4)");
                    return !ScriptDaemon.npc_get(npc, 4);
                case 5:
                case 14:
                case 24:
                case 34:
                    Trace.Assert(originalScript == "npc_get(npc,2) and npc_get(npc,3) and npc_get(npc,4)");
                    return ScriptDaemon.npc_get(npc, 2) && ScriptDaemon.npc_get(npc, 3) && ScriptDaemon.npc_get(npc, 4);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    Trace.Assert(originalScript == "npc_set(npc,1)");
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 6:
                case 15:
                case 25:
                case 35:
                    Trace.Assert(originalScript == "switch_to_ariakas( npc, pc, 310)");
                    switch_to_ariakas(npc, pc, 310);
                    break;
                case 10:
                    Trace.Assert(originalScript == "npc_set(npc,2)");
                    ScriptDaemon.npc_set(npc, 2);
                    break;
                case 20:
                    Trace.Assert(originalScript == "npc_set(npc,3)");
                    ScriptDaemon.npc_set(npc, 3);
                    break;
                case 30:
                    Trace.Assert(originalScript == "npc_set(npc,4)");
                    ScriptDaemon.npc_set(npc, 4);
                    break;
                case 40:
                    Trace.Assert(originalScript == "npc_set(npc,5)");
                    ScriptDaemon.npc_set(npc, 5);
                    break;
                case 41:
                    Trace.Assert(originalScript == "switch_to_ariakas( npc, pc, 170); shake(npc,pc)");
                    switch_to_ariakas(npc, pc, 170);
                    shake(npc, pc);
                    ;
                    break;
                case 51:
                    Trace.Assert(originalScript == "switch_to_ariakas( npc, pc, 180)");
                    switch_to_ariakas(npc, pc, 180);
                    break;
                case 61:
                    Trace.Assert(originalScript == "dump_old_man(npc,pc); switch_to_ariakas( npc, pc, 190)");
                    dump_old_man(npc, pc);
                    switch_to_ariakas(npc, pc, 190);
                    ;
                    break;
                case 71:
                    Trace.Assert(originalScript == "switch_to_ariakas( npc, pc, 320); shake(npc,pc)");
                    switch_to_ariakas(npc, pc, 320);
                    shake(npc, pc);
                    ;
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
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}