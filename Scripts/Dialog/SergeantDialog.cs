
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
    [DialogScript(77)]
    public class SergeantDialog : Sergeant, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 4:
                case 5:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_bluff) >= 5 and anyone(game.party[0].group_list(), \"has_wielded\", 3005)");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 5 && PartyLeader.GetPartyMembers().Any(o => o.HasEquippedByName(3005));
                case 6:
                    Trace.Assert(originalScript == "pc != game.party[0] and game.party[0].distance_to(pc) <= 40 and not critter_is_unconscious(game.party[0]) and anyone(game.party[0].group_list(), \"has_wielded\", 3005)");
                    return pc != PartyLeader && PartyLeader.DistanceTo(pc) <= 40 && !Utilities.critter_is_unconscious(PartyLeader) && PartyLeader.GetPartyMembers().Any(o => o.HasEquippedByName(3005));
                case 12:
                case 13:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 6");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 6;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 6:
                    Trace.Assert(originalScript == "call_leader(npc, pc)");
                    call_leader(npc, pc);
                    break;
                case 11:
                case 27:
                case 28:
                case 53:
                case 62:
                case 71:
                case 72:
                    Trace.Assert(originalScript == "move_pc( npc, pc )");
                    move_pc(npc, pc);
                    break;
                case 26:
                    Trace.Assert(originalScript == "game.global_flags[48] = 1; deliver_pc( npc, pc )");
                    SetGlobalFlag(48, true);
                    deliver_pc(npc, pc);
                    ;
                    break;
                case 30:
                    Trace.Assert(originalScript == "game.global_flags[363] = 1");
                    SetGlobalFlag(363, true);
                    break;
                case 31:
                    Trace.Assert(originalScript == "run_off( npc, pc ); real_time_regroup()");
                    run_off(npc, pc);
                    real_time_regroup();
                    ;
                    break;
                case 41:
                case 73:
                case 74:
                    Trace.Assert(originalScript == "game.global_flags[363] = 1; npc.attack( pc )");
                    SetGlobalFlag(363, true);
                    npc.Attack(pc);
                    ;
                    break;
                case 50:
                    Trace.Assert(originalScript == "game.global_flags[49] = 1");
                    SetGlobalFlag(49, true);
                    break;
                case 81:
                    Trace.Assert(originalScript == "deliver_pc( npc, pc )");
                    deliver_pc(npc, pc);
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
                case 4:
                case 5:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 5);
                    return true;
                case 12:
                case 13:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 6);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
