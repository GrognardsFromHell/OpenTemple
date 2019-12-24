
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
    [DialogScript(370)]
    public class Innkeeper2Dialog : Innkeeper2, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    originalScript = "not npc.has_met(pc)";
                    return !npc.HasMet(pc);
                case 3:
                case 4:
                    originalScript = "npc.has_met(pc)";
                    return npc.HasMet(pc);
                case 13:
                case 23:
                case 71:
                    originalScript = "game.global_flags[993] == 0";
                    return !GetGlobalFlag(993);
                case 14:
                case 24:
                case 72:
                    originalScript = "game.global_flags[993] == 1";
                    return GetGlobalFlag(993);
                case 21:
                    originalScript = "pc.money_get() >= 1000";
                    return pc.GetMoney() >= 1000;
                case 22:
                    originalScript = "pc.money_get() < 900";
                    return pc.GetMoney() < 900;
                case 61:
                    originalScript = "game.global_flags[997] == 0";
                    return !GetGlobalFlag(997);
                case 91:
                    originalScript = "pc.money_get() >= 10";
                    return pc.GetMoney() >= 10;
                case 92:
                    originalScript = "pc.money_get() <= 9";
                    return pc.GetMoney() <= 9;
                case 101:
                case 121:
                    originalScript = "pc.money_get() >= 30";
                    return pc.GetMoney() >= 30;
                case 102:
                case 122:
                    originalScript = "pc.money_get() <= 29";
                    return pc.GetMoney() <= 29;
                case 141:
                    originalScript = "game.global_flags[993] == 0 and not get_1(npc)";
                    return !GetGlobalFlag(993) && !Scripts.get_1(npc);
                case 142:
                    originalScript = "game.global_flags[993] == 1 and not get_1(npc)";
                    return GetGlobalFlag(993) && !Scripts.get_1(npc);
                case 143:
                    originalScript = "not get_2(npc)";
                    return !Scripts.get_2(npc);
                case 181:
                    originalScript = "pc.money_get() >= 15000";
                    return pc.GetMoney() >= 15000;
                case 182:
                    originalScript = "pc.money_get() <= 14900";
                    return pc.GetMoney() <= 14900;
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
                    originalScript = "pc.money_adj(-1000)";
                    pc.AdjustMoney(-1000);
                    break;
                case 30:
                    originalScript = "set_room_flag(npc,pc)";
                    set_room_flag(npc, pc);
                    break;
                case 91:
                    originalScript = "pc.money_adj(-10)";
                    pc.AdjustMoney(-10);
                    break;
                case 101:
                case 121:
                    originalScript = "pc.money_adj(-30)";
                    pc.AdjustMoney(-30);
                    break;
                case 110:
                    originalScript = "create_item_in_inventory( 8004, pc )";
                    Utilities.create_item_in_inventory(8004, pc);
                    break;
                case 141:
                case 142:
                    originalScript = "npc_1(npc)";
                    Scripts.npc_1(npc);
                    break;
                case 143:
                    originalScript = "npc_2(npc)";
                    Scripts.npc_2(npc);
                    break;
                case 151:
                    originalScript = "game.fade(3600,4046,0,4)";
                    Fade(3600, 4046, 0, 4);
                    break;
                case 181:
                    originalScript = "pc.money_adj(-15000)";
                    pc.AdjustMoney(-15000);
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
