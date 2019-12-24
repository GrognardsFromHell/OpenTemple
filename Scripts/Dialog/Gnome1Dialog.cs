
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
    [DialogScript(78)]
    public class Gnome1Dialog : Gnome1, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 4:
                case 7:
                    originalScript = "game.global_flags[54] == 1";
                    return GetGlobalFlag(54);
                case 21:
                case 22:
                    originalScript = "game.global_flags[59] == 1";
                    return GetGlobalFlag(59);
                case 23:
                case 24:
                    originalScript = "game.global_flags[59] == 0";
                    return !GetGlobalFlag(59);
                case 45:
                case 46:
                    originalScript = "(anyone(pc.group_list(),\"has_wielded\",3005))";
                    return (pc.GetPartyMembers().Any(o => o.HasEquippedByName(3005)));
                case 47:
                case 48:
                    originalScript = "not (anyone(pc.group_list(),\"has_wielded\",3005))";
                    return !(pc.GetPartyMembers().Any(o => o.HasEquippedByName(3005)));
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 40:
                    originalScript = "game.global_flags[55] = 1";
                    SetGlobalFlag(55, true);
                    break;
                case 41:
                case 42:
                    originalScript = "npc.item_transfer_to(pc,3007); run_off(npc,pc); game.global_flags[991] = 1";
                    npc.TransferItemByNameTo(pc, 3007);
                    run_off(npc, pc);
                    SetGlobalFlag(991, true);
                    ;
                    break;
                case 43:
                case 44:
                case 111:
                case 112:
                case 121:
                case 122:
                    originalScript = "run_off(npc,pc); game.global_flags[991] = 1";
                    run_off(npc, pc);
                    SetGlobalFlag(991, true);
                    ;
                    break;
                case 45:
                case 46:
                case 47:
                case 48:
                    originalScript = "npc.item_transfer_to(pc,3007)";
                    npc.TransferItemByNameTo(pc, 3007);
                    break;
                case 91:
                    originalScript = "run_off(npc,pc)";
                    run_off(npc, pc);
                    break;
                case 102:
                case 103:
                    originalScript = "game.global_flags[55] = 1; run_off(npc,pc); game.global_flags[991] = 1";
                    SetGlobalFlag(55, true);
                    run_off(npc, pc);
                    SetGlobalFlag(991, true);
                    ;
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
