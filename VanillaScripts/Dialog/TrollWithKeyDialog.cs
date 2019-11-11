
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Dialog
{
    [DialogScript(211)]
    public class TrollWithKeyDialog : TrollWithKey, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 1 and npc.item_find(4401) != OBJ_HANDLE_NULL and not npc.has_met(pc)";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 1 && npc.FindItemByName(4401) != null && !npc.HasMet(pc);
                case 4:
                case 5:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 1 and npc.item_find(4401) == OBJ_HANDLE_NULL and not npc.has_met(pc)";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 1 && npc.FindItemByName(4401) == null && !npc.HasMet(pc);
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
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 1 and not npc.has_met(pc)";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 1 && !npc.HasMet(pc);
                case 43:
                case 44:
                    originalScript = "pc.skill_level_get(npc,skill_intimidate) >= 8 and npc.item_find(4401) != OBJ_HANDLE_NULL";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 8 && npc.FindItemByName(4401) != null;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
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
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 1);
                    return true;
                case 43:
                case 44:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 8);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
