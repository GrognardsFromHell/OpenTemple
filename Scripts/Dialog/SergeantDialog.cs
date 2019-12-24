
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Dialog
{
    [DialogScript(77)]
    public class SergeantDialog : Sergeant, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 4:
                case 5:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 5 and anyone(game.party[0].group_list(), \"has_wielded\", 3005)";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 5 && PartyLeader.GetPartyMembers().Any(o => o.HasEquippedByName(3005));
                case 6:
                    originalScript = "pc != game.party[0] and game.party[0].distance_to(pc) <= 40 and not critter_is_unconscious(game.party[0]) and anyone(game.party[0].group_list(), \"has_wielded\", 3005)";
                    return pc != PartyLeader && PartyLeader.DistanceTo(pc) <= 40 && !Utilities.critter_is_unconscious(PartyLeader) && PartyLeader.GetPartyMembers().Any(o => o.HasEquippedByName(3005));
                case 12:
                case 13:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 6";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 6;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 6:
                    originalScript = "call_leader(npc, pc)";
                    call_leader(npc, pc);
                    break;
                case 11:
                case 27:
                case 28:
                case 53:
                case 62:
                case 71:
                case 72:
                    originalScript = "move_pc( npc, pc )";
                    move_pc(npc, pc);
                    break;
                case 26:
                    originalScript = "game.global_flags[48] = 1; deliver_pc( npc, pc )";
                    SetGlobalFlag(48, true);
                    deliver_pc(npc, pc);
                    ;
                    break;
                case 30:
                    originalScript = "game.global_flags[363] = 1";
                    SetGlobalFlag(363, true);
                    break;
                case 31:
                    originalScript = "run_off( npc, pc ); real_time_regroup()";
                    run_off(npc, pc);
                    real_time_regroup();
                    ;
                    break;
                case 41:
                case 73:
                case 74:
                    originalScript = "game.global_flags[363] = 1; npc.attack( pc )";
                    SetGlobalFlag(363, true);
                    npc.Attack(pc);
                    ;
                    break;
                case 50:
                    originalScript = "game.global_flags[49] = 1";
                    SetGlobalFlag(49, true);
                    break;
                case 81:
                    originalScript = "deliver_pc( npc, pc )";
                    deliver_pc(npc, pc);
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
