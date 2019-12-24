
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
    [DialogScript(110)]
    public class DickDialog : Dick, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                case 12:
                    originalScript = "pc.skill_level_get(npc, skill_intimidate) >= 10";
                    return pc.GetSkillLevel(npc, SkillId.intimidate) >= 10;
                case 13:
                case 14:
                case 123:
                case 124:
                    originalScript = "pc.skill_level_get(npc, skill_diplomacy) >= 10";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 10;
                case 31:
                case 41:
                    originalScript = "game.global_flags[91] == 1";
                    return GetGlobalFlag(91);
                case 32:
                case 42:
                    originalScript = "game.global_flags[91] == 0";
                    return !GetGlobalFlag(91);
                case 72:
                case 73:
                case 105:
                case 106:
                case 206:
                case 207:
                case 237:
                case 238:
                    originalScript = "game.quests[44].state == qs_accepted and game.global_flags[119] == 0 and game.global_flags[120] == 0";
                    return GetQuestState(44) == QuestState.Accepted && !GetGlobalFlag(119) && !GetGlobalFlag(120);
                case 113:
                case 114:
                    originalScript = "pc.skill_level_get(npc, skill_gather_information) >= 6";
                    return pc.GetSkillLevel(npc, SkillId.gather_information) >= 6;
                case 133:
                case 134:
                    originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 10";
                    return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 10;
                case 201:
                case 202:
                    originalScript = "game.global_flags[91] == 0 and game.quests[43].state == qs_unknown";
                    return !GetGlobalFlag(91) && GetQuestState(43) == QuestState.Unknown;
                case 208:
                case 209:
                    originalScript = "game.global_flags[289] == 0";
                    return !GetGlobalFlag(289);
                case 211:
                case 213:
                    originalScript = "pc.money_get() >= 5";
                    return pc.GetMoney() >= 5;
                case 212:
                case 214:
                    originalScript = "pc.money_get() < 5";
                    return pc.GetMoney() < 5;
                case 231:
                case 232:
                    originalScript = "game.quests[39].state == qs_mentioned and pc.skill_level_get(npc, skill_bluff) >= 10";
                    return GetQuestState(39) == QuestState.Mentioned && pc.GetSkillLevel(npc, SkillId.bluff) >= 10;
                case 233:
                case 234:
                    originalScript = "game.quests[39].state == qs_completed and pc.skill_level_get(npc, skill_bluff) >= 10";
                    return GetQuestState(39) == QuestState.Completed && pc.GetSkillLevel(npc, SkillId.bluff) >= 10;
                case 271:
                case 273:
                    originalScript = "not anyone( pc.group_list(), \"has_item\", 5807 )";
                    return !pc.GetPartyMembers().Any(o => o.HasItemByName(5807));
                case 274:
                    originalScript = "anyone( pc.group_list(), \"has_item\", 5807 )";
                    return pc.GetPartyMembers().Any(o => o.HasItemByName(5807));
                case 311:
                case 313:
                    originalScript = "pc.money_get() >= 100";
                    return pc.GetMoney() >= 100;
                case 312:
                case 314:
                    originalScript = "pc.money_get() < 100";
                    return pc.GetMoney() < 100;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 15:
                case 16:
                    originalScript = "npc.attack(pc)";
                    npc.Attack(pc);
                    break;
                case 70:
                    originalScript = "game.areas[4] = 1; game.story_state = 4; game.global_flags[91] = 1";
                    MakeAreaKnown(4);
                    StoryState = 4;
                    SetGlobalFlag(91, true);
                    ;
                    break;
                case 211:
                case 213:
                    originalScript = "pc.money_adj(-5)";
                    pc.AdjustMoney(-5);
                    break;
                case 237:
                case 238:
                    originalScript = "game.global_flags[91] = 0";
                    SetGlobalFlag(91, false);
                    break;
                case 241:
                case 242:
                    originalScript = "game.quests[39].state = qs_accepted";
                    SetQuestState(39, QuestState.Accepted);
                    break;
                case 274:
                    originalScript = "party_transfer_to( npc, 5807 )";
                    Utilities.party_transfer_to(npc, 5807);
                    break;
                case 300:
                    originalScript = "game.global_flags[120] = 1";
                    SetGlobalFlag(120, true);
                    break;
                case 311:
                case 313:
                    originalScript = "pc.money_adj(-100)";
                    pc.AdjustMoney(-100);
                    break;
                case 330:
                    originalScript = "set_hostel_flag(npc,pc)";
                    set_hostel_flag(npc, pc);
                    break;
                case 340:
                    originalScript = "create_item_in_inventory( 8004, pc )";
                    Utilities.create_item_in_inventory(8004, pc);
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
                case 11:
                case 12:
                    skillChecks = new DialogSkillChecks(SkillId.intimidate, 10);
                    return true;
                case 13:
                case 14:
                case 123:
                case 124:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 10);
                    return true;
                case 113:
                case 114:
                    skillChecks = new DialogSkillChecks(SkillId.gather_information, 6);
                    return true;
                case 133:
                case 134:
                    skillChecks = new DialogSkillChecks(SkillId.sense_motive, 10);
                    return true;
                case 231:
                case 232:
                case 233:
                case 234:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 10);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
