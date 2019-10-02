
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
    [DialogScript(200)]
    public class ZaxisDialog : Zaxis, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 31:
                case 32:
                case 33:
                case 34:
                case 611:
                    Trace.Assert(originalScript == "npc.leader_get() == OBJ_HANDLE_NULL");
                    return npc.GetLeader() == null;
                case 35:
                case 36:
                case 237:
                case 238:
                case 287:
                case 288:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_item\", 11008) and game.global_flags[880] == 0");
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(11008)) && !GetGlobalFlag(880);
                case 37:
                case 87:
                case 88:
                case 612:
                    Trace.Assert(originalScript == "npc.leader_get() != OBJ_HANDLE_NULL");
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
                    Trace.Assert(originalScript == "not pc.follower_atmax()");
                    return !pc.HasMaxFollowers();
                case 53:
                case 54:
                case 235:
                case 236:
                case 614:
                case 615:
                    Trace.Assert(originalScript == "pc.follower_atmax()");
                    return pc.HasMaxFollowers();
                case 71:
                    Trace.Assert(originalScript == "game.global_flags[879] == 0");
                    return !GetGlobalFlag(879);
                case 72:
                    Trace.Assert(originalScript == "game.global_flags[879] == 1");
                    return GetGlobalFlag(879);
                case 81:
                case 82:
                case 83:
                case 84:
                    Trace.Assert(originalScript == "game.global_flags[880] == 0");
                    return !GetGlobalFlag(880);
                case 112:
                    Trace.Assert(originalScript == "game.story_state == 6");
                    return StoryState == 6;
                case 113:
                    Trace.Assert(originalScript == "game.story_state >= 3");
                    return StoryState >= 3;
                case 114:
                    Trace.Assert(originalScript == "game.quests[18].state > qs_unknown");
                    return GetQuestState(18) > QuestState.Unknown;
                case 223:
                case 224:
                    Trace.Assert(originalScript == "game.global_flags[878] == 0");
                    return !GetGlobalFlag(878);
                case 243:
                case 244:
                    Trace.Assert(originalScript == "npc.has_met(pc)");
                    return npc.HasMet(pc);
                case 283:
                    Trace.Assert(originalScript == "pc.map == 5057");
                    return pc.GetMap() == 5057;
                case 284:
                    Trace.Assert(originalScript == "npc.map == 5057");
                    return npc.GetMap() == 5057;
                case 501:
                case 502:
                    Trace.Assert(originalScript == "npc.hit_dice_num > 5 and npc.item_find(6091)");
                    return GameSystems.Critter.GetHitDiceNum(npc) > 5 && npc.FindItemByName(6091) != null;
                case 503:
                case 504:
                    Trace.Assert(originalScript == "npc.hit_dice_num == 5");
                    return GameSystems.Critter.GetHitDiceNum(npc) == 5;
                case 551:
                case 552:
                    Trace.Assert(originalScript == "pc.money_get() >= 170000");
                    return pc.GetMoney() >= 170000;
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
                    Trace.Assert(originalScript == "game.global_vars[117] = 1");
                    SetGlobalVar(117, 1);
                    break;
                case 60:
                case 640:
                    Trace.Assert(originalScript == "game.global_flags[879] = 1; pc.follower_add(npc)");
                    SetGlobalFlag(879, true);
                    pc.AddFollower(npc);
                    ;
                    break;
                case 71:
                    Trace.Assert(originalScript == "zaxis_runs_off(npc,pc)");
                    zaxis_runs_off(npc, pc);
                    break;
                case 91:
                case 92:
                case 650:
                    Trace.Assert(originalScript == "pc.follower_remove(npc)");
                    pc.RemoveFollower(npc);
                    break;
                case 551:
                case 552:
                    Trace.Assert(originalScript == "pc.money_adj(-170000); equip_transfer( npc, pc )");
                    pc.AdjustMoney(-170000);
                    equip_transfer(npc, pc);
                    ;
                    break;
                case 601:
                    Trace.Assert(originalScript == "party_transfer_to( npc, 11008 ); game.global_flags[880] = 1");
                    Utilities.party_transfer_to(npc, 11008);
                    SetGlobalFlag(880, true);
                    ;
                    break;
                case 621:
                case 651:
                    Trace.Assert(originalScript == "zaxis_runs_off2(npc,pc)");
                    zaxis_runs_off2(npc, pc);
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillCheck(int lineNumber, out DialogSkillChecks skillChecks)
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
