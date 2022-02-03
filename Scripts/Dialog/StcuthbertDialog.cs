
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

[DialogScript(173)]
public class StcuthbertDialog : Stcuthbert, IDialogScript
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
                originalScript = "unshit(npc,pc); game.global_flags[544] = 1; game.global_flags[328] = 1";
                unshit(npc, pc);
                SetGlobalFlag(544, true);
                SetGlobalFlag(328, true);
                ;
                break;
            case 2:
                originalScript = "switch_to_iuz(npc,pc,200)";
                switch_to_iuz(npc, pc, 200);
                break;
            case 10:
            case 20:
            case 30:
                originalScript = "unshit(npc,pc)";
                unshit(npc, pc);
                break;
            case 11:
                originalScript = "switch_to_iuz(npc,pc,210)";
                switch_to_iuz(npc, pc, 210);
                break;
            case 21:
                originalScript = "switch_to_iuz(npc,pc,220)";
                switch_to_iuz(npc, pc, 220);
                break;
            case 31:
                originalScript = "cuthbert_raise_good(npc,pc); turn_off_gods(npc,pc); game.global_flags[544] = 0";
                cuthbert_raise_good(npc, pc);
                turn_off_gods(npc, pc);
                SetGlobalFlag(544, false);
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