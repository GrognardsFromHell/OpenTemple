
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

namespace Scripts.Dialog
{
    [DialogScript(200)]
    public class ZaxisDialog : Zaxis, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 31:
                case 32:
                case 33:
                case 34:
                case 611:
                    originalScript = "npc.leader_get() == OBJ_HANDLE_NULL";
                    return npc.GetLeader() == null;
                case 35:
                case 36:
                case 237:
                case 238:
                case 287:
                case 288:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 11008) and game.global_flags[880] == 0";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(11008)) && !GetGlobalFlag(880);
                case 37:
                case 87:
                case 88:
                case 612:
                    originalScript = "npc.leader_get() != OBJ_HANDLE_NULL";
                    return npc.GetLeader() != null;
                case 51:
                case 52:
                case 233:
                case 234:
                case 285:
                case 286:
                case 301:
                case 302:
                case 616:
                case 617:
                    originalScript = "not pc.follower_atmax()";
                    return !pc.HasMaxFollowers();
                case 53:
                case 54:
                case 235:
                case 236:
                case 614:
                case 615:
                    originalScript = "pc.follower_atmax()";
                    return pc.HasMaxFollowers();
                case 71:
                    originalScript = "game.global_flags[879] == 0";
                    return !GetGlobalFlag(879);
                case 72:
                    originalScript = "game.global_flags[879] == 1";
                    return GetGlobalFlag(879);
                case 81:
                case 82:
                case 83:
                case 84:
                    originalScript = "game.global_flags[880] == 0";
                    return !GetGlobalFlag(880);
                case 112:
                    originalScript = "game.story_state == 6";
                    return StoryState == 6;
                case 113:
                    originalScript = "game.story_state >= 3";
                    return StoryState >= 3;
                case 114:
                    originalScript = "game.quests[18].state > qs_unknown";
                    return GetQuestState(18) > QuestState.Unknown;
                case 223:
                case 224:
                    originalScript = "game.global_flags[878] == 0";
                    return !GetGlobalFlag(878);
                case 243:
                case 244:
                    originalScript = "npc.has_met(pc)";
                    return npc.HasMet(pc);
                case 283:
                    originalScript = "pc.map == 5057";
                    return pc.GetMap() == 5057;
                case 284:
                    originalScript = "npc.map == 5057";
                    return npc.GetMap() == 5057;
                case 501:
                case 502:
                    originalScript = "npc.hit_dice_num > 5 and npc.item_find(6091)";
                    return GameSystems.Critter.GetHitDiceNum(npc) > 5 && npc.FindItemByName(6091) != null;
                case 503:
                case 504:
                    originalScript = "npc.hit_dice_num == 5";
                    return GameSystems.Critter.GetHitDiceNum(npc) == 5;
                case 551:
                case 552:
                    originalScript = "pc.money_get() >= 170000";
                    return pc.GetMoney() >= 170000;
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
                    originalScript = "game.global_vars[117] = 1";
                    SetGlobalVar(117, 1);
                    break;
                case 60:
                case 640:
                    originalScript = "game.global_flags[879] = 1; pc.follower_add(npc)";
                    SetGlobalFlag(879, true);
                    pc.AddFollower(npc);
                    ;
                    break;
                case 71:
                    originalScript = "zaxis_runs_off(npc,pc)";
                    zaxis_runs_off(npc, pc);
                    break;
                case 91:
                case 92:
                case 650:
                    originalScript = "pc.follower_remove(npc)";
                    pc.RemoveFollower(npc);
                    break;
                case 551:
                case 552:
                    originalScript = "pc.money_adj(-170000); equip_transfer( npc, pc )";
                    pc.AdjustMoney(-170000);
                    equip_transfer(npc, pc);
                    ;
                    break;
                case 601:
                    originalScript = "party_transfer_to( npc, 11008 ); game.global_flags[880] = 1";
                    Utilities.party_transfer_to(npc, 11008);
                    SetGlobalFlag(880, true);
                    ;
                    break;
                case 621:
                case 651:
                    originalScript = "zaxis_runs_off2(npc,pc)";
                    zaxis_runs_off2(npc, pc);
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
