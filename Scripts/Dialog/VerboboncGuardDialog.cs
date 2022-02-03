
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

[DialogScript(385)]
public class VerboboncGuardDialog : VerboboncGuard, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
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
                originalScript = "guard_backup(npc,pc)";
                guard_backup(npc, pc);
                break;
            case 2:
            case 32:
                originalScript = "npc.attack(pc)";
                npc.Attack(pc);
                break;
            case 30:
                originalScript = "game.global_vars[969] = 1";
                SetGlobalVar(969, 1);
                break;
            case 40:
                originalScript = "game.global_vars[969] = 2";
                SetGlobalVar(969, 2);
                break;
            case 41:
                originalScript = "game.fade_and_teleport(0,0,0,5172,471,489)";
                FadeAndTeleport(0, 0, 0, 5172, 471, 489);
                break;
            case 61:
                originalScript = "execution(npc,pc)";
                execution(npc, pc);
                break;
            case 81:
                originalScript = "game.global_flags[260] = 1; game.fade_and_teleport(0,0,0,5121,228,507)";
                SetGlobalFlag(260, true);
                FadeAndTeleport(0, 0, 0, 5121, 228, 507);
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