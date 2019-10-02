
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
    [DialogScript(201)]
    public class IkianDialog : Ikian, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 33:
                case 34:
                    Trace.Assert(originalScript == "game.story_state == 1");
                    return StoryState == 1;
                case 35:
                case 36:
                    Trace.Assert(originalScript == "game.story_state == 2");
                    return StoryState == 2;
                case 37:
                case 38:
                    Trace.Assert(originalScript == "game.story_state == 3");
                    return StoryState == 3;
                case 39:
                case 40:
                    Trace.Assert(originalScript == "game.story_state == 4");
                    return StoryState == 4;
                case 41:
                case 42:
                    Trace.Assert(originalScript == "game.story_state == 5");
                    return StoryState == 5;
                case 43:
                case 44:
                    Trace.Assert(originalScript == "game.story_state == 6");
                    return StoryState == 6;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 10:
                case 20:
                case 30:
                case 50:
                case 60:
                case 70:
                case 80:
                    Trace.Assert(originalScript == "buff_npc( npc, pc )");
                    buff_npc(npc, pc);
                    break;
                case 53:
                    Trace.Assert(originalScript == "all_run_off(npc,pc)");
                    all_run_off(npc, pc);
                    break;
                case 81:
                    Trace.Assert(originalScript == "npc.attack(pc); pc.condition_add_with_args(\"Fallen_Paladin\",0,0)");
                    npc.Attack(pc);
                    pc.AddCondition("Fallen_Paladin", 0, 0);
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
