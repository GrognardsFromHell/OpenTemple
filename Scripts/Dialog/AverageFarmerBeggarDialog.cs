
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
    [DialogScript(179)]
    public class AverageFarmerBeggarDialog : AverageFarmerBeggar, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 4:
                    originalScript = "game.global_flags[1] == 1";
                    return GetGlobalFlag(1);
                case 21:
                case 24:
                    originalScript = "pc.money_get() >= 100";
                    return pc.GetMoney() >= 100;
                case 22:
                case 23:
                case 25:
                case 26:
                    originalScript = "pc.money_get() >= 1000";
                    return pc.GetMoney() >= 1000;
                case 32:
                case 33:
                    originalScript = "game.global_vars[20] == 0";
                    return GetGlobalVar(20) == 0;
                case 34:
                case 35:
                    originalScript = "game.global_vars[20] >= 1";
                    return GetGlobalVar(20) >= 1;
                case 61:
                case 71:
                case 81:
                    originalScript = "game.global_vars[20] >= 50 and game.global_vars[20] <= 100 and game.global_flags[206] == 0";
                    return GetGlobalVar(20) >= 50 && GetGlobalVar(20) <= 100 && !GetGlobalFlag(206);
                case 62:
                case 72:
                case 82:
                    originalScript = "game.global_vars[20] >= 101 and game.global_vars[20] <= 200 and game.global_flags[206] == 1";
                    return GetGlobalVar(20) >= 101 && GetGlobalVar(20) <= 200 && GetGlobalFlag(206);
                case 63:
                case 73:
                case 83:
                    originalScript = "game.global_vars[20] >= 201 and game.global_vars[20] <= 300 and game.global_flags[206] == 0";
                    return GetGlobalVar(20) >= 201 && GetGlobalVar(20) <= 300 && !GetGlobalFlag(206);
                case 64:
                case 74:
                case 84:
                    originalScript = "game.global_vars[20] >= 301 and game.global_vars[20] <= 400 and game.global_flags[206] == 1";
                    return GetGlobalVar(20) >= 301 && GetGlobalVar(20) <= 400 && GetGlobalFlag(206);
                case 65:
                case 75:
                case 85:
                    originalScript = "game.global_vars[20] >= 401";
                    return GetGlobalVar(20) >= 401;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 21:
                case 24:
                    originalScript = "pc.money_adj(-100); game.global_vars[20] = game.global_vars[20] + 1";
                    pc.AdjustMoney(-100);
                    SetGlobalVar(20, GetGlobalVar(20) + 1);
                    ;
                    break;
                case 22:
                case 25:
                    originalScript = "pc.money_adj(-1000); game.global_vars[20] = game.global_vars[20] + 10";
                    pc.AdjustMoney(-1000);
                    SetGlobalVar(20, GetGlobalVar(20) + 10);
                    ;
                    break;
                case 23:
                case 26:
                    originalScript = "pc.money_adj(-10000); game.global_vars[20] = game.global_vars[20] + 100";
                    pc.AdjustMoney(-10000);
                    SetGlobalVar(20, GetGlobalVar(20) + 100);
                    ;
                    break;
                case 90:
                case 110:
                    originalScript = "game.global_flags[206] = 1";
                    SetGlobalFlag(206, true);
                    break;
                case 100:
                case 120:
                    originalScript = "game.global_flags[206] = 0";
                    SetGlobalFlag(206, false);
                    break;
                case 131:
                    originalScript = "run_off(npc,pc)";
                    run_off(npc, pc);
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
