
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
    [DialogScript(126)]
    public class OohlgristDialog : Oohlgrist, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 13:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) == 0");
                    return pc.GetStat(Stat.level_paladin) == 0;
                case 12:
                case 14:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 5 or pc.stat_level_get(stat_level_paladin) > 0");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 5 || pc.GetStat(Stat.level_paladin) > 0;
                case 21:
                case 25:
                    Trace.Assert(originalScript == "pc.money_get() >= 100000");
                    return pc.GetMoney() >= 100000;
                case 23:
                case 27:
                    Trace.Assert(originalScript == "pc.money_get() < 100000");
                    return pc.GetMoney() < 100000;
                case 24:
                case 28:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 5");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 5;
                case 31:
                case 63:
                case 141:
                case 161:
                    Trace.Assert(originalScript == "( (game.quests[46].state == qs_accepted or game.quests[52].state == qs_accepted) and game.global_flags[112] == 0 and game.global_flags[118] == 0 ) or (game.quests[46].state == qs_accepted and game.global_flags[112] == 0 and game.global_flags[107] == 1 and game.global_flags[105] == 0) or (game.quests[52].state == qs_accepted and game.global_flags[118] == 0 and game.global_flags[107] == 0 and game.global_flags[105] == 1)");
                    return ((GetQuestState(46) == QuestState.Accepted || GetQuestState(52) == QuestState.Accepted) && !GetGlobalFlag(112) && !GetGlobalFlag(118)) || (GetQuestState(46) == QuestState.Accepted && !GetGlobalFlag(112) && GetGlobalFlag(107) && !GetGlobalFlag(105)) || (GetQuestState(52) == QuestState.Accepted && !GetGlobalFlag(118) && !GetGlobalFlag(107) && GetGlobalFlag(105));
                case 41:
                case 42:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 5 and pc.stat_level_get(stat_level_paladin) == 0");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 5 && pc.GetStat(Stat.level_paladin) == 0;
                case 61:
                case 62:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 8 and pc.stat_level_get(stat_level_paladin) == 0");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 8 && pc.GetStat(Stat.level_paladin) == 0;
                case 64:
                case 66:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) > 0");
                    return pc.GetStat(Stat.level_paladin) > 0;
                case 71:
                case 72:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 11");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 11;
                case 81:
                case 82:
                case 274:
                case 283:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 14");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 14;
                case 91:
                case 92:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 17");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 17;
                case 111:
                case 112:
                    Trace.Assert(originalScript == "not pc.follower_atmax()");
                    return !pc.HasMaxFollowers();
                case 113:
                case 114:
                    Trace.Assert(originalScript == "pc.follower_atmax()");
                    return pc.HasMaxFollowers();
                case 121:
                    Trace.Assert(originalScript == "( ( (game.quests[46].state == qs_accepted or game.quests[52].state == qs_accepted) and game.global_flags[112] == 0 and game.global_flags[118] == 0 ) or (game.quests[46].state == qs_accepted and game.global_flags[112] == 0 and game.global_flags[107] == 1 and game.global_flags[105] == 0) or (game.quests[52].state == qs_accepted and game.global_flags[118] == 0 and game.global_flags[107] == 0 and game.global_flags[105] == 1) ) and game.global_flags[349] == 0");
                    return (((GetQuestState(46) == QuestState.Accepted || GetQuestState(52) == QuestState.Accepted) && !GetGlobalFlag(112) && !GetGlobalFlag(118)) || (GetQuestState(46) == QuestState.Accepted && !GetGlobalFlag(112) && GetGlobalFlag(107) && !GetGlobalFlag(105)) || (GetQuestState(52) == QuestState.Accepted && !GetGlobalFlag(118) && !GetGlobalFlag(107) && GetGlobalFlag(105))) && !GetGlobalFlag(349);
                case 122:
                case 142:
                    Trace.Assert(originalScript == "game.quests[52].state == qs_accepted and (game.global_flags[112] == 0 or game.global_flags[105] == 1) and game.global_flags[107] == 0 and game.global_flags[118] == 0 and game.global_flags[349] == 1");
                    return GetQuestState(52) == QuestState.Accepted && (!GetGlobalFlag(112) || GetGlobalFlag(105)) && !GetGlobalFlag(107) && !GetGlobalFlag(118) && GetGlobalFlag(349);
                case 123:
                case 143:
                    Trace.Assert(originalScript == "game.quests[46].state == qs_accepted and game.global_flags[112] == 0 and (game.global_flags[118] == 0 or game.global_flags[107] == 1) and game.global_flags[105] == 0 and game.global_flags[349] == 1");
                    return GetQuestState(46) == QuestState.Accepted && !GetGlobalFlag(112) && (!GetGlobalFlag(118) || GetGlobalFlag(107)) && !GetGlobalFlag(105) && GetGlobalFlag(349);
                case 151:
                    Trace.Assert(originalScript == "(    ((game.quests[46].state == qs_accepted or game.quests[52].state == qs_accepted) and game.global_flags[112] == 0 and game.global_flags[118] == 0)    or    (game.quests[46].state == qs_accepted and game.global_flags[112] == 0 and game.global_flags[107] == 1 and game.global_flags[105] == 0)    or    (game.quests[52].state == qs_accepted and game.global_flags[118] == 0 and game.global_flags[107] == 0 and game.global_flags[105] == 1)    )");
                    return (((GetQuestState(46) == QuestState.Accepted || GetQuestState(52) == QuestState.Accepted) && !GetGlobalFlag(112) && !GetGlobalFlag(118)) || (GetQuestState(46) == QuestState.Accepted && !GetGlobalFlag(112) && GetGlobalFlag(107) && !GetGlobalFlag(105)) || (GetQuestState(52) == QuestState.Accepted && !GetGlobalFlag(118) && !GetGlobalFlag(107) && GetGlobalFlag(105)));
                case 152:
                case 153:
                    Trace.Assert(originalScript == "(game.global_flags[122] == 1 or game.global_flags[123] == 1) and (game.global_flags[112] == 1 or game.global_flags[118] == 1) and pc.stat_level_get(stat_level_paladin) == 0");
                    return (GetGlobalFlag(122) || GetGlobalFlag(123)) && (GetGlobalFlag(112) || GetGlobalFlag(118)) && pc.GetStat(Stat.level_paladin) == 0;
                case 154:
                case 155:
                    Trace.Assert(originalScript == "game.global_flags[122] == 1 and game.global_flags[112] == 0 and game.global_flags[118] == 0 and pc.stat_level_get(stat_level_paladin) == 0");
                    return GetGlobalFlag(122) && !GetGlobalFlag(112) && !GetGlobalFlag(118) && pc.GetStat(Stat.level_paladin) == 0;
                case 156:
                case 157:
                    Trace.Assert(originalScript == "game.global_flags[123] == 1 and game.global_flags[112] == 0 and game.global_flags[118] == 0 and pc.stat_level_get(stat_level_paladin) == 0");
                    return GetGlobalFlag(123) && !GetGlobalFlag(112) && !GetGlobalFlag(118) && pc.GetStat(Stat.level_paladin) == 0;
                case 171:
                    Trace.Assert(originalScript == "game.quests[52].state == qs_accepted and game.global_flags[107] == 0");
                    return GetQuestState(52) == QuestState.Accepted && !GetGlobalFlag(107);
                case 172:
                    Trace.Assert(originalScript == "game.quests[46].state == qs_accepted and game.global_flags[105] == 0");
                    return GetQuestState(46) == QuestState.Accepted && !GetGlobalFlag(105);
                case 173:
                    Trace.Assert(originalScript == "game.quests[46].state == qs_accepted and game.quests[52].state == qs_accepted and (game.global_flags[105] == 0 or game.global_flags[107] == 0)");
                    return GetQuestState(46) == QuestState.Accepted && GetQuestState(52) == QuestState.Accepted && (!GetGlobalFlag(105) || !GetGlobalFlag(107));
                case 174:
                    Trace.Assert(originalScript == "game.quests[46].state == qs_accepted and game.quests[52].state == qs_accepted and game.global_flags[105] == 1 and game.global_flags[107] == 1");
                    return GetQuestState(46) == QuestState.Accepted && GetQuestState(52) == QuestState.Accepted && GetGlobalFlag(105) && GetGlobalFlag(107);
                case 175:
                    Trace.Assert(originalScript == "game.quests[52].state == qs_accepted and game.global_flags[107] == 1");
                    return GetQuestState(52) == QuestState.Accepted && GetGlobalFlag(107);
                case 176:
                    Trace.Assert(originalScript == "game.quests[46].state == qs_accepted and game.global_flags[105] == 1");
                    return GetQuestState(46) == QuestState.Accepted && GetGlobalFlag(105);
                case 191:
                    Trace.Assert(originalScript == "game.global_flags[118] == 1");
                    return GetGlobalFlag(118);
                case 192:
                    Trace.Assert(originalScript == "game.global_flags[112] == 1");
                    return GetGlobalFlag(112);
                case 241:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_bluff) >= 7");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 7;
                case 242:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 7");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 7;
                case 243:
                case 282:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_diplomacy) >= 7");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 7;
                case 261:
                    Trace.Assert(originalScript == "game.global_flags[107] == 0");
                    return !GetGlobalFlag(107);
                case 262:
                    Trace.Assert(originalScript == "game.global_flags[105] == 0");
                    return !GetGlobalFlag(105);
                case 271:
                    Trace.Assert(originalScript == "game.global_flags[105] == 1");
                    return GetGlobalFlag(105);
                case 281:
                case 411:
                    Trace.Assert(originalScript == "game.global_flags[107] == 1");
                    return GetGlobalFlag(107);
                case 361:
                    Trace.Assert(originalScript == "pc.money_get() >= 1000000");
                    return pc.GetMoney() >= 1000000;
                case 362:
                    Trace.Assert(originalScript == "pc.money_get() < 1000000");
                    return pc.GetMoney() < 1000000;
                case 423:
                    Trace.Assert(originalScript == "(game.quests[46].state == qs_accepted and game.global_flags[105] == 0) or (game.quests[52].state == qs_accepted and game.global_flags[107] == 0)");
                    return (GetQuestState(46) == QuestState.Accepted && !GetGlobalFlag(105)) || (GetQuestState(52) == QuestState.Accepted && !GetGlobalFlag(107));
                case 441:
                    Trace.Assert(originalScript == "game.quests[52].state == qs_accepted");
                    return GetQuestState(52) == QuestState.Accepted;
                case 442:
                    Trace.Assert(originalScript == "game.quests[46].state == qs_accepted");
                    return GetQuestState(46) == QuestState.Accepted;
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
                    Trace.Assert(originalScript == "game.global_vars[135] = 1");
                    SetGlobalVar(135, 1);
                    break;
                case 21:
                case 25:
                    Trace.Assert(originalScript == "pc.money_adj(-100000); game.global_flags[121] = 1");
                    pc.AdjustMoney(-100000);
                    SetGlobalFlag(121, true);
                    ;
                    break;
                case 43:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 70:
                    Trace.Assert(originalScript == "game.global_flags[350] = 1");
                    SetGlobalFlag(350, true);
                    break;
                case 73:
                case 74:
                    Trace.Assert(originalScript == "npc.item_transfer_to(pc,3011)");
                    npc.TransferItemByNameTo(pc, 3011);
                    break;
                case 83:
                case 84:
                case 95:
                case 96:
                case 103:
                case 104:
                    Trace.Assert(originalScript == "npc.item_transfer_to(pc,3011); npc.item_transfer_to(pc,5806)");
                    npc.TransferItemByNameTo(pc, 3011);
                    npc.TransferItemByNameTo(pc, 5806);
                    ;
                    break;
                case 93:
                case 94:
                    Trace.Assert(originalScript == "game.global_flags[122] = 1");
                    SetGlobalFlag(122, true);
                    break;
                case 101:
                case 102:
                    Trace.Assert(originalScript == "game.global_flags[123] = 1");
                    SetGlobalFlag(123, true);
                    break;
                case 130:
                    Trace.Assert(originalScript == "pc.follower_add(npc)");
                    pc.AddFollower(npc);
                    break;
                case 310:
                case 350:
                case 370:
                    Trace.Assert(originalScript == "join_temple('fire')");
                    join_temple("fire");
                    break;
                case 332:
                case 342:
                case 381:
                    Trace.Assert(originalScript == "game.global_flags[348] = 1");
                    SetGlobalFlag(348, true);
                    break;
                case 361:
                    Trace.Assert(originalScript == "pc.money_adj(-1000000)");
                    pc.AdjustMoney(-1000000);
                    break;
                case 390:
                case 400:
                    Trace.Assert(originalScript == "join_temple('water')");
                    join_temple("water");
                    break;
                case 430:
                    Trace.Assert(originalScript == "pc.follower_remove(npc)");
                    pc.RemoveFollower(npc);
                    break;
                case 471:
                case 472:
                    Trace.Assert(originalScript == "TalkAern(npc,pc,200)");
                    TalkAern(npc, pc, 200);
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
