
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
    [DialogScript(147)]
    public class BarkinarDialog : Barkinar, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 23:
                case 24:
                case 73:
                case 74:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_diplomacy) >= 12");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 12;
                case 75:
                case 76:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_follower\", 8001 ) and pc.skill_level_get(npc, skill_diplomacy) >= 12");
                    return pc.GetPartyMembers().Any(o => o.HasFollowerByName(8001)) && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 12;
                case 81:
                case 82:
                    Trace.Assert(originalScript == "(game.global_flags[159] == 0 and pc.skill_level_get(npc, skill_bluff) >= 4) or game.global_flags[166] == 1");
                    return (!GetGlobalFlag(159) && pc.GetSkillLevel(npc, SkillId.bluff) >= 4) || GetGlobalFlag(166);
                case 83:
                case 84:
                    Trace.Assert(originalScript == "game.global_flags[159] == 1 and game.global_flags[166] == 0");
                    return GetGlobalFlag(159) && !GetGlobalFlag(166);
                case 85:
                case 86:
                    Trace.Assert(originalScript == "game.global_flags[159] == 0 and game.global_flags[166] == 0");
                    return !GetGlobalFlag(159) && !GetGlobalFlag(166);
                case 161:
                case 162:
                    Trace.Assert(originalScript == "game.global_flags[149] == 1");
                    return GetGlobalFlag(149);
                case 163:
                case 164:
                    Trace.Assert(originalScript == "game.global_flags[149] == 0");
                    return !GetGlobalFlag(149);
                case 192:
                case 226:
                    Trace.Assert(originalScript == "game.global_flags[161] == 1");
                    return GetGlobalFlag(161);
                case 193:
                case 194:
                    Trace.Assert(originalScript == "game.global_flags[161] == 0");
                    return !GetGlobalFlag(161);
                case 225:
                    Trace.Assert(originalScript == "game.global_flags[146] == 1 and game.global_flags[147] == 1");
                    return GetGlobalFlag(146) && GetGlobalFlag(147);
                case 227:
                case 228:
                    Trace.Assert(originalScript == "game.global_flags[166] == 1");
                    return GetGlobalFlag(166);
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
                case 3:
                case 13:
                case 14:
                case 15:
                case 16:
                    Trace.Assert(originalScript == "banter (npc, pc, 20)");
                    banter(npc, pc, 20);
                    break;
                case 6:
                case 7:
                case 11:
                case 12:
                case 21:
                case 22:
                case 71:
                case 72:
                case 111:
                case 112:
                case 141:
                case 142:
                case 151:
                case 221:
                case 222:
                case 241:
                case 242:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 50:
                    Trace.Assert(originalScript == "game.global_flags[157] = 1");
                    SetGlobalFlag(157, true);
                    break;
                case 80:
                    Trace.Assert(originalScript == "game.global_flags[158] = 1");
                    SetGlobalFlag(158, true);
                    break;
                case 90:
                    Trace.Assert(originalScript == "game.global_flags[162] = 1");
                    SetGlobalFlag(162, true);
                    break;
                case 143:
                case 144:
                case 171:
                case 172:
                    Trace.Assert(originalScript == "game.global_flags[163] = 1");
                    SetGlobalFlag(163, true);
                    break;
                case 170:
                    Trace.Assert(originalScript == "npc.item_transfer_to_by_proto(pc,6137)");
                    npc.TransferItemByProtoTo(pc, 6137);
                    break;
                case 227:
                case 228:
                    Trace.Assert(originalScript == "banter2 (npc, pc, 240)");
                    banter2(npc, pc, 240);
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
                case 23:
                case 24:
                case 73:
                case 74:
                case 75:
                case 76:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 12);
                    return true;
                case 81:
                case 82:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 4);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
