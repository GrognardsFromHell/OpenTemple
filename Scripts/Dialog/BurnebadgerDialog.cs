
using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObjects;
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

namespace Scripts.Dialog;

[DialogScript(212)]
public class BurnebadgerDialog : Burnebadger, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 31:
            case 35:
                originalScript = "find_npc_near(npc,8054) == OBJ_HANDLE_NULL and (npc.map < 5014 or npc.map > 5019)";
                return Utilities.find_npc_near(npc, 8054) == null && (npc.GetMap() < 5014 || npc.GetMap() > 5019);
            case 32:
            case 36:
                originalScript = "find_npc_near(npc,8054) == OBJ_HANDLE_NULL and npc.map >= 5014 and npc.map <= 5019";
                return Utilities.find_npc_near(npc, 8054) == null && npc.GetMap() >= 5014 && npc.GetMap() <= 5019;
            case 33:
            case 37:
                originalScript = "find_npc_near(npc,8054) != OBJ_HANDLE_NULL";
                return Utilities.find_npc_near(npc, 8054) != null;
            case 71:
                originalScript = "npc.map < 5006 or npc.map > 5008";
                return npc.GetMap() < 5006 || npc.GetMap() > 5008;
            case 72:
                originalScript = "npc.map < 5011 or npc.map > 5013";
                return npc.GetMap() < 5011 || npc.GetMap() > 5013;
            case 73:
                originalScript = "npc.map < 5014 or npc.map > 5019";
                return npc.GetMap() < 5014 || npc.GetMap() > 5019;
            case 201:
            case 202:
                originalScript = "game.global_flags[842] == 1";
                return GetGlobalFlag(842);
            case 203:
            case 204:
                originalScript = "game.global_flags[842] == 0";
                return !GetGlobalFlag(842);
            case 205:
            case 206:
                originalScript = "game.global_flags[842] == 1 and game.global_flags[837] == 0 and pc.skill_level_get(npc, skill_bluff) >= 8";
                return GetGlobalFlag(842) && !GetGlobalFlag(837) && pc.GetSkillLevel(npc, SkillId.bluff) >= 8;
            case 301:
            case 302:
                originalScript = "npc.has_met(pc)";
                return npc.HasMet(pc);
            case 303:
            case 304:
                originalScript = "pc.skill_level_get(npc, skill_gather_information) >= 8 and game.global_flags[842] == 0";
                return pc.GetSkillLevel(npc, SkillId.gather_information) >= 8 && !GetGlobalFlag(842);
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 219:
            case 221:
            case 222:
                originalScript = "game.fade_and_teleport(0,0,0,5016,483,473)";
                FadeAndTeleport(0, 0, 0, 5016, 483, 473);
                break;
            case 230:
                originalScript = "game.global_flags[845] = 1; npc.attack( pc )";
                SetGlobalFlag(845, true);
                npc.Attack(pc);
                ;
                break;
            case 240:
            case 320:
                originalScript = "game.global_flags[842] = 1";
                SetGlobalFlag(842, true);
                break;
            case 251:
                originalScript = "game.fade_and_teleport(0,0,0,5014,472,479)";
                FadeAndTeleport(0, 0, 0, 5014, 472, 479);
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