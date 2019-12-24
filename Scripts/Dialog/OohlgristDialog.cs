
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
    [DialogScript(126)]
    public class OohlgristDialog : Oohlgrist, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 13:
                    originalScript = "pc.stat_level_get(stat_level_paladin) == 0";
                    return pc.GetStat(Stat.level_paladin) == 0;
                case 12:
                case 14:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 5 or pc.stat_level_get(stat_level_paladin) > 0";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 5 || pc.GetStat(Stat.level_paladin) > 0;
                case 21:
                case 25:
                    originalScript = "pc.money_get() >= 100000";
                    return pc.GetMoney() >= 100000;
                case 23:
                case 27:
                    originalScript = "pc.money_get() < 100000";
                    return pc.GetMoney() < 100000;
                case 24:
                case 28:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 5";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 5;
                case 31:
                case 63:
                case 141:
                case 161:
                    originalScript = "( (game.quests[46].state == qs_accepted or game.quests[52].state == qs_accepted) and game.global_flags[112] == 0 and game.global_flags[118] == 0 ) or (game.quests[46].state == qs_accepted and game.global_flags[112] == 0 and game.global_flags[107] == 1 and game.global_flags[105] == 0) or (game.quests[52].state == qs_accepted and game.global_flags[118] == 0 and game.global_flags[107] == 0 and game.global_flags[105] == 1)";
                    return ((GetQuestState(46) == QuestState.Accepted || GetQuestState(52) == QuestState.Accepted) && !GetGlobalFlag(112) && !GetGlobalFlag(118)) || (GetQuestState(46) == QuestState.Accepted && !GetGlobalFlag(112) && GetGlobalFlag(107) && !GetGlobalFlag(105)) || (GetQuestState(52) == QuestState.Accepted && !GetGlobalFlag(118) && !GetGlobalFlag(107) && GetGlobalFlag(105));
                case 41:
                case 42:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 5 and pc.stat_level_get(stat_level_paladin) == 0";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 5 && pc.GetStat(Stat.level_paladin) == 0;
                case 61:
                case 62:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 8 and pc.stat_level_get(stat_level_paladin) == 0";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 8 && pc.GetStat(Stat.level_paladin) == 0;
                case 64:
                case 66:
                    originalScript = "pc.stat_level_get(stat_level_paladin) > 0";
                    return pc.GetStat(Stat.level_paladin) > 0;
                case 71:
                case 72:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 11";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 11;
                case 81:
                case 82:
                case 274:
                case 283:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 14";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 14;
                case 91:
                case 92:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 17";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 17;
                case 111:
                case 112:
                    originalScript = "not pc.follower_atmax()";
                    return !pc.HasMaxFollowers();
                case 113:
                case 114:
                    originalScript = "pc.follower_atmax()";
                    return pc.HasMaxFollowers();
                case 121:
                    originalScript = "( ( (game.quests[46].state == qs_accepted or game.quests[52].state == qs_accepted) and game.global_flags[112] == 0 and game.global_flags[118] == 0 ) or (game.quests[46].state == qs_accepted and game.global_flags[112] == 0 and game.global_flags[107] == 1 and game.global_flags[105] == 0) or (game.quests[52].state == qs_accepted and game.global_flags[118] == 0 and game.global_flags[107] == 0 and game.global_flags[105] == 1) ) and game.global_flags[349] == 0";
                    return (((GetQuestState(46) == QuestState.Accepted || GetQuestState(52) == QuestState.Accepted) && !GetGlobalFlag(112) && !GetGlobalFlag(118)) || (GetQuestState(46) == QuestState.Accepted && !GetGlobalFlag(112) && GetGlobalFlag(107) && !GetGlobalFlag(105)) || (GetQuestState(52) == QuestState.Accepted && !GetGlobalFlag(118) && !GetGlobalFlag(107) && GetGlobalFlag(105))) && !GetGlobalFlag(349);
                case 122:
                case 142:
                    originalScript = "game.quests[52].state == qs_accepted and (game.global_flags[112] == 0 or game.global_flags[105] == 1) and game.global_flags[107] == 0 and game.global_flags[118] == 0 and game.global_flags[349] == 1";
                    return GetQuestState(52) == QuestState.Accepted && (!GetGlobalFlag(112) || GetGlobalFlag(105)) && !GetGlobalFlag(107) && !GetGlobalFlag(118) && GetGlobalFlag(349);
                case 123:
                case 143:
                    originalScript = "game.quests[46].state == qs_accepted and game.global_flags[112] == 0 and (game.global_flags[118] == 0 or game.global_flags[107] == 1) and game.global_flags[105] == 0 and game.global_flags[349] == 1";
                    return GetQuestState(46) == QuestState.Accepted && !GetGlobalFlag(112) && (!GetGlobalFlag(118) || GetGlobalFlag(107)) && !GetGlobalFlag(105) && GetGlobalFlag(349);
                case 151:
                    originalScript = "(    ((game.quests[46].state == qs_accepted or game.quests[52].state == qs_accepted) and game.global_flags[112] == 0 and game.global_flags[118] == 0)    or    (game.quests[46].state == qs_accepted and game.global_flags[112] == 0 and game.global_flags[107] == 1 and game.global_flags[105] == 0)    or    (game.quests[52].state == qs_accepted and game.global_flags[118] == 0 and game.global_flags[107] == 0 and game.global_flags[105] == 1)    )";
                    return (((GetQuestState(46) == QuestState.Accepted || GetQuestState(52) == QuestState.Accepted) && !GetGlobalFlag(112) && !GetGlobalFlag(118)) || (GetQuestState(46) == QuestState.Accepted && !GetGlobalFlag(112) && GetGlobalFlag(107) && !GetGlobalFlag(105)) || (GetQuestState(52) == QuestState.Accepted && !GetGlobalFlag(118) && !GetGlobalFlag(107) && GetGlobalFlag(105)));
                case 152:
                case 153:
                    originalScript = "(game.global_flags[122] == 1 or game.global_flags[123] == 1) and (game.global_flags[112] == 1 or game.global_flags[118] == 1) and pc.stat_level_get(stat_level_paladin) == 0";
                    return (GetGlobalFlag(122) || GetGlobalFlag(123)) && (GetGlobalFlag(112) || GetGlobalFlag(118)) && pc.GetStat(Stat.level_paladin) == 0;
                case 154:
                case 155:
                    originalScript = "game.global_flags[122] == 1 and game.global_flags[112] == 0 and game.global_flags[118] == 0 and pc.stat_level_get(stat_level_paladin) == 0";
                    return GetGlobalFlag(122) && !GetGlobalFlag(112) && !GetGlobalFlag(118) && pc.GetStat(Stat.level_paladin) == 0;
                case 156:
                case 157:
                    originalScript = "game.global_flags[123] == 1 and game.global_flags[112] == 0 and game.global_flags[118] == 0 and pc.stat_level_get(stat_level_paladin) == 0";
                    return GetGlobalFlag(123) && !GetGlobalFlag(112) && !GetGlobalFlag(118) && pc.GetStat(Stat.level_paladin) == 0;
                case 171:
                    originalScript = "game.quests[52].state == qs_accepted and game.global_flags[107] == 0";
                    return GetQuestState(52) == QuestState.Accepted && !GetGlobalFlag(107);
                case 172:
                    originalScript = "game.quests[46].state == qs_accepted and game.global_flags[105] == 0";
                    return GetQuestState(46) == QuestState.Accepted && !GetGlobalFlag(105);
                case 173:
                    originalScript = "game.quests[46].state == qs_accepted and game.quests[52].state == qs_accepted and (game.global_flags[105] == 0 or game.global_flags[107] == 0)";
                    return GetQuestState(46) == QuestState.Accepted && GetQuestState(52) == QuestState.Accepted && (!GetGlobalFlag(105) || !GetGlobalFlag(107));
                case 174:
                    originalScript = "game.quests[46].state == qs_accepted and game.quests[52].state == qs_accepted and game.global_flags[105] == 1 and game.global_flags[107] == 1";
                    return GetQuestState(46) == QuestState.Accepted && GetQuestState(52) == QuestState.Accepted && GetGlobalFlag(105) && GetGlobalFlag(107);
                case 175:
                    originalScript = "game.quests[52].state == qs_accepted and game.global_flags[107] == 1";
                    return GetQuestState(52) == QuestState.Accepted && GetGlobalFlag(107);
                case 176:
                    originalScript = "game.quests[46].state == qs_accepted and game.global_flags[105] == 1";
                    return GetQuestState(46) == QuestState.Accepted && GetGlobalFlag(105);
                case 191:
                    originalScript = "game.global_flags[118] == 1";
                    return GetGlobalFlag(118);
                case 192:
                    originalScript = "game.global_flags[112] == 1";
                    return GetGlobalFlag(112);
                case 241:
                    originalScript = "pc.skill_level_get(npc, skill_bluff) >= 7";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 7;
                case 242:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 7";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 7;
                case 243:
                case 282:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 7";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 7;
                case 261:
                    originalScript = "game.global_flags[107] == 0";
                    return !GetGlobalFlag(107);
                case 262:
                    originalScript = "game.global_flags[105] == 0";
                    return !GetGlobalFlag(105);
                case 271:
                    originalScript = "game.global_flags[105] == 1";
                    return GetGlobalFlag(105);
                case 281:
                case 411:
                    originalScript = "game.global_flags[107] == 1";
                    return GetGlobalFlag(107);
                case 361:
                    originalScript = "pc.money_get() >= 1000000";
                    return pc.GetMoney() >= 1000000;
                case 362:
                    originalScript = "pc.money_get() < 1000000";
                    return pc.GetMoney() < 1000000;
                case 423:
                    originalScript = "(game.quests[46].state == qs_accepted and game.global_flags[105] == 0) or (game.quests[52].state == qs_accepted and game.global_flags[107] == 0)";
                    return (GetQuestState(46) == QuestState.Accepted && !GetGlobalFlag(105)) || (GetQuestState(52) == QuestState.Accepted && !GetGlobalFlag(107));
                case 441:
                    originalScript = "game.quests[52].state == qs_accepted";
                    return GetQuestState(52) == QuestState.Accepted;
                case 442:
                    originalScript = "game.quests[46].state == qs_accepted";
                    return GetQuestState(46) == QuestState.Accepted;
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
                    originalScript = "game.global_vars[135] = 1";
                    SetGlobalVar(135, 1);
                    break;
                case 21:
                case 25:
                    originalScript = "pc.money_adj(-100000); game.global_flags[121] = 1";
                    pc.AdjustMoney(-100000);
                    SetGlobalFlag(121, true);
                    ;
                    break;
                case 43:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 70:
                    originalScript = "game.global_flags[350] = 1";
                    SetGlobalFlag(350, true);
                    break;
                case 73:
                case 74:
                    originalScript = "npc.item_transfer_to(pc,3011)";
                    npc.TransferItemByNameTo(pc, 3011);
                    break;
                case 83:
                case 84:
                case 95:
                case 96:
                case 103:
                case 104:
                    originalScript = "npc.item_transfer_to(pc,3011); npc.item_transfer_to(pc,5806)";
                    npc.TransferItemByNameTo(pc, 3011);
                    npc.TransferItemByNameTo(pc, 5806);
                    ;
                    break;
                case 93:
                case 94:
                    originalScript = "game.global_flags[122] = 1";
                    SetGlobalFlag(122, true);
                    break;
                case 101:
                case 102:
                    originalScript = "game.global_flags[123] = 1";
                    SetGlobalFlag(123, true);
                    break;
                case 130:
                    originalScript = "pc.follower_add(npc)";
                    pc.AddFollower(npc);
                    break;
                case 310:
                case 350:
                case 370:
                    originalScript = "join_temple('fire')";
                    join_temple("fire");
                    break;
                case 332:
                case 342:
                case 381:
                    originalScript = "game.global_flags[348] = 1";
                    SetGlobalFlag(348, true);
                    break;
                case 361:
                    originalScript = "pc.money_adj(-1000000)";
                    pc.AdjustMoney(-1000000);
                    break;
                case 390:
                case 400:
                    originalScript = "join_temple('water')";
                    join_temple("water");
                    break;
                case 430:
                    originalScript = "pc.follower_remove(npc)";
                    pc.RemoveFollower(npc);
                    break;
                case 471:
                case 472:
                    originalScript = "TalkAern(npc,pc,200)";
                    TalkAern(npc, pc, 200);
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
                case 12:
                case 14:
                case 24:
                case 28:
                case 41:
                case 42:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 5);
                    return true;
                case 61:
                case 62:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 8);
                    return true;
                case 71:
                case 72:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 11);
                    return true;
                case 81:
                case 82:
                case 274:
                case 283:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 14);
                    return true;
                case 91:
                case 92:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 17);
                    return true;
                case 241:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 7);
                    return true;
                case 242:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 7);
                    return true;
                case 243:
                case 282:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 7);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
