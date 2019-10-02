
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

namespace Scripts.Dialog
{
    [DialogScript(416)]
    public class StandardEquipmentChestDialog : StandardEquipmentChest, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 6000:
                    Trace.Assert(originalScript == "npc_get(npc, 1) == 0");
                    return !ScriptDaemon.npc_get(npc, 1);
                case 21:
                case 61:
                case 71:
                case 101:
                    Trace.Assert(originalScript == "npc_get(npc, 1) == 1");
                    return ScriptDaemon.npc_get(npc, 1);
                case 22:
                case 41:
                case 102:
                    Trace.Assert(originalScript == "npc_get(npc, 2) == 0 and ( pc.money_get() >= 50000 or (game.global_vars[455] & 2**5 == 0) )");
                    return !ScriptDaemon.npc_get(npc, 2) && (pc.GetMoney() >= 50000 || ((GetGlobalVar(455) & 0x20) == 0));
                case 23:
                case 42:
                case 103:
                    Trace.Assert(originalScript == "npc_get(npc, 2) == 0 and pc.money_get() < 50000 and (game.global_vars[455] & 2**5 != 0)");
                    return !ScriptDaemon.npc_get(npc, 2) && pc.GetMoney() < 50000 && ((GetGlobalVar(455) & 0x20) != 0);
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 1:
                case 100:
                    Trace.Assert(originalScript == "game.sound( 4011 )");
                    Sound(4011);
                    break;
                case 2:
                    Trace.Assert(originalScript == "npc_set(npc, 1); give_default_starting_equipment(1)");
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
                    Trace.Assert(originalScript == "game.sound( 4010 )");
                    Sound(4010);
                    break;
                case 21:
                case 61:
                case 71:
                case 101:
                    Trace.Assert(originalScript == "defalt_equipment_autoequip()");
                    defalt_equipment_autoequip();
                    break;
                case 22:
                case 41:
                case 102:
                    Trace.Assert(originalScript == "pc.money_adj(50000 - pc.money_get() ); npc_set(npc, 2)");
                    pc.AdjustMoney(50000 - pc.GetMoney());
                    ScriptDaemon.npc_set(npc, 2);
                    ;
                    break;
                case 6000:
                    Trace.Assert(originalScript == "npc_set(npc, 1); give_default_starting_equipment(2)");
                    ScriptDaemon.npc_set(npc, 1);
                    give_default_starting_equipment(2);
                    ;
                    break;
                default:
                    Trace.Assert(originalScript == null);
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
