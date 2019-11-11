
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
    [DialogScript(130)]
    public class SailorPrisonerDialog : SailorPrisoner, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 51:
                case 52:
                    originalScript = "not pc.follower_atmax()";
                    return !pc.HasMaxFollowers();
                case 53:
                case 54:
                    originalScript = "pc.follower_atmax()";
                    return pc.HasMaxFollowers();
                case 93:
                case 94:
                    originalScript = "game.global_flags[88] == 1";
                    return GetGlobalFlag(88);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                    originalScript = "game.global_vars[124] = 1";
                    SetGlobalVar(124, 1);
                    break;
                case 2:
                case 3:
                case 20:
                    originalScript = "get_rep( npc, pc )";
                    get_rep(npc, pc);
                    break;
                case 60:
                    originalScript = "pc.follower_add(npc)";
                    pc.AddFollower(npc);
                    break;
                case 100:
                case 110:
                    originalScript = "pc.follower_remove(npc)";
                    pc.RemoveFollower(npc);
                    break;
                case 101:
                case 111:
                    originalScript = "run_off(npc,pc)";
                    run_off(npc, pc);
                    break;
                case 22000:
                    originalScript = "game.global_vars[910] = 32";
                    SetGlobalVar(910, 32);
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
