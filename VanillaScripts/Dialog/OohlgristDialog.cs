
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

namespace VanillaScripts.Dialog
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
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 3 or pc.stat_level_get(stat_level_paladin) > 0");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 3 || pc.GetStat(Stat.level_paladin) > 0;
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
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 3");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 3;
                case 31:
                case 32:
                case 63:
                case 64:
                case 141:
                case 142:
                case 151:
                case 152:
                case 161:
                case 162:
                    Trace.Assert(originalScript == "(game.quests[46].state == qs_accepted or game.quests[52].state == qs_accepted) and game.global_flags[112] == 0 and game.global_flags[118] == 0");
                    return (GetQuestState(46) == QuestState.Accepted || GetQuestState(52) == QuestState.Accepted) && !GetGlobalFlag(112) && !GetGlobalFlag(118);
                case 41:
                case 42:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 3 and pc.stat_level_get(stat_level_paladin) == 0");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 3 && pc.GetStat(Stat.level_paladin) == 0;
                case 61:
                case 62:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 6 and pc.stat_level_get(stat_level_paladin) == 0");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 6 && pc.GetStat(Stat.level_paladin) == 0;
                case 65:
                case 66:
                    Trace.Assert(originalScript == "pc.stat_level_get(stat_level_paladin) > 0");
                    return pc.GetStat(Stat.level_paladin) > 0;
                case 71:
                case 72:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 9");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 9;
                case 81:
                case 82:
                case 277:
                case 278:
                case 283:
                case 287:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 12");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 12;
                case 91:
                case 92:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 15");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 15;
                case 111:
                case 112:
                    Trace.Assert(originalScript == "not pc.follower_atmax()");
                    return !pc.HasMaxFollowers();
                case 113:
                case 114:
                    Trace.Assert(originalScript == "pc.follower_atmax()");
                    return pc.HasMaxFollowers();
                case 121:
                case 122:
                    Trace.Assert(originalScript == "(game.quests[46].state == qs_accepted or game.quests[52].state == qs_accepted) and game.global_flags[112] == 0 and game.global_flags[118] == 0 and game.global_flags[349] == 0");
                    return (GetQuestState(46) == QuestState.Accepted || GetQuestState(52) == QuestState.Accepted) && !GetGlobalFlag(112) && !GetGlobalFlag(118) && !GetGlobalFlag(349);
                case 123:
                case 124:
                case 143:
                case 144:
                    Trace.Assert(originalScript == "game.quests[52].state == qs_accepted and game.global_flags[112] == 0 and game.global_flags[118] == 0 and game.global_flags[349] == 1");
                    return GetQuestState(52) == QuestState.Accepted && !GetGlobalFlag(112) && !GetGlobalFlag(118) && GetGlobalFlag(349);
                case 125:
                case 126:
                case 145:
                case 146:
                    Trace.Assert(originalScript == "game.quests[46].state == qs_accepted and game.global_flags[112] == 0 and game.global_flags[118] == 0 and game.global_flags[349] == 1");
                    return GetQuestState(46) == QuestState.Accepted && !GetGlobalFlag(112) && !GetGlobalFlag(118) && GetGlobalFlag(349);
                case 153:
                case 154:
                    Trace.Assert(originalScript == "(game.global_flags[122] == 1 or game.global_flags[123] == 1) and (game.global_flags[112] == 1 or game.global_flags[118] == 1) and pc.stat_level_get(stat_level_paladin) == 0");
                    return (GetGlobalFlag(122) || GetGlobalFlag(123)) && (GetGlobalFlag(112) || GetGlobalFlag(118)) && pc.GetStat(Stat.level_paladin) == 0;
                case 155:
                case 156:
                    Trace.Assert(originalScript == "game.global_flags[122] == 1 and game.global_flags[112] == 0 and game.global_flags[118] == 0 and pc.stat_level_get(stat_level_paladin) == 0");
                    return GetGlobalFlag(122) && !GetGlobalFlag(112) && !GetGlobalFlag(118) && pc.GetStat(Stat.level_paladin) == 0;
                case 157:
                case 158:
                    Trace.Assert(originalScript == "game.global_flags[123] == 1 and game.global_flags[112] == 0 and game.global_flags[118] == 0 and pc.stat_level_get(stat_level_paladin) == 0");
                    return GetGlobalFlag(123) && !GetGlobalFlag(112) && !GetGlobalFlag(118) && pc.GetStat(Stat.level_paladin) == 0;
                case 171:
                case 174:
                    Trace.Assert(originalScript == "game.quests[46].state != qs_accepted and game.quests[52].state == qs_accepted and game.global_flags[107] == 0");
                    return GetQuestState(46) != QuestState.Accepted && GetQuestState(52) == QuestState.Accepted && !GetGlobalFlag(107);
                case 172:
                case 175:
                    Trace.Assert(originalScript == "game.quests[46].state == qs_accepted and game.quests[52].state != qs_accepted and game.global_flags[105] == 0");
                    return GetQuestState(46) == QuestState.Accepted && GetQuestState(52) != QuestState.Accepted && !GetGlobalFlag(105);
                case 173:
                case 176:
                    Trace.Assert(originalScript == "game.quests[46].state == qs_accepted and game.quests[52].state == qs_accepted and (game.global_flags[105] == 0 or game.global_flags[107] == 0)");
                    return GetQuestState(46) == QuestState.Accepted && GetQuestState(52) == QuestState.Accepted && (!GetGlobalFlag(105) || !GetGlobalFlag(107));
                case 177:
                case 178:
                    Trace.Assert(originalScript == "game.quests[46].state == qs_accepted and game.quests[52].state == qs_accepted and game.global_flags[105] == 1 and game.global_flags[107] == 1");
                    return GetQuestState(46) == QuestState.Accepted && GetQuestState(52) == QuestState.Accepted && GetGlobalFlag(105) && GetGlobalFlag(107);
                case 179:
                case 181:
                    Trace.Assert(originalScript == "game.quests[46].state != qs_accepted and game.quests[52].state == qs_accepted and game.global_flags[107] == 1");
                    return GetQuestState(46) != QuestState.Accepted && GetQuestState(52) == QuestState.Accepted && GetGlobalFlag(107);
                case 180:
                case 182:
                    Trace.Assert(originalScript == "game.quests[46].state == qs_accepted and game.quests[52].state != qs_accepted and game.global_flags[105] == 1");
                    return GetQuestState(46) == QuestState.Accepted && GetQuestState(52) != QuestState.Accepted && GetGlobalFlag(105);
                case 191:
                case 193:
                    Trace.Assert(originalScript == "game.global_flags[118] == 1");
                    return GetGlobalFlag(118);
                case 192:
                case 194:
                    Trace.Assert(originalScript == "game.global_flags[112] == 1");
                    return GetGlobalFlag(112);
                case 241:
                case 245:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_bluff) >= 5");
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 5;
                case 242:
                case 246:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 5");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 5;
                case 243:
                case 247:
                case 282:
                case 286:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_diplomacy) >= 5");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 5;
                case 261:
                case 263:
                    Trace.Assert(originalScript == "game.global_flags[107] == 0");
                    return !GetGlobalFlag(107);
                case 262:
                case 264:
                    Trace.Assert(originalScript == "game.global_flags[105] == 0");
                    return !GetGlobalFlag(105);
                case 271:
                case 274:
                    Trace.Assert(originalScript == "game.global_flags[105] == 1");
                    return GetGlobalFlag(105);
                case 281:
                case 285:
                case 411:
                case 412:
                    Trace.Assert(originalScript == "game.global_flags[107] == 1");
                    return GetGlobalFlag(107);
                case 361:
                case 364:
                    Trace.Assert(originalScript == "pc.money_get() >= 1000000");
                    return pc.GetMoney() >= 1000000;
                case 362:
                case 365:
                    Trace.Assert(originalScript == "pc.money_get() < 1000000");
                    return pc.GetMoney() < 1000000;
                case 423:
                case 424:
                    Trace.Assert(originalScript == "game.quests[46].state == qs_accepted or game.quests[52].state == qs_accepted");
                    return GetQuestState(46) == QuestState.Accepted || GetQuestState(52) == QuestState.Accepted;
                case 441:
                case 443:
                    Trace.Assert(originalScript == "game.quests[52].state == qs_accepted");
                    return GetQuestState(52) == QuestState.Accepted;
                case 442:
                case 444:
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
                    Trace.Assert(originalScript == "game.global_flags[118] = 1");
                    SetGlobalFlag(118, true);
                    break;
                case 333:
                case 343:
                case 381:
                    Trace.Assert(originalScript == "game.global_flags[348] = 1");
                    SetGlobalFlag(348, true);
                    break;
                case 361:
                case 364:
                    Trace.Assert(originalScript == "pc.money_adj(-1000000)");
                    pc.AdjustMoney(-1000000);
                    break;
                case 390:
                case 400:
                    Trace.Assert(originalScript == "game.global_flags[112] = 1");
                    SetGlobalFlag(112, true);
                    break;
                case 430:
                    Trace.Assert(originalScript == "pc.follower_remove(npc)");
                    pc.RemoveFollower(npc);
                    break;
                case 471:
                    Trace.Assert(originalScript == "TalkAern(npc,pc,200)");
                    TalkAern(npc, pc, 200);
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
                case 12:
                case 14:
                case 24:
                case 28:
                case 41:
                case 42:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 3);
                    return true;
                case 61:
                case 62:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 6);
                    return true;
                case 71:
                case 72:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 9);
                    return true;
                case 81:
                case 82:
                case 277:
                case 278:
                case 283:
                case 287:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 12);
                    return true;
                case 91:
                case 92:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 15);
                    return true;
                case 241:
                case 245:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 5);
                    return true;
                case 242:
                case 246:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 5);
                    return true;
                case 243:
                case 247:
                case 282:
                case 286:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 5);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
