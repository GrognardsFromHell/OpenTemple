
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
    [DialogScript(504)]
    public class KwerdDialog : Kwerd, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 31:
                    Trace.Assert(originalScript == "not get_1(npc)");
                    return !Scripts.get_1(npc);
                case 32:
                    Trace.Assert(originalScript == "not get_2(npc)");
                    return !Scripts.get_2(npc);
                case 33:
                    Trace.Assert(originalScript == "game.global_vars[990] == 1");
                    return GetGlobalVar(990) == 1;
                case 34:
                    Trace.Assert(originalScript == "game.global_vars[991] == 1");
                    return GetGlobalVar(991) == 1;
                case 35:
                    Trace.Assert(originalScript == "game.global_vars[990] == 3");
                    return GetGlobalVar(990) == 3;
                case 36:
                    Trace.Assert(originalScript == "game.global_vars[991] == 3");
                    return GetGlobalVar(991) == 3;
                case 37:
                    Trace.Assert(originalScript == "game.global_vars[989] == 1");
                    return GetGlobalVar(989) == 1;
                case 38:
                    Trace.Assert(originalScript == "game.global_vars[989] == 2");
                    return GetGlobalVar(989) == 2;
                case 39:
                    Trace.Assert(originalScript == "game.global_vars[989] == 3");
                    return GetGlobalVar(989) == 3;
                case 40:
                    Trace.Assert(originalScript == "game.global_vars[989] == 4");
                    return GetGlobalVar(989) == 4;
                case 41:
                    Trace.Assert(originalScript == "game.global_vars[989] == 5");
                    return GetGlobalVar(989) == 5;
                case 42:
                    Trace.Assert(originalScript == "game.global_flags[976] == 1");
                    return GetGlobalFlag(976);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 22:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 31:
                    Trace.Assert(originalScript == "npc_1(npc)");
                    Scripts.npc_1(npc);
                    break;
                case 32:
                    Trace.Assert(originalScript == "npc_2(npc)");
                    Scripts.npc_2(npc);
                    break;
                case 60:
                    Trace.Assert(originalScript == "game.global_vars[990] = 2");
                    SetGlobalVar(990, 2);
                    break;
                case 70:
                    Trace.Assert(originalScript == "game.global_vars[991] = 2");
                    SetGlobalVar(991, 2);
                    break;
                case 120:
                    Trace.Assert(originalScript == "game.global_vars[990] = 4");
                    SetGlobalVar(990, 4);
                    break;
                case 130:
                    Trace.Assert(originalScript == "game.global_vars[991] = 4");
                    SetGlobalVar(991, 4);
                    break;
                case 151:
                    Trace.Assert(originalScript == "run_off(npc,pc)");
                    run_off(npc, pc);
                    break;
                case 160:
                    Trace.Assert(originalScript == "game.global_vars[989] = 6");
                    SetGlobalVar(989, 6);
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
