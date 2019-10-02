
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
    [DialogScript(325)]
    public class LhizeasDialog : Lhizeas, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 41:
                case 42:
                case 71:
                case 72:
                case 201:
                case 202:
                    Trace.Assert(originalScript == "not npc_get(npc,1)");
                    return !ScriptDaemon.npc_get(npc, 1);
                case 43:
                case 44:
                case 73:
                case 74:
                case 81:
                case 82:
                case 203:
                case 204:
                    Trace.Assert(originalScript == "not npc_get(npc,2)");
                    return !ScriptDaemon.npc_get(npc, 2);
                case 45:
                case 46:
                case 75:
                case 76:
                case 83:
                case 84:
                case 205:
                case 206:
                    Trace.Assert(originalScript == "not npc_get(npc,3)");
                    return !ScriptDaemon.npc_get(npc, 3);
                case 47:
                case 48:
                case 77:
                case 78:
                case 85:
                case 86:
                case 207:
                case 208:
                    Trace.Assert(originalScript == "game.party_alignment != LAWFUL_GOOD and game.party_alignment != NEUTRAL_GOOD and game.party_alignment != CHAOTIC_GOOD and game.global_flags[370] == 0");
                    return PartyAlignment != Alignment.LAWFUL_GOOD && PartyAlignment != Alignment.NEUTRAL_GOOD && PartyAlignment != Alignment.CHAOTIC_GOOD && !GetGlobalFlag(370);
                case 115:
                case 116:
                    Trace.Assert(originalScript == "game.party_alignment != LAWFUL_GOOD and game.party_alignment != NEUTRAL_GOOD and game.party_alignment != CHAOTIC_GOOD");
                    return PartyAlignment != Alignment.LAWFUL_GOOD && PartyAlignment != Alignment.NEUTRAL_GOOD && PartyAlignment != Alignment.CHAOTIC_GOOD;
                case 283:
                case 284:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc,skill_intimidate) >= 11 and not npc_get(npc,4)");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 11 && !ScriptDaemon.npc_get(npc, 4);
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
                case 32:
                    Trace.Assert(originalScript == "game.global_vars[56] = 1");
                    SetGlobalVar(56, 1);
                    break;
                case 41:
                case 42:
                case 71:
                case 72:
                case 201:
                case 202:
                    Trace.Assert(originalScript == "npc_set(npc,1)");
                    ScriptDaemon.npc_set(npc, 1);
                    break;
                case 43:
                case 44:
                case 73:
                case 74:
                case 81:
                case 82:
                case 203:
                case 204:
                    Trace.Assert(originalScript == "npc_set(npc,2)");
                    ScriptDaemon.npc_set(npc, 2);
                    break;
                case 45:
                case 46:
                case 75:
                case 76:
                case 83:
                case 84:
                case 205:
                case 206:
                    Trace.Assert(originalScript == "npc_set(npc,3)");
                    ScriptDaemon.npc_set(npc, 3);
                    break;
                case 47:
                case 48:
                case 51:
                case 52:
                case 77:
                case 78:
                case 85:
                case 86:
                case 207:
                case 208:
                    Trace.Assert(originalScript == "game.global_vars[56] = 2");
                    SetGlobalVar(56, 2);
                    break;
                case 121:
                    Trace.Assert(originalScript == "npc.attack( pc )");
                    npc.Attack(pc);
                    break;
                case 271:
                case 272:
                    Trace.Assert(originalScript == "create_item_in_inventory(6654,pc); game.global_flags[370] = 1");
                    Utilities.create_item_in_inventory(6654, pc);
                    SetGlobalFlag(370, true);
                    ;
                    break;
                case 283:
                case 284:
                    Trace.Assert(originalScript == "npc_set(npc,4)");
                    ScriptDaemon.npc_set(npc, 4);
                    break;
                case 311:
                case 312:
                    Trace.Assert(originalScript == "create_item_in_inventory(6654,pc)");
                    Utilities.create_item_in_inventory(6654, pc);
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
                case 283:
                case 284:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 11);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
