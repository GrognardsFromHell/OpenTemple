
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

[DialogScript(522)]
public class BookOfHeroesCritterDialog : BookOfHeroesCritter, IDialogScript
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
            case 3:
            case 12:
            case 22:
            case 42:
            case 51:
            case 61:
                originalScript = "npc.destroy()";
                npc.Destroy();
                break;
            case 31:
                originalScript = "npc.destroy(); game.global_vars[994] = 0; game.global_vars[988] = npc.area; game.fade_and_teleport(0,0,0,5119,504,468)";
                npc.Destroy();
                SetGlobalVar(994, 0);
                SetGlobalVar(988, npc.GetArea());
                FadeAndTeleport(0, 0, 0, 5119, 504, 468);
                ;
                break;
            case 41:
                originalScript = "npc.destroy(); game.global_vars[988] = npc.area; game.fade_and_teleport(0,0,0,5119,504,468)";
                npc.Destroy();
                SetGlobalVar(988, npc.GetArea());
                FadeAndTeleport(0, 0, 0, 5119, 504, 468);
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