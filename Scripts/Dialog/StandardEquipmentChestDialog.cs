
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
    [DialogScript(416)]
    public class StandardEquipmentChestDialog : StandardEquipmentChest, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 6000:
                    originalScript = "npc_get(npc, 1) == 0";
                    return !ScriptDaemon.npc_get(npc, 1);
                case 21:
                case 61:
                case 71:
                case 101:
                    originalScript = "npc_get(npc, 1) == 1";
                    return ScriptDaemon.npc_get(npc, 1);
                case 22:
                case 41:
                case 102:
                    originalScript = "npc_get(npc, 2) == 0 and ( pc.money_get() >= 50000 or (game.global_vars[455] & 2**5 == 0) )";
                    return !ScriptDaemon.npc_get(npc, 2) && (pc.GetMoney() >= 50000 || ((GetGlobalVar(455) & 0x20) == 0));
                case 23:
                case 42:
                case 103:
                    originalScript = "npc_get(npc, 2) == 0 and pc.money_get() < 50000 and (game.global_vars[455] & 2**5 != 0)";
                    return !ScriptDaemon.npc_get(npc, 2) && pc.GetMoney() < 50000 && ((GetGlobalVar(455) & 0x20) != 0);
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                case 100:
                    originalScript = "game.sound( 4011 )";
                    Sound(4011);
                    break;
                case 2:
                    originalScript = "npc_set(npc, 1); give_default_starting_equipment(1)";
                    ScriptDaemon.npc_set(npc, 1);
                    give_default_starting_equipment(1);
                    ;
                    break;
                case 4:
                case 24:
                case 43:
                case 62:
                case 72:
                case 104:
                    originalScript = "game.sound( 4010 )";
                    Sound(4010);
                    break;
                case 21:
                case 61:
                case 71:
                case 101:
                    originalScript = "defalt_equipment_autoequip()";
                    defalt_equipment_autoequip();
                    break;
                case 22:
                case 41:
                case 102:
                    originalScript = "pc.money_adj(50000 - pc.money_get() ); npc_set(npc, 2)";
                    pc.AdjustMoney(50000 - pc.GetMoney());
                    ScriptDaemon.npc_set(npc, 2);
                    ;
                    break;
                case 6000:
                    originalScript = "npc_set(npc, 1); give_default_starting_equipment(2)";
                    ScriptDaemon.npc_set(npc, 1);
                    give_default_starting_equipment(2);
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
}
