
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

[DialogScript(596)]
public class HungousDialog : Hungous, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 11:
            case 21:
            case 31:
            case 41:
                originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 0";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 0;
            case 3:
            case 12:
            case 22:
            case 32:
            case 42:
                originalScript = "pc.skill_level_get(npc,skill_bluff) >= 0";
                return pc.GetSkillLevel(npc, SkillId.bluff) >= 0;
            case 4:
            case 13:
            case 23:
            case 33:
            case 43:
                originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 0";
                return pc.GetSkillLevel(npc, SkillId.intimidate) >= 0;
            case 5:
            case 14:
            case 24:
            case 34:
            case 44:
                originalScript = "pc.skill_level_get(npc,skill_gather_information) >= 0";
                return pc.GetSkillLevel(npc, SkillId.gather_information) >= 0;
            case 61:
                originalScript = "(game.global_vars[980] == 2 or game.global_vars[981] == 2 or game.global_vars[982] == 2 or game.global_vars[983] == 2 or game.global_vars[984] == 2) and pc.skill_level_get(npc,skill_diplomacy) >= 0";
                return (GetGlobalVar(980) == 2 || GetGlobalVar(981) == 2 || GetGlobalVar(982) == 2 || GetGlobalVar(983) == 2 || GetGlobalVar(984) == 2) && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 0;
            case 62:
                originalScript = "(game.global_vars[980] == 2 or game.global_vars[981] == 2 or game.global_vars[982] == 2 or game.global_vars[983] == 2 or game.global_vars[984] == 2) and pc.skill_level_get(npc,skill_bluff) >= 0";
                return (GetGlobalVar(980) == 2 || GetGlobalVar(981) == 2 || GetGlobalVar(982) == 2 || GetGlobalVar(983) == 2 || GetGlobalVar(984) == 2) && pc.GetSkillLevel(npc, SkillId.bluff) >= 0;
            case 63:
                originalScript = "(game.global_vars[980] == 2 or game.global_vars[981] == 2 or game.global_vars[982] == 2 or game.global_vars[983] == 2 or game.global_vars[984] == 2) and pc.skill_level_get(npc,skill_intimidate) >= 0";
                return (GetGlobalVar(980) == 2 || GetGlobalVar(981) == 2 || GetGlobalVar(982) == 2 || GetGlobalVar(983) == 2 || GetGlobalVar(984) == 2) && pc.GetSkillLevel(npc, SkillId.intimidate) >= 0;
            case 64:
                originalScript = "(game.global_vars[980] == 2 or game.global_vars[981] == 2 or game.global_vars[982] == 2 or game.global_vars[983] == 2 or game.global_vars[984] == 2) and pc.skill_level_get(npc,skill_gather_information) >= 0";
                return (GetGlobalVar(980) == 2 || GetGlobalVar(981) == 2 || GetGlobalVar(982) == 2 || GetGlobalVar(983) == 2 || GetGlobalVar(984) == 2) && pc.GetSkillLevel(npc, SkillId.gather_information) >= 0;
            case 65:
                originalScript = "game.global_vars[980] != 2 and game.global_vars[981] != 2 and game.global_vars[982] != 2 and game.global_vars[983] != 2 and game.global_vars[984] != 2 and pc.skill_level_get(npc,skill_diplomacy) >= 0";
                return GetGlobalVar(980) != 2 && GetGlobalVar(981) != 2 && GetGlobalVar(982) != 2 && GetGlobalVar(983) != 2 && GetGlobalVar(984) != 2 && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 0;
            case 66:
                originalScript = "game.global_vars[980] != 2 and game.global_vars[981] != 2 and game.global_vars[982] != 2 and game.global_vars[983] != 2 and game.global_vars[984] != 2 and pc.skill_level_get(npc,skill_bluff) >= 0";
                return GetGlobalVar(980) != 2 && GetGlobalVar(981) != 2 && GetGlobalVar(982) != 2 && GetGlobalVar(983) != 2 && GetGlobalVar(984) != 2 && pc.GetSkillLevel(npc, SkillId.bluff) >= 0;
            case 67:
                originalScript = "game.global_vars[980] != 2 and game.global_vars[981] != 2 and game.global_vars[982] != 2 and game.global_vars[983] != 2 and game.global_vars[984] != 2 and pc.skill_level_get(npc,skill_intimidate) >= 0";
                return GetGlobalVar(980) != 2 && GetGlobalVar(981) != 2 && GetGlobalVar(982) != 2 && GetGlobalVar(983) != 2 && GetGlobalVar(984) != 2 && pc.GetSkillLevel(npc, SkillId.intimidate) >= 0;
            case 68:
                originalScript = "game.global_vars[980] != 2 and game.global_vars[981] != 2 and game.global_vars[982] != 2 and game.global_vars[983] != 2 and game.global_vars[984] != 2 and pc.skill_level_get(npc,skill_gather_information) >= 0";
                return GetGlobalVar(980) != 2 && GetGlobalVar(981) != 2 && GetGlobalVar(982) != 2 && GetGlobalVar(983) != 2 && GetGlobalVar(984) != 2 && pc.GetSkillLevel(npc, SkillId.gather_information) >= 0;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 1:
                originalScript = "game.global_vars[802] = game.global_vars[802] + 1; increment_rep(npc,pc)";
                SetGlobalVar(802, GetGlobalVar(802) + 1);
                increment_rep(npc, pc);
                ;
                break;
            case 10:
            case 20:
                originalScript = "buff_2(npc,pc)";
                buff_2(npc, pc);
                break;
            case 30:
            case 40:
                originalScript = "buff_3(npc,pc)";
                buff_3(npc, pc);
                break;
            case 50:
                originalScript = "buff_4(npc,pc)";
                buff_4(npc, pc);
                break;
            case 51:
                originalScript = "npc.attack( pc )";
                npc.Attack(pc);
                break;
            case 60:
                originalScript = "buff_1(npc,pc)";
                buff_1(npc, pc);
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
            case 2:
            case 11:
            case 21:
            case 31:
            case 41:
            case 61:
            case 65:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 0);
                return true;
            case 3:
            case 12:
            case 22:
            case 32:
            case 42:
            case 62:
            case 66:
                skillChecks = new DialogSkillChecks(SkillId.bluff, 0);
                return true;
            case 4:
            case 13:
            case 23:
            case 33:
            case 43:
            case 63:
            case 67:
                skillChecks = new DialogSkillChecks(SkillId.intimidate, 0);
                return true;
            case 5:
            case 14:
            case 24:
            case 34:
            case 44:
            case 64:
            case 68:
                skillChecks = new DialogSkillChecks(SkillId.gather_information, 0);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}