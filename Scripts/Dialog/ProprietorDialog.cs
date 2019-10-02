
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
    [DialogScript(333)]
    public class ProprietorDialog : Proprietor, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    Trace.Assert(originalScript == "pc.money_get() >= 80000");
                    return pc.GetMoney() >= 80000;
                case 3:
                    Trace.Assert(originalScript == "pc.money_get() <= 79900");
                    return pc.GetMoney() <= 79900;
                case 11:
                    Trace.Assert(originalScript == "game.global_flags[967] == 0 and pc.money_get() >= 80000");
                    return !GetGlobalFlag(967) && pc.GetMoney() >= 80000;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 20:
                    Trace.Assert(originalScript == "game.global_flags[967] = 1");
                    SetGlobalFlag(967, true);
                    break;
                case 21:
                    Trace.Assert(originalScript == "pc.money_adj(-80000)");
                    pc.AdjustMoney(-80000);
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
