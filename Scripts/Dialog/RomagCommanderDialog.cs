
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
    [DialogScript(120)]
    public class RomagCommanderDialog : RomagCommander, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    Trace.Assert(originalScript == "game.global_flags[111] == 1");
                    return GetGlobalFlag(111);
                case 4:
                case 5:
                    Trace.Assert(originalScript == "game.global_flags[111] == 0");
                    return !GetGlobalFlag(111);
                case 31:
                case 45:
                    Trace.Assert(originalScript == "npc_get(npc, 1) == 0");
                    return !ScriptDaemon.npc_get(npc, 1);
                case 51:
                    Trace.Assert(originalScript == "npc_get(npc, 1) == 1 and npc_get(npc, 2) == 0 and (game.global_vars[452] & (2**0 + 2**1) >= 3 )");
                    return ScriptDaemon.npc_get(npc, 1) && !ScriptDaemon.npc_get(npc, 2) && ((GetGlobalVar(452) & (0x3)) == 0x3);
                case 141:
                    Trace.Assert(originalScript == "npc_get(npc, 3) == 1 and (game.global_vars[452] & (2**1) != 0 )");
                    return ScriptDaemon.npc_get(npc, 3) && ((GetGlobalVar(452) & (0x2)) != 0);
                case 142:
                    Trace.Assert(originalScript == "npc_get(npc, 5) == 1 and npc_get(npc, 6) == 0 and (game.global_vars[452] & (2**2) != 0 )");
                    return ScriptDaemon.npc_get(npc, 5) && !ScriptDaemon.npc_get(npc, 6) && ((GetGlobalVar(452) & (0x4)) != 0);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 15:
                case 25:
                case 32:
                case 41:
                case 42:
                case 52:
                case 101:
                case 113:
                case 121:
                case 143:
                case 163:
                case 171:
                case 182:
                case 187:
                case 196:
                    Trace.Assert(originalScript == "switch_to_gatekeeper(pc, 1800)");
                    Earthcombat.switch_to_gatekeeper(pc, 1800);
                    break;
                case 31:
                case 45:
                    Trace.Assert(originalScript == "npc_set(npc, 1)");
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 43:
                case 44:
                case 112:
                case 162:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
                    break;
                case 51:
                    Trace.Assert(originalScript == "npc_set(npc, 2)");
                    ScriptDaemon.npc_set(npc, 2);
                    break;
                case 111:
                    Trace.Assert(originalScript == "npc_set(npc, 3)");
                    ScriptDaemon.npc_set(npc, 3);
                    break;
                case 141:
                    Trace.Assert(originalScript == "npc_set(npc, 4)");
                    ScriptDaemon.npc_set(npc, 4);
                    break;
                case 142:
                    Trace.Assert(originalScript == "npc_set(npc, 6)");
                    ScriptDaemon.npc_set(npc, 6);
                    break;
                case 161:
                    Trace.Assert(originalScript == "npc_set(npc, 5)");
                    ScriptDaemon.npc_set(npc, 5);
                    break;
                case 181:
                    Trace.Assert(originalScript == "npc_set(npc, 7)");
                    ScriptDaemon.npc_set(npc, 7);
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
