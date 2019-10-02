
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
    [DialogScript(68)]
    public class KobortDialog : Kobort, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    Trace.Assert(originalScript == "not npc.has_met( pc )");
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                    Trace.Assert(originalScript == "npc.has_met( pc )");
                    return npc.HasMet(pc);
                case 41:
                case 42:
                    Trace.Assert(originalScript == "npc.leader_get() == OBJ_HANDLE_NULL");
                    return npc.GetLeader() == null;
                case 203:
                case 204:
                case 253:
                case 254:
                    Trace.Assert(originalScript == "npc.leader_get() != OBJ_HANDLE_NULL");
                    return npc.GetLeader() != null;
                case 501:
                case 502:
                    Trace.Assert(originalScript == "npc.hit_dice_num == 2");
                    return GameSystems.Critter.GetHitDiceNum(npc) == 2;
                case 503:
                case 504:
                    Trace.Assert(originalScript == "npc.hit_dice_num > 2 and (npc.item_find(6120) or npc.item_find(6059) or npc.item_find(4205))");
                    return GameSystems.Critter.GetHitDiceNum(npc) > 2 && (npc.FindItemByName(6120) != null || npc.FindItemByName(6059) != null || npc.FindItemByName(4205) != null);
                case 551:
                case 552:
                    Trace.Assert(originalScript == "pc.money_get() > 30000");
                    return pc.GetMoney() > 30000;
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
                    Trace.Assert(originalScript == "game.global_vars[105] = 1");
                    SetGlobalVar(105, 1);
                    break;
                case 260:
                    Trace.Assert(originalScript == "pc.follower_remove(npc)");
                    pc.RemoveFollower(npc);
                    break;
                case 570:
                    Trace.Assert(originalScript == "equip_transfer( npc, pc )");
                    equip_transfer(npc, pc);
                    break;
                case 571:
                case 572:
                    Trace.Assert(originalScript == "pc.money_adj(-30000)");
                    pc.AdjustMoney(-30000);
                    break;
                case 601:
                    Trace.Assert(originalScript == "switch_to_turuko( npc, pc, 600)");
                    switch_to_turuko(npc, pc, 600);
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
