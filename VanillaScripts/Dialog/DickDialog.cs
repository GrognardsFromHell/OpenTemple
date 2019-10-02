
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
    [DialogScript(110)]
    public class DickDialog : Dick, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 12:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_intimidate) >= 8");
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 8;
                case 13:
                case 14:
                case 123:
                case 124:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_diplomacy) >= 8");
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 8;
                case 31:
                case 41:
                    Trace.Assert(originalScript == "game.global_flags[91] == 1");
                    return GetGlobalFlag(91);
                case 32:
                case 42:
                    Trace.Assert(originalScript == "game.global_flags[91] == 0");
                    return !GetGlobalFlag(91);
                case 72:
                case 73:
                case 105:
                case 106:
                case 206:
                case 207:
                case 237:
                case 238:
                    Trace.Assert(originalScript == "game.quests[44].state == qs_accepted and game.global_flags[119] == 0 and game.global_flags[120] == 0");
                    return GetQuestState(44) == QuestState.Accepted && !GetGlobalFlag(119) && !GetGlobalFlag(120);
                case 113:
                case 114:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_gather_information) >= 4");
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 4;
                case 133:
                case 134:
                    Trace.Assert(originalScript == "pc.skill_level_get(npc, skill_sense_motive) >= 8");
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 8;
                case 201:
                case 202:
                    Trace.Assert(originalScript == "game.global_flags[91] == 0 and game.quests[43].state == qs_unknown");
                    return !GetGlobalFlag(91) && GetQuestState(43) == QuestState.Unknown;
                case 208:
                case 209:
                    Trace.Assert(originalScript == "game.global_flags[289] == 0");
                    return !GetGlobalFlag(289);
                case 211:
                case 213:
                    Trace.Assert(originalScript == "pc.money_get() >= 5");
                    return pc.GetMoney() >= 5;
                case 212:
                case 214:
                    Trace.Assert(originalScript == "pc.money_get() < 5");
                    return pc.GetMoney() < 5;
                case 231:
                case 232:
                    Trace.Assert(originalScript == "game.quests[39].state == qs_mentioned and pc.skill_level_get(npc, skill_bluff) >= 8");
                    return GetQuestState(39) == QuestState.Mentioned && pc.GetSkillLevel(npc, SkillId.bluff) >= 8;
                case 233:
                case 234:
                    Trace.Assert(originalScript == "game.quests[39].state == qs_completed and pc.skill_level_get(npc, skill_bluff) >= 8");
                    return GetQuestState(39) == QuestState.Completed && pc.GetSkillLevel(npc, SkillId.bluff) >= 8;
                case 271:
                case 273:
                    Trace.Assert(originalScript == "not anyone( pc.group_list(), \"has_item\", 5807 )");
                    return !pc.GetPartyMembers().Any(o => o.HasItemByName(5807));
                case 274:
                    Trace.Assert(originalScript == "anyone( pc.group_list(), \"has_item\", 5807 )");
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(5807));
                case 311:
                case 313:
                    Trace.Assert(originalScript == "pc.money_get() >= 100");
                    return pc.GetMoney() >= 100;
                case 312:
                case 314:
                    Trace.Assert(originalScript == "pc.money_get() < 100");
                    return pc.GetMoney() < 100;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 15:
                case 16:
                    Trace.Assert(originalScript == "npc.attack(pc)");
                    npc.Attack(pc);
                    break;
                case 70:
                    Trace.Assert(originalScript == "game.areas[4] = 1; game.story_state = 4; game.global_flags[91] = 1");
                    MakeAreaKnown(4);
                    StoryState = 4;
                    SetGlobalFlag(91, true);
                    ;
                    break;
                case 211:
                case 213:
                    Trace.Assert(originalScript == "pc.money_adj(-5)");
                    pc.AdjustMoney(-5);
                    break;
                case 237:
                case 238:
                    Trace.Assert(originalScript == "game.global_flags[91] = 0");
                    SetGlobalFlag(91, false);
                    break;
                case 241:
                case 242:
                    Trace.Assert(originalScript == "game.quests[39].state = qs_accepted");
                    SetQuestState(39, QuestState.Accepted);
                    break;
                case 274:
                    Trace.Assert(originalScript == "party_transfer_to( npc, 5807 )");
                    Utilities.party_transfer_to(npc, 5807);
                    break;
                case 300:
                    Trace.Assert(originalScript == "game.global_flags[120] = 1");
                    SetGlobalFlag(120, true);
                    break;
                case 311:
                case 313:
                    Trace.Assert(originalScript == "pc.money_adj(-100)");
                    pc.AdjustMoney(-100);
                    break;
                case 330:
                    Trace.Assert(originalScript == "set_hostel_flag(npc,pc)");
                    set_hostel_flag(npc, pc);
                    break;
                case 340:
                    Trace.Assert(originalScript == "create_item_in_inventory( 8004, pc )");
                    Utilities.create_item_in_inventory(8004, pc);
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
                case 11:
                case 12:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 8);
                    return true;
                case 13:
                case 14:
                case 123:
                case 124:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 8);
                    return true;
                case 113:
                case 114:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 4);
                    return true;
                case 133:
                case 134:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 8);
                    return true;
                case 231:
                case 232:
                case 233:
                case 234:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 8);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
