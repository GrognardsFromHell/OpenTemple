
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

[DialogScript(393)]
public class NightWatchmanDialog : NightWatchman, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 11:
                originalScript = "not is_daytime()";
                return !Utilities.is_daytime();
            case 12:
                originalScript = "is_daytime()";
                return Utilities.is_daytime();
            case 21:
                originalScript = "pc.skill_level_get(npc,skill_sense_motive) >= 9";
                return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 9;
            case 31:
                originalScript = "pc.skill_level_get(npc,skill_gather_information) >= 10";
                return pc.GetSkillLevel(npc, SkillId.gather_information) >= 10;
            case 41:
                originalScript = "pc.skill_level_get(npc,skill_bluff) >= 11";
                return pc.GetSkillLevel(npc, SkillId.bluff) >= 11;
            case 65:
            case 71:
            case 105:
                originalScript = "pc.skill_level_get(npc,skill_bluff) >= 12";
                return pc.GetSkillLevel(npc, SkillId.bluff) >= 12;
            case 151:
                originalScript = "pc.money_get() >= 1000";
                return pc.GetMoney() >= 1000;
            case 152:
                originalScript = "pc.money_get() <= 900";
                return pc.GetMoney() <= 900;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 3:
            case 13:
            case 22:
            case 32:
            case 43:
            case 51:
            case 123:
            case 143:
            case 153:
                originalScript = "game.global_flags[260] = 1; game.fade_and_teleport(0,0,0,5121,555,308)";
                SetGlobalFlag(260, true);
                FadeAndTeleport(0, 0, 0, 5121, 555, 308);
                ;
                break;
            case 30:
                originalScript = "game.global_vars[700] = 1";
                SetGlobalVar(700, 1);
                break;
            case 80:
            case 90:
                originalScript = "game.global_flags[937] = 1; game.global_vars[700] = 2";
                SetGlobalFlag(937, true);
                SetGlobalVar(700, 2);
                ;
                break;
            case 151:
                originalScript = "pc.money_adj(-1000)";
                pc.AdjustMoney(-1000);
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
            case 21:
                skillChecks = new DialogSkillChecks(SkillId.sense_motive, 9);
                return true;
            case 31:
                skillChecks = new DialogSkillChecks(SkillId.gather_information, 10);
                return true;
            case 41:
                skillChecks = new DialogSkillChecks(SkillId.bluff, 11);
                return true;
            case 65:
            case 71:
            case 105:
                skillChecks = new DialogSkillChecks(SkillId.bluff, 12);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}