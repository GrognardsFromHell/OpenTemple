
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
    [DialogScript(182)]
    public class AssassinDialog : Assassin, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 3:
                case 6:
                case 23:
                case 26:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_diplomacy) >= 3");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 3;
                case 4:
                case 7:
                case 22:
                case 25:
                case 32:
                case 34:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 13");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 13;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 41:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 30:
                    Trace.Assert(originalScript == "game.global_flags[292] = 1");
                    SetGlobalFlag(292, true);
                    break;
                case 51:
                    Trace.Assert(originalScript == "run_off(npc,pc)");
                    run_off(npc, pc);
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
                case 3:
                case 6:
                case 23:
                case 26:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 3);
                    return true;
                case 4:
                case 7:
                case 22:
                case 25:
                case 32:
                case 34:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 13);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
