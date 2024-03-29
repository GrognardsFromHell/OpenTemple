
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

namespace VanillaScripts.Dialog;

[DialogScript(121)]
public class TowerSentinelDialog : TowerSentinel, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 8:
                originalScript = "game.global_flags[91] == 1";
                return GetGlobalFlag(91);
            case 3:
            case 9:
                originalScript = "game.global_flags[92] == 1";
                return GetGlobalFlag(92);
            case 66:
            case 67:
                originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 6";
                return pc.GetSkillLevel(npc, SkillId.intimidate) >= 6;
            case 68:
            case 69:
                originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 5";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 5;
            case 82:
            case 83:
                originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 9";
                return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 9;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 4:
            case 5:
            case 63:
            case 64:
                originalScript = "npc.attack( pc )";
                npc.Attack(pc);
                break;
            case 33:
            case 34:
                originalScript = "game.global_flags[287] = 1; game.fade_and_teleport( 1800, 0, 0, 5067, 424, 489 ); run_off( npc, pc )";
                SetGlobalFlag(287, true);
                FadeAndTeleport(1800, 0, 0, 5067, 424, 489);
                run_off(npc, pc);
                ;
                break;
            case 43:
            case 44:
                originalScript = "game.global_flags[287] = 1; game.fade_and_teleport( 1800, 0, 0, 5066, 445, 446 ); run_off( npc, pc )";
                SetGlobalFlag(287, true);
                FadeAndTeleport(1800, 0, 0, 5066, 445, 446);
                run_off(npc, pc);
                ;
                break;
            case 111:
                originalScript = "talk_lareth(npc,pc,310)";
                talk_lareth(npc, pc, 310);
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
            case 66:
            case 67:
                skillChecks = new DialogSkillChecks(SkillId.intimidate, 6);
                return true;
            case 68:
            case 69:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 5);
                return true;
            case 82:
            case 83:
                skillChecks = new DialogSkillChecks(SkillId.sense_motive, 9);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}