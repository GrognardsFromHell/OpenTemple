
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

[DialogScript(70)]
public class FurnokDialog : Furnok, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 3:
                originalScript = "not npc.has_met(pc)";
                return !npc.HasMet(pc);
            case 4:
            case 5:
                originalScript = "not npc.has_met(pc) and game.quests[18].state == qs_accepted";
                return !npc.HasMet(pc) && GetQuestState(18) == QuestState.Accepted;
            case 6:
            case 7:
                originalScript = "npc.has_met(pc)";
                return npc.HasMet(pc);
            case 22:
                originalScript = "game.story_state > 0";
                return StoryState > 0;
            case 23:
                originalScript = "game.story_state >= 3";
                return StoryState >= 3;
            case 61:
            case 65:
            case 71:
            case 73:
            case 81:
            case 83:
            case 241:
            case 242:
            case 361:
            case 371:
            case 373:
                originalScript = "not pc.follower_atmax()";
                return !pc.HasMaxFollowers();
            case 63:
            case 67:
            case 245:
            case 246:
            case 363:
            case 364:
                originalScript = "npc.item_find(8003) != OBJ_HANDLE_NULL and pc.skill_level_get(npc,skill_diplomacy) >= 6";
                return npc.FindItemByName(8003) != null && pc.GetSkillLevel(npc, SkillId.diplomacy) >= 6;
            case 64:
            case 68:
                originalScript = "npc.item_find(8003) == OBJ_HANDLE_NULL";
                return npc.FindItemByName(8003) == null;
            case 69:
            case 75:
            case 76:
            case 85:
            case 86:
            case 247:
            case 248:
            case 365:
            case 366:
            case 375:
            case 376:
                originalScript = "pc.follower_atmax()";
                return pc.HasMaxFollowers();
            case 111:
            case 112:
                originalScript = "game.quests[18].state == qs_accepted";
                return GetQuestState(18) == QuestState.Accepted;
            case 113:
            case 114:
                originalScript = "npc.leader_get() == OBJ_HANDLE_NULL";
                return npc.GetLeader() == null;
            case 141:
            case 142:
            case 171:
            case 172:
                originalScript = "pc.money_get() >= 1000";
                return pc.GetMoney() >= 1000;
            case 143:
            case 144:
            case 173:
            case 174:
                originalScript = "pc.money_get() < 1000";
                return pc.GetMoney() < 1000;
            case 151:
            case 152:
                originalScript = "game.global_vars[7] > 60";
                return GetGlobalVar(7) > 60;
            case 153:
            case 154:
                originalScript = "game.global_vars[7] <= 60";
                return GetGlobalVar(7) <= 60;
            case 163:
            case 164:
            case 193:
            case 194:
                originalScript = "game.global_vars[8] >= (7 - (pc.skill_level_get(npc,skill_spot)/2))";
                throw new NotSupportedException("Conversion failed.");
            case 181:
            case 182:
                originalScript = "game.global_vars[7] > 75";
                return GetGlobalVar(7) > 75;
            case 183:
            case 184:
                originalScript = "game.global_vars[7] <= 75";
                return GetGlobalVar(7) <= 75;
            case 301:
            case 302:
                originalScript = "npc.area != 1 and npc.area != 3";
                return npc.GetArea() != 1 && npc.GetArea() != 3;
            case 303:
            case 304:
                originalScript = "(npc.area == 1 or npc.area == 3) and game.global_flags[51] == 0";
                return (npc.GetArea() == 1 || npc.GetArea() == 3) && !GetGlobalFlag(51);
            case 305:
            case 306:
                originalScript = "(npc.area == 1 or npc.area == 3) and game.global_flags[51] == 1";
                return (npc.GetArea() == 1 || npc.GetArea() == 3) && GetGlobalFlag(51);
            case 413:
            case 414:
                originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= (6+ ((npc.money_get() - 200000) / 50000))";
                return pc.GetSkillLevel(npc, SkillId.diplomacy) >= (6 + ((npc.GetMoney() - 200000) / 50000));
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
                originalScript = "game.global_vars[102] = 1";
                SetGlobalVar(102, 1);
                break;
            case 81:
            case 83:
            case 371:
            case 373:
                originalScript = "npc.item_transfer_to(pc,8003)";
                npc.TransferItemByNameTo(pc, 8003);
                break;
            case 90:
            case 250:
            case 361:
                originalScript = "pc.follower_add(npc); game.global_flags[57] = 1";
                pc.AddFollower(npc);
                SetGlobalFlag(57, true);
                ;
                break;
            case 141:
            case 142:
            case 171:
            case 172:
                originalScript = "pc.money_adj(-1000)";
                pc.AdjustMoney(-1000);
                break;
            case 150:
            case 180:
                originalScript = "game.global_vars[7] = game.random_range( 1, 100 )";
                SetGlobalVar(7, RandomRange(1, 100));
                break;
            case 151:
            case 152:
            case 181:
            case 182:
                originalScript = "pc.money_adj(2000)";
                pc.AdjustMoney(2000);
                break;
            case 153:
            case 154:
            case 183:
            case 184:
                originalScript = "create_item_in_inventory( 7001, NPC ); create_item_in_inventory( 7001, NPC ); game.global_vars[8] = game.global_vars[8] + 1";
                Utilities.create_item_in_inventory(7001, npc);
                Utilities.create_item_in_inventory(7001, npc);
                SetGlobalVar(8, GetGlobalVar(8) + 1);
                break;
            case 200:
                originalScript = "game.global_flags[51] = 1";
                SetGlobalFlag(51, true);
                break;
            case 320:
            case 333:
            case 334:
                originalScript = "pc.follower_remove(npc)";
                pc.RemoveFollower(npc);
                break;
            case 411:
            case 412:
                originalScript = "pc.follower_remove(npc); game.global_flags[61] = 1";
                pc.RemoveFollower(npc);
                SetGlobalFlag(61, true);
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
            case 63:
            case 67:
            case 245:
            case 246:
            case 363:
            case 364:
                skillChecks = new DialogSkillChecks(SkillId.diplomacy, 6);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}