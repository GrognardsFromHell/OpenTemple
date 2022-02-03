
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

[DialogScript(178)]
public class ErliterBeggarDialog : ErliterBeggar, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 41:
            case 43:
            case 61:
            case 64:
                originalScript = "pc.money_get() >= 100";
                return pc.GetMoney() >= 100;
            case 62:
            case 65:
                originalScript = "pc.money_get() >= 1000";
                return pc.GetMoney() >= 1000;
            case 63:
            case 66:
                originalScript = "pc.money_get() >= 10000";
                return pc.GetMoney() >= 10000;
            case 71:
                originalScript = "game.global_vars[19] > 100";
                return GetGlobalVar(19) > 100;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 41:
            case 43:
            case 61:
            case 64:
                originalScript = "pc.money_adj(-100); game.global_vars[19] = game.global_vars[19] + 1";
                pc.AdjustMoney(-100);
                SetGlobalVar(19, GetGlobalVar(19) + 1);
                ;
                break;
            case 62:
            case 65:
                originalScript = "pc.money_adj(-1000); game.global_vars[19] = game.global_vars[19] + 10";
                pc.AdjustMoney(-1000);
                SetGlobalVar(19, GetGlobalVar(19) + 10);
                ;
                break;
            case 63:
            case 66:
                originalScript = "pc.money_adj(-10000); game.global_vars[19] = game.global_vars[19] + 100";
                pc.AdjustMoney(-10000);
                SetGlobalVar(19, GetGlobalVar(19) + 100);
                ;
                break;
            case 91:
                originalScript = "leave_for_city(npc,pc)";
                leave_for_city(npc, pc);
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