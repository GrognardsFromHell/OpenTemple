
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
    [DialogScript(200)]
    public class ZaxisDialog : Zaxis, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 31:
                case 32:
                case 33:
                case 34:
                    originalScript = "npc.leader_get() == OBJ_HANDLE_NULL";
                    return npc.GetLeader() == null;
                case 35:
                    originalScript = "npc.leader_get() != OBJ_HANDLE_NULL";
                    return npc.GetLeader() != null;
                case 51:
                case 52:
                    originalScript = "not pc.follower_atmax()";
                    return !pc.HasMaxFollowers();
                case 53:
                case 54:
                    originalScript = "pc.follower_atmax()";
                    return pc.HasMaxFollowers();
                case 112:
                    originalScript = "game.story_state == 6";
                    return StoryState == 6;
                case 113:
                    originalScript = "game.story_state >= 3";
                    return StoryState >= 3;
                case 114:
                    originalScript = "game.quests[18].state > qs_unknown";
                    return GetQuestState(18) > QuestState.Unknown;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 60:
                    originalScript = "pc.follower_add(npc)";
                    pc.AddFollower(npc);
                    break;
                case 71:
                    originalScript = "zaxis_runs_off(npc,pc)";
                    zaxis_runs_off(npc, pc);
                    break;
                case 91:
                case 92:
                    originalScript = "pc.follower_remove(npc)";
                    pc.RemoveFollower(npc);
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
