
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
    [DialogScript(395)]
    public class GhostDialog : Ghost, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                    originalScript = "dump_parts( npc, pc )";
                    dump_parts(npc, pc);
                    break;
                case 30:
                    originalScript = "game.quests[83].state = qs_botched; game.global_vars[696] = 10";
                    SetQuestState(83, QuestState.Botched);
                    SetGlobalVar(696, 10);
                    ;
                    break;
                case 71:
                case 292:
                    originalScript = "go_ghost( npc, pc )";
                    go_ghost(npc, pc);
                    break;
                case 191:
                    originalScript = "game.quests[83].state = qs_mentioned; go_ghost( npc, pc )";
                    SetQuestState(83, QuestState.Mentioned);
                    go_ghost(npc, pc);
                    ;
                    break;
                case 270:
                case 290:
                    originalScript = "game.global_vars[696] = game.global_vars[696]";
                    SetGlobalVar(696, GetGlobalVar(696));
                    break;
                case 272:
                    originalScript = "game.timevent_add(go_ghost,( npc, pc ), 500, 1)";
                    StartTimer(500, () => Ghost.go_ghost(npc, pc), true);
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
