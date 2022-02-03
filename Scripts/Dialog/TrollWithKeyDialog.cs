
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

[DialogScript(211)]
public class TrollWithKeyDialog : TrollWithKey, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 3:
                originalScript = "pc.skill_level_get(npc,skill_bluff) >= 3 and npc.item_find(4401) != OBJ_HANDLE_NULL and not npc.has_met(pc)";
                return pc.GetSkillLevel(npc, SkillId.bluff) >= 3 && npc.FindItemByName(4401) != null && !npc.HasMet(pc);
            case 4:
            case 5:
                originalScript = "pc.skill_level_get(npc,skill_bluff) >= 3 and npc.item_find(4401) == OBJ_HANDLE_NULL and not npc.has_met(pc)";
                return pc.GetSkillLevel(npc, SkillId.bluff) >= 3 && npc.FindItemByName(4401) == null && !npc.HasMet(pc);
            case 6:
            case 7:
            case 45:
            case 46:
                originalScript = "not npc.has_met(pc)";
                return !npc.HasMet(pc);
            case 8:
            case 9:
            case 47:
            case 48:
                originalScript = "npc.has_met(pc)";
                return npc.HasMet(pc);
            case 41:
            case 42:
                originalScript = "pc.skill_level_get(npc,skill_bluff) >= 3 and not npc.has_met(pc)";
                return pc.GetSkillLevel(npc, SkillId.bluff) >= 3 && !npc.HasMet(pc);
            case 43:
            case 44:
                originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 10 and npc.item_find(4401) != OBJ_HANDLE_NULL";
                return pc.GetSkillLevel(npc, SkillId.intimidate) >= 10 && npc.FindItemByName(4401) != null;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 10:
                originalScript = "npc.item_transfer_to(pc,4401)";
                npc.TransferItemByNameTo(pc, 4401);
                break;
            case 31:
            case 51:
                originalScript = "npc.attack(pc)";
                npc.Attack(pc);
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
            case 3:
            case 4:
            case 5:
            case 41:
            case 42:
                skillChecks = new DialogSkillChecks(SkillId.bluff, 3);
                return true;
            case 43:
            case 44:
                skillChecks = new DialogSkillChecks(SkillId.intimidate, 10);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}