
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
            case 2:
                originalScript = "switch_to_iuz(npc,pc,200)";
                switch_to_iuz(npc, pc, 200);
                break;
            case 11:
                originalScript = "switch_to_iuz(npc,pc,210)";
                switch_to_iuz(npc, pc, 210);
                break;
            case 21:
                originalScript = "switch_to_iuz(npc,pc,220)";
                switch_to_iuz(npc, pc, 220);
                break;
            case 30:
                originalScript = "cuthbert_raise_good(npc,pc)";
                cuthbert_raise_good(npc, pc);
                break;
            case 31:
                originalScript = "turn_off_gods(npc,pc)";
                turn_off_gods(npc, pc);
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