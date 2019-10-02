
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
    [DialogScript(154)]
    public class FarmerPrisonerDialog : FarmerPrisoner, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 12:
                    Trace.Assert(originalScript == "not anyone( pc.group_list(), \"has_follower\", 8034 )");
                    return !pc.GetPartyMembers().Any(o => o.HasFollowerByName(8034));
                case 15:
                case 16:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_follower\", 8034 )");
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8034));
                case 111:
                case 112:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_diplomacy) >= 7");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 7;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    Trace.Assert(originalScript == "banter(npc,pc,20)");
                    banter(npc, pc, 20);
                    break;
                case 13:
                case 14:
                case 91:
                    Trace.Assert(originalScript == "eat_in_three( npc, pc )");
                    eat_in_three(npc, pc);
                    break;
                case 21:
                case 22:
                    Trace.Assert(originalScript == "banter(npc,pc,30)");
                    banter(npc, pc, 30);
                    break;
                case 23:
                case 24:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 31:
                case 82:
                case 92:
                case 93:
                case 101:
                case 123:
                case 124:
                case 191:
                case 192:
                case 203:
                case 204:
                    Trace.Assert(originalScript == "game.global_flags[169] = 1; run_off( npc, pc )");
                    SetGlobalFlag(169, true);
                    run_off(npc, pc);
                    ;
                    break;
                case 51:
                    Trace.Assert(originalScript == "run_off( npc, pc )");
                    run_off(npc, pc);
                    break;
                case 160:
                    Trace.Assert(originalScript == "game.global_flags[171] = 1");
                    SetGlobalFlag(171, true);
                    break;
                case 170:
                    Trace.Assert(originalScript == "game.story_state = 6");
                    StoryState = 6;
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
                case 111:
                case 112:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 7);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
