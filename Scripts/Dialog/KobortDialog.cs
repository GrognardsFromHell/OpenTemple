
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

[DialogScript(68)]
public class KobortDialog : Kobort, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 3:
                originalScript = "not npc.has_met( pc )";
                return !npc.HasMet(pc);
            case 4:
            case 5:
                originalScript = "npc.has_met( pc )";
                return npc.HasMet(pc);
            case 41:
            case 42:
                originalScript = "npc.leader_get() == OBJ_HANDLE_NULL";
                return npc.GetLeader() == null;
            case 203:
            case 204:
            case 253:
            case 254:
                originalScript = "npc.leader_get() != OBJ_HANDLE_NULL";
                return npc.GetLeader() != null;
            case 501:
            case 502:
                originalScript = "npc.hit_dice_num == 2";
                return GameSystems.Critter.GetHitDiceNum(npc) == 2;
            case 503:
            case 504:
                originalScript = "npc.hit_dice_num > 2 and (npc.item_find(6120) or npc.item_find(6059) or npc.item_find(4205))";
                return GameSystems.Critter.GetHitDiceNum(npc) > 2 && (npc.FindItemByName(6120) != null || npc.FindItemByName(6059) != null || npc.FindItemByName(4205) != null);
            case 551:
            case 552:
                originalScript = "pc.money_get() > 30000";
                return pc.GetMoney() > 30000;
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
                originalScript = "game.global_vars[105] = 1";
                SetGlobalVar(105, 1);
                break;
            case 260:
                originalScript = "pc.follower_remove(npc)";
                pc.RemoveFollower(npc);
                break;
            case 570:
                originalScript = "equip_transfer( npc, pc )";
                equip_transfer(npc, pc);
                break;
            case 571:
            case 572:
                originalScript = "pc.money_adj(-30000)";
                pc.AdjustMoney(-30000);
                break;
            case 601:
                originalScript = "switch_to_turuko( npc, pc, 600)";
                switch_to_turuko(npc, pc, 600);
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