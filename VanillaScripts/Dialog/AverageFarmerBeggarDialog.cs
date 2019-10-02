
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
    [DialogScript(179)]
    public class AverageFarmerBeggarDialog : AverageFarmerBeggar, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 4:
                    Trace.Assert(originalScript == "game.global_flags[1] == 1");
                    return GetGlobalFlag(1);
                case 21:
                case 24:
                    Trace.Assert(originalScript == "pc.money_get() >= 100");
                    return pc.GetMoney() >= 100;
                case 22:
                case 23:
                case 25:
                case 26:
                    Trace.Assert(originalScript == "pc.money_get() >= 1000");
                    return pc.GetMoney() >= 1000;
                case 32:
                case 33:
                    Trace.Assert(originalScript == "game.global_vars[20] == 0");
                    return GetGlobalVar(20) == 0;
                case 34:
                case 35:
                    Trace.Assert(originalScript == "game.global_vars[20] >= 1");
                    return GetGlobalVar(20) >= 1;
                case 61:
                case 71:
                case 81:
                    Trace.Assert(originalScript == "game.global_vars[20] >= 50 and game.global_vars[20] <= 100 and game.global_flags[206] == 0");
                    return GetGlobalVar(20) >= 50 && GetGlobalVar(20) <= 100 && !GetGlobalFlag(206);
                case 62:
                case 72:
                case 82:
                    Trace.Assert(originalScript == "game.global_vars[20] >= 101 and game.global_vars[20] <= 200 and game.global_flags[206] == 1");
                    return GetGlobalVar(20) >= 101 && GetGlobalVar(20) <= 200 && GetGlobalFlag(206);
                case 63:
                case 73:
                case 83:
                    Trace.Assert(originalScript == "game.global_vars[20] >= 201 and game.global_vars[20] <= 300 and game.global_flags[206] == 0");
                    return GetGlobalVar(20) >= 201 && GetGlobalVar(20) <= 300 && !GetGlobalFlag(206);
                case 64:
                case 74:
                case 84:
                    Trace.Assert(originalScript == "game.global_vars[20] >= 301 and game.global_vars[20] <= 400 and game.global_flags[206] == 1");
                    return GetGlobalVar(20) >= 301 && GetGlobalVar(20) <= 400 && GetGlobalFlag(206);
                case 65:
                case 75:
                case 85:
                    Trace.Assert(originalScript == "game.global_vars[20] >= 401");
                    return GetGlobalVar(20) >= 401;
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
                case 24:
                    Trace.Assert(originalScript == "pc.money_adj(-100); game.global_vars[20] = game.global_vars[20] + 1");
                    pc.AdjustMoney(-100);
                    SetGlobalVar(20, GetGlobalVar(20) + 1);
                    ;
                    break;
                case 22:
                case 25:
                    Trace.Assert(originalScript == "pc.money_adj(-1000); game.global_vars[20] = game.global_vars[20] + 10");
                    pc.AdjustMoney(-1000);
                    SetGlobalVar(20, GetGlobalVar(20) + 10);
                    ;
                    break;
                case 23:
                case 26:
                    Trace.Assert(originalScript == "pc.money_adj(-10000); game.global_vars[20] = game.global_vars[20] + 100");
                    pc.AdjustMoney(-10000);
                    SetGlobalVar(20, GetGlobalVar(20) + 100);
                    ;
                    break;
                case 90:
                case 110:
                    Trace.Assert(originalScript == "game.global_flags[206] = 1");
                    SetGlobalFlag(206, true);
                    break;
                case 100:
                case 120:
                    Trace.Assert(originalScript == "game.global_flags[206] = 0");
                    SetGlobalFlag(206, false);
                    break;
                case 131:
                    Trace.Assert(originalScript == "run_off(npc,pc)");
                    run_off(npc, pc);
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
