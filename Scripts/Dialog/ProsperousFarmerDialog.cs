
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
    [DialogScript(16)]
    public class ProsperousFarmerDialog : ProsperousFarmer, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 2:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 4 and pc.item_find(5801) == OBJ_HANDLE_NULL";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 4 && pc.FindItemByName(5801) == null;
                case 3:
                case 4:
                case 123:
                case 124:
                    originalScript = "pc.item_find(5801) != OBJ_HANDLE_NULL and pc.skill_level_get(npc,skill_bluff) >= 2";
                    return pc.FindItemByName(5801) != null && pc.GetSkillLevel(npc, SkillId.bluff) >= 2;
                case 5:
                    originalScript = "game.global_flags[2] == 1";
                    return GetGlobalFlag(2);
                case 11:
                case 12:
                case 26:
                case 28:
                case 131:
                case 132:
                case 153:
                case 154:
                    originalScript = "game.quests[7].state == qs_accepted";
                    return GetQuestState(7) == QuestState.Accepted;
                case 13:
                case 14:
                case 42:
                case 82:
                case 84:
                case 141:
                case 142:
                case 151:
                case 152:
                    originalScript = "game.global_flags[9] == 1";
                    return GetGlobalFlag(9);
                case 15:
                case 16:
                case 92:
                case 94:
                    originalScript = "game.global_flags[8] == 1";
                    return GetGlobalFlag(8);
                case 17:
                    originalScript = "game.global_flags[1] == 1";
                    return GetGlobalFlag(1);
                case 18:
                case 19:
                    originalScript = "game.global_flags[9] == 0";
                    return !GetGlobalFlag(9);
                case 21:
                case 24:
                    originalScript = "game.story_state >= 2 and game.areas[3] == 0";
                    return StoryState >= 2 && !IsAreaKnown(3);
                case 22:
                case 27:
                    originalScript = "game.global_vars[4] >= 1 and game.quests[9].state == qs_accepted";
                    return GetGlobalVar(4) >= 1 && GetQuestState(9) == QuestState.Accepted;
                case 23:
                case 25:
                    originalScript = "game.quests[9].state <= qs_mentioned";
                    return GetQuestState(9) <= QuestState.Mentioned;
                case 51:
                case 54:
                case 125:
                case 126:
                    originalScript = "game.quests[5].state == qs_completed";
                    return GetQuestState(5) == QuestState.Completed;
                case 52:
                    originalScript = "game.quests[5].state == qs_mentioned or game.quests[5].state == qs_accepted";
                    return GetQuestState(5) == QuestState.Mentioned || GetQuestState(5) == QuestState.Accepted;
                case 121:
                case 122:
                    originalScript = "pc.skill_level_get(npc,skill_bluff) >= 5 and pc.item_find(5801) == OBJ_HANDLE_NULL";
                    return pc.GetSkillLevel(npc, SkillId.bluff) >= 5 && pc.FindItemByName(5801) == null;
                case 129:
                case 130:
                    originalScript = "game.quests[5].state <= qs_mentioned and game.global_flags[2] == 1";
                    return GetQuestState(5) <= QuestState.Mentioned && GetGlobalFlag(2);
                case 171:
                case 173:
                    originalScript = "pc.skill_level_get(npc,skill_diplomacy) >= 5";
                    return pc.GetSkillLevel(npc, SkillId.diplomacy) >= 5;
                case 175:
                case 176:
                    originalScript = "game.party_alignment == TRUE_NEUTRAL";
                    return PartyAlignment == Alignment.NEUTRAL;
                case 178:
                case 179:
                    originalScript = "game.quests[6].state == qs_completed";
                    return GetQuestState(6) == QuestState.Completed;
                case 224:
                case 227:
                    originalScript = "game.global_vars[4] == 1";
                    return GetGlobalVar(4) == 1;
                case 225:
                case 228:
                    originalScript = "game.global_vars[4] == 2";
                    return GetGlobalVar(4) == 2;
                case 226:
                case 229:
                    originalScript = "game.global_vars[4] == 4";
                    return GetGlobalVar(4) == 4;
                case 236:
                    originalScript = "game.global_flags[1] == 1 and (game.party_alignment == LAWFUL_GOOD or game.party_alignment == CHAOTIC_GOOD or game.party_alignment == LAWFUL_EVIL or game.party_alignment == CHAOTIC_EVIL)";
                    return GetGlobalFlag(1) && (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD || PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL);
                case 302:
                case 304:
                    originalScript = "pc.money_get() >= 5000";
                    return pc.GetMoney() >= 5000;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 13:
                case 14:
                case 42:
                case 82:
                case 84:
                case 141:
                case 142:
                case 190:
                    originalScript = "game.global_flags[10] = 1";
                    SetGlobalFlag(10, true);
                    break;
                case 191:
                case 192:
                case 194:
                    originalScript = "game.quests[7].state = qs_completed";
                    SetQuestState(7, QuestState.Completed);
                    break;
                case 195:
                    originalScript = "create_item_in_inventory(4222,pc)";
                    Utilities.create_item_in_inventory(4222, pc);
                    break;
                case 210:
                    originalScript = "game.areas[3] = 1; game.story_state = 3";
                    MakeAreaKnown(3);
                    StoryState = 3;
                    ;
                    break;
                case 211:
                case 212:
                    originalScript = "game.worldmap_travel_by_dialog(3)";
                    // FIXME: worldmap_travel_by_dialog;
                    break;
                case 224:
                case 225:
                case 226:
                case 227:
                case 228:
                case 229:
                    originalScript = "game.quests[9].state = qs_completed";
                    SetQuestState(9, QuestState.Completed);
                    break;
                case 233:
                    originalScript = "game.quests[9].state = qs_mentioned";
                    SetQuestState(9, QuestState.Mentioned);
                    break;
                case 234:
                case 235:
                case 241:
                case 247:
                    originalScript = "game.quests[9].state = qs_accepted";
                    SetQuestState(9, QuestState.Accepted);
                    break;
                case 260:
                    originalScript = "game.quests[6].state = qs_completed";
                    SetQuestState(6, QuestState.Completed);
                    break;
                case 280:
                case 305:
                    originalScript = "npc.reaction_adj( pc,-10 )";
                    npc.AdjustReaction(pc, -10);
                    break;
                case 291:
                case 292:
                    originalScript = "game.global_vars[4] = 3";
                    SetGlobalVar(4, 3);
                    break;
                case 302:
                case 304:
                    originalScript = "pc.money_adj(-5000)";
                    pc.AdjustMoney(-5000);
                    break;
                case 311:
                case 312:
                    originalScript = "pc.money_adj(+2000); npc.reaction_adj( pc,+10 )";
                    pc.AdjustMoney(+2000);
                    npc.AdjustReaction(pc, +10);
                    ;
                    break;
                case 313:
                case 314:
                    originalScript = "npc.reaction_adj( pc,+30 )";
                    npc.AdjustReaction(pc, +30);
                    break;
                case 331:
                case 341:
                case 351:
                    originalScript = "npc.attack( pc )";
                    npc.Attack(pc);
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
                case 2:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 4);
                    return true;
                case 3:
                case 4:
                case 123:
                case 124:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 2);
                    return true;
                case 121:
                case 122:
                    skillChecks = new DialogSkillChecks(SkillId.bluff, 5);
                    return true;
                case 171:
                case 173:
                    skillChecks = new DialogSkillChecks(SkillId.diplomacy, 5);
                    return true;
                default:
                    skillChecks = default;
                    return false;
            }
        }
    }
}
