
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
    [DialogScript(178)]
    public class ErliterBeggarDialog : ErliterBeggar, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 41:
                case 43:
                case 61:
                case 64:
                    Trace.Assert(originalScript == "pc.money_get() >= 100");
                    return pc.GetMoney() >= 100;
                case 62:
                case 65:
                    Trace.Assert(originalScript == "pc.money_get() >= 1000");
                    return pc.GetMoney() >= 1000;
                case 63:
                case 66:
                    Trace.Assert(originalScript == "pc.money_get() >= 10000");
                    return pc.GetMoney() >= 10000;
                case 71:
                    Trace.Assert(originalScript == "game.global_vars[19] > 100");
                    return GetGlobalVar(19) > 100;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 41:
                case 43:
                case 61:
                case 64:
                    Trace.Assert(originalScript == "pc.money_adj(-100); game.global_vars[19] = game.global_vars[19] + 1");
                    pc.AdjustMoney(-100);
                    SetGlobalVar(19, GetGlobalVar(19) + 1);
                    ;
                    break;
                case 62:
                case 65:
                    Trace.Assert(originalScript == "pc.money_adj(-1000); game.global_vars[19] = game.global_vars[19] + 10");
                    pc.AdjustMoney(-1000);
                    SetGlobalVar(19, GetGlobalVar(19) + 10);
                    ;
                    break;
                case 63:
                case 66:
                    Trace.Assert(originalScript == "pc.money_adj(-10000); game.global_vars[19] = game.global_vars[19] + 100");
                    pc.AdjustMoney(-10000);
                    SetGlobalVar(19, GetGlobalVar(19) + 100);
                    ;
                    break;
                case 91:
                    Trace.Assert(originalScript == "leave_for_city(npc,pc)");
                    leave_for_city(npc, pc);
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
