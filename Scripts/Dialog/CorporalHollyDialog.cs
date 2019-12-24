
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
    [DialogScript(356)]
    public class CorporalHollyDialog : CorporalHolly, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 61:
                    originalScript = "pc.stat_level_get(stat_charisma) >= 12";
                    return pc.GetStat(Stat.charisma) >= 12;
                case 62:
                    originalScript = "pc.stat_level_get(stat_charisma) <= 11";
                    return pc.GetStat(Stat.charisma) <= 11;
                case 111:
                case 121:
                    originalScript = "pc.skill_level_get(npc,skill_gather_information) >= 11";
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 11;
                case 131:
                case 141:
                case 151:
                    originalScript = "pc.skill_level_get(npc,skill_gather_information) >= 12";
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 12;
                case 171:
                    originalScript = "not get_1(npc)";
                    return !Scripts.get_1(npc);
                case 172:
                    originalScript = "pc.skill_level_get(npc,skill_gather_information) >= 10 and not get_2(npc)";
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 10 && !Scripts.get_2(npc);
                case 173:
                    originalScript = "pc.skill_level_get(npc,skill_gather_information) <= 9 and not get_2(npc)";
                    return pc.GetSkillLevel(npc, SkillId.gather_information) <= 9 && !Scripts.get_2(npc);
                case 174:
                    originalScript = "not get_3(npc)";
                    return !Scripts.get_3(npc);
                case 176:
                    originalScript = "game.global_vars[968] >= 3 and game.global_vars[968] <= 7 and (anyone( pc.group_list(), \"item_find\", 8004 ))";
                    return GetGlobalVar(968) >= 3 && GetGlobalVar(968) <= 7 && (pc.GetPartyMembers().Any(o => o.FindItemByName(8004) != null));
                case 177:
                    originalScript = "game.global_vars[968] == 8 and (anyone( pc.group_list(), \"item_find\", 8004 ))";
                    return GetGlobalVar(968) == 8 && (pc.GetPartyMembers().Any(o => o.FindItemByName(8004) != null));
                case 178:
                    originalScript = "game.global_vars[968] == 10";
                    return GetGlobalVar(968) == 10;
                case 181:
                    originalScript = "pc.follower_atmax() == 0";
                    return !pc.HasMaxFollowers();
                case 182:
                    originalScript = "pc.follower_atmax() == 1";
                    return pc.HasMaxFollowers();
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
                    originalScript = "game.global_vars[145] = 1";
                    SetGlobalVar(145, 1);
                    break;
                case 30:
                    originalScript = "game.global_flags[885] = 1";
                    SetGlobalFlag(885, true);
                    break;
                case 31:
                case 191:
                case 200:
                    originalScript = "pc.follower_remove( npc )";
                    pc.RemoveFollower(npc);
                    break;
                case 70:
                    originalScript = "game.global_vars[968] = 1";
                    SetGlobalVar(968, 1);
                    break;
                case 171:
                    originalScript = "npc_1(npc)";
                    Scripts.npc_1(npc);
                    break;
                case 172:
                case 173:
                    originalScript = "npc_2(npc)";
                    Scripts.npc_2(npc);
                    break;
                case 174:
                    originalScript = "npc_3(npc)";
                    Scripts.npc_3(npc);
                    break;
                case 176:
                case 177:
                    originalScript = "get_holly_drunk( npc, pc )";
                    get_holly_drunk(npc, pc);
                    break;
                case 178:
                    originalScript = "game.global_vars[968] = 11";
                    SetGlobalVar(968, 11);
                    break;
                case 181:
                    originalScript = "pc.follower_add( npc ); game.global_vars[976] = 3";
                    pc.AddFollower(npc);
                    SetGlobalVar(976, 3);
                    ;
                    break;
                case 201:
                    originalScript = "run_off( npc,pc )";
                    run_off(npc, pc);
                    break;
                case 222:
                case 232:
                case 252:
                case 262:
                    originalScript = "game.global_vars[968] = 2";
                    SetGlobalVar(968, 2);
                    break;
                case 231:
                    originalScript = "game.global_vars[968] = 3";
                    SetGlobalVar(968, 3);
                    break;
                case 261:
                    originalScript = "game.global_vars[968] = 10; game.fade(28800,4047,0,4)";
                    SetGlobalVar(968, 10);
                    Fade(28800, 4047, 0, 4);
                    ;
                    break;
                case 300:
                    originalScript = "game.party[0].reputation_add(36)";
                    PartyLeader.AddReputation(36);
                    break;
                case 320:
                    originalScript = "game.global_vars[969] = 1";
                    SetGlobalVar(969, 1);
                    break;
                case 322:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 330:
                    originalScript = "game.global_vars[969] = 2";
                    SetGlobalVar(969, 2);
                    break;
                case 331:
                    originalScript = "game.fade_and_teleport(0,0,0,5172,471,489)";
                    FadeAndTeleport(0, 0, 0, 5172, 471, 489);
                    break;
                case 22000:
                    originalScript = "game.global_vars[914] = 32";
                    SetGlobalVar(914, 32);
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
                case 111:
                case 121:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 11);
                    return true;
                case 131:
                case 141:
                case 151:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 12);
                    return true;
                case 172:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
