
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
    [DialogScript(302)]
    public class BrigandDialog : Brigand, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 71:
                case 72:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_bluff) >= 7");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 7;
                case 271:
                case 272:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_bluff) >= 5");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 5;
                case 283:
                case 284:
                case 393:
                case 394:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_bluff) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 10;
                case 395:
                case 396:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 10;
                case 397:
                case 398:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_diplomacy) >= 10");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 10;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 68:
                case 168:
                case 268:
                case 368:
                case 468:
                    Trace.Assert(originalScript == "buff_npc(npc,pc)");
                    buff_npc(npc, pc);
                    break;
                case 70:
                case 170:
                case 270:
                case 370:
                case 470:
                    Trace.Assert(originalScript == "buff_npc_two(npc,pc); game.global_flags[833] = 1");
                    buff_npc_two(npc, pc);
                    SetGlobalFlag(833, true);
                    ;
                    break;
                case 73:
                case 83:
                case 91:
                case 173:
                case 183:
                case 191:
                case 273:
                case 285:
                case 373:
                case 383:
                case 391:
                case 392:
                case 399:
                case 405:
                case 471:
                case 505:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
                    break;
                case 80:
                case 180:
                case 280:
                case 380:
                    Trace.Assert(originalScript == "buff_npc_three(npc,pc)");
                    buff_npc_three(npc, pc);
                    break;
                case 402:
                case 403:
                case 404:
                case 502:
                case 503:
                case 504:
                    Trace.Assert(originalScript == "game.global_flags[835] = 1; game.fade_and_teleport(0,0,0,5111,479,490)");
                    SetGlobalFlag(835, true);
                    FadeAndTeleport(0, 0, 0, 5111, 479, 490);
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
                case 71:
                case 72:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 7);
                    return true;
                case 271:
                case 272:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 5);
                    return true;
                case 283:
                case 284:
                case 393:
                case 394:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 10);
                    return true;
                case 395:
                case 396:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 10);
                    return true;
                case 397:
                case 398:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
