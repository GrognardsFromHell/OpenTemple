
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
    [DialogScript(180)]
    public class BrauApprenticeBeggarDialog : BrauApprenticeBeggar, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 4:
                case 5:
                    Trace.Assert(originalScript == "pc.money_get() >= 1");
                    return pc.GetMoney() >= 1;
                case 23:
                case 34:
                    Trace.Assert(originalScript == "pc.money_get() >= 100");
                    return pc.GetMoney() >= 100;
                case 24:
                case 35:
                    Trace.Assert(originalScript == "pc.money_get() >= 1000");
                    return pc.GetMoney() >= 1000;
                case 25:
                case 26:
                case 36:
                case 37:
                    Trace.Assert(originalScript == "pc.money_get() >= 10000");
                    return pc.GetMoney() >= 10000;
                case 57:
                case 58:
                    Trace.Assert(originalScript == "pc.follower_atmax() == 0");
                    return !pc.HasMaxFollowers();
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
                    Trace.Assert(originalScript == "game.global_vars[114] = 1");
                    SetGlobalVar(114, 1);
                    break;
                case 4:
                case 5:
                    Trace.Assert(originalScript == "pc.money_adj(-1)");
                    pc.AdjustMoney(-1);
                    break;
                case 23:
                case 34:
                    Trace.Assert(originalScript == "pc.money_adj(-100); game.global_vars[21] = 1");
                    pc.AdjustMoney(-100);
                    SetGlobalVar(21, 1);
                    ;
                    break;
                case 24:
                case 35:
                    Trace.Assert(originalScript == "pc.money_adj(-1000); game.global_vars[21] = 5");
                    pc.AdjustMoney(-1000);
                    SetGlobalVar(21, 5);
                    ;
                    break;
                case 25:
                case 26:
                case 36:
                case 37:
                    Trace.Assert(originalScript == "pc.money_adj(-10000); game.global_vars[21] = 10");
                    pc.AdjustMoney(-10000);
                    SetGlobalVar(21, 10);
                    ;
                    break;
                case 41:
                    Trace.Assert(originalScript == "get_drunk(npc,pc)");
                    get_drunk(npc, pc);
                    break;
                case 71:
                case 72:
                    Trace.Assert(originalScript == "pc.follower_add( npc )");
                    pc.AddFollower(npc);
                    break;
                case 91:
                case 101:
                    Trace.Assert(originalScript == "pc.follower_remove( npc ); run_off( npc,pc )");
                    pc.RemoveFollower(npc);
                    run_off(npc, pc);
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
