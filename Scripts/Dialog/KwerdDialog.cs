
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Dialog
{
    [DialogScript(504)]
    public class KwerdDialog : Kwerd, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 31:
                    originalScript = "not get_1(npc)";
                    return !Scripts.get_1(npc);
                case 32:
                    originalScript = "not get_2(npc)";
                    return !Scripts.get_2(npc);
                case 33:
                    originalScript = "game.global_vars[990] == 1";
                    return GetGlobalVar(990) == 1;
                case 34:
                    originalScript = "game.global_vars[991] == 1";
                    return GetGlobalVar(991) == 1;
                case 35:
                    originalScript = "game.global_vars[990] == 3";
                    return GetGlobalVar(990) == 3;
                case 36:
                    originalScript = "game.global_vars[991] == 3";
                    return GetGlobalVar(991) == 3;
                case 37:
                    originalScript = "game.global_vars[989] == 1";
                    return GetGlobalVar(989) == 1;
                case 38:
                    originalScript = "game.global_vars[989] == 2";
                    return GetGlobalVar(989) == 2;
                case 39:
                    originalScript = "game.global_vars[989] == 3";
                    return GetGlobalVar(989) == 3;
                case 40:
                    originalScript = "game.global_vars[989] == 4";
                    return GetGlobalVar(989) == 4;
                case 41:
                    originalScript = "game.global_vars[989] == 5";
                    return GetGlobalVar(989) == 5;
                case 42:
                    originalScript = "game.global_flags[976] == 1";
                    return GetGlobalFlag(976);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 22:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 31:
                    originalScript = "npc_1(npc)";
                    Scripts.npc_1(npc);
                    break;
                case 32:
                    originalScript = "npc_2(npc)";
                    Scripts.npc_2(npc);
                    break;
                case 60:
                    originalScript = "game.global_vars[990] = 2";
                    SetGlobalVar(990, 2);
                    break;
                case 70:
                    originalScript = "game.global_vars[991] = 2";
                    SetGlobalVar(991, 2);
                    break;
                case 120:
                    originalScript = "game.global_vars[990] = 4";
                    SetGlobalVar(990, 4);
                    break;
                case 130:
                    originalScript = "game.global_vars[991] = 4";
                    SetGlobalVar(991, 4);
                    break;
                case 151:
                    originalScript = "run_off(npc,pc)";
                    run_off(npc, pc);
                    break;
                case 160:
                    originalScript = "game.global_vars[989] = 6";
                    SetGlobalVar(989, 6);
                    break;
                default:
                    originalScript = null;
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
