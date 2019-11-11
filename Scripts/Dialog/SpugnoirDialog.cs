
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
    [DialogScript(81)]
    public class SpugnoirDialog : Spugnoir, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                case 3:
                    originalScript = "not npc.has_met(pc)";
                    return !npc.HasMet(pc);
                case 4:
                case 5:
                    originalScript = "npc.has_met(pc)";
                    return npc.HasMet(pc);
                case 21:
                case 22:
                    originalScript = "game.story_state < 2";
                    return StoryState < 2;
                case 23:
                case 24:
                    originalScript = "game.story_state >= 2";
                    return StoryState >= 2;
                case 25:
                case 26:
                    originalScript = "pc.skill_level_get(npc,skill_gather_information) >= 2";
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 2;
                case 31:
                case 32:
                    originalScript = "game.areas[2] == 1 and npc.leader_get() == OBJ_HANDLE_NULL";
                    return IsAreaKnown(2) && npc.GetLeader() == null;
                case 33:
                case 34:
                case 104:
                case 109:
                case 143:
                case 144:
                    originalScript = "game.areas[2] == 0";
                    return !IsAreaKnown(2);
                case 35:
                case 36:
                    originalScript = "game.areas[2] == 1 and npc.leader_get() != OBJ_HANDLE_NULL";
                    return IsAreaKnown(2) && npc.GetLeader() != null;
                case 37:
                case 38:
                    originalScript = "game.areas[5] == 0";
                    return !IsAreaKnown(5);
                case 41:
                case 42:
                    originalScript = "not pc.follower_atmax()";
                    return !pc.HasMaxFollowers();
                case 45:
                case 46:
                    originalScript = "pc.follower_atmax()";
                    return pc.HasMaxFollowers();
                case 71:
                case 72:
                case 81:
                case 82:
                case 101:
                case 106:
                case 121:
                case 122:
                case 131:
                case 132:
                    originalScript = "npc.leader_get() == OBJ_HANDLE_NULL";
                    return npc.GetLeader() == null;
                case 83:
                case 84:
                case 105:
                case 110:
                case 145:
                case 146:
                    originalScript = "game.story_state == 2";
                    return StoryState == 2;
                case 102:
                case 107:
                case 113:
                case 114:
                    originalScript = "npc.leader_get() != OBJ_HANDLE_NULL";
                    return npc.GetLeader() != null;
                case 111:
                case 112:
                case 147:
                case 148:
                    originalScript = "game.story_state <= 1 and game.areas[5] == 0";
                    return StoryState <= 1 && !IsAreaKnown(5);
                case 501:
                case 502:
                    originalScript = "game.global_flags[805] == 0";
                    return !GetGlobalFlag(805);
                case 561:
                    originalScript = "pc.money_get() >= 500";
                    return pc.GetMoney() >= 500;
                case 562:
                case 563:
                    originalScript = "pc.money_get() <= 499";
                    return pc.GetMoney() <= 499;
                case 831:
                case 832:
                    originalScript = "pc.skill_level_get(npc,skill_gather_information) >=4";
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 4;
                case 881:
                    originalScript = "pc.skill_level_get(npc,skill_gather_information) >= 8";
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 8;
                case 892:
                    originalScript = "game.story_state >= 5";
                    return StoryState >= 5;
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
                    originalScript = "game.global_vars[103] = 1";
                    SetGlobalVar(103, 1);
                    break;
                case 33:
                case 34:
                case 104:
                case 109:
                    originalScript = "game.story_state = 1; game.areas[2] = 1";
                    StoryState = 1;
                    MakeAreaKnown(2);
                    ;
                    break;
                case 41:
                case 42:
                    originalScript = "pc.follower_add(npc)";
                    pc.AddFollower(npc);
                    break;
                case 75:
                case 76:
                    originalScript = "game.worldmap_travel_by_dialog(2)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 83:
                case 84:
                case 105:
                case 110:
                    originalScript = "game.story_state = 3; game.areas[3] = 1";
                    StoryState = 3;
                    MakeAreaKnown(3);
                    ;
                    break;
                case 91:
                case 92:
                    originalScript = "pc.follower_remove(npc)";
                    pc.RemoveFollower(npc);
                    break;
                case 125:
                case 126:
                    originalScript = "game.worldmap_travel_by_dialog(3)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 130:
                    originalScript = "game.areas[5] = 1";
                    MakeAreaKnown(5);
                    break;
                case 135:
                case 136:
                    originalScript = "game.worldmap_travel_by_dialog(5)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 503:
                case 504:
                    originalScript = "game.global_flags[806] = 1";
                    SetGlobalFlag(806, true);
                    break;
                case 550:
                    originalScript = "pc.follower_remove( npc ); pc.follower_add( npc ); game.global_flags[806] = 0";
                    pc.RemoveFollower(npc);
                    pc.AddFollower(npc);
                    SetGlobalFlag(806, false);
                    ;
                    break;
                case 561:
                    originalScript = "game.global_flags[805] = 1; pc.money_adj(-500); equip_transfer( npc, pc )";
                    SetGlobalFlag(805, true);
                    pc.AdjustMoney(-500);
                    equip_transfer(npc, pc);
                    ;
                    break;
                case 1000:
                    originalScript = "game.global_flags[819] = 1; NPC.npc_flag_set(ONF_KOS)";
                    SetGlobalFlag(819, true);
                    npc.SetNpcFlag(NpcFlag.KOS);
                    break;
                case 1001:
                case 1002:
                case 1006:
                case 1007:
                    originalScript = "pc.follower_remove(npc); npc.attack( pc )";
                    pc.RemoveFollower(npc);
                    npc.Attack(pc);
                    ;
                    break;
                case 22000:
                    originalScript = "game.global_vars[913] = 32";
                    SetGlobalVar(913, 32);
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
                case 25:
                case 26:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 2);
                    return true;
                case 831:
                case 832:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 4);
                    return true;
                case 881:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 8);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
