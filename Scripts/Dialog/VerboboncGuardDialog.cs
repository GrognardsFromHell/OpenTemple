
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
    [DialogScript(385)]
    public class VerboboncGuardDialog : VerboboncGuard, IDialogScript
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
                case 1:
                    Trace.Assert(originalScript == "guard_backup(npc,pc)");
                    guard_backup(npc, pc);
                    break;
                case 2:
                case 32:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 30:
                    Trace.Assert(originalScript == "game.global_vars[969] = 1");
                    SetGlobalVar(969, 1);
                    break;
                case 40:
                    Trace.Assert(originalScript == "game.global_vars[969] = 2");
                    SetGlobalVar(969, 2);
                    break;
                case 41:
                    Trace.Assert(originalScript == "game.fade_and_teleport(0,0,0,5172,471,489)");
                    FadeAndTeleport(0, 0, 0, 5172, 471, 489);
                    break;
                case 61:
                    Trace.Assert(originalScript == "execution(npc,pc)");
                    execution(npc, pc);
                    break;
                case 81:
                    Trace.Assert(originalScript == "game.global_flags[260] = 1; game.fade_and_teleport(0,0,0,5121,228,507)");
                    SetGlobalFlag(260, true);
                    FadeAndTeleport(0, 0, 0, 5121, 228, 507);
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
