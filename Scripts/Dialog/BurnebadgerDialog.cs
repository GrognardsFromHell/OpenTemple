
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
    [DialogScript(212)]
    public class BurnebadgerDialog : Burnebadger, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 31:
                case 35:
                    Trace.Assert(originalScript == "find_npc_near(npc,8054) == OBJ_HANDLE_NULL and (npc.map < 5014 or npc.map > 5019)");
                    return Utilities.find_npc_near(npc, 8054) == null && (npc.GetMap() < 5014 || npc.GetMap() > 5019);
                case 32:
                case 36:
                    Trace.Assert(originalScript == "find_npc_near(npc,8054) == OBJ_HANDLE_NULL and npc.map >= 5014 and npc.map <= 5019");
                    return Utilities.find_npc_near(npc, 8054) == null && npc.GetMap() >= 5014 && npc.GetMap() <= 5019;
                case 33:
                case 37:
                    Trace.Assert(originalScript == "find_npc_near(npc,8054) != OBJ_HANDLE_NULL");
                    return Utilities.find_npc_near(npc, 8054) != null;
                case 71:
                    Trace.Assert(originalScript == "npc.map < 5006 or npc.map > 5008");
                    return npc.GetMap() < 5006 || npc.GetMap() > 5008;
                case 72:
                    Trace.Assert(originalScript == "npc.map < 5011 or npc.map > 5013");
                    return npc.GetMap() < 5011 || npc.GetMap() > 5013;
                case 73:
                    Trace.Assert(originalScript == "npc.map < 5014 or npc.map > 5019");
                    return npc.GetMap() < 5014 || npc.GetMap() > 5019;
                case 201:
                case 202:
                    Trace.Assert(originalScript == "game.global_flags[842] == 1");
                    return GetGlobalFlag(842);
                case 203:
                case 204:
                    Trace.Assert(originalScript == "game.global_flags[842] == 0");
                    return !GetGlobalFlag(842);
                case 205:
                case 206:
                    Trace.Assert(originalScript == "game.global_flags[842] == 1 and game.global_flags[837] == 0 and pc.skill_level_get(npc, skill_bluff) >= 8");
                    return GetGlobalFlag(842) && !GetGlobalFlag(837) && pc.GetSkillLevel(npc, SkillId.bluff) >= 8;
                case 301:
                case 302:
                    Trace.Assert(originalScript == "npc.has_met(pc)");
                    return npc.HasMet(pc);
                case 303:
                case 304:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_gather_information) >= 8 and game.global_flags[842] == 0");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 8 && !GetGlobalFlag(842);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 219:
                case 221:
                case 222:
                    Trace.Assert(originalScript == "game.fade_and_teleport(0,0,0,5016,483,473)");
                    FadeAndTeleport(0, 0, 0, 5016, 483, 473);
                    break;
                case 230:
                    Trace.Assert(originalScript == "game.global_flags[845] = 1; npc.attack( pc )");
                    SetGlobalFlag(845, true);
                    npc.Attack(pc);
                    ;
                    break;
                case 240:
                case 320:
                    Trace.Assert(originalScript == "game.global_flags[842] = 1");
                    SetGlobalFlag(842, true);
                    break;
                case 251:
                    Trace.Assert(originalScript == "game.fade_and_teleport(0,0,0,5014,472,479)");
                    FadeAndTeleport(0, 0, 0, 5014, 472, 479);
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
                case 205:
                case 206:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 8);
                    return true;
                case 303:
                case 304:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 8);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}