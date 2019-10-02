
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
    [DialogScript(142)]
    public class SenshockDialog : Senshock, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 21:
                case 22:
                    Trace.Assert(originalScript == "game.global_flags[145] == 1 and game.global_flags[146] == 0 and pc.skill_level_get(npc, skill_bluff) >= 12");
                    return GetGlobalFlag(145) && !GetGlobalFlag(146) && pc.GetSkillLevel(npc, SkillId.bluff) >= 12;
                case 111:
                case 112:
                    Trace.Assert(originalScript == "game.global_flags[146] == 0");
                    return !GetGlobalFlag(146);
                case 113:
                case 114:
                    Trace.Assert(originalScript == "game.global_flags[146] == 1");
                    return GetGlobalFlag(146);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 31:
                case 161:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 101:
                    Trace.Assert(originalScript == "senshock_kills_hedrack(npc,pc)");
                    senshock_kills_hedrack(npc, pc);
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
                case 21:
                case 22:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 12);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
