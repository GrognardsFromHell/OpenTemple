
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
    [DialogScript(106)]
    public class MaryDialog : Mary, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "game.global_flags[79] == 0";
                    return !GetGlobalFlag(79);
                case 4:
                case 5:
                    originalScript = "game.global_flags[79] == 1";
                    return GetGlobalFlag(79);
                case 7:
                case 8:
                    originalScript = "game.quests[98].state == qs_accepted and game.global_flags[79] == 1 and game.global_vars[697] != 6";
                    return GetQuestState(98) == QuestState.Accepted && GetGlobalFlag(79) && GetGlobalVar(697) != 6;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    originalScript = "game.global_vars[697] = 5";
                    SetGlobalVar(697, 5);
                    break;
                case 4:
                case 5:
                    originalScript = "game.global_flags[79] = 0; game.global_vars[697] = 5";
                    SetGlobalFlag(79, false);
                    SetGlobalVar(697, 5);
                    ;
                    break;
                case 7:
                case 8:
                    originalScript = "game.global_flags[79] = 1; game.global_vars[697] = 6";
                    SetGlobalFlag(79, true);
                    SetGlobalVar(697, 6);
                    ;
                    break;
                case 20:
                    originalScript = "pc.condition_add_with_args(\"Fallen_Paladin\",0,0)";
                    pc.AddCondition("Fallen_Paladin", 0, 0);
                    break;
                case 21:
                case 31:
                    originalScript = "game.fade(28800,4047,0,4)";
                    Fade(28800, 4047, 0, 4);
                    break;
                case 40:
                    originalScript = "create_item_in_inventory(12895,pc)";
                    Utilities.create_item_in_inventory(12895, pc);
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
