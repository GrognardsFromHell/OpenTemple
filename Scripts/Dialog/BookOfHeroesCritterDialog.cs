
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
    [DialogScript(522)]
    public class BookOfHeroesCritterDialog : BookOfHeroesCritter, IDialogScript
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
                case 3:
                case 12:
                case 22:
                case 42:
                case 51:
                case 61:
                    Trace.Assert(originalScript == "npc.destroy()");
                    npc.Destroy();
                    break;
                case 31:
                    Trace.Assert(originalScript == "npc.destroy(); game.global_vars[994] = 0; game.global_vars[988] = npc.area; game.fade_and_teleport(0,0,0,5119,504,468)");
                    npc.Destroy();
                    SetGlobalVar(994, 0);
                    SetGlobalVar(988, npc.GetArea());
                    FadeAndTeleport(0, 0, 0, 5119, 504, 468);
                    ;
                    break;
                case 41:
                    Trace.Assert(originalScript == "npc.destroy(); game.global_vars[988] = npc.area; game.fade_and_teleport(0,0,0,5119,504,468)");
                    npc.Destroy();
                    SetGlobalVar(988, npc.GetArea());
                    FadeAndTeleport(0, 0, 0, 5119, 504, 468);
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
