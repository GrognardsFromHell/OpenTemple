
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
    [DialogScript(137)]
    public class ManAtArmsPrisoner2Dialog : ManAtArmsPrisoner2, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 71:
                case 72:
                case 73:
                case 74:
                case 131:
                case 132:
                    Trace.Assert(originalScript == "(npc.leader_get() == OBJ_HANDLE_NULL)");
                    return (npc.GetLeader() == null);
                case 75:
                case 76:
                    Trace.Assert(originalScript == "(npc.leader_get() != OBJ_HANDLE_NULL)");
                    return (npc.GetLeader() != null);
                case 133:
                case 134:
                    Trace.Assert(originalScript == "(npc.leader_get() != OBJ_HANDLE_NULL) and game.global_flags[138] == 1");
                    return (npc.GetLeader() != null) && GetGlobalFlag(138);
                case 135:
                case 136:
                    Trace.Assert(originalScript == "(npc.leader_get() != OBJ_HANDLE_NULL) and game.global_flags[138] == 0");
                    return (npc.GetLeader() != null) && !GetGlobalFlag(138);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 151:
                case 152:
                    Trace.Assert(originalScript == "pc.follower_remove(npc)");
                    pc.RemoveFollower(npc);
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
